#region LicenceHeader
//
// Copyright © 2022, Dell Inc., All Rights Reserved.
// This material is confidential and a trade secret.  Permission to use this
// work for any purpose must be obtained in writing from Dell Inc.
//
#endregion

using Microsoft;
using System.ComponentModel;

namespace NGA.ThickClient.Interfaces.BannerNotifications
{
    /// <summary>
    /// Class defining some properties exposed by UXAlert Control. Contains the properties to identify each banner notification uniquely.
    /// </summary>
    public class BannerNotificationDisplayData : IBannerNotificationDisplayData
    {
        /// <summary>
        /// Gets and Sets BannerId.
        /// </summary>
        public Guid BannerId { get; set; }

        /// <summary>
        /// Gets and Sets BannerGroupId.
        /// </summary>
        public Guid BannerGroupId { get; set; }

        /// <summary>
        /// Gets and Sets PluginOwnerId.
        /// </summary>
        public Guid PluginOwnerId { get; set; }

        /// <summary>
        /// Gets and Sets DisplayPriority.
        /// </summary>
        public DisplayPriority DisplayPriority { get; set; }

        /// <summary>
        /// Gets and Sets BannerItemType.
        /// </summary>
        public BannerItemType BannerItemType { get; set; }

        /// <summary>
        /// Gets and Sets Banner Message.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Gets and Sets Banner Label.
        /// </summary>
        public string? Label { get; set; }

        /// <summary>
        /// Gets and Sets HyperLink Text.
        /// </summary>
        public string? HyperlinkText { get; set; }

        /// <summary>
        /// Gets and Sets Display Duration. 
        /// </summary>
        public TimeSpan? DisplayDuration { get; set; }

        /// <summary>
        /// Constructs the BannerNotificationDisplayData Object with BannerId, BannerGroupId, PluginOwnerId, BannerItemType.
        /// Display Priority is set to Minor by default. 
        /// </summary>
        /// <exception cref="ArgumentException">Thrown if <paramref name="bannerId"/> is <see cref="Guid.Empty"/></exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="bannerGroupId"/> is Empty or Null</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="pluginOwnerId"/> is Empty or Null</exception>
        /// <exception cref="InvalidEnumArgumentException">Thrown if <paramref name="bannerItemType"/> is not defined</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="message"/> is not Empty or Null</exception>
        public BannerNotificationDisplayData(Guid bannerId, Guid bannerGroupId, Guid pluginOwnerId, BannerItemType bannerItemType, string message, DisplayPriority displayPriority = DisplayPriority.Minor)
        {
            Requires.NotEmpty(bannerId, $"{nameof(bannerId)} value cannot be empty.");
            Requires.NotEmpty(bannerGroupId, $" {nameof(bannerGroupId)} value cannot be empty.");
            Requires.NotEmpty(pluginOwnerId, $" {nameof(pluginOwnerId)} value cannot be empty.");
            Requires.Defined(bannerItemType, $"{nameof(bannerItemType)} value has to be defined.");
            Requires.NotNullOrEmpty(message, $"{nameof(message)} value cannot be null or empty.");

            BannerId = bannerId;
            BannerGroupId = bannerGroupId;
            PluginOwnerId = pluginOwnerId;
            DisplayPriority = displayPriority;
            BannerItemType = bannerItemType;
            Message = message;
        }
    }
}
