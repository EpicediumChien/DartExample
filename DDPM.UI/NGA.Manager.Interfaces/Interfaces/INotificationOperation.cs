#region LicenceHeader
//
// Copyright © 2022, Dell Inc., All Rights Reserved.
// This material is confidential and a trade secret.  Permission to use this
// work for any purpose must be obtained in writing from Dell Inc.
//
#endregion

using System;
using System.Collections.Generic;

namespace NGA.Manager.Interfaces;

/// <summary>
/// NotificationSourceType
/// </summary>
public enum NotificationSourceType
{
    /// <summary>
    /// NewNotificationAsync
    /// </summary>
    NewNotification = 1,

    /// <summary>
    /// NewNotificationWithButtonsAsync
    /// </summary>
    NewNotificationWithButtons = 2,

    /// <summary>
    /// NewProgressBarNotificationAsync
    /// </summary>
    NewProgressBarNotification = 3,

    /// <summary>
    /// NewProgressBarNotificationWithButtonsAsync
    /// </summary>
    NewProgressBarNotificationWithButtons = 4,

    /// <summary>
    /// UpdateProgressBarNotificationAsync
    /// </summary>
    UpdateProgressBarNotification = 5,

    /// <summary>
    /// RemoveNotificationAsync
    /// </summary>
    RemoveNotification = 6,

    /// <summary>
    /// NewNotificationFromMetaDataAsync
    /// </summary>
    NewNotificationFromMetaData = 7,

    /// <summary>
    /// UpdateFromMetaData
    /// </summary>
    UpdateFromMetaData = 8,

    /// <summary>
    /// NewCustomNotificationAsync
    /// </summary>
    NewCustomNotification = 9,

    /// <summary>
    /// UpdateCustomNotificationAsync
    /// </summary>
    UpdateCustomNotification = 10,

    /// <summary>
    /// NewCustomPopupAsync
    /// </summary>
    NewCustomPopup = 11,

    /// <summary>
    /// UpdateCustomPopupAsync
    /// </summary>
    UpdateCustomPopup = 12,

    /// <summary>
    /// RemoveCustomPopupAsync
    /// </summary>
    RemoveCustomPopup = 13
}

/// <summary>
/// INotificationOperation
/// </summary>
public interface INotificationOperation
{
    /// <summary>
    /// NotificationSourceType
    /// </summary>
    NotificationSourceType NotificationSourceType { get; }

    /// <summary>}
    /// NotificationOperationType
    /// </summary>
    NotificationOperationType NotificationOperationType { get; }

    /// <summary>
    /// NotificationParameters
    /// </summary>
    public NotificationParameters NotificationParameters { get; }

    /// <summary>
    /// BodyParameter
    /// </summary>
    BodyParameters BodyParameter { get; }

    /// <summary>
    /// Progress
    /// </summary>
    double? Progress { get; }

    /// <summary>
    /// Status
    /// </summary>
    string Status { get; }

    /// <summary>
    /// Button1Parameter
    /// </summary>
    ButtonParameters Button1Parameter { get; }

    /// <summary>
    /// Button2Parameter
    /// </summary>
    ButtonParameters Button2Parameter { get; }

    /// <summary>
    /// ValueStringOverride
    /// </summary>
    string ValueStringOverride { get; }

    /// <summary>
    /// XML Payload
    /// </summary>
    string XmlPayload { get; }

    /// <summary>
    /// Update 
    /// </summary>
    Dictionary<string, string> BindValues { get; }

    /// <summary>
    /// NotificationDuration
    /// </summary>
    NotificationDuration NotificationDuration { get; }

    /// <summary>
    /// CreateDateTime
    /// </summary>
    DateTime CreateDateTime { get; }

    /// <summary>
    /// SysTrayPluginId
    /// </summary>
    Guid? SysTrayPluginId { get; }

    /// <summary>
    /// CustomNotification
    /// </summary>
    string CustomNotification { get; }
}