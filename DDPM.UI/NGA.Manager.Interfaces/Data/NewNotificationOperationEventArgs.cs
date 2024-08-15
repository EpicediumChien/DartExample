#region LicenceHeader

//
// Copyright © 2022, Dell Inc., All Rights Reserved.
// This material is confidential and a trade secret.  Permission to use this
// work for any purpose must be obtained in writing from Dell Inc.
//

#endregion

namespace NGA.Manager.Interfaces;

/// <summary>
/// NewNotificationOperationEventArgs: EventArgs for NewNotificationOperation event
/// </summary>
public class NewNotificationOperationEventArgs : EventArgs
{
    #region Properties

    /// <summary>
    /// IsForAllActiveSessions: Is for all active sessions
    /// </summary>
    public bool IsForAllActiveSessions { get; }

    /// <summary>
    /// SessionId: SessionId to be notified if IsForAllActiveSessions is false
    /// </summary>
    public int? SessionId { get; }

    #endregion

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="isForAllActiveSessions">True if for all sessions</param>
    /// <param name="sessionId">session id</param>
    public NewNotificationOperationEventArgs(bool isForAllActiveSessions = true, int? sessionId = null)
    {
        IsForAllActiveSessions = isForAllActiveSessions;
        SessionId = sessionId;
    }
}