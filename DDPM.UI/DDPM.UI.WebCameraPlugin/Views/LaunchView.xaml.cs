using CommunityToolkit.Mvvm.Input;
using DDPM.PowerMon;
using DDPM.SA.Common;
using DDPM.SA.Common.Settings;
using DDPM.UI.Common;
using DDPM.UI.Common.Method;
using DDPM.UI.Interfaces;
using DDPM.UI.Module.WebCameraCapture;
using DDPM.UI.Module.WebCameraColorImage;
using DDPM.UI.Module.WebCameraMicrophone;
using DDPM.UI.Module.WebCameraPresenceDetection;
using DDPM.UI.Module.WebCameraSettings;
using DDPM.UI.Plugin.Common;
using DDPM.UI.Plugin.ViewModels;
using Dell.Client.Framework.UX.WPF.Controls;
using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Windows.Devices.Enumeration;
using Windows.Devices.Sensors;
using Windows.Foundation;
using Windows.Graphics.Imaging;
using Windows.Media;
using Windows.Media.Capture;
using Windows.Media.Capture.Frames;
using Windows.Media.MediaProperties;
using Windows.Storage;
using Windows.UI.Popups;
using WinRT;
using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.InteropServices.JavaScript.JSType;
using BitmapEncoder = Windows.Graphics.Imaging.BitmapEncoder;
using Color = System.Windows.Media.Color;
using Image = System.Windows.Controls.Image;
using LangHelper = DDPM.UI.Resources.Helper.LangHelper;
using MessageBox = System.Windows.MessageBox;
using WebcamProfile = DDPM.SA.Common.Settings.WebcamProfile;
using static Windows.Foundation.UniversalApiContract;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.UX.WPF;
using System.Linq.Expressions;
using DDPM.SA.Common.Alert;
using DDPM.UI.Common.UserControls;
using System.Text.RegularExpressions;

namespace DDPM.UI.Plugin.WebCameraPlugin
{
    /// <summary>
    /// WebCameraPlugin.xaml 的互動邏輯
    /// </summary>
    public partial class LaunchView : System.Windows.Controls.UserControl
    {
        // Rotation metadata to apply to the preview stream and recorded videos (MF_MT_VIDEO_ROTATION)
        // Reference: http://msdn.microsoft.com/en-us/library/windows/apps/xaml/hh868174.aspx
        private static readonly Guid RotationKey = new Guid("C380465D-2271-428C-9B83-ECEA3B4A85C1");

        private readonly WebCameraViewModel _vm;

        private readonly int[] _rightFrameWidth = new int[] { 0, 480, 480, 480, 480, 480 };
        private readonly string CameraControl = LangHelper.Instance["Camera.0"];
        private readonly string ColorandImage = LangHelper.Instance["Camera.1"];
        private readonly string PresenceDetection = LangHelper.Instance["Camera.2"];
        private readonly string Capture = LangHelper.Instance["Camera.3"];
        private readonly string Microphone = LangHelper.Instance["Camera.4"];

        private DispatcherTimer _timer = new();
        private int _countdownValue;
        private bool IsPresetOpen = false;
        private readonly Stopwatch stopwatch = new();
        private DispatcherTimer RecordingTimer;
        private bool _running = false;
        private MediaCapture _mediaCapture;
        private SoftwareBitmap backBitmapBuffer;

        private readonly string[] PresetNames = [LangHelper.Instance["Default"], LangHelper.Instance["Smooth"], LangHelper.Instance["Vibrant"], LangHelper.Instance["Warm"]];
        private string EditMode = "";
        private string EditingProfileName = "";
        private static PowerEventControl _pwr_Mon;

        public Thread status_thread;
        public bool exit_status_thread = false;
        private IConsole _console;

        private readonly DispatcherTimer AlertTimer;
        private ModuleGroup moduleGroup;

        enum PresenceDetectionView { InternalUPDSupport, MicrosoftHPDSupport, MicrosoftHPDNotSupport }

        public LaunchView()
        {
            DdpmCommonHelper.WriteUILog($"Webcam UI LaunchView Begin timestamp: {DateTime.Now:hh:mm:ss.ffffff}");
            try
            {
                var vm = (WebCameraViewModel?)WebCameraplugin.PluginIoc?.GetService<IPeripheralViewModel>();
                //_vm = (WebCameraViewModel?)WebCameraplugin.PluginIoc?.GetService<IPeripheralViewModel>();
                if (vm == null)
                {
                    DdpmCommonHelper.WriteUILog("Webcam ViewModel is null");
                    return;
                }

                _vm = vm;
                _console = WebCameraplugin.PluginIoc?.GetService<IConsole>()!;
                if (_console != null)
                {
                    _console.RegisterForEvent(ConsoleEventNames.MainWindow_Force_Camera_Unlock, OnWebcamCloseEvent);
                    DdpmCommonHelper.WriteUILog("[LaunchView] MainWindow_Force_Camera_Unlock event registered");
                }

                InitializeComponent();
                _vm.Reset();
                DataContext = _vm;
                //_vm.VbarItemClickCommand = new RelayCommand<VbarItem>(OnVbarItemClicked!);

                //leo 2024/12/09 因為多加條件判斷,改變呼叫位置
                //BuildModuleGroups();


                if (_vm.CurrentProfileName == "NONE")
                {
                    txtPreset.Text = $"{Strings.Preset}: {LangHelper.Instance["None"]}";
                }
                else
                {
                    var pName = _vm.ProfileCaptions[_vm.CurrentProfileName];
                    if (PresetNames.Contains(pName))
                    {
                        txtPreset.Text = $"{Strings.Preset}: {pName}";
                    }
                    else
                    {
                        txtPreset.Text = Utility.CheckTextLength($"{_vm.CurrentProfileName}", 140, 14);
                    }
                }

                txtAddPreset.Text = LangHelper.Instance["Camera.5"];

                //ProfileItems.ItemsSource = _vm.ProfileNames;
                ProfileItems.ItemsSource = _vm.ProfileItems;
                Mouse.OverrideCursor = null;
                txtName.Text = Strings.Name;
                txtMsg.Text = Strings.NameIsTaken;
                btnCancel.Content = Strings.Cancel;
                btnSave.Content = Strings.Save;

                //lock/unlock, no ui element currently
                if (DdpmCommonHelper.DeviceManagerSA != null)
                {
                    DdpmCommonHelper.DeviceManagerSA.ITSettingsActionEvent += DeviceManagerSA_ITSettingsActionEvent;
                    DdpmCommonHelper.DeviceManagerSA.SystemSuspend += DeviceManagerSA_OnSystemSuspend;
                    DdpmCommonHelper.DeviceManagerSA.SystemResume += DeviceManagerSA_OnSystemResume;
                    //DdpmCommonHelper.DeviceManagerSA.DeviceChanged += DeviceManagerSA_DeviceChanged;
                    DdpmCommonHelper.DeviceManagerSA.SystemSessionEnd += DeviceManagerSA_OnSystemSessionEnd;
                    //Derek 1212
                    DdpmCommonHelper.DeviceManagerSA.UIUpdateNotify += DeviceManagerSA_UIUpdateNotify;

                    DDPMSettings data = DdpmCommonHelper.DeviceManagerSA.ReloadAppConfigData().Result;
                    if (data != null)
                    {
                        if (data.LockSettings.Lock_Setting_RestoreDefaults)
                        {
                            //RestoreLockIcon.Visibility = Visibility.Visible;
                            //txtRestore.IsEnabled = false;
                        }
                        else
                        {
                            //txtRestore.IsEnabled = !data.LockSettings.Lock_Webcam_RestoreFactoryDefaults;
                            //RestoreLockIcon.Visibility = data.LockSettings.Lock_Webcam_RestoreFactoryDefaults ? Visibility.Visible : Visibility.Collapsed;

                            //Lock Functionality 9/7
                            //When a 1 or more settings are locked, automatically lock 'Restore to default'/'factory reset' control [Webcam]                        
                            if (data.LockSettings != null && DdpmCommonHelper.GetUINotifyPropertyValue_isAnyLocked(data, "Lock_Webcam"))
                            {
                                //RestoreLockIcon.Visibility = Visibility.Visible;
                                //txtRestore.IsEnabled = false;
                            }
                        }
                    }
                }

                RecordingTimer = new DispatcherTimer
                {
                    Interval = TimeSpan.FromSeconds(1)
                };
                RecordingTimer.Tick += RecordingTimer_Tick;

                _vm.WebcamSettingChanged += WebcamSettingChanged;
                _vm.ProfilePropertyChanged += ProfilePropertyChanged;

                imgDevice.Visibility = Visibility.Hidden;
                Preview();
                EnableMonitorOnEvent();


                _timer = new DispatcherTimer
                {
                    Interval = TimeSpan.FromSeconds(1)
                };
                _timer.Tick += Timer_Tick;

                //2024/12/21 fixed
                _vm.running_state = true;
                in_CameraPlugin = true;

                exit_status_thread = false;
                if (status_thread == null)
                {
                    status_thread = new Thread(() =>
                    {
                        DateTime dt = DateTime.Now;
                        Console.WriteLine("[CAM THREAD START] " + status_thread.Name + " " + dt.ToString("yyyyMMddHHmmssfff"));
                        status_thread.Name = "t-" + dt.ToString("yyyyMMddHHmmssfff");
                        while (_vm.mre.WaitOne())
                        {

                            if (exit_status_thread)
                            {
                                Console.WriteLine("[CAM THREAD END] " + status_thread.Name + " " + dt.ToString("yyyyMMddHHmmssfff"));
                                return;
                            }

                            if (!_vm.IsRecording)
                            {
                                try
                                {
                                    Dispatcher.Invoke(new Action(() =>
                                    {
                                        status_change();
                                    }));
                                }
                                catch (Exception ex)
                                {
                                    return;
                                }
                            }

                            _vm.mre.Reset();
                        }
                    });
                    _vm.thread_list.Add(status_thread);
                    _vm.mre.Reset();
                    status_thread.Start();
                }



                //因為需要處理PresenceDetection分頁是否出現判斷,改變呼叫順序
                check_PresenceFunction();
                //BuildModuleGroups();
                //initResolutionFPS();
                //usb 2.0限制規則要放在最後做校正
                CheckUSBtype();

                //grdPreview.Visibility = _vm.WebcamGrid ? Visibility.Visible : Visibility.Hidden;
                _vm.VbarItemClickCommand = new RelayCommand<VbarItem1>(OnVbarItemClicked!);
                BuildModuleGroups();
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog("DDPM.UI.WebCameraPlugin\\Views\\LaunchView.xaml.cs  LaunchView() ex:" + ex.Message);
            }

            AlertText1.Text = string.Format(LangHelper.Instance["InputValidationTooltip.1"], "30");
            AlertText2.Text = string.Format(LangHelper.Instance["InputValidationTooltip.2"], $"@ - {LangHelper.Instance["InputValidationTooltip.5"]}");

            AlertTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(5)
            };
            AlertTimer.Tick += AlertTimer_Tick;

            DdpmCommonHelper.WriteUILog($"Webcam UI LaunchView End timestamp: {DateTime.Now:hh:mm:ss.ffffff}");
        }

        private void AlertTimer_Tick(object? sender, EventArgs e)
        {
            bdrAlert2.Visibility = Visibility.Collapsed;
            AlertTimer.Stop();
        }

        private void OnWebcamCloseEvent(object sender, EventManagerArgs e)
        {
            DdpmCommonHelper.WriteUILog($"[WebcamPlugin][OnWebcamCloseEvent] event MainWindow_Force_Camera_Unlock received");
            FreeWebcamResource();
            //if (writeableBitmap != null)
            //{
            //    try
            //    {
            //        writeableBitmap.Unlock();
            //        DdpmCommonHelper.WriteUILog($"[WebcamPlugin][OnWebcamCloseEvent] preview unlocked");
            //    }
            //    catch (Exception ex)
            //    {
            //        DdpmCommonHelper.WriteUILog($"[WebcamPlugin][OnWebcamCloseEvent] writeableBitmap.Unlock() exception: {ex.Message}");
            //    }
            //}
        }

        private void DeviceManagerSA_UIUpdateNotify(object? sender, UpdateUINotify e)
        {
            if (e == null || e == EventArgs.Empty || e.UI_Field_Name == null
                || string.IsNullOrEmpty(e.UI_Field_Name))
                return;

            //DdpmCommonHelper.WriteUILog($"WebcamLanuchView_UIUpdateNotify catch event msg: {e.UI_Field_Name}");

            try
            {
                if (e.UI_Field_Name.StartsWith("WebcamProfileFromQAM")) //1212 Derek
                {
                    //format "WebcamProfileFromQAM:{profileName}"
                    string[] msgs = e.UI_Field_Name.Split(':');

                    if (null != msgs && msgs.Length == 2)
                    {
                        //_vm.CurrentProfileName = msgs[1];
                        DdpmCommonHelper.WriteUILog($"WebcamLanuchView_UIUpdateNotify profileName: {msgs[1]}");

                        ChangeProfileByQAM(msgs[1]);
                    }
                }
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog($"DeviceManagerSA_UIUpdateNotify catch exception: {ex.Message}");
            }

        }

        private void ChangeProfileByQAM(string profileName)
        {
            try
            {
                if (profileName != _vm.CurrentProfileName || isProfilePropertyChanged)
                {
                    _vm.CurrentProfileName = profileName;
                    //Derek 2025/01/18 cancel this action due to it has done by QAM
                    //_vm.SetProfile();
                    isProfilePropertyChanged = false;
                    _vm.isUIHasUpdateByQAM = true;
                }

                Dispatcher.Invoke(new Action(() =>
                {
                    btnPreset_Click(this, null);
                    btnPreset_Click(this, null);
                }));
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog("DDPM.UI.WebCameraPlugin\\Views\\LaunchView.xaml.cs  ChangeProfileByQAM() ex:" + ex.Message);
            }
        }

        ~LaunchView()
        {
            if (DdpmCommonHelper.DeviceManagerSA != null)
                DdpmCommonHelper.DeviceManagerSA.UIUpdateNotify -= DeviceManagerSA_UIUpdateNotify;
            moduleGroup?.Dispose();
        }

        //private void DeviceManagerSA_DeviceChanged(object? sender, DeviceChangedEventArgs e)
        //{
        //    DdpmCommonHelper.WriteUILog($"catch event DeviceManagerSA_DeviceChanged");

        //    if (_vm.IsRecording)
        //        Dispatcher.Invoke(new Action(() =>
        //        {
        //            UserStopRecord();
        //        }));
        //}

        //public void initResolutionFPS()
        //{
        //    _vm.SetResolution_Selected(1);
        //    try
        //    {
        //        if (_vm.WebcamSettings?.SupportedFPSs != null && _vm.WebcamSettings.SelectedResolution != null)
        //        {
        //            if (_vm.WebcamSettings.SupportedFPSs.ContainsKey(_vm.WebcamSettings.SelectedResolution))
        //            {
        //                List<string> FPS = _vm.WebcamSettings.SupportedFPSs[_vm.WebcamSettings.SelectedResolution];
        //                int index = FPS.FindIndex(x => x == "30");
        //                if (index != -1)
        //                {
        //                    _vm.SetFPS_Selected(index);
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        DdpmCommonHelper.WriteUILog("DDPM.UI.WebCameraPlugin\\Views\\LaunchView.xaml.cs initResolutionFPS() : " + ex.Message);
        //    }
        //}

