//#define REMOVE_EA
//Define this flag will remove EA functions

using CommunityToolkit.Mvvm.DependencyInjection;
using DDPM.Easy.Common;
using DDPM.SA.Common;
using DDPM.SA.Common.Interfaces;
using DDPM.SA.Common.Settings;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.Common.Annotations;
using Dell.Client.Framework.Common.PluginConditions;
using Dell.Client.Framework.Interfaces;
using Microsoft;
using Microsoft.Extensions.DependencyInjection;
using nsWinEventHook;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Windows;
using System.Windows.Media.Animation;
using VcpCore.Common;
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
        private const string PluginLogId = "SAEA";

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

        #region Log/Debug messages

        private void LogInfo(string msg)
        {
            _log?.Info(msg);
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
            InitializeDeviceManagerPlugin();
            InitializeDisplayManagerPlugin();
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

        //Robert_Lin, 2024-0910, unused method, will be removed
        public Task<bool> RequestEditSplit(MonitorInfo monitorInfo, int cellCount, char splitKey, string customName, List<double>? settings = null)
        {
            return Task.FromResult(false); //Remove this statement if you would like it be executed.

            //To avoid reenter Edit mode. If we are in Edit mode already, then return false
            if (_editWindow != null)
            {
                return Task.FromResult(false);
            }

            Thread thread = new Thread(() =>
            {
                Console.WriteLine("[EAPlugin] RequestEditSplit().");
                UI_RequestEditSplit(monitorInfo, cellCount, splitKey, customName, settings);
                System.Windows.Threading.Dispatcher.Run();
            });

            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();

            return Task.FromResult(true);
        }

        //Robert_Lin, 2024-8-5 Old interface, to be removed.
        private bool UI_RequestEditSplit(MonitorInfo monitorInfo, int cellCount, char splitKey, string customName, List<double>? settings = null)
        {
            /*
            //Get the DisplayName from MonitorInfo
            string displayName = monitorInfo.DisplayName;
            //Get the target Screen from the displayName
            Screen? scr = Screen.AllScreens.FirstOrDefault(x => x.DeviceName.Equals(displayName, StringComparison.OrdinalIgnoreCase));
            //If no matched screen found, then report error
            if (scr == null)
            {
                return false;
            }
            bool isVertical = (scr.Bounds.Width < scr.Bounds.Height);

            //Find the WorkWindow of the target screen
            EAWorkWindow? workWin = FindWorkWindowByMonitorInfo(monitorInfo);
            if (workWin != null)
            {
            }

            //Stop WorkWindow fade out animation

            //Create a new EAEditWindow
            if (_editWindow != null)
            {
                _editWindow.Close();
                _editWindow = null;
            }
            EAEditWindow editWin = new EAEditWindow();
            editWin.EditCompleted += (object? sender, string result) =>
            {
                _vmArrange.IsWorkUIEnabled = true;
                _editWindow?.Close();
                _editWindow= null;

                if (EditCompleted != null)
                {
                    EditCompleted(this, result);
                }
            };

            double dpiX = 1.000;
            var dpiXProperty = typeof(SystemParameters).GetProperty("DpiX", BindingFlags.NonPublic | BindingFlags.Static);
            if (dpiXProperty != null)
            {
                var varX = (int)dpiXProperty.GetValue(null, null);
                dpiX = (double)varX / (double)96;
            }

            editWin.SetSplitCtrl(cellCount, splitKey, settings, isVertical);

            editWin.Left = scr.WorkingArea.Left / (double)dpiX;
            editWin.Top = scr.WorkingArea.Top / (double)dpiX;
            editWin.Width = scr.WorkingArea.Width / (double)dpiX;
            editWin.Height = scr.WorkingArea.Height / (double)dpiX;

            editWin.Show();
            _editWindow = editWin;

            //Signal EditStart event
            if (EditStarted != null)
                EditStarted(this, "");

            _vmArrange.IsWorkUIEnabled = false;
            */
            return true;
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

        //Unused event to be removed
        public event EventHandler<string> EditCompleted;

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
                UI_EditCommand(monitorInfo, args);
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

        private bool UI_EditCommand(MonitorInfo monitorInfo, EAArgs args)
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


            if (!editWin.SetInputArg(args, scr))
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


            //if (_editWindow != null)
            //{
            //    _editWindow.SetInputArg(args);
            //    _editWindow.Left = 0;
            //    _editWindow.Top = 0;
            //    _editWindow.Width = 1024;
            //    _editWindow.Height = 768;
            //}

            ////Create a new EAEditWindow
            //if (_editWindow != null)
            //{
            //    _editWindow.Close();
            //    _editWindow = null;
            //}

            //_editWindow = new EAEditWindow();
            //EAEditWindow editWin = new EAEditWindow();

            _eaArgs = args;
            //Register callback for EditReturn from the EditWindow
            //editWin.EditCompleted += (object? sender, string result) =>
            _editWindow.EditReturn += (object? sender, EAArgs args) =>
            {
                _vmArrange.IsWorkUIEnabled = true;
                _editWindow?.Hide();

                if (EditReturn != null)
                {
                    EditReturn(this, args);
                }
            };

            //Calculate the position/size of EditWindow
            double dpiX = 1.000;
            var dpiXProperty = typeof(SystemParameters).GetProperty("DpiX", BindingFlags.NonPublic | BindingFlags.Static);
            if (dpiXProperty != null)
            {
                var varX = (int)dpiXProperty.GetValue(null, null);
                dpiX = (double)varX / (double)96;
            }

            if (!_editWindow.SetInputArg(args, scr))
            {
                if (EditStarted != null)
                {
                    EditStarted(this, _editWindow.LastError);
                }
                return false;
            }

            if (_saveCustomWindow != null)
            {
                _saveCustomWindow.SetInputArg(args, scr);
            }

            //Robert_Lin, 2024-8-26, cannot calling to _editWindow.Show()
            // _editWindow.SetInputArg( ) will call Show() inside _editWindow itself.
            //_editWindow.Show();
            //_editWindow = editWin;

            //Signal EditStart event
            if (EditStarted != null)
                EditStarted(this, "");

            _vmArrange.IsWorkUIEnabled = false;

            return true;
        }

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

                //InitInfoWindow();
                //InitWorkWindows();
                //InitEditWindow();
                //InitSaveCustomWindow();

                //[InfoWin Solution]
                InitEditWindow();
                InitSaveCustomWindow();
                InitInfoWindow();
                InitWorkWindows();

                if (_displayManagerPlugin != null)
                    _displayManagerPlugin.Displaychanged += _displayManagerPlugin_Displaychanged;

                //Microsoft.Win32.SystemEvents.DisplaySettingsChanged += SystemEvents_DisplaySettingsChanged;
                _agent.RegisterForEvent(AgentEventNames.DisplaySettingsChanged, DisplaySettingsChangedHandler);
                _agent.RaiseEvent(AgentEventNames.DisplaySettingsChanged, this, new EventManagerArgs());
            }
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

            //[Standalone Solution]
            //InitWorkWindows();

            //[InfoWin solution]
            //if (_infoWindow != null)
            //    _infoWindow.InitWorkWindows();
            if (_vmArrange != null)
            {
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


            Task.Run(() =>
            {
                int addCount = 0;
                Thread thread = new Thread(() =>
                {
                    _infoWindow = new InfoWindow(_vmArrange);
                    //_infoWindow.DataContext = _vmArrange;
                    _infoWindow.Show();
                    addCount++;
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

        #region EditWindow and SaveCustomWindow

        //[Standalone solution]
        private void InitEditWindow()
        {
            if (_editWindow != null)
                return;

            Task.Run(() =>
            {
                int addCount = 0;
                Thread thread = new Thread(() =>
                {
                    _editWindow = new EAEditWindow();
                    _editWindow.DataContext = _vmArrange;
                    _editWindow.Show();
                    addCount++;
                    System.Windows.Threading.Dispatcher.Run();
                });

                thread.SetApartmentState(ApartmentState.STA);
                thread.Start();

                while (addCount <= 0)
                {
                    Thread.Sleep(100);
                }

                return Task.CompletedTask;
            });
        }

        private void InitSaveCustomWindow()
        {
            if (_saveCustomWindow != null)
                return;

            Task.Run(() =>
            {
                int addCount = 0;
                Thread thread = new Thread(() =>
                {
                    _saveCustomWindow = new SaveCustomWindow();
                    //_saveCustomWindow.Show();

                    addCount++;

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
    }
}