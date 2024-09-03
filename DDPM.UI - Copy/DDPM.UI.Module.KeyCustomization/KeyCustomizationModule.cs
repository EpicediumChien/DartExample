using DDPM.UI.Common.Interfaces;
using DDPM.UI.Common.Models;
using DDPM.UI.Interfaces;
using DDPM.UI.Plugin.ViewModels;
using System.Diagnostics;
using System.Windows.Controls;

namespace DDPM.UI.Module.KeyCustomization
{
    public class KeyCustomizationModule : IDdpmModule
    {
        private UserControl? _leftView = null;
        private UserControl _rightView;
        private readonly KeyboardViewModel _vm;

        private bool isSelectChanged = false;
        public bool IsModuleActive { get; set; } = false;

        public KeyCustomizationModule(KeyboardViewModel vm)
        {
            _rightView = new KeyCustomizationRightView(vm);
            _vm = vm;
        }

        public string ModuleName { get => "KeyCustomizationModule"; }

        public UserControl? GetLeftView()
        {
            return (UserControl?)_leftView;
        }

        public UserControl GetRightView()
        {
            return _rightView;
        }

        public HomeDevice? SelectedHomeDevice { get; set; }

        #region ModuleOwner

        public IModuleOwner? ModuleOwner
        {
            get => null;
            set { }
        }

        #endregion ModuleOwner

        #region Event Handlers

        public void OnSelectedHomeDeviceChanged()
        {
            Trace.WriteLine("KeyCustomization.OnSelectedHomeDeviceChanged");
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
            ((KeyCustomizationRightView)_rightView).Initialize();

            if (isSelectChanged)
            {
                isSelectChanged = false;
                InitNewViewModel();
            }
        }

        public void OnDeactivated()
        {
            _vm.ClearSelectedKey();
        }

        #endregion Event Handlers
    }
}