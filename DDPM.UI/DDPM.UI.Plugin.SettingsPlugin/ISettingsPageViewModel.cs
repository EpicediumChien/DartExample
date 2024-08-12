using DDPM.UI.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDPM.UI.Plugin.SettingsPlugin
{
    internal interface ISettingsPageViewModel
    {
        public List<HomeDevice> HomeDevices { get; set; }
        public HomeDevice? SelectedHomeDevice { get; set; }
    }
}
