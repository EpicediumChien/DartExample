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
/// ButtonParameters
/// </summary>
public class ButtonParameters : IButtonParameters
{
    #region Properties

    /// <summary>
    /// Title
    /// </summary>
    [JsonProperty]
    public string Text { get; protected set; }

    /// <summary>
    /// ButtonAction
    /// </summary>
    [JsonProperty]
    public string Name { get; protected set; }

    /// <summary>
    /// Contains the Arguments when the user clicks the button
    /// </summary>
    [JsonProperty]
    public Dictionary<string, string> Arguments { get; protected set; }

    /// <summary>
    /// Contains the Protocol Uri used to launch external applications
    /// </summary>
    [JsonProperty]
    public Uri ProtocolUri { get; protected set; }

    #endregion

    /// <summary>
    /// ButtonParameters for Json
    /// </summary>
    [JsonConstructor]
    protected ButtonParameters()
    {
    }

    /// <summary>
    /// Constructor that supports arguments
    /// </summary>
    /// <param name="text">button text</param>
    /// <param name="name">button name</param>
    /// <param name="arguments">key/value arguments</param>
    public ButtonParameters(string text, string name, Dictionary<string, string> arguments = null)
    {
        Requires.NotNullOrWhiteSpace(text, nameof(text));
        Requires.NotNullOrWhiteSpace(name, nameof(name));

        Text = text;
        Name = name;
        Arguments = arguments ?? new Dictionary<string, string>();
    }

    /// <summary>
    /// Constructor that supports protocolUri
    /// </summary>
    /// <param name="text">button text</param>
    /// <param name="protocolUri">the protocol uri to run when button is pressed</param>
    public ButtonParameters(string text, Uri protocolUri)
    {
        Requires.NotNullOrWhiteSpace(text, nameof(text));
        Requires.NotNull(protocolUri, nameof(protocolUri));

        Text = text;
        ProtocolUri = protocolUri;
    }
}