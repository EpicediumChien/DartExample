#region LicenceHeader
//
// Copyright © 2022, Dell Inc., All Rights Reserved.
// This material is confidential and a trade secret.  Permission to use this
// work for any purpose must be obtained in writing from Dell Inc.
//
#endregion

using System.Windows;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.Common.Annotations;
using Dell.Client.Framework.UX.WPF;
using Dell.Client.Framework.UX.WPF.Controls;
using Microsoft;
using CommunityToolkit.Mvvm.Input;
using NGA.ThickClient.BannerNotificationPlugin.ViewModels;
using NGA.ThickClient.Interfaces;
using NGA.ThickClient.Interfaces.BannerNotifications;
using System.Windows.Data;
using Application = System.Windows.Application;
using System.Collections.Generic;
using System.Threading;
using System;
using System.Linq;

namespace NGA.ThickClient.BannerNotificationPlugin
{
    /// <summary>
    /// Implementation for BannerNotification to NGA plugins so they can add In Application Notifications.
    /// </summary>
    [Plugin("{8fcd239d-d951-4061-9625-2144e2ee0f77}", "MyDellBannerNotificationPlugin", Version = "1.0", Category = Category.Utility)]
    [Descriptor(Description = "MyDell Banner Notification Plugin")]
    [Publisher(Name = "MyDell Applications", Support = "Contact DCF Team")]
    public class BannerNotificationPlugin : IBannerNotificationPlugin, IThickClientPlugin, IDisposable
    {
        #region PrivateFields

        private readonly ReaderWriterLockSlim _lock = new();
        private readonly List<BannerNotificationDisplayData> _bannerNotificationQueue;
        private readonly IWindowLayout _windowLayout;
        private readonly IPluginManager _pluginManager;
        private readonly UXBellViewModel _uxBellViewModel;
        private readonly UXFlyout? _flyout;

        private UXAlertViewModel _uxAlertViewModel;
        private ILog Log { get; }
        private UXAlertItem? _banner;
        private UXAlert? _alert;
        private DateTime? _endTime;
        private BannerNotificationDisplayData? _expirationBannerData;
        private Timer? _startTimer;
        private bool _disposed;

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor for BannerNotificationPlugin.
        /// </summary>
        public BannerNotificationPlugin(IWindowLayout windowLayout, ICustomWindowLayout customWindowLayout, IPluginManager pluginManager, ILogFactory logFactory)
        {
            Requires.NotNull(windowLayout, nameof(windowLayout));
            Requires.NotNull(customWindowLayout, nameof(customWindowLayout));
            Requires.NotNull(pluginManager, nameof(pluginManager));
            Requires.NotNull(logFactory, nameof(logFactory));

            _windowLayout = windowLayout;
            _pluginManager = pluginManager;
            _bannerNotificationQueue = new List<BannerNotificationDisplayData>();
            _uxAlertViewModel = new UXAlertViewModel();
            _uxBellViewModel = new UXBellViewModel();

            Log = logFactory.CreateLogger("BANNER", typeof(BannerNotificationPlugin));

            if (_windowLayout.Masthead == null)
                return;

            _flyout = new UXFlyout
            {
                PlacementTarget = _windowLayout.Masthead,
                Placement = PlacementArea.Bottom,
                Width = _windowLayout.Masthead.ActualWidth
            };

            _windowLayout.Masthead.SizeChanged += Masthead_SizeChanged;

            var enabledBinding = new Binding("Enabled")
            {
                Source = _uxBellViewModel,
            };

            customWindowLayout.NotificationIcon.SetBinding(UIElement.IsEnabledProperty, enabledBinding);

            var notificationAvailableBinding = new Binding("NotificationAvailable")
            {
                Source = _uxBellViewModel,
            };

            customWindowLayout.NotificationIcon.SetBinding(UXBell.IsNotificationAvailableProperty, notificationAvailableBinding);

        }
        #endregion

