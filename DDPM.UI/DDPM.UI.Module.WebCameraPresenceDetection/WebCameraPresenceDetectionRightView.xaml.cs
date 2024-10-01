using DDPM.SA.Common.Settings;
using DDPM.UI.Common;
using DDPM.UI.Plugin.ViewModels;
using System.Diagnostics;
using System.Windows;
using UserControl = System.Windows.Controls.UserControl;

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
    }
}