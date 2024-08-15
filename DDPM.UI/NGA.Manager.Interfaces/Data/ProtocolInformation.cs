#region LicenceHeader

//
// Copyright © 2023, Dell Inc., All Rights Reserved.
// This material is confidential and a trade secret.  Permission to use this
// work for any purpose must be obtained in writing from Dell Inc.
//

#endregion

using Newtonsoft.Json;

namespace NGA.Manager.Interfaces;

/// <summary>
/// Protocol Information Class
/// </summary>
public class ProtocolInformation
{
    #region Properties

    /// <summary>
    /// It will contain the physical protocol URI registered with application.
    /// </summary>
    [JsonProperty]
    public string ProtocolUri { get; set; }

    /// <summary>
    /// Supported Protocol Version
    /// </summary>
    [JsonProperty]
    public ProtocolVersion SupportedProtocolVersion { get; set; }

    /// <summary>
    /// Default Application Name
    /// </summary>
    [JsonProperty]
    public string DefaultApplicationName { get; set; }

    /// <summary>
    /// Localized Application Names
    /// </summary>
    [JsonProperty]
    public Dictionary<string, string> LocalizedApplicationNames { get; set; } = new();

    #endregion
}