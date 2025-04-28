using DDPM.UI.Plugin.ViewModels;
using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Windows.Storage.Pickers;
using Windows.Storage;
using UserControl = System.Windows.Controls.UserControl;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using DDPM.UI.Plugin.Common;
using System.Diagnostics;
using Dell.Client.Framework.UX.WPF.Controls;
using System.Linq;
using DDPM.UI.Common;
using DDPM.UI.Resources.Helper;
using System.Linq.Expressions;

namespace DDPM.UI.Module.WebCameraCapture
{
    /// <summary>
    /// Interaction logic for WebCameraSettingsRightView.xaml
    /// </summary>
    public partial class WebCameraCaptureRightView : UserControl
    {
        private readonly WebCameraViewModel _vm;

        public WebCameraCaptureRightView(WebCameraViewModel vm)
        {
            try
            {
                InitializeComponent();
                _vm = vm;

                txtCaptureFolder.Text = Utility.CheckTextLength(_vm.VideoCaptureFolder, 155, 14);
                btnOpen.Caption = LangHelper.Instance["WebCameraCapture.2"];
                InitializeResolution();
                InitializeFPS();
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog("DDPM.UI.Module.WebCameraCapture\\WebCameraCaptureRightView.xaml.cs WebCameraCaptureRightView() ex:" + ex.Message);
            }

        }
        private void InitializeResolution()
        {
            try
            {
                var Res = _vm.WebcamSettings?.SupportedFPSs?.Keys.ToList() ?? new List<string>();
                switch (Res.Count)
                {
                    case 2:

                        _vm.btnRes0_width = 201;
                        txtRes0.Text = Res[0];
                        _vm.btnRes1_width = 201;
                        txtRes1.Text = Res[1];
                        _vm.btnRes1_radius_v = new CornerRadius(0, 5, 5, 0);
                        btnRes2.Visibility = Visibility.Collapsed;
                        btnRes2_Col.Width = new GridLength(0);
                        btnRes3.Visibility = Visibility.Collapsed;
                        btnRes3_Col.Width = new GridLength(0);

                        /*btnRes0.Width = 201;
                        txtRes0.Text = Res[0];
                        btnRes1.Width = 201;
                        txtRes1.Text = Res[1];
                        btnRes1.CornerRadius = new CornerRadius(0, 5, 5, 0);
                        btnRes2.Visibility = Visibility.Collapsed;
                        btnRes2_Col.Width = new GridLength(0);
                        btnRes3.Visibility = Visibility.Collapsed;
                        btnRes3_Col.Width = new GridLength(0);*/
                        break;
                    case 3:
                        _vm.btnRes0_width = 134;
                        txtRes0.Text = Res[0];
                        _vm.btnRes1_width = 134;
                        txtRes1.Text = Res[1];
                        _vm.btnRes2_width = 134;
                        txtRes2.Text = Res[2];
                        btnRes2.CornerRadius = new CornerRadius(0, 5, 5, 0);
                        btnRes3.Visibility = Visibility.Collapsed;
                        btnRes3_Col.Width = new GridLength(0);
                        break;
                    case 4:
                        btnRes0.Width = 100.5;
                        txtRes0.Text = Res[0];
                        btnRes1.Width = 100.5;
                        txtRes1.Text = Res[1];
                        btnRes2.Width = 100.5;
                        txtRes2.Text = Res[2];
                        btnRes3.Width = 100.5;
                        txtRes3.Text = Res[3];
                        btnRes3.CornerRadius = new CornerRadius(0, 5, 5, 0);
                        break;
                }
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog("DDPM.UI.Module.WebCameraCapture\\WebCameraCaptureRightView.xaml.cs InitializeResolution() ex:" + ex.Message);
            }
        }
        private void InitializeFPS()
        {
            try
            {
                var FPS = _vm.WebcamSettings.SupportedFPSs[_vm.WebcamSettings.SelectedResolution];
                switch (FPS.Count)
                {
                    case 1:
                        btnFPS0.Width = 402;
                        txtFPS0.Text = FPS[0];
                        btnFPS0.CornerRadius = new CornerRadius(5, 5, 5, 5);
                        btnFPS1.Visibility = Visibility.Collapsed;
                        btnFPS2.Visibility = Visibility.Collapsed;
                        break;
                    case 2:
                        btnFPS0.Width = 201;
                        txtFPS0.Text = FPS[0];
                        btnFPS0.CornerRadius = new CornerRadius(5, 0, 0, 5);
                        btnFPS1.Width = 201;
                        txtFPS1.Text = FPS[1];
                        btnFPS1.CornerRadius = new CornerRadius(0, 5, 5, 0);
                        btnFPS1.Visibility = Visibility.Visible;
                        btnFPS2.Visibility = Visibility.Collapsed;
                        break;
                    case 3:
                        btnFPS0.Width = 134;
                        txtFPS0.Text = FPS[0];
                        btnFPS0.CornerRadius = new CornerRadius(5, 0, 0, 5);
                        btnFPS1.Width = 134;
                        txtFPS1.Text = FPS[1];
                        btnFPS1.CornerRadius = new CornerRadius(0);
                        btnFPS1.Visibility = Visibility.Visible;
                        btnFPS2.Width = 134;
                        txtFPS2.Text = FPS[2];
                        btnFPS2.Visibility = Visibility.Visible;
                        btnFPS2.CornerRadius = new CornerRadius(0, 5, 5, 0);
                        break;
                }
                var idx = _vm.WebcamSettings.SupportedFPSs[_vm.WebcamSettings.SelectedResolution].IndexOf(_vm.WebcamSettings.CurrentFPS);
                if (_vm.IsAutoFramingOn && _vm.WebcamSettings.CurrentFPS == "60" && idx > 0)
                    idx -= 1;
                _vm.SetFPS_Selected(idx);
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog("DDPM.UI.Module.WebCameraCapture\\WebCameraCaptureRightView.xaml.cs InitializeFPS() ex:" + ex.Message);
            }

        }

