using DDPM.SA.Common.Settings;
using DDPM.UI.Common;
using DDPM.UI.Common.Interfaces;
using DDPM.UI.Common.Models;
using DDPM.UI.Interfaces;
using System.Diagnostics;
using System.Windows.Controls;

namespace DDPM.UI.Module.Brightness
{
    public class BrightnessModule : IDdpmModule
    {
        private UserControl? _leftView = null;
        private UserControl? _rightView = new BrightnessRightView();
        private BrightnessViewModel? vm;
        private DDPMSettings? _settings;

        //Robert_Lin 2024-5-30, remove argument from all Module's ctor
        //public BrightnessModule(HomeDevice? homeDevice)
        public BrightnessModule(IModuleOwner? moduleOwner = null)
        {
            if (vm == null)
                vm = new BrightnessViewModel(); //BrightnessViewModel.GetInstance(); //do not use getinstance here due to all monitors share the same view model

            if (vm != null && _rightView != null)
            {
                vm.ModuleOwner = DdpmCommonHelper.ModuleOwner;
                vm.SelectedHomeDevice = SelectedHomeDevice = vm.ModuleOwner.SelectedHomeDevice;
                vm.MyModule = this;
                _rightView.DataContext = vm;
                vm.Invoke_RefreshBrightnessPage();
                //vm.Invoke_RefreshHotkeySettings();
            }
        }

        public string ModuleName { get => "BrightnessModule"; }

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
            get => vm.ModuleOwner;
            set => vm.ModuleOwner = value;
        }

        #endregion ModuleOwner

        #region Event Handlers

        public void OnSelectedHomeDeviceChanged()
        {
            Trace.WriteLine("BrightnessModule.OnSelectedHomeDeviceChanged");
            if (vm != null && _rightView != null)
            {
                //vm.Invoke_RefreshBrightnessPage();
            }
        }

        public void OnActivated()
        {
            Trace.WriteLine("BrightnessModule.OnActivated");
            vm.UpdateHDRStatus();
            if (_rightView == null)
                return;
        }

        public void OnDeactivated()
        {
            Trace.WriteLine("BrightnessModule.OnDeactivated");
        }

        #endregion Event Handlers
    }
}