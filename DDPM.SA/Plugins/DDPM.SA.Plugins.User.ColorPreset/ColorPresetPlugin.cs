#region LicenceHeader

//
// Copyright © 2024, Dell Inc., All Rights Reserved.
// This material is confidential and a trade secret.  Permission to use this
// work for any purpose must be obtained in writing from Dell Inc.
//
// ColorPresetPlugin.cs created on 28/03/2024T09:50 AM
//

#endregion

using DDPM.MonitorBorker;
using DDPM.SA.Common;
using DDPM.SA.Common.Method;
using DDPM.ShowOSD;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.Common.Annotations;
using Dell.Client.Framework.Common.Extensions;
using Dell.Client.Framework.Common.PluginConditions;
using Dell.Client.Framework.Interfaces;
using Microsoft;
using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Policy;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using VcpCore.Common;
using DDPM.SA.Common.Settings;
using DDPM.SA.Common.Security;
using DDPM.SA.Resources.Helper;
using Microsoft.Toolkit.Uwp.Notifications;

namespace ColorPreset.Plugins
{
    [Plugin(DDPM.SA.Common.IDs.DDPM_COLOR_PRESET_PLUGIN_ID, pluginName, PluginOrderGroupType.Core, Version = pluginVersion)]
    [Descriptor(Description = pluginDescription)]
    [Publisher(Name = publisherCompany, Website = publisherWebsite, Support = publisherSupport)]
    [PublishedUnelevatedInterface(new[] { typeof(IColorPresetSA) })]
    [PluginRequires(Id = DDPM.SA.Common.IDs.Device_Manager_Plugin_ID, Version = "1.0.0", AllowDynamicResolving = true)]
    public class ColorPresetPlugin : BaseAgentPlugin, IColorPresetSA, IDisposableObservable
    {
        #region Private Members

        private const string pluginName = "ColorPresetPlugin";
        private const string pluginVersion = "1.0.0";
        private const string pluginDescription = "This plugin implements ColorPreset Plugin.";
        private const string publisherCompany = "Dell Technologies";
        private const string publisherWebsite = "https://www.dell.com";
        private const string publisherSupport = "This plugin implements ColorPreset Plugin.";

        private IAgent _agent;
        public const string PluginLogId = "ColorPreset";

        private Dictionary<string, InstalledAppInfo> _AllAppData = new Dictionary<string, InstalledAppInfo>();
        private List<string> _supported_preset = new List<string>();

        private List<string> HDR_ColorPresetNameList = new List<string>() {
            "Standard HDR",
            "Movie HDR",
            "Game HDR",
            "Vivid HDR",
            "Desktop",
            "Reference",
            "Multiscreen Match",
            "DisplayHDR",
            "HDR10",
            "HLG",
            "Custom Color HDR",
            "HDR Peak 1000"
        };
        private List<string> ColorPresetSupportList = new List<string>();
        private List<string> ColorPresetSupportList_ = new List<string>();

        //20240829 Jim move to here 20240829
        private ISettingsManagerDev _SettingsPlugin_internal;

        private MainWindow? MonitorBorkerWin = null;
        private Thread? newWindowThread_AutoSetColorPresetForMonitorConfig = null;

        //20240830 Jim add
        IIC_Metadata _ICC_Metadata = new IIC_Metadata();
        private Download? download = null;

        //20240905 Jim add
        private MonitorInfo Active_monitorInfo = null;
        public RegistryMonitor_NightLight registryMonitor_NightLight = null;
        public RegistryMonitor_NightLightScheduler registryMonitor_NightLight_Scheduler = null;
        public RegistryMonitor_ICC registryMonitor_ICC = null;

        //20250122 Jim add for PIMS-314617
        bool Active_SmartHDR_ON = false;
        private bool userClosedPopup = false;

        /// <summary>
        /// Colorpreset Manual change event，return Colorpreset name
        /// </summary>
        public event EventHandler<string>? Coloreset_manual_ChangeEvent;
        public event EventHandler<string>? NightLightStatus_ChangeEvent;

        private IDeviceManagerSA _DeviceManagerPlugin_SA = null;
        private CertificateCheck _certChecker = null;

        private enum log_type
        {
            info = 0,
            error
        }

        private static ShowOSDWin OsdWin = null;
        private string iconFolderPath = string.Empty;
        private VcpCore.Common.Logs _logs;

        #endregion

        #region Constructor

        public ColorPresetPlugin(IAgent agent) : base(agent, PluginLogId)
        {
            _agent = agent;
            _logs ??= new Logs(Log, PluginLogId);
            writelog("ColorPresetPlugin constructor ...");

            LoadInstalledAppList(true);
            _certChecker = new CertificateCheck(_logs);
        }

        #endregion

        #region Overriding methods

        protected override void OnPluginStarting()
        {
            _agent.PluginManager.PluginsStarted += PluginManagerOnPluginsStarted;

            PluginCondition = new PluginStartedCondition();

            writelog("ColorPreset plugin started");
        }

        #endregion

        #region IColorPresetSA implementation

        public void SetAppIconFolder(string folder_path)
        {
            if (string.IsNullOrEmpty(folder_path))
                return;

            iconFolderPath = folder_path;
        }

        public Task<Dictionary<string, InstalledAppInfo>> GetInstalledAppsList(bool isReload = false)
        {
            LoadInstalledAppList(isReload);
            return System.Threading.Tasks.Task.FromResult(_AllAppData);
        }

        public int get_index_of_json_config_for_cur_monitor(MonitorInfo mo)
        {
            writelog("ColorPresetPlugin get_index_of_json_config_for_cur_monitor requested ...");

            int index = -1;

            if (Test_AddAppCollectionData.GetInstance()._monitorConfigs != null)
            {
                // chech if ModelName and SerialNumber is null
                if (Test_AddAppCollectionData.GetInstance()._monitorConfigs.Count > 0)
                {
                    for (int i = 0; i < Test_AddAppCollectionData.GetInstance()._monitorConfigs.Count; i++)
                    {
                        if (String.IsNullOrEmpty(Test_AddAppCollectionData.GetInstance()._monitorConfigs[i].ModelName))
                            return -1;
                    }
                }

                index = Test_AddAppCollectionData.GetInstance()._monitorConfigs.FindIndex(x =>
                                                      x.ModelName.Trim() == mo.edid.ModelName.Trim() &&
                                                      x.SerialNumber.Trim() == mo.edid.SerialNumber.Trim());

                if (index == -1)
                {
                    // chech if ModelName and ServiceTag is null
                    if (Test_AddAppCollectionData.GetInstance()._monitorConfigs.Count > 0)
                    {
                        for (int i = 0; i < Test_AddAppCollectionData.GetInstance()._monitorConfigs.Count; i++)
                        {
                            if (String.IsNullOrEmpty(Test_AddAppCollectionData.GetInstance()._monitorConfigs[i].ModelName))
                                return -1;

                            if (String.IsNullOrEmpty(Test_AddAppCollectionData.GetInstance()._monitorConfigs[i].ServiceTag))
                                return -1;
                        }
                    }

                    index = Test_AddAppCollectionData.GetInstance()._monitorConfigs.FindIndex(x =>
                                               x.ModelName.Trim() == mo.edid.ModelName.Trim() &&
                                               x.ServiceTag.Trim() == mo.edid.ServiceTag.Trim());
                }
            }
            return index;
        }

        public ColorPresetSettings get_cur_monitor_preset_config(MonitorInfo mo, List<ColorPresetSettings> config)
        {
            writelog("ColorPresetPlugin get_cur_monitor_preset_config requested ...");

            Test_AddAppCollectionData.GetInstance()._monitorConfigs = config;

            int index = get_index_of_json_config_for_cur_monitor(mo);

            if (index < 0)
            {
                writelog("ColorPresetPlugin get_cur_monitor_preset_config, create new to config for current monitor");
                //data not exist, create new to config for current monitor
                Test_AddAppCollectionData.GetInstance()._monitorConfigs.Add(new ColorPresetSettings()
                {
                    ModelName = mo.edid.ModelName,
                    SerialNumber = mo.edid.SerialNumber,
                    ServiceTag = mo.edid.ServiceTag,
                    RunType = (int)ColorPresetRunType.Manual,
                    AppInfo = new Dictionary<string, ColorPresetSettings_AppInfo>(),
                    ColorForManual = 0,
                    ColorManagement_Status = (int)ColorManagementStatus.Off,
                    ColorManagement_RunType = (int)ColorManagementRunType.Off
                });

                index = get_index_of_json_config_for_cur_monitor(mo);

                Test_AddAppCollectionData.GetInstance()._monitorConfigs[index].AppInfo.Add("Desktop Application", new ColorPresetSettings_AppInfo()
                {
                    Color = 0,
                    HDRColor = -1,
                    IconName = "Assets/palette.png",
                });

                Test_AddAppCollectionData.GetInstance()._monitorConfigs[index].AppInfo.Add("UWP Application", new ColorPresetSettings_AppInfo()
                {
                    Color = 0,
                    HDRColor = -1,
                    IconName = "Assets/palette.png",
                });
            }
            return Test_AddAppCollectionData.GetInstance()._monitorConfigs[index];
        }

        public Task<List<ColorPresetSettings>> AddColorPresetForMonitorConfig(MonitorInfo mo, string AppName, string ColorPreset_Name, string supported_preset, List<ColorPresetSettings> config, bool SmartHDR_ON = false)
        {
            writelog("ColorPresetPlugin AddColorPresetForMonitorConfig requested ...");

            ColorPresetSettings temp = get_cur_monitor_preset_config(mo, config);

            if (temp.AppInfo.Count <= 0)
            {
                //Default items
                temp.AppInfo.Add("Desktop Application", new ColorPresetSettings_AppInfo()
                {
                    Color = 0,
                    HDRColor = -1,
                    IconName = "Assets/palette.png",
                });
                temp.AppInfo.Add("UWP Application", new ColorPresetSettings_AppInfo()
                {
                    Color = 0,
                    HDRColor = -1,
                    IconName = "Assets/palette.png",
                });
            }

            Test_AddAppCollectionData.GetInstance()._monitorConfigs = config;

            int index = get_index_of_json_config_for_cur_monitor(mo);

            if (index < 0)
            {
                //data not exist, create new to config for current monitor
                Test_AddAppCollectionData.GetInstance()._monitorConfigs.Add(new ColorPresetSettings()
                {
                    ModelName = mo.edid.ModelName,
                    SerialNumber = mo.edid.SerialNumber,
                    ServiceTag = mo.edid.ServiceTag,
                    RunType = (int)ColorPresetRunType.Manual,
                    AppInfo = new Dictionary<string, ColorPresetSettings_AppInfo>(),
                    ColorForManual = 0,
                    ColorManagement_Status = (int)ColorManagementStatus.Off,
                    ColorManagement_RunType = (int)ColorManagementRunType.Off
                });

                index = get_index_of_json_config_for_cur_monitor(mo);
            }

            _supported_preset = ReadColorPreset(mo, supported_preset, SmartHDR_ON).Result;

            int index_AppPresetIdx = 0;

            if (_supported_preset.Count > 0)
            {
                index_AppPresetIdx = _supported_preset.FindIndex(x => x.StartsWith(ColorPreset_Name));

                if (index_AppPresetIdx <= 0)
                    index_AppPresetIdx = 0;
            }

            AppData be = Test_AddAppCollectionData.GetInstance().AppsList.FirstOrDefault(x => x.AppName == (AppName));

            KeyValuePair<string, InstalledAppInfo> kvp = _AllAppData.First(x => x.Value.AppName == (AppName));

            if (be != null)
            { }
            else
            {
                Test_AddAppCollectionData.GetInstance().AppsList.Add(new AppData
                {
                    AppIcon = kvp.Value.IconName,
                    AppName = AppName,
                    AppPresetIdx = index_AppPresetIdx,
                    SupportPreset = new List<string>(_supported_preset),
                });
            }

            if (!(Test_AddAppCollectionData.GetInstance()._monitorConfigs[index].AppInfo.ContainsKey(AppName)))
            {
                var nColorVCPCoreValue = GetColorVCPCoreValue(ColorPreset_Name).Result;

                Test_AddAppCollectionData.GetInstance()._monitorConfigs[index].AppInfo.Add(AppName, new ColorPresetSettings_AppInfo()
                {
                    Color = nColorVCPCoreValue,
                    HDRColor = -1,
                    IconName = kvp.Value.IconName,
                });
            }

            return System.Threading.Tasks.Task.FromResult(Test_AddAppCollectionData.GetInstance()._monitorConfigs);
        }

        public Task<List<ColorPresetSettings>> ChangeColorPresetForMonitorConfig(MonitorInfo mo, string AppName, string ColorPreset_Name, List<ColorPresetSettings> config)
        {
            writelog("ColorPresetPlugin AddColorPresetForMonitorConfig requested ...");

            Test_AddAppCollectionData.GetInstance()._monitorConfigs = config;

            int index_config = get_index_of_json_config_for_cur_monitor(mo);
            if (index_config >= 0)
            {
                var nColorVCPCoreValue = GetColorVCPCoreValue(ColorPreset_Name).Result;
                Test_AddAppCollectionData.GetInstance()._monitorConfigs[index_config].RunType = (int)ColorPresetRunType.Auto;
                Test_AddAppCollectionData.GetInstance()._monitorConfigs[index_config].AppInfo[AppName].Color = nColorVCPCoreValue;
            }

            return System.Threading.Tasks.Task.FromResult(Test_AddAppCollectionData.GetInstance()._monitorConfigs);
        }

        public Task<List<ColorPresetSettings>> DeleteColorPresetForMonitorConfig(MonitorInfo mo, string AppName, List<ColorPresetSettings> config)
        {
            writelog("ColorPresetPlugin DeleteColorPresetForMonitorConfig requested ...");

            Test_AddAppCollectionData.GetInstance()._monitorConfigs = config;

            int index_config = get_index_of_json_config_for_cur_monitor(mo);

            if (index_config >= 0)
            {
                Test_AddAppCollectionData.GetInstance()._monitorConfigs[index_config].RunType = (int)ColorPresetRunType.Auto;
                Test_AddAppCollectionData.GetInstance()._monitorConfigs[index_config].AppInfo.Remove(AppName);
            }

            return System.Threading.Tasks.Task.FromResult(Test_AddAppCollectionData.GetInstance()._monitorConfigs);
        }