        private Task _currentOperation;
        private async void btnResolution_Click(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (sender is Border bdr)
                {
                    if (bdr.Tag is not string)
                        return;
                    if (!int.TryParse(bdr.Tag.ToString(), out int idx))
                        return;

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        _vm.AlertType = WebcamAlert.Alert1;
                        _vm.AlertVisibility = Visibility.Visible;
                    });

                    _vm.SetResolution_Selected(idx);
                    await Task.Delay(1000);
                    InitializeFPS();

                    //for default 30 fps
                    //try
                    //{
                    //    if (_vm.WebcamSettings?.SupportedFPSs != null && _vm.WebcamSettings.SelectedResolution != null)
                    //    {
                    //        if (_vm.WebcamSettings.SupportedFPSs.ContainsKey(_vm.WebcamSettings.SelectedResolution))
                    //        {
                    //            List<string> FPS = _vm.WebcamSettings.SupportedFPSs[_vm.WebcamSettings.SelectedResolution];
                    //            int index = FPS.FindIndex(x => x == "30");
                    //            if (index != -1)
                    //            {
                    //                _vm.SetFPS_Selected(index);
                    //            }
                    //        }
                    //    }
                    //}
                    //catch (Exception ex)
                    //{
                    //    DdpmCommonHelper.WriteUILog("DDPM.UI.Module.WebCameraCapture\\WebCameraCaptureRightView.xaml.cs btnResolution_Click : " + ex.Message);
                    //}