        bool noPresenceFunction = false;

        bool is_WindwosHelloSupport = false;// DdpmCommonHelper.DeviceManagerSA?.GetIsWindowsHelloCapabilityVerified(_vm.CurrentDeviceInfo!.ID.ToString()).Result;

        //api回傳camera是否支援ESI
        bool is_EsiSupport = false;// DdpmCommonHelper.DeviceManagerSA?.GetIsESISupported(_vm.CurrentDeviceInfo!.ID.ToString()).Result;

        //檢查是否為dell7 camera做程式分支處理
        int is_camera_dell7 = 0;// check_camera_dell7();

        //檢查windows是否符合windows hello標準 win10需要大於20H2 win11需要大於22H2
        int is_WindowsVer_OK = 0;// check_windowsVer_OK();

        //檢查是否為dell電腦
        bool is_DellPc = false;// check_DellPc();

        bool is_SUT_internal_presence_sensor = false;// check_SUT_internal_presence_sensor();

        bool AllSupportedResolutions = true;

        public void check_PresenceFunction()
        {

            //先過完check_PresenceFunction()功能後,會再過一次 CheckUSBtype(); 做必要的disable和隱藏

            print_debug("check_PresenceFunction() v1 start");

            //需要特殊邏輯處理的型號
            //List<string> SpecialCase = new List<string>()
            //{
            //    "U3223QZ","U3224KB","U3224KBA","P2424HEB","P2724DEB","P3424WEB","WB7022","P2426HEB","P2726DEB","P3426WEB"
            //};

            string? model = _vm.CurrentDeviceInfo?.ModelNumber;

            if (model == null)
            {
                string log = $"[DDPM.UI.WebCameraPlugin\\Views\\LaunchView.xaml.cs] check_PresenceFunction() model is null";
                DdpmCommonHelper.WriteUILog(log);
                return;
            }

            //if (!SpecialCase.Contains(model))
            //    return;
            DdpmCommonHelper.WriteUILog($"GetIsAllSupportedResolutionsFound Before {AllSupportedResolutions}");
            //check usb 2.0 / 3.0
            //AllSupportedResolutions = DdpmCommonHelper.DeviceManagerSA?.GetIsAllSupportedResolutionsFound(_vm.CurrentDeviceInfo?.ID.ToString() ?? "").Result ?? false;
            //DdpmCommonHelper.WriteUILog($"GetIsAllSupportedResolutionsFound After {AllSupportedResolutions}");
            AllSupportedResolutions = _vm.IsUSB3;
            //api回傳camera硬體是否支援windows hello


            //現在規格已經不需要判斷韌體奇偶數直接從 is_EsiSupport 判斷就好

            //硬體與條件狀態模擬測試 rd測試用
            //if (File.Exists(@"C:\ui_cond\ddpm_cond.txt"))
            //{
            //    ui_cond cond = JsonConvert.DeserializeObject<ui_cond>(File.ReadAllText(@"C:\ui_cond\ddpm_cond.txt"));

            //    is_EsiSupport = cond.is_EsiSupport;
            //    is_WindwosHelloSupport = cond.is_WindwosHelloSupport;
            //    is_camera_dell7 = cond.is_camera_dell7;
            //    is_WindowsVer_OK = cond.is_WindowsVer_OK;
            //    is_DellPc = cond.is_DellPc;
            //    AllSupportedResolutions = cond.AllSupportedResolutions;
            //}
            //SUT 指電腦本身 DUT 外接Cam
            //
            is_WindwosHelloSupport = DdpmCommonHelper.DeviceManagerSA?.GetIsWindowsHelloCapabilityVerified(_vm.CurrentDeviceInfo?.ID.ToString() ?? "").Result ?? false;
            if (_vm.CurrentDeviceInfo != null)
            {
                _vm.CurrentDeviceInfo.IsWindowsHelloSupported = is_WindwosHelloSupport;
            }
            //Windows.Devices.Sensors.HumanPresenceSensor.GetDefaultAsync()

            //api回傳camera是否支援ESI
            is_EsiSupport = DdpmCommonHelper.DeviceManagerSA?.GetIsESISupported(_vm.CurrentDeviceInfo?.ID.ToString() ?? "").Result ?? false;//True UPD False MPS

            //檢查是否為dell7 camera做程式分支處理
            is_camera_dell7 = check_camera_dell7(model);

            if (is_camera_dell7 == 3)
            {
                print_debug("check_camera_dell7: " + is_camera_dell7);
                noPresenceFunction = true;
                _vm.UPD_Visibility = Visibility.Collapsed; //HPD
                _vm.MPS_Setting_Visibility = Visibility.Collapsed;//內建電腦MPS
                _vm.MPS_UpdateFW_Visibility = Visibility.Collapsed;//FW Update

                return;
            }

            //檢查windows是否符合windows hello標準 win10需要大於20H2 win11需要大於22H2
            is_WindowsVer_OK = check_IsMPS_OK();

            //檢查是否為dell電腦
            is_DellPc = check_DellPc(model);
            //by pass DTP to Get UPD Support by FW
            print_debug("is_EsiSupport:" + is_EsiSupport);
            is_SUT_internal_presence_sensor = check_SUT_internal_presence_sensor();
            var fv = _vm.CurrentDeviceInfo.FirmwareVersion.PadLeft(4, '0');
            string FirmwareVersion2 = fv.Contains('.') == true ? fv : $"{fv.Substring(0, 1)}.{fv.Substring(1, 1)}.{fv.Substring(2, 1)}.{fv.Substring(3, 1)}";
            //by pass DTP to Get UPD Support by FW
            bool CheckWebCamFwUPD = new Version(FirmwareVersion2).CompareTo(new Version("0.0.9.2")) <= 0 ? true : new Version(FirmwareVersion2).Equals(new Version("0.0.9.4")) == true ? true : false;
            is_EsiSupport = CheckWebCamFwUPD;
            print_debug("CheckWebCamFwUPD:" + CheckWebCamFwUPD);
            print_debug("is_WindwosHelloSupport:" + is_WindwosHelloSupport);
            print_debug("is_camera_dell7:" + is_camera_dell7);
            print_debug("is_WindowsVer_OK:" + is_WindowsVer_OK);
            print_debug("is_DellPc:" + is_DellPc);
            print_debug("is_SUT_internal_presence_sensor:" + is_SUT_internal_presence_sensor);
            print_debug("AllSupportedResolutions:" + AllSupportedResolutions);
            print_debug("NotNeedtoUpdate:" + CheckWebCamFwUPD);
            print_debug("check_PresenceFunction() s0 model-" + model);
            if (is_camera_dell7 == 1)
            {
                CheckTestCase();
            }
            else if (is_camera_dell7 == 2)
            {
                CheckDisplayWebcamTestCase();
            }
            else
            {
                noPresenceFunction = true;
                _vm.UPD_Visibility = Visibility.Collapsed; //HPD
                _vm.MPS_Setting_Visibility = Visibility.Collapsed;//內建電腦MPS
                _vm.MPS_UpdateFW_Visibility = Visibility.Collapsed;//FW Update
            }
            //CheckTestCase2();

            /*noPresenceFunction = false;
            _vm.UPD_Visibility = Visibility.Visible;
            _vm.brdHello_show = Visibility.Visible;
            _vm.MPS_Setting_Visibility = Visibility.Collapsed;
            _vm.MPS_UpdateFW_Visibility = Visibility.Collapsed;*/
            print_debug("check_PresenceFunction() s22-20250102 14:43 update ver step");

            print_debug("check_PresenceFunction() end");
        }
        private void CheckTestCase()
        {
            string model = _vm.CurrentDeviceInfo?.ModelNumber ?? "";
            noPresenceFunction = false;
            if (!AllSupportedResolutions)
            {
                print_debug("is_camera_dell7==1 && !AllSupportedResolutions");
                noPresenceFunction = true;
                return;
            }

            _vm.UPD_Visibility = Visibility.Collapsed; //HPD
            _vm.MPS_Setting_Visibility = Visibility.Collapsed;//內建電腦MPS
            _vm.MPS_UpdateFW_Visibility = Visibility.Collapsed;//FW Update
            CheckSupportWindowsHello(is_WindwosHelloSupport ? Visibility.Visible : Visibility.Collapsed);
            //1A
            if (is_DellPc && is_EsiSupport && is_WindowsVer_OK == 2)
            {
                print_debug("TestCase 1A");
                _vm.UPD_Visibility = Visibility.Visible;
                CheckSupportWindowsHello(Visibility.Visible); //隱藏人物偵測區windows hello設定連結
                return;
            }
            //2A
            if (is_DellPc && is_EsiSupport && is_WindowsVer_OK == 1)
            {
                print_debug("TestCase 2A");
                _vm.MPS_UpdateFW_Visibility = Visibility.Visible;
                return;
            }
            //3A
            if (is_DellPc && !is_EsiSupport && is_WindowsVer_OK == 1)
            {
                print_debug("TestCase 3A");
                _vm.MPS_Setting_Visibility = Visibility.Visible;
                return;
            }
            //4A
            if (is_DellPc && !is_EsiSupport && is_WindowsVer_OK == 2)
            {
                print_debug("TestCase 4A");
                _vm.MPS_UpdateFW_Visibility = Visibility.Visible;
                return;
            }

            //1B
            if (!is_DellPc && is_EsiSupport && is_WindowsVer_OK == 2)
            {
                print_debug("TestCase 1B");
                noPresenceFunction = true;
                return;
            }
            //2B
            if (!is_DellPc && is_EsiSupport && is_WindowsVer_OK == 1)
            {
                print_debug("TestCase 2B");
                _vm.MPS_UpdateFW_Visibility = Visibility.Visible;
                return;
            }
            //3B
            if (!is_DellPc && !is_EsiSupport && is_WindowsVer_OK == 1)
            {
                print_debug("TestCase 3B");
                _vm.MPS_Setting_Visibility = Visibility.Visible;
                return;
            }

            //4B
            if (!is_DellPc && !is_EsiSupport && is_WindowsVer_OK == 2)
            {
                print_debug("TestCase 4B");
                noPresenceFunction = true;
                return;
            }

            print_debug("TestCase Default");
            noPresenceFunction = true;
        }
        private void CheckDisplayWebcamTestCase()
        {
            string model = _vm.CurrentDeviceInfo?.ModelNumber ?? "";
            noPresenceFunction = false;

            if (!AllSupportedResolutions)
            {
                print_debug("is_camera_dell7 ==2 && !AllSupportedResolutions");
                noPresenceFunction = true;
                return;
            }
            _vm.UPD_Visibility = Visibility.Collapsed; //HPD
            _vm.MPS_Setting_Visibility = Visibility.Collapsed;//內建電腦MPS
            _vm.MPS_UpdateFW_Visibility = Visibility.Collapsed;//FW Update
            CheckSupportWindowsHello(is_WindwosHelloSupport ? Visibility.Visible : Visibility.Collapsed);
            //C#1
            if (is_DellPc)
            {
                print_debug("TestCase C#1");
                _vm.UPD_Visibility = Visibility.Visible;
                CheckSupportWindowsHello(Visibility.Visible); //隱藏人物偵測區windows hello設定連結
                return;
            }
            //C#3
            if (!is_DellPc)
            {
                print_debug("TestCase /C#3");
                noPresenceFunction = true;
                return;
            }

            print_debug("TestCase Default");
            noPresenceFunction = true;
        }
        void CheckSupportWindowsHello(Visibility visibility)
        {
            _vm.brdHello_show_control = _vm.brdHello_show = !is_WindwosHelloSupport ? Visibility.Collapsed : visibility;
        }
        public bool checkWinApiSupportMPS()
        {
            if (File.Exists(@"C:\ui_cond\mps_support.txt"))
                return true;
            return false;
        }

        public void print_debug(string str)
        {
            Console.WriteLine(str);
            string info = DateTime.Now.ToString("yyyy-MM-dd h:mm:tt") + "#" + str + "\r\n";
            if (Directory.Exists(@"C:\ui_cond"))
                File.AppendAllText(@"C:\ui_cond\ui_cond.log", info);

            DdpmCommonHelper.WriteUILog(info);
        }

        public class ui_cond
        {
            public bool AllSupportedResolutions = true;
            public bool is_EsiSupport = true;
            public bool is_WindwosHelloSupport = true;
            public bool is_camera_dell7 = true;
            public bool is_WindowsVer_OK = true;
            public bool is_DellPc = true;
        }

