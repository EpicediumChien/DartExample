using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using DDPM.UI.Plugin.ViewModels;
using Dell.Client.Framework.UX.WPF.Controls;
using Windows.Media.Capture;
using Windows.Media.Devices;
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
        }

        //  Jim add 20240628
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        { 
            if (_vm != null && _vm._mediaCapture != null)
            {
                if (_vm._mediaCapture.VideoDeviceController.Zoom.Capabilities.Supported)
                {

                    // Unhook the event handler, so that changing properties on the slider won't trigger an API call
                    ZoomSlider.ValueChanged -= ZoomSlider_ValueChanged;

                    //var value = _vm._mediaCapture.VideoDeviceController.Zoom.Capabilities.Default;
                    var zoomControl = _vm._mediaCapture.VideoDeviceController.Zoom;

                    ZoomSlider.Minimum = _vm._mediaCapture.VideoDeviceController.Zoom.Capabilities.Min;
                    ZoomSlider.Maximum = _vm._mediaCapture.VideoDeviceController.Zoom.Capabilities.Max;
                    ZoomSlider.TickFrequency = _vm._mediaCapture.VideoDeviceController.Zoom.Capabilities.Step * 100;
                    //ZoomSlider.Value = value;

                    double dbvalue = 0.0f;
                    if  ( zoomControl.TryGetValue(out dbvalue) )
                        ZoomSlider.Value = dbvalue;

                    ZoomSlider.ValueChanged += ZoomSlider_ValueChanged;

                    // 20240702 jim add
                    // Unhook the event handler, so that changing properties on the slider won't trigger an API call
                    AutofocusSlider.ValueChanged -= AutofocusSlider_ValueChanged;

                    //var value = _vm._mediaCapture.VideoDeviceController.Zoom.Capabilities.Default;
                    var autofocusControl = _vm._mediaCapture.VideoDeviceController.Focus;

                    AutofocusSlider.Minimum = _vm._mediaCapture.VideoDeviceController.Focus.Capabilities.Min;
                    AutofocusSlider.Maximum = _vm._mediaCapture.VideoDeviceController.Focus.Capabilities.Max;
                    AutofocusSlider.TickFrequency = _vm._mediaCapture.VideoDeviceController.Focus.Capabilities.Step * 100;
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
                        else {
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
            if (_vm != null && _vm._mediaCapture != null)
            {

                var zoomControl = _vm._mediaCapture.VideoDeviceController.Zoom;

                // Make sure zoomFactor is within the valid range
                level = Math.Max(Math.Min(level, (float)_vm._mediaCapture.VideoDeviceController.Zoom.Capabilities.Max), (float)_vm._mediaCapture.VideoDeviceController.Zoom.Capabilities.Min);

                // Make sure zoomFactor is a multiple of Step, snap to the next lower step
                level -= (level % (float)_vm._mediaCapture.VideoDeviceController.Zoom.Capabilities.Step);
             
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
            if (_vm != null && _vm._mediaCapture != null)
            {

                var autofocusControl = _vm._mediaCapture.VideoDeviceController.Focus;

                // Make sure zoomFactor is within the valid range
                level = Math.Max(Math.Min(level, (float)_vm._mediaCapture.VideoDeviceController.Focus.Capabilities.Max), (float)_vm._mediaCapture.VideoDeviceController.Focus.Capabilities.Min);

                // Make sure zoomFactor is a multiple of Step, snap to the next lower step
                level -= (level % (float)_vm._mediaCapture.VideoDeviceController.Focus.Capabilities.Step);

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

            if (_vm != null && _vm._mediaCapture != null)
            {
                var autofocusControl = _vm._mediaCapture.VideoDeviceController.Focus;
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

        private void FOV_65_Button_Click(object sender, MouseButtonEventArgs e)
        {
            _vm.FOV_IsSelected[0] = true;
            _vm.FOV_IsSelected[1] = false;
            _vm.FOV_IsSelected[2] = false;

            
        }

        private void FOV_78_Button_Click(object sender, MouseButtonEventArgs e)
        {            
            _vm.FOV_IsSelected[0] = false;
            _vm.FOV_IsSelected[1] = true;
            _vm.FOV_IsSelected[2] = false;
        }

        private void FOV_90_Button_Click(object sender, MouseButtonEventArgs e)
        {
            _vm.FOV_IsSelected[0] = false;
            _vm.FOV_IsSelected[1] = false;
            _vm.FOV_IsSelected[2] = true;   
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
    }
}
