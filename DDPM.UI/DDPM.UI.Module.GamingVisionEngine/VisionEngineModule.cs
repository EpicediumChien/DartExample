using DDPM.UI.Common;
using DDPM.UI.Common.Interfaces;
using DDPM.UI.Common.Models;
using DDPM.UI.Interfaces;
using System.Windows.Controls;

namespace DDPM.UI.Module.GamingVisionEngine
{
    public class VisionEngineModule : IDdpmModule
    {
        private UserControl? _leftView = null;
        private UserControl _rightView = new VisionEngineRightView();
        private VisionEngineViewModel vm = new VisionEngineViewModel();

        public VisionEngineModule(IModuleOwner? moduleOwner = null)
        {
            this.SelectedHomeDevice = DdpmCommonHelper.ModuleOwner.SelectedHomeDevice;

            _rightView.DataContext = vm;
            vm.MyModule = this;
            vm.Invoke_RefreshData();
        }

        public string ModuleName { get => "VisionEngineModule"; }

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
        }

        public void OnActivated()
        {
        }

        public void OnDeactivated()
        {
        }

        #endregion Event Handlers
    }
}