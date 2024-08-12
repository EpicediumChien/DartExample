#region LicenceHeader
//
// Copyright © 2022, Dell Inc., All Rights Reserved.
// This material is confidential and a trade secret.  Permission to use this
// work for any purpose must be obtained in writing from Dell Inc.
//
#endregion

using System;
using System.Threading;
using System.Threading.Tasks;
using Dell.Client.Framework.Common;

namespace NGA.Manager.Interfaces;

/// <summary>
/// INotificationRequest: Used by product Subagents to submit notification operations to action center
/// </summary>
public interface INotificationRequest : IFrameworkPlugin
{
    /// <summary>
    /// NewNotificationAsync: New notification in action center (to all sids)
    /// </summary>
    /// <param name="pluginId">plug-in id for INotificationResponse</param>
    /// <param name="localeBodyParameters">LocaleBodyParameters</param>
    /// <param name="notificationDuration">notification duration</param>
    /// <param name="cancellationToken">cancellation token</param>
    /// <returns>notificationId Task</returns>
    /// <exception cref="NotificationPluginException"></exception>
    /// <exception cref="OperationCanceledException"></exception> 
    Task<Guid> NewNotificationForAllSidsAsync(Guid pluginId, LocaleBodyParameters localeBodyParameters,
        NotificationDuration notificationDuration, CancellationToken cancellationToken);

    /// <summary>
    /// NewNotificationAsync: New notification in action center
    /// </summary>
    /// <param name="pluginId">plug-in id for INotificationResponse</param>
    /// <param name="localeBodyParameters">LocaleBodyParameters</param>
    /// <param name="notificationDuration">notification duration</param>
    /// <param name="sid">SecurityIdentifier for the user (null for all sids)</param>
    /// <param name="cancellationToken">cancellation token</param>
    /// <returns>notificationId Task</returns>
    /// <exception cref="NotificationPluginException"></exception>
    /// <exception cref="OperationCanceledException"></exception> 
    Task<Guid> NewNotificationAsync(Guid pluginId, LocaleBodyParameters localeBodyParameters,
        NotificationDuration notificationDuration, string sid, CancellationToken cancellationToken);

    /// <summary>
    /// NewNotificationWithButtonsAsync: New notification with buttons in action center (to all sids)
    /// </summary>
    /// <param name="pluginId">plug-in id for INotificationResponse</param>
    /// <param name="localeNewNotificationWithButtonsParameters">LocaleNewNotificationWithButtonsParameters</param>
    /// <param name="notificationDuration">notification duration</param>
    /// <param name="cancellationToken">cancellation token</param>
    /// <returns>notificationId Task</returns>
    /// <exception cref="NotificationPluginException"></exception>
    /// <exception cref="OperationCanceledException"></exception>
    Task<Guid> NewNotificationWithButtonsForAllSidsAsync(Guid pluginId, LocaleNewNotificationWithButtonsParameters localeNewNotificationWithButtonsParameters,
        NotificationDuration notificationDuration, CancellationToken cancellationToken);

    /// <summary>
    /// NewNotificationWithButtonsAsync: New notification with buttons in action center
    /// </summary>
    /// <param name="pluginId">plug-in id for INotificationResponse</param>
    /// <param name="localeNewNotificationWithButtonsParameters">LocaleNewNotificationWithButtonsParameters</param>
    /// <param name="notificationDuration">notification duration</param>
    /// <param name="sid">SecurityIdentifier for the user (null for all sids)</param>
    /// <param name="cancellationToken">cancellation token</param>
    /// <returns>notificationId Task</returns>
    /// <exception cref="NotificationPluginException"></exception>
    /// <exception cref="OperationCanceledException"></exception>
    Task<Guid> NewNotificationWithButtonsAsync(Guid pluginId, LocaleNewNotificationWithButtonsParameters localeNewNotificationWithButtonsParameters,
        NotificationDuration notificationDuration, string sid, CancellationToken cancellationToken);

    /// <summary>
    /// NewProgressBarNotificationForAllSidsAsync: New progress-bar notification with buttons in action center (to all sids)
    /// </summary>
    /// <param name="pluginId">plug-in id for INotificationResponse</param>
    /// <param name="localeNewProgressBarParameters">LocaleNewProgressBarParameters</param>
    /// <param name="notificationDuration">notification duration</param>
    /// <param name="cancellationToken"></param>
    /// <returns>notificationId Task</returns>
    /// <exception cref="NotificationPluginException"></exception>
    /// <exception cref="OperationCanceledException"></exception>
    Task<Guid> NewProgressBarNotificationForAllSidsAsync(Guid pluginId, LocaleNewProgressBarParameters localeNewProgressBarParameters,
        NotificationDuration notificationDuration, CancellationToken cancellationToken);

