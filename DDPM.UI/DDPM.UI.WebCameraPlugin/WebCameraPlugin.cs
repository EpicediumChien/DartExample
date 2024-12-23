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
using System.Windows.Forms;


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
            if (e.device_peripherals != null && e.device_peripherals.LogicalDeviceType != null && e.device_peripherals.LogicalDeviceType.Contains("Webcam"))
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
                .AddSingleton(_pluginManager)
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

            //DdpmCommonHelper.WriteUILog($"Webcam plugin OnActivated start");
            //DdpmCommonHelper.DeviceManagerSA!.SetIsDDPMLaunchByQAMAsync(false);
        }

        private void WebCameraplugin_UIUpdateNotify(object? sender, UpdateUINotify e)
        {
            //Derek 1212
            if (e == null || e == EventArgs.Empty || e.UI_Field_Name == null
                || e.UI_Field_Name == string.Empty)
                return;

            //Open this to get the message format of Webcam event
            //System.Windows.MessageBox.Show(e.UI_Field_Name);
            Console.WriteLine("Get event : " + e.UI_Field_Name + "#" + DateTime.Now.ToString("yyyy-MM-dd h:mm:tt") + "\r\n");

            //cmd format sample
            //5;Device:Webcam;EventType:Webcam_IsHDROnChanged;DeviceId:28d64fee-3544-45c7-a1b0-10db20a4cf8e;NewValue:True
            Dictionary<string, string> event_param = deal_param(e.UI_Field_Name);
            try
            {
                if (!event_param.TryGetValue("Device", out var device))
                {
                    _log.Debug("Device cannot be found in event_param");
                    return;
                }
                if (device == "Webcam")
                {
                    if (!event_param.TryGetValue("EventType", out var eventtype))
                    {
                        _log.Debug("EventType cannot be found in event_param");
                        return;
                    }


                    switch (eventtype)
                    {
                        #region for cli setting
                        case "Webcam_ZoomMeetingTypeChanged":
                        {
                            //No corresponding UI
                        }
                        break;
                        case "Webcam_SharpnessChanged":
                        {
                            if (!event_param.TryGetValue("NewValue", out var NewValue))
                            {
                                _log.Debug("NewValue cannot be found in event_param");
                                return;
                            }
                            System.Windows.Application.Current.Dispatcher.Invoke(() =>
                            {
                                if (int.TryParse(NewValue, out var _t))
                                    _viewModel!.Sharpness = _t;
                                else
                                {
                                    _log.Debug("Webcam_SharpnessChanged NewValue not int");
                                    return;
                                }
                            });
                        }
                        break;

                        case "Webcam_Esi_WALLockCountdownChanged":
                        {
                            //??
                        }
                        break;
                        case "Webcam_IsZoomScreenShareActiveChanged":
                        {
                            //No corresponding UI
                        }
                        break;
                        case "Webcam_IsZoomMeetingActiveChanged":
                        {
                            //No corresponding UI
                        }
                        break;
                        case "Webcam_SerialNumberChanged":
                        {
                            //??
                        }
                        break;
                        case "Webcam_IsHDROnChanged":
                        {
                            if (!event_param.TryGetValue("NewValue", out var NewValue))
                            {
                                _log.Debug("NewValue cannot be found in event_param");
                                return;
                            }
                            System.Windows.Application.Current.Dispatcher.Invoke(() =>
                            {
                                _viewModel!.IsSettingProfile = true;
                                if (NewValue.ToLower() == "true")
                                    _viewModel.IsHDROn = true;
                                else
                                    _viewModel.IsHDROn = false;
                                _viewModel!.IsSettingProfile = false;
                            });
                            //HDR SIWTCH時候,需要重置CAMERA,中間需要一段初始化時間約1秒
                            _viewModel!.mre.Set();
                            Thread.Sleep(1000);
                            _viewModel!.mre.Set();
                            _viewModel!.hdr_change = false;
                        }
                        break;
                        case "Webcam_FieldOfViewChanged":
                        {
                            if (!event_param.TryGetValue("NewValue", out var NewValue))
                            {
                                _log.Debug("NewValue cannot be found in event_param");
                                return;
                            }
                            System.Windows.Application.Current.Dispatcher.Invoke(() =>
                            {
                                if (int.TryParse(NewValue, out var _t))
                                {
                                    _viewModel!.FieldOfView = _t;

                                    //Derek 1211 to sync data with QAM
                                    if (90 == _t)
                                        _viewModel!.SetFOV_Selected(2);
                                    else if (78 == _t)
                                        _viewModel!.SetFOV_Selected(1);
                                    else if (65 == _t)
                                        _viewModel!.SetFOV_Selected(0);
                                }
                                else
                                {
                                    _log.Debug("Webcam_FieldOfViewChanged NewValue not int");
                                    return;
                                }
                            });

                        }
                        break;
                        case "Webcam_AutoFramingFrameSizeChanged":
                        {
                            //if (!event_param.TryGetValue("NewValue", out var NewValue))
                            //{
                            //    _log.Debug("NewValue cannot be found in event_param");
                            //    return;
                            //}
                            //System.Windows.Application.Current.Dispatcher.Invoke(() =>
                            //{
                            //    if (int.TryParse(NewValue, out var _t))
                            //        _viewModel!.AutoFramingFrameSize = _t;
                            //    else
                            //    {
                            //        _log.Debug("Webcam_AutoFramingFrameSizeChanged NewValue not int");
                            //        return;
                            //    }
                            //});
                        }
                        break;
                        case "Webcam_AutoFramingSensitivityChanged":
                        {
                            //if (!event_param.TryGetValue("NewValue", out var NewValue))
                            //{
                            //    _log.Debug("NewValue cannot be found in event_param");
                            //    return;
                            //}
                            //System.Windows.Application.Current.Dispatcher.Invoke(() =>
                            //{
                            //    if (int.TryParse(NewValue, out var _t))
                            //        _viewModel!.AutoFramingFrameSize = _t;
                            //    else
                            //    {
                            //        _log.Debug("Webcam_AutoFramingSensitivityChanged NewValue not int");
                            //        return;
                            //    }
                            //});
                        }
                        break;
                        case "Webcam_IsAutoFramingOnChanged":
                        {
                            if (!event_param.TryGetValue("NewValue", out var NewValue))
                            {
                                _log.Debug("NewValue cannot be found in event_param");
                                return;
                            }
                            System.Windows.Application.Current.Dispatcher.Invoke(() =>
                            {
                                if (NewValue.ToLower() == "true")
                                    _viewModel!.IsAutoFramingOn = true;
                                else
                                    _viewModel!.IsAutoFramingOn = false;
                            });
                        }
                        break;
                        case "Webcam_IsAutoFramingTransitionOnChanged":
                        {
                            if (!event_param.TryGetValue("NewValue", out var NewValue))
                            {
                                _log.Debug("NewValue cannot be found in event_param");
                                return;
                            }
                            System.Windows.Application.Current.Dispatcher.Invoke(() =>
                            {
                                if (NewValue.ToLower() == "true")
                                    _viewModel!.IsAutoFramingTransitionOn = true;
                                else
                                    _viewModel!.IsAutoFramingTransitionOn = false;
                            });
                        }
                        break;
                        case "Webcam_AutoWhiteBalanceChanged":
                        {
                            if (!event_param.TryGetValue("NewValue", out var NewValue))
                            {
                                _log.Debug("NewValue cannot be found in event_param");
                                return;
                            }
                            System.Windows.Application.Current.Dispatcher.Invoke(() =>
                            {
                                if (int.TryParse(NewValue, out var _t))
                                    _viewModel!.AutoWhiteBalance = _t;
                                else
                                {
                                    _log.Debug("Webcam_AutoWhiteBalanceChanged NewValue not int");
                                    return;
                                }
                            });
                        }
                        break;
                        case "Webcam_IsAutoWhiteBalanceOnChanged":
                        {
                            if (!event_param.TryGetValue("NewValue", out var NewValue))
                            {
                                _log.Debug("NewValue cannot be found in event_param");
                                return;
                            }
                            System.Windows.Application.Current.Dispatcher.Invoke(() =>
                            {
                                if (NewValue.ToLower() == "true")
                                    _viewModel!.IsAutoWhiteBalanceOn = true;
                                else
                                    _viewModel!.IsAutoWhiteBalanceOn = false;
                            });
                        }
                        break;
                        case "Webcam_SaturationChanged":
                        {
                            if (!event_param.TryGetValue("NewValue", out var NewValue))
                            {
                                _log.Debug("NewValue cannot be found in event_param");
                                return;
                            }
                            System.Windows.Application.Current.Dispatcher.Invoke(() =>
                            {
                                if (int.TryParse(NewValue, out var _t))
                                    _viewModel!.Saturation = _t;
                                else
                                {
                                    _log.Debug("Webcam_SaturationChanged NewValue not int");
                                    return;
                                }
                            });
                        }
                        break;
                        case "Webcam_AntiFlickerChanged":
                        {
                            //1:50 2:60
                            if (!event_param.TryGetValue("NewValue", out var NewValue))
                            {
                                _log.Debug("NewValue cannot be found in event_param");
                                return;
                            }
                            System.Windows.Application.Current.Dispatcher.Invoke(() =>
                            {
                                if (int.TryParse(NewValue, out var _t))
                                    _viewModel!.AntiFlicker = _t;
                                else
                                {
                                    _log.Debug("Webcam_AntiFlickerChanged NewValue not int");
                                    return;
                                }
                            });
                        }
                        break;
                        case "Webcam_ContrastChanged":
                        {
                            if (!event_param.TryGetValue("NewValue", out var NewValue))
                            {
                                _log.Debug("NewValue cannot be found in event_param");
                                return;
                            }
                            System.Windows.Application.Current.Dispatcher.Invoke(() =>
                            {
                                if (int.TryParse(NewValue, out var _t))
                                    _viewModel!.Contrast = _t;
                                else
                                {
                                    _log.Debug("Webcam_ContrastChanged NewValue not int");
                                    return;
                                }
                            });
                        }
                        break;
                        case "Webcam_BrightnessChanged":
                        {
                            if (!event_param.TryGetValue("NewValue", out var NewValue))
                            {
                                _log.Debug("NewValue cannot be found in event_param");
                                return;
                            }
                            System.Windows.Application.Current.Dispatcher.Invoke(() =>
                            {
                                if (int.TryParse(NewValue, out var _t))
                                    _viewModel!.Brightness = _t;
                                else
                                {
                                    _log.Debug("Webcam_BrightnessChanged NewValue not int");
                                    return;
                                }
                            });
                        }
                        break;
                        case "Webcam_ZoomChanged":
                        {
                            if (!event_param.TryGetValue("NewValue", out var NewValue))
                            {
                                _log.Debug("NewValue cannot be found in event_param");
                                return;
                            }
                            System.Windows.Application.Current.Dispatcher.Invoke(() =>
                            {
                                if (int.TryParse(NewValue, out var _t))
                                    _viewModel!.Zoom = _t;
                                else
                                {
                                    _log.Debug("Webcam_ZoomChanged NewValue not int");
                                    return;
                                }
                            });
                        }
                        break;
                        case "Webcam_TiltChanged":
                        {
                            //??
                        }
                        break;

                        case "Webcam_PanChanged":
                        {
                            //??
                        }
                        break;

                        case "Webcam_FocusChanged":
                        {
                            if (!event_param.TryGetValue("NewValue", out var NewValue))
                            {
                                _log.Debug("NewValue cannot be found in event_param");
                                return;
                            }
                            System.Windows.Application.Current.Dispatcher.Invoke(() =>
                            {
                                if (int.TryParse(NewValue, out var _t))
                                    _viewModel!.Focus = _t;
                                else
                                {
                                    _log.Debug("Webcam_FocusChanged NewValue not int");
                                    return;
                                }
                            });
                        }
                        break;

                        case "Webcam_IsFocusOnChanged":
                        {
                            if (!event_param.TryGetValue("NewValue", out var NewValue))
                            {
                                _log.Debug("NewValue cannot be found in event_param");
                                return;
                            }
                            System.Windows.Application.Current.Dispatcher.Invoke(() =>
                            {
                                if (NewValue.ToLower() == "true")
                                    _viewModel!.IsFocusOn = true;
                                else
                                    _viewModel!.IsFocusOn = false;
                            });
                        }
                        break;

                        case "Webcam_PriorityChanged":
                        {
                            if (!event_param.TryGetValue("NewValue", out var NewValue))
                            {
                                _log.Debug("NewValue cannot be found in event_param");
                                return;
                            }
                            System.Windows.Application.Current.Dispatcher.Invoke(() =>
                            {
                                if (int.TryParse(NewValue, out var _t))
                                    _viewModel!.Priority = _t;
                                else
                                {
                                    _log.Debug("Webcam_PriorityChanged NewValue not int");
                                    return;
                                }
                            });

                        }
                        break;

                        case "Webcam_CustomProfileRemoved":
                        {

                        }
                        break;

                        case "Webcam_CustomProfileAdded":
                        {

                        }
                        break;

                        case "Webcam_CurrentSelectedProfileChanged":
                        {

                        }
                        break;

                        case "Webcam_IsMicEnumerationOnChanged":
                        {
                            if (!event_param.TryGetValue("NewValue", out var NewValue))
                            {
                                _log.Debug("NewValue cannot be found in event_param");
                                return;
                            }
                            _viewModel!.isIsMicEnumerationOnChanged_event = true;

                            DdpmCommonHelper.WriteUILog("WebCameraMicrophone Action 6 : " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                            System.Windows.Application.Current.Dispatcher.Invoke(() =>
                            {
                                if (NewValue.ToLower() == "true")
                                {
                                    DdpmCommonHelper.WriteUILog("WebCameraMicrophone Action 7 : " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                                    _viewModel!.IsMicEnumerationOn = true;
                                }
                                else
                                {
                                    DdpmCommonHelper.WriteUILog("WebCameraMicrophone Action 8 : " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                                    _viewModel!.IsMicEnumerationOn = false;
                                }
                            });
                            DdpmCommonHelper.WriteUILog("WebCameraMicrophone Action 9 : " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                            _viewModel!.isIsMicEnumerationOnChanged_event = false;
                        }
                        break;

                        case "Webcam_ProfileManagerAdded":
                        {

                        }
                        break;
                        #endregion
                        case "Webcam_Esi_IsCameraSensorCoveredChanged":
                        {
                            if (!event_param.TryGetValue("NewValue", out var NewValue))
                            {
                                _log.Debug("NewValue cannot be found in event_param");
                                return;
                            }

                            if (NewValue.ToLower() == "true")
                            {
                                var _globalSettings = DdpmCommonHelper.DeviceManagerSA!.GetGlobalSettingParam().Result;

                                // check if show OSD for Presence Detection Sensor Cover
                                if (_globalSettings.GlobalSetting_General.Webcam_WB7022_Presence_Detection_Sensor_Cover_State)
                                    DdpmCommonHelper.DeviceManagerSA!.ShowOSD(Screen.PrimaryScreen!.DeviceName, OSDType.Fingerprint);
                            }
                        }
                        break;

                        case "Webcam_Esi_IsWALLockCountdownStartedChanged":
                        {
                            if (!event_param.TryGetValue("NewValue", out var NewValue))
                            {
                                _log.Debug("NewValue cannot be found in event_param");
                                return;
                            }

                            if (NewValue.ToLower() == "true")
                                DdpmCommonHelper.DeviceManagerSA!.ShowOSD(Screen.PrimaryScreen!.DeviceName, OSDType.WalkAwayLock);
                        }
                        break;

                        case "Webcam_WALSnoozeTimeLeftInSecondsChanged":
                        {
                            if (!event_param.TryGetValue("NewValue", out var NewValue))
                            {
                                _log.Debug("NewValue cannot be found in event_param");
                                return;
                            }

                            if (!String.IsNullOrEmpty(NewValue))
                            {
                                if (Int32.TryParse(NewValue, out int numValue))
                                {
                                    TimeSpan ts = TimeSpan.FromSeconds(numValue);
                                    _viewModel.WALSnoozeTimeLeft = ts.ToString(@"hh\:mm\:ss");
                                }
                            }
                        }
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                _log.Debug(ex, "WebCameraplugin_UIUpdateNotify");
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
            Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
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