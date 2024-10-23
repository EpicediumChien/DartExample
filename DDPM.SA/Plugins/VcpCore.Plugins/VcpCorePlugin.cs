#region LicenceHeader

//
// Copyright © 2024, Dell Inc., All Rights Reserved.
// This material is confidential and a trade secret.  Permission to use this
// work for any purpose must be obtained in writing from Dell Inc.
//
// VcpCorePlugin.cs created on 28/03/2024T09:50 AM
//

#endregion

using DDPM.SA.Common;
using DDPM.SA.Common.Settings;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.Common.Annotations;
using Dell.Client.Framework.Common.PluginConditions;
using Dell.Client.Framework.Interfaces;
using Dell.Client.Framework.Security;
using Dell.Client.Framework.Security.Interfaces;
using Microsoft;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VcpCore.Common;
using VcpCore.Interfaces;
using static VcpCore.Common.dxva2;
using static VcpCore.Common.User32;
using IDs = VcpCore.Common.IDs;

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
        private const string publisherCompany = "Wistron";
        private const string publisherWebsite = "https://www.wistron.com";
        private const string publisherSupport = "This plugin implements Vcp Core Plugin.";

        private bool _IsAdministrator = ProcessSecurityHelperWrapper.IsCurrentProcessRunningElevated();
        private static bool _Isinitializing = true;
        private IAgent _agent;
        private const string PluginLogId = "VcpCore";
        private bool _IsOnlyGetDellMontor = true;
        private static Logs _logs;

        private static List<MonitorInfo_complex> _AllInfoMonitors;
        private static List<(MonitorInfo_complex, MonitorInfo)> _AllInfoMonitors_Mix;
        private static Dictionary<string, Dictionary<string, string>> _ColorPresets;
        private static TaskLockQueue<ParameterType> _TaskQueue;
        private static BackgroundWorker _TaskQueueExecutor;
        private static ResultLockPool _TaskQueueResult;
        private static System.Timers.Timer _CacheTimer = new System.Timers.Timer(8000);
        private static readonly object TaskQueueExecutorLock = new object();
        private static CancellationTokenSource _cancellationTokenSource;
        private static string _SupportClassification = string.Empty;
        private static Dictionary<string, List<modelinfos>> _SupportDictionary;
        private static readonly string targetFile = "LSTDDPM";
        private static Dictionary<EDID, Dictionary<object, object>> _CacheTable;

        #endregion

        #region Public Members

        public event EventHandler<VCPchangedEventArgs> VCPchanged;

        public event EventHandler<DisplaychangedEventArgs> Displaychanged;

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
            _CacheTable ??= new Dictionary<EDID, Dictionary<object, object>>();
            _ColorPresets ??= new Dictionary<string, Dictionary<string, string>>();
            _TaskQueue ??= new TaskLockQueue<ParameterType>();
            _TaskQueueExecutor ??= new BackgroundWorker();
            _TaskQueueExecutor.DoWork += TaskQueueExecutor_DoWork;
            _TaskQueueExecutor.RunWorkerCompleted += TaskQueueExecutor_RunWorkerCompleted;
            _TaskQueueExecutor.WorkerSupportsCancellation = true;
            _SupportDictionary ??= new Dictionary<string, List<modelinfos>>();
            _CacheTimer.Elapsed += OnCacheTimedRaise;
            _CacheTimer.AutoReset = true;
            _CacheTimer.Enabled = true;

            _logs.DebugMsg("[VcpCorePlugin] Does VcpCorePlugin have Administrator: " + _IsAdministrator.ToString());

            Get_SupportListFile();
            InitialColorPresets();

            if (_cancellationTokenSource != null) InitializeMonitorsList(_cancellationTokenSource.Token).Wait();
            else InitializeMonitorsList(CancellationToken.None).Wait();
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

            return Task.FromResult(_CacheTable ?? new Dictionary<EDID, Dictionary<object, object>>());
        }

        public Task Reset0x52TimerTick(int millisecond)
        {
            _logs.DebugMsg("[VcpCorePlugin] VcpCorePlugin received Reset0x52TimerTick: " + millisecond.ToString() + " requested ...");

            _CacheTimer.Stop();
            _CacheTimer.Interval = millisecond;
            _CacheTimer.AutoReset = true;
            _CacheTimer.Start();

            return Task.FromResult(Task.CompletedTask);
        }

        public Task<List<MonitorInfo>> GetMonitors()
        {
            _logs.DebugMsg("[VcpCorePlugin] VcpCorePlugin received Monitors List requested ...");

            int count = 0;
            do
            {
                if (!_Isinitializing) break;
                SpinWait.SpinUntil(() => false, 250);
                count++;
            } while ((_Isinitializing) && (count < 40));

            List<MonitorInfo> _AllDisplays = new List<MonitorInfo>();
            if (_AllInfoMonitors_Mix.Count > 0)
            {
                foreach (var _AllInfoMonitor in _AllInfoMonitors_Mix)
                    _AllDisplays.Add(_AllInfoMonitor.Item2);
            }

            _logs.DebugMsg("[VcpCorePlugin] AllInfoMonitors count is " + _AllDisplays.Count.ToString());

            return Task.FromResult(_AllDisplays);
        }

        public Task<List<MonitorInfo>> Re_GetMonitors(CancellationToken Token)
        {
            _logs.DebugMsg("[VcpCorePlugin] VcpCorePlugin received Re-Get Monitors List requested ...");

            try
            {
                _AllInfoMonitors = new List<MonitorInfo_complex>();
                _AllInfoMonitors_Mix = new List<(MonitorInfo_complex, MonitorInfo)>();

                while (!_TaskQueue.IsEmpty())
                {
                    if (_TaskQueueExecutor.IsBusy) _TaskQueueExecutor.CancelAsync();
                    else _TaskQueue = new TaskLockQueue<ParameterType>();
                }

                var Cancellation = CancellationTokenSource.CreateLinkedTokenSource(Token);
                var NewToken = Cancellation.Token;

                InitializeMonitorsList(NewToken).Wait();

                List<MonitorInfo> _AllDisplays = new List<MonitorInfo>();
                if (_AllInfoMonitors_Mix.Count > 0)
                {
                    foreach (var _AllInfoMonitor in _AllInfoMonitors_Mix)
                        _AllDisplays.Add(_AllInfoMonitor.Item2);
                }

                _logs.DebugMsg("[VcpCorePlugin] Re-GetMonitors() AllInfoMonitors.count is " + _AllDisplays.Count);

                return Task.FromResult(_AllDisplays);
            }
            catch (TaskCanceledException)
            {
                // Task was canceled before running.
                // Cancelled due to timeout

                _AllInfoMonitors = new List<MonitorInfo_complex>();
                _AllInfoMonitors_Mix = new List<(MonitorInfo_complex, MonitorInfo)>();

                _logs.DebugMsg("[VcpCorePlugin] Re-GetMonitors() cancellation happened...");
                return Task.FromResult(new List<MonitorInfo>());
            }
            catch (OperationCanceledException)
            {
                // Task was canceled while running.
                // Cancelled due to timeout

                _AllInfoMonitors = new List<MonitorInfo_complex>();
                _AllInfoMonitors_Mix = new List<(MonitorInfo_complex, MonitorInfo)>();

                _logs.DebugMsg("[VcpCorePlugin] Re-GetMonitors() cancellation happened...");
                return Task.FromResult(new List<MonitorInfo>());
            }
            catch (Exception e)
            {
                _AllInfoMonitors = new List<MonitorInfo_complex>();
                _AllInfoMonitors_Mix = new List<(MonitorInfo_complex, MonitorInfo)>();

                _logs.DebugMsg("[VcpCorePlugin] Re-GetMonitors Exception : " + e.Message);
                return Task.FromResult(new List<MonitorInfo>());
            }
        }

        public Task<string> GetCapabilitiesString(MonitorInfo monitorInfo)
        {
            _logs.DebugMsg("[VcpCorePlugin] VcpCorePlugin received GetCapabilitiesString requested ...");

            if (_AllInfoMonitors.Count > 0 && _AllInfoMonitors_Mix.Count > 0)
            {
                _logs.DebugMsg("[VcpCorePlugin] TargetMonitor DisplayName is " + monitorInfo.DisplayName);
                _logs.DebugMsg("[VcpCorePlugin] TargetMonitor AliasDeviceName is " + monitorInfo.AliasDeviceName);

                foreach (var moX in _AllInfoMonitors_Mix)
                {
                    if (monitorInfo.Equals(moX.Item2))
                    {
                        //
                        Guid _guid = Guid.NewGuid();
                        _logs.DebugMsg("[VcpCorePlugin] New Job Guid is " + _guid.ToString());

                        ParameterType parameterType = new ParameterType(Queue_CommandType.GetCapabilitiesString, new Type_GetCapabilitiesString(_guid, moX.Item1));
                        _TaskQueue.Enqueue(parameterType);

                        Launch_TaskQueueExecutor();

                        object or = GetResultObjectAsync(_guid).Result;
                        string r = (or != null) ? or.ToString() : string.Empty;

                        return Task.FromResult(r);
                        //
                    }
                }

                _logs.DebugMsg("[VcpCorePlugin] No target display to work. So, ignor requested");
                return Task.FromResult(string.Empty);
            }
            else
            {
                _logs.DebugMsg("[VcpCorePlugin] No monitors to work. So, ignor requested");
                return Task.FromResult(string.Empty);
            }
        }

        public Task<string> GetVCPCapabilities(MonitorInfo monitorInfo)
        {
            _logs.DebugMsg("[VcpCorePlugin] VcpCorePlugin received GetVCPCapabilities requested ...");

            if (_AllInfoMonitors.Count > 0 && _AllInfoMonitors_Mix.Count > 0)
            {
                _logs.DebugMsg("[VcpCorePlugin] TargetMonitor DisplayName is " + monitorInfo.DisplayName);
                _logs.DebugMsg("[VcpCorePlugin] TargetMonitor AliasDeviceName is " + monitorInfo.AliasDeviceName);

                foreach (var moX in _AllInfoMonitors_Mix)
                {
                    if (monitorInfo.Equals(moX.Item2))
                    {
                        //
                        Guid _guid = Guid.NewGuid();
                        _logs.DebugMsg("[VcpCorePlugin] New Job Guid is " + _guid.ToString());

                        ParameterType parameterType = new ParameterType(Queue_CommandType.GetVCPCapabilities, new Type_GetVCPCapabilities(_guid, moX.Item1));
                        _TaskQueue.Enqueue(parameterType);

                        Launch_TaskQueueExecutor();

                        object or = GetResultObjectAsync(_guid).Result;
                        string r = (or != null) ? or.ToString() : string.Empty;

                        return Task.FromResult(r);
                        //
                    }
                }

                _logs.DebugMsg("[VcpCorePlugin] No target display to work. So, ignor requested");
                return Task.FromResult(string.Empty);
            }
            else
            {
                _logs.DebugMsg("[VcpCorePlugin] No monitors to work. So, ignor requested");
                return Task.FromResult(string.Empty);
            }
        }

        public Task<ObjGetVCP> GetVCPCapability(MonitorInfo monitorInfo, byte code, int opt = 0)
        {
            _logs.DebugMsg("[VcpCorePlugin] VcpCorePlugin received GetVCPCapability requested ...");

            if (_AllInfoMonitors.Count > 0 && _AllInfoMonitors_Mix.Count > 0)
            {
                _logs.DebugMsg("[VcpCorePlugin] TargetMonitor DisplayName is " + monitorInfo.DisplayName);
                _logs.DebugMsg("[VcpCorePlugin] TargetMonitor AliasDeviceName is " + monitorInfo.AliasDeviceName);
                _logs.DebugMsg("[VcpCorePlugin] VcpCode is " + BitConverter.ToString(new byte[] { code }));
                _logs.DebugMsg("[VcpCorePlugin] opt is " + opt.ToString());

                foreach (var moX in _AllInfoMonitors_Mix)
                {
                    if (monitorInfo.Equals(moX.Item2))
                    {
                        //
                        Guid _guid = Guid.NewGuid();
                        _logs.DebugMsg("[VcpCorePlugin] New Job Guid is " + _guid.ToString());

                        ParameterType parameterType = new ParameterType(Queue_CommandType.GetVCPCapability_I, new Type_GetVCPCapability_I(_guid, moX.Item1, code, opt));
                        _TaskQueue.Enqueue(parameterType);

                        Launch_TaskQueueExecutor();

                        object or = GetResultObjectAsync(_guid).Result;
                        ObjGetVCP r = (or != null) ? (new ObjGetVCP() { value = or, result = true }) : (new ObjGetVCP() { value = or, result = false });

                        return Task.FromResult(r);
                        //
                    }
                }

                _logs.DebugMsg("[VcpCorePlugin] No target display to work. So, ignor requested");
                return Task.FromResult(new ObjGetVCP() { value = null, result = false });
            }
            else
            {
                _logs.DebugMsg("[VcpCorePlugin] No monitors to work. So, ignor requested");
                return Task.FromResult(new ObjGetVCP() { value = null, result = false });
            }
        }

        public Task<ObjGetVCP> GetVCPCapability(MonitorInfo monitorInfo, string funcName, int opt = 0)
        {
            _logs.DebugMsg("[VcpCorePlugin] VcpCorePlugin received GetVCPCapability requested ...");

            if (_AllInfoMonitors.Count > 0 && _AllInfoMonitors_Mix.Count > 0)
            {
                _logs.DebugMsg("[VcpCorePlugin] TargetMonitor DisplayName is " + monitorInfo.DisplayName);
                _logs.DebugMsg("[VcpCorePlugin] TargetMonitor AliasDeviceName is " + monitorInfo.AliasDeviceName);
                _logs.DebugMsg("[VcpCorePlugin] VcpCode is " + funcName);
                _logs.DebugMsg("[VcpCorePlugin] opt is " + opt.ToString());

                foreach (var moX in _AllInfoMonitors_Mix)
                {
                    if (monitorInfo.Equals(moX.Item2))
                    {
                        //
                        Guid _guid = Guid.NewGuid();
                        _logs.DebugMsg("[VcpCorePlugin] New Job Guid is " + _guid.ToString());

                        ParameterType parameterType = new ParameterType(Queue_CommandType.GetVCPCapability_II, new Type_GetVCPCapability_II(_guid, moX.Item1, funcName, opt));
                        _TaskQueue.Enqueue(parameterType);

                        Launch_TaskQueueExecutor();

                        object or = GetResultObjectAsync(_guid).Result;
                        ObjGetVCP r = (or != null) ? (new ObjGetVCP() { value = or, result = true }) : (new ObjGetVCP() { value = or, result = false });

                        return Task.FromResult(r);
                        //
                    }
                }

                _logs.DebugMsg("[VcpCorePlugin] No target display to work. So, ignor requested");
                return Task.FromResult(new ObjGetVCP() { value = null, result = false });
            }
            else
            {
                _logs.DebugMsg("[VcpCorePlugin] No monitors to work. So, ignor requested");
                return Task.FromResult(new ObjGetVCP() { value = null, result = false });
            }
        }

        public Task<bool> SetVCPCapability(MonitorInfo monitorInfo, byte code, uint val)
        {
            _logs.DebugMsg("[VcpCorePlugin] VcpCorePlugin received SetVCPCapability requested ...");

            if (_AllInfoMonitors.Count > 0 && _AllInfoMonitors_Mix.Count > 0)
            {
                _logs.DebugMsg("[VcpCorePlugin] TargetMonitor DisplayName is " + monitorInfo.DisplayName);
                _logs.DebugMsg("[VcpCorePlugin] TargetMonitor AliasDeviceName is " + monitorInfo.AliasDeviceName);
                _logs.DebugMsg("[VcpCorePlugin] VcpCode is " + BitConverter.ToString(new byte[] { code }));
                _logs.DebugMsg("[VcpCorePlugin] val is " + val.ToString());

                foreach (var moX in _AllInfoMonitors_Mix)
                {
                    if (monitorInfo.Equals(moX.Item2))
                    {
                        //
                        Guid _guid = Guid.NewGuid();
                        _logs.DebugMsg("[VcpCorePlugin] New Job Guid is " + _guid.ToString());

                        ParameterType parameterType = new ParameterType(Queue_CommandType.SetVCPCapability_I, new Type_SetVCPCapability_I(_guid, moX.Item1, code, val));
                        _TaskQueue.Enqueue(parameterType);

                        Launch_TaskQueueExecutor();

                        object or = GetResultObjectAsync(_guid).Result;
                        bool r = (or != null) ? ((bool)or) : false;

                        return Task.FromResult(r);
                        //
                    }
                }

                _logs.DebugMsg("[VcpCorePlugin] No target display to work. So, ignor requested");
                return Task.FromResult(false);
            }
            else
            {
                _logs.DebugMsg("[VcpCorePlugin] No monitors to work. So, ignor requested");
                return Task.FromResult(false);
            }
        }

        public Task<bool> SetVCPCapability(MonitorInfo monitorInfo, string FunctionName, string val)
        {
            _logs.DebugMsg("[VcpCorePlugin] VcpCorePlugin received SetVCPCapability requested ...");

            if (_AllInfoMonitors.Count > 0 && _AllInfoMonitors_Mix.Count > 0)
            {
                _logs.DebugMsg("[VcpCorePlugin] TargetMonitor DisplayName is " + monitorInfo.DisplayName);
                _logs.DebugMsg("[VcpCorePlugin] TargetMonitor AliasDeviceName is " + monitorInfo.AliasDeviceName);
                _logs.DebugMsg("[VcpCorePlugin] VcpCode is " + FunctionName);
                _logs.DebugMsg("[VcpCorePlugin] val is " + val);

                foreach (var moX in _AllInfoMonitors_Mix)
                {
                    if (monitorInfo.Equals(moX.Item2))
                    {
                        //
                        Guid _guid = Guid.NewGuid();
                        _logs.DebugMsg("[VcpCorePlugin] New Job Guid is " + _guid.ToString());

                        ParameterType parameterType = new ParameterType(Queue_CommandType.SetVCPCapability_II, new Type_SetVCPCapability_II(_guid, moX.Item1, FunctionName, val));
                        _TaskQueue.Enqueue(parameterType);

                        Launch_TaskQueueExecutor();

                        object or = GetResultObjectAsync(_guid).Result;
                        bool r = (or != null) ? ((bool)or) : false;

                        return Task.FromResult(r);
                        //
                    }
                }

                _logs.DebugMsg("[VcpCorePlugin] No target display to work. So, ignor requested");
                return Task.FromResult(false);
            }
            else
            {
                _logs.DebugMsg("[VcpCorePlugin] No monitors to work. So, ignor requested");
                return Task.FromResult(false);
            }
        }

        #endregion

        #region Private Methods

        private void Initialize0x52toEmpty()
        {
            _logs.DebugMsg("[VcpCorePlugin] VcpCorePlugin received Initialize0x52toEmpty requested ...");

            if (_AllInfoMonitors.Count > 0 && _AllInfoMonitors_Mix.Count > 0)
            {
                Guid _guid = Guid.NewGuid();
                _logs.DebugMsg("[VcpCorePlugin] New Job Guid is " + _guid.ToString());

                ParameterType parameterType = new ParameterType(Queue_CommandType.Initialize0x52toEmpty, new Type_Initialize0x52toEmpty(_guid));
                _TaskQueue.Enqueue(parameterType);

                Launch_TaskQueueExecutor();
            }
            else
                _logs.DebugMsg("[VcpCorePlugin] No monitors to work. So, ignor requested");
        }

        private void Watcher0x52()
        {
            _logs.DebugMsg("[VcpCorePlugin] VcpCorePlugin received Watcher0x52 requested ...");

            if (_AllInfoMonitors.Count > 0 && _AllInfoMonitors_Mix.Count > 0)
            {
                Guid _guid = Guid.NewGuid();
                _logs.DebugMsg("[VcpCorePlugin] New Job Guid is " + _guid.ToString());

                ParameterType parameterType = new ParameterType(Queue_CommandType.Watcher0x52, new Type_Watcher0x52(_guid));
                _TaskQueue.Enqueue(parameterType);

                Launch_TaskQueueExecutor();
            }
            else
                _logs.DebugMsg("[VcpCorePlugin] No monitors to work. So, ignor requested");
        }

        private void Launch_TaskQueueExecutor()
        {
            try
            {
                Monitor.Enter(TaskQueueExecutorLock);

                if (!_TaskQueueExecutor.IsBusy)
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
            _logs.DebugMsg("[VcpCorePlugin] VcpCorePlugin received GetResultObjectAsync requested ...");
            _logs.DebugMsg("[VcpCorePlugin] Guid is " + guid.ToString());

            using (var tokenSource = new CancellationTokenSource(30 * 1000))
            {
                try
                {
                    var token = tokenSource.Token;

                    //TODO: May be you'll want to add .ConfigureAwait(false);
                    var r = await Task.Run(() => GetQueueResult_(guid, token), token).ConfigureAwait(false);

                    return (r);
                }
                catch (TaskCanceledException)
                {
                    // Task was canceled before running.
                    // Cancelled due to timeout
                    _logs.DebugMsg("[VcpCorePlugin] " + guid.ToString() + " GetResultObjectAsync requested  Timeout ...");
                    return null;
                }
                catch (OperationCanceledException)
                {
                    // Task was canceled while running.
                    // Cancelled due to timeout
                    _logs.DebugMsg("[VcpCorePlugin] " + guid.ToString() + " GetResultObjectAsync requested  Timeout ...");
                    return null;
                }
                catch (Exception e)
                {
                    // Failed to complete due to e exception
                    _logs.DebugMsg($"[VcpCorePlugin] --Task.Run ...there is an exception-- ({e.Message})");

                    //Done: let's be nice and don't swallow the exception
                    //throw;
                    return null;
                }
                finally
                {
                    tokenSource.Dispose();
                }
            }
        }

        private object GetQueueResult_(Guid guid, CancellationToken c_token)
        {
            try
            {
                _logs.DebugMsg("[VcpCorePlugin] VcpCorePlugin into GetQueueResult_ ...");

                object result = null;
                bool r = false;

                while (_TaskQueueExecutor.IsBusy)
                {
                    c_token.ThrowIfCancellationRequested();
                    _logs.DebugMsg("[VcpCorePlugin] GetQueueResult _TaskQueueExecutor is still Running ...");

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

                _logs.DebugMsg(r ? ("[VcpCorePlugin] " + "Guid: " + guid.ToString() + " GetQueueResult_() is Susess") : ("[VcpCorePlugin] " + "Guid: " + guid.ToString() + " GetQueueResult_() is Fail"));

                return result;
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
                        if (_TaskQueue != null) _TaskQueue.Clear();
                        _TaskQueue = new TaskLockQueue<ParameterType>();
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
                                    _logs.DebugMsg(@"[VcpCorePlugin] TaskQueueExecutorDoWork doing GetCapabilitiesString GUID => " + parameter.guid);
                                    var rt = GetCapabilitiesString_(parameter.monitorInfoX);
                                    _TaskQueueResult.Add(parameter.guid, rt);
                                }
                                break;

                            case Queue_CommandType.GetVCPCapabilities:
                                {
                                    Type_GetVCPCapabilities parameter = (Type_GetVCPCapabilities)p.Parameter;
                                    _logs.DebugMsg(@"[VcpCorePlugin] TaskQueueExecutorDoWork doing GetVCPCapabilities GUID => " + parameter.guid);
                                    var rt = GetVCPCapabilities_(parameter.monitorInfoX);
                                    _TaskQueueResult.Add(parameter.guid, rt);
                                }
                                break;

                            case Queue_CommandType.GetVCPCapability_I:
                                {
                                    Type_GetVCPCapability_I parameter = (Type_GetVCPCapability_I)p.Parameter;
                                    _logs.DebugMsg(@"[VcpCorePlugin] TaskQueueExecutorDoWork doing GetVCPCapability_I GUID => " + parameter.guid);
                                    var rt = GetVCPCapability_(parameter.monitorInfoX, parameter.code, parameter.opt);
                                    _TaskQueueResult.Add(parameter.guid, rt);
                                }
                                break;

                            case Queue_CommandType.GetVCPCapability_II:
                                {
                                    Type_GetVCPCapability_II parameter = (Type_GetVCPCapability_II)p.Parameter;
                                    _logs.DebugMsg(@"[VcpCorePlugin] TaskQueueExecutorDoWork doing GetVCPCapability_II GUID => " + parameter.guid);
                                    var rt = GetVCPCapability_(parameter.monitorInfoX, parameter.FunctionName, parameter.opt);
                                    _TaskQueueResult.Add(parameter.guid, rt);
                                }
                                break;

                            case Queue_CommandType.SetVCPCapability_I:
                                {
                                    Type_SetVCPCapability_I parameter = (Type_SetVCPCapability_I)p.Parameter;
                                    _logs.DebugMsg(@"[VcpCorePlugin] TaskQueueExecutorDoWork doing SetVCPCapability_I GUID => " + parameter.guid);
                                    var rt = SetVCPCapability_(parameter.monitorInfoX, parameter.code, parameter.val);
                                    _TaskQueueResult.Add(parameter.guid, rt);
                                }
                                break;

                            case Queue_CommandType.SetVCPCapability_II:
                                {
                                    Type_SetVCPCapability_II parameter = (Type_SetVCPCapability_II)p.Parameter;
                                    _logs.DebugMsg(@"[VcpCorePlugin] TaskQueueExecutorDoWork doing SetVCPCapability_II GUID => " + parameter.guid);
                                    var rt = SetVCPCapability_(parameter.monitorInfoX, parameter.FunctionName, parameter.val);
                                    _TaskQueueResult.Add(parameter.guid, rt);
                                }
                                break;

                            case Queue_CommandType.Initialize0x52toEmpty:
                                {
                                    Type_Initialize0x52toEmpty parameter = (Type_Initialize0x52toEmpty)p.Parameter;
                                    _logs.DebugMsg(@"[VcpCorePlugin] TaskQueueExecutorDoWork doing Initialize0x52toEmpty GUID => " + parameter.guid);
                                    var rt = Initialize0x52toEmpty_();
                                    _TaskQueueResult.Add(parameter.guid, rt);
                                }
                                break;

                            case Queue_CommandType.Watcher0x52:
                                {
                                    Type_Watcher0x52 parameter = (Type_Watcher0x52)p.Parameter;
                                    _logs.DebugMsg(@"[VcpCorePlugin] TaskQueueExecutorDoWork doing Watcher0x52 GUID => " + parameter.guid);
                                    var rt = Watcher0x52_();
                                    _TaskQueueResult.Add(parameter.guid, rt);
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
            _logs.DebugMsg("[VcpCorePlugin] [QueueTrigger] VcpCorePlugin started GetCapabilitiesString_ ...");

            if ((_AllInfoMonitors.Count > 0) && (_AllInfoMonitors.Exists(t => t.edid.SerialNumber == monitorInfoX.edid.SerialNumber)))
            {
                _logs.DebugMsg("[VcpCorePlugin] [QueueTrigger] VcpCorePlugin received GetCapabilitiesString requested ...");
                _logs.DebugMsg("[VcpCorePlugin] [QueueTrigger] TargetMonitor DisplayName is " + monitorInfoX.DisplayName);
                _logs.DebugMsg("[VcpCorePlugin] [QueueTrigger] TargetMonitor AliasDeviceName is " + monitorInfoX.AliasDeviceName);

                uint length = 0;

                _GetCapabilitiesStringLength(monitorInfoX.hPhysicalMonitor, out length);

                var sb = new StringBuilder((int)length);
                _CapabilitiesRequestAndCapabilitiesReply(monitorInfoX.hPhysicalMonitor, sb, (uint)sb.Capacity);

                return sb.ToString();
            }
            else
            {
                _logs.DebugMsg("[VcpCorePlugin] [QueueTrigger] Lose TargetMonitor, ignor requested ...");
                return string.Empty;
            }
        }

        private string GetVCPCapabilities_(MonitorInfo_complex monitorInfoX)
        {
            _logs.DebugMsg("[VcpCorePlugin] [QueueTrigger] VcpCorePlugin started GetVCPCapabilities_ ...");

            if ((_AllInfoMonitors.Count > 0) && (_AllInfoMonitors.Exists(t => t.edid.SerialNumber == monitorInfoX.edid.SerialNumber)))
            {
                _logs.DebugMsg("[VcpCorePlugin] [QueueTrigger] VcpCorePlugin received GetVCPCapabilities requested ...");
                _logs.DebugMsg("[VcpCorePlugin] [QueueTrigger] TargetMonitor DisplayName is " + monitorInfoX.DisplayName);
                _logs.DebugMsg("[VcpCorePlugin] [QueueTrigger] TargetMonitor AliasDeviceName is " + monitorInfoX.AliasDeviceName);

                string rcString = string.Empty;
                //var physicalMonitor = GetPhysicalMonitors(monitorInfoX.hMonitor);
                ITokenizer tokenizer = new CapabilitiesTokenizer();
                IParser parser = new CapabilitiesParser();
                INodeFormatter formatter = new NodeFormatter();

                var capabilities = monitorInfoX.CapabilityString; // GetCapabilities_String(monitorInfoX);
                var tokens = tokenizer.GetTokens(capabilities);
                var node = parser.Parse(tokens);

                if (!string.IsNullOrWhiteSpace(capabilities) && (node.Nodes.Count() > 0))
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

                return rcString;
            }
            else
            {
                _logs.DebugMsg("[VcpCorePlugin] [QueueTrigger] Lose TargetMonitor, ignor requested ...");
                return string.Empty;
            }
        }

        private object GetVCPCapability_(MonitorInfo_complex monitorInfoX, byte code, int opt = 0)
        {
            _logs.DebugMsg("[VcpCorePlugin] [QueueTrigger] VcpCorePlugin started GetVCPCapability_ ...");

            if ((_AllInfoMonitors.Count > 0) && (_AllInfoMonitors.Exists(t => t.edid.SerialNumber == monitorInfoX.edid.SerialNumber)))
            {
                _logs.DebugMsg("[VcpCorePlugin] [QueueTrigger] VcpCorePlugin received GetVCPCapability requested ...");
                _logs.DebugMsg("[VcpCorePlugin] [QueueTrigger] TargetMonitor DisplayName is " + monitorInfoX.DisplayName);
                _logs.DebugMsg("[VcpCorePlugin] [QueueTrigger] TargetMonitor AliasDeviceName is " + monitorInfoX.AliasDeviceName);
                _logs.DebugMsg("[VcpCorePlugin] [QueueTrigger] VcpCode is " + BitConverter.ToString(new byte[] { code }));
                _logs.DebugMsg("[VcpCorePlugin] [QueueTrigger] opt is " + opt.ToString());

                object r = new object();
                if (opt == 0)
                    r = GetFromCacheTable(monitorInfoX, code);

                if (opt != 0 || r == null)
                    r = Get_VCPCapability(monitorInfoX, code, opt, true);

                return r;
            }
            else
            {
                _logs.DebugMsg("[VcpCorePlugin] [QueueTrigger] Lose TargetMonitor, ignor requested ...");
                return null;
            }
        }

        private object GetVCPCapability_(MonitorInfo_complex monitorInfoX, string func, int opt = 0)
        {
            _logs.DebugMsg("[VcpCorePlugin] [QueueTrigger] VcpCorePlugin started GetVCPCapability_ ...");

            if ((_AllInfoMonitors.Count > 0) && (_AllInfoMonitors.Exists(t => t.edid.SerialNumber == monitorInfoX.edid.SerialNumber)))
            {
                _logs.DebugMsg("[VcpCorePlugin] [QueueTrigger] VcpCorePlugin received GetVCPCapability requested ...");
                _logs.DebugMsg("[VcpCorePlugin] [QueueTrigger] TargetMonitor DisplayName is " + monitorInfoX.DisplayName);
                _logs.DebugMsg("[VcpCorePlugin] [QueueTrigger] TargetMonitor AliasDeviceName is " + monitorInfoX.AliasDeviceName);
                _logs.DebugMsg("[VcpCorePlugin] [QueueTrigger] VcpCode is " + func);
                _logs.DebugMsg("[VcpCorePlugin] [QueueTrigger] opt is " + opt.ToString());

                object ro = null;

                switch (func)
                {
                    case "inputsourcelist":
                        {
                            ro = GetFromCacheTable(monitorInfoX, func);
                            if (ro == null)
                            {
                                string VCPCapabilities_ = GetVCPCapabilities_(monitorInfoX);
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

                                        VcpCodeList.VCP60.TryGetValue(val_, out uint value);
                                        list.Add(new InputSourceObject() { Name = R_, value = value });
                                    }
                                    ro = list;
                                    SetToCacheTable(monitorInfoX, "inputsourcelist", ro);
                                }
                            }
                        }
                        break;

                    case @"USB-C Prioritization":
                        {
                            ro = GetVcp2Steps(monitorInfoX, 0xEA, 0xF8FF);
                            ro = (ro == null) ? ro : NodeFormatter.FormatVCP_F8(((uint)ro).ToString("X"));
                        }
                        break;

                    case "colorpreset":
                        ro = GetCurrentColorPreset(monitorInfoX);
                        break;

                    case nameof(Gaming_GameEnhancementMode):
                        ro = ((uint)GetVcp2Steps(monitorInfoX, VcpCodeList.VCPctr["Gaming"], 0x1F) & 0x0f);
                        break;

                    case nameof(Gaming_ResponseTime):
                        ro = ((uint)GetVcp2Steps(monitorInfoX, VcpCodeList.VCPctr["Gaming"], 0x2F) & 0x0f);
                        break;

                    case nameof(Gaming_DarkStabilizer):
                        ro = ((uint)GetVcp2Steps(monitorInfoX, VcpCodeList.VCPctr["Gaming"], 0x3F) & 0x0f);
                        break;

                    case nameof(Gaming_HDRType):
                        ro = ((uint)GetVcp2Steps(monitorInfoX, VcpCodeList.VCPctr["Gaming"], 0x4F) & 0x0f);
                        break;

                    default:
                        {
                            byte fucCode = TranslatorVCPctrCode(func);

                            ro = GetFromCacheTable(monitorInfoX, fucCode);
                            if (ro == null)
                                ro = Get_VCPCapability(monitorInfoX, fucCode, opt, true);
                        }
                        break;
                }

                return ro;
            }
            else
            {
                _logs.DebugMsg("[VcpCorePlugin] [QueueTrigger] Lose TargetMonitor, ignor requested ...");

                return null;
            }
        }

        private bool SetVCPCapability_(MonitorInfo_complex monitorInfoX, byte code, uint val)
        {
            _logs.DebugMsg("[VcpCorePlugin] [QueueTrigger] VcpCorePlugin started SetVCPCapability_ ...");

            if ((_AllInfoMonitors.Count > 0) && (_AllInfoMonitors.Exists(t => t.edid.SerialNumber == monitorInfoX.edid.SerialNumber)))
            {
                _logs.DebugMsg("[VcpCorePlugin] [QueueTrigger] VcpCorePlugin received SetVCPCapability requested ...");
                _logs.DebugMsg("[VcpCorePlugin] [QueueTrigger] TargetMonitor DisplayName is " + monitorInfoX.DisplayName);
                _logs.DebugMsg("[VcpCorePlugin] [QueueTrigger] TargetMonitor AliasDeviceName is " + monitorInfoX.AliasDeviceName);
                _logs.DebugMsg("[VcpCorePlugin] [QueueTrigger] VcpCode is " + BitConverter.ToString(new byte[] { code }));
                _logs.DebugMsg("[VcpCorePlugin] [QueueTrigger] val is " + val.ToString());

                bool rc = Set_VCPCapability(monitorInfoX, code, val, true);

                if (rc)
                {
                    switch (code)
                    {
                        case 0xEC:
                            break;

                        case 0X04:
                            InitializeCacheTable();
                            break;

                        case 0x05:
                            InitializeCacheTable();
                            break;

                        default:
                            SetToCacheTable(monitorInfoX, code, val);
                            break;
                    }
                }
                return rc;  //Task.CompletedTask;
            }
            else
            {
                _logs.DebugMsg("[VcpCorePlugin] [QueueTrigger] Lose TargetMonitor, ignor requested ...");
                return false;
            }
        }

        private bool SetVCPCapability_(MonitorInfo_complex monitorInfoX, string FunctionName, string val)
        {
            _logs.DebugMsg("[VcpCorePlugin] [QueueTrigger] VcpCorePlugin started SetVCPCapability_ ...");

            if ((_AllInfoMonitors.Count > 0) && (_AllInfoMonitors.Exists(t => t.edid.SerialNumber == monitorInfoX.edid.SerialNumber)))
            {
                _logs.DebugMsg("[VcpCorePlugin] [QueueTrigger] VcpCorePlugin received SetVCPCapability requested ...");
                _logs.DebugMsg("[VcpCorePlugin] [QueueTrigger] TargetMonitor DisplayName is " + monitorInfoX.DisplayName);
                _logs.DebugMsg("[VcpCorePlugin] [QueueTrigger] TargetMonitor AliasDeviceName is " + monitorInfoX.AliasDeviceName);
                _logs.DebugMsg("[VcpCorePlugin] [QueueTrigger] FunctionName is " + FunctionName);
                _logs.DebugMsg("[VcpCorePlugin] [QueueTrigger] val is " + val);

                bool rc = false;

                switch (FunctionName.ToLower())
                {
                    case "colorpreset":
                        {
                            rc = SetColorPreset(monitorInfoX, val);

                            if (rc)
                            {
                                SetToCacheTable(monitorInfoX, FunctionName.ToLower(), val);

                                VCPchangedEventArgs _VCPchangedEventArgs = new VCPchangedEventArgs();
                                _VCPchangedEventArgs.vcpcode = FunctionName.ToLower();
                                _VCPchangedEventArgs.value = val;
                                _VCPchangedEventArgs.monitor = new MonitorInfo()
                                {
                                    AliasDeviceName = monitorInfoX.AliasDeviceName,
                                    IsDellMonitor = monitorInfoX.IsDellMonitor,
                                    Index = monitorInfoX.Index,
                                    CapabilityString = monitorInfoX.CapabilityString,
                                    DDCisON = monitorInfoX.DDCisON,
                                    DisplayName = monitorInfoX.DisplayName,
                                    edid = monitorInfoX.edid,
                                    FwVersion = monitorInfoX.FwVersion,
                                    inputSource = monitorInfoX.inputSource,
                                    inputCable = monitorInfoX.inputCable,
                                    CapabilityDic = monitorInfoX.CapabilityDic,
                                    modelName = monitorInfoX.modelName,
                                    series = monitorInfoX.series,
                                    MarketingName = monitorInfoX.MarketingName,
                                    ImageFileName = monitorInfoX.ImageFileName,
                                    SupplierID = monitorInfoX.SupplierID,
                                    D_Ctrl = monitorInfoX.D_Ctrl,
                                };
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
                            uint value = TranslatorVCPcategory(FunctionName, val_);
                            rc = SetVCPCapability_(monitorInfoX, fuc, value);

                            if (rc)
                            {
                                SetToCacheTable(monitorInfoX, FunctionName.ToLower(), val);

                                foreach ((MonitorInfo_complex x, MonitorInfo o) in _AllInfoMonitors_Mix)
                                {
                                    if (x.Equals(monitorInfoX))
                                    {
                                        x.inputSource = val;
                                        o.inputSource = val;

                                        break;
                                    }
                                }

                                Initialize2TypesMonitorInfo(false);

                                VCPchangedEventArgs _VCPchangedEventArgs = new VCPchangedEventArgs();
                                _VCPchangedEventArgs.vcpcode = FunctionName.ToLower();
                                _VCPchangedEventArgs.value = val;
                                _VCPchangedEventArgs.monitor = new MonitorInfo()
                                {
                                    AliasDeviceName = monitorInfoX.AliasDeviceName,
                                    IsDellMonitor = monitorInfoX.IsDellMonitor,
                                    Index = monitorInfoX.Index,
                                    CapabilityString = monitorInfoX.CapabilityString,
                                    DDCisON = monitorInfoX.DDCisON,
                                    DisplayName = monitorInfoX.DisplayName,
                                    edid = monitorInfoX.edid,
                                    FwVersion = monitorInfoX.FwVersion,
                                    inputSource = val,
                                    inputCable = monitorInfoX.inputCable,
                                    CapabilityDic = monitorInfoX.CapabilityDic,
                                    modelName = monitorInfoX.modelName,
                                    series = monitorInfoX.series,
                                    MarketingName = monitorInfoX.MarketingName,
                                    ImageFileName = monitorInfoX.ImageFileName,
                                    SupplierID = monitorInfoX.SupplierID,
                                    D_Ctrl = monitorInfoX.D_Ctrl,
                                };
                                OnVCPchanged(_VCPchangedEventArgs);
                            }
                        }
                        break;

                    default:
                        {
                            byte fuc = TranslatorVCPctrCode(FunctionName);
                            uint value = TranslatorVCPcategory(FunctionName, val);
                            rc = SetVCPCapability_(monitorInfoX, fuc, value);

                            if (rc)
                            {
                                SetToCacheTable(monitorInfoX, FunctionName.ToLower(), val);

                                VCPchangedEventArgs _VCPchangedEventArgs = new VCPchangedEventArgs();
                                _VCPchangedEventArgs.vcpcode = FunctionName.ToLower();
                                _VCPchangedEventArgs.value = val;
                                _VCPchangedEventArgs.monitor = new MonitorInfo()
                                {
                                    AliasDeviceName = monitorInfoX.AliasDeviceName,
                                    IsDellMonitor = monitorInfoX.IsDellMonitor,
                                    Index = monitorInfoX.Index,
                                    CapabilityString = monitorInfoX.CapabilityString,
                                    DDCisON = monitorInfoX.DDCisON,
                                    DisplayName = monitorInfoX.DisplayName,
                                    edid = monitorInfoX.edid,
                                    FwVersion = monitorInfoX.FwVersion,
                                    inputSource = monitorInfoX.inputSource,
                                    inputCable = monitorInfoX.inputCable,
                                    CapabilityDic = monitorInfoX.CapabilityDic,
                                    modelName = monitorInfoX.modelName,
                                    series = monitorInfoX.series,
                                    MarketingName = monitorInfoX.MarketingName,
                                    ImageFileName = monitorInfoX.ImageFileName,
                                    SupplierID = monitorInfoX.SupplierID,
                                    D_Ctrl = monitorInfoX.D_Ctrl,
                                };
                                OnVCPchanged(_VCPchangedEventArgs);
                            }
                        }
                        break;
                }

                return rc;
            }
            else
            {
                _logs.DebugMsg("[VcpCorePlugin] [QueueTrigger] Lose TargetMonitor, ignor requested ...");
                return false;
            }
        }

        private bool Initialize0x52toEmpty_()
        {
            _logs.DebugMsg("[VcpCorePlugin] [QueueTrigger] VcpCorePlugin Initialize 0x52 to 0 Let's go ...");

            if (_AllInfoMonitors.Count > 0)
            {
                bool rc = false;
                object object_0x02 = null;

                foreach (MonitorInfo_complex monitorInfoX in _AllInfoMonitors)
                {
                    _logs.DebugMsg("[VcpCorePlugin] [QueueTrigger] VcpCorePlugin " + monitorInfoX.AliasDeviceName + " Initialize 0x52 to 0");

                    object_0x02 = Get_VCPCapability(monitorInfoX, 0x02, 0, true);
                    while ((object_0x02 != null) && (((uint)object_0x02) != 1))
                    {
                        _logs.DebugMsg("[VcpCorePlugin] [QueueTrigger] VcpCorePlugin VCP 0x02 is " + ((uint)object_0x02).ToString("X"));

                        rc = Set_VCPCapability(monitorInfoX, 0x02, 0x01, true);
                        _logs.DebugMsg("[VcpCorePlugin] [QueueTrigger] VcpCorePlugin set VCP 0x02 to 1 : " + (rc ? "Sucess" : "Fail"));

                        object_0x02 = Get_VCPCapability(monitorInfoX, 0x02, 0, true);
                    }
                }

                _logs.DebugMsg("[VcpCorePlugin] [QueueTrigger] VcpCorePlugin All Monitor Initialize 0x52 to 0 finish ...");
                return rc;
            }
            else
            {
                _logs.DebugMsg("[VcpCorePlugin] [QueueTrigger] Lose all monitors, ignor requested ...");
                return false;
            }
        }

        private bool Watcher0x52_()
        {
            _logs.DebugMsg("[VcpCorePlugin] [QueueTrigger] Watching 0x52 Let's go ...");

            if (_AllInfoMonitors.Count > 0)
            {
                bool rc = false;
                object object_0x02 = null;
                object object_0x52 = null;

                foreach (MonitorInfo_complex monitorInfoX in _AllInfoMonitors)
                {
                    _logs.DebugMsg("[VcpCorePlugin] [QueueTrigger] VcpCorePlugin " + monitorInfoX.AliasDeviceName + " Watching 0x52");

                    object_0x02 = Get_VCPCapability(monitorInfoX, 0x02, 0, true);

                    if (object_0x02 != null)
                    {
                        _logs.DebugMsg("[VcpCorePlugin] [QueueTrigger] VcpCorePlugin DDC/CI is connected");

                        monitorInfoX.DDCCIFail = 0;

                        if (!monitorInfoX.DDCisON)
                        {
                            _logs.DebugMsg("[VcpCorePlugin] [QueueTrigger] VcpCorePlugin prepare DDC/CI broadcast off -> on");

                            DDCCIchangedEventArgs _ddcceventArg = new DDCCIchangedEventArgs();
                            _ddcceventArg.DDCisON = true;
                            foreach ((MonitorInfo_complex x, MonitorInfo o) in _AllInfoMonitors_Mix)
                            {
                                if (x.Equals(monitorInfoX))
                                {
                                    x.DDCisON = true;
                                    x.DDCCIFail = 0;
                                    o.DDCisON = true;

                                    Initialize2TypesMonitorInfo(false);

                                    _ddcceventArg.monitors = o;

                                    OnDDCCIStatuschanged(_ddcceventArg);
                                }
                            }
                        }

                        while ((object_0x02 != null) && (((uint)object_0x02) != 1))
                        {
                            _logs.DebugMsg("[VcpCorePlugin] [QueueTrigger] VcpCorePlugin VCP 0x02 is " + ((uint)object_0x02).ToString("X"));

                            object_0x52 = Get_VCPCapability(monitorInfoX, 0x52, 0, true);

                            if (object_0x52 != null)
                            {
                                var tmp = Get_VCPCapability(monitorInfoX, Convert.ToByte(object_0x52), 0, true);

                                if (tmp != null)
                                {
                                    if (((uint)object_0x52).ToString("X").ToUpper().Equals("E2"))
                                    {
                                        uint val = (Convert.ToUInt32(tmp) & 0XFFFF);
                                        string valstring = val.ToString("X2");
                                        int pos = valstring.Length - 2;
                                        string rc_str = valstring.Substring(pos);

                                        if (!string.IsNullOrWhiteSpace(rc_str))
                                        {
                                            SetToCacheTable(monitorInfoX, Convert.ToByte(object_0x52), val);

                                            VCPchangedEventArgs _VCPchangedEventArgs = new VCPchangedEventArgs();
                                            _VCPchangedEventArgs.vcpcode = ((uint)object_0x52).ToString("X");
                                            _VCPchangedEventArgs.value = NodeFormatter.FormatVCP_E2(rc_str.ToLower());
                                            _VCPchangedEventArgs.monitor = new MonitorInfo()
                                            {
                                                AliasDeviceName = monitorInfoX.AliasDeviceName,
                                                IsDellMonitor = monitorInfoX.IsDellMonitor,
                                                Index = monitorInfoX.Index,
                                                CapabilityString = monitorInfoX.CapabilityString,
                                                DDCisON = monitorInfoX.DDCisON,
                                                DisplayName = monitorInfoX.DisplayName,
                                                edid = monitorInfoX.edid,
                                                FwVersion = monitorInfoX.FwVersion,
                                                inputSource = monitorInfoX.inputSource,
                                                inputCable = monitorInfoX.inputCable,
                                                CapabilityDic = monitorInfoX.CapabilityDic,
                                                modelName = monitorInfoX.modelName,
                                                series = monitorInfoX.series,
                                                MarketingName = monitorInfoX.MarketingName,
                                                ImageFileName = monitorInfoX.ImageFileName,
                                                SupplierID = monitorInfoX.SupplierID,
                                                D_Ctrl = monitorInfoX.D_Ctrl,
                                            };
                                            OnVCPchanged(_VCPchangedEventArgs);
                                        }
                                    }
                                    else if (((uint)object_0x52).ToString("X").ToUpper().Equals("60"))
                                    {
                                        string rstring = string.Empty;
                                        uint val = Convert.ToUInt32(tmp) & 0xFF;
                                        string rc_str = val.ToString("X2");

                                        if (!string.IsNullOrWhiteSpace(rc_str))
                                        {
                                            SetToCacheTable(monitorInfoX, 0x60, val);

                                            foreach ((MonitorInfo_complex x, MonitorInfo o) in _AllInfoMonitors_Mix)
                                            {
                                                if (x.Equals(monitorInfoX))
                                                {
                                                    List<InputSourceObject> r = new List<InputSourceObject>();
                                                    var R = GetFromCacheTable(monitorInfoX, "inputsourcelist");
                                                    if (R != null)
                                                        r.AddRange(R as List<InputSourceObject>);

                                                    var n = NodeFormatter.FormatVCP_60(rc_str.ToLower());

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
                                                        x.inputSource = n;
                                                        o.inputSource = x.inputSource;
                                                        rstring = o.inputSource;
                                                    }
                                                    else
                                                    {
                                                        x.inputSource = System.Text.RegularExpressions.Regex.Replace(n, @"\d", string.Empty);
                                                        o.inputSource = x.inputSource;
                                                        rstring = o.inputSource;
                                                    }

                                                    break;
                                                }
                                            }

                                            Initialize2TypesMonitorInfo(false);

                                            //----------------------------------------------------------------------

                                            VCPchangedEventArgs _VCPchangedEventArgsII = new VCPchangedEventArgs();
                                            _VCPchangedEventArgsII.vcpcode = "input select";
                                            _VCPchangedEventArgsII.value = rstring;
                                            _VCPchangedEventArgsII.monitor = new MonitorInfo()
                                            {
                                                AliasDeviceName = monitorInfoX.AliasDeviceName,
                                                IsDellMonitor = monitorInfoX.IsDellMonitor,
                                                Index = monitorInfoX.Index,
                                                CapabilityString = monitorInfoX.CapabilityString,
                                                DDCisON = monitorInfoX.DDCisON,
                                                DisplayName = monitorInfoX.DisplayName,
                                                edid = monitorInfoX.edid,
                                                FwVersion = monitorInfoX.FwVersion,
                                                inputSource = rstring,
                                                inputCable = monitorInfoX.inputCable,
                                                CapabilityDic = monitorInfoX.CapabilityDic,
                                                modelName = monitorInfoX.modelName,
                                                series = monitorInfoX.series,
                                                MarketingName = monitorInfoX.MarketingName,
                                                ImageFileName = monitorInfoX.ImageFileName,
                                                SupplierID = monitorInfoX.SupplierID,
                                                D_Ctrl = monitorInfoX.D_Ctrl,
                                            };
                                            OnVCPchanged(_VCPchangedEventArgsII);

                                            //----------------------------------------------------------------------

                                            //VCPchangedEventArgs _VCPchangedEventArgIIs = new VCPchangedEventArgs();
                                            //_VCPchangedEventArgIIs.vcpcode = ((uint)object_0x52).ToString("X");
                                            //_VCPchangedEventArgIIs.value = NodeFormatter.FormatVCP_60(rc_str.ToLower());
                                            //_VCPchangedEventArgIIs.monitor = new MonitorInfo()
                                            //{
                                            //    AliasDeviceName = monitorInfoX.AliasDeviceName,
                                            //    IsDellMonitor = monitorInfoX.IsDellMonitor,
                                            //    Index = monitorInfoX.Index,
                                            //    CapabilityString = monitorInfoX.CapabilityString,
                                            //    DDCisON = monitorInfoX.DDCisON,
                                            //    DisplayName = monitorInfoX.DisplayName,
                                            //    edid = monitorInfoX.edid,
                                            //    FwVersion = monitorInfoX.FwVersion,
                                            //    inputSource = NodeFormatter.FormatVCP_60(rc_str.ToLower()),
                                            //    inputCable = monitorInfoX.inputCable,
                                            //    CapabilityDic = monitorInfoX.CapabilityDic,
                                            //    modelName = monitorInfoX.modelName,
                                            //    series = monitorInfoX.series,
                                            //    MarketingName = monitorInfoX.MarketingName,
                                            //    ImageFileName = monitorInfoX.ImageFileName,
                                            //    SupplierID = monitorInfoX.SupplierID,
                                            //    D_Ctrl = monitorInfoX.D_Ctrl,
                                            //};
                                            //OnVCPchanged(_VCPchangedEventArgIIs);
                                        }
                                    }
                                    else
                                    {
                                        SetToCacheTable(monitorInfoX, Convert.ToByte(object_0x52), (Convert.ToUInt32(tmp)));

                                        VCPchangedEventArgs _VCPchangedEventArgs = new VCPchangedEventArgs();
                                        _VCPchangedEventArgs.vcpcode = ((uint)object_0x52).ToString("X");
                                        _VCPchangedEventArgs.value = ((uint)tmp).ToString();
                                        _VCPchangedEventArgs.monitor = new MonitorInfo()
                                        {
                                            AliasDeviceName = monitorInfoX.AliasDeviceName,
                                            IsDellMonitor = monitorInfoX.IsDellMonitor,
                                            Index = monitorInfoX.Index,
                                            CapabilityString = monitorInfoX.CapabilityString,
                                            DDCisON = monitorInfoX.DDCisON,
                                            DisplayName = monitorInfoX.DisplayName,
                                            edid = monitorInfoX.edid,
                                            FwVersion = monitorInfoX.FwVersion,
                                            inputSource = monitorInfoX.inputSource,
                                            inputCable = monitorInfoX.inputCable,
                                            CapabilityDic = monitorInfoX.CapabilityDic,
                                            modelName = monitorInfoX.modelName,
                                            series = monitorInfoX.series,
                                            MarketingName = monitorInfoX.MarketingName,
                                            ImageFileName = monitorInfoX.ImageFileName,
                                            SupplierID = monitorInfoX.SupplierID,
                                            D_Ctrl = monitorInfoX.D_Ctrl,
                                        };
                                        OnVCPchanged(_VCPchangedEventArgs);
                                    }
                                }
                                else
                                    _logs.DebugMsg("[VcpCorePlugin] [QueueTrigger] Get VCP ctr in 0x52 that value is null");
                            }
                            else
                                _logs.DebugMsg("[VcpCorePlugin] [QueueTrigger] 0x52 is null");

                            rc = Set_VCPCapability(monitorInfoX, 0x02, 0x01, true);
                            _logs.DebugMsg("[VcpCorePlugin] [QueueTrigger] VcpCorePlugin set VCP 0x02 to 1 : " + (rc ? "Sucess" : "Fail"));

                            object_0x02 = Get_VCPCapability(monitorInfoX, 0x02, 0, true);
                        }
                    }
                    else
                    {
                        _logs.DebugMsg("[VcpCorePlugin] [QueueTrigger] VcpCorePlugin DDC/CI is disconnected");
                        monitorInfoX.DDCCIFail++;
                        if (monitorInfoX.DDCCIFail >= 3)
                        {
                            monitorInfoX.DDCCIFail = 3;

                            if (monitorInfoX.DDCisON)
                            {
                                _logs.DebugMsg("[VcpCorePlugin] [QueueTrigger] VcpCorePlugin prepare DDC/CI broadcast on -> off");

                                DDCCIchangedEventArgs _ddcceventArg = new DDCCIchangedEventArgs();
                                _ddcceventArg.DDCisON = false;
                                foreach ((MonitorInfo_complex x, MonitorInfo o) in _AllInfoMonitors_Mix)
                                {
                                    if (x.Equals(monitorInfoX))
                                    {
                                        x.DDCisON = false;
                                        x.DDCCIFail = 3;
                                        o.DDCisON = false;

                                        Initialize2TypesMonitorInfo(false);

                                        _ddcceventArg.monitors = o;

                                        OnDDCCIStatuschanged(_ddcceventArg);
                                    }
                                }
                            }
                        }
                    }
                }

                _logs.DebugMsg("[VcpCorePlugin] [QueueTrigger] Watching 0x52 finish ...");

                return rc;
            }
            else
            {
                _logs.DebugMsg("[VcpCorePlugin] [QueueTrigger] Lose all monitors, ignor requested ...");
                return false;
            }
        }

        protected virtual void OnVCPchanged(VCPchangedEventArgs e)
        {
            _logs.DebugMsg("[VcpCorePlugin] VcpCorePlugin brocast OnVCPchanged ...");

            //VCPchanged?.Invoke(this, e);
            EventHandler<VCPchangedEventArgs> handler = VCPchanged;
            if (handler != null)
                Task.Run(() => handler.Invoke(this, e)).ConfigureAwait(false);

            //The Asynchronous Programming Model (APM) (using IAsyncResult and BeginInvoke) is no longer the preferred method of making asynchronous calls.
            //The Task-based Asynchronous Pattern (TAP) is the recommended async model as of .NET Framework 4.5.
            //Because of this, and because the implementation of async delegates depends on remoting features not present in .NET Core, BeginInvoke and EndInvoke delegate calls are not supported in .NET Core.
            //This is discussed in GitHub issue dotnet/corefx #5940.
        }

        protected virtual void OnDisplaychanged(DisplaychangedEventArgs e)
        {
            _logs.DebugMsg("[VcpCorePlugin] VcpCorePlugin brocast OnDisplaychanged ...");

            //Displaychanged?.Invoke(this, e);
            EventHandler<DisplaychangedEventArgs> handler = Displaychanged;
            if (handler != null)
                Task.Run(() => handler.Invoke(this, e)).ConfigureAwait(false);

            //The Asynchronous Programming Model (APM) (using IAsyncResult and BeginInvoke) is no longer the preferred method of making asynchronous calls.
            //The Task-based Asynchronous Pattern (TAP) is the recommended async model as of .NET Framework 4.5.
            //Because of this, and because the implementation of async delegates depends on remoting features not present in .NET Core, BeginInvoke and EndInvoke delegate calls are not supported in .NET Core.
            //This is discussed in GitHub issue dotnet/corefx #5940.
        }

        protected virtual void OnDDCCIStatuschanged(DDCCIchangedEventArgs e)
        {
            _logs.DebugMsg("[VcpCorePlugin] VcpCorePlugin brocast OnDDCCIStatuschanged ...");

            //DDCCIStatuschanged?.Invoke(this, e);
            EventHandler<DDCCIchangedEventArgs> handler = DDCCIStatuschanged;
            if (handler != null)
                Task.Run(() => handler.Invoke(this, e)).ConfigureAwait(false);

            //The Asynchronous Programming Model (APM) (using IAsyncResult and BeginInvoke) is no longer the preferred method of making asynchronous calls.
            //The Task-based Asynchronous Pattern (TAP) is the recommended async model as of .NET Framework 4.5.
            //Because of this, and because the implementation of async delegates depends on remoting features not present in .NET Core, BeginInvoke and EndInvoke delegate calls are not supported in .NET Core.
            //This is discussed in GitHub issue dotnet/corefx #5940.
        }

        private async Task InitializeMonitorsList(CancellationToken token)
        {
            try
            {
                _AllInfoMonitors = new List<MonitorInfo_complex>();
                _AllInfoMonitors_Mix = new List<(MonitorInfo_complex, MonitorInfo)>();

                try
                {
                    if (_cancellationTokenSource != null) _cancellationTokenSource.Cancel();
                }
                catch (TaskCanceledException)
                {
                    // Task was canceled before running.
                    // Cancelled due to timeout

                    _AllInfoMonitors = new List<MonitorInfo_complex>();
                    _AllInfoMonitors_Mix = new List<(MonitorInfo_complex, MonitorInfo)>();
                    _cancellationTokenSource.Dispose();
                    _logs.DebugMsg("[VcpCorePlugin] InitializeMonitorsList cancellation happened...");
                }
                catch (OperationCanceledException)
                {
                    // Task was canceled while running.
                    // Cancelled due to timeout

                    _AllInfoMonitors = new List<MonitorInfo_complex>();
                    _AllInfoMonitors_Mix = new List<(MonitorInfo_complex, MonitorInfo)>();
                    _cancellationTokenSource.Dispose();
                    _logs.DebugMsg("[VcpCorePlugin] InitializeMonitorsList cancellation happened...");
                }
                catch (Exception e)
                {
                    _AllInfoMonitors = new List<MonitorInfo_complex>();
                    _AllInfoMonitors_Mix = new List<(MonitorInfo_complex, MonitorInfo)>();
                    _cancellationTokenSource.Dispose();
                    // Failed to complete due to e exception
                    _logs.DebugMsg($"[VcpCorePlugin] InitializeMonitorsList...there is an exception-- ({e.Message})");

                    //Done: let's be nice and don't swallow the exception
                    //throw new InvalidOperationException("some exception happened but not about InitializeMonitorsList cancellation");
                }
                finally
                {
                    using (var _cancellationTokenSource_tmp = new CancellationTokenSource())
                    {
                        try
                        {
                            var Newtoken = _cancellationTokenSource_tmp.Token;
                            if (token != CancellationToken.None)
                                _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(Newtoken, token);
                            else _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(Newtoken);

                            var TokenNew = _cancellationTokenSource.Token;

                            _logs.DebugMsg("[VcpCorePlugin] VcpCorePlugin into InitializeMonitorsList ...");

                            //TODO: May be you'll want to add .ConfigureAwait(false);
                            var monitors = await Task.Run(() => _GetMonitors(TokenNew), TokenNew).ConfigureAwait(false);

                            if (monitors.Count < 1)
                            {
                                _Isinitializing = false;
                                var T = Task.Run(() =>
                                {
                                    _logs.DebugMsg("[VcpCorePlugin] *** Monitor-Retrier start ...");

                                    int count = 0;
                                    while (count < 10)
                                    {
                                        TokenNew.ThrowIfCancellationRequested();
                                        Thread.Sleep(1000);
                                        List<MonitorInfo_complex> mos = _GetMonitors(TokenNew);
                                        if (mos.Count > 0)
                                        {
                                            _logs.DebugMsg("[VcpCorePlugin] *** Monitor-Retrier catch monitors ...");
                                            _logs.DebugMsg("[VcpCorePlugin] *** Monitor-Retrier Monitors.count is " + mos.Count);
                                            _AllInfoMonitors = mos.ToList();
                                            Initialize2TypesMonitorInfo(true);
                                            _logs.DebugMsg("[VcpCorePlugin] *** Monitor-Retrier AllInfoMonitors.count is " + _AllInfoMonitors_Mix.Count);
                                            Initialize0x52toEmpty();

                                            //----//
                                            List<MonitorInfo> _tmp = new List<MonitorInfo>();
                                            //_tmp.Clear();//Dean 0626 fix SAST issue, remove this line since the object just created and it's empty
                                            foreach (var monitorInfoX in _AllInfoMonitors)
                                            {
                                                MonitorInfo minfo = new MonitorInfo()
                                                {
                                                    AliasDeviceName = monitorInfoX.AliasDeviceName,
                                                    IsDellMonitor = monitorInfoX.IsDellMonitor,
                                                    Index = monitorInfoX.Index,
                                                    CapabilityString = monitorInfoX.CapabilityString,
                                                    DDCisON = monitorInfoX.DDCisON,
                                                    DisplayName = monitorInfoX.DisplayName,
                                                    edid = monitorInfoX.edid,
                                                    FwVersion = monitorInfoX.FwVersion,
                                                    inputSource = monitorInfoX.inputSource,
                                                    inputCable = monitorInfoX.inputCable,
                                                    CapabilityDic = monitorInfoX.CapabilityDic,
                                                    modelName = monitorInfoX.modelName,
                                                    series = monitorInfoX.series,
                                                    MarketingName = monitorInfoX.MarketingName,
                                                    ImageFileName = monitorInfoX.ImageFileName,
                                                    SupplierID = monitorInfoX.SupplierID,
                                                    D_Ctrl = monitorInfoX.D_Ctrl,
                                                };
                                                _tmp.Add(minfo);
                                            }
                                            DisplaychangedEventArgs _displaychangedEventArgss = new DisplaychangedEventArgs();
                                            _displaychangedEventArgss.count = _AllInfoMonitors.Count;
                                            _displaychangedEventArgss.monitors = new List<MonitorInfo>(_tmp);
                                            OnDisplaychanged(_displaychangedEventArgss);
                                            //----//
                                            break;
                                        }
                                        count++;
                                    }
                                }, TokenNew).ConfigureAwait(false);
                            }
                            else
                            {
                                _logs.DebugMsg("[VcpCorePlugin] (mos.Count > 0) ...");

                                _AllInfoMonitors = monitors.ToList();
                                Initialize2TypesMonitorInfo(true);
                                _logs.DebugMsg("[VcpCorePlugin] InitializeMonitorsList AllInfoMonitors.count is " + _AllInfoMonitors_Mix.Count);
                                Initialize0x52toEmpty();
                            }
                            _logs.DebugMsg("[VcpCorePlugin] InitializeMonitorsList AllInfoMonitors.count is " + _AllInfoMonitors_Mix.Count);
                        }
                        catch (TaskCanceledException)
                        {
                            // Task was canceled before running.
                            // Cancelled due to timeout

                            _AllInfoMonitors = new List<MonitorInfo_complex>();
                            _AllInfoMonitors_Mix = new List<(MonitorInfo_complex, MonitorInfo)>();

                            _logs.DebugMsg("[VcpCorePlugin] using Cancellation InitializeMonitorsList cancellation happened...");
                        }
                        catch (OperationCanceledException)
                        {
                            // Task was canceled while running.
                            // Cancelled due to timeout

                            _AllInfoMonitors = new List<MonitorInfo_complex>();
                            _AllInfoMonitors_Mix = new List<(MonitorInfo_complex, MonitorInfo)>();

                            _logs.DebugMsg("[VcpCorePlugin] using Cancellation InitializeMonitorsList cancellation happened...");
                        }
                        catch (Exception e)
                        {
                            _AllInfoMonitors = new List<MonitorInfo_complex>();
                            _AllInfoMonitors_Mix = new List<(MonitorInfo_complex, MonitorInfo)>();

                            // Failed to complete due to e exception
                            _logs.DebugMsg($"[VcpCorePlugin] using Cancellation InitializeMonitorsList there is an exception-- ({e.Message})");

                            ////Done: let's be nice and don't swallow the exception
                            //throw new InvalidOperationException("some exception happened but not about InitializeMonitorsList cancellation");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logs.DebugMsg("[VcpCorePlugin] InitializeMonitorsList Exception : " + ex.Message);
            }
        }

        private void Initialize2TypesMonitorInfo(bool forwardMode = true)
        {
            if (forwardMode)
            {
                _logs.DebugMsg("[VcpCorePlugin] Initialize2TypesMonitorInfo Let's go (forwardMode=true) ...");

                if (_AllInfoMonitors.Count > 0)
                {
                    _AllInfoMonitors_Mix = new List<(MonitorInfo_complex, MonitorInfo)>();

                    InitializeCacheTable();

                    foreach (var MonitorInfoX in _AllInfoMonitors)
                    {
                        GetVCPCapability_(MonitorInfoX, "inputsourcelist", 0);
                        var tmp = GetInputSource(MonitorInfoX);
                        MonitorInfoX.inputSource = tmp.Item2;
                        MonitorInfoX.inputCable = tmp.Item1;

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
                        };
                        _AllInfoMonitors_Mix.Add((MonitorInfoX, monitorInfo));

                        //--------------------------------------------------------------
                        _logs.DebugMsg("//----------------Show MonitorInfo------------//");
                        _logs.DebugMsg("[VcpCorePlugin] Show*** monitorInfo : ");
                        _logs.DebugMsg("[VcpCorePlugin] Show*** AliasDeviceName : " + monitorInfo.AliasDeviceName);
                        _logs.DebugMsg("[VcpCorePlugin] Show*** IsDellMonitor : " + monitorInfo.IsDellMonitor.ToString());
                        _logs.DebugMsg("[VcpCorePlugin] Show*** Index : " + monitorInfo.Index.ToString());
                        _logs.DebugMsg("[VcpCorePlugin] Show*** CapabilityString : " + monitorInfo.CapabilityString);
                        _logs.DebugMsg("[VcpCorePlugin] Show*** DDCisON : " + monitorInfo.DDCisON.ToString());
                        _logs.DebugMsg("[VcpCorePlugin] Show*** DisplayName : " + monitorInfo.DisplayName);
                        _logs.DebugMsg("[VcpCorePlugin] Show*** edid : " + monitorInfo.edid.Edid);
                        _logs.DebugMsg("[VcpCorePlugin] Show*** edid.ManufactureID : " + monitorInfo.edid.ManufactureID);
                        _logs.DebugMsg("[VcpCorePlugin] Show*** edid.PID : " + monitorInfo.edid.PID);
                        _logs.DebugMsg("[VcpCorePlugin] Show*** edid.VendorID : " + monitorInfo.edid.VendorID);
                        _logs.DebugMsg("[VcpCorePlugin] Show*** edid.Year : " + monitorInfo.edid.Year.ToString());
                        _logs.DebugMsg("[VcpCorePlugin] Show*** edid.Month : " + monitorInfo.edid.Month.ToString());
                        _logs.DebugMsg("[VcpCorePlugin] Show*** edid.Week : " + monitorInfo.edid.Week.ToString());
                        _logs.DebugMsg("[VcpCorePlugin] Show*** edid.ModelName : " + monitorInfo.edid.ModelName);
                        _logs.DebugMsg("[VcpCorePlugin] Show*** edid.EdidVersion : " + monitorInfo.edid.EdidVersion);
                        _logs.DebugMsg("[VcpCorePlugin] Show*** edid.VideoInputType : " + monitorInfo.edid.VideoInputType);
                        _logs.DebugMsg("[VcpCorePlugin] Show*** edid.Size : " + monitorInfo.edid.Size.ToString());
                        _logs.DebugMsg("[VcpCorePlugin] Show*** edid.ServiceTag : " + monitorInfo.edid.ServiceTag);
                        _logs.DebugMsg("[VcpCorePlugin] Show*** edid.SerialNumber : " + monitorInfo.edid.SerialNumber);
                        _logs.DebugMsg("[VcpCorePlugin] Show*** FwVersion : " + monitorInfo.FwVersion);
                        _logs.DebugMsg("[VcpCorePlugin] Show*** inputSource : " + monitorInfo.inputSource);
                        _logs.DebugMsg("[VcpCorePlugin] Show*** inputCable : " + monitorInfo.inputCable);
                        _logs.DebugMsg("[VcpCorePlugin] Show*** modelName : " + monitorInfo.modelName);
                        _logs.DebugMsg("[VcpCorePlugin] Show*** series : " + monitorInfo.series);
                        _logs.DebugMsg("[VcpCorePlugin] Show*** MarketingName : " + monitorInfo.MarketingName);
                        _logs.DebugMsg("[VcpCorePlugin] Show*** ImageFileName : " + monitorInfo.ImageFileName);
                        _logs.DebugMsg("//----------------Show END------------//");
                        //--------------------------------------------------------------
                    }
                }
            }
            else
            {
                _logs.DebugMsg("[VcpCorePlugin] Initialize2TypesMonitorInfo Let's go (forwardMode=false) ...");

                if (_AllInfoMonitors_Mix.Count > 0)
                {
                    _AllInfoMonitors = new List<MonitorInfo_complex>();

                    foreach ((MonitorInfo_complex x, MonitorInfo o) in _AllInfoMonitors_Mix)
                        _AllInfoMonitors.Add(x);
                }
            }
            _Isinitializing = false;
            _logs.DebugMsg("[VcpCorePlugin] Initialize2TypesMonitorInfo finishi : count => " + _AllInfoMonitors_Mix.Count);
        }

        private void InitializeCacheTable()
        {
            _logs.DebugMsg("[VcpCorePlugin] InitializeCacheTable Let's go ...");

            if (_AllInfoMonitors.Count > 0)
            {
                foreach (var MonitorInfo in _AllInfoMonitors)
                {
                    bool IsExist = false;
                    var Keys = _CacheTable.Keys.ToList();
                    foreach (var Key in Keys)
                    {
                        if (Key.Equals(MonitorInfo.edid))
                        {
                            IsExist = true;
                            break;
                        }
                    }
                    if (!IsExist)
                        _CacheTable.Add(MonitorInfo.edid, new Dictionary<object, object>() { { "CapibilityString", MonitorInfo.CapabilityString } });
                }
            }

            foreach (var item in _CacheTable)
            {
                var Keys = (item.Value).Keys.ToList();
                foreach (var Key in Keys)
                {
                    if ((Key is string) && (Key.ToString().Equals("CapibilityString")))
                        continue;
                    else
                        item.Value.Remove(Key);
                }
            }

            _logs.DebugMsg("[VcpCorePlugin] InitializeCacheTable finish : CacheTable count => " + _CacheTable.Count);
        }

        private object GetFromCacheTable(MonitorInfo_complex MonitorInfo, object key)
        {
            _logs.DebugMsg("[VcpCorePlugin] GetFromCacheTable Let's go ...");
            _logs.DebugMsg("[VcpCorePlugin] GetFromCacheTable TargetMonitor AliasDeviceName is " + MonitorInfo.AliasDeviceName);
            _logs.DebugMsg("[VcpCorePlugin] GetFromCacheTable Key is " + ((key is string) ? key.ToString() : Convert.ToUInt32(key).ToString("X")));

            if (_CacheTable.Count > 0)
            {
                var Keys = _CacheTable.Keys.ToList();
                foreach (var Key in Keys)
                {
                    if (Key.Equals(MonitorInfo.edid))
                    {
                        if (_CacheTable[Key].ContainsKey(key))
                        {
                            bool rc = false;
                            var result = new object();
                            rc = _CacheTable[Key].TryGetValue(key, out result);

                            _logs.DebugMsg("[VcpCorePlugin] Is GetFromCacheTable success?? : result => " + rc.ToString());
                            return (rc ? result : null);
                        }
                    }
                }
            }

            _logs.DebugMsg("[VcpCorePlugin] GetFromCacheTable finish : result => null");

            return null;
        }

        private void SetToCacheTable(MonitorInfo_complex MonitorInfo, object key, object value)
        {
            _logs.DebugMsg("[VcpCorePlugin] SetToCacheTable Let's go ...");
            _logs.DebugMsg("[VcpCorePlugin] SetToCacheTable TargetMonitor AliasDeviceName is " + MonitorInfo.AliasDeviceName);
            _logs.DebugMsg("[VcpCorePlugin] SetToCacheTable Key is " + ((key is string) ? key.ToString() : Convert.ToUInt32(key).ToString("X")));

            if (_CacheTable.Count > 0)
            {
                bool IsExist = false;
                var Keys = _CacheTable.Keys.ToList();
                foreach (var Key in Keys)
                {
                    if (Key.Equals(MonitorInfo.edid))
                    {
                        if (_CacheTable[Key].ContainsKey(key))
                            (_CacheTable[Key])[key] = value;
                        else
                            (_CacheTable[Key]).Add(key, value);

                        IsExist = true;
                        break;
                    }
                }
                if (!IsExist)
                    _CacheTable.Add(MonitorInfo.edid, new Dictionary<object, object>() { { "CapibilityString", MonitorInfo.CapabilityString }, { key, value } });
            }
            else
                _CacheTable.Add(MonitorInfo.edid, new Dictionary<object, object>() { { "CapibilityString", MonitorInfo.CapabilityString }, { key, value } });

            _logs.DebugMsg("[VcpCorePlugin] SetToCacheTable finish");
        }

        //---------------------------------------------------

        private object GetVcp2Steps(MonitorInfo_complex monitor, byte ctr, uint checkMask)
        {
            int count = 0;
            object ro = null;

            do
            {
                bool r = Set_VCPCapability(monitor, ctr, checkMask, false);
                if (r)
                {
                    ro = Get_VCPCapability(monitor, ctr, 0, false);
                    if (ro != null) { return ro; }
                }
                count += 1;

                _logs.DebugMsg($"[VcpCorePlugin] GetVcp2Steps retry ({count})");
                Thread.Sleep(1000);
            } while (count < 3);

            _logs.DebugMsg($"[VcpCorePlugin] GetVcp2Steps return null");
            return null;
        }

        private byte TranslatorVCPctrCode(string str)
        {
            byte rc = default(byte);

            if (VcpCodeList.VCPctr.ContainsKey(str))
                VcpCodeList.VCPctr.TryGetValue(str, out rc);

            return rc;
        }

        private uint TranslatorVCPcategory(string category, string str)
        {
            uint rc = default(uint);

            if (category.Equals(@"USB-C Prioritization"))
            {
                if (VcpCodeList.VCPF8.ContainsKey(str))
                    VcpCodeList.VCPF8.TryGetValue(str, out rc);
            }
            else if (category.Equals(@"Input Select"))
            {
                if (VcpCodeList.VCP60.ContainsKey(str))
                    VcpCodeList.VCP60.TryGetValue(str, out rc);
            }

            return rc;
        }

        private void OnCacheTimedRaise(System.Object source, System.Timers.ElapsedEventArgs e)
        {
            _logs.DebugMsg("[VcpCorePlugin] [Hook] OnCacheTimedRaise");

            if (_AllInfoMonitors.Count > 0)
                Watcher0x52();
            else
                _logs.DebugMsg("[VcpCorePlugin] [OnCacheTimedRaise] No monitors to service");
        }

        private List<MonitorInfo_complex> _GetMonitors(CancellationToken token)
        {
            _logs.DebugMsg("[VcpCorePlugin] VcpCorePlugin into _GetMonitors() ...");

            try
            {
                List<MonitorInfo_complex> monitors = new List<MonitorInfo_complex>();
                //monitors.Clear(); //Dean 0626 fix SAST issue, remove this line since the object just created and it's empty

                int MoIndexCounter = 0;

                bool _Get_Monitors(IntPtr hMonitor, IntPtr hdcMonitor, ref Rectangle lprcMonitor, IntPtr dwData)
                {
                    try
                    {
                        token.ThrowIfCancellationRequested();  //*****EXTRA CHECK*****//

                        _logs.DebugMsg("[VcpCorePlugin] _Get_Monitors collection start ...");

                        System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
                        watch.Start();
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
                        for (int jj = 0; _EnumDisplayDevices(DeviceName, (uint)jj, ref dd, 0); jj++)
                        {
                            token.ThrowIfCancellationRequested();  //*****EXTRA CHECK*****//

                            if ((dd.StateFlags & DisplayDeviceStateFlags.AttachedToDesktop) == 0)
                                continue;

                            realindex++;

                            DEVMODE devmode = new DEVMODE();
                            bool success = _EnumDisplaySettings(DeviceName, ENUM_CURRENT_SETTINGS, ref devmode);
                            MonitorInfo_complex _TargetMonitor = new MonitorInfo_complex();

                            _TargetMonitor.DisplayName = DeviceName;
                            _TargetMonitor.hMonitor = hMonitor;
                            _TargetMonitor.pMonitorInfoEx = info;
                            _TargetMonitor.Handle = hdcMonitor;
                            _TargetMonitor.pDevmode = devmode;
                            _TargetMonitor.displaydevice = dd;
                            _TargetMonitor.hPhysicalMonitor = pPhysicalMonitors[realindex].hPhysicalMonitor;
                            _TargetMonitor.szPhysicalMonitorDescription = pPhysicalMonitors[realindex].szPhysicalMonitorDescription;
                            _TargetMonitor.ColorPresentDescription = new Dictionary<string, Dictionary<string, string>>();
                            _TargetMonitor.UnDefinedColorPreset = new List<string>();

                            EDID edid = new EDID();

                            int nCount = 0;
                            bool blGetEdidPass = false;
                            while (true)
                            {
                                token.ThrowIfCancellationRequested();  //*****EXTRA CHECK*****//

                                Thread thread = new Thread(() =>
                                {
                                    blGetEdidPass = CommonFun.getEDID(dd.DeviceID, ref edid);
                                });

                                thread.Start();
                                thread.Join(); //wait for the thread to finish

                                if (blGetEdidPass)
                                    break;

                                nCount++;

                                if (nCount > 3)
                                    break;
                            }

                            if (blGetEdidPass)
                            {
                                if (!string.IsNullOrWhiteSpace(edid.ManufactureID))
                                {
                                    if (edid.ManufactureID.ToUpper() == "DEL")
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
                                                    _TargetMonitor.AliasDeviceName = edid.ModelName;
                                            }
                                            else
                                                _TargetMonitor.AliasDeviceName = dd.DeviceString;
                                        }
                                        else
                                        {
                                            if (!string.IsNullOrWhiteSpace(edid.ModelName))
                                                _TargetMonitor.AliasDeviceName = edid.ModelName;
                                        }

                                        if (dd.DeviceID.ToUpper().Contains("DEL"))
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
                                            _TargetMonitor.AliasDeviceName = edid.ModelName;
                                        else
                                            _TargetMonitor.AliasDeviceName = dd.DeviceString;

                                        if (_IsOnlyGetDellMontor)
                                            continue;
                                    }
                                }
                            }
                            else
                            {
                                if (_IsOnlyGetDellMontor)
                                    continue;
                            }

                            //--------------------------------------------------------------------
                            if (true) //getAllData
                            {
                                System.Diagnostics.Stopwatch watch1 = new System.Diagnostics.Stopwatch();
                                watch1.Start();
                                int nRetryCount = 0;
                                do
                                {
                                    token.ThrowIfCancellationRequested();  //*****EXTRA CHECK*****//

                                    var ro = GetFromCacheTable(new MonitorInfo_complex() { edid = _TargetMonitor.edid, AliasDeviceName = _TargetMonitor.AliasDeviceName }, "CapibilityString");
                                    if (ro != null)
                                    {
                                        _TargetMonitor.CapabilityString = ro.ToString();
                                        break;
                                    }
                                    else
                                    {
                                        _TargetMonitor.CapabilityString = GetCapabilities_String(_TargetMonitor.hPhysicalMonitor, token);
                                        if (!string.IsNullOrWhiteSpace(_TargetMonitor.CapabilityString))
                                            break;
                                    }

                                    nRetryCount++;
                                } while (_TargetMonitor.IsDellMonitor && (nRetryCount <= 2));

                                ITokenizer tokenizer = new CapabilitiesTokenizer();
                                IParser parser = new CapabilitiesParser();
                                INodeFormatter formatter = new NodeFormatter();

                                var tokens = tokenizer.GetTokens(_TargetMonitor.CapabilityString);
                                var node = parser.Parse(tokens);

                                if (!string.IsNullOrWhiteSpace(_TargetMonitor.CapabilityString) && (node.Nodes.Count() > 0))
                                {
                                    var modelNode = node.Nodes.RecursiveSelect(n => n.Nodes).Single(n => n.Value == "model");
                                    foreach (var _node in modelNode.Nodes)
                                        _TargetMonitor.modelName = _node.Value;

                                    _logs.DebugMsg("[VcpCorePlugin] _TargetMonitor.modelName: " + _TargetMonitor.modelName);
                                    _logs.DebugMsg("[VcpCorePlugin] _TargetMonitor.MarketingName: " + _TargetMonitor.MarketingName);

                                    _TargetMonitor.series = string.Empty;
                                    foreach (KeyValuePair<string, List<modelinfos>> kv in _SupportDictionary)
                                    {
                                        foreach (var tx in kv.Value)
                                        {
                                            if (tx.ModelName.Equals(_TargetMonitor.modelName))
                                            {
                                                _TargetMonitor.series = kv.Key;
                                                _TargetMonitor.ImageFileName = tx.ImageFileName;
                                                _TargetMonitor.MarketingName = tx.MarketingName;
                                                break;
                                            }
                                        }
                                    }
                                    _logs.DebugMsg("[VcpCorePlugin] _TargetMonitor.series: " + _TargetMonitor.series);
                                }

                                if (string.IsNullOrWhiteSpace(_TargetMonitor.series))
                                {
                                    if (CheckIsSupportDisplayByBit(_TargetMonitor.hPhysicalMonitor, _TargetMonitor.modelName))
                                    {
                                        _TargetMonitor.series = ChekSeries(_TargetMonitor.modelName);
                                        _logs.DebugMsg("[VcpCorePlugin] _TargetMonitor.series: " + _TargetMonitor.series);
                                    }
                                    else
                                        continue;
                                }

                                //_TargetMonitor.CapabilityString = "(prot(monitor)type(lcd)model(Z24nf)cmds(01 02 03 07 0C E3 F3)vcp(02 04 05 08 0B 0C 10 14(01 02 04 05 08 0B) 16 18 1A 52 60(03 0F 10 11) 6C 6E 70 87 AA(01 02 03 04) AC AE B2 B6 C0 C6 C8 C9 CA(01 02) CC(01 02 03 04 05 06 08 0A 0D 14) D6(01 02 03 04 05) DA(00 02 ) DF E9(00 01) EA(00 01) EB(00 01) EF(01 02 03 04 05) F0 FA(00 01 02) FB FC FD FE(00 01 02 04) )mswhql(1)asset_eep(40)mccs_ver(2.2))";

                                _TargetMonitor.ColorPresetSupportList = new List<string>();
                                _TargetMonitor.CapabilityDic = new Dictionary<string, List<string>>();

                                if (!string.IsNullOrWhiteSpace(_TargetMonitor.CapabilityString))
                                {
                                    _TargetMonitor.DDCisON = true;
                                    nRetryCount = 0;
                                    int nRetryCount2 = 0;
                                    while (true)
                                    {
                                        token.ThrowIfCancellationRequested();  //*****EXTRA CHECK*****//

                                        if (GetAllCapabilityData(_TargetMonitor.CapabilityString, ref _TargetMonitor.CapabilityDic))
                                        {
                                            if (_TargetMonitor.IsDellMonitor)
                                            {
                                                //List<string> e2List = GetE2SupportStrings(_TargetMonitor.CapabilityDic);
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
                                            _TargetMonitor.CapabilityString = GetCapabilities_String(_TargetMonitor.hPhysicalMonitor, token);
                                            if (nRetryCount >= 3)
                                                break;

                                            continue;
                                        }

                                        watch1.Stop();
                                        watch1.Restart();

                                        if (!GetAllColorPreset(_TargetMonitor.CapabilityString, ref _TargetMonitor.ColorPresentDescription, ref _TargetMonitor.UnDefinedColorPreset, _TargetMonitor.IsDellMonitor))
                                        {
                                            nRetryCount2++;
                                            _TargetMonitor.CapabilityString = GetCapabilities_String(_TargetMonitor.hPhysicalMonitor, token);

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
                                    watch1.Stop();
                                }
                            }

                            _TargetMonitor.Index = MoIndexCounter;

                            if ((!string.IsNullOrWhiteSpace(_TargetMonitor.series)) && (!string.IsNullOrWhiteSpace(_TargetMonitor.CapabilityString)))
                            {
                                var FwTmp = FwVersion(_TargetMonitor.hPhysicalMonitor, _TargetMonitor.modelName);
                                _TargetMonitor.FwVersion = FwTmp.Item1;
                                _TargetMonitor.D_Ctrl = FwTmp.Item2;
                                _TargetMonitor.SupplierID = FwTmp.Item3;

                                monitors.Add(_TargetMonitor);
                                MoIndexCounter++;
                            }
                        }
                        watch.Stop();
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

                if (token.IsCancellationRequested || (!_EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, _Get_Monitors, IntPtr.Zero)))
                    throw new Win32Exception(Marshal.GetLastWin32Error());

                _logs.DebugMsg("[VcpCorePlugin] VcpCorePlugin exit _GetMonitors() ...");
                return monitors;
            }
            catch (Exception e)
            {
                _logs.DebugMsg("[VcpCorePlugin] _GetMonitors() happened exception : " + e.Message);

                return new List<MonitorInfo_complex>();
            }
        }

        private bool getE2SupportStrings(Dictionary<string, List<string>> CapDic, out List<string> e2List)
        {
            bool blResult = false;
            //List<string> e2List = new List<string>();
            e2List = new List<string>();

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

        private List<string> GetSmartHdrStrings(Dictionary<string, List<string>> CapDic)
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

        private (string, string) GetInputSource(MonitorInfo_complex monitorInfo_)
        {
            string rc_H = string.Empty;
            string rc_L = string.Empty;
            int count = 0;
            do
            {
                var value = Get_VCPCapability(monitorInfo_, 0x60, 0, true);
                if (value != null)
                {
                    uint val_H = (Convert.ToUInt32(value) >> 8) & 0XFF;
                    uint val_L = Convert.ToUInt32(value) & 0XFF;

                    rc_H = val_H.ToString("X2");
                    rc_L = val_L.ToString("X2");

                    if (!string.IsNullOrWhiteSpace(rc_H) && !string.IsNullOrWhiteSpace(rc_L))
                    {
                        SetToCacheTable(monitorInfo_, 0x60, value);

                        List<InputSourceObject> r = new List<InputSourceObject>();
                        var R = GetFromCacheTable(monitorInfo_, "inputsourcelist");
                        if (R != null)
                            r.AddRange(R as List<InputSourceObject>);

                        var n_H = NodeFormatter.FormatVCP_60(rc_H.ToLower());
                        var n_L = NodeFormatter.FormatVCP_60(rc_L.ToLower());
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

                        n_H = (rv_H) ? n_H : (System.Text.RegularExpressions.Regex.Replace(n_H, @"\d", string.Empty));
                        n_L = (rv_L) ? n_L : (System.Text.RegularExpressions.Regex.Replace(n_L, @"\d", string.Empty));

                        return (n_H, n_L);
                    }
                }

                count++;
                _logs.DebugMsg($"[VcpCorePlugin] GetInputSource retry ({count})");
                Thread.Sleep(1000);
            } while (count < 3);

            _logs.DebugMsg($"[VcpCorePlugin] GetInputSource return string.Empty");
            return (string.Empty, string.Empty);
        }

        private bool Set_VCPCapability(MonitorInfo_complex monitorInfoX, byte code, uint val, bool retry = true)
        {
            int count = 0;

            do
            {
                bool rc = _SetVCPFeature(monitorInfoX.hPhysicalMonitor, code, val);

                if (rc)
                {
                    VCPchangedEventArgs _VCPchangedEventArgs = new VCPchangedEventArgs();
                    _VCPchangedEventArgs.vcpcode = code.ToString("X");
                    _VCPchangedEventArgs.value = val.ToString();
                    _VCPchangedEventArgs.monitor = new MonitorInfo()
                    {
                        AliasDeviceName = monitorInfoX.AliasDeviceName,
                        IsDellMonitor = monitorInfoX.IsDellMonitor,
                        Index = monitorInfoX.Index,
                        CapabilityString = monitorInfoX.CapabilityString,
                        DDCisON = monitorInfoX.DDCisON,
                        DisplayName = monitorInfoX.DisplayName,
                        edid = monitorInfoX.edid,
                        FwVersion = monitorInfoX.FwVersion,
                        inputSource = monitorInfoX.inputSource,
                        inputCable = monitorInfoX.inputCable,
                        CapabilityDic = monitorInfoX.CapabilityDic,
                        modelName = monitorInfoX.modelName,
                        series = monitorInfoX.series,
                        MarketingName = monitorInfoX.MarketingName,
                        ImageFileName = monitorInfoX.ImageFileName,
                        SupplierID = monitorInfoX.SupplierID,
                        D_Ctrl = monitorInfoX.D_Ctrl,
                    };
                    OnVCPchanged(_VCPchangedEventArgs);

                    return rc;
                }

                count++;
                _logs.DebugMsg($"[VcpCorePlugin] Set_VCPCapability retry ({count})");
                Thread.Sleep(1000);
            } while (count < 3 && retry);

            _logs.DebugMsg($"[VcpCorePlugin] Set_VCPCapability return false");
            return false;
        }

        private object Get_VCPCapability(MonitorInfo_complex monitorInfoX, byte code, int opt, bool retry = true)
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
                _logs.DebugMsg($"[VcpCorePlugin] Get_VCPCapability " + Convert.ToString(code, 16) ?? string.Empty + " , retry ( " + count.ToString() + " )");
                Thread.Sleep(1000);
            } while (count < 3 && retry);

            _logs.DebugMsg($"[VcpCorePlugin] Get_VCPCapability return null");
            return null;
        }

        private static bool GetColorPresetVcpCode(Dictionary<string, Dictionary<string, string>> ColorPresetHash, string presetName, ref ColorPreset outColorPreset)
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
                    Trace.Write($" presetName = {presetName}");

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

            Trace.Write($" GetResult = {GetResult}");

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

        private static bool GetColorPresetVcpCodeForNonDell(Dictionary<string, Dictionary<string, string>> ColorPresetHash, string presetName, ref ColorPreset outColorPreset)
        {
            bool GetResult = false;
            string GetPresetValue = string.Empty;
            if (ColorPresetHash.ContainsKey("14"))
            {
                outColorPreset.vcpode = VcpCode.ColorSpace14;
                Dictionary<string, string> myresources = new Dictionary<string, string>();
                bool TF_Boolean = ColorPresetHash.TryGetValue("14", out myresources);
                if (TF_Boolean)
                {
                    if (myresources.ContainsKey(presetName))
                    {
                        GetPresetValue = myresources[presetName].ToString();
                        GetResult = true;
                        outColorPreset.codeValue = Convert.ToUInt32(GetPresetValue, 16);
                    }
                }
            }
            return GetResult;
        }

        private string GetCurrentColorPreset(MonitorInfo_complex monitor)
        {
            string rc = string.Empty;
            int count = 0;
            do
            {
                object value = new object();

                // 20241014 jim add
                value = null;
                //value = GetFromCacheTable(monitor, Convert.ToByte(0xE2));

                if (value == null)
                    value = Get_VCPCapability(monitor, 0xE2, 0, true);

                if (value != null)
                {
                    uint val = (Convert.ToUInt32(value) & 0XFFFF);
                    string valstring = val.ToString("X2");

                    if (valstring.Length >= 2)
                    {
                        int pos = valstring.Length - 2;
                        rc = valstring.Substring(pos);
                    }

                    Trace.WriteLine("GetCurrentColorPreset()  valstring= " + valstring);
                    Trace.WriteLine("GetCurrentColorPreset()  rc= " + rc);
                }
                if (!string.IsNullOrWhiteSpace(rc))
                    return NodeFormatter.FormatVCP_E2(rc.ToLower());

                count++;
                _logs.DebugMsg($"[VcpCorePlugin] GetCurrentColorPreset retry ({count})");
                Thread.Sleep(1000);
            } while (count < 3);

            _logs.DebugMsg($"[VcpCorePlugin] GetCurrentColorPreset return string.Empty");
            return rc;
        }

        public bool SetColorPreset(MonitorInfo_complex mi, string presetName)
        {
            bool SetResult = false;
            ColorPreset newpreset = new ColorPreset();

            if (mi.IsDellMonitor)
            {
                if (GetColorPresetVcpCode(mi.ColorPresentDescription, presetName, ref newpreset))
                {
                    if (newpreset.vcpode != VcpCode.ColorSpaceE2)
                        SetResult = Set_VCPCapability(mi, Convert.ToByte(newpreset.vcpode), newpreset.codeValue, true);
                    //SetResult = SetVCPFeature(mi.hPhysicalMonitor, Convert.ToByte(newpreset.vcpode), newpreset.codeValue);
                    else
                        SetResult = false;
                }
            }
            else
            {
                if (GetColorPresetVcpCodeForNonDell(mi.ColorPresentDescription, presetName, ref newpreset))
                    SetResult = Set_VCPCapability(mi, Convert.ToByte(newpreset.vcpode), newpreset.codeValue, true);
                //SetResult = SetVCPFeature(mi.hPhysicalMonitor, Convert.ToByte(newpreset.vcpode), newpreset.codeValue);
            }
            return SetResult;
        }

        private string GetCapabilities_String(IntPtr hPhysicalMonitor, CancellationToken token)
        {
            int count = 0;

            var Cancellation = CancellationTokenSource.CreateLinkedTokenSource(token);
            var NewToken = Cancellation.Token;

            try
            {
                NewToken.ThrowIfCancellationRequested();

                do
                {
                    NewToken.ThrowIfCancellationRequested();

                    try
                    {
                        bool capabilitiesStringLength = false;
                        int num = 4;
                        uint length = 0;
                        capabilitiesStringLength = _GetCapabilitiesStringLength(hPhysicalMonitor, out length);
                        while (!capabilitiesStringLength && num > 0)
                        {
                            NewToken.ThrowIfCancellationRequested();

                            _logs.DebugMsg($"[VcpCorePlugin] GetCapabilitiesStringLength error ({_GetLastError()})");
                            num--;
                            Thread.Sleep(250 * (4 - num));
                        }

                        if (capabilitiesStringLength)
                        {
                            num = 4;
                            var sb = new StringBuilder((int)length);
                            while (!_CapabilitiesRequestAndCapabilitiesReply(hPhysicalMonitor, sb, (uint)sb.Capacity) && num > 0)
                            {
                                NewToken.ThrowIfCancellationRequested();

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
                    }
                    catch (TaskCanceledException)
                    {
                        // Task was canceled before running.
                        // Cancelled due to timeout

                        _logs.DebugMsg("[VcpCorePlugin] GetCapabilities_StringI() cancellation happened ...");
                        return string.Empty;
                    }
                    catch (OperationCanceledException)
                    {
                        // Task was canceled while running.
                        // Cancelled due to timeout

                        _logs.DebugMsg("[VcpCorePlugin] GetCapabilities_StringI() cancellation happened ...");
                        return string.Empty;
                    }
                    catch (Exception)
                    {
                        _logs.DebugMsg($"[VcpCorePlugin] GetCapabilities_StringI() Exception ({_GetLastError()})");
                        return string.Empty;
                    }
                } while (count < 4);
            }
            catch (TaskCanceledException)
            {
                // Task was canceled before running.
                // Cancelled due to timeout

                _logs.DebugMsg("[VcpCorePlugin] GetCapabilities_StringII() cancellation happened...");
                return string.Empty;
            }
            catch (OperationCanceledException)
            {
                // Task was canceled while running.
                // Cancelled due to timeout

                _logs.DebugMsg("[VcpCorePlugin] GetCapabilities_StringII() cancellation happened...");
                return string.Empty;
            }
            catch (Exception ex)
            {
                _logs.DebugMsg("[VcpCorePlugin] GetCapabilities_StringII() AllInfoMonitors Exception is " + ex.Message);
                return string.Empty;
            }

            _logs.DebugMsg($"[VcpCorePlugin] GetCapabilities_String return string.Empty");
            return string.Empty;
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
            AliasDeviceName = devicedescription;
            if (devicedescription.ToUpper().Contains("DELL"))
            {
                if (devicedescription.ToUpper().Split(' ').Length > 1)
                {
                    foreach (string tmp in devicedescription.ToUpper().Split(' '))
                    {
                        if (tmp.StartsWith("AW"))
                            AliasDeviceName = "Alienware " + tmp;
                    }
                }
                else
                {
                    string dell = "DELL";
                    int nStartIndex = devicedescription.ToUpper().IndexOf(dell) + dell.Length;

                    string tmp = devicedescription.Substring(nStartIndex);

                    if (tmp.StartsWith("AW"))
                        AliasDeviceName = "Alienware " + tmp;
                }
            }
        }

        private static PHYSICAL_MONITOR[] GetPhysicalMonitors(IntPtr hMonitor)
        {
            uint dwNumberOfPhysicalMonitors = 0;

            if (!_GetNumberOfPhysicalMonitorsFromHMONITOR(hMonitor, ref dwNumberOfPhysicalMonitors))
                throw new Win32Exception(Marshal.GetLastWin32Error());

            PHYSICAL_MONITOR[] physicalMonitorArray = new PHYSICAL_MONITOR[dwNumberOfPhysicalMonitors];
            if (!_GetPhysicalMonitorsFromHMONITOR(hMonitor, dwNumberOfPhysicalMonitors, physicalMonitorArray))
                throw new Win32Exception(Marshal.GetLastWin32Error());

            return physicalMonitorArray;
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
            catch (Exception) { return false; }
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
            VCPF0.Add("DCI-P3", "0A");
            VCPF0.Add("Display P3", "A1");
            VCPF0.Add("Rec.2020 / BT.2020", "0B"); // add 10/04
            VCPF0.Add("Rec.2020", "0B");
            VCPF0.Add("BT.2020", "0B");
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

            Dictionary<string, string> VCP14 = new Dictionary<string, string>();
            VCP14.Add("sRGB", "01"); ;
            VCP14.Add("5000K", "04"); ;
            VCP14.Add("5700K", "0B"); ;
            VCP14.Add("Warm", "0B"); ;
            VCP14.Add("6500K", "05"); ;
            VCP14.Add("7500K", "06"); ;
            VCP14.Add("9300K", "08"); ;
            VCP14.Add("Cool", "08"); ;
            VCP14.Add("10000K", "09"); ;
            VCP14.Add("Custom Color", "0C"); ;

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
            VCPE2.Add("Presets Disabled", "7F");

            #endregion

            _ColorPresets.Add("DC", VCPDC);
            _ColorPresets.Add("14", VCP14);
            _ColorPresets.Add("F0", VCPF0);
            _ColorPresets.Add("E2", VCPE2);
        }

        private List<string> GetColorPresetStrings(Dictionary<string, List<string>> CapDic, List<string> E2List)
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
                                if (!string.IsNullOrWhiteSpace(s))
                                {
                                    if (E2List.Contains(s))
                                        Newlist.Add(s);
                                }
                            }
                        }
                    }
                }
            }
            return Newlist;
        }

        private bool GetAllColorPreset(string CablitityString, ref Dictionary<string, Dictionary<string, string>> SupportColorPresets, ref List<string> UnDefinedColorPresets, bool isDellMonitor = true)
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
            catch (Exception) { return hasrestult; }

            return hasrestult;
        }

        private List<string> GetColorPresetDescriptionByValue(string ResourceName, string ColorPresetValue)
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

        private bool CheckIsSupportDisplayByBit(IntPtr hPhyMonitor, string model)
        {
            try
            {
                int count = 0;
                object F1supportBit = null;
                do
                {
                    if (F1supportBit == null)
                        F1supportBit = Get_VCPCapability(new MonitorInfo_complex() { hPhysicalMonitor = hPhyMonitor }, 0xF1, 0, true);

                    if (F1supportBit != null)
                    {
                        uint r = ((Convert.ToUInt32(F1supportBit)) & 0xA000);
                        if (r > 0)
                        {
                            _logs.DebugMsg($"[VcpCorePlugin] CheckIsSupportDisplayByBit return true");
                            return true;
                        }
                        else
                        {
                            uint rr = ((Convert.ToUInt32(F1supportBit)) & 0x0001);
                            int CY = ChekCY(model);

                            if (CY >= 19 && rr > 0)
                            {
                                _logs.DebugMsg($"[VcpCorePlugin] CheckIsSupportDisplayByBit return true");
                                return true;
                            }
                            else
                            {
                                _logs.DebugMsg($"[VcpCorePlugin] CheckIsSupportDisplayByBit return false");
                                return false;
                            }
                        }
                    }

                    count++;
                    _logs.DebugMsg($"[VcpCorePlugin] CheckIsSupportDisplayByBit retry ({count})");
                    Thread.Sleep(1000);
                } while (count < 3);

                _logs.DebugMsg($"[VcpCorePlugin] CheckIsSupportDisplayByBit return false");
                return false;
            }
            catch (Exception ex)
            {
                _logs.DebugMsg($"[VcpCorePlugin] CheckIsSupportDisplayByBit into catch: " + ex.Message);
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

            switch (Series.Trim().ToUpper())
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

        private (string, string, string) FwVersion(IntPtr hPhyMonitor, string modelName)
        {
            int count = 0;
            var Version = (string.Empty, string.Empty, string.Empty);
            object OFWstring = null;
            object ScalarICID = null;
            object OEMID = null;
            do
            {
                if (OFWstring == null)
                    OFWstring = Get_VCPCapability(new MonitorInfo_complex() { hPhysicalMonitor = hPhyMonitor }, Convert.ToByte(VcpCode.Version), 0, true);
                if (ScalarICID == null)
                    ScalarICID = Get_VCPCapability(new MonitorInfo_complex() { hPhysicalMonitor = hPhyMonitor }, 0xC8, 0, true);
                if (OEMID == null)
                    OEMID = Get_VCPCapability(new MonitorInfo_complex() { hPhysicalMonitor = hPhyMonitor }, 0xFD, 0, true);

                if ((OFWstring != null) && (ScalarICID != null) && (OEMID != null))
                {
                    Version = FormatFwVersion(Convert.ToUInt32(OFWstring), Convert.ToUInt32(ScalarICID), Convert.ToUInt32(OEMID), WhichCase(modelName));

                    if (!string.IsNullOrWhiteSpace(Version.Item1))
                    {
                        _logs.DebugMsg("[VcpCorePlugin] FwVersion 0XC8 : 0X" + Convert.ToUInt32(ScalarICID).ToString("X"));
                        _logs.DebugMsg("[VcpCorePlugin] FwVersion 0XC9 : 0X" + Convert.ToUInt32(OFWstring).ToString("X"));
                        _logs.DebugMsg("[VcpCorePlugin] FwVersion 0XFD : 0X" + Convert.ToUInt32(OEMID).ToString("X"));
                        _logs.DebugMsg("[VcpCorePlugin] FwVersion : " + Version.Item1);
                        return Version;
                    }
                }

                count++;
                _logs.DebugMsg($"[VcpCorePlugin] FwVersion retry ({count})");
                Thread.Sleep(1000);
            } while (count < 3);

            _logs.DebugMsg($"[VcpCorePlugin] FwVersion return string.empty");
            return Version;
        }

        private (string, string, string) FormatFwVersion(uint fwVersion, uint ScalarICID, uint OEMID, int _Case)
        {
            string str_fwVersion = string.Empty;
            string str_ScalarICID = (ScalarICID & 0xff).ToString("X2");
            string str_OEMID = Encoding.ASCII.GetString(new byte[] { Convert.ToByte(OEMID) }).ToUpper();

            switch (_Case)
            {
                case 1:
                    {
                        _logs.DebugMsg("[VcpCorePlugin] FwVersion into Case 01 [BCD BCD]");
                        str_fwVersion = fwVersion.ToString("X4");
                    }
                    break;

                case 2:
                    {
                        _logs.DebugMsg("[VcpCorePlugin] FwVersion into Case 02 [HEX HEX]");
                        uint tempI = (fwVersion & 0xff00) >> 8;
                        uint tempII = (fwVersion & 0xff);
                        string strI = tempI.ToString("X2");
                        string strII = tempII.ToString("X2");
                        int valI = Convert.ToInt32(strI, 16);
                        int valII = Convert.ToInt32(strII, 16);
                        strI = valI.ToString().PadLeft(2, '0');
                        strII = valII.ToString().PadLeft(2, '0');
                        str_fwVersion = strI + strII;
                    }
                    break;

                case 3:
                    {
                        _logs.DebugMsg("[VcpCorePlugin] FwVersion into Case 03 [HEX BCD]");
                        uint tempI = (fwVersion & 0xff00) >> 8;
                        uint tempII = (fwVersion & 0xff);
                        string strI = tempI.ToString("X2");
                        string strII = tempII.ToString("X2");
                        int valI = Convert.ToInt32(strI, 16);
                        strI = valI.ToString().PadLeft(2, '0');
                        str_fwVersion = strI + strII;
                    }
                    break;

                case 4:
                    {
                        _logs.DebugMsg("[VcpCorePlugin] FwVersion into Case 04 [BCD HEX]");
                        uint tempI = (fwVersion & 0xff00) >> 8;
                        uint tempII = (fwVersion & 0xff);
                        string strI = tempI.ToString("X2");
                        string strII = tempII.ToString("X2");
                        int valII = Convert.ToInt32(strII, 16);
                        strII = valII.ToString().PadLeft(2, '0');
                        str_fwVersion = strI + strII;
                    }
                    break;

                default:
                    {
                        _logs.DebugMsg("[VcpCorePlugin] FwVersion into Case default");
                        str_fwVersion = fwVersion.ToString("X4");
                    }
                    break;
            }

            _logs.DebugMsg("[VcpCorePlugin] FwVersion str_fwVersion is " + str_fwVersion);
            _logs.DebugMsg("[VcpCorePlugin] FwVersion str_ScalarICID is " + str_ScalarICID);
            _logs.DebugMsg("[VcpCorePlugin] FwVersion str_OEMID is " + str_OEMID);

            string SupplierID = string.Empty;
            switch (str_OEMID)
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

            string D_Ctrl = string.Empty;
            string hexValue = str_ScalarICID;
            int nLen = hexValue.Length;
            if (nLen > 0)
            {
                switch (hexValue.ToLower())
                {
                    case "05":
                        D_Ctrl = "Mediatek";
                        str_ScalarICID = "2";
                        break;

                    case "09":
                        D_Ctrl = "Realtek";
                        str_ScalarICID = "3";
                        break;

                    case "0d":
                        D_Ctrl = "STM";
                        str_ScalarICID = "1";
                        break;

                    case "12":
                        D_Ctrl = "Novatek";
                        str_ScalarICID = "4";
                        break;

                    case "ff":
                        D_Ctrl = "Nvidia";
                        str_ScalarICID = "0";
                        break;

                    case "00":
                        D_Ctrl = "Nvidia";
                        str_ScalarICID = "0";
                        break;

                    default:
                        D_Ctrl = "Realtek";
                        str_ScalarICID = "3";
                        break;
                }
            }

            hexValue = str_fwVersion;
            nLen = hexValue.Length;
            if (nLen > 1)
            {
                string strFirst = hexValue.Substring(0, 1);
                switch (strFirst)
                {
                    case "1":
                        {
                            strFirst = strFirst + str_ScalarICID + str_OEMID;
                            string strSecond = hexValue.Substring(1);
                            str_fwVersion = strFirst + strSecond;
                        }
                        break;

                    case "2":
                        {
                            strFirst = strFirst + str_ScalarICID + str_OEMID;
                            string strSecond = hexValue.Substring(1);
                            str_fwVersion = strFirst + strSecond;
                        }
                        break;

                    case "3":
                        {
                            strFirst = strFirst + str_ScalarICID + str_OEMID;
                            string strSecond = hexValue.Substring(1);
                            str_fwVersion = strFirst + strSecond;
                        }
                        break;

                    case "4":
                        {
                            strFirst = "M" + str_ScalarICID + str_OEMID;
                            string strSecond = hexValue.Substring(1);
                            str_fwVersion = strFirst + strSecond;
                        }
                        break;

                    default:
                        break;
                }
            }

            return (str_fwVersion, D_Ctrl, SupplierID);
        }

        private int WhichCase(string Model)
        {
            switch (Model)
            {
                case "AW2724DM": return 1;
                case "AW2723DF": return 4;
                case "AW2523HF": return 5;
                case "AW3423DWF": return 4;
                case "AW2521HF": return 4;
                case "AW2521HFA": return 4;
                case "AW2521HFL": return 4;
                case "AW2521HFLA": return 4;
                case "AW2720HF": return 4;
                case "AW2720HFA": return 4;
                case "AW5520QF": return 4;
                case "C2422HE": return 1;
                case "C2722DE": return 1;
                case "C3422WE": return 2;
                case "C5519Q": return 1;
                case "C5519QA": return 1;
                case "C5522QT": return 5;
                case "C6522QT": return 5;
                case "C7520QT": return 5;
                case "C8621QT": return 5;
                case "C2423H": return 4;
                case "C2424HE": return 1;
                case "C2724DE": return 1;
                case "C2723H": return 4;
                case "C3424WE": return 1;
                case "G2722HS": return 1;
                case "G2422HS": return 4;
                case "S2422HG": return 5;
                case "S2422HGA": return 5;
                case "S2522HG": return 4;
                case "S2722DGM": return 1;
                case "S3422DWG": return 1;
                case "S3222HG": return 1;
                case "S3222DGM": return 1;
                case "S2421HGF": return 4;
                case "S2721HGF": return 4;
                case "S2721HGFA": return 4;
                case "S2721DGFA": return 5;
                case "S2721DGF": return 5;
                case "S2719DGF": return 1;
                case "S3220DGF": return 1;
                case "S2419HGF": return 1;
                case "G2723H": return 1;
                case "G2723HN": return 1;
                case "G3223D": return 1;
                case "G3223Q": return 1;
                case "E1715S": return 1;
                case "E1916HV": return 5;
                case "E1920H": return 2;
                case "E2016HV": return 5;
                case "E2020H": return 4;
                case "E2219HN": return 5;
                case "E2220H": return 2;
                case "E2221HN": return 2;
                case "E2222H": return 2;
                case "E2222HS": return 2;
                case "E2420H": return 2;
                case "E2420HS": return 2;
                case "E2421HN": return 2;
                case "E2422H": return 2;
                case "E2422HN": return 2;
                case "E2422HS": return 2;
                case "E2720H": return 4;
                case "E2720HS": return 4;
                case "E2722H": return 1;
                case "E2722HS": return 1;
                case "E2223HN": return 2;
                case "E2223HV": return 2;
                case "E2423H": return 1;
                case "E2424HS": return 2;
                case "E2423HN": return 1;
                case "E2723H": return 2;
                case "E2723HN": return 1;
                case "E2724HN": return 2;
                case "P1917S": return 1;
                case "P2219H": return 2;
                case "P2219HC": return 4;
                case "P2222H": return 1;
                case "P2319H": return 1;
                case "P2418HT": return 5;
                case "P2419H": return 1;
                case "P2419HC": return 4;
                case "P2421": return 2;
                case "P2421D": return 1;
                case "P2421DC": return 1;
                case "P2422H": return 2;
                case "P2422HE": return 1;
                case "P2719H": return 2;
                case "P2719HC": return 2;
                case "P2720D": return 1;
                case "P2720DC": return 1;
                case "P2721Q": return 5;
                case "P2722H": return 1;
                case "P2722HE": return 1;
                case "P3221D": return 2;
                case "P3222QE": return 1;
                case "P3421W": return 4;
                case "E2724HS": return 2;
                case "P2223HC": return 1;
                case "P2423": return 1;
                case "P2423D": return 1;
                case "P2423DE": return 2;
                case "P2723D": return 1;
                case "P2723DE": return 1;
                case "P2723QE": return 5;
                case "P3223DE": return 1;
                case "P3223QE": return 1;
                case "S3423DWC": return 1;
                case "S2723HC": return 1;
                case "S2422HZ": return 1;
                case "S2722DC": return 4;
                case "S2722DZ": return 1;
                case "S2722QC": return 4;
                case "S3222HN": return 1;
                case "S3222HS": return 1;
                case "S3422DW": return 1;
                case "S2421H": return 1;
                case "S2421HN": return 1;
                case "S2421HS": return 1;
                case "S2421HSX": return 1;
                case "S2421NX": return 1;
                case "S2721D": return 4;
                case "S2721DS": return 4;
                case "S2721H": return 1;
                case "S2721HN": return 1;
                case "S2721HS": return 1;
                case "S2721HSX": return 1;
                case "S2721NX": return 1;
                case "S2721Q": return 4;
                case "S2721QS": return 4;
                case "S2721QSA": return 4;
                case "S3221QS": return 1;
                case "S3221QSA": return 1;
                case "S2319H": return 1;
                case "S2319HN": return 1;
                case "S2319HS": return 2;
                case "S2319NX": return 1;
                case "S2419H": return 1;
                case "S2419HM": return 2;
                case "S2419HN": return 1;
                case "S2419NX": return 1;
                case "S2719DC": return 2;
                case "S2719DM": return 2;
                case "S2719H": return 1;
                case "S2719HN": return 1;
                case "S2719HS": return 2;
                case "S2719NX": return 1;
                case "S3219D": return 1;
                case "SE3223Q": return 1;
                case "SE2723DS": return 1;
                case "SE2423DS": return 1;
                case "SE2222H": return 2;
                case "SE2222HV": return 2;
                case "SE2422H": return 2;
                case "SE2422HM": return 2;
                case "SE2422HR": return 2;
                case "SE2422HX": return 2;
                case "SE2722H": return 1;
                case "SE2722HR": return 1;
                case "SE2722HX": return 1;
                case "SE2219H": return 2;
                case "SE2219HX": return 2;
                case "SE2419H": return 2;
                case "SE2419HX": return 2;
                case "SE2419HR": return 2;
                case "SE2719H": return 2;
                case "SE2719HX": return 2;
                case "SE2719HR": return 2;
                case "U3224KZ": return 1;
                case "U3223QZ": return 1;
                case "U4924DW": return 1;
                case "U3824DW": return 1;
                case "U3423WE": return 1;
                case "U4323QE": return 1;
                case "U3223QE": return 5;
                case "U2723QE": return 5;
                case "U3023E": return 1;
                case "U2422H": return 4;
                case "U2422HE": return 4;
                case "U2422HX": return 4;
                case "U2722D": return 1;
                case "U2722DE": return 1;
                case "U2722DX": return 1;
                case "U2421E": return 1;
                case "U2421HE": return 4;
                case "U2721DE": return 4;
                case "U3421WE": return 4;
                case "U3821DW": return 4;
                case "U4021QW": return 5;
                case "UP3221Q": return 4;
                case "U2520D": return 4;
                case "U2520DR": return 4;
                case "U2720Q": return 4;
                case "U2720QM": return 4;
                case "U4320Q": return 2;
                case "UP2720Q": return 4;
                case "UP2720QA": return 4;
                case "U2419H": return 5;
                case "U2419HC": return 4;
                case "U2419HS": return 4;
                case "U2419HX": return 4;
                case "U2719D": return 4;
                case "U2719DC": return 4;
                case "U2719DS": return 4;
                case "U2719DX": return 4;
                case "U3219Q": return 4;
                case "U3419W": return 4;
                case "U4919DW": return 4;
                case "U4919DWA": return 4;
                case "UP3218K": return 5;
                case "UP3218KA": return 5;
                case "U2723QX": return 5;
                default: return 1;
            }
        }

        private void Get_SupportListFile()
        {
            var path = System.AppDomain.CurrentDomain.BaseDirectory + targetFile;
            var r = DDPMFileSecurity.IsFilePathValid(path, out string log);
            _logs.DebugMsg("[VCPCore plugin] Get_SupportListFile IsFilePathValid : " + log);

            if (r)
            {
                using (FileLock fileLock = new FileLock(path, PathCheckOption.None, lockNow: true))
                {
                    if (File.Exists(path))
                        _SupportClassification = File.ReadAllText(targetFile);

                    if (!string.IsNullOrEmpty(_SupportClassification))
                        _SupportDictionary = JsonConvert.DeserializeObject<Dictionary<string, List<modelinfos>>>(_SupportClassification);
                }
            }
            else
                _logs.DebugMsg("[VCPCore plugin] Get_SupportListFile IsFilePathValid fail : " + log);
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