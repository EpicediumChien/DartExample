using System.Diagnostics;
using System.Windows.Controls;
using DDPM.UI.Common.Interfaces;
using DDPM.UI.Common.Models;
using DDPM.UI.Interfaces;
using DDPM.UI.Plugin.ViewModels;

namespace DDPM.UI.Module.KeyCustomization {
  public class KeyCustomizationModule : IDdpmModule {
    private UserControl? _leftView = null;
    private UserControl _rightView;
    readonly KeyboardViewModel _vm;

    public KeyCustomizationModule(KeyboardViewModel vm) {
      _rightView = new KeyCustomizationRightView(vm);
      _vm = vm;
    }
    public string ModuleName { get => "KeyCustomizationModule"; }

    public UserControl? GetLeftView() {
      return (UserControl?)_leftView;
    }

    public UserControl GetRightView() {
      return _rightView;
    }

    public HomeDevice? SelectedHomeDevice { get; set; }

    #region ModuleOwner
    public IModuleOwner? ModuleOwner {
      get => null;
      set { }
    }
    #endregion

    #region Event Handlers
    public void OnSelectedHomeDeviceChanged() {
      Trace.WriteLine("KeyCustomization.OnSelectedHomeDeviceChanged");
    }
    public void OnActivated() {
      ((KeyCustomizationRightView)_rightView).Initialize();
    }
    public void OnDeactivated() {
      _vm.ClearSelectedKey();
    }
    #endregion
  }
}
