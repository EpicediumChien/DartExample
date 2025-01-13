using DDPM.SA.Common;
using Dell.Client.Framework.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using VcpCore.Common;

namespace DDPM.ColorApp
{
    /// <summary>
    /// Interaction logic for MonitorWin.xaml
    /// </summary>
    public partial class MonitorWin : Window
    {
        private IDeviceManagerSA ddmLib;//Dean 0626 fix SAST issue, remove static as recommend and set as private
        private MonitorInfo Mi;//Dean 0626 fix SAST issue, remove static as recommend and set as private
        //private string Pre_reqKey = string.Empty;//Dean 0626 fix SAST issue, remove static as recommend and set as private
        private int Pre_reqKey = -1;

        // 20240823 jim add - declare log variable
        private Logs _logs;

        // jim add 20240605
        private bool b_AUTO_ColorPresetConfig = false;//Dean 0626 fix SAST issue, remove static as recommend and set as private

        private bool b_SmartHDR_ON= false;
        private bool b_Is_Game_DeviceName = false;  // Jim 20241207 add
        private bool bl_actived_mi_matched_config = false; // Jim 20241219 add

        private List<MonitorInfo> _AllInfoMonitors = new List<MonitorInfo>(); // Jim 20241218 add for PIMS-326072 

        #region data region

        private List<AppCollectionData> _apps = new List<AppCollectionData>();
        private AppStatusQuery? appStatus = null;//Dean 0626 fix SAST issue, remove static as recommend
        private List<ColorPresetSettings>? appconfigs = null;
        //private List<ColorPresetSettings>? appconfigs = new List<ColorPresetSettings>();
        private List<string> _supported_preset = new List<string>();

        private ILog Log { get; set; }

        #endregion data region

        public MonitorWin(IDeviceManagerSA _ddmLib, MonitorInfo m, ILog log)
        {
            //Trace.WriteLine("ColorApp - MonitorWin");

            InitializeComponent();

            ddmLib = _ddmLib;
            Mi = m;
            this.WindowStyle = WindowStyle.None;
            AllowsTransparency = true;
            Opacity = 0.0f;
            Log = log; 
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //Trace.WriteLine("ColorApp - Window_Loaded");

            btnRefresh_Click(null, null);

            reload_color_settings_to_config();

            appStatus = AppStatusQuery.GetInstance(Log);
            //AppStatusQuery.SendValue += EventAppStatus_SendValue;
        }

        // jim add 20240605
        // jim 20241207 modify for The DDPM color profile can not be applied by DDPM on Smart HDR mode.(Gaming monitor ex: AW2724DM)
        public void Set_AUTO_ColorPresetConfig(bool blAUTO, bool blIs_Game_DeviceName, bool blSmartHDR_ON, List<string> ColorPresetSupportList)
        {
            Pre_reqKey = -1;
            b_AUTO_ColorPresetConfig = blAUTO;
            b_SmartHDR_ON = blSmartHDR_ON;
            b_Is_Game_DeviceName = blIs_Game_DeviceName; // jim add 20241207
            _supported_preset = ColorPresetSupportList;

            writelog("Set_AUTO_ColorPresetConfig AUTO_ColorPresetConfig = " + blAUTO);

            if (b_AUTO_ColorPresetConfig)
            {
                AppStatusQuery.SendValue += EventAppStatus_SendValue;
                AppStatusQuery.GetInstance(Log).ClearLastAppRecord("SET_AUTO");
            }
            else
            {                
                AppStatusQuery.SendValue -= EventAppStatus_SendValue;
                AppStatusQuery.GetInstance(Log).ClearLastAppRecord("SET_MANUAL");
            }

            writelog("Set_AUTO_ColorPresetConfig SmartHDR_ON = " + b_SmartHDR_ON);                           
        }

        // jim add 20240620
        public void Notify_refresh_app_list()
        {
            reload_color_settings_to_config();
        }

