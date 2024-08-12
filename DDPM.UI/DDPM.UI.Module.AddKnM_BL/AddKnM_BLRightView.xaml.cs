using System.Diagnostics;
using System.Windows.Controls;
using DDPM.UI.Plugin.ViewModels;

namespace DDPM.UI.Module.AddKnM_BL {
  /// <summary>
  /// Interaction logic for AddKnM_BLRightView.xaml
  /// </summary>
  public partial class AddKnM_BLRightView : UserControl {
    private readonly AddDeviceViewModel _vm;

    private readonly string Caption = "Bluetooth Connection";
    private readonly string Caption2 = "Add Bluetooth device by pairing it through Windows settings";
    private readonly string Step1 = "Slide power switch slider to ON";
    private readonly string Step2 = "Select a Bluetooth channel, then press and hold for 3 seconds to make the device discoverable";
    private readonly string Step3 = "Allow the device to be paired or launch Windows settings and select the respective device once it has been discovered";
    private readonly string Step3_1 = "Windows Settings";

    public AddKnM_BLRightView(AddDeviceViewModel vm) {
      InitializeComponent();
      _vm = vm;

      txtCaption.Text = Caption;
      txtCaption2.Text = Caption2;
      txtStep1.Text = Step1;
      txtStep2.Text = Step2;
      txtStep3.Text = Step3;
      txtStep3_1.Text = Step3_1;
    }

    private void OpenWindowsSettings(object sender, System.Windows.Input.MouseButtonEventArgs e) {
      Version win10Version = new(10, 0);
      Version currentVersion = Environment.OSVersion.Version;
#pragma warning disable CA1416
      if(currentVersion >= win10Version) {
        Process.Start(new ProcessStartInfo("ms-settings:bluetooth") {
          UseShellExecute = true
        });
      }
      else {
        Process.Start(new ProcessStartInfo("control", "bthprops.cpl") {
          UseShellExecute = true
        });
      }
#pragma warning restore CA1416
    }
  }
}
