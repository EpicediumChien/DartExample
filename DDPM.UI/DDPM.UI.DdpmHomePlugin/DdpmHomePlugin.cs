using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using DDPM.SA.Common;
using DDPM.UI.Common;
using DDPM.UI.Common.Models;
using DDPM.UI.Common.UserControls;
using DDPM.UI.Plugin.Common;
using DDPM.UI.Plugin.DdpmHomePlugin.Interfaces;
using DDPM.UI.Plugin.DdpmHomePlugin.ViewModels;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.Common.Annotations;
using Dell.Client.Framework.Common.PluginConditions;
using Dell.Client.Framework.UX.WPF;
using Dell.Client.Framework.UX.WPF.Controls;
using Microsoft;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;
using NGA.ThickClient.Interfaces;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.IO;
using VcpCore.Common;
using Windows.Devices.Geolocation;
using Windows.Devices.Input;
using static Dell.Client.Framework.Security.LocalAccounts;
using static Dell.Client.Framework.UX.WPF.WinApi;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;
using DDPMConstants = DDPM.UI.Common.Constants;
using User32 = DDPM.UI.Common.User32;

//using VcpCore.Interfaces;
using IDdpmHomePageViewModel = DDPM.UI.Plugin.DdpmHomePlugin.Interfaces.IDdpmHomePageViewModel;
using static DDPM.UI.Common.User32;
using System.Windows.Threading;
using DDPM.SA.Common.UpdateProgressPage;
using Windows.ApplicationModel.VoiceCommands;
using Microsoft.Toolkit.Uwp.Notifications;
using System.Linq;
using System.Windows.Automation;
using DDPM.SA.Common.Settings;
using System.Globalization;
using DDPM.UI.Resources.Helper;
using Microsoft.VisualBasic.Logging;
using System.Diagnostics.Eventing.Reader;
using System;
using DDPM.Easy.Common;

namespace DDPM.UI.Plugin.DdpmHomePlugin
{
    //Robert_Lin, 2024-6-21 add IDisposable to notify VCPCode when UI closed.

    /// <summary>
    /// Interaction logic for AboutView Plugin.xaml
    /// </summary>
    [Plugin(PluginId, PluginName, Version = PluginVersion, Category = Category.Utility)]
    [Descriptor(Description = Description)]
    [Publisher(Name = "DDPM HomePlugin", Support = "Wistron DDPM Team")]
    [PluginRequires(Id = DDPM.SA.Common.IDs.Device_Manager_Plugin_ID, AllowDynamicResolving = true)]
    [ExcludeFromCodeCoverage]
    public class DdpmHomePlugin : IThickClientHomePagePlugin, IConsolePluginSupportsActivations, IThickClientPlugin, IDisposable
    {
        private const string PluginId = DDPMConstants.DdpmHomePluginId;
        private const string PluginName = "DDPM Home plugin";
        private const string PluginVersion = "1.0";
        private const string Description = "Display DDPM.Homepage";

        private List<MonitorInfo> _monitorCache = new List<MonitorInfo>();

        //internal static readonly Ioc PluginIoc = new();
        public static readonly Ioc PluginIoc = new();

        private readonly ILog _log;
        private readonly IConsole _console;
        private readonly IShowPluginManager _showPluginManager;

        private bool _isConfigured;
        private bool _isActived = false;

        /// <summary>
        /// This property is required by the IConsolePagePlugin. It specifies the text to display when the page is shown.
        /// </summary>
        public string HeaderText => "Homepage";

        /// <summary>
        /// Page Type
        /// </summary>
        //public Type PageType => typeof(AboutView);
        public Type PageType => typeof(DdpmHomePage);

        public bool HasTiles => throw new NotImplementedException();

        //Device manager related object
        private static IDeviceManagerSA? _deviceManager;

        private readonly IPluginManager _pluginManager;
        private IFrameworkPluginConditionNotification? _IDeviceManagerPluginCondition;
        private CancellationTokenSource StartupCancellationTokenSource { get; } = new();
        private CancellationToken CancellationToken { get; }
        private readonly SemaphoreSlim _lock = new(1, 1);
        private bool _disposed;
        private bool _HasRegisted = false;
        private DdpmHomePageViewModel? _viewModel;
        private bool _IsAnyUpdate = false;


        // For WalkThrough
        public static string UserId { get => userId; set => userId = value; }
        private static string userId = string.Empty;

        public static bool ShowPluginById { get => showPluginById; set => showPluginById = value; }
        private static bool showPluginById = false;
        public static List<WalkThroughInfo> WalkThroughQueue { get; private set; } = new List<WalkThroughInfo>();
        public static List<WalkThroughInfo> WalkThroughEndList { get; private set; } = new List<WalkThroughInfo>();

        private static readonly Dictionary<string, int> ModelTypeMapping = new Dictionary<string, int>
        {
            { "DDPM", 1 },
            { "Displays", 2 },
            { "LogicalWebcam", 3 },
            { "LogicalKeyboard", 4 },
            { "LogicalMouse", 5 },
            { "LogicalPen", 6 },
            { "LogicalHeadset", 7 }
        };

        private GlobalSettingParam _globalSettings = null;

        //Robert_Lin, 2024-12-9, FW, SW Update avaiable count
        //PIMS-299696  Gear icon indication blinking not only twice to show availability of FW update
        //
        private int _updateAvailableCount_FW = 0;
        private int _updateAvailableCount_SW = 0;

        private CancellationTokenSource _deviceChangedDebounceCts;

        //Robert_Lin 2025-4-18 added, to keep the DIspatcher, which will be used for Dispose static UI resources.
        private readonly Dispatcher? uiDIspatcher = null;

  //      private WindowsServiceMonitor _dthMonitor = new WindowsServiceMonitor(UI.Common.Constants.DTH_ServiceName);

        /// <summary>
        /// Default constructor
        /// </summary>
        public DdpmHomePlugin(IWindowLayout windowLayout, IShowPluginManager showPluginManager, IPluginManager pluginManager, IConsole console, IGearMenu gearMenu)
        {
            _showPluginManager = showPluginManager;
            _console = console;
            _pluginManager = pluginManager;
            _log = console.CreateLog("DDPMHOME");
            _log.Info($"{nameof(DdpmHomePlugin)} - Constructed");
            _log.Info($"current process ID: {Process.GetCurrentProcess().Id}");

            //Robert_Lin 2025-1-22 added to force the static contructor of DdpmCultureMap to be called.
            //PIMS-331191 With DDPM installed, observe language in DDPM UI not change for other langauges of Other countries
            _log.Info($"CurrentCultureInfo=[{CultureInfo.CurrentCulture.Name}],CurrentUICultureInfo=[{CultureInfo.CurrentUICulture.Name}], MappedCultureInfo=[{DDPM.UI.Resources.DdpmCultureMap.MappedCultureInfo.Name}]");

            //DdpmCommonHelper.MyConsole = console;

            //var aboutGearItem = new GearMenuItem(string.Format(Resources.Resources.AboutView_Gear_Text, _applicationName), new RelayCommand(ShowAboutView));
            //var ddpmGearItem = new GearMenuItem("DDPM Demo", new RelayCommand(ShowDdpmHome));

            //gearMenu.AddGearMenuItem(ddpmGearItem, 0);

            CancellationToken = StartupCancellationTokenSource.Token;
            _pluginManager.PluginsStarted += PluginManager_PluginsStarted;

            UXMasthead masthead = windowLayout.Masthead;
            if (masthead != null)
            {
                AddIconsToMasthead(masthead);
            }

            //Robert_Lin, 2024-12-21 Register a event handler to handle when mainwindow
            // move to new position
            _console.RegisterForEvent(ConsoleEventNames.MainWindow_MoveToNewPosition, Handle_MainWindow_MoveToNewPosition);

            //Robert_Lin 2025-4-15 Add for dispose when the process exit
            uiDIspatcher = System.Windows.Application.Current.Dispatcher;
            AppDomain.CurrentDomain.ProcessExit += (s, e) =>
            {
                if (uiDIspatcher != null)
                {
                    if (uiDIspatcher.HasShutdownStarted)
                    {
                        //When the shutdown process has started, we should not to use Dispatcher 
                        return;
                    }
                    uiDIspatcher.Invoke(() =>
                    {
                        ISplitCtrl.DisposeAll();
                    });
                }
            };
        }

        private void PluginManager_PluginsStarted(object? sender, PluginsStartedEventArgs pluginsStartedEventArgs)
        {
            _log.Info($"{nameof(PluginManager_PluginsStarted)} started");
            try
            {
                if (_deviceManager != null)
                    return;

                _deviceManager = _pluginManager.FindPluginByType<IDeviceManagerSA>(PluginResolution.Dynamic);

                if (_deviceManager == null)
                {
                    _log.Error($"{nameof(PluginManager_PluginsStarted)} IDeviceManagerSA Plugin is null");
                    return;
                }
                if (!pluginsStartedEventArgs.ChangedPlugins.OfType<IDeviceManagerSA>().Any())
                {
                    _log.Trace($"{nameof(PluginManager_PluginsStarted)} IDeviceManagerSA Plugin is not available.");
                    return;
                }

                // Manager Peripheralslugin Condition
                _IDeviceManagerPluginCondition = _deviceManager as IFrameworkPluginConditionNotification;

                if (_IDeviceManagerPluginCondition == null)
                {
                    _log.Error($"{nameof(PluginManager_PluginsStarted)} _IDeviceManagerPluginCondition is null");
                    return;
                }

                // Subscribe to plugin changes
                _IDeviceManagerPluginCondition.PluginConditionChangeHandler += _IDeviceManagerPluginCondition_PluginConditionChangeHandler;

                // Get current condition
                _ = Task.Run(GetCurrentDeviceManagerPluginPluginCondition, CancellationToken);

                //CheckIfNeedNavigateToWebcamPageV2();

                if (UserId == string.Empty || UserId == null)
                    UserId = WTSFunction.DirectGetUserID(_log);

                _log.Info($"{nameof(PluginManager_PluginsStarted)} out");
            }
            catch (Exception ex)
            {
                var message = $"{nameof(PluginManager_PluginsStarted)} failed: {ex.Message}";
                _log.Error(ex, message);
            }
        }

