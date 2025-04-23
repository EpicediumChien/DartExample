using CommunityToolkit.Mvvm.DependencyInjection;
using DDPM.SA.Common;
using DDPM.UI.Interfaces;
using DDPM.UI.Plugin.ViewModels;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.Common.Annotations;
using Dell.Client.Framework.Common.PluginConditions;
using Dell.Client.Framework.UX.WPF;
using Microsoft.Extensions.DependencyInjection;
using NGA.ThickClient.Interfaces;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Input;
using Cursors = System.Windows.Input.Cursors;

namespace DDPM.UI.Plugin.RtkHubPlugin
{
    /// <summary>
    /// Interaction logic for PenPlugin
    /// </summary>
    [Plugin(PluginId, PluginName, Version = PluginVersion, Category = Category.Utility)]
    [Descriptor(Description = Description)]
    [Publisher(Name = "DDPM RtkHubPlugin", Support = "Wistron DDPM Team")]
    [ExcludeFromCodeCoverage]
    public class RtkHubPlugin : IConsolePagePlugin, IConsolePluginSupportsActivations, IThickClientPlugin
    {
        private const string PluginId = UI.Common.Constants.RtkHubPluginId;
        private const string PluginName = "RtkHubPlugin";
        private const string PluginVersion = "1.0";
        private const string Description = "Display RtkHub page";

        internal static readonly Ioc PluginIoc = new();

        private readonly ILog? _log = null;
        private readonly IConsole _console;
        private readonly IPluginManager _pluginManager;
        private readonly IShowPluginManager _showPluginManager;
        private readonly string? _applicationName;
        private RtkHubViewModel? _viewModel;

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
        public RtkHubPlugin(IPluginManager pluginManager, IConsole console, IShowPluginManager showPluginManager)
        {
            _pluginManager = pluginManager;
            _console = console;
            _showPluginManager = showPluginManager;
            _log = console.CreateLog("RtkHub");
            _log?.Info($"{nameof(LaunchView)} - Constructed");

            CancellationToken = StartupCancellationTokenSource.Token;
            _pluginManager.PluginsStarted += PluginManager_PluginsStarted;
        }

