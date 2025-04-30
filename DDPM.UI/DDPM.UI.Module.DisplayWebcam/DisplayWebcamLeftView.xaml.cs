using DDPM.PowerMon;
using DDPM.SA.Common.Settings;
using DDPM.UI.Common;
using DDPM.UI.Plugin.ViewModels;
using DDPM.UI.Resources.Helper;
using Dell.Client.Framework.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Windows.Graphics.Imaging;
using Windows.Media.Capture;
using Windows.Media.Capture.Frames;

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

        public DisplayWebcamLeftView(WebCameraViewModel webCameraViewModel)
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
        private void DeviceManagerSA_OnSystemSessionEnd(object? sender, EventArgs e)
        {
            DdpmCommonHelper.WriteUILog($"catch event DeviceManagerSA_OnSystemSessionEnd");
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
                    DdpmCommonHelper.WriteUILog("DDPM.UI.WebCameraPlugin\\Views\\LaunchView.xaml.cs StopRecordingAsync() : " + ex.Message);
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

        }

        private void ChangePan(object sender, MouseButtonEventArgs e)
        {

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
            if (_vm.CheckChar(e.Text))
                e.Handled = false;
            else
            {
                e.Handled = true;
                ShowAlert();
            }
        }

        private void ShowAlert()
        {
            bdrAlert2.Visibility = Visibility.Visible;
            AlertTimer.Stop();
            AlertTimer.Start();
        }
        private void txbName_LostFocus(object sender, RoutedEventArgs e)
        {
            bdrAlert2.Visibility = Visibility.Hidden;
        }

        private void NameTextChanged(object sender, TextChangedEventArgs e)
        {
            DdpmCommonHelper.WriteUILog("[DisplayWebcamLeftView]  NameTextChanged() start");
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
                DdpmCommonHelper.WriteUILog("[DisplayWebcamLeftView]  NameTextChanged() ex:" + ex.Message);
            }
            DdpmCommonHelper.WriteUILog("[DisplayWebcamLeftView]  NameTextChanged() end");
        }

        private void CancelClick(object sender, RoutedEventArgs e)
        {
            DdpmCommonHelper.WriteUILog("[DisplayWebcamLeftView]  CancelClick() start");
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
                //txtCaption.Text = _vm.Name;
                //_vm.EnableVBar();
                _vm.TooltipVisibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog("[DisplayWebcamLeftView]  CancelClick() ex: " + ex.Message);
            }
            DdpmCommonHelper.WriteUILog("[DisplayWebcamLeftView]  CancelClick() end");
        }

        private void SaveClick(object sender, RoutedEventArgs e)
        {
            DdpmCommonHelper.WriteUILog("[DisplayWebcamLeftView]  SaveClick() start");

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
                //txtCaption.Text = _vm.Name;
                _vm.ClearUndo();
                //_vm.EnableVBar();
                _vm.TooltipVisibility = Visibility.Collapsed;

                //Derek 2025/01/18
                DdpmCommonHelper.DeviceManagerSA?.SyncWebcamProfile(_vm.CurrentProfileName, false);
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog("[DisplayWebcamLeftView]  SaveClick() ex: " + ex.Message);
            }
            DdpmCommonHelper.WriteUILog("[DisplayWebcamLeftView]  SaveClick() end");
        }
    }
}
