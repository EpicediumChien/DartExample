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
    }
}