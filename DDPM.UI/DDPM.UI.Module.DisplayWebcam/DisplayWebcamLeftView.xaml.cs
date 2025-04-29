using CommunityToolkit.Mvvm.Input;
using DDPM.PowerMon;
using DDPM.SA.Common;
using DDPM.SA.Common.Settings;
using DDPM.UI.Common;
using DDPM.UI.Common.UserControls;
using DDPM.UI.Plugin.Common;
using DDPM.UI.Plugin.ViewModels;
using DDPM.UI.Resources.Helper;
using Dell.Client.Framework.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Windows.Devices.Enumeration;
using Windows.Graphics.Imaging;
using Windows.Media.Capture;
using Windows.Media.Capture.Frames;
using Windows.Media.MediaProperties;
using Windows.Storage;
using Windows.UI.Popups;
using WinRT;

namespace DDPM.UI.Module.DisplayWebcam
{
    /// <summary>
    /// Interaction logic for DisplayWebcamLeftView.xaml
    /// </summary>
    public partial class DisplayWebcamLeftView : UserControl
    {
        private readonly WebCameraViewModel _vm;
        private static PowerEventControl _pwr_Mon;
        bool in_CameraPlugin = true;
        public bool exit_status_thread = false;
        private DispatcherTimer RecordingTimer;
        private readonly Stopwatch stopwatch = new();
        private DispatcherTimer _timer = new();
        private bool _running = false;
        private readonly string[] PresetNames = [LangHelper.Instance["Default"], LangHelper.Instance["Smooth"], LangHelper.Instance["Vibrant"], LangHelper.Instance["Warm"]];
        private int _countdownValue;
        public Thread status_thread;
        private readonly DispatcherTimer AlertTimer;
        bool is_WindwosHelloSupport = false;// DdpmCommonHelper.DeviceManagerSA?.GetIsWindowsHelloCapabilityVerified(_vm.CurrentDeviceInfo!.ID.ToString()).Result;


        bool AllSupportedResolutions = true;
        public DisplayWebcamLeftView(WebCameraViewModel webCameraViewModel)
        {
            DdpmCommonHelper.WriteUILog($"Webcam UI DisplayWebcamLeftView Begin timestamp: {DateTime.Now:hh:mm:ss.ffffff}");
            try
            {
                _vm = webCameraViewModel;
                if (DdpmCommonHelper.MyConsole != null)
                {
                    DdpmCommonHelper.MyConsole.RegisterForEvent(ConsoleEventNames.MainWindow_Force_Camera_Unlock, OnWebcamCloseEvent);
                    DdpmCommonHelper.WriteUILog("[DisplayWebcamLeftView] MainWindow_Force_Camera_Unlock event registered");

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
                //check_PresenceFunction(); //haven't copy from WebCameraPlugin\Views\LaunchView.xaml.cs  because the new monitor doesn't support it.
                //BuildModuleGroups();
                //initResolutionFPS();
                //usb 2.0限制規則要放在最後做校正
                CheckUSBtype();

            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog("[DisplayWebcamLeftView] ex:" + ex.Message);
            }

            AlertText1.Text = string.Format(LangHelper.Instance["InputValidationTooltip.1"], "30");
            AlertText2.Text = string.Format(LangHelper.Instance["InputValidationTooltip.2"], $"@ - {LangHelper.Instance["InputValidationTooltip.5"]}");

            AlertTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(5)
            };
            AlertTimer.Tick += AlertTimer_Tick;
            DdpmCommonHelper.WriteUILog($"Webcam UI DisplayWebcamLeftView End timestamp: {DateTime.Now:hh:mm:ss.ffffff}");

        }
        public void CheckUSBtype()
        {
            print_debug("CheckUSBtype() v1 start");

            _vm.MessageBoxVisibilityUsbType = Visibility.Collapsed;

            string model = _vm.CurrentDeviceInfo?.ModelNumber ?? "";

            if (model == null)
            {
                string log = $"[DisplayWebcamLeftView] CheckUSBtype() model is null";
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
                //new monitor P2426HEB/P2726DEB/P3426WEB
                case "P2426HEB":
                case "P2726DEB":
                case "P3426WEB":

                //
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
            print_debug("[DisplayWebcamLeftView] CheckUSBtype() end");
        }

        private void SetPrioritizeShow(Visibility visibility)
        {
            _vm.bdrPrioritize_show = !is_WindwosHelloSupport ? Visibility.Collapsed : visibility;
        }

        private void AlertTimer_Tick(object? sender, EventArgs e)
        {
            bdrAlert2.Visibility = Visibility.Collapsed;
            AlertTimer.Stop();
        }
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
                DdpmCommonHelper.WriteUILog("[DisplayWebcamLeftView] StartRecordingAsync() : " + ex.Message);
                // File I/O errors are reported as exceptions
                // Debug.WriteLine("Exception when starting video recording: " + ex.ToString());
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
        public void print_debug(string str)
        {
            Console.WriteLine(str);
            string info = DateTime.Now.ToString("yyyy-MM-dd h:mm:tt") + "#" + str + "\r\n";
            if (Directory.Exists(@"C:\ui_cond"))
                File.AppendAllText(@"C:\ui_cond\ui_cond.log", info);

            DdpmCommonHelper.WriteUILog(info);
        }

        private MediaCaptureFailedEventHandler handler = (sender, e) =>
        {
            System.Threading.Tasks.Task task = System.Threading.Tasks.Task.Run(async () =>
            {
                await new MessageDialog("There was an error capturing the video from camera.", "Error").ShowAsync();
            });
        };
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
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        private static extern bool GetDiskFreeSpaceEx(string lpDirectoryName, out ulong lpFreeBytesAvailable, out ulong lpTotalNumberOfBytes, out ulong lpTotalNumberOfFreeBytes);
        private static bool _GetDiskFreeSpaceEx(string lpDirectoryName, out ulong lpFreeBytesAvailable, out ulong lpTotalNumberOfBytes, out ulong lpTotalNumberOfFreeBytes)
        {
            bool rst = GetDiskFreeSpaceEx(lpDirectoryName, out lpFreeBytesAvailable, out lpTotalNumberOfBytes, out lpTotalNumberOfFreeBytes);

            if (!rst)
            {
                DdpmCommonHelper.WriteUILog("[DisplayWebcamLeftView] GetDiskFreeSpaceEx failed.");

#if DEBUG
                Console.WriteLine("[DisplayWebcamLeftView] GetDiskFreeSpaceEx failed.");
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
                        DdpmCommonHelper.WriteUILog($"[DisplayWebcamLeftView] UIUpdateNotify profileName: {msgs[1]}");

                        ChangeProfileByQAM(msgs[1]);
                    }
                }
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog($"[DisplayWebcamLeftView] DeviceManagerSA_UIUpdateNotify catch exception: {ex.Message}");
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
                DdpmCommonHelper.WriteUILog("[DisplayWebcamLeftView] ChangeProfileByQAM() ex:" + ex.Message);
            }
        }

