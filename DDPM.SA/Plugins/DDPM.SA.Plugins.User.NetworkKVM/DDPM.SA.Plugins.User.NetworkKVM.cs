using DDPM.SA.Common;
using DDPM.SA.Common.Display;
using DDPM.SA.Common.Security;
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
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VcpCore.Common;
using VcpCore.Interfaces;
using Windows.System;

namespace NetworkKVM.Plugins
{
    [Plugin(DDPM.SA.Common.IDs.DDPM_NKVM_PLUGIN_ID, pluginName, PluginOrderGroupType.Core, Version = pluginVersion)]
    [Descriptor(Description = pluginDescription)]
    [Publisher(Name = publisherCompany, Website = publisherWebsite, Support = publisherSupport)]
    [PublishedUnelevatedInterface(new[] { typeof(INKVMService) })]
    public class NKVMPlugin : BaseAgentPlugin, INKVMService
    {
        #region Private Members

        private const string pluginName = "NKVMPlugin";
        private const string pluginVersion = "1.0.0";
        private const string pluginDescription = "This plugin implements NKVM Plugin.";
        private const string publisherCompany = "Wistron";
        private const string publisherWebsite = "https://www.wistron.com";
        private const string publisherSupport = "This plugin implements NKVM Plugin.";

        private IAgent _agent;
        private Agent _Agent;
        private Logs _logs;
        public const string PluginLogId = "NKVM";
        private bool _runloop = true;
        private int cid = -1;

        private NamedPipeServerStream pipeServer;
        private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private IVcpCoreService _VcpCorePlugin;
        private List<MonitorInfo> _AllInfoMonitors = new List<MonitorInfo>();
        private List<string> _SupportedMonitors = new List<string>();
        private readonly object _pluginConditionLock = new object();
        private List<HotkeySettings> _HotkeySettings = new List<HotkeySettings>();
        private HotkeyInfo _HotkeyInfo = new HotkeyInfo();

        private object lock_wait = new object();
        private string response;
        private string command;
        private bool NKVMState = false;
        private string namedpipeName;

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
            _agent.PluginManager.PluginsStarted += PluginManagerOnPluginsStarted;

            InitializeVcpCorePlugin();

            PluginCondition = new PluginStartedCondition();
            _ = Task.Run(async () => await NamedPipeServer());
        }

        #endregion Overriding methods

        #region INKVM implementation
        public Task CreatNewNamedpipe()
        {
            Disconnect();
            CreateNamedPipe();

            return Task.CompletedTask;
        }

        public Task<bool> IsNamedpipeConnected()
        {
            bool b = false;
            try
            {
                if (pipeServer != null)
                {
                    b = pipeServer.IsConnected;
                }
            }
            catch
            {
                ;
            }
            return Task.FromResult(b);
        }

