#region LicenceHeader

//
// Copyright © 2022, Dell Inc., All Rights Reserved.
// This material is confidential and a trade secret.  Permission to use this
// work for any purpose must be obtained in writing from Dell Inc.
//

#endregion

using Newtonsoft.Json;

namespace NGA.Manager.Interfaces;

/// <summary>
/// NotificationDuration
/// </summary>
public class NotificationDuration
{
    #region Properties

    /// <summary>
    /// DisplayDuration
    /// </summary>
    [JsonProperty]
    public DateTimeOffset? DisplayDuration { get; protected set; }

    /// <summary>
    /// CacheDuration
    /// </summary>
    [JsonProperty]
    public DateTimeOffset CacheDuration { get; protected set; }

    #endregion

    /// <summary>
    /// NotificationDuration
    /// </summary>
    [JsonConstructor]
    protected NotificationDuration()
    {
    }

    /// <summary>
    /// NotificationDuration constructor for custom popup
    /// </summary>
    /// <param name="cacheDuration">cache display duration time for custom popup. Default is 3 days and maximum is 3 days</param>
    public NotificationDuration(DateTimeOffset? cacheDuration) : this(null, cacheDuration)
    {
    }

    /// <summary>
    /// NotificationDuration constructor for Windows Action Center (WAC) notification
    /// </summary>
    /// <param name="displayDuration">notification display duration time for WAC notification.
    /// This specifies how long WAC notification will be available in the user's notification tray. Default is 3 days and maximum is 3 days</param>
    /// <param name="cacheDuration">cache display duration time for WAC notification. Default is 3 days and maximum is 3 days</param>
    public NotificationDuration(DateTimeOffset? displayDuration, DateTimeOffset? cacheDuration)
    {
        DisplayDuration = displayDuration;
        CacheDuration = UpdateDuration(cacheDuration);
    }

    #region Private methods

    /// <summary>
    /// UpdateDuration
    /// If value is null or greater than DateTimeOffset.Now.AddDays(3), returns DateTimeOffset.Now.AddDays(3).
    /// Otherwise, returns the original value.
    /// </summary>
    private static DateTimeOffset UpdateDuration(DateTimeOffset? duration)
    {
        return
            duration == null || (DateTimeOffset)duration > DateTimeOffset.Now.AddDays(3) ?
            DateTimeOffset.Now.AddDays(3) :
            (DateTimeOffset)duration;
    }

    #endregion
}