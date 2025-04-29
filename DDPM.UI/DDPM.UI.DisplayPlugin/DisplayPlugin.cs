using CommunityToolkit.Mvvm.DependencyInjection;
using DDPM.SA.Common;
using DDPM.SA.Common.Interfaces;
using DDPM.UI.Common;
using DDPM.UI.Common.Interfaces.ViewModels;
using DDPM.UI.Common.Models;
using DDPM.UI.Interfaces;
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
        private WebCameraViewModel? _webCameraViewModel;

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
            }
        }

        ~DisplayPlugin()
        {
            Dispose(false);
        }
        #endregion Exit
    }
}