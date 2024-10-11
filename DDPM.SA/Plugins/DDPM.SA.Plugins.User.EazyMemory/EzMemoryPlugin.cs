
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
using System.Windows.Input;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.VisualBasic.Logging;
using System.Diagnostics;
using DPeMPublic.Common;
using System.Threading;

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
        #region DLL
        //For UWP
        [DllImport("user32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);
        private bool EzMemoryEnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam)
        {
            return EnumWindows(lpEnumFunc, lParam);
        }

        [DllImport("user32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        static extern int GetWindowTextLength(IntPtr hWnd);
        private int EzMemoryGetWindowTextLength(IntPtr hWnd)
        {
            return GetWindowTextLength(hWnd);
        }

        [DllImport("user32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);
        private int EzMemoryGetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount)
        {
            return GetWindowText(hWnd, lpString, nMaxCount);
        }

        [DllImport("user32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);
        private bool EzMemorySetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags)
        {
            return SetWindowPos(hWnd, hWndInsertAfter, X, Y, cx, cy, uFlags);
        }

        [DllImport("user32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);
        private int EzMemoryGetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount)
        {
            return GetClassName(hWnd, lpClassName, nMaxCount);
        }
        [DllImport("user32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        private bool EzMemoryGetWindowRect(IntPtr hWnd, out RECT lpRect)
        {
            return GetWindowRect(hWnd, out lpRect);
        }

        [DllImport("user32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        private bool EzMemorySetForegroundWindow(IntPtr hWnd)
        {
            return SetForegroundWindow(hWnd);
        }

        // 检查窗口是否可见
        [DllImport("user32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool IsWindowVisible(IntPtr hWnd);
        private bool EzMemoryIsWindowVisible(IntPtr hWnd)
        {
            return IsWindowVisible(hWnd);
        }
        //  DPI 
        [DllImport("user32.dll")]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        private static extern IntPtr MonitorFromWindow(IntPtr hwhWndnd, uint dwFlags);
        private IntPtr EzMemoryMonitorFromWindow(IntPtr hWnd, uint dwFlags)
        {
            return MonitorFromWindow(hWnd, dwFlags);
        }

        [DllImport("shcore.dll")]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        private static extern int GetDpiForMonitor(IntPtr hmonitor, MONITOR_DPI_TYPE dpiType, out uint dpiX, out uint dpiY);
        private IntPtr EzMemoryGetDpiForMonitor(IntPtr hmonitor, MONITOR_DPI_TYPE dpiType, out uint dpiX, out uint dpiY)
        {
            return GetDpiForMonitor(hmonitor, dpiType, out dpiX, out dpiY);
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
        private const string publisherCompany = "Wistron";
        private const string publisherWebsite = "https://www.wistron.com";
        private const string publisherSupport = "This plugin implements EzMemory Manager Plugin.";

        //private bool _IsAdministrator = ProcessSecurityHelperWrapper.IsCurrentProcessRunningElevated();
        private static Logs _logs;
        private IAgent _agent;
        public const string PluginLogId = "EzMemoryManager";
        private IDisplayService _DisplayManagerPlugin;
        private ISettingsManagerDev _SettingsPlugin;
        //private static readonly object _PluginConditionLock = new object();
        private static readonly object _PluginConditionLock_Display = new object();
        private static readonly object _PluginConditionLock_Settings = new object();
        //private static System.Timers.Timer _SchedulerCheckTimer = new System.Timers.Timer(60000);
        private static List<MonitorInfo> _AllInfoMonitors;
        //private static List<scheduleInfo> _ScheduleMaps;
        private static DDPMSettings _DDPMSettings;
        //private static readonly object _MoLock = new object();

        #endregion

        private Timer _timer;

        #region Constructor

        public EzMemoryPlugin(IAgent agent) : base(agent, PluginLogId)
        {
            _agent = agent;
            //_IsAdministrator = ProcessSecurityHelperWrapper.IsCurrentProcessRunningElevated();
            _logs ??= new Logs(Log, PluginLogId);

            _AllInfoMonitors ??= new List<MonitorInfo>();
            //_ScheduleMaps ??= new List<scheduleInfo>();

            //_SchedulerCheckTimer.Elapsed += OnSchedulerTimedRaise;
            //_SchedulerCheckTimer.AutoReset = true;
            //_SchedulerCheckTimer.Enabled = true;

            //_timer = new Timer(CheckMonitorsAndLaunchApps, null, TimeSpan.Zero, TimeSpan.FromSeconds(10));
            //CheckMonitorsAndLaunchApps();
            _logs.DebugMsg_1("EzMemoryManagerPlugin constructor ...");
        }

        #endregion

        #region Overriding methods

        protected override void OnPluginStarting()
        {
            _agent.PluginManager.PluginsStarted += PluginManagerOnPluginsStarted;
            PluginCondition = new PluginStartedCondition();
            InitializeSettingsPlugin();
            InitializeDisplayManagerPlugin();

            _logs.DebugMsg_1("EzMemoryManager plugin Starting");
        }

        #endregion

        #region Imprement ISchedulerManager

        public Task StopEzMemoryManger()
        {
            _logs.DebugMsg_1("received StopSchedulerManger requested ...");

            //if (_SchedulerCheckTimer.Enabled)
            //    _SchedulerCheckTimer.Stop();

            return Task.FromResult(Task.CompletedTask);
        }

        public Task StartEzMemoryManger(int millisecond)
        {
            _logs.DebugMsg_1("received StartSchedulerManger: " + millisecond.ToString() + " requested ...");

            //if (_SchedulerCheckTimer.Enabled)
            //    _SchedulerCheckTimer.Stop();

            //_SchedulerCheckTimer.Interval = millisecond;
            //_SchedulerCheckTimer.AutoReset = true;
            //_SchedulerCheckTimer.Start();
            return Task.FromResult(Task.CompletedTask);
        }

        #endregion

        #region Private Methods

        private void InitializeEzMemoryPlugin()
        {
            if (_SettingsPlugin != null)
            {
                _logs.DebugMsg_1("EzMemoryPlugin InitializeScheduleInfo ...");

                //if (_ScheduleMaps != null) _ScheduleMaps.Clear();
                //else _ScheduleMaps = new List<scheduleInfo>();

                //var Count = 0;
                //do
                //{
                //    _DDPMSettings = _SettingsPlugin.ReloadAppConfigData().Result;
                //    Count++;
                //} while ((Count < 3) && (_DDPMSettings == null));

                //var ScheduleMaps_string = _DDPMSettings.UserSettings.Schedule ?? string.Empty;

                //if (!string.IsNullOrWhiteSpace(ScheduleMaps_string))
                //{
                //    _ScheduleMaps.AddRange(JsonConvert.DeserializeObject<List<scheduleInfo>>(ScheduleMaps_string));
                //    _logs.DebugMsg_1("_ScheduleMaps count : " + _ScheduleMaps.Count);
                //}
                //else
                //{
                //    if (_ScheduleMaps != null) _ScheduleMaps.Clear();
                //    else _ScheduleMaps = new List<scheduleInfo>();
                //    _logs.DebugMsg_1("ScheduleMaps setting is null");
                //}
            }
        }

        private void InitializeMonitorInfo()
        {
            if (_DisplayManagerPlugin != null)
            {
                _logs.DebugMsg_1("EzMemoryManager InitializeMonitorInfo ...");

                if (_AllInfoMonitors != null) _AllInfoMonitors.Clear();
                else _AllInfoMonitors = new List<MonitorInfo>();

                _AllInfoMonitors.AddRange(_DisplayManagerPlugin.GetMonitors().Result);

                _logs.DebugMsg_1("_AllInfoMonitors count : " + _AllInfoMonitors.Count);
            }
        }

        //private void OnSchedulerTimedRaise(Object source, System.Timers.ElapsedEventArgs e)
        //{
        //    _logs.DebugMsg_1("[Hook] OnSchedulerTimedRaise");

        //    if (_DisplayManagerPlugin != null && _SettingsPlugin != null)
        //    {
        //        InitializeMonitorInfo();
        //        InitializeScheduleInfo();

        //        if (_AllInfoMonitors.Count > 0)
        //        {
        //            CalculateNowValue();
        //        }
        //        else
        //            _logs.DebugMsg_1("[OnSchedulerTimedRaise] No monitors to service");
        //    }
        //}

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

                        //if (_ScheduleMaps != null) _ScheduleMaps.Clear();
                        //else _ScheduleMaps = new List<scheduleInfo>();
                    }
                    else if (pluginCondition is PluginStartedCondition)
                    {
                        _logs.DebugMsg_1($"{nameof(GetCurrentSettingsPluginCondition)} - Settings Plugin is in a started condition");

                        //if (_ScheduleMaps != null) _ScheduleMaps.Clear();
                        //else _ScheduleMaps = new List<scheduleInfo>();
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

            if (e.ChangedPlugins.OfType<IDisplayService>().Any())
                InitializeDisplayManagerPlugin();
        }

        #endregion

        private void CheckMonitorsAndLaunchApps(object state)//object state
        {
            if (_DisplayManagerPlugin != null)
            {
                _logs.DebugMsg_1("EzMemoryPlugin CheckMonitorsAndLaunchApps ...");
                Trace.WriteLine("EzMemoryPlugin CheckMonitorsAndLaunchApps ");

                if (_AllInfoMonitors != null) _AllInfoMonitors.Clear();
                else _AllInfoMonitors = new List<MonitorInfo>();

                _AllInfoMonitors.AddRange(_DisplayManagerPlugin.GetMonitors().Result);

                // 對每一個螢幕進行檢查
                foreach (var monitor in _AllInfoMonitors)
                {
                    Trace.WriteLine("CheckAndLaunchForMonitor " + monitor.modelName);
                    CheckAndLaunchForMonitor(monitor);
                }

                _logs.DebugMsg_1("_AllInfoMonitors count : " + _AllInfoMonitors.Count);
            }

        }

        private void CheckAndLaunchForMonitor(MonitorInfo monitorInfo)
        {
            List<DDPMMonitorSettings> monitorSettingsList = _SettingsPlugin.ReloadMonitorSettings(monitorInfo.modelName).Result;
           
            if (monitorSettingsList != null)
            {
                var monitorSettings = monitorSettingsList.FirstOrDefault(x => x.ServiceTag == monitorInfo.edid.ServiceTag);

                if (monitorSettings != null && monitorSettings.easyArrangementDDPM != null)
                {
                    var easyArrangement = monitorSettings.easyArrangementDDPM;
                    foreach(var ps in easyArrangement.Desktops[0].ProfileSettings)
                    {
                        //LaunchAndArrangeApps(ps.ID); // for test
                        TimeSpan autoStartTime = TimeSpan.FromSeconds(ps.AutoStartTime.Value);
                        if (ps.Auto && IsTimeToLaunch(autoStartTime))
                        {
                            LaunchAndArrangeApps(ps.ID);
                            Trace.WriteLine("ID = " + ps.ID);
                            Trace.WriteLine("Auto = " + ps.Auto);
                            Trace.WriteLine("AutoStartTime = " + ps.AutoStartTime);
                            Trace.WriteLine("StartUpLaunch = " + ps.StartUpLaunch);
                        }
                    }

                }
            }
        }

        private bool IsTimeToLaunch(TimeSpan autoStartTime)
        {
            var currentTime = DateTime.Now.TimeOfDay;
            Trace.WriteLine("CurrentTime = " + currentTime.Hours + " : " + currentTime.Minutes);
            Trace.WriteLine("StartUpLaunch = " + autoStartTime.Hours + " : " + autoStartTime.Minutes);
            return currentTime.Hours == autoStartTime.Hours && currentTime.Minutes == autoStartTime.Minutes;
        }

        private void LaunchAndArrangeApps(int profileId)
        {
            DDPMSettings ddpmSettings = _SettingsPlugin.ReloadAppConfigData().Result;
            Trace.WriteLine("LaunchAndArrangeApps");
            if (ddpmSettings != null && ddpmSettings.UserSettings.EAProfile != null)
            {
                foreach(var ea in ddpmSettings.UserSettings.EAProfile)
                {
                    if (profileId == ea.ID)
                    {
                        Trace.WriteLine("EAProfile");
                        Trace.WriteLine("ID = " + ea.ID);
                        Trace.WriteLine("Name = " + ea.Name);
                        Trace.WriteLine("Layout = " + ea.Layout);
                        Dictionary<string, Bind_AddFullPage_AppCollectionData> launchApp = new Dictionary<string, Bind_AddFullPage_AppCollectionData>();
                        foreach (var item in ea.AppInfos)
                        {
                            Bind_AddFullPage_AppCollectionData app = new Bind_AddFullPage_AppCollectionData();
                            app.AppPath = item.Path;
                            app.AppName = item.Name;
                            app.AppUserModelID = item.AppUserModelID;
                            app.AppType = item.IsUWP == false ? "False" : "True";
                            launchApp.Add(app.AppName, app);

                            Trace.WriteLine("AppInfos");
                            Trace.WriteLine("Name = " + item.Name);
                            Trace.WriteLine("Param = " + item.Param);
                            Trace.WriteLine("IsUWP = " + item.IsUWP.ToString());
                            Trace.WriteLine("AppUserModelID = " + item.AppUserModelID);
                            Trace.WriteLine("Path = " + item.Path);
                        }
                        bool result = LaunchAndArrangeApps(launchApp).Result;
                    }
                }
                //var easyArrangement = ddpmSettings.UserSettings.EAProfile;
            }
        }

        public Task<Dictionary<string, InstalledAppInfo>> GetAllAppList()
        {
            AppsCollectShell appshell = new AppsCollectShell();
            string RootColorPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Dell\\Dell Display and Peripheral Manager\\AppLibrary";
            string IconFolder = RootColorPath + "\\Icons\\";

            /* KNOWNFOLDERID: From MSDN https://learn.microsoft.com/en-us/windows/win32/shell/knownfolderid */
            Guid FOLDERID_AppsFolder = new Guid("1e87508d-89c2-42f0-8a7e-645a0f50ca58");

            AppListDictionary tmpAppListDictionary = AppListDictionary.GetInstance();

            Dictionary<string, InstalledAppInfo> installedApp = new Dictionary<string, InstalledAppInfo>();
            string fileinfo = string.Empty, info = string.Empty;
            DDPMFileSecurity.CheckFold(IconFolder, out fileinfo, out info);
            if (!System.IO.Directory.Exists(IconFolder))
                System.IO.Directory.CreateDirectory(IconFolder);

            Dictionary<string, List<AppItemInfo>> dictionary = new Dictionary<string, List<AppItemInfo>>();
            IKnownFolder ikf = null;
            try
            {
                ikf = KnownFolderHelper.FromKnownFolderId(FOLDERID_AppsFolder);
            }
            catch (Exception ex)
            {
                _logs.Info($"[EzMemoryLaunchOption], FromKnownFolderId Exception {ex}");
                return Task.FromResult(installedApp);
            }
            if (ikf == null)
            {
                _logs.Info($"[EzMemoryLaunchOption], ikf nul");
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
                    _logs.Info($"[EzMemoryLaunchOption], value and value2 {ex}");
                }

                //
                // Desktop application parsing
                //
                if (value != null && value.Length > 0)
                {
                    value = value.ToLower();
                    string text = value.Split('\\')[^1].ToLower();
                    if (!text.ToLower().Contains("exe"))
                    {
                        continue;
                    }
                    try
                    {
                        System.IO.FileInfo f = new System.IO.FileInfo(value);
                        DateTime lastAccessTime = f.CreationTime;//.LastAccessTime;
                        if (!File.Exists(IconFolder + text + ".png"))
                        {
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
                        _logs.Info($"[EzMemoryLaunchOption], Desktop application parsing Exception {ex}");
                    }
                    continue;
                }
                else
                {
                    _logs.Info($"[EzMemoryLaunchOption], not desktop app");
                }
                //
                // UWP application parsing
                //
                Bitmap bitmap = null;
                try
                {
                    string filename = name.ToLower();
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
                    _logs.Info($"[EzMemoryLaunchOption], UWP Exception {ex}");
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
                _logs.Info($"[EzMemoryLaunchOption], KeyValuePair<string, List<AppItemInfo>> Exception {ex}");
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
                catch
                {
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
        /// 兩個UWPOK
        /// </summary>
        /// <param name="appData"></param>
        /// <returns></returns>
        //public void LaunchAndArrangeApps()
        //{
        //    //if (_vm._seletcApps.Count < 2)
        //    //    return;


        //    var sortedByKey = _vm._sortApps.OrderBy(x => x.Key).ToList();
        //    _vm._seletcApps = sortedByKey.Select(x => x.Value).ToList();

        //    var firstApp = _vm._seletcApps[0];
        //    var secondApp = _vm._seletcApps[1];

        //    Task.Delay(3000).ContinueWith(async t =>
        //    {
        //        IntPtr firstHandle = IntPtr.Zero;
        //        IntPtr secondHandle = IntPtr.Zero;
        //        Process firstProcess = LaunchApp(firstApp);
        //        for (int i = 0; i < 10; i++)
        //        {
        //            firstHandle = GetWindowHandle(firstApp);
        //            //secondHandle = GetWindowHandle(secondApp);

        //            if (firstHandle != IntPtr.Zero)
        //                break;

        //            await Task.Delay(1000);
        //        }
        //        Process secondProcess = LaunchApp(secondApp);
        //        for (int i = 0; i < 10; i++)
        //        {
        //            secondHandle = GetWindowHandle(secondApp);

        //            if (secondHandle != IntPtr.Zero && secondHandle != firstHandle)
        //                break;

        //            await Task.Delay(1000);
        //        }

        //        if (firstHandle == IntPtr.Zero || secondHandle == IntPtr.Zero)
        //        {
        //            //Debug
        //            return;
        //        }

        //        Application.Current.Dispatcher.Invoke(() =>
        //        {
        //            double screenWidth = SystemParameters.PrimaryScreenWidth;
        //            double screenHeight = SystemParameters.PrimaryScreenHeight;

        //            SetWindowPos(firstHandle, IntPtr.Zero, 0, 0, (int)(screenWidth / 2), (int)screenHeight, SWP_SHOWWINDOW);

        //            SetWindowPos(secondHandle, IntPtr.Zero, (int)(screenWidth / 2), 0, (int)(screenWidth / 2), (int)screenHeight, SWP_SHOWWINDOW);
        //        });

        //    });
        //}

        public Task<bool> LaunchAndArrangeApps(Dictionary<String, Bind_AddFullPage_AppCollectionData> sortApps)
        {
            try
            {
                int appCount = sortApps.Count;
                if (appCount == 0)
                {
                    _logs.Info("[EzMemoryLaunchOption] LaunchAndArrangeApps, No apps to launch and arrange.");
                    return Task.FromResult(false);
                }

                List<Bind_AddFullPage_AppCollectionData> seletcApps = new List<Bind_AddFullPage_AppCollectionData>();
                double screenWidth = SystemParameters.PrimaryScreenWidth;
                double screenHeight = SystemParameters.PrimaryScreenHeight;
                double widthPerApp = screenWidth / appCount; // 平均分配寬度

                var sortedByKey = sortApps.OrderBy(x => x.Key).ToList();
                seletcApps = sortedByKey.Select(x => x.Value).ToList();

                Task.Run(async () =>
                {
                    List<IntPtr> windowHandles = new List<IntPtr>();

                    for (int i = 0; i < appCount; i++)
                    {
                        var app = seletcApps[i];
                        IntPtr handle = IntPtr.Zero;

                        try
                        {
                            // 檢查應用程式是否已經存在
                            Process[] processes = GetProcessesByName(app);
                            _logs.Info($"[EzMemoryLaunchOption] LaunchAndArrangeApps, GetProcessesByName(app): {app.AppName}");

                            if (processes.Length > 0)
                            {
                                handle = processes[0].MainWindowHandle;
                                _logs.Info($"[EzMemoryLaunchOption] LaunchAndArrangeApps, App {app.AppName} is already running, handle: {handle}");
                                EzMemorySetForegroundWindow(handle); // 把應用程式拉到前景
                            }
                            else
                            {
                                Process process = LaunchApp(app);

                                if (process == null)
                                {
                                    _logs.Error($"[EzMemoryLaunchOption] LaunchAndArrangeApps, Failed to launch app: {app.AppName}");
                                    continue;
                                }

                                // 等待應用程式的窗口初始化
                                for (int attempt = 0; attempt < 10; attempt++)
                                {
                                    handle = app.AppType == "True" ? process.MainWindowHandle : GetWindowHandle(app);

                                    if (handle != IntPtr.Zero && !windowHandles.Contains(handle))
                                        break;

                                    await Task.Delay(500);
                                }

                                if (handle == IntPtr.Zero)
                                {
                                    _logs.Error($"[EzMemoryLaunchOption] LaunchAndArrangeApps, App {app.AppName} failed to get window handle after launch.");
                                    continue;
                                }
                            }

                            // 取得視窗的 DPI 設定
                            float dpiScale = GetDpiScaleForWindow(handle);

                            // 調整視窗位置與大小，考慮 DPI 比例
                            EzMemorySetWindowPos(handle, IntPtr.Zero,
                                (int)((i * widthPerApp) * dpiScale),
                                0,
                                (int)(widthPerApp * dpiScale),
                                (int)(screenHeight * dpiScale),
                                SWP_SHOWWINDOW);

                            // 確認視窗是否已移動到預期的位置
                            for (int checkAttempt = 0; checkAttempt < 10; checkAttempt++)
                            {
                                if (EzMemoryGetWindowRect(handle, out RECT rect))
                                {
                                    if (rect.Left == (int)((i * widthPerApp) * dpiScale) && rect.Top == 0 &&
                                        rect.Right == (int)(((i + 1) * widthPerApp) * dpiScale) && rect.Bottom == (int)(screenHeight * dpiScale))
                                    {
                                        _logs.Info($"[EzMemoryLaunchOption] LaunchAndArrangeApps, App {app.AppName} positioned correctly.");
                                        break;
                                    }
                                }

                                await Task.Delay(500);
                            }

                            await Task.Delay(500); // 延遲以確保窗口已經穩定
                        }
                        catch (Exception ex)
                        {
                            _logs.Error($"[EzMemoryLaunchOption] LaunchAndArrangeApps, Error arranging app {app.AppName}: {ex}");
                        }
                    }

                });

                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                _logs.Error($"[EzMemoryLaunchOption] LaunchAndArrangeApps, Unexpected error: {ex}");
                return Task.FromResult(false);
            }
        }

        private Process LaunchApp(Bind_AddFullPage_AppCollectionData appData)
        {
            Process process = null;
            try
            {
                if (appData.AppType == "False")
                {
                    // UWP 應用程式
                    ProcessStartInfo startInfo = new ProcessStartInfo
                    {
                        FileName = "explorer.exe",
                        Arguments = $"shell:AppsFolder\\{appData.AppUserModelID}",
                        UseShellExecute = true
                    };
                    _logs.Info($"[EzMemoryLaunchOption] LaunchApp, Launching UWP app: {appData.AppName}");
                    process = Process.Start(startInfo);
                }
                else
                {
                    // Desktop exe或檔案
                    ProcessStartInfo startInfo = new ProcessStartInfo
                    {
                        FileName = appData.AppPath,
                        UseShellExecute = true,  // 系統自動選擇應用程式來開啟
                        Verb = "open"            // 指定開啟檔案的動作
                    };
                    _logs.Info($"[EzMemoryLaunchOption] LaunchApp, Launching desktop app or file: {appData.AppName}");
                    process = Process.Start(startInfo);
                }

                if (process != null)
                {
                    if (!appData.AppPath.EndsWith(".png") && !appData.AppPath.EndsWith(".jpg") && !appData.AppPath.EndsWith(".txt"))
                    {
                        process.WaitForInputIdle();
                        _logs.Info($"[EzMemoryLaunchOption] LaunchApp, App {appData.AppName} is now idle.");
                    }
                }
                else
                {
                    _logs.Error($"[EzMemoryLaunchOption] LaunchApp, Failed to launch app or file: {appData.AppName}");
                }
            }
            catch (Exception ex)
            {
                _logs.Error($"[EzMemoryLaunchOption] LaunchApp, Exception while launching app or file: {appData.AppName}, Error: {ex}");
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
                            _logs.Info($"[EzMemoryLaunchOption] GetWindowHandle, Found window for {appData.AppName}, handle: {windowHandle}");
                            return false;
                        }
                    }

                    return true;
                }, IntPtr.Zero);

                if (windowHandle == IntPtr.Zero)
                {
                    _logs.Error($"[EzMemoryLaunchOption] GetWindowHandle, Failed to get window handle for {appData.AppName}");
                }
            }
            catch (Exception ex)
            {
                _logs.Error($"[EzMemoryLaunchOption] GetWindowHandle, Exception while retrieving window handle for {appData.AppName}, Error: {ex}");
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
                    _logs.Info($"[EzMemoryLaunchOption] GetProcessesByName, UWP app {appData.AppName} process count: {processes.Length}");
                }
                else
                {
                    // Desktop
                    processes = Process.GetProcessesByName(System.IO.Path.GetFileNameWithoutExtension(appData.AppPath));
                    _logs.Info($"[EzMemoryLaunchOption] GetProcessesByName, Desktop app {appData.AppName} process count: {processes.Length}");
                }
            }
            catch (Exception ex)
            {
                _logs.Error($"[EzMemoryLaunchOption] GetProcessesByName, Exception while getting processes for {appData.AppName}, Error: {ex}");
            }

            return processes;
        }

        private float GetDpiScaleForWindow(IntPtr hWnd)
        {
            float dpiScale = 1.0f; // Default DPI scaling is 1.0 (100%)
            try
            {
                IntPtr monitor = MonitorFromWindow(hWnd, MONITOR_DEFAULTTONEAREST);
                if (monitor != IntPtr.Zero)
                {
                    uint dpiX, dpiY;
                    if (GetDpiForMonitor(monitor, MONITOR_DPI_TYPE.MDT_EFFECTIVE_DPI, out dpiX, out dpiY) == 0)
                    {
                        dpiScale = dpiX / 96.0f; // 96 DPI is the default 100% scaling
                        _logs.Info($"[EzMemoryLaunchOption] GetDpiScaleForWindow, DPI scaling for window: {dpiScale}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logs.Error($"[EzMemoryLaunchOption] GetDpiScaleForWindow, Exception while getting DPI scale for window, Error: {ex}");
            }

            return dpiScale;
        }

        private string GetWindowClassName(IntPtr hWnd)
        {
            StringBuilder className = new StringBuilder(256);
            try
            {
                EzMemoryGetClassName(hWnd, className, className.Capacity);
                _logs.Info($"[EzMemoryLaunchOption] GetWindowClassName, Window class name: {className}");
            }
            catch (Exception ex)
            {
                _logs.Error($"[EzMemoryLaunchOption] GetWindowClassName, Exception while getting window class name, Error: {ex}");
            }

            return className.ToString();
        }
    }
}