        public bool check_DellPc(string modelName)
        {
            if (modelName.StartsWith("AW") || modelName.ToUpper().Contains("Alienware".ToUpper()))
            {
                return false;
            }

            if (File.Exists(@"C:\ui_cond\dellpc.txt"))
                return true;

            string manufacturer = WinVersion.GetComputerManufacturer();
            if (manufacturer != null && manufacturer.Contains("Dell", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (manufacturer == null)
                DdpmCommonHelper.WriteUILog("check_DellPc() manufacturer == null");

            return false;

        }

        public bool check_SUT_internal_presence_sensor()
        {
            if (File.Exists(@"C:\ui_cond\dellpc.txt"))
                return true;

            string model = WinVersion.GetComputerModel();
            if (model != null && (model.Contains("Latitude 7350", StringComparison.OrdinalIgnoreCase) ||
                model.Contains("XPS 9345", StringComparison.OrdinalIgnoreCase) ||
                model.Contains("XPS 13 9350", StringComparison.OrdinalIgnoreCase) ||
                model.Contains("Insprion 16 7620", StringComparison.OrdinalIgnoreCase) ||
                (model.Contains("XPS", StringComparison.OrdinalIgnoreCase) && model.Contains("9350", StringComparison.OrdinalIgnoreCase)) ||
                (model.Contains("Insprion", StringComparison.OrdinalIgnoreCase) && model.Contains("7620", StringComparison.OrdinalIgnoreCase))
                ))
            {
                return true;
            }

            if (model == null)
                DdpmCommonHelper.WriteUILog("check_SUT_internal_presence_sensor() model == null");
            return false;
            //bool Result = false;
            //try
            //{
            //    Assembly printDll = Assembly.LoadFile(@"C:\Users\wistronits\source\repos\DDPM\bin\Debug\net8.0-windows10.0.19041.0\CallHumanPresenceSensor.dll");
            //    Type typeTest = printDll.GetType("CallHumanPresenceSensor.CallHumanPresenceSensor");
            //    MethodInfo miGetMethod = typeTest.GetMethod("IsEngagementSupported");
            //    var printTestClass = Activator.CreateInstance(typeTest);
            //    Result = (bool)miGetMethod.Invoke(printTestClass, null);
            //}
            //catch (Exception ex)
            //{
            //    print_debug("Exception ex" + ex.Message);
            //    return Result;
            //}
            //return Result;

        }

        public int check_IsMPS_OK()
        {
            //作業系統必須是Windows10 20H2 以上
            //或是Windows11 22H2以上
            if (WinVersion.GetVersion(out var info))
            {
                //win11 >=Win11 22H2 latter and OsBuild>=22621
                if (info.BuildNum >= (uint)(BuildNumber.Windows_11_22H2) && WinVersion.GetOsBuild() >= 22621)
                    return 1;
                ////win11 >= Win11 22H2 latter and OsBuild< 22621
                //if (info.BuildNum >= (uint)(BuildNumber.Windows_11_22H2) && WinVersion.GetOsBuild() < 22621)
                //    return 2;
                //Win10 Win11<22H2
                if (info.BuildNum < (uint)(BuildNumber.Windows_11_22H2) && info.BuildNum >= (uint)(BuildNumber.Windows_10_1507))
                    return 2;
                ////win10/11
                //if (info.BuildNum <= (uint)(BuildNumber.Windows_11_21H2) && info.BuildNum >= (uint)(BuildNumber.Windows_10_1507))
                //    return 4;

            }
            return 0;
        }

        public bool check_windowsVer_OK()
        {
            //作業系統必須是Windows10 20H2 以上
            //或是Windows11 22H2以上
            if (WinVersion.GetVersion(out var info))
            {
                //win11以上
                if (info.BuildNum >= (uint)(BuildNumber.Windows_11_22H2))
                    return true;

                //win10以上
                if (info.BuildNum <= (uint)(BuildNumber.Windows_11_21H2) && info.BuildNum >= (uint)(BuildNumber.Windows_10_20H2))
                    return true;

            }
            return false;
        }
        string[] DisplayWebcameList = new string[] { "U3223QZ", "U3224KB", "U3224KBA", "P2424HEB", "P2724DEB", "P3424WEB" };

        string[] DisplayWebcameList2 = new string[] { "P3426WEB", "P2726DEB", "P2426HEB" };

        public int check_camera_dell7(string model)
        {
            //hard code 指定特定型號是否為internal
            if (model == "WB7022")
            {
                return 1;
            }
            if (DisplayWebcameList.ToArray().Any(x => x == model))
            {
                return 2;
            }
            if (DisplayWebcameList2.ToArray().Any(x => x == model))
            {
                return 3;
            }

            return 0;
            //switch (model)
            //{
            //    ////螢幕嵌入camera都為internal
            //    case "U3223QZ":
            //    case "U3224KB":
            //    case "U3224KBA":
            //    case "P2424HEB":
            //    case "P2724DEB":
            //    case "P3424WEB":
            //        return true;

            //    //usb 外接
            //    case "WB7022":
            //        return true;

            //    default:
            //        return false;
            //}
        }

        public void CheckUSBtype()
        {
            print_debug("CheckUSBtype() v1 start");

            _vm.MessageBoxVisibilityUsbType = Visibility.Collapsed;

            string model = _vm.CurrentDeviceInfo?.ModelNumber ?? "";

            if (model == null)
            {
                string log = $"[DDPM.UI.WebCameraPlugin\\Views\\LaunchView.xaml.cs] CheckUSBtype() model is null";
                DdpmCommonHelper.WriteUILog(log);
                return;
            }

            //print_debug("CheckUSBtype() s1 model-" + model);

            //print_debug("CheckUSBtype() s2");

            //Dean 2025/3/24 remove test code.
            //硬體與條件狀態模擬測試 rd測試用
            /*if (File.Exists(@"C:\ui_cond\ddpm_cond.txt"))
            {
                try
                {
                    ui_cond cond = JsonConvert.DeserializeObject<ui_cond>(File.ReadAllText(@"C:\ui_cond\ddpm_cond.txt"));
                    AllSupportedResolutions = cond.AllSupportedResolutions;
                }
                catch(Exception ex)
                {
                    DdpmCommonHelper.WriteUILog($"CheckUSBtype catch exception: {ex.Message}");

                }

            }*/

            print_debug("CheckUSBtype() s3 AllSupportedResolutions- " + AllSupportedResolutions);

            switch (model)
            {
                case "WB7022":
                    if (!AllSupportedResolutions)
                    {
                        print_debug("CheckUSBtype() s4");
                        //hdr on按鈕diable & 功能關閉
                        _vm.hdr_enable = false;
                        _vm.usb_hdr_enable = false;
                        //_vm.IsHDROn = false;

                        // ProximitySensor按鈕diable & 功能關閉
                        _vm.is_ProximitySensor_enable = false;
                        _vm.IsChecked_ProximitySensor = false;

                        //autoframe功能關閉 & 區域隱藏
                        _vm.is_AutoFramingVisibility = false;
                        //_vm.IsAutoFramingOn = false;

                        //身分偵測整個功能區域隱藏 
                        //at BuildModuleGroups() to do


                        //攝影機控制區域內windows hello隱藏
                        CheckSupportWindowsHello(Visibility.Collapsed);

                        //連接usb 3.0提示訊息 Camera.14
                        //Connect your monitor via USB 3.0 to enable 4K UHD resolution.
                        _vm.MessageBoxVisibilityUsbType = Visibility.Visible;
                        //_vm.usbtype_info_v = LangHelper.Instance["Camera.25"]; // Jim 20250115 modify PIMS-294596
                        _vm.usbtype_info_v = LangHelper.Instance["Camera.27"]; // Jim 20250115 modify PIMS-294596 //20250120 WB7022 is External webcam not Monitor; 

                        //fps與解析度,排除4k
                        //Connect your monitor via USB 3.0 to enable 4K UHD resolution.


                        //2025/01/07
                        //There is no need to handle the resolution project by yourself, IL will provide the corresponding resolution according to USB 2.0/3.0.
                        /*_vm.btnRes0_show = Visibility.Collapsed;
                        _vm.btnRes0_width = 0;
                        _vm.btnRes1_width = 201;
                        _vm.btnRes1_radius_v = new CornerRadius(5, 0, 0, 5);
                        _vm.btnRes2_width = 201;*/

                        if (_vm.WebcamSettings.SelectedResolution != "Full HD" && _vm.WebcamSettings.SelectedResolution != "HD")
                        {
                            //_vm.SetResolution_Selected(1);
                            if (_vm.WebcamSettings.SupportedFPSs.ContainsKey(_vm.WebcamSettings.SelectedResolution))
                            {
                                List<string> FPS = _vm.WebcamSettings.SupportedFPSs[_vm.WebcamSettings.SelectedResolution];
                                int index = FPS.FindIndex(x => x == "30");
                                if (index != -1)
                                {
                                    _vm.SetFPS_Selected(index);
                                }
                            }
                        }

                        //camera控制權
                        SetPrioritizeShow(Visibility.Collapsed);

                    }
                    else
                    {
                        //恢復usb 3.0預設
                        print_debug("CheckUSBtype() s4-1");
                        //hdr on按鈕diable & 功能關閉
                        _vm.hdr_enable = true;
                        _vm.usb_hdr_enable = true;
                        //_vm.IsHDROn = true;

                        // ProximitySensor按鈕diable & 功能關閉
                        _vm.is_ProximitySensor_enable = true;
                        //_vm.IsChecked_ProximitySensor = true; // Jim 20250116 modify for PIMS-297931 by lio comment

                        //autoframe功能關閉 & 區域隱藏
                        _vm.is_AutoFramingVisibility = true;
                        //_vm.IsAutoFramingOn = true;

                        //身分偵測整個功能區域隱藏 
                        //at BuildModuleGroups() to do


                        //攝影機控制區域內windows hello隱藏
                        //if (_vm.UPD_Visibility == Visibility.Collapsed) // Jim 20250116 modify for PIMS-319086
                        //    CheckSupportWindowsHello(Visibility.Collapsed);   // Jim 20250116 modify for PIMS-319086
                        //else
                        //    CheckSupportWindowsHello(Visibility.Visible);

                        //連接usb 3.0提示訊息 Camera.14
                        //Connect your monitor via USB 3.0 to enable 4K UHD resolution.
                        _vm.MessageBoxVisibilityUsbType = Visibility.Collapsed;
                        //_vm.usbtype_info_v = LangHelper.Instance["Camera.25"]; // Jim 20250115 modify PIMS-294596
                        _vm.usbtype_info_v = LangHelper.Instance["Camera.27"]; // Jim 20250115 modify PIMS-294596

                        //fps與解析度,排除4k
                        //Connect your monitor via USB 3.0 to enable 4K UHD resolution.
                        //2025/01/07
                        //There is no need to handle the resolution project by yourself, IL will provide the corresponding resolution according to USB 2.0/3.0.
                        /*_vm.btnRes0_show = Visibility.Visible;
                        _vm.btnRes0_width = 133;
                        _vm.btnRes0_radius_v = new CornerRadius(5, 0, 0, 5);


                        _vm.btnRes1_width = 133;
                        _vm.btnRes1_radius_v = new CornerRadius(0, 0, 0, 0);
                        _vm.btnRes2_width = 133;*/

                        //camera控制權
                        SetPrioritizeShow(Visibility.Visible);

                    }
                    break;
                case "U3224KBA":
                case "U3224KB":
                    if (!AllSupportedResolutions)
                    {
                        print_debug("CheckUSBtype() s5");
                        //hdr.ProximitySensor.autoframe功能保留
                        //身分偵測整個功能區域隱藏保留


                        CheckSupportWindowsHello(Visibility.Visible);

                        //連接usb 3.0提示訊息 Camera.14
                        //Connect your monitor via USB 3.0 to enable 4K UHD resolution.
                        _vm.MessageBoxVisibilityUsbType = Visibility.Visible;
                        //_vm.usbtype_info_v = LangHelper.Instance["Camera.25"]; // Jim 20250115 modify PIMS-294596
                        _vm.usbtype_info_v = LangHelper.Instance["Camera.26"]; // Jim 20250115 modify PIMS-294596

                        //fps與解析度,排除4k Camera.14

                        //2025/01/07
                        //There is no need to handle the resolution project by yourself, IL will provide the corresponding resolution according to USB 2.0/3.0.
                        /*_vm.btnRes0_show = Visibility.Collapsed;
                        _vm.btnRes0_width = 0;
                        _vm.btnRes1_width = 201;
                        _vm.btnRes1_radius_v = new CornerRadius(5, 0, 0, 5);
                        _vm.btnRes2_width = 201;*/
                        //_vm.SetResolution_Selected(1);

                        if (_vm.WebcamSettings.SelectedResolution != "Full HD" && _vm.WebcamSettings.SelectedResolution != "HD")
                        {
                            //_vm.SetResolution_Selected(1);
                            if (_vm.WebcamSettings.SupportedFPSs.ContainsKey(_vm.WebcamSettings.SelectedResolution))
                            {
                                List<string> FPS = _vm.WebcamSettings.SupportedFPSs[_vm.WebcamSettings.SelectedResolution];
                                int index = FPS.FindIndex(x => x == "30");
                                if (index != -1)
                                {
                                    _vm.SetFPS_Selected(index);
                                }
                            }
                        }



                        //camera控制權
                        //_vm.bdrPrioritize_show = Visibility.Collapsed; // Jim 20250115 modify for PIMS-297931
                        SetPrioritizeShow(Visibility.Visible); // Jim 20250115 modify for PIMS-297931
                    }
                    else
                    {
                        print_debug("CheckUSBtype() s5-1");
                        //hdr.ProximitySensor.autoframe功能保留
                        //身分偵測整個功能區域隱藏保留


                        //攝影機控制區域內windows hello隱藏
                        CheckSupportWindowsHello(Visibility.Visible);


                        //連接usb 3.0提示訊息 Camera.14
                        //Connect your monitor via USB 3.0 to enable 4K UHD resolution.
                        _vm.MessageBoxVisibilityUsbType = Visibility.Collapsed;
                        //_vm.usbtype_info_v = LangHelper.Instance["Camera.25"]; // Jim 20250115 modify PIMS-294596
                        _vm.usbtype_info_v = LangHelper.Instance["Camera.26"]; // Jim 20250115 modify PIMS-294596

                        //fps與解析度,排除4k Camera.14
                        //2025/01/07
                        //There is no need to handle the resolution project by yourself, IL will provide the corresponding resolution according to USB 2.0/3.0.
                        /*_vm.btnRes0_show = Visibility.Visible;
                        _vm.btnRes0_width = 133;
                        _vm.btnRes0_radius_v = new CornerRadius(5, 0, 0, 5);
                        _vm.btnRes1_width = 133;
                        _vm.btnRes1_radius_v = new CornerRadius(0, 0, 0, 0);
                        _vm.btnRes2_width = 133;*/

                        //camera控制權
                        //_vm.bdrPrioritize_show = Visibility.Collapsed; // Jim 20250116 modify for PIMS-297931 by lio comment
                        SetPrioritizeShow(Visibility.Visible); // Jim 20250116 modify for PIMS-297931 by lio comment
                    }
                    break;
                case "U3223QZ":
                    if (!AllSupportedResolutions)
                    {
                        print_debug("CheckUSBtype() s6");
                        //hdr.ProximitySensor.autoframe功能保留
                        //身分偵測整個功能區域隱藏保留

                        //攝影機控制區域內windows hello隱藏
                        CheckSupportWindowsHello(Visibility.Visible); // Jim 20250117 modify for PIMS-297931 by dell PO decdie comment

                        //連接usb 3.0提示訊息 Camera.15
                        //Connect your monitor via USB 3.0 and select 'High Data Speed' under USB-C Prioritization to enable 4K UHD resolution.
                        _vm.MessageBoxVisibilityUsbType = Visibility.Visible;
                        _vm.usbtype_info_v = LangHelper.Instance["Camera.25"];

                        //fps與解析度,排除4k 
                        //2025/01/07
                        //There is no need to handle the resolution project by yourself, IL will provide the corresponding resolution according to USB 2.0/3.0.
                        /*_vm.btnRes0_show = Visibility.Collapsed;
                        _vm.btnRes0_width = 0;
                        _vm.btnRes1_width = 201;
                        _vm.btnRes1_radius_v = new CornerRadius(5, 0, 0, 5);
                        _vm.btnRes2_width = 201;*/
                        //_vm.SetResolution_Selected(1);

                        if (_vm.WebcamSettings.SelectedResolution != "Full HD" && _vm.WebcamSettings.SelectedResolution != "HD")
                        {
                            //_vm.SetResolution_Selected(1);
                            if (_vm.WebcamSettings.SupportedFPSs.ContainsKey(_vm.WebcamSettings.SelectedResolution))
                            {
                                List<string> FPS = _vm.WebcamSettings.SupportedFPSs[_vm.WebcamSettings.SelectedResolution];
                                int index = FPS.FindIndex(x => x == "30");
                                if (index != -1)
                                {
                                    _vm.SetFPS_Selected(index);
                                }
                            }
                        }

                        //camera控制權
                        //_vm.bdrPrioritize_show = Visibility.Collapsed; // Jim 20250115 modify for PIMS-297931
                        SetPrioritizeShow(Visibility.Visible); // Jim 20250115 modify for PIMS-297931

                    }
                    else
                    {
                        print_debug("CheckUSBtype() s6-1");
                        //hdr.ProximitySensor.autoframe功能保留
                        //身分偵測整個功能區域隱藏保留

                        //攝影機控制區域內windows hello隱藏
                        CheckSupportWindowsHello(Visibility.Visible);


                        //連接usb 3.0提示訊息 Camera.15
                        //Connect your monitor via USB 3.0 and select 'High Data Speed' under USB-C Prioritization to enable 4K UHD resolution.
                        _vm.MessageBoxVisibilityUsbType = Visibility.Collapsed;
                        _vm.usbtype_info_v = LangHelper.Instance["Camera.25"];

                        //fps與解析度,排除4k 
                        //2025/01/07
                        //There is no need to handle the resolution project by yourself, IL will provide the corresponding resolution according to USB 2.0/3.0.
                        /*_vm.btnRes0_show = Visibility.Collapsed;
                        _vm.btnRes0_width = 133;
                        _vm.btnRes0_radius_v = new CornerRadius(5, 0, 0, 5);
                        _vm.btnRes1_width = 133;
                        _vm.btnRes1_radius_v = new CornerRadius(0, 0, 0, 0);
                        _vm.btnRes2_width = 133;*/

                        //camera控制權
                        //_vm.bdrPrioritize_show = Visibility.Collapsed; // Jim 20250116 modify for PIMS-297931 by lio comment
                        SetPrioritizeShow(Visibility.Visible); // Jim 20250116 modify for PIMS-297931 by lio comment
                    }
                    break;
                case "P2426HEB":
                case "P2726DEB":
                case "P3426WEB":
                case "P2424HEB":
                case "P2724DEB":
                case "P3424WEB":
                    if (!AllSupportedResolutions)
                    {
                        print_debug("CheckUSBtype() s7");
                        //連接usb 3.0提示訊息 Camera.15
                        //Connect your monitor via USB 3.0 and select 'High Data Speed' under USB-C Prioritization to enable 4K UHD resolution.
                        _vm.MessageBoxVisibilityUsbType = Visibility.Visible;
                        _vm.usbtype_info_v = LangHelper.Instance["Camera.25"].Replace("4K", "2K");

                        //fps與解析度,排除2k 
                        //2025/01/07
                        //There is no need to handle the resolution project by yourself, IL will provide the corresponding resolution according to USB 2.0/3.0.
                        /*_vm.btnRes0_show = Visibility.Collapsed;
                        _vm.btnRes0_width = 0;
                        _vm.btnRes1_width = 201;
                        _vm.btnRes1_radius_v = new CornerRadius(5, 0, 0, 5);
                        _vm.btnRes2_width = 201;*/
                        //_vm.SetResolution_Selected(1);

                        if (_vm.WebcamSettings.SelectedResolution != "Full HD" && _vm.WebcamSettings.SelectedResolution != "HD")
                        {
                            //_vm.SetResolution_Selected(1);
                            if (_vm.WebcamSettings.SupportedFPSs.ContainsKey(_vm.WebcamSettings.SelectedResolution))
                            {
                                List<string> FPS = _vm.WebcamSettings.SupportedFPSs[_vm.WebcamSettings.SelectedResolution];
                                int index = FPS.FindIndex(x => x == "30");
                                if (index != -1)
                                {
                                    _vm.SetFPS_Selected(index);
                                }
                            }
                        }

                        //camera控制權
                        SetPrioritizeShow(Visibility.Collapsed);
                    }
                    else
                    {
                        print_debug("CheckUSBtype() s7-1");
                        //連接usb 3.0提示訊息 Camera.15
                        //Connect your monitor via USB 3.0 and select 'High Data Speed' under USB-C Prioritization to enable 4K UHD resolution.
                        _vm.MessageBoxVisibilityUsbType = Visibility.Collapsed;
                        _vm.usbtype_info_v = LangHelper.Instance["Camera.25"].Replace("4K", "2K");

                        //fps與解析度,排除2k 
                        //2025/01/07
                        //There is no need to handle the resolution project by yourself, IL will provide the corresponding resolution according to USB 2.0/3.0.
                        /*_vm.btnRes0_show = Visibility.Visible;
                        _vm.btnRes0_width = 133;
                        _vm.btnRes0_radius_v = new CornerRadius(5, 0, 0, 5);
                        _vm.btnRes1_width = 133;
                        _vm.btnRes1_radius_v = new CornerRadius(0, 0, 0, 0);
                        _vm.btnRes2_width = 133;*/

                        //camera控制權
                        SetPrioritizeShow(Visibility.Visible);
                    }
                    break;
            }

            //for p.u camera
            if (model != "WB7022")
            {
                _vm.hdr_enable = true;
                _vm.usb_hdr_enable = true;
                //_vm.IsHDROn = true;

                _vm.is_AutoFramingVisibility = true;
                //_vm.IsAutoFramingOn = true;

                _vm.is_ProximitySensor_enable = true;
                //_vm.IsChecked_ProximitySensor = true; // Jim 20250116 modify for PIMS-297931 by lio comment
            }
            print_debug("CheckUSBtype() end");
        }

        private void SetPrioritizeShow(Visibility visibility)
        {
            _vm.bdrPrioritize_show = !is_WindwosHelloSupport ? Visibility.Collapsed : visibility;
        }

        //bool WebcamGrid_old_ststus = false;
        private void status_change()
        {

            if (!in_CameraPlugin)
                return;
            if (_vm == null)
                return;

            if (_vm.running_state)
            {

                //if (_vm.MediaCapture == null || _vm.MediaFrameReader == null)
                {
                    _ = CameraImage.Dispatcher.BeginInvoke(() =>
                    {
                        imgDevice.Visibility = Visibility.Hidden;
                        Preview();
                        CameraImage.Visibility = Visibility.Visible;

                        //恢復9宮格線
                        _vm.ShowGrid = true;
                    });

                }
            }
            else
            {
                //if (_vm.MediaCapture != null || _vm.MediaFrameReader != null)
                {
                    _ = CameraImage.Dispatcher.BeginInvoke(async () =>
                    {
                        CameraImage.Visibility = Visibility.Hidden;
                        _ = CleanupMediaCaptureAsync();

                        //WebcamGrid_old_ststus = _vm.WebcamGrid;
                        _vm.ShowGrid = false;

                        imgDevice.Visibility = Visibility.Visible;
                        DoubleAnimation visibilityAnimation = new()
                        {
                            From = 0,
                            To = 1,
                            Duration = new Duration(TimeSpan.FromSeconds(0.3))
                        };
                        visibilityAnimation.Completed += ShowGrid;
                        imgDevice.BeginAnimation(OpacityProperty, visibilityAnimation);

                        if (!_vm.hdr_change)
                        {
                            _vm.AlertType = WebcamAlert.Alert2;
                            _vm.IsResolutionSectionEnable = false;
                            _vm.OnPropertyChanged(nameof(_vm.IsResolutionSectionEnable));
                            _vm.AlertVisibility = Visibility.Visible;
                            Debug.WriteLine("show Alert");

                        }
                    });
                }
            }
        }

        private void EnableMonitorOnEvent()
        {
            Dispatcher.Invoke(new Action(() =>
            {
                if (_pwr_Mon == null)
                {
                    _pwr_Mon = new PowerEventControl(null);
                    _pwr_Mon.MonitorTurnedOn += MonitorEvent_On;
                    _pwr_Mon.Enable_Event();
                }
            }));
        }

        private void MonitorEvent_On(object sender, EventArgs e)
        {
            Trace.WriteLine("GET MONITOR ON EVENT");
        }

        private bool isProfilePropertyChanged = false;
        private void ProfilePropertyChanged(object? sender, EventArgs e)
        {
            txtPreset.Text = $"{Strings.Preset}: {LangHelper.Instance["None"]}";
            isProfilePropertyChanged = true;
        }

        private void WebcamSettingChanged(object? sender, EventArgs e)
        {
            _vm.mre.Set();
            //Preview();
        }

        private async void Preview()
        {
            if (_vm.MediaCapture != null)
            { _ = CleanupMediaCaptureAsync(); }

            try
            {
                // jim add 20240621
                var frameSourceGroups = await MediaFrameSourceGroup.FindAllAsync();

                // 20240626  jim add to avoid exception

                if (frameSourceGroups.Count <= 0)
                {
                    DdpmCommonHelper.WriteUILog("frameSourceGroups.Count = 0");
                    return;
                }

                // 20240626 jim add
                MediaFrameSourceGroup? selectedFrameSourceGroup = frameSourceGroups[0];

                int index_matched_webcam = 0;

                for (index_matched_webcam = 0; index_matched_webcam < frameSourceGroups.Count; index_matched_webcam++)
                {
                    selectedFrameSourceGroup = frameSourceGroups[index_matched_webcam];

                    if (_vm != null && _vm.CurrentDeviceInfo != null)
                    {
                        if (selectedFrameSourceGroup.Id.Contains(_vm.CurrentDeviceInfo.DeviceSymbolicLink, StringComparison.CurrentCultureIgnoreCase))
                            break;

                        //if (selectedFrameSourceGroup.DisplayName.Contains(_vm.CurrentDeviceInfo.ModelNumber, StringComparison.CurrentCultureIgnoreCase))
                        //    break;
                    }
                    else
                        selectedFrameSourceGroup = null;
                }

                // 20240626  jim add to avoid exception
                if (selectedFrameSourceGroup == null)
                {
                    DdpmCommonHelper.WriteUILog("selectedGroup null");
                    return;
                }

                MediaFrameSourceInfo frameSourceInfo = selectedFrameSourceGroup.SourceInfos[0];

                _vm.MediaCapture = new MediaCapture();
                _vm.MediaCapture.Failed += handler;

                try
                {

                    // get mic list first
                    var audioDevices = await DeviceInformation.FindAllAsync(DeviceClass.AudioCapture);

                    List<DeviceInformation> devices_List = new List<DeviceInformation>();
                    foreach (DeviceInformation device in audioDevices)
                    {
                        if (device.IsEnabled)
                        {
                            //check device enable
                            devices_List.Add(device);
                        }
                    }

                    DeviceInformation microphone = null;
                    if (devices_List.Count > 0)
                    {
                        microphone = devices_List[0];
                        foreach (DeviceInformation device in devices_List)
                        {
                            if (!string.IsNullOrEmpty(_vm.Model) && device.Name.Contains(_vm.Model))
                            {
                                microphone = device;
                            }
                        }
                    }

                    // 2024/12/31 Elie.
                    var captureMode = devices_List.Count == 0 ? StreamingCaptureMode.Video : StreamingCaptureMode.AudioAndVideo;

                    //var microphone = audioDevices.FirstOrDefault();

                    string AudioDeviceId = ""; // 2024/12/31 Elie.
                    if (microphone != null)
                    {
                        AudioDeviceId = microphone.Id;
                    }

                    //if (audioDevices != null)
                    if (!string.IsNullOrEmpty(AudioDeviceId))
                    {
                        await _vm.MediaCapture.InitializeAsync(new MediaCaptureInitializationSettings()
                        {
                            AudioDeviceId = AudioDeviceId,
                            SourceGroup = selectedFrameSourceGroup,
                            SharingMode = MediaCaptureSharingMode.ExclusiveControl,
                            //SharingMode = MediaCaptureSharingMode.SharedReadOnly,
                            MemoryPreference = MediaCaptureMemoryPreference.Cpu,
                            MediaCategory = MediaCategory.Communications,
                            StreamingCaptureMode = captureMode
                        });
                    }
                    else
                    {
                        await _vm.MediaCapture.InitializeAsync(new MediaCaptureInitializationSettings()
                        {
                            SourceGroup = selectedFrameSourceGroup,
                            SharingMode = MediaCaptureSharingMode.ExclusiveControl,
                            //SharingMode = MediaCaptureSharingMode.SharedReadOnly,
                            MemoryPreference = MediaCaptureMemoryPreference.Cpu,
                            MediaCategory = MediaCategory.Communications,
                            StreamingCaptureMode = captureMode
                        });
                    }
                }
                catch (Exception ex)
                {
                    print_debug("ex1:" + ex.Message);
                    Task.Delay(200).Wait();//Thread.Sleep(200);//for wait device init
                    DdpmCommonHelper.WriteUILog("MediaCapture initiate fail (retry): " + ex.Message);
                    _vm.mre.Set();
                    return;
                }

                if (_vm.MediaCapture == null)
                {
                    print_debug("_vm.MediaCapture == null");
                    Task.Delay(100).Wait();//Thread.Sleep(100);//for wait device init
                    DdpmCommonHelper.WriteUILog("WebCameraMicrophone Action 10 (retry) : " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    _vm.mre.Set();
                    return;
                }

                //Derek 1108 Move to here to fix Webcam PIMS-314613
                // Query all properties [resolution and frame rate] of the webcam device
                _vm.allProperties = _vm.MediaCapture.VideoDeviceController.GetAvailableMediaStreamProperties(MediaStreamType.VideoPreview).Select(x => new StreamResolution(x));
                DdpmCommonHelper.WriteUILog($"Preview 28 {JsonConvert.SerializeObject(_vm.allProperties)}");
                // Order them by resolution then frame rate
                _vm.allProperties = _vm.allProperties.OrderByDescending(x => x.Height * x.Width).ThenByDescending(x => x.FrameRate);
                foreach (var property in _vm.allProperties)
                {
                    string properties_temp = property.GetFriendlyName();
                    if (properties_temp.Contains(_vm.WebcamSettings.CurrentResolution, StringComparison.OrdinalIgnoreCase) &&
                        properties_temp.Contains(_vm.WebcamSettings.CurrentFPS, StringComparison.OrdinalIgnoreCase) &&
                        property.EncodingProperties.Subtype != "MJPG")
                    {
                        DdpmCommonHelper.WriteUILog($"properties_temp: {properties_temp}");
                        var encodingProperties = property.EncodingProperties;
                        DdpmCommonHelper.WriteUILog($"encodingProperties: {encodingProperties} Subtype: {encodingProperties.Subtype}");
                        _ = _vm.MediaCapture!.VideoDeviceController.SetMediaStreamPropertiesAsync(MediaStreamType.VideoPreview, encodingProperties);
                        break;
                    }
                }

                MediaFrameSource mediaFrameSource = _vm.MediaCapture.FrameSources[frameSourceInfo.Id];

                // 20240626 jim modify
                _vm.MediaFrameReader = await _vm.MediaCapture.CreateFrameReaderAsync(mediaFrameSource, MediaEncodingSubtypes.Argb32);

                _vm.MediaFrameReader.FrameArrived += MediaFrameReader_FrameArrived;

                await _vm.MediaFrameReader.StartAsync();

                try
                {
                    //Camera被其他process搶先佔據情況下,初始化會失敗,跟據IL提供範例,也是用檢測初始化是否成功為判斷
                    uint _t = mediaFrameSource.CurrentFormat.VideoFormat.Width;
                }
                catch
                {
                    _vm.AlertType = WebcamAlert.Alert2;
                    _vm.AlertVisibility = Visibility.Visible;
                    return;
                }

                DoubleAnimation visibilityAnimation = new()
                {
                    From = 1,
                    To = 0,
                    Duration = new Duration(TimeSpan.FromSeconds(0.3))
                };
                visibilityAnimation.Completed += ShowGrid;
                imgDevice.BeginAnimation(OpacityProperty, visibilityAnimation);

                writeableBitmap = new(
                    (int)mediaFrameSource.CurrentFormat.VideoFormat.Width,
                    (int)mediaFrameSource.CurrentFormat.VideoFormat.Height,
                    96,
                    96,
                    PixelFormats.Bgra32,
                    null);
                react = new Int32Rect(0, 0, writeableBitmap.PixelWidth, writeableBitmap.PixelHeight);
                ImageBufferSize = writeableBitmap.PixelWidth * writeableBitmap.PixelHeight * 4;
                CameraImage.Source = writeableBitmap;
                before_width = (int)mediaFrameSource.CurrentFormat.VideoFormat.Width;
                before_height = (int)mediaFrameSource.CurrentFormat.VideoFormat.Height;

                _vm.AlertVisibility = Visibility.Hidden;
                _vm.IsResolutionSectionEnable = true;
                _vm.OnPropertyChanged(nameof(_vm.IsResolutionSectionEnable));
                Debug.WriteLine("hide alert");

            }
            catch (Exception Exc)
            {
                print_debug("ex2:" + Exc.Message);
                DdpmCommonHelper.WriteUILog("MediaCapture initialization failed 2: " + Exc.Message);
            }
        }

        private void ShowGrid(object? sender, EventArgs e)
        {
            DoubleAnimation visibilityAnimation = new()
            {
                From = 0,
                To = 1,
                Duration = new Duration(TimeSpan.FromSeconds(0.1))
            };
            grdPreview.BeginAnimation(OpacityProperty, visibilityAnimation);
        }

        private void RecordingTimer_Tick(object? sender, EventArgs e)
        {
            txtTimer.Text = stopwatch.Elapsed.ToString(@"hh\:mm\:ss");

            //這邊做錄影長度限制 2小時
            if (txtTimer.Text == "02:00:01")
            {
                Dispatcher.Invoke(new Action(() =>
                {
                    UserStopRecord();
                }));
            }

            if (!HasEnoughSpace(_vm.VideoCaptureFolder, 20 * 1024 * 1024))//不到20MB時停止錄影
            {
                Dispatcher.Invoke(new Action(() =>
                {
                    UserStopRecord();
                }));
            }

        }

        private void DeviceManagerSA_ITSettingsActionEvent(object? sender, SA.Common.ITSettingEventArgs e)
        {
            var rst = DdpmCommonHelper.ApplyRestoreFactoryDefaultsEventData(e, "Lock_Webcam_RestoreFactoryDefaults");
            Dispatcher.Invoke(new Action(() =>
            {
                //no ui element currently
                //RestoreLockIcon.Visibility = rst.isLocked;
                //txtRestore.IsEnabled = rst.isEnabled;

                //Lock Functionality 9/7
                //When a 1 or more settings are locked, automatically lock 'Restore to default'/'factory reset' control [Webcam]
                if (DdpmCommonHelper.DeviceManagerSA != null)
                {
                    DDPMSettings data = DdpmCommonHelper.DeviceManagerSA.ReloadAppConfigData().Result;
                    if (data != null && data.LockSettings != null && DdpmCommonHelper.GetUINotifyPropertyValue_isAnyLocked(data, "Lock_Webcam"))
                    {
                        //RestoreLockIcon.Visibility = Visibility.Visible;
                        //txtRestore.IsEnabled = false;
                    }
                }
            }));
        }

        private void DeviceManagerSA_OnSystemSuspend(object? sender, EventArgs e)
        {
            //Debug.WriteLine("DeviceManagerSA_OnSystemSuspend");
            DdpmCommonHelper.WriteUILog($"catch event DeviceManagerSA_OnSystemSuspend");

            if (_vm.IsRecording)
                Dispatcher.Invoke(new Action(() =>
                {
                    UserStopRecord();
                }));
        }

        private void DeviceManagerSA_OnSystemResume(object? sender, EventArgs e)
        {
            DdpmCommonHelper.WriteUILog($"catch event DeviceManagerSA_OnSystemResume");
            //Debug.WriteLine("DeviceManagerSA_OnSystemResume");
        }

        private void DeviceManagerSA_OnSystemSessionEnd(object? sender, EventArgs e)
        {
            DdpmCommonHelper.WriteUILog($"catch event DeviceManagerSA_OnSystemSessionEnd");
            if (_vm.IsRecording)
                Dispatcher.Invoke(new Action(() =>
                {
                    UserStopRecord();
                }));
        }

        bool in_CameraPlugin = true;
        private async void LaunchView_Unloaded(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("LaunchView_Unloaded start");

            FreeWebcamResource();
            if (AlertTimer != null)
            {
                AlertTimer.Stop();
                AlertTimer.Tick -= AlertTimer_Tick;
            }

            Console.WriteLine("LaunchView_Unloaded end");
        }

        private bool _resourcesReleased = false;

        private void ReleaseResources()
        {
            if (_resourcesReleased)
            {
                return;
            }

            if (DdpmCommonHelper.DeviceManagerSA != null)
            {
                DdpmCommonHelper.DeviceManagerSA.ITSettingsActionEvent -= DeviceManagerSA_ITSettingsActionEvent;
                DdpmCommonHelper.DeviceManagerSA.SystemSuspend -= DeviceManagerSA_OnSystemSuspend;
                DdpmCommonHelper.DeviceManagerSA.SystemResume -= DeviceManagerSA_OnSystemResume;
                //DdpmCommonHelper.DeviceManagerSA.DeviceChanged -= DeviceManagerSA_DeviceChanged;
                DdpmCommonHelper.DeviceManagerSA.SystemSessionEnd -= DeviceManagerSA_OnSystemSessionEnd;
            }

            _resourcesReleased = true;
        }

        private async void FreeWebcamResource()
        {
            // Unexpected closed recording.
            if (_vm.IsRecording)
            {
                DdpmCommonHelper.WriteUILog($"[FreeWebcamResource] force stop recording.");
                StopRecord();
            }

            try
            {
                DdpmCommonHelper.WriteUILog($"[FreeWebcamResource] Free PowerEventControl");

                if (_pwr_Mon != null)
                {
                    _pwr_Mon.UnRegisterAllHotKey();

                    _pwr_Mon.MonitorTurnedOn -= MonitorEvent_On;
                    _pwr_Mon.Close_Event();
                    //_pwr_Mon = null;
                }
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog("[FreeWebcamResource] PowerEvent Control got exception: " + ex.Message);
            }


            try
            {
                in_CameraPlugin = false;
                exit_status_thread = true;
                _vm?.mre.Set();
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog($"[FreeWebcamResource] got exception 1:{ex.ToString()}");
            }

            try
            {
                if (DdpmCommonHelper.DeviceManagerSA != null)
                {
                    DdpmCommonHelper.DeviceManagerSA.ITSettingsActionEvent -= DeviceManagerSA_ITSettingsActionEvent;
                    DdpmCommonHelper.DeviceManagerSA.SystemSuspend -= DeviceManagerSA_OnSystemSuspend;
                    DdpmCommonHelper.DeviceManagerSA.SystemResume -= DeviceManagerSA_OnSystemResume;
                    //DdpmCommonHelper.DeviceManagerSA.DeviceChanged -= DeviceManagerSA_DeviceChanged;
                    DdpmCommonHelper.DeviceManagerSA.SystemSessionEnd -= DeviceManagerSA_OnSystemSessionEnd;
                }
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog($"[FreeWebcamResource] got exception 2:{ex.ToString()}");
            }


            try
            {
                if (_vm?.MediaFrameReader != null)
                    _vm.MediaFrameReader.FrameArrived -= MediaFrameReader_FrameArrived;
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog("[FreeWebcamResource] Free MediaFrameReader got exception: " + ex.Message);
            }

            try
            {
                if (_vm != null)
                {
                    _vm.ProfilePropertyChanged -= ProfilePropertyChanged;
                    _vm.WebcamSettingChanged -= WebcamSettingChanged;
                }
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog($"[FreeWebcamResource] got exception 4:{ex.ToString()}");
            }

            try
            {
                await CleanupMediaCaptureAsync();
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog("[FreeWebcamResource] DDPM.UI.WebCameraPlugin\\Views\\LaunchView.xaml.cs  FreeWebcamResource()  CleanupMediaCaptureAsync() ex 2: " + ex.Message);
            }

            //try
            //{
            //    await CleanupMediaCaptureAsync();

            //    if (_pwr_Mon != null)
            //    {
            //        _pwr_Mon.MonitorTurnedOn -= MonitorEvent_On;
            //        _pwr_Mon.Close_Event();
            //        _pwr_Mon = null;
            //    }
            //}
            //catch (Exception ex)
            //{
            //    DdpmCommonHelper.WriteUILog("DDPM.UI.WebCameraPlugin\\Views\\LaunchView.xaml.cs  LaunchView_Unloaded() ex 2: " + ex.Message);
            //}
            DdpmCommonHelper.WriteUILog("[FreeWebcamResource] Webcam LaunchView_Unloaded end");
            /*if ( _vm?.close_app == true )
            {
                Console.WriteLine("force exit");
                Environment.Exit(0);
            }*/
            //await _vm.CleanupMediaCapture();
        }

        #region Init for Modules

        /// <summary>
        /// Base on specified monitor's capabiliies to build the Vbar items, and headers/modules
        /// </summary>
        private void BuildModuleGroups()
        {
            List<ModuleGroup> groups = new();

            moduleGroup = new ModuleGroup()
            {
                GroupName = CameraControl,
                GroupIcon = DdpmCommonHelper.GetImageSourceFromCommonResource("Resources/Images/CameraControl.png", "DDPM.UI.Resources"),
                GroupIconCanvas = DdpmCommonHelper.CanvasIconCreator(VbarIcon.WebcamControl)
            };
            moduleGroup.AddHeader(CameraControl, new WebCameraSettingsModule(_vm));
            groups.Add(moduleGroup);

            moduleGroup = new ModuleGroup()
            {
                GroupName = ColorandImage,
                GroupIcon = DdpmCommonHelper.GetImageSourceFromCommonResource("Resources/Images/CameraColorImage.png", "DDPM.UI.Resources"),
                GroupIconCanvas = DdpmCommonHelper.CanvasIconCreator(VbarIcon.WebcamColorImg)
            };
            moduleGroup.AddHeader(ColorandImage, new WebCameraColorImageModule(_vm));
            groups.Add(moduleGroup);


            if (_vm.Model == "WB7022" || _vm.Model == "P2424HEB" || _vm.Model == "P2724DEB" || _vm.Model == "P3424WEB" || _vm.Model == "U3223QZ" || _vm.Model == "U3224KB" || _vm.Model == "U3224KBA"
                || _vm.Model == "P2426HEB" || _vm.Model == "P2726DEB" || _vm.Model == "P3426WEB")
            {
                //bool blRet = true;

                //blRet = CheckPresenceDetection_UI();

                //replace with leo check_PresenceFunction() 2024/12/05
                //GetPresenceDetectionView();

                moduleGroup = new ModuleGroup()
                {
                    GroupName = PresenceDetection,
                    GroupIcon = DdpmCommonHelper.GetImageSourceFromCommonResource("Resources/Images/CameraPresenceDetection.png", "DDPM.UI.Resources"),
                    GroupIconCanvas = DdpmCommonHelper.CanvasIconCreator(VbarIcon.WebcamDetection)
                };
                moduleGroup.AddHeader(PresenceDetection, new WebCameraPresenceDetectionModule(_vm));

                bool addPresenceDetection = true;
                if (!AllSupportedResolutions)
                {
                    //規格確認後,可能會再增加需要排除型號
                    if (_vm.Model == "WB7022")
                        addPresenceDetection = false;
                }

                if (noPresenceFunction)
                    addPresenceDetection = false;

                if (addPresenceDetection)
                    groups.Add(moduleGroup);

            }

            moduleGroup = new ModuleGroup()
            {
                GroupName = Capture,
                GroupIcon = DdpmCommonHelper.GetImageSourceFromCommonResource("Resources/Images/CameraCapture.png", "DDPM.UI.Resources"),
                GroupIconCanvas = DdpmCommonHelper.CanvasIconCreator(VbarIcon.WebcamCapture)
            };
            moduleGroup.AddHeader(Capture, new WebCameraCaptureModule(_vm));
            groups.Add(moduleGroup);

            if (_vm.MicList.Contains(_vm.Model))
            {
                moduleGroup = new ModuleGroup()
                {
                    GroupName = Microphone,
                    GroupIcon = DdpmCommonHelper.GetImageSourceFromCommonResource("Resources/Images/Microphone.png", "DDPM.UI.Resources"),
                    GroupIconCanvas = DdpmCommonHelper.CanvasIconCreator(VbarIcon.WebcamMicrophone)
                };
                moduleGroup.AddHeader(Microphone, new WebCameraMicrophoneModule(_vm));
                groups.Add(moduleGroup);
            }

            _vm.ModuleGroups = groups;
        }

        #endregion Init for Modules

        #region Vbar

        private void OnVbarItemClicked(VbarItem1 newItem)
        {
            if (newItem.Id == _vm.VbarSelectedIndex)
            { return; }

            UpdatePVMargin(0);

            if (_rightFrameWidth[newItem.Id + 1] != _rightFrameWidth[_vm.VbarSelectedIndex + 1])
            {
                _vm.RightFrameWidthFrom = _rightFrameWidth[_vm.VbarSelectedIndex + 1];
                _vm.RightFrameWidthTo = _rightFrameWidth[newItem.Id + 1];

                InvokeGotoTwoViewModeAnimation();
            }

            _vm.VbarSelectedIndex = newItem.Id;

            if (_vm.RightViewHeaders != null)
            {
                rightViewHeaderCtrl.SetHeaders(_vm.RightViewHeaders.ToArray());
            }
            _vm.SetLadningMode(false);
            _vm.SelectVBar();

            //if (newItem.Text == LangHelper.Instance["Camera.4"])
            //{
            //    _ = CleanupMediaCaptureAsync();
            //    imgDevice.Visibility = Visibility.Visible;
            //    gridPreview.Visibility = Visibility.Hidden;
            //}
            //else
            //{
            //    Preview();
            //    imgDevice.Visibility = Visibility.Hidden;
            //    gridPreview.Visibility = Visibility.Visible;
            //}
            if (IsPresetOpen)
            { btnPreset_Click(this, null); }
        }

        #endregion Vbar

        #region RightViewHeader

        private void RightViewHeaderCtrl_SelectionChanged(object sender, RoutedEventArgs e)
        {
            if (sender == null)
                return;

            //int newSelId = rightViewHeaderCtrl.SelectedIndex;
            //if (newSelId != displaySettingsSelIdx) {
            //  if (_vm != null) {
            //    _vm.RightViewHeaderSelectedIndex = newSelId;
            //  }
            //  displaySettingsSelIdx = newSelId;
            //  SwitchLeftRightView();
            //}
        }

        #endregion RightViewHeader

        #region Mode Change

        private void InvokeGotoTwoViewModeAnimation()
        {
            Dispatcher.Invoke(new Action(() =>
            {
                Storyboard sb = (Storyboard)this.FindResource("StoryGotoTwoView");
                if (sb != null)
                {
                    sb.Completed += (o, s) =>
                    {
                    };

                    sb.Begin();
                }
            }));
        }

        #endregion Mode Change

        private void Mainframe_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (_vm.VbarSelectedIndex == -1)
            { return; }

            UpdatePVMargin(-1);

            _vm.RightFrameWidthTo = 0;
            _vm.RightFrameWidthFrom = _rightFrameWidth[_vm.VbarSelectedIndex + 1];
            InvokeGotoTwoViewModeAnimation();

            _vm.VbarSelectedIndex = -1;
            _vm.SetLadningMode(true);
            _vm.SelectVBar();

            if (IsPresetOpen)
            { btnPreset_Click(this, null); }
        }

        private MediaCaptureFailedEventHandler handler = (sender, e) =>
        {
            System.Threading.Tasks.Task task = System.Threading.Tasks.Task.Run(async () =>
            {
                await new MessageDialog("There was an error capturing the video from camera.", "Error").ShowAsync();
            });
        };


        WriteableBitmap writeableBitmap;
        SoftwareBitmap backBuffer;
        Int32Rect react;
        int before_width = 0;
        int before_height = 0;

        [ComImport]
        [Guid("5B0D3235-4DBA-4D44-865E-8F1D0E4FD04D")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        unsafe interface IMemoryBufferByteAccess
        {
            void GetBuffer(out byte* buffer, out uint capacity);
        }
        //[DllImport("Kernel32.dll", EntryPoint = "RtlMoveMemory")]
        //public static extern void CopyMemory(IntPtr Destination, IntPtr Source, int Length);
        int ImageBufferSize = 0;
        int count = 0;
        private async void MediaFrameReader_FrameArrived(MediaFrameReader sender, MediaFrameArrivedEventArgs args)
        {
            if (_running)
                return;
            _running = true;

            SoftwareBitmap softwareBitmap = null;

            try
            {
                var latestFrame = sender.TryAcquireLatestFrame();
                if (latestFrame != null)
                {
                    var videoMediaFrame = latestFrame.VideoMediaFrame;
                    if (videoMediaFrame != null)
                    {
                        softwareBitmap = videoMediaFrame.SoftwareBitmap;
                    }
                }
            }
            catch (Exception ex)
            {

                // Log the exception details for further analysis
                DdpmCommonHelper.WriteUILog($"Exception occurred: {ex.Message}");
                _running = false;
                return;
            }


            Task.Delay(60).Wait();////Thread.Sleep(60);

            try
            {

                if (softwareBitmap != null && (_vm.running_state || _vm.IsRecording))
                {
                    _ = CameraImage.Dispatcher.BeginInvoke(() =>
                    {

                        if (before_width != softwareBitmap.PixelWidth || before_height != softwareBitmap.PixelHeight)
                        {

                            writeableBitmap = new(
                                softwareBitmap.PixelWidth,
                                softwareBitmap.PixelHeight,
                                96,
                                96,
                                PixelFormats.Bgra32,
                                null);
                            react = new Int32Rect(0, 0, writeableBitmap.PixelWidth, writeableBitmap.PixelHeight);
                            ImageBufferSize = writeableBitmap.PixelWidth * writeableBitmap.PixelHeight * 4;
                            CameraImage.Source = writeableBitmap;
                            before_width = softwareBitmap.PixelWidth;
                            before_height = softwareBitmap.PixelHeight;
                        }

                        writeableBitmap.Lock();
                        using var m = softwareBitmap.LockBuffer(BitmapBufferAccessMode.Read);
                        using var reference = m.CreateReference();
                        var t = m.GetPlaneDescription(0);
                        unsafe
                        {
                            (reference.As<IMemoryBufferByteAccess>()).GetBuffer(out var ptr, out var capacity);

                            //way 1:
                            writeableBitmap.WritePixels(
                                react,
                                (IntPtr)ptr,
                                (int)capacity,
                                t.Stride);

                            //way 2:
                            /*CopyMemory(writeableBitmap.BackBuffer, (IntPtr)ptr, ImageBufferSize);
                            writeableBitmap.AddDirtyRect(react);*/
                        }
                        writeableBitmap.Unlock();
                    });
                }
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog($"MediaFrameReader_FrameArrived Exception occurred 2 : {ex.Message}");
            }

            _running = false;
        }

        /// <summary>
        /// MediaFrameReader FrameArrived event
        /// </summary>
        /*private void MediaFrameReader_FrameArrived_org(MediaFrameReader sender, MediaFrameArrivedEventArgs args)
        {
            using var latestFrameReference = sender.TryAcquireLatestFrame();

            var videoMediaFrame = latestFrameReference?.VideoMediaFrame;
            var softwareBitmap = videoMediaFrame?.SoftwareBitmap;

            if (softwareBitmap != null)
            {
                if (softwareBitmap.BitmapPixelFormat != BitmapPixelFormat.Bgra8 ||
                    softwareBitmap.BitmapAlphaMode != BitmapAlphaMode.Premultiplied)
                {
                    softwareBitmap = SoftwareBitmap.Convert(softwareBitmap, BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);
                }

                // Swap the processed frame to backBuffer and dispose of the unused image.
                softwareBitmap = Interlocked.Exchange(ref backBitmapBuffer, softwareBitmap);
                softwareBitmap?.Dispose();

                CameraImage.Dispatcher.BeginInvoke(async () =>
                {
                    if (_running)
                        return;
                    _running = true;

                    // Keep draining frames from the backbuffer until the backbuffer is empty.
                    SoftwareBitmap latestBitmap;
                    while ((latestBitmap = Interlocked.Exchange(ref backBitmapBuffer, null)) != null)
                    {
                        CameraImage.Source = await ConvertSoftwareBitmap2BitmapImage(latestBitmap);
                        //var imageSource = (SoftwareBitmapSource)CameraImage.Source;
                        //await imageSource.SetBitmapAsync(latestBitmap);
                        latestBitmap.Dispose();
                    }

                    //CameraImage.Source = await ConvertSoftwareBitmap2BitmapImage(softwareBitmap);
                    _running = false;
                });


            }

            if (latestFrameReference != null)
            {
                latestFrameReference.Dispose();
            }

        }*/

        /// <summary>
        /// Convert SoftwareBitmap to BitmapImage
        /// </summary>
        /// <param name="src">SoftwareBitmap</param>
        /// <returns> result</returns>
        private static async Task<BitmapImage> ConvertSoftwareBitmap2BitmapImage(SoftwareBitmap src)
        {
            using var stream = new Windows.Storage.Streams.InMemoryRandomAccessStream();
            var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.BmpEncoderId, stream);
            encoder.SetSoftwareBitmap(src);
            await encoder.FlushAsync();

            var result = new BitmapImage();
            result.BeginInit();
            result.StreamSource = stream.AsStream();
            result.CacheOption = BitmapCacheOption.OnLoad;
            result.EndInit();
            result.Freeze();

            return result;
        }

        // private void StartRecord()
        private async void StartRecord()
        {
            DdpmCommonHelper.WriteUILog($"StartRecord");


            _vm.IsMicEnumerationOnEnabled = false;

            if (_vm.WebcamCountdown)
            {
                _countdownValue = 3; // 設置倒數起始值
                CountdownText.Text = _countdownValue.ToString();
                CountDownBox.Visibility = Visibility.Visible;
                _timer.Start();
                //DdpmCommonHelper.DeviceManagerSA?.ShowOSD(Screen.PrimaryScreen!.DeviceName, OSDType.StartRecording);
            }
            else
            {
                //leo fixed 2024/01/10
                //Avoid unnecessary expectation warnings.
                //StartRecordingAsync().RunSynchronously();
                await StartRecordingAsync();
            }

        }

        private void StopRecord()
        {
            if (_vm.IsRecording)
            {
                _ = StopRecordingAsync();
            }

        }
        private void Timer_Tick(object? sender, EventArgs e)
        {
            _countdownValue--;
            if (_countdownValue > 0)
            {
                CountdownText.Text = _countdownValue.ToString();
            }
            else
            {
                _timer.Stop();
                CountDownBox.Visibility = Visibility.Collapsed;
                StartRecordingAsync().RunSynchronously();
            }
        }
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        private static extern bool GetDiskFreeSpaceEx(string lpDirectoryName, out ulong lpFreeBytesAvailable, out ulong lpTotalNumberOfBytes, out ulong lpTotalNumberOfFreeBytes);
        private static bool _GetDiskFreeSpaceEx(string lpDirectoryName, out ulong lpFreeBytesAvailable, out ulong lpTotalNumberOfBytes, out ulong lpTotalNumberOfFreeBytes)
        {
            bool rst = GetDiskFreeSpaceEx(lpDirectoryName, out lpFreeBytesAvailable, out lpTotalNumberOfBytes, out lpTotalNumberOfFreeBytes);

            if (!rst)
            {
                DdpmCommonHelper.WriteUILog("[LaunchView] GetDiskFreeSpaceEx failed.");

#if DEBUG
                Console.WriteLine("[LaunchView] GetDiskFreeSpaceEx failed.");
#endif
            }

            return rst;
        }

        public static bool HasEnoughSpace(string path, ulong requiredBytes)
        {
            bool ret = _GetDiskFreeSpaceEx(path, out ulong freeBytesAvailable, out _, out _);
            if (ret == false)
                return false;
            return freeBytesAvailable >= requiredBytes;
        }

        private LowLagMediaRecording _lowLag;
        /// <summary>
        /// Records an MP4 video to a StorageFile and adds rotation metadata to it
        /// </summary>
        /// <returns></returns>
        private async Task StartRecordingAsync()
        {
            try
            {
                _vm.IsRecording = true;
                //var picturesLibrary = await StorageLibrary.GetLibraryAsync(KnownLibraryId.Pictures);
                // Fall back to the local app storage if the Pictures Library is not available
                //_vm._captureFolder = picturesLibrary.SaveFolder ?? ApplicationData.Current.LocalFolder;
                var captureFolder = await StorageFolder.GetFolderFromPathAsync(_vm.VideoCaptureFolder);

                // Create storage file for the capture
                var videoFile = await captureFolder.CreateFileAsync(DateTime.Now.ToString("'DDPMVideo'yyyy-MM-dd-HH-mm-ss.'mp4'"), CreationCollisionOption.GenerateUniqueName);
                VideoEncodingQuality VideoEncoding = VideoEncodingQuality.Auto;
                switch (_vm.WebcamSettings.SelectedResolution)
                {
                    case "HD":
                        VideoEncoding = VideoEncodingQuality.HD720p;
                        break;
                    case "Full HD":
                        VideoEncoding = VideoEncodingQuality.HD1080p;
                        break;
                    case "2K QHD":
                        VideoEncoding = VideoEncodingQuality.Uhd2160p;
                        break;
                    case "4K UHD":
                        VideoEncoding = VideoEncodingQuality.Uhd4320p;
                        break;
                    default:
                        VideoEncoding = VideoEncodingQuality.Auto;
                        break;
                }
                var encodingProfile = MediaEncodingProfile.CreateMp4(VideoEncoding);

                if (VideoEncoding != VideoEncodingQuality.Auto)
                {
                    switch (_vm.WebcamSettings.SelectedResolution)
                    {
                        case "HD":
                            VideoEncoding = VideoEncodingQuality.HD720p;
                            encodingProfile.Video.Width = 1280;
                            encodingProfile.Video.Height = 720;
                            break;
                        case "Full HD":
                            VideoEncoding = VideoEncodingQuality.HD1080p;
                            encodingProfile.Video.Width = 1920;
                            encodingProfile.Video.Height = 1080;
                            break;
                        case "2K QHD":
                            VideoEncoding = VideoEncodingQuality.Uhd2160p;
                            encodingProfile.Video.Width = 2560;
                            encodingProfile.Video.Height = 1440;
                            break;
                        case "4K UHD":
                            VideoEncoding = VideoEncodingQuality.Uhd4320p;
                            encodingProfile.Video.Width = 3840;
                            encodingProfile.Video.Height = 2160;
                            break;
                        default:
                            break;
                    }
                    //encodingProfile.Video.Bitrate = 1500000; // 降低影片位元率為 1.5 Mbps
                    //encodingProfile.Audio.Bitrate = 96000;  // 設定音訊位元率為 96 kbps
                    DdpmCommonHelper.WriteUILog($"_vm.WebcamSettings.SelectedcurrentFPS:{_vm.WebcamSettings.SelectedcurrentFPS}");
                    encodingProfile.Video.FrameRate.Numerator = uint.TryParse(_vm.WebcamSettings.SelectedcurrentFPS, out var Fps) ? Fps : 30; // 設置新的 FPS 分子，例如 60
                    encodingProfile.Video.FrameRate.Denominator = 1; // 分母，通常設為 1
                }

                //// Calculate rotation angle, taking mirroring into account if necessary
                //var rotationAngle = 360 - ConvertDeviceOrientationToDegrees(GetCameraOrientation());
                //encodingProfile.Video.Properties.Add(RotationKey, PropertyValue.CreateInt32(rotationAngle));

                DdpmCommonHelper.WriteUILog("Starting recording to " + videoFile.Path);

                if (_vm.MediaCapture != null)
                {
                    //await _vm.MediaCapture.StartRecordToStorageFileAsync(encodingProfile, videoFile);
                    _lowLag = await _vm.MediaCapture.PrepareLowLagRecordToStorageFileAsync(encodingProfile, videoFile);
                    await _lowLag.StartAsync();
                    stopwatch.Start();
                    RecordingTimer.Start();
                }
                DdpmCommonHelper.WriteUILog("Started recording!");
            }
            catch (Exception ex)
            {
                if (_vm.IsRecording)
                    _vm.IsRecording = false;
                DdpmCommonHelper.WriteUILog("DDPM.UI.WebCameraPlugin\\Views\\LaunchView.xaml.cs StartRecordingAsync() : " + ex.Message);
                // File I/O errors are reported as exceptions
                // Debug.WriteLine("Exception when starting video recording: " + ex.ToString());
            }
        }

        /// <summary>
        /// Stops recording a video
        /// </summary>
        /// <returns></returns>
        private async Task StopRecordingAsync()
        {
            DdpmCommonHelper.WriteUILog("Stopping recording...");

            _vm.IsMicEnumerationOnEnabled = true;

            if (_vm.MediaCapture != null)
            {
                try
                {
                    //await _vm.MediaCapture.StopRecordAsync();
                    await _lowLag?.StopAsync();
                    await _lowLag?.FinishAsync();
                }
                catch (Exception ex)
                {
                    DdpmCommonHelper.WriteUILog("DDPM.UI.WebCameraPlugin\\Views\\LaunchView.xaml.cs StopRecordingAsync() : " + ex.Message);
                }
            }
            _vm.IsRecording = false;

            DdpmCommonHelper.WriteUILog("Stopped recording!");
        }

        /// <summary>
        /// Resume recording a video
        /// </summary>
        /// <returns></returns>
        private async void ResumeRecordingAsync()
        {
            DdpmCommonHelper.WriteUILog("Resuming recording...");
            try
            {
                if (_vm.MediaCapture != null)
                    await _vm.MediaCapture.ResumeRecordAsync();
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog("DDPM.UI.WebCameraPlugin\\Views\\LaunchView.xaml.cs  ResumeRecordingAsync() ex: " + ex.Message);
            }
            DdpmCommonHelper.WriteUILog("Resume recording!");
        }

        /// <summary>
        /// Pause recording a video
        /// </summary>
        /// <returns></returns>
        private async void PauseRecordingAsync()
        {
            DdpmCommonHelper.WriteUILog("Pausing recording...");
            if (_vm.MediaCapture != null)
            {
                _ = await _vm.MediaCapture.PauseRecordWithResultAsync(Windows.Media.Devices.MediaCapturePauseBehavior.RetainHardwareResources);
            }
            DdpmCommonHelper.WriteUILog("Pause recording!");
        }

        /// <summary>
        /// Converts the given orientation of the device in space to the corresponding rotation in degrees
        /// </summary>
        /// <param name="orientation">The orientation of the device in space</param>
        /// <returns>An orientation in degrees</returns>
        private static int ConvertDeviceOrientationToDegrees(SimpleOrientation orientation)
        {
            switch (orientation)
            {
                case SimpleOrientation.Rotated90DegreesCounterclockwise:
                    return 90;

                case SimpleOrientation.Rotated180DegreesCounterclockwise:
                    return 180;

                case SimpleOrientation.Rotated270DegreesCounterclockwise:
                    return 270;

                case SimpleOrientation.NotRotated:
                default:
                    return 0;
            }
        }

        /// <summary>
        /// Calculates the current camera orientation from the device orientation by taking into account whether the camera is external or facing the user
        /// </summary>
        /// <returns>The camera orientation in space, with an inverted rotation in the case the camera is mounted on the device and is facing the user</returns>
        private SimpleOrientation GetCameraOrientation()
        {
            // Cameras that are not attached to the device do not rotate along with it, so apply no rotation
            return SimpleOrientation.NotRotated;
        }

        // 20240626 jim add
        private async Task CleanupMediaCaptureAsync()
        {

            if (_vm.MediaFrameReader != null)
            {
                try
                {
                    _vm.MediaFrameReader.FrameArrived -= MediaFrameReader_FrameArrived;
                }
                catch (Exception ex)
                {
                    DdpmCommonHelper.WriteUILog($"Error del MediaFrameReader FrameArrived: {ex.Message}");
                }

                try
                {
                    await _vm.MediaFrameReader.StopAsync();

                }
                catch (Exception ex)
                {
                    DdpmCommonHelper.WriteUILog($"Error stopping MediaFrameReader: {ex.Message}");
                }

                try
                {
                    if (_vm.MediaFrameReader != null)
                    {
                        _vm.MediaFrameReader.Dispose();
                        _vm.MediaFrameReader = null;
                    }
                }
                catch (Exception ex)
                {
                    DdpmCommonHelper.WriteUILog($"Error Dispose MediaFrameReader: {ex.Message}");
                }
            }

            if (_vm.MediaCapture != null)
            {
                _vm.MediaCapture.Dispose();
                _vm.MediaCapture = null;
            }
        }

        private void btnRecord_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            DdpmCommonHelper.WriteUILog("DDPM.UI.WebCameraPlugin\\Views\\LaunchView.xaml.cs btnRecord_Click() start");
            try
            {
                bool ret = _GetDiskFreeSpaceEx(_vm.VideoCaptureFolder, out ulong freeBytesAvailable, out _, out _);
                if (ret)
                {
                    DdpmCommonHelper.WriteUILog("btnRecord_Click:  DISK Free " + freeBytesAvailable / (1024 * 1024) + "MB");
                }

                PerformanceCounter ramCounter = new PerformanceCounter("Memory", "Available MBytes");
                DdpmCommonHelper.WriteUILog("btnRecord_Click:  Mem Free " + ramCounter.NextValue() + "MB");

                if (!HasEnoughSpace(_vm.VideoCaptureFolder, 20 * 1024 * 1024))
                {
                    return;
                }
                btnPause.Visibility = Visibility.Visible;
                btnRecord.Visibility = Visibility.Collapsed;
                btnStop.Visibility = Visibility.Visible;
                txtTimer.Visibility = Visibility.Visible;
                if (IsPresetOpen)
                { btnPreset_Click(this, null); }
                btnPreset.IsEnabled = false;

                StartRecord();

                //Derek 2025/02/20 for PIMS 338464
                _vm?.SaveWALSettings();
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog("DDPM.UI.WebCameraPlugin\\Views\\LaunchView.xaml.cs btnRecord_Click() ex:" + ex.Message);
            }
            DdpmCommonHelper.WriteUILog("DDPM.UI.WebCameraPlugin\\Views\\LaunchView.xaml.cs btnRecord_Click() end");
        }

        private void btnStop_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            UserStopRecord();
        }

