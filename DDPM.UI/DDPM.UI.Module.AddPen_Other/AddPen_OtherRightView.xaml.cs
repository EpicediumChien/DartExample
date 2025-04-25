using DDPM.SA.Common;
using DDPM.UI.Common;
using DDPM.UI.Plugin.Common;
using DDPM.UI.Plugin.ViewModels;
using DDPM.UI.Resources.Helper;
using System.Net;
using System.Reflection.Metadata;
using System.Text.Json;
using System.Text.RegularExpressions;
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
            txtStep1.Text = string.Format(Step1, HostNameHandler.GetHostName());
            //txtStep2.Text = Step2;

            breakPoints = DdpmCommonHelper.GetBreakPoints();
        }

        volatile int IsBLE = -1;
        bool IsConnected = true;
        bool IsSupported = true;
        private void DeviceManagerSA_DeviceChanged(object? sender, SA.Common.DeviceChangedEventArgs e)
        {
            if (e?.device_peripherals != null)
            {
                var device_peripherals_json = JsonSerializer.Serialize(e.device_peripherals);
                DdpmCommonHelper.WriteUILog($"[DeviceManagerSA_DeviceChanged] Add Pen event, device_peripherals : \"{device_peripherals_json}\"");
            }
            if (e.type == DeviceChangedType.Peripherals_SettingsChange && e.changedProperty == "ActivePenInformationChanged")
            {
                var di = e.device_peripherals;
                IsBLE = di.IsBLE ? 1 : 0;
                IsConnected = di.IsConnected;
                IsSupported = di.IsReady;
            }
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
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

            //AdjustBorderHeight();
        }

        private void ChangeToVerticalLayout()
        {
            stepsStackPanel.Orientation = Orientation.Vertical;

            stepsBorder1.Width = stepsBorder2.Width = this.ActualWidth - 38;
            txtStep1.Width = txtStep2.Width = this.ActualWidth - 80;
            stepsBorder2.Height = stepsBorder1.Height = double.NaN;
            stepsBorder2.Margin = new Thickness(0, 0, 0, 7);
        }

        private void ChangeToHorizontalLayout()
        {
            stepsStackPanel.Orientation = Orientation.Horizontal;

            txtStep1.Width = txtStep2.Width = this.ActualWidth / 2 - 70;
            stepsBorder1.Width = stepsBorder2.Width = this.ActualWidth / 2 - 22;
            stepsBorder1.Height = stepsBorder2.Height = 240;
            stepsBorder2.Margin = new Thickness(7, 0, 0, 7);
        }

        private void Pairing(object sender, System.Windows.Input.StylusDownEventArgs e)
        {
            if (e.StylusDevice.TabletDevice.Type != System.Windows.Input.TabletDeviceType.Stylus)
                return;

            Task.Delay(300).Wait();

            if (IsBLE == 0)
            {
                MessageModalDialog messageModalDialog;
                Window mainWindow = System.Windows.Application.Current.MainWindow;
                if (IsConnected)
                {
                    messageModalDialog = new(LangHelper.Instance["Error"], LangHelper.Instance["PairedInfo.8"], LangHelper.Instance["Common.2"]);
                }
                else if (!IsSupported)
                {
                    messageModalDialog = new(LangHelper.Instance["Error"], LangHelper.Instance["Incompatible"], LangHelper.Instance["Common.2"]);
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
            }
            IsBLE = -1;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (DdpmCommonHelper.DeviceManagerSA != null)
            {
                DdpmCommonHelper.DeviceManagerSA.DeviceChanged += DeviceManagerSA_DeviceChanged;
            }
            //AdjustBorderHeight();
        }

        //private void AdjustBorderHeight()
        //{
        //    //stepsBorder1.Height = stepsBorder2.ActualHeight;
        //}
    }
}