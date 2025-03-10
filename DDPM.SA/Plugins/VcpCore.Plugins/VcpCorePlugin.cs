#region LicenceHeader

//
// Copyright © 2024, Dell Inc., All Rights Reserved.
// This material is confidential and a trade secret.  Permission to use this
// work for any purpose must be obtained in writing from Dell Inc.
//
// VcpCorePlugin.cs created on 28/03/2024T09:50 AM
//

#endregion

using DDPM.SA.Common.Settings;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.Common.Annotations;
using Dell.Client.Framework.Common.PluginConditions;
using Dell.Client.Framework.Interfaces;
using Dell.Client.Framework.Security;
using Dell.Client.Framework.Security.Interfaces;
using Microsoft;
using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using VcpCore.Common;
using VcpCore.Interfaces;
using static VcpCore.Common.dxva2;
using static VcpCore.Common.User32;
using IDs = VcpCore.Common.IDs;
using Timer = System.Timers.Timer;

namespace VcpCore.Plugins
{
    [Plugin(IDs.VCP_CORE_PLUGIN_ID, pluginName, PluginOrderGroupType.Core, Version = pluginVersion)]
    [Descriptor(Description = pluginDescription)]
    [Publisher(Name = publisherCompany, Website = publisherWebsite, Support = publisherSupport)]
    [PublishedUnelevatedInterface(new[] { typeof(IVcpCoreService) })]
    public class VcpCorePlugin : BaseAgentPlugin, IDisposableObservable, IVcpCoreService
    {
        #region Private Members

        private const string pluginName = "VcpCorePlugin";
        private const string pluginVersion = "1.0.0";
        private const string pluginDescription = "This plugin implements Vcp Core Plugin.";
        private const string publisherCompany = "Dell Inc.";
        private const string publisherWebsite = "https://www.dell.com";
        private const string publisherSupport = "This plugin implements Vcp Core Plugin.";

        private bool _IsAdministrator = ProcessSecurityHelperWrapper.IsCurrentProcessRunningElevated();
        private IAgent _agent;
        private const string PluginLogId = "VcpCore";
        private const bool _IsOnlyGetDellMontor = true;
        private static Logs _logs;

        private static List<MonitorInfo_complex> _AllInfoMonitors;
        private static List<(MonitorInfo_complex, MonitorInfo)> _AllInfoMonitors_Mix;
        private static Dictionary<string, Dictionary<string, string>> _ColorPresets;
        private static TaskLockQueue<ParameterType> _TaskQueue;
        private static BackgroundWorker _TaskQueueExecutor;
        private static ResultLockPool _TaskQueueResult;
        private static Timer _CacheTimer = new Timer(8000);
        private static Timer _StatusTimer = new Timer(10500);
        private static readonly object TaskQueueExecutorLock = new object();
        private static readonly object GetVCPLock = new object();
        private static readonly object SetVCPLock = new object();
        private static CancellationTokenSource _cancellationTokenSource;
        private static string _SupportClassification = string.Empty;
        private static Dictionary<string, List<modelinfos>> _SupportDictionary;
        private static readonly string targetFile = "LSTDDPM";
        private static VcpLockCache _CacheTable;
        private static HashSet<Guid> _CancelhashSet;
        private static ManualResetEvent _pauseEvent = new ManualResetEvent(true);
        private static SemaphoreSlim _LockerSemaphoreSlim = new SemaphoreSlim(1, 1);
        private int _CoWorkSignal = 0;
        private int _InitialThreadCounter = 0;
        private int _AddSignalfor0X52 = 0;
        private int _AddSignalforStatusCheck = 0;
        private bool _IsUserActive = true;

        private bool IsOutInitialize
        {
            get => (_InitialThreadCounter < 1);
        }

        #endregion

        #region Public Members

        public event EventHandler<VCPchangedEventArgs> VCPchanged;

        public event EventHandler<DisplaychangedEventArgs> Displaychanged;

        public event EventHandler<MonitorinfoUpdateEventArgs> MonitorinfoUpdated;

        public event EventHandler<DDCCIchangedEventArgs> DDCCIStatuschanged;

        #endregion

        #region Constructor

        public VcpCorePlugin(IAgent agent) : base(agent, PluginLogId)
        {
            _agent = agent;
            _IsAdministrator = ProcessSecurityHelperWrapper.IsCurrentProcessRunningElevated();
            _logs ??= new Logs(Log);
            _TaskQueueResult ??= new ResultLockPool();
            _AllInfoMonitors ??= new List<MonitorInfo_complex>();
            _AllInfoMonitors_Mix ??= new List<(MonitorInfo_complex, MonitorInfo)>();
            _CacheTable ??= new VcpLockCache(_logs);
            _CancelhashSet ??= new HashSet<Guid>();
            _ColorPresets ??= new Dictionary<string, Dictionary<string, string>>();
            _TaskQueue ??= new TaskLockQueue<ParameterType>();
            _TaskQueueExecutor ??= new BackgroundWorker();
            _TaskQueueExecutor.DoWork += TaskQueueExecutor_DoWork;
            _TaskQueueExecutor.RunWorkerCompleted += TaskQueueExecutor_RunWorkerCompleted;
            _TaskQueueExecutor.WorkerSupportsCancellation = true;
            _SupportDictionary ??= new Dictionary<string, List<modelinfos>>();
            _CacheTimer ??= new Timer(8000);
            _StatusTimer ??= new Timer(10500);
            _pauseEvent ??= new ManualResetEvent(true);
            _LockerSemaphoreSlim ??= new SemaphoreSlim(1, 1);
            _InitialThreadCounter = 0;
            _CoWorkSignal = 0;
            _AddSignalfor0X52 = 0;
            _AddSignalforStatusCheck = 0;
            _CacheTimer.Elapsed += OnCacheTimedRaise;
            _CacheTimer.AutoReset = true;
            _CacheTimer.Enabled = false;
            _StatusTimer.Elapsed += OnStatusTimedRaise;
            _StatusTimer.AutoReset = true;
            _StatusTimer.Enabled = false;
            _IsUserActive = true;

            _logs.DebugMsg("[VcpCorePlugin] Does VcpCorePlugin have Administrator: " + _IsAdministrator.ToString());

            Get_SupportListFile();
            InitialColorPresets();

            if (_cancellationTokenSource != null)
                Task.Run(() => InitializeMonitorsList(false, _cancellationTokenSource.Token)).ConfigureAwait(false);
            else
                Task.Run(() => InitializeMonitorsList(false, CancellationToken.None)).ConfigureAwait(false);
        }

        #endregion

        #region Overriding methods

        protected override void OnPluginStarting()
        {
            PluginCondition = new PluginStartedCondition();
            _agent.PluginManager.PluginsStarted += PluginManagerOnPluginsStarted;
        }

        protected override void OnPluginProcess()
        {
            if (PluginCondition != null)
                _logs.DebugMsg("[VcpCorePlugin] VcpCorePlugin on OnPluginProcess PluginCondition:" + PluginCondition.Message);
            else
                _logs.DebugMsg("[VcpCorePlugin] VcpCorePlugin on OnPluginProcess PluginCondition: null");
        }

        #endregion

        #region IVcpCoreService implementation

        public Task<Dictionary<EDID, Dictionary<object, object>>> GetVCPCacheTable()
        {
            _logs.DebugMsg("[VcpCorePlugin] VcpCorePlugin GetVCPCacheTable  ...");

            return Task.FromResult(_CacheTable.GetVCPCacheTable() ?? new Dictionary<EDID, Dictionary<object, object>>());
        }

        public Task Reset0x52TimerTick(int millisecond)
        {
            _logs.DebugMsg("[VcpCorePlugin] VcpCorePlugin received Reset0x52TimerTick: " + millisecond.ToString() + " requested ...");

            var Orig = _CacheTimer.Enabled;

            if (Orig) _CacheTimer.Stop();

            _CacheTimer.Interval = millisecond;
            _CacheTimer.AutoReset = true;

            if (Orig) _CacheTimer.Start();

            return Task.CompletedTask;
        }

        public Task SetIsUserActive(bool IsUserActive)
        {
            _logs.DebugMsg("[VcpCorePlugin] VcpCorePlugin received SetIsUserActive: " + IsUserActive.ToString() + " requested ...");

            _IsUserActive = IsUserActive;

            if (!_IsUserActive)
            {
                if (_CacheTimer.Enabled) _CacheTimer.Stop();
                if (_StatusTimer.Enabled) _StatusTimer.Stop();

                _AllInfoMonitors_Mix = new List<(MonitorInfo_complex, MonitorInfo)>();
                _AllInfoMonitors = new List<MonitorInfo_complex>();

                while (!_TaskQueue.IsEmpty())
                {
                    if (_TaskQueueExecutor.IsBusy) _TaskQueueExecutor.CancelAsync();
                    else
                    {
                        _TaskQueue = new TaskLockQueue<ParameterType>();
                        _TaskQueueResult = new ResultLockPool();
                    }
                    _CancelhashSet.Clear();
                }

                _AddSignalfor0X52 = 0;
                _AddSignalforStatusCheck = 0;

                //------------------------------------------------------------------------------------------------//
                DisplaychangedEventArgs _displaychangedEventArgss = new DisplaychangedEventArgs()
                {
                    count = _AllInfoMonitors_Mix.Count,
                    monitors = new List<MonitorInfo>(),
                };
                OnDisplaychanged(_displaychangedEventArgss);
                //------------------------------------------------------------------------------------------------//
            }

            return Task.CompletedTask;
        }

        public Task CancelVcpTask(Guid user_guid)
        {
            _logs.DebugMsg("[VcpCorePlugin] VcpCorePlugin received CancelVCPTask requested ...");

            _logs.DebugMsg($"[VcpCorePlugin] Before CancelhashSet Count : {_CancelhashSet.Count}");

            if (user_guid != default)
                _CancelhashSet.Add(user_guid);

            _logs.DebugMsg($"[VcpCorePlugin] After CancelhashSet Count : {_CancelhashSet.Count}");

            return Task.CompletedTask;
        }

        public Task<List<MonitorInfo>> GetMonitors()
        {
            _logs.DebugMsg("[VcpCorePlugin] VcpCorePlugin received Monitors List requested ...");

            List<MonitorInfo> _AllDisplays = _AllInfoMonitors_Mix.Select(M => M.Item2).ToList();

            _logs.DebugMsg("[VcpCorePlugin] AllInfoMonitors count is " + _AllDisplays.Count.ToString());

            return Task.FromResult(_AllDisplays);
        }

        public async Task<List<MonitorInfo>> Re_GetMonitors(CancellationToken Token)
        {
            _logs.DebugMsg("[VcpCorePlugin] VcpCorePlugin received Re-Get Monitors List requested ...");

            try
            {
                bool IsFinishedAlready = false;
                var CancelStatusCheck = Task.Run(() => CancellationCheck(Token, in IsFinishedAlready));
                var ReGetTask = Task.Run(() => InitializeMonitorsList(true, Token));

                List<MonitorInfo> _AllDisplays = new List<MonitorInfo>();

                if (await Task.WhenAny(ReGetTask, CancelStatusCheck) == ReGetTask)
                {
                    IsFinishedAlready = true;
                    _logs.DebugMsg("[VcpCorePlugin] ReGetTask finished faster than CancelStatusCheck");
                    _logs.DebugMsg("[VcpCorePlugin] Re_GetMonitors() _AllInfoMonitors_Mix.Count is " + _AllInfoMonitors_Mix.Count);
                }
                else
                {
                    IsFinishedAlready = true;
                    _logs.DebugMsg("[VcpCorePlugin] CancelStatusCheck finished faster than ReGetTask");
                    _logs.DebugMsg("[VcpCorePlugin] Re_GetMonitors() _AllInfoMonitors_Mix.Count is " + _AllInfoMonitors_Mix.Count);
                }

                return _AllInfoMonitors_Mix.Select(x => x.Item2).ToList();
            }
            catch (Exception e)
            {
                _logs.DebugMsg("[VcpCorePlugin] Re-GetMonitors Exception : " + e.Message);
                return new List<MonitorInfo>();
            }
        }

        public async Task<List<MultiCommandArch>> MultiCommandsRun(List<MultiCommandArch> _multiCommands)
        {
            _logs.DebugMsg("[VcpCorePlugin] VcpCorePlugin received MultiCommandRun requested ...");

            try
            {
                Task<MultiCommandArch>[] tasksList = new Task<MultiCommandArch>[_multiCommands.Count];

                for (int i = 0; i < _multiCommands.Count; i++)
                {
                    var command = _multiCommands[i];
                    tasksList[i] = CommandRun(command);
                }

                await Task.WhenAll(tasksList);

                for (int i = 0; i < _multiCommands.Count; i++)
                    _multiCommands[i].Result = (await tasksList[i]).Result;

                return _multiCommands;
            }
            catch (Exception ex)
            {
                _logs.DebugMsg($"[VcpCorePlugin] VcpCorePlugin MultiCommandRun exception : {ex.Message} ...");
                return _multiCommands;
            }

            async Task<MultiCommandArch> CommandRun(MultiCommandArch command)
            {
                if (command.Action.Equals(MultiCommandAction.GetMonitors))
                {
                    var r = await GetMonitors().ConfigureAwait(false);
                    command.Result = r;
                    return command;
                }
                else if (command.Action.Equals(MultiCommandAction.GetCapabilitiesString))
                {
                    var r = await GetCapabilitiesString(command.MonitorInfo, command.Guid, command.Priority).ConfigureAwait(false);
                    command.Result = r;
                    return command;
                }
                else if (command.Action.Equals(MultiCommandAction.GetVCPCapabilities))
                {
                    var r = await GetCapabilitiesString(command.MonitorInfo, command.Guid, command.Priority).ConfigureAwait(false);
                    command.Result = r;
                    return command;
                }
                else if (command.Action.Equals(MultiCommandAction.GetVCPCapability))
                {
                    ObjGetVCP r = new ObjGetVCP();
                    if (command.Function is string)
                        r = await GetVCPCapability(command.MonitorInfo, command.Function.ToString(), command.Guid, command.Opt, command.Priority).ConfigureAwait(false);
                    else
                        r = await GetVCPCapability(command.MonitorInfo, (byte)command.Function, command.Guid, command.Opt, command.Priority).ConfigureAwait(false);

                    command.Result = r;
                    return command;
                }
                else if (command.Action.Equals(MultiCommandAction.SetVCPCapability))
                {
                    bool r = false;

                    if ((command.Function is string) && (command.Value is string))
                        r = await SetVCPCapability(command.MonitorInfo, command.Function.ToString(), command.Value.ToString(), command.Guid, command.Priority).ConfigureAwait(false);
                    else
                        r = await SetVCPCapability(command.MonitorInfo, (byte)command.Function, (uint)command.Value, command.Guid, command.Priority).ConfigureAwait(false);

                    command.Result = r;
                    return command;
                }
                else if (command.Action.Equals(MultiCommandAction.GetVCPCacheTable))
                {
                    var r = await GetVCPCacheTable().ConfigureAwait(false);
                    command.Result = r;
                    return command;
                }
                else if (command.Action.Equals(MultiCommandAction.CancelVcpTask))
                {
                    await CancelVcpTask(command.Guid).ConfigureAwait(false);
                    return command;
                }
                else
                    return command;
            }
        }

        public Task<string> GetCapabilitiesString(MonitorInfo monitorInfo, Guid guid = default, Priority priority = Priority.Low)
        {
            _logs.DebugMsg("[VcpCorePlugin] VcpCorePlugin received GetCapabilitiesString requested ...");

            _pauseEvent.WaitOne(Timeout.Infinite);
            _logs.DebugMsg("[VcpCorePlugin] VcpCorePlugin GetCapabilitiesString overgo WaitOne ...");

            if (IsOutInitialize && _AllInfoMonitors_Mix.Count > 0)
            {
                _logs.DebugMsg("[VcpCorePlugin] TargetMonitor DisplayName is " + monitorInfo.DisplayName);
                _logs.DebugMsg("[VcpCorePlugin] TargetMonitor AliasDeviceName is " + monitorInfo.AliasDeviceName);

                foreach (var moX in _AllInfoMonitors_Mix)
                {
                    if (monitorInfo.Equals(moX.Item2))
                    {
                        if (moX.Item2.DDCisON)
                        {
                            object or;

                            if ((or = _CacheTable.GetFromCacheTable(moX.Item1, "CapibilityString".ToLower(CultureInfo.InvariantCulture))) != null)
                                return Task.FromResult(or.ToString());
                            else
                            {
                                //
                                Guid _guid = Guid.NewGuid();
                                _logs.DebugMsg("[VcpCorePlugin] New Job Guid is " + _guid.ToString());

                                ParameterType parameterType = new ParameterType(Queue_CommandType.GetCapabilitiesString, new Type_GetCapabilitiesString(_guid, moX.Item1, guid));
                                _TaskQueue.Enqueue(parameterType, priority);

                                Launch_TaskQueueExecutor();

                                or = GetResultObjectAsync(_guid).Result;
                                string r = (or != null) ? or.ToString() : string.Empty;

                                return Task.FromResult(r);
                                //
                            }
                        }
                        else
                        {
                            _logs.DebugMsg("[VcpCorePlugin] GetCapabilitiesString Fail => DDCisON is false");
                            return Task.FromResult(string.Empty);
                        }
                    }
                }

                _logs.DebugMsg("[VcpCorePlugin] No target display to work. So, ignor requested");
                return Task.FromResult(string.Empty);
            }
            else
            {
                _logs.DebugMsg("[VcpCorePlugin] No monitors to work or is Initialize. So, ignor requested");
                return Task.FromResult(string.Empty);
            }
        }

        public Task<string> GetVCPCapabilities(MonitorInfo monitorInfo, Guid guid = default, Priority priority = Priority.Low)
        {
            _logs.DebugMsg("[VcpCorePlugin] VcpCorePlugin received GetVCPCapabilities requested ...");

            _pauseEvent.WaitOne(Timeout.Infinite);
            _logs.DebugMsg("[VcpCorePlugin] VcpCorePlugin GetVCPCapabilities overgo WaitOne ...");

            if (IsOutInitialize && _AllInfoMonitors_Mix.Count > 0)
            {
                _logs.DebugMsg("[VcpCorePlugin] TargetMonitor DisplayName is " + monitorInfo.DisplayName);
                _logs.DebugMsg("[VcpCorePlugin] TargetMonitor AliasDeviceName is " + monitorInfo.AliasDeviceName);

                foreach (var moX in _AllInfoMonitors_Mix)
                {
                    if (monitorInfo.Equals(moX.Item2))
                    {
                        if (moX.Item2.DDCisON)
                        {
                            object or;

                            if ((or = _CacheTable.GetFromCacheTable(moX.Item1, "Capabilities".ToLower(CultureInfo.InvariantCulture))) != null)
                                return Task.FromResult(or.ToString());
                            else
                            {
                                //
                                Guid _guid = Guid.NewGuid();
                                _logs.DebugMsg("[VcpCorePlugin] New Job Guid is " + _guid.ToString());

                                ParameterType parameterType = new ParameterType(Queue_CommandType.GetVCPCapabilities, new Type_GetVCPCapabilities(_guid, moX.Item1, guid));
                                _TaskQueue.Enqueue(parameterType, priority);

                                Launch_TaskQueueExecutor();

                                or = GetResultObjectAsync(_guid).Result;
                                string r = (or != null) ? or.ToString() : string.Empty;

                                return Task.FromResult(r);
                                //
                            }
                        }
                        else
                        {
                            _logs.DebugMsg("[VcpCorePlugin] GetVCPCapabilities Fail => DDCisON is false");
                            return Task.FromResult(string.Empty);
                        }
                    }
                }

                _logs.DebugMsg("[VcpCorePlugin] No target display to work. So, ignor requested");
                return Task.FromResult(string.Empty);
            }
            else
            {
                _logs.DebugMsg("[VcpCorePlugin] No monitors to work or is Initialize. So, ignor requested");
                return Task.FromResult(string.Empty);
            }
        }

        public Task<ObjGetVCP> GetVCPCapability(MonitorInfo monitorInfo, byte code, Guid guid = default, int opt = 0, Priority priority = Priority.Low)
        {
            _logs.DebugMsg("[VcpCorePlugin] VcpCorePlugin received GetVCPCapability requested ...");

            _pauseEvent.WaitOne(Timeout.Infinite);
            _logs.DebugMsg("[VcpCorePlugin] VcpCorePlugin GetVCPCapability overgo WaitOne ...");

            if (IsOutInitialize && _AllInfoMonitors_Mix.Count > 0)
            {
                _logs.DebugMsg("[VcpCorePlugin] TargetMonitor DisplayName is " + monitorInfo.DisplayName);
                _logs.DebugMsg("[VcpCorePlugin] TargetMonitor AliasDeviceName is " + monitorInfo.AliasDeviceName);
                _logs.DebugMsg("[VcpCorePlugin] VcpCode is " + BitConverter.ToString(new byte[] { code }));
                _logs.DebugMsg("[VcpCorePlugin] opt is " + opt.ToString());

                foreach (var moX in _AllInfoMonitors_Mix)
                {
                    if (monitorInfo.Equals(moX.Item2))
                    {
                        if (moX.Item2.DDCisON)
                        {
                            if (IsVcpFunctionSupport(moX.Item1, code))
                            {
                                object or;

                                if ((or = _CacheTable.GetFromCacheTable(moX.Item1, code)) != null)
                                    return Task.FromResult(new ObjGetVCP() { value = or, result = true });
                                else
                                {
                                    //
                                    Guid _guid = Guid.NewGuid();
                                    _logs.DebugMsg("[VcpCorePlugin] New Job Guid is " + _guid.ToString());

                                    ParameterType parameterType = new ParameterType(Queue_CommandType.GetVCPCapability_I, new Type_GetVCPCapability_I(_guid, moX.Item1, code, guid, opt));
                                    _TaskQueue.Enqueue(parameterType, priority);

                                    Launch_TaskQueueExecutor();

                                    or = GetResultObjectAsync(_guid).Result;
                                    ObjGetVCP r = (or != null) ? (new ObjGetVCP() { value = or, result = true }) : (new ObjGetVCP() { value = or, result = false });

                                    return Task.FromResult(r);
                                    //
                                }
                            }
                            else
                            {
                                _logs.DebugMsg("[VcpCorePlugin] GetVCPCapability Fail => IsVcpFunctionSupport is false");
                                return Task.FromResult(new ObjGetVCP() { value = null, result = false });
                            }
                        }
                        else
                        {
                            _logs.DebugMsg("[VcpCorePlugin] GetVCPCapability Fail => DDCisON is false");
                            return Task.FromResult(new ObjGetVCP() { value = null, result = false });
                        }
                    }
                }

                _logs.DebugMsg("[VcpCorePlugin] No target display to work. So, ignor requested");
                return Task.FromResult(new ObjGetVCP() { value = null, result = false });
            }
            else
            {
                _logs.DebugMsg("[VcpCorePlugin] No monitors to work or is Initialize. So, ignor requested");
                return Task.FromResult(new ObjGetVCP() { value = null, result = false });
            }
        }

        public Task<ObjGetVCP> GetVCPCapability(MonitorInfo monitorInfo, string FunctionName, Guid guid = default, int opt = 0, Priority priority = Priority.Low)
        {
            _logs.DebugMsg("[VcpCorePlugin] VcpCorePlugin received GetVCPCapability requested ...");

            _pauseEvent.WaitOne(Timeout.Infinite);
            _logs.DebugMsg("[VcpCorePlugin] VcpCorePlugin GetVCPCapability overgo WaitOne ...");

            if (IsOutInitialize && _AllInfoMonitors_Mix.Count > 0)
            {
                _logs.DebugMsg("[VcpCorePlugin] TargetMonitor DisplayName is " + monitorInfo.DisplayName);
                _logs.DebugMsg("[VcpCorePlugin] TargetMonitor AliasDeviceName is " + monitorInfo.AliasDeviceName);
                _logs.DebugMsg("[VcpCorePlugin] VcpCode is " + FunctionName);
                _logs.DebugMsg("[VcpCorePlugin] opt is " + opt.ToString());

                foreach (var moX in _AllInfoMonitors_Mix)
                {
                    if (monitorInfo.Equals(moX.Item2))
                    {
                        if (moX.Item2.DDCisON)
                        {
                            object or;

                            if ((or = _CacheTable.GetFromCacheTable(moX.Item1, FunctionName.ToLower(CultureInfo.InvariantCulture))) != null)
                                return Task.FromResult(new ObjGetVCP() { value = or, result = true });
                            else
                            {
                                //
                                Guid _guid = Guid.NewGuid();
                                _logs.DebugMsg("[VcpCorePlugin] New Job Guid is " + _guid.ToString());

                                ParameterType parameterType = new ParameterType(Queue_CommandType.GetVCPCapability_II, new Type_GetVCPCapability_II(_guid, moX.Item1, FunctionName, guid, opt));
                                _TaskQueue.Enqueue(parameterType, priority);

                                Launch_TaskQueueExecutor();

                                or = GetResultObjectAsync(_guid).Result;
                                ObjGetVCP r = (or != null) ? (new ObjGetVCP() { value = or, result = true }) : (new ObjGetVCP() { value = or, result = false });

                                return Task.FromResult(r);
                                //
                            }
                        }
                        else
                        {
                            _logs.DebugMsg("[VcpCorePlugin] GetVCPCapability Fail => DDCisON is false");
                            return Task.FromResult(new ObjGetVCP() { value = null, result = false });
                        }
                    }
                }

                _logs.DebugMsg("[VcpCorePlugin] No target display to work. So, ignor requested");
                return Task.FromResult(new ObjGetVCP() { value = null, result = false });
            }
            else
            {
                _logs.DebugMsg("[VcpCorePlugin] No monitors to work or is Initialize. So, ignor requested");
                return Task.FromResult(new ObjGetVCP() { value = null, result = false });
            }
        }

