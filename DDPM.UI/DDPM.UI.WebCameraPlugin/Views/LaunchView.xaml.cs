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
using WebcamProfile = DDPM.UI.Common.WebcamProfile;

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

        private readonly WebCameraViewModel? _vm;

        private readonly int[] _rightFrameWidth = new int[] { 0, 483, 483, 483, 483, 483 };
        private readonly string CameraControl = LangHelper.Instance["Camera.0"];
        private readonly string ColorandImage = LangHelper.Instance["Camera.1"];
        private readonly string PresenceDetection = LangHelper.Instance["Camera.2"];
        private readonly string Capture = LangHelper.Instance["Camera.3"];
        private readonly string Microphone = LangHelper.Instance["Camera.4"];

        private DispatcherTimer _timer = new();
        private int _countdownValue;
        private bool IsPresetOpen = false;
        private Stopwatch stopwatch = new Stopwatch();
        private DispatcherTimer RecordingTimer;
        private bool _running = false;
        private MediaCapture _mediaCapture;
        private SoftwareBitmap backBitmapBuffer;

        //private readonly string[] PresetNames = [LangHelper.Instance["Default"], LangHelper.Instance["Camera.10"], LangHelper.Instance["Camera.9"], LangHelper.Instance["Camera.8"]];
        private readonly string[] PresetNames = [LangHelper.Instance["Default"], Strings.Smooth, Strings.Vibrant, Strings.Warm];
        private string EditMode = string.Empty;
        private string EditingProfileName = string.Empty;
        private static PowerEventControl _pwr_Mon = null;

        public LaunchView()
        {
            InitializeComponent();

            _vm = (WebCameraViewModel?)WebCameraplugin.PluginIoc?.GetService<IPeripheralViewModel>()!;

            if (_vm != null)
            {
                _vm.Reset();
                DataContext = _vm;
                _vm.VbarItemClickCommand = new RelayCommand<VbarItem>(OnVbarItemClicked!);
                BuildModuleGroups();

                if (PresetNames.Contains(_vm!.CurrentProfileName))
                {
                    txtPreset.Text = $"{Strings.Preset}: {_vm.CurrentProfileName}";
                }
                else
                {
                    txtPreset.Text = Utility.CheckTextLength($"{_vm!.CurrentProfileName}", 140, 14);
                }
                txtAddPreset.Text = LangHelper.Instance["Camera.5"];

                //ProfileItems.ItemsSource = _vm.ProfileNames;
                ProfileItems.ItemsSource = _vm.ProfileItems;
                Mouse.OverrideCursor = null;
            }
            txtName.Text = Strings.Name;
            txtMsg.Text = Strings.NameIsTaken;
            btnCancel.Caption = Strings.Cancel;
            btnSave.Caption = Strings.Save;

            //lock/unlock, no ui element currently
            if (DdpmCommonHelper.DeviceManagerSA != null)
            {
                DdpmCommonHelper.DeviceManagerSA.ITSettingsActionEvent += DeviceManagerSA_ITSettingsActionEvent;

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
                        if (data.LockSettings != null)
                        {
                            if (DdpmCommonHelper.GetUINotifyPropertyValue_isAnyLocked(data, "Lock_Webcam"))
                            {
                                //RestoreLockIcon.Visibility = Visibility.Visible;
                                //txtRestore.IsEnabled = false;
                            }
                        }
                    }
                }
            }

            RecordingTimer = new DispatcherTimer();
            RecordingTimer.Interval = TimeSpan.FromSeconds(1);
            RecordingTimer.Tick += RecordingTimer_Tick;

            _vm!.WebcamSettingChanged += WebcamSettingChanged;
            _vm!.ProfilePropertyChanged += ProfilePropertyChanged;

            Preview();
            EnableMonitorOnEvent();
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
            Preview();
        }

        private async void Preview()
        {
            //return;
            if (_vm!.MediaCapture != null)
            { _ = CleanupMediaCaptureAsync(); }

            try
            {
                // jim add 20240621
                var frameSourceGroups = await MediaFrameSourceGroup.FindAllAsync();

                // 20240626  jim add to avoid exception
                if (frameSourceGroups.Count <= 0)
                {
                    Debug.WriteLine("frameSourceGroups.Count = 0");
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
                    Debug.WriteLine("selectedGroup null");
                    return;
                }

                MediaFrameSourceInfo frameSourceInfo = selectedFrameSourceGroup.SourceInfos[0];

                _vm!.MediaCapture = new MediaCapture();
                _vm.MediaCapture.Failed += handler;

                try
                {
                    await _vm.MediaCapture.InitializeAsync(new MediaCaptureInitializationSettings()
                    {
                        SourceGroup = selectedFrameSourceGroup,
                        SharingMode = MediaCaptureSharingMode.ExclusiveControl,
                        //SharingMode = MediaCaptureSharingMode.SharedReadOnly,
                        MemoryPreference = MediaCaptureMemoryPreference.Cpu,
                        StreamingCaptureMode = StreamingCaptureMode.AudioAndVideo
                    });
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("MediaCapture initiate fail: " + ex.Message);
                    return;
                }

                //Derek 1108 Move to here to fix Webcam PIMS-314613
                // Query all properties [resolution and frame rate] of the webcam device
                _vm.allProperties = _vm.MediaCapture.VideoDeviceController.GetAvailableMediaStreamProperties(MediaStreamType.VideoPreview).Select(x => new StreamResolution(x));

                // Order them by resolution then frame rate
                _vm.allProperties = _vm.allProperties.OrderByDescending(x => x.Height * x.Width).ThenByDescending(x => x.FrameRate);
                foreach (var property in _vm.allProperties)
                {
                    string properties_temp = property.GetFriendlyName();
                    if (properties_temp.Contains(_vm.WebcamSettings.CurrentResolution, StringComparison.OrdinalIgnoreCase) && properties_temp.Contains(_vm.WebcamSettings.CurrentFPS, StringComparison.OrdinalIgnoreCase))
                    {
                        var encodingProperties = property.EncodingProperties;
                        _ = _vm.MediaCapture!.VideoDeviceController.SetMediaStreamPropertiesAsync(MediaStreamType.VideoPreview, encodingProperties);
                        break;
                    }
                }

                MediaFrameSource mediaFrameSource = _vm.MediaCapture.FrameSources[frameSourceInfo.Id];

                // 20240626 jim modify
                _vm.MediaFrameReader = await _vm.MediaCapture.CreateFrameReaderAsync(mediaFrameSource, MediaEncodingSubtypes.Argb32);
                
                _vm.MediaFrameReader.FrameArrived += MediaFrameReader_FrameArrived;

                await _vm.MediaFrameReader.StartAsync();

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
            }
            catch (Exception Exc)
            {
                Debug.WriteLine("MediaCapture initialization failed: " + Exc.Message);
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
                DDPMSettings data = DdpmCommonHelper.DeviceManagerSA!.ReloadAppConfigData().Result;
                if (data != null && data.LockSettings != null)
                {
                    if (DdpmCommonHelper.GetUINotifyPropertyValue_isAnyLocked(data, "Lock_Webcam"))
                    {
                        //RestoreLockIcon.Visibility = Visibility.Visible;
                        //txtRestore.IsEnabled = false;
                    }
                }
            }));
        }

        private async void LaunchView_Unloaded(object sender, RoutedEventArgs e)
        {
            if (DdpmCommonHelper.DeviceManagerSA != null)
            {
                DdpmCommonHelper.DeviceManagerSA.ITSettingsActionEvent -= DeviceManagerSA_ITSettingsActionEvent;
            }
            _vm!.MediaFrameReader.FrameArrived -= MediaFrameReader_FrameArrived;
            _vm.ProfilePropertyChanged -= ProfilePropertyChanged;
            _vm.WebcamSettingChanged -= WebcamSettingChanged;
            try
            {
                await CleanupMediaCaptureAsync();

                if(_pwr_Mon != null)
                {
                    _pwr_Mon.MonitorTurnedOn -= MonitorEvent_On;
                    _pwr_Mon.Close_Event();
                    _pwr_Mon = null;
                }
            }
            catch
            { }
            //await _vm.CleanupMediaCapture();
        }

        #region Init for Modules

        /// <summary>
        /// Base on specified monitor's capabiliies to build the Vbar items, and headers/modules
        /// </summary>
        private void BuildModuleGroups()
        {
            List<ModuleGroup> groups = new();
            ModuleGroup moduleGroup;

            moduleGroup = new ModuleGroup()
            {
                GroupName = CameraControl,
                GroupIcon = DdpmCommonHelper.GetImageSourceFromCommonResource("Resources/Images/CameraControl.png", "DDPM.UI.Resources")
            };
            moduleGroup.AddHeader(CameraControl, new WebCameraSettingsModule(_vm!));
            groups.Add(moduleGroup);

            moduleGroup = new ModuleGroup()
            {
                GroupName = ColorandImage,
                GroupIcon = DdpmCommonHelper.GetImageSourceFromCommonResource("Resources/Images/CameraColorImage.png", "DDPM.UI.Resources")
            };
            moduleGroup.AddHeader(ColorandImage, new WebCameraColorImageModule(_vm!));
            groups.Add(moduleGroup);


            if (_vm!.Model == "WB7022" || _vm.Model == "P2424HEB" || _vm.Model == "P2724DEB" || _vm.Model == "P3424WEB" || _vm.Model == "U3223QZ" || _vm.Model == "U3224KB" || _vm.Model == "U3224KBA")
            {
                bool blRet = true;

                blRet = CheckPresenceDetection_UI();

                moduleGroup = new ModuleGroup()
                {
                    GroupName = PresenceDetection,
                    GroupIcon = DdpmCommonHelper.GetImageSourceFromCommonResource("Resources/Images/CameraPresenceDetection.png", "DDPM.UI.Resources")
                };
                moduleGroup.AddHeader(PresenceDetection, new WebCameraPresenceDetectionModule(_vm!));
                groups.Add(moduleGroup);
            }

            moduleGroup = new ModuleGroup()
            {
                GroupName = Capture,
                GroupIcon = DdpmCommonHelper.GetImageSourceFromCommonResource("Resources/Images/CameraCapture.png", "DDPM.UI.Resources")
            };
            moduleGroup.AddHeader(Capture, new WebCameraCaptureModule(_vm!));
            groups.Add(moduleGroup);

            if (_vm.CurrentDeviceInfo!.IsMicEnumerationSupported)
            {
                moduleGroup = new ModuleGroup()
                {
                    GroupName = Microphone,
                    GroupIcon = DdpmCommonHelper.GetImageSourceFromCommonResource("Resources/Images/Microphone.png", "DDPM.UI.Resources")
                };
                moduleGroup.AddHeader(Microphone, new WebCameraMicrophoneModule(_vm!));
                groups.Add(moduleGroup);
            }

            _vm!.ModuleGroups = groups;
        }

        #endregion Init for Modules

        #region Vbar

        private void OnVbarItemClicked(VbarItem newItem)
        {
            if (newItem.Id == _vm!.VbarSelectedIndex)
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
            if (_vm!.VbarSelectedIndex == -1)
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
        [DllImport("Kernel32.dll", EntryPoint = "RtlMoveMemory")]
        public static extern void CopyMemory(IntPtr Destination, IntPtr Source, int Length);
        int ImageBufferSize = 0;
        int count = 0;
        private async void MediaFrameReader_FrameArrived(MediaFrameReader sender, MediaFrameArrivedEventArgs args)
        {
            if (_running) return;
            _running = true;

            var softwareBitmap = (sender.TryAcquireLatestFrame()?.VideoMediaFrame)?.SoftwareBitmap;

            Thread.Sleep(60);

            if (softwareBitmap != null)
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
            _running = false;
        }

        /// <summary>
        /// MediaFrameReader FrameArrived event
        /// </summary>
        private void MediaFrameReader_FrameArrived_org(MediaFrameReader sender, MediaFrameArrivedEventArgs args)
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

        }

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

        private void StartRecord()
        {
            _vm!.IsRecording = true;

            if (_vm!.WebcamCountdown)
            {
                //_countdownValue = 3; // 設置倒數起始值
                //CountdownText.Text = _countdownValue.ToString();
                _timer = new DispatcherTimer();
                _timer.Interval = TimeSpan.FromSeconds(3);
                _timer.Tick += Timer_Tick;
                _timer.Start();

                DdpmCommonHelper.DeviceManagerSA!.ShowOSD(Screen.PrimaryScreen!.DeviceName, OSDType.StartRecording);
            }
            else
                StartRecordingAsync().RunSynchronously();
        }

        private void StopRecord()
        {
            _ = StopRecordingAsync();
        }
        private void Timer_Tick(object? sender, EventArgs e)
        {
            //_countdownValue--;
            //if (_countdownValue > 0)
            //{
            //    CountdownText.Text = _countdownValue.ToString();
            //}
            //else
            //{
            //    CountdownText.Text = "";
            //    StartRecordingAsync().RunSynchronously();
            //    _timer.Stop();
            //}
            _timer.Stop();
            StartRecordingAsync().RunSynchronously();
        }

        /// <summary>
        /// Records an MP4 video to a StorageFile and adds rotation metadata to it
        /// </summary>
        /// <returns></returns>
        private async Task StartRecordingAsync()
        {
            stopwatch.Start();
            RecordingTimer.Start();
            try
            {
                //var picturesLibrary = await StorageLibrary.GetLibraryAsync(KnownLibraryId.Pictures);
                // Fall back to the local app storage if the Pictures Library is not available
                //_vm._captureFolder = picturesLibrary.SaveFolder ?? ApplicationData.Current.LocalFolder;
                var captureFolder = await StorageFolder.GetFolderFromPathAsync(_vm!.VideoCaptureFolder);

                // Create storage file for the capture
                var videoFile = await captureFolder.CreateFileAsync(DateTime.Now.ToString("'DDPMVideo'yyyy-MM-dd-HH-mm-ss.'mp4'"), CreationCollisionOption.GenerateUniqueName);

                var encodingProfile = MediaEncodingProfile.CreateMp4(VideoEncodingQuality.Auto);

                // Calculate rotation angle, taking mirroring into account if necessary
                var rotationAngle = 360 - ConvertDeviceOrientationToDegrees(GetCameraOrientation());
                encodingProfile.Video.Properties.Add(RotationKey, PropertyValue.CreateInt32(rotationAngle));

                Debug.WriteLine("Starting recording to " + videoFile.Path);

                if (_vm.MediaCapture != null)
                    await _vm.MediaCapture.StartRecordToStorageFileAsync(encodingProfile, videoFile);

                Debug.WriteLine("Started recording!");
            }
            catch (Exception ex)
            {
                // File I/O errors are reported as exceptions
                Debug.WriteLine("Exception when starting video recording: " + ex.ToString());
            }
        }

        /// <summary>
        /// Stops recording a video
        /// </summary>
        /// <returns></returns>
        private async Task StopRecordingAsync()
        {
            Debug.WriteLine("Stopping recording...");

            if (_vm.MediaCapture != null)
                await _vm.MediaCapture.StopRecordAsync();

            _vm!.IsRecording = false;
            Debug.WriteLine("Stopped recording!");
        }

        /// <summary>
        /// Resume recording a video
        /// </summary>
        /// <returns></returns>
        private async void ResumeRecordingAsync()
        {
            Debug.WriteLine("Resuming recording...");

            if (_vm.MediaCapture != null)
                await _vm.MediaCapture.ResumeRecordAsync();

            Debug.WriteLine("Resume recording!");
        }

        /// <summary>
        /// Pause recording a video
        /// </summary>
        /// <returns></returns>
        private async void PauseRecordingAsync()
        {
            Debug.WriteLine("Pausing recording...");

            if (_vm!.MediaCapture != null)
            {
                _ = await _vm.MediaCapture.PauseRecordWithResultAsync(Windows.Media.Devices.MediaCapturePauseBehavior.RetainHardwareResources);
            }

            Debug.WriteLine("Pause recording!");
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
            if (_vm!.MediaFrameReader != null)
            {
                _vm.MediaFrameReader.FrameArrived -= MediaFrameReader_FrameArrived;
                try
                {
                    await _vm.MediaFrameReader.StopAsync();

                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error stopping MediaFrameReader: {ex.Message}");
                }

                try
                {
                    if (_vm.MediaFrameReader != null)
                        _vm.MediaFrameReader.Dispose();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error Dispose MediaFrameReader: {ex.Message}");
                }

                _vm.MediaFrameReader = null;
            }
            if (_vm!.MediaCapture != null)
            {
                _vm!.MediaCapture.Dispose();
                _vm.MediaCapture = null;
            }
        }

        private void btnRecord_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            btnPause.Visibility = Visibility.Visible;
            btnRecord.Visibility = Visibility.Collapsed;
            btnStop.Visibility = Visibility.Visible;
            txtTimer.Visibility = Visibility.Visible;
            if (IsPresetOpen)
            { btnPreset_Click(this, null); }
            btnPreset.IsEnabled = false;
            StartRecord();
        }

        private void btnStop_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            StopRecord();
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
        }

        private void ProfileSelected(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var profileName = ((UXTextBlock)sender).Tag.ToString()!;
            if (profileName != _vm!.CurrentProfileName || isProfilePropertyChanged)
            {
                //DdpmCommonHelper.DeviceManagerSA!.SetProfile(_vm.CurrentDeviceInfo!.ID.ToString(), _vm.ProfileIDs[profileName]);
                _vm!.CurrentProfileName = profileName;
                _vm.SetProfile();
                isProfilePropertyChanged = false;
            }
            btnPreset_Click(this, null);
        }

        private void btnPreset_Click(object sender, System.Windows.Input.MouseButtonEventArgs? e)
        {
            var img = (Image)FindName($"imgDown");
            DoubleAnimation rotateAnimation;
            var AnimatedPanel = (StackPanel)FindName("spPresets");
            if (IsPresetOpen)
            {
                var txt = $"{Strings.Preset}: {_vm!.CurrentProfileName}";
                if (!PresetNames.Contains(_vm!.CurrentProfileName))
                {
                    txt = Utility.CheckTextLength($"{_vm!.CurrentProfileName}", 140, 14);
                }
                txtPreset.Text = txt;
                rotateAnimation = new()
                {
                    From = 180,
                    To = 0,
                    Duration = new Duration(TimeSpan.FromSeconds(0.3)),
                };
                AnimatedPanel.Visibility = Visibility.Collapsed;
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

        private void EditPreset(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var profileName = ((Image)sender).Tag.ToString()!;
            EditMode = "EDIT";
            EditingProfileName = profileName;
            if (profileName != _vm!.CurrentProfileName)
            {
                _vm.CurrentProfileName = profileName;
                _vm.SetProfile();
            }
            txbName.Text = profileName;
            _vm!.DisableVBar();
            gdBattery.Visibility = Visibility.Collapsed;
            gdAddProfile.Visibility = Visibility.Visible;
            txtCaption.Text = Strings.EditPreset;
            _vm.TooltipVisibility = Visibility.Visible;
        }

        private void DeletePreset(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var profileName = ((Image)sender).Tag.ToString()!;
            //DdpmCommonHelper.DeviceManagerSA!.DeleteProfile(_vm!.CurrentDeviceInfo!.ID.ToString(), _vm.WebcamSettings.CustomProfiles[profileName].Id);
            if (_vm!.WebcamSettings.CustomProfiles.ContainsKey(profileName))
            {
                _vm!.WebcamSettings.CustomProfiles.Remove(profileName);
                WebcamSettings.ExportWebcamSettings(_vm.WebcamSettings, _vm.Model);
                _vm.PrepareProfileItems();
                ProfileItems.ItemsSource = null;
                ProfileItems.ItemsSource = _vm.ProfileItems;
            }

            if (profileName == _vm!.CurrentProfileName)
            {
                _vm!.CurrentProfileName = "Default";
                _vm.SetProfile();
            }
            btnPreset_Click(this, null);

        }

        private void btnPlay_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ResumeRecordingAsync();
            stopwatch.Start();
            btnPause.Visibility = Visibility.Visible;
            btnPlay.Visibility = Visibility.Collapsed;
        }

        private void btnPause_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            PauseRecordingAsync();
            stopwatch.Stop();
            btnPause.Visibility = Visibility.Collapsed;
            btnPlay.Visibility = Visibility.Visible;
        }

        private void btnFolder_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Process.Start("explorer.exe", _vm!.VideoCaptureFolder);
        }

        //private async Task InitializeCameraAsync()
        //{
        //    try
        //    {
        //        _vm!.MediaCapture = new MediaCapture();

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
            if (sender is Image img)
            {
                var value = 0;
                switch (img.Tag.ToString())
                {
                    case "L":
                        if (_vm!.CurrentProfile.Pan == _vm.CurrentDeviceInfo!.PanMin)
                            return;

                        value = _vm.CurrentProfile.Pan - _vm.CurrentDeviceInfo.PanSteppingDelta;
                        if (value < _vm.CurrentDeviceInfo!.PanMin)
                            value = _vm.CurrentDeviceInfo!.PanMin;

                        _vm.SetPan(value);
                        break;
                    case "R":
                        if (_vm!.CurrentProfile.Pan == _vm.CurrentDeviceInfo!.PanMax)
                            return;

                        value = _vm.CurrentProfile.Pan + _vm.CurrentDeviceInfo.PanSteppingDelta;
                        if (value > _vm.CurrentDeviceInfo!.PanMax)
                            value = _vm.CurrentDeviceInfo!.PanMax;

                        _vm.SetPan(value);
                        break;
                    case "T":
                        if (_vm!.CurrentProfile.Tilt == _vm.CurrentDeviceInfo!.TiltMax)
                            return;

                        value = _vm.CurrentProfile.Tilt + _vm.CurrentDeviceInfo.TiltSteppingDelta;
                        if (value > _vm.CurrentDeviceInfo!.TiltMax)
                            value = _vm.CurrentDeviceInfo!.TiltMax;

                        _vm.SetTilt(value);
                        break;
                    case "D":
                        if (_vm!.CurrentProfile.Tilt == _vm.CurrentDeviceInfo!.TiltMin)
                            return;

                        value = _vm.CurrentProfile.Tilt - _vm.CurrentDeviceInfo.TiltSteppingDelta;
                        if (value < _vm.CurrentDeviceInfo!.TiltMin)
                            value = _vm.CurrentDeviceInfo!.TiltMin;

                        _vm.SetTilt(value);
                        break;
                }
            }
        }

        private void AddPreset(object sender, MouseButtonEventArgs e)
        {
            EditMode = "ADD";
            _vm!.DisableVBar();
            gdBattery.Visibility = Visibility.Collapsed;
            gdAddProfile.Visibility = Visibility.Visible;
            txbName.Text = string.Empty;
            txbName.Focus();
            _vm.TooltipVisibility = Visibility.Visible;
        }

        private void NameTextChanged(object sender, TextChangedEventArgs e)
        {
            var txt = txbName.Text.Trim();
            if (string.IsNullOrEmpty(txt))
            {
                btnSave.IsEnabled = false;
                return;

            }

            if (_vm!.ProfileIDs.ContainsKey(txt) && txt != EditingProfileName)
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

        private void CancelClick(object sender, MouseButtonEventArgs e)
        {
            gdBattery.Visibility = Visibility.Visible;
            gdAddProfile.Visibility = Visibility.Collapsed;
            btnPreset_Click(this, null);
            if (EditMode == "EDIT")
            {
                _vm!.CurrentProfileName = EditingProfileName;
                _vm.SetProfile();
            }
            txtCaption.Text = _vm!.Name;
            _vm.EnableVBar();
            _vm.TooltipVisibility = Visibility.Collapsed;
        }

        private void SaveClick(object sender, MouseButtonEventArgs e)
        {
            var txt = txbName.Text.Trim();
            //DdpmCommonHelper.DeviceManagerSA!.CreateCustomProfile(_vm!.CurrentDeviceInfo!.ID.ToString(), $"Test {_vm.WebcamSettings.CustomProfiles.Count + 1}");
            _vm!.CurrentProfile.Name = txt;
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
                            NewProfiles.Add(txt, JsonConvert.DeserializeObject<WebcamProfile>(JsonConvert.SerializeObject(_vm!.CurrentProfile))!);
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
                var profile = JsonConvert.DeserializeObject<WebcamProfile>(JsonConvert.SerializeObject(_vm!.CurrentProfile))!;
                //NewProfiles.Add(txt, profile);
                //_vm.WebcamSettings.CustomProfiles = NewProfiles.Concat(_vm.WebcamSettings.CustomProfiles!).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
                _vm.WebcamSettings.CustomProfiles.Add(_vm!.CurrentProfile.Name, profile);
            }
            _vm.CurrentProfileName = txt;
            WebcamSettings.ExportWebcamSettings(_vm.WebcamSettings, _vm.Model);
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
        }

        private bool CheckPresenceDetection_UI()
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

            string strComputerManufacturer = string.Empty;

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
        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdatePVMargin(_vm!.VbarSelectedIndex);
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
        }

        private void ChangeDevNameWidth()
        {
            txtCaption.Width = this.ActualWidth - RightGrid.ActualWidth - VbarGrid.ActualWidth - 100;
        }

        private void RightFrame_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ChangeDevNameWidth();
        }
    }
}