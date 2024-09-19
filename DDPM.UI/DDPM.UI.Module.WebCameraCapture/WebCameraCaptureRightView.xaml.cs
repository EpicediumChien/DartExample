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

            var picturesLibrary = StorageLibrary.GetLibraryAsync(KnownLibraryId.Pictures);
            // Fall back to the local app storage if the Pictures Library is not available
            //Windows.Storage.StorageFolder _captureFolder = picturesLibrary. ?? ApplicationData.Current.LocalFolder;
            tbx_media_file_location.Text = Environment.GetFolderPath(Environment.SpecialFolder.MyVideos);
            _vm.Media_File_Location = tbx_media_file_location.Text;
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

        private void Resolution_4KUHD_Button_Click(object sender, MouseButtonEventArgs e)
        {
            if (_vm.captureManagerInitialized)
            {
                _vm.SetResolution_Selected(0);

                _vm.strCurrent_Resolution = "3840x2160";

                foreach (var property in _vm.allProperties)
                {
                    string properties_temp = property.GetFriendlyName();

                    if (properties_temp.Contains(_vm.strCurrent_Resolution, StringComparison.OrdinalIgnoreCase) && properties_temp.Contains(_vm.strCurrent_Framerate, StringComparison.OrdinalIgnoreCase))
                    {
                        var encodingProperties = property.EncodingProperties;
                        _vm._mediaCapture.VideoDeviceController.SetMediaStreamPropertiesAsync(MediaStreamType.VideoPreview, encodingProperties);
                        break;
                    }
                }
            }
        }

        private void Resolution_FullHD_Button_Click(object sender, MouseButtonEventArgs e)
        {
            if (_vm.captureManagerInitialized)
            {
                _vm.SetResolution_Selected(1);

                _vm.strCurrent_Resolution = "1920x1080";

                foreach (var property in _vm.allProperties)
                {
                    string properties_temp = property.GetFriendlyName();

                    if (properties_temp.Contains(_vm.strCurrent_Resolution, StringComparison.OrdinalIgnoreCase) && properties_temp.Contains(_vm.strCurrent_Framerate, StringComparison.OrdinalIgnoreCase))
                    {
                        var encodingProperties = property.EncodingProperties;
                        _vm._mediaCapture.VideoDeviceController.SetMediaStreamPropertiesAsync(MediaStreamType.VideoPreview, encodingProperties);
                        break;
                    }

                }
            }
        }

        private void Resolution_HD_Button_Click(object sender, MouseButtonEventArgs e)
        {
            if (_vm.captureManagerInitialized)
            {
                _vm.SetResolution_Selected(2);

                _vm.strCurrent_Resolution = "1280x720";

                foreach (var property in _vm.allProperties)
                {
                    string properties_temp = property.GetFriendlyName();

                    if (properties_temp.Contains(_vm.strCurrent_Resolution, StringComparison.OrdinalIgnoreCase) && properties_temp.Contains(_vm.strCurrent_Framerate, StringComparison.OrdinalIgnoreCase))
                    {
                        var encodingProperties = property.EncodingProperties;
                        _vm._mediaCapture.VideoDeviceController.SetMediaStreamPropertiesAsync(MediaStreamType.VideoPreview, encodingProperties);
                        break;
                    }

                }
            }
        }

        private void FPS_24_Button_Click(object sender, MouseButtonEventArgs e)
        {
            if (_vm.captureManagerInitialized)
            {
                _vm.SetFPS_Selected(0);

                _vm.strCurrent_Framerate = "24FPS";

                foreach (var property in _vm.allProperties)
                {
                    string properties_temp = property.GetFriendlyName();

                    if (properties_temp.Contains(_vm.strCurrent_Resolution, StringComparison.OrdinalIgnoreCase) && properties_temp.Contains(_vm.strCurrent_Framerate, StringComparison.OrdinalIgnoreCase))
                    {
                        var encodingProperties = property.EncodingProperties;
                        _vm._mediaCapture.VideoDeviceController.SetMediaStreamPropertiesAsync(MediaStreamType.VideoPreview, encodingProperties);
                        break;
                    }

                }

            }
        }

        private void FPS_30_Button_Click(object sender, MouseButtonEventArgs e)
        {
            if (_vm.captureManagerInitialized)
            {
                _vm.SetFPS_Selected(1);

                _vm.strCurrent_Framerate = "30FPS";

                foreach (var property in _vm.allProperties)
                {
                    string properties_temp = property.GetFriendlyName();

                    if (properties_temp.Contains(_vm.strCurrent_Resolution, StringComparison.OrdinalIgnoreCase) && properties_temp.Contains(_vm.strCurrent_Framerate, StringComparison.OrdinalIgnoreCase))
                    {
                        var encodingProperties = property.EncodingProperties;
                        _vm._mediaCapture.VideoDeviceController.SetMediaStreamPropertiesAsync(MediaStreamType.VideoPreview, encodingProperties);
                        break;
                    }

                }
            }
        }

        private void FPS_60_Button_Click(object sender, MouseButtonEventArgs e)
        {
            if (_vm.captureManagerInitialized)
            {
                _vm.SetFPS_Selected(2);

                _vm.strCurrent_Framerate = "60FPS";

                foreach (var property in _vm.allProperties)
                {
                    string properties_temp = property.GetFriendlyName();

                    if (properties_temp.Contains(_vm.strCurrent_Resolution, StringComparison.OrdinalIgnoreCase) && properties_temp.Contains(_vm.strCurrent_Framerate, StringComparison.OrdinalIgnoreCase))
                    {
                        var encodingProperties = property.EncodingProperties;
                        _vm._mediaCapture.VideoDeviceController.SetMediaStreamPropertiesAsync(MediaStreamType.VideoPreview, encodingProperties);
                        break;
                    }
                }
            }
        }

        private void Open_Click(object sender, RoutedEventArgs e)
        {
            var folderDialog = new OpenFolderDialog
            {
                // Set options here
            };

            if (folderDialog.ShowDialog() == true)
            {
                var folderName = folderDialog.FolderName;

                tbx_media_file_location.Text = folderName;
            }

            /*
            var folderPicker = new Windows.Storage.Pickers.FolderPicker();
            folderPicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.Desktop;
            folderPicker.FileTypeFilter.Add("*");



            Windows.Storage.StorageFolder folder = await folderPicker.PickSingleFolderAsync();
            
            if (folder != null)
            {
                // Application now has read/write access to all contents in the picked folder
                // (including other sub-folder contents)
                Windows.Storage.AccessCache.StorageApplicationPermissions.
                FutureAccessList.AddOrReplace("PickedFolderToken", folder);

                tbx_media_file_location.Text = folder.Path;
            }
            else
            {
            }
            */
        }

        private void Change_Click(object sender, RoutedEventArgs e)
        {

            _vm.Media_File_Location = tbx_media_file_location.Text;
        }

        private void ChkBoxFramingGrid_Checked(object sender, RoutedEventArgs e)
        {
            Handle_ChkBoxFramingGrid_Checked(sender as CheckBox);
        }

        private void ChkBoxFramingGrid_Unchecked(object sender, RoutedEventArgs e)
        {
            Handle_ChkBoxFramingGrid_Checked(sender as CheckBox);
        }

        private void Handle_ChkBoxFramingGrid_Checked(CheckBox checkBox)
        {
            // Use IsChecked.
            bool flag = checkBox.IsChecked.Value;

            _vm.FramingGrid_IsChecked = flag;

            if (flag == true)
                _vm.IsChecked_FramingGrid = Visibility.Visible;
            else
                _vm.IsChecked_FramingGrid = Visibility.Hidden;
        }

        private void ChkBoxCountdown_Checked(object sender, RoutedEventArgs e)
        {
            Handle_ChkBoxCountdown_Checked(sender as CheckBox);
        }

        private void ChkBoxCountdown_Unchecked(object sender, RoutedEventArgs e)
        {
            Handle_ChkBoxCountdown_Checked(sender as CheckBox);
        }

        private void Handle_ChkBoxCountdown_Checked(CheckBox checkBox)
        {
            // Use IsChecked.
            bool flag = checkBox.IsChecked.Value;

            _vm.Countdown_IsChecked = flag;

            /*
            if (flag == true)
                _vm.Countdown_IsChecked = true;
            else
                _vm.Countdown_IsChecked = false;
            */
        }
    }

}