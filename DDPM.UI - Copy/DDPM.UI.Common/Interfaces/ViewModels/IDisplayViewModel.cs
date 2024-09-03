using DDPM.SA.Common;
using DDPM.SA.Common.Interfaces;
using DDPM.UI.Common.Models;

namespace DDPM.UI.Common.Interfaces.ViewModels
{
    public interface IDisplayViewModel
    {
        public List<HomeDevice> HomeDevices { get; set; }
        public HomeDevice SelectedHomeDevice { get; set; }
        public IDeviceManagerSA DeviceManagerSA { get; }
        public IEasyArrangeService EasyArrangeService { get; }
    }
}