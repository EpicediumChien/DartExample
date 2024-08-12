using System.Windows.Controls;
using DDPM.UI.Plugin.ViewModels;

namespace DDPM.UI.Module.AddWebcam {
  /// <summary>
  /// Interaction logic for AddWebcamRightView.xaml
  /// </summary>
  public partial class AddWebcamRightView : UserControl {
    private readonly AddDeviceViewModel _vm;

    private readonly string Caption = "Wired Connection";
    private readonly string Step1 = "Connect your webcam via USB port on your system";

    public AddWebcamRightView(AddDeviceViewModel vm) {
      InitializeComponent();
      _vm = vm;

      txtCaption.Text = Caption;
      txtStep1.Text = Step1;
    }
  }
}
