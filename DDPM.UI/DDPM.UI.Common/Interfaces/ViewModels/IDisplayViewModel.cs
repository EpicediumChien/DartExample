using DDPM.SA.Common;
using DDPM.SA.Common.Interfaces;
using DDPM.UI.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
