#region LicenceHeader

//
// Copyright © 2022, Dell Inc., All Rights Reserved.
// This material is confidential and a trade secret.  Permission to use this
// work for any purpose must be obtained in writing from Dell Inc.
//

#endregion

using Dell.Client.Framework.Common;

namespace NGA.Manager.Interfaces;

/// <summary>
/// The IDiscoveryNotificationPlugin is an interface that the DiscoveryNotificationPlugin implements. This interface is published via Dell Tech Hub so the NGA ThickClient's Discovery Thick Client Plugin can invoke functions.
/// </summary>
public interface IDiscoveryNotificationPlugin : IFrameworkPlugin
{
    /// <summary>
    /// The ThickClientOpened method is a published API that the Discovery Thick Client plugin can invoke to tell the Discovery Notification Plugin that the user has opened the NGA Thick Client.
    /// </summary>
    /// <param name="sid">user sid.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>ValueTask</returns>
    ValueTask ThickClientOpened(string sid, CancellationToken cancellationToken);
}