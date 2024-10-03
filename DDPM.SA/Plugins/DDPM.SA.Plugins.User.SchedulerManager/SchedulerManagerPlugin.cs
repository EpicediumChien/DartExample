#region LicenceHeader

//
// Copyright © 2024, Dell Inc., All Rights Reserved.
// This material is confidential and a trade secret.  Permission to use this
// work for any purpose must be obtained in writing from Dell Inc.
//
// SchedulerManager.cs created on 25/04/2024T07:20 PM
//

#endregion

using DDPM.SA.Common;
using DDPM.SA.Common.Settings;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.Common.Annotations;
using Dell.Client.Framework.Common.PluginConditions;
using Dell.Client.Framework.Interfaces;
using Microsoft;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VcpCore.Common;
using IDs = DDPM.SA.Common.IDs;

namespace DDPM.SA.Plugins.User.SchedulerManager
{
    [Plugin(IDs.Scheduler_Manager_Plugin_ID, pluginName, PluginOrderGroupType.Core, Version = pluginVersion)]
    [Descriptor(Description = pluginDescription)]
    [Publisher(Name = publisherCompany, Website = publisherWebsite, Support = publisherSupport)]
    [PublishedUnelevatedInterface(new[] { typeof(ISchedulerManager) })]
    [PluginRequires(Id = IDs.Display_Manager_PLUGIN_ID, AllowDynamicResolving = true)]
    [DependencyKnownTypes(new[] { typeof(IDisplayService) })]
    public class SchedulerMangerPlugin : BaseAgentPlugin, IDisposableObservable, ISchedulerManager
    {
        #region Private Members

        private const string pluginName = "SchedulerManagerPlugin";
        private const string pluginVersion = "1.0.0";
        private const string pluginDescription = "This plugin implements Scheduler Manager Plugin.";
        private const string publisherCompany = "Wistron";
        private const string publisherWebsite = "https://www.wistron.com";
        private const string publisherSupport = "This plugin implements Scheduler Manager Plugin.";

        private bool _IsAdministrator = ProcessSecurityHelperWrapper.IsCurrentProcessRunningElevated();
        private static Logs _logs;
        private IAgent _agent;
        public const string PluginLogId = "SchedulerManager";
        private IDisplayService _DisplayManagerPlugin;
        private static readonly object _PluginConditionLock = new object();
        private static readonly object _PluginConditionLock_Display = new object();
        private static System.Timers.Timer _SchedulerCheckTimer = new System.Timers.Timer(600000);
        private static List<MonitorInfo> _AllInfoMonitors;
        private static scheduleInfo _ScheduleMap;
        private static DDPMSettings _DDPMSettings;
        private static readonly object _MoLock = new object();
        private bool _WaitTag = false;

        #endregion

        #region Public Members

        public event EventHandler<ReadWriteRequest> ServiceRequest;

        #endregion

        #region Constructor

        public SchedulerMangerPlugin(IAgent agent) : base(agent, PluginLogId)
        {
            _agent = agent;
            _IsAdministrator = ProcessSecurityHelperWrapper.IsCurrentProcessRunningElevated();
            _logs ??= new Logs(Log, PluginLogId);

            _AllInfoMonitors ??= new List<MonitorInfo>();
            _ScheduleMap ??= new scheduleInfo();

            _SchedulerCheckTimer.Elapsed += OnSchedulerTimedRaise;
            _SchedulerCheckTimer.AutoReset = true;
            _SchedulerCheckTimer.Enabled = true;
            _WaitTag = false;
            _logs.DebugMsg_1("SchedulerManagerPlugin constructor ...");
        }

        #endregion

        #region Overriding methods

        protected override void OnPluginStarting()
        {
            _agent.PluginManager.PluginsStarted += PluginManagerOnPluginsStarted;
            PluginCondition = new PluginStartedCondition();
            InitializeDisplayManagerPlugin();

            _logs.DebugMsg_1("SchedulerManager plugin Starting");
        }

        #endregion

        #region Imprement ISchedulerManager

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
            _logs.DebugMsg_1("ReceiveScheduleInfo ...");
            _ScheduleMap = info;
            _WaitTag = false;
            _logs.DebugMsg_1("ReceiveScheduleInfo _ScheduleMap is " + ((_ScheduleMap != null) ? "Received" : "Null"));
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

