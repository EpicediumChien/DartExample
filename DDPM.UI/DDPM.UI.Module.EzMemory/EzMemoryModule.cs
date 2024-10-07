using DDPM.SA.Common;
using DDPM.UI.Common.Interfaces;
using DDPM.UI.Common.Models;
using DDPM.UI.Common.ViewModels;
using DDPM.UI.Interfaces;
using DDPM.UI.Plugin.Common.ViewModels;
using System.Diagnostics;
using System.Windows.Controls;

namespace DDPM.UI.Module.EzMemory
{
    public class EzMemoryModule : IDdpmModule
    {
        private UserControl? _leftView = null;
        private UserControl _rightView;// = new EzArrangeRightVierw();

        //private EzArrangeViewModel vm = new EzArrangeViewModel();
        private readonly DisplayViewModel _vmDisplay;

        private readonly IDeviceManagerSA _deviceManagerSA;
        private HomeDevice _selHomeDevice;

        private bool isSelectChanged = false;
        public bool IsModuleActive { get; set; } = false;

        public EzMemoryModule(IModuleOwner moduleOwner = null)
        {
            if (moduleOwner != null)
            {
                ModuleOwner = moduleOwner;
                _vmDisplay = moduleOwner as DisplayViewModel;
                _deviceManagerSA = _vmDisplay.DeviceManagerSA;
                _selHomeDevice = _vmDisplay.SelectedHomeDevice;
                if (_selHomeDevice != null)
                {
                    if (_selHomeDevice.vmEzArrange == null)
                    {
                        _selHomeDevice.vmEzArrange = new EzArrangeViewModel(_selHomeDevice);
                    }
                }

                //_selHomeDevice.vmEzArrange.CreateLog(_vmDisplay.Console, "EAMod");
            }
            //_rightView.DataContext = vm;
        }

        public string ModuleName { get => "EzMemoryModule"; }

        public UserControl? GetLeftView()
        {
            return (UserControl?)_leftView;
        }

        public UserControl GetRightView()
        {
            if (_rightView == null)
            {
                _rightView = new EzMemoryRightView(_vmDisplay);
            }
            return _rightView;
        }

        public HomeDevice? SelectedHomeDevice { get; set; }

        #region ModuleOwner

        public IModuleOwner? ModuleOwner { get; set; }
        //{
        //    get => vm.ModuleOwner;
        //    set => vm.ModuleOwner = value;
        //}

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
        }

        public void OnActivated()
        {
            Trace.WriteLine("EzMemoryModule.OnActivated");
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