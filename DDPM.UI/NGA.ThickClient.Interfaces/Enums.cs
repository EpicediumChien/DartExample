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
    /// Enum to identify how critical the banner notification is.
    /// </summary>
    public enum DisplayPriority
    {
        /// <summary>
        /// Banner notification would go last.
        /// </summary>
        Undefined = 0,

        /// <summary>
        /// This will put the notification at the top of the display queue behind any Critical, Major, or Medium Banner Notifications
        /// </summary>
        Minor = 1,

        /// <summary>
        /// This will put the notification at the top of the display queue behind any Critical or Major Banner Notifications.
        /// </summary>
        Medium = 2,

        /// <summary>
        /// This will put the notification at the top of the display queue behind any Critical priority Banner Notifications.
        /// </summary>
        Major = 3,

        /// <summary>
        /// This will remove any currently displayed Banner Notifications, besides other critical Banner Notifications, and display the next critical Banner Notification.
        /// </summary>
        Critical = 4
    }

    /// <summary>
    /// Enum to show different type of banner notification.
    /// </summary>
    public enum BannerItemType
    {
        /// <summary>
        /// Confirmation type.
        /// </summary>
        Confirm,

        /// <summary>
        /// Information type.
        /// </summary>
        Info,

        /// <summary>
        /// Warning type.
        /// </summary>
        Warning,

        /// <summary>
        /// Critical type.
        /// </summary>
        Critical
    }

    /// <summary>
    /// Enum defines the actions the user could have taken in regards to a banner notification.
    /// </summary>
    public enum InteractionType
    {
        /// <summary>
        /// This is an invalid action.
        /// </summary>
        Undefined = 0,

        /// <summary>
        /// Indicates that no user action is made and banner was allowed to expire.
        /// </summary>
        Expired = 1,

        /// <summary>
        /// This indicates that the user closed the Banner Notification.
        /// </summary>
        ControlClosed = 2,

        /// <summary>
        /// This indicates that the user interacted within the Banner Notification.
        /// </summary>
        ControlAction = 3
    }

    /// <summary>
    /// Enum defines possible reasons for which an in-app notification can be closed.
    /// </summary>
    public enum InAppNotificationCloseReason
    {
        /// <summary>
        /// Represents an unknown reason or any reason not covered by other enumeration values.
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// Represents the scenario where the notification is closed due to the elapse of the preset duration.
        /// </summary>
        TimedOut = 1,

        /// <summary>
        /// Represents the scenario where the notification is explicitly closed by the user.
        /// </summary>
        UserClosed = 2,

        /// <summary>
        /// Represents the scenario where the notification is closed due to a programmatic request by a plugin
        /// </summary>
        CloseRequested = 3
    }
}