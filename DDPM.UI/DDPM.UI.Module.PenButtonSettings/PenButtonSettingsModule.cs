using System.Diagnostics;
using System.Windows.Controls;
using DDPM.UI.Common.Interfaces;
using DDPM.UI.Common.Models;
using DDPM.UI.Interfaces;
using DDPM.UI.Plugin.ViewModels;

namespace DDPM.UI.Module.PenButtonSettings {
  public class PenButtonSettingsModule : IDdpmModule {
    private UserControl? _leftView = null;
    private UserControl _rightView;
    readonly PenViewModel _vm;

    public PenButtonSettingsModule(PenViewModel vm) {
      _rightView = new PenButtonSettingsRightView(vm);
      _vm = vm;
    }

    public string ModuleName { get => "PenButtonSettingsModule"; }

    public UserControl? GetLeftView() {
      return _leftView;
    }

    public UserControl GetRightView() {
      return _rightView;
    }
    public HomeDevice? SelectedHomeDevice { get; set; }

    public IModuleOwner? ModuleOwner { get; set; }

    #region Event Handlers
    public void OnSelectedHomeDeviceChanged() {
    }
    public void OnActivated() {
      ((PenButtonSettingsRightView)_rightView).Initialize();
    }
    public void OnDeactivated() {
      _vm.ClearSelectedButton();
    }
    #endregion
  }
}
