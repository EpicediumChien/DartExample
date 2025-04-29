using CommunityToolkit.Mvvm.DependencyInjection;
using DDPM.SA.Common;
using DDPM.SA.Common.Interfaces;
using DDPM.UI.Common;
using DDPM.UI.Common.Interfaces.ViewModels;
using DDPM.UI.Common.Models;
using DDPM.UI.Interfaces;
using DDPM.UI.Module.DisplayWebcam;
using DDPM.UI.Plugin.Common.ViewModels;
using DDPM.UI.Plugin.DisplayPlugin.Interfaces;
using DDPM.UI.Plugin.DisplayPlugin.ViewModels;
using DDPM.UI.Plugin.DisplayPlugin.Views;
using DDPM.UI.Plugin.ViewModels;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.Common.Annotations;
using Dell.Client.Framework.Common.PluginConditions;
using Dell.Client.Framework.UX.WPF;
using Dell.Client.Framework.UX.WPF.Controls;
using Microsoft.Extensions.DependencyInjection;
using NGA.ThickClient.Interfaces;
using System.Diagnostics.CodeAnalysis;
using System.Reflection.Metadata;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Threading;
using VcpCore.Common;
using Windows.Media.AppRecording;

namespace DDPM.UI.Plugin.DisplayPlugin
{
    /// <summary>
    /// Interaction logic for AboutView Plugin.xaml
    /// </summary>
    [Plugin(PluginId, PluginName, Version = PluginVersion, Category = Category.Utility)]
    [Descriptor(Description = TileDetailText)]
    [Publisher(Name = "DDPM DisplayPlugin", Support = "DDPM Wistron Team")]
    [PluginRequires(Id = DDPM.SA.Common.IDs.Device_Manager_Plugin_ID, AllowDynamicResolving = true)]
    [PluginRequires(Id = DDPM.SA.Common.IDs.DDPM_EAPlugin_PLUGIN_ID, AllowDynamicResolving = true)]
    //   [PluginRequires(Id = DDPM.SA.Common.IDs.PipPbp_Manager_PLUGIN_ID, AllowDynamicResolving = true)]
    [ExcludeFromCodeCoverage]
    public class DisplayPlugin : IConsolePagePlugin, IConsolePluginSupportsActivations, IThickClientPlugin, IDisposable
    //IConsolePagePlugin, IConsoleTakeoverPagePlugin, IConsolePluginSupportsActivations, IThickClientPlugin
    {
        #region Private
        private const string PluginId = DDPM.UI.Common.Constants.DisplayPluginId;
        private const string PluginName = "DDPM.UI.Plugin.DisplayPlugin";
        private const string PluginVersion = "1.0";
        private const string TileDetailText = "DDPM.UI.Plugin.DisplayPlugin";

        internal static readonly Ioc PluginIoc = new();

        private readonly ILog _log;
        private readonly IConsole _console;
        private readonly IPluginManager _pluginManager;
        //private readonly string? _applicationName;
        private bool _isConfigured;
        private bool _isActivated = false;

        //Members for DeviceManager
        private IDeviceManagerSA _deviceManagerSAPlugin;
        private IFrameworkPluginConditionNotification? _deviceManagerPluginCondition;

        //Members for EasyArrange plugin
        private IEasyArrangeService _easyArrangePlugin;
        private IFrameworkPluginConditionNotification? _easyArrangePluginCondition;

        private CancellationTokenSource StartupCancellationTokenSource { get; } = new();
        private CancellationToken CancellationToken { get; }
        private readonly SemaphoreSlim _lock = new(1, 1);

        //Robert_Lin, 2024-6-27 using the single view model in DisplayPlugin, copy from other plugins (for example, MousePlugin)
        private DisplayViewModel? _viewModel;
        private DeviceHelper _deviceHelper = new();

        private WebCameraViewModel? _webCameraViewModel;
        private DispatcherTimer timer = new();
        #endregion Private

        /// <summary>
        /// This property is required by the IConsolePagePlugin. It specifies the text to display when the page is shown.
        /// </summary>
        //public string HeaderText => string.Format(Resources.Resources.AboutView_Gear_Text, _applicationName);
        public string HeaderText => "Dell Display";

        /// <summary>
        /// Page Type
        /// </summary>
        //public Type PageType => typeof(AboutView);
        public Type PageType => typeof(DisplayPage);

        /// <summary>
        /// Default constructor, DdpmSample Plugin
        /// </summary>
        //public DisplayPlugin(IConsole console, IGearMenu gearMenu)
        public DisplayPlugin(IWindowLayout windowLayout, IConsole console, IDispatcherWrapper dispatcherWrapper, IPluginManager pluginManager)
        {
            _console = console;
            _pluginManager = pluginManager;
            _log = console.CreateLog("DisplayPLG");
            _log.Info($"{nameof(DisplayPlugin)} - Constructed");

            UXMasthead? masthead = windowLayout.Masthead;
            //masthead.Visibility = System.Windows.Visibility.Collapsed;
            //masthead.Background = new SolidColorBrush(Colors.Yellow);

            //_applicationName = LocalizationManager.Instance?.ResourceManager?.GetString("ApplicationName", null);
            //_applicationName = "DDPM";

            /* Don't need to add item to gear menu
            //var aboutGearItem = new GearMenuItem(string.Format(Resources.Resources.AboutView_Gear_Text, _applicationName), new RelayCommand(ShowAboutView));
            var aboutGearItem = new GearMenuItem("DDPM 2.0", new RelayCommand(ShowAboutView));

            gearMenu.AddGearMenuItem(aboutGearItem, 0);
            */

            CancellationToken = StartupCancellationTokenSource.Token;
            _pluginManager.PluginsStarted += PluginManager_PluginsStarted;

            //Robert_Lin, 2025-1-2, the handler is registered in DeviceBasePageViewModel.cs
            //So will comment-out the registeration and handler in DisplayPlugin.
            //Robert_Lin, 2024-12-21 Register a event handler to handle when mainwindow
            // move to new position
            //_console.RegisterForEvent(ConsoleEventNames.MainWindow_MoveToNewPosition, Handle_MainWindow_MoveToNewPosition);

            timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(5)
            };
            timer.Tick += Timer_Tick;
            if (_console != null)
            {
                _console.RegisterForEvent(ConsoleEventNames.MainWindow_Activate, MainWindowActivate);
                _console.RegisterForEvent(ConsoleEventNames.MainWindow_DeActivate, MainWindowDeActivate);
                _console.RegisterForEvent(ConsoleEventNames.MainWindow_ConsoleWindow_Closed, MainWindowClosed);
            }

        }

