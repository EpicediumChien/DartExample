#region LicenceHeader

//
// Copyright © 2022, Dell Inc., All Rights Reserved.
// This material is confidential and a trade secret.  Permission to use this
// work for any purpose must be obtained in writing from Dell Inc.
//

#endregion

using Newtonsoft.Json;

namespace NGA.Manager.Interfaces
{
    /// <summary>
    /// NotificationPreference class
    /// </summary>
    public class NotificationPreference
    {
        #region Properties

        /// <summary>
        /// Receive Notifications property
        /// </summary>
        [JsonProperty]
        public bool ReceiveNotifications { get; set; }

        #endregion
    }
}