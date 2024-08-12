using System.Diagnostics;
using System.Windows.Controls;
using DDPM.UI.Common.Interfaces;
using DDPM.UI.Common.Models;
using DDPM.UI.Interfaces;
using DDPM.UI.Plugin.ViewModels;

namespace DDPM.UI.Module.HeadsetAudioSettings
{
    public class HeadsetAudioSettingsModule : IDdpmModule
    {
        private UserControl? _leftView = null;
        private UserControl _rightView;

        public HeadsetAudioSettingsModule(HeadsetViewModel vm)
        {
            _rightView = new HeadsetAudioSettingsRightView(vm);
        }

        public string ModuleName { get => "HeadsetAudioSettingsModule"; }

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
            Trace.WriteLine("HeadsetAudioSettingsModule.OnSelectedHomeDeviceChanged");
        }
        public void OnActivated()
        {
            Trace.WriteLine("HeadsetAudioSettingsModule.OnActivated");
        }
        public void OnDeactivated()
        {
            Trace.WriteLine("HeadsetAudioSettingsModule.OnDeactivated");
        }
        #endregion
    }
}