        private async Task GetCurrentDeviceManagerPluginPluginCondition()
        {
            await _lock.WaitAsync(CancellationToken);
            _log.Info($"{nameof(GetCurrentDeviceManagerPluginPluginCondition)} lock");
            try
            {
                if (_IDeviceManagerPluginCondition == null)
                    return;

                var pluginCondition = await ((IFrameworkPluginConditionNotification)_IDeviceManagerPluginCondition).CurrentConditionAsync();

                if (pluginCondition is PluginErrorCondition)
                {
                    if (_viewModel != null)
                    {
                        _viewModel.IsDeviceManagerReady = false;
                        _log.Info($"{nameof(GetCurrentDeviceManagerPluginPluginCondition)} _viewModel.IsDeviceManagerReady = false ... ");
                    }

                    _log.Info($"{nameof(GetCurrentDeviceManagerPluginPluginCondition)} plugin is in {nameof(PluginErrorCondition)}");
                }
                else if (pluginCondition is PluginRunningCondition)
                {
                    _log.Info($"{nameof(GetCurrentDeviceManagerPluginPluginCondition)} plugin is in {nameof(PluginRunningCondition)}");

                    if (_deviceManager != null)
                    {
                        if (!_HasRegisted)
                        {
                            //Robert_Lin, 2024-12-16 add log for each key points to trace status.


                            if (_viewModel != null)
                            {
                                _viewModel.IsDeviceManagerReady = true;
                                _log.Info($"{nameof(GetCurrentDeviceManagerPluginPluginCondition)} _viewModel.IsDeviceManagerReady = true ... ");
                                //Robert_Lin, 2024-12-16 for PleaseWait thread to get devices
                                _viewModel.DeviceManagerPlugin = _deviceManager;

                            }

                            //Robert_Lin 2025-4-12 move these registers to RegisterDeviceManagerEvents()
                            RegisterDeviceManagerEvents();
                            //_HasRegisted = true;
                            //_deviceManager.DeviceChanged += _deviceManager_DeviceChanged;
                            //_deviceManager.VCPchanged += _deviceManager_VCPchanged;
                            //_deviceManager.UIUpdateNotify += _deviceManager_UIUpdateNotify;
                            //_deviceManager.MonitorinfoUpdated += _deviceManager_MonitorinfoUpdated;

                            //Elapsed= 4, 2 msec
                            //Robert_Lin, 2024-6-21 UI shown, tell VCPCore to increase polling rate to 0x52
                            _log.Info("Calling to DeviceManager.Reset0x52TimerTick(2000)");
                            Task delayTask = _deviceManager.Reset0x52TimerTick(2000, Process.GetCurrentProcess().Id);

                            //Elapsed= 5, 5 msec
                            _log.Info("Calling to DeviceManager.ReceiveTelemetryInfo(AppSession,AppStarted)");
                            _deviceManager.ReceiveTelemetryInfo("AppSession", "AppStarted", Telementry_Frequency.RealTime);

                            await CheckIfNeedNavigateToSettingPageByQAMOSD();  //Derek 1217 for QAM PIMS-332041

                            //Elapsed= 2 msec
                            //1030 get global settings for telemetry consent page using
                            _log.Info("Calling to DeviceManager.GetGlobalSettingParam()");
                            _globalSettings = _deviceManager.GetGlobalSettingParam().Result;
                            _log.Info("Return from DeviceManager.GetGlobalSettingParam()");

                            if (DdpmCommonHelper.DeviceManagerSA == null)
                            {
                                _log.Info("DdpmCommonHelper.DeviceManagerSA null, DdpmCommonHelper.DeviceManagerSA = _deviceManager");
                                DdpmCommonHelper.DeviceManagerSA = _deviceManager;
                            }

                            //1030 Dean
                            //For Hess to read global setting "_globalSettings"
                            //After "GetDdpmDevicesAsync" the user setting cache is ready "DdpmCommonHelper.Settings_Cache"

                            if (DdpmCommonHelper.Settings_Cache == null)// &&
                                                                        //DdpmCommonHelper.DeviceManagerSA != null)
                            {
                                //Elapsed= 2 msec
                                _log.Info("Calling to ReadDDPMSettings()");
                                DdpmCommonHelper.ReadDDPMSettings();
                                _log.Info("Return to ReadDDPMSettings()");
                            }
                            if (_globalSettings != null && DdpmCommonHelper.Settings_Cache != null && _viewModel != null)
                            {
                                if (!_globalSettings.isSetTelemetryOverInstaller)
                                {
                                    _log.Info("Invoking ShowConsent()");
                                    await CheckAndQueueDevice("CONSENT_PAGE", "CONSENT_PAGE", null);
                                }
                            }

                            // Add DDPM walk through before walk through page constructed
                            _log.Info($"Calling to CheckAndQueueDevice(DDPM,DDPM,null)");
                            await CheckAndQueueDevice("DDPM", "DDPM", null);//DDPM WalkThrough no need into setting page.
                            _log.Info($"Returned from CheckAndQueueDevice()");

                            _monitorInfos = _deviceManager.GetMonitors().Result;          // change to restore monitor caches
                            _log.Info($"[DdpmHomePlugin] GetCurrentDeviceManagerPluginPluginCondition after GetMonitors");

                            _deviceInfos = _deviceManager.GetDevices().Result.deviceInfo; // change to restore device caches
                            _log.Info($"[DdpmHomePlugin] GetCurrentDeviceManagerPluginPluginCondition after GetDevices");

                            //Elapsed= 78, 61 msec
                            //Wayn 2024-09-04 For WalkThrough
                            _log.Info("Calling to CollectAndCompareDevicesAsync()");
                            await CollectAndCompareDevicesAsync();
                            _log.Info("Return from CollectAndCompareDevicesAsync()");

                            //Elapsed= 3692, 392 msec
                            //Robert_Lin, 2024-12-18, PIMS-316225 DDPM take > 10sec to show Home Page on Launch after restart
                            //Move below code into a background task
                            //Robert_Lin 2024-8-2 DDPMW-579, If there is any FW/SW update available,
                            //then the Gear icon on masthead will show breathe & glow animation.
                            //Call once
                            Task t1 = Task.Run(() =>
                            {
                                //Use this flag to Debug/Test Gear icon GlowEffect, it will force to trigger the GlowEffect startup
                                bool forceGlowEffect = DevSettings.ForceGearIconGlowEffectAtStartup();

                                _log.Info("Calling to CheckIfSwFwUpdateAvailable()");
                                if (CheckIfSwFwUpdateAvailable(_deviceManager) || forceGlowEffect)
                                {
                                    _log.Info("Return from CheckIfSwFwUpdateAvailable(), return true");
                                    //Robert_Lin 2025-1-21 Change GearIcon (InfoPath to GeatButton)
                                    //Robert_Lin, 2024-12-9, Change GlowEffect_Start() to GlowEffect_Trigger()
                                    if (_gearBtn != null)
                                    {
                                        _gearBtn.GlowEffect_Trigger();
                                    }
                                    else if (_iconGear != null)
                                    {
                                        //Elapsed= 1, 1 msec
                                        _log.Info("Calling to GlowEffect_Trigger()");
                                        _iconGear.GlowEffect_Trigger();
                                    }
                                    else
                                    {
                                        _log.Info("Not calling to GlowEffect_Trigger(), due to _iconGear is null.");
                                    }
                                }
                                else
                                    _log.Info("Return from CheckIfSwFwUpdateAvailable(), return false");

                            });

                            //Robert_Lin, 2024-12-9 install event handler for new update fw/sw info
                            _deviceManager.Peripherals_UpdateNotify += _deviceManager_Peripherals_UpdateNotify;

                        }
                        //Elapsed= 13, 14 msec
                        if (WalkThroughQueue.Count != 0 && ShowPluginById == false)
                        {
                            _log.Info($"[Walkthrough] WalkThroughQueue.Count != 0, ShowPluginById Start DDPM");
                            _showPluginManager?.ShowPluginById(DDPM.UI.Common.Constants.WalkThroughPluginId);
                            ShowPluginById = true;
                        }

                        _log.Info("Calling to GetDdpmDevicesAsync()");
                        await GetDdpmDevicesAsync(_deviceManager);
                        _log.Info("Return from GetDdpmDevicesAsync()");

                        //Elapsed= 1 msec
                        _log.Info($"Calling to CheckIfNeedImportSetting_Display()");
                        _monitorCache.Clear();
                        _monitorCache = _monitorInfos;
                        CheckIfNeedImportSetting_Display(_monitorInfos);
                        _log.Info($"Return from CheckIfNeedImportSetting_Display()");
                    }
                }
                else if (pluginCondition is PluginStartedCondition)
                {
                    _log.Info($"{nameof(GetCurrentDeviceManagerPluginPluginCondition)} plugin is in {nameof(PluginStartedCondition)}");
                }
                else if (pluginCondition is PluginStoppedCondition)
                {
                    _log.Info($"{nameof(GetCurrentDeviceManagerPluginPluginCondition)} plugin is in {nameof(PluginStoppedCondition)}");
                }
                _log.Info($"{nameof(GetCurrentDeviceManagerPluginPluginCondition)} lock out");
            }
            catch (Exception ex)
            {
                var message = $"{nameof(GetCurrentDeviceManagerPluginPluginCondition)} failed with error - {ex.Message}";
                _log.Error(ex, message);
            }
            finally
            {
                _lock.Release();
                _log.Trace($"{nameof(GetCurrentDeviceManagerPluginPluginCondition)} unlock");
            }
        }

        //Robert_Lin 2025-3-20 added when MonitorInfo has been updated, one event for each Monitor
        private void _deviceManager_MonitorinfoUpdated(object? sender, MonitorinfoUpdateEventArgs e)
        {
            if ((e != null) && (e.monitor != null) && (e.edid != null))
            {
                MonitorInfo monitor = e.monitor;
                EDID edid = e.edid;

                _log.Info($"@ MonitorInfoUpdated(): Model=[{monitor.modelName}], ServiceTag=[{monitor.edid.ServiceTag}]");

                if (_viewModel != null)
                {
                    HomeDevice? homeDevice = _viewModel.FindMonitorByEdid(edid);
                    if (homeDevice == null)
                    {
                        _log.Info($"@ MonitorInfoUpdated(): Monitor not found in HomeDevices");
                    }
                    else
                    {
                        _log.Info($"@ MonitorInfoUpdated(): MonitorInfo updated");
                        homeDevice.MonitorInfo = monitor;

                        _viewModel.DumpDevicesToLog();
                    }
                }
                //_log.Info($"@ MonitorInfoUpdated: MonitorName=[{e.MonitorName}], MonitorType=[{e.MonitorType}], MonitorStatus=[{e.MonitorStatus}]");
            }
            else
            {
                if (e == null)
                    _log.Info($"@ MonitorInfoUpdated(): e is null");
                else if (e.monitor == null)
                    _log.Info($"@ MonitorInfoUpdated(): e.monitor is null");
                else
                    _log.Info($"@ MonitorInfoUpdated(): e.edid is null");
            }
        }

        private void CloseQAMIfExist()
        {
            _deviceManager!.SetIsDDPMHomepageReadyAsync(true);
        }

        //private void CheckIfNeedNavigateToWebcamPage()
        //{
        //    if (DdpmCommonHelper.DeviceManagerSA!.GetIsDDPMLaunchByQAM().Result == true)
        //    {
        //        DdpmCommonHelper.WriteUILog($"GetIsDDPMLaunchByQAM = true");

        //        DdpmCommonHelper.DeviceManagerSA!.SetIsDDPMHomepageReadyAsync(true);
        //        DdpmCommonHelper.DeviceManagerSA!.SetIsDDPMLaunchByQAMAsync(false);
        //    }
        //    else
        //        DdpmCommonHelper.WriteUILog($"GetIsDDPMLaunchByQAM = false");
        //}

        private async Task CheckIfNeedNavigateToSettingPageByQAMOSD()
        {
            if (_deviceManager!.GetIsDDPMLaunchByQAM().Result == true)
            {
                _log.Info($"GetIsDDPMLaunchByQAM = true");

                await _deviceManager!.SetIsDDPMHomepageReadyAsync(true);
                //await _deviceManager!.SetIsDDPMLaunchByQAMAsync(false);
            }
            else
                _log.Info($"GetIsDDPMLaunchByQAM = false");
        }

        private async Task DDPMInfoSAHomepageIsReady()
        {
            try
            {
                _log.Info("DDPMInfoSAHomepageIsReady start");
                await _deviceManager!.SetIsDDPMHomepageReadyAsync(true);
                _log.Info("DDPMInfoSAHomepageIsReady end");
            }
            catch (Exception e)
            {
                _log.Info($"Catch excepton: {e.Message} when Clsoe QAM");
                throw;
            }
        }

