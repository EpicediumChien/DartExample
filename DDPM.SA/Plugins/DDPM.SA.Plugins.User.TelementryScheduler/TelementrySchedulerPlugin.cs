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
    [DependencyKnownTypes(new[] { typeof(IPlatinumSDKService), typeof(ISettingsManagerDev) })]
    [PluginRequires(Id = IDs.PlatinumSDK_Plugin, Version = "1.0.0", AllowDynamicResolving = true)]
    [PluginRequires(Id = IDs.DDPM_SETTINGSMANAGER_SA_PLUGIN_ID, Version = "1.0.0", AllowDynamicResolving = true)]
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
        private IPlatinumSDKService _PlatinumSDKPlugin;
        private ISettingsManagerDev _SettingsPlugin;
        private readonly object _PluginConditionLock_PlatinumSDKPlugin = new object();
        private readonly object _PluginConditionLock_Settings = new object();
        private static System.Timers.Timer _SchedulerCheckTimer = new System.Timers.Timer(90000);
        private bool IsStartTelementry = false;
        private bool IsTelemetryConsentOn = false;
        private DDPMSettings DDPMSettingsconfig;
        private FrequencyDateTime _FrequencyDateTime = new FrequencyDateTime();

        #endregion

        #region Public Members

        public event EventHandler<ReadWriteRequest> ServiceRequest;

        #endregion

        #region Constructor

        public TelementrySchedulerPlugin(IAgent agent) : base(agent, PluginLogId)
        {
            _agent = agent;
            _IsAdministrator = ProcessSecurityHelperWrapper.IsCurrentProcessRunningElevated();
            _logs ??= new Logs(Log);

            _SchedulerCheckTimer.Elapsed += OnSchedulerTimedRaise;
            _SchedulerCheckTimer.AutoReset = true;
            _SchedulerCheckTimer.Enabled = true;

            _logs.DebugMsg("[TelementryScheduler] TelementrySchedulerPlugin constructor ...");
        }

        #endregion

        #region Overriding methods

        protected override void OnPluginStarting()
        {
            _agent.PluginManager.PluginsStarted += PluginManagerOnPluginsStarted;
            PluginCondition = new PluginStartedCondition();
            InitializePlatinumSDKPlugin();
            InitializeSettingsPlugin();

            _logs.DebugMsg("[TelementryScheduler] TelementryScheduler plugin Starting");
        }

        #endregion

        #region Imprement ITelementryScheduler

        public Task StartTelemetrySchedulerManger(bool IsStart)
        {
            _logs.DebugMsg("[TelementryScheduler] received StartSchedulerManger IsStart: " + IsStart.ToString() + " requested ...");

            IsStartTelementry = IsStart;

            return Task.FromResult(Task.CompletedTask);
        }

        public Task<bool> ReceiveTelemetryInfo(string EventTag, string EventValue, Telementry_Frequency Frequency)
        {
            _logs.DebugMsg("[TelementryScheduler] received ReceiveTelemetryInfo requested ...");
            _logs.DebugMsg("[TelementryScheduler] received ReceiveTelemetryInfo ET : " + EventTag);
            _logs.DebugMsg("[TelementryScheduler] received ReceiveTelemetryInfo EV : " + EventValue);
            _logs.DebugMsg("[TelementryScheduler] received ReceiveTelemetryInfo Frequency : " + Frequency.ToString());

            var r = false;

            if (IsTelemetryConsentOn)
            {
                switch (Frequency)
                {
                    case Telementry_Frequency.RealTime:
                        {
                            if (_PlatinumSDKPlugin != null)
                                r = _PlatinumSDKPlugin.UpdateEventValue(EventTag, EventValue).Result;
                            break;
                        }
                    case Telementry_Frequency.FirstDayofMonth:
                        {
                            if (_PlatinumSDKPlugin != null)
                            {
                                if (DateTime.Now.Day == 1)
                                    r = _PlatinumSDKPlugin.UpdateEventValue(EventTag, EventValue).Result;
                                else
                                    r = true;
                            }
                            break;
                        }
                    case Telementry_Frequency.PerDay:
                        {
                            if (_PlatinumSDKPlugin != null)
                            {
                                if (DateTime.Now.AddDays(-1) >= _FrequencyDateTime.PerDay)
                                    r = _PlatinumSDKPlugin.UpdateEventValue(EventTag, EventValue).Result;
                                else
                                    r = true;
                            }
                            break;
                        }
                    case Telementry_Frequency.Weekly:
                        {
                            if (_PlatinumSDKPlugin != null)
                            {
                                if (DateTime.Now.AddDays(-7) >= _FrequencyDateTime.Weekly)
                                    r = _PlatinumSDKPlugin.UpdateEventValue(EventTag, EventValue).Result;
                                else
                                    r = true;
                            }
                            break;
                        }
                    default:
                        break;
                }
            }

            return Task.FromResult(r);
        }

        public Task GetGlobalsetting_IsTelemetryConsentOn(bool value)
        {
            _logs.DebugMsg("[TelementryScheduler] received GetGlobalsetting_IsTelemetryConsentOn ...");

            IsTelemetryConsentOn = value;

            return Task.FromResult(Task.CompletedTask);
        }

        #endregion

        #region Private Methods

        private void OnSchedulerTimedRaise(Object source, System.Timers.ElapsedEventArgs e)
        {
            _logs.DebugMsg("[TelementryScheduler] [Hook] OnSchedulerTimedRaise");
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

        private void InitializeSettingsPlugin()
        {
            if (_SettingsPlugin != null)
                return;

            _SettingsPlugin = _agent.PluginManager.FindPluginByType<ISettingsManagerDev>(PluginResolution.Dynamic);

            if (_SettingsPlugin is IFrameworkPluginConditionNotification pluginCondition)
            {
                pluginCondition.PluginConditionChangeHandler += OnSettingsPluginConditionChangeHandler;
                GetCurrentSettingsPluginCondition();
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

        private void GetCurrentSettingsPluginCondition()
        {
            _ = Task.Run(async () =>
            {
                var pluginCondition = await (_SettingsPlugin as IFrameworkPluginConditionNotification)?.CurrentConditionAsync();
                lock (_PluginConditionLock_Settings)
                {
                    if (pluginCondition is PluginErrorCondition)
                    {
                        _logs.DebugMsg($"[TelementryScheduler] {nameof(GetCurrentSettingsPluginCondition)} - Settings Plugin is in an error condition");
                    }
                    else if (pluginCondition is PluginRunningCondition || pluginCondition is PluginStartedCondition)
                    {
                        _logs.DebugMsg($"[TelementryScheduler] {nameof(GetCurrentSettingsPluginCondition)} - Settings Plugin is in a running/started condition");
                        _SettingsPlugin.SettingReadyEvent += SettingsReady;
                        DDPMSettingsconfig = _SettingsPlugin.ReloadAppConfigData().Result;
                    }
                    else
                    {
                        _logs.DebugMsg($"[TelementryScheduler] {nameof(GetCurrentSettingsPluginCondition)} - Settings Plugin is in unknow condition: {pluginCondition}");
                    }
                }
            });
        }

        private void SettingsReady(object o, EventArgs eventArgs)
        {
            DDPMSettingsconfig = _SettingsPlugin.ReloadAppConfigData().Result;
            if (DDPMSettingsconfig != null)
            {
                _logs.DebugMsg($"[TelementryScheduler] {nameof(SettingsReady)} - Get _FrequencyDateTime Already");
                _FrequencyDateTime = DDPMSettingsconfig.UserSettings.TelementryFrequency;
            }
            else _logs.DebugMsg($"[TelementryScheduler] {nameof(SettingsReady)} - Get _FrequencyDateTime Fail");
        }

        private bool Get_FrequencyDateTime()
        {
            var rc = false;

            DDPMSettingsconfig = _SettingsPlugin.ReloadAppConfigData().Result;

            if (_SettingsPlugin != null)
            {
                if (DDPMSettingsconfig != null)
                    rc = true;
            }

            return rc;
        }

        private bool Set_FrequencyDateTime(DDPMSettings config)
        {
            var rc = _SettingsPlugin.SetAppConfigData(config).Result;

            return rc;
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
            _logs.DebugMsg($"[TelementryScheduler] Dispose: {disposing}");
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

        private void OnSettingsPluginConditionChangeHandler(object sender, EventArgs e)
        {
            GetCurrentSettingsPluginCondition();
        }

        private void PluginManagerOnPluginsStarted(object sender, PluginsStartedEventArgs e)
        {
            if (e == null)
                return;
            if (e.ChangedPlugins == null)
                return;
            if (e.ChangedPlugins.Any() == false)
                return;

            if (e.ChangedPlugins.OfType<ISettingsManagerDev>().Any())
                InitializeSettingsPlugin();

            if (e.ChangedPlugins.OfType<IPlatinumSDKService>().Any())
                InitializePlatinumSDKPlugin();
        }

        #endregion
    }
}