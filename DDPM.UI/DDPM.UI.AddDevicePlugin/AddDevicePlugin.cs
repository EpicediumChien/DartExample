using CommunityToolkit.Mvvm.DependencyInjection;
using DDPM.SA.Common;
using DDPM.UI.Common;
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
        }

        private void GetRFDongleAsync()
        {
            _log.Info($"[AddDevicePlugin] GetRFDongleAsync is invoked ... in");
            //Task<DeviceHelper> tsk = DdpmCommonHelper.DeviceManagerSA.GetDevices(true);
            //_viewModel!.WacomVersion = tsk.Result.IsdDriverVersion;
            if (DdpmCommonHelper.DeviceManagerSA == null)
            {
                DdpmCommonHelper.WriteUILog($"Error: DeviceManagerSA is null");
            }
            else
            {
                _viewModel!.WacomVersion = DdpmCommonHelper.DeviceManagerSA.GetIsdDriverVersion().Result;
                _deviceHelper = DdpmCommonHelper.DeviceManagerSA.GetRFDongleDevices().Result;

                _viewModel?.PrepareDongleInfo(_deviceHelper.dongleInfo);
            }
            _log.Info($"[AddDevicePlugin] GetRFDongleAsync is invoked ... out");
        }

        /// <summary>
        /// Initialize or register services
        /// </summary>
        /// <remarks>Below code will be removed when <see cref="IConsole"/> provides the bootstrapper support</remarks>
        private void ConfigureServices()
        {
            _log.Info($"[AddDevicePlugin] ConfigureServices ... in");
            if (_isConfigured)
                return;

            // Marked all the instances as singleton
            // Pass the existing _console and _log instance so that Ioc doesn't new'up them
            PluginIoc.ConfigureServices(new ServiceCollection()
                .AddSingleton(_showPluginManager)
                .AddSingleton(_console)
                .AddSingleton(_log)
                .AddSingleton<IAddDeviceViewModel, AddDeviceViewModel>()
                .BuildServiceProvider());

            _viewModel = (AddDeviceViewModel?)PluginIoc.GetService<IAddDeviceViewModel>();
            _isConfigured = true;
            _log.Info($"[AddDevicePlugin] ConfigureServices ... out");
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
            if (DdpmCommonHelper.DeviceManagerSA != null)
            {
                DdpmCommonHelper.DeviceManagerSA.DeviceChanged -= DeviceChanged;
            }
            _viewModel!.StopPairing();
            //Mouse.OverrideCursor = Cursors.Wait;
        }

        /// <inheritdoc/>
        public void OnShown(string pluginParameter)
        {
            DdpmCommonHelper.WriteUILog($"AddDevice pugin OnShown Begin timestamp: {DateTime.Now:hh:mm:ss.ffffff}");
            ConfigureServices();
            //GetPeripheralsAsync();
            GetRFDongleAsync();
            if (DdpmCommonHelper.DeviceManagerSA != null)
            {
                DdpmCommonHelper.DeviceManagerSA.DeviceChanged += DeviceChanged;
            }
            Mouse.OverrideCursor = null;
            DdpmCommonHelper.WriteUILog($"AddDevice pugin OnShown End timestamp: {DateTime.Now:hh:mm:ss.ffffff}");
        }

        #endregion Interface IConsolePluginSupportsActivations

        //private void GetPeripheralsAsync()
        //{
        //    _log.Debug($"GetPeripherals is invoked");
        //    Task<DeviceHelper> task = DdpmCommonHelper.DeviceManagerSA.GetDevices(true);

        //    _viewModel?.CheckPandora(task.Result.deviceInfo);
        //}

        private void DeviceChanged(object? sender, DeviceChangedEventArgs e)
        {
            if (e.type == DeviceChangedType.Peripherals_PlugIn)
            {
                if (e.changedProperty == "PhysicalDeviceAdded")
                {
                    GetRFDongleAsync();
                    return;
                }
                //if (_viewModel!.CurrentDongle != null && e.device_peripherals != null && e.device_peripherals.PhyscialDeviceID == _viewModel!.CurrentDongle.ID)
                //{
                //    _viewModel.NewDevice = e.device_peripherals;
                //    //if(e.device_peripherals.PhysicalDeviceType == DeviceType.PhysicalAudioDongle)
                //    _viewModel.GotoNewDevice();
                //}
                if (e.device_peripherals!.PhysicalDeviceType == DeviceType.PhysicalBluetooth || e.device_peripherals.PhysicalDeviceType == DeviceType.PhysicalBluetoothAudio || e.device_peripherals.PhysicalDeviceType == DeviceType.PhysicalPen)
                {
                    _viewModel.NewDevice = e.device_peripherals;
                    _viewModel.GotoNewDevice();
                }
            }
            if (e.changedProperty == "PhysicalDeviceRemoved")
            {
                GetRFDongleAsync();
                return;
            }
            //if (e.device_peripherals?.IsPhysicalDeviceDongle ?? false)
            //{
            //    if (e.type == DeviceChangedType.Peripherals_SettingsChange)
            //    {
            //        //_viewModel?.HandleNotification(e.type, e.device_peripherals, e.changedProperty);
            //    }
            //    else
            //    {
            //        GetRFDongleAsync();
            //    }
            //}
        }
    }
}