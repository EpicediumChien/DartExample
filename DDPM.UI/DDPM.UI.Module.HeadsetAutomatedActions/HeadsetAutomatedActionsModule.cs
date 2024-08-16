using DDPM.UI.Common.Interfaces;
using DDPM.UI.Common.Models;
using DDPM.UI.Interfaces;
using DDPM.UI.Plugin.ViewModels;
using System.Diagnostics;
using System.Windows.Controls;

namespace DDPM.UI.Module.HeadsetAutomatedActions
{
    public class HeadsetAutomatedActionsModule : IDdpmModule
    {
        private UserControl? _leftView = null;
        private UserControl _rightView;

        public HeadsetAutomatedActionsModule(HeadsetViewModel vm)
        {
            _rightView = new HeadsetAutomatedActionsRightView(vm);
            //vm.DetectPageShow(vm.Model);
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
            Trace.WriteLine("HeadsetAutomatedActionsModule.OnSelectedHomeDeviceChanged");
        }

        public void OnActivated()
        {
            Trace.WriteLine("HeadsetAutomatedActionsModule.OnActivated");
        }

        public void OnDeactivated()
        {
            Trace.WriteLine("HeadsetAutomatedActionsModule.OnDeactivated");
        }

        #endregion Event Handlers
    }
}