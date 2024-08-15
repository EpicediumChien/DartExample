using CommunityToolkit.Mvvm.Input;
using DDPM.UI.Common;
using DDPM.UI.Interfaces;
using DDPM.UI.Module.WebCameraCapture;
using DDPM.UI.Module.WebCameraColorImage;
using DDPM.UI.Module.WebCameraMicrophone;
using DDPM.UI.Module.WebCameraPresenceDetection;
using DDPM.UI.Module.WebCameraSettings;
using DDPM.UI.Plugin.Common;
using DDPM.UI.Plugin.ViewModels;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Windows;
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
using BitmapEncoder = Windows.Graphics.Imaging.BitmapEncoder;

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

        private readonly int[] _rightFrameWidth = new int[] { 0, 500, 500, 500, 500, 500 };//SDL, change to use new
        private readonly string Restore = "Restore to default";
        private readonly string Unpair = "Unpair";
        private readonly Style ConnectionStyle1;
        private readonly Style ConnectionStyle2;
        private readonly BitmapImage img1 = new(new Uri($"/DDPM.UI.Resources;component/Resources/Images/Bluetooth.png", UriKind.Relative));
        private readonly BitmapImage img2 = new(new Uri($"/DDPM.UI.Resources;component/Resources/Images/Bluetooth2.png", UriKind.Relative));

        private readonly string WebCameraButton = "Button\nCustomization";

        // 20240731
        private DispatcherTimer _timer;

        private int _countdownValue;

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
            }

            txtUnpair.Text = Unpair;
            txtRestore.Text = Restore;

            ConnectionStyle1 = (Style)FindResource("ConnectionStyle1");
            ConnectionStyle2 = (Style)FindResource("ConnectionStyle2");
            txtSystemName1.Text = _vm!.VisiblePairedHostName1;
            txtSystemName2.Text = _vm.VisiblePairedHostName1;
            txtSystemName3.Text = _vm.VisiblePairedHostName1;
            txtFirmware.Text = "Dongle " + _vm.PhysicalDeviceFWVersion;
            //txtSlot.Text = $"{_vm.CurrentDeviceInfo!.MaxPairingSlots - _vm.CurrentDeviceInfo.PairedDeviceCount} of {_vm.CurrentDeviceInfo.MaxPairingSlots} slots available";
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
                GroupName = "Camera Control",
                GroupIcon = DdpmCommonHelper.GetImageSourceFromCommonResource("Resources/Images/CameraControl.png", "DDPM.UI.Resources")
            };
            moduleGroup.AddHeader("Camera Control", new WebCameraSettingsModule(_vm!));
            groups.Add(moduleGroup);

            moduleGroup = new ModuleGroup()
            {
                GroupName = "Color and Image",
                GroupIcon = DdpmCommonHelper.GetImageSourceFromCommonResource("Resources/Images/CameraColorImage.png", "DDPM.UI.Resources")
            };
            moduleGroup.AddHeader("Color and Image", new WebCameraColorImageModule(_vm!));
            groups.Add(moduleGroup);

            moduleGroup = new ModuleGroup()
            {
                GroupName = "Presence Detection",
                GroupIcon = DdpmCommonHelper.GetImageSourceFromCommonResource("Resources/Images/CameraPresenceDetection.png", "DDPM.UI.Resources")
            };
            moduleGroup.AddHeader("Presence Detection", new WebCameraPresenceDetectionModule(_vm!));
            groups.Add(moduleGroup);

            moduleGroup = new ModuleGroup()
            {
                GroupName = "Capture",
                GroupIcon = DdpmCommonHelper.GetImageSourceFromCommonResource("Resources/Images/CameraCapture.png", "DDPM.UI.Resources")
            };
            moduleGroup.AddHeader("Capture", new WebCameraCaptureModule(_vm!));
            groups.Add(moduleGroup);
            moduleGroup = new ModuleGroup()
            {
                GroupName = "Microphone",
                GroupIcon = DdpmCommonHelper.GetImageSourceFromCommonResource("Resources/Images/Microphone.png", "DDPM.UI.Resources")
            };
            moduleGroup.AddHeader("Microphone", new WebCameraMicrophoneModule(_vm!));
            groups.Add(moduleGroup);

            _vm!.ModuleGroups = groups;
        }

        #endregion Init for Modules

        #region Vbar

        private void OnVbarItemClicked(VbarItem newItem)
        {
            if (newItem.Id == _vm!.VbarSelectedIndex) { return; }

            if (_rightFrameWidth[newItem.Id + 1] != _rightFrameWidth[_vm.VbarSelectedIndex + 1])
            {
                _vm.RightFrameWidthFrom = _rightFrameWidth[_vm.VbarSelectedIndex + 1];
                _vm.RightFrameWidthTo = _rightFrameWidth[newItem.Id + 1];

                InvokeGotoTwoViewModeAnimation();

                if (newItem.Id == 0)
                {
                    InvokeShrinkAnimation();
                }
                else if (_vm.VbarSelectedIndex == 0)
                {
                    InvokeEnlargeAnimation();
                }
            }

            _vm.VbarSelectedIndex = newItem.Id;

            if (_vm.RightViewHeaders != null)
            {
                rightViewHeaderCtrl.SetHeaders(_vm.RightViewHeaders.ToArray());
            }
            btnUnpair.Visibility = Visibility.Collapsed;
            _vm.SetLadningMode(false);
            _vm.SelectVBar();
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

        private void InvokeShrinkAnimation()
        {
            Dispatcher.Invoke(new Action(() =>
            {
                Storyboard sb = (Storyboard)this.FindResource("StoryShrink");
                if (sb != null)
                {
                    sb.Completed += (o, s) =>
                    {
                    };

                    sb.Begin();
                }
            }));
        }

        private void InvokeEnlargeAnimation()
        {
            Dispatcher.Invoke(new Action(() =>
            {
                Storyboard sb = (Storyboard)this.FindResource("StoryEnlarge");
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

        private void Unpair_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (_vm!.ConnectionType == "Dongle")
            {
                UnpairModalDialog unpairModalDialog = new(eDeviceCategory.KB);
                Window parentWindow = Window.GetWindow(this);
                if (parentWindow != null)
                {
                    unpairModalDialog.Owner = parentWindow;
                }

                bool? dialogResult = unpairModalDialog.ShowDialog();
                if (dialogResult == true)
                {
                    _vm.Unpair();
                }
            }
            else
            {
                Version win10Version = new(10, 0);
                Version currentVersion = Environment.OSVersion.Version;
#pragma warning disable CA1416
                if (currentVersion >= win10Version)
                {
                    Process.Start(new ProcessStartInfo("ms-settings:bluetooth")
                    {
                        UseShellExecute = true
                    });
                }
                else
                {
                    Process.Start(new ProcessStartInfo("control", "bthprops.cpl")
                    {
                        UseShellExecute = true
                    });
                }
#pragma warning restore CA1416
            }
        }

        private void Mainframe_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (_vm!.VbarSelectedIndex == -1) { return; }

            _vm.RightFrameWidthTo = 0;
            _vm.RightFrameWidthFrom = _rightFrameWidth[_vm.VbarSelectedIndex + 1];
            InvokeGotoTwoViewModeAnimation();
            btnUnpair.Visibility = Visibility.Visible;
            if (_vm.VbarSelectedIndex == 0) { InvokeEnlargeAnimation(); }
            _vm.VbarSelectedIndex = -1;
            _vm.SetLadningMode(true);
            _vm.SelectVBar();
        }

        private void Restore_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            RestoreModalDialog restoreModalDialog = new();
            Window parentWindow = Window.GetWindow(this);
            if (parentWindow != null)
            {
                restoreModalDialog.Owner = parentWindow;
            }

            bool? dialogResult = restoreModalDialog.ShowDialog();
            if (dialogResult == true)
            {
                //MessageBox.Show("OK button was clicked");
            }
        }

        private void BatteryIndicator_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (_vm!.ConnectionType == "Dongle")
            {
                DongleConnection.Visibility = Visibility.Visible;
            }
            else
            {
                string hostName = Dns.GetHostName();
                if (_vm.VisiblePairedHostName1 == hostName)
                {
                    txt1.Style = ConnectionStyle1;
                    txt2.Style = ConnectionStyle2;
                    imgBL1.Source = img1;
                    imgBL2.Source = img2;
                    txtSystemName1.Style = ConnectionStyle1;
                    txtSystemName2.Style = ConnectionStyle2;
                }
                else
                {
                    txt1.Style = ConnectionStyle2;
                    txt2.Style = ConnectionStyle1;
                    imgBL1.Source = img2;
                    imgBL2.Source = img1;
                    txtSystemName1.Style = ConnectionStyle2;
                    txtSystemName2.Style = ConnectionStyle1;
                }
                BLConnection.Visibility = Visibility.Visible;
            }
        }

        private void BatteryIndicator_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            DongleConnection.Visibility = Visibility.Collapsed;
            BLConnection.Visibility = Visibility.Collapsed;
        }

        private void LargeImage_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
        }

        // 20240626 jim add
        private async void Button_Preview_Click(object sender, RoutedEventArgs e)
        {
            // 20240626 jim add
            if (_vm.captureManagerInitialized == true)
            {
                return;
            }

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

                _vm._mediaCapture = new MediaCapture();

                try
                {
                    await _vm._mediaCapture.InitializeAsync(new MediaCaptureInitializationSettings()
                    {
                        SourceGroup = selectedFrameSourceGroup,
                        //SharingMode = MediaCaptureSharingMode.ExclusiveControl,
                        SharingMode = MediaCaptureSharingMode.SharedReadOnly,
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

                // jim add 20240626
                _vm.captureManagerInitialized = true;
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
                    if (_vm._running) return;
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

        private async void Button_Record_Click(object sender, RoutedEventArgs e)
        {
            // jim add 20240625

            if (!_vm._isRecording)
            {
                _countdownValue = 3; // 設置倒數起始值
                CountdownText.Text = _countdownValue.ToString();

                _timer = new DispatcherTimer();
                _timer.Interval = TimeSpan.FromSeconds(1);
                _timer.Tick += Timer_Tick;
                _timer.Start();

                //System.Threading.Thread.Sleep(3000);
                //await StartRecordingAsync();
            }
            else
            {
                await StopRecordingAsync();
            }
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
                var picturesLibrary = await StorageLibrary.GetLibraryAsync(KnownLibraryId.Pictures);
                // Fall back to the local app storage if the Pictures Library is not available
                _vm._captureFolder = picturesLibrary.SaveFolder ?? ApplicationData.Current.LocalFolder;

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
            if (_vm._mediaCapture != null)
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
    }
}