using CommunityToolkit.Mvvm.DependencyInjection;
using DDPM.SA.Common;
using DDPM.SA.Common.Settings;
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
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows.Threading;
using Windows.Graphics.Imaging;

namespace DDPM.UI.Plugin.WebCameraPlugin
{
    /// <summary>
    /// Interaction logic for WebCameraPlugin
    /// </summary>
    [Plugin(PluginId, PluginName, Version = PluginVersion, Category = Category.Utility)]
    [Descriptor(Description = Description)]
    [Publisher(Name = "DDPM WebCameraPlugin", Support = "Wistron DDPM Team")]
    [ExcludeFromCodeCoverage]
    public class WebCameraplugin : IConsoleTakeoverPagePlugin, IConsolePluginSupportsActivations, IThickClientPlugin
    {
        private const string PluginId = UI.Common.Constants.WebCameraPluginId;
        private const string PluginName = "WebCamera plugin";
        private const string PluginVersion = "1.0";
        private const string Description = "Display WebCamera page";

        internal static readonly Ioc PluginIoc = new();

        private readonly ILog _log;
        private readonly IConsole _console;
        private readonly IPluginManager _pluginManager;
        private WebCameraViewModel? _viewModel;

        private bool _isConfigured;
        private readonly SemaphoreSlim _lock = new(1, 1);
        private DeviceHelper _deviceHelper = new();

        private DispatcherTimer timer = new();

        /// <summary>
        /// Default constructor
        /// </summary>
        public WebCameraplugin(IPluginManager pluginManager, IConsole console, IGearMenu gearMenu)
        {
            _pluginManager = pluginManager;
            _console = console;
            _log = console.CreateLog("WebCamera");
            _log.Info($"{nameof(LaunchView)} - Constructed");

            timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(5)
            };
            timer.Tick += Timer_Tick;

            if (_console != null)
            {
                _console.RegisterForEvent(ConsoleEventNames.MainWindow_Activate, MainWindowActivate);
                _console.RegisterForEvent(ConsoleEventNames.MainWindow_DeActivate, MainWindowDeActivate);
            }
        }


        private void DeviceManager_DeviceChanged(object? sender, DeviceChangedEventArgs e)
        {
            if (e.device_peripherals != null && e.device_peripherals.LogicalDeviceType.Contains("Webcam"))
            {
                if (e.type == DeviceChangedType.Peripherals_UnPlug)
                {
                    if (e.device_peripherals.ID == _viewModel!.CurrentDeviceID)
                    {
                        if (_viewModel.IsMicEnumerationOnEnabled)
                        {
                            _viewModel.AlertType = WebcamAlert.Alert4;
                            _viewModel.AlertVisibility = System.Windows.Visibility.Visible;
                            timer.Start();
                            //_viewModel!.OnGoBackClicked();
                        }
                    }
                    return;
                }
                if (e.type == DeviceChangedType.Peripherals_PlugIn)
                {
                    if (e.device_peripherals.Name == _viewModel!.CurrentDeviceInfo!.Name)
                    {
                        timer.Stop();
                        //Mouse.OverrideCursor = null;
                        //_viewModel.CurrentCursor = Cursors.Arrow;
                        //_viewModel.IsMicEnumerationOnEnabled = true;
                        _viewModel.AlertVisibility = Visibility.Collapsed;
                        _console.ShowPluginById(PluginId);
                        GetPeripheralsAsync();
                        _viewModel!.SetCurrentDevice(e.device_peripherals.ID.ToString());
                    }
                    return;
                }
                _viewModel?.HandleNotification(e.type, e.device_peripherals, e.changedProperty);
            }
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            //if (!_viewModel!.DeviceInfos.ContainsKey(CurrentDeviceID))
            _viewModel!.OnGoBackClicked();
            timer.Stop();
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

            PluginIoc.ConfigureServices(new ServiceCollection()
                .AddSingleton(_console)
                .AddSingleton(_log)
                .AddSingleton<IPeripheralViewModel, WebCameraViewModel>()
                .BuildServiceProvider());

            _viewModel = (WebCameraViewModel?)PluginIoc.GetService<IPeripheralViewModel>();
            _isConfigured = true;
        }