        /// <summary>
        /// 啟動 MonitorBorker 執行抓前景active app name
        /// </summary>
        /// <param name="m"></param>
        // jim 20241207 modify for The DDPM color profile can not be applied by DDPM on Smart HDR mode.(Gaming monitor ex: AW2724DM)
        public void Launch_MonitorBorker(List<MonitorInfo> _AllInfoMonitors, MonitorInfo m, IDeviceManagerSA _DeviceManagerPlugin, bool Is_Game_DeviceName = false, bool SmartHDR_ON = false, List<string> ColorPresetSupportList = null)
        {
            writelog("ColorPresetPlugin Launch_MonitorBorker requested ...");

            _DeviceManagerPlugin_SA = _DeviceManagerPlugin;

            if (m != null)
            {
                var v = (MonitorInfo)m;

                System.Windows.Forms.Screen s = System.Windows.Forms.Screen.AllScreens.FirstOrDefault(x => x.DeviceName == v.DisplayName);

                if (s != null)
                {
                    // jim modify 20240605
                    if (MonitorBorkerWin == null)
                    {
                        MonitorBorkerWin = new MainWindow(_DeviceManagerPlugin, m, Log);

                        //Derek 2025/03/31
                        MonitorBorkerWin.Closed += MonitorBorkerWin_Closed;

                        MonitorBorkerWin.Show();
                        MonitorBorkerWin.Set_AllMonitors(_AllInfoMonitors); //jim 20241218 modify For PIMS-326072
                        //jim 20241207 modify for The DDPM color profile can not be applied by DDPM on Smart HDR mode.(Gaming monitor ex: AW2724DM)
                        MonitorBorkerWin.Set_AUTO_ColorPresetConfig(true, Is_Game_DeviceName, SmartHDR_ON, ColorPresetSupportList);
                    }
                    else
                    {
                        //MonitorBorkerWin.Close();
                    }
                }
            }
            return;
        }

        //Derek 2025/03/31
        private void ExitUIThread()
        {
            if (System.Windows.Threading.Dispatcher.CurrentDispatcher != null)
            {
                System.Windows.Threading.Dispatcher.CurrentDispatcher.InvokeShutdown();
            }
        }

        //Derek 2025/03/31
        private void MonitorBorkerWin_Closed(object sender, EventArgs e)
        {
            MonitorBorkerWin.Closed -= MonitorBorkerWin_Closed;

            ExitUIThread();
        }

        // jim 20241207 modify for The DDPM color profile can not be applied by DDPM on Smart HDR mode.(Gaming monitor ex: AW2724DM)
        public Task<bool> AutoSetColorPresetForMonitorConfig(List<MonitorInfo> _AllInfoMonitors, MonitorInfo mo, string on_off, ISettingsManagerDev _SettingsPlugin, IDeviceManagerSA _DeviceManagerPlugin, bool Is_Game_DeviceName = false, bool SmartHDR_ON = false, List<string> ColorPresetSupportList = null)
        {
            writelog("ColorPresetPlugin AutoSetColorPresetForMonitorConfig requested ...");

            _DeviceManagerPlugin_SA = _DeviceManagerPlugin;

            List<ColorPresetSettings> config = _SettingsPlugin.ReadColorPresetSettings().Result;

            if (on_off.Equals("ON", StringComparison.OrdinalIgnoreCase))
            {
                writelog("ColorPresetPlugin AutoSetColorPresetForMonitorConfig ON ...");

                Test_AddAppCollectionData.GetInstance()._monitorConfigs = config;

                int index = get_index_of_json_config_for_cur_monitor(mo);

                if (index >= 0)
                {
                    Test_AddAppCollectionData.GetInstance()._monitorConfigs[index].RunType = (int)ColorPresetRunType.Auto;
                }

                _SettingsPlugin.WriteColorPresetSettings(config);
                Task.Delay(100).Wait();

                if (newWindowThread_AutoSetColorPresetForMonitorConfig == null)
                {
                    // create a thread
                    newWindowThread_AutoSetColorPresetForMonitorConfig = new Thread(new ThreadStart(() =>
                    {
                        // create and show the window
                        // jim 20241207 modify for The DDPM color profile can not be applied by DDPM on Smart HDR mode.(Gaming monitor ex: AW2724DM)
                        Launch_MonitorBorker(_AllInfoMonitors, mo, _DeviceManagerPlugin, Is_Game_DeviceName, SmartHDR_ON, ColorPresetSupportList);

                        // start the Dispatcher processing
                        // 啟動消息循環
                        System.Windows.Threading.Dispatcher.Run();
                    }));

                    // set the apartment state
                    // 設定為單線程單元（STA），WPF需要STA模式
                    newWindowThread_AutoSetColorPresetForMonitorConfig.SetApartmentState(ApartmentState.STA);

                    // make the thread a background thread
                    newWindowThread_AutoSetColorPresetForMonitorConfig.IsBackground = true;

                    // start the thread
                    // 啟動執行緒
                    newWindowThread_AutoSetColorPresetForMonitorConfig.Start();
                }
                else
                {
                    // jim add 20240605
                    if (MonitorBorkerWin != null) // jim add 20240809
                    {
                        MonitorBorkerWin.Set_AllMonitors(_AllInfoMonitors); //jim 20241218 modify For PIMS-326072
                        // jim 20241207 modify for The DDPM color profile can not be applied by DDPM on Smart HDR mode.(Gaming monitor ex: AW2724DM)
                        MonitorBorkerWin.Set_AUTO_ColorPresetConfig(true, Is_Game_DeviceName, SmartHDR_ON, ColorPresetSupportList);
                    }
                }
            }
            else if (on_off.Equals("OFF", StringComparison.OrdinalIgnoreCase))
            {
                writelog("ColorPresetPlugin AutoSetColorPresetForMonitorConfig OFF ...");
                Test_AddAppCollectionData.GetInstance()._monitorConfigs = config;
                int index = get_index_of_json_config_for_cur_monitor(mo);

                if (index >= 0)
                {
                    Test_AddAppCollectionData.GetInstance()._monitorConfigs[index].RunType = (int)ColorPresetRunType.Manual;
                }

                _SettingsPlugin.WriteColorPresetSettings(config);
                Task.Delay(100).Wait();

                // jim modify 20240605
                if (newWindowThread_AutoSetColorPresetForMonitorConfig != null &&
                    MonitorBorkerWin != null) // jim add 20240809
                {
                    MonitorBorkerWin.Set_AllMonitors(_AllInfoMonitors); // // jim 20241218 modify for PIMS-326072
                    // jim 20241207 modify for The DDPM color profile can not be applied by DDPM on Smart HDR mode.(Gaming monitor ex: AW2724DM)
                    MonitorBorkerWin.Set_AUTO_ColorPresetConfig(false, Is_Game_DeviceName, SmartHDR_ON, ColorPresetSupportList);
                }
            }
            return System.Threading.Tasks.Task.FromResult(true);
        }

        public Task<bool> AutoColorManagementForMonitorConfig(MonitorInfo monitorInfo, string off_bymonitor_byhost, ISettingsManagerDev _SettingsPlugin, string ColorPreset_Name = "", string ICC_profile_Name = "")
        {
            writelog("ColorPresetPlugin AutoColorManagementForMonitorConfig requested ...");
            bool blRet = true;
            Active_monitorInfo = monitorInfo;
            List<ColorPresetSettings> config = _SettingsPlugin.ReadColorPresetSettings().Result;

            if (off_bymonitor_byhost.Equals("ON", StringComparison.OrdinalIgnoreCase))
            {
                writelog("ColorPresetPlugin AutoColorManagementForMonitorConfig ON ...");

                Test_AddAppCollectionData.GetInstance()._monitorConfigs = config;

                int index = get_index_of_json_config_for_cur_monitor(monitorInfo);

                if (index >= 0)
                {
                    Test_AddAppCollectionData.GetInstance()._monitorConfigs[index].ColorManagement_Status = (int)ColorManagementStatus.On;
                }

                _SettingsPlugin.WriteColorPresetSettings(config);
                Task.Delay(100).Wait();

                if (Test_AddAppCollectionData.GetInstance()._monitorConfigs[index].ColorManagement_RunType == (int)ColorManagementRunType.Bymonitor)
                {
                    if (_ICC_Metadata.Is_Support_ICC_DeviceName)
                    {
                        if (registryMonitor_ICC != null)
                        {
                            registryMonitor_ICC.Stop();
                            registryMonitor_ICC.RegChanged -= new EventHandler(OnRegChanged_ICC);
                            registryMonitor_ICC.Error -= new System.IO.ErrorEventHandler(OnError_ICC);

                            if (registryMonitor_ICC.IsMonitoring)
                                registryMonitor_ICC.Dispose();
                            registryMonitor_ICC = null;
                        }
                    }
                }
                else if (Test_AddAppCollectionData.GetInstance()._monitorConfigs[index].ColorManagement_RunType == (int)ColorManagementRunType.Byhost)
                {
                    if (_ICC_Metadata.Is_Support_ICC_DeviceName &&
                        _ICC_Metadata._match_ICC_DeviceName != null)
                    {
                        string keyName = string.Format("{0}\\{1}", "HKEY_CURRENT_USER", @"Software\Microsoft\Windows NT\CurrentVersion\ICM\ProfileAssociations\Display\{4d36e96e-e325-11ce-bfc1-08002be10318}");

                        if (registryMonitor_ICC == null)
                        {
                            try
                            {
                                writelog("Monitor ICC change initiate...");
                                registryMonitor_ICC = new RegistryMonitor_ICC(keyName);
                                registryMonitor_ICC.RegChanged += new EventHandler(OnRegChanged_ICC);
                                registryMonitor_ICC.Error += new System.IO.ErrorEventHandler(OnError_ICC);
                                registryMonitor_ICC.Start();
                                writelog("Monitor ICC change started");
                            }
                            catch (Exception ex)
                            {
                                writelog($"Monitor ICC change Exception, message: {ex.Message}");
                            }
                        }
                    }

                }

            }
            else if (off_bymonitor_byhost.Equals("OFF", StringComparison.OrdinalIgnoreCase))
            {
                writelog("ColorPresetPlugin AutoColorManagementForMonitorConfig OFF ...");

                Test_AddAppCollectionData.GetInstance()._monitorConfigs = config;

                int index = get_index_of_json_config_for_cur_monitor(monitorInfo);

                if (index >= 0)
                {
                    Test_AddAppCollectionData.GetInstance()._monitorConfigs[index].ColorManagement_Status = (int)ColorManagementStatus.Off;
                }

                _SettingsPlugin.WriteColorPresetSettings(config);
                Task.Delay(100).Wait();

                if (_ICC_Metadata.Is_Support_ICC_DeviceName &&
                    registryMonitor_ICC != null)
                {
                    registryMonitor_ICC.Stop();
                    registryMonitor_ICC.RegChanged -= new EventHandler(OnRegChanged_ICC);
                    registryMonitor_ICC.Error -= new System.IO.ErrorEventHandler(OnError_ICC);

                    if (registryMonitor_ICC.IsMonitoring)
                        registryMonitor_ICC.Dispose();
                    registryMonitor_ICC = null;
                }
            }
            else if (off_bymonitor_byhost.Equals("BYMONITOR", StringComparison.OrdinalIgnoreCase))
            {
                writelog("ColorPresetPlugin AutoColorManagementForMonitorConfig BYMONITOR ...");

                Test_AddAppCollectionData.GetInstance()._monitorConfigs = config;

                int index = get_index_of_json_config_for_cur_monitor(monitorInfo);

                if (index >= 0)
                {
                    Test_AddAppCollectionData.GetInstance()._monitorConfigs[index].ColorManagement_Status = (int)ColorManagementStatus.On;
                    Test_AddAppCollectionData.GetInstance()._monitorConfigs[index].ColorManagement_RunType = (int)ColorManagementRunType.Bymonitor;
                }

                _SettingsPlugin.WriteColorPresetSettings(config);
                Task.Delay(100).Wait();

                if (_ICC_Metadata.Is_Support_ICC_DeviceName)
                {
                    if (registryMonitor_ICC != null)
                    {
                        registryMonitor_ICC.Stop();
                        registryMonitor_ICC.RegChanged -= new EventHandler(OnRegChanged_ICC);
                        registryMonitor_ICC.Error -= new System.IO.ErrorEventHandler(OnError_ICC);

                        if (registryMonitor_ICC.IsMonitoring)
                            registryMonitor_ICC.Dispose();
                        registryMonitor_ICC = null;
                    }

                    int count = _ICC_Metadata._match_ICC_DeviceName.Count;

                    for (int i = 0; i < count; i++)
                    {
                        try
                        {
                            MonitorProfile.IntsallMonitorProfile(monitorInfo.DisplayName, _ICC_Metadata.strICC_Folder + _ICC_Metadata._match_ICC_DeviceName[i].File);
                        }
                        catch (Exception ex)
                        {
                            writelog($"IntsallMonitorProfile Exception {ex.Message.ToString()}");
                        }
                    }
                }
            }
            else if (off_bymonitor_byhost.Equals("BYHOST", StringComparison.OrdinalIgnoreCase))
            {
                writelog("ColorPresetPlugin AutoColorManagementForMonitorConfig BYHOST ...");

                Test_AddAppCollectionData.GetInstance()._monitorConfigs = config;

                int index = get_index_of_json_config_for_cur_monitor(monitorInfo);

                if (index >= 0)
                {
                    Test_AddAppCollectionData.GetInstance()._monitorConfigs[index].ColorManagement_Status = (int)ColorManagementStatus.On;
                    Test_AddAppCollectionData.GetInstance()._monitorConfigs[index].ColorManagement_RunType = (int)ColorManagementRunType.Byhost;
                }

                _SettingsPlugin.WriteColorPresetSettings(config);
                Task.Delay(100).Wait();

                if (_ICC_Metadata.Is_Support_ICC_DeviceName &&
                    _ICC_Metadata._match_ICC_DeviceName != null)
                {
                    int count = _ICC_Metadata._match_ICC_DeviceName.Count;

                    for (int i = 0; i < count; i++)
                    {
                        try
                        {
                            MonitorProfile.IntsallMonitorProfile(monitorInfo.DisplayName, _ICC_Metadata.strICC_Folder + _ICC_Metadata._match_ICC_DeviceName[i].File);
                        }
                        catch (Exception ex)
                        {
                            writelog($"IntsallMonitorProfile Exception {ex.Message.ToString()}");
                        }
                    }

                    string keyName = string.Format("{0}\\{1}", "HKEY_CURRENT_USER", @"Software\Microsoft\Windows NT\CurrentVersion\ICM\ProfileAssociations\Display\{4d36e96e-e325-11ce-bfc1-08002be10318}");

                    if (registryMonitor_ICC == null)
                    {
                        try
                        {
                            writelog("Monitor ICC change initiate...");
                            registryMonitor_ICC = new RegistryMonitor_ICC(keyName);
                            registryMonitor_ICC.RegChanged += new EventHandler(OnRegChanged_ICC);
                            registryMonitor_ICC.Error += new System.IO.ErrorEventHandler(OnError_ICC);
                            registryMonitor_ICC.Start();
                            writelog("Monitor ICC change started");

                        }
                        catch (Exception ex)
                        {
                            writelog($"Monitor ICC change Exception, message: {ex.Message}");
                        }
                    }
                }
            }
            return System.Threading.Tasks.Task.FromResult(blRet);
        }

