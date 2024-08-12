using DDPM.UI.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDPM.UI.Plugin.DockPlugin.Interfaces
{
    internal interface IDockPageViewModel
    {
        public List<HomeDevice> HomeDevices { get; set; }
        public HomeDevice? SelectedHomeDevice { get; set; }
    }
}
