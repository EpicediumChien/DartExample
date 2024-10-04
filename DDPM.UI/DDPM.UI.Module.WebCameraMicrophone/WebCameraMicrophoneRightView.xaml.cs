using DDPM.UI.Plugin.ViewModels;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows;
using DDPM.UI.Common;
using DDPM.SA.Common.Settings;
using System.Diagnostics;

namespace DDPM.UI.Module.WebCameraMicrophone
{
    /// <summary>
    /// Interaction logic for WebCameraMicrophoneRightView.xaml
    /// </summary>
    public partial class WebCameraMicrophoneRightView : UserControl
    {
        private readonly WebCameraViewModel _vm;

        public WebCameraMicrophoneRightView(WebCameraViewModel vm)
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
                    vm.ShowLockMask = data.LockSettings.Lock_Webcam_MicSwitch;
                    vm.IsTabStoppable = !data.LockSettings.Lock_Webcam_MicSwitch;

                    if (vm.ShowLockMask)
                        vm.TabNavigation = "None";
                    else
                        vm.TabNavigation = "Cycle";

                    vm.LockMaskVisible = vm.ShowLockMask ? Visibility.Visible : Visibility.Collapsed;


                    //if (data.LockSettings.Lock_Webcam_MicSwitch)
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

        ~WebCameraMicrophoneRightView()
        {
            if (DdpmCommonHelper.DeviceManagerSA != null)
            {
                DdpmCommonHelper.DeviceManagerSA.ITSettingsActionEvent -= DeviceManagerSA_ITSettingsActionEvent;
            }
        }
        private void DeviceManagerSA_ITSettingsActionEvent(object? sender, SA.Common.ITSettingEventArgs e)
        {
            //bool? rst = DdpmCommonHelper.GetUINotifyPropertyValue_Boolean("Lock_Webcam_MicSwitch", e);
            bool? isLocked = DdpmCommonHelper.GetUINotifyPropertyValue_Boolean("Lock_Webcam_MicSwitch", e);

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
                        Trace.WriteLine($"[SettingsPage] WebCameraMicrophoneRightView(Lock) : {isLocked}");
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

        private void UXToggleSwitch_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            _vm.CurrentCursor = Cursors.Wait;
            _vm.IsMicEnumerationOnEnabled = false;
            _vm.AlertType = WebcamAlert.Alert1;
            _vm.AlertVisibility = Visibility.Visible;
        }

        private void Image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Image elm)
            {
                var val = elm.Tag.ToString();
                if (val == "0")
                    _vm.Undo();
                else
                    _vm.Redo();
            }
        }
    }
}