        private void OnWebcamCloseEvent(object sender, EventManagerArgs e)
        {
            DdpmCommonHelper.WriteUILog($"[DisplayWebcamLeftView][OnWebcamCloseEvent] event MainWindow_Force_Camera_Unlock received");
            FreeWebcamResource();
        }
        private async void FreeWebcamResource()
        {
            // Unexpected closed recording.
            if (_vm.IsRecording)
            {
                DdpmCommonHelper.WriteUILog($"[DisplayWebcamLeftView] [FreeWebcamResource] force stop recording.");
                StopRecord();
            }

            try
            {
                DdpmCommonHelper.WriteUILog($"[DisplayWebcamLeftView] [FreeWebcamResource] Free PowerEventControl");

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
                DdpmCommonHelper.WriteUILog("[DisplayWebcamLeftView] [FreeWebcamResource] PowerEvent Control got exception: " + ex.Message);
            }


            try
            {
                in_CameraPlugin = false;
                exit_status_thread = true;
                _vm?.mre.Set();
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog($"[DisplayWebcamLeftView] [FreeWebcamResource] got exception 1:{ex.ToString()}");
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
                DdpmCommonHelper.WriteUILog($"[DisplayWebcamLeftView] [FreeWebcamResource] got exception 2:{ex.ToString()}");
            }


            try
            {
                if (_vm?.MediaFrameReader != null)
                    _vm.MediaFrameReader.FrameArrived -= MediaFrameReader_FrameArrived;
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog("[DisplayWebcamLeftView] [FreeWebcamResource] Free MediaFrameReader got exception: " + ex.Message);
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
                DdpmCommonHelper.WriteUILog($"[DisplayWebcamLeftView] [FreeWebcamResource] got exception 4:{ex.ToString()}");
            }

            try
            {
                await CleanupMediaCaptureAsync();
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog("[DisplayWebcamLeftView] FreeWebcamResource()  CleanupMediaCaptureAsync() ex 2: " + ex.Message);
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
            DdpmCommonHelper.WriteUILog("[DisplayWebcamLeftView] [FreeWebcamResource] Webcam LaunchView_Unloaded end");
            /*if ( _vm?.close_app == true )
            {
                Console.WriteLine("force exit");
                Environment.Exit(0);
            }*/
            //await _vm.CleanupMediaCapture();
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
                    DdpmCommonHelper.WriteUILog($"[DisplayWebcamLeftView] Error del MediaFrameReader FrameArrived: {ex.Message}");
                }

                try
                {
                    await _vm.MediaFrameReader.StopAsync();

                }
                catch (Exception ex)
                {
                    DdpmCommonHelper.WriteUILog($"[DisplayWebcamLeftView] Error stopping MediaFrameReader: {ex.Message}");
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
                    DdpmCommonHelper.WriteUILog($"[DisplayWebcamLeftView] Error Dispose MediaFrameReader: {ex.Message}");
                }
            }

