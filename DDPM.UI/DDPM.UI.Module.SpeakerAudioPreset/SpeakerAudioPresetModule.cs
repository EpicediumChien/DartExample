using DDPM.UI.Common.Interfaces;
using DDPM.UI.Common.Models;
using DDPM.UI.Interfaces;
using DDPM.UI.Plugin.ViewModels;
using System.Diagnostics;
using System.Windows.Controls;

namespace DDPM.UI.Module.SpeakerAudioPreset
{
    public class SpeakerAudioPresetModule : IDdpmModule
    {
        private UserControl? _leftView = null;
        private UserControl _rightView;

        public SpeakerAudioPresetModule(SoundBarViewModel vm)
        {
            _rightView = new SpeakerAudioPresetRightView(vm);
        }

        public string ModuleName { get => "SpeakerAudioPresetModule"; }

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
            Trace.WriteLine("SpeakerAudioPresetModule.OnSelectedHomeDeviceChanged");
        }

        public void OnActivated()
        {
            Trace.WriteLine("SpeakerAudioPresetModule.OnActivated");
        }

        public void OnDeactivated()
        {
            Trace.WriteLine("SpeakerAudioPresetModule.OnDeactivated");
        }

        #endregion Event Handlers
    }
}