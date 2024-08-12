using System.Windows.Controls;
using DDPM.UI.Common.Interfaces;
using DDPM.UI.Common.Models;
using DDPM.UI.Interfaces;
using DDPM.UI.Module.Illumination;
using DDPM.UI.Plugin.ViewModels;

namespace DDPM.UI.Module.Illumination {
  public class IlluminationModule : IDdpmModule {
    private UserControl? _leftView = null;
    private UserControl _rightView;

    public IlluminationModule(KeyboardViewModel vm) {
      _rightView = new IlluminationRightView(vm);
      //_rightView.DataContext = vm;

    }
    public string ModuleName { get => "IlluminationModule"; }

    public UserControl? GetLeftView() {
      return _leftView;
    }

    public UserControl GetRightView() {
      return _rightView;
    }
        public HomeDevice? SelectedHomeDevice { get; set; }
        #region ModuleOwner
        public IModuleOwner? ModuleOwner
        {
            get => null;
            set { }
        }
        #endregion

        #region Event Handlers
        public void OnSelectedHomeDeviceChanged()
        {

        }
        public void OnActivated()
        {

        }
        public void OnDeactivated()
        {

        }
        #endregion
    }

}
