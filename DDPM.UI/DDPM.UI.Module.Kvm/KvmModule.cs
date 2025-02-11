using DDPM.UI.Common;
using DDPM.UI.Common.Interfaces;
using DDPM.UI.Common.Models;
using DDPM.UI.Interfaces;
using System.Windows.Controls;

namespace DDPM.UI.Module.Kvm
{
    public class KvmModule : IDdpmModule
    {
        public UserControl? _leftView; // = new KvmLeftView();
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
            vm._log.Info("[InitNewViewModel] running...");
            this.SelectedHomeDevice = DdpmCommonHelper.ModuleOwner.SelectedHomeDevice;
            isUSBKVM = DdpmCommonHelper.DeviceManagerSA.GetOnUSBKVM(DdpmCommonHelper.ModuleOwner.SelectedHomeDevice.MonitorInfo).Result;
            if (isUSBKVM)
            {
                _leftView = new KvmLeftView(vm);
                _leftView.DataContext = vm;
                DdpmCommonHelper.ModuleOwner.LoadLeftView();
            }
            else
            {
                _leftView = null;
            }
            vm.Invoke_RefreshData();
        }

        public void OnActivated()
        {
            vm._log.Info("[OnActivated] running....");
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