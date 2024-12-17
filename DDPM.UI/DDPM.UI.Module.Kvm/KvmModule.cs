using DDPM.UI.Common;
using DDPM.UI.Common.Interfaces;
using DDPM.UI.Common.Models;
using DDPM.UI.Interfaces;
using System.Windows.Controls;

namespace DDPM.UI.Module.Kvm
{
    public class KvmModule : IDdpmModule
    {
        private UserControl? _leftView; // = new KvmLeftView();
        private UserControl _rightView;
        private KvmViewModel vm = new KvmViewModel();

        private bool isSelectChanged = false;
        public bool IsModuleActive { get; set; } = false;

        public bool isUSBKVM { get; set; } = false;

        public KvmModule(IModuleOwner? moduleOwner)
        {
            this.SelectedHomeDevice = DdpmCommonHelper.ModuleOwner.SelectedHomeDevice;
            vm.KvmModule = this;
            _rightView = new KvmRightView(vm);
            _rightView.DataContext = vm;
            isUSBKVM = DdpmCommonHelper.DeviceManagerSA.GetOnUSBKVM(DdpmCommonHelper.ModuleOwner.SelectedHomeDevice.MonitorInfo).Result;
            if (isUSBKVM)
            {
                _leftView = new KvmLeftView(vm);
                _leftView.DataContext = vm;
            }
            else
            {
                _leftView = null;
            }
            //vm.Invoke_RefreshData();
            //Jason 12/11 add loadleftview
            //moduleOwner.LoadLeftView();
        }

        public string ModuleName { get => Constants.ModuleName_KVM; } //"KvmModule"

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
            vm._log.Debug("[InitNewViewModel] running...");
            vm.ModuleOwner = DdpmCommonHelper.ModuleOwner;
            this.SelectedHomeDevice = DdpmCommonHelper.ModuleOwner.SelectedHomeDevice;
            vm.Invoke_RefreshData();
        }

        public void OnActivated()
        {
            vm._log.Debug("[OnActivated] running....");
            if (isSelectChanged)
            {
                isSelectChanged = false;
                InitNewViewModel();
            }
            else
            {
                vm.Invoke_RefreshData();
            }
        }

        public void OnDeactivated()
        {
        }

        #endregion Event Handlers
    }
}