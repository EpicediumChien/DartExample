using DDPM.SA.Common;
using DDPM.SA.Common.Settings;
using DDPM.UI.Common;
using DDPM.UI.Plugin.ViewModels;
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
        }

        //  Jim add 20240628
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (_vm != null && _vm.MediaCapture != null)
            {
                if (_vm.MediaCapture.VideoDeviceController.Brightness.Capabilities.Supported)
                {
                    // Unhook the event handler, so that changing properties on the slider won't trigger an API call
                    BrightnessSlider.ValueChanged -= BrightnessSlider_ValueChanged;

                    var brightnessControl = _vm.MediaCapture.VideoDeviceController.Brightness;

                    BrightnessSlider.Minimum = _vm.MediaCapture.VideoDeviceController.Brightness.Capabilities.Min;
                    BrightnessSlider.Maximum = _vm.MediaCapture.VideoDeviceController.Brightness.Capabilities.Max;
                    BrightnessSlider.TickFrequency = _vm.MediaCapture.VideoDeviceController.Brightness.Capabilities.Step;

                    double dbvalue = 0.0f;

                    //if (brightnessControl.TryGetValue(out dbvalue))
                    // 20240903 Jim add
                    //Task<int> task = _vm._deviceManager.GetBrightnessValueByDTP("DellPeripheral.Webcam.0");

                    //dbvalue = (double)(task.Result);

                    //if (brightnessControl.TryGetValue(out dbvalue))
                        BrightnessSlider.Value = dbvalue;

                    BrightnessSlider.ValueChanged += BrightnessSlider_ValueChanged;

                    // 20240702 jim add
                    // Unhook the event handler, so that changing properties on the slider won't trigger an API call
                    AWBSlider.ValueChanged -= AWBSlider_ValueChanged;

                    var awbControl = _vm.MediaCapture.VideoDeviceController.WhiteBalance;

                    AWBSlider.Minimum = _vm.MediaCapture.VideoDeviceController.WhiteBalance.Capabilities.Min;
                    AWBSlider.Maximum = _vm.MediaCapture.VideoDeviceController.WhiteBalance.Capabilities.Max;
                    AWBSlider.TickFrequency = _vm.MediaCapture.VideoDeviceController.WhiteBalance.Capabilities.Step * 100;

                    dbvalue = 0.0f;
                    if (awbControl.TryGetValue(out dbvalue))
                        AWBSlider.Value = dbvalue;

                    AWBSlider.ValueChanged += AWBSlider_ValueChanged;

                    if (awbControl.Capabilities.AutoModeSupported)
                    {
                        bool isAuto;
                        awbControl.TryGetAuto(out isAuto);
                        AWB_ToggleSwitch.IsChecked = isAuto;
                        if (isAuto)
                        {
                            _vm.IsChecked_AWB = true;
                            _vm.AWBStatus_String = "ON";
                        }
                        else
                        {
                            _vm.IsChecked_AWB = false;
                            _vm.AWBStatus_String = "OFF";
                        }
                    }

                    //20240702 jim add
                    // Unhook the event handler, so that changing properties on the slider won't trigger an API call
                    ContrastSlider.ValueChanged -= ContrastSlider_ValueChanged;

                    var contrastControl = _vm.MediaCapture.VideoDeviceController.Contrast;

                    ContrastSlider.Minimum = _vm.MediaCapture.VideoDeviceController.Contrast.Capabilities.Min;
                    ContrastSlider.Maximum = _vm.MediaCapture.VideoDeviceController.Contrast.Capabilities.Max;
                    ContrastSlider.TickFrequency = _vm.MediaCapture.VideoDeviceController.Contrast.Capabilities.Step;

                    dbvalue = 0.0f;
                    if (contrastControl.TryGetValue(out dbvalue))
                        ContrastSlider.Value = dbvalue;

                    ContrastSlider.ValueChanged += ContrastSlider_ValueChanged;

                    //20240702 jim add
                    // Unhook the event handler, so that changing properties on the slider won't trigger an API call
                    SaturationSlider.ValueChanged -= SaturationSlider_ValueChanged;

                    var saturationControl = _vm.MediaCapture.VideoDeviceController.Hue;

                    SaturationSlider.Minimum = _vm.MediaCapture.VideoDeviceController.Hue.Capabilities.Min;
                    SaturationSlider.Maximum = _vm.MediaCapture.VideoDeviceController.Hue.Capabilities.Max;
                    SaturationSlider.TickFrequency = _vm.MediaCapture.VideoDeviceController.Hue.Capabilities.Step;

                    dbvalue = 0.0f;
                    if (saturationControl.TryGetValue(out dbvalue))
                        SaturationSlider.Value = dbvalue;

                    SaturationSlider.ValueChanged += SaturationSlider_ValueChanged;
                }
            }

            //lock/unlock init, 9/23 add
            if (DdpmCommonHelper.DeviceManagerSA != null)
            {
                DdpmCommonHelper.DeviceManagerSA.ITSettingsActionEvent += DeviceManagerSA_ITSettingsActionEvent;

                DDPMSettings data = DdpmCommonHelper.DeviceManagerSA.ReloadAppConfigData().Result;
                if (data != null)
                {
                    //if (data.LockSettings.Lock_Webcam_hdr)
                    {
                        //_vm.isHdrLocked = Visibility.Visible;
                        //_vm.isHdrTabStopped = false;
                    }
                    //else
                    {
                        //_vm.isHdrLocked = Visibility.Collapsed;
                        //_vm.isHdrTabStopped = true;
                    }

                    //if (data.LockSettings.Lock_Webcam_AntiFlicker)
                    {
                        //_vm.isAntiLocked = Visibility.Visible;
                        //_vm.isAntiTabStopped = false;
                    }
                    //else
                    {
                        //_vm.isAntiLocked = Visibility.Collapsed;
                        //_vm.isAntiTabStopped = true;
                    }
                }
            }
        }

        private void DeviceManagerSA_ITSettingsActionEvent(object? sender, SA.Common.ITSettingEventArgs e)
        {
            bool? rst = DdpmCommonHelper.GetUINotifyPropertyValue_Boolean("Lock_Webcam_hdr", e);
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

        //  Jim add 20240702
        private void AWBSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            SetAWBLevel((float)AWBSlider.Value);
        }

        private void SetAWBLevel(float level)
        {
            if (_vm != null && _vm.MediaCapture != null)
            {
                var awbControl = _vm.MediaCapture.VideoDeviceController.WhiteBalance;

                // Make sure zoomFactor is within the valid range
                level = Math.Max(Math.Min(level, (float)_vm.MediaCapture.VideoDeviceController.WhiteBalance.Capabilities.Max), (float)_vm.MediaCapture.VideoDeviceController.WhiteBalance.Capabilities.Min);

                // Make sure zoomFactor is a multiple of Step, snap to the next lower step
                level -= (level % (float)_vm.MediaCapture.VideoDeviceController.WhiteBalance.Capabilities.Step);

                awbControl.TrySetValue(level);
            }
        }

        //  Jim add 20240702
        private void AWB_Switch_Click(object sender, RoutedEventArgs e)
        {
            if ((bool)AWB_ToggleSwitch.IsChecked)
            {
                _vm.IsChecked_AWB = true;
                _vm.AWBStatus_String = "ON";
            }
            else
            {
                _vm.IsChecked_AWB = false;
                _vm.AWBStatus_String = "OFF";
            }

            if (_vm != null && _vm.MediaCapture != null)
            {
                var awbControl = _vm.MediaCapture.VideoDeviceController.WhiteBalance;
                awbControl.TrySetAuto((bool)AWB_ToggleSwitch.IsChecked);
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
    }
}