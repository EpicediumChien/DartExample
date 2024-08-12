
using DDPM.UI.Common;
using DDPM.UI.Common.Interfaces;
using DDPM.UI.Common.Models;
using DDPM.UI.Interfaces;
using System.Diagnostics;
using System.Windows.Controls;

namespace DDPM.UI.Module.InputSource
{
    public class InputSourceModule : IDdpmModule
    {
        private UserControl? _leftView = null;
        private UserControl _rightView/* = new InputSourceRightView()*/;
        private InputSourceViewModel vm = new InputSourceViewModel();

        //Robert_Lin 20240530-remove argument on ctor
        //public InputSourceModule(HomeDevice? SelectedHomeDevice)
       public InputSourceModule(IModuleOwner? moduleOwner = null)
        {
            //Robert_Lin 20240530-remove argument on ctor
            //this.SelectedHomeDevice = SelectedHomeDevice;
            vm.ModuleOwner = DdpmCommonHelper.ModuleOwner;
            this.SelectedHomeDevice = DdpmCommonHelper.ModuleOwner.SelectedHomeDevice;
            vm.InputSourceModule = this;
            _rightView = new InputSourceRightView(vm);
            vm.Invoke_RefreshData();
        }
        public string ModuleName { get => "InputSourceModule"; }

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
        #endregion

        #region Event Handlers
        public void OnSelectedHomeDeviceChanged()
        {
            Trace.WriteLine("InputSourceModule.OnSelectedHomeDeviceChanged");
        }
        public void OnActivated()
        {
            Trace.WriteLine("InputSourceModule.OnActivated");
        }
        public void OnDeactivated()
        {
            Trace.WriteLine("InputSourceModule.OnDeactivated");
        }
        #endregion
    }

}