        public void OnRegChanged_ICC(object sender, EventArgs e)
        {
            writelog("ColorPresetPlugin OnRegChanged_ICC requested ...");

            string Key_Profile_Name = string.Empty;

            try
            {
                if (Active_monitorInfo != null && Active_monitorInfo.DisplayName != null)
                {
                    Key_Profile_Name = MonitorProfile.GetMonitorProfile(Active_monitorInfo.DisplayName);
                }
            }
            catch (Exception ex)
            {
                writelog($"GetMonitorProfile Exception {ex.Message.ToString()}");
            }

            Trace.WriteLine($" Key_Profile_Name = {Key_Profile_Name}");

            int count = _ICC_Metadata._match_ICC_DeviceName.Count;

            for (int i = 0; i < count; i++)
            {
                if (string.Equals(Key_Profile_Name, _ICC_Metadata._match_ICC_DeviceName[i].File, StringComparison.OrdinalIgnoreCase))
                {
                    Trace.WriteLine($" _ICC_Metadata._match_ICC_DeviceName[i].File = {_ICC_Metadata._match_ICC_DeviceName[i].File}");
                    string[] separators = { "," };
                    string[] strICC_ColorPresets = _ICC_Metadata._match_ICC_DeviceName[i].ColorPreset.Split(separators, StringSplitOptions.None);

                    if (strICC_ColorPresets.Length > 0)
                    {
                        Trace.WriteLine($" strICC_ColorPresets = {strICC_ColorPresets[0]}");

                        writelog($"ColorPresetPlugin OnRegChanged_ICC ColorPreset = {strICC_ColorPresets[0]}");

                        // Jim 20250122 modify for PIMS-314617 - U2725QEt Wistron- P3:DDPM(Windows)-Color Management behavior is out of spec
                        string result = ColorPresetSupportList.FirstOrDefault(x => x == strICC_ColorPresets[0]);
                        if (result != null)
                        {
                            //found
                            Coloreset_manual_ChangeEvent?.AsyncFireAndForget(this, strICC_ColorPresets[0], System.Threading.CancellationToken.None);
                        }
                        else
                        {
                            if (Active_SmartHDR_ON)
                            {
                                if (string.Equals(strICC_ColorPresets[0], "DisplayHDR", StringComparison.OrdinalIgnoreCase) || string.Equals(strICC_ColorPresets[0], "Display HDR", StringComparison.OrdinalIgnoreCase))
                                {
                                    Coloreset_manual_ChangeEvent?.AsyncFireAndForget(this, strICC_ColorPresets[0], System.Threading.CancellationToken.None);
                                }
                                else
                                {
                                    PopupContentPackage popupContentPackage = new PopupContentPackage()
                                    {
                                        Title = Strings.Dell_Display_and_Peripheral_Manager0,
                                        Info = Strings.ICC_notification_SmartHDR0 + " " + Active_monitorInfo.modelName,
                                        IsInfo = true,
                                        IsOnlyUpdate = false,
                                        StayOpen = false,
                                        Timeout = 5,
                                    };

                                    CallPopup(this, popupContentPackage);
                                }
                            }
                            else
                            {
                                if (string.Equals(strICC_ColorPresets[0], "Standard", StringComparison.OrdinalIgnoreCase) || string.Equals(strICC_ColorPresets[0], "Native", StringComparison.OrdinalIgnoreCase))
                                {
                                    Coloreset_manual_ChangeEvent?.AsyncFireAndForget(this, strICC_ColorPresets[0], System.Threading.CancellationToken.None);
                                }
                                else if (string.Equals(strICC_ColorPresets[0], "Game", StringComparison.OrdinalIgnoreCase) || string.Equals(strICC_ColorPresets[0], "Game1", StringComparison.OrdinalIgnoreCase))
                                {
                                    Coloreset_manual_ChangeEvent?.AsyncFireAndForget(this, strICC_ColorPresets[0], System.Threading.CancellationToken.None);
                                }
                                else if (strICC_ColorPresets[0].Contains("Rec", StringComparison.OrdinalIgnoreCase) || strICC_ColorPresets[0].Contains("BT.", StringComparison.OrdinalIgnoreCase) || strICC_ColorPresets[0].Contains("709", StringComparison.OrdinalIgnoreCase) || strICC_ColorPresets[0].Contains("2020", StringComparison.OrdinalIgnoreCase))
                                {
                                    Coloreset_manual_ChangeEvent?.AsyncFireAndForget(this, strICC_ColorPresets[0], System.Threading.CancellationToken.None);
                                }
                                else
                                {
                                    PopupContentPackage popupContentPackage = new PopupContentPackage()
                                    {
                                        Title = Strings.Dell_Display_and_Peripheral_Manager0,
                                        Info = Strings.ICC_notification_NonSmartHDR0 + " " + Active_monitorInfo.modelName,
                                        IsInfo = true,
                                        IsOnlyUpdate = false,
                                        StayOpen = false,
                                        Timeout = 5,
                                    };

                                    CallPopup(this, popupContentPackage);
                                }
                            }
                        }
                    }
                    break;
                }
            }
            return;
        }

        public void StopRegistryMonitor()
        {
            writelog("ColorPresetPlugin StopRegistryMonitor requested ...");

            if (registryMonitor_ICC != null)
            {
                registryMonitor_ICC.Stop();
                registryMonitor_ICC.RegChanged -= new EventHandler(OnRegChanged_ICC);
                registryMonitor_ICC.Error -= new System.IO.ErrorEventHandler(OnError_ICC);
                registryMonitor_ICC = null;
            }
        }

        public Task<bool> CheckColorICCStatus()
        {
            writelog("ColorPresetPlugin CheckColorICCStatus requested ...");

            string keyName = string.Format("{0}\\{1}", "HKEY_CURRENT_USER", @"Software\Microsoft\Windows NT\CurrentVersion\ICM\ProfileAssociations\Display\{4d36e96e-e325-11ce-bfc1-08002be10318}");

            if (registryMonitor_ICC == null)
            {
                writelog("Monitor ICC change initiate...");
                registryMonitor_ICC = new RegistryMonitor_ICC(keyName);
                registryMonitor_ICC.RegChanged += new EventHandler(OnRegChanged_ICC);
                registryMonitor_ICC.Error += new System.IO.ErrorEventHandler(OnError_ICC);
                registryMonitor_ICC.Start();
                writelog("Monitor ICC change started");
                return System.Threading.Tasks.Task.FromResult(true);
            }

            return System.Threading.Tasks.Task.FromResult(false);
        }

        public Task<bool> StopRegistryMonitor_ICC()
        {
            writelog("ColorPresetPlugin StopRegistryMonitor_ICC requested ...");

            if (registryMonitor_ICC != null)
            {
                registryMonitor_ICC.Stop();
                registryMonitor_ICC.RegChanged -= new EventHandler(OnRegChanged_ICC);
                registryMonitor_ICC.Error -= new System.IO.ErrorEventHandler(OnError_ICC);
                registryMonitor_ICC = null;
                return System.Threading.Tasks.Task.FromResult(true);
            }

            return System.Threading.Tasks.Task.FromResult(false);
        }

        public void OnError_ICC(object sender, ErrorEventArgs e)
        {
            writelog("ColorPresetPlugin OnError_ICC requested ...");

            StopRegistryMonitor_ICC();
        }

        public Task<bool> StopRegistryMonitor_NightLight()
        {
            writelog("ColorPresetPlugin StopRegistryMonitor_NightLight requested ...");

            if (registryMonitor_NightLight != null)
            {
                registryMonitor_NightLight.Stop();
                registryMonitor_NightLight.RegChanged -= new EventHandler(OnRegChanged_NightLight);
                registryMonitor_NightLight.Error -= new System.IO.ErrorEventHandler(OnError_NightLight);
                registryMonitor_NightLight = null;
                return System.Threading.Tasks.Task.FromResult(true);
            }

            return System.Threading.Tasks.Task.FromResult(false);
        }

        public Task<bool> StopRegistryMonitor_NightLightScheduler()
        {
            writelog("ColorPresetPlugin StopRegistryMonitor_NightLightscheduler requested ...");

            if (registryMonitor_NightLight_Scheduler != null)
            {
                registryMonitor_NightLight_Scheduler.Stop();
                registryMonitor_NightLight_Scheduler.RegChanged -= new EventHandler(OnRegChanged_NightLightscheduler);
                registryMonitor_NightLight_Scheduler.Error -= new System.IO.ErrorEventHandler(OnError_NightLightscheduler);
                registryMonitor_NightLight_Scheduler = null;
                return System.Threading.Tasks.Task.FromResult(true);
            }

            return System.Threading.Tasks.Task.FromResult(false);
        }

        public Task<bool> CheckNightLightStatus()
        {
            writelog("ColorPresetPlugin CheckNightLightStatus requested ...");
            string keyName = string.Format("{0}\\{1}", "HKEY_CURRENT_USER", "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\CloudStore\\Store\\DefaultAccount\\Current\\default$windows.data.bluelightreduction.bluelightreductionstate\\windows.data.bluelightreduction.bluelightreductionstate");

            if (registryMonitor_NightLight == null)
            {
                writelog("Monitor NightLight Status change initiate...");
                registryMonitor_NightLight = new RegistryMonitor_NightLight(keyName);
                registryMonitor_NightLight.RegChanged += new EventHandler(OnRegChanged_NightLight);
                registryMonitor_NightLight.Error += new System.IO.ErrorEventHandler(OnError_NightLight);
                registryMonitor_NightLight.Start();
                writelog("Monitor NightLight Status change started");
                return System.Threading.Tasks.Task.FromResult(true);
            }

            return System.Threading.Tasks.Task.FromResult(false);
        }

        public Task<bool> CheckNightLightScheduler()
        {
            writelog("ColorPresetPlugin CheckNightLightScheduler requested ...");
            string keyName = string.Format("{0}\\{1}", "HKEY_CURRENT_USER", "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\CloudStore\\Store\\DefaultAccount\\Current\\default$windows.data.bluelightreduction.settings\\windows.data.bluelightreduction.settings");

            if (registryMonitor_NightLight_Scheduler == null)
            {
                writelog("Monitor NightLight Scheduler Status change initiate...");
                registryMonitor_NightLight_Scheduler = new RegistryMonitor_NightLightScheduler(keyName);
                registryMonitor_NightLight_Scheduler.RegChanged += new EventHandler(OnRegChanged_NightLightscheduler);
                registryMonitor_NightLight_Scheduler.Error += new System.IO.ErrorEventHandler(OnError_NightLightscheduler);
                registryMonitor_NightLight_Scheduler.Start();
                writelog("Monitor NightLight Status Scheduler change started");
                return System.Threading.Tasks.Task.FromResult(true);
            }

            return System.Threading.Tasks.Task.FromResult(false);
        }

        public void OnRegChanged_NightLight(object sender, EventArgs e)
        {
            SyncNightlightStatus();
            return;
        }

        public void OnRegChanged_NightLightscheduler(object sender, EventArgs e)
        {
            SyncNightlightSchedulerStatus();
            return;
        }

        // add jim 20240604
        public void OnError_NightLight(object sender, ErrorEventArgs e)
        {
            StopRegistryMonitor_NightLight();
        }

        public void OnError_NightLightscheduler(object sender, ErrorEventArgs e)
        {
            StopRegistryMonitor_NightLightScheduler();
        }

        public Task<bool> SyncNightlightStatus()
        {
            writelog("ColorPresetPlugin SyncNightlightStatus requested ...");
            RegistryKey localKey64 = RegistryKey.OpenBaseKey(Microsoft.Win32.RegistryHive.CurrentUser, RegistryView.Registry64);

            if (localKey64 != null)
            {
                RegistryKey registryKey = localKey64.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\CloudStore\\Store\\DefaultAccount\\Current\\default$windows.data.bluelightreduction.bluelightreductionstate\\windows.data.bluelightreduction.bluelightreductionstate", false);
                if (registryKey != null)
                {
                    object obj = registryKey?.GetValue("Data");
                    if (obj != null)
                    {
                        byte[] array = (byte[])obj;

                        if (array.Length >= 18)
                        {
                            int ch = array[18];

                            if (ch == 0x15)
                            {
                                NightLightStatus_ChangeEvent?.AsyncFireAndForget(this, "On", System.Threading.CancellationToken.None);

                                if (_DeviceManagerPlugin_SA != null && Active_monitorInfo != null)
                                {
                                    System.Threading.Tasks.Task.Run(() =>
                                    {
                                        _DeviceManagerPlugin_SA.Send_NightLightStatus_Telementry_SA(Active_monitorInfo, "On");
                                    });
                                }
                            }
                            else if (ch == 0x13)
                            {
                                NightLightStatus_ChangeEvent?.AsyncFireAndForget(this, "Off", System.Threading.CancellationToken.None);

                                if (_DeviceManagerPlugin_SA != null && Active_monitorInfo != null)
                                {
                                    System.Threading.Tasks.Task.Run(() =>
                                    {
                                        _DeviceManagerPlugin_SA.Send_NightLightStatus_Telementry_SA(Active_monitorInfo, "Off");
                                    });
                                }
                            }
                        }
                    }
                    else
                    {
                        NightLightStatus_ChangeEvent?.AsyncFireAndForget(this, "Off", System.Threading.CancellationToken.None);

                        if (_DeviceManagerPlugin_SA != null && Active_monitorInfo != null)
                        {
                            System.Threading.Tasks.Task.Run(() =>
                            {
                                _DeviceManagerPlugin_SA.Send_NightLightStatus_Telementry_SA(Active_monitorInfo, "Off");
                            });
                        }
                    }
                    registryKey.Close();
                }
                else
                {
                    NightLightStatus_ChangeEvent?.AsyncFireAndForget(this, "Off", System.Threading.CancellationToken.None);

                    if (_DeviceManagerPlugin_SA != null && Active_monitorInfo != null)
                    {
                        System.Threading.Tasks.Task.Run(() =>
                        {
                            _DeviceManagerPlugin_SA.Send_NightLightStatus_Telementry_SA(Active_monitorInfo, "Off");
                        });
                    }
                }
                localKey64.Close();

                return System.Threading.Tasks.Task.FromResult(true);
            }
            else
            {
                NightLightStatus_ChangeEvent?.AsyncFireAndForget(this, "Off", System.Threading.CancellationToken.None);

                if (_DeviceManagerPlugin_SA != null && Active_monitorInfo != null)
                {
                    System.Threading.Tasks.Task.Run(() =>
                    {
                        _DeviceManagerPlugin_SA.Send_NightLightStatus_Telementry_SA(Active_monitorInfo, "Off");
                    });
                }

                return System.Threading.Tasks.Task.FromResult(true);
            }
        }

        private void SendNightLightSchedulerStatus(string status)
        {
            if (_DeviceManagerPlugin_SA != null && Active_monitorInfo != null)
            {
                Task.Run(() =>
                {
                    _DeviceManagerPlugin_SA.Send_NightLightschedulerStatus_Telementry_SA(Active_monitorInfo, status);
                });
            }
        }

