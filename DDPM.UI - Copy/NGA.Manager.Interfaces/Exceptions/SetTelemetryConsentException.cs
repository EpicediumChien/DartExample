#region LicenceHeader

//
// Copyright © 2024, Dell Inc., All Rights Reserved.
// This material is confidential and a trade secret.  Permission to use this
// work for any purpose must be obtained in writing from Dell Inc.
//

#endregion

using System.Runtime.Serialization;

namespace NGA.Manager.Interfaces;

/// <summary>
/// SetTelemetryConsentException class
/// </summary>
[Serializable]
public class SetTelemetryConsentException : Exception
{
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="message"></param>
    public SetTelemetryConsentException(string message) : base(message)
    {
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="message"></param>
    /// <param name="innerException"></param>
    public SetTelemetryConsentException(string message, Exception innerException) : base(message, innerException)
    {
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="info"></param>
    /// <param name="context"></param>
    protected SetTelemetryConsentException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}