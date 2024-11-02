#region LicenceHeader

//
// Copyright © 2024, Dell Inc., All Rights Reserved.
// This material is confidential and a trade secret.  Permission to use this
// work for any purpose must be obtained in writing from Dell Inc.
//
// DisplayMangerPlugin.cs created on 25/04/2024T07:20 PM
//

#endregion

using DdmLibrary.Utility;
using DDPM.MonitorBorker;
using DDPM.OSDs;
using DDPM.PowerMon;
using DDPM.QAM;
using DDPM.SA.Common;
using DDPM.SA.Common.Display;
using DDPM.SA.Common.Popup;
using DDPM.SA.Common.Screen;
using DDPM.SA.Common.Settings;
using DDPM.SA.Common.Telemetry;
using DDPM.SA.Common.UpdateProgressPage;
using DDPM.ShowOSD;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.Common.Annotations;
using Dell.Client.Framework.Common.Extensions;
using Dell.Client.Framework.Common.PluginConditions;
using Dell.Client.Framework.Interfaces;
using Dell.Client.Framework.UX.WPF.Controls;
using DPeMPublic.Common.Enums;
using Microsoft;
using Microsoft.Toolkit.Uwp.Notifications;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Runtime;
using System.Security.Policy;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Threading;
using VcpCore.Common;
using Windows.System;
using static DDPM.SA.Common.Telementry_GeneralFunction;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;
using static VcpCore.Common.User32;
using IDs = DDPM.SA.Common.IDs;
using System.Runtime;
//using MonitorProfile = DDPM.SA.Common.MonitorProfile;
using Point = System.Windows.Point;
using static DDPM.SA.Plugins.User.DeviceManager.DisplayDeviceHelper;
using System.Windows.Resources;

namespace DDPM.SA.Plugins.User.DeviceManager
{
    [Plugin(IDs.Device_Manager_Plugin_ID, pluginName, PluginOrderGroupType.Core, Version = pluginVersion)]
    [Descriptor(Description = pluginDescription)]
    [Publisher(Name = publisherCompany, Website = publisherWebsite, Support = publisherSupport)]
    [PublishedUnelevatedInterface(new[] { typeof(IDeviceManagerSA) })]
    [PluginRequires(Id = IDs.Telementry_Scheduler_Plugin_ID, AllowDynamicResolving = true)]
    [PluginRequires(Id = IDs.Display_Manager_PLUGIN_ID, AllowDynamicResolving = true)]
    [PluginRequires(Id = IDs.Scheduler_Manager_Plugin_ID, AllowDynamicResolving = true)]
    [PluginRequires(Id = IDs.DDPM_PERIPHERALS_PLUGIN_ID, AllowDynamicResolving = true)]
    [PluginRequires(Id = IDs.DDPM_SETTINGSMANAGER_SA_PLUGIN_ID, AllowDynamicResolving = true)]
    [PluginRequires(Id = IDs.CLI_Manager_Plugin, AllowDynamicResolving = true)]
    [PluginRequires(Id = IDs.DDPM_EMPlugin_PLUGIN_ID, AllowDynamicResolving = true)]
    //[DependencyKnownTypes(new[] { typeof(IDisplayService), typeof(ISchedulerManager), typeof(IDPeMPlugin), typeof(ISettingsManagerDev), typeof(IFWUpdateService), typeof(ISWUpdateService), typeof(IEzMemoryPlugin) })]
    [DependencyKnownTypes(new[] { typeof(ITelementryScheduler), typeof(IDisplayService), typeof(ISchedulerManager), typeof(IDPeMPlugin), typeof(ISettingsManagerDev), typeof(IFWUpdateService), typeof(ISWUpdateService) })]
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
        private IEzMemoryPlugin _IEzMemoryPlugin;
        private ITelementryScheduler _TelementryScheduler;

        private readonly object _FwUpdateLock = new object();
        private readonly object _DisplayChangedLock = new object();
        private readonly object _PluginConditionLock = new object();
        private readonly object _PluginConditionLock_Display = new object();
        private readonly object _PluginConditionLock_Peripherals = new object();
        private readonly object _PluginConditionLock_Settings = new object();
        private readonly object _PluginConditionLock_ColorPreset = new object();
        private readonly object _PluginConditionLock_NKVM = new object();
        private readonly object _PluginConditionLock_Hotkey = new object();
        private readonly object _PluginConditionLock_ScheduleManager = new object();
        private readonly object _PluginConditionLock_DTPProxy = new object();
        private readonly object _PluginConditionLock_EzMemory = new object();
        private readonly object _PluginConditionLock_TelementryScheduler = new object();

        private DisplayChange displayChange;

        //private static Dell.Client.Framework.Common.Log _log;
        // ColorPreset objects
        private Dictionary<string, InstalledAppInfo> _AllAppData_tmp = new Dictionary<string, InstalledAppInfo>();

        private Dictionary<string, InstalledAppInfo> _AllAppData = new Dictionary<string, InstalledAppInfo>();
        private List<string> _SupportedColorPreset = new List<string>();

        private readonly object _CheckAutoLock = new object();

        // Jim move to here 20240621
        private ShowOSDWin OsdWin = null;

        private string iconFolderPath = string.Empty;

        private MainWindow? MonitorBorkerWin = null; //Dean 0626 fix SAST issue, remove static

        private Thread newWindowThread_AutoSetColorPresetForMonitorConfig = null;

        /// <summary>
        /// Colorpreset Manual change event，return Colorpreset name
        /// </summary>
        public event EventHandler<string> Coloreset_manual_ChangeEvent;

        /// <summary>
        /// NightLight Status change event，return On or Off
        /// </summary>
        public event EventHandler<string> NightLightStatus_ChangeEvent;

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
        private List<HotkeySettings> _hotkeySettings = null;// = new List<HotkeySettings>();

        private JobQueue _hotkeyJobQueue = new JobQueue();

        private static MonitorInfo lastSelectedMonitor_UI = null;

        //powerNap
        private JobQueue _powerNapJobQueue = new JobQueue();

        private static System.Timers.Timer _PowerNapTimer = new System.Timers.Timer(2000);

        //FW update progress bar
        private UpdateProgress _UpdateProgress;

        //Bruce 07-30 Added total screens
        private int _lastScreenCount;

        //private enum log_type
        //{
        //    info = 0,
        //    error
        //}

        private readonly object _MoLock = new object();

        //Bruce 0815 Added new judgment whether to trigger DisplayChang event
        private bool displayInOut = true;

        private GlobalSettingParam _GlobalSettingParam = new GlobalSettingParam();

        private bool isInitMonitorSettings = false;
        private static bool _IsSkipCA = false;

        private QAMPage _QAM;
        private Point QAM_Position;

        private static CancellationTokenSource _ReGetcancellationTokenSource;

        private static bool _isSubagentActive = true;
        private bool userClosedPopup = false;

        private static PowerEventControl _pwr_Mon = null;
        private static DisplayDeviceHelper _disDevHelper = null;
        private static int _millisecond = 8000;
        private static OSD_Controler _OSD_Controler = new OSD_Controler();


        private OSThemeEnum previousOsTheme = OSThemeEnum.Dark;
        #endregion

        #region Constructor

        public DeviceMangerPlugin(IAgent agent) : base(agent, PluginLogId)
        {
            _agent = agent;
            _PowerNapTimer.Elapsed += OnPowerNapTimedRaise;
            _PowerNapTimer.AutoReset = true;
            _PowerNapTimer.Enabled = true;
            UXSystemParameters.Instance.ParameterChangedEvent += UXSystemParametersChanged;
            writelog("DeviceManagerPlugin constructor ...");

            _isSubagentActive = WTSFunction.IsYourProcessInActiveSession(Log);
        }

        private void UXSystemParametersChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(UXSystemParameters.Instance.OSTheme))
            {
                OSThemeEnum oSTheme = UXSystemParameters.Instance.OSTheme;
                if (previousOsTheme == oSTheme) return;
                //telemetry [Application Settings ==>AppMode : "Dark","Light"]
                Debug.WriteLine($"UXSystemParametersChanged:current theme= {oSTheme.ToString()}");
                //Telementry Collection
                //var rt = false;
                var applicationSettings_Function = new ApplicationSettings_Function();
                string appModeTelementryData = string.Empty;
                switch (oSTheme)
                {
                    case OSThemeEnum.Light:
                        appModeTelementryData = "Light";
                        break;
                    case OSThemeEnum.Dark:
                        appModeTelementryData = "Dark";
                        break;
                    default:
                        break;
                }
                Debug.WriteLine($"AppModeTelemetry=> {appModeTelementryData}");
                writelog("[DeviceMangerPlugin] [Telementry] Send Telementry for AppMode...");
                Task.Run(() => applicationSettings_Function.Send_AppMode_Telementry(_TelementryScheduler, _AllInfoMonitors, appModeTelementryData)).ConfigureAwait(false);
                /* if (rt) writelog("[DeviceMangerPlugin] [Telementry] Send Telementry for AppMode Success ...");
                 else writelog("[DeviceMangerPlugin] [Telementry] Send Telementry for AppMode Fail ...");*/
                previousOsTheme = oSTheme;
            }
        }

        #endregion

        #region Overriding methods

        protected override void OnPluginStarting()
        {
            _agent.PluginManager.PluginsStarted += PluginManagerOnPluginsStarted;

            InitializeSettingsPlugin();
            InitializeDisplayManagerPlugin();
            InitializeTelementrySchedulerPlugin();
            InitializeColorPresetPlugin();
            InitializePeripheralsPlugin();
            InitializeFWUpdatePlugin();
            InitializeNKVMPlugin();
            InitializeHotkeyPlugin();
            InitializeSWUpdatePlugin();
            InitializeSchedulerManagerPlugin();
            InitializeDTPProxyPlugin();
            InitializeEzMemoryPlugin();

            PluginCondition = new PluginStartedCondition();
            writelog("DeviceManager plugin started");

            Microsoft.Win32.SystemEvents.DisplaySettingsChanged += SystemEvents_DisplaySettingsChanged;
            ToastNotificationManagerCompat.OnActivated += toastArgs =>
            {
                CheckInput(toastArgs);
            };

            //displayChange = new DisplayChange(Log);
            //Task.Run(() =>
            //{
            //    displayChange.Initialize_DisplayChangeEvent();
            //});
            //displayChange.DisplayChange_Event += SystemEvents_DisplaySettingsChanged;

            Thread thread = new Thread(() =>
            {
                if (_pwr_Mon == null)
                {
                    _pwr_Mon = new PowerEventControl(Log);
                    _pwr_Mon.MonitorTurnedOn += MonitorEvent_On;
                    _pwr_Mon.Enable_Event();
                }
                System.Windows.Threading.Dispatcher.Run();
            });

            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();

            _disDevHelper = new DisplayDeviceHelper(Log);
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
        public event EventHandler<UpdateProgressInfo> ProgressUpdate_Notify;

        public event EventHandler<bool> FWU_UILock_Notify;

        /// <summary>
        /// FW Update供CLI使用
        /// </summary>
        public event EventHandler<List<FWUpdateInfo>> DownloadAndInstall_Result_Notify;

        //EasyArrange
        //
        //Notify to DDPM.UI when EAPlugin open the EditWindow for editing custom layout
        public event EventHandler<string> EAEditStarted;

        //Robert_Lin, 2024-9-13 Remove unused interfaces
        //Notify to DDPM.UI when EAPlugin has finished the edit custom layout, and sent back the result.
        //public event EventHandler<string> EAEditCompleted;

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

        public event EventHandler<EAArgs> EASettingsChanged;

        //End of EasyArrange
        ///////////////////////

        /// <summary>
        /// HDR status change event，return HDR status
        /// </summary>
        public event EventHandler<bool> HDRChangeEvent;

        /// <summary>
        /// gaming parameter changes event，return gaming parameter
        /// </summary>
        public event EventHandler<GamingDisplayPropertiesInfo> GamingChangeEvent;

        public event EventHandler<NKVMRespone> NKVMCLIRespone;

        public event EventHandler GlobalSettingChangeEvent;

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
                _ICC_Metadata = _ColorPresetPlugin.DownloadICCData(m, _SettingsPlugin, savelPath).Result;
            }

            return Task.FromResult(_ICC_Metadata);
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

                        bool SmartHDR_ON = GetHDRStatus(m).Result;

                        _SupportedColorPreset = _ColorPresetPlugin.ReadColorPreset(m, VCP_capbility, SmartHDR_ON).Result;
                    }
                }
            }

            return Task.FromResult(_SupportedColorPreset);
        }

        public Task<string> GetMonitorProfile(MonitorInfo m)
        {
            string Key_Profile_Name = string.Empty;

            try
            {
                Key_Profile_Name = MonitorProfile.GetMonitorProfile(m.DisplayName);
            }
            catch (Exception ex)
            {
                writelog($"GetMonitorProfile Exception {ex.Message.ToString()}");
            }

            return Task.FromResult(Key_Profile_Name);
        }

        public Task<string> GetAutoColorPresetStatus(MonitorInfo m)
        {
            writelog("DeviceManagerPlugin received GetAutoColorPresetStatus requested ...");

            if (_ColorPresetPlugin == null)
            {
                writelog("null _ColorPresetPlugin in [DeviceManagerPlugin - GetAutoColorPresetStatus]");
                return Task.FromResult("OFF");
            }

            var temp = _ColorPresetPlugin.GetAutoColorPresetStatus(m, _SettingsPlugin).Result;

            return Task.FromResult(temp.ToString());
        }

        public Task<string> GetColorManagementStatus(MonitorInfo m)
        {
            writelog("DeviceManagerPlugin received GetColorManagementStatus requested ...");

            if (_ColorPresetPlugin == null)
            {
                writelog("null _ColorPresetPlugin in [DeviceManagerPlugin - GetColorManagementStatus]");
                return Task.FromResult("OFF");
            }

            var temp = _ColorPresetPlugin.GetColorManagementStatus(m, _SettingsPlugin).Result;

            return Task.FromResult(temp.ToString());
        }

        public Task<bool> SetMonitorProfile(MonitorInfo m, string ColorPreset_Name)
        {
            bool blRet = true;

            if (_ColorPresetPlugin == null)
            {
                writelog("null _ColorPresetPlugin in [DeviceManagerPlugin - SetMonitorProfile]");
                return Task.FromResult(blRet);
                //return blRet;
            }

            blRet = _ColorPresetPlugin.SetMonitorProfile(m, ColorPreset_Name).Result;

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
        public async Task<bool> WriteColorPreset(MonitorInfo m, string ColorPreset_Name, int colorPresetRunType = 0, string reqAppName = null, bool showOSD = true)
        {
            writelog("DeviceManagerPlugin received WriteColorPreset requested ...");

            bool r = false;
            if (_ColorPresetPlugin == null)
            {
                writelog("null _ColorPresetPlugin in [DeviceManagerPlugin - WriteColorPreset]");
                //return Task.FromResult(r);
                return r;
            }

            //if (colorPresetRunType == 0)
            //{
            //data process
            if (colorPresetRunType == (int)ColorPresetRunType.Auto)
                r = _ColorPresetPlugin.WriteColorPreset(m, ColorPreset_Name, null, colorPresetRunType).Result;
            else
                r = _ColorPresetPlugin.WriteColorPreset(m, ColorPreset_Name, _SettingsPlugin, colorPresetRunType).Result;
            //var tmp = _ColorPresetPlugin.WriteColorPreset(m, ColorPreset_Name, _SettingsPlugin.ReadColorPresetSettings().Result).Result;

            //write back to settings
            //r = _SettingsPlugin.WriteColorPresetSettings(tmp).Result;

            //Thread.Sleep(100);
            //}

            Trace.WriteLine("reqKey (_GlobalSettingParam.GlobalSetting_General.Display_Color_Preset_and_Easy_Memory) = " + _GlobalSettingParam.GlobalSetting_General.Display_Color_Preset_and_Easy_Memory);
            Trace.WriteLine("reqKey (showOSD) = " + showOSD);

            // Jim add 20240925
            if (_GlobalSettingParam.GlobalSetting_General.Display_Color_Preset_and_Easy_Memory && showOSD)
            {
                //show OSD over colorpreset plugin
                _ColorPresetPlugin.ShowOSD_ColoPreset(m, ColorPreset_Name);
            }

            //if (r) // 20240717 jim remove
            //{
            //write VCP over display manager
            //r = SetVCPCapability(m, "colorpreset", ColorPreset_Name).Result;
            r = await Task.Run(() => SetVCPCapability(m, "colorpreset", ColorPreset_Name).Result).ConfigureAwait(false);

            Trace.Write($"ColorPreset_Name = {ColorPreset_Name}");
            //}
            //return Task.FromResult(r);


            //Telementry Collection
            var rt = false;
            var Displaysettings_Function = new Displaysettings_Function();

            if (colorPresetRunType == 1) //Auto
            {
                if (!string.IsNullOrEmpty(reqAppName) && !string.IsNullOrEmpty(ColorPreset_Name))
                {
                    writelog("[DeviceMangerPlugin] Send Telementry for Color_Preset_Auto...");
                    rt = Displaysettings_Function.Send_Color_Preset_Auto_Telementry(_TelementryScheduler, m, reqAppName + "_" + ColorPreset_Name, GetMonitorCurrentResolution(m), GetMonitorMaxResolution(m));
                    if (rt) writelog("[DeviceMangerPlugin] Send Telementry for Color_Preset_Auto Success ...");
                    else writelog("[DeviceMangerPlugin] Send Telementry for Color_Preset_Auto Fail ...");
                }
            }
            else //Manual
            {
                if (!string.IsNullOrEmpty(ColorPreset_Name))
                {
                    writelog("[DeviceMangerPlugin] Send Telementry for Color_Preset_Manual...");
                    rt = Displaysettings_Function.Send_Color_Preset_Manual_Telementry(_TelementryScheduler, m, ColorPreset_Name, GetMonitorCurrentResolution(m), GetMonitorMaxResolution(m));
                    if (rt) writelog("[DeviceMangerPlugin] Send Telementry for Color_Preset_Manual Success ...");
                    else writelog("[DeviceMangerPlugin] Send Telementry for Color_Preset_Manual Fail ...");
                }
            }

            return r;
        }

        public Task<bool> Send_NightLightStatus_Telementry_SA(MonitorInfo m, string NightLightStatus)
        {
            writelog("DeviceManagerPlugin received Send_NightLightStatus_Telementry_SA requested ...");

            bool blRet = true;

            var rt = false;
            var Displaysettings_Function = new Displaysettings_Function();

            if (!string.IsNullOrEmpty(NightLightStatus))
            {
                writelog("[DeviceMangerPlugin] Send Telementry for NightLightStatus...");
                rt = Displaysettings_Function.Send_NightLightStatus_Telementry(_TelementryScheduler, m, NightLightStatus, GetMonitorCurrentResolution(m), GetMonitorMaxResolution(m));
                if (rt) writelog("[DeviceMangerPlugin] Send Telementry for NightLightStatus Success ...");
                else writelog("[DeviceMangerPlugin] Send Telementry for NightLightStatus Fail ...");
            }


            return Task.FromResult(blRet);
        }

        // 20240619 jim modify
        public async Task<bool> WriteColorPreset_AUTO(MonitorInfo m, string ColorPreset_Name)
        {
            writelog("ColorPresetPlugin received WriteColorPreset_AUTO requested ...");

            bool r = false;
            if (_ColorPresetPlugin == null)
            {
                writelog("null _ColorPresetPlugin in [DeviceManagerPlugin - WriteColorPreset_AUTO]");
                return r;
            }

            /*
            //data process
            var tmp = _ColorPresetPlugin.WriteColorPreset_AUTO(m, ColorPreset_Name, _SettingsPlugin.ReadColorPresetSettings().Result).Result;

            //write back to settings
            r = _SettingsPlugin.WriteColorPresetSettings(tmp).Result;

            Thread.Sleep(100);
            */

            //show OSD over colorpreset plugin
            _ColorPresetPlugin.ShowOSD_ColoPreset(m, ColorPreset_Name, true, true);

            //if (r)
            //{
            //write VCP over display manager
            r = await Task.Run(() => SetVCPCapability(m, "colorpreset", ColorPreset_Name).Result).ConfigureAwait(false);

            //}
            return r;
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
            //var tmp = _ColorPresetPlugin.WriteColorPreset(m, ColorPreset_Name, _SettingsPlugin.ReadColorPresetSettings().Result).Result;

            //write back to settings
            //r = _SettingsPlugin.WriteColorPresetSettings(tmp).Result;

            //Thread.Sleep(100);

            //show OSD over colorpreset plugin
            //_ColorPresetPlugin.ShowOSD_ColoPreset(m, ColorPreset_Name);

            //if (r)
            //{
            //write VCP over display manager
            //    r = SetVCPCapability(m, "colorpreset", ColorPreset_Name).Result;
            //}

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

            // Elsa Add Security
            string FileInfo;
            if (!DDPMFileSecurity.IsFolderPathValid(iconFolder, out FileInfo))
            {
                writelog($"{nameof(AddColorPresetForMonitorConfig)} {FileInfo}");
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
        public void Launch_MonitorBorker(MonitorInfo m, bool SmartHDR_ON = false)
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
                        MonitorBorkerWin.Set_AUTO_ColorPresetConfig(true, SmartHDR_ON, _SupportedColorPreset);
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

        /// <summary>
        /// 啟動監視NightLight Status
        /// </summary>
        public Task<bool> CheckNightLightStatus()
        {
            writelog("DeviceManagerPlugin received CheckNightLightStatus requested ...");

            if (_ColorPresetPlugin == null)
            {
                writelog("null _ColorPresetPlugin in [DeviceManagerPlugin - CheckNightLightStatus]");
                return Task.FromResult(false);
            }

            var temp = _ColorPresetPlugin.CheckNightLightStatus().Result;

            return Task.FromResult(temp);
        }

        /// <summary>
        /// 啟動監視Color ICC profile Status
        /// </summary>
        public Task<bool> CheckColorICCStatus()
        {
            writelog("DeviceManagerPlugin received CheckColorICCStatus requested ...");

            if (_ColorPresetPlugin == null)
            {
                writelog("null _ColorPresetPlugin in [DeviceManagerPlugin - CheckColorICCStatus]");
                return Task.FromResult(false);
            }

            var temp = _ColorPresetPlugin.CheckColorICCStatus().Result;

            return Task.FromResult(temp);
        }

        /// <summary>
        /// 停止監視NightLight Status
        /// </summary>
        public Task<bool> StopRegistryMonitor_NightLight()
        {
            writelog("DeviceManagerPlugin received StopRegistryMonitor_NightLight requested ...");

            if (_ColorPresetPlugin == null)
            {
                writelog("null _ColorPresetPlugin in [DeviceManagerPlugin - StopRegistryMonitor_NightLight]");
                return Task.FromResult(false);
            }

            var temp = _ColorPresetPlugin.StopRegistryMonitor_NightLight().Result;

            return Task.FromResult(temp);
        }

        /// <summary>
        /// 停止監視Color ICC profile Status
        /// </summary>
        public Task<bool> StopRegistryMonitor_ICC()
        {
            writelog("DeviceManagerPlugin received StopRegistryMonitor_ICC requested ...");

            if (_ColorPresetPlugin == null)
            {
                writelog("null _ColorPresetPlugin in [DeviceManagerPlugin - StopRegistryMonitor_ICC]");
                return Task.FromResult(false);
            }

            var temp = _ColorPresetPlugin.StopRegistryMonitor_ICC().Result;

            return Task.FromResult(temp);
        }

        /// <summary>
        /// 自動根據App name 去設定 color preset
        /// </summary>
        /// <param name="mo"></param> 螢幕資訊
        /// <param name="on_off"></param> 啟用/關閉 自動根據App name 去設定 color preset
        public Task<bool> AutoSetColorPresetForMonitorConfig(MonitorInfo mo, string on_off, bool Islock = false)
        {
            writelog("DeviceManagerPlugin received AutoSetColorPresetForMonitorConfig requested ...");

            if (_ColorPresetPlugin == null)
            {
                writelog("null _ColorPresetPlugin in [DeviceManagerPlugin - AutoSetColorPresetForMonitorConfig]");
                return Task.FromResult(false);
            }

            bool SmartHDR_ON = GetHDRStatus(mo).Result;

            var temp = _ColorPresetPlugin.AutoSetColorPresetForMonitorConfig(mo, on_off, _SettingsPlugin, this, SmartHDR_ON).Result;

            return Task.FromResult(temp);
        }

        /// <summary>
        /// Auto turn on when Bymonitor and Byhost values are sent
        /// </summary>
        /// <param name="monitorInfo"></param> 螢幕資訊
        /// <param name="off_bymonitor_byhost"></param> off - turn off Auto Color Management
        /// <param name="off_bymonitor_byhost"></param> Bymonitor - automatically adjust the ICC color profile based on monitor color preset
        /// <param name="off_bymonitor_byhost"></param> Byhost - Automatically adjust the monitor color preset based on ICC color profile
        public Task<bool> AutoColorManagementForMonitorConfig(MonitorInfo monitorInfo, string off_bymonitor_byhost, string ColorPreset_Name = "", string ICC_profile_Name = "")
        {
            writelog("DeviceManagerPlugin received AutoColorManagementForMonitorConfig requested ...");

            if (_ColorPresetPlugin == null)
            {
                writelog("null _ColorPresetPlugin in [DeviceManagerPlugin - AutoColorManagementForMonitorConfig]");
                return Task.FromResult(false);
            }

            var temp = _ColorPresetPlugin.AutoColorManagementForMonitorConfig(monitorInfo, off_bymonitor_byhost, _SettingsPlugin, ColorPreset_Name, ICC_profile_Name).Result;

            return Task.FromResult(temp);
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

        public Task<string> GetColorPresetName(int Color_VCPCore_E2)
        {
            writelog("DeviceManagerPlugin received GetColorPresetName requested ...");

            if (_ColorPresetPlugin == null)
            {
                writelog("null _ColorPresetPlugin in [DeviceManagerPlugin - GetColorPresetName]");
                return Task.FromResult(string.Empty);
            }

            var temp = _ColorPresetPlugin.GetColorPresetName(Color_VCPCore_E2).Result;

            return Task.FromResult(temp);
        }

        public Task<int> GetColorVCPCoreValue(string ColorPreset_Name)
        {
            writelog("DeviceManagerPlugin received GetColorVCPCoreValue requested ...");

            if (_ColorPresetPlugin == null)
            {
                writelog("null _ColorPresetPlugin in [DeviceManagerPlugin - GetColorVCPCoreValue]");
                return Task.FromResult(-1);
            }

            var temp = _ColorPresetPlugin.GetColorVCPCoreValue(ColorPreset_Name).Result;

            return Task.FromResult(temp);
        }

        public Task<string> Sync_ColorPresetName(MonitorInfo monitorInfo, string ColorPreset_Name)
        {
            writelog("DeviceManagerPlugin received Sync_ColorPresetName requested ...");

            if (_ColorPresetPlugin == null)
            {
                writelog("null _ColorPresetPlugin in [DeviceManagerPlugin - Sync_ColorPresetName]");
                return Task.FromResult(string.Empty);
            }

            var temp = _ColorPresetPlugin.Sync_ColorPresetName(monitorInfo, ColorPreset_Name).Result;

            return Task.FromResult(temp);
        }

        public Task<bool> SyncNightlightStatus()
        {
            writelog("DeviceManagerPlugin received SyncNightlightStatus requested ...");

            if (_ColorPresetPlugin == null)
            {
                writelog("null _ColorPresetPlugin in [DeviceManagerPlugin - SyncNightlightStatus]");
                return Task.FromResult(false);
            }

            var temp = _ColorPresetPlugin.SyncNightlightStatus().Result;

            return Task.FromResult(true);
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

        public Task<bool> WriteScheduleMonitorSettings(MonitorInfo monitorInfo, scheduleInfo scheduleInfo)
        {
            if (_SettingsPlugin == null)
            {
                writelog("@ WriteScheduleMonitorSettings: _SettingsPlugin is null.");
                return Task.FromResult(false);
            }

            //Keep the device ID for usage
            string model = monitorInfo.modelName;
            string serviceTag = monitorInfo.edid.ServiceTag;

            List<DDPMMonitorSettings> settings = _SettingsPlugin.ReloadMonitorSettings(model).Result;
            if (settings == null)
            {
                writelog($"@ WriteScheduleMonitorSettings: ReloadMonitorSettings(model={model}) return null.");
                return Task.FromResult(false);
            }

            //Find the previous saved device settings
            DDPMMonitorSettings? monitorSettings = settings.FirstOrDefault(x => x.ServiceTag.Equals(monitorInfo.edid.ServiceTag));
            //If not found => return error, GetAllMonitor() will init and create an initial settings instance for us
            if (monitorSettings == null)
            {
                writelog($"@ WriteScheduleMonitorSettings: Reloaded settings not contains (model={model}, serviceTage={serviceTag}).");
                return Task.FromResult(false);
            }

            monitorSettings.scheduleInfo = scheduleInfo;

            if (_SettingsPlugin.WriteMonitorSettings(monitorInfo.modelName, settings).Result)
            {
                writelog($"@ WriteScheduleMonitorSettings(model={model}, serviceTage={serviceTag}) OK.");
                return Task.FromResult(true);
            }
            writelog($"@ WriteScheduleMonitorSettings: WriteMonitorSettings(model={model}, serviceTage={serviceTag}) failed.");
            return Task.FromResult(false);
        }

        public Task<scheduleInfo> ReadScheduleMonitorSettings(MonitorInfo monitorInfo)
        {
            //Create a default output
            scheduleInfo defaultOutput = null;

            if (_SettingsPlugin == null)
            {
                writelog("@ ReadScheduleMonitorSettings: _SettingsPlugin is null.");
                return Task.FromResult(defaultOutput);
            }

            //Keep the device ID for usage
            string model = monitorInfo.modelName;
            string serviceTag = monitorInfo.edid.ServiceTag;

            //Read all settings for this model
            List<DDPMMonitorSettings> settings = _SettingsPlugin.ReloadMonitorSettings(model).Result;
            if (settings == null) //never, but check for safe
            {
                writelog($"@ ReadScheduleMonitorSettings: ReloadMonitorSettings(model={model}) is null.");
                return Task.FromResult(defaultOutput);
            }

            //Find the settings for the specified device
            DDPMMonitorSettings monitorSetting = settings.Find(x => x.ServiceTag == monitorInfo.edid.ServiceTag);
            //There is no settings found for this device
            if (monitorSetting == null)
            {
                writelog($"@ ReadScheduleMonitorSettings: Settings for (model={model}, serviceTag={serviceTag}) is not found (never be saved before).");
                return Task.FromResult(defaultOutput);
            }
            defaultOutput = new scheduleInfo();
            defaultOutput = monitorSetting.scheduleInfo;
            //Return the EA settings from the settings file
            return Task.FromResult(defaultOutput);
        }

        public Task<bool> MigrateScheduleMonitorSettings(string Model, string ServiceTag, BriConSchedule DDMSetting)
        {
            bool r = false;

            if (DDMSetting != null)
            {
                MonitorInfo TempMonitorinfo = new MonitorInfo()
                {
                    modelName = Model,
                    edid = new EDID()
                    {
                        ServiceTag = ServiceTag
                    }
                };
                scheduleInfo DDPMSetting = ReadScheduleMonitorSettings(TempMonitorinfo).Result;

                if (DDPMSetting == null)
                    DDPMSetting = new scheduleInfo();

                DDPMSetting.model = Model;
                DDPMSetting.serviceTag = ServiceTag;
                DDPMSetting.IsEnable = DDMSetting.IsEnabled;
                DDPMSetting.Pre1Name = DDMSetting.Profile1.PresetName;
                DDPMSetting.Pre2Name = DDMSetting.Profile2.PresetName;
                bool r1 = Int32.TryParse(DDMSetting.Profile1.Time.Replace("AM", string.Empty).Replace("PM", string.Empty).Trim().Split(':')[0], out int h1);
                DDPMSetting.Hours1 = r1 ? h1 : 8;
                bool r2 = Int32.TryParse(DDMSetting.Profile1.Time.Replace("AM", string.Empty).Replace("PM", string.Empty).Trim().Split(':')[1], out int m1);
                DDPMSetting.Mins1 = r2 ? m1 : 0;
                DDPMSetting.Duration1 = DDMSetting.Profile1.Duration;
                bool r3 = Int32.TryParse(DDMSetting.Profile2.Time.Replace("AM", string.Empty).Replace("PM", string.Empty).Trim().Split(':')[0], out int h2);
                DDPMSetting.Hours2 = r3 ? h2 : 8;
                bool r4 = Int32.TryParse(DDMSetting.Profile2.Time.Replace("AM", string.Empty).Replace("PM", string.Empty).Trim().Split(':')[1], out int m2);
                DDPMSetting.Mins2 = r4 ? m2 : 0;
                DDPMSetting.Duration2 = DDMSetting.Profile2.Duration;
                DDPMSetting.Brightness1 = DDMSetting.Profile1.Brightness;
                DDPMSetting.Contrast1 = DDMSetting.Profile1.Contrast;
                DDPMSetting.Brightness2 = DDMSetting.Profile2.Brightness;
                DDPMSetting.Contrast2 = DDMSetting.Profile2.Contrast;

                r = WriteScheduleMonitorSettings(TempMonitorinfo, DDPMSetting).Result;
            }

            return Task.FromResult(r);
        }

        #endregion

        #region Display Service implementation

        public Task Reset0x52TimerTick(int millisecond, int processID = -0xFF)
        {
            writelog("DeviceMangerPlugin received Reset0x52TimerTick: " + millisecond.ToString() + $" requested, process ID[{processID}]");

            _DisplayManagerPlugin.Reset0x52TimerTick(millisecond, processID);
            _millisecond = millisecond;

            return Task.FromResult(Task.CompletedTask);
        }

        public Task<List<MonitorInfo>> GetMonitors()
        {
            lock (_MoLock)//this) //Dean 0626 fix SAST issue, do not lock over this object
            {
                writelog("DeviceMangerPlugin received GetMonitors requested ...");
                //if (_AllInfoMonitorsRecord.Count == 0)
                //{
                if (_AllInfoMonitors != null)
                    _AllInfoMonitors.Clear();
                else
                    _AllInfoMonitors = new List<MonitorInfo>();

                if (_DisplayManagerPlugin == null)
                {
                    writelog("null _DisplayManagerPlugin in [GetMonitors], retrun empty monitor list");
                    return Task.FromResult(_AllInfoMonitors);
                }
                List<MonitorInfo> mos = _DisplayManagerPlugin.GetMonitors().Result;
                _AllInfoMonitors.AddRange(mos);

                //review monitor list to check duplicated data
                ReviewAllMonitorToAvoidDuplicatedInfo();

                InitMonitorSettings();
                //List<DDPMMonitorSettings> monitorSettingsList = new List<DDPMMonitorSettings>();
                //foreach (MonitorInfo m in _AllInfoMonitors)
                //{
                //    monitorSettingsList = _SettingsPlugin.InitDDPMMonitorConfigFile(m.modelName, out isInitMonitorSettings).Result;
                //    if (isInitMonitorSettings)
                //    {
                //        if (monitorSettingsList == null)
                //        {
                //            monitorSettingsList = new List<DDPMMonitorSettings>();
                //        }
                //        if (monitorSettingsList.Count == 0 || monitorSettingsList.FindIndex(x => x.ServiceTag == m.edid.ServiceTag) == -1)
                //        {
                //            DDPMMonitorSettings settings = new DDPMMonitorSettings();
                //            settings.Model = m.modelName;
                //            settings.ServiceTag = m.edid.ServiceTag;
                //            settings.VCPs = GetAllVCPcode(m);
                //            monitorSettingsList.Add(settings);
                //            bool b = _SettingsPlugin.WriteMonitorSettings(m.modelName, monitorSettingsList).Result;
                //        }

                //    }
                //}

                /*
                _ = Task.Run(async () =>
                {
                    lock (_CheckAutoLock)
                    {
                        CheckAutoColorPresetEnableOnStartedCondition(_AllInfoMonitors);
                    }
                });
                */

                //CheckAutoColorPresetEnableOnStartedCondition(_AllInfoMonitors);

                return Task.FromResult(_AllInfoMonitors);
            }
        }

        public Task<List<MonitorInfo>> Re_GetMonitors()
        {
            writelog("DeviceMangerPlugin received Re_GetMonitors requested ...");

            try
            {
                if (_AllInfoMonitors != null)
                    _AllInfoMonitors.Clear();
                else
                    _AllInfoMonitors = new List<MonitorInfo>();

                if (_DisplayManagerPlugin == null)
                {
                    writelog("null _DisplayManagerPlugin in [Re_GetMonitors], retrun empty monitor list");
                    return Task.FromResult(_AllInfoMonitors);
                }

                if (isLetDisplayServiceIdle == true)
                {
                    writelog("The idle state is true to drop display settings change event, need caller to unblock this param");
                    return Task.FromResult(_AllInfoMonitors);
                }

                try
                {
                    if (_ReGetcancellationTokenSource != null)
                        _ReGetcancellationTokenSource.Cancel();

                    return Task.FromResult(_AllInfoMonitors);
                }
                catch (TaskCanceledException)
                {
                    _ReGetcancellationTokenSource.Dispose();
                    writelog("[DeviceMangerPlugin] Re_GetMonitors cancellation happened...");
                    return Task.FromResult(_AllInfoMonitors);
                }
                catch (OperationCanceledException)
                {
                    _ReGetcancellationTokenSource.Dispose();
                    writelog("[DeviceMangerPlugin] Re_GetMonitors cancellation happened...");
                    return Task.FromResult(_AllInfoMonitors);
                }
                catch (Exception ex)
                {
                    _ReGetcancellationTokenSource.Dispose();
                    // Failed to complete due to e exception
                    writelog($"[DeviceMangerPlugin] --Task.Run(Re_GetMonitors) ...there is an exceptionI-- ({ex.Message})");
                    return Task.FromResult(_AllInfoMonitors);
                    //Done: let's be nice and don't swallow the exception
                    //throw new InvalidOperationException("some exception happened but not about InitializeMonitorsList cancellation");
                }
                finally
                {
                    using (_ReGetcancellationTokenSource = new CancellationTokenSource())
                    {
                        try
                        {
                            var _cancellationTokenSource_tmp = CancellationTokenSource.CreateLinkedTokenSource(_ReGetcancellationTokenSource.Token);

                            var token = _cancellationTokenSource_tmp.Token;

                            writelog("[DeviceMangerPlugin] DeviceMangerPlugin into (Re_GetMonitors) ...");

                            //TODO: May be you'll want to add .ConfigureAwait(false);
                            Task.Run(() =>
                            {
                                try
                                {
                                    List<MonitorInfo> mos = _DisplayManagerPlugin.Re_GetMonitors(token).Result;
                                    _AllInfoMonitors.AddRange(mos);

                                    ReviewAllMonitorToAvoidDuplicatedInfo();

                                    InitMonitorSettings();

                                    Task.Run(() =>
                                    {
                                        //Telementry Collection
                                        var rt = false;
                                        var DeviceTypeConnected_Function = new DeviceTypeConnected_Function();
                                        writelog("[DeviceMangerPlugin] [Telementry] Send Telementry for DeviceTypeConnected_Function...");
                                        rt = DeviceTypeConnected_Function.DeviceTypeConnected_Telementry(_TelementryScheduler, _AllInfoMonitors);
                                        if (rt) writelog("[DeviceMangerPlugin] [Telementry] Send Telementry for DeviceTypeConnected_Function Success ...");
                                        else writelog("[DeviceMangerPlugin] [Telementry] Send Telementry for DeviceTypeConnected_Function Fail ...");
                                    }).ConfigureAwait(false);
                                }
                                catch (Exception ex)
                                {
                                    writelog("(Re_GetMonitors) happened Exception ... " + ex.Message);
                                }
                            }, token).ConfigureAwait(false);

                            writelog("[DeviceMangerPlugin] DeviceMangerPlugin (Re_GetMonitors) finish ...");
                        }
                        catch (TaskCanceledException)
                        {
                            writelog("[DeviceMangerPlugin] DeviceMangerPlugin (Re_GetMonitors) cancellation happened ...");
                        }
                        catch (OperationCanceledException)
                        {
                            writelog("[DeviceMangerPlugin] DeviceMangerPlugin (Re_GetMonitors) cancellation happened ...");
                        }
                        catch (Exception ex)
                        {
                            // Failed to complete due to e exception
                            writelog($"[DeviceMangerPlugin] --Task.Run(Re_GetMonitors) ...there is an exceptionII-- ({ex.Message})");
                        }
                        finally
                        {
                            _ReGetcancellationTokenSource.Dispose();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                writelog("[DeviceMangerPlugin] Initialize Monitors List Exception : " + ex.Message);
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

            Task.Run(() =>
            {
                //Telementry Collection
                var rt = false;
                var Displaysettings_Function = new Displaysettings_Function();
                switch (code)
                {
                    case 0x10:

                        if (monitorInfo.CapabilityDic.ContainsKey("12"))
                        {
                            writelog("[DeviceMangerPlugin] Send Telementry for Brightness...");
                            rt = Displaysettings_Function.Send_Brightness_Telementry(_TelementryScheduler, monitorInfo, val, GetMonitorCurrentResolution(monitorInfo), GetMonitorMaxResolution(monitorInfo));
                            if (rt) writelog("[DeviceMangerPlugin] Send Telementry for Brightness Success ...");
                            else writelog("[DeviceMangerPlugin] Send Telementry for Brightness Fail ...");
                        }
                        else
                        {
                            writelog("[DeviceMangerPlugin] [Telementry] Send Telementry for Luminanc...");
                            rt = Displaysettings_Function.Send_Luminance_Telementry(_TelementryScheduler, monitorInfo, val, GetMonitorCurrentResolution(monitorInfo), GetMonitorMaxResolution(monitorInfo));
                            if (rt) writelog("[DeviceMangerPlugin] [Telementry] Send  Telementry for Luminanc Success ...");
                            else writelog("[DeviceMangerPlugin] [Telementry] Send  Telementry for Luminanc Fail ...");
                        }
                        break;

                    case 0x12:

                        writelog("[DeviceMangerPlugin] [Telementry] Send Telementry for Contrast...");
                        rt = Displaysettings_Function.Send_Contrast_Telementry(_TelementryScheduler, monitorInfo, val, GetMonitorCurrentResolution(monitorInfo), GetMonitorMaxResolution(monitorInfo));
                        if (rt) writelog("[DeviceMangerPlugin] [Telementry] Send Telementry for Contrast Success ...");
                        else writelog("[DeviceMangerPlugin] [Telementry] Send Telementry for Contrast Fail ...");

                        break;

                    default:
                        break;
                }
            }).ConfigureAwait(false);

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

            if (monitorInfoX != null)
            {
                if (!string.IsNullOrEmpty(val))
                {
                    r = _DisplayManagerPlugin.SetVCPCapability(monitorInfoX, FunctionName, val).Result;

                    //Telementry Collection
                    var Displaysettings_Function = new Displaysettings_Function();
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
                        if (Displaysettings_Function.Send_InputSource_Telementry(_TelementryScheduler, monitorInfoX, val, GetMonitorCurrentResolution(monitorInfoX), GetMonitorMaxResolution(monitorInfoX)))
                        {
                            writelog("[DeviceMangerPlugin] [Telementry] Send Telementry for InputSource Success ...");
                        }
                        else
                        {
                            writelog("[DeviceMangerPlugin] [Telementry] Send Telementry for InputSource Fail ...");
                        }
                        _AllInfoMonitors = GetMonitors().Result;
                    }
                }
                else
                {
                    writelog("[DeviceMangerPlugin] [SetVCPCapability] val is null or empty...");
                }
            }
            else
            {
                writelog("[DeviceMangerPlugin] [SetVCPCapability] monitorInfoX is null ...");
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
            writelog("[DeviceMangerPlugin] GetInputSourcelist ...");
            //_inputSourcelist = _DisplayManagerPlugin.GetInputSourcelist(monitorInfo).Result;
            //DDPMSettings config = _SettingsPlugin.ReloadAppConfigData().Result;
            Dictionary<string, InputInfo> inputSourcelist = new Dictionary<string, InputInfo>();
            Dictionary<string, InputInfo> copyinputlist = new Dictionary<string, InputInfo>();
            Dictionary<string, InputInfo> readinputlist = new Dictionary<string, InputInfo>();
            //get monitor settings
            List<DDPMMonitorSettings> settings = _SettingsPlugin.ReloadMonitorSettings(monitorInfo.modelName).Result;
            if (settings != null)
            {
                //get monitor setting
                DDPMMonitorSettings monitorSetting = settings.Find(x => x.ServiceTag == monitorInfo.edid.ServiceTag);
                if (monitorSetting == null)
                {
                    //inputSourcelist = _DisplayManagerPlugin.GetInputSourcelist(monitorInfo).Result;
                    //bool b = SetInputSourcelist(monitorInfo, inputSourcelist).Result;
                    writelog("[DeviceMangerPlugin] monitorSetting is null ...");
                }
                else
                {
                    try
                    {
                        if (monitorSetting.Input != null)
                        {
                            if (!String.IsNullOrEmpty(monitorSetting.Input.strInputSourceList)/* != null && monitorSetting.Input.strInputSourceList != string.Empty*/)
                            {
                                inputSourcelist = InputSourceListDeserialize(monitorSetting.Input.strInputSourceList);
                                if (inputSourcelist != null)
                                {
                                    if (inputSourcelist.Count != 0)
                                    {
                                        copyinputlist = inputSourcelist;
                                        foreach (var input in inputSourcelist)
                                        {
                                            //Maybe Migration...
                                            if (input.Value.USBUpstream == string.Empty)
                                            {
                                                readinputlist = _DisplayManagerPlugin.GetInputSourcelist(monitorInfo).Result;
                                                if (readinputlist != null)
                                                {
                                                    if (readinputlist.Count != 0)
                                                    {
                                                        foreach (var readinput in readinputlist)
                                                        {
                                                            foreach (var copyinput in copyinputlist)
                                                            {
                                                                if (readinput.Value.Code == copyinput.Value.Code)
                                                                {
                                                                    readinput.Value.InputName = copyinput.Value.InputName;
                                                                    break;
                                                                }
                                                            }
                                                        }
                                                        bool b1 = SetInputSourcelist(monitorInfo, readinputlist).Result;
                                                        return Task.FromResult(readinputlist);
                                                    }
                                                }
                                                break;
                                            }
                                        }
                                        return Task.FromResult(inputSourcelist);
                                    }
                                }
                            }
                        }
                        else
                        {
                            writelog("[DeviceMangerPlugin] monitorSetting.Input is null ...");
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
            writelog("[DeviceMangerPlugin] SetInputSourcelist ...");
            List<DDPMMonitorSettings> settings = _SettingsPlugin.ReloadMonitorSettings(monitorInfo.modelName).Result;
            if (settings != null && inputlist != null)
            {
                if (inputlist.Count != 0)
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
                else
                {
                    writelog("[DeviceMangerPlugin] inputlist count is 0 ...");
                }
            }
            else
            {
                writelog("[DeviceMangerPlugin] settings or inputlist is null ...");
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
                    //string strInputList = InputSourceListSerialize(inputSourceList);
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
                        //string strInputList = InputSourceListSerialize(inputSourceList);
                        if (SetInputSourcelist(monitorInfo, inputSourceList).Result)
                        {
                            return Task.FromResult(true);
                        }
                    }
                }
                //Telementry Collection
                var Displaysettings_Function = new Displaysettings_Function();
                if (Displaysettings_Function.Send_USB_Telementry(_TelementryScheduler, monitorInfo, upstream, GetMonitorCurrentResolution(monitorInfo), GetMonitorMaxResolution(monitorInfo)))
                {
                    writelog("[DeviceMangerPlugin] [Telementry] Send Telementry for USB Association Success ...");
                }
                else
                {
                    writelog("[DeviceMangerPlugin] [Telementry] Send Telementry for USB Association Fail ...");
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
                    //string strInputList = InputSourceListSerialize(inputSourceList);
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

        public async Task<DeviceHelper> GetDevices(bool Rescan = false)
        {
            return await Task.Run(() => _PeripheralsPlugin.GetDevices(Rescan));
        }

        public async Task<DeviceHelper> GetDevices_WithoutAwait(bool Rescan = false)
        {
            return _PeripheralsPlugin.GetDevices_WithoutAwait(Rescan).Result;
        }

        public async Task<CTKMessageHelper> GetCTKMessageHelper()
        {
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

            DeviceInfo di = _PeripheralsPlugin.GetDevices().Result.deviceInfo.FirstOrDefault(x => x.ID == deviceId);
            DeviceChangedEventArgs _EventArgs = new DeviceChangedEventArgs();
            _EventArgs.type = DeviceChangedType.Peripherals_SettingsChange;
            _EventArgs.device_peripherals = di;
            _EventArgs.changedProperty = "CollaborationScreenShareEnable";
            DeviceChanged?.Invoke(this, _EventArgs);

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

        public Task StopPairingPen()
        {
            writelog("DeviceMangerPlugin received StopPairingPen requested ...");
            _PeripheralsPlugin.StopPairingPen();
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

        public Task SetWearDetectionForCLI(int newValue, Guid deviceId)
        {
            writelog("DeviceMangerPlugin received SetWearDetectionForCLI requested ...");
            writelog($"Target SetWearDetectionForCLI is {deviceId}");
            _PeripheralsPlugin.SetWearDetectionForCLI(newValue, deviceId);
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

        public Task SetWALTime(int newValue, Guid deviceId)
        {
            writelog("DeviceMangerPlugin received SetWALTime requested ...");
            writelog($"Target DeviceID is {deviceId}");
            _PeripheralsPlugin.SetWALTime(newValue, deviceId);
            return Task.FromResult(true);
        }

        public Task SetSnooze(int newValue, Guid deviceId)
        {
            writelog("DeviceMangerPlugin received SetSnooze requested ...");
            writelog($"Target DeviceID is {deviceId}");
            _PeripheralsPlugin.SetSnooze(newValue, deviceId);
            return Task.FromResult(true);
        }

        public Task SetSnoozeLength(int newValue, Guid deviceId)
        {
            writelog("DeviceMangerPlugin received SetSnoozeLength requested ...");
            writelog($"Target DeviceID is {deviceId}");
            _PeripheralsPlugin.SetSnoozeLength(newValue, deviceId);
            return Task.FromResult(true);
        }

        public Task SetIsProximitySensorEnable(bool newValue, Guid deviceId)
        {
            writelog("DeviceMangerPlugin received SetIsProximitySensorEnable requested ...");
            writelog($"Target DeviceID is {deviceId}");
            _PeripheralsPlugin.SetIsProximitySensorEnable(newValue, deviceId);
            return Task.FromResult(true);
        }

        public Task SetIsWakeonApproachEnable(bool newValue, Guid deviceId)
        {
            writelog("DeviceMangerPlugin received SetIsWakeonApproachEnable requested ...");
            writelog($"Target DeviceID is {deviceId}");
            _PeripheralsPlugin.SetIsWakeonApproachEnable(newValue, deviceId);
            return Task.FromResult(true);
        }

        public Task SetIsWalkAwayLockEnable(bool newValue, Guid deviceId)
        {
            writelog("DeviceMangerPlugin received SetIsWalkAwayLockEnable requested ...");
            writelog($"Target DeviceID is {deviceId}");
            _PeripheralsPlugin.SetIsWalkAwayLockEnable(newValue, deviceId);
            return Task.FromResult(true);
        }

        public Task<int> GetSnooze(Guid deviceId)
        {
            writelog("DeviceMangerPlugin received GetSnooze requested ...");
            writelog($"Target DeviceID is {deviceId}");
            int nRes;
            nRes = _PeripheralsPlugin.GetSnooze(deviceId);
            return Task.FromResult(nRes);
        }

        public Task<int> GetSnoozeLength(Guid deviceId)
        {
            writelog("DeviceMangerPlugin received GetSnoozeLength requested ...");
            writelog($"Target DeviceID is {deviceId}");
            int nRes;
            nRes = _PeripheralsPlugin.GetSnoozeLength(deviceId);
            return Task.FromResult(nRes);
        }

        #endregion

        #region Headset

        //////////////////////////////////Set///////////////////////////////////

        public async Task<bool> SetMicNoiseCancellationAsync(string guid, bool newValue)
        {
            try
            {
                await _DTPProxyPlugin.SetMicNoiseCancellationAsync(guid, newValue);
                writelog($"[DeviceManagerPlugin] [Headset] SetMicNoiseCancellationAsync success, value is {newValue.ToString()}");
                return true;
            }
            catch (Exception ex)
            {
                writelog($"[DeviceManagerPlugin] [Headset] SetMicNoiseCancellationAsync failed for GUID: {guid}, Error: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SetSidetoneAsync(string guid, bool newValue)
        {
            try
            {
                await _DTPProxyPlugin.SetSidetoneAsync(guid, newValue);
                writelog($"[DeviceManagerPlugin] [Headset] SetSidetoneAsync success, value is {newValue.ToString()}");
                return true;
            }
            catch (Exception ex)
            {
                writelog($"[DeviceManagerPlugin] [Headset] SetSidetoneAsync failed for GUID: {guid}, Error: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SetBusyLightAsync(string guid, bool newValue)
        {
            try
            {
                await _DTPProxyPlugin.SetBusyLightAsync(guid, newValue);
                writelog($"[DeviceManagerPlugin] [Headset] SetBusyLightAsync success, value is {newValue.ToString()}");
                return true;
            }
            catch (Exception ex)
            {
                writelog($"[DeviceManagerPlugin] [Headset] SetBusyLightAsync failed for GUID: {guid}, Error: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SetVoiceGuidanceAsync(string guid, bool newValue)
        {
            try
            {
                await _DTPProxyPlugin.SetVoiceGuidanceAsync(guid, newValue);
                writelog($"[DeviceManagerPlugin] [Headset] SetVoiceGuidanceAsync success, value is {newValue.ToString()}");
                return true;
            }
            catch (Exception ex)
            {
                writelog($"[DeviceManagerPlugin] [Headset] SetVoiceGuidanceAsync failed for GUID: {guid}, Error: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SetSelectedPresetAsync(string guid, int newValue)
        {
            try
            {
                await _DTPProxyPlugin.SetSelectedPresetAsync(guid, newValue);
                writelog($"[DeviceManagerPlugin] [Headset] SetSelectedPresetAsync success, value is {newValue.ToString()}");
                return true;
            }
            catch (Exception ex)
            {
                writelog($"[DeviceManagerPlugin] [Headset] SetSelectedPresetAsync failed for GUID: {guid}, Error: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SetSidetoneLevelAsync(string guid, int newValue)
        {
            try
            {
                await _DTPProxyPlugin.SetSidetoneLevelAsync(guid, newValue);
                writelog($"[DeviceManagerPlugin] [Headset] SetSidetoneLevelAsync success, value is {newValue.ToString()}");
                return true;
            }
            catch (Exception ex)
            {
                writelog($"[DeviceManagerPlugin] [Headset] SetSidetoneLevelAsync failed for GUID: {guid}, Error: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SetBandsGainAsync(string guid, byte[] newValue)
        {
            try
            {
                await _DTPProxyPlugin.SetBandsGainAsync(guid, newValue);
                writelog($"[DeviceManagerPlugin] [Headset] SetBandsGainAsync success, value is {newValue.ToString()}");
                return true;
            }
            catch (Exception ex)
            {
                writelog($"[DeviceManagerPlugin] [Headset] SetBandsGainAsync failed for GUID: {guid}, Error: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SetAncModeAsync(string guid, int newValue)
        {
            try
            {
                await _DTPProxyPlugin.SetAncModeAsync(guid, newValue);
                writelog($"[DeviceManagerPlugin] [Headset] SetAncModeAsync success, value is {newValue.ToString()}");
                return true;
            }
            catch (Exception ex)
            {
                writelog($"[DeviceManagerPlugin] [Headset] SetAncModeAsync failed for GUID: {guid}, Error: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SetAncGainAsync(string guid, int newValue)
        {
            try
            {
                await _DTPProxyPlugin.SetAncGainAsync(guid, newValue);
                writelog($"[DeviceManagerPlugin] [Headset] SetAncGainAsync success, value is {newValue.ToString()}");
                return true;
            }
            catch (Exception ex)
            {
                writelog($"[DeviceManagerPlugin] [Headset] SetAncGainAsync failed for GUID: {guid}, Error: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SetWearDetectionAsync(string guid, int newValue)
        {
            try
            {
                await _DTPProxyPlugin.SetWearDetectionAsync(guid, newValue);
                writelog($"[DeviceManagerPlugin] [Headset] SetWearDetectionAsync success, value is {newValue.ToString()}");
                return true;
            }
            catch (Exception ex)
            {
                writelog($"[DeviceManagerPlugin] [Headset] SetWearDetectionAsync failed for GUID: {guid}, Error: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SetMicNCIncomingAsync(string guid, bool newValue)
        {
            try
            {
                await _DTPProxyPlugin.SetMicNCIncomingAsync(guid, newValue);
                writelog($"[DeviceManagerPlugin] [Headset] SetMicNCIncomingAsync success, value is {newValue.ToString()}");
                return true;
            }
            catch (Exception ex)
            {
                writelog($"[DeviceManagerPlugin] [Headset] SetMicNCIncomingAsync failed for GUID: {guid}, Error: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SetUnPairAsync(string guid, bool newValue)
        {
            try
            {
                await _DTPProxyPlugin.SetUnPairAsync(guid, newValue);
                writelog($"[DeviceManagerPlugin] [Headset] SetUnPairAsync success, value is {newValue.ToString()}");
                return true;
            }
            catch (Exception ex)
            {
                writelog($"[DeviceManagerPlugin] [Headset] SetUnPairAsync failed for GUID: {guid}, Error: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SetFactoryResetAsyncValueForHeadset(string guid, bool newValue)
        {
            try
            {
                await _DTPProxyPlugin.SetFactoryResetAsyncValueForHeadset(guid, newValue);
                writelog($"[DeviceManagerPlugin] [Headset] SetFactoryResetAsync success, value is {newValue.ToString()}");
                return true;
            }
            catch (Exception ex)
            {
                writelog($"[DeviceManagerPlugin] [Headset] SetFactoryResetAsync failed for GUID: {guid}, Error: {ex.Message}");
                return false;
            }
        }

        //////////////////////////////////// Get//////////////////////////////////////////

        public async Task<JArray> GetDeviceItemsExAsync(string guid)
        {
            try
            {
                var result = await _DTPProxyPlugin.GetDeviceItemsExAsync(guid);
                writelog($"[DeviceManagerPlugin] [Headset] GetDeviceItemsExAsync Success, value is {result.ToString()}");
                return result;
            }
            catch (Exception ex)
            {
                writelog($"[DeviceManagerPlugin] [Headset] GetDeviceItemsExAsync failed for GUID: {guid} - Exception: {ex.Message}");
                return null;
            }
        }

        public async Task<DeviceInterfaceType> GetInterfaceTypeAsync(string guid)
        {
            try
            {
                var result = await _DTPProxyPlugin.GetInterfaceTypeAsync(guid);
                writelog($"[DeviceManagerPlugin] [Headset] GetInterfaceTypeAsync succeeded, value is {result.ToString()}");
                return result;
            }
            catch (Exception ex)
            {
                writelog($"[DeviceManagerPlugin] [Headset] GetInterfaceTypeAsync failed for {guid} - Exception: {ex.Message}");
                return default(DeviceInterfaceType);
            }
        }

        public async Task<string> GetDeviceNameAsync(string guid)
        {
            try
            {
                var result = await _DTPProxyPlugin.GetDeviceNameAsync(guid);
                writelog($"[DeviceManagerPlugin] [Headset] GetDeviceNameAsync succeeded, value is {result.ToString()}");
                return result;
            }
            catch (Exception ex)
            {
                writelog($"[DeviceManagerPlugin] [Headset] GetDeviceNameAsync failed for {guid} - Exception: {ex.Message}");
                return null;
            }
        }

        public async Task<string> GetDeviceIdAsync(string guid)
        {
            try
            {
                var result = await _DTPProxyPlugin.GetDeviceIdAsync(guid);
                writelog($"[DeviceManagerPlugin] [Headset] GetDeviceIdAsync succeeded, value is {result.ToString()}");
                return result;
            }
            catch (Exception ex)
            {
                writelog($"[DeviceManagerPlugin] [Headset] GetDeviceIdAsync failed for {guid} - Exception: {ex.Message}");
                return null;
            }
        }

        public async Task<string> GetPluginIdAsync(string guid)
        {
            try
            {
                var result = await _DTPProxyPlugin.GetPluginIdAsync(guid);
                writelog($"[DeviceManagerPlugin] [Headset] GetPluginIdAsync succeeded, value is {result.ToString()}");
                return result;
            }
            catch (Exception ex)
            {
                writelog($"[DeviceManagerPlugin] [Headset] GetPluginIdAsync failed for {guid} - Exception: {ex.Message}");
                return null;
            }
        }

        public async Task<int> GetODMIdAsync(string guid)
        {
            try
            {
                var result = await _DTPProxyPlugin.GetODMIdAsync(guid);
                writelog($"[DeviceManagerPlugin] [Headset] GetODMIdAsync succeeded, value is {result.ToString()}");
                return result;
            }
            catch (Exception ex)
            {
                writelog($"[DeviceManagerPlugin] [Headset] GetODMIdAsync failed for {guid} - Exception: {ex.Message}");
                return -1;
            }
        }

        public async Task<string> GetModelNumberAsync(string guid)
        {
            try
            {
                var result = await _DTPProxyPlugin.GetModelNumberAsync(guid);
                writelog($"[DeviceManagerPlugin] [Headset] GetModelNumberAsync succeeded, value is {result.ToString()}");
                return result;
            }
            catch (Exception ex)
            {
                writelog($"[DeviceManagerPlugin] [Headset] GetModelNumberAsync failed for {guid} - Exception: {ex.Message}");
                return null;
            }
        }

        public async Task<int> GetInstanceNumberAsync(string guid)
        {
            try
            {
                var result = await _DTPProxyPlugin.GetInstanceNumberAsync(guid);
                writelog($"[DeviceManagerPlugin] [Headset] GetInstanceNumberAsync succeeded, value is {result.ToString()}");
                return result;
            }
            catch (Exception ex)
            {
                writelog($"[DeviceManagerPlugin] [Headset] GetInstanceNumberAsync failed for {guid} - Exception: {ex.Message}");
                return -1;
            }
        }

        public async Task<int> GetInstanceIdAsync(string guid)
        {
            try
            {
                var result = await _DTPProxyPlugin.GetInstanceIdAsync(guid);
                writelog($"[DeviceManagerPlugin] [Headset] GetInstanceIdAsync succeeded, value is {result.ToString()}");
                return result;
            }
            catch (Exception ex)
            {
                writelog($"[DeviceManagerPlugin] [Headset] GetInstanceIdAsync failed for {guid} - Exception: {ex.Message}");
                return -1;
            }
        }

        public async Task<string> GetFirmwareVersionAsync(string guid)
        {
            try
            {
                var result = await _DTPProxyPlugin.GetFirmwareVersionAsync(guid);
                writelog($"[DeviceManagerPlugin] [Headset] GetFirmwareVersionAsync succeeded, value is {result.ToString()}");
                return result;
            }
            catch (Exception ex)
            {
                writelog($"[DeviceManagerPlugin] [Headset] GetFirmwareVersionAsync failed for {guid} - Exception: {ex.Message}");
                return null;
            }
        }

        public async Task<string> GetDeviceTypeAsync(string guid)
        {
            try
            {
                var result = await _DTPProxyPlugin.GetDeviceTypeAsync(guid);
                writelog($"[DeviceManagerPlugin] [Headset] GetDeviceTypeAsync succeeded, value is {result.ToString()}");
                return result;
            }
            catch (Exception ex)
            {
                writelog($"[DeviceManagerPlugin] [Headset] GetDeviceTypeAsync failed for {guid} - Exception: {ex.Message}");
                return null;
            }
        }

        public async Task<string> GetParentDeviceTypeAsync(string guid)
        {
            try
            {
                var result = await _DTPProxyPlugin.GetParentDeviceTypeAsync(guid);
                writelog($"[DeviceManagerPlugin] [Headset] GetParentDeviceTypeAsync succeeded, value is {result.ToString()}");
                return result;
            }
            catch (Exception ex)
            {
                writelog($"[DeviceManagerPlugin] [Headset] GetParentDeviceTypeAsync failed for {guid} - Exception: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> GetIsBatteryLevelSupportedAsync(string guid)
        {
            try
            {
                var result = await _DTPProxyPlugin.GetIsBatteryLevelSupportedAsync(guid);
                writelog($"[DeviceManagerPlugin] [Headset] GetIsBatteryLevelSupportedAsync succeeded, value is {result.ToString()}");
                return result;
            }
            catch (Exception ex)
            {
                writelog($"[DeviceManagerPlugin] [Headset] GetIsBatteryLevelSupportedAsync failed for {guid} - Exception: {ex.Message}");
                return false;
            }
        }

        public async Task<int> GetBatteryLevelAsync(string guid)
        {
            try
            {
                var result = await _DTPProxyPlugin.GetBatteryLevelAsync(guid);
                writelog($"[DeviceManagerPlugin] [Headset] GetBatteryLevelAsync succeeded, value is {result.ToString()}");
                return result;
            }
            catch (Exception ex)
            {
                writelog($"[DeviceManagerPlugin] [Headset] GetBatteryLevelAsync failed for {guid} - Exception: {ex.Message}");
                return -1;
            }
        }

        public async Task<string> GetDeviceBatteryStatusAsync(string guid)
        {
            try
            {
                var result = await _DTPProxyPlugin.GetDeviceBatteryStatusAsync(guid);
                writelog($"[DeviceManagerPlugin] [Headset] GetDeviceBatteryStatusAsync succeeded, value is {result.ToString()}");
                return result;
            }
            catch (Exception ex)
            {
                writelog($"[DeviceManagerPlugin] [Headset] GetDeviceBatteryStatusAsync failed for {guid} - Exception: {ex.Message}");
                return null;
            }
        }

        public async Task<string> GetPairingStatusAsync(string guid)
        {
            try
            {
                var result = await _DTPProxyPlugin.GetPairingStatusAsync(guid);
                writelog($"[DeviceManagerPlugin] [Headset] GetPairingStatusAsync succeeded, value is {result.ToString()}");
                return result;
            }
            catch (Exception ex)
            {
                writelog($"[DeviceManagerPlugin] [Headset] GetPairingStatusAsync failed for {guid} - Exception: {ex.Message}");
                return null;
            }
        }

        public async Task<int> GetMaxPairingSlotsAsync(string guid)
        {
            try
            {
                var result = await _DTPProxyPlugin.GetMaxPairingSlotsAsync(guid);
                writelog($"[DeviceManagerPlugin] [Headset] GetMaxPairingSlotsAsync succeeded, value is {result.ToString()}");
                return result;
            }
            catch (Exception ex)
            {
                writelog($"[DeviceManagerPlugin] [Headset] GetMaxPairingSlotsAsync failed for {guid} - Exception: {ex.Message}");
                return -1;
            }
        }

        public async Task<int> GetPairedDeviceCountAsync(string guid)
        {
            try
            {
                var result = await _DTPProxyPlugin.GetPairedDeviceCountAsync(guid);
                writelog($"[DeviceManagerPlugin] [Headset] GetPairedDeviceCountAsync succeeded, value is {result.ToString()}");
                return result;
            }
            catch (Exception ex)
            {
                writelog($"[DeviceManagerPlugin] [Headset] GetPairedDeviceCountAsync failed for {guid} - Exception: {ex.Message}");
                return -1;
            }
        }

        public async Task<int> GetTotalNumberOfPairedHostNameAsync(string guid)
        {
            try
            {
                var result = await _DTPProxyPlugin.GetTotalNumberOfPairedHostNameAsync(guid);
                writelog($"[DeviceManagerPlugin] [Headset] GetTotalNumberOfPairedHostNameAsync succeeded, value is {result.ToString()}");
                return result;
            }
            catch (Exception ex)
            {
                writelog($"[DeviceManagerPlugin] [Headset] GetTotalNumberOfPairedHostNameAsync failed for {guid} - Exception: {ex.Message}");
                return -1;
            }
        }

        public async Task<string> GetSerialNumberAsync(string guid)
        {
            try
            {
                var result = await _DTPProxyPlugin.GetSerialNumberAsync(guid);
                writelog($"[DeviceManagerPlugin] [Headset] GetSerialNumberAsync succeeded, value is {result.ToString()}");
                return result;
            }
            catch (Exception ex)
            {
                writelog($"[DeviceManagerPlugin] [Headset] GetSerialNumberAsync failed for {guid} - Exception: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> GetIsReadyAsync(string guid)
        {
            try
            {
                var result = await _DTPProxyPlugin.GetIsReadyAsync(guid);
                writelog($"[DeviceManagerPlugin] [Headset] GetIsReadyAsync succeeded, value is {result.ToString()}");
                return result;
            }
            catch (Exception ex)
            {
                writelog($"[DeviceManagerPlugin] [Headset] GetIsReadyAsync failed for {guid} - Exception: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> GetIsDirtyAsync(string guid)
        {
            try
            {
                var result = await _DTPProxyPlugin.GetIsDirtyAsync(guid);
                writelog($"[DeviceManagerPlugin] [Headset] GetIsDirtyAsync succeeded, value is {result.ToString()}");
                return result;
            }
            catch (Exception ex)
            {
                writelog($"[DeviceManagerPlugin] [Headset] GetIsDirtyAsync failed for {guid} - Exception: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> GetIsMicNoiseCancellationSupportedAsync(string guid)
        {
            try
            {
                var result = await _DTPProxyPlugin.GetIsMicNoiseCancellationSupportedAsync(guid);
                writelog($"[DeviceManagerPlugin] [Headset] GetIsMicNoiseCancellationSupportedAsync succeeded, value is {result.ToString()}");
                return result;
            }
            catch (Exception ex)
            {
                writelog($"[DeviceManagerPlugin] [Headset] GetIsMicNoiseCancellationSupportedAsync failed for {guid} - Exception: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> GetIsSidetoneSupportedAsync(string guid)
        {
            try
            {
                var result = await _DTPProxyPlugin.GetIsSidetoneSupportedAsync(guid);
                writelog($"[DeviceManagerPlugin] [Headset] GetIsSidetoneSupportedAsync succeeded, value is {result.ToString()}");
                return result;
            }
            catch (Exception ex)
            {
                writelog($"[DeviceManagerPlugin] [Headset] GetIsSidetoneSupportedAsync failed for {guid} - Exception: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> GetIsBusyLightSupportedAsync(string guid)
        {
            try
            {
                var result = await _DTPProxyPlugin.GetIsBusyLightSupportedAsync(guid);
                writelog($"[DeviceManagerPlugin] [Headset] GetIsBusyLightSupportedAsync succeeded, value is {result.ToString()}");
                return result;
            }
            catch (Exception ex)
            {
                writelog($"[DeviceManagerPlugin] [Headset] GetIsBusyLightSupportedAsync failed for {guid} - Exception: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> GetIsVoiceGuidanceSupportedAsync(string guid)
        {
            try
            {
                var result = await _DTPProxyPlugin.GetIsVoiceGuidanceSupportedAsync(guid);
                writelog($"[DeviceManagerPlugin] [Headset] GetIsVoiceGuidanceSupportedAsync succeeded, value is {result.ToString()}");
                return result;
            }
            catch (Exception ex)
            {
                writelog($"[DeviceManagerPlugin] [Headset] GetIsVoiceGuidanceSupportedAsync failed for {guid} - Exception: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> GetIsPresetsSupportedAsync(string guid)
        {
            try
            {
                var result = await _DTPProxyPlugin.GetIsPresetsSupportedAsync(guid);
                writelog($"[DeviceManagerPlugin] [Headset] GetIsPresetsSupportedAsync succeeded, value is {result.ToString()}");
                return result;
            }
            catch (Exception ex)
            {
                writelog($"[DeviceManagerPlugin] [Headset] GetIsPresetsSupportedAsync failed for {guid} - Exception: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> GetIsEqualizerSupportedAsync(string guid)
        {
            try
            {
                var result = await _DTPProxyPlugin.GetIsEqualizerSupportedAsync(guid);
                writelog($"[DeviceManagerPlugin] [Headset] GetIsEqualizerSupportedAsync succeeded, value is {result.ToString()}");
                return result;
            }
            catch (Exception ex)
            {
                writelog($"[DeviceManagerPlugin] [Headset] GetIsEqualizerSupportedAsync failed for {guid} - Exception: {ex.Message}");
                return false;
            }
        }

        public async Task<HeadsetConnectionType> GetConnectionTypeAsync(string guid)
        {
            try
            {
                var result = await _DTPProxyPlugin.GetConnectionTypeAsync(guid);
                writelog($"[DeviceManagerPlugin] [Headset] GetConnectionTypeAsync succeeded, value is {result.ToString()}");
                return result;
            }
            catch (Exception ex)
            {
                writelog($"[DeviceManagerPlugin] [Headset] GetConnectionTypeAsync failed for {guid} - Exception: {ex.Message}");
                return HeadsetConnectionType.HeadsetConnectionTypeUnknown;
            }
        }

        public async Task<bool> GetIsANCSupportedAsync(string guid)
        {
            try
            {
                var result = await _DTPProxyPlugin.GetIsANCSupportedAsync(guid);
                writelog($"[DeviceManagerPlugin] [Headset] GetIsANCSupportedAsync succeeded, value is {result.ToString()}");
                return result;
            }
            catch (Exception ex)
            {
                writelog($"[DeviceManagerPlugin] [Headset] GetIsANCSupportedAsync failed for {guid} - Exception: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> GetIsWearDetectionSupportedAsync(string guid)
        {
            try
            {
                var result = await _DTPProxyPlugin.GetIsWearDetectionSupportedAsync(guid);
                writelog($"[DeviceManagerPlugin] [Headset] GetIsWearDetectionSupportedAsync succeeded, value is {result.ToString()}");
                return result;
            }
            catch (Exception ex)
            {
                writelog($"[DeviceManagerPlugin] [Headset] GetIsWearDetectionSupportedAsync failed for {guid} - Exception: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> GetIsWearDetectionSensitivitySupportedAsync(string guid)
        {
            try
            {
                var result = await _DTPProxyPlugin.GetIsWearDetectionSensitivitySupportedAsync(guid);
                writelog($"[DeviceManagerPlugin] [Headset] GetIsWearDetectionSensitivitySupportedAsync succeeded, value is {result.ToString()}");
                return result;
            }
            catch (Exception ex)
            {
                writelog($"[DeviceManagerPlugin] [Headset] GetIsWearDetectionSensitivitySupportedAsync failed for {guid} - Exception: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> GetIsWearDetectionPauseMusicSupportedAsync(string guid)
        {
            try
            {
                var result = await _DTPProxyPlugin.GetIsWearDetectionPauseMusicSupportedAsync(guid);
                writelog($"[DeviceManagerPlugin] [Headset] GetIsWearDetectionPauseMusicSupportedAsync succeeded, value is {result.ToString()}");
                return result;
            }
            catch (Exception ex)
            {
                writelog($"[DeviceManagerPlugin] [Headset] GetIsWearDetectionPauseMusicSupportedAsync failed for {guid} - Exception: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> GetIsWearDetectionMuteMicSupportedAsync(string guid)
        {
            try
            {
                var result = await _DTPProxyPlugin.GetIsWearDetectionMuteMicSupportedAsync(guid);
                writelog($"[DeviceManagerPlugin] [Headset] GetIsWearDetectionMuteMicSupportedAsync succeeded, value is {result.ToString()}");
                return result;
            }
            catch (Exception ex)
            {
                writelog($"[DeviceManagerPlugin] [Headset] GetIsWearDetectionMuteMicSupportedAsync failed for {guid} - Exception: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> GetIsWearDetectionQuickPauseSupportedAsync(string guid)
        {
            try
            {
                var result = await _DTPProxyPlugin.GetIsWearDetectionQuickPauseSupportedAsync(guid);
                writelog($"[DeviceManagerPlugin] [Headset] GetIsWearDetectionQuickPauseSupportedAsync succeeded, value is {result.ToString()}");
                return result;
            }
            catch (Exception ex)
            {
                writelog($"[DeviceManagerPlugin] [Headset] GetIsWearDetectionQuickPauseSupportedAsync failed for {guid} - Exception: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> GetMicNoiseCancellationAsync(string guid)
        {
            try
            {
                var result = await _DTPProxyPlugin.GetMicNoiseCancellationAsync(guid);
                writelog($"[DeviceManagerPlugin] [Headset] GetMicNoiseCancellationAsync succeeded, value is {result.ToString()}");
                return result;
            }
            catch (Exception ex)
            {
                writelog($"[DeviceManagerPlugin] [Headset] GetMicNoiseCancellationAsync failed for {guid} - Exception: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> GetMicNCIncomingAsync(string guid)
        {
            try
            {
                var result = await _DTPProxyPlugin.GetMicNCIncomingAsync(guid);
                writelog($"[DeviceManagerPlugin] [Headset] GetMicNCIncomingAsync succeeded, value is {result.ToString()}");
                return result;
            }
            catch (Exception ex)
            {
                writelog($"[DeviceManagerPlugin] [Headset] GetMicNCIncomingAsync failed for {guid} - Exception: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> GetSidetoneAsync(string guid)
        {
            try
            {
                var result = await _DTPProxyPlugin.GetSidetoneAsync(guid);
                writelog($"[DeviceManagerPlugin] [Headset] GetSidetoneAsync succeeded for {result.ToString()}");
                return result;
            }
            catch (Exception ex)
            {
                writelog($"[DeviceManagerPlugin] [Headset] GetSidetoneAsync failed for {guid} - Exception: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> GetBusyLightAsync(string guid)
        {
            try
            {
                var result = await _DTPProxyPlugin.GetBusyLightAsync(guid);
                writelog($"[DeviceManagerPlugin] [Headset] GetBusyLightAsync succeeded for {result.ToString()}");
                return result;
            }
            catch (Exception ex)
            {
                writelog($"[DeviceManagerPlugin] [Headset] GetBusyLightAsync failed for {guid} - Exception: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> GetVoiceGuidanceAsync(string guid)
        {
            try
            {
                var result = await _DTPProxyPlugin.GetVoiceGuidanceAsync(guid);
                writelog($"[DeviceManagerPlugin] [Headset] GetVoiceGuidanceAsync succeeded for {result.ToString()}");
                return result;
            }
            catch (Exception ex)
            {
                writelog($"[DeviceManagerPlugin] [Headset] GetVoiceGuidanceAsync failed for {guid} - Exception: {ex.Message}");
                return false;
            }
        }

        public async Task<int> GetSelectedPresetAsync(string guid)
        {
            try
            {
                var result = await _DTPProxyPlugin.GetSelectedPresetAsync(guid);
                writelog($"[DeviceManagerPlugin] [Headset] GetSelectedPresetAsync succeeded for {result.ToString()}");
                return result;
            }
            catch (Exception ex)
            {
                writelog($"[DeviceManagerPlugin] [Headset] GetSelectedPresetAsync failed for {guid} - Exception: {ex.Message}");
                return -1;
            }
        }

        public async Task<int> GetSidetoneLevelAsync(string guid)
        {
            try
            {
                var result = await _DTPProxyPlugin.GetSidetoneLevelAsync(guid);
                writelog($"[DeviceManagerPlugin] [Headset] GetSidetoneLevelAsync succeeded for {result.ToString()}");
                return result;
            }
            catch (Exception ex)
            {
                writelog($"[DeviceManagerPlugin] [Headset] GetSidetoneLevelAsync failed for {guid} - Exception: {ex.Message}");
                return -1;
            }
        }

        public async Task<bool> GetMuteStatusAsync(string guid)
        {
            try
            {
                var result = await _DTPProxyPlugin.GetMuteStatusAsync(guid);
                writelog($"[DeviceManagerPlugin] [Headset] GetMuteStatusAsync succeeded for {result.ToString()}");
                return result;
            }
            catch (Exception ex)
            {
                writelog($"[DeviceManagerPlugin] [Headset] GetMuteStatusAsync failed for {guid} - Exception: {ex.Message}");
                return false;
            }
        }

        public async Task<byte[]> GetBandsGainAsync(string guid)
        {
            try
            {
                var result = await _DTPProxyPlugin.GetBandsGainAsync(guid);
                if (result != null)
                {
                    writelog($"[DeviceManagerPlugin] [Headset] GetBandsGainAsync succeeded, result length is {result.Length}");
                }
                else
                {
                    writelog($"[DeviceManagerPlugin] [Headset] GetBandsGainAsync succeeded, but result is null");
                }
                return result;
            }
            catch (Exception ex)
            {
                writelog($"[DeviceManagerPlugin] [Headset] GetBandsGainAsync failed for {guid} - Exception: {ex.Message}");
                return null;
            }
        }

        public async Task<int> GetAncModeAsync(string guid)
        {
            try
            {
                var result = await _DTPProxyPlugin.GetAncModeAsync(guid);
                writelog($"[DeviceManagerPlugin] [Headset] GetAncModeAsync succeeded for {result.ToString()}");
                return result;
            }
            catch (Exception ex)
            {
                writelog($"[DeviceManagerPlugin] [Headset] GetAncModeAsync failed for {guid} - Exception: {ex.Message}");
                return -1;
            }
        }

        public async Task<int> GetAncGainAsync(string guid)
        {
            try
            {
                var result = await _DTPProxyPlugin.GetAncGainAsync(guid);
                writelog($"[DeviceManagerPlugin] [Headset] GetAncGainAsync succeeded for {result.ToString()}");
                return result;
            }
            catch (Exception ex)
            {
                writelog($"[DeviceManagerPlugin] [Headset] GetAncGainAsync failed for {guid} - Exception: {ex.Message}");
                return -1;
            }
        }

        public async Task<int> GetWearDetectionAsync(string guid)
        {
            try
            {
                var result = await _DTPProxyPlugin.GetWearDetectionAsync(guid);
                writelog($"[DeviceManagerPlugin] [Headset] GetWearDetectionAsync succeeded for {result.ToString()}");
                return result;
            }
            catch (Exception ex)
            {
                writelog($"[DeviceManagerPlugin] [Headset] GetWearDetectionAsync failed for {guid} - Exception: {ex.Message}");
                return -1;
            }
        }

        public async Task<bool> GetIsMicNCIncomingSupportedAsync(string guid)
        {
            try
            {
                var result = await _DTPProxyPlugin.GetIsMicNCIncomingSupportedAsync(guid);
                writelog($"[DeviceManagerPlugin] [Headset] GetIsMicNCIncomingSupportedAsync succeeded for {result.ToString()}");
                return result;
            }
            catch (Exception ex)
            {
                writelog($"[DeviceManagerPlugin] [Headset] GetIsMicNCIncomingSupportedAsync failed for {guid} - Exception: {ex.Message}");
                return false;
            }
        }

        #endregion Headset

        #region Wired Audio

        //////////////////////////////////Set///////////////////////////////////

        public async Task<bool> SetBassAsync(string guid, int newValue)
        {
            try
            {
                await _DTPProxyPlugin.SetBassAsync(guid, newValue);
                writelog($"[DeviceManagerPlugin] [Speaker] SetBassAsync succeeded, value is {newValue.ToString()}");
                return true;
            }
            catch (Exception ex)
            {
                writelog($"[DeviceManagerPlugin] [Speaker] SetBassAsync failed for {guid} - Exception: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SetMidRangeAsync(string guid, int newValue)
        {
            try
            {
                await _DTPProxyPlugin.SetMidRangeAsync(guid, newValue);
                writelog($"[DeviceManagerPlugin] [Speaker] SetMidRangeAsync succeeded, value is {newValue.ToString()}");
                return true;
            }
            catch (Exception ex)
            {
                writelog($"[DeviceManagerPlugin] [Speaker] SetMidRangeAsync failed for {guid} - Exception: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SetTrebleAsync(string guid, int newValue)
        {
            try
            {
                await _DTPProxyPlugin.SetTrebleAsync(guid, newValue);
                writelog($"[DeviceManagerPlugin] [Speaker] SetTrebleAsync succeeded, value is {newValue.ToString()}");
                return true;
            }
            catch (Exception ex)
            {
                writelog($"[DeviceManagerPlugin] [Speaker] SetTrebleAsync failed for {guid} - Exception: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SetProfileForSpeaker(string guid, string newValue)
        {
            try
            {
                await _DTPProxyPlugin.SetProfileForSpeaker(guid, newValue);
                writelog($"[DeviceManagerPlugin] [Speaker] SetProfileForSpeaker succeeded, value is {newValue.ToString()}");
                return true;
            }
            catch (Exception ex)
            {
                writelog($"[DeviceManagerPlugin] [Speaker] SetProfileForSpeaker failed for {guid} - Exception: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SetIsWiredAudioMicMuteSoundEnableAsync(string guid, bool newValue)
        {
            try
            {
                await _DTPProxyPlugin.SetIsWiredAudioMicMuteSoundEnableAsync(guid, newValue);
                writelog($"[DeviceManagerPlugin] [Speaker] SetIsWiredAudioMicMuteSoundEnableAsync succeeded, value is {newValue.ToString()}");
                return true;
            }
            catch (Exception ex)
            {
                writelog($"[DeviceManagerPlugin] [Speaker] SetIsWiredAudioMicMuteSoundEnableAsync failed for {guid} - Exception: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SetWiredAudioVolumeAdjustmentToneAsync(string guid, int newValue)
        {
            try
            {
                await _DTPProxyPlugin.SetWiredAudioVolumeAdjustmentToneAsync(guid, newValue);
                writelog($"[DeviceManagerPlugin] [Speaker] SetWiredAudioVolumeAdjustmentToneAsync succeeded, value is {newValue.ToString()}");
                return true;
            }
            catch (Exception ex)
            {
                writelog($"[DeviceManagerPlugin] [Speaker] SetWiredAudioVolumeAdjustmentToneAsync failed for {guid} - Exception: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SetIsWiredAudioIMicNSEnableAsync(string guid, bool newValue)
        {
            try
            {
                await _DTPProxyPlugin.SetIsWiredAudioIMicNSEnableAsync(guid, newValue);
                writelog($"[DeviceManagerPlugin] [Speaker] SetIsWiredAudioIMicNSEnableAsync succeeded, value is {newValue.ToString()}");
                return true;
            }
            catch (Exception ex)
            {
                writelog($"[DeviceManagerPlugin] [Speaker] SetIsWiredAudioIMicNSEnableAsync failed for {guid} - Exception: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SetResetToDefaultAsyncForSoundbar(string guid, bool newValue)
        {
            try
            {
                await _DTPProxyPlugin.SetResetToDefaultAsyncForSoundbar(guid, newValue);
                writelog($"[DeviceManagerPlugin] [Speaker] SetResetToDefaultAsyncForSoundbar succeeded, value is {newValue.ToString()}");
                return true;
            }
            catch (Exception ex)
            {
                writelog($"[DeviceManagerPlugin] [Speaker] SetResetToDefaultAsyncForSoundbar failed for {guid} - Exception: {ex.Message}");
                return false;
            }
        }

        //////////////////////////////////Get///////////////////////////////////

        public async Task<string> GetProfileAsync(string guid)
        {
            try
            {
                var result = await _DTPProxyPlugin.GetProfileAsync(guid);
                writelog($"[DeviceManagerPlugin] [Speaker] GetProfileAsync succeeded, value is {result.ToString()}");
                return result;
            }
            catch (Exception ex)
            {
                writelog($"[DeviceManagerPlugin] [Speaker] GetProfileAsync failed for {guid} - Exception: {ex.Message}");
                return null;
            }
        }

        public async Task<int> GetBassAsync(string guid)
        {
            try
            {
                var result = await _DTPProxyPlugin.GetBassAsync(guid);
                writelog($"[DeviceManagerPlugin] [Speaker] GetBassAsync succeeded, value is {result.ToString()}");
                return result;
            }
            catch (Exception ex)
            {
                writelog($"[DeviceManagerPlugin] [Speaker] GetBassAsync failed for {guid} - Exception: {ex.Message}");
                return -1;
            }
        }

        public async Task<int> GetMidRangeAsync(string guid)
        {
            try
            {
                var result = await _DTPProxyPlugin.GetMidRangeAsync(guid);
                writelog($"[DeviceManagerPlugin] [Speaker] GetMidRangeAsync succeeded, value is {result.ToString()}");
                return result;
            }
            catch (Exception ex)
            {
                writelog($"[DeviceManagerPlugin] [Speaker] GetMidRangeAsync failed for {guid} - Exception: {ex.Message}");
                return -1;
            }
        }

        public async Task<int> GetTrebleAsync(string guid)
        {
            try
            {
                var result = await _DTPProxyPlugin.GetTrebleAsync(guid);
                writelog($"[DeviceManagerPlugin] [Speaker] GetTrebleAsync succeeded, value is {result.ToString()}");
                return result;
            }
            catch (Exception ex)
            {
                writelog($"[DeviceManagerPlugin] [Speaker] GetTrebleAsync failed for {guid} - Exception: {ex.Message}");
                return -1;
            }
        }

        public async Task<bool> GetIsWiredAudioMicMuteSoundEnableAsync(string guid)
        {
            try
            {
                var result = await _DTPProxyPlugin.GetIsWiredAudioMicMuteSoundEnableAsync(guid);
                writelog($"[DeviceManagerPlugin] [Speaker] GetIsWiredAudioMicMuteSoundEnableAsync succeeded, value is {result.ToString()}");
                return result;
            }
            catch (Exception ex)
            {
                writelog($"[DeviceManagerPlugin] [Speaker] GetIsWiredAudioMicMuteSoundEnableAsync failed for {guid} - Exception: {ex.Message}");
                return false;
            }
        }

        public async Task<int> GetWiredAudioVolumeAdjustmentToneAsync(string guid)
        {
            try
            {
                var result = await _DTPProxyPlugin.GetWiredAudioVolumeAdjustmentToneAsync(guid);
                writelog($"[DeviceManagerPlugin] [Speaker] GetWiredAudioVolumeAdjustmentToneAsync succeeded, value is {result.ToString()}");
                return result;
            }
            catch (Exception ex)
            {
                writelog($"[DeviceManagerPlugin] [Speaker] GetWiredAudioVolumeAdjustmentToneAsync failed for {guid} - Exception: {ex.Message}");
                return -1;
            }
        }

        public async Task<bool> GetIsWiredAudioIMicNSEnableAsync(string guid)
        {
            try
            {
                var result = await _DTPProxyPlugin.GetIsWiredAudioIMicNSEnableAsync(guid);
                writelog($"[DeviceManagerPlugin] [Speaker] GetIsWiredAudioIMicNSEnableAsync succeeded, value is {result.ToString()}");
                return result;
            }
            catch (Exception ex)
            {
                writelog($"[DeviceManagerPlugin] [Speaker] GetIsWiredAudioIMicNSEnableAsync failed for {guid} - Exception: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> GetIsAudioEqualizerSupportedAsync(string guid)
        {
            try
            {
                var result = await _DTPProxyPlugin.GetIsAudioEqualizerSupportedAsync(guid);
                writelog($"[DeviceManagerPlugin] [Speaker] GetIsAudioEqualizerSupportedAsync succeeded, value is {result.ToString()}");
                return result;
            }
            catch (Exception ex)
            {
                writelog($"[DeviceManagerPlugin] [Speaker] GetIsAudioEqualizerSupportedAsync failed for {guid} - Exception: {ex.Message}");
                return false;
            }
        }

        #endregion

        #region Dongle

        public async Task<string> GetFirmwareVersionAsyncForDongle(string guid)
        {
            try
            {
                var result = await _DTPProxyPlugin.GetFirmwareVersionAsyncForDongle(guid);
                writelog($"[DeviceManagerPlugin] [Dongle] GetFirmwareVersionAsyncForDongle succeeded, value is {result.ToString()}");
                return result;
            }
            catch (Exception ex)
            {
                writelog($"[DeviceManagerPlugin] [Dongle] GetFirmwareVersionAsyncForDongle failed for {guid} - Exception: {ex.Message}");
                return null;
            }
        }

        public async Task<string> GetConnectedDeviceInfoAsyncForDongle(string guid)
        {
            try
            {
                var result = await _DTPProxyPlugin.GetConnectedDeviceInfoAsyncForDongle(guid);
                writelog($"[DeviceManagerPlugin] [Dongle] GetConnectedDeviceInfoAsyncForDongle succeeded, value is {result.ToString()}");
                return result;
            }
            catch (Exception ex)
            {
                writelog($"[DeviceManagerPlugin] [Dongle] GetConnectedDeviceInfoAsyncForDongle failed for {guid} - Exception: {ex.Message}");
                return null;
            }
        }

        public async Task<string> GetDeviceIdAsyncForDongle(string guid)
        {
            try
            {
                var result = await _DTPProxyPlugin.GetDeviceIdAsyncForDongle(guid);
                writelog($"[DeviceManagerPlugin] [Dongle] GetDeviceIdAsyncForDongle succeeded, value is {result.ToString()}");
                return result;
            }
            catch (Exception ex)
            {
                writelog($"[DeviceManagerPlugin] [Dongle] GetDeviceIdAsyncForDongle failed for {guid} - Exception: {ex.Message}");
                return null;
            }
        }

        public async Task<string> GetPluginIdAsyncForDongle(string guid)
        {
            try
            {
                var result = await _DTPProxyPlugin.GetPluginIdAsyncForDongle(guid);
                writelog($"[DeviceManagerPlugin] [Dongle] GetPluginIdAsyncForDongle succeeded, value is {result.ToString()}");
                return result;
            }
            catch (Exception ex)
            {
                writelog($"[DeviceManagerPlugin] [Dongle] GetPluginIdAsyncForDongle failed for {guid} - Exception: {ex.Message}");
                return null;
            }
        }

        public async Task<JArray> GetDeviceItemsExAsyncForDongle(string guid)
        {
            try
            {
                var result = await _DTPProxyPlugin.GetDeviceItemsExAsyncForDongle(guid);
                writelog($"[DeviceManagerPlugin] [Dongle] GetDeviceItemsExAsyncForDongle succeeded, value is {result.ToString()}");
                return result;
            }
            catch (Exception ex)
            {
                writelog($"[DeviceManagerPlugin] [Dongle] GetDeviceItemsExAsyncForDongle failed for {guid} - Exception: {ex.Message}");
                return null;
            }
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

        #region display properties implementation

        public Task<DisplayPropertiesInfo> GetDisplayPropertiesInfo(MonitorInfo monitorInfos)
        {
            DisplayPropertiesInfo ret = new DisplayPropertiesInfo();
            if (_DisplayManagerPlugin != null)
            {
                ret = _DisplayManagerPlugin.GetDisplayPropertiesInfo(monitorInfos).Result;
            }
            return Task.FromResult(ret);
        }

        public Task<bool> SetDisplayPropertiest(MonitorInfo monitorInfos, DDPM.SA.Common.Properties properties, DisplayOrientation orientation)
        {
            bool ret = false;
            if (_DisplayManagerPlugin != null)
            {
                ret = _DisplayManagerPlugin.SetDisplayPropertiest(monitorInfos, properties, orientation).Result;
            }
            return Task.FromResult(ret);
        }

        public Task<bool> SetResolutions(MonitorInfo monitorInfo, Properties properties)
        {
            bool ret = false;
            if (_DisplayManagerPlugin != null)
            {
                displayInOut = false;
                ret = _DisplayManagerPlugin.SetResolutions(monitorInfo, properties).Result;
                //Telementry Collection
                var rt = false;
                var Displaysettings_Function = new Displaysettings_Function();
                rt = Displaysettings_Function.Send_GamingRefreshRate_Telementry(_TelementryScheduler, monitorInfo, properties.Frequency.ToString(), GetMonitorCurrentResolution(monitorInfo), GetMonitorMaxResolution(monitorInfo));
                if (rt) writelog("[DeviceMangerPlugin] Send Telementry for GamingRefreshRate Success ...");
                else writelog("[DeviceMangerPlugin] Send Telementry for GamingRefreshRate Fail ...");
            }
            return Task.FromResult(ret);
        }

        public Task<bool> SetOrientation(MonitorInfo monitorInfo, DisplayOrientation orientation)
        {
            bool ret = false;
            if (_DisplayManagerPlugin != null)
            {
                ret = _DisplayManagerPlugin.SetOrientation(monitorInfo, orientation).Result;
            }
            return Task.FromResult(ret);
        }

        public Task<bool> CallWindowsDisplaySetting()
        {
            bool ret = false;
            if (_DisplayManagerPlugin != null)
            {
                ret = _DisplayManagerPlugin.CallWindowsDisplaySetting().Result;
            }
            return Task.FromResult(ret);
        }

        public Task<bool> GetHDRStatus(MonitorInfo monitorInfo)
        {
            bool ret = false;
            return Task.FromResult(_DisplayManagerPlugin.GetHDRStatus(monitorInfo).Result);
        }

        public Task<bool> SetHDRStatus(MonitorInfo monitorInfo, bool onoff)
        {
            bool ret = false;
            return Task.FromResult(_DisplayManagerPlugin.SetHDRStatus(monitorInfo, onoff).Result);
        }

        public Task<bool> SetUSBCPrioritizationType(MonitorInfo monitorInfo, USBCPrioritizationType type)
        {
            bool ret = false;
            return Task.FromResult(_DisplayManagerPlugin.SetUSBCPrioritizationType(monitorInfo, type).Result);
        }

        //0606 Bruce 新增鎖定自動旋轉方向
        public Task<bool> LockRotate(bool onoff)
        {
            bool ret = false;
            DDPMSettings config = _SettingsPlugin.ReloadAppConfigData().Result;
            if (config != null && _DisplayManagerPlugin != null)
            {
                config.UserSettings.LockRotate = onoff;
                _DisplayManagerPlugin.SetEnableLockOrientation(onoff);
                ret = _SettingsPlugin.SetAppConfigData(config).Result;
            }

            Task.Run(() =>
            {
                //Telementry Collection
                var rt = false;
                var ApplicationSettings_Function = new ApplicationSettings_Function();
                writelog("[DeviceMangerPlugin] Send Telementry for LockRotation...");
                rt = ApplicationSettings_Function.Send_LockRotation_Telementry(_TelementryScheduler, _AllInfoMonitors, onoff);
                if (rt) writelog("[DeviceMangerPlugin] Send Telementry for LockRotation Success ...");
                else writelog("[DeviceMangerPlugin] Send Telementry for LockRotation Fail ...");
            }).ConfigureAwait(false);

            return Task.FromResult(ret);
        }

        //0606 Bruce 新增鎖定自動旋轉方向
        public Task<bool> GetLockRotateStatus()
        {
            bool ret = false;
            try
            {
                if (_SettingsPlugin != null && _DisplayManagerPlugin != null)
                {
                    DDPMSettings config = _SettingsPlugin.ReloadAppConfigData().Result;
                    _DisplayManagerPlugin.SetEnableLockOrientation(config.UserSettings.LockRotate);
                    ret = config.UserSettings.LockRotate;
                }
            }
            catch
            {
            }
            return Task.FromResult(ret);
        }

        public Task<string> GetOSDOrientation(MonitorInfo monitorInfo)
        {
            string ret = "";
            if (_DisplayManagerPlugin != null)
            {
                ret = _DisplayManagerPlugin.GetOSDOrientation(monitorInfo).Result;
            }
            return Task.FromResult(ret);
        }

        public Task<bool?> SetOSDOrientation(MonitorInfo monitorInfo, string orientation)
        {
            bool? ret = null;
            if (_DisplayManagerPlugin != null)
            {
                ret = _DisplayManagerPlugin.SetOSDOrientation(monitorInfo, orientation).Result;
            }
            return Task.FromResult(ret);
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

        public Task<object> ReadRegistryData(RegistryHive hive, string keyPath, string keyName)
        {
            object settings = _SettingsPlugin.ReadRegistryData(hive, keyPath, keyName).Result;
            return Task.FromResult(settings);
        }

        public Task<bool> WriteRegistryData(RegistryHive hive, string keyPath, string keyName, object value)
        {
            bool settings = _SettingsPlugin.WriteRegistryData(hive, keyPath, keyName, value).Result;
            return Task.FromResult(settings);
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
            if (b)
            {
                //Robert_Lin, 2024-10-27 send Telemetry PIPPBP
                SendPipPbpTelemetry("0x21", monitorInfo);
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
            if (b)
            {
                //Robert_Lin, 2024-10-27 send Telemetry PIPPBP
                SendPipPbpTelemetry("0x22", monitorInfo);
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
            if (b)
            {
                //Robert_Lin, 2024-10-27 send Telemetry PIPPBP
                SendPipPbpTelemetry($"0x{modeCode:X}", monitorInfo);
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

        #region FW Update implementation

        public Task<FWUpdateInfoPackage> GetFWUpdateInfo(bool isShowNotify = true, bool isForce = false, bool isDefer = false, List<DeviceType> deviceTypeList = null, bool UODMode = false, bool isOnlyDisplay = false, bool reScan = true, bool isUITrigger = false, List<string> giuds = null, List<string> serviceTags = null, List<string> models = null, string minVersion = "")
        {
            if (_PeripheralsPlugin != null && _FWUpdatePlugin != null && _DisplayManagerPlugin != null && _SettingsPlugin != null)
            {
                UpdateHelper updateHelper = _PeripheralsPlugin.GetFWUpdateInfo().Result;
                if (updateHelper == null || updateHelper.UpdateItems == null)
                {
                    updateHelper = new UpdateHelper();
                    updateHelper.UpdateItems = new List<UpdateItemInfo>();
                }
                List<DeviceInfo> deviceInfos = _PeripheralsPlugin.GetDevices().Result.deviceInfo;
                if (deviceInfos == null)
                {
                    deviceInfos = new List<DeviceInfo>();
                }
                DisplayUpdateHelper displayUpdateHelper = _DisplayManagerPlugin.GetDisplayFWUpdate(_IsSkipCA, _SettingsPlugin).Result;
                if (displayUpdateHelper == null || displayUpdateHelper.Firmwares == null)
                {
                    displayUpdateHelper = new DisplayUpdateHelper();
                    displayUpdateHelper.Firmwares = new List<Display_Firmwares_item>();
                }

                //0612 Bruce 將傳入值null移除因已不需使用，不會影響UI和CLI
                return Task.FromResult(_FWUpdatePlugin.GetFWUpdateInfo(updateHelper, deviceInfos, isShowNotify, isForce, isDefer, deviceTypeList, UODMode, displayUpdateHelper, isOnlyDisplay, reScan, isUITrigger, giuds, serviceTags, models, minVersion).Result);
            }
            return Task.FromResult(new FWUpdateInfoPackage());
        }

        public Task<List<FWUpdateInfo>> DownloadAndInstall(List<FWUpdateInfo> fwUpdateInfos, bool isUITrigger = false, string installPath = "")
        {
            _UpdateProgress = null;
            SetDelayFWUpdateInfoPackage();
            if (isUITrigger)
            {
                CallUpdateProgressUI().Wait();
            }
            List<FWUpdateInfo> tmpFWUpdateInfos = _FWUpdatePlugin.DownloadAndInstall(fwUpdateInfos, isUITrigger, installPath).Result;
            if (_UpdateProgress != null)
            {
                _FWUpdatePlugin.ProgressUpdate_Notify -= _UpdateProgress._FWUpdatePlugin_ProgressUpdate;
                _UpdateProgress.CloseWindow();
                _UpdateProgress = null;
            }
            //Telementry Collection
            var rt = false;
            var ApplicationSettings_Function = new DeviceFirmware_Functions();
            List<FWUpdateInfo> DisplayList = tmpFWUpdateInfos.FindAll(o => o.IsDisplay);
            List<FWUpdateInfo> PeripheralsList = tmpFWUpdateInfos.FindAll(o => o.IsDisplay == false);
            if (DisplayList != null && DisplayList.Count > 0)
            {
                writelog("[DeviceMangerPlugin] Send Telementry for DisplayDeviceFirmware...");
                rt = ApplicationSettings_Function.Send_DisplayDeviceFirmware_Telementry(_TelementryScheduler, DisplayList);
                if (rt) writelog("[DeviceMangerPlugin] Send Telementry for DisplayDeviceFirmware Success ...");
                else writelog("[DeviceMangerPlugin] Send Telementry for DisplayDeviceFirmware Fail ...");
            }
            if (PeripheralsList != null && PeripheralsList.Count > 0)
            {
                rt = false;
                writelog("[DeviceMangerPlugin] Send Telementry for PeripheralsDeviceFirmware...");
                rt = ApplicationSettings_Function.Send_PeripheralsDeviceFirmware_Telementry(_TelementryScheduler, PeripheralsList);
                if (rt) writelog("[DeviceMangerPlugin] Send Telementry for PeripheralsDeviceFirmware Success ...");
                else writelog("[DeviceMangerPlugin] Send Telementry for PeripheralsDeviceFirmware Fail ...");
            }
            return Task.FromResult(tmpFWUpdateInfos);
        }

        public Task<FWUErrorCode> Install(string installPath, bool isOnlyDisplay = false)
        {
            FWUErrorCode ret = FWUErrorCode.Unknow;
            SetDelayFWUpdateInfoPackage();
            //if (_UpdateProgress != null)
            //{
            ret = _FWUpdatePlugin.Install(installPath, isOnlyDisplay).Result;
            //}
            return Task.FromResult(ret);
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

        public Task<bool> SetSkipCA(bool isSkipCA)
        {
            bool ret = false;
            string isSkipCA_int = isSkipCA ? "1" : "0";
            ret = WriteRegistryData(RegistryHive.LocalMachine, @"SOFTWARE\Dell\DDPM Subagent", "SkipCA", isSkipCA_int).Result;
            writelog($"[SetSkipCA], ret={ret}.");
            if (ret)
            {
                _IsSkipCA = isSkipCA;
                if (_FWUpdatePlugin != null)
                {
                    _FWUpdatePlugin.SetSkipCA(_IsSkipCA);
                }
                if (_SWUpdatePlugin != null)
                {
                    _SWUpdatePlugin.SetSkipCA(_IsSkipCA);
                }
            }
            return Task.FromResult(ret);
        }

        public Task<bool> GetSkipCA()
        {
            object o = ReadRegistryData(RegistryHive.LocalMachine, @"SOFTWARE\Dell\DDPM Subagent", "SkipCA").Result;
            writelog($"[GetSkipCA], o={o}.");
            if (o != null && o is string && !string.IsNullOrEmpty(o.ToString()))
            {
                _IsSkipCA = o.ToString().Equals("1") ? true : false;
                if (_FWUpdatePlugin != null)
                {
                    _FWUpdatePlugin.SetSkipCA(_IsSkipCA);
                }
                if (_SWUpdatePlugin != null)
                {
                    _SWUpdatePlugin.SetSkipCA(_IsSkipCA);
                }
            }
            return Task.FromResult(_IsSkipCA);
        }

        public Task<bool> SetServerURL(string url)
        {
            bool ret = false;
            writelog($"{nameof(SetServerURL)} start");
            if (!string.IsNullOrEmpty(url))
            {
                if (url.ToUpper() == "ON")
                {
                    string KeyPath = @"SOFTWARE\Dell\DDPM Subagent";
                    string KeyName = @"TestServerURL";
                    string url2 = "https://clientperipherals.dell.com/DDPM/3fcf51beb3c8/";
                    ret = WriteRegistryData(RegistryHive.LocalMachine, KeyPath, KeyName, url2).Result;
                    writelog($"{nameof(SetServerURL)} DDPM Subagent Ret:{ret}");
                    KeyPath = @"SOFTWARE\Dell\Dell Display Manager";
                    ret = WriteRegistryData(RegistryHive.LocalMachine, KeyPath, KeyName, url2).Result && ret;
                    writelog($"{nameof(SetServerURL)} Dell Display Manager Ret:{ret}");
                    if (_FWUpdatePlugin != null)
                    {
                        ret = _FWUpdatePlugin.RestartService().Result && ret;
                        writelog($"{nameof(SetServerURL)} Restart Service Ret:{ret}");
                    }
                }
                else if (url.ToUpper() == "OFF")
                {
                    string KeyPath = @"SOFTWARE\Dell\DDPM Subagent";
                    string KeyName = @"TestServerURL";
                    string url2 = "";
                    ret = WriteRegistryData(RegistryHive.LocalMachine, KeyPath, KeyName, url2).Result;
                    writelog($"{nameof(SetServerURL)} DDPM Subagent Ret:{ret}");
                    KeyPath = @"SOFTWARE\Dell\Dell Display Manager";
                    ret = WriteRegistryData(RegistryHive.LocalMachine, KeyPath, KeyName, url2).Result && ret;
                    writelog($"{nameof(SetServerURL)} Dell Display Manager Ret:{ret}");
                    if (_FWUpdatePlugin != null)
                    {
                        ret = _FWUpdatePlugin.RestartService().Result && ret;
                        writelog($"{nameof(SetServerURL)} Restart Service Ret:{ret}");
                    }
                }
                else
                {
                    writelog($"{nameof(url)} is valid");
                    string KeyPath = @"SOFTWARE\Dell\DDPM Subagent";
                    string KeyName = @"TestServerURL";
                    ret = WriteRegistryData(RegistryHive.LocalMachine, KeyPath, KeyName, url).Result;
                    writelog($"{nameof(SetServerURL)} DDPM Subagent Ret:{ret}");
                    KeyPath = @"SOFTWARE\Dell\Dell Display Manager";
                    ret = WriteRegistryData(RegistryHive.LocalMachine, KeyPath, KeyName, url).Result && ret;
                    writelog($"{nameof(SetServerURL)} Dell Display Manager Ret:{ret}");
                    if (_FWUpdatePlugin != null)
                    {
                        ret = _FWUpdatePlugin.RestartService().Result && ret;
                        writelog($"{nameof(SetServerURL)} Restart Service Ret:{ret}");
                    }
                }
            }
            writelog($"{nameof(SetServerURL)} done");
            return Task.FromResult(ret);
        }

        public Task<string> GetServerURL()
        {
            bool ret = false;
            string URL = null;
            writelog($"{nameof(SetServerURL)} start");
            //if (!string.IsNullOrEmpty(url))
            //{
            //writelog($"{nameof(url)} is valid");
            string KeyPath = @"SOFTWARE\Dell\DDPM Subagent";
            string KeyName = @"TestServerURL";
            object o = ReadRegistryData(RegistryHive.LocalMachine, KeyPath, KeyName).Result;
            //writelog($"{nameof(SetServerURL)} DDPM Subagent Ret:{ret}");
            KeyPath = @"SOFTWARE\Dell\Dell Display Manager";
            object o_ = ReadRegistryData(RegistryHive.LocalMachine, KeyPath, KeyName).Result;
            //writelog($"{nameof(SetServerURL)} Dell Display Manager Ret:{ret}");
            //if (_FWUpdatePlugin != null)
            //{
            //    ret = _FWUpdatePlugin.RestartService().Result && ret;
            //    writelog($"{nameof(SetServerURL)} Restart Service Ret:{ret}");
            //}
            //}
            writelog($"{nameof(SetServerURL)} done");
            return Task.FromResult(o.ToString());
        }

        public Task<bool> CallDDPMUI(string DDPMPath)
        {
            writelog($"{nameof(CallDDPMUI)} start");
            bool ret = false;
            if (!string.IsNullOrEmpty(DDPMPath))
            {
                try
                {
                    writelog($"CloseDDPM start");
                    string processName = "DDPM";
                    Process[] processes = Process.GetProcessesByName(processName);
                    writelog($"CloseDDPM processes.Length {processes.Length}");
                    if (processes.Length > 0)
                    {
                        foreach (Process process in processes)
                        {
                            // Close process by sending a close message to its main window.
                            process.CloseMainWindow();
                            // Free resources associated with process.
                            process.Close();
                        }
                    }
                    writelog($"CloseDDPM done");
                }
                catch (Exception ex)
                {
                    writelog($"CloseDDPM Error:{ex.Message}");
                }
                Thread.Sleep(5000);
                try
                {
                    writelog($"RunDDPM start");
                    Process.Start(DDPMPath + "\\DDPM.exe");
                    writelog($"RunDDPM done");
                }
                catch (Exception ex)
                {
                    writelog($"RunDDPM Error:{ex.Message}");
                }
            }
            writelog($"{nameof(CallDDPMUI)} done");
            return Task.FromResult(ret);
        }
        private Task<bool> SetFWUpdateInfoPackage(FWUpdateInfoPackage fwUpdateInfoPackage)
        {
            if (_SettingsPlugin != null)
            {
                DDPMSettings config = _SettingsPlugin.ReloadAppConfigData().Result;
                if (config != null)
                {
                    foreach (FWUpdateInfo updateInfo in fwUpdateInfoPackage.FWUpdateInfo)
                    {
                        updateInfo.ServerPath = "";
                        updateInfo.SHA256 = "";
                        //updateInfo.SHA512 = "";
                        updateInfo.Thumbprint = "";
                    }
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
            UpdateHelper updateHelper;
            DisplayUpdateHelper displayUpdateHelper;
            List<DeviceInfo> deviceInfos;
            try
            {
                updateHelper = _PeripheralsPlugin.GetFWUpdateInfo().Result;
                if (updateHelper == null || updateHelper.UpdateItems == null)
                {
                    updateHelper = new UpdateHelper();
                    updateHelper.UpdateItems = new List<UpdateItemInfo>();
                }
                deviceInfos = _PeripheralsPlugin.GetDevices().Result.deviceInfo;
                if (deviceInfos == null)
                {
                    deviceInfos = new List<DeviceInfo>();
                }
            }
            catch (Exception ex)
            {
                writelog($"{nameof(CheckUpdate)} GetFWUpdateInfo Error:{ex.Message}");
                return Task.FromResult(false);
            }
            if (_DisplayManagerPlugin == null)
                return Task.FromResult(false);
            if (_SettingsPlugin == null)
            {
                return Task.FromResult(false);
            }
            try
            {
                displayUpdateHelper = _DisplayManagerPlugin.GetDisplayFWUpdate(_IsSkipCA, _SettingsPlugin).Result;
                if (displayUpdateHelper == null || displayUpdateHelper.Firmwares == null)
                {
                    displayUpdateHelper = new DisplayUpdateHelper();
                    displayUpdateHelper.Firmwares = new List<Display_Firmwares_item>();
                }
            }
            catch (Exception ex)
            {
                writelog($"{nameof(CheckUpdate)} GetDisplayFWUpdate Error:{ex.Message}");
                return Task.FromResult(false);
            }

            if (_FWUpdatePlugin == null)
                return Task.FromResult(false);
            try
            {
                SetDelayFWUpdateInfoPackage();
                FWUpdateInfoPackage fwUpdateInfos = _FWUpdatePlugin.GetFWUpdateInfo(updateHelper, deviceInfos, true, false, false, null, false, displayUpdateHelper, false, true, false, null, null, null, "").Result;
                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                writelog($"{nameof(CheckUpdate)} _FWUpdatePlugin.CheckUpdate Error:{ex.Message}");
                return Task.FromResult(false);
            }
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
                if (config != null && config.UserSettings != null)
                {
                    _FWUpdatePlugin.SetDelayFWUpdateInfoPackage(config.UserSettings.DelayFWUpdateInfoPackage);
                }
                else
                {
                    writelog("[SetDelayFWUpdateInfoPackage], ReloadAppConfigData is null.");
                }
            }
        }

        private Task CallUpdateProgressUI()
        {
            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
            Thread thread1 = new Thread(() =>
            {
                _UpdateProgress = new UpdateProgress();
                _UpdateProgress.Closed += (sender2, e2) =>
                {
                    _UpdateProgress.Dispatcher.InvokeShutdown();
                };
                _UpdateProgress.Dispatcher.Invoke(() => _UpdateProgress.Show());
                _FWUpdatePlugin.ProgressUpdate_Notify += _UpdateProgress._FWUpdatePlugin_ProgressUpdate;
                tcs.SetResult(true);
                Dispatcher.Run();
            });
            thread1.SetApartmentState(ApartmentState.STA);
            thread1.Start();
            return tcs.Task;
        }

        private void CallOSD(object o, (string, string, bool) args)
        {
            ShowOSD(Screen.PrimaryScreen.DeviceName, OSDType.Error, false, args);
        }

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
                Task.Run(async () =>
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
                            toastContentBuilder.AddButton("Update now", ToastActivationType.Background, "Update " + popupContentPackage.PopupType.ToString());
                            toastContentBuilder.AddButton("Defer", ToastActivationType.Background, "Delay");
                        }
                        else
                        {
                            toastContentBuilder.AddButton("Ok", ToastActivationType.Background, "Update");
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
                    Thread.Sleep(5000);
                    if (!userClosedPopup)
                    {
                        writelog("[CallPopup], is no user closed popup.");
                        if (!isInfo)
                        {
                            if (!isOnlyUpdate)
                            {
                                writelog("[CallPopup], go to DelayEvent.");
                                DelayEvent(this, popupContentPackage.PopupType.ToString());
                            }
                            else
                            {
                                writelog("[CallPopup], go to UpdateEvent.");
                                UpdateEvent(this, popupContentPackage.PopupType.ToString());
                            }
                        }
                    }
                });
            }
        }

        private void CheckInput(ToastNotificationActivatedEventArgsCompat e)
        {
            string[] ret = e.Argument.Split(" ");
            if (ret.Length >= 2)
            {
                userClosedPopup = true;
                if (e.Argument.StartsWith("Update"))
                {
                    UpdateEvent(this, ret[1]);
                }
                else if (e.Argument.StartsWith("Delay"))
                {
                    DelayEvent(this, ret[1]);
                }
            }
            else if (ret.Length == 1)
            {
                if (e.Argument.StartsWith("left_btn"))
                {
                    writelog("*** left_button_action");
                }
                else if (e.Argument.StartsWith("right_btn"))
                {
                    writelog("*** right_button_action");
                }
            }
        }

        private void UpdateEvent(object o, string ob)
        {
            writelog($"[UpdateEvent],{ob} start.");
            ////// 將 e 轉換成 JSON 字串
            ////string json = JsonConvert.SerializeObject(ob);
            //// 將 JSON 字串轉換成 FWUpdateInfoPackage 對象
            //FWUpdateInfoPackage fWUpdateInfoPackage = JsonConvert.DeserializeObject<FWUpdateInfoPackage>(ob.ToString());
            //// 將 JSON 字串轉換成 SWUpdateInfoPackage 對象
            //SWUpdateInfoPackage sWUpdateInfoPackage = JsonConvert.DeserializeObject<SWUpdateInfoPackage>(ob.ToString());
            if (!string.IsNullOrEmpty(ob))
            {
                if (ob.Equals(PopupContentPackage_Enum.SWU.ToString()))
                {
                    if (_SWUpdatePlugin != null)
                    {
                        writelog("[UpdateEvent], go to _SWUpdatePlugin.UpdateEvent.");
                        _SWUpdatePlugin.UpdateEvent();
                    }
                }
                else
                {
                    if (_FWUpdatePlugin != null)
                    {
                        writelog("[UpdateEvent], go to _FWUpdatePlugin.UpdateEvent.");
                        List<FWUpdateInfo> fWUpdateInfos = new List<FWUpdateInfo>();
                        fWUpdateInfos = _FWUpdatePlugin.UpdateEvent().Result;
                        List<FWUpdateInfo> DisplayList = fWUpdateInfos.FindAll(o => o.IsDisplay);
                        List<FWUpdateInfo> PeripheralsList = fWUpdateInfos.FindAll(o => o.IsDisplay == false);
                        //Telementry Collection
                        var rt = false;
                        var ApplicationSettings_Function = new DeviceFirmware_Functions();
                        if (DisplayList != null && DisplayList.Count > 0)
                        {
                            writelog("[DeviceMangerPlugin] Send Telementry for DisplayDeviceFirmware...");
                            rt = ApplicationSettings_Function.Send_DisplayDeviceFirmware_Telementry(_TelementryScheduler, DisplayList);
                            if (rt) writelog("[DeviceMangerPlugin] Send Telementry for DisplayDeviceFirmware Success ...");
                            else writelog("[DeviceMangerPlugin] Send Telementry for DisplayDeviceFirmware Fail ...");
                        }
                        if (PeripheralsList != null && PeripheralsList.Count > 0)
                        {
                            rt = false;
                            writelog("[DeviceMangerPlugin] Send Telementry for PeripheralsDeviceFirmware...");
                            rt = ApplicationSettings_Function.Send_PeripheralsDeviceFirmware_Telementry(_TelementryScheduler, PeripheralsList);
                            if (rt) writelog("[DeviceMangerPlugin] Send Telementry for PeripheralsDeviceFirmware Success ...");
                            else writelog("[DeviceMangerPlugin] Send Telementry for PeripheralsDeviceFirmware Fail ...");
                        }
                    }
                }
            }
            writelog($"[UpdateEvent],{ob} done.");
        }

        private void DelayEvent(object o, string ob)
        {
            writelog($"[DelayEvent],{ob} start.");
            ////// 將 e 轉換成 JSON 字串
            ////string json = JsonConvert.SerializeObject(ob);
            //// 將 JSON 字串轉換成 FWUpdateInfoPackage 對象
            //FWUpdateInfoPackage fWUpdateInfoPackage = JsonConvert.DeserializeObject<FWUpdateInfoPackage>(ob.ToString());
            //// 將 JSON 字串轉換成 SWUpdateInfoPackage 對象
            //SWUpdateInfoPackage sWUpdateInfoPackage = JsonConvert.DeserializeObject<SWUpdateInfoPackage>(ob.ToString());
            if (!string.IsNullOrEmpty(ob))
            {
                if (ob.Equals(PopupContentPackage_Enum.SWU.ToString()))
                {
                    if (_SWUpdatePlugin != null)
                    {
                        writelog("[DelayEvent], go to _SWUpdatePlugin.DelayEvent.");
                        _SWUpdatePlugin.DelayEvent();
                    }
                }
                else
                {
                    if (_FWUpdatePlugin != null)
                    {
                        writelog("[DelayEvent], go to _FWUpdatePlugin.DelayEvent.");
                        _FWUpdatePlugin.DelayEvent();
                    }
                }
            }
            writelog($"[DelayEvent],{ob} done.");
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
            if (inputList != null && subInputList != null)
            {
                if (GetOnUSBKVM(monitorInfo).Result)
                {
                    //get monitor settings
                    List<DDPMMonitorSettings> settings = _SettingsPlugin.ReloadMonitorSettings(monitorInfo.modelName).Result;
                    if (settings != null)
                    {
                        //get monitor setting
                        DDPMMonitorSettings monitorSetting = settings.Find(x => x.ServiceTag == monitorInfo.edid.ServiceTag);
                        if (monitorSetting == null)
                        {
                            USBKVMPCsList = _DisplayManagerPlugin.GetUSBKVMPCsList(monitorInfo, inputList, subInputList).Result;
                            //bool b = SetUSBKVMPCsList(monitorInfo, USBKVMPCsList).Result;
                        }
                        else
                        {
                            try
                            {
                                if (!string.IsNullOrEmpty(monitorSetting.KVM.strUSBKVMPCsList))
                                {
                                    USBKVMPCsList = USBKVMPCsListDeserialize(monitorSetting.KVM.strUSBKVMPCsList);
                                    if (USBKVMPCsList != null)
                                    {
                                        if (USBKVMPCsList.Count != 0)
                                        {
                                            foreach (var pc in USBKVMPCsList)
                                            {
                                                if (string.IsNullOrEmpty(pc.Key) || pc.Value == null)
                                                {
                                                    USBKVMPCsList = _DisplayManagerPlugin.GetUSBKVMPCsList(monitorInfo, inputList, subInputList).Result;
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    USBKVMPCsList = _DisplayManagerPlugin.GetUSBKVMPCsList(monitorInfo, inputList, subInputList).Result;
                                    //bool b = SetUSBKVMPCsList(monitorInfo, USBKVMPCsList).Result;
                                }
                            }
                            catch (Exception e)
                            {
                                USBKVMPCsList = _DisplayManagerPlugin.GetUSBKVMPCsList(monitorInfo, inputList, subInputList).Result;
                                //bool b = SetUSBKVMPCsList(monitorInfo, USBKVMPCsList).Result;
                            }
                        }
                    }
                }
                else
                {
                    USBKVMPCsList = _DisplayManagerPlugin.GetUSBKVMPCsList(monitorInfo, inputList, subInputList).Result;
                }
            }
            else
            {
                writelog("[GetUSBKVMPCsList]inputList or subInputList is null");
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
                if (monitorSetting != null)
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
                                if (isON)
                                {
                                    bool b = SentKVMtoTelementry(monitorInfo, "KVMMode", "USB").Result;
                                }
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

        public Task CreatNewNamedpipe()
        {
            if (_NKVMPlugin != null)
            {
                _NKVMPlugin.CreatNewNamedpipe();
            }
            return Task.CompletedTask;
        }

        public Task<bool> IsNamedpipeConnected()
        {
            if (_NKVMPlugin != null)
            {
                return Task.FromResult(_NKVMPlugin.IsNamedpipeConnected().Result);
            }
            return Task.FromResult(false);
        }

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
                if (monitorSetting != null)
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
                                //_NKVMPlugin.OnNKVM().Wait();
                                bool bt = SentKVMtoTelementry(monitorInfo, "KVMMode", "Network").Result;
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

        public Task NKVM_ChangeMonitorIndex(MonitorInfo monitorInfo)
        {
            if (_NKVMPlugin != null)
            {
                _NKVMPlugin.NKVM_ChangeMonitorIndex(monitorInfo);
            }
            return Task.CompletedTask;
        }

        public Task GetNKVMVersion()
        {
            if (_NKVMPlugin != null)
            {
                _NKVMPlugin.GetNKVMVersion();
            }
            return Task.CompletedTask;
        }

        public Task GetNKVMStatus()
        {
            if (_NKVMPlugin != null)
            {
                _NKVMPlugin.GetNKVMStatus();
            }
            return Task.CompletedTask;
        }

        public Task GetNKVMAutoConnect()
        {
            if (_NKVMPlugin != null)
            {
                _NKVMPlugin.GetNKVMAutoConnect();
            }
            return Task.CompletedTask;
        }

        public Task GetNKVMContentTransfer()
        {
            if (_NKVMPlugin != null)
            {
                _NKVMPlugin.GetNKVMContentTransfer();
            }
            return Task.CompletedTask;
        }

        public Task GetNKVMIncommingPort()
        {
            if (_NKVMPlugin != null)
            {
                _NKVMPlugin.GetNKVMIncommingPort();
            }
            return Task.CompletedTask;
        }

        public Task GetNKVMOutgoingPort()
        {
            if (_NKVMPlugin != null)
            {
                _NKVMPlugin.GetNKVMOutgoingPort();
            }
            return Task.CompletedTask;
        }

        public Task GetNKVMContentTransferPort()
        {
            if (_NKVMPlugin != null)
            {
                _NKVMPlugin.GetNKVMContentTransferPort();
            }
            return Task.CompletedTask;
        }

        public Task GetNKVMSettings()
        {
            if (_NKVMPlugin != null)
            {
                _NKVMPlugin.GetNKVMSettings();
            }
            return Task.CompletedTask;
        }

        public Task NKVM_State(bool state)
        {
            if (_NKVMPlugin != null)
            {
                _NKVMPlugin.NKVM_State(state);
            }
            return Task.CompletedTask;
        }

        public Task CallNKVMConnent()
        {
            if (_NKVMPlugin != null)
            {
                _NKVMPlugin.CallNKVMConnent();
            }
            return Task.CompletedTask;
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

        /// <summary>
        /// Set current WorkSplit (selected layout).
        /// EAPlugin will show the new WorkSpit layout on the target "Screen" and autofade-out.
        /// This method will not save to settings file, please use WriteEAMonitorSettings() to
        /// save new per-monitor settings.
        /// </summary>
        /// <param name="monitorInfo">The target monitor, EAPlugin will use this to find the target "Screen"</param>
        /// <param name="cellCount"></param>
        /// <param name="splitKey"></param>
        /// <param name="settings"></param>
        /// <returns>Always true unless DisplayManager is not ready</returns>
        public Task<bool> SetEAWrokSplit(MonitorInfo monitorInfo, int cellCount, char splitKey, List<double>? settings)
        {
            if (_DisplayManagerPlugin != null)
            {
                _DisplayManagerPlugin.SetEAWrokSplit(monitorInfo, cellCount, splitKey, settings);
                //Telemetry
                SendEasyArrangeTelemetry("Change_layout");
            }
            return Task.FromResult(false);
        }

        //Robert_Lin, 2024-9-13 Remove unused interfaces
        //public Task<bool> RequestEditSplit(MonitorInfo monitorInfo, int cellCount, char splitKey, string customName, List<double>? settings = null)
        //{
        //    if (_DisplayManagerPlugin != null)
        //    {
        //        return _DisplayManagerPlugin.RequestEditSplit(monitorInfo, cellCount, splitKey, customName, settings);
        //    }
        //    return Task.FromResult(false);
        //}

        //public Task<string> WriteEasyArrangeSettings(EAMonitorSettings eaMonitorSettings)
        //{
        //    if (_SettingsPlugin == null)
        //    {
        //        string err = "SettingsPlugin is null.";
        //        writelog($"WriteEasyArrangeSettings(), {err}");
        //        return Task.FromResult(err);
        //    }
        //    return _SettingsPlugin.WriteEasyArrangeSettings(eaMonitorSettings);
        //}

        //public Task<EAMonitorSettings> ReadEasyArrangeSettings(string monitorModel, string serialNumber)
        //{
        //    if (_SettingsPlugin == null)
        //    {
        //        string err = "SettingsPlugin is null.";
        //        writelog($"WriteEasyArrangeSettings(), {err}");
        //        return Task.FromResult<EAMonitorSettings>(null);
        //    }
        //    return _SettingsPlugin.ReadEasyArrangeSettings(monitorModel, serialNumber);
        //}

        //Robert_Lin, 2024-9-13 Remove unused interfaces
        //private void _DisplayManagerPlugin_EAEditCompleted(object sender, string e)
        //{
        //    if (EAEditCompleted != null)
        //    {
        //        Task.Run(() => EAEditCompleted.Invoke(this, e));
        //    }
        //}

        /// <summary>
        /// Notify to UI: The EAPlugin is enter the Edit stage. The Layout you specified in EAEditCommand()
        /// is under editing. By design, UI should minimized itself.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _DisplayManagerPlugin_EAEditStarted(object sender, string e)
        {
            if (EAEditStarted != null)
            {
                writelog("@ DeviceManaerPlugin._DisplayManagerPlugin_EAEditStarted(), Call to next handler.");
                Task.Run(() => EAEditStarted.Invoke(this, e));
            }
            else
            {
                writelog("@ DeviceManaerPlugin._DisplayManagerPlugin_EAEditStarted(), EAEditStarted is null.");
            }
        }

        //Robert_Lin, 2024-8-4 added
        /// <summary>
        /// Request from UI, to initiate a layout edit process.
        /// </summary>
        /// <param name="monitorInfo"></param>
        /// <param name="args">The arguments for the Edit command.</param>
        /// <returns></returns>
        public Task<bool> EAEditCommand(MonitorInfo monitorInfo, EAArgs args)
        {
            if (_DisplayManagerPlugin != null)
            {
                return _DisplayManagerPlugin.EAEditCommand(monitorInfo, args);
            }
            writelog("@ DeviceManaerPlugin.EAEditCommand(), _DisplayManagerPlugin is null.");
            return Task.FromResult(false);
        }

        /// <summary>
        /// Notify to UI, the EditCommand has been finished and return to UI.
        /// UI can get the return from EAArgs.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">The result of the edit command.
        /// UI can check if user finish the edit process by clicking "Save", or "Cancel"</param>
        private void _DisplayManagerPlugin_EAEditReturn(object sender, EAArgs e)
        {
            if (EAEditReturn != null)
            {
                writelog("@ DeviceManaerPlugin._DisplayManagerPlugin_EAEditReturn(), Call to next handler.");
                Task.Run(() => EAEditReturn.Invoke(this, e));

                //Only if Result==true will send Telemetry
                if (e.Result)
                    SendEasyArrangeTelemetry("Custom_Layout");
            }
            else
            {
                writelog("@ DeviceManaerPlugin._DisplayManagerPlugin_EAEditReturn(), EAEditReturn is null.");
            }
        }

        public Task<bool> WriteEAMonitorSettings(MonitorInfo monitorInfo, EAMonitorSettings eaSettings)
        {
            if (_SettingsPlugin == null)
            {
                writelog("@ WriteEAMonitorSettings: _SettingsPlugin is null.");
                return Task.FromResult(false);
            }

            //Keep the device ID for usage
            string model = monitorInfo.modelName;
            string serviceTag = monitorInfo.edid.ServiceTag;

            List<DDPMMonitorSettings> settings = _SettingsPlugin.ReloadMonitorSettings(model).Result;
            if (settings == null)
            {
                writelog($"@ WriteEAMonitorSettings: ReloadMonitorSettings(model={model}) return null.");
                return Task.FromResult(false);
            }

            //Find the previous saved device settings
            DDPMMonitorSettings? monitorSettings = settings.FirstOrDefault(x => x.ServiceTag.Equals(monitorInfo.edid.ServiceTag));
            //If not found => return error, GetAllMonitor() will init and create an initial settings instance for us
            if (monitorSettings == null)
            {
                writelog($"@ WriteEAMonitorSettings: Reloaded settings not contains (model={model}, serviceTage={serviceTag}).");
                return Task.FromResult(false);
            }

            monitorSettings.EA = eaSettings;

            if (_SettingsPlugin.WriteMonitorSettings(monitorInfo.modelName, settings).Result)
            {
                writelog($"@ WriteEAMonitorSettings(model={model}, serviceTage={serviceTag}) OK.");
                return Task.FromResult(true);
            }
            writelog($"@ WriteEAMonitorSettings: WriteMonitorSettings(model={model}, serviceTage={serviceTag}) failed.");
            return Task.FromResult(false);
        }

        //Robert_Lin, 2024-10-12 Note that CustomList has been moved to UserSettings
        //New added method: ReadEACustomList()
        public Task<EAMonitorSettings> ReadEAMonitorSettings(MonitorInfo monitorInfo)
        {
            //Create a default output
            EAMonitorSettings defaultOutput = new EAMonitorSettings();
            //Robert_Lin, 2024-10-10 to fix defaule list will return double items when deserialize json
            defaultOutput.RecentList = SplitJson.DefaultRecentList.ToArray();
            //_dump_SplitJsonList(monitorInfo, defaultOutput.RecentList.ToList<SplitJson>());

            if (_SettingsPlugin == null)
            {
                writelog("@ ReadEAMonitorSettings: _SettingsPlugin is null.");
                return Task.FromResult(defaultOutput);
            }

            //Keep the device ID for usage
            string model = monitorInfo.modelName;
            string serviceTag = monitorInfo.edid.ServiceTag;

            //Read all settings for this model
            List<DDPMMonitorSettings> settings = _SettingsPlugin.ReloadMonitorSettings(model).Result;
            if (settings == null) //never, but check for safe
            {
                writelog($"@ ReadEAMonitorSettings: ReloadMonitorSettings(model={model}) is null.");
                return Task.FromResult(defaultOutput);
            }

            //Find the settings for the specified device
            DDPMMonitorSettings monitorSetting = settings.Find(x => x.ServiceTag == monitorInfo.edid.ServiceTag);
            //There is no settings found for this device
            if (monitorSetting == null)
            {
                writelog($"@ ReadEAMonitorSettings: Settings for (model={model}, serviceTag={serviceTag}) is not found (never be saved before).");
                return Task.FromResult(defaultOutput);
            }

            //Robert_Lin, 2024-10-13, If the settings are migrated from DDM, some of SplitJson,
            //1 CustomLayouts:
            //2 PresetLayouts: containts EAID only

            //Need to refernce to CustomLayouts
            SplitJson[] customArray = ReadEACustomList().Result;
            List<SplitJson> customList = new List<SplitJson>();
            if ((customArray != null) && (customArray.Length > 0))
                customList = new List<SplitJson>(customArray);

            //Convert SelectedSplit
            if (monitorSetting.EA.SelectedSplit.CellCount < 0)
            {
                //It's a settings migrated from DDM

                //If it's a custom layout
                if (monitorSetting.EA.SelectedSplit.EAID >= 1000)
                {
                    monitorSetting.EA.SelectedSplit = customList.Find(x => x.EAID == monitorSetting.EA.SelectedSplit.EAID);
                    if (monitorSetting.EA.SelectedSplit == null)
                        monitorSetting.EA.SelectedSplit = new SplitJson() { CellCount = 0, SplitKey = 'A' };
                }
                else
                {
                    //It's a preset layout
                    monitorSetting.EA.SelectedSplit = SplitJson.CreatePresetLayoutFromEAID(monitorSetting.EA.SelectedSplit.EAID);
                }
            }

            //Robert_Lin, 2024-10-11 for default RecentList, if RecentList is null, then assign default list to it
            if ((monitorSetting.EA.RecentList == null) || (monitorSetting.EA.RecentList.Length == 0))
                monitorSetting.EA.RecentList = SplitJson.DefaultRecentList.ToArray();
            else
            {
                //Convert RecentList
                List<SplitJson> migratedRecentList = new List<SplitJson>();
                foreach (SplitJson recentJson in monitorSetting.EA.RecentList)
                {
                    if (recentJson.CellCount < 0)
                    {
                        //If it's a custom layout
                        if (recentJson.EAID >= 1000)
                        {
                            SplitJson? custJson = customList.Find(x => x.EAID == monitorSetting.EA.SelectedSplit.EAID);
                            if (custJson != null)
                            {
                                migratedRecentList.Add(custJson);
                            }
                        }
                        else
                        {
                            //It's a preset layout
                            SplitJson? presetJson = SplitJson.CreatePresetLayoutFromEAID(monitorSetting.EA.SelectedSplit.EAID);
                            if (presetJson != null)
                            {
                                migratedRecentList.Add(presetJson);
                            }
                        }
                    }
                    else
                    {
                        migratedRecentList.Add(recentJson);
                    }
                }
                monitorSetting.EA.RecentList = migratedRecentList.ToArray();
            }

            //_dump_SplitJsonList(monitorInfo, monitorSetting.EA.RecentList.ToList<SplitJson>());
            //Return the EA settings from the settings file
            return Task.FromResult(monitorSetting.EA);
        }

        private void _dump_SplitJsonList(MonitorInfo mi, List<SplitJson> splitJsonList)
        {
            Trace.WriteLine($"Monitor: {mi.AliasDeviceName}");
            int idx = 0;
            foreach (SplitJson splitJson in splitJsonList)
            {
                Trace.WriteLine($"[{idx}] {splitJson.ToString()}");
                idx++;
            }
        }

        //public Task<bool> EAReloadMonitorSettings(MonitorInfo monitorInfo)
        //{
        //    if (_DisplayManagerPlugin != null)
        //    {
        //        return _DisplayManagerPlugin.EAReloadMonitorSettings(monitorInfo);
        //    }
        //    return Task.FromResult(false);
        //}

        //Request from UI, when EzSettings changed
        //public Task<bool> EASaveOptions(MonitorInfo monitorInfo, EAMonitorSettings eaSettings)
        //{
        //    if (_SettingsPlugin == null)
        //    {
        //        writelog("@ EASaveOptions: _SettingsPlugin is null.");
        //        return Task.FromResult(false);
        //    }

        //    //Keep the device ID for usage
        //    string model = monitorInfo.modelName;
        //    string serviceTag = monitorInfo.edid.ServiceTag;

        //    //Read all settings for this model
        //    List<DDPMMonitorSettings> settings = _SettingsPlugin.ReloadMonitorSettings(model).Result;
        //    if (settings == null) //never, but check for safe
        //    {
        //        writelog($"@ EASaveOptions: ReloadMonitorSettings(model={model}) is null.");
        //        return Task.FromResult(false);
        //    }

        //    //Find the settings for the specified device
        //    DDPMMonitorSettings monitorSetting = settings.Find(x => x.ServiceTag == monitorInfo.edid.ServiceTag);
        //    //There is no settings found for this device
        //    if (monitorSetting == null)
        //    {
        //        writelog($"@ EASaveOptions: Settings for (model={model}, serviceTag={serviceTag}) is not found (never be saved before).");
        //        return Task.FromResult(false);
        //    }

        //    monitorSetting.EA.IsWidthoutGap = eaSettings.IsWidthoutGap;
        //    monitorSetting.EA.IsOnlyAllowWhenShiftKeyPressed = eaSettings.IsOnlyAllowWhenShiftKeyPressed;
        //    monitorSetting.EA.IsSpanAcrossMultiMonitors = eaSettings.IsSpanAcrossMultiMonitors;
        //    monitorSetting.EA.IsAwsEnabled = eaSettings.IsAwsEnabled;

        //    if (_SettingsPlugin.WriteMonitorSettings(monitorInfo.modelName, settings).Result)
        //    {
        //        writelog($"@ EASaveOptions(model={model}, serviceTage={serviceTag}) Save to settings file OK.");

        //        //Notify EAPlugin to reaload settings
        //        bool reloadOK = EAReloadMonitorSettings(monitorInfo).Result;
        //        return Task.FromResult(reloadOK);
        //    }

        //     return Task.FromResult(false);
        //}

        public Task<EzSettings> ReadEzSettings()
        {
            //Read DDPMSettings
            DDPMSettings ddpmSettings = _SettingsPlugin.ReloadAppConfigData().Result;
            if (ddpmSettings != null)
            {
                return Task.FromResult(ddpmSettings.UserSettings.EzSettings);
            }
            //Fail to read, will return the default settings
            return Task.FromResult(new EzSettings());
        }

        public Task<bool> WriteEzSettings_IsWidthoutGap(bool newValue)
        {
            if (_SettingsPlugin != null)
            {
                DDPMSettings ddpmSettings = _SettingsPlugin.ReloadAppConfigData().Result;
                if (ddpmSettings != null)
                {
                    //Check if value is changed
                    if (ddpmSettings.UserSettings.EzSettings.IsWidthoutGap == newValue)
                        return Task.FromResult(true);

                    //Apply new setting value
                    ddpmSettings.UserSettings.EzSettings.IsWidthoutGap = newValue;
                    //Save the DDPMSettings back to Settings file
                    if (_SettingsPlugin.SetAppConfigData(ddpmSettings).Result)
                    {
                        if (_DisplayManagerPlugin != null)
                        {
                            _DisplayManagerPlugin.ReloadEzSettings();
                        }

                        SendEasyArrangeTelemetry("OverlapBorder");
                        return Task.FromResult(true);
                    }
                }
            }
            //Read DDPMSettings
            //Fail to read, will return false
            return Task.FromResult(false);
        }

        public Task<bool> WriteEzSettings_IsOnlyAllowWhenShiftKeyPressed(bool newValue)
        {
            if (_SettingsPlugin != null)
            {
                DDPMSettings ddpmSettings = _SettingsPlugin.ReloadAppConfigData().Result;
                if (ddpmSettings != null)
                {
                    //Check if value is changed
                    if (ddpmSettings.UserSettings.EzSettings.IsOnlyAllowWhenShiftKeyPressed == newValue)
                    {
                        writelog($"@ DeviceManager.WriteEzSettings_IsOnlyAllowWhenShiftKeyPressed({newValue}): Value is not changed");
                        return Task.FromResult(true);
                    }

                    //Apply new setting value
                    ddpmSettings.UserSettings.EzSettings.IsOnlyAllowWhenShiftKeyPressed = newValue;
                    writelog($"@ DeviceManager.WriteEzSettings_IsOnlyAllowWhenShiftKeyPressed({newValue}): Value is changed");
                    //Save the DDPMSettings back to Settings file
                    if (_SettingsPlugin.SetAppConfigData(ddpmSettings).Result)
                    {
                        writelog($"@ DeviceManager.WriteEzSettings_IsOnlyAllowWhenShiftKeyPressed({newValue}): Update to settings file");
                        if (_DisplayManagerPlugin != null)
                        {
                            writelog($"@ DeviceManager.WriteEzSettings_IsOnlyAllowWhenShiftKeyPressed({newValue}): Notify EAPlugin to refresh itself");
                            _DisplayManagerPlugin.ReloadEzSettings();
                        }

                        SendEasyArrangeTelemetry("Hold-Shift");
                        return Task.FromResult(true);
                    }
                }
            }
            else
            {
                writelog($"@ DeviceManager.WriteEzSettings_IsOnlyAllowWhenShiftKeyPressed({newValue}): _SettingsPlugin is null");
            }
            //Read DDPMSettings
            //Fail to read, will return false
            return Task.FromResult(false);
        }

        public Task<bool> WriteEzSettings_IsSpanAcrossMultiMonitors(bool newValue)
        {
            if (_SettingsPlugin != null)
            {
                DDPMSettings ddpmSettings = _SettingsPlugin.ReloadAppConfigData().Result;
                if (ddpmSettings != null)
                {
                    //Check if value is changed
                    if (ddpmSettings.UserSettings.EzSettings.IsSpanAcrossMultiMonitors == newValue)
                        return Task.FromResult(true);

                    //Apply new setting value
                    ddpmSettings.UserSettings.EzSettings.IsSpanAcrossMultiMonitors = newValue;
                    //Save the DDPMSettings back to Settings file
                    if (_SettingsPlugin.SetAppConfigData(ddpmSettings).Result)
                    {
                        if (_DisplayManagerPlugin != null)
                        {
                            _DisplayManagerPlugin.ReloadEzSettings();
                        }

                        SendEasyArrangeTelemetry("span_monitors");
                        return Task.FromResult(true);
                    }
                }
            }
            //Read DDPMSettings
            //Fail to read, will return false
            return Task.FromResult(false);
        }

        public Task<bool> WriteEzSettings_IsAwsEnabled(bool newValue)
        {
            if (_SettingsPlugin != null)
            {
                DDPMSettings ddpmSettings = _SettingsPlugin.ReloadAppConfigData().Result;
                if (ddpmSettings != null)
                {
                    //Check if value is changed
                    if (ddpmSettings.UserSettings.EzSettings.IsAwsEnabled == newValue)
                        return Task.FromResult(true);
                    //Apply new setting value
                    ddpmSettings.UserSettings.EzSettings.IsAwsEnabled = newValue;
                    //Save the DDPMSettings back to Settings file
                    if (_SettingsPlugin.SetAppConfigData(ddpmSettings).Result)
                    {
                        if (_DisplayManagerPlugin != null)
                        {
                            _DisplayManagerPlugin.ReloadEzSettings();
                        }

                        //Telemetry
                        SendEasyArrangeTelemetry("App-Snap");

                        return Task.FromResult(true);
                    }
                }
            }
            //Read DDPMSettings
            //Fail to read, will return false
            return Task.FromResult(false);
        }

        public Task<bool> SetEASelectedLayout(MonitorInfo monitorInfo, SplitJson spJson)
        {
            if (_DisplayManagerPlugin != null)
            {
                return _DisplayManagerPlugin.SetEASelectedLayout(monitorInfo, spJson);
            }
            writelog("@ DeviceManager.SetEASelectedLayout(): _DisplayManagerPlugin is null");
            return Task.FromResult(false);
        }

        //Robert_Lin, 2024-10-8, bridge of EASettingsChanged
        //DisplayManagerPlugin will call to here, and DeviceManagerPlugin call to its handler
        private void _DisplayManagerPlugin_EASettingsChanged(object sender, EAArgs e)
        {
            if (EASettingsChanged != null)
            {
                writelog("@ DeviceManaerPlugin._DisplayManagerPlugin_EASettingsChanged(), Call to next handler.");
                Task.Run(() => EASettingsChanged.Invoke(this, e));
            }
            else
            {
                writelog("@ DeviceManaerPlugin._DisplayManagerPlugin_EASettingsChanged(), EASettingsChanged is null.");
            }
        }

        //Robert_Ln, 2024-10-12, Added after move CustomList to UserSettings from MonitorSettings
        /// <summary>
        /// Read the EACustomList for current user
        /// </summary>
        /// <returns></returns>
        public Task<SplitJson[]> ReadEACustomList()
        {
            if (_SettingsPlugin != null)
            {
                //Read App Settings
                DDPMSettings appSettings = _SettingsPlugin.ReloadAppConfigData().Result;
                //Don't return null, return empty array instead
                if (appSettings != null)
                {
                    if (appSettings.UserSettings != null)
                    {
                        if (appSettings.UserSettings.EACustomList != null)
                            return Task.FromResult(appSettings.UserSettings.EACustomList);
                    }
                }
            }
            //Failed, return an empty array instead of null
            return Task.FromResult(new SplitJson[] { });
        }

        /// <summary>
        /// Write the EACustomList to current user's settings file
        /// </summary>
        /// <param name="customList"></param>
        /// <returns></returns>
        public Task<bool> WriteEACustomList(SplitJson[] customList)
        {
            if (_SettingsPlugin != null)
            {
                //Read App Settings
                DDPMSettings appSettings = _SettingsPlugin.ReloadAppConfigData().Result;
                if (appSettings != null)
                {
                    if (appSettings.UserSettings != null)
                    {
                        appSettings.UserSettings.EACustomList = (SplitJson[])customList.Clone();
                        //Writeback to app settings
                        _SettingsPlugin.SetAppConfigData(appSettings);
                    }
                }
            }
            //Failed, return an empty array instead of null
            return Task.FromResult(false);
        }

        #endregion EasyArrage

        #region EasyMemory

        /// <summary>
        /// Update Monitorsettings EasyArrangement
        /// </summary>
        /// <param name="eaProfile"></param>
        /// <returns></returns>
        public async Task<bool> WriteMonitorEasyArrangement(MonitorInfo monitorInfo, EasyArrangementDDPM easyArrangementDDPM)
        {
            if (_SettingsPlugin == null)
            {
                writelog("@ WriteMonitorEasyArrangement: _SettingsPlugin is null.");
                return false;
            }

            try
            {
                string model = monitorInfo.modelName;
                string serviceTag = monitorInfo.edid.ServiceTag;

                List<DDPMMonitorSettings> settings = await _SettingsPlugin.ReloadMonitorSettings(model);
                if (settings == null)
                {
                    writelog($"@ WriteMonitorEasyArrangement: ReloadMonitorSettings(model={model}) return null.");
                    return false;
                }

                DDPMMonitorSettings? monitorSettings = settings.FirstOrDefault(x => x.ServiceTag.Equals(serviceTag));

                if (monitorSettings == null)
                {
                    writelog($"@ WriteMonitorEasyArrangement: Reloaded settings not contains (model={model}, serviceTag={serviceTag}).");
                    return false;
                }

                monitorSettings.easyArrangementDDPM = easyArrangementDDPM;

                bool writeResult = await _SettingsPlugin.WriteMonitorSettings(model, settings);
                if (writeResult)
                {
                    DisplayPropertiesInfo dpInfo = GetDisplayPropertiesInfo(monitorInfo).Result;
                    if (dpInfo != null)
                    {
                        DisplayFeatures_Functions infos = new DisplayFeatures_Functions();
                        bool var = infos.SentInfoToTelementry(Log, _TelementryScheduler, monitorInfo, dpInfo, monitorSettings.easyArrangementDDPM, null, "EasyMemory");
                        if (var)
                            writelog($"@ WriteMonitorEasyArrangement(model={model}, serviceTag={serviceTag}) : SentInfoToTelementry Success.");
                        else
                            writelog($"@ WriteMonitorEasyArrangement(model={model}, serviceTag={serviceTag}) : SentInfoToTelementry Error.");
                    }
                    writelog($"@ WriteMonitorEasyArrangement(model={model}, serviceTag={serviceTag}) OK.");
                    return true;
                }

                writelog($"@ WriteMonitorEasyArrangement: WriteMonitorSettings(model={model}, serviceTag={serviceTag}) failed.");
                return false;
            }
            catch (Exception ex)
            {
                writelog($"@ WriteMonitorEasyArrangement: Error occurred - {ex.Message}");
                return false;
            }
        }

        public async Task<bool> UpdateMonitorEzProfileSettingDDPM(MonitorInfo monitorInfo, EzProfileSettingDDPM profileSettingDDPM)
        {
            if (_SettingsPlugin == null)
            {
                writelog("@ UpdateMonitorEzProfileSettingDDPM: _SettingsPlugin is null.");
                return false;
            }

            try
            {
                string model = monitorInfo.modelName;
                string serviceTag = monitorInfo.edid.ServiceTag;

                // Reload Monitor
                List<DDPMMonitorSettings> settings = await _SettingsPlugin.ReloadMonitorSettings(model);
                if (settings == null)
                {
                    writelog($"@ UpdateMonitorEzProfileSettingDDPM: ReloadMonitorSettings(model={model}) return null.");
                    return false;
                }

                // 找到對應的 Monitor
                DDPMMonitorSettings? monitorSettings = settings.FirstOrDefault(x => x.ServiceTag.Equals(serviceTag));
                if (monitorSettings == null)
                {
                    writelog($"@ UpdateMonitorEzProfileSettingDDPM: Reloaded settings not contains (model={model}, serviceTag={serviceTag}).");
                    return false;
                }

                // 檢查 EasyArrangement
                if (monitorSettings.easyArrangementDDPM == null || monitorSettings.easyArrangementDDPM.Desktops.Count == 0)
                {
                    writelog($"@ UpdateMonitorEzProfileSettingDDPM: No EasyArrangement or Desktops found.");
                    return false;
                }

                // 在 DesktopDDPM[0] 中尋找相同 ID 的 ProfileSetting
                EzProfileSettingDDPM? existingProfileSetting = monitorSettings.easyArrangementDDPM.Desktops[0].ProfileSettings.FirstOrDefault(ps => ps.ID == profileSettingDDPM.ID);

                if (existingProfileSetting != null)
                {
                    // 更新 ProfileSetting 資料
                    existingProfileSetting.Auto = profileSettingDDPM.Auto;
                    existingProfileSetting.AutoStartTime = profileSettingDDPM.AutoStartTime;
                    existingProfileSetting.StartUpLaunch = profileSettingDDPM.StartUpLaunch;

                    // 寫回 Monitor 設定
                    bool writeResult = await _SettingsPlugin.WriteMonitorSettings(model, settings);
                    if (writeResult)
                    {
                        DisplayPropertiesInfo dpInfo = GetDisplayPropertiesInfo(monitorInfo).Result;
                        if (dpInfo != null)
                        {
                            DisplayFeatures_Functions infos = new DisplayFeatures_Functions();
                            bool var = infos.SentInfoToTelementry(Log, _TelementryScheduler, monitorInfo, dpInfo, monitorSettings.easyArrangementDDPM, null, "EasyMemory");
                            if (var)
                                writelog($"@ UpdateMonitorEzProfileSettingDDPM(model={model}, serviceTag={serviceTag}) : SentInfoToTelementry Success.");
                            else
                                writelog($"@ UpdateMonitorEzProfileSettingDDPM(model={model}, serviceTag={serviceTag}) : SentInfoToTelementry Error.");
                        }
                        writelog($"@ UpdateMonitorEzProfileSettingDDPM(model={model}, serviceTag={serviceTag}, ID={profileSettingDDPM.ID}) updated successfully.");
                        return true;
                    }

                    writelog($"@ UpdateMonitorEzProfileSettingDDPM: Failed to write MonitorSettings(model={model}, serviceTag={serviceTag}).");
                    return false;
                }
                else
                {
                    writelog($"@ UpdateMonitorEzProfileSettingDDPM: ProfileSetting with ID={profileSettingDDPM.ID} not found.");
                    return false; // 未找到
                }
            }
            catch (Exception ex)
            {
                writelog($"@ UpdateMonitorEzProfileSettingDDPM: Error occurred - {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Read Monitorsettings EasyArrangement
        /// </summary>
        /// <param name="eaProfile"></param>
        /// <returns></returns>
        public async Task<EasyArrangementDDPM> ReadMonitorEasyArrangement(MonitorInfo monitorInfo)
        {
            EasyArrangementDDPM defaultOutput = null;

            if (_SettingsPlugin == null)
            {
                writelog("@ ReadMonitorEasyArrangement: _SettingsPlugin is null.");
                return defaultOutput;
            }

            try
            {
                string model = monitorInfo.modelName;
                string serviceTag = monitorInfo.edid.ServiceTag;

                List<DDPMMonitorSettings> settings = await _SettingsPlugin.ReloadMonitorSettings(model);
                if (settings == null)
                {
                    writelog($"@ ReadMonitorEasyArrangement: ReloadMonitorSettings(model={model}) is null.");
                    return defaultOutput;
                }

                DDPMMonitorSettings monitorSetting = settings.Find(x => x.ServiceTag == serviceTag);
                if (monitorSetting == null)
                {
                    writelog($"@ ReadMonitorEasyArrangement: Settings for (model={model}, serviceTag={serviceTag}) is not found.");
                    return defaultOutput;
                }

                defaultOutput = monitorSetting.easyArrangementDDPM;
                return defaultOutput;
            }
            catch (Exception ex)
            {
                writelog($"@ ReadMonitorEasyArrangement: Error occurred - {ex.Message}");
                return defaultOutput;
            }
        }

        public async Task<bool> WriteUserListEAProfileDDPM(List<EAProfileDDPM> eaProfileList)
        {
            if (_SettingsPlugin == null)
            {
                writelog("@ WriteUserListEAProfileDDPM: _SettingsPlugin is null.");
                return false;
            }

            try
            {
                DDPMSettings ddpmSettings = await _SettingsPlugin.ReloadAppConfigData();
                if (ddpmSettings == null)
                {
                    writelog($"@ WriteUserListEAProfileDDPM: ReloadAppConfigData return null.");
                    return false;
                }

                if (ddpmSettings.UserSettings.EAProfile != null)
                {
                    ddpmSettings.UserSettings.EAProfile = eaProfileList;
                    writelog($"@ WriteUserListEAProfileDDPM Update OK.");
                }
                else
                {
                    ddpmSettings.UserSettings = new DDPMUserSettings
                    {
                        EAProfile = new List<EAProfileDDPM>()
                    };
                    writelog($"@ WriteUserListEAProfileDDPM new List<EAProfileDDPM>() OK.");
                }

                if (await _SettingsPlugin.SetAppConfigData(ddpmSettings))
                {
                    writelog($"@ WriteUserListEAProfileDDPM OK.");
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                writelog($"@ WriteUserEAProfileDDPM: Error occurred - {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Update Usersettings eaProfile
        /// </summary>
        /// <param name="eaProfile"></param>
        /// <returns></returns>
        public async Task<bool> WriteUserEAProfileDDPM(MonitorInfo monitorInfo, EAProfileDDPM eaProfile)
        {
            if (_SettingsPlugin == null)
            {
                writelog("@ WriteUserEAProfileDDPM: _SettingsPlugin is null.");
                return false;
            }

            try
            {
                DDPMSettings ddpmSettings = await _SettingsPlugin.ReloadAppConfigData();
                if (ddpmSettings == null)
                {
                    writelog($"@ WriteUserEAProfileDDPM: ReloadAppConfigData return null.");
                    return false;
                }

                if (ddpmSettings.UserSettings.EAProfile != null)
                {
                    EAProfileDDPM existingProfile = ddpmSettings.UserSettings.EAProfile.FirstOrDefault(p => p.ID == eaProfile.ID);
                    if (existingProfile == null)
                    {
                        existingProfile = new EAProfileDDPM
                        {
                            AppInfos = eaProfile.AppInfos,
                            ID = eaProfile.ID,
                            Layout = eaProfile.Layout,
                            Name = eaProfile.Name
                        };
                        ddpmSettings.UserSettings.EAProfile.Add(existingProfile);
                        writelog($"@ WriteUserEAProfileDDPM Update OK.");
                    }
                }
                else
                {
                    ddpmSettings.UserSettings = new DDPMUserSettings
                    {
                        EAProfile = new List<EAProfileDDPM> { eaProfile }
                    };
                    writelog($"@ WriteUserEAProfileDDPM Add OK.");
                }

                if (await _SettingsPlugin.SetAppConfigData(ddpmSettings))
                {
                    if (monitorInfo != null)
                    {
                        DisplayPropertiesInfo dpInfo = GetDisplayPropertiesInfo(monitorInfo).Result;
                        if (dpInfo != null)
                        {
                            DisplayFeatures_Functions infos = new DisplayFeatures_Functions();
                            List<string> telementryList = new List<string> { "EasyMemoryProfileCount", "MaxEasyMemoryLayoutUsed" };
                            foreach (var telem in telementryList)
                            {
                                if (infos.SentInfoToTelementry(Log, _TelementryScheduler, monitorInfo, dpInfo, null, ddpmSettings, telem))
                                {
                                    writelog($"@ UpdateUserEAProfileDDPM(model={monitorInfo.modelName}, serviceTag={monitorInfo.edid.ServiceTag}) : {telem} SentInfoToTelementry Success.");
                                }
                                else
                                {
                                    writelog($"@ UpdateUserEAProfileDDPM(model={monitorInfo.modelName}, serviceTag={monitorInfo.edid.ServiceTag}) : {telem} SentInfoToTelementry Error.");
                                }
                            }
                        }
                    }
                    writelog($"@ WriteUserEAProfileDDPM OK.");
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                writelog($"@ WriteUserEAProfileDDPM: Error occurred - {ex.Message}");
                return false;
            }
        }

        public async Task<bool> UpdateUserEAProfileDDPM(MonitorInfo monitorInfo, EAProfileDDPM eaProfile)
        {
            if (_SettingsPlugin == null)
            {
                writelog("@ UpdateUserEAProfileDDPM: _SettingsPlugin is null.");
                return false;
            }

            try
            {
                DDPMSettings ddpmSettings = await _SettingsPlugin.ReloadAppConfigData();
                if (ddpmSettings == null)
                {
                    writelog("@ UpdateUserEAProfileDDPM: ReloadAppConfigData returned null.");
                    return false;
                }

                if (ddpmSettings.UserSettings?.EAProfile != null)
                {
                    EAProfileDDPM existingProfile = ddpmSettings.UserSettings.EAProfile.FirstOrDefault(p => p.ID == eaProfile.ID);

                    if (existingProfile != null)
                    {
                        existingProfile.AppInfos = eaProfile.AppInfos;
                        existingProfile.Layout = eaProfile.Layout;
                        existingProfile.Name = eaProfile.Name;

                        if (await _SettingsPlugin.SetAppConfigData(ddpmSettings))
                        {
                            DisplayPropertiesInfo dpInfo = GetDisplayPropertiesInfo(monitorInfo).Result;
                            if (dpInfo != null)
                            {
                                DisplayFeatures_Functions infos = new DisplayFeatures_Functions();
                                List<string> telementryList = new List<string> { "EasyMemoryProfileCount", "MaxEasyMemoryLayoutUsed" };
                                foreach (var telem in telementryList)
                                {
                                    if (infos.SentInfoToTelementry(Log, _TelementryScheduler, monitorInfo, dpInfo, null, ddpmSettings, telem))
                                    {
                                        writelog($"@ UpdateUserEAProfileDDPM(model={monitorInfo.modelName}, serviceTag={monitorInfo.edid.ServiceTag}) : {telem} SentInfoToTelementry Success.");
                                    }
                                    else
                                    {
                                        writelog($"@ UpdateUserEAProfileDDPM(model={monitorInfo.modelName}, serviceTag={monitorInfo.edid.ServiceTag}) : {telem} SentInfoToTelementry Error.");
                                    }
                                }
                            }
                            writelog($"@ UpdateUserEAProfileDDPM: Profile with ID {eaProfile.ID} updated successfully.");
                            return true;
                        }
                        else
                        {
                            writelog($"@ UpdateUserEAProfileDDPM: Failed to update profile with ID {eaProfile.ID}.");
                            return false;
                        }
                    }
                    else
                    {
                        writelog($"@ UpdateUserEAProfileDDPM: No profile found with ID {eaProfile.ID}. No update performed.");
                        return false;
                    }
                }
                else
                {
                    writelog($"@ UpdateUserEAProfileDDPM: EAProfile list is null in UserSettings.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                writelog($"@ UpdateUserEAProfileDDPM: Error occurred - {ex.Message}");
                return false;
            }
        }

        public async Task<List<EAProfileDDPM>> ReadUserEAProfileDDPM()
        {
            List<EAProfileDDPM> defaultOutput = null;

            if (_SettingsPlugin == null)
            {
                writelog("@ ReadUserEAProfileDDPM: _SettingsPlugin is null.");
                return defaultOutput;
            }

            try
            {
                // 非同步讀取 User Settings，並等待完成後再繼續
                DDPMSettings ddpmSettings = await _SettingsPlugin.ReloadAppConfigData();
                if (ddpmSettings == null)
                {
                    writelog($"@ ReadUserEAProfileDDPM: null.");
                    return defaultOutput;
                }

                defaultOutput = ddpmSettings.UserSettings.EAProfile;
                return defaultOutput;
            }
            catch (Exception ex)
            {
                writelog($"@ ReadUserEAProfileDDPM: Error occurred - {ex.Message}");
                return defaultOutput;
            }
        }

        public Task<bool> CleanUserEzProfiles()
        {
            if (_SettingsPlugin == null)
            {
                writelog("@ CleanUserEzProfiles: _SettingsPlugin is null.");
                return Task.FromResult(false);
            }

            DDPMSettings ddpmSettings = _SettingsPlugin.ReloadAppConfigData().Result;

            if (ddpmSettings == null)
            {
                writelog("@ CleanUserEzProfiles: ddpmSettings is null.");
                return Task.FromResult(false);
            }

            if (ddpmSettings.UserSettings.EAProfile == null)
            {
                ddpmSettings.UserSettings.EAProfile = new List<EAProfileDDPM>();
            }
            else
            {
                ddpmSettings.UserSettings.EAProfile.Clear();
            }
            if (_SettingsPlugin.SetAppConfigData(ddpmSettings).Result)
            {
                writelog($"@ CleanUserEzProfiles OK.");
                return Task.FromResult(true);
            }
            writelog("@ CleanUserEzProfiles: fail.");
            return Task.FromResult(true);
        }

        #endregion EasyMemory

        #region SW Update implementation

        public Task<SWUpdateInfoPackage> SW_GetSWUpdateInfo(bool isShowNotify = true, bool isDefer = false, bool isForce = false, bool reScan = true, bool isUITrigger = false)
        {
            if (_SWUpdatePlugin != null)
            {
                return Task.FromResult(_SWUpdatePlugin.GetSWUpdateInfo(isShowNotify, isDefer, isForce, _GlobalSettingParam.GlobalSetting_About.SWVersion, reScan, isUITrigger).Result);
            }
            return Task.FromResult(new SWUpdateInfoPackage());
        }

        public Task<List<SWUpdateInfo>> SW_DownloadAndInstall(List<SWUpdateInfo> swUpdateInfos, bool isUITrigger = false, string installPath = "")
        {
            writelog("[SW_DownloadAndInstall], start.");
            try
            {
                if (swUpdateInfos != null && swUpdateInfos.Count > 0)
                {
                    writelog("[SW_DownloadAndInstall], WriteRegistryData go.");
                    string registryKey = @"SOFTWARE\Dell Display and Peripheral Manager";
                    string SW_Available_date = swUpdateInfos[0].Available_date;
                    bool b = WriteRegistryData(RegistryHive.LocalMachine, registryKey, nameof(SW_Available_date), SW_Available_date).Result;
                    writelog($"[SW_DownloadAndInstall], WriteRegistryData ret : {b}");
                }
            }
            catch (Exception ex)
            {
                writelog($"[SW_DownloadAndInstall], Error : {ex.Message}");
            }
            return Task.FromResult(_SWUpdatePlugin.DownloadAndInstall(swUpdateInfos, isUITrigger, installPath).Result);
        }

        private Task<bool> SW_SetSWUpdateInfoPackage(SWUpdateInfoPackage swUpdateInfoPackage)
        {
            if (_SettingsPlugin != null)
            {
                DDPMSettings config = _SettingsPlugin.ReloadAppConfigData().Result;
                foreach (SWUpdateInfo updateInfo in swUpdateInfoPackage.SWUpdateInfo)
                {
                    updateInfo.ServerPath = "";
                    updateInfo.SHA256 = "";
                    updateInfo.SHA512 = "";
                    updateInfo.Thumbprint = "";
                }
                config.UserSettings.DelaySWUpdateInfoPackage = swUpdateInfoPackage;
                return Task.FromResult(_SettingsPlugin.SetAppConfigData(config).Result);
            }
            return Task.FromResult(false);
        }

        private Task<bool> SW_CheckSWUpdate()
        {
            writelog("[SW_CheckSWUpdate], start.");
            bool ret = false;
            if (_SWUpdatePlugin == null)
                return Task.FromResult(ret);
            try
            {
                SW_SetDelaySWUpdateInfoPackage();
                SWUpdateInfoPackage swUpdateInfos = _SWUpdatePlugin.GetSWUpdateInfo(true, false, false, _GlobalSettingParam.GlobalSetting_About.SWVersion, true, false).Result;
                ret = true;
            }
            catch (Exception ex)
            {
                writelog($"[SW_CheckSWUpdate], Error:{ex.Message}");
            }
            writelog("[SW_CheckSWUpdate], done.");
            return Task.FromResult(ret);
        }

        private void SW_SetDelaySWUpdateInfoPackage()
        {
            if (_SettingsPlugin != null && _SWUpdatePlugin != null)
            {
                DDPMSettings config = _SettingsPlugin.ReloadAppConfigData().Result;

                if (config != null && config.UserSettings != null) // 2024-08-16 Elie, check if null before using.
                    _SWUpdatePlugin.SetDelaySWUpdateInfoPackage(config.UserSettings.DelaySWUpdateInfoPackage);
                else
                {
                    writelog("[SW_SetDelaySWUpdateInfoPackage], ReloadAppConfigData is null.");
                }
            }
        }

        private void DeleteDdpmSwUpdaterFolder()
        {
            try
            {
                writelog("[DeleteDdpmSwUpdaterFolder], start.");
                string registryKey = @"SOFTWARE\Dell Display and Peripheral Manager";
                object o = ReadRegistryData(RegistryHive.LocalMachine, registryKey, "DdpmSwUpdater").Result;
                writelog($"[DeleteDdpmSwUpdaterFolder], o={o}.");
                if (o != null && o is string && !string.IsNullOrEmpty(o.ToString()))
                {
                    writelog($"[DeleteDdpmSwUpdaterFolder], o_String={o.ToString()}.");
                    DDPMFileSecurity DDPMFileSecurity = new DDPMFileSecurity();
                    string AppDataPath = DDPMFileSecurity.GetActiveUserLocalAppDataPath();
                    if (!string.IsNullOrEmpty(AppDataPath))
                    {
                        string path = AppDataPath + "\\Dell\\Dell Display and Peripheral Manager" + "\\" + o.ToString();
                        if (!string.IsNullOrEmpty(path) && Directory.Exists(path))
                        {
                            writelog($"[DeleteDdpmSwUpdaterFolder], Exists.");
                            Directory.Delete(path, true);
                            writelog($"[DeleteDdpmSwUpdaterFolder], Delete.");
                        }
                        WriteRegistryData(RegistryHive.LocalMachine, registryKey, "DdpmSwUpdater", "");
                        writelog($"[DeleteDdpmSwUpdaterFolder], WriteRegistryData.");
                    }
                    else
                    {
                        writelog("[DeleteDdpmSwUpdaterFolder], AppDataPath get null.");
                    }
                }
                writelog("[DeleteDdpmSwUpdaterFolder], done.");
            }
            catch (Exception ex)
            {
                writelog($"[DeleteDdpmSwUpdaterFolder], Error : {ex.Message}");
            }
        }
        private void TelemetryDdpmSwUpdater()
        {
            writelog("[TelemetryDdpmSwUpdater], start.");
            if (_TelementryScheduler != null && _SettingsPlugin != null)
            {
                try
                {
                    string registryKey = @"SOFTWARE\Dell Display and Peripheral Manager";
                    string UpdateVersion = string.Empty;
                    string Results = string.Empty;
                    string FailureMessage = string.Empty;
                    string SW_Update_date = string.Empty;
                    string SW_Available_date = string.Empty;
                    string ErrorCode = string.Empty;
                    object o = ReadRegistryData(RegistryHive.LocalMachine, registryKey, nameof(UpdateVersion)).Result;
                    writelog($"[TelemetryDdpmSwUpdater],ReadRegistryData UpdateVersion o = {o}.");
                    if (o != null && o is string && !string.IsNullOrEmpty(o.ToString()))
                    {
                        UpdateVersion = o.ToString();
                    }
                    o = ReadRegistryData(RegistryHive.LocalMachine, registryKey, nameof(Results)).Result;
                    writelog($"[TelemetryDdpmSwUpdater],ReadRegistryData Results o = {o}.");
                    if (o != null && o is string && !string.IsNullOrEmpty(o.ToString()))
                    {
                        Results = o.ToString();
                    }
                    o = ReadRegistryData(RegistryHive.LocalMachine, registryKey, nameof(FailureMessage)).Result;
                    writelog($"[TelemetryDdpmSwUpdater],ReadRegistryData FailureMessage o = {o}.");
                    if (o != null && o is string && !string.IsNullOrEmpty(o.ToString()))
                    {
                        FailureMessage = o.ToString();
                    }
                    o = ReadRegistryData(RegistryHive.LocalMachine, registryKey, nameof(SW_Update_date)).Result;
                    writelog($"[TelemetryDdpmSwUpdater],ReadRegistryData SW_Update_date o = {o}.");
                    if (o != null && o is string && !string.IsNullOrEmpty(o.ToString()))
                    {
                        SW_Update_date = o.ToString();
                    }
                    o = ReadRegistryData(RegistryHive.LocalMachine, registryKey, nameof(SW_Available_date)).Result;
                    writelog($"[TelemetryDdpmSwUpdater],ReadRegistryData SW_Available_date o = {o}.");
                    if (o != null && o is string && !string.IsNullOrEmpty(o.ToString()))
                    {
                        SW_Available_date = o.ToString();
                    }
                    o = ReadRegistryData(RegistryHive.LocalMachine, registryKey, nameof(ErrorCode)).Result;
                    writelog($"[TelemetryDdpmSwUpdater],ReadRegistryData ErrorCode o = {o}.");
                    if (o != null && o is string && !string.IsNullOrEmpty(o.ToString()))
                    {
                        ErrorCode = o.ToString();
                    }
                    if (!string.IsNullOrEmpty(UpdateVersion) &&
                        !string.IsNullOrEmpty(Results) &&
                        !string.IsNullOrEmpty(FailureMessage) &&
                        !string.IsNullOrEmpty(SW_Update_date) &&
                        !string.IsNullOrEmpty(SW_Available_date) &&
                        !string.IsNullOrEmpty(ErrorCode))
                    {
                        //Telementry Collection
                        var rt = false;
                        var ApplicationSettings_Function = new ApplicationSettings_Function();

                        if (FailureMessage.Equals(SWUErrorCode.NoError.ToString()))
                        {
                            writelog("[TelemetryDdpmSwUpdater] Send Telementry for SoftwareUpdate...");
                            rt = ApplicationSettings_Function.Send_SoftwareUpdate_Telementry(_TelementryScheduler, _AllInfoMonitors, UpdateVersion, Results, SW_Available_date, SW_Update_date);

                        }
                        else
                        {
                            writelog("[TelemetryDdpmSwUpdater] Send Telementry for SoftwareFailure...");
                            rt = ApplicationSettings_Function.Send_SoftwareFailure_Telementry(_TelementryScheduler, _AllInfoMonitors, ErrorCode, FailureMessage, SW_Available_date, SW_Update_date);
                        }
                        if (rt)
                        {
                            writelog("[TelemetryDdpmSwUpdater] Send Telementry Success ...");
                            bool b = false;
                            b = WriteRegistryData(RegistryHive.LocalMachine, registryKey, nameof(UpdateVersion), "").Result;
                            writelog($"[TelemetryDdpmSwUpdater],WriteRegistryData UpdateVersion b = {b}.");
                            b = WriteRegistryData(RegistryHive.LocalMachine, registryKey, nameof(Results), "").Result;
                            writelog($"[TelemetryDdpmSwUpdater],WriteRegistryData Results b = {b}.");
                            b = WriteRegistryData(RegistryHive.LocalMachine, registryKey, nameof(FailureMessage), "").Result;
                            writelog($"[TelemetryDdpmSwUpdater],WriteRegistryData FailureMessage b = {b}.");
                            b = WriteRegistryData(RegistryHive.LocalMachine, registryKey, nameof(SW_Update_date), "").Result;
                            writelog($"[TelemetryDdpmSwUpdater],WriteRegistryData SW_Update_date b = {b}.");
                            b = WriteRegistryData(RegistryHive.LocalMachine, registryKey, nameof(SW_Available_date), "").Result;
                            writelog($"[TelemetryDdpmSwUpdater],WriteRegistryData SW_Available_date b = {b}.");
                            b = WriteRegistryData(RegistryHive.LocalMachine, registryKey, nameof(ErrorCode), "").Result;
                            writelog($"[TelemetryDdpmSwUpdater],WriteRegistryData ErrorCode b = {b}.");
                        }
                        else
                        {
                            writelog("[TelemetryDdpmSwUpdater] Send Telementry Fail ...");
                        }
                    }
                }
                catch (Exception ex)
                {
                    writelog($"[TelemetryDdpmSwUpdater], Error : {ex.Message}");
                }
            }
            writelog("[TelemetryDdpmSwUpdater], done.");
        }

        #endregion

        #region ImpExpSettings

        private Dictionary<object, object> FindVCPTable(Dictionary<EDID, Dictionary<object, object>> cacheTable, EDID edid)
        {
            if (cacheTable.Count > 0)
            {
                var Keys = cacheTable.Keys.ToList();
                foreach (EDID Key in Keys)
                {
                    if (Key.Equals(edid))
                    {
                        return cacheTable[Key];
                    }
                }
            }
            return null;
        }

        public Task<bool> DisplayExportSettings(MonitorInfo monitorInfo, string path)
        {
            writelog("[DisplayExportSettings]Export Settings");
            //need test, but need other function
            //if vcp code is null, get vcp code
            List<DDPMMonitorSettings> settings = _SettingsPlugin.ReloadMonitorSettings(monitorInfo.modelName).Result;
            Dictionary<EDID, Dictionary<object, object>> VCPTable = _DisplayManagerPlugin.GetVCPCacheTable().Result;
            if (settings != null)
            {
                foreach (DDPMMonitorSettings monitorSettings in settings)
                {
                    if (monitorSettings != null)
                    {
                        if (monitorSettings.ServiceTag == monitorInfo.edid.ServiceTag)
                        {
                            try
                            {
                                writelog("[DisplayExportSettings]Export FindVCPTable");
                                Dictionary<object, object> cacheTable = new Dictionary<object, object>();
                                cacheTable = FindVCPTable(VCPTable, monitorInfo.edid);
                                writelog("[DisplayExportSettings]Export DisplayProperties");
                                monitorSettings.DisplayPropertiesInfo = Export_DisplayProperties(monitorInfo);
                                writelog("[DisplayExportSettings]Export ColorPreset");
                                if (_ColorPresetPlugin != null)
                                {
                                    monitorSettings.ColorPreset = _ColorPresetPlugin.Export(monitorInfo, _SettingsPlugin).Result;
                                }
                                writelog("[DisplayExportSettings]Export ALS");
                                List<ALSConfig> aLSConfigs = new List<ALSConfig>();
                                aLSConfigs = GetAllExistAlsConfig().Result;
                                if (aLSConfigs != null)
                                {
                                    ALSConfig aLSConfig = aLSConfigs.Find(x => (x.ModelName == monitorInfo.modelName));
                                    monitorSettings.ALSConfig = aLSConfig.AllValue;
                                }
                                writelog("[DisplayExportSettings]Export Gaming");
                                GamingDisplayPropertiesInfo gamingDisplayPropertiesInfo = new GamingDisplayPropertiesInfo();
                                gamingDisplayPropertiesInfo = GetGamingProperties_SupportedList(monitorInfo).Result;
                                if (gamingDisplayPropertiesInfo != null)
                                {
                                    if (gamingDisplayPropertiesInfo.IsSupported_GameEnhancementMode && gamingDisplayPropertiesInfo.Current_GameEnhancementMode != null)
                                    {
                                        monitorSettings.Gaming.Current_GameEnhancementMode = (Gaming_GameEnhancementMode)gamingDisplayPropertiesInfo.Current_GameEnhancementMode;
                                    }
                                    if (gamingDisplayPropertiesInfo.IsSupported_ResponseTime && gamingDisplayPropertiesInfo.Current_ResponseTime != null)
                                    {
                                        monitorSettings.Gaming.Current_ResponseTime = (Gaming_ResponseTime)gamingDisplayPropertiesInfo.Current_ResponseTime;
                                    }
                                    if (gamingDisplayPropertiesInfo.IsSupported_DarkStabilizer && gamingDisplayPropertiesInfo.Current_DarkStabilizer != null)
                                    {
                                        monitorSettings.Gaming.Current_DarkStabilizer = (Gaming_DarkStabilizer)gamingDisplayPropertiesInfo.Current_DarkStabilizer;
                                    }
                                    if (gamingDisplayPropertiesInfo.IsSupported_HDRType && gamingDisplayPropertiesInfo.Current_HDRType != null)
                                    {
                                        monitorSettings.Gaming.Current_HDRType = (Gaming_HDRType)gamingDisplayPropertiesInfo.Current_HDRType;
                                    }
                                    if (gamingDisplayPropertiesInfo.IsSupported_DualResolutionType && gamingDisplayPropertiesInfo.Current_DualResolutionType != null)
                                    {
                                        monitorSettings.Gaming.Current_DualResolutionType = (Gaming_DualResolutionType)gamingDisplayPropertiesInfo.Current_DualResolutionType;
                                    }
                                    if (gamingDisplayPropertiesInfo.IsSupported_VisionEngineType && gamingDisplayPropertiesInfo.IsEnable_VisionEngineType != null)
                                    {
                                        monitorSettings.Gaming.IsEnable_VisionEngineType = gamingDisplayPropertiesInfo.IsEnable_VisionEngineType;
                                    }
                                }
                                writelog("[DisplayExportSettings]Export VCPs");
                                foreach (VCPCode vcp in monitorSettings.VCPs)
                                {
                                    if (vcp.Value != null)
                                    {
                                        vcp.Value.Clear();
                                        byte b_vcpcode = Convert.ToByte(vcp.Code);
                                        //object value = cacheTable[b_vcpcode];
                                        ObjGetVCP objGet = GetVCPCapability(monitorInfo, b_vcpcode).Result;
                                        if (objGet.result && vcp.Value.FindIndex(x => x == (int)(uint)objGet.value) == -1)
                                        {
                                            writelog($"VCP code: {vcp.Code.ToString()}, value:{objGet.value.ToString()}");
                                            vcp.Value.Add((int)(uint)objGet.value);
                                        }
                                        //if (value != null)
                                        //{
                                        //    Console.WriteLine("VCP code : " + vcp.Code.ToString());
                                        //    Console.WriteLine("VCP value : " + value.ToString());
                                        //    vcp.Value.Add((int)value);
                                        //}
                                    }
                                }
                            }
                            catch
                            {
                                writelog("[DisplayExportSettings]Export is fail");
                                return Task.FromResult(false);
                            }
                        }
                    }
                    else
                    {
                        writelog("[DisplayExportSettings]monitorSettings is null");
                    }
                }
            }
            else
            {
                writelog("[DisplayExportSettings]settings is null");
            }

            try
            {
                //expot settings
                if (_SettingsPlugin.DisplayExportSettings(monitorInfo.modelName, monitorInfo.edid.ServiceTag, path).Result)
                {
                    writelog("[DisplayExportSettings]Export is Success");
                    writelog("[SentSettingstoTelementry] Send_Settings_Telementry : Export");
                    ApplicationSettings_Function ApplicationSettings_Function = new ApplicationSettings_Function();
                    if (_TelementryScheduler != null)
                    {
                        if (_AllInfoMonitors != null)
                        {
                            if (_AllInfoMonitors.Count > 0)
                            {
                                if (ApplicationSettings_Function.Send_Settings_Telementry(_TelementryScheduler, _AllInfoMonitors, "Export"))
                                {
                                    writelog("[SentSettingstoTelementry] Send_Settings_Telementry is success");
                                }
                                writelog("[SentSettingstoTelementry] Send_Settings_Telementry is fail");
                            }
                            else
                            {
                                writelog("[SentSettingstoTelementry] _AllInfoMonitors count is 0");
                            }
                        }
                        else
                        {
                            writelog("[SentSettingstoTelementry] _AllInfoMonitors is null");
                        }
                    }
                    else
                    {
                        writelog("[SentSettingstoTelementry] _TelementryScheduler is null");
                    }
                    return Task.FromResult(true);
                }
                else
                {
                    writelog("[DisplayExportSettings]Export is fail");
                }
            }
            catch
            {
                writelog("[DisplayExportSettings]Export catch is fail");
                return Task.FromResult(false);
            }
            return Task.FromResult(false);
        }

        public Task<bool> DisplayImportSettings(MonitorInfo monitorInfo, bool isSameModel, string path)
        {
            writelog("[DisplayImportSettings] Import Settings");
            ImportVCP importVCP = new ImportVCP();
            ImpVCPSequence impVCPSequence = new ImpVCPSequence();
            if (_SettingsPlugin != null)
            {
                List<VCPCode> vcps = new List<VCPCode>();
                if (_SettingsPlugin.DisplayImportSettings(path, isSameModel, out DDPMImpExpSettings ImpExpSettings).Result)
                {
                    if (ImpExpSettings != null)
                    {
                        if (ImpExpSettings.MonitorSettings != null)
                        {
                            try
                            {
                                vcps = ImpExpSettings.MonitorSettings.VCPs;
                                writelog("[DisplayImportSettings] Import DisplayProperties");
                                if (Import_DisplayProperties(monitorInfo, ImpExpSettings.MonitorSettings.DisplayPropertiesInfo))
                                {
                                    writelog("[DisplayImportSettings] Import_DisplayProperties");
                                }
                                writelog("[DisplayImportSettings] Import ColorPreset");
                                if (_ColorPresetPlugin != null)
                                {
                                    bool b = _ColorPresetPlugin.Import(monitorInfo, ImpExpSettings.MonitorSettings.ColorPreset, _SettingsPlugin).Result;
                                }
                                if (ImpExpSettings.MonitorSettings.Gaming != null)
                                {
                                    Gaming gaming = ImpExpSettings.MonitorSettings.Gaming;
                                    if (gaming.Current_GameEnhancementMode != 0)
                                    {
                                        bool bge = SetGameEnhancementMode(monitorInfo, gaming.Current_GameEnhancementMode).Result;
                                    }
                                    if (gaming.Current_ResponseTime != 0)
                                    {
                                        bool bgr = SetGaming_ResponseTime(monitorInfo, gaming.Current_ResponseTime).Result;
                                    }
                                    if (gaming.Current_DarkStabilizer != 0)
                                    {
                                        bool bgd = SetGaming_DarkStabilizer(monitorInfo, gaming.Current_DarkStabilizer).Result;
                                    }
                                    if (gaming.Current_HDRType != 0)
                                    {
                                        bool bgh = SetGaming_HDRType(monitorInfo, gaming.Current_HDRType).Result;
                                    }
                                    if (gaming.Current_DualResolutionType != 0)
                                    {
                                        bool bgdr = SetGaming_DualResolutionType(monitorInfo, gaming.Current_DualResolutionType).Result;
                                    }
                                    if (gaming.IsEnable_VisionEngineType != null)
                                    {
                                        if (gaming.IsEnable_VisionEngineType.Length > 0)
                                        {
                                            bool bgv = SetGaming_VisionEngineEnableType(monitorInfo, gaming.IsEnable_VisionEngineType).Result;
                                        }
                                    }
                                }
                                writelog("[DisplayImportSettings]Import VCP");
                                if (vcps != null)
                                {
                                    if (vcps.Count > 0)
                                    {
                                        //set ImportVCPSequence
                                        impVCPSequence.ALSConfig = ImpExpSettings.MonitorSettings.ALSConfig;
                                        SetVCPSequence(monitorInfo, impVCPSequence, vcps);
                                        foreach (VCPCode code in vcps)
                                        {
                                            writelog("[DisplayImportSettings] VCP code : " + code.Code.ToString());
                                            if (importVCP.NotImportVCPs.FindIndex(x => x == code.Code) == -1 &&
                                                importVCP.ImportVCPSequence.FindIndex(x => x == code.Code) == -1)
                                            {
                                                bool b = false;
                                                ObjGetVCP objGetVCP = new ObjGetVCP();
                                                //SHR on/off need load settings
                                                //if (code.Code == 0xF0)
                                                //{
                                                //    b = _DisplayManagerPlugin.SetHDRStatus(monitorInfo, )
                                                //}
                                                if (ImpExpSettings.MonitorSettings.Gaming.Current_DualResolutionType != 0 && code.Code == 0xEA)
                                                {
                                                    continue;
                                                }
                                                //get vcp code
                                                objGetVCP = GetVCPCapability(monitorInfo, (byte)code.Code).Result;
                                                if (objGetVCP.result && (int)(uint)objGetVCP.value != (int)code.Value[0])
                                                {
                                                    //set vcp code
                                                    writelog("[DisplayImportSettings] Set VCP code : " + code.Code.ToString());
                                                    b = SetVCPCapability(monitorInfo, (byte)code.Code, (uint)code.Value[0]).Result;
                                                }
                                            }
                                        }
                                        writelog("[DisplayImportSettings] Import Success");
                                        writelog("[SentSettingstoTelementry] Send_Settings_Telementry : Import");
                                        ApplicationSettings_Function ApplicationSettings_Function = new ApplicationSettings_Function();
                                        if (_TelementryScheduler != null)
                                        {
                                            if (_AllInfoMonitors != null)
                                            {
                                                if (_AllInfoMonitors.Count > 0)
                                                {
                                                    if (ApplicationSettings_Function.Send_Settings_Telementry(_TelementryScheduler, _AllInfoMonitors, "Import"))
                                                    {
                                                        writelog("[SentSettingstoTelementry] Send_Settings_Telementry is success");
                                                    }
                                                    writelog("[SentSettingstoTelementry] Send_Settings_Telementry is fail");
                                                }
                                                else
                                                {
                                                    writelog("[SentSettingstoTelementry] _AllInfoMonitors count is 0");
                                                }
                                            }
                                            else
                                            {
                                                writelog("[SentSettingstoTelementry] _AllInfoMonitors is null");
                                            }
                                        }
                                        else
                                        {
                                            writelog("[SentSettingstoTelementry] _TelementryScheduler is null");
                                        }
                                        return Task.FromResult(true);
                                    }
                                    else
                                    {
                                        writelog("[DisplayImportSettings] VCPs List count is 0");
                                    }
                                }
                                else
                                {
                                    writelog("[DisplayImportSettings] VCPs List is null");
                                }
                            }
                            catch
                            {
                                writelog("[DisplayImportSettings] Import Fail");
                                return Task.FromResult(false);
                            }
                        }
                        else
                        {
                            writelog("[DisplayImportSettings] ImpExpSettings.MonitorSettings is null");
                        }
                    }
                    else
                    {
                        writelog("[DisplayImportSettings] ImpExpSettings is null");
                    }
                }
                else
                {
                    //Import DDMSettings
                    DDMImpSettings impSettings = new DDMImpSettings();
                    impSettings = _SettingsPlugin.ReadDDMImpSettingsFile(path).Result;
                    if (impSettings != null)
                    {
                        if (impSettings.MonitorSettings != null)
                        {
                            try
                            {
                                writelog("[DisplayImportSettings] Import DDM Input");
                                DDMtoDDPM_Input(impSettings.MonitorSettings);
                                writelog("[DisplayImportSettings] Import DDM DisplayProperties");
                                DisplayCurrentPropertiesInfo displayCurrentPropertiesInfo = new DisplayCurrentPropertiesInfo();
                                displayCurrentPropertiesInfo = DDMtoDDPM_DisplayProperties(impSettings.MonitorSettings);
                                if (Import_DisplayProperties(monitorInfo, displayCurrentPropertiesInfo))
                                {
                                    writelog("[DisplayImportSettings] Import_DisplayProperties Success");
                                }
                                List<DDPMMonitorSettings> monitorSettingsList = new List<DDPMMonitorSettings>();
                                monitorSettingsList = _SettingsPlugin.ReloadMonitorSettings(monitorInfo.modelName).Result;
                                if (monitorSettingsList != null)
                                {
                                    if (monitorSettingsList.Count > 0)
                                    {
                                        writelog("[DisplayImportSettings] Import DDM VCP");
                                        int index = monitorSettingsList.FindIndex(x => (x.ServiceTag == monitorInfo.edid.ServiceTag));
                                        vcps = monitorSettingsList[index].VCPs;
                                        SetVCPSequence(monitorInfo, impVCPSequence, vcps);
                                        foreach (VCPCode code in vcps)
                                        {
                                            writelog("[DisplayImportSettings] VCP code : " + code.Code.ToString());
                                            if (importVCP.NotImportVCPs.FindIndex(x => x == code.Code) == -1 &&
                                                importVCP.ImportVCPSequence.FindIndex(x => x == code.Code) == -1)
                                            {
                                                bool b = false;
                                                ObjGetVCP objGetVCP = new ObjGetVCP();
                                                //SHR on/off need load settings
                                                //if (code.Code == 0xF0)
                                                //{
                                                //    b = _DisplayManagerPlugin.SetHDRStatus(monitorInfo, )
                                                //}
                                                //get vcp code
                                                objGetVCP = GetVCPCapability(monitorInfo, (byte)code.Code).Result;
                                                if (objGetVCP.result && (int)(uint)objGetVCP.value != (int)code.Value[0])
                                                {
                                                    //set vcp code
                                                    writelog("[DisplayImportSettings] Set VCP code : " + code.Code.ToString());
                                                    b = SetVCPCapability(monitorInfo, (byte)code.Code, (uint)code.Value[0]).Result;
                                                }
                                            }
                                        }
                                        writelog("[DisplayImportSettings]import is Success");
                                        writelog("[SentSettingstoTelementry] Send_Settings_Telementry : Import");
                                        ApplicationSettings_Function ApplicationSettings_Function = new ApplicationSettings_Function();
                                        if (_TelementryScheduler != null)
                                        {
                                            if (_AllInfoMonitors != null)
                                            {
                                                if (_AllInfoMonitors.Count > 0)
                                                {
                                                    if (ApplicationSettings_Function.Send_Settings_Telementry(_TelementryScheduler, _AllInfoMonitors, "Import"))
                                                    {
                                                        writelog("[SentSettingstoTelementry] Send_Settings_Telementry is success");
                                                    }
                                                    writelog("[SentSettingstoTelementry] Send_Settings_Telementry is fail");
                                                }
                                                else
                                                {
                                                    writelog("[SentSettingstoTelementry] _AllInfoMonitors count is 0");
                                                }
                                            }
                                            else
                                            {
                                                writelog("[SentSettingstoTelementry] _AllInfoMonitors is null");
                                            }
                                        }
                                        else
                                        {
                                            writelog("[SentSettingstoTelementry] _TelementryScheduler is null");
                                        }
                                        return Task.FromResult(true);
                                    }
                                    else
                                    {
                                        writelog("[DisplayImportSettings]monitorSettingsList count is 0");
                                    }
                                }
                                else
                                {
                                    writelog("[DisplayImportSettings]monitorSettingsList is null");
                                }
                            }
                            catch
                            {
                                writelog("[DisplayImportSettings]import is Fail");
                                return Task.FromResult(false);
                            }
                        }
                    }
                    else
                    {
                        writelog("[DisplayImportSettings]DDMImpSettingsFile is null");
                    }
                }
            }
            return Task.FromResult(false);
        }

        public Task SetSameModel(MonitorInfo monitorInfo, bool isSameModel)
        {
            writelog("[SetSameModel]SetSameModel");
            if (_SettingsPlugin != null)
            {
                List<DDPMMonitorSettings> monitorSettingslist = new List<DDPMMonitorSettings>();
                monitorSettingslist = _SettingsPlugin.ReloadMonitorSettings(monitorInfo.modelName).Result;
                foreach (DDPMMonitorSettings settings in monitorSettingslist)
                {
                    if (settings.ServiceTag == monitorInfo.edid.ServiceTag)
                    {
                        if (settings.ImpExpSettings == null)
                        {
                            ImpExpSettings impExpSettings = new ImpExpSettings();
                            impExpSettings.SameModel = false;
                            settings.ImpExpSettings = impExpSettings;
                        }
                        settings.ImpExpSettings.SameModel = isSameModel;
                        break;
                    }
                }
                bool b = _SettingsPlugin.WriteMonitorSettings(monitorInfo.modelName, monitorSettingslist).Result;
            }
            return Task.CompletedTask;
        }

        public Task<bool> GetSameModel(MonitorInfo monitorInfo)
        {
            writelog("[GetSameModel]GetSameModel");
            if (_SettingsPlugin != null)
            {
                List<DDPMMonitorSettings> monitorSettingslist = new List<DDPMMonitorSettings>();
                monitorSettingslist = _SettingsPlugin.ReloadMonitorSettings(monitorInfo.modelName).Result;
                foreach (DDPMMonitorSettings settings in monitorSettingslist)
                {
                    if (settings.ServiceTag == monitorInfo.edid.ServiceTag)
                    {
                        if (settings.ImpExpSettings == null)
                        {
                            ImpExpSettings impExpSettings = new ImpExpSettings();
                            impExpSettings.SameModel = false;
                            settings.ImpExpSettings = impExpSettings;
                        }
                        return Task.FromResult(settings.ImpExpSettings.SameModel);
                    }
                }
            }
            return Task.FromResult(false);
        }

        #endregion

        #region Gaming

        public Task<GamingDisplayPropertiesInfo> GetGamingProperties_SupportedList(MonitorInfo monitorInfo)
        {
            if (_DisplayManagerPlugin != null)
            {
                return Task.FromResult(_DisplayManagerPlugin.GetGamingProperties_SupportedList(monitorInfo).Result);
            }
            return Task.FromResult(new GamingDisplayPropertiesInfo());
        }

        public Task<Gaming_GameEnhancementMode> GetCurrentGame_EnhancementMode(MonitorInfo monitorInfo)
        {
            if (_DisplayManagerPlugin != null)
            {
                return Task.FromResult(_DisplayManagerPlugin.GetCurrentGame_EnhancementMode(monitorInfo).Result);
            }
            return Task.FromResult(Gaming_GameEnhancementMode.Off);
        }

        public Task<Gaming_ResponseTime> GetCurrentGaming_ResponseTime(MonitorInfo monitorInfo)
        {
            if (_DisplayManagerPlugin != null)
            {
                return Task.FromResult(_DisplayManagerPlugin.GetCurrentGaming_ResponseTime(monitorInfo).Result);
            }
            return Task.FromResult(Gaming_ResponseTime.Disable);
        }

        public Task<Gaming_DarkStabilizer> GetCurrentGaming_DarkStabilizer(MonitorInfo monitorInfo)
        {
            if (_DisplayManagerPlugin != null)
            {
                return Task.FromResult(_DisplayManagerPlugin.GetCurrentGaming_DarkStabilizer(monitorInfo).Result);
            }
            return Task.FromResult(Gaming_DarkStabilizer.Disable);
        }

        public Task<Gaming_HDRType> GetCurrentGaming_HDRType(MonitorInfo monitorInfo)
        {
            if (_DisplayManagerPlugin != null)
            {
                return Task.FromResult(_DisplayManagerPlugin.GetCurrentGaming_HDRType(monitorInfo).Result);
            }
            return Task.FromResult(Gaming_HDRType.Off);
        }

        public Task<Gaming_DualResolutionType> GetCurrentGaming_DualResolutionType(MonitorInfo monitorInfo)
        {
            if (_DisplayManagerPlugin != null)
            {
                return Task.FromResult(_DisplayManagerPlugin.GetCurrentGaming_DualResolutionType(monitorInfo).Result);
            }
            return Task.FromResult(Gaming_DualResolutionType.Unknow);
        }

        public Task<Gaming_VisionEngineType> GetCurrentGaming_VisionEngineType(MonitorInfo monitorInfo)
        {
            if (_DisplayManagerPlugin != null)
            {
                return Task.FromResult(_DisplayManagerPlugin.GetCurrentGaming_VisionEngineType(monitorInfo).Result);
            }
            return Task.FromResult(Gaming_VisionEngineType.off);
        }

        public Task<bool[]> GetCurrentGaming_VisionEngineEnableType(MonitorInfo monitorInfo, GamingDisplayPropertiesInfo gamingDisplayPropertiesInfo)
        {
            if (_DisplayManagerPlugin != null)
            {
                return Task.FromResult(_DisplayManagerPlugin.GetCurrentGaming_VisionEngineEnableType(monitorInfo, gamingDisplayPropertiesInfo).Result);
            }
            return Task.FromResult(new bool[0]);
        }

        public Task<bool> SetGameEnhancementMode(MonitorInfo monitorInfo, Gaming_GameEnhancementMode GameEnhancementMode)
        {
            if (_DisplayManagerPlugin != null)
            {
                //Telementry Collection
                var rt = false;
                var Displaysettings_Function = new Displaysettings_Function();
                rt = Displaysettings_Function.Send_GamingEnhancementMode_Telementry(_TelementryScheduler, monitorInfo, GameEnhancementMode, GetMonitorCurrentResolution(monitorInfo), GetMonitorMaxResolution(monitorInfo));
                if (rt) writelog("[DeviceMangerPlugin] Send Telementry for GamingEnhancementMode Success ...");
                else writelog("[DeviceMangerPlugin] Send Telementry for GamingEnhancementMode Fail ...");
                return Task.FromResult(_DisplayManagerPlugin.SetGameEnhancementMode(monitorInfo, GameEnhancementMode).Result);
            }

            return Task.FromResult(false);
        }

        public Task<bool> SetGaming_ResponseTime(MonitorInfo monitorInfo, Gaming_ResponseTime ResponseTime)
        {
            if (_DisplayManagerPlugin != null)
            {
                var rt = false;
                var Displaysettings_Function = new Displaysettings_Function();
                rt = Displaysettings_Function.Send_GamingResponseTime_Telementry(_TelementryScheduler, monitorInfo, ResponseTime, GetMonitorCurrentResolution(monitorInfo), GetMonitorMaxResolution(monitorInfo));
                if (rt) writelog("[DeviceMangerPlugin] Send Telementry for GamingResponseTime Success ...");
                else writelog("[DeviceMangerPlugin] Send Telementry for GamingResponseTime Fail ...");
                return Task.FromResult(_DisplayManagerPlugin.SetGaming_ResponseTime(monitorInfo, ResponseTime).Result);
            }
            return Task.FromResult(false);
        }

        public Task<bool> SetGaming_DarkStabilizer(MonitorInfo monitorInfo, Gaming_DarkStabilizer DarkStabilizer)
        {
            if (_DisplayManagerPlugin != null)
            {
                var rt = false;
                var Displaysettings_Function = new Displaysettings_Function();
                rt = Displaysettings_Function.Send_GamingDarkStabilizer_Telementry(_TelementryScheduler, monitorInfo, DarkStabilizer, GetMonitorCurrentResolution(monitorInfo), GetMonitorMaxResolution(monitorInfo));
                if (rt) writelog("[DeviceMangerPlugin] Send Telementry for GamingDarkStabilizer Success ...");
                else writelog("[DeviceMangerPlugin] Send Telementry for GamingDarkStabilizer Fail ...");
                return Task.FromResult(_DisplayManagerPlugin.SetGaming_DarkStabilizer(monitorInfo, DarkStabilizer).Result);
            }
            return Task.FromResult(false);
        }

        public Task<bool> SetGaming_HDRType(MonitorInfo monitorInfo, Gaming_HDRType HDRType)
        {
            if (_DisplayManagerPlugin != null)
            {
                var rt = false;
                var Displaysettings_Function = new Displaysettings_Function();
                rt = Displaysettings_Function.Send_GamingHDRType_Telementry(_TelementryScheduler, monitorInfo, HDRType, GetMonitorCurrentResolution(monitorInfo), GetMonitorMaxResolution(monitorInfo));
                if (rt) writelog("[DeviceMangerPlugin] Send Telementry for GamingHDRType Success ...");
                else writelog("[DeviceMangerPlugin] Send Telementry for GamingHDRType Fail ...");
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

        public Task<bool> SetGaming_VisionEngineEnableType(MonitorInfo monitorInfo, bool[] VisionEngineEnableType)
        {
            if (_DisplayManagerPlugin != null)
            {
                return Task.FromResult(_DisplayManagerPlugin.SetGaming_VisionEngineEnableType(monitorInfo, VisionEngineEnableType).Result);
            }
            return Task.FromResult(false);
        }

        private Task<bool> SwitchGaming_VisionEngineType(MonitorInfo monitorInfo, Gaming_VisionEngineType VisionEngineType)
        {
            if (_DisplayManagerPlugin != null)
            {
                var rt = false;
                var Displaysettings_Function = new Displaysettings_Function();
                rt = Displaysettings_Function.Send_GamingVisionEngine_Telementry(_TelementryScheduler, monitorInfo, VisionEngineType, GetMonitorCurrentResolution(monitorInfo), GetMonitorMaxResolution(monitorInfo));
                if (rt) writelog("[DeviceMangerPlugin] Send Telementry for GamingVisionEngine Success ...");
                else writelog("[DeviceMangerPlugin] Send Telementry for GamingVisionEngine Fail ...");
                return Task.FromResult(_DisplayManagerPlugin.SwitchGaming_VisionEngineType(monitorInfo, VisionEngineType).Result);
            }
            return Task.FromResult(false);
        }

        #endregion

        #region DTPProxy implementation

        #region Mouse

        public async Task<int> GetDpiValue(string Guid)
        {
            return await Task.Run(() => _DTPProxyPlugin.GetDpiValue(Guid));
        }

        public async Task<JArray> GetMouseAssignableActions(string Guid)
        {
            return await Task.Run(() => _DTPProxyPlugin.GetMouseAssignableActions(Guid));
        }

        public async Task<JArray> GetMouseProgrammableKeys(string Guid)
        {
            return await Task.Run(() => _DTPProxyPlugin.GetMouseProgrammableKeys(Guid));
        }

        public async Task<JArray> GetAppSpecificProfiles(string Guid)
        {
            return await Task.Run(() => _DTPProxyPlugin.GetAppSpecificProfiles(Guid));
        }

        public async Task<bool> DeleteMouseAllAssignedActions(string Guid)
        {
            return await Task.Run(() => _DTPProxyPlugin.DeleteMouseAllAssignedActions(Guid));
        }

        public async Task<string> GetMouseKeystrokeDisplayData(string Guid)
        {
            return await Task.Run(() => _DTPProxyPlugin.GetMouseKeystrokeDisplayData(Guid));
        }

        public async Task<bool> StartMouseKeystrokeRecording(string Guid)
        {
            return await Task.Run(() => _DTPProxyPlugin.StartMouseKeystrokeRecording(Guid));
        }

        public async Task<bool> StopMouseKeystrokeRecording(string Guid)
        {
            return await Task.Run(() => _DTPProxyPlugin.StopMouseKeystrokeRecording(Guid));
        }

        public Task SetDPIValue(string Guid, int newValue)
        {
            writelog("DeviceMangerPlugin received SetDPIValueByDTP requested ...");
            writelog($"Target Guid is {Guid}");
            writelog($"Target DPI Value is {newValue}");
            _DTPProxyPlugin.SetDpiValue(Guid, newValue);
            return Task.FromResult(true);
        }

        public Task SetMouseAction(string Guid, byte[] newValue)
        {
            writelog("DeviceMangerPlugin received SetMouseAction requested ...");
            writelog($"Target Guid is {Guid}");
            writelog($"Target Value is {newValue}");
            _DTPProxyPlugin.SetMouseAction(Guid, newValue);
            return Task.FromResult(true);
        }

        public Task SetCurrentSelectedAppSpecificProfile(string Guid, string newValue)
        {
            writelog("DeviceMangerPlugin received SetMouseAction requested ...");
            writelog($"Target Guid is {Guid}");
            writelog($"Target Value is {newValue}");
            _DTPProxyPlugin.SetCurrentSelectedAppSpecificProfile(Guid, newValue);
            return Task.FromResult(true);
        }

        public Task DeleteMouseAssignedAction(string Guid, int newValue)
        {
            writelog("DeviceMangerPlugin received DeleteAssignedAction requested ...");
            writelog($"Target Guid is {Guid}");
            writelog($"Target Value is {newValue}");
            _DTPProxyPlugin.DeleteMouseAssignedAction(Guid, newValue);
            return Task.FromResult(true);
        }

        public Task SetMouseAssignDialogAction(string Guid, byte[] newValue)
        {
            writelog("DeviceMangerPlugin received SetMouseAssignDialogAction requested ...");
            writelog($"Target Guid is {Guid}");
            writelog($"Target Value is {newValue}");
            _DTPProxyPlugin.SetMouseAssignDialogAction(Guid, newValue);
            return Task.FromResult(true);
        }

        public Task SetMouseAssignKeystrokeAction(string Guid, byte[] newValue)
        {
            writelog("DeviceMangerPlugin received SetMouseAssignKeystrokeAction requested ...");
            writelog($"Target Guid is {Guid}");
            writelog($"Target Value is {newValue}");
            _DTPProxyPlugin.SetMouseAssignKeystrokeAction(Guid, newValue);
            return Task.FromResult(true);
        }

        #endregion

        #region Pen

        public async Task<JArray> GetPenDeviceItemsEx()
        {
            return await Task.Run(() => _DTPProxyPlugin.GetPenDeviceItemsEx());
        }

        public Task<string> PairingPen()
        {
            writelog("DeviceMangerPlugin received PairingPen requested ...");
            return _DTPProxyPlugin.PairingPen();
        }

        public Task UnPairPen(string Guid)
        {
            writelog("DeviceMangerPlugin received PairingPen requested ...");
            return _DTPProxyPlugin.UnPairPen(Guid);
        }

        public Task<string> GetEraserDoublePressValues()
        {
            return _DTPProxyPlugin.GetEraserDoublePressValues();
        }

        public Task<string> GetEraserSinglePressValues()
        {
            return _DTPProxyPlugin.GetEraserSinglePressValues();
        }

        public Task<string> GetEraserLongPressValues()
        {
            return _DTPProxyPlugin.GetEraserLongPressValues();
        }

        public Task<string> GetSideSwitchSinglePressValues()
        {
            return _DTPProxyPlugin.GetSideSwitchSinglePressValues();
        }

        public Task<string> GetMenuSinglePressValues()
        {
            return _DTPProxyPlugin.GetMenuSinglePressValues();
        }

        public Task<string> GetLaunchableAppValues()
        {
            return _DTPProxyPlugin.GetLaunchableAppValues();
        }

        public Task<string> GetEraserDoublePressSetting()
        {
            return _DTPProxyPlugin.GetEraserDoublePressSetting();
        }

        public Task<string> GetEraserSinglePressSetting()
        {
            return _DTPProxyPlugin.GetEraserSinglePressSetting();
        }

        public Task<string> GetEraserLongPressSetting()
        {
            return _DTPProxyPlugin.GetEraserLongPressSetting();
        }

        public Task<string> GetSideTopSwitchSinglePressSetting()
        {
            return _DTPProxyPlugin.GetSideTopSwitchSinglePressSetting();
        }

        public Task<string> GetSideBottomSwitchSinglePressSetting()
        {
            return _DTPProxyPlugin.GetSideBottomSwitchSinglePressSetting();
        }

        public Task<string> GetMenuSinglePressSetting()
        {
            return _DTPProxyPlugin.GetMenuSinglePressSetting();
        }

        public Task<bool> GetMenuCenterRightClickSetting()
        {
            return _DTPProxyPlugin.GetMenuCenterRightClickSetting();
        }

        public Task<bool> GetIsSideTopButtonHoverClick()
        {
            return _DTPProxyPlugin.GetIsSideTopButtonHoverClick();
        }

        public Task<bool> GetIsSideBottomButtonHoverClick()
        {
            return _DTPProxyPlugin.GetIsSideBottomButtonHoverClick();
        }

        public Task SetEraserDoublePressSetting(string itemID, byte[] newValue)
        {
            writelog("DeviceMangerPlugin received SetEraserDoublePressSetting requested ...");
            writelog($"Target itemID is {itemID}");
            writelog($"Target Value is {newValue}");
            _DTPProxyPlugin.SetEraserDoublePressSetting(itemID, newValue);
            return Task.FromResult(true);
        }

        public Task SetEraserLongPressSetting(string itemID, byte[] newValue)
        {
            writelog("DeviceMangerPlugin received SetEraserLongPressSetting requested ...");
            writelog($"Target itemID is {itemID}");
            writelog($"Target Value is {newValue}");
            _DTPProxyPlugin.SetEraserLongPressSetting(itemID, newValue);
            return Task.FromResult(true);
        }

        public Task SetEraserSinglePressSetting(string itemID, byte[] newValue)
        {
            writelog("DeviceMangerPlugin received SetEraserSinglePressSetting requested ...");
            writelog($"Target itemID is {itemID}");
            writelog($"Target Value is {newValue}");
            _DTPProxyPlugin.SetEraserSinglePressSetting(itemID, newValue);
            return Task.FromResult(true);
        }

        public Task SetIsSideBottomButtonHoverClick(string itemID, bool newValue)
        {
            writelog("DeviceMangerPlugin received SetIsSideBottomButtonHoverClick requested ...");
            writelog($"Target itemID is {itemID}");
            writelog($"Target Value is {newValue}");
            _DTPProxyPlugin.SetIsSideBottomButtonHoverClick(itemID, newValue);
            return Task.FromResult(true);
        }

        public Task SetIsSideTopButtonHoverClick(string itemID, bool newValue)
        {
            writelog("DeviceMangerPlugin received SetIsSideTopButtonHoverClick requested ...");
            writelog($"Target itemID is {itemID}");
            writelog($"Target Value is {newValue}");
            _DTPProxyPlugin.SetIsSideTopButtonHoverClick(itemID, newValue);
            return Task.FromResult(true);
        }

        public Task SetMenuSinglePressSetting(string itemID, byte[] newValue)
        {
            writelog("DeviceMangerPlugin received SetMenuSinglePressSetting requested ...");
            writelog($"Target itemID is {itemID}");
            writelog($"Target Value is {newValue}");
            _DTPProxyPlugin.SetMenuSinglePressSetting(itemID, newValue);
            return Task.FromResult(true);
        }

        public Task SetMenuCenterRightClickSetting(string itemID, bool newValue)
        {
            writelog("DeviceMangerPlugin received SetMenuCenterRightClickSetting requested ...");
            writelog($"Target itemID is {itemID}");
            writelog($"Target Value is {newValue}");
            _DTPProxyPlugin.SetMenuCenterRightClickSetting(itemID, newValue);
            return Task.FromResult(true);
        }

        public Task SetSideBottomSwitchSinglePressSetting(string itemID, byte[] newValue)
        {
            writelog("DeviceMangerPlugin received SetSideBottomSwitchSinglePressSetting requested ...");
            writelog($"Target itemID is {itemID}");
            writelog($"Target Value is {newValue}");
            _DTPProxyPlugin.SetSideBottomSwitchSinglePressSetting(itemID, newValue);
            return Task.FromResult(true);
        }

        public Task SetSideTopSwitchSinglePressSetting(string itemID, byte[] newValue)
        {
            writelog("DeviceMangerPlugin received SetSideTopSwitchSinglePressSetting requested ...");
            writelog($"Target itemID is {itemID}");
            writelog($"Target Value is {newValue}");
            _DTPProxyPlugin.SetSideTopSwitchSinglePressSetting(itemID, newValue);
            return Task.FromResult(true);
        }

        public Task SetTiltSensitivity(string itemID, int newValue)
        {
            writelog("DeviceMangerPlugin received SetTiltSensitivity requested ...");
            writelog($"Target itemID is {itemID}");
            writelog($"Target Value is {newValue}");
            _DTPProxyPlugin.SetTiltSensitivity(itemID, newValue);
            return Task.FromResult(true);
        }

        public Task SetTipSensitivity(string itemID, int newValue)
        {
            writelog("DeviceMangerPlugin received SetTipSensitivity requested ...");
            writelog($"Target itemID is {itemID}");
            writelog($"Target Value is {newValue}");
            _DTPProxyPlugin.SetTipSensitivity(itemID, newValue);
            return Task.FromResult(true);
        }

        #endregion

        #region keyboard

        public async Task<JArray> GetKeyboardDeviceItemsEx()
        {
            return await Task.Run(() => _DTPProxyPlugin.GetKeyboardDeviceItemsEx());
        }

        public Task DeleteKeyboardAssignedAction(string Guid, int newValue)
        {
            writelog("DeviceMangerPlugin received SetKbAssignKeystrokeAction requested ...");
            _DTPProxyPlugin.DeleteKeyboardAssignedAction(Guid, newValue);
            return Task.FromResult(true);
        }

        public Task<JArray> GetKbProgrammableKeys(string Guid)
        {
            return Task.Run(() => _DTPProxyPlugin.GetKbProgrammableKeys(Guid));
        }

        public Task<bool> DeleteKeyboardAllAssignedActions(string Guid)
        {
            return Task.Run(() => _DTPProxyPlugin.DeleteKeyboardAllAssignedActions(Guid));
        }

        public Task<JArray> GetKbAssignableActions(string Guid)
        {
            return Task.Run(() => _DTPProxyPlugin.GetKbAssignableActions(Guid));
        }

        public Task SetKbAssignKeystrokeAction(string Guid, string newValue)
        {
            writelog("DeviceMangerPlugin received SetKbAssignKeystrokeAction requested ...");
            _DTPProxyPlugin.SetKbAssignKeystrokeAction(Guid, newValue);
            return Task.FromResult(true);
        }

        public Task SetKbAssignDialogAction(string Guid, string newValue)
        {
            writelog("DeviceMangerPlugin received SetKbAssignDialogAction requested ...");
            _DTPProxyPlugin.SetKbAssignDialogAction(Guid, newValue);
            return Task.FromResult(true);
        }

        public Task SetKbAssignedAction(string Guid, string newValue)
        {
            writelog("DeviceMangerPlugin received SetKbAssignedAction requested ...");
            _DTPProxyPlugin.SetKbAssignedAction(Guid, newValue);
            return Task.FromResult(true);
        }

        #endregion

        #region Webcam

        public async Task<JArray> GetPresetProfiles(string Guid)
        {
            return await Task.Run(() => _DTPProxyPlugin.GetPresetProfiles(Guid));
        }

        public async Task<JArray> GetCustomProfiles(string Guid)
        {
            return await Task.Run(() => _DTPProxyPlugin.GetCustomProfiles(Guid));
        }

        public async Task<string> GetProfile(string Guid)
        {
            return await Task.Run(() => _DTPProxyPlugin.GetProfile(Guid));
        }

        public async Task<string> GetProfileName(string Guid)
        {
            return await Task.Run(() => _DTPProxyPlugin.GetProfileName(Guid));
        }

        public async Task<int> GetBrightness(string Guid)
        {
            return await Task.Run(() => _DTPProxyPlugin.GetBrightness(Guid));
        }

        public async Task<string> GetCameraFirmwareVersionByDTP(string Guid)
        {
            return await Task.Run(() => _DTPProxyPlugin.GetCameraFirmwareVersion(Guid));
        }

        public async Task<bool> GetIsPropertyFOVSupportedByDTP(string Guid)
        {
            return await Task.Run(() => _DTPProxyPlugin.GetIsPropertyFOVSupported(Guid));
        }

        public async Task<int> GetFieldOfView(string Guid)
        {
            return await Task.Run(() => _DTPProxyPlugin.GetFieldOfView(Guid));
        }

        public async Task<bool> GetIsPropertyHDRSupported(string Guid)
        {
            return await Task.Run(() => _DTPProxyPlugin.GetIsPropertyHDRSupported(Guid));
        }

        public async Task<bool> GetIsHDROn(string Guid)
        {
            return await Task.Run(() => _DTPProxyPlugin.GetIsHDROn(Guid));
        }

        public async Task<bool> GetIsPropertyAntiFlickerSupported(string Guid)
        {
            return await Task.Run(() => _DTPProxyPlugin.CheckIsPropertyAntiFlickerSupported(Guid));
        }

        public async Task<int> GetAntiFlickerValueByDTP(string Guid)
        {
            return await Task.Run(() => _DTPProxyPlugin.GetAntiFlicker(Guid));
        }

        public async Task<bool> GetIsPropertyAutoFramingSupported(string Guid)
        {
            return await Task.Run(() => _DTPProxyPlugin.GetIsPropertyAutoFramingSupported(Guid));
        }

        public async Task<bool> GetIsAutoFramingOn(string Guid)
        {
            return await Task.Run(() => _DTPProxyPlugin.GetIsAutoFramingOn(Guid));
        }

        public async Task<string> GetSupportedResolutions(string Guid)
        {
            return await Task.Run(() => _DTPProxyPlugin.GetSupportedResolutions(Guid));
        }

        public async Task<string> GetSelectedResolution(string Guid)
        {
            return await Task.Run(() => _DTPProxyPlugin.GetSelectedResolution(Guid));
        }

        public Task SetIsMicEnumerationOn(string guid, bool newValue)
        {
            writelog("DeviceMangerPlugin received SetIsMicEnumerationOn requested ...");
            writelog($"Target Guid is {guid}");
            writelog($"Target Value is {newValue}");
            _DTPProxyPlugin.SetIsMicEnumerationOn(guid, newValue);
            return Task.FromResult(true);
        }

        public Task SetProfile(string guid, string newValue)
        {
            writelog("DeviceMangerPlugin received SetProfile requested ...");
            writelog($"Target Guid is {guid}");
            writelog($"Target Value is {newValue}");
            _DTPProxyPlugin.SetProfile(guid, newValue);
            return Task.FromResult(true);
        }

        public Task SetProfileName(string guid, string newValue)
        {
            writelog("DeviceMangerPlugin received SetProfileName requested ...");
            writelog($"Target Guid is {guid}");
            writelog($"Target Value is {newValue}");
            _DTPProxyPlugin.SetProfileName(guid, newValue);
            return Task.FromResult(true);
        }

        public Task CreateCustomProfile(string guid, string newValue)
        {
            writelog("DeviceMangerPlugin received CreateCustomProfile requested ...");
            writelog($"Target Guid is {guid}");
            writelog($"Target Value is {newValue}");
            _DTPProxyPlugin.CreateCustomProfile(guid, newValue);
            return Task.FromResult(true);
        }

        public Task DeleteProfile(string guid, string newValue)
        {
            writelog("DeviceMangerPlugin received DeleteProfile requested ...");
            writelog($"Target Guid is {guid}");
            writelog($"Target Value is {newValue}");
            _DTPProxyPlugin.DeleteProfile(guid, newValue);
            return Task.FromResult(true);
        }

        public Task SetZoom(string guid, int newValue)
        {
            writelog("DeviceMangerPlugin received SetZoom requested ...");
            writelog($"Target Guid is {guid}");
            writelog($"Target Value is {newValue}");
            _DTPProxyPlugin.SetZoom(guid, newValue);
            return Task.FromResult(true);
        }

        public Task SetAutoFramingSensitivity(string guid, int newValue)
        {
            writelog("DeviceMangerPlugin received SetAutoFramingSensitivity requested ...");
            writelog($"Target Guid is {guid}");
            writelog($"Target Value is {newValue}");
            _DTPProxyPlugin.SetAutoFramingSensitivity(guid, newValue);
            return Task.FromResult(true);
        }

        public Task SetAutoFramingFrameSize(string guid, int newValue)
        {
            writelog("DeviceMangerPlugin received SetAutoFramingFrameSize requested ...");
            writelog($"Target Guid is {guid}");
            writelog($"Target Value is {newValue}");
            _DTPProxyPlugin.SetAutoFramingFrameSize(guid, newValue);
            return Task.FromResult(true);
        }

        public Task SetIsAutoFramingOn(string guid, bool newValue)
        {
            writelog("DeviceMangerPlugin received SetIsAutoFramingOn requested ...");
            writelog($"Target Guid is {guid}");
            writelog($"Target Value is {newValue}");
            _DTPProxyPlugin.SetIsAutoFramingOn(guid, newValue);
            return Task.FromResult(true);
        }

        public Task SetIsAutoFramingTransitionOn(string guid, bool newValue)
        {
            writelog("DeviceMangerPlugin received SetIsAutoFramingTransitionOn requested ...");
            writelog($"Target Guid is {guid}");
            writelog($"Target Value is {newValue}");
            _DTPProxyPlugin.SetIsAutoFramingTransitionOn(guid, newValue);
            return Task.FromResult(true);
        }

        public Task SetFieldOfView(string guid, int newValue)
        {
            writelog("DeviceMangerPlugin received SetFieldOfView requested ...");
            writelog($"Target Guid is {guid}");
            writelog($"Target Value is {newValue}");
            _DTPProxyPlugin.SetFieldOfView(guid, newValue);
            return Task.FromResult(true);
        }

        public Task SetIsFocusOn(string guid, bool newValue)
        {
            writelog("DeviceMangerPlugin received SetIsFocusOn requested ...");
            writelog($"Target Guid is {guid}");
            writelog($"Target Value is {newValue}");
            _DTPProxyPlugin.SetIsFocusOn(guid, newValue);
            return Task.FromResult(true);
        }

        public Task SetFocus(string guid, int newValue)
        {
            writelog("DeviceMangerPlugin received SetFocus requested ...");
            writelog($"Target Guid is {guid}");
            writelog($"Target Value is {newValue}");
            _DTPProxyPlugin.SetFocus(guid, newValue);
            return Task.FromResult(true);
        }

        public Task SetPriority(string guid, int newValue)
        {
            writelog("DeviceMangerPlugin received SetPriority requested ...");
            writelog($"Target Guid is {guid}");
            writelog($"Target Value is {newValue}");
            _DTPProxyPlugin.SetPriority(guid, newValue);
            return Task.FromResult(true);
        }

        public Task SetIsHDROn(string guid, bool newValue)
        {
            writelog("DeviceMangerPlugin received SetIsHDROn requested ...");
            writelog($"Target Guid is {guid}");
            writelog($"Target Value is {newValue}");
            _DTPProxyPlugin.SetIsHDROn(guid, newValue);
            return Task.FromResult(true);
        }

        public Task SetIsAutoWhiteBalanceOn(string guid, bool newValue)
        {
            writelog("DeviceMangerPlugin received SetIsAutoWhiteBalanceOn requested ...");
            writelog($"Target Guid is {guid}");
            writelog($"Target Value is {newValue}");
            _DTPProxyPlugin.SetIsAutoWhiteBalanceOn(guid, newValue);
            return Task.FromResult(true);
        }

        public Task SetAutoWhiteBalance(string guid, int newValue)
        {
            writelog("DeviceMangerPlugin received SetAutoWhiteBalance requested ...");
            writelog($"Target Guid is {guid}");
            writelog($"Target Value is {newValue}");
            _DTPProxyPlugin.SetAutoWhiteBalance(guid, newValue);
            return Task.FromResult(true);
        }

        public Task SetWALTime(string guid, int newValue)
        {
            writelog("DeviceMangerPlugin received SetWALTime requested ...");
            writelog($"Target Guid is {guid}");
            writelog($"Target Value is {newValue}");
            _DTPProxyPlugin.SetWALTime(guid, newValue);
            return Task.FromResult(true);
        }

        public Task SetSnooze(string guid, int newValue)
        {
            writelog("DeviceMangerPlugin received SetSnooze requested ...");
            writelog($"Target Guid is {guid}");
            writelog($"Target Value is {newValue}");
            _DTPProxyPlugin.SetSnooze(guid, newValue);
            return Task.FromResult(true);
        }

        public Task SetSnoozeLength(string guid, int newValue)
        {
            writelog("DeviceMangerPlugin received SetSnoozeLength requested ...");
            writelog($"Target Guid is {guid}");
            writelog($"Target Value is {newValue}");
            _DTPProxyPlugin.SetSnoozeLength(guid, newValue);
            return Task.FromResult(true);
        }

        public Task SetIsProximitySensorEnable(string guid, bool newValue)
        {
            writelog("DeviceMangerPlugin received SetIsProximitySensorEnable requested ...");
            writelog($"Target Guid is {guid}");
            writelog($"Target Value is {newValue}");
            _DTPProxyPlugin.SetIsProximitySensorEnable(guid, newValue);
            return Task.FromResult(true);
        }

        public Task SetIsWakeonApproachEnable(string guid, bool newValue)
        {
            writelog("DeviceMangerPlugin received SetIsWakeonApproachEnable requested ...");
            writelog($"Target Guid is {guid}");
            writelog($"Target Value is {newValue}");
            _DTPProxyPlugin.SetIsWakeonApproachEnable(guid, newValue);
            return Task.FromResult(true);
        }

        public Task SetIsWalkAwayLockEnable(string guid, bool newValue)
        {
            writelog("DeviceMangerPlugin received SetIsWalkAwayLockEnable requested ...");
            writelog($"Target Guid is {guid}");
            writelog($"Target Value is {newValue}");
            _DTPProxyPlugin.SetIsWalkAwayLockEnable(guid, newValue);
            return Task.FromResult(true);
        }

        public Task SetIsPrioritizeExternalWebcam(string guid, bool newValue)
        {
            writelog("DeviceMangerPlugin received SetIsPrioritizeExternalWebcam requested ...");
            writelog($"Target Guid is {guid}");
            writelog($"Target Value is {newValue}");
            _DTPProxyPlugin.SetIsPrioritizeExternalWebcam(guid, newValue);
            return Task.FromResult(true);
        }

        public Task ResetToDefault_webcam(string guid, bool newValue)
        {
            writelog("DeviceMangerPlugin received ResetToDefault_webcam requested ...");
            writelog($"Target Guid is {guid}");
            writelog($"Target Value is {newValue}");
            _DTPProxyPlugin.ResetToDefault_webcam(guid, newValue);
            return Task.FromResult(true);
        }

        public Task SetBrightness(string guid, int newValue)
        {
            writelog("DeviceMangerPlugin received SetBrightness requested ...");
            writelog($"Target Guid is {guid}");
            writelog($"Target Value is {newValue}");
            _DTPProxyPlugin.SetBrightness(guid, newValue);
            return Task.FromResult(true);
        }

        public Task SetSharpness(string guid, int newValue)
        {
            writelog("DeviceMangerPlugin received SetSharpness requested ...");
            writelog($"Target Guid is {guid}");
            writelog($"Target Value is {newValue}");
            _DTPProxyPlugin.SetSharpness(guid, newValue);
            return Task.FromResult(true);
        }

        public Task SetContrast(string guid, int newValue)
        {
            writelog("DeviceMangerPlugin received SetContrast requested ...");
            writelog($"Target Guid is {guid}");
            writelog($"Target Value is {newValue}");
            _DTPProxyPlugin.SetContrast(guid, newValue);
            return Task.FromResult(true);
        }

        public Task SetSaturation(string guid, int newValue)
        {
            writelog("DeviceMangerPlugin received SetSaturation requested ...");
            writelog($"Target Guid is {guid}");
            writelog($"Target Value is {newValue}");
            _DTPProxyPlugin.SetSaturation(guid, newValue);
            return Task.FromResult(true);
        }

        public Task SetAntiFlicker(string guid, int newValue)
        {
            writelog("DeviceMangerPlugin received SetAntiFlicker requested ...");
            writelog($"Target Guid is {guid}");
            writelog($"Target Value is {newValue}");
            _DTPProxyPlugin.SetAntiFlicker(guid, newValue);
            return Task.FromResult(true);
        }

        public Task SetTilt(string guid, int newValue)
        {
            writelog("DeviceMangerPlugin received SetTilt requested ...");
            writelog($"Target Guid is {guid}");
            writelog($"Target Value is {newValue}");
            _DTPProxyPlugin.SetTilt(guid, newValue);
            return Task.FromResult(true);
        }

        public Task SetPan(string guid, int newValue)
        {
            writelog("DeviceMangerPlugin received SetPan requested ...");
            writelog($"Target Guid is {guid}");
            writelog($"Target Value is {newValue}");
            _DTPProxyPlugin.SetPan(guid, newValue);
            return Task.FromResult(true);
        }

        public async Task<int> GetWALTime(string guid)
        {
            return await Task.Run(() => _DTPProxyPlugin.GetWALTime(guid));
        }

        public async Task<int> GetSnooze(string guid)
        {
            return await Task.Run(() => _DTPProxyPlugin.GetSnooze(guid));
        }

        public async Task<int> GetSnoozeLength(string guid)
        {
            return await Task.Run(() => _DTPProxyPlugin.GetSnoozeLength(guid));
        }

        public async Task<bool> GetIsProximitySensorEnable(string guid)
        {
            return await Task.Run(() => _DTPProxyPlugin.GetIsProximitySensorEnable(guid));
        }

        public async Task<bool> GetIsWakeonApproachEnable(string guid)
        {
            return await Task.Run(() => _DTPProxyPlugin.GetIsWakeonApproachEnable(guid));
        }

        public async Task<bool> GetIsWalkAwayLockEnable(string guid)
        {
            return await Task.Run(() => _DTPProxyPlugin.GetIsWalkAwayLockEnable(guid));
        }

        public async Task<bool> GetIsPrioritizeExternalWebcam(string guid)
        {
            return await Task.Run(() => _DTPProxyPlugin.GetIsPrioritizeExternalWebcam(guid));
        }

        #endregion

        #endregion

        public Task<bool> SentKVMtoTelementry(MonitorInfo monitorInfo, string mode, string val)
        {
            writelog("[SentKVMtoTelementry] SentKVMToTelementry");
            DisplayFeatures_Functions displayFeatures_Functions = new DisplayFeatures_Functions();
            DisplayPropertiesInfo displayInfo = GetDisplayPropertiesInfo(monitorInfo).Result;
            if (_TelementryScheduler != null)
            {
                if (displayFeatures_Functions.SentKVMToTelementry(Log, _TelementryScheduler, monitorInfo, displayInfo, mode, val))
                {
                    writelog("[SentKVMtoTelementry] SentKVMToTelementry is success");
                    return Task.FromResult(true);
                }
                writelog("[SentKVMtoTelementry] SentKVMToTelementry is fail");
            }
            else
            {
                writelog("[SentKVMtoTelementry] _TelementryScheduler is null");
            }
            return Task.FromResult(false);
        }

        #endregion

        #region GlobalSetting

        public Task<GlobalSettingParam> GetGlobalSettingParam()
        {
            return Task.FromResult(_GlobalSettingParam);
        }

        public Task<bool> Set_GlobalSetting_DisplayLowBatteryLevel(bool isDisplay)
        {
            bool ret = false;
            if (_SettingsPlugin != null)
            {
                _GlobalSettingParam.GlobalSetting_General.Low_Battery_Level = isDisplay;
                ret = SaveGlobalSettingParam();
                //Telementry Collection
                var rt = false;
                var ApplicationSettings_Function = new ApplicationSettings_Function();
                writelog("[DeviceMangerPlugin] Send Telementry for DisplayLowBatteryLevel...");
                rt = ApplicationSettings_Function.Send_LowBatteryLevel_Telementry(_TelementryScheduler, _AllInfoMonitors, isDisplay);
                if (rt) writelog("[DeviceMangerPlugin] Send Telementry for DisplayLowBatteryLevel Success ...");
                else writelog("[DeviceMangerPlugin] Send Telementry for DisplayLowBatteryLevel Fail ...");
            }
            GlobalSettingChangeEvent?.Invoke(this, null);
            return Task.FromResult(ret);
        }

        public Task<bool> Set_GlobalSetting_DisplayKeyboardLockKey(bool isDisplay)
        {
            bool ret = false;
            if (_SettingsPlugin != null)
            {
                _GlobalSettingParam.GlobalSetting_General.Keyboard_Lock_Key = isDisplay;
                ret = SaveGlobalSettingParam();
                //Telementry Collection
                var rt = false;
                var ApplicationSettings_Function = new ApplicationSettings_Function();
                writelog("[DeviceMangerPlugin] Send Telementry for DisplayKeyboardLockKey...");
                rt = ApplicationSettings_Function.Send_KeyboardLockKey_Telementry(_TelementryScheduler, _AllInfoMonitors, isDisplay);
                if (rt) writelog("[DeviceMangerPlugin] Send Telementry for DisplayKeyboardLockKey Success ...");
                else writelog("[DeviceMangerPlugin] Send Telementry for DisplayKeyboardLockKey Fail ...");
            }
            GlobalSettingChangeEvent?.Invoke(this, null);
            return Task.FromResult(ret);
        }

        public Task<bool> Set_GlobalSetting_DisplayWB7022CoverState(bool isDisplay)
        {
            bool ret = false;
            if (_SettingsPlugin != null)
            {
                _GlobalSettingParam.GlobalSetting_General.Webcam_WB7022_Presence_Detection_Sensor_Cover_State = isDisplay;
                ret = SaveGlobalSettingParam();
                //Telementry Collection
                var rt = false;
                var ApplicationSettings_Function = new ApplicationSettings_Function();
                writelog("[DeviceMangerPlugin] Send Telementry for DisplayWB7022CoverState...");
                rt = ApplicationSettings_Function.Send_WB7022CoverState_Telementry(_TelementryScheduler, _AllInfoMonitors, isDisplay);
                if (rt) writelog("[DeviceMangerPlugin] Send Telementry for DisplayWB7022CoverState Success ...");
                else writelog("[DeviceMangerPlugin] Send Telementry for DisplayWB7022CoverState Fail ...");
            }
            GlobalSettingChangeEvent?.Invoke(this, null);
            return Task.FromResult(ret);
        }

        public Task<bool> Set_GlobalSetting_DisplayMuteState(bool isDisplay)
        {
            bool ret = false;
            if (_SettingsPlugin != null)
            {
                _GlobalSettingParam.GlobalSetting_General.Display_MuteState = isDisplay;
                ret = SaveGlobalSettingParam();
                //Telementry Collection
                var rt = false;
                var ApplicationSettings_Function = new ApplicationSettings_Function();
                writelog("[DeviceMangerPlugin] Send Telementry for DisplayMuteState...");
                rt = ApplicationSettings_Function.Send_MuteState_Telementry(_TelementryScheduler, _AllInfoMonitors, isDisplay);
                if (rt) writelog("[DeviceMangerPlugin] Send Telementry for DisplayMuteState Success ...");
                else writelog("[DeviceMangerPlugin] Send Telementry for DisplayMuteState Fail ...");
            }
            GlobalSettingChangeEvent?.Invoke(this, null);
            return Task.FromResult(ret);
        }

        public Task<bool> Set_GlobalSetting_DisplayColorPresetAndEasyMemory(bool isDisplay)
        {
            bool ret = false;
            if (_SettingsPlugin != null)
            {
                _GlobalSettingParam.GlobalSetting_General.Display_Color_Preset_and_Easy_Memory = isDisplay;
                ret = SaveGlobalSettingParam();
                //Telementry Collection
                var rt = false;
                var ApplicationSettings_Function = new ApplicationSettings_Function();
                writelog("[DeviceMangerPlugin] Send Telementry for DisplayColorPresetAndEasyMemory...");
                rt = ApplicationSettings_Function.Send_ColorPresetAndEasyMemory_Telementry(_TelementryScheduler, _AllInfoMonitors, isDisplay);
                if (rt) writelog("[DeviceMangerPlugin] Send Telementry for DisplayColorPresetAndEasyMemory Success ...");
                else writelog("[DeviceMangerPlugin] Send Telementry for DisplayColorPresetAndEasyMemory Fail ...");
            }
            GlobalSettingChangeEvent?.Invoke(this, null);
            return Task.FromResult(ret);
        }

        public Task<bool> Set_GlobalSetting_EnableQuickAccessWidget(bool isEnable)
        {
            bool ret = false;
            if (_SettingsPlugin != null)
            {
                _GlobalSettingParam.GlobalSetting_WidgetSettings.EnableQuickAccessWidget = isEnable;
                ret = SaveGlobalSettingParam();
                //Telementry Collection
                var rt = false;
                var ApplicationSettings_Function = new ApplicationSettings_Function();
                writelog("[DeviceMangerPlugin] Send Telementry for EnableQuickAccessWidget...");
                rt = ApplicationSettings_Function.Send_EnableQuickAccessWidget_Telementry(_TelementryScheduler, _AllInfoMonitors, isEnable);
                if (rt) writelog("[DeviceMangerPlugin] Send Telementry for EnableQuickAccessWidget Success ...");
                else writelog("[DeviceMangerPlugin] Send Telementry for EnableQuickAccessWidget Fail ...");
            }
            GlobalSettingChangeEvent?.Invoke(this, null);
            return Task.FromResult(ret);
        }

        public Task<bool> Set_GlobalSetting_EnableQuickAccessWidget_Reminder(bool isEnable)
        {
            bool ret = false;
            if (_SettingsPlugin != null)
            {
                _GlobalSettingParam.GlobalSetting_WidgetSettings.EnableQuickAccessWidget_Reminder = isEnable;
                ret = SaveGlobalSettingParam();
                //Telementry Collection
                var rt = false;
                var ApplicationSettings_Function = new ApplicationSettings_Function();
                writelog("[DeviceMangerPlugin] Send Telementry for EnableQuickAccessWidget_Reminder...");
                rt = ApplicationSettings_Function.Send_EnableQuickAccessWidget_Reminder_Telementry(_TelementryScheduler, _AllInfoMonitors, isEnable);
                if (rt) writelog("[DeviceMangerPlugin] Send Telementry for EnableQuickAccessWidget_Reminder Success ...");
                else writelog("[DeviceMangerPlugin] Send Telementry for EnableQuickAccessWidget_Reminder Fail ...");
            }
            GlobalSettingChangeEvent?.Invoke(this, null);
            return Task.FromResult(ret);
        }

        public Task<bool> Set_GlobalSetting_EnableTelemetryConsent(bool isEnable)
        {
            bool ret = false;
            if (_SettingsPlugin != null)
            {
                _GlobalSettingParam.isTelemetryConsentOn = isEnable;

                if (_TelementryScheduler != null)
                {
                    writelog(nameof(Set_GlobalSetting_EnableTelemetryConsent) + " Call GetGlobalsetting_IsTelemetryConsentOn:");

                    if (!isEnable)
                        Task.Run(() => _TelementryScheduler.ReceiveTelemetryInfo("AppTelemetry,", isEnable ? "Enabled" : "Disabled", Telementry_Frequency.RealTime));

                    _TelementryScheduler.GetGlobalsetting_IsTelemetryConsentOn(isEnable);

                    if (isEnable)
                        Task.Run(() => _TelementryScheduler.ReceiveTelemetryInfo("AppTelemetry", isEnable ? "Enabled" : "Disabled", Telementry_Frequency.RealTime));
                }
                else
                    writelog(nameof(Set_GlobalSetting_EnableTelemetryConsent) + " _TelementryScheduler is null");

                ret = SaveGlobalSettingParam();
            }

            GlobalSettingChangeEvent?.Invoke(this, null);
            return Task.FromResult(ret);
        }

        private bool LoadGlobalSettingParam()
        {
            bool ret = false;
            if (_SettingsPlugin != null)
            {
                string tmpDriberVersion = string.Empty;
                if (!string.IsNullOrEmpty(_GlobalSettingParam.GlobalSetting_About.DriverVersion))
                {
                    tmpDriberVersion = _GlobalSettingParam.GlobalSetting_About.DriverVersion;
                }
                _GlobalSettingParam = _SettingsPlugin.ReadGlobalSettings().Result;
                _GlobalSettingParam.GlobalSetting_About.DriverVersion = tmpDriberVersion;
            }
            if (_PeripheralsPlugin != null)
            {
                _GlobalSettingParam.GlobalSetting_About.DriverVersion = _PeripheralsPlugin.GetDevices().Result.IsdDriverVersion;
                if (string.IsNullOrEmpty(_GlobalSettingParam.GlobalSetting_About.DriverVersion))
                {
                    _GlobalSettingParam.GlobalSetting_About.DriverVersion = "N/A";
                }
            }

            if (_GlobalSettingParam != null)
            {
                if (_TelementryScheduler != null)
                {
                    writelog(nameof(LoadGlobalSettingParam) + " Call GetGlobalsetting_IsTelemetryConsentOn:");
                    _TelementryScheduler.GetGlobalsetting_IsTelemetryConsentOn(_GlobalSettingParam.isTelemetryConsentOn);
                }
                else
                    writelog(nameof(LoadGlobalSettingParam) + " _TelementryScheduler is null");
            }
            else
                writelog(nameof(LoadGlobalSettingParam) + " _GlobalSettingParam is null");

            return ret;
        }

        private bool SaveGlobalSettingParam()
        {
            bool ret = false;
            if (_SettingsPlugin != null)
            {
                ret = _SettingsPlugin.WriteGlobalSettings(_GlobalSettingParam).Result;
            }
            return ret;
        }

        private void SettingsReady(object o, EventArgs eventArgs)
        {
            LoadGlobalSettingParam();
            //Migration
            DDMMigration();
            ReloadHotkeyConfigData();
            ToNKVM_initHotKeys();
            DeleteDdpmSwUpdaterFolder();
            GetSkipCA().Wait();
            SetDelayFWUpdateInfoPackage();
            CheckUODFWUInfoPackage();
            //hook keyboard
            //if (_HotkeyPlugin != null)
            //{
            //    _HotkeyPlugin.Hook();
            //    _HotkeyPlugin.KeyUp += Keyboard_KeyUpProc;
            //}
            CheckAutoColorPresetEnableOnStartedCondition(_AllInfoMonitors);
            CheckAutoColorManagementEnableOnStartedCondition(_AllInfoMonitors);
            LauchNightLightStatusMonitor();
        }

        #region OutReport

        public Task<bool> ExportMonitorAssetReport(List<MonitorInfo> monitorInfos, string savePath)
        {
            bool ret = false;
            if (_DisplayManagerPlugin != null)
            {
                List<MonitorAssetReport> monitorAssetReports = _DisplayManagerPlugin.GetMonitorAssetReport(monitorInfos).Result;
                if (monitorAssetReports != null && monitorAssetReports.Count > 0)
                {
                    ret = SaveMonitorAssetReport(monitorAssetReports, savePath);
                    //Telementry Collection
                    var rt = false;
                    var ApplicationSettings_Function = new ApplicationSettings_Function();
                    writelog("[DeviceMangerPlugin] Send Telementry for SaveMonitorAssetReport...");
                    rt = ApplicationSettings_Function.Send_SaveMonitorAssetReport_Telementry(_TelementryScheduler, _AllInfoMonitors, 1);
                    if (rt) writelog("[DeviceMangerPlugin] Send Telementry for SaveMonitorAssetReport Success ...");
                    else writelog("[DeviceMangerPlugin] Send Telementry for SaveMonitorAssetReport Fail ...");
                }
            }
            return Task.FromResult(ret);
        }

        public Task<bool> SaveLogFile(string saveFolderPath = "")
        {
            writelog($"{nameof(SaveLogFile)} start");
            bool ret = false;
            if (string.IsNullOrEmpty(saveFolderPath))
            {
                saveFolderPath = @$"C:\temp\Log";
            }
            if (_DisplayManagerPlugin != null && !string.IsNullOrEmpty(saveFolderPath))
            {
                // 確保資料夾存在
                if (!Directory.Exists(saveFolderPath))
                {
                    Directory.CreateDirectory(saveFolderPath);
                }
                //0913 Bruce Add Security
                string FolderInfo;
                string PathSymbolicLinInfo;
                int count = 0;
                bool folderValid = false;
                do
                {
                    FolderInfo = string.Empty;
                    PathSymbolicLinInfo = string.Empty;
                    folderValid = false;
                    folderValid = !DDPMFileSecurity.IsPathSymbolicLinked(saveFolderPath, out PathSymbolicLinInfo);
                    if (!folderValid)
                    {
                        writelog(nameof(DownloadAndInstall) + " FolderIsNotSafe:" + PathSymbolicLinInfo + " Retry:" + (count++));
                        //Do remove Symbolic Link than delete folder
                        Directory.Delete(saveFolderPath, true);
                        Directory.CreateDirectory(saveFolderPath);
                    }
                    folderValid = DDPMFileSecurity.IsFolderPathValid(saveFolderPath, out FolderInfo) && folderValid;
                    if (!folderValid)
                    {
                        writelog(nameof(DownloadAndInstall) + " FolderIsNotSafe:" + FolderInfo + " Retry:" + (count++));
                        //Do remove Symbolic Link than delete folder
                        Directory.Delete(saveFolderPath, true);
                        Directory.CreateDirectory(saveFolderPath);
                    }
                } while (!folderValid && count < 2);

                string programdataPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
                string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                if (!string.IsNullOrEmpty(appDataPath))
                {
                    string LogFolder = @$"{appDataPath}\Dell\Dell Display and Peripheral Manager\Log\DDPM.Subagent.User";
                    if (DirectoryContainsFiles(LogFolder))
                    {
                        // 取得資料夾名稱
                        string folderName = GetFolderName(LogFolder);
                        string savePath = Path.Combine(saveFolderPath, folderName);
                        // 複製指定的 log 文件到選擇的資料夾
                        CopyLogFolder(LogFolder, savePath);
                    }
                    LogFolder = @$"{appDataPath}\Dell\Dell Display and Peripheral Manager\Log\DDPM.GUI";
                    if (DirectoryContainsFiles(LogFolder))
                    {
                        // 取得資料夾名稱
                        string folderName = GetFolderName(LogFolder);
                        string savePath = Path.Combine(saveFolderPath, folderName);
                        // 複製指定的 log 文件到選擇的資料夾
                        CopyLogFolder(LogFolder, savePath);
                    }
                    LogFolder = @$"{appDataPath}\Dell\Dell Display and Peripheral Manager\Log\DDPM-Setup-MiniInstall";
                    if (DirectoryContainsFiles(LogFolder))
                    {
                        // 取得資料夾名稱
                        string folderName = GetFolderName(LogFolder);
                        string savePath = Path.Combine(saveFolderPath, folderName);
                        // 複製指定的 log 文件到選擇的資料夾
                        CopyLogFolder(LogFolder, savePath);
                    }
                }
                if (!string.IsNullOrEmpty(programdataPath))
                {
                    string LogFolder = @$"{programdataPath}\Dell\DDPM.Subagent";
                    if (DirectoryContainsFiles(LogFolder))
                    {
                        // 取得資料夾名稱
                        string folderName = GetFolderName(LogFolder);
                        string savePath = Path.Combine(saveFolderPath, folderName);
                        // 複製指定的 log 文件到選擇的資料夾
                        CopyLogFolder(LogFolder, savePath);
                    }
                    LogFolder = @$"{programdataPath}\Dell\Dell TechHub";
                    if (DirectoryContainsFiles(LogFolder))
                    {
                        // 取得資料夾名稱
                        string folderName = GetFolderName(LogFolder);
                        string savePath = Path.Combine(saveFolderPath, folderName);
                        // 複製指定的 log 文件到選擇的資料夾
                        CopyLogFolder(LogFolder, savePath);
                    }
                    LogFolder = @$"{programdataPath}\Dell\DTP\Logs";
                    if (DirectoryContainsFiles(LogFolder))
                    {
                        // 取得資料夾名稱
                        string folderName = "DTP_Log";
                        string savePath = Path.Combine(saveFolderPath, folderName);
                        // 複製指定的 log 文件到選擇的資料夾
                        CopyLogFolder(LogFolder, savePath);
                    }
                    string registryKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\DDPMW-NKVM";
                    object o = ReadRegistryData(RegistryHive.LocalMachine, registryKey, "GUID").Result;
                    if (o != null && o is string && !string.IsNullOrEmpty(o.ToString()))
                    {
                        LogFolder = @$"{programdataPath}\{o.ToString()}\DDPMW-NKVM";
                        if (DirectoryContainsFiles(LogFolder))
                        {
                            // 取得資料夾名稱
                            string folderName = GetFolderName(LogFolder);
                            string savePath = Path.Combine(saveFolderPath, folderName);
                            // 複製指定的 log 文件到選擇的資料夾
                            CopyLogFolder(LogFolder, savePath);
                        }
                    }
                    LogFolder = @$"{programdataPath}\Dell\Dell Peripheral Manager\DPMService\Log";
                    if (DirectoryContainsFiles(LogFolder))
                    {
                        // 取得資料夾名稱
                        string folderName = "DPMService_Log";
                        string savePath = Path.Combine(saveFolderPath, folderName);
                        // 複製指定的 log 文件到選擇的資料夾
                        CopyLogFolder(LogFolder, savePath);
                    }
                    LogFolder = @$"{programdataPath}\Dell\Dell Peripheral Manager\DPM\Log";
                    if (DirectoryContainsFiles(LogFolder))
                    {
                        // 取得資料夾名稱
                        string folderName = "DPM_Log";
                        string savePath = Path.Combine(saveFolderPath, folderName);
                        // 複製指定的 log 文件到選擇的資料夾
                        CopyLogFolder(LogFolder, savePath);
                    }
                    LogFolder = @$"{programdataPath}\Dell\Dell Peripheral Manager\DPeMSDK\Log";
                    if (DirectoryContainsFiles(LogFolder))
                    {
                        // 取得資料夾名稱
                        string folderName = "DPeMSDK_Log";
                        string savePath = Path.Combine(saveFolderPath, folderName);
                        // 複製指定的 log 文件到選擇的資料夾
                        CopyLogFolder(LogFolder, savePath);
                    }
                }
                string logFileName = "EventLog.evtx";
                string logFilePath = Path.Combine(saveFolderPath, logFileName);
                ExecuteWevtutilCommand(logFilePath);

                string zipFilePath = saveFolderPath + ".zip";
                // 壓縮資料夾
                CreateZipFile(saveFolderPath, zipFilePath);
                Directory.Delete(saveFolderPath, true);
            }
            writelog($"{nameof(SaveLogFile)} end");
            //Telementry Collection
            var rt = false;
            var ApplicationSettings_Function = new ApplicationSettings_Function();
            writelog("[DeviceMangerPlugin] Send Telementry for SaveDiagnosticReport...");
            rt = ApplicationSettings_Function.Send_SaveDiagnosticReport_Telementry(_TelementryScheduler, _AllInfoMonitors, 1);
            if (rt) writelog("[DeviceMangerPlugin] Send Telementry for SaveDiagnosticReport Success ...");
            else writelog("[DeviceMangerPlugin] Send Telementry for SaveDiagnosticReport Fail ...");
            return Task.FromResult(ret);
        }

        private void CreateZipFile(string folderPath, string zipFilePath)
        {
            writelog($"{nameof(CreateZipFile)} start");
            try
            {
                if (File.Exists(zipFilePath))
                {
                    File.Delete(zipFilePath);
                }
                ZipFile.CreateFromDirectory(folderPath, zipFilePath, CompressionLevel.Fastest, includeBaseDirectory: true);
            }
            catch (Exception ex)
            {
                writelog($"{nameof(CreateZipFile)} Exception occurred while creating ZIP file: {ex.Message}");
            }
            writelog($"{nameof(CreateZipFile)} end");
        }

        private bool SaveMonitorAssetReport(List<MonitorAssetReport> monitorAssetReports, string savePath)
        {
            bool ret = false;
            try
            {
                string filePath = savePath;
                // 如果檔案路徑不以 .mif 結尾，則附加 .mif 副檔名
                if (!filePath.EndsWith(".mif", StringComparison.OrdinalIgnoreCase))
                {
                    Debug.Write(filePath);
                    filePath = filePath.Substring(0, filePath.IndexOf("."));
                    Debug.Write(filePath);
                    filePath += ".mif";
                }
                string contentToSave = "";
                contentToSave += "Start Component\r\n";
                contentToSave += $"  Name = \"Machine\"\r\n";
                for (int i = 0; i < monitorAssetReports.Count; i++)
                {
                    MonitorAssetReport report = monitorAssetReports[i];
                    Type type = report.GetType();
                    PropertyInfo[] properties = type.GetProperties();
                    contentToSave += $"  Start Group\r\n";
                    contentToSave += $"    Name = \"Monitor Information\"\r\n";
                    contentToSave += $"    ID = {i + 1}\r\n";
                    contentToSave += $"    Class = \"Dell|Monitor Information|2.0\"\r\n";
                    for (int j = 0; j < properties.Length; j++)
                    {
                        PropertyInfo property = properties[j];
                        contentToSave += $"    Start Attribute\r\n";
                        string propertyName = property.Name;
                        contentToSave += $"      Name = \"{propertyName}\"\r\n";
                        contentToSave += $"      ID = {j + 1}\r\n";
                        contentToSave += $"      Type = String\r\n";
                        contentToSave += $"      Storage = Specific\r\n";
                        object value = property.GetValue(report);
                        contentToSave += $"      Value = \"{value}\"\r\n";
                        contentToSave += $"    End Attribute\r\n";
                    }
                    contentToSave += $"  End Group\r\n";
                }
                File.WriteAllText(filePath, contentToSave);
            }
            catch
            {
            }
            return ret;
        }

        private void ExecuteWevtutilCommand(string exportFilePath)
        {
            try
            {
                // 設定要查詢的日誌名稱
                string logName = "Application"; // 可選擇 "Application", "System", "Security"

                // 獲取當前時間
                DateTime now = DateTime.UtcNow;

                // 設定開始和結束時間範圍（UTC）
                DateTime endTime = now;
                DateTime startTime = endTime.AddDays(-1);

                // 生成查詢語句
                string query = $"*[System[TimeCreated[@SystemTime>='{startTime:yyyy-MM-ddTHH:mm:ss.fffZ}' and @SystemTime<='{endTime:yyyy-MM-ddTHH:mm:ss.fffZ}']]]";
                // 建立我們要執行的命令
                string command = $"epl {logName} \"{exportFilePath}\" /ow:true /q:\"{query}\"";
                // 設定 ProcessStartInfo
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = "wevtutil",
                    Arguments = command,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                // 開啟進程
                using (Process process = Process.Start(startInfo))
                {
                    // 讀取標準輸出和錯誤輸出
                    string output = process.StandardOutput.ReadToEnd();
                    string error = process.StandardError.ReadToEnd();

                    // 等待進程結束
                    process.WaitForExit();

                    // 輸出結果
                    if (process.ExitCode == 0)
                    {
                        Console.WriteLine("Events have been exported successfully.");
                    }
                    else
                    {
                        Console.WriteLine($"Error exporting events: {error}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception occurred: {ex.Message}");
            }
        }

        private string GetFolderName(string path)
        {
            try
            {
                string folderName = System.IO.Path.GetFileName(path.TrimEnd(System.IO.Path.DirectorySeparatorChar));
                return folderName;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception occurred: {ex.Message}");
                return null;
            }
        }

        private bool DirectoryContainsFiles(string folderPath)
        {
            bool ret = false;
            try
            {
                if (Directory.Exists(folderPath))
                {
                    // 檢查資料夾是否包含檔案
                    string[] files = Directory.GetFiles(folderPath);
                    // 檢查資料夾是否包含子資料夾
                    string[] directories = Directory.GetDirectories(folderPath);

                    // 如果檔案或子資料夾數量大於0，則返回 true
                    ret = files.Length > 0 || directories.Length > 0;
                }
            }
            catch
            {
            }
            return ret;
        }

        private void CopyLogFolder(string sourceFolder, string destinationFolder)
        {
            try
            {
                if (Directory.Exists(sourceFolder))
                {
                    // 複製資料夾及其內容
                    DirectoryCopy(sourceFolder, destinationFolder, true);
                    Console.WriteLine("Log folder copied successfully.");
                }
                else
                {
                    Console.WriteLine("Source folder does not exist.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception occurred while copying log folder: {ex.Message}");
            }
        }

        private void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            // 確保目標資料夾存在
            Directory.CreateDirectory(destDirName);
            // 複製檔案
            foreach (string file in Directory.GetFiles(sourceDirName))
            {
                string destFile = Path.Combine(destDirName, Path.GetFileName(file));
                File.Copy(file, destFile, true);
            }
            // 複製子資料夾
            if (copySubDirs)
            {
                foreach (string subDir in Directory.GetDirectories(sourceDirName))
                {
                    string destSubDir = Path.Combine(destDirName, Path.GetFileName(subDir));
                    DirectoryCopy(subDir, destSubDir, true);
                }
            }
        }

        #endregion

        #endregion

        #region WebCamera

        private void QAMCloseEvent(object o, EventArgs e)
        {
            if (_QAM != null)
            {
                QAM_Position = new Point(_QAM.Left, _QAM.Top);
                _QAM.Closed -= QAMCloseEvent;
                _QAM = null;
            }
        }

        private void CallQAM_UI(DeviceMangerPlugin deviceMangerPlugin)
        {
            writelog($"CallQAM_UI: Start");
            if (_QAM == null)
            {
                writelog($"CallQAM_UI: Go");
                List<DeviceInfo> deviceInfos = GetDevices_WithoutAwait().Result.deviceInfo;
                if (deviceInfos != null)
                {
                    writelog($"CallQAM_UI: deviceInfos.Count:{deviceInfos.Count}");
                    if (deviceInfos.Any(x => (x.PhysicalDeviceType.Equals(DeviceType.LogicalWebcam) || x.PhysicalDeviceType.Equals(DeviceType.PhysicalWebcam))))
                    {
                        writelog($"CallQAM_UI: have Webcam show QAM");
                        Thread thread1 = new Thread(() =>
                        {
                            _QAM = new QAMPage(deviceMangerPlugin);
                            _QAM.Closed += QAMCloseEvent;
                            if (QAM_Position != null && (QAM_Position.X != 0 && QAM_Position.Y != 0))
                            {
                                _QAM.Top = QAM_Position.Y;
                                _QAM.Left = QAM_Position.X;
                            }
                            else
                            {
                                float scaleFactorX = 1;
                                float scaleFactorY = 1;
                                using (Graphics graphics = Graphics.FromHwnd(IntPtr.Zero))
                                {
                                    float dpiX = graphics.DpiX;
                                    float dpiY = graphics.DpiY;
                                    float logicalDpi = 96.0f;
                                    scaleFactorX = dpiX / logicalDpi;
                                    scaleFactorY = dpiY / logicalDpi;
                                }
                                _QAM.Top = (Screen.PrimaryScreen.Bounds.Height / scaleFactorX / 2) - (_QAM.Height / scaleFactorX / 2);
                                _QAM.Left = 0;
                            }
                            _QAM.Dispatcher.Invoke(() => _QAM.Show());
                            Dispatcher.Run();
                        });
                        thread1.SetApartmentState(ApartmentState.STA);
                        thread1.Start();
                    }
                }
            }
            writelog($"CallQAM_UI: done");
        }

        #endregion

        #region Private Methods

        private void SystemEvents_DisplaySettingsChanged(object sender, EventArgs e)
        {
            writelog($"Receive DisplaySettingsChanged: {sender}, e:{e}, rescan monitor");
            if (displayInOut)
            {
                if (_AllInfoMonitors != null)
                    _AllInfoMonitors.Clear();
                else
                    _AllInfoMonitors = new List<MonitorInfo>();

                try
                {
                    writelog($"DisplaySettingsChanged: displayInOut is true");

                    if (isLetDisplayServiceIdle == true)
                    {
                        writelog("The idle state is true to drop display settings change event, need caller to unblock this param");
                        return;
                    }

                    writelog("[DeviceMangerPlugin] SystemEvents_DisplaySettingsChanged() Bruce count Screen Length ...");

                    //Bruce 08-09 Added judgment that if the number of screens does not change, the screen orientation adjustment function will not be performed. (For example: PxP change will trigger this event, but the screen is not actually plugged in or out)
                    bool displayDeviceNumChange = false;
                    int AllScreens = Screen.AllScreens.Length;
                    if (_lastScreenCount != AllScreens)
                    {
                        displayDeviceNumChange = true;
                        _lastScreenCount = AllScreens;
                    }

                    writelog("[DeviceMangerPlugin] SystemEvents_DisplaySettingsChanged() Bruce count Screen Length finish ...");
                    ///==============================================================

                    try
                    {
                        writelog("[DeviceMangerPlugin] SystemEvents_DisplaySettingsChanged() Ready trigger cancel ...");

                        if (_ReGetcancellationTokenSource != null)
                        {
                            writelog("[DeviceMangerPlugin] SystemEvents_DisplaySettingsChanged() _ReGetcancellationTokenSource trigger cancel ...");
                            _ReGetcancellationTokenSource.Cancel();
                        }
                    }
                    catch (TaskCanceledException)
                    {
                        _ReGetcancellationTokenSource.Dispose();
                        writelog("[DeviceMangerPlugin] SystemEvents_DisplaySettingsChanged() trigger cancel cancellation happened ...");
                    }
                    catch (OperationCanceledException)
                    {
                        _ReGetcancellationTokenSource.Dispose();
                        writelog("[DeviceMangerPlugin] SystemEvents_DisplaySettingsChanged() trigger cancel cancellation happened ...");
                    }
                    catch (Exception ex)
                    {
                        _ReGetcancellationTokenSource.Dispose();
                        // Failed to complete due to e exception
                        writelog($"[DeviceMangerPlugin] SystemEvents_DisplaySettingsChanged() --Task.Run ...there is an exceptionI-- ({ex.Message})");

                        //Done: let's be nice and don't swallow the exception
                        //throw new InvalidOperationException("some exception happened but not about InitializeMonitorsList cancellation");
                    }
                    finally
                    {
                        using (_ReGetcancellationTokenSource = new CancellationTokenSource())
                        {
                            try
                            {
                                var _cancellationTokenSource_tmp = CancellationTokenSource.CreateLinkedTokenSource(_ReGetcancellationTokenSource.Token);

                                var token = _cancellationTokenSource_tmp.Token;

                                writelog("[DeviceMangerPlugin] SystemEvents_DisplaySettingsChanged() into Re-GetDevices ...");

                                //TODO: May be you'll want to add .ConfigureAwait(false);
                                Task.Run(() =>
                                {
                                    try
                                    {
                                        writelog("[DeviceMangerPlugin] XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX");

                                        //Call VCP to catch updated monitor info
                                        if (_AllInfoMonitors != null)
                                            _AllInfoMonitors.Clear();
                                        else
                                            _AllInfoMonitors = new List<MonitorInfo>();
                                        _AllInfoMonitors.AddRange((_DisplayManagerPlugin.Re_GetMonitors(token).Result).ToList());

                                        List<MonitorInfo> new_mo = new List<MonitorInfo>();
                                        if (_AllInfoMonitors.Count > 0)
                                            new_mo.AddRange(_AllInfoMonitors);

                                        writelog($"[DeviceManager] SystemEvents_DisplaySettingsChanged() Got event, monitor count {_AllInfoMonitors.Count}");

                                        if (_AllInfoMonitors.Count > 0)
                                            OnDeviceChanged(_AllInfoMonitors[0], null, DeviceChangedType.NotifyOnly, token, "DisplayChanged");//DeviceChangedType.Display_PlugIn);
                                        else
                                            OnDeviceChanged(null, null, DeviceChangedType.NotifyOnly, token, "DisplayChanged");

                                        writelog("[DeviceMangerPlugin] SystemEvents_DisplaySettingsChanged() OnDeviceChanged finish ...");

                                        //Robert_Lin, 2024-9-9 Signal a DisplaySettingsChanged event through Agent
                                        //Anyone who would like to receive this event, you can add below code: (refer to EAPlugin.cs)
                                        // _agent.RegisterForEvent(AgentEventNames.DisplaySettingsChanged, DisplaySettingsChangedHandler);
                                        //
                                        // private void DisplaySettingsChangedHandler(object sender, EventManagerArgs e)
                                        // {
                                        //    your handler code
                                        // }
                                        //

                                        if (_agent != null)
                                            _agent.RaiseEvent(AgentEventNames.DisplaySettingsChanged, this, new EventManagerArgs());

                                        writelog("[DeviceMangerPlugin] SystemEvents_DisplaySettingsChanged() _agent.RaiseEvent finish ...");

                                        Task.Run(() =>
                                        {
                                            //Telementry Collection
                                            var rt = false;
                                            var DeviceTypeConnected_Function = new DeviceTypeConnected_Function();
                                            writelog("[DeviceMangerPlugin] [Telementry] Send Telementry for DeviceTypeConnected_Function...");
                                            rt = DeviceTypeConnected_Function.DeviceTypeConnected_Telementry(_TelementryScheduler, _AllInfoMonitors);
                                            if (rt) writelog("[DeviceMangerPlugin] [Telementry] Send Telementry for DeviceTypeConnected_Function Success ...");
                                            else writelog("[DeviceMangerPlugin] [Telementry] Send Telementry for DeviceTypeConnected_Function Fail ...");
                                        }).ConfigureAwait(false);

                                        if (displayDeviceNumChange && _AllInfoMonitors.Count > 0)
                                        {
                                            //displayInOut = false;
                                            _DisplayManagerPlugin.SetDisplayOrientation(_AllInfoMonitors).Wait();
                                            //displayInOut = true;
                                        }

                                        writelog("[DeviceMangerPlugin] SystemEvents_DisplaySettingsChanged() SetDisplayOrientation finish ...");

                                        _DisplayManagerPlugin.UpdateExistAlsConfig(new_mo).Wait();

                                        writelog("[DeviceMangerPlugin] SystemEvents_DisplaySettingsChanged() UpdateExistAlsConfig finish ...");

                                        writelog("[DeviceMangerPlugin] SystemEvents_DisplaySettingsChanged() Re-GetDevices finish ...");

                                        if (_AllInfoMonitors != null && _AllInfoMonitors.Count > 0)
                                            Task.Run(() => _disDevHelper?.CheckAndTriggerToastWhileMonitorPlugged(_millisecond, new_mo));
                                    }
                                    catch (Exception ex)
                                    {
                                        writelog("SystemEvents_DisplaySettingsChanged() Task.Run() happened Exception ... " + ex.Message);
                                    }
                                }, token).ConfigureAwait(false);
                            }
                            catch (TaskCanceledException)
                            {
                                writelog("[DeviceMangerPlugin] SystemEvents_DisplaySettingsChanged() Re-GetDevices cancellation happened ...");
                            }
                            catch (OperationCanceledException)
                            {
                                writelog("[DeviceMangerPlugin] SystemEvents_DisplaySettingsChanged() Re-GetDevices cancellation happened ...");
                            }
                            catch (Exception ex)
                            {
                                // Failed to complete due to e exception
                                writelog($"[DeviceMangerPlugin] SystemEvents_DisplaySettingsChanged() --Task.Run ...there is an exceptionII-- ({ex.Message})");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    writelog("[DeviceMangerPlugin] Initialize Monitors List Exception : " + ex.Message);
                }
            }
            else//Add by Bruce
            {
                writelog($"DisplaySettingsChanged: {sender}, e:{e}, By pass.");
                displayInOut = true;
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
            if (e == null || e.monitors == null)
            {
                writelog("DeviceMangerPlugin brocast OnDisplaychanged ...null object, return directly");
                return;
            }
            writelog($"DeviceMangerPlugin brocast OnDisplaychanged ...(monitor count {e.monitors.Count})");

            EventHandler<DisplaychangedEventArgs> handler = Displaychanged;
            //if (handler != null)
            //    handler.Invoke(this, e);
            if (_DisplayManagerPlugin != null)
            {
                //displayInOut = false;
                _DisplayManagerPlugin.SetDisplayOrientation(e.monitors).Wait();
                //displayInOut = true;
            }
            DeviceChangedEventArgs arg = new DeviceChangedEventArgs();
            arg.changedProperty = "DisplayChanged";
            arg.type = DeviceChangedType.NotifyOnly;
            EventHandler<DeviceChangedEventArgs> devHandler = DeviceChanged;
            if (devHandler != null)
                devHandler.Invoke(this, arg);

            if (_NKVMPlugin != null)
            {
                var Cancellation = new CancellationTokenSource();
                var CancellationToken = Cancellation.Token;
                _NKVMPlugin.UpdateMonitorInfo(_AllInfoMonitors, CancellationToken);
                SupportedNKVMMonitors();
            }

            if(_AllInfoMonitors != null && _AllInfoMonitors.Count > 0)
                Task.Run(() => _disDevHelper?.CheckAndTriggerToastWhileMonitorPlugged(_millisecond, e.monitors));
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

        private void OnProgressUpdateEvent(UpdateProgressInfo fWUpdateInfo)
        {
            //ProgressUpdate_Notify?.Invoke(this, fWUpdateInfo);
            EventHandler<UpdateProgressInfo> handler = ProgressUpdate_Notify;
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
            DDPMSettings data = ReloadAppConfigData().Result;
            if (data != null)
            {
                if (!data.LockSettings.Lock_Settings_Updates)
                {
                    CheckUpdate();
                }
            }
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
            DDPMSettings data = ReloadAppConfigData().Result;
            if (data != null)
            {
                if (!data.LockSettings.Lock_Settings_Updates)
                {
                    SW_CheckSWUpdate();
                }
            }
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

        protected virtual void OnDeviceChanged(MonitorInfo mo, DeviceInfo di, DeviceChangedType type, CancellationToken token, string changedProperty = "")
        {
            var Cancellation = CancellationTokenSource.CreateLinkedTokenSource(token);
            var CancellationToken = Cancellation.Token;

            DeviceChangedEventArgs _EventArgs = new DeviceChangedEventArgs();

            if (type == DeviceChangedType.NotifyOnly)
            {
                //writelog("[OnDeviceChanged] Notify event to registers");
                //Task.Run(() => updateALSwithAllMonitors());
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
                Task.Run(() => handler.Invoke(this, _EventArgs), CancellationToken).ConfigureAwait(false);

            if (changedProperty.ToLower().Contains("add"))
            {
                //else
                //{
                //    _NKVMPlugin.MonitorPlug();
                //    SupportedNKVMMonitors();
                //}

                //0909 Bruce move to add and remove
                var thread = new Thread(() =>
                {
                    //CheckUpdate();
                    CheckUODFWUInfoPackage(true);
                    CheckDocks();
                });
                thread.Start();
            }
            else if (changedProperty.ToLower().Contains("remove"))
            {
                //0909 Bruce move to add and remove
                var thread = new Thread(() =>
                {
                    CheckDocks();
                });
                thread.Start();
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
                    _NKVMPlugin.UpdateMonitorInfo(_AllInfoMonitors, CancellationToken);
                    SupportedNKVMMonitors();
                }
            }
        }

        //0613 Bruce 用於看是否連接超過2個dock
        private void CheckDocks()
        {
            if (_PeripheralsPlugin == null)
            {
                return;
            }
            List<DeviceInfo> _peripheralslist = _PeripheralsPlugin.GetDevices(true).Result.deviceInfo;

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
                //PopupBaseManage popupBaseManage = new PopupBaseManage();
                //popupBaseManage.FWU_Show("Warning", "Multiple docks are detected. Keep only one dock connected to prevent damage to your dock(s).", "", "", null, true, 5);
                PopupContentPackage popupContentPackage = new PopupContentPackage()
                {
                    Title = "Warning",
                    Info = "Multiple docks are detected. Keep only one dock connected to prevent damage to your docks.",
                    IsInfo = true,
                    IsOnlyUpdate = false,
                    StayOpen = true,
                    Timeout = 5,
                };
                CallPopup(this, popupContentPackage);
                // 顯示Toast通知
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

            Task.Run(() =>
            {
                //Telementry Collection
                var rt = false;
                var DeviceTypeConnected_Function = new DeviceTypeConnected_Function();
                writelog("[DeviceMangerPlugin] [Telementry] Send Telementry for DeviceTypeConnected_Function...");
                rt = DeviceTypeConnected_Function.DeviceTypeConnected_Telementry(_TelementryScheduler, e.monitors);
                if (rt) writelog("[DeviceMangerPlugin] [Telementry] Send Telementry for DeviceTypeConnected_Function Success ...");
                else writelog("[DeviceMangerPlugin] [Telementry] Send Telementry for DeviceTypeConnected_Function Fail ...");
            }).ConfigureAwait(false);
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
            OnDeviceChanged(null, e.device_peripherals, e.type, CancellationToken.None, e.changedProperty);
        }

        private void show_peripheralsUpdateNotify(object sender, bool e)
        {
            writelog("Receive UpdateNotify Event from PeripheralsPlugin");
            writelog("Send out UpdateNotify Event from DeviceMangerPlugin");

            OnPeripheralsUpdateNotify(e);
        }

        private void show_fwProgressUpdateEvent(object sender, UpdateProgressInfo e)
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
            if (string.IsNullOrEmpty(text))
                text = "";

            text = "[DeviceManager] " + text;
            Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss.fff") + " " + text);

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

        private void InitializeSchedulerManagerPlugin()
        {
            if (_ScheduleManagerPlugin != null)
                return;

            _ScheduleManagerPlugin = _agent.PluginManager.FindPluginByType<ISchedulerManager>(PluginResolution.Dynamic);

            if (_ScheduleManagerPlugin is IFrameworkPluginConditionNotification pluginCondition)
            {
                pluginCondition.PluginConditionChangeHandler += OnScheduleManagerPluginConditionChangeHandler;
                GetCurrentScheduleManagerCondition();
            }
        }

        private void InitializeDTPProxyPlugin()
        {
            if (_DTPProxyPlugin != null)
                return;

            _DTPProxyPlugin = _agent.PluginManager.FindPluginByType<IDTPProxyPlugin>(PluginResolution.Dynamic);

            if (_DTPProxyPlugin is IFrameworkPluginConditionNotification pluginCondition)
            {
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

        private void InitializeEzMemoryPlugin()
        {
            if (_IEzMemoryPlugin != null)
                return;

            _IEzMemoryPlugin = _agent.PluginManager.FindPluginByType<IEzMemoryPlugin>(PluginResolution.Dynamic);

            if (_IEzMemoryPlugin is IFrameworkPluginConditionNotification pluginCondition)
            {
                pluginCondition.PluginConditionChangeHandler += OnEzMemoryPluginConditionChangeHandler;
                GetCurrentEzMemoryPluginCondition();
            }
        }

        private void InitializeTelementrySchedulerPlugin()
        {
            if (_TelementryScheduler != null)
                return;

            _TelementryScheduler = _agent.PluginManager.FindPluginByType<ITelementryScheduler>(PluginResolution.Dynamic);

            if (_TelementryScheduler is IFrameworkPluginConditionNotification pluginCondition)
            {
                pluginCondition.PluginConditionChangeHandler += OnTelementrySchedulerConditionChangeHandler;
                GetCurrentTelementrySchedulerCondition();
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
                        _ScheduleManagerPlugin.ServiceRequest += _ScheduleManagerPlugin_ServiceRequest;
                    }
                    else if (pluginCondition is PluginStartedCondition)
                    {
                        writelog($"{nameof(GetCurrentScheduleManagerCondition)} - Schedule Manager Plugin is in a started condition");
                        _ScheduleManagerPlugin.ServiceRequest += _ScheduleManagerPlugin_ServiceRequest;
                    }
                }
            });
        }

        private void _ScheduleManagerPlugin_ServiceRequest(object sender, ReadWriteRequest e)
        {
            Task.Run(() =>
            {
                var monitor = e.monitor;
                var type = e.service;
                if (type == ReadWriteRequest_Type.Read)
                {
                    var info = ReadScheduleMonitorSettings(monitor).Result;

                    if (_ScheduleManagerPlugin != null)
                        _ScheduleManagerPlugin.ReceiveScheduleInfo(info);
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
                        //Robert_Lin, 2024-9-13 Remove unused interfaces
                        //_DisplayManagerPlugin.EAEditCompleted += _DisplayManagerPlugin_EAEditCompleted;
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
                        //Robert_Lin, 2024-10-8, for EasyArrange when EA Settings changed
                        _DisplayManagerPlugin.EASettingsChanged += _DisplayManagerPlugin_EASettingsChanged;

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
                        //Robert_Lin, 2024-9-13 Remove unused interfaces
                        //_DisplayManagerPlugin.EAEditCompleted += _DisplayManagerPlugin_EAEditCompleted;
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
                        //Robert_Lin, 2024-10-8, for EasyArrange when EA Settings changed
                        _DisplayManagerPlugin.EASettingsChanged += _DisplayManagerPlugin_EASettingsChanged;

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
                    else if (pluginCondition is PluginStartedCondition || pluginCondition is PluginRunningCondition)
                    {
                        writelog($"{nameof(GetCurrentColorPresetCondition)} - ColorPreset Plugin is in a started/running condition");
                        //_ColorPresetPluginCondition = pluginCondition;
                        _ColorPresetPlugin.VCPchanged += show_colorpreset;

                        _ColorPresetPlugin.Coloreset_manual_ChangeEvent += OnColoresetManualChangeHandler;

                        _ColorPresetPlugin.NightLightStatus_ChangeEvent += OnNightLightStatusChangeHandler;

                        if (_SettingsPlugin != null)
                        {
                            _AllAppData = _ColorPresetPlugin.GetInstalledAppsList().Result;//_ColorPresetPlugin.FindAppsbyShell().Result;
                        }

                        //CheckAutoColorPresetEnableOnStartedCondition(_AllInfoMonitors);
                        //CheckAutoColorManagementEnableOnStartedCondition(_AllInfoMonitors);
                    }
                    /*
                    else if(pluginCondition is PluginStartedCondition)
                    {
                        writelog($"{nameof(GetCurrentColorPresetCondition)} - ColorPreset Plugin is in a started condition");
                        //_ColorPresetPluginCondition = pluginCondition;
                        _ColorPresetPlugin.VCPchanged += show_colorpreset;

                        if (_SettingsPlugin != null)
                        {
                            _AllAppData = _ColorPresetPlugin.GetInstalledAppsList().Result;//_ColorPresetPlugin.FindAppsbyShell().Result;
                        }
                    }*/
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
                        //LoadGlobalSettingParam();
                    }
                    else if (pluginCondition is PluginStartedCondition)
                    {
                        writelog($"{nameof(GetCurrentPeripheralsPluginCondition)} - Peripherals Plugin is in a started condition");
                        //LoadGlobalSettingParam(); //here is too early, please refer to function "SettingsReady"
                    }
                }
            });
        }

        private void CheckAutoColorPresetEnableOnStartedCondition(List<MonitorInfo> _AllInfoMonitors)
        {
            if (_SettingsPlugin == null)
            {
                writelog("CheckAutoColorPresetEnableOnStartedCondition, _SettingsPlugin == null");
                return;
            }

            if (_ColorPresetPlugin == null)
            {
                writelog("CheckAutoColorPresetEnableOnStartedCondition, _ColorPresetPlugin == null");
                return;
            }

            writelog("CheckAutoColorPresetEnableOnStartedCondition, Enter");

            List<ColorPresetSettings> appconfigs = ReadColorPresetSettings().Result;

            foreach (var config in appconfigs)
            {
                if (config.AppInfo == null || config.AppInfo.Count <= 0)
                {
                    //Trace.WriteLine("config.AppInfo.Count = " +  config.AppInfo.Count.ToString());
                    writelog("CheckAutoColorPresetEnableOnStartedCondition, appconfigs.Count = " + appconfigs.Count.ToString());
                    continue;
                }

                foreach (var _InfoMonitors in _AllInfoMonitors)
                {
                    //Check if actived monitor has its color preset section in config file
                    if (_InfoMonitors.edid.ModelName.Trim().IndexOf(config.ModelName.Trim()) >= 0 &&
                         _InfoMonitors.edid.SerialNumber.Trim() == config.SerialNumber.Trim())
                    {
                        if (config.RunType == (int)ColorPresetRunType.Auto)
                        {
                            writelog("CheckAutoColorPresetEnableOnStartedCondition, config.RunType is ColorPresetRunType.Auto");

                            _ = Task.Run(async () =>
                            {
                                await AutoSetColorPresetForMonitorConfig(_InfoMonitors, "ON");
                            });

                            writelog("CheckAutoColorPresetEnableOnStartedCondition, AutoSetColorPresetForMonitorConfig(_InfoMonitors, \"ON\")");

                            break;
                        }
                    }
                }

                writelog("CheckAutoColorPresetEnableOnStartedCondition, exit(break) for foreach (var _InfoMonitors in _AllInfoMonitors)");
            }

            writelog("CheckAutoColorPresetEnableOnStartedCondition, Exit");
        }

        private void CheckAutoColorManagementEnableOnStartedCondition(List<MonitorInfo> _AllInfoMonitors)
        {
            if (_SettingsPlugin == null)
            {
                writelog("CheckAutoColorManagementEnableOnStartedCondition, _SettingsPlugin == null");
                return;
            }

            if (_ColorPresetPlugin == null)
            {
                writelog("CheckAutoColorManagementEnableOnStartedCondition, _ColorPresetPlugin == null");
                return;
            }

            writelog("CheckAutoColorManagementEnableOnStartedCondition, Enter");

            List<ColorPresetSettings> appconfigs = ReadColorPresetSettings().Result;

            foreach (var config in appconfigs)
            {
                if (config.AppInfo == null || config.AppInfo.Count <= 0)
                {
                    //Trace.WriteLine("config.AppInfo.Count = " +  config.AppInfo.Count.ToString());
                    writelog("CheckAutoColorManagementEnableOnStartedCondition, appconfigs.Count = " + appconfigs.Count.ToString());
                    continue;
                }

                foreach (var _InfoMonitors in _AllInfoMonitors)
                {
                    //Check if actived monitor has its color preset section in config file
                    if (_InfoMonitors.edid.ModelName.Trim().IndexOf(config.ModelName.Trim()) >= 0 &&
                         _InfoMonitors.edid.SerialNumber.Trim() == config.SerialNumber.Trim())
                    {
                        if (config.ColorManagement_Status == (int)ColorManagementStatus.Off)
                        {
                            writelog("CheckAutoColorManagementEnableOnStartedCondition, config.ColorManagement_Status is ColorManagementStatus.Off");

                            _ = Task.Run(async () =>
                            {
                                await AutoColorManagementForMonitorConfig(_InfoMonitors, "OFF").ConfigureAwait(false);
                            });

                            writelog("CheckAutoColorManagementEnableOnStartedCondition, AutoColorManagementForMonitorConfig(_InfoMonitors, \"OFF\")");

                            break;
                        }
                        else if ((config.ColorManagement_Status == (int)ColorManagementStatus.On) && (config.ColorManagement_RunType == (int)ColorManagementRunType.Bymonitor))
                        {
                            writelog("CheckAutoColorManagementEnableOnStartedCondition, config.ColorManagement_Status is ColorManagementStatus.On  config.ColorManagement_RunType is ColorManagementRunType.Bymonitor");

                            _ = Task.Run(async () =>
                            {
                                await AutoColorManagementForMonitorConfig(_InfoMonitors, "BYMONITOR").ConfigureAwait(false);
                            });

                            writelog("CheckAutoColorManagementEnableOnStartedCondition, AutoColorManagementForMonitorConfig(_InfoMonitors, \"BYMONITOR\")");

                            break;
                        }
                        else if ((config.ColorManagement_Status == (int)ColorManagementStatus.On) && (config.ColorManagement_RunType == (int)ColorManagementRunType.Byhost))
                        {
                            writelog("CheckAutoColorManagementEnableOnStartedCondition, config.ColorManagement_Status is ColorManagementStatus.On  config.ColorManagement_RunType is ColorManagementRunType.Byhost");

                            _ = Task.Run(async () =>
                            {
                                await AutoColorManagementForMonitorConfig(_InfoMonitors, "BYHOST").ConfigureAwait(false);
                            });

                            writelog("CheckAutoColorManagementEnableOnStartedCondition, AutoColorManagementForMonitorConfig(_InfoMonitors, \"BYHOST\")");

                            break;
                        }
                    }
                }

                writelog("CheckAutoColorManagementEnableOnStartedCondition, exit(break) for foreach (var _InfoMonitors in _AllInfoMonitors)");
            }

            writelog("CheckAutoColorManagementEnableOnStartedCondition, Exit");
        }

        private void LauchNightLightStatusMonitor()
        {
            if (_SettingsPlugin == null)
            {
                writelog("LauchNightLightStatusMonitor, _SettingsPlugin == null");
                return;
            }

            if (_ColorPresetPlugin == null)
            {
                writelog("LauchNightLightStatusMonitor, _ColorPresetPlugin == null");
                return;
            }

            writelog("LauchNightLightStatusMonitor, Enter");

            writelog("LauchNightLightStatusMonitor, CheckNightLightStatus()");
            CheckNightLightStatus();

            writelog("LauchNightLightStatusMonitor, Exit");
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
                        _SettingsPlugin.SettingReadyEvent += SettingsReady;
                        DDPMSettings config = _SettingsPlugin.ReloadAppConfigData().Result;
                        if (config != null)
                        {
                            //0612 Bruce 自動旋轉畫面功能，因需使用display跟settings兩個Plugin，其中一個可能還沒被叫起來，故兩邊都新增取得狀態方法。
                            //GetLockRotateStatus();
                            //0812 check required plugins before init
                            DoThingsAfterDisplayRelatedPluginsReady(nameof(GetCurrentDisplayManagerCondition));

                            //SetDelayFWUpdateInfoPackage();
                            //CheckUODFWUInfoPackage();
                            //load hotkeysetting
                            //ReloadHotkeyConfigData();
                            ToNKVM_SupportedMonitorList();
                            //ToNKVM_initHotKeys();
                        }

                        //CheckAutoColorPresetEnableOnStartedCondition(_AllInfoMonitors);
                        //CheckAutoColorManagementEnableOnStartedCondition(_AllInfoMonitors);
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
                lock (_FwUpdateLock)
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
                        _FWUpdatePlugin.CallOSD += CallOSD;
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
                        _FWUpdatePlugin.CallOSD += CallOSD;
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
                        _NKVMPlugin.NKVMCLIEvent += NKVMCLIEvent;
                        _NKVMPlugin.NKVMSetHotkey += NKVMSetHotkey;
                        ToNKVM_SupportedMonitorList();
                        //ToNKVM_initHotKeys();
                    }
                    else if (pluginCondition is PluginStartedCondition)
                    {
                        writelog($"{nameof(GetCurrentNKVMPluginCondition)} - NKVM Plugin is in a started condition");
                        //_NKVMPluginCondition = pluginCondition;
                        _NKVMPlugin.NKVMCLIEvent += NKVMCLIEvent;
                        _NKVMPlugin.NKVMSetHotkey += NKVMSetHotkey;
                        ToNKVM_SupportedMonitorList();
                        //ToNKVM_initHotKeys();
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

        private void GetCurrentEzMemoryPluginCondition()
        {
            _ = Task.Run(async () =>
            {
                var pluginCondition = await (_IEzMemoryPlugin as IFrameworkPluginConditionNotification)?.CurrentConditionAsync();
                //PluginCondition _DisplayManagerPluginCondition;
                lock (_PluginConditionLock_EzMemory)
                {
                    if (pluginCondition is PluginErrorCondition)
                    {
                        writelog($"{nameof(GetCurrentEzMemoryPluginCondition)} - EzMemory Plugin is in an error condition");
                    }
                    else if (pluginCondition is PluginRunningCondition)
                    {
                        writelog($"{nameof(GetCurrentEzMemoryPluginCondition)} - EzMemory Plugin is in a running condition");
                    }
                    else if (pluginCondition is PluginStartedCondition)
                    {
                        writelog($"{nameof(GetCurrentEzMemoryPluginCondition)} - EzMemory Plugin is in a started condition");
                    }
                }
            });
        }

        private void GetCurrentTelementrySchedulerCondition()
        {
            _ = Task.Run(async () =>
            {
                var pluginCondition = await (_TelementryScheduler as IFrameworkPluginConditionNotification)?.CurrentConditionAsync();
                lock (_PluginConditionLock_TelementryScheduler)
                {
                    if (pluginCondition is PluginErrorCondition)
                    {
                        writelog($"{nameof(GetCurrentTelementrySchedulerCondition)} - Telementry Scheduler is in an error condition");
                    }
                    else if (pluginCondition is PluginRunningCondition)
                    {
                        writelog($"{nameof(GetCurrentTelementrySchedulerCondition)} - Telementry Scheduler is in a running condition");

                        if (_GlobalSettingParam != null)
                        {
                            writelog(nameof(GetCurrentTelementrySchedulerCondition) + " Call GetGlobalsetting_IsTelemetryConsentOn:");
                            _TelementryScheduler.GetGlobalsetting_IsTelemetryConsentOn(_GlobalSettingParam.isTelemetryConsentOn);
                            TelemetryDdpmSwUpdater();
                        }
                        else
                            writelog(nameof(GetCurrentTelementrySchedulerCondition) + " _GlobalSettingParam is null");
                    }
                    else if (pluginCondition is PluginStartedCondition)
                    {
                        writelog($"{nameof(GetCurrentTelementrySchedulerCondition)} - Telementry Scheduler is in a started condition");

                        if (_GlobalSettingParam != null)
                        {
                            writelog(nameof(GetCurrentTelementrySchedulerCondition) + " Call GetGlobalsetting_IsTelemetryConsentOn:");
                            _TelementryScheduler.GetGlobalsetting_IsTelemetryConsentOn(_GlobalSettingParam.isTelemetryConsentOn);
                            TelemetryDdpmSwUpdater();
                        }
                        else
                            writelog(nameof(GetCurrentTelementrySchedulerCondition) + " _GlobalSettingParam is null");
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
                    if (_NKVMPlugin != null && _hotkeySettings != null)
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
            HotkeySettings find = settings.Find(x => x.SerialNumber.Equals("DDPM"));// hotkeySettings.SerialNumber));
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

        private List<HotkeyData> GetInputSourceHotKeyData(MonitorInfo mo)
        {
            string model = mo.modelName;
            string serviceTag = mo.edid.ServiceTag;

            List<DDPMMonitorSettings> settings = _SettingsPlugin.ReloadMonitorSettings(model).Result;
            if (settings == null)
            {
                writelog($"@ GetInputSourceHotKeyData: ReloadMonitorSettings(model={model}) return null.");
                return null;
            }

            //Find the previous saved device settings
            DDPMMonitorSettings? monitorSettings = settings.FirstOrDefault(x => x.ServiceTag.Equals(mo.edid.ServiceTag));
            //If not found => return error, GetAllMonitor() will init and create an initial settings instance for us
            if (monitorSettings == null)
            {
                writelog($"@ GetInputSourceHotKeyData: Reloaded settings not contains (model={model}, serviceTage={serviceTag}).");
                return null;
            }

            return monitorSettings.hotkeyData;
        }

        private bool GetInputSourceHotKeyDataAndSaveNewBack(MonitorInfo mo, HotkeyType hotkeyType, List<InputSourceObj> hotkeyDataInputSource)
        {
            string model = mo.modelName;
            string serviceTag = mo.edid.ServiceTag;

            if (hotkeyDataInputSource == null || hotkeyDataInputSource.Count == 0)
            {
                writelog($"@ GetInputSourceHotKeyDataAndSaveBack: ReloadMonitorSettings(model={model}) return null.");
                //means clear the setting
                hotkeyDataInputSource = new List<InputSourceObj>();
            }

            List<DDPMMonitorSettings> settings = _SettingsPlugin.ReloadMonitorSettings(model).Result;
            if (settings == null)
            {
                writelog($"@ GetInputSourceHotKeyDataAndSaveBack: ReloadMonitorSettings(model={model}) return null.");
                return false;
            }
            Debug.WriteLine($"GetInputSourceHotKeyDataAndSaveNewBack:{mo.edid.ServiceTag}");
            //Find the previous saved device settings
            DDPMMonitorSettings? monitorSettings = settings.FirstOrDefault(x => x.ServiceTag.Equals(mo.edid.ServiceTag));
            //If not found => return error, GetAllMonitor() will init and create an initial settings instance for us
            if (monitorSettings == null)
            {
                writelog($"@ GetInputSourceHotKeyDataAndSaveBack: Reloaded settings not contains (model={model}, serviceTage={serviceTag}).");
                return false;
            }

            //monitorSettings.hotkeyData = hotkeyData;
            //update hotkey data
            HotkeyData hotkeyData = new HotkeyData { hotkeyType = hotkeyType, inputSource = hotkeyDataInputSource };
            List<HotkeyData> removeHotkeyDatas = monitorSettings.hotkeyData.Where(x => x.hotkeyType.Equals(hotkeyType) || x.hotkeyType.Equals(HotkeyType.None)).ToList();
            foreach (var item in removeHotkeyDatas)
            {
                monitorSettings.hotkeyData.Remove(item);
            }
            monitorSettings.hotkeyData.Add(hotkeyData);

            if (!_SettingsPlugin.WriteMonitorSettings(mo.modelName, settings).Result)
            {
                writelog($"@ GetInputSourceHotKeyDataAndSaveBack(model={model}, serviceTage={serviceTag}) failed.");
                return false;
            }
            writelog($"@ GetInputSourceHotKeyDataAndSaveBack(model={model}, serviceTage={serviceTag}) OK.");
            return true;
        }

        //public Task<bool> SaveHotkeySetting(EDID monitorEdid, HotkeyInfo info)
        public Task<bool> SaveHotkeySetting(MonitorInfo mo, HotkeyInfo info)
        {
            EDID monitorEdid = null;
            if (mo != null)
                monitorEdid = mo.edid;
            var hotkeys = info.Hotkey;
            List<HotkeySettings> saveList = new List<HotkeySettings>();
            List<InputSourceObj> inputSourceList = new List<InputSourceObj>();
            List<HotkeyInfo> hotkeyInfoList = new List<HotkeyInfo>();
            //HotkeySettings curHotkey = ReadCurrentHotkey(mo).Result;// monitorEdid).Result;
            var temp = ReadCurrentHotkey(mo).Result;
            HotkeySettings curHotkey = temp.Item1;
            List<HotkeySettings> allSettings = ReadHotkeySettings().Result;
            //if (curHotkey.DeviceInfo == null)
            if (curHotkey.ModelName == null || curHotkey.ServiceTag == null || curHotkey.SerialNumber == null)
            {
                //new monitor
                hotkeyInfoList.Add(info);
                HotkeySettings hotkeySettings = new HotkeySettings();
                hotkeySettings.HotkeyInfo = hotkeyInfoList;
                hotkeySettings.SerialNumber = "DDPM";// monitorEdid.SerialNumber; //Dean 1001 temporally make all update to single fake monitor
                hotkeySettings.ServiceTag = "DDPM";// monitorEdid.ServiceTag;     //Reason: change per monitor as per user
                hotkeySettings.ModelName = "DDPM";// monitorEdid.ModelName;
                saveList.Add(hotkeySettings);
                //set default inputsource value of other monitor due to the hotkey is global
                List<MonitorInfo> defaultMoList = _AllInfoMonitors.Where(x => !x.edid.ServiceTag.Equals(mo.edid.ServiceTag)).ToList();
                foreach (var m in defaultMoList)
                {
                    List<InputSourceObj> defaultList = new List<InputSourceObj>();
                    Debug.WriteLine($"set default value on mo: {m.edid.ServiceTag}");
                    Debug.WriteLine($"set default value on mo: {m.inputSource}");
                    switch (info.Job)
                    {
                        case HotkeyType.FavoriteInputSource:
                            defaultList.Add(new InputSourceObj(m.inputSource));
                            break;

                        case HotkeyType.SwitchInputSource:
                            defaultList.Add(new InputSourceObj(m.inputSource));
                            Dictionary<string, InputInfo> result = GetInputSourcelist(m).Result;
                            if (result != null)
                            {
                                string sencondInput = result.Keys.FirstOrDefault(x => !x.Equals(m.inputSource));
                                defaultList.Add(new InputSourceObj(sencondInput));
                            }
                            break;
                    }
                    GetInputSourceHotKeyDataAndSaveNewBack(m, info.Job, defaultList);
                    foreach (var s in defaultList)
                    {
                        Debug.WriteLine($"defaultList {s?.Name}");
                        Debug.WriteLine($"defaultList {s?.Code}");
                    }
                }
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
                        monitorSnList.Add("DDPM");// hotkeySetting.SerialNumber);
                    }
                }
                int allCount = monitorSnList.Count;
                int distCount = monitorSnList.Distinct().Count();
                if (distCount != 0 && (allCount == distCount))
                {
                    //overwite
                    string overWiteMonitorSn = monitorSnList.SingleOrDefault(x => !x.Equals("DDPM"));// monitorEdid.SerialNumber));
                    if (overWiteMonitorSn != null)
                    {
                        HotkeySettings overWitrHotkeysettings = allSettings.SingleOrDefault(x => x.SerialNumber.Equals("DDPM"));// overWiteMonitorSn));
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
                        //hotkeySettings.DeviceInfo = monitorEdid;
                        hotkeySettings.SerialNumber = "DDPM";// monitorEdid.SerialNumber;
                        hotkeySettings.ModelName = "DDPM";// monitorEdid.ModelName;
                        hotkeySettings.ServiceTag = "DDPM";// monitorEdid.ServiceTag;
                        saveList.Add(hotkeySettings);
                    }
                    //set default inputsource value of other monitor due to the hotkey is global
                    List<MonitorInfo> defaultMoList = _AllInfoMonitors.Where(x => !x.edid.ServiceTag.Equals(mo.edid.ServiceTag)).ToList();
                    foreach (var m in defaultMoList)
                    {
                        List<InputSourceObj> defaultList = new List<InputSourceObj>();
                        Debug.WriteLine($"set default value on mo: {m.edid.ServiceTag}");
                        Debug.WriteLine($"set default value on mo: {m.inputSource}");
                        switch (info.Job)
                        {
                            case HotkeyType.FavoriteInputSource:
                                defaultList.Add(new InputSourceObj(m.inputSource));
                                break;

                            case HotkeyType.SwitchInputSource:
                                defaultList.Add(new InputSourceObj(m.inputSource));
                                Dictionary<string, InputInfo> result = GetInputSourcelist(m).Result;
                                if (result != null)
                                {
                                    string sencondInput = result.Keys.FirstOrDefault(x => !x.Equals(m.inputSource));
                                    defaultList.Add(new InputSourceObj(sencondInput));
                                }
                                break;
                        }
                        GetInputSourceHotKeyDataAndSaveNewBack(m, info.Job, defaultList);
                    }
                }
            }

            foreach (HotkeySettings setting in allSettings)
            {
                if (saveList.Any(x => x.SerialNumber.Equals("DDPM")))//setting.SerialNumber)))
                    continue;
                saveList.Add(setting);
            }
            Debug.WriteLine($"SaveHotkeySetting -> saveList -> count: {saveList.Count}");
            if (saveList.Count > 0)
            {
                //1006 1007
                if (mo != null)
                {
                    Debug.WriteLine($"{mo.edid.ServiceTag}: SaveHotkeySetting:GetInputSourceHotKeyDataAndSaveNewBack,InputSource count:[{info.InputSource.Count}] ");
                    GetInputSourceHotKeyDataAndSaveNewBack(mo, info.Job, info.InputSource);
                }
                else
                {
                    Debug.WriteLine($"SaveHotkeySetting:GetInputSourceHotKeyDataAndSaveNewBack mo is null ");
                }
            }
            if (WriteHotkeySettings(saveList).Result)
            {
                if (_NKVMPlugin != null && info.Job != HotkeyType.NkvmConflict)
                {
                    _NKVMPlugin.ToNKVM_HotkeySettings(saveList).Wait();
                    if (_NKVMPlugin.IsNamedpipeConnected().Result)
                    {
                        bool b = _NKVMPlugin.SetHotkey(info).Result;
                    }
                }
            }
            ReloadHotkeyConfigData();
            //Telementry Collection
            var rt = false;
            var applicationSettings_Function = new ApplicationSettings_Function();
            if (mo != null)
            {
                string hotkeyTelementryData = string.Empty;
                switch (info.Job)
                {
                    case HotkeyType.ToggleInputSource:
                        hotkeyTelementryData = HotkeyTelementryHelper.toHotKeyText(info.Hotkey);
                        break;
                    case HotkeyType.SwitchInputSource:
                        hotkeyTelementryData = HotkeyTelementryHelper.getHotkeyNoStr(info.Hotkey);
                        break;
                    case HotkeyType.ChangePIPPosition:
                        hotkeyTelementryData = HotkeyTelementryHelper.getHotkeyNoStr(info.Hotkey);
                        break;
                    case HotkeyType.ToggleEzRecentSetting:
                        hotkeyTelementryData = HotkeyTelementryHelper.getHotkeyNoStr(info.Hotkey);
                        break;
                    case HotkeyType.VisionEngineToggle:
                        hotkeyTelementryData = HotkeyTelementryHelper.getHotkeyNoStr(info.Hotkey);
                        break;
                    case HotkeyType.DarkStabilizerToggle:
                        hotkeyTelementryData = HotkeyTelementryHelper.getHotkeyNoStr(info.Hotkey);
                        break;
                }
                Debug.WriteLine($"HotkeyTelemetry:{mo.edid.SerialNumber}:{info.Job}=> {hotkeyTelementryData}");
                writelog("[DeviceMangerPlugin] [Telementry] Send Telementry for PowerNap...");
                rt = applicationSettings_Function.Send_Hotkey_Telementry(_TelementryScheduler, _AllInfoMonitors, hotkeyTelementryData, info.Job);
                if (rt) writelog("[DeviceMangerPlugin] [Telementry] Send Telementry for HotkeyTelemetry Success ...");
                else writelog("[DeviceMangerPlugin] [Telementry] Send Telementry for HotkeyTelemetry Fail ...");
            }

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
            if (hotkeyInfo.Hotkey.Count == 1 && hotkeyInfo.Hotkey[0] != VirtualKey.None)
            {
                return Task.FromResult(HotkeyWarning.SingleKey);
            }
            if (hotkeyInfo.Hotkey.Count == 1 && hotkeyInfo.Hotkey[0] == VirtualKey.None)
            {
                return Task.FromResult(HotkeyWarning.None);
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
                        monitorSnList.Add("DDPM");// hotkeySetting.SerialNumber);
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

        private bool _OSDKeyLock = false;

        private void Keyboard_KeyUpProc(object sender, KeyEventArgs e)
        {
            string strKey = e.KeyCode.ToString().ToUpper();
            Debug.WriteLine($"Keyboard_KeyUpProc ---{strKey}");

            bool _altPressed = _HotkeyPlugin.IsKeyPushedDown(System.Windows.Forms.Keys.Menu);
            bool _ctrlPressed = _HotkeyPlugin.IsKeyPushedDown(System.Windows.Forms.Keys.ControlKey);
            bool _shiftPressed = _HotkeyPlugin.IsKeyPushedDown(System.Windows.Forms.Keys.ShiftKey);
            if (_altPressed && strKey.Equals("Z"))
            {
                CallQAM_UI(this);
                return;
            }

            //osd
            GlobalSettingParam result = GetGlobalSettingParam().Result;
            if (result != null)
            {
                Debug.WriteLine($"GlobalSettingParam.GlobalSetting_General.Keyboard_Lock_Key={result.GlobalSetting_General.Keyboard_Lock_Key}");
                if (result.GlobalSetting_General.Keyboard_Lock_Key)
                {
                    if (e.KeyCode == Keys.CapsLock)
                    {
                        bool isCapsLockOn = (System.Windows.Input.Keyboard.GetKeyStates(System.Windows.Input.Key.CapsLock) & System.Windows.Input.KeyStates.Toggled) == System.Windows.Input.KeyStates.Toggled;
                        Debug.WriteLine($"Key.CapsLock={isCapsLockOn}");
                        if (isCapsLockOn)
                        {
                            ShowOSD(Screen.PrimaryScreen.DeviceName, OSDType.CapsLock, true);
                        }
                        else
                        {
                            ShowOSD(Screen.PrimaryScreen.DeviceName, OSDType.CapsLock, false);
                        }
                        //_OSDKeyLock = true;
                        //e.Handled = true;
                    }
                    if (e.KeyCode == Keys.Scroll)
                    {
                        bool isScrollLockOn = (System.Windows.Input.Keyboard.GetKeyStates(System.Windows.Input.Key.CapsLock) & System.Windows.Input.KeyStates.Toggled) == System.Windows.Input.KeyStates.Toggled;
                        Debug.WriteLine($"Key.Scroll={isScrollLockOn}");
                        if (isScrollLockOn)
                        {
                            ShowOSD(Screen.PrimaryScreen.DeviceName, OSDType.ScrollLock, true);
                        }
                        else
                        {
                            ShowOSD(Screen.PrimaryScreen.DeviceName, OSDType.ScrollLock, false);
                        }
                        //_OSDKeyLock = true;
                        //e.Handled = true;
                    }
                    if (e.KeyCode == Keys.NumLock)
                    {
                        bool isNumLockLockOn = (System.Windows.Input.Keyboard.GetKeyStates(System.Windows.Input.Key.CapsLock) & System.Windows.Input.KeyStates.Toggled) == System.Windows.Input.KeyStates.Toggled;
                        Debug.WriteLine($"Key.NumLock={isNumLockLockOn}");
                        if (isNumLockLockOn)
                        {
                            ShowOSD(Screen.PrimaryScreen.DeviceName, OSDType.NumLock, true);
                        }
                        else
                        {
                            ShowOSD(Screen.PrimaryScreen.DeviceName, OSDType.NumLock, false);
                        }
                        ShowOSD(Screen.PrimaryScreen.DeviceName, OSDType.NumLock, true);
                        //_OSDKeyLock = true;
                        //e.Handled = true;
                    }
                }
                //else
                //{
                //    if (_OSDKeyLock)
                //    {
                //        if (e.KeyCode == Keys.CapsLock)
                //        {
                //            ShowOSD(Screen.PrimaryScreen.DeviceName, OSDType.CapsLock, false);
                //            _OSDKeyLock = false;
                //        }
                //        if (e.KeyCode == Keys.Scroll)
                //        {
                //            ShowOSD(Screen.PrimaryScreen.DeviceName, OSDType.ScrollLock, false);
                //            _OSDKeyLock = false;
                //        }
                //        if (e.KeyCode == Keys.NumLock)
                //        {
                //            ShowOSD(Screen.PrimaryScreen.DeviceName, OSDType.NumLock, false);
                //            _OSDKeyLock = false;
                //        }
                //    }
                //}
            }

            //osd
            if (_hotkeySettings != null && _hotkeySettings.Count == 0)
            {
                _hotkeySettings = _SettingsPlugin.ReadHotkeySettings().Result;
            }
            if (_hotkeySettings != null && _hotkeySettings.Count > 0)
            {
                foreach (var settings in _hotkeySettings)
                {
                    foreach (var hotkeyInfo in settings.HotkeyInfo)
                    {
                        var xx = hotkeyInfo.Hotkey.Any(x => x == VirtualKey.Menu);
                        var b1 = hotkeyInfo.Hotkey.Any(x => (int)x == e.KeyValue);
                        Debug.WriteLine($"{hotkeyInfo.Description}: Hotkey => : {string.Join("+", hotkeyInfo.Hotkey.Select(x => x + "(" + (int)x + ")").ToList())}");
                        Debug.WriteLine($"key _ctrlPressed={_ctrlPressed}; _altPressed={_altPressed}; _shiftPressed={_shiftPressed}; current pressed:{e.KeyValue}={e.KeyCode},isUsing={b1}");
                        if (_ctrlPressed || _altPressed || _shiftPressed)
                        {
                            writelog($"{hotkeyInfo.Description}: Hotkey => : {string.Join("+", hotkeyInfo.Hotkey.Select(x => x + "(" + (int)x + ")").ToList())}");
                            writelog($"key _ctrlPressed={_ctrlPressed}; _altPressed={_altPressed}; _shiftPressed={_shiftPressed}; current pressed:{e.KeyValue}={e.KeyCode},isUsing={b1}");
                        }
                        if (hotkeyInfo.Hotkey.Any(x => x == VirtualKey.Control) == _ctrlPressed
                        && hotkeyInfo.Hotkey.Any(x => x == VirtualKey.Menu) == _altPressed
                        && hotkeyInfo.Hotkey.Any(x => x == VirtualKey.Shift) == _shiftPressed
                        && hotkeyInfo.Hotkey.Any(x => (int)x == e.KeyValue))
                        {
                            Debug.WriteLine($"job matched:{hotkeyInfo.Job}");
                            HotkeyType job = hotkeyInfo.Job;
                            ExecHotkeyJob(settings, job);
                        }
                    }
                }
            }
            else
            {
                if (_hotkeySettings != null)
                {
                    Debug.WriteLine($"Keyboard_KeyUpProc ==> _hotkeySettings :count = {_hotkeySettings.Count}");
                }
                else
                {
                    Debug.WriteLine($"Keyboard_KeyUpProc==> _hotkeySettings is null");
                }
            }
        }

        public Task SetLastSelectedMonitorFromUI(MonitorInfo mo)
        {
            lastSelectedMonitor_UI = mo;
            return Task.CompletedTask;
        }

        private Task<bool> ExecHotkeyJob(HotkeySettings settings, HotkeyType job)
        {
            //1001 add to tracking mouse point and its location on specific monitor
            //cursor position
            System.Drawing.Point cursorPosition = Cursor.Position;

            // retrieve the monitor object from cursor's position
            Screen currentScreen = Screen.FromPoint(cursorPosition);
            //Here should change to be (1)last UI selected monitor or (2)dell monitor with mouse placed in [Dean 1001]
            //check (2)
            MonitorInfo monitorInfo = _AllInfoMonitors.Find(x => x.DisplayName.ToUpper().Equals(currentScreen.DeviceName.ToUpper()));
            Debug.WriteLine($"cursor mo ={monitorInfo?.edid.ServiceTag}");
            bool getTargetMo = false;
            if (monitorInfo == null)
            {
                writelog($"[ExecHotkeyJob] null dell monitor get over mouse: locate at Screen({currentScreen.DeviceName})");
                //check (1)
                if (lastSelectedMonitor_UI == null)
                {
                    Debug.WriteLine($"[ExecHotkeyJob：{job}] UI didn't set any selected monitor");
                    writelog($"[ExecHotkeyJob：{job}] UI didn't set any selected monitor");
                    return Task.FromResult(false);
                }
                monitorInfo = _AllInfoMonitors.Find(x => x.modelName.Equals(lastSelectedMonitor_UI.modelName) && x.edid.ServiceTag.Equals(lastSelectedMonitor_UI.edid.ServiceTag));
                if (monitorInfo == null)
                {
                    writelog($"[ExecHotkeyJob：{job}] Selected monitor ({lastSelectedMonitor_UI.modelName}) from UI do not exist in current monitor list");
                    Debug.WriteLine($"[ExecHotkeyJob：{job}] Selected monitor ({lastSelectedMonitor_UI.modelName}) from UI do not exist in current monitor list");
                    return Task.FromResult(false);
                }
            }
            else
            {
                getTargetMo = true;
            }
            Debug.WriteLine($"getTargetMo: {getTargetMo}");
            Debug.WriteLine($"job: {job}");
            writelog($"job: {job}");
            switch (job)
            {
                case HotkeyType.BrightnessReduce:
                    if (!isAutoBrightnessOn(monitorInfo))
                        if (IsALSautobrightness(monitorInfo))
                        {
                            HotkeyPopWrap hotkeyPopWrap = new HotkeyPopWrap() { monitorInfo = monitorInfo, hotkeyType = job };
                            HotkeyPopup(hotkeyPopWrap);
                        }
                        else
                        {
                            _hotkeyJobQueue.Enqueue(new JobInfo(1000, monitorInfo, null, Reduce_Brightness_Value));
                        }
                    break;

                case HotkeyType.BrightnessIncrease:
                    if (!isAutoBrightnessOn(monitorInfo))
                        if (IsALSautobrightness(monitorInfo))
                        {
                            HotkeyPopWrap hotkeyPopWrap = new HotkeyPopWrap() { monitorInfo = monitorInfo, hotkeyType = job };
                            HotkeyPopup(hotkeyPopWrap);
                        }
                        else
                        {
                            _hotkeyJobQueue.Enqueue(new JobInfo(1000, monitorInfo, null, Increase_Brightness_Value));
                        }
                    break;

                case HotkeyType.ContrastReduce:
                    if (!isAutoBrightnessOn(monitorInfo))
                        if (IsALSautobrightness(monitorInfo))
                        {
                            HotkeyPopWrap hotkeyPopWrap = new HotkeyPopWrap() { monitorInfo = monitorInfo, hotkeyType = job };
                            HotkeyPopup(hotkeyPopWrap);
                        }
                        else
                        {
                            _hotkeyJobQueue.Enqueue(new JobInfo(1000, monitorInfo, null, Reduce_Contrast_Value));
                        }
                    break;

                case HotkeyType.ContrastIncrease:
                    if (!isAutoBrightnessOn(monitorInfo))
                        if (IsALSautobrightness(monitorInfo))
                        {
                            HotkeyPopWrap hotkeyPopWrap = new HotkeyPopWrap() { monitorInfo = monitorInfo, hotkeyType = job };
                            HotkeyPopup(hotkeyPopWrap);
                        }
                        else
                        {
                            _hotkeyJobQueue.Enqueue(new JobInfo(1000, monitorInfo, null, Increase_Contrast_Value));
                        }
                    break;

                case HotkeyType.LuminanceReduce:
                    _hotkeyJobQueue.Enqueue(new JobInfo(1000, monitorInfo, null, Reduce_Luminance_Value));
                    break;

                case HotkeyType.LuminanceIncrease:
                    _hotkeyJobQueue.Enqueue(new JobInfo(1000, monitorInfo, null, Increase_Luminance_Value));
                    break;

                case HotkeyType.ToggleInputSource:
                    _hotkeyJobQueue.Enqueue(new JobInfo(1000, monitorInfo, null, Toggle_InputSource));
                    break;

                case HotkeyType.FavoriteInputSource:
                    HotkeyInfo hotkeyInfoIs = settings.HotkeyInfo.Where(x => x.Job.Equals(HotkeyType.FavoriteInputSource)).SingleOrDefault();
                    List<HotkeyData> list = GetInputSourceHotKeyData(monitorInfo);
                    HotkeyData hotkeyData = list.SingleOrDefault(x => x.hotkeyType == HotkeyType.FavoriteInputSource);
                    Debug.WriteLine($"FavoriteInputSource: {hotkeyData?.inputSource.Count}");
                    if (hotkeyInfoIs != null && hotkeyData != null)// hotkeyInfoIs.InputSource != null)
                    {
                        _hotkeyJobQueue.Enqueue(new JobInfo(1000, monitorInfo, new object[] { hotkeyInfoIs, hotkeyData.inputSource }, Favorite_InputSource));
                    }
                    break;

                case HotkeyType.SwitchInputSource:
                    HotkeyInfo hotkeyInfo = settings.HotkeyInfo.Where(x => x.Job.Equals(HotkeyType.SwitchInputSource)).SingleOrDefault();
                    List<HotkeyData> list2 = GetInputSourceHotKeyData(monitorInfo);
                    HotkeyData hotkeyData2 = list2.SingleOrDefault(x => x.hotkeyType == HotkeyType.SwitchInputSource);
                    if (hotkeyInfo != null && hotkeyData2 != null)// hotkeyInfo.InputSource != null)
                    {
                        _hotkeyJobQueue.Enqueue(new JobInfo(1000, monitorInfo, new object[] { hotkeyInfo, hotkeyData2.inputSource }, Switch_InputSource));
                    }
                    break;

                case HotkeyType.SwapIputPIPPBP:
                    HotkeyInfo hotkeyInfo_SwapIputPIPPBP = settings.HotkeyInfo.Where(x => x.Job.Equals(HotkeyType.SwapIputPIPPBP)).SingleOrDefault();
                    if (hotkeyInfo_SwapIputPIPPBP != null)
                        _hotkeyJobQueue.Enqueue(new JobInfo(1000, monitorInfo, new object[] { hotkeyInfo_SwapIputPIPPBP }, Swap_IputPIPPBP));
                    break;

                case HotkeyType.ChangePIPPosition:
                    _hotkeyJobQueue.Enqueue(new JobInfo(1000, monitorInfo, null, Change_PIPPosition));
                    break;

                case HotkeyType.KvmSwitchInputSource:
                    HotkeyInfo kvmhotkeyInfo = settings.HotkeyInfo.Where(x => x.Job.Equals(HotkeyType.KvmSwitchInputSource)).SingleOrDefault();
                    List<HotkeyData> list3 = GetInputSourceHotKeyData(monitorInfo);
                    HotkeyData hotkeyData3 = list3.SingleOrDefault(x => x.hotkeyType == HotkeyType.KvmSwitchInputSource);
                    if (kvmhotkeyInfo != null && hotkeyData3 != null)// kvmhotkeyInfo.InputSource != null)
                    {
                        _hotkeyJobQueue.Enqueue(new JobInfo(1000, monitorInfo, new object[] { kvmhotkeyInfo, hotkeyData3.inputSource }, Kvm_SwitchInputSource));
                    }
                    break;

                case HotkeyType.KvmSwitchKbMsKey:
                    _hotkeyJobQueue.Enqueue(new JobInfo(1000, monitorInfo, null, Kvm_SwitchKbMsKey));
                    break;

                case HotkeyType.KvmChangePIPPosition:
                    _hotkeyJobQueue.Enqueue(new JobInfo(1000, monitorInfo, null, Kvm_ChangePIPPosition));
                    break;

                case HotkeyType.DarkStabilizerToggle:
                    _hotkeyJobQueue.Enqueue(new JobInfo(1000, monitorInfo, null, Gaming_DarkStabilizerToggle));
                    break;

                case HotkeyType.DualResolutionToggle:
                    _hotkeyJobQueue.Enqueue(new JobInfo(1000, monitorInfo, null, Gaming_DualResolutionToggle));
                    break;

                case HotkeyType.VisionEngineToggle:
                    _hotkeyJobQueue.Enqueue(new JobInfo(1000, monitorInfo, null, Gaming_VisionEngineToggle));
                    break;

                case HotkeyType.ToggleEzRecentSetting:
                    _hotkeyJobQueue.Enqueue(new JobInfo(1000, monitorInfo, null, Toggle_EzRecentSetting));
                    break;
            }
            return Task.FromResult(true);
        }

        private bool isAutoBrightnessOn(MonitorInfo mo)
        {
            scheduleInfo result = ReadScheduleMonitorSettings(mo).Result;
            return result.IsEnable;
        }

        private void Toggle_EzRecentSetting(MonitorInfo monitorInfo, Object[] param)
        {
            //Validation
            //todo Toggle_EzRecentSetting
            //Read the EAMonitorSettings
            EAMonitorSettings eaSettings = ReadEAMonitorSettings(monitorInfo).Result;
            //Change selected layout to the latest item of RecentList
            int idxRecent = 0;
            if (eaSettings.RecentList == null)
            {
                writelog("@ Toggle_EzRecentSetting(), EA RecentList is null");
                return;
            }
            if (eaSettings.RecentList.Length == 0)
            {
                writelog("@ Toggle_EzRecentSetting(), EA RecentList is empty");
                return;
            }
            else
            {
                //Should be always EAEMConstants.MaxRecentItems(=5)-1 = 4
                writelog($"@ Toggle_EzRecentSetting(), EA RecentList.Count={eaSettings.RecentList.Length}");
            }
            idxRecent = eaSettings.RecentList.Length - 1;

            //Force await to avoid reenter this method (it will update to MonitorSettings file)
            bool isOKSetSelected = SetEASelectedLayout(monitorInfo, eaSettings.RecentList[idxRecent]).Result;

            //TO DO: invoke an event to UI to reload settings
            // TO be implement in EASettingsChanged event

            writelog($"@ Toggle_EzRecentSetting(), result is {isOKSetSelected}");
        }

        private bool IsHotkeyFuncLock(HotkeyType type)
        {
            DDPMSettings config = _SettingsPlugin.ReloadAppConfigData().Result;
            if (config != null)
            {
                switch (type)
                {
                    case HotkeyType.LockBriCont:
                        return config.LockSettings.Lock_Display_BriCont;

                    case HotkeyType.LockActiveInputSource:
                        return config.LockSettings.Lock_Display_ActiveInputSource;
                }
            }
            return false;
        }

        private void Gaming_VisionEngineToggle(MonitorInfo monitorInfo, Object[] param)
        {
            GamingDisplayPropertiesInfo gamingDisplayProperties = GetGamingProperties_SupportedList(monitorInfo).Result;
            gamingDisplayProperties.IsEnable_VisionEngineType = GetCurrentGaming_VisionEngineEnableType(monitorInfo, gamingDisplayProperties).Result;
            gamingDisplayProperties.Current_VisionEngineType = GetCurrentGaming_VisionEngineType(monitorInfo).Result;
            if (gamingDisplayProperties != null && gamingDisplayProperties.IsSupported_VisionEngineType)
            {
                List<Gaming_VisionEngineType> supported_VisionEngineType = gamingDisplayProperties.Supported_VisionEngineType;
                //supported_VisionEngineType.Insert(0, Gaming_VisionEngineType.off);
                if (supported_VisionEngineType != null && supported_VisionEngineType.Count > 0)
                {
                    List<Gaming_VisionEngineType> enabledList = new List<Gaming_VisionEngineType>();
                    enabledList.Add(Gaming_VisionEngineType.off);
                    for (int i = 0; i < gamingDisplayProperties.Supported_VisionEngineType.Count; i++)
                    {
                        if (gamingDisplayProperties.IsEnable_VisionEngineType[i])
                        {
                            Debug.WriteLine(gamingDisplayProperties.Supported_VisionEngineType[i]);
                            enabledList.Add(gamingDisplayProperties.Supported_VisionEngineType[i]);
                        }
                    }
                    if (enabledList.Count > 0)
                    {
                        Gaming_VisionEngineType current_VisionEngineType = gamingDisplayProperties.Current_VisionEngineType;
                        Gaming_VisionEngineType nextVisionEngineType = Gaming_VisionEngineType.off;

                        for (int i = 0; i < enabledList.Count; i++)
                        {
                            if (enabledList[i].Equals(current_VisionEngineType))
                            {
                                if (i < (enabledList.Count - 1))
                                {
                                    nextVisionEngineType = enabledList[i + 1];
                                }
                                else
                                {
                                    nextVisionEngineType = enabledList[0];
                                }
                            }
                        }
                        Debug.WriteLine($"Gaming_VisionEngineToggle:[{monitorInfo.edid.ModelName}:{monitorInfo.edid.SerialNumber}] from [{current_VisionEngineType}] to [{nextVisionEngineType}]");
                        bool result = SwitchGaming_VisionEngineType(monitorInfo, nextVisionEngineType).Result;
                        writelog($"Gaming_VisionEngineToggle:[{monitorInfo.edid.ModelName}:{monitorInfo.edid.SerialNumber}] from [{current_VisionEngineType}] to [{nextVisionEngineType}]" + (result ? "success" : "fail"));
                    }
                    else
                    {
                        writelog($"Gaming_VisionEngineToggle:[{monitorInfo.edid.ModelName}:{monitorInfo.edid.SerialNumber}] not Gaming VisionEngine checked");
                    }
                    /*Gaming_VisionEngineType current_VisionEngineType = gamingDisplayProperties.Current_VisionEngineType;
                    Gaming_VisionEngineType nextVisionEngineType = Gaming_VisionEngineType.off;

                    for (int i = 0; i < supported_VisionEngineType.Count; i++)
                    {
                        if (supported_VisionEngineType[i].Equals(current_VisionEngineType))
                        {
                            if (i < (supported_VisionEngineType.Count - 1))
                            {
                                nextVisionEngineType = supported_VisionEngineType[i + 1];
                            }
                            else
                            {
                                nextVisionEngineType = supported_VisionEngineType[0];
                            }
                        }
                    }
                    Debug.WriteLine($"Gaming_VisionEngineToggle:[{monitorInfo.edid.ModelName}:{monitorInfo.edid.SerialNumber}] from [{current_VisionEngineType}] to [{nextVisionEngineType}]");
                    bool result = SwitchGaming_VisionEngineType(monitorInfo, nextVisionEngineType).Result;
                    writelog($"Gaming_VisionEngineToggle:[{monitorInfo.edid.ModelName}:{monitorInfo.edid.SerialNumber}] from [{current_VisionEngineType}] to [{nextVisionEngineType}]" + (result ? "success" : "fail"));*/
                }
                else
                {
                    writelog($"Gaming_VisionEngineToggle:[{monitorInfo.edid.ModelName}:{monitorInfo.edid.SerialNumber}] Gaming VisionEngine is empty");
                }
            }
            else
            {
                writelog($"Gaming_VisionEngineToggle:[{monitorInfo.edid.ModelName}:{monitorInfo.edid.SerialNumber}] not support Gaming VisionEngine");
            }
        }

        private void Gaming_DualResolutionToggle(MonitorInfo monitorInfo, Object[] param)
        {
            GamingDisplayPropertiesInfo gamingDisplayProperties = GetGamingProperties_SupportedList(monitorInfo).Result;
            gamingDisplayProperties.Current_DualResolutionType = GetCurrentGaming_DualResolutionType(monitorInfo).Result;
            if (gamingDisplayProperties != null && gamingDisplayProperties.IsSupported_DualResolutionType)
            {
                List<Gaming_DualResolutionType> supported_DualResolutionType = gamingDisplayProperties.Supported_DualResolutionType;
                if (supported_DualResolutionType != null && supported_DualResolutionType.Count > 0 && gamingDisplayProperties.Current_DualResolutionType != null)
                {
                    Gaming_DualResolutionType current_DualResolutionType = (Gaming_DualResolutionType)gamingDisplayProperties.Current_DualResolutionType;
                    Gaming_DualResolutionType nextDualResolutionType = Gaming_DualResolutionType.Unknow;

                    for (int i = 0; i < supported_DualResolutionType.Count; i++)
                    {
                        if (supported_DualResolutionType[i].Equals(current_DualResolutionType))
                        {
                            if (i < (supported_DualResolutionType.Count - 1))
                            {
                                nextDualResolutionType = supported_DualResolutionType[i + 1];
                            }
                            else
                            {
                                nextDualResolutionType = supported_DualResolutionType[0];
                            }
                        }
                    }
                    Debug.WriteLine($"Gaming_DualResolutionToggle:[{monitorInfo.edid.ModelName}:{monitorInfo.edid.SerialNumber}] from [{current_DualResolutionType}] to [{nextDualResolutionType}]");
                    bool result = SetGaming_DualResolutionType(monitorInfo, nextDualResolutionType).Result;
                    //Bruce ,Evente back UI
                    if (result)
                    {
                        gamingDisplayProperties.Current_DualResolutionType = nextDualResolutionType;
                        gamingDisplayProperties.Current_DarkStabilizer = null;
                        gamingDisplayProperties.Current_HDRType = null;
                        gamingDisplayProperties.Current_ResponseTime = null;
                        gamingDisplayProperties.Current_GameEnhancementMode = null;
                        OnGamingParamChangeHandler(this, gamingDisplayProperties);
                    }
                    writelog($"Gaming_DualResolutionToggle:[{monitorInfo.edid.ModelName}:{monitorInfo.edid.SerialNumber}] from [{current_DualResolutionType}] to [{nextDualResolutionType}]" + (result ? "success" : "fail"));
                }
                else
                {
                    writelog($"Gaming_DualResolutionToggle:[{monitorInfo.edid.ModelName}:{monitorInfo.edid.SerialNumber}] Gaming DualResolution is empty");
                }
            }
            else
            {
                writelog($"Gaming_DualResolutionToggle:[{monitorInfo.edid.ModelName}:{monitorInfo.edid.SerialNumber}] not support Gaming DualResolution");
            }
        }

        private void Gaming_DarkStabilizerToggle(MonitorInfo monitorInfo, Object[] param)
        {
            GamingDisplayPropertiesInfo gamingDisplayProperties = GetGamingProperties_SupportedList(monitorInfo).Result;
            gamingDisplayProperties.Current_DarkStabilizer = GetCurrentGaming_DarkStabilizer(monitorInfo).Result;
            if (gamingDisplayProperties != null && gamingDisplayProperties.IsSupported_DarkStabilizer)
            {
                List<Gaming_DarkStabilizer> supported_DarkStabilizer = gamingDisplayProperties.Supported_DarkStabilizer;
                if (supported_DarkStabilizer != null && supported_DarkStabilizer.Count > 0 && gamingDisplayProperties.Current_DarkStabilizer != null)
                {
                    Gaming_DarkStabilizer current_DarkStabilizer = (Gaming_DarkStabilizer)gamingDisplayProperties.Current_DarkStabilizer;
                    Gaming_DarkStabilizer nextDarkStabilizer = Gaming_DarkStabilizer.Disable;

                    for (int i = 0; i < supported_DarkStabilizer.Count; i++)
                    {
                        if (supported_DarkStabilizer[i].Equals(current_DarkStabilizer))
                        {
                            if (i < (supported_DarkStabilizer.Count - 1))
                            {
                                nextDarkStabilizer = supported_DarkStabilizer[i + 1];
                            }
                            else
                            {
                                nextDarkStabilizer = supported_DarkStabilizer[0];
                            }
                        }
                    }
                    Debug.WriteLine($"Gaming_DarkStabilizerToggle:[{monitorInfo.edid.ModelName}:{monitorInfo.edid.SerialNumber}] from [{current_DarkStabilizer}] to [{nextDarkStabilizer}]");
                    bool result = SetGaming_DarkStabilizer(monitorInfo, nextDarkStabilizer).Result;
                    //Bruce ,Evente back UI
                    if (result)
                    {
                        gamingDisplayProperties.Current_DarkStabilizer = nextDarkStabilizer;
                        gamingDisplayProperties.Current_DualResolutionType = null;
                        gamingDisplayProperties.Current_HDRType = null;
                        gamingDisplayProperties.Current_ResponseTime = null;
                        gamingDisplayProperties.Current_GameEnhancementMode = null;
                        OnGamingParamChangeHandler(this, gamingDisplayProperties);
                    }
                    writelog($"Gaming_DarkStabilizerToggle:[{monitorInfo.edid.ModelName}:{monitorInfo.edid.SerialNumber}] from [{current_DarkStabilizer}] to [{nextDarkStabilizer}]" + (result ? "success" : "fail"));
                }
                else
                {
                    writelog($"Gaming_DarkStabilizerToggle:[{monitorInfo.edid.ModelName}:{monitorInfo.edid.SerialNumber}] Gaming DarkStabilizer is empty");
                }
            }
            else
            {
                writelog($"Gaming_DarkStabilizerToggle:[{monitorInfo.edid.ModelName}:{monitorInfo.edid.SerialNumber}] not support Gaming DarkStabilizer");
            }
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
            List<InputSourceObj> list = (List<InputSourceObj>)param[1];// GetInputSourceHotKeyData(monitorInfo);
            if (list == null | list.Count == 0)//hotkey.InputSource.Count == 0)
            {
                //hotkey.InputSource Count must not 0
                return;
            }
            string crtInput = monitorInfo.inputSource;
            // InputSourceObj switchTo = hotkey.InputSource.FirstOrDefault(x => !x.Name.Equals(crtInput));
            string nextInput = string.Empty;
            //List<string> inputsList = hotkey.InputSource.OrderBy(x => x.Name).Select(input => input.Name).ToList();
            List<string> inputsList = list.OrderBy(x => x.Name).Select(input => input.Name).ToList();
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
            writelog($"Kvm_SwitchKbMsKey:[{monitorInfo.edid.ModelName}:{monitorInfo.edid.SerialNumber}]" + (usbSwitch ? "success" : "fail"));
        }

        private void Kvm_ChangePIPPosition(MonitorInfo monitorInfo, Object[] param)
        {
            Change_PIPPosition(monitorInfo, param);
        }

        private void Change_PIPPosition(MonitorInfo monitorInfo, Object[] param)
        {
            if (!IsHotkeyFuncLock(HotkeyType.LockActiveInputSource))
            {
                if (!IsPIPMode(monitorInfo))
                {
                    //pxp off
                    Debug.WriteLine($"Monitor: {monitorInfo.edid.ServiceTag} Swap_IputPIPPBP not take effect due to PXP mode is off or not supported");
                    writelog($"[hotkey]Swap_IputPIPPBP:[{monitorInfo.edid.ModelName}:{monitorInfo.edid.SerialNumber}] will not take effect due to PXP mode is off");
                    return;
                }
                else
                {
                    bool changePip = TogglePipPosition(monitorInfo).Result;
                    writelog($"Change_PIPPosition:[{monitorInfo.edid.ModelName}:{monitorInfo.edid.SerialNumber}] " + (changePip ? "success" : "fail"));
                }
            }
        }

        private bool IsPIPMode(MonitorInfo mo)
        {
            ObjGetVCP pxpMode = GetPxpMode(mo).Result;
            Debug.WriteLine($"GetPxpMode result={pxpMode?.result}, value={(UInt32)pxpMode.value}");
            if (pxpMode != null && pxpMode.result == true)
            {
                return (UInt32)pxpMode.value != 0;
            }
            return false;
        }

        private void Swap_IputPIPPBP(MonitorInfo monitorInfo, Object[] param)
        {
            string log_keys = string.Empty;
            if (param != null && param.Count() > 0)
            {
                HotkeyInfo hotkey = (HotkeyInfo)param[0];
                log_keys = string.Join("+", hotkey.Hotkey.Select(x => x + "(" + (int)x + ")").ToList());
            }
            Debug.WriteLine($"Monitor: {monitorInfo.edid.ServiceTag} Swap_IputPIPPBP >begin [keys:{log_keys}]");
            writelog($"[hotkey]Swap_IputPIPPBP:[{monitorInfo.edid.ModelName}:{monitorInfo.edid.SerialNumber}] Swap_IputPIPPBP >begin [keys:{log_keys}]");
            if (!IsHotkeyFuncLock(HotkeyType.LockActiveInputSource))
            {
                if (!IsPIPMode(monitorInfo))
                {
                    //pxp off
                    Debug.WriteLine($"Monitor: {monitorInfo.edid.ServiceTag} Swap_IputPIPPBP not take effect due to PXP mode is off or not supported");
                    writelog($"[hotkey]Swap_IputPIPPBP:[{monitorInfo.edid.ModelName}:{monitorInfo.edid.SerialNumber}] will not take effect due to PXP mode is off.[keys:{log_keys}]");
                    return;
                }
                //0 = main, 1 = sub1, 2 = sub2, 3 = sub3
                Dictionary<string, InputInfo> inputList = GetInputSourcelist(monitorInfo).Result;
                //pip/pbp subinput should only one
                List<InputSourceObj> subInputs = GetSubInputs(monitorInfo).Result;
                List<InputSourceObj> allInputs = new List<InputSourceObj>();
                //inputList.ForEach(input => allInputs.Add(new InputSourceObj(input.Value.InputName)));
                //[Dean] remove WinCopies utilties and fix code conflict
                writelog($"[hotkey]Swap_IputPIPPBP:[{monitorInfo.edid.ModelName}:{monitorInfo.edid.SerialNumber}] inputList(count): {inputList.Count}.[keys:{log_keys}]");
                foreach (var input in inputList)
                {
                    allInputs.Add(new InputSourceObj((ushort)input.Value.Code, input.Value.InputName));
                    writelog($"[hotkey]Swap_IputPIPPBP:[{monitorInfo.edid.ModelName}:{monitorInfo.edid.SerialNumber}] inputList[{input.Key}] ==> {input.Value.InputName}, {input.Value.Code}.[keys:{log_keys}]");
                }
                //debug
                int idx = 0;
                writelog($"[hotkey]Swap_IputPIPPBP:[{monitorInfo.edid.ModelName}:{monitorInfo.edid.SerialNumber}] subInputs(count): {subInputs.Count}.[keys:{log_keys}]");
                foreach (var s in subInputs)
                {
                    Debug.WriteLine($"subInputs[{idx}] ==> {s.Code}, {s.Name}"); //Robert_Lin, 2024-10-16 add idx and Code
                    writelog($"[hotkey]Swap_IputPIPPBP:[{monitorInfo.edid.ModelName}:{monitorInfo.edid.SerialNumber}] subInputs[{idx}] ==> {s.Code}, {s.Name}.[keys:{log_keys}]");
                    idx++;
                }
                idx = 0;
                writelog($"[hotkey]Swap_IputPIPPBP:[{monitorInfo.edid.ModelName}:{monitorInfo.edid.SerialNumber}] allInputs(count): {allInputs.Count}.[keys:{log_keys}]");
                foreach (var s in allInputs)
                {
                    Debug.WriteLine($"allInputs[{idx}] ==> {s.Code}, {s.Name}"); //Robert_Lin, 2024-10-16 add idx and Code
                    writelog($"[hotkey]Swap_IputPIPPBP:[{monitorInfo.edid.ModelName}:{monitorInfo.edid.SerialNumber}] allInputs[{idx}] ==> {s.Code}, {s.Name}.[keys:{log_keys}]");
                    idx++;
                }
                //Robert_Lin, 2024-10-16, changed
                //debug end
                //OLD:
                //List<int> swapList = subInputs.Select(tmp => allInputs.IndexOf(allInputs.FirstOrDefault(x => x.Name.Equals(tmp.Name.Replace("-", "")) && x.Code.Equals(tmp.Code)))).ToList();
                //NEW:
                List<int> swapList = subInputs.Select(tmp => allInputs.IndexOf(allInputs.FirstOrDefault(x => x.Code.Equals(tmp.Code)))).ToList();
                if (swapList.Count != 1 && swapList.Any(x => x.Equals(-1)))
                {
                    return;
                }
                Trace.WriteLine($"Calling to VideoSwap(0,{swapList[0]})");
                bool swapPxp = VideoSwap(monitorInfo, (UInt16)0, (UInt16)swapList[0]).Result;
                writelog($"Swap_IputPIPPBP:[{monitorInfo.edid.ModelName}:{monitorInfo.edid.SerialNumber}](keys:{log_keys}) from [0] to [{(UInt16)swapList[0]}]" + (swapPxp ? "success" : "fail"));
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
            else
            {
                Debug.WriteLine($"Monitor: {monitorInfo.edid.ServiceTag} Swap_IputPIPPBP >end; HotkeyFuncLock");
                writelog($"[hotkey]Swap_IputPIPPBP:[{monitorInfo.edid.ModelName}:{monitorInfo.edid.SerialNumber}] Swap_IputPIPPBP >end; HotkeyFuncLock [keys:{log_keys}]");
            }
        }

        private void Switch_InputSource(MonitorInfo monitorInfo, Object[] param)
        {
            if (!IsHotkeyFuncLock(HotkeyType.LockActiveInputSource))
            {
                HotkeyInfo hotkey = (HotkeyInfo)param[0];
                List<InputSourceObj> list = (List<InputSourceObj>)param[1];// GetInputSourceHotKeyData(monitorInfo);
                Debug.WriteLine($"Switch_InputSource [{monitorInfo.edid.ServiceTag}]");
                if (list == null || list.Count == 0)//hotkey.InputSource.Count == 0)
                {
                    //hotkey.InputSource Count must not 0
                    Debug.WriteLine($"Switch_InputSource InputSource count is 0");
                    return;
                }
                Dictionary<string, InputInfo> inputList = GetInputSourcelist(monitorInfo).Result;
                //for magration that hotkeyInfo that inputsource name is empty
                if (list.Any(x => string.IsNullOrEmpty(x.Name)))
                {
                    List<InputSourceObj> inputSourceObjs = list.Join(inputList.Values, a => a.Code, b => b.Code, (a, b) => new InputSourceObj()
                    {
                        Name = b.InputName,
                        Code = a.Code,
                    }).ToList();

                    foreach (var item in inputSourceObjs)
                    {
                        Debug.WriteLine($"inputSourceObjs: {item.Name}={item.Code}");
                    }
                    if (inputSourceObjs == null || list.Count == 0)
                    {
                        //hotkey.InputSource Count must not 0.
                        Debug.WriteLine($"Switch_InputSource convert InputSource count is 0");
                        writelog($"Switch_InputSource:[{monitorInfo.edid.ModelName}:{monitorInfo.edid.SerialNumber}],migration hotkeyInfo:inputsoure is empty or can't convert ");
                        return;
                    }
                    else
                    {
                        list.Clear();
                        list.AddRange(inputSourceObjs);
                        //update settings
                        GetInputSourceHotKeyDataAndSaveNewBack(monitorInfo, HotkeyType.FavoriteInputSource, inputSourceObjs);
                    }
                }
                string crtInput = monitorInfo.inputSource;
                Debug.WriteLine($"Switch_InputSource [{monitorInfo.edid.ServiceTag}] crtInput is [{crtInput}]");
                //InputSourceObj switchTo = hotkey.InputSource.FirstOrDefault(x => !x.Name.Equals(crtInput));
                InputSourceObj switchTo = list.FirstOrDefault(x => !x.Name.Equals(crtInput));
                Debug.WriteLine($"Switch_InputSource [{monitorInfo.edid.ServiceTag}] switch to [{switchTo?.Name}]");
                if (switchTo != null)
                {
                    bool setInput = SetVCPCapability(monitorInfo, "Input Select", switchTo.Name).Result;
                    writelog($"Switch_InputSource:[{monitorInfo.edid.ModelName}:{monitorInfo.edid.SerialNumber}] from [{crtInput}] to [{switchTo.Name}]" + (setInput ? "success" : "fail"));
                }
            }
        }

        private void Favorite_InputSource(MonitorInfo monitorInfo, Object[] param)
        {
            if (!IsHotkeyFuncLock(HotkeyType.LockActiveInputSource))
            {
                HotkeyInfo hotkey = (HotkeyInfo)param[0];
                List<InputSourceObj> list = (List<InputSourceObj>)param[1];
                //for magration that hotkeyInfo that inputsource name is empty
                string changeInput = string.Empty;// hotkey.InputSource[0];
                if (list.Any(x => string.IsNullOrEmpty(x.Name)))
                {
                    Dictionary<string, InputInfo> inputList = GetInputSourcelist(monitorInfo).Result;
                    InputInfo inputInfo = inputList.Values.SingleOrDefault(x => x.Code.Equals(list[0].Code));
                    if (inputInfo != null)
                        changeInput = inputInfo.InputName;
                    //update settings
                    List<InputSourceObj> inputSources = new List<InputSourceObj>();
                    inputSources.Add(new InputSourceObj((UInt16)inputInfo.Code, inputInfo.InputName));
                    GetInputSourceHotKeyDataAndSaveNewBack(monitorInfo, HotkeyType.FavoriteInputSource, inputSources);
                }
                else
                {
                    changeInput = list[0].Name;
                }
                Debug.WriteLine($"Favorite_InputSource changeInput[{monitorInfo.edid.ServiceTag}]=> {changeInput}");
                bool setNextInput = SetVCPCapability(monitorInfo, "Input Select", changeInput).Result;
                Debug.WriteLine($"Favorite_InputSource => {setNextInput}");
                writelog($"Favorite_InputSource:[{monitorInfo.edid.ModelName}:{monitorInfo.edid.SerialNumber}] to [{changeInput}]" + (setNextInput ? "success" : "fail"));
            }
        }

        private void Toggle_InputSource(MonitorInfo monitorInfo, Object[] param)
        {
            if (!IsHotkeyFuncLock(HotkeyType.LockActiveInputSource))
            {
                Dictionary<string, InputInfo> result = GetInputSourcelist(monitorInfo).Result;
                string nextInput = string.Empty;
                //get current main input source
                string crtInput = monitorInfo.inputSource;
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
                    case "01":
                        crtInput = "VGA-1";
                        break;

                    case "02":
                        crtInput = "VGA-2";
                        break;

                    case "03":
                        crtInput = "DVI-1";
                        break;

                    case "04":
                        crtInput = "DVI-2";
                        break;

                    case "05":
                        crtInput = "Composite video 1";
                        break;

                    case "06":
                        crtInput = "Composite video 2";
                        break;

                    case "07":
                        crtInput = "S-Video-1";
                        break;

                    case "08":
                        crtInput = "S-Video-2";
                        break;

                    case "09":
                        crtInput = "Tuner-1";
                        break;

                    case "0a":
                        crtInput = "Tuner-2";
                        break;

                    case "0b":
                        crtInput = "Tuner-3";
                        break;

                    case "0c":
                        crtInput = "Component video (YPrPb/YCrCb) 1";
                        break;

                    case "0d":
                        crtInput = "Component video (YPrPb/YCrCb) 2";
                        break;

                    case "0e":
                        crtInput = "Component video (YPrPb/YCrCb) 3";
                        break;

                    case "0f":
                        crtInput = "DisplayPort-1";
                        break;

                    case "10":
                        crtInput = "Mini DisplayPort-1";
                        break;

                    case "11":
                        crtInput = "HDMI-1";
                        break;

                    case "12":
                        crtInput = "HDMI-2";
                        break;

                    case "13":
                        crtInput = "DisplayPort-2";
                        break;

                    case "14":
                        crtInput = "Mini DisplayPort-2";
                        break;

                    case "15":
                        crtInput = "HDMI3";
                        break;

                    case "16":
                        crtInput = "HDMI4";
                        break;

                    case "17":
                        crtInput = "DisplayPort-3";
                        break;

                    case "18":
                        crtInput = "Mini DisplayPort-3";
                        break;

                    case "19":
                        crtInput = "Thunderbolt-1";
                        break;

                    case "1a":
                        crtInput = "Thunderbolt-2";
                        break;

                    case "1b":
                        crtInput = "USB-C1";
                        break;

                    case "1c":
                        crtInput = "USB-C2";
                        break;

                    case "1d":
                        crtInput = "USB-C3";
                        break;

                    case "1e":
                        crtInput = "USB-C4";
                        break;

                    case "80":
                        crtInput = "USB Comm from USB1 (Type-B, port 1)";
                        break;

                    case "81":
                        crtInput = "USB Comm from USB2 (Type-B, port 2)";
                        break;

                    case "82":
                        crtInput = "USB Comm from USB-C1 (Type-C, port 1)";
                        break;

                    case "83":
                        crtInput = "USB Comm from USB-C2 (Type-C, port 2)";
                        break;

                    case "84":
                        crtInput = "USB Comm from USB-C3 (Type-C, port 3)";
                        break;

                    case "85":
                        crtInput = "USB Comm from USB-C4 (Type-C, port 4)";
                        break;

                    default:
                        crtInput = string.Empty;
                        break;
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
                    _hotkeyJobQueue.Enqueue(new JobInfo(1000, hotkeyPopWrap.monitorInfo, null, Reduce_Brightness_Value));
                    break;

                case HotkeyType.BrightnessIncrease:
                    _hotkeyJobQueue.Enqueue(new JobInfo(1000, hotkeyPopWrap.monitorInfo, null, Increase_Brightness_Value));
                    break;

                case HotkeyType.ContrastReduce:
                    _hotkeyJobQueue.Enqueue(new JobInfo(1000, hotkeyPopWrap.monitorInfo, null, Reduce_Contrast_Value));
                    break;

                case HotkeyType.ContrastIncrease:
                    _hotkeyJobQueue.Enqueue(new JobInfo(1000, hotkeyPopWrap.monitorInfo, null, Increase_Contrast_Value));
                    break;
            }
        }

        private void NoEvent(object o, object ob)
        {
            //do nothing
        }

        private void Reduce_Brightness_Value(MonitorInfo monitorInfo, Object[] param)
        {
            if (!IsHotkeyFuncLock(HotkeyType.LockBriCont))
            {
                ObjGetVCP obBrightness = GetVCPCapability(monitorInfo, 0x10, 0).Result;
                if (obBrightness.result)
                {
                    uint brightnessValue = ((uint)obBrightness.value) <= 5 ? 0 : ((uint)obBrightness.value - 5);
                    bool ret = SetVCPCapability(monitorInfo, 0x10, brightnessValue).Result;
                    writelog($"Reduce_Brightness:[{monitorInfo.edid.ModelName}:{monitorInfo.edid.SerialNumber}] from [{(uint)obBrightness.value}] to [{brightnessValue}]" + (ret ? "success" : "fail"));
                }
            }
        }

        private void Increase_Brightness_Value(MonitorInfo monitorInfo, Object[] param)
        {
            if (!IsHotkeyFuncLock(HotkeyType.LockBriCont))
            {
                ObjGetVCP obBrightness = GetVCPCapability(monitorInfo, 0x10, 0).Result;
                if (obBrightness.result)
                {
                    uint brightnessValue = (((uint)obBrightness.value) + 5) >= 100 ? 100 : ((uint)obBrightness.value + 5);
                    bool ret = SetVCPCapability(monitorInfo, 0x10, brightnessValue).Result;
                    writelog($"Increase_Brightness:[{monitorInfo.edid.ModelName}:{monitorInfo.edid.SerialNumber}] from [{(uint)obBrightness.value}] to [{brightnessValue}]" + (ret ? "success" : "fail"));
                }
            }
        }

        private void Reduce_Contrast_Value(MonitorInfo monitorInfo, Object[] param)
        {
            if (!IsHotkeyFuncLock(HotkeyType.LockBriCont))
            {
                ObjGetVCP obContrast = GetVCPCapability(monitorInfo, 0x12, 0).Result;
                if (obContrast.result)
                {
                    uint contrastValue = ((uint)obContrast.value) <= 5 ? 0 : (uint)obContrast.value - 5;
                    bool ret = SetVCPCapability(monitorInfo, 0x12, contrastValue).Result;
                    writelog($"Reduce_Contrast:[{monitorInfo.edid.ModelName}:{monitorInfo.edid.SerialNumber}] from [{(uint)obContrast.value}] to [{contrastValue}]" + (ret ? "success" : "fail"));
                }
            }
        }

        private void Increase_Contrast_Value(MonitorInfo monitorInfo, Object[] param)
        {
            if (!IsHotkeyFuncLock(HotkeyType.LockBriCont))
            {
                ObjGetVCP obContrast = GetVCPCapability(monitorInfo, 0x12, 0).Result;
                if (obContrast.result)
                {
                    uint contrastValue = ((uint)obContrast.value) + 5 >= 100 ? 100 : (uint)obContrast.value + 5;
                    bool ret = SetVCPCapability(monitorInfo, 0x12, contrastValue).Result;
                    writelog($"Increase_Contrast:[{monitorInfo.edid.ModelName}:{monitorInfo.edid.SerialNumber}] from [{(uint)obContrast.value}] to [{contrastValue}]" + (ret ? "success" : "fail"));
                }
            }
        }

        private void Reduce_Luminance_Value(MonitorInfo monitorInfo, Object[] param)
        {
            if (!IsHotkeyFuncLock(HotkeyType.LockBriCont))
            {
                ObjGetVCP obLuminance = GetVCPCapability(monitorInfo, 0x10, 0).Result;
                if (obLuminance.result)
                {
                    uint luminanceValue = ((uint)obLuminance.value) <= 1 ? 0 : (uint)obLuminance.value - 1;
                    bool ret = SetVCPCapability(monitorInfo, 0x10, luminanceValue).Result;
                    writelog($"Reduce_Luminance:[{monitorInfo.edid.ModelName}:{monitorInfo.edid.SerialNumber}] from [{(uint)obLuminance.value}] to [{luminanceValue}]" + (ret ? "success" : "fail"));
                }
            }
        }

        private void Increase_Luminance_Value(MonitorInfo monitorInfo, Object[] param)
        {
            if (!IsHotkeyFuncLock(HotkeyType.LockBriCont))
            {
                ObjGetVCP obLuminance = GetVCPCapability(monitorInfo, 0x10, 0).Result;
                ObjGetVCP obLuminanceMax = GetVCPCapability(monitorInfo, 0x10, 1).Result;
                if (obLuminance.result && obLuminanceMax.result)
                {
                    uint luminanceValue = ((uint)obLuminance.value) + 1 >= (uint)obLuminanceMax.value ? (uint)obLuminanceMax.value : (uint)obLuminance.value + 1;
                    bool ret = SetVCPCapability(monitorInfo, 0x10, luminanceValue).Result;
                    writelog($"Increase_Luminance:[{monitorInfo.edid.ModelName}:{monitorInfo.edid.SerialNumber}] from [{(uint)obLuminance.value}] to [{luminanceValue}]" + (ret ? "success" : "fail"));
                }
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
                    writelog($"powerNap screenSaver Status {screenSaverStatus}");
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
                                        allJobs.Add(new JobInfo(1000, monitorInfo, new object[] { true }, PowerNapReduceBrightness));
                                        //_powerNapJobQueue.Enqueue(new JobInfo(monitorInfo, new object[] { true }, PowerNapReduceBrightness));
                                        Debug.WriteLine($"{setting.ModelName}:{setting.SerialNumber} ReduceBrightness - Enqueue:true");
                                        writelog($"powerNap [{setting.ModelName}:{setting.SerialNumber}] ReduceBrightness - Enqueue:true");
                                        break;

                                    case PowerNapType.SleepIfRunning:
                                        allJobs.Add(new JobInfo(1000, monitorInfo, new object[] { true }, PowerNapSuspendMonitor));
                                        //_powerNapJobQueue.Enqueue(new JobInfo(monitorInfo, new object[] { true }, PowerNapSuspendMonitor));
                                        Debug.WriteLine($"{setting.ModelName}:{setting.SerialNumber} SleepIfRunning - Enqueue:true");
                                        writelog($"powerNap [{setting.ModelName}:{setting.SerialNumber}] SleepIfRunning - Enqueue:true");
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
                    writelog($"powerNap screenSaver Status {screenSaverStatus}");
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
                                        allJobs.Add(new JobInfo(1000, monitorInfo, new object[] { false }, PowerNapReduceBrightness));
                                        //_powerNapJobQueue.Enqueue(new JobInfo(monitorInfo, new object[] { false }, PowerNapReduceBrightness));
                                        Debug.WriteLine($"{setting.ModelName}:{setting.SerialNumber} ReduceBrightness - Enqueue:false");
                                        writelog($"powerNap [{setting.ModelName}:{setting.SerialNumber}] ReduceBrightness - Enqueue:false");
                                        break;

                                    case PowerNapType.SleepIfRunning:
                                        allJobs.Add(new JobInfo(1000, monitorInfo, new object[] { false }, PowerNapSuspendMonitor));
                                        //_powerNapJobQueue.Enqueue(new JobInfo(monitorInfo, new object[] { false }, PowerNapSuspendMonitor));
                                        Debug.WriteLine($"{setting.ModelName}:{setting.SerialNumber} SleepIfRunning - Enqueue:false");
                                        writelog($"powerNap [{setting.ModelName}:{setting.SerialNumber}] SleepIfRunning - Enqueue:false");
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
            ObjGetVCP rc = new ObjGetVCP();
            string capability = monitorInfo.CapabilityString;
            if (capability.Contains("E0("))
            {
                string[] ss = capability.Split("E0(");
                ss = ss[1].Split(")");
                ss = ss[0].Split(" ");
                if (ss[0] == "03" || ss[0] == "0F")
                {
                    rc = GetVCPCapability(monitorInfo, 0xE0).Result;
                    int getvalue = (Convert.ToInt32(rc.value) & 0x0c);
                    if (cs)
                    {
                        bool ret = SetVCPCapability(monitorInfo, 0xE0, (1 | (uint)getvalue)).Result;
                        writelog($"PowerNap ReduceBrightness:[{monitorInfo.edid.ModelName}:{monitorInfo.edid.SerialNumber}] ON and setVcp:]" + (ret ? "success" : "fail"));
                    }
                    else
                    {
                        bool ret = SetVCPCapability(monitorInfo, 0xE0, (0 | (uint)getvalue)).Result;
                        writelog($"PowerNap ReduceBrightness:[{monitorInfo.edid.ModelName}:{monitorInfo.edid.SerialNumber}] OFF and setVcp:]" + (ret ? "success" : "fail"));
                    }
                }
            }
            else
            {
                if (cs)
                {
                    bool ret = SetVCPCapability(monitorInfo, 0xE0, 1).Result;
                    writelog($"PowerNap ReduceBrightness:[{monitorInfo.edid.ModelName}:{monitorInfo.edid.SerialNumber}] ON and setVcp:]" + (ret ? "success" : "fail"));
                }
                else
                {
                    bool ret = SetVCPCapability(monitorInfo, 0xE0, 0).Result;
                    writelog($"PowerNap ReduceBrightness:[{monitorInfo.edid.ModelName}:{monitorInfo.edid.SerialNumber}] OFF and setVcp:]" + (ret ? "success" : "fail"));
                }
            }
        }

        public void PowerNapSuspendMonitor(MonitorInfo monitorInfo, Object[] param)
        {
            //SuspendMonitor
            //SetVCPCapability(monitorInfo, 0xE1, 1);
            bool cs = (bool)param[0];
            ObjGetVCP rc = new ObjGetVCP();
            string capability = monitorInfo.CapabilityString;
            if (capability.Contains("E0("))
            {
                string[] ss = capability.Split("E0(");
                ss = ss[1].Split(")");
                ss = ss[0].Split(" ");
                if (ss[0] == "03" || ss[0] == "0F")
                {
                    rc = GetVCPCapability(monitorInfo, 0xE0).Result;
                    int getvalue = (Convert.ToInt32(rc.value) & 0x0c);
                    if (cs)
                    {
                        bool ret = SetVCPCapability(monitorInfo, 0xE0, (2 | (uint)getvalue)).Result;
                        writelog($"PowerNap SuspendMonitor:[{monitorInfo.edid.ModelName}:{monitorInfo.edid.SerialNumber}] ON and setVcp:]" + (ret ? "success" : "fail"));
                    }
                    else
                    {
                        bool ret = SetVCPCapability(monitorInfo, 0xE0, (0 | (uint)getvalue)).Result;
                        writelog($"PowerNap SuspendMonitor:[{monitorInfo.edid.ModelName}:{monitorInfo.edid.SerialNumber}] OFF and setVcp:]" + (ret ? "success" : "fail"));
                    }
                }
            }
            else
            {
                if (cs)
                {
                    bool ret = SetVCPCapability(monitorInfo, 0xE1, 1).Result;
                    writelog($"PowerNap SuspendMonitor:[{monitorInfo.edid.ModelName}:{monitorInfo.edid.SerialNumber}] ON and setVcp:]" + (ret ? "success" : "fail"));
                }
                else
                {
                    bool ret = SetVCPCapability(monitorInfo, 0xE1, 0).Result;
                    writelog($"PowerNap SuspendMonitor:[{monitorInfo.edid.ModelName}:{monitorInfo.edid.SerialNumber}] OFF and setVcp:]" + (ret ? "success" : "fail"));
                }
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
                if (saveList.Any(x => x.SerialNumber.Equals(setting.SerialNumber)))
                    continue;
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

            //Telementry Collection
            var rt = false;
            var Displaysettings_Function = new Displaysettings_Function();
            MonitorInfo monitorInfo = _AllInfoMonitors.SingleOrDefault(x => x.edid.SerialNumber.Equals(powerNapSetting.SerialNumber));
            if (monitorInfo != null)
            {
                if (powerNapSetting.Status)
                {
                    string powerNapTelementryData = string.Empty;
                    switch (powerNapSetting.RunType)
                    {
                        case PowerNapType.Off:
                            powerNapTelementryData = "Off";
                            break;
                        case PowerNapType.ReduceBrightness:
                            powerNapTelementryData = "Reduce_brightness";
                            break;
                        case PowerNapType.SleepIfRunning:
                            powerNapTelementryData = "Sleep";
                            break;
                    }
                    Debug.WriteLine($"powerNapTelementry:{monitorInfo.edid.SerialNumber}=> {powerNapTelementryData}");
                    writelog("[DeviceMangerPlugin] [Telementry] Send Telementry for PowerNap...");
                    rt = Displaysettings_Function.Send_PowerNap_Telementry(_TelementryScheduler, monitorInfo, powerNapTelementryData, GetMonitorCurrentResolution(monitorInfo), GetMonitorMaxResolution(monitorInfo));
                    if (rt) writelog("[DeviceMangerPlugin] [Telementry] Send Telementry for PowerNap Success ...");
                    else writelog("[DeviceMangerPlugin] [Telementry] Send Telementry for PowerNap Fail ...");
                }
            }
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

        private void DDMtoDDPM_Input(DDMMonitorSettings DDMmonitorsettings)
        {
            if (DDMmonitorsettings != null)
            {
                Dictionary<string, InputInfo> DDMinputlist = new Dictionary<string, InputInfo>();
                string model = DDMmonitorsettings.Model;
                string serviceTag = DDMmonitorsettings.ServiceTag;
                Input input = DDMmonitorsettings.Input;
                if (input.FriendlyNames != null)
                {
                    if (input.FriendlyNames.Count != 0)
                    {
                        foreach (FriendlyName friendlyName in input.FriendlyNames)
                        {
                            foreach (var vcpcode in VcpCodeList.VCP60)
                            {
                                InputInfo inputInfo = new InputInfo();
                                if (vcpcode.Value == (uint)friendlyName.Input)
                                {
                                    inputInfo.InputName = friendlyName.Name;
                                    inputInfo.Code = vcpcode.Value;
                                    inputInfo.USBUpstream = string.Empty;
                                    DDMinputlist.Add(vcpcode.Key, inputInfo);
                                    break;
                                }
                            }
                        }
                        string strDDMinputlist = InputSourceListSerialize(DDMinputlist);
                        if (_SettingsPlugin != null)
                        {
                            List<DDPMMonitorSettings> ddpmMonitorSettings = _SettingsPlugin.ReloadMonitorSettings(model).Result;
                            if (ddpmMonitorSettings != null)
                            {
                                int index = ddpmMonitorSettings.FindIndex(x => x.ServiceTag == serviceTag);
                                if (index != -1)
                                {
                                    if (ddpmMonitorSettings[index].Input != null)
                                    {
                                        ddpmMonitorSettings[index].Input.strInputSourceList = strDDMinputlist;
                                    }
                                    else
                                    {
                                        InputSource inputSource = new InputSource();
                                        inputSource.strInputSourceList = strDDMinputlist;
                                        ddpmMonitorSettings[index].Input = inputSource;
                                    }
                                }
                                else
                                {
                                    DDPMMonitorSettings monitorSettings = new DDPMMonitorSettings();
                                    monitorSettings.Input.strInputSourceList = strDDMinputlist;
                                }

                                bool b = _SettingsPlugin.WriteMonitorSettings(model, ddpmMonitorSettings).Result;
                            }
                        }
                        else
                        {
                            writelog("[DDMtoDDPM_Input]ddpmMonitorSettings is null!");
                        }
                    }
                    else
                    {
                        writelog("[DDMtoDDPM_Input]FriendlyNames count is 0...");
                    }
                }
                else
                {
                    writelog("[DDMtoDDPM_Input]FriendlyNames is null!");
                }
            }
            else
            {
                writelog("[DDMtoDDPM_Input]DDMmonitorsettings is null!");
            }
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

        public Task<(HotkeySettings, List<HotkeyData>)> ReadCurrentHotkey(MonitorInfo mo)//EDID monitorEdid)
        {
            List<HotkeySettings> read = _SettingsPlugin.ReadHotkeySettings().Result;
            //HotkeySettings hotkeySettings = read.Where(x => x.ModelName.Equals(monitorEdid.ModelName) && x.SerialNumber.Equals(monitorEdid.SerialNumber)).SingleOrDefault();
            HotkeySettings hotkeySettings = read.Where(x => x.ModelName.Equals("DDPM") && x.SerialNumber.Equals("DDPM")).SingleOrDefault();

            //1006 read hotkey data per monitor
            List<HotkeyData> list = GetInputSourceHotKeyData(mo);

            if (hotkeySettings != null && hotkeySettings.HotkeyInfo.Count > 0)
            {
                return Task.FromResult((hotkeySettings, list));
            }
            return Task.FromResult((new HotkeySettings(), list));
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
            if (mos == null)
                return null;
            if (mos.Count == 0)
                return mos;

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
            if (_NKVMPlugin != null && hotkeySettings != null)
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

        private void NKVMCLIEvent(object sender, NKVMRespone e)
        {
            SendCLINKVMRespone(e);
        }

        private void NKVMSetHotkey(object sender, NKVMSetHotkey e)
        {
            SetNKVMHotkey(e);
        }

        private void SendCLINKVMRespone(NKVMRespone e)
        {
            EventHandler<NKVMRespone> handler = NKVMCLIRespone;
            if (handler != null)
            {
                handler.AsyncFireAndForget(this, e, System.Threading.CancellationToken.None);
            }
        }

        private void SetNKVMHotkey(NKVMSetHotkey e)
        {
            writelog("[SetNKVMHotkey] SetNKVMHotkey");
            bool b = false;
            if (_AllInfoMonitors == null)
            {
                _AllInfoMonitors = GetMonitors().Result;
            }
            else
            {
                if (_AllInfoMonitors.Count == 0)
                {
                    _AllInfoMonitors = GetMonitors().Result;
                }
            }
            if (_AllInfoMonitors != null)
            {
                if (_AllInfoMonitors.Count > 0)
                {
                    b = SaveHotkeySetting(_AllInfoMonitors[0], e.HotkeyInfo).Result;
                    if (b)
                    {
                        writelog("[SetNKVMHotkey] SetNKVMHotkey is success");
                    }
                    else
                    {
                        writelog("[SetNKVMHotkey] SetNKVMHotkey is fail");
                    }
                }
                else
                {
                    writelog("[SetNKVMHotkey] _AllInfoMonitors count is 0");
                }
            }
            else
            {
                writelog("[SetNKVMHotkey] _AllInfoMonitors is null");
            }
            if (_NKVMPlugin != null)
            {
                _NKVMPlugin.SetHotkeyResponse(e.jsonstring, b);
            }
            else
            {
                writelog("[SetNKVMHotkey] _NKVMPlugin is null");
            }
        }

        #endregion

        #region Settings

        private List<VCPCode> GetAllVCPcode(MonitorInfo monitorInfo)
        {
            List<VCPCode> vcps = new List<VCPCode>();
            foreach (string key in monitorInfo.CapabilityDic.Keys)
            {
                VCPCode vcp = new VCPCode(Int32.Parse(key, System.Globalization.NumberStyles.HexNumber), null);
                vcps.Add(vcp);
            }
            return vcps;
        }

        private void SetVCPSequence(MonitorInfo monitorInfo, ImpVCPSequence impVCPSequence, List<VCPCode> vcps)
        {
            if (impVCPSequence != null)
            {
                if (vcps.Count != 0)
                {
                    foreach (var item in vcps)
                    {
                        Trace.WriteLine($"Code: {item.Code}, Value:{item.Value}");
                    }

                    ImportVCP importVCP = new ImportVCP();
                    foreach (int code in importVCP.ImportVCPSequence)
                    {
                        if (vcps.Exists(x => x.Code == code))
                        {
                            VCPCode vcp = vcps.Find(x => x.Code == code);
                            writelog("[SetVCPSequence] VCP code : " + vcp.Code.ToString());
                            ObjGetVCP objGetVCP = new ObjGetVCP();
                            objGetVCP = GetVCPCapability(monitorInfo, (byte)vcp.Code).Result;
                            if (objGetVCP.result && (int)(uint)objGetVCP.value != (int)vcp.Value[0])
                            {
                                writelog("[SetVCPSequence] Set VCP code : " + vcp.Code.ToString());
                                if (code == 0x66)
                                {
                                    writelog("[SetVCPSequence] ALS");
                                    bool b = SetVCPCapability(monitorInfo, 0x66, impVCPSequence.ALSConfig).Result;
                                }
                                else
                                {
                                    bool b = SetVCPCapability(monitorInfo, (byte)code, (uint)vcp.Value[0]).Result;
                                }
                            }
                        }
                        else
                        {
                            writelog($"[SetVCPSequence] Code:{code} cannot find in vcps");
                        }
                    }
                }
                else
                {
                    writelog("[SetVCPSequence] vcps count = 0");
                }
            }
            else
            {
                writelog("[SetVCPSequence] ImpExpSettings is null");
            }
        }

        private void InitMonitorSettings()
        {
            List<DDPMMonitorSettings> monitorSettingsList = new List<DDPMMonitorSettings>();
            if (_AllInfoMonitors != null)
            {
                foreach (MonitorInfo m in _AllInfoMonitors.ToList())
                {
                    monitorSettingsList = _SettingsPlugin.InitDDPMMonitorConfigFile(m.modelName, out isInitMonitorSettings).Result;
                    if (isInitMonitorSettings)
                    {
                        if (monitorSettingsList == null)
                        {
                            monitorSettingsList = new List<DDPMMonitorSettings>();
                        }
                        if (monitorSettingsList.Count == 0 || !monitorSettingsList.Exists(x => x.ServiceTag == m.edid.ServiceTag))
                        {
                            DDPMMonitorSettings settings = new DDPMMonitorSettings();
                            settings.Model = m.modelName;
                            settings.ServiceTag = m.edid.ServiceTag;
                            settings.VCPs = GetAllVCPcode(m);
                            settings.DisplayPropertiesInfo = new DisplayCurrentPropertiesInfo();
                            settings.EA = new EAMonitorSettings();
                            settings.easyArrangementDDPM = new EasyArrangementDDPM();
                            settings.ImpExpSettings = new ImpExpSettings();
                            settings.hotkeyData = new List<HotkeyData>();
                            settings.scheduleInfo = new scheduleInfo();
                            settings.ALSConfig = 0;
                            monitorSettingsList.Add(settings);
                            bool b = _SettingsPlugin.WriteMonitorSettings(m.modelName, monitorSettingsList).Result;
                        }
                    }
                }
            }
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
                    ToastNotificationManagerCompat.OnActivated -= CheckInput;//Bruce 0924 add Popup Event
                    //displayChange.DisplayChange_Event -= SystemEvents_DisplaySettingsChanged;
                    if (_SettingsPlugin != null)
                        _SettingsPlugin.ITSettingsActionEvent -= _SettingsPlugin_ITSettingsActionEvent;
                }

                IsDisposed = true;
            }
            base.Dispose(disposing);
        }

        #endregion

        #region Migration

        public Task<bool> DDMtoDDPM_EzMemory(DDMMonitorSettings dDMMonitorSettings, DDMUserSettings dDMUserSettings)
        {
            bool result = false;
            try
            {
                //DDMUserSettings
                if (dDMUserSettings.Profiles.Count != 0)
                {
                    List<EAProfileDDPM> userEAProfileDDPMList = ReadUserEAProfileDDPM().Result;

                    if (userEAProfileDDPMList == null)
                        userEAProfileDDPMList = new List<EAProfileDDPM>();

                    foreach (var dDMuserProfile in dDMUserSettings.Profiles)
                    {
                        EAProfileDDPM eaProfileDDPM = new EAProfileDDPM(dDMuserProfile.ID, dDMuserProfile.Name, dDMuserProfile.Layout, dDMuserProfile.AppInfos.ConvertAll
                                              (app => new EAAppInfoDDPM(app.Name, app.Path, app.IsUWP, app.AppUserModelID, app.Param)));

                        // 將更新後的 currentProfile 寫入
                        result = WriteUserEAProfileDDPM(null, eaProfileDDPM).Result;
                    }
                    if (result)
                        writelog($"@ DDMtoDDPM_EzMemory: UserSettings PASS");
                    else
                        writelog($"@ DDMtoDDPM_EzMemory: UserSettings Fail");
                }

                //DDMMonitorSettings
                if (dDMMonitorSettings != null && dDMMonitorSettings.EasyArrangement != null)
                {
                    MonitorInfo moinfo = new MonitorInfo();
                    EDID edid = new EDID();
                    moinfo.edid = edid;
                    moinfo.modelName = dDMMonitorSettings.Model;
                    moinfo.edid.ModelName = dDMMonitorSettings.Model;
                    moinfo.edid.ServiceTag = dDMMonitorSettings.ServiceTag;

                    EasyArrangementDDPM easyArrangementDDPM = new EasyArrangementDDPM();
                    easyArrangementDDPM.Desktops = new List<DesktopDDPM>();

                    // 新增 DesktopDDPM 物件
                    easyArrangementDDPM.Desktops.Add(new DesktopDDPM(dDMMonitorSettings.EasyArrangement.Desktops[0].ID, dDMMonitorSettings.EasyArrangement.Desktops[0].ActiveLayout));

                    // 設定 DesktopDDPM 的屬性
                    easyArrangementDDPM.Desktops[0].ID = dDMMonitorSettings.EasyArrangement.Desktops[0].ID;
                    easyArrangementDDPM.Desktops[0].Index = dDMMonitorSettings.EasyArrangement.Desktops[0].Index;
                    easyArrangementDDPM.Desktops[0].ActiveLayout = dDMMonitorSettings.EasyArrangement.Desktops[0].ActiveLayout;

                    List<int> layoutMRU = new List<int>();
                    layoutMRU = dDMMonitorSettings.EasyArrangement.Desktops[0].LayoutMRU;
                    easyArrangementDDPM.Desktops[0].LayoutMRU = layoutMRU;

                    List<int> profileMRU = new List<int>();
                    profileMRU = dDMMonitorSettings.EasyArrangement.Desktops[0].ProfileMRU;
                    easyArrangementDDPM.Desktops[0].ProfileMRU = profileMRU;

                    // Profiles
                    easyArrangementDDPM.Desktops[0].Profiles = new List<EzProfileDDPM>();
                    foreach (var profile in dDMMonitorSettings.EasyArrangement.Desktops[0].Profiles)
                    {
                        var newProfile = new EzProfileDDPM(
                            profile.ID,
                            profile.Name,
                            profile.Layout,
                            profile.Auto,
                            profile.AutoStartTime ?? 0,
                            profile.StartUpLaunch,
                            new List<EAAppInfoDDPM>()
                        );

                        // AppInfos
                        foreach (var app in profile.AppInfos)
                        {
                            var newAppInfo = new EAAppInfoDDPM(
                                app.Name,
                                app.Path,
                                app.IsUWP,
                                app.AppUserModelID,
                                app.Param
                            );
                            newProfile.AppInfos.Add(newAppInfo);
                        }

                        easyArrangementDDPM.Desktops[0].Profiles.Add(newProfile);
                    }

                    // ProfileSettings
                    easyArrangementDDPM.Desktops[0].ProfileSettings = new List<EzProfileSettingDDPM>();
                    foreach (var setting in dDMMonitorSettings.EasyArrangement.Desktops[0].ProfileSettings)
                    {
                        var newSetting = new EzProfileSettingDDPM(
                            setting.ID,
                            setting.Auto,
                            setting.AutoStartTime ?? 0,
                            setting.StartUpLaunch
                        );

                        easyArrangementDDPM.Desktops[0].ProfileSettings.Add(newSetting);
                    }
                    result = WriteMonitorEasyArrangement(moinfo, easyArrangementDDPM).Result;
                }
                if (result)
                    writelog($"@ DDMtoDDPM_EzMemory: MonitorSettings PASS");
                else
                    writelog($"@ DDMtoDDPM_EzMemory: MonitorSettings Fail");
            }
            catch (Exception ex)
            {
                writelog($"@ DDMtoDDPM_EzMemory: {ex.Message}");
            }
            return Task.FromResult(result);
        }

        private void DDMMigration()
        {
            if (_SettingsPlugin != null)
            {
                string migration = string.Empty;
                if (_SettingsPlugin.isDDMMigration(out migration).Result)
                {
                    DDMUserSettings ddmUserSettings = new DDMUserSettings();
                    string migrationPath = migration + "\\" + "UserFoler";
                    DirectoryInfo di = new DirectoryInfo(migrationPath);
                    if (_SettingsPlugin.ReadDDMUserSettings(migrationPath + "\\UserSettings", ref ddmUserSettings).Result)
                    {
                        //Hotkey
                        //DDMtoDDPM_Hotkey(ddmUserSettings);
                        foreach (var file in di.GetFiles("*_*"))
                        {
                            DDMMonitorSettings DDMmonitorsettings = new DDMMonitorSettings();
                            string path = migrationPath + "\\" + file.Name;
                            if (_SettingsPlugin.ReadDDMMonitorSettings(path, ref DDMmonitorsettings).Result)
                            {
                                if (DDMmonitorsettings.ServiceTag != string.Empty)
                                {
                                    //add settings file in DDPM
                                    bool binit = false;
                                    List<DDPMMonitorSettings> ddpmMonitorSettings = new List<DDPMMonitorSettings>();
                                    ddpmMonitorSettings = _SettingsPlugin.ReloadMonitorSettings(DDMmonitorsettings.Model).Result;
                                    if (ddpmMonitorSettings == null)
                                    {
                                        ddpmMonitorSettings = _SettingsPlugin.InitDDPMMonitorConfigFile(DDMmonitorsettings.Model, out binit).Result;
                                    }
                                    else
                                    {
                                        if (ddpmMonitorSettings.Count == 0)
                                        {
                                            ddpmMonitorSettings = _SettingsPlugin.InitDDPMMonitorConfigFile(DDMmonitorsettings.Model, out binit).Result;
                                        }
                                        else
                                        {
                                            binit = true;
                                        }
                                    }
                                    if (binit)
                                    {
                                        if (ddpmMonitorSettings == null)
                                        {
                                            ddpmMonitorSettings = new List<DDPMMonitorSettings>();
                                            DDPMMonitorSettings settings = new DDPMMonitorSettings();
                                            settings.Model = DDMmonitorsettings.Model;
                                            settings.ServiceTag = DDMmonitorsettings.ServiceTag;
                                            ddpmMonitorSettings.Add(settings);
                                            bool b = _SettingsPlugin.WriteMonitorSettings(DDMmonitorsettings.Model, ddpmMonitorSettings).Result;
                                        }
                                        else
                                        {
                                            if (!ddpmMonitorSettings.Exists(x => (x.ServiceTag == DDMmonitorsettings.ServiceTag)))
                                            {
                                                DDPMMonitorSettings settings = new DDPMMonitorSettings();
                                                settings.Model = DDMmonitorsettings.Model;
                                                settings.ServiceTag = DDMmonitorsettings.ServiceTag;
                                                ddpmMonitorSettings.Add(settings);
                                                bool b = _SettingsPlugin.WriteMonitorSettings(DDMmonitorsettings.Model, ddpmMonitorSettings).Result;
                                            }
                                        }
                                        //DDM settings -> DDPM settings
                                        ImportDDMMonitorSettings(DDMmonitorsettings, ddmUserSettings);
                                    }
                                    else
                                    {
                                        writelog("[DDMMigration] InitDDPMMonitorConfigFile fail");
                                    }
                                }
                            }
                            else
                            {
                                writelog($"[DDMMigration] read DDM MonitorSettings file fail: {path}");
                            }
                        }
                        if (CopyFile(migrationPath, migration + "\\" + "CopyMigrationFile"))
                        {
                            Directory.Delete(migrationPath, true);
                        }
                    }
                    else
                    {
                        writelog($"[DDMMigration] read DDM UserSettings file fail: {migrationPath + "\\UserSettings"}");
                    }
                }
                else
                {
                    writelog("[DDMMigration] not find Migration folder.");
                }
            }
        }

        private void ImportDDMMonitorSettings(DDMMonitorSettings DDMmonitorsettings, DDMUserSettings DDMusersettings)
        {
            //Input
            DDMtoDDPM_Input(DDMmonitorsettings);
            //Color
            if (_ColorPresetPlugin != null)
            {
                _ColorPresetPlugin.Migration(DDMmonitorsettings.ColorPreset, DDMmonitorsettings.Model, DDMmonitorsettings.ServiceTag, _SettingsPlugin);
            }
            //EA
            DDMtoDDPM_EzArrange(DDMmonitorsettings, DDMusersettings);
            //EM
            DDMtoDDPM_EzMemory(DDMmonitorsettings, DDMusersettings);
            //Schedule
            bool bSchedule = MigrateScheduleMonitorSettings(DDMmonitorsettings.Model, DDMmonitorsettings.ServiceTag, DDMmonitorsettings.BriConSchedule).Result;
            //Hotkey
            DDMtoDDPM_Hotkey(DDMusersettings, DDMmonitorsettings);
        }

        private void DDMtoDDPM_Hotkey(DDMUserSettings ddmUserSettings, DDMMonitorSettings ddmMonitorSettings)
        {
            try
            {
                if (_SettingsPlugin != null)
                {
                    List<HotkeySettings> hotkeySettingList = _SettingsPlugin.ReadHotkeySettings().Result;
                    if (ddmUserSettings != null)
                    {
                        foreach (var Hotkey in ddmUserSettings.Hotkeys)
                        {
                            if (Hotkey.Keys != null)
                            {
                                if (Hotkey.Keys.Count != 0)
                                {
                                    DDMtoDDPM dDMtodDPM = new DDMtoDDPM();
                                    if (dDMtodDPM.HotkeyMap.TryGetValue(Hotkey.Function, out HotkeyType hotkeyType))
                                    {
                                        writelog($"[DDMtoDDPM_Hotkey] Fun is {Hotkey.Function}");
                                        HotkeySettings hotkeySettings = new HotkeySettings();
                                        HotkeyInfo hotkeyInfo = new HotkeyInfo();
                                        hotkeySettings.ServiceTag = "DDPM";
                                        hotkeySettings.SerialNumber = "DDPM";
                                        hotkeySettings.ModelName = "DDPM";
                                        hotkeyInfo = new HotkeyInfo();
                                        hotkeyInfo.Job = hotkeyType;
                                        hotkeyInfo.Hotkey = new List<VirtualKey>();
                                        if (hotkeyInfo.Hotkey != null)
                                        {
                                            foreach (int key in Hotkey.Keys)
                                            {
                                                writelog($"[DDMtoDDPM_Hotkey] Key is {key}");
                                                if (key == -1)
                                                {
                                                    continue;
                                                }
                                                else if (key == 262144)
                                                {
                                                    hotkeyInfo.Hotkey.Add(VirtualKey.Menu);
                                                }
                                                else if (key == 131072)
                                                {
                                                    hotkeyInfo.Hotkey.Add(VirtualKey.Control);
                                                }
                                                else if (key == 65536)
                                                {
                                                    hotkeyInfo.Hotkey.Add(VirtualKey.Shift);
                                                }
                                                else
                                                {
                                                    VirtualKey Vkey = (VirtualKey)key;
                                                    hotkeyInfo.Hotkey.Add(Vkey);
                                                }
                                            }
                                            if (hotkeyInfo.Hotkey.Count != 0)
                                            {
                                                hotkeySettings.HotkeyInfo.Add(hotkeyInfo);
                                            }
                                        }
                                        hotkeySettingList.Add(hotkeySettings);
                                        bool b = _SettingsPlugin.WriteHotkeySettings(hotkeySettingList).Result;
                                    }
                                }
                            }
                        }
                    }
                    if (ddmMonitorSettings != null)
                    {
                        List<DDPMMonitorSettings> monitorSettingList = new List<DDPMMonitorSettings>();
                        monitorSettingList = _SettingsPlugin.ReloadMonitorSettings(ddmMonitorSettings.Model).Result;
                        if (monitorSettingList != null)
                        {
                            DDPMMonitorSettings monitorSettings = monitorSettingList.Find(x => (x.ServiceTag == ddmMonitorSettings.ServiceTag));
                            if (monitorSettings != null)
                            {
                                List<HotkeyData> hotkeyDataList = new List<HotkeyData>();
                                hotkeyDataList = monitorSettings.hotkeyData;
                                if (hotkeyDataList != null)
                                {
                                    if (ddmMonitorSettings.Input.FavoriteHotkeyInput != 0)
                                    {
                                        HotkeyData hotkeyData = new HotkeyData();
                                        hotkeyData.hotkeyType = HotkeyType.FavoriteInputSource;
                                        List<InputSourceObj> inputSourceObjs = new List<InputSourceObj>();
                                        InputSourceObj inputSourceObj = new InputSourceObj();
                                        inputSourceObj.Name = string.Empty;
                                        inputSourceObj.Code = (UInt16)ddmMonitorSettings.Input.FavoriteHotkeyInput;
                                        inputSourceObjs.Add(inputSourceObj);
                                        hotkeyData.inputSource = inputSourceObjs;
                                        monitorSettings.hotkeyData.Add(hotkeyData);
                                    }
                                    if (ddmMonitorSettings.Input.Toogle2InputHotkeysInfo.Count != 0)
                                    {
                                        HotkeyData hotkeyData = new HotkeyData();
                                        hotkeyData.hotkeyType = HotkeyType.SwitchInputSource;
                                        List<InputSourceObj> inputSourceObjs = new List<InputSourceObj>();
                                        foreach (int input in ddmMonitorSettings.Input.Toogle2InputHotkeysInfo)
                                        {
                                            InputSourceObj inputSourceObj = new InputSourceObj();
                                            inputSourceObj.Name = string.Empty;
                                            inputSourceObj.Code = (UInt16)input;
                                            inputSourceObjs.Add(inputSourceObj);
                                        }
                                        hotkeyData.inputSource = inputSourceObjs;
                                        monitorSettings.hotkeyData.Add(hotkeyData);
                                    }
                                    bool bh = _SettingsPlugin.WriteMonitorSettings(ddmMonitorSettings.Model, monitorSettingList).Result;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ;
            }
        }

        private DisplayCurrentPropertiesInfo DDMtoDDPM_DisplayProperties(DDMMonitorSettings DDMmonitorsettings)
        {
            DisplayCurrentPropertiesInfo ret = null;
            try
            {
                ret = new DisplayCurrentPropertiesInfo();
                ret.CurrentProperties.Resolutions_Width = DDMmonitorsettings.Display.devmode.dmPelsWidth;
                ret.CurrentProperties.Resolutions_High = DDMmonitorsettings.Display.devmode.dmPelsHeight;
                ret.CurrentProperties.Frequency = DDMmonitorsettings.Display.devmode.dmDisplayFrequency;
                ret.CurrentOrientation = (DisplayOrientation)(DDMmonitorsettings.Display.orientation - 1);
                ret.isHDREnable = DDMmonitorsettings.Display.SmartHDR;
            }
            catch
            {
                ret = null;
            }
            return ret;
        }

        private bool Import_DisplayProperties(MonitorInfo monitorInfo, DisplayCurrentPropertiesInfo displayCurrentPropertiesInfo)
        {
            bool ret = false;
            try
            {
                //MonitorInfo monitorInfo = new MonitorInfo();
                ret = SetDisplayPropertiest(monitorInfo, displayCurrentPropertiesInfo.CurrentProperties, displayCurrentPropertiesInfo.CurrentOrientation).Result;
                ret = SetHDRStatus(monitorInfo, displayCurrentPropertiesInfo.isHDREnable).Result && ret;
                if (displayCurrentPropertiesInfo.USBCPrioritizationType != USBCPrioritizationType.Unknow)
                {
                    ret = SetUSBCPrioritizationType(monitorInfo, displayCurrentPropertiesInfo.USBCPrioritizationType).Result && ret;
                }
                ret = true;
            }
            catch
            {
                ret = false;
            }
            return ret;
        }

        private DisplayCurrentPropertiesInfo Export_DisplayProperties(MonitorInfo monitorInfo)
        {
            DisplayCurrentPropertiesInfo ret = null;
            try
            {
                if (_DisplayManagerPlugin != null)
                {
                    ret = _DisplayManagerPlugin.GetCurrentDisplayProperties(monitorInfo).Result;
                }
            }
            catch
            {
                ret = null;
            }
            return ret;
        }

        private bool CopyFile(string copyPath, string savePath)
        {
            writelog($"{nameof(CopyFile)} start");
            bool ret = false;
            if (_DisplayManagerPlugin != null)
            {
                // 確保資料夾存在
                if (!Directory.Exists(savePath))
                {
                    Directory.CreateDirectory(savePath);
                }

                if (DirectoryContainsFiles(copyPath))
                {
                    // 取得資料夾名稱
                    string folderName = GetFolderName(copyPath);
                    // 複製指定的 log 文件到選擇的資料夾
                    CopyLogFolder(copyPath, savePath);
                    writelog($"{nameof(CopyFile)} end");
                    return true;
                }
            }
            writelog($"{nameof(CopyFile)} end");
            return false;
        }

        //Robert_Lin, 2024-10-11, added
        private void DDMtoDDPM_EzArrange(DDMMonitorSettings ddmMonitorSettings, DDMUserSettings ddmUserSettings)
        {
            if (_SettingsPlugin == null)
                return;

            //Migration UserSettings
            //
            //1 Convert DDMUserSettings.CustLayouts to ddpmCustomList
            //
            List<SplitJson> ddpmCustomList = new List<SplitJson>();
            if (ddmUserSettings.CustLayouts != null)
            {
                foreach (CustLayout custLayout in ddmUserSettings.CustLayouts)
                {
                    SplitJson spJson = new SplitJson();
                    spJson.CellCount = 0;
                    spJson.SplitKey = 'B';

                    List<double> settings = new List<double>();
                    //Format: settings[0] : BorderCount
                    settings.Add((double)custLayout.Rects.Count);
                    //settings[1] : screenScale
                    settings.Add((double)1.000);
                    //settings[2] : screenWidth
                    settings.Add((double)1.000);
                    //settings[3] : screenHeight
                    settings.Add((double)1.000);

                    //Settings[4 ~] : Rects
                    int idxRect = 0;
                    foreach (EARect eARect in custLayout.Rects)
                    {
                        //settings[4 + idxRect + 0] : left
                        settings.Add(eARect.x);
                        //settings[4 + idxRect + 1] : top
                        settings.Add(eARect.y);
                        //settings[4 + idxRect + 2] : width
                        settings.Add(eARect.w);
                        //settings[4 + idxRect + 3] : height
                        settings.Add(eARect.h);
                        idxRect++;
                    }
                    spJson.Settings = settings;
                    spJson.CustomName = custLayout.Name;
                    spJson.CustomId = custLayout.ID;
                    spJson.EAID = custLayout.ID;

                    ddpmCustomList.Add(spJson);
                }
            }

            //2 Convert EasyArrange Per-user settings (EzSettings)
            //
            EzSettings ezSettings = new EzSettings();

            ezSettings.IsWidthoutGap = ddmUserSettings.EAWithoutGap;
            ezSettings.IsOnlyAllowWhenShiftKeyPressed = ddmUserSettings.EAWithShiftKey;
            ezSettings.IsSpanAcrossMultiMonitors = ddmUserSettings.EASpan;
            ezSettings.IsAwsEnabled = ddmUserSettings.SnapEnable;

            //3 Save to DDPMUserSettings
            //
            DDPMSettings appSettings = _SettingsPlugin.ReloadAppConfigData().Result;
            if (appSettings != null)
            {
                appSettings.UserSettings.EACustomList = ddpmCustomList.ToArray();
                appSettings.UserSettings.EzSettings = ezSettings;

                _SettingsPlugin.SetAppConfigData(appSettings);
            }

            //Migration MonitorSettings
            //
            if ((ddmMonitorSettings != null) || (ddmMonitorSettings.EasyArrangement != null))
            {
                MonitorInfo moinfo = new MonitorInfo();
                moinfo.modelName = ddmMonitorSettings.Model;
                moinfo.edid.ModelName = ddmMonitorSettings.Model;
                moinfo.edid.ServiceTag = ddmMonitorSettings.ServiceTag;

                //Will migrate Desktop[0] only
                if (ddmMonitorSettings.EasyArrangement.Desktops.Count > 0)
                {
                    //Convert ActiveLayout to SelectedSplit
                    //
                    int activeLayout = ddmMonitorSettings.EasyArrangement.Desktops[0].ActiveLayout;
                    SplitJson? selJson = null;
                    //activaLayout: [0~49]=preset layout, [1000~1004]=custom layout
                    if (activeLayout >= 1000)
                    {
                        //Find the CustomLayout by EAID
                        selJson = ddpmCustomList.Find(x => x.EAID == activeLayout);
                        if (selJson != null)
                        {
                        }
                    }
                    else
                    {
                        //Preset layout
                        selJson = new SplitJson()
                        {
                            CellCount = -1,
                            CustomId = 0,
                            EAID = activeLayout
                        };
                    }

                    //Convert RecentList
                    List<SplitJson> recentList = new List<SplitJson>();
                    foreach (int eaidRecent in ddmMonitorSettings.EasyArrangement.Desktops[0].LayoutMRU)
                    {
                        if (activeLayout >= 1000) //Custom Layout
                        {
                            //Find the CustomLayout by EAID
                            SplitJson? cusJson = ddpmCustomList.Find(x => x.EAID == eaidRecent);
                            if (cusJson != null)
                            {
                                recentList.Add(cusJson.Clone());
                            }
                        }
                        else
                        {
                            //Preset layout
                            SplitJson presetJson = new SplitJson()
                            {
                                CellCount = -1,
                                CustomId = 0,
                                EAID = eaidRecent
                            };
                            recentList.Add(presetJson);
                        }
                    }

                    //Save to EAMonitorSettings
                    EAMonitorSettings eaSettings = ReadEAMonitorSettings(moinfo).Result;
                    eaSettings.SelectedSplit = selJson;
                    eaSettings.RecentList = recentList.ToArray();
                    WriteEAMonitorSettings(moinfo, eaSettings);
                }
            }
        }

        #endregion Migration

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

        private void OnEzMemoryPluginConditionChangeHandler(object sender, EventArgs e)
        {
            GetCurrentEzMemoryPluginCondition();
        }

        private void OnTelementrySchedulerConditionChangeHandler(object sender, EventArgs e)
        {
            GetCurrentTelementrySchedulerCondition();
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

            if (e.ChangedPlugins.OfType<IDTPProxyPlugin>().Any())
                InitializeDTPProxyPlugin();

            if (e.ChangedPlugins.OfType<IEzMemoryPlugin>().Any())
                InitializeEzMemoryPlugin();

            if (e.ChangedPlugins.OfType<ITelementryScheduler>().Any())
                InitializeTelementrySchedulerPlugin();
        }

        //Jim, 2024-09-05 add new event
        private void OnColoresetManualChangeHandler(object sender, string e)
        {
            Coloreset_manual_ChangeEvent?.AsyncFireAndForget(this, e, System.Threading.CancellationToken.None);
        }

        private void OnNightLightStatusChangeHandler(object sender, string e)
        {
            NightLightStatus_ChangeEvent?.AsyncFireAndForget(this, e, System.Threading.CancellationToken.None);
        }

        #endregion

        #region OSD

        public Task ShowOSD(object monitorInfo, OSDType type, OSDType_Device Device, string Content)
        {
            if (monitorInfo != null)
            {
                switch (type)
                {
                    case OSDType.BatteryLow:
                        {
                            if (Device is OSDType_Device.Headset)
                            {
                                if (!string.IsNullOrWhiteSpace(Content))
                                    _showosd(monitorInfo, OSDType.BatteryLow, OSDType_Device.Headset, Content);
                                else
                                    writelog("[_showosd*******] Content error can't be NullOrWhiteSpace");
                                return Task.CompletedTask;
                            }
                            else if (Device is OSDType_Device.Keyboard)
                            {
                                if (!string.IsNullOrWhiteSpace(Content))
                                    _showosd(monitorInfo, OSDType.BatteryLow, OSDType_Device.Keyboard, Content);
                                else
                                    writelog("[_showosd*******] Content error can't be NullOrWhiteSpace");
                                return Task.CompletedTask;
                            }
                            else if (Device is OSDType_Device.Mouse)
                            {
                                if (!string.IsNullOrWhiteSpace(Content))
                                    _showosd(monitorInfo, OSDType.BatteryLow, OSDType_Device.Mouse, Content);
                                else
                                    writelog("[_showosd*******] Content error can't be NullOrWhiteSpace");
                                return Task.CompletedTask;
                            }
                            else
                                return Task.CompletedTask;
                        }
                    default:
                        return Task.CompletedTask;
                }
            }
            else
                return Task.CompletedTask;
        }

        public Task ShowOSD(object monitorInfo, OSDType type, string Content, bool State = false)
        {
            if (monitorInfo != null)
            {
                switch (type)
                {
                    case OSDType.Mute:
                        {
                            if (!string.IsNullOrWhiteSpace(Content))
                                _showosd(monitorInfo, OSDType.Mute, OSDType_Device.Unknown, Content, State);
                            else
                                writelog("[_showosd*******] Content error can't be NullOrWhiteSpace");
                            return Task.CompletedTask;
                        }
                    default:
                        return Task.CompletedTask;
                }
            }
            else
                return Task.CompletedTask;
        }

        public Task ShowOSD(object monitorInfo, OSDType type, bool State)
        {
            if (monitorInfo != null)
            {
                switch (type)
                {
                    case OSDType.ScrollLock:
                        {
                            _showosd(monitorInfo, OSDType.ScrollLock, OSDType_Device.Unknown, string.Empty, State);
                            return Task.CompletedTask;
                        }
                    case OSDType.NumLock:
                        {
                            _showosd(monitorInfo, OSDType.NumLock, OSDType_Device.Unknown, string.Empty, State);
                            return Task.CompletedTask;
                        }
                    case OSDType.CapsLock:
                        {
                            _showosd(monitorInfo, OSDType.CapsLock, OSDType_Device.Unknown, string.Empty, State);
                            return Task.CompletedTask;
                        }
                    default:
                        return Task.CompletedTask;
                }
            }
            else
                return Task.CompletedTask;
        }

        public Task ShowOSD(object monitorInfo, OSDType type)
        {
            if (monitorInfo != null)
            {
                switch (type)
                {
                    case OSDType.EasyMemory:
                        {
                            _showosd(monitorInfo, OSDType.EasyMemory, OSDType_Device.Unknown, string.Empty);
                            return Task.CompletedTask;
                        }
                    case OSDType.Fingerprint:
                        {
                            _showosd(monitorInfo, OSDType.Fingerprint, OSDType_Device.Unknown, string.Empty);
                            return Task.CompletedTask;
                        }
                    case OSDType.DisplayChanged:
                        {
                            _showosd(monitorInfo, OSDType.DisplayChanged, OSDType_Device.Unknown, string.Empty);
                            return Task.CompletedTask;
                        }
                    case OSDType.WalkAwayLock:
                        {
                            _showosd(monitorInfo, OSDType.WalkAwayLock, OSDType_Device.Unknown, "5");
                            return Task.CompletedTask;
                        }
                    case OSDType.StartRecording:
                        {
                            _showosd(monitorInfo, OSDType.StartRecording, OSDType_Device.Unknown, "3");
                            return Task.CompletedTask;
                        }
                    default:
                        return Task.CompletedTask;
                }
            }
            else
                return Task.CompletedTask;
        }

        public Task ShowOSD(object monitorInfo, OSDType type, bool State, (string, string, bool) args)
        {
            if (monitorInfo != null)
            {
                switch (type)
                {
                    case OSDType.Error:
                        {
                            _showosd(Screen.PrimaryScreen.DeviceName, OSDType.Error, OSDType_Device.Unknown, args.Item2, State, args.Item1, args.Item3);
                            return Task.CompletedTask;
                        }
                    default:
                        return Task.CompletedTask;
                }
            }
            else
                return Task.CompletedTask;
        }

        private void _showosd(object monitorInfo, OSDType _types, OSDType_Device _DeviceType, string Content, bool State = false, string title = "", bool stayOpen = false)
        {
            //writelog($"For debugging - Skip _showosd().");
            //return;
            //=====================================================================================
            try
            {
                //System.Windows.Application.Current.Dispatcher.Invoke(() =>
                //{
                Thread thread = new Thread(() =>
                {
                    if (monitorInfo != null)
                    {
                        var vr = IsValidJson(monitorInfo.ToString());

                        MonitorInfo typeCheck_MonitorInfo = new MonitorInfo();
                        if (vr)
                            typeCheck_MonitorInfo = JsonConvert.DeserializeObject<MonitorInfo>(monitorInfo.ToString());

                        var DisplayName = string.Empty;

                        if (monitorInfo is string)
                        {
                            DisplayName = monitorInfo.ToString();
                        }
                        else if (monitorInfo is MonitorInfo)
                        {
                            DisplayName = ((MonitorInfo)monitorInfo).DisplayName;
                            if (_types.Equals(OSDType.DisplayChanged))
                                Content = ((MonitorInfo)monitorInfo).modelName;
                        }
                        else if (typeCheck_MonitorInfo != null)
                        {
                            DisplayName = typeCheck_MonitorInfo.DisplayName;
                            if (_types.Equals(OSDType.DisplayChanged))
                                Content = typeCheck_MonitorInfo.modelName;
                        }

                        if (!string.IsNullOrWhiteSpace(DisplayName))
                        {
                            System.Windows.Forms.Screen sreen = System.Windows.Forms.Screen.AllScreens.FirstOrDefault(x => x.DeviceName == DisplayName);

                            if (sreen != null)
                            {
                                if (string.IsNullOrWhiteSpace(Content))
                                {
                                    string[] strings = (ScreenInterrogatory.DeviceFriendlyName(sreen).Split(' ')) ?? string.Empty.Split(' ');
                                    if (strings.Length > 1)
                                        Content = strings[1];
                                    else
                                        Content = strings[0];
                                }

                                var dpiXProperty = typeof(SystemParameters).GetProperty("DpiX", BindingFlags.NonPublic | BindingFlags.Static);
                                var varX = (int)dpiXProperty.GetValue(null, null);
                                double dpiX = (double)varX / (double)96;

                                switch (_types)
                                {
                                    case OSDType.Mute:
                                        {
                                            if (State)
                                            {
                                                try
                                                {
                                                    _OSD_Controler.Mute_CloseWindow();
                                                    _OSD_Controler.Mute_ShowWindow(Content, (sreen.WorkingArea.Top / (double)dpiX), (sreen.WorkingArea.Left / (double)dpiX));
                                                }
                                                catch (Exception ex)
                                                {
                                                    writelog($"[_showosd] ERROR - OSDType.Mute: {ex.Message}, State: {State}");
                                                }
                                            }
                                            else
                                            {
                                                try
                                                {
                                                    _OSD_Controler.UnMute_CloseWindow();
                                                    _OSD_Controler.UnMute_ShowWindow(Content, (sreen.WorkingArea.Top / (double)dpiX), (sreen.WorkingArea.Left / (double)dpiX));
                                                }
                                                catch (Exception ex)
                                                {
                                                    writelog($"[_showosd] ERROR - OSDType.Mute: {ex.Message}, State: {State}");
                                                }
                                            }
                                        }
                                        break;

                                    case OSDType.BatteryLow:
                                        {
                                            if (_DeviceType is OSDType_Device.Headset)
                                            {
                                                try
                                                {
                                                    _OSD_Controler.HeadsetBatteryLow_CloseWindow();
                                                    _OSD_Controler.HeadsetBatteryLow_ShowWindow(Content, (sreen.WorkingArea.Top / (double)dpiX), (sreen.WorkingArea.Left / (double)dpiX));
                                                }
                                                catch (Exception ex)
                                                {
                                                    writelog($"[_showosd] ERROR - OSDType.BatteryLow: {ex.Message}");
                                                }
                                            }
                                            else if (_DeviceType is OSDType_Device.Keyboard)
                                            {
                                                try
                                                {
                                                    _OSD_Controler.KeybordBatteryLow_CloseWindow();
                                                    _OSD_Controler.KeybordBatteryLow_ShowWindow(Content, (sreen.WorkingArea.Top / (double)dpiX), (sreen.WorkingArea.Left / (double)dpiX));
                                                }
                                                catch (Exception ex)
                                                {
                                                    writelog($"[_showosd] ERROR - OSDType_Device.Keyboard: {ex.Message}");
                                                }
                                            }
                                            else if (_DeviceType is OSDType_Device.Mouse)
                                            {
                                                try
                                                {
                                                    _OSD_Controler.MouseBatteryLow_CloseWindow();
                                                    _OSD_Controler.MouseBatteryLow_ShowWindow(Content, (sreen.WorkingArea.Top / (double)dpiX), (sreen.WorkingArea.Left / (double)dpiX));
                                                }
                                                catch (Exception ex)
                                                {
                                                    writelog($"[_showosd] ERROR - OSDType_Device.Mouse: {ex.Message}");
                                                }
                                            }
                                            else if (_DeviceType is OSDType_Device.Pen)
                                            {
                                                try
                                                {
                                                    _OSD_Controler.StylusBatteryLow_CloseWindow();
                                                    _OSD_Controler.StylusBatteryLow_ShowWindow(Content, (sreen.WorkingArea.Top / (double)dpiX), (sreen.WorkingArea.Left / (double)dpiX));
                                                }
                                                catch (Exception ex)
                                                {
                                                    writelog($"[_showosd] ERROR - OSDType_Device.Mouse: {ex.Message}");
                                                }
                                            }
                                        }
                                        break;

                                    case OSDType.StartRecording:
                                        {
                                            try
                                            {
                                                _OSD_Controler.StartRecording_CloseWindow();
                                                _OSD_Controler.StartRecording_ShowWindow(Content, (sreen.WorkingArea.Top / (double)dpiX), (sreen.WorkingArea.Left / (double)dpiX));
                                            }
                                            catch (Exception ex)
                                            {
                                                writelog($"[_showosd] ERROR - OSDType.StartRecording: {ex.Message}");
                                            }
                                        }
                                        break;

                                    case OSDType.DisplayChanged:
                                        {
                                            try
                                            {
                                                _OSD_Controler.DisplayChanged_CloseWindow();
                                                _OSD_Controler.DisplayChanged_ShowWindow(Content, (sreen.WorkingArea.Top / (double)dpiX), (sreen.WorkingArea.Left / (double)dpiX));
                                            }
                                            catch (Exception ex)
                                            {
                                                writelog($"[_showosd] ERROR - OSDType.DisplayChanged: {ex.Message}");
                                            }
                                        }
                                        break;

                                    case OSDType.WalkAwayLock:
                                        {
                                            try
                                            {
                                                _OSD_Controler.WalkAwayLock_CloseWindow();
                                                _OSD_Controler.WalkAwayLock_ShowWindow(Content, (sreen.WorkingArea.Top / (double)dpiX), (sreen.WorkingArea.Left / (double)dpiX));
                                            }
                                            catch (Exception ex)
                                            {
                                                writelog($"[_showosd] ERROR - OSDType.WalkAwayLock: {ex.Message}");
                                            }
                                        }
                                        break;

                                    case OSDType.ScrollLock:
                                        {
                                            if (State)
                                            {
                                                try
                                                {
                                                    _OSD_Controler.ScrollLockOn_CloseWindow();
                                                    _OSD_Controler.ScrollLockOn_ShowWindow((sreen.WorkingArea.Top / (double)dpiX), (sreen.WorkingArea.Left / (double)dpiX));
                                                }
                                                catch (Exception ex)
                                                {
                                                    writelog($"[_showosd] ERROR - OSDType.ScrollLock: {ex.Message}, State:{State}");
                                                }
                                            }
                                            else
                                            {
                                                try
                                                {
                                                    _OSD_Controler.ScrollLockOff_CloseWindow();
                                                    _OSD_Controler.ScrollLockOff_ShowWindow((sreen.WorkingArea.Top / (double)dpiX), (sreen.WorkingArea.Left / (double)dpiX));
                                                }
                                                catch (Exception ex)
                                                {
                                                    writelog($"[_showosd] ERROR - OSDType.ScrollLock: {ex.Message}, State:{State}");
                                                }
                                            }
                                        }
                                        break;

                                    case OSDType.NumLock:
                                        {
                                            if (State)
                                            {
                                                try
                                                {
                                                    _OSD_Controler.NumLockOn_CloseWindow();
                                                    _OSD_Controler.NumLockOn_ShowWindow((sreen.WorkingArea.Top / (double)dpiX), (sreen.WorkingArea.Left / (double)dpiX));
                                                }
                                                catch (Exception ex)
                                                {
                                                    writelog($"[_showosd] ERROR - OSDType.NumLock: {ex.Message}, State:{State}");
                                                }
                                            }
                                            else
                                            {
                                                try
                                                {
                                                    _OSD_Controler.NumLockOff_CloseWindow();
                                                    _OSD_Controler.NumLockOff_ShowWindow((sreen.WorkingArea.Top / (double)dpiX), (sreen.WorkingArea.Left / (double)dpiX));
                                                }
                                                catch (Exception ex)
                                                {
                                                    writelog($"[_showosd] ERROR - OSDType.NumLock: {ex.Message}, State:{State}");
                                                }
                                            }
                                        }
                                        break;

                                    case OSDType.CapsLock:
                                        {
                                            if (State)
                                            {
                                                try
                                                {
                                                    _OSD_Controler.CapsLockOn_CloseWindow();
                                                    _OSD_Controler.CapsLockOn_ShowWindow((sreen.WorkingArea.Top / (double)dpiX), (sreen.WorkingArea.Left / (double)dpiX));
                                                }
                                                catch (Exception ex)
                                                {
                                                    writelog($"[_showosd] ERROR - OSDType.CapsLock: {ex.Message}, State:{State}");
                                                }
                                            }
                                            else
                                            {
                                                try
                                                {
                                                    _OSD_Controler.CapsLockOff_CloseWindow();
                                                    _OSD_Controler.CapsLockOff_ShowWindow((sreen.WorkingArea.Top / (double)dpiX), (sreen.WorkingArea.Left / (double)dpiX));
                                                }
                                                catch (Exception ex)
                                                {
                                                    writelog($"[_showosd] ERROR - OSDType.CapsLock: {ex.Message}, State:{State}");
                                                }
                                            }
                                        }
                                        break;

                                    case OSDType.Fingerprint:
                                        {
                                            try
                                            {
                                                _OSD_Controler.Fingerprint_CloseWindow();
                                                _OSD_Controler.Fingerprint_ShowWindow((sreen.WorkingArea.Top / (double)dpiX), (sreen.WorkingArea.Left / (double)dpiX));
                                            }
                                            catch (Exception ex)
                                            {
                                                writelog($"[_showosd] ERROR - OSDType.Fingerprint: {ex.Message}");
                                            }
                                        }
                                        break;

                                    case OSDType.EasyMemory:
                                        {
                                            try
                                            {
                                                _OSD_Controler.EasyMemory_CloseWindow();
                                                _OSD_Controler.EasyMemory_ShowWindow((sreen.WorkingArea.Top / (double)dpiX), (sreen.WorkingArea.Left / (double)dpiX));
                                            }
                                            catch (Exception ex)
                                            {
                                                writelog($"[_showosd] ERROR - OSDType.EasyMemory: {ex.Message}");
                                            }
                                        }
                                        break;

                                    case OSDType.Error:
                                        {
                                            if (State)
                                            {
                                                try
                                                {
                                                    _OSD_Controler.Error_CloseWindow();
                                                    _OSD_Controler.Error_ShowWindow(title, Content, stayOpen, (sreen.WorkingArea.Top / (double)dpiX), (sreen.WorkingArea.Left / (double)dpiX));
                                                }
                                                catch (Exception ex)
                                                {
                                                    writelog($"[_showosd] ERROR - OSDType.Error: {ex.Message}, State:{State}");
                                                }
                                            }
                                            else
                                            {
                                                try
                                                {
                                                    _OSD_Controler.Error_CloseWindow();
                                                    _OSD_Controler.Error_ShowWindow(title, Content, stayOpen, (sreen.WorkingArea.Top / (double)dpiX), (sreen.WorkingArea.Left / (double)dpiX));
                                                }
                                                catch (Exception ex)
                                                {
                                                    writelog($"[_showosd] ERROR - OSDType.Error: {ex.Message}, State:{State}");
                                                }
                                            }
                                        }
                                        break;

                                    default:
                                        break;
                                }
                            }
                        }
                    }
                    System.Windows.Threading.Dispatcher.Run();
                });
                thread.SetApartmentState(ApartmentState.STA);
                thread.Start();
                //});

                //=====================================================================================
            }
            catch (Exception ex)
            {
                writelog($"OSD Exception: {ex.ToString()}");
            }
        }

        private bool IsValidJson(string jsonString)
        {
            try
            {
                JObject.Parse(jsonString);
                return true;
            }
            catch (JsonReaderException)
            {
                return false;
            }
        }

        #endregion OSD

        #region EzM

        public Task<Dictionary<string, InstalledAppInfo>> GetAllAppList()
        {
            if (_IEzMemoryPlugin != null)
                return Task.FromResult(_IEzMemoryPlugin.GetAllAppList().Result);
            else
                return null;
        }

        public Task<bool> LaunchAndArrangeApps(Dictionary<String, Bind_AddFullPage_AppCollectionData> sortApps)
        {
            if (_IEzMemoryPlugin != null)
                return Task.FromResult(_IEzMemoryPlugin.LaunchAndArrangeApps(sortApps).Result);
            else
                return null;
        }

        #endregion EzM

        #region Common Json read/write interfaces

        //For common json file read/write
        public Task<string> ReadSerializedContentFromFile(string filePath)
        {
            string result = null;
            if (_SettingsPlugin != null)
            {
                return Task.FromResult(_SettingsPlugin.ReadSerializedContentFromFile(filePath).Result);
            }

            return Task.FromResult(result);
        }

        public Task<bool> WriteSerializedContentToFile(string filePath, string content)
        {
            bool result = false;
            if (_SettingsPlugin != null)
            {
                return Task.FromResult(_SettingsPlugin.WriteSerializedContentToFile(filePath, content).Result);
            }

            return Task.FromResult(result);
        }

        #endregion

        #region TelemetryScheduler

        public Task StartTelemetrySchedulerManger(bool IsStart)
        {
            if (_TelementryScheduler != null)
                _TelementryScheduler.StartTelemetrySchedulerManger(IsStart);

            return Task.CompletedTask;
        }

        public Task<bool> ReceiveTelemetryInfo(string EventTag, string EventValue, Telementry_Frequency Frequency)
        {
            var r = false;
            if (_TelementryScheduler != null)
                r = _TelementryScheduler.ReceiveTelemetryInfo(EventTag, EventValue, Frequency).Result;

            return Task.FromResult(r);
        }

        //Robert_Lin, 2024-10-27 Telemetry for PIP/PBP
        public void SendPipPbpTelemetry(string eventValue, MonitorInfo? mi = null, Telementry_Frequency frequency = Telementry_Frequency.RealTime)
        {
            if (_TelementryScheduler != null)
            {
                PipPbpTelemetry pxpTelemetry = new PipPbpTelemetry();
                pxpTelemetry.PbpPip = eventValue;
                pxpTelemetry.CommunicationPath = "Video";
                pxpTelemetry.GraphicCardName = string.Empty;
                pxpTelemetry.MonitorName = (mi == null) ? "" : mi.AliasDeviceName;
                pxpTelemetry.D_Ctrl = (mi == null) ? "" : mi.D_Ctrl;
                pxpTelemetry.SupplierID = (mi == null) ? "" : mi.SupplierID;
                pxpTelemetry.FirmwareVersion = (mi == null) ? "" : mi.FwVersion;
                pxpTelemetry.DisplayModelname = (mi == null) ? "" : mi.modelName;
                pxpTelemetry.DsiplayResolution = string.Empty;
                pxpTelemetry.MaxDisplayResolution = string.Empty;
                _TelementryScheduler.ReceiveTelemetryInfo("DisplayFeatures", pxpTelemetry.ToJson(), frequency);
            }
        }

        //Robert_Lin, 2024-10-27 Telemetry for EasyArrange
        private void SendEasyArrangeTelemetry(string eventValue, MonitorInfo? mi = null, Telementry_Frequency frequency = Telementry_Frequency.RealTime)
        {
            if (_TelementryScheduler != null)
            {
                EasyArrangeTelemetry easyArrangeTelemetry = new EasyArrangeTelemetry();
                easyArrangeTelemetry.EasyArrange = eventValue;
                easyArrangeTelemetry.CommunicationPath = "Video";
                easyArrangeTelemetry.GraphicCardName = string.Empty;
                easyArrangeTelemetry.MonitorName = (mi == null) ? "" : mi.AliasDeviceName;
                easyArrangeTelemetry.D_Ctrl = (mi == null) ? "" : mi.D_Ctrl;
                easyArrangeTelemetry.SupplierID = (mi == null) ? "" : mi.SupplierID;
                easyArrangeTelemetry.FirmwareVersion = (mi == null) ? "" : mi.FwVersion;
                easyArrangeTelemetry.DisplayModelname = (mi == null) ? "" : mi.modelName;
                easyArrangeTelemetry.DisplayServiceTag = (mi == null) ? "" : mi.edid.ServiceTag;
                easyArrangeTelemetry.DsiplayResolution = string.Empty;
                easyArrangeTelemetry.MaxDisplayResolution = string.Empty;
                _TelementryScheduler.ReceiveTelemetryInfo("DisplayFeatures", easyArrangeTelemetry.ToJson(), frequency);
            }
        }

        #endregion

        private void MonitorEvent_On(object sender, EventArgs e)
        {
            writelog("GOT MONITOR ON EVENT");
            if (_DisplayManagerPlugin != null)
            {
                //using (_ReGetcancellationTokenSource = new CancellationTokenSource())
                {
                    try
                    {
                        //var _cancellationTokenSource_tmp = CancellationTokenSource.CreateLinkedTokenSource(_ReGetcancellationTokenSource.Token);
                        //var token = _cancellationTokenSource_tmp.Token;

                        //Call VCP to catch updated monitor info
                        if (_AllInfoMonitors != null)
                            _AllInfoMonitors.Clear();
                        else
                            _AllInfoMonitors = new List<MonitorInfo>();
                        writelog("_DisplayManagerPlugin.Re_GetMonitors with token");
                        _AllInfoMonitors.AddRange((Re_GetMonitors().Result).ToList());
                    }
                    catch (Exception ex)
                    {
                        writelog($"MonitorEvent_On to Re-GetMonitor failed: {ex.Message}");
                    }
                }
            }
        }

        private string GetMonitorCurrentResolution(MonitorInfo monitor)
        {
            var rc = string.Empty;
            if (_DisplayManagerPlugin != null)
                rc = _DisplayManagerPlugin.GetMonitorCurrentResolution(monitor).Result;
            return rc;
        }

        private string GetMonitorMaxResolution(MonitorInfo monitor)
        {
            var rc = string.Empty;
            if (_DisplayManagerPlugin != null)
                rc = _DisplayManagerPlugin.GetMonitorMaxResolution(monitor).Result;
            return rc;
        }
    }
}