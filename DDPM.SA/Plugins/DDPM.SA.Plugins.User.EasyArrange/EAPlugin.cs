//#define REMOVE_EA
//Define this flag will remove EA functions

using CommunityToolkit.Mvvm.DependencyInjection;
using DDPM.Easy.Common;
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
using nsWinEventHook;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Windows;
using System.Windows.Controls.Ribbon;
using System.Windows.Forms;
using System.Windows.Media.Animation;
using VcpCore.Common;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using IDs = DDPM.SA.Common.IDs;

namespace DDPM.SA.Plugins.User.EasyArrange
{
    [Plugin(IDs.DDPM_EAPlugin_PLUGIN_ID, pluginName, PluginOrderGroupType.Core, Version = pluginVersion)]
    [Descriptor(Description = pluginDescription)]
    [Publisher(Name = publisherCompany, Website = publisherWebsite, Support = publisherSupport)]
    [PublishedUnelevatedInterface(new[] { typeof(IEasyArrangeService) })]
    //[PublishedInterface(new[] { typeof(IPipPbpService) })]
    [DependencyKnownTypes(new[] { typeof(IDisplayService) })]
    [PluginRequires(Id = IDs.Display_Manager_PLUGIN_ID, Version = "1.0.0", AllowDynamicResolving = true)]
    public class EAPlugin : BaseAgentPlugin, IDisposableObservable, IEasyArrangeService
    {
        #region Private Members

        //Plugin strings
        private const string pluginName = "EAPlugin";

        private const string pluginVersion = "1.0.0";
        private const string pluginDescription = "This plugin implements EasyArrange Plugin functions.";
        private const string publisherCompany = "Wistron";
        private const string publisherWebsite = "https://www.wistron.com";
        private const string publisherSupport = "This plugin implements EasyArrange Plugin functions.";

        private bool _IsAdministrator = ProcessSecurityHelperWrapper.IsCurrentProcessRunningElevated();
        private IAgent _agent;
        private const string PluginLogId = "EAPlugin";

        //private Logs _logs;
        private ILog? _log;

        //IDisplayService Plugin - Robert_Lin 2024-7-5, to be replaced with DeviceManagerSA
        private IDisplayService _displayManagerPlugin;

        private PluginCondition _displayManagerPluginCondition;
        private bool _displayManagerPluginUsable = false;

        //IDeviceManagerSA Plugin - Robert_Lin 2024-7-5 added
        private IDeviceManagerSA _deviceManagerPlugin;

        private PluginCondition _deviceManagerPluginCondition;
        private bool _deviceManagerPluginUsable = false;

        private readonly object _PluginConditionLock = new object();

        public static readonly Ioc PluginIoc = new();
        private bool _isConfigured = false;

        #endregion Private Members

        #region Constructor

        public EAPlugin(IAgent agent) : base(agent, PluginLogId)
        {
            _agent = agent;
            _IsAdministrator = ProcessSecurityHelperWrapper.IsCurrentProcessRunningElevated();
            _log = Log;
            _vmArrange.Log = Log;
            _log?.Info($"[{pluginName}] is constructed.");
        }

        #endregion Constructor

        private bool isDebug20241008()
        {
            string iniFile = @"C:\temp\DDPMDebug.txt";
            if (System.IO.File.Exists(iniFile))
            {
                return (Win32Lib.Win32.IniReadInt("DDPMDebug", "DDPM.SA.EAPlugin.Debug.20241008", 0, iniFile) == 1);
            }
            return false;
        }

        #region Log/Debug messages

        private void LogInfo(string msg)
        {
            _log?.Info(msg);
        }

        private void LogException(Exception ex, string msg)
        {
            _log?.Error(ex, msg);
        }

        private void ConsoleWriteLine(string msg)
        {
            Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss.fff") + " " + "[EAPlugin] " + msg);
        }

        #endregion Log/Debug messages

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

#if !REMOVE_EA
            //Monitoring plugins state
            //2024-8-13 Robert_Lin, EAPlugin has fixed .NET 8 issues, so uncomment below statements.
            // 2024-08-06 Elie, Mask InitializeDeviceManagerPlugin() function to skip .NET 8 for more than two monitor cause exception issue. ==> System.IO.IOException: 'Cannot locate resource 'eaworkwindow.baml'.'
            if (isDebug20241008()) 
            {
                InitializeDeviceManagerPlugin();
                InitializeDisplayManagerPlugin();
            }
#endif
        }

#endregion Overriding methods

        #region PluginManager related

        private void PluginManagerOnPluginsStarted(object sender, PluginsStartedEventArgs e)
        {
            if (e == null)
                return;
            if (e.ChangedPlugins == null)
                return;
            if (e.ChangedPlugins.Any() == false)
                return;

            _log?.Info($"@ PluginManagerOnPluginsStarted, ChangedPlugins.Count={e.ChangedPlugins.Count}");
            //foreach(IFrameworkPlugin plugin in e.ChangedPlugins)
            //{
            //    _log?.Info($"      Name=[{plugin.} @PluginManagerOnPluginsStarted, ChangedPlugins.Count={e.ChangedPlugins.Count}");
            //}
        }

        // DeviceManager Plugin
        //
        private void InitializeDeviceManagerPlugin()
        {
            //If DeviceManager plugin is got already then return, prevent to call twice
            if (_deviceManagerPlugin != null)
                return;

            _deviceManagerPlugin = _agent.PluginManager.FindPluginByType<IDeviceManagerSA>(PluginResolution.Dynamic);

            if (_deviceManagerPlugin is IFrameworkPluginConditionNotification DeviceManagerCondition)
            {
                DeviceManagerCondition.PluginConditionChangeHandler += OnDeviceManagerPluginConditionChangeHandler;
                GetCurrentDeviceManagerPluginCondition();
            }
            _log?.Info($"Initializing DeviceManager plugin.");
        }

        private void OnDeviceManagerPluginConditionChangeHandler(object sender, EventArgs e)
        {
            GetCurrentDeviceManagerPluginCondition();
        }

        private void GetCurrentDeviceManagerPluginCondition()
        {
            _ = Task.Run(async () =>
            {
                var pluginCondition = await (_deviceManagerPlugin as IFrameworkPluginConditionNotification)?.CurrentConditionAsync();

                lock (_PluginConditionLock)
                {
                    if (pluginCondition is PluginErrorCondition)
                    {
                        _log?.Info($"DeviceManager plugin is in an error condition");
                        _deviceManagerPluginCondition = pluginCondition;
                        _deviceManagerPluginUsable = false;
                    }
                    else if (pluginCondition is PluginStartedCondition)
                    {
                        _log?.Info($"DeviceManager plugin is in a started condition");
                        _deviceManagerPluginCondition = pluginCondition;
                        _deviceManagerPluginUsable = true;
                        _vmArrange.DeviceManager = _deviceManagerPlugin;
                        if (CheckIfReadyToStartEABorker())
                        {
                            ConfigureServices();
                            //Only after DisplayManager is ready to use, will start the EasyArrange service
                            EABroker_Start();
                        }
                    }
                    else
                    {
                        _log?.Info($"DeviceManager plugin is in others condition");
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
                        _log?.Info($"{nameof(GetCurrentDisplayManagerPluginCondition)} - Display ManagerPlugin is in an error condition");
                        _displayManagerPluginCondition = pluginCondition;
                        _displayManagerPluginUsable = false;
                    }
                    else if (pluginCondition is PluginStartedCondition)
                    {
                        _log?.Info($"{nameof(GetCurrentDisplayManagerPluginCondition)} -Display ManagerPlugin is in a started condition");
                        _displayManagerPluginCondition = pluginCondition;
                        _displayManagerPluginUsable = true;
                        _vmArrange.DisplayManager = _displayManagerPlugin;
                        if (CheckIfReadyToStartEABorker())
                        {
                            ConfigureServices();
                            //Only after DisplayManager is ready to use, will start the EasyArrange service
                            EABroker_Start();
                        }
                    }
                }
            });
        }

