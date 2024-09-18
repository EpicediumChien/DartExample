using DDPM.UI.Plugin.ViewModels;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows;

namespace DDPM.UI.Module.WebCameraMicrophone
{
    /// <summary>
    /// Interaction logic for WebCameraMicrophoneRightView.xaml
    /// </summary>
    public partial class WebCameraMicrophoneRightView : UserControl
    {
        private readonly WebCameraViewModel _vm;

        public WebCameraMicrophoneRightView(WebCameraViewModel vm)
        {
            InitializeComponent();
            _vm = vm;
        }

        private void UXToggleSwitch_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            _vm.CurrentCursor = Cursors.Wait;
            _vm.IsMicEnumerationOnEnabled = false;
            _vm.AlertVisibility = Visibility.Visible;
        }
    }
}