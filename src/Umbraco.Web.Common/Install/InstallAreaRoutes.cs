﻿using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Routing;
using System;
using System.Threading.Tasks;
using Umbraco.Core;
using Umbraco.Core.Hosting;
using Umbraco.Core.Logging;
using Umbraco.Extensions;
using Umbraco.Web.Common.Routing;

namespace Umbraco.Web.Common.Install
{

    public class InstallAreaRoutes : IAreaRoutes
    {
        private readonly IRuntimeState _runtime;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly LinkGenerator _linkGenerator;

        public InstallAreaRoutes(IRuntimeState runtime, IHostingEnvironment hostingEnvironment, LinkGenerator linkGenerator)
        {
            _runtime = runtime;
            _hostingEnvironment = hostingEnvironment;
            _linkGenerator = linkGenerator;
        }

        public void CreateRoutes(IEndpointRouteBuilder endpoints)
        {
            var installPathSegment = _hostingEnvironment.ToAbsolute(Core.Constants.SystemDirectories.Install).TrimStart('/');

            switch (_runtime.Level)
            {
                case RuntimeLevel.Install:
                case RuntimeLevel.Upgrade:

                    endpoints.MapUmbracoRoute<InstallApiController>(installPathSegment, Core.Constants.Web.Mvc.InstallArea, "api", includeControllerNameInRoute: false);
                    endpoints.MapUmbracoRoute<InstallController>(installPathSegment, Core.Constants.Web.Mvc.InstallArea, string.Empty, includeControllerNameInRoute: false);

                    // register catch all because if we are in install/upgrade mode then we'll catch everything and redirect
                    endpoints.MapFallbackToAreaController(
                        "Redirect",
                        ControllerExtensions.GetControllerName<InstallController>(),
                        Core.Constants.Web.Mvc.InstallArea);

                    
                    break;
                case RuntimeLevel.Run:

                    // when we are in run mode redirect to the back office if the installer endpoint is hit
                    endpoints.MapGet($"{installPathSegment}/{{controller?}}/{{action?}}", context =>
                    {
                        // redirect to umbraco
                        context.Response.Redirect(_linkGenerator.GetBackOfficeUrl(_hostingEnvironment), false);
                        return Task.CompletedTask;
                    });

                    break;
                case RuntimeLevel.BootFailed:
                case RuntimeLevel.Unknown:
                case RuntimeLevel.Boot:
                    break;
                
            }
        }

        
    }
}