        /// <summary>
        /// This function is used to distinc the monitor add or remove
        /// </summary>
        /// <param name="monitorInfos">new coming MonitorInfo list, after check, replace to current object</param>
        /// <returns></returns>
        public Task UpdateMonitorInfo(List<MonitorInfo> monitorInfos)
        {
            if (monitorInfos == null || monitorInfos.Count == 0)
            {
                if (_AllInfoMonitors.Count == 0)
                    return Task.CompletedTask;//return directly, no change

                //means unplug all connected dell monitors
                //Call Func: OnMonitorUnPlug(_AllInfoMonitors);
                _SupportedMonitors = GetSupportedNKVM().Result;
                if (pipeServer.IsConnected)
                {
                    //ResponseSupportedMonitor();
                    MonitorPlug();
                }
                _AllInfoMonitors.Clear();
            }
            else
            {
                if (_AllInfoMonitors.Count == 0 && monitorInfos.Count > 0)
                {
                    //means plugin 1 or more monitor in
                    _logs.DebugMsg("[NetworkKVM] monitor 0 -> 1");
                    //Call Func: OnMonitorPlugIn(List<MonitorInfo> mos);
                    _SupportedMonitors = GetSupportedNKVM().Result;
                    _logs.DebugMsg("[NetworkKVM] NKVMState:" + NKVMState);
                    //if (NKVMState)
                    //{
                        if (pipeServer != null)
                        {
                            if (pipeServer.IsConnected)
                            {
                                ResponseSupportedMonitor();
                                MonitorPlug();
                            }
                        }
                    //}
                    _AllInfoMonitors.AddRange(monitorInfos);
                }
                else
                {
                    List<MonitorInfo> unplug = _AllInfoMonitors
                        .Where(x => !monitorInfos.Any(y => y.edid.ModelName == x.edid.ModelName &&
                                                            y.edid.SerialNumber == x.edid.SerialNumber &&
                                                            y.DisplayName == x.DisplayName))
                        .ToList();
                    if (unplug.Count > 0)//means unplug
                    {
                        //Call Func: OnMonitorUnPlug(unplug);
                        _SupportedMonitors = GetSupportedNKVM().Result;
                        if (pipeServer.IsConnected)
                        {
                            ResponseSupportedMonitor();
                            MonitorPlug();
                        }
                    }
                    List<MonitorInfo> plugin = monitorInfos
                        .Where(x => !_AllInfoMonitors.Any(y => y.edid.ModelName == x.edid.ModelName &&
                                                               y.edid.SerialNumber == x.edid.SerialNumber &&
                                                               y.DisplayName == x.DisplayName))
                        .ToList();
                    if (plugin.Count > 0)//means plugin
                    {
                        //Call Func: OnMonitorPlugIn(plugin);
                        _SupportedMonitors = GetSupportedNKVM().Result;
                        if (pipeServer.IsConnected)
                        {
                            ResponseSupportedMonitor();
                            MonitorPlug();
                        }
                    }

                    _AllInfoMonitors.Clear();
                    _AllInfoMonitors.AddRange(monitorInfos);
                }
            }
            return Task.CompletedTask;
        }

        public Task MonitorPlug()
        {
            _logs.DebugMsg("[NetworkKVM] MonitorPlug....");
            if (pipeServer.IsConnected)
            {
                MONITOR_PLUG_DETECTION _COMMAND = new MONITOR_PLUG_DETECTION();
                WriteAsync(_COMMAND.ToJson()).Wait();
            }
            return Task.CompletedTask;
        }

        public Task ToNKVM_SupportedMonitorList(List<string> supportedMonitorList)
        {
            _SupportedMonitors = supportedMonitorList;
            return Task.CompletedTask;
        }

        public Task<List<string>> UpdateSupportMonitors()
        {
            return Task.FromResult(_SupportedMonitors);
        }

