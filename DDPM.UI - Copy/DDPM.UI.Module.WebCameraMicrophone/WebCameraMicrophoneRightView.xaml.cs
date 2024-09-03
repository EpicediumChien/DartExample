using DDPM.UI.Plugin.ViewModels;
using System.Windows.Controls;

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
    }
}