        private void UserStopRecord()
        {
            DdpmCommonHelper.WriteUILog("DDPM.UI.WebCameraPlugin\\Views\\LaunchView.xaml.cs UserStopRecord() start");

            try
            {
                StopRecord();

                //Derek 2025/02/20 for PIMS 338464
                _vm?.RecoverWALSettings();

                RecordingTimer.Stop();
                stopwatch.Stop();
                stopwatch.Reset();
                btnPause.Visibility = Visibility.Collapsed;
                btnRecord.Visibility = Visibility.Visible;
                btnStop.Visibility = Visibility.Collapsed;
                btnPlay.Visibility = Visibility.Collapsed;
                txtTimer.Visibility = Visibility.Collapsed;
                txtTimer.Text = "00:00:00";
                btnPreset.IsEnabled = true;
                if (_vm.WebcamCountdown)
                {
                    CountDownBox.Visibility = Visibility.Collapsed;
                    _timer.Stop();
                }
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog("DDPM.UI.WebCameraPlugin\\Views\\LaunchView.xaml.cs UserStopRecord() ex:" + ex.Message);
            }
            DdpmCommonHelper.WriteUILog("DDPM.UI.WebCameraPlugin\\Views\\LaunchView.xaml.cs UserStopRecord() end");

        }

        private void ProfileSelected(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            DdpmCommonHelper.WriteUILog("DDPM.UI.WebCameraPlugin\\Views\\LaunchView.xaml.cs ProfileSelected() start");
            try
            {
                var profileName = ((UXTextBlock)sender).Tag.ToString()!;
                if (profileName != _vm.CurrentProfileName)
                {
                    //DdpmCommonHelper.DeviceManagerSA?.SetProfile(_vm.CurrentDeviceInfo!.ID.ToString(), _vm.ProfileIDs[profileName]);
                    _vm.CurrentProfileName = profileName;
                    isProfilePropertyChanged = false;

                    _vm.AlertType = WebcamAlert.Alert1;
                    _vm.AlertVisibility = Visibility.Visible;

                    //new Thread(() =>
                    //{
                    //    Thread.Sleep(3000);
                    //    _vm.AlertVisibility = Visibility.Collapsed;
                    //}).Start();
                    Task.Run(async () =>
                    {
                        await Task.Delay(3000);
                        _vm.AlertVisibility = Visibility.Collapsed;
                    });

                    _vm.SetProfile();

                    //Derek 2025/02/12 force preview refresh to apply changed settings when change profile
                    Preview();

                    //Derek 1212
                    DdpmCommonHelper.DeviceManagerSA?.SyncWebcamProfile(_vm.CurrentProfileName, false);
                }
                btnPreset_Click(this, null);
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog("DDPM.UI.WebCameraPlugin\\Views\\LaunchView.xaml.cs ProfileSelected() ex:" + ex.Message);
            }
            DdpmCommonHelper.WriteUILog("DDPM.UI.WebCameraPlugin\\Views\\LaunchView.xaml.cs ProfileSelected() end");
        }

