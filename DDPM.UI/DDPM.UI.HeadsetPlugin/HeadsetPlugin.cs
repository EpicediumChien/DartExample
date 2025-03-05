using CommunityToolkit.Mvvm.DependencyInjection;
using DDPM.SA.Common;
using DDPM.UI.Common;
using DDPM.UI.Interfaces;
using DDPM.UI.Plugin.ViewModels;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.Common.Annotations;
using Dell.Client.Framework.Common.PluginConditions;
using Dell.Client.Framework.UX.WPF;
using Microsoft.Extensions.DependencyInjection;
using NGA.ThickClient.Interfaces;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Forms;
using System.Windows.Input;
using Cursors = System.Windows.Input.Cursors;

namespace DDPM.UI.Plugin.HeadsetPlugin
{
    /// <summary>
    /// Interaction logic for PenPlugin
    /// </summary>
    [Plugin(PluginId, PluginName, Version = PluginVersion, Category = Category.Utility)]
    [Descriptor(Description = Description)]
    [Publisher(Name = "DDPM HeadsetPlugin", Support = "Wistron DDPM Team")]
    [ExcludeFromCodeCoverage]
    public class HeadsetPlugin : IConsoleTakeoverPagePlugin, IConsolePluginSupportsActivations, IThickClientPlugin
    {
        private const string PluginId = UI.Common.Constants.HeadsetPluginId;
        private const string PluginName = "HeadsetPlugin";
        private const string PluginVersion = "1.0";
        private const string Description = "Display Headset page";

        internal static readonly Ioc PluginIoc = new();

        private readonly ILog _log;
        private readonly IConsole _console;
        private readonly IPluginManager _pluginManager;
        private readonly IShowPluginManager _showPluginManager;
        private readonly string? _applicationName;
        private HeadsetViewModel? _viewModel;

        private bool _isConfigured;
        private IDeviceManagerSA _deviceManagerPlugin;
        private IFrameworkPluginConditionNotification? _deviceManagerPluginCondition;
        private CancellationTokenSource StartupCancellationTokenSource { get; } = new();
        private CancellationToken CancellationToken { get; }
        private readonly SemaphoreSlim _lock = new(1, 1);
        private DeviceHelper _deviceHelper = new();
        private List<DeviceInfo> _deviceInfos = new();
        private bool IsEventRegistered = false;
        /// <summary>
        /// Default constructor
        /// </summary>
        public HeadsetPlugin(IShowPluginManager showPluginManager, IPluginManager pluginManager, IConsole console)
        {
            _showPluginManager = showPluginManager;
            _pluginManager = pluginManager;
            _console = console;
            _log = console.CreateLog("Headset");
            _log.Info($"{nameof(LaunchView)} - Constructed");

            CancellationToken = StartupCancellationTokenSource.Token;
            _pluginManager.PluginsStarted += PluginManager_PluginsStarted;
        }

        private void PluginManager_PluginsStarted(object? sender, PluginsStartedEventArgs pluginsStartedEventArgs)
        {
            _log.Info($"[HeadsetPlugin] {nameof(PluginManager_PluginsStarted)} started");
            try
            {
                _deviceManagerPlugin = _pluginManager.FindPluginByType<IDeviceManagerSA>(PluginResolution.Dynamic);

                if (_deviceManagerPlugin == null)
                {
                    _log.Error($"[HeadsetPlugin] {nameof(PluginManager_PluginsStarted)} DeviceManager Plugin is null");
                    return;
                }

                // Manager Peripheralslugin Condition
                _deviceManagerPluginCondition = _deviceManagerPlugin as IFrameworkPluginConditionNotification;

                if (_deviceManagerPluginCondition == null)
                    return;

                // Subscribe to plugin changes
                _deviceManagerPluginCondition.PluginConditionChangeHandler += _peripheralsPluginCondition_PluginConditionChangeHandler;

                // Get current condition
                _ = Task.Run(GetCurrentPeripheralsPluginCondition, CancellationToken);

                _log.Info($"[HeadsetPlugin] {nameof(PluginManager_PluginsStarted)} end");
            }
            catch (Exception ex)
            {
                _log.Error(ex, $"[HeadsetPlugin] PluginManager_PluginsStarted ... failed: {ex.Message}");
            }
        }

