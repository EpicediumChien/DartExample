using DDPM.SA.Common;
using DDPM.UI.Common;
using DDPM.UI.Common.Interfaces;
using DDPM.UI.Common.Interfaces.ViewModels;
using DDPM.UI.Common.Models;
using DDPM.UI.Interfaces;
using DDPM.UI.Plugin.Common.ViewModels;
using System.Windows.Controls;
using UserControl = System.Windows.Controls.UserControl;

namespace DDPM.UI.Module.EzArrange
{
    public class EzArrangeModule : IDdpmModule
    {
        private UserControl? _leftView = null;
        private UserControl _rightView;// = new EzArrangeRightVierw();

        //private EzArrangeViewModel vm = new EzArrangeViewModel();
        private readonly DisplayViewModel _vmDisplay;

        private readonly IDeviceManagerSA _deviceManagerSA;
        private HomeDevice _selHomeDevice;

        private bool isSelectChanged = false;
        public bool IsModuleActive { get; set; } = false;

        public EzArrangeModule(IModuleOwner? moduleOwner = null)
        {
            if (moduleOwner != null) //DisplayViewModel
            {
                ModuleOwner = moduleOwner;
                _vmDisplay = moduleOwner as DisplayViewModel;
                _deviceManagerSA = _vmDisplay.DeviceManagerSA;
                _selHomeDevice = _vmDisplay.SelectedHomeDevice;
                if (_selHomeDevice != null)
                {
                    if (_selHomeDevice.vmEzArrange == null)
                    {
                        _selHomeDevice.vmEzArrange = new Common.ViewModels.EzArrangeViewModel(_selHomeDevice);
                    }
                }

                _selHomeDevice.vmEzArrange.CreateLog(_vmDisplay.Console, "EAMod");
            }

            if (_deviceManagerSA != null)
            {
                _deviceManagerSA.EASettingsChanged += _deviceManagerSA_EASettingsChanged;
            }
        }


        public string ModuleName { get => Constants.ModuleName_EzArrange; } //"EzArrangeModule"

        public UserControl? GetLeftView()
        {
            return (UserControl?)_leftView;
        }

        public UserControl GetRightView()
        {
            if (_rightView == null)
            {
                _rightView = new EzArrangeRightVierw(_vmDisplay);
            }
            return _rightView;
        }

        public HomeDevice? SelectedHomeDevice { get; set; }

        public List<HomeDevice>? HomeDevices
        {
            get
            {
                if (ModuleOwner != null)
                {
                    return ModuleOwner.HomeDevices;
                }
                else
                {
                    if (DdpmCommonHelper.ModuleOwner != null)
                        return DdpmCommonHelper.ModuleOwner.HomeDevices;
                }
                return null;
            }
        }

        #region ModuleOwner

        public IModuleOwner? ModuleOwner { get; set; }
        /*
        {
            get => vm.ModuleOwner;
            set => vm.ModuleOwner = value;
        }*/

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
                _selHomeDevice = ModuleOwner.SelectedHomeDevice;
                if (_selHomeDevice != null)
                {
                    if (_selHomeDevice.vmEzArrange == null)
                    {
                        _selHomeDevice.vmEzArrange = new Common.ViewModels.EzArrangeViewModel(_selHomeDevice);
                    }
                }
            }
            if (_rightView != null)
            {
                EzArrangeRightVierw ezRightView = _rightView as EzArrangeRightVierw;
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

        //To be called (from Subagent IDeviceManagerSA, when EAMonitorSettings is changed from Subagent side.
        private void _deviceManagerSA_EASettingsChanged(object? sender, EAArgs e)
        {
            //The quick and easy way: use the SelectedDeviceChange event
            OnSelectedHomeDeviceChanged();

            //If the OnSelectedHomeDeviceChanged() take too long time, then
            //we need to consider below method:

            //EAArgs spec.
            //Check DDPM.SA.Plugins.User.EasyArrange project, EAPlugin.cs
            // Method: STA_SetEASelectedLayout( )
            //
            // Message: "{MonitorModel}|{MonitorServiceTag}"
            //string[] tokens = e.Message.Split('|', StringSplitOptions.RemoveEmptyEntries);
            //string model = "", serviceTag = "";
            //if (tokens.Length >= 2)
            //{
            //    model = tokens[0];
            //    serviceTag = tokens[1];
            //}
            //if (HomeDevices != null)
            //{
            //    foreach (HomeDevice homeDevice in HomeDevices)
            //    {
            //        if (homeDevice.DeviceCategory != eDeviceCategory.Display)
            //            continue;
            //        if (homeDevice.MonitorInfo == null)
            //            continue;

            //        if ((homeDevice.MonitorInfo.modelName.Equals(model, StringComparison.OrdinalIgnoreCase)) &&
            //            (homeDevice.MonitorInfo.edid.ServiceTag.Equals(serviceTag, StringComparison.OrdinalIgnoreCase)))
            //        {
            //            if (homeDevice.vmEzArrange != null)
            //            {
            //                //TO DO: refresh EA Settings
            //            }
            //        }
            //    }
            //}

        }
        #endregion Event Handlers
    }
}