        public void OnActivated()
        {
            _log?.Info("[DisplayPlugin] OnActivated()");
            Mouse.OverrideCursor = null;
            _isActivated = true;
            DdpmCommonHelper.IsDisplayPluginActivated = true;
            //IDeviceInfo deviceInfo =
            //(IDeviceInfo)DdpmHomePlugin.DdpmHomePlugin.PluginIoc.GetServices<IDeviceInfo>();//
        }

        public void OnDeactivated()
        {
            _log?.Info("[DisplayPlugin] OnDeactivated()");

            if (DdpmCommonHelper.DeviceManagerSA != null)
            {
                DdpmCommonHelper.DeviceManagerSA.DeviceChanged -= DeviceManager_DeviceChanged;
                DdpmCommonHelper.DeviceManagerSA.UIUpdateNotify -= DisplayPlugin_UIUpdateNotify;
            }

            Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
            _isActivated = false;
            DdpmCommonHelper.IsDisplayPluginActivated = false;
        }
        private void ConfigureServices()
        {
            _log?.Info("[DisplayPlugin] ConfigureServices ... in");
            if (_isConfigured)
                return;

            // Marked all the instances as singleton
            // Pass the existing _console and _log instance so that Ioc doesn't new'up them
            /*
            AboutPluginIoc.ConfigureServices(new ServiceCollection()
                .AddSingleton(_console)
                .AddSingleton(_log)
                .AddSingleton<IAboutViewModel, AboutViewModel>()
                .AddSingleton<IProductInfoService, ProductInfoService>()
                .AddSingleton<IProductInfo, ProductInfo>()
                .BuildServiceProvider());
            */
            /*
            PluginIoc.ConfigureServices(new ServiceCollection()
                .AddSingleton(_console)
                .AddSingleton(_log)
                .AddSingleton(_deviceManagerSAPlugin)
                //.AddSingleton(_easyArrangePlugin)
                .AddSingleton<IDisplayPageViewModel, DisplayPageViewModel>()
                //.AddSingleton<IDisplayViewModel, DisplayViewModel>()
                //.AddSingleton<IProductInfoService, ProductInfoService>()
                //.AddSingleton<IProductInfo, ProductInfo>()
                .BuildServiceProvider());
            */
            ServiceCollection services = new ServiceCollection();
            if (_console != null)
                services.AddSingleton(_console);
            if (_log != null)
                services.AddSingleton(_log);
            if (_deviceManagerSAPlugin != null)
                services.AddSingleton(_deviceManagerSAPlugin);
            if (_easyArrangePlugin != null)
                services.AddSingleton(_easyArrangePlugin);
            services.AddSingleton<IDisplayPageViewModel, DisplayPageViewModel>();
            services.AddSingleton<IDisplayViewModel, DisplayViewModel>();

            services.AddSingleton<IPeripheralViewModel, WebCameraViewModel>();
            PluginIoc.ConfigureServices(services.BuildServiceProvider());

            _webCameraViewModel = (WebCameraViewModel?)PluginIoc.GetService<IPeripheralViewModel>();

            _viewModel = (DisplayViewModel?)PluginIoc.GetService<IDisplayViewModel>();
            if (_viewModel != null)
            {
                if (_deviceManagerSAPlugin != null)
                {
                }
                _viewModel.SelectedHomeDevice = DdpmHomePlugin.DdpmHomePlugin.GetSelectedHomeDevice();
                _viewModel.HomeDevices = DdpmHomePlugin.DdpmHomePlugin.GetHomeDevices();
            }
            _isConfigured = true;
            _log?.Info("[DisplayPlugin] ConfigureServices ... out");
        }

        public void OnShown()
        {
            _log?.Info("[DisplayPlugin] OnShown() ... in");
            ConfigureServices();
            PrepareHomeDevices();

            GetPeripheralsAsync();
            if (_webCameraViewModel != null)
            {
                //TODO: pass Monitor associate webcam deviceid from DdpmHomepage or anywhere
                if (0 != _webCameraViewModel.DeviceInfos.Count)
                    _webCameraViewModel.SetCurrentDevice(_webCameraViewModel.DeviceInfos.Keys.First().ToString());
            }

            if (DdpmCommonHelper.DeviceManagerSA != null)
            {
                DdpmCommonHelper.DeviceManagerSA.DeviceChanged += DeviceManager_DeviceChanged;
                DdpmCommonHelper.DeviceManagerSA.UIUpdateNotify += DisplayPlugin_UIUpdateNotify;
            }

            _log?.Info("[DisplayPlugin] OnShown() ... out");
        }

        /// <summary>
        /// Get HomeDevices and SelectedHomeDevice from DdpmHomePlugin, and add them to IDisplayPageViewModel
        /// </summary>
        private void PrepareHomeDevices()
        {
            _log?.Info("[DisplayPlugin] PrepareHomeDevices ... in");
            //Get IDisplayPageViewModel, it will be created after ConfigureServices() executed
            IDisplayPageViewModel? vmDisplay = PluginIoc.GetService<IDisplayPageViewModel>();

            if (vmDisplay != null)
            {
                //Get the selected HomeDevice
                vmDisplay.SelectedHomeDevice = DdpmHomePlugin.DdpmHomePlugin.GetSelectedHomeDevice();

                //Collect Monitor devices
                List<HomeDevice> monitors = new List<HomeDevice>();
                foreach (HomeDevice obj in DdpmHomePlugin.DdpmHomePlugin.GetHomeDevices())
                {
                    // 20240617 jim modify
                    if (obj.DeviceCategory == DDPM.UI.Common.eDeviceCategory.Display)
                        monitors.Add(obj);
                }
                //Assign to DisplayPageViewModel
                vmDisplay.HomeDevices = monitors;
            }
            _log?.Info("[DisplayPlugin] PrepareHomeDevices ... out");
        }

