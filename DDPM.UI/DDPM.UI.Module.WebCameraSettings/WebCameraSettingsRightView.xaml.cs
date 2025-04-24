using DDPM.SA.Common;
using DDPM.SA.Common.Settings;
using DDPM.UI.Common;
using DDPM.UI.Common.Method;
using DDPM.UI.Plugin.ViewModels;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Windows.ApplicationModel.VoiceCommands;
using UserControl = System.Windows.Controls.UserControl;

namespace DDPM.UI.Module.WebCameraSettings
{
    /// <summary>
    /// Interaction logic for WebCameraSettingsRightView.xaml
    /// </summary>
    public partial class WebCameraSettingsRightView : UserControl
    {
        private readonly WebCameraViewModel _vm;
        //private readonly List<string> HelloExcludedList = new() { "WB3023", "WB5023" };

        public WebCameraSettingsRightView(WebCameraViewModel vm)
        {

            try
            {

                InitializeComponent();
                _vm = vm;
                if (_vm?.CurrentDeviceInfo == null)
                    return;

                //lock/unlock init, 9/23 add lock
                if (DdpmCommonHelper.DeviceManagerSA != null)
                {
                    DdpmCommonHelper.DeviceManagerSA.ITSettingsActionEvent += DeviceManagerSA_ITSettingsActionEvent;

                    DDPMSettings data = DdpmCommonHelper.DeviceManagerSA.ReloadAppConfigData().Result;
                    if (data != null)
                    {
                        vm.ShowLockMask = data.LockSettings.Lock_Webcam_AIAutoFraming;
                        vm.IsTabStoppable = !data.LockSettings.Lock_Webcam_AIAutoFraming;
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

                if (_vm.CurrentDeviceInfo.IsPropertyFOVSupported)
                    InitializeFOV();

                if (_vm.CurrentDeviceInfo.IsPropertyZoomSupported)
                    InitializeZoom();

                if (_vm.CurrentDeviceInfo.IsPropertyZoomSupported)
                    InitializeAutofocus();

                if (DdpmCommonHelper.DeviceManagerSA != null)
                {
                    _vm.CurrentDeviceInfo.IsWindowsHelloSupported = DdpmCommonHelper.DeviceManagerSA.GetIsWindowsHelloCapabilityVerified(_vm.CurrentDeviceInfo?.ID.ToString() ?? "").Result;
                    DdpmCommonHelper.WriteUILog($"_vm.CurrentDeviceInfo.IsWindowsHelloSupported:{_vm.CurrentDeviceInfo.IsWindowsHelloSupported}");
                }
                _vm.bdrPrioritize_show = Visibility.Visible;
                _vm.brdHello_show_control = _vm.brdHello_show = brdHello.Visibility = Visibility.Visible;
                if (_vm.Model == "WB5023" || _vm.Model == "WB3023" || !_vm.CurrentDeviceInfo.IsWindowsHelloSupported)
                {
                    DdpmCommonHelper.WriteUILog($"!_vm.CurrentDeviceInfo.IsWindowsHelloSupported:{!_vm.CurrentDeviceInfo.IsWindowsHelloSupported}");
                    //bdrPrioritize.Visibility = Visibility.Collapsed;
                    _vm.bdrPrioritize_show = Visibility.Collapsed;
                    _vm.brdHello_show_control = _vm.brdHello_show = brdHello.Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog("DDPM.UI.Module.WebCameraSettings\\WebCameraSettingsRightView.xaml.cs  WebCameraSettingsRightView() ex:" + ex.Message);
            }
        }

        private void InitializeFOV()
        {
            if (_vm?.CurrentDeviceInfo == null)
                return;
            try
            {
                var FOV = _vm.CurrentDeviceInfo.FOVValues;
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
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog("DDPM.UI.Module.WebCameraSettings\\WebCameraSettingsRightView.xaml.cs  InitializeFOV() ex:" + ex.Message);
            }
        }

        private void InitializeZoom()
        {
            if (_vm?.CurrentDeviceInfo == null)
                return;
            try
            {
                ZoomSlider.Maximum = _vm.CurrentDeviceInfo.ZoomMax;
                ZoomSlider.Minimum = _vm.CurrentDeviceInfo.ZoomMin;
                ZoomSlider.TickFrequency = _vm.CurrentDeviceInfo.ZoomSteppingDelta;
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog("DDPM.UI.Module.WebCameraSettings\\WebCameraSettingsRightView.xaml.cs InitializeZoom() ex:" + ex.Message);
            }
        }
        private void InitializeAutofocus()
        {
            if (_vm?.CurrentDeviceInfo == null)
                return;
            try
            {
                AutofocusSlider.Maximum = _vm.CurrentDeviceInfo.FocusMax;
                AutofocusSlider.Minimum = _vm.CurrentDeviceInfo.FocusMin;
                AutofocusSlider.TickFrequency = _vm.CurrentDeviceInfo.FocusSteppingDelta;
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog("DDPM.UI.Module.WebCameraSettings\\WebCameraSettingsRightView.xaml.cs InitializeAutofocus() ex:" + ex.Message);
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

            try
            {

                //bool? rst = DdpmCommonHelper.GetUINotifyPropertyValue_Boolean("Lock_Webcam_AIAutoFraming", e);
                bool? isLocked = DdpmCommonHelper.GetUINotifyPropertyValue_Boolean("Lock_Webcam_AIAutoFraming", e);

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
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog("DDPM.UI.Module.WebCameraSettings\\WebCameraSettingsRightView.xaml.cs DeviceManagerSA_ITSettingsActionEvent() ex:" + ex.Message);
            }
        }

        //  Jim add 20240628
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //if (_vm != null && _vm.MediaCapture != null && 
            //    _vm.MediaCapture.VideoDeviceController.Zoom.Capabilities.Supported)
            //{
            // 20240702 jim add
            // Unhook the event handler, so that changing properties on the slider won't trigger an API call
            //AutofocusSlider.ValueChanged -= AutofocusSlider_ValueChanged;

            //var value = _vm.MediaCapture.VideoDeviceController.Zoom.Capabilities.Default;
            //var autofocusControl = _vm.MediaCapture.VideoDeviceController.Focus;

            //AutofocusSlider.Minimum = _vm.MediaCapture.VideoDeviceController.Focus.Capabilities.Min;
            //AutofocusSlider.Maximum = _vm.MediaCapture.VideoDeviceController.Focus.Capabilities.Max;
            //AutofocusSlider.TickFrequency = _vm.MediaCapture.VideoDeviceController.Focus.Capabilities.Step * 100;
            ////ZoomSlider.Value = value;

            //dbvalue = 0.0f;
            //if (autofocusControl.TryGetValue(out dbvalue))
            //    AutofocusSlider.Value = dbvalue;

            //AutofocusSlider.ValueChanged += AutofocusSlider_ValueChanged;

            //if (autofocusControl.Capabilities.AutoModeSupported)
            //{
            //    bool isAuto;
            //    autofocusControl.TryGetAuto(out isAuto);
            //    Autofocus_ToggleSwitch.IsChecked = isAuto;
            //    if (isAuto)
            //    {
            //        _vm.IsChecked_Autofocus = true;
            //        _vm.AutofocusStatus_String = "ON";
            //    }
            //    else
            //    {
            //        _vm.IsChecked_Autofocus = false;
            //        _vm.AutofocusStatus_String = "OFF";
            //    }
            //}
            //}
            //NarratorModeSupport.RecurseUitems(start  );

        }

        private void SetZoomLevel(float level)
        {
            try
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
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog("DDPM.UI.Module.WebCameraSettings\\WebCameraSettingsRightView.xaml.cs SetZoomLevel() ex:" + ex.Message);
            }
        }

        //  Jim add 20240702
        //private void AutofocusSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        //{
        //    SetAutofocusLevel((float)AutofocusSlider.Value);
        //}

        //private void SetAutofocusLevel(float level)
        //{
        //    if (_vm != null && _vm.MediaCapture != null)
        //    {
        //        var autofocusControl = _vm.MediaCapture.VideoDeviceController.Focus;

        //        // Make sure zoomFactor is within the valid range
        //        level = Math.Max(Math.Min(level, (float)_vm.MediaCapture.VideoDeviceController.Focus.Capabilities.Max), (float)_vm.MediaCapture.VideoDeviceController.Focus.Capabilities.Min);

        //        // Make sure zoomFactor is a multiple of Step, snap to the next lower step
        //        level -= (level % (float)_vm.MediaCapture.VideoDeviceController.Focus.Capabilities.Step);

        //        autofocusControl.TrySetValue(level);
        //    }
        //}

        private void FOV_Click(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (sender is Border bdr)
                {
                    _vm.isUIHasUpdateByQAM = false; //Derek 2025/01/22

                    var index = int.Parse(bdr.Tag.ToString()!);
                    //var val = _vm.FOVs[index];
                    if (index == _vm.SelectedFovIndex)
                    { return; }

                    _vm.SelectedFovIndex = index;
                    _vm.SetFOV_Selected(index);
                    //_vm.FieldOfView = val;

                    //Derek 2025/01/17
                    InfoQAMSetProfileToNone();
                }
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog("DDPM.UI.Module.WebCameraSettings\\WebCameraSettingsRightView.xaml.cs FOV_Click() ex:" + ex.Message);
            }
        }

        private void InfoQAMSetProfileToNone()
        {
            try
            {
                //Derek 2025/01/17
                DdpmCommonHelper.DeviceManagerSA?.SyncWebcamProfile("DDPMSetProfileToNoneByFOV", false);
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog($"InfoQAMSetProfileToNone catch exception: {ex.Message}");
            }
        }

        private void Priority_Click(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (sender is Border bdr)
                {
                    var val = int.Parse(bdr.Tag.ToString()!);
                    if (val == _vm.Priority)
                    { return; }

                    _vm.Priority = val;
                }
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog("DDPM.UI.Module.WebCameraSettings\\WebCameraSettingsRightView.xaml.cs Priority_Click() ex:" + ex.Message);
            }
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
                DdpmCommonHelper.WriteUILog("DDPM.UI.Module.WebCameraSettings\\WebCameraSettingsRightView.xaml.cs CallWindowsHello_Click() ex:" + ex.Message);
            }
        }

        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
        }

        private void AutoFramingSensitivity_Click(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (sender is Border bdr)
                {
                    var val = int.Parse(bdr.Tag.ToString()!);
                    if (val == _vm.AutoFramingSensitivity)
                    { return; }

                    _vm.AutoFramingSensitivity = val;
                }
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog("DDPM.UI.Module.WebCameraSettings\\WebCameraSettingsRightView.xaml.cs AutoFramingSensitivity_Click() ex:" + ex.Message);
            }
        }

        private void AutoFramingSize_Click(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (sender is Border bdr)
                {
                    var val = int.Parse(bdr.Tag.ToString()!);
                    if (val == _vm.AutoFramingFrameSize)
                    { return; }

                    _vm.AutoFramingFrameSize = val;
                }
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog("DDPM.UI.Module.WebCameraSettings\\WebCameraSettingsRightView.xaml.cs AutoFramingSize_Click() ex:" + ex.Message);
            }
        }

        private void ZoomSlider_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {
            try
            {
                _vm.IsSliderDragging = true;
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog("DDPM.UI.Module.WebCameraSettings\\WebCameraSettingsRightView.xaml.cs ZoomSlider_DragStarted() ex:" + ex.Message);
            }
        }

        private void ZoomSlider_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            try
            {
                _vm.IsSliderDragging = false;
                _vm.SetZoom();
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog("DDPM.UI.Module.WebCameraSettings\\WebCameraSettingsRightView.xaml.cs ZoomSlider_DragCompleted() ex:" + ex.Message);
            }
        }

        private void AutofocusSlider_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {
            try
            {
                _vm.IsSliderDragging = true;
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog("DDPM.UI.Module.WebCameraSettings\\WebCameraSettingsRightView.xaml.cs AutofocusSlider_DragStarted() ex:" + ex.Message);
            }
        }

        private void AutofocusSlider_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            try
            {
                _vm.IsSliderDragging = false;
                _vm.SetFocus();
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog("DDPM.UI.Module.WebCameraSettings\\WebCameraSettingsRightView.xaml.cs AutofocusSlider_DragCompleted() ex:" + ex.Message);
            }
        }

        private void ShowHDR_Click(object sender, MouseButtonEventArgs e)
        {
            try
            {
                ShowHDR();
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog("DDPM.UI.Module.WebCameraSettings\\WebCameraSettingsRightView.xaml.cs howHDR_Click() ex:" + ex.Message);
            }
        }
        private void ShowHDR()
        {
            try
            {
                _vm.VbarSelectedIndex = 1;
                _vm.SelectVBar();
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog("DDPM.UI.Module.WebCameraSettings\\WebCameraSettingsRightView.xaml.cs ShowHDR() ex:" + ex.Message);
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
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog("DDPM.UI.Module.WebCameraSettings\\WebCameraSettingsRightView.xaml.cs Image_MouseLeftButtonDown() ex:" + ex.Message);
            }
        }

        private void CloseMessageBox(object sender, MouseButtonEventArgs e)
        {
            try
            {
                _vm.MessageBoxVisibility = Visibility.Collapsed;
                _vm.OnPropertyChanged(nameof(_vm.MessageBoxVisibility));
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog("DDPM.UI.Module.WebCameraSettings\\WebCameraSettingsRightView.xaml.cs CloseMessageBox() ex:" + ex.Message);
            }
        }

        private void Slider_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                _vm.IsSliderDragging = true;
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog("DDPM.UI.Module.WebCameraSettings\\WebCameraSettingsRightView.xaml.cs Slider_PreviewKeyDown() ex:" + ex.Message);
            }
        }

        private void ZoomSlider_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                _vm.IsSliderDragging = false;
                _vm.SetZoom();
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog("DDPM.UI.Module.WebCameraSettings\\WebCameraSettingsRightView.xaml.cs ZoomSlider_KeyUp() ex:" + ex.Message);
            }
        }

        private void AutofocusSlider_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                _vm.IsSliderDragging = false;
                _vm.SetFocus();
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog("DDPM.UI.Module.WebCameraSettings\\WebCameraSettingsRightView.xaml.cs AutofocusSlider_KeyUp() ex:" + ex.Message);
            }
        }

        //Derek 2025/01/17
        private void AutoFrameSwitch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //Derek 2025/01/17
                DdpmCommonHelper.DeviceManagerSA?.SyncWebcamProfile("DDPMSetProfileToNoneByAutoFrame", false);
                _vm.isUIHasUpdateByQAM = false; //Derek 2025/01/22
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog($"AutoFrameSwitch_Click catch exception: {ex.Message}");
            }
        }

        private void BgBlurSwitch_Click(object sender, RoutedEventArgs e)
        {
            //try
            //{
            //    DdpmCommonHelper.DeviceManagerSA?.SetIsBgBlurEnable("DDPMSetProfileToNoneByAutoFrame", false);
            //}
            //catch (Exception ex)
            //{
            //    DdpmCommonHelper.WriteUILog($"AutoFrameSwitch_Click catch exception: {ex.Message}");
            //}
        }

        private void SetBgBlur_Click(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (sender is Border bdr)
                {
                    var val = int.Parse(bdr.Tag.ToString()!);
                    if (val == _vm.BgBlur)
                    { return; }

                    _vm.BgBlur = val;
                }
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog("DDPM.UI.Module.WebCameraSettings\\WebCameraSettingsRightView.xaml.cs AutoFramingSensitivity_Click() ex:" + ex.Message);
            }
        }

        //Elsa add for tooltip issue fix
        private double GetScreenScaleX()
        {
            var source = PresentationSource.FromVisual(this);
            if (source?.CompositionTarget != null)
            {
                return source.CompositionTarget.TransformToDevice.M11;
            }
            return 1;
        }

        //Elsa add for tooltip issue fix
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
                target.HorizontalOffset = 13;
            }
            else
            {
                target.HorizontalOffset = -1 * target.ActualWidth + 25;
            }
        }
    }
}