        #region Device Changed
        private async void _deviceManager_DeviceChanged(object? sender, DeviceChangedEventArgs e)
        {
            _log.Info("DdpmHomePlugin._deviceManager_DeviceChanged() executed");
            try
            {
                if ((e != null) && !string.IsNullOrEmpty(e.changedProperty))
                {
                    // << 25-04-14 added by Hess to tune Unpair peripheral performance
                    // seperate Peripherals_UnPlug event
                    if (e.type == DeviceChangedType.Peripherals_UnPlug && _deviceManager != null)
                    {
                        _log.Info("DdpmHomePlugin.DeviceChanged.Peripherals_UnPlug start...");
                        _deviceInfos = _deviceManager.GetDevices().Result.deviceInfo;
                        _viewModel?.PrepareMonitorInfos(_monitorInfos);
                        _viewModel?.PrepareDeviceInfos(_deviceInfos);
                        _log.Info("DdpmHomePlugin.DeviceChanged.Peripherals_UnPlug end...");
                        return;
                    }

                    //Robert_Lin, 2024-7-22 log info
                    _log.Info($"@ ChangedProperty=[{e.changedProperty}], ChangedType=[{e.type}] DeviceID=[{e.deviceID}]");

                    //Robert_Lin, 2025-4-12, If the changedProperty is Battery related, then call DeviceChanged_Battery to handle.
                    if (e.changedProperty.Equals("BatteryStatusChanged", StringComparison.OrdinalIgnoreCase) ||
                        e.changedProperty.Equals("BatteryLevelChanged", StringComparison.OrdinalIgnoreCase))
                    {
                        _ = Task.Run(() => { DeviceChanged_Battery(e); });
                        return;
                    }

                    //2024-8-6 Robert, fix bug. compare string should be lowercase due to ToLower()
                    //2024-07-02, Elie, we only handle remove and add event on the DdpmHomePlugin.
                    var lowerChangedProperty = e.changedProperty.ToLower();
                    bool isAddOrRemove = lowerChangedProperty.Contains("remove") || lowerChangedProperty.Contains("add");

                    if (isAddOrRemove ||
                        lowerChangedProperty.Contains("batterystatuschanged") || //Robert_Lin 2025-4-12, both battery* check can be removed.
                        lowerChangedProperty.Contains("batterylevelchanged") ||
                        string.Equals(e.changedProperty, "DisplayChanged", StringComparison.OrdinalIgnoreCase))
                    {
                        if (isAddOrRemove)
                        {
                            _deviceChangedDebounceCts?.Cancel();
                            _deviceChangedDebounceCts = new CancellationTokenSource();
                            var token = _deviceChangedDebounceCts.Token;

                            try
                            {
                                await Task.Delay(500, token);
                                _log.Info("DdpmHomePlugin._deviceManager_DeviceChanged() after Task.Delay");
                            }
                            catch (TaskCanceledException)
                            {
                                _log.Info($"DdpmHomePlugin._deviceManager_DeviceChanged() TaskCanceledException executed : {lowerChangedProperty}");
                                return;
                            }
                        }

                        if (_deviceManager == null)
                        {
                            _log.Info($"[DdpmHomePlugin] {nameof(_deviceManager_DeviceChanged)} _deviceManager is null, return");
                            return;
                        }
                        _monitorInfos = _deviceManager.GetMonitors().Result;          // change to restore monitor caches
                        _log.Info($"[DdpmHomePlugin] {nameof(_deviceManager_DeviceChanged)} after GetMonitors");

                        _deviceInfos = _deviceManager.GetDevices().Result.deviceInfo; // change to restore device caches
                        _log.Info($"[DdpmHomePlugin] {nameof(_deviceManager_DeviceChanged)} after GetDevices");

                        // If event Contains Add, then into Walkthrough
                        _log.Info($"[Walkthrough] {nameof(_deviceManager_DeviceChanged)} {e.changedProperty.ToString()} Start");
                        await CollectAndCompareDevicesAsync();
                        //// Check Queue¡Afirst use device need to show WalkThroughPage
                        if (WalkThroughQueue.Count > 0 && ShowPluginById == false)
                        {
                            _log.Info($"[Walkthrough] {nameof(_deviceManager_DeviceChanged)} WalkThroughQueue has items, ShowPluginById.");
                            _showPluginManager?.ShowPluginById(DDPM.UI.Common.Constants.WalkThroughPluginId);
                            ShowPluginById = true;
                        }

                        //Force return to HomePage
                        // 2024-06-19 From Dean, using DeviceChangedType.NotifyOnly to check if it's a monitor settings change.

                        //2024-6-20 move refresh device form HomeView to here
                        //_ = Task.Run(GetDdpmDevicesAsync(_deviceManager));
                        if (_deviceManager != null)
                        {
                            _log.Info($"[DdpmHomePlugin] _deviceManager_DeviceChanged call GetDdpmDevicesAsync ... ");
                            _ = GetDdpmDevicesAsync(_deviceManager, e, e.changedProperty.ToLower());
                            _log.Info($"[DdpmHomePlugin] _deviceManager_DeviceChanged return GetDdpmDevicesAsync ... ");
                        }

                        if (e.type == DeviceChangedType.NotifyOnly && WalkThroughQueue.Count == 0)
                        {
                            _log.Info($"CALL ShowDdpmHome(), when e.type == DeviceChangedType.NotifyOnly.");
                            ShowDdpmHome();
                        }
                        else
                        {
                            if (_isActived) // 2024-07-04 Elie, for Peripheral and when at HomepagePlugin already.
                            {
                                _log.Info($"CALL ShowDdpmHome(), when _isActived.");
                                ShowDdpmHome();
                            }
                        }

                        MonitorInfo newPlugIn = null;
                        // If is a plugin event
                        if (_monitorInfos.Count >= _monitorCache.Count)
                        {
                            foreach (MonitorInfo monitor in _monitorInfos)
                            {
                                if (!_monitorCache.Contains(monitor))
                                {
                                    newPlugIn = monitor;
                                    break;
                                }
                            }
                        }
                        // Always save cache for monitor when event launched
                        _monitorCache = _monitorInfos;
                        if (newPlugIn != null)
                            CheckIfNeedImportSetting_Display(new List<MonitorInfo>() { newPlugIn });
                    }
                    else
                    {
                        _log.Info($"DdpmHomePlugin._deviceManager_notifyDeviceDisConnecte() skip : [{e.changedProperty.ToString()}]");
                    }
                }
                else
                {
                    if ((e == null))
                    {
                        _log.Info($"DdpmHomePlugin._deviceManager_notifyDeviceDisConnecte() skip : e == null");
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(e.changedProperty))
                            _log.Info($"DdpmHomePlugin._deviceManager_notifyDeviceDisConnecte() skip : e.changedProperty == null");
                        else
                            _log.Info($"DdpmHomePlugin._deviceManager_notifyDeviceDisConnecte() skip : e.changedProperty : {e.changedProperty}");
                    }
                }
                _log.Info("DdpmHomePlugin._deviceManager_DeviceChanged() out ... ");
            }
            catch (Exception ex)
            {
                _log.Info($"DdpmHomePlugin._deviceManager_DeviceChanged() Exception {ex.Message} ... ");
                if (_viewModel != null && _viewModel.IsPleaseWaitVisible)
                {
                    _viewModel.IsPleaseWaitVisible = false;
                }
                ShowDdpmHome();
            }
        }

        /// <summary>
        /// Handle the Battery changed (Charging/Level).
        /// </summary>
        /// <returns></returns>
        private async Task DeviceChanged_Battery(DeviceChangedEventArgs e)
        {
            _log.Info($"@ DeviceChanged_Battery(), ID={e.deviceID}, ChangedProperty={e.changedProperty}");
            //Find the target device
            if (e.device_peripherals == null)
            {
                _log.Info($"@ DeviceChanged_Battery(), device_peripherals is null");
                return;
            }
            Guid guid = e.device_peripherals.ID;
            HomeDevice? homeDev = _viewModel.FindPeripheralByGuid(guid);
            if (homeDev != null)
            {
                homeDev.DeviceInfo = e.device_peripherals;
                //TO DO: Raise an event to notify event handlers, or the Peripheral plugin
                //should handle the event firectly from DeviceManagerSA
            }

        }
        #endregion Device Canged

        private void CheckIfNeedImportSetting_Display(List<MonitorInfo> monitorInfos)
        {
            //For existing monitor to check if need to pop-up message to import setting
            Task.Run(() =>
            {
                _log.Info($"[DdpmHomePlugin] CheckIfNeedImportSetting_Display Task.Run in ... ");
                if (monitorInfos == null || monitorInfos.Count == 0)
                    return;
                //make sure no walkthrough page displaying
                int tick = 0;
                while (WalkThroughQueue != null && WalkThroughQueue.Count > 0)
                {
                    Task.Delay(5000).Wait();
                    tick++;
                    _log.Info($"[DdpmHomePlugin] CheckIfNeedImportSetting_Display, 5 sec x {tick.ToString()}... ");
                }
                string localAppDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Dell");
                string path = localAppDataPath + "\\Dell Display and Peripheral Manager\\Export";

                _log.Info($"[CheckIfNeedImportSetting_Display] Monitor count is {monitorInfos.Count}.");
                foreach (MonitorInfo info in monitorInfos)
                {
                    _log.Info($"[CheckIfNeedImportSetting_Display] Monitor ServiceTag: {info.edid.ServiceTag} Model: {info.modelName} search for previous exported data.");
                    string model = info.modelName;//"U2724DE";
                    string serviceTag = info.edid.ServiceTag;
                    string exportpath = path + "\\" + model + ".json";
                    _log.Info("[CheckIfNeedImportSetting_Display] export path : " + exportpath);
                    //if(can popup messagebox && not yet to import / already click no need import)
                    if (File.Exists(exportpath))
                    {
                        //force return here to avoid page trigger, need Jason handle it
                        //return;
                        if (_viewModel != null)
                        {
                            _viewModel.InvokeImportQuestion(info);
                        }
                    }
                }
                _log.Info($"[DdpmHomePlugin] CheckIfNeedImportSetting_Display Task.Run end ... ");
            });
        }

        private void _deviceManager_notifyDeviceDisConnected(object? sender, EventArgs e)
        {
            _log.Info("DdpmHomePlugin._deviceManager_notifyDeviceDisConnecte() executed");
            //throw new NotImplementedException();
        }

        private void _deviceManager_notifyDeviceConnected(object? sender, EventArgs e)
        {
            _log.Info("DdpmHomePlugin._deviceManager_notifyDeviceConnected() executed");
            //throw new NotImplementedException();
        }
        private void _deviceManager_UIUpdateNotify(object? sender, UpdateUINotify e)
        {
            //Derek 1127
            if (e == null || e.UI_Field_Name == null || e == UpdateUINotify.Empty)
                return;

            _log.Info($"_deviceManager_UIUpdateNotify received msg is {e.UI_Field_Name}");
            //System.Windows.MessageBox.Show(e.UI_Field_Name);

            if (e.UI_Field_Name.StartsWith("QAMEvent_StartPreview"))
            {
                //_log.Info($"_deviceManager_UIUpdateNotify start to show webcam preview");
                _console.ShowPluginById(DDPM.UI.Common.Constants.WebCameraPluginId);
                //_showPluginManager?.ShowPluginById(DDPM.UI.Common.Constants.SettingsPluginId);
                //_console.ShowPluginById(DDPM.UI.Common.Constants.SettingsPluginId);
            }
            else if (e.UI_Field_Name.StartsWith("QAMEvent_QAMIsLaunched"))
            {
                CloseMyself();
            }
            else if (e.UI_Field_Name.StartsWith("QAMEvent_NavigateToWidgetSettingPage"))
            {
                _console.ShowPluginById(DDPM.UI.Common.Constants.SettingsPluginId);
                DdpmCommonHelper.IsDDPMSwitchToSettingPageByQAM = true;
            }
        }

