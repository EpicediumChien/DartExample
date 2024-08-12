using System.Diagnostics;
using System.Windows.Controls;
using DDPM.UI.Common.Interfaces;
using DDPM.UI.Common.Models;
using DDPM.UI.Interfaces;
using DDPM.UI.Plugin.ViewModels;

namespace DDPM.UI.Module.AddHeadset_Wired {
  public class AddHeadset_WiredModule : IDdpmModule {
    readonly UserControl? _leftView = null;
    readonly UserControl _rightView;

    public AddHeadset_WiredModule(AddDeviceViewModel vm) {
      _rightView = new AddHeadset_WiredRightView(vm);
    }
    public string ModuleName { get => "AddHeadset_WiredModule"; }

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
