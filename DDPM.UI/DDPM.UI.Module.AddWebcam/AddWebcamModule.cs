using DDPM.UI.Common.Interfaces;
using DDPM.UI.Common.Models;
using DDPM.UI.Interfaces;
using DDPM.UI.Plugin.ViewModels;
using System.Diagnostics;
using System.Windows.Controls;

namespace DDPM.UI.Module.AddWebcam
{
    public class AddWebcamModule : IDdpmModule
    {
        private UserControl? _leftView = null;
        private UserControl _rightView;

        public AddWebcamModule(AddDeviceViewModel vm)
        {
            _rightView = new AddWebcamRightView(vm);
        }

        public string ModuleName { get => "AddWebcamModule"; }

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
            Trace.WriteLine("BrightnessModule.OnSelectedHomeDeviceChanged");
        }

        public void OnActivated()
        {
            Trace.WriteLine("BrightnessModule.OnActivated");
        }

        public void OnDeactivated()
        {
            Trace.WriteLine("BrightnessModule.OnDeactivated");
        }

        #endregion Event Handlers
    }
}