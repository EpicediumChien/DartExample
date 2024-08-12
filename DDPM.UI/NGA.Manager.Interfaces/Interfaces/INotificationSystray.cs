#region LicenceHeader
//
// Copyright © 2022, Dell Inc., All Rights Reserved.
// This material is confidential and a trade secret.  Permission to use this
// work for any purpose must be obtained in writing from Dell Inc.
//
#endregion

using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Dell.Client.Framework.Common;

namespace NGA.Manager.Interfaces;

/// <summary>
/// INotificationSystray: Used by NGA.Systray.NotificationPlugin to communicate with NGA.NotificationPlugin
/// </summary>
public interface INotificationSystray : IFrameworkPlugin
{
    /// <summary>
    /// NewNotificationOperation: New NotificationOperation event
    /// NGA.Systray.NotificationPlugin should register to listen to this event at
    /// startup and then call GetNextNotificationOperationAsync().
    /// </summary>
    event EventHandler<NewNotificationOperationEventArgs> NewNotificationOperation;

    /// <summary>
    /// GetNextNotificationOperationAsync: Get next NotificationOperation for sid.
    /// A null is returned if a NotificationOperation is NOT available.
    /// 1) NGA.Systray.NotificationPlugin should call GetNextNotificationOperationAsync()
    ///    at startup (one or more times until a null is returned).
    /// 2) NGA.Systray.NotificationPlugin should call GetNextNotificationOperationAsync()
    ///    after receiving an event notification.
    /// Prior to performing an UpdateOperation or RemoveOperation, NGA.Systray.NotificationPlugin
    /// should make verify that a notification with the notificationId exists in the action center.
    /// </summary>
    /// <param name="sid">SecurityIdentifier for the user</param>
    /// <param name="locale">Current locale language</param>
    /// <param name="cancellationToken">cancellation token</param>
    /// <returns>Next notification operation or null if none exist</returns>
    /// <exception cref="NotificationPluginException"></exception>
    /// <exception cref="OperationCanceledException"></exception>
    Task<NotificationOperation> GetNextNotificationOperationAsync(string sid, CultureInfo locale,
        CancellationToken cancellationToken);

    /// <summary>
    /// NotificationActivatedAsync is called when the user clicks on a notification.
    /// NGA.Systray.NotificationPlugin should call NotificationActivatedAsync when the
    /// user clicks on a notification.
    /// </summary>
    /// <param name="userAction">User action</param>
    /// <param name="cancellationToken">cancellation token</param>
    /// <exception cref="NotificationPluginException"></exception>
    /// <exception cref="OperationCanceledException"></exception>
    Task NotificationActivatedAsync(UserAction userAction, CancellationToken cancellationToken);

    /// <summary>
    /// NotificationOperationAcknowledgedAsync should be called by Systray.NotificatonPlugin
    /// to report the result of an operation.
    /// 1) Return Success if the notification operation is successful.
    /// 2) Return Failure if the notification operation is unsuccessful. For example,
    ///    if the notificationId is not found for a UpdateOperation or RemoveOperation.
    /// </summary>
    /// <param name="operationResult">Operation result</param>
    /// <param name="cancellationToken">cancellation token</param>
    /// <exception cref="NotificationPluginException"></exception>
    /// <exception cref="OperationCanceledException"></exception>
    Task NotificationOperationAcknowledgedAsync(OperationResult operationResult, 
        CancellationToken cancellationToken);
}