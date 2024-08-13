using DDPM.UI.Plugin.DdpmHomePlugin.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("DDPM.UI.Plugin.DdpmHomePlugin.Tests")]
namespace DDPM.UI.Plugin.DdpmHomePlugin.Model {
  internal class DeviceInfoService : IDeviceInfoService {
    private readonly IDeviceInfo _deviceInfo;


    /// <summary>
    /// Default constructor
    /// </summary>
    public DeviceInfoService(IDeviceInfo deviceInfo) {
      _deviceInfo = deviceInfo;
    }

    public IDeviceInfo? GetDeviceInfo() {

      return _deviceInfo;
    }


  }
}
