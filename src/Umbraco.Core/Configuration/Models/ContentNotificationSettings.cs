// Copyright (c) Umbraco.
// See LICENSE for more details.

namespace Umbraco.Core.Configuration.Models
{
    /// <summary>
    /// Typed configuration options for content notification settings.
    /// </summary>
    public class ContentNotificationSettings
    {
        /// <summary>
        /// Gets or sets a value for the email address for notifications.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether HTML email notifications should be disabled.
        /// </summary>
        public bool DisableHtmlEmail { get; set; } = false;
    }
}
