﻿using System;
using System.IO;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Core.Cache;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.Models;
using Umbraco.Core.DependencyInjection;
using Umbraco.Core.Events;
using Umbraco.Core.Hosting;
using Umbraco.Core.Packaging;
using Umbraco.Core.Routing;
using Umbraco.Core.Services;
using Umbraco.Core.Services.Implement;

namespace Umbraco.Core.Composing.CompositionExtensions
{
    internal static class Services
    {
        public static IUmbracoBuilder ComposeServices(this IUmbracoBuilder builder)
        {
            // register a transient messages factory, which will be replaced by the web
            // boot manager when running in a web context
            builder.Services.AddUnique<IEventMessagesFactory, TransientEventMessagesFactory>();

            // register the service context
            builder.Services.AddUnique<ServiceContext>();

            // register the special idk map
            builder.Services.AddUnique<IIdKeyMap, IdKeyMap>();

            // register the services
            builder.Services.AddUnique<IPropertyValidationService, PropertyValidationService>();
            builder.Services.AddUnique<IKeyValueService, KeyValueService>();
            builder.Services.AddUnique<IPublicAccessService, PublicAccessService>();
            builder.Services.AddUnique<IDomainService, DomainService>();
            builder.Services.AddUnique<IAuditService, AuditService>();
            builder.Services.AddUnique<ITagService, TagService>();
            builder.Services.AddUnique<IContentService, ContentService>();
            builder.Services.AddUnique<IUserService, UserService>();
            builder.Services.AddUnique<IMemberService, MemberService>();
            builder.Services.AddUnique<IMediaService, MediaService>();
            builder.Services.AddUnique<IContentTypeService, ContentTypeService>();
            builder.Services.AddUnique<IContentTypeBaseServiceProvider, ContentTypeBaseServiceProvider>();
            builder.Services.AddUnique<IMediaTypeService, MediaTypeService>();
            builder.Services.AddUnique<IDataTypeService, DataTypeService>();
            builder.Services.AddUnique<IFileService, FileService>();
            builder.Services.AddUnique<ILocalizationService, LocalizationService>();
            builder.Services.AddUnique<IPackagingService, PackagingService>();
            builder.Services.AddUnique<IServerRegistrationService, ServerRegistrationService>();
            builder.Services.AddUnique<IEntityService, EntityService>();
            builder.Services.AddUnique<IRelationService, RelationService>();
            builder.Services.AddUnique<IMacroService, MacroService>();
            builder.Services.AddUnique<IMemberTypeService, MemberTypeService>();
            builder.Services.AddUnique<IMemberGroupService, MemberGroupService>();
            builder.Services.AddUnique<INotificationService, NotificationService>();
            builder.Services.AddUnique<IExternalLoginService, ExternalLoginService>();
            builder.Services.AddUnique<IRedirectUrlService, RedirectUrlService>();
            builder.Services.AddUnique<IConsentService, ConsentService>();
            builder.Services.AddTransient<LocalizedTextServiceFileSources>(SourcesFactory);
            builder.Services.AddUnique<ILocalizedTextService>(factory => new LocalizedTextService(
                factory.GetRequiredService<Lazy<LocalizedTextServiceFileSources>>(),
                factory.GetRequiredService<ILogger<LocalizedTextService>>()));

            builder.Services.AddUnique<IEntityXmlSerializer, EntityXmlSerializer>();

            builder.Services.AddUnique<IPackageActionRunner, PackageActionRunner>();

            builder.Services.AddUnique<ConflictingPackageData>();
            builder.Services.AddUnique<CompiledPackageXmlParser>();
            builder.Services.AddUnique<ICreatedPackagesRepository>(factory => CreatePackageRepository(factory, "createdPackages.config"));
            builder.Services.AddUnique<IInstalledPackagesRepository>(factory => CreatePackageRepository(factory, "installedPackages.config"));
            builder.Services.AddUnique<PackageDataInstallation>();
            builder.Services.AddUnique<PackageFileInstallation>();
            builder.Services.AddUnique<IPackageInstallation, PackageInstallation>();

            return builder;
        }

        /// <summary>
        /// Creates an instance of PackagesRepository for either the ICreatedPackagesRepository or the IInstalledPackagesRepository
        /// </summary>
        /// <param name="factory"></param>
        /// <param name="packageRepoFileName"></param>
        /// <returns></returns>
        private static PackagesRepository CreatePackageRepository(IServiceProvider factory, string packageRepoFileName)
            => new PackagesRepository(
                factory.GetRequiredService<IContentService>(),
                factory.GetRequiredService<IContentTypeService>(),
                factory.GetRequiredService<IDataTypeService>(),
                factory.GetRequiredService<IFileService>(),
                factory.GetRequiredService<IMacroService>(),
                factory.GetRequiredService<ILocalizationService>(),
                factory.GetRequiredService<IHostingEnvironment>(),
                factory.GetRequiredService<IEntityXmlSerializer>(),
                factory.GetRequiredService<ILoggerFactory>(),
                factory.GetRequiredService<IUmbracoVersion>(),
                factory.GetRequiredService<IOptions<GlobalSettings>>(),
                factory.GetRequiredService<IMediaService>(),
                factory.GetRequiredService<IMediaTypeService>(),
                packageRepoFileName);

        private static LocalizedTextServiceFileSources SourcesFactory(IServiceProvider container)
        {
            var hostingEnvironment = container.GetRequiredService<IHostingEnvironment>();
            var globalSettings = container.GetRequiredService<IOptions<GlobalSettings>>().Value;
            var mainLangFolder = new DirectoryInfo(hostingEnvironment.MapPathContentRoot(WebPath.Combine(globalSettings.UmbracoPath , "config","lang")));
            var appPlugins = new DirectoryInfo(hostingEnvironment.MapPathContentRoot(Constants.SystemDirectories.AppPlugins));
            var configLangFolder = new DirectoryInfo(hostingEnvironment.MapPathContentRoot(WebPath.Combine(Constants.SystemDirectories.Config  ,"lang")));

            var pluginLangFolders = appPlugins.Exists == false
                ? Enumerable.Empty<LocalizedTextServiceSupplementaryFileSource>()
                : appPlugins.GetDirectories()
                    .SelectMany(x => x.GetDirectories("Lang", SearchOption.AllDirectories))
                    .SelectMany(x => x.GetFiles("*.xml", SearchOption.TopDirectoryOnly))
                    .Select(x => new LocalizedTextServiceSupplementaryFileSource(x, false));

            //user defined langs that overwrite the default, these should not be used by plugin creators
            var userLangFolders = configLangFolder.Exists == false
                ? Enumerable.Empty<LocalizedTextServiceSupplementaryFileSource>()
                : configLangFolder
                    .GetFiles("*.user.xml", SearchOption.TopDirectoryOnly)
                    .Select(x => new LocalizedTextServiceSupplementaryFileSource(x, true));

            return new LocalizedTextServiceFileSources(
                container.GetRequiredService<ILogger<LocalizedTextServiceFileSources>>(),
                container.GetRequiredService<AppCaches>(),
                mainLangFolder,
                pluginLangFolders.Concat(userLangFolders));
        }
    }
}
