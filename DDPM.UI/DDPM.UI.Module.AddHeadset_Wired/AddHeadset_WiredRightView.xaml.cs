using DDPM.UI.Plugin.ViewModels;
using System.Windows.Controls;

namespace DDPM.UI.Module.AddHeadset_Wired
{
    /// <summary>
    /// Interaction logic for AddHeadset_WiredRightView.xaml
    /// </summary>
    public partial class AddHeadset_WiredRightView : UserControl
    {
        private readonly AddDeviceViewModel _vm;

        //private readonly string Caption = "Wired Connection";
        //private readonly string Step1 = "Connect your headset via USB port on your system.";

        public AddHeadset_WiredRightView(AddDeviceViewModel vm)
        {
            InitializeComponent();
            _vm = vm;

            //txtCaption.Text = Caption;
            //txtStep1.Text = Step1;
        }
    }
}