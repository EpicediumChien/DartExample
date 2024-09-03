using System.Windows.Controls;
using DDPM.UI.Common.Interfaces;
using DDPM.UI.Common.Models;
using DDPM.UI.Interfaces;
using DDPM.UI.Plugin.ViewModels;

namespace DDPM.UI.Module.AddDisplay {
  public class AddDisplayModule : IDdpmModule {
    private UserControl? _leftView = null;
    private UserControl _rightView;

    public AddDisplayModule(AddDeviceViewModel vm) {
      _rightView = new AddDisplayRightView(vm);

    }
    public string ModuleName { get => "KeyCustomizationModule"; }

    public UserControl? GetLeftView() {
      return _leftView;
    }

    public UserControl GetRightView() {
      return _rightView;
    }
    public HomeDevice SelectedHomeDevice { get; set; }
    public IModuleOwner? ModuleOwner { get; set; }
  }
}
