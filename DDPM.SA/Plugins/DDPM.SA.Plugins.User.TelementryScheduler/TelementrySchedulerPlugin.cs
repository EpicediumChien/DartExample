#region LicenceHeader

//
// Copyright © 2024, Dell Inc., All Rights Reserved.
// This material is confidential and a trade secret.  Permission to use this
// work for any purpose must be obtained in writing from Dell Inc.
//
// TelementryScheduler.cs created on 25/04/2024T07:20 PM
//

#endregion

using DDPM.SA.Common;
using DDPM.SA.Common.Settings;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.Common.Annotations;
using Dell.Client.Framework.Common.PluginConditions;
using Dell.Client.Framework.Interfaces;
using Microsoft;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VcpCore.Common;
using IDs = DDPM.SA.Common.IDs;

namespace DDPM.SA.Plugins.User.TelementryScheduler
{
    [Plugin(IDs.Telementry_Scheduler_Plugin_ID, pluginName, PluginOrderGroupType.Core, Version = pluginVersion)]
    [Descriptor(Description = pluginDescription)]
    [Publisher(Name = publisherCompany, Website = publisherWebsite, Support = publisherSupport)]
    [PublishedUnelevatedInterface(new[] { typeof(ITelementryScheduler) })]
    [DependencyKnownTypes(new[] { typeof(IPlatinumSDKService) })]
    [PluginRequires(Id = IDs.PlatinumSDK_Plugin, Version = "1.0.0", AllowDynamicResolving = true)]
    public class TelementrySchedulerPlugin : BaseAgentPlugin, IDisposableObservable, ITelementryScheduler
    {
        #region Private Members

        private const string pluginName = "TelementrySchedulerPlugin";
        private const string pluginVersion = "1.0.0";
        private const string pluginDescription = "This plugin implements Telementry Scheduler Plugin.";
        private const string publisherCompany = "Wistron";
        private const string publisherWebsite = "https://www.wistron.com";
        private const string publisherSupport = "This plugin implements Telementry Scheduler Plugin.";

        private bool _IsAdministrator = ProcessSecurityHelperWrapper.IsCurrentProcessRunningElevated();
        private static Logs _logs;
        private IAgent _agent;
        public const string PluginLogId = "TelementryScheduler";
        private IPlatinumSDKService? _PlatinumSDKPlugin;
        private readonly object _PluginConditionLock_PlatinumSDKPlugin = new object();

        private static System.Timers.Timer _SchedulerCheckTimer = new System.Timers.Timer(600000);

        #endregion

        #region Public Members

        public event EventHandler<ReadWriteRequest> ServiceRequest;

        #endregion

        #region Constructor

        public TelementrySchedulerPlugin(IAgent agent) : base(agent, PluginLogId)
        {
            _agent = agent;
            _IsAdministrator = ProcessSecurityHelperWrapper.IsCurrentProcessRunningElevated();
            _logs ??= new Logs(Log, PluginLogId);

            _SchedulerCheckTimer.Elapsed += OnSchedulerTimedRaise;
            _SchedulerCheckTimer.AutoReset = true;
            _SchedulerCheckTimer.Enabled = true;

            _logs.DebugMsg_1("TelementrySchedulerPlugin constructor ...");
        }

        #endregion

        #region Overriding methods

        protected override void OnPluginStarting()
        {
            _agent.PluginManager.PluginsStarted += PluginManagerOnPluginsStarted;
            PluginCondition = new PluginStartedCondition();
            InitializePlatinumSDKPlugin();

            _logs.DebugMsg_1("TelementryScheduler plugin Starting");
        }

        #endregion

        #region Imprement ITelementryScheduler

        public Task StopSchedulerManger()
        {
            _logs.DebugMsg_1("received StopSchedulerManger requested ...");

            if (_SchedulerCheckTimer.Enabled)
                _SchedulerCheckTimer.Stop();

            return Task.FromResult(Task.CompletedTask);
        }

