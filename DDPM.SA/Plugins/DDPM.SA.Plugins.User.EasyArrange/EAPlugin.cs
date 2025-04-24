using CommunityToolkit.Mvvm.DependencyInjection;
using DDPM.Easy.Common;
using DDPM.EABroker;
using DDPM.SA.Common;
using DDPM.SA.Common.Display;
using DDPM.SA.Common.Interfaces;
using DDPM.SA.Common.Settings;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.Common.Annotations;
using Dell.Client.Framework.Common.PluginConditions;
using Dell.Client.Framework.Interfaces;
using Microsoft;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using VcpCore.Common;
using IDs = DDPM.SA.Common.IDs;
using DDPM.SA.Common.Telemetry;

namespace DDPM.SA.Plugins.User.EasyArrange
{
    [Plugin(IDs.DDPM_EAPlugin_PLUGIN_ID, pluginName, PluginOrderGroupType.Core, Version = pluginVersion)]
    [Descriptor(Description = pluginDescription)]
    [Publisher(Name = publisherCompany, Website = publisherWebsite, Support = publisherSupport)]
    [PublishedUnelevatedInterface(new[] { typeof(IEasyArrangeService) })]
    //[PublishedInterface(new[] { typeof(IPipPbpService) })]
    [DependencyKnownTypes(new[] { typeof(IDisplayService) })]
    [PluginRequires(Id = IDs.DDPM_SETTINGSMANAGER_SA_PLUGIN_ID, AllowDynamicResolving = true)]
    [PluginRequires(Id = IDs.Device_Manager_Plugin_ID, AllowDynamicResolving = true)]
    [PluginRequires(Id = IDs.Display_Manager_PLUGIN_ID, Version = "1.0.0", AllowDynamicResolving = true)]
    public class EAPlugin : BaseAgentPlugin, IDisposableObservable, IEasyArrangeService
    {
        #region Private Members
        //Plugin strings
        private const string pluginName = "EAPlugin";
        private const string pluginVersion = "1.0.0";
        private const string pluginDescription = "This plugin implements EasyArrange Plugin functions.";
        private const string publisherCompany = "Dell Technologies";
        private const string publisherWebsite = "https://www.dell.com";
        private const string publisherSupport = "This plugin implements EasyArrange Plugin functions.";
        private const string PluginLogId = "EAPlugin";

        //DCF/Agent related
        private bool _IsAdministrator = ProcessSecurityHelperWrapper.IsCurrentProcessRunningElevated();
        private IAgent? _agent = null;
        private ILog? _log;
        private bool _isConfigured = false;

        //DDPM Subagent Plugins - DisplayManager
        private IDisplayService _displayManagerPlugin;
        private PluginCondition _displayManagerPluginCondition;
        private bool _displayManagerPluginUsable = false;

        //DDPM Subagent Plugins - DeviceManager
        private IDeviceManagerSA _deviceManagerPlugin;
        private PluginCondition _deviceManagerPluginCondition;
        private bool _deviceManagerPluginUsable = false;

        //DDPM Subagent Plugins - SettingsManager
        private ISettingsManagerDev _settingsManagerPlugin;
        private PluginCondition _settingsManagerPluginCondition;
        private bool _settingsManagerPluginUsable = false;

        //DDPM Subagent Plugins - TelemetryScheduler
        private ITelementryScheduler _telementrySchedulerPlugin;
        //private readonly object _PluginConditionLock_TelementryScheduler = new object();  //Derek 2025/04/01
        private bool _telementrySchedulerPluginUsable = false;
        private GlobalSettingParam? _globalSettingParam = null;

        //DDPM Subagent Plugins - Hotkey
        private IHotkey _HotkeyPlugin;
        //private PluginCondition _hotkeyPluginCondition; //Derek 2025/04/01
        private readonly object _PluginConditionLock_Hotkey = new object();

        //Lock objects
        private readonly object _PluginConditionLock = new object();

        //EABroker (init by EABroker_Start())
        private readonly object _eaBrokerLock = new object();
        private bool _isEaBrokerStarted = false;
        private DDPM.EABroker.EABroker? _eaBroker = null;

        //EA Windows (implemented in DDPM.EABroker Assembly)
        // Need call InitEditWindow() to create below windows at EABroker_Start() stage
        private DDPM.EABroker.EAEditWindow? _editWindow = null;
        private DDPM.EABroker.SaveCustomWindow? _saveCustomWindow = null;
        private EAArgs? _eaArgs = null; //Temporary keep when EditCommand(), and add result when EditReturn

        //Edit Overlap Custom process ways
        //0 = DDM v2 : 1 Show SaveCustomWindow until click "Save"; 2 Frame windows, auto close in 3 sec
        //1 = DDPM : 1 Show EditWindow and SaveCustomWindow at the same time; 2 Wait until click "Save"
        private int _editOverlapCutsomWay = 0;
        #endregion Private Members

        #region Public members
        public static readonly Ioc PluginIoc = new();
        #endregion

        #region Constructor
        public EAPlugin(IAgent agent) : base(agent, PluginLogId)
        {
            _agent = agent;
            _IsAdministrator = ProcessSecurityHelperWrapper.IsCurrentProcessRunningElevated();
            _log = Log;
            //_vmArrange.Log = Log;

            AppDomain.CurrentDomain.ProcessExit += SAUser_ProcessExit;
            _log?.Info($"[{pluginName}] is constructed.");
        }

        private void SAUser_ProcessExit(object? sender, EventArgs e)
        {
            SplitJson.DisposePresetList();
            AppDomain.CurrentDomain.ProcessExit -= SAUser_ProcessExit;
        }

        #endregion Constructor

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
                    EABroker_Stop();
                    _agent = null;

