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
/// IBodyParameters
/// </summary>
public interface IBodyParameters
{
    /// <summary>
    /// Title
    /// </summary>
    string Title { get; }

    /// <summary>
    /// ButtonAction
    /// </summary>
    string Body { get; }

    /// <summary>
    /// Contains the Arguments when the user clicks the button
    /// </summary>
    Dictionary<string, string> Arguments { get; }

    /// <summary>
    /// Contains the Protocol Uri used to launch external applications
    /// </summary>
    Uri ProtocolUri { get; }
}