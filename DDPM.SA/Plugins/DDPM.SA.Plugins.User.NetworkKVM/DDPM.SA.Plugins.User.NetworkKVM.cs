using DDPM.SA.Common;
using DDPM.SA.Common.Display;
using DDPM.SA.Common.Security;
using DDPM.SA.Common.NKVM;
using DdpmJsonCommon;
using Dell.Client.Framework.Agent;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.Common.Annotations;
using Dell.Client.Framework.Common.Extensions;
using Dell.Client.Framework.Common.PluginConditions;
using Dell.Client.Framework.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VcpCore.Common;
using VcpCore.Interfaces;
using Windows.System;
using DDPM.SA.Common.Settings;
using PInvoke;
using System.Text.RegularExpressions;
using Microsoft;

namespace NetworkKVM.Plugins
{
    [Plugin(DDPM.SA.Common.IDs.DDPM_NKVM_PLUGIN_ID, pluginName, PluginOrderGroupType.Core, Version = pluginVersion)]
    [Descriptor(Description = pluginDescription)]
    [Publisher(Name = publisherCompany, Website = publisherWebsite, Support = publisherSupport)]
    [PublishedUnelevatedInterface(new[] { typeof(INKVMService) })]
    public class NKVMPlugin : BaseAgentPlugin, INKVMService, IDisposableObservable
    {
        public enum log_type
        {
            info = 0,
            error
        }
        /// <summary>
        /// //
        /// </summary>
        /// <param name="text"></param>
        /// <param name="log_type">0 means info, others means error</param>
        private void WriteLog(string text, log_type log_type = log_type.info,
            [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
            [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
            [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0)
        {
            if (string.IsNullOrEmpty(text))
                text = "";

            text = $"[NKVMPlugin] {text}, Caller Name:{memberName}, Source Line {sourceLineNumber}";
#if DEBUG
            Console.WriteLine(text);
#endif
            if (Log != null)
            {
                if (log_type == log_type.info)
                    Log.Info(text);
                else
                    Log.Error(text);
            }
        }

        #region Private Members

        private const string pluginName = "NKVMPlugin";
        private const string pluginVersion = "1.0.0";
        private const string pluginDescription = "This plugin implements NKVM Plugin.";
        private const string publisherCompany = "Dell Technologies";
        private const string publisherWebsite = "https://www.dell.com";
        private const string publisherSupport = "This plugin implements NKVM Plugin.";

        private IAgent _agent;
        private Logs _logs;
        public const string PluginLogId = "NKVM";
        private bool _runloop = true;
        private int cid = -1;

        private NamedPipeServerStream pipeServer;
        private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private CancellationTokenSource cts = new CancellationTokenSource();
        private IVcpCoreService _VcpCorePlugin;
        private List<MonitorInfo> _AllInfoMonitors = new List<MonitorInfo>();
        private List<string> _SupportedMonitors = new List<string>();
        private readonly object _pluginConditionLock = new object();
        private List<HotkeySettings> _HotkeySettings = new List<HotkeySettings>();
        private HotkeyInfo _HotkeyInfo = new HotkeyInfo();

        private object NewNKVM_lock_wait = new object();
        private object lock_wait = new object();
        private string response;
        private string command;
        private bool NKVMState = false;
        private string namedpipeName;
        private bool isMonintorChange = false;

        private bool isSetVCP = false;
        private int lockVCP = 0;

        private List<NKVMVCPValue> nKVMVCPValues = new List<NKVMVCPValue>();

        private int namedpipe_Fail = 0;

        //private bool isMonitorUpdate = false;

        //private string jsonstring_MonitorUpdate = string.Empty;

        private List<NeedUpdateMonitorInfo> updateMonitorInfos = new List<NeedUpdateMonitorInfo>(); 

        #endregion Private Members

        #region Constructor

        public NKVMPlugin(IAgent agent) : base(agent, PluginLogId)
        {
            _agent = agent;
            _logs = new Logs(Log);
        }

        #endregion Constructor

        #region Overriding methods

        protected override void OnPluginStarting()
        {
            _logs.DebugMsg("[NetworkKVM] OnPluginStarting");
            _agent.PluginManager.PluginsStarted += PluginManagerOnPluginsStarted;

            InitializeVcpCorePlugin();

            PluginCondition = new PluginStartedCondition();
            cts = new CancellationTokenSource();
            CancellationToken token = cts.Token;
            _ = Task.Run(async () => await NamedPipeServer(token));
        }

        #endregion Overriding methods

        #region INKVM implementation
        public Task CreatNewNamedpipe()
        {
            _logs.DebugMsg("[CreatNewNamedpipe] CreatNewNamedpipe...");
            if (pipeServer != null)
            {
                if (pipeServer.IsConnected)
                {
                    _logs.DebugMsg("[CreatNewNamedpipe] Named pipe is Connected.");
                }
                else
                {
                    _logs.DebugMsg("[CreatNewNamedpipe] Named pipe is Disconnected.");
                    Disconnect();
                    Task.Delay(1000).Wait();
                    cts = new CancellationTokenSource();
                    CancellationToken token = cts.Token;
                    _ = Task.Run(async () => await NamedPipeServer_UI(token));
                }
            }
            else
            {
                _logs.DebugMsg("[CreatNewNamedpipe] Named pipe is null.");
                cts = new CancellationTokenSource();
                CancellationToken token = cts.Token;
                _ = Task.Run(async () => await NamedPipeServer_UI(token));
            }

            return Task.CompletedTask;
        }

        public Task<bool> IsNamedpipeConnected()
        {
            if (pipeServer != null)
            {
                if (pipeServer.IsConnected)
                {
                    _logs.DebugMsg("[IsNamedpipeConnected] NamedPipe is Connected.");
                    return Task.FromResult(true);
                }
                else
                    _logs.DebugMsg("[IsNamedpipeConnected] NamedPipe is NOT Connected.");
            }
            return Task.FromResult(false);
        }

        /// <summary>
        /// This function is used to distinc the monitor add or remove
        /// </summary>
        /// <param name="monitorInfos">new coming MonitorInfo list, after check, replace to current object</param>
        /// <returns></returns>
        public Task UpdateMonitorInfo(List<MonitorInfo> monitorInfos, CancellationToken token)
        {
            //lock (NewNKVM_lock_wait)
            //{
            if (monitorInfos == null || monitorInfos.Count == 0)
            {
                _logs.DebugMsg("[UpdateMonitorInfo] monitorInfos is null or zero count.");
                if (_AllInfoMonitors.Count == 0)
                {
                    _logs.DebugMsg("[UpdateMonitorInfo] _AllInfoMonitors count is zero.");
                    //return Task.CompletedTask;//return directly, no change
                }
                _AllInfoMonitors.Clear();
                //means unplug all connected dell monitors
                //Call Func: OnMonitorUnPlug(_AllInfoMonitors);
                //_SupportedMonitors = GetSupportedNKVM().Result;
                if (pipeServer != null)
                {
                    if (pipeServer.IsConnected)
                    {
                        _logs.DebugMsg("[UpdateMonitorInfo] MonitorPlug wait.");
                        MonitorPlug().Wait();
                    }
                    else
                    {
                        _logs.DebugMsg("[UpdateMonitorInfo] do disconnect(1).");
                        Disconnect();
                        Task.Delay(1000).Wait();
                        isMonintorChange = true;
                        _logs.DebugMsg("[UpdateMonitorInfo] await NamedPipeServer(1).");
                        _ = Task.Run(async () => await NamedPipeServer(token));
                    }
                }
                else
                {
                    _logs.DebugMsg("[UpdateMonitorInfo] await NamedPipeServer(1).");
                    _ = Task.Run(async () => await NamedPipeServer(token));
                }
            }
            else
            {
                if (_AllInfoMonitors.Count == 0 && monitorInfos.Count > 0)
                {
                    //means plugin 1 or more monitor in
                    _logs.DebugMsg("[UpdateMonitorInfo] monitor 0 -> 1");
                    //Call Func: OnMonitorPlugIn(List<MonitorInfo> mos);
                    //_SupportedMonitors = GetSupportedNKVM().Result;
                    _logs.DebugMsg("[UpdateMonitorInfo] NKVMState:" + NKVMState);
                    //if (NKVMState)
                    //{
                    _logs.DebugMsg($"[UpdateMonitorInfo] add {monitorInfos.Count} monitor(s) (1)");
                    _AllInfoMonitors.AddRange(monitorInfos);
                    if (pipeServer != null)
                    {
                        if (pipeServer.IsConnected)
                        {
                            //ResponseSupportedMonitor();
                            _logs.DebugMsg("[UpdateMonitorInfo] MonitorPlug no wait(1).");
                            MonitorPlug();
                        }
                        else
                        {
                            _logs.DebugMsg("[UpdateMonitorInfo] do disconnect(2).");
                            Disconnect();
                            Task.Delay(1000).Wait();
                            isMonintorChange = true;
                            //_runloop = true;
                            _logs.DebugMsg("[UpdateMonitorInfo] await NamedPipeServer(2).");
                            _ = Task.Run(async () => await NamedPipeServer(token));
                        }
                    }
                    else
                    {
                        _logs.DebugMsg("[UpdateMonitorInfo] await NamedPipeServer(3).");
                        isMonintorChange = true;
                        _ = Task.Run(async () => await NamedPipeServer(token));
                    }
                }
                else
                {
                    List<MonitorInfo> unplug = _AllInfoMonitors
                        .Where(x => !monitorInfos.Any(y => y.edid.ModelName == x.edid.ModelName &&
                                                            y.edid.SerialNumber == x.edid.SerialNumber &&
                                                            y.DisplayName == x.DisplayName))
                        .ToList();
                    //if (unplug.Count > 0)//means unplug
                    //{
                    //    //Call Func: OnMonitorUnPlug(unplug);
                    //    //_SupportedMonitors = GetSupportedNKVM().Result;
                    //    _logs.DebugMsg($"[UpdateMonitorInfo] unplug {unplug.Count} monitor(s)");
                    //    if (pipeServer != null)
                    //    {
                    //        if (pipeServer.IsConnected)
                    //        {
                    //            _logs.DebugMsg("[UpdateMonitorInfo] MonitorPlug no wait(2).");
                    //            MonitorPlug();
                    //        }
                    //        else
                    //        {
                    //            _logs.DebugMsg("[UpdateMonitorInfo] do disconnect(3).");
                    //            Disconnect();
                    //            Task.Delay(1000).Wait();
                    //            isMonintorChange = true;
                    //            //_runloop = true;
                    //            _logs.DebugMsg("[UpdateMonitorInfo] await NamedPipeServer(4).");
                    //            _ = Task.Run(async () => await NamedPipeServer(token));
                    //        }
                    //    }
                    //    else
                    //    {
                    //        _logs.DebugMsg("[UpdateMonitorInfo] await NamedPipeServer(5).");
                    //        isMonintorChange = true;
                    //        _ = Task.Run(async () => await NamedPipeServer(token));
                    //    }
                    //}
                    List<MonitorInfo> plugin = monitorInfos
                        .Where(x => !_AllInfoMonitors.Any(y => y.edid.ModelName == x.edid.ModelName &&
                                                               y.edid.SerialNumber == x.edid.SerialNumber &&
                                                               y.DisplayName == x.DisplayName))
                        .ToList();
                    //if (plugin.Count > 0)//means plugin
                    //{
                    //    //Call Func: OnMonitorPlugIn(plugin);
                    //    //_SupportedMonitors = GetSupportedNKVM().Result;
                    //    if (pipeServer != null)
                    //    {
                    //        if (pipeServer.IsConnected)
                    //        {
                    //            _logs.DebugMsg("[UpdateMonitorInfo] MonitorPlug no wait(3).");
                    //            //ResponseSupportedMonitor();
                    //            MonitorPlug();
                    //        }
                    //        else
                    //        {
                    //            _logs.DebugMsg("[UpdateMonitorInfo] do disconnect(4).");
                    //            Disconnect();
                    //            Task.Delay(1000).Wait();
                    //            isMonintorChange = true;
                    //            //_runloop = true;
                    //            _logs.DebugMsg("[UpdateMonitorInfo] await NamedPipeServer(6).");
                    //            _ = Task.Run(async () => await NamedPipeServer(token));
                    //        }
                    //    }
                    //    else
                    //    {
                    //        _logs.DebugMsg("[UpdateMonitorInfo] await NamedPipeServer(7).");
                    //        isMonintorChange = true;
                    //        _ = Task.Run(async () => await NamedPipeServer(token));
                    //    }
                    //}
                    _logs.DebugMsg($"[UpdateMonitorInfo] clear and add {monitorInfos.Count} monitor(s)");
                    _AllInfoMonitors.Clear();
                    _AllInfoMonitors.AddRange(monitorInfos);
                    //if (unplug.Count > 0 || plugin.Count > 0)
                    //{
                    if (pipeServer != null)
                    {
                        if (pipeServer.IsConnected)
                        {
                            _logs.DebugMsg("[UpdateMonitorInfo] MonitorPlug no wait(3).");
                            //ResponseSupportedMonitor();
                            MonitorPlug();
                        }
                        else
                        {
                            _logs.DebugMsg("[UpdateMonitorInfo] do disconnect(4).");
                            Disconnect();
                            Task.Delay(1000).Wait();
                            isMonintorChange = true;
                            //_runloop = true;
                            _logs.DebugMsg("[UpdateMonitorInfo] await NamedPipeServer(6).");
                            _ = Task.Run(async () => await NamedPipeServer(token));
                        }
                    }
                    else
                    {
                        _logs.DebugMsg("[UpdateMonitorInfo] await NamedPipeServer(7).");
                        isMonintorChange = true;
                        _ = Task.Run(async () => await NamedPipeServer(token));
                    }
                    //}
                }
            }
            return Task.CompletedTask;
            //}
        }

        public Task MonitorPlug()
        {
            _logs.DebugMsg("[NetworkKVM] MonitorPlug....");
            if (pipeServer != null && pipeServer.IsConnected)
            {
                MONITOR_PLUG_DETECTION _COMMAND = new MONITOR_PLUG_DETECTION();
                _COMMAND.UpdateChecksum();
                WriteAsync(_COMMAND.ToJson()).Wait();
            }
            return Task.CompletedTask;
        }

        public Task ToNKVM_SupportedMonitorList(List<string> supportedMonitorList)
        {
            //_SupportedMonitors = supportedMonitorList;
            return Task.CompletedTask;
        }

        public Task<List<string>> UpdateSupportMonitors()
        {
            return Task.FromResult(_SupportedMonitors);
        }

        public Task<List<string>> GetSupportedNKVM()
        {
            _logs.DebugMsg("[NetworkKVM] GetSupportedNKVM....");
            //bool isAdd = false;
            _AllInfoMonitors = GetMonitors().Result;
            foreach (MonitorInfo monitorInfo in _AllInfoMonitors)
            {
                string ModelName = monitorInfo.modelName;
                string capabilityString = monitorInfo.CapabilityString;
                if (monitorInfo.CapabilityDic.ContainsKey("C6"))
                {
                    _logs.DebugMsg("[NetworkKVM] C6....");
                    if (_SupportedMonitors != null)
                    {
                        if (_SupportedMonitors.Count > 0)
                        {
                            if (_SupportedMonitors.IndexOf(ModelName) == -1 &&
                                IsSupportNKVM(capabilityString))
                            {
                                _SupportedMonitors.Add(ModelName);
                                //isAdd = true;
                            }
                        }
                        else
                        {
                            if (IsSupportNKVM(capabilityString))
                            {
                                _SupportedMonitors.Add(ModelName);
                                //isAdd = true;
                            }
                        }
                    }
                    else
                    {
                        _SupportedMonitors = new List<string>();
                        if (IsSupportNKVM(capabilityString))
                        {
                            _SupportedMonitors.Add(ModelName);
                            //isAdd = true;
                        }
                    }
                }
            }
            //if (pipeServer.IsConnected)
            //{
            //    WriteAsync(ResponseSupportedMonitor().Result).Wait();
            //}
            return Task.FromResult(_SupportedMonitors);
        }

        public Task OnNKVM()
        {
            _logs.DebugMsg("[NetworkKVM] OnNKVM....");
            if (pipeServer != null &&
                pipeServer.IsConnected)
            {
                cid = cid + 1;
                ON_NKVM _COMMAND = new ON_NKVM();
                _COMMAND.cid = cid;
                _COMMAND.UpdateChecksum();
                //_COMMAND.type = "ON_NKVM";
                NKVMState = true;
                WriteAsync(_COMMAND.ToJson()).Wait();
            }
            return Task.CompletedTask;
        }

        public Task OffNKVM()
        {
            _logs.DebugMsg("[NetworkKVM] OffNKVM....");
            if (pipeServer != null &&
                pipeServer.IsConnected)
            {
                cid = cid + 1;
                OFF_NKVM _COMMAND = new OFF_NKVM();
                _COMMAND.cid = cid;
                _COMMAND.UpdateChecksum();
                //_COMMAND.type = "OFF_NKVM";
                NKVMState = false;
                WriteAsync(_COMMAND.ToJson()).Wait();
            }
            return Task.CompletedTask;
        }

        public Task<bool> isSupportMonitor(MonitorInfo monitorInfo)
        {
            _logs.DebugMsg("[NetworkKVM] isSupportMonitor");
            string ModelName = monitorInfo.modelName.Replace(" ", "");
            _logs.DebugMsg("[NetworkKVM] ModelName : " + ModelName);

            NKVMSupportList nKVMSupport = new NKVMSupportList();
            if (nKVMSupport.SupportList.Exists(x => ModelName.Contains(x)))
            {
                _logs.DebugMsg("[NetworkKVM] Monitor is support monitor");
                return Task.FromResult(true);
            }
            else
            {
                if (monitorInfo.CapabilityDic.ContainsKey("C6"))
                {
                    _logs.DebugMsg("[NetworkKVM] C6....");
                    string capabilityString = monitorInfo.CapabilityString;
                    if (_SupportedMonitors != null)
                    {
                        if (_SupportedMonitors.Count > 0)
                        {
                            if (_SupportedMonitors.IndexOf(ModelName) == -1)
                            {
                                _logs.DebugMsg("[NetworkKVM] _SupportedMonitors is not find monitor");
                                if (IsSupportNKVM(capabilityString))
                                {
                                    _logs.DebugMsg("[NetworkKVM] IsSupportNKVM is true");
                                    _SupportedMonitors.Add(ModelName);
                                    return Task.FromResult(true);
                                }
                            }
                            else
                            {
                                _logs.DebugMsg("[NetworkKVM] _SupportedMonitors is find monitor");
                                return Task.FromResult(true);
                            }
                        }
                        else
                        {
                            _logs.DebugMsg("[NetworkKVM] _SupportedMonitors count is 0");
                            if (IsSupportNKVM(capabilityString))
                            {
                                _logs.DebugMsg("[NetworkKVM] IsSupportNKVM is true");
                                _SupportedMonitors.Add(ModelName);
                                return Task.FromResult(true);
                            }
                        }
                    }
                    else
                    {
                        _logs.DebugMsg("[NetworkKVM] _SupportedMonitors is null");
                        _SupportedMonitors = new List<string>();
                        if (IsSupportNKVM(capabilityString))
                        {
                            _logs.DebugMsg("[NetworkKVM] IsSupportNKVM is true");
                            _SupportedMonitors.Add(ModelName);
                            return Task.FromResult(true);
                        }
                    }
                }
                else
                {
                    _logs.DebugMsg("[NetworkKVM]No C6....");
                }
            }

            return Task.FromResult(false);
        }

        public Task<bool> HaveSuppertMonitor()
        {
            //if (_AllInfoMonitors == null || _AllInfoMonitors.Count == 0)
            //{
            _AllInfoMonitors = GetMonitors().Result;//_DisplayPlugin.GetMonitors();
            //}
            foreach (MonitorInfo monitorInfo in _AllInfoMonitors)
            {
                if (IsDisposed)
                {
                    break;
                }
                if (isSupportMonitor(monitorInfo).Result && monitorInfo.DDCisON)
                {
                    _logs.DebugMsg("[NetworkKVM] HaveSuppertMonitor is : " + monitorInfo.modelName);
                    return Task.FromResult(true);
                }
            }
            return Task.FromResult(false);
        }

        public Task SetVCPNotify(MonitorInfo monitorInfo, int vcpcode, int value)
        {
            try
            {
                if (pipeServer != null &&
                    pipeServer.IsConnected)
                {
                    _logs.DebugMsg("[NetworkKVM] SetVCPNotify VcpCode : " + vcpcode.ToString());
                    _logs.DebugMsg("[NetworkKVM] SetVCPNotify value : " + value.ToString());
                    if (!isSetVCP || (isSetVCP && lockVCP != vcpcode))
                    {
                        if (vcpcode == 0x60 && value == 0)
                        {
                            return Task.CompletedTask;
                        }
                        SET_VCP_NOTIFY set_VCP_NOTIFY = new SET_VCP_NOTIFY();
                        set_VCP_NOTIFY.MonitorIndex = monitorInfo.Index;
                        set_VCP_NOTIFY.VcpCode = vcpcode;
                        set_VCP_NOTIFY.Value = value;
                        set_VCP_NOTIFY.UpdateChecksum();

                        WriteAsync(set_VCP_NOTIFY.ToJson()).Wait();
                    }
                    else
                    {
                        _logs.DebugMsg("[NetworkKVM] isSetVCP : " + isSetVCP.ToString());
                    }
                }
                if (vcpcode == 0x60)
                {
                    _AllInfoMonitors = GetMonitors().Result;
                }
            }
            catch (Exception ex)
            {
                _logs?.DebugMsg("[NetworkKVM] SetVCPNotify " + ex.ToString());
            }
            return Task.CompletedTask;
        }

        public Task ToNKVM_HotkeySettings(List<HotkeySettings> hotkeySettings)
        {
            _HotkeySettings = hotkeySettings;
            return Task.CompletedTask;
        }

        public Task<bool> SetHotkey(HotkeyInfo info)
        {
            if (info != null)
            {
                _HotkeyInfo = info;
            }
            try
            {
                _logs.DebugMsg("[NetworkKVM] SetHotkey....");
                if (pipeServer.IsConnected)
                {
                    SET_HOTKEY set_HOTKEY = new SET_HOTKEY();
                    HotkeyWinform hotkeyWinform = new HotkeyWinform();
                    hotkeyWinform.Id = HotkeyWinform.HotkeyId.None;
                    foreach (VirtualKey hotkey in _HotkeyInfo.Hotkey)
                    {
                        if (IsDisposed)
                        {
                            break;
                        }
                        if (hotkey == VirtualKey.Menu)
                        {
                            hotkeyWinform.Alt = true;
                            _logs.DebugMsg("[SetHotkey] Alt");
                        }
                        else
                        {
                            hotkeyWinform.Alt = false;
                        }
                        if (hotkey == VirtualKey.Control)
                        {
                            hotkeyWinform.Control = true;
                            _logs.DebugMsg("[SetHotkey] Ctrl");
                        }
                        else
                        {
                            hotkeyWinform.Control = false;
                        }
                        if (hotkey == VirtualKey.Shift)
                        {
                            hotkeyWinform.Shift = true;
                            _logs.DebugMsg("[SetHotkey] Shift");
                        }
                        else
                        {
                            hotkeyWinform.Shift = false;
                        }
                        if (hotkey != VirtualKey.Menu && hotkey != VirtualKey.Shift && hotkey != VirtualKey.Control)
                        {
                            hotkeyWinform.Key = (int)hotkey;
                            _logs.DebugMsg($"[SetHotkey] Key {hotkey}");
                            break;
                        }
                    }
                    cid = cid + 1;
                    set_HOTKEY.cid = cid;
                    set_HOTKEY.Hotkey = hotkeyWinform;
                    set_HOTKEY.UpdateChecksum();

                    WriteAsync(set_HOTKEY.ToJson()).Wait();
                }
            }
            catch (Exception e)
            {
                //_logs.DebugMsg($"[SetHotkey] exception: {e.Message}");
                WriteLog($"[SetHotkey] exception: {e.Message}", log_type.error);
            }
            return Task.FromResult(false);
        }

        public Task NKVM_ChangeLimitedSW(MonitorInfo monitorInfo, bool isON)
        {
            try
            {
                if (pipeServer.IsConnected)
                {
                    _logs.DebugMsg("[NetworkKVM] ChangeLimitedSW.....");
                    if (_AllInfoMonitors != null && _AllInfoMonitors.Count != 0)
                    {
                        foreach (MonitorInfo monitor in _AllInfoMonitors)
                        {
                            if (IsDisposed)
                            {
                                break;
                            }
                            if (monitor.modelName == monitorInfo.modelName && monitor.series == monitorInfo.series)
                            {
                                _logs.DebugMsg("[NetworkKVM] DDCisON: " + monitor.DDCisON.ToString());
                                _logs.DebugMsg("[NetworkKVM] isON: " + isON.ToString());
                                if (isON)
                                {
                                    _logs.DebugMsg("[NetworkKVM] DDCCI is changed from off to on");
                                    SendChangeLimitedSW(monitorInfo, false);
                                }
                                else
                                {
                                    _logs.DebugMsg("[NetworkKVM] DDCCI is changed from on to off");
                                    SendChangeLimitedSW(monitorInfo, true);
                                }
                                break;
                            }
                        }
                    }
                    else
                    {
                        _logs.DebugMsg("[NetworkKVM] No all monitor DDCCI is changed");
                        SendChangeLimitedSW(monitorInfo, false);
                    }
                }
            }
            catch (Exception e)
            {
                //_logs.DebugMsg($"[NKVM_ChangeLimitedSW] exception: {e.Message}");
                WriteLog($"[NKVM_ChangeLimitedSW] exception: {e.Message}", log_type.error);
            }

            _AllInfoMonitors = GetMonitors().Result;

            return Task.CompletedTask;
        }

        public Task NKVM_ChangeMonitorIndex(MonitorInfo monitorInfo)
        {
            try
            {
                if (monitorInfo != null && pipeServer.IsConnected)
                {
                    _logs.DebugMsg("[NetworkKVM] MonitorIndexChange...");
                    CHANGE_MONITOR_ID change_MONITOR_ID = new CHANGE_MONITOR_ID();
                    change_MONITOR_ID.MonitorId = monitorInfo.Index;
                    change_MONITOR_ID.UpdateChecksum();

                    WriteAsync(change_MONITOR_ID.ToJson()).Wait();
                }
            }
            catch (Exception e)
            {
                //_logs.DebugMsg($"[NKVM_ChangeMonitorIndex] exception: {e.Message}");
                WriteLog($"[NKVM_ChangeMonitorIndex] exception: {e.Message}", log_type.error);
            }
            return Task.CompletedTask;
        }
        /// <summary>
        /// DDPMtoNKVM SetHotkeyResponse
        /// </summary>
        /// <param name="jsonstring"></param>
        /// <param name="isSuccess"></param>
        /// <returns></returns>
        public Task SetHotkeyResponse(string jsonstring, bool isSuccess)
        {
            _logs.DebugMsg("[NetworkKVM] SetHotkeyResponse....");
            SET_HOTKEY set_HOTKEY = new SET_HOTKEY();
            set_HOTKEY = JsonConvert.DeserializeObject<SET_HOTKEY>(jsonstring);
            SET_HOTKEY_RESPONSE set_HOTKEY_RESPONSE = new SET_HOTKEY_RESPONSE();
            if (set_HOTKEY != null)
            {
                set_HOTKEY_RESPONSE.cid = set_HOTKEY.cid;
                set_HOTKEY_RESPONSE.Hotkey = set_HOTKEY.Hotkey;
                if (set_HOTKEY.IsChecksumValid())
                {
                    if (isSuccess)
                    {
                        set_HOTKEY_RESPONSE.Success = true;
                        set_HOTKEY_RESPONSE.UpdateChecksum();
                        if (set_HOTKEY_RESPONSE.ToJson() != string.Empty)
                        {
                            WriteAsync(set_HOTKEY_RESPONSE.ToJson()).Wait();
                        }
                        return Task.CompletedTask;
                    }
                    else
                    {
                        _logs.DebugMsg("[NetworkKVM] SetHotkey isn't Success....");
                    }
                }
                else
                {
                    _logs.DebugMsg("[NetworkKVM] Checksum Fail....");
                }
            }
            else
            {
                _logs.DebugMsg("[NetworkKVM] Not SET_HOTKEY");
            }
            set_HOTKEY_RESPONSE.Success = false;
            set_HOTKEY_RESPONSE.UpdateChecksum();
            if (set_HOTKEY_RESPONSE.ToJson() != string.Empty)
            {
                WriteAsync(set_HOTKEY_RESPONSE.ToJson()).Wait();
            }
            return Task.CompletedTask;
        }

        #region CLI_NKVM
        public Task GetNKVMVersion()
        {
            if (pipeServer.IsConnected)
            {
                _logs.DebugMsg("[NetworkKVM] GetNKVMVersion...");
                GET_NKVM_VERSION get_NKVM_VERSION = new GET_NKVM_VERSION();
                cid = cid + 1;
                get_NKVM_VERSION.cid = cid;
                get_NKVM_VERSION.UpdateChecksum();
                WriteAsync(get_NKVM_VERSION.ToJson()).Wait();
            }
            return Task.CompletedTask;
        }

        public Task GetNKVMStatus()
        {
            if (pipeServer.IsConnected)
            {
                _logs.DebugMsg("[NetworkKVM] GetNKVMStatus...");
                GET_NKVM_STATUS get_NKVM_STATUS = new GET_NKVM_STATUS();
                cid = cid + 1;
                get_NKVM_STATUS.cid = cid;
                get_NKVM_STATUS.UpdateChecksum();
                WriteAsync(get_NKVM_STATUS.ToJson()).Wait();
            }
            return Task.CompletedTask;
        }

        public Task GetNKVMAutoConnect()
        {
            if (pipeServer.IsConnected)
            {
                _logs.DebugMsg("[NetworkKVM] GetNKVMAutoConnect...");
                GET_NKVM_AUTO_CONNECT get_NKVM_AUTO_CONNECT = new GET_NKVM_AUTO_CONNECT();
                cid = cid + 1;
                get_NKVM_AUTO_CONNECT.cid = cid;
                get_NKVM_AUTO_CONNECT.UpdateChecksum();
                WriteAsync(get_NKVM_AUTO_CONNECT.ToJson()).Wait();
            }
            return Task.CompletedTask;
        }

        public Task GetNKVMContentTransfer()
        {
            if (pipeServer.IsConnected)
            {
                _logs.DebugMsg("[NetworkKVM] GetNKVMContentTransfer...");
                GET_NKVM_CONTENT_TRANSFER get_NKVM_CONTENT_TRANSFER = new GET_NKVM_CONTENT_TRANSFER();
                cid = cid + 1;
                get_NKVM_CONTENT_TRANSFER.cid = cid;
                get_NKVM_CONTENT_TRANSFER.UpdateChecksum();
                WriteAsync(get_NKVM_CONTENT_TRANSFER.ToJson()).Wait();
            }
            return Task.CompletedTask;
        }

        public Task GetNKVMIncommingPort()
        {
            if (pipeServer.IsConnected)
            {
                _logs.DebugMsg("[NetworkKVM] GetNKVMIncommingPort...");
                GET_NKVM_INCOMMING_PORT get_NKVM_INCOMMING_PORT = new GET_NKVM_INCOMMING_PORT();
                cid = cid + 1;
                get_NKVM_INCOMMING_PORT.cid = cid;
                get_NKVM_INCOMMING_PORT.UpdateChecksum();
                WriteAsync(get_NKVM_INCOMMING_PORT.ToJson()).Wait();
            }
            return Task.CompletedTask;
        }

        public Task GetNKVMOutgoingPort()
        {
            if (pipeServer.IsConnected)
            {
                _logs.DebugMsg("[NetworkKVM] GetNKVMOutgoingPort...");
                GET_NKVM_OUTGOING_PORT get_NKVM_OUTGOING_PORT = new GET_NKVM_OUTGOING_PORT();
                cid = cid + 1;
                get_NKVM_OUTGOING_PORT.cid = cid;
                get_NKVM_OUTGOING_PORT.UpdateChecksum();
                WriteAsync(get_NKVM_OUTGOING_PORT.ToJson()).Wait();
            }
            return Task.CompletedTask;
        }

        public Task GetNKVMContentTransferPort()
        {
            if (pipeServer.IsConnected)
            {
                _logs.DebugMsg("[NetworkKVM] GetNKVMContentTransferPort...");
                GET_NKVM_CONTENT_TRANSFER_PORT get_NKVM_CONTENT_TRANSFER_PORT = new GET_NKVM_CONTENT_TRANSFER_PORT();
                cid = cid + 1;
                get_NKVM_CONTENT_TRANSFER_PORT.cid = cid;
                get_NKVM_CONTENT_TRANSFER_PORT.UpdateChecksum();
                WriteAsync(get_NKVM_CONTENT_TRANSFER_PORT.ToJson()).Wait();
            }
            return Task.CompletedTask;
        }

        public Task GetNKVMSettings()
        {
            if (pipeServer.IsConnected)
            {
                _logs.DebugMsg("[NetworkKVM] GetNKVMSettings...");
                GET_NKVM_SETTINGS get_NKVM_SETTINGS = new GET_NKVM_SETTINGS();
                cid = cid + 1;
                get_NKVM_SETTINGS.cid = cid;
                get_NKVM_SETTINGS.UpdateChecksum();
                WriteAsync(get_NKVM_SETTINGS.ToJson()).Wait();
            }
            return Task.CompletedTask;
        }
        #endregion

        public Task NKVM_State(bool state)
        {
            NKVMState = state;
            return Task.CompletedTask;
        }

        //public Task OpenNKVMUI(System.Windows.Window mainWindow, int index, int x, int y)
        //{
        //    var directory = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

        //    directory = $"C:\\Program Files\\Dell\\Dell Display and Peripheral Manager";
        //    string strFullPath = string.Format("{0}\\Plugins\\NKVM\\DDM.exe", directory);

        //    Trace.WriteLine($"NKVM full path is {strFullPath}");

        //    try
        //    {
        //        IntPtr NkvmdHandle = IntPtr.Zero;
        //        string processName = "DDM";
        //        Process[] processes = Process.GetProcessesByName(processName);

        //        if (processes.Length == 0)
        //        {
        //            Console.WriteLine("No process found with the name: " + processName);

        //            Process procNew = new Process();
        //            procNew.StartInfo.FileName = strFullPath;
        //            procNew.Start();
        //        }
        //        else
        //        {
        //            foreach (Process process in processes)
        //            {
        //                NkvmdHandle = process.Handle;
        //                Console.WriteLine($"Process ID: {process.Id}, Handle: {NkvmdHandle}");
        //                break;
        //            }
        //        }

        //        Process proc = new Process();
        //        proc.StartInfo.FileName = strFullPath;
        //        proc.StartInfo.Arguments = $"/ShowNKVM {index} {x} {y}";
        //        proc.Start();

        //        try
        //        {
        //            proc.WaitForInputIdle();
        //        }
        //        catch (Exception)
        //        {
        //        }

        //        if (NkvmdHandle == IntPtr.Zero)
        //        {
        //            // Get the handle of the NKVM main window
        //            NkvmdHandle = proc.MainWindowHandle;
        //        }

        //        IntPtr mainWindowHandle = new WindowInteropHelper(mainWindow).Handle;

        //        if (NkvmdHandle != IntPtr.Zero/* && mainWindowHandle != IntPtr.Zero*/)
        //        {
        //            //SetParent(NkvmdHandle, mainWindowHandle);
        //            DDPMWindowPos windowPos = new DDPMWindowPos(NkvmdHandle, mainWindowHandle);
        //            //SetWindowPos(mainWindowHandle, HWND_BOTTOM, 0, 0, 0, 0, SWP_NOSIZE | SWP_NOMOVE | SWP_NOACTIVATE | SWP_NOOWNERZORDER | SWP_NOREDRAW);
        //            //EnableWindow(mainWindowHandle, false);
        //        }

        //        //WindowInteropHelper helper = new WindowInteropHelper(mainWindow);
        //        //helper.Owner = NkvmdHandle;
        //    }
        //    catch (System.Exception ex)
        //    {
        //        Trace.WriteLine($"ERROR : Run NKVM ==> {ex.ToString()}");
        //        Task.Delay(1000).Wait();
        //    }
        //    return Task.CompletedTask;
        //}

        public Task<bool> CallNKVMConnent()
        {
            _logs.DebugMsg("[NetworkKVM] CallNKVMConnent....");
            var directory = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

            directory = $"C:\\Program Files\\Dell\\Dell Display and Peripheral Manager";
            string strFullPath = string.Format("{0}\\Plugins\\NKVM\\{1}", directory, GlobalDefinitions.DDMExeName);

            Trace.WriteLine($"NKVM full path is {strFullPath}");

            if (File.Exists(strFullPath))
            {
                if (!string.IsNullOrEmpty(namedpipeName))
                {
                    try
                    {
                        Process proc = new Process();
                        proc.StartInfo.FileName = strFullPath;
                        proc.StartInfo.Arguments = $"/Connect " + namedpipeName;
                        _logs.DebugMsg("[NetworkKVM] Connect " + namedpipeName);
#if DEBUG
                        proc.Start();
#else
                        //Check process with inbox thumbprint and with argument via startInfo
                        //DDPMFileSecurity.StartProcessSafely(Log, proc.StartInfo, true);
                        DDPMFileSecurity.StartProcessSafely(Log, proc.StartInfo, true, "", "", false, false);//lock nkvm
#endif

                        return Task.FromResult(true);
                    }
                    catch (System.Exception ex)
                    {
                        Trace.WriteLine($"ERROR : Run NKVM ==> {ex.ToString()}");
                        //_logs.DebugMsg($"ERROR : Run NKVM ==> {ex.ToString()}");
                        WriteLog($"ERROR : Run NKVM ==> {ex.ToString()}", log_type.error);
                        Task.Delay(1000).Wait();
                        Disconnect();
                        return Task.FromResult(false);
                    }
                }
            }
            else
            {
                _logs.DebugMsg("[NetworkKVM] Not Find NKVM");
                Disconnect();
            }
            return Task.FromResult(false);
        }

        public Task CallShowNKVM(int num, int x, int y)
        {
            _logs.DebugMsg("[NetworkKVM] CallShowNKVM....");
            SHOW_NKVM show_NKVM = new SHOW_NKVM();
            show_NKVM.cid = cid + 1;
            show_NKVM.MonitorNum = num;
            show_NKVM.MonitorX = x;
            show_NKVM.MonitorY = y;
            show_NKVM.UpdateChecksum();
            if (show_NKVM.ToJson() != string.Empty)
            {
                WriteAsync(show_NKVM.ToJson()).Wait();
            }

            return Task.CompletedTask;
        }

        public Task SaveVCPcode(NKVMVCPValue value)
        {
            _logs.DebugMsg("[NetworkKVM] SaveVCPcode....");
            if (nKVMVCPValues != null)
            {
                int idx = nKVMVCPValues.FindIndex(x => x.monitorInfo == value.monitorInfo);
                if (idx >= 0)
                {
                    _logs.DebugMsg("[NetworkKVM] SaveVCPcode: exist and replace");
                    nKVMVCPValues[idx] = value;//do update with index
                }
                else
                {
                    _logs.DebugMsg("[NetworkKVM] SaveVCPcode: new add");
                    nKVMVCPValues.Add(value);//new object, add it
                }
            }
            return Task.CompletedTask;
        }

        #endregion INKVM implementation

        #region Private Methods

        private void InitializeVcpCorePlugin()
        {
            if (_VcpCorePlugin != null)
                return;

            _VcpCorePlugin = _agent.PluginManager.FindPluginByType<IVcpCoreService>(PluginResolution.Dynamic);

            if (_VcpCorePlugin is IFrameworkPluginConditionNotification VcpCoreCondition)
            {
                VcpCoreCondition.PluginConditionChangeHandler += OnVcpCorePluginConditionChangeHandler;
                GetCurrentVcpCoreCondition();
            }
        }

        private void GetCurrentVcpCoreCondition()
        {
            _ = Task.Run(async () =>
            {
                var pluginCondition = await (_VcpCorePlugin as IFrameworkPluginConditionNotification)?.CurrentConditionAsync();

                lock (_pluginConditionLock)
                {
                    if (pluginCondition is PluginErrorCondition)
                    {
                        //_VcpCorePluginCondition = pluginCondition;
                    }
                    else if (pluginCondition is PluginStartedCondition)
                    {
                        //_VcpCorePluginCondition = pluginCondition;
                        _VcpCorePlugin.VCPchanged += VCPchangedEvent;
                        _VcpCorePlugin.MonitorinfoUpdated += MonitorUpdateEvent;
                        InitializeMonitorsList();
                    }
                }
            });
        }

        private void InitializeMonitorsList()
        {
            _AllInfoMonitors.Clear();
            GetMonitors();
        }

        private Task<List<MonitorInfo>> GetMonitors()
        {
            _logs.DebugMsg("[NetworkKVM] GetMonitors");
            lock (_pluginConditionLock)
            {
                //_logs.DebugMsg("[DisplayMangerPlugin] DisplayMangerPlugin received GetMonitors requested ...");
                if (_VcpCorePlugin != null)
                {
                    _AllInfoMonitors.Clear();
                    _AllInfoMonitors.AddRange(_VcpCorePlugin.GetMonitors().Result);
                }

                //_logs.DebugMsg("[DisplayMangerPlugin] GetMonitors() AllInfoMonitors.count is " + _AllInfoMonitors.Count);

                return Task.FromResult(_AllInfoMonitors);
            }
        }

        private async Task NamedPipeServer(CancellationToken token)
        {
            var Cancellation = CancellationTokenSource.CreateLinkedTokenSource(token);
            var CancellationToken = Cancellation.Token;
            if (CreateNamedPipe_init())
            {
                _logs.DebugMsg("NKVM NamedPipeServer is go...");
                Trace.WriteLine("NKVM NamedPipeServer is go...");
                //int i = 0;
                while (_runloop)
                {
                    if (IsDisposed)
                    {
                        break;
                    }
                    //if (i > 10)
                    //{
                    //    _logs.DebugMsg("[NetworkKVM] loop error times is 10");
                    //    Disconnect();
                    //    break;
                    //}
                    if (pipeServer != null)
                    {
                        if (pipeServer.IsConnected)
                        {
                            lock (lock_wait)
                            {
                                try
                                {
                                    //i = 0;
                                    response = ReadAsync().Result;
                                    _logs.DebugMsg("[NetworkKVM] Get :" + response);
                                    if (!string.IsNullOrEmpty(response))
                                    {
                                        if (response == "Disconnect")
                                        {
                                            Disconnect();
                                            //CreateNamedPipe();
                                        }
                                        else
                                        {
                                            //var matches = Regex.Matches(response, @"\{.*?\}");
                                            List<string> respList = new List<string>();
                                            int braceCount = 0;
                                            int startIndex = 0;

                                            for (int l = 0; l < response.Length; l++)
                                            {
                                                if (IsDisposed)
                                                {
                                                    break;
                                                }
                                                if (response[l] == '{')
                                                {
                                                    if (braceCount == 0)
                                                    {
                                                        startIndex = l;
                                                    }
                                                    braceCount++;
                                                }
                                                else if (response[l] == '}')
                                                {
                                                    braceCount--;

                                                    if (braceCount == 0)
                                                    {
                                                        respList.Add(response.Substring(startIndex, l - startIndex + 1));
                                                    }
                                                }
                                            }
                                            if (respList != null)
                                            {
                                                foreach (string resp in respList)
                                                {
                                                    if (IsDisposed)
                                                    {
                                                        break;
                                                    }
                                                    _logs.DebugMsg("[NetworkKVM] response string :" + resp);
                                                    JsonstringParse(resp); //read json type
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        _logs.DebugMsg("[NetworkKVM] Get is null or empty");
                                    }
                                }
                                catch (Exception ex)
                                {
                                    //_logs.DebugMsg($"[NetworkKVM] Failed to connect {ex}");
                                    WriteLog($"[NetworkKVM] Failed to connect {ex.Message}", log_type.error);
                                    Disconnect();
                                    Task.Delay(1000).Wait();
                                    _AllInfoMonitors = GetMonitors().Result;
                                    if (CreateNamedPipe_init())
                                    {
                                        Task.Delay(500).Wait();
                                    }
                                }
                            }
                        }
                        else
                        {
                            //i++;
                            Task.Delay(500).Wait();
                        }
                    }
                    else
                    {
                        _logs.DebugMsg("pipeServer is null");
                        //Disconnect();
                        //Task.Delay(1000).Wait();
                        _AllInfoMonitors = GetMonitors().Result;
                        break; // 2024-12-13 Elie, break infinite loop when it doesn't support NKVM.
                    }
                }
            }
            _logs.DebugMsg("NKVM NamedPipeServer is End...");
            Trace.WriteLine("NKVM NamedPipeServer is End...");
        }

        private async Task NamedPipeServer_UI(CancellationToken token)
        {
            var Cancellation = CancellationTokenSource.CreateLinkedTokenSource(token);
            var CancellationToken = Cancellation.Token;
            if (CreateNamedPipe())
            {
                //CallShowNKVM(0, 100, 100);
                _logs.DebugMsg("NKVM NamedPipeServer_UI is go...");
                Trace.WriteLine("NKVM NamedPipeServer_UI is go...");
                //int i = 0;
                while (_runloop)
                {
                    //if (i > 10)
                    //{
                    //    _logs.DebugMsg("[NetworkKVM] loop error times = 10");
                    //    Disconnect();
                    //    break;
                    //}
                    if (IsDisposed)
                    {
                        break;
                    }
                    if (pipeServer != null)
                    {
                        if (pipeServer.IsConnected)
                        {
                            lock (lock_wait)
                            {
                                try
                                {
                                    //i = 0;
                                    response = ReadAsync().Result;
                                    _logs.DebugMsg("[NetworkKVM] Get :" + response);
                                    if (!string.IsNullOrEmpty(response))
                                    {
                                        if (response == "Disconnect")
                                        {
                                            Disconnect();
                                            //CreateNamedPipe();
                                        }
                                        else
                                        {
                                            //var matches = Regex.Matches(response, @"\{.*?\}");
                                            List<string> respList = new List<string>();
                                            int braceCount = 0;
                                            int startIndex = 0;

                                            for (int l = 0; l < response.Length; l++)
                                            {
                                                if (response[l] == '{')
                                                {
                                                    if (braceCount == 0)
                                                    {
                                                        startIndex = l;
                                                    }
                                                    braceCount++;
                                                }
                                                else if (response[l] == '}')
                                                {
                                                    braceCount--;

                                                    if (braceCount == 0)
                                                    {
                                                        respList.Add(response.Substring(startIndex, l - startIndex + 1));
                                                    }
                                                }
                                            }
                                            if (respList != null)
                                            {
                                                foreach (string resp in respList)
                                                {
                                                    _logs.DebugMsg("[NetworkKVM] response string :" + resp);
                                                    JsonstringParse(resp); //read json type
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        _logs.DebugMsg("[NetworkKVM] Get is null or empty");
                                    }
                                }
                                catch (Exception ex)
                                {
                                    //_logs.DebugMsg($"[NetworkKVM] Failed to connect {ex}");
                                    WriteLog($"[NetworkKVM] Failed to connect {ex.Message}", log_type.error);
                                    Disconnect();
                                    Task.Delay(1000).Wait();
                                    _AllInfoMonitors = GetMonitors().Result;
                                    if (CreateNamedPipe())
                                    {
                                        Task.Delay(500).Wait();
                                    }
                                    //else
                                    //{
                                    //    //i++;
                                    //    Task.Delay(500).Wait();
                                    //}
                                }
                            }
                        }
                        else
                        {
                            //i++;
                            Task.Delay(500).Wait();
                        }
                    }
                    else
                    {
                        _logs.DebugMsg("pipeServer is null");
                        //Disconnect();
                        //Task.Delay(1000).Wait();
                        _AllInfoMonitors = GetMonitors().Result;
                        break;
                    }
                }
            }
            _logs.DebugMsg("NKVM NamedPipeServer_UI is End...");
            Trace.WriteLine("NKVM NamedPipeServer_UI is End...");
        }

        private bool CreateNamedPipe_init()
        {
            try
            {
                if (namedpipe_Fail > 10)
                {
                    _logs.DebugMsg($"[NetworkKVM] Named pipe Fail....");
                    namedpipe_Fail = 0;
                    return false;
                }
                if (HaveSuppertMonitor().Result)
                {
#if DEBUG
                    namedpipeName = "VCPNamedPipe";
#else
                namedpipeName = Guid.NewGuid().ToString("D");
#endif
                    _logs.DebugMsg("[NetworkKVM] Name: " + namedpipeName);
                    PipeSecurity pipeSecurity = NPipeSecurity.CreatePipeSecurity(PipeAccessRights.ReadWrite);

                    pipeServer = NamedPipeServerStreamAcl.Create(namedpipeName,
                                                                PipeDirection.InOut,
                                                                NamedPipeServerStream.MaxAllowedServerInstances,
                                                                PipeTransmissionMode.Byte,
                                                                PipeOptions.Asynchronous | PipeOptions.WriteThrough,
                                                                0,
                                                                0,
                                                                pipeSecurity);
                    cancellationTokenSource = new CancellationTokenSource();
                    var c = CancellationTokenSource.CreateLinkedTokenSource(cancellationTokenSource.Token);
                    _logs.DebugMsg("[NetworkKVM] Wait Connection.....");

                    if (CallNKVMConnent().Result)
                    {
                        StartAsync().Wait();
                    }
                    else
                    {
                        _logs.DebugMsg("NKVM CreateNamedPipe_init is not NKVM...");
                        Trace.WriteLine("NKVM CreateNamedPipe_init is not NKVM...");
                        Disconnect();
                    }
                }
                else
                {
                    _logs.DebugMsg("NKVM CreateNamedPipe_init is not supperMonitor...");
                    Trace.WriteLine("NKVM CreateNamedPipe_init is not supperMonitor...");
                    Disconnect();
                    return false;
                }
                Trace.WriteLine("NKVM CreateNamedPipe_init is End...");
            }
            catch (Exception ex)
            {
                //_logs.DebugMsg("[NetworkKVM] CreateNamedPipe_init is error");
                //_logs.DebugMsg($"[NetworkKVM] Failed to create {ex}");
                _logs.DebugMsg("[NetworkKVM] CreateNamedPipe_init is error, failed to create");
                WriteLog($"[NetworkKVM] CreateNamedPipe_init is error, failed to create {ex.Message}", log_type.error);

                Disconnect();
                return false;
            }
            return true;
        }

        private bool CreateNamedPipe()
        {
            try
            {
                if (namedpipe_Fail > 10)
                {
                    _logs.DebugMsg($"[NetworkKVM] Named pipe Fail....");
                    namedpipe_Fail = 0;
                    return false;
                }
#if DEBUG
                namedpipeName = "VCPNamedPipe";
#else
            namedpipeName = Guid.NewGuid().ToString("D");
#endif
                _logs.DebugMsg("[NetworkKVM] Name: " + namedpipeName);
                PipeSecurity pipeSecurity = NPipeSecurity.CreatePipeSecurity(PipeAccessRights.ReadWrite);

                pipeServer = NamedPipeServerStreamAcl.Create(namedpipeName,
                                                            PipeDirection.InOut,
                                                            NamedPipeServerStream.MaxAllowedServerInstances,
                                                            PipeTransmissionMode.Byte,
                                                            PipeOptions.Asynchronous | PipeOptions.WriteThrough,
                                                            0,
                                                            0,
                                                            pipeSecurity);
                cancellationTokenSource = new CancellationTokenSource();
                var c = CancellationTokenSource.CreateLinkedTokenSource(cancellationTokenSource.Token);
                _logs.DebugMsg("[NetworkKVM] Wait Connection.....");
                if (CallNKVMConnent().Result)
                {
                    StartAsync().Wait();
                }
                else
                {
                    Trace.WriteLine("NKVM CreateNamedPipe is not NKVM...");
                    Disconnect();
                }
                Trace.WriteLine("NKVM CreateNamedPipe is End...");
            }
            catch (Exception ex)
            {
                _logs.DebugMsg("[NetworkKVM] CreateNamedPipe is error, failed to create");
                //_logs.DebugMsg($"[NetworkKVM] Failed to create {ex}");
                WriteLog($"[NetworkKVM] CreateNamedPipe is error, failed to create {ex.Message}", log_type.error);
                Disconnect();
                return false;
            }
            return true;
        }

        private async Task StartAsync()
        {
            await pipeServer.WaitForConnectionAsync(cancellationTokenSource.Token);
            _logs.DebugMsg("[NetworkKVM] Client Connect....");
            try
            {
#if RELEASE
            string info;
            if (NPipeSecurity.NamedPipeClientSecurity(pipeServer, out info))
            {
#endif
                _logs.DebugMsg("[NetworkKVM] Client Security Pass....");
                namedpipe_Fail = 0;
                if (isMonintorChange)
                {
                    MonitorPlug().Wait();
                    isMonintorChange = false;
                }
                //ResponseSupportedMonitor().Wait();
                OnNKVM().Wait();
                _logs.DebugMsg("[NetworkKVM] StartAsync is end");
#if RELEASE
            }
            else
            {
                _logs.DebugMsg($"[NetworkKVM] Client Security Fail....({info})");
                namedpipe_Fail++;
                Disconnect();
                Task.Delay(1000).Wait();
                CreateNamedPipe_init();
            }
#endif
            }
            catch (Exception ex)
            {
                _logs.DebugMsg("[NetworkKVM] StartAsync is error : " + ex.ToString());
            }
        }

        private void Stop()
        {
            if (cancellationTokenSource != null)
            {
                cancellationTokenSource.Cancel();
            }
            //if (cts != null)
            //{
            //    cts.Cancel();
            //}
        }

        private void Disconnect()
        {
            _logs.DebugMsg("[NetworkKVM] Disconnect....");
            Stop();
            if (pipeServer != null)
            {
                if (pipeServer.IsConnected)
                {
                    //WriteAsync(DisconnectNamedPipe().Result).Wait();
                    pipeServer.Disconnect();
                }
                pipeServer.Close();
                pipeServer.Dispose();
            }
            //_runloop = false;
        }

        private async Task WriteAsync(string message)
        {
            _logs.DebugMsg("[NetworkKVM] WriteAsync : " + message);
            byte[] buffer = Encoding.UTF8.GetBytes(message);

            if (pipeServer != null)
            {
                if (pipeServer.IsConnected)
                {
                    try
                    {
                        await pipeServer.WriteAsync(buffer, 0, buffer.Length).ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        //_logs.DebugMsg("[NetworkKVM] WriteAsync exception, message: " + ex.Message);
                        WriteLog($"WriteAsync exception, message: {ex.Message}", log_type.error);
                    }

                    await pipeServer.FlushAsync();
                    //pipeServer.WaitForPipeDrain();
                }
                else
                {
                    _logs.DebugMsg("[NetworkKVM] pipe not connected");
                }
            }
            else
            {
                _logs.DebugMsg("[NetworkKVM] pipe is null");
            }
        }

        private async Task<string> ReadAsync()
        {
            _logs.DebugMsg("[NetworkKVM] ReadAsync...");
            byte[] buffer = new byte[2048];

            int bytesRead = default;

            string readmessage = string.Empty;

            if (pipeServer != null)
            {
                if (pipeServer.IsConnected)
                {
                    try
                    {
                        bytesRead = await pipeServer.ReadAsync(buffer, 0, buffer.Length).ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        //_logs.DebugMsg("[NetworkKVM] ReadAsync failed, message: " + ex.Message);
                        WriteLog($"ReadAsync failed, message: " + ex.Message, log_type.error);
                        return string.Empty;
                    }

                    readmessage = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    _logs.DebugMsg("[NetworkKVM] ReadAsync : " + readmessage);
                }
                else
                {
                    _logs.DebugMsg("[NetworkKVM] pipe not connected");
                }
            }
            else
            {
                _logs.DebugMsg("[NetworkKVM] pipe is null");
            }
            return readmessage;
        }

        private Task JsonstringParse(string jsonstring)
        {
            string type;
            string reStr = string.Empty;
            if (!string.IsNullOrEmpty(jsonstring))
            {
                try
                {
                    JToken jToken = JToken.Parse(jsonstring);
                    if (jToken != null)
                    {
                        if (jToken.Type == JTokenType.Object)
                        {
                            _logs.DebugMsg("[NetworkKVM] jsonstring is JObject");
                            JObject json = (JObject)jToken;
                            if (json.ContainsKey("type"))
                            {
                                if (json["type"].Type != JTokenType.Null)
                                {
                                    type = (string)json["type"];
                                    _logs.DebugMsg("[NetworkKVM] type: " + type);
                                    switch (type)
                                    {
                                        case "SET_VCP":
                                            SetVCP(jsonstring);
                                            break;

                                        case "GET_VCP":
                                            GetVCP(jsonstring);
                                            break;

                                        case "GET_MONITOR_INFO":
                                            GetMonitorInfo(jsonstring);
                                            break;

                                        case "GET_CURRENT_MONITOR_INDEX":
                                            GetCurrentMonitorIndex(jsonstring);
                                            break;

                                        case "IS_HOTKEY_AVAILABLE":
                                            isHotkeyAvailable(jsonstring);
                                            break;

                                        case "DISCONNECT":
                                            Disconnect();
                                            break;

                                        //case "UPDATE_SUPPORTED_MONITOR_LIST_RESPONSE":
                                        //    if (!ResponseSucces(json).Result)
                                        //    {
                                        //        ResponseSupportedMonitor();
                                        //    }
                                        //    else
                                        //    {
                                        //        OnNKVM();
                                        //    }
                                        //    break;

                                        case "ON_NKVM_RESPONSE":
                                            if (!ResponseSucces(json).Result)
                                            {
                                                OnNKVM();
                                            }
                                            break;

                                        case "OFF_NKVM_RESPONSE":
                                            if (!ResponseSucces(json).Result)
                                            {
                                                OffNKVM();
                                            }
                                            break;

                                        case "SET_HOTKEY":
                                            GetSetHotkey(jsonstring);
                                            break;

                                        case "SET_HOTKEY_RESPONSE":
                                            if (!ResponseSucces(json).Result && _HotkeyInfo != null)
                                            {
                                                SetHotkey(_HotkeyInfo);
                                            }
                                            break;

                                        case "GET_NKVM_VERSION_RESPONSE":
                                            if (!ResponseSucces(json).Result)
                                            {
                                                GetVersionResponse(jsonstring);
                                            }
                                            break;

                                        case "GET_NKVM_STATUS_RESPONSE":
                                            if (!ResponseSucces(json).Result)
                                            {
                                                GetStatusResponse(jsonstring);
                                            }
                                            break;

                                        case "GET_NKVM_AUTO_CONNECT_RESPONSE":
                                            if (!ResponseSucces(json).Result)
                                            {
                                                GetAutoConnectResponse(jsonstring);
                                            }
                                            break;

                                        case "GET_NKVM_CONTENT_TRANSFER_RESPONSE":
                                            if (!ResponseSucces(json).Result)
                                            {
                                                GetContentTransferResponse(jsonstring);
                                            }
                                            break;

                                        case "GET_NKVM_INCOMMING_PORT_RESPONSE":
                                            if (!ResponseSucces(json).Result)
                                            {
                                                GetIncommingPortResponse(jsonstring);
                                            }
                                            break;
                                        case "GET_NKVM_OUTGOING_PORT_RESPONSE":
                                            if (!ResponseSucces(json).Result)
                                            {
                                                GetOutgoingPortResponse(jsonstring);
                                            }
                                            break;

                                        case "GET_NKVM_CONTENT_TRANSFER_PORT_RESPONSE":
                                            if (!ResponseSucces(json).Result)
                                            {
                                                GetContentTransfedPortResponse(jsonstring);
                                            }
                                            break;

                                        case "SHOW_NKVM_RESPONSE":
                                            if (!ResponseSucces(json).Result)
                                            {
                                                CallShowNKVM(0, 100, 100);
                                            }
                                            break;

                                        default:
                                            //NotFindType(json).Wait();
                                            break;
                                    }
                                }
                                else
                                {
                                    _logs.DebugMsg("[NetworkKVM] json type is null");
                                }
                            }
                            else
                            {
                                _logs.DebugMsg("[NetworkKVM] jsonstring is not find type");
                            }
                        }
                        else
                        {
                            _logs.DebugMsg("[NetworkKVM] jsonstring is not JObject");
                        }
                    }
                    else
                    {
                        _logs.DebugMsg("[NetworkKVM] jToken is null");
                    }
                }
                catch (Exception ex)
                {
                    //_logs.DebugMsg("[NetworkKVM] JsonstringParse exception : " + ex.ToString());
                    WriteLog("[NetworkKVM] JsonstringParse exception : " + ex.ToString(), log_type.error);
                }
            }
            else
            {
                _logs.DebugMsg("[NetworkKVM] jsonstring is null or empty");
            }
            return Task.CompletedTask;
        }

        private Task SetVCP(string jsonstring)
        {
            byte b_vcpcode;
            SET_VCP_RESPONSE set_VCP_R = new SET_VCP_RESPONSE();
            SET_VCP set_VCP = new SET_VCP();
            set_VCP = JsonConvert.DeserializeObject<SET_VCP>(jsonstring);
            if (set_VCP != null)
            {
                if (set_VCP.IsChecksumValid())
                {
                    if (_AllInfoMonitors == null || _AllInfoMonitors.Count == 0)
                    {
                        _AllInfoMonitors = GetMonitors().Result;//_DisplayPlugin.GetMonitors();
                    }
                    set_VCP_R.cid = set_VCP.cid;
                    //set_VCP_R.type = (string)json["type"] + "_RESPONSE";
                    if (_AllInfoMonitors.Count != 0)
                    {
                        isSetVCP = true;
                        lockVCP = set_VCP.VcpCode;
                        b_vcpcode = Convert.ToByte(set_VCP.VcpCode.ToString());
                        bool b = _VcpCorePlugin.SetVCPCapability(_AllInfoMonitors[set_VCP.MonitorIndex], b_vcpcode, (uint)set_VCP.Value).Result;
                        set_VCP_R.MonitorIndex = set_VCP.MonitorIndex;
                        set_VCP_R.VcpCode = set_VCP.VcpCode;
                        set_VCP_R.Value = set_VCP.Value;
                        set_VCP_R.Success = b;
                        set_VCP_R.UpdateChecksum();
                        if (set_VCP_R.ToJson() != string.Empty)
                        {
                            WriteAsync(set_VCP_R.ToJson()).Wait();
                        }
                        if (b)
                        {
                            _logs.DebugMsg("[NetworkKVM] SetVCPEvent.....");
                            NKVMSetVCP nKVMSetVCP = new NKVMSetVCP();
                            nKVMSetVCP.monitorInfo = _AllInfoMonitors[set_VCP.MonitorIndex];
                            nKVMSetVCP.code = set_VCP.VcpCode;
                            nKVMSetVCP.value = set_VCP.Value;
                            SetVCPEvent(nKVMSetVCP);
                        }
                        isSetVCP = false;
                        lockVCP = 0;
                        return Task.CompletedTask;
                    }
                    else
                    {
                        _logs.DebugMsg("[NetworkKVM] No Monitor....");
                    }
                }
                else
                {
                    _logs.DebugMsg("[NetworkKVM] Checksum Fail....");
                }
            }
            else
            {
                _logs.DebugMsg("[NetworkKVM] Not SET_VCP....");
            }
            set_VCP_R.MonitorIndex = -1;
            set_VCP_R.VcpCode = -1;
            set_VCP_R.Value = -1;
            set_VCP_R.Success = false;
            set_VCP_R.UpdateChecksum();
            if (set_VCP_R.ToJson() != string.Empty)
            {
                WriteAsync(set_VCP_R.ToJson()).Wait();
            }
            return Task.CompletedTask;
        }

        private Task GetVCP(string jsonstring)
        {
            byte b_vcpcode;
            ObjGetVCP objGetVCP;
            string rc_string = string.Empty;
            GET_VCP_RESPONSE get_VCP_R = new GET_VCP_RESPONSE();
            GET_VCP get_VCP = new GET_VCP();
            get_VCP = JsonConvert.DeserializeObject<GET_VCP>(jsonstring);
            if (get_VCP != null)
            {
                if (get_VCP.IsChecksumValid())
                {
                    if (_AllInfoMonitors == null || _AllInfoMonitors.Count == 0)
                    {
                        _AllInfoMonitors = GetMonitors().Result;//_DisplayPlugin.GetMonitors();
                    }
                    get_VCP_R.cid = get_VCP.cid;
                    if (_AllInfoMonitors.Count != 0)
                    {
                        b_vcpcode = Convert.ToByte(get_VCP.VcpCode.ToString());
                        objGetVCP = _VcpCorePlugin.GetVCPCapability(_AllInfoMonitors[get_VCP.MonitorIndex], b_vcpcode, opt: 0).Result;

                        get_VCP_R.MonitorIndex = get_VCP.MonitorIndex;
                        get_VCP_R.VcpCode = get_VCP.VcpCode;
                        if (objGetVCP.result)
                        {
                            get_VCP_R.Success = true;
                            get_VCP_R.Value = (int)(uint)objGetVCP.value;
                            get_VCP_R.UpdateChecksum();
                            if (get_VCP_R.ToJson() != string.Empty)
                            {
                                WriteAsync(get_VCP_R.ToJson()).Wait();
                            }
                            return Task.CompletedTask;
                        }
                        else
                        {
                            _logs.DebugMsg("[NetworkKVM] get VCP code Fail....");
                            get_VCP_R.Success = false;
                            get_VCP_R.Value = 0;
                            get_VCP_R.UpdateChecksum();
                            if (get_VCP_R.ToJson() != string.Empty)
                            {
                                WriteAsync(get_VCP_R.ToJson()).Wait();
                            }
                            return Task.CompletedTask;
                        }
                    }
                    else
                    {
                        _logs.DebugMsg("[NetworkKVM] No Monitor....");
                    }
                }
                else
                {
                    _logs.DebugMsg("[NetworkKVM] Checksum Fail....");
                }
            }
            else
            {
                _logs.DebugMsg("[NetworkKVM] Not GET_VCP....");
            }
            get_VCP_R.MonitorIndex = -1;
            get_VCP_R.VcpCode = -1;
            get_VCP_R.Success = false;
            get_VCP_R.Value = 0;
            get_VCP_R.UpdateChecksum();
            if (get_VCP_R.ToJson() != string.Empty)
            {
                WriteAsync(get_VCP_R.ToJson()).Wait();
            }
            return Task.CompletedTask;
        }

        private Task GetMonitorInfo(string jsonstring)
        {
            _logs.DebugMsg("[NetworkKVM] GetMonitorInfo....");
            GET_MONITOR_INFO get_MONITOR_INFO = new GET_MONITOR_INFO();
            GET_MONITOR_INFO_RESPONSE get_MONITORINFO_R = new GET_MONITOR_INFO_RESPONSE();
            get_MONITOR_INFO = JsonConvert.DeserializeObject<GET_MONITOR_INFO>(jsonstring);
            get_MONITORINFO_R.Monitors = new List<DdpmJsonCommon.Monitor>();
            if (get_MONITOR_INFO != null)
            {
                if (get_MONITOR_INFO.IsChecksumValid())
                {
                    DdpmJsonCommon.Monitor get_MonitorInfo = new DdpmJsonCommon.Monitor();
                    _AllInfoMonitors = GetMonitors().Result;
                    get_MONITORINFO_R.cid = get_MONITOR_INFO.cid;
                    get_MONITORINFO_R.Success = true;
                    if (_AllInfoMonitors.Count != 0)
                    {
                        foreach (var item in _AllInfoMonitors)
                        {
                            if (IsDisposed)
                            {
                                break;
                            }
                            if (string.IsNullOrEmpty(item.CapabilityString))
                            {
                                NeedUpdateMonitorInfo needUpdate = new NeedUpdateMonitorInfo();
                                needUpdate.Model = item.modelName;
                                needUpdate.seviceTag = item.edid.ServiceTag;
                                _logs.DebugMsg($"[NetworkKVM] GetMonitorInfo CapabilityString is empty : {item.modelName.ToString()}, {item.edid.ServiceTag.ToString()}.");
                                updateMonitorInfos.Add(needUpdate);
                                //    isMonitorUpdate = true;
                                //    jsonstring_MonitorUpdate = jsonstring;
                                //    _logs.DebugMsg("[NetworkKVM] GetMonitorInfo isMonitorUpdate : " + isMonitorUpdate.ToString());
                                //    _logs.DebugMsg("[NetworkKVM] GetMonitorInfo jsonstring_MonitorUpdate : " + jsonstring_MonitorUpdate);
                                //return Task.CompletedTask;
                            }
                            get_MonitorInfo = new DdpmJsonCommon.Monitor();
                            string capability = string.Empty;
                            string capability_all = _VcpCorePlugin.GetVCPCapabilities(item).Result;
                            if (capability_all != null && capability_all != string.Empty)
                            {
                                JObject json_capability = JObject.Parse(capability_all);
                                capability = (string)json_capability["CapabilityString"];
                            }
                            //Console.WriteLine("Capability Length : " + capability_all.Length.ToString());
                            get_MonitorInfo.Index = item.Index;
                            get_MonitorInfo.ModelName = item.modelName;
                            get_MonitorInfo.SerialNumber = item.edid.SerialNumber;
                            get_MonitorInfo.ServiceTag = item.edid.ServiceTag;
                            get_MonitorInfo.DeviceName = item.DisplayName;
                            get_MonitorInfo.CapabilityString = capability;
                            get_MonitorInfo.LimitedSW = !item.DDCisON;//0708 fix data to invert value
                            get_MonitorInfo.iMST = false;
                            get_MONITORINFO_R.Monitors.Add(get_MonitorInfo);
                        }
                        get_MONITORINFO_R.UpdateChecksum();
                        if (get_MONITORINFO_R.ToJson() != string.Empty)
                        {
                            _ = WriteAsync(get_MONITORINFO_R.ToJson());
                            //isMonitorUpdate = false;
                            //_logs.DebugMsg("[NetworkKVM] GetMonitorInfo isMonitorUpdate : " + isMonitorUpdate.ToString());
                            //jsonstring_MonitorUpdate = string.Empty;
                        }
                        return Task.CompletedTask;
                    }
                    else
                    {
                        _logs.DebugMsg("[NetworkKVM] No Monitor....");
                    }
                }
                else
                {
                    _logs.DebugMsg("[NetworkKVM] Checksum Fail....");
                    get_MONITORINFO_R.Success = false;
                }
            }
            else
            {
                _logs.DebugMsg("[NetworkKVM] Not GET_MONITOR_INFO....");
                get_MONITORINFO_R.Success = false;
            }
            get_MONITORINFO_R.Monitors = null;
            get_MONITORINFO_R.UpdateChecksum();
            if (get_MONITORINFO_R.ToJson() != string.Empty)
            {
                _ = WriteAsync(get_MONITORINFO_R.ToJson());
            }
            return Task.CompletedTask;
        }

        private Task GetCurrentMonitorIndex(string jsonstring)
        {
            _logs.DebugMsg("[NetworkKVM] GetCurrentMonitorIndex....");
            GET_CURRENT_MONITOR_INDEX_RESPONSE get_CURRENT_MONITOR_INDEX_R = new GET_CURRENT_MONITOR_INDEX_RESPONSE();
            GET_CURRENT_MONITOR_INDEX get_CURRENT_MONITOR_INDEX = new GET_CURRENT_MONITOR_INDEX();
            get_CURRENT_MONITOR_INDEX = JsonConvert.DeserializeObject<GET_CURRENT_MONITOR_INDEX>(jsonstring);
            if (get_CURRENT_MONITOR_INDEX != null)
            {
                if (get_CURRENT_MONITOR_INDEX.IsChecksumValid())
                {
                    MonitorInfo monitorInfo = new MonitorInfo();
                    if (_AllInfoMonitors == null || _AllInfoMonitors.Count == 0)
                    {
                        _AllInfoMonitors = GetMonitors().Result;//_DisplayPlugin.GetMonitors();
                    }
                    get_CURRENT_MONITOR_INDEX_R.cid = get_CURRENT_MONITOR_INDEX.cid;
                    //get_CURRENT_MONITOR_INDEX_R.type = (string)json["type"] + "_RESPONSE";
                    if (_AllInfoMonitors.Count != 0)
                    {
                        monitorInfo = _AllInfoMonitors.Find(x => (x.Index == 0));
                        get_CURRENT_MONITOR_INDEX_R.Success = true;
                        get_CURRENT_MONITOR_INDEX_R.MonitorIndex = monitorInfo.Index;
                        get_CURRENT_MONITOR_INDEX_R.UpdateChecksum();
                        if (get_CURRENT_MONITOR_INDEX_R.ToJson() != string.Empty)
                        {
                            _ = WriteAsync(get_CURRENT_MONITOR_INDEX_R.ToJson());
                        }
                        return Task.CompletedTask;
                    }
                    else
                    {
                        _logs.DebugMsg("[NetworkKVM] No Monitor....");
                    }
                }
                else
                {
                    _logs.DebugMsg("[NetworkKVM] Checksum Fail....");
                }
            }
            else
            {
                _logs.DebugMsg("[NetworkKVM] Not GET_CURRENT_MONITOR_INDEX....");
            }
            get_CURRENT_MONITOR_INDEX_R.Success = false;
            get_CURRENT_MONITOR_INDEX_R.MonitorIndex = -1;
            get_CURRENT_MONITOR_INDEX_R.UpdateChecksum();
            if (get_CURRENT_MONITOR_INDEX_R.ToJson() != string.Empty)
            {
                _ = WriteAsync(get_CURRENT_MONITOR_INDEX_R.ToJson());
            }
            return Task.CompletedTask;
        }
        /*
        private async Task<string> DisconnectNamedPipe()
        {
            DISCONNECT _COMMAND = new DISCONNECT();
            _COMMAND.UpdateChecksum();

            return _COMMAND.ToJson();
        }

        private Task NotFindType(JObject json)
        {
            _logs.DebugMsg("[NetworkKVM] NotFindType....");
            DDM_RESPONSE _RESPONSE = new DDM_RESPONSE();
            _RESPONSE.Success = false;
            _RESPONSE.cid = (int)json["cid"];
            _RESPONSE.UpdateChecksum();
            //_RESPONSE.type = (string)json["type"] + "_RESPONSE";
            if (_RESPONSE.ToJson() != string.Empty)
            {
                _ = WriteAsync(_RESPONSE.ToJson());
            }
            return Task.CompletedTask;
        }*/

        private async Task<bool> ResponseSucces(JObject json)
        {
            if ((bool)json["Success"])
            {
                return true;
            }
            return false;
        }

        //private Task ResponseSupportedMonitor()
        //{
        //    _logs.DebugMsg("[NetworkKVM] ResponseSupportedMonitor....");
        //    if (_SupportedMonitors != null && _SupportedMonitors.Count > 0)
        //    {
        //        cid = cid + 1;
        //        UPDATE_SUPPORTED_MONITOR_LIST SUPPORTED_MONITOR_LIST = new UPDATE_SUPPORTED_MONITOR_LIST();
        //        SUPPORTED_MONITOR_LIST.cid = cid;
        //        //SUPPORTED_MONITOR_LIST.type = "UPDATE_SUPPORTED_MONITOR_LIST";
        //        SUPPORTED_MONITOR_LIST.Monitors = _SupportedMonitors;
        //        SUPPORTED_MONITOR_LIST.UpdateChecksum();
        //        if (SUPPORTED_MONITOR_LIST.ToJson() != string.Empty)
        //        {
        //            WriteAsync(SUPPORTED_MONITOR_LIST.ToJson()).Wait();
        //        }
        //    }
        //    else
        //    {
        //        OnNKVM().Wait();
        //    }
        //    return Task.CompletedTask;
        //}

        private bool IsSupportNKVM(string s)
        {
            try
            {
                if (!string.IsNullOrEmpty(s))
                {
                    if (s == "" || s.Length < 10)
                    {
                        return false;
                    }
                    string[] ss = s.Split("C6(");

                    if (ss.Length >= 2)
                    {
                        ss = ss[1].Split(")");
                        _logs.DebugMsg("[NetworkKVM] C6 string : " + ss[0]);
                        ss = ss[0].Split(" ");
                        for (int i = 0; i < ss.Length; i++)
                        {
                            if (IsDisposed)
                            {
                                break;
                            }
                            _logs.DebugMsg("[NetworkKVM] C6 number : " + ss[i]);
                            if (ss[i].Equals("01"))
                            {
                                _logs.DebugMsg("[NetworkKVM] C6 number is 01.");
                                return (true);
                            }
                        }
                        return (false);
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                    return (false);
            }
            catch
            {
                return (false);
            }
        }

        private Task isHotkeyAvailable(string jsonstring)
        {
            _logs.DebugMsg("[NetworkKVM] isHotkeyAvailable....");
            IS_HOTKEY_AVAILABLE_RESPONSE is_HOTKEY_AVAILABLE_RESPONSE = new IS_HOTKEY_AVAILABLE_RESPONSE();
            IS_HOTKEY_AVAILABLE is_HOTKEY_AVAILABLE = new IS_HOTKEY_AVAILABLE();
            is_HOTKEY_AVAILABLE = JsonConvert.DeserializeObject<IS_HOTKEY_AVAILABLE>(jsonstring);
            if (is_HOTKEY_AVAILABLE != null)
            {
                if (is_HOTKEY_AVAILABLE.IsChecksumValid())
                {
                    is_HOTKEY_AVAILABLE_RESPONSE.cid = is_HOTKEY_AVAILABLE.cid;
                    HotkeyWinform jsonHotkey = is_HOTKEY_AVAILABLE.Hotkey;
                    is_HOTKEY_AVAILABLE_RESPONSE.Hotkey = jsonHotkey;
                    if (_HotkeySettings != null)
                    {
                        _logs.DebugMsg("[NetworkKVM] _HotkeySettings not null");
                        if (_HotkeySettings.Count > 0)
                        {
                            foreach (HotkeySettings hotkeySettings in _HotkeySettings)
                            {
                                if (IsDisposed)
                                {
                                    break;
                                }
                                foreach (HotkeyInfo hotkeyInfo in hotkeySettings.HotkeyInfo)
                                {
                                    if (IsDisposed)
                                    {
                                        break;
                                    }
                                    if (jsonHotkey.Control == hotkeyInfo.Hotkey.Exists(x => x == VirtualKey.Control) &&
                                        jsonHotkey.Alt == hotkeyInfo.Hotkey.Exists(x => x == VirtualKey.Menu) &&
                                        jsonHotkey.Shift == hotkeyInfo.Hotkey.Exists(x => x == VirtualKey.Shift))
                                    {
                                        _logs.DebugMsg("[NetworkKVM] isHotkeyAvailable false");
                                        //VirtualKey thisVirtualKey_system = (VirtualKey)KeyInterop.VirtualKeyFromKey((Key)jsonHotkey.Key);
                                        int index = hotkeyInfo.Hotkey.FindIndex(x => (x == (VirtualKey)jsonHotkey.Key));
                                        if (index != -1)
                                        {
                                            is_HOTKEY_AVAILABLE_RESPONSE.Available = false;
                                            is_HOTKEY_AVAILABLE_RESPONSE.Success = true;
                                            is_HOTKEY_AVAILABLE_RESPONSE.UpdateChecksum();
                                            if (is_HOTKEY_AVAILABLE_RESPONSE.ToJson() != string.Empty)
                                            {
                                                _ = WriteAsync(is_HOTKEY_AVAILABLE_RESPONSE.ToJson());
                                            }
                                            return Task.CompletedTask;
                                        }
                                    }
                                }
                            }
                            _logs.DebugMsg("[NetworkKVM] isHotkeyAvailable true");
                            is_HOTKEY_AVAILABLE_RESPONSE.Available = true;
                            is_HOTKEY_AVAILABLE_RESPONSE.Success = true;
                            is_HOTKEY_AVAILABLE_RESPONSE.UpdateChecksum();
                            if (is_HOTKEY_AVAILABLE_RESPONSE.ToJson() != string.Empty)
                            {
                                _ = WriteAsync(is_HOTKEY_AVAILABLE_RESPONSE.ToJson());
                            }
                            return Task.CompletedTask;
                        }
                        else
                        {
                            _logs.DebugMsg("[NetworkKVM] isHotkeyAvailable true");
                            is_HOTKEY_AVAILABLE_RESPONSE.Available = true;
                            is_HOTKEY_AVAILABLE_RESPONSE.Success = true;
                            is_HOTKEY_AVAILABLE_RESPONSE.UpdateChecksum();
                            if (is_HOTKEY_AVAILABLE_RESPONSE.ToJson() != string.Empty)
                            {
                                _ = WriteAsync(is_HOTKEY_AVAILABLE_RESPONSE.ToJson());
                            }
                            return Task.CompletedTask;
                        }
                    }
                    else
                    {
                        _logs.DebugMsg("[NetworkKVM] _HotkeySettings is null....");
                    }
                }
                else
                {
                    _logs.DebugMsg("[NetworkKVM] Checksum Fail....");
                }
            }
            else
            {
                _logs.DebugMsg("[NetworkKVM] Not IS_HOTKEY_AVAILABLE....");
            }
            is_HOTKEY_AVAILABLE_RESPONSE.Available = false;
            is_HOTKEY_AVAILABLE_RESPONSE.Success = false;
            is_HOTKEY_AVAILABLE_RESPONSE.UpdateChecksum();
            if (is_HOTKEY_AVAILABLE_RESPONSE.ToJson() != string.Empty)
            {
                _ = WriteAsync(is_HOTKEY_AVAILABLE_RESPONSE.ToJson());
            }
            return Task.CompletedTask;
        }
        /// <summary>
        /// NKVMtoDDPM SetHotkey
        /// </summary>
        /// <param name="jsonstring"></param>
        /// <returns></returns>
        private Task GetSetHotkey(string jsonstring)
        {
            _logs.DebugMsg("[NetworkKVM] GetSendHotkey....");
            SET_HOTKEY set_HOTKEY = new SET_HOTKEY();
            set_HOTKEY = JsonConvert.DeserializeObject<SET_HOTKEY>(jsonstring);
            SET_HOTKEY_RESPONSE set_HOTKEY_RESPONSE = new SET_HOTKEY_RESPONSE();
            if (set_HOTKEY != null)
            {
                if (set_HOTKEY.IsChecksumValid())
                {
                    _logs.DebugMsg("[NetworkKVM] _HotkeySettings not null");
                    NKVMSetHotkey nKVMSetHotkey = new NKVMSetHotkey();
                    HotkeyInfo hotkeyInfo = new HotkeyInfo();
                    hotkeyInfo.Hotkey = new List<VirtualKey>();
                    if (set_HOTKEY.Hotkey.Control)
                    {
                        hotkeyInfo.Hotkey.Add(VirtualKey.Control);
                    }
                    if (set_HOTKEY.Hotkey.Alt)
                    {
                        hotkeyInfo.Hotkey.Add(VirtualKey.Menu);
                    }
                    if (set_HOTKEY.Hotkey.Shift)
                    {
                        hotkeyInfo.Hotkey.Add(VirtualKey.Shift);
                    }
                    hotkeyInfo.Hotkey.Add((VirtualKey)set_HOTKEY.Hotkey.Key);
                    hotkeyInfo.Job = HotkeyType.NkvmConflict;
                    nKVMSetHotkey.HotkeyInfo = hotkeyInfo;
                    nKVMSetHotkey.jsonstring = jsonstring;
                    SetDDPMHotkey(nKVMSetHotkey);
                    return Task.CompletedTask;
                }
                else
                {
                    _logs.DebugMsg("[NetworkKVM] Checksum Fail....");
                }
                set_HOTKEY_RESPONSE.cid = set_HOTKEY.cid;
                set_HOTKEY_RESPONSE.Hotkey = set_HOTKEY.Hotkey;
            }
            else
            {
                _logs.DebugMsg("[NetworkKVM] Not SET_HOTKEY");
            }
            set_HOTKEY_RESPONSE.Success = false;
            set_HOTKEY_RESPONSE.UpdateChecksum();
            if (set_HOTKEY_RESPONSE.ToJson() != string.Empty)
            {
                _ = WriteAsync(set_HOTKEY_RESPONSE.ToJson());
            }
            return Task.CompletedTask;
        }

        private void SendChangeLimitedSW(MonitorInfo monitorInfo, bool isON)
        {
            CHANGE_LIMITED_SW chanage_LIMITED_SW = new CHANGE_LIMITED_SW();
            chanage_LIMITED_SW.MonitorIndex = monitorInfo.Index;
            if (isON)
            {
                _logs.DebugMsg("[NetworkKVM] SendChangeLimitedSW true");
                chanage_LIMITED_SW.LimitedSW = true;
            }
            else
            {
                _logs.DebugMsg("[NetworkKVM] SendChangeLimitedSW false");
                chanage_LIMITED_SW.LimitedSW = false;
            }
            chanage_LIMITED_SW.UpdateChecksum();

            WriteAsync(chanage_LIMITED_SW.ToJson()).Wait();
        }

        private void GetVersionResponse(string jsonstring)
        {
            GET_NKVM_VERSION_RESPONSE get_NKVM_VERSION_RESPONSE = new GET_NKVM_VERSION_RESPONSE();
            get_NKVM_VERSION_RESPONSE = JsonConvert.DeserializeObject<GET_NKVM_VERSION_RESPONSE>(jsonstring);
            if (get_NKVM_VERSION_RESPONSE != null)
            {
                NKVMRespone CLIrespone = new NKVMRespone();
                CLIrespone.CLIName = "NetworkKVMVersion";
                CLIrespone.Respone = get_NKVM_VERSION_RESPONSE.Version;
                ToNKVMCLI(CLIrespone);
            }
            else
            {
                _logs.DebugMsg("[NetworkKVM] Not GET_NKVM_VERSION_RESPONSE....");
            }
        }

        private void GetStatusResponse(string jsonstring)
        {
            GET_NKVM_STATUS_RESPONSE get_NKVM_STATUS_R = new GET_NKVM_STATUS_RESPONSE();
            get_NKVM_STATUS_R = JsonConvert.DeserializeObject<GET_NKVM_STATUS_RESPONSE>(jsonstring);
            if (get_NKVM_STATUS_R != null)
            {
                NKVMRespone CLIrespone = new NKVMRespone();
                CLIrespone.CLIName = "NetworkKVMStatus";
                CLIrespone.Respone = get_NKVM_STATUS_R.Status;
                ToNKVMCLI(CLIrespone);
            }
            else
            {
                _logs.DebugMsg("[NetworkKVM] Not GET_NKVM_STATUS_RESPONSE....");
            }
        }

        private void GetAutoConnectResponse(string jsonstring)
        {
            GET_NKVM_AUTO_CONNECT_RESPONSE get_NKVM_AUTO_CONNECT_R = new GET_NKVM_AUTO_CONNECT_RESPONSE();
            get_NKVM_AUTO_CONNECT_R = JsonConvert.DeserializeObject<GET_NKVM_AUTO_CONNECT_RESPONSE>(jsonstring);
            if (get_NKVM_AUTO_CONNECT_R != null)
            {
                NKVMRespone CLIrespone = new NKVMRespone();
                CLIrespone.CLIName = "NetworkKVMAutoConnect";
                CLIrespone.Respone = get_NKVM_AUTO_CONNECT_R.Enable.ToString();
                ToNKVMCLI(CLIrespone);
            }
            else
            {
                _logs.DebugMsg("[NetworkKVM] Not GET_NKVM_AUTO_CONNECT_RESPONSE....");
            }
        }

        private void GetContentTransferResponse(string jsonstring)
        {
            GET_NKVM_CONTENT_TRANSFER_RESPONSE get_NKVM_CONTENT_TRANSFER_R = new GET_NKVM_CONTENT_TRANSFER_RESPONSE();
            get_NKVM_CONTENT_TRANSFER_R = JsonConvert.DeserializeObject<GET_NKVM_CONTENT_TRANSFER_RESPONSE>(jsonstring);
            if (get_NKVM_CONTENT_TRANSFER_R != null)
            {
                NKVMRespone CLIrespone = new NKVMRespone();
                CLIrespone.CLIName = "NetworkKVMContentTransfer";
                CLIrespone.Respone = get_NKVM_CONTENT_TRANSFER_R.Enable.ToString();
                ToNKVMCLI(CLIrespone);
            }
            else
            {
                _logs.DebugMsg("[NetworkKVM] Not GET_NKVM_CONTENT_TRANSFER_RESPONSE....");
            }
        }

        private void GetIncommingPortResponse(string jsonstring)
        {
            GET_NKVM_INCOMMING_PORT_RESPONSE get_NKVM_INCOMMING_R = new GET_NKVM_INCOMMING_PORT_RESPONSE();
            get_NKVM_INCOMMING_R = JsonConvert.DeserializeObject<GET_NKVM_INCOMMING_PORT_RESPONSE>(jsonstring);
            if (get_NKVM_INCOMMING_R != null)
            {
                NKVMRespone CLIrespone = new NKVMRespone();
                CLIrespone.CLIName = "GetNetworkKVMIncomingPort";
                CLIrespone.Respone = get_NKVM_INCOMMING_R.Port.ToString();
                ToNKVMCLI(CLIrespone);
            }
            else
            {
                _logs.DebugMsg("[NetworkKVM] Not GET_NKVM_INCOMMING_PORT_RESPONSE....");
            }
        }

        private void GetOutgoingPortResponse(string jsonstring)
        {
            GET_NKVM_OUTGOING_PORT_RESPONSE get_NKVM_OUTGOING_PORT_R = new GET_NKVM_OUTGOING_PORT_RESPONSE();
            get_NKVM_OUTGOING_PORT_R = JsonConvert.DeserializeObject<GET_NKVM_OUTGOING_PORT_RESPONSE>(jsonstring);
            if (get_NKVM_OUTGOING_PORT_R != null)
            {
                NKVMRespone CLIrespone = new NKVMRespone();
                CLIrespone.CLIName = "GetNetworkKVMOutgoingPort";
                CLIrespone.Respone = get_NKVM_OUTGOING_PORT_R.Port.ToString();
                ToNKVMCLI(CLIrespone);
            }
            else
            {
                _logs.DebugMsg("[NetworkKVM] Not GET_NKVM_OUTGOING_PORT_RESPONSE....");
            }
        }

        private void GetContentTransfedPortResponse(string jsonstring)
        {
            GET_NKVM_CONTENT_TRANSFER_PORT_RESPONSE get_NKVM_CONTENT_TRANSFER_R = new GET_NKVM_CONTENT_TRANSFER_PORT_RESPONSE();
            get_NKVM_CONTENT_TRANSFER_R = JsonConvert.DeserializeObject<GET_NKVM_CONTENT_TRANSFER_PORT_RESPONSE>(jsonstring);
            if (get_NKVM_CONTENT_TRANSFER_R != null)
            {
                NKVMRespone CLIrespone = new NKVMRespone();
                CLIrespone.CLIName = "GetNetworkKVMContentTransferPort";
                CLIrespone.Respone = get_NKVM_CONTENT_TRANSFER_R.Port.ToString();
                ToNKVMCLI(CLIrespone);
            }
            else
            {
                _logs.DebugMsg("[NetworkKVM] Not GET_NKVM_CONTENT_TRANSFER_PORT_RESPONSE....");
            }
        }

        private Task UpdateMonitorInfo(MonitorInfo monitorInfo)
        {
            if (pipeServer != null && pipeServer.IsConnected)
            {
                _logs.DebugMsg("[NetworkKVM] UpdateMonitorInfo....");
                if (updateMonitorInfos != null && updateMonitorInfos.Count > 0)
                {
                    int index = updateMonitorInfos.FindIndex(x => x.Model == monitorInfo.modelName &&
                                                            x.seviceTag == monitorInfo.edid.ServiceTag);
                    if (index != -1)
                    {
                        _logs.DebugMsg($"[NetworkKVM][UpdateMonitorInfo] Find updateMonitorInfo count is {index.ToString()}.");
                        //send MonitorPlug to NKVM
                        MonitorPlug();
                        //remove
                        updateMonitorInfos.RemoveAt(index);
                    }
                }
                else
                {
                    _logs.DebugMsg("[NetworkKVM][UpdateMonitorInfo] updateMonitorInfos is null or count = 0.");
                }
            }
            return Task.CompletedTask;
        }

        #endregion Private Methods

        #region IDisposableObservable Support

        /// <summary>
        /// To detect redundant calls
        /// </summary>
        public bool IsDisposed { get; private set; }

        /// <summary>
        /// Override for Dispose
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                _logs.DebugMsg("[NetworkKVM] into Dispose ～～～～～～～～～～～～～～～～～！！！！！！！");

                IsDisposed = true;

                if (disposing)
                {
                    DisposeAction();

                    _agent.PluginManager.PluginsStarted -= PluginManagerOnPluginsStarted;
                    _agent = null;
                    if (_VcpCorePlugin is IFrameworkPluginConditionNotification VcpCoreCondition)
                    {
                        VcpCoreCondition.PluginConditionChangeHandler -= OnVcpCorePluginConditionChangeHandler;
                    }
                    _VcpCorePlugin.VCPchanged -= VCPchangedEvent;
                    _VcpCorePlugin.MonitorinfoUpdated -= MonitorUpdateEvent;
                }
            }

            base.Dispose(disposing);
        }

        private void DisposeAction()
        {
            Disconnect();
        }

        #endregion

        #region Event Handler

        public delegate void NKVMPluginEventHandler(object sender, EventArgsjson eventArgsjson);

        public event NKVMPluginEventHandler NKVMPluginEvent;

        public event EventHandler<NKVMRespone> NKVMCLIEvent;

        public event EventHandler<NKVMSetHotkey> NKVMSetHotkey;

        public event EventHandler<NKVMSetVCP> NKVMSetVCPEvent;

        public class EventArgsjson : EventArgs
        {
            public EventArgsjson(string jsonstring)
            {
                jsonString = jsonstring;
            }

            public string jsonString { get; set; }
        }

        private void OnVcpCorePluginConditionChangeHandler(object sender, EventArgs e)
        {
            GetCurrentVcpCoreCondition();
        }

        private void PluginManagerOnPluginsStarted(object sender, PluginsStartedEventArgs e)
        {
            if (e == null)
                return;
            if (e.ChangedPlugins == null)
                return;
            if (e.ChangedPlugins.Any() == false)
                return;
            return;
        }

        private void PluginsStarted(object sender, PluginsStartedEventArgs e)
        {
            if (e?.ChangedPlugins == null)
                return;
            if (e.ChangedPlugins.Any() == false)
                return;
        }

        public Task<bool> GetOnNKVM(MonitorInfo monitorInfo, ISettingsManagerDev _SettingsPlugin)
        {
            _logs.DebugMsg("[GetOnNKVM] GetOnNKVM");
            DDPMSettings config = _SettingsPlugin.ReloadAppConfigData().Result;
            if (config != null)
            {
                if (config.LockSettings.Enable_Display_NetworkKVM)
                {
                    _logs.DebugMsg("[GetOnNKVM] Enable_Display_NetworkKVM is true");
                    return Task.FromResult(true);
                }
            }
            else
            {
                _logs.DebugMsg("[GetOnNKVM] DDPMSettings is null");
            }
            List<DDPMMonitorSettings> settings = _SettingsPlugin.ReloadMonitorSettings(monitorInfo.modelName).Result;
            if (settings != null)
            {
                DDPMMonitorSettings monitorSetting = settings.Find(x => x.ServiceTag == monitorInfo.edid.ServiceTag);
                if (monitorSetting != null)
                {
                    return Task.FromResult(monitorSetting.KVM.isOnNKVM);
                }
            }
            return Task.FromResult(false);
        }

        public Task SetOnNKVM(MonitorInfo monitorInfo, bool ison, ISettingsManagerDev _SettingsPlugin)
        {
            _logs.DebugMsg("[SetOnNKVM] SetOnNKVM ison : " + ison.ToString());
            List<DDPMMonitorSettings> settings = _SettingsPlugin.ReloadMonitorSettings(monitorInfo.modelName).Result;
            if (settings != null) //Robert_Lin 0731
            {
                foreach (DDPMMonitorSettings setting in settings)
                {
                    if (IsDisposed)
                    {
                        break;
                    }
                    if (setting != null &&
                        setting.ServiceTag == monitorInfo.edid.ServiceTag)
                    {
                        setting.KVM.isOnNKVM = ison;
                        bool b = _SettingsPlugin.WriteMonitorSettings(monitorInfo.modelName, settings).Result;
                        if (ison)
                        {
                            //_SupportedMonitorList = _NKVMPlugin.GetSupportedNKVM().Result;
                            OnNKVM().Wait();
                            //bool bt = SentKVMtoTelementry(monitorInfo, "KVMMode", "Network").Result;
                        }
                        else
                        {
                            OffNKVM().Wait();
                            DDPMSettings config = _SettingsPlugin.ReloadAppConfigData().Result;
                            if (config != null)
                            {
                                config.LockSettings.Enable_Display_NetworkKVM = false;
                            }
                        }
                        break;
                    }
                }
            }

            return Task.CompletedTask;
        }

        private void VCPchangedEvent(object sender, VCPchangedEventArgs e)
        {
            _logs.DebugMsg("[NetworkKVM] VCPchangedEvent.....");
            _logs.DebugMsg("[NetworkKVM] VCPchanged " + e.vcpcode);
            _logs.DebugMsg("[NetworkKVM] VCPcode value " + e.value);
            if (e.vcpcode.Equals("input select") || e.vcpcode.Equals("E8") || e.vcpcode.Equals("E9") || e.vcpcode.Equals("E5") || e.vcpcode.Equals("04"))
            {
                try
                {
                    int vcpcode = 0;
                    int value = 0;
                    int value_60 = 0;
                    int value_E8 = 0;
                    //int value_E9 = 0;
                    ObjGetVCP objGetVCP_60 = new ObjGetVCP();
                    ObjGetVCP objGetVCP_E8 = new ObjGetVCP();
                    ObjGetVCP objGetVCP_E9 = new ObjGetVCP();
                    if (e.vcpcode.Equals("input select"))
                    {
                        //vcpcode = 96;
                        objGetVCP_60 = _VcpCorePlugin.GetVCPCapability(e.monitor, 0x60).Result;
                        if (objGetVCP_60 != null && objGetVCP_60.result)
                        {
                            value_60 = (int)(uint)objGetVCP_60.value;
                            _logs.DebugMsg("[NetworkKVM] VCPcode 60 value " + (int)(uint)value_60);
                            SetVCPNotify(e.monitor, 0x60, value_60);
                        }
                        //objGetVCP_E8 = _VcpCorePlugin.GetVCPCapability(e.monitor, 0xE8).Result;
                        //if (objGetVCP_E8 != null && objGetVCP_E8.result)
                        //{
                        //    value_E8 = (int)(uint)objGetVCP_E8.value;
                        //    _logs.DebugMsg("[NetworkKVM] VCPcode E8 value " + value_E8);
                        //    SetVCPNotify(e.monitor, 0xE8, value_E8);
                        //}
                        //objGetVCP_E9 = _VcpCorePlugin.GetVCPCapability(e.monitor, 0xE9).Result;
                        //if (objGetVCP_E9 != null && objGetVCP_E9.result)
                        //{
                        //    value_E9 = (int)(uint)objGetVCP_E9.value;
                        //    _logs.DebugMsg("[NetworkKVM] VCPcode E9 value " + value_E9);
                        //    SetVCPNotify(e.monitor, 0xE9, value_E9);
                        //}
                    }
                    else
                    {
                        vcpcode = Convert.ToInt32(e.vcpcode, 16);
                        _logs.DebugMsg("[NetworkKVM] VCPcode " + vcpcode);
                        value = Convert.ToInt32(e.value);
                        _logs.DebugMsg("[NetworkKVM] VCPcode value " + value);
                        if (e.vcpcode.Equals("E8"))
                        {
                            _logs.DebugMsg("[NetworkKVM] VCPcode E8 value " + value);
                            if (value != 0)
                            {
                                SetVCPNotify(e.monitor, vcpcode, value);
                            }
                            objGetVCP_60 = _VcpCorePlugin.GetVCPCapability(e.monitor, 0x60).Result;
                            if (objGetVCP_60 != null && objGetVCP_60.result)
                            {
                                value_60 = (int)(uint)objGetVCP_60.value;
                                _logs.DebugMsg("[NetworkKVM] VCPcode 60 value " + (int)(uint)value_60);
                                SetVCPNotify(e.monitor, 0x60, value_60);
                            }
                            //if (nKVMVCPValues != null && nKVMVCPValues.Count > 0)
                            //{
                            //    NKVMVCPValue nKVMVCPValue = nKVMVCPValues.Find(x => x.monitorInfo == e.monitor);
                            //    if (nKVMVCPValue != null)
                            //    {
                            objGetVCP_E9 = _VcpCorePlugin.GetVCPCapability(e.monitor, 0xE9).Result;
                            if (objGetVCP_E9 != null && objGetVCP_E9.result)
                            {
                                _logs.DebugMsg("[NetworkKVM] MonitorPlug E9 : " + (int)(uint)objGetVCP_E9.value);
                                //_logs.DebugMsg("[NetworkKVM] MonitorPlug nKVMVCPValue : " + nKVMVCPValue.value);
                                //if (nKVMVCPValue.value != (int)(uint)objGetVCP_E9.value)
                                //{
                                //nKVMVCPValue.value = (int)(uint)objGetVCP_E9.value;
                                SetVCPNotify(e.monitor, 0xE9, (int)(uint)objGetVCP_E9.value);
                                //}
                            }
                            //    }
                            //}
                        }
                        else if (e.vcpcode.Equals("E9"))
                        {
                            objGetVCP_60 = _VcpCorePlugin.GetVCPCapability(e.monitor, 0x60).Result;
                            if (objGetVCP_60 != null && objGetVCP_60.result)
                            {
                                value_60 = (int)(uint)objGetVCP_60.value;
                                _logs.DebugMsg("[NetworkKVM] VCPcode 60 value " + value_60);
                                SetVCPNotify(e.monitor, 0x60, value_60);
                            }
                            objGetVCP_E8 = _VcpCorePlugin.GetVCPCapability(e.monitor, 0xE8).Result;
                            if (objGetVCP_E8 != null && objGetVCP_E8.result)
                            {
                                value_E8 = (int)(uint)objGetVCP_E8.value;
                                _logs.DebugMsg("[NetworkKVM] VCPcode E8 value " + value_E8);
                                if (value_E8 != 0)
                                {
                                    SetVCPNotify(e.monitor, 0xE8, value_E8);
                                }
                            }
                            //if (nKVMVCPValues != null && nKVMVCPValues.Count > 0)
                            //{
                            //    NKVMVCPValue nKVMVCPValue = nKVMVCPValues.Find(x => x.monitorInfo == e.monitor);
                            //    if (nKVMVCPValue != null)
                            //    {
                            objGetVCP_E9 = _VcpCorePlugin.GetVCPCapability(e.monitor, 0xE9).Result;
                            if (objGetVCP_E9 != null && objGetVCP_E9.result)
                            {
                                _logs.DebugMsg("[NetworkKVM] MonitorPlug E9 : " + (int)(uint)objGetVCP_E9.value);
                                //_logs.DebugMsg("[NetworkKVM] MonitorPlug nKVMVCPValue : " + nKVMVCPValue.value);
                                //if (nKVMVCPValue.value != (int)(uint)objGetVCP_E9.value)
                                //{
                                //nKVMVCPValue.value = (int)(uint)objGetVCP_E9.value;
                                SetVCPNotify(e.monitor, 0xE9, (int)(uint)objGetVCP_E9.value);
                                //}
                            }
                            //    }
                            //}
                        }
                        else
                        {
                            SetVCPNotify(e.monitor, vcpcode, value);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logs.DebugMsg("[NetworkKVM] VCPchanged exception : " + ex.ToString());
                }
            }
        }

        private void MonitorUpdateEvent(object sender, MonitorinfoUpdateEventArgs e)
        {
            _logs.DebugMsg("[NetworkKVM] MonitorUpdateEvent...");
            if (e.monitor != null && !string.IsNullOrEmpty(e.monitor.CapabilityString))
            {
                _logs.DebugMsg("[NetworkKVM][MonitorUpdateEvent] monitor info capabilityString isn't empty.");
                UpdateMonitorInfo(e.monitor).Wait();
            }
        }

        public void ToNKVMCLI(NKVMRespone response)
        {
            NKVMCLIEvent?.AsyncFireAndForget(this, response, System.Threading.CancellationToken.None);
        }

        public void SetDDPMHotkey(NKVMSetHotkey setHotkey)
        {
            _logs.DebugMsg("[NetworkKVM] Send SetDDPMHotkey Event");
            NKVMSetHotkey?.AsyncFireAndForget(this, setHotkey, System.Threading.CancellationToken.None);
        }

        public void SetVCPEvent(NKVMSetVCP nKVMSetVCP)
        {
            _logs.DebugMsg("[NetworkKVM] NKVMSetVCP Event");
            NKVMSetVCPEvent?.AsyncFireAndForget(this, nKVMSetVCP, System.Threading.CancellationToken.None);
        }

        #endregion Event Handler
    }
}