using DDPM.UI.Common;
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
        private readonly int breakPoints = 1050;

        private bool userControlLoaded = false;

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

            breakPoints = DdpmCommonHelper.GetBreakPoints();
        }

        private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            userControlLoaded = true;
            AdjustBorderHeight();
        }

        //10/13 Derek 需要根据breakpoints来调整布局
        private void UserControl_SizeChanged(object sender, System.Windows.SizeChangedEventArgs e)
        {
            if (this.ActualWidth <= breakPoints - 315)
                ChangeToVerticalLayout();
            else
                ChangeToHorizontalLayout();

            //Derek 1107 in debug mode ，ActualWidth maybe 0
            if (bdrAlert.ActualWidth > 50)
                txtAlert.Width = bdrAlert.ActualWidth - 50;

            AdjustBorderHeight();
        }

        private void AdjustBorderHeight()
        {
            if (userControlLoaded)
            {
                double maxTextHeight = txtStep1.ActualHeight;
                if (txtStep2.ActualHeight > maxTextHeight) maxTextHeight = txtStep2.ActualHeight;
                if (txtStep3.ActualHeight > maxTextHeight) maxTextHeight = txtStep3.ActualHeight;
                if (txtStep4.ActualHeight > maxTextHeight) maxTextHeight = txtStep4.ActualHeight;
                maxTextHeight += 28; // icon margin
                maxTextHeight += 46; // icon
                maxTextHeight += 15; // stack panel margin
                maxTextHeight += 25; // text margin
                stepsBorder1.Height = maxTextHeight;
                stepsBorder2.Height = maxTextHeight;
                stepsBorder3.Height = maxTextHeight;
                stepsBorder4.Height = maxTextHeight;
            }
        }

        private void ChangeToVerticalLayout()
        {
            stepsStackPanel.Orientation = Orientation.Vertical;

            stepsBorder1.Width = stepsBorder2.Width = stepsBorder3.Width = stepsBorder4.Width = this.ActualWidth - 58;
            txtStep1.Width = txtStep2.Width = txtStep3.Width = txtStep4.Width = this.ActualWidth  - 110;
        }

        private void ChangeToHorizontalLayout()
        {
            stepsStackPanel.Orientation = Orientation.Horizontal;

            stepsBorder1.Width = stepsBorder2.Width = stepsBorder3.Width = stepsBorder4.Width = this.ActualWidth / 4 - 18;
            txtStep1.Width = txtStep2.Width = txtStep3.Width = txtStep4.Width = this.ActualWidth / 4 - 70;
        }
    }
}