        #region Plugin related
        private void PluginManager_PluginsStarted(object? sender, PluginsStartedEventArgs pluginsStartedEventArgs)
        {
            _log.Info($"{nameof(PluginManager_PluginsStarted)} started");
            if (_pluginManager != null)
            {
                try
                {
                    InitializePipPbpPlugin();
                    InitializeDeviceManagerPlugin();
                    InitializeEasyArrangePlugin();
                }
                catch (Exception e1)
                {
                    var message = $"{nameof(PluginManager_PluginsStarted)} exception: {e1.Message}";
                    _log.Error(e1, message);
                }
            }
        }

        private void InitializeDeviceManagerPlugin()
        {
            if (_deviceManagerSAPlugin != null)
                return;

            _deviceManagerSAPlugin = _pluginManager.FindPluginByType<IDeviceManagerSA>(PluginResolution.Dynamic);
            if (_deviceManagerSAPlugin == null)
            {
                _log.Error($"{nameof(InitializeDeviceManagerPlugin)}: _deviceManagerSAPlugin is null");
                return;
            }

            _deviceManagerPluginCondition = _deviceManagerSAPlugin as IFrameworkPluginConditionNotification;
            if (_deviceManagerPluginCondition == null)
            {
                _log.Error($"{nameof(InitializeDeviceManagerPlugin)}: _deviceManagerPluginCondition is null");
                return;
            }

            // Subscribe to plugin changes
            _deviceManagerPluginCondition.PluginConditionChangeHandler += DeviceManagerPluginCondition_PluginConditionChangeHandler;

            // Get current condition
            _ = Task.Run(GetCurrentDeviceManagerePluginCondition, CancellationToken);
        }

        private async Task GetCurrentDeviceManagerePluginCondition()
        {
            await _lock.WaitAsync(CancellationToken);
            _log.Trace($"{nameof(GetCurrentDeviceManagerePluginCondition)} lock");
            try
            {
                if (_deviceManagerPluginCondition == null)
                    return;

                var pluginCondition = await _deviceManagerPluginCondition.CurrentConditionAsync();

                if (pluginCondition is PluginErrorCondition)
                {
                    _log.Info($"{nameof(GetCurrentDeviceManagerePluginCondition)}: plugin is in {nameof(PluginErrorCondition)}");
                }
                else if (pluginCondition is PluginRunningCondition)
                {
                    _log.Info($"{nameof(GetCurrentDeviceManagerePluginCondition)}: plugin is in {nameof(PluginRunningCondition)}");
                    HomeDevice.DeviceManagerSA = _deviceManagerSAPlugin;

                    //Robert_Lin, 2024-8-6 Register event handler for DDC/CI status change event
                    _deviceManagerSAPlugin.DDCCIStatuschanged += _deviceManagerSA_DDCCIStatuschanged;
                }
            }
            catch (Exception ex)
            {
                var message = $"{nameof(GetCurrentDeviceManagerePluginCondition)}: exception, {ex.Message}";
                _log.Error(ex, message);
            }
            finally
            {
                _lock.Release();
                _log.Trace($"{nameof(GetCurrentDeviceManagerePluginCondition)} unlock");
            }
        }

        private void DeviceManagerPluginCondition_PluginConditionChangeHandler(object? sender, EventArgs e)
        {
            _ = Task.Run(GetCurrentDeviceManagerePluginCondition, CancellationToken);
        }

        private void InitializeEasyArrangePlugin()
        {
            if (_easyArrangePlugin != null)
                return;

            _easyArrangePlugin = _pluginManager.FindPluginByType<IEasyArrangeService>(PluginResolution.Dynamic);
            if (_easyArrangePlugin == null)
            {
                _log.Error($"{nameof(PluginManager_PluginsStarted)} EasyArrange Plugin is null");
                return;
            }
            _easyArrangePluginCondition = _easyArrangePlugin as IFrameworkPluginConditionNotification;
            if (_easyArrangePluginCondition == null)
            {
                _log.Error("_easyArrangePluginCondition is null");
                return;
            }

            // Subscribe to plugin changes
            _easyArrangePluginCondition.PluginConditionChangeHandler += EasyArrangePluginCondition_PluginConditionChangeHandler;

            // Get current condition
            _ = Task.Run(GetCurrentEasyArrangePluginCondition, CancellationToken);
        }

        private void EasyArrangePluginCondition_PluginConditionChangeHandler(object? sender, EventArgs e)
        {
            _ = Task.Run(GetCurrentEasyArrangePluginCondition, CancellationToken);
        }

        private async Task GetCurrentEasyArrangePluginCondition()
        {
            await _lock.WaitAsync(CancellationToken);
            _log.Trace($"{nameof(GetCurrentEasyArrangePluginCondition)} lock");
            try
            {
                if (_easyArrangePluginCondition == null)
                    return;

                var pluginCondition = await _easyArrangePluginCondition.CurrentConditionAsync();

                if (pluginCondition is PluginErrorCondition)
                {
                    _log.Info($"{nameof(GetCurrentEasyArrangePluginCondition)} plugin is in {nameof(PluginErrorCondition)}");
                }
                else if (pluginCondition is PluginRunningCondition)
                {
                    _log.Info($"{nameof(GetCurrentEasyArrangePluginCondition)} plugin is in {nameof(PluginRunningCondition)}");
                    //HomeDevice.EasyArrangeService = _easyArrangePlugin;
                }
            }
            catch (Exception ex)
            {
                var message = $"{nameof(GetCurrentEasyArrangePluginCondition)} failed with error - {ex.Message}";
                _log.Error(ex, message);
                //throw new NotificationPluginException(message);
            }
            finally
            {
                _lock.Release();
                _log.Trace($"{nameof(GetCurrentEasyArrangePluginCondition)} unlock");
            }
        }

        //Robert_Lin Debug, try to get IPipPgpManager
        private IPipPbpService _pipPbpPlugin;
        private IFrameworkPluginConditionNotification? _pipPbpPluginCondition;

