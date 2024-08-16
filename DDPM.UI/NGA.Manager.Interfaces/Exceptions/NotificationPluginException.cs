#region LicenceHeader

//
// Copyright © 2022, Dell Inc., All Rights Reserved.
// This material is confidential and a trade secret.  Permission to use this
// work for any purpose must be obtained in writing from Dell Inc.
//

#endregion

using System.Runtime.Serialization;

namespace NGA.Manager.Interfaces;

/// <summary>
/// NotificationPluginException
/// </summary>
[Serializable]
public class NotificationPluginException : Exception
{
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="message">string?</param>
    public NotificationPluginException(string message) : base(message)
    {
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="message">string?</param>
    /// <param name="innerException">Exception?</param>
    public NotificationPluginException(string message, Exception innerException) : base(message, innerException)
    {
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="info"></param>
    /// <param name="context"></param>
    protected NotificationPluginException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}