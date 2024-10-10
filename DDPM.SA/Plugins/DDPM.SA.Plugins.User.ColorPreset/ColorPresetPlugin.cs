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
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Reflection;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Policy;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using VcpCore.Common;
//using WinCopies.Util;
using DdmLibrary;
using DdmLibrary.Utility;
//using DDPM.SA.Common.Settings;

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
        private const string publisherCompany = "Wistron";
        private const string publisherWebsite = "https://www.wistron.com";
        private const string publisherSupport = "This plugin implements ColorPreset Plugin.";

        private IAgent _agent;
        public const string PluginLogId = "ColorPreset";

        private Dictionary<string, InstalledAppInfo> _AllAppData = new Dictionary<string, InstalledAppInfo>();
        private List<string> _supported_preset = new List<string>();

        List<string> HDR_ColorPresetNameList = new List<string>() { "Standard HDR", "Movie HDR", "Game HDR", "Vivid HDR", "Desktop", "Reference", "Multiscreen Match", "DisplayHDR", "HDR10", "HLG" };
        private List<string> ColorPresetSupportList = new List<string>();

        //20240802 jim add
        //public List<string> Support_ICC_DeviceName { get; set; } = new List<string> { "U4021QW", "U2723QE", "U3223QE", "U3223QZ", "U3423WE", "U3824DW", "U4924DW", "U3224KB", "U2724D", "U2724DE", "U3425WE", "U4025QW", "UP2720Q", "UP3221Q" };

        private List<X509Certificate2> TrustedPublisher = new List<X509Certificate2>();
        private List<X509Certificate2> TrustedRoot = new List<X509Certificate2>();

        //private readonly string[] Issuer = { "CN=Entrust Certification Authority - L1F, O=\"Entrust, C=US", "CN=localhost, O=DigiNow, C=US" };
        //private readonly string[] Issuer = { "Entrust Certification Authority - L1F, OU=\"(c) 2016 Entrust, Inc. - for authorized use only\", OU=See www.entrust.net/legal-terms, O=\"Entrust, Inc.\", C=US" };
        //private readonly string[] Subject = { "CN=content-cdn.dell.com, O=Dell, L=Round Rock, S=Texas, C=US" };
        private string[] Issuers;
        private string[] Subjects;

        //20240829 Jim move to here 20240829
        private ISettingsManagerDev _SettingsPlugin;

        private MainWindow? MonitorBorkerWin = null; //Dean 0626 fix SAST issue, remove static
        private Thread newWindowThread_AutoSetColorPresetForMonitorConfig = null;

        //20240830 Jim add
        IIC_Metadata _ICC_Metadata = new IIC_Metadata();
        private Download? download = null;
        //private Logs _logs;

        //20240905 Jim add
        MonitorInfo Active_monitorInfo = null;
        public RegistryMonitor_ICC registryMonitor_ICC = null;

        /// <summary>
        /// Colorpreset Manual change event，return Colorpreset name
        /// </summary>
        public event EventHandler<string>? Coloreset_manual_ChangeEvent;

        private enum log_type
        {
            info = 0,
            error
        }

        public event EventHandler<VCPchangedEventArgs> VCPchanged;

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
            return Task.FromResult(_AllAppData);
        }

        public int get_index_of_json_config_for_cur_monitor(MonitorInfo mo)
        {
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

                        if (String.IsNullOrEmpty(Test_AddAppCollectionData.GetInstance()._monitorConfigs[i].SerialNumber))
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

                //int index = Test_AddAppCollectionData.GetInstance()._monitorConfigs.FindIndex(x =>
                //x.ModelName.Trim() == mo.edid.ModelName.Trim() &&
                //x.SerialNumber.Trim() == mo.edid.SerialNumber.Trim());
            }
            return index;
        }

        public ColorPresetSettings get_cur_monitor_preset_config(MonitorInfo mo, List<ColorPresetSettings> config)
        {
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
                    //PresetForManual = "Standard/Native",
                    ColorForManual = 0,
                    ColorManagement_Status = (int)ColorManagementStatus.Off,
                    ColorManagement_RunType = (int)ColorManagementRunType.Off
                });

                index = get_index_of_json_config_for_cur_monitor(mo);

                Test_AddAppCollectionData.GetInstance()._monitorConfigs[index].AppInfo.Add("Desktop Application", new ColorPresetSettings_AppInfo()
                {
                    //ColorPresetName = "Standard/Native",
                    Color = 0,
                    HDRColor = -1,
                    IconName = "Assets/palette.png",
                });

                Test_AddAppCollectionData.GetInstance()._monitorConfigs[index].AppInfo.Add("UWP Application", new ColorPresetSettings_AppInfo()
                {
                    //ColorPresetName = "Standard/Native",
                    Color = 0,
                    HDRColor = -1,
                    IconName = "Assets/palette.png",
                });
            }
            return Test_AddAppCollectionData.GetInstance()._monitorConfigs[index];
        }

        public Task<List<ColorPresetSettings>> AddColorPresetForMonitorConfig(MonitorInfo mo, string AppName, string ColorPreset_Name, string supported_preset, List<ColorPresetSettings> config , bool SmartHDR_ON=false)
        {
            ColorPresetSettings temp = get_cur_monitor_preset_config(mo, config);

            if (temp.AppInfo.Count <= 0)
            {
                //Default items
                temp.AppInfo.Add("Desktop Application", new ColorPresetSettings_AppInfo()
                {
                    //ColorPresetName = "Standard/Native",
                    Color = 0,
                    HDRColor = -1,
                    IconName = "Assets/palette.png",
                });
                temp.AppInfo.Add("UWP Application", new ColorPresetSettings_AppInfo()
                {
                    //ColorPresetName = "Standard/Native",
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
                    //PresetForManual = "Standard/Native",
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
                    //ColorPresetName = ColorPreset_Name,
                    Color = nColorVCPCoreValue,
                    HDRColor = -1,
                    IconName = kvp.Value.IconName,
                });
            }

            return Task.FromResult(Test_AddAppCollectionData.GetInstance()._monitorConfigs);
        }

        public Task<List<ColorPresetSettings>> ChangeColorPresetForMonitorConfig(MonitorInfo mo, string AppName, string ColorPreset_Name, List<ColorPresetSettings> config)
        {
            Test_AddAppCollectionData.GetInstance()._monitorConfigs = config;

            int index_config = get_index_of_json_config_for_cur_monitor(mo);
            if (index_config >= 0)
            {
                var nColorVCPCoreValue = GetColorVCPCoreValue(ColorPreset_Name).Result;

                Test_AddAppCollectionData.GetInstance()._monitorConfigs[index_config].RunType = (int)ColorPresetRunType.Auto;
                //Test_AddAppCollectionData.GetInstance()._monitorConfigs[index_config].AppInfo[AppName].ColorPresetName = ColorPreset_Name;

                Test_AddAppCollectionData.GetInstance()._monitorConfigs[index_config].AppInfo[AppName].Color = nColorVCPCoreValue;
            }

            return Task.FromResult(Test_AddAppCollectionData.GetInstance()._monitorConfigs);
        }

        public Task<List<ColorPresetSettings>> DeleteColorPresetForMonitorConfig(MonitorInfo mo, string AppName, List<ColorPresetSettings> config)
        {
            Test_AddAppCollectionData.GetInstance()._monitorConfigs = config;

            int index_config = get_index_of_json_config_for_cur_monitor(mo);

            if (index_config >= 0)
            {
                Test_AddAppCollectionData.GetInstance()._monitorConfigs[index_config].RunType = (int)ColorPresetRunType.Auto;
                Test_AddAppCollectionData.GetInstance()._monitorConfigs[index_config].AppInfo.Remove(AppName);
            }

            return Task.FromResult(Test_AddAppCollectionData.GetInstance()._monitorConfigs);
        }

        /// <summary>
        /// 啟動 MonitorBorker 執行抓前景active app name
        /// </summary>
        /// <param name="m"></param>
        public void Launch_MonitorBorker(MonitorInfo m, IDeviceManagerSA _DeviceManagerPlugin, bool SmartHDR_ON = false, List<string> ColorPresetSupportList = null)
        {
            if (Log != null)
            {
                Log.Info($"Launch_MonitorBorker requested ...");
            }
            writelog("DeviceManagerPlugin Launch_MonitorBorker requested ...");

            if (m != null)
            {
                var v = (MonitorInfo)m;

                System.Windows.Forms.Screen s = System.Windows.Forms.Screen.AllScreens.FirstOrDefault(x => x.DeviceName == v.DisplayName);

                if (s != null)
                {
                    // jim modify 20240605
                    if (MonitorBorkerWin == null)
                    {
                        MonitorBorkerWin = new MainWindow(_DeviceManagerPlugin, m);

                        MonitorBorkerWin.Show();
                        MonitorBorkerWin.Set_AUTO_ColorPresetConfig(true, SmartHDR_ON, ColorPresetSupportList);
                    }
                    else
                    {
                        //MonitorBorkerWin.Close();
                    }
                }
            }

            return;
        }

        public Task<bool> AutoSetColorPresetForMonitorConfig(MonitorInfo mo, string on_off, ISettingsManagerDev _SettingsPlugin, IDeviceManagerSA _DeviceManagerPlugin, bool SmartHDR_ON = false, List<string> ColorPresetSupportList = null)
        {
            List<ColorPresetSettings> config = _SettingsPlugin.ReadColorPresetSettings().Result;

            if (on_off.Equals("ON", StringComparison.OrdinalIgnoreCase))
            {
                Test_AddAppCollectionData.GetInstance()._monitorConfigs = config;

                int index = get_index_of_json_config_for_cur_monitor(mo);

                if (index >= 0)
                {
                    Test_AddAppCollectionData.GetInstance()._monitorConfigs[index].RunType = (int)ColorPresetRunType.Auto;
                }

                _SettingsPlugin.WriteColorPresetSettings(config);
                Thread.Sleep(100);

                if (newWindowThread_AutoSetColorPresetForMonitorConfig == null)
                {
                    // create a thread
                    newWindowThread_AutoSetColorPresetForMonitorConfig = new Thread(new ThreadStart(() =>
                    {
                        // create and show the window
                        Launch_MonitorBorker(mo, _DeviceManagerPlugin, SmartHDR_ON, ColorPresetSupportList);

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
                        MonitorBorkerWin.Set_AUTO_ColorPresetConfig(true, SmartHDR_ON, ColorPresetSupportList);
                }
            }
            else if (on_off.Equals("OFF", StringComparison.OrdinalIgnoreCase))
            {
                Test_AddAppCollectionData.GetInstance()._monitorConfigs = config;

                int index = get_index_of_json_config_for_cur_monitor(mo);

                if (index >= 0)
                {
                    Test_AddAppCollectionData.GetInstance()._monitorConfigs[index].RunType = (int)ColorPresetRunType.Manual;
                }

                _SettingsPlugin.WriteColorPresetSettings(config);
                Thread.Sleep(100);

                // jim modify 20240605
                if (newWindowThread_AutoSetColorPresetForMonitorConfig != null)
                {
                    if (MonitorBorkerWin != null) // jim add 20240809
                        MonitorBorkerWin.Set_AUTO_ColorPresetConfig(false, SmartHDR_ON, ColorPresetSupportList);
                }

            }

            return Task.FromResult(true);
        }

        public Task<bool> AutoColorManagementForMonitorConfig(MonitorInfo monitorInfo, string off_bymonitor_byhost, ISettingsManagerDev _SettingsPlugin, string ColorPreset_Name = "", string ICC_profile_Name = "")
        {
            bool blRet = true;
            Active_monitorInfo = monitorInfo;
            List<ColorPresetSettings> config = _SettingsPlugin.ReadColorPresetSettings().Result;

            if (off_bymonitor_byhost.Equals("ON", StringComparison.OrdinalIgnoreCase))
            {
                Test_AddAppCollectionData.GetInstance()._monitorConfigs = config;

                int index = get_index_of_json_config_for_cur_monitor(monitorInfo);

                if (index >= 0)
                {
                    Test_AddAppCollectionData.GetInstance()._monitorConfigs[index].ColorManagement_Status = (int)ColorManagementStatus.On;
                }

                _SettingsPlugin.WriteColorPresetSettings(config);
                Thread.Sleep(100);

                if (Test_AddAppCollectionData.GetInstance()._monitorConfigs[index].ColorManagement_RunType == (int)ColorManagementRunType.Bymonitor)
                {
                    if (_ICC_Metadata.Is_Support_ICC_DeviceName)
                    {
                        if (registryMonitor_ICC != null)
                        {
                            if (registryMonitor_ICC.IsMonitoring)
                                registryMonitor_ICC.Dispose();
                            registryMonitor_ICC = null;
                        }

                        int count = _ICC_Metadata._match_ICC_DeviceName.Count;

                        for (int i = 0; i < count; i++)
                        {
                            MonitorProfile.IntsallMonitorProfile(_ICC_Metadata.strICC_Folder + _ICC_Metadata._match_ICC_DeviceName[i].File);
                        }
                    }
                }
                else if (Test_AddAppCollectionData.GetInstance()._monitorConfigs[index].ColorManagement_RunType == (int)ColorManagementRunType.Byhost)
                {
                    if (_ICC_Metadata.Is_Support_ICC_DeviceName)
                    {
                        if (_ICC_Metadata._match_ICC_DeviceName != null)
                        {
                            int count = _ICC_Metadata._match_ICC_DeviceName.Count;

                            for (int i = 0; i < count; i++)
                            {
                                MonitorProfile.IntsallMonitorProfile(_ICC_Metadata.strICC_Folder + _ICC_Metadata._match_ICC_DeviceName[i].File);
                            }

                            string keyName = string.Format("{0}\\{1}", "HKEY_CURRENT_USER", @"Software\Microsoft\Windows NT\CurrentVersion\ICM\ProfileAssociations\Display\{4d36e96e-e325-11ce-bfc1-08002be10318}");

                            registryMonitor_ICC = new RegistryMonitor_ICC(keyName);
                            registryMonitor_ICC.RegChanged += new EventHandler(OnRegChanged_ICC);
                            registryMonitor_ICC.Error += new System.IO.ErrorEventHandler(OnError_ICC);
                            registryMonitor_ICC.Start();
                        }
                    }

                }

            }
            else if (off_bymonitor_byhost.Equals("OFF", StringComparison.OrdinalIgnoreCase))
            {
                Test_AddAppCollectionData.GetInstance()._monitorConfigs = config;

                int index = get_index_of_json_config_for_cur_monitor(monitorInfo);

                if (index >= 0)
                {
                    Test_AddAppCollectionData.GetInstance()._monitorConfigs[index].ColorManagement_Status = (int)ColorManagementStatus.Off;
                }

                _SettingsPlugin.WriteColorPresetSettings(config);
                Thread.Sleep(100);

                if (_ICC_Metadata.Is_Support_ICC_DeviceName)
                {

                    if (registryMonitor_ICC != null)
                    {
                        if (registryMonitor_ICC.IsMonitoring)
                            registryMonitor_ICC.Dispose();
                        registryMonitor_ICC = null;
                    }

                }
            }
            else if (off_bymonitor_byhost.Equals("BYMONITOR", StringComparison.OrdinalIgnoreCase))
            {
                Test_AddAppCollectionData.GetInstance()._monitorConfigs = config;

                int index = get_index_of_json_config_for_cur_monitor(monitorInfo);

                if (index >= 0)
                {
                    Test_AddAppCollectionData.GetInstance()._monitorConfigs[index].ColorManagement_Status = (int)ColorManagementStatus.On;
                    Test_AddAppCollectionData.GetInstance()._monitorConfigs[index].ColorManagement_RunType = (int)ColorManagementRunType.Bymonitor;
                }

                _SettingsPlugin.WriteColorPresetSettings(config);
                Thread.Sleep(100);

                if (_ICC_Metadata.Is_Support_ICC_DeviceName)
                {
                    if (registryMonitor_ICC != null)
                    {
                        if (registryMonitor_ICC.IsMonitoring)
                            registryMonitor_ICC.Dispose();
                        registryMonitor_ICC = null;
                    }

                    int count = _ICC_Metadata._match_ICC_DeviceName.Count;

                    for (int i = 0; i < count; i++)
                    {
                        MonitorProfile.IntsallMonitorProfile(_ICC_Metadata.strICC_Folder + _ICC_Metadata._match_ICC_DeviceName[i].File);
                    }
                }
            }
            else if (off_bymonitor_byhost.Equals("BYHOST", StringComparison.OrdinalIgnoreCase))
            {
                Test_AddAppCollectionData.GetInstance()._monitorConfigs = config;

                int index = get_index_of_json_config_for_cur_monitor(monitorInfo);

                if (index >= 0)
                {
                    Test_AddAppCollectionData.GetInstance()._monitorConfigs[index].ColorManagement_Status = (int)ColorManagementStatus.On;
                    Test_AddAppCollectionData.GetInstance()._monitorConfigs[index].ColorManagement_RunType = (int)ColorManagementRunType.Byhost;
                }

                _SettingsPlugin.WriteColorPresetSettings(config);
                Thread.Sleep(100);

                if (_ICC_Metadata.Is_Support_ICC_DeviceName)
                {
                    if (_ICC_Metadata._match_ICC_DeviceName != null)
                    {
                        int count = _ICC_Metadata._match_ICC_DeviceName.Count;

                        for (int i = 0; i < count; i++)
                        {
                            MonitorProfile.IntsallMonitorProfile(_ICC_Metadata.strICC_Folder + _ICC_Metadata._match_ICC_DeviceName[i].File);
                        }

                        string keyName = string.Format("{0}\\{1}", "HKEY_CURRENT_USER", @"Software\Microsoft\Windows NT\CurrentVersion\ICM\ProfileAssociations\Display\{4d36e96e-e325-11ce-bfc1-08002be10318}");

                        registryMonitor_ICC = new RegistryMonitor_ICC(keyName);
                        registryMonitor_ICC.RegChanged += new EventHandler(OnRegChanged_ICC);
                        registryMonitor_ICC.Error += new System.IO.ErrorEventHandler(OnError_ICC);
                        registryMonitor_ICC.Start();
                    }
                }

            }

            return Task.FromResult(blRet);
        }

        public void OnRegChanged_ICC(object sender, EventArgs e)
        {
            string Key_Profile_Name = string.Empty;
            Key_Profile_Name = MonitorProfile.GetMonitorProfile(Active_monitorInfo.DisplayName);

            int count = _ICC_Metadata._match_ICC_DeviceName.Count;

            for (int i = 0; i < count; i++)
            {
                if (string.Equals(Key_Profile_Name, _ICC_Metadata._match_ICC_DeviceName[i].File, StringComparison.OrdinalIgnoreCase))
                {
                    // 20240619 jim modify

                    //string strICC_ColorPreset = _ICC_Metadata._support_ICC_DeviceName[MyModule.SelectedHomeDevice.MonitorInfo.modelName][i].ColorPreset;

                    string[] separators = { "," };
                    string[] strICC_ColorPresets = _ICC_Metadata._match_ICC_DeviceName[i].ColorPreset.Split(separators, StringSplitOptions.None);

                    if (string.Equals(strICC_ColorPresets[0], "Standard", StringComparison.OrdinalIgnoreCase) || string.Equals(strICC_ColorPresets[0], "Native", StringComparison.OrdinalIgnoreCase))
                        WriteColorPreset(Active_monitorInfo, "Standard/Native");
                    else if (string.Equals(strICC_ColorPresets[0], "Game", StringComparison.OrdinalIgnoreCase) || string.Equals(strICC_ColorPresets[0], "Game1", StringComparison.OrdinalIgnoreCase))
                        WriteColorPreset(Active_monitorInfo, "Game/Game1");
                    else if (strICC_ColorPresets[0].Contains("Rec", StringComparison.OrdinalIgnoreCase) || strICC_ColorPresets[0].Contains("BT.", StringComparison.OrdinalIgnoreCase) || strICC_ColorPresets[0].Contains("709", StringComparison.OrdinalIgnoreCase))
                        WriteColorPreset(Active_monitorInfo, "Rec. 709 / BT.709");
                    else
                        WriteColorPreset(Active_monitorInfo, strICC_ColorPresets[0]);

                    Coloreset_manual_ChangeEvent?.AsyncFireAndForget(this, strICC_ColorPresets[0], System.Threading.CancellationToken.None);

                    break;
                }
            }

            return;
        }

        public void StopRegistryMonitor()
        {
            if (registryMonitor_ICC != null)
            {
                registryMonitor_ICC.Stop();
                registryMonitor_ICC.RegChanged -= new EventHandler(OnRegChanged_ICC);
                registryMonitor_ICC.Error -= new System.IO.ErrorEventHandler(OnError_ICC);
                registryMonitor_ICC = null;
            }
        }

        // add jim 20240604
        public void OnError_ICC(object sender, ErrorEventArgs e)
        {
            StopRegistryMonitor();
        }

        public Task<string> GetAutoColorPresetStatus(MonitorInfo mo, ISettingsManagerDev _SettingsPlugin)
        {
            List<ColorPresetSettings> config = _SettingsPlugin.ReadColorPresetSettings().Result;

            Test_AddAppCollectionData.GetInstance()._monitorConfigs = config;

            int index = get_index_of_json_config_for_cur_monitor(mo);

            if (index >= 0)
            {
                if (Test_AddAppCollectionData.GetInstance()._monitorConfigs[index].RunType == (int)ColorPresetRunType.Auto)
                    return Task.FromResult("ON");
                else
                    return Task.FromResult("OFF");
            }

            return Task.FromResult("OFF");
        }

        public Task<string> GetColorManagementStatus(MonitorInfo mo, ISettingsManagerDev _SettingsPlugin)
        {
            List<ColorPresetSettings> config = _SettingsPlugin.ReadColorPresetSettings().Result;

            Test_AddAppCollectionData.GetInstance()._monitorConfigs = config;

            int index = get_index_of_json_config_for_cur_monitor(mo);

            if (index >= 0)
            {
                if (Test_AddAppCollectionData.GetInstance()._monitorConfigs[index].ColorManagement_Status == (int)ColorManagementStatus.Off)
                    return Task.FromResult("OFF");
                else if (Test_AddAppCollectionData.GetInstance()._monitorConfigs[index].ColorManagement_RunType == (int)ColorManagementRunType.Bymonitor)
                    return Task.FromResult("BYMONITOR");
                else if (Test_AddAppCollectionData.GetInstance()._monitorConfigs[index].ColorManagement_RunType == (int)ColorManagementRunType.Byhost)
                    return Task.FromResult("BYHOST");
            }

            return Task.FromResult("OFF");
        }

        public void ShowOSD_ColoPreset(MonitorInfo monitorInfo, string strMsg, bool is_ShowUI = true, bool is_AUTO = false)
        {
            if (Log != null)
            {
                Log.Info($"ShowOSD_ColoPreset requested ...");
            }
            writelog("ColorPresetPlugin ShowOSD_ColoPreset requested ...");

            Thread thread = new Thread(() =>
            {
                if (monitorInfo != null)
                {
                    //var v = (MonitorInfo)m;

                    System.Windows.Forms.Screen sreen = System.Windows.Forms.Screen.AllScreens.FirstOrDefault(x => x.DeviceName == monitorInfo.DisplayName);

                    if (sreen != null)
                    {
                        if (OsdWin != null)
                        {
                            //OsdWin.Close();
                        }

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

                            //OsdWin.Height = (sreen.WorkingArea.Height * dbscale) / ((40 * dbscale));
                            //OsdWin.Width = (sreen.WorkingArea.Width) / ((40 * dbfactor));

                            if (is_ShowUI)
                                OsdWin.Show();
                        }
                        catch (Exception)
                        {
                            //OsdWin.Top = s.WorkingArea.Top;
                            //OsdWin.Left = s.WorkingArea.Left;

                            OsdWin.Top = sreen.WorkingArea.Top / (double)dpiX;
                            OsdWin.Left = sreen.WorkingArea.Left / (double)dpiX;

                            double dbfactor = ((double)((double)40 / (double)96));

                            double dbscale = dbfactor * dpiX;

                            //OsdWin.Height = (sreen.WorkingArea.Height * dbscale) / ((40 * dbscale));
                            //OsdWin.Width = (sreen.WorkingArea.Width) / ((40 * dbfactor));

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
            int index = -1;

            if (Log != null)
            {
                Log.Info($"ReadColorPreset requested ...");
            }

            if (string.IsNullOrEmpty(vcp_capbilities))
            {
                ColorPresetSupportList.Clear();
                return Task.FromResult(ColorPresetSupportList);
            }

            if (Log != null)
            {
                Log.Info($"ReadColorPreset requested [vcp_capbilities] = {vcp_capbilities}");
            }
            // 20240619 jim add
            if (!string.IsNullOrEmpty(vcp_capbilities))
            {
                var Capabilities = (JObject)JsonConvert.DeserializeObject(vcp_capbilities);
                if (Capabilities.ContainsKey("CapsDataMap"))
                {
                    var CapsDataMap = (JObject)Capabilities["CapsDataMap"];
                    if (CapsDataMap.ContainsKey("ColorPreset"))
                    {
                        if (CapsDataMap["ColorPreset"].Type == JTokenType.Null)
                        {
                            ColorPresetSupportList.Clear();
                            ColorPresetSupportList.Add("Standard/Native");
                        }
                        else
                        {


                            JArray colorrreset = (JArray)CapsDataMap["ColorPreset"];

                            ColorPresetSupportList.Clear();

                            foreach (var tmp in colorrreset)
                                ColorPresetSupportList.Add(new string(tmp.ToString()));

                        }
                    }
                }
            }

            if (SmartHDR_ON)
            {
                List<string> common_ColorPreset = ColorPresetSupportList.Intersect(HDR_ColorPresetNameList).ToList();

                ColorPresetSupportList.Clear();

                foreach (string info in common_ColorPreset)
                {
                    ColorPresetSupportList.Add(new string(info));
                }

            }
            else
            {
                ColorPresetSupportList.RemoveAll(r => HDR_ColorPresetNameList.Any(a => a == r));                

                // check Color Preset Strings Standard or Native

                if (m.modelName.StartsWith("UP"))
                {
                    index = ColorPresetSupportList.FindIndex(x => x == "Standard/Native");
                    if (index >= 0)
                        ColorPresetSupportList[index] = "Native";
                }
                else
                {
                    index = ColorPresetSupportList.FindIndex(x => x == "Standard/Native");
                    if (index >= 0)
                        ColorPresetSupportList[index] = "Standard";
                }

                // check Color Preset Strings Custom 1/2/3 or User 1/2/3

                if (m.modelName.StartsWith("UP3221Q"))
                {
                    index = ColorPresetSupportList.FindIndex(x => x == "Custom 1 / User 1");
                    if (index >= 0)
                        ColorPresetSupportList[index] = "User 1";

                    index = ColorPresetSupportList.FindIndex(x => x == "Custom 2 / User 2");
                    if (index >= 0)
                        ColorPresetSupportList[index] = "User 2";

                    index = ColorPresetSupportList.FindIndex(x => x == "Custom 3 / User 3");
                    if (index >= 0)
                        ColorPresetSupportList[index] = "User 3";
                }
                else
                {
                    index = ColorPresetSupportList.FindIndex(x => x == "Custom 1 / User 1");
                    if (index >= 0)
                        ColorPresetSupportList[index] = "Custom 1";

                    index = ColorPresetSupportList.FindIndex(x => x == "Custom 2 / User 2");
                    if (index >= 0)
                        ColorPresetSupportList[index] = "Custom 2";

                    index = ColorPresetSupportList.FindIndex(x => x == "Custom 3 / User 3");
                    if (index >= 0)
                        ColorPresetSupportList[index] = "Custom 3";
                }

                // check Color Preset Strings Game or Game1

                index = ColorPresetSupportList.FindIndex(x => x == "Game2");

                if (index >= 0)
                {
                    index = ColorPresetSupportList.FindIndex(x => x == "Game/Game1");
                    if (index >= 0)
                        ColorPresetSupportList[index] = "Game1";
                }
                else
                {

                    index = ColorPresetSupportList.FindIndex(x => x == "Game/Game1");
                    if (index >= 0)
                        ColorPresetSupportList[index] = "Game";
                }

                // check Color Preset Strings Rec.709 or BT.709 / Rec.709 or BT.709

                string strFY = string.Empty;

                for (int i = 0; i < m.modelName.Length; i++) // loop over the complete modelName
                {
                    if (Char.IsDigit(m.modelName[i])) //check if the current char is digit
                    {
                        strFY = m.modelName.Substring(i + 2, 2);
                        break;
                    }

                }

                int numFY = 0;
                try
                {
                    numFY = Int32.Parse(strFY);

                    if (ColorPresetSupportList.Contains("Rec.709 / BT.709"))
                    {
                        if (numFY <= 23)
                        {
                            index = ColorPresetSupportList.FindIndex(x => x == "Rec.709 / BT.709");
                            if (index >= 0)
                                ColorPresetSupportList[index] = "Rec.709";
                        }
                        else if (numFY >= 25)
                        {
                            index = ColorPresetSupportList.FindIndex(x => x == "Rec.709 / BT.709");
                            if (index >= 0)
                                ColorPresetSupportList[index] = "BT.709";
                        }
                    }
                }
                catch (FormatException e)
                {
                    Log?.Error("check Color Preset Strings Rec.709 or BT.709 / Rec.709 or BT.709..." + e.Message);
                }

                // check Color Preset Strings Rec.2020 or BT.2020 / Rec.2020 or BT.2020
                try
                {
                    if (ColorPresetSupportList.Contains("Rec.2020 / BT.2020"))
                    {
                        if (numFY <= 23)
                        {
                            index = ColorPresetSupportList.FindIndex(x => x == "Rec.2020 / BT.2020");
                            if (index >= 0)
                                ColorPresetSupportList[index] = "Rec.2020";
                        }
                        else if (numFY >= 25)
                        {
                            index = ColorPresetSupportList.FindIndex(x => x == "Rec.2020 / BT.2020");
                            if (index >= 0)
                                ColorPresetSupportList[index] = "BT.2020";
                        }
                    }
                }
                catch (FormatException e)
                {
                    Log?.Error("check Color Preset Strings Rec.2020 or BT.2020 / Rec.2020 or BT.2020..." + e.Message);
                }                
            }

            index = 0;

            Console.WriteLine("[" + m.AliasDeviceName + "] Color Preset SupportList : ");

            foreach (var h in ColorPresetSupportList)
            {
                Console.WriteLine("[" + index.ToString() + "] : " + h);
                index++;
            }

            return Task.FromResult(ColorPresetSupportList);
        }

        public Task<bool> WriteColorPreset(MonitorInfo m, string ColorPreset_Name, ISettingsManagerDev _SettingsPlugin = null, int colorPresetRunType = 0)
        {
            if (Log != null)
            {
                Log.Info($"WriteColorPreset requested ...");
            }

            if (_SettingsPlugin != null)
            {
                List<ColorPresetSettings> config = _SettingsPlugin.ReadColorPresetSettings().Result;

                Test_AddAppCollectionData.GetInstance()._monitorConfigs = config;

                int index = get_index_of_json_config_for_cur_monitor(m);


                if (colorPresetRunType == (int)ColorPresetRunType.Manual)
                {
                    if (index >= 0)
                    {
                        var nColorVCPCoreValue = GetColorVCPCoreValue(ColorPreset_Name).Result;

                        Test_AddAppCollectionData.GetInstance()._monitorConfigs[index].RunType = (int)ColorPresetRunType.Manual;
                        //Test_AddAppCollectionData.GetInstance()._monitorConfigs[index].PresetForManual = ColorPreset_Name;

                        Test_AddAppCollectionData.GetInstance()._monitorConfigs[index].ColorForManual = nColorVCPCoreValue;
                    }
                }
                else if (colorPresetRunType == (int)ColorPresetRunType.Auto)
                {
                    if (index >= 0)
                    {
                        var nColorVCPCoreValue = GetColorVCPCoreValue(ColorPreset_Name).Result;

                        Test_AddAppCollectionData.GetInstance()._monitorConfigs[index].RunType = (int)ColorPresetRunType.Auto;
                        //Test_AddAppCollectionData.GetInstance()._monitorConfigs[index].PresetForManual = ColorPreset_Name;

                        Test_AddAppCollectionData.GetInstance()._monitorConfigs[index].ColorForManual = nColorVCPCoreValue;
                    }

                }

                _SettingsPlugin.WriteColorPresetSettings(config);
                Thread.Sleep(100);

                if (index >= 0)
                {
                    if (Test_AddAppCollectionData.GetInstance()._monitorConfigs[index].ColorManagement_Status == (int)ColorManagementStatus.On &&
                        Test_AddAppCollectionData.GetInstance()._monitorConfigs[index].ColorManagement_RunType == (int)ColorManagementRunType.Bymonitor)
                    {
                        SetMonitorProfile(m, ColorPreset_Name);
                    }
                }

            }

            return Task.FromResult(true);
            //return Task.FromResult(Test_AddAppCollectionData.GetInstance()._monitorConfigs);
        }

        /*
        public Task<List<ColorPresetSettings>> WriteColorPreset(MonitorInfo m, string ColorPreset_Name, List<ColorPresetSettings> config)
        {
            if (Log != null)
            {
                Log.Info($"WriteColorPreset requested ...");
            }

            Test_AddAppCollectionData.GetInstance()._monitorConfigs = config;

            int index = get_index_of_json_config_for_cur_monitor(m);

            if (index >= 0)
            {
                Test_AddAppCollectionData.GetInstance()._monitorConfigs[index].RunType = (int)ColorPresetRunType.Manual;
                Test_AddAppCollectionData.GetInstance()._monitorConfigs[index].PresetForManual = ColorPreset_Name;
            }

            return Task.FromResult(Test_AddAppCollectionData.GetInstance()._monitorConfigs);
        }
        */

        public Task<List<ColorPresetSettings>> WriteColorPreset_AUTO(MonitorInfo m, string ColorPreset_Name, List<ColorPresetSettings> config)
        {
            if (Log != null)
            {
                Log.Info($"WriteColorPreset_AUTO requested ...");
            }

            Test_AddAppCollectionData.GetInstance()._monitorConfigs = config;

            int index = get_index_of_json_config_for_cur_monitor(m);

            if (index >= 0)
            {
                var nColorVCPCoreValue = GetColorVCPCoreValue(ColorPreset_Name).Result;

                Test_AddAppCollectionData.GetInstance()._monitorConfigs[index].RunType = (int)ColorPresetRunType.Auto;
                //Test_AddAppCollectionData.GetInstance()._monitorConfigs[index].PresetForManual = ColorPreset_Name;

                Test_AddAppCollectionData.GetInstance()._monitorConfigs[index].ColorForManual = nColorVCPCoreValue;
            }

            return Task.FromResult(Test_AddAppCollectionData.GetInstance()._monitorConfigs);
        }

        public Task<string> GetColorPresetName(int Color_VCPCore_E2)
        {
            if (Log != null)
            {
                Log.Info($"GetColorPresetName requested ...");
            }

            // 判斷Key是否存在
            // 若存在，回傳True，將Key為Color_VCPCore_E2的Value，帶入tmp
            if (!VcpCodeList.VCPE2.TryGetValue(Color_VCPCore_E2, out string tmp))
            {
                return Task.FromResult(string.Empty);
            }
            else
            {
                return Task.FromResult(tmp);
            }
        }

        public Task<int> GetColorVCPCoreValue(string ColorPreset_Name)
        {
            if (Log != null)
            {
                Log.Info($"GetColorVCPCoreValue requested ...");
            }

            // 判斷Key是否存在
            // 若存在，回傳True，將Key為ColorPreset_Name的Value，帶入tmp
            if (!VcpCodeList.VCPE2_ref.TryGetValue(ColorPreset_Name, out int tmp))
            {
                return Task.FromResult(-1);
            }
            else
            {
                return Task.FromResult(tmp);
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
        private void writelog(string text, log_type log_type = log_type.info)
        {
            text = "[ColorPreset] " + text;
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
                AppsCollectShell appshell = new AppsCollectShell(iconFolderPath);
                _AllAppData = appshell.FindAppsbyShell();
            }
            writelog($"appshell.FindAppsbyShell get app count({_AllAppData.Count})");
        }

        // The cryptographic service provider.
        private SHA256 Sha256 = SHA256.Create();

        // Compute the file's hash.
        private byte[] GetHashSha256(string filename)
        {
            using (FileStream stream = System.IO.File.OpenRead(filename))
            {
                return Sha256.ComputeHash(stream);
            }
        }

        // 驗證伺服器證書
        private bool CheckCertificateIsVaild(X509Certificate2 certificate)
        {
            bool result = false;
            try
            {
                X509Chain x509Chain = new X509Chain();
                x509Chain.ChainPolicy.RevocationFlag = X509RevocationFlag.EntireChain;
                x509Chain.ChainPolicy.RevocationMode = X509RevocationMode.Online;
                x509Chain.ChainPolicy.UrlRetrievalTimeout = new System.TimeSpan(0, 1, 0);
                x509Chain.ChainPolicy.VerificationFlags = X509VerificationFlags.NoFlag;
                result = x509Chain.Build(certificate);
            }
            catch (System.Exception ex)
            {
                //Console.WriteLine("[CheckCertificateIsVaild] error: " + ex.Message);
                writelog("[CheckCertificateIsVaild] error: " + ex.Message);
            }
            return result;
        }

        private bool CheckIssuerAndSubject(X509Certificate2 certificate)
        {
            try
            {
                Console.WriteLine($"Issuer:{certificate.Issuer.ToString()}");
                Console.WriteLine($"Subject:{certificate.Subject.ToString()}");
                writelog($"Issuer:{certificate.Issuer.ToString()}");
                writelog($"Subject:{certificate.Subject.ToString()}");

                foreach (string s in Issuers)
                {
                    if (!certificate.Issuer.Contains(s))
                    {
                        Console.WriteLine("[CheckIssuerAndSubject] Not match.");
                        writelog("[CheckIssuerAndSubject] Not match.");
                        return false;
                    }
                }

                foreach (string s in Subjects)
                {
                    if (!certificate.Subject.Contains(s))
                    {
                        Console.WriteLine("[CheckIssuerAndSubject] Not match.");
                        writelog("[CheckIssuerAndSubject] Not match.");
                        return false;
                    }
                }

                return true;
            }
            catch
            {
                Console.WriteLine("[CheckIssuerAndSubject] Error.");
                writelog("[CheckIssuerAndSubject] Error.");
            }
            return false;
        }

        private IIC_Metadata RunDeserializeObject(string value)
        {
            IIC_Metadata retList = new IIC_Metadata();

            try
            {
                // 20240725 jim remove
                //Dictionary<string, List<ICC_SupportDeviceName>> _temp = new Dictionary<string, List<ICC_SupportDeviceName>>() { };

                retList._support_ICC_DeviceName = JsonConvert.DeserializeObject<Dictionary<string, List<ICC_SupportDeviceName>>>(value);

                //retList._support_ICC_DeviceName = JsonConvert.DeserializeObject<List<Dictionary<string, List<ICC_SupportDeviceName>>>>(value);

                //retList._supportDeviceName

                //retList = JsonConvert.DeserializeObject<IIC_Metadata>(value);
            }
            catch (System.Exception ex)
            {
                Console.WriteLine("[DownloadICCData] RunDeserializeObject() error:" + ex.Message.ToString());
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

        public bool CheckCA(string URL)
        {
            bool flag = false;
            int num = 1;
            GetLocalTrustedCert();
            while (!flag && num > 0)
            {
                try
                {
                    HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(URL);
                    httpWebRequest.ServerCertificateValidationCallback = PinPublicKey;
                    httpWebRequest.GetResponse();
                    flag = true;
                }
                catch (System.Exception ex)
                {
                    Console.WriteLine("[CheckCA] error:" + ex.Message.ToString());
                    writelog("[CheckCA] error:" + ex.Message.ToString());
                    flag = false;
                    Console.WriteLine(string.Format("[CheckCA] error, retry:" + num));
                    writelog(string.Format("[CheckCA] error, retry:" + num));
                    Thread.Sleep(1000);
                }
                num--;
            }
            Console.WriteLine(string.Format("[CheckCA] res:" + flag));
            writelog(string.Format("[CheckCA] res:" + flag));
            if (!flag)
            {
                flag = CheckCAHTTP(URL);
            }
            if (!flag)
            {
                Console.WriteLine(string.Format("[CheckCA][CheckCAHTTP] Fail, Send Telemetry." + flag));
                writelog(string.Format("[CheckCA][CheckCAHTTP] Fail, Send Telemetry." + flag));
            }

            return flag;
        }

        private bool CheckCAHTTP(string URL)
        {
            try
            {
                if (CheckHTTPAvailable(URL))
                {
                    HttpClientHandler httpClientHandler = new HttpClientHandler();
                    httpClientHandler.ServerCertificateCustomValidationCallback = ValidateCertificate;
                    HttpClient client = new HttpClient(httpClientHandler);
                    bool response = GetResponse(client, URL);
                    Console.WriteLine("[CheckCAHTTP] result:" + response);
                    writelog("[CheckCAHTTP] result:" + response);
                    return response;
                }
            }
            catch (System.Exception ex)
            {
                Console.WriteLine("[CheckCAHTTP] error:" + ex.Message.ToString());
                writelog("[CheckCAHTTP] error:" + ex.Message.ToString());
            }
            return false;
        }

        private bool ValidateCertificate(HttpRequestMessage request, X509Certificate2? certificate, X509Chain? chain, SslPolicyErrors sslPolicyErrors)
        {
            if (certificate == null)
            {
                Console.WriteLine("[ValidateCertificate] certificate null.");
                writelog("[ValidateCertificate] certificate null.");
                return false;
            }

            if (request == null)
            {
                Console.WriteLine("[ValidateCertificate] request null.");
                writelog("[ValidateCertificate] request null.");
            }

            if (sslPolicyErrors != 0)
            {
                Console.WriteLine($"[ValidateCertificate] sslPolicyErrors is Error. {sslPolicyErrors}");
                writelog($"[ValidateCertificate] sslPolicyErrors is Error. {sslPolicyErrors}");
            }

            if (chain == null)
            {
                Console.WriteLine("[ValidateCertificate] chain null.");
                writelog("[ValidateCertificate] chain null.");
                return false;
            }
            return CheckCertificateIsVaild(certificate) && CheckIssuerAndSubject(certificate);
        }

        private bool PinPublicKey(object sender, X509Certificate? certificate, X509Chain? chain, SslPolicyErrors sslPolicyErrors)
        {
            X509Certificate2 certificate2 = new X509Certificate2(certificate);
            if (certificate == null)
            {
                Console.WriteLine("[PinPublicKey] certificate null.");
                writelog("[PinPublicKey] certificate null.");
                return false;
            }
            HttpWebRequest httpWebRequest = sender as HttpWebRequest;
            if (httpWebRequest == null)
            {
                Console.WriteLine("[PinPublicKey] request null.");
                writelog("[PinPublicKey] request null.");
            }
            if (chain == null)
            {
                Console.WriteLine("[PinPublicKey] chain null.");
                writelog("[PinPublicKey] chain null.");
                return false;
            }

            return CheckIssuerAndSubject(certificate2) && CheckCertificateIsVaild(certificate2);
        }

        private bool CheckHTTPAvailable(string URL)
        {
            try
            {
                HttpClientHandler handler = new HttpClientHandler();
                HttpClient httpClient = new HttpClient(handler);
                httpClient.GetAsync(URL).GetAwaiter().GetResult();
                return true;
            }
            catch (System.Exception ex)
            {
                Console.WriteLine("[CheckHTTPAvailable] error:" + ex.Message.ToString());
                writelog("[CheckHTTPAvailable] error:" + ex.Message.ToString());
            }
            return false;
        }

        private bool GetResponse(HttpClient client, string URL)
        {
            bool flag = false;
            int num = 5;
            while (!flag && num > 0)
            {
                try
                {
                    HttpResponseMessage result = client.GetAsync(URL).GetAwaiter().GetResult();
                    Console.WriteLine("[GetResponse] statusCode:" + result.StatusCode);
                    writelog("[GetResponse] statusCode:" + result.StatusCode);
                    flag = true;
                }
                catch (System.Exception ex)
                {
                    Console.WriteLine("[GetResponse] error:" + ex.Message.ToString());
                    writelog("[GetResponse] error:" + ex.Message.ToString());
                    flag = false;
                    Console.WriteLine(string.Format("[GetResponse] error, retry:" + num));
                    writelog(string.Format("[GetResponse] error, retry:" + num));
                    Thread.Sleep(1000);
                }
                num--;
            }
            Console.WriteLine(string.Format("[GetResponse] result:" + flag));
            writelog(string.Format("[GetResponse] result:" + flag));
            return flag;
        }

        private void GetLocalTrustedCert()
        {
            try
            {
                X509Store x509Store = new X509Store(StoreName.TrustedPublisher, StoreLocation.LocalMachine);
                x509Store.Open(OpenFlags.ReadOnly);
                foreach (X509Certificate2 certificate in x509Store.Certificates)
                {
                    TrustedPublisher.Add(certificate);
                }
                x509Store = new X509Store(StoreName.Root, StoreLocation.LocalMachine);
                x509Store.Open(OpenFlags.ReadOnly);
                foreach (X509Certificate2 certificate2 in x509Store.Certificates)
                {
                    TrustedRoot.Add(certificate2);
                }
                x509Store.Close();
            }
            catch (System.Exception ex)
            {
                Console.WriteLine("[GetLocalTrustedCert] error:" + ex.Message.ToString());
                writelog("[GetLocalTrustedCert] error:" + ex.Message.ToString());
            }
        }

        bool CheckICC_JSON_Security(string filePath, out string strJson)
        {
            strJson = string.Empty;
            bool ret = false;
            writelog("[CheckICC_JSON_Security] :" + filePath);
           
            List<string> InfoPkey = new List<string>();
            if(_SettingsPlugin != null)
            {
                InfoPkey = _SettingsPlugin.GetInfos().Result;
            }
            if(InfoPkey == null || InfoPkey.Count == 0)
            {
                //if read info failed, load default key as well
                InfoPkey = new List<string>();
                InfoPkey.Add(DDPM.SA.Obfuscation.InfoHash.Info_Hash);
            }
            string szInfo = string.Empty;
            ret = DDPM.SA.Common.Settings.DDPMFileSecurity.VerifyDDPMMetadata(Log, filePath, InfoPkey, out szInfo, out strJson);

            if (!string.IsNullOrEmpty(szInfo) && _SettingsPlugin != null)
            {
                _SettingsPlugin.AddInfo(szInfo);//pass info to settings manager and judge if new to add
            }                       

            return ret;
        }


        /// <summary>
        /// 從伺服端下載ICC.json檔，下載後會 Run Deserialize接續下載ICC profile .icm檔
        /// </summary>
        /// <param name="m">Monitor Info</param>
        /// <returns> Run Deserialize ICC.json後的 object   </returns>
        public Task<IIC_Metadata> DownloadICCData(MonitorInfo m, string savelPath = "")
        {
            try
            {
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

                if (!Directory.Exists(strICC_Folder))
                {
                    Directory.CreateDirectory(strICC_Folder);
                }

                _ICC_Metadata.strICC_Folder = String.Format($"{strICC_Folder}");

                string url = string.Empty;
                //string strFilePath = string.Empty;
                download = new Download(_logs);
                string downloadInfo = string.Empty;
                // 20240627 jim add
                string str_EnableDDPMMetadataTest = string.Empty;
                string str_IncludeTestPath = string.Empty;

                // 20240627 jim add
                RegistryKey localKey64 = RegistryKey.OpenBaseKey(Microsoft.Win32.RegistryHive.LocalMachine, RegistryView.Registry64);

                if (localKey64 != null)
                {
                    RegistryKey registryKey = localKey64.OpenSubKey(@"SOFTWARE\DELL\Dell Display and Peripheral Manager\UpdateServer", false);

                    if (registryKey != null)
                    {
                        object obj_tmp_key_EnableDDPMMetadataTest = registryKey?.GetValue("EnableDDPMMetadataTest");
                        object obj_tmp_keyIncludeTestPath = registryKey?.GetValue("IncludeTestPath");

                        if (obj_tmp_key_EnableDDPMMetadataTest != null)
                        {
                            str_EnableDDPMMetadataTest = (string)obj_tmp_key_EnableDDPMMetadataTest;
                        }

                        if (obj_tmp_keyIncludeTestPath != null)
                        {
                            str_IncludeTestPath = (string)obj_tmp_keyIncludeTestPath;
                        }
                    }
                }

                if (str_EnableDDPMMetadataTest.ToUpper().Contains("TRUE"))
                {
                    string str_url_prefix = @"https://clientperipherals.dell.com/DDPM/";
                    str_url_prefix += str_IncludeTestPath;
                    str_url_prefix += @"/Windows/Display/ICC/";
                    str_url_prefix += @"icc_profile_sha256_new.json";

                    url = str_url_prefix;

                    if (!string.IsNullOrEmpty(url))
                    {
                        //20240920 Add Security
                        string FileInfo;
                        if (!DDPM.SA.Common.Settings.DDPMFileSecurity.SRemoveSymbolicFolder(strICC_Folder, out FileInfo))
                        {
                            writelog($"[DownloadICCData] {FileInfo}");
                            return Task.FromResult(_ICC_Metadata);
                        }

                        strFilePath = Path.Combine(strICC_Folder, Path.GetFileName(url));

                        download.DownloadFile(url, strFilePath, out downloadInfo);

                        if (System.IO.File.Exists(strFilePath))
                        {
                            //Elsa Add Security
                            if (!DDPM.SA.Common.Settings.DDPMFileSecurity.IsFilePathValid(strFilePath, out FileInfo))
                            {
                                writelog($"[DownloadICCData] {FileInfo}");
                                //return null;
                                return Task.FromResult(_ICC_Metadata);
                            }

                            strReadJson = string.Empty;
                            if (!CheckICC_JSON_Security(strFilePath, out strReadJson))
                            {
                                writelog($"[DownloadICCData] CheckICC_JSON_Security Fail. {strFilePath}");
                                return Task.FromResult(_ICC_Metadata);
                            }

                            if (strReadJson.Length < 1)
                            {
                                using (var reader = new StreamReader(strFilePath))
                                {
                                    strReadJson = reader.ReadToEnd();
                                }
                            }

                            if (strReadJson == string.Empty || strReadJson.Length == 0)
                                return Task.FromResult(_ICC_Metadata);

                            try
                            {

                                _ICC_Metadata = RunDeserializeObject(strReadJson);
                                _ICC_Metadata.strICC_Folder = strICC_Folder;
                                _ICC_Metadata.Is_Support_ICC_DeviceName = false;
                            }
                            catch (System.Exception ex)
                            {
                                Console.WriteLine("[DownloadICCData] RunDeserializeObject error:" + ex.Message.ToString());
                                writelog("[DownloadICCData] RunDeserializeObject error:" + ex.Message.ToString());
                            }
                        }


                        foreach (var kvp in _ICC_Metadata._support_ICC_DeviceName)
                        {
                            Trace.WriteLine($" Model name = {kvp.Key}");
                        }

                        Trace.WriteLine($"m.modelName = {m.modelName} ");

                        var lookup = _ICC_Metadata._support_ICC_DeviceName.First(x => x.Key.Equals(m.modelName, StringComparison.OrdinalIgnoreCase));

                        Trace.WriteLine($"lookup.Key = {lookup.Key} ");

                        if (lookup.Key != null)
                        {
                            _ICC_Metadata._match_ICC_DeviceName = lookup.Value;
                            _ICC_Metadata.Is_Support_ICC_DeviceName = true;
                        }
                        else
                        {
                            _ICC_Metadata._match_ICC_DeviceName.Clear();
                            _ICC_Metadata.Is_Support_ICC_DeviceName = false;
                        }

                        int count = _ICC_Metadata._match_ICC_DeviceName.Count;

                        // 20240725 jim add
                        str_url_prefix = string.Empty;

                        str_url_prefix = @"https://clientperipherals.dell.com/DDPM/";
                        str_url_prefix += str_IncludeTestPath;
                        str_url_prefix += @"/Windows/Display/ICC/";
                        str_url_prefix += m.modelName;
                        str_url_prefix += @"/";

                        for (int i = 0; i < count; i++)
                        {
                            url = string.Empty;

                            url = str_url_prefix + _ICC_Metadata._match_ICC_DeviceName[i].File;

                            strFilePath = Path.Combine(strICC_Folder, Path.GetFileName(url));

                            download.DownloadFile(url, strFilePath, out downloadInfo);

                            string txtSha256 = BytesToString(GetHashSha256(strFilePath));

                            if (!string.Equals(txtSha256, _ICC_Metadata._match_ICC_DeviceName[i].SHA256, StringComparison.OrdinalIgnoreCase))
                            {
                                writelog($"[DownloadICCData] {_ICC_Metadata._match_ICC_DeviceName[i]} SHA256 error: icc profile sha256 download = {txtSha256} , icc profile sha256 json = {_ICC_Metadata._match_ICC_DeviceName[i].SHA256}");
                            }

                        }

                    }
                }


                return Task.FromResult(_ICC_Metadata);
            }
            catch (Exception ex)
            {
                return Task.FromResult(_ICC_Metadata);
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
            if (_ICC_Metadata._match_ICC_DeviceName != null)
            {
                int count = _ICC_Metadata._match_ICC_DeviceName.Count;

                for (int i = 0; i < count; i++)
                {
                    string[] separators = { "|" };
                    string[] strICC_ColorPresets = _ICC_Metadata._match_ICC_DeviceName[i].ColorPreset.Split(separators, StringSplitOptions.None);

                    if (string.Equals(ColorPreset_Name, "Standard/Native", StringComparison.OrdinalIgnoreCase))
                    {
                        if (string.Equals(strICC_ColorPresets[0], "Standard", StringComparison.OrdinalIgnoreCase) || string.Equals(strICC_ColorPresets[0], "Native", StringComparison.OrdinalIgnoreCase))
                        {
                            MonitorProfile.SetMonitorProfile(_ICC_Metadata._match_ICC_DeviceName[i].File);
                            break;
                        }
                    }
                    else if (string.Equals(ColorPreset_Name, "Game/Game1", StringComparison.OrdinalIgnoreCase))
                    {
                        if (string.Equals(strICC_ColorPresets[0], "Game", StringComparison.OrdinalIgnoreCase) || string.Equals(strICC_ColorPresets[0], "Game1", StringComparison.OrdinalIgnoreCase))
                        {
                            MonitorProfile.SetMonitorProfile(_ICC_Metadata._match_ICC_DeviceName[i].File);
                            break;
                        }
                    }
                    else if (string.Equals(ColorPreset_Name, "Rec. 709 / BT.709", StringComparison.OrdinalIgnoreCase))
                    {
                        if (strICC_ColorPresets[0].Contains("Rec", StringComparison.OrdinalIgnoreCase) || strICC_ColorPresets[0].Contains("BT.", StringComparison.OrdinalIgnoreCase) || strICC_ColorPresets[0].Contains("709", StringComparison.OrdinalIgnoreCase))
                        {
                            MonitorProfile.SetMonitorProfile(_ICC_Metadata._match_ICC_DeviceName[i].File);
                            break;
                        }
                    }

                    if (string.Equals(ColorPreset_Name, strICC_ColorPresets[0], StringComparison.OrdinalIgnoreCase))
                    {
                        MonitorProfile.SetMonitorProfile(_ICC_Metadata._match_ICC_DeviceName[i].File);
                        break;
                    }
                }
            }

            return Task.FromResult(true);
        }

        /// <summary>
        /// DDPM MonitorSettings Data Export for Color
        /// </summary>
        /// <param name="MonitorInfo"></param>
        /// <param name="_SettingsPlugin"></param>
        /// <returns></returns>
        public Task<ColorPresetSettings> Export(MonitorInfo MonitorInfo, ISettingsManagerDev _SettingsPlugin)
        {
            List<ColorPresetSettings> config = _SettingsPlugin.ReadColorPresetSettings().Result;

            ColorPresetSettings curColorPresetSetting = get_cur_monitor_preset_config(MonitorInfo, config);

            if (curColorPresetSetting.AppInfo.Count <= 0)
            {
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

            return Task.FromResult(curColorPresetSetting);

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
            List<ColorPresetSettings> config = _SettingsPlugin.ReadColorPresetSettings().Result;

            Test_AddAppCollectionData.GetInstance()._monitorConfigs = config;

            int index = get_index_of_json_config_for_cur_monitor(MonitorInfo);

            if (index >= 0)
            {
                Test_AddAppCollectionData.GetInstance()._monitorConfigs[index].RunType = colorPresetSetting_Import.RunType;
                Test_AddAppCollectionData.GetInstance()._monitorConfigs[index].ColorForManual = colorPresetSetting_Import.ColorForManual;
                Test_AddAppCollectionData.GetInstance()._monitorConfigs[index].ColorManagement_Status = colorPresetSetting_Import.ColorManagement_Status;
                Test_AddAppCollectionData.GetInstance()._monitorConfigs[index].ColorManagement_RunType = colorPresetSetting_Import.ColorManagement_RunType;

                Test_AddAppCollectionData.GetInstance()._monitorConfigs[index].AppInfo = colorPresetSetting_Import.AppInfo;

                _SettingsPlugin.WriteColorPresetSettings(config);

                return Task.FromResult(true);
            }

            if (index < 0)
            {
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
                    foreach (KeyValuePair<string, ColorPresetSettings_AppInfo> kvp in colorPresetSetting_Import.AppInfo)
                    {
                        ColorPresetSettings_AppInfo temp = new ColorPresetSettings_AppInfo();

                        temp.Color = kvp.Value.Color;
                        temp.HDRColor = kvp.Value.HDRColor;
                        temp.IconName = kvp.Value.IconName;

                        Test_AddAppCollectionData.GetInstance()._monitorConfigs[index].AppInfo.Add(kvp.Key, temp);

                    }
                }

                _SettingsPlugin.WriteColorPresetSettings(config);

                return Task.FromResult(true);
            }

            return Task.FromResult(false);
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
            List<ColorPresetSettings> config = _SettingsPlugin.ReadColorPresetSettings().Result;

            Test_AddAppCollectionData.GetInstance()._monitorConfigs = config;

            int index = -1;

            if (Test_AddAppCollectionData.GetInstance()._monitorConfigs != null)
            {
                if (Test_AddAppCollectionData.GetInstance()._monitorConfigs.Count > 0)
                {
                    index = Test_AddAppCollectionData.GetInstance()._monitorConfigs.FindIndex(x =>
                                                    x.ModelName.Trim() == Model.Trim() &&
                                                    x.ServiceTag.Trim() == ServiceTag.Trim());                    
                }
            }

            if (index >= 0)
            {
                if (colorPresetSetting_Migration.Auto)
                    Test_AddAppCollectionData.GetInstance()._monitorConfigs[index].RunType = (int)ColorPresetRunType.Auto;
                else
                    Test_AddAppCollectionData.GetInstance()._monitorConfigs[index].RunType = (int)ColorPresetRunType.Manual;
                
                Test_AddAppCollectionData.GetInstance()._monitorConfigs[index].ColorForManual = ColorForManual_VCPE2Code_value;

                if (colorPresetSetting_Migration.ColorManagement == 0)
                {
                    Test_AddAppCollectionData.GetInstance()._monitorConfigs[index].ColorManagement_Status = 0;
                    Test_AddAppCollectionData.GetInstance()._monitorConfigs[index].ColorManagement_RunType = 0;
                }
                else if (colorPresetSetting_Migration.ColorManagement == 1)
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

                if (colorPresetSetting_Migration.AppInfos.Count > 0)
                { 
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

                            foreach (KeyValuePair<string, InstalledAppInfo> kvp_applist in _AllAppData)
                            {
                               if ( strAppName == kvp_applist.Value.AppName)
                               {
                                    temp.IconName = _SettingsPlugin.GetAppIconFolderPath() + "\\" + strAppName + ".png";
                                }

                            }                   

                            Test_AddAppCollectionData.GetInstance()._monitorConfigs[index].AppInfo.Add(strAppName, temp);

                        }
                    }  
                }
                _SettingsPlugin.WriteColorPresetSettings(config);

                return Task.FromResult(true);
            }

            if (index < 0)
            {
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

                if (Test_AddAppCollectionData.GetInstance()._monitorConfigs != null)
                {
                    if (Test_AddAppCollectionData.GetInstance()._monitorConfigs.Count > 0)
                    {
                        index = Test_AddAppCollectionData.GetInstance()._monitorConfigs.FindIndex(x =>
                                                        x.ModelName.Trim() == Model.Trim() &&
                                                        x.ServiceTag.Trim() == ServiceTag.Trim());
                    }
                }

                if (index >= 0 )
                {
                    if (colorPresetSetting_Migration.Auto)
                        Test_AddAppCollectionData.GetInstance()._monitorConfigs[index].RunType = (int)ColorPresetRunType.Auto;
                    else
                        Test_AddAppCollectionData.GetInstance()._monitorConfigs[index].RunType = (int)ColorPresetRunType.Manual;

                    Test_AddAppCollectionData.GetInstance()._monitorConfigs[index].ColorForManual = ColorForManual_VCPE2Code_value;

                    if (colorPresetSetting_Migration.ColorManagement == 0)
                    {
                        Test_AddAppCollectionData.GetInstance()._monitorConfigs[index].ColorManagement_Status = 0;
                        Test_AddAppCollectionData.GetInstance()._monitorConfigs[index].ColorManagement_RunType = 0;
                    }
                    else if (colorPresetSetting_Migration.ColorManagement == 1)
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

                }

                if (index >= 0 && colorPresetSetting_Migration.AppInfos.Count > 0)
                {
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

                            foreach (KeyValuePair<string, InstalledAppInfo> kvp_applist in _AllAppData)
                            {
                                if (strAppName == kvp_applist.Value.AppName)
                                {
                                    temp.IconName = _SettingsPlugin.GetAppIconFolderPath() + "\\" + colorPresetSetting_Migration.AppInfos[i].ExeName + ".png";
                                }

                            }

                            Test_AddAppCollectionData.GetInstance()._monitorConfigs[index].AppInfo.Add(strAppName, temp);

                        }
                    }                  
                }

                _SettingsPlugin.WriteColorPresetSettings(config);

                return Task.FromResult(true);
            }

            return Task.FromResult(false);
        }

        #endregion
    }
}
