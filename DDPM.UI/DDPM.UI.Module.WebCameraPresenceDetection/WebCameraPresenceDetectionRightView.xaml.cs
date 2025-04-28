using DDPM.SA.Common.Settings;
using DDPM.UI.Common;
using DDPM.UI.Plugin.ViewModels;
using System.Diagnostics;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Input;
using UserControl = System.Windows.Controls.UserControl;
using DDPM.SA.Common;
using static System.Net.Mime.MediaTypeNames;
using System.Reflection;
using System.Windows.Threading;
using System.Xml.Linq;
using System.Windows.Forms;
using System.Linq.Expressions;
using static System.Net.WebRequestMethods;

namespace DDPM.UI.Module.WebCameraPresenceDetection
{
    /// <summary>
    /// Interaction logic for WebCameraSettingsRightView.xaml
    /// </summary>
    public partial class WebCameraPresenceDetectionRightView : UserControl
    {
        private readonly WebCameraViewModel _vm;

        private DispatcherTimer _timer;
        private int _countdown;

        public WebCameraPresenceDetectionRightView(WebCameraViewModel vm)
        {
            try
            {

                InitializeComponent();
                _vm = vm;

                // webcam event hander
                //_vm._log.Info($" subscribe event  {nameof(DdpmCommonHelper.DeviceManagerSA.Esi_IsCameraSensorCover_ChangeEvent)} , Handler Name: {nameof(OnEsi_IsCameraSensorCoverChangeHandler)} ");
                //DdpmCommonHelper.DeviceManagerSA.Esi_IsCameraSensorCover_ChangeEvent += OnEsi_IsCameraSensorCoverChangeHandler;

                //_vm._log.Info($" subscribe event  {nameof(DdpmCommonHelper.DeviceManagerSA.WALSnoozeTimeLeftInSeconds_ChangeEvent)} , Handler Name: {nameof(OnWALSnoozeTimeLeftInSecondsChangeHandler)} ");
                //DdpmCommonHelper.DeviceManagerSA.WALSnoozeTimeLeftInSeconds_ChangeEvent += OnWALSnoozeTimeLeftInSecondsChangeHandler;

                //_vm._log.Info($" subscribe event  {nameof(DdpmCommonHelper.DeviceManagerSA.Esi_IsWALLockCountdownStartedChanged_ChangeEvent)} , Handler Name: {nameof(OnEsi_IsWALLockCountdownStartedChangeHandler)} ");
                //DdpmCommonHelper.DeviceManagerSA.Esi_IsWALLockCountdownStartedChanged_ChangeEvent += OnEsi_IsWALLockCountdownStartedChangeHandler;

                //_vm._log.Info($" subscribe event  {nameof(DdpmCommonHelper.DeviceManagerSA.Esi_WALLockCountdownChanged_ChangeEvent)} , Handler Name: {nameof(OnEsi_WALLockCountdownChangeHandler)} ");
                //DdpmCommonHelper.DeviceManagerSA.Esi_WALLockCountdownChanged_ChangeEvent += OnEsi_WALLockCountdownChangeHandler;


                //_vm.UPD_Visibility = Visibility.Visible;
                //_vm.MPS_Setting_Visibility = Visibility.Collapsed;
                //_vm.MPS_UpdateFW_Visibility = Visibility.Collapsed;

                _vm.Delay_ItemsCollection = new List<UI_Delay_WalkAwayLock>();

                _vm.Delay_ItemsCollection.Add(new UI_Delay_WalkAwayLock
                {
                    Delay = 30
                });
                _vm.Delay_ItemsCollection.Add(new UI_Delay_WalkAwayLock
                {
                    Delay = 60
                });
                _vm.Delay_ItemsCollection.Add(new UI_Delay_WalkAwayLock
                {
                    Delay = 120
                });

                _vm.SnoozeLength_ItemsCollection = new List<UI_SnoozeLength>();

                _vm.SnoozeLength_ItemsCollection.Add(new UI_SnoozeLength
                {
                    SnoozeLength = 30,
                    SnoozeLength_sec = 1800
                });

                _vm.SnoozeLength_ItemsCollection.Add(new UI_SnoozeLength
                {
                    SnoozeLength = 60,
                    SnoozeLength_sec = 3600
                });

                _vm.SnoozeLength_ItemsCollection.Add(new UI_SnoozeLength
                {
                    SnoozeLength = 90,
                    SnoozeLength_sec = 5400
                });

                _vm.SnoozeLength_ItemsCollection.Add(new UI_SnoozeLength
                {
                    SnoozeLength = 120,
                    SnoozeLength_sec = 7200
                });


                bool blRes = false;
                if (DdpmCommonHelper.DeviceManagerSA != null)
                {
                    blRes = DdpmCommonHelper.DeviceManagerSA.GetIsProximitySensorEnable(_vm.CurrentDeviceInfo!.ID.ToString()).Result;
                    _vm.IsChecked_ProximitySensor = blRes;

                    blRes = DdpmCommonHelper.DeviceManagerSA.GetIsWakeonApproachEnable(_vm.CurrentDeviceInfo!.ID.ToString()).Result;
                    _vm.IsChecked_WakeOnApproach = blRes;

                    blRes = DdpmCommonHelper.DeviceManagerSA.GetIsWalkAwayLockEnable(_vm.CurrentDeviceInfo!.ID.ToString()).Result;
                    _vm.IsChecked_WalkAwayLock = blRes;

                    int nRes = -1;

                    nRes = DdpmCommonHelper.DeviceManagerSA.GetWALTime(_vm.CurrentDeviceInfo!.ID.ToString()).Result;

                    if (nRes != 30 && nRes != 60 && nRes != 120)
                        nRes = 60;

                    _vm.SelectedDelay = _vm.Delay_ItemsCollection.Find(x => (x.Delay == nRes));


                    nRes = DdpmCommonHelper.DeviceManagerSA.GetSnooze(_vm.CurrentDeviceInfo!.ID.ToString()).Result;
                    //nRes = DdpmCommonHelper.DeviceManagerSA.GetSnooze(_vm.CurrentDeviceInfo!.ID).Result;

                    if (nRes >= 0)
                        _vm.IsChecked_Snooze = true;
                    else
                        _vm.IsChecked_Snooze = false;

                    if (nRes < 0)
                        nRes = 30;
                    else if (nRes == 0)
                        nRes = 30;
                    else if (nRes == 1)
                        nRes = 60;
                    else if (nRes == 2)
                        nRes = 90;
                    else if (nRes == 3)
                        nRes = 120;

                    _vm.SelectedSnoozeLength = _vm.SnoozeLength_ItemsCollection.Find(x => (x.SnoozeLength == nRes));

                    if (_vm.IsChecked_Snooze == false)
                        DdpmCommonHelper.DeviceManagerSA.SetSnooze(-1, _vm.CurrentDeviceInfo!.ID);
                }

                //lock/unlock init, 9/23 add lock
                if (DdpmCommonHelper.DeviceManagerSA != null)
                {
                    DdpmCommonHelper.DeviceManagerSA.ITSettingsActionEvent += DeviceManagerSA_ITSettingsActionEvent;

                    DDPMSettings data = DdpmCommonHelper.DeviceManagerSA.ReloadAppConfigData().Result;
                    if (data != null)
                    {
                        vm.ShowLockMask = data.LockSettings.Lock_Webcam_PresenceDetection;
                        vm.IsTabStoppable = !data.LockSettings.Lock_Webcam_PresenceDetection;

                        if (vm.ShowLockMask)
                            vm.TabNavigation = "None";
                        else
                            vm.TabNavigation = "Cycle";

                        vm.LockMaskVisible = vm.ShowLockMask ? Visibility.Visible : Visibility.Collapsed;

                        //if (data.LockSettings.Lock_Webcam_PresenceDetection)
                        //{
                        //_vm.lockIcon = Visibility.Visible;
                        //_vm.viewMask = Visibility.Visible;
                        //_vm.tabStop = false;
                        //}
                        //else
                        //{
                        //_vm.lockIcon = Visibility.Collapsed;
                        //_vm.viewMask = Visibility.Collapsed;
                        //_vm.tabStop = true;
                        //}
                    }
                }

                //Added by Derek for Webcam
                //PIMS 319334
                //[DDPM Win 2.0][R15 webcam] Proximity Sensor in Presence Detection default is not disable.
                SetUPDToDefaultStatus();
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog("DDPM.UI.Module.WebCameraPresenceDetection\\WebCameraPresenceDetectionRightView.xaml.cs WebCameraPresenceDetectionRightView() ex:" + ex.Message);
            }
        }

