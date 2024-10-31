using DDPM.UI.Common;
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
        private readonly int breakPoints = 1050;

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

            breakPoints = DdpmCommonHelper.GetBreakPoints();
        }

        //10/13 Derek 需要根据breakpoints来调整布局
        private void UserControl_SizeChanged(object sender, System.Windows.SizeChangedEventArgs e)
        {
            if (this.ActualWidth <= breakPoints - 315)
                ChangeToVerticalLayout();
            else
                ChangeToHorizontalLayout();

            //bdrAlert.Width = this.ActualWidth;
            txtAlert.Width = bdrAlert.ActualWidth - 50;

            AdjustBorderHeight();
        }

        private void AdjustBorderHeight()
        {
            stepsBorder1.Height = stepsBorder3.Height = stepsBorder2.ActualHeight;
        }

        private void ChangeToVerticalLayout()
        {
            stepsStackPanel.Orientation = Orientation.Vertical;

            stepsBorder1.Width = stepsBorder2.Width = stepsBorder3.Width = this.ActualWidth - 58;
            txtStep1.Width = txtStep2.Width = txtStep3.Width = this.ActualWidth - 110;
        }

        private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            AdjustBorderHeight();
        }

        private void ChangeToHorizontalLayout()
        {
            stepsStackPanel.Orientation = Orientation.Horizontal;

            txtStep1.Width = txtStep2.Width = txtStep3.Width = this.ActualWidth / 3 - 70;
            stepsBorder1.Width = stepsBorder2.Width = stepsBorder3.Width = this.ActualWidth / 3 - 25;
        }
    }
}