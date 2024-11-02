using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using DDPM.SA.Common;
using DDPM.UI.Common;
using DDPM.UI.Common.Models;
using DDPM.UI.Common.UserControls;
using DDPM.UI.Plugin.Common;
using DDPM.UI.Plugin.DdpmHomePlugin.Interfaces;
using DDPM.UI.Plugin.DdpmHomePlugin.ViewModels;
using DDPM.UI.WalkThroughData;
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
using VcpCore.Common;
using Windows.Devices.Geolocation;
using Windows.Devices.Input;
using static Dell.Client.Framework.Security.LocalAccounts;
using static Dell.Client.Framework.UX.WPF.WinApi;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;
using DDPMConstants = DDPM.UI.Common.Constants;

//using VcpCore.Interfaces;
using IDdpmHomePageViewModel = DDPM.UI.Plugin.DdpmHomePlugin.Interfaces.IDdpmHomePageViewModel;

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
        public string HeaderText => "DDPM Homepage";

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


        // For WalkThrough
        public static string _userId = string.Empty;
        public static bool _showPluginById = false;
        public static List<WalkThroughInfo> WalkThroughQueue { get; private set; } = new List<WalkThroughInfo>();
        private static readonly Dictionary<string, int> ModelTypeMapping = new Dictionary<string, int>
        {
            { "Consent", 1}, //Add by Derek 2024/10/24
            { "DDPM", 2 },
            { "Displays", 3 },
            { "Webcam", 4 },
            { "Keyboard", 5 },
            { "Mice", 6 },
            { "Stylus", 7 },
            { "Headset", 8 },
            { "Speakerphone", 9 },
            { "Soundbar", 10 },
            { "Audio", 11 },
            { "Docks", 12 }
        };

        private GlobalSettingParam _globalSettings = null;

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
        }


        private void PluginManager_PluginsStarted(object? sender, PluginsStartedEventArgs pluginsStartedEventArgs)
        {
            _log.Info($"{nameof(PluginManager_PluginsStarted)} started");
            try
            {
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
                    return;

                // Subscribe to plugin changes
                _IDeviceManagerPluginCondition.PluginConditionChangeHandler += _IDeviceManagerPluginCondition_PluginConditionChangeHandler;

                // Get current condition
                _ = Task.Run(GetCurrentDeviceManagerPluginPluginCondition, CancellationToken);
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
            _log.Trace($"{nameof(GetCurrentDeviceManagerPluginPluginCondition)} lock");
            try
            {
                if (_IDeviceManagerPluginCondition == null)
                    return;

                var pluginCondition = await ((IFrameworkPluginConditionNotification)_IDeviceManagerPluginCondition).CurrentConditionAsync();

                if (pluginCondition is PluginErrorCondition)
                {
                    if (_viewModel != null)
                        _viewModel.IsDeviceManagerReady = false;

                    _log.Info($"{nameof(GetCurrentDeviceManagerPluginPluginCondition)} plugin is in {nameof(PluginErrorCondition)}");
                }
                else if (pluginCondition is PluginRunningCondition)
                {
                    _log.Info($"{nameof(GetCurrentDeviceManagerPluginPluginCondition)} plugin is in {nameof(PluginRunningCondition)}");

                    if (_deviceManager != null)
                    {
                        if (!_HasRegisted)
                        {
                            if (_viewModel != null)
                                _viewModel.IsDeviceManagerReady = true;
                            _HasRegisted = true;
                            _deviceManager.DeviceChanged += _deviceManager_DeviceChanged;
                            _deviceManager.VCPchanged += _deviceManager_VCPchanged;

                            //Move to call from OnActivated( ) => Failed, it's called too late
                            //So uncommented below code
                            //Task.Run(async () => await GetDdpmDevicesAsync(_deviceManager));

                            //Robert_Lin, 2024-6-21 UI shown, tell VCPCore to increase polling rate to 0x52
                            //Derek_Du, 2024-10-21 add send process ID to SA
                            Task delayTask = _deviceManager.Reset0x52TimerTick(2000, Process.GetCurrentProcess().Id);
                            _deviceManager.ReceiveTelemetryInfo("AppSession", "AppStarted",Telementry_Frequency.RealTime);

                            await GetDdpmDevicesAsync(_deviceManager);

                            //1030 get global settings for telemetry consent page using
                            _globalSettings = _deviceManager.GetGlobalSettingParam().Result;
                            //1030 Dean
                            //For Hess to read global setting "_globalSettings"
                            //After "GetDdpmDevicesAsync" the user setting cache is ready "DdpmCommonHelper.Settings_Cache"
                            if (_globalSettings != null && DdpmCommonHelper.Settings_Cache != null && _viewModel != null)
                            {
                                if (!_globalSettings.isSetTelemetryOverInstaller && !DdpmCommonHelper.Settings_Cache.UserSettings.isDisplayConsentPage)
                                {
                                    _viewModel.ShowConsent();
                                    DdpmCommonHelper.Settings_Cache.UserSettings.isDisplayConsentPage = true;
                                    DdpmCommonHelper.WriteDDPMSettings(DdpmCommonHelper.Settings_Cache);
                                }
                            }

                            if (_deviceManager == null)
                            {
                                _log.Error($"{nameof(PluginManager_PluginsStarted)} ISettingsManagerDev Plugin is null");
                                return;
                            }
                            //Wayn 2024-09-04 For WalkThrough
                            _log.Info($"[Walkthrough] {nameof(GetCurrentDeviceManagerPluginPluginCondition)} Start");
                            await CollectAndCompareDevicesAsync();

                            //Robert_Lin 2024-8-2 DDPMW-579, If there is any FW/SW update available,
                            //then the Gear icon on masthead will show breathe & glow animation.
                            //Call once
                            if (CheckIfSwFwUpdateAvailable(_deviceManager))
                            {
                                if (_iconGear != null)
                                    _iconGear.GlowEffect_Start();
                            }
                        }
                        await CheckAndQueueDevice("DDPM", "DDPM");
                        if (WalkThroughQueue.Count != 0 && _showPluginById == false)
                        {
                            _log.Info($"[Walkthrough] WalkThroughQueue.Count != 0, ShowPluginById Start DDPM");
                            _showPluginManager?.ShowPluginById(DDPM.UI.Common.Constants.WalkThroughPluginId);
                            _showPluginById = true;
                        }

                        CheckIfNeedImportSetting_Display();
                    }
                }
            }
            catch (Exception ex)
            {
                var message = $"{nameof(GetCurrentDeviceManagerPluginPluginCondition)} failed with error - {ex.Message}";
                _log.Error(ex, message);
                //throw new NotificationPluginException(message);
            }
            finally
            {
                _lock.Release();
                _log.Trace($"{nameof(GetCurrentDeviceManagerPluginPluginCondition)} unlock");
            }
        }

        private async void _deviceManager_DeviceChanged(object? sender, DeviceChangedEventArgs e)
        {
            _log.Info("DdpmHomePlugin._deviceManager_DeviceChanged() executed");

            if ((e != null) && !string.IsNullOrEmpty(e.changedProperty))
            {
                //Robert_Lin, 2024-7-22 log info
                _log.Info($"@ ChangedProperty=[{e.changedProperty}], ChangedType=[{e.type}] DeviceID=[{e.deviceID}]");
                if (e.device_peripherals != null)
                {
                    // Check and handle new inserted devices
                    //await CheckAndQueueDevice(e.device_peripherals);
                    _log.Info($"@ DeviceName=[{e.device_peripherals.Name}]");
                }
                _log.Info($"[Walkthrough] {nameof(_deviceManager_DeviceChanged)} Start");
                await CollectAndCompareDevicesAsync();
                //// Check Queue，first use device need to show WalkThroughPage
                if (WalkThroughQueue.Count > 0 && _showPluginById == false)
                {
                    _log.Info($"[Walkthrough] {nameof(_deviceManager_DeviceChanged)} WalkThroughQueue has items, ShowPluginById.");
                    _showPluginManager?.ShowPluginById(DDPM.UI.Common.Constants.WalkThroughPluginId);
                    _showPluginById = true;
                }
                //2024-8-6 Robert, fix bug. compare string should be lowercase due to ToLower()
                //2024-07-02, Elie, we only handle remove and add event on the DdpmHomePlugin.
                if (e.changedProperty.ToLower().Contains("remove") ||
                    (e.changedProperty.ToLower().Contains("add")) ||
                    (e.changedProperty.ToLower().Contains("batterystatuschanged")) ||
                    (e.changedProperty.ToLower().Contains("batterylevelchanged")) ||
                    ((string.Compare(e.changedProperty, "DisplayChanged", true) == 0)))
                {
                    //Force return to HomePage
                    // 2024-06-19 From Dean, using DeviceChangedType.NotifyOnly to check if it's a monitor settings change.

                    //2024-6-20 move refresh device form HomeView to here
                    //_ = Task.Run(GetDdpmDevicesAsync(_deviceManager));
                    if (_deviceManager != null)
                        _ = GetDdpmDevicesAsync(_deviceManager, e.changedProperty.ToLower());

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

                    CheckIfNeedImportSetting_Display();
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
        }
        
        private void CheckIfNeedImportSetting_Display()
        {
            //For existing monitor to check if need to pop-up message to import setting
            Task.Run(() =>
            {
                if (_monitorInfos == null && _monitorInfos.Count == 0)
                    return;
                List<MonitorInfo> temp_mos = _monitorInfos;
                //make sure no walkthrough page displaying
                while (WalkThroughQueue != null && WalkThroughQueue.Count > 0)
                {
                    Thread.Sleep(5000);
                }

                foreach (MonitorInfo info in temp_mos)
                {
                    //if(can popup messagebox && not yet to import / already click no need import)
                    {
                        //avoid timing issue to cause monitor updated
                        if (temp_mos.Count != _monitorInfos.Count)
                            return;

                        //force return here to avoid page trigger, need Jason handle it
                        return;
                        if(_viewModel != null)
                        {
                            _viewModel.InvokeImportQuestion(info);
                        }
                    }
                }
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

        private void _deviceManager_VCPchanged(object? sender, VCPchangedEventArgs e)
        {
            string monitorName = "";
            string vcpCode = "";
            if (e.monitor != null)
                if (e.monitor.AliasDeviceName != null)
                    monitorName = e.monitor.AliasDeviceName;
            if (e.vcpcode != null)
                vcpCode = e.vcpcode;
            _log.Info($"DdpmHomePlugin._deviceManager_VCPchanged() executed, Monitor=[{monitorName}], VcpCode=[{e.vcpcode}]");
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

        private async Task GetDdpmDevicesAsync(IDeviceManagerSA deviceManager, string condition = "all")
        {
            if (!SpinWait.SpinUntil(() =>
            (_IDeviceManagerPluginCondition is IFrameworkPluginConditionNotification), TimeSpan.FromMinutes(2)))
            {
                Console.WriteLine("Could not establish communication with DeviceManager plugin!!");
                return;
            }
            _log.Info("GetDdpmDevicesAsync is invoked");

            //_displayService = await deviceManager.GetDisplayServiceInterface(); Robert0502
            if (deviceManager != null)
            {
                DdpmCommonHelper.DeviceManagerSA = deviceManager;
                DdpmCommonHelper.Settings_Cache = deviceManager.ReloadAppConfigData().Result;
                //List<MonitorInfo> monitorInfos = deviceManager.GetMonitors().Result;
                if (condition.Equals("all") || condition.Equals("displaychanged"))
                {
                    _monitorInfos = deviceManager.GetMonitors().Result;
                    _log.Info($"Monitor count is ${_monitorInfos.Count}");
                }
                if (condition.Equals("all") || !condition.Equals("displaychanged"))
                {
                    DeviceHelper deviceHelper = deviceManager.GetDevices().Result;
                    //List<DeviceInfo> deviceInfos = new List<DeviceInfo>();
                    _deviceInfos = new List<DeviceInfo>();
                    if ((deviceHelper != null) && (deviceHelper.deviceInfo != null))
                    {
                        _deviceInfos = deviceHelper.deviceInfo;
                    }
                    _log.Info($"Peripheral count is ${_deviceInfos.Count}");
                }
                _ = Task.Run(() =>
                {
                    while (!_isConfigured)
                        ;

                    Interfaces.IDdpmHomePageViewModel? viewModel = PluginIoc.GetService<Interfaces.IDdpmHomePageViewModel>();
                    if (viewModel != null)
                    {
                        viewModel.ResetDevices();

                        _log.Info("Adding Monitors to HomePageViewModel...");

                        viewModel.PrepareMonitorInfos(_monitorInfos);

                        _log.Info("Adding Periphrals to HomePageViewModel...");
                        viewModel.PrepareDeviceInfos(_deviceInfos);

                        //Robert_Lin, 2024-8-5 for PIMS-289060, display a "Please wait" UI before devices ready
                        if (viewModel.HomeDevices.Count == 0)
                        {
                            //The "PleaseWait" UI will be displayed and auto closed after timeout (=12 sec)
                            viewModel.Invoke_PleaseWait();
                        }
                        else
                        {
                            viewModel.IsPleaseWaitVisible = false;
                            viewModel.DumpDevicesToLog();
                        }
                    }
                });
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
                _viewModel.Invoke_PleaseWait();
            }
            _isConfigured = true;
        }

        #region Interface IConsolePluginSupportsActivations

        /// <inheritdoc/>
        public void OnActivated()
        {
            //_deviceManager.DeviceChanged += _deviceManager_DeviceChanged;
            Mouse.OverrideCursor = null;
            _isActived = true;
        }

        /// <inheritdoc/>
        public void OnDeactivated()
        {
            //_deviceManager.DeviceChanged -= _deviceManager_DeviceChanged;
            Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
            _isActived = false;
        }

        /// <inheritdoc/>
        public void OnShown(string param = "")
        {
            if (!string.IsNullOrEmpty(param))
            {
                IDdpmHomePageViewModel? viewModel = PluginIoc.GetService<IDdpmHomePageViewModel>();
                if (viewModel != null)
                {
                    viewModel.HomeDevices = new System.Collections.ObjectModel.ObservableCollection<HomeDevice>();
                }
            }
            ConfigureServices();
            Mouse.OverrideCursor = System.Windows.Input.Cursors.Arrow;
            Mouse.OverrideCursor = null;
            DdpmCommonHelper.MyConsole = PluginIoc.GetService<IConsole>();
            DdpmCommonHelper.MyShowPluginManager= PluginIoc.GetService<IShowPluginManager>();


            //Robert_Lin, 2024-7-17, fix PIMS-286435 in AddDevice menu, the AddDevice icon is in Top Right side.
            if (_iconAddDevice != null)
                _iconAddDevice.Visibility = Visibility.Visible;
            if (_iconGear != null)
                _iconGear.Visibility = Visibility.Visible;
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

        public static List<HomeDevice> GetHomeDevices()
        {
            IDdpmHomePageViewModel? viewModel = PluginIoc.GetService<IDdpmHomePageViewModel>();
            if (viewModel != null)
            {
                return viewModel.HomeDevices.ToList();
            }
            return new List<HomeDevice>();
        }

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

        #region Icons on Masthead

        //Robert_Lin, 2024-7-17, fix PIMS-286435 in AddDevice menu, the AddDevice icon is in Top Right side.
        //Expected behavior: When entering AddDevice plugin, the 'AddDevice' icon on Masthead should be hidden.
        //  and show back when exiting from AddDevice plugin.
        //  It should have the same behavior for 'Gear' icon.
        private PathIcon? _iconGear = null;

        private PathIcon? _iconAddDevice = null;

        private void AddIconsToMasthead(UXMasthead masthead)
        {
            //Add Grear icon, click it will call to OnGreaeIconClicked()
            if (_iconGear == null)
            {
                _iconGear = new PathIcon();
                //iconGear.Width = 12; iconGear.Height = 12;
                _iconGear.PathData = "M5.1525 12L6.99 12L7.5 10.4475L8.04 10.2375L9.54 10.9875L10.905 9.6675L10.155 8.1675L10.335 7.6275L12 7.02L12 5.145L10.35 4.6275L10.14 4.0875L10.965 2.43L9.6525 1.11L8.0625 1.965L7.5 1.6875L6.93 -2.21617e-07L5.085 -3.02264e-07L4.545 1.7175L4.0125 1.9575L2.3925 1.2075L1.0875 2.5275L1.9125 4.1175L1.665 4.6725L7.26157e-07 5.205L6.43214e-07 7.1025L1.68 7.62L1.92 8.1675L1.215 9.7125L2.5125 11.0175L4.0125 10.2675L4.545 10.4325L5.1525 12ZM2.6775 10.05L2.175 9.54L2.79 8.1825L2.25 6.9675L0.750001 6.51L0.750001 5.76L2.25 5.25L2.805 4.0275L2.055 2.61L2.55 2.1075L3.9825 2.805L5.205 2.3025L5.67 0.8025L6.3675 0.8025L6.87 2.25L8.0775 2.8575L9.51 2.1075L9.99 2.595L9.24 4.095L9.75 5.25L11.205 5.7075L11.205 6.4575L9.705 6.9825L9.2925 8.205L9.9375 9.51L9.435 10.0125L8.0925 9.375L6.885 9.8475L6.42 11.2125L5.715 11.2125L5.19 9.75L3.9675 9.3825L2.6775 10.0275L2.6775 10.05Z M8.1825 6C8.18102 5.56866 8.05176 5.14744 7.81104 4.78952C7.57033 4.4316 7.22895 4.15303 6.83002 3.98899C6.43109 3.82495 5.9925 3.7828 5.56964 3.86785C5.14677 3.95291 4.75859 4.16136 4.45411 4.46689C4.14963 4.77241 3.94251 5.16131 3.8589 5.58447C3.7753 6.00762 3.81896 6.44607 3.98436 6.84443C4.14977 7.24279 4.42951 7.58321 4.78825 7.8227C5.147 8.06218 5.56866 8.19 6 8.19C6.28724 8.19 6.57166 8.1333 6.83694 8.02315C7.10223 7.913 7.34316 7.75157 7.54592 7.54811C7.74868 7.34465 7.90929 7.10317 8.01852 6.83751C8.12776 6.57185 8.18349 6.28724 8.1825 6ZM4.6275 6C4.63047 5.7274 4.71411 5.46179 4.86787 5.23667C5.02163 5.01156 5.23862 4.83703 5.49146 4.7351C5.7443 4.63318 6.02167 4.60842 6.28857 4.66396C6.55547 4.7195 6.79993 4.85285 6.99113 5.04718C7.18232 5.24151 7.31167 5.48811 7.36287 5.75587C7.41406 6.02364 7.3848 6.30057 7.27878 6.55172C7.17275 6.80287 6.99472 7.017 6.76713 7.16708C6.53955 7.31716 6.27261 7.39647 6 7.395C5.63332 7.39104 5.28311 7.24208 5.02592 6.98068C4.76874 6.71928 4.6255 6.3667 4.6275 6Z";
                _iconGear.ClickCommand = new RelayCommand(OnGearIconClicked);
                //_iconGear.IsTabStop = true;
                //_iconGear.TabIndex = 0;
                //_iconGear.IsHitTestVisible = true;
                masthead.InsertCustomContent(_iconGear);
                //masthead.IsTabStop = true;
            }

            if (_iconAddDevice == null)
            {
                //Add Add icon, click it will call to OnGreaeIconClicked()
                _iconAddDevice = new PathIcon();
                //iconAdd.Width = 12; iconAdd.Height = 12;
                _iconAddDevice.PathData = "M11.9475 5.25C11.7801 3.92998 11.1787 2.70306 10.2378 1.76219C9.29697 0.821322 8.07004 0.219895 6.75003 0.0525C6.50151 0.0179984 6.25093 0.000457859 6.00003 0C4.4809 0.0103651 3.02227 0.596591 1.91848 1.64037C0.814689 2.68416 0.147927 4.10778 0.0527501 5.62395C-0.042427 7.14012 0.441067 8.63595 1.40566 9.80958C2.37025 10.9832 3.74413 11.7472 5.25003 11.9475C5.49854 11.982 5.74912 11.9995 6.00003 12C6.85082 11.9992 7.69172 11.8175 8.46693 11.4669C9.24214 11.1164 9.93393 10.6049 10.4964 9.96659C11.0588 9.32824 11.4791 8.57756 11.7293 7.76439C11.9795 6.95121 12.0539 6.09412 11.9475 5.25ZM9.90003 9.4425C9.11363 10.3318 8.0465 10.9251 6.87611 11.1237C5.70572 11.3224 4.50261 11.1143 3.46685 10.5343C2.43109 9.95422 1.62511 9.03707 1.18295 7.93536C0.740791 6.83366 0.6891 5.61378 1.03647 4.47862C1.38385 3.34345 2.10935 2.36141 3.09232 1.69581C4.0753 1.03021 5.25651 0.72116 6.4395 0.820065C7.6225 0.918969 8.73599 1.41987 9.5948 2.23945C10.4536 3.05903 11.006 4.14791 11.16 5.325C11.253 6.06139 11.1888 6.80912 10.9716 7.51887C10.7544 8.22862 10.3892 8.88425 9.90003 9.4425Z M6.42753 2.625H5.52752L5.52752 5.55L2.62503 5.55V6.45L5.52752 6.45L5.52752 9.375L6.42753 9.375L6.42753 6.45L9.37503 6.45V5.55L6.42753 5.55L6.42753 2.625Z";
                _iconAddDevice.ClickCommand = new RelayCommand(OnAddIconClicked);
                _iconAddDevice.TooltipText = "Add device";
                masthead.InsertCustomContent(_iconAddDevice);
            }

            if (_console != null)
            {
                _console.RegisterForEvent(ConsoleEventNames.Masthead_ShowAddDevicePlugin, ShowAddDevicePlugin);
                _console.RegisterForEvent(ConsoleEventNames.Masthead_ShowSettingsPlugin, ShowSettingsPlugin);
                _console.RegisterForEvent(ConsoleEventNames.Masthead_StartGlowEffectOnGearIcon, StartGlowEffectOnGearIcon);
                _console.RegisterForEvent(ConsoleEventNames.Masthead_StopGlowEffectOnGearIcon, StopGlowEffectOnGearIcon);
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
            _console.ShowPluginById(UI.Common.Constants.SettingsPluginId);
        }

        //Robert_Lin 2024-8-2 added for DDMPW-579 story
        //To trigger this event:
        //  IConsole.RaiseEvent("StartGlowEffectOnGearIcon", this, new EventManagerArgs());
        private void StartGlowEffectOnGearIcon(object sender, EventManagerArgs e)
        {
            if (_iconGear != null)
                _iconGear.GlowEffect_Start();
        }

        //To trigger this event:
        //  IConsole.RaiseEvent("StopGlowEffectOnGearIcon", this, new EventManagerArgs());
        private void StopGlowEffectOnGearIcon(object sender, EventManagerArgs e)
        {
            if (_iconGear != null)
                _iconGear.GlowEffect_Stop();
        }

        //Robert_Lin 2024-8-23 added for PIMS-293574
        // When exit from AddDevicePlugin and GlobalSettingsPlugin, we should reshow the Masthead icons
        private void ShowAllMastheadIcons()
        {
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
            if (e.Tag != null)
            {
                if (e.Tag is bool)
                {
                    bool isShow = (bool)e.Tag;
                    if (isShow)
                    {
                        if (_iconAddDevice != null)
                            _iconAddDevice.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        if (_iconAddDevice != null)
                            _iconAddDevice.Visibility = Visibility.Collapsed;
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
            if (e.Tag != null)
            {
                if (e.Tag is bool)
                {
                    bool isShow = (bool)e.Tag;
                    if (isShow)
                    {
                        if (_iconGear != null)
                            _iconGear.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        if (_iconGear != null)
                            _iconGear.Visibility = Visibility.Collapsed;
                    }
                }
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

                        if (pluginCondition is PluginRunningCondition)
                        {
                            //_log.Info("pluginCondition is PluginRunningCondition");
                            _deviceManager.Reset0x52TimerTick(8000);
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
        private static bool CheckIfSwFwUpdateAvailable(IDeviceManagerSA devMgr)
        {
            Requires.NotNull(devMgr, nameof(devMgr));
            //Get FW avaiable count
            FWUpdateInfoPackage fwUpdateInfoPackage = devMgr.GetFWUpdateInfo(false).Result;
            if (fwUpdateInfoPackage.FWUpdateInfo.Count > 0)
                return true;

            //Check SW avaiable count
            SWUpdateInfoPackage sWUpdateInfoPackage = devMgr.SW_GetSWUpdateInfo(false).Result;
            if (sWUpdateInfoPackage.SWUpdateInfo.Count > 0)
                return true;

            return false;
        }

        #endregion SW/FW Update

        #region WalkThrough
        private enum WTS_INFO_CLASS
        {
            WTSUserName = 5,
            WTSDomainName = 7,
        }
        [DllImport("Kernel32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        private static extern int WTSGetActiveConsoleSessionId();

        private int WTSGetActiveConsoleSessionId_Public()
        {
            return WTSGetActiveConsoleSessionId();
        }
        [DllImport("Wtsapi32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        private static extern void WTSFreeMemory(IntPtr pointer);

        private void WTSFreeMemory_Public(IntPtr pointer)
        {
            WTSFreeMemory(pointer);
        }
        [DllImport("Wtsapi32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        private static extern bool WTSQuerySessionInformation(IntPtr hServer, int sessionId, WTS_INFO_CLASS wtsInfoClass, out IntPtr ppBuffer, out int pBytesReturned);

        private bool WTSQuerySessionInformation_Public(IntPtr hServer, int sessionId, WTS_INFO_CLASS wtsInfoClass, out IntPtr ppBuffer, out int pBytesReturned)
        {
            return WTSQuerySessionInformation(hServer, sessionId, wtsInfoClass, out ppBuffer, out pBytesReturned);
        }
        /// <summary>
        /// From SA code
        /// </summary>
        /// <returns>User Sid</returns>
        public string GetActiveUserID()
        {
            IntPtr buffer;
            int bytesReturned = 0;
            int sessionId = WTSGetActiveConsoleSessionId_Public(); // This gets the session ID of the user logged into the console
            Console.WriteLine($"[Walkthrough] WTSGetActiveConsoleSessionId: {sessionId}");
            _log.Info($"[Walkthrough] WTSGetActiveConsoleSessionId: {sessionId}");
            if (WTSQuerySessionInformation_Public(IntPtr.Zero, sessionId, WTS_INFO_CLASS.WTSUserName, out buffer, out bytesReturned))
            {
                string userName = Marshal.PtrToStringAnsi(buffer);
                WTSFreeMemory_Public(buffer);
                Console.WriteLine($"[Walkthrough] WTSQuerySessionInformation: user name ({userName})");
                _log.Info($"[Walkthrough] WTSQuerySessionInformation: user name ({userName})");
                if (!string.IsNullOrEmpty(userName))
                {
                    string userSid = GetUserSid(userName);
                    if (!string.IsNullOrEmpty(userSid))
                    {
#if DEBUG
                        Console.WriteLine($"[Walkthrough] User ID from registry: {userSid}");
                        _log.Info($"[Walkthrough] User ID from registry: {userSid}");
#endif
                        return userSid;
                    }
                }
                else
                {
                    Console.WriteLine("[Walkthrough] Got null user id");
                    _log.Info($"[Walkthrough] Got null user id");
                }
            }
            else
            {
                Console.WriteLine("[Walkthrough] WTSQuerySessionInformation: return false");
                _log.Info($"[Walkthrough] WTSQuerySessionInformation: return false");
            }
            return null;
        }
        /// <summary>
        /// Get User Sid. From SA code
        /// </summary>
        /// <param name="userName"></param>
        /// <returns>User Sid</returns>
        private string GetUserSid(string userName)
        {
            _log.Info($"[Walkthrough] {nameof(GetUserSid)} Start");
            NTAccount f_normal, f_domain = null;
            string accountName = $"{Environment.MachineName}\\{userName}";
            f_normal = new NTAccount(accountName);
            Console.WriteLine($"[Walkthrough] GetUserSid: Machine name: {Environment.MachineName}, User name:{userName}");
            if (!string.IsNullOrEmpty(Environment.UserDomainName))
            {
                accountName = $"{Environment.UserDomainName}\\{userName}";
                Console.WriteLine($"[Walkthrough] GetUserSid: find domain name: {Environment.UserDomainName}, User name:{userName}");
                f_domain = new NTAccount(Environment.UserDomainName, userName);
            }
            String sidString;
            try
            {
                SecurityIdentifier s = (SecurityIdentifier)f_normal.Translate(typeof(SecurityIdentifier));
                sidString = s.ToString();
                Console.WriteLine($"[Walkthrough] GetUserSid(normal user): SID: {sidString}");
            }
            catch (Exception ex)
            {
                sidString = null;
                Console.WriteLine($"[Walkthrough] GetUserSid(normal user): try translate fail: {ex.Message}");

                //0724 add code that translate normal user and do translate domain user if fail.
                if (f_domain != null)
                {
                    try
                    {
                        SecurityIdentifier s = (SecurityIdentifier)f_domain.Translate(typeof(SecurityIdentifier));
                        sidString = s.ToString();
                        Console.WriteLine($"[Walkthrough] GetUserSid(domain user): SID: {sidString}");
                    }
                    catch (Exception e)
                    {
                        sidString = null;
                        Console.WriteLine($"[Walkthrough] GetUserSid(domain user): try translate fail: {e.Message}");
                        _log.Error($"[Walkthrough] GetUserSid(domain user): try translate fail: {e.Message}");
                    }
                }
            }
            return sidString;
        }

        /// <summary>
        /// Check if the device has completed the WalkThrough, and add new devices to the queue
        /// </summary>
        /// <param name="device">DeviceInfo list</param>
        /// <returns>Task</returns>
        private async Task CheckAndQueueDevice(String modelNumber, String modelType)
        {
            _log.Info($"[Walkthrough] {nameof(CheckAndQueueDevice)} Start for ModelNumber {modelNumber}, ModelType {modelType}");
            object regValue;
            _userId = GetActiveUserID();
            string regPath = $@"SOFTWARE\Dell\Dell Display And Peripheral Manager\UserSettings\Local\{_userId}";
            string regKey = $"IsFirstTimeWalkThroughDone_com.dell.DPM.Plugin.LogicalDevice.{modelNumber}";
            string regKeyForDDPM = $"IsFirstTimeWalkThroughDone_com.dell.DPM.Plugin.LogicalDevice.DDPM";

            //Derek 10/24 for Consent screen to share DDPM data with Dell 
            string regPathForConsent = $@"SOFTWARE\Dell\Dell Display And Peripheral Manager\UserSettings\Global\Consent";
            string regKeyForConsent = $"IsFirstTimeLaunchDDPM_com.dell.DPM.Plugin.LogicalDevice.Consent";

            var devicePages = WalkThroughData.WalkThroughData.GetDevicePages((int)DdpmCommonHelper.previousOsTheme);

            try
            {
                if (null == _deviceManager)
                    return;

                //Derek 10/25 for Consent, please don't remove it
                //regValue = await _deviceManager.ReadRegistryData(DDPM.SA.Common.Settings.RegistryHive.LocalMachine, regPathForConsent, regKeyForConsent);

                //if (!Convert.ToBoolean(regValue))
                //{
                //    if (!WalkThroughQueue.Exists(info => info.ModelName == "Consent"))
                //        WalkThroughQueue.Add(new WalkThroughInfo("Consent", "Consent"));
                //}


                regValue = await _deviceManager.ReadRegistryData(DDPM.SA.Common.Settings.RegistryHive.LocalMachine, regPath, regKeyForDDPM);

                if (!Convert.ToBoolean(regValue))
                {
                    if (!WalkThroughQueue.Exists(info => info.ModelName == "DDPM"))
                    {
                        WalkThroughQueue.Add(new WalkThroughInfo("DDPM", "DDPM"));
                    }
                }

                // If the device is not supported, directly update the registry to true and return
                if (!devicePages.ContainsKey(modelNumber))
                {
                    await _deviceManager.WriteRegistryData(DDPM.SA.Common.Settings.RegistryHive.LocalMachine, regPath, regKey, true);
                    _log.Info($"[Walkthrough] Device {modelNumber} not found in devicePages, skipping.");
                    return;
                }

                // read reg
                regValue = await _deviceManager.ReadRegistryData(DDPM.SA.Common.Settings.RegistryHive.LocalMachine, regPath, regKey);

                // mean null or "" or is false, add to the queue and set it to true
                if (regValue == null || (regValue is string strValue && string.IsNullOrEmpty(strValue)) || !Convert.ToBoolean(regValue))
                {
                    // Add the device to the queue and update the registry
                    if (!WalkThroughQueue.Exists(info => info.ModelName == modelNumber))
                    {
                        WalkThroughQueue.Add(new WalkThroughInfo(modelNumber, modelType));
                    }

                    //await _deviceManager.WriteRegistryData(DDPM.SA.Common.Settings.RegistryHive.LocalMachine, regPath, regKey, true);                    
                    _log.Info($"[Walkthrough] Device {modelNumber} added to the queue and registry value updated to true.");
                }
                else
                {
                    _log.Info($"[Walkthrough] Device {modelNumber} reg is true, skipping.");
                }
                await DeviceSort(new WalkThroughInfo(modelNumber, modelType));
            }
            catch (Exception ex)
            {
                _log.Error($"[Walkthrough] Error processing device {modelNumber}: {ex.Message}");
            }
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
                List<MonitorInfo> monitorInfos = _deviceManager.GetMonitors().Result;
                var deviceHelper = _deviceManager.GetDevices().Result;

                // 轉成 WalkThroughInfo 並加入
                foreach (var monitor in monitorInfos)
                {
                    _log.Info($"[Walkthrough] CheckAndQueueDevice Start Add (Monitor)");
                    await CheckAndQueueDevice(monitor.modelName, "Displays");
                }

                foreach (var device in deviceHelper.deviceInfo)
                {
                    _log.Info($"[Walkthrough] CheckAndQueueDevice Start Add (Device)");
                    await CheckAndQueueDevice(device.ModelNumber, device.PhysicalDeviceType.ToString());
                }

                if (WalkThroughQueue.Count != 0 && _showPluginById == false)
                {
                    _log.Info($"[Walkthrough] WalkThroughQueue.Count != 0, ShowPluginById Start");
                    _showPluginManager?.ShowPluginById(DDPM.UI.Common.Constants.WalkThroughPluginId);
                    _showPluginById = true;
                }
            }
            catch (Exception ex)
            {
                _log.Error($"[Walkthrough] {nameof(CollectAndCompareDevicesAsync)} Error collecting devices: {ex.Message}");
            }
        }
        #endregion WalkThrough
    }
}