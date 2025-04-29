using DDPM.UI.Common.Interfaces;
using DDPM.UI.Common.Models;
using DDPM.UI.Interfaces;
using DDPM.UI.Plugin.ViewModels;
using System.Diagnostics;
using System.Windows.Controls;

namespace DDPM.UI.Module.WebCameraSettings
{
    public class WebCameraSettingsModule : IDdpmModule
    {
        private UserControl? _leftView = null;
        private UserControl _rightView;

        private bool isSelectChanged = false;
        public bool IsModuleActive { get; set; } = false;

        public WebCameraSettingsModule(WebCameraViewModel vm)
        {
            _rightView = new WebCameraSettingsRightView(vm);
            _rightView.DataContext = vm;
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
            Trace.WriteLine("WebCameraSettingsModule.OnSelectedHomeDeviceChanged");
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
            Trace.WriteLine("WebCameraSettingsModule.OnActivated");
            if (isSelectChanged)
            {
                isSelectChanged = false;
                InitNewViewModel();
            }
        }

        public void OnDeactivated()
        {
            Trace.WriteLine("WebCameraSettingsModule.OnDeactivated");
        }

        #endregion Event Handlers
    }
}