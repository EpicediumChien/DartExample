using System.Windows.Media;
using DDPM.UI.Common;

namespace DDPM.UI.Plugin.DdpmHomePlugin.Interfaces {
  public interface IDeviceInfo {
    public ImageSource? DeviceImage { get; set; }
    public string? DeviceName { get; set; }
    public eDeviceCategory DeviceCategory { get; set; }
  }
}
