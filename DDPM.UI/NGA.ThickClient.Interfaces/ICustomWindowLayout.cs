#region LicenceHeader
//
// Copyright © 2022, Dell Inc., All Rights Reserved.
// This material is confidential and a trade secret.  Permission to use this
// work for any purpose must be obtained in writing from Dell Inc.
//
#endregion

using Dell.Client.Framework.UX.WPF.Controls;

namespace NGA.ThickClient.Interfaces
{
    /// <summary>
    /// This allows plugins to access the custom layout
    /// </summary>
    public interface ICustomWindowLayout
    {
        /// <summary>
        /// This allows plugins like the notification plugin the ability to access the UXBell
        /// </summary>
        public UXBell NotificationIcon { get; }

        /// <summary>
        /// This allows user to open the Context Menu from MastHead
        /// </summary>
        public UXIconComboBox IconComboBox { get; }
    }
}