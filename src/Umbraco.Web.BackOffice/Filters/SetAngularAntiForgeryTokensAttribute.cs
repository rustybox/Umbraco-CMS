﻿using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using Umbraco.Core;
using Umbraco.Core.Configuration.Models;
using Umbraco.Web.BackOffice.Security;

namespace Umbraco.Web.BackOffice.Filters
{
    /// <summary>
    /// An attribute/filter to set the csrf cookie token based on angular conventions
    /// </summary>
    public class SetAngularAntiForgeryTokensAttribute : TypeFilterAttribute
    {
        public SetAngularAntiForgeryTokensAttribute() : base(typeof(SetAngularAntiForgeryTokensFilter))
        {
        }

        internal class SetAngularAntiForgeryTokensFilter : IAsyncActionFilter
        {
            private readonly IBackOfficeAntiforgery _antiforgery;
            private readonly GlobalSettings _globalSettings;

            public SetAngularAntiForgeryTokensFilter(IBackOfficeAntiforgery antiforgery, IOptions<GlobalSettings> globalSettings)
            {
                _antiforgery = antiforgery;
                _globalSettings = globalSettings.Value;
            }

            public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
            {
                if (context.HttpContext.Response != null)
                {
                    //DO not set the token cookies if the request has failed!!
                    if (context.HttpContext.Response.StatusCode == (int)HttpStatusCode.OK)
                    {
                        //don't need to set the cookie if they already exist and they are valid
                        if (context.HttpContext.Request.Cookies.TryGetValue(Constants.Web.AngularCookieName, out var angularCookieVal)
                            && context.HttpContext.Request.Cookies.TryGetValue(Constants.Web.CsrfValidationCookieName, out var csrfCookieVal))
                        {
                            //if they are not valid for some strange reason - we need to continue setting valid ones
                            var valResult = await _antiforgery.ValidateRequestAsync(context.HttpContext);
                            if (valResult.Success)
                            {
                                await next();
                                return;
                            }
                        }

                        string cookieToken, headerToken;
                        _antiforgery.GetTokens(context.HttpContext, out cookieToken, out headerToken);

                        //We need to set 2 cookies: one is the cookie value that angular will use to set a header value on each request,
                        // the 2nd is the validation value generated by the anti-forgery helper that we use to validate the header token against.

                        if (!(headerToken is null))
                        {
                            context.HttpContext.Response.Cookies.Append(
                                Constants.Web.AngularCookieName, headerToken,
                                new Microsoft.AspNetCore.Http.CookieOptions
                                {
                                    Path = "/",
                                    //must be js readable
                                    HttpOnly = false,
                                    Secure = _globalSettings.UseHttps
                                });
                        }

                        if (!(cookieToken is null))
                        {
                            context.HttpContext.Response.Cookies.Append(
                                Constants.Web.CsrfValidationCookieName, cookieToken,
                                new Microsoft.AspNetCore.Http.CookieOptions
                                {
                                    Path = "/",
                                    HttpOnly = true,
                                    Secure = _globalSettings.UseHttps
                                });
                        }

                    }
                }

                await next();
            }

        }
    }
}
