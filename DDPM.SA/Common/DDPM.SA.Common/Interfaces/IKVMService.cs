using DDPM.SA.Common.Display;
using Dell.Client.Framework.Common;
using System.Collections.Generic;
using System.Threading.Tasks;
using VcpCore.Common;

namespace DDPM.SA.Common
{
    public interface IUSBKVMService : IFrameworkPlugin
    {
        Task<Dictionary<string, PCsInfo>> GetUSBKVMPCsList(MonitorInfo monitorInfo, Dictionary<string, InputInfo> inputList);

        Task<Dictionary<string, PCsInfo>> PCInfoSwap(Dictionary<string, PCsInfo> pcsList, string swapinput1, string swapinput2);
    }

    public interface INKVMService : IFrameworkPlugin
    {
        Task OnNKVM();

        Task OffNKVM();

        Task MonitorPlug();

        Task ToNKVM_SupportedMonitorList(List<string> supportedMonitorList);

        Task<List<string>> GetSupportedNKVM();

        Task<List<string>> UpdateSupportMonitors();

        Task UpdateMonitorInfo(List<MonitorInfo> monitorInfos);

        Task<bool> isSupportMonitor(MonitorInfo monitorInfo);

        Task SetVCPNotify(MonitorInfo monitorInfo, int vcpcode, int value);

        Task ToNKVM_HotkeySettings(List<HotkeySettings> hotkeySettings);

        Task<bool> SetHotkey(HotkeyInfo info);

        Task NKVM_ChangeLimitedSW(MonitorInfo monitorInfo, bool isON);
    }
}