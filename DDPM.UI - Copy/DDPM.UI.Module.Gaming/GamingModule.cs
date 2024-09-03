using DDPM.UI.Common;
using DDPM.UI.Common.Interfaces;
using DDPM.UI.Common.Models;
using DDPM.UI.Interfaces;
using System.Windows.Controls;

namespace DDPM.UI.Module.Gaming
{
    public class GamingModule : IDdpmModule
    {
        private UserControl? _leftView = null;
        private UserControl _rightView = new GamingRightView();
        private GamingViewModel vm = new GamingViewModel();

        private bool isSelectChanged = false;
        public bool IsModuleActive { get; set; } = false;

        //Robert_Lin 20240530-remove argument on ctor
        //public GamingModule(HomeDevice? SelectedHomeDevice)
        public GamingModule(IModuleOwner? moduleOwner = null)
        {
            //Robert_Lin 20240530-remove argument on ctor
            //this.SelectedHomeDevice = SelectedHomeDevice;
            this.SelectedHomeDevice = DdpmCommonHelper.ModuleOwner.SelectedHomeDevice;

            _rightView.DataContext = vm;
            vm.MyModule = this;
            vm.Invoke_RefreshData();
        }

        public string ModuleName { get => "GamingModule"; }

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
            vm.Invoke_RefreshData();
        }

        public void OnActivated()
        {
            DdpmCommonHelper.DeviceManagerSA.GamingChangeEvent += vm.GamingParamChang;
            if (isSelectChanged)
            {
                isSelectChanged = false;
                //InitNewViewModel();
            }
        }

        public void OnDeactivated()
        {
            DdpmCommonHelper.DeviceManagerSA.GamingChangeEvent -= vm.GamingParamChang;
        }

        #endregion Event Handlers
    }
}