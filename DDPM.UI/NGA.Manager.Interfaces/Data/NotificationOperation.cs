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
/// NotificationOperation
/// </summary>
public class NotificationOperation : INotificationOperation
{
    #region Properties

    /// <summary>
    /// NotificationSourceType
    /// </summary>
    [JsonProperty]
    public NotificationSourceType NotificationSourceType { get; protected set; }

    /// <summary>}
    /// NotificationOperationType
    /// </summary>
    [JsonProperty]
    public NotificationOperationType NotificationOperationType { get; protected set; }

    /// <summary>
    /// NotificationParameters
    /// </summary>
    [JsonProperty]
    public NotificationParameters NotificationParameters { get; protected set; }

    /// <summary>
    /// BodyParameter
    /// </summary>
    [JsonProperty]
    public BodyParameters BodyParameter { get; protected set; }

    /// <summary>
    /// Progress
    /// </summary>
    [JsonProperty]
    public double? Progress { get; protected set; }

    /// <summary>
    /// Status
    /// </summary>
    [JsonProperty]
    public string Status { get; protected set; }

    /// <summary>
    /// Button1Parameter
    /// </summary>
    [JsonProperty]
    public ButtonParameters Button1Parameter { get; protected set; }

    /// <summary>
    /// Button2Parameter
    /// </summary>
    [JsonProperty]
    public ButtonParameters Button2Parameter { get; protected set; }

    /// <summary>
    /// ValueStringOverride
    /// </summary>
    [JsonProperty]
    public string ValueStringOverride { get; protected set; }

    /// <summary>
    /// XmlPayload
    /// </summary>
    [JsonProperty]
    public string XmlPayload { get; protected set; }

    /// <summary>
    /// BindValues
    /// </summary>
    [JsonProperty]
    public Dictionary<string, string> BindValues { get; protected set; }

    /// <summary>
    /// NotificationDuration
    /// </summary>
    [JsonProperty]
    public NotificationDuration NotificationDuration { get; protected set; }

    /// <summary>
    /// CreateDateTime
    /// </summary>
    [JsonProperty]
    public DateTime CreateDateTime { get; protected set; }

    /// <summary>
    /// SysTrayPluginId
    /// </summary>
    [JsonProperty]
    public Guid? SysTrayPluginId { get; protected set; }

    /// <summary>
    /// CustomNotification
    /// </summary>
    [JsonProperty]
    public string CustomNotification { get; protected set; }

    /// <summary>
    /// NotificationType
    /// </summary>
    [JsonProperty]
    public NotificationType NotificationType { get; protected set; }

    #endregion

    /// <summary>
    /// NotificationOperation
    /// </summary>
    [JsonConstructor]
    protected NotificationOperation()
    {
    }

    /// <summary>
    /// Constructor for NewNotificationAsync
    /// </summary>
    /// <param name="notificationParameters">NotificationParameters</param>
    /// <param name="bodyParameters">BodyParameters</param>    
    /// <param name="notificationDuration">notification duration</param>
    /// <param name="createDateTime">Create time</param>
    public NotificationOperation(NotificationParameters notificationParameters, BodyParameters bodyParameters,
        NotificationDuration notificationDuration, DateTime? createDateTime = null)
    {        
        Requires.NotNull(bodyParameters, nameof(bodyParameters));
        BodyParameter = bodyParameters;

        InitializeNewOperation(notificationParameters, notificationDuration, createDateTime);
        InitializeSourceTypeAndOperationType(NotificationSourceType.NewNotification, NotificationOperationType.NewOperation);
    }

    /// <summary> 
    /// Constructor for NewNotificationWithButtonsAsync
    /// </summary>
    /// <param name="notificationParameters">NotificationParameters</param>
    /// <param name="parameters">NewNotificationWithButtonsParameters</param>
    /// <param name="notificationDuration">notification duration</param>
    /// <param name="createDateTime">Create time</param>
    public NotificationOperation(NotificationParameters notificationParameters, NewNotificationWithButtonsParameters parameters,
        NotificationDuration notificationDuration, DateTime? createDateTime = null)
    {        
        Requires.NotNull(parameters, nameof(parameters));
        BodyParameter = parameters.BodyParameters;
        Button1Parameter = parameters.ButtonParameters[0];
        Button2Parameter = parameters.ButtonParameters[1];

        InitializeNewOperation(notificationParameters, notificationDuration, createDateTime);
        InitializeSourceTypeAndOperationType(NotificationSourceType.NewNotificationWithButtons, NotificationOperationType.NewOperation);
    }