        private void InitializeScheduleInfo(MonitorInfo monitor)
        {
            _logs.DebugMsg_1("InitializeScheduleInfo ...");
            _WaitTag = true;
            ReadWriteRequest _ReadWriteRequestEventArgs = new ReadWriteRequest();
            _ReadWriteRequestEventArgs.monitor = monitor;
            _ReadWriteRequestEventArgs.service = ReadWriteRequest_Type.Read;
            OnServiceRequest(_ReadWriteRequestEventArgs);
        }

        private void InitializeMonitorInfo()
        {
            if (_DisplayManagerPlugin != null)
            {
                _logs.DebugMsg_1("SchedulerManager InitializeMonitorInfo ...");

                if (_AllInfoMonitors != null) _AllInfoMonitors.Clear();
                else _AllInfoMonitors = new List<MonitorInfo>();

                _AllInfoMonitors.AddRange(_DisplayManagerPlugin.GetMonitors().Result);

                _logs.DebugMsg_1("_AllInfoMonitors count : " + _AllInfoMonitors.Count);
            }
        }

        private void OnSchedulerTimedRaise(Object source, System.Timers.ElapsedEventArgs e)
        {
            _logs.DebugMsg_1("[Hook] OnSchedulerTimedRaise");

            if (_DisplayManagerPlugin != null )
            {
                InitializeMonitorInfo();

                if (_AllInfoMonitors.Count > 0)
                {
                    CalculateNowValue();
                }
                else
                    _logs.DebugMsg_1("[OnSchedulerTimedRaise] No monitors to service");
            }
        }

