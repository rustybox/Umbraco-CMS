﻿using System;

namespace Umbraco.Core
{
    public static partial class Constants
    {
        /// <summary>
        /// Specific web.config AppSetting keys for Umbraco.Core application
        /// </summary>
        public static class AppSettings
        {
            // TODO: Are these all obsolete in netcore now?

            public const string MainDomLock = "Umbraco.Core.MainDom.Lock";

            // TODO: Kill me - still used in Umbraco.Core.IO.SystemFiles:27
            [Obsolete("We need to kill this appsetting as we do not use XML content cache umbraco.config anymore due to NuCache")]
            public const string ContentXML = "Umbraco.Core.ContentXML"; //umbracoContentXML

            /// <summary>
            /// TODO: FILL ME IN
            /// </summary>
            public const string RegisterType = "Umbraco.Core.RegisterType";

            /// <summary>
            /// This is used for a unit test in PublishedMediaCache
            /// </summary>
            public const string PublishedMediaCacheSeconds = "Umbraco.Core.PublishedMediaCacheSeconds";

            /// <summary>
            /// TODO: FILL ME IN
            /// </summary>
            public const string AssembliesAcceptingLoadExceptions = "Umbraco.Core.AssembliesAcceptingLoadExceptions";

            /// <summary>
            /// This will return the version number of the currently installed umbraco instance
            /// </summary>
            /// <remarks>
            /// Umbraco will automatically set & modify this value when installing & upgrading
            /// </remarks>
            public const string ConfigurationStatus = "Umbraco.Core.ConfigurationStatus";

            /// <summary>
            /// The path to umbraco's root directory (/umbraco by default).
            /// </summary>
            public const string UmbracoPath = "Umbraco.Core.Path";

            /// <summary>
            /// Gets the path to umbraco's icons directory (/umbraco/assets/icons by default).
            /// </summary>
            public const string IconsPath = "Umbraco.Icons.Path";

            /// <summary>
            /// The reserved urls from web.config.
            /// </summary>
            public const string ReservedUrls = "Umbraco.Core.ReservedUrls";

            /// <summary>
            /// The path of the stylesheet folder.
            /// </summary>
            public const string UmbracoCssPath = "Umbraco.Web.CssPath";

            /// <summary>
            /// The path of script folder.
            /// </summary>
            public const string UmbracoScriptsPath = "Umbraco.Core.ScriptsPath";

            /// <summary>
            /// The path of media folder.
            /// </summary>
            public const string UmbracoMediaPath = "Umbraco.Core.MediaPath";

            /// <summary>
            /// The reserved paths from web.config
            /// </summary>
            public const string ReservedPaths = "Umbraco.Core.ReservedPaths";

            /// <summary>
            /// Set the timeout for the Umbraco backoffice in minutes
            /// </summary>
            public const string TimeOutInMinutes = "Umbraco.Core.TimeOutInMinutes";

            /// <summary>
            /// The number of days to check for a new version of Umbraco
            /// </summary>
            /// <remarks>
            /// Default is set to 7. Setting this to 0 will never check
            /// This is used to help track statistics
            /// </remarks>
            public const string VersionCheckPeriod = "Umbraco.Core.VersionCheckPeriod";

            /// <summary>
            /// This is the location type to store temporary files such as cache files or other localized files for a given machine
            /// </summary>
            /// <remarks>
            /// Currently used for the xml cache file and the plugin cache files
            /// </remarks>
            public const string LocalTempStorage = "Umbraco.Core.LocalTempStorage";

            /// <summary>
            /// The default UI language of the backoffice such as 'en-US'
            /// </summary>
            public const string DefaultUILanguage = "Umbraco.Core.DefaultUILanguage";

            /// <summary>
            /// A true/false value indicating whether umbraco should hide top level nodes from generated URLs.
            /// </summary>
            public const string HideTopLevelNodeFromPath = "Umbraco.Core.HideTopLevelNodeFromPath";

            /// <summary>
            /// A true or false indicating whether umbraco should force a secure (https) connection to the backoffice.
            /// </summary>
            public const string UseHttps = "Umbraco.Core.UseHttps";

            /// <summary>
            /// TODO: FILL ME IN
            /// </summary>
            public const string DisableElectionForSingleServer = "Umbraco.Core.DisableElectionForSingleServer";

            /// <summary>
            /// Gets the path to the razor file used when no published content is available.
            /// </summary>
            public const string NoNodesViewPath = "Umbraco.Core.NoNodesViewPath";

            /// <summary>
            /// Debug specific web.config AppSetting keys for Umbraco
            /// </summary>
            /// <remarks>
            /// Do not use these keys in a production environment
            /// </remarks>
            public static class Debug
            {
                /// <summary>
                /// When set to true, Scope logs the stack trace for any scope that gets disposed without being completed.
                /// this helps troubleshooting rogue scopes that we forget to complete
                /// </summary>
                public const string LogUncompletedScopes = "Umbraco.Core.Debug.LogUncompletedScopes";

                /// <summary>
                /// When set to true, the Logger creates a mini dump of w3wp in ~/App_Data/MiniDump whenever it logs
                /// an error due to a ThreadAbortException that is due to a timeout.
                /// </summary>
                public const string DumpOnTimeoutThreadAbort = "Umbraco.Core.Debug.DumpOnTimeoutThreadAbort";

                /// <summary>
                /// TODO: FILL ME IN
                /// </summary>
                public const string DatabaseFactoryServerVersion = "Umbraco.Core.Debug.DatabaseFactoryServerVersion";
            }
        }
    }
}
