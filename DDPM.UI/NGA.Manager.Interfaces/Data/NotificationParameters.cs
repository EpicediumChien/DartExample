#region LicenceHeader
//
// Copyright © 2022, Dell Inc., All Rights Reserved.
// This material is confidential and a trade secret.  Permission to use this
// work for any purpose must be obtained in writing from Dell Inc.
//
#endregion

using System;
using Microsoft;
using Newtonsoft.Json;

namespace NGA.Manager.Interfaces;

/// <summary>
/// NotificationDuration
/// </summary>
public class NotificationParameters
{
    /// <summary>
    /// PluginId: Source plug-in id
    /// </summary>
    [JsonProperty]
    public Guid PluginId { get; protected set; }

    /// <summary>
    /// NotificationId: notification id
    /// </summary>
    [JsonProperty]
    public Guid NotificationId { get; protected set; }

    /// <summary>
    /// SecurityIdentifier for the user
    /// </summary>
    [JsonProperty]
    public string Sid { get; protected set; }

    /// <summary>
    /// NotificationDuration
    /// </summary>
    [JsonConstructor]
    protected NotificationParameters()
    {
    }

    /// <summary>
    /// NotificationParameters
    /// </summary>
    /// <param name="pluginId"></param>
    /// <param name="notificationId"></param>
    /// <param name="sid"></param>
    public NotificationParameters(Guid pluginId, Guid notificationId, string sid = null)
    {
        Requires.NotEmpty(pluginId, nameof(pluginId));
        Requires.NotEmpty(notificationId, nameof(notificationId));

        PluginId = pluginId;
        NotificationId = notificationId;
        Sid = sid;
    }
}