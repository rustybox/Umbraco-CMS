﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Umbraco.Core.Cache;
using Umbraco.Core.Hosting;
using Umbraco.Core.IO;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Serialization;
using Umbraco.Core.Services;
using Umbraco.Core.Strings;

namespace Umbraco.Core.Manifest
{
    /// <summary>
    /// Parses the Main.js file and replaces all tokens accordingly.
    /// </summary>
    public class ManifestParser : IManifestParser
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly IIOHelper _ioHelper;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IJsonSerializer _jsonSerializer;
        private readonly ILocalizedTextService _localizedTextService;
        private readonly IShortStringHelper _shortStringHelper;
        private static readonly string Utf8Preamble = Encoding.UTF8.GetString(Encoding.UTF8.GetPreamble());

        private readonly IAppPolicyCache _cache;
        private readonly ILogger<ManifestParser> _logger;
        private readonly IDataTypeService _dataTypeService;
        private readonly ILocalizationService _localizationService;
        private readonly ManifestValueValidatorCollection _validators;
        private readonly ManifestFilterCollection _filters;

        private string _path;

        /// <summary>
        /// Initializes a new instance of the <see cref="ManifestParser"/> class.
        /// </summary>
        public ManifestParser(
            AppCaches appCaches,
            ManifestValueValidatorCollection validators,
            ManifestFilterCollection filters,
            ILogger<ManifestParser> logger,
            ILoggerFactory loggerFactory,
            IIOHelper ioHelper,
            IHostingEnvironment hostingEnvironment,
            IDataTypeService dataTypeService,
            ILocalizationService localizationService,
            IJsonSerializer jsonSerializer,
            ILocalizedTextService localizedTextService,
            IShortStringHelper shortStringHelper)
            : this(appCaches, validators, filters, "~/App_Plugins", logger, loggerFactory, ioHelper, hostingEnvironment, dataTypeService, localizationService)
        {
            _loggerFactory = loggerFactory;
            _jsonSerializer = jsonSerializer;
            _localizedTextService = localizedTextService;
            _shortStringHelper = shortStringHelper;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ManifestParser"/> class.
        /// </summary>
        private ManifestParser(AppCaches appCaches, ManifestValueValidatorCollection validators, ManifestFilterCollection filters, string path, ILogger<ManifestParser> logger, ILoggerFactory loggerFactory, IIOHelper ioHelper, IHostingEnvironment hostingEnvironment, IDataTypeService dataTypeService, ILocalizationService localizationService)
        {
            if (appCaches == null) throw new ArgumentNullException(nameof(appCaches));
            _cache = appCaches.RuntimeCache;
            _dataTypeService = dataTypeService;
            _localizationService = localizationService;
            _validators = validators ?? throw new ArgumentNullException(nameof(validators));
            _filters = filters ?? throw new ArgumentNullException(nameof(filters));
            if (path == null) throw new ArgumentNullException(nameof(path));
            if (string.IsNullOrWhiteSpace(path)) throw new ArgumentException("Value can't be empty or consist only of white-space characters.", nameof(path));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
            _ioHelper = ioHelper;
            _hostingEnvironment = hostingEnvironment;

            Path = path;


        }

        public string Path
        {
            get => _path;
            set => _path = value.StartsWith("~/") ? _hostingEnvironment.MapPathContentRoot(value) : value;
        }

        /// <summary>
        /// Gets all manifests, merged into a single manifest object.
        /// </summary>
        /// <returns></returns>
        public PackageManifest Manifest
            => _cache.GetCacheItem<PackageManifest>("Umbraco.Core.Manifest.ManifestParser::Manifests", () =>
            {
                var manifests = GetManifests();
                return MergeManifests(manifests);
            }, new TimeSpan(0, 4, 0));

        /// <summary>
        /// Gets all manifests.
        /// </summary>
        private IEnumerable<PackageManifest> GetManifests()
        {
            var manifests = new List<PackageManifest>();

            foreach (var path in GetManifestFiles())
            {
                try
                {
                    var text = File.ReadAllText(path);
                    text = TrimPreamble(text);
                    if (string.IsNullOrWhiteSpace(text))
                        continue;
                    var manifest = ParseManifest(text);
                    manifest.Source = path;
                    manifests.Add(manifest);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Failed to parse manifest at '{Path}', ignoring.", path);
                }
            }

            _filters.Filter(manifests);

            return manifests;
        }

        /// <summary>
        /// Merges all manifests into one.
        /// </summary>
        private static PackageManifest MergeManifests(IEnumerable<PackageManifest> manifests)
        {
            var scripts = new HashSet<string>();
            var stylesheets = new HashSet<string>();
            var propertyEditors = new List<IDataEditor>();
            var parameterEditors = new List<IDataEditor>();
            var gridEditors = new List<GridEditor>();
            var contentApps = new List<ManifestContentAppDefinition>();
            var dashboards = new List<ManifestDashboard>();
            var sections = new List<ManifestSection>();

            foreach (var manifest in manifests)
            {
                if (manifest.Scripts != null) foreach (var script in manifest.Scripts) scripts.Add(script);
                if (manifest.Stylesheets != null) foreach (var stylesheet in manifest.Stylesheets) stylesheets.Add(stylesheet);
                if (manifest.PropertyEditors != null) propertyEditors.AddRange(manifest.PropertyEditors);
                if (manifest.ParameterEditors != null) parameterEditors.AddRange(manifest.ParameterEditors);
                if (manifest.GridEditors != null) gridEditors.AddRange(manifest.GridEditors);
                if (manifest.ContentApps != null) contentApps.AddRange(manifest.ContentApps);
                if (manifest.Dashboards != null) dashboards.AddRange(manifest.Dashboards);
                if (manifest.Sections != null) sections.AddRange(manifest.Sections.DistinctBy(x => x.Alias.ToLowerInvariant()));
            }

            return new PackageManifest
            {
                Scripts = scripts.ToArray(),
                Stylesheets = stylesheets.ToArray(),
                PropertyEditors = propertyEditors.ToArray(),
                ParameterEditors = parameterEditors.ToArray(),
                GridEditors = gridEditors.ToArray(),
                ContentApps = contentApps.ToArray(),
                Dashboards = dashboards.ToArray(),
                Sections = sections.ToArray()
            };
        }

        // gets all manifest files (recursively)
        private IEnumerable<string> GetManifestFiles()
        {
            if (Directory.Exists(_path) == false)
                return new string[0];
            return Directory.GetFiles(_path, "package.manifest", SearchOption.AllDirectories);
        }

        private static string TrimPreamble(string text)
        {
            // strangely StartsWith(preamble) would always return true
            if (text.Substring(0, 1) == Utf8Preamble)
                text = text.Remove(0, Utf8Preamble.Length);

            return text;
        }

        /// <summary>
        /// Parses a manifest.
        /// </summary>
        public PackageManifest ParseManifest(string text)
        {
            if (text == null) throw new ArgumentNullException(nameof(text));
            if (string.IsNullOrWhiteSpace(text)) throw new ArgumentException("Value can't be empty or consist only of white-space characters.", nameof(text));

            var manifest = JsonConvert.DeserializeObject<PackageManifest>(text,
                new DataEditorConverter(_loggerFactory, _ioHelper, _dataTypeService, _localizationService, _localizedTextService, _shortStringHelper, _jsonSerializer),
                new ValueValidatorConverter(_validators),
                new DashboardAccessRuleConverter());

            // scripts and stylesheets are raw string, must process here
            for (var i = 0; i < manifest.Scripts.Length; i++)
                manifest.Scripts[i] = _ioHelper.ResolveRelativeOrVirtualUrl(manifest.Scripts[i]);
            for (var i = 0; i < manifest.Stylesheets.Length; i++)
                manifest.Stylesheets[i] = _ioHelper.ResolveRelativeOrVirtualUrl(manifest.Stylesheets[i]);
            foreach (var contentApp in manifest.ContentApps)
            {
                contentApp.View = _ioHelper.ResolveRelativeOrVirtualUrl(contentApp.View);
            }
            foreach (var dashboard in manifest.Dashboards)
            {
                dashboard.View = _ioHelper.ResolveRelativeOrVirtualUrl(dashboard.View);
            }
            foreach (var gridEditor in manifest.GridEditors)
            {
                gridEditor.View = _ioHelper.ResolveRelativeOrVirtualUrl(gridEditor.View);
                gridEditor.Render = _ioHelper.ResolveRelativeOrVirtualUrl(gridEditor.Render);
            }

            // add property editors that are also parameter editors, to the parameter editors list
            // (the manifest format is kinda legacy)
            var ppEditors = manifest.PropertyEditors.Where(x => (x.Type & EditorType.MacroParameter) > 0).ToList();
            if (ppEditors.Count > 0)
                manifest.ParameterEditors = manifest.ParameterEditors.Union(ppEditors).ToArray();

            return manifest;
        }
    }
}
