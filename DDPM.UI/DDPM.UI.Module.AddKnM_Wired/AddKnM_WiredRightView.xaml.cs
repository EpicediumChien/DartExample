using System.Windows.Controls;
using DDPM.UI.Plugin.ViewModels;

namespace DDPM.UI.Module.AddKnM_Wired {
  /// <summary>
  /// Interaction logic for AddKnM_WiredRightView.xaml
  /// </summary>
  public partial class AddKnM_WiredRightView : UserControl {
    private readonly AddDeviceViewModel _vm;

    private readonly string Caption = "Wired Connection";
    private readonly string Step1 = "Connect your keyboard and mouse via USB port on your system";

    public AddKnM_WiredRightView(AddDeviceViewModel vm) {
      InitializeComponent();
      _vm = vm;

      txtCaption.Text = Caption;
      txtStep1.Text = Step1;
    }
  }
}