        private void OnDisplayManagerPluginConditionChangeHandler(object sender, EventArgs e)
        {
            GetCurrentDisplayManagerPluginCondition();
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
                if (!_displayManagerPluginUsable || !_deviceManagerPluginUsable)
                {
                    //Either DisplayManager or DeviceManager is not ready
                    _log?.Info("@ CheckIfReadyToStartEABorker: DisplayManager or DeviceManager not ready.");
                    return false;
                }
                //If EABroker is already started
                if (_isEaBrokerStarted)
                {
                    //EABroker is started already
                    _log?.Info("@ CheckIfReadyToStartEABorker: EABroker is started already.");
                    return false;
                }

                //Robert_Lin 2024-0910, comment out below statements
                //Let EABRoker start event if there is no Monitor connected
                //We will refresh when DisplaySettingsChanged event
                /*
                List<MonitorInfo>? monitors = GetMonitors();
                if (monitors == null)
                {
                    _log?.Info("@ CheckIfReadyToStartEABorker: Monitors is null.");
                    return false;
                }
                if (!monitors.Any())
                {
                    _log?.Info("@ CheckIfReadyToStartEABorker: Monitors is empty.");
                    return false;
                }

                _log?.Info($"@ CheckIfReadyToStartEABorker: Monitor count={monitors.Count}");
                */
                return true;
            }
        }
        #endregion PluginManager related

        #region IEasyArrangeService Implementation
        #region CLI Flags: Enabled/Locked
        public bool IsFunctionEnabled
        {
            get
            {
#if REMOVE_EA
                return false;
#else
                return _vmArrange.IsFunctionEnabled;
#endif
            }
            set => _vmArrange.IsFunctionEnabled = value;
        }

        #endregion CLI Flags: Enabled/Locked

        #region Events
        /// <summary>
        /// Notify to DDPM.UI (EasyArrangeModule) that the EditCommand request has been accepted.
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
        /// Robert_Lin, 2024-10-6: This method may be deprecated after confirm that can be replaced with SetEASelectedLayout()
        /// </summary>
        /// <param name="monitorInfo"></param>
        /// <param name="cellCount"></param>
        /// <param name="splitKey"></param>
        /// <param name="settings"></param>
        /// <returns></returns>
        public Task<bool> SetEAWrokSplit(MonitorInfo monitorInfo, int cellCount, char splitKey, List<double>? settings = null)
        {
            EAWorkWindow? workWin = _vmArrange.FindWorkWindowByDisplayName2(monitorInfo.DisplayName);
            if (workWin == null)
            {
                return Task.FromResult(false);
            }

            bool res = workWin.SetWorkingSplit(cellCount, splitKey, settings);
            return Task.FromResult(res);
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
            Trace.WriteLine($"  * EAArgs.Split=[{args.CellCount}{args.SplitKey}], CustomName=[{args.CustomName}]");

            //If InitEditWindow() not been called or failed.
            //if (_editWindow == null)
            //{
            //    return Task.FromResult(false);
            //}
            //Check if the monitorInfo contains the monitor

            //Launch the major function in UI Thread
            Thread thread = new Thread(() =>
            {
                STA_EditCommand(monitorInfo, args);
                //EAEditWindow editWindow = new EAEditWindow();
                //if (!editWindow.SetInputArg(args, false))
                //    editWindow.Show();

                //SaveCustomWindow saveCustomWindow = new SaveCustomWindow();
                //saveCustomWindow.Show();

                System.Windows.Threading.Dispatcher.Run();
            });

            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();

            return Task.FromResult(true);
        }

        /// <summary>
        /// EditCommand() function which is running under STA thread.
        /// </summary>
        /// <param name="monitorInfo"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        private bool STA_EditCommand(MonitorInfo monitorInfo, EAArgs args)
        {
            Trace.WriteLine("@ UI_EditCommand()");
            Trace.WriteLine($"  * Monitor.Model=[{monitorInfo.modelName}], ServiceTag=[{monitorInfo.edid.ServiceTag}]");
            Trace.WriteLine($"  * EAArgs.Split=[{args.CellCount}{args.SplitKey}], CustomName=[{args.CustomName}]");

            //Get the DisplayName from MonitorInfo
            string displayName = monitorInfo.DisplayName;
            //Get the target Screen from the displayName
            Screen? scr = Screen.AllScreens.FirstOrDefault(x => x.DeviceName.Equals(displayName, StringComparison.OrdinalIgnoreCase));
            //If no matched screen found, then report error
            if (scr == null)
            {
                if (EditStarted != null)
                {
                    EditStarted(this, $"Canot find a Screen from monitorInfo. MonitorInfo.DisplayName=[{monitorInfo.DisplayName}]");
                }
                return false;
            }
            bool isVertical = (scr.Bounds.Width < scr.Bounds.Height);

            //Find the WorkWindow of the target screen
            //EAWorkWindow? workWin = FindWorkWindowByMonitorInfo(monitorInfo);
            //if (workWin == null)
            //{
            //    EAArgs retArgs = new EAArgs(args);
            //    retArgs.Result = false;
            //    retArgs.Command = "EditError";
            //    retArgs.Message = "Canot find a Screen from monitorInfo.";
            //    if (EditReturn != null)
            //    {
            //        EditReturn(this, retArgs);
            //    }
            //    return false;
            //}

            //Stop WorkWindow fade out animation

            //Get EditWindow
            //

            if (_editWindow == null)
            {
                InitEditWindow();
            }
            if (_saveCustomWindow == null)
            {
                InitSaveCustomWindow();
            }

            //[Standalone solution]
            EAEditWindow editWin = _editWindow;

            //[InfoWin solution]
            //EAEditWindow editWin = new EAEditWindow();

            if (editWin == null)
            {
                if (EditStarted != null)
                {
                    EditStarted(this, "create EditWindow error");
                }
                return false;
            }


            _eaArgs = args;

            //Assign the handler of EditReturn event from editWin
            editWin.EditReturn += (object? sender, EAArgs args) =>
            {
                _vmArrange.IsWorkUIEnabled = true;
                _editWindow?.Hide();

                if (EditReturn != null)
                {
                    EditReturn(this, args);
                }
            };


            //if (!editWin.SetInputArg(args, scr))
            if (!editWin.ShowAndEdit(args, scr))
            {
                if (EditStarted != null)
                {
                    EditStarted(this, editWin.LastError);
                }
                return false;
            }

            //[Standalone solution]
            //
            if (_saveCustomWindow != null)
            {
                _saveCustomWindow.SetInputArg(args, scr);
            }

            //
            //////////////////////

            //Signal EditStart event
            if (EditStarted != null)
                EditStarted(this, "");

            _vmArrange.IsWorkUIEnabled = false;

            return true;
        }

        /// <summary>
        /// Called by DeviceManagerSA when UI call WriteEzSettings_XXXXXX() method to update EzSettings.
        /// In EAPluging, will reload EzSettings from DDPMSettings file, and update to ArrangeViewModel.
        /// </summary>
        /// <returns></returns>
        public Task<bool> ReloadEzSettings()
        {
            //Reload settings
            if ((_deviceManagerPlugin != null) && (_vmArrange != null))
            {
                _vmArrange.EzSettings = _deviceManagerPlugin.ReadEzSettings().Result;
                _vmArrange.LogInfo($"@ EAPlugin.ReloadEzSettings(), Refresh values: IsWidthoutGap={_vmArrange.EzSettings.IsWidthoutGap}, IsOnlyShift={_vmArrange.EzSettings.IsOnlyAllowWhenShiftKeyPressed}, IsSpan ={_vmArrange.EzSettings.IsSpanAcrossMultiMonitors}, IsAwsEnabled ={_vmArrange.EzSettings.IsAwsEnabled}");
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }

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
            //Validation
            LogInfo($"@ SetEASelectedLayout(monitor:{monitorInfo.modelName}_{monitorInfo.edid.ServiceTag}, Split:{spJson.CellCount}{spJson.SplitKey}[{spJson.CustomName}]");
            if (_deviceManagerPlugin == null)
            {
                LogInfo(" SetEASelectedLayout() return false: DeviceManager is null.");
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

        /// <summary>
        /// SetEASelectedLayout() function which is running under STA thread.
        /// </summary>
        /// <param name="monitorInfo"></param>
        /// <param name="spJson"></param>
        /// <returns></returns>
        private bool STA_SetEASelectedLayout(MonitorInfo monitorInfo, SplitJson spJson)
        {
            //Step I. Check if spJson is an existing layout (either in CustomList or WinLists)
            //
            //Read EAMonitorSettings
            EAMonitorSettings? eaSettings = _vmArrange.ReadEAMonitorSettings(monitorInfo);
            if (eaSettings == null)
            {
                LogInfo(" SetEASelectedLayout() return false: ReadEAMonitorSettings return null.");
                return false;
            }
            List<SplitJson> recentList = new List<SplitJson>();
            recentList.AddRange(eaSettings.RecentList);

            _dump_SplitJsonList(recentList.ToList<SplitJson>());
            //If the spJson is a custom layout
            if (spJson.CustomId != 0)
            {
                //Check if it's exist in CustomList
                SplitJson? cusSplit = eaSettings.CustomList.Find(x => x.IsEquals(spJson));
                if (cusSplit == null)
                {
                    LogInfo(" SetEASelectedLayout() return false: Specified layout is not found in custom list.");
                    return false;
                }
            }
            else
            {
                //Check if it's a valid WinList item
                if (!ISplitCtrl.IsExisted(spJson.CellCount, spJson.SplitKey))
                {
                    LogInfo(" SetEASelectedLayout() return false: Specified layout is not a valid predefined layout.");
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
                    recentList.RemoveAt(EAEMConstants.MaxRecentItems - 2);
                    recentList.Insert(0, spJson.Clone());
                }
            }
            //Step V. Save Settings
            _dump_SplitJsonList(recentList);
            //Update the selected layout
            eaSettings.SelectedSplit = spJson;
            eaSettings.RecentList = recentList.ToArray();
            //Save the settings to MonitorSettings file
            bool isOKSaveSettings = _vmArrange.WriteEAMonitorSettings(monitorInfo, eaSettings);
            if (!isOKSaveSettings)
            {
                LogInfo(" SetEASelectedLayout() return false: Fail to write to MonitorSettings file.");
                return false;
            }

            //Step VI. Notify to Windows in EAPlugin
            //1 Notify WorkWins to refresh WorkSplit and show fadeout animation
            //2 Notify AwsWindow to release RecentList from settings file

            EAWorkWindow? workWin = _vmArrange.FindWorkWindowByDisplayName2(monitorInfo.DisplayName);
            if (workWin != null)
            {
                bool isOKRefreshWorkWin = workWin.SetWorkingSplit(spJson.CellCount, spJson.SplitKey, spJson.Settings);
                LogInfo(" SetEASelectedLayout() return true but it fails to refresh settings to WorkWindow.");
            }
            else
            {
                LogInfo(" SetEASelectedLayout() return true but it fails to get WorkWindow of curent Screen.");
            }

            _vmArrange.RefreshAwsWindowIcons();

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
        #endregion Methods

        #endregion IEasyArrangeService Implementation

        #region EA Broker

        /// <summary>
        /// A <Display.DeviceName, WorkWindow> dictionary, each display will allcate a WorkWindow to serve it.
        /// where Display.DeviceName example: "\\.\DISPLAY1"
        /// </summary>
        //private Dictionary<string, EAWorkWindow> _workWindows = new Dictionary<string, EAWorkWindow>();

        private readonly object _eaBrokerLock = new object();
        private bool _isEaBrokerStarted = false;
        private ArrangeVM _vmArrange = new ArrangeVM();
        private EAEditWindow? _editWindow = null;
        private SaveCustomWindow? _saveCustomWindow = null;
        private EAArgs _eaArgs;

         public void EABroker_Start()
        {
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
                ConsoleWriteLine("EABroker Start = = = = = = = =");
                LogInfo("EABroker Start = = = = = = = =");
                _vmArrange.DisplayManager = _displayManagerPlugin;


                Stopwatch sw = new Stopwatch();
                sw.Start();
                //new windows in separate STD threads 
                //InitEditWindow();
                //InitSaveCustomWindow();
                //InitInfoWindow();
                //InitWorkWindows();
                //InitAwsWindow();

                //new windows in the same STD thread
                InitAllWindows();

                //Debug purpose
                //Debug_New3Windows();

                sw.Stop();
                LogInfo($"EABroker Init Windows duration=[{sw.ElapsedMilliseconds} msec]");

                ReloadEzSettings();
                if (_displayManagerPlugin != null)
                {
                    _displayManagerPlugin.Displaychanged += _displayManagerPlugin_Displaychanged;
                   
                }

                //Robert_Lin: Debug - Need to remove in release build
                if (isDebug20241008())
                {
                    _vmArrange.EzSettings = new EzSettings()
                    {
                        //IsOnlyAllowWhenShiftKeyPressed = true,
                        IsAwsEnabled = true
                    };
                }

                //Robert's Debug, need to remove in release build
                //List<MonitorInfo> monitors = _deviceManagerPlugin.GetMonitors().Result;
                //foreach (MonitorInfo monitor in monitors)
                //{
                //    SplitJson spJson = new SplitJson()
                //    {
                //        CellCount = 4,
                //        SplitKey = 'A',
                //        CustomId = 0
                //    };
                //    SetEASelectedLayout(monitor, spJson);
                //    break;
                //}



                //Microsoft.Win32.SystemEvents.DisplaySettingsChanged += SystemEvents_DisplaySettingsChanged;
                _agent.RegisterForEvent(AgentEventNames.DisplaySettingsChanged, DisplaySettingsChangedHandler);
                _agent.RaiseEvent(AgentEventNames.DisplaySettingsChanged, this, new EventManagerArgs());
                ConsoleWriteLine(" = = = = = = = = = =   EABroker Exit");
            }
        }

        private void Debug_New3Windows()
        {
            Thread thread = new Thread(() =>
            {
                try
                {
                    _log?.Info($"@ before new w1.");
                    Window1 w1 = new Window1();
                    _log?.Info($"@ after new w1.");
                    w1.Show();
                }
                catch (Exception e1)
                {
                    _log?.Info(e1, $"new w1 exception");
                }

                try
                {
                    _log?.Info($"@ before new w2.");
                    Window1 w2 = new Window1();
                    _log?.Info($"@ after new w2.");
                    w2.Show();
                }
                catch (Exception e2)
                {
                    _log?.Info(e2, $"new w2 exception");
                }

                try
                {
                    _log?.Info($"@ before new w3.");
                    Window1 w3 = new Window1();
                    _log?.Info($"@ after new w3.");
                    w3.Show();
                }
                catch (Exception e3)
                {
                    _log?.Info(e3, $"new w3 exception");
                }

                System.Windows.Threading.Dispatcher.Run();
            });

            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();

            _log?.Info($"@ Debug_New3Windows(), after thread.Start().");
        }

        public void EABroker_Stop()
        {
            Thread thread = new Thread(() =>
            {
                Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss.fff") + " " + "[EAPlugin] EABroker_Stop.");

                //WinEventHook_Stop();
                //_vmArrange.ClearWorkWindows();
                _vmArrange.ResetWorkWindows2();

                System.Windows.Threading.Dispatcher.Run();
            });

            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();

            //if (_displayManagerPlugin != null)
            //    _displayManagerPlugin.Displaychanged -= _displayManagerPlugin_Displaychanged;
            _agent.UnregisterForEvent(AgentEventNames.DisplaySettingsChanged, DisplaySettingsChangedHandler);
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

        private EAWorkWindow? _workWin0, _workWin1, _workWin2, _workWin3, _workWin4;
        private SaveCustomWindow? _save2;

        private void InitAllWindows()
        {
            int added = 0;
            Thread thread = new Thread(() =>
            {

                if (_editWindow == null)
                {
                    try
                    {
                        LogInfo("Before new EAEditWindow");
                        _editWindow = new EAEditWindow();
                        LogInfo("After new EAEditWindow");
                        _editWindow.DataContext = _vmArrange;
                        _editWindow.Show();
                    }
                    catch (Exception exA)
                    {
                        LogException(exA, "EXCEPTION when new EAEditWindow()");
                    }
                }
                if (_saveCustomWindow == null)
                {
                    try
                    {
                        LogInfo("Before new SaveCustomWindow");
                        _saveCustomWindow = new SaveCustomWindow();
                        LogInfo("After new SaveCustomWindow");
                        if (_editWindow != null)
                        {
                            LogInfo("Setting up SaveCustomWindow");
                            _saveCustomWindow.Owner = _editWindow;
                            _saveCustomWindow.CancelButtonClick += saveCustomWidow_CancelButtonClick;
                            _saveCustomWindow.SaveButtonClick += saveCustomWidow_SaveButtonClick;
                            LogInfo("Setting up SaveCustomWindow - done");
                        }
                    }
                    catch (Exception exS)
                    {
                        LogException(exS, "EXCEPTION when new SaveCustomWindow");
                    }

                }

                if (_infoWindow == null)
                {
                    try
                    {
                        LogInfo("Before new InfoWindow");
                        _infoWindow = new InfoWindow(_vmArrange);
                        LogInfo("After new InfoWindow");
                        _infoWindow.Show();
                    }
                    catch (Exception exIn)
                    {
                        LogException(exIn, "EXCEPTION when new InfoWindow");
                    }
                }

                if (_workWin0 == null)
                {
                    try
                    {
                        LogInfo($"Before new EAWorkWindow(0)");
                        _workWin0 = new EAWorkWindow(_vmArrange);
                        LogInfo($"After new EAWorkWindow(0)");
                        _workWin0.Show();
                        _vmArrange.AddWorkWindow(_workWin0);
                    }
                    catch (Exception exW0)
                    {
                        LogException(exW0, "EXCEPTION when new EAWorkWindow(0)");
                    }
                }

                if (_workWin1 == null)
                {
                    try
                    {
                        LogInfo($"Before new EAWorkWindow(1)");
                        _workWin1 = new EAWorkWindow(_vmArrange);
                        LogInfo($"After new EAWorkWindow(1)");
                        _workWin1.Show();
                        _vmArrange.AddWorkWindow(_workWin1);
                    }
                    catch (Exception exW1)
                    {
                        LogException(exW1, "EXCEPTION when new EAWorkWindow(1)");
                    }
                }

                if (_workWin2 == null)
                {
                    try
                    {
                        LogInfo($"Before new EAWorkWindow(2)");
                        _workWin2 = new EAWorkWindow(_vmArrange);
                        LogInfo($"After new EAWorkWindow(2)");
                        _workWin2.Show();
                        _vmArrange.AddWorkWindow(_workWin2);
                    }
                    catch (Exception exW2)
                    {
                        LogException(exW2, "EXCEPTION when new EAWorkWindow(2)");
                    }
                }

                if (_workWin3 == null)
                {
                    try
                    {
                        LogInfo($"Before new EAWorkWindow(3)");
                        _workWin3 = new EAWorkWindow(_vmArrange);
                        LogInfo($"After new EAWorkWindow(3)");
                        _workWin3.Show();
                        _vmArrange.AddWorkWindow(_workWin3);
                    }
                    catch (Exception exW3)
                    {
                        LogException(exW3, "EXCEPTION when new EAWorkWindow(3)");
                    }
                }

                if (_workWin4 == null)
                {
                    try
                    {
                        LogInfo($"Before new EAWorkWindow(4)");
                        _workWin4 = new EAWorkWindow(_vmArrange);
                        LogInfo($"After new EAWorkWindow(4)");
                        _workWin4.Show();
                        _vmArrange.AddWorkWindow(_workWin4);
                    }
                    catch (Exception exW4)
                    {
                        LogException(exW4, "EXCEPTION when new EAWorkWindow(4)");
                    }
                }

                if (_awsWindow == null)
                {
                    try
                    {
                        LogInfo($"Before new AwsWindow()");
                        _awsWindow = new AwsWindow(_vmArrange);
                        LogInfo($"After new AwsWindow(3)");
                        _awsWindow.Show();
                        _vmArrange.AwsWindow = _awsWindow;

                    }
                    catch (Exception exAws)
                    {
                        LogException(exAws, "EXCEPTION when new AwsWindow");
                    }
                }



                added++;

                System.Windows.Threading.Dispatcher.Run();
            });

            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();

            while (added <= 0)
            {
                Thread.Sleep(10);
            }
        }
        #endregion EA Broker

        #region Display Changed event

        private void _displayManagerPlugin_Displaychanged(object? sender, DisplaychangedEventArgs e)
        {
            _log?.Info($"@ OnDisplaychanged, ChangedCount={e.count}");
            //Invoke_RefreshWorkWindows();
        }

        private void DisplaySettingsChangedHandler(object sender, EventManagerArgs e)
        {
            _log?.Info($"@ OnDisplaychanged");

            if (_vmArrange != null)
            {
                _vmArrange.RefreshEAScreens();
                _vmArrange.RefreshWorkWindows2();


            }
        }

        #endregion Display Changed event

        #region InfoWindow

        private InfoWindow _infoWindow;

        private void InitInfoWindow()
        {
            if (_infoWindow != null)
                return;

            int added = 0;
            Task.Run(() =>
            {
                int addCount = 0;
                Thread thread = new Thread(() =>
                {
                    LogInfo("Before new InfoWindow");
                    _infoWindow = new InfoWindow(_vmArrange);
                    LogInfo("After new InfoWindow");
                    //_infoWindow.DataContext = _vmArrange;
                    _infoWindow.Show();
                    addCount++;
                    added++;
                    System.Windows.Threading.Dispatcher.Run();
                });

                thread.SetApartmentState(ApartmentState.STA);
                thread.IsBackground = true;
                thread.Start();

                while (addCount <= 0)
                {
                    Thread.Sleep(10);
                }
                //thread.Abort();
                return Task.CompletedTask;
            });
            while (added <= 0)
            {
                Thread.Sleep(10);
            }


        }

        #endregion InfoWindow

        #region WorkWindows

        private void InitWorkWindows()
        {
            if (_vmArrange != null)
                _vmArrange.CreateWorkWindows2();

            Thread.Sleep(2000);
        }

        //private Dictionary<string, EAWorkWindow> _workWindows_Unused = new Dictionary<string, EAWorkWindow>();
        //private EAWorkWindow _tempWorkWindow_Unused;

        //[Standalone solution]
        //private void InitWorkWindows()
        //{
        /*
        //Double check, it should be true if it has ran into Broker_Start()
        if (!_displayManagerPluginUsable)
        {
            LogInfo("@ InitWorkWindows, exit due to _displayManagerPluginUsable is false.");
            return;
        }
        LogInfo("@ InitWorkWindows");


        var dpiXProperty = typeof(SystemParameters).GetProperty("DpiX", BindingFlags.NonPublic | BindingFlags.Static);
        var varX = (int)dpiXProperty.GetValue(null, null);
        double dpiX = (double)varX / (double)96;
        LogInfo($"  * dpiX={dpiX}");

        //GetMonitors() will return all supported Monitors (Dell Monitors)
        List<MonitorInfo>? monitors = GetMonitors();
        if (monitors == null)
        {
            LogInfo($"  * Monitors is null.");
            return;
        }
        if (monitors.Count <= 0)
        {
            LogInfo($"  * Monitors is empty.");
            return;
        }
        LogInfo($"  * Monitors.Count={monitors.Count}");

        //Default SplitCtrl, in official release it sould be read from per-monitor settings file
        //int cellCount = 0;
        //char splitKey = 'A';
        //List<double> settings = new List<double>();

        _vmArrange.ClearWorkWindows();

        //Rebuild WorkWindows in a temp list
        Dictionary<string, EAWorkWindow> tempWorkWindows = new Dictionary<string, EAWorkWindow>();

        //Refresh with new AllScreens
        LogInfo($"  * Refreshing WorkWindows... AllScreens.Count={System.Windows.Forms.Screen.AllScreens.Length}");
        int idxScr = 0;
        int addCount = 0;
        foreach (Screen scr in System.Windows.Forms.Screen.AllScreens)
        {
            bool isVertical = (scr.Bounds.Width < scr.Bounds.Height);
            double left = scr.WorkingArea.Left / (double)dpiX;
            double top = scr.WorkingArea.Top / (double)dpiX;
            double width = scr.WorkingArea.Width / (double)dpiX;
            double height = scr.WorkingArea.Height / (double)dpiX;
            LogInfo($"    - Screen[{idxScr}] {scr.DeviceName}   IsPrimary={scr.Primary}");
            LogInfo($"      WorkingArea: ({left},{top}){width}x{height}");

            //Find all monitors which have the same DeviceName (DisplayName)
            List<MonitorInfo> attachedMonitors = monitors.FindAll(x => x.DisplayName.Equals(scr.DeviceName, StringComparison.OrdinalIgnoreCase));

            //If there is no any Dell Monitor attached on this Screen, then do not need to create a
            // Workwindow for it
            if ((attachedMonitors == null) || (attachedMonitors.Count <= 0))
            {
                LogInfo($"      No attached Monitor for this screen => No WorkWindow to create for it.");
                idxScr++;
                continue;
            }

            //Dump attached monitors
            LogInfo($"    - AttachedMonitors");
            int idxMonitor = 0;
            foreach (MonitorInfo mi in attachedMonitors)
            {
                LogInfo($"        [{mi.Index}] Name=[{mi.AliasDeviceName}], Model=[{mi.modelName}], ServiceTag=[{mi.edid.ServiceTag}], MarketName=[{mi.MarketingName}]");
                idxMonitor++;
            }

            MonitorInfo miWork = attachedMonitors[0];
            //_deviceManagerPlugin.ShowOSD(miWork, OSDType.DisplayChanged);

            //Create a WorkWindow work for it
            //
            Thread thread = new Thread(() =>
            {
                //Read settings for this monitor
                LogInfo($"  * ReadEAMonitorSettings({miWork.modelName}/{miWork.edid.ServiceTag})");
                EAMonitorSettings eaSettings = _deviceManagerPlugin.ReadEAMonitorSettings(miWork).Result;
                int cellCount = eaSettings.SelectedSplit.CellCount;
                char splitKey = eaSettings.SelectedSplit.SplitKey;
                List<double> settings = eaSettings.SelectedSplit.Settings;

                LogInfo($"  * InitWorkSplit: {eaSettings.SelectedSplit.ToString()}");

                EAWorkWindow workWin = new EAWorkWindow(_vmArrange, scr, attachedMonitors);

                workWin.Left = left;
                workWin.Top = top;
                workWin.Width = width;
                workWin.Height = height;
                workWin.IsVertical = isVertical;

                LogInfo($"  * SetWorkWindow pos ({left},{top}){width}x{height}, Split={cellCount}{splitKey}");
                workWin.SetWorkingSplit(cellCount, splitKey, settings);
                workWin.Show();
                tempWorkWindows.Add(scr.DeviceName, workWin);
                LogInfo($"  * Add WorkWindow for [{idxScr}]{scr.DeviceName}.");

                System.Windows.Threading.Dispatcher.Run();
            });

            addCount++;
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            //thread.Join(2000); //Wait until thread finished
            idxScr++;
        } //foreach(Screen scr)

        //Wait for all WorkWindows are added into tempWorkWindows
        while (tempWorkWindows.Count < addCount)
        {
            Thread.Sleep(10);
        }
        _vmArrange.WorkWindows = tempWorkWindows;
        */
        //}

        //private void ClearWorkWindows()
        //{
        //    foreach (KeyValuePair<string, EAWorkWindow> keyValuePair in _workWindows)
        //    {
        //        keyValuePair.Value.Close();
        //    }
        //    _workWindows.Clear();
        //}

        //private void Invoke_RefreshWorkWindows()
        //{
        //    Thread thread = new Thread(() =>
        //    {
        //        UI_RefreshWorkWindows();
        //        System.Windows.Threading.Dispatcher.Run();
        //    });

        //    thread.SetApartmentState(ApartmentState.STA);
        //    thread.Start();
        //}

        //private void UI_RefreshWorkWindows()
        //{
        //    /*
        //    if (_infoWindow != null)
        //    {
        //        _infoWindow.RefreshWorkWindows();
        //        return;
        //    }

        //    //Double check, it should be true if it has ran into Broker_Start()
        //    if (!_displayManagerPluginUsable)
        //    {
        //        LogInfo("@ UI_RefreshWorkWindows, exit due to _displayManagerPluginUsable is false.");
        //        return;
        //    }
        //    */
        //    LogInfo("@ UI_RefreshWorkWindows");

        //    //Case 4 => failed
        //    //Move into InfoWindow, use Dispathcer to invoke => return is failed
        //    //_infoWindow.RefreshWorkWindows();

        //    var dpiXProperty = typeof(SystemParameters).GetProperty("DpiX", BindingFlags.NonPublic | BindingFlags.Static);
        //    var varX = (int)dpiXProperty.GetValue(null, null);
        //    double dpiX = (double)varX / (double)96;
        //    LogInfo($"  * dpiX={dpiX}");

        //    //GetMonitors() will return all supported Monitors (Dell Monitors)
        //    List<MonitorInfo>? monitors = GetMonitors();
        //    /*
        //    if (monitors == null)
        //    {
        //        LogInfo($"  * Monitors is null.");
        //        return;
        //    }
        //    if (monitors.Count <= 0)
        //    {
        //        LogInfo($"  * Monitors is empty.");
        //        return;
        //    }
        //    LogInfo($"  * Monitors.Count={monitors.Count}");
        //    */

        //    //Default SplitCtrl, in official release it sould be read from per-monitor settings file
        //    int cellCount = 2;
        //    char splitKey = 'A';
        //    List<double> settings = new List<double>();

        //    //_vmArrange.ClearWorkWindows();

        //    //Rebuild WorkWindows in a temp list
        //    Dictionary<string, EAWorkWindow> tempWorkWindows = new Dictionary<string, EAWorkWindow>();

        //    //Refresh with new AllScreens
        //    LogInfo($"  * Refreshing WorkWindows... AllScreens.Count={System.Windows.Forms.Screen.AllScreens.Length}");
        //    int idxScr = 0;
        //    int addCount = 0;
        //    foreach (Screen scr in System.Windows.Forms.Screen.AllScreens)
        //    {
        //        bool isVertical = (scr.Bounds.Width < scr.Bounds.Height);
        //        double left = scr.WorkingArea.Left / (double)dpiX;
        //        double top = scr.WorkingArea.Top / (double)dpiX;
        //        double width = scr.WorkingArea.Width / (double)dpiX;
        //        double height = scr.WorkingArea.Height / (double)dpiX;
        //        LogInfo($"    - Screen[{idxScr}] {scr.DeviceName}   IsPrimary={scr.Primary}");
        //        LogInfo($"      WorkingArea: ({left},{top}){width}x{height}");

        //        EAWorkWindow workWin = null;

        //        //Try to find if the WorkWindow work for current scr is exist
        //        if (_vmArrange.WorkWindows.TryGetValue(scr.DeviceName, out workWin))
        //        {
        //            LogInfo($"      Changed: (Exist => refresh WorkingArea)");
        //            //The scr have an existing WorkWindow, no need to create new
        //            //Just to renew some screen properties
        //            //workWin.Left = left;
        //            //workWin.Top = top;
        //            //workWin.Width = width;
        //            //workWin.Height = height;
        //            workWin.ChangeWindowPos(left, top, width, height);

        //            //Refresh screen orientation (not been implemented)

        //            //Add to temp workwindows
        //            addCount++;
        //            tempWorkWindows.Add(scr.DeviceName, workWin);
        //            //Remove from old dictionary
        //            _vmArrange.RemoveWorkWindow(scr.DeviceName);
        //        }
        //        else
        //        {
        //            //Add new WorkWindow
        //            LogInfo($"      Changed: (Added => Add new WorkWindow)");

        //            //Cannot find the WorkWindow which is work for scr => scr is a new screen
        //            //We will need to create a new WorkWindow work for scr
        //            List<MonitorInfo> attachedMonitors = monitors.FindAll(x => x.DisplayName.Equals(scr.DeviceName, StringComparison.OrdinalIgnoreCase));

        //            //If there is no any Dell Monitor attached on this Screen, then do not need to create a
        //            // Workwindow for it

        //            if ((attachedMonitors == null) || (attachedMonitors.Count <= 0))
        //            {
        //                LogInfo($"      No attached Monitor for this screen => No WorkWindow to creat for it.");
        //                continue;
        //            }

        //            //Dump attached monitors
        //            LogInfo($"    - AttachedMonitors");
        //            int idxMonitor = 0;
        //            foreach (MonitorInfo mi in attachedMonitors)
        //            {
        //                LogInfo($"        [{mi.Index}] Name={mi.AliasDeviceName}]");
        //                idxMonitor++;
        //            }

        //            //Create a WorkWindow work for it
        //            //
        //            Thread thread = new Thread(() =>
        //            {
        //                //Read settings for this monitor

        //                //Case 2 => failed
        //                //New a WorkWindow in local
        //                EAWorkWindow addedWorkWin = new EAWorkWindow(_vmArrange, scr, attachedMonitors);
        //                addedWorkWin.Left = left;
        //                addedWorkWin.Top = top;
        //                addedWorkWin.Width = width;
        //                addedWorkWin.Height = height;
        //                addedWorkWin.SetWorkingSplit(cellCount, splitKey, settings);
        //                addedWorkWin.Show();
        //                tempWorkWindows.Add(scr.DeviceName, addedWorkWin);

        //                //Case 3 => failed
        //                //New a WorkWindow in class member
        //                //_tempWorkWindow = new EAWorkWindow(_vmArrange, scr, attachedMonitors);
        //                //_tempWorkWindow.Left = left;
        //                //_tempWorkWindow.Top = top;
        //                //_tempWorkWindow.Width = width;
        //                //_tempWorkWindow.Height = height;
        //                //_tempWorkWindow.SetWorkingSplit(cellCount, splitKey, settings);
        //                //_tempWorkWindow.Show();
        //                //tempWorkWindows.Add(scr.DeviceName, _tempWorkWindow);

        //                System.Windows.Threading.Dispatcher.Run();
        //            });
        //            addCount++;
        //            thread.SetApartmentState(ApartmentState.STA);
        //            thread.Start();
        //        }
        //        idxScr++;
        //    } //foreach (Screen scr)

        //    //Case_3 Exist->NotExist, a display has been unplugged
        //    LogInfo($"  * Removing unplugged WorkWindows... Count={_vmArrange.WorkWindows.Count}");
        //    _vmArrange.ClearWorkWindows();

        //    //foreach (KeyValuePair<string, EAWorkWindow> pair in _vmArrange.WorkWindows)
        //    //{
        //    //    _vmArrange.WorkWindows.Remove(pair.Key);
        //    //    //Close the workWin
        //    //    pair.Value.DispatcherClose();
        //    //}

        //    //Wait for all WorkWindows are added into tempWorkWindows
        //    while (tempWorkWindows.Count < addCount)
        //    {
        //        Thread.Sleep(10);
        //    }
        //    _vmArrange.WorkWindows = tempWorkWindows;
        //}

        #endregion WorkWindows

        #region AWS Window
        private AwsWindow _awsWindow;

        private void InitAwsWindow()
        {
            if (_awsWindow != null)
                return;

            int added = 0;
            Task.Run(() =>
            {
                int addCount = 0;
                Thread thread = new Thread(() =>
                {
                    LogInfo("Before new AwsWindow");
                    _awsWindow = new AwsWindow(_vmArrange);
                    LogInfo("After new AwsWindow");
                    //_infoWindow.DataContext = _vmArrange;
                    _awsWindow.Show();
                    _vmArrange.AwsWindow = _awsWindow;  
                    addCount++;
                    added++;
                    System.Windows.Threading.Dispatcher.Run();
                });

                thread.SetApartmentState(ApartmentState.STA);
                thread.IsBackground = true;
                thread.Start();

                while (addCount <= 0)
                {
                    Thread.Sleep(10);
                }
                //thread.Abort();
                return Task.CompletedTask;
            });
            while (added <= 0)
            {
                Thread.Sleep(10);
            }


        }
        #endregion AWS Window

        #region EditWindow and SaveCustomWindow

        /// <summary>
        /// Create a EAEditWindow (assign to unique _editWindow)
        /// And display it (but the Window is transparent until we are handing EditCommand())
        /// </summary>
        private void InitEditWindow()
        {
            if (_editWindow != null)
                return;

            int added = 0;
            Task.Run(() =>
            {
                //Robert_Lin
                int addCount = 0;
                Thread thread = new Thread(() =>
                {
                    LogInfo("Before new EAEditWindow");
                    _editWindow = new EAEditWindow();
                    LogInfo("After new EAEditWindow");
                    _editWindow.DataContext = _vmArrange;
                    _editWindow.Show();

                    _saveCustomWindow = new SaveCustomWindow();

                    addCount++;
                    added++;

                    System.Windows.Threading.Dispatcher.Run();
                });

                thread.SetApartmentState(ApartmentState.STA);
                thread.Start();

                //thread.Abort

                while (addCount <= 0)
                {
                    Thread.Sleep(100);
                }

                return Task.CompletedTask;
            }); //Task.Run()

            while (added <= 0)
            {
                Thread.Sleep(100);
            }
        }


        private void InitSaveCustomWindow()
        {
            if (_saveCustomWindow != null)
                return;

            int added = 0;
            Task.Run(() =>
            {
                int addCount = 0;
                Thread thread = new Thread(() =>
                {
                    LogInfo("Before new SaveCustomWindow");
                    _saveCustomWindow = new SaveCustomWindow();
                    LogInfo("After new SaveCustomWindow");
                    //_saveCustomWindow.Show();

                    addCount++;
                    added++;

                    _saveCustomWindow.CancelButtonClick += saveCustomWidow_CancelButtonClick;
                    _saveCustomWindow.SaveButtonClick += saveCustomWidow_SaveButtonClick;

                    System.Windows.Threading.Dispatcher.Run();
                });

                thread.SetApartmentState(ApartmentState.STA);
                thread.IsBackground = true;
                thread.Start();

                while (addCount <= 0)
                {
                    Thread.Sleep(10);
                }
                return Task.CompletedTask;
            });
            while (added <= 0)
            {
                Thread.Sleep(10);
            }
        }

        private void saveCustomWidow_CancelButtonClick(object sender, string e)
        {
            //if (EditCompleted != null)
            //    EditCompleted(this, "");

            if (EditReturn != null)
            {
                EAArgs retArgs = new EAArgs(_eaArgs);
                retArgs.Result = false;
                retArgs.Command = "EditReturn";
                retArgs.Message = "User cancel the editing.";
                EditReturn(this, retArgs);
            }
            _editWindow.InvokeClose();
            _saveCustomWindow.Hide();

            _vmArrange.IsWorkUIEnabled = true;
        }

        private void saveCustomWidow_SaveButtonClick(object sender, string e)
        {
            //if (EditCompleted != null)
            //    EditCompleted(this, "");

            if (EditReturn != null)
            {
                EAArgs retArgs = new EAArgs(_eaArgs);
                retArgs.Result = true;

                retArgs.Settings = _editWindow.GetSettings();
                retArgs.CustomName = e;
                retArgs.Command = "EditReturn";
                EditReturn(this, retArgs);
            }
            _editWindow.InvokeClose();
            _saveCustomWindow.Hide();
            _vmArrange.IsWorkUIEnabled = true;
        }

        #endregion EditWindow and SaveCustomWindow

        #region Window Event Hook - Unused

        //private WinEventHook _winEventHook = new WinEventHook();

        //private void WinEventHook_Start()
        //{
            //_winEventHook.OnStartMoving += OnWindowStartMovingProc;
            //_winEventHook.OnEndMoving += OnWindowEndMovingProc;
            //_winEventHook.OnLocationChanged += OnLocationChangedProc;
            //_winEventHook.OnForegroundWindowChanged += OnForegroundWindowChangedProc;
            //_winEventHook.Hook();
        //}

        //private void WinEventHook_Stop()
        //{
            //_winEventHook.Unhook();
            //_winEventHook.OnStartMoving -= OnWindowStartMovingProc;
            //_winEventHook.OnEndMoving -= OnWindowEndMovingProc;
            //_winEventHook.OnLocationChanged -= OnLocationChangedProc;
            //_winEventHook.OnForegroundWindowChanged -= OnForegroundWindowChangedProc;
        //}

        //private void OnForegroundWindowChangedProc(IntPtr hWndNew, IntPtr hWndOld)
        //{
            //Noting to do in this project
        //}

        //private bool _isDebuggingOnWindowStartMoving_Unused = true;

        //private void OnWindowStartMovingProc(IntPtr hWnd)
        //{
            //if (_isDebuggingOnWindowStartMoving)
            //    _log?.Info($"Enter OnWindowStartMovingProc(), hWnd=0x{hWnd:X}");

            //if (!_vmArrange.IsFunctionEnabled)
            //    return;

            //Process process;
            //string msg;
            //if (WinEventHook.GetProcessFromWindowHandle(hWnd, out process, out msg))
            //{
            //    //Try to get the PathName of the process
            //    try
            //    {
            //        if (process.MainModule != null)
            //        {
            //            if (!String.IsNullOrEmpty(process.MainModule.FileName))
            //            {
            //                string pathName = process.MainModule.FileName;
            //                if (_isDebuggingOnWindowStartMoving)
            //                    _log?.Info($"Process.PathName={pathName}");
            //            }
            //        }
            //    }
            //    catch (Exception e1)
            //    {
            //        _log?.Info($"@OnWindowStartMovingProc, access to process causes an exception, msg: {e1.Message}");

            //        //Temporary allow to continue moving
            //        _vmArrange.IsMoving = true;
            //        //Robert_Lin Debug, let it contine
            //        //return;
            //    }
            //}
            //else
            //{
            //    _log.Info($"@OnWindowStartMovingProc, GetProcessFromWindowHandle error, msg:{msg}");
            //}

            //var dpiXProperty = typeof(SystemParameters).GetProperty("DpiX", BindingFlags.NonPublic | BindingFlags.Static);
            //var varX = (int)dpiXProperty.GetValue(null, null);
            //double dpiX = (double)varX / (double)96;

            //_vmArrange.ScreenScale = dpiX;
            //_vmArrange.IsMoving = true;
            //RefreshCellRects();
        //}

        //private void OnWindowEndMovingProc(IntPtr hWnd, bool isCanceled = false)
        //{
            //bool isWorkUIShowing = _vmArrange.IsWorkUIShowing;

            //if (!_vmArrange.IsMoving)
            //    return;

            //_vmArrange.IsMoving = false;

            //if (!isWorkUIShowing)
            //    return;

            //if (_vmArrange.HoveringCellObj == null)
            //    return;

            ////Check if user cancel the window moving by pressing [Esc] key
            ////Assumption:
            //// When user moving window, the mouse [LeftButton] is pressed and hold.
            //// When user canceling the moving, he/she press [Esc] key and the
            ////     mouse [LeftButton] is strll pressed and hold.
            ////
            //if (WinEventHook.IsUserCancelMoving())
            //    return;

            //Rect rcArrange = _vmArrange.HoveringCellObj.rc;

            ////Inflate the rect, because the rcArrange not include the border thickness(=6) of CellBorder
            //rcArrange.Inflate(6, 6);
            //WinEventHook.SetWindowPosition(hWnd, rcArrange);
        //}

        //private void OnLocationChangedProc(int x, int y)
        //{
            //_vmArrange.xCursor = x;
            //_vmArrange.yCursor = y;

            //if (!_vmArrange.IsWorkUIShowing)
            //    return;

            //CellObj orgCell = _vmArrange.HoveringCellObj;
            //_vmArrange.HoveringCellObj = DetermineHoveringCellObj(x, y);

            //if (orgCell != _vmArrange.HoveringCellObj)
            //{
            //    string strOrg = "null";
            //    if (orgCell != null)
            //        strOrg = orgCell.Name;
            //    string strNew = "null";
            //    if (_vmArrange.HoveringCellObj != null)
            //        strNew = _vmArrange.HoveringCellObj.Name;

            //    //Trace.WriteLine($" * HoveringCell: {strOrg}->{strNew}");
            //}
            //if (_vmArrange.HoveringCellObj != null)
            //{
            //    _vmArrange.HoveringCell = _vmArrange.HoveringCellObj.Name;
            //}
            //else
            //{
            //    _vmArrange.HoveringCell = "";
            //}
            ////if (_workingSplit != null)
            ////    _workingSplit.VM.HoveringCell = vm.HoveringCell;

            ////Set WorkWins to topmost
        //}

        //private void RefreshCellRects()
        //{
            //foreach (KeyValuePair<string, EAWorkWindow> keyValuePair in _workWindows)
            //{
            //    EAWorkWindow workWin = keyValuePair.Value;
            //    workWin.Invoke_RefreshCellRects();
            //}
        //}

        //private CellObj? DetermineHoveringCellObj(int x, int y)
        //{
            //foreach (KeyValuePair<string, EAWorkWindow> keyValuePair in _workWindows)
            //{
            //    EAWorkWindow workWin = keyValuePair.Value;
            //    CellObj? cellObj = workWin.DetermineHoveringCellObj(x, y);
            //    if (cellObj != null)
            //    {
            //        return cellObj;
            //    }
            //}
            //return null;
        //}

        #region GetAsyncKeyState

        //private const short VK_ESCAPE = 0x1b;
        //private const short VK_LBUTTON = 0x01;

        //[DllImport("User32.dll")]
        //private static extern short GetAsyncKeyState(System.Int32 vKey);

        #endregion GetAsyncKeyState

        #endregion Window Event Hook

        #region Helpers

        //private EAWorkWindow? FindWorkWindowByMonitorInfo(MonitorInfo monitorInfo)
        //{
        //    EAWorkWindow workWindow = null;
        //    string key = monitorInfo.DisplayName;
        //    if (_workWindows.TryGetValue(key, out workWindow))
        //    {
        //        return workWindow;
        //    }
        //    return null;
        //}

        /// <summary>
        /// Format a Rectangle to string, format: "(0,0)-(1920,1200)1920x1200"
        /// </summary>
        /// <param name="rc"></param>
        /// <returns></returns>
        //private string FormatRectangle(Rectangle rc)
        //{
        //    return $"({rc.Left},{rc.Top})-({rc.Right},{rc.Bottom}){rc.Width}x{rc.Height}";
        //}

        #endregion Helpers

        #region Debug Msg

        public static void Dmsg(string msg)
        {
            Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss.fff") + " " + msg);
        }

        #endregion Debug Msg

        #region General DDPM.SA Plugins Methods

        private List<MonitorInfo>? GetMonitors()
        {
            if (_displayManagerPlugin == null)
                return null;

            return _displayManagerPlugin.GetMonitors().Result;
        }

        #endregion General DDPM.SA Plugins Methods

        #region Test Monitor Settings
        //private void Test_MonitorSettings()
        //{
        //    //Read
        //    EAMonitorSettings eaSettings = _deviceManagerPlugin.ReadEAMonitorSettings()
        //}
        #endregion
    }
}