using DDPM.UI.Common.Models;
using System.Windows.Controls;

namespace DDPM.UI.Common.Interfaces
{
    public interface IModuleOwner
    {
        public List<HomeDevice>? HomeDevices { get; }
        public HomeDevice? SelectedHomeDevice { get; }

        public void OpenFullView(ContentControl content);

        public void CloseFullView();

        //Jason 12/11 add LoadLeftView()
        public void LoadLeftView();

        //Robert_Lin, 2024-11-15 for KVM Hotkey FullView, user click a link will nevigate to HotkeyModule
        public bool ShowSpecificModule(string groupName, string moduleName);
    }
}