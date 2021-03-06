﻿using Microsoft.Extensions.DependencyInjection;
using System;

namespace Umbraco.Web.BackOffice.Security
{
    /// <summary>
    /// Used to add back office login providers
    /// </summary>
    public class BackOfficeExternalLoginsBuilder
    {
        public BackOfficeExternalLoginsBuilder(IServiceCollection services)
        {
            _services = services;
        }

        private readonly IServiceCollection _services;

        /// <summary>
        /// Add a back office login provider with options
        /// </summary>
        /// <param name="loginProviderOptions"></param>
        /// <param name="build"></param>
        /// <returns></returns>
        public BackOfficeExternalLoginsBuilder AddBackOfficeLogin(
            BackOfficeExternalLoginProviderOptions loginProviderOptions,
            Action<BackOfficeAuthenticationBuilder> build)
        {
            build(new BackOfficeAuthenticationBuilder(_services, loginProviderOptions));
            return this;
        }
    }

}
