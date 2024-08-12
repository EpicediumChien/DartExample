#region LicenceHeader
//
// Copyright © 2022, Dell Inc., All Rights Reserved.
// This material is confidential and a trade secret.  Permission to use this
// work for any purpose must be obtained in writing from Dell Inc.
//
#endregion

using System;
using System.Collections.Generic;
using Microsoft;
using Newtonsoft.Json;

namespace NGA.Manager.Interfaces;

/// <summary>
/// NewProgressBarParameters
/// </summary>
public class NewProgressBarWithButtonsParameters
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

    /// <summary>
    /// ButtonParameters
    /// </summary>
    [JsonProperty]
    public List<ButtonParameters> ButtonParameters { get; protected set; }

    #endregion

    /// <summary>
    /// NewProgressBarWithButtonsParameters
    /// </summary>
    [JsonConstructor]
    protected NewProgressBarWithButtonsParameters()
    {
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="bodyParameters">BodyParameters</param>
    /// <param name="progress">progress value</param>
    /// <param name="status">progress status text</param>
    /// <param name="buttonParameters">ButtonParameters</param>
    /// <param name="valueStringOverride">text progress value (to override progress value)</param>
    public NewProgressBarWithButtonsParameters(BodyParameters bodyParameters, List<ButtonParameters> buttonParameters, 
        double progress, string status, string valueStringOverride = null)
    {
        Requires.NotNull(bodyParameters, nameof(bodyParameters));
        Requires.NotNullOrEmpty(status, nameof(status));
        Requires.NotNull(buttonParameters, nameof(buttonParameters));

        if (buttonParameters.Count != 2)
            throw new ArgumentException("buttonParameters should have a count of 2");

        BodyParameters = bodyParameters;
        Progress = progress;
        Status = status;
        ValueStringOverride = valueStringOverride;
        ButtonParameters = buttonParameters;
    }
}