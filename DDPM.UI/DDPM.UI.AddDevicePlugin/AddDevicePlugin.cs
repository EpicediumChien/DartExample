using CommunityToolkit.Mvvm.DependencyInjection;
using DDPM.SA.Common;
using DDPM.UI.Interfaces;
using DDPM.UI.Plugin.ViewModels;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.Common.Annotations;
using Dell.Client.Framework.Common.PluginConditions;
using Dell.Client.Framework.UX.WPF;
using DPeMPublic.Common.Enums;
using Microsoft.Extensions.DependencyInjection;
using NGA.ThickClient.Interfaces;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Input;

namespace DDPM.UI.Plugin.AddDevicePlugin
{
    /// <summary>
    /// Interaction logic for KeyboardPlugin
    /// </summary>
    [Plugin(PluginId, PluginName, Version = PluginVersion, Category = Category.Utility)]
    [Descriptor(Description = Description)]
    [Publisher(Name = "DDPM AddDevicePlugin", Support = "Wistron DDPM Team")]
    [ExcludeFromCodeCoverage]
    public class AddDevicePlugin : IConsoleTakeoverPagePlugin, IConsolePluginSupportsActivations, IThickClientPlugin
    {
        private const string PluginId = UI.Common.Constants.AddDevicePluginId;
        private const string PluginName = "AddDevice plugin";
        private const string PluginVersion = "1.0";
        private const string Description = "Add Device page";

        internal static readonly Ioc PluginIoc = new();

        private readonly IShowPluginManager _showPluginManager;
        private readonly ILog _log;
        private readonly IConsole _console;
        private readonly IPluginManager _pluginManager;
        private AddDeviceViewModel? _viewModel;
        private bool _isConfigured;

        private IDeviceManagerSA? _deviceManagerPlugin;
        private IFrameworkPluginConditionNotification? _deviceManagerPluginCondition;
        private readonly CancellationTokenSource StartupCancellationTokenSource = new();
        private readonly CancellationToken CancellationToken;
        private readonly SemaphoreSlim _lock = new(1, 1);
        private RFDeviceHelper _deviceHelper = new();

        /// <summary>
        /// Default constructor
        /// </summary>
        public AddDevicePlugin(IShowPluginManager showPluginManager, IPluginManager pluginManager, IConsole console, IGearMenu gearMenu)
        {
            _showPluginManager = showPluginManager;
            _pluginManager = pluginManager;
            _console = console;
            _log = console.CreateLog("AddDevice");
            _log.Info($"{nameof(AddDeviceView)} - Constructed");

            CancellationToken = StartupCancellationTokenSource.Token;
            _pluginManager.PluginsStarted += PluginManager_PluginsStarted;
        }

        private void PluginManager_PluginsStarted(object? sender, PluginsStartedEventArgs pluginsStartedEventArgs)
        {
            _log.Info($"{nameof(PluginManager_PluginsStarted)} started");
            try
            {
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

        private void GetRFDongleAsync()
        {
            if (!SpinWait.SpinUntil(() =>
            _deviceManagerPluginCondition is not null, TimeSpan.FromMinutes(2)))
            {
                Console.WriteLine("Could not establish communication with DDPM!!");
                return;
            }
            _log.Debug($"GetPeripherals is invoked");
            Task<DeviceHelper> tsk = _deviceManagerPlugin!.GetDevices();
            _viewModel!.WacomVersion = tsk.Result.IsdDriverVersion;

            Task<RFDeviceHelper> task = _deviceManagerPlugin!.GetRFDongleDevices();
            _deviceHelper = task.Result;

            _viewModel?.PrepareDongleInfo(_deviceHelper.dongleInfo);
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
                .AddSingleton(_deviceManagerPlugin!)
                .AddSingleton<IAddDeviceViewModel, AddDeviceViewModel>()
                .BuildServiceProvider());

            _viewModel = (AddDeviceViewModel?)PluginIoc.GetService<IAddDeviceViewModel>();
            _isConfigured = true;
        }

        public string HeaderText => "Add Device";
        public Type PageType => typeof(AddDeviceView);

        #region Interface IConsolePluginSupportsActivations

        /// <inheritdoc/>
        public void OnActivated()
        {
            Mouse.OverrideCursor = null;
        }

        /// <inheritdoc/>
        public void OnDeactivated()
        {
            _deviceManagerPlugin!.DeviceChanged -= DeviceChanged;
            _viewModel!.StopPairing();
            //Mouse.OverrideCursor = Cursors.Wait;
        }

        /// <inheritdoc/>
        public void OnShown(string parameter)
        {
            ConfigureServices();
            GetRFDongleAsync();
            _deviceManagerPlugin!.DeviceChanged += DeviceChanged;
            Mouse.OverrideCursor = null;
        }

        #endregion Interface IConsolePluginSupportsActivations

        private void DeviceChanged(object? sender, DeviceChangedEventArgs e)
        {
            if (e.type == DeviceChangedType.Peripherals_PlugIn)
            {
                if (e.changedProperty == "PhysicalDeviceAdded")
                {
                    GetRFDongleAsync();
                    return;
                }
                if (_viewModel!.CurrentDongle != null && e.device_peripherals.PhyscialDeviceID == _viewModel!.CurrentDongle.ID)
                {
                    _viewModel.NewDevice = e.device_peripherals;
                    //if(e.device_peripherals.PhysicalDeviceType == DeviceType.PhysicalAudioDongle)
                    //  _viewModel.GotoNewDevice();
                }
                if (e.device_peripherals.PhysicalDeviceType == DeviceType.PhysicalBluetooth || e.device_peripherals.PhysicalDeviceType == DeviceType.PhysicalBluetoothAudio)
                {
                    _viewModel.NewDevice = e.device_peripherals;
                    _viewModel.GotoNewDevice();
                }
            }
            if (e.type == DeviceChangedType.Peripherals_UnPlug && e.changedProperty == "PhysicalDeviceRemoved")
            {
                GetRFDongleAsync();
                return;
            }
            if (e.device_peripherals?.IsPhysicalDeviceDongle ?? false)
            {
                if (e.type == DeviceChangedType.Peripherals_SettingsChange)
                {
                    _viewModel?.HandleNotification(e.type, e.device_peripherals, e.changedProperty);
                }
                else
                {
                    GetRFDongleAsync();
                }
            }
        }
    }
}