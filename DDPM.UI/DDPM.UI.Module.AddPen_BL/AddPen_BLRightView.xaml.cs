using DDPM.UI.Plugin.ViewModels;
using System.Diagnostics;
using System.Net;
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
        //private readonly string Step2 = "Press and hold the top button for 3 seconds. Wait for the device to be discovered by Windows.";
        //private readonly string Step3 = "Allow the device to be paired or launch Windows settings and select the respective device once it has been discovered.";
        //private readonly string Step3_1 = "Windows Settings";

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
            //stepsStackPanel.ActualWidth
        }
    }
}