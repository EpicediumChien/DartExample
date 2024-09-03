using DDPM.UI.Common.Models;

namespace DDPM.UI.Plugin.SettingsPlugin
{
    internal interface ISettingsPageViewModel
    {
        public List<HomeDevice> HomeDevices { get; set; }
        public HomeDevice? SelectedHomeDevice { get; set; }
    }
}