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
using Newtonsoft.Json.Linq;
using NGA.ThickClient.Interfaces;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Input;

namespace DDPM.UI.Plugin.KeyboardPlugin
{
    /// <summary>
    /// Interaction logic for KeyboardPlugin
    /// </summary>
    [Plugin(PluginId, PluginName, Version = PluginVersion, Category = Category.Utility)]
    [Descriptor(Description = Description)]
    [Publisher(Name = "DDPM KeyboardPlugin", Support = "Wistron DDPM Team")]
    [ExcludeFromCodeCoverage]
    public class Keyboardplugin : IConsoleTakeoverPagePlugin, IConsolePluginSupportsActivations, IThickClientPlugin
    {
        private const string PluginId = UI.Common.Constants.KeyboardPluginId;
        private const string PluginName = "Keyboard plugin";
        private const string PluginVersion = "1.0";
        private const string Description = "Display Keyboard page";

        internal static readonly Ioc PluginIoc = new();

        private readonly ILog _log;
        private readonly IConsole _console;
        private readonly IPluginManager _pluginManager;
        private readonly string? _applicationName;
        private KeyboardViewModel? _viewModel;

        private bool _isConfigured;
        private IFrameworkPluginConditionNotification? _deviceManagerPluginCondition;
        private readonly CancellationTokenSource StartupCancellationTokenSource = new();
        private readonly SemaphoreSlim _lock = new(1, 1);
        private DeviceHelper _deviceHelper = new();
        private bool IsEventRegistered = false;

        /// <summary>
        /// Default constructor
        /// </summary>
        public Keyboardplugin(IPluginManager pluginManager, IConsole console)
        {
            _pluginManager = pluginManager;
            _console = console;
            _log = console.CreateLog("Keyboard");
            _log.Info($"{nameof(LaunchView)} - Constructed");
            //_console.RegisterForEvent(ConsoleEventNames.MainWindow_MoveToNewScreen, (sender, args) =>
            //{
            //    if (args != null)
            //    {
            //        if (args.Tag != null)
            //        {
            //            string screenDeviceName = args.Tag as string;
            //            if (screenDeviceName != null)
            //            {
            //            }
            //        }
            //    }
            //});
        }

        private void DeviceManager_DeviceChanged(object? sender, DeviceChangedEventArgs e)
        {
            if (e.device_peripherals != null && e.device_peripherals.LogicalDeviceType != null)
            {
                if (e.device_peripherals.LogicalDeviceType.Contains("Keyboard"))
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
                    if (e.type == DeviceChangedType.Peripherals_PlugIn)
                    {
                        if (e.device_peripherals.ModelNumber == _viewModel?.CurrentDeviceInfo?.ModelNumber && !_viewModel.DeviceInfos.ContainsKey(e.device_peripherals.ID))
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

        private void GetPeripheralsAsync()
        {
            _log.Debug($"GetPeripherals is invoked");
            Task<DeviceHelper> task = DdpmCommonHelper.DeviceManagerSA!.GetDevices(true);
            _deviceHelper = task.Result;

            //Task<JArray> task2 = DdpmCommonHelper.DeviceManagerSA!.GetKeyboardDeviceItemsEx();
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
                .AddSingleton<IPeripheralViewModel, KeyboardViewModel>()
                .BuildServiceProvider());

            _viewModel = (KeyboardViewModel?)PluginIoc.GetService<IPeripheralViewModel>();
            _isConfigured = true;
        }

        public string HeaderText => "Dell Keyboard";
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
        public void OnShown(string parameter)
        {
            if (!IsEventRegistered)
            {
                DdpmCommonHelper.DeviceManagerSA!.DeviceChanged += DeviceManager_DeviceChanged;
                IsEventRegistered = true;
            }
            ConfigureServices();
            GetPeripheralsAsync();
            if (_viewModel != null && _viewModel.SetCurrentDevice(parameter))
            {
                if (_viewModel.CurrentDeviceInfo!.IsCollabsKeysSupported)
                {
                    _log.Debug($"GetCTKMessageHelper is invoked");
                    Task<CTKMessageHelper> task = DdpmCommonHelper.DeviceManagerSA!.GetCTKMessageHelper();
                    _viewModel.CTKMessageHelper = task.Result;
                    _log.Debug($"GetCTKMessageHelper is successful");
                }
            }
        }

        #endregion Interface IConsolePluginSupportsActivations

        ~Keyboardplugin()
        {
            DdpmCommonHelper.DeviceManagerSA!.DeviceChanged -= DeviceManager_DeviceChanged;
        }
    }
}