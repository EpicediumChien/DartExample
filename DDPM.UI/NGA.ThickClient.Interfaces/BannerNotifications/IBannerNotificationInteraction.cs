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
    /// Result of the user interacting with a banner notification within the ConsoleWindow.
    /// </summary>
    public interface IBannerNotificationInteraction
    {
        /// <summary>
        /// Unique identifier for an individual banner.
        /// </summary>
        Guid BannerId { get; }

        /// <summary>
        /// unique identifier for a set of banners.
        /// </summary>
        Guid BannerGroupId { get;}

        /// <summary>
        /// Specifies the User action taken on Banner notification.
        /// </summary>
        InteractionType UserInteractionType { get; }
    }
}