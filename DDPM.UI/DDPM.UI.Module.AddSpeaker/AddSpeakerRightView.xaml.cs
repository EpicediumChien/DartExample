using DDPM.UI.Plugin.ViewModels;
using System.Windows.Controls;

namespace DDPM.UI.Module.AddSpeaker
{
    /// <summary>
    /// Interaction logic for AddSpeakerRightView.xaml
    /// </summary>
    public partial class AddSpeakerRightView : UserControl
    {
        private readonly AddDeviceViewModel _vm;

        //private readonly string Caption = "Wired Connection";
        //private readonly string Step1 = "Connect your Speaker or soundbar via USB port on your system.";

        public AddSpeakerRightView(AddDeviceViewModel vm)
        {
            InitializeComponent();
            _vm = vm;

            //txtCaption.Text = Caption;
            //txtStep1.Text = Step1;
        }

        private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            txtStep1.Width = this.ActualWidth - 80;
        }
    }
}