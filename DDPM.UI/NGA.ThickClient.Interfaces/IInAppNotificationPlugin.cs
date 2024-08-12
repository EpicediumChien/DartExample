#region LicenceHeader
//
// Copyright © 2022, Dell Inc., All Rights Reserved.
// This material is confidential and a trade secret.  Permission to use this
// work for any purpose must be obtained in writing from Dell Inc.
//
#endregion

using System.Windows;

namespace NGA.ThickClient.Interfaces
{
    /// <summary>
    /// Provides an interface to be implemented by plugins for displaying in-app notifications
    /// </summary>
    public interface IInAppNotificationPlugin
    {
        /// <summary>
        /// Gets a value indicating whether the in-app notification is currently open
        /// </summary>
        bool IsOpen { get; }

        /// <summary>
        /// Asynchronously opens an in-app notification with the specified content, callback function, and duration.
        /// </summary>
        /// <param name="content">
        /// The content to be displayed inside the notification, represented as a FrameworkElement
        /// Allows flexibility in the type of content displayed, such as controls, shapes, and panels.
        /// </param>
        /// <param name="function">
        /// The InAppNotificationClosed delegate representing the callback function to be invoked when the notification is closed.
        /// It receives an InAppNotificationCloseReason parameter indicating the reason the notification was closed.
        /// </param>
        /// <param name="duration">
        /// The duration, in seconds, for which the notification should remain open. 
        /// Defaults to 10 seconds. The notification will auto-close after the elapsed duration.
        /// </param>
        Task<bool> OpenAsync(FrameworkElement content, InAppNotificationClosed function, int duration = 10);

        /// <summary>
        /// Asynchronously closes any open in-app notification.
        /// </summary>
        Task<bool> CloseAsync();
    }

    /// <summary>
    /// Represents a delegate to be used as a callback for handling the closing of in-app notifications.
    /// </summary>
    public delegate void InAppNotificationClosed(InAppNotificationCloseReason reason);
}