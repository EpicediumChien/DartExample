
using DDPM.SA.Common;
using DDPM.SA.Common.Settings;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.Common.Annotations;
using Dell.Client.Framework.Common.PluginConditions;
using Dell.Client.Framework.Interfaces;
using Microsoft;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using VcpCore.Common;
using IDs = DDPM.SA.Common.IDs;
using Microsoft.WindowsAPICodePack.Shell;
using System.Runtime.InteropServices;
using System.Text;
using System.Diagnostics;
using System.Threading;
using System.Globalization;


namespace DDPM.SA.Plugins.User.EzMemory
{
    [Plugin(IDs.DDPM_EMPlugin_PLUGIN_ID, pluginName, PluginOrderGroupType.Core, Version = pluginVersion)]
    [Descriptor(Description = pluginDescription)]
    [Publisher(Name = publisherCompany, Website = publisherWebsite, Support = publisherSupport)]
    [PublishedUnelevatedInterface(new[] { typeof(IEzMemoryPlugin) })]
    //[PluginRequires(Id = IDs.Display_Manager_PLUGIN_ID, AllowDynamicResolving = true)]
    //[PluginRequires(Id = IDs.DDPM_SETTINGSMANAGER_SA_PLUGIN_ID, AllowDynamicResolving = true)]
    [DependencyKnownTypes(new[] { typeof(IDisplayService), typeof(ISettingsManagerDev) })]

