using DDPM.SA.Common;
using DDPM.SA.Common.Display;
using DDPM.SA.Common.Security;
using DdpmJsonCommon;
using Dell.Client.Framework.Agent;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.Common.Annotations;
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
using WinCopies.Util;
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
                GetSupportedNKVM();
                if (pipeServer.IsConnected)
                {
                    WriteAsync(ResponseSupportedMonitor().Result).Wait();
                    MonitorPlug();
                }
                _AllInfoMonitors.Clear();
            }
            else
            {
                if (_AllInfoMonitors.Count == 0 && monitorInfos.Count > 0)
                {
                    //means plugin 1 or more monitor in
                    //Call Func: OnMonitorPlugIn(List<MonitorInfo> mos);
                    GetSupportedNKVM();
                    if (pipeServer.IsConnected)
                    {
                        WriteAsync(ResponseSupportedMonitor().Result).Wait();
                        MonitorPlug();
                    }
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
                        GetSupportedNKVM();
                        if (pipeServer.IsConnected)
                        {
                            WriteAsync(ResponseSupportedMonitor().Result).Wait();
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
                        GetSupportedNKVM();
                        if (pipeServer.IsConnected)
                        {
                            WriteAsync(ResponseSupportedMonitor().Result).Wait();
                            MonitorPlug();
                        }
                    }

                    _AllInfoMonitors.Clear();
                    _AllInfoMonitors.AddRange(monitorInfos);
                }
            }
            return Task.CompletedTask;
        }

        //public Task<bool> getNKVMStatus() //UI get to NKVM
        //{
        //    //to NKVM json....

        //    //send to VCPSDK
        //    NKVMPluginEvent?.Invoke(this, new EventArgsjson("Status"));
        //    //await WriteAsync("getStatus");

        //    //return Status...

        //    return Task.FromResult(true);
        //}
        //public Task setNKVMStatus() //UI set to NKVM
        //{
        //    NKVMPluginEvent?.Invoke(this, new EventArgsjson("set Status"));
        //    return Task.CompletedTask;
        //}
        public Task MonitorPlug()
        {
            //if (pipeServer.IsConnected)
            //{
            //cid = cid + 1;
            //_AllInfoMonitors.Clear();
            //_SupportedMonitors = GetSupportedNKVM().Result;
            MONITOR_PLUG_DETECTION _COMMAND = new MONITOR_PLUG_DETECTION();
            //_COMMAND.cid = cid;
            //_COMMAND.type = "MONITOR_PLUG_DETECTION";
            WriteAsync(_COMMAND.ToJson()).Wait();
            //}
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
            bool isAdd = false;
            if (_AllInfoMonitors == null || _AllInfoMonitors.Count == 0)
            {
                _AllInfoMonitors = GetMonitors().Result;//_DisplayPlugin.GetMonitors();
            }
            foreach (MonitorInfo monitorInfo in _AllInfoMonitors)
            {
                string ModelName = monitorInfo.modelName;
                string capabilityString = monitorInfo.CapabilityString;
                if (ModelName.IndexOf("P2425E") == -1 && ModelName.IndexOf("P2425HE") == -1 && ModelName.IndexOf("P2725HE") == -1)
                {
                    if (monitorInfo.CapabilityDic.ContainsKey("C6"))
                    {
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
            }
            //if (pipeServer.IsConnected)
            //{
            //    WriteAsync(ResponseSupportedMonitor().Result).Wait();
            //}
            return Task.FromResult(_SupportedMonitors);
        }

        public Task OnNKVM()
        {
            if (pipeServer.IsConnected)
            {
                cid = cid + 1;
                ON_NKVM _COMMAND = new ON_NKVM();
                _COMMAND.cid = cid;
                _COMMAND.Checksum = _COMMAND.CalculateChecksum();
                //_COMMAND.type = "ON_NKVM";
                WriteAsync(_COMMAND.ToJson()).Wait();
            }
            return Task.CompletedTask;
        }

        public Task OffNKVM()
        {
            if (pipeServer.IsConnected)
            {
                cid = cid + 1;
                OFF_NKVM _COMMAND = new OFF_NKVM();
                _COMMAND.cid = cid;
                _COMMAND.Checksum = _COMMAND.CalculateChecksum();
                //_COMMAND.type = "OFF_NKVM";
                WriteAsync(_COMMAND.ToJson()).Wait();
            }
            return Task.CompletedTask;
        }

        public Task<bool> isSupportMonitor(MonitorInfo monitorInfo)
        {
            string ModelName = monitorInfo.modelName.Replace(" ", "");
            if (ModelName.IndexOf("P2425E") != -1 || ModelName.IndexOf("P2425HE") != -1 || ModelName.IndexOf("P2725HE") != -1)
            {
                return Task.FromResult(true);
            }
            if (monitorInfo.CapabilityDic.ContainsKey("C6"))
            {
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
                        else
                        {
                            string strSupport = ModelName.Substring(0, 1);
                            switch (strSupport)
                            {
                                case "U":
                                case "C":
                                    return Task.FromResult(true);
                            }
                        }
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
                    else
                    {
                        string strSupport = ModelName.Substring(0, 1);
                        switch (strSupport)
                        {
                            case "U":
                            case "C":
                                return Task.FromResult(true);
                        }
                    }
                }
            }
            else
            {
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
                    set_VCP_NOTIFY.Checksum = set_VCP_NOTIFY.CalculateChecksum();

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
                    set_HOTKEY.Checksum = set_HOTKEY.CalculateChecksum();

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
            CreateNamedPipe();
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
                                CreateNamedPipe();
                            }
                            else
                            {
                                string returntest = JsonstringParse(response).Result; //read json type
                                if (returntest != string.Empty)
                                {
                                    WriteAsync(returntest).Wait();
                                }
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

        private void CreateNamedPipe()
        {
#if DEBUG
            string namedPipeName = "VCPNamedPipe";
#else
            string namedPipeName = Guid.NewGuid().ToString("D");
#endif
            _logs.DebugMsg("[NetworkKVM] Name: " + namedPipeName);
            PipeSecurity pipeSecurity = NPipeSecurity.CreatePipeSecurity(PipeAccessRights.ReadWrite);

            pipeServer = NamedPipeServerStreamAcl.Create(namedPipeName,
                                                        PipeDirection.InOut,
                                                        1,
                                                        PipeTransmissionMode.Byte,
                                                        PipeOptions.Asynchronous | PipeOptions.WriteThrough,
                                                        0,
                                                        0,
                                                        pipeSecurity);
            cancellationTokenSource = new CancellationTokenSource();
            StartAsync(namedPipeName).Wait();
        }

        private async Task StartAsync(string NamedpipeName)
        {
            //Console.WriteLine("Wait Connection....." + "\n");
            _logs.DebugMsg("[NetworkKVM] Wait Connection.....");
            CallNKVMConnent(NamedpipeName);
            await pipeServer.WaitForConnectionAsync(cancellationTokenSource.Token);
            //Console.WriteLine("Client Connect....");
            _logs.DebugMsg("[NetworkKVM] Client Connect....");
            //command = OnNetworkKVM().Result;
            //WriteAsync(command).Wait();
            _SupportedMonitors = GetSupportedNKVM().Result;
            WriteAsync(ResponseSupportedMonitor().Result).Wait();
            OnNKVM().Wait();
        }

        private void Stop()
        {
            cancellationTokenSource.Cancel();
        }

        private void Disconnect()
        {
            Stop();
            if (pipeServer.IsConnected)
            {
                WriteAsync(DisconnectNamedPipe().Result).Wait();
                pipeServer.Disconnect();
            }
            pipeServer.Close();
            pipeServer.Dispose();
        }

        private async Task WriteAsync(string message)
        {
            Console.WriteLine("WriteAsync : " + message);
            byte[] buffer = Encoding.UTF8.GetBytes(message);
            await pipeServer.WriteAsync(buffer, 0, buffer.Length);
            await pipeServer.FlushAsync();
            pipeServer.WaitForPipeDrain();
        }

        private async Task<string> ReadAsync()
        {
            byte[] buffer = new byte[2048];
            int bytesRead = await pipeServer.ReadAsync(buffer, 0, buffer.Length);
            return Encoding.UTF8.GetString(buffer, 0, bytesRead);
        }

        private async Task<string> JsonstringParse(string jsonstring)
        {
            string type;
            string reStr = string.Empty;
            JObject json = JObject.Parse(jsonstring);
            if (json.ContainsKey("type"))
            {
                type = (string)json["type"];
                switch (type)
                {
                    case "SET_VCP":
                        reStr = SetVCP(json).Result;
                        break;

                    case "GET_VCP":
                        reStr = GetVCP(json).Result;
                        break;

                    case "GET_MONITOR_INFO":
                        reStr = GetMonitorInfo(jsonstring).Result;
                        break;

                    case "GET_CURRENT_MONITOR_INDEX":
                        reStr = GetCurrentMonitorIndex(json).Result;
                        break;

                    case "IS_HOTKEY_AVAILABLE":
                        reStr = isHotkeyAvailable(jsonstring).Result;
                        break;
                    case "DISCONNECT":
                        Disconnect();
                        CreateNamedPipe();
                        break;
                    case "UPDATE_SUPPORTED_MONITOR_LIST_RESPONSE":
                        if (!ResponseSucces(json).Result)
                        {
                            reStr = ResponseSupportedMonitor().Result;
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

                    case "SET_HOTKEY_RESPONSE":
                        if (!ResponseSucces(json).Result && _HotkeyInfo != null)
                        {
                            bool b = SetHotkey(_HotkeyInfo).Result;
                        }
                        break;

                    default:
                        reStr = NotFindType(json).Result;
                        break;
                }
            }
            return reStr;
        }

        private async Task<string> SetVCP(JObject json)
        {
            byte b_vcpcode;
            SET_VCP_RESPONSE set_VCP_R = new SET_VCP_RESPONSE();
            if (_AllInfoMonitors == null || _AllInfoMonitors.Count == 0)
            {
                _AllInfoMonitors = GetMonitors().Result;//_DisplayPlugin.GetMonitors();
            }
            set_VCP_R.cid = (int)json["cid"];
            //set_VCP_R.type = (string)json["type"] + "_RESPONSE";
            if (_AllInfoMonitors.Count != 0)
            {
                if (json.ContainsKey("MonitorIndex") && json.ContainsKey("VcpCode") && json.ContainsKey("Value"))
                {
                    b_vcpcode = Convert.ToByte(((int)json["VcpCode"]).ToString());
                    bool b = _VcpCorePlugin.SetVCPCapability(_AllInfoMonitors[(int)json["MonitorIndex"]], b_vcpcode, (uint)(int)json["Value"]).Result;
                    set_VCP_R.MonitorIndex = (int)json["MonitorIndex"];
                    set_VCP_R.VcpCode = (int)json["VcpCode"];
                    set_VCP_R.Value = (int)json["Value"];
                    set_VCP_R.Success = b;
                    set_VCP_R.Checksum = set_VCP_R.CalculateChecksum();
                }
                else
                {
                    set_VCP_R.MonitorIndex = -1;
                    set_VCP_R.VcpCode = -1;
                    set_VCP_R.Value = -1;
                    set_VCP_R.Success = false;
                    set_VCP_R.Checksum = set_VCP_R.CalculateChecksum();
                }
            }
            else
            {
                set_VCP_R.MonitorIndex = -1;
                set_VCP_R.VcpCode = -1;
                set_VCP_R.Value = -1;
                set_VCP_R.Success = false;
                set_VCP_R.Checksum = set_VCP_R.CalculateChecksum();
            }
            return set_VCP_R.ToJson();
        }

        private async Task<string> GetVCP(JObject json)
        {
            byte b_vcpcode;
            ObjGetVCP objGetVCP;
            string rc_string = string.Empty;
            GET_VCP_RESPONSE get_VCP_R = new GET_VCP_RESPONSE();
            if (_AllInfoMonitors == null || _AllInfoMonitors.Count == 0)
            {
                _AllInfoMonitors = GetMonitors().Result;//_DisplayPlugin.GetMonitors();
            }
            get_VCP_R.cid = (int)json["cid"];
            //get_VCP_R.type = (string)json["type"] + "_RESPONSE";
            if (_AllInfoMonitors.Count != 0)
            {
                if (json.ContainsKey("MonitorIndex") && json.ContainsKey("VcpCode"))
                {
                    b_vcpcode = Convert.ToByte(((int)json["VcpCode"]).ToString());
                    objGetVCP = _VcpCorePlugin.GetVCPCapability(_AllInfoMonitors[(int)json["MonitorIndex"]], b_vcpcode, 0).Result;

                    get_VCP_R.MonitorIndex = (int)json["MonitorIndex"];
                    get_VCP_R.VcpCode = (int)json["VcpCode"];
                    if (objGetVCP.result)
                    {
                        get_VCP_R.Success = true;
                        get_VCP_R.Value = (int)(uint)objGetVCP.value;
                        get_VCP_R.Checksum = get_VCP_R.CalculateChecksum();
                    }
                    else
                    {
                        get_VCP_R.Success = false;
                        get_VCP_R.Value = 0;
                        get_VCP_R.Checksum = get_VCP_R.CalculateChecksum();
                    }
                }
                else
                {
                    get_VCP_R.MonitorIndex = -1;
                    get_VCP_R.VcpCode = -1;
                    get_VCP_R.Success = false;
                    get_VCP_R.Value = 0;
                    get_VCP_R.Checksum = get_VCP_R.CalculateChecksum();
                }
            }
            else
            {
                get_VCP_R.MonitorIndex = -1;
                get_VCP_R.VcpCode = -1;
                get_VCP_R.Success = false;
                get_VCP_R.Value = 0;
                get_VCP_R.Checksum = get_VCP_R.CalculateChecksum();
            }
            return get_VCP_R.ToJson();
        }

        private async Task<string> GetMonitorInfo(string jsonstring)
        {
            GET_MONITOR_INFO get_MONITOR_INFO = new GET_MONITOR_INFO();
            //GET_MONITOR_INFO get_MONITOR_INFO1 = new GET_MONITOR_INFO();
            GET_MONITOR_INFO_RESPONSE get_MONITORINFO_R = new GET_MONITOR_INFO_RESPONSE();
            get_MONITOR_INFO = JsonConvert.DeserializeObject<GET_MONITOR_INFO>(jsonstring);

            //byte[] by1 = get_MONITOR_INFO.Checksum;
            //get_MONITOR_INFO1.cid = get_MONITOR_INFO.cid;
            //get_MONITOR_INFO1.Checksum = get_MONITOR_INFO.CalculateChecksum();
            //byte[] by = get_MONITOR_INFO1.CalculateChecksum();

            get_MONITORINFO_R.Monitors = new List<DdpmJsonCommon.Monitor>();
            DdpmJsonCommon.Monitor get_MonitorInfo = new DdpmJsonCommon.Monitor();
            if (_AllInfoMonitors == null || _AllInfoMonitors.Count == 0)
            {
                _AllInfoMonitors = GetMonitors().Result;//_DisplayPlugin.GetMonitors();
            }
            get_MONITORINFO_R.cid = get_MONITOR_INFO.cid;
            //get_MONITORINFO_R.type = (string)json["type"] + "_RESPONSE";
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
                    get_MONITORINFO_R.Checksum = get_MONITORINFO_R.CalculateChecksum();
                }
            }
            else
            {
                _logs.DebugMsg("[NetworkKVM] No Monitor....");
                get_MONITORINFO_R.Success = false;
                get_MONITORINFO_R.Monitors = null;
                get_MONITORINFO_R.Checksum = get_MONITORINFO_R.CalculateChecksum();
            }
            return get_MONITORINFO_R.ToJson();
        }

        private async Task<string> GetCurrentMonitorIndex(JObject json)
        {
            GET_CURRENT_MONITOR_INDEX_RESPONSE get_CURRENT_MONITOR_INDEX_R = new GET_CURRENT_MONITOR_INDEX_RESPONSE();
            MonitorInfo monitorInfo = new MonitorInfo();
            if (_AllInfoMonitors == null || _AllInfoMonitors.Count == 0)
            {
                _AllInfoMonitors = GetMonitors().Result;//_DisplayPlugin.GetMonitors();
            }
            get_CURRENT_MONITOR_INDEX_R.cid = (int)json["cid"];
            //get_CURRENT_MONITOR_INDEX_R.type = (string)json["type"] + "_RESPONSE";
            if (_AllInfoMonitors.Count != 0)
            {
                monitorInfo = _AllInfoMonitors.Find(x => (x.Index == 0));
                get_CURRENT_MONITOR_INDEX_R.Success = true;
                get_CURRENT_MONITOR_INDEX_R.MonitorIndex = monitorInfo.Index;
                get_CURRENT_MONITOR_INDEX_R.Checksum = get_CURRENT_MONITOR_INDEX_R.CalculateChecksum();
            }
            else
            {
                get_CURRENT_MONITOR_INDEX_R.Success = false;
                get_CURRENT_MONITOR_INDEX_R.MonitorIndex = -1;
                get_CURRENT_MONITOR_INDEX_R.Checksum = get_CURRENT_MONITOR_INDEX_R.CalculateChecksum();
            }
            return get_CURRENT_MONITOR_INDEX_R.ToJson();
        }

        private async Task<string> DisconnectNamedPipe()
        {
            //cid = cid + 1;
            DISCONNECT _COMMAND = new DISCONNECT();
            _COMMAND.Checksum = _COMMAND.CalculateChecksum();
            //_COMMAND.cid = cid;
            //_COMMAND.type = "DISCONNECT";

            return _COMMAND.ToJson();
        }

        private async Task<string> NotFindType(JObject json)
        {
            DDM_RESPONSE _RESPONSE = new DDM_RESPONSE();
            _RESPONSE.Success = false;
            _RESPONSE.cid = (int)json["cid"];
            _RESPONSE.Checksum = _RESPONSE.CalculateChecksum();
            //_RESPONSE.type = (string)json["type"] + "_RESPONSE";
            return _RESPONSE.ToJson();
        }

        private async Task<bool> ResponseSucces(JObject json)
        {
            if ((bool)json["Success"])
            {
                return true;
            }
            return false;
        }

        private async Task<string> ResponseSupportedMonitor()
        {
            cid = cid + 1;
            UPDATE_SUPPORTED_MONITOR_LIST SUPPORTED_MONITOR_LIST = new UPDATE_SUPPORTED_MONITOR_LIST();
            SUPPORTED_MONITOR_LIST.cid = cid;
            //SUPPORTED_MONITOR_LIST.type = "UPDATE_SUPPORTED_MONITOR_LIST";
            SUPPORTED_MONITOR_LIST.Monitors = _SupportedMonitors;
            SUPPORTED_MONITOR_LIST.Checksum = SUPPORTED_MONITOR_LIST.CalculateChecksum();
            return SUPPORTED_MONITOR_LIST.ToJson();
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

        private async Task<string> isHotkeyAvailable(string jsonstring)
        {
            IS_HOTKEY_AVAILABLE is_HOTKEY_AVAILABLE = new IS_HOTKEY_AVAILABLE();
            is_HOTKEY_AVAILABLE = JsonConvert.DeserializeObject<IS_HOTKEY_AVAILABLE>(jsonstring);
            IS_HOTKEY_AVAILABLE_RESPONSE is_HOTKEY_AVAILABLE_RESPONSE = new IS_HOTKEY_AVAILABLE_RESPONSE();
            is_HOTKEY_AVAILABLE_RESPONSE.cid = is_HOTKEY_AVAILABLE.cid;
            HotkeyWinform jsonHotkey = is_HOTKEY_AVAILABLE.Hotkey;
            is_HOTKEY_AVAILABLE_RESPONSE.Hotkey = jsonHotkey;
            if (_HotkeySettings != null && _HotkeySettings.Count != 0)
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
                                is_HOTKEY_AVAILABLE_RESPONSE.Checksum = is_HOTKEY_AVAILABLE_RESPONSE.CalculateChecksum();
                                return is_HOTKEY_AVAILABLE_RESPONSE.ToJson();
                            }
                        }
                    }
                }
                _logs.DebugMsg("[NetworkKVM] isHotkeyAvailable true");
                is_HOTKEY_AVAILABLE_RESPONSE.Available = true;
                is_HOTKEY_AVAILABLE_RESPONSE.Success = true;
                is_HOTKEY_AVAILABLE_RESPONSE.Checksum = is_HOTKEY_AVAILABLE_RESPONSE.CalculateChecksum();
            }
            else
            {
                is_HOTKEY_AVAILABLE_RESPONSE.Available = false;
                is_HOTKEY_AVAILABLE_RESPONSE.Success = false;
                is_HOTKEY_AVAILABLE_RESPONSE.Checksum = is_HOTKEY_AVAILABLE_RESPONSE.CalculateChecksum();
            }
            return is_HOTKEY_AVAILABLE_RESPONSE.ToJson();
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
            chanage_LIMITED_SW.Checksum = chanage_LIMITED_SW.CalculateChecksum();

            WriteAsync(chanage_LIMITED_SW.ToJson()).Wait();
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

        private void CallNKVMConnent(string NamedpipeName)
        {
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
                        NkvmdHandle = process.Handle;
                        Console.WriteLine($"Process ID: {process.Id}, Handle: {NkvmdHandle}");
                        break;
                    }
                }

                Process proc = new Process();
                proc.StartInfo.FileName = strFullPath;
                proc.StartInfo.Arguments = $"/Connect " + NamedpipeName;
                _logs.DebugMsg("[NetworkKVM] Connect " + NamedpipeName);
                proc.Start();
            }
            catch (System.Exception ex)
            {
                Trace.WriteLine($"ERROR : Run NKVM ==> {ex.ToString()}");
                Thread.Sleep(1000);
            }
        }
#endregion

        #region Event Handler

        public delegate void NKVMPluginEventHandler(object sender, EventArgsjson eventArgsjson);

        public event NKVMPluginEventHandler NKVMPluginEvent;

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

        #endregion Event Handler
    }
}