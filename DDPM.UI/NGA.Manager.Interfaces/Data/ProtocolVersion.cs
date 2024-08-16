#region LicenceHeader

//
// Copyright © 2023, Dell Inc., All Rights Reserved.
// This material is confidential and a trade secret.  Permission to use this
// work for any purpose must be obtained in writing from Dell Inc.
//

#endregion

namespace NGA.Manager.Interfaces;

/// <summary>
/// Protocol Version Enum
/// This is a version component, which helps in maintaining backwards compatibility.
/// </summary>
public enum ProtocolVersion
{
    /// <summary>
    /// Unknown
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// Version1
    /// </summary>
    Version1 = 1
}