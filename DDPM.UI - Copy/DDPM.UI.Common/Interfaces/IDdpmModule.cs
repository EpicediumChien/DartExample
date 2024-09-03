using DDPM.UI.Common.Interfaces;
using DDPM.UI.Common.Models;
using UserControl = System.Windows.Controls.UserControl;

namespace DDPM.UI.Interfaces
{
    public interface IDdpmModule
    {
        public UserControl? GetLeftView();

        public UserControl GetRightView();

        public string ModuleName { get; }

        /// <summary>
        /// Pass current user selected device to DDPM Module
        /// For Monitor devices, it can use HomeDevice.MonitorInfo,
        /// For Peripherals, it can use HomeDevuce.DeviceInfo (not been implemented)
        /// </summary>
        public HomeDevice SelectedHomeDevice { get; set; }

        //Robert_Lin, 2024-5-23, suggest to get ModuleOwner from DdpmCommonHelper
        public IModuleOwner ModuleOwner { get; set; }

        //Robert_Lin, 2024-5-30 added to process SelectedHomeDeviceChanged event
        public void OnSelectedHomeDeviceChanged();

        //Robert_Lin, 2024-5-30 added to process When the module is activated and deactivated
        public void OnActivated();

        public void OnDeactivated();

        //Robert_Lin, 2024-5-30
        // public IDdpmModule

        //0827 for active check to perform UI refresh via display selection change
        public bool IsModuleActive { get; set; }
    }
}