    public class EzMemoryPlugin : BaseAgentPlugin, IDisposableObservable, IEzMemoryPlugin
    {
        public enum log_type
        {
            info = 0,
            error
        }
        /// <summary>
        /// //
        /// </summary>
        /// <param name="text"></param>
        /// <param name="log_type">0 means info, others means error</param>
        private void WriteLog(string text, log_type log_type = log_type.info,
            [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
            [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
            [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0)
        {
            if (string.IsNullOrEmpty(text))
                text = "";

            text = $"[EzMemoryPlugin] {text}, Caller Name:{memberName}, Source Line {sourceLineNumber}";
#if DEBUG
            Console.WriteLine(text);
#endif
            if (Log != null)
            {
                if (log_type == log_type.info)
                    Log.Info(text);
                else
                    Log.Error(text);
            }
        }


        #region DLL
        //For UWP
        [DllImport("user32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);
        private bool EzMemoryEnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam)
        {
            bool rst = EnumWindows(lpEnumFunc, lParam);

            if (!rst)
            {
                _logs.Info($"[EzMemoryPlugin] EzMemoryEnumWindows failed.");

#if DEBUG
                Console.WriteLine("[EzMemoryPlugin] EzMemoryEnumWindows failed.");
#endif
            }

            return rst;
        }

        [DllImport("user32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        private static extern int GetWindowTextLength(IntPtr hWnd);
        private int EzMemoryGetWindowTextLength(IntPtr hWnd)
        {
            int rst = GetWindowTextLength(hWnd);

            if (rst == 0)
            {
                _logs.Info($"[EzMemoryPlugin] GetWindowTextLength: the window has no text.");

#if DEBUG
                Console.WriteLine("[EzMemoryPlugin] GetWindowTextLength: the window has no text.");
#endif
            }

            return rst;
        }

        [DllImport("user32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);
        private int EzMemoryGetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount)
        {
            int rst = GetWindowText(hWnd, lpString, nMaxCount);

            if (rst == 0)
            {
                _logs.Info($"[EzMemoryPlugin] EzMemoryGetWindowText failed.");

#if DEBUG
                Console.WriteLine("[EzMemoryPlugin] EzMemoryGetWindowText failed.");
#endif
            }

            return rst;
        }

        [DllImport("user32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        private static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);
        private int EzMemoryGetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount)
        {
            int rst = GetClassName(hWnd, lpClassName, nMaxCount);

            if (rst == 0)
            {
                _logs.Info($"[EzMemoryPlugin] EzMemoryGetClassName failed.");

#if DEBUG
                Console.WriteLine("[EzMemoryPlugin] EzMemoryGetClassName failed.");
#endif
            }

            return rst;
        }

        //  DPI 
        [DllImport("user32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        private static extern IntPtr MonitorFromWindow(IntPtr hwhWndnd, uint dwFlags);
        private IntPtr EzMemoryMonitorFromWindow(IntPtr hWnd, uint dwFlags)
        {
            IntPtr rst = MonitorFromWindow(hWnd, dwFlags);

            if (rst == IntPtr.Zero)
            {
                _logs.Info($"[EzMemoryPlugin] EzMemoryMonitorFromWindow failed.");

#if DEBUG
                Console.WriteLine("[EzMemoryPlugin] EzMemoryMonitorFromWindow failed.");
#endif
            }

            return rst;
        }

        [DllImport("shcore.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        private static extern int GetDpiForMonitor(IntPtr hmonitor, MONITOR_DPI_TYPE dpiType, out uint dpiX, out uint dpiY);
        private IntPtr EzMemoryGetDpiForMonitor(IntPtr hmonitor, MONITOR_DPI_TYPE dpiType, out uint dpiX, out uint dpiY)
        {
            IntPtr rst = GetDpiForMonitor(hmonitor, dpiType, out dpiX, out dpiY);

            if (rst == IntPtr.Zero)
            {
                _logs.Info($"[EzMemoryPlugin] GetDpiForMonitor failed.");

#if DEBUG
                Console.WriteLine("[EzMemoryPlugin] GetDpiForMonitor failed.");
#endif
            }

            return rst;
        }

        private const uint MONITOR_DEFAULTTONEAREST = 0x00000002;

        private enum MONITOR_DPI_TYPE
        {
            MDT_EFFECTIVE_DPI = 0,
            MDT_ANGULAR_DPI = 1,
            MDT_RAW_DPI = 2,
            MDT_DEFAULT = MDT_EFFECTIVE_DPI
        }
        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }
        private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        const uint SWP_SHOWWINDOW = 0x0040;
        #endregion

        #region Private Members

        private const string pluginName = "EzMemoryManagerPlugin";
        private const string pluginVersion = "1.0.0";
        private const string pluginDescription = "This plugin implements EzMemory Manager Plugin.";
        private const string publisherCompany = "Dell Technologies";
        private const string publisherWebsite = "https://www.dell.com";
        private const string publisherSupport = "This plugin implements EzMemory Manager Plugin.";

        private bool _IsAdministrator = ProcessSecurityHelperWrapper.IsCurrentProcessRunningElevated();
        private static Logs _logs;
        private IAgent _agent;
        public const string PluginLogId = "EzMemoryManager";
        private IDisplayService _DisplayManagerPlugin;
        private ISettingsManagerDev _SettingsPlugin;
        private IDeviceManagerSA _DeviceManagerPlugin;
        private static readonly object _PluginConditionLock_Display = new object();
        private static readonly object _PluginConditionLock_Settings = new object();
        private static List<MonitorInfo> _AllInfoMonitors;
        private static DDPMSettings _DDPMSettings;
        private Timer _EzMemoryTimer;
        private bool startup_Launch_flag = false;
        DDPM.EABroker.EABroker _eaBroker = null;
        #endregion

        #region Constructor

        public EzMemoryPlugin(IAgent agent) : base(agent, PluginLogId)
        {
            _agent = agent;
            _logs ??= new Logs(Log, PluginLogId);
            _logs.DebugMsg_1("[EzMemoryManagerPlugin] constructor ...");
            _logs.DebugMsg("[EzMemoryManagerPlugin] Plugin have Administrator: " + _IsAdministrator.ToString());
            _AllInfoMonitors ??= new List<MonitorInfo>();
            object first_state = null;
            CheckMonitorsAndLaunchApps(first_state);
            _EzMemoryTimer = new Timer(CheckMonitorsAndLaunchApps, null, TimeSpan.Zero, TimeSpan.FromSeconds(60));

            //SaveCustomWindow fff = new SaveCustomWindow(_DeviceManagerPlugin);
            //_eaBroker.NotifySettingsManagerIsInitializedDone();
        }

        #endregion

        #region Overriding methods

        protected override void OnPluginStarting()
        {
            _agent.PluginManager.PluginsStarted += PluginManagerOnPluginsStarted;
            PluginCondition = new PluginStartedCondition();
            InitializeSettingsPlugin();
            InitializeDisplayManagerPlugin();
            InitializeDeviceManagerPlugin();
            _logs.DebugMsg_1("[EzMemoryManagerPlugin] Starting");
        }

        #endregion

        #region Imprement EzMemoryManager

        public Task StopEzMemoryManger()
        {
            _logs.DebugMsg_1("[EzMemoryManagerPlugin] StopEzMemoryManger requested ...");

            // 停止
            _EzMemoryTimer.Change(Timeout.Infinite, Timeout.Infinite);

            return Task.FromResult(Task.CompletedTask);
        }

        public Task StartEzMemoryManger(int millisecond)
        {
            _logs.DebugMsg_1("[EzMemoryManagerPlugin] StartEzMemoryManger: " + millisecond.ToString() + " requested ...");

            _EzMemoryTimer.Change(TimeSpan.Zero, TimeSpan.FromMinutes(millisecond)); // 重新啟動

            return Task.FromResult(Task.CompletedTask);
        }

        public Task DisposeEzMemoryManger()
        {
            _logs.DebugMsg_1("[EzMemoryManagerPlugin] DisposeEzMemoryManger ...");

            _EzMemoryTimer.Dispose();

            return Task.FromResult(Task.CompletedTask);
        }

        #endregion

        #region Private Methods

        private void InitializeEzMemoryPlugin()
        {
            if (_SettingsPlugin != null)
            {
                _logs.DebugMsg_1("[EzMemoryManagerPlugin] InitializeScheduleInfo ...");
            }
        }

        private void InitializeMonitorInfo()
        {
            if (_DisplayManagerPlugin != null)
            {
                _logs.DebugMsg_1("[EzMemoryManagerPlugin] InitializeMonitorInfo ...");

                if (_AllInfoMonitors != null) _AllInfoMonitors.Clear();
                else _AllInfoMonitors = new List<MonitorInfo>();

                _AllInfoMonitors.AddRange(_DisplayManagerPlugin.GetMonitors().Result);

                _logs.DebugMsg_1("[EzMemoryManagerPlugin] _AllInfoMonitors count : " + _AllInfoMonitors.Count);
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

        private void InitializeDeviceManagerPlugin()
        {
            if (_DeviceManagerPlugin != null)
                return;

            _DeviceManagerPlugin = _agent.PluginManager.FindPluginByType<IDeviceManagerSA>(PluginResolution.Dynamic);

            if (_SettingsPlugin is IFrameworkPluginConditionNotification pluginCondition)
            {
                pluginCondition.PluginConditionChangeHandler += OnDeviceManagerPluginConditionChangeHandler;
                GetCurrentDeviceManagerPluginCondition();
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

        private void GetCurrentSettingsPluginCondition()
        {
            _ = Task.Run(async () =>
            {
                var pluginCondition = await (_SettingsPlugin as IFrameworkPluginConditionNotification)?.CurrentConditionAsync();
                lock (_PluginConditionLock_Settings)
                {
                    if (pluginCondition is PluginErrorCondition)
                    {
                        _logs.DebugMsg_1($"{nameof(GetCurrentSettingsPluginCondition)} - Settings Plugin is in an error condition");
                    }
                    else if (pluginCondition is PluginRunningCondition)
                    {
                        _logs.DebugMsg_1($"{nameof(GetCurrentSettingsPluginCondition)} - Settings Plugin is in a running condition");
                    }
                    else if (pluginCondition is PluginStartedCondition)
                    {
                        _logs.DebugMsg_1($"{nameof(GetCurrentSettingsPluginCondition)} - Settings Plugin is in a started condition");
                    }
                }
            });
        }

        private void GetCurrentDeviceManagerPluginCondition()
        {
            _ = Task.Run(async () =>
            {
                var pluginCondition = await (_DeviceManagerPlugin as IFrameworkPluginConditionNotification)?.CurrentConditionAsync();
                lock (_PluginConditionLock_Settings)
                {
                    if (pluginCondition is PluginErrorCondition)
                    {
                        _logs.DebugMsg_1($"{nameof(GetCurrentDeviceManagerPluginCondition)} - Settings Plugin is in an error condition");
                    }
                    else if (pluginCondition is PluginRunningCondition)
                    {
                        _logs.DebugMsg_1($"{nameof(GetCurrentDeviceManagerPluginCondition)} - Settings Plugin is in a running condition");
                    }
                    else if (pluginCondition is PluginStartedCondition)
                    {
                        _logs.DebugMsg_1($"{nameof(GetCurrentDeviceManagerPluginCondition)} - Settings Plugin is in a started condition");
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
                    if (_EzMemoryTimer != null)
                    {
                        _EzMemoryTimer.Dispose();
                        _logs.DebugMsg_1($"Dispose: _EzMemoryTimer Dispose ... ");
                    }
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

        private void OnSettingsPluginConditionChangeHandler(object sender, EventArgs e)
        {
            GetCurrentSettingsPluginCondition();
        }

        private void OnDeviceManagerPluginConditionChangeHandler(object sender, EventArgs e)
        {
            GetCurrentDeviceManagerPluginCondition();
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

            if (e.ChangedPlugins.OfType<IDisplayService>().Any())
                InitializeDisplayManagerPlugin();

            if (e.ChangedPlugins.OfType<IDeviceManagerSA>().Any())
                InitializeDeviceManagerPlugin();

        }

        #endregion

        private void CheckMonitorsAndLaunchApps(object state)//object state
        {
            try
            {
                if (_DisplayManagerPlugin != null)
                {
                    _logs.Info("[EzMemoryManagerPlugin] CheckMonitorsAndLaunchApps ... in");

                    if (_AllInfoMonitors != null) _AllInfoMonitors.Clear();
                    else _AllInfoMonitors = new List<MonitorInfo>();

                    _AllInfoMonitors.AddRange(_DisplayManagerPlugin.GetMonitors().Result);

                    if (_AllInfoMonitors.Count >= 1)
                    {
                        // 對每一個螢幕進行檢查
                        foreach (var monitor in _AllInfoMonitors)
                        {
                            _logs.Info($"[EzMemoryManagerPlugin] CheckMonitorsAndLaunchApps ... Check {monitor.modelName}");
                            CheckAndLaunchForMonitor(monitor);
                        }
                    }

                    _logs.Info("[EzMemoryManagerPlugin] _AllInfoMonitors count : " + _AllInfoMonitors.Count);
                }
            }
            catch (Exception ex)
            {
                //_logs.Error($"[EzMemoryManagerPlugin] CheckMonitorsAndLaunchApps Exception occurred: {ex.Message}");
                WriteLog($"[EzMemoryManagerPlugin] CheckMonitorsAndLaunchApps Exception occurred: {ex.Message}", log_type.error);
            }
        }

        private void CheckAndLaunchForMonitor(MonitorInfo monitorInfo)
        {
            try
            {
                _logs.Info($"[EzMemoryManagerPlugin] CheckAndLaunchForMonitor ... {monitorInfo.modelName} ... in");

                List<DDPMMonitorSettings> monitorSettingsList = _SettingsPlugin.ReloadMonitorSettings(monitorInfo.modelName).Result;

                if (monitorSettingsList != null)
                {
                    var monitorSettings = monitorSettingsList.FirstOrDefault(x => x.ServiceTag == monitorInfo.edid.ServiceTag);

                    if (monitorSettings != null && monitorSettings.easyArrangementDDPM != null)
                    {
                        var easyArrangement = monitorSettings.easyArrangementDDPM;
                        if (easyArrangement.Desktops != null && easyArrangement.Desktops.Count > 0)
                        {
                            foreach (var ps in easyArrangement.Desktops[0].ProfileSettings)
                            {
                                if (ps.Auto)
                                {
                                    _logs.Info($"[EzMemoryManagerPlugin] CheckAndLaunchForMonitor find ps.Auto = {ps.Auto}");
                                    TimeSpan autoStartTime = TimeSpan.FromSeconds(ps.AutoStartTime.Value);
                                    //    Trace.WriteLine("ID = " + ps.ID);
                                    //    Trace.WriteLine("Auto = " + ps.Auto);
                                    //    Trace.WriteLine("AutoStartTime = " + ps.AutoStartTime);
                                    //    Trace.WriteLine("StartUpLaunch = " + ps.StartUpLaunch);
                                    if (IsTimeToLaunch(autoStartTime))
                                    {
                                        _logs.Info($"[EzMemoryManagerPlugin] CheckAndLaunchForMonitor ps.Auto match, MonitorInfo {monitorInfo.modelName} Auto = " + ps.Auto.ToString() + ", StartUpLaunch = " + ps.StartUpLaunch.ToString());
                                        LaunchAndArrangeApps(ps.ID, monitorInfo);
                                        _logs.Info($"[EzMemoryManagerPlugin] CheckAndLaunchForMonitor IsTimeToLaunch, LaunchAndArrangeApps End");
                                    }
                                    else
                                    {
                                        _logs.Info($"[EzMemoryManagerPlugin] CheckAndLaunchForMonitor autoStartTime = {autoStartTime.ToString()}");
                                    }
                                }
                                else
                                {
                                    _logs.Info($"[EzMemoryManagerPlugin] CheckAndLaunchForMonitor find ps.Auto = {ps.Auto}");
                                }

                                if (ps.StartUpLaunch)
                                {
                                    long startupTime = Environment.TickCount64;
                                    if (IsStartupRecently(startupTime))
                                    {
                                        if (startup_Launch_flag == false)
                                        {
                                            _logs.Info($"[EzMemoryManagerPlugin] CheckAndLaunchForMonitor ps.StartUpLaunch match, MonitorInfo {monitorInfo.modelName} " + ", StartupTime : " + startupTime.ToString() + ", StartUpLaunch = " + ps.StartUpLaunch.ToString());
                                            LaunchAndArrangeApps(ps.ID, monitorInfo);
                                            _logs.Info($"[EzMemoryManagerPlugin] CheckAndLaunchForMonitor IsStartupRecently, LaunchAndArrangeApps End");
                                            startup_Launch_flag = true;
                                        }
                                        else
                                        {
                                            _logs.Info($"[EzMemoryManagerPlugin] CheckAndLaunchForMonitor startup_Launch_flag = {startup_Launch_flag.ToString()}");
                                        }
                                    }
                                    else
                                    {
                                        _logs.Info($"[EzMemoryManagerPlugin] CheckAndLaunchForMonitor ps.StartUpLaunch No match, MonitorInfo {monitorInfo.modelName} " + ", StartupTime : " + startupTime.ToString() + ", StartUpLaunch = " + ps.StartUpLaunch.ToString());
                                    }
                                }
                                else
                                {
                                    _logs.Info($"[EzMemoryManagerPlugin] CheckAndLaunchForMonitor find ps.StartUpLaunch = {ps.StartUpLaunch.ToString()}");
                                }
                            }
                        }
                    }
                    else
                    {
                        _logs.Info("[EzMemoryManagerPlugin] CheckAndLaunchForMonitor, monitorSettings == null && monitorSettings.easyArrangementDDPM == null ");
                    }
                }
                else
                {
                    _logs.Info("[EzMemoryManagerPlugin] CheckAndLaunchForMonitor, _AllInfoMonitors count : " + _AllInfoMonitors.Count);
                }
            }
            catch (Exception ex)
            {
                //_logs.Error($"[EzMemoryManagerPlugin] CheckAndLaunchForMonitor Exception occurred: {ex.Message}");
                WriteLog($"[EzMemoryManagerPlugin] CheckAndLaunchForMonitor Exception occurred: {ex.Message}", log_type.error);
            }
        }

        private bool IsTimeToLaunch(TimeSpan autoStartTime)
        {
            var currentTime = DateTime.Now.TimeOfDay;
            _logs.Info("CurrentTime = " + currentTime.Hours + " : " + currentTime.Minutes + " || " + "StartUpLaunch = " + autoStartTime.Hours + " : " + autoStartTime.Minutes);
            return currentTime.Hours == autoStartTime.Hours && currentTime.Minutes == autoStartTime.Minutes;
        }

        private void LaunchAndArrangeApps(int profileId, MonitorInfo monitorInfo)
        {
            try
            {
                DDPMSettings ddpmSettings = _SettingsPlugin.ReloadAppConfigData().Result;
                _logs.Info($"[EzMemoryManagerPlugin] LaunchAndArrangeApps Profile ID: {profileId}");

                if (ddpmSettings != null && ddpmSettings.UserSettings.EAProfile != null)
                {
                    var profile = ddpmSettings.UserSettings.EAProfile.FirstOrDefault(p => p.ID == profileId);

                    if (profile != null)
                    {
                        _logs.Info($"[EzMemoryManagerPlugin] LaunchAndArrangeApps Found profile: {profile.Name} (ID: {profile.ID})");

                        Dictionary<string, Bind_AddFullPage_AppCollectionData> launchApp = new Dictionary<string, Bind_AddFullPage_AppCollectionData>();

                        foreach (var appInfo in profile.AppInfos)
                        {
                            var appData = new Bind_AddFullPage_AppCollectionData
                            {
                                AppPath = appInfo.Path,
                                AppName = appInfo.Name,
                                AppUserModelID = appInfo.AppUserModelID,
                                AppType = appInfo.IsUWP ? "True" : "False"
                            };

                            launchApp.Add(appData.AppName, appData);
                            _logs.Info($"[EzMemoryManagerPlugin] LaunchAndArrangeApps App to launch: {appData.AppName}, Path: {appData.AppPath}, Is UWP: {appData.AppType}");
                        }
                        //bool result = true;
                        //bool result = LaunchAndArrangeApps(launchApp).Result;
                        bool result = _DisplayManagerPlugin.LaunchAndArrangeAppsWithEzArrange(launchApp, monitorInfo, profile.Layout).Result;//LaunchAndArrangeAppsWithEzArrange(launchApp, monitorInfo, profile.Layout).Result;
                        if (result)
                        {
                            _logs.Info($"[EzMemoryManagerPlugin] LaunchAndArrangeApps Apps launched and arranged successfully for profile: {profile.Name}");
                        }
                        else
                        {
                            _logs.Error($"[EzMemoryManagerPlugin] LaunchAndArrangeApps Failed to launch and arrange apps for profile: {profile.Name}");
                        }
                    }
                    else
                    {
                        _logs.Error($"[EzMemoryManagerPlugin] LaunchAndArrangeApps No profile found with ID: {profileId}");
                    }
                }
                else
                {
                    _logs.Error("[EzMemoryManagerPlugin] LaunchAndArrangeApps DDPM settings or EAProfile is null");
                }
            }
            catch (Exception ex)
            {
                //_logs.Error($"[EzMemoryManagerPlugin] LaunchAndArrangeApps Exception occurred: {ex.Message}");
                WriteLog($"[EzMemoryManagerPlugin] DeleteEAID, Exception  Error: {ex.Message}", log_type.error);
            }
        }
        private bool IsStartupRecently(long startupTime)
        {
            // 5分鐘內定義為"剛啟動"狀態
            long oneMinuteInMilliseconds = 300000;
            return startupTime < oneMinuteInMilliseconds;
        }

        public Task<Dictionary<string, InstalledAppInfo>> GetAllAppList()
        {
            AppsCollectShell appshell = new AppsCollectShell(Log);
            string RootColorPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Dell\\Dell Display and Peripheral Manager\\AppLibrary";
            string IconFolder = RootColorPath + "\\Icons\\";

            /* KNOWNFOLDERID: From MSDN https://learn.microsoft.com/en-us/windows/win32/shell/knownfolderid */
            Guid FOLDERID_AppsFolder = new Guid("1e87508d-89c2-42f0-8a7e-645a0f50ca58");

            AppListDictionary tmpAppListDictionary = AppListDictionary.GetInstance();

            Dictionary<string, InstalledAppInfo> installedApp = new Dictionary<string, InstalledAppInfo>();
            string fileinfo = string.Empty, info = string.Empty;
            bool canSave = true;
            if (!System.IO.Directory.Exists(RootColorPath))
            {
                System.IO.Directory.CreateDirectory(RootColorPath);
                _logs.Info($"[EzMemoryManagerPlugin][ValidateFilePath] RootColorPath isn't exist, create it");
            }
            if (canSave && !DDPMFileSecurity.ValidateFilePath(RootColorPath, out info))
            {
                _logs.Info($"[EzMemoryManagerPlugin][ValidateFilePath] RootColorPath abnormal, do not save icon: {info}");
                canSave = false;
            }
            if (canSave && !System.IO.Directory.Exists(IconFolder))
            {
                System.IO.Directory.CreateDirectory(IconFolder);
                _logs.Info($"[EzMemoryManagerPlugin][ValidateFilePath] IconFolder isn't exist, create it");
            }
            if (canSave && !DDPMFileSecurity.ValidateFilePath(IconFolder, out info))
            {
                _logs.Info($"[EzMemoryManagerPlugin][ValidateFilePath] IconFolder abnormal, do not save icon: {info}");
                canSave = false;
            }

            Dictionary<string, List<AppItemInfo>> dictionary = new Dictionary<string, List<AppItemInfo>>();
            IKnownFolder ikf = null;
            try
            {
                ikf = KnownFolderHelper.FromKnownFolderId(FOLDERID_AppsFolder);
            }
            catch (Exception ex)
            {
                //_logs.Info($"[EzMemoryManagerPlugin], FromKnownFolderId Exception {ex}");
                WriteLog($"[EzMemoryManagerPlugin], FromKnownFolderId Exception {ex.Message}", log_type.error);
                return Task.FromResult(installedApp);
            }
            if (ikf == null)
            {
                _logs.Info($"[EzMemoryManagerPlugin], ikf nul");
                return Task.FromResult(installedApp);
            }

            foreach (ShellObject item in (IKnownFolder)(ShellObject)ikf)
            {
                string name = string.IsNullOrEmpty(item.Name) ? string.Empty : item.Name;
                string parsingName = string.IsNullOrEmpty(item.ParsingName) ? string.Empty : item.ParsingName;
                string value = string.Empty;// item.Properties.System.Link.TargetParsingPath.Value;
                string value2 = string.Empty;// item.Properties.System.Link.Arguments.Value;
                try
                {
                    value = item.Properties.System.Link.TargetParsingPath.Value;
                    value2 = item.Properties.System.Link.Arguments.Value;
                }
                catch (Exception ex)// ex)
                {
                    //_logs.Info($"[EzMemoryManagerPlugin], value and value2 {ex}");
                    WriteLog($"[EzMemoryManagerPlugin], value and value2 {ex.Message}", log_type.error);
                }

                //
                // Desktop application parsing
                //
                if (value != null && value.Length > 0)
                {
                    value = value.ToLower(CultureInfo.InvariantCulture);
                    string text = value.Split('\\')[^1].ToLower(CultureInfo.InvariantCulture);
                    if (!text.ToLower(CultureInfo.InvariantCulture).Contains("exe"))
                    {
                        continue;
                    }
                    try
                    {
                        System.IO.FileInfo f = new System.IO.FileInfo(value);
                        DateTime lastAccessTime = f.CreationTime;//.LastAccessTime;
                        if (!File.Exists(IconFolder + text + ".png"))
                        {
                            if (canSave)
                                System.Drawing.Icon.ExtractAssociatedIcon(value)!.ToBitmap().Save(IconFolder + text + ".png");
                        }
                        if (!dictionary.ContainsKey(value))
                        {
                            dictionary.Add(value, new List<AppItemInfo>
                                                    {
                                                        new AppItemInfo
                                                        {
                                                            AppName = name,
                                                            AppExeName = text,
                                                            InstalledDate = lastAccessTime,
                                                            PathArgument = value2,
                                                            AppUserModelID = parsingName
                                                        }
                                                    }
                            );
                            //Console.WriteLine("Desktop01******************** " + name.ToString() + " || " + text.ToString() + " || " + value.ToString());
                        }
                        else
                        {
                            dictionary[value].Add(new AppItemInfo
                            {
                                AppName = name,
                                AppExeName = text,
                                InstalledDate = lastAccessTime,
                                PathArgument = value2,
                                AppUserModelID = parsingName
                            });
                            //Console.WriteLine("Desktop02******************** " + name.ToString() + " || " + text.ToString() + " || " + value.ToString());
                        }
                    }
                    catch (Exception ex)
                    {
                        //_logs.Info($"[EzMemoryManagerPlugin], Desktop application parsing Exception {ex}");
                        WriteLog($"[EzMemoryManagerPlugin], Desktop application parsing Exception {ex.Message}", log_type.error);
                    }
                    continue;
                }
                //else
                //{
                //    //_logs.Info($"[EzMemoryManagerPlugin], not desktop app");
                //}
                //
                // UWP application parsing
                //
                Bitmap bitmap = null;
                try
                {
                    string filename = name.ToLower(CultureInfo.InvariantCulture);
                    filename = CheckFileNameValid(filename);
                    string text2 = item.Properties.GetProperty("System.AppUserModel.PackageInstallPath")?.ValueAsObject?.ToString();
                    bool installed_uwp = false;

                    if (!File.GetAttributes(text2).HasFlag(FileAttributes.Directory) || installed_uwp || Directory.GetFiles(text2, "*.exe").Length != 0)
                    {
                        DateTime now = DateTime.Now;
                        //Find uwp app installed date

                        System.Windows.Media.Imaging.BitmapSource bitmapSource = item.Thumbnail.ExtraLargeBitmapSource;
                        bitmap = new Bitmap(bitmapSource.PixelWidth, bitmapSource.PixelHeight, PixelFormat.Format32bppPArgb);
                        BitmapData bitmapData = bitmap.LockBits(new Rectangle(System.Drawing.Point.Empty, bitmap.Size), ImageLockMode.WriteOnly, PixelFormat.Format32bppPArgb);
                        bitmapSource.CopyPixels(Int32Rect.Empty, bitmapData.Scan0, bitmapData.Height * bitmapData.Stride, bitmapData.Stride);
                        bitmap.UnlockBits(bitmapData);
                        if (!File.Exists(IconFolder + filename + ".png"))
                        {
                            if (canSave)
                                bitmap.Save(IconFolder + filename + ".png");
                        }
                        if (!installedApp.ContainsKey(text2))
                        {
                            installedApp.Add(text2, new InstalledAppInfo(name, text2, filename, now, bDesktopApp: false, parsingName));
                            //Console.WriteLine("UWP0******************** " + name.ToString() + " || " + text2.ToString() + " || " + filename.ToString());
                        }
                    }
                }
                catch (Exception ex)
                {
                    //_logs.Info($"[EzMemoryManagerPlugin], UWP Exception {ex}");
                    WriteLog($"[EzMemoryManagerPlugin], UWP Exception {ex.Message}", log_type.error);

                }
                finally
                {
                    bitmap?.Dispose();
                }
            }
            try
            {
                foreach (KeyValuePair<string, List<AppItemInfo>> item2 in dictionary)
                {
                    List<AppItemInfo> value3 = item2.Value;
                    if (value3.Count == 1)
                    {
                        if (!installedApp.ContainsKey(item2.Key))
                        {
                            installedApp.Add(item2.Key, new InstalledAppInfo(value3[0].AppName, item2.Key, value3[0].AppExeName, value3[0].InstalledDate, bDesktopApp: true, value3[0].AppUserModelID));
                            //Console.WriteLine("UWP1******************** " + value3[0].AppName.ToString() +  " || " + item2.Key.ToString() + " || " + value3[0].AppExeName.ToString());
                        }
                    }
                    else
                    {
                        if (value3.Count < 1)
                        {
                            continue;
                        }
                        List<AppItemInfo> list = value3.FindAll((AppItemInfo x) => string.IsNullOrEmpty(x.PathArgument));
                        if (list.Count >= 1 && !installedApp.ContainsKey(item2.Key))
                        {
                            installedApp.Add(item2.Key, new InstalledAppInfo(list[0].AppName, item2.Key, list[0].AppExeName, list[0].InstalledDate, bDesktopApp: true, value3[0].AppUserModelID));
                            continue;
                        }
                        list = value3.FindAll((AppItemInfo x) => !x.PathArgument.Contains("url"));
                        if (list.Count > 0 && !installedApp.ContainsKey(item2.Key))
                        {
                            installedApp.Add(item2.Key, new InstalledAppInfo(list[0].AppName, item2.Key, list[0].AppExeName, list[0].InstalledDate, bDesktopApp: true, value3[0].AppUserModelID));
                            //Console.WriteLine("UWP2******************** " + value3[0].AppName.ToString() + " || " + item2.Key.ToString() + " || " + value3[0].AppExeName.ToString());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //_logs.Info($"[EzMemoryManagerPlugin], KeyValuePair<string, List<AppItemInfo>> Exception {ex}");
                WriteLog($"[EzMemoryManagerPlugin], KeyValuePair<string, List<AppItemInfo>> Exception {ex.Message}", log_type.error);
            }
            AppListDictionary.GetInstance().LoadFile();
            tmpAppListDictionary = AppListDictionary.GetInstance();
            foreach (string key in installedApp.Keys)
            {
                if (!tmpAppListDictionary.AppInstallsList.ContainsKey(key))
                {
                    tmpAppListDictionary.AppInstallsList.Add(key, installedApp[key]);
                    //Console.WriteLine("3******************** " + key.ToString() + " || " + installedApp[key].ToString());
                }
            }
            foreach (string key2 in tmpAppListDictionary.AppInstallsList.Keys)
            {
                if (installedApp.ContainsKey(key2))
                {
                    continue;
                }
                try
                {
                    if (tmpAppListDictionary.AppInstallsList[key2].isDesktopApp)
                    {
                        if (!File.Exists(key2))
                        {
                            tmpAppListDictionary.AppInstallsList.Remove(key2);
                        }
                    }
                    else if (!Directory.Exists(key2))
                    {
                        tmpAppListDictionary.AppInstallsList.Remove(key2);
                    }
                }
                catch (Exception ex)
                {
                    //_logs.Error($"[EzMemoryManagerPlugin] tmpAppListDictionary.AppInstallsList.Keys Exception occurred: {ex.Message}");
                    WriteLog($"[EzMemoryManagerPlugin] tmpAppListDictionary.AppInstallsList.Keys Exception occurred: {ex.Message}", log_type.error);
                }
            }
            tmpAppListDictionary.SaveInstalledAppInfo_Thread();

            return Task.FromResult(installedApp);
        }
        private string CheckFileNameValid(string filename)
        {
            if (filename.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            {
                filename = filename.Replace("//", "");
                filename = filename.Replace("\\", "");
                filename = filename.Replace(":", "");
                filename = filename.Replace("*", "");
                filename = filename.Replace("?", "");
                filename = filename.Replace("\"", "");
                filename = filename.Replace(">", "");
                filename = filename.Replace("<", "");
                filename = filename.Replace("|", "");
            }
            return filename;
        }

        /// <summary>
        /// Test with EzArrange
        /// </summary>
        /// <param name="moInfo"></param>
        /// <param name="eAID"></param>
        /// <returns>Return 0 & 1 mean EzArrange busy, must break； Return > 1 mean EzArrange need Handl count</returns>
        //private int LaunchStart(MonitorInfo moInfo, int eAID)
        //{
        //    return 2;
        //}

        /// <summary>
        /// Test with EzArrange
        /// </summary>
        /// <param name="moInfo"></param>
        /// <param name="handle"></param>
        /// <param name="no"></param>
        /// <returns>Send handle to EzArrange, EzArrange will arrange app location</returns>
        //private bool HandleArrange(MonitorInfo moInfo, IntPtr handle, int no)
        //{
        //    return true;
        //}

        /// <summary>
        /// Test with EzArrange
        /// </summary>
        /// <returns>Notify EzArrange stop and end.</returns>
        //private bool LaunchEnd()
        //{
        //    return true;
        //}

        //public async Task<bool> LaunchAndArrangeAppsWithEzArrange(Dictionary<String, Bind_AddFullPage_AppCollectionData> sortApps, MonitorInfo moInfo, int eAid)
        //{
        //    try
        //    {
        //        // Check EAID count
        //        int handleCount = LaunchStart(moInfo, eAid);
        //        if (handleCount == 0 || handleCount == 1)
        //        {
        //            _logs.Info("[EzMemoryManagerPlugin] LaunchAndArrangeAppsWithEzArrange, EAID return 0/1. Task aborted.");
        //            return false;
        //        }

        //        // Check applications list
        //        int appCount = sortApps.Count;
        //        if (appCount == 0)
        //        {
        //            _logs.Info("[EzMemoryManagerPlugin] LaunchAndArrangeAppsWithEzArrange, No apps to launch and arrange.");
        //            return false;
        //        }

        //        var sortedApps = sortApps.OrderBy(x => x.Key).Select(x => x.Value).ToList();

        //        for (int i = 0; i < appCount; i++)
        //        {
        //            var app = sortedApps[i];
        //            IntPtr handle = IntPtr.Zero;

        //            // Get handle
        //            Process[] processes = GetProcessesByName(app);
        //            if (processes.Length > 0)
        //            {
        //                handle = processes[0].MainWindowHandle;
        //                EzMemorySetForegroundWindow(handle);
        //            }
        //            else
        //            {
        //                Process process = LaunchApp(app);
        //                if (process == null)
        //                {
        //                    _logs.Error($"[EzMemoryManagerPlugin] LaunchAndArrangeAppsWithEzArrange, Failed to launch app: {app.AppName}");
        //                    continue;
        //                }

        //                // Wait app window initialize
        //                for (int attempt = 0; attempt < 10; attempt++)
        //                {
        //                    handle = app.AppType == "True" ? process.MainWindowHandle : GetWindowHandle(app);
        //                    if (handle != IntPtr.Zero)
        //                        break;

        //                    await Task.Delay(500);
        //                }

        //                if (handle == IntPtr.Zero)
        //                {
        //                    _logs.Error($"[EzMemoryManagerPlugin] LaunchAndArrangeAppsWithEzArrange, App {app.AppName} failed to get window handle after launch.");
        //                    continue;
        //                }
        //            }

        //            // Arrange handle with HandleArrange
        //            bool arrangeResult = HandleArrange(moInfo, handle, i);
        //            if (!arrangeResult)
        //            {
        //                _logs.Error($"[EzMemoryManagerPlugin] LaunchAndArrangeAppsWithEzArrange, Arrangement failed for app {app.AppName}. Task stopped.");
        //                return false;
        //            }

        //            _logs.Info($"[EzMemoryManagerPlugin] LaunchAndArrangeAppsWithEzArrange, App {app.AppName} arranged successfully.");
        //        }

        //        // End
        //        LaunchEnd();
        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        _logs.Error($"[EzMemoryManagerPlugin] LaunchAndArrangeAppsWithEzArrange, Unexpected error: {ex}");
        //        return false;
        //    }
        //}


        //public Task<bool> LaunchAndArrangeApps(Dictionary<String, Bind_AddFullPage_AppCollectionData> sortApps)
        //{
        //    try
        //    {
        //        int appCount = sortApps.Count;
        //        if (appCount == 0)
        //        {
        //            _logs.Info("[EzMemoryManagerPlugin] LaunchAndArrangeApps, No apps to launch and arrange.");
        //            return Task.FromResult(false);
        //        }


        //        List<Bind_AddFullPage_AppCollectionData> seletcApps = new List<Bind_AddFullPage_AppCollectionData>();
        //        double screenWidth = SystemParameters.PrimaryScreenWidth;
        //        double screenHeight = SystemParameters.PrimaryScreenHeight;
        //        double widthPerApp = screenWidth / appCount; // 平均分配寬度

        //        var sortedByKey = sortApps.OrderBy(x => x.Key).ToList();
        //        seletcApps = sortedByKey.Select(x => x.Value).ToList();

        //        //Task.Run(async () =>
        //        //{
        //            EzMemLauncher ezMemLauncher = new EzMemLauncher();
        //            MonitorInfo mi = _AllInfoMonitors[0];
        //            int eaId = 9;
        //            ezMemLauncher.LaunchStart(mi, eaId);

        //            List<IntPtr> windowHandles = new List<IntPtr>();

        //            //Robert_Lin, add to make sure all opened windows has been arranged
        //            int arrangeCount = 0;
        //            int addCount = 0;

        //            for (int i = 0; i < appCount; i++)
        //            {
        //                var app = seletcApps[i];
        //                IntPtr handle = IntPtr.Zero;

        //                try
        //                {
        //                    // 檢查應用程式是否已經存在
        //                    Process[] processes = GetProcessesByName(app);
        //                    _logs.Info($"[EzMemoryManagerPlugin] LaunchAndArrangeApps, GetProcessesByName(app): {app.AppName}");

        //                    if (processes.Length > 0)
        //                    {
        //                        handle = processes[0].MainWindowHandle;
        //                        _logs.Info($"[EzMemoryManagerPlugin] LaunchAndArrangeApps, App {app.AppName} is already running, handle: {handle}");
        //                        EzMemorySetForegroundWindow(handle); // 把應用程式拉到前景
        //                    }
        //                    else
        //                    {
        //                        Process process = LaunchApp(app);

        //                        if (process == null)
        //                        {
        //                            _logs.Error($"[EzMemoryManagerPlugin] LaunchAndArrangeApps, Failed to launch app: {app.AppName}");
        //                            continue;
        //                        }

        //                        // 等待應用程式的窗口初始化
        //                        for (int attempt = 0; attempt < 10; attempt++)
        //                        {
        //                            handle = app.AppType == "True" ? process.MainWindowHandle : GetWindowHandle(app);

        //                            if (handle != IntPtr.Zero && !windowHandles.Contains(handle))
        //                                break;

        //                            Task.Delay(500);
        //                        }

        //                        if (handle == IntPtr.Zero)
        //                        {
        //                            _logs.Error($"[EzMemoryManagerPlugin] LaunchAndArrangeApps, App {app.AppName} failed to get window handle after launch.");
        //                            continue;
        //                        }
        //                    }

        //                     // 取得視窗的 DPI 設定
        //                    float dpiScale = GetDpiScaleForWindow(handle);

        //                    /*
        //                   // 調整視窗位置與大小，考慮 DPI 比例
        //                    EzMemorySetWindowPos(handle, IntPtr.Zero,
        //                        (int)((i * widthPerApp) * dpiScale),
        //                        0,
        //                        (int)(widthPerApp * dpiScale),
        //                        (int)(screenHeight * dpiScale),
        //                        SWP_SHOWWINDOW);
        //                    */

        //                    ezMemLauncher.ArrangeWindow(handle, i);
        //                    arrangeCount++;


        //                    // 確認視窗是否已移動到預期的位置
        //                    //for (int checkAttempt = 0; checkAttempt < 10; checkAttempt++)
        //                    //{
        //                    //    if (EzMemoryGetWindowRect(handle, out RECT rect))
        //                    //    {
        //                    //        if (rect.Left == (int)((i * widthPerApp) * dpiScale) && rect.Top == 0 &&
        //                    //            rect.Right == (int)(((i + 1) * widthPerApp) * dpiScale) && rect.Bottom == (int)(screenHeight * dpiScale))
        //                    //        {
        //                    //            _logs.Info($"[EzMemoryManagerPlugin] LaunchAndArrangeApps, App {app.AppName} positioned correctly.");
        //                    //            break;
        //                    //        }
        //                    //    }

        //                        //Task.Delay(500);
        //                    //}

        //                    Task.Delay(1000);

        //                }
        //                catch (Exception ex)
        //                {
        //                    _logs.Error($"[EzMemoryManagerPlugin] LaunchAndArrangeApps, Error arranging app {app.AppName}: {ex}");
        //                }


        //            } //for

        //            //Robert_Lin, 2024-11-27, Wait until all opened windows are arranged
        //            while (arrangeCount < appCount)
        //            {
        //                Task.Delay(100);
        //            }
        //            ezMemLauncher.LaunchEnd();
        //        //});

        //        return Task.FromResult(true);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logs.Error($"[EzMemoryManagerPlugin] LaunchAndArrangeApps, Unexpected error: {ex}");
        //        return Task.FromResult(false);
        //    }
        //}

        /*private static string GetExecutablePath(string executableName, out string info)
        {
            string path = string.Empty;
            foreach (var process in Process.GetProcessesByName(executableName))
            {
                try
                {
                    info = "success";
                    return process.MainModule.FileName;
                }
                catch(Exception e)
                {
                    info = $"failed with: {e.Message}";
                }
            }
            info = "Can't find specific file";
            return string.Empty;
        }*/

        private Process LaunchApp(Bind_AddFullPage_AppCollectionData appData)
        {
            Process process = null;
            try
            {
                string info = string.Empty;
                string filePath = (appData.AppType == "False") ? "explorer.exe" : appData.AppPath;
                string argument = string.Empty;
                string argument_sanitized = string.Empty;
                string filePath_sanitized = string.Empty;
                if (appData.AppType == "False")
                {
                    filePath_sanitized = DDPMFileSecurity.SanitizePath(filePath, out info);
                    if (!string.IsNullOrEmpty(filePath_sanitized))//add path check for checkmarx issue fix, Dean 1211
                    {
                        argument = $"shell:AppsFolder\\{appData.AppUserModelID}";
                        argument_sanitized = DDPMFileSecurity.SanitizePath(argument, out info);
                        if (!string.IsNullOrEmpty(argument_sanitized))
                        {
                            // UWP 應用程式
                            ProcessStartInfo startInfo = new ProcessStartInfo
                            {
                                FileName = filePath_sanitized,
                                Arguments = argument_sanitized,
                                UseShellExecute = true
                            };
                            _logs.Info($"[EzMemoryManagerPlugin] LaunchApp, Launching UWP app: {appData.AppName}");
                            process = Process.Start(startInfo);
                        }
                        else
                            throw new Exception($"argument SanitizePath check return empty: {argument}, {info}");
                    }
                    else
                        throw new Exception($"filePath SanitizePath check return empty: {filePath}, {info}");
                }
                else
                {
                    // Desktop exe或檔案
                    filePath_sanitized = DDPMFileSecurity.SanitizePath(filePath, out info);
                    if (!string.IsNullOrEmpty(filePath_sanitized))//add path check for checkmarx issue fix, Dean 1211
                    {
                        if (DDPMFileSecurity.ValidateFilePath(filePath_sanitized, out info)) //add path check for checkmarx issue fix, Dean 1208
                        {
                            ProcessStartInfo startInfo = new ProcessStartInfo
                            {
                                FileName = filePath_sanitized,
                                UseShellExecute = true,  // 系統自動選擇應用程式來開啟
                                Verb = "open"            // 指定開啟檔案的動作
                            };
                            _logs.Info($"[EzMemoryManagerPlugin] LaunchApp, Launching desktop app or file: {appData.AppName}");
                            process = Process.Start(startInfo);
                        }
                        else
                            throw new Exception($"filePath validation return fail: {filePath_sanitized}, {info}");
                    }
                    else
                        throw new Exception($"filePath SanitizePath check return empty: {filePath}, {info}");
                }

                if (process != null)
                {
                    if (!appData.AppPath.EndsWith(".png") && !appData.AppPath.EndsWith(".jpg") && !appData.AppPath.EndsWith(".txt"))
                    {
                        process.WaitForInputIdle();
                        _logs.Info($"[EzMemoryManagerPlugin] LaunchApp, App {appData.AppName} is now idle.");
                    }
                }
                else
                {
                    _logs.Error($"[EzMemoryManagerPlugin] LaunchApp, Failed to launch app or file: {appData.AppName}");
                }
            }
            catch (Exception ex)
            {
                //_logs.Error($"[EzMemoryManagerPlugin] LaunchApp, Failed while launching app or file: {appData.AppName}, Error: {ex}");
                WriteLog($"[EzMemoryManagerPlugin] LaunchApp, Failed while launching app or file: {appData.AppName}, Error: {ex.Message}", log_type.error);

            }

            return process;
        }

        private IntPtr GetWindowHandle(Bind_AddFullPage_AppCollectionData appData)
        {
            IntPtr windowHandle = IntPtr.Zero;

            try
            {
                EzMemoryEnumWindows((hWnd, lParam) =>
                {
                    int length = EzMemoryGetWindowTextLength(hWnd);
                    if (length == 0) return true;

                    StringBuilder windowName = new StringBuilder(length);
                    EzMemoryGetWindowText(hWnd, windowName, length + 1);

                    if (appData.AppType == "False")
                    {
                        string className = GetWindowClassName(hWnd);
                        if (className.Contains("ApplicationFrameWindow"))
                        {
                            windowHandle = hWnd;
                            _logs.Info($"[EzMemoryManagerPlugin] GetWindowHandle, Found window for {appData.AppName}, handle: {windowHandle}");
                            return false;
                        }
                    }

                    return true;
                }, IntPtr.Zero);

                if (windowHandle == IntPtr.Zero)
                {
                    _logs.Error($"[EzMemoryManagerPlugin] GetWindowHandle, Failed to get window handle for {appData.AppName}");
                }
            }
            catch (Exception ex)
            {
                //_logs.Error($"[EzMemoryManagerPlugin] GetWindowHandle, Exception while retrieving window handle for {appData.AppName}, Error: {ex}");
                WriteLog($"[EzMemoryManagerPlugin] GetWindowHandle, Exception while retrieving window handle for {appData.AppName}, Error: {ex}", log_type.error);

            }

            return windowHandle;
        }

        private Process[] GetProcessesByName(Bind_AddFullPage_AppCollectionData appData)
        {
            Process[] processes = Array.Empty<Process>();
            try
            {
                if (appData.AppType == "False")
                {
                    // UWP 
                    processes = Process.GetProcessesByName(appData.AppUserModelID);
                    _logs.Info($"[EzMemoryManagerPlugin] GetProcessesByName, UWP app {appData.AppName} process count: {processes.Length}");
                }
                else
                {
                    // Desktop
                    processes = Process.GetProcessesByName(System.IO.Path.GetFileNameWithoutExtension(appData.AppPath));
                    _logs.Info($"[EzMemoryManagerPlugin] GetProcessesByName, Desktop app {appData.AppName} process count: {processes.Length}");
                }
            }
            catch (Exception ex)
            {
                //_logs.Error($"[EzMemoryManagerPlugin] GetProcessesByName, Exception while getting processes for {appData.AppName}, Error: {ex}");
                WriteLog($"[EzMemoryManagerPlugin] GetProcessesByName, Exception while getting processes for {appData.AppName}, Error: {ex.Message}", log_type.error);
            }

            return processes;
        }

        private float GetDpiScaleForWindow(IntPtr hWnd)
        {
            float dpiScale = 1.0f; // Default DPI scaling is 1.0 (100%)
            try
            {
                IntPtr monitor = EzMemoryMonitorFromWindow(hWnd, MONITOR_DEFAULTTONEAREST);
                if (monitor != IntPtr.Zero)
                {
                    uint dpiX, dpiY;
                    if (EzMemoryGetDpiForMonitor(monitor, MONITOR_DPI_TYPE.MDT_EFFECTIVE_DPI, out dpiX, out dpiY) == 0)
                    {
                        dpiScale = dpiX / 96.0f; // 96 DPI is the default 100% scaling
                        _logs.Info($"[EzMemoryManagerPlugin] GetDpiScaleForWindow, DPI scaling for window: {dpiScale}");
                    }
                }
            }
            catch (Exception ex)
            {
                //_logs.Error($"[EzMemoryManagerPlugin] GetDpiScaleForWindow, Exception while getting DPI scale for window, Error: {ex}");
                WriteLog($"[EzMemoryManagerPlugin] GetDpiScaleForWindow, Exception while getting DPI scale for window, Error: {ex.Message}", log_type.error);
            }
            return dpiScale;
        }

        private string GetWindowClassName(IntPtr hWnd)
        {
            StringBuilder className = new StringBuilder(256);
            try
            {
                EzMemoryGetClassName(hWnd, className, className.Capacity);
                _logs.Info($"[EzMemoryManagerPlugin] GetWindowClassName, Window class name: {className}");
            }
            catch (Exception ex)
            {
                //_logs.Error($"[EzMemoryManagerPlugin] GetWindowClassName, Exception while getting window class name, Error: {ex}");
                WriteLog($"[EzMemoryManagerPlugin] GetWindowClassName, Exception while getting window class name, Error: {ex.Message}", log_type.error);
            }
            return className.ToString();
        }

        public Task<bool> CheckEAIDExit(MonitorInfo moinfo, int eAID)
        {
            bool exists = false;
            try
            {
                DDPMSettings ddpmSettings = _SettingsPlugin.ReloadAppConfigData().Result;
                if (ddpmSettings != null)
                {
                    List<EAProfileDDPM> checkEAID = ddpmSettings.UserSettings.EAProfile;
                    //Robert_Lin 2025-3-4, when no any EAProfile in the Computer,EAProfile will be null.
                    //OLD:
                    // if (checkEAID.Count > 0)
                    //NEW:
                    if ((checkEAID != null) && (checkEAID.Count > 0))
                    {
                        exists = checkEAID.Any(profile => profile.Layout == eAID);
                        _logs.Info($"[EzMemoryManagerPlugin] CheckEAIDExit Success");
                    }
                    return Task.FromResult(exists);
                }
                else
                {
                    return Task.FromResult(exists);
                }
            }
            catch (Exception ex)
            {
                //_logs.Error($"[EzMemoryManagerPlugin] CheckEAIDExit, Exception  Error: {ex}");
                WriteLog($"[EzMemoryManagerPlugin] CheckEAIDExit, Exception  Error: {ex.Message}", log_type.error);
            }
            return Task.FromResult(exists);
        }

        public Task<bool> DeleteEAID(MonitorInfo moinfo, int eAID)
        {
            bool result = false;
            try
            {
                DDPMSettings ddpmSettings = _SettingsPlugin.ReloadAppConfigData().Result;
                if (ddpmSettings != null)
                {
                    List<EAProfileDDPM> delEAID = ddpmSettings.UserSettings.EAProfile;
                    if (delEAID.Count > 0)
                    {
                        result = delEAID.RemoveAll(profile => profile.Layout == eAID) > 0;
                        _DeviceManagerPlugin.WriteUserListEAProfileDDPM(delEAID);
                        _logs.Info($"[EzMemoryManagerPlugin] DeleteEAID Success");
                    }
                    return Task.FromResult(result);
                }
                else
                {
                    return Task.FromResult(result);
                }
            }
            catch (Exception ex)
            {
                //_logs.Error($"[EzMemoryManagerPlugin] DeleteEAID, Exception  Error: {ex}");
                WriteLog($"[EzMemoryManagerPlugin] DeleteEAID, Exception  Error: {ex.Message}", log_type.error);
            }
            return Task.FromResult(result);
        }
    }
}