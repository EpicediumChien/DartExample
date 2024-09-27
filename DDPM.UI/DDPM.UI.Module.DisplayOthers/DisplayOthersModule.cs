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
        private UserControl _rightView = new DisplayOthersRightView();
        private DisplayOthersViewModel vm = new DisplayOthersViewModel();

        private bool isSelectChanged = false;
        public bool IsModuleActive { get; set; } = false;

        public DisplayOthersModule(IModuleOwner moduleOwner = null)
        {
            this.SelectedHomeDevice = DdpmCommonHelper.ModuleOwner.SelectedHomeDevice;

            _rightView.DataContext = vm;
            vm.DisplayOthersModule = this;
            //_rightView = new DisplayOthersRightView(vm);
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
            this.SelectedHomeDevice = DdpmCommonHelper.ModuleOwner.SelectedHomeDevice;
            vm.Invoke_RefreshData();
        }

        public void OnActivated()
        {
            Trace.WriteLine("DisplayOthersModule.OnActivated");
            if (isSelectChanged)
            {
                isSelectChanged = false;
                InitNewViewModel();
            }
        }

        public void OnDeactivated()
        {
            Trace.WriteLine("DisplayOthersModule.OnDeactivated");
        }

        #endregion Event Handlers
    }
}