        public string HeaderText => "Dell WebCamera";
        public Type PageType => typeof(LaunchView);

        #region Interface IConsolePluginSupportsActivations

        /// <inheritdoc/>
        public void OnActivated()
        {
            DdpmCommonHelper.DeviceManagerSA!.DeviceChanged += DeviceManager_DeviceChanged;
            DdpmCommonHelper.DeviceManagerSA!.UIUpdateNotify += WebCameraplugin_UIUpdateNotify;
            Mouse.OverrideCursor = null;
        }

        private void WebCameraplugin_UIUpdateNotify(object? sender, UpdateUINotify e)
        {
            //Open this to get the message format of Webcam event
            //System.Windows.MessageBox.Show(e.UI_Field_Name);

            //cmd format sample
            //5;Device:Webcam;EventType:Webcam_IsHDROnChanged;DeviceId:28d64fee-3544-45c7-a1b0-10db20a4cf8e;NewValue:True
            Dictionary<string, string> event_param = deal_param(e.UI_Field_Name);
            try
            {
                if (!event_param.TryGetValue("Device", out var device)) return;
                if (device == "Webcam")
                {
                    if (!event_param.TryGetValue("EventType", out var eventtype)) return;
                    switch (eventtype)
                    {
                        case "Webcam_IsHDROnChanged":
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                _viewModel!.IsHDROn = DdpmCommonHelper.DeviceManagerSA!.GetIsHDROn(_viewModel!.CurrentDeviceID.ToString()).Result;
                            });
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                _log.Debug(ex , "WebCameraplugin_UIUpdateNotify");
            }
        }

        private Dictionary<string, string> deal_param(string param)
        {
            Dictionary<string, string> tmp = new Dictionary<string, string>();

            try
            {
                List<string> list = param.Split(new char[] { ';' }).ToList();
                foreach (string s in list)
                {
                    List<string> item = s.Split(new char[] { ':' }).ToList();
                    if (item.Count == 2)
                    {
                        tmp.Add(item[0], item[1]);
                    }
                }
            }
            catch
            {

            }
            return tmp;
        }


        /// <inheritdoc/>
        public void OnDeactivated()
        {
            DdpmCommonHelper.DeviceManagerSA!.DeviceChanged -= DeviceManager_DeviceChanged;
            DdpmCommonHelper.DeviceManagerSA!.UIUpdateNotify -= WebCameraplugin_UIUpdateNotify;
            Mouse.OverrideCursor = Cursors.Wait;
        }

        /// <inheritdoc/>
        public void OnShown(string parameter)
        {
            ConfigureServices();
            GetPeripheralsAsync();
            if (_viewModel != null)
            {
                _viewModel.SetCurrentDevice(parameter);
            }
        }

        #endregion Interface IConsolePluginSupportsActivations

        ~WebCameraplugin()
        {
            DdpmCommonHelper.DeviceManagerSA!.DeviceChanged -= DeviceManager_DeviceChanged;
        }

        private void MainWindowActivate(object sender, EventManagerArgs e)
        {
            this._log?.Write(LogMsgType.Debug, "WebCamera plugin receive MainWindow Activate event");
            if (_viewModel == null)
                return;
            _viewModel.running_state = true;
            _viewModel.mre.Set();
        }

        private void MainWindowDeActivate(object sender, EventManagerArgs e)
        {
            this._log?.Write(LogMsgType.Debug, "WebCamera plugin receive MainWindow DeActivate event");

            if (_viewModel == null)
                return;
            _viewModel.running_state = false;
            _viewModel.mre.Set();
        }
    }
}