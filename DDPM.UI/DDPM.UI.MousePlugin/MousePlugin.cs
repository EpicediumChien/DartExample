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
        private MouseViewModel? _viewModel;

        private bool _isConfigured;
        private readonly CancellationTokenSource StartupCancellationTokenSource = new();
        private readonly CancellationToken CancellationToken;
        private readonly SemaphoreSlim _lock = new(1, 1);
        private DeviceHelper _deviceHelper = new();
        private bool IsEventRegistered = false;

        /// <summary>
        /// Default constructor
        /// </summary>
        public Mouseplugin(IPluginManager pluginManager, IConsole console)
        {
            _pluginManager = pluginManager;
            _console = console;
            _log = console.CreateLog("Mouse");
            _log.Info($"{nameof(LaunchView)} - Constructed");
        }

        private void DeviceManager_DeviceChanged(object? sender, DeviceChangedEventArgs e)
        {
            try
            {
                if (e.device_peripherals != null && e.device_peripherals.LogicalDeviceType != null)
                {
                    if (e.device_peripherals.LogicalDeviceType.Contains("Mouse"))
                    {
                        if (e.type == DeviceChangedType.Peripherals_UnPlug)
                        {
                            if (e.device_peripherals.ID == _viewModel!.CurrentDeviceID && _viewModel.CurrentInstanceID == 0)
                            {
                                _viewModel.OnGoBackClicked();
                                return;
                            }
                            if (_viewModel.DeviceInfos.ContainsKey(e.device_peripherals.ID))
                                _viewModel.DeviceInfos.Remove(e.device_peripherals.ID);
                            //GetPeripheralsAsync();
                        }
                        if (e.type == DeviceChangedType.Peripherals_PlugIn &&
                            e.device_peripherals.ModelNumber == _viewModel?.CurrentDeviceInfo?.ModelNumber &&
                            !_viewModel.DeviceInfos.ContainsKey(e.device_peripherals.ID))
                        {
                            _viewModel.DeviceInfos.Add(e.device_peripherals.ID, e.device_peripherals);
                        }
                        _viewModel?.HandleNotification(e.type, e.device_peripherals, e.changedProperty);

                    }
                    else
                    {
                        if (e.type == DeviceChangedType.Peripherals_UnPlug || e.type == DeviceChangedType.Peripherals_PlugIn)
                            _viewModel!.OnGoBackClicked();
                    }
                }
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog("DDPM.UI.MousePlugin\\MousePlugin.cs  DeviceManager_DeviceChanged ex:" + ex.Message);
            }
        }

        private void GetPeripheralsAsync()
        {
            _log.Debug($"GetPeripherals is invoked");
            Task<DeviceHelper> task = DdpmCommonHelper.DeviceManagerSA!.GetDevices(true);
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
            if (!IsEventRegistered)
            {
                DdpmCommonHelper.DeviceManagerSA!.DeviceChanged += DeviceManager_DeviceChanged;
                IsEventRegistered = true;
            }
            Mouse.OverrideCursor = null;
        }

        /// <inheritdoc/>
        public void OnDeactivated()
        {
            if (IsEventRegistered)
            {
                DdpmCommonHelper.DeviceManagerSA!.DeviceChanged -= DeviceManager_DeviceChanged;
                IsEventRegistered = false;
            }
            Mouse.OverrideCursor = Cursors.Wait;
        }

        /// <inheritdoc/>
        public void OnShown(string pluginParameter)
        {
            DdpmCommonHelper.WriteUILog($"Mouse pugin OnShown Begin timestamp: {DateTime.Now:hh:mm:ss.ffffff}");
            if (!IsEventRegistered)
            {
                DdpmCommonHelper.DeviceManagerSA!.DeviceChanged += DeviceManager_DeviceChanged;
                IsEventRegistered = true;
            }
            ConfigureServices();
            GetPeripheralsAsync();
            if (_viewModel != null && !_viewModel.SetCurrentDevice(pluginParameter))
            { }
            Mouse.OverrideCursor = null;
            DdpmCommonHelper.WriteUILog($"Mouse pugin OnShown End timestamp: {DateTime.Now:hh:mm:ss.ffffff}");
        }

        #endregion Interface IConsolePluginSupportsActivations

        ~Mouseplugin()
        {
            DdpmCommonHelper.DeviceManagerSA!.DeviceChanged -= DeviceManager_DeviceChanged;
        }
    }
}