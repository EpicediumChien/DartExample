#region LicenceHeader

//
// Copyright © 2022, Dell Inc., All Rights Reserved.
// This material is confidential and a trade secret.  Permission to use this
// work for any purpose must be obtained in writing from Dell Inc.
//

#endregion

namespace NGA.Manager.Interfaces;

/// <summary>
/// NotificationOperationExtension
/// </summary>
public static class NotificationOperationExtension
{
    /// <summary>
    /// IsExpiredNewOperation
    /// </summary>
    /// <param name="operation">NotificationOperation</param>
    /// <returns>True if NotificationOperation is a NewOperation and expired</returns>
    public static bool IsExpiredNewOperation(this INotificationOperation operation)
    {
        return operation.NotificationOperationType == NotificationOperationType.NewOperation &&
               operation.NotificationDuration != null &&
               operation.NotificationDuration.CacheDuration < DateTimeOffset.Now;
    }
}