        private void InitializePipPbpPlugin()
        {
            if (_pipPbpPlugin != null)
                return;

            _pipPbpPlugin = _pluginManager.FindPluginByType<IPipPbpService>(PluginResolution.Dynamic);
            if (_pipPbpPlugin == null)
            {
                _log.Error($"{nameof(PluginManager_PluginsStarted)} PipPbp Plugin is null");
                return;
            }
            _pipPbpPluginCondition = _pipPbpPlugin as IFrameworkPluginConditionNotification;
            if (_pipPbpPluginCondition == null)
            {
                _log.Error("_pipPbpPluginCondition is null");
                return;
            }

            // Subscribe to plugin changes
            _pipPbpPluginCondition.PluginConditionChangeHandler += PipPbpPluginCondition_PluginConditionChangeHandler;

            // Get current condition
            _ = Task.Run(GetCurrentPipPbpPluginCondition, CancellationToken);
        }

        private void PipPbpPluginCondition_PluginConditionChangeHandler(object? sender, EventArgs e)
        {
            _ = Task.Run(GetCurrentPipPbpPluginCondition, CancellationToken);
        }

        private async Task GetCurrentPipPbpPluginCondition()
        {
            await _lock.WaitAsync(CancellationToken);
            _log.Trace($"{nameof(GetCurrentPipPbpPluginCondition)} lock");
            try
            {
                if (_pipPbpPluginCondition == null)
                    return;

                var pluginCondition = await _pipPbpPluginCondition.CurrentConditionAsync();

                if (pluginCondition is PluginErrorCondition)
                {
                    _log.Info($"{nameof(GetCurrentPipPbpPluginCondition)} plugin is in {nameof(PluginErrorCondition)}");
                }
                else if (pluginCondition is PluginRunningCondition)
                {
                    _log.Info($"{nameof(GetCurrentPipPbpPluginCondition)} plugin is in {nameof(PluginRunningCondition)}");
                }
            }
            catch (Exception ex)
            {
                var message = $"{nameof(GetCurrentPipPbpPluginCondition)} failed with error - {ex.Message}";
                _log.Error(ex, message);
                //throw new NotificationPluginException(message);
            }
            finally
            {
                _lock.Release();
                _log.Trace($"{nameof(GetCurrentPipPbpPluginCondition)} unlock");
            }
        }
        #endregion Plugin related

        #region DDC/CI Status Changed
        private void _deviceManagerSA_DDCCIStatuschanged(object? sender, VcpCore.Common.DDCCIchangedEventArgs e)
        {
            List<HomeDevice> homeDevices = DdpmHomePlugin.DdpmHomePlugin.GetHomeDevices();
            MonitorInfo miChanged = e.monitors;

            _log?.Info($"_deviceManagerSA_DDCCIStatuschanged(mo: {miChanged.modelName}, {miChanged.edid.ServiceTag}), DDC/CI is ON? {e.DDCisON}");

            foreach (HomeDevice homeDev in homeDevices)
            {
                if (homeDev.DeviceCategory != eDeviceCategory.Display)
                    continue;

                //If the homeDev is the monitor that DDC/CI is changed
                if (HomeDevice.IsSameMonitor(homeDev.MonitorInfo, miChanged, "DDCisON"))
                {
                    //Update the DDCisON status
                    homeDev.MonitorInfo.DDCisON = e.DDCisON;
                }
                else
                {
                    //Robert_Lin, 2024-8-6, not the monitor which DDC/CI is chnaged
                    //then nothing to do.
                    //homeDev.MonitorInfo.DDCisON = isDdcCiOn;
                }
            }
        }

        #endregion DDC/CI Status Changed

        #region MainWindow Move To new position - Unused
        /*
        /// <summary>
        /// Handle the IConsole Event, MainWindow_MoveToNewPosition, when DDPM main window
        /// move to a new position.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Handle_MainWindow_MoveToNewPosition(object sender, EventManagerArgs e)
        {
            //TO DO:
            //Action_1: Determine to Monitor where DDPM is moved to
            //Action_2: Show the model name OSD (if the monitor is supported monitor)
            //Action_3: Change the SelectedItem of Monitors combobox (if DisplayPlugin is activated)
            _log?.Info($"@Handle_MainWindow_MoveToNewPosition() is called. DisplayPugin.IsActivated={_isActivated}");

            double dpiX = DDPM.SA.Common.CommonFunctions.GetDpiX();
            string monitorDisplayName = "";
            //Checking event args
            if (e != null &&
                e.Tag != null)
            {
                monitorDisplayName = e.Tag?.ToString();

                //Get mouse cursor position
                System.Drawing.Point cursorPosition = System.Windows.Forms.Cursor.Position;
                //Get the Screen of the cursor
                System.Windows.Forms.Screen screenOfCursor = System.Windows.Forms.Screen.FromPoint(cursorPosition);
                if (_deviceManagerSAPlugin != null)
                {
                    //Get AllMonitors
                    List<MonitorInfo> monitors = _deviceManagerSAPlugin.GetMonitors().Result;
                }
                if (_viewModel != null &&
                    _viewModel.HomeDevices != null)
                {
                    HomeDevice? inPlaceDevice = _viewModel.HomeDevices.Find(x => x.DisplayName == screenOfCursor.DeviceName);
                    if (inPlaceDevice != null &&
                        _deviceManagerSAPlugin != null)
                    {
                        _deviceManagerSAPlugin.ShowOSD(inPlaceDevice.MonitorInfo, OSDType.DisplayChanged);
                    }
                    
                }

                System.Drawing.Point ptMouse = System.Windows.Forms.Control.MousePosition;
                System.Drawing.Point mousePos = new System.Drawing.Point(ptMouse.X, ptMouse.Y);

                mousePos.X = (int)((double)mousePos.X * dpiX);
                mousePos.Y = (int)((double)mousePos.Y * dpiX);
                System.Windows.Forms.Screen screen1 = System.Windows.Forms.Screen.FromPoint(mousePos);

                VcpCore.Common.User32.POINTL ptCursor = new VcpCore.Common.User32.POINTL();
                VcpCore.Common.User32._GetCursorPos(out ptCursor);
                System.Drawing.Point cursorPos = new System.Drawing.Point(ptCursor.x, ptCursor.y);
                cursorPos.X = (int)((double)cursorPos.X / dpiX);
                cursorPos.Y = (int)((double)cursorPos.Y / dpiX);
                System.Windows.Forms.Screen screen2 = System.Windows.Forms.Screen.FromPoint(cursorPos);

                monitorDisplayName = screen2.DeviceName;                
            }

            //Dispatcher.Invoke(new Action(() =>
            //{
            //    this.Activate();
            //}));
        }
        */
        #endregion  MainWindow Move To new position