        // jim 20241218 modify For PIMS-326072
        public void Set_AllMonitors(List<MonitorInfo> allInfoMonitors)
        {
            //if (Mi != null && m != null)
            //    Mi = m;

            _AllInfoMonitors.Clear();
            _AllInfoMonitors.AddRange(allInfoMonitors);
        }

        public void reload_color_settings_to_config()
        {
            //Trace.WriteLine("ColorApp - reload_color_settings_to_config()");

            //Load saved app preset info

            // jim add 20240806
            if (appconfigs != null)
                appconfigs.Clear();

            appconfigs = ddmLib.ReadColorPresetSettings().Result;
            //Pre_reqKey = string.Empty;
            Pre_reqKey = -1;
        }

        private List<AppCollectionData> load_app_list()
        {
            AppsCollectShell appshell = new AppsCollectShell(Log);
            Dictionary<string, InstalledAppInfo> data = appshell.FindAppsbyShell();
            List<AppCollectionData> apps = new List<AppCollectionData>();

            // 20240619 jim modify to fix exception
            if (data == null)
            {
                // bad data, you better handle it and not carry on
                return apps;
            }
            if (data.Count <= 0)
                return apps;

            foreach (var item in data)
            {
                InstalledAppInfo app = item.Value;
                if (app != null)
                {
                    //InstalledAppInfo app = item.Value;
                    apps.Add(new AppCollectionData()
                    {
                        AppName = app.AppName,
                        IconName = app.IconName,
                        AppType = app.isDesktopApp ? "Desktop" : "UWP",
                        AppUserModelID = app.AppUserModelID,
                        AppPath = app.AppInstallPath,
                        InstalledDate = app.lastModifyTime
                    });
                }
            }

            return apps;
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            if (_apps != null)
                _apps.Clear();

            Thread update = new Thread(refresh_app_list)
            {
                Name = "refresh_app_list",
                IsBackground = true
            };
            update.Start();
        }

        private void refresh_app_list()
        {
            _apps = load_app_list();

            this.Dispatcher.Invoke(() =>
            {
                lstApps.ItemsSource = _apps;
            });
        }
        /*
        public List<AppCollectionData>? get_loaded_apps_list()
        {
            if (_apps != null && _apps.Count > 0)
            {
                return _apps;
            }
            else
            {
                refresh_app_list();
                return _apps;
            }
        }*/

        private enum log_type
        {
            info = 0,
            error
        }

        private void writelog(string? text, log_type log_type = log_type.info)
        {
            text = "[ColorApp] " + text;
            Console.WriteLine(text);

            if (Log != null) // Elie, the instance of Log is from DTH. So we just check if it's null or not.
            {
                if (log_type == log_type.info)
                    Log.Info(text);
                else
                    Log.Error(text);
            }
        }