        public Task<List<string>> GetSupportedNKVM()
        {
            _logs.DebugMsg("[NetworkKVM] GetSupportedNKVM....");
            bool isAdd = false;
            if (_AllInfoMonitors == null || _AllInfoMonitors.Count == 0)
            {
                _AllInfoMonitors = GetMonitors().Result;//_DisplayPlugin.GetMonitors();
            }
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
                            if (_SupportedMonitors.IndexOf(ModelName) == -1)
                            {
                                if (IsSupportNKVM(capabilityString))
                                {
                                    _SupportedMonitors.Add(ModelName);
                                    isAdd = true;
                                }
                            }
                        }
                        else
                        {
                            if (IsSupportNKVM(capabilityString))
                            {
                                _SupportedMonitors.Add(ModelName);
                                isAdd = true;
                            }
                        }
                    }
                    else
                    {
                        _SupportedMonitors = new List<string>();
                        if (IsSupportNKVM(capabilityString))
                        {
                            _SupportedMonitors.Add(ModelName);
                            isAdd = true;
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
            if (pipeServer != null)
            {
                if (pipeServer.IsConnected)
                {
                    cid = cid + 1;
                    ON_NKVM _COMMAND = new ON_NKVM();
                    _COMMAND.cid = cid;
                    _COMMAND.Checksum = _COMMAND.CalculateChecksum();
                    //_COMMAND.type = "ON_NKVM";
                    NKVMState = true;
                    WriteAsync(_COMMAND.ToJson()).Wait();
                }
            }
            return Task.CompletedTask;
        }

        public Task OffNKVM()
        {
            _logs.DebugMsg("[NetworkKVM] OffNKVM....");
            if (pipeServer != null)
            {
                if (pipeServer.IsConnected)
                {
                    cid = cid + 1;
                    OFF_NKVM _COMMAND = new OFF_NKVM();
                    _COMMAND.cid = cid;
                    _COMMAND.Checksum = _COMMAND.CalculateChecksum();
                    //_COMMAND.type = "OFF_NKVM";
                    NKVMState = false;
                    WriteAsync(_COMMAND.ToJson()).Wait();
                }
            }
            return Task.CompletedTask;
        }

        public Task<bool> isSupportMonitor(MonitorInfo monitorInfo)
        {
            string ModelName = monitorInfo.modelName.Replace(" ", "");
            if (ModelName.IndexOf("P2424HEB") != -1 ||
                ModelName.IndexOf("P2725DEB") != -1 ||
                ModelName.IndexOf("P3424WEB") != -1 ||
                ModelName.IndexOf("P5524Q") != -1 ||
                ModelName.IndexOf("P5524QT") != -1 ||
                ModelName.IndexOf("P6524QT") != -1 ||
                ModelName.IndexOf("P7524QT") != -1 ||
                ModelName.IndexOf("P8624QT") != -1 ||
                ModelName.IndexOf("P5525QC") != -1)
            {
                return Task.FromResult(true);
            }
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
                            if (IsSupportNKVM(capabilityString))
                            {
                                _SupportedMonitors.Add(ModelName);
                                return Task.FromResult(true);
                            }
                        }
                        else
                        {
                            return Task.FromResult(true);
                        }
                    }
                    else
                    {
                        if (IsSupportNKVM(capabilityString))
                        {
                            _SupportedMonitors.Add(ModelName);
                            return Task.FromResult(true);
                        }
                        //else
                        //{
                        //    string strSupport = ModelName.Substring(0, 1);
                        //    switch (strSupport)
                        //    {
                        //        case "U":
                        //        case "C":
                        //            return Task.FromResult(true);
                        //    }
                        //}
                    }
                }
                else
                {
                    _SupportedMonitors = new List<string>();
                    if (IsSupportNKVM(capabilityString))
                    {
                        _SupportedMonitors.Add(ModelName);
                        return Task.FromResult(true);
                    }
                    //else
                    //{
                    //    string strSupport = ModelName.Substring(0, 1);
                    //    switch (strSupport)
                    //    {
                    //        case "U":
                    //        case "C":
                    //            return Task.FromResult(true);
                    //    }
                    //}
                }
            }
            else
            {
                _logs.DebugMsg("[NetworkKVM] no C6....");
                string strSupport = ModelName.Substring(0, 1);
                switch (strSupport)
                {
                    case "U":
                    case "C":
                        return Task.FromResult(true);
                }
            }
            return Task.FromResult(false);
        }

        public Task<bool> HaveSuppertMonitor()
        {
            if (_AllInfoMonitors == null || _AllInfoMonitors.Count == 0)
            {
                _AllInfoMonitors = GetMonitors().Result;//_DisplayPlugin.GetMonitors();
            }
            foreach (MonitorInfo monitorInfo in _AllInfoMonitors)
            {
                if (isSupportMonitor(monitorInfo).Result)
                {
                    return Task.FromResult(true);
                }
            }
            return Task.FromResult(false);
        }

        public Task SetVCPNotify(MonitorInfo monitorInfo, int vcpcode, int value)
        {
            try
            {
                if (pipeServer.IsConnected)
                {
                    _logs.DebugMsg("[NetworkKVM] SetVCPNotify VcpCode : " + vcpcode.ToString());
                    _logs.DebugMsg("[NetworkKVM] SetVCPNotify value : " + value.ToString());
                    SET_VCP_NOTIFY set_VCP_NOTIFY = new SET_VCP_NOTIFY();
                    set_VCP_NOTIFY.MonitorIndex = monitorInfo.Index;
                    set_VCP_NOTIFY.VcpCode = vcpcode;
                    set_VCP_NOTIFY.Value = value;
                    set_VCP_NOTIFY.UpdateChecksum();

                    WriteAsync(set_VCP_NOTIFY.ToJson()).Wait();
                }
                if (vcpcode == 0x60)
                {
                    _AllInfoMonitors = GetMonitors().Result;
                }
            }
            catch
            {
                ;
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
                        if (hotkey == VirtualKey.Menu)
                        {
                            hotkeyWinform.Alt = true;
                        }
                        else
                        {
                            hotkeyWinform.Alt = false;
                        }
                        if (hotkey == VirtualKey.Control)
                        {
                            hotkeyWinform.Control = true;
                        }
                        else
                        {
                            hotkeyWinform.Control = false;
                        }
                        if (hotkey == VirtualKey.Shift)
                        {
                            hotkeyWinform.Shift = true;
                        }
                        else
                        {
                            hotkeyWinform.Shift = false;
                        }
                        if (hotkey != VirtualKey.Menu && hotkey != VirtualKey.Shift && hotkey != VirtualKey.Control)
                        {
                            hotkeyWinform.Key = (int)hotkey;
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
            catch
            {
                ;
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
            catch
            {
                ;
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
            catch
            {
                ;
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
            return Task.CompletedTask ;
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
            return Task.CompletedTask ;
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
            return Task.CompletedTask ;
        }

        public Task GetNKVMOutgoingPort()
        {
            if (pipeServer.IsConnected)
            {
                _logs.DebugMsg("[NetworkKVM] GetNKVMOutgoingPort...");
                GET_NKVM_OUTGOING_PORT get_NKVM_OUTGOING_PORT = new GET_NKVM_OUTGOING_PORT();
                cid = cid + 1;
                get_NKVM_OUTGOING_PORT.cid = cid;
                get_NKVM_OUTGOING_PORT.UpdateChecksum() ;
                WriteAsync(get_NKVM_OUTGOING_PORT.ToJson()).Wait();
            }
            return Task.CompletedTask ;
        }

        public Task GetNKVMContentTransferPort()
        {
            if (pipeServer.IsConnected)
            {
                _logs.DebugMsg("[NetworkKVM] GetNKVMContentTransferPort...");
                GET_NKVM_CONTENT_TRANSFER_PORT get_NKVM_CONTENT_TRANSFER_PORT = new GET_NKVM_CONTENT_TRANSFER_PORT();
                cid = cid + 1;
                get_NKVM_CONTENT_TRANSFER_PORT.cid = cid;
                get_NKVM_CONTENT_TRANSFER_PORT.UpdateChecksum() ;
                WriteAsync(get_NKVM_CONTENT_TRANSFER_PORT.ToJson()).Wait();
            }
            return Task.CompletedTask ;
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
            return Task.CompletedTask ;
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
        //        Thread.Sleep(1000);
        //    }
        //    return Task.CompletedTask;
        //}

        public Task CallNKVMConnent()
        {
            _logs.DebugMsg("[NetworkKVM] CallNKVMConnent....");
            var directory = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

            directory = $"C:\\Program Files\\Dell\\Dell Display and Peripheral Manager";
            string strFullPath = string.Format("{0}\\Plugins\\NKVM\\DDM.exe", directory);

            Trace.WriteLine($"NKVM full path is {strFullPath}");

            try
            {
                IntPtr NkvmdHandle = IntPtr.Zero;
                string processName = "DDM";
                Process[] processes = Process.GetProcessesByName(processName);

                if (processes.Length == 0)
                {
                    Console.WriteLine("No process found with the name: " + processName);

                    Process procNew = new Process();
                    procNew.StartInfo.FileName = strFullPath;
                    procNew.Start();
                }
                else
                {
                    foreach (Process process in processes)
                    {
                        if (process.ProcessName == processName)
                        {
                            NkvmdHandle = process.Handle;
                            Console.WriteLine($"Process ID: {process.Id}, Handle: {NkvmdHandle}");
                            break;
                        }
                    }
                }

                Process proc = new Process();
                proc.StartInfo.FileName = strFullPath;
                proc.StartInfo.Arguments = $"/Connect " + namedpipeName;
                _logs.DebugMsg("[NetworkKVM] Connect " + namedpipeName);
                proc.Start();
            }
            catch (System.Exception ex)
            {
                Trace.WriteLine($"ERROR : Run NKVM ==> {ex.ToString()}");
                Thread.Sleep(1000);
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

        //private class User32_SetWindowPos
        //{
        //    [DllImport("user32.dll", SetLastError = true)]
        //    public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        //    public static readonly IntPtr HWND_BOTTOM = new IntPtr(1);
        //    public static readonly IntPtr HWND_TOP = new IntPtr(0);
        //    public const uint SWP_NOSIZE = 0x0001;
        //    public const uint SWP_NOMOVE = 0x0002;
        //    public const uint SWP_NOACTIVATE = 0x0010;
        //    public const uint SWP_SHOWWINDOW = 0x0040;
        //    public const uint SWP_NOOWNERZORDER = 0x0200;
        //    public const uint SWP_NOREDRAW = 0x0008;
        //}

        //private struct WINDOWPOS
        //{
        //    public IntPtr hwnd;
        //    public IntPtr hwndInsertAfter;
        //    public int x;
        //    public int y;
        //    public int cx;
        //    public int cy;
        //    public uint flags;
        //}

        //private class DDPMWindowPos : NativeWindow
        //{
        //    private IntPtr _hwnd;
        //    private IntPtr _parent;

        //    private const int SWP_NOMOVE = 0x0002;
        //    private const int SWP_NOSIZE = 0x0001;
        //    private const int SWP_NOACTIVATE = 0x0010;
        //    private const int WM_WINDOWPOSCHANGING = 0x0046;
        //    private const int WM_ACTIVATE = 0x0006;
        //    private const int WM_NCACTIVATE = 0x0086;

        //    public DDPMWindowPos(IntPtr hwnd, IntPtr parent)
        //    {
        //        this._hwnd = hwnd;
        //        this._parent = parent;
        //        this.AssignHandle(parent);
        //    }

        //    protected override void WndProc(ref Message m)
        //    {
        //        switch (m.Msg)
        //        {
        //            case WM_WINDOWPOSCHANGING: // WM_WINDOWPOSCHANGING
        //                {
        //                    if (_hwnd != IntPtr.Zero)
        //                    {
        //                        //var pos = (WINDOWPOS)Marshal.PtrToStructure(m.LParam, typeof(WINDOWPOS));
        //                        //pos.hwndInsertAfter = User32_SetWindowPos.HWND_BOTTOM;
        //                        //pos.flags |= User32_SetWindowPos.SWP_NOACTIVATE;
        //                        //Marshal.StructureToPtr(pos, m.LParam, true);
        //                        User32_SetWindowPos.SetWindowPos(_hwnd, User32_SetWindowPos.HWND_TOP, 100, 100, 100, 100, User32_SetWindowPos.SWP_NOMOVE | User32_SetWindowPos.SWP_NOSIZE);
        //                    }
        //                }
        //                break;
        //                //case WM_ACTIVATE: // WM_ACTIVATE
        //                //    {
        //                //        if (m.WParam != IntPtr.Zero && _hwnd != IntPtr.Zero)
        //                //        {
        //                //            User32_SetWindowPos.SetWindowPos(this.Handle, User32_SetWindowPos.HWND_BOTTOM, 0, 0, 0, 0, User32_SetWindowPos.SWP_NOMOVE | User32_SetWindowPos.SWP_NOSIZE);
        //                //        }
        //                //    }
        //                //    break;
        //                //case WM_NCACTIVATE: // WM_NCACTIVATE
        //                //    {
        //                //        if (m.WParam != IntPtr.Zero && _hwnd != IntPtr.Zero)
        //                //        {
        //                //            User32_SetWindowPos.SetWindowPos(this.Handle, User32_SetWindowPos.HWND_BOTTOM, 0, 0, 0, 0, User32_SetWindowPos.SWP_NOMOVE | User32_SetWindowPos.SWP_NOSIZE);
        //                //        }
        //                //    }
        //                //    break;
        //        }
        //        base.WndProc(ref m);
        //    }
        //}

        private Task<List<MonitorInfo>> GetMonitors(bool renew = false)
        {
            lock (_pluginConditionLock)
            {
                //_logs.DebugMsg("[DisplayMangerPlugin] DisplayMangerPlugin received GetMonitors requested ...");

                _AllInfoMonitors.Clear();
                _AllInfoMonitors.AddRange(_VcpCorePlugin.GetMonitors(renew).Result);

                //_logs.DebugMsg("[DisplayMangerPlugin] GetMonitors() AllInfoMonitors.count is " + _AllInfoMonitors.Count);

                return Task.FromResult(_AllInfoMonitors);
            }
        }

        private async Task NamedPipeServer()
        {
            CreateNamedPipe_init();
            while (_runloop)
            {
                if (pipeServer.IsConnected)
                {
                    try
                    {
                        lock (lock_wait)
                        {
                            response = ReadAsync().Result;
                            _logs.DebugMsg("[NetworkKVM] Get :" + response);
                            if (response == "Disconnect")
                            {
                                Disconnect();
                                //CreateNamedPipe();
                            }
                            else
                            {
                                JsonstringParse(response).Wait(); //read json type
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        //throw;
                    }
                }
                else
                {
                    Disconnect();
                    CreateNamedPipe();
                }
            }
            _agent.StopAgent();
        }

        private void CreateNamedPipe_init()
        {
            //#if Debug_NKVM
            //            namedpipeName = "VCPNamedPipe";
            //#else
            //            namedpipeName = Guid.NewGuid().ToString("D");
            //#endif
            namedpipeName = "VCPNamedPipe";
            _logs.DebugMsg("[NetworkKVM] Name: " + namedpipeName);
            PipeSecurity pipeSecurity = NPipeSecurity.CreatePipeSecurity(PipeAccessRights.ReadWrite);

            pipeServer = NamedPipeServerStreamAcl.Create(namedpipeName,
                                                        PipeDirection.InOut,
                                                        1,
                                                        PipeTransmissionMode.Byte,
                                                        PipeOptions.Asynchronous | PipeOptions.WriteThrough,
                                                        0,
                                                        0,
                                                        pipeSecurity);
            cancellationTokenSource = new CancellationTokenSource();
            var c = CancellationTokenSource.CreateLinkedTokenSource(cancellationTokenSource.Token);
            if (HaveSuppertMonitor().Result)
            {
                CallNKVMConnent();
            }
            StartAsync().Wait();
        }

        private void CreateNamedPipe()
        {
            //#if Debug_NKVM
            //            namedpipeName = "VCPNamedPipe";
            //#else
            //            namedpipeName = Guid.NewGuid().ToString("D");
            //#endif
            namedpipeName = "VCPNamedPipe";
            _logs.DebugMsg("[NetworkKVM] Name: " + namedpipeName);
            PipeSecurity pipeSecurity = NPipeSecurity.CreatePipeSecurity(PipeAccessRights.ReadWrite);

            pipeServer = NamedPipeServerStreamAcl.Create(namedpipeName,
                                                        PipeDirection.InOut,
                                                        1,
                                                        PipeTransmissionMode.Byte,
                                                        PipeOptions.Asynchronous | PipeOptions.WriteThrough,
                                                        0,
                                                        0,
                                                        pipeSecurity);
            cancellationTokenSource = new CancellationTokenSource();
            var c = CancellationTokenSource.CreateLinkedTokenSource(cancellationTokenSource.Token);
            CallNKVMConnent();
            StartAsync().Wait();
        }

        private async Task StartAsync()
        {
            _logs.DebugMsg("[NetworkKVM] Wait Connection.....");
            await pipeServer.WaitForConnectionAsync(cancellationTokenSource.Token);
            _logs.DebugMsg("[NetworkKVM] Client Connect....");
            //string info;
            //if (NPipeSecurity.NamedPipeClientSecurity(pipeServer, out info))
            //{
                _logs.DebugMsg("[NetworkKVM] Client Security Pass....");
                ResponseSupportedMonitor().Wait();
                OnNKVM().Wait();
            //}
            //else
            //{
            //    _logs.DebugMsg($"[NetworkKVM] Client Security Fail....({info})");
            //    Disconnect();
            //    CreateNamedPipe();
            //}
        }

        private void Stop()
        {
            cancellationTokenSource.Cancel();
        }

        private void Disconnect()
        {
            _logs.DebugMsg("[NetworkKVM] Disconnect....");
            Stop();
            if (pipeServer.IsConnected)
            {
                //WriteAsync(DisconnectNamedPipe().Result).Wait();
                pipeServer.Disconnect();
            }
            pipeServer.Close();
            pipeServer.Dispose();
        }

        private async Task WriteAsync(string message)
        {
            _logs.DebugMsg("[NetworkKVM] WriteAsync : " + message);
            byte[] buffer = Encoding.UTF8.GetBytes(message);
            await pipeServer.WriteAsync(buffer, 0, buffer.Length).ConfigureAwait(false);
            await pipeServer.FlushAsync();
            pipeServer.WaitForPipeDrain();
        }

        private async Task<string> ReadAsync()
        {
            byte[] buffer = new byte[2048];
            int bytesRead = await pipeServer.ReadAsync(buffer, 0, buffer.Length).ConfigureAwait(false);
            string readmessage = Encoding.UTF8.GetString(buffer, 0, bytesRead);
            _logs.DebugMsg("[NetworkKVM] ReadAsync : " + readmessage);
            return readmessage;
        }

        private Task JsonstringParse(string jsonstring)
        {
            string type;
            string reStr = string.Empty;
            JObject json = JObject.Parse(jsonstring);
            if (json.ContainsKey("type"))
            {
                if (json["type"].Type != JTokenType.Null)
                {
                    type = (string)json["type"];
                    _logs.DebugMsg("[NetworkKVM] type: " + type);
                    switch (type)
                    {
                        case "SET_VCP":
                            SetVCP(jsonstring).Wait();
                            break;

                        case "GET_VCP":
                            GetVCP(jsonstring).Wait();
                            break;

                        case "GET_MONITOR_INFO":
                            GetMonitorInfo(jsonstring).Wait();
                            break;

                        case "GET_CURRENT_MONITOR_INDEX":
                            GetCurrentMonitorIndex(jsonstring).Wait();
                            break;

                        case "IS_HOTKEY_AVAILABLE":
                            isHotkeyAvailable(jsonstring).Wait();
                            break;

                        case "DISCONNECT":
                            Disconnect();
                            break;

                        case "UPDATE_SUPPORTED_MONITOR_LIST_RESPONSE":
                            if (!ResponseSucces(json).Result)
                            {
                                ResponseSupportedMonitor().Wait();
                            }
                            else
                            {
                                OnNKVM().Wait();
                            }
                            break;

                        case "ON_NKVM_RESPONSE":
                            if (!ResponseSucces(json).Result)
                            {
                                OnNKVM().Wait();
                            }
                            break;

                        case "OFF_NKVM_RESPONSE":
                            if (!ResponseSucces(json).Result)
                            {
                                OffNKVM().Wait();
                            }
                            break;

                        case "SET_HOTKEY":
                            GetSetHotkey(jsonstring).Wait();
                            break;

                        case "SET_HOTKEY_RESPONSE":
                            if (!ResponseSucces(json).Result && _HotkeyInfo != null)
                            {
                                bool b = SetHotkey(_HotkeyInfo).Result;
                            }
                            break;

                        case "GET_NKVM_VERSION_RESPONSE":
                            if (ResponseSucces(json).Result)
                            {
                                GetVersionResponse(jsonstring);
                            }
                            break;

                        case "GET_NKVM_STATUS_RESPONSE":
                            if (ResponseSucces(json).Result)
                            {
                                GetStatusResponse(jsonstring);
                            }
                            break;

                        case "GET_NKVM_AUTO_CONNECT_RESPONSE":
                            if (ResponseSucces(json).Result)
                            {
                                GetAutoConnectResponse(jsonstring);
                            }
                            break;

                        case "GET_NKVM_CONTENT_TRANSFER_RESPONSE":
                            if (ResponseSucces(json).Result)
                            {
                                GetContentTransferResponse(jsonstring);
                            }
                            break;

                        case "GET_NKVM_INCOMMING_PORT_RESPONSE":
                            if (ResponseSucces(json).Result)
                            {
                                GetIncommingPortResponse(jsonstring);
                            }
                            break;
                        case "GET_NKVM_OUTGOING_PORT_RESPONSE":
                            if (ResponseSucces(json).Result)
                            {
                                GetOutgoingPortResponse(jsonstring);
                            }
                            break;

                        case "GET_NKVM_CONTENT_TRANSFER_PORT_RESPONSE":
                            if (ResponseSucces(json).Result)
                            {
                                GetContentTransfedPortResponse(jsonstring);
                            }
                            break;

                        default:
                            NotFindType(json).Wait();
                            break;
                    }
                }
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
                        objGetVCP = _VcpCorePlugin.GetVCPCapability(_AllInfoMonitors[get_VCP.MonitorIndex], b_vcpcode, 0).Result;

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
            //byte[] by1 = get_MONITOR_INFO.Checksum;
            //get_MONITOR_INFO1.cid = get_MONITOR_INFO.cid;
            //get_MONITOR_INFO1.Checksum = get_MONITOR_INFO.CalculateChecksum();
            //byte[] by = get_MONITOR_INFO1.CalculateChecksum();
            if (get_MONITOR_INFO != null)
            {
                if (get_MONITOR_INFO.IsChecksumValid())
                {
                    DdpmJsonCommon.Monitor get_MonitorInfo = new DdpmJsonCommon.Monitor();
                    if (_AllInfoMonitors == null || _AllInfoMonitors.Count == 0)
                    {
                        _AllInfoMonitors = GetMonitors().Result;//_DisplayPlugin.GetMonitors();
                    }
                    get_MONITORINFO_R.cid = get_MONITOR_INFO.cid;
                    if (_AllInfoMonitors.Count != 0)
                    {
                        get_MONITORINFO_R.Success = true;
                        foreach (var item in _AllInfoMonitors)
                        {
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
                _logs.DebugMsg("[NetworkKVM] Not GET_MONITOR_INFO....");
            }
            get_MONITORINFO_R.Success = false;
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
        }

        private async Task<bool> ResponseSucces(JObject json)
        {
            if ((bool)json["Success"])
            {
                return true;
            }
            return false;
        }

        private Task ResponseSupportedMonitor()
        {
            _logs.DebugMsg("[NetworkKVM] ResponseSupportedMonitor....");
            cid = cid + 1;
            UPDATE_SUPPORTED_MONITOR_LIST SUPPORTED_MONITOR_LIST = new UPDATE_SUPPORTED_MONITOR_LIST();
            SUPPORTED_MONITOR_LIST.cid = cid;
            //SUPPORTED_MONITOR_LIST.type = "UPDATE_SUPPORTED_MONITOR_LIST";
            SUPPORTED_MONITOR_LIST.Monitors = _SupportedMonitors;
            SUPPORTED_MONITOR_LIST.UpdateChecksum();
            if (SUPPORTED_MONITOR_LIST.ToJson() != string.Empty)
            {
                WriteAsync(SUPPORTED_MONITOR_LIST.ToJson()).Wait();
            }
            return Task.CompletedTask;
        }

        private bool IsSupportNKVM(string s)
        {
            try
            {
                if (s == "" || s.Length < 10)
                {
                    return false;
                }
                string[] ss = s.Split("C6(");
                ss = ss[1].Split(")");
                _logs.DebugMsg("[NetworkKVM] C6 string : " + ss[0]);
                ss = ss[0].Split(" ");
                for (int i = 0; i < ss.Length; i++)
                {
                    _logs.DebugMsg("[NetworkKVM] C6 number : " + ss[i]);
                    if (ss[i].Equals("01"))
                    {
                        _logs.DebugMsg("[NetworkKVM] C6 number is 01.");
                        return (true);
                    }
                }
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
                                foreach (HotkeyInfo hotkeyInfo in hotkeySettings.HotkeyInfo)
                                {
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
                chanage_LIMITED_SW.Reason = "true";
            }
            else
            {
                _logs.DebugMsg("[NetworkKVM] SendChangeLimitedSW false");
                chanage_LIMITED_SW.Reason = "false";
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

        #endregion Private Methods

        #region Event Handler

        public delegate void NKVMPluginEventHandler(object sender, EventArgsjson eventArgsjson);

        public event NKVMPluginEventHandler NKVMPluginEvent;

        public event EventHandler<NKVMRespone> NKVMCLIEvent;

        public event EventHandler<NKVMSetHotkey> NKVMSetHotkey;

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

        private void VCPchangedEvent(object sender, VCPchangedEventArgs e)
        {
            _logs.DebugMsg("[NetworkKVM] VCPchangedEvent.....");
            if (e.vcpcode.Equals("60") || e.vcpcode.Equals("E8") || e.vcpcode.Equals("E9") || e.vcpcode.Equals("E5") || e.vcpcode.Equals("04"))
            {
                _logs.DebugMsg("[NetworkKVM] VCPchanged " + e.vcpcode);
                try
                {
                    int vcpcode = Convert.ToInt32(e.vcpcode);
                    int value = Convert.ToInt32(e.value);
                    SetVCPNotify(e.monitor, vcpcode, value);
                }
                catch
                {
                    ;
                }
            }
        }

        public void ToNKVMCLI(NKVMRespone response)
        {
            NKVMCLIEvent?.AsyncFireAndForget(this, response, System.Threading.CancellationToken.None);
        }

        public void SetDDPMHotkey(NKVMSetHotkey setHotkey)
        {
            NKVMSetHotkey?.AsyncFireAndForget(this, setHotkey, System.Threading.CancellationToken.None);
        }

        #endregion Event Handler
    }
}