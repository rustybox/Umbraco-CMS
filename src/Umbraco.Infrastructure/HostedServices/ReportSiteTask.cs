﻿using Newtonsoft.Json;
using System;
using System.IO;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Hosting;
using Umbraco.Core.IO;
using Umbraco.Infrastructure.HostedServices;

namespace Umbraco.Web.Telemetry
{
    public class ReportSiteTask : RecurringHostedServiceBase
    {
        private readonly ILogger<ReportSiteTask> _logger;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IUmbracoVersion _umbracoVersion;
        private static HttpClient s_httpClient;

        public ReportSiteTask(
            ILogger<ReportSiteTask> logger,
            IHostingEnvironment hostingEnvironment,
            IUmbracoVersion umbracoVersion)
            : base(TimeSpan.FromDays(1), TimeSpan.FromMinutes(1))
        {
            _logger = logger;
            _hostingEnvironment = hostingEnvironment;
            _umbracoVersion = umbracoVersion;
            s_httpClient = new HttpClient();
        }

        /// <summary>
        /// Runs the background task to send the anynomous ID
        /// to telemetry service
        /// </summary>
        internal override async Task PerformExecuteAsync(object state)
        {
             // Try & find file at '/umbraco/telemetrics-id.umb'
            var telemetricsFilePath = _hostingEnvironment.MapPathContentRoot(SystemFiles.TelemetricsIdentifier);

            if (File.Exists(telemetricsFilePath) == false)
            {
                // Some users may have decided to not be tracked by deleting/removing the marker file
                _logger.LogWarning("No telemetry marker file found at '{filePath}' and will not report site to telemetry service", telemetricsFilePath);

                return;
            }


            string telemetricsFileContents;
            try
            {
                // Open file & read its contents
                // It may throw due to file permissions or file locking
                telemetricsFileContents = File.ReadAllText(telemetricsFilePath);
            }
            catch (Exception ex)
            {
                // Silently swallow ex - but lets log it (ReadAllText throws a ton of different types of ex)
                // Hence the use of general exception type
                _logger.LogError(ex, "Error in reading file contents of telemetry marker file found at '{filePath}'", telemetricsFilePath);

                // Exit out early, but mark this task to be repeated in case its a file lock so it can be rechecked the next time round
                return;
            }


            // Parse as a GUID & verify its a GUID and not some random string
            // In case of users may have messed or decided to empty the file contents or put in something random
            if (Guid.TryParse(telemetricsFileContents, out var telemetrySiteIdentifier) == false)
            {
                // Some users may have decided to mess with file contents
                _logger.LogWarning("The telemetry marker file found at '{filePath}' with '{telemetrySiteId}' is not a valid identifier for the telemetry service", telemetricsFilePath, telemetrySiteIdentifier);

                return;
            }

            try
            {

                // Send data to LIVE telemetry
                s_httpClient.BaseAddress = new Uri("https://telemetry.umbraco.com/");

#if DEBUG
                // Send data to DEBUG telemetry service
                s_httpClient.BaseAddress = new Uri("https://telemetry.rainbowsrock.net/");
#endif

                s_httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");

                using (var request = new HttpRequestMessage(HttpMethod.Post, "installs/"))
                {
                    var postData = new TelemetryReportData { Id = telemetrySiteIdentifier, Version = _umbracoVersion.SemanticVersion.ToSemanticString() };
                    request.Content = new StringContent(JsonConvert.SerializeObject(postData), Encoding.UTF8, "application/json"); //CONTENT-TYPE header

                    // Set a low timeout - no need to use a larger default timeout for this POST request
                    s_httpClient.Timeout = new TimeSpan(0, 0, 1);

                    // Make a HTTP Post to telemetry service
                    // https://telemetry.umbraco.com/installs/
                    // Fire & Forget, do not need to know if its a 200, 500 etc
                    using (HttpResponseMessage response = await s_httpClient.SendAsync(request))
                    {
                    }
                }
            }
            catch
            {
                // Silently swallow
                // The user does not need the logs being polluted if our service has fallen over or is down etc
                // Hence only loggigng this at a more verbose level (Which users should not be using in prod)
                _logger.LogDebug("There was a problem sending a request to the Umbraco telemetry service");
            }
        }

        [DataContract]
        private class TelemetryReportData
        {
            [DataMember(Name = "id")]
            public Guid Id { get; set; }

            [DataMember(Name = "version")]
            public string Version { get; set; }
        }


    }
}