        public Task<bool> SetVCPCapability(MonitorInfo monitorInfo, byte code, uint val, Guid guid = default, Priority priority = Priority.Low)
        {
            _logs.DebugMsg("[VcpCorePlugin] VcpCorePlugin received SetVCPCapability requested ...");

            _pauseEvent.WaitOne(Timeout.Infinite);
            _logs.DebugMsg("[VcpCorePlugin] VcpCorePlugin SetVCPCapability overgo WaitOne ...");

            if (IsOutInitialize && _AllInfoMonitors_Mix.Count > 0)
            {
                _logs.DebugMsg("[VcpCorePlugin] TargetMonitor DisplayName is " + monitorInfo.DisplayName);
                _logs.DebugMsg("[VcpCorePlugin] TargetMonitor AliasDeviceName is " + monitorInfo.AliasDeviceName);
                _logs.DebugMsg("[VcpCorePlugin] VcpCode is " + BitConverter.ToString(new byte[] { code }));
                _logs.DebugMsg("[VcpCorePlugin] val is " + val.ToString());

                foreach (var moX in _AllInfoMonitors_Mix)
                {
                    if (monitorInfo.Equals(moX.Item2))
                    {
                        if (moX.Item2.DDCisON)
                        {
                            if (IsVcpFunctionSupport(moX.Item1, code))
                            {
                                //
                                Guid _guid = Guid.NewGuid();
                                _logs.DebugMsg("[VcpCorePlugin] New Job Guid is " + _guid.ToString());

                                ParameterType parameterType = new ParameterType(Queue_CommandType.SetVCPCapability_I, new Type_SetVCPCapability_I(_guid, moX.Item1, code, val, guid));
                                _TaskQueue.Enqueue(parameterType, priority);

                                Launch_TaskQueueExecutor();

                                object or = GetResultObjectAsync(_guid).Result;
                                bool r = (or != null) ? ((bool)or) : false;

                                return Task.FromResult(r);
                                //
                            }
                            else
                            {
                                _logs.DebugMsg("[VcpCorePlugin] SetVCPCapability Fail => IsVcpFunctionSupport is false");
                                return Task.FromResult(false);
                            }
                        }
                        else
                        {
                            _logs.DebugMsg("[VcpCorePlugin] SetVCPCapability Fail => DDCisON is false");
                            return Task.FromResult(false);
                        }
                    }
                }

                _logs.DebugMsg("[VcpCorePlugin] No target display to work. So, ignor requested");
                return Task.FromResult(false);
            }
            else
            {
                _logs.DebugMsg("[VcpCorePlugin] No monitors to work or is Initialize. So, ignor requested");
                return Task.FromResult(false);
            }
        }

        public Task<bool> SetVCPCapability(MonitorInfo monitorInfo, string FunctionName, string val, Guid guid = default, Priority priority = Priority.Low)
        {
            _logs.DebugMsg("[VcpCorePlugin] VcpCorePlugin received SetVCPCapability requested ...");

            _pauseEvent.WaitOne(Timeout.Infinite);
            _logs.DebugMsg("[VcpCorePlugin] VcpCorePlugin SetVCPCapability overgo WaitOne ...");

            if (IsOutInitialize && _AllInfoMonitors_Mix.Count > 0)
            {
                _logs.DebugMsg("[VcpCorePlugin] TargetMonitor DisplayName is " + monitorInfo.DisplayName);
                _logs.DebugMsg("[VcpCorePlugin] TargetMonitor AliasDeviceName is " + monitorInfo.AliasDeviceName);
                _logs.DebugMsg("[VcpCorePlugin] VcpCode is " + FunctionName);
                _logs.DebugMsg("[VcpCorePlugin] val is " + val);

                foreach (var moX in _AllInfoMonitors_Mix)
                {
                    if (monitorInfo.Equals(moX.Item2))
                    {
                        if (moX.Item2.DDCisON)
                        {
                            //
                            Guid _guid = Guid.NewGuid();
                            _logs.DebugMsg("[VcpCorePlugin] New Job Guid is " + _guid.ToString());

                            ParameterType parameterType = new ParameterType(Queue_CommandType.SetVCPCapability_II, new Type_SetVCPCapability_II(_guid, moX.Item1, FunctionName, val, guid));
                            _TaskQueue.Enqueue(parameterType, priority);

                            Launch_TaskQueueExecutor();

                            object or = GetResultObjectAsync(_guid).Result;
                            bool r = (or != null) ? ((bool)or) : false;

                            return Task.FromResult(r);
                            //
                        }
                        else
                        {
                            _logs.DebugMsg("[VcpCorePlugin] SetVCPCapability Fail => DDCisON is false");
                            return Task.FromResult(false);
                        }
                    }
                }

                _logs.DebugMsg("[VcpCorePlugin] No target display to work. So, ignore requested");
                return Task.FromResult(false);
            }
            else
            {
                _logs.DebugMsg("[VcpCorePlugin] No monitors to work or is Initialize. So, ignore requested");
                return Task.FromResult(false);
            }
        }

        #endregion

        #region Private Methods

        private void CancellationCheck(CancellationToken token, in bool IsFinishedAlready)
        {
            var can_Time = new CancellationTokenSource(15000);
            var canI_All = CancellationTokenSource.CreateLinkedTokenSource(can_Time.Token, token);
            while (!IsFinishedAlready)
            {
                if (canI_All.Token.IsCancellationRequested)
                {
                    _logs.DebugMsg("[VcpCorePlugin] Re_GetMonitors() IsCancellationRequested is True");
                    return;
                }
            }
            _logs.DebugMsg("[VcpCorePlugin] Re_GetMonitors() while loop exit");
            return;
        }

        private void Initialize0x52toEmpty()
        {
            _logs.DebugMsg("[VcpCorePlugin] VcpCorePlugin received Initialize0x52toEmpty requested ...");

            if (IsOutInitialize && _AllInfoMonitors_Mix.Count > 0)
            {
                Guid _guid = Guid.NewGuid();
                _logs.DebugMsg("[VcpCorePlugin] New Job Guid is " + _guid.ToString());

                ParameterType parameterType = new ParameterType(Queue_CommandType.Initialize0x52toEmpty, new Type_Initialize0x52toEmpty(_guid, default));
                _TaskQueue.Enqueue(parameterType, Priority.Low);

                Launch_TaskQueueExecutor();
            }
            else
                _logs.DebugMsg("[VcpCorePlugin] No monitors to work or is in Initialize. So, ignore requested");
        }

        private void Watcher0x52()
        {
            _logs.DebugMsg("[VcpCorePlugin] VcpCorePlugin received Watcher0x52 requested ...");

            if (IsOutInitialize && _AllInfoMonitors_Mix.Count > 0)
            {
                if (_AddSignalfor0X52 < 1)
                {
                    _logs.DebugMsg("[VcpCorePlugin] VcpCorePlugin add Watcher0x52 Task to TaskQueue ...");
                    Interlocked.Add(ref _AddSignalfor0X52, 1);
                    _logs.DebugMsg($"[VcpCorePlugin] After add Watcher0x52 Task _AddSignalfor0X52: {_AddSignalfor0X52}");

                    Guid _guid = Guid.NewGuid();
                    _logs.DebugMsg("[VcpCorePlugin] New Job Guid is " + _guid.ToString());

                    ParameterType parameterType = new ParameterType(Queue_CommandType.Watcher0x52, new Type_Watcher0x52(_guid, default));
                    _TaskQueue.Enqueue(parameterType, Priority.Low);

                    Launch_TaskQueueExecutor();
                }
                else
                    _logs.DebugMsg("[VcpCorePlugin] TaskQueue exist the same as Watcher0x52 Task alredy, so ...Ignore");
            }
            else
                _logs.DebugMsg("[VcpCorePlugin] No monitors to work or is in Initialize. So, ignore requested");
        }

        private void Watcher0x02forStatusCheck()
        {
            _logs.DebugMsg("[VcpCorePlugin] VcpCorePlugin received Watcher0x02forStatusCheck requested ...");

            if (IsOutInitialize && _AllInfoMonitors_Mix.Count > 0)
            {
                if (_AddSignalforStatusCheck < 1)
                {
                    _logs.DebugMsg("[VcpCorePlugin] VcpCorePlugin add Watcher0x02forStatusCheck Task to TaskQueue ...");
                    Interlocked.Add(ref _AddSignalforStatusCheck, 1);
                    _logs.DebugMsg($"[VcpCorePlugin] After add Watcher0x02forStatusCheck Task _AddSignalforStatusCheck: {_AddSignalforStatusCheck}");

                    Guid _guid = Guid.NewGuid();
                    _logs.DebugMsg("[VcpCorePlugin] New Job Guid is " + _guid.ToString());

                    ParameterType parameterType = new ParameterType(Queue_CommandType.Watcher0x02forStatusCheck, new Type_Watcher0x02forStatusCheck(_guid, default));
                    _TaskQueue.Enqueue(parameterType, Priority.Low);

                    Launch_TaskQueueExecutor();
                }
                else
                    _logs.DebugMsg("[VcpCorePlugin] TaskQueue exist the same as Watcher0x02forStatusCheck Task alredy, so ...Ignore");
            }
            else
                _logs.DebugMsg("[VcpCorePlugin] No monitors to work or is in Initialize. So, ignore requested");
        }

        private void Launch_TaskQueueExecutor()
        {
            try
            {
                Monitor.Enter(TaskQueueExecutorLock);

                if (!_TaskQueueExecutor.IsBusy && IsOutInitialize)
                    _TaskQueueExecutor.RunWorkerAsync();
            }
            catch (Exception)
            {
                _logs.DebugMsg("[VcpCorePlugin] Launch_TaskQueueExecutor() _TaskQueueExecutor.RunWorkerAsync Exception...");
            }
            finally
            {
                Monitor.Exit(TaskQueueExecutorLock);
            }
        }

        private async Task<object> GetResultObjectAsync(Guid guid)
        {
            try
            {
                _logs.DebugMsg("[VcpCorePlugin] VcpCorePlugin received GetResultObjectAsync requested ...");
                _logs.DebugMsg("[VcpCorePlugin] Guid is " + guid.ToString());

                using (var tokenSource = new CancellationTokenSource(30 * 1000))
                {
                    CancellationTokenSource newGetResultCancellationTokenSource;

                    if ((_cancellationTokenSource != null) && (!_cancellationTokenSource.Token.Equals(CancellationToken.None)))
                        newGetResultCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(tokenSource.Token, _cancellationTokenSource.Token);
                    else
                        newGetResultCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(tokenSource.Token);

                    try
                    {
                        var token = newGetResultCancellationTokenSource.Token;
                        var r = await Task.Run(() => GetQueueResult_(guid, token), token).ConfigureAwait(false);

                        return r;
                    }
                    catch (TaskCanceledException)
                    {
                        _logs.DebugMsg("[VcpCorePlugin] " + guid.ToString() + " GetResultObjectAsync requested  Timeout ...");
                        return null;
                    }
                    catch (OperationCanceledException)
                    {
                        _logs.DebugMsg("[VcpCorePlugin] " + guid.ToString() + " GetResultObjectAsync requested  Timeout ...");
                        return null;
                    }
                    catch (Exception e)
                    {
                        _logs.DebugMsg($"[VcpCorePlugin] --Task.Run ...GetResultObjectAsync is an exception-- ({e.Message})");
                        return null;
                    }
                    finally
                    {
                        tokenSource?.Dispose();
                        newGetResultCancellationTokenSource?.Dispose();
                        newGetResultCancellationTokenSource = null;
                    }
                }
            }
            catch (Exception ex)
            {
                _logs.DebugMsg("[VcpCorePlugin] GetResultObjectAsync ex: " + ex.Message);
                return null;
            }
        }

        private static object GetQueueResult_(Guid guid, CancellationToken c_token)
        {
            try
            {
                _logs.DebugMsg("[VcpCorePlugin] VcpCorePlugin into GetQueueResult_ ...");

                object result = null;
                bool r = false;

                while (_TaskQueueExecutor.IsBusy)
                {
                    c_token.ThrowIfCancellationRequested();

                    _logs.DebugMsg("[VcpCorePlugin] GetQueueResult _TaskQueueExecutor [" + guid.ToString() + "] is still Running ...");

                    if (!_TaskQueueResult.IsEmpty())
                    {
                        if (_TaskQueueResult.ContainsKey(guid))
                        {
                            r = _TaskQueueResult.TakeAway(guid, out result);
                            break;
                        }
                    }
                    else
                        _logs.DebugMsg("[VcpCorePlugin] GetQueueResult _TaskQueueResult is Empty ...");

                    Thread.Sleep(1000);
                }

                if (!c_token.IsCancellationRequested)
                {
                    if (!r)
                    {
                        _logs.DebugMsg("[VcpCorePlugin] GetQueueResult _TaskQueueExecutor Running Done ...");

                        if (!_TaskQueueResult.IsEmpty())
                        {
                            if (_TaskQueueResult.ContainsKey(guid))
                                r = _TaskQueueResult.TakeAway(guid, out result);
                        }
                        else
                            _logs.DebugMsg("[VcpCorePlugin] _TaskQueueResult is Empty ...");
                    }
                }
                else
                    _logs.DebugMsg("[VcpCorePlugin] GetQueueResult CancellationRequested is be true to trigger ...");

                _logs.DebugMsg(r ? ("[VcpCorePlugin] " + "Guid: " + guid.ToString() + " GetQueueResult_() is Success") : ("[VcpCorePlugin] " + "Guid: " + guid.ToString() + " GetQueueResult_() is Fail"));

                return result;
            }
            catch (TaskCanceledException)
            {
                _logs.DebugMsg("[VcpCorePlugin] GetQueueResult CancellationRequested trigger TaskCanceledException ...");
                return null;
            }
            catch (OperationCanceledException)
            {
                _logs.DebugMsg("[VcpCorePlugin] GetQueueResult CancellationRequested trigger OperationCanceledException ...");
                return null;
            }
            catch (Exception)
            {
                _logs.DebugMsg("[VcpCorePlugin] GetQueueResult CancellationRequested trigger exception ...");
                return null;
            }
        }

        private void TaskQueueExecutor_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                _logs.DebugMsg("[VcpCorePlugin] TaskQueueExecutorDoWork TaskQueueExecutor is Starting ...");

                while (!_TaskQueue.IsEmpty())
                {
                    _logs.DebugMsg("[VcpCorePlugin] TaskQueueExecutorDoWork _TaskQueue SIZE : " + _TaskQueue.Count().ToString());

                    if (_TaskQueueExecutor.CancellationPending)
                    {
                        _logs.DebugMsg("[VcpCorePlugin] TaskQueueExecutorDoWork TaskQueueExecutor Cancellation Occur...");
                        _logs.DebugMsg("[VcpCorePlugin] TaskQueueExecutorDoWork _TaskQueue cleaning...");
                        _TaskQueue = new TaskLockQueue<ParameterType>();
                        _TaskQueueResult = new ResultLockPool();
                        _CancelhashSet.Clear();
                        _logs.DebugMsg("[VcpCorePlugin] TaskQueueExecutorDoWork _TaskQueue.IsEmpty(): " + _TaskQueue.IsEmpty().ToString());
                        e.Cancel = true;
                        return;
                    }
                    else
                    {
                        ParameterType p = _TaskQueue.Dequeue();
                        switch (p.CommandType)
                        {
                            case Queue_CommandType.GetCapabilitiesString:
                                {
                                    Type_GetCapabilitiesString parameter = (Type_GetCapabilitiesString)p.Parameter;
                                    if ((!_CancelhashSet.Contains(parameter.user_guid)) || parameter.user_guid.Equals(default))
                                    {
                                        _logs.DebugMsg(@"[VcpCorePlugin] TaskQueueExecutorDoWork doing GetCapabilitiesString GUID => " + parameter.guid);
                                        var rt = GetCapabilitiesString_(parameter.monitorInfoX);
                                        _TaskQueueResult.Add(parameter.guid, rt);
                                    }
                                    else
                                    {
                                        _logs.DebugMsg(@"[VcpCorePlugin] TaskQueueExecutorDoWork doing GetCapabilitiesString GUID in CancelhashSet =>" + parameter.guid);
                                        _TaskQueueResult.Add(parameter.guid, string.Empty);
                                    }
                                }
                                break;

                            case Queue_CommandType.GetVCPCapabilities:
                                {
                                    Type_GetVCPCapabilities parameter = (Type_GetVCPCapabilities)p.Parameter;
                                    if ((!_CancelhashSet.Contains(parameter.user_guid)) || parameter.user_guid.Equals(default))
                                    {
                                        _logs.DebugMsg(@"[VcpCorePlugin] TaskQueueExecutorDoWork doing GetVCPCapabilities GUID => " + parameter.guid);
                                        var rt = GetVCPCapabilities_(parameter.monitorInfoX);
                                        _TaskQueueResult.Add(parameter.guid, rt);
                                    }
                                    else
                                    {
                                        _logs.DebugMsg(@"[VcpCorePlugin] TaskQueueExecutorDoWork doing GetVCPCapabilities GUID in CancelhashSet => " + parameter.guid);
                                        _TaskQueueResult.Add(parameter.guid, string.Empty);
                                    }
                                }
                                break;

                            case Queue_CommandType.GetVCPCapability_I:
                                {
                                    Type_GetVCPCapability_I parameter = (Type_GetVCPCapability_I)p.Parameter;
                                    if ((!_CancelhashSet.Contains(parameter.user_guid)) || parameter.user_guid.Equals(default))
                                    {
                                        _logs.DebugMsg(@"[VcpCorePlugin] TaskQueueExecutorDoWork doing GetVCPCapability_I GUID => " + parameter.guid);
                                        var rt = GetVCPCapability_(parameter.monitorInfoX, parameter.code, parameter.opt);
                                        _TaskQueueResult.Add(parameter.guid, rt);
                                    }
                                    else
                                    {
                                        _logs.DebugMsg(@"[VcpCorePlugin] TaskQueueExecutorDoWork doing GetVCPCapability_I GUID in CancelhashSet => " + parameter.guid);
                                        _TaskQueueResult.Add(parameter.guid, null);
                                    }
                                }
                                break;

                            case Queue_CommandType.GetVCPCapability_II:
                                {
                                    Type_GetVCPCapability_II parameter = (Type_GetVCPCapability_II)p.Parameter;
                                    if ((!_CancelhashSet.Contains(parameter.user_guid)) || parameter.user_guid.Equals(default))
                                    {
                                        _logs.DebugMsg(@"[VcpCorePlugin] TaskQueueExecutorDoWork doing GetVCPCapability_II GUID => " + parameter.guid);
                                        var rt = GetVCPCapability_(parameter.monitorInfoX, parameter.FunctionName, parameter.opt);
                                        _TaskQueueResult.Add(parameter.guid, rt);
                                    }
                                    else
                                    {
                                        _logs.DebugMsg(@"[VcpCorePlugin] TaskQueueExecutorDoWork doing GetVCPCapability_II GUID in CancelhashSet => " + parameter.guid);
                                        _TaskQueueResult.Add(parameter.guid, null);
                                    }
                                }
                                break;

                            case Queue_CommandType.SetVCPCapability_I:
                                {
                                    Type_SetVCPCapability_I parameter = (Type_SetVCPCapability_I)p.Parameter;
                                    if ((!_CancelhashSet.Contains(parameter.user_guid)) || parameter.user_guid.Equals(default))
                                    {
                                        _logs.DebugMsg(@"[VcpCorePlugin] TaskQueueExecutorDoWork doing SetVCPCapability_I GUID => " + parameter.guid);
                                        var rt = SetVCPCapability_(parameter.monitorInfoX, parameter.code, parameter.val);
                                        _TaskQueueResult.Add(parameter.guid, rt);
                                    }
                                    else
                                    {
                                        _logs.DebugMsg(@"[VcpCorePlugin] TaskQueueExecutorDoWork doing SetVCPCapability_I GUID in CancelhashSet => " + parameter.guid);
                                        _TaskQueueResult.Add(parameter.guid, false);
                                    }
                                }
                                break;

                            case Queue_CommandType.SetVCPCapability_II:
                                {
                                    Type_SetVCPCapability_II parameter = (Type_SetVCPCapability_II)p.Parameter;
                                    if ((!_CancelhashSet.Contains(parameter.user_guid)) || parameter.user_guid.Equals(default))
                                    {
                                        _logs.DebugMsg(@"[VcpCorePlugin] TaskQueueExecutorDoWork doing SetVCPCapability_II GUID => " + parameter.guid);
                                        var rt = SetVCPCapability_(parameter.monitorInfoX, parameter.FunctionName, parameter.val);
                                        _TaskQueueResult.Add(parameter.guid, rt);
                                    }
                                    else
                                    {
                                        _logs.DebugMsg(@"[VcpCorePlugin] TaskQueueExecutorDoWork doing SetVCPCapability_II GUID in CancelhashSet => " + parameter.guid);
                                        _TaskQueueResult.Add(parameter.guid, false);
                                    }
                                }
                                break;

                            case Queue_CommandType.Initialize0x52toEmpty:
                                {
                                    Type_Initialize0x52toEmpty parameter = (Type_Initialize0x52toEmpty)p.Parameter;
                                    if ((!_CancelhashSet.Contains(parameter.user_guid)) || parameter.user_guid.Equals(default))
                                    {
                                        _logs.DebugMsg(@"[VcpCorePlugin] TaskQueueExecutorDoWork doing Initialize0x52toEmpty GUID => " + parameter.guid);
                                        Initialize0x52toEmpty_();
                                    }
                                    else
                                        _logs.DebugMsg(@"[VcpCorePlugin] TaskQueueExecutorDoWork doing Initialize0x52toEmpty GUID in CancelhashSet => " + parameter.guid);
                                }
                                break;

                            case Queue_CommandType.Watcher0x52:
                                {
                                    _logs.DebugMsg($"[VcpCorePlugin] Before Dequeue Watcher0x52 Task _AddSignalfor0X52: {_AddSignalfor0X52}");
                                    if (_AddSignalfor0X52 > 0)
                                        Interlocked.Add(ref _AddSignalfor0X52, -1);
                                    _logs.DebugMsg($"[VcpCorePlugin] After Dequeue Watcher0x52 Task _AddSignalfor0X52: {_AddSignalfor0X52}");

                                    Type_Watcher0x52 parameter = (Type_Watcher0x52)p.Parameter;
                                    if ((!_CancelhashSet.Contains(parameter.user_guid)) || parameter.user_guid.Equals(default))
                                    {
                                        _logs.DebugMsg(@"[VcpCorePlugin] TaskQueueExecutorDoWork doing Watcher0x52 GUID => " + parameter.guid);
                                        Watcher0x52_();
                                    }
                                    else
                                        _logs.DebugMsg(@"[VcpCorePlugin] TaskQueueExecutorDoWork doing Watcher0x52 GUID in CancelhashSet => " + parameter.guid);
                                }
                                break;

                            case Queue_CommandType.Watcher0x02forStatusCheck:
                                {
                                    _logs.DebugMsg($"[VcpCorePlugin] Before Dequeue Watcher0x02forStatusCheck Task _AddSignalforStatusCheck: {_AddSignalforStatusCheck}");
                                    if (_AddSignalforStatusCheck > 0)
                                        Interlocked.Add(ref _AddSignalforStatusCheck, -1);
                                    _logs.DebugMsg($"[VcpCorePlugin] After Dequeue Watcher0x02forStatusCheck Task _AddSignalforStatusCheck: {_AddSignalforStatusCheck}");

                                    Type_Watcher0x02forStatusCheck parameter = (Type_Watcher0x02forStatusCheck)p.Parameter;
                                    if ((!_CancelhashSet.Contains(parameter.user_guid)) || parameter.user_guid.Equals(default))
                                    {
                                        _logs.DebugMsg(@"[VcpCorePlugin] TaskQueueExecutorDoWork doing Watcher0x02forStatusCheck GUID => " + parameter.guid);
                                        Watcher0x02forStatusCheck_();
                                    }
                                    else
                                        _logs.DebugMsg(@"[VcpCorePlugin] TaskQueueExecutorDoWork doing Watcher0x02forStatusCheck GUID in CancelhashSet => " + parameter.guid);
                                }
                                break;

                            default:
                                break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logs.DebugMsg("[VcpCorePlugin] TaskQueueExecutorDoWork Exception : " + ex.Message);
                _TaskQueue = new TaskLockQueue<ParameterType>();
                _TaskQueueResult = new ResultLockPool();
                _CancelhashSet.Clear();
            }
        }

        private void TaskQueueExecutor_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
                _logs.DebugMsg("[VcpCorePlugin] VcpCorePlugin TaskQueueExecutor Cancellation ...");
            else if (e.Error != null)
                _logs.DebugMsg("[VcpCorePlugin] VcpCorePlugin TaskQueueExecutor exception: " + e.Error.ToString() + " ...");
            else
                _logs.DebugMsg("[VcpCorePlugin] VcpCorePlugin TaskQueueExecutor Completed ...");
        }

        //-----&&&&&-----&&&&&-----&&&&&-----&&&&&-----&&&&&-----&&&&&-----&&&&&-----&&&&&-----&&&&&-----&&&&&-----//

        private string GetCapabilitiesString_(MonitorInfo_complex monitorInfoX)
        {
            try
            {
                _logs.DebugMsg("[VcpCorePlugin] [QueueTrigger] VcpCorePlugin started GetCapabilitiesString_ ...");
                _logs.DebugMsg("[VcpCorePlugin] [QueueTrigger] VcpCorePlugin received GetCapabilitiesString requested ...");
                _logs.DebugMsg("[VcpCorePlugin] [QueueTrigger] TargetMonitor DisplayName is " + monitorInfoX.DisplayName);
                _logs.DebugMsg("[VcpCorePlugin] [QueueTrigger] TargetMonitor AliasDeviceName is " + monitorInfoX.AliasDeviceName);

                if (IsOutInitialize && _AllInfoMonitors_Mix.Count > 0 && (_AllInfoMonitors_Mix.Exists(t => t.Item1.edid.Equals(monitorInfoX.edid))))
                {
                    if (!string.IsNullOrWhiteSpace(monitorInfoX.CapabilityString))
                    {
                        _logs.DebugMsg("[VcpCorePlugin] [QueueTrigger] GetCapabilitiesString_ Sucess ...");
                        return monitorInfoX.CapabilityString;
                    }
                    else
                    {
                        if (monitorInfoX.DDCisON)
                        {
                            var r = GetCapabilities_String(in monitorInfoX, CancellationToken.None);

                            if (!string.IsNullOrEmpty(r))
                            {
                                _logs.DebugMsg("[VcpCorePlugin] [QueueTrigger] GetCapabilitiesString_ Success ...");
                                return r;
                            }
                            else
                            {
                                _logs.DebugMsg("[VcpCorePlugin] [QueueTrigger] GetCapabilitiesString_ Fail ...");
                                return string.Empty;
                            }
                        }
                        else
                            _logs.DebugMsg("[VcpCorePlugin] [QueueTrigger] GetCapabilitiesString_ Bcuz ddcci disconnect, So Capstr is empty ...");
                    }

                    return string.Empty;
                }
                else
                {
                    _logs.DebugMsg("[VcpCorePlugin] [QueueTrigger] Lose TargetMonitor or is in Initialize, ignore requested ...");
                    return string.Empty;
                }
            }
            catch (Exception ex)
            {
                _logs.DebugMsg("[VcpCorePlugin] [QueueTrigger] GetCapabilitiesString_ ex: " + ex.Message);
                return string.Empty;
            }
        }

