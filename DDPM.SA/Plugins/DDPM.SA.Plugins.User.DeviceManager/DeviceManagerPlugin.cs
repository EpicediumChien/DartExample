#region LicenceHeader

//
// Copyright © 2024, Dell Inc., All Rights Reserved.
// This material is confidential and a trade secret.  Permission to use this
// work for any purpose must be obtained in writing from Dell Inc.
//
// DisplayMangerPlugin.cs created on 25/04/2024T07:20 PM
//

#endregion

using DDPM.MonitorBorker;
using DDPM.SA.Common;
using DDPM.SA.Common.Display;
using DDPM.SA.Common.Popup;
using DDPM.SA.Common.Settings;
using DDPM.SA.Common.UpdateProgressPage;
using DDPM.ShowOSD;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.Common.Annotations;
using Dell.Client.Framework.Common.Extensions;
using Dell.Client.Framework.Common.PluginConditions;
using Dell.Client.Framework.Interfaces;
using DPeMPublic.Common.Enums;
using IndiLogic.DPeM.Broker;
using Microsoft;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;
using System.Windows.Threading;
using VcpCore.Common;
using WinCopies.Util;
using Windows.System;
using static VcpCore.Common.User32;
using IDs = DDPM.SA.Common.IDs;

namespace DDPM.SA.Plugins.User.DeviceManager
{
    [Plugin(IDs.Device_Manager_Plugin_ID, pluginName, PluginOrderGroupType.Core, Version = pluginVersion)]
    [Descriptor(Description = pluginDescription)]
    [Publisher(Name = publisherCompany, Website = publisherWebsite, Support = publisherSupport)]
    [PublishedUnelevatedInterface(new[] { typeof(IDeviceManagerSA) })]
    [PluginRequires(Id = IDs.Display_Manager_PLUGIN_ID, AllowDynamicResolving = true)]
    [PluginRequires(Id = IDs.Scheduler_Manager_Plugin_ID, AllowDynamicResolving = true)]
    [PluginRequires(Id = IDs.DDPM_PERIPHERALS_PLUGIN_ID, AllowDynamicResolving = true)]
    [PluginRequires(Id = IDs.DDPM_SETTINGSMANAGER_SA_PLUGIN_ID, AllowDynamicResolving = true)]
    [PluginRequires(Id = IDs.CLI_Manager_Plugin, AllowDynamicResolving = true)]
    [DependencyKnownTypes(new[] { typeof(IDisplayService), typeof(ISchedulerManager), typeof(IDPeMPlugin), typeof(ISettingsManagerDev), typeof(IFWUpdateService), typeof(ISWUpdateService) })]

    public class DeviceMangerPlugin : BaseAgentPlugin, IDisposableObservable, IDeviceManagerSA
    {
        #region Private Members

        private const string pluginName = "DeviceManagerPlugin";
        private const string pluginVersion = "1.0.0";
        private const string pluginDescription = "This plugin implements Device Manager Plugin.";
        private const string publisherCompany = "Wistron";
        private const string publisherWebsite = "https://www.wistron.com";
        private const string publisherSupport = "This plugin implements Device Manager Plugin.";

        private IAgent _agent;
        public const string PluginLogId = "DeviceManager";

        private IDisplayService _DisplayManagerPlugin;
        private IDPeMPlugin _PeripheralsPlugin;
        private ISettingsManagerDev _SettingsPlugin;
        private IColorPresetSA _ColorPresetPlugin;
        private IFWUpdateService _FWUpdatePlugin;
        private INKVMService _NKVMPlugin;
        private IHotkey _HotkeyPlugin;
        private ISWUpdateService _SWUpdatePlugin;
    private ISchedulerManager _ScheduleManagerPlugin;
    private IDTPProxyPlugin _DTPProxyPlugin;

    private readonly object _PluginConditionLock = new object();
        private readonly object _PluginConditionLock_Display = new object();
        private readonly object _PluginConditionLock_Peripherals = new object();
        private readonly object _PluginConditionLock_Settings = new object();
        private readonly object _PluginConditionLock_ColorPreset = new object();
        private readonly object _PluginConditionLock_NKVM = new object();
        private readonly object _PluginConditionLock_Hotkey = new object();
    private readonly object _PluginConditionLock_ScheduleManager = new object();
    private readonly object _PluginConditionLock_DTPProxy = new object();
    DisplayChange displayChange;

        // ColorPreset objects
        private Dictionary<string, InstalledAppInfo> _AllAppData_tmp = new Dictionary<string, InstalledAppInfo>();
        private Dictionary<string, InstalledAppInfo> _AllAppData = new Dictionary<string, InstalledAppInfo>();
        private List<string> _SupportedColorPreset = new List<string>();

        // Jim move to here 20240621
        private ShowOSDWin OsdWin = null;

        private string iconFolderPath = string.Empty;

        private MainWindow? MonitorBorkerWin = null; //Dean 0626 fix SAST issue, remove static

        private Thread newWindowThread_AutoSetColorPresetForMonitorConfig = null;

        //Monitor objects
        private List<MonitorInfo> _AllInfoMonitors = new List<MonitorInfo>();

        private bool isLetDisplayServiceIdle { get; set; } = false;

        //Input Source
        private Dictionary<string, InputInfo> _inputSourcelist = new Dictionary<string, InputInfo>();

        //KVM
        private Dictionary<string, PCsInfo> _USBKVMPCsList = new Dictionary<string, PCsInfo>();

        private List<string> _SupportedMonitorList;

        //For CMA, the param "deviceStatus" is used to recognize target status "connected" or "disconnected".
        private List<Monitor_Listen_param> _Monitor_Listening = new List<Monitor_Listen_param>();

        private List<Peripheral_Listen_param> _Peripheral_Listening = new List<Peripheral_Listen_param>();

        //hotkey settings
        private List<HotkeySettings> _hotkeySettings = new List<HotkeySettings>();

        private JobQueue _hotkeyJobQueue = new JobQueue();

        //powerNap
        private JobQueue _powerNapJobQueue = new JobQueue();

        private static System.Timers.Timer _PowerNapTimer = new System.Timers.Timer(2000);

        //FW update progress bar
        private UpdateProgress _UpdateProgress;

        //Bruce 07-30 Added total screens
        private int _lastScreenCount;

        private enum log_type
        {
            info = 0,
            error
        }

        private readonly object _MoLock = new object();

        //Bruce 0815 Added new judgment whether to trigger DisplayChang event
        private bool displayInOut = true;

        #endregion

        #region Constructor

        public DeviceMangerPlugin(IAgent agent) : base(agent, PluginLogId)
        {
            _agent = agent;
            _PowerNapTimer.Elapsed += OnPowerNapTimedRaise;
            _PowerNapTimer.AutoReset = true;
            _PowerNapTimer.Enabled = true;

            writelog("DeviceManagerPlugin constructor ...");
        }

        #endregion

        #region Overriding methods

        protected override void OnPluginStarting()
        {
            _agent.PluginManager.PluginsStarted += PluginManagerOnPluginsStarted;

            InitializeSettingsPlugin();
            InitializeDisplayManagerPlugin();
            InitializeColorPresetPlugin();
            InitializePeripheralsPlugin();
            InitializeFWUpdatePlugin();
            InitializeNKVMPlugin();
            InitializeHotkeyPlugin();
            InitializeSWUpdatePlugin();
      InitializeSchedulerManagerPlugin();
      InitializeDTPProxyPlugin();

      PluginCondition = new PluginStartedCondition();
            writelog("DeviceManager plugin started");

            Microsoft.Win32.SystemEvents.DisplaySettingsChanged += SystemEvents_DisplaySettingsChanged;
            //displayChange = new DisplayChange(Log);
            //Task.Run(() =>
            //{
            //    displayChange.Initialize_DisplayChangeEvent();
            //});
            //displayChange.DisplayChange_Event += SystemEvents_DisplaySettingsChanged;
        }

        #endregion

        #region IDeviceManager implementation

        #region EventHandlers

        public event EventHandler<VCPchangedEventArgs> VCPchanged;

        public event EventHandler<DDCCIchangedEventArgs> DDCCIStatuschanged;

        public event EventHandler<DisplaychangedEventArgs> Displaychanged;

        public event EventHandler<DeviceChangedEventArgs> DeviceChanged;

        public event EventHandler<DeviceChangedEventArgs> Peripherals_Notify;

        public event EventHandler<bool> Peripherals_UpdateNotify;

        public event EventHandler<CommandOutput_DeviceConnection> CMA_notify;

        //FW Update by Bruce
        public event EventHandler<FWUpdateInfo> ProgressUpdate_Notify;

        public event EventHandler<bool> FWU_UILock_Notify;

        /// <summary>
        /// FW Update供CLI使用
        /// </summary>
        public event EventHandler<List<FWUpdateInfo>> DownloadAndInstall_Result_Notify;

        //EasyArrange
        //
        //Notify to DDPM.UI when EAPlugin open the EditWindow for editing custom layout
        public event EventHandler<string> EAEditStarted;

        //Notify to DDPM.UI when EAPlugin has finished the edit custom layout, and sent back the result.
        public event EventHandler<string> EAEditCompleted;

        //Robert_Lin, 2024-8-4 added
        /// <summary>
        /// Notify to DDPM.UI, the EasyArrange Subagent is starting the edit window.
        /// It's a second notification after DDPM.UI called EAEditCommand() and return true.
        /// The EAEditReturn will return the edit result and messages.
        /// </summary>
        /// <param name="EAArgs">The arguments when DDPM.UI call EAEditCommand() and the return result.</param>
        /// <returns></returns>
        ///
        public event EventHandler<EAArgs> EAEditReturn;

        /// <summary>
        /// HDR status change event，return HDR status
        /// </summary>
        public event EventHandler<bool> HDRChangeEvent;
        /// <summary>
        /// gaming parameter changes event，return gaming parameter
        /// </summary>
        public event EventHandler<GamingDisplayPropertiesInfo> GamingChangeEvent;

        #endregion

        #region ColorPreset implementation

        public Task<DDPM.SA.Common.IIC_Metadata> DownloadICCData(MonitorInfo m, string savelPath = "")
        {
            DDPM.SA.Common.IIC_Metadata _ICC_Metadata = new DDPM.SA.Common.IIC_Metadata();

            if (_ColorPresetPlugin == null)
            {
                writelog("null _ColorPresetPlugin in [DownloadICCData]");
                return Task.FromResult(_ICC_Metadata);
            }
            else
            {
                _ICC_Metadata = _ColorPresetPlugin.DownloadICCData(m, savelPath).Result;
            }

            return Task.FromResult(_ICC_Metadata);
        }

        public Task<bool> ColorManagement_Off(MonitorInfo m)
        {
            DDPMSettings setting = _SettingsPlugin.ReloadAppConfigData().Result;
            setting.UserSettings.ColorManagement_off = true;
            _SettingsPlugin.SetAppConfigData(setting);

            return Task.FromResult(true);
        }

        public Task<bool> ColorManagement_Bymonitor(MonitorInfo m)
        {
            DDPMSettings setting = _SettingsPlugin.ReloadAppConfigData().Result;
            setting.UserSettings.ColorManagement_bymonitor = true;
            _SettingsPlugin.SetAppConfigData(setting);

            return Task.FromResult(true);
        }

        public Task<bool> ColorManagement_Byhost(MonitorInfo m)
        {
            DDPMSettings setting = _SettingsPlugin.ReloadAppConfigData().Result;
            setting.UserSettings.ColorManagement_byhost = true;
            _SettingsPlugin.SetAppConfigData(setting);

            return Task.FromResult(true);
        }

        public Task<List<string>> ReadColorPreset(MonitorInfo m)
        {
            //Log.Info($"ReadColorPreset requested ...");
            writelog("ColorPresetPlugin received ReadColorPreset requested ...");

            // 20240619 jim add check
            if (_SupportedColorPreset != null)
            {
                _SupportedColorPreset.Clear();

                if (_SupportedColorPreset.Count == 0)
                {
                    if (_ColorPresetPlugin == null)
                    {
                        writelog("null _ColorPresetPlugin in [ReadColorPreset]");
                        return Task.FromResult(_SupportedColorPreset);
                    }
                    else
                    {
                        // 20240619 jim modify
                        string VCP_capbility = string.Empty;
                        VCP_capbility = GetVCPCapabilities(m).Result;

                        // 20240619 jim add one retry
                        if (string.IsNullOrEmpty(VCP_capbility))
                            VCP_capbility = GetVCPCapabilities(m).Result;

                        _SupportedColorPreset = _ColorPresetPlugin.ReadColorPreset(m, VCP_capbility).Result;
                    }
                }
            }

            return Task.FromResult(_SupportedColorPreset);
        }

        public Task<string> GetMonitorProfile(MonitorInfo m)
        {
            string Key_Profile_Name = string.Empty;
            Key_Profile_Name = MonitorProfile.GetMonitorProfile(m.DisplayName);

            return Task.FromResult(Key_Profile_Name);
        }

        public Task<bool> SetMonitorProfile(MonitorInfo m, string ColorPreset_Name)
        {
            bool blRet = true;

            if (string.Equals(ColorPreset_Name, "Standard", StringComparison.OrdinalIgnoreCase))
                blRet = MonitorProfile.SetMonitorProfile("Dell_U3224KB_Native_v2.icm");
            else if (string.Equals(ColorPreset_Name, "Display P3", StringComparison.OrdinalIgnoreCase))
                blRet = MonitorProfile.SetMonitorProfile("Dell_U3224KB_DisplayP3_v2.icm");
            else if (string.Equals(ColorPreset_Name, "DCI-P3", StringComparison.OrdinalIgnoreCase))
                blRet = MonitorProfile.SetMonitorProfile("Dell_U3224KB_DCIP3_v2.icm");
            else if (string.Equals(ColorPreset_Name, "sRGB", StringComparison.OrdinalIgnoreCase))
                blRet = MonitorProfile.SetMonitorProfile("Dell_U3224KB_sRGB_v2.icm");
            else if (string.Equals(ColorPreset_Name, "Rec. 709", StringComparison.OrdinalIgnoreCase))
                blRet = MonitorProfile.SetMonitorProfile("Dell_U3224KB_Rec709_v2.icm");

            return Task.FromResult(blRet);
        }

        public Task<string> ReadCurrentColorPreset(MonitorInfo m)
        {
            if (_DisplayManagerPlugin != null)
            {
                var result = _DisplayManagerPlugin.GetVCPCapability(m, "colorpreset").Result;
                if (result.result)
                    return Task.FromResult(result.value.ToString());
            }
            return Task.FromResult("");
        }

        public Task<bool> Notify_refresh_app_list()
        {
            bool blRet = true;

            // jim modify 20240605
            if (MonitorBorkerWin != null)
            {
                MonitorBorkerWin.Notify_refresh_app_list();
            }

            return Task.FromResult(blRet);
        }

        // 20240619 jim modify
        public Task<bool> WriteColorPreset(MonitorInfo m, string ColorPreset_Name)
        {
            writelog("DeviceManagerPlugin received WriteColorPreset requested ...");

            bool r = false;
            if (_ColorPresetPlugin == null)
            {
                writelog("null _ColorPresetPlugin in [WriteColorPreset]");
                return Task.FromResult(r);
            }

            //data process
            var tmp = _ColorPresetPlugin.WriteColorPreset(m, ColorPreset_Name, _SettingsPlugin.ReadColorPresetSettings().Result).Result;

            //write back to settings
            r = _SettingsPlugin.WriteColorPresetSettings(tmp).Result;

            Thread.Sleep(100);

            //show OSD over colorpreset plugin
            _ColorPresetPlugin.ShowOSD_ColoPreset(m, ColorPreset_Name);

            //if (r) // 20240717 jim remove
            //{
            //write VCP over display manager
            r = SetVCPCapability(m, "colorpreset", ColorPreset_Name).Result;
            //}
            return Task.FromResult(r);
        }

        // 20240619 jim modify
        public Task<bool> WriteColorPreset_AUTO(MonitorInfo m, string ColorPreset_Name)
        {
            writelog("ColorPresetPlugin received WriteColorPreset_AUTO requested ...");

            bool r = false;
            if (_ColorPresetPlugin == null)
            {
                writelog("null _ColorPresetPlugin in [WriteColorPreset_AUTO]");
                return Task.FromResult(r);
            }

            //data process
            var tmp = _ColorPresetPlugin.WriteColorPreset_AUTO(m, ColorPreset_Name, _SettingsPlugin.ReadColorPresetSettings().Result).Result;

            //write back to settings
            r = _SettingsPlugin.WriteColorPresetSettings(tmp).Result;

            Thread.Sleep(100);

            //show OSD over colorpreset plugin
            _ColorPresetPlugin.ShowOSD_ColoPreset(m, ColorPreset_Name, true, true);

            //if (r)
            //{
            //write VCP over display manager
            r = SetVCPCapability(m, "colorpreset", ColorPreset_Name).Result;

            //}
            return Task.FromResult(r);
        }

        // jim add 20240607
        public Task<bool> WriteColorPresetByColorProfile(MonitorInfo m, string ColorProfile_Name)
        {
            writelog("DeviceManagerPlugin received WriteColorPresetByColorProfile requested ...");

            bool r = false;

            string ColorPreset_Name = string.Empty;

            if (_ColorPresetPlugin == null)
            {
                writelog("null _ColorPresetPlugin in [WriteColorPreset]");
                return Task.FromResult(r);
            }

            if (string.Equals(ColorProfile_Name, "Dell_U3224KB_Native_v2.icm", StringComparison.OrdinalIgnoreCase))
            {
                ColorPreset_Name = "Standard";
            }
            else if (string.Equals(ColorProfile_Name, "Dell_U3224KB_DisplayP3_v2.icm", StringComparison.OrdinalIgnoreCase))
            {
                ColorPreset_Name = "Display P3";
            }
            else if (string.Equals(ColorProfile_Name, "Dell_U3224KB_DCIP3_v2.icm", StringComparison.OrdinalIgnoreCase))
            {
                ColorPreset_Name = "DCI-P3";
            }
            else if (string.Equals(ColorProfile_Name, "Dell_U3224KB_sRGB_v2.icm", StringComparison.OrdinalIgnoreCase))
            {
                ColorPreset_Name = "sRGB";
            }
            else if (string.Equals(ColorProfile_Name, "Dell_U3224KB_Rec709_v2.icm", StringComparison.OrdinalIgnoreCase))
            {
                ColorPreset_Name = "Rec. 709";
            }
            else if (string.Equals(ColorProfile_Name, "Dell_U3224KB_HDR_v4_MHC2.icm", StringComparison.OrdinalIgnoreCase))
            {
                ColorPreset_Name = "Desktop";
            }

            //data process
            var tmp = _ColorPresetPlugin.WriteColorPreset(m, ColorPreset_Name, _SettingsPlugin.ReadColorPresetSettings().Result).Result;

            //write back to settings
            r = _SettingsPlugin.WriteColorPresetSettings(tmp).Result;

            Thread.Sleep(100);

            //show OSD over colorpreset plugin
            _ColorPresetPlugin.ShowOSD_ColoPreset(m, ColorPreset_Name);

            if (r)
            {
                //write VCP over display manager
                r = SetVCPCapability(m, "colorpreset", ColorPreset_Name).Result;
            }

            return Task.FromResult(r);
        }

        /// <summary>
        /// 取回目前安裝在電腦裡面的APP list
        /// </summary>
        /// <param name="isReload"></param> 是否要重新更新取回
        /// <returns></returns> 取回目前安裝在電腦裡面的APP list
        public Task<Dictionary<string, InstalledAppInfo>> FindAppsbyShell(bool isReload = false)
        {
            writelog("ColorPresetPlugin received FindAppsbyShell requested ...");

            if (_AllAppData.Count == 0 || isReload == true)
            {
                _AllAppData.Clear();

                if (_ColorPresetPlugin == null)
                {
                    writelog("null _ColorPresetPlugin in [FindAppsbyShell]");
                    return Task.FromResult(_AllAppData);
                }

                string iconFolder = _SettingsPlugin.GetAppIconFolderPath().Result;
                if (!string.IsNullOrEmpty(iconFolder))
                {
                    _ColorPresetPlugin.SetAppIconFolder(iconFolder);
                }

                _AllAppData_tmp = _ColorPresetPlugin.GetInstalledAppsList(isReload).Result;

                foreach (string key in _AllAppData_tmp.Keys)
                {
                    _AllAppData.Add(key, _AllAppData_tmp[key]);
                }
            }

            return Task.FromResult(_AllAppData);
        }

        /// <summary>
        /// 在 ColorSetting setting config裡面新增 App name和其對應 color preset
        /// </summary>
        /// <param name="index_monitor"></param>
        /// <param name="AppName"></param>
        /// <param name="ColorPreset_Name"></param>
        /// <returns></returns> 是否成功
        public Task<bool> AddColorPresetForMonitorConfig(string index_monitor, string AppName, string ColorPreset_Name)
        {
            writelog("ColorPresetPlugin received AddColorPresetForMonitorConfig requested ...");

            if (_ColorPresetPlugin == null)
            {
                writelog("null _ColorPresetPlugin in [AddColorPresetForMonitorConfig]");
                return Task.FromResult(false);
            }
            if (_SettingsPlugin == null)
            {
                writelog("null _SettingsPlugin in [AddColorPresetForMonitorConfig]");
                return Task.FromResult(false);
            }
            string iconFolder = _SettingsPlugin.GetAppIconFolderPath().Result;
            if (string.IsNullOrEmpty(iconFolder))
            {
                writelog("null/empty iconFolder in _SettingsPlugin [AddColorPresetForMonitorConfig]");
                return Task.FromResult(false);
            }
            _ColorPresetPlugin.SetAppIconFolder(iconFolder);
            if (!int.TryParse(index_monitor, out int idx))
            {
                writelog(" index_monitor -> idx convert fail  [AddColorPresetForMonitorConfig]");
                return Task.FromResult(false);
            }
            MonitorInfo m = _AllInfoMonitors[idx];
            string ability = GetVCPCapabilities(m).Result;
            var read = _SettingsPlugin.ReadColorPresetSettings().Result;
            var temp = _ColorPresetPlugin.AddColorPresetForMonitorConfig(m, AppName, ColorPreset_Name, ability, read).Result;
            _SettingsPlugin.WriteColorPresetSettings(temp);
            Thread.Sleep(100);

            return Task.FromResult(true);
        }

        /// <summary>
        /// 在 ColorSetting setting config裡面更改 App name和其對應 color preset
        /// </summary>
        /// <param name="index_monitor"></param>
        /// <param name="AppName"></param>
        /// <param name="ColorPreset_Name"></param>
        public void ChangeColorPresetForMonitorConfig(string index_monitor, string AppName, string ColorPreset_Name)
        {
            writelog("ColorPresetPlugin received ChangeColorPresetForMonitorConfig requested ...");

            if (_ColorPresetPlugin == null)
            {
                writelog("null _ColorPresetPlugin in [ChangeColorPresetForMonitorConfig]");
                //return  Task.CompletedTask;
                return;
            }

            var temp = _ColorPresetPlugin.ChangeColorPresetForMonitorConfig(_AllInfoMonitors[Convert.ToInt32(index_monitor)], AppName, ColorPreset_Name, _SettingsPlugin.ReadColorPresetSettings().Result).Result;
            _SettingsPlugin.WriteColorPresetSettings(temp);
            Thread.Sleep(100);

            return;
        }

        /// <summary>
        /// 在 ColorSetting setting config裡面移除 App name和其對應 color preset
        /// </summary>
        /// <param name="index_monitor"></param>
        /// <param name="AppName"></param>
        public void DeleteColorPresetForMonitorConfig(string index_monitor, string AppName)
        {
            writelog("ColorPresetPlugin received DeleteColorPresetForMonitorConfig requested ...");

            if (_ColorPresetPlugin == null)
            {
                writelog("null _ColorPresetPlugin in [DeleteColorPresetForMonitorConfig]");

                return;
            }

            var temp = _ColorPresetPlugin.DeleteColorPresetForMonitorConfig(_AllInfoMonitors[Convert.ToInt32(index_monitor)], AppName, _SettingsPlugin.ReadColorPresetSettings().Result).Result;
            _SettingsPlugin.WriteColorPresetSettings(temp);
            Thread.Sleep(100);

            return;
        }

