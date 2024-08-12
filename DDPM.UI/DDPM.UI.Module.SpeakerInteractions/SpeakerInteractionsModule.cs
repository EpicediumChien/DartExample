using System.Diagnostics;
using System.Windows.Controls;
using DDPM.UI.Common.Interfaces;
using DDPM.UI.Common.Models;
using DDPM.UI.Interfaces;
using DDPM.UI.Plugin.ViewModels;

namespace DDPM.UI.Module.SpeakerInteractions
{
    public class SpeakerInteractionsModule : IDdpmModule
    {
        private UserControl? _leftView = null;
        private UserControl _rightView;

        public SpeakerInteractionsModule(SoundBarViewModel vm)
        {
            _rightView = new SpeakerInteractionsRightView(vm);
        }

        public string ModuleName { get => "SpeakerInteractionsModule"; }

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
            Trace.WriteLine("SpeakerInteractionsModule.OnSelectedHomeDeviceChanged");
        }
        public void OnActivated()
        {
            Trace.WriteLine("SpeakerInteractionsModule.OnActivated");
        }
        public void OnDeactivated()
        {
            Trace.WriteLine("SpeakerInteractionsModule.OnDeactivated");
        }
        #endregion
    }
}