        #region Properties
        /// <summary>
        /// It verifies if the banner notification is currently displayed or not. (Corresponds to IsOpen property)
        /// </summary>
        public bool IsAnyNotificationCurrentlyDisplayed
        {
            get
            {
                _lock.EnterReadLock();
                try
                {
                    return IsAnyNotificationCurrentlyDisplayedWithoutLock;
                }
                finally
                {
                    _lock.ExitReadLock();
                }
            }
        }

        private bool IsAnyNotificationCurrentlyDisplayedWithoutLock => _uxAlertViewModel.BannerId != Guid.Empty;

        /// <summary>
        /// Returns the Banner Id of Currently Displayed notification and Returns Guid.Empty in case of empty banner.
        /// </summary>
        public Guid CurrentlyDisplayedNotification
        {
            get
            {
                _lock.EnterReadLock();
                try
                {
                    return CurrentlyDisplayedNotificationWithoutLock;
                }
                finally
                {
                    _lock.ExitReadLock();
                }
            }
        }

        private Guid CurrentlyDisplayedNotificationWithoutLock => _uxAlertViewModel.BannerId;

        /// <summary>
        /// Returns number of notifications in Queue (including current displayed banner).
        /// </summary>
        public int NotificationQueueCount
        {
            get
            {
                _lock.EnterReadLock();
                try
                {
                    return _bannerNotificationQueue.Count;
                }
                finally
                {
                    _lock.ExitReadLock();
                }
            }
        }

        #endregion

        #region Methods
        /// <summary>
        /// Method to add In-Application Banner notifications Immediately or Queue It depending on the current banner state. 
        /// </summary>
        /// <param name="notificationData" cref="IBannerNotificationDisplayData">IBannerNotificationDisplayData.</param>
        public void AddNotification(IBannerNotificationDisplayData notificationData)
        {
            _lock.EnterWriteLock();
            try
            {
                ValidateNotification(notificationData);
                Log.Trace($"{nameof(AddNotification)} - {nameof(notificationData.BannerId)} {notificationData.BannerId}");
                try
                {
                    Log.Trace($"Started AddNotification for Banner :{notificationData.BannerId}.");
                    Log.Trace($"Any Banner Notification Currently Displayed :{IsAnyNotificationCurrentlyDisplayedWithoutLock}.");
                    if (!IsAnyNotificationCurrentlyDisplayedWithoutLock)
                    {
                        //Add Banner to Queue                        
                        AddBannerToQueue((BannerNotificationDisplayData)notificationData);

                        //Update Binding
                        UpdateBinding(_bannerNotificationQueue[0]);

                        //Show Flyout
                        StaysFlyoutOpen(true);
                    }
                    else
                    {
                        //Add Banner to Queue based on Priority
                        AddToQueueOnNotificationPriority(notificationData);
                    }

                    // AddNotificationToBell
                    UpdateBell();
                    Log.Trace($"Completed AddNotification for Banner :{notificationData.BannerId}.");
                }
                catch (ArgumentException ex)
                {
                    Log.Error(ex, "ArgumentException occurred on Adding Banner Notification");
                    throw new ArgumentException($"ArgumentException occurred on Adding Banner Notification - {ex}");
                }
            }
            finally
            {
                _lock.ExitWriteLock();
                Log.Trace($"Exit from AddNotification.");
            }
        }

