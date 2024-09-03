#region LicenceHeader

//
// Copyright © 2022, Dell Inc., All Rights Reserved.
// This material is confidential and a trade secret.  Permission to use this
// work for any purpose must be obtained in writing from Dell Inc.
//

#endregion

namespace NGA.ThickClient.Interfaces
{
    /// <summary>
    /// Interface extension matching some of the properties exposed by UXAlert control and unique properties to identify Banner.
    /// </summary>
    public interface IBannerNotificationDisplayData
    {
        /// <summary>
        /// Unique identifier for an individual banner.
        /// </summary>
        Guid BannerId { get; }

        /// <summary>
        /// Unique identifier for group of banner.
        /// </summary>
        Guid BannerGroupId { get; }

        /// <summary>
        /// Unique identifier for the plugin that requested the banner notification.
        /// </summary>
        Guid PluginOwnerId { get; }

        /// <summary>
        /// Optional value that decides how long the Banner Notification should be displayed to the user.
        /// </summary>
        System.TimeSpan? DisplayDuration { get; }

        /// <summary>
        /// Enum allowing plugins to tell DCF UX in what order it should display the banner notifications.
        /// </summary>
        DisplayPriority DisplayPriority { get; }

        /// <summary>
        /// Enum indicating the type of banners.
        /// </summary>
        BannerItemType BannerItemType { get; }

        /// <summary>
        /// Bold Text to be displayed before Message.
        /// </summary>
        string? Label { get; }

        /// <summary>
        /// Main description text provided to the UXAlertItem control
        /// </summary>
        string Message { get; }

        /// <summary>
        /// Displayed to user to click and navigate to respective landing pages.
        /// </summary>
        string? HyperlinkText { get; }
    }
}