        [DllImport("user32.dll")]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        private static extern int SendMessage(IntPtr hWnd, int wMsg, IntPtr wParam, IntPtr lParam);
        private static int _SendMessage(IntPtr hWnd, int wMsg, IntPtr wParam, IntPtr lParam)
        {
            return SendMessage(hWnd, wMsg, wParam, lParam);
        }
        private const int WM_EXITBYMYSELF = 0xFF30;
        private void CloseMyself()
        {
            try
            {
                IntPtr hWnd = Process.GetCurrentProcess().MainWindowHandle;
                int result = _SendMessage(hWnd, WM_EXITBYMYSELF, IntPtr.Zero, IntPtr.Zero);

                _log.Info($"SendMessage result = {result}");
            }
            catch (Exception e)
            {
                _log.Info($"Catch exception: {e.Message} when CloseMyself");
            }

            _log.Info($"Run CloseMyself successfully.");
        }

        #region DeviceManager event handlers
        private void RegisterDeviceManagerEvents()
        {
            if (_deviceManager != null)
            {
                if (!_HasRegisted)
                {
                    _HasRegisted = true;
                    _deviceManager.DeviceChanged += _deviceManager_DeviceChanged;
                    _deviceManager.VCPchanged += _deviceManager_VCPchanged;
                    _deviceManager.UIUpdateNotify += _deviceManager_UIUpdateNotify;
                    _deviceManager.MonitorinfoUpdated += _deviceManager_MonitorinfoUpdated;
                }
            }
        }
        private void UnregisterDeviceMnagerEvents()
        {
            if (_deviceManager != null)
            {
                if (_HasRegisted)
                {
                    _HasRegisted = false;
                    _deviceManager.DeviceChanged -= _deviceManager_DeviceChanged;
                    _deviceManager.VCPchanged -= _deviceManager_VCPchanged;
                    _deviceManager.UIUpdateNotify -= _deviceManager_UIUpdateNotify;
                    _deviceManager.MonitorinfoUpdated -= _deviceManager_MonitorinfoUpdated;
                }
            }
        }
        #endregion DeviceManager event handers
        private void _deviceManager_VCPchanged(object? sender, VCPchangedEventArgs e)
        {
            string monitorName = "";
            string vcpCode = "";
            if (e.monitor != null &&
                e.monitor.AliasDeviceName != null)
            {
                monitorName = e.monitor.AliasDeviceName;
            }
            if (e.vcpcode != null)
                vcpCode = e.vcpcode;
            _log.Info($"DdpmHomePlugin._deviceManager_VCPchanged() executed, Monitor=[{monitorName}], VcpCode=[{e.vcpcode}], Value=[{e.value}]");
            //throw new NotImplementedException();
        }

        private void _IDeviceManagerPluginCondition_PluginConditionChangeHandler(object? sender, EventArgs e)
        {
            _ = Task.Run(GetCurrentDeviceManagerPluginPluginCondition, CancellationToken);
        }

        //Unused, Use GetDdpmDevicesAsync() instead
        //private async Task GetMonitorsAsync(IDeviceManagerSA deviceManager)
        //{
        //    if (!SpinWait.SpinUntil(() =>
        //    (_IDeviceManagerPluginCondition is IFrameworkPluginConditionNotification), TimeSpan.FromMinutes(2)))
        //    {
        //        Console.WriteLine("Could not establish communication with DeviceManager plugin!!");
        //        return;
        //    }
        //    _log.Debug($"GetMonitorsAsync is invoked");
        //    //_displayService = await deviceManager.GetDisplayServiceInterface(); Robert0502
        //    if (deviceManager != null)
        //    {
        //        List<MonitorInfo> mo = await deviceManager.GetMonitors();
        //        _log.Debug($"GetMonitor count is ${mo.Count}");

        //        Interfaces.IDdpmHomePageViewModel? viewModel = PluginIoc.GetService<Interfaces.IDdpmHomePageViewModel>();
        //        if (viewModel != null)
        //        {
        //            if (_deviceManager != null)
        //                if (mo != null)
        //                {
        //                    foreach (MonitorInfo info in mo)
        //                    {
        //                        HomeDevice dev = new HomeDevice()
        //                        {
        //                            DeviceName = info.AliasDeviceName,
        //                            DeviceCategory = eDeviceCategory.Display,
        //                            MonitorInfo = info,
        //                            //Text1 = currentInput,
        //                            DeviceImage = DdpmCommonHelper.GetImageSourceFromCommonResource("Resources/Product_Display.png")
        //                        };
        //                        //HomeDevices.Add(dev);
        //                    }
        //                }
        //            viewModel.PrepareMonitorInfos(mo);
        //        }
        //    }
        //}

        //device caches, Dean 1018 add
        private static List<DeviceInfo> _deviceInfos = null;
        private static List<MonitorInfo> _monitorInfos = null;

        private async Task GetDdpmDevicesAsync(IDeviceManagerSA deviceManager, DeviceChangedEventArgs e = null, string condition = "all")
        {
            try
            {
                _log.Info($"[DdpmHomePlugin] GetDdpmDevicesAsync {condition} in ...");
                if (!SpinWait.SpinUntil(() =>
                (_IDeviceManagerPluginCondition is IFrameworkPluginConditionNotification), TimeSpan.FromMinutes(2)))
                {
                    Console.WriteLine("Could not establish communication with DeviceManager plugin!!");
                    _log.Info($"[DdpmHomePlugin] GetDdpmDevicesAsync Could not establish communication with DeviceManager plugin!!");
                    return;
                }
                _log.Info("[DdpmHomePlugin] GetDdpmDevicesAsync is invoked");

                //_displayService = await deviceManager.GetDisplayServiceInterface(); Robert0502
                if (deviceManager != null)
                {
                    DdpmCommonHelper.DeviceManagerSA = deviceManager;
                    DdpmCommonHelper.Log = this._log;//assign this log for global using
                    DdpmCommonHelper.ReadDDPMSettings(true);//.Settings_Cache = deviceManager.ReloadAppConfigData().Result;
                                                            //List<MonitorInfo> monitorInfos = deviceManager.GetMonitors().Result;
                    if (condition.Equals("all") || condition.Equals("displaychanged"))
                    {
                        if (_monitorInfos == null)
                        {
                            _monitorInfos = deviceManager.GetMonitors().Result;
                            _log.Info($"[DdpmHomePlugin] GetDdpmDevicesAsync GetMonitors count is ${_monitorInfos.Count}");
                        }
                        if (_deviceInfos == null)
                        {
                            _deviceInfos = deviceManager!.GetDevices().Result.deviceInfo;
                            _log.Info($"[DdpmHomePlugin] GetDdpmDevicesAsync GetDevices count is ${_deviceInfos.Count}");
                        }
                        _log.Info($"[Walkthrough] CollectAndCompareDevicesAsync, monitor count:{_monitorInfos.Count.ToString()}, device count : {_deviceInfos.Count.ToString()}");
                    }
                    _ = Task.Run(() =>
                    {
                        _log.Info($"[DdpmHomePlugin] GetDdpmDevicesAsync Task.Run {_isConfigured.ToString()} ...");
                        while (!_isConfigured)
                            ;
                        _log.Info($"[DdpmHomePlugin] GetDdpmDevicesAsync Task.Run {_isConfigured.ToString()} ...");
                        Interfaces.IDdpmHomePageViewModel? viewModel = PluginIoc.GetService<Interfaces.IDdpmHomePageViewModel>();
                        if (viewModel != null)
                        {
                            //Robert_Lin, 2024-12-16, PIMS-329606 Observe no device connected manu flash on disconnect - connect device.
                            //To prevent "Add your first device" (flash) show, we will show Please wait before clear all devices
                            viewModel.IsPleaseWaitVisible = true;
                            _log.Info($"[DdpmHomePlugin] GetDdpmDevicesAsync IsPleaseWaitVisible true ...");

                            viewModel.ResetDevices();

                            _log.Info("[DdpmHomePlugin] GetDdpmDevicesAsync Adding Monitors to HomePageViewModel...");

                            viewModel.PrepareMonitorInfos(_monitorInfos);

                            _log.Info("[DdpmHomePlugin] GetDdpmDevicesAsync Adding Periphrals to HomePageViewModel...");
                            viewModel.PrepareDeviceInfos(_deviceInfos);

                            //Robert_Lin, 2024-8-5 for PIMS-289060, display a "Please wait" UI before devices ready
                            if (viewModel.HomeDevices.Count == 0)
                            {
                                //Robert_Lin, 2024-12-16, PIMS-329606 Observe no device connected manu flash on disconnect - connect device.
                                //Invoke_PleaseWait() will set IsPleaseWaitVisible=true again, and reset to false, when device count>0
                                // or time out.
                                //The "PleaseWait" UI will be displayed and auto closed after timeout (=12 sec)
                                _log.Info($"[DdpmHomePlugin] GetDdpmDevicesAsync Invoke_PleaseWait");
                                viewModel.Invoke_PleaseWait();
                                //Robert_Lin, 2025-1-7, the DDPMDebug.txt solution will be removed, use DevSettings instaed.
                                //NEW:
                                if (DevSettings.DdpmHomeAddFakeMonitorIfHomeDevicesEmpty())
                                {
                                    //Add a Fake monitor to the listView of Homepage
                                    _viewModel?.AddFakeMonitorToListView();
                                    _log.Info($"[DdpmHomePlugin] GetDdpmDevicesAsync AddFakeMonitorToListView");
                                }
                                //OLD:
                                ////Robert_Lin, 2024-11-9 for Developer debug, check if C:\temp\DDPMDebug.txt contains
                                ////[DDPMDebug]
                                ////HomePlugin.GetDdpmDevicesAsync.AddFakeMonitorIfEmpty=1
                                //if (File.Exists(@"C:\temp\DDPMDebug.txt"))
                                //{
                                //    if (User32.IniReadInt("DDPMDebug", "HomePlugin.GetDdpmDevicesAsync.AddFakeMonitorIfEmpty", 0, @"C:\temp\DDPMDebug.txt") == 1)
                                //    {
                                //        //Add a Fake monitor to the listView of Homepage
                                //        _viewModel?.AddFakeMonitorToListView();
                                //    }
                                //}

                            }
                            else
                            {
                                viewModel.IsPleaseWaitVisible = false;
                                _log.Info($"[DdpmHomePlugin] GetDdpmDevicesAsync IsPleaseWaitVisible false ...");
                                viewModel.DumpDevicesToLog();
                            }
                        }
                    });
                }
                else
                {
                    _log.Info($"[DdpmHomePlugin] GetDdpmDevicesAsync IDeviceManagerSA is null");
                }
                _log.Info($"[DdpmHomePlugin] GetDdpmDevicesAsync out ... ");
            }
            catch (Exception ex)
            {
                _log.Error($"[DdpmHomePlugin] GetDdpmDevicesAsync  Exception: {ex.Message}");
            }
        }

        /// <summary>
        /// WalkThrough Sort
        /// </summary>
        /// <param name="device"></param>
        /// <returns></returns>
        private async Task DeviceSort(WalkThroughInfo device)
        {
            _log.Info($"[Walkthrough] DeviceSort {device.ModelName} with ModelType {device.ModelType} Start");

            // Sort
            WalkThroughQueue.Sort((device1, device2) =>
            {
                int device1Order = ModelTypeMapping.ContainsKey(device1.ModelType) ? ModelTypeMapping[device1.ModelType] : int.MaxValue;
                int device2Order = ModelTypeMapping.ContainsKey(device2.ModelType) ? ModelTypeMapping[device2.ModelType] : int.MaxValue;
                return device1Order.CompareTo(device2Order);
            });

            _log.Info($"[Walkthrough] DeviceSort End.");
        }
        //Unused
        /// <summary>
        /// Method to show <see cref="DdpmHomePage"/>
        /// </summary>
        private void ShowDdpmHome()
        {
            _log.Info($"{nameof(DdpmHomePage)} - shown");
            _console.ShowPluginById(PluginId);
        }

