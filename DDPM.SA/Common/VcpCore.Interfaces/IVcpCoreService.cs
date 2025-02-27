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

        Task SetIsUserActive(bool IsUserActive);

        Task CancelVcpTask(Guid user_guid);

        Task<List<MonitorInfo>> GetMonitors();

        Task<List<MonitorInfo>> Re_GetMonitors(CancellationToken Token);

        Task<Dictionary<EDID, Dictionary<object, object>>> GetVCPCacheTable();

        Task<List<MultiCommandArch>> MultiCommandsRun(List<MultiCommandArch> _multiCommands);

        Task<string> GetCapabilitiesString(MonitorInfo monitorInfo, Guid guid = default, Priority priority = Priority.Low);

        Task<string> GetVCPCapabilities(MonitorInfo monitorInfo, Guid guid = default, Priority priority = Priority.Low);

        Task<ObjGetVCP> GetVCPCapability(MonitorInfo monitorInfo, byte code, Guid guid = default, int opt = 0, Priority priority = Priority.Low);

        Task<ObjGetVCP> GetVCPCapability(MonitorInfo monitorInfo, string FunctionName, Guid guid = default, int opt = 0, Priority priority = Priority.Low);

        Task<bool> SetVCPCapability(MonitorInfo monitorInfo, byte code, uint val, Guid guid = default, Priority priority = Priority.Low);

        Task<bool> SetVCPCapability(MonitorInfo monitorInfo, string FunctionName, string val, Guid guid = default, Priority priority = Priority.Low);

        event EventHandler<VCPchangedEventArgs> VCPchanged;

        event EventHandler<DDCCIchangedEventArgs> DDCCIStatuschanged;

        event EventHandler<DisplaychangedEventArgs> Displaychanged;

        event EventHandler<MonitorinfoUpdateEventArgs> MonitorinfoUpdated;
    }
}