using DDPM.UI.Common;
using DDPM.UI.Common.Interfaces;
using DDPM.UI.Common.Models;
using DDPM.UI.Interfaces;
using System.Diagnostics;
using System.Windows.Controls;

namespace DDPM.UI.Module.DisplayOthers
{
    public class DisplayOthersModule : IDdpmModule
    {
        private UserControl? _leftView = null;
        private UserControl _rightView/* = new DisplayOthersRightView()*/;
        private DisplayOthersViewModel vm = new DisplayOthersViewModel();

        public DisplayOthersModule(IModuleOwner moduleOwner = null)
        {
            this.SelectedHomeDevice = DdpmCommonHelper.ModuleOwner.SelectedHomeDevice;

            vm.DisplayOthersModule = this;
            _rightView = new DisplayOthersRightView(vm);
            vm.Invoke_RefreshData();
        }

        public string ModuleName { get => "DisplayOthersModule"; }

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
            Trace.WriteLine("DisplayOthersModule.OnSelectedHomeDeviceChanged");
        }

        public void OnActivated()
        {
            Trace.WriteLine("DisplayOthersModule.OnActivated");
        }

        public void OnDeactivated()
        {
            Trace.WriteLine("DisplayOthersModule.OnDeactivated");
        }

        #endregion Event Handlers
    }
}