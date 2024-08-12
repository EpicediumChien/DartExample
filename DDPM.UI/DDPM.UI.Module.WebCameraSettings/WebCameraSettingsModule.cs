using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using DDPM.UI.Common.Interfaces;
using DDPM.UI.Common.Models;
using DDPM.UI.Interfaces;
using DDPM.UI.Plugin.ViewModels;
using Windows.Media.Capture;
using Windows.Media.Devices;

namespace DDPM.UI.Module.WebCameraSettings
{
    public class WebCameraSettingsModule : IDdpmModule
    {
        // 20240628 jim add
        private readonly WebCameraViewModel? _vm;

        private UserControl? _leftView = null;
        private UserControl _rightView;

        public WebCameraSettingsModule(WebCameraViewModel vm)
        {
            _rightView = new WebCameraSettingsRightView(vm);


            // 20240628 jim add
            _vm = vm;          

        }        

        public string ModuleName { get => "WebCameraSettingsModule"; }

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
        #endregion
    }
}
