﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Core.Configuration.Models;
using Umbraco.Core.Hosting;
using Umbraco.Core.IO;
using Umbraco.Core.IO.MediaPathSchemes;
using Umbraco.Core.Strings;

namespace Umbraco.Core.Composing.CompositionExtensions
{
    internal static class FileSystems
    {
        /*
         * HOW TO REPLACE THE MEDIA UNDERLYING FILESYSTEM
         * ----------------------------------------------
         *
         * Create an implementation of IFileSystem and register it as the underlying filesystem for
         * MediaFileSystem with the following extension on composition.
         *
         * composition.SetMediaFileSystem(factory => FactoryMethodToReturnYourImplementation())
         *
         * Alternatively you can just register an Implementation of IMediaFileSystem, however the
         * extension above ensures that your IFileSystem implementation is wrapped by the "ShadowWrapper".
         *
         * WHAT IS SHADOWING
         * -----------------
         *
         * Shadowing is the technology used for Deploy to implement some sort of
         * transaction-management on top of filesystems. The plumbing explained above,
         * compared to creating your own physical filesystem, ensures that your filesystem
         * would participate into such transactions.
         *
         */

        public static Composition ComposeFileSystems(this Composition composition)
        {
            // register FileSystems, which manages all filesystems
            // it needs to be registered (not only the interface) because it provides additional
            // functionality eg for scoping, and is injected in the scope provider - whereas the
            // interface is really for end-users to get access to filesystems.
            composition.Services.AddUnique(factory => factory.CreateInstance<Core.IO.FileSystems>(factory));

            // register IFileSystems, which gives access too all filesystems
            composition.Services.AddUnique<IFileSystems>(factory => factory.GetRequiredService<Core.IO.FileSystems>());

            // register the scheme for media paths
            composition.Services.AddUnique<IMediaPathScheme, UniqueMediaPathScheme>();

            composition.SetMediaFileSystem(factory =>
            {
                var ioHelper = factory.GetRequiredService<IIOHelper>();
                var hostingEnvironment = factory.GetRequiredService<IHostingEnvironment>();
                var logger = factory.GetRequiredService<ILogger<PhysicalFileSystem>>();
                var globalSettings = factory.GetRequiredService<IOptions<GlobalSettings>>().Value;

                var rootPath = hostingEnvironment.MapPathWebRoot(globalSettings.UmbracoMediaPath);
                var rootUrl = hostingEnvironment.ToAbsolute(globalSettings.UmbracoMediaPath);
                return new PhysicalFileSystem(ioHelper, hostingEnvironment, logger, rootPath, rootUrl);
            });

            return composition;
        }
    }
}
