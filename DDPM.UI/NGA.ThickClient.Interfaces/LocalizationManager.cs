#region LicenceHeader

//
// Copyright © 2023, Dell Inc., All Rights Reserved.
// This material is confidential and a trade secret.  Permission to use this
// work for any purpose must be obtained in writing from Dell Inc.
//

#endregion

namespace NGA.ThickClient.Interfaces;

/// <summary>
/// LocalizationManager allows the developer to add support for additional languages using Resources
/// </summary>
public class LocalizationManager
{
    /// <summary>
    /// LocalizationManager instance
    /// </summary>
    public static LocalizationManager? Instance { get; set; }

    /// <summary>
    /// ResourceManager with additional languages support
    /// </summary>
    public System.Resources.ResourceManager? ResourceManager { get; set; }
}