        private void EventAppStatus_SendValue(object? sender, EventArgs e)
        {
            if (b_AUTO_ColorPresetConfig)
            {
                /*ActiveWindowData data = null;
                Screen screen = null;

                if (sender != null)
                {
                    data = sender as ActiveWindowData;
                    screen = Screen.FromHandle(data.ActiveWindowHandle);

                    System.Windows.Forms.Screen? s = System.Windows.Forms.Screen.AllScreens.FirstOrDefault(x => x.DeviceName == Mi.DisplayName);

                    if (s == null)//Dean 0626 fix SAST issue
                        return;

                    if ((screen.WorkingArea.Height != s.WorkingArea.Height) || (screen.WorkingArea.Width != s.WorkingArea.Width) || (screen.WorkingArea.Left != s.WorkingArea.Left))
                        return;
                }*/

                if (sender != null)
                {
                    //Get window data from active window's event
                    ActiveWindowData data = null;
                    Screen screen = null;
                    data = sender as ActiveWindowData;
                    //tbWndName.Text = data.ActiveWindowTitle;
                    //tbWndPID.Text = data.ActiveWindowProcessId.ToString();
                    //tbWndModule.Text = data.ActiveWindowProcessModuleName;

                    screen = Screen.FromHandle(data.ActiveWindowHandle);

                    if (screen == null)
                    {
                        writelog($"[EventAppStatus_SendValue] Active window handle is null");
                        return;
                    }

                    System.Windows.Forms.Screen? s = null;

                    // Jim 20250108 add exception handling 
                    try
                    {
                        s = System.Windows.Forms.Screen.AllScreens.FirstOrDefault(x => x.DeviceName == Mi.DisplayName);
                    }
                    catch (Exception ex)
                    {
                        writelog($"[EventAppStatus_SendValue(] FindScreen {ex.Message} ");
                    }                    

                    if (Mi != null && screen!= null && s != null) // Jim 20250108 add if null check
                    {
                        Trace.WriteLine("Mi.DisplayName = " + Mi.DisplayName);
                        Trace.WriteLine("Mi.modelName = " + Mi.modelName);


                    Trace.WriteLine("screen.WorkingArea.Height = " + screen.WorkingArea.Height);
                    Trace.WriteLine("s.WorkingArea.Height = " + s?.WorkingArea.Height);

                    Trace.WriteLine("screen.WorkingArea.Width = " + screen.WorkingArea.Width);
                    Trace.WriteLine("s.WorkingArea.Width = " + s?.WorkingArea.Width);

                    Trace.WriteLine("screen.WorkingArea.Left = " + screen.WorkingArea.Left);
                    Trace.WriteLine("s.WorkingArea.Left = " + s?.WorkingArea.Left);

                        writelog("Mi.DisplayName = " + Mi.DisplayName);
                        writelog("Mi.modelName = " + Mi.modelName);


                    writelog("screen.WorkingArea.Height = " + screen.WorkingArea.Height);
                    writelog("s.WorkingArea.Height = " + s?.WorkingArea.Height);

                    writelog("screen.WorkingArea.Width = " + screen.WorkingArea.Width);
                    writelog("s.WorkingArea.Width = " + s?.WorkingArea.Width);

                    writelog("screen.WorkingArea.Left = " + screen.WorkingArea.Left);
                    writelog("s.WorkingArea.Left = " + s?.WorkingArea.Left);

                    //if (s == null)//Dean 0626 fix SAST issue
                    //    return;

                    //if ((screen.WorkingArea.Height != s.WorkingArea.Height) || (screen.WorkingArea.Width != s.WorkingArea.Width) || (screen.WorkingArea.Left != s.WorkingArea.Left))
                    //    return;

                    string strFilePath = data.ActiveWindowFilePath;

                    writelog("EventAppStatus_SendValue ActiveWindowTitle = " + data.ActiveWindowTitle);
                    writelog("EventAppStatus_SendValue ActiveWindowProcessId = " + data.ActiveWindowProcessId.ToString());
                    writelog("EventAppStatus_SendValue ActiveWindowProcessModuleName = " + data.ActiveWindowProcessModuleName);
                    writelog("EventAppStatus_SendValue ActiveWindowFilePath = " + data.ActiveWindowFilePath);

                    MonitorInfo actived_mi = null;

                    // Jim 20250110 modify for exception 
                    if (_AllInfoMonitors == null || _AllInfoMonitors.Count == 0)
                    {
                        writelog("No any Monitors is matched, _AllInfoMonitors was null or empty");
                        actived_mi = null;
                        return;
                    }

                    try
                    {
                        actived_mi = _AllInfoMonitors.Find(x => x.DisplayName.ToUpper().Equals(screen.DeviceName.ToUpper()));
                    }
                    catch (Exception ex)
                    {
                        writelog($"[EventAppStatus_SendValue] Find matched Monitor has error = {ex.Message}");
                        actived_mi = null;
                    }            

                    //Get actived Monitor from actived window
                    //screen = Screen.FromHandle(data.ActiveWindowHandle);

                    //MonitorInfo actived_mi = Mi;

                    // Jim 20250110 modify for exception
                    if (actived_mi == null)
                    {
                        writelog("No any Monitors is matched, actived_mi was null");
                        return;
                    }

                    if (!actived_mi.IsDellMonitor) // Jim 20250109 modify
                    {
                        writelog("The activated monitor does not meet the criteria");
                        return;
                    }                

                    //////get active process's modeul info
                    Process forgroundProcess = Process.GetProcessById((int)data.ActiveWindowProcessId);

                    if (appconfigs == null)
                    {
                        writelog("appconfigs is null");
                        //Trace.WriteLine("appconfigs is null");
                        return;
                    }
                    //convert module name to app name, ex: 7zFM.exe -> 7-Zip File Manager
                    string reqAppName = string.Empty;
                    int index = -1;
                    if (forgroundProcess != null && forgroundProcess.MainModule != null) {
                    index = _apps.FindIndex(x =>
                                     forgroundProcess.MainModule.FileName.ToLower().Trim().IndexOf(x.AppPath.ToLower().Trim()) >= 0 ||
                                    //forgroundProcess.MainModule.FileName.ToLower().Trim().IndexOf(x.AppName.ToLower().Trim()) >= 0 ||
                                    forgroundProcess.MainModule.ModuleName.ToLower().Trim().Replace(".exe", "") == x.AppName.ToLower().Trim().Replace(".exe", "")
                                    );
                    }

                    if (index < 0)
                    {
                        //check with filepath (UWP like app would be this condition)
                        string folder = strFilePath;
                        if (strFilePath.Trim().ToLower().EndsWith(".exe"))
                        {
                            folder = System.IO.Path.GetDirectoryName(strFilePath);
                        }
                        index = _apps.FindIndex(x => folder.Trim().IndexOf(x.AppPath.Trim()) >= 0);
                        if (index < 0)
                        {
                            writelog("check with filepath (UWP like app would be this condition) no matched");
                            return;
                        }
                        writelog("check with filepath (UWP like app would be this condition) " + index.ToString());
                    }

                    reqAppName = _apps[index].AppName;

                    //Trace.WriteLine("reqAppName = " + reqAppName);
                    writelog("reqAppName = " + reqAppName);

                    bool isDesktop = _apps[index].AppType.Equals("Desktop"); //besides are UWP

                    //Trace.WriteLine("isDesktop  = " + isDesktop);
                    writelog("isDesktop  = " + isDesktop.ToString());

                    //string tmp = string.Empty;// used for UI display

                    // jim add 20240809
                    if (appconfigs != null)
                        appconfigs.Clear();

                    appconfigs = ddmLib.ReadColorPresetSettings().Result;

                    Trace.WriteLine("appconfigs.Count = " + appconfigs.Count.ToString());
                    writelog("appconfigs.Count = " + appconfigs.Count.ToString());

                    foreach (var config in appconfigs)
                    {
                        if (config.AppInfo == null || config.AppInfo.Count <= 0)
                        {
                            //Trace.WriteLine("config.AppInfo.Count = " +  config.AppInfo.Count.ToString());
                            writelog("appconfigs.Count = " + appconfigs.Count.ToString());
                            continue;
                        }

                        //Check if actived monitor has its color preset section in config file

                        bl_actived_mi_matched_config = (actived_mi.edid.ModelName.Trim() == config.ModelName.Trim()) && (actived_mi.edid.SerialNumber.Trim() == config.SerialNumber.Trim());

                        if (!bl_actived_mi_matched_config)
                            bl_actived_mi_matched_config = (actived_mi.edid.ModelName.Trim() == config.ModelName.Trim()) && (actived_mi.edid.ServiceTag.Trim() == config.ServiceTag.Trim());

                        //if (actived_mi.edid.ModelName.Trim().IndexOf(config.ModelName.Trim()) >= 0 &&
                        //    actived_mi.edid.SerialNumber.Trim() == config.SerialNumber.Trim())
                        if (bl_actived_mi_matched_config)
                        {
                            if (config.RunType != (int)ColorPresetRunType.Auto)
                            {
                                writelog("config.RunType  is not ColorPresetRunType.Auto");
                                //Trace.WriteLine("config.RunType  is not ColorPresetRunType.Auto");
                                writelog("config.RunType  is not ColorPresetRunType.Auto");
                                break;
                            }

                            //string reqKey = string.Empty;
                            int reqKey = -1;

                            /*foreach (var item_appname in config.AppInfo.Keys)
                            {
                                //Trace.WriteLine("item_appname = " + item_appname);

                                if (!reqAppName.Contains(item_appname,StringComparison.OrdinalIgnoreCase))
                                {
                                    if (isDesktop)
                                    {
                                        if (config.AppInfo.ContainsKey("Desktop Application"))
                                        {
                                            reqKey = config.AppInfo["Desktop Application"].ColorPresetName.Trim();
                                        }
                                        else
                                        {
                                            //return;
                                            continue;
                                        }
                                    }
                                    else //UWP
                                    {
                                        if (config.AppInfo.ContainsKey("UWP Application"))
                                        {
                                            reqKey = config.AppInfo["UWP Application"].ColorPresetName.Trim();
                                        }
                                        else
                                        {
                                            //return;
                                            continue;
                                        }
                                    }
                                }
                                else
                                {
                                    reqKey = (config.AppInfo[item_appname]).ColorPresetName.Trim();
                                    writelog("reqKey (ColorPresetName)  = " + reqKey);
                                    //Trace.WriteLine("reqKey (ColorPresetName) = " + reqKey);
                                    break;
                                }
                            }*/

                            if (!config.AppInfo.ContainsKey(reqAppName))
                            {
                                if (isDesktop)
                                {
                                    if (config.AppInfo.ContainsKey("Desktop Application"))
                                    {
                                        //reqKey = config.AppInfo["Desktop Application"].ColorPresetName.Trim();
                                        if (b_SmartHDR_ON)
                                            reqKey = config.AppInfo["Desktop Application"].HDRColor;
                                        else
                                            reqKey = config.AppInfo["Desktop Application"].Color;

                                        writelog("[Desktop Application] ColorPresetName = " + reqKey.ToString());
                                    }
                                    else
                                    {
                                        writelog(" return  - Desktop Application");
                                        return;
                                    }
                                }
                                else //UWP
                                {
                                    if (config.AppInfo.ContainsKey("UWP Application"))
                                    {
                                        //reqKey = config.AppInfo["UWP Application"].ColorPresetName.Trim();
                                        if (b_SmartHDR_ON)
                                            reqKey = config.AppInfo["UWP Application"].HDRColor;
                                        else
                                            reqKey = config.AppInfo["UWP Application"].Color;

                                        writelog("[UWP Application] ColorPresetName = " + reqKey.ToString());
                                    }
                                    else
                                    {
                                        writelog(" return  - UWP Application");
                                        return;
                                    }
                                }
                            }
                            else
                            {
                                //reqKey = (config.AppInfo[reqAppName]).ColorPresetName.Trim();

                                if (b_SmartHDR_ON)
                                    reqKey = (config.AppInfo[reqAppName]).HDRColor;
                                else
                                    reqKey = (config.AppInfo[reqAppName]).Color;

                                writelog("reqAppName = " + reqAppName + "," + "reqKey [ColorPresetName] = " + reqKey.ToString());
                            }

                            //if (string.IsNullOrEmpty(reqKey))
                            if (reqKey == -1)
                            {
                                writelog("reqAppName = " + reqAppName + "," + "reqKey [ColorPresetName] is string.IsNullOrEmpty");
                                //Trace.WriteLine("reqKey is string.IsNullOrEmpty");
                                return;
                            }

                            //if (!Pre_reqKey.Equals(reqKey, StringComparison.OrdinalIgnoreCase))
                            if ( Pre_reqKey != reqKey)
                            {
                                //writelog("reqAppName = " + reqAppName + "," + "Pre_reqKey  [ColorPresetName] is " + Pre_reqKey);
                                //writelog("reqAppName = " + reqAppName + "," + "reqKey  [ColorPresetName] is " + reqKey);

                                writelog("reqAppName = " + reqAppName + "," + "Pre_reqKey  [ColorPresetName] is " + Pre_reqKey.ToString());
                                writelog("reqAppName = " + reqAppName + "," + "reqKey  [ColorPresetName] is " + reqKey.ToString());

                                Pre_reqKey = reqKey;
                                //
                                //Set request key to update color preset and draw OSD
                                string outmsg = string.Empty;
                                set_monitor_preset_by_request_key(actived_mi, reqKey, out outmsg, b_SmartHDR_ON, reqAppName);
                                //tmp = actived_mi.AliasDeviceName + ":" + reqKey;
                                break;
                            }
                            else
                            {
                                //writelog("reqAppName = " + reqAppName + "," + "Pre_reqKey  [ColorPresetName] is " + Pre_reqKey);
                                //writelog("reqAppName = " + reqAppName + "," + "Pre_reqKey  [ColorPresetName] is " + reqKey);

                                writelog("reqAppName = " + reqAppName + "," + "Pre_reqKey  [ColorPresetName] is " + Pre_reqKey.ToString());
                                writelog("reqAppName = " + reqAppName + "," + "Pre_reqKey  [ColorPresetName] is " + reqKey.ToString());

                                writelog("Pre_reqKey and reqKey is the same");

                                //Trace.WriteLine("Pre_reqKey and reqKey is the same");
                            }
                            
                        }
                    }

                    //if (string.IsNullOrEmpty(tmp))
                    //{
                    //    tbColorPreset.Text = "NA";
                    //}
                    //else
                    //    tbColorPreset.Text = tmp;
                }
            }
        }

        //public bool set_monitor_preset_by_request_key(MonitorInfo actived_mi, string reqKey, out string outmsg, bool isDrawOSD = true)
        public bool set_monitor_preset_by_request_key(MonitorInfo actived_mi, int reqKey, out string outmsg, bool b_SmartHDR_ON, string reqAppName = null, bool isDrawOSD = true)
        {
            //if (string.IsNullOrEmpty(reqKey))
            if (reqKey == -1)
            {
                outmsg = string.Format($"SetVCP] {actived_mi.AliasDeviceName}, null request key!");
                writelog("[set_monitor_preset_by_request_key] reqKey is string.IsNullOrEmpty");
                return false;
            }

            outmsg = string.Format($"[SetVCP] {actived_mi.AliasDeviceName}, SetVCP preset:{reqKey} OK!");

            // 20240619 jim modify
            //bool bi = ddmLib.WriteColorPreset_AUTO("0", actived_mi, reqKey).Result;
            //bool bi = ddmLib.WriteColorPreset_AUTO(actived_mi, reqKey).Result;

            var strColorPresetName = ddmLib.GetColorPresetName(reqKey).Result;

            string strSync_CurrentColorPreset = string.Empty;

            // PIMS-327394 , jim 20241212 modify 
            //if (!b_SmartHDR_ON)
            //{
            strSync_CurrentColorPreset = ddmLib.Sync_ColorPresetName(actived_mi, strColorPresetName).Result;
                //strSync_CurrentColorPreset = Sync_CurrentColorPreset(strColorPresetName);
            //}
            //else
                //strSync_CurrentColorPreset = strColorPresetName;

            // jim 20241207  modify for The DDPM color profile can not be applied by DDPM on Smart HDR mode.(Gaming monitor ex: AW2724DM)
            bool bi = ddmLib.WriteColorPreset(actived_mi, strSync_CurrentColorPreset, 1, b_Is_Game_DeviceName, b_SmartHDR_ON, reqAppName).Result;

            return true;
        }      
    }
}