        /// <summary>
        /// 啟動 MonitorBorker 執行抓前景active app name
        /// </summary>
        /// <param name="m"></param>
        public void Launch_MonitorBorker(MonitorInfo m)
        {
            Log.Info($"Launch_MonitorBorker requested ...");
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
                        MonitorBorkerWin = new MainWindow(this, m);

                        MonitorBorkerWin.Show();
                    }
                    else
                    {
                        //MonitorBorkerWin.Close();
                    }
                }
            }

            return;
        }

        /// <summary>
        /// 回傳目前螢幕在 ColorSetting setting config的 index number
        /// </summary>
        /// <param name="mo"></param> 螢幕資訊
        /// <returns></returns> 回傳目前螢幕在 ColorSetting setting config的 index number
        public int get_index_of_json_config_for_cur_monitor(MonitorInfo mo)//string index_monitor)
        {
            int index = -1;

            if (Test_AddAppCollectionData.GetInstance()._monitorConfigs != null)
            {
                if (Test_AddAppCollectionData.GetInstance()._monitorConfigs.Count > 0)
                {
                    index = Test_AddAppCollectionData.GetInstance()._monitorConfigs.FindIndex(x =>
                                                    x.DeviceInfo.ModelName.Trim() == mo.edid.ModelName.Trim() &&
                                                    x.DeviceInfo.SerialNumber.Trim() == mo.edid.SerialNumber.Trim());
                }
            }
            return index;
        }

        /// <summary>
        /// 自動根據App name 去設定 color preset
        /// </summary>
        /// <param name="mo"></param> 螢幕資訊
        /// <param name="on_off"></param> 啟用/關閉 自動根據App name 去設定 color preset
        //public void AutoSetColorPresetForMonitorConfig(string index_monitor, string on_off, bool Islock = false)
        public void AutoSetColorPresetForMonitorConfig(MonitorInfo mo, string on_off, bool Islock = false)        
        {
            writelog("ColorPresetPlugin received AutoSetColorPresetForMonitorConfig requested ...");

            if (_ColorPresetPlugin == null)
            {
                writelog("null _ColorPresetPlugin in [AutoSetColorPresetForMonitorConfig]");
                return;
            }

            DDPMSettings setting = _SettingsPlugin.ReloadAppConfigData().Result;
            setting.UserSettings.IsAutoColorPreset_Lock = Islock;
            _SettingsPlugin.SetAppConfigData(setting);

            if (on_off.Equals("ON", StringComparison.OrdinalIgnoreCase))
            {
                var temp = _ColorPresetPlugin.AutoSetColorPresetForMonitorConfig(mo, on_off, _SettingsPlugin.ReadColorPresetSettings().Result).Result;
                _SettingsPlugin.WriteColorPresetSettings(temp);
                Thread.Sleep(100);

                if (newWindowThread_AutoSetColorPresetForMonitorConfig == null)
                {
                    // create a thread
                    newWindowThread_AutoSetColorPresetForMonitorConfig = new Thread(new ThreadStart(() =>
                    {
                        // create and show the window
                        Launch_MonitorBorker(mo);

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
                        MonitorBorkerWin.Set_AUTO_ColorPresetConfig(true);
                }
            }
            else if (on_off.Equals("OFF", StringComparison.OrdinalIgnoreCase))
            {
                // jim modify 20240605
                if (newWindowThread_AutoSetColorPresetForMonitorConfig != null)
                {
                    // 20240620 jim add back the code
                    var temp = _ColorPresetPlugin.AutoSetColorPresetForMonitorConfig(mo, on_off, _SettingsPlugin.ReadColorPresetSettings().Result).Result;
                    _SettingsPlugin.WriteColorPresetSettings(temp);
                    Thread.Sleep(100);

                    if (MonitorBorkerWin != null) // jim add 20240809
                        MonitorBorkerWin.Set_AUTO_ColorPresetConfig(false);
                }
            }

            return;
        }

        /// <summary>
        /// 在 螢幕上 秀出 color preset的 OSD文字
        /// </summary>
        /// <param name="m"></param> 螢幕資訊
        /// <param name="strMsg"></param>  OSD文字
        public void ShowOSD_ColoPreset(MonitorInfo m, string strMsg)//, bool isMainUI = false)
        {
            writelog("ColorPresetPlugin received ShowOSD_ColoPreset requested ...");

            if (_ColorPresetPlugin == null)
            {
                writelog("null _ColorPresetPlugin in [ShowOSD_ColoPreset]");
                return;
            }

            _ColorPresetPlugin.ShowOSD_ColoPreset(m, strMsg);
        }
        #endregion

        #region Schedule Manger implementation
        public Task StartSchedulerManger(int millisecond)
        {
            writelog("DeviceMangerPlugin received StartSchedulerManger: " + millisecond.ToString() + " requested ...");

            _ScheduleManagerPlugin.StartSchedulerManger(60000);
            return Task.FromResult(Task.CompletedTask);
        }
        public Task StopSchedulerManger()
        {
            writelog("DeviceMangerPlugin received StopSchedulerManger requested ...");

            _ScheduleManagerPlugin.StopSchedulerManger();
            return Task.FromResult(Task.CompletedTask);
        }
        #endregion

        #region Display Service implementation

        public Task Reset0x52TimerTick(int millisecond)
        {
            writelog("DeviceMangerPlugin received Reset0x52TimerTick: " + millisecond.ToString() + " requested ...");

            _DisplayManagerPlugin.Reset0x52TimerTick(millisecond);

            return Task.FromResult(Task.CompletedTask);
        }

        public Task<List<MonitorInfo>> GetMonitors(bool reScan = false)
        {
            lock (_MoLock)//this) //Dean 0626 fix SAST issue, do not lock over this object
            {
                writelog("DeviceMangerPlugin received GetMonitors requested ...");
                //if (_AllInfoMonitorsRecord.Count == 0)
                //{
                _AllInfoMonitors.Clear();

                if (_DisplayManagerPlugin == null)
                {
                    writelog("null _DisplayManagerPlugin in [GetMonitors], retrun empty monitor list");
                    return Task.FromResult(_AllInfoMonitors);
                }
                List<MonitorInfo> mos = _DisplayManagerPlugin.GetMonitors(reScan).Result;
                _AllInfoMonitors.AddRange(mos);

                //review monitor list to check duplicated data
                ReviewAllMonitorToAvoidDuplicatedInfo();

                List<DDPMMonitorSettings> monitorSettingsList = new List<DDPMMonitorSettings>();
                foreach (MonitorInfo m in _AllInfoMonitors)
                {
                    monitorSettingsList = _SettingsPlugin.InitDDPMMonitorConfigFile(m.modelName).Result;
                    if (monitorSettingsList == null || monitorSettingsList.Count == 0)
                    {
                        DDPMMonitorSettings settings = new DDPMMonitorSettings();
                        settings.Model = m.modelName;
                        settings.ServiceTag = m.edid.ServiceTag;
                        settings.VCPs = GetAllVCPcode(m);
                        monitorSettingsList.Add(settings);
                    }
                    bool b = _SettingsPlugin.WriteMonitorSettings(m.modelName, monitorSettingsList).Result;
                }

                return Task.FromResult(_AllInfoMonitors);
            }
        }

        public Task<string> GetCapabilitiesString(MonitorInfo monitorInfo)
        {
            writelog("DeviceMangerPlugin received GetCapabilitiesString requested ...");
            writelog("TargetMonitor DisplayName is " + monitorInfo.DisplayName);
            writelog("TargetMonitor AliasDeviceName is " + monitorInfo.AliasDeviceName);

            string r = string.Empty;

            r = _DisplayManagerPlugin.GetCapabilitiesString(monitorInfo).Result;

            return Task.FromResult(r);
        }

        public Task<string> GetVCPCapabilities(MonitorInfo monitorInfo)
        {
            writelog("DeviceMangerPlugin received GetVCPCapabilities requested ...");
            writelog("TargetMonitor DisplayName is " + monitorInfo.DisplayName);
            writelog("TargetMonitor AliasDeviceName is " + monitorInfo.AliasDeviceName);

            string r = string.Empty;

            r = _DisplayManagerPlugin.GetVCPCapabilities(monitorInfo).Result;

            return Task.FromResult(r);
        }

        public Task<ObjGetVCP> GetVCPCapability(MonitorInfo monitorInfo, byte code, int opt = 0)
        {
            writelog("DeviceMangerPlugin received GetVCPCapability requested ...");
            writelog("TargetMonitor DisplayName is " + monitorInfo.DisplayName);
            writelog("TargetMonitor AliasDeviceName is " + monitorInfo.AliasDeviceName);
            writelog("VcpCode is " + BitConverter.ToString(new byte[] { code }));
            writelog("opt is " + opt.ToString());

            ObjGetVCP r = new ObjGetVCP();

            r = _DisplayManagerPlugin.GetVCPCapability(monitorInfo, code, opt).Result;

            return Task.FromResult(r);
        }

        public Task<ObjGetVCP> GetVCPCapability(MonitorInfo monitorInfo, string FunctionName, int opt = 0)//Dean 0626 fix SAST issue, syncup param name as well
        {
            writelog("DeviceMangerPlugin received GetVCPCapability requested ...");
            writelog("TargetMonitor DisplayName is " + monitorInfo.DisplayName);
            writelog("TargetMonitor AliasDeviceName is " + monitorInfo.AliasDeviceName);
            writelog("VcpCode is " + FunctionName);
            writelog("opt is " + opt.ToString());

            ObjGetVCP r = new ObjGetVCP();

            r = _DisplayManagerPlugin.GetVCPCapability(monitorInfo, FunctionName, opt).Result;

            return Task.FromResult(r);
        }

        public Task<bool> SetVCPCapability(MonitorInfo monitorInfo, byte code, uint val)
        {
            writelog("DeviceMangerPlugin received SetVCPCapability requested ...");
            writelog("TargetMonitor DisplayName is " + monitorInfo.DisplayName);
            writelog("TargetMonitor AliasDeviceName is " + monitorInfo.AliasDeviceName);
            writelog("VcpCode is " + BitConverter.ToString(new byte[] { code }));
            writelog("val is " + val.ToString());

            bool r = false;

            r = _DisplayManagerPlugin.SetVCPCapability(monitorInfo, code, val).Result;

            //0715 Jason add
            if (r && code == 0x04 && _NKVMPlugin != null)
            {
                _NKVMPlugin.SetVCPNotify(monitorInfo, code, (int)val).Wait();
            }
            else if (!r && code == 0x04)
            {
                writelog("Set VCP code 0x04 Fail...");
            }

            return Task.FromResult(r);
        }

        public Task<bool> SetVCPCapability(MonitorInfo monitorInfoX, string FunctionName, string val)//Dean 0626 fix SAST issue, syncup param name as well
        {
            writelog("DeviceMangerPlugin received SetVCPCapability requested ...");
            writelog("TargetMonitor DisplayName is " + monitorInfoX.DisplayName);
            writelog("TargetMonitor AliasDeviceName is " + monitorInfoX.AliasDeviceName);
            writelog("FunctionName is " + FunctionName);
            writelog("val is " + val);

            bool r = false;

            r = _DisplayManagerPlugin.SetVCPCapability(monitorInfoX, FunctionName, val).Result;

            //0712 Jason add
            if (r && FunctionName == "Input Select")
            {
                if (_NKVMPlugin != null)
                {
                    ObjGetVCP obj = new ObjGetVCP();
                    obj = _DisplayManagerPlugin.GetVCPCapability(monitorInfoX, 0x60).Result;
                    if (obj.result)
                    {
                        _NKVMPlugin.SetVCPNotify(monitorInfoX, 0x60, (int)(uint)obj.value).Wait();
                    }
                }
                _AllInfoMonitors = GetMonitors().Result;
            }

            return Task.FromResult(r);
        }

        public void SetDisplayServiceIdle(bool isIdle)
        {
            isLetDisplayServiceIdle = isIdle;
        }

        public Task<bool> GetDisplayServiceIdleState()
        {
            return Task.FromResult(isLetDisplayServiceIdle);
        }

        #region Input Source

        /// <summary>
        /// get input list
        /// </summary>
        /// <param name="monitorInfo"></param>
        /// <returns>
        /// return inputsource list
        /// </returns>
        public Task<Dictionary<string, InputInfo>> GetInputSourcelist(MonitorInfo monitorInfo)
        {
            //_inputSourcelist = _DisplayManagerPlugin.GetInputSourcelist(monitorInfo).Result;
            //DDPMSettings config = _SettingsPlugin.ReloadAppConfigData().Result;
            Dictionary<string, InputInfo> inputSourcelist = new Dictionary<string, InputInfo>();
            //get monitor settings
            List<DDPMMonitorSettings> settings = _SettingsPlugin.ReloadMonitorSettings(monitorInfo.modelName).Result;
            if (settings != null)
            {
                //get monitor setting
                DDPMMonitorSettings monitorSetting = settings.Find(x => x.ServiceTag == monitorInfo.edid.ServiceTag);
                if (monitorSetting == null)
                {
                    inputSourcelist = _DisplayManagerPlugin.GetInputSourcelist(monitorInfo).Result;
                    bool b = SetInputSourcelist(monitorInfo, inputSourcelist).Result;
                }
                else
                {
                    try
                    {
                        if (monitorSetting.Input != null)
                        {
                            if (monitorSetting.Input.strInputSourceList != null && monitorSetting.Input.strInputSourceList != string.Empty)
                            {
                                inputSourcelist = InputSourceListDeserialize(monitorSetting.Input.strInputSourceList);
                                return Task.FromResult(inputSourcelist);
                            }
                        }
                        inputSourcelist = _DisplayManagerPlugin.GetInputSourcelist(monitorInfo).Result;
                        bool b = SetInputSourcelist(monitorInfo, inputSourcelist).Result;
                    }
                    catch (Exception e)
                    {
                        inputSourcelist = _DisplayManagerPlugin.GetInputSourcelist(monitorInfo).Result;
                        bool b = SetInputSourcelist(monitorInfo, inputSourcelist).Result;
                    }
                }
            }

            return Task.FromResult(inputSourcelist);
        }

        /// <summary>
        /// set input list to settings
        /// </summary>
        /// <param name="monitorInfo"></param>
        /// <param name="inputlist"></param>
        /// <returns></returns>
        public Task<bool> SetInputSourcelist(MonitorInfo monitorInfo, Dictionary<string, InputInfo> inputlist)
        {
            List<DDPMMonitorSettings> settings = _SettingsPlugin.ReloadMonitorSettings(monitorInfo.modelName).Result;
            if (settings != null && inputlist != null)
            {
                foreach (DDPMMonitorSettings monitorSettings in settings)
                {
                    if (monitorSettings != null)
                    {
                        if (monitorSettings.ServiceTag == monitorInfo.edid.ServiceTag)
                        {
                            string strinputlist = string.Empty;
                            strinputlist = InputSourceListSerialize(inputlist);
                            monitorSettings.Input.strInputSourceList = strinputlist;
                            if (_SettingsPlugin.WriteMonitorSettings(monitorInfo.modelName, settings).Result)
                            {
                                return Task.FromResult(true);
                            }
                            break;
                        }
                    }
                }
            }
            return Task.FromResult(false);
        }

        /// <summary>
        /// get input name form settings
        /// </summary>
        /// <param name="input"></param>
        /// <returns>
        /// input name
        /// </returns>
        public Task<string> GetInputName(MonitorInfo monitorInfo, string input)
        {
            Dictionary<string, InputInfo> inputSourceList = GetInputSourcelist(monitorInfo).Result;
            if (inputSourceList != null)
            {
                InputInfo outinput;
                if (inputSourceList.TryGetValue(input, out outinput))
                {
                    return Task.FromResult(inputSourceList[input].InputName);
                }
            }

            return Task.FromResult("");
        }

        /// <summary>
        /// set input name to settings
        /// </summary>
        /// <param name="input"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public Task<bool> SetInputName(MonitorInfo monitorInfo, string input, string name)
        {
            Dictionary<string, InputInfo> inputSourceList = GetInputSourcelist(monitorInfo).Result;
            if (inputSourceList != null)
            {
                InputInfo outinput;
                if (inputSourceList.TryGetValue(input, out outinput))
                {
                    inputSourceList[input].InputName = name;
                    string strInputList = InputSourceListSerialize(inputSourceList);
                    if (SetInputSourcelist(monitorInfo, inputSourceList).Result)
                    {
                        return Task.FromResult(true);
                    }
                }
            }

            return Task.FromResult(false);
        }

        /// <summary>
        /// get USB list
        /// </summary>
        /// <param name="monitorInfo"></param>
        /// <returns>
        /// List<string> USB list
        /// </returns>
        public Task<List<string>> GetUSBUpstreamList(MonitorInfo monitorInfo)
        {
            List<string> list = new List<string>();
            list = _DisplayManagerPlugin.GetUSBUpstreamList(monitorInfo).Result;
            return Task.FromResult(list);
        }

        /// <summary>
        /// set USBupstream
        /// </summary>
        /// <param name="monitorInfo"></param>
        /// <param name="inputsource"></param>
        /// <param name="upstream"></param>
        /// <returns></returns>
        public Task<bool> SetUSBUpstream(MonitorInfo monitorInfo, string inputsource, string upstream)
        {
            if (_DisplayManagerPlugin.SetUSBUpstream(monitorInfo, inputsource, upstream).Result)
            {
                Dictionary<string, InputInfo> inputSourceList = GetInputSourcelist(monitorInfo).Result;
                if (inputSourceList != null)
                {
                    InputInfo outinput;
                    if (inputSourceList.TryGetValue(inputsource, out outinput))
                    {
                        inputSourceList[inputsource].USBUpstream = upstream;
                        string strInputList = InputSourceListSerialize(inputSourceList);
                        if (SetInputSourcelist(monitorInfo, inputSourceList).Result)
                        {
                            return Task.FromResult(true);
                        }
                    }
                }
            }

            return Task.FromResult(false);
        }

        public Task<bool> USBSwitch(MonitorInfo monitorInfo, string inputsource1, string upstream1, string inputsource2, string upstream2)
        {
            Dictionary<string, InputInfo> inputSourceList = GetInputSourcelist(monitorInfo).Result;
            if (inputSourceList != null)
            {
                if (_DisplayManagerPlugin.USBSwitch(monitorInfo, inputsource1, upstream1, inputsource2, upstream2).Result)
                {
                    inputSourceList[inputsource1].USBUpstream = upstream1;
                    inputSourceList[inputsource2].USBUpstream = upstream2;
                    string strInputList = InputSourceListSerialize(inputSourceList);
                    if (SetInputSourcelist(monitorInfo, inputSourceList).Result)
                    {
                        return Task.FromResult(true);
                    }
                }
            }
            return Task.FromResult(false);
        }

    #endregion

    #endregion

    #region Peripherals implementation

    public async Task<DeviceHelper> GetDevices() {
      return await Task.Run(() => _PeripheralsPlugin.GetDevices());
    }

    public async Task<CTKMessageHelper> GetCTKMessageHelper() {
      return await Task.Run(() => _PeripheralsPlugin.GetCTKMessageHelper());
    }

    public async Task<RFDeviceHelper> GetRFDongleDevices()
        {
            return await Task.Run(() => _PeripheralsPlugin.GetRFDongleDevices());
        }

        public Task SetBackLightingControls(int newValue, Guid deviceId)
        {
            writelog("DeviceMangerPlugin received SetBackLightingControls requested ...");
            writelog($"Target DeviceID is {deviceId}");
            writelog($"Target BackLighting Controls is {newValue}");
            _PeripheralsPlugin.SetBackLightingControls(newValue, deviceId);
            return Task.FromResult(true);
        }

        public Task SetBackLightingLevel(int newValue, Guid deviceId)
        {
            writelog("DeviceMangerPlugin received SetBackLightingLevel requested ...");
            writelog($"Target DeviceID is {deviceId}");
            writelog($"Target BackLighting Level is {newValue}");
            _PeripheralsPlugin.SetBackLightingLevel(newValue, deviceId);
            return Task.FromResult(true);
        }

        public Task SetCollaborationBlinkEffectEnable(bool newValue, Guid deviceId)
        {
            writelog("DeviceMangerPlugin received SetCollaborationBlinkEffectEnable requested ...");
            writelog($"Target DeviceID is {deviceId}");
            writelog($"Target Collaboration Blink Effect Enable is {newValue}");
            _PeripheralsPlugin.SetCollaborationBlinkEffectEnable(newValue, deviceId);
            return Task.FromResult(true);
        }

        public Task SetCollaborationCameraEnable(bool newValue, Guid deviceId)
        {
            writelog("DeviceMangerPlugin received SetCollaborationCameraEnable requested ...");
            writelog($"Target DeviceID is {deviceId}");
            writelog($"Target Collaboration Camera Enable is {newValue}");
            _PeripheralsPlugin.SetCollaborationCameraEnable(newValue, deviceId);
            return Task.FromResult(true);
        }

        public Task SetCollaborationChatEnable(bool newValue, Guid deviceId)
        {
            writelog("DeviceMangerPlugin received SetCollaborationChatEnable requested ...");
            writelog($"Target DeviceID is {deviceId}");
            writelog($"Target Collaboration Chat Enable is {newValue}");
            _PeripheralsPlugin.SetCollaborationChatEnable(newValue, deviceId);
            return Task.FromResult(true);
        }

        public Task SetCollaborationDoubleTapEnable(bool newValue, Guid deviceId)
        {
            writelog("DeviceMangerPlugin received SetCollaborationDoubleTapEnable requested ...");
            writelog($"Target DeviceID is {deviceId}");
            writelog($"Target Collaboration Double Tap Enable is {newValue}");
            _PeripheralsPlugin.SetCollaborationDoubleTapEnable(newValue, deviceId);
            return Task.FromResult(true);
        }

        public Task SetCollaborationKeyEnable(bool newValue, Guid deviceId)
        {
            writelog("DeviceMangerPlugin received SetCollaborationKeyEnable requested ...");
            writelog($"Target DeviceID is {deviceId}");
            writelog($"Target Collaboration Key Enable is {newValue}");
            _PeripheralsPlugin.SetCollaborationKeyEnable(newValue, deviceId);
            return Task.FromResult(true);
        }

        public Task SetCollaborationMicEnable(bool newValue, Guid deviceId)
        {
            writelog("DeviceMangerPlugin received SetCollaborationMicEnable requested ...");
            writelog($"Target DeviceID is {deviceId}");
            writelog($"Target Collaboration Mic Enable is {newValue}");
            _PeripheralsPlugin.SetCollaborationMicEnable(newValue, deviceId);
            return Task.FromResult(true);
        }

        public Task SetCollaborationScreenShareEnable(bool newValue, Guid deviceId)
        {
            writelog("DeviceMangerPlugin received SetCollaborationScreenShareEnable requested ...");
            writelog($"Target DeviceID is {deviceId}");
            writelog($"Target Collaboration Screen Share Enable is {newValue}");
            _PeripheralsPlugin.SetCollaborationScreenShareEnable(newValue, deviceId);
            return Task.FromResult(true);
        }

        public Task SetDPILevel(int newValue, Guid deviceId)
        {
            writelog("DeviceMangerPlugin received SetDPILevel requested ...");
            writelog($"Target DeviceID is {deviceId}");
            writelog($"Target DPI Level is {newValue}");
            _PeripheralsPlugin.SetDPILevel(newValue, deviceId);
            return Task.FromResult(true);
        }

        public Task SetDPIValue(int newValue, Guid deviceId)
        {
            writelog("DeviceMangerPlugin received SetDPIValue requested ...");
            writelog($"Target DeviceID is {deviceId}");
            writelog($"Target DPI Value is {newValue}");
            _PeripheralsPlugin.SetDPIValue(newValue, deviceId);
            return Task.FromResult(true);
        }

        public Task SetPrimaryMouseButton(MouseButton newMouseButton, Guid deviceId)
        {
            writelog("DeviceMangerPlugin received SetPrimaryMouseButton requested ...");
            writelog($"Target DeviceID is {deviceId}");
            writelog($"Target Primary Mouse Button is {newMouseButton}");
            _PeripheralsPlugin.SetPrimaryMouseButton(newMouseButton, deviceId);
            return Task.FromResult(true);
        }

        public Task SetTouchScrollSensitivityLevel(int newTouchScrollSensitivityLevel, Guid deviceId)
        {
            writelog("DeviceMangerPlugin received SetTouchScrollSensitivityLevel requested ...");
            writelog($"Target DeviceID is {deviceId}");
            writelog($"Target Touch Scroll Sensitivity Level is {newTouchScrollSensitivityLevel}");
            _PeripheralsPlugin.SetTouchScrollSensitivityLevel(newTouchScrollSensitivityLevel, deviceId);
            return Task.FromResult(true);
        }

        public Task UnPair(Guid deviceId)
        {
            writelog("DeviceMangerPlugin received UnPair requested ...");
            writelog($"Target DeviceID is {deviceId}");
            _PeripheralsPlugin.UnPair(deviceId);
            return Task.FromResult(true);
        }

        public Task StartPairing(Guid deviceId)
        {
            writelog("DeviceMangerPlugin received StartPairing requested ...");
            writelog($"Target DeviceID is {deviceId}");
            _PeripheralsPlugin.StartPairing(deviceId);
            return Task.FromResult(true);
        }

        public Task StopPairing(Guid deviceId)
        {
            writelog("DeviceMangerPlugin received StopPairing requested ...");
            writelog($"Target DeviceID is {deviceId}");
            _PeripheralsPlugin.StopPairing(deviceId);
            return Task.FromResult(true);
        }

        public Task SetWiredAudioIMicNSEnable(bool newValue, Guid deviceId)
        {
            writelog("DeviceMangerPlugin received SetWiredAudioIMicNSEnable requested ...");
            writelog($"Target DeviceID is {deviceId}");
            _PeripheralsPlugin.SetWiredAudioIMicNSEnable(newValue, deviceId);
            return Task.FromResult(true);
        }

        public Task SetWiredAudioMicMuteSoundEnable(bool newValue, Guid deviceId)
        {
            writelog("DeviceMangerPlugin received SetWiredAudioMicMuteSoundEnable requested ...");
            writelog($"Target DeviceID is {deviceId}");
            _PeripheralsPlugin.SetWiredAudioMicMuteSoundEnable(newValue, deviceId);
            return Task.FromResult(true);
        }

        public Task SetWiredAudioVolumeAdjustmentTone(int newValue, Guid deviceId)
        {
            writelog("DeviceMangerPlugin received SetWiredAudioVolumeAdjustmentTone requested ...");
            writelog($"Target DeviceID is {deviceId}");
            _PeripheralsPlugin.SetWiredAudioVolumeAdjustmentTone(newValue, deviceId);
            return Task.FromResult(true);
        }

        public Task SetAncMode(int newValue, Guid deviceId)
        {
            writelog("DeviceMangerPlugin received SetAncMode requested ...");
            writelog($"Target DeviceID is {deviceId}");
            _PeripheralsPlugin.SetAncMode(newValue, deviceId);
            return Task.FromResult(true);
        }

        public Task SetAncGain(int newValue, Guid deviceId)
        {
            writelog("DeviceMangerPlugin received SetAncGain requested ...");
            writelog($"Target DeviceID is {deviceId}");
            _PeripheralsPlugin.SetAncGain(newValue, deviceId);
            return Task.FromResult(true);
        }

        public Task SetSelectedPreset(int newValue, Guid deviceId)
        {
            writelog("DeviceMangerPlugin received SetSelectedPreset requested ...");
            writelog($"Target DeviceID is {deviceId}");
            _PeripheralsPlugin.SetSelectedPreset(newValue, deviceId);
            return Task.FromResult(true);
        }

        public Task SetBandsGain(int newValue, Guid deviceId, string bandGainNumber)
        {
            writelog("DeviceMangerPlugin received SetBandsGain requested ...");
            writelog($"Target DeviceID is {deviceId}");
            _PeripheralsPlugin.SetBandsGain(newValue, deviceId, bandGainNumber);
            return Task.FromResult(true);
        }

        public Task SetMicNoiseCancellation(bool newValue, Guid deviceId)
        {
            writelog("DeviceMangerPlugin received SetMicNoiseCancellation requested ...");
            writelog($"Target DeviceID is {deviceId}");
            _PeripheralsPlugin.SetMicNoiseCancellation(newValue, deviceId);
            return Task.FromResult(true);
        }

        public Task SetSidetone(bool newValue, Guid deviceId)
        {
            writelog("DeviceMangerPlugin received SetSidetone requested ...");
            writelog($"Target DeviceID is {deviceId}");
            _PeripheralsPlugin.SetSidetone(newValue, deviceId);
            return Task.FromResult(true);
        }

        public Task SetSidetoneLevel(int newValue, Guid deviceId)
        {
            writelog("DeviceMangerPlugin received SetSidetoneLevel requested ...");
            writelog($"Target DeviceID is {deviceId}");
            _PeripheralsPlugin.SetSidetoneLevel(newValue, deviceId);
            return Task.FromResult(true);
        }

        public Task SetWearDetection(int newValue, Guid deviceId)
        {
            writelog("DeviceMangerPlugin received SetWearDetection requested ...");
            writelog($"Target SetWearDetection is {deviceId}");
            _PeripheralsPlugin.SetWearDetection(newValue, deviceId);
            return Task.FromResult(true);
        }

        public Task SetBusyLight(bool newValue, Guid deviceId)
        {
            writelog("DeviceMangerPlugin received SetBusyLight requested ...");
            writelog($"Target DeviceID is {deviceId}");
            _PeripheralsPlugin.SetBusyLight(newValue, deviceId);
            return Task.FromResult(true);
        }

        public Task SetVoiceGuidance(bool newValue, Guid deviceId)
        {
            writelog("DeviceMangerPlugin received SetVoiceGuidance requested ...");
            writelog($"Target DeviceID is {deviceId}");
            _PeripheralsPlugin.SetVoiceGuidance(newValue, deviceId);
            return Task.FromResult(true);
        }

        public Task SetMicNCIncoming(bool newValue, Guid deviceId)
        {
            writelog("DeviceMangerPlugin received SetMicNCIncoming requested ...");
            writelog($"Target DeviceID is {deviceId}");
            _PeripheralsPlugin.SetMicNCIncoming(newValue, deviceId);
            return Task.FromResult(true);
        }

        //public Task SetEqualizerValues(ILogicalDeviceHeadset logicalDeviceHeadset, DeviceInfo info)
        //{
        //    writelog("DeviceMangerPlugin received SetEqualizerValues requested ...");
        //    writelog($"Target DeviceID is {info.ID}");
        //    _PeripheralsPlugin.SetEqualizerValues(logicalDeviceHeadset, info);
        //    return Task.FromResult(true);
        //}
        public Task SetIsMicEnumerationOn(bool newValue, Guid deviceId)
        {
            writelog("DeviceMangerPlugin received SetIsMicEnumerationOn requested ...");
            writelog($"Target DeviceID is {deviceId}");
            _PeripheralsPlugin.SetIsMicEnumerationOn(newValue, deviceId);
            return Task.FromResult(true);
        }

        #endregion

        #region CMA/CLI Function area

        /*public void notifyDeviceConnected(CommandInput_notifyDeviceConnection input)
        {
            notifyDeviceAddToListenPool(input, "connected");
        }

        public void notifyDeviceDisConnected(CommandInput_notifyDeviceConnection input)
        {
            notifyDeviceAddToListenPool(input, "disconnected");
        }

        private void notifyDeviceAddToListenPool(CommandInput_notifyDeviceConnection input, string deviceStatus)
        {
            if (input.Params.deviceID != null)
            {
                if (input.Params.deviceID.Length >= Guid.NewGuid().ToString().Length)//peripheral
                {
                    _Peripheral_Listening.Add(new Peripheral_Listen_param()
                    {
                        ID = input.Params.deviceID,
                        //InstanceId = input.Params.
                        deviceStatus = deviceStatus
                    });
                }
                else //for monitor
                {
                    _Monitor_Listening.Add(new Monitor_Listen_param()
                    {
                        serialNumber = input.Params.deviceID,
                        serviceTag = input.Params.serviceTag,
                        deviceStatus = deviceStatus
                    });
                }
            }
        }*/
        /*public Task<CommandOutput_DeviceConnection> queryConnectedDeviceInfo(CommandInput_notifyDeviceConnection input)
        {
            CommandOutput_DeviceConnection output = new CommandOutput_DeviceConnection();
            output.methodName = "queryConnectedDeviceInfo";
            var error = new CommandError()
            {
                code = (int)CommandReturnCode.null_input,
                message = "null object of input"
            };

            if (input == null)
            {
                writelog("queryConnectedDeviceInfo - null command input", log_type.error);
                output.error = error;
                return Task.FromResult(output);
            }
            CommandResult result = new CommandResult()
            {
                deviceID = input.Params?.deviceID
            };
            if (result.deviceID == null || string.IsNullOrEmpty(result.deviceID))
            {
                writelog("queryConnectedDeviceInfo - empty deviceID", log_type.error);
                error.code = (int)CommandReturnCode.empty_deviceID;
                error.message = "null object of input";
                output.error = error;
                return Task.FromResult(output);
            }
            List<DeviceInfo> peripheralslist = new List<DeviceInfo>();
            if (_PeripheralsPlugin != null)
            {
                var dev = _PeripheralsPlugin.GetDevices().Result;
                dev.deviceInfo.ForEach(di =>
                {
                    if (di.IsConnected)
                        peripheralslist.Add(di);
                });
                if (peripheralslist.Count == 0)
                    writelog("empty device count of peripheral device");
                else
                {
                    //update the comparison result
                    peripheralslist = peripheralslist.FindAll(x => x.ID.ToString().ToLower().Trim().Equals(result.deviceID.ToLower().Trim()));
                    writelog(peripheralslist.Count > 0 ? peripheralslist.ToString() : $"no connected peripheral device matched with {result.deviceID}");
                    if (peripheralslist.Count > 0)
                    {
                        List<Peripheral_Listen_param> temp = new List<Peripheral_Listen_param>();
                        foreach (DeviceInfo di in peripheralslist)
                        {
                            temp.Add(new Peripheral_Listen_param()
                            {
                                ID = di.ID.ToString(),
                                //InstanceId = di.InstanceId.ToString(),
                                deviceStatus = ""
                            });
                        }
                        result.peripherals.AddRange(temp);// peripheralslist);
                        output.result = result;
                        return Task.FromResult(output);
                    }
                }
            }
            else
                writelog("null peripheralsplugin");

            List<MonitorInfo> monitorlist = null;// new List<MonitorInfo>();
            error.code = (int)CommandReturnCode.no_match_deviceID;
            error.message = "no matched device";
            if (_DisplayManagerPlugin != null)
            {
                var dev = _DisplayManagerPlugin.GetMonitors().Result;
                if (dev == null)
                {
                    writelog("empty device count of monitor");
                    output.error = error;
                    return Task.FromResult(output);
                }
                monitorlist = dev;
                monitorlist = monitorlist.FindAll(x => x.edid.ServiceTag.ToLower().Trim().Equals(result.deviceID.ToLower().Trim()));
                writelog(monitorlist.Count > 0 ? monitorlist.ToString() : $"no connected monitor matched with {result.deviceID}");
                if (monitorlist.Count > 0)
                {
                    List<Monitor_Listen_param> temp = new List<Monitor_Listen_param>();
                    foreach (MonitorInfo mi in monitorlist)
                    {
                        temp.Add(new Monitor_Listen_param()
                        {
                            serialNumber = mi.edid.SerialNumber,
                            serviceTag = mi.edid.ServiceTag,
                            deviceStatus = ""
                        });
                    }
                    result.monitors.AddRange(temp);// monitorlist);
                    output.result = result;
                    return Task.FromResult(output);
                }
            }
            else
                writelog("null displaymanagerplugin");

            //no matched device and set it as error
            output.error = error;
            return Task.FromResult(output);
        }*/
        /*public Task<List<CommandResult>> listConnectedDeviceInfo()
        {
            List<CommandResult> result = new List<CommandResult>();
            List<DeviceInfo> peripheralslist = new List<DeviceInfo>();
            if (_PeripheralsPlugin != null)
            {
                var dev = _PeripheralsPlugin.GetDevices().Result;
                dev.deviceInfo.ForEach(di =>
                {
                    if (di.IsConnected)
                    {
                        peripheralslist.Add(di);
                        CommandResult temp = new CommandResult();
                        temp.deviceID = di.ID.ToString();
                        temp.peripherals.Add(new Peripheral_Listen_param()
                        {
                            ID = di.ID.ToString(),
                            //InstanceId = di.InstanceId.ToString(),
                            deviceStatus = "connected"
                        });
                        result.Add(temp);
                    }
                });
                if (peripheralslist.Count == 0)
                    writelog("empty device count of peripheral device");
            }
            else
                writelog("null peripheralsplugin");

            List<MonitorInfo> monitorlist = new List<MonitorInfo>();

            if (_DisplayManagerPlugin != null)
            {
                monitorlist.AddRange(_DisplayManagerPlugin.GetMonitors().Result);

                if (monitorlist.Count == 0)
                    writelog("empty device count of display device");
                else
                {
                    foreach (var mo in monitorlist)
                    {
                        CommandResult temp = new CommandResult();
                        temp.deviceID = mo.edid.ServiceTag;//mo.edid.SerialNumber;
                        temp.monitors.Add(new Monitor_Listen_param()
                        {
                            serialNumber = mo.edid.SerialNumber,
                            serviceTag = mo.edid.ServiceTag,
                            deviceStatus = "connected"
                        });
                        result.Add(temp);
                    }
                }
            }
            else
                writelog("null displaymanagerplugin");

            return Task.FromResult(result);
        }*/

        public event EventHandler<UpdateUINotify> UIUpdateNotify;

        //Target to notify UI update
        public void OnUIUpdateNotify(UpdateUINotify e)
        {
            if (UIUpdateNotify == null || e == null || e == EventArgs.Empty)
                return;

            EventHandler<UpdateUINotify> Handler = UIUpdateNotify;
            if (Handler != null)
            {
                Handler.Invoke(this, e);
                writelog($"[UIUpdateNotify] be Invoked");
            }
        }
        #endregion

        #region Bruce display properties implementation

        public Task<DisplayPropertiesInfo> GetDisplayPropertiesInfo(MonitorInfo monitorInfos)
        {
            return Task.FromResult(_DisplayManagerPlugin.GetDisplayPropertiesInfo(monitorInfos).Result);
        }

        public Task<bool> SetDisplayPropertiest(MonitorInfo monitorInfos, DDPM.SA.Common.Properties properties, DisplayOrientation orientation)
        {
            displayInOut = false;
            bool result = _DisplayManagerPlugin.SetDisplayPropertiest(monitorInfos, properties, orientation).Result;
            displayInOut = true;
            return Task.FromResult(result);
        }

        public Task<bool> CallWindowsDisplaySetting()
        {
            return Task.FromResult(_DisplayManagerPlugin.CallWindowsDisplaySetting().Result);
        }

        public Task<bool> GetHDRStatus(MonitorInfo monitorInfos)
        {
            return Task.FromResult(_DisplayManagerPlugin.GetHDRStatus(monitorInfos).Result);
        }

        public Task<bool> SetHDRStatus(MonitorInfo monitorInfos, bool onoff)
        {
            return Task.FromResult(_DisplayManagerPlugin.SetHDRStatus(monitorInfos, onoff).Result);
        }

        public Task<bool> SetUSBCPrioritizationType(MonitorInfo monitorInfos, USBCPrioritizationType type)
        {
            return Task.FromResult(_DisplayManagerPlugin.SetUSBCPrioritizationType(monitorInfos, type).Result);
        }

        //0606 Bruce 新增鎖定自動旋轉方向
        public Task<bool> LockRotate(bool onoff)
        {
            DDPMSettings config = _SettingsPlugin.ReloadAppConfigData().Result;
            if (config != null)
            {
                config.UserSettings.LockRotate = onoff;
                _DisplayManagerPlugin.SetEnableLockOrientation(onoff);
                return Task.FromResult(_SettingsPlugin.SetAppConfigData(config).Result);
            }
            return Task.FromResult(false);
        }

        //0606 Bruce 新增鎖定自動旋轉方向
        public Task<bool> GetLockRotateStatus()
        {
            try
            {
                if (_SettingsPlugin != null && _DisplayManagerPlugin != null)
                {
                    DDPMSettings config = _SettingsPlugin.ReloadAppConfigData().Result;
                    _DisplayManagerPlugin.SetEnableLockOrientation(config.UserSettings.LockRotate);
                    return Task.FromResult(config.UserSettings.LockRotate);
                }
                else
                {
                    return Task.FromResult(false);
                }
            }
            catch
            {
                return Task.FromResult(false);
            }
        }

        public Task<string> GetOSDOrientation(MonitorInfo monitorInfo)
        {
            return Task.FromResult(_DisplayManagerPlugin.GetOSDOrientation(monitorInfo).Result);
        }

        public Task<bool?> SetOSDOrientation(MonitorInfo monitorInfo, string orientation)
        {
            return Task.FromResult(_DisplayManagerPlugin.SetOSDOrientation(monitorInfo, orientation).Result);
        }

        #endregion

        #region ISettingsManager implementation

        public Task<DDPMSettings> ReloadAppConfigData(bool force_reload = false)
        {
            DDPMSettings settings = _SettingsPlugin.ReloadAppConfigData().Result;
            return Task.FromResult(settings);
        }

        public Task<bool> SetAppConfigData(DDPMSettings data)
        {
            return Task.FromResult(_SettingsPlugin.SetAppConfigData(data).Result);
        }

        #endregion

        #region PIP/PBP Manager

        public Task<UInt16[]> GetPipPbpCapabilitiesWords(MonitorInfo monitorInfo)
        {
            return _DisplayManagerPlugin.GetPipPbpCapabilitiesWords(monitorInfo);
        }

        public Task<bool> SetPipModeOff(MonitorInfo monitorInfo)
        {
            bool b = _DisplayManagerPlugin.SetPipModeOff(monitorInfo).Result;
            if (b && _NKVMPlugin != null)
            {
                _NKVMPlugin.SetVCPNotify(monitorInfo, 0xE9, 0).Wait();
            }
            return Task.FromResult(b);
        }

        public Task<bool> SetPipModeSmall(MonitorInfo monitorInfo)
        {
            bool b = _DisplayManagerPlugin.SetPipModeSmall(monitorInfo).Result;
            if (b && _NKVMPlugin != null)
            {
                _NKVMPlugin.SetVCPNotify(monitorInfo, 0xE9, 0x21).Wait();
            }
            return Task.FromResult(b);
        }

        public Task<bool> SetPipModeLarge(MonitorInfo monitorInfo)
        {
            bool b = _DisplayManagerPlugin.SetPipModeLarge(monitorInfo).Result;
            if (b && _NKVMPlugin != null)
            {
                _NKVMPlugin.SetVCPNotify(monitorInfo, 0xE9, 0x22).Wait();
            }
            return Task.FromResult(b);
        }

        public Task<bool> TogglePipSize(MonitorInfo monitorInfo)
        {
            bool b = _DisplayManagerPlugin.TogglePipSize(monitorInfo).Result;
            if (b && _NKVMPlugin != null)
            {
                _NKVMPlugin.SetVCPNotify(monitorInfo, 0xE9, 0x01).Wait();
            }
            return Task.FromResult(b);
        }

        public Task<bool> TogglePipPosition(MonitorInfo monitorInfo)
        {
            bool b = _DisplayManagerPlugin.TogglePipPosition(monitorInfo).Result;
            if (b && _NKVMPlugin != null)
            {
                _NKVMPlugin.SetVCPNotify(monitorInfo, 0xE9, 0x02).Wait();
            }
            return Task.FromResult(b);
        }

        public Task<bool> SetPbpMode(MonitorInfo monitorInfo, UInt16 modeCode)
        {
            bool b = _DisplayManagerPlugin.SetPbpMode(monitorInfo, modeCode).Result;
            if (b && _NKVMPlugin != null)
            {
                _NKVMPlugin.SetVCPNotify(monitorInfo, 0xE9, (int)modeCode).Wait();
            }
            return Task.FromResult(b);
        }

        public Task<bool> VideoSwap(MonitorInfo monitorInfo, UInt16 x, UInt16 y)
        {
            bool b = _DisplayManagerPlugin.VideoSwap(monitorInfo, x, y).Result;
            if (b && _NKVMPlugin != null)
            {
                ObjGetVCP obj = new ObjGetVCP();
                obj = _DisplayManagerPlugin.GetVCPCapability(monitorInfo, 0xE5).Result;
                if (obj.result)
                {
                    _NKVMPlugin.SetVCPNotify(monitorInfo, 0xE5, (int)(uint)obj.value).Wait();
                }
            }
            return Task.FromResult(b);
        }

        public Task<ObjGetVCP> GetPxpMode(MonitorInfo monitorInfo)
        {
            return _DisplayManagerPlugin.GetPxpMode(monitorInfo);
        }

        public Task<List<UInt16>> GetSubInputList(MonitorInfo monitorInfo)
        {
            return _DisplayManagerPlugin.GetSubInputList(monitorInfo);
        }

        public Task<List<InputSourceObj>> GetSubInputs(MonitorInfo monitorInfo)
        {
            return _DisplayManagerPlugin.GetSubInputs(monitorInfo);
        }

        public Task<bool> SetSubInputs(MonitorInfo monitorInfo, InputSourceObj? sub1, InputSourceObj? sub2, InputSourceObj? sub3)
        {
            bool b = _DisplayManagerPlugin.SetSubInputs(monitorInfo, sub1, sub2, sub3).Result;
            if (b && _NKVMPlugin != null)
            {
                ObjGetVCP obj = new ObjGetVCP();
                obj = _DisplayManagerPlugin.GetVCPCapability(monitorInfo, 0xE8).Result;
                if (obj.result)
                {
                    _NKVMPlugin.SetVCPNotify(monitorInfo, 0xE8, (int)(uint)obj.value).Wait();
                }
            }
            return Task.FromResult(b);
        }

        public Task<bool> UsbSwitch1(MonitorInfo monitorInfo, UInt16 target = 0)
        {
            return _DisplayManagerPlugin.UsbSwitch(monitorInfo, target);
        }

        #endregion

        #region Bruce FW Update implementation

        public Task<FWUpdateInfoPackage> GetFWUpdateInfo(bool isShowNotify = true, bool isForce = false, bool isDefer = false, List<DeviceType> deviceTypeList = null, bool UODMode = false)
        {
            if (_PeripheralsPlugin != null && _FWUpdatePlugin != null)
            {
                UpdateHelper updateHelper = _PeripheralsPlugin.GetFWUpdateInfo().Result;
                //0612 Bruce 將傳入值null移除因已不需使用，不會影響UI和CLI
                return Task.FromResult(_FWUpdatePlugin.GetFWUpdateInfo(updateHelper, isShowNotify, isForce, isDefer, deviceTypeList, UODMode).Result);
            }
            return Task.FromResult(new FWUpdateInfoPackage());
        }

        public Task<List<FWUpdateInfo>> DownloadAndInstall(List<FWUpdateInfo> fwUpdateInfos, string installPath = "")
        {
            _UpdateProgress = null;
            CallUI().Wait();
            List<FWUpdateInfo> tmpFWUpdateInfos = _FWUpdatePlugin.DownloadAndInstall(fwUpdateInfos, installPath).Result;
            if (_UpdateProgress != null)
            {
                _FWUpdatePlugin.ProgressUpdate_Notify -= _UpdateProgress._FWUpdatePlugin_ProgressUpdate;
                _UpdateProgress.CloseWindow();
                _UpdateProgress = null;
            }
            return Task.FromResult(tmpFWUpdateInfos);
        }

        public void SetUILockStatus(bool isLockFWU_UI)
        {
            if (_SettingsPlugin != null)
            {
                DDPMSettings config = _SettingsPlugin.ReloadAppConfigData().Result;
                if (config != null)
                {
                    config.UserSettings.LockFWU_UI = isLockFWU_UI;
                    _SettingsPlugin.SetAppConfigData(config).Wait();
                    OnUILockEvent(isLockFWU_UI);
                }
            }
        }

        public Task<bool> GetUILockStatus()
        {
            if (_SettingsPlugin == null)
            {
                return Task.FromResult(false);
            }
            DDPMSettings config = _SettingsPlugin.ReloadAppConfigData().Result;
            if (config == null)
            {
                return Task.FromResult(false);
            }
            return Task.FromResult(config.UserSettings.LockFWU_UI);
        }

        private Task<bool> SetFWUpdateInfoPackage(FWUpdateInfoPackage fwUpdateInfoPackage)
        {
            if (_SettingsPlugin != null)
            {
                DDPMSettings config = _SettingsPlugin.ReloadAppConfigData().Result;
                if (config != null)
                {
                    config.UserSettings.DelayFWUpdateInfoPackage = fwUpdateInfoPackage;
                    return Task.FromResult(_SettingsPlugin.SetAppConfigData(config).Result);
                }
            }
            return Task.FromResult(false);
        }

        private Task<bool> CheckUpdate()
        {
            if (_PeripheralsPlugin == null)
                return Task.FromResult(false);
            UpdateHelper updateHelper = _PeripheralsPlugin.GetFWUpdateInfo().Result;
            if (_FWUpdatePlugin == null)
                return Task.FromResult(false);
            SetDelayFWUpdateInfoPackage();
            List<FWUpdateInfo> fwUpdateInfos = _FWUpdatePlugin.CheckUpdate(updateHelper, true, null, false).Result;
            bool b = true;
            foreach (FWUpdateInfo fwUpdateInfo in fwUpdateInfos)
            {
                if (fwUpdateInfo.FWUErrorCode != FWUErrorCode.NoError)
                {
                    b = false;
                }
            }
            return Task.FromResult(b);
        }

        private Task<bool> GetDeviceinfos()
        {
            if (_PeripheralsPlugin != null && _FWUpdatePlugin != null)
            {
                _FWUpdatePlugin.SetDeviceinfo(_PeripheralsPlugin.GetDevices().Result.deviceInfo);
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }

        private Task<bool> SetUODFWUInfoPackage(DokcUODUpdateInfoPackage UODFWUInfo)
        {
            if (_SettingsPlugin != null)
            {
                bool b = false;
                DDPMSettings config = _SettingsPlugin.ReloadAppConfigData().Result;
                if (config != null)
                {
                    config.UserSettings.UODFWUInfoPackage = UODFWUInfo;
                    b = _SettingsPlugin.SetAppConfigData(config).Result;
                }
                return Task.FromResult(b);
            }
            return Task.FromResult(false);
        }

        private void CheckUODFWUInfoPackage(bool isDevuceTrigger = false)
        {
            if (_SettingsPlugin != null)
            {
                DDPMSettings config = _SettingsPlugin.ReloadAppConfigData().Result;
                if (config != null && config.UserSettings.UODFWUInfoPackage != null && _FWUpdatePlugin != null && _PeripheralsPlugin != null)
                {
                    if (isDevuceTrigger)
                    {
                        _FWUpdatePlugin.CheckUODFWUInfo(config.UserSettings.UODFWUInfoPackage, _PeripheralsPlugin.GetDevices().Result.deviceInfo);
                    }
                    else
                    {
                        _FWUpdatePlugin.CheckUODFWUInfo(config.UserSettings.UODFWUInfoPackage, null);
                    }
                }
            }
        }

        private void SetDelayFWUpdateInfoPackage()
        {
            if (_SettingsPlugin != null && _FWUpdatePlugin != null)
            {
                DDPMSettings config = _SettingsPlugin.ReloadAppConfigData().Result;
                if (config != null)
                {
                    _FWUpdatePlugin.SetDelayFWUpdateInfoPackage(config.UserSettings.DelayFWUpdateInfoPackage);
                }
            }
        }

        private Task CallUI()
        {
            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
            Thread thread1 = new Thread(() =>
            {
                _UpdateProgress = new UpdateProgress();
                _UpdateProgress.Width = 800;
                _UpdateProgress.Height = 440;
                _UpdateProgress.Topmost = true;
                _UpdateProgress.Closed += (sender2, e2) =>
                {
                    _UpdateProgress.Dispatcher.InvokeShutdown();
                };
                _UpdateProgress.Show();
                _FWUpdatePlugin.ProgressUpdate_Notify += _UpdateProgress._FWUpdatePlugin_ProgressUpdate;
                tcs.SetResult(true);
                Dispatcher.Run();
            });
            thread1.SetApartmentState(ApartmentState.STA);
            thread1.Start();
            return tcs.Task;
        }

        private void CallPopup(object o, PopupContentPackage popupContentPackage)
        {
            // 將 popupContentPackage.Object 轉換成 JSON 字串
            string json = JsonConvert.SerializeObject(popupContentPackage.Object);
            // 將 JSON 字串轉換成 FWUpdateInfoPackage 對象
            FWUpdateInfoPackage fWUpdateInfoPackage = JsonConvert.DeserializeObject<FWUpdateInfoPackage>(json);
            // 將 JSON 字串轉換成 SWUpdateInfoPackage 對象
            SWUpdateInfoPackage sWUpdateInfoPackage = JsonConvert.DeserializeObject<SWUpdateInfoPackage>(json);
            string title = popupContentPackage.Title;
            string info = popupContentPackage.Info;
            bool isInfo = popupContentPackage.IsInfo;
            bool isOnlyUpdate = popupContentPackage.IsOnlyUpdate;
            bool stayOpen = popupContentPackage.StayOpen;
            int timeout = popupContentPackage.Timeout;
            object ob;
            if (sWUpdateInfoPackage.SWUpdateInfo.Count > 0)
            {
                ob = sWUpdateInfoPackage;
            }
            else
            {
                ob = fWUpdateInfoPackage;
            }
            if (!string.IsNullOrEmpty(info))
            {
                Task.Run(() =>
                {
                    PopupBaseManage popupBaseManage = new PopupBaseManage();
                    popupBaseManage.LeftButtonClick += UpdateEvent;
                    popupBaseManage.RightButtonClick += DelayEvent;
                    if (isInfo)
                    {
                        popupBaseManage.FWU_Show(title, info, "", "", ob, stayOpen, timeout);
                    }
                    else if (isOnlyUpdate)
                    {
                        popupBaseManage.Default_Event += UpdateEvent;
                        popupBaseManage.FWU_Show(title, info, "Update", "", ob, stayOpen, timeout);
                    }
                    else
                    {
                        popupBaseManage.Default_Event += DelayEvent;
                        popupBaseManage.FWU_Show(title, info, "Update", "Delay", ob, stayOpen, timeout);
                    }
                });
            }
        }

        private void UpdateEvent(object o, object ob)
        {
            // 將 e 轉換成 JSON 字串
            string json = JsonConvert.SerializeObject(ob);
            // 將 JSON 字串轉換成 FWUpdateInfoPackage 對象
            FWUpdateInfoPackage fWUpdateInfoPackage = JsonConvert.DeserializeObject<FWUpdateInfoPackage>(json);
            // 將 JSON 字串轉換成 SWUpdateInfoPackage 對象
            SWUpdateInfoPackage sWUpdateInfoPackage = JsonConvert.DeserializeObject<SWUpdateInfoPackage>(json);
            if (sWUpdateInfoPackage.SWUpdateInfo.Count > 0)
            {
                if (_SWUpdatePlugin != null)
                {
                    _SWUpdatePlugin.UpdateEvent(ob);
                }
            }
            else
            {
                if (_FWUpdatePlugin != null)
                {
                    _FWUpdatePlugin.UpdateEvent(ob);
                }
            }
        }

        private void DelayEvent(object o, object ob)
        {
            // 將 e 轉換成 JSON 字串
            string json = JsonConvert.SerializeObject(ob);
            // 將 JSON 字串轉換成 FWUpdateInfoPackage 對象
            FWUpdateInfoPackage fWUpdateInfoPackage = JsonConvert.DeserializeObject<FWUpdateInfoPackage>(json);
            // 將 JSON 字串轉換成 SWUpdateInfoPackage 對象
            SWUpdateInfoPackage sWUpdateInfoPackage = JsonConvert.DeserializeObject<SWUpdateInfoPackage>(json);
            if (sWUpdateInfoPackage.SWUpdateInfo.Count > 0)
            {
                if (_SWUpdatePlugin != null)
                {
                    _SWUpdatePlugin.DelayEvent(ob);
                }
            }
            else
            {
                if (_FWUpdatePlugin != null)
                {
                    _FWUpdatePlugin.DelayEvent(ob);
                }
            }
        }

        #endregion

        #region USBKVM implementation

        /// <summary>
        /// get USBKVM InputSource list
        /// </summary>
        /// <param name="monitorInfo"></param>
        /// <param name="inputList"></param>
        /// <param name="subInputList"></param>
        /// <returns>
        /// Dictionary<string, PCsInfo> USBKVM InputSource list
        /// </returns>
        public Task<Dictionary<string, PCsInfo>> GetUSBKVMPCsList(MonitorInfo monitorInfo, Dictionary<string, InputInfo> inputList, List<InputSourceObj> subInputList)
        {
            //_USBKVMPCsList = _DisplayManagerPlugin.GetUSBKVMPCsList(monitorInfo, inputList, subInputList).Result;
            Dictionary<string, PCsInfo> USBKVMPCsList = new Dictionary<string, PCsInfo>();
            //get monitor settings
            List<DDPMMonitorSettings> settings = _SettingsPlugin.ReloadMonitorSettings(monitorInfo.modelName).Result;
            if (settings != null)
            {
                //get monitor setting
                DDPMMonitorSettings monitorSetting = settings.Find(x => x.ServiceTag == monitorInfo.edid.ServiceTag);
                if (monitorSetting == null)
                {
                    USBKVMPCsList = _DisplayManagerPlugin.GetUSBKVMPCsList(monitorInfo, inputList, subInputList).Result;
                    bool b = SetUSBKVMPCsList(monitorInfo, USBKVMPCsList).Result;
                }
                else
                {
                    try
                    {
                        if (monitorSetting.KVM.strUSBKVMPCsList != null && monitorSetting.KVM.strUSBKVMPCsList != string.Empty)
                        {
                            USBKVMPCsList = USBKVMPCsListDeserialize(monitorSetting.KVM.strUSBKVMPCsList);
                        }
                        else
                        {
                            USBKVMPCsList = _DisplayManagerPlugin.GetUSBKVMPCsList(monitorInfo, inputList, subInputList).Result;
                            bool b = SetUSBKVMPCsList(monitorInfo, USBKVMPCsList).Result;
                        }
                    }
                    catch (Exception e)
                    {
                        USBKVMPCsList = _DisplayManagerPlugin.GetUSBKVMPCsList(monitorInfo, inputList, subInputList).Result;
                        bool b = SetUSBKVMPCsList(monitorInfo, USBKVMPCsList).Result;
                    }
                }
            }

            return Task.FromResult(USBKVMPCsList);
        }

        /// <summary>
        /// set USBKVM InputSource list
        /// </summary>
        /// <param name="monitorInfo"></param>
        /// <param name="pcsInfo"></param>
        /// <returns></returns>
        public Task<bool> SetUSBKVMPCsList(MonitorInfo monitorInfo, Dictionary<string, PCsInfo> pcsList)//Dean 0626 fix SAST issue, rename as pcsList
        {
            List<DDPMMonitorSettings> settings = _SettingsPlugin.ReloadMonitorSettings(monitorInfo.modelName).Result;
            if (settings != null && pcsList != null)
            {
                string strUSBKVMPCsList = string.Empty;
                //Dean 0626 fix SAST issue, before check count the object should not be null
                if (pcsList.Count > 0)//|| pcsInfo.Count > 0)
                {
                    foreach (DDPMMonitorSettings monitorSettings in settings)
                    {
                        if (monitorSettings != null)
                        {
                            if (monitorSettings.ServiceTag == monitorInfo.edid.ServiceTag)
                            {
                                strUSBKVMPCsList = USBKVMPCsListSerialize(pcsList);
                                monitorSettings.KVM.strUSBKVMPCsList = strUSBKVMPCsList;
                                if (_SettingsPlugin.WriteMonitorSettings(monitorInfo.modelName, settings).Result)
                                {
                                    return Task.FromResult(true);
                                }
                                break;
                            }
                        }
                    }
                }
            }
            return Task.FromResult(false);
        }

        /// <summary>
        /// USBKVM InputSource Swap
        /// </summary>
        /// <param name="pcsList"></param>
        /// <param name="swapPC1"></param>
        /// <param name="swapPC2"></param>
        /// <returns>
        /// Dictionary<string, PCsInfo> after Swap, USBKVM InputSource list
        /// </returns>
        public Task<Dictionary<string, PCsInfo>> PCInfoSwap(Dictionary<string, PCsInfo> pcsList, string swapPC1, string swapPC2)
        {
            if (pcsList.Count <= 0)
                return Task.FromResult(pcsList);

            PCsInfo pcSwap1 = new PCsInfo();
            PCsInfo pcSwap2 = new PCsInfo();
            PCsInfo outpcs1;
            PCsInfo outpcs2;

            if (pcsList.TryGetValue(swapPC1, out outpcs1) && pcsList.TryGetValue(swapPC2, out outpcs2))
            {
                pcSwap1 = pcsList[swapPC1];
                pcSwap2 = pcsList[swapPC2];

                pcsList[swapPC1] = pcSwap2;
                pcsList[swapPC2] = pcSwap1;

                return Task.FromResult(pcsList);
            }
            return Task.FromResult(pcsList);
        }

        public Task<bool> GetOnUSBKVM(MonitorInfo monitorInfo)
        {
            List<DDPMMonitorSettings> settings = _SettingsPlugin.ReloadMonitorSettings(monitorInfo.modelName).Result;
            if (settings != null)
            {
                DDPMMonitorSettings monitorSetting = settings.Find(x => x.ServiceTag == monitorInfo.edid.ServiceTag);
                if (monitorSetting == null)
                {
                    return Task.FromResult(monitorSetting.KVM.isOnUSBKVM);
                }
            }
            return Task.FromResult(false);
        }

        public Task<bool> SetOnUSBKVM(MonitorInfo monitorInfo, bool isON)
        {
            List<DDPMMonitorSettings> settings = _SettingsPlugin.ReloadMonitorSettings(monitorInfo.modelName).Result;
            if (settings != null) //Robert_Lin 0731
            {
                foreach (DDPMMonitorSettings setting in settings)
                {
                    if (setting != null)
                    {
                        if (setting.ServiceTag == monitorInfo.edid.ServiceTag)
                        {
                            setting.KVM.isOnUSBKVM = isON;
                            if (_SettingsPlugin.WriteMonitorSettings(monitorInfo.modelName, settings).Result)
                            {
                                return Task.FromResult(true);
                            }
                            break;
                        }
                    }
                }
            }
            return Task.FromResult(false);
        }

        #endregion

        #region ALS feature functions

        public Task<ALSConfig> GetALSFeatureValue(MonitorInfo monitorInfos, ALSFeatureQueryType type, int value)//Dean 0626 fix SAST issue, rename as value
        {
            return Task.FromResult(_DisplayManagerPlugin.GetALSFeatureValue(monitorInfos, type, value).Result);
        }

        public Task<bool> SetALSFeatureValue(MonitorInfo monitorInfos, ALSConfig param, ALSFeatureQueryType type, string value)
        {
            return Task.FromResult(_DisplayManagerPlugin.SetALSFeatureValue(monitorInfos, ref param, type, value).Result);
        }

        public Task<List<ALSConfig>> GetConnectedALSConfig()
        {
            return Task.FromResult(_DisplayManagerPlugin.GetConnectedALSConfig()).Result;
        }

        public Task<List<ALSConfig>> GetAllExistAlsConfig()
        {
            return Task.FromResult(_DisplayManagerPlugin.GetAllExistAlsConfig().Result);
        }

        public Task<List<ALSConfig>> UpdateExistAlsConfig(List<MonitorInfo> monitorInfoMain)
        {
            return Task.FromResult(_DisplayManagerPlugin.UpdateExistAlsConfig(monitorInfoMain).Result);
        }

        public Task<bool> SynchronizeALSFeatureValue(ALSConfig monitorALS)
        {
            return Task.FromResult(_DisplayManagerPlugin.SynchronizeALSFeatureValue(monitorALS).Result);
        }

        #endregion

        #region NKVM implementation

        public Task SupportedNKVMMonitors()
        {
            if (_NKVMPlugin != null && _SettingsPlugin != null)
            {
                DDPMSettings config = _SettingsPlugin.ReloadAppConfigData().Result;
                _SupportedMonitorList = _NKVMPlugin.UpdateSupportMonitors().Result;
                if (config != null)
                {
                    config.UserSettings.SupportedMonitorList = _SupportedMonitorList;
                    _SettingsPlugin.SetAppConfigData(config);
                }
            }
            return Task.CompletedTask;
        }

        public Task<bool> GetOnNKVM(MonitorInfo monitorInfo)
        {
            List<DDPMMonitorSettings> settings = _SettingsPlugin.ReloadMonitorSettings(monitorInfo.modelName).Result;
            if (settings != null)
            {
                DDPMMonitorSettings monitorSetting = settings.Find(x => x.ServiceTag == monitorInfo.edid.ServiceTag);
                if (monitorSetting == null)
                {
                    return Task.FromResult(monitorSetting.KVM.isOnNKVM);
                }
            }
            return Task.FromResult(false);
        }

        public Task SetOnNKVM(MonitorInfo monitorInfo, bool ison)
        {
            List<DDPMMonitorSettings> settings = _SettingsPlugin.ReloadMonitorSettings(monitorInfo.modelName).Result;
            if (settings != null && _NKVMPlugin != null) //Robert_Lin 0731
            {
                foreach (DDPMMonitorSettings setting in settings)
                {
                    if (setting != null)
                    {
                        if (setting.ServiceTag == monitorInfo.edid.ServiceTag)
                        {
                            setting.KVM.isOnNKVM = ison;
                            bool b = _SettingsPlugin.WriteMonitorSettings(monitorInfo.modelName, settings).Result;
                            if (ison)
                            {
                                _SupportedMonitorList = _NKVMPlugin.GetSupportedNKVM().Result;
                                _NKVMPlugin.OnNKVM().Wait();
                            }
                            else
                            {
                                _NKVMPlugin.OffNKVM().Wait();
                            }
                            break;
                        }
                    }
                }
            }

            return Task.CompletedTask;
        }

        public Task<bool> isNKVMSupportMonitor(MonitorInfo monitorInfo)
        {
            if (_NKVMPlugin != null)
            {
                return _NKVMPlugin?.isSupportMonitor(monitorInfo);
            }
            return Task.FromResult(false);
        }

        #endregion

        #region EasyArrage

        /// <summary>
        /// Enable/Disable EasyArrange function for all monitors.
        /// When Disabled (isEnable=false), DDPM will not show the WorkWindow (to arrange window),
        /// but user can edit/setup in DDPM.UI and save their settings.
        /// </summary>
        /// <param name="isEnabled"></param>
        /// <returns></returns>
        public Task<bool> SetEAFunctionEnabled(bool isEnabled)
        {
            if (_DisplayManagerPlugin != null)
            {
                return _DisplayManagerPlugin.SetEAFunctionEnabled(isEnabled);
            }
            return Task.FromResult(false);
        }

        public Task<ObjGetVCP> GetEAFunctionEnabled()
        {
            if (_DisplayManagerPlugin != null)
            {
                return _DisplayManagerPlugin.GetEAFunctionEnabled();
            }
            return Task.FromResult<ObjGetVCP>(new ObjGetVCP() { result = false, value = false });
        }

        public Task<bool> SetEAWrokSplit(MonitorInfo monitorInfo, int cellCount, char splitKey, List<double>? settings)
        {
            if (_DisplayManagerPlugin != null)
            {
                _DisplayManagerPlugin.SetEAWrokSplit(monitorInfo, cellCount, splitKey, settings);
            }
            return Task.FromResult(false);
        }

        public Task<bool> RequestEditSplit(MonitorInfo monitorInfo, int cellCount, char splitKey, string customName, List<double>? settings = null)
        {
            if (_DisplayManagerPlugin != null)
            {
                return _DisplayManagerPlugin.RequestEditSplit(monitorInfo, cellCount, splitKey, customName, settings);
            }
            return Task.FromResult(false);
        }

        public Task<string> WriteEasyArrangeSettings(EAMonitorSettings eaMonitorSettings)
        {
            if (_SettingsPlugin == null)
            {
                string err = "SettingsPlugin is null.";
                writelog($"WriteEasyArrangeSettings(), {err}");
                return Task.FromResult(err);
            }
            return _SettingsPlugin.WriteEasyArrangeSettings(eaMonitorSettings);
        }

        public Task<EAMonitorSettings> ReadEasyArrangeSettings(string monitorModel, string serialNumber)
        {
            if (_SettingsPlugin == null)
            {
                string err = "SettingsPlugin is null.";
                writelog($"WriteEasyArrangeSettings(), {err}");
                return Task.FromResult<EAMonitorSettings>(null);
            }
            return _SettingsPlugin.ReadEasyArrangeSettings(monitorModel, serialNumber);
        }

        private void _DisplayManagerPlugin_EAEditCompleted(object sender, string e)
        {
            if (EAEditCompleted != null)
            {
                Task.Run(() => EAEditCompleted.Invoke(this, e));
            }
        }

        private void _DisplayManagerPlugin_EAEditStarted(object sender, string e)
        {
            if (EAEditStarted != null)
            {
                Task.Run(() => EAEditStarted.Invoke(this, e));
            }
        }

        //Robert_Lin, 2024-8-4 added
        public Task<bool> EAEditCommand(MonitorInfo monitorInfo, EAArgs args)
        {
            if (_DisplayManagerPlugin != null)
            {
                return _DisplayManagerPlugin.EAEditCommand(monitorInfo, args);
            }
            return Task.FromResult(false);
        }

        private void _DisplayManagerPlugin_EAEditReturn(object sender, EAArgs e)
        {
            if (EAEditReturn != null)
            {
                Task.Run(() => EAEditReturn.Invoke(this, e));
            }
        }

        #endregion

        #region SW Update implementation

        public Task<SWUpdateInfoPackage> SW_GetSWUpdateInfo(bool isShowNotify = true, bool isDefer = false, bool isForce = false)
        {
            if (_SWUpdatePlugin != null)
            {
                return Task.FromResult(_SWUpdatePlugin.GetSWUpdateInfo(isShowNotify, isDefer, isForce).Result);
            }
            return Task.FromResult(new SWUpdateInfoPackage());
        }

        public Task<List<SWUpdateInfo>> SW_DownloadAndInstall(List<SWUpdateInfo> swUpdateInfos, string installPath = "")
        {
            return Task.FromResult(_SWUpdatePlugin.DownloadAndInstall(swUpdateInfos, installPath).Result);
        }

        private Task<bool> SW_SetSWUpdateInfoPackage(SWUpdateInfoPackage swUpdateInfoPackage)
        {
            if (_SettingsPlugin != null)
            {
                DDPMSettings config = _SettingsPlugin.ReloadAppConfigData().Result;
                config.UserSettings.DelaySWUpdateInfoPackage = swUpdateInfoPackage;
                return Task.FromResult(_SettingsPlugin.SetAppConfigData(config).Result);
            }
            return Task.FromResult(false);
        }

        private Task<bool> SW_CheckSWUpdate()
        {
            if (_SWUpdatePlugin == null)
                return Task.FromResult(false);
            SW_SetDelaySWUpdateInfoPackage();
            List<SWUpdateInfo> swUpdateInfos = _SWUpdatePlugin.CheckUpdate(true).Result;
            bool b = true;
            foreach (SWUpdateInfo swUpdateInfo in swUpdateInfos)
            {
                if (swUpdateInfo.SWUErrorCode != SWUErrorCode.NoError)
                {
                    b = false;
                }
            }
            return Task.FromResult(b);
        }

        private void SW_SetDelaySWUpdateInfoPackage()
        {
            if (_SettingsPlugin != null && _SWUpdatePlugin != null)
            {
                DDPMSettings config = _SettingsPlugin.ReloadAppConfigData().Result;

                if (config != null) // 2024-08-16 Elie, check if null before using.
                    _SWUpdatePlugin.SetDelaySWUpdateInfoPackage(config.UserSettings.DelaySWUpdateInfoPackage);
                else
                {
                    writelog("[SW_SetDelaySWUpdateInfoPackage], ReloadAppConfigData is null.");
                }

            }
        }

        #endregion

        #region ImpExpSettings
        public Task<bool> DisplayExportSettings(MonitorInfo monitorInfo, string path)
        {
            //need test, but need other function
            ////if vcp code is null, get vcp code
            //List<DDPMMonitorSettings> settings = _SettingsPlugin.ReloadMonitorSettings(monitorInfo.modelName).Result;
            //if (settings != null)
            //{
            //    foreach (DDPMMonitorSettings monitorSettings in settings)
            //    {
            //        if (monitorSettings != null)
            //        {
            //            if (monitorSettings.ServiceTag == monitorInfo.edid.ServiceTag)
            //            {
            //                foreach (VCP vcp in monitorSettings.VCPs)
            //                {
            //                    if (vcp.Value == null)
            //                    {
            //                        byte b_vcpcode = Convert.ToByte(vcp.Code);
            //                        ObjGetVCP res = GetVCPCapability(monitorInfo, b_vcpcode).Result;
            //                        if(res.result)
            //                        {
            //                            vcp.Value.Add((int)res.value);
            //                        }
            //                    }
            //            }
            //            }
            //        }
            //    }
            //}

            //expot settings
            if (_SettingsPlugin.DisplayExportSettings(monitorInfo.modelName, monitorInfo.edid.ServiceTag, path).Result)
            {
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }
        #endregion
        #region Gaming
        public Task<GamingDisplayPropertiesInfo> GetGamingProperties(MonitorInfo monitorInfo)
        {
            if (_DisplayManagerPlugin != null)
            {
                return Task.FromResult(_DisplayManagerPlugin.GetGamingProperties(monitorInfo).Result);
            }
            return Task.FromResult(new GamingDisplayPropertiesInfo());
        }
        public Task<bool> SetGameEnhancementMode(MonitorInfo monitorInfo, Gaming_GameEnhancementMode GameEnhancementMode)
        {
            if (_DisplayManagerPlugin != null)
            {
                return Task.FromResult(_DisplayManagerPlugin.SetGameEnhancementMode(monitorInfo, GameEnhancementMode).Result);
            }

            return Task.FromResult(false);
        }
        public Task<bool> SetGaming_ResponseTime(MonitorInfo monitorInfo, Gaming_ResponseTime ResponseTime)
        {
            if (_DisplayManagerPlugin != null)
            {
                return Task.FromResult(_DisplayManagerPlugin.SetGaming_ResponseTime(monitorInfo, ResponseTime).Result);
            }
            return Task.FromResult(false);
        }
        public Task<bool> SetGaming_DarkStabilizer(MonitorInfo monitorInfo, Gaming_DarkStabilizer DarkStabilizer)
        {
            if (_DisplayManagerPlugin != null)
            {
                return Task.FromResult(_DisplayManagerPlugin.SetGaming_DarkStabilizer(monitorInfo, DarkStabilizer).Result);
            }
            return Task.FromResult(false);
        }
        public Task<bool> SetGaming_HDRType(MonitorInfo monitorInfo, Gaming_HDRType HDRType)
        {
            if (_DisplayManagerPlugin != null)
            {
                return Task.FromResult(_DisplayManagerPlugin.SetGaming_HDRType(monitorInfo, HDRType).Result);
            }
            return Task.FromResult(false);
        }
        public Task<bool> SetGaming_DualResolutionType(MonitorInfo monitorInfo, Gaming_DualResolutionType DualResolutionType)
        {
            if (_DisplayManagerPlugin != null)
            {
                return Task.FromResult(_DisplayManagerPlugin.SetGaming_DualResolutionType(monitorInfo, DualResolutionType).Result);
            }
            return Task.FromResult(false);
        }
    #endregion

    #region DTPProxy implementation

    public async Task<int> GetDpiValueByDTP(string itemID) {
      return await Task.Run(() => _DTPProxyPlugin.GetDpiValue(itemID));
    }

    public Task SetDPIValueByDTP(string itemID, int newValue) {
      writelog("DeviceMangerPlugin received SetDPIValueByDTP requested ...");
      writelog($"Target DeviceID is {itemID}");
      writelog($"Target DPI Value is {newValue}");
      _DTPProxyPlugin.SetDPIValue(itemID, newValue);
      return Task.FromResult(true);
    }

    #endregion

    #endregion


    #region Private Methods

    private void SystemEvents_DisplaySettingsChanged(object sender, EventArgs e)
        {
            if (displayInOut)
            {
                writelog($"DisplaySettingsChanged: {sender}, e:{e}, rescan monitor");

                if (isLetDisplayServiceIdle == true)
                {
                    writelog("The idle state is true to drop display settings change event, need caller to unblock this param");
                    return;
                }
                //Bruce 08-09 Added judgment that if the number of screens does not change, the screen orientation adjustment function will not be performed. (For example: PxP change will trigger this event, but the screen is not actually plugged in or out)
                bool displayDeviceNumChange = false;
                int AllScreens = Screen.AllScreens.Length;
                if (_lastScreenCount != AllScreens)
                {
                    displayDeviceNumChange = true;
                    _lastScreenCount = AllScreens;
                }
                Task.Run(() =>
                {
                    lock (_PluginConditionLock)
                    {
                        //Call VCP to catch updated monitor info
                        _AllInfoMonitors = _DisplayManagerPlugin.GetMonitors(true).Result;

                        //List<MonitorInfo> pre_mo = new List<MonitorInfo>();
                        //pre_mo.AddRange(_AllInfoMonitorsRecord);
                        List<MonitorInfo> new_mo = new List<MonitorInfo>();
                        if (_AllInfoMonitors.Count > 0)
                            new_mo.AddRange(_AllInfoMonitors);

                        _DisplayManagerPlugin.UpdateExistAlsConfig(new_mo).Wait();

                        writelog($"[DeviceManager] Got event SystemEvents_DisplaySettingsChanged, monitor count {_AllInfoMonitors.Count}");
                        if (_AllInfoMonitors.Count > 0)
                            OnDeviceChanged(_AllInfoMonitors[0], null, DeviceChangedType.NotifyOnly, "DisplayChanged");//DeviceChangedType.Display_PlugIn);
                        else
                            OnDeviceChanged(null, null, DeviceChangedType.NotifyOnly, "DisplayChanged");

                        if (displayDeviceNumChange && _AllInfoMonitors.Count > 0)
                        {
                            _DisplayManagerPlugin.SetDisplayOrientation(_AllInfoMonitors).Wait();
                        }
                    }
                });
            }
        }

        protected virtual void OnVCPchanged(VCPchangedEventArgs e)
        {
            writelog("DeviceMangerPlugin brocast OnVCPchanged ...");

            EventHandler<VCPchangedEventArgs> handler = VCPchanged;
            if (handler != null)
                Task.Run(() => handler.Invoke(this, e));

            //The Asynchronous Programming Model (APM) (using IAsyncResult and BeginInvoke) is no longer the preferred method of making asynchronous calls.
            //The Task-based Asynchronous Pattern (TAP) is the recommended async model as of .NET Framework 4.5.
            //Because of this, and because the implementation of async delegates depends on remoting features not present in .NET Core, BeginInvoke and EndInvoke delegate calls are not supported in .NET Core.
            //This is discussed in GitHub issue dotnet/corefx #5940.
        }

        protected virtual void OnDDCCIStatuschanged(DDCCIchangedEventArgs e)
        {
            writelog("DeviceMangerPlugin brocast OnDDCCIStatuschanged ...");

            //DDCCIStatuschanged?.Invoke(this, e);
            EventHandler<DDCCIchangedEventArgs> handler = DDCCIStatuschanged;
            if (handler != null)
                Task.Run(() => handler.Invoke(this, e));

            if (_NKVMPlugin != null)
            {
                _NKVMPlugin.NKVM_ChangeLimitedSW(e.monitors, e.DDCisON).Wait();
            }

            //The Asynchronous Programming Model (APM) (using IAsyncResult and BeginInvoke) is no longer the preferred method of making asynchronous calls.
            //The Task-based Asynchronous Pattern (TAP) is the recommended async model as of .NET Framework 4.5.
            //Because of this, and because the implementation of async delegates depends on remoting features not present in .NET Core, BeginInvoke and EndInvoke delegate calls are not supported in .NET Core.
            //This is discussed in GitHub issue dotnet/corefx #5940.
        }

        protected virtual void OnDisplaychanged(DisplaychangedEventArgs e)
        {
            writelog($"DeviceMangerPlugin brocast OnDisplaychanged ...(monitor count {e.monitors.Count})");

            EventHandler<DisplaychangedEventArgs> handler = Displaychanged;
            //if (handler != null)
            //    handler.Invoke(this, e);
            if (_DisplayManagerPlugin != null)
            {
                _DisplayManagerPlugin.SetDisplayOrientation(e.monitors).Wait();
            }
            DeviceChangedEventArgs arg = new DeviceChangedEventArgs();
            arg.changedProperty = "DisplayChanged";
            arg.type = DeviceChangedType.NotifyOnly;
            EventHandler<DeviceChangedEventArgs> devHandler = DeviceChanged;
            if (devHandler != null)
                devHandler.Invoke(this, arg);
        }

        private void OnPeripheralsNotify(DeviceChangedEventArgs data)
        {
            if (Peripherals_Notify != null)
                Peripherals_Notify?.Invoke(this, data);
        }

        private void OnPeripheralsUpdateNotify(bool e)
        {
            Peripherals_UpdateNotify?.Invoke(this, e);
        }

        #region FW Update

        private void OnProgressUpdateEvent(FWUpdateInfo fWUpdateInfo)
        {
            //ProgressUpdate_Notify?.Invoke(this, fWUpdateInfo);
            EventHandler<FWUpdateInfo> handler = ProgressUpdate_Notify;
            if (handler != null)
                handler.Invoke(this, fWUpdateInfo);
        }

        private void OnUILockEvent(bool isLockFWU_UI)
        {
            EventHandler<bool> handler = FWU_UILock_Notify;
            if (handler != null)
                handler.Invoke(this, isLockFWU_UI);
        }

        private void OnFWSaveEvent(FWUpdateInfoPackage fwUpdateInfoPackage)
        {
            SetFWUpdateInfoPackage(fwUpdateInfoPackage).Wait();
        }

        private void OnCheckUpdateScheduleEvent()
        {
            CheckUpdate();
        }

        private void OnGetDeviceinfos()
        {
            GetDeviceinfos().Wait();
        }

        private void OnFWSaveUODEvent(DokcUODUpdateInfoPackage fwUpdateInfo)
        {
            SetUODFWUInfoPackage(fwUpdateInfo).Wait();
        }

        private void OnCheckUODEvent()
        {
            CheckUODFWUInfoPackage();
        }

        private void OnDownloadAndInstall_ResultEvent(List<FWUpdateInfo> fWUpdateInfos)
        {
            EventHandler<List<FWUpdateInfo>> handler = DownloadAndInstall_Result_Notify;
            if (handler != null)
                handler.Invoke(this, fWUpdateInfos);
        }

        #endregion

        #region SW Update

        private void OnSWSaveEvent(SWUpdateInfoPackage swUpdateInfoPackage)
        {
            SW_SetSWUpdateInfoPackage(swUpdateInfoPackage).Wait();
        }

        private void OnCheckSWUpdateScheduleEvent()
        {
            SW_CheckSWUpdate();
        }

        #endregion

        /// <summary>
        /// Update ALS with All Monitors, when Monitor plugin/off
        /// </summary>
        private void updateALSwithAllMonitors()
        {
            if (_DisplayManagerPlugin != null)
            {
                var mos = _DisplayManagerPlugin.GetMonitors().Result;
                if (mos.Count != 0)
                {
                    //foreach (var mo in mos)
                    for (int i = 0; i < mos.Count; i++) // Dean 0614 fix exception [Collection was modified; enumeration operation may not execute.]
                    {
                        MonitorInfo mo = mos[i];
                        _DisplayManagerPlugin.UpdateALSFeatureValue(mo);
                    }
                }
            }
        }

        protected virtual void OnDeviceChanged(MonitorInfo mo, DeviceInfo di, DeviceChangedType type, string changedProperty = "")
        {
            DeviceChangedEventArgs _EventArgs = new DeviceChangedEventArgs();

            if (type == DeviceChangedType.NotifyOnly)
            {
                writelog("[OnDeviceChanged] Notify event to registers");
                Task.Run(() => updateALSwithAllMonitors());
            }
            else
            {
                // << 240715 revised by Hess to handle dongle emply
                //if (di == null)
                //{
                //    writelog("[OnDeviceChanged] null pheripherals device info object!");
                //    return;
                //}
                _EventArgs.deviceID = di?.ID.ToString() ?? "";// temp unique guid;
                                                              // >>
            }
            _EventArgs.type = type;
            _EventArgs.device_display = mo;
            _EventArgs.device_peripherals = di;
            _EventArgs.changedProperty = changedProperty;
            EventHandler<DeviceChangedEventArgs> handler = DeviceChanged;
            if (handler != null)
                handler.Invoke(this, _EventArgs);

            if (changedProperty.ToLower().Contains("add"))
            {
                //else
                //{
                //    _NKVMPlugin.MonitorPlug();
                //    SupportedNKVMMonitors();
                //}
                CheckUpdate();
                CheckUODFWUInfoPackage(true);
            }
            else if (changedProperty.ToLower().Contains("remove"))
            {
            }
            else if ((string.Compare(changedProperty, "DisplayChanged", true) == 0))
            {
                if (_NKVMPlugin != null)
                {
                    //if (mo != null)
                    //{
                    //    _NKVMPlugin.MonitorPlug();
                    //    SupportedNKVMMonitors();
                    //}
                    _NKVMPlugin.UpdateMonitorInfo(_AllInfoMonitors);
                    SupportedNKVMMonitors();
                }
            }
            //0617 Bruce 如使用Dell的Popup視窗顯示，需卡執行緒，故另外使用一條執行緒給Popup顯示用
            var thread = new Thread(() =>
            {
                CheckDocks();
            });
            thread.Start();
        }

        //0613 Bruce 用於看是否連接超過2個dock
        private void CheckDocks()
        {
            if (_PeripheralsPlugin == null)
            {
                return;
            }
            List<DeviceInfo> _peripheralslist = _PeripheralsPlugin.GetDevices().Result.deviceInfo;

            // 2024-08-07 Elie, fix got exception while don't check this is null or not.
            if ((_peripheralslist == null) || (_peripheralslist.Count == 0))
                return;
            // >>
            int dockCount = 0;
            foreach (DeviceInfo deviceInfo in _peripheralslist)
            {
                //0617 Bruce 在其他電腦有發現List有item，但是item會是null，故新增判斷
                if (deviceInfo != null)
                {
                    if (deviceInfo.Type == DeviceType.LogicalDock)
                    {
                        dockCount++;
                    }
                    if (dockCount >= 2)
                    {
                        break;
                    }
                }
            }
            if (dockCount >= 2)
            {
                //0704 Bruce 使用另一種Popup顯示
                PopupBaseManage popupBaseManage = new PopupBaseManage();
                popupBaseManage.FWU_Show("Warning", "Multiple docks are detected. Keep only one dock connected to prevent damage to your dock(s).", "", "", null, true, 5);
                //0614 Bruce 先使用ToastContentBuilder做通知，之後修改回客戶的模板
                //ToastContentBuilder toastContentBuilder = new ToastContentBuilder();
                //toastContentBuilder.AddArgument("DDPM");
                //toastContentBuilder.AddText("Warning");
                //toastContentBuilder.AddText("Multiple docks are detected. Keep only one dock connected to prevent damage to your dock(s).");
                //toastContentBuilder.SetToastScenario(ToastScenario.IncomingCall);

                //toastContentBuilder.Show(); // 顯示Toast通知
                //0617 Bruce 使用Dell的Popup視窗顯示，目前已可以使用並停留，但是Popup視窗的標頭沒有顯示，還需要詢問
                /*var windowClosedEvent = new ManualResetEvent(false);
                var thread = new Thread(() =>
                {
                    PopupMode popupMode = PopupMode.Normal;
                    IPopupMgr popupManager = new PopupMgr();
                    PopupBase popupBase = new PopupBase(true, false, "Warning", "Multiple docks are detected. Keep only one dock connected to prevent damage to your dock(s).");
                    IUXPopup popup = popupManager.AddPopup(popupMode, "", popupBase, true, true);
                    popup.Tag = "DDPM";
                    popup.IsOpen = true;
                    popup.Closed += (s, e) =>
                    {
                        windowClosedEvent.Set(); //視窗關閉時通知主執行緒
                        Dispatcher.ExitAllFrames(); //結束WPF的消息循環
                    };
                    Dispatcher.Run(); // 確保WPF消息循環運行
                });
                thread.SetApartmentState(ApartmentState.STA);
                thread.Start();
                thread.Join(); // 確保執行緒已結束*/
            }
        }

        private void show_displays(object sender, VCPchangedEventArgs e)
        {
            writelog("Receive VcpChanged Event Notify from DisplayManagerPlugin");
            writelog("Send out VcpChanged Event Notify from DeviceMangerPlugin");

            VCPchangedEventArgs _VCPchangedEventArgs = new VCPchangedEventArgs();
            _VCPchangedEventArgs.vcpcode = e.vcpcode;
            _VCPchangedEventArgs.value = e.value;
            //0607 Bruce 因VCPChange事件需要取得螢幕資訊故請Jarvis新增這段變數 代為新增
            _VCPchangedEventArgs.monitor = e.monitor;
            OnVCPchanged(_VCPchangedEventArgs);
        }

        private void show_DDCCIchangedEventArgs(object sender, DDCCIchangedEventArgs e)
        {
            writelog("Receive DDCCIStatuschanged Event Notify from DisplayManagerPlugin");
            writelog("Send DDCCIStatuschanged Event Notify from DeviceMangerPlugin");

            DDCCIchangedEventArgs _DDCCIchangedEventArgs = new DDCCIchangedEventArgs();
            _DDCCIchangedEventArgs.DDCisON = e.DDCisON;
            _DDCCIchangedEventArgs.monitors = e.monitors;
            OnDDCCIStatuschanged(_DDCCIchangedEventArgs);
        }

        private void show_displays_changed(object sender, DisplaychangedEventArgs e)
        {
            writelog("Receive Displaychanged Event Notify from DisplayManagerPlugin");
            writelog("Send out Displaychanged Event Notify from DeviceMangerPlugin");

            DisplaychangedEventArgs _displaychangedEventArgs = new DisplaychangedEventArgs();
            _displaychangedEventArgs.count = e.count;
            _displaychangedEventArgs.monitors = e.monitors;
            OnDisplaychanged(_displaychangedEventArgs);
        }

        private void show_colorpreset(object sender, VCPchangedEventArgs e)
        {
            writelog("Receive VcpChanged Event Notify from ColorPresetPlugin");
            writelog("Send out VcpChanged Event Notify from ColorPresetPlugin");

            VCPchangedEventArgs _VCPchangedEventArgs = new VCPchangedEventArgs();
            _VCPchangedEventArgs.vcpcode = e.vcpcode;
            _VCPchangedEventArgs.value = e.value;
            //0607 Bruce 因VCPChange事件需要取得螢幕資訊故請Jarvis新增這段變數 代為新增
            _VCPchangedEventArgs.monitor = e.monitor;
            OnVCPchanged(_VCPchangedEventArgs);
        }

        private void show_peripheralsNotify(object sender, DeviceChangedEventArgs e)
        {
            writelog("Receive Notify Event from PeripheralsPlugin");
            writelog("Send out Notify Event from DeviceMangerPlugin");

            OnPeripheralsNotify(e);
            OnDeviceChanged(null, e.device_peripherals, e.type, e.changedProperty);
        }

        private void show_peripheralsUpdateNotify(object sender, bool e)
        {
            writelog("Receive UpdateNotify Event from PeripheralsPlugin");
            writelog("Send out UpdateNotify Event from DeviceMangerPlugin");

            OnPeripheralsUpdateNotify(e);
        }

        private void show_fwProgressUpdateEvent(object sender, FWUpdateInfo e)
        {
            OnProgressUpdateEvent(e);
        }

        private void show_fwCheckUpdateScheduleEvent(object sender, EventArgs e)
        {
            OnCheckUpdateScheduleEvent();
        }

        private void show_fwSaveUpdateInfoPackage(object sender, FWUpdateInfoPackage e)
        {
            OnFWSaveEvent(e);
        }

        private void show_GetDeviceinfos(object sender, EventArgs e)
        {
            OnGetDeviceinfos();
        }

        private void show_fwUODUpdateInfo(object sender, DokcUODUpdateInfoPackage e)
        {
            OnFWSaveUODEvent(e);
        }

        private void show_CheckUODUpdateInfo(object sender, EventArgs e)
        {
            OnCheckUODEvent();
        }

        private void show_fwUpdateResultEvent(object sender, List<FWUpdateInfo> e)
        {
            OnDownloadAndInstall_ResultEvent(e);
        }

        private void show_swCheckUpdateScheduleEvent(object sender, EventArgs e)
        {
            OnCheckSWUpdateScheduleEvent();
        }

        private void show_swSaveUpdateInfoPackage(object sender, SWUpdateInfoPackage e)
        {
            OnSWSaveEvent(e);
        }

        /// <summary>
        /// //
        /// </summary>
        /// <param name="text"></param>
        /// <param name="log_type">0 means info, others means error</param>
        private void writelog(string text, log_type log_type = log_type.info)
        {
            text = "[DeviceManager] " + text;
            Console.WriteLine(text);

            if (Log != null) // Elie, the instance of Log is from DTH. So we just check if it's null or not.
            {
                if (log_type == log_type.info)
                    Log.Info(text);
                else
                    Log.Error(text);
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

        private void InitializeColorPresetPlugin()
        {
            if (_ColorPresetPlugin != null)
                return;

            _ColorPresetPlugin = _agent.PluginManager.FindPluginByType<IColorPresetSA>(PluginResolution.Dynamic);

            if (_ColorPresetPlugin is IFrameworkPluginConditionNotification pluginCondition)
            {
                pluginCondition.PluginConditionChangeHandler += OnColorPresetPluginConditionChangeHandler;
                GetCurrentColorPresetCondition();
            }
        }

        private void InitializePeripheralsPlugin()
        {
            if (_PeripheralsPlugin != null)
                return;

            _PeripheralsPlugin = _agent.PluginManager.FindPluginByType<IDPeMPlugin>(PluginResolution.Dynamic);

            //<< 240509 by Hess
            if (_PeripheralsPlugin != null)
            {
                _PeripheralsPlugin.Notify += show_peripheralsNotify;
                _PeripheralsPlugin.UpdateNotify += show_peripheralsUpdateNotify;
                //0617 Bruce 如使用Dell的Popup視窗顯示，需卡執行緒，故另外使用一條執行緒給Popup顯示用
                var thread = new Thread(() =>
                {
                    CheckDocks();
                });
                thread.Start();
            }
            //>>

            if (_PeripheralsPlugin is IFrameworkPluginConditionNotification pluginCondition)
            {
                pluginCondition.PluginConditionChangeHandler += OnPeripheralsPluginConditionChangeHandler;
                GetCurrentPeripheralsPluginCondition();
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

        //FW Update by Bruce
        private void InitializeFWUpdatePlugin()
        {
            if (_FWUpdatePlugin != null)
                return;

            _FWUpdatePlugin = _agent.PluginManager.FindPluginByType<IFWUpdateService>(PluginResolution.Dynamic);

            if (_FWUpdatePlugin is IFrameworkPluginConditionNotification pluginCondition)
            {
                pluginCondition.PluginConditionChangeHandler += OnFWUpdatePluginConditionChangeHandler;
                GetCurrentFWUpdatePluginCondition();
            }
        }

        private void InitializeNKVMPlugin()
        {
            if (_NKVMPlugin != null)
                return;

            _NKVMPlugin = _agent.PluginManager.FindPluginByType<INKVMService>(PluginResolution.Dynamic);

            if (_NKVMPlugin is IFrameworkPluginConditionNotification pluginCondition)
            {
                pluginCondition.PluginConditionChangeHandler += OnNKVMPluginConditionChangeHandler;
                GetCurrentNKVMPluginCondition();
            }
        }

        private void InitializeHotkeyPlugin()
        {
            if (_HotkeyPlugin != null)
                return;

            _HotkeyPlugin = _agent.PluginManager.FindPluginByType<IHotkey>(PluginResolution.Dynamic);
            if (_HotkeyPlugin is IFrameworkPluginConditionNotification pluginCondition)
            {
                pluginCondition.PluginConditionChangeHandler += OnHotkeyPluginConditionChangeHandler;
                GetCurrentHotkeyPluginCondition();
            }
        }

    private void InitializeSchedulerManagerPlugin() {
      if(_ScheduleManagerPlugin != null)
        return;

      _ScheduleManagerPlugin = _agent.PluginManager.FindPluginByType<ISchedulerManager>(PluginResolution.Dynamic);

      if(_ScheduleManagerPlugin is IFrameworkPluginConditionNotification pluginCondition) {
        pluginCondition.PluginConditionChangeHandler += OnScheduleManagerPluginConditionChangeHandler;
        GetCurrentScheduleManagerCondition();
      }
    }

    private void InitializeDTPProxyPlugin() {
      if(_DTPProxyPlugin != null)
        return;

      _DTPProxyPlugin = _agent.PluginManager.FindPluginByType<IDTPProxyPlugin>(PluginResolution.Dynamic);

      if(_ScheduleManagerPlugin is IFrameworkPluginConditionNotification pluginCondition) {
        pluginCondition.PluginConditionChangeHandler += OnDTPProxyPluginConditionChangeHandler;
        GetCurrentDTPProxyPluginCondition();
      }
    }

    private void InitializeSWUpdatePlugin()
        {
            if (_SWUpdatePlugin != null)
                return;

            _SWUpdatePlugin = _agent.PluginManager.FindPluginByType<ISWUpdateService>(PluginResolution.Dynamic);

            if (_SWUpdatePlugin is IFrameworkPluginConditionNotification pluginCondition)
            {
                pluginCondition.PluginConditionChangeHandler += OnSWUpdatePluginConditionChangeHandler;
                GetCurrentSWUpdatePluginCondition();
            }
        }

        private void GetCurrentScheduleManagerCondition()
        {
            _ = Task.Run(async () =>
            {
                var pluginCondition = await (_ScheduleManagerPlugin as IFrameworkPluginConditionNotification)?.CurrentConditionAsync();
                //PluginCondition _DisplayManagerPluginCondition;
                lock (_PluginConditionLock_ScheduleManager)
                {
                    if (pluginCondition is PluginErrorCondition)
                    {
                        writelog($"{nameof(GetCurrentScheduleManagerCondition)} - Schedule Manager Plugin is in an error condition");
                    }
                    else if (pluginCondition is PluginRunningCondition)
                    {
                        writelog($"{nameof(GetCurrentScheduleManagerCondition)} - Schedule Manager Plugin is in a running condition");
                    }
                    else if (pluginCondition is PluginStartedCondition)
                    {
                        writelog($"{nameof(GetCurrentScheduleManagerCondition)} - Schedule Manager Plugin is in a started condition");
                    }
                }
            });
        }

        private void GetCurrentDisplayManagerCondition()
        {
            _ = Task.Run(async () =>
            {
                var pluginCondition = await (_DisplayManagerPlugin as IFrameworkPluginConditionNotification)?.CurrentConditionAsync();
                //PluginCondition _DisplayManagerPluginCondition;
                lock (_PluginConditionLock_Display)
                {
                    if (pluginCondition is PluginErrorCondition)
                    {
                        writelog($"{nameof(GetCurrentDisplayManagerCondition)} - Display Manager Plugin is in an error condition");
                        //_DisplayManagerPluginCondition = pluginCondition;
                    }
                    else if (pluginCondition is PluginRunningCondition)
                    {
                        //_DisplayManagerPluginCondition = pluginCondition;
                        _DisplayManagerPlugin.VCPchanged += show_displays;
                        _DisplayManagerPlugin.DDCCIStatuschanged += show_DDCCIchangedEventArgs;
                        _DisplayManagerPlugin.Displaychanged += show_displays_changed;
                        //Robert_Lin, 2024-7-16 added to handle EasyArrange EAPlugin events
                        _DisplayManagerPlugin.EAEditStarted += _DisplayManagerPlugin_EAEditStarted;
                        _DisplayManagerPlugin.EAEditCompleted += _DisplayManagerPlugin_EAEditCompleted;
                        //Bruce 07-30 Added total screens
                        _lastScreenCount = Screen.AllScreens.Length;
                        //Robert_Lin, 2024-8-4 add new events
                        _DisplayManagerPlugin.EAEditReturn += _DisplayManagerPlugin_EAEditReturn;
                        //Bruce, 2024-08-09 add new event
                        _DisplayManagerPlugin.HDRChangeEvent += OnHDRStatusChangeHandler;
                        //0812 check required plugins before init
                        DoThingsAfterDisplayRelatedPluginsReady(nameof(GetCurrentDisplayManagerCondition));
                        //Bruce, 2024-0820 add new event
                        _DisplayManagerPlugin.GamingChangeEvent += OnGamingParamChangeHandler;

                        writelog($"{nameof(GetCurrentDisplayManagerCondition)} - Display Manager Plugin is in a running condition");
                    }
                    else if (pluginCondition is PluginStartedCondition)
                    {
                        //_DisplayManagerPluginCondition = pluginCondition;
                        _DisplayManagerPlugin.VCPchanged += show_displays;
                        _DisplayManagerPlugin.DDCCIStatuschanged += show_DDCCIchangedEventArgs;
                        _DisplayManagerPlugin.Displaychanged += show_displays_changed;
                        //Robert_Lin, 2024-7-16 added to handle EasyArrange EAPlugin events
                        _DisplayManagerPlugin.EAEditStarted += _DisplayManagerPlugin_EAEditStarted;
                        _DisplayManagerPlugin.EAEditCompleted += _DisplayManagerPlugin_EAEditCompleted;
                        //Bruce 07-30 Added total screens
                        _lastScreenCount = Screen.AllScreens.Length;
                        //Robert_Lin, 2024-8-4 add new events
                        _DisplayManagerPlugin.EAEditReturn += _DisplayManagerPlugin_EAEditReturn;
                        //Bruce, 2024-08-09 add new event
                        _DisplayManagerPlugin.HDRChangeEvent += OnHDRStatusChangeHandler;
                        //0812 check required plugins before init
                        DoThingsAfterDisplayRelatedPluginsReady(nameof(GetCurrentDisplayManagerCondition));
                        //Bruce, 2024-0820 add new event
                        _DisplayManagerPlugin.GamingChangeEvent += OnGamingParamChangeHandler;

                        writelog($"{nameof(GetCurrentDisplayManagerCondition)} - Display Manager Plugin is in a started condition");
                    }
                }
            });
        }

        //Required plugins:
        //1. _DisplayManagerPlugin
        //2. _SettingsPlugin
        private void DoThingsAfterDisplayRelatedPluginsReady(string caller)
        {
            if (_DisplayManagerPlugin == null || _SettingsPlugin == null)
            {
                writelog($"[DoThingsAfterDisplayRelatedPluginsReady] caller: {caller}");
                writelog($"[DoThingsAfterDisplayRelatedPluginsReady] Has _DisplayManagerPlugin:{(_DisplayManagerPlugin == null)}, has _SettingsPlugin: {_SettingsPlugin == null}");
                return;
            }
            _AllInfoMonitors.Clear();
            List<MonitorInfo> monitorInfos = GetMonitors().Result;//it it used to active monitor settings
            _AllInfoMonitors.AddRange(monitorInfos);

            _SettingsPlugin.ITSettingsActionEvent += _SettingsPlugin_ITSettingsActionEvent;

            GetLockRotateStatus();
            writelog($"[DoThingsAfterDisplayRelatedPluginsReady] caller: {caller}, OK. Monitor count is {_AllInfoMonitors.Count}");
        }

        private void GetCurrentColorPresetCondition()
        {
            _ = Task.Run(async () =>
            {
                var pluginCondition = await (_ColorPresetPlugin as IFrameworkPluginConditionNotification)?.CurrentConditionAsync();
                //PluginCondition _ColorPresetPluginCondition;
                lock (_PluginConditionLock_ColorPreset)
                {
                    if (pluginCondition is PluginErrorCondition)
                    {
                        writelog($"{nameof(GetCurrentColorPresetCondition)} - ColorPreset Plugin is in an error condition");
                        //_ColorPresetPluginCondition = pluginCondition;
                    }
                    else if (pluginCondition is PluginRunningCondition)
                    {
                        writelog($"{nameof(GetCurrentColorPresetCondition)} - ColorPreset Plugin is in a running condition");
                        //_ColorPresetPluginCondition = pluginCondition;
                        _ColorPresetPlugin.VCPchanged += show_colorpreset;

                        if (_SettingsPlugin != null)
                        {
                            _AllAppData = _ColorPresetPlugin.GetInstalledAppsList().Result;//_ColorPresetPlugin.FindAppsbyShell().Result;
                        }
                    }
                    else if (pluginCondition is PluginStartedCondition)
                    {
                        writelog($"{nameof(GetCurrentColorPresetCondition)} - ColorPreset Plugin is in a started condition");
                        //_ColorPresetPluginCondition = pluginCondition;
                        _ColorPresetPlugin.VCPchanged += show_colorpreset;

                        if (_SettingsPlugin != null)
                        {
                            _AllAppData = _ColorPresetPlugin.GetInstalledAppsList().Result;//_ColorPresetPlugin.FindAppsbyShell().Result;
                        }
                    }
                }
            });
        }

        private void GetCurrentPeripheralsPluginCondition()
        {
            _ = Task.Run(async () =>
            {
                var pluginCondition = await (_PeripheralsPlugin as IFrameworkPluginConditionNotification)?.CurrentConditionAsync();
                //PluginCondition _PeripheralsPluginCondition;
                lock (_PluginConditionLock_Peripherals)
                {
                    if (pluginCondition is PluginErrorCondition)
                    {
                        writelog($"{nameof(GetCurrentPeripheralsPluginCondition)} - Peripherals Plugin is in an error condition");
                        //_PeripheralsPluginCondition = pluginCondition;
                    }
                    else if (pluginCondition is PluginRunningCondition)
                    {
                        writelog($"{nameof(GetCurrentPeripheralsPluginCondition)} - Peripherals Plugin is in a running condition");
                    }
                    else if (pluginCondition is PluginStartedCondition)
                    {
                        writelog($"{nameof(GetCurrentPeripheralsPluginCondition)} - Peripherals Plugin is in a started condition");
                    }
                }
            });
        }

        private void GetCurrentSettingsPluginCondition()
        {
            _ = Task.Run(async () =>
            {
                var pluginCondition = await (_SettingsPlugin as IFrameworkPluginConditionNotification)?.CurrentConditionAsync();
                //PluginCondition _SettingsPluginCondition; 
                lock (_PluginConditionLock_Settings)
                {
                    if (pluginCondition is PluginErrorCondition)
                    {
                        writelog($"{nameof(GetCurrentSettingsPluginCondition)} - Settings Plugin is in an error condition");
                    }
                    else if (pluginCondition is PluginRunningCondition || pluginCondition is PluginStartedCondition)
                    {
                        writelog($"{nameof(GetCurrentSettingsPluginCondition)} - Settings Plugin is in a running/started condition");

                        DDPMSettings config = _SettingsPlugin.ReloadAppConfigData().Result;
                        if (config != null)
                        {
                            //0612 Bruce 自動旋轉畫面功能，因需使用display跟settings兩個Plugin，其中一個可能還沒被叫起來，故兩邊都新增取得狀態方法。
                            //GetLockRotateStatus();
                            //0812 check required plugins before init
                            DoThingsAfterDisplayRelatedPluginsReady(nameof(GetCurrentDisplayManagerCondition));

                            SetDelayFWUpdateInfoPackage();
                            CheckUODFWUInfoPackage();
                            //load hotkeysetting
                            ReloadHotkeyConfigData();
                            ToNKVM_SupportedMonitorList();
                            ToNKVM_initHotKeys();                            
                        }
                    }
                    else
                    {
                        writelog($"{nameof(GetCurrentSettingsPluginCondition)} - Settings Plugin is in unknow condition: {pluginCondition}");
                    }
                }
            });
        }

        private void _SettingsPlugin_ITSettingsActionEvent(object sender, ITSettingEventArgs e)
        {
            _ = Task.Run(() =>
            {
                if (_SettingsPlugin == null)
                {
                    writelog("[From user Settings with IT event] null Device Manager object!");
                    return;
                }

                if (e == null || e == EventArgs.Empty)
                {
                    writelog("[From user Settings with IT event] Got Empty ITSettingEventArgs!");
                    return;
                }
                //
                //Do IT Settings update notify
                //
                OnITSettingsActionEventNotify(e);
            });
        }

        public event EventHandler<ITSettingEventArgs> ITSettingsActionEvent;

        //Target to notify User setting
        private void OnITSettingsActionEventNotify(ITSettingEventArgs e)
        {
            if (ITSettingsActionEvent == null || e == null || e == EventArgs.Empty)
                return;

            EventHandler<ITSettingEventArgs> Handler = ITSettingsActionEvent;
            if (Handler != null)
            {
                Handler.Invoke(this, e);
                writelog($"ITSettingsActionEvent Invoked at DeviceManagerPlugins");
            }
        }

        //FW Update by Bruce
        private void GetCurrentFWUpdatePluginCondition()
        {
            _ = Task.Run(async () =>
            {
                var pluginCondition = await (_FWUpdatePlugin as IFrameworkPluginConditionNotification)?.CurrentConditionAsync();
                //PluginCondition _FWUpdatePluginCondition;
                lock (_PluginConditionLock)
                {
                    if (pluginCondition is PluginErrorCondition)
                    {
                        writelog($"{nameof(GetCurrentFWUpdatePluginCondition)} - FW Update Plugin is in an error condition");
                        //_FWUpdatePluginCondition = pluginCondition;
                    }
                    else if (pluginCondition is PluginRunningCondition)
                    {
                        writelog($"{nameof(GetCurrentFWUpdatePluginCondition)} - FW Update Plugin is in a running condition");
                        //0531 Bruce 因使用者可能在執行前將裝置移除，故將檢查是否延期的功能修改到底層的排程中
                        //_FWUpdatePluginCondition = pluginCondition;
                        _FWUpdatePlugin.ProgressUpdate_Notify += show_fwProgressUpdateEvent;
                        _FWUpdatePlugin.CollCheckUpdate += show_fwCheckUpdateScheduleEvent;
                        _FWUpdatePlugin.CallSaveUpdateInfoPackage += show_fwSaveUpdateInfoPackage;
                        _FWUpdatePlugin.StartCheckUpdateScheduleTimer();
                        _FWUpdatePlugin.CallGetDeviceInfos += show_GetDeviceinfos;
                        _FWUpdatePlugin.CallSaveUODFWDeviceInfos += show_fwUODUpdateInfo;
                        SetDelayFWUpdateInfoPackage();
                        _FWUpdatePlugin.CallCheckUODFWInfos += show_CheckUODUpdateInfo;
                        CheckUODFWUInfoPackage();
                        _FWUpdatePlugin.DownloadAndInstall_Result_Notify += show_fwUpdateResultEvent;
                        _FWUpdatePlugin.CallPopup += CallPopup;
                    }
                    else if (pluginCondition is PluginStartedCondition)
                    {
                        writelog($"{nameof(GetCurrentFWUpdatePluginCondition)} - FW Update Plugin is in a started condition");
                        //0531 Bruce 因使用者可能在執行前將裝置移除，故將檢查是否延期的功能修改到底層的排程中
                        //_FWUpdatePluginCondition = pluginCondition;
                        _FWUpdatePlugin.ProgressUpdate_Notify += show_fwProgressUpdateEvent;
                        _FWUpdatePlugin.CollCheckUpdate += show_fwCheckUpdateScheduleEvent;
                        _FWUpdatePlugin.CallSaveUpdateInfoPackage += show_fwSaveUpdateInfoPackage;
                        _FWUpdatePlugin.StartCheckUpdateScheduleTimer();
                        _FWUpdatePlugin.CallGetDeviceInfos += show_GetDeviceinfos;
                        _FWUpdatePlugin.CallSaveUODFWDeviceInfos += show_fwUODUpdateInfo;
                        SetDelayFWUpdateInfoPackage();
                        _FWUpdatePlugin.CallCheckUODFWInfos += show_CheckUODUpdateInfo;
                        CheckUODFWUInfoPackage();
                        _FWUpdatePlugin.DownloadAndInstall_Result_Notify += show_fwUpdateResultEvent;
                        _FWUpdatePlugin.CallPopup += CallPopup;
                    }
                }
            });
        }

        private void GetCurrentNKVMPluginCondition()
        {
            _ = Task.Run(async () =>
            {
                var pluginCondition = await (_NKVMPlugin as IFrameworkPluginConditionNotification)?.CurrentConditionAsync();
                //PluginCondition _USBKVMPluginCondition;
                lock (_PluginConditionLock_NKVM)
                {
                    if (pluginCondition is PluginErrorCondition)
                    {
                        writelog($"{nameof(GetCurrentNKVMPluginCondition)} - NKVM Plugin is in an error condition");
                        //_NKVMPluginCondition = pluginCondition;
                    }
                    else if (pluginCondition is PluginRunningCondition)
                    {
                        writelog($"{nameof(GetCurrentNKVMPluginCondition)} - NKVM Plugin is in a running condition");
                        //_NKVMPluginCondition = pluginCondition;
                        ToNKVM_SupportedMonitorList();
                        ToNKVM_initHotKeys();
                    }
                    else if (pluginCondition is PluginStartedCondition)
                    {
                        writelog($"{nameof(GetCurrentNKVMPluginCondition)} - NKVM Plugin is in a started condition");
                        //_NKVMPluginCondition = pluginCondition;
                        ToNKVM_SupportedMonitorList();
                        ToNKVM_initHotKeys();
                    }
                }
            });
        }

        private void GetCurrentHotkeyPluginCondition()
        {
            _ = Task.Run(async () =>
            {
                var pluginCondition = await (_HotkeyPlugin as IFrameworkPluginConditionNotification)?.CurrentConditionAsync();
                //PluginCondition _HotkeyPluginCondition;
                lock (_PluginConditionLock_Hotkey)
                {
                    if (pluginCondition is PluginErrorCondition)
                    {
                        writelog($"{nameof(GetCurrentHotkeyPluginCondition)} - Hotkey Plugin is in an error condition");
                        //_HotkeyPluginCondition = pluginCondition;
                        //unhook keyboard
                        _HotkeyPlugin.KeyUp -= Keyboard_KeyUpProc;
                        _HotkeyPlugin.Unhook();
                    }
                    else if (pluginCondition is PluginRunningCondition)
                    {
                        writelog($"{nameof(GetCurrentHotkeyPluginCondition)} - Hotkey Plugin is in a running condition");
                    }
                    else if (pluginCondition is PluginStartedCondition)
                    {
                        writelog($"{nameof(GetCurrentHotkeyPluginCondition)} - Hotkey Plugin is in a started condition");
                        //_HotkeyPluginCondition = pluginCondition;
                        //hook keyboard
                        _HotkeyPlugin.Hook();
                        _HotkeyPlugin.KeyUp += Keyboard_KeyUpProc;
                    }
                }
            });
        }

        private void GetCurrentSWUpdatePluginCondition()
        {
            _ = Task.Run(async () =>
            {
                var pluginCondition = await (_SWUpdatePlugin as IFrameworkPluginConditionNotification)?.CurrentConditionAsync();
                lock (_PluginConditionLock)
                {
                    if (pluginCondition is PluginErrorCondition)
                    {
                        writelog($"{nameof(GetCurrentSWUpdatePluginCondition)} - SW Update Plugin is in an error condition");
                    }
                    else if (pluginCondition is PluginRunningCondition)
                    {
                        writelog($"{nameof(GetCurrentSWUpdatePluginCondition)} - SW Update Plugin is in a running condition");
                        _SWUpdatePlugin.CollCheckUpdate += show_swCheckUpdateScheduleEvent;
                        _SWUpdatePlugin.CallSaveUpdateInfoPackage += show_swSaveUpdateInfoPackage;
                        _SWUpdatePlugin.StartCheckUpdateScheduleTimer();
                        SW_SetDelaySWUpdateInfoPackage();
                        _SWUpdatePlugin.CallPopup += CallPopup;
                    }
                    else if (pluginCondition is PluginStartedCondition)
                    {
                        writelog($"{nameof(GetCurrentSWUpdatePluginCondition)} - SW Update Plugin is in a started condition");
                        _SWUpdatePlugin.CollCheckUpdate += show_swCheckUpdateScheduleEvent;
                        _SWUpdatePlugin.CallSaveUpdateInfoPackage += show_swSaveUpdateInfoPackage;
                        _SWUpdatePlugin.StartCheckUpdateScheduleTimer();
                        SW_SetDelaySWUpdateInfoPackage();
                        _SWUpdatePlugin.CallPopup += CallPopup;
                    }
                }
            });
        }

        private void GetCurrentDTPProxyPluginCondition()
        {
            _ = Task.Run(async () =>
            {
                var pluginCondition = await (_DTPProxyPlugin as IFrameworkPluginConditionNotification)?.CurrentConditionAsync();
                lock (_PluginConditionLock_DTPProxy)
                {
                    if (pluginCondition is PluginErrorCondition)
                    {
                        writelog($"{nameof(GetCurrentDTPProxyPluginCondition)} - DTPProxy Plugin is in an error condition");
                    }
                    else if (pluginCondition is PluginRunningCondition)
                    {
                        writelog($"{nameof(GetCurrentDTPProxyPluginCondition)} - DTPProxy Plugin is in a running condition");
                    }
                    else if (pluginCondition is PluginStartedCondition)
                    {
                        writelog($"{nameof(GetCurrentDTPProxyPluginCondition)} - DTPProxy Plugin is in a started condition");
                    }
                }
            });
        }

        public Task<bool> ReloadHotkeyConfigData()
        {
            try
            {
                if (_SettingsPlugin != null)
                {
                    _hotkeySettings = _SettingsPlugin.ReadHotkeySettings().Result;
                    if (_NKVMPlugin != null)
                    {
                        _NKVMPlugin.ToNKVM_HotkeySettings(_hotkeySettings);
                    }
                    return Task.FromResult(true);
                }
                return Task.FromResult(false);
            }
            catch
            {
                return Task.FromResult(false);
            }
        }

        public Task<bool> SaveHotkeyOptionOnly(HotkeySettings hotkeySettings)
        {
            List<HotkeySettings> settings = ReadHotkeySettings().Result;
            HotkeySettings find = settings.Find(x => x.DeviceInfo.SerialNumber.Equals(hotkeySettings.DeviceInfo.SerialNumber));
            if (find == null)
            {
                //new
                settings.Add(hotkeySettings);
            }
            else
            {
                find.HotkeyOptions = hotkeySettings.HotkeyOptions;
            }
            WriteHotkeySettings(settings);
            ReloadHotkeyConfigData();
            return Task.FromResult(true);
        }

        public Task<bool> SaveHotkeySetting(EDID monitorEdid, HotkeyInfo info)
        {
            var hotkeys = info.Hotkey;
            List<HotkeySettings> saveList = new List<HotkeySettings>();
            List<InputSourceObj> inputSourceList = new List<InputSourceObj>();
            List<HotkeyInfo> hotkeyInfoList = new List<HotkeyInfo>();
            HotkeySettings curHotkey = ReadCurrentHotkey(monitorEdid).Result;
            List<HotkeySettings> allSettings = ReadHotkeySettings().Result;
            if (curHotkey.DeviceInfo == null)
            {
                //new monitor
                hotkeyInfoList.Add(info);
                HotkeySettings hotkeySettings = new HotkeySettings();
                hotkeySettings.HotkeyInfo = hotkeyInfoList;
                hotkeySettings.DeviceInfo = monitorEdid;
                saveList.Add(hotkeySettings);
            }
            else
            {
                //overwrite diff monitor conflict key
                List<string> monitorSnList = new List<string>();
                foreach (HotkeySettings hotkeySetting in allSettings)
                {
                    HotkeyInfo findHotkeyInfoList = hotkeySetting.HotkeyInfo.Find(x => (x.Hotkey.Count == hotkeys.Count) && KeysTostr(x.Hotkey).Equals(KeysTostr(hotkeys)));

                    if (findHotkeyInfoList != null)
                    {
                        monitorSnList.Add(hotkeySetting.DeviceInfo.SerialNumber);
                    }
                }
                int allCount = monitorSnList.Count;
                int distCount = monitorSnList.Distinct().Count();
                if (distCount != 0 && (allCount == distCount))
                {
                    //overwite
                    string overWiteMonitorSn = monitorSnList.SingleOrDefault(x => !x.Equals(monitorEdid.SerialNumber));
                    if (overWiteMonitorSn != null)
                    {
                        HotkeySettings overWitrHotkeysettings = allSettings.SingleOrDefault(x => x.DeviceInfo.SerialNumber.Equals(overWiteMonitorSn));
                        HotkeyInfo overWitehotkeyInfo = overWitrHotkeysettings.HotkeyInfo.SingleOrDefault(x => KeysTostr(x.Hotkey).Equals(KeysTostr(hotkeys)));
                        if (overWitehotkeyInfo != null)
                        {
                            overWitehotkeyInfo.Hotkey = new List<VirtualKey>();
                            saveList.Add(overWitrHotkeysettings);
                        }
                    }
                }

                //overwrite conflict key
                HotkeyInfo conflictKeyinfo = curHotkey.HotkeyInfo.SingleOrDefault(x => KeysTostr(x.Hotkey).Equals(KeysTostr(hotkeys)));
                if (conflictKeyinfo != null)
                {
                    conflictKeyinfo.Hotkey = new List<VirtualKey>();
                }

                //save key
                HotkeyInfo? hotkeyInfo = curHotkey.HotkeyInfo.Find(x => x.Job.Equals(info.Job));
                hotkeys = hotkeys.Count > 0 ? hotkeys : new List<VirtualKey>();

                if (hotkeyInfo != null)
                {
                    hotkeyInfo.Hotkey = info.Hotkey;
                    hotkeyInfo.InputSource = info.InputSource;
                    saveList.Add(curHotkey);
                }
                else
                {
                    if (curHotkey.HotkeyInfo.Count != 0)
                    {
                        curHotkey.HotkeyInfo.Add(info);
                        saveList.Add(curHotkey);
                    }
                    else
                    {
                        hotkeyInfoList.Add(info);
                        HotkeySettings hotkeySettings = new HotkeySettings();
                        hotkeySettings.HotkeyInfo = hotkeyInfoList;
                        hotkeySettings.DeviceInfo = monitorEdid;
                        saveList.Add(hotkeySettings);
                    }
                }
            }

            foreach (HotkeySettings setting in allSettings)
            {
                if (saveList.Any(x => x.DeviceInfo.SerialNumber.Equals(setting.DeviceInfo.SerialNumber))) continue;
                saveList.Add(setting);
            }
            if (WriteHotkeySettings(saveList).Result && _NKVMPlugin != null)
            {
                bool b = _NKVMPlugin.SetHotkey(info).Result;
            }
            ReloadHotkeyConfigData();

            return Task.FromResult(true);
        }

        public Task<bool> Hook()
        {
            bool result = false;
            if (_HotkeyPlugin != null)
            {
                result = _HotkeyPlugin.Hook();
                _HotkeyPlugin.KeyUp += Keyboard_KeyUpProc;
                return Task.FromResult(result);
            }
            return Task.FromResult(result);
        }

        public Task<bool> UnHook()
        {
            bool result = false;
            if (_HotkeyPlugin != null)
            {
                _HotkeyPlugin.KeyUp -= Keyboard_KeyUpProc;
                result = _HotkeyPlugin.Unhook();
                return Task.FromResult(result);
            }
            return Task.FromResult(result);
        }

        private string KeysTostr(List<VirtualKey> keys)
        {
            keys.Sort();
            string rt = string.Join(",", keys);
            return rt;
        }

        public Task<HotkeyWarning> GetHotkeyConflicts(HotkeyInfo hotkeyInfo)
        {
            //single key
            if (hotkeyInfo.Hotkey.Count == 1)
            {
                return Task.FromResult(HotkeyWarning.SingleKey);
            }
            if (_SettingsPlugin != null)
            {
                List<HotkeySettings> hotkeySettingList = _SettingsPlugin.ReadHotkeySettings().Result;
                foreach (HotkeySettings hotkeySetting in hotkeySettingList)
                {
                    List<HotkeyInfo> findHotkeyInfoList = hotkeySetting.HotkeyInfo.Where(x => !x.Job.Equals(hotkeyInfo.Job) && (x.Hotkey.Count == hotkeyInfo.Hotkey.Count)).ToList();

                    if (findHotkeyInfoList != null)
                    {
                        if (findHotkeyInfoList.Any(x => KeysTostr(x.Hotkey).Equals(KeysTostr(hotkeyInfo.Hotkey))))
                        {
                            return Task.FromResult(HotkeyWarning.ConflictInbox);
                        }
                    }
                }
                //diff monitor
                List<string> monitorSnList = new List<string>();
                foreach (HotkeySettings hotkeySetting in hotkeySettingList)
                {
                    HotkeyInfo findHotkeyInfo = hotkeySetting.HotkeyInfo.Find(x => (x.Hotkey.Count == hotkeyInfo.Hotkey.Count) && KeysTostr(x.Hotkey).Equals(KeysTostr(hotkeyInfo.Hotkey)));

                    if (findHotkeyInfo != null)
                    {
                        monitorSnList.Add(hotkeySetting.DeviceInfo.SerialNumber);
                    }
                }
                int allCount = monitorSnList.Count;
                int distCount = monitorSnList.Distinct().Count();
                if (distCount != 0 && (allCount == distCount))
                {
                    return Task.FromResult(HotkeyWarning.ConflictInbox);
                }
                return Task.FromResult(HotkeyWarning.None);
            }
            else
            {
                return Task.FromResult(HotkeyWarning.ConflictInbox);
            }
        }

        private void Keyboard_KeyUpProc(object sender, KeyEventArgs e)
        {
            string strKey = e.KeyCode.ToString().ToUpper();
            Debug.WriteLine($"Keyboard_KeyUpProc ---{strKey}");

            bool _altPressed = _HotkeyPlugin.IsKeyPushedDown(System.Windows.Forms.Keys.Menu);
            bool _ctrlPressed = _HotkeyPlugin.IsKeyPushedDown(System.Windows.Forms.Keys.ControlKey);
            bool _shiftPressed = _HotkeyPlugin.IsKeyPushedDown(System.Windows.Forms.Keys.ShiftKey);

            if (_hotkeySettings != null && _hotkeySettings.Count > 0)
            {
                foreach (var settings in _hotkeySettings)
                {
                    foreach (var hotkeyInfo in settings.HotkeyInfo)
                    {
                        var xx = hotkeyInfo.Hotkey.Any(x => x == VirtualKey.Menu);
                        var b1 = hotkeyInfo.Hotkey.Any(x => (int)x == e.KeyValue);
                        if (hotkeyInfo.Hotkey.Any(x => x == VirtualKey.Control) == _ctrlPressed
                        && hotkeyInfo.Hotkey.Any(x => x == VirtualKey.Menu) == _altPressed
                        && hotkeyInfo.Hotkey.Any(x => x == VirtualKey.Shift) == _shiftPressed
                        && hotkeyInfo.Hotkey.Any(x => (int)x == e.KeyValue))
                        {
                            HotkeyType job = hotkeyInfo.Job;
                            ExecHotkeyJob(settings, job);
                        }
                    }
                }
            }
        }

        private Task<bool> ExecHotkeyJob(HotkeySettings settings, HotkeyType job)
        {
            MonitorInfo monitorInfo = _AllInfoMonitors.Find(x => x.edid.SerialNumber.ToUpper().Equals(settings.DeviceInfo.SerialNumber.ToUpper()));
            if (monitorInfo == null)
            {
                //after PxP etc. operation and immediately trigger hotkey then _AllInfoMonitors could be empty
                return Task.FromResult(false);
            }

            switch (job)
            {
                case HotkeyType.BrightnessReduce:
                    if (IsALSautobrightness(monitorInfo))
                    {
                        HotkeyPopWrap hotkeyPopWrap = new HotkeyPopWrap() { monitorInfo = monitorInfo, hotkeyType = job };
                        HotkeyPopup(hotkeyPopWrap);
                    }
                    else
                    {
                        _hotkeyJobQueue.Enqueue(new JobInfo(monitorInfo, null, Reduce_Brightness_Value));
                    }
                    break;

                case HotkeyType.BrightnessIncrease:
                    if (IsALSautobrightness(monitorInfo))
                    {
                        HotkeyPopWrap hotkeyPopWrap = new HotkeyPopWrap() { monitorInfo = monitorInfo, hotkeyType = job };
                        HotkeyPopup(hotkeyPopWrap);
                    }
                    else
                    {
                        _hotkeyJobQueue.Enqueue(new JobInfo(monitorInfo, null, Increase_Brightness_Value));
                    }
                    break;

                case HotkeyType.ContrastReduce:
                    if (IsALSautobrightness(monitorInfo))
                    {
                        HotkeyPopWrap hotkeyPopWrap = new HotkeyPopWrap() { monitorInfo = monitorInfo, hotkeyType = job };
                        HotkeyPopup(hotkeyPopWrap);
                    }
                    else
                    {
                        _hotkeyJobQueue.Enqueue(new JobInfo(monitorInfo, null, Reduce_Contrast_Value));
                    }
                    break;

                case HotkeyType.ContrastIncrease:
                    if (IsALSautobrightness(monitorInfo))
                    {
                        HotkeyPopWrap hotkeyPopWrap = new HotkeyPopWrap() { monitorInfo = monitorInfo, hotkeyType = job };
                        HotkeyPopup(hotkeyPopWrap);
                    }
                    else
                    {
                        _hotkeyJobQueue.Enqueue(new JobInfo(monitorInfo, null, Increase_Contrast_Value));
                    }
                    break;

                case HotkeyType.LuminanceReduce:
                    _hotkeyJobQueue.Enqueue(new JobInfo(monitorInfo, null, Reduce_Luminance_Value));
                    break;

                case HotkeyType.LuminanceIncrease:
                    _hotkeyJobQueue.Enqueue(new JobInfo(monitorInfo, null, Increase_Luminance_Value));
                    break;

                case HotkeyType.ToggleInputSource:
                    _hotkeyJobQueue.Enqueue(new JobInfo(monitorInfo, null, Toggle_InputSource));
                    break;

                case HotkeyType.FavoriteInputSource:
                    HotkeyInfo hotkeyInfoIs = settings.HotkeyInfo.Where(x => x.Job.Equals(HotkeyType.FavoriteInputSource)).SingleOrDefault();
                    if (hotkeyInfoIs != null && hotkeyInfoIs.InputSource != null)
                    {
                        _hotkeyJobQueue.Enqueue(new JobInfo(monitorInfo, new object[] { hotkeyInfoIs }, Favorite_InputSource));
                    }
                    break;

                case HotkeyType.SwitchInputSource:
                    HotkeyInfo hotkeyInfo = settings.HotkeyInfo.Where(x => x.Job.Equals(HotkeyType.SwitchInputSource)).SingleOrDefault();
                    if (hotkeyInfo != null && hotkeyInfo.InputSource != null)
                    {
                        _hotkeyJobQueue.Enqueue(new JobInfo(monitorInfo, new object[] { hotkeyInfo }, Switch_InputSource));
                    }
                    break;

                case HotkeyType.SwapIputPIPPBP:
                    _hotkeyJobQueue.Enqueue(new JobInfo(monitorInfo, null, Swap_IputPIPPBP));
                    break;

                case HotkeyType.ChangePIPPosition:
                    _hotkeyJobQueue.Enqueue(new JobInfo(monitorInfo, null, Change_PIPPosition));
                    break;

                case HotkeyType.KvmSwitchInputSource:
                    HotkeyInfo kvmhotkeyInfo = settings.HotkeyInfo.Where(x => x.Job.Equals(HotkeyType.KvmSwitchInputSource)).SingleOrDefault();
                    if (kvmhotkeyInfo != null && kvmhotkeyInfo.InputSource != null)
                    {
                        _hotkeyJobQueue.Enqueue(new JobInfo(monitorInfo, new object[] { kvmhotkeyInfo }, Kvm_SwitchInputSource));
                    }
                    break;

                case HotkeyType.KvmSwitchKbMsKey:
                    _hotkeyJobQueue.Enqueue(new JobInfo(monitorInfo, null, Kvm_SwitchKbMsKey));
                    break;

                case HotkeyType.KvmChangePIPPosition:
                    _hotkeyJobQueue.Enqueue(new JobInfo(monitorInfo, null, Kvm_ChangePIPPosition));
                    break;
            }
            return Task.FromResult(true);
        }

        private void Kvm_SwitchInputSource(MonitorInfo monitorInfo, Object[] param)
        {
            HotkeyInfo hotkey = (HotkeyInfo)param[0];
            //mock data
            /* Dictionary<string, InputInfo> inputList = GetInputSourcelist(monitorInfo).Result;
             foreach (var inputInfo in inputList)
             {
                 hotkey.InputSource.Add(new InputSourceObj(inputInfo.Value.InputName));
             }*/
            if (hotkey.InputSource.Count == 0)
            {
                //hotkey.InputSource Count must not 0
                return;
            }
            string crtInput = GetCurrentInputSource(monitorInfo);
            // InputSourceObj switchTo = hotkey.InputSource.FirstOrDefault(x => !x.Name.Equals(crtInput));
            string nextInput = string.Empty;
            List<string> inputsList = hotkey.InputSource.OrderBy(x => x.Name).Select(input => input.Name).ToList();
            for (int i = 0; i < inputsList.Count; i++)
            {
                if (inputsList[i].Equals(crtInput))
                {
                    if (i < (inputsList.Count - 1))
                    {
                        nextInput = inputsList[i + 1];
                    }
                    else
                    {
                        nextInput = inputsList[0];
                    }
                }
            }
            if (!string.IsNullOrEmpty(nextInput))
            {
                bool setNextInput = SetVCPCapability(monitorInfo, "Input Select", nextInput).Result;
                writelog($"Kvm_SwitchInputSource:[{monitorInfo.edid.ModelName}:{monitorInfo.edid.SerialNumber}] from [{crtInput}] to [{nextInput}]" + (setNextInput ? "success" : "fail"));
            }
        }

        private void Kvm_SwitchKbMsKey(MonitorInfo monitorInfo, Object[] param)
        {
            bool usbSwitch = UsbSwitch1(monitorInfo).Result;
            writelog($"Kvm_SwitchKbMsKey::[{monitorInfo.edid.ModelName}:{monitorInfo.edid.SerialNumber}]" + (usbSwitch ? "success" : "fail"));
        }

        private void Kvm_ChangePIPPosition(MonitorInfo monitorInfo, Object[] param)
        {
            Change_PIPPosition(monitorInfo, param);
        }

        private void Change_PIPPosition(MonitorInfo monitorInfo, Object[] param)
        {
            ObjGetVCP pxpMode = GetPxpMode(monitorInfo).Result;
            if (pxpMode != null && pxpMode.result == true && !IsPIPMode((UInt32)pxpMode.value))
            {
                //not in pip mode
                ushort[] pxpCap = GetPipPbpCapabilitiesWords(monitorInfo).Result;
                if (pxpCap == null)
                {
                    return;
                }
                else
                {
                    foreach (UInt16 mode in pxpCap)
                    {
                        PxpModeObj? obj = Array.Find(PxpModeObj.Table, x => x.ModeCode == mode && x.Arg.ToLower().Contains("pip"));
                        if (obj != null)
                        {
                            bool setPxp = SetPbpMode(monitorInfo, (UInt16)obj.ModeCode).Result;
                        }
                    }
                }
            }
            else
            {
                bool changePip = TogglePipPosition(monitorInfo).Result;
            }
        }

        private bool IsPIPMode(UInt32 value)
        {
            PxpModeObj? obj = Array.Find(PxpModeObj.Table, x => x.ModeCode == value);
            if (obj != null)
            {
                return obj.Arg.ToLower().Contains("pip");
            }
            return false;
        }

        private void Swap_IputPIPPBP(MonitorInfo monitorInfo, Object[] param)
        {
            ObjGetVCP pxpMode = GetPxpMode(monitorInfo).Result;
            if (pxpMode != null && pxpMode.result == true && (UInt32)pxpMode.value == 0)
            {
                //pxp off
                return;
            }
            //0 = main, 1 = sub1, 2 = sub2, 3 = sub3
            Dictionary<string, InputInfo> inputList = GetInputSourcelist(monitorInfo).Result;
            //pip/pbp subinput should only one
            List<InputSourceObj> subInputs = GetSubInputs(monitorInfo).Result;
            List<InputSourceObj> allInputs = new List<InputSourceObj>();
            inputList.ForEach(input => allInputs.Add(new InputSourceObj(input.Value.InputName)));
            List<int> swapList = subInputs.Select(tmp => allInputs.IndexOf(allInputs.First(x => x.Name.Equals(tmp.Name) && x.Code.Equals(tmp.Code)))).ToList();
            if (swapList.Count != 1 && swapList.Any(x => x.Equals(-1)))
            {
                return;
            }
            bool swapPxp = VideoSwap(monitorInfo, (UInt16)0, (UInt16)swapList[0]).Result;
            writelog($"Swap_IputPIPPBP:[{monitorInfo.edid.ModelName}:{monitorInfo.edid.SerialNumber}] from [0] to [{(UInt16)swapList[0]}]" + (swapPxp ? "success" : "fail"));
            /*if (subInputs != null && subInputs.Count > 0)
            {
                allInputs.AddRange(subInputs);
            }
            //main inputsource :0
            KeyValuePair<string, InputInfo> keyValuePair = result.Where(x => x.Key.Equals(monitorInfo.inputSource)).SingleOrDefault();
            if (keyValuePair.Value != null)
            {
                allInputs.Insert(0, new InputSourceObj(keyValuePair.Value.InputName));
            }
            int x = -1;
            int y = -1;
            List<int> swapList = hotkey.InputSource.Select(tmp =>allInputs.IndexOf( allInputs.First(x => x.Name.Equals(tmp.Name) && x.Code.Equals(tmp.Code)))).ToList();
            if (swapList.Count != 2 && swapList.Any(x=>x.Equals(-1)))
            {
                return;
            }
            x = swapList[0];
            y = swapList[1];
            bool swap= VideoSwap(monitorInfo, (UInt16)x, (UInt16)y).Result;*/
        }

        private void Switch_InputSource(MonitorInfo monitorInfo, Object[] param)
        {
            HotkeyInfo hotkey = (HotkeyInfo)param[0];
            if (hotkey.InputSource.Count == 0)
            {
                //hotkey.InputSource Count must not 0
                return;
            }
            string crtInput = GetCurrentInputSource(monitorInfo);
            InputSourceObj switchTo = hotkey.InputSource.FirstOrDefault(x => !x.Name.Equals(crtInput));
            if (switchTo != null)
            {
                bool setInput = SetVCPCapability(monitorInfo, "Input Select", switchTo.Name).Result;
                writelog($"Switch_InputSource:[{monitorInfo.edid.ModelName}:{monitorInfo.edid.SerialNumber}] from [{crtInput}] to [{switchTo.Name}]" + (setInput ? "success" : "fail"));
            }
        }

        private void Favorite_InputSource(MonitorInfo monitorInfo, Object[] param)
        {
            HotkeyInfo hotkey = (HotkeyInfo)param[0];
            InputSourceObj changeInput = hotkey.InputSource[0];
            bool setNextInput = SetVCPCapability(monitorInfo, "Input Select", changeInput.Name).Result;
            writelog($"Favorite_InputSource:[{monitorInfo.edid.ModelName}:{monitorInfo.edid.SerialNumber}] to [{changeInput.Name}]" + (setNextInput ? "success" : "fail"));
        }

        private void Toggle_InputSource(MonitorInfo monitorInfo, Object[] param)
        {
            Dictionary<string, InputInfo> result = GetInputSourcelist(monitorInfo).Result;
            string nextInput = string.Empty;
            //get current main input source
            string crtInput = GetCurrentInputSource(monitorInfo);
            List<KeyValuePair<string, InputInfo>> list = result.OrderBy(x => x.Key).ToList();
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].Key.Equals(crtInput))
                {
                    if (i < (list.Count - 1))
                    {
                        nextInput = list[i + 1].Key;
                    }
                    else
                    {
                        nextInput = list[0].Key;
                    }
                }
            }
            bool setNextInput = SetVCPCapability(monitorInfo, "Input Select", nextInput).Result;
            writelog($"Toggle_InputSource:[{monitorInfo.edid.ModelName}:{monitorInfo.edid.SerialNumber}] from [{crtInput}] to [{nextInput}]" + (setNextInput ? "success" : "fail"));
        }

        private string GetCurrentInputSource(MonitorInfo monitorInfo)
        {
            string crtInput = string.Empty;
            ObjGetVCP obInput = GetVCPCapability(monitorInfo, "Input Select", 0).Result;
            if (obInput.result)
            {
                uint val = (Convert.ToUInt32(obInput.value) & 0XFFFF);
                string valstring = val.ToString("X2");
                int pos = valstring.Length - 2;
                crtInput = valstring.Substring(pos);
                switch (crtInput.ToLower())
                {
                    case "01": crtInput = "VGA-1"; break;
                    case "02": crtInput = "VGA-2"; break;
                    case "03": crtInput = "DVI-1"; break;
                    case "04": crtInput = "DVI-2"; break;
                    case "05": crtInput = "Composite video 1"; break;
                    case "06": crtInput = "Composite video 2"; break;
                    case "07": crtInput = "S-Video-1"; break;
                    case "08": crtInput = "S-Video-2"; break;
                    case "09": crtInput = "Tuner-1"; break;
                    case "0a": crtInput = "Tuner-2"; break;
                    case "0b": crtInput = "Tuner-3"; break;
                    case "0c": crtInput = "Component video (YPrPb/YCrCb) 1"; break;
                    case "0d": crtInput = "Component video (YPrPb/YCrCb) 2"; break;
                    case "0e": crtInput = "Component video (YPrPb/YCrCb) 3"; break;
                    case "0f": crtInput = "DisplayPort-1"; break;
                    case "10": crtInput = "Mini DisplayPort-1"; break;
                    case "11": crtInput = "HDMI-1"; break;
                    case "12": crtInput = "HDMI-2"; break;
                    case "13": crtInput = "DisplayPort-2"; break;
                    case "14": crtInput = "Mini DisplayPort-2"; break;
                    case "15": crtInput = "HDMI3"; break;
                    case "16": crtInput = "HDMI4"; break;
                    case "17": crtInput = "DisplayPort-3"; break;
                    case "18": crtInput = "Mini DisplayPort-3"; break;
                    case "19": crtInput = "Thunderbolt-1"; break;
                    case "1a": crtInput = "Thunderbolt-2"; break;
                    case "1b": crtInput = "USB-C1"; break;
                    case "1c": crtInput = "USB-C2"; break;
                    case "1d": crtInput = "USB-C3"; break;
                    case "1e": crtInput = "USB-C4"; break;
                    case "80": crtInput = "USB Comm from USB1 (Type-B, port 1)"; break;
                    case "81": crtInput = "USB Comm from USB2 (Type-B, port 2)"; break;
                    case "82": crtInput = "USB Comm from USB-C1 (Type-C, port 1)"; break;
                    case "83": crtInput = "USB Comm from USB-C2 (Type-C, port 2)"; break;
                    case "84": crtInput = "USB Comm from USB-C3 (Type-C, port 3)"; break;
                    case "85": crtInput = "USB Comm from USB-C4 (Type-C, port 4)"; break;
                    default: crtInput = string.Empty; break;
                }
                return crtInput;
            }
            return string.Empty;
        }

        private bool IsALSautobrightness(MonitorInfo monitorInfo)
        {
            List<ALSConfig> aLSConfigs = GetAllExistAlsConfig().Result;
            ALSConfig find = aLSConfigs.Find(x => x.serialNumber.Equals(monitorInfo.edid.SerialNumber) && x.isAutoBrightness);
            return find != null;
        }

        private void HotkeyPopup(object o)
        {
            Task.Run(() =>
            {
                PopupBaseManage popupBaseManage = new PopupBaseManage();
                popupBaseManage.LeftButtonClick += YesEvent;
                popupBaseManage.RightButtonClick += NoEvent;
                string title = @"Warning";
                string info = @"Auto Brightness is currently enabled.Do you wish to override it?";
                popupBaseManage.FWU_Show(title, info, "Yes", "No", o, true, -1);
            });
        }

        private void YesEvent(object o, object ob)
        {
            //Auto Brightness OFF & Auto OFF & Manual ON?
            HotkeyPopWrap hotkeyPopWrap = (HotkeyPopWrap)ob;
            List<ALSConfig> aLSConfigs = GetAllExistAlsConfig().Result;
            ALSConfig find = aLSConfigs.Find(x => x.serialNumber.Equals(hotkeyPopWrap.monitorInfo.edid.SerialNumber));
            //diable autobrightness
            SetALSFeatureValue(hotkeyPopWrap.monitorInfo, find, ALSFeatureQueryType.AutoBrightness, "");
            switch (hotkeyPopWrap.hotkeyType)
            {
                case HotkeyType.BrightnessReduce:
                    _hotkeyJobQueue.Enqueue(new JobInfo(hotkeyPopWrap.monitorInfo, null, Reduce_Brightness_Value));
                    break;

                case HotkeyType.BrightnessIncrease:
                    _hotkeyJobQueue.Enqueue(new JobInfo(hotkeyPopWrap.monitorInfo, null, Increase_Brightness_Value));
                    break;

                case HotkeyType.ContrastReduce:
                    _hotkeyJobQueue.Enqueue(new JobInfo(hotkeyPopWrap.monitorInfo, null, Reduce_Contrast_Value));
                    break;

                case HotkeyType.ContrastIncrease:
                    _hotkeyJobQueue.Enqueue(new JobInfo(hotkeyPopWrap.monitorInfo, null, Increase_Contrast_Value));
                    break;
            }
        }

        private void NoEvent(object o, object ob)
        {
            //do nothing
        }

        private void Reduce_Brightness_Value(MonitorInfo monitorInfo, Object[] param)
        {
            ObjGetVCP obBrightness = GetVCPCapability(monitorInfo, 0x10, 0).Result;
            if (obBrightness.result)
            {
                uint brightnessValue = ((uint)obBrightness.value) <= 1 ? 0 : (uint)obBrightness.value - 1;
                SetVCPCapability(monitorInfo, 0x10, brightnessValue);
            }
        }

        private void Increase_Brightness_Value(MonitorInfo monitorInfo, Object[] param)
        {
            ObjGetVCP obBrightness = GetVCPCapability(monitorInfo, 0x10, 0).Result;
            if (obBrightness.result)
            {
                uint brightnessValue = ((uint)obBrightness.value) + 1 >= 100 ? 100 : (uint)obBrightness.value + 1;
                SetVCPCapability(monitorInfo, 0x10, brightnessValue);
            }
        }

        private void Reduce_Contrast_Value(MonitorInfo monitorInfo, Object[] param)
        {
            ObjGetVCP obContrast = GetVCPCapability(monitorInfo, 0x12, 0).Result;
            if (obContrast.result)
            {
                uint brightnessValue = ((uint)obContrast.value) <= 1 ? 0 : (uint)obContrast.value - 1;
                SetVCPCapability(monitorInfo, 0x12, brightnessValue);
            }
        }

        private void Increase_Contrast_Value(MonitorInfo monitorInfo, Object[] param)
        {
            ObjGetVCP obContrast = GetVCPCapability(monitorInfo, 0x12, 0).Result;
            if (obContrast.result)
            {
                uint brightnessValue = ((uint)obContrast.value) + 1 >= 100 ? 100 : (uint)obContrast.value + 1;
                SetVCPCapability(monitorInfo, 0x12, brightnessValue);
            }
        }

        private void Reduce_Luminance_Value(MonitorInfo monitorInfo, Object[] param)
        {
            ObjGetVCP obLuminance = GetVCPCapability(monitorInfo, 0x10, 0).Result;
            if (obLuminance.result)
            {
                uint brightnessValue = ((uint)obLuminance.value) <= 1 ? 0 : (uint)obLuminance.value - 1;
                SetVCPCapability(monitorInfo, 0x10, brightnessValue);
            }
        }

        private void Increase_Luminance_Value(MonitorInfo monitorInfo, Object[] param)
        {
            ObjGetVCP obLuminance = GetVCPCapability(monitorInfo, 0x10, 0).Result;
            ObjGetVCP obLuminanceMax = GetVCPCapability(monitorInfo, 0x10, 1).Result;
            if (obLuminance.result && obLuminanceMax.result)
            {
                uint brightnessValue = ((uint)obLuminance.value) + 1 >= (uint)obLuminanceMax.value ? (uint)obLuminanceMax.value : (uint)obLuminance.value + 1;
                SetVCPCapability(monitorInfo, 0x10, brightnessValue);
            }
        }

        private bool _screenSaver = false;

        private void OnPowerNapTimedRaise(object sender, ElapsedEventArgs e)
        {
            bool screenSaverStatus = JobQueue.GetScreensaverCurrentStatus(JobQueue.SPI_GETSCREENSAVERRUNNING);
            if (screenSaverStatus)
            {
                if (!_screenSaver)
                {
                    List<PowerNapSetting> read = ReadPowerNapSettings().Result;
                    List<JobInfo> allJobs = new List<JobInfo>();
                    foreach (PowerNapSetting setting in read)
                    {
                        MonitorInfo monitorInfo = _AllInfoMonitors.Find(x => x.edid.SerialNumber.Equals(setting.SerialNumber));
                        if (monitorInfo != null)
                        {
                            if (setting.Status)
                            {
                                switch (setting.RunType)
                                {
                                    case PowerNapType.ReduceBrightness:
                                        allJobs.Add(new JobInfo(monitorInfo, new object[] { true }, PowerNapReduceBrightness));
                                        //_powerNapJobQueue.Enqueue(new JobInfo(monitorInfo, new object[] { true }, PowerNapReduceBrightness));
                                        Debug.WriteLine($"{setting.ModelName} ReduceBrightness - Enqueue:true");
                                        writelog($"powerNap [{setting.ModelName}] ReduceBrightness - Enqueue:true");
                                        break;

                                    case PowerNapType.SleepIfRunning:
                                        allJobs.Add(new JobInfo(monitorInfo, new object[] { true }, PowerNapSuspendMonitor));
                                        //_powerNapJobQueue.Enqueue(new JobInfo(monitorInfo, new object[] { true }, PowerNapSuspendMonitor));
                                        Debug.WriteLine($"{setting.ModelName} SleepIfRunning - Enqueue:true");
                                        writelog($"powerNap [{setting.ModelName}] SleepIfRunning - Enqueue:true");
                                        break;

                                    case PowerNapType.Off:
                                        break;
                                }
                            }
                        }
                    }
                    List<JobInfo> jobInfos = allJobs.Distinct().ToList();
                    foreach (JobInfo jobInfo in jobInfos)
                    {
                        _powerNapJobQueue.Enqueue(jobInfo);
                    }
                    _screenSaver = true;
                }
            }
            else
            {
                if (_screenSaver)
                {
                    List<PowerNapSetting> read = ReadPowerNapSettings().Result;
                    List<JobInfo> allJobs = new List<JobInfo>();
                    foreach (PowerNapSetting setting in read)
                    {
                        MonitorInfo monitorInfo = _AllInfoMonitors.Find(x => x.edid.SerialNumber.Equals(setting.SerialNumber));
                        if (monitorInfo != null)
                        {
                            if (setting.Status)
                            {
                                switch (setting.RunType)
                                {
                                    case PowerNapType.ReduceBrightness:
                                        allJobs.Add(new JobInfo(monitorInfo, new object[] { false }, PowerNapReduceBrightness));
                                        //_powerNapJobQueue.Enqueue(new JobInfo(monitorInfo, new object[] { false }, PowerNapReduceBrightness));
                                        Debug.WriteLine($"{setting.ModelName} ReduceBrightness - Enqueue:false");
                                        writelog($"powerNap [{setting.ModelName}] ReduceBrightness - Enqueue:false");
                                        break;

                                    case PowerNapType.SleepIfRunning:
                                        allJobs.Add(new JobInfo(monitorInfo, new object[] { false }, PowerNapSuspendMonitor));
                                        //_powerNapJobQueue.Enqueue(new JobInfo(monitorInfo, new object[] { false }, PowerNapSuspendMonitor));
                                        Debug.WriteLine($"{setting.ModelName} SleepIfRunning - Enqueue:false");
                                        writelog($"powerNap [{setting.ModelName}] SleepIfRunning - Enqueue:false");
                                        break;

                                    case PowerNapType.Off:
                                        break;
                                }
                            }
                        }
                    }
                    List<JobInfo> jobInfos = allJobs.Distinct().ToList();
                    foreach (JobInfo jobInfo in jobInfos)
                    {
                        _powerNapJobQueue.Enqueue(jobInfo);
                    }
                    _screenSaver = false;
                }
            }
        }

        public void PowerNapReduceBrightness(MonitorInfo monitorInfo, Object[] param)
        {
            // ReduceBrightness
            //SetVCPCapability(monitorInfo, 0xE0, 1);
            bool cs = (bool)param[0];
            if (cs)
            {
                SetVCPCapability(monitorInfo, 0xE0, 1);
            }
            else
            {
                SetVCPCapability(monitorInfo, 0xE0, 0);
            }
        }

        public void PowerNapSuspendMonitor(MonitorInfo monitorInfo, Object[] param)
        {
            //SuspendMonitor
            //SetVCPCapability(monitorInfo, 0xE1, 1);
            bool cs = (bool)param[0];
            if (cs)
            {
                SetVCPCapability(monitorInfo, 0xE1, 1);
            }
            else
            {
                SetVCPCapability(monitorInfo, 0xE1, 0);
            }
        }

        public Task<bool> SavePowerNapSetting(PowerNapSetting powerNapSetting)
        {
            List<PowerNapSetting> saveList = new List<PowerNapSetting>();
            List<PowerNapSetting> allSettings = ReadPowerNapSettings().Result;
            allSettings.RemoveAll(x => x.SerialNumber == null);
            saveList.Add(powerNapSetting);
            foreach (PowerNapSetting setting in allSettings)
            {
                if (saveList.Any(x => x.SerialNumber.Equals(setting.SerialNumber))) continue;
                saveList.Add(setting);
            }
            WritePowerNapSettings(saveList);

            if (powerNapSetting.RunType == PowerNapType.Off)
            {
                //only save btn status
                return Task.FromResult(true);
            }

            //reset powerNaptimer and status
            _PowerNapTimer.Stop();
            _powerNapJobQueue.Clear();
            _screenSaver = false;
            _PowerNapTimer.Start();
            return Task.FromResult(true);
        }

        public Task<List<PowerNapSetting>> ReadPowerNapSettings()
        {
            List<PowerNapSetting> read = _SettingsPlugin.ReadPowerNapSettings().Result;
            read.RemoveAll(x => x.SerialNumber == null);
            return Task.FromResult(read);
        }

        #region InputSource

        /// <summary>
        /// InputSourceList Serialize
        /// </summary>
        /// <param name="inputlist"></param>
        /// <returns>
        /// InputSourceListSerialize string
        /// </returns>
        private string InputSourceListSerialize(Dictionary<string, InputInfo> inputlist)
        {
            string strInputList;
            strInputList = JsonConvert.SerializeObject(inputlist, Formatting.Indented);
            return strInputList;
        }

        /// <summary>
        /// InputSourceList Deserialize
        /// </summary>
        /// <param name="strinputlist"></param>
        /// <returns>
        /// inputlist dictionary
        /// </returns>
        private Dictionary<string, InputInfo> InputSourceListDeserialize(string strinputlist)
        {
            Dictionary<string, InputInfo> inputlist = new Dictionary<string, InputInfo>();
            inputlist = JsonConvert.DeserializeObject<Dictionary<string, InputInfo>>(strinputlist);
            return inputlist;
        }

        #endregion

        public Task<string> GetAppIconFolderPath()
        {
            string iconFolder = _SettingsPlugin.GetAppIconFolderPath().Result;

            return Task.FromResult(iconFolder);
        }

        public Task<List<ColorPresetSettings>> ReadColorPresetSettings()
        {
            List<ColorPresetSettings> read = _SettingsPlugin.ReadColorPresetSettings().Result;

            return Task.FromResult(read);
        }

        public Task<bool> WriteColorPresetSettings(List<ColorPresetSettings> colorPresetSettings)
        {
            bool r = false;

            //if (r)
            //{
            //data process
            var tmp = colorPresetSettings;
            //write back to settings
            r = _SettingsPlugin.WriteColorPresetSettings(tmp).Result;
            Thread.Sleep(100);
            //}
            return Task.FromResult(r);
        }

        public Task<List<HotkeySettings>> ReadHotkeySettings()
        {
            List<HotkeySettings> read = _SettingsPlugin.ReadHotkeySettings().Result;
            ToNKVM_HotKeys(read);

            return Task.FromResult(read);
        }

        public Task<bool> WriteHotkeySettings(List<HotkeySettings> hotkeySettings)
        {
            bool r = false;

            //if (r)
            //{
            //data process
            var tmp = hotkeySettings;
            //write back to settings
            r = _SettingsPlugin.WriteHotkeySettings(tmp).Result;
            Thread.Sleep(100);
            if (r)
            {
                ToNKVM_HotKeys(tmp);
            }
            //}
            return Task.FromResult(r);
        }

        public Task<HotkeySettings> ReadCurrentHotkey(EDID monitorEdid)
        {
            List<HotkeySettings> read = _SettingsPlugin.ReadHotkeySettings().Result;
            HotkeySettings hotkeySettings = read.Where(x => x.DeviceInfo.ModelName.Equals(monitorEdid.ModelName) && x.DeviceInfo.SerialNumber.Equals(monitorEdid.SerialNumber)).SingleOrDefault();

            if (hotkeySettings != null && hotkeySettings.HotkeyInfo.Count > 0)
            {
                return Task.FromResult(hotkeySettings);
            }
            return Task.FromResult(new HotkeySettings());
        }

        public Task<bool> WritePowerNapSettings(List<PowerNapSetting> powerNapSettings)
        {
            bool r = false;

            //if (r)
            //{
            //data process
            var tmp = powerNapSettings;
            //write back to settings
            r = _SettingsPlugin.WritePowerNapSettings(tmp).Result;
            Thread.Sleep(100);
            //}
            return Task.FromResult(r);
        }

        #region USBKVM

        private string USBKVMPCsListSerialize(Dictionary<string, PCsInfo> pcslist)
        {
            string strpcsList;
            strpcsList = JsonConvert.SerializeObject(pcslist, Formatting.Indented);
            return strpcsList;
        }

        private Dictionary<string, PCsInfo> USBKVMPCsListDeserialize(string strpcslist)
        {
            Dictionary<string, PCsInfo> pcslist = new Dictionary<string, PCsInfo>();
            pcslist = JsonConvert.DeserializeObject<Dictionary<string, PCsInfo>>(strpcslist);
            return pcslist;
        }

        #endregion

        private List<MonitorInfo> RemoveDuplicatesByDisplayName(List<MonitorInfo> mos)
        {
            if (mos == null) return null;
            if (mos.Count == 0) return mos;

            return mos.GroupBy(p => new { p.DisplayName, p.edid.SerialNumber }).Select(g => g.First()).ToList();
        }

        private void ReviewAllMonitorToAvoidDuplicatedInfo()
        {
            lock (_MoLock)//this) //Dean 0626 fix SAST issue, do not lock over this object
            {
                try
                {
                    if (_AllInfoMonitors == null || _AllInfoMonitors.Count == 0)//Dean 0626 fix SAST issue
                    {
                        writelog("[ReviewAllMonitorToAvoidDuplicatedInfo] null or no monitor info exist");
                        return;
                    }

                    List<MonitorInfo> _HadleMonitors = _AllInfoMonitors;
                    int nCount = _HadleMonitors.Count;

                    for (int n = 0; n < nCount; n++)
                    {
                        writelog($"[Original] Monitor: {_HadleMonitors[n].DisplayName}, SN: {_HadleMonitors[n].edid.SerialNumber}");
                    }

                    List<MonitorInfo> distinctMonitor = RemoveDuplicatesByDisplayName(_HadleMonitors);
                    nCount = distinctMonitor.Count;
                    for (int n = 0; n < nCount; n++)
                    {
                        writelog($"[Reviewed] Monitor: {distinctMonitor[n].DisplayName}, SN: {distinctMonitor[n].edid.SerialNumber}");
                    }

                    _AllInfoMonitors = distinctMonitor;
                }
                catch (Exception ex)
                {
                    writelog($"[ReviewAllMonitorToAvoidDuplicatedInfo] Exception: {ex.Message}");
                }
                finally
                {
                    //return _AllInfoMonitors;
                }
            }
        }

        #region NKVM

        private void ToNKVM_SupportedMonitorList()
        {
            if (_SettingsPlugin != null && _NKVMPlugin != null)
            {
                DDPMSettings config = _SettingsPlugin.ReloadAppConfigData().Result;
                if (config != null)
                {
                    _SupportedMonitorList = config.UserSettings.SupportedMonitorList;
                    _NKVMPlugin.ToNKVM_SupportedMonitorList(_SupportedMonitorList);
                }
            }
        }

        private void ToNKVM_HotKeys(List<HotkeySettings> hotkeySettings)
        {
            if (_NKVMPlugin != null)
            {
                _NKVMPlugin.ToNKVM_HotkeySettings(hotkeySettings).Wait();
            }
        }

        private void ToNKVM_initHotKeys()
        {
            List<HotkeySettings> read = new List<HotkeySettings>();
            if (_SettingsPlugin != null && _NKVMPlugin != null)
            {
                read = ReadHotkeySettings().Result;
            }
        }

        #endregion

        #region Settings
        private List<VCP> GetAllVCPcode(MonitorInfo monitorInfo)
        {
            List<VCP> vcps = new List<VCP>();
            foreach (string key in monitorInfo.CapabilityDic.Keys)
            {
                VCP vcp = new VCP(Int32.Parse(key, System.Globalization.NumberStyles.HexNumber), null);
                vcps.Add(vcp);
            }
            return vcps;
        }
        #endregion
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
            writelog($"Dispose: {disposing}");
            if (!IsDisposed)
            {
                if (disposing)
                {
                    _agent.PluginManager.PluginsStarted -= PluginManagerOnPluginsStarted;
                    _agent = null;

                    //Bruce 08 - 09 Add a new event to determine whether it is a display signal event or a setting event.
                    Microsoft.Win32.SystemEvents.DisplaySettingsChanged -= SystemEvents_DisplaySettingsChanged;
                    //displayChange.DisplayChange_Event -= SystemEvents_DisplaySettingsChanged;
                    if(_SettingsPlugin != null)
                        _SettingsPlugin.ITSettingsActionEvent -= _SettingsPlugin_ITSettingsActionEvent;
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

        private void OnScheduleManagerPluginConditionChangeHandler(object sender, EventArgs e)
        {
            GetCurrentScheduleManagerCondition();
        }

        private void OnColorPresetPluginConditionChangeHandler(object sender, EventArgs e)
        {
            GetCurrentColorPresetCondition();
        }

        private void OnPeripheralsPluginConditionChangeHandler(object sender, EventArgs e)
        {
            GetCurrentPeripheralsPluginCondition();
        }

        private void OnSettingsPluginConditionChangeHandler(object sender, EventArgs e)
        {
            GetCurrentSettingsPluginCondition();
        }

        private void OnFWUpdatePluginConditionChangeHandler(object sender, EventArgs e)
        {
            GetCurrentFWUpdatePluginCondition();
        }

        private void OnNKVMPluginConditionChangeHandler(object sender, EventArgs e)
        {
            GetCurrentNKVMPluginCondition();
        }

        private void OnHotkeyPluginConditionChangeHandler(object sender, EventArgs e)
        {
            GetCurrentHotkeyPluginCondition();
        }

        private void OnSWUpdatePluginConditionChangeHandler(object sender, EventArgs e)
        {
            GetCurrentSWUpdatePluginCondition();
        }

        private void OnDTPProxyPluginConditionChangeHandler(object sender, EventArgs e)
        {
      GetCurrentDTPProxyPluginCondition();
        }

        //Bruce, 2024-08-09 add new event
        private void OnHDRStatusChangeHandler(object sender, bool e)
        {
            HDRChangeEvent?.AsyncFireAndForget(this, e, System.Threading.CancellationToken.None);
        }
        //Bruce, 2024-08-09 add new event
        private void OnGamingParamChangeHandler(object sender, GamingDisplayPropertiesInfo e)
        {
            GamingChangeEvent?.AsyncFireAndForget(this, e, System.Threading.CancellationToken.None);
        }

        private void PluginManagerOnPluginsStarted(object sender, PluginsStartedEventArgs e)
        {
            if (e == null)
                return;
            if (e.ChangedPlugins == null)
                return;
            if (e.ChangedPlugins.Any() == false)
                return;

            if (e.ChangedPlugins.OfType<ISchedulerManager>().Any())
                InitializeSchedulerManagerPlugin();

            if (e.ChangedPlugins.OfType<ISettingsManagerDev>().Any())
                InitializeSettingsPlugin();

            if (e.ChangedPlugins.OfType<IDisplayService>().Any())
                InitializeDisplayManagerPlugin();

            if (e.ChangedPlugins.OfType<IColorPresetSA>().Any())
                InitializeColorPresetPlugin();

            if (e.ChangedPlugins.OfType<IDPeMPlugin>().Any())
                InitializePeripheralsPlugin();

            if (e.ChangedPlugins.OfType<IFWUpdateService>().Any())
                InitializeFWUpdatePlugin();

            if (e.ChangedPlugins.OfType<ISWUpdateService>().Any())
                InitializeSWUpdatePlugin();

      if(e.ChangedPlugins.OfType<IDTPProxyPlugin>().Any())
        InitializeDTPProxyPlugin();
    }

    #endregion
  }
}