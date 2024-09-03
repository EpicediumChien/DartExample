using System.Windows.Controls;

namespace DDPM.UI.Interfaces {
  public interface IDdpmModule2 {
    public UserControl? GetLeftView();
    public UserControl GetRightView();
    public string ModuleName { get; }
  }
}
