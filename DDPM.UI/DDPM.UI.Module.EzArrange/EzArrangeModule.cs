
using DDPM.UI.Common.Interfaces;
using DDPM.UI.Common.Interfaces.ViewModels;
using DDPM.UI.Common.Models;
using DDPM.UI.Interfaces;
using System.Windows.Controls;
using DDPM.UI.Plugin.Common.ViewModels;
using DDPM.SA.Common;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.UX.WPF;

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

        public EzArrangeModule(IModuleOwner? moduleOwner = null)
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
                        _selHomeDevice.vmEzArrange = new Common.ViewModels.EzArrangeViewModel(_selHomeDevice);
                    }
                }

                _selHomeDevice.vmEzArrange.CreateLog(_vmDisplay.Console, "EAMod");
            }
            //_rightView.DataContext = vm;
        }
        public string ModuleName { get => "EzArrangeModule"; }

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
        #region ModuleOwner
        public IModuleOwner? ModuleOwner { get; set; }
        /*
        {
            get => vm.ModuleOwner;
            set => vm.ModuleOwner = value;
        }*/
        #endregion

        #region Event Handlers
        public void OnSelectedHomeDeviceChanged()
        {

        }
        public void OnActivated()
        {

        }
        public void OnDeactivated()
        {

        }
        #endregion
    }

}