                    _HotkeyPlugin.Unhook();
                    _HotkeyPlugin.KeyUp -= Keyboard_KeyUpProc;
                    _HotkeyPlugin.KeyDown -= Keyboard_KeyDownProc;
                }
                IsDisposed = true;
            }
            base.Dispose(disposing);
        }

        #endregion IDisposableObservable Support

        #region Overriding methods

        //The method is called by Agent when the plugin is starting
        protected override void OnPluginStarting()
        {
            PluginCondition = new PluginStartedCondition();
            _agent.PluginManager.PluginsStarted += PluginManagerOnPluginsStarted;

            //Monitoring plugins state
            //2024-8-13 Robert_Lin, EAPlugin has fixed .NET 8 issues, so uncomment below statements.
            // 2024-08-06 Elie, Mask InitializeDeviceManagerPlugin() function to skip .NET 8 for more than two monitor cause exception issue. ==> System.IO.IOException: 'Cannot locate resource 'eaworkwindow.baml'.'
            //if (isDebug20241008()) 
            {
                InitializeSettingsManagerPlugin();
                InitializeDeviceManagerPlugin();
                InitializeDisplayManagerPlugin();
                InitializeHotkeyPlugin();
            }
        }

        #endregion Overriding methods

        #region PluginManager related

        private void PluginManagerOnPluginsStarted(object? sender, PluginsStartedEventArgs e)
        {
            if (e == null)
                return;
            if (e.ChangedPlugins == null)
                return;
            if (e.ChangedPlugins.Any() == false)
                return;

            WriteLog($"@ PluginManagerOnPluginsStarted, ChangedPlugins.Count={e.ChangedPlugins.Count}");
        }

        // DeviceManager Plugin
        //
        private void InitializeDeviceManagerPlugin()
        {
            //If DeviceManager plugin is got already then return, prevent to call twice
            if (_deviceManagerPlugin != null || _agent == null)
                return;

            _deviceManagerPlugin = _agent.PluginManager.FindPluginByType<IDeviceManagerSA>(PluginResolution.Dynamic);

            if (_deviceManagerPlugin is IFrameworkPluginConditionNotification DeviceManagerCondition)
            {
                DeviceManagerCondition.PluginConditionChangeHandler += OnDeviceManagerPluginConditionChangeHandler;
                GetCurrentDeviceManagerPluginCondition();
            }
            WriteLog($"Initializing DeviceManager plugin.");
        }

        private void OnDeviceManagerPluginConditionChangeHandler(object? sender, EventArgs e)
        {
            GetCurrentDeviceManagerPluginCondition();
        }

        private void GetCurrentDeviceManagerPluginCondition()
        {
            _ = Task.Run(async () =>
            {
                if (_deviceManagerPlugin == null)
                    return;

                var pluginCondition = await (_deviceManagerPlugin as IFrameworkPluginConditionNotification)?.CurrentConditionAsync();

                lock (_PluginConditionLock)
                {
                    if (pluginCondition is PluginErrorCondition)
                    {
                        WriteLog($"DeviceManager plugin is in an error condition");
                        _deviceManagerPluginCondition = pluginCondition;
                        _deviceManagerPluginUsable = false;
                    }
                    else if (pluginCondition is PluginStartedCondition)
                    {
                        _log?.Info($"DeviceManager plugin is in a started condition");
                        _deviceManagerPluginCondition = pluginCondition;
                        _deviceManagerPluginUsable = true;
                        //_vmArrange.DeviceManager = _deviceManagerPlugin;
                        if (CheckIfReadyToStartEABorker())
                        {
                            ConfigureServices();
                            //Only after DisplayManager is ready to use, will start the EasyArrange service
                            WriteLog($"EABroker_Start from GetCurrentDeviceManagerPluginCondition");
                            EABroker_Start();
                        }
                    }
                    else
                    {
                        WriteLog($"DeviceManager plugin is in others condition");
                    }
                }
            });
        }

        // DisplayManager Plugin
        //
        private void InitializeDisplayManagerPlugin()
        {
            //If DisplayManager plugin is got already then return, prevent to call twice
            if (_displayManagerPlugin != null)
                return;

            _displayManagerPlugin = _agent.PluginManager.FindPluginByType<IDisplayService>(PluginResolution.Dynamic);

            if (_displayManagerPlugin is IFrameworkPluginConditionNotification DisplayManagerCondition)
            {
                DisplayManagerCondition.PluginConditionChangeHandler += OnDisplayManagerPluginConditionChangeHandler;
                GetCurrentDisplayManagerPluginCondition();
            }
        }

        private void GetCurrentDisplayManagerPluginCondition()
        {
            _ = Task.Run(async () =>
            {
                var pluginCondition = await (_displayManagerPlugin as IFrameworkPluginConditionNotification)?.CurrentConditionAsync();

                lock (_PluginConditionLock)
                {
                    if (pluginCondition is PluginErrorCondition)
                    {
                        WriteLog($"{nameof(GetCurrentDisplayManagerPluginCondition)} - Display ManagerPlugin is in an error condition");
                        _displayManagerPluginCondition = pluginCondition;
                        _displayManagerPluginUsable = false;
                    }
                    else if (pluginCondition is PluginStartedCondition)
                    {
                        _log?.Info($"{nameof(GetCurrentDisplayManagerPluginCondition)} -Display ManagerPlugin is in a started condition");
                        _displayManagerPluginCondition = pluginCondition;
                        _displayManagerPluginUsable = true;
                        //_vmArrange.DisplayManager = _displayManagerPlugin;
                        if (CheckIfReadyToStartEABorker())
                        {
                            ConfigureServices();
                            //Only after DisplayManager is ready to use, will start the EasyArrange service
                            WriteLog($"EABroker_Start from GetCurrentDisplayManagerPluginCondition");
                            EABroker_Start();
                        }
                    }
                }
            });
        }

        private void OnDisplayManagerPluginConditionChangeHandler(object? sender, EventArgs e)
        {
            GetCurrentDisplayManagerPluginCondition();
        }

        // SettingsManager Plugin
        //
        private void InitializeSettingsManagerPlugin()
        {
            //If SettingsManager plugin is got already then return, prevent to call twice
            if (_settingsManagerPlugin != null)
                return;

            _settingsManagerPlugin = _agent.PluginManager.FindPluginByType<ISettingsManagerDev>(PluginResolution.Dynamic);

            if (_settingsManagerPlugin is IFrameworkPluginConditionNotification SettingsManagerCondition)
            {
                SettingsManagerCondition.PluginConditionChangeHandler += OnSettingsManagerPluginConditionChangeHandler;
                GetCurrentSettingsManagerPluginCondition();
            }
            _log?.Info($"Initializing SettingsManager plugin.");
        }

        private void OnSettingsManagerPluginConditionChangeHandler(object? sender, EventArgs e)
        {
            GetCurrentSettingsManagerPluginCondition();
        }

        private void GetCurrentSettingsManagerPluginCondition()
        {
            _ = Task.Run(async () =>
            {
                var pluginCondition = await (_settingsManagerPlugin as IFrameworkPluginConditionNotification)?.CurrentConditionAsync();

                lock (_PluginConditionLock)
                {
                    if (pluginCondition is PluginErrorCondition)
                    {
                        WriteLog($"SettingsManager plugin is in an error condition");
                        _settingsManagerPluginCondition = pluginCondition;
                        _settingsManagerPluginUsable = false;
                    }
                    else if (pluginCondition is PluginStartedCondition)
                    {
                        WriteLog($"SettingsManager plugin is in a started condition");
                        _settingsManagerPluginCondition = pluginCondition;
                        _settingsManagerPluginUsable = true;
                        //Robert_Lin, 2024-11-6 Register a hander when SettingsMnager Init donw.
                        //We need to reload settings in that handler
                        _settingsManagerPlugin.SettingReadyEvent += _settingsManagerPlugin_SettingReadyEvent;
                        //_vmArrange.DeviceManager = _deviceManagerPlugin;
                        if (CheckIfReadyToStartEABorker())
                        {
                            ConfigureServices();
                            //Only after all required Plugins are ready to use, will start the EasyArrange service
                            WriteLog($"EABroker_Start from GetCurrentSettingsManagerPluginCondition");
                            EABroker_Start();
                        }
                    }
                    else
                    {
                        WriteLog($"SettingsManager plugin is in others condition");
                    }
                }
            });
        }

        //Called (event) when SettingsManager has init done
        private void _settingsManagerPlugin_SettingReadyEvent(object? sender, EventArgs e)
        {
            //If eaBroker
            if (_eaBroker != null)
            {
                _eaBroker.NotifySettingsManagerIsInitializedDone();
            }
            //Unregister the event handler
            if (_settingsManagerPlugin != null)
            {
                _settingsManagerPlugin.SettingReadyEvent -= _settingsManagerPlugin_SettingReadyEvent;
            }
        }

        //TelemetryScheduler Plugin
        //Derek 2025/03/30 due to 0 reference
        //private void InitializeTelementrySchedulerPlugin()
        //{
        //    if (_telementrySchedulerPlugin != null)
        //        return;

        //    _telementrySchedulerPlugin = _agent.PluginManager.FindPluginByType<ITelementryScheduler>(PluginResolution.Dynamic);

        //    if (_telementrySchedulerPlugin is IFrameworkPluginConditionNotification pluginCondition)
        //    {
        //        pluginCondition.PluginConditionChangeHandler += OnTelementrySchedulerConditionChangeHandler;
        //        GetCurrentTelementrySchedulerCondition();
        //    }
        //}
        //private void OnTelementrySchedulerConditionChangeHandler(object? sender, EventArgs e)
        //{
        //    GetCurrentTelementrySchedulerCondition();
        //}
        //private void GetCurrentTelementrySchedulerCondition()
        //{
        //    _ = Task.Run(async () =>
        //    {
        //        var pluginCondition = await (_telementrySchedulerPlugin as IFrameworkPluginConditionNotification)?.CurrentConditionAsync();
        //        lock (_PluginConditionLock_TelementryScheduler)
        //        {
        //            if (pluginCondition is PluginErrorCondition)
        //            {
        //                WriteLog($"{nameof(GetCurrentTelementrySchedulerCondition)} - Telementry Scheduler is in an error condition");
        //                _telementrySchedulerPluginUsable = false;
        //            }
        //            else if (pluginCondition is PluginRunningCondition)
        //            {
        //                WriteLog($"{nameof(GetCurrentTelementrySchedulerCondition)} - Telementry Scheduler is in a running condition");

        //                if (_GlobalSettingParam != null)
        //                {
        //                    WriteLog(nameof(GetCurrentTelementrySchedulerCondition) + " Call GetGlobalsetting_IsTelemetryConsentOn:");
        //                    _telementrySchedulerPlugin.GetGlobalsetting_IsTelemetryConsentOn(_GlobalSettingParam.isTelemetryConsentOn);
        //                    _telementrySchedulerPluginUsable = true;
        //                }
        //                else
        //                {
        //                    WriteLog(nameof(GetCurrentTelementrySchedulerCondition) + " _GlobalSettingParam is null");
        //                    _telementrySchedulerPluginUsable = false;
        //                }
        //            }
        //            else if (pluginCondition is PluginStartedCondition)
        //            {
        //                WriteLog($"{nameof(GetCurrentTelementrySchedulerCondition)} - Telementry Scheduler is in a started condition");

        //                if (_GlobalSettingParam != null)
        //                {
        //                    WriteLog(nameof(GetCurrentTelementrySchedulerCondition) + " Call GetGlobalsetting_IsTelemetryConsentOn:");
        //                    _telementrySchedulerPlugin.GetGlobalsetting_IsTelemetryConsentOn(_GlobalSettingParam.isTelemetryConsentOn);
        //                    _telementrySchedulerPluginUsable = true;
        //                }
        //                else
        //                {
        //                    WriteLog(nameof(GetCurrentTelementrySchedulerCondition) + " _GlobalSettingParam is null");
        //                    _telementrySchedulerPluginUsable = false;
        //                }
        //            }
        //        }
        //    });
        //}
        //End Derek 2025/03/30

        // Hotkey Plugin
        //
        private void InitializeHotkeyPlugin()
        {
            if (_HotkeyPlugin != null)
                return;

            _HotkeyPlugin = _agent.PluginManager.FindPluginByType<IHotkey>(PluginResolution.Dynamic);
            if (_HotkeyPlugin is IFrameworkPluginConditionNotification pluginCondition)
            {
                pluginCondition.PluginConditionChangeHandler += OnHotkeyPluginConditionChangeHandler;
                GetCurrentHotkeyPluginCondition();
            }
        }
        private void OnHotkeyPluginConditionChangeHandler(object? sender, EventArgs e)
        {
            GetCurrentHotkeyPluginCondition();
        }
        private void GetCurrentHotkeyPluginCondition()
        {
            _ = Task.Run(async () =>
            {
                var pluginCondition = await (_HotkeyPlugin as IFrameworkPluginConditionNotification)?.CurrentConditionAsync();
                //PluginCondition _HotkeyPluginCondition;
                lock (_PluginConditionLock_Hotkey)
                {
                    if (pluginCondition is PluginErrorCondition)
                    {
                        WriteLog($"{nameof(GetCurrentHotkeyPluginCondition)} - Hotkey Plugin is in an error condition");
                        //_HotkeyPluginCondition = pluginCondition;
                        //unhook keyboard
                        _HotkeyPlugin.KeyUp -= Keyboard_KeyUpProc;
                        _HotkeyPlugin.KeyDown -= Keyboard_KeyDownProc;
                        _HotkeyPlugin.Unhook();
                    }
                    else if (pluginCondition is PluginRunningCondition)
                    {
                        WriteLog($"{nameof(GetCurrentHotkeyPluginCondition)} - Hotkey Plugin is in a running condition");
                    }
                    else if (pluginCondition is PluginStartedCondition)
                    {
                        WriteLog($"{nameof(GetCurrentHotkeyPluginCondition)} - Hotkey Plugin is in a started condition");
                        //_HotkeyPluginCondition = pluginCondition;
                        //hook keyboard
                        _HotkeyPlugin.Hook();
                        _HotkeyPlugin.KeyUp += Keyboard_KeyUpProc;
                        _HotkeyPlugin.KeyDown += Keyboard_KeyDownProc;
                    }
                }
            });
        }

        private GlobalSettingParam? _GlobalSettingParam
        {
            get
            {
                if (_globalSettingParam == null)
                {
                    if (_deviceManagerPlugin == null)
                        return null;
                    _globalSettingParam = _deviceManagerPlugin.GetGlobalSettingParam().Result;
                }
                return _globalSettingParam;
            }
        }


        private object _lockCheckIfReadyToStartEABorker = new object();

        /// <summary>
        /// Determine if all depended DDPM.SA plugins are ready to start EABroker, which will initiate
        /// EasyArrange subagent to run. 
        /// </summary>
        /// <returns></returns>
        private bool CheckIfReadyToStartEABorker()
        {
            lock (_lockCheckIfReadyToStartEABorker)
            {
                //[For Debuging] Skip waiting until both DeviceManager and DisplayManager are ready
                // Please comment out below line for release build
                //return true;

                //If both DeviceManager and DisplayManager are ready to call
                if (!_displayManagerPluginUsable || !_deviceManagerPluginUsable || !_settingsManagerPluginUsable)
                {
                    //Robert_Lin, 2025-1-7 remove this log information
                    //Either DisplayManager or DeviceManager is not ready
                    //_log?.Info("@ CheckIfReadyToStartEABorker: DisplayManager, SettingsManager, or DeviceManager not ready.");
                    return false;
                }
                //If EABroker is already started
                if (_isEaBrokerStarted)
                {
                    //EABroker is started already
                    WriteLog("@ CheckIfReadyToStartEABorker: EABroker is started already.");
                    return false;
                }

                return true;
            }
        }
        #endregion PluginManager related

        #region IEasyArrangeService Implementation

        #region Properties
        //Robert_Lin, 2024-12-15, This property will be removed, for CLI, please use
        //Get/Set EASelectedLayout() method instead.
        //Robert_Lin, 2024-10-23, this flag should be saved in user settings 
        //Temporary always true
        //public bool IsFunctionEnabled
        //{
        //    get
        //    {
        //        return true;
        //    }
        //    set
        //    {

        //    }
        //}

        /// <summary>
        /// The last error string after a EAPlugin method return error.
        /// </summary>
        public string EALastError
        {
            get
            {
                if (_eaBroker == null)
                    return "EABroker is null";
                if (!_isEaBrokerStarted)
                    return "EABroker is not started.";
                if (_eaBroker.VM == null)
                    return "EABroker.VM is null";
                return _eaBroker.VM.EAPluginLastError;
            }
        }
        #endregion Properties

        #region Events
        /// <summary>
        /// Notify to DDPM.UI (EzArrangeModule) that the EditCommand request has been accepted.
        /// The EAEditWindow is working for user. 
        /// Argument string: return with errMsg. If errMsg is empty it means no error.
        /// DDPM.UI should wait for next EditReturn event.
        /// </summary>
        public event EventHandler<string> EditStarted;

        /// <summary>
        /// The EditComand request has been finished and return the result in EAArg argument.
        /// </summary>
        public event EventHandler<EAArgs> EditReturn;

        /// <summary>
        /// To DDPM.UI EzArrange module when the Settings file has been changed from SA side.
        /// </summary>
        public event EventHandler<EAArgs> EASettingsChanged;
        #endregion Events

        #region Methods


        /// <summary>
        /// Called from DDPM.UI, when user's selection changed.
        /// Robert_Lin, 2024-10-6: This method may be deprecated after confirm that can be replaced with NotifyEASelectedLayoutChanged()
        /// </summary>
        /// <param name="monitorInfo"></param>
        /// <param name="cellCount"></param>
        /// <param name="splitKey"></param>
        /// <param name="settings"></param>
        /// <returns></returns>
        public Task<bool> SetEAWrokSplit(MonitorInfo monitorInfo, int cellCount, char splitKey, List<double>? settings)
        {
            throw new NotImplementedException("EAPlugin.SetEAWrokSplit() is deprecated method.");

            if (_eaBroker != null)
            {
                _eaBroker.SetWorkSplit(monitorInfo, cellCount, splitKey, settings);
                return Task.FromResult(true);
            }

            //EAWorkWindow? workWin = _vmArrange.FindWorkWindowByDisplayName2(monitorInfo.DisplayName);
            //if (workWin == null)
            //{
            //    return Task.FromResult(false);
            //}

            //bool res = workWin.SetWorkingSplit(cellCount, splitKey, settings);

            return Task.FromResult(false);

        }

        /// <summary>
        /// Called from DDPM.UI, when user select a layout. UI will update UI and save setting after changed.
        /// This method will only notify working windows to update their UI only.
        /// </summary>
        /// <returns></returns>
        public Task<bool> NotifyEASelectedLayoutChanged(MonitorInfo monitorInfo, SplitJson spJson)
        {
            if (_eaBroker != null)
            {
                bool res = _eaBroker.NotifyEASelectedLayoutChanged(monitorInfo, spJson);
                return Task.FromResult(res);
            }
            return Task.FromResult(false);
        }


        // EditCommand() and related events (Robert_Lin 2024-0910)
        // 1 UI call EditCommand() to initiate a Edit command to edit a layout.
        // 2 UI_EditCommand() will try to show the EAEditWindow for the editing
        //   If failed, will send a EditReturn(errMsg) with error message in errMsg.
        // 3 UI_EditCommand() will sent EditStarted("") with empty string to UI.
        // 4 UI receive a EditStarted event and errMsg is empty, it will minimize itself to taskbar.
        // 5 User will edit the layout in EAEditWindow.
        // 6 User will click "Save" or "Cancel" button in SaveCustomWindow when it edit finished or
        //   cancel the editing.
        // 7 SaveCustomWindow will notify to EAPlugin which is handled by
        //   saveCustomWidow_CancelButtonClick() or saveCustomWidow_SaveButtonClick()
        // 8 EAPlugin will notify UI the editing result with EditReturn event


        /// <summary>
        /// Request EAPlugin to do Edit Custom Layout.
        /// </summary>
        /// <param name="monitorInfo">MonitorInfo to identify which monitor.</param>
        /// <param name="args">EAArgs contains the arguments</param>
        /// <returns>
        /// False: EAPlugin reject the command. Possible reason: EAPlugin is under Editing state.
        /// True: EAPlugin accept the command, caller must wait for EditStarted or EditCompleted event</returns>
        public Task<bool> EditCommand(MonitorInfo monitorInfo, EAArgs args)
        {
            Trace.WriteLine("@ EditCommand()");
            Trace.WriteLine($"  * Monitor.Model=[{monitorInfo.modelName}], ServiceTag=[{monitorInfo.edid.ServiceTag}]");
            Trace.WriteLine($"  * EAArgs.Split=[{args.SplitJson.CellCount}{args.SplitJson.SplitKey}], CustomName=[{args.SplitJson.CustomName}]");

            //Robert_Lin, 2024-12-20, LogInfo will be removed, used WriteLog() instead.
            //Add LogInfo in this file will be chaned to WriteLog()

            //LogInfo($"@EAPlugin.EditCommand(Monitor:{monitorInfo.modelName}[{monitorInfo.edid.ServiceTag}],EAArgs:{args.SplitJson.CellCount}{args.SplitJson.SplitKey}[{args.SplitJson.CustomName}])");
            WriteLog($"@EAPlugin.EditCommand(Monitor:{monitorInfo.modelName}[{monitorInfo.edid.ServiceTag}],EAArgs:{args.SplitJson.CellCount}{args.SplitJson.SplitKey}[{args.SplitJson.CustomName}])");

            //Validation
            //
            if (!_isEaBrokerStarted)
            {
                WriteLog("@EAPlugin.EditCommand(), _isEaBrokerStarted is false.");
                return Task.FromResult(false);
            }
            if (_eaBroker == null)
            {
                WriteLog("@EAPlugin.EditCommand(), _eaBroker is null.");
                return Task.FromResult(false);
            }
            if (_eaBroker?.RunningState != eEARunningStates.Waiting)
            {
                WriteLog($"@EAPlugin.EditCommand(), EABroker.RunningState is {_eaBroker?.RunningState}.");
                return Task.FromResult(false);
            }

            //If InitEditWindow() not been called or failed.
            //if (_editWindow == null)
            //{
            //    LogInfo("@EAPlugin.EditCommand(), _editWindow is null.");
            //    return Task.FromResult(false);
            //}
            //if (_saveCustomWindow == null)
            //{
            //    LogInfo("@EAPlugin.EditCommand(), _saveCustomWindow is null.");
            //    return Task.FromResult(false);
            //}

            //Launch the major function in UI Thread
            Thread thread = new Thread(() =>
            {
                if (!STA_EditCommand(monitorInfo, args))
                    return;

                WriteLog("@ EditCommand(), Entering Dispatcher.Run().");
                System.Windows.Threading.Dispatcher.Run();
                WriteLog("@ EditCommand(), exit from Dispatcher.Run().");
            });

            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            WriteLog("@ EditCommand(), STA Thread is started.");

            return Task.FromResult(true);
        }

        //Derek 2028/03/31
        private void ExitUIThread()
        {
            if (System.Windows.Threading.Dispatcher.CurrentDispatcher != null)
            {
                System.Windows.Threading.Dispatcher.CurrentDispatcher.InvokeShutdown();
            }
        }

        private OverlapWindow? _overlapWindow = null;
        /// <summary>
        /// EditCommand() function which is running under STA thread.
        /// In this method, it must return a EditStarted event to UI, to tell UI
        /// 1) The EditWindow is shown and start edit with "" (empty string) argument,
        /// 2) Something wrong so caould not start the edit with error message as the argument.
        /// </summary>
        /// <param name="monitorInfo"></param>
        /// <param name="args"></param>
        /// <returns>
        /// false: the STA thread can be terminated.
        /// true: the STA thread need to keep running, STA_EditCommand will call Dispatcher.InvokeShutdown() to terminate the STA thread.
        /// </returns>
        private bool STA_EditCommand(MonitorInfo monitorInfo, EAArgs args)
        {
            //Should be never, these flags are checked alaredy in EditCommand()
            if (_eaBroker == null)
            {
                WriteLog("@ STA_EditCommand(), exit due to _eaBroker is null.");

                return false;
            }
            //if (_editWindow == null)
            //    return false;
            //if (_saveCustomWindow == null)
            //    return false;

            Trace.WriteLine("@ STA_EditCommand()");
            Trace.WriteLine($"  * Monitor.Model=[{monitorInfo.modelName}], ServiceTag=[{monitorInfo.edid.ServiceTag}]");
            Trace.WriteLine($"  * EAArgs.Split=[{args.CellCount}{args.SplitKey}], CustomName=[{args.CustomName}]");

            //Robert_Lin, 2024-12-6, use the method in CommonFunctions
            double dpiX = CommonFunctions.GetDpiX();
            //double dpiX = 1.000;
            //var dpiXProperty = typeof(SystemParameters).GetProperty("DpiX", BindingFlags.NonPublic | BindingFlags.Static);
            //if (dpiXProperty != null)
            //{
            //    var varX = (int)dpiXProperty.GetValue(null, null);
            //    dpiX = (double)varX / (double)96;
            //}

            //Get the DisplayName from MonitorInfo
            string displayName = monitorInfo.DisplayName;
            //Get the target Screen from the displayName
            Screen? scr = Screen.AllScreens.FirstOrDefault(x => x.DeviceName.Equals(displayName, StringComparison.OrdinalIgnoreCase));
            //If no matched screen found, then report error
            if (scr == null)
            {
                if (EditStarted != null)
                {
                    EditStarted(this, $"Cannot find a Screen from monitorInfo. MonitorInfo.DisplayName=[{monitorInfo.DisplayName}]");
                }

                return false;
            }
            bool isVertical = (scr.Bounds.Width < scr.Bounds.Height);

            _eaArgs = args;

            //Determine the WorkingArea
            //
            Rectangle workingArea = new Rectangle();
            if (_eaBroker.VM.IsSpanScreenWorking)
            {
                workingArea = _eaBroker.VM.SpanWorkingArea;
            }
            else
            {
                workingArea = scr.WorkingArea;
            }

            //Show EditWindow and SaveCustomWindow, and start editing
            //
            if (_eaArgs.SplitJson.IsOverlapLayout)
            {
                if (_editOverlapCutsomWay == 0)
                {
                    //1 Signal EditStart event to UI, UI will Minimized to taskbar
                    if (EditStarted != null)
                        EditStarted(this, "");

                    //2 To notify EABroker, we are in Edit process, disable WorkWindow/AwsWindow
                    _eaBroker.VM.IsWorkUIEnabled = false;
                    _eaBroker.RunningState = eEARunningStates.Edit;

                    //3 Show SaveCustomWindow, until user click Save or Cancel
                    //_saveCustomWindow = new EABroker.SaveCustomWindow(_deviceManagerPlugin);
                    //_saveCustomWindow.CancelButtonClick += saveCustomWidow_CancelButtonClick;
                    //_saveCustomWindow.SaveButtonClick += saveCustomWidow_SaveButtonClick;
                    ////_saveCustomWindow.ShowAndEdit(args, workingArea);
                    //_saveCustomWindow.ShowAndEdit_v1(args, scr);

                    int x = workingArea.X + (int)(32 /dpiX);
                    int y = workingArea.Y + (int)(32 /dpiX);
                    _saveCustomWindow = new EABroker.SaveCustomWindow(_deviceManagerPlugin, args, x, y);
                    bool? dlgResult = _saveCustomWindow.ShowDialog();
                    if (dlgResult != true)
                    {
                        //if ((EditReturn != null) && (_eaArgs != null))
                        //{
                        //    EAArgs retArgs = new EAArgs(_eaArgs);
                        //    retArgs.Result = false;
                        //    retArgs.Command = "EditReturn";
                        //    retArgs.Message = "User cancel the editing.";
                        //    EditReturn(this, retArgs);
                        //}
                        SendEditReturn_Cancel("User cancel the editing.");
                        if (_eaBroker != null)
                        {
                            _eaBroker.VM.IsWorkUIEnabled = true;
                            _eaBroker.RunningState = eEARunningStates.Waiting;
                        }

                        ExitUIThread();
                        return false;
                    }

                    //Get the selected/edited CustomName from SaveCustomWindow
                    string customName = "";
                    if (_saveCustomWindow.SelectedCustomItem != null)
                    {
                        customName = _saveCustomWindow.SelectedCustomItem.CustomName;
                    }

                    WriteLog($"SaveCustomWindow.SaveClicked, CutomName=[{customName}]");

                    _overlapWindow = new OverlapWindow(_log!);
                    _overlapWindow.Closing += _overlapWindow_Closing; //Derek 2025/03/31
                    //Handler of CaptureDone
                    //After CaptureOverlapLayout() finished it job and returned.
                    //The output (OverlapWindow.SplitCtrl) has ready to get.
                    //But we would like to wait for the delay of OverlapWindow (3 sec)
                    //Until its delay finished, then we report UI 'Done' with the result.
                    _overlapWindow.CaptureDone += delegate
                    {
                        EAArgs retArgs = new EAArgs(_eaArgs);
                        retArgs.Command = "EditReturn";
                        retArgs.Result = true;
                        retArgs.SplitJson = new SplitJson()
                        {
                            CellCount = 0,
                            SplitKey = 'B',
                            Settings = new List<double>()
                        };
                        //Update from OverlapWindow.SplitCtrl
                        if (_overlapWindow.SplitCtrl != null)
                        {
                            retArgs.SplitJson = new SplitJson()
                            {
                                CellCount = _overlapWindow.SplitCtrl.CellCount, 
                                SplitKey = _overlapWindow.SplitCtrl.SplitKey, 
                                Settings = new List<double>(_overlapWindow.SplitCtrl.Settings)
                            };
                        }
                        //Update from SaveCustomWindow
                        if (_saveCustomWindow.SelectedCustomItem != null)
                        {
                            //CustomName will copy from SaveCustomWindow
                            retArgs.SplitJson.CustomName = _saveCustomWindow.SelectedCustomItem.CustomName;

                            //If user has selected an existed custom layout
                            if (_saveCustomWindow.SelectedCustomItem.EAID >= EAEMConstants.EAID_FirstCustom)
                            {
                                retArgs.SplitJson.EAID = _saveCustomWindow.SelectedCustomItem.EAID;
                            }

                        }
                        //Send the EditReturn event to UI
                        if (EditReturn != null)
                            EditReturn(this, retArgs);

                        if (_eaBroker != null)
                        {
                            _eaBroker.VM.IsWorkUIEnabled = true;
                            _eaBroker.RunningState = eEARunningStates.Waiting;
                        }
                    }; //_overlapWindow.CaptureDone += delegate

                   // _overlapWindow.Show();

                    int addCount = 0;
                    if (_eaBroker.VM.IsSpanScreenWorking)
                    {
                        _overlapWindow.Left = workingArea.Left / dpiX;
                        _overlapWindow.Top = workingArea.Top / dpiX;
                        _overlapWindow.Width = workingArea.Width / dpiX;
                        _overlapWindow.Height = workingArea.Height / dpiX;

                        Trace.WriteLine($"WorkingArea: {workingArea.Width}x{workingArea.Height}");
                        _overlapWindow.Show();
                        addCount = _overlapWindow.CaptureOverlapLayoutByWorkingArea(workingArea);
                    }
                    else
                    {
                        _overlapWindow.Show();
                        addCount = _overlapWindow.CaptureOverlapLayout(scr);

                    }
                    if (addCount <= 0)
                    {
                        //Robert_Lin, 2024-12-4
                        //There no any window on the target screen.
                        //Reference to DDM v2, it will return (no cell) to UI.
                        //DDPM v2.0 will follow it
                    }
                }
                else //_editOverlapCutsomWay=1
                {
                    //DO NOT set _editOverlapCutsomWay=1, issues not been fixed
                    //Robert_Lin, 2024-12-19 comment-out the unused code
                    /*
                    //1 Show EditWindow
                    _editWindow = new EABroker.EAEditWindow(_log);
                    //if (!_editWindow.ShowAndEdit_v1(args, workingArea))
                        if (!_editWindow.ShowAndEdit_v1(args, scr))
                        {
                            if (EditStarted != null)
                        {
                            EditStarted(this, "Error");
                        }
                        return false;
                    }

                    //2 Show SaveCustomWindow at the same time
                    _saveCustomWindow = new EABroker.SaveCustomWindow(_deviceManagerPlugin);
                    _saveCustomWindow.Owner = _editWindow;
                    _saveCustomWindow.CancelButtonClick += saveCustomWidow_CancelButtonClick;
                    _saveCustomWindow.SaveButtonClick += saveCustomWidow_SaveButtonClick;
                    //_saveCustomWindow.ShowAndEdit(args, workingArea);
                    _saveCustomWindow.ShowAndEdit_v1(args, scr);

                    //3 Signal EditStart event to UI, UI will Minimized to taskbar
                    if (EditStarted != null)
                        EditStarted(this, "");

                    //4 To notify EABroker, we are in Edit process, disable WorkWindow/AwsWindow
                    _eaBroker.VM.IsWorkUIEnabled = false;
                    */
                }
            }
            else //Non-Overlap layout
            {
                //Non-Overlap edit steps
                //1 Show the layout for editing
                _editWindow = new EABroker.EAEditWindow(_log);
                _editWindow.Closed += _editWindow_Closed;
                //Robert_Lin 2025-3-19 remove the duplicate code
                //_saveCustomWindow = new EABroker.SaveCustomWindow(_deviceManagerPlugin);
                if (!_editWindow.ShowAndEdit(args, workingArea))
                {
                    if (EditStarted != null)
                    {
                        EditStarted(this, "Error");
                    }

                    return false;
                }

                //2 Show the SaveCustomWindow in the same time
                int x = workingArea.X + (int)(32 / dpiX);
                int y = workingArea.Y + (int)(32 / dpiX);

                _saveCustomWindow = new EABroker.SaveCustomWindow(_deviceManagerPlugin, args, x, y);
                _saveCustomWindow.Owner = _editWindow;
                _saveCustomWindow.CancelButtonClick += delegate
                {
                    SendEditReturn_Cancel("User cancel the editing.");
                    if (_editWindow != null)
                    {
                        _editWindow.Close();
                        _editWindow = null;
                    }
                    if (_saveCustomWindow != null)
                    {
                        _saveCustomWindow.Close();
                        _saveCustomWindow = null;
                    }

                    if (_eaBroker != null)
                    {
                        _eaBroker.VM.IsWorkUIEnabled = true;
                        _eaBroker.RunningState = eEARunningStates.Waiting;
                    }

                    ExitUIThread();
                };
                _saveCustomWindow.SaveButtonClick += delegate
                {
                    EAArgs retArgs = new EAArgs(_eaArgs);
                    retArgs.Command = "EditReturn";
                    retArgs.Result = true;
                    //Update new settings from EditWindow
                    if (_editWindow != null)
                    {
                        retArgs.SplitJson.Settings = _editWindow.GetSettings();
                    }
                    //Update from SaveCustomWindow
                    if (_saveCustomWindow.SelectedCustomItem != null)
                    {
                        //CustomName will copy from SaveCustomWindow
                        retArgs.SplitJson.CustomName = _saveCustomWindow.SelectedCustomItem.CustomName;

                        //If user has selected an existed custom layout
                        if (_saveCustomWindow.SelectedCustomItem.EAID >= EAEMConstants.EAID_FirstCustom)
                        {
                            retArgs.SplitJson.EAID = _saveCustomWindow.SelectedCustomItem.EAID;
                        }
                    }
                    //Send the EditReturn event to UI
                    if (EditReturn != null)
                        EditReturn(this, retArgs);

                    if (_eaBroker != null)
                    {
                        _eaBroker.VM.IsWorkUIEnabled = true;
                        _eaBroker.RunningState = eEARunningStates.Waiting;
                    }

                    if (_editWindow != null)
                    {
                        _editWindow.Close();
                        _editWindow = null;
                    }
                    if (_saveCustomWindow != null)
                    {
                        _saveCustomWindow.Close();
                        _saveCustomWindow = null;
                    }

                    ExitUIThread();
                };

                //_saveCustomWindow.ShowAndEdit(args, workingArea);
                _saveCustomWindow.Show();

                //3 Signal EditStart event to UI, UI will Minimized to taskbar
                if (EditStarted != null)
                    EditStarted(this, "");

                //4 To notify EABroker, we are in Edit process, disable WorkWindow/AwsWindow
                _eaBroker.VM.IsWorkUIEnabled = false;
                _eaBroker.RunningState = eEARunningStates.Edit;

                //5 Wait for user click "Save" or "Cancel"
            }

            return true;
        }

        private void _editWindow_Closed(object? sender, EventArgs e)
        {
            if (_editWindow != null)
                _editWindow.Closed -= _editWindow_Closed;

            ExitUIThread();
        }

        private void _overlapWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            if (_overlapWindow != null)
            {
                _overlapWindow.Closing -= _overlapWindow_Closing;
                _overlapWindow = null;
            }

            ExitUIThread();
        }

        /// <summary>
        /// Called by DeviceManagerSA when UI call WriteEzSettings_XXXXXX() method to update EzSettings.
        /// In EAPluging, will reload EzSettings from DDPMSettings file, and update to ArrangeViewModel.
        /// </summary>
        /// <returns></returns>
        public Task<bool> ReloadEzSettings()
        {
            if (_eaBroker != null)
            {
                if (_deviceManagerPlugin != null)
                {
                    WriteLog($"[EAPlugin] ReloadEzSettingsFromUserSettingsFile by ReloadEzSettings"); //add for debug
                    _eaBroker.VM.ReloadEzSettingsFromUserSettingsFile();
                    return Task.FromResult(true);
                }
                return Task.FromResult(false);
            }

            ////Reload settings
            //if ((_deviceManagerPlugin != null) && (_vmArrange != null))
            //{
            //    _vmArrange.EzSettings = _deviceManagerPlugin.ReadEzSettings().Result;
            //    _vmArrange.LogInfo($"@ EAPlugin.ReloadEzSettings(), Refresh values: IsWidthoutGap={_vmArrange.EzSettings.IsWidthoutGap}, IsOnlyShift={_vmArrange.EzSettings.IsOnlyAllowWhenShiftKeyPressed}, IsSpan ={_vmArrange.EzSettings.IsSpanAcrossMultiMonitors}, IsAwsEnabled ={_vmArrange.EzSettings.IsAwsEnabled}");
            //    return Task.FromResult(true);
            //}
            return Task.FromResult(false);
        }

        #region Set Selected Layout
        /// <summary>
        /// Set Selected EA Layout by EAID (Robert_Lin, 2024-12-13, wait for CLI verification)
        /// Fully simulate the secnario that user select a layout from DDPM UI.
        /// 1. Set the specified layout (by EAID) as selected layout.
        /// 2. (if not exist then) Add to Recent list.
        /// 3. Save the changed to EAMonitorSettings.
        /// 4. Notify SA.EAPlugin (EABroker) to update/refresh. 
        /// 5. User will see the selected layout shown and auto fade-out animation.
        /// 6. Notify UI to reload settings.
        /// </summary>
        /// <param name="monitorInfo">It can set to null, if eaId>=1000. </param>
        /// <param name="eaId">0=Off, [1~49]=Preset layout, [1000~1004]=Custom Layout.</param>
        /// <returns></returns>
        public Task<bool> SetEASelectedLayout(MonitorInfo monitorInfo, int eaId)
        {
            //Robert_Lin, 2024-12-20, LogInfo will be removed, used WriteLog() instead.
            //Add LogInfo in this file will be chaned to WriteLog()

            if (_eaBroker == null)
            {
                WriteLog($"SetEASelectedLayout({eaId}) return false: _eaBroker is null.");
                return Task.FromResult(false);
            }
            if (!_isEaBrokerStarted)
            {
                WriteLog($"SetEASelectedLayout({eaId}) return false: _eaBroker is not started.");
                return Task.FromResult(false);
            }
            if (_deviceManagerPlugin == null)
            {
                WriteLog($"SetEASelectedLayout({eaId}) return false: _deviceManagerPlugin is not started.");
                return Task.FromResult(false);
            }
            if (_eaBroker.VM == null)
            {
                WriteLog($"SetEASelectedLayout({eaId}) return false: EABroker.VM is null.");
                return Task.FromResult(false);
            }

            //Validate eaId: 0=Off, [1~49]=Preset, [1000~1004]=Custom
            //[0 ~ 49]
            if (ISplitCtrl.IsExistedPresetEAID(eaId))
            {
            }
            else if (eaId >= EAEMConstants.EAID_FirstCustom)
            {
                //Custom: will check if exist in CustomList, from UserSettings

                SplitJson[] customArray = _deviceManagerPlugin.ReadEACustomList().Result;
                if (customArray != null)
                {
                    List<SplitJson> customList = customArray.ToList<SplitJson>();
                    //Check if it's exist in CustomList
                    SplitJson? cusSplit = customList.Find(x => x.EAID == eaId);
                    if (cusSplit == null)
                    {
                        _eaBroker.VM.EAPluginLastError = $"EAID(={eaId}) not found in CustomList.";
                        WriteLog($"SetEASelectedLayout({eaId}) return false: {_eaBroker.VM.EAPluginLastError}");
                        return Task.FromResult(false);
                    }
                }
            }
            else
            {
                //[50~999] Invalid EAID
                _eaBroker.VM.EAPluginLastError = $"EAID(={eaId}) is not a valid EA layout ID.";
                WriteLog($"SetEASelectedLayout({eaId}) return false: {_eaBroker.VM.EAPluginLastError}");
                return Task.FromResult(false);
            }

            //Read EAMonitorSettings by MonitorInfo
            EAMonitorSettings? eaSettings = _eaBroker.VM.ReadEAMonitorSettings(monitorInfo);
            if (eaSettings == null)
            {
                _eaBroker.VM.EAPluginLastError = "ReadEAMonitorSettings() return null.";
                WriteLog($"SetEASelectedLayout({eaId}) return false: {_eaBroker.VM.EAPluginLastError}");
                return Task.FromResult(false);
            }

            //Check if current selected layout is the same
            if (eaSettings.SelectedSplit != null &&
                eaSettings.SelectedSplit.EAID == eaId)
            {
                WriteLog($"SetEASelectedLayout({eaId}) return true: Current selected layout is the same, nothing to do.");
                return Task.FromResult(true);
            }

            //Launch the major function in UI Thread
            Thread thread = new Thread(() =>
            {
                if (!STA_SetEASelectedLayout(monitorInfo, eaId))
                    return;

                WriteLog("STA_SetEASelectedLayout thread start");
                //System.Windows.Threading.Dispatcher.Run();
                WriteLog("STA_SetEASelectedLayout thread end");
            });

            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();

            return Task.FromResult(true);

        }

        private bool STA_SetEASelectedLayout(MonitorInfo monitorInfo, int eaId)
        {
            //Robert_Lin, 2024-12-20, LogInfo will be removed, used WriteLog() instead.
            //Add LogInfo in this file will be changed to WriteLog()

            if (_eaBroker == null)
            {
                WriteLog($"STA_SetEASelectedLayout({eaId}) return false: _eaBroker is null.");

                return false;
            }
            if (_eaBroker.VM == null)
            {
                WriteLog($"SetEASelectedLayout({eaId}) return false: EABroker.VM is null.");

                return false;
            }

            WriteLog($"@ STA_SetEASelectedLayout({monitorInfo.modelName}, {eaId})");

            //Read EAMonitorSettings for SelectedLayout and RecentList
            EAMonitorSettings? eaSettings = _eaBroker.VM.ReadEAMonitorSettings(monitorInfo);
            if (eaSettings == null)
            {
                _eaBroker.VM.EAPluginLastError = "ReadEAMonitorSettings() return null.";
                WriteLog($"STA_SetEASelectedLayout({eaId}) return false: {_eaBroker.VM.EAPluginLastError}");

                return false;
            }

            //Update Selected Layout to eaSettings.SelectedLayout
            //
            //If the eaId is a preset layout
            if (ISplitCtrl.IsExistedPresetEAID(eaId))
            {
                ISplitCtrl? isp = ISplitCtrl.Create(eaId);
                eaSettings.SelectedSplit = new SplitJson()
                {
                    CellCount = isp.CellCount,
                    SplitKey = isp.SplitKey,
                    EAID = eaId,
                    Settings = new List<double>(isp.Settings)
                };
                WriteLog($"@STA_SetEASelectedLayout, PresetLayout {isp.CellCount}{isp.SplitKey}");
            }
            //eaId is a custom layout, need to read settings from CustomList (from UserSettings)
            else if (eaId >= EAEMConstants.EAID_FirstCustom)
            {
                //Read CustomList
                SplitJson[] customArray = _deviceManagerPlugin.ReadEACustomList().Result;
                if (customArray != null)
                {
                    List<SplitJson> customList = customArray.ToList<SplitJson>();
                    //Check if it's exist in CustomList
                    SplitJson? cusSplit = customList.Find(x => x.EAID == eaId);
                    if (cusSplit == null)
                    {
                        _eaBroker.VM.EAPluginLastError = $"Specified EAID({eaId}) is not found in custom list.";
                        WriteLog($"STA_SetEASelectedLayout({eaId}) return false: {_eaBroker.VM.EAPluginLastError}");
                        return false;
                    }
                    eaSettings.SelectedSplit = cusSplit.Clone();
                    WriteLog($"@STA_SetEASelectedLayout, CustomLayout {cusSplit.CellCount}{cusSplit.SplitKey} [{cusSplit.CustomName}]");
                }
                else
                {
                    //Should naver to here, ReadEACustomList() never return null.
                    _eaBroker.VM.EAPluginLastError = "ReadEACustomList() return null.";
                    WriteLog($"STA_SetEASelectedLayout({eaId}) return false: {_eaBroker.VM.EAPluginLastError}");

                    return false;
                }
            }

            //Update Recent List
            //
            //1 eaId==0 (Off) => no need to update RecentList
            //2 SelectedSplait is null => EAMonitorSettings default value => assume SelectedLayout is Off (EAID=0)
            if ((eaId != 0) && (eaSettings.SelectedSplit != null))
            {
                //RecentList should never null, even if EAMonitorSetting is default, it will contains 5 default items.
                //In this method, we will not report error but skip to update.
                if (eaSettings.RecentList != null)
                {
                    //Conver array to List, in order to use List.Find
                    List<SplitJson> recentList = new List<SplitJson>(eaSettings.RecentList);
                    string recentListString = $"[{recentList[0].EAID}";
                    foreach (SplitJson spj in recentList.Skip(1))
                    {
                        recentListString += $", {spj.EAID}";
                    }
                    recentListString += "]";
                    WriteLog($"@STA_SetEASelectedLayout, Original RecentList={recentListString}");

                    //Find the index of spJson in RecentList
                    int idxRecent = recentList.FindIndex(x => x.EAID == eaId);
                    //If found in RecentList
                    if (idxRecent >= 0)
                    {
                        //Move the recentSplit to RecentList[0]
                        //If it's not at [0], then need to move
                        if (idxRecent > 0)
                        {
                            recentList.RemoveAt(idxRecent);
                            recentList.Insert(0, eaSettings.SelectedSplit.Clone());
                            WriteLog($"@STA_SetEASelectedLayout, Move inside RecentList to head from [{idxRecent}].");
                        }
                    }
                    else //Not found in RecentList, need to clone then add into RecentList
                    {
                        //If the RecentList.Count < 5, then Insert a new (clone) item to RecentList[0]
                        if (recentList.Count < EAEMConstants.MaxRecentItems)
                        {
                            //Insert new(clone) item to RecentList[0]
                            recentList.Insert(0, eaSettings.SelectedSplit.Clone());
                            WriteLog($"@STA_SetEASelectedLayout,Add new selected item to head  of RecentList.");
                        }
                        else //RecentList.Count >= 5, need to remove the latest item, then insert new (clone) item to RecentList[0]
                        {
                            //Why not using RemoveAt(recentList.Count - 1)?  
                            // 1 UI will load the first 5 items only
                            // 2 The settings file (CustomList) may be modified (unknown reason), and count > 5
                            //   we would like to remove [4] (MaxRecentItems-1)
                            recentList.RemoveAt(EAEMConstants.MaxRecentItems - 1);
                            recentList.Insert(0, eaSettings.SelectedSplit.Clone());

                            WriteLog($"@STA_SetEASelectedLayout, Remove tail item, Add new selected item to head  of RecentList.");
                        }
                    }
                    //Convert back to array
                    eaSettings.RecentList = recentList.ToArray();
                    recentListString = $"[{recentList[0].EAID}";
                    foreach (SplitJson spj in recentList.Skip(1))
                    {
                        recentListString += $", {spj.EAID}";
                    }
                    recentListString += "]";
                    WriteLog($"@STA_SetEASelectedLayout, New RecentList={recentListString}");
                }
            }

            //Save new settings (SelectedLayout and RecentList)
            bool isOKSaveSettings = _eaBroker.VM.WriteEAMonitorSettings(monitorInfo, eaSettings);
            if (!isOKSaveSettings)
            {
                WriteLog(" SetEASelectedLayout() return false: Fail to write to MonitorSettings file.");

                return false;
            }

            //Notify EABroker to update related windows/objects
            //1 Notify WorkWins to refresh WorkSplit and show fadeout animation
            //2 Notify AwsWindow to release RecentList from settings file

            bool isOKRefreshWorkWin = _eaBroker.NotifyEASelectedLayoutChanged(monitorInfo, eaSettings.SelectedSplit);

            //Notify to EASettingsChanged event handler (the end handler should be DDPM.UI)
            if (EASettingsChanged != null)
            {
                //Only below properties are used currently
                // Command: 
                // Message: "{MonitorModel}|{MonitorServiceTag}"
                string model = monitorInfo.modelName;
                string serviceTag = monitorInfo.edid.ServiceTag;
                EAArgs eaArgs = new EAArgs();
                eaArgs.Message = $"{model}|{serviceTag}";
                EASettingsChanged(this, eaArgs);
            }

            return true;
        }


        //This method will be deprecated. use SetEASelectedLayout(MonitorInfo monitorInfo, int eaId) instead.
        //
        /// <summary>
        /// Called from DDPM.SA DeviceManager, it may triggered by HotkeyManager or CLI.
        /// EAPlugin will make the specified layout (spJson) as current selected Layout,
        /// update to EAMonitorSettings (save to file), then update to EAPlugin.WorkWindows
        /// and AwsWindow. Notify DDPM.UI to reaload settings is the responsiblity of caller
        /// (DeviceManager or CLI)
        /// </summary>
        /// <param name="monitorInfo"></param>
        /// <param name="spJson"></param>
        /// <returns></returns>
        public Task<bool> SetEASelectedLayout(MonitorInfo monitorInfo, SplitJson spJson)
        {
            throw new NotImplementedException("EAPlugin.SetEASelectedLayout(MonitorInfo monitorInfo, SplitJson spJson) is deprecated.");

            //Validation
            WriteLog($"@ SetEASelectedLayout(monitor:{monitorInfo.modelName}_{monitorInfo.edid.ServiceTag}, Split:{spJson.CellCount}{spJson.SplitKey}[{spJson.CustomName}]");
            if (_deviceManagerPlugin == null)
            {
                WriteLog(" SetEASelectedLayout() return false: DeviceManager is null.");
                return Task.FromResult(false);
            }
            if (_eaBroker == null)
            {
                WriteLog(" SetEASelectedLayout() return false: EABroker is null.");
                return Task.FromResult(false);
            }
            if (!_isEaBrokerStarted)
            {
                WriteLog(" SetEASelectedLayout() return false: EABroker is not started.");
                return Task.FromResult(false);
            }

            //Validation for MonitorInfo
            if (monitorInfo == null)
            {
                WriteLog(" SetEASelectedLayout() return false: monitorInfo is null.");
                return Task.FromResult(false);
            }
            else if (String.IsNullOrWhiteSpace(monitorInfo.modelName))
            {
                WriteLog(" SetEASelectedLayout() return false: monitorInfo.modelName is null or empty.");
                return Task.FromResult(false);
            }
            else if (monitorInfo.edid == null)
            {
                WriteLog(" SetEASelectedLayout() return false: monitorInfo.edid is null.");
                return Task.FromResult(false);
            }
            else if (String.IsNullOrWhiteSpace(monitorInfo.edid.ServiceTag))
            {
                WriteLog(" SetEASelectedLayout() return false: monitorInfo.edit.ServiceTag is null or empty.");
                return Task.FromResult(false);
            }

            //Validation spJson
            // spJson.EAID: [0~49] or [1000~1004]
            // If EAID in [1000~1004]
            //    Read CustomList to check if the layout is existed
            if (spJson == null)
            {
                WriteLog(" SetEASelectedLayout() return false: spJson is null.");
                return Task.FromResult(false);
            }

            //Launch the major function in UI Thread
            Thread thread = new Thread(() =>
            {
                STA_SetEASelectedLayout(monitorInfo, spJson);
                System.Windows.Threading.Dispatcher.Run();
            });

            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();

            return Task.FromResult(true);
        }


        //New for EABroker
        /// <summary>
        /// SetEASelectedLayout() function which is running under STA thread.
        /// </summary>
        /// <param name="monitorInfo"></param>
        /// <param name="spJson"></param>
        /// <returns>false: no settings for this monitor (under default settings)</returns>
        private bool STA_SetEASelectedLayout(MonitorInfo monitorInfo, SplitJson spJson)
        {
            //if (_eaBroker == null)
            //{
            //    return STA_SetEASelectedLayout_OLD(monitorInfo, spJson);
            //}

            //Step I. Check if spJson is an existing layout (either in CustomList or WinLists)
            //
            //Read EAMonitorSettings
            EAMonitorSettings? eaSettings = _eaBroker.VM.ReadEAMonitorSettings(monitorInfo);
            if (eaSettings == null)
            {
                WriteLog(" SetEASelectedLayout() return false: ReadEAMonitorSettings return null.");
                return false;
            }
            List<SplitJson> recentList = new List<SplitJson>();
            recentList.AddRange(eaSettings.RecentList);

            _dump_SplitJsonList(recentList.ToList<SplitJson>());
            //If the spJson is a custom layout
            if (spJson.IsCustomLayout)
            {
                //Read CustomList
                SplitJson[] customArray = _deviceManagerPlugin.ReadEACustomList().Result;
                if (customArray != null)
                {
                    List<SplitJson> customList = customArray.ToList<SplitJson>();
                    //Check if it's exist in CustomList
                    SplitJson? cusSplit = customList.Find(x => x.IsEquals(spJson));
                    if (cusSplit == null)
                    {
                        WriteLog(" SetEASelectedLayout() return false: Specified layout is not found in custom list.");
                        return false;
                    }
                }
            }
            else
            {
                //Check if it's a valid WinList item
                if (!ISplitCtrl.IsExisted(spJson.CellCount, spJson.SplitKey))
                {
                    WriteLog(" SetEASelectedLayout() return false: Specified layout is not a valid predefined layout.");
                    return false;
                }
            }

            //Step II. Find the index of spJson in RecentList
            //int idxRecent = eaSettings.RecentList.FindIndex(x => x.IsEquals(spJson));
            int idxRecent = recentList.FindIndex(x => x.IsEquals(spJson));
            //If found in RecentList
            if (idxRecent >= 0)
            {
                //Step III. Move the recentSplit to RecentList[0]
                //If it's not at [0]
                if (idxRecent > 0)
                {
                    //eaSettings.RecentList.RemoveAt(idxRecent);
                    //eaSettings.RecentList.Insert(0, spJson.Clone());
                    recentList.RemoveAt(idxRecent);
                    recentList.Insert(0, spJson.Clone());
                }
            }
            else //Not found in RecentList, need to clone then add into RecentList
            {
                //Step IV.
                //If the RecentList.Count < 5, then Insert new (clone) item to RecentList[0]
                //if (eaSettings.RecentList.Count < EAEMConstants.MaxRecentItems - 1)
                if (recentList.Count < EAEMConstants.MaxRecentItems)
                {
                    //Insert to RecentList[0]
                    //eaSettings.RecentList.Insert(0, spJson.Clone());
                    recentList.Insert(0, spJson.Clone());
                }
                else //RecentList.Count >= 5-1, need to remove the latest item, then insert new (clone) item to RecentList[0]
                {
                    //eaSettings.RecentList.RemoveAt(EAEMConstants.MaxRecentItems - 2);
                    //eaSettings.RecentList.Insert(0, spJson.Clone());
                    recentList.RemoveAt(EAEMConstants.MaxRecentItems - 1);
                    recentList.Insert(0, spJson.Clone());
                }
            }
            //Step V. Save Settings
            _dump_SplitJsonList(recentList);
            //Update the selected layout
            eaSettings.SelectedSplit = spJson;
            eaSettings.RecentList = recentList.ToArray();
            //Save the settings to MonitorSettings file
            bool isOKSaveSettings = _eaBroker.VM.WriteEAMonitorSettings(monitorInfo, eaSettings);
            if (!isOKSaveSettings)
            {
                WriteLog(" SetEASelectedLayout() return false: Fail to write to MonitorSettings file.");
                return false;
            }

            //Step VI. Notify to Windows in EAPlugin
            //1 Notify WorkWins to refresh WorkSplit and show fadeout animation
            //2 Notify AwsWindow to release RecentList from settings file

            DDPM.EABroker.EAWorkWindow? workWin = _eaBroker.VM.FindWorkWindowByMonitor(monitorInfo);
            if (workWin != null)
            {
                bool isOKRefreshWorkWin = workWin.SetWorkingSplit(spJson.CellCount, spJson.SplitKey, spJson.Settings);
                WriteLog(" SetEASelectedLayout() return true but it fails to refresh settings to WorkWindow.");
            }
            else
            {
                WriteLog(" SetEASelectedLayout() return true but it fails to get WorkWindow of current Screen.");
            }

            if (_eaBroker.VM.AwsWindow != null)
            {
                _eaBroker.VM.AwsWindow.ReloadRecentList(monitorInfo.DisplayName); //Robert_Lin, 2024-10-16 fix
            }
            //_eaBroker.VM.RefreshAwsIconsFromRecentList(eaSettings.RecentList);

            //Step VII. Notify to EASettingsChanged event handler (the end handler should be DDPM.UI)
            if (EASettingsChanged != null)
            {
                //Only below properties are used currently
                // Command: 
                // Message: "{MonitorModel}|{MonitorServiceTag}"
                string model = monitorInfo.modelName;
                string serviceTag = monitorInfo.edid.ServiceTag;
                EAArgs eaArgs = new EAArgs();
                eaArgs.Message = $"{model}|{serviceTag}";
                EASettingsChanged(this, eaArgs);
            }
            return true;
        }
        #endregion Set Selected Layout


        /// <summary>
        /// For developer debug used, dump list of SplitJson to VS2022 Output console.
        /// </summary>
        /// <param name="splitJsonList"></param>
        private void _dump_SplitJsonList(List<SplitJson> splitJsonList)
        {
            int idx = 0;
            int max = 10;
            foreach (SplitJson splitJson in splitJsonList)
            {
                Trace.WriteLine($"[{idx}] {splitJson.ToString()}");
                idx++;
                if (idx > max)
                {
                    Trace.WriteLine($"  TotalCount={splitJsonList.Count} . . .");
                    break;
                }
            }
        }

 
        /// <summary>
        /// Return current Span across multiple monitor option is Enabled/Disabled;
        /// Note that it's different with EzSettings.IsSpanAcrossMultiMonitors (=ON|OFF)
        /// </summary>
        /// <returns>True=Enabled; False=Disabled</returns>
        public Task<bool> GetIsSpanEnabled()
        {
            if (_eaBroker != null &&
                _eaBroker.VM != null)
            {
                return Task.FromResult(_eaBroker.VM.IsSpanEnabled);                
            }
            return Task.FromResult(false);
        }

        /// <summary>
        /// General notification  to EAPlugin from other Plugins inside DDPM.SA.User
        /// </summary>
        /// <param name="eaArgs">
        /// eaArgs.Command=EAEMConstants.EACommand_LastSelectedMonitorChanged:
        ///     Notify EAPlugin when SelectedMonitor is changed, 
        /// </param>
        /// <returns></returns>
        public Task<bool> NotifyEAMessage(EAArgs eaArgs)
        {
            if (eaArgs.Command.Equals(EAEMConstants.EACommand_LastSelectedMonitorChanged) &&
                _eaBroker != null && 
                _isEaBrokerStarted)
            {
                _eaBroker.NotifySelectedMonitorChanged();
                return Task.FromResult(true);                
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
            if (sortApps == null || sortApps.Count == 0)
            {
                WriteLog($"@ LaunchAndArrangeAppsWithEzArrange(): sortApps is empty.");
                return Task.FromResult(false);
            }
            if (moInfo == null)
            {
                WriteLog($"@ LaunchAndArrangeAppsWithEzArrange(): monitorInfo is null.");
                return Task.FromResult(false);
            }

            string moInfoText = "";
            if (!string.IsNullOrEmpty(moInfo.modelName))
                moInfoText = moInfo.modelName;
            if (moInfo.edid != null)
            {
                if (!string.IsNullOrEmpty(moInfo.edid.ServiceTag))
                    moInfoText += $", {moInfo.edid.ServiceTag}";
                else
                    moInfoText += $", ";
            }
            else
            {
                moInfoText += $", (null)";
            }

            if (_eaBroker == null)
            {
                WriteLog($"@ LaunchAndArrangeAppsWithEzArrange(appCount={sortApps.Count}), Monitor={moInfoText}, EAID={eAid}) => EABroker is null.");
                return Task.FromResult(false);
            }

            if (!_isEaBrokerStarted)
            {
                WriteLog($"@ LaunchAndArrangeAppsWithEzArrange(appCount={sortApps.Count}), Monitor={moInfoText}, EAID={eAid}) => EABroker is not started.");
                return Task.FromResult(false);
            }
            WriteLog($"@ LaunchAndArrangeAppsWithEzArrange(appCount={sortApps.Count}), Monitor={moInfoText}, EAID={eAid}");

            //Run in a STA Thread
            Thread thread = new Thread(() =>
            {
                if (!_eaBroker.STA_LaunchAndArrangeAppsWithEzArrange(sortApps, moInfo, eAid))
                    return;

                WriteLog("STA_LaunchAndArrangeAppsWithEzArrange thread start");
                System.Windows.Threading.Dispatcher.Run();
                WriteLog("STA_LaunchAndArrangeAppsWithEzArrange thread end");
            });

            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();

            return Task.FromResult(true);

        }
        #endregion Methods

        #endregion IEasyArrangeService Implementation

        #region EA Broker

        /// <summary>
        /// Startup the EABroker. Set _isEaBrokerStarted to true, and let EABroker enter Waiting state.
        /// </summary>
        public void EABroker_Start()
        {
            //Lock until we set _isEaBrokerStarted
            lock (_eaBrokerLock)
            {
                if (!_isEaBrokerStarted)
                {
                    _isEaBrokerStarted = true;
                }
                else
                {
                    return;
                }
            }

            if (DevSettings.IsEABorkerRevoked())
            {
                WriteLog("EABroker_Start(): EABroker is revoked.");
                return;
            }

            Dmsg("EABroker Start = = = = = = = =");
            WriteLog("EABroker Start = = = = = = = =");

            Stopwatch sw = new Stopwatch();
            sw.Start();
            _eaBroker = new DDPM.EABroker.EABroker(_agent, _deviceManagerPlugin, _displayManagerPlugin, this, _settingsManagerPlugin);
            //InfoWindow, WorkWindows, AwsWindow, AwsBuddyWindow,... will be inited inside _eaBroker.Start()
            _eaBroker.Start();
            //Init EAEditWindow and SaveCustomWindow below
            //InitEditWindow();
            sw.Stop();
            WriteLog($"EABroker Init Windows duration=[{sw.ElapsedMilliseconds} msec]");

            if (_displayManagerPlugin != null)
            {
                _displayManagerPlugin.Displaychanged += _displayManagerPlugin_Displaychanged;

            }

            //EzMemLauncher.DeviceManagerSA = _deviceManagerPlugin;
            //EzMemLauncher.Log = _log;

            //Microsoft.Win32.SystemEvents.DisplaySettingsChanged += SystemEvents_DisplaySettingsChanged;
            _agent.RegisterForEvent(AgentEventNames.DisplaySettingsChanged, DisplaySettingsChangedHandler);

            //Robert_Lin, 2025-4-16 remove the comment-out by Derek.
            //QT and myself found sometime the EA has no function after startup.
            //It happend when EABroker is started later than DisplayManager has invoke DisplaySettingsChanged and AllMonitorInfoChanged.
            //
            //Derek0403 why raise DisplaySettingsChanged event here?
            //Remove???????
            EventManagerArgs evtArgs = new EventManagerArgs() { Tag = "init" };
            _agent.RaiseEvent(AgentEventNames.DisplaySettingsChanged, this, evtArgs);

            //Robert_Lin, 2024-12-10
            _agent.RegisterForEvent(AgentEventNames.AllInfoMonitorsChanged, AllInfoMonitorChangedHandler);
        }

        /// <summary>
        /// Exiting from EABroker_Start().
        /// </summary>
        public void EABroker_Stop()
        {
            //Robert_Lin, 2024-12-20 no need a STA thread to do something any more, comment-out
            //Thread thread = new Thread(() =>
            //{
            //    Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss.fff") + " " + "[EAPlugin] EABroker_Stop.");

            //    //WinEventHook_Stop();
            //    //_vmArrange.ClearWorkWindows();
            //    //_vmArrange.ResetWorkWindows2();

            //    System.Windows.Threading.Dispatcher.Run();
            //});

            //thread.SetApartmentState(ApartmentState.STA);
            //thread.Start();

            if (_displayManagerPlugin != null)
                _displayManagerPlugin.Displaychanged -= _displayManagerPlugin_Displaychanged;

            _agent.UnregisterForEvent(AgentEventNames.DisplaySettingsChanged, DisplaySettingsChangedHandler);
            if (_eaBroker != null)
            {
                _eaBroker.Stop();
            }
            Dmsg(" = = = = = = = = = =  EABroker Stop");
            WriteLog("EABroker_Stop");
        }

        //It should be call once DeviceManagerSA & DisplayManager are loaded
        private void ConfigureServices()
        {
            if (_isConfigured)
                return;
            _isConfigured = true;

            ServiceCollection services = new ServiceCollection();
            if (_log != null)
                services.AddSingleton(_log);
            if (_deviceManagerPlugin != null)
                services.AddSingleton(_deviceManagerPlugin);
            if (_displayManagerPlugin != null)
                services.AddSingleton(_displayManagerPlugin);

            PluginIoc.ConfigureServices(services.BuildServiceProvider());
        }

         #endregion EA Broker

        #region Display Changed event

        private void _displayManagerPlugin_Displaychanged(object? sender, DisplaychangedEventArgs e)
        {
            WriteLog($"@ OnDisplaychanged, ChangedCount={e.count}");
            //Invoke_RefreshWorkWindows();
        }

        private void DisplaySettingsChangedHandler(object sender, EventManagerArgs e)
        {
            _log?.Info($"@[EAPlugin] DisplaySettingsChangedHandler");

            if (_eaBroker != null)
            {
                bool isInit = false;

                //if (e.Tag != null &&
                //    e.Tag is string &&
                //    e.Tag == "init")
                if (e.Tag is not null and string and "init")
                {
                    isInit = true;
                }

                //If we are in Edit state, then cancel the editing
                if (_eaBroker.RunningState == eEARunningStates.Edit)
                {
                    //Fore to cancel the Edit Procedure
                    
                }

                _log?.Info($"@[EAPlugin] isInit={isInit} before call _eaBroker.Handle_DisplaySettingsChanged");
                _eaBroker.Handle_DisplaySettingsChanged(isInit);
                //Move blew statement into Handle_DisplaySettingsChanged()
                //_eaBroker.VM.RefreshWorkWindows();
                return;
            }
            else
            {
                WriteLog("@ DisplaySettingsChangedHandler(), _eaBroker is null.");
            }
        }

        //Robert_Lin, 2024-12-10 added 
        /// <summary>
        /// The handler of IAgent event: AllInfoMonitorsChanged. 
        /// The event is signaled from DisplayManagerPlugin.NotifyEAPluginAllInfoMonitorsChanged() 
        /// when DisplayManager has collected all monitors.
        /// EABroker need to detect SpanScreen when all monitors are rebuild.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AllInfoMonitorChangedHandler(object sender, EventManagerArgs e)
        {
            if (_eaBroker != null)
            {
                _eaBroker.Handle_AllInfoMonitorChanged();
            }
        }
        #endregion Display Changed event



        #region EditWindow and SaveCustomWindow

        //Derek 2025/03/31 due to 0 references
        //private void saveCustomWidow_CancelButtonClick(object? sender, string e)
        //{
        //    if ((EditReturn != null) && (_eaArgs != null))
        //    {
        //        EAArgs retArgs = new EAArgs(_eaArgs);
        //        retArgs.Result = false;
        //        retArgs.Command = "EditReturn";
        //        retArgs.Message = "User cancel the editing.";
        //        EditReturn(this, retArgs);
        //    }
        //    if (_editWindow != null)
        //    {
        //        _editWindow.Dispatcher_Hide();
        //        _editWindow = null;
        //    }
        //    if (_eaBroker != null)
        //        _eaBroker.VM.IsWorkUIEnabled = true;
        //}

        //Derek 2025/03/31 due to 0 references
        //private void saveCustomWidow_SaveButtonClick(object? sender, string e)
        //{
        //    if ((EditReturn != null) && (_eaArgs != null))
        //    {
        //        if (_eaArgs.SplitJson.IsOverlapLayout)
        //        {
        //            EditForOverlapLayout();
        //            return;
        //        }

        //        EAArgs retArgs = new EAArgs(_eaArgs);
        //        retArgs.Command = "EditReturn";
        //        retArgs.Result = true;
        //        retArgs.SplitJson.Settings = _editWindow.GetSettings();

        //        if (_saveCustomWindow != null &&
        //            _saveCustomWindow.SelectedCustomItem != null)
        //        { 
        //            //CustomName will copy from SaveCustomWindow
        //            retArgs.SplitJson.CustomName = _saveCustomWindow.SelectedCustomItem.CustomName;

        //            //If user has selected an existed custom layout
        //            if (_saveCustomWindow.SelectedCustomItem.EAID >= EAEMConstants.EAID_FirstCustom)
        //            {
        //                retArgs.SplitJson.EAID = _saveCustomWindow.SelectedCustomItem.EAID;

        //            }
        //            else
        //            {
        //                //The SplitClass will update from SaveCustomWindow

        //                //retArgs.SplitJson = _saveCustomWindow.SelectedCustomItem.Clone();
        //            }                    
        //        }

        //        //Can be removed
        //        //retArgs.CustomName = e;
        //        //retArgs.Settings = _editWindow.GetSettings();

        //        EditReturn(this, retArgs);
        //    }
        //    if (_editWindow != null)
        //    {
        //        _editWindow.Dispatcher_Hide();
        //        _editWindow = null;
        //    }

        //    if (_eaBroker != null)
        //        _eaBroker.VM.IsWorkUIEnabled = true;
        //}

        private void EditForOverlapLayout()
        {
            Screen workScreen = _saveCustomWindow.WorkScreen;
            Rectangle workingArea = _saveCustomWindow.WorkingArea;
            Screen scr = _saveCustomWindow.WorkScreen;


            //4 When user click "Save" from SaveCustomWindow
            //5 Show the EditWindow to capture Windows and frame them
            _editWindow = new EABroker.EAEditWindow(_log);
            _editWindow.EditReturn += _editWindow_EditReturn;
            _editWindow.ShowAndEdit(_eaArgs, workingArea);
            //6 Delay for 3 sec
        }

        private void _editWindow_EditReturn(object? sender, EAArgs e)
        {
            if (_eaArgs == null)
                return;
            if (_editWindow == null)
                return;

            //7 EditWindow sent EditReturn event, and goto here

            _editWindow.EditReturn -= _editWindow_EditReturn;

            //8 get the returned layout data from EditWindow
            EAArgs retArgs = new EAArgs(_eaArgs);
            retArgs.Command = "EditReturn";
            retArgs.Result = true;
            retArgs.SplitJson.Settings = _editWindow.GetSettings();

            //9 EditWindow can be closed (Hide) now
            _editWindow.Dispatcher_Hide();
            _editWindow = null;

            //10 Get the returned CustomName and Selected CustomItem from SaveCustomWindow
            if (_saveCustomWindow != null &&
                _saveCustomWindow.SelectedCustomItem != null)
            {
                //CustomName will copy from SaveCustomWindow
                retArgs.SplitJson.CustomName = _saveCustomWindow.SelectedCustomItem.CustomName;


                //If user has selected an existed custom layout
                if (_saveCustomWindow.SelectedCustomItem.EAID >= EAEMConstants.EAID_FirstCustom)
                {
                    retArgs.SplitJson.EAID = _saveCustomWindow.SelectedCustomItem.EAID;

                }
                else
                {
                    //The SplitClass will update from SaveCustomWindow

                    //retArgs.SplitJson = _saveCustomWindow.SelectedCustomItem.Clone();
                }                
            }

            //11 Notify UI to get the updates
            if (EditReturn != null)
                Task.Run(() => EditReturn.Invoke(this, retArgs));

            //EditReturn(this, retArgs);

            //12 WorkWindow can be resumed 
            if (_eaBroker != null)
                _eaBroker.VM.IsWorkUIEnabled = true;
        }

        private void SendEditReturn_Cancel(string message)
        {
            //UI will look at the Result and log Message only, so we don't need to copy from
            // input EAArgs (from EditCommand)
            EAArgs retArgs = new EAArgs()
            {
                Command = "EditReturn",
                Result = false,
                Message = message
            };

            if (EditReturn != null)
            {
                EditReturn(this, retArgs);
            }
        }
        #endregion EditWindow and SaveCustomWindow

        #region Debug Msg

        public static void Dmsg(string msg)
        {
            Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss.fff") + " " + msg);
        }
        public void WriteLog(string msg, Exception? e = null)
        {
            if (_log != null)
            {
                if (e == null)
                {
                    _log.Info(msg);
                }
                else
                {
                    _log.Error(e, msg);
                }
            }
        }
        #endregion Debug Msg

        #region Telemetry
        public void SendEasyArrangeLayoutTelemetry(string eventValue, MonitorInfo? mi = null, Telementry_Frequency frequency = Telementry_Frequency.RealTime)
        {
            if (_telementrySchedulerPlugin == null)
            {
                //Robert_Lin, 2025-1-7, add log to note
                WriteLog("@SendEasyArrangeLayoutTelemetry(), not send data due to TelemetryPlugin is null.");
                return;
            }

            if (_telementrySchedulerPluginUsable)
            {
                EasyArrangeLayoutTelemetry easyArrangeLayoutTelemetry = new EasyArrangeLayoutTelemetry();
                easyArrangeLayoutTelemetry.EasyArrangeLayout = eventValue;
                easyArrangeLayoutTelemetry.CommunicationPath = "Video";
                easyArrangeLayoutTelemetry.GraphicCardName = string.Empty;
                easyArrangeLayoutTelemetry.MonitorName = (mi == null) ? "" : mi.AliasDeviceName;
                easyArrangeLayoutTelemetry.D_Ctrl = (mi == null) ? "" : mi.D_Ctrl;
                easyArrangeLayoutTelemetry.SupplierID = (mi == null) ? "" : mi.SupplierID;
                easyArrangeLayoutTelemetry.FirmwareVersion = (mi == null) ? "" : mi.FwVersion;
                easyArrangeLayoutTelemetry.DisplayModelname = (mi == null) ? "" : mi.modelName;
                easyArrangeLayoutTelemetry.DisplayServiceTag = (mi == null) ? "" : mi.edid.ServiceTag;
                easyArrangeLayoutTelemetry.DsiplayResolution = string.Empty;
                easyArrangeLayoutTelemetry.MaxDisplayResolution = string.Empty;
                _telementrySchedulerPlugin.ReceiveTelemetryInfo("DisplayFeatures", easyArrangeLayoutTelemetry.ToJson(), frequency);
            }
        }
        #endregion Telemetry

        #region Hotkey

        //Robert_Lin, 2024-11-20 for the Shift key option 
        //To detecte if [Shift] is down in real-time
        //1 GetAsycKeyStart(ShiftKey) will return the previous state not current.
        //2 In Keyboard_KeyDownProc() when you pressing LShift (or RShift)
        //  GetAsycKeyStart(ShiftKey) will return False (Up)
        //3 Keyboard_KeyDownProc() will be continute called if you hold down the Shift key
        //  GetAsycKeyStart(ShiftKey) will return True (Down)
        //4 When you release Shift key, Keyboard_KeyUpProc() will be called once
        //  GetAsycKeyStart(ShiftKey) will return True (Down)
        //  => Problem: this event is triggered when Shift key is released
        //So chnage the logic to check the event Down-Up and record which key are Up/Down
        //  _isLShiftDown is true when [LShift] is pressing down, and false when it's up
        //  _isRShiftDown is the same for [RShift]
        //  To check if any [Shift] is down:  (_isLShiftDown || _isRShiftDown)

        private bool _isLShiftDown = false;
        private bool _isRShiftDown = false;
        private void Keyboard_KeyUpProc(object? sender, KeyEventArgs e)
        {
            string strKey = e.KeyCode.ToString().ToUpper();
           // Debug.WriteLine($"Keyboard_KeyUpProc ---{strKey}");
            //bool _altPressed = _HotkeyPlugin.IsKeyPushedDown(System.Windows.Forms.Keys.Menu);
            //bool _ctrlPressed = _HotkeyPlugin.IsKeyPushedDown(System.Windows.Forms.Keys.ControlKey);
            //bool _shiftPressed = _HotkeyPlugin.IsKeyPushedDown(System.Windows.Forms.Keys.ShiftKey);

            if (e.KeyCode == Keys.LShiftKey)
                _isLShiftDown = false;
            else if (e.KeyCode == Keys.RShiftKey)
                _isRShiftDown = false;

            if (_eaBroker != null)
            {
                bool isShiftDown = (_isLShiftDown || _isRShiftDown);
                _eaBroker.VM.IsShiftPressed = isShiftDown;
                Debug.WriteLine($"KeyDown({strKey})--LShift({_isLShiftDown}), RShift({_isRShiftDown}) => IsShiftDown{isShiftDown}");
            }
        }

        private void Keyboard_KeyDownProc(object? sender, KeyEventArgs e)
        {
            string strKey = e.KeyCode.ToString().ToUpper();
           //// Debug.WriteLine($"Keyboard_KeyUpProc ---{strKey}");
           // bool _altPressed = _HotkeyPlugin.IsKeyPushedDown(System.Windows.Forms.Keys.Menu);
           // bool _ctrlPressed = _HotkeyPlugin.IsKeyPushedDown(System.Windows.Forms.Keys.ControlKey);
           // bool _shiftPressed = _HotkeyPlugin.IsKeyPushedDown(System.Windows.Forms.Keys.ShiftKey);

            if (e.KeyCode == Keys.LShiftKey)
                _isLShiftDown = true;
            else if (e.KeyCode == Keys.RShiftKey)
                _isRShiftDown = true;

            if (_eaBroker != null)
            {
                bool isShiftDown = (_isLShiftDown || _isRShiftDown);
                //Debug.WriteLine($"Keyboard_KeyDownProc --- Shift is down? {isShiftDown}");
                _eaBroker.VM.IsShiftPressed = isShiftDown;
                Debug.WriteLine($"KeyDown({strKey})--LShift({_isLShiftDown}), RShift({_isRShiftDown}) => IsShiftDown{isShiftDown}");
            }

        }
        #endregion Hotkey
    }
}