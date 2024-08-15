using DDPM.UI.Common.Interfaces;
using DDPM.UI.Common.Models;
using DDPM.UI.Interfaces;
using DDPM.UI.Plugin.ViewModels;
using System.Windows.Controls;

namespace DDPM.UI.Module.ButtonSettings
{
    public class ButtonSettingsModule : IDdpmModule
    {
        private UserControl? _leftView = null;
        private UserControl _rightView;
        private readonly MouseViewModel _vm;

        public ButtonSettingsModule(MouseViewModel vm)
        {
            _rightView = new ButtonSettingsRightView(vm);
            _vm = vm;
        }

        public string ModuleName { get => "ButtonSettingsModule"; }

        public UserControl? GetLeftView()
        {
            return _leftView;
        }

        public UserControl GetRightView()
        {
            return _rightView;
        }

        public HomeDevice? SelectedHomeDevice { get; set; }

        #region ModuleOwner

        public IModuleOwner? ModuleOwner
        {
            //get => vm.ModuleOwner;
            //set => vm.ModuleOwner = value;
            get;
            set;
        }

        #endregion ModuleOwner

        #region Event Handlers

        public void OnSelectedHomeDeviceChanged()
        {
        }

        public void OnActivated()
        {
            ((ButtonSettingsRightView)_rightView).Initialize();
        }

        public void OnDeactivated()
        {
            _vm.ClearSelectedButton();
        }

        #endregion Event Handlers
    }
}