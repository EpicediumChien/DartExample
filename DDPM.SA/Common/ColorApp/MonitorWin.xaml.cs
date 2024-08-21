using DDPM.SA.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
        private string Pre_reqKey = string.Empty;//Dean 0626 fix SAST issue, remove static as recommend and set as private

        // 20240619 jim add - declare log variable
        private Logs _logs;

        // jim add 20240605
        private bool b_AUTO_ColorPresetConfig = true;//Dean 0626 fix SAST issue, remove static as recommend and set as private

        #region data region

        private List<AppCollectionData> _apps = new List<AppCollectionData>();
        private AppStatusQuery? appStatus = null;//Dean 0626 fix SAST issue, remove static as recommend
        private List<ColorPresetSettings>? appconfigs = null;

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

            appStatus = AppStatusQuery.GetInstance();
            AppStatusQuery.SendValue += EventAppStatus_SendValue;
        }

        // jim add 20240605
        public void Set_AUTO_ColorPresetConfig(bool blAUTO)
        {
            b_AUTO_ColorPresetConfig = blAUTO;
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
            Pre_reqKey = string.Empty;
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
            text = "[ColorApp  ] " + text;
            Console.WriteLine(text);
        }

        private void EventAppStatus_SendValue(object? sender, EventArgs e)
        {
            if (b_AUTO_ColorPresetConfig)
            {
                ActiveWindowData data = null;
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
                }

                if (sender != null)
                {
                    //Get window data from active window's event
                    data = sender as ActiveWindowData;
                    tbWndName.Text = data.ActiveWindowTitle;
                    tbWndPID.Text = data.ActiveWindowProcessId.ToString();
                    tbWndModule.Text = data.ActiveWindowProcessModuleName;
                    string strFilePath = data.ActiveWindowFilePath;

                    writelog("EventAppStatus_SendValue ActiveWindowTitle = " + data.ActiveWindowTitle);
                    writelog("EventAppStatus_SendValue ActiveWindowProcessId = " + data.ActiveWindowProcessId.ToString());
                    writelog("EventAppStatus_SendValue ActiveWindowProcessModuleName = " + data.ActiveWindowProcessModuleName);
                    writelog("EventAppStatus_SendValue ActiveWindowFilePath = " + data.ActiveWindowFilePath);

                    //Get actived Monitor from actived window
                    screen = Screen.FromHandle(data.ActiveWindowHandle);

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
                            //writelog("index < 0 -1");
                            return;
                        }
                        //writelog("index < 0 -2 ");
                    }

                    reqAppName = _apps[index].AppName;

                    //Trace.WriteLine("reqAppName = " + reqAppName);

                    bool isDesktop = _apps[index].AppType.Equals("Desktop"); //besides are UWP

                    //Trace.WriteLine("isDesktop  = " + isDesktop);

                    //string tmp = string.Empty;// used for UI display

                    // jim add 20240809
                    if (appconfigs != null)
                        appconfigs.Clear();

                    appconfigs = ddmLib.ReadColorPresetSettings().Result;

                    //Trace.WriteLine("appconfigs.Count = " + appconfigs.Count.ToString());

                    foreach (var config in appconfigs)
                    {
                        if (config.AppInfo == null || config.AppInfo.Count <= 0)
                        {
                            //Trace.WriteLine("config.AppInfo.Count = " +  config.AppInfo.Count.ToString());
                            continue;
                        }

                        //Check if actived monitor has its color preset section in config file
                        if (actived_mi.edid.ModelName.Trim().IndexOf(config.DeviceInfo.ModelName.Trim()) >= 0 &&
                            actived_mi.edid.SerialNumber.Trim() == config.DeviceInfo.SerialNumber.Trim())
                        {
                            if (config.RunType != (int)ColorPresetRunType.Auto)
                            {
                                writelog("config.RunType  is not ColorPresetRunType.Auto");
                                //Trace.WriteLine("config.RunType  is not ColorPresetRunType.Auto");
                                break;
                            }

                            string reqKey = string.Empty;

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
                                        reqKey = config.AppInfo["Desktop Application"].ColorPresetName.Trim();
                                    }
                                    else
                                    {
                                        return;
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
                                        return;
                                    }
                                }
                            }
                            else
                            {
                                reqKey = (config.AppInfo[reqAppName]).ColorPresetName.Trim();
                                writelog("reqKey = " + reqKey);
                            }

                            if (string.IsNullOrEmpty(reqKey))
                            {
                                writelog("reqKey is string.IsNullOrEmpty");
                                //Trace.WriteLine("reqKey is string.IsNullOrEmpty");
                                return;
                            }

                            if (!Pre_reqKey.Equals(reqKey,StringComparison.OrdinalIgnoreCase))
                            {
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

        public bool set_monitor_preset_by_request_key(MonitorInfo actived_mi, string reqKey, out string outmsg, bool isDrawOSD = true)
        {
            if (string.IsNullOrEmpty(reqKey))
            {
                outmsg = string.Format($"SetVCP] {actived_mi.AliasDeviceName}, null request key!");
                writelog("[set_monitor_preset_by_request_key] reqKey is string.IsNullOrEmpty");
                return false;
            }

            outmsg = string.Format($"[SetVCP] {actived_mi.AliasDeviceName}, SetVCP preset:{reqKey} OK!");

            // 20240619 jim modify
            //bool bi = ddmLib.WriteColorPreset_AUTO("0", actived_mi, reqKey).Result;
            bool bi = ddmLib.WriteColorPreset_AUTO(actived_mi, reqKey).Result;

            return true;
        }
    }
}