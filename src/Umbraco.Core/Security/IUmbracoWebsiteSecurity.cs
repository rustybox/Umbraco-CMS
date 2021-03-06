﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Umbraco.Core.Models.Security;

namespace Umbraco.Core.Security
{
    public interface IUmbracoWebsiteSecurity
    {
        /// <summary>
        /// Creates a model to use for registering new members with custom member properties
        /// </summary>
        /// <param name="memberTypeAlias">Alias of member type for created member (default used if not provided).</param>
        /// <returns>Instance of <see cref="RegisterModel"/></returns>
        RegisterModel CreateRegistrationModel(string memberTypeAlias = null);

        /// <summary>
        /// Registers a new member.
        /// </summary>
        /// <param name="model">Register member model.</param>
        /// <param name="logMemberIn">Flag for whether to log the member in upon successful registration.</param>
        /// <returns>Result of registration operation.</returns>
        Task<RegisterMemberStatus> RegisterMemberAsync(RegisterModel model, bool logMemberIn = true);

        /// <summary>
        /// Creates a new profile model filled in with the current members details if they are logged in which allows for editing
        /// profile properties.
        /// </summary>
        /// <returns>Instance of <see cref="ProfileModel"/></returns>
        Task<ProfileModel> GetCurrentMemberProfileModelAsync();

        /// <summary>
        /// Updates the currently logged in member's profile.
        /// </summary>
        /// <param name="model">Update member profile model.</param>
        /// <returns>Result of update profile operation.</returns>
        Task<UpdateMemberProfileResult> UpdateMemberProfileAsync(ProfileModel model);

        /// <summary>
        /// A helper method to perform the validation and logging in of a member.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="password">The password.</param>
        /// <returns>Result of login operation.</returns>
        Task<bool> LoginAsync(string username, string password);

        /// <summary>
        /// Check if a member is logged in
        /// </summary>
        /// <returns>True if logged in, false if not.</returns>
        bool IsLoggedIn();

        /// <summary>
        /// Returns the login status model of the currently logged in member.
        /// </summary>
        /// <returns>Instance of <see cref="LoginStatusModel"/></returns>
        Task<LoginStatusModel> GetCurrentLoginStatusAsync();

        /// <summary>
        /// Logs out the current member.
        /// </summary>
        Task LogOutAsync();

        /// <summary>
        /// Checks if the current member is authorized based on the parameters provided.
        /// </summary>
        /// <param name="allowTypes">Allowed types.</param>
        /// <param name="allowGroups">Allowed groups.</param>
        /// <param name="allowMembers">Allowed individual members.</param>
        /// <returns>True or false if the currently logged in member is authorized</returns>
        bool IsMemberAuthorized(
            IEnumerable<string> allowTypes = null,
            IEnumerable<string> allowGroups = null,
            IEnumerable<int> allowMembers = null);
    }
}