        private void PluginManager_PluginsStarted(object? sender, PluginsStartedEventArgs pluginsStartedEventArgs)
        {
            _log?.Info($"[RtkHubPlugin] {nameof(PluginManager_PluginsStarted)} started");
            try
            {
                if (_deviceManagerPlugin != null)
                    return;

                _deviceManagerPlugin = _pluginManager.FindPluginByType<IDeviceManagerSA>(PluginResolution.Dynamic);

                if (_deviceManagerPlugin == null)
                {
                    _log?.Error($"[RtkHubPlugin] {nameof(PluginManager_PluginsStarted)} DeviceManager Plugin is null");
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

                _log?.Info($"[RtkHubPlugin] {nameof(PluginManager_PluginsStarted)} end");
            }
            catch (Exception ex)
            {
                _log?.Error(ex, $"[RtkHubPlugin] PluginManager_PluginsStarted ... failed: {ex.Message}");
            }
        }

        private void DeviceManager_DeviceChanged(object? sender, DeviceChangedEventArgs e)
        {
            _log?.Info($"[RtkHubPlugin] {nameof(DeviceManager_DeviceChanged)} in ...");
            try
            {
                if (_viewModel != null)
                {
                    if (e.device_peripherals != null && (e.device_peripherals.LogicalDeviceType.Contains("RtkHub")))
                    {
                        if (e.type == DeviceChangedType.Peripherals_UnPlug)
                        {
                            if (e.device_peripherals.ID == _viewModel.CurrentDeviceID && _viewModel.CurrentInstanceID == _viewModel.CurrentInstanceID.GetHashCode())
                            {
                                _viewModel.OnGoBackClicked();
                                return;
                            }
                            GetPeripheralsAsync();
                        }
                        _viewModel?.HandleNotification(e.type, e.device_peripherals, e.changedProperty);
                    }
                    _log?.Info($"[RtkHubPlugin] {nameof(DeviceManager_DeviceChanged)} end ...");
                }
                else
                {
                    _log?.Info($"[RtkHubPlugin] {nameof(DeviceManager_DeviceChanged)} _viewModel is null.");
                }


            }
            catch (Exception ex)
            {
                _log?.Error(ex, $"[RtkHubPlugin] DeviceManager_DeviceChanged ... failed: {ex.Message}");
            }
        }

        private void PeripheralsPlugin_UpdateNotify(object? sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private void _peripheralsPluginCondition_PluginConditionChangeHandler(object? sender, EventArgs e)
        {
            _ = Task.Run(GetCurrentPeripheralsPluginCondition, CancellationToken);
            _log?.Info($"[RtkHubPlugin] {nameof(_peripheralsPluginCondition_PluginConditionChangeHandler)} trigger ...");
        }

        private async Task GetCurrentPeripheralsPluginCondition()
        {
            await _lock.WaitAsync(CancellationToken);
            _log?.Info($"[RtkHubPlugin] {nameof(GetCurrentPeripheralsPluginCondition)} lock");
            try
            {
                if (_deviceManagerPluginCondition == null)
                    return;

                var pluginCondition = await _deviceManagerPluginCondition.CurrentConditionAsync();

                if (pluginCondition is PluginErrorCondition)
                {
                    _log?.Info($"[RtkHubPlugin] {nameof(GetCurrentPeripheralsPluginCondition)} plugin is in {nameof(PluginErrorCondition)}");
                }
                else if (pluginCondition is PluginRunningCondition)
                {
                    _log?.Info($"[RtkHubPlugin] {nameof(GetCurrentPeripheralsPluginCondition)} plugin is in {nameof(PluginRunningCondition)}");
                }
            }
            catch (Exception ex)
            {
                _log?.Error(ex, $"[RtkHubPlugin] GetCurrentPeripheralsPluginCondition ... failed: {ex.Message}");
            }
            finally
            {
                _lock.Release();
                _log?.Info($"[RtkHubPlugin] {nameof(GetCurrentPeripheralsPluginCondition)} unlock");
            }
        }

        private void GetPeripheralsAsync()
        {
            _log?.Info($"[RtkHubPlugin] GetPeripheralsAsync ... in");
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
                _log?.Info($"[RtkHubPlugin] GetPeripheralsAsync ... out");
            }
            catch (Exception ex)
            {
                _log?.Error(ex, $"[RtkHubPlugin] GetPeripheralsAsync ... failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Initialize or register services
        /// </summary>
        /// <remarks>Below code will be removed when <see cref="IConsole"/> provides the bootstrapper support</remarks>
        private void ConfigureServices()
        {
            _log?.Info($"[RtkHubPlugin] ConfigureServices ... in");
            try
            {
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
                services.AddSingleton<IPeripheralViewModel, RtkHubViewModel>();
                PluginIoc.ConfigureServices(services.BuildServiceProvider());

                _viewModel = (RtkHubViewModel?)PluginIoc.GetService<IPeripheralViewModel>();
                _isConfigured = true;
                _log?.Info($"[RtkHubPlugin] ConfigureServices ... out");
            }
            catch (Exception ex)
            {
                _log?.Error(ex, $"[RtkHubPlugin] ConfigureServices ... failed: {ex.Message}");
            }
        }

        public string HeaderText => "Dell RtkHub";
        public Type PageType => typeof(LaunchView);

        #region Interface IConsolePluginSupportsActivations

        /// <inheritdoc/>
        public void OnActivated()
        {
            _log?.Info($"[RtkHubPlugin] OnActivated ... in");
            try
            {
                if (!IsEventRegistered)
                {
                    if (_deviceManagerPlugin == null)
                    {
                        _deviceManagerPlugin = _pluginManager.FindPluginByType<IDeviceManagerSA>(PluginResolution.Dynamic);
                        _log?.Info($"[RtkHubPlugin] OnActivated ... _deviceManagerPlugin null,  FindPlugin again ... ");
                    }
                    if (_deviceManagerPlugin != null)
                    {
                        _deviceManagerPlugin.DeviceChanged += DeviceManager_DeviceChanged;
                        IsEventRegistered = true;
                        _log?.Info($"[RtkHubPlugin] OnActivated ... _deviceManagerPlugin normal ... ");
                    }
                    else
                    {
                        _log?.Info($"[RtkHubPlugin] OnActivated ... _deviceManagerPlugin null,  FindPlugin still failed ... ");
                    }

                }
                Mouse.OverrideCursor = null;
                _log?.Info($"[RtkHubPlugin] OnActivated ... out");
            }
            catch (Exception ex)
            {
                _log?.Error(ex, $"[RtkHubPlugin] OnActivated ... failed: {ex.Message}");
            }
        }

        /// <inheritdoc/>
        public void OnDeactivated()
        {
            _log?.Info($"[RtkHubPlugin] OnDeactivated ... in");
            try
            {
                if (IsEventRegistered)
                {
                    if (_deviceManagerPlugin == null)
                    {
                        _deviceManagerPlugin = _pluginManager.FindPluginByType<IDeviceManagerSA>(PluginResolution.Dynamic);
                        _log?.Info($"[RtkHubPlugin] OnDeactivated ... _deviceManagerPlugin null,  FindPlugin again ... ");
                    }
                    if (_deviceManagerPlugin != null)
                    {
                        _deviceManagerPlugin.DeviceChanged -= DeviceManager_DeviceChanged;
                        IsEventRegistered = false;
                        _log?.Info($"[RtkHubPlugin] OnDeactivated ... _deviceManagerPlugin normal ... ");
                    }
                    else
                    {
                        _log?.Info($"[RtkHubPlugin] OnDeactivated ... _deviceManagerPlugin null,  FindPlugin still failed ... ");
                    }
                }
                Mouse.OverrideCursor = Cursors.Wait;
                _log?.Info($"[RtkHubPlugin] OnDeactivated ... out");
            }
            catch (Exception ex)
            {
                _log?.Error(ex, $"[RtkHubPlugin] OnDeactivated ... failed: {ex.Message}");
            }
        }

        /// <inheritdoc/>
        public void OnShown(string pluginParameter)
        {
            _log?.Info($"[RtkHubPlugin] OnShown ... in");
            try
            {
                if (!IsEventRegistered)
                {
                    if (_deviceManagerPlugin == null)
                    {
                        _deviceManagerPlugin = _pluginManager.FindPluginByType<IDeviceManagerSA>(PluginResolution.Dynamic);
                        _log?.Info($"[RtkHubPlugin] OnShown ... _deviceManagerPlugin null,  FindPlugin again ... ");
                    }
                    if (_deviceManagerPlugin != null)
                    {
                        _deviceManagerPlugin.DeviceChanged += DeviceManager_DeviceChanged;
                        IsEventRegistered = true;
                        _log?.Info($"[RtkHubPlugin] OnShown ... _deviceManagerPlugin normal ... ");
                    }
                    else
                    {
                        _log?.Info($"[RtkHubPlugin] OnShown ... _deviceManagerPlugin null,  FindPlugin still failed ... ");
                    }
                }
                ConfigureServices();
                GetPeripheralsAsync();
                if (_viewModel != null && !_viewModel.SetCurrentDevice(pluginParameter)) { }
                _log?.Info($"[RtkHubPlugin] OnShown ... out");
            }
            catch (Exception ex)
            {
                _log?.Error(ex, $"[RtkHubPlugin] OnShown ... failed: {ex.Message}");
            }
        }
        #endregion Interface IConsolePluginSupportsActivations

        ~RtkHubPlugin()
        {
            _log?.Info($"[RtkHubPlugin] ~RtkHubPlugin ... in");
            _deviceManagerPlugin.DeviceChanged -= DeviceManager_DeviceChanged;
            _log?.Info($"[RtkHubPlugin] ~RtkHubPlugin ... out");
        }
    }
}