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

        #region data region

        private List<AppCollectionData> _apps = new List<AppCollectionData>();
        private AppStatusQuery? appStatus = null;//Dean 0626 fix SAST issue, remove static as recommend
        private List<ColorPresetSettings>? appconfigs = null;
        //private List<ColorPresetSettings>? appconfigs = new List<ColorPresetSettings>();
        private List<string> _supported_preset = new List<string>();

        private ILog Log { get; set; }

        #endregion data region

        public MonitorWin(IDeviceManagerSA _ddmLib, MonitorInfo m)
        {
            //Trace.WriteLine("ColorApp - MonitorWin");

            InitializeComponent();

            ddmLib = _ddmLib;
            Mi = m;
            this.WindowStyle = WindowStyle.None;
            AllowsTransparency = true;
            Opacity = 0.0f;
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
        public void Set_AUTO_ColorPresetConfig(bool blAUTO, bool blSmartHDR_ON, List<string> ColorPresetSupportList)
        {
            b_AUTO_ColorPresetConfig = blAUTO;
            b_SmartHDR_ON = blSmartHDR_ON;
            _supported_preset = ColorPresetSupportList;

            if (b_AUTO_ColorPresetConfig)
            {
                writelog("Set_AUTO_ColorPresetConfig AUTO_ColorPresetConfig = " + blAUTO);
                AppStatusQuery.SendValue += EventAppStatus_SendValue;
                AppStatusQuery.GetInstance(Log).ClearLastAppRecord("SET_AUTO");
            }
            else
            {
                writelog("Set_AUTO_ColorPresetConfig AUTO_ColorPresetConfig = " + blAUTO);
                AppStatusQuery.SendValue -= EventAppStatus_SendValue;
                AppStatusQuery.GetInstance(Log).ClearLastAppRecord("SET_MANUAL");
            }

            if (b_SmartHDR_ON)
            {
                writelog("Set_AUTO_ColorPresetConfig SmartHDR_ON = " + b_SmartHDR_ON);               
            }
            else
            {
                writelog("Set_AUTO_ColorPresetConfig SmartHDR_ON = " + b_SmartHDR_ON);
            
            }
        }

        // jim add 20240620
        public void Notify_refresh_app_list()
        {
            reload_color_settings_to_config();
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
            AppsCollectShell appshell = new AppsCollectShell();
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
        }

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

                    System.Windows.Forms.Screen? s = System.Windows.Forms.Screen.AllScreens.FirstOrDefault(x => x.DeviceName == Mi.DisplayName);

                    if (s == null)//Dean 0626 fix SAST issue
                        return;

                    if ((screen.WorkingArea.Height != s.WorkingArea.Height) || (screen.WorkingArea.Width != s.WorkingArea.Width) || (screen.WorkingArea.Left != s.WorkingArea.Left))
                        return;

                    string strFilePath = data.ActiveWindowFilePath;

                    writelog("EventAppStatus_SendValue ActiveWindowTitle = " + data.ActiveWindowTitle);
                    writelog("EventAppStatus_SendValue ActiveWindowProcessId = " + data.ActiveWindowProcessId.ToString());
                    writelog("EventAppStatus_SendValue ActiveWindowProcessModuleName = " + data.ActiveWindowProcessModuleName);
                    writelog("EventAppStatus_SendValue ActiveWindowFilePath = " + data.ActiveWindowFilePath);

                    //Get actived Monitor from actived window
                    //screen = Screen.FromHandle(data.ActiveWindowHandle);

                    MonitorInfo actived_mi = Mi;

                    if (!actived_mi.IsDellMonitor)
                    {
                        writelog("actived_mi.IsDellMonitor is False");
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
                    int index = _apps.FindIndex(x =>
                                     forgroundProcess.MainModule.FileName.ToLower().Trim().IndexOf(x.AppPath.ToLower().Trim()) >= 0 ||
                                    //forgroundProcess.MainModule.FileName.ToLower().Trim().IndexOf(x.AppName.ToLower().Trim()) >= 0 ||
                                    forgroundProcess.MainModule.ModuleName.ToLower().Trim().Replace(".exe", "") == x.AppName.ToLower().Trim().Replace(".exe", "")
                                    );
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
                        if (actived_mi.edid.ModelName.Trim().IndexOf(config.ModelName.Trim()) >= 0 &&
                            actived_mi.edid.SerialNumber.Trim() == config.SerialNumber.Trim())
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
                                set_monitor_preset_by_request_key(actived_mi, reqKey, out outmsg);
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
        public bool set_monitor_preset_by_request_key(MonitorInfo actived_mi, int reqKey, out string outmsg, bool isDrawOSD = true)
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
            strSync_CurrentColorPreset = Sync_CurrentColorPreset(strColorPresetName);

            bool bi = ddmLib.WriteColorPreset(actived_mi, strSync_CurrentColorPreset, 1).Result;

            return true;
        }

        public string Sync_CurrentColorPreset(string curcolorPreset)
        {
            string strSync_CurrentColorPreset = string.Empty;

            int index = -1;

            // check Color Preset Strings Standard or Native

            if (Mi.modelName.StartsWith("UP"))
            {
                if (curcolorPreset == "Standard/Native")
                    strSync_CurrentColorPreset = "Native";
            }
            else
            {
                if (curcolorPreset == "Standard/Native")
                    strSync_CurrentColorPreset = "Standard";
            }

            // check Color Preset Strings Custom 1/2/3 or User 1/2/3

            if (Mi.modelName.StartsWith("UP3221Q"))
            {
                if (curcolorPreset == "Custom 1 / User 1")
                    strSync_CurrentColorPreset = "User 1";
                else if (curcolorPreset == "Custom 2 / User 2")
                    strSync_CurrentColorPreset = "User 2";
                else if (curcolorPreset == "Custom 3 / User 3")
                    strSync_CurrentColorPreset = "User 3";
            }
            else
            {
                if (curcolorPreset == "Custom 1 / User 1")
                    strSync_CurrentColorPreset = "Custom 1";
                else if (curcolorPreset == "Custom 2 / User 2")
                    strSync_CurrentColorPreset = "Custom 2";
                else if (curcolorPreset == "Custom 3 / User 3")
                    strSync_CurrentColorPreset = "Custom 3";
            }

            // check Color Preset Strings Game or Game1

            if (_supported_preset!= null)
            {
                if (_supported_preset.Count >= 0)
                {
                    index = _supported_preset.FindIndex(x => x == "Game2");

                    if (index >= 0)
                    {
                        if (curcolorPreset == "Game/Game1")
                            strSync_CurrentColorPreset = "Game1";
                    }
                    else
                    {
                        if (curcolorPreset == "Game/Game1")
                            strSync_CurrentColorPreset = "Game";
                    }

                }              
            }          

            // check Color Preset Strings Rec.709 or BT.709 / Rec.709 or BT.709

            string strFY = string.Empty;

            for (int i = 0; i < Mi.modelName.Length; i++) // loop over the complete modelName
            {
                if (Char.IsDigit(Mi.modelName[i])) //check if the current char is digit
                {
                    strFY = Mi.modelName.Substring(i + 2, 2);
                    break;
                }

            }

            // check Color Preset Strings Rec.2020 or BT.2020 / Rec.2020 or BT.2020

            if (strFY == "23")
            {
                if (curcolorPreset == "Rec.709 / BT.709")
                    strSync_CurrentColorPreset = "Rec.709";

                if (curcolorPreset == "Rec.2020 / BT.2020")
                    strSync_CurrentColorPreset = "Rec.2020";

            }
            else if (strFY == "25")
            {
                if (curcolorPreset == "Rec.709 / BT.709")
                    strSync_CurrentColorPreset = "BT.709";

                if (curcolorPreset == "Rec.2020 / BT.2020")
                    strSync_CurrentColorPreset = "BT.2020";
            }

            if (System.String.IsNullOrEmpty(strSync_CurrentColorPreset))
                strSync_CurrentColorPreset = curcolorPreset;

            return strSync_CurrentColorPreset;
        }
    }
}