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
            InitializeComponent();
            _vm = vm;

            txtCaptureFolder.Text = Utility.CheckTextLength(_vm.VideoCaptureFolder, 155, 14);
            btnOpen.Caption = LangHelper.Instance["WebCameraCapture.2"];
            InitializeResolution();
            InitializeFPS();

        }
        private void InitializeResolution()
        {
            var Res = _vm.WebcamSettings.SupportedFPSs.Keys.ToList();
            switch (Res.Count)
            {
                case 2:
                    btnRes0.Width = 201;
                    txtRes0.Text = Res[0];
                    btnRes1.Width = 201;
                    txtRes1.Text = Res[1];
                    btnRes1.CornerRadius = new CornerRadius(0, 5, 5, 0);
                    btnRes2.Visibility = Visibility.Collapsed;
                    btnRes2_Col.Width = new GridLength(0);
                    btnRes3.Visibility = Visibility.Collapsed;
                    btnRes3_Col.Width = new GridLength(0);
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
        private void InitializeFPS()
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
            _vm.SetFPS_Selected(_vm.WebcamSettings.SupportedFPSs[_vm.WebcamSettings.SelectedResolution].IndexOf(_vm.WebcamSettings.CurrentFPS));
        }
        private async void btnResolution_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border bdr)
            {
                int idx;
                if (!(bdr.Tag is string))
                    return;
                bool r = int.TryParse(bdr.Tag.ToString()!, out idx);
                if (!r)
                    return;
                _vm.SetResolution_Selected(idx);
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
                    if (properties_temp.Contains(_vm.WebcamSettings.CurrentResolution, StringComparison.OrdinalIgnoreCase) && properties_temp.Contains(_vm.WebcamSettings.CurrentFPS, StringComparison.OrdinalIgnoreCase))
                    {
                        var encodingProperties = property.EncodingProperties;
                        //_ = _vm.MediaCapture!.VideoDeviceController.SetMediaStreamPropertiesAsync(MediaStreamType.VideoPreview, encodingProperties);


                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            _vm.AlertType = WebcamAlert.Alert1;
                            _vm.AlertVisibility = Visibility.Visible;
                        });


                        bool set_ok = false;
                        while (set_ok != true)
                        {
                            try
                            {
                                //參數設置需要一段硬體初始時間,中間再設定參數會造成設定失敗crush,所以要防呆
                                await _vm.MediaCapture!.VideoDeviceController.SetMediaStreamPropertiesAsync(MediaStreamType.VideoPreview, encodingProperties);
                                set_ok = true;
                            }
                            catch
                            {
                                _vm._log.Debug("WebCameraCaptureRightView.cs set Resolution fail!");
                                await Task.Delay(250);
                            }
                        }

                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            _vm.AlertVisibility = Visibility.Hidden;
                        });



                        break;
                    }
                }
            }
        }

        private async void btnFPS_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border bdr)
            {
                int idx;
                if (!(bdr.Tag is string))
                    return;
                bool r = int.TryParse(bdr.Tag.ToString()!, out idx);
                if (!r)
                    return;
                _vm.SetFPS_Selected(idx);
                foreach (var property in _vm.allProperties)
                {
                    string properties_temp = property.GetFriendlyName();
                    if (properties_temp.Contains(_vm.WebcamSettings.CurrentResolution, StringComparison.OrdinalIgnoreCase) && properties_temp.Contains(_vm.WebcamSettings.CurrentFPS, StringComparison.OrdinalIgnoreCase))
                    {
                        var encodingProperties = property.EncodingProperties;

                        //_ = _vm.MediaCapture!.VideoDeviceController.SetMediaStreamPropertiesAsync(MediaStreamType.VideoPreview, encodingProperties);

                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            _vm.AlertType = WebcamAlert.Alert1;
                            _vm.AlertVisibility = Visibility.Visible;
                        });

                        bool set_ok = false;
                        while (set_ok != true)
                        {
                            try
                            {
                                //參數設置需要一段硬體初始時間,中間再設定參數會造成設定失敗crush,所以要防呆
                                await _vm.MediaCapture!.VideoDeviceController.SetMediaStreamPropertiesAsync(MediaStreamType.VideoPreview, encodingProperties);
                                set_ok = true;
                            }
                            catch
                            {
                                _vm._log.Debug("WebCameraCaptureRightView.cs set FPS fail!");
                                await Task.Delay(250);
                            }
                        }

                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            _vm.AlertVisibility = Visibility.Hidden;
                        });

                        break;
                    }
                }
            }
        }

        private void Open_Click(object sender, MouseButtonEventArgs e)
        {
            Process.Start("explorer.exe", _vm!.VideoCaptureFolder);
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
            if (sender is Border elm)
            {
                var val = elm.Tag.ToString();
                if (val == "0")
                    _vm.Undo();
                else
                    _vm.Redo();
            }
        }

        private void CloseMessageBox(object sender, MouseButtonEventArgs e)
        {
            _vm.MessageBoxVisibilityUsbType = Visibility.Collapsed;
            _vm.OnPropertyChanged(nameof(_vm.MessageBoxVisibilityUsbType));
        }
    }
}