        private string GetVCPCapabilities_(MonitorInfo_complex monitorInfoX)
        {
            try
            {
                _logs.DebugMsg("[VcpCorePlugin] [QueueTrigger] VcpCorePlugin started GetVCPCapabilities_ ...");

                var rcCache = _CacheTable.GetFromCacheTable(monitorInfoX, "Capabilities".ToLower(CultureInfo.InvariantCulture))?.ToString();

                if (!string.IsNullOrWhiteSpace(rcCache))
                    return rcCache;
                else
                {
                    if (_AllInfoMonitors_Mix.Count > 0 && (_AllInfoMonitors_Mix.Exists(t => t.Item1.edid.Equals(monitorInfoX.edid))))
                    {
                        string rcString = string.Empty;
                        if (!string.IsNullOrWhiteSpace(monitorInfoX.CapabilityString))
                        {
                            _logs.DebugMsg("[VcpCorePlugin] [QueueTrigger] VcpCorePlugin received GetVCPCapabilities requested ...");
                            _logs.DebugMsg("[VcpCorePlugin] [QueueTrigger] TargetMonitor DisplayName is " + monitorInfoX.DisplayName);
                            _logs.DebugMsg("[VcpCorePlugin] [QueueTrigger] TargetMonitor AliasDeviceName is " + monitorInfoX.AliasDeviceName);

                            ITokenizer tokenizer = new CapabilitiesTokenizer();
                            IParser parser = new CapabilitiesParser();
                            INodeFormatter formatter = new NodeFormatter();

                            var capabilitiestr = monitorInfoX.CapabilityString;
                            var tokens = tokenizer.GetTokens(capabilitiestr);
                            var node = parser.Parse(tokens);

                            if (!string.IsNullOrWhiteSpace(capabilitiestr) && (node.Nodes.Count() > 0))
                            {
                                var vcpNode = node.Nodes.RecursiveSelect(n => n.Nodes).Single(n => n.Value == "vcp");

                                StringWrite(vcpNode, ref rcString);

                                rcString += "ColorPreset\n";

                                foreach (string colorPreset in monitorInfoX.ColorPresetSupportList)
                                    rcString += "\t" + colorPreset + "\n";

                                //---------------------------------------------

                                List<JProperty> CapsDataMapJProperty = new List<JProperty>();
                                string[] Split_rcString = rcString.Trim().Split('\n');
                                for (int x = 0; x < Split_rcString.Length; x++)
                                {
                                    if (!Split_rcString[x].Contains('\t'))
                                    {
                                        if (x == (Split_rcString.Length - 1))
                                            CapsDataMapJProperty.Add(new JProperty(Split_rcString[x], null));
                                        else
                                        {
                                            if (!Split_rcString[x + 1].Contains('\t'))
                                                CapsDataMapJProperty.Add(new JProperty(Split_rcString[x], null));
                                            else
                                            {
                                                List<string> string_tmp = new List<string>();

                                                for (int y = (x + 1); y < Split_rcString.Length; y++)
                                                {
                                                    if (y == (Split_rcString.Length - 1))
                                                    {
                                                        if (Split_rcString[y].Contains('\t'))
                                                        {
                                                            string_tmp.Add(Split_rcString[y].Trim('\t'));
                                                            CapsDataMapJProperty.Add(new JProperty(Split_rcString[x], string_tmp));
                                                            x = y;
                                                        }
                                                        else
                                                        {
                                                            CapsDataMapJProperty.Add(new JProperty(Split_rcString[x], string_tmp));
                                                            x = (y - 1);
                                                        }
                                                        break;
                                                    }
                                                    else
                                                    {
                                                        if (Split_rcString[y].Contains('\t'))
                                                            string_tmp.Add(Split_rcString[y].Trim('\t'));
                                                        else
                                                        {
                                                            CapsDataMapJProperty.Add(new JProperty(Split_rcString[x], string_tmp));
                                                            x = (y - 1);
                                                            break;
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }

                                JObject obj = new JObject(
                                     new JProperty("Index", monitorInfoX.Index),
                                     new JProperty("ModelName", monitorInfoX.edid.ModelName),
                                     new JProperty("SerialNumber", monitorInfoX.edid.SerialNumber),
                                     new JProperty("ServiceTag", monitorInfoX.edid.ServiceTag),
                                     new JProperty("DeviceName", monitorInfoX.DisplayName),
                                     new JProperty("CapabilityString", monitorInfoX.CapabilityString),
                                     new JProperty("CapsDataMap", new JObject(CapsDataMapJProperty))
                                );

                                if (obj.ContainsKey("CapsDataMap"))
                                {
                                    JObject capsDataMap = (JObject)obj["CapsDataMap"];
                                    JArray input = new JArray();

                                    if (capsDataMap.ContainsKey("Input Select"))
                                        input = (JArray)capsDataMap["Input Select"];

                                    if (input.HasValues)
                                    {
                                        string T_strI = string.Empty;
                                        string T_strII = string.Empty;
                                        bool rc = false;
                                        for (int i = 0; i < input.Count; i++)
                                        {
                                            T_strI = System.Text.RegularExpressions.Regex.Replace(input[i].ToString(), @"\d", string.Empty);

                                            for (int j = 0; j < input.Count; j++)
                                            {
                                                if (i != j)
                                                {
                                                    T_strII = System.Text.RegularExpressions.Regex.Replace(input[j].ToString(), @"\d", string.Empty);

                                                    if (T_strI.Equals(T_strII))
                                                    {
                                                        rc = true;
                                                        break;
                                                    }
                                                }
                                            }

                                            if (!rc)
                                                input[i] = T_strI;
                                        }
                                    }
                                }

                                rcString = JsonConvert.SerializeObject(obj, Newtonsoft.Json.Formatting.Indented);

                                CapsDataMapJProperty.Clear();
                                Array.Clear(Split_rcString);

                                //---------------------------------------------
                            }
                        }
                        else
                            _logs.DebugMsg("[VcpCorePlugin] [QueueTrigger] VcpCorePlugin GetVCPCapabilities_ Bcuz ddcci disconnect, CapStr is empty ...");

                        return rcString;
                    }
                    else
                    {
                        _logs.DebugMsg("[VcpCorePlugin] [QueueTrigger] Lose TargetMonitor, ignore requested ...");
                        return string.Empty;
                    }
                }
            }
            catch (Exception ex)
            {
                _logs.DebugMsg("[VcpCorePlugin] [QueueTrigger] GetVCPCapabilities_ ex: " + ex.Message);
                return string.Empty;
            }
        }

        private object GetVCPCapability_(MonitorInfo_complex monitorInfoX, byte code, int opt = 0)
        {
            try
            {
                _logs.DebugMsg("[VcpCorePlugin] [QueueTrigger] VcpCorePlugin started GetVCPCapability_ ...");

                if (IsOutInitialize && _AllInfoMonitors_Mix.Count > 0 && (_AllInfoMonitors_Mix.Exists(t => t.Item1.edid.Equals(monitorInfoX.edid))))
                {
                    object r = null;
                    if (monitorInfoX.DDCisON)
                    {
                        _logs.DebugMsg("[VcpCorePlugin] [QueueTrigger] VcpCorePlugin received GetVCPCapability requested ...");
                        _logs.DebugMsg("[VcpCorePlugin] [QueueTrigger] TargetMonitor DisplayName is " + monitorInfoX.DisplayName);
                        _logs.DebugMsg("[VcpCorePlugin] [QueueTrigger] TargetMonitor AliasDeviceName is " + monitorInfoX.AliasDeviceName);
                        _logs.DebugMsg("[VcpCorePlugin] [QueueTrigger] VcpCode is " + BitConverter.ToString(new byte[] { code }));
                        _logs.DebugMsg("[VcpCorePlugin] [QueueTrigger] opt is " + opt.ToString());

                        if (opt == 0)
                            r = _CacheTable.GetFromCacheTable(monitorInfoX, code);

                        if (opt != 0 || r == null)
                            r = Get_VCPCapability(monitorInfoX, code, opt, IsOutInitialize);
                    }
                    else
                        _logs.DebugMsg("[VcpCorePlugin] [QueueTrigger] VcpCorePlugin GetVCPCapability_ Fail Bcuz ddcci disconnect ...");

                    return r;
                }
                else
                {
                    _logs.DebugMsg("[VcpCorePlugin] [QueueTrigger] Lose TargetMonitor or is in Initialize, ignore requested ...");
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logs.DebugMsg("[VcpCorePlugin] [QueueTrigger] GetVCPCapability_ ex: " + ex.Message);
                return null;
            }
        }

        private object GetVCPCapability_(MonitorInfo_complex monitorInfoX, string func, int opt = 0)
        {
            try
            {
                _logs.DebugMsg("[VcpCorePlugin] [QueueTrigger] VcpCorePlugin started GetVCPCapability_ ...");

                if (_AllInfoMonitors_Mix.Count > 0 && (_AllInfoMonitors_Mix.Exists(t => t.Item1.edid.Equals(monitorInfoX.edid))))
                {
                    object ro = null;
                    if (monitorInfoX.DDCisON)
                    {
                        _logs.DebugMsg("[VcpCorePlugin] [QueueTrigger] VcpCorePlugin received GetVCPCapability requested ...");
                        _logs.DebugMsg("[VcpCorePlugin] [QueueTrigger] TargetMonitor DisplayName is " + monitorInfoX.DisplayName);
                        _logs.DebugMsg("[VcpCorePlugin] [QueueTrigger] TargetMonitor AliasDeviceName is " + monitorInfoX.AliasDeviceName);
                        _logs.DebugMsg("[VcpCorePlugin] [QueueTrigger] VcpCode is " + func);
                        _logs.DebugMsg("[VcpCorePlugin] [QueueTrigger] opt is " + opt.ToString());

                        switch (func.ToUpper(CultureInfo.InvariantCulture))
                        {
                            case "INPUT SELECT":
                                {
                                    if (IsVcpFunctionSupport(monitorInfoX, 0x60))
                                    {
                                        var tmp = GetInputSource(monitorInfoX);
                                        var monitor = _AllInfoMonitors_Mix.FirstOrDefault(t => t.Item1.edid.Equals(monitorInfoX.edid));
                                        if (monitor.Item1 != null && monitor.Item2 != null)
                                        {
                                            monitor.Item1.inputSource = tmp.Item2;
                                            monitor.Item1.inputCable = tmp.Item1;
                                            monitor.Item2.inputSource = tmp.Item2;
                                            monitor.Item2.inputCable = tmp.Item1;

                                            Initialize2TypesMonitorInfo(false, CancellationToken.None);

                                            _logs.DebugMsg("[VcpCorePlugin] GetVCPCapability \"INPUT SELECT\" [inputCable] : " + tmp.Item1);
                                            _logs.DebugMsg("[VcpCorePlugin] GetVCPCapability \"INPUT SELECT\" [inputSource] : " + tmp.Item2);
                                            _logs.DebugMsg("[VcpCorePlugin] GetVCPCapability \"INPUT SELECT\" [monitor.Item1.inputSource] : " + monitor.Item1.inputSource);
                                            _logs.DebugMsg("[VcpCorePlugin] GetVCPCapability \"INPUT SELECT\" [monitor.Item1.inputCable] : " + monitor.Item1.inputCable);
                                            _logs.DebugMsg("[VcpCorePlugin] GetVCPCapability \"INPUT SELECT\" [monitor.Item2.inputSource] : " + monitor.Item2.inputSource);
                                            _logs.DebugMsg("[VcpCorePlugin] GetVCPCapability \"INPUT SELECT\" [monitor.Item2.inputCable] : " + monitor.Item2.inputCable);

                                            ro = tmp.Item2;
                                        }
                                        else
                                        {
                                            _logs.DebugMsg("[VcpCorePlugin] GetVCPCapability monitor.Item1 or monitor.Item2 = null");
                                        }
                                    }
                                    else
                                        _logs.DebugMsg("[VcpCorePlugin] GetVCPCapability inputsourcelist Fail => IsVcpFunctionSupport is false");
                                }
                                break;

                            case "INPUTSOURCELIST":
                                {
                                    if (IsVcpFunctionSupport(monitorInfoX, 0x60))
                                    {
                                        ro = _CacheTable.GetFromCacheTable(monitorInfoX, func.ToLower(CultureInfo.InvariantCulture));
                                        if (ro == null)
                                        {
                                            string VCPCapabilities_ = GetVCPCapabilities_(monitorInfoX);

                                            if (!string.IsNullOrWhiteSpace(VCPCapabilities_))
                                                _CacheTable.SetToCacheTable(monitorInfoX, "Capabilities".ToLower(CultureInfo.InvariantCulture), VCPCapabilities_);

                                            var obj = JObject.Parse(VCPCapabilities_);
                                            if (obj.ContainsKey("CapsDataMap"))
                                            {
                                                JObject capsDataMap = (JObject)obj["CapsDataMap"];
                                                JArray inputs = (JArray)capsDataMap["Input Select"];
                                                string R_ = string.Empty;
                                                List<InputSourceObject> list = new List<InputSourceObject>();
                                                foreach (var input in inputs)
                                                {
                                                    R_ = input.ToString();
                                                    var val_ = System.Text.RegularExpressions.Regex.Replace((R_.Substring(R_.Length - 1)), @"\d", string.Empty);
                                                    if (!string.IsNullOrWhiteSpace(val_))
                                                        val_ = R_ + "1";
                                                    else val_ = R_;

                                                    var Issucess = VcpCodeList.VCP60.TryGetValue(val_, out uint value);
                                                    if (Issucess)
                                                        list.Add(new InputSourceObject() { Name = R_, value = value });
                                                }
                                                ro = list;
                                                _CacheTable.SetToCacheTable(monitorInfoX, "inputsourcelist".ToLower(CultureInfo.InvariantCulture), ro);
                                            }
                                        }
                                    }
                                    else
                                        _logs.DebugMsg("[VcpCorePlugin] GetVCPCapability inputsourcelist Fail => IsVcpFunctionSupport is false");
                                }
                                break;

                            case @"USB-C PRIORITIZATION":
                                {
                                    if (IsVcpFunctionSupport(monitorInfoX, 0xEA))
                                    {
                                        ro = GetVcp2Steps(monitorInfoX, 0xEA, 0xF8FF);
                                        ro = (ro == null) ? ro : NodeFormatter.FormatVCP_F8(((uint)ro).ToString("X"));
                                    }
                                    else
                                        _logs.DebugMsg("[VcpCorePlugin] GetVCPCapability USB-C Prioritization Fail => IsVcpFunctionSupport is false");
                                }
                                break;

                            case "COLORPRESET":
                                {
                                    if (IsVcpFunctionSupport(monitorInfoX, 0xE2))
                                        ro = GetCurrentColorPreset(monitorInfoX);
                                    else
                                        _logs.DebugMsg("[VcpCorePlugin] GetVCPCapability ColorPreset Fail => IsVcpFunctionSupport is false");
                                }
                                break;

                            case "GAMING_GAMEENHANCEMENTMODE":
                                {
                                    if (IsVcpFunctionSupport(monitorInfoX, 0xF4))
                                    {
                                        var temp = GetVcp2Steps(monitorInfoX, VcpCodeList.VCPctr["Gaming"], 0x1F);
                                        if (temp != null)
                                            ro = ((uint)temp & 0x0f);
                                    }
                                    else
                                        _logs.DebugMsg("[VcpCorePlugin] GetVCPCapability Gaming_GameEnhancementMode Fail => IsVcpFunctionSupport is false");
                                }
                                break;

                            case "GAMING_RESPONSETIME":
                                {
                                    if (IsVcpFunctionSupport(monitorInfoX, 0xF4))
                                    {
                                        var temp = GetVcp2Steps(monitorInfoX, VcpCodeList.VCPctr["Gaming"], 0x2F);
                                        if (temp != null)
                                            ro = ((uint)temp & 0x0f);
                                    }
                                    else
                                        _logs.DebugMsg("[VcpCorePlugin] GetVCPCapability Gaming_GameEnhancementMode Fail => IsVcpFunctionSupport is false");
                                }
                                break;

                            case "GAMING_DARKSTABILIZER":
                                {
                                    if (IsVcpFunctionSupport(monitorInfoX, 0xF4))
                                    {
                                        var temp = GetVcp2Steps(monitorInfoX, VcpCodeList.VCPctr["Gaming"], 0x3F);
                                        if (temp != null)
                                            ro = ((uint)temp & 0x0f);
                                    }
                                    else
                                        _logs.DebugMsg("[VcpCorePlugin] GetVCPCapability Gaming_DarkStabilizer Fail => IsVcpFunctionSupport is false");
                                }
                                break;

                            case "GAMING_HDRTYPE":
                                {
                                    if (IsVcpFunctionSupport(monitorInfoX, 0xF4))
                                    {
                                        var temp = GetVcp2Steps(monitorInfoX, VcpCodeList.VCPctr["Gaming"], 0x4F);
                                        if (temp != null)
                                            ro = ((uint)temp & 0x0f);
                                    }
                                    else
                                        _logs.DebugMsg("[VcpCorePlugin] GetVCPCapability Gaming_HDRType Fail => IsVcpFunctionSupport is false");
                                }
                                break;

                            default:
                                {
                                    byte fucCode = TranslatorVCPctrCode(func);

                                    var roo = _CacheTable.GetFromCacheTable(monitorInfoX, fucCode);
                                    ro = roo ?? Get_VCPCapability(monitorInfoX, fucCode, opt, IsOutInitialize);
                                }
                                break;
                        }
                    }
                    else
                        _logs.DebugMsg("[VcpCorePlugin] [QueueTrigger] VcpCorePlugin GetVCPCapability_ Fail Bcuz ddcci disconnect ...");

                    return ro;
                }
                else
                {
                    _logs.DebugMsg("[VcpCorePlugin] [QueueTrigger] Lose TargetMonitor, ignore requested ...");

                    return null;
                }
            }
            catch (Exception ex)
            {
                _logs.DebugMsg("[VcpCorePlugin] [QueueTrigger] GetVCPCapability_ ex: " + ex.Message);
                return null;
            }
        }

        private bool SetVCPCapability_(MonitorInfo_complex monitorInfoX, byte code, uint val)
        {
            try
            {
                _logs.DebugMsg("[VcpCorePlugin] [QueueTrigger] VcpCorePlugin started SetVCPCapability_ ...");

                if (IsOutInitialize && _AllInfoMonitors_Mix.Count > 0 && (_AllInfoMonitors_Mix.Exists(t => t.Item1.edid.Equals(monitorInfoX.edid))))
                {
                    bool rc = false;
                    if (monitorInfoX.DDCisON)
                    {
                        _logs.DebugMsg("[VcpCorePlugin] [QueueTrigger] VcpCorePlugin received SetVCPCapability requested ...");
                        _logs.DebugMsg("[VcpCorePlugin] [QueueTrigger] TargetMonitor DisplayName is " + monitorInfoX.DisplayName);
                        _logs.DebugMsg("[VcpCorePlugin] [QueueTrigger] TargetMonitor AliasDeviceName is " + monitorInfoX.AliasDeviceName);
                        _logs.DebugMsg("[VcpCorePlugin] [QueueTrigger] VcpCode is " + BitConverter.ToString(new byte[] { code }));
                        _logs.DebugMsg("[VcpCorePlugin] [QueueTrigger] val is " + val.ToString());

                        rc = Set_VCPCapability(monitorInfoX, code, val, IsOutInitialize);

                        if (rc)
                        {
                            _CacheTable.SetToCacheTable(monitorInfoX, code, val);

                            switch (code)
                            {
                                case 0x04:
                                    InitializeCacheTable(CancellationToken.None);
                                    break;

                                case 0x05:
                                    InitializeCacheTable(CancellationToken.None);
                                    break;

                                default:
                                    break;
                            }
                        }
                    }
                    else
                        _logs.DebugMsg("[VcpCorePlugin] [QueueTrigger] VcpCorePlugin SetVCPCapability_ Fail Bcuz ddcci disconnect ...");

                    return rc;
                }
                else
                {
                    _logs.DebugMsg("[VcpCorePlugin] [QueueTrigger] Lose TargetMonitor or is in Initialize, ignore requested ...");
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logs.DebugMsg("[VcpCorePlugin] [QueueTrigger] SetVCPCapability_ ex: " + ex.Message);
                return false;
            }
        }

        private bool SetVCPCapability_(MonitorInfo_complex monitorInfoX, string FunctionName, string val)
        {
            try
            {
                _logs.DebugMsg("[VcpCorePlugin] [QueueTrigger] VcpCorePlugin started SetVCPCapability_ ...");

                if (IsOutInitialize && _AllInfoMonitors_Mix.Count > 0 && (_AllInfoMonitors_Mix.Exists(t => t.Item1.edid.Equals(monitorInfoX.edid))))
                {
                    bool rc = false;
                    if (monitorInfoX.DDCisON)
                    {
                        _logs.DebugMsg("[VcpCorePlugin] [QueueTrigger] VcpCorePlugin received SetVCPCapability requested ...");
                        _logs.DebugMsg("[VcpCorePlugin] [QueueTrigger] TargetMonitor DisplayName is " + monitorInfoX.DisplayName);
                        _logs.DebugMsg("[VcpCorePlugin] [QueueTrigger] TargetMonitor AliasDeviceName is " + monitorInfoX.AliasDeviceName);
                        _logs.DebugMsg("[VcpCorePlugin] [QueueTrigger] FunctionName is " + FunctionName);
                        _logs.DebugMsg("[VcpCorePlugin] [QueueTrigger] val is " + val);

                        switch (FunctionName.ToLower(CultureInfo.InvariantCulture))
                        {
                            case "colorpreset":
                                {
                                    rc = SetColorPreset(monitorInfoX, val);

                                    if (rc)
                                    {
                                        //_CacheTable.SetToCacheTable(monitorInfoX, FunctionName.ToLower(CultureInfo.InvariantCulture), val); // Jim modify to fix 0x52 colore preset no Synchronization issue

                                        VCPchangedEventArgs _VCPchangedEventArgs = new VCPchangedEventArgs();
                                        _VCPchangedEventArgs.vcpcode = FunctionName.ToLower(CultureInfo.InvariantCulture);
                                        _VCPchangedEventArgs.value = val;
                                        _VCPchangedEventArgs.monitor = (_AllInfoMonitors_Mix.Find(M => M.Item1.edid.Equals(monitorInfoX.edid))).Item2.Clone();

                                        OnVCPchanged(_VCPchangedEventArgs);
                                    }
                                }
                                break;

                            case "input select":
                                {
                                    var val_ = System.Text.RegularExpressions.Regex.Replace((val.Substring(val.Length - 1)), @"\d", string.Empty);
                                    if (!string.IsNullOrWhiteSpace(val_))
                                        val_ = val + "1";
                                    else val_ = val;

                                    byte fuc = TranslatorVCPctrCode(FunctionName);
                                    if (fuc != default)
                                    {
                                        var value = TranslatorVCPcategory(FunctionName, val_);

                                        if (value != default)
                                        {
                                            rc = SetVCPCapability_(monitorInfoX, fuc, value.Value);

                                            if (rc)
                                            {
                                                var inputsourcelist = _CacheTable.GetFromCacheTable(monitorInfoX, "inputsourcelist".ToLower(CultureInfo.InvariantCulture));
                                                if (inputsourcelist != null)
                                                {
                                                    var inputsourcelist_ = inputsourcelist as List<InputSourceObject>;

                                                    var IsExist = false;
                                                    foreach (var input in inputsourcelist_)
                                                    {
                                                        if (input.Name.Equals(val, StringComparison.OrdinalIgnoreCase))
                                                        {
                                                            IsExist = true;
                                                            val = input.Name;
                                                            break;
                                                        }
                                                    }
                                                    if (!IsExist)
                                                    {
                                                        val = System.Text.RegularExpressions.Regex.Replace(val, @"\d", string.Empty);
                                                        foreach (var input in inputsourcelist_)
                                                        {
                                                            if (input.Name.Equals(val, StringComparison.OrdinalIgnoreCase))
                                                            {
                                                                val = input.Name;
                                                                break;
                                                            }
                                                        }
                                                    }
                                                }

                                                _CacheTable.SetToCacheTable(monitorInfoX, FunctionName.ToLower(CultureInfo.InvariantCulture), val);

                                                foreach ((MonitorInfo_complex x, MonitorInfo o) in _AllInfoMonitors_Mix)
                                                {
                                                    if (x.Equals(monitorInfoX))
                                                    {
                                                        x.inputSource = val;
                                                        o.inputSource = val;

                                                        break;
                                                    }
                                                }

                                                Initialize2TypesMonitorInfo(false, CancellationToken.None);

                                                VCPchangedEventArgs _VCPchangedEventArgs = new VCPchangedEventArgs();
                                                _VCPchangedEventArgs.vcpcode = FunctionName.ToLower(CultureInfo.InvariantCulture);
                                                _VCPchangedEventArgs.value = val;
                                                _VCPchangedEventArgs.monitor = (_AllInfoMonitors_Mix.Find(M => M.Item1.edid.Equals(monitorInfoX.edid))).Item2.Clone();

                                                OnVCPchanged(_VCPchangedEventArgs);
                                            }
                                        }
                                    }
                                }
                                break;

                            default:
                                {
                                    byte fuc = TranslatorVCPctrCode(FunctionName);

                                    if (fuc != default)
                                    {
                                        var value = TranslatorVCPcategory(FunctionName, val);

                                        if (value != default)
                                        {
                                            rc = SetVCPCapability_(monitorInfoX, fuc, value.Value);

                                            if (rc)
                                            {
                                                _CacheTable.SetToCacheTable(monitorInfoX, FunctionName.ToLower(CultureInfo.InvariantCulture), val);

                                                VCPchangedEventArgs _VCPchangedEventArgs = new VCPchangedEventArgs();
                                                _VCPchangedEventArgs.vcpcode = FunctionName.ToLower(CultureInfo.InvariantCulture);
                                                _VCPchangedEventArgs.value = val;
                                                _VCPchangedEventArgs.monitor = (_AllInfoMonitors_Mix.Find(M => M.Item1.edid.Equals(monitorInfoX.edid))).Item2.Clone() ?? new MonitorInfo();

                                                OnVCPchanged(_VCPchangedEventArgs);
                                            }
                                        }
                                    }
                                }
                                break;
                        }
                    }
                    else
                        _logs.DebugMsg("[VcpCorePlugin] [QueueTrigger] VcpCorePlugin SetVCPCapability_ Fail Bcuz ddcci disconnect ...");

                    return rc;
                }
                else
                {
                    _logs.DebugMsg("[VcpCorePlugin] [QueueTrigger] Lose TargetMonitor or is in Initialize, ignore requested ...");
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logs.DebugMsg("[VcpCorePlugin] [QueueTrigger] SetVCPCapability_ ex: " + ex.Message);
                return false;
            }
        }

        private void Initialize0x52toEmpty_()
        {
            try
            {
                _logs.DebugMsg("[VcpCorePlugin] [QueueTrigger] VcpCorePlugin Initialize 0x52 to 0 Let's go ...");

                if (IsOutInitialize && _AllInfoMonitors_Mix.Count > 0)
                {
                    bool rc = false;
                    object object_0x02 = null;

                    for (int i = 0; ((i < _AllInfoMonitors_Mix.Count) && IsOutInitialize); i++)  //foreach (MonitorInfo_complex monitorInfoX in _AllInfoMonitors)
                    {
                        var monitorInfoX = _AllInfoMonitors_Mix[i].Item1;

                        if (monitorInfoX.DDCisON)
                        {
                            _logs.DebugMsg("[VcpCorePlugin] [QueueTrigger] VcpCorePlugin " + monitorInfoX.AliasDeviceName + " Initialize 0x52 to 0");

                            object_0x02 = Get_VCPCapability(monitorInfoX, 0x02, 0, IsOutInitialize);

                            var FailTimes = 0;
                            while ((object_0x02 != null) && (((uint)object_0x02) != 1) && (FailTimes < 5) && IsOutInitialize)
                            {
                                _logs.DebugMsg("[VcpCorePlugin] [QueueTrigger] VcpCorePlugin VCP 0x02 is " + ((uint)object_0x02).ToString("X"));

                                rc = Set_VCPCapability(monitorInfoX, 0x02, 0x01, IsOutInitialize);

                                _logs.DebugMsg("[VcpCorePlugin] [QueueTrigger] VcpCorePlugin set VCP 0x02 to 1 : " + (rc ? "Success" : "Fail"));

                                if (rc) FailTimes = 0;
                                else FailTimes++;

                                _logs.DebugMsg("[VcpCorePlugin] [QueueTrigger] VcpCorePlugin FailTimes : " + FailTimes.ToString());

                                object_0x02 = Get_VCPCapability(monitorInfoX, 0x02, 0, IsOutInitialize);
                            }
                        }
                    }
                    _logs.DebugMsg("[VcpCorePlugin] [QueueTrigger] VcpCorePlugin All Monitor Initialize 0x52 to 0 finish ...");
                }
                else
                    _logs.DebugMsg("[VcpCorePlugin] [QueueTrigger] Lose all monitors or in Initialize, ignore requested ...");
            }
            catch (Exception ex)
            {
                _logs.DebugMsg("[VcpCorePlugin] [QueueTrigger] Initialize0x52toEmpty_ ex: " + ex.Message);
            }
        }

        private void Watcher0x02forStatusCheck_()
        {
            try
            {
                _logs.DebugMsg("[VcpCorePlugin] [QueueTrigger] Watching 0x02 for Status Check Let's go ...");

                if (IsOutInitialize && _AllInfoMonitors_Mix.Count > 0)
                {
                    object object_0x02 = null;

                    for (int count = 0; ((count < _AllInfoMonitors_Mix.Count) && IsOutInitialize); count++)
                    {
                        var monitor = _AllInfoMonitors_Mix[count];

                        _logs.DebugMsg("[VcpCorePlugin] [QueueTrigger] VcpCorePlugin Watcher0x02forStatusCheck " + monitor.Item1.AliasDeviceName);

                        object_0x02 = Get_VCPCapability(monitor.Item1, 0x02, 0, IsOutInitialize);

                        if (object_0x02 != null)
                        {
                            _logs.DebugMsg("[VcpCorePlugin] [QueueTrigger] VcpCorePlugin Watcher0x02forStatusCheck 0x02 is " + ((uint)object_0x02).ToString("X"));
                            _logs.DebugMsg("[VcpCorePlugin] [QueueTrigger] VcpCorePlugin Watcher0x02forStatusCheck DDC/CI is connected");

                            var ori_DDCCIStatus = monitor.Item1.DDCisON;

                            monitor.Item1.DDCisON = true;
                            monitor.Item2.DDCisON = true;

                            var rt = UpdateMyself(ref monitor, CancellationToken.None);

                            if (rt.Item1)
                                _logs.DebugMsg("[VcpCorePlugin] [QueueTrigger] Watcher0x02forStatusCheck UpdateMyself Sucess");
                            else
                                _logs.DebugMsg("[VcpCorePlugin] [QueueTrigger] Watcher0x02forStatusCheck UpdateMyself Fail");

                            if (rt.Item2)
                            {
                                OnMonitorinfoUpdatechanged(new MonitorinfoUpdateEventArgs()
                                {
                                    edid = monitor.Item1.edid,
                                    monitor = monitor.Item2.Clone(),
                                });
                            }

                            if (!string.IsNullOrWhiteSpace(monitor.Item1.CapabilityString))
                            {
                                monitor.Item1.DDCCIFail = 0;

                                if (!ori_DDCCIStatus)
                                {
                                    _logs.DebugMsg("[VcpCorePlugin] [QueueTrigger] VcpCorePlugin Watcher0x02forStatusCheck prepare DDC/CI broadcast off -> on");

                                    monitor.Item1.DDCisON = true;
                                    monitor.Item2.DDCisON = true;

                                    Initialize2TypesMonitorInfo(false, CancellationToken.None);

                                    DDCCIchangedEventArgs _ddcceventArg = new DDCCIchangedEventArgs();
                                    _ddcceventArg.DDCisON = true;
                                    _ddcceventArg.monitors = monitor.Item2.Clone();

                                    OnDDCCIStatuschanged(_ddcceventArg);
                                }
                            }
                            else
                            {
                                monitor.Item1.DDCisON = false;
                                monitor.Item2.DDCisON = false;
                                Initialize2TypesMonitorInfo(false, CancellationToken.None);
                            }
                        }
                        else
                        {
                            _logs.DebugMsg("[VcpCorePlugin] [QueueTrigger] VcpCorePlugin Watcher0x02forStatusCheck 0x02 is null");
                            _logs.DebugMsg("[VcpCorePlugin] [QueueTrigger] VcpCorePlugin DDC/CI is disconnected");

                            if (monitor.Item1.DDCCIFail == 0)
                            {
                                _logs.DebugMsg("[VcpCorePlugin] [QueueTrigger] VcpCorePlugin VCP 0x02 First Fail Try Renew hPhysicalMonitor if possible");
                                RenewPhysicalMonitor(ref monitor.Item1);
                            }

                            monitor.Item1.DDCCIFail++;

                            if (monitor.Item1.DDCCIFail >= 3)
                            {
                                monitor.Item1.DDCCIFail = 3;

                                if (monitor.Item1.DDCisON)
                                {
                                    _logs.DebugMsg("[VcpCorePlugin] [QueueTrigger] VcpCorePlugin prepare DDC/CI broadcast on -> off");

                                    monitor.Item1.DDCisON = false;
                                    monitor.Item2.DDCisON = false;

                                    Initialize2TypesMonitorInfo(false, CancellationToken.None);

                                    DDCCIchangedEventArgs _ddcceventArg = new DDCCIchangedEventArgs();
                                    _ddcceventArg.DDCisON = false;
                                    _ddcceventArg.monitors = monitor.Item2.Clone();

                                    OnDDCCIStatuschanged(_ddcceventArg);
                                }
                            }
                        }
                    }
                    _logs.DebugMsg("[VcpCorePlugin] [QueueTrigger] Watching 0x02 for Status Check finish ...");
                }
                else
                    _logs.DebugMsg("[VcpCorePlugin] [QueueTrigger] Lose all monitors, ignore requested ...");
            }
            catch (Exception ex)
            {
                _logs.DebugMsg("[VcpCorePlugin] [QueueTrigger] Watching 0x02 for Status Check ex: " + ex.Message);
            }
        }

        private void Watcher0x52_()
        {
            try
            {
                _logs.DebugMsg("[VcpCorePlugin] [QueueTrigger] Watching 0x52 Let's go ...");

                if (IsOutInitialize && _AllInfoMonitors_Mix.Count > 0)
                {
                    bool rc = false;
                    object object_0x02 = null;
                    object object_0x52 = null;

                    for (int count = 0; ((count < _AllInfoMonitors_Mix.Count) && IsOutInitialize); count++)    //foreach (var monitor in _AllInfoMonitors_Mix)
                    {
                        var monitor = _AllInfoMonitors_Mix[count];

                        _logs.DebugMsg("[VcpCorePlugin] [QueueTrigger] VcpCorePlugin " + monitor.Item1.AliasDeviceName + " Watching 0x52");

                        if (monitor.Item1.DDCisON)
                        {
                            _logs.DebugMsg("[VcpCorePlugin] [QueueTrigger] VcpCorePlugin DDC/CI is connected");

                            object_0x02 = Get_VCPCapability(monitor.Item1, 0x02, 0, IsOutInitialize);

                            if (object_0x02 != null)
                            {
                                monitor.Item1.DDCCIFail = 0;

                                var FailTimes = 0;
                                while ((object_0x02 != null) && (((uint)object_0x02) != 1) && (FailTimes < 5) && IsOutInitialize)
                                {
                                    _logs.DebugMsg("[VcpCorePlugin] [QueueTrigger] VcpCorePlugin VCP 0x02 is " + ((uint)object_0x02).ToString("X"));

                                    object_0x52 = Get_VCPCapability(monitor.Item1, 0x52, 0, IsOutInitialize);

                                    if (object_0x52 != null)
                                    {
                                        _logs.DebugMsg("[VcpCorePlugin] [QueueTrigger] VcpCorePlugin VCP 0x52 is " + ((uint)object_0x52).ToString("X"));

                                        var tmp = Get_VCPCapability(monitor.Item1, Convert.ToByte(object_0x52), 0, IsOutInitialize);

                                        if (tmp != null)
                                        {
                                            _logs.DebugMsg("[VcpCorePlugin] [QueueTrigger] VcpCorePlugin VCP " + Convert.ToUInt32(object_0x52).ToString("X") + " is " + Convert.ToUInt32(tmp).ToString());

                                            if (((uint)object_0x52).ToString("X").ToUpper(CultureInfo.InvariantCulture).Equals("E2"))
                                            {
                                                uint val = (Convert.ToUInt32(tmp) & 0XFFFF);
                                                string valstring = val.ToString("X2");
                                                int pos = valstring.Length - 2;
                                                string rc_str = valstring.Substring(pos);

                                                if (!string.IsNullOrWhiteSpace(rc_str))
                                                {
                                                    _CacheTable.SetToCacheTable(monitor.Item1, Convert.ToByte(object_0x52), val);

                                                    _logs.DebugMsg("[VcpCorePlugin] [QueueTrigger]~~~ 0x52 ctr code is " + ((uint)object_0x52).ToString("X"));
                                                    _logs.DebugMsg("[VcpCorePlugin] [QueueTrigger]~~~ 0x52 ctr code value is " + NodeFormatter.FormatVCP_E2(rc_str.ToLower(CultureInfo.InvariantCulture)));

                                                    VCPchangedEventArgs _VCPchangedEventArgs = new VCPchangedEventArgs();
                                                    _VCPchangedEventArgs.vcpcode = ((uint)object_0x52).ToString("X");
                                                    _VCPchangedEventArgs.value = NodeFormatter.FormatVCP_E2(rc_str.ToLower(CultureInfo.InvariantCulture));
                                                    _VCPchangedEventArgs.monitor = monitor.Item2.Clone();

                                                    OnVCPchanged(_VCPchangedEventArgs);
                                                }
                                            }
                                            else if (((uint)object_0x52).ToString("X").ToUpper(CultureInfo.InvariantCulture).Equals("60"))
                                            {
                                                string rstring = string.Empty;
                                                uint val = Convert.ToUInt32(tmp) & 0xFF;
                                                string rc_str = val.ToString("X2");

                                                if (!string.IsNullOrWhiteSpace(rc_str))
                                                {
                                                    List<InputSourceObject> r = new List<InputSourceObject>();
                                                    var R = _CacheTable.GetFromCacheTable(monitor.Item1, "inputsourcelist".ToLower(CultureInfo.InvariantCulture));
                                                    if (R != null)
                                                        r.AddRange(R as List<InputSourceObject>);

                                                    var n = NodeFormatter.FormatVCP_60(rc_str.ToLower(CultureInfo.InvariantCulture));

                                                    bool rv = false;
                                                    if (r.Count > 0)
                                                    {
                                                        foreach (InputSourceObject t in r)
                                                        {
                                                            if (n.Equals(t.Name))
                                                            {
                                                                rv = true;
                                                                break;
                                                            }
                                                        }
                                                    }

                                                    if (rv)
                                                    {
                                                        monitor.Item1.inputSource = n;
                                                        monitor.Item2.inputSource = n;
                                                        rstring = n;
                                                    }
                                                    else
                                                    {
                                                        string rs = System.Text.RegularExpressions.Regex.Replace(n, @"\d", string.Empty);
                                                        monitor.Item1.inputSource = rs;
                                                        monitor.Item2.inputSource = rs;
                                                        rstring = rs;
                                                    }

                                                    Initialize2TypesMonitorInfo(false, CancellationToken.None);

                                                    _logs.DebugMsg("[VcpCorePlugin] [QueueTrigger]~~~ 0x52 ctr code is input select");
                                                    _logs.DebugMsg("[VcpCorePlugin] [QueueTrigger]~~~ 0x52 ctr code value is " + rstring);

                                                    //----------------------------------------------------------------------

                                                    VCPchangedEventArgs _VCPchangedEventArgsII = new VCPchangedEventArgs();
                                                    _VCPchangedEventArgsII.vcpcode = "input select";
                                                    _VCPchangedEventArgsII.value = rstring;
                                                    _VCPchangedEventArgsII.monitor = monitor.Item2.Clone();

                                                    OnVCPchanged(_VCPchangedEventArgsII);
                                                }
                                            }
                                            else
                                            {
                                                _CacheTable.SetToCacheTable(monitor.Item1, Convert.ToByte(object_0x52), (Convert.ToUInt32(tmp)));

                                                _logs.DebugMsg("[VcpCorePlugin] [QueueTrigger]~~~ 0x52 ctr code is " + ((uint)object_0x52).ToString("X"));
                                                _logs.DebugMsg("[VcpCorePlugin] [QueueTrigger]~~~ 0x52 ctr code value is " + ((uint)tmp).ToString());

                                                VCPchangedEventArgs _VCPchangedEventArgs = new VCPchangedEventArgs();
                                                _VCPchangedEventArgs.vcpcode = ((uint)object_0x52).ToString("X");
                                                _VCPchangedEventArgs.value = ((uint)tmp).ToString();
                                                _VCPchangedEventArgs.monitor = monitor.Item2.Clone();

                                                OnVCPchanged(_VCPchangedEventArgs);
                                            }
                                        }
                                        else
                                            _logs.DebugMsg("[VcpCorePlugin] [QueueTrigger] Get VCP " + Convert.ToUInt32(object_0x52).ToString("X") + " is null");
                                    }
                                    else
                                        _logs.DebugMsg("[VcpCorePlugin] [QueueTrigger] 0x52 is null");

                                    rc = Set_VCPCapability(monitor.Item1, 0x02, 0x01, IsOutInitialize);

                                    _logs.DebugMsg("[VcpCorePlugin] [QueueTrigger] VcpCorePlugin set VCP 0x02 to 1 : " + (rc ? "Sucess" : "Fail"));

                                    if (rc) FailTimes = 0;
                                    else FailTimes++;

                                    _logs.DebugMsg("[VcpCorePlugin] [QueueTrigger] VcpCorePlugin FailTimes : " + FailTimes.ToString());

                                    object_0x02 = Get_VCPCapability(monitor.Item1, 0x02, 0, IsOutInitialize);
                                }
                            }
                            else
                            {
                                _logs.DebugMsg("[VcpCorePlugin] [QueueTrigger] VcpCorePlugin VCP 0x02 is null");
                                _logs.DebugMsg("[VcpCorePlugin] [QueueTrigger] VcpCorePlugin DDC/CI is disconnected");

                                if (monitor.Item1.DDCCIFail == 0)
                                {
                                    _logs.DebugMsg("[VcpCorePlugin] [QueueTrigger] VcpCorePlugin VCP 0x02 First Fail Try Renew hPhysicalMonitor if possible");
                                    RenewPhysicalMonitor(ref monitor.Item1);
                                }

                                monitor.Item1.DDCCIFail++;

                                if (monitor.Item1.DDCCIFail >= 3)
                                {
                                    monitor.Item1.DDCCIFail = 3;

                                    if (monitor.Item1.DDCisON)
                                    {
                                        _logs.DebugMsg("[VcpCorePlugin] [QueueTrigger] VcpCorePlugin prepare DDC/CI broadcast on -> off");

                                        monitor.Item1.DDCisON = false;
                                        monitor.Item2.DDCisON = false;

                                        Initialize2TypesMonitorInfo(false, CancellationToken.None);

                                        DDCCIchangedEventArgs _ddcceventArg = new DDCCIchangedEventArgs();
                                        _ddcceventArg.DDCisON = false;
                                        _ddcceventArg.monitors = monitor.Item2.Clone();

                                        OnDDCCIStatuschanged(_ddcceventArg);
                                    }
                                }
                            }
                        }
                        else
                            _logs.DebugMsg("[VcpCorePlugin] [QueueTrigger] VcpCorePlugin DDC/CI is disconnected");
                    }

                    _logs.DebugMsg("[VcpCorePlugin] [QueueTrigger] Watching 0x52 finish ...");
                }
                else
                    _logs.DebugMsg("[VcpCorePlugin] [QueueTrigger] Lose all monitors, ignore requested ...");
            }
            catch (Exception ex)
            {
                _logs.DebugMsg("[VcpCorePlugin] [QueueTrigger] Watcher0x52_ ex: " + ex.Message);
            }
        }

        protected virtual void OnVCPchanged(VCPchangedEventArgs e)
        {
            _logs.DebugMsg("[VcpCorePlugin] VcpCorePlugin Broadcast OnVCPchanged ...");

            //VCPchanged?.Invoke(this, e);
            EventHandler<VCPchangedEventArgs> handler = VCPchanged;
            if (handler != null)
                Task.Run(() => handler.Invoke(this, e)).ConfigureAwait(false);
        }

        protected virtual void OnMonitorinfoUpdatechanged(MonitorinfoUpdateEventArgs e)
        {
            _logs.DebugMsg("[VcpCorePlugin] VcpCorePlugin Broadcast MonitorinfoUpdatechanged ...");

            //MonitorUpdatechanged?.Invoke(this, e);
            EventHandler<MonitorinfoUpdateEventArgs> handler = MonitorinfoUpdated;
            if (handler != null)
                Task.Run(() => handler.Invoke(this, e)).ConfigureAwait(false);
        }

        protected virtual void OnDisplaychanged(DisplaychangedEventArgs e)
        {
            _logs.DebugMsg("[VcpCorePlugin] VcpCorePlugin Broadcast OnDisplaychanged ...");

            //Displaychanged?.Invoke(this, e);
            EventHandler<DisplaychangedEventArgs> handler = Displaychanged;
            if (handler != null)
                Task.Run(() => handler.Invoke(this, e)).ConfigureAwait(false);
        }

        protected virtual void OnDDCCIStatuschanged(DDCCIchangedEventArgs e)
        {
            _logs.DebugMsg("[VcpCorePlugin] VcpCorePlugin Broadcast OnDDCCIStatuschanged ...");

            //DDCCIStatuschanged?.Invoke(this, e);
            EventHandler<DDCCIchangedEventArgs> handler = DDCCIStatuschanged;
            if (handler != null)
                Task.Run(() => handler.Invoke(this, e)).ConfigureAwait(false);
        }

        private void InitializeMonitorsList(bool IsFromReGet, CancellationToken token)
        {
            Guid TheadGUID = Guid.NewGuid();
            {
                try
                {
                    Interlocked.Add(ref _InitialThreadCounter, 1);
                    _logs.DebugMsg($"[VcpCorePlugin] _InitialThreadCounter count : ({_InitialThreadCounter}) when InitializeMonitorsList start");

                    _logs.DebugMsg($"[VcpCorePlugin] {TheadGUID} Wait ...");
                    _LockerSemaphoreSlim.Wait(token);
                    _logs.DebugMsg($"[VcpCorePlugin] {TheadGUID} Enter ...");

                    if (_pauseEvent.WaitOne(0))
                    {
                        _logs.DebugMsg("[VcpCorePlugin] _pauseEvent.Reset() when InitializeMonitorsList start");
                        _pauseEvent.Reset();
                    }

                    if (_CacheTimer.Enabled) _CacheTimer.Stop();
                    if (_StatusTimer.Enabled) _StatusTimer.Stop();

                    _AllInfoMonitors = new List<MonitorInfo_complex>();
                    _AllInfoMonitors_Mix = new List<(MonitorInfo_complex, MonitorInfo)>();

                    while (!_TaskQueue.IsEmpty())
                    {
                        if (_TaskQueueExecutor.IsBusy) _TaskQueueExecutor.CancelAsync();
                        else
                        {
                            _TaskQueue = new TaskLockQueue<ParameterType>();
                            _TaskQueueResult = new ResultLockPool();
                        }
                        _CancelhashSet.Clear();
                    }

                    _AddSignalfor0X52 = 0;
                    _AddSignalforStatusCheck = 0;

                    try
                    {
                        if (_cancellationTokenSource != null)
                        {
                            _logs.DebugMsg("[VcpCorePlugin] _cancellationTokenSource trigger cancel ...");
                            _cancellationTokenSource.Cancel();
                        }
                    }
                    catch (TaskCanceledException)
                    {
                        _logs.DebugMsg("[DeviceMangerPlugin] _cancellationTokenSource trigger cancel cancellation happened ...");
                        _cancellationTokenSource.Dispose();
                    }
                    catch (OperationCanceledException)
                    {
                        _logs.DebugMsg("[DeviceMangerPlugin] _cancellationTokenSource trigger cancel cancellation happened ...");
                        _cancellationTokenSource.Dispose();
                    }
                    catch (Exception ex)
                    {
                        _logs.DebugMsg($"[DeviceMangerPlugin] _cancellationTokenSource ...there is an exception-- ({ex.Message})");
                        _cancellationTokenSource.Dispose();
                    }
                    finally
                    {
                        Task T = null;
                        try
                        {
                            if (!token.Equals(CancellationToken.None))
                                _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(token);
                            else
                                _cancellationTokenSource = new CancellationTokenSource();

                            var TokenNew = _cancellationTokenSource.Token;

                            _logs.DebugMsg("[VcpCorePlugin] VcpCorePlugin into InitializeMonitorsList ...");

                            var monitors = _GetMonitors(TokenNew);

                            TokenNew.ThrowIfCancellationRequested();  //Extra Check IfCancellationRequested

                            if (monitors.Count < 1)
                            {
                                _logs.DebugMsg("[VcpCorePlugin] (mos.Count = 0) ...");

                                _AllInfoMonitors = new List<MonitorInfo_complex>();
                                _AllInfoMonitors_Mix = new List<(MonitorInfo_complex, MonitorInfo)>();

                                TokenNew.ThrowIfCancellationRequested();  //Extra Check IfCancellationRequested

                                if (_CoWorkSignal < 1)
                                {
                                    Interlocked.Add(ref _CoWorkSignal, 1);
                                    _logs.DebugMsg($"[VcpCorePlugin] *** Monitor-Retrier set _CoWorkSignal +1 : {_CoWorkSignal}");
                                }

                                _logs.DebugMsg($"[VcpCorePlugin] *** Monitor-Retrier now _CoWorkSignal : {_CoWorkSignal}");

                                Interlocked.Add(ref _InitialThreadCounter, 1);

                                _logs.DebugMsg($"[VcpCorePlugin] _InitialThreadCounter count : ({_InitialThreadCounter}) when mos=0 and Task run");

                                T = Task.Run(() => DoMonitorDisplayChanged(monitors, true, TokenNew));
                            }
                            else
                            {
                                _logs.DebugMsg("[VcpCorePlugin] (mos.Count > 0) ...");

                                //-------------------------------------------

                                TokenNew.ThrowIfCancellationRequested();  //Extra Check IfCancellationRequested

                                if (_CoWorkSignal < 1)
                                {
                                    Interlocked.Add(ref _CoWorkSignal, 1);
                                    _logs.DebugMsg($"[VcpCorePlugin] *** Doing-Extent-Detect set _CoWorkSignal +1 : {_CoWorkSignal}");
                                }

                                _logs.DebugMsg($"[VcpCorePlugin] *** Doing-Extent-Detect now _CoWorkSignal: {_CoWorkSignal}");

                                Interlocked.Add(ref _InitialThreadCounter, 1);

                                _logs.DebugMsg($"[VcpCorePlugin] _InitialThreadCounter count : ({_InitialThreadCounter}) when mos>0 and Task run");

                                T = Task.Run(() => DoMonitorDisplayChanged(monitors, false, TokenNew));

                                //-------------------------------------------

                                TokenNew.ThrowIfCancellationRequested();
                                _AllInfoMonitors = new List<MonitorInfo_complex>(monitors);

                                TokenNew.ThrowIfCancellationRequested();
                                Initialize2TypesMonitorInfo(true, TokenNew);

                                _logs.DebugMsg("[VcpCorePlugin] InitializeMonitorsList _AllInfoMonitors_Mix.Count is " + _AllInfoMonitors_Mix.Count);

                                TokenNew.ThrowIfCancellationRequested();
                                Initialize0x52toEmpty();
                            }
                        }
                        catch (TaskCanceledException)
                        {
                            _logs.DebugMsg("[VcpCorePlugin] InitializeMonitorsList TokenNew cancellation happened...");
                            _logs.DebugMsg($"[VcpCorePlugin] {TheadGUID} TaskCanceledException ...");
                        }
                        catch (OperationCanceledException)
                        {
                            _logs.DebugMsg("[VcpCorePlugin] InitializeMonitorsList TokenNew cancellation happened...");
                            _logs.DebugMsg($"[VcpCorePlugin] {TheadGUID} OperationCanceledException ...");
                        }
                        catch (Exception e)
                        {
                            _logs.DebugMsg($"[VcpCorePlugin] InitializeMonitorsList...there is an exception-- ({e.Message})");
                            _logs.DebugMsg($"[VcpCorePlugin] {TheadGUID} Exception ...");
                        }
                        finally
                        {
                            _logs.DebugMsg($"[VcpCorePlugin] {TheadGUID} Ready Exit ...");

                            Interlocked.Add(ref _InitialThreadCounter, -1);
                            _logs.DebugMsg($"[VcpCorePlugin] _InitialThreadCounter count : ({_InitialThreadCounter}) when InitializeMonitorsList finished and in finally");

                            if (!IsFromReGet)
                            {
                                DisplaychangedEventArgs _displaychangedEventArgss = new DisplaychangedEventArgs()
                                {
                                    count = _AllInfoMonitors_Mix.Count,
                                    monitors = _AllInfoMonitors_Mix.Select(x => x.Item2).ToList(),
                                };
                                OnDisplaychanged(_displaychangedEventArgss);
                            }

                            if (_InitialThreadCounter < 1)
                            {
                                if (!(_pauseEvent.WaitOne(0)))
                                {
                                    _logs.DebugMsg("[VcpCorePlugin] _pauseEvent.Set() when InitializeMonitorsList finished and in finally");
                                    _pauseEvent.Set();
                                }

                                if (!_CacheTimer.Enabled) _CacheTimer.Start();
                                if (!_StatusTimer.Enabled) _StatusTimer.Start();
                            }

                            if (_CoWorkSignal < 1 && T is null)
                            {
                                _logs.DebugMsg($"[VcpCorePlugin] InitializeMonitorsList finished and in finally _CoWorkSignal: {_CoWorkSignal}");

                                if (_LockerSemaphoreSlim.CurrentCount < 1)
                                {
                                    _logs.DebugMsg("[VcpCorePlugin] _LockerSemaphoreSlim.Release() when InitializeMonitorsList finished and in finally");
                                    _LockerSemaphoreSlim.Release();
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logs.DebugMsg($"[VcpCorePlugin] III_InitializeMonitorsList...there is an exception-- ({ex.Message})");
                }
            }

            void DoMonitorDisplayChanged(List<MonitorInfo_complex> monitors, bool IsEqualO, CancellationToken TokenNew)
            {
                string Who = IsEqualO ? @"Monitor-Retrier" : @"Doing-Extent-Detect";
                string MathematicalSymbols = IsEqualO ? @"=" : @">";

                try
                {
                    _logs.DebugMsg($"[VcpCorePlugin] *** ( mos.Count {MathematicalSymbols} 0 ) ...");
                    _logs.DebugMsg($"[VcpCorePlugin] *** {Who} start ...");

                    var origan = monitors.ToList();

                    int count = 0;
                    while (count < 10 && (!TokenNew.IsCancellationRequested))
                    {
                        Thread.Sleep(1500);

                        TokenNew.ThrowIfCancellationRequested();  //Extra Check IfCancellationRequested

                        List<MonitorInfo_complex> mos = _GetMonitors(TokenNew);

                        TokenNew.ThrowIfCancellationRequested();  //Extra Check IfCancellationRequested

                        if (mos.Count > 0)
                        {
                            if ((origan.Count == mos.Count))
                            {
                                var IsDiff = mos.ExceptBy(origan.Select(x => x.edid), x => x.edid).Any();

                                if (!IsDiff)
                                {
                                    _logs.DebugMsg($"[VcpCorePlugin] *** {Who} No Except ...");
                                    count++;
                                    continue;
                                }
                            }

                            origan = mos.ToList();

                            TokenNew.ThrowIfCancellationRequested();  //Extra Check IfCancellationRequested

                            if (_pauseEvent.WaitOne(0))
                            {
                                _logs.DebugMsg($"[VcpCorePlugin] _pauseEvent.Reset() when mos {MathematicalSymbols} 0 and Task run");
                                _pauseEvent.Reset();
                            }

                            if (_CacheTimer.Enabled) _CacheTimer.Stop();
                            if (_StatusTimer.Enabled) _StatusTimer.Stop();

                            while (!_TaskQueue.IsEmpty() && (!TokenNew.IsCancellationRequested))
                            {
                                if (_TaskQueueExecutor.IsBusy) _TaskQueueExecutor.CancelAsync();
                                else
                                {
                                    _TaskQueue = new TaskLockQueue<ParameterType>();
                                    _TaskQueueResult = new ResultLockPool();
                                }
                                _CancelhashSet.Clear();

                                TokenNew.ThrowIfCancellationRequested();  //Extra Check IfCancellationRequested
                            }

                            _AllInfoMonitors = mos.ToList();
                            _logs.DebugMsg($"[VcpCorePlugin] *** {Who} Monitors.count is " + mos.Count);
                            TokenNew.ThrowIfCancellationRequested();  //Extra Check IfCancellationRequested

                            Initialize2TypesMonitorInfo(true, TokenNew);
                            _logs.DebugMsg($"[VcpCorePlugin] *** {Who} _AllInfoMonitors_Mix.Count is " + _AllInfoMonitors_Mix.Count);
                            TokenNew.ThrowIfCancellationRequested();  //Extra Check IfCancellationRequested

                            Initialize0x52toEmpty();
                            TokenNew.ThrowIfCancellationRequested();  //Extra Check IfCancellationRequested

                            //------------------------------------------------------------------------------------------------//
                            DisplaychangedEventArgs _displaychangedEventArgss = new DisplaychangedEventArgs()
                            {
                                count = _AllInfoMonitors_Mix.Count,
                                monitors = new List<MonitorInfo>(_AllInfoMonitors_Mix.Select(x => x.Item2).ToList()),
                            };
                            OnDisplaychanged(_displaychangedEventArgss);
                            //------------------------------------------------------------------------------------------------//

                            TokenNew.ThrowIfCancellationRequested();  //Extra Check IfCancellationRequested
                        }

                        count++;
                    }
                }
                catch (TaskCanceledException)
                {
                    _logs.DebugMsg($"[VcpCorePlugin] {Who} into TaskCanceledException ...");
                }
                catch (OperationCanceledException)
                {
                    _logs.DebugMsg($"[VcpCorePlugin] {Who} into OperationCanceledException ...");
                }
                catch (Exception ex)
                {
                    _logs.DebugMsg($"[VcpCorePlugin] {Who} into catch {ex.Message} ...");
                }
                finally
                {
                    if (_CoWorkSignal > 0)
                    {
                        Interlocked.Add(ref _CoWorkSignal, -1);
                        _logs.DebugMsg($"[VcpCorePlugin] *** {Who} set _CoWorkSignal -1 : {_CoWorkSignal}");
                    }

                    _logs.DebugMsg($"[VcpCorePlugin] *** {Who} finally now _CoWorkSignal : {_CoWorkSignal}");

                    Interlocked.Add(ref _InitialThreadCounter, -1);
                    _logs.DebugMsg($"[VcpCorePlugin] _InitialThreadCounter count : ({_InitialThreadCounter}) when mos.Count {MathematicalSymbols} 0 and Task run in finally");

                    if (_InitialThreadCounter < 1)
                    {
                        if (!(_pauseEvent.WaitOne(0)))
                        {
                            _logs.DebugMsg($"[VcpCorePlugin] _pauseEvent.Set() when mos.Count {MathematicalSymbols} 0 and Task run in finally");
                            _pauseEvent.Set();
                        }

                        if (!_CacheTimer.Enabled) _CacheTimer.Start();
                        if (!_StatusTimer.Enabled) _StatusTimer.Start();
                    }

                    if (_LockerSemaphoreSlim.CurrentCount < 1)
                    {
                        _logs.DebugMsg($"[VcpCorePlugin] _LockerSemaphoreSlim.Release() when mos.Count {MathematicalSymbols} 0 and Task run in finally");
                        _LockerSemaphoreSlim.Release();
                    }
                }
            }
        }

        private void Initialize2TypesMonitorInfo(bool forwardMode, CancellationToken token)
        {
            if (forwardMode)
            {
                _logs.DebugMsg("[VcpCorePlugin] Initialize2TypesMonitorInfo Let's go (forwardMode=true) ...");

                if (_AllInfoMonitors.Count > 0)
                {
                    _AllInfoMonitors_Mix = new List<(MonitorInfo_complex, MonitorInfo)>();

                    for (int i = 0; (i < _AllInfoMonitors.Count && (!token.IsCancellationRequested)); i++)
                    {
                        var MonitorInfoX = _AllInfoMonitors[i];

                        MonitorInfo monitorInfo = new MonitorInfo()
                        {
                            AliasDeviceName = MonitorInfoX.AliasDeviceName,
                            IsDellMonitor = MonitorInfoX.IsDellMonitor,
                            Index = MonitorInfoX.Index,
                            CapabilityString = MonitorInfoX.CapabilityString,
                            DDCisON = MonitorInfoX.DDCisON,
                            DisplayName = MonitorInfoX.DisplayName,
                            edid = MonitorInfoX.edid,
                            FwVersion = MonitorInfoX.FwVersion,
                            inputSource = MonitorInfoX.inputSource,
                            inputCable = MonitorInfoX.inputCable,
                            CapabilityDic = MonitorInfoX.CapabilityDic,
                            modelName = MonitorInfoX.modelName,
                            series = MonitorInfoX.series,
                            MarketingName = MonitorInfoX.MarketingName,
                            ImageFileName = MonitorInfoX.ImageFileName,
                            SupplierID = MonitorInfoX.SupplierID,
                            D_Ctrl = MonitorInfoX.D_Ctrl,
                            scalingFactor = MonitorInfoX.scalingFactor,
                        };

                        _AllInfoMonitors_Mix.Add((MonitorInfoX, monitorInfo));
                    }

                    if (token.IsCancellationRequested)
                        return;

                    InitializeCacheTable(token);

                    foreach (var Monitor in _AllInfoMonitors_Mix)
                    {
                        if (token.IsCancellationRequested)
                            return;

                        if (Monitor.Item1.DDCisON)
                        {
                            _ = GetVCPCapability_(Monitor.Item1, "inputsourcelist", 0);
                            var tmp = GetInputSource(Monitor.Item1);
                            Monitor.Item1.inputSource = tmp.Item2;
                            Monitor.Item1.inputCable = tmp.Item1;
                            Monitor.Item2.inputSource = tmp.Item2;
                            Monitor.Item2.inputCable = tmp.Item1;

                            if (string.IsNullOrWhiteSpace(tmp.Item1) || string.IsNullOrWhiteSpace(tmp.Item2))
                                _logs.DebugMsg("[VcpCorePlugin] Initialize2TypesMonitorInfo (forwardMode=true) " + Monitor.Item1.modelName + " inputSource or inputCable is null ...");
                            else
                                _logs.DebugMsg("[VcpCorePlugin] Initialize2TypesMonitorInfo (forwardMode=true) " + Monitor.Item1.modelName + " inputSource inputCable all pass ...");
                        }

                        //--------------------------------------------------------------
                        _logs.DebugMsg("//----------------Show MonitorInfo(forwardMode=true)------------//");
                        _logs.DebugMsg("[VcpCorePlugin] Show*** monitorInfo : ");
                        _logs.DebugMsg("[VcpCorePlugin] Show*** AliasDeviceName : " + Monitor.Item1.AliasDeviceName);
                        _logs.DebugMsg("[VcpCorePlugin] Show*** IsDellMonitor : " + Monitor.Item1.IsDellMonitor.ToString());
                        _logs.DebugMsg("[VcpCorePlugin] Show*** Index : " + Monitor.Item1.Index.ToString());
                        _logs.DebugMsg("[VcpCorePlugin] Show*** CapabilityString : " + Monitor.Item1.CapabilityString);
                        _logs.DebugMsg("[VcpCorePlugin] Show*** DDCisON : " + Monitor.Item1.DDCisON.ToString());
                        _logs.DebugMsg("[VcpCorePlugin] Show*** DisplayName : " + Monitor.Item1.DisplayName);
                        _logs.DebugMsg("[VcpCorePlugin] Show*** edid : " + Monitor.Item1.edid.Edid);
                        _logs.DebugMsg("[VcpCorePlugin] Show*** edid.ManufactureID : " + Monitor.Item1.edid.ManufactureID);
                        _logs.DebugMsg("[VcpCorePlugin] Show*** edid.PID : " + Monitor.Item1.edid.PID);
                        _logs.DebugMsg("[VcpCorePlugin] Show*** edid.VendorID : " + Monitor.Item1.edid.VendorID);
                        _logs.DebugMsg("[VcpCorePlugin] Show*** edid.Year : " + Monitor.Item1.edid.Year.ToString());
                        _logs.DebugMsg("[VcpCorePlugin] Show*** edid.Month : " + Monitor.Item1.edid.Month.ToString());
                        _logs.DebugMsg("[VcpCorePlugin] Show*** edid.Week : " + Monitor.Item1.edid.Week.ToString());
                        _logs.DebugMsg("[VcpCorePlugin] Show*** edid.ModelName : " + Monitor.Item1.edid.ModelName);
                        _logs.DebugMsg("[VcpCorePlugin] Show*** edid.EdidVersion : " + Monitor.Item1.edid.EdidVersion);
                        _logs.DebugMsg("[VcpCorePlugin] Show*** edid.VideoInputType : " + Monitor.Item1.edid.VideoInputType);
                        _logs.DebugMsg("[VcpCorePlugin] Show*** edid.Size : " + Monitor.Item1.edid.Size.ToString());
                        _logs.DebugMsg("[VcpCorePlugin] Show*** edid.ServiceTag : " + Monitor.Item1.edid.ServiceTag);
                        _logs.DebugMsg("[VcpCorePlugin] Show*** edid.SerialNumber : " + Monitor.Item1.edid.SerialNumber);
                        _logs.DebugMsg("[VcpCorePlugin] Show*** FwVersion : " + Monitor.Item1.FwVersion);
                        _logs.DebugMsg("[VcpCorePlugin] Show*** inputSource : " + Monitor.Item1.inputSource);
                        _logs.DebugMsg("[VcpCorePlugin] Show*** inputCable : " + Monitor.Item1.inputCable);
                        _logs.DebugMsg("[VcpCorePlugin] Show*** modelName : " + Monitor.Item1.modelName);
                        _logs.DebugMsg("[VcpCorePlugin] Show*** series : " + Monitor.Item1.series);
                        _logs.DebugMsg("[VcpCorePlugin] Show*** MarketingName : " + Monitor.Item1.MarketingName);
                        _logs.DebugMsg("[VcpCorePlugin] Show*** ImageFileName : " + Monitor.Item1.ImageFileName);
                        _logs.DebugMsg("[VcpCorePlugin] Show*** SupplierID : " + Monitor.Item1.SupplierID);
                        _logs.DebugMsg("[VcpCorePlugin] Show*** D_Ctrl : " + Monitor.Item1.D_Ctrl);
                        _logs.DebugMsg("[VcpCorePlugin] Show*** scalingFactor : " + Monitor.Item1.scalingFactor.ToString());
                        _logs.DebugMsg("//----------------Show END------------//");
                        //--------------------------------------------------------------
                    }

                    if (!token.IsCancellationRequested)
                        _AllInfoMonitors = new List<MonitorInfo_complex>(_AllInfoMonitors_Mix.Select(M => M.Item1).ToList());
                }
            }
            else
            {
                _logs.DebugMsg("[VcpCorePlugin] Initialize2TypesMonitorInfo Let's go (forwardMode=false) ...");

                if (_AllInfoMonitors_Mix.Count > 0 && IsOutInitialize && (!token.IsCancellationRequested))
                    _AllInfoMonitors = new List<MonitorInfo_complex>(_AllInfoMonitors_Mix.Select(M => M.Item1).ToList());
            }

            _logs.DebugMsg("[VcpCorePlugin] Initialize2TypesMonitorInfo finish : count => " + _AllInfoMonitors_Mix.Count);
        }

        private void InitializeCacheTable(CancellationToken token)
        {
            _logs.DebugMsg("[VcpCorePlugin] InitializeCacheTable Let's go ...");
            _CacheTable.InitializeCacheTable(_AllInfoMonitors_Mix, token);
        }

        //---------------------------------------------------

        private object GetVcp2Steps(MonitorInfo_complex monitor, byte ctr, uint checkMask)
        {
            try
            {
                int count = 0;
                object ro = null;

                do
                {
                    bool r = Set_VCPCapability(monitor, ctr, checkMask, false, false);

                    if (r)
                    {
                        ro = Get_VCPCapability(monitor, ctr, 0, false);
                        if (ro != null) { return ro; }
                    }

                    count += 1;
                    _logs.DebugMsg($"[VcpCorePlugin] GetVcp2Steps retry ({count})");
                    Thread.Sleep(1000);
                } while (count < 3);

                _logs.DebugMsg("[VcpCorePlugin] GetVcp2Steps return null");
                return null;
            }
            catch (Exception ex)
            {
                _logs.DebugMsg("[VcpCorePlugin] GetVcp2Steps into catch: " + ex.Message);
                return null;
            }
        }

        private byte TranslatorVCPctrCode(string str)
        {
            try
            {
                byte rc = default;

                if (VcpCodeList.VCPctr.ContainsKey(str))
                    VcpCodeList.VCPctr.TryGetValue(str, out rc);

                return rc;
            }
            catch (Exception ex)
            {
                _logs.DebugMsg("[VcpCorePlugin] TranslatorVCPctrCode into catch: " + ex.Message);
                return default;
            }
        }

        private uint? TranslatorVCPcategory(string category, string str)
        {
            try
            {
                uint? rc = default;

                if (category.Equals(@"USB-C Prioritization", StringComparison.OrdinalIgnoreCase))
                {
                    if (VcpCodeList.VCPF8.ContainsKey(str))
                    {
                        var s = VcpCodeList.VCPF8.TryGetValue(str, out uint rcc);
                        if (s) rc = rcc;
                    }
                }
                else if (category.Equals(@"Input Select", StringComparison.OrdinalIgnoreCase))
                {
                    var defaultValue = default(KeyValuePair<string, uint>);
                    var input = VcpCodeList.VCP60.FirstOrDefault(x => x.Key.ToLower(CultureInfo.InvariantCulture).Equals(str.ToLower(CultureInfo.InvariantCulture)));
                    if (!input.Equals(defaultValue))
                        rc = input.Value;
                }

                return rc;
            }
            catch (Exception ex)
            {
                _logs.DebugMsg("[VcpCorePlugin] TranslatorVCPcategory into catch: " + ex.Message);
                return default;
            }
        }

        private void OnCacheTimedRaise(System.Object source, System.Timers.ElapsedEventArgs e)
        {
            _logs.DebugMsg("[VcpCorePlugin] [Hook] OnCacheTimedRaise");

            if (IsOutInitialize && _AllInfoMonitors_Mix.Count > 0)
                Watcher0x52();
            else
                _logs.DebugMsg("[VcpCorePlugin] [OnCacheTimedRaise] No monitors to service or in Initialize");
        }

        private void OnStatusTimedRaise(System.Object source, System.Timers.ElapsedEventArgs e)
        {
            _logs.DebugMsg("[VcpCorePlugin] [Hook] OnStatusTimedRaise");

            if (IsOutInitialize && _AllInfoMonitors_Mix.Count > 0)
                Watcher0x02forStatusCheck();
            else
                _logs.DebugMsg("[VcpCorePlugin] [OnStatusTimedRaise] No monitors to service or in Initialize");
        }

        private void RenewPhysicalMonitor(ref MonitorInfo_complex monitor)
        {
            uint cPhysicalMonitors = 0;
            bool bSuccess = _GetNumberOfPhysicalMonitorsFromHMONITOR(monitor.hMonitor, ref cPhysicalMonitors);
            if (bSuccess)
            {
                PHYSICAL_MONITOR[] pPhysicalMonitors = new PHYSICAL_MONITOR[cPhysicalMonitors];
                bSuccess = _GetPhysicalMonitorsFromHMONITOR(monitor.hMonitor, cPhysicalMonitors, pPhysicalMonitors);
                if (bSuccess &&
                    pPhysicalMonitors.Length >= monitor.cPhysicalMonitors_index)
                {
                    monitor.hPhysicalMonitor = pPhysicalMonitors[monitor.cPhysicalMonitors_index].hPhysicalMonitor;
                }
            }
        }

        private List<MonitorInfo_complex> _GetMonitors(CancellationToken token)
        {
            _logs.DebugMsg("[VcpCorePlugin] VcpCorePlugin into _GetMonitors() ...");

            try
            {
                List<MonitorInfo_complex> monitors = new List<MonitorInfo_complex>();
                List<Screen> screenList = Screen.AllScreens.ToList();
                int MoIndexCounter = 0;
                uint count_reg = 0;   // REG monitor count
                uint monitorCounter = 0;  // _EnumDisplayMonitors monitor count

                try
                {
                    if (token.IsCancellationRequested || (!_EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, _Get_Monitors, IntPtr.Zero)))
                    {
                        _logs.DebugMsg("[VcpCorePlugin] _Get_Monitors() collection while _EnumDisplayMonitors false or token IsCancellationRequested ");
                        throw new Win32Exception(Marshal.GetLastWin32Error());
                    }

                    if (token.IsCancellationRequested)
                    {
                        _logs.DebugMsg("[VcpCorePlugin] _GetMonitors IsCancellationRequested True After _EnumDisplayMonitors");
                        return new List<MonitorInfo_complex>();
                    }

                    using (var regCount = Registry.LocalMachine.OpenSubKey("SYSTEM\\CurrentControlSet\\Services\\monitor\\Enum"))
                    {
                        _logs.DebugMsg("[VcpCorePlugin] _Get_Monitors() open regCount ...");
                        try
                        {
                            if (regCount != null)
                            {
                                count_reg = (uint)((int)regCount.GetValue("Count"));

                                _logs.DebugMsg("[VcpCorePlugin] _Get_Monitors() collection REG monitor count : " + count_reg.ToString());
                                _logs.DebugMsg("[VcpCorePlugin] _Get_Monitors() collection monitor count : " + monitorCounter.ToString());
                                _logs.DebugMsg("[VcpCorePlugin] _Get_Monitors() collection Screen count : " + screenList.Count.ToString());
                            }
                            else
                            {
                                _logs.DebugMsg("[VcpCorePlugin] _Get_Monitors() collection regCount is null");
                                _logs.DebugMsg("[VcpCorePlugin] _Get_Monitors() collection monitor count : " + monitorCounter.ToString());
                                _logs.DebugMsg("[VcpCorePlugin] _Get_Monitors() collection Screen count : " + screenList.Count.ToString());
                            }

                            _logs.DebugMsg("[VcpCorePlugin] VcpCorePlugin exit _GetMonitors() ...");

                            return monitors;
                        }
                        catch (Exception ex)
                        {
                            _logs.DebugMsg("[VcpCorePlugin] _Get_Monitors() collection REG monitor count exception: " + ex.Message);
                            return new List<MonitorInfo_complex>();
                        }
                        finally
                        {
                            if (regCount != null)
                            {
                                regCount.Close();
                                regCount.Dispose();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logs.DebugMsg("[VcpCorePlugin] _Get_Monitors() collection while _EnumDisplayMonitors exception: " + ex.Message);
                    return new List<MonitorInfo_complex>();
                }

                bool _Get_Monitors(IntPtr hMonitor, IntPtr hdcMonitor, ref Rectangle lprcMonitor, IntPtr dwData)
                {
                    try
                    {
                        token.ThrowIfCancellationRequested();  //*****EXTRA CHECK*****//

                        _logs.DebugMsg("[VcpCorePlugin] _Get_Monitors collection start ...");

                        screenList = new List<Screen>(Screen.AllScreens.ToList());
                        var info = new MonitorInfoEx();
                        _GetMonitorInfo(new HandleRef(null, hMonitor), info);
                        string DeviceName = new string(info.szDevice).Trim('\0');
                        //----
                        uint cPhysicalMonitors = 0;
                        bool bSuccess = _GetNumberOfPhysicalMonitorsFromHMONITOR(hMonitor, ref cPhysicalMonitors);
                        PHYSICAL_MONITOR[] pPhysicalMonitors = new PHYSICAL_MONITOR[cPhysicalMonitors];
                        bSuccess = _GetPhysicalMonitorsFromHMONITOR(hMonitor, cPhysicalMonitors, pPhysicalMonitors);
                        DISPLAY_DEVICE dd = new DISPLAY_DEVICE();
                        dd.cb = Marshal.SizeOf(dd);
                        //----
                        int realindex = -1;
                        for (uint jj = 0u; _EnumDisplayDevices(DeviceName, jj, ref dd, 0); jj++)
                        {
                            token.ThrowIfCancellationRequested();  //*****EXTRA CHECK*****//

                            if ((dd.StateFlags & DisplayDeviceStateFlags.AttachedToDesktop) == 0)   //EnumDisplaySettings can also be used to obtain the StateFlags member of the DISPLAY_DEVICE structure & the flag bit DISPLAY_DEVICE_ATTACHED_TO_DESKTOP to determine whether it is a valid desktop environment.
                            {
                                monitorCounter++;
                                continue;
                            }
                            else
                            {
                                realindex++;
                                monitorCounter++;
                            }

                            DEVMODE devmode = new DEVMODE();
                            devmode.dmSize = (short)Marshal.SizeOf(typeof(DEVMODE));
                            bool success = _EnumDisplaySettings(DeviceName, ENUM_CURRENT_SETTINGS, ref devmode);
                            MonitorInfo_complex _TargetMonitor = new MonitorInfo_complex();

                            _TargetMonitor.DisplayName = DeviceName;
                            _logs.DebugMsg($"[VcpCorePlugin] _Get_Monitors collection DisplayName: {_TargetMonitor.DisplayName}");
                            _TargetMonitor.hMonitor = hMonitor;
                            _TargetMonitor.pMonitorInfoEx = info;
                            _TargetMonitor.Handle = hdcMonitor;
                            _TargetMonitor.pDevmode = devmode;
                            _TargetMonitor.displaydevice = dd;
                            _TargetMonitor.cPhysicalMonitors_index = Convert.ToUInt32(realindex);
                            _TargetMonitor.hPhysicalMonitor = pPhysicalMonitors[realindex].hPhysicalMonitor;
                            _TargetMonitor.szPhysicalMonitorDescription = pPhysicalMonitors[realindex].szPhysicalMonitorDescription;
                            _TargetMonitor.ColorPresentDescription = new Dictionary<string, Dictionary<string, string>>();
                            _TargetMonitor.UnDefinedColorPreset = new List<string>();

                            EDID edid = new EDID();

                            bool blGetEdidPass = false;

                            int nCount = 0;
                            while (nCount < 2)
                            {
                                token.ThrowIfCancellationRequested();  //*****EXTRA CHECK*****//

                                Thread thread = new Thread(() =>
                                {
                                    blGetEdidPass = CommonFun.getEDID(dd.DeviceID, ref edid, _logs);
                                });

                                thread.Start();
                                thread.Join(); //wait for the thread to finish

                                if (blGetEdidPass)
                                    break;

                                nCount++;
                                _logs.DebugMsg("[VcpCorePlugin] _Get_Monitors collection Get EDID Retry " + nCount.ToString());
                                Thread.Sleep(1000 * nCount);
                            }

                            if (blGetEdidPass)
                            {
                                _logs.DebugMsg("[VcpCorePlugin] _Get_Monitors collection Get EDID Success");

                                if (!string.IsNullOrWhiteSpace(edid.ManufactureID))
                                {
                                    if (edid.ManufactureID.ToUpper(CultureInfo.InvariantCulture) == "DEL")
                                        _TargetMonitor.IsDellMonitor = true;

                                    _TargetMonitor.edid = edid;

                                    DISPLAYCONFIG_PATH_INFO getPathInfo;
                                    if (DisplayConfigLibrary.GetDisplayConfigPath(_TargetMonitor.edid, out getPathInfo))
                                        _TargetMonitor.pathInfoTarget = getPathInfo;

                                    if (_TargetMonitor.IsDellMonitor)
                                    {
                                        if (!string.IsNullOrWhiteSpace(dd.DeviceString))
                                        {
                                            if (string.Compare(dd.DeviceString, "Generic PnP Monitor", true) == 0)
                                            {
                                                if (!string.IsNullOrWhiteSpace(edid.ModelName))
                                                {
                                                    _TargetMonitor.AliasDeviceName = edid.ModelName;
                                                    _logs.DebugMsg($"[VcpCorePlugin] _Get_Monitors collection AliasDeviceName: {_TargetMonitor.AliasDeviceName}");
                                                }
                                            }
                                            else
                                            {
                                                _TargetMonitor.AliasDeviceName = dd.DeviceString;
                                                _logs.DebugMsg($"[VcpCorePlugin] _Get_Monitors collection AliasDeviceName: {_TargetMonitor.AliasDeviceName}");
                                            }
                                        }
                                        else
                                        {
                                            if (!string.IsNullOrWhiteSpace(edid.ModelName))
                                            {
                                                _TargetMonitor.AliasDeviceName = edid.ModelName;
                                                _logs.DebugMsg($"[VcpCorePlugin] _Get_Monitors collection AliasDeviceName: {_TargetMonitor.AliasDeviceName}");
                                            }
                                        }

                                        if (dd.DeviceID.ToUpper(CultureInfo.InvariantCulture).Contains("DEL"))
                                        {
                                            if (string.IsNullOrWhiteSpace(_TargetMonitor.AliasDeviceName))
                                                GetAliasDeviceName(dd.DeviceString, ref _TargetMonitor.AliasDeviceName);
                                            else
                                                GetAliasDeviceName(/*dd.DeviceString*/_TargetMonitor.AliasDeviceName, ref _TargetMonitor.AliasDeviceName);
                                        }
                                    }
                                    else
                                    {
                                        if (!string.IsNullOrWhiteSpace(edid.ModelName))
                                        {
                                            _TargetMonitor.AliasDeviceName = edid.ModelName;
                                            _logs.DebugMsg($"[VcpCorePlugin] _Get_Monitors collection AliasDeviceName: {_TargetMonitor.AliasDeviceName}");
                                        }
                                        else
                                        {
                                            _TargetMonitor.AliasDeviceName = dd.DeviceString;
                                            _logs.DebugMsg($"[VcpCorePlugin] _Get_Monitors collection AliasDeviceName: {_TargetMonitor.AliasDeviceName}");
                                        }

                                        if (_IsOnlyGetDellMontor)
                                            continue;
                                    }
                                }
                            }
                            else
                            {
                                _logs.DebugMsg("[VcpCorePlugin] _Get_Monitors collection Get EDID Fail");

                                if (_IsOnlyGetDellMontor)
                                    continue;
                            }

                            //--------------------------------------------------------------------

                            _TargetMonitor.modelName = _TargetMonitor.edid.ModelName.ToUpper(CultureInfo.InvariantCulture);
                            _logs.DebugMsg("[VcpCorePlugin] _Get_Monitors ModelName Get by EDID is " + _TargetMonitor.modelName);

                            bool IsSupportDisplay = CheckIsSupportDisplay(ref _TargetMonitor);
                            _logs.DebugMsg("[VcpCorePlugin] _Get_Monitors IsSupportDisplay first judge : " + IsSupportDisplay.ToString());

                            if ((string.IsNullOrWhiteSpace(_TargetMonitor.modelName)) || ((!string.IsNullOrWhiteSpace(_TargetMonitor.modelName)) && IsSupportDisplay))
                            {
                                int nRetryCount = 0;
                                do
                                {
                                    token.ThrowIfCancellationRequested();  //*****EXTRA CHECK*****//

                                    var ro = _CacheTable.GetFromCacheTable(new MonitorInfo_complex() { edid = _TargetMonitor.edid, AliasDeviceName = _TargetMonitor.AliasDeviceName }, "CapibilityString".ToLower(CultureInfo.InvariantCulture));
                                    if (ro != null)
                                    {
                                        _TargetMonitor.CapabilityString = ro.ToString();
                                        break;
                                    }
                                    else
                                    {
                                        _TargetMonitor.CapabilityString = GetCapabilities_String(in _TargetMonitor, token);
                                        if (!string.IsNullOrWhiteSpace(_TargetMonitor.CapabilityString))
                                            break;
                                    }

                                    nRetryCount++;
                                } while (_TargetMonitor.IsDellMonitor && (nRetryCount < 2));

                                if (string.IsNullOrWhiteSpace(_TargetMonitor.modelName))
                                {
                                    ITokenizer tokenizer = new CapabilitiesTokenizer();
                                    IParser parser = new CapabilitiesParser();
                                    var tokens = tokenizer.GetTokens(_TargetMonitor.CapabilityString);
                                    var node = parser.Parse(tokens);

                                    if (!string.IsNullOrWhiteSpace(_TargetMonitor.CapabilityString) && (node.Nodes.Count() > 0))
                                    {
                                        var modelNode = node.Nodes.RecursiveSelect(n => n.Nodes).Single(n => n.Value == "model");
                                        foreach (var _node in modelNode.Nodes)
                                            _TargetMonitor.modelName = _node.Value;
                                    }

                                    IsSupportDisplay = CheckIsSupportDisplay(ref _TargetMonitor);
                                    _logs.DebugMsg("[VcpCorePlugin] _Get_Monitors IsSupportDisplay second judge : " + IsSupportDisplay.ToString());
                                }
                            }

                            token.ThrowIfCancellationRequested();  //*****EXTRA CHECK*****//

                            if (IsSupportDisplay)
                            {
                                var dpiXProperty = typeof(SystemParameters).GetProperty("DpiX", BindingFlags.NonPublic | BindingFlags.Static);
                                var varX = (int)dpiXProperty.GetValue(null, null);
                                double dpiX = (double)varX / (double)96;
                                foreach (Screen screen in screenList)
                                {
                                    token.ThrowIfCancellationRequested();  //*****EXTRA CHECK*****//

                                    if (screen.DeviceName.ToUpper(CultureInfo.InvariantCulture).Equals(DeviceName.ToUpper(CultureInfo.InvariantCulture), StringComparison.OrdinalIgnoreCase))
                                        _TargetMonitor.scalingFactor = dpiX * Decimal.ToDouble(Math.Round(Decimal.Divide(devmode.dmPelsWidth, screen.Bounds.Width), 2));
                                }

                                _TargetMonitor.ColorPresetSupportList = new List<string>();
                                _TargetMonitor.CapabilityDic = new Dictionary<string, List<string>>();

                                if (!string.IsNullOrWhiteSpace(_TargetMonitor.CapabilityString))
                                {
                                    _logs.DebugMsg("[VcpCorePlugin] _Get_Monitors DDCisON true");

                                    _TargetMonitor.DDCisON = true;

                                    int nRetryCount = 0;
                                    int nRetryCount2 = 0;
                                    while (true)
                                    {
                                        token.ThrowIfCancellationRequested();  //*****EXTRA CHECK*****//

                                        if (GetAllCapabilityData(_TargetMonitor.CapabilityString, ref _TargetMonitor.CapabilityDic))
                                        {
                                            if (_TargetMonitor.IsDellMonitor)
                                            {
                                                List<string> e2List = new List<string>();
                                                bool blE2Ret = getE2SupportStrings(_TargetMonitor.CapabilityDic, out e2List);

                                                if (blE2Ret && (e2List.Count == 0))
                                                {
                                                    ;
                                                }
                                                else
                                                {
                                                    if (blE2Ret && (e2List.Count > 0))
                                                        _TargetMonitor.ColorPresetSupportList = GetColorPresetStrings(_TargetMonitor.CapabilityDic, e2List);

                                                    nRetryCount = 0;
                                                }
                                                _TargetMonitor.SmartHDRSupportList = GetSmartHdrStrings(_TargetMonitor.CapabilityDic);
                                            }
                                        }
                                        else
                                        {
                                            _TargetMonitor.CapabilityDic.Clear();
                                            nRetryCount++;
                                            _TargetMonitor.CapabilityString = GetCapabilities_String(in _TargetMonitor, token);
                                            if (nRetryCount >= 3)
                                                break;

                                            continue;
                                        }

                                        if (!GetAllColorPreset(_TargetMonitor.CapabilityString, ref _TargetMonitor.ColorPresentDescription, ref _TargetMonitor.UnDefinedColorPreset, _TargetMonitor.IsDellMonitor))
                                        {
                                            nRetryCount2++;
                                            _TargetMonitor.CapabilityString = GetCapabilities_String(in _TargetMonitor, token);

                                            if (nRetryCount2 >= 3)
                                                break;
                                        }
                                        else
                                        {
                                            if (!_TargetMonitor.IsDellMonitor)
                                            {
                                                if (_TargetMonitor.ColorPresentDescription.ContainsKey("14"))
                                                {
                                                    if (_TargetMonitor.ColorPresetSupportList.Count != 0)
                                                        _TargetMonitor.ColorPresetSupportList.Clear();

                                                    Dictionary<string, string> vcp14 = new Dictionary<string, string>();
                                                    bool TF_boolean = _TargetMonitor.ColorPresentDescription.TryGetValue("14", out vcp14);
                                                    if (TF_boolean)
                                                    {
                                                        foreach (string tmpkey1 in vcp14.Keys)
                                                        {
                                                            token.ThrowIfCancellationRequested();  //*****EXTRA CHECK*****//

                                                            _TargetMonitor.ColorPresetSupportList.Add(tmpkey1);
                                                        }
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                // It's Dell monitor but doesn't support E2, F0, DC feature, so we check 0x14 for Color Preset support list.
                                                if ((_TargetMonitor.ColorPresetSupportList.Count == 0) && _TargetMonitor.ColorPresentDescription.ContainsKey("14"))
                                                {
                                                    Dictionary<string, string> vcp14 = new Dictionary<string, string>();
                                                    bool TF_boolean = _TargetMonitor.ColorPresentDescription.TryGetValue("14", out vcp14);
                                                    if (TF_boolean)
                                                    {
                                                        foreach (string tmpkey1 in vcp14.Keys)
                                                        {
                                                            token.ThrowIfCancellationRequested();  //*****EXTRA CHECK*****//

                                                            _TargetMonitor.ColorPresetSupportList.Add(tmpkey1);
                                                        }
                                                    }
                                                }
                                            }
                                            break;
                                        }
                                    }

                                    token.ThrowIfCancellationRequested();  //*****EXTRA CHECK*****//

                                    var FwTmp = FwVersion(in _TargetMonitor, _TargetMonitor.modelName, token);
                                    _TargetMonitor.FwVersion = FwTmp.Item1;
                                    _TargetMonitor.D_Ctrl = FwTmp.Item2;
                                    _TargetMonitor.SupplierID = FwTmp.Item3;
                                }
                                else
                                {
                                    token.ThrowIfCancellationRequested();  //*****EXTRA CHECK*****//

                                    _logs.DebugMsg("[VcpCorePlugin] _Get_Monitors DDCisON false");
                                    _TargetMonitor.DDCisON = false;
                                }

                                token.ThrowIfCancellationRequested();  //*****EXTRA CHECK*****//

                                _logs.DebugMsg("[VcpCorePlugin] _Get_Monitors add " + _TargetMonitor.modelName + " into _AllMonitors");
                                _TargetMonitor.Index = MoIndexCounter;
                                monitors.Add(_TargetMonitor);
                                MoIndexCounter++;
                            }
                        }

                        return true;
                    }
                    catch (TaskCanceledException)
                    {
                        _logs.DebugMsg("[VcpCorePlugin] VcpCorePlugin _Get_Monitors() cancellation happened ...");
                        _logs.DebugMsg("[VcpCorePlugin] _Get_Monitors collection exception");
                        return false;
                    }
                    catch (OperationCanceledException)
                    {
                        _logs.DebugMsg("[VcpCorePlugin] VcpCorePlugin _Get_Monitors() cancellation happened ...");
                        _logs.DebugMsg("[VcpCorePlugin] _Get_Monitors collection exception");
                        return false;
                    }
                    catch (Exception ex)
                    {
                        _logs.DebugMsg("[VcpCorePlugin] _Get_Monitors() collection exception : " + ex.Message);
                        return false;
                    }
                }
            }
            catch (Exception e)
            {
                _logs.DebugMsg("[VcpCorePlugin] _GetMonitors() happened exception : " + e.Message);
                return new List<MonitorInfo_complex>();
            }
        }

        private bool getE2SupportStrings(Dictionary<string, List<string>> CapDic, out List<string> e2List)
        {
            e2List = new List<string>();

            try
            {
                bool blResult = false;
                //List<string> e2List = new List<string>();
                //e2List = new List<string>();

                List<string> value = null;
                if (!CapDic.TryGetValue("E2", out value))
                    return blResult;

                if (value == null)
                {
                    blResult = true;
                    return blResult;
                }

                foreach (var item in CapDic["E2"])
                {
                    blResult = true;
                    string strHex = $"0x{item}";

                    int hex;

                    if (strHex.StartsWith("0x") && int.TryParse(strHex.Substring(2), NumberStyles.AllowHexSpecifier, null, out hex))
                    {
                        List<string> getData = VcpCodeList.VCPE2.Where(kvp => kvp.Key == hex).Select(kvp => kvp.Value).ToList();

                        if (getData.Count > 0)
                        {
                            foreach (var s in getData)
                            {
                                if (!string.IsNullOrWhiteSpace(s))
                                    e2List.Add(s);
                            }
                        }
                    }
                }
                return blResult;
            }
            catch (Exception ex)
            {
                _logs.DebugMsg("[VcpCorePlugin] getE2SupportStrings into catch: " + ex.Message);
                return false;
            }
        }

        private List<string> GetSmartHdrStrings(Dictionary<string, List<string>> CapDic)
        {
            try
            {
                List<string> Newlist = new List<string>();

                List<string> value;
                string strF4 = "F4";
                if (!CapDic.TryGetValue(strF4, out value))
                    return Newlist;

                foreach (var item in CapDic["F4"])
                {
                    string strHex = $"0x{item}";
                    int hex;

                    if (strHex.StartsWith("0x") && int.TryParse(strHex.Substring(2), NumberStyles.AllowHexSpecifier, null, out hex))
                    {
                        List<string> getData = VcpCodeList.VCPF4.Where(kvp => kvp.Key == hex).Select(kvp => kvp.Value).ToList();

                        if (getData.Count > 0)
                        {
                            foreach (var s in getData)
                            {
                                if (!string.IsNullOrWhiteSpace(s))
                                    Newlist.Add(s);
                            }
                        }
                    }
                }

                return Newlist;
            }
            catch (Exception ex)
            {
                _logs.DebugMsg("[VcpCorePlugin] GetSmartHdrStrings into catch: " + ex.Message);
                return new List<string>();
            }
        }

        private (string, string) GetInputSource(MonitorInfo_complex monitorInfo_)
        {
            try
            {
                string rc_H = string.Empty;
                string rc_L = string.Empty;
                int count = 0;
                do
                {
                    var value = Get_VCPCapability(monitorInfo_, 0x60, 0, IsOutInitialize);

                    if (value != null)
                    {
                        uint val_H = (Convert.ToUInt32(value) >> 8) & 0XFF;
                        uint val_L = Convert.ToUInt32(value) & 0XFF;

                        rc_H = val_H.ToString("X2");
                        rc_L = val_L.ToString("X2");

                        if (!string.IsNullOrWhiteSpace(rc_H) && !string.IsNullOrWhiteSpace(rc_L))
                        {
                            List<InputSourceObject> r = new List<InputSourceObject>();
                            var R = _CacheTable.GetFromCacheTable(monitorInfo_, "inputsourcelist".ToLower(CultureInfo.InvariantCulture));
                            if (R != null)
                                r.AddRange(R as List<InputSourceObject>);

                            var n_H = NodeFormatter.FormatVCP_60(rc_H.ToLower(CultureInfo.InvariantCulture));
                            var n_L = NodeFormatter.FormatVCP_60(rc_L.ToLower(CultureInfo.InvariantCulture));

                            if (string.IsNullOrWhiteSpace(n_H))
                                n_H = n_L;

                            bool rv_H = false;
                            bool rv_L = false;

                            if (r.Count > 0)
                            {
                                foreach (InputSourceObject t in r)
                                {
                                    if (n_H.Equals(t.Name))
                                        rv_H = true;

                                    if (n_L.Equals(t.Name))
                                        rv_L = true;

                                    if (rv_H && rv_L)
                                        break;
                                }
                            }

                            n_H = ((rv_H) ? n_H : (System.Text.RegularExpressions.Regex.Replace(n_H, @"\d", string.Empty))) ?? "Unknown";
                            n_L = ((rv_L) ? n_L : (System.Text.RegularExpressions.Regex.Replace(n_L, @"\d", string.Empty))) ?? "Unknown";

                            return (n_H, n_L);
                        }
                    }

                    count++;
                    _logs.DebugMsg($"[VcpCorePlugin] GetInputSource retry ({count})");
                    Thread.Sleep(1000);
                } while (count < 3);

                _logs.DebugMsg("[VcpCorePlugin] GetInputSource return string.Empty");
                return (string.Empty, string.Empty);
            }
            catch (Exception ex)
            {
                _logs.DebugMsg("[VcpCorePlugin] GetInputSource into catch: " + ex.Message);
                return (string.Empty, string.Empty);
            }
        }

        private bool Set_VCPCapability(MonitorInfo_complex monitorInfoX, byte code, uint val, bool retry = true, bool IsNotifyVcpCanghed = true)
        {
            try
            {
                Monitor.Enter(SetVCPLock);

                if (_AllInfoMonitors_Mix.Count > 0 && (_AllInfoMonitors_Mix.Exists(M => M.Item1.edid.Equals(monitorInfoX.edid))))
                {
                    int count = 0;

                    var mo_tmp = (_AllInfoMonitors_Mix.Find(M => M.Item1.edid.Equals(monitorInfoX.edid))).Item2.Clone() ?? new MonitorInfo();

                    do
                    {
                        bool rc = _SetVCPFeature(monitorInfoX.hPhysicalMonitor, code, val);

                        if (rc)
                        {
                            _logs.DebugMsg("[VcpCorePlugin] [QueueTrigger]~~~ Set_VCPCapability ctr code is " + code.ToString("X"));
                            _logs.DebugMsg("[VcpCorePlugin] [QueueTrigger]~~~ Set_VCPCapability ctr code value is " + val.ToString());

                            if (IsNotifyVcpCanghed)//Bruce Added 0306
                            {
                                VCPchangedEventArgs _VCPchangedEventArgs = new VCPchangedEventArgs();
                                _VCPchangedEventArgs.vcpcode = code.ToString("X");
                                _VCPchangedEventArgs.value = val.ToString();
                                _VCPchangedEventArgs.monitor = mo_tmp;

                                OnVCPchanged(_VCPchangedEventArgs);
                            }

                            return rc;
                        }

                        count++;
                        _logs.DebugMsg($"[VcpCorePlugin] Set_VCPCapability retry ({count})");

                        if (retry) Thread.Sleep(1000);
                    } while (count < 3 && retry && IsOutInitialize);
                }

                _logs.DebugMsg("[VcpCorePlugin] Set_VCPCapability return false");
                return false;
            }
            catch (Exception ex)
            {
                _logs.DebugMsg("[VcpCorePlugin] Set_VCPCapability ex: " + ex.Message);
                return false;
            }
            finally
            {
                Monitor.Exit(SetVCPLock);
            }
        }

        private object Get_VCPCapability(MonitorInfo_complex monitorInfoX, byte code, int opt, bool retry = true)
        {
            try
            {
                Monitor.Enter(GetVCPLock);

                _logs.DebugMsg("[VcpCorePlugin] Get_VCPCapability conditional expressions I : " + IsOutInitialize.ToString());
                _logs.DebugMsg("[VcpCorePlugin] Get_VCPCapability conditional expressions II : " + (_AllInfoMonitors_Mix.Count > 0 && (_AllInfoMonitors_Mix.Exists(M => M.Item1.edid.Equals(monitorInfoX.edid)))).ToString());

                if ((!IsOutInitialize) || (_AllInfoMonitors_Mix.Count > 0 && (_AllInfoMonitors_Mix.Exists(M => M.Item1.edid.Equals(monitorInfoX.edid)))))
                {
                    int count = 0;

                    do
                    {
                        bool rc = _GetVCPFeatureAndVCPFeatureReply(monitorInfoX.hPhysicalMonitor, code, IntPtr.Zero, out uint currentValue, out uint maxValue); /*monitor[0].hPhysicalMonitor*/
                        if (rc)
                        {
                            if (opt == 1)
                                return maxValue;
                            else
                                return currentValue;
                        }

                        count++;
                        _logs.DebugMsg("[VcpCorePlugin] Get_VCPCapability " + code.ToString("X2") + ", retry(" + count.ToString() + ")");

                        if (retry) Thread.Sleep(1000);
                    } while (count < 3 && retry && IsOutInitialize);
                }

                _logs.DebugMsg("[VcpCorePlugin] Get_VCPCapability return null");
                return null;
            }
            catch (Exception ex)
            {
                _logs.DebugMsg("[VcpCorePlugin] Get_VCPCapability ex: " + ex.Message);
                return null;
            }
            finally
            {
                Monitor.Exit(GetVCPLock);
            }
        }

        private static bool GetColorPresetVcpCode(Dictionary<string, Dictionary<string, string>> ColorPresetHash, string presetName, ref ColorPreset outColorPreset)
        {
            try
            {
                bool GetResult = false;
                string GetResourceName = string.Empty;
                string GetPresetValue = string.Empty;
                var sortedDict = ColorPresetHash.OrderBy(pair => pair.Key).ToDictionary(pair => pair.Key, pair => pair.Value);
                if (sortedDict.ContainsKey("E2"))
                    sortedDict.Remove("E2");
                foreach (string ResourceName in sortedDict.Keys)
                {
                    Dictionary<string, string> myresources = new Dictionary<string, string>();
                    bool TF_Boolean = ColorPresetHash.TryGetValue(ResourceName, out myresources);
                    if (TF_Boolean)
                    {
                        /*
                        if (myresources.ContainsKey(presetName))
                        {
                            GetResourceName = ResourceName;
                            GetPresetValue = myresources[presetName].ToString();
                            GetResult = true;
                            break;
                        }
                        */

                        var lookup = myresources.FirstOrDefault(x => x.Key.Equals(presetName, StringComparison.OrdinalIgnoreCase));

                        if (lookup.Key != null)
                        {
                            GetResourceName = ResourceName;
                            GetPresetValue = myresources[lookup.Key];
                            GetResult = true;
                            break;
                        }
                    }
                }

                if (GetResult)
                {
                    if (GetResourceName == "E2")
                        outColorPreset.vcpode = VcpCode.ColorSpaceE2;
                    else if (GetResourceName == "14")
                        outColorPreset.vcpode = VcpCode.ColorSpace14;
                    else if (GetResourceName == "F0")
                        outColorPreset.vcpode = VcpCode.ColorSpaceF0;
                    else if (GetResourceName == "DC")
                        outColorPreset.vcpode = VcpCode.ColorSpaceDC;
                    else
                        GetResult = false;

                    if (GetResult)
                    {
                        GetResult = false;
                        try
                        {
                            outColorPreset.codeValue = Convert.ToUInt32(GetPresetValue, 16);
                            GetResult = true;
                        }
                        catch (Exception) { GetResult = false; }
                    }
                }
                return GetResult;
            }
            catch (Exception ex)
            {
                _logs.DebugMsg("[VcpCorePlugin] GetColorPresetVcpCode into catch: " + ex.Message);
                return false;
            }
        }

        private static bool GetColorPresetVcpCodeForNonDell(Dictionary<string, Dictionary<string, string>> ColorPresetHash, string presetName, ref ColorPreset outColorPreset)
        {
            try
            {
                bool GetResult = false;
                string GetPresetValue = string.Empty;
                if (ColorPresetHash.ContainsKey("14"))
                {
                    outColorPreset.vcpode = VcpCode.ColorSpace14;
                    Dictionary<string, string> myresources = new Dictionary<string, string>();
                    bool TF_Boolean = ColorPresetHash.TryGetValue("14", out myresources);
                    if (TF_Boolean &&
                        myresources.ContainsKey(presetName))
                    {
                        GetPresetValue = myresources[presetName].ToString();
                        GetResult = true;
                        outColorPreset.codeValue = Convert.ToUInt32(GetPresetValue, 16);
                    }
                }
                return GetResult;
            }
            catch (Exception ex)
            {
                _logs.DebugMsg("[VcpCorePlugin] GetColorPresetVcpCodeForNonDell into catch: " + ex.Message);
                return false;
            }
        }

        private string GetCurrentColorPreset(MonitorInfo_complex monitor)
        {
            try
            {
                object value = null;
                string rc = string.Empty;
                int count = 0;
                do
                {
                    value = Get_VCPCapability(monitor, 0xE2, 0, IsOutInitialize);

                    if (value != null)
                    {
                        uint val = (Convert.ToUInt32(value) & 0XFFFF);
                        string valstring = val.ToString("X2");

                        if (valstring.Length >= 2)
                        {
                            int pos = valstring.Length - 2;
                            rc = valstring.Substring(pos);
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(rc))
                        return NodeFormatter.FormatVCP_E2(rc.ToLower(CultureInfo.InvariantCulture));

                    count++;
                    _logs.DebugMsg($"[VcpCorePlugin] GetCurrentColorPreset retry ({count})");
                    Thread.Sleep(1000);
                } while (count < 3);

                _logs.DebugMsg("[VcpCorePlugin] GetCurrentColorPreset return string.Empty");
                return rc;
            }
            catch (Exception ex)
            {
                _logs.DebugMsg("[VcpCorePlugin] GetCurrentColorPreset into catch: " + ex.Message);
                return string.Empty;
            }
        }

        public bool SetColorPreset(MonitorInfo_complex mi, string presetName)
        {
            try
            {
                bool SetResult = false;
                ColorPreset newpreset = new ColorPreset();

                if (mi.IsDellMonitor)
                {
                    if (GetColorPresetVcpCode(mi.ColorPresentDescription, presetName, ref newpreset))
                    {
                        if (newpreset.vcpode != VcpCode.ColorSpaceE2)
                            SetResult = Set_VCPCapability(mi, Convert.ToByte(newpreset.vcpode), newpreset.codeValue, IsOutInitialize);
                        else
                            SetResult = false;
                    }
                }
                else
                {
                    if (GetColorPresetVcpCodeForNonDell(mi.ColorPresentDescription, presetName, ref newpreset))
                        SetResult = Set_VCPCapability(mi, Convert.ToByte(newpreset.vcpode), newpreset.codeValue, IsOutInitialize);
                }
                return SetResult;
            }
            catch (Exception ex)
            {
                _logs.DebugMsg("[VcpCorePlugin] SetColorPreset into catch: " + ex.Message);
                return false;
            }
        }

        private string GetCapabilities_String(in MonitorInfo_complex monitorx, CancellationToken token)
        {
            bool CheckStatus()
            {
                if (token.Equals(CancellationToken.None))
                {
                    if (IsOutInitialize)
                        return true;   // Not In Initialize and No thread trigger Initialize
                    else
                        return false;  // Not In Initialize but exist thread trigger Initialize
                }
                else
                    return true;  // In Initialize and interrupt by ThrowIfCancellationRequested()
            }

            int count = 0;
            try
            {
                token.ThrowIfCancellationRequested();

                do
                {
                    token.ThrowIfCancellationRequested();

                    bool capabilitiesStringLength = false;
                    int num = 4;
                    uint length = 0;
                    do
                    {
                        token.ThrowIfCancellationRequested();

                        capabilitiesStringLength = _GetCapabilitiesStringLength(monitorx.hPhysicalMonitor, out length);
                        if (capabilitiesStringLength) break;
                        else
                        {
                            _logs.DebugMsg($"[VcpCorePlugin] GetCapabilitiesStringLength error ({_GetLastError()})");
                            num--;
                            Thread.Sleep(250 * (4 - num));
                        }
                    } while (!capabilitiesStringLength && num > 0 && CheckStatus());

                    if (capabilitiesStringLength)
                    {
                        num = 4;
                        var sb = new StringBuilder((int)length);
                        while (!_CapabilitiesRequestAndCapabilitiesReply(monitorx.hPhysicalMonitor, sb, (uint)sb.Capacity) && num > 0 && CheckStatus())
                        {
                            token.ThrowIfCancellationRequested();

                            _logs.DebugMsg($"[VcpCorePlugin] CapabilitiesRequestAndCapabilitiesReply error ({_GetLastError()})");
                            num--;
                            Thread.Sleep(250 * (4 - num));
                        }

                        if (!string.IsNullOrWhiteSpace(sb.ToString()))
                            return sb.ToString();
                    }

                    count++;
                    _logs.DebugMsg($"[VcpCorePlugin] GetCapabilities_String retry ({count})");
                    Thread.Sleep(1000);
                } while (count < 2);

                _logs.DebugMsg("[VcpCorePlugin] GetCapabilities_String return string.Empty");
                return string.Empty;
            }
            catch (TaskCanceledException)
            {
                // Task was canceled before running.
                // Cancelled due to timeout

                _logs.DebugMsg("[VcpCorePlugin] GetCapabilities_String() cancellation happened...");
                return string.Empty;
            }
            catch (OperationCanceledException)
            {
                // Task was canceled while running.
                // Cancelled due to timeout

                _logs.DebugMsg("[VcpCorePlugin] GetCapabilities_String() cancellation happened...");
                return string.Empty;
            }
            catch (Exception ex)
            {
                _logs.DebugMsg("[VcpCorePlugin] GetCapabilities_String() AllInfoMonitors Exception is " + ex.Message);
                return string.Empty;
            }
        }

        private string StringWrite(INode node, ref string rcString, int i = 0)
        {
            INodeFormatter nodeFormatter = new NodeFormatter();

            var formattedNode = nodeFormatter.FormatNode(node);

            if (!string.IsNullOrWhiteSpace(formattedNode))
            {
                var indents = string.Empty.PadLeft((i - 1), '\t');
                rcString += (indents + formattedNode + "\n");
            }

            if (node.Nodes != null && node.Nodes.Any())
                foreach (var childNode in node.Nodes)
                    StringWrite(childNode, ref rcString, (i + 1));

            return rcString;
        }

        private void GetAliasDeviceName(string devicedescription, ref string AliasDeviceName)
        {
            try
            {
                AliasDeviceName = devicedescription;
                if (devicedescription.ToUpper(CultureInfo.InvariantCulture).Contains("DELL"))
                {
                    if (devicedescription.ToUpper(CultureInfo.InvariantCulture).Split(' ').Length > 1)
                    {
                        foreach (string tmp in devicedescription.ToUpper(CultureInfo.InvariantCulture).Split(' '))
                        {
                            if (tmp.StartsWith("AW"))
                            {
                                AliasDeviceName = "Alienware " + tmp;
                                _logs.DebugMsg($"[VcpCorePlugin] _Get_Monitors collection [GetAliasDeviceName] AliasDeviceName: {AliasDeviceName}");
                            }
                        }
                    }
                    else
                    {
                        string dell = "DELL";
                        int nStartIndex = devicedescription.ToUpper(CultureInfo.InvariantCulture).IndexOf(dell) + dell.Length;

                        string tmp = devicedescription.Substring(nStartIndex);

                        if (tmp.StartsWith("AW"))
                        {
                            AliasDeviceName = "Alienware " + tmp;
                            _logs.DebugMsg($"[VcpCorePlugin] _Get_Monitors collection [GetAliasDeviceName] AliasDeviceName: {AliasDeviceName}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logs.DebugMsg("[VcpCorePlugin] GetAliasDeviceName into catch: " + ex.Message);
                return;
            }
        }

        private bool GetAllCapabilityData(string CablitityString, ref Dictionary<string, List<string>> CapabilityDic)
        {
            try
            {
                CablitityString = CablitityString.Replace(") ", ")");
                CablitityString = CablitityString.Replace(" )", ")");
                CablitityString = CablitityString.Replace(")", " ) ");
                string[] array = null;
                string[] array2 = CablitityString.Split(" ");
                int num = 0;

                string[] getArray = CablitityString.Split("model(");

                if (getArray.Length >= 2)
                {
                    _ = CablitityString.Split("model(")[1].Split(")")[0];
                    while (!array2[++num].Contains("vcp"))
                        ;
                    string key = array2[num].Substring(array2[num].IndexOf("vcp(") + "vcp(".Length);
                    CapabilityDic.Add(key, null);
                    while (true)
                    {
                        num++;
                        if (num == array2.Length || array2[num].Contains("mccs_ver") || array2[num].Contains("mswhql") || array2[num].Contains("asset_eep"))
                            break;

                        if (array2[num].Contains("("))
                        {
                            List<string> list = new List<string>();
                            key = array2[num].Split("(")[0];
                            string text = array2[num].Split("(")[1];
                            if (text.Length > 0)
                                list.Add(text);

                            while (!array2[++num].Contains(")"))
                            {
                                if (array2[num].Length > 0)
                                    list.Add(array2[num]);
                            }
                            if (array2[num].Length > 1)
                            {
                                text = array2[num].Replace(")", "");
                                if (text.Length > 0)
                                    list.Add(text);
                            }
                            CapabilityDic.Add(key, list);
                            if (array != null && array.Length > 1 && array[1].Length > 0)
                                CapabilityDic.Add(array[1], null);

                            array = null;
                        }
                        else if (array2[num] != ")")
                            CapabilityDic.Add(array2[num], null);
                    }
                    return true;
                }
                else
                {
                    while (!array2[num].Contains("vcp"))
                        num++;

                    string key = array2[num].Substring(array2[num].IndexOf("vcp(") + "vcp(".Length);
                    CapabilityDic.Add(key, null);
                    while (true)
                    {
                        num++;
                        if (num == array2.Length || array2[num].Contains("mccs_ver") || array2[num].Contains("mswhql") || array2[num].Contains("asset_eep"))
                            break;
                        if (array2[num].Contains("("))
                        {
                            List<string> list = new List<string>();
                            key = array2[num].Split("(")[0];
                            string text = array2[num].Split("(")[1];
                            if (text.Length > 0)
                                list.Add(text);
                            while (!array2[++num].Contains(")"))
                            {
                                if (array2[num].Length > 0)
                                    list.Add(array2[num]);
                            }
                            if (array2[num].Length > 1)
                            {
                                text = array2[num].Replace(")", "");
                                if (text.Length > 0)
                                    list.Add(text);
                            }
                            CapabilityDic.Add(key, list);
                            if (array != null && array.Length > 1 && array[1].Length > 0)
                                CapabilityDic.Add(array[1], null);

                            array = null;
                        }
                        else if (array2[num] != ")")
                            CapabilityDic.Add(array2[num], null);
                    }
                    return true;
                }
            }
            catch (Exception ex)
            {
                _logs.DebugMsg("[VcpCorePlugin] GetAllCapabilityData into catch: " + ex.Message);
                return false;
            }
        }

        private static void InitialColorPresets()
        {
            #region HastTable DC, F0, 14

            Dictionary<string, string> VCPDC = new Dictionary<string, string>();
            VCPDC.Add("Standard/Native", "00");
            VCPDC.Add("Standard", "00");
            VCPDC.Add("Native", "00");
            VCPDC.Add("Multimedia", "02");
            VCPDC.Add("Movie", "03");
            VCPDC.Add("Nature", "04");
            VCPDC.Add("Game/Game1", "05");
            VCPDC.Add("Game", "05");
            VCPDC.Add("Game1", "05");
            VCPDC.Add("Sport", "06");

            Dictionary<string, string> VCPF0 = new Dictionary<string, string>();
            VCPF0.Add("Text", "01");
            VCPF0.Add("AdobeRGB", "02");
            VCPF0.Add("AdobeRGB1", "21");
            VCPF0.Add("AdobeRGB2", "22");
            VCPF0.Add("AdobeRGB1 (D65G2.2L250)", "21");
            VCPF0.Add("AdobeRGB2 (D50G2.2L250)", "22");
            VCPF0.Add("Adobe RGB D65 G2.2 L160", "21"); // PIMS-327394 jim 20241211
            VCPF0.Add("Adobe RGB D50 G2.2 L160", "22"); // PIMS-327394 jim 20241211
            VCPF0.Add("Adobe RGB D65 G2.2 L250", "21"); // PIMS-327394 jim 20241211
            VCPF0.Add("Adobe RGB D50 G2.2 L250", "22"); // PIMS-327394 jim 20241211

            VCPF0.Add("xvMode", "03");
            VCPF0.Add("DICOM", "04");
            VCPF0.Add("CAL1", "05");
            VCPF0.Add("Custom 1 / User 1", "C1");
            VCPF0.Add("Custom 2 / User 2", "C2");
            VCPF0.Add("Custom 3 / User 3", "C3");
            VCPF0.Add("Custom 1", "C1");
            VCPF0.Add("Custom 2", "C2");
            VCPF0.Add("Custom 3", "C3");
            VCPF0.Add("User 1", "C1");
            VCPF0.Add("User 2", "C2");
            VCPF0.Add("User 3", "C3");
            VCPF0.Add("CAL2", "06");
            VCPF0.Add("Metro", "07");
            //VCPF0.Add("Raper", "08");
            VCPF0.Add("Paper", "08");
            VCPF0.Add("Rec.709 / BT.709", "09"); //add 10/04
            VCPF0.Add("Rec. 709 / BT.709", "09");
            VCPF0.Add("Rec. 709/BT.709", "09");
            VCPF0.Add("Rec.709/BT.709", "09");
            VCPF0.Add("Rec 709", "09");
            VCPF0.Add("Rec.709", "09");
            VCPF0.Add("Rec. 709", "09");
            VCPF0.Add("BT.709", "09");
            VCPF0.Add("BT.709 D65 BT1886 L100", "09"); // PIMS-327394 jim 20241211
            VCPF0.Add("DCI-P3", "0A");
            VCPF0.Add("DCI P3 D65 G2.4 L100", "0A"); // PIMS-327394 jim 20241211
            VCPF0.Add("Display P3", "A1");
            VCPF0.Add("Rec.2020 / BT.2020", "0B"); // add 10/04
            VCPF0.Add("Rec.2020", "0B");
            VCPF0.Add("BT.2020", "0B");
            VCPF0.Add("BT.2020 D65 BT1886 L100", "0B"); // PIMS-327394 jim 20241211
            VCPF0.Add("ComfortView", "0C");
            VCPF0.Add("Game2", "0D");
            VCPF0.Add("Game3", "0E");
            VCPF0.Add("FPS Game", "0F");
            VCPF0.Add("RTS Game", "10");
            VCPF0.Add("RPG Game", "11");
            VCPF0.Add("SPORTS Game", "13");
            VCPF0.Add("Standard HDR", "30");
            VCPF0.Add("Movie HDR", "31");
            VCPF0.Add("Game HDR", "32");
            VCPF0.Add("Vivid HDR", "33");
            VCPF0.Add("Desktop", "34");
            VCPF0.Add("Reference", "35");
            VCPF0.Add("Multiscreen Match", "12");
            VCPF0.Add("DisplayHDR", "36");
            VCPF0.Add("HDR10", "37");
            VCPF0.Add("HLG", "38");
            VCPF0.Add("Custom Color HDR", "39"); // Jim add 2025 for [S3225QC] HDR list
            VCPF0.Add("HDR Peak 1000", "3A"); // Jim add 2025 for [S3225QC] HDR list

            Dictionary<string, string> VCP14 = new Dictionary<string, string>();
            VCP14.Add("sRGB", "01");
            VCP14.Add("sRGB D65 sRGB L120", "01"); // PIMS-327394 jim 20241211
            VCP14.Add("sRGB D65 sRGB L250", "01"); // PIMS-327394 jim 20241211
            VCP14.Add("5000K", "04");
            VCP14.Add("5700K", "0B");
            VCP14.Add("Warm", "0B");
            VCP14.Add("6500K", "05");
            VCP14.Add("7500K", "06");
            VCP14.Add("9300K", "08");
            VCP14.Add("Cool", "08");
            VCP14.Add("10000K", "09");
            VCP14.Add("Custom Color", "0C");

            Dictionary<string, string> VCPE2 = new Dictionary<string, string>();
            VCPE2.Add("Standard/Native", "00");
            VCPE2.Add("Standard", "00");
            VCPE2.Add("Native", "00");
            VCPE2.Add("Multimedia", "01");
            VCPE2.Add("Movie", "02");
            VCPE2.Add("Nature", "03");
            VCPE2.Add("Game/Game1", "04"); // 20240731 jim add
            VCPE2.Add("Game", "04");
            VCPE2.Add("Game1", "04");
            VCPE2.Add("Sport", "05");
            VCPE2.Add("Text", "06");
            VCPE2.Add("AdobeRGB", "07");
            VCPE2.Add("AdobeRGB1 (D65G2.2L250)", "2A");
            VCPE2.Add("AdobeRGB2 (D50G2.2L250)", "2B");
            VCPE2.Add("AdobeRGB1", "2A");
            VCPE2.Add("AdobeRGB2", "2B");
            VCPE2.Add("xvMode", "08");
            VCPE2.Add("DICOM", "09");
            VCPE2.Add("CAL1", "0A");
            VCPE2.Add("sRGB", "0B");
            VCPE2.Add("5000K", "0C"); // k -> K
            VCPE2.Add("5700K", "0D"); // k -> K
            VCPE2.Add("Warm", "0E");
            VCPE2.Add("6500K", "0F"); // k -> K
            VCPE2.Add("7500K", "10"); // k -> K
            VCPE2.Add("9300K", "11"); // k -> K
            VCPE2.Add("Cool", "12");
            VCPE2.Add("10000K", "13"); // k -> K
            VCPE2.Add("Custom Color", "14");
            VCPE2.Add("Custom 1 / User 1", "2C");
            VCPE2.Add("Custom 2 / User 2", "2D");
            VCPE2.Add("Custom 3 / User 3", "2E");
            VCPE2.Add("Custom 1", "2C");
            VCPE2.Add("Custom 2", "2D");
            VCPE2.Add("Custom 3", "2E");
            VCPE2.Add("User 1", "2C");
            VCPE2.Add("User 2", "2D");
            VCPE2.Add("User 3", "2E");
            VCPE2.Add("CAL2", "15");
            VCPE2.Add("Metro", "18");
            VCPE2.Add("Paper", "19");
            VCPE2.Add("Rec.709 / BT.709", "1A"); // 20241004 jim add
            VCPE2.Add("Rec. 709 / BT.709", "1A"); // 20240731 jim add
            VCPE2.Add("Rec. 709/BT.709", "1A"); // 20240731 jim add
            VCPE2.Add("Rec.709/BT.709", "1A"); // 20240731 jim add
            VCPE2.Add("Rec 709", "1A");
            VCPE2.Add("Rec.709", "1A");
            VCPE2.Add("Rec. 709", "1A");
            VCPE2.Add("BT.709", "1A");
            VCPE2.Add("DCI-P3", "1B");
            VCPE2.Add("Display P3", "3D");
            VCPE2.Add("Rec.2020 / BT.2020", "1C"); // add 10/14
            VCPE2.Add("Rec.2020", "1C");
            VCPE2.Add("BT.2020", "1C");
            VCPE2.Add("ComfortView", "1D");
            VCPE2.Add("Game2", "1E");
            VCPE2.Add("Game3", "1F");
            VCPE2.Add("FPS Game", "20");
            VCPE2.Add("RTS Game", "21");
            VCPE2.Add("RPG Game", "22");
            VCPE2.Add("SPORTS Game", "2F");
            VCPE2.Add("Standard HDR", "25");
            VCPE2.Add("Movie HDR", "23");
            VCPE2.Add("Game HDR", "24");
            VCPE2.Add("Vivid HDR", "26");
            VCPE2.Add("Desktop", "27");
            VCPE2.Add("Reference", "28");
            VCPE2.Add("Multiscreen Match", "29");
            VCPE2.Add("DisplayHDR", "3A");
            VCPE2.Add("HDR10", "3B");
            VCPE2.Add("HLG", "3C");
            VCPE2.Add("Presets Disabled", "7F"); // Jim  20250111 add back , due to accidental deletion on 20250106
            VCPE2.Add("Custom Color HDR", "30"); // Jim add 2025 for [S3225QC] HDR list
            VCPE2.Add("HDR Peak 1000", "31"); // Jim add 2025 for [S3225QC] HDR list

            #endregion

            _ColorPresets.Add("DC", VCPDC);
            _ColorPresets.Add("14", VCP14);
            _ColorPresets.Add("F0", VCPF0);
            _ColorPresets.Add("E2", VCPE2);
        }

        private List<string> GetColorPresetStrings(Dictionary<string, List<string>> CapDic, List<string> E2List)
        {
            try
            {
                List<string> cmdList = new List<string> { "DC", "F0", "14" };
                List<string> Newlist = new List<string>();

                foreach (string strCmd in cmdList)
                {
                    List<string> value;
                    if (!CapDic.TryGetValue(strCmd, out value))
                        continue;

                    foreach (var item in CapDic[strCmd])
                    {
                        string strHex = $"0x{item}";

                        int hex;

                        if (strHex.StartsWith("0x") && int.TryParse(strHex.Substring(2), NumberStyles.AllowHexSpecifier, null, out hex))
                        {
                            // Single String
                            //string osdString = VcpCodeList.VCPDC.FirstOrDefault((KeyValuePair<string, byte> x) => x.Value == hex)!.Key;
                            List<string> getData = new List<string>();
                            switch (strCmd)
                            {
                                case "DC":
                                    {
                                        getData = VcpCodeList.VCPDC.Where(kvp => kvp.Value == hex).Select(kvp => kvp.Key).ToList();
                                    }
                                    break;

                                case "14":
                                    {
                                        getData = VcpCodeList.VCP14.Where(kvp => kvp.Value == hex).Select(kvp => kvp.Key).ToList();
                                    }
                                    break;

                                case "F0":
                                    {
                                        getData = VcpCodeList.VCPF0.Where(kvp => kvp.Value == hex).Select(kvp => kvp.Key).ToList();
                                    }
                                    break;

                                default:
                                    break;
                            }

                            if (getData.Count > 0)
                            {
                                foreach (var s in getData)
                                {
                                    if (!string.IsNullOrWhiteSpace(s) &&
                                        E2List.Contains(s))
                                    {
                                        Newlist.Add(s);
                                    }
                                }
                            }
                        }
                    }
                }
                return Newlist;
            }
            catch (Exception ex)
            {
                _logs.DebugMsg("[VcpCorePlugin] GetColorPresetStrings into catch: " + ex.Message);
                return new List<string>();
            }
        }

        private bool GetAllColorPreset(string CablitityString, ref Dictionary<string, Dictionary<string, string>> SupportColorPresets, ref List<string> UnDefinedColorPresets, bool isDellMonitor = true)
        {
            try
            {
                bool blRet = true;

                if (isDellMonitor)
                {
                    List<string> KeywordList = new List<string> { "DC", "F0", "14", "E2" };

                    SupportColorPresets = new Dictionary<string, Dictionary<string, string>>();
                    UnDefinedColorPresets = new List<string>();
                    foreach (string keyword in KeywordList)
                    {
                        /*blRet = */
                        GetColorPreset(CablitityString, keyword, ref SupportColorPresets, ref UnDefinedColorPresets);
                    }
                }
                else
                {
                    /*blRet = */
                    GetColorPreset(CablitityString, "14", ref SupportColorPresets, ref UnDefinedColorPresets);
                }
                return blRet;
            }
            catch (Exception ex)
            {
                _logs.DebugMsg("[VcpCorePlugin] GetAllColorPreset into catch: " + ex.Message);
                return true;
            }
        }

        private bool GetColorPreset(string CablitityString, string ResourceName, ref Dictionary<string, Dictionary<string, string>> SupportColorPresets, ref List<string> UnDefinedColorPresets)
        {
            bool hasrestult = false;

            try
            {
                string tempkeyword = ResourceName + "("; ;
                int presetsstartindex = -1;
                int presetsendindex = -1;
                string presetsstring = string.Empty;
                if (CablitityString.Contains(tempkeyword))
                    presetsstartindex = CablitityString.IndexOf(tempkeyword);

                if (presetsstartindex != -1)
                {
                    presetsstartindex += tempkeyword.Length;
                    presetsendindex = CablitityString.IndexOf(')', presetsstartindex);

                    if (presetsendindex == -1)
                        return false;

                    presetsstring = CablitityString.Substring(presetsstartindex, presetsendindex - presetsstartindex).Trim();
                }

                Dictionary<string, string> Resourcepreset = new Dictionary<string, string>();

                foreach (string presetvaluevalue in presetsstring.Split(' '))
                {
                    if (string.IsNullOrWhiteSpace(presetvaluevalue)) continue;

                    hasrestult = true;
                    List<string> tempdescription = GetColorPresetDescriptionByValue(ResourceName, presetvaluevalue);

                    if (tempdescription.Count > 0)
                    {
                        foreach (var data in tempdescription)
                            Resourcepreset.Add(data, presetvaluevalue);

                        // ColorPresetsDescription.Add(tempdescription);
                    }
                    else
                        UnDefinedColorPresets.Add(presetvaluevalue);
                }

                if (SupportColorPresets == null)
                    SupportColorPresets = new Dictionary<string, Dictionary<string, string>>();

                SupportColorPresets.Add(ResourceName, Resourcepreset);
            }
            catch (Exception ex)
            {
                _logs.DebugMsg("[VcpCorePlugin] GetColorPreset into catch: " + ex.Message);
                return hasrestult;
            }

            return hasrestult;
        }

        private List<string> GetColorPresetDescriptionByValue(string ResourceName, string ColorPresetValue)
        {
            try
            {
                //string GetColorPresetDescription = string.Empty;
                List<string> ColorPresetDescriptions = new List<string>();

                if (_ColorPresets.ContainsKey(ResourceName))
                {
                    Dictionary<string, string> mPresets = new Dictionary<string, string>();
                    bool TF_Boolean = _ColorPresets.TryGetValue(ResourceName, out mPresets);
                    if (TF_Boolean)
                    {
                        foreach (string mPresetsDescription in mPresets.Keys)
                        {
                            if (mPresets[mPresetsDescription].ToString() == ColorPresetValue)
                            {
                                ColorPresetDescriptions.Add(mPresetsDescription);
                                //break;
                            }
                        }
                    }
                }
                return ColorPresetDescriptions;
            }
            catch (Exception ex)
            {
                _logs.DebugMsg("[VcpCorePlugin] GetColorPresetDescriptionByValue into catch: " + ex.Message);
                return new List<string>();
            }
        }

        private (bool, bool) UpdateMyself(ref (MonitorInfo_complex, MonitorInfo) Monitor, CancellationToken token)
        {
            try
            {
                var _TargetMonitorx = Monitor.Item1;
                var _TargetMonitor = Monitor.Item2;

                _logs.DebugMsg("[VcpCorePlugin] UpdateMyself  Let's go ...");
                _logs.DebugMsg($"[VcpCorePlugin] {_TargetMonitorx.modelName} UpdateMyself ...");

                var IsConnectAllCorrect = true;
                var IsUpdate = false;

                if (string.IsNullOrWhiteSpace(_TargetMonitorx.CapabilityString))
                {
                    int nRetryCount = 0;
                    do
                    {
                        var ro = _CacheTable.GetFromCacheTable(_TargetMonitorx, "CapibilityString".ToLower(CultureInfo.InvariantCulture));
                        if (ro != null)
                        {
                            if (!string.IsNullOrWhiteSpace(ro.ToString()))
                                _TargetMonitorx.CapabilityString = ro.ToString();
                            break;
                        }
                        else
                        {
                            _TargetMonitorx.CapabilityString = GetCapabilities_String(in _TargetMonitorx, token);
                            if (!string.IsNullOrWhiteSpace(_TargetMonitorx.CapabilityString))
                                break;
                        }

                        nRetryCount++;
                    } while (_TargetMonitorx.IsDellMonitor && (nRetryCount < 2) && IsOutInitialize);

                    Screen[] screenList = Screen.AllScreens;
                    var dpiXProperty = typeof(SystemParameters).GetProperty("DpiX", BindingFlags.NonPublic | BindingFlags.Static);
                    var varX = (int)dpiXProperty.GetValue(null, null);
                    double dpiX = (double)varX / (double)96;
                    foreach (Screen screen in screenList)
                    {
                        if (screen.DeviceName.ToUpper(CultureInfo.InvariantCulture).Equals(_TargetMonitorx.DisplayName.ToUpper(CultureInfo.InvariantCulture), StringComparison.OrdinalIgnoreCase))
                            _TargetMonitorx.scalingFactor = dpiX * Decimal.ToDouble(Math.Round(Decimal.Divide(_TargetMonitorx.pDevmode.dmPelsWidth, screen.Bounds.Width), 2));
                    }

                    _TargetMonitorx.ColorPresetSupportList = new List<string>();
                    _TargetMonitorx.CapabilityDic = new Dictionary<string, List<string>>();

                    if (!string.IsNullOrWhiteSpace(_TargetMonitorx.CapabilityString))
                    {
                        IsUpdate = true;
                        IsConnectAllCorrect &= true;

                        nRetryCount = 0;
                        int nRetryCount2 = 0;
                        while (true)
                        {
                            if (GetAllCapabilityData(_TargetMonitorx.CapabilityString, ref _TargetMonitorx.CapabilityDic))
                            {
                                if (_TargetMonitorx.IsDellMonitor)
                                {
                                    //List<string> e2List = GetE2SupportStrings(_TargetMonitor.CapabilityDic);
                                    List<string> e2List = new List<string>();
                                    bool blE2Ret = getE2SupportStrings(_TargetMonitorx.CapabilityDic, out e2List);

                                    if (blE2Ret && (e2List.Count == 0))
                                    {
                                        ;
                                    }
                                    else
                                    {
                                        if (blE2Ret && (e2List.Count > 0))
                                            _TargetMonitorx.ColorPresetSupportList = GetColorPresetStrings(_TargetMonitorx.CapabilityDic, e2List);

                                        nRetryCount = 0;
                                    }
                                    _TargetMonitorx.SmartHDRSupportList = GetSmartHdrStrings(_TargetMonitorx.CapabilityDic);
                                }
                            }
                            else
                            {
                                _TargetMonitorx.CapabilityDic.Clear();
                                nRetryCount++;
                                _TargetMonitorx.CapabilityString = GetCapabilities_String(in _TargetMonitorx, token);
                                if (nRetryCount >= 2)
                                    break;

                                continue;
                            }

                            if (!GetAllColorPreset(_TargetMonitorx.CapabilityString, ref _TargetMonitorx.ColorPresentDescription, ref _TargetMonitorx.UnDefinedColorPreset, _TargetMonitorx.IsDellMonitor))
                            {
                                nRetryCount2++;
                                _TargetMonitorx.CapabilityString = GetCapabilities_String(in _TargetMonitorx, token);

                                if (nRetryCount2 >= 2)
                                    break;
                            }
                            else
                            {
                                if (!_TargetMonitorx.IsDellMonitor)
                                {
                                    if (_TargetMonitorx.ColorPresentDescription.ContainsKey("14"))
                                    {
                                        if (_TargetMonitorx.ColorPresetSupportList.Count != 0)
                                            _TargetMonitorx.ColorPresetSupportList.Clear();

                                        Dictionary<string, string> vcp14 = new Dictionary<string, string>();
                                        bool TF_boolean = _TargetMonitorx.ColorPresentDescription.TryGetValue("14", out vcp14);
                                        if (TF_boolean)
                                        {
                                            foreach (string tmpkey1 in vcp14.Keys)
                                                _TargetMonitorx.ColorPresetSupportList.Add(tmpkey1);
                                        }
                                    }
                                }
                                else
                                {
                                    // It's Dell monitor but doesn't support E2, F0, DC feature, so we check 0x14 for Color Preset support list.
                                    if ((_TargetMonitorx.ColorPresetSupportList.Count == 0) && _TargetMonitorx.ColorPresentDescription.ContainsKey("14"))
                                    {
                                        Dictionary<string, string> vcp14 = new Dictionary<string, string>();
                                        bool TF_boolean = _TargetMonitorx.ColorPresentDescription.TryGetValue("14", out vcp14);
                                        if (TF_boolean)
                                        {
                                            foreach (string tmpkey1 in vcp14.Keys)
                                                _TargetMonitorx.ColorPresetSupportList.Add(tmpkey1);
                                        }
                                    }
                                }
                                break;
                            }
                        }
                    }
                    else
                    {
                        IsConnectAllCorrect &= false;
                        _logs.DebugMsg($"[VcpCorePlugin] {_TargetMonitorx.modelName} UpdateMyself CapabilityString is null ...");
                    }
                }
                else
                {
                    IsConnectAllCorrect &= true;
                    _logs.DebugMsg($"[VcpCorePlugin] {_TargetMonitorx.modelName} UpdateMyself CapabilityString exist ...");
                }

                if (IsConnectAllCorrect && ((string.IsNullOrWhiteSpace(_TargetMonitorx.FwVersion)) || (string.IsNullOrWhiteSpace(_TargetMonitorx.D_Ctrl)) || (string.IsNullOrWhiteSpace(_TargetMonitorx.SupplierID))))
                {
                    var FwTmp = FwVersion(in _TargetMonitorx, _TargetMonitorx.modelName, CancellationToken.None);
                    _TargetMonitorx.FwVersion = FwTmp.Item1;
                    _TargetMonitorx.D_Ctrl = FwTmp.Item2;
                    _TargetMonitorx.SupplierID = FwTmp.Item3;

                    if ((string.IsNullOrWhiteSpace(_TargetMonitorx.FwVersion)) || (string.IsNullOrWhiteSpace(_TargetMonitorx.D_Ctrl)) || (string.IsNullOrWhiteSpace(_TargetMonitorx.SupplierID)))
                    {
                        IsConnectAllCorrect &= false;
                        _logs.DebugMsg($"[VcpCorePlugin] {_TargetMonitorx.modelName} UpdateMyself FwVersion or D_Ctrl or SupplierID is null ...");
                    }
                    else
                    {
                        IsConnectAllCorrect &= true;
                        IsUpdate = true;
                    }
                }
                else
                {
                    IsConnectAllCorrect &= true;
                    _logs.DebugMsg($"[VcpCorePlugin] {_TargetMonitorx.modelName} UpdateMyself FwVersion & D_Ctrl & SupplierID exist ...");
                }

                if (IsConnectAllCorrect && ((string.IsNullOrWhiteSpace(_TargetMonitor.inputCable)) || (string.IsNullOrWhiteSpace(_TargetMonitor.inputSource))))
                {
                    _ = GetVCPCapability_(_TargetMonitorx, "inputsourcelist", 0);

                    var tmp = GetInputSource(_TargetMonitorx);
                    _TargetMonitorx.inputSource = tmp.Item2;
                    _TargetMonitorx.inputCable = tmp.Item1;

                    if (string.IsNullOrWhiteSpace(tmp.Item1) || string.IsNullOrWhiteSpace(tmp.Item2))
                    {
                        IsConnectAllCorrect &= false;
                        _logs.DebugMsg($"[VcpCorePlugin] {_TargetMonitorx.modelName} UpdateMyself inputSource or inputCable is null ...");
                    }
                    else
                    {
                        IsUpdate = true;
                        IsConnectAllCorrect &= true;
                    }
                }
                else
                {
                    IsConnectAllCorrect &= true;
                    _logs.DebugMsg($"[VcpCorePlugin] {_TargetMonitorx.modelName} UpdateMyself inputSource & inputCable exist ...");
                }

                _TargetMonitor.AliasDeviceName = _TargetMonitorx.AliasDeviceName;
                _TargetMonitor.IsDellMonitor = _TargetMonitorx.IsDellMonitor;
                _TargetMonitor.Index = _TargetMonitorx.Index;
                _TargetMonitor.CapabilityString = _TargetMonitorx.CapabilityString;
                _TargetMonitor.DDCisON = _TargetMonitorx.DDCisON;
                _TargetMonitor.DisplayName = _TargetMonitorx.DisplayName;
                _TargetMonitor.edid = _TargetMonitorx.edid;
                _TargetMonitor.FwVersion = _TargetMonitorx.FwVersion;
                _TargetMonitor.inputSource = _TargetMonitorx.inputSource;
                _TargetMonitor.inputCable = _TargetMonitorx.inputCable;
                _TargetMonitor.CapabilityDic = _TargetMonitorx.CapabilityDic;
                _TargetMonitor.modelName = _TargetMonitorx.modelName;
                _TargetMonitor.series = _TargetMonitorx.series;
                _TargetMonitor.MarketingName = _TargetMonitorx.MarketingName;
                _TargetMonitor.ImageFileName = _TargetMonitorx.ImageFileName;
                _TargetMonitor.SupplierID = _TargetMonitorx.SupplierID;
                _TargetMonitor.D_Ctrl = _TargetMonitorx.D_Ctrl;
                _TargetMonitor.scalingFactor = _TargetMonitorx.scalingFactor;

                _logs.DebugMsg($"[VcpCorePlugin] {_TargetMonitorx.modelName} UpdateMyself go to Initialize2TypesMonitorInfo ...");
                Initialize2TypesMonitorInfo(false, CancellationToken.None);
                _logs.DebugMsg($"[VcpCorePlugin] {_TargetMonitorx.modelName} Initialize2TypesMonitorInfo ok...");

                return (IsConnectAllCorrect, IsUpdate);
            }
            catch (Exception ex)
            {
                _logs.DebugMsg("[VcpCorePlugin] UpdateMyself into catch: " + ex.Message);
                return (false, false);
            }
        }

        private bool CheckIsSupportDisplay(ref MonitorInfo_complex _TargetMonitor)
        {
            try
            {
                bool rc = false;

                if (!string.IsNullOrWhiteSpace(_TargetMonitor.modelName))
                {
                    _TargetMonitor.series = string.Empty;
                    foreach (KeyValuePair<string, List<modelinfos>> kv in _SupportDictionary)
                    {
                        foreach (var tx in kv.Value)
                        {
                            if (tx.ModelName.Equals(_TargetMonitor.modelName, StringComparison.OrdinalIgnoreCase))
                            {
                                _TargetMonitor.series = kv.Key;
                                _TargetMonitor.ImageFileName = tx.ImageFileName;
                                _TargetMonitor.MarketingName = tx.MarketingName;

                                rc = true;
                                break;
                            }
                        }
                    }

                    if (string.IsNullOrWhiteSpace(_TargetMonitor.series) && CheckIsSupportDisplayByBit(in _TargetMonitor, _TargetMonitor.modelName))
                    {
                        _TargetMonitor.series = ChekSeries(_TargetMonitor.modelName);
                        rc = true;
                    }

                    _logs.DebugMsg("[VcpCorePlugin] _TargetMonitor.modelName: " + _TargetMonitor.modelName);
                    _logs.DebugMsg("[VcpCorePlugin] _TargetMonitor.MarketingName: " + _TargetMonitor.MarketingName);
                    _logs.DebugMsg("[VcpCorePlugin] _TargetMonitor.ImageFileName: " + _TargetMonitor.ImageFileName);
                    _logs.DebugMsg("[VcpCorePlugin] _TargetMonitor.series: " + _TargetMonitor.series);
                }

                return rc;
            }
            catch (Exception ex)
            {
                _logs.DebugMsg("[VcpCorePlugin] CheckIsSupportDisplay into catch: " + ex.Message);
                return false;
            }
        }

        private bool CheckIsSupportDisplayByBit(in MonitorInfo_complex monitorx, string model)
        {
            try
            {
                int count = 0;
                object F1supportBit = null;
                do
                {
                    if (F1supportBit == null)
                        F1supportBit = Get_VCPCapability(monitorx, 0xF1, 0, IsOutInitialize);

                    if (F1supportBit != null)
                    {
                        uint r = ((Convert.ToUInt32(F1supportBit)) & 0xA000);
                        if (r > 0)
                        {
                            _logs.DebugMsg("[VcpCorePlugin] CheckIsSupportDisplayByBit return true");
                            return true;
                        }
                        else
                        {
                            uint rr = ((Convert.ToUInt32(F1supportBit)) & 0x0001);
                            int CY = ChekCY(model);

                            if (CY >= 19 && rr > 0)
                            {
                                _logs.DebugMsg("[VcpCorePlugin] CheckIsSupportDisplayByBit return true");
                                return true;
                            }
                            else
                            {
                                _logs.DebugMsg("[VcpCorePlugin] CheckIsSupportDisplayByBit return false");
                                return false;
                            }
                        }
                    }

                    count++;
                    _logs.DebugMsg($"[VcpCorePlugin] CheckIsSupportDisplayByBit retry ({count})");
                    Thread.Sleep(1000);
                } while (count < 3);

                _logs.DebugMsg("[VcpCorePlugin] CheckIsSupportDisplayByBit return false");
                return false;
            }
            catch (Exception ex)
            {
                _logs.DebugMsg("[VcpCorePlugin] CheckIsSupportDisplayByBit into catch: " + ex.Message);
                return false;
            }
        }

        private int ChekCY(string model)
        {
            string number = new string(model.SkipWhile(c => !char.IsDigit(c))
                             .TakeWhile(c => char.IsDigit(c))
                             .ToArray());

            var r = number.Trim().Substring(number.Length - 2);

            if (!string.IsNullOrWhiteSpace(r))
            {
                var IsNumeric = int.TryParse(r, out int result);

                if (IsNumeric)
                    return result;
                else
                    return 0x0;
            }
            return 0x0;
        }

        private string ChekSeries(string model)
        {
            string Series = new string(model.SkipWhile(c => char.IsDigit(c))
                             .TakeWhile(c => !char.IsDigit(c))
                             .ToArray());

            var result = Series.Trim();

            switch (Series.Trim().ToUpper(CultureInfo.InvariantCulture))
            {
                case "AW":
                    return "Alienware Monitors";

                case "G":
                    return "Dell Gaming Monitors";

                case "C":
                    return "Dell C Series Displays";

                case "SE":
                    return "Dell SE Series Monitors";

                case "P":
                    return "Dell P Series Monitors";

                case "S":
                    return "Dell S Series Monitors";

                case "E":
                    return "Dell E Series Monitors";

                case "U":
                    return "Dell UltraSharp (U) Series Monitors";

                case "UP":
                    return "Dell Ultrasharp Premier Color (UP) Series Monitors";

                case "D":
                    return "Dell ODM Series Monitors";

                default:
                    return string.Empty;
            }
        }

        private (string, string, string) FwVersion(in MonitorInfo_complex monitorx, string modelName, CancellationToken token)
        {
            if (FWVersionTable.HideFWModel.Contains(modelName, StringComparer.InvariantCultureIgnoreCase))
            {
                _logs.DebugMsg("[VcpCorePlugin] FwVersion in hide list return \"Ignor\"");
                var stringIgnor = ("\n", "Ignor", "Ignor");
                return stringIgnor;
            }

            int count = 0;
            var Version = (string.Empty, string.Empty, string.Empty);
            object OFWstring = null;
            object ScalarICID = null;
            object OEMID = null;
            do
            {
                if (token.IsCancellationRequested)
                    return (string.Empty, string.Empty, string.Empty);

                if (OFWstring == null && (!token.IsCancellationRequested))
                    OFWstring = Get_VCPCapability(monitorx, Convert.ToByte(VcpCode.Version), 0, IsOutInitialize);

                if (token.IsCancellationRequested)
                    return (string.Empty, string.Empty, string.Empty);

                if (ScalarICID == null && (!token.IsCancellationRequested))
                    ScalarICID = Get_VCPCapability(monitorx, 0xC8, 0, IsOutInitialize);

                if (token.IsCancellationRequested)
                    return (string.Empty, string.Empty, string.Empty);

                if (OEMID == null && (!token.IsCancellationRequested))
                    OEMID = Get_VCPCapability(monitorx, 0xFD, 0, IsOutInitialize);

                if ((OFWstring != null) && (ScalarICID != null) && (OEMID != null) && (!token.IsCancellationRequested))
                {
                    _logs.DebugMsg("[VcpCorePlugin] FwVersion 0XC8 : 0X" + Convert.ToUInt32(ScalarICID).ToString("X"));
                    _logs.DebugMsg("[VcpCorePlugin] FwVersion 0XC9 : 0X" + Convert.ToUInt32(OFWstring).ToString("X"));
                    _logs.DebugMsg("[VcpCorePlugin] FwVersion 0XFD : 0X" + Convert.ToUInt32(OEMID).ToString("X"));

                    Version = getFW2(modelName.ToUpper(CultureInfo.InvariantCulture), Convert.ToInt32(Convert.ToUInt32(OFWstring)), Convert.ToInt32(Convert.ToUInt32(ScalarICID)), Convert.ToInt32(Convert.ToUInt32(OEMID)));

                    if (!string.IsNullOrWhiteSpace(Version.Item1))
                    {
                        _logs.DebugMsg("[VcpCorePlugin] FwVersion : " + Version.Item1);
                        return Version;
                    }
                }

                count++;
                _logs.DebugMsg($"[VcpCorePlugin] FwVersion retry ({count})");
                Thread.Sleep(1000);
            } while (count < 3);

            _logs.DebugMsg("[VcpCorePlugin] FwVersion return string.empty");
            return Version;
        }

        //================================================================================

        private string HEX2BCD(string sBinCode)
        {
            try
            {
                return int.Parse(sBinCode, NumberStyles.HexNumber).ToString().PadLeft(2, '0');
            }
            catch
            {
                _logs.DebugMsg("[VcpCorePlugin] [HEX2BCD] Fail convert :" + sBinCode);
                return sBinCode;
            }
        }

        private (string, string, string) getFW2(string name = "", int C9 = -1, int C8 = -1, int FD = -1)
        {
            try
            {
                _logs.DebugMsg($"[VcpCorePlugin] [getFW2] modelName:{name}, C9:{C9}, C8:{C8}, FD:{FD}");

                string SupplierID = string.Empty;
                string D_Ctrl = string.Empty;

                string text = checkSpecialCase(ref SupplierID, ref D_Ctrl, name, C9, C8, FD);
                if (text.Length > 0)
                    return (text, D_Ctrl, SupplierID);

                string sIcode = GetSIcode(FD, ref SupplierID);
                string scalar = GetScalar(C8, ref D_Ctrl);
                int num = 0;
                num = C9;
                string[] array = BitConverter.ToString(BitConverter.GetBytes(num)).Split("-", StringSplitOptions.None);
                if (FWVersionTable.Case2_HEXHEX.Contains(name))
                {
                    array[0] = HEX2BCD(array[0]);
                    array[1] = HEX2BCD(array[1]);

                    _logs.DebugMsg($"[VcpCorePlugin] {name} C9 is Case2_HEXHEX");
                }
                else if (FWVersionTable.Case4_BCDHEX.Contains(name))
                {
                    array[0] = HEX2BCD(array[0]);

                    _logs.DebugMsg($"[VcpCorePlugin] {name} C9 is Case4_BCDHEX");
                }
                else
                    _logs.DebugMsg($"[VcpCorePlugin] {name} C9 is Case1_BCDBCD");

                if (array[1].Length > 1 && array[1][1] >= 'A' && array[1][1] <= 'F')
                    array[1] = int.Parse(array[1], NumberStyles.HexNumber).ToString();

                string fWStage = GetFWStage(array[1], name);
                string c9High = GetC9High(array[1], name);
                string text2 = array[0];
                return ((fWStage + scalar + sIcode + c9High + text2), D_Ctrl, SupplierID);
            }
            catch
            {
                return (string.Empty, string.Empty, string.Empty);
            }
        }

        private string GetFWStage(string HH, string modelName)
        {
            string text = HH[0].ToString();
            int num = int.Parse(text);
            int modelYear = GetModelYear(modelName);
            switch (num)
            {
                case 0:
                case 4:
                    text = "M";
                    break;

                case 2:
                    if (modelYear < 25)
                        text = "M";
                    break;

                case 12:
                    text = "TM";
                    break;

                case 11:
                    text = "T3";
                    break;

                case 9:
                    text = "T1";
                    break;

                case 5:
                    text = "T";
                    break;

                case 6:
                    text = "S";
                    break;
            }

            return text;
        }

        private static int GetModelYear(string ModelName)
        {
            try
            {
                if (ModelName.Length > 0)
                {
                    string text = new string(ModelName.Where(char.IsDigit).ToArray());
                    string text2 = text.Substring(2, 2);
                    return Convert.ToInt32(text2);
                }
            }
            catch (Exception ex)
            {
                _logs.DebugMsg($"[VcpCorePlugin] GetModelYear ex : {ex.Message}");
            }

            return -1;
        }

        private string GetScalar(int C8, ref string D_Ctrl)
        {
            int num = C8 & 0xFF;
            string result = string.Empty;
            switch (num)
            {
                case 13:
                    result = "1";
                    D_Ctrl = "STM";
                    break;

                case 5:
                    result = "2";
                    D_Ctrl = "Mediatek";
                    break;

                case 9:
                    result = "3";
                    D_Ctrl = "Realtek";
                    break;

                case 18:
                    result = "4";
                    D_Ctrl = "Novatek";
                    break;
            }

            return result;
        }

        private string GetSIcode(int FD, ref string SupplierID)
        {
            string text = string.Empty;
            string @string = Encoding.ASCII.GetString(BitConverter.GetBytes(FD));
            string text2 = @string;
            for (int i = 0; i < text2.Length; i++)
            {
                char c = text2[i];
                if (Convert.ToInt32(c) >= 48)
                    text += c;
            }

            switch (text.ToUpper(CultureInfo.InvariantCulture))
            {
                case "C":
                    SupplierID = "TPV";
                    break;

                case "B":
                    SupplierID = "Qisda";
                    break;

                case "T":
                    SupplierID = "Wistron";
                    break;

                case "F":
                    SupplierID = "Foxconn";
                    break;

                case "O":
                    SupplierID = "BOE";
                    break;

                default:
                    break;
            }

            return text.ToUpper(CultureInfo.InvariantCulture);
        }

        private string GetC9High(string HH, string name)
        {
            string text = HH[1].ToString();
            if (GetModelYear(name) < 24)
            {
                string text2 = text;
                string text3 = text2;
                if (text3 == "9")
                {
                    text = "1";
                }
            }

            return text;
        }

        private string checkSpecialCase(ref string SupplierID, ref string D_Ctrl, string name = "", int C9 = -1, int C8 = -1, int FD = -1)
        {
            string result = string.Empty;
            if (name == "P2423")
            {
                int num = C8;
                string scalar = GetScalar(C8, ref D_Ctrl);
                if (scalar == "3")
                {
                    num = C9;
                    string[] array = BitConverter.ToString(BitConverter.GetBytes(num)).Split("-", StringSplitOptions.None);
                    int num2 = int.Parse(array[1], NumberStyles.HexNumber);
                    int num3 = int.Parse(array[0], NumberStyles.HexNumber);
                    num2 |= 0x100;
                    int num4 = num2 * 100 + num3;
                    array = BitConverter.ToString(BitConverter.GetBytes(num4)).Split("-", StringSplitOptions.None);
                    string hH = int.Parse(array[1]).ToString("X2");
                    string text = int.Parse(array[0]).ToString("X2");
                    string sIcode = GetSIcode(FD, ref SupplierID);
                    string fWStage = GetFWStage(hH, name);
                    string c9High = GetC9High(hH, name);
                    string text2 = text;
                    result = fWStage + scalar + sIcode + c9High + text2;
                }
            }

            return result;
        }

        //================================================================================

        //================================================================================

        private void Get_SupportListFile()
        {
            try
            {
                var path = System.AppDomain.CurrentDomain.BaseDirectory + targetFile;
                var r = DDPMFileSecurity.IsFilePathValid(path, out string log);
                _logs.DebugMsg("[VCPCore plugin] Get_SupportListFile IsFilePathValid : " + log);

                if (r)
                {
                    using (FileLock fileLock = new FileLock(path, PathCheckOption.None, lockNow: true))
                    {
                        if (File.Exists(path))
                        {
                            try
                            {
                                _SupportClassification = File.ReadAllText(targetFile);
                            }
                            catch (Exception ex)
                            {
                                _SupportClassification = string.Empty;
                                _logs.DebugMsg("[VCPCore plugin] Get_SupportListFile IsFilePathValid exception : " + ex.Message);
                            }
                        }

                        if (!string.IsNullOrEmpty(_SupportClassification))
                            _SupportDictionary = JsonConvert.DeserializeObject<Dictionary<string, List<modelinfos>>>(_SupportClassification);
                    }
                }
                else
                    _logs.DebugMsg("[VCPCore plugin] Get_SupportListFile IsFilePathValid fail : " + log);
            }
            catch (Exception ex)
            {
                _logs.DebugMsg("[VcpCorePlugin] Get_SupportListFile into catch: " + ex.Message);
                return;
            }
        }

        private bool IsVcpFunctionSupport(MonitorInfo_complex monitor, uint code)
        {
            _logs.DebugMsg("[VcpCorePlugin] [QueueTrigger] VcpCorePlugin IsVcpFunctionSupport ...");

            var rc = false;

            string codestr = code.ToString("X2");

            var ignoreCodes = new List<uint> { 0xC0, 0xCA, 0x14, 0xDC, 0xF0 };

            if (ignoreCodes.Contains(code))
            {
                _logs.DebugMsg("[VcpCorePlugin] [QueueTrigger] VcpCorePlugin IsVcpFunctionSupport IgnoreCodes...Support");
                rc = true;
            }
            else if (monitor.CapabilityDic.ContainsKey(codestr))
            {
                _logs.DebugMsg("[VcpCorePlugin] [QueueTrigger] VcpCorePlugin IsVcpFunctionSupport ...Support");
                rc = true;
            }
            else
            {
                _logs.DebugMsg("[VcpCorePlugin] [QueueTrigger] VcpCorePlugin IsVcpFunctionSupport ...Not Support");
                rc = false;
            }

            return rc;
        }

        //---------------------------------------------------

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

        #region Event Handler

        private void PluginManagerOnPluginsStarted(object sender, PluginsStartedEventArgs e)
        {
            if (e == null)
                return;
            if (e.ChangedPlugins == null)
                return;
            if (e.ChangedPlugins.Any() == false)
                return;
        }

        #endregion
    }
}