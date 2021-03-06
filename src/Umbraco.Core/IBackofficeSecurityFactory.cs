﻿using Umbraco.Web.Security;

namespace Umbraco.Core
{
    /// <summary>
    /// Creates and manages <see cref="IBackOfficeSecurity"/> instances.
    /// </summary>
    public interface IBackOfficeSecurityFactory
    {
        /// <summary>
        /// Ensures that a current <see cref="IBackOfficeSecurity"/> exists.
        /// </summary>
        void EnsureBackOfficeSecurity();
    }
}
