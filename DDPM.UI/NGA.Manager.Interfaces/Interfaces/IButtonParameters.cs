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
/// IButtonParameters
/// </summary>
public interface IButtonParameters
{
    /// <summary>
    /// Title
    /// </summary>
    string Text { get; }

    /// <summary>
    /// ButtonAction
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Contains the Arguments when the user clicks the button
    /// </summary>
    Dictionary<string, string> Arguments { get; }

    /// <summary>
    /// Contains the Protocol Uri used to launch external applications
    /// </summary>
    Uri ProtocolUri { get; }
}