        /// <summary>
        /// Method to remove the notification from queue or display.
        /// </summary>
        /// <param name="bannerId">Banner Id of specific banner that needs to be removed</param>
        /// <returns>True on Successfully updating Banner, else false</returns>
        public bool RemoveNotification(Guid bannerId)
        {
            _lock.EnterWriteLock();
            try
            {
                Log.Trace($"{nameof(RemoveNotification)} - Removing {nameof(bannerId)} - {bannerId}");
                var isNotificationRemoved = false;

                if (bannerId == Guid.Empty)
                {
                    Log.Error("Banner Notification is null in RemoveNotification.");
                    return isNotificationRemoved;
                }

                if (_bannerNotificationQueue.Count == 0 || _bannerNotificationQueue.All(x => x.BannerId != bannerId))
                {
                    Log.Error($"Banner Notification - {bannerId} does not exist for RemoveNotification.");
                    return isNotificationRemoved;
                }
                try
                {
                    Log.Trace($"Started RemoveNotification for Banner :{bannerId}");
                    Log.Trace($"Validate Queue Count :{_bannerNotificationQueue.Count}");

                    if (_bannerNotificationQueue.Count > 0)
                    {
                        var queueNotificationData = _bannerNotificationQueue.FirstOrDefault(x => x.BannerId == bannerId);

                        if (queueNotificationData != null)
                        {
                            Log.Trace($"Banner :{bannerId} to remove exists in Queue.");
                            RemoveNotificationFromQueue(queueNotificationData);
                            isNotificationRemoved = true;
                        }
                    }
                    else
                    {
                        //HideFlyout
                        StaysFlyoutOpen(false);
                    }
                }
                catch (ArgumentException ex)
                {
                    Log.Error(ex, "ArgumentException occurred during RemoveNotification");
                }

                Log.Trace($"Completed RemoveNotification for Banner :{bannerId} with RemovedStatus :{isNotificationRemoved}");
                return isNotificationRemoved;
            }
            finally
            {
                _lock.ExitWriteLock();
                Log.Trace("Exit from RemoveNotification.");
            }
        }

        /// <summary>
        /// Removes the Group of notifications from the queue or display belonging to same GroupId. 
        /// </summary>
        /// <param name="bannerGroupId" cref="IBannerNotificationDisplayData.BannerGroupId">Banner <paramref name="bannerGroupId"/> of specific set of banners</param>
        /// <returns>Number of notifications removed</returns>
        public int RemoveNotificationByGroup(Guid bannerGroupId)
        {
            _lock.EnterWriteLock();
            try
            {
                int notificationsRemoved = 0;
                if (bannerGroupId == Guid.Empty)
                {
                    Log.Error("Banner Notification Group is null in RemoveNotification.");
                    return notificationsRemoved;
                }

                if (_bannerNotificationQueue.Count == 0 || _bannerNotificationQueue.All(x => x.BannerGroupId != bannerGroupId))
                {
                    Log.Error($"Banner Notification Group - {bannerGroupId} does not exist for RemoveNotification.");
                    return notificationsRemoved;
                }
                try
                {
                    Log.Trace($"Started RemoveNotificationByGroup for Banner Group :{bannerGroupId}");
                    Log.Trace($"Validate Queue Count :{_bannerNotificationQueue.Count}");

                    if (_bannerNotificationQueue.Count > 0)
                    {
                        var notificationListToRemove = _bannerNotificationQueue.FindAll(x => x.BannerGroupId == bannerGroupId);
                        notificationsRemoved = GetNotificationRemovalCount(notificationListToRemove);
                    }
                    else
                    {
                        //HideFlyout
                        StaysFlyoutOpen(false);
                    }
                }
                catch (ArgumentException ex)
                {
                    Log.Error(ex, "ArgumentException occurred during RemoveNotificationByGroup");
                }

                Log.Trace($"Completed RemoveNotificationByGroup for Banner Group :{bannerGroupId} with {notificationsRemoved} Items removed from Queue.");
                return notificationsRemoved;
            }
            finally
            {
                _lock.ExitWriteLock();
                Log.Trace($"Exit from RemoveNotificationByGroup.");
            }
        }


