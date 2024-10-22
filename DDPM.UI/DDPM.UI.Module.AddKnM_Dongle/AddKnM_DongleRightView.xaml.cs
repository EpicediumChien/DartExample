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

        // 10/15 Derek for RWD
        private readonly int alertTextOriWidth = 300, alertTextWidthRWD = 200;
        private readonly Int16 breakPoints = 537;
        private readonly int textBlockWidth = 190;
        private readonly int textBlockWidthRWD = 200;
        private readonly int bdrWidth = 245, bdrHeight = 220;
        private readonly int bdrWidthRWD = 260, bdrHeightRWD = 200;

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

            txtAlert.Width = bdrAlert.ActualWidth - 50;
        }

        private void ChangeToVerticalLayout()
        {
            stepsStackPanel.Orientation = Orientation.Vertical;

            //change Border size
            stepsBorder1.Width = stepsBorder2.Width = stepsBorder3.Width = stepsBorder4.Width = bdrWidthRWD;
            stepsBorder1.Height = stepsBorder2.Height = stepsBorder3.Height = stepsBorder4.Height = bdrHeightRWD;

            //change textBlock size
            txtStep1.Width = txtStep2.Width = txtStep3.Width = txtStep4.Width = textBlockWidthRWD;
        }

        private void ChangeToHorizontalLayout()
        {
            stepsStackPanel.Orientation = Orientation.Horizontal;

            //restore Border size
            stepsBorder1.Width = stepsBorder2.Width = stepsBorder3.Width = stepsBorder4.Width = bdrWidth;
            stepsBorder1.Height = stepsBorder2.Height = stepsBorder3.Height = stepsBorder4.Height = bdrHeight;

            //restore textBlock size
            txtStep1.Width = txtStep2.Width = txtStep3.Width = txtStep4.Width = textBlockWidth;
        }
    }
}