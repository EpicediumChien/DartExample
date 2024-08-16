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
using System.Windows;
using System.Windows.Input;

namespace DDPM.UI.Plugin.MousePlugin
{
    /// <summary>
    /// Interaction logic for MousePlugin
    /// </summary>
    [Plugin(PluginId, PluginName, Version = PluginVersion, Category = Category.Utility)]
    [Descriptor(Description = Description)]
    [Publisher(Name = "DDPM MousePlugin", Support = "Wistron DDPM Team")]
    [ExcludeFromCodeCoverage]
    public class Mouseplugin : IConsoleTakeoverPagePlugin, IConsolePluginSupportsActivations, IThickClientPlugin
    {
        private const string PluginId = UI.Common.Constants.MousePluginId;
        private const string PluginName = "Mouse plugin";
        private const string PluginVersion = "1.0";
        private const string Description = "Display Mouse page";

        internal static readonly Ioc PluginIoc = new();

        private readonly ILog _log;
        private readonly IConsole _console;
        private readonly IPluginManager _pluginManager;
        private readonly string? _applicationName;
        private MouseViewModel? _viewModel;

        private bool _isConfigured;
        private IDeviceManagerSA? _deviceManagerPlugin;
        private IFrameworkPluginConditionNotification? _deviceManagerPluginCondition;
        private readonly CancellationTokenSource StartupCancellationTokenSource = new();
        private readonly CancellationToken CancellationToken;
        private readonly SemaphoreSlim _lock = new(1, 1);
        private DeviceHelper _deviceHelper = new();

        /// <summary>
        /// Default constructor
        /// </summary>
        public Mouseplugin(IPluginManager pluginManager, IConsole console)
        {
            _pluginManager = pluginManager;
            _console = console;
            _log = console.CreateLog("Mouse");
            _log.Info($"{nameof(LaunchView)} - Constructed");

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

        private void DeviceManager_DeviceChanged(object? sender, DeviceChangedEventArgs e)
        {
            if (e.device_peripherals != null && e.device_peripherals.LogicalDeviceType.Contains("Mouse"))
            {
                if (e.type == DeviceChangedType.Peripherals_UnPlug)
                    GetPeripheralsAsync();
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
            if (!SpinWait.SpinUntil(() =>
            _deviceManagerPluginCondition is not null, TimeSpan.FromMinutes(2)))
            {
                Console.WriteLine("Could not establish communication with DDPM!!");
                return;
            }
            _log.Debug($"GetPeripherals is invoked");
            //_deviceHelper = await peripheralsPlugin.GetDevices();
            Task<DeviceHelper> task = _deviceManagerPlugin!.GetDevices();
            _deviceHelper = task.Result;

            _viewModel?.PrepareDeviceInfo(_deviceHelper.deviceInfo);
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
                .AddSingleton(_console)
                .AddSingleton(_log)
                .AddSingleton(_deviceManagerPlugin!)
                .AddSingleton<IPeripheralViewModel, MouseViewModel>()
                .BuildServiceProvider());

            _viewModel = (MouseViewModel?)PluginIoc?.GetService<IPeripheralViewModel>();
            if (_viewModel != null)
            {
                _viewModel.WordVisibility = CheckOfficeApp("Word.Application");
                _viewModel.ExcelVisibility = CheckOfficeApp("Excel.Application");
                _viewModel.PowerPointVisibility = CheckOfficeApp("PowerPoint.Application");
                _viewModel.OutlookVisibility = CheckOfficeApp("Outlook.Application");
            }
            _isConfigured = true;
        }

        private static Visibility CheckOfficeApp(string progId)
        {
            Type? type = Type.GetTypeFromProgID(progId);
            if (type == null)
                return Visibility.Collapsed;
            else
                return Visibility.Visible;
        }

        public string HeaderText => "Dell Mouse";
        public Type PageType => typeof(LaunchView);

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
        public void OnShown(string parameter)
        {
            ConfigureServices();
            GetPeripheralsAsync();
            if (_viewModel != null && !_viewModel.SetCurrentDevice(parameter)) { }
            Mouse.OverrideCursor = null;
        }

        #endregion Interface IConsolePluginSupportsActivations

        ~Mouseplugin()
        {
            _deviceManagerPlugin!.DeviceChanged -= DeviceManager_DeviceChanged;
        }
    }
}