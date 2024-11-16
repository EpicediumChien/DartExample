using DDPM.UI.Common;
using DDPM.UI.Common.Interfaces;
using DDPM.UI.Common.Models;
using DDPM.UI.Interfaces;
using System.Diagnostics;
using System.Windows.Controls;

namespace DDPM.UI.Module.DisplayProperties
{
    public class DisplayPropertiesModule : IDdpmModule
    {
        private UserControl? _leftView = null;
        private UserControl _rightView = new DisplayPropertiesRightView();
        private DisplayPropertiesViewModel vm = new DisplayPropertiesViewModel();

        private bool isSelectChanged = false;
        public bool IsModuleActive { get; set; } = false;

        //Robert_Lin 20240530-remove argument on ctor
        //public DisplayPropertiesModule(HomeDevice? SelectedHomeDevice)
        public DisplayPropertiesModule(IModuleOwner? moduleOwner = null)
        {
            //Robert_Lin 20240530-remove argument on ctor
            //this.SelectedHomeDevice = SelectedHomeDevice;
            this.SelectedHomeDevice = DdpmCommonHelper.ModuleOwner.SelectedHomeDevice;

            _rightView.DataContext = vm;
            vm.MyModule = this;
            vm.Invoke_RefreshData();
        }

        public string ModuleName { get => Constants.ModuleName_DisplayProperties; } //"DisplayPropertiesModule"

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
            Trace.WriteLine("DisplayPropertiesModule.OnSelectedHomeDeviceChanged");
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
            Trace.WriteLine("DisplayPropertiesModule.OnActivated");
            if (isSelectChanged)
            {
                isSelectChanged = false;
                InitNewViewModel();
            }
        }

        public void OnDeactivated()
        {
        }

        #endregion Event Handlers
    }
}