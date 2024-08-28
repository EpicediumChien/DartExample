using DDPM.UI.Common;
using DDPM.UI.Common.Interfaces;
using DDPM.UI.Common.Models;
using DDPM.UI.Interfaces;
using System.Diagnostics;
using System.Windows.Controls;

namespace DDPM.UI.Module.Color
{
    public class ColorModule : IDdpmModule
    {
        private UserControl? _leftView = null;
        private UserControl _rightView = new ColorRightView();
        private ColorViewModel vm;

        //Robert_Lin 2024-5-30, remove argument from ctor
        //public ColorModule(HomeDevice? SelectedHomeDevice)
        public ColorModule(IModuleOwner? moduleOwner = null)
        {
            //_rightView.DataContext = vm;
            this.SelectedHomeDevice = DdpmCommonHelper.ModuleOwner.SelectedHomeDevice;
            vm = new ColorViewModel();
            _rightView.DataContext = vm;
            vm.MyModule = this;

            vm.Invoke_RefreshData();
        }

        public string ModuleName { get => "ColorModule"; }

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
            Trace.WriteLine("ColorModule.OnSelectedHomeDeviceChanged");
            vm.Invoke_RefreshData();
        }

        public void OnActivated()
        {
            Trace.WriteLine("ColorModule.OnActivated");
        }

        public void OnDeactivated()
        {
            Trace.WriteLine("ColorModule.OnDeactivated");
        }

        #endregion Event Handlers
    }
}