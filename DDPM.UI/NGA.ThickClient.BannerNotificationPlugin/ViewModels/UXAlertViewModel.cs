#region LicenceHeader
//
// Copyright © 2022, Dell Inc., All Rights Reserved.
// This material is confidential and a trade secret.  Permission to use this
// work for any purpose must be obtained in writing from Dell Inc.
//
#endregion

using Dell.Client.Framework.UX.WPF.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace NGA.ThickClient.BannerNotificationPlugin.ViewModels
{
    /// <summary>
    /// View Model for Banner Notification Plugin.
    /// </summary>
    public class UXAlertViewModel : ObservableObject
    {
        #region PrivateFields

        private string? _label = string.Empty;
        private string _message = string.Empty;
        private UXAlertType? _alertItemType;
        private string? _hyperlinkText = string.Empty;
        private Guid _bannerId = Guid.Empty;
        private Guid _bannerGroupId = Guid.Empty;
        private Guid _pluginOwnerId = Guid.Empty;

        #endregion

        #region Properties   

        /// <summary>
        /// BannerId of Plugin.
        /// </summary>
        public Guid BannerId
        {
            get => _bannerId;
            set => SetProperty(ref _bannerId, value);
        }

        /// <summary>
        /// BannerGroupId of Plugin.
        /// </summary>
        public Guid BannerGroupId
        {
            get => _bannerGroupId;
            set => SetProperty(ref _bannerGroupId, value);
        }

        /// <summary>
        /// PluginOwnerId of Plugin.
        /// </summary>
        public Guid PluginOwnerId
        {
            get => _pluginOwnerId;
            set => SetProperty(ref _pluginOwnerId, value);
        }

        /// <summary>
        /// Label for Plugin.
        /// </summary>
        public string? Label
        {
            get => _label;
            set => SetProperty(ref _label, value);
        }


        /// <summary>
        /// Message for Plugin.
        /// </summary>
        public string Message
        {
            get => _message;
            set => SetProperty(ref _message, value);
        }

        /// <summary>
        /// AlertItemType for Plugin.
        /// </summary>
        public UXAlertType? AlertItemType
        {
            get => _alertItemType;
            set => SetProperty(ref _alertItemType, value);
        }

        /// <summary>
        /// HyperlinkText for Plugin.
        /// </summary>
        public string? HyperlinkText
        {
            get => _hyperlinkText;
            set => SetProperty(ref _hyperlinkText, value);
        }

        #endregion
    }
}
