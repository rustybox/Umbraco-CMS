// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Umbraco.Core;
using Umbraco.Core.Security;
using Umbraco.Core.Services;
using Umbraco.Web.Editors;

namespace Umbraco.Web.BackOffice.Authorization
{
    /// <summary>
    /// If the users being edited is an admin then we must ensure that the current user is also an admin.
    /// </summary>
    public class AdminUsersHandler : MustSatisfyRequirementAuthorizationHandler<AdminUsersRequirement>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUserService _userService;
        private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;
        private readonly UserEditorAuthorizationHelper _userEditorAuthorizationHelper;

        /// <summary>
        /// Initializes a new instance of the <see cref="AdminUsersHandler"/> class.
        /// </summary>
        /// <param name="httpContextAccessor">Accessor for the HTTP context of the current request.</param>
        /// <param name="userService">Service for user related operations.</param>
        /// <param name="backOfficeSecurityAccessor">Accessor for back-office security.</param>
        /// <param name="userEditorAuthorizationHelper">Helper for user authorization checks.</param>
        public AdminUsersHandler(
            IHttpContextAccessor httpContextAccessor,
            IUserService userService,
            IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
            UserEditorAuthorizationHelper userEditorAuthorizationHelper)
        {
            _httpContextAccessor = httpContextAccessor;
            _userService = userService;
            _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
            _userEditorAuthorizationHelper = userEditorAuthorizationHelper;
        }

        /// <inheritdoc/>
        protected override Task<bool> IsAuthorized(AuthorizationHandlerContext context, AdminUsersRequirement requirement)
        {
            StringValues? queryString = _httpContextAccessor.HttpContext?.Request.Query[requirement.QueryStringName];
            if (!queryString.HasValue || !queryString.Value.Any())
            {
                // Must succeed this requirement since we cannot process it.
                return Task.FromResult(true);
            }

            int[] userIds;
            if (int.TryParse(queryString, out var userId))
            {
                userIds = new[] { userId };
            }
            else
            {
                var ids = _httpContextAccessor.HttpContext.Request.Query.Where(x => x.Key == requirement.QueryStringName).ToList();
                if (ids.Count == 0)
                {
                    // Must succeed this requirement since we cannot process it.
                    return Task.FromResult(true);
                }

                userIds = ids
                    .Select(x => x.Value.ToString())
                    .Select(x => x.TryConvertTo<int>()).Where(x => x.Success).Select(x => x.Result).ToArray();
            }

            if (userIds.Length == 0)
            {
                // Must succeed this requirement since we cannot process it.
                return Task.FromResult(true);
            }

            IEnumerable<Core.Models.Membership.IUser> users = _userService.GetUsersById(userIds);
            var isAuth = users.All(user => _userEditorAuthorizationHelper.IsAuthorized(_backOfficeSecurityAccessor.BackOfficeSecurity.CurrentUser, user, null, null, null) != false);

            return Task.FromResult(isAuth);
        }
    }
}
