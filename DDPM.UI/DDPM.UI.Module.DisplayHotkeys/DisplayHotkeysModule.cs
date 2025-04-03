using DDPM.UI.Common;
using DDPM.UI.Common.Interfaces;
using DDPM.UI.Common.Models;
using DDPM.UI.Interfaces;
using System.Diagnostics;
using System.Windows.Controls;

namespace DDPM.UI.Module.DisplayHotkeys
{
    public class DisplayHotkeysModule : IDdpmModule
    {
        private UserControl? _leftView = null;
        private UserControl _rightView /*= new DisplayHotkeysRightView()*/;
        private DisplayHotkeysViewModel vm = new DisplayHotkeysViewModel();

        private bool isSelectChanged = false;
        public bool IsModuleActive { get; set; } = false;

        public DisplayHotkeysModule(IModuleOwner? moduleOwner = null)
        {
            //_rightView.DataContext = vm;
            SelectedHomeDevice = DdpmCommonHelper.ModuleOwner.SelectedHomeDevice;
            vm.DisplayHotkeysModule = this;
            _rightView = new DisplayHotkeysRightView(vm);
            vm.InitLog();

            //Robert_Lin 2025-2-10 comment out below statement to avid invoke twice in Init time.
            //If comment-out below statement will cause the ComboBox.Items empty, so remove the comment mark.
            vm.Invoke_RefreshData();
        }

        public string ModuleName { get => "DisplayHotkeysModule"; }

        public UserControl? GetLeftView()
        {
            return (UserControl?)_leftView;
        }

        public UserControl GetRightView()
        {
            return _rightView;
        }

        public IModuleOwner? ModuleOwner
        {
            get => DdpmCommonHelper.ModuleOwner;
            set { }
        }

        public HomeDevice SelectedHomeDevice
        {
            get
            {
                return ModuleOwner?.SelectedHomeDevice ?? null;
            }
            set { }
        }

        #region Event Handlers

        public void OnSelectedHomeDeviceChanged()
        {
            Trace.WriteLine("DisplayHotkeys.OnSelectedHomeDeviceChanged");
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
            Trace.WriteLine("DisplayHotkeys.OnActivated");
            if (isSelectChanged)
            {
                isSelectChanged = false;
                InitNewViewModel();
            }
            if (DdpmCommonHelper.isJumpFromUsbKvm)
            {
                DdpmCommonHelper.isJumpFromUsbKvm = false;
                InitNewViewModel();
            }
        }

        public void OnDeactivated()
        {
            Trace.WriteLine("DisplayHotkeys.OnDeactivated");
        }

        #endregion Event Handlers
    }
}