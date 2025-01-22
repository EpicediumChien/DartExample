using DDPM.SA.Common;
using DDPM.SA.Common.Settings;
using DDPM.UI.Common;
using DDPM.UI.Plugin.ViewModels;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using UserControl = System.Windows.Controls.UserControl;

namespace DDPM.UI.Module.WebCameraColorImage
{
    /// <summary>
    /// Interaction logic for WebCameraSettingsRightView.xaml
    /// </summary>
    public partial class WebCameraColorImageRightView : UserControl
    {
        private WebCameraViewModel _vm;

        public WebCameraColorImageRightView(WebCameraViewModel vm)
        {
            try
            {
                InitializeComponent();
                _vm = vm;

                if (_vm.CurrentDeviceInfo!.IsPropertyWhiteBalanceSupported)
                {
                    AWBSlider.Maximum = _vm.CurrentDeviceInfo!.WhiteBalanceMax;
                    AWBSlider.Minimum = _vm.CurrentDeviceInfo.WhiteBalanceMin;
                    AWBSlider.TickFrequency = _vm.CurrentDeviceInfo!.WhiteBalanceSteppingDelta;
                }

                if (_vm.CurrentDeviceInfo!.IsPropertyBrightnessSupported)
                {
                    BrightnessSlider.Maximum = _vm.CurrentDeviceInfo!.BrightnessMax;
                    BrightnessSlider.Minimum = _vm.CurrentDeviceInfo.BrightnessMin;
                    BrightnessSlider.TickFrequency = _vm.CurrentDeviceInfo!.BrightnessSteppingDelta;
                    spBrightness.Visibility = Visibility.Visible;
                    BSCS.Visibility = Visibility.Visible;
                }

                if (_vm.CurrentDeviceInfo!.IsPropertySharpnessSupported)
                {
                    SharpnessSlider.Maximum = _vm.CurrentDeviceInfo!.SharpnessMax;
                    SharpnessSlider.Minimum = _vm.CurrentDeviceInfo.SharpnessMin;
                    SharpnessSlider.TickFrequency = _vm.CurrentDeviceInfo!.SharpnessSteppingDelta;
                    spSharpness.Visibility = Visibility.Visible;
                    BSCS.Visibility = Visibility.Visible;
                }

                if (_vm.CurrentDeviceInfo!.IsPropertyContrastSupported)
                {
                    ContrastSlider.Maximum = _vm.CurrentDeviceInfo!.ContrastMax;
                    ContrastSlider.Minimum = _vm.CurrentDeviceInfo.ContrastMin;
                    ContrastSlider.TickFrequency = _vm.CurrentDeviceInfo!.ContrastSteppingDelta;
                    spContrast.Visibility = Visibility.Visible;
                    BSCS.Visibility = Visibility.Visible;
                }

                if (_vm.CurrentDeviceInfo!.IsPropertySaturationSupported)
                {
                    SaturationSlider.Maximum = _vm.CurrentDeviceInfo!.SaturationMax;
                    SaturationSlider.Minimum = _vm.CurrentDeviceInfo.SaturationMin;
                    SaturationSlider.TickFrequency = _vm.CurrentDeviceInfo!.SaturationSteppingDelta;
                    spSaturation.Visibility = Visibility.Visible;
                    BSCS.Visibility = Visibility.Visible;
                }
            }
            catch (Exception ex) 
            {
                DdpmCommonHelper.WriteUILog("DDPM.UI.Module.WebColorImage\\WebCameraColorImageRightView.xaml.cs WebCameraColorImageRightView() ex:" + ex.Message);
            }
        }


