using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DDPM.SA.Common.Display;
using Dell.Client.Framework.Common;
using VcpCore.Common;

namespace DDPM.SA.Common
{
    public interface IPipPbpService : IFrameworkPlugin
    {
        public string LastError { get; }

        public Task<string> GetCapabilitiesString(MonitorInfo monitorInfo);
        public Task<UInt16[]> GetCapabilitiesWords(MonitorInfo monitorInfo);

        public Task<bool> SetPipModeOff(MonitorInfo monitorInfo);
        public Task<bool> SetPipModeSmall(MonitorInfo monitorInfo);
        public Task<bool> SetPipModeLarge(MonitorInfo monitorInfo);
        public Task<bool> TogglePipSize(MonitorInfo monitorInfo);
        public Task<bool> TogglePipPosition(MonitorInfo monitorInfo);
        public Task<bool> SetPbpMode(MonitorInfo monitorInfo, UInt16 modeCode);
        public Task<bool> VideoSwap(MonitorInfo monitorInfo, UInt16 x, UInt16 y);
        public Task<ObjGetVCP> GetPxpMode(MonitorInfo monitorInfo);
        public Task<List<UInt16>> GetSubInputList(MonitorInfo monitorInfo);
        public Task<List<InputSourceObj>> GetSubInputs(MonitorInfo monitorInfo);
        public Task<bool> SetSubInputs(MonitorInfo monitorInfo, InputSourceObj? sub1, InputSourceObj? sub2, InputSourceObj? sub3);
        public Task<bool> UsbSwitch(MonitorInfo monitorInfo, UInt16 target = 0);
    }
}
