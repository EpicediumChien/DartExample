using DDPM.UI.Plugin.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
