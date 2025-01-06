using CommunityToolkit.Mvvm.DependencyInjection;
using DDPM.SA.Common;
using DDPM.UI.Common;
using DDPM.UI.Common.Models;
using DDPM.UI.Interfaces;
using DDPM.UI.Plugin.ViewModels;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.Common.Annotations;
using Dell.Client.Framework.UX.WPF;
using Microsoft.Extensions.DependencyInjection;
using NGA.ThickClient.Interfaces;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Input;
using DDPMConstants = DDPM.UI.Common.Constants;

namespace DDPM.UI.Plugin.SettingsPlugin
{
    /// <summary>
    /// Interaction logic for AboutView Plugin.xaml
    /// </summary>
    [Plugin(PluginId, PluginName, Version = PluginVersion, Category = Category.Utility)]
    [Descriptor(Description = Description)]
    [Publisher(Name = "DDPM SettingsPlugin", Support = "Wistron DDPM Team")]
    [PluginRequires(Id = DDPM.SA.Common.IDs.Device_Manager_Plugin_ID, AllowDynamicResolving = true)]
    [ExcludeFromCodeCoverage]
    public class SettingsPlugin : IConsoleTakeoverPagePlugin, IConsolePluginSupportsActivations, IThickClientPlugin
    {
        private const string PluginId = DDPMConstants.SettingsPluginId;
        private const string PluginName = "DDPM Settings plugin";
        private const string PluginVersion = "1.0";
        private const string Description = "Display DDPM.Settingspage";

        //internal static readonly Ioc PluginIoc = new();
        public static readonly Ioc PluginIoc = new();

        private readonly ILog _log;
        private readonly IConsole _console;
        private readonly IShowPluginManager _showPluginManager;
        private SettingsPageViewModel? _viewModel;

        private bool _isConfigured;

        /// <summary>
        /// This property is required by the IConsolePagePlugin. It specifies the text to display when the page is shown.
        /// </summary>
        public string HeaderText => "Settings Page";

        /// <summary>
        /// Page Type
        /// </summary>
        public Type PageType => typeof(SettingsPage);

        public bool HasTiles => throw new NotImplementedException();

        //Device manager related object
        private static IDeviceManagerSA? _deviceManager;
        private readonly IPluginManager _pluginManager;
        private IFrameworkPluginConditionNotification? _IDeviceManagerPluginCondition;
        private CancellationTokenSource StartupCancellationTokenSource { get; } = new();
        private CancellationToken CancellationToken { get; }
        private readonly SemaphoreSlim _lock = new(1, 1);

        /// <summary>
        /// Default constructor
        /// </summary>
        public SettingsPlugin(IConsole console, IGearMenu gearMenu)
        {
            _console = console;
            _log = console.CreateLog("SettingsPLG");
            _log.Info($"{nameof(SettingsPlugin)} - Constructed");
            //var ddpmGearItem = new GearMenuItem("Settings Plugin", new RelayCommand(ShowSettingPage));
            //gearMenu.AddGearMenuItem(ddpmGearItem, 3);
        }

        public void OnActivated()
        {
            Mouse.OverrideCursor = null;
            //IDeviceInfo deviceInfo =
            //(IDeviceInfo)DdpmHomePlugin.DdpmHomePlugin.PluginIoc.GetServices<IDeviceInfo>();//
        }

        public void OnDeactivated()
        {
            Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
        }
        public void OnShown(string pluginParameter)
        {
            ConfigureServices();
            PrepareHomeDevices();
            if (_viewModel != null)
            {
                if (string.IsNullOrEmpty(pluginParameter))
                {
                    pluginParameter = "0";
                }
                if (int.TryParse(pluginParameter, out int page))
                {
                    _viewModel.SetSelected(page);
                }
            }
            Mouse.OverrideCursor = null;
        }
        private void ConfigureServices()
        {
            if (_isConfigured)
                return;

            PluginIoc.ConfigureServices(new ServiceCollection()
                .AddSingleton(_console)
                .AddSingleton(_log)
                .AddSingleton<ISettingsPageViewModel, SettingsPageViewModel>()
                .BuildServiceProvider());

            _viewModel = (SettingsPageViewModel?)PluginIoc.GetService<ISettingsPageViewModel>();
            if (_viewModel != null)
            {
                _viewModel.Log = PluginIoc.GetService<ILog>();
            }
            _isConfigured = true;
        }
        /// <summary>
        /// Get HomeDevices and SelectedHomeDevice from DdpmHomePlugin, and add them to ISettingsPageViewModel
        /// </summary>
        private void PrepareHomeDevices()
        {
            //Get ISettingsPageViewModel, it will be created after ConfigureServices() executed
            ISettingsPageViewModel? vmDisplay = PluginIoc.GetService<ISettingsPageViewModel>();

            if (vmDisplay != null)
            {
                //Get the selected HomeDevice
                vmDisplay.SelectedHomeDevice = DdpmHomePlugin.DdpmHomePlugin.GetSelectedHomeDevice();

                foreach (HomeDevice obj in DdpmHomePlugin.DdpmHomePlugin.GetHomeDevices())
                {
                    if (obj.DeviceCategory == eDeviceCategory.Display)
                        vmDisplay.HomeDevices.Add(obj);
                }
            }
        }
        /// <summary>
        /// Method to show <see cref="SettingsPage"/>
        /// </summary>
        private void ShowSettingPage()
        {
            _log.Info($"{nameof(SettingsPage)} - shown");
            _console.ShowPluginById(PluginId);
        }
    }
}