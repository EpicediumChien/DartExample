using DDPM.SA.Common;
using DDPM.SA.Common.Settings;
using DDPM.UI.Common;
using DDPM.UI.Plugin.ViewModels;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using UserControl = System.Windows.Controls.UserControl;

namespace DDPM.UI.Module.WebCameraSettings
{
    /// <summary>
    /// Interaction logic for WebCameraSettingsRightView.xaml
    /// </summary>
    public partial class WebCameraSettingsRightView : UserControl
    {
        private readonly WebCameraViewModel _vm;

        public WebCameraSettingsRightView(WebCameraViewModel vm)
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
                    vm.ShowLockMask = data.LockSettings.Lock_Webcam_AIAutoFraming;
                    vm.isTabStoppable = !data.LockSettings.Lock_Webcam_AIAutoFraming;
                    vm.LockMaskVisible = vm.ShowLockMask ? Visibility.Visible : Visibility.Collapsed;

                    //if (data.LockSettings.Lock_Webcam_AIAutoFraming)
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

            if (_vm.CurrentDeviceInfo!.IsPropertyFOVSupported)
                InitializeFOV();
        }

        private void InitializeFOV()
        {
            var FOV = _vm.CurrentDeviceInfo!.FOVValues;
            switch (FOV.Length)
            {
                case 2:
                    btnFOV0.Width = 201;
                    txtFOV0.Text = $"{FOV[0]}°";
                    btnFOV1.Width = 201;
                    txtFOV1.Text = $"{FOV[1]}°";
                    btnFOV1.CornerRadius = new CornerRadius(0, 5, 5, 0);
                    btnFOV2.Visibility = Visibility.Collapsed;
                    break;
                case 3:
                    btnFOV0.Width = 134;
                    txtFOV0.Text = $"{FOV[0]}°";
                    btnFOV1.Width = 134;
                    txtFOV1.Text = $"{FOV[1]}°";
                    btnFOV1.CornerRadius = new CornerRadius(0);
                    btnFOV2.Width = 134;
                    txtFOV2.Text = $"{FOV[2]}°";
                    btnFOV2.Visibility = Visibility.Visible;
                    btnFOV2.CornerRadius = new CornerRadius(0, 5, 5, 0);
                    break;
                default:
                    bdrFOV.Visibility = Visibility.Collapsed;
                    break;
            }
        }

