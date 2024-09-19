using CommunityToolkit.Mvvm.Input;
using DDPM.SA.Common.Settings;
using DDPM.UI.Common;
using DDPM.UI.Interfaces;
using DDPM.UI.Module.WebCameraCapture;
using DDPM.UI.Module.WebCameraColorImage;
using DDPM.UI.Module.WebCameraMicrophone;
using DDPM.UI.Module.WebCameraPresenceDetection;
using DDPM.UI.Module.WebCameraSettings;
using DDPM.UI.Plugin.Common;
using DDPM.UI.Plugin.ViewModels;
using Dell.Client.Framework.UX.WPF.Controls;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Windows.Devices.Sensors;
using Windows.Foundation;
using Windows.Graphics.Imaging;
using Windows.Media.Capture;
using Windows.Media.Capture.Frames;
using Windows.Media.MediaProperties;
using Windows.Storage;
using Windows.UI.Popups;
using BitmapEncoder = Windows.Graphics.Imaging.BitmapEncoder;
using LangHelper = DDPM.UI.Resources.Helper.LangHelper;

namespace DDPM.UI.Plugin.WebCameraPlugin
{
    /// <summary>
    /// WebCameraPlugin.xaml 的互動邏輯
    /// </summary>
    public partial class LaunchView : System.Windows.Controls.UserControl
    {
        /*
        // MediaCapture and its state variables
        private MediaCapture? _mediaCapture;
        private MediaFrameReader _mediaFrameReader;

        // 20240626 jim add
        private bool captureManagerInitialized = false;
        private bool _running = false;
        private bool _isRecording;

        // Folder in which the captures will be stored (initialized in SetupUiAsync)
        private StorageFolder _captureFolder;
        */

        // Rotation metadata to apply to the preview stream and recorded videos (MF_MT_VIDEO_ROTATION)
        // Reference: http://msdn.microsoft.com/en-us/library/windows/apps/xaml/hh868174.aspx
        private static readonly Guid RotationKey = new Guid("C380465D-2271-428C-9B83-ECEA3B4A85C1");

        private readonly WebCameraViewModel? _vm;

        private readonly int[] _rightFrameWidth = new int[] { 0, 483, 483, 483, 483, 483 };
        private readonly string CameraControl = Strings.CameraControl;
        private readonly string ColorandImage = Strings.ColorandImage;
        private readonly string PresenceDetection = Strings.PresenceDetection;
        private readonly string Capture = Strings.Capture;
        private readonly string Microphone = Strings.Microphone;

        private DispatcherTimer _timer = new();
        private int _countdownValue;
        private bool IsPresetOpen = false;

        //private readonly string[] PresetNames = [LangHelper.Instance["Default"], LangHelper.Instance["Camera.10"], LangHelper.Instance["Camera.9"], LangHelper.Instance["Camera.8"]];
        private readonly string[] PresetNames = [LangHelper.Instance["Default"], Strings.Smooth, Strings.Vibrant, Strings.Warm];

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

                //txtPreset.Text = $"{LangHelper.Instance["Camera.7"]} {_vm.CurrentProfileName}";
                txtPreset.Text = $"{Strings.Preset}: {_vm.CurrentProfileName}";
                txtAddPreset.Text = LangHelper.Instance["Camera.5"];

