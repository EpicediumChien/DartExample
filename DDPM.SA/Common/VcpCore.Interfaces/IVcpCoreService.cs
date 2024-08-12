using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dell.Client.Framework.Common;
using VcpCore.Common;

namespace VcpCore.Interfaces
{
    public interface IVcpCoreService : IFrameworkPlugin
    {
        Task Reset0x52TimerTick(int millisecond);

        Task<List<MonitorInfo>> GetMonitors(bool renew = false);

        Task<List<MonitorInfo>> Re_GetMonitors();

        Task<string> GetCapabilitiesString(MonitorInfo monitorInfo);

        Task<string> GetVCPCapabilities(MonitorInfo monitorInfo);

        Task<ObjGetVCP> GetVCPCapability(MonitorInfo monitorInfo, byte code, int opt = 0);

        Task<ObjGetVCP> GetVCPCapability(MonitorInfo monitorInfo, string FunctionName, int opt = 0);

        Task<bool> SetVCPCapability(MonitorInfo monitorInfo, byte code, uint val);

        Task<bool> SetVCPCapability(MonitorInfo monitorInfoX, string FunctionName, string val);

        event EventHandler<VCPchangedEventArgs> VCPchanged;

        event EventHandler<DDCCIchangedEventArgs> DDCCIStatuschanged;

        event EventHandler<DisplaychangedEventArgs> Displaychanged;

    }
}
