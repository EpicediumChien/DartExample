#region LicenceHeader
//
// Copyright © 2022, Dell Inc., All Rights Reserved.
// This material is confidential and a trade secret.  Permission to use this
// work for any purpose must be obtained in writing from Dell Inc.
//
#endregion

using System.Threading;
using System.Threading.Tasks;
using Dell.Client.Framework.Common;

namespace NGA.Manager.Interfaces;

/// <summary>
/// INotificationResponse: Implemented by product Subagents to receive notification responses
/// </summary>
public interface INotificationResponse : IFrameworkPlugin
{
    /// <summary>
    /// NotificationActivated: Notification activated (from user interaction) by NGA.Systray.NotificationPlugin 
    /// </summary>
    /// <param name="userAction">User action</param>
    /// <param name="cancellationToken">cancellation token</param>
    /// <exception cref="OperationCanceledException"></exception>
    ValueTask NotificationActivated(UserAction userAction, CancellationToken cancellationToken);

    /// <summary>
    /// NotificationOperationAcknowledged: Notification operation acknowledged by NGA.Systray.NotificationPlugin
    /// </summary>
    /// <param name="operationResult">Operation result</param>
    /// <param name="cancellationToken">cancellation token</param>
    /// <exception cref="OperationCanceledException"></exception>
    ValueTask NotificationOperationAcknowledged(OperationResult operationResult, CancellationToken cancellationToken);
}