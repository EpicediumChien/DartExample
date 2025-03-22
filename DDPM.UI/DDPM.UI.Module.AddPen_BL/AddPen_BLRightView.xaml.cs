using DDPM.SA.Common;
using DDPM.UI.Common;
using DDPM.UI.Plugin.Common;
using DDPM.UI.Plugin.ViewModels;
using DDPM.UI.Resources.Helper;
using System.Diagnostics;
using System.Net;
using System.Windows;
using System.Windows.Controls;

namespace DDPM.UI.Module.AddPen_BL
{
    /// <summary>
    /// Interaction logic for AddPen_BLRightView.xaml
    /// </summary>
    public partial class AddPen_BLRightView : UserControl
    {
        private readonly AddDeviceViewModel _vm;

        //private readonly string Caption = "Connecting your Pen";
        private readonly string Step1 = UI.Resources.Helper.LangHelper.Instance["AddDevice.Pen.1"];

        // 10/15 Derek for RWD  -- not tested yet due to no device
        private readonly int breakPoints = 1050;

        public AddPen_BLRightView(AddDeviceViewModel vm)
        {
            InitializeComponent();
            _vm = vm;

            //txtCaption.Text = Caption;
            //txtStep1.Text = string.Format(Step1, Dns.GetHostName());
            txtStep1.Text = Step1;
            //txtStep2.Text = Step2;
            //txtStep3.Text = Step3;
            //txtStep3_1.Text = Step3_1;

            breakPoints = DdpmCommonHelper.GetBreakPoints();
        }

        private void DeviceManagerSA_DeviceChanged(object? sender, SA.Common.DeviceChangedEventArgs e)
        {
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

        private void OpenWindowsSettings(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Version win10Version = new(10, 0);
            Version currentVersion = Environment.OSVersion.Version;
#pragma warning disable CA1416
            if (currentVersion >= win10Version)
            {
                Process.Start(new ProcessStartInfo("ms-settings:bluetooth")
                {
                    UseShellExecute = true
                });
            }
            else
            {
                Process.Start(new ProcessStartInfo("control", "bthprops.cpl")
                {
                    UseShellExecute = true
                });
            }
#pragma warning restore CA1416
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

            stepsBorder1.Width = stepsBorder2.Width = stepsBorder3.Width = this.ActualWidth - 58;
            txtStep1.Width = txtStep2.Width = txtStep3.Width = this.ActualWidth - 110;
        }

        private void ChangeToHorizontalLayout()
        {
            stepsStackPanel.Orientation = Orientation.Horizontal;

            txtStep1.Width = txtStep2.Width = txtStep3.Width = this.ActualWidth / 3 - 70;
            stepsBorder1.Width = stepsBorder2.Width = stepsBorder3.Width = this.ActualWidth / 3 - 25;
        }

        private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            if (DdpmCommonHelper.DeviceManagerSA != null)
            {
                DdpmCommonHelper.DeviceManagerSA.DeviceChanged += DeviceManagerSA_DeviceChanged;
            }
            AdjustBorderHeight();
        }

        private void AdjustBorderHeight()
        {
            stepsBorder1.Height = stepsBorder2.Height = stepsBorder3.ActualHeight;
        }

        volatile int IsBLE = -1;
        bool IsConnected = true;
        bool IsSupported = true;
        private void Pairing(object sender, System.Windows.Input.StylusDownEventArgs e)
        {
            if (e.StylusDevice.TabletDevice.Type != System.Windows.Input.TabletDeviceType.Stylus)
                return;

            Thread.Sleep(300);

            if (IsBLE == 1)
            {
                MessageModalDialog messageModalDialog;
                string msg;
                if (IsConnected)
                {
                    msg = LangHelper.Instance["PairedInfo.8"];
                }
                else if (!IsSupported)
                {
                    msg = LangHelper.Instance["Incompatible"];
                }
                else
                { return; }

                Window mainWindow = System.Windows.Application.Current.MainWindow;
                messageModalDialog = new(LangHelper.Instance["Error"], msg, LangHelper.Instance["Common.2"]);
                if (mainWindow != null)
                {
                    messageModalDialog.Owner = mainWindow;
                    messageModalDialog.Left = mainWindow.Left + (mainWindow!.ActualWidth - 420) / 2;
                    messageModalDialog.Top = mainWindow.Top + 300;
                }
                messageModalDialog.ShowDialog();
            }
            IsBLE = -1;
        }
    }
}