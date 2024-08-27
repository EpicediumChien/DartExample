using DDPM.UI.Common.Interfaces;
using DDPM.UI.Common.Models;
using DDPM.UI.Interfaces;
using DDPM.UI.Plugin.ViewModels;
using System.Windows.Controls;

namespace DDPM.UI.Module.PenButtonSettings
{
    public class PenButtonSettingsModule : IDdpmModule
    {
        private UserControl? _leftView = null;
        private UserControl _rightView;
        private readonly PenViewModel _vm;

        private bool isSelectChanged = false;
        public bool IsModuleActive { get; set; } = false;

        public PenButtonSettingsModule(PenViewModel vm)
        {
            _rightView = new PenButtonSettingsRightView(vm);
            _vm = vm;
        }

        public string ModuleName { get => "PenButtonSettingsModule"; }

        public UserControl? GetLeftView()
        {
            return _leftView;
        }

        public UserControl GetRightView()
        {
            return _rightView;
        }

        public HomeDevice? SelectedHomeDevice { get; set; }

        public IModuleOwner? ModuleOwner { get; set; }

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
            ((PenButtonSettingsRightView)_rightView).Initialize();

            if (isSelectChanged)
            {
                isSelectChanged = false;
                InitNewViewModel();
            }
        }

        public void OnDeactivated()
        {
            _vm.ClearSelectedButton();
        }

        #endregion Event Handlers
    }
}