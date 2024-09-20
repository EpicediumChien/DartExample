using Dell.Client.Framework.Common;
using System;
using System.Threading.Tasks;
using VcpCore.Common;

namespace DDPM.SA.Common
{
    public interface IDisplayProperties : IFrameworkPlugin
    {
        /// <summary>
        /// HDR change event，return HDR status
        /// </summary>
        event EventHandler<bool> HDRChangeEvent;

        Task<DisplayPropertiesInfo> GetDisplayPropertiesInfo(MonitorInfo monitorInfo, string s, bool isSupportedHDR, bool isHDREnable, bool isSupportUSBCPrioritization, USBCPrioritizationType USBCPrioritizationType);

        Task<DisplaySupportedProperties> GetDisplaySupportedProperties(MonitorInfo monitorInfo);

        Task<DisplayOrientation> GetCurrentDisplayOrientation(string DisplayName);

        Task<bool> SetDisplayPropertiest(string DisplayName, Properties properties, DisplayOrientation orientation);
        Task<bool> SetResolutions(string DisplayName, Properties properties);
        Task<bool> SetOrientation(string DisplayName, DisplayOrientation orientation);

        Task<bool> CallWindowsDisplaySetting();

        Task<bool> GetHDRStatus(EDID monitorEdid);

        Task<bool> SetHDRStatus(EDID monitorEdid, bool onoff);

        void SetExtendMode(MonitorInfo monitorInfo);
    }
}