    /// <summary>
    /// NewProgressBarNotificationAsync: New progress-bar notification with buttons in action center
    /// </summary>
    /// <param name="pluginId">plug-in id for INotificationResponse</param>
    /// <param name="localeNewProgressBarParameters">LocaleNewProgressBarParameters</param>
    /// <param name="notificationDuration">notification duration</param>
    /// <param name="sid">SecurityIdentifier for the user (null for all sids)</param>
    /// <param name="cancellationToken"></param>
    /// <returns>notificationId Task</returns>
    /// <exception cref="NotificationPluginException"></exception>
    /// <exception cref="OperationCanceledException"></exception>
    Task<Guid> NewProgressBarNotificationAsync(Guid pluginId, LocaleNewProgressBarParameters localeNewProgressBarParameters,
        NotificationDuration notificationDuration, string sid, CancellationToken cancellationToken);

    /// <summary>
    /// NewProgressBarNotificationWithButtonsForAllSidsAsync: New progress-bar notification with buttons in action center (to all sids)
    /// </summary>
    /// <param name="pluginId">plug-in id for INotificationResponse</param>
    /// <param name="localeNewProgressBarWithButtonsParameters">LocaleNewProgressBarWithButtonsParameters</param>
    /// <param name="notificationDuration">notification duration</param>
    /// <param name="cancellationToken">cancellation token</param>
    /// <returns>notificationId Task</returns>
    /// <exception cref="NotificationPluginException"></exception>
    /// <exception cref="OperationCanceledException"></exception>
    Task<Guid> NewProgressBarNotificationWithButtonsForAllSidsAsync(Guid pluginId, LocaleNewProgressBarWithButtonsParameters localeNewProgressBarWithButtonsParameters,
        NotificationDuration notificationDuration, CancellationToken cancellationToken);

    /// <summary>
    /// NewProgressBarNotificationWithButtonsAsync: New progress-bar notification with buttons in action center
    /// </summary>
    /// <param name="pluginId">plug-in id for INotificationResponse</param>
    /// <param name="localeNewProgressBarWithButtonsParameters">LocaleNewProgressBarWithButtonsParameters</param>
    /// <param name="notificationDuration">notification duration</param>
    /// <param name="sid">SecurityIdentifier for the user (null for all sids)</param>
    /// <param name="cancellationToken">cancellation token</param>
    /// <returns>notificationId Task</returns>
    /// <exception cref="NotificationPluginException"></exception>
    /// <exception cref="OperationCanceledException"></exception>
    Task<Guid> NewProgressBarNotificationWithButtonsAsync(Guid pluginId, LocaleNewProgressBarWithButtonsParameters localeNewProgressBarWithButtonsParameters,
        NotificationDuration notificationDuration, string sid, CancellationToken cancellationToken);

    /// <summary>
    /// UpdateProgressBarNotificationForAllSidsAsync: Update progress bar notification in action center (to all sids)
    /// </summary>
    /// <param name="pluginId">plug-in id for INotificationResponse</param>
    /// <param name="notificationId">notification id of notification to be updated</param>
    /// <param name="localeUpdateProgressBarParameters">LocaleUpdateProgressBarParameters</param>
    /// <param name="cancellationToken">cancellation token</param>
    /// <exception cref="NotificationPluginException"></exception>
    /// <exception cref="OperationCanceledException"></exception>
    Task UpdateProgressBarNotificationForAllSidsAsync(Guid pluginId, Guid notificationId, LocaleUpdateProgressBarParameters localeUpdateProgressBarParameters,
        CancellationToken cancellationToken);

    /// <summary>
    /// UpdateProgressBarNotificationAsync: Update progress bar notification in action center
    /// </summary>
    /// <param name="pluginId">plug-in id for INotificationResponse</param>
    /// <param name="notificationId">notification id of notification to be updated</param>
    /// <param name="localeUpdateProgressBarParameters">LocaleUpdateProgressBarParameters</param>
    /// <param name="sid">SecurityIdentifier for the user (null for all sids)</param>
    /// <param name="cancellationToken">cancellation token</param>
    /// <exception cref="NotificationPluginException"></exception>
    /// <exception cref="OperationCanceledException"></exception>
    Task UpdateProgressBarNotificationAsync(Guid pluginId, Guid notificationId, LocaleUpdateProgressBarParameters localeUpdateProgressBarParameters,
        string sid, CancellationToken cancellationToken);

