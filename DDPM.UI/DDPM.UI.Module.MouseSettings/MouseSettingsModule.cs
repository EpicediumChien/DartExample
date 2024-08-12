using System.Windows.Controls;
using DDPM.UI.Common.Interfaces;
using DDPM.UI.Common.Models;
using DDPM.UI.Interfaces;
using DDPM.UI.Module.MouseSettings;
using DDPM.UI.Plugin.ViewModels;

namespace DDPM.UI.Module.MouseSettings
{
    public class MouseSettingsModule : IDdpmModule
    {
        private UserControl? _leftView = null;
        private UserControl _rightView;

        public MouseSettingsModule(MouseViewModel vm)
        {
            _rightView = new MouseSettingsRightView(vm);

        }
        public string ModuleName { get => "MouseSettingsModule"; }

        public UserControl? GetLeftView()
        {
            return _leftView;
        }

        public UserControl GetRightView()
        {
            return _rightView;
        }
        public HomeDevice SelectedHomeDevice { get; set; }
        #region ModuleOwner
        public IModuleOwner? ModuleOwner
        {
            //get => vm.ModuleOwner;
            //set => vm.ModuleOwner = value;
            get;
            set;
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
