using DDPM.UI.Common.Interfaces;
using DDPM.UI.Common.Models;
using DDPM.UI.Interfaces;
using DDPM.UI.Plugin.ViewModels;
using System.Windows.Controls;

namespace DDPM.UI.Module.MouseSettings
{
    public class MouseSettingsModule : IDdpmModule
    {
        private UserControl? _leftView = null;
        private UserControl _rightView;

        private bool isSelectChanged = false;
        public bool IsModuleActive { get; set; } = false;

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

        #endregion ModuleOwner

        #region Event Handlers

        public void OnSelectedHomeDeviceChanged()
        {
            isSelectChanged = true;
            if (IsModuleActive)
            {
                isSelectChanged = false;
                InitNewViewModel();
            }
        }

        //Handle new device coming
        private void InitNewViewModel()
        {
        }

        public void OnActivated()
        {
            if (isSelectChanged)
            {
                isSelectChanged = false;
                InitNewViewModel();
            }
        }

        public void OnDeactivated()
        {
        }

        #endregion Event Handlers
    }
}