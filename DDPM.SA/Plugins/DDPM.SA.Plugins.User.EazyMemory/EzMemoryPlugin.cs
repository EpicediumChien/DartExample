
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

        public Task<Dictionary<string, InstalledAppInfo>> GetAllAppList()
        {
            AppsCollectShell appshell = new AppsCollectShell();
            string RootColorPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Dell\\Dell Display and Peripheral Manager\\AppLibrary";
            string IconFolder = RootColorPath + "\\Icons\\";

            /* KNOWNFOLDERID: From MSDN https://learn.microsoft.com/en-us/windows/win32/shell/knownfolderid */
            Guid FOLDERID_AppsFolder = new Guid("1e87508d-89c2-42f0-8a7e-645a0f50ca58");

            AppListDictionary tmpAppListDictionary = AppListDictionary.GetInstance();
            //AppsCollectShell appshell = new AppsCollectShell();
            //Dictionary<string, InstalledAppInfo> data = appshell.FindAppsbyShellForEzMemoryFullPathKey();
            //return data;

            //logger.SetLogModule("ColorApp");

            Dictionary<string, InstalledAppInfo> installedApp = new Dictionary<string, InstalledAppInfo>();
            //logger.WriteLog($"[ColorApp][FindAppsbyShell] App Icon folder: {IconFolder}");

            if (!System.IO.Directory.Exists(IconFolder))
                System.IO.Directory.CreateDirectory(IconFolder);

            Dictionary<string, List<AppItemInfo>> dictionary = new Dictionary<string, List<AppItemInfo>>();
            IKnownFolder ikf = null;
            try
            {
                ikf = KnownFolderHelper.FromKnownFolderId(FOLDERID_AppsFolder);
            }
            catch (ArgumentException)// ae)
            {
                //logger.WriteLog($"[ColorApp][FindAppsbyShell] try to query [FOLDERID_AppsFolder], exception: {ae.Message}");
                return Task.FromResult(installedApp);
            }
            if (ikf == null)
            {
                //logger.WriteLog($"[ColorApp][FindAppsbyShell] KnownFolderHelper.FromKnownFolderId got null return");
                return Task.FromResult(installedApp);
            }

            //logger.WriteLog($"[ColorApp][FindAppsbyShell] Step ShellObject loop, count:{ikf.ToList().Count}");
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
                catch (Exception)// ex)
                {
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
                        //logger.WriteLog($"[ColorApp][FindAppsbyShell] Desktop:({value}), not end with exe, next loop");
                        continue;
                    }
                    try
                    {
                        System.IO.FileInfo f = new System.IO.FileInfo(value);
                        DateTime lastAccessTime = f.CreationTime;//.LastAccessTime;
                        if (!File.Exists(IconFolder + text + ".png"))
                        {
                            System.Drawing.Icon.ExtractAssociatedIcon(value)!.ToBitmap().Save(IconFolder + text + ".png");
                            //logger.WriteLog($"[ColorApp][FindAppsbyShell] Save icon to [{IconFolder}{text}.png] (Desktop)");
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
                            Console.WriteLine("Desktop01******************** " + name.ToString() + " || " + text.ToString() + " || " + value.ToString());
                            //logger.WriteLog($"[ColorApp][FindAppsbyShell] Add installed app AppName[{name}]AppExeName[{text}]Date[{lastAccessTime}]ModelID[{parsingName}]");
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
                            Console.WriteLine("Desktop02******************** " + name.ToString() + " || " + text.ToString() + " || " + value.ToString());
                            //logger.WriteLog($"[ColorApp][FindAppsbyShell] Add installed app Exist[{value}]: AppName[{name}]AppExeName[{text}]Date[{lastAccessTime}]ModelID[{parsingName}]");
                        }
                    }
                    catch (Exception)// ex1)
                    {
                        //logger.WriteLog($"[ColorApp][FindAppsbyShell] Desktop:({ex1.Message})");
                    }
                    continue;
                }
                else
                {
                    //logger.WriteLog($"[ColorApp][FindAppsbyShell] item:({item}), got null [item.Properties.System.Link.TargetParsingPath.Value], not desktop app");
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
                            //logger.WriteLog($"[ColorApp][FindAppsbyShell] Save icon to [{IconFolder}{filename}.png] (UWP)");
                        }
                        if (!installedApp.ContainsKey(text2))
                        {
                            installedApp.Add(text2, new InstalledAppInfo(name, text2, filename, now, bDesktopApp: false, parsingName));
                            Console.WriteLine("UWP0******************** " + name.ToString() + " || " + text2.ToString() + " || " + filename.ToString());
                            //logger.WriteLog($"[ColorApp][FindAppsbyShell] Add installed app AppName[{name}]AppExeName[{text2}]Date[{now}]ModelID[{parsingName}]");
                        }
                    }
                }
                catch (Exception)// ex2)
                {
                    //logger.WriteLog($"[ColorApp][FindAppsbyShell] UWP:({ex2.Message})");
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
            catch (Exception)// ex3)
            {
                //logger.WriteLog($"[ColorApp][FindAppsbyShell] Merge:({ex3.Message})");
            }
            AppListDictionary.GetInstance().LoadFile();
            tmpAppListDictionary = AppListDictionary.GetInstance();
            foreach (string key in installedApp.Keys)
            {
                if (!tmpAppListDictionary.AppInstallsList.ContainsKey(key))
                {
                    tmpAppListDictionary.AppInstallsList.Add(key, installedApp[key]);
                    Console.WriteLine("3******************** " + key.ToString() + " || " + installedApp[key].ToString());
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
    }
}