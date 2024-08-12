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
/// UpdateProgressBarParameters
/// </summary>
public class UpdateProgressBarParameters
{
    #region Properties

    /// <summary>
    /// Progress
    /// </summary>
    [JsonProperty]
    public double Progress { get; protected set; }

    /// <summary>
    /// ValueStringOverride
    /// </summary>
    [JsonProperty]
    public string ValueStringOverride { get; protected set; }

    #endregion

    /// <summary>
    /// OperatioUpdateProgressBarParametersnResult
    /// </summary>
    [JsonConstructor]
    protected UpdateProgressBarParameters()
    {
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="progress">notification progress value</param>
    /// <param name="valueStringOverride"></param>
    public UpdateProgressBarParameters(double progress, string valueStringOverride = null)
    {
        Requires.NotDefault(progress, nameof(progress));

        Progress = progress;
        ValueStringOverride = valueStringOverride;
    }
}