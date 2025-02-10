using DDPM.UI.Common;
using DDPM.UI.Plugin.ViewModels;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DDPM.UI.Module.AddHeadset_BL
{
    /// <summary>
    /// Interaction logic for AddHeadset_BLRightView.xaml
    /// </summary>
    public partial class AddHeadset_BLRightView : UserControl
    {
        private readonly AddDeviceViewModel _vm;

        // 10/14 Derek add for RWD
        private readonly int breakPoints = 1050;

        //private readonly string Caption = "Bluetooth Connection";
        //private readonly string Caption2 = "For USB wireless receiver free and on-the-go connectivity";
        //private readonly string Step1 = "Power ON headset";
        //private readonly string Step2 = "Press and hold power button for 3 seconds to make the headset discoverable by Bluetooth";
        //private readonly string Step3 = "Allow the device to be paired or launch Windows settings and select the respective device once it has been discovered.";
        //private readonly string Step3_1 = "Windows Settings";

        public AddHeadset_BLRightView(AddDeviceViewModel vm)
        {
            InitializeComponent();
            _vm = vm;

            //txtCaption.Text = Caption;
            //txtCaption2.Text = Caption2;
            //txtStep1.Text = Step1;
            //txtStep2.Text = Step2;
            //txtStep3.Text = Step3;
            //txtStep3_1.Text = Step3_1;

            breakPoints = DdpmCommonHelper.GetBreakPoints();
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

        // 10/13 Derek 需要根据breakpoints来调整布局
        private void UserControl_SizeChanged(object sender, System.Windows.SizeChangedEventArgs e)
        {

            if (this.ActualWidth <= breakPoints - 315)
                ChangeToVerticalLayout();
            else
                ChangeToHorizontalLayout();

            AdjustBorderHeight();
        }

        private void AdjustBorderHeight()
        {
            stepsBorder1.Height = stepsBorder2.Height = stepsBorder3.ActualHeight;
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

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            AdjustBorderHeight();
        }

        //Robert_Lin 2025-2-9 for Narrator, when user press [Enter] on "Windows Settings" text
        private void windowsSettings_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                e.Handled = true;
                //Redirect to OpenWindowsSettings()
                var mouseDevice = InputManager.Current.PrimaryMouseDevice;
                if (mouseDevice != null)
                {
                    var args = new MouseButtonEventArgs(mouseDevice, 0, MouseButton.Left)
                    {
                        RoutedEvent = UIElement.MouseLeftButtonDownEvent
                    };
                    OpenWindowsSettings(sender, args);
                }
            }
        }
    }
}