#region LicenceHeader

//
// Copyright © 2022, Dell Inc., All Rights Reserved.
// This material is confidential and a trade secret.  Permission to use this
// work for any purpose must be obtained in writing from Dell Inc.
//

#endregion

namespace NGA.Manager.Interfaces
{
    /// <summary>
    /// NotificationType Enum
    /// </summary>
    public enum NotificationType
    {
        /// <summary>
        /// Unknown
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// Windows Action Center Notification
        /// </summary>
        ActionCenter = 1,

        /// <summary>
        /// Popup Notification
        /// </summary>
        PopUp = 2
    }
}