using CommunityToolkit.Mvvm.DependencyInjection;
using DDPM.SA.Common;
using DDPM.UI.Common.Interfaces.ViewModels;
using DDPM.UI.Interfaces;
using DDPM.UI.Plugin.Common.ViewModels;
using DDPM.UI.Plugin.BootloaderPlugin.Views;
using DDPM.UI.Plugin.ViewModels;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.Common.Annotations;
using Dell.Client.Framework.Common.PluginConditions;
using Dell.Client.Framework.UX.WPF;
using Microsoft.Extensions.DependencyInjection;
using NGA.ThickClient.Interfaces;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Input;

namespace DDPM.UI.Plugin.BootloaderPlugin
{
    /// <summary>
    /// Interaction logic for AboutBootloaderView Plugin.xaml
    /// </summary>
    [Plugin(PluginId, PluginName, Version = PluginVersion, Category = Category.Utility)]
    [Descriptor(Description = TileDetailText)]
    [Publisher(Name = "DDPM BootloaderPlugin", Support = "Wistron DDPM Team")]
    [ExcludeFromCodeCoverage]
    public class BootloaderPlugin : IConsolePagePlugin, IConsolePluginSupportsActivations, IThickClientPlugin
    {
        private const string PluginId = UI.Common.Constants.BootloaderPluginId;
        private const string PluginName = "Bootloader plugin";
        private const string PluginVersion = "1.0";
        private const string TileDetailText = "Display Bootloader page";

        internal static readonly Ioc PluginIoc = new();

        private readonly ILog _log;
        private readonly IConsole _console;
        private readonly IPluginManager _pluginManager;
        private readonly IShowPluginManager _showPluginManager;
        private readonly string? _applicationName;
        private BootloaderViewModel? _viewModel;

        private bool _isConfigured;
        private IDeviceManagerSA? _deviceManagerPlugin;
        private IFrameworkPluginConditionNotification? _deviceManagerPluginCondition;
        private readonly CancellationTokenSource StartupCancellationTokenSource = new();
        private readonly CancellationToken CancellationToken;
        private readonly SemaphoreSlim _lock = new(1, 1);
        private DeviceHelper _deviceHelper = new();
        private readonly List<DeviceInfo> _deviceInfos = new();

        /// <summary>
        /// Default constructor
        /// </summary>
        public BootloaderPlugin(IPluginManager pluginManager, IConsole console, IShowPluginManager showPluginManager)
        {
            _pluginManager = pluginManager;
            _showPluginManager = showPluginManager;
            _console = console;
            _log = console.CreateLog("Bootloader");
            _log.Info($"{nameof(BootloaderPage)} - Constructed");

            CancellationToken = StartupCancellationTokenSource.Token;
            _pluginManager.PluginsStarted += PluginManager_PluginsStarted;
        }

        private void PluginManager_PluginsStarted(object? sender, PluginsStartedEventArgs pluginsStartedEventArgs)
        {
            _log.Info($"{nameof(PluginManager_PluginsStarted)} started");
            try
            {
                if (_deviceManagerPlugin != null)
                    return;

                _deviceManagerPlugin = _pluginManager.FindPluginByType<IDeviceManagerSA>(PluginResolution.Dynamic);

                if (_deviceManagerPlugin == null)
                {
                    _log.Error($"{nameof(PluginManager_PluginsStarted)} DeviceManager Plugin is null");
                    return;
                }

                // Manager Peripheralslugin Condition
                _deviceManagerPluginCondition = _deviceManagerPlugin as IFrameworkPluginConditionNotification;

                if (_deviceManagerPluginCondition == null)
                    return;

                // Subscribe to plugin changes
                _deviceManagerPluginCondition.PluginConditionChangeHandler += PeripheralsPluginCondition_PluginConditionChangeHandler;

                // Get current condition
                _ = Task.Run(GetCurrentPeripheralsPluginCondition, CancellationToken);
            }
            catch (Exception ex)
            {
                var message = $"{nameof(PluginManager_PluginsStarted)} failed: {ex.Message}";
                _log.Error(ex, message);
            }
        }

        private void DeviceManager_DeviceChanged(object? sender, DeviceChangedEventArgs e)
        {
            if (e != null && e.device_peripherals != null && _viewModel != null)
            {
                if (e.type == DeviceChangedType.Peripherals_UnPlug)
                {
                    if (e.device_peripherals.ID == _viewModel!.CurrentDeviceID && _viewModel.CurrentInstanceID == 0)
                    {
                        _viewModel.OnGoBackClicked();
                        return;
                    }
                    GetPeripheralsAsync();
                }
                _viewModel?.HandleNotification(e.type, e.device_peripherals, e.changedProperty);
            }
        }