        /// <summary>
        /// Removes all notifications from queue or display matching plugin Owner.
        /// </summary>
        /// <param name="pluginId" cref="IBannerNotificationDisplayData.PluginOwnerId">Banner <paramref name="pluginId"/> of specific set of banners</param>
        /// <returns>Number of notifications removed</returns>
        public int RemoveNotificationByPluginId(Guid pluginId)
        {
            _lock.EnterWriteLock();
            try
            {
                var notificationsRemoved = 0;
                if (pluginId == Guid.Empty)
                {
                    Log.Error("Banner Notification Plugin Owner is null in RemoveNotification.");
                    return notificationsRemoved;
                }

                if (_bannerNotificationQueue.Count == 0 || _bannerNotificationQueue.All(x => x.PluginOwnerId != pluginId))
                {
                    Log.Error($"Banner Notification Plugin Owner - {pluginId} does not exist for RemoveNotification.");
                    return notificationsRemoved;
                }
                try
                {
                    Log.Trace($"Started RemoveNotificationByPluginId for Owner :{pluginId}");
                    Log.Trace($"Validate Queue Count :{_bannerNotificationQueue.Count}");

                    if (_bannerNotificationQueue.Count > 0)
                    {
                        var notificationListToRemove = _bannerNotificationQueue.FindAll(x => x.PluginOwnerId == pluginId);
                        notificationsRemoved = GetNotificationRemovalCount(notificationListToRemove);
                    }
                    else
                    {
                        //HideFlyout
                        StaysFlyoutOpen(false);
                    }
                }
                catch (ArgumentException ex)
                {
                    Log.Error(ex, "ArgumentException occurred during RemoveNotificationByPluginId");
                }

                Log.Trace($"Completed RemoveNotificationByPluginId for Plugin Owner :{pluginId} with {notificationsRemoved} Items removed from Queue.");
                return notificationsRemoved;
            }
            finally
            {
                _lock.ExitWriteLock();
                Log.Trace("Exit from RemoveNotificationByPluginId.");
            }
        }

