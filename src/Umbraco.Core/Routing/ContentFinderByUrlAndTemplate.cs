using Umbraco.Core;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.Services;
using Umbraco.Core.Configuration.Models;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;

namespace Umbraco.Web.Routing
{
    /// <summary>
    /// Provides an implementation of <see cref="IContentFinder"/> that handles page nice URLs and a template.
    /// </summary>
    /// <remarks>
    /// <para>This finder allows for an odd routing pattern similar to altTemplate, probably only use case is if there is an alternative mime type template and it should be routable by something like "/hello/world/json" where the JSON template is to be used for the "world" page</para>
    /// <para>Handles <c>/foo/bar/template</c> where <c>/foo/bar</c> is the nice URL of a document, and <c>template</c> a template alias.</para>
    /// <para>If successful, then the template of the document request is also assigned.</para>
    /// </remarks>
    public class ContentFinderByUrlAndTemplate : ContentFinderByUrl
    {
        private readonly ILogger<ContentFinderByUrlAndTemplate> _logger;
        private readonly IFileService _fileService;

        private readonly IContentTypeService _contentTypeService;
        private readonly WebRoutingSettings _webRoutingSettings;

        public ContentFinderByUrlAndTemplate(ILogger<ContentFinderByUrlAndTemplate> logger, IFileService fileService, IContentTypeService contentTypeService, IOptions<WebRoutingSettings> webRoutingSettings)
            : base(logger)
        {
            _logger = logger;
            _fileService = fileService;
            _contentTypeService = contentTypeService;
            _webRoutingSettings = webRoutingSettings.Value;
        }

        /// <summary>
        /// Tries to find and assign an Umbraco document to a <c>PublishedRequest</c>.
        /// </summary>
        /// <param name="frequest">The <c>PublishedRequest</c>.</param>
        /// <returns>A value indicating whether an Umbraco document was found and assigned.</returns>
        /// <remarks>If successful, also assigns the template.</remarks>
        public override bool TryFindContent(IPublishedRequest frequest)
        {
            IPublishedContent node = null;
            var path = frequest.Uri.GetAbsolutePathDecoded();

            if (frequest.HasDomain)
                path = DomainUtilities.PathRelativeToDomain(frequest.Domain.Uri, path);

            // no template if "/"
            if (path == "/")
            {
                _logger.LogDebug("No template in path '/'");
                return false;
            }

            // look for template in last position
            var pos = path.LastIndexOf('/');
            var templateAlias = path.Substring(pos + 1);
            path = pos == 0 ? "/" : path.Substring(0, pos);

            var template = _fileService.GetTemplate(templateAlias);

            if (template == null)
            {
                _logger.LogDebug("Not a valid template: '{TemplateAlias}'", templateAlias);
                return false;
            }

            _logger.LogDebug("Valid template: '{TemplateAlias}'", templateAlias);

            // look for node corresponding to the rest of the route
            var route = frequest.HasDomain ? (frequest.Domain.ContentId + path) : path;
            node = FindContent(frequest, route); // also assigns to published request

            if (node == null)
            {
                _logger.LogDebug("Not a valid route to node: '{Route}'", route);
                return false;
            }

            // IsAllowedTemplate deals both with DisableAlternativeTemplates and ValidateAlternativeTemplates settings
            if (!node.IsAllowedTemplate(_contentTypeService, _webRoutingSettings, template.Id))
            {
                _logger.LogWarning("Alternative template '{TemplateAlias}' is not allowed on node {NodeId}.", template.Alias, node.Id);
                frequest.PublishedContent = null; // clear
                return false;
            }

            // got it
            frequest.TemplateModel = template;
            return true;
        }
    }
}
