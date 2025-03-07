using Dell.Client.Framework.Common;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VcpCore.Common;

namespace DDPM.SA.Common
{
    public interface IInputSource : IFrameworkPlugin
    {
        /// <summary>
        /// if get to setting plugin
        /// </summary>
        /// <returns></returns>
        Task<Dictionary<string, InputInfo>> GetInputSourcelist(MonitorInfo monitorInfo);

        /// <summary>
        /// if save in setting plugin
        /// </summary>
        /// <param name="inputSource"></param>
        /// <returns></returns>
        Task<List<string>> GetUSBUpstreamList(MonitorInfo monitorInfo);

        Task<string> GetUSBUpstream(MonitorInfo monitorInfo, string inputsource);

        Task<bool> GetAllUSBUpstream(MonitorInfo monitorInfo);

        Task<bool> SetUSBUpstream(MonitorInfo monitorInfo, string inputsource, string upstream);

        public Task<bool> SetAllUSBUpstream(MonitorInfo monitorInfo, string input1, string usb1, string input2, string usb2,
                                                                    string input3 = "", string usb3 = "", string input4 = "", string usb4 = "");

        Task<bool> USBSwitch(MonitorInfo monitorInfo, string inputsource1, string upstream1, string inputsource2, string upstream2);

        Task<string> GetCurrentInput(MonitorInfo monitorInfo, Guid guid = default, Priority priority = Priority.Low);
    }
}