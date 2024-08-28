using DDPM.UI.Common;
using DDPM.UI.Common.Interfaces;
using DDPM.UI.Common.Models;
using DDPM.UI.Interfaces;
using Dell.Client.Framework.Common;
using System.Diagnostics;
using System.Windows.Controls;

namespace DDPM.UI.Module.PipPbp
{
    public class PipPbpModule : IDdpmModule
    {
        private UserControl? _leftView;
        private UserControl _rightView;
        private PipPbpViewModel vm = new PipPbpViewModel();

        private bool isSelectChanged = false;
        public bool IsModuleActive { get; set; } = false;

        public PipPbpModule(IModuleOwner? moduleOwner = null)
        {
            if (DdpmCommonHelper.MyConsole != null)
            {
                vm.Log = (ILog?)DdpmCommonHelper.MyConsole.CreateLog("PXP");
            }
            _leftView = null;// new PipPbpLeftView();
            _rightView = new PipPbpRightView(vm);
        }

        public string ModuleName { get => "PipPbpModule"; }

        public UserControl? GetLeftView()
        {
            return (UserControl?)_leftView;
        }

        public UserControl GetRightView()
        {
            return _rightView;
        }

        public HomeDevice? SelectedHomeDevice { get; set; }

        private void PrepareInputSourceList()
        {
            //vm.InputSourceList.Clear();
            //vm.InputSourceList.Add(new InputSourceObj() { DisplayName = "HDMI 1" });
            //vm.InputSourceList.Add(new InputSourceObj() { DisplayName = "HDMI 2" });
            //vm.InputSourceList.Add(new InputSourceObj() { DisplayName = "HDMI 3" });
            //vm.InputSourceList.Add(new InputSourceObj() { DisplayName = "HDMI 4" });
            //vm.InputSourceList.Add(new InputSourceObj() { DisplayName = "USB C" });
            //vm.InputSourceList.Add(new InputSourceObj() { DisplayName = "DP" });
        }

        #region ModuleOwner

        public IModuleOwner? ModuleOwner
        {
            get => vm.ModuleOwner;
            set
            {
                vm.ModuleOwner = value;
            }
        }

        #endregion ModuleOwner

        #region Event Handlers

        public void OnSelectedHomeDeviceChanged()
        {
            Trace.WriteLine("PipPbpModule.OnSelectedHomeDeviceChanged");
       
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
            //Update SelectedHomeDevices
            if (DdpmCommonHelper.ModuleOwner != null)
            {
                vm.SelectedHomeDevice = DdpmCommonHelper.ModuleOwner.SelectedHomeDevice;
                vm.RefreshData();
            }
        }

        public void OnActivated()
        {
            Trace.WriteLine("PipPbpModule.OnActivated");
            vm.OnActivated();

            if (isSelectChanged)
            {
                isSelectChanged = false;
                InitNewViewModel();
            }
        }

        public void OnDeactivated()
        {
            Trace.WriteLine("PipPbpModule.OnDeactivated");
        }

        #endregion Event Handlers
    }
}