        public Task<bool> SyncNightlightSchedulerStatus()
        {
            writelog("ColorPresetPlugin SyncNightlightSchedulerStatus requested ...");
            using (RegistryKey localKey64 = RegistryKey.OpenBaseKey(Microsoft.Win32.RegistryHive.CurrentUser, RegistryView.Registry64))
            {
                using (RegistryKey registryKey = localKey64.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\CloudStore\\Store\\DefaultAccount\\Current\\default$windows.data.bluelightreduction.settings\\windows.data.bluelightreduction.settings", false))
                {
                    if (registryKey != null)
                    {
                        object obj = registryKey?.GetValue("Data");
                        if (obj != null)
                        {
                            byte[] array = (byte[])obj;

                            //bool nightLightIsOn = false;

                            if (array.Length == 51)
                            {
                                SendNightLightSchedulerStatus("Off");
                            }
                            else if (array.Length == 53)
                            {
                                SendNightLightSchedulerStatus("On; Sunset_to_sunrise");
                            }
                            else if (array.Length == 56)
                            {
                                SendNightLightSchedulerStatus("On; custom_time");
                            }
                            else
                            {
                                writelog($"Length: {array.Length}");
                            }
                        }
                    }
                }
            }

            return System.Threading.Tasks.Task.FromResult(true);
        }

        [Obsolete] //will remove, Dean 20250318
        public Task<bool> SyncNightlightSchedulerStatus_old()
        {
            writelog("ColorPresetPlugin SyncNightlightSchedulerStatus requested ...");
            RegistryKey localKey64 = RegistryKey.OpenBaseKey(Microsoft.Win32.RegistryHive.CurrentUser, RegistryView.Registry64);

            if (localKey64 != null)
            {
                RegistryKey registryKey = localKey64.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\CloudStore\\Store\\DefaultAccount\\Current\\default$windows.data.bluelightreduction.settings\\windows.data.bluelightreduction.settings", false);
                if (registryKey != null)
                {
                    object obj = registryKey?.GetValue("Data");
                    if (obj != null)
                    {
                        byte[] array = (byte[])obj;

                        //bool nightLightIsOn = false;

                        if (array.Length == 51)
                        {

                            if (_DeviceManagerPlugin_SA != null && Active_monitorInfo != null)
                            {
                                System.Threading.Tasks.Task.Run(() =>
                                {
                                    _DeviceManagerPlugin_SA.Send_NightLightschedulerStatus_Telementry_SA(Active_monitorInfo, "Off");
                                });
                            }

                        }
                        else if (array.Length == 53)
                        {

                            if (_DeviceManagerPlugin_SA != null && Active_monitorInfo != null)
                            {
                                System.Threading.Tasks.Task.Run(() =>
                                {
                                    _DeviceManagerPlugin_SA.Send_NightLightschedulerStatus_Telementry_SA(Active_monitorInfo, "On; Sunset_to_sunrise");
                                });
                            }

                        }
                        else if (array.Length == 56)
                        {

                            if (_DeviceManagerPlugin_SA != null && Active_monitorInfo != null)
                            {
                                System.Threading.Tasks.Task.Run(() =>
                                {
                                    _DeviceManagerPlugin_SA.Send_NightLightschedulerStatus_Telementry_SA(Active_monitorInfo, "On; custom_time");
                                });
                            }

                        }
                    }
                    registryKey.Close();
                }
                localKey64.Close();
            }

            return System.Threading.Tasks.Task.FromResult(true);
        }

        public Task<string> GetAutoColorPresetStatus(MonitorInfo mo, ISettingsManagerDev _SettingsPlugin)
        {
            writelog("ColorPresetPlugin GetAutoColorPresetStatus requested ...");

            List<ColorPresetSettings> config = _SettingsPlugin.ReadColorPresetSettings().Result;

            Test_AddAppCollectionData.GetInstance()._monitorConfigs = config;

            int index = get_index_of_json_config_for_cur_monitor(mo);

            if (index >= 0)
            {
                if (Test_AddAppCollectionData.GetInstance()._monitorConfigs[index].RunType == (int)ColorPresetRunType.Auto)
                    return System.Threading.Tasks.Task.FromResult("ON");
                else
                    return System.Threading.Tasks.Task.FromResult("OFF");
            }

            return System.Threading.Tasks.Task.FromResult("OFF");
        }

        public Task<string> GetColorManagementStatus(MonitorInfo mo, ISettingsManagerDev _SettingsPlugin)
        {
            writelog("ColorPresetPlugin GetColorManagementStatus requested ...");

            List<ColorPresetSettings> config = _SettingsPlugin.ReadColorPresetSettings().Result;

            Test_AddAppCollectionData.GetInstance()._monitorConfigs = config;

            int index = get_index_of_json_config_for_cur_monitor(mo);

            if (index >= 0)
            {
                if (Test_AddAppCollectionData.GetInstance()._monitorConfigs[index].ColorManagement_Status == (int)ColorManagementStatus.Off)
                    return System.Threading.Tasks.Task.FromResult("OFF");
                else if (Test_AddAppCollectionData.GetInstance()._monitorConfigs[index].ColorManagement_RunType == (int)ColorManagementRunType.Bymonitor)
                    return System.Threading.Tasks.Task.FromResult("BYMONITOR");
                else if (Test_AddAppCollectionData.GetInstance()._monitorConfigs[index].ColorManagement_RunType == (int)ColorManagementRunType.Byhost)
                    return System.Threading.Tasks.Task.FromResult("BYHOST");
            }

            return System.Threading.Tasks.Task.FromResult("OFF");
        }

        public void ShowOSD_ColoPreset(MonitorInfo m, string strMsg, bool is_ShowUI = true, bool is_AUTO = false)
        {
            MonitorInfo monitorInfo = m;
            writelog("ColorPresetPlugin ShowOSD_ColoPreset requested ...");

            Thread thread = new Thread(() =>
            {
                if (monitorInfo != null)
                {
                    System.Windows.Forms.Screen sreen = System.Windows.Forms.Screen.AllScreens.FirstOrDefault(x => x.DeviceName == monitorInfo.DisplayName);

                    if (sreen != null)
                    {
                        OsdWin = new ShowOSDWin(strMsg, 40);

                        var dpiXProperty = typeof(SystemParameters).GetProperty("DpiX", BindingFlags.NonPublic | BindingFlags.Static);
                        var varX = (int)dpiXProperty.GetValue(null, null);
                        double dpiX = (double)varX / (double)96;
                        try
                        {
                            OsdWin.Top = sreen.WorkingArea.Top / (double)dpiX;
                            OsdWin.Left = sreen.WorkingArea.Left / (double)dpiX;

                            double dbfactor = ((double)((double)40 / (double)96));
                            double dbscale = dbfactor * dpiX;

                            if (is_ShowUI)
                                OsdWin.Show();
                        }
                        catch (Exception)
                        {
                            OsdWin.Top = sreen.WorkingArea.Top / (double)dpiX;
                            OsdWin.Left = sreen.WorkingArea.Left / (double)dpiX;

                            double dbfactor = ((double)((double)40 / (double)96));
                            double dbscale = dbfactor * dpiX;

                            if (is_ShowUI)
                                OsdWin.Show();
                        }
                        finally
                        {
                            OsdWin = null;
                        }
                    }
                }
                // 啟動消息循環
                System.Windows.Threading.Dispatcher.Run();
            });

            // 設定為單線程單元（STA），WPF需要STA模式
            thread.SetApartmentState(ApartmentState.STA);

            // 啟動執行緒
            thread.Start();
        }

        public Task<List<string>> ReadColorPreset(MonitorInfo m, string vcp_capbilities, bool SmartHDR_ON = false)
        {
            //20250122 Jim add for PIMS-314617
            Active_SmartHDR_ON = SmartHDR_ON;
            Trace.WriteLine($" [ReadColorPreset]  vcp_capbilities = {vcp_capbilities}");

            int index = -1;
            writelog("ColorPresetPlugin ReadColorPreset requested ...");

            if (string.IsNullOrEmpty(vcp_capbilities))
            {
                ColorPresetSupportList_.Clear();
                return System.Threading.Tasks.Task.FromResult(ColorPresetSupportList_);
            }

            // 20240619 jim add
            if (!string.IsNullOrEmpty(vcp_capbilities))
            {
                var Capabilities = (JObject)JsonConvert.DeserializeObject(vcp_capbilities);
                if (Capabilities.ContainsKey("CapsDataMap"))
                {
                    var CapsDataMap = (JObject)Capabilities["CapsDataMap"];

                    // Jim 20241211 to fix PIMS-327396 - The DDPM color profile list is not matching exactly with OSD.(G2723H)
                    // Jim 20241211 to fix PIMS-326656 - The DDPM color profile list is not matching exactly with OSD. "Game1" not show in DDPM.(AW3225QF)
                    if (string.Equals(m.modelName, "G2723H", StringComparison.OrdinalIgnoreCase) || string.Equals(m.modelName, "AW3225QF", StringComparison.OrdinalIgnoreCase))
                    {
                        if (CapsDataMap.ContainsKey("Preset Modes Specific")) // VCP E2
                        {
                            if (CapsDataMap["Preset Modes Specific"].Type == JTokenType.Null)
                            {
                                ColorPresetSupportList_.Clear();
                                ColorPresetSupportList_.Add("Standard/Native");
                            }
                            else
                            {
                                try
                                {
                                    JArray colorrreset = (JArray)CapsDataMap["Preset Modes Specific"]; // VCP E2

                                    ColorPresetSupportList_.Clear();

                                    foreach (var tmp in colorrreset)
                                        ColorPresetSupportList_.Add(new string(tmp.ToString()));
                                }
                                catch (Exception ex)
                                {
                                    writelog($"[ReadColorPreset] collect colore presets from VCP E2 , message: {ex.Message}");
                                }
                            }
                        }
                    }

                    else if (CapsDataMap.ContainsKey("ColorPreset"))
                    {
                        if (CapsDataMap["ColorPreset"].Type == JTokenType.Null)
                        {
                            ColorPresetSupportList_.Clear();
                            ColorPresetSupportList_.Add("Standard/Native");
                        }
                        else
                        {
                            try
                            {
                                JArray colorrreset = (JArray)CapsDataMap["ColorPreset"];

                                ColorPresetSupportList_.Clear();

                                foreach (var tmp in colorrreset)
                                    ColorPresetSupportList_.Add(new string(tmp.ToString()));
                            }
                            catch (Exception ex)
                            {
                                writelog($"[ReadColorPreset] collect colore presets , message: {ex.Message}");
                            }

                            // PIMS-327395 jim 20241212 add
                            if (string.Equals(m.modelName, "G2724D", StringComparison.OrdinalIgnoreCase) ||
                                string.Equals(m.modelName, "G3223D", StringComparison.OrdinalIgnoreCase))
                                ColorPresetSupportList_.Add("sRGB");
                        }
                    }
                }
            }

            if (SmartHDR_ON)
            {
                List<string> common_ColorPreset = ColorPresetSupportList_.Intersect(HDR_ColorPresetNameList).ToList();

                ColorPresetSupportList_.Clear();

                foreach (string info in common_ColorPreset)
                {
                    ColorPresetSupportList_.Add(new string(info));
                }

                // PIMS-298376 jim 20241212 add
                if (string.Equals(m.modelName, "UP3221Q", StringComparison.OrdinalIgnoreCase))
                {
                    ColorPresetSupportList_.Add("Custom 1 / User 1");
                    ColorPresetSupportList_.Add("Custom 2 / User 2");
                    ColorPresetSupportList_.Add("Custom 3 / User 3");
                    ColorPresetSupportList_.Add("CAL1");
                    ColorPresetSupportList_.Add("CAL2");
                }

            }
            else
            {
                ColorPresetSupportList_.RemoveAll(r => HDR_ColorPresetNameList.Any(a => a == r));
            }

            index = 0;

            //Console.WriteLine("[" + m.AliasDeviceName + "] Color Preset SupportList : ");
            writelog($"ColorPresetPlugin ReadColorPreset [" + m.modelName + "] Color Preset SupportList : ");

            ColorPresetSupportList.Clear();

            foreach (var h in ColorPresetSupportList_)
            {
                var temp = Sync_ColorPresetName(m, h).Result;

                ColorPresetSupportList.Add(temp);

                //Console.WriteLine("[" + index.ToString() + "] : " + h);
                writelog($"ColorPresetPlugin ReadColorPreset [" + index.ToString() + "] : " + h);
                index++;
            }

            return System.Threading.Tasks.Task.FromResult(ColorPresetSupportList);
        }

        public Task<bool> WriteColorPreset(MonitorInfo m, string ColorPreset_Name, ISettingsManagerDev _SettingsPlugin = null, int colorPresetRunType = 0)
        {
            bool blRes = true;

            writelog("ColorPresetPlugin WriteColorPreset requested ...");

            if (_SettingsPlugin != null)
            {
                List<ColorPresetSettings> config = _SettingsPlugin.ReadColorPresetSettings().Result;

                Test_AddAppCollectionData.GetInstance()._monitorConfigs = config;

                int index = get_index_of_json_config_for_cur_monitor(m);


                if (index >= 0)
                {
                    if (colorPresetRunType == (int)ColorPresetRunType.Manual)
                    {
                        var nColorVCPCoreValue = GetColorVCPCoreValue(ColorPreset_Name).Result;

                        Test_AddAppCollectionData.GetInstance()._monitorConfigs[index].RunType = (int)ColorPresetRunType.Manual;
                        //Test_AddAppCollectionData.GetInstance()._monitorConfigs[index].PresetForManual = ColorPreset_Name;

                        Test_AddAppCollectionData.GetInstance()._monitorConfigs[index].ColorForManual = nColorVCPCoreValue;
                    }
                    else if (colorPresetRunType == (int)ColorPresetRunType.Auto)
                    {
                        var nColorVCPCoreValue = GetColorVCPCoreValue(ColorPreset_Name).Result;

                        Test_AddAppCollectionData.GetInstance()._monitorConfigs[index].RunType = (int)ColorPresetRunType.Auto;
                        //Test_AddAppCollectionData.GetInstance()._monitorConfigs[index].PresetForManual = ColorPreset_Name;

                        Test_AddAppCollectionData.GetInstance()._monitorConfigs[index].ColorForManual = nColorVCPCoreValue;
                    }
                }

                _SettingsPlugin.WriteColorPresetSettings(config);
                Task.Delay(100).Wait();

                if (index >= 0 &&
                    Test_AddAppCollectionData.GetInstance()._monitorConfigs[index].ColorManagement_Status == (int)ColorManagementStatus.On &&
                    Test_AddAppCollectionData.GetInstance()._monitorConfigs[index].ColorManagement_RunType == (int)ColorManagementRunType.Bymonitor)
                {
                    writelog($"ColorPreset plugin SetMonitorProfile ColorPreset_Name = {ColorPreset_Name}");
                    Trace.WriteLine($"ColorPreset plugin SetMonitorProfile ColorPreset_Name = {ColorPreset_Name}");

                    try
                    {
                        blRes = SetMonitorProfile(m, ColorPreset_Name).Result;
                        Trace.WriteLine($"SetMonitorProfile() blRes={blRes}");

                    }
                    catch (Exception ex)
                    {
                        writelog($"SetMonitorProfile Exception {ex.Message.ToString()}");
                    }
                }

            }

            return System.Threading.Tasks.Task.FromResult(blRes);
            //return Task.FromResult(Test_AddAppCollectionData.GetInstance()._monitorConfigs);
        }

        public Task<List<ColorPresetSettings>> WriteColorPreset_AUTO(MonitorInfo m, string ColorPreset_Name, List<ColorPresetSettings> config)
        {
            //if (Log != null)
            //{
            //    Log.Info($"WriteColorPreset_AUTO requested ...");
            //}

            writelog("ColorPresetPlugin WriteColorPreset_AUTO requested ...");

            Test_AddAppCollectionData.GetInstance()._monitorConfigs = config;

            int index = get_index_of_json_config_for_cur_monitor(m);

            if (index >= 0)
            {
                var nColorVCPCoreValue = GetColorVCPCoreValue(ColorPreset_Name).Result;

                Test_AddAppCollectionData.GetInstance()._monitorConfigs[index].RunType = (int)ColorPresetRunType.Auto;
                //Test_AddAppCollectionData.GetInstance()._monitorConfigs[index].PresetForManual = ColorPreset_Name;

                Test_AddAppCollectionData.GetInstance()._monitorConfigs[index].ColorForManual = nColorVCPCoreValue;
            }

            return System.Threading.Tasks.Task.FromResult(Test_AddAppCollectionData.GetInstance()._monitorConfigs);
        }

        public Task<string> GetColorPresetName(int Color_VCPCore_E2)
        {
            writelog("ColorPresetPlugin GetColorPresetName requested ...");

            // 判斷Key是否存在
            // 若存在，回傳True，將Key為Color_VCPCore_E2的Value，帶入tmp
            if (!VcpCodeList.VCPE2.TryGetValue(Color_VCPCore_E2, out string tmp))
            {
                return System.Threading.Tasks.Task.FromResult(string.Empty);
            }
            else
            {
                return System.Threading.Tasks.Task.FromResult(tmp);
            }
        }

        public Task<int> GetColorVCPCoreValue(string ColorPreset_Name)
        {
            writelog("ColorPresetPlugin GetColorVCPCoreValue requested ...");

            // 判斷Key是否存在
            // 若存在，回傳True，將Key為ColorPreset_Name的Value，帶入tmp
            if (!VcpCodeList.VCPE2_ref.TryGetValue(ColorPreset_Name, out int tmp))
            {
                return System.Threading.Tasks.Task.FromResult(-1);
            }
            else
            {
                return System.Threading.Tasks.Task.FromResult(tmp);
            }
        }

        public Task<string> Sync_ColorPresetName(MonitorInfo monitorInfo, string ColorPreset_Name)
        {
            writelog("ColorPresetPlugin Sync_ColorPresetName requested ...");
            Trace.WriteLine($" ColorPreset_Name = {ColorPreset_Name}");

            var colorPresetsUP32 = new Dictionary<string, string>
            {
                { "AdobeRGB1 (D65G2.2L250)", "Adobe RGB D65 G2.2 L160" },// add 1127
                { "AdobeRGB2 (D50G2.2L250)", "Adobe RGB D50 G2.2 L160" },// add 1127
                { "AdobeRGB1/Adobe RGB D65 G2.2 L160/Adobe RGB D65 G2.2 L250", "Adobe RGB D65 G2.2 L160" },// add 1127
                { "AdobeRGB2/Adobe RGB D50 G2.2 L160/Adobe RGB D50 G2.2 L250", "Adobe RGB D50 G2.2 L160" },// add 1127
                { "AdobeRGB1", "Adobe RGB D65 G2.2 L160" },
                { "AdobeRGB2", "Adobe RGB D50 G2.2 L160" },
                { "Rec.709 / BT.709", "BT.709 D65 BT1886 L100" },
                { "Rec.2020 / BT.2020", "BT.2020 D65 BT1886 L100" },
                { "sRGB", "sRGB D65 sRGB L120" }
            };

            var colorPresetsUP27 = new Dictionary<string, string>
            {
                { "AdobeRGB1 (D65G2.2L250)", "Adobe RGB D65 G2.2 L250" },// add 1127
                { "AdobeRGB2 (D50G2.2L250)", "Adobe RGB D50 G2.2 L250" },// add 1127
                { "AdobeRGB1/Adobe RGB D65 G2.2 L160/Adobe RGB D65 G2.2 L250", "Adobe RGB D65 G2.2 L250" },// add 1127
                { "AdobeRGB2/Adobe RGB D50 G2.2 L160/Adobe RGB D50 G2.2 L250", "Adobe RGB D50 G2.2 L250" },// add 1127
                { "AdobeRGB1", "Adobe RGB D65 G2.2 L250" },
                { "AdobeRGB2", "Adobe RGB D50 G2.2 L250" },
                { "sRGB", "sRGB D65 sRGB L250" },
                { "Rec.709 / BT.709", "BT.709 D65 BT1886 L100" },
                { "Rec.2020 / BT.2020", "BT.2020 D65 BT1886 L100" }
            };


            string strSync_ColorPreset_Name = string.Empty;

            int index = -1;

            if (!string.IsNullOrEmpty(monitorInfo.modelName))
            {
                // check Color Preset Strings Standard or Native
                if (monitorInfo.modelName.StartsWith("UP"))
                {
                    Trace.WriteLine($" ColorPreset_Name = {ColorPreset_Name}");

                    if (ColorPreset_Name == "Standard/Native")
                        strSync_ColorPreset_Name = "Native";

                    if (ColorPreset_Name == "DCI-P3")
                        strSync_ColorPreset_Name = "DCI P3 D65 G2.4 L100";
                }
                else
                {
                    if (ColorPreset_Name == "Standard/Native")
                        strSync_ColorPreset_Name = "Standard";
                }

                // check Color Preset Strings Custom 1/2/3 or User 1/2/3
                if (monitorInfo.modelName.StartsWith("UP3221Q") || (monitorInfo.modelName.StartsWith("UP32") && !monitorInfo.modelName.Contains("UP3218K")))
                {
                    if (ColorPreset_Name == "Custom 1 / User 1")
                        strSync_ColorPreset_Name = "User 1";
                    else if (ColorPreset_Name == "Custom 2 / User 2")
                        strSync_ColorPreset_Name = "User 2";
                    else if (ColorPreset_Name == "Custom 3 / User 3")
                        strSync_ColorPreset_Name = "User 3";
                }
                else
                {
                    if (ColorPreset_Name == "Custom 1 / User 1")
                        strSync_ColorPreset_Name = "Custom 1";
                    else if (ColorPreset_Name == "Custom 2 / User 2")
                        strSync_ColorPreset_Name = "Custom 2";
                    else if (ColorPreset_Name == "Custom 3 / User 3")
                        strSync_ColorPreset_Name = "Custom 3";
                }
            }
#if DEBUG
            foreach (string color in ColorPresetSupportList_)
            {
                Debug.WriteLine("color_ : " + color);
            }

            foreach (string color in ColorPresetSupportList)
            {
                Debug.WriteLine("color : " + color);
            }
#endif
            // check Color Preset Strings Game or Game1
            if (ColorPresetSupportList_.Count > 0)
                index = ColorPresetSupportList_.FindIndex(x => x == "Game2");

            if (index >= 0)
            {
                if (ColorPreset_Name == "Game/Game1")
                    strSync_ColorPreset_Name = "Game1";
            }
            else
            {
                if (ColorPreset_Name == "Game/Game1")
                    strSync_ColorPreset_Name = "Game";
            }

            // check Color Preset Strings Rec.709 or BT.709 / Rec.709 or BT.709
            string strFY = string.Empty;

            if (!string.IsNullOrEmpty(monitorInfo.modelName))
            {
                for (int i = 0; i < monitorInfo.modelName.Length; i++) // loop over the complete modelName
                {
                    if (Char.IsDigit(monitorInfo.modelName[i])) //check if the current char is digit
                    {
                        strFY = monitorInfo.modelName.Substring(i + 2, 2);
                        break;
                    }

                }
            }

            // check Color Preset Strings Rec.2020 or BT.2020 / Rec.2020 or BT.2020
            int numFY = 0;
            try
            {
                numFY = Int32.Parse(strFY);

                if (numFY <= 23)
                {
                    if (ColorPreset_Name == "Rec.709 / BT.709")
                        strSync_ColorPreset_Name = "Rec.709";

                    if (ColorPreset_Name == "Rec.2020 / BT.2020")
                        strSync_ColorPreset_Name = "Rec.2020";

                }
                else if (numFY >= 25)
                {
                    if (ColorPreset_Name == "Rec.709 / BT.709")
                        strSync_ColorPreset_Name = "BT.709";

                    if (ColorPreset_Name == "Rec.2020 / BT.2020")
                        strSync_ColorPreset_Name = "BT.2020";
                }
            }
            catch (FormatException e)
            {
                Log?.Error("check Color Preset Strings Rec.709 or BT.709 / Rec.2020 or BT.2020..." + e.Message.ToString());
            }


            if (monitorInfo.modelName.StartsWith("UP32") && !monitorInfo.modelName.Contains("UP3218K"))
            {
                if (colorPresetsUP32.TryGetValue(ColorPreset_Name, out var presetValue))
                {
                    strSync_ColorPreset_Name = presetValue;
                }
            }
            else if (monitorInfo.modelName.StartsWith("UP27"))
            {
                if (colorPresetsUP27.TryGetValue(ColorPreset_Name, out var presetValue))
                {
                    strSync_ColorPreset_Name = presetValue;
                }
            }

            if (System.String.IsNullOrEmpty(strSync_ColorPreset_Name))
                strSync_ColorPreset_Name = ColorPreset_Name;

            return System.Threading.Tasks.Task.FromResult(strSync_ColorPreset_Name);

        }

        //20250122 Jim add for PIMS-314617
        private void CallPopup(object o, PopupContentPackage popupContentPackage)
        {
            writelog("[CallPopup], Start.");
            // 將 popupContentPackage.Object 轉換成 JSON 字串
            string json = JsonConvert.SerializeObject(popupContentPackage.Object);
            //// 將 JSON 字串轉換成 FWUpdateInfoPackage 對象
            //FWUpdateInfoPackage fWUpdateInfoPackage = JsonConvert.DeserializeObject<FWUpdateInfoPackage>(json);
            //// 將 JSON 字串轉換成 SWUpdateInfoPackage 對象
            //SWUpdateInfoPackage sWUpdateInfoPackage = JsonConvert.DeserializeObject<SWUpdateInfoPackage>(json);
            string title = popupContentPackage.Title;
            string info = popupContentPackage.Info;
            bool isInfo = popupContentPackage.IsInfo;
            bool isOnlyUpdate = popupContentPackage.IsOnlyUpdate;
            if (!string.IsNullOrEmpty(json))
            {
                userClosedPopup = false;
                System.Threading.Tasks.Task.Run(async () =>
                {
                    ToastContentBuilder toastContentBuilder = new ToastContentBuilder();
                    // 將物件序列化為 JSON 字串
                    string jsonString = System.Text.Json.JsonSerializer.Serialize(json);
                    Console.WriteLine(jsonString);
                    if (!isInfo)
                    {
                        toastContentBuilder.AddArgument(title);
                        toastContentBuilder.AddText(title);
                        toastContentBuilder.AddText(info);
                        if (!isOnlyUpdate)
                        {
                            toastContentBuilder.AddButton(LangHelper.Instance["UpdateNow"], ToastActivationType.Background, "Update " + popupContentPackage.PopupType.ToString());
                            toastContentBuilder.AddButton(LangHelper.Instance["Defer"], ToastActivationType.Background, "Delay");
                        }
                        else
                        {
                            toastContentBuilder.AddButton(LangHelper.Instance["Ok"], ToastActivationType.Background, "Update");
                        }
                    }
                    else
                    {
                        toastContentBuilder.AddArgument(title);
                        toastContentBuilder.AddText(title);
                        toastContentBuilder.AddText(info);
                    }

                    toastContentBuilder.Show(); // 顯示Toast通知
                    writelog("[CallPopup], popup Show.");
                    Task.Delay(5000).Wait();
                    if (!userClosedPopup)
                    {
                        writelog("[CallPopup], is no user closed popup.");
                        if (!isInfo)
                        {
                            if (!isOnlyUpdate)
                            {
                                writelog("[CallPopup], go to DelayEvent.");
                                //DelayEvent(this, popupContentPackage.PopupType.ToString());
                            }
                            else
                            {
                                writelog("[CallPopup], go to UpdateEvent.");
                                //UpdateEvent(this, popupContentPackage.PopupType.ToString());
                            }
                        }
                    }
                });
            }
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
                    _agent.PluginManager.PluginsStarted -= PluginManagerOnPluginsStarted;
                    _agent = null;

                    //[Dispose] event
                    Coloreset_manual_ChangeEvent = null;
                    NightLightStatus_ChangeEvent = null;

                    //[Dispose] Abort and stop all threads
                    try
                    {
                        newWindowThread_AutoSetColorPresetForMonitorConfig?.Abort();
                    }
                    catch (Exception ex)
                    {
                        writelog($"[Dispose] newWindowThread_AutoSetColorPresetForMonitorConfig.Abort() error:{ex.Message}");
                    }
                    try
                    {
                        registryMonitor_NightLight?.Dispose();
                    }
                    catch (Exception ex)
                    {
                        writelog($"[Dispose] registryMonitor_NightLight.Stop() error:{ex.Message}");
                    }
                    registryMonitor_NightLight = null;
                    try
                    {
                        registryMonitor_NightLight_Scheduler.Dispose();
                    }
                    catch (Exception ex)
                    {
                        writelog($"[Dispose] registryMonitor_NightLight_Scheduler.Stop() error:{ex.Message}");
                    }
                    registryMonitor_NightLight_Scheduler = null;
                    try
                    {
                        registryMonitor_ICC?.Dispose();
                    }
                    catch (Exception ex)
                    {
                        writelog($"[Dispose] registryMonitor_ICC.Stop() error:{ex.Message}");
                    }
                    registryMonitor_ICC = null;

                    //[Dispose] function class
                    _certChecker = null;
                    _ICC_Metadata = null;
                    download = null;
                    Active_monitorInfo = null;

                    //[Dispose] data object
                    try
                    {
                        _supported_preset?.Clear();
                        _supported_preset = null;
                        _AllAppData?.Clear();
                        _AllAppData = null;
                        ColorPresetSupportList?.Clear();
                        ColorPresetSupportList = null;
                        ColorPresetSupportList_?.Clear();
                        ColorPresetSupportList_ = null;
                    }
                    catch (Exception ex)
                    {
                        writelog($"[Dispose] data object clear error:{ex.Message}");
                    }

                    //[Dispose] user control close or window close
                    try
                    {
                        MonitorBorkerWin?.Close();
                    }
                    catch (Exception ex)
                    {
                        writelog($"[Dispose] MonitorBorkerWin.Close() error:{ex.Message}");
                    }
                    MonitorBorkerWin = null;
                    try
                    {
                        OsdWin?.Close();
                    }
                    catch (Exception ex)
                    {
                        writelog($"[Dispose] OsdWin.Close() error:{ex.Message}");
                    }
                    OsdWin = null;

                    //[Dispose] plugin
                    _SettingsPlugin_internal = null;
                    _DeviceManagerPlugin_SA = null;
                }
                IsDisposed = true;
            }
            base.Dispose(disposing);
        }

