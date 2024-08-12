
using DDPM.SA.Common;
using Dell.Client.Framework.Common.Annotations;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.Interfaces;
using Microsoft;
using Dell.Client.Framework.Common.PluginConditions;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Windows;
using nsWinEventHook;
using DDPM.Easy.Common;
using DDPM.SA.Common.Interfaces;
using VcpCore.Common;
using IDs= DDPM.SA.Common.IDs;
using Windows.Media.Streaming.Adaptive;
using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

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
        #endregion

        public static readonly Ioc PluginIoc = new();
        private bool _isConfigured = false;

        #region Constructor
        public EAPlugin(IAgent agent) : base(agent, PluginLogId)
        {
            _agent = agent;
            _IsAdministrator = ProcessSecurityHelperWrapper.IsCurrentProcessRunningElevated();
            _log = Log;
            //_logs = new Logs(Log, pluginName);
            //_logs.DebugMsg($"[EAPlugin] is constructed, IsAdministrator={_IsAdministrator}.", true);
            _log?.Info($"[{pluginName}] is constructed.");
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
                    EABroker_Stop();
                    _agent = null;
                }
                IsDisposed = true;
            }
            base.Dispose(disposing);
        }
        #endregion   

        #region Overriding methods
        //The method is called by Agent when the plugin is starting
        protected override void OnPluginStarting()
        {
            PluginCondition = new PluginStartedCondition();
            _agent.PluginManager.PluginsStarted += PluginManagerOnPluginsStarted;

            //Monitoring plugins state
            // 2024-08-06 Elie, Mask InitializeDeviceManagerPlugin() function to skip .NET 8 for more than two monitor cause exception issue. ==> System.IO.IOException: 'Cannot locate resource 'eaworkwindow.baml'.'
            //InitializeDeviceManagerPlugin();
            //InitializeDisplayManagerPlugin();
        }
        #endregion

        #region PluginManager related
        private void PluginManagerOnPluginsStarted(object sender, PluginsStartedEventArgs e)
        {
            if (e == null)
                return;
            if (e.ChangedPlugins == null)
                return;
            if (e.ChangedPlugins.Any() == false)
                return;

            _log?.Info($"[{pluginName} @PluginManagerOnPluginsStarted, ChangedPlugins.Count={e.ChangedPlugins.Count}");
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
            _log?.Info($"[{pluginName}] Initializing DeviceManager plugin.");
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
                        _log?.Info($"[{pluginName}] DeviceManager plugin is in an error condition");
                        _deviceManagerPluginCondition = pluginCondition;
                        _deviceManagerPluginUsable = false;
                    }
                    else if (pluginCondition is PluginStartedCondition)
                    {
                        _log?.Info($"[{pluginName}] DeviceManager plugin is in a started condition");
                        _deviceManagerPluginCondition = pluginCondition;
                        _deviceManagerPluginUsable = true;

                        ConfigureServices();
                        //Only after DisplayManager is ready to use, will start the EasyArrange service
                        EABroker_Start();
                    }
                    else
                    {
                        _log?.Info($"[{pluginName}] DeviceManager plugin is in others condition");
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
                        _log?.Info($"[{pluginName}] {nameof(GetCurrentDisplayManagerPluginCondition)} - Display ManagerPlugin is in an error condition");
                        _displayManagerPluginCondition = pluginCondition;
                        _displayManagerPluginUsable = false;
                    }
                    else if (pluginCondition is PluginStartedCondition)
                    {
                        _log?.Info($"[{pluginName}] {nameof(GetCurrentDisplayManagerPluginCondition)} -Display ManagerPlugin is in a started condition");
                        _displayManagerPluginCondition = pluginCondition;
                        _displayManagerPluginUsable = true;

                        //Only after DisplayManager is ready to use, will start the EasyArrange service
                        EABroker_Start();

                    }
                }
            });
        }
        private void OnDisplayManagerPluginConditionChangeHandler(object sender, EventArgs e)
        {
            GetCurrentDisplayManagerPluginCondition();
        }
        #endregion

        #region IEasyArrangeService Implementation

        public bool IsFunctionEnabled
        {
            get => _vmArrange.IsFunctionEnabled;
            set => _vmArrange.IsFunctionEnabled = value;
        }

        public Task<bool> SetEAWrokSplit(MonitorInfo monitorInfo, int cellCount, char splitKey, List<double>? settings=null)
        {

            EAWorkWindow? workWin = FindWorkWindowByMonitorInfo(monitorInfo);
            if (workWin == null)
            {
                return Task.FromResult(false);
            }
            bool res = workWin.SetWorkingSplit(cellCount, splitKey, settings);
            return Task.FromResult(res);


            /*
            Thread thread = new Thread(() =>
            {
                EAWorkWindow? workWin = FindWorkWindowByMonitorInfo(monitorInfo);
                if (workWin == null)
                {
                    return;// Task.FromResult(false);
                }
                bool res = workWin.SetWorkingSplit(cellCount, splitKey, settings);
                System.Windows.Threading.Dispatcher.Run();

            });

            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();

            return Task.FromResult(true);
            */
        }

        public Task<bool> RequestEditSplit(MonitorInfo monitorInfo, int cellCount, char splitKey, string customName, List<double>? settings = null)
        {
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

        public event EventHandler<string> EditCompleted;
        public event EventHandler<string> EditStarted;
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
            //To avoid reenter Edit mode. If we are in Edit mode already, then return false
            if (_editWindow != null)
            {
                return Task.FromResult(false);
            }
            //Check if the monitorInfo contains the monitor

            //Launch the major function in UI Thread
            Thread thread = new Thread(() =>
            {
                UI_EditCommand(monitorInfo, args);
                System.Windows.Threading.Dispatcher.Run();

            });

            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();

            return Task.FromResult(true);
        }
        private bool UI_EditCommand(MonitorInfo monitorInfo, EAArgs args)
        {
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
            //if (workWin != null)
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

            //Create a new EAEditWindow
            if (_editWindow != null)
            {
                _editWindow.Close();
                _editWindow = null;
            }
            EAEditWindow editWin = new EAEditWindow();

            //Register callback for EditReturn from the EditWindow
            //editWin.EditCompleted += (object? sender, string result) =>
            editWin.EditReturn += (object? sender, EAArgs args) =>
            {
                _vmArrange.IsWorkUIEnabled = true;
                _editWindow?.Close();
                _editWindow = null;

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

            if (!editWin.SetInputArg(args, isVertical))
            {
                if (EditStarted != null)
                {
                    EditStarted(this, editWin.LastError);
                }
                return false;
            }

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

            return true;
        }
        #endregion

        #region EA Broker
        /// <summary>
        /// A <Display.DeviceName, WorkWindow> dictionary, each display will allcate a WorkWindow to serve it.
        /// where Display.DeviceName example: "\\.\DISPLAY1" 
        /// </summary>
        private Dictionary<string, EAWorkWindow> _workWindows = new Dictionary<string, EAWorkWindow>();
        private readonly object _eaBrokerLock = new object();
        private bool _isEaBrokerStarted = false;
        private ArrangeVM _vmArrange = new ArrangeVM();
        private EAEditWindow? _editWindow = null;

        public void EABroker_Start()
        {
            lock (_eaBrokerLock) 
            {
                if (!_isEaBrokerStarted)
                {
                    _isEaBrokerStarted = true;
                }
                Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss.fff") + " " + "[EAPlugin] EABroker_Start.");

                Thread thread = new Thread(() =>
                {
                    InitWorkWindows();
                    WinEventHook_Start();
                    System.Windows.Threading.Dispatcher.Run();

                });

                thread.SetApartmentState(ApartmentState.STA);
                thread.Start();

                Microsoft.Win32.SystemEvents.DisplaySettingsChanged += SystemEvents_DisplaySettingsChanged;
            }
        }

        public void EABroker_Stop()
        {
            Thread thread = new Thread(() =>
            {
                Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss.fff") + " " + "[EAPlugin] EABroker_Stop.");

                WinEventHook_Stop();
                ClearWorkWindows();

                System.Windows.Threading.Dispatcher.Run();

            });

            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
        }

        private void InitWorkWindows()
        {
            int isOnlyDellMonitor = Win32Lib.Win32.IniReadInt("DDPMDebug", "EA.IsOnlyDellMonitor", 1, @"C:\temp\DDPMDebug.txt");
            if (!_deviceManagerPluginUsable)
            {
                _log?.Error($"[{pluginName}] @InitWorkWindows, DeviceManager is not usable.");
                return;
            }

            var dpiXProperty = typeof(SystemParameters).GetProperty("DpiX", BindingFlags.NonPublic | BindingFlags.Static);
            var varX = (int)dpiXProperty.GetValue(null, null);
            double dpiX = (double)varX / (double)96;
            Log?.Info($"[{pluginName}] @InitWorkWindows, ScreenDpi={dpiX}");

            List<MonitorInfo> allMonitors = _deviceManagerPlugin.GetMonitors().Result;
            Log?.Info($"[{pluginName}] @InitWorkWindows, Monitors.Count={allMonitors.Count}");
            Log?.Info($"[{pluginName}] @InitWorkWindows, Screens.Count={System.Windows.Forms.Screen.AllScreens.Length}");

            ClearWorkWindows();

            if (allMonitors.Count <= 0) 
            {
                Log?.Info($"[{pluginName}] @InitWorkWindows, No any Monitor found.");
                return;
            }

            int idxScreen = 0;
            foreach (Screen s in System.Windows.Forms.Screen.AllScreens)
            {
                bool isVertical = (s.Bounds.Width < s.Bounds.Height);
                Log?.Info($"[{pluginName}] @InitWorkWindows, Screen[{idxScreen}]:DeviceName=[{s.DeviceName}],WorkArea=[{FormatRectangle(s.WorkingArea)}]");

                //Find all monitors which have the same DeviceName (DisplayName)
                List<MonitorInfo> attachedMonitors = allMonitors.FindAll(x => x.DisplayName.Equals(s.DeviceName, StringComparison.OrdinalIgnoreCase));

                Log?.Info($"[{pluginName}] @InitWorkWindows, Attached Monitors: Count=[{attachedMonitors.Count}]:");
                int dellMoniorCount = 0;
                foreach(MonitorInfo mi in attachedMonitors)
                {
                    Log?.Info($"        [{mi.Index}] Name={mi.AliasDeviceName}, IsDellMonitor=[{mi.IsDellMonitor}]");
                    if (mi.IsDellMonitor)
                        dellMoniorCount++;
                }
                if (dellMoniorCount <= 0)
                {
                    Log?.Info($"[{pluginName}] @InitWorkWindows, No any Dell Monitor attahced at [{s.DeviceName}]");
                    if (isOnlyDellMonitor == 1)
                        continue;
                }

                //Load the settings from the first attached Monitor
                int workCellCount = 0;
                char workSplitKey = 'A';
                List<double>? workSettings = null;

                //Create a WorkWindow work for it
                EAWorkWindow workWin = new EAWorkWindow(_vmArrange);
                workWin.Left = s.WorkingArea.Left / (double)dpiX;
                workWin.Top = s.WorkingArea.Top / (double)dpiX;
                workWin.Width = s.WorkingArea.Width / (double)dpiX;
                workWin.Height = s.WorkingArea.Height / (double)dpiX;
                workWin.SetWorkingSplit(workCellCount, workSplitKey, workSettings);
                workWin.Show();
                _workWindows.Add(s.DeviceName, workWin);

            }
        }

        private void ClearWorkWindows()
        {
            foreach (KeyValuePair<string, EAWorkWindow> keyValuePair in _workWindows)
            {
                keyValuePair.Value.Close();
            }
            _workWindows.Clear();
        }

        private void RefreshWorkWindows()
        {

        }
        #endregion

        #region Window Event Hook
        private WinEventHook _winEventHook = new WinEventHook();

        private void WinEventHook_Start()
        {
            _winEventHook.OnStartMoving += OnWindowStartMovingProc;
            _winEventHook.OnEndMoving += OnWindowEndMovingProc;
            _winEventHook.OnLocationChanged += OnLocationChangedProc;
            _winEventHook.OnForegroundWindowChanged += OnForegroundWindowChangedProc;
            _winEventHook.Hook();
        }

        private void WinEventHook_Stop()
        {
            _winEventHook.Unhook();
            _winEventHook.OnStartMoving -= OnWindowStartMovingProc;
            _winEventHook.OnEndMoving -= OnWindowEndMovingProc;
            _winEventHook.OnLocationChanged -= OnLocationChangedProc;
            _winEventHook.OnForegroundWindowChanged -= OnForegroundWindowChangedProc;
        }

        private void OnForegroundWindowChangedProc(IntPtr hWndNew, IntPtr hWndOld)
        {
            //Noting to do in this project
        }

        private bool _isDebuggingOnWindowStartMoving = true;
        private void OnWindowStartMovingProc(IntPtr hWnd)
        {
            if (_isDebuggingOnWindowStartMoving)
                _log?.Info($"Enter OnWindowStartMovingProc(), hWnd=0x{hWnd:X}");

            if (!_vmArrange.IsFunctionEnabled)
                return;

            Process process;
            string msg;
            if (WinEventHook.GetProcessFromWindowHandle(hWnd, out process, out msg))
            {
                //Try to get the PathName of the process
                try
                {
                    if (process.MainModule != null)
                    {
                        if (!String.IsNullOrEmpty(process.MainModule.FileName))
                        {
                            string pathName = process.MainModule.FileName;
                            if (_isDebuggingOnWindowStartMoving)
                                _log?.Info($"Process.PathName={pathName}");
                        }
                    }
                }
                catch (Exception e1)
                {
                    _log?.Info($"@OnWindowStartMovingProc, access to process causes an exception, msg: {e1.Message}");

                    //Temporary allow to continue moving
                    _vmArrange.IsMoving = true;
                    //Robert_Lin Debug, let it contine
                    //return;
                }
            }
            else
            {
                _log.Info($"@OnWindowStartMovingProc, GetProcessFromWindowHandle error, msg:{msg}");
            }

            var dpiXProperty = typeof(SystemParameters).GetProperty("DpiX", BindingFlags.NonPublic | BindingFlags.Static);
            var varX = (int)dpiXProperty.GetValue(null, null);
            double dpiX = (double)varX / (double)96;

            _vmArrange.ScreenScale = dpiX;
            _vmArrange.IsMoving = true;
            RefreshCellRects();


        }

        private void OnWindowEndMovingProc(IntPtr hWnd, bool isCanceled = false)
        {
            bool isWorkUIShowing = _vmArrange.IsWorkUIShowing;

            if (!_vmArrange.IsMoving)
                return;

            _vmArrange.IsMoving = false;

            if (!isWorkUIShowing)
                return;

            if (_vmArrange.HoveringCellObj == null)
                return;

            //Check if user cancel the window moving by pressing [Esc] key
            //Assumption:
            // When user moving window, the mouse [LeftButton] is pressed and hold.
            // When user canceling the moving, he/she press [Esc] key and the 
            //     mouse [LeftButton] is strll pressed and hold.
            //
            if (WinEventHook.IsUserCancelMoving())
                return;

            Rect rcArrange = _vmArrange.HoveringCellObj.rc;

            //Inflate the rect, because the rcArrange not include the border thickness(=6) of CellBorder
            rcArrange.Inflate(6, 6);
            WinEventHook.SetWindowPosition(hWnd, rcArrange);

        }

        private void OnLocationChangedProc(int x, int y)
        {
            _vmArrange.xCursor = x;
            _vmArrange.yCursor = y;

            if (!_vmArrange.IsWorkUIShowing)
                return;

            CellObj orgCell = _vmArrange.HoveringCellObj;
            _vmArrange.HoveringCellObj = DetermineHoveringCellObj(x, y);

            if (orgCell != _vmArrange.HoveringCellObj)
            {
                string strOrg = "null";
                if (orgCell != null)
                    strOrg = orgCell.Name;
                string strNew = "null";
                if (_vmArrange.HoveringCellObj != null)
                    strNew = _vmArrange.HoveringCellObj.Name;

                //Trace.WriteLine($" * HoveringCell: {strOrg}->{strNew}");
            }
            if (_vmArrange.HoveringCellObj != null)
            {
                _vmArrange.HoveringCell = _vmArrange.HoveringCellObj.Name;
            }
            else
            {
                _vmArrange.HoveringCell = "";
            }
            //if (_workingSplit != null)
            //    _workingSplit.VM.HoveringCell = vm.HoveringCell;

            //Set WorkWins to topmost
        }

        private void RefreshCellRects()
        {
            foreach (KeyValuePair<string, EAWorkWindow> keyValuePair in _workWindows)
            {
                EAWorkWindow workWin = keyValuePair.Value;
                workWin.RefreshCellRects();
            }
        }

        private CellObj? DetermineHoveringCellObj(int x, int y)
        {
            foreach (KeyValuePair<string, EAWorkWindow> keyValuePair in _workWindows)
            {
                EAWorkWindow workWin = keyValuePair.Value;
                CellObj? cellObj = workWin.DetermineHoveringCellObj(x, y);
                if (cellObj != null)
                {
                    return cellObj;
                }
            }
            return null;
        }
        #region GetAsyncKeyState
        private const short VK_ESCAPE = 0x1b;
        private const short VK_LBUTTON = 0x01;

        [DllImport("User32.dll")]
        private static extern short GetAsyncKeyState(System.Int32 vKey);
        #endregion
        #endregion

        #region Helpers
        private EAWorkWindow? FindWorkWindowByMonitorInfo(MonitorInfo monitorInfo)
        {
            EAWorkWindow workWindow = null;
            string key = monitorInfo.DisplayName;
            if (_workWindows.TryGetValue(key, out workWindow))
            {
                return workWindow;
            }
            return null;
        }
        /// <summary>
        /// Format a Rectangle to string, format: "(0,0)-(1920,1200)1920x1200"
        /// </summary>
        /// <param name="rc"></param>
        /// <returns></returns>
        private string FormatRectangle(Rectangle rc)
        {
            return $"({rc.Left},{rc.Top})-({rc.Right},{rc.Bottom}){rc.Width}x{rc.Height}";
        }
        #endregion

        private void SystemEvents_DisplaySettingsChanged(object sender, EventArgs e)
        {

        }

        #region Debug Msg
        public static void Dmsg(string msg)
        {
            Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss.fff") + " " + msg);
        }
        #endregion


        //It should be call once DeviceManagerSA is loaded
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
            PluginIoc.ConfigureServices(services.BuildServiceProvider());
        }
    }

}