        private void btnPreset_Click(object sender, System.Windows.Input.MouseButtonEventArgs? e)
        {
            DdpmCommonHelper.WriteUILog("DDPM.UI.WebCameraPlugin\\Views\\LaunchView.xaml.cs btnPreset_Click() start");
            try
            {
                var img = (UIElement)FindName($"imgDown");
                DoubleAnimation rotateAnimation;
                var AnimatedPanel = (StackPanel)FindName("spPresets");
                if (IsPresetOpen)
                {
                    if (_vm.CurrentProfileName == "NONE")
                    {
                        txtPreset.Text = $"{Strings.Preset}: {LangHelper.Instance["None"]}";
                    }
                    else
                    {
                        var pName = _vm.ProfileCaptions[_vm.CurrentProfileName];
                        if (PresetNames.Contains(pName))
                        {
                            txtPreset.Text = $"{Strings.Preset}: {pName}";
                        }
                        else
                        {
                            txtPreset.Text = Utility.CheckTextLength($"{_vm.CurrentProfileName}", 140, 14);
                        }
                    }

                    //var pName = _vm.CurrentProfileName == "NONE" ? "NONE" : _vm.ProfileCaptions[_vm.CurrentProfileName];
                    //var txt = $"{Strings.Preset}: {pName}";
                    //if (!PresetNames.Contains(pName)|| pName != "NONE")
                    //{
                    //    txt = Utility.CheckTextLength($"{_vm.CurrentProfileName}", 140, 14);
                    //}
                    //txtPreset.Text = txt;
                    rotateAnimation = new()
                    {
                        From = 180,
                        To = 0,
                        Duration = new Duration(TimeSpan.FromSeconds(0.3)),
                    };
                    AnimatedPanel.Visibility = Visibility.Collapsed;
                    //_vm?.OnUpdateIsHDROn();
                }
                else
                {
                    txtPreset.Text = LangHelper.Instance["Camera.6"];
                    rotateAnimation = new()
                    {
                        From = 0,
                        To = 180,
                        Duration = new Duration(TimeSpan.FromSeconds(0.3)),
                    };
                    AnimatedPanel.Visibility = Visibility.Visible;
                    DoubleAnimation visibilityAnimation = new()
                    {
                        From = 0,
                        To = 1,
                        Duration = new Duration(TimeSpan.FromSeconds(0.3))
                    };
                    AnimatedPanel.BeginAnimation(DockPanel.OpacityProperty, visibilityAnimation);
                }
                img.RenderTransform = new RotateTransform();
                img.RenderTransform.BeginAnimation(RotateTransform.AngleProperty, rotateAnimation);
                IsPresetOpen = !IsPresetOpen;
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog("DDPM.UI.WebCameraPlugin\\Views\\LaunchView.xaml.cs btnPreset_Click() ex:" + ex.Message);
            }
            DdpmCommonHelper.WriteUILog("DDPM.UI.WebCameraPlugin\\Views\\LaunchView.xaml.cs btnPreset_Click() end");
        }

