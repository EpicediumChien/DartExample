using System.Collections.ObjectModel;
using DDPM.SA.Common;
using DDPM.UI.Common.Models;
using UserControl = System.Windows.Controls.UserControl;
using DDPM.UI.Common;

namespace DDPM.UI.Interfaces {
  public interface IPeripheralViewModel {
    /// <summary>
    /// Device Info
    /// </summary>
    string? DeviceInfo { get; set; }
    /// <summary>
    /// Device Name
    /// </summary>
    string Name { get; set; }

    /// <summary>
    /// Device Model
    /// </summary>
    string Model { get; set; }

    /// <summary>
    /// Image File Path
    /// </summary>
    string ImageFilePath { get; set; }

    /// <summary>
    /// Image File Path
    /// </summary>
    int CurrentInstanceID { get; set; }

    bool SetCurrentDevice(string instanceIDs);

    #region LeftView
    public UserControl DefaultLeftView { get; }
    public UserControl? LeftView { get; set; }
    #endregion

    #region RightView
    public UserControl? RightView { get; set; }
    public int RightViewHeaderSelectedIndex { get; set; }

    public ObservableCollection<RightViewHeader> RightViewHeaders { get; set; }
    #endregion

    public ModuleGroup? SelectedGroup { get; }
  }
}
