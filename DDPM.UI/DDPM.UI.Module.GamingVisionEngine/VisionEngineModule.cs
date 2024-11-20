using DDPM.UI.Common;
using DDPM.UI.Common.Interfaces;
using DDPM.UI.Common.Models;
using DDPM.UI.Interfaces;
using System.Diagnostics;
using System.Windows.Controls;

namespace DDPM.UI.Module.GamingVisionEngine
{
    public class VisionEngineModule : IDdpmModule
    {
        private UserControl? _leftView = null;
        private UserControl _rightView = new VisionEngineRightView();
        private VisionEngineViewModel vm = new VisionEngineViewModel();

        private bool isSelectChanged = false;
        public bool IsModuleActive { get; set; } = false;

        public VisionEngineModule(IModuleOwner? moduleOwner = null)
        {
            this.SelectedHomeDevice = DdpmCommonHelper.ModuleOwner.SelectedHomeDevice;

            _rightView.DataContext = vm;
            vm.MyModule = this;
            vm.Invoke_RefreshData();
        }

        public string ModuleName { get => Constants.ModuleName_VisionEngine; } //"VisionEngineModule"

        public UserControl? GetLeftView()
        {
            return (UserControl?)_leftView;
        }

        public UserControl GetRightView()
        {
            return _rightView;
        }

        public HomeDevice? SelectedHomeDevice { get; set; }

        #region ModuleOwner

        public IModuleOwner? ModuleOwner
        {
            get => vm.ModuleOwner;
            set => vm.ModuleOwner = value;
        }

        #endregion ModuleOwner

        #region Event Handlers

        public void OnSelectedHomeDeviceChanged()
        {
            vm.Invoke_RefreshData();

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
            DdpmCommonHelper.DeviceManagerSA.GamingChangeEvent += vm.GamingParamChang;
            Trace.WriteLine("VisionEngineModule.OnActivated");
            if (isSelectChanged)
            {
                isSelectChanged = false;
                InitNewViewModel();
            }
        }

        public void OnDeactivated()
        {
            DdpmCommonHelper.DeviceManagerSA.GamingChangeEvent -= vm.GamingParamChang;
        }

        #endregion Event Handlers
    }
}