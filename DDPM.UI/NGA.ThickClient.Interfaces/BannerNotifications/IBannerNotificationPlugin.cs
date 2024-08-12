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
    /// API provided to NGA plugins so they can add In Application Notifications.
    /// </summary>
    public interface IBannerNotificationPlugin
    {
        /// <summary>
        /// Is to check if the banner notification is currently displayed or not. (Corresponds to IsOpen property)
        /// </summary>
        bool IsAnyNotificationCurrentlyDisplayed { get; }

        /// <summary>
        /// Returns the Banner Id of Currently Displayed notification and Returns Guid.Empty in case of empty banner.
        /// </summary>
        Guid CurrentlyDisplayedNotification { get; }

        /// <summary>
        /// Returns number of notifications in Queue.
        /// </summary>
        int NotificationQueueCount { get; }

        /// <summary>
        /// Method to add In-Application Banner notifications Immediately or Queue It depending on the current banner state. 
        /// </summary>
        /// <param name="notificationData" cref="IBannerNotificationDisplayData"></param>
        void AddNotification(IBannerNotificationDisplayData notificationData);

        /// <summary>
        /// Method to update an existing banner notifications currently displayed or queued for display. 
        /// </summary>
        /// <param name="notificationData" cref="IBannerNotificationDisplayData"></param>
        /// <returns>True on Successfully updating Banner, else false</returns>
        bool UpdateNotification(IBannerNotificationDisplayData notificationData);

        /// <summary>
        /// Method to remove the notification from queue or display.
        /// </summary>
        /// <param name="bannerId">Banner Id of specific banner that needs to be removed</param>
        /// <returns>True on Successfully updating Banner, else false</returns>
        bool RemoveNotification(Guid bannerId);

        /// <summary>
        /// Removes the Group of notifications from the queue or display belonging to same GroupId. 
        /// </summary>
        /// <param name="bannerGroupId" cref="IBannerNotificationDisplayData.BannerGroupId">Banner Id of specific set of banners</param>
        /// <returns>Number of notifications removed</returns>
        int RemoveNotificationByGroup(Guid bannerGroupId);

        /// <summary>
        /// Removes all notifications from queue or display matching plugin Id.
        /// </summary>
        /// <param name="pluginId" cref="IBannerNotificationDisplayData.PluginOwnerId"></param>
        /// <returns>Number of notifications removed</returns>
        int RemoveNotificationByPluginId(Guid pluginId);
    }
}
