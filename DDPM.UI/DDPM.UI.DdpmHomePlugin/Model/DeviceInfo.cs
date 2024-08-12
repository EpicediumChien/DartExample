using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using DDPM.UI.Plugin.DdpmHomePlugin.Interfaces;
using DDPM.UI.Common;

namespace DDPM.UI.Plugin.DdpmHomePlugin.Model {
  /// <summary>
  /// DeviceInfo class
  /// </summary>
  public class DeviceInfo_Unused : INotifyPropertyChanged, IDeviceInfo {
    /// <summary>
    /// _deviceImage
    /// 
    /// </summary>
    private ImageSource? _deviceImage;
    /// <summary>
    /// DeviceImage
    /// 
    /// </summary>
    public ImageSource? DeviceImage {
      get => _deviceImage;
      set {
        _deviceImage = value;
        OnPropertyChanged(nameof(DeviceImage));
      }
    }

    private string? _deviceName;
    /// <summary>
    /// DeviceName
    /// </summary>
    public string? DeviceName {
      get => _deviceName;
      set {
        _deviceName = value;
        OnPropertyChanged(nameof(DeviceName));
      }
    }

    private eDeviceCategory deviceCategory;
    /// <summary>
    /// 
    /// </summary>
    public eDeviceCategory DeviceCategory {
      get { return deviceCategory; }
      set {
        deviceCategory = value;
        OnPropertyChanged(nameof(DeviceCategory));
      }
    }


    #region Monitor
    //private MonitorInfo? _monitorInfo;
    //public MonitorInfo? MonitorInfo {
    //  get => _monitorInfo;
    //  set {
    //    _monitorInfo = value;
    //    OnPropertyChanged("MonitorInfo");

    //    DeviceName = _monitorInfo?.AliasDeviceName;
    //  }
    //}

    #endregion


    #region PropertyChanged
    /// <inheritdoc/>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Raise PropertyChanged event
    /// </summary>
    /// <param name="name"></param>
    protected void OnPropertyChanged([CallerMemberName] string? name = null) {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
    #endregion
  }
}
