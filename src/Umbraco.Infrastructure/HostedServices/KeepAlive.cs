// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Core;
using Umbraco.Core.Configuration.Models;
using Umbraco.Core.Logging;
using Umbraco.Core.Sync;
using Umbraco.Web;

namespace Umbraco.Infrastructure.HostedServices
{
    /// <summary>
    /// Hosted service implementation for keep alive feature.
    /// </summary>
    public class KeepAlive : RecurringHostedServiceBase
    {
        private readonly IRequestAccessor _requestAccessor;
        private readonly IMainDom _mainDom;
        private readonly KeepAliveSettings _keepAliveSettings;
        private readonly ILogger<KeepAlive> _logger;
        private readonly IProfilingLogger _profilingLogger;
        private readonly IServerRegistrar _serverRegistrar;
        private readonly IHttpClientFactory _httpClientFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="KeepAlive"/> class.
        /// </summary>
        /// <param name="requestAccessor">Accessor for the current request.</param>
        /// <param name="mainDom">Representation of the main application domain.</param>
        /// <param name="keepAliveSettings">The configuration for keep alive settings.</param>
        /// <param name="logger">The typed logger.</param>
        /// <param name="profilingLogger">The profiling logger.</param>
        /// <param name="serverRegistrar">Provider of server registrations to the distributed cache.</param>
        /// <param name="httpClientFactory">Factory for <see cref="HttpClient" /> instances.</param>
        public KeepAlive(
            IRequestAccessor requestAccessor,
            IMainDom mainDom,
            IOptions<KeepAliveSettings> keepAliveSettings,
            ILogger<KeepAlive> logger,
            IProfilingLogger profilingLogger,
            IServerRegistrar serverRegistrar,
            IHttpClientFactory httpClientFactory)
            : base(TimeSpan.FromMinutes(5), DefaultDelay)
        {
            _requestAccessor = requestAccessor;
            _mainDom = mainDom;
            _keepAliveSettings = keepAliveSettings.Value;
            _logger = logger;
            _profilingLogger = profilingLogger;
            _serverRegistrar = serverRegistrar;
            _httpClientFactory = httpClientFactory;
        }

        internal override async Task PerformExecuteAsync(object state)
        {
            if (_keepAliveSettings.DisableKeepAliveTask)
            {
                return;
            }

            // Don't run on replicas nor unknown role servers
            switch (_serverRegistrar.GetCurrentServerRole())
            {
                case ServerRole.Replica:
                    _logger.LogDebug("Does not run on replica servers.");
                    return;
                case ServerRole.Unknown:
                    _logger.LogDebug("Does not run on servers with unknown role.");
                    return;
            }

            // Ensure we do not run if not main domain, but do NOT lock it
            if (_mainDom.IsMainDom == false)
            {
                _logger.LogDebug("Does not run if not MainDom.");
                return;
            }

            using (_profilingLogger.DebugDuration<KeepAlive>("Keep alive executing", "Keep alive complete"))
            {
                var keepAlivePingUrl = _keepAliveSettings.KeepAlivePingUrl;
                try
                {
                    if (keepAlivePingUrl.Contains("{umbracoApplicationUrl}"))
                    {
                        var umbracoAppUrl = _requestAccessor.GetApplicationUrl().ToString();
                        if (umbracoAppUrl.IsNullOrWhiteSpace())
                        {
                            _logger.LogWarning("No umbracoApplicationUrl for service (yet), skip.");
                            return;
                        }

                        keepAlivePingUrl = keepAlivePingUrl.Replace("{umbracoApplicationUrl}", umbracoAppUrl.TrimEnd('/'));
                    }

                    var request = new HttpRequestMessage(HttpMethod.Get, keepAlivePingUrl);
                    HttpClient httpClient = _httpClientFactory.CreateClient();
                    _ = await httpClient.SendAsync(request);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Keep alive failed (at '{keepAlivePingUrl}').", keepAlivePingUrl);
                }
            }
        }
    }
}
