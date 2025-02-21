using DDPM.SA.Common;
using DDPM.UI.Common;
using DDPM.UI.Plugin.Common;
using DDPM.UI.Plugin.ViewModels;
using System.Net;
using System.Reflection.Metadata;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using static System.Net.Mime.MediaTypeNames;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;

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
        private readonly int breakPoints = 1050;

        public AddPen_OtherRightView(AddDeviceViewModel vm)
        {
            InitializeComponent();
            _vm = vm;

            txtOther.Text = Strings.AddDeviceTypeOther;
            txtCaption.Text = UI.Resources.Helper.LangHelper.Instance["AddDevice.Pen.5"];
            txtStep1.Text = string.Format(Step1, Dns.GetHostName());
            //txtStep2.Text = Step2;

            breakPoints = DdpmCommonHelper.GetBreakPoints();
            Unloaded += AddPen_OtherRightView_Unloaded;
            if (DdpmCommonHelper.DeviceManagerSA != null)
            {
                DdpmCommonHelper.DeviceManagerSA.DeviceChanged += DeviceManagerSA_DeviceChanged;
            }
        }

        bool IsBLE = true;
        bool IsConnected = true;
        private void DeviceManagerSA_DeviceChanged(object? sender, SA.Common.DeviceChangedEventArgs e)
        {
            if (e.type == DeviceChangedType.Peripherals_SettingsChange && e.changedProperty == "ActivePenInformationChanged")
            {
                var di = e.device_peripherals;
                IsBLE = di.IsBLE;
                IsConnected = di.IsConnected;
            }
        }

        private void AddPen_OtherRightView_Unloaded(object sender, RoutedEventArgs e)
        {
            if (DdpmCommonHelper.DeviceManagerSA != null)
            {
                DdpmCommonHelper.DeviceManagerSA.DeviceChanged -= DeviceManagerSA_DeviceChanged;
            }
        }

        private void UserControl_SizeChanged(object sender, System.Windows.SizeChangedEventArgs e)
        {
            if (this.ActualWidth <= breakPoints - 315)
                ChangeToVerticalLayout();
            else
                ChangeToHorizontalLayout();

            AdjustBorderHeight();
        }

        private void ChangeToVerticalLayout()
        {
            stepsStackPanel.Orientation = Orientation.Vertical;

            stepsBorder1.Width = stepsBorder2.Width = this.ActualWidth - 58;
            txtStep1.Width = txtStep2.Width = this.ActualWidth - 110;
        }

        private void ChangeToHorizontalLayout()
        {
            stepsStackPanel.Orientation = Orientation.Horizontal;

            txtStep1.Width = txtStep2.Width = this.ActualWidth / 2 - 70;
            stepsBorder1.Width = stepsBorder2.Width = this.ActualWidth / 2 - 25;
        }

        private void Pairing(object sender, System.Windows.Input.StylusDownEventArgs e)
        {
            Thread.Sleep(300);

            if (!IsBLE)
            {
                MessageModalDialog messageModalDialog;
                Window mainWindow = System.Windows.Application.Current.MainWindow;
                if (IsConnected)
                {
                    messageModalDialog = new(Strings.Error, Strings.PenAlreadyPaired, Strings.Cancel);
                }
                else
                {
                    messageModalDialog = new(Strings.PairYourPen, Strings.PairYourPenMessage, Strings.No, Strings.Yes);
                }
                if (mainWindow != null)
                {
                    messageModalDialog.Owner = mainWindow;
                    messageModalDialog.Left = mainWindow.Left + (mainWindow!.ActualWidth - 420) / 2;
                    messageModalDialog.Top = mainWindow.Top + 300;
                }
                if (messageModalDialog.ShowDialog()!.Value)
                {
                    _vm.StartPairingPen();
                }
                //IsBLE = true;
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            AdjustBorderHeight();
        }

        private void AdjustBorderHeight()
        {
            stepsBorder1.Height = stepsBorder2.ActualHeight;
        }
    }
}