        /// <summary>
        /// Initialize or register services
        /// </summary>
        /// <remarks>Below code will be removed when <see cref="IConsole"/> provides the bootstrapper support</remarks>
        private void ConfigureServices()
        {
            _log.Info($"[DdpmHomePlugin] ConfigureServices in ...");
            if (_isConfigured)
                return;

            // Marked all the instances as singleton
            // Pass the existing _console and _log instance so that Ioc doesn't new'up them
            PluginIoc.ConfigureServices(new ServiceCollection()
                .AddSingleton(_showPluginManager)
                .AddSingleton(_console)
                .AddSingleton(_log)
                .AddSingleton<IDdpmHomePageViewModel, DdpmHomePageViewModel>()
                .BuildServiceProvider());

            _viewModel = (DdpmHomePageViewModel?)PluginIoc.GetService<IDdpmHomePageViewModel>();
            if (_viewModel != null)
            {
                _viewModel.DTHServiceStopped += _viewModel_DTHServiceStopped;
                _log.Info($"[DdpmHomePlugin] ConfigureServices Invoke_PleaseWait");
                _viewModel.Invoke_PleaseWait();
            }
            _isConfigured = true;
            _log.Info($"[DdpmHomePlugin] ConfigureServices _isConfigured = true, out ...");
        }

        private void _viewModel_DTHServiceStopped()
        {
            //_console?.RaiseEvent(ConsoleEventNames.Masthead_ShowAddDeviceIcon, this, new EventManagerArgs() { Tag = new List<bool> { false, false } });
            //_console?.RaiseEvent(ConsoleEventNames.Masthead_ShowSettingsIcon, this, new EventManagerArgs() { Tag = new List<bool> { false, false } });

            UnregisterDeviceMnagerEvents();
            _console?.ShowPluginById(DDPM.UI.Common.Constants.ExitAppPluginId);
        }

        #region Interface IConsolePluginSupportsActivations

        /// <inheritdoc/>
        public void OnActivated()
        {
            _log.Info($"[DdpmHomePlugin] OnActivated ...");
            //_deviceManager.DeviceChanged += _deviceManager_DeviceChanged;
            Mouse.OverrideCursor = null;
            _isActived = true;
        }

        /// <inheritdoc/>
        public void OnDeactivated()
        {
            _log.Info($"[DdpmHomePlugin] OnDeactivated ...");
            //_deviceManager.DeviceChanged -= _deviceManager_DeviceChanged;
            Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
            _isActived = false;
        }

        /// <inheritdoc/>
        public void OnShown(string pluginParameter = "")
        {
            _log.Info($"[DdpmHomePlugin] OnShown in ...");
            if (!string.IsNullOrEmpty(pluginParameter))
            {
                IDdpmHomePageViewModel? viewModel = PluginIoc.GetService<IDdpmHomePageViewModel>();
                if (viewModel != null)
                {
                    //Robert_Lin, 2024-12-16 fix, once you clear HomeDevices, the "Add your first device"
                    //will be visible immediately. We need set ViewModel.IsPleaseWaitVisible=true to show
                    //the Please wait
                    viewModel.IsPleaseWaitVisible = true;
                    _log.Info($"[DdpmHomePlugin] OnShown IsPleaseWaitVisible true ...");
                    //Robert_Lin, 2024-12-16
                    //NEW:
                    viewModel.ResetDevices();
                    _log.Info($"[DdpmHomePlugin] OnShown ResetDevices ...");
                    DdpmCommonHelper.DeviceManagerSA.ReGetMonitors();
                    //OLD:
                    //viewModel.HomeDevices = new System.Collections.ObjectModel.ObservableCollection<HomeDevice>();

                    //After HomeDevices are cleared, Add your first device will not show, because that we has set
                    // IsPleaseWaitVisible to true.

                    //Robert_Lin, 2024-12-16 Not sure if we should invokd PleaseWait??
                    //If the device will be reconnect soon, then we don't need to invoke it.
                    //But if we clear the devices, but no a DeviceChanged later, then we need to invoke it.
                    _log.Info($"[DdpmHomePlugin] OnShown Invoke_PleaseWait");
                    viewModel.Invoke_PleaseWait();
                }
            }
            ConfigureServices();
            Mouse.OverrideCursor = System.Windows.Input.Cursors.Arrow;
            Mouse.OverrideCursor = null;
            DdpmCommonHelper.MyConsole = PluginIoc.GetService<IConsole>();
            DdpmCommonHelper.MyShowPluginManager = PluginIoc.GetService<IShowPluginManager>();

            //Robert_Lin 2025-3-5 Change to new method instead
            ShowAllMastheadIcons();
            ////Robert_Lin, 2024-7-17, fix PIMS-286435 in AddDevice menu, the AddDevice icon is in Top Right side.
            //if (_iconAddDevice != null)
            //    _iconAddDevice.Visibility = Visibility.Visible;
            //if (_iconGear != null)
            //    _iconGear.Visibility = Visibility.Visible;

            _log.Info($"[DdpmHomePlugin] OnShown out ...");
        }

        [Obsolete]
        public void AddTileToHomePage(TileModel tileModel, int position, Guid? region = null)
        {
            //throw new NotImplementedException();
        }

        [Obsolete]
        public TileModel? FindTileOnHomePage(string titleText, Guid? region = null)
        {
            //throw new NotImplementedException();
            return null;
        }

        [Obsolete]
        public TileModel? FindTileOnHomePageAtPosition(int position, Guid? region = null)
        {
            //throw new NotImplementedException();
            return null;
        }

        [Obsolete]
        public void RemoveTileFromHomePage(TileModel tileModel, Guid? region = null)
        {
            //throw new NotImplementedException();
        }

        public Task<bool> AddContentAsync(FrameworkElement content)
        {
            //throw new NotImplementedException();
            return Task.FromResult(false);
        }

        #endregion Interface IConsolePluginSupportsActivations

        #region HomeDevices
        /// <summary>
        /// Called from other plugins to get the HomeDevices from DdpmHomePlugin's ViewModel
        /// </summary>
        /// <returns></returns>
        public static List<HomeDevice> GetHomeDevices()
        {
            IDdpmHomePageViewModel? viewModel = PluginIoc.GetService<IDdpmHomePageViewModel>();
            if (viewModel != null)
            {
                return viewModel.HomeDevices.ToList();
            }
            return new List<HomeDevice>();
        }

        /// <summary>
        /// Called from other plugins to get the SelectedHomeDevices from DdpmHomePlugin's ViewModel
        /// For the modules of DisplayPlugin, can get this list from their IModuleOwner 
        /// because that DisplayPlugin will call this method and copy/update to IModuleOwner.
        /// NOTE. for Display, this return object is the one user click from homepage.
        /// But not the selected item from Display Landing Page combobox.
        /// </summary>
        /// <returns></returns>
        public static HomeDevice? GetSelectedHomeDevice()
        {
            IDdpmHomePageViewModel? viewModel = PluginIoc.GetService<IDdpmHomePageViewModel>();
            if (viewModel != null)
            {
                return viewModel.SelectedHomeDevice;
            }
            else
                return null;
        }
        #endregion

        #region Icons on Masthead

        //Robert_Lin, 2024-7-17, fix PIMS-286435 in AddDevice menu, the AddDevice icon is in Top Right side.
        //Expected behavior: When entering AddDevice plugin, the 'AddDevice' icon on Masthead should be hidden.
        //  and show back when exiting from AddDevice plugin.
        //  It should have the same behavior for 'Gear' icon.
        private PathIcon? _iconGear = null;
        private GearButton? _gearBtn = null;

        private PathIcon? _iconAddDevice = null;