        private void SetUPDToDefaultStatus()
        {
            //_vm.IsChecked_ProximitySensor = false;

            // jim add for PIMS-328192
            //_vm.IsChecked_WakeOnApproach = false;
            //_vm.IsChecked_WalkAwayLock = false;
        }

        ~WebCameraPresenceDetectionRightView()
        {
            // Add to unsubscribe corresponding event if there is an event subscribe
            if (DdpmCommonHelper.DeviceManagerSA != null)
            {
                DdpmCommonHelper.DeviceManagerSA.ITSettingsActionEvent -= DeviceManagerSA_ITSettingsActionEvent;
                // unsubscribe webcam event

                //_vm._log.Info($" unsubscribe event  {nameof(DdpmCommonHelper.DeviceManagerSA.Esi_IsCameraSensorCover_ChangeEvent)} , Handler Name: {nameof(OnEsi_IsCameraSensorCoverChangeHandler)} ");
                //DdpmCommonHelper.DeviceManagerSA.Esi_IsCameraSensorCover_ChangeEvent -= OnEsi_IsCameraSensorCoverChangeHandler;

                //_vm._log.Info($" unsubscribe event  {nameof(DdpmCommonHelper.DeviceManagerSA.WALSnoozeTimeLeftInSeconds_ChangeEvent)} , Handler Name: {nameof(OnWALSnoozeTimeLeftInSecondsChangeHandler)} ");
                //DdpmCommonHelper.DeviceManagerSA.WALSnoozeTimeLeftInSeconds_ChangeEvent -= OnWALSnoozeTimeLeftInSecondsChangeHandler;

                //_vm._log.Info($" unsubscribe event  {nameof(DdpmCommonHelper.DeviceManagerSA.Esi_IsWALLockCountdownStartedChanged_ChangeEvent)} , Handler Name: {nameof(OnEsi_IsWALLockCountdownStartedChangeHandler)} ");
                //DdpmCommonHelper.DeviceManagerSA.Esi_IsWALLockCountdownStartedChanged_ChangeEvent -= OnEsi_IsWALLockCountdownStartedChangeHandler;

                //_vm._log.Info($" unsubscribe event  {nameof(DdpmCommonHelper.DeviceManagerSA.Esi_WALLockCountdownChanged_ChangeEvent)} , Handler Name: {nameof(OnEsi_WALLockCountdownChangeHandler)} ");
                //DdpmCommonHelper.DeviceManagerSA.Esi_WALLockCountdownChanged_ChangeEvent -= OnEsi_WALLockCountdownChangeHandler;
            }
        }
        private void DeviceManagerSA_ITSettingsActionEvent(object? sender, SA.Common.ITSettingEventArgs e)
        {
            try
            {
                //bool? rst = DdpmCommonHelper.GetUINotifyPropertyValue_Boolean("Lock_Webcam_PresenceDetection", e);
                bool? isLocked = DdpmCommonHelper.GetUINotifyPropertyValue_Boolean("Lock_Webcam_PresenceDetection", e);

                if (isLocked != null)
                {
                    Dispatcher.Invoke(new Action(() =>
                    {
                        if (_vm != null)
                        {
                            _vm.IsTabStoppable = !(bool)isLocked;
                            _vm.ShowLockMask = (bool)isLocked;

                            if (_vm.ShowLockMask)
                                _vm.TabNavigation = "None";
                            else
                                _vm.TabNavigation = "Cycle";

                            _vm.LockMaskVisible = (bool)isLocked ? Visibility.Visible : Visibility.Collapsed;
                            Trace.WriteLine($"[SettingsPage] WebCameraPresenceDetectionRightView(Lock) : {isLocked}");
                        }
                    }));
                }

                /*
                Dispatcher.Invoke(new Action(() =>
                {
                    //if (_vm != null)
                    {
                        bool locked = false;
                        if (rst != null && rst == true)
                            locked = true;

                        //_vm.lockIcon = locked ? Visibility.Visible : Visibility.Collapsed;
                        //_vm.viewMask = locked ? Visibility.Visible : Visibility.Collapsed;
                        //_vm.tabStop = !locked;
                    }
                }));
                */
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog("DDPM.UI.Module.WebCameraPresenceDetection\\WebCameraPresenceDetectionRightView.xaml.cs DeviceManagerSA_ITSettingsActionEvent() ex:" + ex.Message);
            }
        }

