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
    /// Class defining Banner Notification Results within the ConsoleWindow. 
    /// </summary>
    public class BannerNotificationInteraction : IBannerNotificationInteraction
    {
        /// <summary>
        /// Gets and Sets Unique BannerId. 
        /// </summary>
        /// <remarks>
        /// This is mandatory for every notification instance, could be passed in constructor while creating it.
        /// </remarks>
        public Guid BannerId { get; set; }

        /// <summary>
        /// Gets and Sets BannerGroupId.
        /// </summary>
        /// <remarks>
        /// This is mandatory for every notification instance, could be passed in constructor while creating it.
        /// </remarks>
        public Guid BannerGroupId { get; set; }

        /// <summary>
        /// Gets and Sets UserInteractionType.
        /// </summary>
        /// <remarks>
        /// This is mandatory for every notification instance, could be passed in constructor while creating it.
        /// </remarks>
        public InteractionType UserInteractionType { get; set; }

        /// <summary>
        /// Constructs the object with BannerId , BannerGroupId and Interaction Type.
        /// </summary>
        /// <exception cref="ArgumentException">Thrown if <paramref name="bannerId"/> is <see cref="Guid.Empty"/></exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="bannerGroupId"/> is Empty or Null</exception>
        /// <exception cref="InvalidEnumArgumentException">Thrown if <paramref name="interactionType"/> is Empty or Null</exception>
        public BannerNotificationInteraction(Guid bannerId, Guid bannerGroupId, InteractionType interactionType)
        {
            Requires.NotEmpty(bannerId, $"{nameof(bannerId)} value cannot be empty.");
            Requires.NotEmpty(bannerGroupId, $" {nameof(bannerGroupId)} value cannot be empty.");
            Requires.Defined(interactionType, $" {nameof(interactionType)} value has to be defined.");

            BannerId = bannerId;
            BannerGroupId = bannerGroupId;
            UserInteractionType = interactionType;
        }
    }
}