        private void EditPreset(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            DdpmCommonHelper.WriteUILog("DDPM.UI.WebCameraPlugin\\Views\\LaunchView.xaml.cs EditPreset() start");
            try
            {
                var profileName = ((FrameworkElement)sender).Tag.ToString()!;
                EditMode = "EDIT";
                EditingProfileName = profileName;
                if (profileName != _vm.CurrentProfileName)
                {
                    _vm.CurrentProfileName = profileName;
                    _vm.SetProfile();
                }
                txbName.Text = profileName;
                _vm.DisableVBar();
                gdBattery.Visibility = Visibility.Collapsed;
                gdAddProfile.Visibility = Visibility.Visible;
                txtCaption.Text = Strings.EditPreset;
                _vm.TooltipVisibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog("DDPM.UI.WebCameraPlugin\\Views\\LaunchView.xaml.cs EditPreset() ex:" + ex.Message);
            }
            DdpmCommonHelper.WriteUILog("DDPM.UI.WebCameraPlugin\\Views\\LaunchView.xaml.cs EditPreset() end");
        }

        private void DeletePreset(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            DdpmCommonHelper.WriteUILog("DDPM.UI.WebCameraPlugin\\Views\\LaunchView.xaml.cs DeletePreset() start");
            try
            {
                var profileName = ((Image)sender).Tag.ToString()!;
                //DdpmCommonHelper.DeviceManagerSA.DeleteProfile(_vm.CurrentDeviceInfo!.ID.ToString(), _vm.WebcamSettings.CustomProfiles[profileName].Id);
                if (_vm.WebcamSettings.CustomProfiles.ContainsKey(profileName))
                {
                    _vm.WebcamSettings.CustomProfiles.Remove(profileName);
                    WebcamSettings.ExportWebcamSettings(_vm.WebcamSettings, _vm.Model, DdpmCommonHelper.DeviceManagerSA, DdpmCommonHelper.Log);
                    _vm.PrepareProfileItems();
                    ProfileItems.ItemsSource = null;
                    ProfileItems.ItemsSource = _vm.ProfileItems;
                }

                if (profileName == _vm.CurrentProfileName)
                {
                    _vm.CurrentProfileName = "Default";
                    _vm.SetProfile();
                }
                btnPreset_Click(this, null);

                //Derek 2025/01/18
                DdpmCommonHelper.DeviceManagerSA?.SyncWebcamProfile(_vm.CurrentProfileName, false);
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog("DDPM.UI.WebCameraPlugin\\Views\\LaunchView.xaml.cs DeletePreset() ex:" + ex.Message);
            }
            DdpmCommonHelper.WriteUILog("DDPM.UI.WebCameraPlugin\\Views\\LaunchView.xaml.cs DeletePreset() end");
        }