        #region Exit
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (DdpmCommonHelper.DeviceManagerSA != null)
                {
                    DdpmCommonHelper.DeviceManagerSA.DeviceChanged -= DeviceManager_DeviceChanged;
                    DdpmCommonHelper.DeviceManagerSA.UIUpdateNotify -= DisplayPlugin_UIUpdateNotify;
                }
            }
        }

        ~DisplayPlugin()
        {
            Dispose(false);
        }
        #endregion Exit

        #region Display WebCam Part

        private void Timer_Tick(object? sender, EventArgs e)
        {
            //if (!_viewModel!.DeviceInfos.ContainsKey(CurrentDeviceID))
            _webCameraViewModel!.OnGoBackClicked();
            timer.Stop();
        }

        private void GetPeripheralsAsync()
        {
            DdpmCommonHelper.WriteUILog($"GetPeripherals is invoked ... in");
            if (DdpmCommonHelper.DeviceManagerSA != null)
            {
                Task<DeviceHelper> task = DdpmCommonHelper.DeviceManagerSA.GetDevices(); //(true);
                _deviceHelper = task.Result;
            }

            _webCameraViewModel?.PrepareDeviceInfo(_deviceHelper.deviceInfo);
            DdpmCommonHelper.WriteUILog($"GetPeripherals is invoked ... out");
        }

        private void DeviceManager_DeviceChanged(object? sender, DeviceChangedEventArgs e)
        {
            try
            {
                if (e.device_peripherals != null && e.device_peripherals.LogicalDeviceType != null)
                {
                    if (e.device_peripherals.LogicalDeviceType.Contains("Webcam"))
                    {
                        if (e.type == DeviceChangedType.Peripherals_UnPlug)
                        {
                            if (e.device_peripherals.ID == _webCameraViewModel!.CurrentDeviceID)
                            {
                                if (_webCameraViewModel.IsMicEnumerationOnEnabled)
                                {
                                    _webCameraViewModel.AlertType = WebcamAlert.Alert4;
                                    _webCameraViewModel.AlertVisibility = System.Windows.Visibility.Visible;
                                    timer.Start();
                                    //_viewModel!.OnGoBackClicked();
                                }
                            }
                            if (_webCameraViewModel?.IsRecording == true)
                            {
                                _webCameraViewModel!.IsRecording = false;
                                _webCameraViewModel!.OnGoBackClicked();
                            }
                            return;
                        }
                        if (e.type == DeviceChangedType.Peripherals_PlugIn)
                        {
                            if (e.device_peripherals.Name == _webCameraViewModel?.CurrentDeviceInfo?.Name)
                            {
                                timer.Stop();
                                //Mouse.OverrideCursor = null;
                                //_viewModel.CurrentCursor = Cursors.Arrow;
                                //_viewModel.IsMicEnumerationOnEnabled = true;
                                _webCameraViewModel.AlertVisibility = Visibility.Collapsed;
                                _console.ShowPluginById(PluginId);
                                GetPeripheralsAsync();
                                _webCameraViewModel!.SetCurrentDevice(e.device_peripherals.ID.ToString());
                            }
                            if (_webCameraViewModel?.IsRecording ?? true)
                                return;
                            return;
                        }
                        _webCameraViewModel?.HandleNotification(e.type, e.device_peripherals, e.changedProperty);
                    }
                    else
                    {
                        if (e.type == DeviceChangedType.Peripherals_UnPlug || e.type == DeviceChangedType.Peripherals_PlugIn)
                            _webCameraViewModel!.OnGoBackClicked();
                    }
                }
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog("DDPM.UI.DisplayPlugin\\DisplayPlugin.cs  DeviceManager_DeviceChanged() ex:" + ex.Message);
            }
        }

        private void DisplayPlugin_UIUpdateNotify(object? sender, UpdateUINotify e)
        {
            //Derek 1212
            if (e == null || e == EventArgs.Empty || e.UI_Field_Name == null
                || string.IsNullOrEmpty(e.UI_Field_Name))
                return;

            //Open this to get the message format of Webcam event
            //System.Windows.MessageBox.Show(e.UI_Field_Name);
            Console.WriteLine("WebCameraplugin_UIUpdateNotify Get event : " + e.UI_Field_Name + "#" + DateTime.Now.ToString("yyyy-MM-dd h:mm:tt") + "\r\n");
            DdpmCommonHelper.WriteUILog("WebCameraplugin_UIUpdateNotify Get event : " + e.UI_Field_Name + "#" + DateTime.Now.ToString("yyyy-MM-dd h:mm:tt") + "\r\n");

            //cmd format sample
            //5;Device:Webcam;EventType:Webcam_IsHDROnChanged;DeviceId:28d64fee-3544-45c7-a1b0-10db20a4cf8e;NewValue:True
            Dictionary<string, string> event_param = deal_param(e.UI_Field_Name);
            try
            {
                if (!event_param.TryGetValue("Device", out var device))
                {
                    DdpmCommonHelper.WriteUILog("Device cannot be found in event_param");
                    return;
                }
                if (device == "Webcam")
                {
                    if (!event_param.TryGetValue("EventType", out var eventtype))
                    {
                        DdpmCommonHelper.WriteUILog("EventType cannot be found in event_param");
                        return;
                    }


                    switch (eventtype)
                    {
                        #region for cli setting
                        case "Webcam_IsAllSupportedResolutionsFoundChanged":
                            {
                                if (_webCameraViewModel?.IsRecording == true)
                                {
                                    _webCameraViewModel.IsRecording = false;
                                    _webCameraViewModel!.OnGoBackClicked();
                                }
                            }
                            break;
                        case "Webcam_ZoomMeetingTypeChanged":
                            {
                                //No corresponding UI
                            }
                            break;
                        case "Webcam_SharpnessChanged":
                            {
                                if (!event_param.TryGetValue("NewValue", out var NewValue))
                                {
                                    DdpmCommonHelper.WriteUILog("NewValue cannot be found in event_param");
                                    return;
                                }
                                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                                {
                                    if (int.TryParse(NewValue, out var _t))
                                        _webCameraViewModel!.Sharpness = _t;
                                    else
                                    {
                                        DdpmCommonHelper.WriteUILog("Webcam_SharpnessChanged NewValue not int");
                                        return;
                                    }
                                });
                            }
                            break;

                        case "Webcam_Esi_WALLockCountdownChanged":
                            {
                                //??
                            }
                            break;
                        case "Webcam_IsZoomScreenShareActiveChanged":
                            {
                                //No corresponding UI
                            }
                            break;
                        case "Webcam_IsZoomMeetingActiveChanged":
                            {
                                //No corresponding UI
                            }
                            break;
                        case "Webcam_SerialNumberChanged":
                            {
                                //??
                            }
                            break;
                        //case "Webcam_IsHDROnChanged":
                        //{
                        //    if (!event_param.TryGetValue("NewValue", out var NewValue))
                        //    {
                        //        DdpmCommonHelper.WriteUILog("NewValue cannot be found in event_param");
                        //        return;
                        //    }
                        //    System.Windows.Application.Current.Dispatcher.Invoke(() =>
                        //    {
                        //        //_viewModel!.IsSettingProfile = true;
                        //        if (NewValue.ToLower() == "true")
                        //            _viewModel.IsHDROn = true;
                        //        else
                        //            _viewModel.IsHDROn = false;
                        //        //_viewModel!.IsSettingProfile = false;
                        //    });
                        //    //HDR SIWTCH�ɭ�,�ݭn���mCAMERA,�����ݭn�@�q��l�Ʈɶ���1��
                        //    //_viewModel!.mre.Set();
                        //    Thread.Sleep(1000);
                        //    _viewModel!.mre.Set();
                        //    _viewModel!.hdr_change = false;
                        //}
                        //break;
                        case "Webcam_FieldOfViewChanged":
                            {
                                if (!event_param.TryGetValue("NewValue", out var NewValue))
                                {
                                    DdpmCommonHelper.WriteUILog("NewValue cannot be found in event_param");
                                    return;
                                }
                                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                                {
                                    if (int.TryParse(NewValue, out var _t))
                                    {
                                        _webCameraViewModel!.FieldOfView = _t;

                                        //Derek 1211 to sync data with QAM
                                        if (90 == _t)
                                        {
                                            _webCameraViewModel!.SetFOV_Selected(2);
                                            _webCameraViewModel.SelectedFovIndex = 2;
                                        }
                                        else if (78 == _t)
                                        {
                                            _webCameraViewModel!.SetFOV_Selected(1);
                                            _webCameraViewModel.SelectedFovIndex = 1;
                                        }
                                        else if (65 == _t)
                                        {
                                            _webCameraViewModel!.SetFOV_Selected(0);
                                            _webCameraViewModel.SelectedFovIndex = 0;
                                        }
                                    }
                                    else
                                    {
                                        DdpmCommonHelper.WriteUILog("Webcam_FieldOfViewChanged NewValue not int");
                                        return;
                                    }
                                });

                            }
                            break;
                        case "Webcam_AutoFramingFrameSizeChanged":
                            {
                                //if (!event_param.TryGetValue("NewValue", out var NewValue))
                                //{
                                //    DdpmCommonHelper.WriteUILog("NewValue cannot be found in event_param");
                                //    return;
                                //}
                                //System.Windows.Application.Current.Dispatcher.Invoke(() =>
                                //{
                                //    if (int.TryParse(NewValue, out var _t))
                                //        _viewModel!.AutoFramingFrameSize = _t;
                                //    else
                                //    {
                                //        DdpmCommonHelper.WriteUILog("Webcam_AutoFramingFrameSizeChanged NewValue not int");
                                //        return;
                                //    }
                                //});
                            }
                            break;
                        case "Webcam_AutoFramingSensitivityChanged":
                            {
                                //if (!event_param.TryGetValue("NewValue", out var NewValue))
                                //{
                                //    DdpmCommonHelper.WriteUILog("NewValue cannot be found in event_param");
                                //    return;
                                //}
                                //System.Windows.Application.Current.Dispatcher.Invoke(() =>
                                //{
                                //    if (int.TryParse(NewValue, out var _t))
                                //        _viewModel!.AutoFramingFrameSize = _t;
                                //    else
                                //    {
                                //        DdpmCommonHelper.WriteUILog("Webcam_AutoFramingSensitivityChanged NewValue not int");
                                //        return;
                                //    }
                                //});
                            }
                            break;
                        case "Webcam_IsAutoFramingOnChanged":
                            {
                                if (!event_param.TryGetValue("NewValue", out var NewValue))
                                {
                                    DdpmCommonHelper.WriteUILog("NewValue cannot be found in event_param");
                                    return;
                                }
                                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                                {
                                    if (NewValue.ToLower() == "true" && !_webCameraViewModel!.IsAutoFramingOn)
                                        _webCameraViewModel!.IsAutoFramingOn = true;
                                    if (NewValue.ToLower() != "true" && _webCameraViewModel!.IsAutoFramingOn)
                                        _webCameraViewModel!.IsAutoFramingOn = false;
                                });
                            }
                            break;
                        case "Webcam_IsAutoFramingTransitionOnChanged":
                            {
                                if (!event_param.TryGetValue("NewValue", out var NewValue))
                                {
                                    DdpmCommonHelper.WriteUILog("NewValue cannot be found in event_param");
                                    return;
                                }
                                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                                {
                                    if (NewValue.ToLower() == "true")
                                        _webCameraViewModel!.IsAutoFramingTransitionOn = true;
                                    else
                                        _webCameraViewModel!.IsAutoFramingTransitionOn = false;
                                });
                            }
                            break;
                        case "Webcam_AutoWhiteBalanceChanged":
                            {
                                if (!event_param.TryGetValue("NewValue", out var NewValue))
                                {
                                    DdpmCommonHelper.WriteUILog("NewValue cannot be found in event_param");
                                    return;
                                }
                                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                                {
                                    if (int.TryParse(NewValue, out var _t))
                                        _webCameraViewModel!.AutoWhiteBalance = _t;
                                    else
                                    {
                                        DdpmCommonHelper.WriteUILog("Webcam_AutoWhiteBalanceChanged NewValue not int");
                                        return;
                                    }
                                });
                            }
                            break;
                        case "Webcam_IsAutoWhiteBalanceOnChanged":
                            {
                                if (!event_param.TryGetValue("NewValue", out var NewValue))
                                {
                                    DdpmCommonHelper.WriteUILog("NewValue cannot be found in event_param");
                                    return;
                                }
                                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                                {
                                    if (NewValue.ToLower() == "true")
                                        _webCameraViewModel!.IsAutoWhiteBalanceOn = true;
                                    else
                                        _webCameraViewModel!.IsAutoWhiteBalanceOn = false;
                                });
                            }
                            break;
                        case "Webcam_SaturationChanged":
                            {
                                if (!event_param.TryGetValue("NewValue", out var NewValue))
                                {
                                    DdpmCommonHelper.WriteUILog("NewValue cannot be found in event_param");
                                    return;
                                }
                                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                                {
                                    if (int.TryParse(NewValue, out var _t))
                                        _webCameraViewModel!.Saturation = _t;
                                    else
                                    {
                                        DdpmCommonHelper.WriteUILog("Webcam_SaturationChanged NewValue not int");
                                        return;
                                    }
                                });
                            }
                            break;
                        case "Webcam_AntiFlickerChanged":
                            {
                                //1:50 2:60
                                if (!event_param.TryGetValue("NewValue", out var NewValue))
                                {
                                    DdpmCommonHelper.WriteUILog("NewValue cannot be found in event_param");
                                    return;
                                }
                                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                                {
                                    if (int.TryParse(NewValue, out var _t))
                                        _webCameraViewModel!.AntiFlicker = _t;
                                    else
                                    {
                                        DdpmCommonHelper.WriteUILog("Webcam_AntiFlickerChanged NewValue not int");
                                        return;
                                    }
                                });
                            }
                            break;
                        case "Webcam_ContrastChanged":
                            {
                                if (!event_param.TryGetValue("NewValue", out var NewValue))
                                {
                                    DdpmCommonHelper.WriteUILog("NewValue cannot be found in event_param");
                                    return;
                                }
                                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                                {
                                    if (int.TryParse(NewValue, out var _t))
                                        _webCameraViewModel!.Contrast = _t;
                                    else
                                    {
                                        DdpmCommonHelper.WriteUILog("Webcam_ContrastChanged NewValue not int");
                                        return;
                                    }
                                });
                            }
                            break;
                        case "Webcam_BrightnessChanged":
                            {
                                if (!event_param.TryGetValue("NewValue", out var NewValue))
                                {
                                    DdpmCommonHelper.WriteUILog("NewValue cannot be found in event_param");
                                    return;
                                }
                                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                                {
                                    if (int.TryParse(NewValue, out var _t))
                                        _webCameraViewModel!.Brightness = _t;
                                    else
                                    {
                                        DdpmCommonHelper.WriteUILog("Webcam_BrightnessChanged NewValue not int");
                                        return;
                                    }
                                });
                            }
                            break;
                        case "Webcam_ZoomChanged":
                            {
                                if (!event_param.TryGetValue("NewValue", out var NewValue))
                                {
                                    DdpmCommonHelper.WriteUILog("NewValue cannot be found in event_param");
                                    return;
                                }
                                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                                {
                                    if (int.TryParse(NewValue, out var _t))
                                        _webCameraViewModel!.Zoom = _t;
                                    else
                                    {
                                        DdpmCommonHelper.WriteUILog("Webcam_ZoomChanged NewValue not int");
                                        return;
                                    }
                                });
                            }
                            break;
                        case "Webcam_TiltChanged":
                            {
                                //??
                            }
                            break;

                        case "Webcam_PanChanged":
                            {
                                //??
                            }
                            break;

                        case "Webcam_FocusChanged":
                            {
                                if (!event_param.TryGetValue("NewValue", out var NewValue))
                                {
                                    DdpmCommonHelper.WriteUILog("NewValue cannot be found in event_param");
                                    return;
                                }
                                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                                {
                                    if (int.TryParse(NewValue, out var _t))
                                        _webCameraViewModel!.Focus = _t;
                                    else
                                    {
                                        DdpmCommonHelper.WriteUILog("Webcam_FocusChanged NewValue not int");
                                        return;
                                    }
                                });
                            }
                            break;

                        case "Webcam_IsFocusOnChanged":
                            {
                                if (!event_param.TryGetValue("NewValue", out var NewValue))
                                {
                                    DdpmCommonHelper.WriteUILog("NewValue cannot be found in event_param");
                                    return;
                                }
                                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                                {
                                    if (NewValue.ToLower() == "true")
                                        _webCameraViewModel!.IsFocusOn = true;
                                    else
                                        _webCameraViewModel!.IsFocusOn = false;
                                });
                            }
                            break;

                        case "Webcam_PriorityChanged":
                            {
                                if (!event_param.TryGetValue("NewValue", out var NewValue))
                                {
                                    DdpmCommonHelper.WriteUILog("NewValue cannot be found in event_param");
                                    return;
                                }
                                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                                {
                                    if (int.TryParse(NewValue, out var _t))
                                        _webCameraViewModel!.Priority = _t;
                                    else
                                    {
                                        DdpmCommonHelper.WriteUILog("Webcam_PriorityChanged NewValue not int");
                                        return;
                                    }
                                });

                            }
                            break;

                        case "Webcam_CustomProfileRemoved":
                            {

                            }
                            break;

                        case "Webcam_CustomProfileAdded":
                            {

                            }
                            break;

                        case "Webcam_CurrentSelectedProfileChanged":
                            {

                            }
                            break;

                        case "Webcam_IsMicEnumerationOnChanged":
                            {
                                if (!event_param.TryGetValue("NewValue", out var NewValue))
                                {
                                    DdpmCommonHelper.WriteUILog("NewValue cannot be found in event_param");
                                    return;
                                }
                                _webCameraViewModel!.isIsMicEnumerationOnChanged_event = true;

                                DdpmCommonHelper.WriteUILog("WebCameraMicrophone Action 6 : " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                                {
                                    if (NewValue.ToLower() == "true")
                                    {
                                        DdpmCommonHelper.WriteUILog("WebCameraMicrophone Action 7 : " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                                        _webCameraViewModel!.IsMicEnumerationOn = true;
                                    }
                                    else
                                    {
                                        DdpmCommonHelper.WriteUILog("WebCameraMicrophone Action 8 : " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                                        _webCameraViewModel!.IsMicEnumerationOn = false;
                                    }
                                });
                                DdpmCommonHelper.WriteUILog("WebCameraMicrophone Action 9 : " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                                _webCameraViewModel!.isIsMicEnumerationOnChanged_event = false;
                            }
                            break;

                        case "Webcam_ProfileManagerAdded":
                            {

                            }
                            break;
                        #endregion
                        case "Webcam_Esi_IsCameraSensorCoveredChanged":
                            {
                                if (!event_param.TryGetValue("NewValue", out var NewValue))
                                {
                                    DdpmCommonHelper.WriteUILog("[WebCameraPlugin] [Webcam_Esi_IsCameraSensorCoveredChanged] NewValue cannot be found in event_param");
                                    return;
                                }

                                if (NewValue.ToLower() == "true")
                                {
                                    var _globalSettings = DdpmCommonHelper.DeviceManagerSA?.GetGlobalSettingParam().Result;
                                    DdpmCommonHelper.WriteUILog($"[WebCameraPlugin][Webcam_Esi_IsCameraSensorCoveredChanged] Webcam_WB7022_Presence_Detection_Sensor_Cover_State={_globalSettings.GlobalSetting_General.Webcam_WB7022_Presence_Detection_Sensor_Cover_State}");

                                    // check if show OSD for Presence Detection Sensor Cover
                                    if (_globalSettings != null && _globalSettings.GlobalSetting_General.Webcam_WB7022_Presence_Detection_Sensor_Cover_State)
                                    {
                                        if (DdpmCommonHelper.DeviceManagerSA != null)
                                        {
                                            DdpmCommonHelper.DeviceManagerSA.ShowOSD(Screen.PrimaryScreen!.DeviceName, OSDType.Fingerprint);
                                            DdpmCommonHelper.WriteUILog($"[WebCameraPlugin][Webcam_Esi_IsCameraSensorCoveredChanged] show OSD");
                                            _webCameraViewModel.IsChecked_Snooze = false; // jim 20241221 add 
                                        }
                                    }
                                }
                            }
                            break;

                        // Jim 20250104 comment out for PIMS-335905 [DDPM Win 2.0][R19] Observe no WAL Countdown OSD is seen when WAL is act=tivated
                        /*
                        case "Webcam_Esi_IsWALLockCountdownStartedChanged":
                        {
                            if (!event_param.TryGetValue("NewValue", out var NewValue))
                            {
                                DdpmCommonHelper.WriteUILog("NewValue cannot be found in event_param");
                                return;
                            }

                            if (NewValue.ToLower() == "true")
                                DdpmCommonHelper.DeviceManagerSA?.ShowOSD(Screen.PrimaryScreen!.DeviceName, OSDType.WalkAwayLock);
                        }
                        break;
                        */

                        case "Webcam_WALSnoozeTimeLeftInSecondsChanged":
                            {
                                if (!event_param.TryGetValue("NewValue", out var NewValue))
                                {
                                    DdpmCommonHelper.WriteUILog("NewValue cannot be found in event_param");
                                    return;
                                }

                                if (!String.IsNullOrEmpty(NewValue))
                                {
                                    if (Int32.TryParse(NewValue, out int numValue))
                                    {
                                        TimeSpan ts = TimeSpan.FromSeconds(numValue);
                                        _webCameraViewModel.WALSnoozeTimeLeft = ts.ToString(@"hh\:mm\:ss");
                                    }
                                }
                            }
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog("Displayplugin_UIUpdateNotify:" + ex.Message);
            }
        }

        private Dictionary<string, string> deal_param(string param)
        {
            Dictionary<string, string> tmp = new Dictionary<string, string>();

            try
            {
                List<string> list = param.Split(new char[] { ';' }).ToList();
                foreach (string s in list)
                {
                    List<string> item = s.Split(new char[] { ':' }).ToList();
                    if (item.Count == 2)
                    {
                        tmp.Add(item[0], item[1]);
                    }
                }
            }
            catch (Exception ex) // Jim 20250108 add exception handling for PIMS-335905 on ARM has crash
            {
                DdpmCommonHelper.WriteUILog($"deal_param {ex.Message}");
                tmp.Clear();
            }
            return tmp;
        }

        private void MainWindowActivate(object sender, EventManagerArgs e)
        {
            this._log?.Write(LogMsgType.Debug, "Display plugin receive MainWindow Activate event");
            if (_webCameraViewModel == null)
                return;
            _webCameraViewModel.running_state = true;
            _webCameraViewModel.mre.Set();
        }

        private void MainWindowDeActivate(object sender, EventManagerArgs e)
        {
            this._log?.Write(LogMsgType.Debug, "Display plugin receive MainWindow DeActivate event");

            if (_webCameraViewModel == null)
                return;
            _webCameraViewModel.running_state = false;
            _webCameraViewModel.mre.Set();
        }

        private void MainWindowClosed(object sender, EventManagerArgs e)
        {
            this._log?.Write(LogMsgType.Debug, "Display plugin receive MainWindow DeActivate event");

            if (_webCameraViewModel == null)
                return;

            try
            {
                _webCameraViewModel.OnMainWindowClosed();

                foreach (Thread t in _webCameraViewModel.thread_list)
                {
                    if (t != null)
                    {
                        Console.WriteLine(t.Name + " " + t.IsAlive);

                        if (t.IsAlive)
                        {
                            t.Interrupt();
                        }

                    }
                    else
                    {
                        Console.WriteLine("Null thread");
                    }
                }

                Console.WriteLine("D:\\DDPM\\DDPM.UI\\DDPM.UI.DisplayPlugin\\DisplayPlugin.cs MainWindowClosed 2");
            }
            catch (Exception ex)
            {
                Console.WriteLine("D:\\DDPM\\DDPM.UI\\DDPM.UI.DisplayPlugin\\DisplayPlugin.cs MainWindowClosed 2 ex:" + ex.ToString());
                this._log?.Write(LogMsgType.Debug, $"DisplayPlugin plugin got exception: {ex.ToString()}");
            }


            this._log?.Write(LogMsgType.Debug, "DisplayPlugin plugin receive MainWindow DeActivate event - End");
        }
    }
    #endregion
}