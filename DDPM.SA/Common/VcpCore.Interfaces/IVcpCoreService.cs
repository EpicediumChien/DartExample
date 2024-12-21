using Dell.Client.Framework.Common;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VcpCore.Common;

namespace VcpCore.Interfaces
{
    public interface IVcpCoreService : IFrameworkPlugin
    {
        Task Reset0x52TimerTick(int millisecond);

        Task<List<MonitorInfo>> GetMonitors();

        Task<List<MonitorInfo>> Re_GetMonitors(CancellationToken Token);

        Task<Dictionary<EDID, Dictionary<object, object>>> GetVCPCacheTable();

        Task<string> GetCapabilitiesString(MonitorInfo monitorInfo);

        Task<string> GetVCPCapabilities(MonitorInfo monitorInfo);

        Task<ObjGetVCP> GetVCPCapability(MonitorInfo monitorInfo, byte code, int opt = 0);

        Task<ObjGetVCP> GetVCPCapability(MonitorInfo monitorInfo, string FunctionName, int opt = 0);

        Task<bool> SetVCPCapability(MonitorInfo monitorInfo, byte code, uint val);

        Task<bool> SetVCPCapability(MonitorInfo monitorInfo, string FunctionName, string val);

        event EventHandler<VCPchangedEventArgs> VCPchanged;

        event EventHandler<DDCCIchangedEventArgs> DDCCIStatuschanged;

        event EventHandler<DisplaychangedEventArgs> Displaychanged;

        event EventHandler<MonitorinfoUpdateEventArgs> MonitorinfoUpdated;
    }
}