        private void btnPlay_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            DdpmCommonHelper.WriteUILog("DDPM.UI.WebCameraPlugin\\Views\\LaunchView.xaml.cs btnPlay_Click() start");
            try
            {
                ResumeRecordingAsync();
                stopwatch.Start();
                btnPause.Visibility = Visibility.Visible;
                btnPlay.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog("DDPM.UI.WebCameraPlugin\\Views\\LaunchView.xaml.cs btnPlay_Click() ex:" + ex.Message);
            }
            DdpmCommonHelper.WriteUILog("DDPM.UI.WebCameraPlugin\\Views\\LaunchView.xaml.cs btnPlay_Click() end");
        }

        private void btnPause_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            DdpmCommonHelper.WriteUILog("DDPM.UI.WebCameraPlugin\\Views\\LaunchView.xaml.cs btnPause_Click() start");
            try
            {
                PauseRecordingAsync();
                stopwatch.Stop();
                btnPause.Visibility = Visibility.Collapsed;
                btnPlay.Visibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog("DDPM.UI.WebCameraPlugin\\Views\\LaunchView.xaml.cs btnPause_Click() ex:" + ex.Message);
            }
            DdpmCommonHelper.WriteUILog("DDPM.UI.WebCameraPlugin\\Views\\LaunchView.xaml.cs btnPause_Click() end");
        }

        private void btnFolder_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Process.Start("explorer.exe", _vm.VideoCaptureFolder);
        }

        //private async Task InitializeCameraAsync()
        //{
        //    try
        //    {
        //        _vm.MediaCapture = new MediaCapture();

        //        // Find available video devices (cameras)
        //        var cameraDevices = await DeviceInformation.FindAllAsync(DeviceClass.VideoCapture);
        //        if (cameraDevices.Count == 0)
        //        {
        //            MessageBox.Show("No camera devices found.");
        //            return;
        //        }

        //        // Initialize with the first available camera
        //        var settings = new MediaCaptureInitializationSettings
        //        {
        //            VideoDeviceId = cameraDevices[0].Id // You can select specific camera by ID
        //        };
        //        await _vm.MediaCapture.InitializeAsync(settings);

        //        // Set the camera resolution
        //        //SetCameraResolution(1280, 720); // Desired resolution (e.g., 1280x720)

        //        // Start the preview
        //        await _mediaCapture.StartPreviewAsync();

        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show($"Error initializing camera: {ex.Message}");
        //    }
        //}

        private void ChangePan(object sender, MouseButtonEventArgs e)
        {
            DdpmCommonHelper.WriteUILog("DDPM.UI.WebCameraPlugin\\Views\\LaunchView.xaml.cs ChangePan() start");

            try
            {
                if (sender is Image img)
                {
                    var value = 0;
                    switch (img.Tag.ToString())
                    {
                        case "L":
                            if (_vm.CurrentProfile.Pan == _vm.CurrentDeviceInfo?.PanMin)
                                return;

                            value = _vm.CurrentProfile.Pan - _vm.CurrentDeviceInfo?.PanSteppingDelta ?? 1;
                            if (value < _vm.CurrentDeviceInfo?.PanMin)
                                value = _vm.CurrentDeviceInfo?.PanMin ?? 0;

                            _vm.SetPan(value);
                            break;
                        case "R":
                            if (_vm.CurrentProfile.Pan == _vm.CurrentDeviceInfo?.PanMax)
                                return;

                            value = _vm.CurrentProfile.Pan + _vm.CurrentDeviceInfo?.PanSteppingDelta ?? 1;
                            if (value > _vm.CurrentDeviceInfo?.PanMax)
                                value = _vm.CurrentDeviceInfo?.PanMax ?? 0;

                            _vm.SetPan(value);
                            break;
                        case "T":
                            if (_vm.CurrentProfile.Tilt == _vm.CurrentDeviceInfo?.TiltMax)
                                return;

                            value = _vm.CurrentProfile.Tilt + _vm.CurrentDeviceInfo?.TiltSteppingDelta ?? 1;
                            if (value > _vm.CurrentDeviceInfo?.TiltMax)
                                value = _vm.CurrentDeviceInfo?.TiltMax ?? 0;

                            _vm.SetTilt(value);
                            break;
                        case "D":
                            if (_vm.CurrentProfile.Tilt == _vm.CurrentDeviceInfo?.TiltMin)
                                return;

                            value = _vm.CurrentProfile.Tilt - _vm.CurrentDeviceInfo?.TiltSteppingDelta ?? 1;
                            if (value < _vm.CurrentDeviceInfo?.TiltMin)
                                value = _vm.CurrentDeviceInfo?.TiltMin ?? 0;

                            _vm.SetTilt(value);
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog("DDPM.UI.WebCameraPlugin\\Views\\LaunchView.xaml.cs ChangePan() ex:" + ex.Message);
            }
            DdpmCommonHelper.WriteUILog("DDPM.UI.WebCameraPlugin\\Views\\LaunchView.xaml.cs ChangePan() end");
        }

        private void AddPreset(object sender, MouseButtonEventArgs e)
        {
            DdpmCommonHelper.WriteUILog("DDPM.UI.WebCameraPlugin\\Views\\LaunchView.xaml.cs AddPreset() start");
            try
            {
                EditMode = "ADD";
                txtCaption.Text = LangHelper.Instance["Camera.5"];
                _vm.DisableVBar();
                gdBattery.Visibility = Visibility.Collapsed;
                gdAddProfile.Visibility = Visibility.Visible;
                txbName.Text = "";
                txbName.Focus();
                _vm.TooltipVisibility = Visibility.Visible;

                if (_vm.VbarSelectedIndex == -1)
                {
                    OnVbarItemClicked(_vm.VbarItems[0]);
                }
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog("DDPM.UI.WebCameraPlugin\\Views\\LaunchView.xaml.cs AddPreset() ex:" + ex.Message);
            }
            DdpmCommonHelper.WriteUILog("DDPM.UI.WebCameraPlugin\\Views\\LaunchView.xaml.cs AddPreset() end");
        }

        private void ShowAlert()
        {
            bdrAlert2.Visibility = Visibility.Visible;
            AlertTimer.Stop();
            AlertTimer.Start();
        }
        private void NameTextChanged(object sender, TextChangedEventArgs e)
        {
            DdpmCommonHelper.WriteUILog("DDPM.UI.WebCameraPlugin\\Views\\LaunchView.xaml.cs  NameTextChanged() start");
            if (txbName.Text.Length > 30)
            {
                ShowAlert();
                txbName.Text = txbName.Text.Substring(0, 30);
                txbName.CaretIndex = 30;
                return;
            }
            try
            {
                var txt = txbName.Text.Trim();
                if (string.IsNullOrEmpty(txt))
                {
                    btnSave.IsEnabled = false;
                    return;

                }

                if (_vm.ProfileCaptions.ContainsKey(txt) && txt != EditingProfileName)
                {
                    txtMsg.Visibility = Visibility.Visible;
                    bdrName.BorderBrush = new SolidColorBrush(Color.FromArgb(0xFF, 0xFF, 0x3E, 0x3B));
                    btnSave.IsEnabled = false;
                }
                else
                {
                    txtMsg.Visibility = Visibility.Hidden;
                    bdrName.BorderBrush = new SolidColorBrush(Color.FromArgb(0xFF, 0x7E, 0x7E, 0x7E));
                    btnSave.IsEnabled = true;
                }
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog("DDPM.UI.WebCameraPlugin\\Views\\LaunchView.xaml.cs  NameTextChanged() ex:" + ex.Message);
            }
            DdpmCommonHelper.WriteUILog("DDPM.UI.WebCameraPlugin\\Views\\LaunchView.xaml.cs  NameTextChanged() end");
        }

        private void CancelClick(object sender, RoutedEventArgs e)
        {
            DdpmCommonHelper.WriteUILog("DDPM.UI.WebCameraPlugin\\Views\\LaunchView.xaml.cs  CancelClick() start");
            try
            {
                gdBattery.Visibility = Visibility.Visible;
                gdAddProfile.Visibility = Visibility.Collapsed;
                btnPreset_Click(this, null);
                if (EditMode == "EDIT")
                {
                    _vm.CurrentProfileName = EditingProfileName;
                    _vm.SetProfile();
                }
                txtCaption.Text = _vm.Name;
                _vm.EnableVBar();
                _vm.TooltipVisibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog("DDPM.UI.WebCameraPlugin\\Views\\LaunchView.xaml.cs  CancelClick() ex: " + ex.Message);
            }
            DdpmCommonHelper.WriteUILog("DDPM.UI.WebCameraPlugin\\Views\\LaunchView.xaml.cs  CancelClick() end");
        }

        private void SaveClick(object sender, RoutedEventArgs e)
        {
            DdpmCommonHelper.WriteUILog("DDPM.UI.WebCameraPlugin\\Views\\LaunchView.xaml.cs  SaveClick() start");

            try
            {
                var txt = txbName.Text.Trim();
                //DdpmCommonHelper.DeviceManagerSA?.CreateCustomProfile(_vm.CurrentDeviceInfo!.ID.ToString(), $"Test {_vm.WebcamSettings.CustomProfiles.Count + 1}");
                _vm.CurrentProfile.Name = txt;
                Dictionary<string, WebcamProfile> NewProfiles = new();
                if (EditMode == "EDIT")
                {
                    if (txt == EditingProfileName)
                    {
                        _vm.WebcamSettings.CustomProfiles[txt] = _vm.CurrentProfile;
                    }
                    else
                    {
                        foreach (var profile in _vm.WebcamSettings.CustomProfiles)
                        {
                            if (profile.Key == EditingProfileName)
                            {
                                NewProfiles.Add(txt, JsonConvert.DeserializeObject<WebcamProfile>(JsonConvert.SerializeObject(_vm.CurrentProfile))!);
                            }
                            else
                            {
                                NewProfiles.Add(profile.Key, profile.Value);
                            }
                        }
                        _vm.WebcamSettings.CustomProfiles = NewProfiles;
                    }
                }
                else
                {
                    var profile = JsonConvert.DeserializeObject<WebcamProfile>(JsonConvert.SerializeObject(_vm.CurrentProfile))!;
                    //NewProfiles.Add(txt, profile);
                    //_vm.WebcamSettings.CustomProfiles = NewProfiles.Concat(_vm.WebcamSettings.CustomProfiles!).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
                    _vm.WebcamSettings.CustomProfiles.Add(_vm.CurrentProfile.Name, profile);
                }
                _vm.CurrentProfileName = txt;
                WebcamSettings.ExportWebcamSettings(_vm.WebcamSettings, _vm.Model, DdpmCommonHelper.DeviceManagerSA, DdpmCommonHelper.Log);
                _vm.PrepareProfileItems();
                ProfileItems.ItemsSource = null;
                ProfileItems.ItemsSource = _vm.ProfileItems;
                btnPreset_Click(this, null);
                gdBattery.Visibility = Visibility.Visible;
                gdAddProfile.Visibility = Visibility.Collapsed;
                txtCaption.Text = _vm.Name;
                _vm.ClearUndo();
                _vm.EnableVBar();
                _vm.TooltipVisibility = Visibility.Collapsed;

                //Derek 2025/01/18
                DdpmCommonHelper.DeviceManagerSA?.SyncWebcamProfile(_vm.CurrentProfileName, false);
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog("DDPM.UI.WebCameraPlugin\\Views\\LaunchView.xaml.cs  SaveClick() ex: " + ex.Message);
            }
            DdpmCommonHelper.WriteUILog("DDPM.UI.WebCameraPlugin\\Views\\LaunchView.xaml.cs  SaveClick() end");
        }

        //Derek 1115 for Webcam PIMS 319099 and 319086
        /*private PresenceDetectionView GetPresenceDetectionView()
        {

            if (_vm.CurrentDeviceInfo!.IsESISupported)
            {
                _vm.UPD_Visibility = Visibility.Visible;
                _vm.MPS_Setting_Visibility = Visibility.Collapsed;
                _vm.MPS_UpdateFW_Visibility = Visibility.Collapsed;

                return PresenceDetectionView.InternalUPDSupport;
            }
            else if (_vm.CurrentDeviceInfo!.IsWindowsHelloSupported)
            {
                _vm.UPD_Visibility = Visibility.Collapsed;
                _vm.MPS_Setting_Visibility = Visibility.Visible;
                _vm.MPS_UpdateFW_Visibility = Visibility.Collapsed;

                return PresenceDetectionView.MicrosoftHPDSupport;
            }
            else
            {
                _vm.UPD_Visibility = Visibility.Collapsed;
                _vm.MPS_Setting_Visibility = Visibility.Collapsed;
                _vm.MPS_UpdateFW_Visibility = Visibility.Visible;

                return PresenceDetectionView.MicrosoftHPDNotSupport;
            }
        }*/

        /*private bool CheckPresenceDetection_UI()
        {
            bool blWebcamFW_UPD = false;
            bool blSystemcompatibility_MPS = false;

            int nFirmwareVersion = int.TryParse(_vm.FirmwareVersion, out var fw) ? fw : 0;

            if (nFirmwareVersion % 2 == 0 || _vm.Model == "P2424HEB" || _vm.Model == "P2724DEB" || _vm.Model == "P3424WEB" || _vm.Model == "U3223QZ" || _vm.Model == "U3224KB" || _vm.Model == "U3224KBA")
            {
                //is even, is UPD FW
                blWebcamFW_UPD = true;
            }
            else
            {
                //is odd , is MPS FW
                blWebcamFW_UPD = false;
            }

            if (WinVersion.GetVersion(out var info))
            {
                if (info.BuildNum >= (uint)(BuildNumber.Windows_11_22H2))
                    blSystemcompatibility_MPS = true;
                else
                    blSystemcompatibility_MPS = false;
            }

            string strComputerManufacturer = "";

            strComputerManufacturer = WinVersion.GetComputerManufacturer();

            bool blDellComputer = false;

            if (strComputerManufacturer.Contains("Dell", StringComparison.OrdinalIgnoreCase))
                blDellComputer = true;
            else
                blDellComputer = false;

            if (blWebcamFW_UPD && blDellComputer && info.BuildNum >= (uint)(BuildNumber.Windows_10_1507))
            {
                _vm.UPD_Visibility = Visibility.Visible;
                _vm.MPS_Setting_Visibility = Visibility.Collapsed;
                _vm.MPS_UpdateFW_Visibility = Visibility.Collapsed;

                return true;
            }

            if (blWebcamFW_UPD && !blSystemcompatibility_MPS && !blDellComputer && info.BuildNum >= (uint)(BuildNumber.Windows_10_1507))
            {
                return false;
            }

            if (blWebcamFW_UPD && blSystemcompatibility_MPS && blDellComputer && info.BuildNum >= (uint)(BuildNumber.Windows_11_22H2))
            {
                _vm.UPD_Visibility = Visibility.Collapsed;
                _vm.MPS_Setting_Visibility = Visibility.Collapsed;
                _vm.MPS_UpdateFW_Visibility = Visibility.Visible;

                return true;
            }

            if (blWebcamFW_UPD && blSystemcompatibility_MPS && !blDellComputer && info.BuildNum >= (uint)(BuildNumber.Windows_11_22H2))
            {
                _vm.UPD_Visibility = Visibility.Collapsed;
                _vm.MPS_Setting_Visibility = Visibility.Collapsed;
                _vm.MPS_UpdateFW_Visibility = Visibility.Visible;

                return true;
            }

            if (!blWebcamFW_UPD && !blSystemcompatibility_MPS && !blDellComputer && info.BuildNum < (uint)(BuildNumber.Windows_11_22H2))
            {
                return false;
            }

            if (!blWebcamFW_UPD && blDellComputer && info.BuildNum < (uint)(BuildNumber.Windows_11_22H2))
            {
                _vm.UPD_Visibility = Visibility.Collapsed;
                _vm.MPS_Setting_Visibility = Visibility.Collapsed;
                _vm.MPS_UpdateFW_Visibility = Visibility.Visible;

                return true;
            }

            if (!blWebcamFW_UPD && !blSystemcompatibility_MPS && !blDellComputer && info.BuildNum < (uint)(BuildNumber.Windows_11_22H2))
            {
                return false;
            }

            if (!blWebcamFW_UPD && blSystemcompatibility_MPS && info.BuildNum >= (uint)(BuildNumber.Windows_11_22H2))
            {
                _vm.UPD_Visibility = Visibility.Collapsed;
                _vm.MPS_Setting_Visibility = Visibility.Visible;
                _vm.MPS_UpdateFW_Visibility = Visibility.Collapsed;

                return true;
            }

            return false;
        }*/

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdatePVMargin(_vm.VbarSelectedIndex);
            ChangeDevNameWidth();
        }

        private void UpdatePVMargin(int index)
        {
            int mR = index == -1 ? 155 : 85;
            int mT = 0;
            int mB = 35;
            if (this.ActualHeight < 640)
            {
                mT = 55;
                mB = 75;

            }
            largeImage.Margin = new Thickness(40, mT, mR, mB);
            PanArrowContent.Margin = new Thickness(40, mT, mR, mB);
        }

        private void ChangeDevNameWidth()
        {
            txtCaption.MaxWidth = this.ActualWidth - RightGrid.ActualWidth - VbarGrid.ActualWidth - 100;
        }

        private void RightFrame_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ChangeDevNameWidth();
        }

        private void txtSearchText_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (_vm.CheckChar(e.Text))
                e.Handled = false;
            else
            {
                e.Handled = true;
                ShowAlert();
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //NarratorModeSupport.RecurseUitems( start) ;

            //Derek 2025/01/17 info QAM select current profile
            try
            {
                //Derek 2025/01/17 
                //DdpmCommonHelper.DeviceManagerSA?.SyncWebcamProfile("DDPMSetProfileToCurrent", false);
                DdpmCommonHelper.DeviceManagerSA?.SyncWebcamProfile(_vm?.CurrentProfileName, false);
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog($"UserControl_Loaded catch exception: {ex.Message}");
            }
            DdpmCommonHelper.WriteUILog($"Webcam UI Loaded timestamp: {DateTime.Now:hh:mm:ss.ffffff}");
        }

        private void txbName_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            e.Handled = e.Key == Key.Enter;
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            // Assumes MyViewbox is your Viewbox, and it has 1 child element
            if (largeImage.Child is FrameworkElement child)
            {
                // Original size before scaling
                double originalWidth = child.ActualWidth;
                double originalHeight = child.ActualHeight;

                // Scaled size (i.e., Viewbox render size inside Border)
                double viewboxWidth = largeImage.ActualWidth;
                double viewboxHeight = largeImage.ActualHeight;

                // Compute scale
                double scaleX = viewboxWidth / originalWidth;
                double scaleY = viewboxHeight / originalHeight;

                double uniformScale = Math.Min(scaleX, scaleY);

                Console.WriteLine($"Scale: {uniformScale:F3} (X: {scaleX:F3}, Y: {scaleY:F3})");
            }
        }

        private void txbName_LostFocus(object sender, RoutedEventArgs e)
        {
            bdrAlert2.Visibility = Visibility.Hidden;
        }
    }
}