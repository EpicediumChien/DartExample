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
/// NewNotificationWithButtonsParameters
/// </summary>
public class NewNotificationWithButtonsParameters
{
    #region Properties

    /// <summary>
    /// BodyParameters
    /// </summary>
    [JsonProperty]
    public BodyParameters BodyParameters { get; protected set; }

    /// <summary>
    /// ButtonButtonParameters
    /// </summary>
    [JsonProperty]
    public List<ButtonParameters> ButtonParameters { get; protected set; }

    #endregion

    /// <summary>
    /// NewNotificationWithButtonsParameters
    /// </summary>
    [JsonConstructor]
    protected NewNotificationWithButtonsParameters()
    {
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="bodyParameters">BodyParameters</param>
    /// <param name="buttonParameters">ButtonParameters</param>
    public NewNotificationWithButtonsParameters(BodyParameters bodyParameters, List<ButtonParameters> buttonParameters)
    {
        Requires.NotNull(bodyParameters, nameof(bodyParameters));
        Requires.NotNull(buttonParameters, nameof(buttonParameters));

        if (buttonParameters.Count != 2)
            throw new ArgumentException("buttonParameters should have a count of 2");

        BodyParameters = bodyParameters;
        ButtonParameters = buttonParameters;
    }
}