        public Task StartSchedulerManger(int millisecond)
        {
            _logs.DebugMsg_1("received StartSchedulerManger: " + millisecond.ToString() + " requested ...");

            if (_SchedulerCheckTimer.Enabled)
                _SchedulerCheckTimer.Stop();

            _SchedulerCheckTimer.Interval = millisecond;
            _SchedulerCheckTimer.AutoReset = true;
            _SchedulerCheckTimer.Start();
            return Task.FromResult(Task.CompletedTask);
        }

        public Task ReceiveScheduleInfo(scheduleInfo info)
        {
            //_logs.DebugMsg_1("ReceiveScheduleInfo ...");
            //_ScheduleMap = info;
            //_WaitTag = false;
            //_logs.DebugMsg_1("ReceiveScheduleInfo _ScheduleMap is " + ((_ScheduleMap != null) ? "Received" : "Null"));
            return Task.CompletedTask;
        }

        #endregion

        #region Private Methods

        protected virtual void OnServiceRequest(ReadWriteRequest e)
        {
            _logs.DebugMsg_1("Brocast OnServiceRequest ...");

            //VCPchanged?.Invoke(this, e);
            EventHandler<ReadWriteRequest> handler = ServiceRequest;
            if (handler != null)
                Task.Run(() => handler.Invoke(this, e));

            //The Asynchronous Programming Model (APM) (using IAsyncResult and BeginInvoke) is no longer the preferred method of making asynchronous calls.
            //The Task-based Asynchronous Pattern (TAP) is the recommended async model as of .NET Framework 4.5.
            //Because of this, and because the implementation of async delegates depends on remoting features not present in .NET Core, BeginInvoke and EndInvoke delegate calls are not supported in .NET Core.
            //This is discussed in GitHub issue dotnet/corefx #5940.
        }

        private void OnSchedulerTimedRaise(Object source, System.Timers.ElapsedEventArgs e)
        {
            _logs.DebugMsg_1("[Hook] OnSchedulerTimedRaise");
        }

        private void InitializePlatinumSDKPlugin()
        {
            if (_PlatinumSDKPlugin != null)
                return;

            _PlatinumSDKPlugin = _agent.PluginManager.FindPluginByType<IPlatinumSDKService>(PluginResolution.Dynamic);

            if (_PlatinumSDKPlugin is IFrameworkPluginConditionNotification pluginCondition)
            {
                pluginCondition.PluginConditionChangeHandler += OnPlatinumSDKPluginConditionChangeHandler;
                GetCurrentPlatinumSDKPluginCondition();
            }
        }

        private void GetCurrentPlatinumSDKPluginCondition()
        {
            _ = Task.Run(async () =>
            {
                var pluginCondition = await (_PlatinumSDKPlugin as IFrameworkPluginConditionNotification)?.CurrentConditionAsync();

                lock (_PluginConditionLock_PlatinumSDKPlugin)
                {
                    if (pluginCondition is PluginErrorCondition)
                    {
                        _logs.DebugMsg($"[TelementryScheduler] {nameof(GetCurrentPlatinumSDKPluginCondition)} - PlatinumSDK Plugin is in an error condition");
                    }
                    else if (pluginCondition is PluginRunningCondition || pluginCondition is PluginStartedCondition)
                    {
                        _logs.DebugMsg($"[TelementryScheduler] {nameof(GetCurrentPlatinumSDKPluginCondition)} - PlatinumSDK Plugin is in a {nameof(pluginCondition)} condition");
                        
                        if (_PlatinumSDKPlugin != null)
                            _logs.DebugMsg($"[TelementryScheduler] PlatinumSDK Plugin is Ready ....");
                    }
                }
            });
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
            _logs.DebugMsg_1($"Dispose: {disposing}");
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

        private void OnPlatinumSDKPluginConditionChangeHandler(object sender, EventArgs e)
        {
            GetCurrentPlatinumSDKPluginCondition();
        }


        private void PluginManagerOnPluginsStarted(object sender, PluginsStartedEventArgs e)
        {
            if (e == null)
                return;
            if (e.ChangedPlugins == null)
                return;
            if (e.ChangedPlugins.Any() == false)
                return;

            if (e.ChangedPlugins.OfType<IPlatinumSDKService>().Any())
                InitializePlatinumSDKPlugin();
        }

        #endregion
    }
}