    /// <summary>
    /// RemoveNotificationForAllSidsAsync: RemoveNotificationAsync notification with notificationId from action center (to all sids)
    /// </summary>
    /// <param name="pluginId">plug-in id for INotificationResponse</param>
    /// <param name="notificationId">notification id of notification to be removed</param>
    /// <param name="cancellationToken">cancellation token</param>
    /// <exception cref="NotificationPluginException"></exception>
    /// <exception cref="OperationCanceledException"></exception>
    Task RemoveNotificationForAllSidsAsync(Guid pluginId, Guid notificationId, CancellationToken cancellationToken);

    /// <summary>
    /// RemoveNotificationAsync: RemoveNotificationAsync notification with notificationId from action center
    /// </summary>
    /// <param name="pluginId">plug-in id for INotificationResponse</param>
    /// <param name="notificationId">notification id of notification to be removed</param>
    /// <param name="sid">SecurityIdentifier for the user (null for all sids)</param>
    /// <param name="cancellationToken">cancellation token</param>
    /// <exception cref="NotificationPluginException"></exception>
    /// <exception cref="OperationCanceledException"></exception>
    Task RemoveNotificationAsync(Guid pluginId, Guid notificationId, string sid, CancellationToken cancellationToken);

    /// <summary>
    /// NewNotificationFromMetaDataForAllSidsAsync: New notification from meta data (to all sids)
    /// </summary>
    /// <param name="pluginId">plug-in id for INotificationResponse</param>
    /// <param name="localeNewNotificationFromMetaData">LocaleNewNotificationFromMetaData</param>
    /// <param name="notificationDuration">notification duration</param>
    /// <param name="cancellationToken">cancellation token</param>
    /// <returns>notificationId Task</returns>
    /// <exception cref="NotificationPluginException"></exception>
    /// <exception cref="OperationCanceledException"></exception>
    Task<Guid> NewNotificationFromMetaDataForAllSidsAsync(Guid pluginId, LocaleNewNotificationFromMetaData localeNewNotificationFromMetaData,
        NotificationDuration notificationDuration, CancellationToken cancellationToken);

    /// <summary>
    /// NewNotificationFromMetaDataAsync: New notification from meta data
    /// </summary>
    /// <param name="pluginId">plug-in id for INotificationResponse</param>
    /// <param name="localeNewNotificationFromMetaData">LocaleNewNotificationFromMetaData</param>
    /// <param name="notificationDuration">notification duration</param>
    /// <param name="sid">SecurityIdentifier for the user (null for all sids)</param>
    /// <param name="cancellationToken">cancellation token</param>
    /// <returns>notificationId Task</returns>
    /// <exception cref="NotificationPluginException"></exception>
    /// <exception cref="OperationCanceledException"></exception>
    Task<Guid> NewNotificationFromMetaDataAsync(Guid pluginId, LocaleNewNotificationFromMetaData localeNewNotificationFromMetaData,
        NotificationDuration notificationDuration, string sid, CancellationToken cancellationToken);

    /// <summary>
    /// UpdateNotificationFromMetaDataForAllSidsAsync: Update notification from meta data (to all sids)
    /// </summary>
    /// <param name="pluginId">plug-in id for INotificationResponse</param>
    /// <param name="notificationId">notification id of notification to be updated</param>
    /// <param name="localeUpdateNotificationFromMetaData">LocaleUpdateNotificationFromMetaData</param>
    /// <param name="cancellationToken">cancellation token</param>
    /// <returns>Task</returns>
    /// <exception cref="NotificationPluginException"></exception>
    /// <exception cref="OperationCanceledException"></exception>
    /// <exception cref="NotificationPluginException"></exception>
    /// <exception cref="OperationCanceledException"></exception>
    Task UpdateNotificationFromMetaDataForAllSidsAsync(Guid pluginId, Guid notificationId, LocaleUpdateNotificationFromMetaData localeUpdateNotificationFromMetaData, 
        CancellationToken cancellationToken);

