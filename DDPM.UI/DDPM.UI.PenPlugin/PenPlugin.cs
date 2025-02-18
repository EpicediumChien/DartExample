using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using DDPM.SA.Common;
using DDPM.UI.Common;
using DDPM.UI.Interfaces;
using DDPM.UI.Plugin.ViewModels;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.Common.Annotations;
using Dell.Client.Framework.Common.PluginConditions;
using Dell.Client.Framework.UX.WPF;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using NGA.ThickClient.Interfaces;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Input;

namespace DDPM.UI.Plugin.PenPlugin
{
    /// <summary>
    /// Interaction logic for PenPlugin
    /// </summary>
    [Plugin(PluginId, PluginName, Version = PluginVersion, Category = Category.Utility)]
    [Descriptor(Description = Description)]
    [Publisher(Name = "DDPM PenPlugin", Support = "Wistron DDPM Team")]
    [ExcludeFromCodeCoverage]
    public class Penplugin : IConsoleTakeoverPagePlugin, IConsolePluginSupportsActivations, IThickClientPlugin
    {
        private const string PluginId = UI.Common.Constants.PenPluginId;
        private const string PluginName = "Pen plugin";
        private const string PluginVersion = "1.0";
        private const string Description = "Display Pen page";

        internal static readonly Ioc PluginIoc = new();

        private readonly ILog _log;
        private readonly IConsole _console;
        private readonly IPluginManager _pluginManager;
        private readonly string? _applicationName;
        private PenViewModel? _viewModel;

        private bool _isConfigured;
        private CancellationTokenSource StartupCancellationTokenSource { get; } = new();
        private CancellationToken CancellationToken { get; }
        private readonly SemaphoreSlim _lock = new(1, 1);
        private DeviceHelper _deviceHelper = new();
        private List<DeviceInfo> _deviceInfos = new();

        /// <summary>
        /// Default constructor
        /// </summary>
        public Penplugin(IPluginManager pluginManager, IConsole console, IGearMenu gearMenu)
        {
            _pluginManager = pluginManager;
            _console = console;
            _log = console.CreateLog("Pen");
            _log.Info($"{nameof(LaunchView)} - Constructed");
        }

        private void DeviceManager_DeviceChanged(object? sender, DeviceChangedEventArgs e)
        {
            try
            {

                if (e.device_peripherals != null && e.device_peripherals.LogicalDeviceType != null)
                {
                    if (e.device_peripherals.LogicalDeviceType.Contains("Pen"))
                    {
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
                DdpmCommonHelper.WriteUILog("DDPM.UI.PenPlugin\\PenPlugin.cs  DeviceManager_DeviceChanged() ex:" + ex.Message);
            }
        }

        private void GetPeripheralsAsync()
        {
            _log.Debug($"GetPeripherals is invoked");
            Task<DeviceHelper> task = DdpmCommonHelper.DeviceManagerSA!.GetDevices(true);
            _deviceHelper = task.Result;

            //Task<JArray> task2 = DdpmCommonHelper.DeviceManagerSA!.GetPenDeviceItemsEx();
            //var jArray = JArray.FromObject(task2.Result);
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
                .AddSingleton<IPeripheralViewModel, PenViewModel>()
                .BuildServiceProvider());

            _viewModel = (PenViewModel?)PluginIoc.GetService<IPeripheralViewModel>();
            _isConfigured = true;
        }

        public string HeaderText => "Dell Pen";
        public Type PageType => typeof(LaunchView);

        #region Interface IConsolePluginSupportsActivations

        /// <inheritdoc/>
        public void OnActivated()
        {
            DdpmCommonHelper.DeviceManagerSA!.DeviceChanged += DeviceManager_DeviceChanged;
            Mouse.OverrideCursor = null;
        }

        /// <inheritdoc/>
        public void OnDeactivated()
        {
            DdpmCommonHelper.DeviceManagerSA!.DeviceChanged -= DeviceManager_DeviceChanged;
            Mouse.OverrideCursor = Cursors.Wait;
        }

        /// <inheritdoc/>
        public void OnShown(string pluginParameter)
        {
            DdpmCommonHelper.WriteUILog($"Pen pugin OnShown Begin timestamp: {DateTime.Now:hh:mm:ss.ffffff}");
            ConfigureServices();
            GetPeripheralsAsync();
            if (_viewModel != null && !_viewModel.SetCurrentDevice(pluginParameter))
            { }
            DdpmCommonHelper.WriteUILog($"Pen pugin OnShown End timestamp: {DateTime.Now:hh:mm:ss.ffffff}");
        }

        #endregion Interface IConsolePluginSupportsActivations

        ~Penplugin()
        {
            DdpmCommonHelper.DeviceManagerSA!.DeviceChanged -= DeviceManager_DeviceChanged;
        }
    }
}