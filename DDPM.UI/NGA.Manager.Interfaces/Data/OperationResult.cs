#region LicenceHeader
//
// Copyright © 2022, Dell Inc., All Rights Reserved.
// This material is confidential and a trade secret.  Permission to use this
// work for any purpose must be obtained in writing from Dell Inc.
//
#endregion

using System;
using Microsoft;
using Newtonsoft.Json;

namespace NGA.Manager.Interfaces;

/// <summary>
/// OperationResult
/// </summary>
public sealed class OperationResult : IOperationResult
{
    #region Properties

    /// <summary>
    /// PluginId
    /// </summary>
    [JsonProperty]
    public Guid PluginId { get; private set; }

    /// <summary>
    /// NotificationId
    /// </summary>
    [JsonProperty]
    public Guid NotificationId { get; private set; }

    /// <summary>
    /// NotificationOperationType
    /// </summary>
    [JsonProperty]
    public NotificationOperationType NotificationOperationType { get; private set; }

    /// <summary>
    /// ResultType
    /// </summary>
    [JsonProperty]
    public ResultType ResultType { get; private set; }

    /// <summary>
    /// SecurityIdentifier for the user
    /// </summary>
    [JsonProperty]
    public string Sid { get; set; }

    /// <summary>
    /// CreateDateTime
    /// </summary>
    [JsonProperty]
    public DateTime CreateDateTime { get; private set; }

    /// <summary>
    /// Guid
    /// </summary>
    [JsonProperty]
    public Guid Guid { get; private set; }

    #endregion

    /// <summary>
    /// OperationResult
    /// </summary>
    [JsonConstructor]
    private OperationResult()
    {
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="pluginId">plugin id for INotificationResponse</param>
    /// <param name="notificationId">Guid</param>
    /// <param name="sid">SecurityIdentifier for the user</param>
    /// <param name="notificationOperationType">NotificationOperationType</param>
    /// <param name="resultType">ResultType</param>
    /// <param name="createDateTime">Create time for the object</param>
    /// <param name="guid">Guid for the object</param>
    public OperationResult(Guid pluginId, Guid notificationId, string sid, NotificationOperationType notificationOperationType, ResultType resultType,
        DateTime? createDateTime = null, Guid? guid = null)
    {
        Requires.NotEmpty(pluginId, nameof(pluginId));
        Requires.NotEmpty(notificationId, nameof(notificationId));

        if (createDateTime != null)
            Requires.NotDefault((DateTime)createDateTime, nameof(createDateTime));

        if (guid != null)
            Requires.NotEmpty((Guid)guid, nameof(guid));

        NotificationId = notificationId;
        Sid = sid;
        NotificationOperationType = notificationOperationType;
        ResultType = resultType;
        PluginId = pluginId;
        CreateDateTime = createDateTime ?? DateTime.Now;
        Guid = guid ?? Guid.NewGuid();
    }
}