        private void DeviceManager_DeviceChanged(object? sender, DeviceChangedEventArgs e)
        {
            _log.Info($"[HeadsetPlugin] {nameof(DeviceManager_DeviceChanged)} in ...");
            try
            {
                if (e.device_peripherals != null && (e.device_peripherals.LogicalDeviceType.Contains("Headset")|| e.device_peripherals.LogicalDeviceType.Contains("AirAudio")))
                {
                    if (e.type == DeviceChangedType.Peripherals_UnPlug)
                    {
                        if (e.device_peripherals.ID == _viewModel!.CurrentDeviceID && _viewModel.CurrentInstanceID == _viewModel.CurrentInstanceID.GetHashCode())
                        {
                            _viewModel.OnGoBackClicked();
                            return;
                        }
                        GetPeripheralsAsync();
                    }
                    _viewModel?.HandleNotification(e.type, e.device_peripherals, e.changedProperty);
                }
                _log.Info($"[HeadsetPlugin] {nameof(DeviceManager_DeviceChanged)} end ...");
            }
            catch (Exception ex)
            {
                _log.Error(ex, $"[HeadsetPlugin] DeviceManager_DeviceChanged ... failed: {ex.Message}");
            }
        }

        private void PeripheralsPlugin_UpdateNotify(object? sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private void _peripheralsPluginCondition_PluginConditionChangeHandler(object? sender, EventArgs e)
        {
            _ = Task.Run(GetCurrentPeripheralsPluginCondition, CancellationToken);
            _log.Info($"[HeadsetPlugin] {nameof(_peripheralsPluginCondition_PluginConditionChangeHandler)} trigger ...");
        }

        private async Task GetCurrentPeripheralsPluginCondition()
        {
            await _lock.WaitAsync(CancellationToken);
            _log.Info($"[HeadsetPlugin] {nameof(GetCurrentPeripheralsPluginCondition)} lock");
            try
            {
                if (_deviceManagerPluginCondition == null)
                    return;

                var pluginCondition = await _deviceManagerPluginCondition.CurrentConditionAsync();

                if (pluginCondition is PluginErrorCondition)
                {
                    _log.Info($"[HeadsetPlugin] {nameof(GetCurrentPeripheralsPluginCondition)} plugin is in {nameof(PluginErrorCondition)}");
                }
                else if (pluginCondition is PluginRunningCondition)
                {
                    _log.Info($"[HeadsetPlugin] {nameof(GetCurrentPeripheralsPluginCondition)} plugin is in {nameof(PluginRunningCondition)}");
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex, $"[HeadsetPlugin] GetCurrentPeripheralsPluginCondition ... failed: {ex.Message}");
            }
            finally
            {
                _lock.Release();
                _log.Info($"[HeadsetPlugin] {nameof(GetCurrentPeripheralsPluginCondition)} unlock");
            }
        }

        private void GetPeripheralsAsync()
        {
            _log.Info($"[HeadsetPlugin] GetPeripheralsAsync ... in");
            try
            {
                if (!SpinWait.SpinUntil(() =>
                _deviceManagerPluginCondition is IFrameworkPluginConditionNotification, TimeSpan.FromMinutes(2)))
                {
                    Console.WriteLine("Could not establish communication with DDPM!!");
                    return;
                }
                //_deviceHelper = await peripheralsPlugin.GetDevices();
                Task<DeviceHelper> task = _deviceManagerPlugin.GetDevices();
                _deviceHelper = task.Result;
                _viewModel?.PrepareDeviceInfo(_deviceHelper.deviceInfo);
                _log.Info($"[HeadsetPlugin] GetPeripheralsAsync ... out");
            }
            catch (Exception ex)
            {
                _log.Error(ex, $"[HeadsetPlugin] GetPeripheralsAsync ... failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Initialize or register services
        /// </summary>
        /// <remarks>Below code will be removed when <see cref="IConsole"/> provides the bootstrapper support</remarks>
        private void ConfigureServices()
        {
            _log.Info($"[HeadsetPlugin] ConfigureServices ... in");
            try
            {
                if (_isConfigured)
                    return;

                // Marked all the instances as singleton
                // Pass the existing _console and _log instance so that Ioc doesn't new'up them
                PluginIoc.ConfigureServices(new ServiceCollection()
                    .AddSingleton(_showPluginManager)
                    .AddSingleton(_console)
                    .AddSingleton(_log)
                    .AddSingleton(_deviceManagerPlugin)
                    .AddSingleton<IPeripheralViewModel, HeadsetViewModel>()
                    .BuildServiceProvider());

                _viewModel = (HeadsetViewModel?)PluginIoc.GetService<IPeripheralViewModel>();
                _isConfigured = true;
                _log.Info($"[HeadsetPlugin] ConfigureServices ... out");
            }
            catch (Exception ex)
            {
                _log.Error(ex, $"[HeadsetPlugin] ConfigureServices ... failed: {ex.Message}");
            }
        }

        public string HeaderText => "Dell Headset";
        public Type PageType => typeof(LaunchView);

        #region Interface IConsolePluginSupportsActivations

        /// <inheritdoc/>
        public void OnActivated()
        {
            _log.Info($"[HeadsetPlugin] OnActivated ... in");
            try
            {
                if (!IsEventRegistered)
                {
                    if (_deviceManagerPlugin == null)
                    {
                        _deviceManagerPlugin = _pluginManager.FindPluginByType<IDeviceManagerSA>(PluginResolution.Dynamic);
                        _log.Info($"[HeadsetPlugin] OnActivated ... _deviceManagerPlugin null,  FindPlugin again ... ");
                    }
                    if (_deviceManagerPlugin != null)
                    {
                        _deviceManagerPlugin.DeviceChanged += DeviceManager_DeviceChanged;
                        IsEventRegistered = true;
                        _log.Info($"[HeadsetPlugin] OnActivated ... _deviceManagerPlugin normal ... ");
                    }
                    else
                    {
                        _log.Info($"[HeadsetPlugin] OnActivated ... _deviceManagerPlugin null,  FindPlugin still failed ... ");
                    }

                }
                Mouse.OverrideCursor = null;
                _log.Info($"[HeadsetPlugin] OnActivated ... out");
            }
            catch (Exception ex)
            {
                _log.Error(ex, $"[HeadsetPlugin] OnActivated ... failed: {ex.Message}");
            }
        }

        /// <inheritdoc/>
        public void OnDeactivated()
        {
            _log.Info($"[HeadsetPlugin] OnDeactivated ... in");
            try
            {
                if (IsEventRegistered)
                {
                    if (_deviceManagerPlugin == null)
                    {
                        _deviceManagerPlugin = _pluginManager.FindPluginByType<IDeviceManagerSA>(PluginResolution.Dynamic);
                        _log.Info($"[HeadsetPlugin] OnDeactivated ... _deviceManagerPlugin null,  FindPlugin again ... ");
                    }
                    if (_deviceManagerPlugin != null)
                    {
                        _deviceManagerPlugin.DeviceChanged -= DeviceManager_DeviceChanged;
                        IsEventRegistered = false;
                        _log.Info($"[HeadsetPlugin] OnDeactivated ... _deviceManagerPlugin normal ... ");
                    }
                    else
                    {
                        _log.Info($"[HeadsetPlugin] OnDeactivated ... _deviceManagerPlugin null,  FindPlugin still failed ... ");
                    }
                }
                Mouse.OverrideCursor = Cursors.Wait;
                _log.Info($"[HeadsetPlugin] OnDeactivated ... out");
            }
            catch (Exception ex)
            {
                _log.Error(ex, $"[HeadsetPlugin] OnDeactivated ... failed: {ex.Message}");
            }
        }

        /// <inheritdoc/>
        public void OnShown(string pluginParameter)
        {
            _log.Info($"[HeadsetPlugin] OnShown ... in");
            try
            {
                if (!IsEventRegistered)
                {
                    if (_deviceManagerPlugin == null)
                    {
                        _deviceManagerPlugin = _pluginManager.FindPluginByType<IDeviceManagerSA>(PluginResolution.Dynamic);
                        _log.Info($"[HeadsetPlugin] OnShown ... _deviceManagerPlugin null,  FindPlugin again ... ");
                    }
                    if (_deviceManagerPlugin != null)
                    {
                        _deviceManagerPlugin.DeviceChanged += DeviceManager_DeviceChanged;
                        IsEventRegistered = true;
                        _log.Info($"[HeadsetPlugin] OnShown ... _deviceManagerPlugin normal ... ");
                    }
                    else
                    {
                        _log.Info($"[HeadsetPlugin] OnShown ... _deviceManagerPlugin null,  FindPlugin still failed ... ");
                    }
                }
                ConfigureServices();
                GetPeripheralsAsync();
                if (_viewModel != null && !_viewModel.SetCurrentDevice(pluginParameter)) { }
                _log.Info($"[HeadsetPlugin] OnShown ... out");
            }
            catch (Exception ex)
            {
                _log.Error(ex, $"[HeadsetPlugin] OnShown ... failed: {ex.Message}");
            }
        }
        #endregion Interface IConsolePluginSupportsActivations

        ~HeadsetPlugin()
        {
            _log.Info($"[HeadsetPlugin] ~SoundBarPlugin ... in");
            _deviceManagerPlugin.DeviceChanged -= DeviceManager_DeviceChanged;
            _log.Info($"[HeadsetPlugin] ~SoundBarPlugin ... out");
        }
    }
}