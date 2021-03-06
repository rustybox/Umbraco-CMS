﻿using Microsoft.Extensions.DependencyInjection;
using Smidge.FileProcessors;
using Umbraco.Core;
using Umbraco.Core.DependencyInjection;
using Umbraco.Core.Composing;
using Umbraco.Core.Runtime;
using Umbraco.Core.WebAssets;

namespace Umbraco.Web.Common.RuntimeMinification
{
    public sealed class SmidgeComposer : IComposer
    {
        public void Compose(IUmbracoBuilder builder)
        {
            // TODO: For this to work we need to have services.AddSmidge() based on the Smidge APIs but our composer APIs don't really let us do that
            // This makes it a bit awkward to boot the runtime since that call would need to be made outside of the composer... .hrm...

            builder.Services.AddUnique<IRuntimeMinifier, SmidgeRuntimeMinifier>();
            builder.Services.AddUnique<SmidgeHelperAccessor>();
            builder.Services.AddTransient<IPreProcessor, SmidgeNuglifyJs>();
        }
    }
}