    /// <summary>
    /// UpdateNotificationFromMetaDataAsync: Update notification from meta data
    /// </summary>
    /// <param name="pluginId">plug-in id for INotificationResponse</param>
    /// <param name="notificationId">notification id of notification to be updated</param>
    /// <param name="localeUpdateNotificationFromMetaData">LocaleUpdateNotificationFromMetaData</param>
    /// <param name="sid">SecurityIdentifier for the user (null for all sids)</param>
    /// <param name="cancellationToken">cancellation token</param>
    /// <returns>Task</returns>
    /// <exception cref="NotificationPluginException"></exception>
    /// <exception cref="OperationCanceledException"></exception>
    /// <exception cref="NotificationPluginException"></exception>
    /// <exception cref="OperationCanceledException"></exception>
    Task UpdateNotificationFromMetaDataAsync(Guid pluginId, Guid notificationId, LocaleUpdateNotificationFromMetaData localeUpdateNotificationFromMetaData,
        string sid, CancellationToken cancellationToken);

    /// <summary>
    /// NewCustomNotificationForAllSidsAsync: New custom notification (to all sids)
    /// </summary>
    /// <param name="pluginId">plug-in id for INotificationResponse</param>
    /// <param name="sysTrayPluginId">systray pluginId</param>
    /// <param name="customNotification">string to pass to sysTrayPluginId</param>
    /// <param name="notificationDuration">notification duration</param>
    /// <param name="cancellationToken">cancellation token</param>
    /// <returns>notificationId Task</returns>
    /// <exception cref="NotificationPluginException"></exception>
    /// <exception cref="OperationCanceledException"></exception>
    Task<Guid> NewCustomNotificationForAllSidsAsync(Guid pluginId, Guid sysTrayPluginId, string customNotification,
        NotificationDuration notificationDuration, CancellationToken cancellationToken);

    /// <summary>
    /// NewCustomNotificationAsync: New custom notification
    /// </summary>
    /// <param name="pluginId">plug-in id for INotificationResponse</param>
    /// <param name="sysTrayPluginId">systray pluginId</param>
    /// <param name="customNotification">string to pass to sysTrayPluginId</param>
    /// <param name="notificationDuration">notification duration</param>
    /// <param name="sid">SecurityIdentifier for the user (null for all sids)</param>
    /// <param name="cancellationToken">cancellation token</param>
    /// <returns>notificationId Task</returns>
    /// <exception cref="NotificationPluginException"></exception>
    /// <exception cref="OperationCanceledException"></exception>
    Task<Guid> NewCustomNotificationAsync(Guid pluginId, Guid sysTrayPluginId, string customNotification,
        NotificationDuration notificationDuration, string sid, CancellationToken cancellationToken);

    /// <summary>
    /// UpdateCustomNotificationForAllSidsAsync: Update custom notification (to all sids)
    /// </summary>
    /// <param name="pluginId">plug-in id for INotificationResponse</param>
    /// <param name="notificationId">notification id of notification to be updated</param>
    /// <param name="sysTrayPluginId">systray pluginId</param>
    /// <param name="customNotification">string to pass to sysTrayPluginId</param>
    /// <param name="cancellationToken">cancellation token</param>
    /// <returns>Task</returns>
    /// <exception cref="NotificationPluginException"></exception>
    /// <exception cref="OperationCanceledException"></exception>
    Task UpdateCustomNotificationForAllSidsAsync(Guid pluginId, Guid notificationId, Guid sysTrayPluginId, string customNotification,
        CancellationToken cancellationToken);

    /// <summary>
    /// UpdateCustomNotificationAsync: Update custom notification
    /// </summary>
    /// <param name="pluginId">plug-in id for INotificationResponse</param>
    /// <param name="notificationId">notification id of notification to be updated</param>
    /// <param name="sysTrayPluginId">systray pluginId</param>
    /// <param name="customNotification">string to pass to sysTrayPluginId</param>
    /// <param name="sid">SecurityIdentifier for the user (null for all sids)</param>
    /// <param name="cancellationToken">cancellation token</param>
    /// <returns>Task</returns>
    /// <exception cref="NotificationPluginException"></exception>
    /// <exception cref="OperationCanceledException"></exception>
    Task UpdateCustomNotificationAsync(Guid pluginId, Guid notificationId, Guid sysTrayPluginId, string customNotification,
        string sid, CancellationToken cancellationToken);

