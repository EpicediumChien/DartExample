#region LicenceHeader
//
// Copyright © 2022, Dell Inc., All Rights Reserved.
// This material is confidential and a trade secret.  Permission to use this
// work for any purpose must be obtained in writing from Dell Inc.
//
#endregion

using Microsoft;
using Newtonsoft.Json;

namespace NGA.Manager.Interfaces;

/// <summary>
/// NewProgressBarParameters
/// </summary>
public class NewProgressBarParameters
{
    #region Properties

    /// <summary>
    /// BodyParameters
    /// </summary>
    [JsonProperty]
    public BodyParameters BodyParameters { get; protected set; }

    /// <summary>
    /// Progress
    /// </summary>
    [JsonProperty]
    public double Progress { get; protected set; }

    /// <summary>
    /// Status
    /// </summary>
    [JsonProperty]
    public string Status { get; protected set; }

    /// <summary>
    /// ValueStringOverride
    /// </summary>
    [JsonProperty]
    public string ValueStringOverride { get; protected set; }

    #endregion

    /// <summary>
    /// NewProgressBarParameters
    /// </summary>
    [JsonConstructor]
    protected NewProgressBarParameters()
    {
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="bodyParameters">BodyParameters</param>
    /// <param name="progress">progress value</param>
    /// <param name="status">progress status text</param>
    /// <param name="valueStringOverride">text progress value (to override progress value)</param>
    public NewProgressBarParameters(BodyParameters bodyParameters, double progress, string status, string valueStringOverride = null)
    {
        Requires.NotNull(bodyParameters, nameof(bodyParameters));
        Requires.NotNullOrEmpty(status, nameof(status));

        BodyParameters = bodyParameters;
        Progress = progress;
        Status = status;
        ValueStringOverride = valueStringOverride;
    }
}