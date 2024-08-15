#region LicenceHeader

//
// Copyright © 2022, Dell Inc., All Rights Reserved.
// This material is confidential and a trade secret.  Permission to use this
// work for any purpose must be obtained in writing from Dell Inc.
//

#endregion

namespace NGA.Manager.Interfaces;

/// <summary>
/// LastResonseType
/// </summary>
public enum LatestResponseType
{
    /// <summary>
    /// NewOperation
    /// </summary>
    NewAcknowledged = 1,

    /// <summary>
    /// UpdateOperation
    /// </summary>
    UpdateAcknowledged = 2,

    /// <summary>
    /// RemoveOperation
    /// </summary>
    RemoveAcknowledged = 3,

    /// <summary>
    /// UserActivated
    /// </summary>
    UserActivated = 4,
}

/// <summary>
/// NotificationOperationType
/// </summary>
public enum NotificationOperationType
{
    /// <summary>
    /// NewOperation
    /// </summary>
    NewOperation = 1,

    /// <summary>
    /// UpdateOperation
    /// </summary>
    UpdateOperation = 2,

    /// <summary>
    /// RemoveOperation
    /// </summary>
    RemoveOperation = 3,
}

/// <summary>
/// ResultType
/// </summary>
public enum ResultType
{
    /// <summary>
    /// Default value
    /// </summary>
    None = 0,

    /// <summary>
    /// Success: notification operation was performed successfully
    /// </summary>
    Success = 1,

    /// <summary>
    /// Failure: notification operation could not be perform
    /// </summary>
    Failure = 2,

    /// <summary>
    /// Notification operation could not be updated or removed because the notification could not be found
    /// </summary>
    FailureDueToNotificationNotBeingFound = 3,

    /// <summary>
    /// Notification operation could not be updated or removed because the custom notification plugin could not be found
    /// </summary>
    FailureDueToCustomNotificationPluginNotBeingFound = 4,

    /// <summary>
    /// Notification operation could not be updated or removed because the custom popup plugin could not be found
    /// </summary>
    FailureDueToCustomPopupPluginNotBeingFound = 5,

    /// <summary>
    /// Did not receive CustomToastData from CustomNotificationPlugin
    /// </summary>
    FailureDueToEmptyCustomToastData = 6,

    /// <summary>
    /// Received CustomToastData from CustomNotificationPlugin but the Payload is null
    /// </summary>
    FailureDueToEmptyPayload = 7,

    /// <summary>
    /// New Notification could not be displayed because display of notifications is disabled
    /// </summary>
    FailureDueToNotificationDisplayDisabled = 8,

    /// <summary>
    /// Did not receive CustomPopupMetaData from CustomPopupPlugin
    /// </summary>
    FailureDueToEmptyPopupMetaData = 9,

    /// <summary>
    /// Received CustomTPopupMetaData from CustomPopupPlugin but the CustomContent is null
    /// </summary>
    FailureDueToEmptyCustomContent = 10
}

/// <summary>
/// IOperationResult
/// </summary>
public interface IOperationResult
{
    /// <summary>
    /// PluginId
    /// </summary>
    Guid PluginId { get; }

    /// <summary>
    /// NotificationId
    /// </summary>
    Guid NotificationId { get; }

    /// <summary>
    /// NotificationOperationType
    /// </summary>
    NotificationOperationType NotificationOperationType { get; }

    /// <summary>
    /// ResultType
    /// </summary>
    ResultType ResultType { get; }

    /// <summary>
    /// SecurityIdentifier for the user
    /// </summary>
    string Sid { get; set; }

    /// <summary>
    /// CreateDateTime
    /// </summary>
    DateTime CreateDateTime { get; }

    /// <summary>
    /// Guid
    /// </summary>
    Guid Guid { get; }
}