    /// <summary>
    /// Constructor for NewProgressBarNotificationAsync
    /// </summary>
    /// <param name="notificationParameters">NotificationParameters</param>
    /// <param name="parameters">NewProgressBarParameters</param>
    /// <param name="notificationDuration">notification duration</param>
    /// <param name="createDateTime">Create time</param>
    public NotificationOperation(NotificationParameters notificationParameters, NewProgressBarParameters parameters,
        NotificationDuration notificationDuration, DateTime? createDateTime = null)
    {        
        Requires.NotNull(parameters, nameof(parameters));
        BodyParameter = parameters.BodyParameters;
        Progress = parameters.Progress;
        Status = parameters.Status;
        ValueStringOverride = parameters.ValueStringOverride;

        InitializeNewOperation(notificationParameters, notificationDuration, createDateTime);
        InitializeSourceTypeAndOperationType(NotificationSourceType.NewProgressBarNotification, NotificationOperationType.NewOperation);
    }

    /// <summary>
    /// Constructor for NewProgressBarNotificationWithButtonsAsync
    /// </summary>
    /// <param name="notificationParameters">NotificationParameters</param>
    /// <param name="parameters">NewProgressBarWithButtonsParameters</param>
    /// <param name="notificationDuration">notification duration</param>
    /// <param name="createDateTime">Create time</param>
    public NotificationOperation(NotificationParameters notificationParameters, NewProgressBarWithButtonsParameters parameters,
        NotificationDuration notificationDuration, DateTime? createDateTime = null)
    {        
        Requires.NotNull(parameters, nameof(parameters));
        BodyParameter = parameters.BodyParameters;
        Progress = parameters.Progress;
        Status = parameters.Status;
        Button1Parameter = parameters.ButtonParameters[0];
        Button2Parameter = parameters.ButtonParameters[1];
        ValueStringOverride = parameters.ValueStringOverride;

        InitializeNewOperation(notificationParameters, notificationDuration, createDateTime);
        InitializeSourceTypeAndOperationType(NotificationSourceType.NewProgressBarNotificationWithButtons, NotificationOperationType.NewOperation);
    }

    /// <summary>
    /// Constructor for UpdateProgressBarNotificationAsync
    /// </summary>
    /// <param name="notificationParameters">NotificationParameters</param>
    /// <param name="parameters">UpdateProgressBarParameters</param>
    /// <param name="createDateTime">Create time</param>
    public NotificationOperation(NotificationParameters notificationParameters, UpdateProgressBarParameters parameters,
        DateTime? createDateTime = null)
    {        
        Requires.NotNull(parameters, nameof(parameters));
        Progress = parameters.Progress;
        ValueStringOverride = parameters.ValueStringOverride;

        InitializeOperation(notificationParameters, createDateTime);
        InitializeSourceTypeAndOperationType(NotificationSourceType.UpdateProgressBarNotification, NotificationOperationType.UpdateOperation);
    }

    /// <summary>
    /// Constructor for NewNotificationFromMetaDataAsync
    /// </summary>
    /// <param name="notificationParameters">NotificationParameters</param>
    /// <param name="xmlPayload">XML Payload</param>
    /// <param name="notificationDuration">notification duration</param>
    /// <param name="bindValues">Dictionary of key/value pairs for update values</param>
    /// <param name="createDateTime">Create time</param>
    public NotificationOperation(NotificationParameters notificationParameters, string xmlPayload,
        NotificationDuration notificationDuration, Dictionary<string, string> bindValues = null, 
        DateTime? createDateTime = null)
    {        
        Requires.NotNullOrWhiteSpace(xmlPayload, nameof(xmlPayload));
        XmlPayload = xmlPayload;
        BindValues = bindValues;

        InitializeNewOperation(notificationParameters, notificationDuration, createDateTime);
        InitializeSourceTypeAndOperationType(NotificationSourceType.NewNotificationFromMetaData, NotificationOperationType.NewOperation);
    }

    /// <summary>
    /// Constructor for UpdateNotificationFromMetaDataAsync
    /// </summary>
    /// <param name="notificationParameters">NotificationParameters</param>
    /// <param name="bindValues">Dictionary of key/value pairs for bind values</param>
    /// <param name="createDateTime">Create time</param>
    public NotificationOperation(NotificationParameters notificationParameters, Dictionary<string, string> bindValues,
        DateTime? createDateTime = null)
    {        
        Requires.NotNull(bindValues, nameof(bindValues));
        BindValues = bindValues;

        InitializeOperation(notificationParameters, createDateTime);
        InitializeSourceTypeAndOperationType(NotificationSourceType.UpdateFromMetaData, NotificationOperationType.UpdateOperation);
    }