        private void DeviceManagerSA_ITSettingsActionEvent(object? sender, SA.Common.ITSettingEventArgs e)
        {
            try
            {
                //bool? rst = DdpmCommonHelper.GetUINotifyPropertyValue_Boolean("Lock_Webcam_hdr", e);
                bool? isLocked = DdpmCommonHelper.GetUINotifyPropertyValue_Boolean("Lock_Webcam_hdr", e);

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
                            Trace.WriteLine($"[SettingsPage] WebCameraColorImageRightView(Lock) : {isLocked}");
                        }
                    }));
                }
            }
            catch (Exception ex) 
            {
                DdpmCommonHelper.WriteUILog("DDPM.UI.Module.WebColorImage\\WebCameraColorImageRightView.xaml.cs DeviceManagerSA_ITSettingsActionEvent() ex:" + ex.Message);
            }

            /*
            Dispatcher.Invoke(new Action(() =>
            {
                if (_vm != null)
                {
                    bool locked = false;
                    if (rst != null && rst == true)
                        locked = true;

                    //_vm.isHdrLocked = locked ? Visibility.Visible : Visibility.Collapsed;
                    //_vm.isHdrTabStopped = !locked;
                }
            }));
            rst = DdpmCommonHelper.GetUINotifyPropertyValue_Boolean("Lock_Webcam_AntiFlicker", e);
            Dispatcher.Invoke(new Action(() =>
            {
                if (_vm != null)
                {
                    bool locked = false;
                    if (rst != null && rst == true)
                        locked = true;

                    //_vm.isAntiLocked = locked ? Visibility.Visible : Visibility.Collapsed;
                    //_vm.isAntiTabStopped = !locked;
                }
            }));
            */
        }

        private void AWBSlider_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {
            _vm.IsSliderDragging = true;
        }

        private void AWBSlider_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            _vm.IsSliderDragging = false;
            _vm.SetAutoWhiteBalance();
        }

        private void BrightnessSlider_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {
            _vm.IsSliderDragging = true;
        }

        private void BrightnessSlider_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            _vm.IsSliderDragging = false;
            _vm.SetBrightness();

            //Derek 2025/01/17
            InfoQAMSetProfileToNone(0);
        }

        private void InfoQAMSetProfileToNone(int type)
        {
            try
            {
                string infoType = string.Empty;

                switch (type)
                {
                    case 0:
                        infoType = "DDPMSetProfileToNoneByBrightness";
                        break;

                    case 1:
                        infoType = "DDPMSetProfileToNoneBySharpness";
                        break;

                    case 2:
                        infoType = "DDPMSetProfileToNoneByContrast"; 
                        break;

                    case 3:
                        infoType = "DDPMSetProfileToNoneBySaturation";
                        break;
                }

                DdpmCommonHelper.DeviceManagerSA?.SyncWebcamProfile(infoType, false);
                _vm.isUIHasUpdateByQAM = false; //Derek 2025/01/22
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog($"InfoQAMSetProfileToNone catch exception: {ex.Message}");
            }
        }

        private void SharpnessSlider_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {
            _vm.IsSliderDragging = true;
        }

        private void SharpnessSlider_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            _vm.IsSliderDragging = false;
            _vm.SetSharpness();

            //Derek 2025/01/17
            InfoQAMSetProfileToNone(1);
        }

        private void ContrastSlider_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {
            _vm.IsSliderDragging = true;
        }

        private void ContrastSlider_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            _vm.IsSliderDragging = false;
            _vm.SetContrast();

            //Derek 2025/01/17
            InfoQAMSetProfileToNone(2);
        }

        private void SaturationSlider_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {
            _vm.IsSliderDragging = true;
        }

        private void SaturationSlider_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            _vm.IsSliderDragging = false;
            _vm.SetSaturation();

            //Derek 2025/01/17
            InfoQAMSetProfileToNone(3);
        }

        private void AntiFlicker_Click(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (sender is Border bdr)
                {
                    var val = int.Parse(bdr.Tag.ToString()!);
                    if (val == _vm.AntiFlicker)
                    { return; }

                    _vm.AntiFlicker = val;
                }
            }
            catch (Exception ex) 
            {
                DdpmCommonHelper.WriteUILog("DDPM.UI.Module.WebColorImage\\WebCameraColorImageRightView.xaml.cs AntiFlicker_Click() ex:" + ex.Message);
            }
        }

        private void Image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (sender is Border elm)
                {
                    var val = elm.Tag.ToString();
                    if (val == "0")
                        _vm.Undo();
                    else
                        _vm.Redo();
                }
            }
            catch ( Exception ex) 
            {
                    DdpmCommonHelper.WriteUILog("DDPM.UI.Module.WebColorImage\\WebCameraColorImageRightView.xaml.cs mage_MouseLeftButtonDown() ex:" + ex.Message);
            }
        }

        private void Slider_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            _vm.IsSliderDragging = true;
        }

        private void BrightnessSlider_KeyUp(object sender, KeyEventArgs e)
        {
            _vm.IsSliderDragging = false;
            _vm.SetBrightness();
        }

        private void SharpnessSlider_KeyUp(object sender, KeyEventArgs e)
        {
            _vm.IsSliderDragging = false;
            _vm.SetSharpness();
        }

        private void ContrastSlider_KeyUp(object sender, KeyEventArgs e)
        {
            _vm.IsSliderDragging = false;
            _vm.SetContrast();
        }

        private void SaturationSlider_KeyUp(object sender, KeyEventArgs e)
        {
            _vm.IsSliderDragging = false;
            _vm.SetSaturation();
        }

        private void AWBSlider_KeyUp(object sender, KeyEventArgs e)
        {
            _vm.IsSliderDragging = false;
            _vm.SetAutoWhiteBalance();
        }

        private void HDRToggleSwitch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //Derek 2025/01/17
                DdpmCommonHelper.DeviceManagerSA?.SyncWebcamProfile("DDPMSetProfileToNoneByHDR", false);
                _vm.isUIHasUpdateByQAM = false; //Derek 2025/01/22
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog($"HDRToggleSwitch_Click catch exception: {ex.Message}");
            }
        }

        private void AWB_ToggleSwitch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //Derek 2025/01/17
                DdpmCommonHelper.DeviceManagerSA?.SyncWebcamProfile("DDPMSetProfileToNoneByAWB", false);
                _vm.isUIHasUpdateByQAM = false; //Derek 2025/01/22
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog($"AWB_ToggleSwitch_Click catch exception: {ex.Message}");
            }
        }
    }
}