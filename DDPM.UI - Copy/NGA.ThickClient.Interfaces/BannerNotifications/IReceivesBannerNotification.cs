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
    /// Interface that must be implemented to get information about the status of a Banner Notification.
    /// </summary>
    public interface IReceivesBannerNotification
    {
        /// <summary>
        /// On user interaction with the banner notification the PluginOwnerId will have its OnBannerNotificationInteracted method invoked.
        /// </summary>
        /// <param name="notificationInteraction" cref="IBannerNotificationInteraction">The parameter will contain the type of interaction that occurred.</param>
        void OnBannerNotificationInteracted(IBannerNotificationInteraction notificationInteraction);
    }
}