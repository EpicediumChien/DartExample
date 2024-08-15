using DDPM.UI.Common.Interfaces;
using DDPM.UI.Common.Models;
using DDPM.UI.Interfaces;
using DDPM.UI.Plugin.ViewModels;
using System.Diagnostics;
using System.Windows.Controls;

namespace DDPM.UI.Module.HeadsetDeviceSettings
{
    public class HeadsetDeviceSettingsModule : IDdpmModule
    {
        private UserControl? _leftView = null;
        private UserControl _rightView;

        public HeadsetDeviceSettingsModule(HeadsetViewModel vm)
        {
            _rightView = new HeadsetDeviceSettingsRightView(vm);
            //vm.DetectPageShow(vm.Model);
        }

        public string ModuleName { get => "HeadsetDeviceSettingsModule"; }

        public UserControl? GetLeftView()
        {
            return _leftView;
        }

        public UserControl GetRightView()
        {
            return _rightView;
        }

        public HomeDevice SelectedHomeDevice { get; set; }
        public IModuleOwner? ModuleOwner { get; set; }

        #region Event Handlers

        public void OnSelectedHomeDeviceChanged()
        {
            Trace.WriteLine("HeadsetDeviceSettingsModule.OnSelectedHomeDeviceChanged");
        }

        public void OnActivated()
        {
            Trace.WriteLine("HeadsetDeviceSettingsModule.OnActivated");
        }

        public void OnDeactivated()
        {
            Trace.WriteLine("HeadsetDeviceSettingsModule.OnDeactivated");
        }

        #endregion Event Handlers
    }
}