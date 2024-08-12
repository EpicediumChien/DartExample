#region LicenceHeader
//
// Copyright © 2022, Dell Inc., All Rights Reserved.
// This material is confidential and a trade secret.  Permission to use this
// work for any purpose must be obtained in writing from Dell Inc.
//
#endregion

using CommunityToolkit.Mvvm.ComponentModel;

namespace NGA.ThickClient.BannerNotificationPlugin.ViewModels
{
    /// <summary>
    /// View model for the bell control
    /// </summary>
    public class UXBellViewModel : ObservableObject
    {
        private bool _enabled;
        private bool _notificationAvailable;

        /// <summary>
        /// Label for Plugin.
        /// </summary>
        public bool Enabled
        {
            get => _enabled;
            set => SetProperty(ref _enabled, value);
        }

        /// <summary>
        /// Notification Available
        /// </summary>
        public bool NotificationAvailable
        {
            get => _notificationAvailable;
            set => SetProperty(ref _notificationAvailable, value);
        }
    }
}
