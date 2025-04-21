#region LicenceHeader

//
// Copyright © 2024, Dell Inc., All Rights Reserved.
// This material is confidential and a trade secret.  Permission to use this
// work for any purpose must be obtained in writing from Dell Inc.
//
// DisplayMangerPlugin.cs created on 24/04/2024T11:20 AM
//

#endregion

using DDPM.SA.Common;
using DDPM.SA.Common.Display;
using DDPM.SA.Common.Interfaces;
using DDPM.SA.Common.Method;
using DDPM.SA.Common.Security;
using DDPM.SA.Common.Settings;
using DDPM.SA.Obfuscation;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.Common.Annotations;
using Dell.Client.Framework.Common.Extensions;
using Dell.Client.Framework.Common.PluginConditions;
using Dell.Client.Framework.Interfaces;
using Microsoft;
using Microsoft.Win32;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Security.Policy;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using VcpCore.Common;
using VcpCore.Interfaces;
using static VcpCore.Common.EDIDReader;
using static VcpCore.Common.User32;
using IDs = DDPM.SA.Common.IDs;

//using WinCopies;

namespace DDPM.SA.Plugins.User.DisplayManager
{
    [Plugin(IDs.Display_Manager_PLUGIN_ID, pluginName, PluginOrderGroupType.Core, Version = pluginVersion)]
    [Descriptor(Description = pluginDescription)]
    [Publisher(Name = publisherCompany, Website = publisherWebsite, Support = publisherSupport)]
    [PublishedUnelevatedInterface(new[] { typeof(IDisplayService) })]
    [DependencyKnownTypes(new[] { typeof(IVcpCoreService) })]
    [PluginRequires(Id = IDs.VCP_CORE_PLUGIN_ID, Version = "1.0.0", AllowDynamicResolving = true)]
    public class DisplayMangerPlugin : BaseAgentPlugin, IDisposableObservable, IDisplayService
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

            text = $"[DisplayMangerPlugin] {text}, Caller Name:{memberName}, Source Line {sourceLineNumber}";
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

        private const string pluginName = "DisplayManagerPlugin";
        private const string pluginVersion = "1.0.0";
        private const string pluginDescription = "This plugin implements Display Manager Plugin.";
        private const string publisherCompany = "Dell Technologies";
        private const string publisherWebsite = "https://www.dell.com";
        private const string publisherSupport = "This plugin implements Display Manager Plugin.";

        private bool _IsAdministrator = ProcessSecurityHelperWrapper.IsCurrentProcessRunningElevated();

        private IAgent _agent;
        private const string PluginLogId = "DisplayManager";

        private Logs _logs;
        private IVcpCoreService _VcpCorePlugin;
        private PluginCondition _VcpCorePluginCondition;
        //private bool _VcpCorePluginUsable = false;

        private List<MonitorInfo> _AllInfoMonitors = new List<MonitorInfo>();

        //Input
        private Dictionary<string, InputInfo> inputSourcelist = new Dictionary<string, InputInfo>();

        private List<string> usbUpstreamList = new List<string>();
        private string _getVCPCapabilities = string.Empty;
        //private string currentInput;

        private readonly object _PluginConditionLock = new object();
        private readonly object _GetMonitorsLock = new object();

        //0607 Bruce 是否鎖定畫面自動旋轉
        private bool isLockOrientation;

        private bool isSWSetOrientation;

        private readonly object _ALSVCPChangeLock = new object();

        private Dictionary<string, PCsInfo> _PCsList = new Dictionary<string, PCsInfo>();

        private Dictionary<string, string> USBUplink = new Dictionary<string, string>() // Uplink Port num, Port name
        {
            ["0000"] = "USB-B1",
            ["0001"] = "USB-B2",
            ["1000"] = "USB-C1",
            ["1001"] = "USB-C2",
            ["1010"] = "USB-C3",
            ["1011"] = "USB-C4",
            ["1100"] = "Thunderbolt1",
            ["1101"] = "Thunderbolt2"
        };

        private Dictionary<string, string> USBUplink_E7 = new Dictionary<string, string>()
        {
            ["00"] = "USB-B1",
            ["01"] = "USB-B2",
            ["10"] = "USB-C1",
            ["11"] = "USB-C2"
        };

        private Dictionary<string, string> USBUpstream = new Dictionary<string, string>(); // Port name, Upstream Port num

        private string[] OrientationString = new string[] { "", "Landscape", "Portrait", "Landscape_flipped", "Portrait_flipped" };//OSD orientation

        //Derek 2024/10/21
        private Process uiProcess = null;

        private DisplayDataManger _displayDataManger = new DisplayDataManger();

        #endregion

        #region Public Members

        public event EventHandler<VCPchangedEventArgs> VCPchanged;

        public event EventHandler<DDCCIchangedEventArgs> DDCCIStatuschanged;

        public event EventHandler<DisplaychangedEventArgs> Displaychanged;

        public event EventHandler<MonitorinfoUpdateEventArgs> MonitorinfoUpdated;

        public static List<ALSConfig> AllALSConfig { get => allALSConfig; set => allALSConfig = value; }

        private static List<ALSConfig> allALSConfig = new List<ALSConfig>();

        /// <summary>
        /// HDR status change event，return HDR status
        /// </summary>
        public event EventHandler<bool> HDRChangeEvent;

        /// <summary>
        /// gaming parameter changes event，return gaming parameter
        /// </summary>
        public event EventHandler<GamingDisplayPropertiesInfo> GamingChangeEvent;

        /// <summary>
        /// Control 67/68 event
        /// </summary>
        private readonly Dictionary<string, DateTime> LastProcessedTimestamps = new();

        private ISettingsManagerDev _SettingsPlugin;
        private readonly object _SettingsPluginConditionLock = new object();
        private bool IsDDPMLaunchEarly = false;
        private bool IsDDPMLaunchNow = false;

        #endregion

        #region Constructor

        public DisplayMangerPlugin(IAgent agent) : base(agent, PluginLogId)
        {
            _agent = agent;

            _IsAdministrator = ProcessSecurityHelperWrapper.IsCurrentProcessRunningElevated();
            _logs = new Logs(Log);

            _logs.DebugMsg("[DisplayMangerPlugin] Does DisplayMangerPlugin have Administrator: " + _IsAdministrator.ToString());
        }

        #endregion

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
                if (disposing)
                {
                    _agent.PluginManager.PluginsStarted -= PluginManagerOnPluginsStarted;
                    _agent = null;
                }