        ~WebCameraSettingsRightView()
        {
            if (DdpmCommonHelper.DeviceManagerSA != null)
            {
                DdpmCommonHelper.DeviceManagerSA.ITSettingsActionEvent -= DeviceManagerSA_ITSettingsActionEvent;
            }
        }
        private void DeviceManagerSA_ITSettingsActionEvent(object? sender, SA.Common.ITSettingEventArgs e)
        {
            //bool? rst = DdpmCommonHelper.GetUINotifyPropertyValue_Boolean("Lock_Webcam_AIAutoFraming", e);
            bool? isLocked = DdpmCommonHelper.GetUINotifyPropertyValue_Boolean("Lock_Webcam_AIAutoFraming", e);

            if (isLocked != null)
            {
                Dispatcher.Invoke(new Action(() =>
                {
                    if (_vm != null)
                    {
                        _vm.isTabStoppable = !(bool)isLocked;
                        _vm.ShowLockMask = (bool)isLocked;

                        if (_vm.ShowLockMask)
                            _vm.TabNavigation = "None";
                        else
                            _vm.TabNavigation = "Cycle";

                        _vm.LockMaskVisible = (bool)isLocked ? Visibility.Visible : Visibility.Collapsed;
                        Trace.WriteLine($"[SettingsPage] WebCameraSettingsRightView(Lock) : {isLocked}");
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

        //  Jim add 20240628
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (_vm != null && _vm.MediaCapture != null)
            {
                if (_vm.MediaCapture.VideoDeviceController.Zoom.Capabilities.Supported)
                {
                    // Unhook the event handler, so that changing properties on the slider won't trigger an API call
                    ZoomSlider.ValueChanged -= ZoomSlider_ValueChanged;

                    //var value = _vm.MediaCapture.VideoDeviceController.Zoom.Capabilities.Default;
                    var zoomControl = _vm.MediaCapture.VideoDeviceController.Zoom;

                    ZoomSlider.Minimum = _vm.MediaCapture.VideoDeviceController.Zoom.Capabilities.Min;
                    ZoomSlider.Maximum = _vm.MediaCapture.VideoDeviceController.Zoom.Capabilities.Max;
                    ZoomSlider.TickFrequency = _vm.MediaCapture.VideoDeviceController.Zoom.Capabilities.Step * 100;
                    //ZoomSlider.Value = value;

                    double dbvalue = 0.0f;
                    if (zoomControl.TryGetValue(out dbvalue))
                        ZoomSlider.Value = dbvalue;

                    ZoomSlider.ValueChanged += ZoomSlider_ValueChanged;

                    // 20240702 jim add
                    // Unhook the event handler, so that changing properties on the slider won't trigger an API call
                    AutofocusSlider.ValueChanged -= AutofocusSlider_ValueChanged;

                    //var value = _vm.MediaCapture.VideoDeviceController.Zoom.Capabilities.Default;
                    var autofocusControl = _vm.MediaCapture.VideoDeviceController.Focus;

                    AutofocusSlider.Minimum = _vm.MediaCapture.VideoDeviceController.Focus.Capabilities.Min;
                    AutofocusSlider.Maximum = _vm.MediaCapture.VideoDeviceController.Focus.Capabilities.Max;
                    AutofocusSlider.TickFrequency = _vm.MediaCapture.VideoDeviceController.Focus.Capabilities.Step * 100;
                    //ZoomSlider.Value = value;

                    dbvalue = 0.0f;
                    if (autofocusControl.TryGetValue(out dbvalue))
                        AutofocusSlider.Value = dbvalue;

                    AutofocusSlider.ValueChanged += AutofocusSlider_ValueChanged;

                    if (autofocusControl.Capabilities.AutoModeSupported)
                    {
                        bool isAuto;
                        autofocusControl.TryGetAuto(out isAuto);
                        Autofocus_ToggleSwitch.IsChecked = isAuto;
                        if (isAuto)
                        {
                            _vm.IsChecked_Autofocus = true;
                            _vm.AutofocusStatus_String = "ON";
                        }
                        else
                        {
                            _vm.IsChecked_Autofocus = false;
                            _vm.AutofocusStatus_String = "OFF";
                        }
                    }
                }
            }
        }

        //  Jim add 20240628
        private void ZoomSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            SetZoomLevel((float)ZoomSlider.Value);
        }

        private void SetZoomLevel(float level)
        {
            if (_vm != null && _vm.MediaCapture != null)
            {
                var zoomControl = _vm.MediaCapture.VideoDeviceController.Zoom;

                // Make sure zoomFactor is within the valid range
                level = Math.Max(Math.Min(level, (float)_vm.MediaCapture.VideoDeviceController.Zoom.Capabilities.Max), (float)_vm.MediaCapture.VideoDeviceController.Zoom.Capabilities.Min);

                // Make sure zoomFactor is a multiple of Step, snap to the next lower step
                level -= (level % (float)_vm.MediaCapture.VideoDeviceController.Zoom.Capabilities.Step);

                zoomControl.TrySetValue(level);
            }
        }

        //  Jim add 20240702
        private void AutofocusSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            SetAutofocusLevel((float)AutofocusSlider.Value);
        }

        private void SetAutofocusLevel(float level)
        {
            if (_vm != null && _vm.MediaCapture != null)
            {
                var autofocusControl = _vm.MediaCapture.VideoDeviceController.Focus;

                // Make sure zoomFactor is within the valid range
                level = Math.Max(Math.Min(level, (float)_vm.MediaCapture.VideoDeviceController.Focus.Capabilities.Max), (float)_vm.MediaCapture.VideoDeviceController.Focus.Capabilities.Min);

                // Make sure zoomFactor is a multiple of Step, snap to the next lower step
                level -= (level % (float)_vm.MediaCapture.VideoDeviceController.Focus.Capabilities.Step);

                autofocusControl.TrySetValue(level);
            }
        }

        //  Jim add 20240702
        private void Autofocus_Switch_Click(object sender, RoutedEventArgs e)
        {
            if ((bool)Autofocus_ToggleSwitch.IsChecked)
            {
                _vm.IsChecked_Autofocus = true;
                _vm.AutofocusStatus_String = "ON";
            }
            else
            {
                _vm.IsChecked_Autofocus = false;
                _vm.AutofocusStatus_String = "OFF";
            }

            if (_vm != null && _vm.MediaCapture != null)
            {
                var autofocusControl = _vm.MediaCapture.VideoDeviceController.Focus;
                autofocusControl.TrySetAuto((bool)Autofocus_ToggleSwitch.IsChecked);
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

        private void TrackingSensitivity_Normal_Button_Click(object sender, MouseButtonEventArgs e)
        {
        }

        private void TrackingSensitivity_Fast_Button_Click(object sender, MouseButtonEventArgs e)
        {
        }

        private void FrameSize_Narrow_Button_Click(object sender, MouseButtonEventArgs e)
        {
        }

        private void FrameSize_Standard_Button_Click(object sender, MouseButtonEventArgs e)
        {
        }

        private void FOV_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border bdr)
            {
                var index = int.Parse(bdr.Tag.ToString()!);
                var val = _vm.FOVs[index];
                if (val == _vm.FieldOfView)
                { return; }

                _vm.SetFOV_Selected(index);
                _vm.FieldOfView = val;
            }
        }

        private void Priority_Exposure_Button_Click(object sender, MouseButtonEventArgs e)
        {
        }

        private void Priority_FrameRate_Button_Click(object sender, MouseButtonEventArgs e)
        {
        }

        private void CallWindowsHello_Click(object sender, RoutedEventArgs e)
        {
            var psi = new System.Diagnostics.ProcessStartInfo();

            psi.FileName = "ms-settings:signinoptions-launchfaceenrollment";
            psi.UseShellExecute = true;

            System.Diagnostics.Process.Start(psi);
        }

        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
        }

        private void AutoFramingSensitivity_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border bdr)
            {
                var val = int.Parse(bdr.Tag.ToString()!);
                if (val == _vm.AutoFramingSensitivity)
                { return; }

                _vm.AutoFramingSensitivity = val;
            }
        }

        private void AutoFramingSize_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border bdr)
            {
                var val = int.Parse(bdr.Tag.ToString()!);
                if (val == _vm.AutoFramingFrameSize)
                { return; }

                _vm.AutoFramingFrameSize = val;
            }
        }
    }
}