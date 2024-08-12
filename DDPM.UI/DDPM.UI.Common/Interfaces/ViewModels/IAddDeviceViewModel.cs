using System.Collections.ObjectModel;
using DDPM.UI.Common.Models;
using UserControl = System.Windows.Controls.UserControl;
using DDPM.UI.Common;

namespace DDPM.UI.Interfaces {
  public interface IAddDeviceViewModel {

    #region RightView
    public UserControl? RightView { get; }
    public int RightViewHeaderSelectedIndex { get; set; }

    public ObservableCollection<RightViewHeader> RightViewHeaders { get; set; }
    #endregion

    public ModuleGroup? SelectedGroup { get; }
  }
}
