#region LicenceHeader

//
// Copyright © 2022, Dell Inc., All Rights Reserved.
// This material is confidential and a trade secret.  Permission to use this
// work for any purpose must be obtained in writing from Dell Inc.
//

#endregion

using Microsoft;
using Newtonsoft.Json;

namespace NGA.Manager.Interfaces;

/// <summary>
/// UserAction
/// </summary>
public class UserAction : IUserAction
{
    #region Properties

    /// <inheritdoc/>
    [JsonProperty]
    public Guid PluginId { get; protected set; }

    /// <inheritdoc/>
    [JsonProperty]
    public Guid NotificationId { get; protected set; }

    /// <inheritdoc/>
    [JsonProperty]
    public Dictionary<string, string> ActivationData { get; protected set; }

    /// <inheritdoc/>
    [JsonProperty]
    public Dictionary<string, string> UserData { get; protected set; }

    /// <inheritdoc/>
    [JsonProperty]
    public string Sid { get; set; }

    /// <inheritdoc/>
    [JsonProperty]
    public DateTime CreateDateTime { get; protected set; }

    /// <inheritdoc/>
    [JsonProperty]
    public Guid Guid { get; protected set; }

    #endregion

    /// <summary>
    /// UserAction
    /// </summary>
    [JsonConstructor]
    protected UserAction()
    {
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="pluginId">plugin id for INotificationResponse</param>
    /// <param name="notificationId">Guid</param>
    /// <param name="sid">SecurityIdentifier for the user</param>
    /// <param name="activationData">string to string Dictionary</param>
    /// <param name="userData">User selection data</param>
    /// <param name="createDateTime">Create time for the object</param>
    /// <param name="guid">Guid for the object</param>
    public UserAction(Guid pluginId, Guid notificationId, string sid, Dictionary<string, string> activationData, Dictionary<string, string> userData = null,
        DateTime? createDateTime = null, Guid? guid = null)
    {
        Requires.NotEmpty(pluginId, nameof(pluginId));
        Requires.NotEmpty(notificationId, nameof(notificationId));
        Requires.NotNull(activationData, nameof(activationData));

        if (createDateTime != null)
            Requires.NotDefault((DateTime)createDateTime, nameof(createDateTime));

        if (guid != null)
            Requires.NotEmpty((Guid)guid, nameof(guid));

        PluginId = pluginId;
        NotificationId = notificationId;
        Sid = sid;
        ActivationData = activationData;
        UserData = userData ?? new Dictionary<string, string>();
        CreateDateTime = createDateTime ?? DateTime.Now;
        Guid = guid ?? Guid.NewGuid();
    }
}