        private void Slider_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {
            _vm.IsSliderDragging = true;
        }

        private void TipSlider_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            _vm.IsSliderDragging = false;
            //_vm.SetDPIValue();
        }

        private void TiltSlider_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            _vm.IsSliderDragging = false;
            //_vm.SetTouchScrollSensitivityLevel();
        }

        private void btnPair_Click(object sender, RoutedEventArgs e)
        {
        }

        private void CallWindowsHello_Click(object sender, RoutedEventArgs e)
        {

            try
            {
                var psi = new System.Diagnostics.ProcessStartInfo();

                psi.FileName = "ms-settings:signinoptions-launchfaceenrollment";
                psi.UseShellExecute = true;

                System.Diagnostics.Process.Start(psi);
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog("DDPM.UI.Module.WebCameraPresenceDetection\\WebCameraPresenceDetectionRightView.xaml.cs CallWindowsHello_Click() ex:" + ex.Message);
            }
        }

        private void CallPresenceSensor_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var psi = new System.Diagnostics.ProcessStartInfo();

                psi.FileName = "ms-settings:presence"; // Jim 20241223 Modify for PIMS-319086 [DDPM Win 2.0][R15 webcam] Click "Windows setting" in DDPM Presence Detection link is wrong, when SUT Support HPD_MPS, DUT is with HPD_MPS FW.
                psi.UseShellExecute = true;

                System.Diagnostics.Process.Start(psi);
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog("DDPM.UI.Module.WebCameraPresenceDetection\\WebCameraPresenceDetectionRightView.xaml.cs CallPresenceSensor_Click() ex:" + ex.Message);
            }
        }

        private void CallUpdateMPSFW_Click(object sender, RoutedEventArgs e)
        {
            //Derek 1116 for Webcam PIMS 319078
            //string url = "https://www.dell.com/";
            string url = "https://www.dell.com/support/wb7022/downloads"; // Jim 20250123 modify for PIMS-319078
            // Open the browser and navigate to specified url
            //Process.Start(new ProcessStartInfo
            //{
            //    FileName = url,
            //    UseShellExecute = true
            //});
            try
            {
                DDPM.SA.Common.Settings.DDPMFileSecurity.StartProcessSafely(
                null,
                new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog($"Catch exception[{ex.Message}] when open url: {url}");
            }

        }

        private void Image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (sender is Border elm)
                {
                    var val = elm.Tag!.ToString();
                    if (val == "0")
                        _vm.Undo();
                    else
                        _vm.Redo();
                }
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog("DDPM.UI.Module.WebCameraPresenceDetection\\WebCameraPresenceDetectionRightView.xaml.cs Image_MouseLeftButtonDown() ex:" + ex.Message);
            }
        }

        private void HandleSnoozeCheck(object sender, RoutedEventArgs e)
        {
            if (_vm.IsChecked_Snooze)
            {
                txtTimer.Visibility = Visibility.Visible;
                //_countdown = DdpmCommonHelper.DeviceManagerSA.GetSnoozeLength(_vm.CurrentDeviceInfo!.ID.ToString()).Result;
                //_timer = new DispatcherTimer();
                //_timer.Interval = TimeSpan.FromSeconds(1);
                //_timer.Tick += Timer_Tick;
                //_timer.Start();
            }
        }

        private void HandleSnoozeUnchecked(object sender, RoutedEventArgs e)
        {
            if (_vm.IsChecked_Snooze == false)
            {
                //_timer.Stop();
                txtTimer.Visibility = Visibility.Collapsed;
                txtTimer.Text = "00:00:00";
                //_countdown = 0;
            }

        }


        private void onSoozeLegthcbxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_vm.IsChecked_Snooze)
            {
                //_countdown = DdpmCommonHelper.DeviceManagerSA.GetSnoozeLength(_vm.CurrentDeviceInfo!.ID.ToString()).Result;             
            }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            _countdown--;
            TimeSpan ts = TimeSpan.FromSeconds(_countdown);

            txtTimer.Text = ts.ToString(@"hh\:mm\:ss");
        }

        //20250428 Chewlin add for tooltip issue fix
        private double GetScreenScaleX()
        {
            var source = PresentationSource.FromVisual(this);
            if (source?.CompositionTarget != null)
            {
                return source.CompositionTarget.TransformToDevice.M11;
            }
            return 1;
        }

        //20250428 Chewlin add for tooltip issue fix
        private void toolTip_Opened(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.ToolTip? target = sender as System.Windows.Controls.ToolTip;
            if (target == null)
                return;
            double screenScaleX = GetScreenScaleX();
            Window mainWindow = System.Windows.Application.Current.MainWindow;
            double winRightX = (mainWindow.Left + mainWindow!.ActualWidth) * screenScaleX;
            double mousepositionX = System.Windows.Forms.Cursor.Position.X;
            double mouseaddtooltip = mousepositionX + target.ActualWidth * screenScaleX;
            if (winRightX > mouseaddtooltip)
            {
                target.HorizontalOffset = 11;
            }
            else
            {
                target.HorizontalOffset = -1 * target.ActualWidth + 23;
            }
        }

        //private void OnEsi_IsCameraSensorCoverChangeHandler(object sender, bool e)
        //{
        //    _vm._log.Info($" Catch event {nameof(OnEsi_IsCameraSensorCoverChangeHandler)} , Caller Name: {nameof(DdpmCommonHelper.DeviceManagerSA.Esi_IsCameraSensorCover_ChangeEvent)} ");

        //    if (e) 
        //        DdpmCommonHelper.DeviceManagerSA.ShowOSD(Screen.PrimaryScreen!.DeviceName, OSDType.Fingerprint);
        //}

        //private void OnWALSnoozeTimeLeftInSecondsChangeHandler(object sender, int e)
        //{
        //    _vm._log.Info($" Catch event {nameof(OnWALSnoozeTimeLeftInSecondsChangeHandler)} , Caller Name: {nameof(DdpmCommonHelper.DeviceManagerSA.WALSnoozeTimeLeftInSeconds_ChangeEvent)} ");

        //    TimeSpan ts = TimeSpan.FromSeconds(e);

        //    txtTimer.Text = ts.ToString(@"hh\:mm\:ss");
        //}

        //private void OnEsi_IsWALLockCountdownStartedChangeHandler(object sender, bool e)
        //{
        //    _vm._log.Info($" Catch event {nameof(OnEsi_IsWALLockCountdownStartedChangeHandler)} , Caller Name: {nameof(DdpmCommonHelper.DeviceManagerSA.Esi_IsWALLockCountdownStartedChanged_ChangeEvent)} ");

        //    if (e)
        //        DdpmCommonHelper.DeviceManagerSA.ShowOSD(Screen.PrimaryScreen!.DeviceName, OSDType.WalkAwayLock);
        //}

        //private void OnEsi_WALLockCountdownChangeHandler(object sender, int e)
        //{
        //    _vm._log.Info($" Catch event {nameof(OnEsi_WALLockCountdownChangeHandler)} , Caller Name: {nameof(DdpmCommonHelper.DeviceManagerSA.Esi_WALLockCountdownChanged_ChangeEvent)} ");
        //}

    }
}