        private void AddIconsToMasthead(UXMasthead masthead)
        {
            //To get the Narrator strig for the Gear icon
            string narratorString_Gear = "Settings";
            try
            {
                narratorString_Gear = LangHelper.Instance["Settings"];
            }
            catch (Exception e2)
            {
            }

            //Robert_Lin 2025-2-6 two options to choice either one:
            //1 GearBtn (If usingGearBtn=True)         2 PathIcon
            bool usingGearBtn = true;

            if (usingGearBtn)
            {
                if (_gearBtn == null)
                {
                    _gearBtn = new GearButton();
                    _gearBtn.Command = new RelayCommand(OnGearIconClicked);
                    _gearBtn.SetValue(AutomationProperties.NameProperty, narratorString_Gear);
                    masthead.InsertCustomContent(_gearBtn);
                }
            }
            else //using PathIcon
            {
                //Add Grear icon, click it will call to OnGreaeIconClicked()
                if (_iconGear == null)
                {
                    _iconGear = new PathIcon();
                    //iconGear.Width = 12; iconGear.Height = 12;
                    _iconGear.PathData = "M5.1525 12L6.99 12L7.5 10.4475L8.04 10.2375L9.54 10.9875L10.905 9.6675L10.155 8.1675L10.335 7.6275L12 7.02L12 5.145L10.35 4.6275L10.14 4.0875L10.965 2.43L9.6525 1.11L8.0625 1.965L7.5 1.6875L6.93 -2.21617e-07L5.085 -3.02264e-07L4.545 1.7175L4.0125 1.9575L2.3925 1.2075L1.0875 2.5275L1.9125 4.1175L1.665 4.6725L7.26157e-07 5.205L6.43214e-07 7.1025L1.68 7.62L1.92 8.1675L1.215 9.7125L2.5125 11.0175L4.0125 10.2675L4.545 10.4325L5.1525 12ZM2.6775 10.05L2.175 9.54L2.79 8.1825L2.25 6.9675L0.750001 6.51L0.750001 5.76L2.25 5.25L2.805 4.0275L2.055 2.61L2.55 2.1075L3.9825 2.805L5.205 2.3025L5.67 0.8025L6.3675 0.8025L6.87 2.25L8.0775 2.8575L9.51 2.1075L9.99 2.595L9.24 4.095L9.75 5.25L11.205 5.7075L11.205 6.4575L9.705 6.9825L9.2925 8.205L9.9375 9.51L9.435 10.0125L8.0925 9.375L6.885 9.8475L6.42 11.2125L5.715 11.2125L5.19 9.75L3.9675 9.3825L2.6775 10.0275L2.6775 10.05Z M8.1825 6C8.18102 5.56866 8.05176 5.14744 7.81104 4.78952C7.57033 4.4316 7.22895 4.15303 6.83002 3.98899C6.43109 3.82495 5.9925 3.7828 5.56964 3.86785C5.14677 3.95291 4.75859 4.16136 4.45411 4.46689C4.14963 4.77241 3.94251 5.16131 3.8589 5.58447C3.7753 6.00762 3.81896 6.44607 3.98436 6.84443C4.14977 7.24279 4.42951 7.58321 4.78825 7.8227C5.147 8.06218 5.56866 8.19 6 8.19C6.28724 8.19 6.57166 8.1333 6.83694 8.02315C7.10223 7.913 7.34316 7.75157 7.54592 7.54811C7.74868 7.34465 7.90929 7.10317 8.01852 6.83751C8.12776 6.57185 8.18349 6.28724 8.1825 6ZM4.6275 6C4.63047 5.7274 4.71411 5.46179 4.86787 5.23667C5.02163 5.01156 5.23862 4.83703 5.49146 4.7351C5.7443 4.63318 6.02167 4.60842 6.28857 4.66396C6.55547 4.7195 6.79993 4.85285 6.99113 5.04718C7.18232 5.24151 7.31167 5.48811 7.36287 5.75587C7.41406 6.02364 7.3848 6.30057 7.27878 6.55172C7.17275 6.80287 6.99472 7.017 6.76713 7.16708C6.53955 7.31716 6.27261 7.39647 6 7.395C5.63332 7.39104 5.28311 7.24208 5.02592 6.98068C4.76874 6.71928 4.6255 6.3667 4.6275 6Z";
                    _iconGear.ClickCommand = new RelayCommand(OnGearIconClicked);
                    _iconGear.IsTabStop = true;
                    _iconGear.Focusable = true;
                    _iconGear.SetValue(AutomationProperties.NameProperty, narratorString_Gear);
                    //_iconGear.TabIndex = 0;
                    //_iconGear.IsHitTestVisible = true;
                    masthead.InsertCustomContent(_iconGear);
                    //masthead.IsTabStop = true;
                }
            }



            if (_iconAddDevice == null)
            {
                //Add Add icon, click it will call to OnGreaeIconClicked()
                _iconAddDevice = new PathIcon();
                //iconAdd.Width = 12; iconAdd.Height = 12;
                _iconAddDevice.PathData = "M11.9475 5.25C11.7801 3.92998 11.1787 2.70306 10.2378 1.76219C9.29697 0.821322 8.07004 0.219895 6.75003 0.0525C6.50151 0.0179984 6.25093 0.000457859 6.00003 0C4.4809 0.0103651 3.02227 0.596591 1.91848 1.64037C0.814689 2.68416 0.147927 4.10778 0.0527501 5.62395C-0.042427 7.14012 0.441067 8.63595 1.40566 9.80958C2.37025 10.9832 3.74413 11.7472 5.25003 11.9475C5.49854 11.982 5.74912 11.9995 6.00003 12C6.85082 11.9992 7.69172 11.8175 8.46693 11.4669C9.24214 11.1164 9.93393 10.6049 10.4964 9.96659C11.0588 9.32824 11.4791 8.57756 11.7293 7.76439C11.9795 6.95121 12.0539 6.09412 11.9475 5.25ZM9.90003 9.4425C9.11363 10.3318 8.0465 10.9251 6.87611 11.1237C5.70572 11.3224 4.50261 11.1143 3.46685 10.5343C2.43109 9.95422 1.62511 9.03707 1.18295 7.93536C0.740791 6.83366 0.6891 5.61378 1.03647 4.47862C1.38385 3.34345 2.10935 2.36141 3.09232 1.69581C4.0753 1.03021 5.25651 0.72116 6.4395 0.820065C7.6225 0.918969 8.73599 1.41987 9.5948 2.23945C10.4536 3.05903 11.006 4.14791 11.16 5.325C11.253 6.06139 11.1888 6.80912 10.9716 7.51887C10.7544 8.22862 10.3892 8.88425 9.90003 9.4425Z M6.42753 2.625H5.52752L5.52752 5.55L2.62503 5.55V6.45L5.52752 6.45L5.52752 9.375L6.42753 9.375L6.42753 6.45L9.37503 6.45V5.55L6.42753 5.55L6.42753 2.625Z";
                _iconAddDevice.ClickCommand = new RelayCommand(OnAddIconClicked);
                _iconAddDevice.IsTabStop = true;
                _iconAddDevice.Focusable = true;
                string narrarorString = "Add Device";
                try
                {
                    narrarorString = LangHelper.Instance["AddDevice"];
                }
                catch (Exception e1)
                {
                }
                _iconAddDevice.SetValue(AutomationProperties.NameProperty, narrarorString);
                //_iconAddDevice.TooltipText = "Add device";
                masthead.InsertCustomContent(_iconAddDevice);
            }

            if (_console != null)
            {
                _console.RegisterForEvent(ConsoleEventNames.Masthead_ShowAddDevicePlugin, ShowAddDevicePlugin);
                _console.RegisterForEvent(ConsoleEventNames.Masthead_ShowSettingsPlugin, ShowSettingsPlugin);
                //Robert_Lin, 2025-1-19 comment-out unused events
                //_console.RegisterForEvent(ConsoleEventNames.Masthead_StartGlowEffectOnGearIcon, StartGlowEffectOnGearIcon);
                //_console.RegisterForEvent(ConsoleEventNames.Masthead_StopGlowEffectOnGearIcon, StopGlowEffectOnGearIcon);
                _console.RegisterForEvent(ConsoleEventNames.Masthead_ShowAddDeviceIcon, Handler_ShowAddDeviceIcon);
                _console.RegisterForEvent(ConsoleEventNames.Masthead_ShowSettingsIcon, Handler_ShowSettingsIcon);
            }
        }

        private void OnGearIconClicked()
        {
            EventManagerArgs args = new EventManagerArgs();
            if (_console != null)
                _console.RaiseEvent(ConsoleEventNames.Masthead_ShowSettingsPlugin, this, args);
        }

        private void OnAddIconClicked()
        {
            EventManagerArgs args = new EventManagerArgs();
            if (_console != null)
                _console.RaiseEvent(ConsoleEventNames.Masthead_ShowAddDevicePlugin, this, args);
        }

        private void ShowAddDevicePlugin(object sender, EventManagerArgs e)
        {
            ShowAllMastheadIcons(); //PIMS-293574
            //Robert_Lin, 2024-7-17, fix PIMS-286435 in AddDevice menu, the AddDevice icon is in Top Right side.
            if (_iconAddDevice != null)
                _iconAddDevice.Visibility = Visibility.Collapsed;
            _console.ShowPluginById(UI.Common.Constants.AddDevicePluginId);
        }

        private void ShowSettingsPlugin(object sender, EventManagerArgs e)
        {
            ShowAllMastheadIcons(); //PIMS-293574
            //Robert_Lin, 2024-7-17, fix PIMS-286435 in AddDevice menu, the AddDevice icon is in Top Right side.
            if (_iconGear != null)
                _iconGear.Visibility = Visibility.Collapsed;
            if (_gearBtn != null)
                _gearBtn.Visibility = Visibility.Collapsed;

            if (_IsAnyUpdate)
                _showPluginManager?.ShowPluginById(DDPM.UI.Common.Constants.SettingsPluginId, "1");
            else
                _console.ShowPluginById(UI.Common.Constants.SettingsPluginId);
        }

        //Robert_Lin 2024-8-2 added for DDMPW-579 story
        //To trigger this event:
        //  IConsole.RaiseEvent("StartGlowEffectOnGearIcon", this, new EventManagerArgs());
        private void StartGlowEffectOnGearIcon(object sender, EventManagerArgs e)
        {
            //if (_iconGear != null)
            //    _iconGear.GlowEffect_Start();
        }

        //To trigger this event:
        //  IConsole.RaiseEvent("StopGlowEffectOnGearIcon", this, new EventManagerArgs());
        private void StopGlowEffectOnGearIcon(object sender, EventManagerArgs e)
        {
            //if (_iconGear != null)
            //    _iconGear.GlowEffect_Stop();
        }

        //Robert_Lin 2024-8-23 added for PIMS-293574
        // When exit from AddDevicePlugin and GlobalSettingsPlugin, we should reshow the Masthead icons
        private void ShowAllMastheadIcons()
        {
            if (_gearBtn != null)
                _gearBtn.Visibility = Visibility.Visible;
            if (_iconGear != null)
                _iconGear.Visibility = Visibility.Visible;
            if (_iconAddDevice != null)
                _iconAddDevice.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Show/Hide the AddDevice icon
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">Put a bool value to e.Tag, True=Show; False=Hide</param>
        private void Handler_ShowAddDeviceIcon(object sender, EventManagerArgs e)
        {
            if (e.Tag != null &&
                e.Tag is List<bool>)
            {
                List<bool> param = (List<bool>)e.Tag;
                bool isShow = param[0];
                if (isShow)
                {
                    if (_iconAddDevice != null)
                        _iconAddDevice.Visibility = Visibility.Visible;
                }
                else
                {
                    if (_iconAddDevice != null)
                    {
                        Dispatcher.CurrentDispatcher.InvokeAsync(() => {
                            _iconAddDevice.Visibility = Visibility.Collapsed;
                        }, DispatcherPriority.Loaded);
                    }
                }


                bool isEnabled = param[1];
                if (isEnabled)
                {
                    if (_iconAddDevice != null)
                        _iconAddDevice.IsEnabled = isEnabled;
                }
                else
                {
                    if (_iconAddDevice != null)
                    {
                        Dispatcher.CurrentDispatcher.InvokeAsync(() => {
                            _iconAddDevice.IsEnabled = isEnabled;
                        }, DispatcherPriority.Loaded);
                    }
                }
            }
        }

        /// <summary>
        /// Show/Hide the Settings icon
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">Put a bool value to e.Tag, True=Show; False=Hide</param>
        private void Handler_ShowSettingsIcon(object sender, EventManagerArgs e)
        {
            if (e.Tag != null &&
                e.Tag is List<bool>)
            {
                List<bool> param = (List<bool>)e.Tag;
                bool isShow = param[0];
                if (isShow)
                {
                    if (_gearBtn != null)
                        _gearBtn.Visibility = Visibility.Visible;
                    if (_iconGear != null)
                        _iconGear.Visibility = Visibility.Visible;
                }
                else
                {
                    Dispatcher.CurrentDispatcher.InvokeAsync(() => {
                        if (_gearBtn != null)
                            _gearBtn.Visibility = Visibility.Collapsed;
                        if (_iconGear != null)
                            _iconGear.Visibility = Visibility.Collapsed;
                    }, DispatcherPriority.Loaded);

                }

                bool isEnabled = param[1];

                Dispatcher.CurrentDispatcher.InvokeAsync(() => {
                    if (_gearBtn != null)
                        _gearBtn.IsEnabled = isEnabled;
                    if (_iconGear != null)
                        _iconGear.IsEnabled = isEnabled;
                }, DispatcherPriority.Loaded);

            }
        }

        #endregion Icons on Masthead

        #region IDispose

        //Robert_Lin, 2024-6-21, when HomePlugin exit, tell VCPCore restore to low polling rate
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose unmanaged resources
        /// </summary>
        /// <param name="disposing">bool</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (_log != null)
                _log.Info("DdpmHomePlugin Dispose(bool disposing).");

            // Not using now
            if (disposing && _deviceManager != null)
            {
                //_log.Info("disposing && _deviceManager != null");

                if (_IDeviceManagerPluginCondition != null)
                {
                    //_log.Info("_IDeviceManagerPluginCondition != null");

                    var pluginCondition = ((IFrameworkPluginConditionNotification)_IDeviceManagerPluginCondition).CurrentConditionAsync();
                    if (pluginCondition != null)
                    {
                        //_log.Info("pluginCondition != null");

                        //if (pluginCondition is PluginRunningCondition)
                        {
                            //_log.Info("pluginCondition is PluginRunningCondition");
                            _deviceManager.Reset0x52TimerTick(8000);

                            UnregisterDeviceMnagerEvents();
                        }
                        //else
                        //{
                        //    _log.Info("pluginCondition is not PluginRunningCondition");
                        //}
                    }
                }
            }

            _disposed = true;
        }

        #endregion IDispose

        #region SW/FW Update

