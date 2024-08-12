using DDPM.UI.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
