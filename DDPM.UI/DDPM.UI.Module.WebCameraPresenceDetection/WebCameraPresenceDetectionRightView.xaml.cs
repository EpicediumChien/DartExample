using DDPM.SA.Common.Settings;
using DDPM.UI.Common;
using DDPM.UI.Plugin.ViewModels;
using System.Diagnostics;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Input;
using UserControl = System.Windows.Controls.UserControl;
using DDPM.SA.Common;

namespace DDPM.UI.Module.WebCameraPresenceDetection
{
    /// <summary>
    /// Interaction logic for WebCameraSettingsRightView.xaml
    /// </summary>
    public partial class WebCameraPresenceDetectionRightView : UserControl
    {
        private readonly WebCameraViewModel _vm;

        public WebCameraPresenceDetectionRightView(WebCameraViewModel vm)
        {
            InitializeComponent();
            _vm = vm;

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
                SnoozeLength = 30
            });

            _vm.SnoozeLength_ItemsCollection.Add(new UI_SnoozeLength
            {
                SnoozeLength = 60
            });

            _vm.SnoozeLength_ItemsCollection.Add(new UI_SnoozeLength
            {
                SnoozeLength = 90
            });

            _vm.SnoozeLength_ItemsCollection.Add(new UI_SnoozeLength
            {
                SnoozeLength = 120
            });

           
            bool blRes = false;

            blRes = DdpmCommonHelper.DeviceManagerSA!.GetIsProximitySensorEnable(_vm.CurrentDeviceInfo!.ID.ToString()).Result;
            _vm.IsChecked_ProximitySensor = blRes;

            blRes = DdpmCommonHelper.DeviceManagerSA!.GetIsWakeonApproachEnable(_vm.CurrentDeviceInfo!.ID.ToString()).Result;
            _vm.IsChecked_WakeOnApproach = blRes;

            blRes = DdpmCommonHelper.DeviceManagerSA!.GetIsWalkAwayLockEnable(_vm.CurrentDeviceInfo!.ID.ToString()).Result;
            _vm.IsChecked_WalkAwayLock = blRes;

            int nRes = -1;
          
            nRes = DdpmCommonHelper.DeviceManagerSA!.GetWALTime(_vm.CurrentDeviceInfo!.ID.ToString()).Result;

            if (nRes != 30 && nRes != 60 && nRes != 120)
                nRes = 60;

            _vm.SelectedDelay = _vm.Delay_ItemsCollection.Find(x => (x.Delay == nRes));

            nRes = DdpmCommonHelper.DeviceManagerSA!.GetSnooze(_vm.CurrentDeviceInfo!.ID.ToString()).Result;
            //nRes = DdpmCommonHelper.DeviceManagerSA!.GetSnooze(_vm.CurrentDeviceInfo!.ID).Result;

            if (nRes > 0)
                _vm.IsChecked_Snooze = true;
            else
                _vm.IsChecked_Snooze = false;

            nRes = DdpmCommonHelper.DeviceManagerSA!.GetSnoozeLength(_vm.CurrentDeviceInfo!.ID.ToString()).Result;
            //nRes = DdpmCommonHelper.DeviceManagerSA!.GetSnoozeLength(_vm.CurrentDeviceInfo!.ID).Result;

            if (nRes != 30 && nRes != 60 && nRes != 90 && nRes != 120)
                nRes = 60;

             _vm.SelectedSnoozeLength = _vm.SnoozeLength_ItemsCollection.Find(x => (x.SnoozeLength == nRes));


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
        }

        ~WebCameraPresenceDetectionRightView()
        {
            if (DdpmCommonHelper.DeviceManagerSA != null)
            {
                DdpmCommonHelper.DeviceManagerSA.ITSettingsActionEvent -= DeviceManagerSA_ITSettingsActionEvent;
            }
        }
        private void DeviceManagerSA_ITSettingsActionEvent(object? sender, SA.Common.ITSettingEventArgs e)
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
            var psi = new System.Diagnostics.ProcessStartInfo();

            psi.FileName = "ms-settings:signinoptions-launchfaceenrollment";
            psi.UseShellExecute = true;

            System.Diagnostics.Process.Start(psi);
        }

        private void Image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Image elm)
            {
                var val = elm.Tag!.ToString();
                if (val == "0")
                    _vm.Undo();
                else
                    _vm.Redo();
            }
        }
    }
}