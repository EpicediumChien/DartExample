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
    [DependencyKnownTypes(new[] { typeof(IDisplayService), typeof(IPlatinumSDKService), typeof(ISettingsManagerDev) })]
    [PluginRequires(Id = IDs.PlatinumSDK_Plugin, Version = "1.0.0", AllowDynamicResolving = true)]
    [PluginRequires(Id = IDs.DDPM_SETTINGSMANAGER_SA_PLUGIN_ID, Version = "1.0.0", AllowDynamicResolving = true)]
    [PluginRequires(Id = IDs.Display_Manager_PLUGIN_ID, AllowDynamicResolving = true)]
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
        private IDisplayService _DisplayManagerPlugin;
        private readonly object _PluginConditionLock_PlatinumSDKPlugin = new object();
        private readonly object _PluginConditionLock_Settings = new object();
        private readonly object _PluginConditionLock_Display = new object();
        private static System.Timers.Timer _SchedulerCheckTimer = new System.Timers.Timer(3600000);
        private static bool IsStartTelementry = false;
        private static bool IsTelemetryConsentOn = false;
        private static DDPMSettings DDPMSettingsconfig;
        private static FrequencyDateTime _FrequencyDateTime = new FrequencyDateTime();

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
            InitializeDisplayManagerPlugin();
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
            _logs.DebugMsg("[TelementryScheduler] received ReceiveTelemetryInfo EV :\n" + EventValue);
            _logs.DebugMsg("[TelementryScheduler] received ReceiveTelemetryInfo Frequency : " + Frequency.ToString("G"));

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

        public Task<bool> ReceiveTelemetryInfo(string EventTag, Telementry_Frequency Frequency)
        {
            var r = ReceiveTelemetryInfo(EventTag, string.Empty, Frequency).Result;

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

            var _Allmonitors = _DisplayManagerPlugin.GetMonitors().Result;

            if (DateTime.Now.AddDays(-1) >= _FrequencyDateTime.PerDay)
            {
                #region DisplayInformation Collection

                {
                    if (_Allmonitors.Count > 0)
                    {
                        DisplayInformation_Function _dp = new DisplayInformation_Function();
                        foreach (var monitor in _Allmonitors)
                        {
                            var Orientation = _DisplayManagerPlugin.GetCurrentDisplayOrientation(monitor.DisplayName).Result;
                            var RefreshRate = _DisplayManagerPlugin.GetMonitorRefreshRate(monitor).Result;
                            var HDRStatus = _DisplayManagerPlugin.GetHDRStatus(monitor).Result;
                            var currentResolution = _DisplayManagerPlugin.GetMonitorCurrentResolution(monitor).Result;
                            var maxResolution = _DisplayManagerPlugin.GetMonitorMaxResolution(monitor).Result;
                            var rc = _dp.DisplayInformation_Telementry(monitor, Orientation, RefreshRate, HDRStatus, currentResolution, maxResolution);
                            var r = ReceiveTelemetryInfo("DisplayInformation", rc.ToJson(), Telementry_Frequency.PerDay).Result;

                            if (r)
                                _logs.DebugMsg("[TelementryScheduler] Send Telementry for DisplayInformation Success ...");
                            else
                                _logs.DebugMsg("[TelementryScheduler] Send Telementry for DisplayInformation Fail ...");
                        }
                    }
                }

                #endregion DisplayInformation Collection

                #region ScreenTimeInfo Collection

                {
                    if (_Allmonitors.Count > 0)
                    {
                        ScreenTimeInfo_Function _sf = new ScreenTimeInfo_Function();
                        List<uint> screetimes = new List<uint>();
                        List<string> serviceTags = new List<string>();
                        List<string> Models = new List<string>();
                        foreach (var monitor in _Allmonitors)
                        {
                            if (!string.IsNullOrWhiteSpace(monitor.modelName))
                                Models.Add(monitor.modelName);
                            else
                                Models.Add(string.Empty);

                            if (!string.IsNullOrWhiteSpace(monitor.edid.ServiceTag))
                                serviceTags.Add(monitor.edid.ServiceTag);
                            else
                                serviceTags.Add(string.Empty);

                            var screetime = _DisplayManagerPlugin.GetVCPCapability(monitor, 0xC0).Result;

                            if (screetime.result)
                                screetimes.Add((uint)screetime.value);
                            else
                                screetimes.Add(0x0);
                        }

                        var rc = _sf.ScreenTimeInfo_Telementry(screetimes, serviceTags, Models);
                        var r = ReceiveTelemetryInfo("ScreenTimeInfo", rc.ToJson(), Telementry_Frequency.PerDay).Result;

                        if (r)
                            _logs.DebugMsg("[TelementryScheduler] Send Telementry for ScreenTimeInfo Success ...");
                        else
                            _logs.DebugMsg("[TelementryScheduler] Send Telementry for ScreenTimeInfo Fail ...");
                    }
                }

                #endregion ScreenTimeInfo Collection
            }
            if (DateTime.Now.Day == 1)
            {
                #region HB

                var r = ReceiveTelemetryInfo("HB", Telementry_Frequency.PerDay).Result;

                if (r)
                    _logs.DebugMsg("[TelementryScheduler] Send Telementry for HB Success ...");
                else
                    _logs.DebugMsg("[TelementryScheduler] Send Telementry for HB Fail ...");

                #endregion HB
            }
        }

        private void InitializeDisplayManagerPlugin()
        {
            if (_DisplayManagerPlugin != null)
                return;

            _DisplayManagerPlugin = _agent.PluginManager.FindPluginByType<IDisplayService>(PluginResolution.Dynamic);

            if (_DisplayManagerPlugin is IFrameworkPluginConditionNotification pluginCondition)
            {
                pluginCondition.PluginConditionChangeHandler += OnDisplayManagerPluginConditionChangeHandler;
                GetCurrentDisplayManagerCondition();
            }
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

        private void GetCurrentDisplayManagerCondition()
        {
            _ = Task.Run(async () =>
            {
                var pluginCondition = await (_DisplayManagerPlugin as IFrameworkPluginConditionNotification)?.CurrentConditionAsync();
                //PluginCondition _DisplayManagerPluginCondition;
                lock (_PluginConditionLock_Display)
                {
                    if (pluginCondition is PluginErrorCondition)
                    {
                        _logs.DebugMsg($"[TelementryScheduler] {nameof(GetCurrentDisplayManagerCondition)} - Display Manager Plugin is in an error condition");
                    }
                    else if (pluginCondition is PluginRunningCondition)
                    {
                        _logs.DebugMsg($"[TelementryScheduler] {nameof(GetCurrentDisplayManagerCondition)} - Display Manager Plugin is in a running condition");
                    }
                    else if (pluginCondition is PluginStartedCondition)
                    {
                        _logs.DebugMsg($"[TelementryScheduler] {nameof(GetCurrentDisplayManagerCondition)} - Display Manager Plugin is in a started condition");
                    }
                }
            });
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

            if (_SettingsPlugin != null)
            {
                DDPMSettingsconfig = _SettingsPlugin.ReloadAppConfigData().Result;
                if (DDPMSettingsconfig != null)
                {
                    _logs.DebugMsg($"[TelementryScheduler] {nameof(Get_FrequencyDateTime)} - Get _FrequencyDateTime Already");
                    _FrequencyDateTime = DDPMSettingsconfig.UserSettings.TelementryFrequency;
                    rc = true;
                }
                else
                    _logs.DebugMsg($"[TelementryScheduler] {nameof(Get_FrequencyDateTime)} - Get _FrequencyDateTime Fail");
            }

            return rc;
        }

        private bool Set_FrequencyDateTime(DDPMSettings config)
        {
            var rc = false;

            if (_SettingsPlugin != null)
            {
                rc = _SettingsPlugin.SetAppConfigData(config).Result;
                if (rc)
                    _logs.DebugMsg($"[TelementryScheduler] {nameof(Set_FrequencyDateTime)} - Set _FrequencyDateTime Pass");
                else
                    _logs.DebugMsg($"[TelementryScheduler] {nameof(Set_FrequencyDateTime)} - Set _FrequencyDateTime Fail");
            }

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

        private void OnDisplayManagerPluginConditionChangeHandler(object sender, EventArgs e)
        {
            GetCurrentDisplayManagerCondition();
        }

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

            if (e.ChangedPlugins.OfType<IDisplayService>().Any())
                InitializeDisplayManagerPlugin();

            if (e.ChangedPlugins.OfType<ISettingsManagerDev>().Any())
                InitializeSettingsPlugin();

            if (e.ChangedPlugins.OfType<IPlatinumSDKService>().Any())
                InitializePlatinumSDKPlugin();
        }

        #endregion
    }
}