using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using DDPM.UI.Plugin.ViewModels;
using Dell.Client.Framework.UX.WPF.Controls;
using UserControl = System.Windows.Controls.UserControl;

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

        }

        private void Resolution_FullHD_Button_Click(object sender, MouseButtonEventArgs e)
        {

        }

        private void Resolution_HD_Button_Click(object sender, MouseButtonEventArgs e)
        {

        }

        private void FPS_24_Button_Click(object sender, MouseButtonEventArgs e)
        {

        }

        private void FPS_30_Button_Click(object sender, MouseButtonEventArgs e)
        {

        }

        private void FPS_60_Button_Click(object sender, MouseButtonEventArgs e)
        {

        }

        private void Open_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Change_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ChkBoxFramingGrid_Checked(object sender, RoutedEventArgs e)
        {
            Handle(sender as CheckBox);
        }

        private void ChkBoxFramingGrid_Unchecked(object sender, RoutedEventArgs e)
        {
            Handle(sender as CheckBox);
        }

        void Handle(CheckBox checkBox)
        {
            // Use IsChecked.
            bool flag = checkBox.IsChecked.Value;

            _vm.FramingGrid_IsChecked = flag;

            if (flag == true)
                _vm.IsChecked_FramingGrid = Visibility.Visible;
            else
                _vm.IsChecked_FramingGrid = Visibility.Hidden;
        }
    }
}