                    foreach (var property in _vm.allProperties)
                    {
                        string properties_temp = property.GetFriendlyName();
                        if (properties_temp.Contains(_vm.WebcamSettings.CurrentResolution, StringComparison.OrdinalIgnoreCase) && properties_temp.Contains(_vm.WebcamSettings.CurrentFPS, StringComparison.OrdinalIgnoreCase) && property.EncodingProperties.Subtype != "MJPG")
                        {
                            DdpmCommonHelper.WriteUILog($"[btnResolution_Click] properties_temp: {properties_temp}");
                            var encodingProperties = property.EncodingProperties;
                            //_ = _vm.MediaCapture!.VideoDeviceController.SetMediaStreamPropertiesAsync(MediaStreamType.VideoPreview, encodingProperties);
                            DdpmCommonHelper.WriteUILog($"[btnResolution_Click] encodingProperties: {encodingProperties} Subtype: {encodingProperties.Subtype}");

                            //參數設置需要一段硬體初始時間,中間再設定參數會造成設定失敗crush,所以要防呆
                            if (_currentOperation != null && !_currentOperation.IsCompleted)
                                await _currentOperation;
                            _currentOperation = SetMediaStreamPropertiesAsync(encodingProperties);

                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog("DDPM.UI.Module.WebCameraCapture\\WebCameraCaptureRightView.xaml.cs btnResolution_Click() ex:" + ex.Message);
            }
            finally
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    _vm.AlertVisibility = Visibility.Hidden;
                });
            }
        }

        private async Task SetMediaStreamPropertiesAsync(IMediaEncodingProperties encodingProperties)
        {
            await _vm.MediaCapture?.VideoDeviceController.SetMediaStreamPropertiesAsync(MediaStreamType.VideoRecord, encodingProperties);
        }

        private async void btnFPS_Click(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (sender is Border bdr)
                {
                    if (bdr.Tag is not string)
                        return;

                    if (!int.TryParse(bdr.Tag.ToString(), out int idx))
                        return;

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        _vm.AlertType = WebcamAlert.Alert1;
                        _vm.AlertVisibility = Visibility.Visible;
                    });

                    _vm.SetFPS_Selected(idx);
                    await Task.Delay(1000);
                    foreach (var property in _vm.allProperties)
                    {
                        string properties_temp = property.GetFriendlyName();

                        if (properties_temp.Contains(_vm.WebcamSettings.CurrentResolution, StringComparison.OrdinalIgnoreCase) && properties_temp.Contains(_vm.WebcamSettings.CurrentFPS, StringComparison.OrdinalIgnoreCase) && property.EncodingProperties.Subtype != "MJPG")
                        {
                            DdpmCommonHelper.WriteUILog($"[btnFPS_Click] properties_temp: {properties_temp}");
                            var encodingProperties = property.EncodingProperties;
                            DdpmCommonHelper.WriteUILog($"[btnFPS_Click] encodingProperties: {encodingProperties} Subtype: {encodingProperties.Subtype}");
                            //_ = _vm.MediaCapture!.VideoDeviceController.SetMediaStreamPropertiesAsync(MediaStreamType.VideoPreview, encodingProperties);

                            //參數設置需要一段硬體初始時間,中間再設定參數會造成設定失敗crush,所以要防呆
                            if (_currentOperation != null && !_currentOperation.IsCompleted)
                                await _currentOperation;
                            _currentOperation = SetMediaStreamPropertiesAsync(encodingProperties);

                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog("DDPM.UI.Module.WebCameraCapture\\WebCameraCaptureRightView.xaml.cs btnFPS_Click() ex:" + ex.Message);
            }
            finally
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    _vm.AlertVisibility = Visibility.Hidden;
                });
            }
        }

        private void Open_Click(object sender, MouseButtonEventArgs e)
        {
            Process.Start("explorer.exe", _vm?.VideoCaptureFolder ?? Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
        }

        private void Change_Click(object sender, RoutedEventArgs e)
        {
            var folderDialog = new OpenFolderDialog();
            folderDialog.FolderName = _vm.VideoCaptureFolder;

            if (folderDialog.ShowDialog() == true)
            {
                _vm.VideoCaptureFolder = folderDialog.FolderName;
                txtCaptureFolder.Text = Utility.CheckTextLength(_vm.VideoCaptureFolder, 155, 14);
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
                DdpmCommonHelper.WriteUILog("DDPM.UI.Module.WebCameraCapture\\WebCameraCaptureRightView.xaml.cs Image_MouseLeftButtonDown() ex:" + ex.Message);
            }
        }

        private void CloseMessageBox(object sender, MouseButtonEventArgs e)
        {
            _vm.MessageBoxVisibilityUsbType = Visibility.Collapsed;
            _vm.OnPropertyChanged(nameof(_vm.MessageBoxVisibilityUsbType));
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
                target.HorizontalOffset = 11;
            }
            else
            {
                target.HorizontalOffset = -1 * target.ActualWidth + 23;
            }
        }
    }
}