        /// <summary>
        /// Check if any Software/Firmware update available by calling Subangent/DeviceManagerSA.
        /// </summary>
        /// <returns>True if YES, either FW or SW is available.</returns>
        private bool CheckIfSwFwUpdateAvailable(IDeviceManagerSA devMgr)
        {
            //Robert_Lin, 2024-12-17 add log for trace elapsed time

            bool ret = false;
            DDPMSettings data = DdpmCommonHelper.ReadDDPMSettings();
            if (data != null)
            {
                if (data.LockSettings != null)
                {
                    if (data.LockSettings.Lock_Settings_Updates)
                    {
                        _log.Info($"Peripherals_UpdateNotify UI locked, drop the update checking");
                        return ret;
                    }
                    else
                    {
                        _log.Info($"Peripherals_UpdateNotify UI unlocked, continue to check update");
                    }
                }
                else
                {
                    _log.Info($"Peripherals_UpdateNotify can't read LockSettings to check if UI locked, continue to check update");
                }
            }
            else
            {
                _log.Info($"Peripherals_UpdateNotify can't read DDPM settings to check if UI locked, continue to check update");
            }
            Requires.NotNull(devMgr, nameof(devMgr));
            //Get FW avaiable count
            //Check SW avaiable count
            _log.Info("Calling to GetFWUpdateInfo()");
            FWUpdateInfoPackage fwUpdateInfoPackage = devMgr.GetFWUpdateInfo(true).Result;
            _log.Info("Calling to SW_GetSWUpdateInfo()");
            SWUpdateInfoPackage sWUpdateInfoPackage = devMgr.SW_GetSWUpdateInfo(false, true).Result;
            _log.Info("Return from SW_GetSWUpdateInfo()");
            if (fwUpdateInfoPackage.FWUpdateInfo.Count > 0 || sWUpdateInfoPackage.SWUpdateInfo.Count > 0)
                ret = true;

            //Robert_Lin, 2024-12-9 store the count for later check
            if ((fwUpdateInfoPackage == null) || (fwUpdateInfoPackage.FWUpdateInfo == null))
                _updateAvailableCount_FW = 0;
            else
                _updateAvailableCount_FW = fwUpdateInfoPackage.FWUpdateInfo.Count;

            if ((sWUpdateInfoPackage == null) || (sWUpdateInfoPackage.SWUpdateInfo == null))
                _updateAvailableCount_SW = 0;
            else
                _updateAvailableCount_SW = sWUpdateInfoPackage.SWUpdateInfo.Count;

            _log.Info($"@ Startup update count: SW={_updateAvailableCount_SW}, FW={_updateAvailableCount_FW}");

            _IsAnyUpdate = ret;
            if (sWUpdateInfoPackage != null && sWUpdateInfoPackage.SWUpdateInfo != null && sWUpdateInfoPackage.SWUpdateInfo.Count >= 1)
            {
                InterruptScreenRoot myDeserializedClass = DdpmCommonHelper.DeviceManagerSA.InterruptScreen_Metadata().Result;
                if (myDeserializedClass != null)
                {
                    bool? b = false;
                    System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        InterruptScreen interruptScreen = new InterruptScreen(sWUpdateInfoPackage.SWUpdateInfo[0].TheLatestVersion, myDeserializedClass, DdpmCommonHelper.DeviceManagerSA, sWUpdateInfoPackage, _log);
                        interruptScreen.Owner = System.Windows.Application.Current.MainWindow;
                        interruptScreen.Show();
                        /*if (b == true)
                        {
                            _log?.Info("CheckIfSwFwUpdateAvailable SW_DownloadAndInstall go");
                            List<SWUpdateInfo> swUpdateInfos = DdpmCommonHelper.DeviceManagerSA.SW_DownloadAndInstall(sWUpdateInfoPackage.SWUpdateInfo, true).Result;
                            _log?.Info("CheckIfSwFwUpdateAvailable SW_DownloadAndInstall finish");
                            //_showPluginManager?.ShowPluginById(DDPM.UI.Common.Constants.SettingsPluginId, "1");
                        }*/
                    }));
                }
            }
            return ret;
        }

        //Robert_Lin, 2024-12-9 added for PIMS-299696 Gear icon indication blinking not only twice to show availability.
        /// <summary>
        /// Invoked when any changed of FW/SW update package information.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _deviceManager_Peripherals_UpdateNotify(object? sender, bool e)
        {
            DDPMSettings data = DdpmCommonHelper.ReadDDPMSettings();
            if (data != null)
            {
                if (data.LockSettings != null)
                {
                    if (data.LockSettings.Lock_Settings_Updates)
                    {
                        _log.Info($"Peripherals_UpdateNotify UI locked, drop the update checking");
                        return;
                    }
                    else
                    {
                        _log.Info($"Peripherals_UpdateNotify UI unlocked, continue to check update");
                    }
                }
                else
                {
                    _log.Info($"Peripherals_UpdateNotify can't read LockSettings to check if UI locked, continue to check update");
                }
            }
            else
            {
                _log.Info($"Peripherals_UpdateNotify can't read DDPM settings to check if UI locked, continue to check update");
            }
            if (_deviceManager != null)
            {
                //Get FW/SW update count
                FWUpdateInfoPackage fwUpdateInfoPackage = _deviceManager.GetFWUpdateInfo(true).Result;
                SWUpdateInfoPackage sWUpdateInfoPackage = _deviceManager.SW_GetSWUpdateInfo(false, true).Result;

                int newSwCount = 0;
                if ((sWUpdateInfoPackage != null) && (sWUpdateInfoPackage.SWUpdateInfo != null))
                    newSwCount = sWUpdateInfoPackage.SWUpdateInfo.Count;
                int newFwCount = 0;
                if ((fwUpdateInfoPackage != null) && (fwUpdateInfoPackage.FWUpdateInfo != null))
                    newFwCount = fwUpdateInfoPackage.FWUpdateInfo.Count;

                _log.Info($"@ Peripherals_UpdateNotify, SW: {_updateAvailableCount_SW} -> {newSwCount}, FW: {_updateAvailableCount_FW} -> {newFwCount}");

                //Case_1, no any sw/fw count => turn the indicator to OFF
                if ((newSwCount <= 0) && (newFwCount <= 0))
                {
                    //Update to variables
                    _updateAvailableCount_SW = 0;
                    _updateAvailableCount_FW = 0;

                    //Set to Off anyway
                    if (_gearBtn != null)
                    {
                        _log.Info($"@ Peripherals_UpdateNotify, Turn off GearIcon indicator");
                        _gearBtn.SetOrangeDotVisible(false);
                    }
                    else if (_iconGear != null)
                    {
                        _log.Info($"@ Peripherals_UpdateNotify, Turn off GearIcon indicator");
                        _iconGear.SetOrangeDotVisible(false);
                    }
                    else
                    {
                        _log.Info($"@ Peripherals_UpdateNotify, GearIcon is null.");
                    }
                    return;
                }

                //Case_2, Either SW or FW are increased
                bool isSwCountIncreased = (newSwCount - _updateAvailableCount_SW > 0);
                bool isFwCountIncreased = (newFwCount - _updateAvailableCount_FW > 0);
                if (isSwCountIncreased || isFwCountIncreased)
                {
                    //Trigger a Blinking effect
                    if (_gearBtn != null)
                    {
                        _log.Info($"@ Peripherals_UpdateNotify, Trigger a glow effect.");
                        _gearBtn.GlowEffect_Trigger();
                    }
                    else if (_iconGear != null)
                    {
                        _log.Info($"@ Peripherals_UpdateNotify, Trigger a glow effect.");
                        _iconGear.GlowEffect_Trigger();
                    }
                    else
                    {
                        _log.Info($"@ Peripherals_UpdateNotify, Trigger a glow effect error, GearIcon is null.");
                    }
                }
                else
                {
                    //Case_3, no changed : nothing to do
                    _log.Info($"@ Peripherals_UpdateNotify, update count is not changed.");
                }

                //Update to variables
                _updateAvailableCount_SW = newSwCount;
                _updateAvailableCount_FW = newFwCount;
            }
            else
            {
                _log.Info("@ Peripherals_UpdateNotify, DeviceManager is null.");
            }
        }
        #endregion SW/FW Update

        #region WalkThrough
        /// <summary>
        /// Check if the device has completed the WalkThrough, and add new devices to the queue
        /// </summary>
        /// <param name="device">DeviceInfo list</param>
        /// <returns>Task</returns>
        private async Task CheckAndQueueDevice(String modelNumber, String modelType, object info)
        {
            _log.Info($"[Walkthrough] {nameof(CheckAndQueueDevice)} Start ... in");
            _log.Info($"[Walkthrough] {nameof(CheckAndQueueDevice)} Start for ModelNumber {modelNumber}, ModelType {modelType}, UserID {UserId}");
            object regValue;
            string regPath = $@"SOFTWARE\Dell\Dell Display And Peripheral Manager\UserSettings\Local\{UserId}";
            string regKey = $"IsFirstTimeWalkThroughDone_com.dell.DPM.Plugin.LogicalDevice.{modelNumber}";
            string regKeyForConsentPage = $"IsFirstTimeWalkThroughDone_com.dell.DPM.Plugin.LogicalDevice.CONSENT_PAGE";
            string regKeyForDDPM = $"IsFirstTimeWalkThroughDone_com.dell.DPM.Plugin.LogicalDevice.DDPM";

            try
            {
                if (null == _deviceManager)
                {
                    _log.Info($"[Walkthrough] {nameof(CheckAndQueueDevice)} _deviceManager null");
                    return;
                }

                if (modelNumber == "CONSENT_PAGE" && modelType == "CONSENT_PAGE")
                {
                    _log.Info($"[Walkthrough] {nameof(CheckAndQueueDevice)} WalkThrough CONSENT_PAGE in ... ");
                    // Check ConsentPage reg
                    regPath = @"SOFTWARE\Dell\Dell Display And Peripheral Manager\UserSettings\Local";
                    //regValue = await _deviceManager.ReadRegistryData(DDPM.SA.Common.Settings.RegistryHive.LocalMachine, regPath, regKeyForConsentPage);
                    regValue = DdpmCommonHelper.ReadRegistryData(DDPM.SA.Common.Settings.RegistryHive.LocalMachine, regPath, regKeyForConsentPage);

                    if (!Convert.ToBoolean(regValue))
                    {
                        if (!WalkThroughQueue.Exists(info => info.ModelName == modelNumber) && !WalkThroughEndList.Exists(info => info.ModelName == modelType))
                        {
                            WalkThroughQueue.Add(new WalkThroughInfo(modelNumber, modelType, info));
                            _log.Info($"[Walkthrough] {nameof(CheckAndQueueDevice)} WalkThroughQueue Add ConsentPage.");
                        }
                    }
                    else
                        _log.Info($"[Walkthrough] {nameof(CheckAndQueueDevice)} WalkThroughQueue ConsentPage Walk Through skipped.");
                }
                else if (modelNumber == "DDPM" && modelType == "DDPM")
                {
                    _log.Info($"[Walkthrough] {nameof(CheckAndQueueDevice)} WalkThrough DDPM in ... ");
                    // Check DDPM walkthrough
                    //regValue = await _deviceManager.ReadRegistryData(DDPM.SA.Common.Settings.RegistryHive.LocalMachine, regPath, regKeyForDDPM);
                    regValue = DdpmCommonHelper.ReadRegistryData(DDPM.SA.Common.Settings.RegistryHive.LocalMachine, regPath, regKeyForDDPM);

                    if (!Convert.ToBoolean(regValue))
                    {
                        // 0114 Wayn Remove to walkthrough end then record the registry
                        //await _deviceManager.WriteRegistryData(DDPM.SA.Common.Settings.RegistryHive.LocalMachine, regPath, regKeyForDDPM, true);
                        if (!WalkThroughQueue.Exists(info => info.ModelName == "DDPM") && !WalkThroughEndList.Exists(info => info.ModelName == "DDPM"))
                        {
                            WalkThroughQueue.Add(new WalkThroughInfo("DDPM", "DDPM", info));
                            _log.Info($"[Walkthrough] {nameof(CheckAndQueueDevice)} WalkThroughQueue Add DDPM ");
                        }
                    }
                    else
                        _log.Info($"[Walkthrough] {nameof(CheckAndQueueDevice)} WalkThroughQueue DDPM walhthrough skip.");
                }
                else
                {
                    _log.Info($"[Walkthrough] {nameof(CheckAndQueueDevice)} Device WalkThrough : {modelNumber} in ...");
                    // read reg
                    if (modelNumber == "U3224KB" || modelNumber == "U3224KBA") // correct DPeM typo
                    {
                        regKey = $"IsFirstTimeWalkThroughDone_com.dell.DPM.Plugin.LogicalDevice.U3224KB";
                        //regValue = await _deviceManager.ReadRegistryData(DDPM.SA.Common.Settings.RegistryHive.LocalMachine, regPath, regKey); //false, mean DPeM already have walkthrough.   
                        regValue = DdpmCommonHelper.ReadRegistryData(DDPM.SA.Common.Settings.RegistryHive.LocalMachine, regPath, regKey);
                        if (!CheckRegReturnValue(regValue)) // If false, need to check "U3224KBA" again
                        {
                            _log.Info($"[Walkthrough] {nameof(CheckAndQueueDevice)} U3224KB ReadRegistryData false");
                            regKey = $"IsFirstTimeWalkThroughDone_com.dell.DPM.Plugin.LogicalDevice.U3224KBA";
                            //regValue = await _deviceManager.ReadRegistryData(DDPM.SA.Common.Settings.RegistryHive.LocalMachine, regPath, regKey);
                            regValue = DdpmCommonHelper.ReadRegistryData(DDPM.SA.Common.Settings.RegistryHive.LocalMachine, regPath, regKey);
                        }
                        else
                            _log.Info($"[Walkthrough] {nameof(CheckAndQueueDevice)} U3224KB ReadRegistryData true");
                    }
                    else if (modelNumber == "P2424HEB") // correct DPeM typo
                    {
                        regKey = $"IsFirstTimeWalkThroughDone_com.dell.DPM.Plugin.LogicalDevice.CXXXXXX";
                        //regValue = await _deviceManager.ReadRegistryData(DDPM.SA.Common.Settings.RegistryHive.LocalMachine, regPath, regKey); //false, mean DPeM already have walkthrough.                   
                        regValue = DdpmCommonHelper.ReadRegistryData(DDPM.SA.Common.Settings.RegistryHive.LocalMachine, regPath, regKey);
                        if (!CheckRegReturnValue(regValue)) // If false, need to check "P2424HEB" again
                        {
                            _log.Info($"[Walkthrough] {nameof(CheckAndQueueDevice)} CXXXXXX ReadRegistryData false");
                            regKey = $"IsFirstTimeWalkThroughDone_com.dell.DPM.Plugin.LogicalDevice.P2424HEB";
                            //regValue = await _deviceManager.ReadRegistryData(DDPM.SA.Common.Settings.RegistryHive.LocalMachine, regPath, regKey);
                            regValue = DdpmCommonHelper.ReadRegistryData(DDPM.SA.Common.Settings.RegistryHive.LocalMachine, regPath, regKey);
                        }
                        else
                            _log.Info($"[Walkthrough] {nameof(CheckAndQueueDevice)} CXXXXXX ReadRegistryData true");
                    }
                    else if (modelNumber == "MS700/7")
                    {
                        regKey = $"IsFirstTimeWalkThroughDone_com.dell.DPM.Plugin.LogicalDevice.MS700";
                        //regValue = await _deviceManager.ReadRegistryData(DDPM.SA.Common.Settings.RegistryHive.LocalMachine, regPath, regKey);
                        regValue = DdpmCommonHelper.ReadRegistryData(DDPM.SA.Common.Settings.RegistryHive.LocalMachine, regPath, regKey);
                    }
                    else
                        //regValue = await _deviceManager.ReadRegistryData(DDPM.SA.Common.Settings.RegistryHive.LocalMachine, regPath, regKey);
                        regValue = DdpmCommonHelper.ReadRegistryData(DDPM.SA.Common.Settings.RegistryHive.LocalMachine, regPath, regKey);

                    _log.Info($"[Walkthrough] {nameof(CheckAndQueueDevice)} regPath : {regPath}");
                    _log.Info($"[Walkthrough] {nameof(CheckAndQueueDevice)} regPath : {regKey}");
                    // mean null or "" or is false, add to the queue and set it to true
                    if (!CheckRegReturnValue(regValue))
                    {
                        _log.Info($"[Walkthrough] CheckRegReturnValue false");
                        var devicePages = WalkThroughData.WalkThroughData.GetDevicePages((int)DdpmCommonHelper.PreviousOsTheme);
                        // If the device is not supported, directly update the registry to true and return
                        if (!devicePages.ContainsKey(modelNumber))
                        {
                            //await _deviceManager.WriteRegistryData(DDPM.SA.Common.Settings.RegistryHive.LocalMachine, regPath, regKey, true);
                            DdpmCommonHelper.WriteRegistryData(DDPM.SA.Common.Settings.RegistryHive.LocalMachine, regPath, regKey, true);
                            _log.Info($"[Walkthrough] Device {modelNumber} not found in devicePages, skipping.");
                            return;
                        }

                        // Add the device to the queue and update the registry
                        if (!WalkThroughQueue.Exists(info => info.ModelName == modelNumber) && !WalkThroughEndList.Exists(info => info.ModelName == modelNumber))
                        {
                            WalkThroughQueue.Add(new WalkThroughInfo(modelNumber, modelType, info));
                            _log.Info($"[Walkthrough] Device {modelNumber} Add in WalkThroughQueue.");
                        }

                        _log.Info($"[Walkthrough] Device {modelNumber} added to the queue and registry value updated to true.");
                    }
                    else
                    {
                        _log.Info($"[Walkthrough] CheckRegReturnValue true, Device {modelNumber} reg is true, skipping.");
                    }
                }
                await DeviceSort(new WalkThroughInfo(modelNumber, modelType, info));
            }
            catch (Exception ex)
            {
                _log.Error($"[Walkthrough] Error processing device {modelNumber}: {ex.Message}");
            }
            _log.Error($"[Walkthrough] CheckAndQueueDevice ... end");
        }

        public bool CheckRegReturnValue(object regValue)
        {
            if (regValue == null || (regValue is string strValue && string.IsNullOrEmpty(strValue)) || !Convert.ToBoolean(regValue))
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Collect currently connected devices
        /// </summary>
        /// <returns></returns>
        public async Task CollectAndCompareDevicesAsync()
        {
            _log.Info($"[Walkthrough] {nameof(CollectAndCompareDevicesAsync)} Start");
            try
            {
                if (_monitorInfos == null)
                {
                    _monitorInfos = _deviceManager!.GetMonitors().Result;  // change to restore monitor caches
                    _log.Info($"[Walkthrough] CollectAndCompareDevicesAsync GetMonitors, _monitorInfos == null");
                }
                if (_deviceInfos == null)
                {
                    _deviceInfos = _deviceManager!.GetDevices().Result.deviceInfo; // change to restore device caches
                    _log.Info($"[Walkthrough] CollectAndCompareDevicesAsync GetDevices, _deviceInfos == null");
                }

                _log.Info($"[Walkthrough] CollectAndCompareDevicesAsync, monitor count:{_monitorInfos.Count.ToString()}, device count : {_deviceInfos.Count.ToString()}");

                // WalkThroughInfo
                foreach (var monitor in _monitorInfos)
                {
                    _log.Info($"[Walkthrough] CheckAndQueueDevice Start Add (Monitor)");
                    await CheckAndQueueDevice(monitor.modelName, "Displays", monitor);
                }

                foreach (var device in _deviceInfos)
                {
                    _log.Info($"[Walkthrough] CheckAndQueueDevice Start Add (device)");
                    // Check color code
                    string _modelNumber = device.ModelNumber;
                    if (device.ModelNumber == "MS700" &&
                        device.ColorCode != 0)
                    {
                        _modelNumber = _modelNumber + "/" + device.ColorCode.ToString();
                        _log.Info($"[Walkthrough] CheckAndQueueDevice Check color code = {device.ColorCode.ToString()}");
                    }
                    _log.Info($"[Walkthrough] CheckAndQueueDevice Start Add (Device), device.ID : {device.ID.ToString()}");
                    await CheckAndQueueDevice(_modelNumber, device.LogicalDeviceType.ToString(), device);
                }

                if (WalkThroughQueue.Count != 0 && ShowPluginById == false)
                {
                    _log.Info($"[Walkthrough] WalkThroughQueue.Count != 0, ShowPluginById Start");
                    _showPluginManager?.ShowPluginById(DDPM.UI.Common.Constants.WalkThroughPluginId);
                    ShowPluginById = true;
                }
            }
            catch (Exception ex)
            {
                _log.Error($"[Walkthrough] {nameof(CollectAndCompareDevicesAsync)} Error collecting devices: {ex.Message}");
            }
            _log.Error($"[Walkthrough] CollectAndCompareDevicesAsync ... end");
        }
        #endregion WalkThrough

        #region MainWindow Move To new position
        /// <summary>
        /// Handle the IConsole Event, MainWindow_MoveToNewPosition, when DDPM main window
        /// move to a new position.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">
        /// e.Tag (bool): True if the window is moved by hotkey.
        /// </param>
        private void Handle_MainWindow_MoveToNewPosition(object sender, EventManagerArgs e)
        {
            //Conditions to handle this event
            //1 DisplayPlugin is not activate
            if (DdpmCommonHelper.IsDisplayPluginActivated)
                return;
            //2 DeviceManagerPlugin is ready
            if (_deviceManager == null)
                return;
            //3 HomeDevices has monitor
            if (_viewModel == null)
                return;
            if (_viewModel.HomeDeviceCount == 0)
                return;

            //TO DO:
            //Action_1: Determine the Monitor where DDPM is moved to
            //Action_2: Show the model name OSD (if the monitor is supported monitor)
            //Action_3: Change the SelectedItem of Monitors combobox (if DisplayPlugin is activated)
            _log?.Info($"@Handle_MainWindow_MoveToNewPosition() is called.");

            //Action_1: Determine the Monitor where DDPM is moved to
            string screenDeviceName = "";

            //If e.Tag contains a DeviceName, then move the specified screen
            if (e.Tag != null &&
                e.Tag is string)
            {
                screenDeviceName = e.Tag.ToString();
            }
            //Else the screen will be get from mouse cursor position
            if (String.IsNullOrEmpty(screenDeviceName))
            {
                //Get mouse cursor position
                System.Drawing.Point cursorPosition = System.Windows.Forms.Cursor.Position;
                //Get the Screen of the cursor
                System.Windows.Forms.Screen screenOfCursor = System.Windows.Forms.Screen.FromPoint(cursorPosition);
                screenDeviceName = screenOfCursor.DeviceName;
            }

            //Check if screen is different with last show
            if (!screenDeviceName.Equals(DdpmCommonHelper.LastShowOsdScreenDeviceName))
            {
                //Find the monitor of the screen
                foreach (HomeDevice homeDev in _viewModel.HomeDevices)
                {
                    if (homeDev.DeviceCategory != eDeviceCategory.Display)
                        continue;
                    if (homeDev.MonitorInfo == null)
                        continue;
                    if (homeDev.MonitorInfo.DisplayName.Equals(screenDeviceName, StringComparison.OrdinalIgnoreCase))
                    {
                        _deviceManager.ShowOSD(homeDev.MonitorInfo, OSDType.DisplayChanged);
                    }
                } //foreach

                DdpmCommonHelper.LastShowOsdScreenDeviceName = screenDeviceName;

                Screen? screenNow = Screen.AllScreens.FirstOrDefault(x => x.DeviceName.Equals(screenDeviceName, StringComparison.OrdinalIgnoreCase));
                if (screenNow != null)
                {
                    if (_viewModel != null)
                    {
                        _viewModel.IsSmallScreenResolution = (screenNow.Bounds.Width < 1050);
                    }
                }
            }
        }
        #endregion  MainWindow Move To new position

    }
}