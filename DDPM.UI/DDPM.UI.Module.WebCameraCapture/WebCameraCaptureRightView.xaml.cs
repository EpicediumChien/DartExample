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
        }

        private void Resolution_4KUHD_Button_Click(object sender, MouseButtonEventArgs e)
        {
            _vm.SetResolution_Selected(0);

            _vm.strCurrent_Resolution = "3840x2160";

            foreach (var property in _vm.allProperties)
            {
                string properties_temp = property.GetFriendlyName();

                if (properties_temp.Contains(_vm.strCurrent_Resolution, StringComparison.OrdinalIgnoreCase) && properties_temp.Contains(_vm.strCurrent_Framerate, StringComparison.OrdinalIgnoreCase))
                {
                    var encodingProperties = property.EncodingProperties;
                    _ = _vm.MediaCapture!.VideoDeviceController.SetMediaStreamPropertiesAsync(MediaStreamType.VideoPreview, encodingProperties);
                    break;
                }
            }
        }

        private void Resolution_FullHD_Button_Click(object sender, MouseButtonEventArgs e)
        {
            _vm.SetResolution_Selected(1);

            _vm.strCurrent_Resolution = "1920x1080";

            foreach (var property in _vm.allProperties)
            {
                string properties_temp = property.GetFriendlyName();

                if (properties_temp.Contains(_vm.strCurrent_Resolution, StringComparison.OrdinalIgnoreCase) && properties_temp.Contains(_vm.strCurrent_Framerate, StringComparison.OrdinalIgnoreCase))
                {
                    var encodingProperties = property.EncodingProperties;
                    _vm.MediaCapture.VideoDeviceController.SetMediaStreamPropertiesAsync(MediaStreamType.VideoPreview, encodingProperties);
                    break;
                }

            }
        }

        private void Resolution_HD_Button_Click(object sender, MouseButtonEventArgs e)
        {
            _vm.SetResolution_Selected(2);

            _vm.strCurrent_Resolution = "1280x720";

            foreach (var property in _vm.allProperties)
            {
                string properties_temp = property.GetFriendlyName();

                if (properties_temp.Contains(_vm.strCurrent_Resolution, StringComparison.OrdinalIgnoreCase) && properties_temp.Contains(_vm.strCurrent_Framerate, StringComparison.OrdinalIgnoreCase))
                {
                    var encodingProperties = property.EncodingProperties;
                    _vm.MediaCapture.VideoDeviceController.SetMediaStreamPropertiesAsync(MediaStreamType.VideoPreview, encodingProperties);
                    break;
                }

            }
        }

        private void FPS_24_Button_Click(object sender, MouseButtonEventArgs e)
        {
            _vm.SetFPS_Selected(0);

            _vm.strCurrent_Framerate = "24FPS";

            foreach (var property in _vm.allProperties)
            {
                string properties_temp = property.GetFriendlyName();

                if (properties_temp.Contains(_vm.strCurrent_Resolution, StringComparison.OrdinalIgnoreCase) && properties_temp.Contains(_vm.strCurrent_Framerate, StringComparison.OrdinalIgnoreCase))
                {
                    var encodingProperties = property.EncodingProperties;
                    _vm.MediaCapture.VideoDeviceController.SetMediaStreamPropertiesAsync(MediaStreamType.VideoPreview, encodingProperties);
                    break;
                }

            }
        }

        private void FPS_30_Button_Click(object sender, MouseButtonEventArgs e)
        {
            _vm.SetFPS_Selected(1);

            _vm.strCurrent_Framerate = "30FPS";

            foreach (var property in _vm.allProperties)
            {
                string properties_temp = property.GetFriendlyName();

                if (properties_temp.Contains(_vm.strCurrent_Resolution, StringComparison.OrdinalIgnoreCase) && properties_temp.Contains(_vm.strCurrent_Framerate, StringComparison.OrdinalIgnoreCase))
                {
                    var encodingProperties = property.EncodingProperties;
                    _vm.MediaCapture.VideoDeviceController.SetMediaStreamPropertiesAsync(MediaStreamType.VideoPreview, encodingProperties);
                    break;
                }

            }
        }

        private void FPS_60_Button_Click(object sender, MouseButtonEventArgs e)
        {
            _vm.SetFPS_Selected(2);

            _vm.strCurrent_Framerate = "60FPS";

            foreach (var property in _vm.allProperties)
            {
                string properties_temp = property.GetFriendlyName();

                if (properties_temp.Contains(_vm.strCurrent_Resolution, StringComparison.OrdinalIgnoreCase) && properties_temp.Contains(_vm.strCurrent_Framerate, StringComparison.OrdinalIgnoreCase))
                {
                    var encodingProperties = property.EncodingProperties;
                    _vm.MediaCapture.VideoDeviceController.SetMediaStreamPropertiesAsync(MediaStreamType.VideoPreview, encodingProperties);
                    break;
                }
            }
        }

        private void Open_Click(object sender, RoutedEventArgs e)
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
    }
}