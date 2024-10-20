using DDPM.UI.Common;
using DDPM.UI.Plugin.ViewModels;
using System.Net;
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
        private readonly string Step1 = UI.Resources.Helper.LangHelper.Instance["AddDevice.Pen.1"];

        // 10/15 Derek for RWD
        private readonly Int16 breakPoints = 537;
        private readonly int textBlockWidth = 420;
        private readonly int textBlockWidthRWD = 400;

        public AddPen_OtherRightView(AddDeviceViewModel vm)
        {
            InitializeComponent();
            _vm = vm;

            txtOther.Text = Strings.AddDeviceTypeOther;
            txtCaption.Text = UI.Resources.Helper.LangHelper.Instance["AddDevice.Pen.5"];
            txtStep1.Text = string.Format(Step1, Dns.GetHostName());
            //txtStep2.Text = Step2;

            if (System.Windows.Application.Current?.TryFindResource("breakPoint") is Int16 width)
                breakPoints = width;
        }

        private void UserControl_SizeChanged(object sender, System.Windows.SizeChangedEventArgs e)
        {
            if (this.ActualWidth <= breakPoints)
                ChangeToVerticalLayout();
            else
                ChangeToHorizontalLayout();
        }

        private void ChangeToVerticalLayout()
        {
            stepsStackPanel.Orientation = Orientation.Vertical;

            //change Border size
            //stepsBorder1.Width = stepsBorder2.Width  = 400;
            stepsBorder1.Width = stepsBorder2.Width = stepsStackPanel.Width - 10;
            stepsBorder1.Height = stepsBorder2.Height = 180;

            //change textBlock size
            txtStep1.Width = txtStep2.Width = textBlockWidthRWD;
        }

        private void ChangeToHorizontalLayout()
        {
            stepsStackPanel.Orientation = Orientation.Horizontal;

            //restore Border size
            stepsBorder1.Width = stepsBorder2.Width = 303;
            stepsBorder1.Height = stepsBorder2.Height = 262;

            //restore textBlock size
            txtStep1.Width = txtStep2.Width = textBlockWidth;
        }
    }
}