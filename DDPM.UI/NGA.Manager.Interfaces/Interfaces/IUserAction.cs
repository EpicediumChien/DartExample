#region LicenceHeader
//
// Copyright © 2022, Dell Inc., All Rights Reserved.
// This material is confidential and a trade secret.  Permission to use this
// work for any purpose must be obtained in writing from Dell Inc.
//
#endregion

using System;
using System.Collections.Generic;

namespace NGA.Manager.Interfaces;

/// <summary>
/// IUserAction
/// </summary>
public interface IUserAction
{
    /// <summary>
    /// PluginId
    /// </summary>
    Guid PluginId { get; }

    /// <summary>
    /// NotificationId
    /// </summary>
    Guid NotificationId { get; }

    /// <summary>
    /// ActivationData
    /// </summary>
    Dictionary<string, string> ActivationData { get; }

    /// <summary>
    /// UserData
    /// The Key portion of the data corresponds to the ID that was provided to Windows Action Center.
    /// The Value portion of the data is the JsonSerialized object provided by Windows Action Center.
    /// It is up to the consumer of this data to deserialize the data to whatever object they are expecting.
    /// This is done this way because Dell Tech Hub will not allow us to send type information from the unelevated
    /// pipe to the elevated pipe.
    /// </summary>
    Dictionary<string, string> UserData { get; }

    /// <summary>
    /// Sid
    /// </summary>
    string Sid { get; set; }

    /// <summary>
    /// CreateDateTime
    /// </summary>
    DateTime CreateDateTime { get; }

    /// <summary>
    /// Guid
    /// </summary>
    Guid Guid { get; }
}