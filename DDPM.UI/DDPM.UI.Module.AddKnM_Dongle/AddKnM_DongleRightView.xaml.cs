using DDPM.UI.Plugin.ViewModels;
using System.Windows.Controls;

namespace DDPM.UI.Module.AddKnM_Dongle
{
    /// <summary>
    /// Interaction logic for AddKnM_DongleRightView.xaml
    /// </summary>
    public partial class AddKnM_DongleRightView : UserControl
    {
        private readonly AddDeviceViewModel _vm;

        //private readonly string Step1 = "Connect your USB wireless receiver to your system";
        //private readonly string Step2 = "Slide the power switch to OFF.";
        //private readonly string Step3 = "Press and hold any key/button and slide power to ON";
        //private readonly string Step4 = "Keep this window open. Pairing will begin after a few seconds. If not, repeat steps 2 & 3 to try again.";

        public AddKnM_DongleRightView(AddDeviceViewModel vm)
        {
            InitializeComponent();
            _vm = vm;
            DataContext = _vm;

            //txtStep1.Text = Step1;
            //txtStep2.Text = Step2;
            //txtStep3.Text = Step3;
            //txtStep4.Text = Step4;
        }
    }
}