        private void PeripheralsPlugin_UpdateNotify(object? sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private void PeripheralsPluginCondition_PluginConditionChangeHandler(object? sender, EventArgs e)
        {
            _ = Task.Run(GetCurrentPeripheralsPluginCondition, CancellationToken);
        }

        private async Task GetCurrentPeripheralsPluginCondition()
        {
            await _lock.WaitAsync(CancellationToken);
            _log.Trace($"{nameof(GetCurrentPeripheralsPluginCondition)} lock");
            try
            {
                if (_deviceManagerPluginCondition == null)
                    return;

                var pluginCondition = await _deviceManagerPluginCondition.CurrentConditionAsync();

                if (pluginCondition is PluginErrorCondition)
                {
                    _log.Info($"{nameof(GetCurrentPeripheralsPluginCondition)} plugin is in {nameof(PluginErrorCondition)}");
                }
                else if (pluginCondition is PluginRunningCondition)
                {
                    _log.Info($"{nameof(GetCurrentPeripheralsPluginCondition)} plugin is in {nameof(PluginRunningCondition)}");
                }
            }
            catch (Exception ex)
            {
                var message = $"{nameof(GetCurrentPeripheralsPluginCondition)} failed with error - {ex.Message}";
                _log.Error(ex, message);
                //throw new NotificationPluginException(message);
            }
            finally
            {
                _lock.Release();
                _log.Trace($"{nameof(GetCurrentPeripheralsPluginCondition)} unlock");
            }
        }

        private void GetPeripheralsAsync()
        {
            _log.Info($"[BootloaderPlugin] GetPeripheralsAsync is invoked ... in");
            if (!SpinWait.SpinUntil(() =>
            _deviceManagerPluginCondition is not null, TimeSpan.FromMinutes(2)))
            {
                Console.WriteLine("Could not establish communication with DDPM!!");
                return;
            }

            //_deviceHelper = await peripheralsPlugin.GetDevices();
            Task<DeviceHelper> task = _deviceManagerPlugin!.GetDevices();
            _deviceHelper = task.Result;

            _viewModel?.PrepareDeviceInfo(_deviceHelper.deviceInfo);
            _log.Info($"[BootloaderPlugin] GetPeripheralsAsync is invoked ... out");
        }

        /// <summary>
        /// Initialize or register services
        /// </summary>
        /// <remarks>Below code will be removed when <see cref="IConsole"/> provides the bootstrapper support</remarks>
        private void ConfigureServices()
        {
            _log.Info($"[BootloaderPlugin] ConfigureServices ... in");
            if (_isConfigured)
                return;

            // Marked all the instances as singleton
            // Pass the existing _console and _log instance so that Ioc doesn't new'up them
            ServiceCollection services = new ServiceCollection();
            if (_console != null)
                services.AddSingleton(_console);
            if (_log != null)
                services.AddSingleton(_log);
            if (_deviceManagerPlugin != null)
                services.AddSingleton(_deviceManagerPlugin);
            if (_showPluginManager != null)
                services.AddSingleton(_showPluginManager);
            services.AddSingleton<IPeripheralViewModel, BootloaderViewModel>();
            PluginIoc.ConfigureServices(services.BuildServiceProvider());

            _viewModel = (BootloaderViewModel?)PluginIoc.GetService<IPeripheralViewModel>();
            _isConfigured = true;
            _log.Info($"[BootloaderPlugin] ConfigureServices ... out");
        }

        public string HeaderText => "Bootloader";
        public Type PageType => typeof(BootloaderPage);

        #region Interface IConsolePluginSupportsActivations

        /// <inheritdoc/>
        public void OnActivated()
        {
            _deviceManagerPlugin!.DeviceChanged += DeviceManager_DeviceChanged;
            //_deviceManagerPlugin.UpdateNotify += PeripheralsPlugin_UpdateNotify;
            Mouse.OverrideCursor = null;
        }

        /// <inheritdoc/>
        public void OnDeactivated()
        {
            _deviceManagerPlugin!.DeviceChanged -= DeviceManager_DeviceChanged;
            //_deviceManagerPlugin.UpdateNotify -= PeripheralsPlugin_UpdateNotify;
            Mouse.OverrideCursor = Cursors.Wait;
        }

        /// <inheritdoc/>
        public void OnShown(string pluginParameter)
        {
            _log.Info($"Bootloader pugin OnShown Begin timestamp: {DateTime.Now:hh:mm:ss.ffffff}");
            ConfigureServices();
            GetPeripheralsAsync();
            if (_viewModel != null && !_viewModel.SetCurrentDevice(pluginParameter)) { }
            _log.Info($"Bootloader pugin OnShown End timestamp: {DateTime.Now:hh:mm:ss.ffffff}");
        }

        #endregion Interface IConsolePluginSupportsActivations

        ~BootloaderPlugin()
        {
            _deviceManagerPlugin!.DeviceChanged -= DeviceManager_DeviceChanged;
        }
    }
}