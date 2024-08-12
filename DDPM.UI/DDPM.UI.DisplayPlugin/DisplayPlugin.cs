
using CommunityToolkit.Mvvm.DependencyInjection;
using Dell.Client.Framework.Common.Annotations;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.UX.WPF;
using Microsoft.Extensions.DependencyInjection;
using NGA.ThickClient.Interfaces;
using System.Diagnostics.CodeAnalysis;
using DDPM.UI.Plugin.DisplayPlugin.Views;
using DDPM.UI.Plugin.DdpmHomePlugin.Interfaces;
using DDPM.UI.Plugin.DisplayPlugin.ViewModels;
using DDPM.UI.Plugin.DisplayPlugin.Interfaces;
using DDPM.UI.Plugin.DdpmHomePlugin.ViewModels;
using DDPM.UI.Plugin.DdpmHomePlugin.Model;
using VcpCore.Common;
using DDPM.UI.Common.Models;
using System.Windows.Input;
using Dell.Client.Framework.UX.WPF.Controls;
using System.Drawing;
using System.Windows.Media;
using DDPM.SA.Common;
using DDPM.SA.Common.Interfaces;
using Dell.Client.Framework.Common.PluginConditions;
using DDPM.UI.Plugin.Common;
using DDPM.UI.Plugin.Common.ViewModels;
using DDPM.UI.Common.Interfaces.ViewModels;
using DDPM.UI.Common.ViewModels;
using DDPM.UI.Common;
using System.Windows.Threading;

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
    public class DisplayPlugin : IConsoleTakeoverPagePlugin, IConsolePluginSupportsActivations, IThickClientPlugin
    //IConsolePagePlugin, IConsolePluginSupportsActivations, IThickClientPlugin
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

        #endregion

        /// <summary>
        /// This property is required by the IConsolePagePlugin. It specifies the text to display when the page is shown.
        /// </summary>
        //public string HeaderText => string.Format(Resources.Resources.AboutView_Gear_Text, _applicationName);
        public string HeaderText => "DisplayPlugin";

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
        }

    public void OnActivated() 
    {
      Mouse.OverrideCursor = null;
      //IDeviceInfo deviceInfo =
      //(IDeviceInfo)DdpmHomePlugin.DdpmHomePlugin.PluginIoc.GetServices<IDeviceInfo>();//
    }

    public void OnDeactivated() {
      Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
    }
    private void ConfigureServices() {
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

            PluginIoc.ConfigureServices(services.BuildServiceProvider());

            _viewModel = (DisplayViewModel?)PluginIoc.GetService<IDisplayViewModel>();
            if ( _viewModel != null )
            {
                if (_deviceManagerSAPlugin != null)
                {

                }
                _viewModel.SelectedHomeDevice = DdpmHomePlugin.DdpmHomePlugin.GetSelectedHomeDevice();
                _viewModel.HomeDevices = DdpmHomePlugin.DdpmHomePlugin.GetHomeDevices();
            }
            _isConfigured = true;
        }

        public void OnShown()
        {
            ConfigureServices();
            PrepareHomeDevices();
        }


        /// <summary>
        /// Get HomeDevices and SelectedHomeDevice from DdpmHomePlugin, and add them to IDisplayPageViewModel
        /// </summary>
        private void PrepareHomeDevices()
        {
            //Get IDisplayPageViewModel, it will be created after ConfigureServices() executed
            IDisplayPageViewModel? vmDisplay = PluginIoc.GetService<IDisplayPageViewModel>();

            if ( vmDisplay != null )
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
        #endregion

        #region DDC/CI Status Changed
        private void _deviceManagerSA_DDCCIStatuschanged(object? sender, VcpCore.Common.DDCCIchangedEventArgs e)
        {
            List<HomeDevice> homeDevices = DdpmHomePlugin.DdpmHomePlugin.GetHomeDevices();
            MonitorInfo miChanged = e.monitors;
            
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

        #endregion
    }

}
