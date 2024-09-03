using DDPM.UI.Common.Models;

namespace DDPM.UI.Plugin.DockPlugin.Interfaces
{
    internal interface IDockPageViewModel
    {
        public List<HomeDevice> HomeDevices { get; set; }
        public HomeDevice? SelectedHomeDevice { get; set; }
    }
}