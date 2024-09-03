using System.Windows.Controls;
using DDPM.UI.Plugin.ViewModels;

namespace DDPM.UI.Module.AddDisplay {
  /// <summary>
  /// Interaction logic for AddDisplayRightView.xaml
  /// </summary>
  public partial class AddDisplayRightView : UserControl {
    private readonly AddDeviceViewModel _vm;
    public AddDisplayRightView(AddDeviceViewModel vm) {
      InitializeComponent();
      _vm = vm;
    }
  }
}
