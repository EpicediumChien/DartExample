using DDPM.UI.Plugin.ViewModels;
using System.Windows.Controls;

namespace DDPM.UI.Module.AddDisplay
{
    /// <summary>
    /// Interaction logic for AddDisplayRightView.xaml
    /// </summary>
    public partial class AddDisplayRightView : UserControl
    {
        private readonly AddDeviceViewModel _vm;

        private readonly string Caption = "Wired Connection";
        private readonly string Step1 = "Connect your display via HDMI/USB-C port on your system";

        public AddDisplayRightView(AddDeviceViewModel vm)
        {
            InitializeComponent();
            _vm = vm;

            txtCaption.Text = Caption;
            txtStep1.Text = Step1;
        }
    }
}