using DDPM.UI.Common;
using DDPM.UI.Common.Models;
using System.Collections.ObjectModel;
using UserControl = System.Windows.Controls.UserControl;

namespace DDPM.UI.Interfaces
{
    public interface IAddDeviceViewModel
    {
        #region RightView

        public UserControl? RightView { get; }
        public int RightViewHeaderSelectedIndex { get; set; }

        public ObservableCollection<RightViewHeader> RightViewHeaders { get; set; }

        #endregion RightView

        public ModuleGroup? SelectedGroup { get; }
    }
}