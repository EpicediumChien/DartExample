using DDPM.SA.Common.Display;
using Dell.Client.Framework.Common;
using System;
using System.Collections.Generic;
using System.Threading;
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
        event EventHandler<NKVMRespone> NKVMCLIEvent;

        event EventHandler<NKVMSetHotkey> NKVMSetHotkey;

        event EventHandler<NKVMSetVCP> NKVMSetVCPEvent;

        Task CreatNewNamedpipe();

        Task<bool> IsNamedpipeConnected();

        Task OnNKVM();

        Task OffNKVM();

        Task MonitorPlug();

        Task ToNKVM_SupportedMonitorList(List<string> supportedMonitorList);

        Task<List<string>> GetSupportedNKVM();

        Task<List<string>> UpdateSupportMonitors();

        Task UpdateMonitorInfo(List<MonitorInfo> monitorInfos, CancellationToken token);

        Task<bool> isSupportMonitor(MonitorInfo monitorInfo);

        Task SetVCPNotify(MonitorInfo monitorInfo, int vcpcode, int value);

        Task ToNKVM_HotkeySettings(List<HotkeySettings> hotkeySettings);

        Task<bool> SetHotkey(HotkeyInfo info);

        Task NKVM_ChangeLimitedSW(MonitorInfo monitorInfo, bool isON);

        Task NKVM_ChangeMonitorIndex(MonitorInfo monitorInfo);

        Task SetHotkeyResponse(string jsonstring, bool isSuccess);

        Task GetNKVMVersion();

        Task GetNKVMStatus();

        Task GetNKVMAutoConnect();

        Task GetNKVMContentTransfer();

        Task GetNKVMIncommingPort();

        Task GetNKVMOutgoingPort();

        Task GetNKVMContentTransferPort();

        Task GetNKVMSettings();

        Task NKVM_State(bool state);

        Task<bool> CallNKVMConnent();

        //Task ChangeNKVMState(bool state);
        Task CallShowNKVM(int num, int x, int y);

        Task SaveVCPcode(NKVMVCPValue value);

        Task<bool> GetOnNKVM(MonitorInfo monitorInfo, ISettingsManagerDev _SettingsPlugin);

        Task SetOnNKVM(MonitorInfo monitorInfo, bool ison, ISettingsManagerDev _SettingsPlugin);
    }
}