using DDPM.UI.Plugin.ViewModels;
using System.Windows.Controls;
using Windows.Management;

namespace DDPM.UI.Module.AddHeadset_Dongle
{
    /// <summary>
    /// Interaction logic for AddHeadset_DongleRightView.xaml
    /// </summary>
    public partial class AddHeadset_DongleRightView : UserControl
    {
        private readonly AddDeviceViewModel _vm;

        // 10/15 Derek for RWD
        private readonly int alertTextOriWidth = 300, alertTextWidthRWD = 200;
        private readonly Int16 breakPoints = 537;
        private readonly int textBlockWidth = 250;
        private readonly int textBlockWidthRWD = 220;

        //private readonly string Step1 = "Connect your USB wireless receiver to your system";
        //private readonly string Step2 = "Power OFF headset. Hold mic mute button and power ON again.";
        //private readonly string Step3 = "Pairing will automatically begin after a few seconds";

        public AddHeadset_DongleRightView(AddDeviceViewModel vm)
        {
            InitializeComponent();
            _vm = vm;

            //txtStep1.Text = Step1;
            //txtStep2.Text = Step2;
            //txtStep3.Text = Step3;

            if (System.Windows.Application.Current?.TryFindResource("breakPoint") is Int16 width)
                breakPoints = width;
        }

        //10/13 Derek 需要根据breakpoints来调整布局
        private void UserControl_SizeChanged(object sender, System.Windows.SizeChangedEventArgs e)
        {
            if (this.ActualWidth <= breakPoints)
                ChangeToVerticalLayout();
            else
                ChangeToHorizontalLayout();

            //bdrAlert.Width = this.ActualWidth;
            txtAlert.Width = bdrAlert.ActualWidth - 50;
        }

        private void ChangeToVerticalLayout()
        {
            stepsStackPanel.Orientation = Orientation.Vertical;

            //change Border size
            stepsBorder1.Width = stepsBorder2.Width = stepsBorder3.Width = 400;
            stepsBorder1.Height = stepsBorder2.Height = stepsBorder3.Height = 180;

            //change textBlock size
            txtStep1.Width = txtStep2.Width = txtStep3.Width = textBlockWidthRWD;
        }

        private void ChangeToHorizontalLayout()
        {
            stepsStackPanel.Orientation = Orientation.Horizontal;

            //restore Border size
            stepsBorder1.Width = stepsBorder2.Width = stepsBorder3.Width = 303;
            stepsBorder1.Height = stepsBorder2.Height = stepsBorder3.Height = 262;

            //restore textBlock size
            txtStep1.Width = txtStep2.Width = txtStep3.Width = textBlockWidth;
        }
    }
}