using System.Diagnostics;
using System.Windows.Controls;
using DDPM.UI.Common.Interfaces;
using DDPM.UI.Common.Models;
using DDPM.UI.Interfaces;
using DDPM.UI.Plugin.ViewModels;

namespace DDPM.UI.Module.AddKnM_BL {
  public class AddKnM_BLModule : IDdpmModule {
    readonly UserControl? _leftView = null;
    readonly UserControl _rightView;

    public AddKnM_BLModule(AddDeviceViewModel vm) {
      _rightView = new AddKnM_BLRightView(vm);
    }
    public string ModuleName { get => "AddKnM_BLModule"; }

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
      Trace.WriteLine("BrightnessModule.OnSelectedHomeDeviceChanged");
    }
    public void OnActivated() {
      Trace.WriteLine("BrightnessModule.OnActivated");
    }
    public void OnDeactivated() {
      Trace.WriteLine("BrightnessModule.OnDeactivated");
    }
    #endregion
  }
}
