using DDPM.UI.Plugin.ViewModels;
using System.Windows.Controls;

namespace DDPM.UI.Module.AddPen_Other
{
    /// <summary>
    /// Interaction logic for AddPen_OtherRightView.xaml
    /// </summary>
    public partial class AddPen_OtherRightView : UserControl
    {
        private readonly AddDeviceViewModel _vm;

        //private readonly string Caption = "Connecting your Pen";
        //private readonly string Step1 = "Touch your pen tip to the screen";
        //private readonly string Step2 = "Select 'Yes' to confirm pairing on the prompt box to pair your pen";

        public AddPen_OtherRightView(AddDeviceViewModel vm)
        {
            InitializeComponent();
            _vm = vm;

            //txtCaption.Text = Caption;
            //txtStep1.Text = Step1;
            //txtStep2.Text = Step2;
        }
    }
}