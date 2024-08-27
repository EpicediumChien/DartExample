using DDPM.UI.Common.Interfaces;
using DDPM.UI.Common.Models;
using DDPM.UI.Interfaces;
using DDPM.UI.Plugin.ViewModels;
using System.Diagnostics;
using System.Windows.Controls;

namespace DDPM.UI.Module.AddKnM_Wired
{
    public class AddKnM_WiredModule : IDdpmModule
    {
        private readonly UserControl? _leftView = null;
        private readonly UserControl? _rightView;

        private bool isSelectChanged = false;
        public bool IsModuleActive { get; set; } = false;

        public AddKnM_WiredModule()
        { }

        public AddKnM_WiredModule(AddDeviceViewModel vm)
        {
            _rightView = new AddKnM_WiredRightView(vm);
        }

        public string ModuleName { get => "AddKnM_WiredModule"; }

        public UserControl? GetLeftView()
        {
            return _leftView;
        }

        public UserControl GetRightView()
        {
            return _rightView!;
        }

        public HomeDevice SelectedHomeDevice { get; set; } = new HomeDevice();
        public IModuleOwner ModuleOwner { get; set; }

        #region Event Handlers

        public void OnSelectedHomeDeviceChanged()
        {
            Trace.WriteLine("BrightnessModule.OnSelectedHomeDeviceChanged");
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
            Trace.WriteLine("BrightnessModule.OnActivated");
            if (isSelectChanged)
            {
                isSelectChanged = false;
                InitNewViewModel();
            }
        }

        public void OnDeactivated()
        {
            Trace.WriteLine("BrightnessModule.OnDeactivated");
        }

        #endregion Event Handlers
    }
}