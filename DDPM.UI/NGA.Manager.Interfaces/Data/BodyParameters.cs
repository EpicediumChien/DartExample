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
/// BodyParameters
/// </summary>
public class BodyParameters : IBodyParameters
{
    #region Properties

    /// <summary>
    /// Title
    /// </summary>
    [JsonProperty]
    public string Title { get; protected set; }

    /// <summary>
    /// Body
    /// </summary>
    [JsonProperty]
    public string Body { get; protected set; }

    /// <summary>
    /// Contains the Arguments when the user clicks
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
    /// BodyParameters for Json
    /// </summary>
    [JsonConstructor]
    protected BodyParameters()
    {
    }

    /// <summary>
    /// Constructor that supports arguments
    /// </summary>
    /// <param name="title">title</param>
    /// <param name="body">body</param>
    /// <param name="arguments">key/value arguments</param>
    public BodyParameters(string title, string body, Dictionary<string, string> arguments = null)
    {
        Requires.NotNullOrWhiteSpace(title, nameof(title));
        Requires.NotNullOrWhiteSpace(body, nameof(body));

        Title = title;
        Body = body;
        Arguments = arguments ?? new Dictionary<string, string>();
    }

    /// <summary>
    /// Constructor that supports protocolUri
    /// </summary>
    /// <param name="title">title</param>
    /// <param name="body">body</param>
    /// <param name="protocolUri">the protocol uri to run when pressed</param>
    public BodyParameters(string title, string body, Uri protocolUri)
    {
        Requires.NotNullOrWhiteSpace(title, nameof(title));
        Requires.NotNullOrWhiteSpace(body, nameof(body));
        Requires.NotNull(protocolUri, nameof(protocolUri));

        Title = title;
        Body = body;
        ProtocolUri = protocolUri;
    }
}