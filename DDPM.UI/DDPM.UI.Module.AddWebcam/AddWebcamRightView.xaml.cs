using DDPM.UI.Common;
using DDPM.UI.Plugin.ViewModels;
using System.Windows.Controls;

namespace DDPM.UI.Module.AddWebcam
{
    /// <summary>
    /// Interaction logic for AddWebcamRightView.xaml
    /// </summary>
    public partial class AddWebcamRightView : UserControl
    {
        private readonly AddDeviceViewModel _vm;

        //private readonly string Caption = "Wired Connection";
        //private readonly string Step1 = "Connect your webcam via USB port on your system";

        public AddWebcamRightView(AddDeviceViewModel vm)
        {
            InitializeComponent();
            _vm = vm;

            //txtCaption.Text = Caption;
            //txtStep1.Text = Step1;
        }

        private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                txtStep1.Width = this.ActualWidth - 80;
            }
            catch (System.Exception ex)
            {
                DdpmCommonHelper.WriteUILog($"[AddWebcamRightView] exception: {ex}");
            }
        }
    }
}