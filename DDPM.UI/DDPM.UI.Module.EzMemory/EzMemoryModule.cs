using DDPM.SA.Common;
using DDPM.UI.Common;
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
        private DDPM.UI.Common.ViewModels.EzArrangeViewModel _vm;
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
                if (_selHomeDevice != null &&
                    _selHomeDevice.vmEzArrange == null)
                {
                    _selHomeDevice.vmEzArrange = new EzArrangeViewModel(_selHomeDevice);
                }

                //Robert_Lin 2025-1-13 Fix bug: Create log for EzMemoryModule
                //And all _log.Info() has been changed to LogInfo() in the EzArrangeViewModel
                if (_selHomeDevice != null)
                {
                    _selHomeDevice.vmEzArrange.CreateLog(_vmDisplay.Console, "EMMod");
                }
            }
            //_rightView.DataContext = vm;
        }

        public string ModuleName { get => Constants.ModuleName_EzMemory; } //"EzMemoryModule"

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
            //Update SelectedHomeDevice
            if (DdpmCommonHelper.ModuleOwner != null)
            {
                ModuleOwner = DdpmCommonHelper.ModuleOwner;
                int selProfileId = -1;
                if (_selHomeDevice != null)
                {
                    if (_selHomeDevice.vmEzArrange != null)
                        selProfileId = _selHomeDevice.vmEzArrange.SelectedProfileId;
                }
                
                _selHomeDevice = ModuleOwner.SelectedHomeDevice;
                if (_selHomeDevice != null &&
                    _selHomeDevice.vmEzArrange == null)
                {
                    _selHomeDevice.vmEzArrange = new Common.ViewModels.EzArrangeViewModel(_selHomeDevice);
                }
                _selHomeDevice.vmEzArrange.SelectedProfileId = selProfileId;
            }
            if (_rightView != null)
            {
                EzMemoryRightView ezRightView = _rightView as EzMemoryRightView;
                ezRightView.HandleSelectedHomeDeviceChanged();
            }

        }

        public void OnActivated()
        {
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