        private void CalculateNowValue()
        {
            _logs.DebugMsg_1("Into CalculateNowValue ...");

            foreach (MonitorInfo monitor in _AllInfoMonitors)
            {
                InitializeScheduleInfo(monitor);
                int countx = 0;
                do
                {
                    countx++;
                } while (_WaitTag && countx < 10);

                if (_ScheduleMap != null)
                {
                    if (_ScheduleMap.IsEnable)
                    {
                        if (_ScheduleMap.Hours1 > -1 && _ScheduleMap.Hours2 > -1 && _ScheduleMap.Mins1 > -1 && _ScheduleMap.Mins2 > -1 && _ScheduleMap.Duration1 > -1 && _ScheduleMap.Duration2 > -1)
                        {
                            var CurDateTime = DateTime.Now;
                            var Brightness_PR1 = _ScheduleMap.Brightness1;
                            var Brightness_PR2 = _ScheduleMap.Brightness2;
                            var Brightness_difference = Brightness_PR1 - Brightness_PR2;
                            var Contrast_PR1 = _ScheduleMap.Contrast1;
                            var Contrast_PR2 = _ScheduleMap.Contrast2;
                            var Contrast_difference = Contrast_PR1 - Contrast_PR2;
                            bool IsBrightnessPR1Plus = Brightness_difference > 0 ? true : false;
                            bool IsBrightnessPR2Plus = Brightness_difference > 0 ? false : true;
                            bool IsContrastPR1Plus = Contrast_difference > 0 ? true : false;
                            bool IsContrastPR2Plus = Contrast_difference > 0 ? false : true;
                            var Hour_PR1 = (_ScheduleMap.Hours1 < 12) ? _ScheduleMap.Hours1 : (_ScheduleMap.Hours1 - 12);
                            var Hour_PR2 = (_ScheduleMap.Hours2 < 12) ? (_ScheduleMap.Hours2 + 12) : _ScheduleMap.Hours2;
                            var Min_PR1 = _ScheduleMap.Mins1;
                            var Min_PR2 = _ScheduleMap.Mins2;
                            var Duration_PR1 = _ScheduleMap.Duration1;
                            var Duration_PR2 = _ScheduleMap.Duration2;
                            var PR1Time = new DateTime(CurDateTime.Year, CurDateTime.Month, CurDateTime.Day, Hour_PR1, Min_PR1, 0);
                            var Pre_PR1Time = new DateTime(CurDateTime.Year, CurDateTime.Month, CurDateTime.Day, Hour_PR1, Min_PR1, 0).AddMinutes(Duration_PR1 * -1);
                            var PR1Time_ADD1D = new DateTime(CurDateTime.Year, CurDateTime.Month, CurDateTime.Day, Hour_PR1, Min_PR1, 0).AddDays(1);
                            var Pre_PR1Timee_ADD1D = (new DateTime(CurDateTime.Year, CurDateTime.Month, CurDateTime.Day, Hour_PR1, Min_PR1, 0).AddMinutes(Duration_PR1 * -1)).AddDays(1);
                            var PR2Time = new DateTime(CurDateTime.Year, CurDateTime.Month, CurDateTime.Day, Hour_PR2, Min_PR2, 0);
                            var Pre_PR2Time = new DateTime(CurDateTime.Year, CurDateTime.Month, CurDateTime.Day, Hour_PR2, Min_PR2, 0).AddMinutes(Duration_PR2 * -1);
                            if (Brightness_difference < 0) Brightness_difference = Brightness_difference * -1;
                            if (Contrast_difference < 0) Contrast_difference = Contrast_difference * -1;
                            var PerStepValue = 5;
                            byte Bvcp = 0x10;
                            byte Cvcp = 0x12;
                            if (CurDateTime.Hour < 12)  // AM
                            {
                                if (DateTime.Compare(CurDateTime, PR1Time) > 0) //CurDateTime is later than PR1Time.
                                {
                                    if (DateTime.Compare(CurDateTime, Pre_PR2Time) > 0)  //CurDateTime is later than Pre_PR2Time.
                                    {
                                        var BrightnessSteps = Brightness_difference / PerStepValue;
                                        var BrightnessSteps_min = (PR2Time - Pre_PR2Time).TotalMinutes / BrightnessSteps;
                                        var Brightness_steps = Convert.ToInt32((CurDateTime - Pre_PR2Time).TotalMinutes / BrightnessSteps_min);

                                            var ContrastSteps = Contrast_difference / PerStepValue;
                                            var ContrastSteps_min = (PR2Time - Pre_PR2Time).TotalMinutes / ContrastSteps;
                                            var Contrast_steps = Convert.ToInt32((CurDateTime - Pre_PR2Time).TotalMinutes / ContrastSteps_min);

                                            if (IsBrightnessPR2Plus)
                                            {
                                                var value = Convert.ToUInt32(Brightness_PR1 + (PerStepValue * Brightness_steps));
                                                _DisplayManagerPlugin.SetVCPCapability(monitor, Bvcp, value);

                                                _logs.DebugMsg_1("CalculateNowValue Set Brightness to : " + value.ToString());
                                            }
                                            else
                                            {
                                                var value = Convert.ToUInt32(Brightness_PR1 - (PerStepValue * Brightness_steps));
                                                _DisplayManagerPlugin.SetVCPCapability(monitor, Bvcp, value);

                                                _logs.DebugMsg_1("CalculateNowValue Set Brightness to : " + value.ToString());
                                            }

                                            if (IsContrastPR2Plus)
                                            {
                                                var value = Convert.ToUInt32(Contrast_PR1 + (PerStepValue * Contrast_steps));
                                                _DisplayManagerPlugin.SetVCPCapability(monitor, Cvcp, value);

                                                _logs.DebugMsg_1("CalculateNowValue Set Contrast to : " + value.ToString());
                                            }
                                            else
                                            {
                                                var value = Convert.ToUInt32(Contrast_PR1 - (PerStepValue * Contrast_steps));
                                                _DisplayManagerPlugin.SetVCPCapability(monitor, Cvcp, value);

                                                _logs.DebugMsg_1("CalculateNowValue Set Contrast to : " + value.ToString());
                                            }
                                        }
                                        else  //CurDateTime is same as or earlier than Pre_PR2Time
                                        {
                                            var valueI = Convert.ToUInt32(Brightness_PR1);
                                            _DisplayManagerPlugin.SetVCPCapability(monitor, Bvcp, valueI);
                                            var valueII = Convert.ToUInt32(Contrast_PR1);
                                            _DisplayManagerPlugin.SetVCPCapability(monitor, Cvcp, valueII);

                                            _logs.DebugMsg_1("CalculateNowValue Set Brightness to : " + valueI.ToString());
                                            _logs.DebugMsg_1("CalculateNowValue Set Contrast to : " + valueII.ToString());
                                        }
                                    }
                                    else if (DateTime.Compare(CurDateTime, PR1Time) == 0) //The same as PR1Time
                                    {
                                        var valueI = Convert.ToUInt32(Brightness_PR1);
                                        _DisplayManagerPlugin.SetVCPCapability(monitor, Bvcp, valueI);
                                        var valueII = Convert.ToUInt32(Contrast_PR1);
                                        _DisplayManagerPlugin.SetVCPCapability(monitor, Cvcp, valueII);

                                        _logs.DebugMsg_1("CalculateNowValue Set Brightness to : " + valueI.ToString());
                                        _logs.DebugMsg_1("CalculateNowValue Set Contrast to : " + valueII.ToString());
                                    }
                                    else //CurDateTime is earlier than PR1Time
                                    {
                                        if (DateTime.Compare(CurDateTime, Pre_PR1Time) > 0)  //CurDateTime is later than Pre_PR1Time.
                                        {
                                            var BrightnessSteps = Brightness_difference / PerStepValue;
                                            var BrightnessSteps_min = (PR1Time - Pre_PR1Time).TotalMinutes / BrightnessSteps;
                                            var Brightness_steps = Convert.ToInt32((CurDateTime - Pre_PR1Time).TotalMinutes / BrightnessSteps_min);

                                            var ContrastSteps = Contrast_difference / PerStepValue;
                                            var ContrastSteps_min = (PR1Time - Pre_PR1Time).TotalMinutes / ContrastSteps;
                                            var Contrast_steps = Convert.ToInt32((CurDateTime - Pre_PR1Time).TotalMinutes / ContrastSteps_min);

                                            if (IsBrightnessPR1Plus)
                                            {
                                                var value = Convert.ToUInt32(Brightness_PR2 + (PerStepValue * Brightness_steps));
                                                _DisplayManagerPlugin.SetVCPCapability(monitor, Bvcp, value);

                                                _logs.DebugMsg_1("CalculateNowValue Set Brightness to : " + value.ToString());
                                            }
                                            else
                                            {
                                                var value = Convert.ToUInt32(Brightness_PR2 - (PerStepValue * Brightness_steps));
                                                _DisplayManagerPlugin.SetVCPCapability(monitor, Bvcp, value);

                                                _logs.DebugMsg_1("CalculateNowValue Set Brightness to : " + value.ToString());
                                            }

                                            if (IsContrastPR1Plus)
                                            {
                                                var value = Convert.ToUInt32(Contrast_PR2 + (PerStepValue * Contrast_steps));
                                                _DisplayManagerPlugin.SetVCPCapability(monitor, Cvcp, value);

                                                _logs.DebugMsg_1("CalculateNowValue Set Contrast to : " + value.ToString());
                                            }
                                            else
                                            {
                                                var value = Convert.ToUInt32(Contrast_PR2 - (PerStepValue * Contrast_steps));
                                                _DisplayManagerPlugin.SetVCPCapability(monitor, Cvcp, value);

                                                _logs.DebugMsg_1("CalculateNowValue Set Contrast to : " + value.ToString());
                                            }
                                        }
                                        else  //CurDateTime is same as  or  earlier than Pre_PR1Time
                                        {
                                            var valueI = Convert.ToUInt32(Brightness_PR2);
                                            _DisplayManagerPlugin.SetVCPCapability(monitor, Bvcp, valueI);
                                            var valueII = Convert.ToUInt32(Contrast_PR2);
                                            _DisplayManagerPlugin.SetVCPCapability(monitor, Cvcp, valueII);

                                            _logs.DebugMsg_1("CalculateNowValue Set Brightness to : " + valueI.ToString());
                                            _logs.DebugMsg_1("CalculateNowValue Set Contrast to : " + valueII.ToString());
                                        }
                                    }
                                }
                                else //PM
                                {
                                    if (DateTime.Compare(CurDateTime, PR2Time) > 0) //CurDateTime is later than PR2Time.
                                    {
                                        if (DateTime.Compare(CurDateTime, Pre_PR1Timee_ADD1D) > 0)  //CurDateTime is later than Pre_PR1Timee_ADD1D.
                                        {
                                            var BrightnessSteps = Brightness_difference / PerStepValue;
                                            var BrightnessSteps_min = (PR1Time - Pre_PR1Time).TotalMinutes / BrightnessSteps;
                                            var Brightness_steps = Convert.ToInt32((CurDateTime - Pre_PR1Time).TotalMinutes / BrightnessSteps_min);

                                            var ContrastSteps = Contrast_difference / PerStepValue;
                                            var ContrastSteps_min = (PR1Time - Pre_PR1Time).TotalMinutes / ContrastSteps;
                                            var Contrast_steps = Convert.ToInt32((CurDateTime - Pre_PR1Time).TotalMinutes / ContrastSteps_min);

                                            if (IsBrightnessPR1Plus)
                                            {
                                                var value = Convert.ToUInt32(Brightness_PR2 + (PerStepValue * Brightness_steps));
                                                _DisplayManagerPlugin.SetVCPCapability(monitor, Bvcp, value);

                                                _logs.DebugMsg_1("CalculateNowValue Set Brightness to : " + value.ToString());
                                            }
                                            else
                                            {
                                                var value = Convert.ToUInt32(Brightness_PR2 - (PerStepValue * Brightness_steps));
                                                _DisplayManagerPlugin.SetVCPCapability(monitor, Bvcp, value);

                                                _logs.DebugMsg_1("CalculateNowValue Set Brightness to : " + value.ToString());
                                            }

                                            if (IsContrastPR1Plus)
                                            {
                                                var value = Convert.ToUInt32(Contrast_PR2 + (PerStepValue * Contrast_steps));
                                                _DisplayManagerPlugin.SetVCPCapability(monitor, Cvcp, value);

                                                _logs.DebugMsg_1("CalculateNowValue Set Contrast to : " + value.ToString());
                                            }
                                            else
                                            {
                                                var value = Convert.ToUInt32(Contrast_PR2 - (PerStepValue * Contrast_steps));
                                                _DisplayManagerPlugin.SetVCPCapability(monitor, Cvcp, value);

                                                _logs.DebugMsg_1("CalculateNowValue Set Contrast to : " + value.ToString());
                                            }
                                        }
                                        else  //CurDateTime is same as or earlier than Pre_PR1Timee_ADD1D
                                        {
                                            var valueI = Convert.ToUInt32(Brightness_PR2);
                                            _DisplayManagerPlugin.SetVCPCapability(monitor, Bvcp, valueI);
                                            var valueII = Convert.ToUInt32(Contrast_PR2);
                                            _DisplayManagerPlugin.SetVCPCapability(monitor, Cvcp, valueII);

                                            _logs.DebugMsg_1("CalculateNowValue Set Brightness to : " + valueI.ToString());
                                            _logs.DebugMsg_1("CalculateNowValue Set Contrast to : " + valueII.ToString());
                                        }
                                    }
                                    else if (DateTime.Compare(CurDateTime, PR2Time) == 0) //The same as PR2Time.
                                    {
                                        var valueI = Convert.ToUInt32(Brightness_PR2);
                                        _DisplayManagerPlugin.SetVCPCapability(monitor, Bvcp, valueI);
                                        var valueII = Convert.ToUInt32(Contrast_PR2);
                                        _DisplayManagerPlugin.SetVCPCapability(monitor, Cvcp, valueII);

                                        _logs.DebugMsg_1("CalculateNowValue Set Brightness to : " + valueI.ToString());
                                        _logs.DebugMsg_1("CalculateNowValue Set Contrast to : " + valueII.ToString());
                                    }
                                    else //CurDateTime is earlier than PR2Time.
                                    {
                                        if (DateTime.Compare(CurDateTime, Pre_PR2Time) > 0)  //CurDateTime is later than Pre_PR2Time.
                                        {
                                            var BrightnessSteps = Brightness_difference / PerStepValue;
                                            var BrightnessSteps_min = (PR2Time - Pre_PR2Time).TotalMinutes / BrightnessSteps;
                                            var Brightness_steps = Convert.ToInt32((CurDateTime - Pre_PR2Time).TotalMinutes / BrightnessSteps_min);

                                            var ContrastSteps = Contrast_difference / PerStepValue;
                                            var ContrastSteps_min = (PR2Time - Pre_PR2Time).TotalMinutes / ContrastSteps;
                                            var Contrast_steps = Convert.ToInt32((CurDateTime - Pre_PR2Time).TotalMinutes / ContrastSteps_min);

                                            if (IsBrightnessPR2Plus)
                                            {
                                                var value = Convert.ToUInt32(Brightness_PR1 + (PerStepValue * Brightness_steps));
                                                _DisplayManagerPlugin.SetVCPCapability(monitor, Bvcp, value);

                                                _logs.DebugMsg_1("CalculateNowValue Set Brightness to : " + value.ToString());
                                            }
                                            else
                                            {
                                                var value = Convert.ToUInt32(Brightness_PR1 - (PerStepValue * Brightness_steps));
                                                _DisplayManagerPlugin.SetVCPCapability(monitor, Bvcp, value);

                                                _logs.DebugMsg_1("CalculateNowValue Set Brightness to : " + value.ToString());
                                            }

                                            if (IsContrastPR2Plus)
                                            {
                                                var value = Convert.ToUInt32(Contrast_PR1 + (PerStepValue * Contrast_steps));
                                                _DisplayManagerPlugin.SetVCPCapability(monitor, Cvcp, value);

                                                _logs.DebugMsg_1("CalculateNowValue Set Contrast to : " + value.ToString());
                                            }
                                            else
                                            {
                                                var value = Convert.ToUInt32(Contrast_PR1 - (PerStepValue * Contrast_steps));
                                                _DisplayManagerPlugin.SetVCPCapability(monitor, Cvcp, value);

                                                _logs.DebugMsg_1("CalculateNowValue Set Contrast to : " + value.ToString());
                                            }
                                        }
                                        else  //CurDateTime is same as  or  earlier than Pre_PR2Time
                                        {
                                            var valueI = Convert.ToUInt32(Brightness_PR1);
                                            _DisplayManagerPlugin.SetVCPCapability(monitor, Bvcp, valueI);
                                            var valueII = Convert.ToUInt32(Contrast_PR1);
                                            _DisplayManagerPlugin.SetVCPCapability(monitor, Cvcp, valueII);

                                        _logs.DebugMsg_1("CalculateNowValue Set Brightness to : " + valueI.ToString());
                                        _logs.DebugMsg_1("CalculateNowValue Set Contrast to : " + valueII.ToString());
                                    }
                                }
                            }
                        }
                    }
                }
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

        private void GetCurrentDisplayManagerCondition()
        {
            _ = Task.Run(async () =>
            {
                var pluginCondition = await (_DisplayManagerPlugin as IFrameworkPluginConditionNotification)?.CurrentConditionAsync();
                lock (_PluginConditionLock_Display)
                {
                    if (pluginCondition is PluginErrorCondition)
                    {
                        _logs.DebugMsg_1($"{nameof(GetCurrentDisplayManagerCondition)} - Display Manager Plugin is in an error condition");
                    }
                    else if (pluginCondition is PluginRunningCondition)
                    {
                        if (_AllInfoMonitors != null) _AllInfoMonitors.Clear();
                        else _AllInfoMonitors = new List<MonitorInfo>();

                        _logs.DebugMsg_1($"{nameof(GetCurrentDisplayManagerCondition)} - Display Manager Plugin is in a running condition, monitor count is {_AllInfoMonitors.Count}");
                    }
                    else if (pluginCondition is PluginStartedCondition)
                    {
                        if (_AllInfoMonitors != null) _AllInfoMonitors.Clear();
                        else _AllInfoMonitors = new List<MonitorInfo>();

                        _logs.DebugMsg_1($"{nameof(GetCurrentDisplayManagerCondition)} - Display Manager Plugin is in a started condition, monitor count is {_AllInfoMonitors.Count}");
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

        private void OnDisplayManagerPluginConditionChangeHandler(object sender, EventArgs e)
        {
            GetCurrentDisplayManagerCondition();
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
        }

        #endregion
    }
}