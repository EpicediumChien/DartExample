using System.Diagnostics;
using System.Windows.Controls;
using DDPM.UI.Common.Interfaces;
using DDPM.UI.Common.Models;
using DDPM.UI.Interfaces;
using DDPM.UI.Plugin.ViewModels;

namespace DDPM.UI.Module.AddPen_Other {
  public class AddPen_OtherModule : IDdpmModule {
    readonly UserControl? _leftView;
    readonly UserControl? _rightView;

    public AddPen_OtherModule() { }
    public AddPen_OtherModule(AddDeviceViewModel vm) {
      _rightView = new AddPen_OtherRightView(vm);
    }
    public string ModuleName { get => "AddPen_OtherModule"; }

    public UserControl? GetLeftView() {
      return _leftView;
    }

    public UserControl GetRightView() {
      return _rightView!;
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
