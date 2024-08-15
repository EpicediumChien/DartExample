using DDPM.UI.Common.Interfaces;
using DDPM.UI.Common.Models;
using DDPM.UI.Interfaces;
using DDPM.UI.Plugin.ViewModels;
using System.Diagnostics;
using System.Windows.Controls;

namespace DDPM.UI.Module.SpeakerAudioSettings
{
    public class SpeakerAudioSettingsModule : IDdpmModule
    {
        private UserControl? _leftView = null;
        private UserControl _rightView;

        public SpeakerAudioSettingsModule(SoundBarViewModel vm)
        {
            _rightView = new SpeakerAudioSettingsRightView(vm);
        }

        public string ModuleName { get => "SpeakerAudioSettingsModule"; }

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
            Trace.WriteLine("SpeakerAudioSettingsModule.OnSelectedHomeDeviceChanged");
        }

        public void OnActivated()
        {
            Trace.WriteLine("SpeakerAudioSettingsModule.OnActivated");
        }

        public void OnDeactivated()
        {
            Trace.WriteLine("SpeakerAudioSettingsModule.OnDeactivated");
        }

        #endregion Event Handlers
    }
}