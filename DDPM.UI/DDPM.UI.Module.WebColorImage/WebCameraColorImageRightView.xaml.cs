using DDPM.SA.Common;
using DDPM.SA.Common.Settings;
using DDPM.UI.Common;
using DDPM.UI.Plugin.ViewModels;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using UserControl = System.Windows.Controls.UserControl;

namespace DDPM.UI.Module.WebCameraColorImage
{
    /// <summary>
    /// Interaction logic for WebCameraSettingsRightView.xaml
    /// </summary>
    public partial class WebCameraColorImageRightView : UserControl
    {
        private readonly WebCameraViewModel _vm;

        public WebCameraColorImageRightView(WebCameraViewModel vm)
        {
            InitializeComponent();
            _vm = vm;

            if (_vm.CurrentDeviceInfo!.IsPropertyWhiteBalanceSupported)
                InitializeAWB();
        }

        private void InitializeAWB()
        {
            AWBSlider.Maximum = _vm.CurrentDeviceInfo!.WhiteBalanceMax;
            AWBSlider.Minimum = _vm.CurrentDeviceInfo.WhiteBalanceMin;
            AWBSlider.TickFrequency = _vm.CurrentDeviceInfo!.WhiteBalanceSteppingDelta;
        }

        private void DeviceManagerSA_ITSettingsActionEvent(object? sender, SA.Common.ITSettingEventArgs e)
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

        //  Jim add 20240628
        private void BrightnessSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            // 20240903 Jim add
            //_vm._deviceManager.SetBrightnessValueByDTP("DellPeripheral.Webcam.0", (int)BrightnessSlider.Value);


            //SetBrightnessLevel((float)BrightnessSlider.Value);
        }

        private void SetBrightnessLevel(float level)
        {
            if (_vm != null && _vm.MediaCapture != null)
            {
                var brightnessControl = _vm.MediaCapture.VideoDeviceController.Brightness;

                // Make sure brightnessFactor is within the valid range
                level = Math.Max(Math.Min(level, (float)_vm.MediaCapture.VideoDeviceController.Brightness.Capabilities.Max), (float)_vm.MediaCapture.VideoDeviceController.Brightness.Capabilities.Min);

                // Make sure brightnessFactor is a multiple of Step, snap to the next lower step
                level -= (level % (float)_vm.MediaCapture.VideoDeviceController.Brightness.Capabilities.Step);

                brightnessControl.TrySetValue(level);
            }
        }

        //  Jim add 20240702
        private void ContrastSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            SetContrastLevel((float)ContrastSlider.Value);
        }

        private void SetContrastLevel(float level)
        {
            if (_vm != null && _vm.MediaCapture != null)
            {
                var contrastControl = _vm.MediaCapture.VideoDeviceController.Contrast;

                // Make sure brightnessFactor is within the valid range
                level = Math.Max(Math.Min(level, (float)_vm.MediaCapture.VideoDeviceController.Contrast.Capabilities.Max), (float)_vm.MediaCapture.VideoDeviceController.Contrast.Capabilities.Min);

                // Make sure brightnessFactor is a multiple of Step, snap to the next lower step
                level -= (level % (float)_vm.MediaCapture.VideoDeviceController.Contrast.Capabilities.Step);

                contrastControl.TrySetValue(level);
            }
        }

        //  Jim add 20240702
        private void SaturationSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            SetSaturationLevel((float)SaturationSlider.Value);
        }

        private void SetSaturationLevel(float level)
        {
            if (_vm != null && _vm.MediaCapture != null)
            {
                var saturationControl = _vm.MediaCapture.VideoDeviceController.Hue;

                // Make sure brightnessFactor is within the valid range
                level = Math.Max(Math.Min(level, (float)_vm.MediaCapture.VideoDeviceController.Hue.Capabilities.Max), (float)_vm.MediaCapture.VideoDeviceController.Hue.Capabilities.Min);

                // Make sure brightnessFactor is a multiple of Step, snap to the next lower step
                level -= (level % (float)_vm.MediaCapture.VideoDeviceController.Hue.Capabilities.Step);

                saturationControl.TrySetValue(level);
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

        private void AntiFlicker_50Hz_Button_Click(object sender, MouseButtonEventArgs e)
        {
        }

        private void AntiFlicker_60Hz_Button_Click(object sender, MouseButtonEventArgs e)
        {
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
    }
}