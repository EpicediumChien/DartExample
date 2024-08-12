using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDPM.UI.Plugin.DdpmHomePlugin.Interfaces {
  internal interface IDeviceInfoService {
    /// <summary>
    /// Get the DeviceInfo
    /// </summary>
    /// <returns>Returns instance of <see cref="IDeviceInfo"/></returns>
    IDeviceInfo? GetDeviceInfo();
  }
}