    /// <summary>
    /// Constructor for RemoveNotificationAsync or RemoveCustomPopupAsync
    /// </summary>
    /// <param name="notificationParameters">NotificationParameters</param>
    /// <param name="notificationType"></param>
    /// <param name="createDateTime">Create time</param>
    public NotificationOperation(NotificationParameters notificationParameters, 
        NotificationType notificationType = NotificationType.ActionCenter,
        DateTime? createDateTime = null)
    {
        NotificationSourceType = notificationType switch
        {
            NotificationType.ActionCenter => NotificationSourceType.RemoveNotification,
            NotificationType.PopUp => NotificationSourceType.RemoveCustomPopup,
            _ => throw new ArgumentException($"NotificationType is invalid: {notificationType}"),
        };

        InitializeOperation(notificationParameters, createDateTime);
        InitializeTypesOperationTypeAndNotificationType(NotificationOperationType.RemoveOperation, notificationType);
    }

    /// <summary>
    /// Constructor for NewCustomNotificationAsync or NewCustomPopupAsync
    /// </summary>
    /// <param name="notificationParameters">NotificationParameters</param>
    /// <param name="sysTrayPluginId">Systray PluginId</param>
    /// <param name="customNotification">Custom string</param>    
    /// <param name="notificationDuration">notification duration</param>
    /// <param name="notificationType">Notification type</param>
    /// <param name="createDateTime">Create time</param>
    public NotificationOperation(NotificationParameters notificationParameters, Guid sysTrayPluginId, string customNotification,
        NotificationDuration notificationDuration,
        NotificationType notificationType = NotificationType.ActionCenter, DateTime? createDateTime = null)
    {
        NotificationSourceType = notificationType switch
        {
            NotificationType.ActionCenter => NotificationSourceType.NewCustomNotification,
            NotificationType.PopUp => NotificationSourceType.NewCustomPopup,
            _ => throw new ArgumentException($"NotificationType is invalid: {notificationType}"),
        };

        InitializeNewOperation(notificationParameters, notificationDuration, createDateTime);
        InitializeCustomOperation(NotificationOperationType.NewOperation, notificationType, sysTrayPluginId, customNotification);
    }

    /// <summary>
    /// Constructor for UpdateCustomNotificationAsync or UpdateCustomPopupAsync
    /// </summary>
    /// <param name="notificationParameters">NotificationParameters</param>
    /// <param name="sysTrayPluginId">Systray PluginId</param>
    /// <param name="customNotification">Custom string</param>    
    /// <param name="notificationType">Notification type</param>
    /// <param name="createDateTime">Create time</param>
    public NotificationOperation(NotificationParameters notificationParameters, Guid sysTrayPluginId, string customNotification,
        NotificationType notificationType = NotificationType.ActionCenter, DateTime? createDateTime = null)
    {
        NotificationSourceType = notificationType switch
        {
            NotificationType.ActionCenter => NotificationSourceType.UpdateCustomNotification,
            NotificationType.PopUp => NotificationSourceType.UpdateCustomPopup,
            _ => throw new ArgumentException($"NotificationType is invalid: {notificationType}"),
        };

        InitializeOperation(notificationParameters, createDateTime);
        InitializeCustomOperation(NotificationOperationType.UpdateOperation, notificationType, sysTrayPluginId, customNotification);
    }

    #region Private methods

    private void InitializeNewOperation(NotificationParameters notificationParameters, NotificationDuration notificationDuration, 
        DateTime? createDateTime)
    {        
        InitializeOperation(notificationParameters, createDateTime);
        Requires.NotNull(notificationDuration, nameof(notificationDuration));

        NotificationDuration = notificationDuration;
    }

    private void InitializeOperation(NotificationParameters notificationParameters, DateTime? createDateTime)
    {
        Requires.NotNull(notificationParameters, nameof(notificationParameters));

        if (createDateTime != null)
            Requires.NotDefault((DateTime)createDateTime, nameof(createDateTime));

        NotificationParameters = notificationParameters;
        CreateDateTime = createDateTime ?? DateTime.Now;
    }

    private void InitializeCustomOperation(NotificationOperationType notificationOperationType, NotificationType notificationType, 
        Guid sysTrayPluginId, string customNotification)
    {
        NotificationOperationType = notificationOperationType;
        NotificationType = notificationType;

        Requires.NotEmpty(sysTrayPluginId, nameof(sysTrayPluginId));
        Requires.NotNullOrWhiteSpace(customNotification, nameof(customNotification));

        SysTrayPluginId = sysTrayPluginId;
        CustomNotification = customNotification;
    }

    private void InitializeSourceTypeAndOperationType(NotificationSourceType notificationSourceType, NotificationOperationType notificationOperationType)
    {
        NotificationSourceType = notificationSourceType;
        InitializeTypesOperationTypeAndNotificationType(notificationOperationType, NotificationType.ActionCenter);
    }

    private void InitializeTypesOperationTypeAndNotificationType(NotificationOperationType notificationOperationType, NotificationType notificationType)
    {
        NotificationOperationType = notificationOperationType;
        NotificationType = notificationType;
    }

    #endregion
}