    /// <summary>
    /// NewCustomPopupForAllSidsAsync: New Custom Popup (to all sids)
    /// </summary>
    /// <param name="pluginId">plug-in id</param>
    /// <param name="sysTrayPluginId">systray pluginId</param>
    /// <param name="customNotification">string to pass to sysTrayPluginId</param>
    /// <param name="notificationDuration">notification duration</param>
    /// <param name="cancellationToken">cancellation token</param>
    /// <returns>notificationId Task</returns>
    /// <exception cref="NotificationPluginException"></exception>
    /// <exception cref="OperationCanceledException"></exception>
    Task<Guid> NewCustomPopupForAllSidsAsync(Guid pluginId, Guid sysTrayPluginId, string customNotification,
        NotificationDuration notificationDuration, CancellationToken cancellationToken);

    /// <summary>
    /// NewCustomPopupAsync: New Custom Popup
    /// </summary>
    /// <param name="pluginId">plug-in id</param>
    /// <param name="sysTrayPluginId">systray pluginId</param>
    /// <param name="customNotification">string to pass to sysTrayPluginId</param>
    /// <param name="notificationDuration">notification duration</param>
    /// <param name="sid">SecurityIdentifier for the user (null for all sids)</param>
    /// <param name="cancellationToken">cancellation token</param>
    /// <returns>notificationId Task</returns>
    /// <exception cref="NotificationPluginException"></exception>
    /// <exception cref="OperationCanceledException"></exception>
    Task<Guid> NewCustomPopupAsync(Guid pluginId, Guid sysTrayPluginId, string customNotification,
        NotificationDuration notificationDuration, string sid, CancellationToken cancellationToken);

    /// <summary>
    /// UpdateCustomPopupForAllSidsAsync: Update Custom Popup (to all sids)
    /// </summary>
    /// <param name="pluginId">plug-in id</param>
    /// <param name="notificationId">notification id of notification to be updated</param>
    /// <param name="customNotification">string to pass to sysTrayPluginId</param>
    /// <param name="sysTrayPluginId">systray pluginId</param>
    /// <param name="cancellationToken">cancellation token</param>
    /// <returns>Task</returns>
    /// <exception cref="NotificationPluginException"></exception>
    /// <exception cref="OperationCanceledException"></exception>
    Task UpdateCustomPopupForAllSidsAsync(Guid pluginId, Guid notificationId, Guid sysTrayPluginId, string customNotification,
        CancellationToken cancellationToken);

    /// <summary>
    /// UpdateCustomPopupAsync: Update Custom Popup
    /// </summary>
    /// <param name="pluginId">plug-in id</param>
    /// <param name="notificationId">notification id of notification to be updated</param>
    /// <param name="customNotification">string to pass to sysTrayPluginId</param>
    /// <param name="sysTrayPluginId">systray pluginId</param>
    /// <param name="sid">SecurityIdentifier for the user (null for all sids)</param>
    /// <param name="cancellationToken">cancellation token</param>
    /// <returns>Task</returns>
    /// <exception cref="NotificationPluginException"></exception>
    /// <exception cref="OperationCanceledException"></exception>
    Task UpdateCustomPopupAsync(Guid pluginId, Guid notificationId, Guid sysTrayPluginId, string customNotification,
        string sid, CancellationToken cancellationToken);

    /// <summary>
    /// RemoveCustomPopupForAllSidsAsync: Remove custom popup (to all sids)
    /// </summary>
    /// <param name="pluginId">plug-in id</param>
    /// <param name="notificationId">notification id of notification to be removed</param>
    /// <param name="cancellationToken">cancellation token</param>
    /// <exception cref="NotificationPluginException"></exception>
    /// <exception cref="OperationCanceledException"></exception>
    Task RemoveCustomPopupForAllSidsAsync(Guid pluginId, Guid notificationId, CancellationToken cancellationToken);

    /// <summary>
    /// RemovePopup: Remove custom popup
    /// </summary>
    /// <param name="pluginId">plug-in id</param>
    /// <param name="notificationId">notification id of notification to be removed</param>
    /// <param name="sid">SecurityIdentifier for the user (null for all sids)</param>
    /// <param name="cancellationToken">cancellation token</param>
    /// <exception cref="NotificationPluginException"></exception>
    /// <exception cref="OperationCanceledException"></exception>
    Task RemoveCustomPopupAsync(Guid pluginId, Guid notificationId, string sid, CancellationToken cancellationToken);
}