                IsDisposed = true;
            }
            base.Dispose(disposing);
        }

        #endregion

        #region Overriding methods

        protected override void OnPluginStarting()
        {
            PluginCondition = new PluginStartedCondition();

            _agent.PluginManager.PluginsStarted += PluginManagerOnPluginsStarted;

            InitializeVcpCorePlugin();

            #region Bruce display properties

            InitializeDisplayPropertiesPlugin();

            #endregion

            //Robert_Lin, 2024-5-23
            InitializePipPbpManagerPlugin();
            //Robert_Lin, 2024-7-4
            InitializeEAPlugin();
        }

        #endregion

        #region IDisplayService implementation

        public Task<Dictionary<EDID, Dictionary<object, object>>> GetVCPCacheTable()
        {
            _logs.DebugMsg("[DisplayMangerPlugin] DisplayMangerPlugin GetVCPCacheTable  ...");

            var _CacheTable = _VcpCorePlugin.GetVCPCacheTable().Result;

            return Task.FromResult(_CacheTable);
        }

        public Task<bool> GetIsDDPMLaunchNow()
        {
            return Task.FromResult(IsDDPMLaunchNow);
        }

        public Task<bool> GetIsDDPMLaunchEarly()
        {
            return Task.FromResult(IsDDPMLaunchEarly);
        }

        public Task<bool> LauncDDPM(string UserId, string ddpmExePath)
        {
            _logs.DebugMsg($"LauncDDPM CheckDeviceFirstTimesToConnect UserId : {UserId}, StartProcess ... ");
            bool result = DDPMFileSecurity.ValidateFilePath(ddpmExePath, out string info);
            if (result)
            {
                result = DDPM.SA.Common.Settings.DDPMFileSecurity.StartProcessSafely(
                    null,
                    new ProcessStartInfo
                    {
                        FileName = ddpmExePath,
                        UseShellExecute = true
                    });

                if (!result)
                {
                    _logs.DebugMsg($"LauncDDPM CheckDeviceFirstTimesToConnect StartProcessSafely fail");
                }
            }
            else
            {
                _logs.DebugMsg($"LauncDDPM CheckDeviceFirstTimesToConnect ValidateFilePath fail");
            }
            return Task.FromResult(result);
        }

        public Task Reset0x52TimerTick(int millisecond, int processID = -0xFF)
        {
            _logs.DebugMsg("[DisplayMangerPlugin] DisplayMangerPlugin received Reset0x52TimerTick: " +
                millisecond.ToString() + $" requested, process ID[{processID}]");

            if (-0xFF != processID)
                CreateProcessExitEvent(processID);

            if (_VcpCorePlugin != null)
                _VcpCorePlugin.Reset0x52TimerTick(millisecond);

            IsDDPMLaunchEarly = IsDDPMLaunchNow;
            if (millisecond == 2000) // 8000 mean UI close, 2000 mean UI open
            {
                IsDDPMLaunchNow = true;
                _logs.DebugMsg("[DisplayMangerPlugin] Reset0x52TimerTick: millisecond = 2000 , UI Open");
            }
            else
            {
                IsDDPMLaunchNow = false;
                _logs.DebugMsg("[DisplayMangerPlugin] Reset0x52TimerTick: millisecond = 8000 , UI Close");
            }

            // If APP WalkThrough not done, need re-launch APP
            //if (IsDDPMLaunchEarly && !IsDDPMLaunchNow)
            //{
            //    if (_SettingsPlugin == null)
            //    {
            //        _logs.DebugMsg($"Reset0x52TimerTick LauncDDPM but _SettingsPlugin NULL ... ");
            //        return Task.FromResult(Task.CompletedTask);
            //    }
            //    _logs.DebugMsg($"LauncDDPM CheckDeviceFirstTimesToConnect IsDDPMLaunchEarly true, IsDDPMLaunchNow false");
            //    string UserId = WTSFunction.DirectGetUserID(Log);
            //    string regPath = $@"SOFTWARE\Dell\Dell Display And Peripheral Manager\UserSettings\Local\{UserId}";
            //    string regKeyForDDPM = $"IsFirstTimeWalkThroughDone_com.dell.DPM.Plugin.LogicalDevice.DDPM";
            //    object regValue = _SettingsPlugin.ReadRegistryData(DDPM.SA.Common.Settings.RegistryHive.LocalMachine, regPath, regKeyForDDPM).Result;
            //    //_logs.DebugMsg($"LauncDDPM CheckDeviceFirstTimesToConnect regValue {Convert.ToBoolean(regValue).ToString()}");
            //    if (regValue == null || (regValue is string strValue && string.IsNullOrEmpty(strValue)))
            //    {
            //        string ddpmExePath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), @"Dell\Dell Display and Peripheral Manager\DDPM.exe");
            //        //string ddpmExePath = @"D:\\NEW\DDPM\DDPM.UI\\bin\\net8.0-windows10.0.19041.0\\DDPM.exe";
            //        LauncDDPM(UserId, ddpmExePath);
            //        _logs.DebugMsg($"LauncDDPM From Reset0x52TimerTick ... ");
            //    }
            //    else
            //    {
            //        _logs.DebugMsg($"LauncDDPM From Reset0x52TimerTick Reg Exit ... ");
            //    }
            //}
            return Task.CompletedTask;
        }

        public Task SetIsUserActive(bool IsUserActive)
        {
            _logs.DebugMsg("[DisplayMangerPlugin] DisplayMangerPlugin received SetIsUserActive: " + IsUserActive.ToString() + " requested ...");

            if (_VcpCorePlugin != null)
                _VcpCorePlugin.SetIsUserActive(IsUserActive);

            return Task.CompletedTask;
        }

        public Task CancelVcpTask(Guid user_guid)
        {
            _logs.DebugMsg("[DisplayMangerPlugin] DisplayMangerPlugin received CancelVcpTask: " + user_guid.ToString() + " requested ...");

            if (_VcpCorePlugin != null)
                _VcpCorePlugin.CancelVcpTask(user_guid);

            return Task.CompletedTask;
        }

        private Task<bool> CreateProcessExitEvent(int processID)
        {
            bool result = true;

            try
            {
                uiProcess = Process.GetProcessById(processID);
                uiProcess.EnableRaisingEvents = true;
                uiProcess.Exited += new EventHandler(Process_Exited);

                //_logs.DebugMsg($"Process Name: {uiProcess.ProcessName}");
                //_logs.DebugMsg($"Process ID: {uiProcess.Id}");
            }
            catch (ArgumentException ex)
            {
                result = false;
                //_logs.Error($"Process with ID {processID} is not running: {ex.Message}");
                WriteLog($"Process with ID {processID} is not running: {ex.Message}", log_type.error);
            }

            return Task.FromResult(result);
        }

        private async void Process_Exited(object sender, EventArgs e)
        {
            uiProcess = null;

            //await _VcpCorePlugin.Reset0x52TimerTick(8000);
            await Reset0x52TimerTick(8000);
        }

        public Task<List<MonitorInfo>> GetMonitors()
        {
            lock (_GetMonitorsLock)
            {
                _logs.DebugMsg("[DisplayMangerPlugin] DisplayMangerPlugin received GetMonitors requested ...");

                if (_VcpCorePlugin != null)
                {
                    var monitorInfos = (_VcpCorePlugin.GetMonitors().Result).ToList();

                    _AllInfoMonitors = monitorInfos.ToList();

                    _logs.DebugMsg("[DisplayMangerPlugin] GetMonitors() AllInfoMonitors.count is " + _AllInfoMonitors.Count);

                    if (_AllInfoMonitors == null || _AllInfoMonitors.Count == 0)
                    {
                        AllALSConfig.Clear();
                        _logs.DebugMsg("[DisplayMangerPlugin] GetMonitors() AllALSConfig Clear ");
                    }

                    return Task.FromResult(monitorInfos);
                }
                else
                {
                    _logs.DebugMsg("[DisplayMangerPlugin] _VcpCorePlugin is null");
                    return Task.FromResult(new List<MonitorInfo>());
                }
            }
        }

        public async Task<List<MonitorInfo>> Re_GetMonitors(CancellationToken Token)
        {
            try
            {
                _logs.DebugMsg("[DisplayMangerPlugin] DisplayMangerPlugin received Re_GetMonitors requested ...");

                if (_VcpCorePlugin != null)
                {
                    var monitorInfos = (await _VcpCorePlugin.Re_GetMonitors(Token)).ToList();

                    _AllInfoMonitors.Clear();
                    _AllInfoMonitors.AddRange(monitorInfos.ToList());

                    _logs.DebugMsg("[DisplayMangerPlugin] Re_GetMonitors() AllInfoMonitors.count is " + _AllInfoMonitors.Count);

                    InitializeAllALSInfo();

                    //Robert_Lin, 2024-12-10 added to notify EAPlugin
                    NotifyEAPluginAllInfoMonitorsChanged();

                    return monitorInfos;
                }
                else
                {
                    _logs.DebugMsg("[DisplayMangerPlugin] _VcpCorePlugin is null");
                    return new List<MonitorInfo>();
                }
            }
            catch (Exception ex)
            {
                //_logs.DebugMsg("[DisplayMangerPlugin] Re_GetMonitors() AllInfoMonitors Exception is " + ex.Message);
                WriteLog("Re_GetMonitors() AllInfoMonitors Exception is " + ex.Message, log_type.error);
                return new List<MonitorInfo>();
            }
        }

        public Task<List<MultiCommandArch>> MultiCommandsRun(List<MultiCommandArch> _multiCommands)
        {
            _logs.DebugMsg("[DisplayMangerPlugin] DisplayMangerPlugin received MultiCommandsRun requested ...");
            _logs.DebugMsg($"[DisplayMangerPlugin] MultiCommands count is {_multiCommands.Count}");

            var r = _multiCommands.ToList();

            if (_VcpCorePlugin != null)
                r = (_VcpCorePlugin.MultiCommandsRun(_multiCommands).Result).ToList();
            else
                _logs.DebugMsg("[DisplayMangerPlugin] _VcpCorePlugin is null");

            return Task.FromResult(r);
        }

        public Task<string> GetCapabilitiesString(MonitorInfo monitorInfo, Guid guid = default, Priority priority = Priority.Low)
        {
            _logs.DebugMsg("[DisplayMangerPlugin] DisplayMangerPlugin received GetCapabilitiesString requested ...");
            _logs.DebugMsg("[DisplayMangerPlugin] TargetMonitor DisplayName is " + monitorInfo.DisplayName);
            _logs.DebugMsg("[DisplayMangerPlugin] TargetMonitor AliasDeviceName is " + monitorInfo.AliasDeviceName);

            string r = string.Empty;

            if (_VcpCorePlugin != null)
                r = _VcpCorePlugin.GetCapabilitiesString(monitorInfo, guid, priority).Result;
            else
                _logs.DebugMsg("[DisplayMangerPlugin] _VcpCorePlugin is null");

            return Task.FromResult(r);
        }

        public Task<string> GetVCPCapabilities(MonitorInfo monitorInfo, Guid guid = default, Priority priority = Priority.Low)
        {
            _logs.DebugMsg("[DisplayMangerPlugin] DisplayMangerPlugin received GetVCPCapabilities requested ...");
            _logs.DebugMsg("[DisplayMangerPlugin] TargetMonitor DisplayName is " + monitorInfo.DisplayName);
            _logs.DebugMsg("[DisplayMangerPlugin] TargetMonitor AliasDeviceName is " + monitorInfo.AliasDeviceName);

            string r = string.Empty;

            if (_VcpCorePlugin != null)
                r = _VcpCorePlugin.GetVCPCapabilities(monitorInfo, guid, priority).Result;
            else
                _logs.DebugMsg("[DisplayMangerPlugin] _VcpCorePlugin is null");

            return Task.FromResult(r);
        }

        public Task<ObjGetVCP> GetVCPCapability(MonitorInfo monitorInfo, byte code, Guid guid = default, int opt = 0, Priority priority = Priority.Low)
        {
            _logs.DebugMsg("[DisplayMangerPlugin] DisplayMangerPlugin received GetVCPCapability requested ...");
            _logs.DebugMsg("[DisplayMangerPlugin] TargetMonitor DisplayName is " + monitorInfo.DisplayName);
            _logs.DebugMsg("[DisplayMangerPlugin] TargetMonitor AliasDeviceName is " + monitorInfo.AliasDeviceName);
            _logs.DebugMsg("[DisplayMangerPlugin] VcpCode is " + BitConverter.ToString(new byte[] { code }));
            _logs.DebugMsg("[DisplayMangerPlugin] opt is " + opt.ToString());

            ObjGetVCP result = new ObjGetVCP() { result = false, value = null };

            //Jason add 0xE9
            if (code == 0xE9 && _displayDataManger != null)
            {
                uint datacode = 1;

                if (_displayDataManger.GetMonitorE9(monitorInfo, out datacode))
                {
                    result.result = true;
                    result.value = datacode;
                    return Task.FromResult(result);
                }
            }

            if (_VcpCorePlugin != null)
            {
                result = _VcpCorePlugin.GetVCPCapability(monitorInfo, code, guid, opt, priority).Result;

                //Jason add 0xE9
                if (result.result && code == 0xE9 && _displayDataManger != null)
                    _displayDataManger.SetMonitorE9(monitorInfo, (uint)result.value);
            }
            else
                _logs.DebugMsg("[DisplayMangerPlugin] _VcpCorePlugin is null");

            return Task.FromResult(result);
        }

        public Task<ObjGetVCP> GetVCPCapability(MonitorInfo monitorInfo, string FunctionName, Guid guid = default, int opt = 0, Priority priority = Priority.Low)
        {
            _logs.DebugMsg("[DisplayMangerPlugin] DisplayMangerPlugin received GetVCPCapability requested ...");
            _logs.DebugMsg("[DisplayMangerPlugin] TargetMonitor DisplayName is " + monitorInfo.DisplayName);
            _logs.DebugMsg("[DisplayMangerPlugin] TargetMonitor AliasDeviceName is " + monitorInfo.AliasDeviceName);
            _logs.DebugMsg("[DisplayMangerPlugin] VcpCode is " + FunctionName);
            _logs.DebugMsg("[DisplayMangerPlugin] opt is " + opt.ToString());

            ObjGetVCP result = new ObjGetVCP() { result = false, value = null };

            //Jason add color
            string strColor = string.Empty;
            if (FunctionName.Equals("colorpreset") && _displayDataManger != null)
            {
                if (_displayDataManger.GetColor(monitorInfo, GetHDRStatus(monitorInfo).Result, out strColor))
                {
                    result.result = true;
                    result.value = strColor;
                    return Task.FromResult(result);
                }
            }

            if (_VcpCorePlugin != null)
            {
                result = _VcpCorePlugin.GetVCPCapability(monitorInfo, FunctionName, guid, opt, priority).Result;
                //Jason add color
                if (result.result && FunctionName.Equals("colorpreset") && _displayDataManger != null)
                {
                    _displayDataManger.SetColor(monitorInfo, GetHDRStatus(monitorInfo).Result, (string)result.value);
                }
            }
            else
                _logs.DebugMsg("[DisplayMangerPlugin] _VcpCorePlugin is null");

            return Task.FromResult(result);
        }

        public Task<bool> SetVCPCapability(MonitorInfo monitorInfo, byte code, uint val, Guid guid = default, Priority priority = Priority.Low)
        {
            _logs.DebugMsg("[DisplayMangerPlugin] DisplayMangerPlugin received SetVCPCapability requested ...");
            _logs.DebugMsg("[DisplayMangerPlugin] TargetMonitor DisplayName is " + monitorInfo.DisplayName);
            _logs.DebugMsg("[DisplayMangerPlugin] TargetMonitor AliasDeviceName is " + monitorInfo.AliasDeviceName);
            _logs.DebugMsg("[DisplayMangerPlugin] VcpCode is " + BitConverter.ToString(new byte[] { code }));
            _logs.DebugMsg("[DisplayMangerPlugin] val is " + val.ToString());

            bool r = false;

            if (_VcpCorePlugin != null)
            {
                r = _VcpCorePlugin.SetVCPCapability(monitorInfo, code, val, guid, priority).Result;

                if (_displayDataManger != null)
                {
                    //0410 Jason add 0x04
                    if (r && code == 0x04)
                        _displayDataManger.ResetDisplayData(monitorInfo, "ALL");

                    //0410 Jason add 0x05
                    if (r && code == 0x05)
                        _displayDataManger.ResetDisplayData(monitorInfo, "COLOR");

                    //Jason add 0xE9
                    if (r && code == 0xE9 && _displayDataManger != null)
                        _displayDataManger.SetMonitorE9(monitorInfo, val);
                }
            }
            else
                _logs.DebugMsg("[DisplayMangerPlugin] _VcpCorePlugin is null");

            return Task.FromResult(r);
        }

        public Task<bool> SetVCPCapability(MonitorInfo monitorInfoX, string FunctionName, string val, Guid guid = default, Priority priority = Priority.Low)
        {
            _logs.DebugMsg("[DisplayMangerPlugin] DisplayMangerPlugin received SetVCPCapability requested ...");
            _logs.DebugMsg("[DisplayMangerPlugin] TargetMonitor DisplayName is " + monitorInfoX.DisplayName);
            _logs.DebugMsg("[DisplayMangerPlugin] TargetMonitor AliasDeviceName is " + monitorInfoX.AliasDeviceName);
            _logs.DebugMsg("[DisplayMangerPlugin] FunctionName is " + FunctionName);
            _logs.DebugMsg("[DisplayMangerPlugin] val is " + val);

            bool r = false;

            if (_VcpCorePlugin != null)
            {
                r = _VcpCorePlugin.SetVCPCapability(monitorInfoX, FunctionName, val, guid, priority).Result;

                if (r && FunctionName.Equals("Input Select"))
                    _AllInfoMonitors = GetMonitors().Result;
                //04.07 Jason add color to DisplayData 
                if (r && FunctionName.Equals("colorpreset"))
                {
                    _displayDataManger.SetColor(monitorInfoX, GetHDRStatus(monitorInfoX).Result, val);
                }
            }
            else
                _logs.DebugMsg("[DisplayMangerPlugin] _VcpCorePlugin is null");

            return Task.FromResult(r);
        }

        public Task<ObjGetVCP> GetVCPCapability_NoGetCache(MonitorInfo monitorInfo, string FunctionName, Guid guid = default, int opt = 0, Priority priority = Priority.Low)
        {
            _logs.DebugMsg("[DisplayMangerPlugin] DisplayMangerPlugin received GetVCPCapability_NoGetCache requested ...");
            _logs.DebugMsg("[DisplayMangerPlugin] TargetMonitor DisplayName is " + monitorInfo.DisplayName);
            _logs.DebugMsg("[DisplayMangerPlugin] TargetMonitor AliasDeviceName is " + monitorInfo.AliasDeviceName);
            _logs.DebugMsg("[DisplayMangerPlugin] VcpCode is " + FunctionName);
            _logs.DebugMsg("[DisplayMangerPlugin] opt is " + opt.ToString());

            ObjGetVCP result = new ObjGetVCP() { result = false, value = null };

            if (_VcpCorePlugin != null)
            {
                result = _VcpCorePlugin.GetVCPCapability(monitorInfo, FunctionName, guid, opt, priority).Result;
                //Jason add color
                if (result.result && FunctionName.Equals("colorpreset") && _displayDataManger != null)
                {
                    _displayDataManger.SetColor(monitorInfo, GetHDRStatus(monitorInfo).Result, (string)result.value);
                }
            }
            else
                _logs.DebugMsg("[DisplayMangerPlugin] _VcpCorePlugin is null");

            return Task.FromResult(result);
        }

        #endregion

        #region IInputSource implementation

        /// <summary>
        /// get monitor all input source from VCPCode
        /// </summary>
        /// <param name="monitorInfo"></param>
        /// <returns>
        /// all input source dictionary
        /// </returns>
        public Task<Dictionary<string, InputInfo>> GetInputSourcelist(MonitorInfo monitorInfo)
        {
            inputSourcelist = new Dictionary<string, InputInfo>();

            int input_num = 0;
            ObjGetVCP objGetVCP = new ObjGetVCP();
            ObjGetVCP objGet = new ObjGetVCP();
            objGetVCP = GetVCPCapability(monitorInfo, 0xE7).Result;
            objGet = GetVCPCapability(monitorInfo, "inputsourcelist").Result;
            if (objGet.result)
            {
                foreach (InputSourceObject tmp in (List<InputSourceObject>)objGet.value)
                {
                    InputInfo inputInfo = new InputInfo();
                    inputInfo.InputName = tmp.Name;
                    inputInfo.Code = tmp.value;
                    inputInfo.USBUpstream = string.Empty;

                    if (inputSourcelist.ContainsKey(tmp.Name))
                    {
                        inputSourcelist[tmp.Name] = inputInfo;
                    }
                    else
                    {
                        inputSourcelist.Add(tmp.Name, inputInfo);
                    }
                    input_num = input_num + 1;
                }
            }
            return Task.FromResult(inputSourcelist);
        }

        public Task<bool> SetInputSourcelist(Dictionary<string, InputInfo> inputlist)
        {
            //set list
            inputSourcelist = inputlist;
            return Task.FromResult(true);
        }

        public Task<List<string>> GetUSBUpstreamList(MonitorInfo monitorInfo)
        {
            InputTypeString inputTypeString = new InputTypeString();
            ObjGetVCP objGetVCPEE = new ObjGetVCP();
            usbUpstreamList = new List<string>()
                /*{ "Thunderbolt", "USB-C" }*/;
            List<string> _usbUpstreamList = new List<string>();
            string subUSB = String.Empty;
            Dictionary<string, string> _USBs = new Dictionary<string, string>();
            List<USBPorts> _USBPorts = new List<USBPorts>();
            if (monitorInfo != null)
            {
                if (_displayDataManger != null)
                {
                    if (_displayDataManger.GetMonitorUSBList(monitorInfo, out _USBPorts) &&
                        _USBPorts != null && _USBPorts.Count > 0)
                    {
                        foreach (USBPorts usbPort in _USBPorts)
                        {
                            _usbUpstreamList.Add(usbPort.USBName);
                        }
                        return Task.FromResult(_usbUpstreamList);
                    }

                    if (!string.IsNullOrEmpty(monitorInfo.CapabilityString))
                    {
                        string capabilityString = monitorInfo.CapabilityString;
                        if (monitorInfo.CapabilityDic.ContainsKey("E7"))
                        {
                            if (monitorInfo.CapabilityDic.ContainsKey("EE"))
                            {
                                objGetVCPEE = GetVCPCapability(monitorInfo, 0xEE).Result;
                                if (objGetVCPEE.result)
                                {
                                    string strUSB = Convert.ToString((uint)objGetVCPEE.value, 2);
                                    if (strUSB.Length >= 4)//0708 temp solution for non-EE monitor
                                    {
                                        _USBs.Clear();
                                        for (int i = 0; i < strUSB.Length; i = i + 4)
                                        {
                                            subUSB = strUSB.Substring(i, 4);
                                            _logs.DebugMsg("[DisplayMangerPlugin] subUSB:" + subUSB);

                                            string outUSB;
                                            if (USBUplink.TryGetValue(subUSB, out outUSB))
                                            {
                                                _logs.DebugMsg("[DisplayMangerPlugin] outUSB:" + outUSB);
                                                _usbUpstreamList.Add(outUSB);
                                                string s = string.Empty;
                                                if (i == 0)
                                                {
                                                    s = "11";
                                                }
                                                else if (i == 4)
                                                {
                                                    s = "10";
                                                }
                                                else if (i == 8)
                                                {
                                                    s = "01";
                                                }
                                                else if (i == 12)
                                                {
                                                    s = "00";
                                                }
                                                _USBs.Add(outUSB, s);
                                                Trace.WriteLine("USBUpstream : " + outUSB + "," + s);
                                            }
                                        }
                                        _usbUpstreamList = inputTypeString.SubInputType(_usbUpstreamList);
                                    }
                                    USBUpstream.Clear();
                                    string str = string.Empty;
                                    foreach (var _usb in _USBs)
                                    {
                                        str = _usb.Key;
                                        if (_usb.Key == "USB-B1")
                                        {
                                            if (!_usbUpstreamList.Exists(x => x == "USB-B1"))
                                            {
                                                str = "USB-B";
                                            }
                                        }
                                        else if (_usb.Key == "USB-C1")
                                        {
                                            if (!_usbUpstreamList.Exists(x => x == "USB-C1"))
                                            {
                                                str = "USB-C";
                                            }
                                        }
                                        else if (_usb.Key == "Thunderbolt1")
                                        {
                                            if (!_usbUpstreamList.Exists(x => x == "Thunderbolt1"))
                                            {
                                                str = "Thunderbolt";
                                            }
                                        }
                                        USBUpstream.Add(str, _usb.Value);
                                        Trace.WriteLine("USBUpstream : " + str + "," + _usb.Value);
                                    }

                                    //try
                                    //{
                                    //    if (capabilityString != "" && capabilityString.Length > 10)
                                    //    {
                                    //        string[] ss = capabilityString.Split("E7(");
                                    //        ss = ss[1].Split(")");
                                    //        ss = ss[0].Split(" ");
                                    //        if (ss.Length <= _usbUpstreamList.Count)
                                    //        {
                                    //            for (int i = 0; i < ss.Length; i++)
                                    //            {
                                    //                if (ss[i].Equals("03") && USBUpstream.Count > 0)
                                    //                {
                                    //                    string usbkey = USBUpstream.FirstOrDefault(x => x.Value == "11").Key;
                                    //                    usbUpstreamList.Add(usbkey);
                                    //                    USBPorts uSBPorts = new USBPorts();
                                    //                    uSBPorts.USBName = usbkey;
                                    //                    uSBPorts.USBPort = "11";
                                    //                    _USBPorts.Add(uSBPorts);
                                    //                }
                                    //                else if (ss[i].Equals("02") && USBUpstream.Count > 0)
                                    //                {
                                    //                    string usbkey = USBUpstream.FirstOrDefault(x => x.Value == "10").Key;
                                    //                    usbUpstreamList.Add(usbkey);
                                    //                    USBPorts uSBPorts = new USBPorts();
                                    //                    uSBPorts.USBName = usbkey;
                                    //                    uSBPorts.USBPort = "10";
                                    //                    _USBPorts.Add(uSBPorts);
                                    //                }
                                    //                else if (ss[i].Equals("01") && USBUpstream.Count > 0)
                                    //                {
                                    //                    string usbkey = USBUpstream.FirstOrDefault(x => x.Value == "01").Key;
                                    //                    usbUpstreamList.Add(usbkey);
                                    //                    USBPorts uSBPorts = new USBPorts();
                                    //                    uSBPorts.USBName = usbkey;
                                    //                    uSBPorts.USBPort = "01";
                                    //                    _USBPorts.Add(uSBPorts);
                                    //                }
                                    //                else if (ss[i].Equals("00") && USBUpstream.Count > 0)
                                    //                {
                                    //                    string usbkey = USBUpstream.FirstOrDefault(x => x.Value == "00").Key;
                                    //                    usbUpstreamList.Add(usbkey);
                                    //                    USBPorts uSBPorts = new USBPorts();
                                    //                    uSBPorts.USBName = usbkey;
                                    //                    uSBPorts.USBPort = "00";
                                    //                    _USBPorts.Add(uSBPorts);
                                    //                }
                                    //            }
                                    //            _displayDataManger.SetMonitorUSBList(monitorInfo, _USBPorts);
                                    //        }
                                    //    }
                                    //}
                                    //catch
                                    //{
                                    //    usbUpstreamList = _usbUpstreamList;
                                    //    //usbUpstreamList = inputTypeString.SubInputType(_usbUpstreamList);
                                    //}
                                    // 250303 Elie: need Jason to double confirm.
                                    try
                                    {
                                        if (!string.IsNullOrEmpty(capabilityString) && capabilityString.Length > 10)
                                        {
                                            string[] ss = capabilityString.Split("E7(");
                                            if (ss.Length > 1)
                                            {
                                                ss = ss[1].Split(")");
                                                if (ss.Length > 0)
                                                {
                                                    ss = ss[0].Split(" ");
                                                    //if (ss.Length <= _usbUpstreamList.Count)
                                                    //{
                                                    foreach (var s in ss)
                                                    {
                                                        if (USBUpstream.Count > 0)
                                                        {
                                                            string usbkey = null;
                                                            string usbPort = null;

                                                            switch (s)
                                                            {
                                                                case "03":
                                                                    usbkey = USBUpstream.FirstOrDefault(x => x.Value == "11").Key;
                                                                    usbPort = "11";
                                                                    break;

                                                                case "02":
                                                                    usbkey = USBUpstream.FirstOrDefault(x => x.Value == "10").Key;
                                                                    usbPort = "10";
                                                                    break;

                                                                case "01":
                                                                    usbkey = USBUpstream.FirstOrDefault(x => x.Value == "01").Key;
                                                                    usbPort = "01";
                                                                    break;

                                                                case "00":
                                                                    usbkey = USBUpstream.FirstOrDefault(x => x.Value == "00").Key;
                                                                    usbPort = "00";
                                                                    break;
                                                            }

                                                            if (!string.IsNullOrEmpty(usbkey))
                                                            {
                                                                usbUpstreamList.Add(usbkey);
                                                                USBPorts uSBPorts = new USBPorts
                                                                {
                                                                    USBName = usbkey,
                                                                    USBPort = usbPort
                                                                };
                                                                _USBPorts.Add(uSBPorts);
                                                            }
                                                        }
                                                    }
                                                    _displayDataManger.SetMonitorUSBList(monitorInfo, _USBPorts);
                                                    //}
                                                }
                                            }
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        //_logs.DebugMsg($"[Error] Exception occurred: {ex.Message}");
                                        WriteLog($"[Error] Exception occurred: {ex.Message}", log_type.error);
                                        usbUpstreamList = _usbUpstreamList;
                                        //usbUpstreamList = inputTypeString.SubInputType(_usbUpstreamList);
                                    }
                                }
                            }
                            else
                            {
                                try
                                {
                                    if (capabilityString != "" && capabilityString.Length > 10)
                                    {
                                        string[] ss = capabilityString.Split("E7(");
                                        ss = ss[1].Split(")");
                                        ss = ss[0].Split(" ");
                                        if (ss.Length > 0)
                                        {
                                            for (int i = 0; i < ss.Length; i++)
                                            {
                                                if (ss[i].Equals("03"))
                                                {
                                                    usbUpstreamList.Add(USBUplink_E7["11"]);
                                                }
                                                else if (ss[i].Equals("02"))
                                                {
                                                    usbUpstreamList.Add(USBUplink_E7["10"]);
                                                }
                                                else if (ss[i].Equals("01"))
                                                {
                                                    usbUpstreamList.Add(USBUplink_E7["01"]);
                                                }
                                                else if (ss[i].Equals("00"))
                                                {
                                                    usbUpstreamList.Add(USBUplink_E7["00"]);
                                                }
                                            }
                                            usbUpstreamList = inputTypeString.SubInputType(usbUpstreamList);

                                            USBUpstream.Clear();
                                            foreach (var _usb in usbUpstreamList)
                                            {
                                                if (_usb == "USB-B1" || _usb == "USB-B")
                                                {
                                                    USBUpstream.Add(_usb, "00");
                                                    USBPorts uSBPorts = new USBPorts();
                                                    uSBPorts.USBName = _usb;
                                                    uSBPorts.USBPort = "00";
                                                    _USBPorts.Add(uSBPorts);
                                                }
                                                else if (_usb == "USB-B2")
                                                {
                                                    USBUpstream.Add(_usb, "01");
                                                    USBPorts uSBPorts = new USBPorts();
                                                    uSBPorts.USBName = _usb;
                                                    uSBPorts.USBPort = "01";
                                                    _USBPorts.Add(uSBPorts);
                                                }
                                                else if (_usb == "USB-C1" || _usb == "USB-C")
                                                {
                                                    USBUpstream.Add(_usb, "10");
                                                    USBPorts uSBPorts = new USBPorts();
                                                    uSBPorts.USBName = _usb;
                                                    uSBPorts.USBPort = "10";
                                                    _USBPorts.Add(uSBPorts);
                                                }
                                                else if (_usb == "USB-C2")
                                                {
                                                    USBUpstream.Add(_usb, "11");
                                                    USBPorts uSBPorts = new USBPorts();
                                                    uSBPorts.USBName = _usb;
                                                    uSBPorts.USBPort = "11";
                                                    _USBPorts.Add(uSBPorts);
                                                }
                                            }
                                            _displayDataManger.SetMonitorUSBList(monitorInfo, _USBPorts);
                                        }
                                    }
                                }
                                catch
                                {
                                    usbUpstreamList = _usbUpstreamList;
                                }
                            }
                        }
                    }
                    else
                    {
                        _logs.DebugMsg("[DisplayMangerPlugin][GetUSBUpstreamList] CapabilityString is null or empty.");
                    }
                }
            }
            else
            {
                _logs.DebugMsg("[DisplayMangerPlugin][GetUSBUpstreamList] monitorInfo is null.");
            }
            if (usbUpstreamList == null || usbUpstreamList.Count == 0)
            {
                usbUpstreamList = _usbUpstreamList;
            }

            return Task.FromResult(usbUpstreamList);
        }

        public Task<string> GetUSBUpstream(MonitorInfo monitorInfo, string inputsource)
        {
            string usbUpstream = string.Empty;
            _displayDataManger.GetMonitorUSB(monitorInfo, inputsource, out usbUpstream);
            if (!string.IsNullOrEmpty(usbUpstream))
            {
                return Task.FromResult(usbUpstream);
            }
            int input_num = 0;
            ObjGetVCP objGetVCP = new ObjGetVCP();
            InputSource_USB inputSource_USB = new InputSource_USB();
            if (monitorInfo.CapabilityDic.ContainsKey("E7"))
            {
                usbUpstreamList = GetUSBUpstreamList(monitorInfo).Result;
                if (usbUpstreamList != null && usbUpstreamList.Count > 0)
                {
                    objGetVCP = GetVCPCapability(monitorInfo, 0xE7).Result;
                    if (objGetVCP.result)
                    {
                        if (_getVCPCapabilities == string.Empty)
                        {
                            _getVCPCapabilities = GetVCPCapabilities(monitorInfo).Result;
                        }
                        if (!string.IsNullOrEmpty(_getVCPCapabilities))
                        {
                            JObject VCPjson = JObject.Parse(_getVCPCapabilities);

                            JObject capsDataMap = (JObject)VCPjson["CapsDataMap"];
                            JArray input = (JArray)capsDataMap["Input Select"];
                            foreach (var tmp in input)
                            {
                                if (tmp.ToString() == inputsource)
                                {
                                    break;
                                }
                                input_num = input_num + 1;
                            }

                            string getUpstream = Convert.ToString((uint)objGetVCP.value, 2);
                            string newstrUpstream = getUpstream;
                            if (getUpstream.Length < 16)
                            {
                                for (int i = 0; i < (16 - getUpstream.Length); i++)
                                {
                                    newstrUpstream = "0" + newstrUpstream;
                                }
                            }
                            Trace.WriteLine(newstrUpstream);
                            _logs.DebugMsg("[DisplayMangerPlugin][GetUSBUpstream] newstrUpstream : " + newstrUpstream);
                            _logs.DebugMsg("[DisplayMangerPlugin][GetUSBUpstream] input_num : " + input_num);
                            if (newstrUpstream.Length == 16)
                            {
                                string subUpstream = string.Empty;
                                if (monitorInfo.CapabilityDic.ContainsKey("EE"))
                                {
                                    subUpstream = newstrUpstream.Substring(input_num * 2, 2);
                                }
                                else
                                {
                                    subUpstream = newstrUpstream.Substring(14 - (input_num * 2), 2);
                                }
                                if (!string.IsNullOrEmpty(subUpstream))
                                {
                                    List<USBPorts> ports = new List<USBPorts>();
                                    _displayDataManger.GetMonitorUSBList(monitorInfo, out ports);
                                    if (ports != null && ports.Count != 0)
                                    {
                                        foreach (var tmp in ports)
                                        {
                                            if (subUpstream == tmp.USBPort)
                                            {
                                                inputSource_USB.inputSource = inputsource;
                                                inputSource_USB.USB = tmp.USBName;
                                                _displayDataManger.SetMonitorUSB(monitorInfo, inputSource_USB);
                                                return Task.FromResult(tmp.USBName);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        _logs.DebugMsg("[DisplayMangerPlugin][GetUSBUpstream] ports is null or Count = 0");
                                    }
                                }
                            }
                            else
                            {
                                _logs.DebugMsg("[DisplayMangerPlugin][GetUSBUpstream] newstrUpstream length is not 16");
                            }
                        }
                        else
                        {
                            _logs.DebugMsg("[DisplayMangerPlugin][GetUSBUpstream] _getVCPCapabilities is null or empty.");
                        }
                    }
                    else
                    {
                        _logs.DebugMsg("[DisplayMangerPlugin][GetUSBUpstream] GetVCPCapability 0xE7 fail.");
                    }
                }
                else
                {
                    _logs.DebugMsg("[DisplayMangerPlugin][GetUSBUpstream] usbUpstreamList is null or count = 0.");
                }
            }
            return Task.FromResult("");
        }

        public Task<bool> GetAllUSBUpstream(MonitorInfo monitorInfo)
        {
            string usbUpstream = string.Empty;
            int input_num = 0;
            ObjGetVCP objGetVCP = new ObjGetVCP();
            List<InputSource_USB> input_USBList = new List<InputSource_USB>();
            if (monitorInfo.CapabilityDic.ContainsKey("E7"))
            {
                if (_displayDataManger != null)
                {
                    _displayDataManger.GetMonitorUSB(monitorInfo, out input_USBList);
                    if (input_USBList != null && input_USBList.Count > 0)
                    {
                        return Task.FromResult(true);
                    }
                }
                usbUpstreamList = GetUSBUpstreamList(monitorInfo).Result;
                if (usbUpstreamList != null && usbUpstreamList.Count > 0)
                {
                    objGetVCP = GetVCPCapability(monitorInfo, 0xE7).Result;
                    if (objGetVCP.result)
                    {
                        if (_getVCPCapabilities == string.Empty)
                        {
                            _getVCPCapabilities = GetVCPCapabilities(monitorInfo).Result;
                        }
                        if (!string.IsNullOrEmpty(_getVCPCapabilities))
                        {
                            JObject VCPjson = JObject.Parse(_getVCPCapabilities);

                            JObject capsDataMap = (JObject)VCPjson["CapsDataMap"];
                            JArray input = (JArray)capsDataMap["Input Select"];

                            string getUpstream = Convert.ToString((uint)objGetVCP.value, 2);
                            string newstrUpstream = getUpstream;
                            if (getUpstream.Length < 16)
                            {
                                for (int i = 0; i < (16 - getUpstream.Length); i++)
                                {
                                    newstrUpstream = "0" + newstrUpstream;
                                }
                            }

                            Trace.WriteLine(newstrUpstream);
                            _logs.DebugMsg("[DisplayMangerPlugin][GetUSBUpstream] newstrUpstream : " + newstrUpstream);

                            if (newstrUpstream.Length == 16)
                            {
                                foreach (var tmp in input)
                                {
                                    string subUpstream = string.Empty;
                                    if (monitorInfo.CapabilityDic.ContainsKey("EE"))
                                    {
                                        subUpstream = newstrUpstream.Substring(input_num * 2, 2);
                                    }
                                    else
                                    {
                                        subUpstream = newstrUpstream.Substring(14 - (input_num * 2), 2);
                                    }
                                    Trace.WriteLine(subUpstream);
                                    if (!string.IsNullOrEmpty(subUpstream))
                                    {
                                        List<USBPorts> ports = new List<USBPorts>();
                                        _displayDataManger.GetMonitorUSBList(monitorInfo, out ports);
                                        if (ports != null && ports.Count != 0)
                                        {
                                            foreach (var tmp1 in ports)
                                            {
                                                if (subUpstream == tmp1.USBPort)
                                                {
                                                    InputSource_USB inputSource_USB = new InputSource_USB();
                                                    inputSource_USB.inputSource = tmp.ToString();
                                                    Trace.WriteLine(inputSource_USB.inputSource);
                                                    inputSource_USB.USB = tmp1.USBName;
                                                    Trace.WriteLine(inputSource_USB.USB);
                                                    input_USBList.Add(inputSource_USB);
                                                    break;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            _logs.DebugMsg("[DisplayMangerPlugin][GetUSBUpstream] ports is null or Count = 0");
                                        }
                                    }
                                    input_num++;
                                }
                                if (_displayDataManger != null)
                                {
                                    _displayDataManger.SetMonitorUSB(monitorInfo, input_USBList);
                                    return Task.FromResult(true);
                                }
                            }
                            else
                            {
                                _logs.DebugMsg("[DisplayMangerPlugin][GetUSBUpstream] newstrUpstream length is not 16");
                            }
                        }
                        else
                        {
                            _logs.DebugMsg("[DisplayMangerPlugin][GetUSBUpstream] _getVCPCapabilities is null or empty.");
                        }
                    }
                    else
                    {
                        _logs.DebugMsg("[DisplayMangerPlugin][GetUSBUpstream] GetVCPCapability 0xE7 fail.");
                    }
                }
                else
                {
                    _logs.DebugMsg("[DisplayMangerPlugin][GetUSBUpstream] usbUpstreamList is null or count = 0.");
                }
            }
            return Task.FromResult(false);
        }

        public Task<bool> SetUSBUpstream(MonitorInfo monitorInfo, string inputsource, string upstream)
        {
            //inputSourcelist[input].USBUpstream = upstream;
            int input_num = 0;
            ObjGetVCP objGetVCP = new ObjGetVCP();
            InputSource_USB inputSource_USB = new InputSource_USB();
            if (monitorInfo.CapabilityDic.ContainsKey("E7"))
            {
                objGetVCP = GetVCPCapability(monitorInfo, 0xE7).Result;

                if (objGetVCP.result)
                {
                    if (_getVCPCapabilities == string.Empty)
                    {
                        _getVCPCapabilities = GetVCPCapabilities(monitorInfo).Result;
                    }
                    if (!string.IsNullOrEmpty(_getVCPCapabilities))
                    {
                        usbUpstreamList = GetUSBUpstreamList(monitorInfo).Result;
                        JObject VCPjson = JObject.Parse(_getVCPCapabilities);

                        JObject capsDataMap = (JObject)VCPjson["CapsDataMap"];
                        JArray input = (JArray)capsDataMap["Input Select"];
                        foreach (var tmp in input)
                        {
                            if (tmp.ToString() == inputsource)
                            {
                                break;
                            }
                            input_num = input_num + 1;
                        }
                        if (input_num != 0)
                        {
                            string getUpstream = Convert.ToString((uint)objGetVCP.value, 2);
                            string newstrUpstream = getUpstream;
                            if (getUpstream.Length < 16)
                            {
                                for (int i = 0; i < (16 - getUpstream.Length); i++)
                                {
                                    newstrUpstream = "0" + newstrUpstream;
                                }
                            }
                            List<USBPorts> uSBPorts = new List<USBPorts>();
                            _displayDataManger.GetMonitorUSBList(monitorInfo, out uSBPorts);
                            if (uSBPorts != null && uSBPorts.Count != 0)
                            {
                                string strsetUpstream = string.Empty;
                                int index = uSBPorts.FindIndex(x => x.USBName == upstream);
                                if (monitorInfo.CapabilityDic.ContainsKey("EE"))
                                {
                                    _logs.DebugMsg("[DisplayMangerPlugin][SetUSBUpstream] 0xEE.");
                                    strsetUpstream = newstrUpstream.Substring(0, input_num * 2) + uSBPorts[index].USBPort + newstrUpstream.Substring((input_num + 1) * 2, newstrUpstream.Length - ((input_num + 1) * 2));
                                }
                                else
                                {
                                    Trace.WriteLine("input_num:" + input_num.ToString());
                                    strsetUpstream = newstrUpstream.Substring(0, newstrUpstream.Length - ((input_num + 1) * 2)) + uSBPorts[index].USBPort + newstrUpstream.Substring(newstrUpstream.Length - (input_num * 2));
                                }
                                if (!string.IsNullOrEmpty(strsetUpstream))
                                {
                                    uint code = Convert.ToUInt16(strsetUpstream, 2);
                                    Trace.WriteLine("strsetUpstream:" + code.ToString());
                                    bool b = SetVCPCapability(monitorInfo, 0xE7, code).Result;
                                    if (b)
                                    {
                                        inputSource_USB.inputSource = inputsource;
                                        inputSource_USB.USB = upstream;
                                        _displayDataManger.SetMonitorUSB(monitorInfo, inputSource_USB);
                                    }
                                    return Task.FromResult(b);
                                }
                            }
                            else
                            {
                                _logs.DebugMsg("[DisplayMangerPlugin][SetUSBUpstream] uSBPorts is null or Count = 0");
                            }
                        }
                    }
                }
            }
            return Task.FromResult(false);
        }

        public Task<bool> SetAllUSBUpstream(MonitorInfo monitorInfo, string input1, string usb1, string input2, string usb2,
                                                                     string input3, string usb3, string input4, string usb4)
        {
            int input_num = 0;
            ObjGetVCP objGetVCP = new ObjGetVCP();
            List<InputSource_USB> allinputSource_USB = new List<InputSource_USB>();
            if (monitorInfo.CapabilityDic.ContainsKey("E7"))
            {
                objGetVCP = GetVCPCapability(monitorInfo, 0xE7).Result;

                if (objGetVCP.result)
                {
                    if (_getVCPCapabilities == string.Empty)
                    {
                        _getVCPCapabilities = GetVCPCapabilities(monitorInfo).Result;
                    }
                    if (!string.IsNullOrEmpty(_getVCPCapabilities))
                    {
                        usbUpstreamList = GetUSBUpstreamList(monitorInfo).Result;
                        JObject VCPjson = JObject.Parse(_getVCPCapabilities);

                        JObject capsDataMap = (JObject)VCPjson["CapsDataMap"];
                        JArray input = (JArray)capsDataMap["Input Select"];
                        string getUpstream = Convert.ToString((uint)objGetVCP.value, 2);
                        string newstrUpstream = getUpstream;
                        string strsetUpstream = string.Empty;
                        if (getUpstream.Length < 16)
                        {
                            for (int i = 0; i < (16 - getUpstream.Length); i++)
                            {
                                newstrUpstream = "0" + newstrUpstream;
                            }
                        }
                        List<USBPorts> uSBPorts = new List<USBPorts>();
                        _displayDataManger.GetMonitorUSBList(monitorInfo, out uSBPorts);
                        if (uSBPorts != null && uSBPorts.Count != 0)
                        {
                            strsetUpstream = newstrUpstream;
                            Trace.WriteLine("strsetUpstream:" + strsetUpstream);
                            if (monitorInfo.CapabilityDic.ContainsKey("EE"))
                            {
                                foreach (var tmp in input)
                                {
                                    Trace.WriteLine("tmp:" + tmp.ToString());
                                    int index = -1;
                                    Trace.WriteLine("input_num:" + input_num.ToString());
                                    if (tmp.ToString() == input1)
                                    {
                                        index = uSBPorts.FindIndex(x => x.USBName == usb1);
                                        strsetUpstream = strsetUpstream.Substring(0, input_num * 2) + uSBPorts[index].USBPort + strsetUpstream.Substring((input_num + 1) * 2, strsetUpstream.Length - ((input_num + 1) * 2));
                                        Trace.WriteLine("strsetUpstream:" + strsetUpstream);
                                    }
                                    else if (tmp.ToString() == input2)
                                    {
                                        index = uSBPorts.FindIndex(x => x.USBName == usb2);
                                        strsetUpstream = strsetUpstream.Substring(0, input_num * 2) + uSBPorts[index].USBPort + strsetUpstream.Substring((input_num + 1) * 2, strsetUpstream.Length - ((input_num + 1) * 2));
                                        Trace.WriteLine("strsetUpstream:" + strsetUpstream);
                                    }
                                    else if (tmp.ToString() == input3)
                                    {
                                        index = uSBPorts.FindIndex(x => x.USBName == usb3);
                                        strsetUpstream = strsetUpstream.Substring(0, input_num * 2) + uSBPorts[index].USBPort + strsetUpstream.Substring((input_num + 1) * 2, strsetUpstream.Length - ((input_num + 1) * 2));
                                        Trace.WriteLine("strsetUpstream:" + strsetUpstream);
                                    }
                                    else if (tmp.ToString() == input4)
                                    {
                                        index = uSBPorts.FindIndex(x => x.USBName == usb4);
                                        strsetUpstream = strsetUpstream.Substring(0, input_num * 2) + uSBPorts[index].USBPort + strsetUpstream.Substring((input_num + 1) * 2, strsetUpstream.Length - ((input_num + 1) * 2));
                                        Trace.WriteLine("strsetUpstream:" + strsetUpstream);
                                    }
                                    input_num = input_num + 1;
                                }
                            }
                            else
                            {
                                foreach (var tmp in input)
                                {
                                    Trace.WriteLine("tmp:" + tmp.ToString());
                                    int index = -1;
                                    Trace.WriteLine("input_num:" + input_num.ToString());
                                    if (tmp.ToString() == input1)
                                    {
                                        index = uSBPorts.FindIndex(x => x.USBName == usb1);
                                        strsetUpstream = strsetUpstream.Substring(0, strsetUpstream.Length - ((input_num + 1) * 2)) + uSBPorts[index].USBPort + strsetUpstream.Substring(strsetUpstream.Length - (input_num * 2));
                                        Trace.WriteLine("strsetUpstream:" + strsetUpstream);
                                    }
                                    else if (tmp.ToString() == input2)
                                    {
                                        index = uSBPorts.FindIndex(x => x.USBName == usb2);
                                        strsetUpstream = strsetUpstream.Substring(0, strsetUpstream.Length - ((input_num + 1) * 2)) + uSBPorts[index].USBPort + strsetUpstream.Substring(strsetUpstream.Length - (input_num * 2));
                                        Trace.WriteLine("strsetUpstream:" + strsetUpstream);
                                    }
                                    else if (tmp.ToString() == input3)
                                    {
                                        index = uSBPorts.FindIndex(x => x.USBName == usb3);
                                        strsetUpstream = strsetUpstream.Substring(0, strsetUpstream.Length - ((input_num + 1) * 2)) + uSBPorts[index].USBPort + strsetUpstream.Substring(strsetUpstream.Length - (input_num * 2));
                                        Trace.WriteLine("strsetUpstream:" + strsetUpstream);
                                    }
                                    else if (tmp.ToString() == input4)
                                    {
                                        index = uSBPorts.FindIndex(x => x.USBName == usb4);
                                        strsetUpstream = strsetUpstream.Substring(0, strsetUpstream.Length - ((input_num + 1) * 2)) + uSBPorts[index].USBPort + strsetUpstream.Substring(strsetUpstream.Length - (input_num * 2));
                                        Trace.WriteLine("strsetUpstream:" + strsetUpstream);
                                    }
                                    input_num = input_num + 1;
                                }
                            }
                            if (!string.IsNullOrEmpty(strsetUpstream))
                            {
                                uint code = Convert.ToUInt16(strsetUpstream, 2);
                                Trace.WriteLine("strsetUpstream:" + code.ToString());
                                bool b = SetVCPCapability(monitorInfo, 0xE7, code).Result;
                                if (b)
                                {
                                    _displayDataManger.GetMonitorUSB(monitorInfo, out allinputSource_USB);
                                    foreach (InputSource_USB inputSource_USB in allinputSource_USB)
                                    {
                                        if (inputSource_USB.inputSource == input1)
                                        {
                                            inputSource_USB.USB = usb1;
                                        }
                                        else if (inputSource_USB.inputSource == input2)
                                        {
                                            inputSource_USB.USB = usb2;
                                        }
                                        else if (inputSource_USB.inputSource == input3)
                                        {
                                            inputSource_USB.USB = usb3;
                                        }
                                        else if (inputSource_USB.inputSource == input4)
                                        {
                                            inputSource_USB.USB = usb4;
                                        }
                                    }
                                }
                                return Task.FromResult(b);
                            }
                        }
                    }
                }
            }
            return Task.FromResult(false);
        }

        public Task ChangeCurrentInput(Dictionary<string, InputInfo> inputSource, string input)
        {
            //set VCP
            //InputPluginEvent?.Invoke(this, new strEventArgs(input));
            return Task.CompletedTask;
        }

        public Task<string> GetCurrentInput(MonitorInfo monitorInfo, Guid guid = default, Priority priority = Priority.Low)
        {
            ObjGetVCP objGetVCP = GetVCPCapability(monitorInfo, "input select", guid, priority: priority).Result;
            if (objGetVCP != null && objGetVCP.result)
            {
                _logs.DebugMsg("[DisplayManger]CurrentInput:" + objGetVCP.value.ToString());
                string currentInpt = objGetVCP.value.ToString();
                return Task.FromResult(currentInpt);
            }
            return Task.FromResult("");
        }

        public Task<bool> USBSwitch(MonitorInfo monitorInfo, string inputsource1, string upstream1, string inputsource2, string upstream2)
        {
            int input_num = 0;
            int input_num1 = 0;
            int input_num2 = 0;
            ObjGetVCP objGetVCP = new ObjGetVCP();
            objGetVCP = GetVCPCapability(monitorInfo, 0xE7).Result;

            if (objGetVCP.result)
            {
                if (_getVCPCapabilities == string.Empty)
                {
                    _getVCPCapabilities = GetVCPCapabilities(monitorInfo).Result;
                }
                if (!string.IsNullOrEmpty(_getVCPCapabilities))
                {
                    JObject VCPjson = JObject.Parse(_getVCPCapabilities);

                    JObject capsDataMap = (JObject)VCPjson["CapsDataMap"];
                    JArray input = (JArray)capsDataMap["Input Select"];
                    foreach (var tmp in input)
                    {
                        if (tmp.ToString() == inputsource1)
                        {
                            input_num1 = input_num;
                        }
                        if (tmp.ToString() == inputsource2)
                        {
                            input_num2 = input_num;
                        }
                        input_num = input_num + 1;
                    }
                    if (input_num1 != 0 && input_num2 != 0)
                    {
                        string getUpstream = Convert.ToString((uint)objGetVCP.value, 2);
                        string newstrUpstream = getUpstream;
                        if (getUpstream.Length < 16)
                        {
                            for (int i = 0; i < (16 - getUpstream.Length); i++)
                            {
                                newstrUpstream = "0" + newstrUpstream;
                            }
                        }
                        string strsetUpstream1 = newstrUpstream.Substring(0, input_num1 * 2) + USBUpstream[upstream1] + newstrUpstream.Substring((input_num1 + 1) * 2, newstrUpstream.Length - ((input_num1 + 1) * 2));
                        string strsetUpstream2 = strsetUpstream1.Substring(0, input_num2 * 2) + USBUpstream[upstream2] + strsetUpstream1.Substring((input_num2 + 1) * 2, strsetUpstream1.Length - ((input_num2 + 1) * 2));
                        uint code = Convert.ToUInt16(strsetUpstream2, 2);
                        bool b = SetVCPCapability(monitorInfo, 0xE7, code).Result;
                        return Task.FromResult(b);
                    }
                }
            }
            return Task.FromResult(false);
        }

        public Task<bool> isScreenPartition(MonitorInfo monitorInfo, Guid guid = default, Priority priority = Priority.Low)
        {
            ObjGetVCP objGetVCP = GetVCPCapability(monitorInfo, 0xF2, guid, priority: priority).Result;
            if (objGetVCP != null &&
                objGetVCP.result &&
                (uint)objGetVCP.value != 0)
            {
                string strSP = Convert.ToString((uint)objGetVCP.value, 2);
                string strSP_16 = strSP;
                //add 16 to string
                if (strSP.Length < 16)
                {
                    for (int i = 0; i < (16 - strSP.Length); i++)
                    {
                        strSP_16 = "0" + strSP_16;
                    }
                }
                _logs.DebugMsg("[DisplayMangerPlugin][isScreenPartition] strSP_16 : " + strSP_16);
                //find 8
                if (strSP_16.Length == 16 &&
                    strSP_16.Substring(7, 1) == "1")
                {
                    return Task.FromResult(true);
                }
            }
            return Task.FromResult(false);
        }

        #region ALS Function

        /// <summary>
        /// Initialize All ALS monitor Info data on start up
        /// </summary>
        private void InitializeAllALSInfo()
        {
            Task.Run(() =>
            {
                AllALSConfig.Clear();
                _logs.DebugMsg("[DisplayMangerPlugin] InitializeAllALSInfo() AllALSConfig Clear ... in");
                List<MonitorInfo> monitorALS = GetMonitors().Result;
                try
                {
                    //foreach (MonitorInfo als in monitorALS)
                    for (int i = 0; i < monitorALS.Count; i++)
                    {
                        MonitorInfo als = monitorALS[i]; //Dean 0614 fix exception [Collection was modified; enumeration operation may not execute.]

                        ALSConfig als_param = new ALSConfig();
                        //GetALSMMS(als, ref als_param);
                        //Dean 0624 move down get ALS settings before check if support
                        GetALSupport(als, ref als_param);
                        if (als_param.result == true && als_param.isSupportALS != 0)//Read the ALS value only if ALS is supported
                        {
                            GetALSAll(als, ref als_param);
                        }
                        else
                        {
                            ParseMonitorInfo(als, ref als_param);
                        }
                        AllALSConfig.Add(als_param);
                        _logs.DebugMsg($"[DisplayMangerPlugin] InitializeAllALSInfo() {i.ToString()} : AllALSConfig {AllALSConfig[i].Edid.ModelName} {AllALSConfig[i].Edid.SerialNumber} {AllALSConfig[i].Edid.ServiceTag}");
                    }
                    //If Monitors have Primary, need to sync ALS data to other monitor
                    if (AllALSConfig.Count > 0)
                    {
                        var firstPrimaryMonitorSyncConfig = AllALSConfig.FirstOrDefault(config => config.isPrimaryMonitorSync);
                        if (firstPrimaryMonitorSyncConfig != null)
                        {
                            uint newVal = SetBitValue(firstPrimaryMonitorSyncConfig.AllValue, 5, 0);
                            _logs.DebugMsg($"[DisplayMangerPlugin] InitializeAllALSInfo() Primary Monitor ModelName = {firstPrimaryMonitorSyncConfig.Edid.ModelName}, SerialNumber = {firstPrimaryMonitorSyncConfig.Edid.SerialNumber}, ServiceTag = {firstPrimaryMonitorSyncConfig.Edid.ServiceTag}, AllValue = {firstPrimaryMonitorSyncConfig.AllValue}");
                            for (int i = 0; i < AllALSConfig.Count; i++)
                            {
                                //MonitorInfo tmp = monitorALS.Find(x => x.edid.Equals(AllALSConfig[i].Edid));
                                if (!firstPrimaryMonitorSyncConfig.MoInfo.Equals(AllALSConfig[i].MoInfo))//If find Primary, do not need do this.
                                {
                                    _logs.DebugMsg($"[DisplayMangerPlugin] InitializeAllALSInfo() Non Primary Monitor ModelName = {AllALSConfig[i].MoInfo.edid.ModelName}, SerialNumber = {AllALSConfig[i].MoInfo.edid.SerialNumber}, ServiceTag = {AllALSConfig[i].MoInfo.edid.ServiceTag}, AllValue = {AllALSConfig[i].AllValue} will change to {newVal.ToString()}");
                                    if (SetVCPCapability(AllALSConfig[i].MoInfo, 0x66, newVal).Result)//set ALS value, but bit5 need to change to 0
                                    {
                                        _logs.DebugMsg($"[DisplayMangerPlugin] InitializeAllALSInfo() SetVCPCapability 0x66 value {newVal.ToString()}, result = true ... ");
                                    }
                                    else
                                    {
                                        _logs.DebugMsg($"[DisplayMangerPlugin] InitializeAllALSInfo() SetVCPCapability 0x66 {newVal.ToString()}, result = false ... ");
                                    }
                                    _logs.DebugMsg($"[DisplayMangerPlugin] InitializeAllALSInfo() Sync ALS Data Finish ... ");

                                    ObjGetVCP obColor = GetVCPCapability(firstPrimaryMonitorSyncConfig.MoInfo, "colorpreset").Result;

                                    if (obColor.result)
                                    {
                                        if (SetVCPCapability(AllALSConfig[i].MoInfo, "colorpreset", obColor.value.ToString()).Result)
                                        {
                                            _logs.DebugMsg($"[DisplayMangerPlugin] InitializeAllALSInfo() SetVCPCapability colorpreset = {obColor.value.ToString()}, true  {AllALSConfig[i].MoInfo.modelName.ToString()} ... ");
                                        }
                                        else
                                        {
                                            _logs.DebugMsg($"[DisplayMangerPlugin] InitializeAllALSInfo() SetVCPCapability colorpreset = {obColor.value.ToString()}, fail  {AllALSConfig[i].MoInfo.modelName.ToString()} ... ");
                                        }
                                    }
                                    else
                                    {
                                        _logs.DebugMsg($"[DisplayMangerPlugin] InitializeAllALSInfo() GetVCPCapability colorpreset = {obColor.value.ToString()}, fail  {AllALSConfig[i].MoInfo.modelName.ToString()} ... ");
                                    }
                                    _logs.DebugMsg($"[DisplayMangerPlugin] InitializeAllALSInfo() Sync colorpreset Data Finish ... ");
                                }
                            }
                        }
                        else
                        {
                            _logs.DebugMsg($"[DisplayMangerPlugin] InitializeAllALSInfo() Non Find Primary Monitor ... ");
                            return;
                        }
                    }
                    _logs.DebugMsg("[DisplayMangerPlugin] InitializeAllALSInfo ... Monitor.Count = " + monitorALS.Count.ToString() + " || AllALSConfig.Count = " + AllALSConfig.Count.ToString());
                }
                catch (Exception ex)
                {
                    //_logs.DebugMsg($"[DisplayMangerPlugin][InitializeAllALSInfo] Init ALSConfig got exception. {ex}");
                    WriteLog($"[DisplayMangerPlugin][InitializeAllALSInfo] Init ALSConfig got exception. {ex.Message}", log_type.error);
                }
            });
        }

        /// <summary>
        /// Use VCP command to Get ALS Feature Value
        /// </summary>
        /// <param name="monitorInfos">monitor Info</param>
        /// <param name="type">ALSFeatureQueryType</param>
        /// <param name="val">value</param>
        /// <returns></returns>
        public Task<ALSConfig> GetALSFeatureValue(MonitorInfo monitorInfos, ALSFeatureQueryType type, int value)
        {
            _logs.DebugMsg($"[DisplayMangerPlugin] GetALSFeatureValue ... in");
            if (type != ALSFeatureQueryType.MMS)
            {
                //Dean 0624, the comparison should with DisplayName and SerialNumber
                ALSConfig aconfig = AllALSConfig.Find(x => x.Edid.Equals(monitorInfos.edid));// && x.serialNumber.ToUpper().Equals(monitorInfos.edid.SerialNumber.ToUpper()));
                if (aconfig == null)
                {
                    aconfig = new ALSConfig(); // 2024-06-11 Wayn fixed.
                    GetALSupport(monitorInfos, ref aconfig); //fixed releate PIMS-287891
                    if (aconfig.result == true && aconfig.isSupportALS != 0)//Read the ALS value only if ALS is supported
                    {
                        GetALSAll(monitorInfos, ref aconfig);
                    }
                    ParseMonitorInfo(monitorInfos, ref aconfig);
                    AllALSConfig.Add(aconfig);
                    _logs.DebugMsg($"[DisplayMangerPlugin] GetALSFeatureValue ModelName = {aconfig.ModelName} , SerialNumber = {aconfig.serialNumber}");
                }
                _logs.DebugMsg($"[DisplayMangerPlugin] GetALSFeatureValue ... out ");
                return Task.FromResult(aconfig);
            }
            else//get MMS
            {
                ALSConfig cfg = new ALSConfig();
                GetALSMMS(monitorInfos, ref cfg);
                _logs.DebugMsg($"[DisplayMangerPlugin] GetALSFeatureValue ... Else out ");
                return Task.FromResult(cfg);
            }
        }

        /// <summary>
        /// Use VCP command to Set ALS Feature Value
        /// </summary>
        /// <param name="monitorInfos">monitor Info</param>
        /// <param name="param">ALSConfig type</param>
        /// <param name="type">ALSFeatureQueryType</param>
        /// <param name="value">value</param>
        /// <returns></returns>
        public Task<bool> SetALSFeatureValue(MonitorInfo monitorInfos, ref ALSConfig param, ALSFeatureQueryType type, string value)
        {
            _logs.DebugMsg("[DisplayMangerPlugin] SetALSFeatureValue ... in " + monitorInfos.edid.ModelName.ToString() + " || type = " + type.ToString());
            _logs.DebugMsg($"[DisplayMangerPlugin] SetALSFeatureValue AllValue = {param.AllValue.ToString()}, value = {value}");
            _logs.DebugMsg($"[DisplayMangerPlugin] SetALSFeatureValue AutoBrightness . = {param.isAutoBrightness.ToString()}");
            _logs.DebugMsg($"[DisplayMangerPlugin] SetALSFeatureValue AutoColorTemp .. = {param.isAutoColorTemp.ToString()}");
            _logs.DebugMsg($"[DisplayMangerPlugin] SetALSFeatureValue RangeLevel Value = {param.AutoBrightnessRangeLevel.level_value.ToString()}");
            _logs.DebugMsg($"[DisplayMangerPlugin] SetALSFeatureValue PrimaryMonitor . = {param.isPrimaryMonitorSync.ToString()}");
            Trace.WriteLine("[DisplayMangerPlugin] SetALSFeatureValue ... in " + monitorInfos.edid.ModelName.ToString() + " || type = " + type.ToString());
            switch (type)
            {
                case ALSFeatureQueryType.MMS:
                    SetALSMMS(monitorInfos, ref param, value);
                    break;

                case ALSFeatureQueryType.PrimaryMonitorSync:
                    SetALSPrimaryMS(monitorInfos, ref param, value);
                    break;

                case ALSFeatureQueryType.AutoColorTemperature:
                    SetALSAutoColorTemp(monitorInfos, ref param, value);
                    break;

                case ALSFeatureQueryType.AutoBrightness:
                    SetALSAutoBrightness(monitorInfos, ref param, value);
                    break;

                case ALSFeatureQueryType.AutoBrightnessRangeLevel: //CLI: Mark 0723
                    SetALSAutoBrightnessRangeLevel(monitorInfos, ref param, value);
                    break;

                case ALSFeatureQueryType.All:
                    SetALSAll(monitorInfos, ref param, value);
                    break;

                default:
                    return Task.FromResult(false);
            }

            if (AllALSConfig != null)
            {
                _logs.DebugMsg($"[DisplayMangerPlugin] SetALSFeatureValue AllALSConfig.Count = {AllALSConfig.Count.ToString()} || Into monitorInfos = {monitorInfos.edid.ModelName.ToString()} ,{monitorInfos.edid.SerialNumber.ToString()} ");
                for (int i = 0; i < AllALSConfig.Count; i++)
                {
                    _logs.DebugMsg($"[DisplayMangerPlugin] SetALSFeatureValue AllALSConfig.ModelName = {AllALSConfig[i].Edid.ModelName.ToString()} ||  AllALSConfig.SerialNumber = {AllALSConfig[i].Edid.SerialNumber.ToString()}");
                }
            }
            else
            {
                _logs.DebugMsg($"[DisplayMangerPlugin] SetALSFeatureValue AllALSConfig is null. New a empty object");
                AllALSConfig = new List<ALSConfig>();
            }
            //Dean 0624, the comparison should with DisplayName and SerialNumber
            int idx = AllALSConfig.FindIndex(x => x.Edid.Equals(monitorInfos.edid));// || x.serialNumber.Equals(monitorInfos.edid.SerialNumber));//Find if it exists

            _logs.DebugMsg("[DisplayMangerPlugin] SetALSFeatureValue idx = " + idx.ToString());

            if (idx >= 0)
            {
                ALSConfig aconfig = AllALSConfig[idx];
                AllALSConfig[idx] = param;
                if (AllALSConfig.Count > 1 && // If only one monitor, do not need show Busy
                    GetBitValue(param.AllValue, 5) == 1)
                {
                    for (int i = 0; i < AllALSConfig.Count; i++)
                    {
                        if (AllALSConfig[i].isSupportALS == 2)
                            AllALSConfig[i].isBusy = true;
                        else
                            AllALSConfig[i].isBusy = false;
                        _logs.DebugMsg($"[DisplayMangerPlugin] SetALSFeatureValue ... {AllALSConfig[i].Edid.ModelName} ... Busy ... ");
                    }
                }
                //CheckisPrimaryMonitorSyncOnOff(monitorInfos, param, "0");
            }
            else
            {
                AllALSConfig.Add(param);//add in
            }
            _logs.DebugMsg("[DisplayMangerPlugin] SetALSFeatureValue ... out ");
            return Task.FromResult(param.result);
        }

        /// <summary>
        /// Check Primary Monitor Sync Status, if Primary Monitor Sync = true need to do, because need to sync other monitor status
        /// </summary>
        /// <param name="value">VCP set value</param>
        /// <returns>success or false</returns>
        public Task<bool> CheckisPrimaryMonitorSyncOnOff(MonitorInfo monitorInfoMain, ALSConfig value, string vcpcode)
        {
            lock (_ALSVCPChangeLock)
            {
                _logs.DebugMsg($"[DisplayMangerPlugin] CheckisPrimaryMonitorSyncOnOff ... in");
                Trace.WriteLine($"[DisplayMangerPlugin] CheckisPrimaryMonitorSyncOnOff ... in");
                Trace.WriteLine(monitorInfoMain.modelName.ToString() + " ||" + value.isPrimaryMonitorSync.ToString());
                Trace.WriteLine(value.ModelName + " || AllValue = " + value.AllValue.ToString() + " || GetBit = " + (GetBitValue(value.AllValue, 5).ToString()));
                if (GetBitValue(value.AllValue, 5) == 1)//check isPrimaryMonitorSync whether to change
                {
                    _logs.DebugMsg($"[DisplayMangerPlugin] CheckisPrimaryMonitorSyncOnOff ModelName {monitorInfoMain.edid.ModelName}, AllValue {value.AllValue.ToString()}, isAutoBrightness {value.isAutoBrightness.ToString()}, isAutoColorTemp {value.isAutoColorTemp.ToString()}, isPrimaryMonitorSync {value.isPrimaryMonitorSync.ToString()}");
                    Trace.WriteLine($"[DisplayMangerPlugin] CheckisPrimaryMonitorSyncOnOff ModelName {monitorInfoMain.edid.ModelName}, AllValue {value.AllValue.ToString()}, isAutoBrightness {value.isAutoBrightness.ToString()}, isAutoColorTemp {value.isAutoColorTemp.ToString()}, isPrimaryMonitorSync {value.isPrimaryMonitorSync.ToString()}");
                    List<ALSConfig> als_connected = new List<ALSConfig>();
                    List<ALSConfig> als_connected2 = new List<ALSConfig>();
                    List<MonitorInfo> monitorALS = GetMonitors().Result;//Get Monitor now
                    List<ALSConfig> temp = new List<ALSConfig>();

                    Trace.WriteLine($"[DisplayMangerPlugin] GetBitValue(value.AllValue, 5) == 1 " + value.Edid.ModelName.ToString());
                    _logs.DebugMsg($"[DisplayMangerPlugin] CheckisPrimaryMonitorSyncOnOff ... Monitors.Count = " + monitorALS.Count.ToString() + " || AllALSConfig.Count = " + AllALSConfig.Count.ToString());

                    foreach (MonitorInfo mon in monitorALS)//copy to als_connected first
                    {
                        ALSConfig als_nowtemp = new ALSConfig();
                        ParseMonitorInfo(mon, ref als_nowtemp);
                        als_connected.Add(als_nowtemp);
                    }

                    foreach (ALSConfig aLs1 in als_connected)//from new MonitorInfo and check already exists info
                    {
                        foreach (ALSConfig aLs2 in AllALSConfig)
                        {
                            //Dean 0624, check with displayname and serialnumber at the same time
                            if (aLs1.Edid.Equals(aLs2.Edid))
                            {
                                als_connected2.Add(aLs2);//find original info copy to als_connected2
                            }
                        }
                    }

                    //als_connected = new List<ALSConfig>();//clear

                    if (SyncPrimaryMonitorValueToOtherMonitor(monitorInfoMain, monitorALS, ref als_connected2, value, vcpcode).Result)
                        _logs.DebugMsg($"[DisplayMangerPlugin] SyncPrimaryMonitorValueToOtherMonitor ... True");
                    else
                        _logs.DebugMsg($"[DisplayMangerPlugin] SyncPrimaryMonitorValueToOtherMonitor ... Fail");

                    AllALSConfig = als_connected2;//replace static AllALSConfig data
                    _logs.DebugMsg($"2 [DisplayMangerPlugin] CheckisPrimaryMonitorSyncOnOff ... Monitors.Count = " + monitorALS.Count.ToString() + " || AllALSConfig.Count = " + AllALSConfig.Count.ToString());
                }
                return Task.FromResult(true);
            }
        }

        /// <summary>
        /// IF moMain SyncPrimaryMonitorStatus is on and isAutoBrightness on, should need to sync 0x67 value to other
        /// IF moMain SyncPrimaryMonitorStatus is on and isAutoColorTemp on, should need to sync 0x68 value to other
        /// </summary>
        /// <param name="moMain">Change event Monitor</param>
        /// <param name="monitorAll">All Monitor</param>
        /// <param name="exitAls">Exit ALSConfig</param>
        /// <param name="moMainvalue">Change event Monitor, ALS value</param>
        /// <returns>True or False</returns>
        public Task<bool> SyncPrimaryMonitorValueToOtherMonitor(MonitorInfo moMain, List<MonitorInfo> monitorAll, ref List<ALSConfig> exitAls, ALSConfig moMainvalue, string vcpcode)
        {
            Trace.WriteLine("SyncPrimaryMonitorValueToOtherMonitor ... in");
            Trace.WriteLine("moMain = " + moMain.modelName.ToString() + " ||  moMainvalue.ModelName = " + moMainvalue.ModelName.ToString() + " ||  isPrimaryMonitorSync = " + moMainvalue.isPrimaryMonitorSync.ToString());
            _logs.DebugMsg($"[DisplayMangerPlugin] SyncPrimaryMonitorValueToOtherMonitor ... in");
            var monitorInfoMain = moMain;
            var monitorALS = monitorAll;
            var als_connected2 = exitAls;
            var value = moMainvalue;
            // Read monitorInfoMain value one times
            ObjGetVCP obBrightness = GetVCPCapability(monitorInfoMain, 0x10).Result;
            ObjGetVCP obContrast = GetVCPCapability(monitorInfoMain, 0x12).Result;
            ObjGetVCP obColor = GetVCPCapability(monitorInfoMain, "colorpreset").Result;
            var isprimarysupportlum = !monitorInfoMain.CapabilityDic.ContainsKey("12");
            for (int i = 0; i < als_connected2.Count; i++)
            {
                Trace.WriteLine("monitorALS = " + monitorALS.Find(x => x.edid.Equals(als_connected2[i].Edid)).modelName.ToString() + " || value.isPrimaryMonitorSync = " + value.isPrimaryMonitorSync.ToString() + " , als_connected2[i] = " + als_connected2[i].ModelName.ToString());
                //Dean 0624, check with displayname and serialnumber at the same time
                if (!als_connected2[i].Edid.Equals(monitorInfoMain.edid))//sync AutoBrightness & AutoColorTemp value
                {
                    if (!(als_connected2[i].isSupportALS == 2))
                        continue; //If no support, still continue next monitor

                    if (isprimarysupportlum) // Lum True
                    {
                        _logs.DebugMsg($"[DisplayMangerPlugin] SyncPrimaryMonitorValueToOtherMonitor ... Lum True");
                        if (!als_connected2[i].CapDict.ContainsKey("12"))// Lum True
                        {
                            //only Do 0x10 and colorpreset
                            if (obBrightness.result)
                            {
                                uint brightnessValue = (uint)obBrightness.value;
                                SetVCPCapability(monitorALS.Find(x => x.edid.Equals(als_connected2[i].Edid)), 0x10, brightnessValue);
                                _logs.DebugMsg($"[DisplayMangerPlugin] SyncPrimaryMonitorValueToOtherMonitor ... Lum True Do 0x10 ");
                            }

                            if (obColor.result)
                            {
                                string obColorValue = obColor.value.ToString();
                                SetVCPCapability(monitorALS.Find(x => x.edid.Equals(als_connected2[i].Edid)), "colorpreset", obColorValue);
                                _logs.DebugMsg($"[DisplayMangerPlugin] SyncPrimaryMonitorValueToOtherMonitor ... Lum True Do colorpreset ");
                            }
                        }
                        else// Lum False
                        {
                            //only colorpreset
                            if (obColor.result)
                            {
                                string obColorValue = obColor.value.ToString();
                                SetVCPCapability(monitorALS.Find(x => x.edid.Equals(als_connected2[i].Edid)), "colorpreset", obColorValue);
                                _logs.DebugMsg($"[DisplayMangerPlugin] SyncPrimaryMonitorValueToOtherMonitor ... Lum False Do colorpreset ");
                            }
                        }
                    }

                    if (!isprimarysupportlum) // Lum False
                    {
                        _logs.DebugMsg($"[DisplayMangerPlugin] SyncPrimaryMonitorValueToOtherMonitor ... Lum False");
                        if (!als_connected2[i].CapDict.ContainsKey("12"))// Lum True
                        {
                            //only colorpreset
                            if (obColor.result)
                            {
                                string obColorValue = obColor.value.ToString();
                                Trace.WriteLine("(Lum True) SetVCPCapability to " + als_connected2[i].ModelName + "value.AllValue = " + als_connected2[i].AllValue.ToString());
                                SetVCPCapability(monitorALS.Find(x => x.edid.Equals(als_connected2[i].Edid)), "colorpreset", obColorValue);
                                _logs.DebugMsg($"[DisplayMangerPlugin] SyncPrimaryMonitorValueToOtherMonitor ... Lum True Do colorpreset ");
                            }
                        }
                        else// Lum False
                        {
                            //Dean 0624, add object check as well
                            var mo_tmp = monitorALS.Find(x => x.edid.Equals(als_connected2[i].Edid));
                            if (mo_tmp == null)
                            {
                                continue;
                            }
                            Trace.WriteLine("SetVCPCapability to 0x66 " + mo_tmp.modelName + "value.AllValue = " + value.AllValue.ToString());
                            SetVCPCapability(mo_tmp, 0x66, SetBitValue(value.AllValue, 5, 0));//set VCP command value, but bit5 need to change to 0
                            value.AllValue = SetBitValue(value.AllValue, 5, 0);
                            als_connected2[i].isAutoBrightness = value.isAutoBrightness;
                            als_connected2[i].isAutoColorTemp = value.isAutoColorTemp;
                            als_connected2[i].isPrimaryMonitorSync = false;//set isPrimaryMonitorSync off
                            _logs.DebugMsg($"[DisplayMangerPlugin] SyncPrimaryMonitorValueToOtherMonitor ... Lum False set isPrimaryMonitorSync off ");
                            // Dean 0614 handle brightness/ contrast
                            if (obBrightness.result)
                            {
                                uint brightnessValue = (uint)obBrightness.value;
                                Trace.WriteLine("SetVCPCapability to " + monitorALS.Find(x => x.edid.Equals(als_connected2[i].Edid)).modelName + " ||" + brightnessValue.ToString());
                                SetVCPCapability(monitorALS.Find(x => x.edid.Equals(als_connected2[i].Edid)), 0x10, brightnessValue);
                                _logs.DebugMsg($"[DisplayMangerPlugin] SyncPrimaryMonitorValueToOtherMonitor ... Lum False set 0x10 ");
                            }

                            if (obContrast.result)
                            {
                                uint obcontrastValue = (uint)obContrast.value;
                                Trace.WriteLine("SetVCPCapability to " + monitorALS.Find(x => x.edid.Equals(als_connected2[i].Edid)).modelName + " ||" + obcontrastValue.ToString());
                                SetVCPCapability(monitorALS.Find(x => x.edid.Equals(als_connected2[i].Edid)), 0x12, obcontrastValue);
                                _logs.DebugMsg($"[DisplayMangerPlugin] SyncPrimaryMonitorValueToOtherMonitor ... Lum False set 0x12 ");
                            }

                            if (obColor.result)
                            {
                                string obColorValue = obColor.value.ToString();
                                Trace.WriteLine("SetVCPCapability to " + monitorALS.Find(x => x.edid.Equals(als_connected2[i].Edid)).modelName + " ||" + obColorValue.ToString());
                                SetVCPCapability(monitorALS.Find(x => x.edid.Equals(als_connected2[i].Edid)), "colorpreset", obColorValue);
                                _logs.DebugMsg($"[DisplayMangerPlugin] SyncPrimaryMonitorValueToOtherMonitor ... Lum False set colorpreset ");
                            }
                        }
                    }
                }
            }
            _logs.DebugMsg($"[DisplayMangerPlugin] SyncPrimaryMonitorValueToOtherMonitor ... out");
            return Task.FromResult(true);
        }

        public async Task<bool> SyncPrimaryMonitorBrightnessAndColorTemp(MonitorInfo monitorInfoMain, MonitorInfo monitorvalue, string vcpcode, string eValue)
        {
            Trace.WriteLine($"[DisplayMangerPlugin] SyncPrimaryMonitorBrightnessAndColorTemp Primary = {monitorInfoMain.edid.ModelName} | {monitorInfoMain.edid.ServiceTag}, vcpcode = {vcpcode.ToString()}, need set val = {eValue}, isSupportALS = 2 ... in ");
            _logs.DebugMsg($"[DisplayMangerPlugin] SyncPrimaryMonitorBrightnessAndColorTemp Primary = {monitorInfoMain.edid.ModelName} | {monitorInfoMain.edid.ServiceTag}, vcpcode = {vcpcode.ToString()}, need set val = {eValue}, isSupportALS = 2 ... in ");

            //Trace.WriteLine($"[DisplayMangerPlugin] SyncPrimaryMonitorBrightnessAndColorTemp Other Monitor = {monitorvalue.edid.ModelName} | {monitorInfoMain.edid.ServiceTag} "); // Jim 20250120 modify for PIMS-314608
            // _logs.DebugMsg($"[DisplayMangerPlugin] SyncPrimaryMonitorBrightnessAndColorTemp Other Monitor = {monitorvalue.edid.ModelName} | {monitorInfoMain.edid.ServiceTag} "); // Jim 20250120 modify for PIMS-314608
            Trace.WriteLine($"[DisplayMangerPlugin] SyncPrimaryMonitorBrightnessAndColorTemp Other Monitor = {monitorvalue.edid.ModelName} | {monitorvalue.edid.ServiceTag} ");  // Jim 20250120 modify for PIMS-314608
            _logs.DebugMsg($"[DisplayMangerPlugin] SyncPrimaryMonitorBrightnessAndColorTemp Other Monitor = {monitorvalue.edid.ModelName} | {monitorvalue.edid.ServiceTag} ");   // Jim 20250120 modify for PIMS-314608

            var aconfig = AllALSConfig.Find(x => x.Edid.Equals(monitorvalue.edid));
            if (aconfig == null)
                return false;

            if (uint.TryParse(eValue, out uint temp))
                _logs.DebugMsg($"[DisplayMangerPlugin] SyncPrimaryMonitorBrightnessAndColorTemp 68 SetVCPCapability {temp.ToString()} ");
            else
            {
                _logs.DebugMsg($"[DisplayMangerPlugin] SyncPrimaryMonitorBrightnessAndColorTemp 68 TryParse eValue fail ... ");
                return false;
            }

            if (vcpcode == "68")
            {
                //uint contrastValue = temp & 0xFF;
                if (aconfig.ContrastValue == temp) // same as last time
                    return true;

                // Jim 20250120 modify for PIMS-314608 - U2725QEt Wistron- P3:DDPM(Windows)-Shine a torch or cover the sensor of DUT1,DUT2 screen has not changed
                //ALS_CT control values
                //LL: 0~255.Nx100K
                //HH: Reserved
                uint contrastValue = temp & 0xFF;
                uint ColorTempValue = contrastValue * (uint)100;  // Jim 20250120 add for PIMS-314608

                aconfig.ContrastValue = (int)contrastValue;
                Trace.WriteLine($" [DisplayMangerPlugin] SyncPrimaryMonitorBrightnessAndColorTemp 68 SetVCPCapability, ModelName = {monitorvalue.edid.ModelName} | {monitorvalue.edid.ServiceTag}, temp = {temp.ToString()}, contrastValue = {contrastValue.ToString()}");
                Trace.WriteLine($" [DisplayMangerPlugin] SyncPrimaryMonitorBrightnessAndColorTemp 68 SetVCPCapability, ModelName = {monitorvalue.edid.ModelName} | {monitorvalue.edid.ServiceTag}, temp = {temp.ToString()}, ColorTempValue = {ColorTempValue.ToString()}"); // Jim 20250120 add for PIMS-314608
                _logs.DebugMsg($"[DisplayMangerPlugin] SyncPrimaryMonitorBrightnessAndColorTemp 68 SetVCPCapability, ModelName = {monitorvalue.edid.ModelName} | {monitorvalue.edid.ServiceTag}, {temp.ToString()} ");
                _logs.DebugMsg($"[DisplayMangerPlugin] SyncPrimaryMonitorBrightnessAndColorTemp 68 SetVCPCapability, ModelName = {monitorvalue.edid.ModelName} | {monitorvalue.edid.ServiceTag}, temp = {temp.ToString()}, ColorTempValue = {ColorTempValue.ToString()} ");  // Jim 20250120 add for PIMS-314608
                //if (await _VcpCorePlugin.SetVCPCapability(monitorvalue, 0x68, contrastValue))
                if (await _VcpCorePlugin.SetVCPCapability(monitorvalue, 0x68, ColorTempValue)) // Jim 20250120 modify for PIMS-314608
                {
                    //_logs.DebugMsg($"[DisplayMangerPlugin] SyncPrimaryMonitorBrightnessAndColorTemp Success Set 0x68 = {contrastValue.ToString()}, e.monitor = {monitorInfoMain.edid.SerialNumber} | {monitorInfoMain.edid.ServiceTag}, Set ModelName = {monitorvalue.edid.ModelName} | {monitorvalue.edid.ServiceTag},");
                    _logs.DebugMsg($"[DisplayMangerPlugin] SyncPrimaryMonitorBrightnessAndColorTemp Success Set 0x68 = {ColorTempValue.ToString()}, e.monitor = {monitorInfoMain.edid.SerialNumber} | {monitorInfoMain.edid.ServiceTag}, Set ModelName = {monitorvalue.edid.ModelName} | {monitorvalue.edid.ServiceTag},"); // Jim 20250120 modify for PIMS-314608
                }
                else
                {
                    //_logs.DebugMsg($"[DisplayMangerPlugin] SyncPrimaryMonitorBrightnessAndColorTemp Fail Set 0x68 = {contrastValue.ToString()}, e.monitor = {monitorInfoMain.edid.SerialNumber} | {monitorInfoMain.edid.ServiceTag}, Set ModelName = {monitorvalue.edid.ModelName} | {monitorvalue.edid.ServiceTag},");
                    _logs.DebugMsg($"[DisplayMangerPlugin] SyncPrimaryMonitorBrightnessAndColorTemp Fail Set 0x68 = {ColorTempValue.ToString()}, e.monitor = {monitorInfoMain.edid.SerialNumber} | {monitorInfoMain.edid.ServiceTag}, Set ModelName = {monitorvalue.edid.ModelName} | {monitorvalue.edid.ServiceTag},"); // Jim 20250120 modify for PIMS-314608
                }
            }
            else if (vcpcode == "67")
            {
                if (!monitorvalue.CapabilityDic.ContainsKey("12")) // lum no Brightness
                {
                    Trace.WriteLine($"[DisplayMangerPlugin] Foreach monitor is {monitorvalue.modelName} | {monitorvalue.edid.ServiceTag} support lum monitor ... ");
                    _logs.DebugMsg($"[DisplayMangerPlugin] Foreach monitor is {monitorvalue.modelName} | {monitorvalue.edid.ServiceTag} support lum monitor ... ");
                    return true;
                }

                if (aconfig.BrightnessValue == temp)  // same as last time
                    return true;

                aconfig.BrightnessValue = (int)temp;
                Trace.WriteLine($"[DisplayMangerPlugin] SyncPrimaryMonitorBrightnessAndColorTemp 67 SetVCPCapability , ModelName = {monitorvalue.edid.ModelName} | {monitorvalue.edid.ServiceTag},, {temp.ToString()} ");
                _logs.DebugMsg($"[DisplayMangerPlugin] SyncPrimaryMonitorBrightnessAndColorTemp 67 SetVCPCapability , ModelName = {monitorvalue.edid.ModelName} | {monitorvalue.edid.ServiceTag},, {temp.ToString()} ");
                if (await _VcpCorePlugin.SetVCPCapability(monitorvalue, 0x67, temp))
                {
                    _logs.DebugMsg($"[DisplayMangerPlugin] SyncPrimaryMonitorBrightnessAndColorTemp Success Set 0x67 = {temp.ToString()}, e.monitor = {monitorInfoMain.modelName} | {monitorInfoMain.edid.ServiceTag}, Set ModelName = {monitorvalue.edid.ModelName} | {monitorvalue.edid.ServiceTag},");
                }
                else
                {
                    _logs.DebugMsg($"[DisplayMangerPlugin] SyncPrimaryMonitorBrightnessAndColorTemp Fail Set 0x67 = {temp.ToString()}, e.monitor = {monitorInfoMain.modelName} | {monitorInfoMain.edid.ServiceTag}, Set ModelName = {monitorvalue.edid.ModelName} | {monitorvalue.edid.ServiceTag},");
                }
            }

            _logs.DebugMsg("[DisplayMangerPlugin] SyncPrimaryMonitorBrightnessAndColorTemp vcpcode = 67 68 ... out ");
            return true;
        }

        /// <summary>
        /// Get Connected ALS Config
        /// </summary>
        /// <returns>Return List<ALSConfig> type</returns>
        public Task<List<ALSConfig>> GetConnectedALSConfig()
        {
            List<ALSConfig> als_connecte = new List<ALSConfig>();
            List<MonitorInfo> monitorALS = GetMonitors().Result;
            for (int i = 0; i < monitorALS.Count; i++)
            {
                ALSConfig tempALSConfig = new ALSConfig();
                ParseMonitorInfo(monitorALS[i], ref tempALSConfig);
                als_connecte.Add(tempALSConfig);
            }
            return Task.FromResult(als_connecte);
        }

        /// <summary>
        /// Delete duplicate data, return All Exist Als Config
        /// </summary>
        /// <returns>Return static AllALSConfig</returns>
        public Task<List<ALSConfig>> GetAllExistAlsConfig()
        {
            try
            {
                _logs.DebugMsg($"[DisplayMangerPlugin] GetAllExistAlsConfig ... in, AllALSConfig.Count = " + AllALSConfig.Count.ToString());

                var distinctALSConfigList = new List<ALSConfig>();

                distinctALSConfigList = AllALSConfig.Where(config => config != null && config.Edid != null
                                                                                   && !string.IsNullOrWhiteSpace(config.ModelName)
                                                                                   && !string.IsNullOrWhiteSpace(config.serialNumber)
                                                                                   && !string.IsNullOrWhiteSpace(config.Edid.SerialNumber)
                                                                                   && !string.IsNullOrWhiteSpace(config.Edid.ServiceTag)).GroupBy(p => new { p.Edid }).Select(g => g.First()).ToList();

                for (int i = 0; i < distinctALSConfigList.Count; i++)
                {
                    _logs.DebugMsg($"[DisplayMangerPlugin] GetAllExistAlsConfig, ALSConfig index        : " + i.ToString());
                    _logs.DebugMsg($"[DisplayMangerPlugin] GetAllExistAlsConfig, ALSConfig ModelName    : " + distinctALSConfigList[i].ModelName.ToString());
                    _logs.DebugMsg($"[DisplayMangerPlugin] GetAllExistAlsConfig, ALSConfig SerialNumber : " + distinctALSConfigList[i].serialNumber.ToString());
                    _logs.DebugMsg($"[DisplayMangerPlugin] GetAllExistAlsConfig, ALSConfig ServiceTag   : " + distinctALSConfigList[i].Edid.ServiceTag.ToString());
                    _logs.DebugMsg($"[DisplayMangerPlugin] GetAllExistAlsConfig, ALSConfig EDID         : " + distinctALSConfigList[i].Edid.ToString());
                    _logs.DebugMsg($"[DisplayMangerPlugin] GetAllExistAlsConfig, ALSConfig SupportALS   : " + distinctALSConfigList[i].isSupportALS.ToString());
                    _logs.DebugMsg($"[DisplayMangerPlugin] GetAllExistAlsConfig, ALSConfig AllValue     : " + distinctALSConfigList[i].AllValue.ToString());
                    _logs.DebugMsg($"[DisplayMangerPlugin] GetAllExistAlsConfig, AutoBrightness         : " + distinctALSConfigList[i].isAutoBrightness.ToString());
                    _logs.DebugMsg($"[DisplayMangerPlugin] GetAllExistAlsConfig, AutoColorTemp          : " + distinctALSConfigList[i].isAutoColorTemp.ToString());
                    _logs.DebugMsg($"[DisplayMangerPlugin] GetAllExistAlsConfig, RangeLevel Value       : " + distinctALSConfigList[i].AutoBrightnessRangeLevel.level_value.ToString());
                    _logs.DebugMsg($"[DisplayMangerPlugin] GetAllExistAlsConfig, PrimaryMonitor         : " + distinctALSConfigList[i].isPrimaryMonitorSync.ToString());
                }

                _logs.DebugMsg($"[DisplayMangerPlugin] GetAllExistAlsConfig ... out ");
                AllALSConfig = distinctALSConfigList;
                return Task.FromResult(distinctALSConfigList);
            }
            catch (Exception ex)
            {
                //_logs.DebugMsg($"[DisplayMangerPlugin] GetAllExistAlsConfig Exception {ex.Message}");
                WriteLog($"GetAllExistAlsConfig Exception {ex.Message}", log_type.error);
                return Task.FromResult(new List<ALSConfig>());
            }
        }


        /// <summary>
        /// Update Import ALS Value
        /// </summary>
        /// <returns>bool type</returns>
        public Task<bool> UpdateImportAlsValueAsync(MonitorInfo monitorInfos, uint value)
        {
            try
            {
                _logs.DebugMsg($"[DisplayMangerPlugin] UpdateImportAlsValueAsync ... in");
                List<ALSConfig> als_connecte = new List<ALSConfig>();
                ALSConfig aconfig = AllALSConfig.Find(x => x.ModelName.Equals(monitorInfos.modelName));
                if (aconfig != null)
                {
                    ParseBitDefineToAlsObject(value, ref aconfig);
                    ParseMonitorInfo(monitorInfos, ref aconfig);
                }
                else
                {
                    _logs.DebugMsg($"[DisplayMangerPlugin] UpdateImportAlsValueAsync can not find aconfig  monitor");
                    aconfig = new ALSConfig();
                    GetALSupport(monitorInfos, ref aconfig);
                    if (aconfig.result == true && aconfig.isSupportALS != 0)//Read the ALS value only if ALS is supported
                    {
                        GetALSAll(monitorInfos, ref aconfig);
                    }
                    ParseMonitorInfo(monitorInfos, ref aconfig);
                    AllALSConfig.Add(aconfig);
                }
                _logs.DebugMsg($"[DisplayMangerPlugin] UpdateImportAlsValueAsync       modelName = {monitorInfos.modelName}");
                _logs.DebugMsg($"[DisplayMangerPlugin] UpdateImportAlsValueAsync           value = {value.ToString()}");
                _logs.DebugMsg($"[DisplayMangerPlugin] UpdateImportAlsValueAsync  AutoBrightness = {aconfig.isAutoBrightness.ToString()}");
                _logs.DebugMsg($"[DisplayMangerPlugin] UpdateImportAlsValueAsync  AutoColorTemp  = {aconfig.isAutoColorTemp.ToString()}");
                if (aconfig.AutoBrightnessRangeLevel != null)
                {
                    _logs.DebugMsg($"[DisplayMangerPlugin] UpdateImportAlsValueAsync RangeLevelValue = {aconfig.AutoBrightnessRangeLevel.level_value.ToString()}");
                }
                else
                {
                    _logs.DebugMsg("[DisplayMangerPlugin] UpdateImportAlsValueAsync RangeLevelValue is empty or null.");
                }
                _logs.DebugMsg($"[DisplayMangerPlugin] UpdateImportAlsValueAsync RangeLevelValue = {aconfig.AutoBrightnessRangeLevel.level_value.ToString()}");
                _logs.DebugMsg($"[DisplayMangerPlugin] UpdateImportAlsValueAsync  PrimaryMonitor = {aconfig.isPrimaryMonitorSync.ToString()}");
                _logs.DebugMsg($"[DisplayMangerPlugin] UpdateImportAlsValueAsync ... out");
                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                WriteLog($"UpdateImportAlsValueAsync Exception {ex.Message}", log_type.error);
                return Task.FromResult(false);
            }
        }

        /// <summary>
        /// Update Connected ALS Config
        /// </summary>
        /// <returns>Return List<ALSConfig> type</returns>
        public Task<List<ALSConfig>> UpdateExistAlsConfig(List<MonitorInfo> monitorInfos)
        {
            try
            {
                _logs.DebugMsg($"[DisplayMangerPlugin] UpdateExistAlsConfig ... in");
                List<ALSConfig> als_connecte = new List<ALSConfig>();
                for (int i = 0; i < monitorInfos.Count; i++)
                {
                    ALSConfig aconfig = AllALSConfig.Find(x => x.Edid.Equals(monitorInfos[i].edid));// && x.serialNumber.ToUpper().Equals(monitorInfos[i].edid.SerialNumber.ToUpper()));
                    if (aconfig != null)
                    {
                        _logs.DebugMsg($"[DisplayMangerPlugin]UpdateExistAlsConfig aconfig " + aconfig.DisplayName.ToString() + " || " + aconfig.serialNumber.ToString());
                        als_connecte.Add(aconfig);
                    }
                }
                AllALSConfig = als_connecte;
                _logs.DebugMsg($"[DisplayMangerPlugin] UpdateExistAlsConfig ... out");
                return Task.FromResult(als_connecte);
            }
            catch (Exception ex)
            {
                //_logs.DebugMsg($"[DisplayMangerPlugin] UpdateExistAlsConfig Exception {ex.Message}");
                WriteLog($"UpdateExistAlsConfig Exception {ex.Message}", log_type.error);
                return Task.FromResult(new List<ALSConfig>());
            }
        }

        /// <summary>
        /// Synchronize ALSF eature Value
        /// </summary>
        /// <param name="monitorALS"></param>
        /// <returns>success or fail</returns>
        public Task<bool> SynchronizeALSFeatureValue(ALSConfig monitorALS)
        {
            ALSConfig aconfig = AllALSConfig.Find(x => x.Edid.Equals(monitorALS.Edid));// && x.serialNumber.Equals(monitorALS.serialNumber));//Dean 0624
            for (int i = 0; i < AllALSConfig.Count; i++)
            {
                AllALSConfig[i].Edid = monitorALS.Edid;
                AllALSConfig[i].CapDict = monitorALS.CapDict;
                AllALSConfig[i].AllValue = monitorALS.AllValue;
                AllALSConfig[i].isAutoBrightness = monitorALS.isAutoBrightness;
                AllALSConfig[i].isAutoColorTemp = monitorALS.isAutoColorTemp;
                AllALSConfig[i].AutoBrightnessRangeLevel = monitorALS.AutoBrightnessRangeLevel;//CLI: Mark 0723
                //AllALSConfig[i].serialNumber = monitorALS.serialNumber; //Dean 0624 modify
                if (AllALSConfig[i].DisplayName.Equals(monitorALS.DisplayName) && AllALSConfig[i].serialNumber.Equals(monitorALS.serialNumber))
                    AllALSConfig[i].isPrimaryMonitorSync = true;
                else
                    AllALSConfig[i].isPrimaryMonitorSync = false;
            }

            return Task.FromResult(true);
        }

        /// <summary>
        /// Update ALS Feature Value, when Monitor plugin/off
        /// </summary>
        public Task<bool> UpdateALSFeatureValue(MonitorInfo monitorInfos)
        {
            _logs.DebugMsg("[DisplayMangerPlugin] UpdateALSFeatureValue ... in");
            ALSConfig aconfig = AllALSConfig.Find(x => x.Edid.Equals(monitorInfos.edid));// && x.serialNumber.Equals(monitorInfos.edid.SerialNumber));//Dean 0624
            if (aconfig == null)
            {
                aconfig = new ALSConfig();
                GetALSupport(monitorInfos, ref aconfig);
                if (aconfig.result == false)
                {
                    _logs.DebugMsg("[DisplayMangerPlugin] UpdateALSFeatureValue GetALSupport False...");
                    return Task.FromResult(false);
                }
                if (aconfig.result == true && aconfig.isSupportALS != 0)//Read the ALS value only if ALS is supported
                {
                    GetALSAll(monitorInfos, ref aconfig);
                }
                if (aconfig.result == false)
                {
                    _logs.DebugMsg("[DisplayMangerPlugin] UpdateALSFeatureValue GetALSAll False...");
                    return Task.FromResult(false);
                }
                ParseMonitorInfo(monitorInfos, ref aconfig);
                AllALSConfig.Add(aconfig);
                _logs.DebugMsg("[DisplayMangerPlugin] UpdateALSFeatureValue ... out");
                return Task.FromResult(true);
            }
            return Task.FromResult(true);
        }

        /// <summary>
        /// Update ALS Feature Value
        /// </summary>
        private ALSConfig UpdateALSFeatureByValue(MonitorInfo monitorInfos, uint value)
        {
            _logs.DebugMsg("[DisplayMangerPlugin] UpdateALSFeatureByValue ... in");
            lock (_ALSVCPChangeLock)
            {
                _logs.DebugMsg($"[DisplayMangerPlugin] UpdateALSFeatureByValue AllALSConfig {monitorInfos.edid.ModelName}  {monitorInfos.edid.SerialNumber}");
                ALSConfig alsTemp = new ALSConfig();
                ALSConfig? alsConfig = null;
                int idx = AllALSConfig.FindIndex(x => x.Edid.Equals(monitorInfos.edid));// && x.serialNumber.Equals(monitorInfos.edid.SerialNumber));//Dean 0624
                if (idx >= 0)
                    alsConfig = AllALSConfig[idx];

                _logs.DebugMsg($"[DisplayMangerPlugin] UpdateALSFeatureByValue {monitorInfos.edid.ModelName} , idx = {idx.ToString()}, AllALSConfig.Count = {AllALSConfig.Count.ToString()}");

                ParseBitDefineToAlsObject(value, ref alsTemp);
                ParseMonitorInfo(monitorInfos, ref alsTemp);

                _logs.DebugMsg($"[DisplayMangerPlugin] UpdateALSFeatureByValue {monitorInfos.edid.ModelName} , idx = {idx.ToString()}, AllALSConfig.Count = {AllALSConfig.Count.ToString()}");
                _logs.DebugMsg($"[DisplayMangerPlugin] UpdateALSFeatureByValue ModelName {alsTemp.Edid.ModelName}, AllValue {alsTemp.AllValue.ToString()}, isAutoBrightness {alsTemp.isAutoBrightness.ToString()}, isAutoColorTemp {alsTemp.isAutoColorTemp.ToString()}, isPrimaryMonitorSync {alsTemp.isPrimaryMonitorSync.ToString()}");

                if (alsConfig == null)
                {
                    GetALSupport(monitorInfos, ref alsTemp);
                    ParseMonitorInfo(monitorInfos, ref alsTemp);
                    AllALSConfig.Add(alsTemp);
                    _logs.DebugMsg($"[DisplayMangerPlugin] UpdateALSFeatureValue {monitorInfos.edid.ModelName},  alsConfig == null ... out");
                    return alsTemp;
                }
                else
                {
                    int tmp = alsConfig.isSupportALS;
                    alsTemp.copyByType(ALSFeatureQueryType.no_SerialNumber, alsTemp, ref alsConfig);
                    alsConfig.isSupportALS = tmp;
                    AllALSConfig[idx] = alsConfig;
                    _logs.DebugMsg($"[DisplayMangerPlugin] UpdateALSFeatureValue {monitorInfos.edid.ModelName},  alsConfig != null ... out");
                    return alsConfig;
                }
            }
        }

        /// <summary>
        /// Get ALS Support Status
        /// Capabilities string:
        /// ALS full function: 66(00F2)
        /// AlS without ALS_Primary: 66(00D2)
        /// ALS without sensor: 66(0012)
        /// Exception: U2724D/DE & U2424H/HE using old definition: 66(0F02) / 66(0D02) / 66(0102)
        /// </summary>
        /// <param name="monitorInfos">monitor Info</param>
        /// <param name="param">ALSConfig data</param>
        private void GetALSupport(MonitorInfo monitorInfos, ref ALSConfig param)
        {
            _logs.DebugMsg("[DisplayMangerPlugin] ALSFeature into GetALSupport ...");

            if (monitorInfos.CapabilityString.Contains("66"))//Directly determine CapabilityString to improve performance
            {
                if (monitorInfos.CapabilityString.Contains("00F2") || monitorInfos.CapabilityString.Contains("0F02"))
                {
                    param.isSupportALS = 2;
                }
                else if (monitorInfos.CapabilityString.Contains("00D2") || monitorInfos.CapabilityString.Contains("0D02"))
                {
                    param.isPrimaryMonitorSync = false;
                    param.isSupportALS = 1;
                }
                else
                {
                    param.isSupportALS = 0;
                }
            }
            else
            {
                param.isSupportALS = 0;
                _logs.DebugMsg($"[DisplayMangerPlugin] not support ALS from {monitorInfos.edid.ModelName}");
            }
            param.result = true;
            _logs.DebugMsg("[DisplayMangerPlugin] ALSFeature leave GetALSupport ");
        }

        /// <summary>
        /// Get ALS Multi Monitor Sync status
        /// </summary>
        /// <param name="monitorInfos">monitor Info</param>
        /// <param name="param">ALSConfig data</param>
        private void GetALSMMS(MonitorInfo monitorInfos, ref ALSConfig param)
        {
            _logs.DebugMsg("[DisplayMangerPlugin] ALSFeature into GetALSMMS ...");

            ObjGetVCP result = new ObjGetVCP();
            ////==Multi - Monitor Sync(MMS)==//0x00 MMS Off; 0x01 MMS On (DUT1); 0x03 (On, DP-out, MST)
            result = GetVCPCapability(monitorInfos, 0xEF, opt: 0).Result;
            if (result != null && result.result)
            {
                param.isMMSEnable = System.Convert.ToBoolean(result.value);
                param.result = result.result;
            }
            else
                param.result = false;

            _logs.DebugMsg("[DisplayMangerPlugin] ALSFeature leave GetALSMMS ");
        }

        /// <summary>
        /// Set ALS Multi Monitor Sync status
        /// </summary>
        /// <param name="monitorInfos">monitor Info</param>
        /// <param name="param">ALSConfig</param>
        /// <param name="value">value</param>
        private void SetALSMMS(MonitorInfo monitorInfos, ref ALSConfig param, string value)
        {
            _logs.DebugMsg("[DisplayMangerPlugin] ALSFeature into SetALSMMS ...");

            ////==Multi - Monitor Sync(MMS)==//0x00 MMS Off; 0x01 MMS On (DUT1); 0x03 (On, DP-out, MST)
            if (monitorInfos.CapabilityDic.ContainsKey("EF"))
            {
                if (SetVCPCapability(monitorInfos, 0xEF, StrConvertUint(value)).Result)
                {
                    param.isMMSEnable = StrConvertOnOff(value);
                    param.result = true;
                }
                else
                    param.result = false;
            }

            _logs.DebugMsg("[DisplayMangerPlugin] ALSFeature leave SetALSMMS ");
        }

        /// <summary>
        /// Get ALS Primary Monitor Sync
        /// </summary>
        /// <param name="monitorInfos">monitor Info</param>
        /// <param name="param">ALSConfig data</param>
        private void GetALSPrimaryMS(MonitorInfo monitorInfos, ref ALSConfig param)
        {
            _logs.DebugMsg("[DisplayMangerPlugin] ALSFeature into GetALSPrimaryMS ...");

            ObjGetVCP result = new ObjGetVCP();
            //==Primary ==//Bit 5 : 0 = UnSelected, 1 = Selected
            result = GetVCPCapability(monitorInfos, 0x66, opt: 0).Result;
            if (result != null && result.result)
            {
                param.isPrimaryMonitorSync = ((uint)result.value & (1u << 5)) != 0;
                param.result = result.result;
            }
            else
                param.result = false;

            _logs.DebugMsg("[DisplayMangerPlugin] ALSFeature leave GetSetALSFeatureMMS ");
        }

        /// <summary>
        /// Set ALS Primary Monitor Sync
        /// </summary>
        /// <param name="monitorInfos">monitor Info</param>
        /// <param name="param">ALSConfig data</param>
        /// <param name="value">value</param>
        private void SetALSPrimaryMS(MonitorInfo monitorInfos, ref ALSConfig param, string value)
        {
            _logs.DebugMsg("[DisplayMangerPlugin] ALSFeature into SetALPrimaryMS ...");
            Trace.WriteLine($"[DisplayMangerPlugin] ALSFeature into SetALPrimaryMS ... {monitorInfos.edid.ModelName}...value = {value}");
            ObjGetVCP result = new ObjGetVCP();
            //==Primary ==//Bit 5 : 0 = UnSelected, 1 = Selected
            if (monitorInfos.CapabilityDic.ContainsKey("66"))
            {
                result = GetVCPCapability(monitorInfos, 0x66, opt: 0).Result;
                if (result != null && result.result)
                {
                    uint val = SetBitValue((uint)result.value, 5, (int)StrConvertUint(value));
                    if (SetVCPCapability(monitorInfos, 0x66, val).Result)
                    {
                        param.isPrimaryMonitorSync = StrConvertOnOff(value);
                        param.result = true;
                    }
                    else
                        param.result = false;
                }
                else
                    param.result = false;
            }

            _logs.DebugMsg("[DisplayMangerPlugin] ALSFeature leave SetALPrimaryMS ");
        }

        /// <summary>
        /// Get ALS Auto Color Temp
        /// </summary>
        /// <param name="monitorInfos">monitor Info</param>
        /// <param name="param">ALSConfig data</param>
        private void GetALSAutoColorTemp(MonitorInfo monitorInfos, ref ALSConfig param)
        {
            _logs.DebugMsg("[DisplayMangerPlugin] ALSFeature into GetALSAutoColorTemp ...");

            ObjGetVCP result = new ObjGetVCP();
            //==Auto Color Temperature==//Bit 4 : 0 = Off, 1 = On
            result = GetVCPCapability(monitorInfos, 0x66, opt: 0).Result;
            if (result != null && result.result)
            {
                param.isAutoColorTemp = ((uint)result.value & (1u << 4)) != 0;
                param.result = result.result;
            }
            else
                param.result = false;

            _logs.DebugMsg("[DisplayMangerPlugin] ALSFeature leave GetALSAutoColorTemp ");
        }

        /// <summary>
        /// Set ALS Auto Color Temp
        /// </summary>
        /// <param name="monitorInfos">monitor Info</param>
        /// <param name="param">ALSConfig data</param>
        /// <param name="value">value</param>
        private void SetALSAutoColorTemp(MonitorInfo monitorInfos, ref ALSConfig param, string value)
        {
            _logs.DebugMsg("[DisplayMangerPlugin] ALSFeature into SetALSAutoColorTemp ...");

            ObjGetVCP result = new ObjGetVCP();
            //==Auto Color Temperature==//Bit 4 : 0 = Off, 1 = On
            if (monitorInfos.CapabilityDic.ContainsKey("66"))
            {
                result = GetVCPCapability(monitorInfos, 0x66, opt: 0).Result;
                if (result != null && result.result)
                {
                    uint val = SetBitValue((uint)result.value, 4, (int)StrConvertUint(value));
                    if (SetVCPCapability(monitorInfos, 0x66, val).Result)
                    {
                        param.isAutoColorTemp = StrConvertOnOff(value);
                        param.result = true;
                    }
                    else
                        param.result = false;
                }
                else
                    param.result = false;
            }

            _logs.DebugMsg("[DisplayMangerPlugin] ALSFeature leave SetALSAutoColorTemp ");
        }

        /// <summary>
        /// Get ALS Auto Brightness
        /// </summary>
        /// <param name="monitorInfos">monitor Info</param>
        /// <param name="param">ALSConfig data</param>
        private void GetALSAutoBrightness(MonitorInfo monitorInfos, ref ALSConfig param)
        {
            _logs.DebugMsg("[DisplayMangerPlugin] ALSFeature into GetALSAutoBrightness ...");

            ObjGetVCP result = new ObjGetVCP();
            //==AutoBrightness==//Bit 0: 0 = Reserved, 1 = AutoBrightness Off || Bit 1: 0 = Reserved, 1 = AutoBrightness On
            result = GetVCPCapability(monitorInfos, 0x66, opt: 0).Result;
            if (result != null && result.result)
            {
                uint val = GetBitsValue((uint)result.value, 0);
                param.isAutoBrightness = (val == 1 ? false : true);
                param.result = result.result;
            }
            else
                param.result = false;

            _logs.DebugMsg("[DisplayMangerPlugin] ALSFeature leave GetALSAutoBrightness ");
        }

        /// <summary>
        /// Set ALS Auto Brightness
        /// </summary>
        /// <param name="monitorInfos">monitor Info</param>
        /// <param name="param">ALSConfig data</param>
        /// <param name="value">value</param>
        private void SetALSAutoBrightness(MonitorInfo monitorInfos, ref ALSConfig param, string value)
        {
            _logs.DebugMsg("[DisplayMangerPlugin] ALSFeature into SetALSAutoBrightness ...");

            ObjGetVCP result = new ObjGetVCP();
            if (monitorInfos.CapabilityDic.ContainsKey("66"))
            {
                //==AutoBrightness==//Bit 0: 0 = Reserved, 1 = AutoBrightness Off || Bit 1: 0 = Reserved, 1 = AutoBrightness On
                result = GetVCPCapability(monitorInfos, 0x66, opt: 0).Result;
                if (result != null && result.result)
                {
                    uint val;
                    if (string.Equals(value, "ON", StringComparison.OrdinalIgnoreCase))
                        val = SetBitsValue((uint)result.value, 0, 2);
                    else
                        val = SetBitsValue((uint)result.value, 0, 1);

                    if (SetVCPCapability(monitorInfos, 0x66, val).Result)
                    {
                        param.isAutoBrightness = StrConvertOnOff(value);
                        param.result = true;
                    }
                    else
                        param.result = false;
                }
                else
                    param.result = false;
            }

            _logs.DebugMsg("[DisplayMangerPlugin] ALSFeature leave SetALSAutoBrightness ");
        }

        /// <summary>
        /// Get ALS Auto Brightness Level
        /// </summary>
        /// <param name="monitorInfos">monitor Info</param>
        /// <param name="param">ALSConfig data</param>
        private void GetALSAutoBrightnessRangeLevel(MonitorInfo monitorInfos, ref ALSConfig param)//CLI: Mark 0723
        {
            _logs.DebugMsg("[DisplayMangerPlugin] ALSFeature into GetALSAutoBrightness ...");

            ObjGetVCP result = new ObjGetVCP();
            List<AutoBrightnessRangeLevel> brightnessrangelevellist = new List<AutoBrightnessRangeLevel>();
            AutoBrightnessRangeLevel brightnessrangelevel = new AutoBrightnessRangeLevel();
            //==Auto Brightness Range  Level==//Bit 6~7 : 0=Leve 1 | 1=Level 2 | 2=Level 3
            result = GetVCPCapability(monitorInfos, 0x66, opt: 0).Result;
            if (result != null && result.result)
            {
                brightnessrangelevel.level_value = GetBitsValue((uint)result.value, 6);
                switch (brightnessrangelevel.level_value)
                {
                    case 0:
                        brightnessrangelevel.level_name = "Low";
                        break;

                    case 1:
                        brightnessrangelevel.level_name = "Mid";
                        break;

                    case 2:
                        brightnessrangelevel.level_name = "High";
                        break;
                }
                param.AutoBrightnessRangeLevel = brightnessrangelevel;
                param.result = result.result;
            }
            else
                param.result = false;

            _logs.DebugMsg("[DisplayMangerPlugin] ALSFeature leave GetALSAutoBrightness ");
        }

        /// <summary>
        /// Set ALS Auto Brightness Level
        /// </summary>
        /// <param name="monitorInfos">monitor Info</param>
        /// <param name="param">ALSConfig data</param>
        /// <param name="value">value</param>
        private void SetALSAutoBrightnessRangeLevel(MonitorInfo monitorInfos, ref ALSConfig param, string value)//CLI: Mark 0723
        {
            _logs.DebugMsg("[DisplayMangerPlugin] ALSFeature into SetALSAutoBrightnessRangeLevel ...");

            ObjGetVCP result = new ObjGetVCP();
            List<AutoBrightnessRangeLevel> brightnessrangelevellist = new List<AutoBrightnessRangeLevel>();
            AutoBrightnessRangeLevel brightnessrangelevel = new AutoBrightnessRangeLevel();
            if (monitorInfos.CapabilityDic.ContainsKey("66"))
            {
                //==Auto Brightness Range  Level==//Bit 6~7 : 0=Leve 1 | 1=Level 2 | 2=Level 3
                result = GetVCPCapability(monitorInfos, 0x66, opt: 0).Result;
                if (result != null && result.result)
                {
                    uint val = SetBitsValue((uint)result.value, 6, int.Parse(value));
                    if (SetVCPCapability(monitorInfos, 0x66, val).Result)
                    {
                        switch (value)
                        {
                            case "0":
                                brightnessrangelevel.level_name = "Low";
                                break;

                            case "1":
                                brightnessrangelevel.level_name = "Mid";
                                break;

                            case "2":
                                brightnessrangelevel.level_name = "High";
                                break;
                        }
                        param.AutoBrightnessRangeLevel = brightnessrangelevel;
                        param.result = true;
                    }
                    else
                        param.result = false;
                }
                else
                    param.result = false;
            }

            _logs.DebugMsg("[DisplayMangerPlugin] ALSFeature leave SetALSAutoBrightnessRangeLevel ");
        }

        /// <summary>
        /// Get ALS All status
        /// </summary>
        /// <param name="monitorInfos">monitor Info</param>
        /// <param name="param">ALSConfig data</param>
        private void GetALSAll(MonitorInfo monitorInfos, ref ALSConfig param)//CLI: Mark 0723
        {
            _logs.DebugMsg("[DisplayMangerPlugin] ALSFeature into GetALSAll ...");

            ObjGetVCP result = new ObjGetVCP();
            //List<AutoBrightnessRangeLevel> brightnessrangelevellist = new List<AutoBrightnessRangeLevel>();
            //AutoBrightnessRangeLevel brightnessrangelevel = new AutoBrightnessRangeLevel();
            if (monitorInfos.CapabilityString.Contains("66"))//Directly determine CapabilityString to improve performance
            {
                result = GetVCPCapability(monitorInfos, 0x66, opt: 0).Result;
            }

            if (result != null && result.result)
            {
                ParseBitDefineToAlsObject((uint)result.value, ref param);
                ParseMonitorInfo(monitorInfos, ref param);
                param.result = result.result;
            }
            else
                param.result = false;

            _logs.DebugMsg("[DisplayMangerPlugin] ALSFeature leave GetALSAll ");
        }

        /// <summary>
        /// Parse Bit Define To Als Object, analysis Byte
        /// </summary>
        /// <param name="vcp_value">Als all status (1Byte)</param>
        /// <param name="param">ALSConfig</param>
        private void ParseBitDefineToAlsObject(uint vcp_value, ref ALSConfig param)
        {
            _logs.DebugMsg("[DisplayMangerPlugin] ParseBitDefineToAlsObject ... in ");
            AutoBrightnessRangeLevel brightnessLevel = new AutoBrightnessRangeLevel();

            uint val = GetBitsValue(vcp_value, 0);

            param.AllValue = vcp_value;

            param.isAutoBrightness = (val == 1 ? false : true);

            param.isAutoColorTemp = (vcp_value & (1u << 4)) != 0;

            param.isPrimaryMonitorSync = (vcp_value & (1u << 5)) != 0;

            brightnessLevel.level_value = GetBitsValue(vcp_value, 6);
            switch (brightnessLevel.level_value)
            {
                case 0:
                    brightnessLevel.level_name = "Low";
                    break;

                case 1:
                    brightnessLevel.level_name = "Mid";
                    break;

                case 2:
                    brightnessLevel.level_name = "High";
                    break;
            }
            param.AutoBrightnessRangeLevel = brightnessLevel;
            _logs.DebugMsg($"[DisplayMangerPlugin] ParseBitDefineToAlsObject param = vcp_value {vcp_value.ToString()}, isAutoBrightness {param.isAutoBrightness.ToString()}, isAutoColorTemp {param.isAutoColorTemp.ToString()}, isPrimaryMonitorSync {param.isPrimaryMonitorSync.ToString()}, level_name {brightnessLevel.level_name}  ");
            _logs.DebugMsg("[DisplayMangerPlugin] ParseBitDefineToAlsObject ... out ");
        }

        private void ParseMonitorInfo(MonitorInfo moinfo, ref ALSConfig param)
        {
            _logs.DebugMsg("[DisplayMangerPlugin] ParseMonitorInfo ... in ");

            param.MoInfo = moinfo;
            param.ModelName = moinfo.edid.ModelName;
            param.serialNumber = moinfo.edid.SerialNumber;
            param.CapDict = moinfo.CapabilityDic;
            param.Edid = moinfo.edid;
            param.DisplayName = moinfo.DisplayName;

            _logs.DebugMsg("[DisplayMangerPlugin] ParseMonitorInfo ... out ");
        }

        /// <summary>
        /// Set ALS All status
        /// </summary>
        /// <param name="monitorInfos">monitor Info</param>
        /// <param name="param">ALSConfig</param>
        /// <param name="value">value</param>
        private void SetALSAll(MonitorInfo monitorInfos, ref ALSConfig param, string value)
        {
            _logs.DebugMsg($"[DisplayMangerPlugin] ALSFeature into SetALSAll to {monitorInfos.edid.ModelName}, param = {param.AllValue.ToString()}, value = {value}");
            _logs.DebugMsg($"[DisplayMangerPlugin] ALSFeature into SetALSAll AutoBrightness . = {param.isAutoBrightness.ToString()}");
            _logs.DebugMsg($"[DisplayMangerPlugin] ALSFeature into SetALSAll AutoColorTemp .. = {param.isAutoColorTemp.ToString()}");
            _logs.DebugMsg($"[DisplayMangerPlugin] ALSFeature into SetALSAll RangeLevel Value = {param.AutoBrightnessRangeLevel.level_value.ToString()}");
            _logs.DebugMsg($"[DisplayMangerPlugin] ALSFeature into SetALSAll PrimaryMonitor . = {param.isPrimaryMonitorSync.ToString()}");
            Trace.WriteLine($"[DisplayMangerPlugin] ALSFeature into SetALSAll to {monitorInfos.edid.ModelName}...value = {value}");
            param.AllValue = UpdateAllValue(param);
            if (monitorInfos.CapabilityString.Contains("66"))
            {
                if (SetVCPCapability(monitorInfos, 0x66, param.AllValue).Result)
                {
                    param.result = true;
                }
                else
                    param.result = false;
            }

            _logs.DebugMsg("[DisplayMangerPlugin] ALSFeature leave SetALSAll ");
        }

        /// <summary>
        /// Update All ALS Value
        /// </summary>
        /// <param name="config">ALSConfig</param>
        /// <returns>return ALS each bit status</returns>
        private uint UpdateAllValue(ALSConfig config)
        {
            uint value = config.AllValue;
            // Rule 1
            if (config.isAutoBrightness)
            {
                value &= ~((uint)1 << 0); // Set bit0 to 0
                value |= (uint)1 << 1; // Set bit1 to 1
            }
            else
            {
                value |= (uint)1 << 0; // Set bit0 to 1
                value &= ~((uint)1 << 1); // Set bit1 to 0
            }
            // Rule 2
            if (config.isAutoColorTemp)
            {
                value |= (uint)1 << 4; // Set bit4 to 1
            }
            else
            {
                value &= ~((uint)1 << 4); // Clear bit4 to 0
            }
            // Rule 3
            if (config.isPrimaryMonitorSync)
            {
                value |= (uint)1 << 5; // Set bit5 to 1
            }
            else
            {
                value &= ~((uint)1 << 5); // Clear bit5 to 0
            }
            // Rules 4-6
            if (config.AutoBrightnessRangeLevel != null)
            {
                var level = config.AutoBrightnessRangeLevel;
                if (level.level_value == 0)
                {
                    value &= ~((uint)1 << 6); // Clear bit6 to 0
                    value &= ~((uint)1 << 7); // Clear bit7 to 0
                }
                else if (level.level_value == 1)
                {
                    value |= (uint)1 << 6; // Set bit6 to 1
                    value &= ~((uint)1 << 7); // Clear bit7 to 0
                }
                else if (level.level_value == 2)
                {
                    value &= ~((uint)1 << 6); // Clear bit6 to 0
                    value |= (uint)1 << 7; // Set bit7 to 1
                }
            }
            return value;
        }

        /// <summary>
        /// Set Bits Value
        /// </summary>
        /// <param name="number">ALS status (2Byte)</param>
        /// <param name="startBitPosition">Bit Position</param>
        /// <param name="value">value</param>
        /// <returns>return set value</returns>
        private uint SetBitsValue(uint number, int startBitPosition, int value)
        {
            uint mask = 0b11u << startBitPosition;//Create mask to clear two bits at the specified position
            number &= ~mask;// Clear two bits at the specified position
            number |= (uint)(value << startBitPosition);// Set the new value
            return number;
        }

        /// <summary>
        /// Set Bit Value
        /// </summary>
        /// <param name="number">ALS status (1bit)</param>
        /// <param name="startBitPosition">Bit Position</param>
        /// <param name="value">value</param>
        /// <returns>return set value</returns>
        private uint SetBitValue(uint number, int startBitPosition, int value)
        {
            uint mask = 0b1u << startBitPosition;//Create mask to clear one bits at the specified position
            number &= ~mask;// Clear one bits at the specified position
            number |= (uint)(value << startBitPosition);// Set the new value
            return number;
        }

        /// <summary>
        /// Get Bits Value
        /// </summary>
        /// <param name="number">ALS status (2Byte)</param>
        /// <param name="startBitPosition">Bit Position</param>
        /// <returns>return BitPosition value</returns>
        private uint GetBitsValue(uint number, int startBitPosition)
        {
            uint bitValue = ((number >> startBitPosition) & 0b11u);// Get startBitPosition和startBitPosition+1 value
            return bitValue;
        }

        /// <summary>
        /// Get Bits Value
        /// </summary>
        /// <param name="number">status</param>
        /// <param name="startBitPosition">Bit Position</param>
        /// <returns>return BitPosition value</returns>
        private uint GetBitValue(uint number, int startBitPosition)
        {
            uint bitValue = ((number >> startBitPosition) & 0b1u);// Get startBitPosition和startBitPosition+1 value
            return bitValue;
        }

        /// <summary>
        /// On , Off String Convert
        /// </summary>
        /// <param name="onoff">On or Off</param>
        /// <returns>on = true ; off = false</returns>
        private bool StrConvertOnOff(string onoff)
        {
            _logs.DebugMsg("[DisplayMangerPlugin] DisplayMangerPlugin into StrConvertOnOff ");

            if (string.Equals(onoff, "ON", StringComparison.OrdinalIgnoreCase))
                return true;
            else
                return false;
        }

        /// <summary>
        /// On , Off Uint Convert
        /// </summary>
        /// <param name="onoff">On or Off</param>
        /// <returns>on = 1 ; off = 0</returns>
        private uint StrConvertUint(string onoff)
        {
            _logs.DebugMsg("[DisplayMangerPlugin] DisplayMangerPlugin into StrConvertUint ");

            if (string.Equals(onoff, "ON", StringComparison.OrdinalIgnoreCase))
                return 1;
            else
                return 0;
        }

        #endregion

        protected virtual void OnVCPchanged(VCPchangedEventArgs e)
        {
            _logs.DebugMsg("[DisplayMangerPlugin] DisplayMangerPlugin brocast OnVCPchanged ...");

            //VCPchanged?.Invoke(this, e);
            EventHandler<VCPchangedEventArgs> handler = VCPchanged;
            if (handler != null)
                Task.Run(() => handler.Invoke(this, e));
        }

        protected virtual void OnDDCCIStatuschanged(DDCCIchangedEventArgs e)
        {
            _logs.DebugMsg("[DisplayMangerPlugin] DisplayMangerPlugin brocast OnDDCCIStatuschanged ...");

            //DDCCIStatuschanged?.Invoke(this, e);
            EventHandler<DDCCIchangedEventArgs> handler = DDCCIStatuschanged;
            if (handler != null)
                Task.Run(() => handler.Invoke(this, e));
        }

        protected virtual void OnMonitorinfoUpdatechanged(MonitorinfoUpdateEventArgs e)
        {
            _logs.DebugMsg("[DisplayMangerPlugin] DisplayMangerPlugin Broadcast MonitorinfoUpdatechanged ...");

            //MonitorUpdatechanged?.Invoke(this, e);
            EventHandler<MonitorinfoUpdateEventArgs> handler = MonitorinfoUpdated;
            if (handler != null)
                Task.Run(() => handler.Invoke(this, e)).ConfigureAwait(false);
        }

        protected virtual void OnDisplaychanged(DisplaychangedEventArgs e)
        {
            _logs.DebugMsg("[DisplayMangerPlugin] DisplayMangerPlugin brocast OnDisplaychanged ...");

            //Displaychanged?.Invoke(this, e);
            EventHandler<DisplaychangedEventArgs> handler = Displaychanged;
            if (handler != null)
                Task.Run(() => handler.Invoke(this, e));
        }

        /// <summary>
        /// Initialize Monitors ALS Info
        /// </summary>
        private void InitializeMonitorsList()
        {
            _logs.DebugMsg("[DisplayMangerPlugin] DisplayMangerPlugin into InitializeMonitorsList ...");

            _AllInfoMonitors.Clear();
            GetMonitors();

            _logs.DebugMsg("[DisplayMangerPlugin] InitializeMonitorsList AllInfoMonitors.count is " + _AllInfoMonitors.Count);
        }

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

                lock (_PluginConditionLock)
                {
                    if (pluginCondition is PluginErrorCondition)
                    {
                        _logs.DebugMsg($"[DisplayMangerPlugin] {nameof(GetCurrentVcpCoreCondition)} - Vcp Core Plugin is in an error condition");
                        _VcpCorePluginCondition = pluginCondition;
                        //_VcpCorePluginUsable = false;
                    }
                    else if (pluginCondition is PluginStartedCondition)
                    {
                        _logs.DebugMsg($"[DisplayMangerPlugin] {nameof(GetCurrentVcpCoreCondition)} - Vcp Core Plugin is in a started condition");
                        _VcpCorePluginCondition = pluginCondition;
                        //_VcpCorePluginUsable = true;
                        _VcpCorePlugin.VCPchanged += show_VCPchangedEventArgs;
                        _VcpCorePlugin.Displaychanged += show_DisplaychangedEventArgs;
                        _VcpCorePlugin.DDCCIStatuschanged += show_DDCCIchangedEventArgs;
                        _VcpCorePlugin.MonitorinfoUpdated += show_MonitorinfoUpdatechangedEventArgs;
                        InitializeMonitorsList();
                        InitializeAllALSInfo();
                    }
                }
            });
        }

        /// <summary>
        /// Catch OSD event
        /// </summary>
        /// <param name="sender">object type</param>
        /// <param name="e">VCP changed Event Args</param>
        private void show_VCPchangedEventArgs(object sender, VCPchangedEventArgs e)
        {
            uint.TryParse(e.value, NumberStyles.Integer, CultureInfo.CurrentCulture, out uint result2);
            _logs.DebugMsg("[DisplayMangerPlugin] Receive VcpChanged Event Notify from VcpCorePlugin");
            _logs.DebugMsg("[DisplayMangerPlugin] Send VcpChanged Event Notify from DisplayMangerPlugin");
            Trace.WriteLine("[DisplayMangerPlugin] show_VCPchangedEventArgs vcpcode = " + e.vcpcode.ToString() + " || monitor = " + e.monitor.edid.ModelName + " || e.Value = " + e.value.ToString());

            VCPchangedEventArgs _VCPchangedEventArgs = new VCPchangedEventArgs();
            _VCPchangedEventArgs.value = e.value;
            _VCPchangedEventArgs.monitor = e.monitor;

            ////0607 Bruce 自動旋轉畫面顧新增下面兩行程式碼
            //SetDisplayOrientation(_VCPchangedEventArgs);

            //0611 Dean
            if (e.vcpcode.Equals("66") &&
                uint.TryParse(e.value, NumberStyles.Integer, CultureInfo.CurrentCulture, out uint result))
            {
                //update target als config via target monitorinfo with e.value
                ALSConfig alsConfig = UpdateALSFeatureByValue(e.monitor, result);

                Trace.WriteLine("[DisplayMangerPlugin] show_VCPchangedEventArgs 0x66 " + result.ToString() + "|| AllValue = " + alsConfig.AllValue.ToString());
                if (alsConfig != null)// && alsConfig.AllValue != result)
                {
                    CheckisPrimaryMonitorSyncOnOff(e.monitor, alsConfig, e.vcpcode);//JIRA DDPMW-770
                    int idx = AllALSConfig.FindIndex(x => x.Edid.Equals(e.monitor.edid));
                    AllALSConfig[idx].isBusy = false;
                    _logs.DebugMsg($"[DisplayMangerPlugin] show_VCPchangedEventArgs {AllALSConfig[idx].ModelName} : isBusy = false");
                }
            }

            //Jason 0224 DisplayData_USB
            if (e.vcpcode.Equals("E7"))
            {
                SetMonitorAllUSB(e);
            }

            if (e.vcpcode.Equals("E9"))
            {
                SetMonitorVCPE9(e);
            }
            //Jason 0409 add DisplayData_Color
            if (e.vcpcode.Equals("DC") || e.vcpcode.Equals("F0") || e.vcpcode.Equals("14") || e.vcpcode.Equals("E2") || e.vcpcode.Equals("F4"))
            {
                SetMonitorColor(e);
            }

            _VCPchangedEventArgs.vcpcode = e.vcpcode;
            OnVCPchanged(_VCPchangedEventArgs);

            //Keep first
            if (e.vcpcode.Equals("67") || e.vcpcode.Equals("68"))
            {
                if (AllALSConfig == null || AllALSConfig.Count == 0) // No AllALSConfig
                {
                    Trace.WriteLine($"[DisplayMangerPlugin] show_VCPchangedEventArgs 67 / 68, AllALSConfig empty");
                    _logs.DebugMsg($" [DisplayMangerPlugin] show_VCPchangedEventArgs 67 / 68, AllALSConfig empty");
                    return;
                }

                if (!AllALSConfig.All(config => !config.isBusy)) // ALS Busy
                {
                    Trace.WriteLine($"[DisplayMangerPlugin] show_VCPchangedEventArgs 67 / 68, -------------------- ALS Busy --------------------");
                    _logs.DebugMsg($" [DisplayMangerPlugin] show_VCPchangedEventArgs 67 / 68, -------------------- ALS Busy --------------------");
                    return;
                }

                ALSConfig aconfig = AllALSConfig.Find(x => x.Edid.Equals(e.monitor.edid));
                if (aconfig != null)
                {
                    if (!aconfig.isPrimaryMonitorSync) // Monitor Non Primary
                    {
                        Trace.WriteLine($"[DisplayMangerPlugin] show_VCPchangedEventArgs 67 / 68, ModelName : {e.monitor.edid.ModelName}, ServiceTag : {e.monitor.edid.ServiceTag} -------------------- Not Primary --------------------");
                        _logs.DebugMsg($" [DisplayMangerPlugin] show_VCPchangedEventArgs 67 / 68, ModelName : {e.monitor.edid.ModelName}, ServiceTag : {e.monitor.edid.ServiceTag} -------------------- Not Primary --------------------");
                        return;
                    }

                    if (!aconfig.isAutoBrightness && !aconfig.isAutoColorTemp) // AutoBrightness off, AutoColorTemp off
                    {
                        Trace.WriteLine($"[DisplayMangerPlugin] show_VCPchangedEventArgs 67 / 68, ModelName : {e.monitor.edid.ModelName}, ServiceTag : {e.monitor.edid.ServiceTag}, AutoBrightness off, AutoColorTemp off ...");
                        _logs.DebugMsg($" [DisplayMangerPlugin] show_VCPchangedEventArgs 67 / 68, ModelName : {e.monitor.edid.ModelName}, ServiceTag : {e.monitor.edid.ServiceTag}, AutoBrightness off, AutoColorTemp off ...");
                        return;
                    }

                    if (e.vcpcode.Equals("67") && !aconfig.isAutoBrightness) // 67 event, but Primary on and AutoBrightness off
                    {
                        Trace.WriteLine($"[DisplayMangerPlugin] show_VCPchangedEventArgs 67 , ModelName : {e.monitor.edid.ModelName}, ServiceTag : {e.monitor.edid.ServiceTag}, AutoBrightness off ...");
                        _logs.DebugMsg($" [DisplayMangerPlugin] show_VCPchangedEventArgs 67 , ModelName : {e.monitor.edid.ModelName}, ServiceTag : {e.monitor.edid.ServiceTag}, AutoBrightness off ...");
                        return;
                    }

                    if (e.vcpcode.Equals("68") && !aconfig.isAutoColorTemp) // 68 event, but Primary on and AutoColorTemp off
                    {
                        Trace.WriteLine($"[DisplayMangerPlugin] show_VCPchangedEventArgs 68, ModelName : {e.monitor.edid.ModelName}, ServiceTag : {e.monitor.edid.ServiceTag}, AutoColorTemp off ...");
                        _logs.DebugMsg($" [DisplayMangerPlugin] show_VCPchangedEventArgs 68, ModelName : {e.monitor.edid.ModelName}, ServiceTag : {e.monitor.edid.ServiceTag}, AutoColorTemp off ...");
                        return;
                    }
                }
                else
                {
                    Trace.WriteLine($"[DisplayMangerPlugin] show_VCPchangedEventArgs 67 / 68, can not find ALSConfig ... ");
                    _logs.DebugMsg($" [DisplayMangerPlugin] show_VCPchangedEventArgs 67 / 68, can not find ALSConfig ... ");
                    return;
                }

                DateTime now = DateTime.Now;
                Trace.WriteLine($"[DisplayMangerPlugin] show_VCPchangedEventArgs 67 / 68, ModelName : {e.monitor.edid.ModelName}, ServiceTag : {e.monitor.edid.ServiceTag}, DateTime = {now.ToString()}");
                _logs.DebugMsg($"[DisplayMangerPlugin] show_VCPchangedEventArgs 67 / 68, ModelName : {e.monitor.edid.ModelName}, ServiceTag : {e.monitor.edid.ServiceTag}, DateTime = {now.ToString()}");
                if (LastProcessedTimestamps.TryGetValue(e.vcpcode, out DateTime lastProcessedTime)) // Timestamps
                {
                    Trace.WriteLine($"[DisplayMangerPlugin] show_VCPchangedEventArgs, lastProcessedTime = {lastProcessedTime.ToString()} || now - lastProcessedTime = {(now - lastProcessedTime).TotalSeconds.ToString()}");
                    _logs.DebugMsg($"[DisplayMangerPlugin] show_VCPchangedEventArgs, lastProcessedTime = {lastProcessedTime.ToString()} || now - lastProcessedTime = {(now - lastProcessedTime).TotalSeconds.ToString()}");
                    if ((now - lastProcessedTime).TotalSeconds < 4)
                    {
                        Trace.WriteLine($"[DisplayManagerPlugin] show_VCPchangedEventArgs, Skipping VCP code {e.vcpcode.ToString()} event ... Time to close {(now - lastProcessedTime).TotalSeconds.ToString()}");
                        _logs.DebugMsg($"[DisplayManagerPlugin] show_VCPchangedEventArgs, Skipping VCP code {e.vcpcode.ToString()} event ... Time to close {(now - lastProcessedTime).TotalSeconds.ToString()}");
                        return;
                    }
                    else
                    {
                        Trace.WriteLine($"[DisplayManagerPlugin] show_VCPchangedEventArgs, need to sync {(now - lastProcessedTime).TotalSeconds.ToString()} ...");
                        _logs.DebugMsg($"[DisplayManagerPlugin] show_VCPchangedEventArgs, need to sync {(now - lastProcessedTime).TotalSeconds.ToString()} ...");
                        LastProcessedTimestamps[e.vcpcode] = now;
                        PeocessALSTriggerEvent(e); // e.monitor must be primary
                        _logs.DebugMsg("[DisplayManagerPlugin] show_VCPchangedEventArgs => PeocessALSTriggerEvent, Completed processing VCP code 67 / 68.");
                    }
                }
                else
                {
                    Trace.WriteLine($"[DisplayManagerPlugin] show_VCPchangedEventArgs, lastProcessedTime = {lastProcessedTime.ToString()} || now = {now.ToString()}");
                    _logs.DebugMsg($"[DisplayManagerPlugin] show_VCPchangedEventArgs, lastProcessedTime = {lastProcessedTime.ToString()} || now = {now.ToString()}");
                    LastProcessedTimestamps[e.vcpcode] = now;
                    PeocessALSTriggerEvent(e); // e.monitor must be primary
                }
                _logs.DebugMsg("[DisplayManagerPlugin] show_VCPchangedEventArgs => PeocessALSTriggerEvent, End processing VCP code 67 / 68.");
            }

            //Dean 0614 handle brightness/contrast
            if (e.vcpcode.Equals("10") || e.vcpcode.Equals("12") || e.vcpcode.Equals("E2") || e.vcpcode.Equals("14") || e.vcpcode.Equals("F0") || e.vcpcode.Equals("DC"))
            {
                //1.if current primary monitor
                ALSConfig findconfig = AllALSConfig.Find(x => x.DisplayName.ToUpper(CultureInfo.InvariantCulture).Equals(e.monitor.DisplayName.ToUpper(CultureInfo.InvariantCulture)));

                ////DDPMW-771 OSD PASS but UI cause loop
                //if (findconfig == null || findconfig.isSupportALS == 0 )
                //{
                //    ALSConfig findpri = AllALSConfig.Find(x => x.isPrimaryMonitorSync == true);
                //    if (findpri != null && findpri.isAutoBrightness == true)
                //    {
                //        MonitorInfo mo = _AllInfoMonitors.Find(x => x.edid.SerialNumber.Equals(findpri.serialNumber));
                //        if (mo != null)
                //        {
                //            SetVCPCapability(mo, 0x66, SetBitsValue(findpri.AllValue, 0, 1));//set VCP command value, AutoBrightness to 0
                //        }
                //    }
                //}

                if (findconfig != null && findconfig.isPrimaryMonitorSync)//
                {
                    //2.do value sync
                    for (int i = 0; i < _AllInfoMonitors.Count; i++)
                    {
                        MonitorInfo mo = _AllInfoMonitors[i];
                        if (e.monitor.DisplayName.Equals(mo.DisplayName) == false)
                        {
                            ALSConfig findconfigtocheckmms = AllALSConfig.Find(x => x.DisplayName.ToUpper(CultureInfo.InvariantCulture).Equals(mo.DisplayName.ToUpper(CultureInfo.InvariantCulture)));
                            bool r = false;
                            if (findconfigtocheckmms != null)// || findconfigtocheckmms.isMMSEnable == false)//False need to set, if null or MMS true no action
                            {
                                if (uint.TryParse(e.value, NumberStyles.Integer, CultureInfo.CurrentCulture, out uint result3))
                                {
                                    //No need sync, PIMS - 285803
                                    //if (e.vcpcode.Equals("10"))
                                    //    SetVCPCapability(mo, 0x10, Convert.ToUInt32(e.value));
                                    //else if (e.vcpcode.Equals("12"))
                                    //    SetVCPCapability(mo, 0x12, Convert.ToUInt32(e.value));
                                    //else
                                    //if (e.vcpcode.Equals("14") || e.vcpcode.Equals("F0") || e.vcpcode.Equals("DC"))
                                    //{
                                    //    r = int.TryParse(e.vcpcode, System.Globalization.NumberStyles.HexNumber, CultureInfo.CurrentCulture, out int number);
                                    //    if (r) SetVCPCapability(mo, Convert.ToByte(number), Convert.ToUInt32(e.value));
                                    //}
                                }
                                else
                                {
                                    if (e.vcpcode.Equals("E2"))
                                    {
                                        SetVCPCapability(mo, "colorpreset", e.value);
                                    }
                                    else
                                        break;
                                }
                            }
                        }
                    }
                }
            }
            //Bruce 0820
            GamingChangeEventHandle(_VCPchangedEventArgs);
            if (e.vcpcode.Equals("EA"))
            {
                GetUSBCPrioritization(_VCPchangedEventArgs.monitor);
            }
            GetHDRStatus(_VCPchangedEventArgs.monitor, true).Wait();
        }

        private void PeocessALSTriggerEvent(VCPchangedEventArgs e)
        {
            Trace.WriteLine($"[DisplayMangerPlugin] PeocessALSTriggerEvent, e.ModelName : {e.monitor.edid.ModelName}, ServiceTag : {e.monitor.edid.ServiceTag},  e.value = {e.value.ToString()} ...... in");
            _logs.DebugMsg($"[DisplayMangerPlugin] PeocessALSTriggerEvent, e.ModelName : {e.monitor.edid.ModelName}, ServiceTag : {e.monitor.edid.ServiceTag} ...... in");
            if (_AllInfoMonitors.Count == 0)
            {
                Trace.WriteLine($"[DisplayMangerPlugin] PeocessALSTriggerEvent _AllInfoMonitors.Count == 0 ...");
                _logs.DebugMsg($"[DisplayMangerPlugin] PeocessALSTriggerEvent _AllInfoMonitors.Count == 0 ...");
                return;
            }
            var allInfoMonitorsSnapshot = _AllInfoMonitors.ToList();
            foreach (var targetMonitor in allInfoMonitorsSnapshot)
            {
                if (!e.monitor.edid.Equals(targetMonitor.edid)) // Get other monitor
                {
                    Trace.WriteLine($"[DisplayMangerPlugin] Foreach monitor is {e.monitor.modelName} | {e.monitor.edid.ServiceTag}");
                    Trace.WriteLine($"[DisplayMangerPlugin] Foreach monitor is {targetMonitor.modelName} | {targetMonitor.edid.ServiceTag}");

                    _logs.DebugMsg($"[DisplayMangerPlugin] Foreach monitor is {e.monitor.modelName} | {e.monitor.edid.ServiceTag}");
                    _logs.DebugMsg($"[DisplayMangerPlugin] Foreach monitor is {targetMonitor.modelName} | {targetMonitor.edid.ServiceTag}");

                    Task.Run(async () =>
                            {
                                try
                                {
                                    //待定義，先KEEP
                                    if (!SyncPrimaryMonitorBrightnessAndColorTemp(e.monitor, targetMonitor, e.vcpcode, e.value).Result)
                                    {
                                        Trace.WriteLine($"[DisplayMangerPlugin] SyncPrimaryMonitorBrightnessAndColorTemp false ...");
                                        _logs.DebugMsg($"[DisplayMangerPlugin] SyncPrimaryMonitorBrightnessAndColorTemp false ...");
                                        return;
                                    }
                                }
                                catch (Exception ex)
                                {
                                    _logs.DebugMsg($"[DisplayMangerPlugin] show_VCPchangedEventArgs Sync Task Exception: {ex.Message}");
                                }
                            });
                }
            }
            _logs.DebugMsg("[DisplayMangerPlugin] show_VCPchangedEventArgs vcpcode = 67 68 ... out");
        }

        private void show_DDCCIchangedEventArgs(object sender, DDCCIchangedEventArgs e)
        {
            _logs.DebugMsg("[DisplayMangerPlugin] Receive DDCCIStatuschanged Event Notify from VcpCorePlugin");
            _logs.DebugMsg("[DisplayMangerPlugin] Send DDCCIStatuschanged Event Notify from DisplayMangerPlugin");

            var monitor = _AllInfoMonitors.Find(m => m.edid.Equals(e.monitors.edid));
            monitor = e.monitors.Clone();

            DDCCIchangedEventArgs _DDCCIchangedEventArgs = new DDCCIchangedEventArgs();
            _DDCCIchangedEventArgs.DDCisON = e.DDCisON;
            _DDCCIchangedEventArgs.monitors = e.monitors.Clone();
            OnDDCCIStatuschanged(_DDCCIchangedEventArgs);
        }

        private void show_DisplaychangedEventArgs(object sender, DisplaychangedEventArgs e)
        {
            _logs.DebugMsg("[DisplayMangerPlugin] Receive DisplayChanged Event Notify from VcpCorePlugin");
            _logs.DebugMsg("[DisplayMangerPlugin] Send DisplayChanged Event Notify from DisplayMangerPlugin");

            _AllInfoMonitors = new List<MonitorInfo>(e.monitors);

            DisplaychangedEventArgs _displaychangedEventArgss = new DisplaychangedEventArgs();
            _displaychangedEventArgss.count = e.count;
            _displaychangedEventArgss.monitors = _AllInfoMonitors.ToList();
            OnDisplaychanged(_displaychangedEventArgss);

            //Re Get ALS
            InitializeAllALSInfo();
            //Robert_Lin, 2024-12-10 added to notify EAPlugin
            NotifyEAPluginAllInfoMonitorsChanged();
        }

        private void show_MonitorinfoUpdatechangedEventArgs(object sender, MonitorinfoUpdateEventArgs e)
        {
            _logs.DebugMsg("[DisplayMangerPlugin] Receive MonitorinfoUpdatechanged Event Notify from VcpCorePlugin");
            _logs.DebugMsg("[DisplayMangerPlugin] Send MonitorinfoUpdatechanged Event Notify from DisplayMangerPlugin");

            MonitorinfoUpdateEventArgs _EventArgss = new MonitorinfoUpdateEventArgs()
            {
                edid = e.edid,
                monitor = e.monitor,
            };
            OnMonitorinfoUpdatechanged(_EventArgss);
        }

        #endregion

        #region Event Handler

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
            if (!e.ChangedPlugins.OfType<IVcpCoreService>().Any()) return;

            InitializeVcpCorePlugin();
        }

        //Bruce, 2024-08-09 add new event
        private void OnHDRStatusChangeHandler(object sender, bool e)
        {
            HDRChangeEvent?.AsyncFireAndForget(this, e, System.Threading.CancellationToken.None);
        }

        #endregion

        #region Display properties

        private IDisplayProperties _DisplayPropertiesPlugin;
        private PluginCondition _DisplayPropertiesPluginCondition;

        #region Display properties implementation

        public Task<DisplaySupportedProperties> GetDisplaySupportedProperties(MonitorInfo monitorInfo)
        {
            _logs.DebugMsg($"[DisplayMangerPlugin] GetDisplaySupportedProperties {monitorInfo.modelName} start");
            DisplayPropertiesInfo rc = null;
            if (_DisplayPropertiesPlugin != null)
            {
                _logs.DebugMsg("[DisplayMangerPlugin] GetDisplaySupportedProperties _DisplayPropertiesPlugin.GetDisplaySupportedProperties go");
                rc = _DisplayPropertiesPlugin.GetDisplaySupportedProperties(monitorInfo).Result;
                _logs.DebugMsg($"[DisplayMangerPlugin] GetDisplaySupportedProperties rc.SupportedProperties.Properties.Count : {rc.SupportedProperties.Properties.Count}");
                _logs.DebugMsg($"[DisplayMangerPlugin] GetDisplaySupportedProperties rc.CurrentOrientation : {rc.CurrentOrientation}");
                if (_displayDataManger != null)
                {
                    _logs.DebugMsg("[DisplayMangerPlugin] find display data go");
                    if (_displayDataManger.GetMonitorDisplayPropertiesInfo(monitorInfo, out DisplayPropertiesInfo ret_DisplayPropertiesInfo))
                    {
                        _logs.DebugMsg("[DisplayMangerPlugin] GetDisplaySupportedProperties update display data go");
                        ret_DisplayPropertiesInfo.SupportedProperties = rc.SupportedProperties;
                        ret_DisplayPropertiesInfo.CurrentOrientation = rc.CurrentOrientation;
                    }
                    if (_displayDataManger.GetMonitorGamingDisplayPropertiesInfo(monitorInfo, out GamingDisplayPropertiesInfo ret_GamingDisplayPropertiesInfo))
                    {
                        _logs.DebugMsg("[DisplayMangerPlugin] GetMonitorGamingDisplayPropertiesInfo update display data go");
                        ret_GamingDisplayPropertiesInfo.SupportedProperties = rc.SupportedProperties;
                    }
                }
            }
            _logs.DebugMsg($"[DisplayMangerPlugin] GetDisplaySupportedProperties {monitorInfo.modelName} done");
            return Task.FromResult(rc.SupportedProperties);
        }

        public Task<DisplayPropertiesInfo> GetDisplayPropertiesInfo(MonitorInfo monitorInfos)
        {
            _logs.DebugMsg("[DisplayMangerPlugin] GetDisplayPropertiesInfo start");
            DisplayPropertiesInfo ret_DisplayPropertiesInfo = new DisplayPropertiesInfo();
            if (_displayDataManger != null &&
                _displayDataManger.GetMonitorDisplayPropertiesInfo(monitorInfos, out ret_DisplayPropertiesInfo))
            {
                return Task.FromResult(ret_DisplayPropertiesInfo);
            }
            string setParam = "USB-C Prioritization";
            string capabilityString = monitorInfos.CapabilityString;
            USBCPrioritizationType PrioritizationType = USBCPrioritizationType.Unknow;
            bool supportedHDR = IsSupportHDR(capabilityString), supportedUSBC = IsSupportUSBCPrioritization(capabilityString);
            bool? supported_OSD_Orientation = IsSupportWriteOSDOrientation(capabilityString);
            _logs.DebugMsg($"[DisplayMangerPlugin] GetDisplayPropertiesInfo supportedHDR:{supportedHDR}");
            _logs.DebugMsg($"[DisplayMangerPlugin] GetDisplayPropertiesInfo supportedUSBC:{supportedUSBC}");
            _logs.DebugMsg($"[DisplayMangerPlugin] GetDisplayPropertiesInfo supported_OSD_Orientation:{supported_OSD_Orientation}");
            bool isHDREnable = false;
            if (supportedHDR)
            {
                int count = 0;
                ObjGetVCP ObjGetVCP = null;
                do
                {
                    if (IsDisposed)
                    {
                        _logs?.DebugMsg_1($"GetDisplayPropertiesInfo IsDisposed");
                        break;
                    }
                    ObjGetVCP = GetVCPCapability(monitorInfos, 0xE2).Result;
                    count++;
                } while (ObjGetVCP.result != true && count < 3);
                if (ObjGetVCP != null && ObjGetVCP.result == true)
                {
                    uint[] stand = new uint[] { 0x25, 0x23, 0x24, 0x26, 0x27, 0x3A, 0x3B, 0x3C };
                    foreach (uint hdrType in stand)
                    {
                        if (hdrType.Equals((uint)ObjGetVCP.value))
                        {
                            isHDREnable = true;
                            _logs.DebugMsg($"[DisplayMangerPlugin] GetDisplayPropertiesInfo isHDREnable:{isHDREnable}");
                            break;
                        }
                    }
                }
            }
            if (supportedUSBC)
            {
                PrioritizationType = GetUSBCPrioritization(monitorInfos);
            }
            ret_DisplayPropertiesInfo = _DisplayPropertiesPlugin.GetDisplayPropertiesInfo(monitorInfos, capabilityString, supportedHDR, isHDREnable, supportedUSBC, PrioritizationType).Result;
            ret_DisplayPropertiesInfo.Supported_OSD_Orientation = supported_OSD_Orientation;
            if (ret_DisplayPropertiesInfo.Supported_OSD_Orientation != null)
            {
                string osd_Orientation_str = GetOSDOrientation(monitorInfos).Result;
                for (int i = 0; i < OrientationString.Length; i++)
                {
                    if (osd_Orientation_str.Equals(OrientationString[i]))
                    {
                        ret_DisplayPropertiesInfo.Current_OSD_Orientation = (DisplayOrientation)(i - 1);
                    }
                }
                if (ret_DisplayPropertiesInfo != null && ret_DisplayPropertiesInfo.SupportedProperties != null)
                {
                    ret_DisplayPropertiesInfo.SupportedProperties.OSD_Orientations = IsSupportOSDOrientation(capabilityString);
                }
            }
            if (_displayDataManger != null)
            {
                _displayDataManger.SetMonitorDisplayPropertiesInfo(monitorInfos, ret_DisplayPropertiesInfo);
            }
            return Task.FromResult(ret_DisplayPropertiesInfo);
        }

        public Task<DisplayCurrentPropertiesInfo> GetCurrentDisplayProperties(MonitorInfo monitorInfo)
        {
            _logs.DebugMsg($"[DisplayMangerPlugin] GetCurrentDisplayProperties start");
            DisplayCurrentPropertiesInfo ret = new DisplayCurrentPropertiesInfo();
            if (_DisplayPropertiesPlugin != null)
            {
                ret = _DisplayPropertiesPlugin.GetCurrentDisplayProperties(monitorInfo).Result;
                string setParam = "USB-C Prioritization";
                string capabilityString = monitorInfo.CapabilityString;
                USBCPrioritizationType PrioritizationType = USBCPrioritizationType.Unknow;
                bool supportedHDR = IsSupportHDR(capabilityString), supportedUSBC = IsSupportUSBCPrioritization(capabilityString);
                _logs.DebugMsg($"[DisplayMangerPlugin] GetCurrentDisplayProperties supportedHDR:{supportedHDR}");
                _logs.DebugMsg($"[DisplayMangerPlugin] GetCurrentDisplayProperties supportedUSBC:{supportedUSBC}");
                if (supportedHDR)
                {
                    ret.isHDREnable = _DisplayPropertiesPlugin.GetHDRStatus(monitorInfo.edid).Result;
                    _logs.DebugMsg($"[DisplayMangerPlugin] GetCurrentDisplayProperties isHDREnable:{ret.isHDREnable}");
                }
                if (supportedUSBC)
                {
                    int count = 0;
                    ObjGetVCP ObjGetVCP = null;
                    do
                    {
                        if (IsDisposed)
                        {
                            _logs?.DebugMsg_1($"GetCurrentDisplayProperties IsDisposed");
                            break;
                        }
                        ObjGetVCP = GetVCPCapability(monitorInfo, setParam).Result;
                        count++;
                    } while (ObjGetVCP.result != true && count < 3);
                    if (ObjGetVCP != null && ObjGetVCP.result == true)
                    {
                        PrioritizationType = ObjGetVCP.value.ToString() == "High Data Speed" ? USBCPrioritizationType.HighDataSpeed : USBCPrioritizationType.HighResolution;
                        _logs.DebugMsg($"[DisplayMangerPlugin] GetCurrentDisplayProperties PrioritizationType:{PrioritizationType}");
                    }
                    ret.USBCPrioritizationType = PrioritizationType;
                }
            }
            _logs.DebugMsg($"[DisplayMangerPlugin] GetCurrentDisplayProperties done");
            return Task.FromResult(ret);
        }

        //Bruce, 2024-08-09 Modify the incoming value.
        public Task<bool> SetDisplayPropertiest(MonitorInfo monitorInfos, Properties properties, DisplayOrientation orientation)
        {
            _logs.DebugMsg($"[DisplayMangerPlugin] SetDisplayPropertiest start");
            bool ret = false;
            if (monitorInfos != null && properties != null)
            {
                _logs.DebugMsg($"[DisplayMangerPlugin] set_OSD_Orientation go");
                //Bruce, 2024-08-09 Added the feature that if the screen is rotated, the OSD will also be rotated together.
                bool? set_OSD_Orientation_Ret = SetOSDOrientation(monitorInfos, OrientationString[(int)orientation + 1]).Result;
                _logs.DebugMsg($"[DisplayMangerPlugin] set_OSD_Orientation set_OSD_Orienset_OSD_Orientation_Rettation : {set_OSD_Orientation_Ret}");
                isSWSetOrientation = true;
                _logs.DebugMsg($"[DisplayMangerPlugin] _DisplayPropertiesPlugin.SetDisplayPropertiest go");
                ret = _DisplayPropertiesPlugin.SetDisplayPropertiest(monitorInfos.DisplayName, properties, orientation).Result;
                isSWSetOrientation = false;
                if (ret &&
                    _displayDataManger != null &&
                    _displayDataManger.GetMonitorDisplayPropertiesInfo(monitorInfos, out DisplayPropertiesInfo ret_DisplayPropertiesInfo))
                {
                    bool cleanCurrentFlae = false, setCurrentFlae = false;
                    if (ret_DisplayPropertiesInfo.CurrentOrientation != orientation)
                    {
                        ret_DisplayPropertiesInfo.CurrentOrientation = orientation;
                        ret_DisplayPropertiesInfo.SupportedProperties = _DisplayPropertiesPlugin.GetDisplaySupportedProperties(monitorInfos).Result.SupportedProperties;
                    }
                    else
                    {
                        foreach (Properties tempProperties in ret_DisplayPropertiesInfo.SupportedProperties.Properties)
                        {
                            if (tempProperties.isCurrent && !cleanCurrentFlae)
                            {
                                tempProperties.isCurrent = false;
                                cleanCurrentFlae = true;
                            }
                            if (tempProperties.Resolutions_Width == properties.Resolutions_Width &&
                                tempProperties.Resolutions_High == properties.Resolutions_High &&
                                tempProperties.Frequency == properties.Frequency &&
                                !setCurrentFlae)
                            {
                                tempProperties.isCurrent = true;
                                setCurrentFlae = true;
                            }
                            if (cleanCurrentFlae && setCurrentFlae)
                            {
                                break;
                            }
                        }
                    }
                }
            }
            _logs.DebugMsg($"[DisplayMangerPlugin] SetDisplayPropertiest ret : {ret}");
            _logs.DebugMsg($"[DisplayMangerPlugin] SetDisplayPropertiest done");
            return Task.FromResult(ret);
        }

        public Task<bool> SetResolutions(MonitorInfo monitorInfos, Properties properties)
        {
            _logs.DebugMsg($"[DisplayMangerPlugin] SetResolutions start");
            bool ret = false;
            if (monitorInfos != null && properties != null)
            {
                _logs.DebugMsg($"[DisplayMangerPlugin] _DisplayPropertiesPlugin.SetResolutions go");
                isSWSetOrientation = true;
                ret = _DisplayPropertiesPlugin.SetResolutions(monitorInfos.DisplayName, properties).Result;
                isSWSetOrientation = false;
                if (ret &&
                    _displayDataManger != null &&
                    _displayDataManger.GetMonitorDisplayPropertiesInfo(monitorInfos, out DisplayPropertiesInfo ret_DisplayPropertiesInfo))
                {
                    bool cleanCurrentFlae = false, setCurrentFlae = false;
                    foreach (Properties tempProperties in ret_DisplayPropertiesInfo.SupportedProperties.Properties)
                    {
                        if (tempProperties.isCurrent && !cleanCurrentFlae)
                        {
                            tempProperties.isCurrent = false;
                            cleanCurrentFlae = true;
                        }
                        if (tempProperties.Resolutions_Width == properties.Resolutions_Width &&
                            tempProperties.Resolutions_High == properties.Resolutions_High &&
                            tempProperties.Frequency == properties.Frequency &&
                            !setCurrentFlae)
                        {
                            tempProperties.isCurrent = true;
                            setCurrentFlae = true;
                        }
                        if (cleanCurrentFlae && setCurrentFlae)
                        {
                            break;
                        }
                    }
                }
            }
            _logs.DebugMsg($"[DisplayMangerPlugin] SetResolutions done");
            return Task.FromResult(ret);
        }

        public Task<bool> SetOrientation(MonitorInfo monitorInfos, DisplayOrientation orientation)
        {
            _logs.DebugMsg($"[DisplayMangerPlugin] SetOrientation start");
            bool ret = false;
            if (monitorInfos != null)
            {
                _logs.DebugMsg($"[DisplayMangerPlugin] SetOSDOrientation go");
                SetOSDOrientation(monitorInfos, OrientationString[(int)orientation + 1]).Wait();
                isSWSetOrientation = true;
                ret = _DisplayPropertiesPlugin.SetOrientation_New(monitorInfos.DisplayName, orientation).Result;
                //ret = _DisplayPropertiesPlugin.SetOrientation(monitorInfos.DisplayName, orientation).Result;
                isSWSetOrientation = false;
                if (ret &&
                    _displayDataManger != null)
                {
                    _displayDataManger.GetMonitorDisplayPropertiesInfo(monitorInfos, out DisplayPropertiesInfo ret_DisplayPropertiesInfo);
                    if (ret_DisplayPropertiesInfo != null &&
                        ret_DisplayPropertiesInfo.SupportedProperties != null &&
                        ret_DisplayPropertiesInfo.SupportedProperties.Properties != null &&
                        ret_DisplayPropertiesInfo.SupportedProperties.Properties.Count > 0)
                    {
                        ret_DisplayPropertiesInfo.CurrentOrientation = orientation;
                        ret_DisplayPropertiesInfo.SupportedProperties = _DisplayPropertiesPlugin.GetDisplaySupportedProperties(monitorInfos).Result.SupportedProperties;
                    }
                }
            }
            _logs.DebugMsg($"[DisplayMangerPlugin] SetOrientation done");
            return Task.FromResult(ret);
        }

        public Task<bool> CallWindowsDisplaySetting()
        {
            return Task.FromResult(_DisplayPropertiesPlugin.CallWindowsDisplaySetting().Result);
        }

        public Task<bool> GetHDRStatus(MonitorInfo monitorInfos, bool reGet = false)
        {
            _logs.DebugMsg($"[DisplayMangerPlugin] GetHDRStatus start");
            string capabilityString = monitorInfos.CapabilityString;
            bool supportedHDR = IsSupportHDR(capabilityString);
            _logs.DebugMsg($"[DisplayMangerPlugin] GetHDRStatus supportedHDR :{supportedHDR}");
            if (supportedHDR)
            {
                if (!reGet && _displayDataManger != null)
                {
                    if (_displayDataManger.GetMonitorDisplayPropertiesInfo(monitorInfos, out DisplayPropertiesInfo ret_DisplayPropertiesInfo))
                    {
                        return Task.FromResult(ret_DisplayPropertiesInfo.isHDREnable);
                    }
                }
                _logs.DebugMsg($"[DisplayMangerPlugin] _DisplayPropertiesPlugin.GetHDRStatus go");
                bool HDREnable = _DisplayPropertiesPlugin.GetHDRStatus(monitorInfos.edid).Result;
                if (_displayDataManger != null)
                {
                    if (_displayDataManger.GetMonitorDisplayPropertiesInfo(monitorInfos, out DisplayPropertiesInfo ret_DisplayPropertiesInfo))
                    {
                        ret_DisplayPropertiesInfo.isHDREnable = HDREnable;
                    }
                }
                return Task.FromResult(HDREnable);
            }
            _logs.DebugMsg($"[DisplayMangerPlugin] SetOrientation done");
            return Task.FromResult(false);
        }

        public Task<bool> SetHDRStatus(MonitorInfo monitorInfos, bool onoff)
        {
            bool ret = false;
            bool vcp_Ret = false;
            _logs.DebugMsg($"[DisplayMangerPlugin] SetHDRStatus start");
            if (_DisplayPropertiesPlugin != null)
            {
                _logs.DebugMsg($"[DisplayMangerPlugin] SetHDRStatus on/off : {(onoff ? "on" : "off")}");
                if (onoff)
                {
                    _logs.DebugMsg($"[DisplayMangerPlugin] _DisplayPropertiesPlugin.SetExtendMode go");
                    _DisplayPropertiesPlugin.SetExtendMode(monitorInfos);
                    _logs.DebugMsg($"[DisplayMangerPlugin] _DisplayPropertiesPlugin.SetExtendMode done");
                    if (monitorInfos.CapabilityString != "" && monitorInfos.CapabilityString.Length > 10)
                    {
                        if (monitorInfos.CapabilityString.Contains("F4"))
                        {
                            _logs.DebugMsg($"[DisplayMangerPlugin] monitorInfos.CapabilityString have F4");
                            _logs.DebugMsg($"[DisplayMangerPlugin] GetCurrentGaming_HDRType go");
                            Gaming_HDRType gaming_HDRType = GetCurrentGaming_HDRType(monitorInfos).Result;
                            _logs.DebugMsg($"[DisplayMangerPlugin] GetCurrentGaming_HDRType done gaming_HDRType : {gaming_HDRType.ToString()}");
                            if (!gaming_HDRType.Equals(Gaming_HDRType.Disable))
                            {
                                if (gaming_HDRType.Equals(Gaming_HDRType.Off))
                                {
                                    _logs.DebugMsg($"[DisplayMangerPlugin] SetGaming_HDRType go");
                                    bool b = SetGaming_HDRType(monitorInfos, Gaming_HDRType.Desktop).Result;
                                    _logs.DebugMsg($"[DisplayMangerPlugin] SetGaming_HDRType done b : {b}");
                                }
                            }
                        }
                        else
                        {
                            string desktop_E2 = "27";
                            string desktop_F0 = "34";
                            string[] ss = monitorInfos.CapabilityString.Split("E2(");
                            ss = ss[1].Split(")");
                            ss = ss[0].Split(" ");
                            for (int i = 0; i < ss.Length; i++)
                            {
                                _logs.DebugMsg($"[DisplayMangerPlugin] SetHDRStatus ss:{ss[i]}");
                                if (ss[i].Equals(desktop_E2))
                                {
                                    var hexStyle = System.Globalization.NumberStyles.HexNumber;
                                    int number;
                                    if (int.TryParse(desktop_F0, hexStyle, CultureInfo.CurrentCulture, out number))
                                    {
                                        _logs.DebugMsg($"[DisplayMangerPlugin] SetHDRStatus SetVCPCapability go");
                                        _logs.DebugMsg($"[DisplayMangerPlugin] SetHDRStatus SetVCPCapability(monitorInfos, {0xF0},{(uint)number} )");
                                        vcp_Ret = SetVCPCapability(monitorInfos, 0xF0, (uint)number).Result;
                                        _logs.DebugMsg($"[DisplayMangerPlugin] SetHDRStatus SetVCPCapability done vcp_Ret : {vcp_Ret}");
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                _logs.DebugMsg($"[DisplayMangerPlugin] _DisplayPropertiesPlugin is null");
            }
            _logs.DebugMsg($"[DisplayMangerPlugin] _DisplayPropertiesPlugin.SetHDRStatus go");
            int count = 0;
            do
            {
                ret = _DisplayPropertiesPlugin.SetHDRStatus(monitorInfos.edid, onoff).Result;
                if (ret == false)
                {
                    Task.Delay(1000).Wait();
                }
                count++;
            } while (ret == false && count < 10);
            if (ret &&
                _displayDataManger != null &&
                _displayDataManger.GetMonitorDisplayPropertiesInfo(monitorInfos, out DisplayPropertiesInfo ret_DisplayPropertiesInfo))
            {
                ret_DisplayPropertiesInfo.isHDREnable = onoff;
            }
            _logs.DebugMsg($"[DisplayMangerPlugin] _DisplayPropertiesPlugin.SetHDRStatus done ret : {ret}");
            _logs.DebugMsg($"[DisplayMangerPlugin] SetHDRStatus done");
            return Task.FromResult(ret && vcp_Ret);
        }

        public Task<bool> SetUSBCPrioritizationType(MonitorInfo monitorInfos, USBCPrioritizationType type)
        {
            bool ret = false;
            try
            {
                _logs.DebugMsg($"[DisplayMangerPlugin] SetUSBCPrioritizationType start");
                _logs.DebugMsg($"[DisplayMangerPlugin] SetUSBCPrioritizationType ontype:{type}");
                if (type != USBCPrioritizationType.Unknow)
                {
                    string setParam = "USB-C Prioritization";
                    string PrioritizationType = type == USBCPrioritizationType.HighDataSpeed ? "High Data Speed" : "High Resolution";
                    _logs.DebugMsg($"[DisplayMangerPlugin] SetUSBCPrioritizationType PrioritizationType is {PrioritizationType}");
                    _logs.DebugMsg($"[DisplayMangerPlugin] SetUSBCPrioritizationType SetVCPCapability go");
                    if (SetVCPCapability(monitorInfos, setParam, PrioritizationType).Result)
                    {
                        _logs.DebugMsg($"[DisplayMangerPlugin] SetUSBCPrioritizationType SetVCPCapability is done");
                        ret = true;
                    }
                }
                else
                {
                    _logs.DebugMsg($"[DisplayMangerPlugin] SetUSBCPrioritizationType type is Unknow");
                }
            }
            catch (Exception ex)
            {
                _logs.DebugMsg($"[DisplayMangerPlugin] SetUSBCPrioritizationType error:{ex.Message}");
            }
            if (ret)
            {
                if (_displayDataManger != null)
                {
                    if (_displayDataManger.GetMonitorDisplayPropertiesInfo(monitorInfos, out DisplayPropertiesInfo ret_DisplayPropertiesInfo))
                    {
                        ret_DisplayPropertiesInfo.USBCPrioritizationType = type;
                    }
                }
            }
            _logs.DebugMsg($"[DisplayMangerPlugin] SetUSBCPrioritizationType done");
            return Task.FromResult(ret);
        }

        //0603 Bruce 根據VCPChange事件來判斷是否AA(自動旋轉畫面)被更改了，如果是的話將旋轉角度傳入插件的方法裡去設定OS的方向，設定是否鎖定
        public void SetEnableLockOrientation(bool isLock)
        {
            _logs.DebugMsg($"[DisplayMangerPlugin] SetEnableLockOrientation isLock : {isLock}");
            isLockOrientation = isLock;
        }

        /// <summary>
        /// Set screen orientation for all monitors when monitors are plugged in and out
        /// </summary>
        /// <param name="monitorInfos">all monitors</param>
        /// <returns></returns>
        public Task<List<bool>> SetDisplayOrientation(List<MonitorInfo> monitorInfos)
        {
            _logs.DebugMsg($"[DisplayMangerPlugin] Plugged trigger SetDisplayOrientation start");
            _logs.DebugMsg($"[DisplayMangerPlugin] SetDisplayOrientation monitorInfos.Count : {monitorInfos.Count}");
            bool[] bools = new bool[monitorInfos.Count];

            for (int i = 0; i < monitorInfos.Count; i++)
            {
                bool? supportWriteOSD = IsSupportWriteOSDOrientation(monitorInfos[i].CapabilityString);
                if (supportWriteOSD != true)
                {
                    _logs.DebugMsg($"[DisplayMangerPlugin] SetDisplayOrientation {monitorInfos[i].modelName} support write OSD : {supportWriteOSD}");
                    continue;
                }
                /* 1119 Bruce
                  int count = 0;
                  ObjGetVCP ObjGetVCP;
                  _logs.DebugMsg($"[DisplayMangerPlugin] SetDisplayOrientation GetVCPCapability go");
               do
               {
                   ObjGetVCP = GetVCPCapability(monitorInfos[i], 0xAA).Result;
                   count++;
               } while (ObjGetVCP.result != true && count < 3);*/
                _logs.DebugMsg($"[DisplayMangerPlugin] GetCurrentDisplayOrientation go");
                DisplayOrientation displayOrientation = GetCurrentDisplayOrientation(monitorInfos[i].DisplayName).Result;
                _logs.DebugMsg($"[DisplayMangerPlugin] GetCurrentDisplayOrientation done displayOrientation : {displayOrientation}");
                if ((int)displayOrientation + 1 < OrientationString.Length)
                {
                    _logs.DebugMsg($"[DisplayMangerPlugin] SetOSDOrientation go");
                    bool? ret = SetOSDOrientation(monitorInfos[i], OrientationString[(int)displayOrientation + 1]).Result;
                    bools[i] = ret == true ? true : false;
                    _logs.DebugMsg($"[DisplayMangerPlugin] SetOSDOrientation done ret : {ret}");
                }
                /* 1119 Bruce
                  _logs.DebugMsg($"[DisplayMangerPlugin] SetDisplayOrientation ObjGetVCP.result :{ObjGetVCP.result}");
                if (ObjGetVCP.result == true)
                {
                    uint retValue;
                    _logs.DebugMsg($"[DisplayMangerPlugin] SetDisplayOrientation ObjGetVCP.value :{ObjGetVCP.value}");
                    if (uint.TryParse(ObjGetVCP.value.ToString(), out retValue))
                    {
                        _logs.DebugMsg($"[DisplayMangerPlugin] SetDisplayOrientation retValue :{retValue}");
                        if (_DisplayPropertiesPlugin != null)
                        {
                            _logs.DebugMsg($"[DisplayMangerPlugin] GetCurrentDisplayOrientation go");
                            DisplayOrientation currentOrientation = _DisplayPropertiesPlugin.GetCurrentDisplayOrientation(monitorInfos[i].DisplayName).Result;
                            _logs.DebugMsg($"[DisplayMangerPlugin] currentOrientation : {currentOrientation}");
                            DisplayOrientation orientation = (DisplayOrientation)(retValue - 1);
                            _logs.DebugMsg($"[DisplayMangerPlugin] orientation : {orientation}");
                            if (!currentOrientation.Equals(orientation))
                            {
                                _logs.DebugMsg($"[DisplayMangerPlugin] SetDisplayPropertiest go");
                                Properties properties = new Properties();
                                bools[i] = SetDisplayPropertiest(monitorInfos[i], properties, orientation).Result;
                            }
                        }
                    }
                }*/
            }
            _logs.DebugMsg($"[DisplayMangerPlugin] SetDisplayOrientation done");
            return Task.FromResult(bools.ToList());
        }

        public Task<string> GetOSDOrientation(MonitorInfo monitorInfos)
        {
            _logs.DebugMsg($"[DisplayMangerPlugin] GetOSDOrientation start");
            int count = 0;
            ObjGetVCP ObjGetVCP;
            _logs.DebugMsg($"[DisplayMangerPlugin] GetOSDOrientation GetVCPCapability go");
            do
            {
                ObjGetVCP = GetVCPCapability(monitorInfos, 0xAA).Result;
                count++;
            } while (ObjGetVCP.result != true && count < 3);
            _logs.DebugMsg($"[DisplayMangerPlugin] GetOSDOrientation ObjGetVCP.result :{ObjGetVCP.result}");
            if (ObjGetVCP.result == true)
            {
                _logs.DebugMsg($"[DisplayMangerPlugin] GetOSDOrientation ObjGetVCP.value :{ObjGetVCP.value.ToString()}");
                uint retValue;
                if (uint.TryParse(ObjGetVCP.value.ToString(), out retValue))
                {
                    _logs.DebugMsg($"[DisplayMangerPlugin] GetOSDOrientation retValue :{retValue}");
                    return Task.FromResult(OrientationString[retValue]);
                }
            }
            _logs.DebugMsg($"[DisplayMangerPlugin] GetOSDOrientation done");
            return Task.FromResult("");
        }

        public Task<bool?> SetOSDOrientation(MonitorInfo monitorInfos, string Orientation)
        {
            bool? ret = null;
            _logs.DebugMsg($"[DisplayMangerPlugin] SetOSDOrientation start");
            _logs.DebugMsg($"[DisplayMangerPlugin] SetOSDOrientation orientation : {Orientation}");
            if (monitorInfos != null && !string.IsNullOrEmpty(Orientation))
            {
                _logs.DebugMsg($"[DisplayMangerPlugin] IsSupportWriteOSDOrientation go");
                if (IsSupportWriteOSDOrientation(monitorInfos.CapabilityString) == true)
                {
                    ret = false;
                    for (int i = 1; i < OrientationString.Length; i++)
                    {
                        if (Orientation.ToUpper().Equals(OrientationString[i].ToUpper()))
                        {
                            _logs.DebugMsg($"[DisplayMangerPlugin] SetVCPCapability go");
                            _logs.DebugMsg($"[DisplayMangerPlugin] SetVCPCapability(monitorInfo, 0xAA, {(uint)(i & 0xFFFF)})");
                            ret = SetVCPCapability(monitorInfos, 0xAA, (uint)(i & 0xFFFF)).Result;
                        }
                    }
                }
                else
                {
                    _logs.DebugMsg($"[DisplayMangerPlugin] SetOSDOrientation Is no Support write OSD Orientation");
                }
            }
            _logs.DebugMsg($"[DisplayMangerPlugin] SetOSDOrientation ret : {ret}");
            _logs.DebugMsg($"[DisplayMangerPlugin] SetOSDOrientation done");
            return Task.FromResult(ret);
        }

        private USBCPrioritizationType GetUSBCPrioritization(MonitorInfo monitorInfo)
        {
            string setParam = "USB-C Prioritization";
            USBCPrioritizationType PrioritizationType = USBCPrioritizationType.Unknow;
            int count = 0;
            ObjGetVCP ObjGetVCP;
            do
            {
                ObjGetVCP = GetVCPCapability(monitorInfo, setParam).Result;
                count++;
            } while (ObjGetVCP.result != true && count < 3);
            _logs.DebugMsg($"[DisplayMangerPlugin] GetUSBCPrioritization ObjGetVCP.result:{ObjGetVCP.result}");
            if (ObjGetVCP.result == true)
            {
                PrioritizationType = ObjGetVCP.value.ToString() == "High Data Speed" ? USBCPrioritizationType.HighDataSpeed : USBCPrioritizationType.HighResolution;
                _logs.DebugMsg($"[DisplayMangerPlugin] GetUSBCPrioritization PrioritizationType:{PrioritizationType}");
                if (_displayDataManger != null)
                {
                    if (_displayDataManger.GetMonitorDisplayPropertiesInfo(monitorInfo, out DisplayPropertiesInfo displayPropertiesInfo))
                    {
                        displayPropertiesInfo.USBCPrioritizationType = PrioritizationType;
                    }
                }
            }
            return PrioritizationType;
        }

        /// <summary>
        /// Trigger display screen rotation when VCP has AA event
        /// </summary>
        /// <param name="vcpchangedEventArgs"></param>
        /// <returns></returns>
        private Task<bool> SetDisplayOrientation(VCPchangedEventArgs vcpchangedEventArgs)
        {
            bool ret = true;
            _logs.DebugMsg($"[DisplayMangerPlugin] VCPchangedEventArgs trigger SetDisplayOrientation start");
            _logs.DebugMsg($"[DisplayMangerPlugin] SetDisplayOrientation isLockOrientation : {isLockOrientation}");
            _logs.DebugMsg($"[DisplayMangerPlugin] SetDisplayOrientation isSWSetOrientation : {isSWSetOrientation}");
            if (!isLockOrientation && !isSWSetOrientation)
            {
                _logs.DebugMsg($"[DisplayMangerPlugin] SetDisplayOrientation vcpchangedEventArgs.vcpcode:{vcpchangedEventArgs.vcpcode}");
                if (vcpchangedEventArgs.vcpcode == "AA")
                {
                    int retValue;
                    _logs.DebugMsg($"[DisplayMangerPlugin] SetDisplayOrientation vcpchangedEventArgs.value:{vcpchangedEventArgs.value}");
                    if (int.TryParse(vcpchangedEventArgs.value, out retValue))
                    {
                        _logs.DebugMsg($"[DisplayMangerPlugin] SetDisplayOrientation retValue:{retValue}");
                        DisplayOrientation orientation = (DisplayOrientation)(retValue - 1);
                        _logs.DebugMsg($"[DisplayMangerPlugin] SetDisplayOrientation orientation:{orientation}");
                        //OSDOrientationChangeEvent?.AsyncFireAndForget(this, orientation, System.Threading.CancellationToken.None);
                        Properties properties = new Properties();
                        _logs.DebugMsg($"[DisplayMangerPlugin] SetDisplayOrientation go");
                        ret = SetDisplayPropertiest(vcpchangedEventArgs.monitor, properties, orientation).Result;
                    }
                }
            }
            _logs.DebugMsg($"[DisplayMangerPlugin] SetDisplayOrientation done");
            return Task.FromResult(ret);
        }

        private bool IsSupportHDR(string s)
        {
            _logs.DebugMsg($"[DisplayMangerPlugin] IsSupportHDR s : {s}");
            try
            {
                if (s == "" || s.Length < 10)
                {
                    return false;
                }
                string[] ss = s.Split("E2(");
                ss = ss[1].Split(")");
                _logs.DebugMsg($"[DisplayMangerPlugin] IsSupportHDR ss : {ss}");
                ss = ss[0].Split(" ");
                string[] stand = new string[] { "25", "23", "24", "26", "27", "3A", "3B", "3C" };
                for (int i = 0; i < ss.Length; i++)
                {
                    for (int j = 0; j < stand.Length; j++)
                    {
                        if (ss[i].Equals(stand[j]))
                        {
                            //Console.WriteLine("OK");
                            return (true);
                        }
                    }
                }
                return (false);
            }
            catch
            {
                return (false);
            }
        }

        private bool IsSupportUSBCPrioritization(string s)
        {
            _logs.DebugMsg($"[DisplayMangerPlugin] IsSupportUSBCPrioritization s : {s}");
            try
            {
                if (s == "" || s.Length < 10)
                {
                    return false;
                }
                string[] ss = s.Split("EA(");
                ss = ss[1].Split(")");
                _logs.DebugMsg($"[DisplayMangerPlugin] IsSupportUSBCPrioritization ss : {ss}");
                ss = ss[0].Split(" ");
                string[] stand = new string[] { "F8", "F800", "F801" };
                for (int i = 0; i < ss.Length; i++)
                {
                    for (int j = 0; j < stand.Length; j++)
                    {
                        if (ss[i].Equals(stand[j]))
                        {
                            //Console.WriteLine("OK");
                            return (true);
                        }
                    }
                }
                return (false);
            }
            catch
            {
                return (false);
            }
        }

        private bool? IsSupportWriteOSDOrientation(string s)
        {
            _logs.DebugMsg($"[DisplayMangerPlugin] IsSupportWriteOSDOrientation start");
            _logs.DebugMsg($"[DisplayMangerPlugin] IsSupportWriteOSDOrientation s : {s}");
            bool? ret = null;
            try
            {
                if (!string.IsNullOrEmpty(s) && s.Length > 10 && s.Contains("AA"))
                {
                    string[] ss = s.Split("AA(");
                    if (ss.Length >= 2)
                    {
                        _logs.DebugMsg($"[DisplayMangerPlugin] IsSupportWriteOSDOrientation s.Split(AA) : {ss[1]}");
                        ss = ss[1].Split(")");
                        if (ss.Length >= 1)
                        {
                            _logs.DebugMsg($"[DisplayMangerPlugin] IsSupportWriteOSDOrientation ss[1].Split() step 2 : {ss[0]}");
                            ret = ss[0].Contains("00");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logs.DebugMsg($"[DisplayMangerPlugin] IsSupportWriteOSDOrientation error : {ex.Message}");
            }
            _logs.DebugMsg($"[DisplayMangerPlugin] IsSupportWriteOSDOrientation ret : {ret}");
            _logs.DebugMsg($"[DisplayMangerPlugin] IsSupportWriteOSDOrientation done");
            return (ret);
        }

        private DisplayOrientation[] IsSupportOSDOrientation(string s)
        {
            _logs.DebugMsg($"[DisplayMangerPlugin] IsSupportWriteOSDOrientation start");
            _logs.DebugMsg($"[DisplayMangerPlugin] IsSupportWriteOSDOrientation s : {s}");
            List<DisplayOrientation> ret = new List<DisplayOrientation>();
            try
            {
                if (!string.IsNullOrEmpty(s) && s.Length > 10 && s.Contains("AA"))
                {
                    string[] ss = s.Split("AA(");
                    if (ss.Length >= 2)
                    {
                        _logs.DebugMsg($"[DisplayMangerPlugin] IsSupportWriteOSDOrientation s.Split(AA) : {ss[1]}");
                        ss = ss[1].Split(")");
                        if (ss.Length >= 1)
                        {
                            _logs.DebugMsg($"[DisplayMangerPlugin] IsSupportWriteOSDOrientation ss.Split() step 2 : {ss[0]}");
                            ss = ss[0].Split(" ");
                            if (ss != null)
                            {
                                _logs.DebugMsg($"[DisplayMangerPlugin] IsSupportWriteOSDOrientation ss.Split( ) step 3 : {ss}");
                                for (int i = 0; i < ss.Length; i++)
                                {
                                    if (ss[i] != "00" && int.TryParse(ss[i], out int retValue))
                                    {
                                        ret.Add((DisplayOrientation)(retValue - 1));
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logs.DebugMsg($"[DisplayMangerPlugin] IsSupportWriteOSDOrientation error : {ex.Message}");
            }
            _logs.DebugMsg($"[DisplayMangerPlugin] IsSupportWriteOSDOrientation ret.Count : {ret.Count}");
            _logs.DebugMsg($"[DisplayMangerPlugin] IsSupportWriteOSDOrientation done");
            return (ret.ToArray());
        }

        public Task<string> GetMonitorCurrentResolution(MonitorInfo monitor)
        {
            var rc = string.Empty;

            if (_DisplayPropertiesPlugin != null)
            {
                if (_displayDataManger != null)
                {
                    if (_displayDataManger.GetMonitorDisplayPropertiesInfo(monitor, out DisplayPropertiesInfo ret_DisplayPropertiesInfo))
                    {
                        foreach (Properties tmp in ret_DisplayPropertiesInfo.SupportedProperties.Properties)
                        {
                            if (tmp.isCurrent)
                            {
                                rc = tmp.Resolutions_Width.ToString() + " X " + tmp.Resolutions_High.ToString();
                                return Task.FromResult(rc);
                            }
                        }
                    }
                }
                var r = _DisplayPropertiesPlugin.GetDisplaySupportedProperties(monitor).Result;
                if (r != null)
                {
                    foreach (Properties tmp in r.SupportedProperties.Properties)
                    {
                        if (tmp.isCurrent)
                        {
                            rc = tmp.Resolutions_Width.ToString() + " X " + tmp.Resolutions_High.ToString();
                            break;
                        }
                    }
                }
            }
            return Task.FromResult(rc);
        }

        public Task<string> GetMonitorMaxResolution(MonitorInfo monitor)
        {
            var rc = string.Empty;

            if (_DisplayPropertiesPlugin != null)
            {
                if (_displayDataManger != null)
                {
                    if (_displayDataManger.GetMonitorDisplayPropertiesInfo(monitor, out DisplayPropertiesInfo ret_DisplayPropertiesInfo))
                    {
                        foreach (Properties tmp in ret_DisplayPropertiesInfo.SupportedProperties.Properties)
                        {
                            if (tmp.isRecommended)
                            {
                                rc = tmp.Resolutions_Width.ToString() + " X " + tmp.Resolutions_High.ToString();
                                return Task.FromResult(rc);
                            }
                        }
                    }
                }
                var r = _DisplayPropertiesPlugin.GetDisplaySupportedProperties(monitor).Result;
                if (r != null)
                {
                    foreach (Properties tmp in r.SupportedProperties.Properties)
                    {
                        if (tmp.isRecommended)
                        {
                            rc = tmp.Resolutions_Width.ToString() + " X " + tmp.Resolutions_High.ToString();
                            break;
                        }
                    }
                }
            }
            return Task.FromResult(rc);
        }

        public Task<string> GetMonitorRefreshRate(MonitorInfo monitor)
        {
            var rc = string.Empty;

            if (_DisplayPropertiesPlugin != null)
            {
                if (_displayDataManger != null)
                {
                    if (_displayDataManger.GetMonitorDisplayPropertiesInfo(monitor, out DisplayPropertiesInfo ret_DisplayPropertiesInfo))
                    {
                        foreach (Properties tmp in ret_DisplayPropertiesInfo.SupportedProperties.Properties)
                        {
                            if (tmp.isCurrent)
                            {
                                rc = tmp.Frequency.ToString();
                                return Task.FromResult(rc);
                            }
                        }
                    }
                }
                var r = _DisplayPropertiesPlugin.GetDisplaySupportedProperties(monitor).Result;
                if (r != null)
                {
                    foreach (Properties tmp in r.SupportedProperties.Properties)
                    {
                        if (tmp.isCurrent)
                        {
                            rc = tmp.Frequency.ToString();
                            break;
                        }
                    }
                }
            }
            return Task.FromResult(rc);
        }

        public Task<DisplayOrientation> GetCurrentDisplayOrientation(string DisplayName)
        {
            if (_DisplayPropertiesPlugin != null)
            {
                DisplayOrientation rc = _DisplayPropertiesPlugin.GetCurrentDisplayOrientation(DisplayName).Result;
                return Task.FromResult(rc);
            }

            return Task.FromResult(DisplayOrientation.Unknow);
        }

        #endregion

        private void InitializeDisplayPropertiesPlugin()
        {
            if (_DisplayPropertiesPlugin != null)
                return;

            _DisplayPropertiesPlugin = _agent.PluginManager.FindPluginByType<IDisplayProperties>(PluginResolution.Dynamic);

            if (_DisplayPropertiesPlugin is IFrameworkPluginConditionNotification DisplayPropertiesCondition)
            {
                DisplayPropertiesCondition.PluginConditionChangeHandler += OnDisplayPropertiesPluginConditionChangeHandler;
                GetCurrentDisplayPropertiesCondition();
            }
        }

        private void GetCurrentDisplayPropertiesCondition()
        {
            _ = Task.Run(async () =>
            {
                var pluginCondition = await (_DisplayPropertiesPlugin as IFrameworkPluginConditionNotification)?.CurrentConditionAsync();

                lock (_PluginConditionLock)
                {
                    if (pluginCondition is PluginErrorCondition)
                    {
                        _logs.DebugMsg($"[DisplayMangerPlugin] {nameof(GetCurrentDisplayPropertiesCondition)} - Display Properties Plugin is in an error condition");
                        _DisplayPropertiesPluginCondition = pluginCondition;
                    }
                    else if (pluginCondition is PluginStartedCondition)
                    {
                        _logs.DebugMsg($"[DisplayMangerPlugin] {nameof(GetCurrentDisplayPropertiesCondition)} - Display Properties Plugin is in a started condition");
                        _DisplayPropertiesPluginCondition = pluginCondition;
                        //Bruce, 2024-08-09 add new event
                        _DisplayPropertiesPlugin.HDRChangeEvent += OnHDRStatusChangeHandler;
                    }
                    else if (pluginCondition is PluginRunningCondition)
                    {
                        _logs.DebugMsg($"[DisplayMangerPlugin] {nameof(GetCurrentDisplayPropertiesCondition)} - Display Properties Plugin is in a running condition");
                        _DisplayPropertiesPluginCondition = pluginCondition;
                        //Bruce, 2024-08-09 add new event
                        _DisplayPropertiesPlugin.HDRChangeEvent += OnHDRStatusChangeHandler;
                    }
                }
            });
        }

        private void OnDisplayPropertiesPluginConditionChangeHandler(object sender, EventArgs e)
        {
            GetCurrentDisplayPropertiesCondition();
        }

        #endregion

        #region PipPbpManagerPlugin

        private IPipPbpService _pipPbpService;
        private PluginCondition _pipPbpPluginCondition;

        private void InitializePipPbpManagerPlugin()
        {
            if (_pipPbpService != null)
                return;

            _pipPbpService = _agent.PluginManager.FindPluginByType<IPipPbpService>(PluginResolution.Dynamic);
            if (_pipPbpService is IFrameworkPluginConditionNotification pipPbpCondition)
            {
                pipPbpCondition.PluginConditionChangeHandler += PipPbpCondition_PluginConditionChangeHandler;
                GetCurrentPipPbpCondition();
            }
        }

        public Task GetSettingsPlugin(ISettingsManagerDev SettingsPlugin)
        {
            _SettingsPlugin = SettingsPlugin;
            return Task.CompletedTask;
        }

        private void GetCurrentPipPbpCondition()
        {
            _ = Task.Run(async () =>
            {
                var pluginCondition = await (_pipPbpService as IFrameworkPluginConditionNotification)?.CurrentConditionAsync();

                lock (_PluginConditionLock)
                {
                    if (pluginCondition is PluginErrorCondition)
                    {
                        _logs.DebugMsg($"[DisplayMangerPlugin] {nameof(GetCurrentPipPbpCondition)} - PipPbp Plugin is in an error condition");
                        _pipPbpPluginCondition = pluginCondition;
                    }
                    else if (pluginCondition is PluginStartedCondition)
                    {
                        _logs.DebugMsg($"[DisplayMangerPlugin] {nameof(GetCurrentPipPbpCondition)} -PipPbp Plugin is in a started condition");
                        _pipPbpPluginCondition = pluginCondition;
                    }
                    else if (pluginCondition is PluginRunningCondition)
                    {
                        _logs.DebugMsg($"[DisplayMangerPlugin] {nameof(GetCurrentPipPbpCondition)} -PipPbp Plugin is in a running condition");
                        _pipPbpPluginCondition = pluginCondition;
                    }
                }
            });
        }

        private void PipPbpCondition_PluginConditionChangeHandler(object sender, EventArgs e)
        {
            GetCurrentPipPbpCondition();
        }

        public Task<UInt16[]> GetPipPbpCapabilitiesWords(MonitorInfo monitorInfo)
        {
            if (_pipPbpService != null)
            {
                return _pipPbpService.GetCapabilitiesWords(monitorInfo);
            }
            return Task.FromResult<UInt16[]>(null);
        }

        public Task<bool> SetPipModeOff(MonitorInfo monitorInfo)
        {
            if (_pipPbpService != null)
            {
                return _pipPbpService.SetPipModeOff(monitorInfo);
            }
            return Task.FromResult(false);
        }

        public Task<bool> SetPipModeSmall(MonitorInfo monitorInfo)
        {
            if (_pipPbpService != null)
            {
                return _pipPbpService.SetPipModeSmall(monitorInfo);
            }
            return Task.FromResult(false);
        }

        public Task<bool> SetPipModeLarge(MonitorInfo monitorInfo)
        {
            if (_pipPbpService != null)
            {
                return _pipPbpService.SetPipModeLarge(monitorInfo);
            }
            return Task.FromResult(false);
        }

        public Task<bool> TogglePipSize(MonitorInfo monitorInfo)
        {
            if (_pipPbpService != null)
            {
                return _pipPbpService.TogglePipSize(monitorInfo);
            }
            return Task.FromResult(false);
        }

        public Task<bool> TogglePipPosition(MonitorInfo monitorInfo)
        {
            if (_pipPbpService != null)
            {
                return _pipPbpService.TogglePipPosition(monitorInfo);
            }
            return Task.FromResult(false);
        }

        public Task<bool> SetPbpMode(MonitorInfo monitorInfo, UInt16 modeCode)
        {
            if (_pipPbpService != null)
            {
                return _pipPbpService.SetPbpMode(monitorInfo, modeCode);
            }
            return Task.FromResult(false);
        }

        public Task<bool> VideoSwap(MonitorInfo monitorInfo, UInt16 x, UInt16 y)
        {
            if (_pipPbpService != null)
            {
                return _pipPbpService.VideoSwap(monitorInfo, x, y);
            }
            return Task.FromResult(false);
        }

        public Task<ObjGetVCP> GetPxpMode(MonitorInfo monitorInfo)
        {
            if (_pipPbpService != null)
            {
                return _pipPbpService.GetPxpMode(monitorInfo);
            }
            return Task.FromResult<ObjGetVCP>(new ObjGetVCP() { result = false, value = 0xff });
        }

        public Task<List<UInt16>> GetSubInputList(MonitorInfo monitorInfo)
        {
            if (_pipPbpService != null)
            {
                if (monitorInfo.CapabilityDic.ContainsKey("E8"))
                {
                    return _pipPbpService.GetSubInputList(monitorInfo);
                }
                else
                {
                    List<UInt16> subInputList = new List<UInt16>();
                    Dictionary<string, InputInfo> inputList = GetInputSourcelist(monitorInfo).Result;
                    if (inputList != null)
                    {
                        foreach (var input in inputList)
                        {
                            if (input.Key != monitorInfo.inputSource)
                            {
                                subInputList.Add((UInt16)input.Value.Code);
                                break;
                            }
                        }
                    }
                    return Task.FromResult(subInputList);
                }
            }
            return Task.FromResult<List<UInt16>>(null);
        }

        public Task<List<InputSourceObj>> GetSubInputs(MonitorInfo monitorInfo)
        {
            if (_pipPbpService != null)
            {
                return _pipPbpService.GetSubInputs(monitorInfo);
            }
            return Task.FromResult<List<InputSourceObj>>(null);
        }

        public Task<bool> SetSubInputs(MonitorInfo monitorInfo, InputSourceObj? sub1, InputSourceObj? sub2, InputSourceObj? sub3)
        {
            if (_pipPbpService != null)
            {
                return _pipPbpService.SetSubInputs(monitorInfo, sub1, sub2, sub3);
            }
            return Task.FromResult<bool>(false);
        }

        public Task<bool> UsbSwitch(MonitorInfo monitorInfo, UInt16 target = 0)
        {
            if (_pipPbpService != null)
            {
                return _pipPbpService.UsbSwitch(monitorInfo, target);
            }
            return Task.FromResult(false);
        }

        #endregion

        #region USBKVMService implementation

        public Task<Dictionary<string, PCsInfo>> GetUSBKVMPCsList(MonitorInfo monitorInfo, Dictionary<string, InputInfo> inputList, List<InputSourceObj> subInputList)
        {
            _PCsList = new Dictionary<string, PCsInfo>();
            string currentInput = GetCurrentInput(monitorInfo).Result;

            if (inputList.Count <= 0) // 2024-06-19 Elie, fix exception.
                return Task.FromResult(_PCsList);

            if (subInputList == null)
                return Task.FromResult(_PCsList);

            PCsInfo PC = new PCsInfo();
            PC.InputType = currentInput;

            InputInfo outValue;
            if (inputList.TryGetValue(currentInput, out outValue))
            {
                PC.InputName = outValue.InputName;
                PC.USBUpstream = GetUSBUpstream(monitorInfo, currentInput).Result;
                PC.Code = outValue.Code;
                _PCsList.Add("PC1", PC);
                int i = 1;
                int loop = subInputList.Count + 1; // get input sub
                foreach (var item in subInputList)
                {
                    if (i >= loop)
                    {
                        break;
                    }
                    PC = new PCsInfo();
                    PC.InputType = item.Name;

                    InputInfo outSubValue;

                    if (inputList.TryGetValue(item.Name, out outSubValue))
                    {
                        PC.InputName = outSubValue.InputName;
                        PC.USBUpstream = GetUSBUpstream(monitorInfo, item.Name).Result;
                        PC.Code = outSubValue.Code;
                        _PCsList.Add("PC" + (i + 1).ToString(), PC);
                    }
                    i = i + 1;
                }

                return Task.FromResult(_PCsList);
            }
            else
            {
                // 2024-06-19 ERROR Handle.
            }

            return Task.FromResult(_PCsList);
        }

        #endregion

        #region EasyArrange implementation

        #region Private members - EasyArrange

        private bool _isEaPluginConfigured = false;
        private IEasyArrangeService _eaService;
        private PluginCondition _eaPluginCondition;

        #endregion Private members - EasyArrange

        #region Properties - EasyArrange

        /// <summary>
        /// The last error string after a EAPlugin method return error.
        /// </summary>
        public string EALastError
        {
            get
            {
                if (_eaService == null)
                    return "EAPlugin is not constructed.";
                if (!_isEaPluginConfigured)
                    return "EAPlugin Condition is NOT configured.";
                return _eaService.EALastError;
            }
        }

        #endregion Properties - EasyArrange

        //Robert_Lin, 2024-9-13 Remove unused interfaces
        //public event EventHandler<string> EAEditCompleted;

        public event EventHandler<string> EAEditStarted;

        public event EventHandler<EAArgs> EAEditReturn;

        public event EventHandler<EAArgs> EASettingsChanged;

        private void InitializeEAPlugin()
        {
            if (_eaService != null)
                return;

            _eaService = _agent.PluginManager.FindPluginByType<IEasyArrangeService>(PluginResolution.Dynamic);
            if (_eaService is IFrameworkPluginConditionNotification eaCondition)
            {
                eaCondition.PluginConditionChangeHandler += EaCondition_PluginConditionChangeHandler;
                GetCurrentEaCondition();
            }
        }

        private void GetCurrentEaCondition()
        {
            _ = Task.Run(async () =>
            {
                var pluginCondition = await (_eaService as IFrameworkPluginConditionNotification)?.CurrentConditionAsync();

                lock (_PluginConditionLock)
                {
                    if (pluginCondition is PluginErrorCondition)
                    {
                        _logs.DebugMsg($"[DisplayMangerPlugin] {nameof(GetCurrentEaCondition)} - EA Plugin is in an error condition");
                        _eaPluginCondition = pluginCondition;
                    }
                    else if (pluginCondition is PluginStartedCondition)
                    {
                        _logs.DebugMsg($"[DisplayMangerPlugin] {nameof(GetCurrentEaCondition)} - EA Plugin is in a started condition");
                        _eaPluginCondition = pluginCondition;
                        if (!_isEaPluginConfigured)
                        {
                            _isEaPluginConfigured = true;
                            //Robert_Lin, 2024-9-13 Remove unused interfaces
                            //_eaService.EditCompleted += _eaService_EditCompleted;
                            _eaService.EditStarted += _eaService_EditStarted;
                            //Robert_Lin, 2024-8-4
                            _eaService.EditReturn += _eaService_EditReturn;
                            //Robert_Lin, 2024-10-8
                            _eaService.EASettingsChanged += _eaService_EASettingsChanged;
                        }
                    }
                    else if (pluginCondition is PluginRunningCondition)
                    {
                        _logs.DebugMsg($"[DisplayMangerPlugin] {nameof(GetCurrentEaCondition)} - EA Plugin is in a running condition");
                        _eaPluginCondition = pluginCondition;

                        if (!_isEaPluginConfigured)
                        {
                            _isEaPluginConfigured = true;
                            //Robert_Lin, 2024-9-13 Remove unused interfaces
                            //_eaService.EditCompleted += _eaService_EditCompleted;
                            _eaService.EditStarted += _eaService_EditStarted;
                            //Robert_Lin, 2024-8-4
                            _eaService.EditReturn += _eaService_EditReturn;
                            //Robert_Lin, 2024-10-8
                            _eaService.EASettingsChanged += _eaService_EASettingsChanged;
                        }
                    }
                }
            });
        }

        private void _eaService_EASettingsChanged(object sender, EAArgs e)
        {
            if (EASettingsChanged != null)
            {
                Task.Run(() => EASettingsChanged.Invoke(this, e));
            }
        }

        private void _eaService_EditStarted(object sender, string e)
        {
            if (EAEditStarted != null)
            {
                Task.Run(() => EAEditStarted.Invoke(this, e));
            }
        }

        //Robert_Lin, 2024-9-13 Remove unused interfaces
        //private void _eaService_EditCompleted(object sender, string e)
        //{
        //    if (EAEditCompleted != null)
        //    {
        //        Task.Run(() => EAEditCompleted.Invoke(this, e));
        //    }
        //}

        private void EaCondition_PluginConditionChangeHandler(object sender, EventArgs e)
        {
            GetCurrentEaCondition();
        }

        // Robert_Lin, 2025-1-7, EAPlugin.IsFunctionEnabled is deleted.
        //public Task<bool> SetEAFunctionEnabled(bool isEnabled)
        //{
        //    if (_eaService != null)
        //    {
        //        _eaService.IsFunctionEnabled = isEnabled;
        //        return Task.FromResult(true);
        //    }
        //    return Task.FromResult(false);
        //}

        //Robert_Lin, 2025-1-7, EAPlugin.IsFunctionEnabled is deleted.
        //public Task<ObjGetVCP> GetEAFunctionEnabled()
        //{
        //    if (_eaService != null)
        //    {
        //        return Task.FromResult<ObjGetVCP>(new ObjGetVCP()
        //        { result = false, value = _eaService.IsFunctionEnabled });
        //    }
        //    return Task.FromResult<ObjGetVCP>(new ObjGetVCP() { result = false, value = false });
        //}

        public Task<bool> SetEAWrokSplit(MonitorInfo monitorInfo, int cellCount, char splitKey, List<double>? settings)
        {
            if (_eaService != null)
            {
                return _eaService.SetEAWrokSplit(monitorInfo, cellCount, splitKey, settings);
            }
            return Task.FromResult(false);
        }

        public Task<bool> NotifyEASelectedLayoutChanged(MonitorInfo monitorInfo, SplitJson spJson)
        {
            if (_eaService != null)
            {
                return _eaService.NotifyEASelectedLayoutChanged(monitorInfo, spJson);
            }
            return Task.FromResult(false);
        }

        //Robert_Lin, 2024-9-13 Remove unused interfaces
        //public Task<bool> RequestEditSplit(MonitorInfo monitorInfo, int cellCount, char splitKey, string customName, List<double>? settings = null)
        //{
        //    if (_eaService != null)
        //    {
        //        return _eaService.RequestEditSplit(monitorInfo, cellCount, splitKey, customName, settings);
        //    }
        //    return Task.FromResult(false);
        //}

        //Robert_Lin,2024-8-4
        public Task<bool> EAEditCommand(MonitorInfo monitorInfo, EAArgs args)
        {
            if (_eaService != null)
            {
                return _eaService.EditCommand(monitorInfo, args);
            }
            return Task.FromResult(false);
        }

        private void _eaService_EditReturn(object sender, EAArgs e)
        {
            if (_eaService != null)
            {
                Task.Run(() =>
                {
                    if (EAEditReturn != null)
                        EAEditReturn.Invoke(this, e);
                });
            }
        }

        //public Task<bool> EAReloadMonitorSettings(MonitorInfo monitorInfo)
        //{
        //    if (_eaService != null)
        //    {
        //        Task.Run(() =>
        //        {
        //            return _eaService.EAReloadMonitorSettings(monitorInfo);
        //        });
        //    }
        //    return Task.FromResult(false);
        //}

        public Task<bool> ReloadEzSettings()
        {
            if (_eaService != null)
            {
                return _eaService.ReloadEzSettings();
            }
            else
            {
                _logs.DebugMsg($"[DisplayMangerPlugin] @ DisplayManager.ReloadEzSettings(): _eaService is in null");
            }
            return Task.FromResult(false);
        }

        public Task<bool> SetEASelectedLayout(MonitorInfo monitorInfo, SplitJson spJson)
        {
            if (_eaService != null)
            {
                return _eaService.SetEASelectedLayout(monitorInfo, spJson);
            }
            else
            {
                _logs.DebugMsg($"[DisplayMangerPlugin] @ DisplayManager.SetEASelectedLayout(): _eaService is in null");
            }
            return Task.FromResult(false);
        }

        public Task<bool> SetEASelectedLayout(MonitorInfo monitorInfo, int eaId)
        {
            if (_eaService != null)
            {
                return _eaService.SetEASelectedLayout(monitorInfo, eaId);
            }
            else
            {
                _logs.DebugMsg($"[DisplayMangerPlugin] @ DisplayManager.SetEASelectedLayout(): _eaService is in null");
            }
            return Task.FromResult(false);
        }

        /// <summary>
        /// Return current Span across multiple monitor option is Enabled/Disabled;
        /// Note that it's different with EzSettings.IsSpanAcrossMultiMonitors (=ON|OFF)
        /// </summary>
        /// <returns>True=Enabled; False=Disabled</returns>
        public Task<bool> GetIsSpanEnabled()
        {
            if (_eaService != null)
            {
                return _eaService.GetIsSpanEnabled();
            }
            else
            {
                _logs.DebugMsg($"[DisplayMangerPlugin] @ DisplayManager.GetIsSpanEnabled(): _eaService is in null");
            }
            return Task.FromResult(false);
        }

        /// <summary>
        /// General notification  to EAPlugin from other Plugins inside DDPM.SA.User
        /// </summary>
        /// <param name="eaArgs"></param>
        /// <returns></returns>
        public Task<bool> NotifyEAMessage(EAArgs eaArgs)
        {
            if (_eaService != null)
            {
                return _eaService.NotifyEAMessage(eaArgs);
            }
            else
            {
                _logs.DebugMsg($"[DisplayMangerPlugin] @ DisplayManager.NotifyEAMessage(): _eaService is in null");
            }
            return Task.FromResult(false);
        }

        /// <summary>
        /// Launch Apps in the specified EasyMemory Profile, and arrange their window to the EasyArrange layout.
        /// This method is moved from EzMemoryPlugin. Can be called from UI (EzMemory module) and SA (EzMemoryPlugin).
        /// </summary>
        /// <param name="sortApps">List of AppInfos which are load from EM profile.</param>
        /// <param name="moInfo">MonitorInfo to specify the target monitor to be arranged.</param>
        /// <param name="eAid">EAID of a EasyArrange layout. [1~49] are preset layout, [1000~1004] are saved custom layout.</param>
        /// <returns></returns>
        public Task<bool> LaunchAndArrangeAppsWithEzArrange(Dictionary<String, Bind_AddFullPage_AppCollectionData> sortApps, MonitorInfo moInfo, int eAid)
        {
            if (_eaService != null)
            {
                return _eaService.LaunchAndArrangeAppsWithEzArrange(sortApps, moInfo, eAid);
            }
            else
            {
                _logs.DebugMsg($"[DisplayMangerPlugin] @ LaunchAndArrangeAppsWithEzArrange(): _eaService is in null");
            }
            return Task.FromResult(false);
        }

        //Robert_Lin, 2024-12-10
        /// <summary>
        /// Raise a event to notify EAPlugin that DisplayManager AllInfoMonitors is changed
        /// </summary>
        private void NotifyEAPluginAllInfoMonitorsChanged()
        {
            if (_agent != null)
            {
                EventManagerArgs args = new EventManagerArgs();
                _agent.RaiseEvent(AgentEventNames.AllInfoMonitorsChanged, this, args);
            }
        }

        #endregion

        #region OutReport

        public Task<List<MonitorAssetReport>> GetMonitorAssetReport(List<MonitorInfo> monitorInfos)
        {
            _logs.DebugMsg($"[DisplayMangerPlugin] {nameof(GetMonitorAssetReport)} start");
            List<MonitorAssetReport> ret = new List<MonitorAssetReport>();
            List<byte[]> currentMnoitorByteArr = GetCurrentMonitorEdid();
            for (int i = 0; i < currentMnoitorByteArr.Count; i++)
            {
                EDID edid_FromeVCP = CommonFun.getEDID(currentMnoitorByteArr[i]);
                byte[] edid_byte = currentMnoitorByteArr[i];
                var (Resolutions_Width, Resolutions_High, Frequency) = Preferred_Detailed_Timing.GetResolutionAndRefreshRate(edid_byte);
                int daysBetween = DaysBetweenWeekStartAndToday(edid_FromeVCP.Year, edid_FromeVCP.Week);
                string manufacturer_Name = Vendor_Product_Identification.Manufacturer_Name(edid_byte);
                string product_Id = Vendor_Product_Identification.Product_Id(edid_byte).PadLeft(4, '0');
                MonitorInfo? monitorInfo = monitorInfos.Find(o => BitConverter.ToString((byte[])currentMnoitorByteArr[i]).Replace("-", "").StartsWith(o.edid.Edid));
                string modelName = $"{manufacturer_Name} {manufacturer_Name + product_Id}";
                string manufacturer = manufacturer_Name;
                string orientation = "N/A";
                string activeHour = "N/A";
                string powerStatus = "N/A";
                string technologyType = "N/A";
                string controllerId = "N/A";
                string firmwareVersion = "N/A";
                string connection = "DisplayPort";
                string serialNumber = "N/A";
                string optimalResolution = "N/A";
                _logs.DebugMsg($"[DisplayMangerPlugin] {nameof(GetMonitorAssetReport)} monitorInfo is null :{monitorInfo == null}");
                if (monitorInfo != null)
                {
                    modelName = monitorInfo.modelName;
                    if (modelName.StartsWith("AW"))
                    {
                        modelName = $"Alienware {modelName}";
                    }
                    else
                    {
                        modelName = $"Dell {modelName}";
                    }
                    manufacturer = manufacturer_Name.Equals("DEL") ? "Dell" : "";
                    orientation = GetOSDOrientation(monitorInfo).Result;
                    if (string.IsNullOrEmpty(orientation))
                    {
                        orientation = "N/A";
                    }
                    activeHour = $"{GetActiveHour(monitorInfo)} hours";
                    powerStatus = GetPowerStatus(monitorInfo);
                    controllerId = GetControllerID(monitorInfo);
                    firmwareVersion = monitorInfo.FwVersion;
                    connection = monitorInfo.inputSource;
                    serialNumber = $"{edid_FromeVCP.ServiceTag}-{edid_FromeVCP.SerialNumber}";
                }
                if (Frequency == 60)
                {
                    technologyType = "LCD";
                }
                else if (Frequency == 75 || Frequency == 85)
                {
                    technologyType = "CRT";
                }
                if (Resolutions_Width > 0 && Resolutions_High > 0 && Frequency > 0)
                {
                    optimalResolution = $"{Resolutions_Width}x{Resolutions_High} at {Frequency}Hz";
                }
                _logs.DebugMsg($"[DisplayMangerPlugin] {nameof(GetMonitorAssetReport)} ret.Add : {modelName}");
                ret.Add(new MonitorAssetReport()
                {
                    ModelName = modelName,
                    Manufacturer = manufacturer,
                    PlugandPlayID = manufacturer_Name + product_Id,
                    SerialNumber = serialNumber,
                    DateOfManufacture = $"{edid_FromeVCP.Year} ISO week {edid_FromeVCP.Week}",
                    Age = $"{daysBetween.ToString()} days",
                    ScreenSize = $"{Display_Parameters.Max_Horizontal_Image_Size(edid_byte)} x {Display_Parameters.Max_Vertical_Image_Size(edid_byte)} mm ({Display_Parameters.Max_Display_Size(edid_byte)} in)",
                    InputFrequency = "N/A",
                    PhysicalOrientation = orientation,
                    TechnologyType = technologyType,
                    UsageTime = activeHour,
                    ControllerID = controllerId,
                    FirmwareVersion = firmwareVersion,
                    PowerState = powerStatus,
                    OptimalResolution = optimalResolution,
                    OptimalAspectRatio = Preferred_Detailed_Timing.Active_Ratio(edid_byte),
                    Connection = connection
                });
            }
            _logs.DebugMsg($"[DisplayMangerPlugin] {nameof(GetMonitorAssetReport)} end");
            return Task.FromResult(ret);
        }

        private List<byte[]> GetCurrentMonitorEdid()
        {
            _logs.DebugMsg($"[DisplayMangerPlugin] {nameof(GetCurrentMonitorEdid)} start");
            List<byte[]> ret_Edie_Byt = new List<byte[]>();
            try
            {
                // Open the Display Reg-Key
                RegistryKey actiyDisplayRegistry = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\monitor\Enum");
                actiyDisplayRegistry.GetSubKeyNames();
                int count = int.Parse(actiyDisplayRegistry.GetValue("Count").ToString());
                List<string> displayPath = new List<string>();
                for (int i = 0; i < count; i++)
                {
                    displayPath.Add($"SYSTEM\\CurrentControlSet\\Enum\\{actiyDisplayRegistry.GetValue(i.ToString()).ToString()}\\Device Parameters");
                }
                for (int i = 0; i < displayPath.Count; i++)
                {
                    RegistryKey a = Registry.LocalMachine.OpenSubKey(displayPath[i]);
                    byte[] edidObj = (byte[])a.GetValue("EDID");
                    ret_Edie_Byt.Add(edidObj);
                }
            }
            catch (Exception ex)
            {
                _logs.DebugMsg($"[DisplayMangerPlugin] {nameof(GetCurrentMonitorEdid)} error:{ex.ToString()}");
            }
            _logs.DebugMsg($"[DisplayMangerPlugin] {nameof(GetCurrentMonitorEdid)} end");
            return ret_Edie_Byt;
        }

        private int DaysBetweenWeekStartAndToday(int year, int weekOfYear)
        {
            // 獲取當前日期
            DateTime today = DateTime.Today;
            // 創建 CultureInfo 物件，用於計算週數
            CultureInfo ci = CultureInfo.CurrentCulture;
            Calendar calendar = ci.Calendar;
            // 獲取該年的第一個日期
            DateTime jan1 = new DateTime(year, 1, 1);
            // 計算該年第一週的第一天
            DateTime jan1WeekStart = calendar.AddWeeks(jan1, 1 - (int)calendar.GetWeekOfYear(jan1, ci.DateTimeFormat.CalendarWeekRule, ci.DateTimeFormat.FirstDayOfWeek));
            // 計算指定週的第一天
            DateTime weekStart = jan1WeekStart.AddDays((weekOfYear - 1) * 7);
            // 計算從該週第一天到今天的天數
            int daysBetween = (int)(today - weekStart).TotalDays;
            return daysBetween;
        }

        private string GetActiveHour(MonitorInfo monitorInfo)
        {
            string ret = "N/A";
            try
            {
                ObjGetVCP rc = GetVCPCapability(monitorInfo, 0xC0).Result;
                if (rc.result)
                {
                    ret = rc.value.ToString();
                }
            }
            catch
            {
            }
            return ret;
        }

        private string GetPowerStatus(MonitorInfo monitorInfo)
        {
            string ret = "N/A";
            try
            {
                ObjGetVCP rc = GetVCPCapability(monitorInfo, 0xD6).Result;
                if (rc.result)
                {
                    Debug.WriteLine((uint)rc.value);
                    uint ret_uint = (uint)rc.value;
                    switch (ret_uint)
                    {
                        case 0x01:
                            ret = "On";
                            break;

                        case 0x04:
                            ret = "Saving";
                            break;

                        case 0x05:
                            ret = "Off";
                            break;
                    }
                }
            }
            catch
            {
            }
            return ret;
        }

        private string GetControllerID(MonitorInfo monitorInfo)
        {
            string ret = "N/A";
            try
            {
                ObjGetVCP rc = GetVCPCapability(monitorInfo, 0xC8).Result;
                if (rc.result)
                {
                    uint controllerId = ((uint)rc.value & 0xff);
                    if (VcpCodeList.VCPC8.ContainsKey(controllerId))
                    {
                        ret = VcpCodeList.VCPC8[controllerId];
                    }
                }
            }
            catch
            {
            }
            return ret;
        }

        #endregion

        #region Gaming

        private bool GamingChangeEventByPass = false;

        public Task<GamingDisplayPropertiesInfo> GetGamingProperties_SupportedList(MonitorInfo monitorInfo)
        {
            _logs.DebugMsg($"[DisplayMangerPlugin] {nameof(GetGamingProperties_SupportedList)} start");
            GamingDisplayPropertiesInfo gamingDisplayPropertiesInfo = new GamingDisplayPropertiesInfo();
            if (_displayDataManger != null)
            {
                if (_displayDataManger.GetMonitorGamingDisplayPropertiesInfo(monitorInfo, out gamingDisplayPropertiesInfo))
                {
                    return Task.FromResult(gamingDisplayPropertiesInfo);
                }
            }
            gamingDisplayPropertiesInfo.DisplayName = monitorInfo.DisplayName;
            gamingDisplayPropertiesInfo.SupportedProperties = _DisplayPropertiesPlugin.GetDisplaySupportedProperties(monitorInfo).Result.SupportedProperties;

            string[] ss = monitorInfo.CapabilityString.Split("F4(");
            if (ss.Length == 2)
            {
                ss = ss[1].Split(")");
                _logs.DebugMsg($"[DisplayMangerPlugin] {nameof(GetGamingProperties_SupportedList)} ss : {ss}");
                ss = ss[0].Split(" ");
                foreach (string temps in ss)
                {
                    if (!string.IsNullOrEmpty(temps))
                    {
                        string a = "0x" + temps;
                        uint u = Convert.ToUInt32(a, 16);
                        switch ((uint)(u & 0xf0))
                        {
                            case (uint)Gaming_Supported.GameEnhancementMode:
                                gamingDisplayPropertiesInfo.IsSupported_GameEnhancementMode = true;
                                gamingDisplayPropertiesInfo.Supported_GameEnhancementMode.Add((Gaming_GameEnhancementMode)(u & 0x0f));
                                break;

                            case (uint)Gaming_Supported.ResponseTime:
                                gamingDisplayPropertiesInfo.IsSupported_ResponseTime = true;
                                gamingDisplayPropertiesInfo.Supported_ResponseTime.Add((Gaming_ResponseTime)(u & 0x0f));
                                break;

                            case (uint)Gaming_Supported.DarkStabilizer:
                                gamingDisplayPropertiesInfo.IsSupported_DarkStabilizer = true;
                                gamingDisplayPropertiesInfo.Supported_DarkStabilizer.Add((Gaming_DarkStabilizer)(u & 0x0f));
                                break;

                            case (uint)Gaming_Supported.HDRType:
                                gamingDisplayPropertiesInfo.IsSupported_HDRType = true;
                                gamingDisplayPropertiesInfo.Supported_HDRType.Add((Gaming_HDRType)(u & 0x0f));
                                break;
                        }
                    }
                }
            }
            //0826 Bruce Hard code, Let AW2725QF supported dual resolution.
            string tempCapabilityString = monitorInfo.CapabilityString;
            if (monitorInfo.modelName.ToUpper().Contains("AW2725QF"))
            {
                tempCapabilityString = "EA(F810 F811 )";
            }
            ss = tempCapabilityString.Split("EA(");
            if (ss.Length == 2)
            {
                ss = ss[1].Split(")");
                ss = ss[0].Split(" ");
                foreach (string temps in ss)
                {
                    if (!string.IsNullOrEmpty(temps))
                    {
                        uint u = Convert.ToUInt32(temps, 16);
                        if (Enum.IsDefined(typeof(Gaming_DualResolutionType), u))
                        {
                            gamingDisplayPropertiesInfo.IsSupported_DualResolutionType = true;
                            gamingDisplayPropertiesInfo.Supported_DualResolutionType.Add((Gaming_DualResolutionType)u);
                        }
                    }
                }
            }
            if (monitorInfo.modelName.Contains("G"))
            {
                ss = monitorInfo.CapabilityString.Split("EC(");
                if (ss.Length == 2)
                {
                    ss = ss[1].Split(")");
                    ss = ss[0].Split(" ");
                    foreach (string temps in ss)
                    {
                        if (!string.IsNullOrEmpty(temps))
                        {
                            uint u = Convert.ToUInt32(temps, 16);
                            if (Enum.IsDefined(typeof(Gaming_VisionEngineType), u))
                            {
                                gamingDisplayPropertiesInfo.IsSupported_VisionEngineType = true;
                                gamingDisplayPropertiesInfo.Supported_VisionEngineType.Add((Gaming_VisionEngineType)u);
                            }
                        }
                    }
                    gamingDisplayPropertiesInfo.IsEnable_VisionEngineType = new bool[gamingDisplayPropertiesInfo.Supported_VisionEngineType.Count];
                }
            }
            if (gamingDisplayPropertiesInfo.IsSupported_GameEnhancementMode)
            {
                gamingDisplayPropertiesInfo.Current_GameEnhancementMode = GetCurrentGame_EnhancementMode(monitorInfo).Result;
            }
            if (gamingDisplayPropertiesInfo.IsSupported_ResponseTime)
            {
                gamingDisplayPropertiesInfo.Current_ResponseTime = GetCurrentGaming_ResponseTime(monitorInfo).Result;
            }
            if (gamingDisplayPropertiesInfo.IsSupported_DarkStabilizer)
            {
                gamingDisplayPropertiesInfo.Current_DarkStabilizer = GetCurrentGaming_DarkStabilizer(monitorInfo).Result;
            }
            if (gamingDisplayPropertiesInfo.IsSupported_HDRType)
            {
                gamingDisplayPropertiesInfo.Current_HDRType = GetCurrentGaming_HDRType(monitorInfo).Result;
            }
            if (gamingDisplayPropertiesInfo.IsSupported_DualResolutionType)
            {
                gamingDisplayPropertiesInfo.Current_DualResolutionType = GetCurrentGaming_DualResolutionType(monitorInfo).Result;
            }
            if (gamingDisplayPropertiesInfo.IsSupported_VisionEngineType)
            {
                gamingDisplayPropertiesInfo.IsEnable_VisionEngineType = GetCurrentGaming_VisionEngineEnableType(monitorInfo, gamingDisplayPropertiesInfo).Result;
            }
            if (_displayDataManger != null)
            {
                _displayDataManger.SetMonitorGamingDisplayPropertiesInfo(monitorInfo, gamingDisplayPropertiesInfo);
            }
            _logs.DebugMsg($"[DisplayMangerPlugin] {nameof(GetGamingProperties_SupportedList)} done");
            return Task.FromResult(gamingDisplayPropertiesInfo);
        }

        public Task<Gaming_GameEnhancementMode> GetCurrentGame_EnhancementMode(MonitorInfo monitorInfo, bool reGet = false)
        {
            _logs.DebugMsg($"[DisplayMangerPlugin] {nameof(GetCurrentGame_EnhancementMode)} start");
            Gaming_GameEnhancementMode GameEnhancementMode = Gaming_GameEnhancementMode.Disable;
            if (!reGet && _displayDataManger != null)
            {
                if (_displayDataManger.GetMonitorGamingDisplayPropertiesInfo(monitorInfo, out GamingDisplayPropertiesInfo gamingDisplayPropertiesInfo) &&
                    gamingDisplayPropertiesInfo.Current_GameEnhancementMode != null)
                {
                    GameEnhancementMode = (Gaming_GameEnhancementMode)gamingDisplayPropertiesInfo.Current_GameEnhancementMode;
                    return Task.FromResult(GameEnhancementMode);
                }
            }
            GamingChangeEventByPass = true;
            try
            {
                _logs.DebugMsg($"[DisplayMangerPlugin] {nameof(GetCurrentGame_EnhancementMode)} GetVCPCapability go");
                ObjGetVCP ObjGetVCP = GetVCPCapability(monitorInfo, nameof(Gaming_GameEnhancementMode)).Result;
                _logs.DebugMsg($"[DisplayMangerPlugin] {nameof(GetCurrentGame_EnhancementMode)} GetVCPCapability ObjGetVCP.result : {ObjGetVCP.result}");
                if (ObjGetVCP.result == true)
                {
                    _logs.DebugMsg($"[DisplayMangerPlugin] {nameof(GetCurrentGame_EnhancementMode)} GetVCPCapability ObjGetVCP.result : {(uint)ObjGetVCP.value}");
                    GameEnhancementMode = (Gaming_GameEnhancementMode)(uint)ObjGetVCP.value;
                }
            }
            catch (Exception ex)
            {
                _logs.DebugMsg($"[DisplayMangerPlugin] {nameof(GetCurrentGame_EnhancementMode)} Error : {ex.ToString()}");
            }
            GamingChangeEventByPass = false;
            if (_displayDataManger != null)
            {
                if (_displayDataManger.GetMonitorGamingDisplayPropertiesInfo(monitorInfo, out GamingDisplayPropertiesInfo gamingDisplayPropertiesInfo))
                {
                    gamingDisplayPropertiesInfo.Current_GameEnhancementMode = GameEnhancementMode;
                }
            }
            _logs.DebugMsg($"[DisplayMangerPlugin] {nameof(GetCurrentGame_EnhancementMode)} done Result : {GameEnhancementMode}");
            return Task.FromResult(GameEnhancementMode);
        }

        public Task<Gaming_ResponseTime> GetCurrentGaming_ResponseTime(MonitorInfo monitorInfo, bool reGet = false)
        {
            _logs.DebugMsg($"[DisplayMangerPlugin] {nameof(GetCurrentGaming_ResponseTime)} start");
            Gaming_ResponseTime ResponseTime = Gaming_ResponseTime.Disable;
            if (!reGet && _displayDataManger != null)
            {
                if (_displayDataManger.GetMonitorGamingDisplayPropertiesInfo(monitorInfo, out GamingDisplayPropertiesInfo gamingDisplayPropertiesInfo) &&
                    gamingDisplayPropertiesInfo.Current_ResponseTime != null)
                {
                    ResponseTime = (Gaming_ResponseTime)gamingDisplayPropertiesInfo.Current_ResponseTime;
                    return Task.FromResult(ResponseTime);
                }
            }
            GamingChangeEventByPass = true;
            try
            {
                ObjGetVCP ObjGetVCP = GetVCPCapability(monitorInfo, nameof(Gaming_ResponseTime)).Result;
                _logs.DebugMsg($"[DisplayMangerPlugin] {nameof(GetCurrentGaming_ResponseTime)} GetVCPCapability ObjGetVCP.result : {ObjGetVCP.result}");
                if (ObjGetVCP.result == true)
                {
                    _logs.DebugMsg($"[DisplayMangerPlugin] {nameof(GetCurrentGaming_ResponseTime)} GetVCPCapability ObjGetVCP.result : {(uint)ObjGetVCP.value}");
                    ResponseTime = (Gaming_ResponseTime)(uint)ObjGetVCP.value;
                }
            }
            catch (Exception ex)
            {
                _logs.DebugMsg($"[DisplayMangerPlugin] {nameof(GetCurrentGaming_ResponseTime)} Error : {ex.ToString()}");
            }
            if (_displayDataManger != null)
            {
                if (_displayDataManger.GetMonitorGamingDisplayPropertiesInfo(monitorInfo, out GamingDisplayPropertiesInfo gamingDisplayPropertiesInfo))
                {
                    gamingDisplayPropertiesInfo.Current_ResponseTime = ResponseTime;
                }
            }
            _logs.DebugMsg($"[DisplayMangerPlugin] {nameof(GetCurrentGaming_ResponseTime)} done Result : {ResponseTime}");
            GamingChangeEventByPass = false;
            return Task.FromResult(ResponseTime);
        }

        public Task<Gaming_DarkStabilizer> GetCurrentGaming_DarkStabilizer(MonitorInfo monitorInfo, bool reGet = false)
        {
            _logs.DebugMsg($"[DisplayMangerPlugin] {nameof(GetCurrentGaming_DarkStabilizer)} start");
            Gaming_DarkStabilizer DarkStabilizer = Gaming_DarkStabilizer.Disable;
            if (!reGet && _displayDataManger != null)
            {
                if (_displayDataManger.GetMonitorGamingDisplayPropertiesInfo(monitorInfo, out GamingDisplayPropertiesInfo gamingDisplayPropertiesInfo) &&
                    gamingDisplayPropertiesInfo.Current_DarkStabilizer != null)
                {
                    DarkStabilizer = (Gaming_DarkStabilizer)gamingDisplayPropertiesInfo.Current_DarkStabilizer;
                    return Task.FromResult(DarkStabilizer);
                }
            }
            GamingChangeEventByPass = true;
            try
            {
                ObjGetVCP ObjGetVCP = GetVCPCapability(monitorInfo, nameof(Gaming_DarkStabilizer)).Result;
                _logs.DebugMsg($"[DisplayMangerPlugin] {nameof(GetCurrentGaming_DarkStabilizer)} GetVCPCapability ObjGetVCP.result : {ObjGetVCP.result}");
                if (ObjGetVCP.result == true)
                {
                    _logs.DebugMsg($"[DisplayMangerPlugin] {nameof(GetCurrentGaming_DarkStabilizer)} GetVCPCapability ObjGetVCP.result : {(uint)ObjGetVCP.value}");
                    DarkStabilizer = (Gaming_DarkStabilizer)(uint)ObjGetVCP.value;
                }
            }
            catch (Exception ex)
            {
                _logs.DebugMsg($"[DisplayMangerPlugin] {nameof(GetCurrentGaming_DarkStabilizer)} Error : {ex.ToString()}");
            }
            if (_displayDataManger != null)
            {
                if (_displayDataManger.GetMonitorGamingDisplayPropertiesInfo(monitorInfo, out GamingDisplayPropertiesInfo gamingDisplayPropertiesInfo))
                {
                    gamingDisplayPropertiesInfo.Current_DarkStabilizer = DarkStabilizer;
                }
            }
            _logs.DebugMsg($"[DisplayMangerPlugin] {nameof(GetCurrentGaming_DarkStabilizer)} done Result : {DarkStabilizer}");
            GamingChangeEventByPass = false;
            return Task.FromResult(DarkStabilizer);
        }

        public Task<Gaming_HDRType> GetCurrentGaming_HDRType(MonitorInfo monitorInfo, bool reGet = false)
        {
            _logs.DebugMsg($"[DisplayMangerPlugin] {nameof(GetCurrentGaming_HDRType)} start");
            Gaming_HDRType HDRType = Gaming_HDRType.Disable;
            if (!reGet && _displayDataManger != null)
            {
                if (_displayDataManger.GetMonitorGamingDisplayPropertiesInfo(monitorInfo, out GamingDisplayPropertiesInfo gamingDisplayPropertiesInfo) &&
                    gamingDisplayPropertiesInfo.Current_HDRType != null)
                {
                    HDRType = (Gaming_HDRType)gamingDisplayPropertiesInfo.Current_HDRType;
                    return Task.FromResult(HDRType);
                }
            }
            GamingChangeEventByPass = true;
            try
            {
                ObjGetVCP ObjGetVCP = GetVCPCapability(monitorInfo, nameof(Gaming_HDRType)).Result;
                _logs.DebugMsg($"[DisplayMangerPlugin] {nameof(GetCurrentGaming_HDRType)} GetVCPCapability ObjGetVCP.result : {ObjGetVCP.result}");
                if (ObjGetVCP.result == true)
                {
                    _logs.DebugMsg($"[DisplayMangerPlugin] {nameof(GetCurrentGaming_HDRType)} GetVCPCapability ObjGetVCP.result : {(uint)ObjGetVCP.value}");
                    HDRType = (Gaming_HDRType)(uint)ObjGetVCP.value;
                }
            }
            catch (Exception ex)
            {
                _logs.DebugMsg($"[DisplayMangerPlugin] {nameof(GetCurrentGaming_HDRType)} Error : {ex.ToString()}");
            }
            _logs.DebugMsg($"[DisplayMangerPlugin] {nameof(GetCurrentGaming_HDRType)} Result : {HDRType}");
            GamingChangeEventByPass = false;
            if (HDRType != Gaming_HDRType.Disable)
            {
                UInt16 PipMode_Off = 0;
                ObjGetVCP ret = GetPxpMode(monitorInfo).Result;
                _logs.DebugMsg($"[DisplayMangerPlugin] {nameof(GetCurrentGaming_HDRType)} GetPxpMode ret.result: {ret.result}");
                if (ret.result)
                {
                    UInt16 _curPxpMode = Convert.ToUInt16(ret.value);
                    _logs.DebugMsg($"[DisplayMangerPlugin] {nameof(GetCurrentGaming_HDRType)} GetPxpMode _curPxpMode : {_curPxpMode}");
                    if (_curPxpMode != PipMode_Off)
                    {
                        HDRType = Gaming_HDRType.Disable;
                    }
                }
            }
            if (_displayDataManger != null)
            {
                if (_displayDataManger.GetMonitorGamingDisplayPropertiesInfo(monitorInfo, out GamingDisplayPropertiesInfo gamingDisplayPropertiesInfo))
                {
                    gamingDisplayPropertiesInfo.Current_HDRType = HDRType;
                }
            }
            _logs.DebugMsg($"[DisplayMangerPlugin] {nameof(GetCurrentGaming_HDRType)} done");
            return Task.FromResult(HDRType);
        }

        public Task<Gaming_DualResolutionType> GetCurrentGaming_DualResolutionType(MonitorInfo monitorInfo, bool reGet = false)
        {
            _logs.DebugMsg($"[DisplayMangerPlugin] {nameof(GetCurrentGaming_DualResolutionType)} start");
            Gaming_DualResolutionType DualResolutionType = Gaming_DualResolutionType.Unknow;
            if (!reGet && _displayDataManger != null)
            {
                if (_displayDataManger.GetMonitorGamingDisplayPropertiesInfo(monitorInfo, out GamingDisplayPropertiesInfo gamingDisplayPropertiesInfo) &&
                    gamingDisplayPropertiesInfo.Current_DualResolutionType != null)
                {
                    DualResolutionType = (Gaming_DualResolutionType)gamingDisplayPropertiesInfo.Current_DualResolutionType;
                    return Task.FromResult(DualResolutionType);
                }
            }
            try
            {
                ObjGetVCP ObjGetVCP = GetVCPCapability(monitorInfo, "USB-C Prioritization").Result;
                _logs.DebugMsg($"[DisplayMangerPlugin] {nameof(GetCurrentGaming_DualResolutionType)} GetVCPCapability ObjGetVCP.result : {ObjGetVCP.result}");
                if (ObjGetVCP.result == true)
                {
                    _logs.DebugMsg($"[DisplayMangerPlugin] {nameof(GetCurrentGaming_DualResolutionType)} GetVCPCapability ObjGetVCP.result : {ObjGetVCP.value}");
                    DualResolutionType = ObjGetVCP.value.ToString() == "4K" ? Gaming_DualResolutionType._4K : Gaming_DualResolutionType._FHD; ;
                }
            }
            catch (Exception ex)
            {
                _logs.DebugMsg($"[DisplayMangerPlugin] {nameof(GetCurrentGaming_DualResolutionType)} Error : {ex.ToString()}");
            }
            if (_displayDataManger != null)
            {
                if (_displayDataManger.GetMonitorGamingDisplayPropertiesInfo(monitorInfo, out GamingDisplayPropertiesInfo gamingDisplayPropertiesInfo))
                {
                    gamingDisplayPropertiesInfo.Current_DualResolutionType = DualResolutionType;
                }
            }
            _logs.DebugMsg($"[DisplayMangerPlugin] {nameof(GetCurrentGaming_DualResolutionType)} done Result : {DualResolutionType}");
            return Task.FromResult(DualResolutionType);
        }

        public Task<Gaming_VisionEngineType> GetCurrentGaming_VisionEngineType(MonitorInfo monitorInfo, bool reGet = false)
        {
            _logs.DebugMsg($"[DisplayMangerPlugin] {nameof(GetCurrentGaming_VisionEngineType)} start");
            Gaming_VisionEngineType current_VisionEngineType = Gaming_VisionEngineType.off;
            if (!reGet && _displayDataManger != null)
            {
                if (_displayDataManger.GetMonitorGamingDisplayPropertiesInfo(monitorInfo, out GamingDisplayPropertiesInfo gamingDisplayPropertiesInfo))
                {
                    current_VisionEngineType = gamingDisplayPropertiesInfo.Current_VisionEngineType;
                    return Task.FromResult(current_VisionEngineType);
                }
            }
            try
            {
                if (monitorInfo.modelName.Contains("G"))
                {
                    ObjGetVCP ObjGetVCP = GetVCPCapability(monitorInfo, 0xEC).Result;
                    _logs.DebugMsg($"[DisplayMangerPlugin] {nameof(GetCurrentGaming_VisionEngineType)} GetVCPCapability ObjGetVCP.result : {ObjGetVCP.result}");
                    if (ObjGetVCP.result == true)
                    {
                        _logs.DebugMsg($"[DisplayMangerPlugin] {nameof(GetCurrentGaming_VisionEngineType)} GetVCPCapability ObjGetVCP.result : {(uint)ObjGetVCP.value}");
                        current_VisionEngineType = (Gaming_VisionEngineType)((uint)ObjGetVCP.value & 0xf);
                    }
                }
            }
            catch (Exception ex)
            {
                _logs.DebugMsg($"[DisplayMangerPlugin] {nameof(GetCurrentGaming_VisionEngineType)} Error : {ex.ToString()}");
            }
            if (_displayDataManger != null)
            {
                if (_displayDataManger.GetMonitorGamingDisplayPropertiesInfo(monitorInfo, out GamingDisplayPropertiesInfo gamingDisplayPropertiesInfo))
                {
                    gamingDisplayPropertiesInfo.Current_VisionEngineType = current_VisionEngineType;
                }
            }
            _logs.DebugMsg($"[DisplayMangerPlugin] {nameof(GetCurrentGaming_VisionEngineType)} done Result : {current_VisionEngineType}");
            return Task.FromResult(current_VisionEngineType);
        }

        public Task<bool[]> GetCurrentGaming_VisionEngineEnableType(MonitorInfo monitorInfo, GamingDisplayPropertiesInfo gamingDisplayPropertiesInfo, bool reGet = false)
        {
            _logs.DebugMsg($"[DisplayMangerPlugin] {nameof(GetCurrentGaming_VisionEngineEnableType)} start");
            bool[] IsEnable_VisionEngineType = new bool[gamingDisplayPropertiesInfo.IsEnable_VisionEngineType.Length];
            if (!reGet && _displayDataManger != null)
            {
                if (_displayDataManger.GetMonitorGamingDisplayPropertiesInfo(monitorInfo, out GamingDisplayPropertiesInfo tempGamingDisplayPropertiesInfo))
                {
                    IsEnable_VisionEngineType = tempGamingDisplayPropertiesInfo.IsEnable_VisionEngineType;
                    return Task.FromResult(IsEnable_VisionEngineType);
                }
            }
            try
            {
                if (monitorInfo.modelName.Contains("G"))
                {
                    ObjGetVCP ObjGetVCP = GetVCPCapability(monitorInfo, 0xEC).Result;
                    _logs.DebugMsg($"[DisplayMangerPlugin] {nameof(GetCurrentGaming_VisionEngineEnableType)} GetVCPCapability ObjGetVCP.result : {ObjGetVCP.result}");
                    if (ObjGetVCP.result == true)
                    {
                        _logs.DebugMsg($"[DisplayMangerPlugin] {nameof(GetCurrentGaming_VisionEngineEnableType)} GetVCPCapability ObjGetVCP.result : {(uint)ObjGetVCP.value}");
                        // 右移8位，將後半段不要的曲調
                        uint ea_Ret = ((uint)ObjGetVCP.value >> 8);
                        //轉成二進制
                        string binaryString = Convert.ToString(ea_Ret, 2).PadLeft(8, '0');
                        // 反向字串，使得順向
                        string reversedBinaryString = Algorithm.ReverseString(binaryString);
                        _logs.DebugMsg($"[DisplayMangerPlugin] {nameof(GetCurrentGaming_VisionEngineEnableType)} GetVCPCapability reversedBinaryString : {reversedBinaryString}");
                        Debug.WriteLine(reversedBinaryString);
                        for (int i = 0; i < gamingDisplayPropertiesInfo.IsEnable_VisionEngineType.Length; i++)
                        {
                            //字串中為1的代表啟用該引擎
                            if (reversedBinaryString[i] == ('1'))
                            {
                                IsEnable_VisionEngineType[i] = true;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logs.DebugMsg($"[DisplayMangerPlugin] {nameof(GetCurrentGaming_VisionEngineEnableType)} Error : {ex.ToString()}");
            }
            if (_displayDataManger != null)
            {
                if (_displayDataManger.GetMonitorGamingDisplayPropertiesInfo(monitorInfo, out GamingDisplayPropertiesInfo tempGamingDisplayPropertiesInfo))
                {
                    gamingDisplayPropertiesInfo.IsEnable_VisionEngineType = IsEnable_VisionEngineType;
                }
            }
            _logs.DebugMsg($"[DisplayMangerPlugin] {nameof(GetCurrentGaming_VisionEngineEnableType)} done Result :  {IsEnable_VisionEngineType.Length}");
            return Task.FromResult(IsEnable_VisionEngineType);
        }

        public Task<bool> SetGameEnhancementMode(MonitorInfo monitorInfo, Gaming_GameEnhancementMode GameEnhancementMode)
        {
            _logs.DebugMsg($"[DisplayMangerPlugin] {nameof(SetGameEnhancementMode)} start");
            GamingChangeEventByPass = true;
            bool ret = false;
            try
            {
                uint title = (uint)Gaming_Supported.GameEnhancementMode;
                uint param = (uint)GameEnhancementMode;
                _logs.DebugMsg($"[DisplayMangerPlugin] {nameof(SetGameEnhancementMode)} title : {title}, param : {param}");
                ret = SetVCPCapability(monitorInfo, VcpCodeList.VCPctr["Gaming"], title + param).Result;
                if (ret)
                {
                    if (_displayDataManger != null)
                    {
                        if (_displayDataManger.GetMonitorGamingDisplayPropertiesInfo(monitorInfo, out GamingDisplayPropertiesInfo gamingDisplayPropertiesInfo))
                        {
                            gamingDisplayPropertiesInfo.Current_GameEnhancementMode = GameEnhancementMode;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logs.DebugMsg($"[DisplayMangerPlugin] {nameof(SetGameEnhancementMode)} Error : {ex.ToString()}");
            }
            GamingChangeEventByPass = false;
            _logs.DebugMsg($"[DisplayMangerPlugin] {nameof(SetGameEnhancementMode)} done Result : ret");
            return Task.FromResult(ret);
        }

        public Task<bool> SetGaming_ResponseTime(MonitorInfo monitorInfo, Gaming_ResponseTime ResponseTime)
        {
            _logs.DebugMsg($"[DisplayMangerPlugin] {nameof(SetGaming_ResponseTime)} start");
            GamingChangeEventByPass = true;
            bool ret = false;
            try
            {
                uint title = (uint)Gaming_Supported.ResponseTime;
                uint param = (uint)ResponseTime;
                _logs.DebugMsg($"[DisplayMangerPlugin] {nameof(SetGaming_ResponseTime)} title : {title}, param : {param}");
                ret = SetVCPCapability(monitorInfo, VcpCodeList.VCPctr["Gaming"], title + param).Result;
                if (ret)
                {
                    if (_displayDataManger != null)
                    {
                        if (_displayDataManger.GetMonitorGamingDisplayPropertiesInfo(monitorInfo, out GamingDisplayPropertiesInfo gamingDisplayPropertiesInfo))
                        {
                            gamingDisplayPropertiesInfo.Current_ResponseTime = ResponseTime;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logs.DebugMsg($"[DisplayMangerPlugin] {nameof(SetGaming_ResponseTime)} Error : {ex.ToString()}");
            }
            _logs.DebugMsg($"[DisplayMangerPlugin] {nameof(SetGaming_ResponseTime)} done Result : {ret}");
            GamingChangeEventByPass = false;
            return Task.FromResult(ret);
        }

        public Task<bool> SetGaming_DarkStabilizer(MonitorInfo monitorInfo, Gaming_DarkStabilizer DarkStabilizer)
        {
            _logs.DebugMsg($"[DisplayMangerPlugin] {nameof(SetGaming_DarkStabilizer)} start");
            GamingChangeEventByPass = true;
            bool ret = false;
            try
            {
                uint title = (uint)Gaming_Supported.DarkStabilizer;
                uint param = (uint)DarkStabilizer;
                _logs.DebugMsg($"[DisplayMangerPlugin] " + nameof(SetGameEnhancementMode) + " title : {title}, param : {param}");
                ret = SetVCPCapability(monitorInfo, VcpCodeList.VCPctr["Gaming"], title + param).Result;
                if (ret)
                {
                    if (_displayDataManger != null)
                    {
                        if (_displayDataManger.GetMonitorGamingDisplayPropertiesInfo(monitorInfo, out GamingDisplayPropertiesInfo gamingDisplayPropertiesInfo))
                        {
                            gamingDisplayPropertiesInfo.Current_DarkStabilizer = DarkStabilizer;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logs.DebugMsg($"[DisplayMangerPlugin] {nameof(SetGaming_DarkStabilizer)} Error : {ex.ToString()}");
            }
            _logs.DebugMsg($"[DisplayMangerPlugin] {nameof(SetGaming_DarkStabilizer)} done Result : {ret}");
            GamingChangeEventByPass = false;
            return Task.FromResult(ret);
        }

        public Task<bool> SetGaming_HDRType(MonitorInfo monitorInfo, Gaming_HDRType HDRType)
        {
            _logs.DebugMsg($"[DisplayMangerPlugin] {nameof(SetGaming_HDRType)} start");
            GamingChangeEventByPass = true;
            bool ret = false;
            try
            {
                uint title = (uint)Gaming_Supported.HDRType;
                uint param = (uint)HDRType;
                _logs.DebugMsg($"[DisplayMangerPlugin] {nameof(SetGaming_HDRType)} title : {title}, param : {param}");
                ret = SetVCPCapability(monitorInfo, VcpCodeList.VCPctr["Gaming"], title + param).Result;
                if (ret)
                {
                    if (_displayDataManger != null)
                    {
                        if (_displayDataManger.GetMonitorGamingDisplayPropertiesInfo(monitorInfo, out GamingDisplayPropertiesInfo gamingDisplayPropertiesInfo))
                        {
                            gamingDisplayPropertiesInfo.Current_HDRType = HDRType;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logs.DebugMsg($"[DisplayMangerPlugin] {nameof(SetGaming_HDRType)} Error : {ex.ToString()}");
            }
            _logs.DebugMsg($"[DisplayMangerPlugin] {nameof(SetGaming_HDRType)} done Result : {ret}");
            GamingChangeEventByPass = false;
            return Task.FromResult(ret);
        }

        public Task<bool> SetGaming_DualResolutionType(MonitorInfo monitorInfo, Gaming_DualResolutionType DualResolutionType)
        {
            _logs.DebugMsg($"[DisplayMangerPlugin] {nameof(SetGaming_DualResolutionType)} start");
            bool ret = false;
            try
            {
                if (DualResolutionType != Gaming_DualResolutionType.Unknow)
                {
                    string setParam = "USB-C Prioritization";
                    string PrioritizationType = DualResolutionType == Gaming_DualResolutionType._4K ? "4K" : "FHD";
                    _logs.DebugMsg($"[DisplayMangerPlugin] {nameof(SetGaming_DualResolutionType)} value:" + PrioritizationType);
                    ret = SetVCPCapability(monitorInfo, setParam, PrioritizationType).Result;
                    if (ret)
                    {
                        if (_displayDataManger != null)
                        {
                            if (_displayDataManger.GetMonitorGamingDisplayPropertiesInfo(monitorInfo, out GamingDisplayPropertiesInfo gamingDisplayPropertiesInfo))
                            {
                                gamingDisplayPropertiesInfo.Current_DualResolutionType = DualResolutionType;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logs.DebugMsg($"[DisplayMangerPlugin] {nameof(SetGaming_DualResolutionType)} Error : {ex.ToString()}");
            }
            _logs.DebugMsg($"[DisplayMangerPlugin] {nameof(SetGaming_DualResolutionType)} done Result : {ret}");
            return Task.FromResult(ret);
        }

        public Task<bool> SetGaming_VisionEngineEnableType(MonitorInfo monitorInfo, bool[] VisionEngineEnableType)
        {
            _logs.DebugMsg($"[DisplayMangerPlugin] {nameof(SetGaming_VisionEngineEnableType)} start");
            bool ret = false;
            try
            {
                if (VisionEngineEnableType != null)
                {
                    string command = "";
                    for (int i = 0; i < VisionEngineEnableType.Length; i++)
                    {
                        //需啟用的引擎增加1字串
                        if (VisionEngineEnableType[i])
                        {
                            command += "1";
                        }
                        else
                        {
                            command += "0";
                        }
                    }
                    //1.將字串反向，因韌體是右到左，修改回韌體順序
                    //2.反向後先往右邊補0，補齊b8-b15共7位
                    //3.再往左邊補0，共補16位b0-17
                    command = Algorithm.ReverseString(command).PadLeft(8, '0').PadRight(16, '0');
                    //轉成16進制
                    command = Algorithm.BinaryToHex(command);
                    var hexStyle = System.Globalization.NumberStyles.HexNumber;
                    int number;
                    _logs.DebugMsg($"[DisplayMangerPlugin] {nameof(SetGaming_VisionEngineEnableType)} value : {command}");
                    if (int.TryParse(command, hexStyle, CultureInfo.CurrentCulture, out number))
                    {
                        ret = SetVCPCapability(monitorInfo, 0xEC, (uint)number).Result;
                        if (ret)
                        {
                            if (_displayDataManger != null)
                            {
                                if (_displayDataManger.GetMonitorGamingDisplayPropertiesInfo(monitorInfo, out GamingDisplayPropertiesInfo gamingDisplayPropertiesInfo))
                                {
                                    gamingDisplayPropertiesInfo.IsEnable_VisionEngineType = VisionEngineEnableType;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logs.DebugMsg($"[DisplayMangerPlugin] {nameof(SetGaming_VisionEngineEnableType)} Error : {ex.ToString()}");
            }
            _logs.DebugMsg($"[DisplayMangerPlugin] {nameof(SetGaming_VisionEngineEnableType)} done Result : {ret}");
            return Task.FromResult(ret);
        }

        public Task<bool> SwitchGaming_VisionEngineType(MonitorInfo monitorInfo, Gaming_VisionEngineType VisionEngineType)
        {
            _logs.DebugMsg($"[DisplayMangerPlugin] {nameof(SwitchGaming_VisionEngineType)} start");
            bool ret = false;
            try
            {
                uint param = (uint)VisionEngineType;
                _logs.DebugMsg($"[DisplayMangerPlugin] {nameof(SwitchGaming_VisionEngineType)} value : {param}");
                ret = SetVCPCapability(monitorInfo, 0xEC, param).Result;
                if (ret)
                {
                    if (_displayDataManger != null)
                    {
                        if (_displayDataManger.GetMonitorGamingDisplayPropertiesInfo(monitorInfo, out GamingDisplayPropertiesInfo gamingDisplayPropertiesInfo))
                        {
                            gamingDisplayPropertiesInfo.Current_VisionEngineType = VisionEngineType;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logs.DebugMsg($"[DisplayMangerPlugin] {nameof(SwitchGaming_VisionEngineType)} Error : {ex.ToString()}");
            }
            _logs.DebugMsg($"[DisplayMangerPlugin] {nameof(SwitchGaming_VisionEngineType)} done Result : {ret}");
            return Task.FromResult(ret);
        }

        private bool GetCurrentGamingParam(MonitorInfo monitorInfo, ref GamingDisplayPropertiesInfo gamingDisplayPropertiesInfo)
        {
            _logs.DebugMsg($"[DisplayMangerPlugin] {nameof(GetCurrentGamingParam)} start");
            GamingChangeEventByPass = true;
            bool ret = false;
            try
            {
                gamingDisplayPropertiesInfo.Current_GameEnhancementMode = GetCurrentGame_EnhancementMode(monitorInfo, true).Result;
                gamingDisplayPropertiesInfo.Current_ResponseTime = GetCurrentGaming_ResponseTime(monitorInfo, true).Result;
                gamingDisplayPropertiesInfo.Current_DarkStabilizer = GetCurrentGaming_DarkStabilizer(monitorInfo, true).Result;
                gamingDisplayPropertiesInfo.Current_HDRType = GetCurrentGaming_HDRType(monitorInfo, true).Result;
                gamingDisplayPropertiesInfo.Current_DualResolutionType = GetCurrentGaming_DualResolutionType(monitorInfo, true).Result;
                gamingDisplayPropertiesInfo.Current_VisionEngineType = GetCurrentGaming_VisionEngineType(monitorInfo, true).Result;
                gamingDisplayPropertiesInfo.IsEnable_VisionEngineType = GetCurrentGaming_VisionEngineEnableType(monitorInfo, gamingDisplayPropertiesInfo, true).Result;
                ret = true;
            }
            catch (Exception ex)
            {
                _logs.DebugMsg($"[DisplayMangerPlugin] {nameof(GetCurrentGamingParam)} Error : {ex.ToString()}");
            }
            GamingChangeEventByPass = false;
            _logs.DebugMsg($"[DisplayMangerPlugin] {nameof(GetCurrentGamingParam)} done:Result {ret}");
            return ret;
        }

        private void GamingChangeEventHandle(VCPchangedEventArgs vcpchangedEventArgs)
        {
            _logs.DebugMsg($"[DisplayMangerPlugin] {nameof(GamingChangeEventHandle)} start");
            if (!GamingChangeEventByPass && vcpchangedEventArgs.vcpcode == "F4")
            {
                _logs.DebugMsg($"[DisplayMangerPlugin] {nameof(GamingChangeEventHandle)} vcpchangedEventArgs.vcpcode is F4");
                GamingDisplayPropertiesInfo gamingDisplayPropertiesInfo = new GamingDisplayPropertiesInfo();
                if (GetCurrentGamingParam(vcpchangedEventArgs.monitor, ref gamingDisplayPropertiesInfo))
                {
                    _logs.DebugMsg($"[DisplayMangerPlugin] {nameof(GamingChangeEventHandle)} send GamingChangeEvent");
                    GamingChangeEvent?.AsyncFireAndForget(this, gamingDisplayPropertiesInfo, System.Threading.CancellationToken.None);
                }
            }
            _logs.DebugMsg($"[DisplayMangerPlugin] {nameof(GamingChangeEventHandle)} done");
        }

        private void SetMonitorAllUSB(VCPchangedEventArgs vcpchangedEventArgs)
        {
            string usbUpstream = string.Empty;
            int input_num = 0;
            ObjGetVCP objGetVCP = new ObjGetVCP();
            List<InputSource_USB> input_USBList = new List<InputSource_USB>();
            if (vcpchangedEventArgs.monitor != null)
            {
                usbUpstreamList = GetUSBUpstreamList(vcpchangedEventArgs.monitor).Result;
                if (usbUpstreamList != null && usbUpstreamList.Count > 0)
                {
                    if (_getVCPCapabilities == string.Empty)
                    {
                        _getVCPCapabilities = GetVCPCapabilities(vcpchangedEventArgs.monitor).Result;
                    }
                    if (!string.IsNullOrEmpty(_getVCPCapabilities))
                    {
                        JObject VCPjson = JObject.Parse(_getVCPCapabilities);

                        JObject capsDataMap = (JObject)VCPjson["CapsDataMap"];
                        JArray input = (JArray)capsDataMap["Input Select"];
                        string getUpstream = Convert.ToString(int.Parse(vcpchangedEventArgs.value), 2);
                        Trace.WriteLine(getUpstream);
                        string newstrUpstream = getUpstream;
                        if (getUpstream.Length < 16)
                        {
                            for (int i = 0; i < (16 - getUpstream.Length); i++)
                            {
                                newstrUpstream = "0" + newstrUpstream;
                            }
                        }

                        Trace.WriteLine(newstrUpstream);
                        _logs.DebugMsg("[DisplayMangerPlugin][SetMonitorAllUSB] newstrUpstream : " + newstrUpstream);

                        if (newstrUpstream.Length == 16)
                        {
                            foreach (var tmp in input)
                            {
                                string subUpstream = string.Empty;
                                if (vcpchangedEventArgs.monitor.CapabilityDic.ContainsKey("EE"))
                                {
                                    subUpstream = newstrUpstream.Substring(input_num * 2, 2);
                                }
                                else
                                {
                                    subUpstream = newstrUpstream.Substring(14 - (input_num * 2), 2);
                                }
                                if (!string.IsNullOrEmpty(subUpstream))
                                {
                                    List<USBPorts> ports = new List<USBPorts>();
                                    _displayDataManger.GetMonitorUSBList(vcpchangedEventArgs.monitor, out ports);
                                    if (ports != null && ports.Count != 0)
                                    {
                                        foreach (var tmp1 in ports)
                                        {
                                            if (subUpstream == tmp1.USBPort)
                                            {
                                                InputSource_USB inputSource_USB = new InputSource_USB();
                                                inputSource_USB.inputSource = tmp.ToString();
                                                Trace.WriteLine(inputSource_USB.inputSource);
                                                inputSource_USB.USB = tmp1.USBName;
                                                Trace.WriteLine(inputSource_USB.USB);
                                                input_USBList.Add(inputSource_USB);
                                                break;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        _logs.DebugMsg("[DisplayMangerPlugin][SetMonitorAllUSB] ports is null or Count = 0");
                                    }
                                }
                                input_num++;
                            }
                            if (_displayDataManger != null)
                            {
                                _displayDataManger.SetMonitorUSB(vcpchangedEventArgs.monitor, input_USBList);
                            }
                        }
                        else
                        {
                            _logs.DebugMsg("[DisplayMangerPlugin][SetMonitorAllUSB] newstrUpstream length is not 16");
                        }
                    }
                    else
                    {
                        _logs.DebugMsg("[DisplayMangerPlugin][SetMonitorAllUSB] _getVCPCapabilities is null or empty.");
                    }
                }
                else
                {
                    _logs.DebugMsg("[DisplayMangerPlugin][SetMonitorAllUSB] usbUpstreamList is null or count = 0.");
                }
            }
        }

        #endregion

        #region Display FWU Metadata

        public Task<DisplayUpdateHelper> GetDisplayFWUpdate(bool isSkipCA, ISettingsManagerDev settingsPlugin)
        {
            DisplayUpdateHelper displayUpdateHelper = new DisplayUpdateHelper();
            displayUpdateHelper = GetDisplayFWMetadata(isSkipCA, settingsPlugin);
            return Task.FromResult(displayUpdateHelper);
        }

        private DisplayUpdateHelper GetDisplayFWMetadata(bool isSkipCA, ISettingsManagerDev settingsPlugin)
        {
            _logs.DebugMsg($"[DisplayMangerPlugin] {nameof(GetDisplayFWMetadata)} start");
            string display_FWU_URL = Download.GetTestServerURL() + GlobalDefinitions.Display_FWU_URL_Folder;
            DisplayUpdateHelper ret = new DisplayUpdateHelper();
            if (!isSkipCA)
            {
                CertificateCheck certificateCheck = new CertificateCheck(_logs);
                if (!certificateCheck.CheckURLCACertificate(display_FWU_URL))
                {
                    _logs.DebugMsg($"[DisplayMangerPlugin] {nameof(GetDisplayFWMetadata)} check CA fail");
                    return ret;
                }
            }
            else
            {
                _logs.DebugMsg($"[DisplayMangerPlugin] {nameof(GetDisplayFWMetadata)} check CA is skip");
            }
            List<MonitorInfo> monitorInfos = GetMonitors().Result;
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    client.DefaultRequestHeaders.UserAgent.ParseAdd(GlobalDefinitions.HttpAgentNamePreset + SettingsAccess.QueryAppAccessInfo().ver);//DDPMW-2896
                    _logs.DebugMsg($"[DisplayMangerPlugin] {nameof(GetDisplayFWMetadata)} get jsonContent");
                    client.Timeout = TimeSpan.FromSeconds(60);
                    HttpResponseMessage response = client.GetAsync(display_FWU_URL + "version_sha256.json").Result;
                    response.EnsureSuccessStatusCode();
                    string jsonContent = response.Content.ReadAsStringAsync().Result;
                    List<string> InfoPkey = new List<string>();
                    if (settingsPlugin != null)
                    {
                        InfoPkey = settingsPlugin.GetInfos().Result;
                    }
                    if (InfoPkey == null || InfoPkey.Count == 0)
                    {
                        //if read info failed, load default key as well
                        InfoPkey = new List<string>(DDPM.SA.Obfuscation.InfoHash.Info_Hash);
                        //InfoPkey.Add(DDPM.SA.Obfuscation.InfoHash.Info_Hash);
                    }
                    string szInfo = string.Empty;
                    string jsonString = string.Empty;
                    _logs.DebugMsg($"{nameof(GetDisplayFWMetadata)} json content check start");
                    jsonString = DDPM.SA.Common.Settings.DDPMFileSecurity.VerifyDDPMMetadata(Log, jsonContent, InfoPkey, out szInfo);
                    if (!string.IsNullOrEmpty(szInfo) && settingsPlugin != null)
                    {
                        settingsPlugin.AddInfo(szInfo);//pass info to settings manager and judge if new to add
                    }
                    if (!string.IsNullOrEmpty(jsonString))
                    {
                        Dictionary<string, Display_Firmwares_item> data = JsonSerializer.Deserialize<Dictionary<string, Display_Firmwares_item>>(jsonString);
                        foreach (MonitorInfo monitorInfo in monitorInfos)
                        {
                            string model = data.Keys.ToList().Find(o => o.Equals(monitorInfo.modelName));
                            if (!string.IsNullOrEmpty(model) && data.ContainsKey(model))
                            {
                                Display_Firmwares_item firmwares_item = new Display_Firmwares_item()
                                {
                                    id = data[model].id,
                                    url = data[model].url,
                                    TheLastVersion = data[model].TheLastVersion,
                                    SHA256 = data[model].SHA256,
                                    SHA512 = data[model].SHA512,
                                    Thumbprint = data[model].Thumbprint,
                                    SupportedPlatform = data[model].SupportedPlatform,
                                    fileName = data[model].fileName,
                                    //date = data[model].date, //1/23 Remove by Bruce
                                };
                                if (firmwares_item != null)
                                {
                                    firmwares_item.id = model;
                                    _logs.DebugMsg($"[DisplayMangerPlugin] {nameof(GetDisplayFWMetadata)} model : {model}");
                                    if (firmwares_item.url.Contains("%2"))
                                    {
                                        firmwares_item.url = firmwares_item.url.Replace("%2", GlobalDefinitions.percent_two_url);// "https://downloads.dell.com");
                                    }
                                    else
                                    {
                                        firmwares_item.url = display_FWU_URL + firmwares_item.url;
                                    }
                                    firmwares_item.CurrentVersion = monitorInfo.FwVersion;
                                    //firmwares_item.TheLastVersion = firmwares_item.TheLastVersion;
                                    firmwares_item.ServiceTag = monitorInfo.edid.ServiceTag;
                                    firmwares_item.SupplierID = monitorInfo.SupplierID;
                                    firmwares_item.D_Ctrl = monitorInfo.D_Ctrl;
                                    if (firmwares_item.SupportedPlatform != null)
                                    {
                                        _logs.DebugMsg($"[DisplayMangerPlugin] firmwares_item.SupportedPlatform : {firmwares_item.SupportedPlatform}");
                                        string currentPlatform = GetSystemArchitecture();
                                        string[] supportedPlatform = firmwares_item.SupportedPlatform.Split(",");
                                        _logs.DebugMsg($"[DisplayMangerPlugin] currentPlatform : {currentPlatform}");
                                        if (!supportedPlatform.ToList().Contains(currentPlatform))
                                        {
                                            _logs.DebugMsg($"[DisplayMangerPlugin] {nameof(GetDisplayFWMetadata)} {firmwares_item.id} Platform no supported. currentPlatform:{currentPlatform} ");
                                            continue;
                                        }
                                    }
                                    if (!string.IsNullOrEmpty(data[model].UpdateTime))
                                    {
                                        firmwares_item.UpdateTime = data[model].UpdateTime;
                                        _logs.DebugMsg($"[DisplayMangerPlugin] firmwares_item.UpdateTime : {firmwares_item.UpdateTime}");
                                    }
                                    else
                                    {
                                        firmwares_item.UpdateTime = "";
                                        _logs.DebugMsg($"[DisplayMangerPlugin] data[{model}].UpdateTime is null");
                                    }
                                    _logs.DebugMsg($"[DisplayMangerPlugin] {nameof(GetDisplayFWMetadata)} firmwares_item.CurrentVersion is null or empty : {string.IsNullOrEmpty(firmwares_item.CurrentVersion)}");
                                    _logs.DebugMsg($"[DisplayMangerPlugin] {nameof(GetDisplayFWMetadata)} firmwares_item.TheLastVersion is null or empty : {string.IsNullOrEmpty(firmwares_item.TheLastVersion)}");
                                    if (!string.IsNullOrEmpty(firmwares_item.CurrentVersion) &&
                                        !string.IsNullOrEmpty(firmwares_item.TheLastVersion))
                                    {
                                        _logs.DebugMsg($"[DisplayMangerPlugin] {nameof(GetDisplayFWMetadata)} firmwares_item.CurrentVersion : {firmwares_item.CurrentVersion}");
                                        _logs.DebugMsg($"[DisplayMangerPlugin] {nameof(GetDisplayFWMetadata)} firmwares_item.TheLastVersion : {firmwares_item.TheLastVersion}");
                                        int newVersion = -1;
                                        int oldVersion = 9999;
                                        for (int j = firmwares_item.TheLastVersion.Length - 1; j >= 0; j--)
                                        {
                                            if (char.IsLetter(firmwares_item.TheLastVersion[j]))
                                            {
                                                int index = j + 1;
                                                int.TryParse(firmwares_item.TheLastVersion.Substring(index, firmwares_item.TheLastVersion.Length - index), out newVersion);
                                                break;
                                            }
                                        }
                                        for (int j = firmwares_item.CurrentVersion.Length - 1; j >= 0; j--)
                                        {
                                            if (char.IsLetter(firmwares_item.CurrentVersion[j]))
                                            {
                                                int index = j + 1;
                                                int.TryParse(firmwares_item.CurrentVersion.Substring(index, firmwares_item.CurrentVersion.Length - index), out oldVersion);
                                                break;
                                            }
                                        }
                                        _logs.DebugMsg($"[DisplayMangerPlugin] {nameof(GetDisplayFWMetadata)} newVersion : {newVersion}");
                                        _logs.DebugMsg($"[DisplayMangerPlugin] {nameof(GetDisplayFWMetadata)} oldVersion : {oldVersion}");
                                        if (newVersion > oldVersion)
                                        {
                                            ret.Firmwares.Add(firmwares_item);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        _logs.DebugMsg($"[DisplayMangerPlugin] {nameof(GetDisplayFWMetadata)} json content check fail");
                    }
                }
                catch (Exception ex)
                {
                    _logs.DebugMsg($"[DisplayMangerPlugin] {nameof(GetDisplayFWMetadata)} error {ex.Message}");
                }
            }
            _logs.DebugMsg($"[DisplayMangerPlugin] {nameof(GetDisplayFWMetadata)} done");
            return ret;
        }

        public string GetSystemArchitecture()
        {
            _logs.DebugMsg($"[DisplayMangerPlugin] {nameof(GetSystemArchitecture)} RuntimeInformation.ProcessArchitecture : {RuntimeInformation.OSArchitecture.ToString()}");
            if (RuntimeInformation.OSArchitecture == Architecture.X64)
            {
                return "Intel";//"Intel_x64";
            }
            else if (RuntimeInformation.OSArchitecture == Architecture.X86)
            {
                return "Intel";//"Intel_x86";
            }
            else if (RuntimeInformation.OSArchitecture == Architecture.Arm)
            {
                return "ARM";
            }
            else if (RuntimeInformation.OSArchitecture == Architecture.Arm64)
            {
                return "ARM";//"ARM_64";
            }
            else
            {
                return "Unknow";
            }
        }

        #endregion

        #region DisplayData

        public Task InitDisplayData(List<MonitorInfo> monitorInfos)
        {
            if (monitorInfos != null)
            {
                _displayDataManger.InitDisplayData(monitorInfos);
            }
            return Task.CompletedTask;
        }

        public Task SetVCPtoDisplayData(MonitorInfo monitorInfo, int vcpcode, int value)
        {
            if (monitorInfo != null)
            {
                if (vcpcode == 0xE9)
                {
                    _displayDataManger.SetMonitorE9(monitorInfo, (uint)value);
                }
            }
            return Task.CompletedTask;
        }

        private void SetMonitorVCPE9(VCPchangedEventArgs vCPchangedEventArgs)
        {
            if (vCPchangedEventArgs.monitor != null)
            {
                if (vCPchangedEventArgs.value != "1" && vCPchangedEventArgs.value != "2")
                {
                    if (_displayDataManger != null)
                    {
                        _displayDataManger.SetMonitorE9(vCPchangedEventArgs.monitor, (uint)int.Parse(vCPchangedEventArgs.value));
                    }
                }
                else
                {
                    _logs.DebugMsg("[DisplayMangerPlugin][SetMonitorE9] E9 is 1 or 2.");
                }
            }
            else
            {
                _logs.DebugMsg("[DisplayMangerPlugin][SetMonitorE9] monitor info is null.");
            }
        }

        private void SetMonitorColor(VCPchangedEventArgs vCPchangedEventArgs)
        {
            if (vCPchangedEventArgs.monitor != null)
            {
                ObjGetVCP result = new ObjGetVCP() { result = false, value = null };
                if (_displayDataManger != null)
                {
                    result = _VcpCorePlugin.GetVCPCapability(vCPchangedEventArgs.monitor, "colorpreset").Result;
                    if (result.result)
                    {
                        Trace.WriteLine("SetMonitorColor value : " + (string)result.value);
                        _displayDataManger.SetColor(vCPchangedEventArgs.monitor, GetHDRStatus(vCPchangedEventArgs.monitor).Result, (string)result.value);
                    }
                }
            }
            else
            {
                _logs.DebugMsg("[DisplayMangerPlugin][SetMonitorColor] monitor info is null.");
            }
        }

        #endregion
    }
}