                //ProfileItems.ItemsSource = _vm.ProfileNames;
                ProfileItems.ItemsSource = _vm.ProfileItems;
            }
        }

        //  Jim remove 20240626
        private async void LaunchView_Unloaded(object sender, RoutedEventArgs e)
        {
            try
            {
                await CleanupMediaCaptureAsync();
            }
            catch (Exception Exc)
            {
                Debug.WriteLine("MediaCapture CleanupMediaCaptureAsync failed: " + Exc.Message);
            }
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

            if (_vm!.Model == "WB7022" || _vm.Model == "P2424HEB")
            {
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

            if (newItem.Text == LangHelper.Instance["Camera.4"])
            {
                _ = CleanupMediaCaptureAsync();
                imgDevice.Visibility = Visibility.Visible;
                gridPreview.Visibility = Visibility.Hidden;
            }
            else
            {
                Preview();
                imgDevice.Visibility = Visibility.Hidden;
                gridPreview.Visibility = Visibility.Visible;
            }
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

            _vm.RightFrameWidthTo = 0;
            _vm.RightFrameWidthFrom = _rightFrameWidth[_vm.VbarSelectedIndex + 1];
            InvokeGotoTwoViewModeAnimation();

            _vm.VbarSelectedIndex = -1;
            _vm.SetLadningMode(true);
            _vm.SelectVBar();

            _ = CleanupMediaCaptureAsync();
            imgDevice.Visibility = Visibility.Visible;
            gridPreview.Visibility = Visibility.Hidden;
            if (IsPresetOpen)
            { btnPreset_Click(this, null); }
        }

        MediaCaptureFailedEventHandler handler = (sender, e) =>
        {
            System.Threading.Tasks.Task task = System.Threading.Tasks.Task.Run(async () =>
            {
                await new MessageDialog("There was an error capturing the video from camera.", "Error").ShowAsync();
            });
        };

        // 20240626 jim add
        private async void Preview()
        {
            if (_vm!.captureManagerInitialized)
            { return; }

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
                        if (selectedFrameSourceGroup.DisplayName.ToUpper().Contains(_vm.CurrentDeviceInfo.ModelNumber.ToUpper()))
                            break;
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

                _vm!._mediaCapture = new MediaCapture();
                _vm._mediaCapture.Failed += handler;

                try
                {
                    await _vm._mediaCapture.InitializeAsync(new MediaCaptureInitializationSettings()
                    {
                        SourceGroup = selectedFrameSourceGroup,
                        SharingMode = MediaCaptureSharingMode.ExclusiveControl,
                        //SharingMode = MediaCaptureSharingMode.SharedReadOnly,
                        MemoryPreference = MediaCaptureMemoryPreference.Cpu,
                        StreamingCaptureMode = StreamingCaptureMode.Video
                    });
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("MediaCapture initiate fail: " + ex.Message);
                    return;
                }

                MediaFrameSource mediaFrameSource = _vm._mediaCapture.FrameSources[frameSourceInfo.Id];

                // 20240626 jim modify
                _vm._mediaFrameReader = await _vm._mediaCapture.CreateFrameReaderAsync(mediaFrameSource, MediaEncodingSubtypes.Argb32);

                _vm._mediaFrameReader.FrameArrived += MediaFrameReader_FrameArrived;

                await _vm._mediaFrameReader.StartAsync();

                // Query all properties [resolution and frame rate] of the webcam device
                _vm.allProperties = _vm._mediaCapture.VideoDeviceController.GetAvailableMediaStreamProperties(MediaStreamType.VideoPreview).Select(x => new StreamResolution(x));

                // Order them by resolution then frame rate
                _vm.allProperties = _vm.allProperties.OrderByDescending(x => x.Height * x.Width).ThenByDescending(x => x.FrameRate);

                // jim add 20240626
                _vm.captureManagerInitialized = true;

                _vm.SetResolution_Selected(1);
                _vm.SetFPS_Selected(1);

            }
            catch (Exception Exc)
            {
                Debug.WriteLine("MediaCapture initialization failed: " + Exc.Message);
            }
        }

        /// <summary>
        /// MediaFrameReader FrameArrived event
        /// </summary>
        private void MediaFrameReader_FrameArrived(MediaFrameReader sender, MediaFrameArrivedEventArgs args)
        {
            using var latestFrameReference = sender.TryAcquireLatestFrame();

            // 2024060626 jim modify to avoid exception
            var videoMediaFrame = latestFrameReference?.VideoMediaFrame;
            var softwareBitmap = videoMediaFrame?.SoftwareBitmap;

            // 2024060626 jim modify to avoid exception
            if (softwareBitmap != null)
            {
                if (softwareBitmap.BitmapPixelFormat != BitmapPixelFormat.Bgra8 ||
                    softwareBitmap.BitmapAlphaMode != BitmapAlphaMode.Premultiplied)
                {
                    softwareBitmap = SoftwareBitmap.Convert(softwareBitmap, BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);
                }

                CameraImage.Dispatcher.BeginInvoke(async () =>
                {
                    if (_vm._running)
                        return;
                    _vm._running = true;

                    CameraImage.Source = await ConvertSoftwareBitmap2BitmapImage(softwareBitmap);

                    _vm._running = false;
                });
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

        private async void StartRecord()
        {
            // jim add 20240625

            if (!_vm._isRecording)
            {
                if (_vm.Countdown_IsChecked)
                {
                    _countdownValue = 3; // 設置倒數起始值
                    CountdownText.Text = _countdownValue.ToString();


                    _timer = new DispatcherTimer();
                    _timer.Interval = TimeSpan.FromSeconds(1);
                    _timer.Tick += Timer_Tick;
                    _timer.Start();
                }
                else
                    StartRecordingAsync().RunSynchronously();

                //_timer.Interval = TimeSpan.FromSeconds(1);
                //_timer.Tick += Timer_Tick;
                //_timer.Start();

                //System.Threading.Thread.Sleep(3000);
                //await StartRecordingAsync();

            }
            else
            {
                await StopRecordingAsync();
            }
        }

        private void StopRecord()
        {

        }
        private void Timer_Tick(object sender, EventArgs e)
        {
            _countdownValue--;
            if (_countdownValue > 0)
            {
                CountdownText.Text = _countdownValue.ToString();
            }
            else
            {
                CountdownText.Text = "";
                StartRecordingAsync().RunSynchronously();
                _timer.Stop();
            }
        }

        /// <summary>
        /// Records an MP4 video to a StorageFile and adds rotation metadata to it
        /// </summary>
        /// <returns></returns>
        private async Task StartRecordingAsync()
        {
            try
            {
                //var picturesLibrary = await StorageLibrary.GetLibraryAsync(KnownLibraryId.Pictures);
                // Fall back to the local app storage if the Pictures Library is not available
                //_vm._captureFolder = picturesLibrary.SaveFolder ?? ApplicationData.Current.LocalFolder;
                _vm._captureFolder = await StorageFolder.GetFolderFromPathAsync(_vm.Media_File_Location);

                // Create storage file for the capture
                var videoFile = await _vm._captureFolder.CreateFileAsync("SimpleVideo.mp4", CreationCollisionOption.GenerateUniqueName);

                var encodingProfile = MediaEncodingProfile.CreateMp4(VideoEncodingQuality.Auto);

                // Calculate rotation angle, taking mirroring into account if necessary
                var rotationAngle = 360 - ConvertDeviceOrientationToDegrees(GetCameraOrientation());
                encodingProfile.Video.Properties.Add(RotationKey, PropertyValue.CreateInt32(rotationAngle));

                Debug.WriteLine("Starting recording to " + videoFile.Path);

                if (_vm._mediaCapture != null)
                    await _vm._mediaCapture.StartRecordToStorageFileAsync(encodingProfile, videoFile);

                _vm._isRecording = true;

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

            _vm._isRecording = false;

            if (_vm._mediaCapture != null)
                await _vm._mediaCapture.StopRecordAsync();

            Debug.WriteLine("Stopped recording!");

        }

        /// <summary>
        /// Resume recording a video
        /// </summary>
        /// <returns></returns>
        private async Task ResumeRecordingAsync()
        {
            Debug.WriteLine("Resuming recording...");

            _vm._isRecording = true;

            if (_vm._mediaCapture != null)
                await _vm._mediaCapture.ResumeRecordAsync();

            Debug.WriteLine("Resume recording!");
        }

        /// <summary>
        /// Pause recording a video
        /// </summary>
        /// <returns></returns>
        private async Task PauseRecordingAsync()
        {
            Debug.WriteLine("Pausing recording...");

            _vm._isRecording = true;

            if (_vm._mediaCapture != null)
            {
                MediaCapturePauseResult result =
                await _vm._mediaCapture.PauseRecordWithResultAsync(Windows.Media.Devices.MediaCapturePauseBehavior.RetainHardwareResources);
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
            if (_vm!._mediaCapture != null)
            {
                using (var mediaCapture = _vm._mediaCapture)
                {
                    _vm._mediaCapture = null;

                    _vm._mediaFrameReader.FrameArrived -= MediaFrameReader_FrameArrived;
                    await _vm._mediaFrameReader.StopAsync();
                    _vm._mediaFrameReader.Dispose();
                }
            }

            _vm.captureManagerInitialized = false;

            Debug.WriteLine("Media preview has canceled.");
        }

        private void btnRecord_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            btnPause.Visibility = Visibility.Visible;
            btnRecord.Visibility = Visibility.Collapsed;
            btnStop.Visibility = Visibility.Visible;
            StartRecord();

        }
        private void btnStop_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            btnPause.Visibility = Visibility.Collapsed;
            btnRecord.Visibility = Visibility.Visible;
            btnStop.Visibility = Visibility.Collapsed;
            StopRecord();
        }

        private void ProfileSelected(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            _vm!.CurrentProfileName = ((UXTextBlock)sender).Tag.ToString()!;
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

        }

        private void DeletePreset(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {

        }
    }
}