        #endregion

        /// <summary>
        /// //
        /// </summary>
        /// <param name="text"></param>
        /// <param name="log_type">0 means info, others means error</param>
        private void writelog(string text,
            [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
            [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
            [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0,
            log_type log_type = log_type.info)
        {
            if (string.IsNullOrEmpty(text))
                text = "";

            text = $"[ColorPreset] {text}, Caller Name:{memberName}, Source Line {sourceLineNumber}";
            Console.WriteLine(text);
            if (Log != null)
            {
                if (log_type == log_type.info)
                    Log.Info(text);
                else
                    Log.Error(text);
            }
        }

        #region Event Handler

        private void PluginManagerOnPluginsStarted(object sender, PluginsStartedEventArgs e)
        {
            if (e == null)
                return;
            if (e.ChangedPlugins == null)
                return;
            if (e.ChangedPlugins.Any() == false)
                return;
        }

        private void LoadInstalledAppList(bool renew_data = false)
        {
            writelog("IColorPresetSA get LoadInstalledAppList request");
            if (_AllAppData == null || _AllAppData.Count == 0 || renew_data == true)
            {
                writelog("enter reflash procedure");
                AppsCollectShell appshell = new AppsCollectShell(iconFolderPath, Log);
                _AllAppData = appshell.FindAppsbyShell();
            }
            writelog($"appshell.FindAppsbyShell get app count({_AllAppData.Count})");
        }

        private IIC_Metadata RunDeserializeObject(string value)
        {
            writelog("[DownloadICCData] RunDeserializeObject()  requested ...");

            IIC_Metadata retList = new IIC_Metadata();

            try
            {
                retList._support_ICC_DeviceName = JsonConvert.DeserializeObject<Dictionary<string, List<ICC_SupportDeviceName>>>(value);
            }
            catch (System.Exception ex)
            {
                writelog("[DownloadICCData] RunDeserializeObject() error:" + ex.Message.ToString());

                return retList;
            }

            return retList;
        }

        // Return a byte array as a sequence of hex values.
        public static string BytesToString(byte[] bytes)
        {
            string result = "";
            foreach (byte b in bytes) result += b.ToString("x2");
            return result;
        }

        private bool CheckICC_JSON_Security(string filePath, out string strJson)
        {
            strJson = string.Empty;
            bool ret = false;
            writelog("[CheckICC_JSON_Security] :" + filePath);

            List<string> InfoPkey = new List<string>();
            if (_SettingsPlugin_internal != null)
            {
                InfoPkey = _SettingsPlugin_internal.GetInfos().Result;
            }
            if (InfoPkey == null || InfoPkey.Count == 0)
            {
                //if read info failed, load default key as well
                InfoPkey = new List<string>(DDPM.SA.Obfuscation.InfoHash.Info_Hash);
                //InfoPkey.Add(DDPM.SA.Obfuscation.InfoHash.Info_Hash);
            }
            string szInfo = string.Empty;
            ret = DDPM.SA.Common.Settings.DDPMFileSecurity.VerifyDDPMMetadata(Log, filePath, InfoPkey, out szInfo, out strJson);

            if (!string.IsNullOrEmpty(szInfo) && _SettingsPlugin_internal != null)
            {
                _SettingsPlugin_internal.AddInfo(szInfo);//pass info to settings manager and judge if new to add
            }

            return ret;
        }


        /// <summary>
        /// 從伺服端下載ICC.json檔，下載後會 Run Deserialize接續下載ICC profile .icm檔
        /// </summary>
        /// <param name="m">Monitor Info</param>
        /// <returns> Run Deserialize ICC.json後的 object   </returns>
        public Task<IIC_Metadata> DownloadICCData(string modelName, string displayName, bool blICCProfile = false, string savelPath = "", bool UpdateMetadata = false)
        {
            writelog("ColorPresetPlugin DownloadICCData requested ...");
            try
            {
                //if (_SettingsPlugin_internal == null)
                //{
                //    writelog("ColorPresetPlugin DownloadICCData SettingsPlugin initiate");
                //    _SettingsPlugin_internal = _SettingsPlugin;
                //}

                string strFilePath = string.Empty;
                string strReadJson = string.Empty;

                // ICC profiles mapping schema
                //FileStream fileStream;
                //FileStream fileStream_ICM;

                _ICC_Metadata.Is_Support_ICC_DeviceName = false;

                string strICC_Folder;
                if (string.IsNullOrEmpty(savelPath))
                {
                    strICC_Folder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Dell\\Dell Display and Peripheral Manager" + @"\ICC\";
                }
                else
                {
                    strICC_Folder = savelPath;
                }

                writelog($"DownloadICCData Folder = {strICC_Folder}");

                if (!Directory.Exists(strICC_Folder))
                {
                    Directory.CreateDirectory(strICC_Folder);
                    writelog("DownloadICCData Folder created.");
                }

                _ICC_Metadata.strICC_Folder = String.Format($"{strICC_Folder}");

                string url = string.Empty;
                download = new Download(_logs);
                string downloadInfo = string.Empty;

                //Jim 20250113 modify for ICC profile Production Server
                //ex: https://clientperipherals.dell.com/DDPM/ICC/icc_profile_sha256.json
                string Display_ICC_URL = Download.GetTestServerURL();//@$"https://clientperipherals.dell.com/DDPM/";
                string Display_ICC_URL_Folder = GlobalDefinitions.Display_ICC_URL_Folder;//@"ICC/";
                string str_url_prefix = Display_ICC_URL + Display_ICC_URL_Folder;

                url = str_url_prefix + @"icc_profile_sha256.json";

                if (!string.IsNullOrEmpty(url))
                {
                    string info = string.Empty;
                    if (!DDPMFileSecurity.IsFolderPathValid(strICC_Folder, out info))
                    {
                        writelog($"[DownloadICCData][IsFolderPathValid] {info}");
                        return System.Threading.Tasks.Task.FromResult(_ICC_Metadata);
                    }

                    strFilePath = Path.Combine(strICC_Folder, Path.GetFileName(url));
                    string Info;
                    writelog("ColorPresetPlugin DownloadICCData check file is exists go");
                    writelog($"ColorPresetPlugin DownloadICCData UpdateMetadata : {UpdateMetadata}");
                    if (UpdateMetadata || !System.IO.File.Exists(strFilePath))
                    {
                        writelog("ColorPresetPlugin DownloadICCData need download new metadata, go download");
                        if (!download.DownloadFile(url, strFilePath, out downloadInfo))
                        {
                            writelog($"[DownloadICCData] Download ICC Metadata failed = {downloadInfo}");
                            return System.Threading.Tasks.Task.FromResult(_ICC_Metadata);
                        }
                    }
                    writelog("ColorPresetPlugin DownloadICCData check file is exists done");
                    //Elsa Add Security
                    if (!DDPM.SA.Common.Settings.DDPMFileSecurity.IsFilePathValid(strFilePath, out Info))
                    {
                        writelog($"[DownloadICCData] {Info}");
                        return System.Threading.Tasks.Task.FromResult(_ICC_Metadata);
                    }

                    //strReadJson = string.Empty;
                    if (!CheckICC_JSON_Security(strFilePath, out strReadJson))
                    {
                        writelog($"[DownloadICCData] CheckICC_JSON_Security Fail. {strFilePath}");
                        return System.Threading.Tasks.Task.FromResult(_ICC_Metadata);
                    }

                    if (strReadJson.Length < 1)
                    {
                        using (var reader = new StreamReader(strFilePath))
                        {
                            strReadJson = reader.ReadToEnd();
                        }
                    }

                    if (strReadJson == string.Empty || strReadJson.Length == 0)
                        return System.Threading.Tasks.Task.FromResult(_ICC_Metadata);

                    try
                    {
                        _ICC_Metadata = RunDeserializeObject(strReadJson);
                        _ICC_Metadata.strICC_Folder = strICC_Folder;
                        _ICC_Metadata.Is_Support_ICC_DeviceName = false;
                    }
                    catch (System.Exception ex)
                    {
                        //Console.WriteLine("[DownloadICCData] RunDeserializeObject error:" + ex.Message.ToString());
                        writelog("[DownloadICCData] RunDeserializeObject error:" + ex.Message.ToString());
                    }

                    foreach (var kvp in _ICC_Metadata._support_ICC_DeviceName)
                    {
                        writelog($" Model name = {kvp.Key}");
                    }

                    writelog($"m.modelName = {modelName} ");

                    var lookup = _ICC_Metadata._support_ICC_DeviceName.FirstOrDefault(x => x.Key.Equals(modelName, StringComparison.OrdinalIgnoreCase));

                    if (lookup.Key != null)
                    {
                        writelog($"lookup.Key = {lookup.Key} ");
                        _ICC_Metadata._match_ICC_DeviceName = lookup.Value;
                        _ICC_Metadata.Is_Support_ICC_DeviceName = true;

                        writelog($"[DownloadICCData] DeviceName = {modelName} is Support ICC.");
                    }
                    else
                    {
                        _ICC_Metadata._match_ICC_DeviceName.Clear();
                        _ICC_Metadata.Is_Support_ICC_DeviceName = false;

                        writelog($"[DownloadICCData] DeviceName = {modelName} is not Support ICC.");
                    }

                    if (blICCProfile)
                    {
                        int count = _ICC_Metadata._match_ICC_DeviceName.Count;
                        str_url_prefix += modelName;
                        str_url_prefix += @"/";

                        //info = string.Empty;
                        for (int i = 0; i < count; i++)
                        {
                            //url = string.Empty;

                            url = str_url_prefix + _ICC_Metadata._match_ICC_DeviceName[i].File;

                            strFilePath = Path.Combine(strICC_Folder, Path.GetFileName(url));
                            //Bruce 02/10 if file is exists and sha is same no go to download
                            if (File.Exists(strFilePath) && _certChecker?.CheckFile_SHA256(strFilePath, _ICC_Metadata._match_ICC_DeviceName[i].SHA256, out info) == true && _certChecker?.CheckFile_SHA512(strFilePath, _ICC_Metadata._match_ICC_DeviceName[i].SHA512, out info) == true)
                            {
                                writelog($"[DownloadICCData] {_ICC_Metadata._match_ICC_DeviceName[i].File} icc profile is exists and not change");
                            }
                            else
                            {
                                writelog($"[DownloadICCData] {_ICC_Metadata._match_ICC_DeviceName[i].File} icc profile is no exists go download");
                                if (download.DownloadFile(url, strFilePath, out downloadInfo))
                                {
                                    bool? rst = _certChecker?.CheckFile_SHA256(strFilePath, _ICC_Metadata._match_ICC_DeviceName[i].SHA256, out info);
                                    if (rst == null || rst == false)
                                    {
                                        writelog($"[DownloadICCData] {_ICC_Metadata._match_ICC_DeviceName[i]} icc profile sha256 json = {_ICC_Metadata._match_ICC_DeviceName[i].SHA256} mismatch!");
                                        continue;
                                    }
                                    rst = _certChecker?.CheckFile_SHA512(strFilePath, _ICC_Metadata._match_ICC_DeviceName[i].SHA512, out info);
                                    if (rst == null || rst == false)
                                    {
                                        writelog($"[DownloadICCData] {_ICC_Metadata._match_ICC_DeviceName[i]} icc profile sha512 json = {_ICC_Metadata._match_ICC_DeviceName[i].SHA512} mismatch!");
                                        continue;
                                    }
                                    try
                                    {
                                        if (!MonitorProfile.IntsallMonitorProfile(displayName, strFilePath))
                                        {
                                            writelog($"[DownloadICCData] {_ICC_Metadata._match_ICC_DeviceName[i]} icc profile install failed!");
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        writelog($"[DownloadICCData] {_ICC_Metadata._match_ICC_DeviceName[i]} icc profile install failed! {ex.Message}");
                                    }
                                }
                                else
                                {
                                    writelog($"[DownloadICCData] download icc {_ICC_Metadata._match_ICC_DeviceName[i]} failed");
                                }
                            }
                        }
                    }
                }

                writelog("ColorPresetPlugin DownloadICCData exit ...");
                return System.Threading.Tasks.Task.FromResult(_ICC_Metadata);
            }
            catch (Exception ex)
            {
                writelog($"ColorPresetPlugin DownloadICCData Exception = {ex.Message.ToString()}");
                return System.Threading.Tasks.Task.FromResult(_ICC_Metadata);
            }
        }
        [Obsolete] //will remove older code, 20250318 Dean
        public Task<IIC_Metadata> DownloadICCData_old(MonitorInfo m, ISettingsManagerDev _SettingsPlugin, bool blICCProfile = false, string savelPath = "", bool UpdateMetadata = false)
        {
            writelog("ColorPresetPlugin DownloadICCData requested ...");
            try
            {
                if (_SettingsPlugin_internal == null)
                {
                    writelog("ColorPresetPlugin DownloadICCData SettingsPlugin initiate");
                    _SettingsPlugin_internal = _SettingsPlugin;
                }

                string strFilePath = string.Empty;
                string strReadJson = string.Empty;

                // ICC profiles mapping schema
                FileStream fileStream;
                FileStream fileStream_ICM;

                _ICC_Metadata.Is_Support_ICC_DeviceName = false;

                string strICC_Folder;
                if (string.IsNullOrEmpty(savelPath))
                {
                    strICC_Folder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Dell\\Dell Display and Peripheral Manager" + @"\ICC\";
                }
                else
                {
                    strICC_Folder = savelPath;
                }

                writelog($"DownloadICCData Folder = {strICC_Folder}");

                if (!Directory.Exists(strICC_Folder))
                {
                    Directory.CreateDirectory(strICC_Folder);
                    writelog("DownloadICCData Folder created.");
                }

                _ICC_Metadata.strICC_Folder = String.Format($"{strICC_Folder}");

                string url = string.Empty;
                download = new Download(_logs);
                string downloadInfo = string.Empty;

                //Jim 20250113 modify for ICC profile Production Server
                //ex: https://clientperipherals.dell.com/DDPM/ICC/icc_profile_sha256.json
                string Display_ICC_URL = Download.GetTestServerURL();//@$"https://clientperipherals.dell.com/DDPM/";
                string Display_ICC_URL_Folder = GlobalDefinitions.Display_ICC_URL_Folder;//@"ICC/";
                string str_url_prefix = Display_ICC_URL + Display_ICC_URL_Folder;

                url = str_url_prefix + @"icc_profile_sha256.json";

                if (!string.IsNullOrEmpty(url))
                {
                    string info = string.Empty;
                    if (!DDPMFileSecurity.IsFolderPathValid(strICC_Folder, out info))
                    {
                        writelog($"[DownloadICCData][IsFolderPathValid] {info}");
                        return System.Threading.Tasks.Task.FromResult(_ICC_Metadata);
                    }

                    strFilePath = Path.Combine(strICC_Folder, Path.GetFileName(url));
                    string Info;
                    writelog("ColorPresetPlugin DownloadICCData check file is exists go");
                    writelog($"ColorPresetPlugin DownloadICCData UpdateMetadata : {UpdateMetadata}");
                    if (UpdateMetadata || !System.IO.File.Exists(strFilePath))
                    {
                        writelog("ColorPresetPlugin DownloadICCData need download new metadata, go download");
                        if (!download.DownloadFile(url, strFilePath, out downloadInfo))
                        {
                            writelog($"[DownloadICCData] Download ICC Metadata failed = {downloadInfo}");
                            return System.Threading.Tasks.Task.FromResult(_ICC_Metadata);
                        }
                    }
                    writelog("ColorPresetPlugin DownloadICCData check file is exists done");
                    //Elsa Add Security
                    if (!DDPM.SA.Common.Settings.DDPMFileSecurity.IsFilePathValid(strFilePath, out Info))
                    {
                        writelog($"[DownloadICCData] {Info}");
                        return System.Threading.Tasks.Task.FromResult(_ICC_Metadata);
                    }

                    strReadJson = string.Empty;
                    if (!CheckICC_JSON_Security(strFilePath, out strReadJson))
                    {
                        writelog($"[DownloadICCData] CheckICC_JSON_Security Fail. {strFilePath}");
                        return System.Threading.Tasks.Task.FromResult(_ICC_Metadata);
                    }

                    if (strReadJson.Length < 1)
                    {
                        using (var reader = new StreamReader(strFilePath))
                        {
                            strReadJson = reader.ReadToEnd();
                        }
                    }

                    if (strReadJson == string.Empty || strReadJson.Length == 0)
                        return System.Threading.Tasks.Task.FromResult(_ICC_Metadata);

                    try
                    {
                        _ICC_Metadata = RunDeserializeObject(strReadJson);
                        _ICC_Metadata.strICC_Folder = strICC_Folder;
                        _ICC_Metadata.Is_Support_ICC_DeviceName = false;
                    }
                    catch (System.Exception ex)
                    {
                        //Console.WriteLine("[DownloadICCData] RunDeserializeObject error:" + ex.Message.ToString());
                        writelog("[DownloadICCData] RunDeserializeObject error:" + ex.Message.ToString());
                    }

                    foreach (var kvp in _ICC_Metadata._support_ICC_DeviceName)
                    {
                        writelog($" Model name = {kvp.Key}");
                    }

                    writelog($"m.modelName = {m.modelName} ");

                    var lookup = _ICC_Metadata._support_ICC_DeviceName.First(x => x.Key.Equals(m.modelName, StringComparison.OrdinalIgnoreCase));

                    writelog($"lookup.Key = {lookup.Key} ");

                    if (lookup.Key != null)
                    {
                        _ICC_Metadata._match_ICC_DeviceName = lookup.Value;
                        _ICC_Metadata.Is_Support_ICC_DeviceName = true;

                        writelog($"[DownloadICCData] DeviceName = {m.modelName} is Support ICC.");
                    }
                    else
                    {
                        _ICC_Metadata._match_ICC_DeviceName.Clear();
                        _ICC_Metadata.Is_Support_ICC_DeviceName = false;

                        writelog($"[DownloadICCData] DeviceName = {m.modelName} is not Support ICC.");
                    }

                    if (blICCProfile)
                    {
                        int count = _ICC_Metadata._match_ICC_DeviceName.Count;
                        str_url_prefix += m.modelName;
                        str_url_prefix += @"/";

                        info = string.Empty;
                        for (int i = 0; i < count; i++)
                        {
                            url = string.Empty;

                            url = str_url_prefix + _ICC_Metadata._match_ICC_DeviceName[i].File;

                            strFilePath = Path.Combine(strICC_Folder, Path.GetFileName(url));
                            //Bruce 02/10 if file is exists and sha is same no go to download
                            if (File.Exists(strFilePath) && _certChecker?.CheckFile_SHA256(strFilePath, _ICC_Metadata._match_ICC_DeviceName[i].SHA256, out info) == true && _certChecker?.CheckFile_SHA512(strFilePath, _ICC_Metadata._match_ICC_DeviceName[i].SHA512, out info) == true)
                            {
                                writelog($"[DownloadICCData] {_ICC_Metadata._match_ICC_DeviceName[i].File} icc profile is exists and not change");
                            }
                            else
                            {
                                writelog($"[DownloadICCData] {_ICC_Metadata._match_ICC_DeviceName[i].File} icc profile is no exists go download");
                                if (download.DownloadFile(url, strFilePath, out downloadInfo))
                                {
                                    bool? rst = _certChecker?.CheckFile_SHA256(strFilePath, _ICC_Metadata._match_ICC_DeviceName[i].SHA256, out info);
                                    if (rst == null || rst == false)
                                    {
                                        writelog($"[DownloadICCData] {_ICC_Metadata._match_ICC_DeviceName[i]} icc profile sha256 json = {_ICC_Metadata._match_ICC_DeviceName[i].SHA256} mismatch!");
                                        continue;
                                    }
                                    rst = _certChecker?.CheckFile_SHA512(strFilePath, _ICC_Metadata._match_ICC_DeviceName[i].SHA512, out info);
                                    if (rst == null || rst == false)
                                    {
                                        writelog($"[DownloadICCData] {_ICC_Metadata._match_ICC_DeviceName[i]} icc profile sha512 json = {_ICC_Metadata._match_ICC_DeviceName[i].SHA512} mismatch!");
                                        continue;
                                    }
                                    MonitorProfile.IntsallMonitorProfile(m.DisplayName, strFilePath);
                                }
                                else
                                {
                                    writelog($"[DownloadICCData] download icc {_ICC_Metadata._match_ICC_DeviceName[i]} failed");
                                }
                            }
                        }
                    }
                }

                writelog("ColorPresetPlugin DownloadICCData exit ...");
                return System.Threading.Tasks.Task.FromResult(_ICC_Metadata);
            }
            catch (Exception ex)
            {
                writelog($"ColorPresetPlugin DownloadICCData Exception = {ex.Message.ToString()}");
                return System.Threading.Tasks.Task.FromResult(_ICC_Metadata);
            }
        }


        /// <summary>
        ///  Set Monitor ICC color Profile
        /// </summary>
        /// <param name="m"> Monitor Info </param>
        /// <param name="ColorPreset_Name"> ColorPreset Name </param>
        /// <returns></returns>
        public Task<bool> SetMonitorProfile(MonitorInfo m, string ColorPreset_Name)
        {
            bool blRes = true;

            writelog("ColorPresetPlugin SetMonitorProfile requested ...");

            if (_ICC_Metadata._match_ICC_DeviceName != null)
            {
                int count = _ICC_Metadata._match_ICC_DeviceName.Count;

                for (int i = 0; i < count; i++)
                {
                    string[] separators = { "|" };
                    string[] strICC_ColorPresets = _ICC_Metadata._match_ICC_DeviceName[i].ColorPreset.Split(separators, StringSplitOptions.None);

                    if (string.Equals(ColorPreset_Name, "Standard", StringComparison.OrdinalIgnoreCase) || string.Equals(ColorPreset_Name, "Native", StringComparison.OrdinalIgnoreCase))
                    {
                        if (string.Equals(strICC_ColorPresets[0], "Standard", StringComparison.OrdinalIgnoreCase) || string.Equals(strICC_ColorPresets[0], "Native", StringComparison.OrdinalIgnoreCase))
                        {
                            try
                            {
                                blRes = MonitorProfile.SetMonitorProfile(m.DisplayName, _ICC_Metadata._match_ICC_DeviceName[i].File);
                                Trace.WriteLine($"MonitorProfile.SetMonitorProfile blRes={blRes}");
                            }
                            catch (Exception ex)
                            {
                                writelog($"SetMonitorProfile Exception {ex.Message.ToString()}");
                            }

                            writelog($"ColorPresetPlugin SetMonitorProfile = {_ICC_Metadata._match_ICC_DeviceName[i].File}");
                            Trace.WriteLine($"ColorPresetPlugin SetMonitorProfile = {_ICC_Metadata._match_ICC_DeviceName[i].File}");
                            break;
                        }
                    }
                    else if (string.Equals(ColorPreset_Name, "Game", StringComparison.OrdinalIgnoreCase) || string.Equals(ColorPreset_Name, "Game1", StringComparison.OrdinalIgnoreCase))
                    {
                        if (string.Equals(strICC_ColorPresets[0], "Game", StringComparison.OrdinalIgnoreCase) || string.Equals(strICC_ColorPresets[0], "Game1", StringComparison.OrdinalIgnoreCase))
                        {
                            try
                            {
                                blRes = MonitorProfile.SetMonitorProfile(m.DisplayName, _ICC_Metadata._match_ICC_DeviceName[i].File);
                                Trace.WriteLine($"MonitorProfile.SetMonitorProfile blRes={blRes}");
                            }
                            catch (Exception ex)
                            {
                                writelog($"SetMonitorProfile Exception {ex.Message.ToString()}");
                            }

                            writelog($"ColorPresetPlugin SetMonitorProfile = {_ICC_Metadata._match_ICC_DeviceName[i].File}");
                            Trace.WriteLine($"ColorPresetPlugin SetMonitorProfile = {_ICC_Metadata._match_ICC_DeviceName[i].File}");
                            break;
                        }
                    }
                    else if (string.Equals(ColorPreset_Name, "Rec.709 / BT.709", StringComparison.OrdinalIgnoreCase) || string.Equals(ColorPreset_Name, "Rec.709", StringComparison.OrdinalIgnoreCase) || string.Equals(ColorPreset_Name, "BT.709", StringComparison.OrdinalIgnoreCase) || string.Equals(ColorPreset_Name, "Rec.2020 / BT.2020", StringComparison.OrdinalIgnoreCase) || string.Equals(ColorPreset_Name, "Rec.2020", StringComparison.OrdinalIgnoreCase) || string.Equals(ColorPreset_Name, "BT.2020", StringComparison.OrdinalIgnoreCase))
                    {
                        if (strICC_ColorPresets[0].Contains("Rec", StringComparison.OrdinalIgnoreCase) || strICC_ColorPresets[0].Contains("BT.", StringComparison.OrdinalIgnoreCase) || strICC_ColorPresets[0].Contains("709", StringComparison.OrdinalIgnoreCase) || strICC_ColorPresets[0].Contains("2020", StringComparison.OrdinalIgnoreCase))
                        {
                            try
                            {
                                blRes = MonitorProfile.SetMonitorProfile(m.DisplayName, _ICC_Metadata._match_ICC_DeviceName[i].File);
                                Trace.WriteLine($"MonitorProfile.SetMonitorProfile blRes={blRes}");
                            }
                            catch (Exception ex)
                            {
                                writelog($"SetMonitorProfile Exception {ex.Message.ToString()}");
                            }

                            writelog($"ColorPresetPlugin SetMonitorProfile = {_ICC_Metadata._match_ICC_DeviceName[i].File}");
                            Trace.WriteLine($"ColorPresetPlugin SetMonitorProfile = {_ICC_Metadata._match_ICC_DeviceName[i].File}");
                            break;
                        }
                    }

                    if (string.Equals(ColorPreset_Name, strICC_ColorPresets[0], StringComparison.OrdinalIgnoreCase))
                    {
                        try
                        {
                            blRes = MonitorProfile.SetMonitorProfile(m.DisplayName, _ICC_Metadata._match_ICC_DeviceName[i].File);

                            Trace.WriteLine($"MonitorProfile.SetMonitorProfile blRes={blRes}");
                        }
                        catch (Exception ex)
                        {
                            writelog($"SetMonitorProfile Exception {ex.Message.ToString()}");
                        }

                        writelog($"ColorPresetPlugin SetMonitorProfile = {_ICC_Metadata._match_ICC_DeviceName[i].File}");
                        Trace.WriteLine($"ColorPresetPlugin SetMonitorProfile = {_ICC_Metadata._match_ICC_DeviceName[i].File}");
                        break;
                    }
                    else
                        blRes = false;

                }
            }

            writelog("ColorPresetPlugin SetMonitorProfile exit ...");
            return System.Threading.Tasks.Task.FromResult(blRes);
        }

        /// <summary>
        /// DDPM MonitorSettings Data Export for Color
        /// </summary>
        /// <param name="MonitorInfo"></param>
        /// <param name="_SettingsPlugin"></param>
        /// <returns></returns>
        public Task<ColorPresetSettings> Export(MonitorInfo MonitorInfo, ISettingsManagerDev _SettingsPlugin)
        {
            writelog("ColorPresetPlugin Export requested ...");

            List<ColorPresetSettings> config = _SettingsPlugin.ReadColorPresetSettings().Result;

            ColorPresetSettings curColorPresetSetting = get_cur_monitor_preset_config(MonitorInfo, config);

            if (curColorPresetSetting.AppInfo.Count <= 0)
            {
                writelog("ColorPresetPlugin Export add Default items ...");

                //Default items
                curColorPresetSetting.AppInfo.Add("Desktop Application", new ColorPresetSettings_AppInfo()
                {
                    //ColorPresetName = "Standard/Native",
                    Color = 0,
                    HDRColor = -1,
                    IconName = "Assets/palette.png",
                });
                curColorPresetSetting.AppInfo.Add("UWP Application", new ColorPresetSettings_AppInfo()
                {
                    //ColorPresetName = "Standard/Native",
                    Color = 0,
                    HDRColor = -1,
                    IconName = "Assets/palette.png",
                });
            }

            writelog("ColorPresetPlugin Export exit ...");
            return System.Threading.Tasks.Task.FromResult(curColorPresetSetting);
        }

        /// <summary>
        /// DDPM MonitorSettings Data Import for Color
        /// </summary>
        /// <param name="MonitorInfo"></param>
        /// <param name="colorPresetSetting_Import"></param>
        /// <param name="_SettingsPlugin"></param>
        /// <returns></returns>
        public Task<bool> Import(MonitorInfo MonitorInfo, ColorPresetSettings colorPresetSetting_Import, ISettingsManagerDev _SettingsPlugin)
        {
            writelog("ColorPresetPlugin Import requested ...");

            List<ColorPresetSettings> config = _SettingsPlugin.ReadColorPresetSettings().Result;

            Test_AddAppCollectionData.GetInstance()._monitorConfigs = config;

            int index = get_index_of_json_config_for_cur_monitor(MonitorInfo);

            if (index >= 0)
            {
                writelog("ColorPresetPlugin Import Data is started");

                Test_AddAppCollectionData.GetInstance()._monitorConfigs[index].RunType = colorPresetSetting_Import.RunType;
                Test_AddAppCollectionData.GetInstance()._monitorConfigs[index].ColorForManual = colorPresetSetting_Import.ColorForManual;
                Test_AddAppCollectionData.GetInstance()._monitorConfigs[index].ColorManagement_Status = colorPresetSetting_Import.ColorManagement_Status;
                Test_AddAppCollectionData.GetInstance()._monitorConfigs[index].ColorManagement_RunType = colorPresetSetting_Import.ColorManagement_RunType;

                Test_AddAppCollectionData.GetInstance()._monitorConfigs[index].AppInfo = colorPresetSetting_Import.AppInfo;

                _SettingsPlugin.WriteColorPresetSettings(config);

                writelog("ColorPresetPlugin Import Data is finished");

                return System.Threading.Tasks.Task.FromResult(true);
            }

            if (index < 0)
            {
                writelog("ColorPresetPlugin Import create new to config for current monitor  ...");

                //data not exist, create new to config for current monitor
                Test_AddAppCollectionData.GetInstance()._monitorConfigs.Add(new ColorPresetSettings()
                {
                    ModelName = colorPresetSetting_Import.ModelName,
                    SerialNumber = colorPresetSetting_Import.SerialNumber,
                    ServiceTag = colorPresetSetting_Import.ServiceTag,
                    RunType = colorPresetSetting_Import.RunType,
                    AppInfo = new Dictionary<string, ColorPresetSettings_AppInfo>(),
                    //PresetForManual = "Standard/Native",
                    ColorForManual = colorPresetSetting_Import.ColorForManual,
                    ColorManagement_Status = colorPresetSetting_Import.ColorManagement_Status,
                    ColorManagement_RunType = colorPresetSetting_Import.ColorManagement_RunType
                });

                index = get_index_of_json_config_for_cur_monitor(MonitorInfo);

                if (index >= 0 && colorPresetSetting_Import.AppInfo.Count > 0)
                {
                    writelog("ColorPresetPlugin Import AppInfo is started");

                    foreach (KeyValuePair<string, ColorPresetSettings_AppInfo> kvp in colorPresetSetting_Import.AppInfo)
                    {
                        ColorPresetSettings_AppInfo temp = new ColorPresetSettings_AppInfo();

                        temp.Color = kvp.Value.Color;
                        temp.HDRColor = kvp.Value.HDRColor;
                        temp.IconName = kvp.Value.IconName;

                        Test_AddAppCollectionData.GetInstance()._monitorConfigs[index].AppInfo.Add(kvp.Key, temp);

                    }

                    writelog("ColorPresetPlugin Import AppInfo is finished");
                }

                _SettingsPlugin.WriteColorPresetSettings(config);
                writelog("ColorPresetPlugin Import exit ...");

                return System.Threading.Tasks.Task.FromResult(true);
            }

            writelog("ColorPresetPlugin Import exit ...");

            return System.Threading.Tasks.Task.FromResult(false);
        }

        /// <summary>
        /// DDPM MonitorSettings Data migration for Color
        /// </summary>
        /// <param name="colorPresetSetting_Migration"></param>
        /// <param name="Model"></param>
        /// <param name="ServiceTag"></param>
        /// <param name="_SettingsPlugin"></param>
        /// <param name="ColorForManual_VCPE2Code_value"></param>
        /// <returns></returns>
        public Task<bool> Migration(DdmLibrary.Utility.ColorPreset colorPresetSetting_Migration, string Model, string ServiceTag, ISettingsManagerDev _SettingsPlugin, int ColorForManual_VCPE2Code_value = 0)
        {
            writelog("ColorPresetPlugin Migration requested ...");

            List<ColorPresetSettings> config = _SettingsPlugin.ReadColorPresetSettings().Result;

            Test_AddAppCollectionData.GetInstance()._monitorConfigs = config;

            int index = -1;

            if (Test_AddAppCollectionData.GetInstance()._monitorConfigs != null &&
                Test_AddAppCollectionData.GetInstance()._monitorConfigs.Count > 0)
            {
                index = Test_AddAppCollectionData.GetInstance()._monitorConfigs.FindIndex(x =>
                                                x.ModelName.Trim() == Model.Trim() &&
                                                x.ServiceTag.Trim() == ServiceTag.Trim());

            }

            if (index >= 0)
            {
                writelog($"ColorPresetPlugin Migration Get Monitor index = {index}");

                if (colorPresetSetting_Migration.Auto)
                    Test_AddAppCollectionData.GetInstance()._monitorConfigs[index].RunType = (int)ColorPresetRunType.Auto;
                else
                    Test_AddAppCollectionData.GetInstance()._monitorConfigs[index].RunType = (int)ColorPresetRunType.Manual;

                Test_AddAppCollectionData.GetInstance()._monitorConfigs[index].ColorForManual = ColorForManual_VCPE2Code_value;

                if (colorPresetSetting_Migration.ColorManagement == 0 ||
                    colorPresetSetting_Migration.ColorManagement == 1)
                {
                    Test_AddAppCollectionData.GetInstance()._monitorConfigs[index].ColorManagement_Status = 0;
                    Test_AddAppCollectionData.GetInstance()._monitorConfigs[index].ColorManagement_RunType = 0;
                }
                else if (colorPresetSetting_Migration.ColorManagement == 2) // color_manage_off_bymonitor
                {
                    Test_AddAppCollectionData.GetInstance()._monitorConfigs[index].ColorManagement_Status = 0;
                    Test_AddAppCollectionData.GetInstance()._monitorConfigs[index].ColorManagement_RunType = 1;
                }
                else if (colorPresetSetting_Migration.ColorManagement == 3) // color_manage_on_bymonitor 
                {
                    Test_AddAppCollectionData.GetInstance()._monitorConfigs[index].ColorManagement_Status = 1;
                    Test_AddAppCollectionData.GetInstance()._monitorConfigs[index].ColorManagement_RunType = 1;

                }
                else if (colorPresetSetting_Migration.ColorManagement == 4) // color_manage_off_byhost
                {
                    Test_AddAppCollectionData.GetInstance()._monitorConfigs[index].ColorManagement_Status = 0;
                    Test_AddAppCollectionData.GetInstance()._monitorConfigs[index].ColorManagement_RunType = 2;

                }
                else if (colorPresetSetting_Migration.ColorManagement == 5) //color_manage_on_byhost
                {
                    Test_AddAppCollectionData.GetInstance()._monitorConfigs[index].ColorManagement_Status = 1;
                    Test_AddAppCollectionData.GetInstance()._monitorConfigs[index].ColorManagement_RunType = 2;
                }
                else
                {
                    writelog("ColorPresetPlugin Migration ColorManagement value is not define.");
                }

                writelog("ColorPresetPlugin Migration ColorManagement is finished");

                if (colorPresetSetting_Migration.AppInfos.Count > 0)
                {
                    writelog("ColorPresetPlugin Migration AppInfo is started");

                    for (int i = 0; i < colorPresetSetting_Migration.AppInfos.Count; i++)
                    {
                        string strAppName = string.Empty;
                        strAppName = colorPresetSetting_Migration.AppInfos[i].Name;

                        string strExeName = string.Empty;
                        strExeName = colorPresetSetting_Migration.AppInfos[i].ExeName;

                        if (Test_AddAppCollectionData.GetInstance()._monitorConfigs[index].AppInfo.ContainsKey(strAppName))
                        {
                            Test_AddAppCollectionData.GetInstance()._monitorConfigs[index].AppInfo[strAppName].Color = colorPresetSetting_Migration.AppInfos[i].Color;
                            Test_AddAppCollectionData.GetInstance()._monitorConfigs[index].AppInfo[strAppName].HDRColor = colorPresetSetting_Migration.AppInfos[i].HDRColor;
                        }
                        else  // 如果Migration 的AppName 不存在 DDPM 的 ColorSetting.json 就新增
                        {
                            ColorPresetSettings_AppInfo temp = new ColorPresetSettings_AppInfo();

                            temp.Color = colorPresetSetting_Migration.AppInfos[i].Color;
                            temp.HDRColor = colorPresetSetting_Migration.AppInfos[i].HDRColor;

                            foreach (KeyValuePair<string, InstalledAppInfo> kvp_applist in _AllAppData)
                            {
                                if (strAppName == kvp_applist.Value.AppName)
                                {
                                    temp.IconName = _SettingsPlugin.GetAppIconFolderPath().Result + "\\" + strExeName + ".png"; // Jim 20250206 modify  PIMS-344233 for Color : auto list
                                }

                            }

                            Test_AddAppCollectionData.GetInstance()._monitorConfigs[index].AppInfo.Add(strAppName, temp);

                        }
                    }

                    writelog("ColorPresetPlugin Migration AppInfo is finished");
                }
                _SettingsPlugin.WriteColorPresetSettings(config);

                writelog("ColorPresetPlugin Migration exit ...");

                return System.Threading.Tasks.Task.FromResult(true);
            }

            if (index < 0)
            {
                writelog($"ColorPresetPlugin Migration, create new to config for current monitor");

                //data not exist, create new to config for current monitor
                Test_AddAppCollectionData.GetInstance()._monitorConfigs.Add(new ColorPresetSettings()
                {
                    ModelName = Model,
                    SerialNumber = "",
                    ServiceTag = ServiceTag,
                    RunType = 0,
                    AppInfo = new Dictionary<string, ColorPresetSettings_AppInfo>(),
                    //PresetForManual = "Standard/Native",
                    ColorForManual = 0,
                    ColorManagement_Status = 0,
                    ColorManagement_RunType = 0
                });

                if (Test_AddAppCollectionData.GetInstance()._monitorConfigs != null &&
                    Test_AddAppCollectionData.GetInstance()._monitorConfigs.Count > 0)
                {

                    index = Test_AddAppCollectionData.GetInstance()._monitorConfigs.FindIndex(x =>
                                                    x.ModelName.Trim() == Model.Trim() &&
                                                    x.ServiceTag.Trim() == ServiceTag.Trim());

                }

                if (index >= 0)
                {
                    if (colorPresetSetting_Migration.Auto)
                        Test_AddAppCollectionData.GetInstance()._monitorConfigs[index].RunType = (int)ColorPresetRunType.Auto;
                    else
                        Test_AddAppCollectionData.GetInstance()._monitorConfigs[index].RunType = (int)ColorPresetRunType.Manual;

                    Test_AddAppCollectionData.GetInstance()._monitorConfigs[index].ColorForManual = ColorForManual_VCPE2Code_value;

                    if (colorPresetSetting_Migration.ColorManagement == 0 ||
                        colorPresetSetting_Migration.ColorManagement == 1)
                    {
                        Test_AddAppCollectionData.GetInstance()._monitorConfigs[index].ColorManagement_Status = 0;
                        Test_AddAppCollectionData.GetInstance()._monitorConfigs[index].ColorManagement_RunType = 0;
                    }
                    else if (colorPresetSetting_Migration.ColorManagement == 2) // color_manage_off_bymonitor
                    {
                        Test_AddAppCollectionData.GetInstance()._monitorConfigs[index].ColorManagement_Status = 0;
                        Test_AddAppCollectionData.GetInstance()._monitorConfigs[index].ColorManagement_RunType = 1;
                    }
                    else if (colorPresetSetting_Migration.ColorManagement == 3) // color_manage_on_bymonitor 
                    {
                        Test_AddAppCollectionData.GetInstance()._monitorConfigs[index].ColorManagement_Status = 1;
                        Test_AddAppCollectionData.GetInstance()._monitorConfigs[index].ColorManagement_RunType = 1;

                    }
                    else if (colorPresetSetting_Migration.ColorManagement == 4) // color_manage_off_byhost
                    {
                        Test_AddAppCollectionData.GetInstance()._monitorConfigs[index].ColorManagement_Status = 0;
                        Test_AddAppCollectionData.GetInstance()._monitorConfigs[index].ColorManagement_RunType = 2;

                    }
                    else if (colorPresetSetting_Migration.ColorManagement == 5) //color_manage_on_byhost
                    {
                        Test_AddAppCollectionData.GetInstance()._monitorConfigs[index].ColorManagement_Status = 1;
                        Test_AddAppCollectionData.GetInstance()._monitorConfigs[index].ColorManagement_RunType = 2;
                    }
                    else
                    {
                        writelog("ColorPresetPlugin Migration ColorManagement value is not define.");
                    }

                    writelog("ColorPresetPlugin Migration ColorManagement is finished");

                }

                if (index >= 0 && colorPresetSetting_Migration.AppInfos.Count > 0)
                {
                    writelog("ColorPresetPlugin Migration AppInfo is started");

                    for (int i = 0; i < colorPresetSetting_Migration.AppInfos.Count; i++)
                    {
                        string strAppName = string.Empty;
                        strAppName = colorPresetSetting_Migration.AppInfos[i].Name;

                        if (Test_AddAppCollectionData.GetInstance()._monitorConfigs[index].AppInfo.ContainsKey(strAppName))
                        {
                            Test_AddAppCollectionData.GetInstance()._monitorConfigs[index].AppInfo[strAppName].Color = colorPresetSetting_Migration.AppInfos[i].Color;
                            Test_AddAppCollectionData.GetInstance()._monitorConfigs[index].AppInfo[strAppName].HDRColor = colorPresetSetting_Migration.AppInfos[i].HDRColor;
                        }
                        else  // 如果Migration 的AppName 不存在 DDPM 的 ColorSetting.json 就新增
                        {
                            ColorPresetSettings_AppInfo temp = new ColorPresetSettings_AppInfo();

                            temp.Color = colorPresetSetting_Migration.AppInfos[i].Color;
                            temp.HDRColor = colorPresetSetting_Migration.AppInfos[i].HDRColor;

                            if (strAppName.Equals("Desktop Application", StringComparison.OrdinalIgnoreCase) || strAppName.Equals("UWP Application", StringComparison.OrdinalIgnoreCase)) // Jim 20250207 modify  PIMS-344233 for Color : auto list
                            {
                                temp.IconName = "Assets/palette.png";
                            }
                            else
                            {
                                foreach (KeyValuePair<string, InstalledAppInfo> kvp_applist in _AllAppData)
                                {
                                    if (strAppName == kvp_applist.Value.AppName)
                                    {
                                        temp.IconName = _SettingsPlugin.GetAppIconFolderPath().Result + "\\" + colorPresetSetting_Migration.AppInfos[i].ExeName + ".png"; // Jim 20250207 modify  PIMS-344233 for Color : auto list
                                    }

                                }
                            }

                            Test_AddAppCollectionData.GetInstance()._monitorConfigs[index].AppInfo.Add(strAppName, temp);
                        }
                    }

                    writelog("ColorPresetPlugin Migration AppInfo is finished");
                }

                _SettingsPlugin.WriteColorPresetSettings(config);

                writelog("ColorPresetPlugin Migration exit ...");

                return System.Threading.Tasks.Task.FromResult(true);
            }

            return System.Threading.Tasks.Task.FromResult(false);
        }

        public Task<bool> CompareColorPresetSupportList(List<string> NewList)
        {
            if (NewList != null && NewList.Count > 0)
            {
#if DEBUG
                foreach (string color_N in NewList)
                {
                    Debug.WriteLine("NewList : " + color_N);
                }

                foreach (string color in ColorPresetSupportList)
                {
                    Debug.WriteLine("color : " + color);
                }
#endif
                if (ColorPresetSupportList.Count == NewList.Count)
                {
                    for (int i = 0; i < NewList.Count; i++)
                    {
                        if (NewList[i] != ColorPresetSupportList[i])
                        {
                            return Task.FromResult(false);
                        }
                    }
                    return Task.FromResult(true);
                }
            }
            return Task.FromResult(false);
        }

        #endregion
    }
}
