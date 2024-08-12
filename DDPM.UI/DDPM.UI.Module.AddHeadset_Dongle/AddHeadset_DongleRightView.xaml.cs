using System.Windows.Controls;
using DDPM.UI.Plugin.ViewModels;

namespace DDPM.UI.Module.AddHeadset_Dongle {
  /// <summary>
  /// Interaction logic for AddHeadset_DongleRightView.xaml
  /// </summary>
  public partial class AddHeadset_DongleRightView : UserControl {
    private readonly AddDeviceViewModel _vm;

    private readonly string Step1 = "Connect your USB wireless receiver to your system";
    private readonly string Step2 = "Power OFF headset. Hold mic mute button and power ON again.";
    private readonly string Step3 = "Pairing will automatically begin after a few seconds";

    public AddHeadset_DongleRightView(AddDeviceViewModel vm) {
      InitializeComponent();
      _vm = vm;

      txtStep1.Text = Step1;
      txtStep2.Text = Step2;
      txtStep3.Text = Step3;
    }
  }
}