            if (_vm.MediaCapture != null)
            {
                _vm.MediaCapture.Dispose();
                _vm.MediaCapture = null;
            }
        }

        private void WebcamSettingChanged(object? sender, EventArgs e)
        {
            _vm.mre.Set();
            //Preview();
        }

        private bool isProfilePropertyChanged = false;
        private void ProfilePropertyChanged(object? sender, EventArgs e)
        {
            txtPreset.Text = $"{Strings.Preset}: {LangHelper.Instance["None"]}";
            isProfilePropertyChanged = true;
        }

        WriteableBitmap writeableBitmap;
        Int32Rect react;
        int before_width = 0;
        int before_height = 0;
        int ImageBufferSize = 0;

        [ComImport]
        [Guid("5B0D3235-4DBA-4D44-865E-8F1D0E4FD04D")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        unsafe interface IMemoryBufferByteAccess
        {
            void GetBuffer(out byte* buffer, out uint capacity);
        }

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
                DdpmCommonHelper.WriteUILog($"[DisplayWebcamLeftView] Exception occurred: {ex.Message}");
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
                DdpmCommonHelper.WriteUILog($"[DisplayWebcamLeftView] MediaFrameReader_FrameArrived Exception occurred 2 : {ex.Message}");
            }

            _running = false;
        }
        private void DeviceManagerSA_OnSystemSessionEnd(object? sender, EventArgs e)
        {
            DdpmCommonHelper.WriteUILog($"[DisplayWebcamLeftView] catch event DeviceManagerSA_OnSystemSessionEnd");
            if (_vm.IsRecording)
                Dispatcher.Invoke(new Action(() =>
                {
                    UserStopRecord();
                }));
        }
        private void DeviceManagerSA_OnSystemResume(object? sender, EventArgs e)
        {
            DdpmCommonHelper.WriteUILog($"[DisplayWebcamLeftView] catch event DeviceManagerSA_OnSystemResume");
            //Debug.WriteLine("DeviceManagerSA_OnSystemResume");
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
            DdpmCommonHelper.WriteUILog($"[DisplayWebcamLeftView] catch event DeviceManagerSA_OnSystemSuspend");

            if (_vm.IsRecording)
                Dispatcher.Invoke(new Action(() =>
                {
                    UserStopRecord();
                }));
        }

        private void UserStopRecord()
        {
            DdpmCommonHelper.WriteUILog("[DisplayWebcamLeftView]  UserStopRecord() start");

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
                DdpmCommonHelper.WriteUILog("[DisplayWebcamLeftView]  UserStopRecord() ex:" + ex.Message);
            }
            DdpmCommonHelper.WriteUILog("[DisplayWebcamLeftView]  UserStopRecord() end");

        }
        private void MonitorEvent_On(object? sender, EventArgs e)
        {
            Trace.WriteLine("GET MONITOR ON EVENT");
        }

        private void StopRecord()
        {
            if (_vm.IsRecording)
            {
                _ = StopRecordingAsync();
            }

        }
        private LowLagMediaRecording _lowLag;
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
                    DdpmCommonHelper.WriteUILog("[DisplayWebcamLeftView]  StopRecordingAsync() : " + ex.Message);
                }
            }
            _vm.IsRecording = false;

            DdpmCommonHelper.WriteUILog("Stopped recording!");
        }
        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {

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
                DdpmCommonHelper.WriteUILog($"[DisplayWebcamLeftView] UserControl_Loaded catch exception: {ex.Message}");
            }
            DdpmCommonHelper.WriteUILog($"[DisplayWebcamLeftView] Webcam UI Loaded timestamp: {DateTime.Now:hh:mm:ss.ffffff}");
        }

        private void ChangePan(object sender, MouseButtonEventArgs e)
        {
            DdpmCommonHelper.WriteUILog("[DisplayWebcamLeftView] ChangePan() start");

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
                DdpmCommonHelper.WriteUILog("[DisplayWebcamLeftView] ChangePan() ex:" + ex.Message);
            }
            DdpmCommonHelper.WriteUILog("[DisplayWebcamLeftView] ChangePan() end");
        }

        private void btnFolder_Click(object sender, MouseButtonEventArgs e)
        {

        }

        private void btnPause_Click(object sender, MouseButtonEventArgs e)
        {

        }

        private void btnPlay_Click(object sender, MouseButtonEventArgs e)
        {

        }

        private void btnRecord_Click(object sender, MouseButtonEventArgs e)
        {

        }

        private void btnStop_Click(object sender, MouseButtonEventArgs e)
        {

        }

        private void ProfileSelected(object sender, MouseButtonEventArgs e)
        {

        }

        private void DeletePreset(object sender, MouseButtonEventArgs e)
        {

        }

        private void EditPreset(object sender, MouseButtonEventArgs e)
        {

        }

        private void AddPreset(object sender, MouseButtonEventArgs e)
        {

        }

        private void btnPreset_Click(object sender, MouseButtonEventArgs e)
        {

        }

        private void txtSearchText_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {

        }

        private void txbName_LostFocus(object sender, RoutedEventArgs e)
        {

        }

        private void NameTextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void CancelClick(object sender, RoutedEventArgs e)
        {

        }

        private void SaveClick(object sender, RoutedEventArgs e)
        {

        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
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
    }
}