        /// <summary>
        /// Method to update an existing banner notifications currently displayed or queued for display. 
        /// </summary>
        /// <param name="notificationData" cref="IBannerNotificationDisplayData">IBannerNotificationDisplayData.</param>
        /// <returns>True on Successfully updating Banner, else false</returns>
        public bool UpdateNotification(IBannerNotificationDisplayData notificationData)
        {
            _lock.EnterWriteLock();
            try
            {
                var isNotificationUpdated = false;
                if (notificationData == null!)
                {
                    Log.Error("Banner Notification is null in UpdateNotification.");
                    return isNotificationUpdated;
                }

                Log.Trace($"{nameof(UpdateNotification)} - Updating {nameof(IBannerNotificationDisplayData.BannerId)} - {notificationData.BannerId}");

                if (_bannerNotificationQueue.Count == 0 || _bannerNotificationQueue.All(x => x.BannerId != notificationData.BannerId))
                {
                    Log.Error($"Banner Notification - {notificationData.BannerId} does not exist for Updating Notification.");
                    return isNotificationUpdated;
                }
                try
                {
                    Log.Trace($"Started UpdateNotification for Banner :{notificationData.BannerId} with Priority :{notificationData.DisplayPriority}");
                    var queueNotificationData = _bannerNotificationQueue.FirstOrDefault(x => x.BannerId == notificationData.BannerId);
                    if (queueNotificationData != null)
                    {
                        Log.Trace($"Banner to update :{notificationData.BannerId} exists in Queue.");

                        // Move Banner In Queue Based On Priority                        
                        var oldIndex = _bannerNotificationQueue.IndexOf(queueNotificationData);
                        _bannerNotificationQueue[oldIndex] = (BannerNotificationDisplayData)notificationData;

                        Log.Trace($"Moved Banner :{notificationData.BannerId} with Priority :{notificationData.DisplayPriority} In Queue Based On Priority.");

                        //Process Queue Change   
                        SortQueue();

                        var isCurrentNotificationLowPriority = _bannerNotificationQueue.Any(x => x.BannerId == CurrentlyDisplayedNotificationWithoutLock && x.DisplayPriority < notificationData.DisplayPriority);

                        if (IsAnyNotificationCurrentlyDisplayedWithoutLock &&
                            (CurrentlyDisplayedNotificationWithoutLock == notificationData.BannerId || isCurrentNotificationLowPriority))
                        {
                            //Update Binding
                            UpdateBinding(_bannerNotificationQueue[0]);
                        }
                        else
                        {
                            // set timer and close Flyout on DisplayDuration.
                            FlyoutClosureOnExpiration((BannerNotificationDisplayData)notificationData);
                        }

                        isNotificationUpdated = true;
                    }
                }
                catch (ArgumentNullException ex)
                {
                    Log.Error(ex, "ArgumentNullException occurred during Banner Notification Update");
                }

                Log.Trace($"Completed UpdateNotification for Banner :{notificationData.BannerId} with Priority :{notificationData.DisplayPriority} and UpdateStatus :{isNotificationUpdated}");
                return isNotificationUpdated;
            }
            finally
            {
                _lock.ExitWriteLock();
                Log.Trace($"Exit from UpdateNotification.");
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Set Binding of Banner.
        /// </summary>        
        private void SetBinding()
        {
            Log.Trace($"Set Binding for Banner:{_uxAlertViewModel.BannerId}");
            _banner ??= new UXAlertItem();

            var labelBinding = new Binding("Label")
            {
                Source = _uxAlertViewModel,
            };
            _banner.SetBinding(UXAlertItem.LabelProperty, labelBinding);

            var messageBinding = new Binding("Message")
            {
                Source = _uxAlertViewModel,
            };
            _banner.SetBinding(UXAlertItem.MessageProperty, messageBinding);

            var alertItemTypeBinding = new Binding("AlertItemType")
            {
                Source = _uxAlertViewModel,
            };
            _banner.SetBinding(UXAlertItem.AlertItemTypeProperty, alertItemTypeBinding);

            _banner.Hyperlink ??= new AlertCommand();
            var hyperlinkBinding = new Binding("HyperlinkText")
            {
                Source = _uxAlertViewModel,
            };

            BindingOperations.SetBinding(_banner.Hyperlink, AlertCommand.TextProperty, hyperlinkBinding);
            _banner.Hyperlink.Command = new RelayCommand(BannerNotificationHyperlinkCommand);

            _alert ??= new UXAlert
            {
                Close = new AlertCommand
                {
                    Command = new RelayCommand(BannerNotificationCloseCommand),
                }
            };

            if (_alert.Items.Count == 0)
            {
                _alert.Items.Add(_banner);

                Log.Trace($"Added Banner :{_uxAlertViewModel.BannerId} to UXAlert.");

                if (_flyout != null)
                {
                    _flyout.Child = _alert;
                    Log.Trace("Set Popup Child as UXAlert.");
                }
            }
            else
            {
                _alert.Items.Remove(_banner);
                _alert.Items.Add(_banner);
                Log.Trace($"Replace Banner :{_uxAlertViewModel.BannerId} in UXAlert.");
            }
        }

        /// <summary>
        /// Validate Add Notification Method Parameter.
        /// </summary>
        /// <param name="notificationData">IBannerNotificationDisplayData.</param>
        /// <exception cref="ArgumentException">ArgumentException.</exception>
        private void ValidateNotification(IBannerNotificationDisplayData notificationData)
        {
            if (notificationData == null)
            {
                Log.Error("Banner Notification is null in AddNotification.");
                throw new ArgumentException("Banner Notification is null in AddNotification.");
            }

            if (_bannerNotificationQueue.Any(x => x.BannerId == notificationData.BannerId))
            {
                Log.Error($"Banner Notification - {notificationData.BannerId} Already Existing in AddNotification.");
                throw new ArgumentException($"Banner Notification - {notificationData.BannerId} Already Existing in AddNotification.");
            }
        }

        private void AddToQueueOnNotificationPriority(IBannerNotificationDisplayData notificationData)
        {
            Log.Trace($"Add Banner :{notificationData.BannerId} with Priority :{notificationData.DisplayPriority} to Queue based on Priority.");
            bool isNotificationHighPriority = _bannerNotificationQueue[0].DisplayPriority < notificationData.DisplayPriority;

            if (isNotificationHighPriority)
            {
                // if New Notification Higher Priority
                Log.Trace($"Banner :{notificationData.BannerId} is of Higher Priority :{notificationData.DisplayPriority}.");

                //Insert Banner at Position 1
                _bannerNotificationQueue.Insert(0, (BannerNotificationDisplayData)notificationData);

                Log.Trace($"Banner :{notificationData.BannerId} inserted at 1st position in Queue.");

                //Process Queue Change
                SortQueue();

                //Update Binding
                UpdateBinding(_bannerNotificationQueue[0]);
            }
            else
            {
                //if New Notification Lower Priority
                Log.Trace($"Banner :{notificationData.BannerId} is of Lower Priority :{notificationData.DisplayPriority}.");

                //Add Banner to Queue
                AddBannerToQueue((BannerNotificationDisplayData)notificationData);

                // set timer and close Flyout on DisplayDuration.
                FlyoutClosureOnExpiration((BannerNotificationDisplayData)notificationData);
            }
        }

        private int GetNotificationRemovalCount(List<BannerNotificationDisplayData> notificationListToRemove)
        {
            var itemsRemoved = 0;
            Log.Trace($"{notificationListToRemove.Count} items waiting for removal from Queue.");

            if (notificationListToRemove.Count > 0)
            {
                foreach (BannerNotificationDisplayData notificationToRemove in notificationListToRemove)
                {
                    Log.Trace($"Trying to remove Banner :{notificationToRemove.BannerId} with Priority :{notificationToRemove.DisplayPriority} from Queue.");

                    // Remove Notification From Queue
                    RemoveNotificationFromQueue(notificationToRemove);
                    itemsRemoved++;
                }
            }

            Log.Trace($"{itemsRemoved} items removed from Queue.");
            return itemsRemoved;
        }

        private void RemoveNotificationFromQueue(BannerNotificationDisplayData notificationToRemove)
        {
            _bannerNotificationQueue.Remove(notificationToRemove);
            Log.Trace($"Banner :{notificationToRemove.BannerId} with Display Priority :{notificationToRemove.DisplayPriority} successfully removed from Queue.");

            //Process Queue Change
            SortQueue();
            Log.Trace($"Validate Queue Count :{_bannerNotificationQueue.Count}");

            if (_bannerNotificationQueue.Count == 0)
            {
                Log.Trace("Queue is empty.");

                //HideFlyout
                StaysFlyoutOpen(false);
            }
            else
            {
                Log.Trace("Queue is not empty.");
                if (CurrentlyDisplayedNotificationWithoutLock == notificationToRemove.BannerId)
                {
                    //Update Binding
                    UpdateBinding(_bannerNotificationQueue[0]);
                }
            }

            // RemoveNotificationFromBell.
            UpdateBell();
        }

        private void BannerNotificationCloseCommand()
        {
            Log.Trace($"Started BannerNotificationCloseCommand for Banner :{_uxAlertViewModel.BannerId} with Display Priority :{_uxAlertViewModel.AlertItemType}");
            var receivesBannerNotification = (IReceivesBannerNotification?)_pluginManager.FindPluginById(_uxAlertViewModel.PluginOwnerId.ToString());

            if (receivesBannerNotification != null)
            {
                Log.Trace($"ReceivesBannerNotificationPlugin for Banner :{_uxAlertViewModel.BannerId} is not null.");
                IBannerNotificationInteraction data = new BannerNotificationInteraction(_uxAlertViewModel.BannerId, _uxAlertViewModel.BannerGroupId
                    , InteractionType.ControlClosed);
                receivesBannerNotification.OnBannerNotificationInteracted(data);
                Log.Trace($"{InteractionType.ControlClosed} Interaction triggered for Banner :{_uxAlertViewModel.BannerId}");
            }

            //Remove Banner
            RemoveNotification(_uxAlertViewModel.BannerId);
        }

        private void BannerNotificationHyperlinkCommand()
        {
            Log.Trace($"Started BannerNotificationHyperlinkCommand for Banner :{_uxAlertViewModel.BannerId} with Display Priority :{_uxAlertViewModel.AlertItemType}");
            var receivesBannerNotification = (IReceivesBannerNotification?)_pluginManager.FindPluginById(_uxAlertViewModel.PluginOwnerId.ToString());

            if (receivesBannerNotification != null)
            {
                Log.Trace($"ReceivesBannerNotificationPlugin for Banner :{_uxAlertViewModel.BannerId} is not null.");
                IBannerNotificationInteraction data = new BannerNotificationInteraction(_uxAlertViewModel.BannerId, _uxAlertViewModel.BannerGroupId
                , InteractionType.ControlAction);
                receivesBannerNotification.OnBannerNotificationInteracted(data);
                Log.Trace($"{InteractionType.ControlAction} Interaction triggered for Banner :{_uxAlertViewModel.BannerId}");
            }
        }

        private void BannerNotificationExpirationCommand(BannerNotificationDisplayData banner)
        {
            _lock.EnterWriteLock();
            Log.Trace($"Started BannerNotificationExpirationCommand for {banner.BannerId} with Display Priority :{banner.DisplayPriority}");
            try
            {
                var receivesBannerNotification = (IReceivesBannerNotification?)_pluginManager.FindPluginById(banner.PluginOwnerId.ToString());
                if (receivesBannerNotification != null)
                {
                    Log.Trace($"ReceivesBannerNotificationPlugin for Banner :{banner.BannerId} is not null.");
                    IBannerNotificationInteraction data = new BannerNotificationInteraction(banner.BannerId, banner.BannerGroupId, InteractionType.Expired);
                    receivesBannerNotification.OnBannerNotificationInteracted(data);
                    Log.Trace($"{InteractionType.Expired} Interaction triggered for Banner :{_uxAlertViewModel.BannerId}");
                }
            }
            finally
            {
                _lock.ExitWriteLock();
                Log.Trace($"Exit BannerNotificationExpirationCommand for Banner:{banner.BannerId}");
            }
        }

        private void UpdateBinding(BannerNotificationDisplayData bannerNotificationDisplayData)
        {
            UpdateAlertItemViewModel(bannerNotificationDisplayData);
            Log.Trace($"Updated Binding for Banner :{bannerNotificationDisplayData.BannerId}.");

            // Check if we have an application AND we are not on the UI thread
            if (Application.Current != null && !Application.Current.Dispatcher.CheckAccess())
                Application.Current.Dispatcher.BeginInvoke(SetBinding);
            else
                SetBinding();

            // set timer and close Flyout on DisplayDuration.
            FlyoutClosureOnExpiration(bannerNotificationDisplayData);
        }

        private void UpdateAlertItemViewModel(BannerNotificationDisplayData bannerNotificationDisplayData)
        {
            _uxAlertViewModel.BannerId = bannerNotificationDisplayData.BannerId;
            _uxAlertViewModel.BannerGroupId = bannerNotificationDisplayData.BannerGroupId;
            _uxAlertViewModel.PluginOwnerId = bannerNotificationDisplayData.PluginOwnerId;
            _uxAlertViewModel.Label = bannerNotificationDisplayData.Label;
            _uxAlertViewModel.Message = bannerNotificationDisplayData.Message;
            _uxAlertViewModel.AlertItemType = (UXAlertType)bannerNotificationDisplayData.BannerItemType;
            _uxAlertViewModel.HyperlinkText = bannerNotificationDisplayData.HyperlinkText;
        }

        /// <summary>
        /// Trigger the Notification Expiry.
        /// </summary>
        /// <param name="displayData">BannerNotificationDisplayData to expire.</param>
        private void FlyoutClosureOnExpiration(BannerNotificationDisplayData displayData)
        {
            Log.Info($"{nameof(FlyoutClosureOnExpiration)} - {nameof(displayData.BannerId)} {displayData.BannerId} Time {displayData.DisplayDuration.GetValueOrDefault().TotalMilliseconds}");
            Log.Trace($"Checking Banner :{displayData.BannerId} Expiry.");
            if (displayData.DisplayDuration.HasValue && displayData.DisplayDuration.Value.Ticks != 0)
            {
                Log.Trace($"Banner :{displayData.BannerId} will Expires in {displayData.DisplayDuration.Value}.");
                if (_startTimer == null)
                {
                    _endTime = DateTime.Now.Add(displayData.DisplayDuration.Value);
                    _expirationBannerData = displayData;
                    _startTimer = new Timer(CheckTimerStatus, null, 0, 1000);
                }
                else
                {
                    _endTime = DateTime.Now.Add(displayData.DisplayDuration.Value);
                    _expirationBannerData = displayData;
                }

                Log.Trace($"Timer triggered for Banner :{displayData.BannerId}.");
            }
        }

        private void CheckTimerStatus(object? sender)
        {
            if (DateTime.Now >= _endTime)
            {
                _startTimer?.Dispose();
                _startTimer = null;

                if (_expirationBannerData != null)
                    OnTimerTick(_expirationBannerData);
            }
        }

        private void OnTimerTick(BannerNotificationDisplayData banner)
        {
            Log.Trace($"{nameof(OnTimerTick)} - {nameof(banner.BannerId)} {banner.BannerId}");
            BannerNotificationExpirationCommand(banner);
            RemoveNotification(banner.BannerId);
            Log.Trace($"Banner :{banner.BannerId} with Priority :{banner.DisplayPriority} Expired.");
        }

        /// <summary>
        /// Set Flyout Visibility.
        /// </summary>
        /// <param name="isFlyoutOpen">It sets visibility.</param>
        private void StaysFlyoutOpen(bool isFlyoutOpen)
        {
            Application.Current?.Dispatcher.BeginInvoke(() =>
            {
                ToggleFlyout(isFlyoutOpen);
            });

            if (isFlyoutOpen)
                return;

            Log.Trace("Clear Banner Queue.");
            _uxAlertViewModel = new UXAlertViewModel();
            _bannerNotificationQueue.Clear();
        }

        /// <summary>
        /// Toggles the Flyout open or closed
        /// </summary>
        /// <param name="isFlyoutOpen"></param>
        private void ToggleFlyout(bool isFlyoutOpen)
        {
            if (_flyout != null)
            {
                _flyout.IsOpen = isFlyoutOpen;
                Log.Trace($"Set Flyout Open :{_flyout.IsOpen}.");
            }
        }

        /// <summary>
        /// Add Banner to Queue.
        /// </summary>
        /// <param name="notificationData">BannerNotificationDisplayData.</param>
        private void AddBannerToQueue(BannerNotificationDisplayData notificationData)
        {
            _bannerNotificationQueue.Add(notificationData);
            Log.Trace($"Add Banner to Queue :{notificationData.BannerId} with Priority :{notificationData.DisplayPriority}.");

            //Process Queue Change
            SortQueue();
        }

        /// <summary>
        /// Sort Queue.
        /// </summary>
        private void SortQueue()
        {
            _bannerNotificationQueue.Sort(delegate (BannerNotificationDisplayData x, BannerNotificationDisplayData y)
            {
                if (x.DisplayPriority < y.DisplayPriority) return 1;
                if (x.DisplayPriority > y.DisplayPriority) return -1;
                return 0;
            });
            Log.Trace("Sorted Banner Queue.");
        }

        /// <summary>
        /// Masthead SizeChanged Event.
        /// </summary>
        /// <param name="sender">sender.</param>
        /// <param name="e">SizeChangedEventArgs.</param>
        private void Masthead_SizeChanged(object sender, System.Windows.SizeChangedEventArgs e)
        {
            if (_windowLayout.Masthead != null && _flyout != null)
                _flyout.Width = _windowLayout.Masthead.ActualWidth;
        }

        /// <summary>
        /// Updates the Bell
        /// </summary>
        private void UpdateBell()
        {
            // No need for a lock here since we already have either a read or write lock
            if (_bannerNotificationQueue.Count > 1)
            {
                _uxBellViewModel.Enabled = true;
                _uxBellViewModel.NotificationAvailable = true;
            }
            else
            {
                _uxBellViewModel.Enabled = false;
                _uxBellViewModel.NotificationAvailable = false;
            }
        }

        /// <summary>
        /// Dispose unmanaged resources
        /// </summary>
        /// <param name="disposing">bool</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing && _windowLayout.Masthead != null)
            {
                _windowLayout.Masthead.SizeChanged -= Masthead_SizeChanged;
            }

            _disposed = true;
        }

        #endregion
    }
}
