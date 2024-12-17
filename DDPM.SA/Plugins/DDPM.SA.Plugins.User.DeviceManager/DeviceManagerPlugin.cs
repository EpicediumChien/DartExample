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
using DDPM.SA.Common.Method;
using DDPM.SA.Common.Popup;
using DDPM.SA.Common.Screen;
using DDPM.SA.Common.Settings;
using DDPM.SA.Common.Telemetry;
using DDPM.SA.Common.UI;
using DDPM.SA.Common.UpdateProgressPage;
using DDPM.SA.Resources.Helper;
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
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
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
using static DDPM.SA.Plugins.User.DeviceManager.DisplayDeviceHelper;
using static VcpCore.Common.User32;
using IDs = DDPM.SA.Common.IDs;
using Point = System.Windows.Point;
//using MonitorProfile = DDPM.SA.Utility.MonitorProfile;

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
        private const string publisherCompany = "Dell Inc.";
        private const string publisherWebsite = "https://www.dell.com";
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

        //private static Dell.Client.Framework.Utility.Log _log;
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

        /// <summary>
        /// Webcam change event
        /// </summary>
        //public event EventHandler<bool>? Esi_IsCameraSensorCover_ChangeEvent;
        //public event EventHandler<int>? WALSnoozeTimeLeftInSeconds_ChangeEvent;
        //public event EventHandler<bool>? Esi_IsWALLockCountdownStartedChanged_ChangeEvent;
        //public event EventHandler<int>? Esi_WALLockCountdownChanged_ChangeEvent;

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
        private bool isBypassHotkey { get; set; } = false;

        //powerNap
        private JobQueue _powerNapJobQueue = new JobQueue();

        private static System.Timers.Timer _PowerNapTimer = new System.Timers.Timer(2000);

        //USBKVM auto switch USB upstream ports in PBP side-by-side mode
        private static System.Timers.Timer _USBKVMAutoSwitchTimer = new System.Timers.Timer(1500);

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

        private QAMPage _QAM = null;
        private Point QAM_Position;
        private bool isDDPMHomepageReady = false;
        private bool isDDPMLaunchedByQAM = false;
        private bool isWidgetSettingPageLoadedByQAM = false;

        private static CancellationTokenSource _ReGetcancellationTokenSource = null;

        private static bool _isSubagentActive = true;
        private bool userClosedPopup = false;

        private static PowerEventControl _pwr_Mon = null;
        private static DisplayDeviceHelper _disDevHelper = null;
        private static int _millisecond = 8000;
        private static OSD_Controler _OSD_Controler = new OSD_Controler();

        private OSThemeEnum previousOsTheme = OSThemeEnum.Dark;

        private List<NKVMVCPValue> _nKVMVCPValues = new List<NKVMVCPValue>();

        #endregion

        #region Constructor

        public DeviceMangerPlugin(IAgent agent) : base(agent, PluginLogId)
        {
            _agent = agent;
            _PowerNapTimer.Elapsed += OnPowerNapTimedRaise;
            _PowerNapTimer.AutoReset = true;
            _PowerNapTimer.Enabled = true;

            _USBKVMAutoSwitchTimer.Elapsed += OnUsbKvmAutoSwitchTimedRaise;
            _USBKVMAutoSwitchTimer.AutoReset = true;
            _USBKVMAutoSwitchTimer.Enabled = true;

            UXSystemParameters.Instance.ParameterChangedEvent += UXSystemParametersChanged;
            writelog("DeviceManagerPlugin constructor ...");

            _isSubagentActive = WTSFunction.IsYourProcessInActiveSession(Log);
            SACommonHelper.GetResourceDictionary();
            loadResourceDictionary(UXSystemParameters.Instance.OSTheme);

            //Robert_Lin, 2024-12-1 added, to let TextBox highlight text color can be changed with TextBox.SelectionTextBrush
            //Reference: https://github.com/dotnet/wpf/issues/4571
            AppContext.SetSwitch("Switch.System.Windows.Controls.Text.UseAdornerForTextboxSelectionRendering", false);
        }

        private void _DTPProxyPlugin_DTPEventHandler(object sender, UpdateUINotify e)
        {
            if (e.UI_Field_Name.StartsWith("Keyboard") || e.UI_Field_Name.StartsWith("Mouse") || e.UI_Field_Name.StartsWith("Pen"))
            {
                var paras = e.UI_Field_Name.Split('|');
                if (paras.Length < 4)
                {
                    writelog("_DTPProxyPlugin_DTPEventHandler UpdateUINotify e.UI_Field_Na incorrect!");
                    return;
                }
                DeviceInfo di = new();
                di.LogicalDeviceType = paras[0];
                di.ModelNumber = paras[3];
                DeviceChangedEventArgs _EventArgs = new DeviceChangedEventArgs();
                _EventArgs.type = DeviceChangedType.Peripherals_SettingsChange;
                _EventArgs.device_peripherals = di;
                _EventArgs.changedProperty = paras[1];
                DeviceChanged?.Invoke(this, _EventArgs);
            }
            else
                OnUIUpdateNotify(e);
        }

        private string debugPreMsg = string.Empty;
        private System.Drawing.Point previousCursorPosition = new System.Drawing.Point { X = 0, Y = 0 };
        private bool isKvm_Auto_SwitchKbMsKey = false;
        private bool isKvm_Auto_SwitchKbMsWideMove = false;
        private int KvmAutoSwitchCounter = 0;

        private void OnUsbKvmAutoSwitchTimedRaise(object sender, ElapsedEventArgs e)
        {
            //cursor position
            System.Drawing.Point cursorPosition = Cursor.Position;
            // retrieve the monitor object from cursor's position
            Screen currentScreen = Screen.FromPoint(cursorPosition);
            MonitorInfo monitorInfo = _AllInfoMonitors.Find(x => x.DisplayName.ToUpper().Equals(currentScreen.DeviceName.ToUpper()));
            if (monitorInfo != null)
            {
                string debugMsg = $"monitor:{monitorInfo.modelName}{monitorInfo.edid.SerialNumber}:currentScreen.WorkingAreaWidth={currentScreen.WorkingArea.Width}:inputCable={monitorInfo.inputCable};inputSource={monitorInfo.inputSource};{currentScreen.DeviceName};Primary:{currentScreen.Primary};WorkingArea.X:{currentScreen.WorkingArea.X};,X={cursorPosition.X},Y={cursorPosition.Y}";

                if (isKvm_Auto_SwitchKbMsWideMove)
                {
                    if ((cursorPosition.X <= (previousCursorPosition.X - 50)) || (cursorPosition.X >= (previousCursorPosition.X + 50)) ||
                        (cursorPosition.Y <= (previousCursorPosition.Y - 50)) || (cursorPosition.Y >= (previousCursorPosition.Y + 50)))
                    {
                        isKvm_Auto_SwitchKbMsWideMove = false;
                    }
                    return;
                }
                if (isUsbKvmCursorEdge(monitorInfo, currentScreen, cursorPosition))
                {
                    KvmAutoSwitchCounter += 1;
                }
                else
                {
                    KvmAutoSwitchCounter = 0;
                }
                if (KvmAutoSwitchCounter >= 3)
                {
                    if (!debugPreMsg.Equals(debugMsg))
                    {
                        debugPreMsg = debugMsg;
                        Debug.WriteLine($"{debugMsg}");
                    }

                    if (isKvm_Auto_SwitchKbMsKey || (previousCursorPosition.X.Equals(cursorPosition.X) && previousCursorPosition.Y.Equals(cursorPosition.Y)))
                    {
                        Debug.WriteLine($"[USBKVM_Auto_Switch],previous.X={previousCursorPosition.X},previous.Y={previousCursorPosition.Y};Curs.X={cursorPosition.X},Curs.Y={cursorPosition.Y};isKvm_Auto_SwitchKbMsKey= {isKvm_Auto_SwitchKbMsKey}");
                        writelog($"[USBKVM_Auto_Switch],previous.X={previousCursorPosition.X},previous.Y={previousCursorPosition.Y};Curs.X={cursorPosition.X},Curs.Y={cursorPosition.Y};isKvm_Auto_SwitchKbMsKey= {isKvm_Auto_SwitchKbMsKey}");
                        previousCursorPosition = cursorPosition;
                        KvmAutoSwitchCounter = 0;
                        return;
                    }
                    writelog($"[USBKVM_Auto_Switch] Kvm_Auto_SwitchKbMsKey:monitor:{monitorInfo.edid.ServiceTag} Enqueue");
                    //send usbKvm switch

                    isKvm_Auto_SwitchKbMsKey = true;
                    isKvm_Auto_SwitchKbMsWideMove = true;
                    KvmAutoSwitchCounter = 0;
                    previousCursorPosition = cursorPosition;
                    _hotkeyJobQueue.Enqueue(new JobInfo(1000, monitorInfo, null, Kvm_Auto_SwitchKbMsKey));
                }
            }
        }

        private bool isUsbKvmCursorEdge(MonitorInfo monitorInfo, Screen currentScreen, System.Drawing.Point cursorPosition)
        {
            bool ret = false;
            if (monitorInfo != null)
            {
                if (GetOnUSBKVM(monitorInfo).Result)
                {
                    //check
                    if (_hotkeySettings != null && _hotkeySettings.Count == 0)
                    {
                        _hotkeySettings = _SettingsPlugin.ReadHotkeySettings().Result;
                    }
                    if (_hotkeySettings != null && _hotkeySettings.Count > 0)
                    {
                        HotkeySettings hotkeySettings = _hotkeySettings.FirstOrDefault(x => x.ServiceTag.Equals("DDPM") && x.SerialNumber.Equals("DDPM"));
                        if (hotkeySettings != null)
                        {
                            if (hotkeySettings.HotkeyOptions.Count > 0 && hotkeySettings.HotkeyOptions.Any(x => x.Equals(HotkeyOption.KvmAutoApply)))
                            {
                                //check cursor position at the edge
                                //1.get PBP mode sub input source
                                Rectangle bounds = currentScreen.Bounds;
                                Dictionary<string, InputInfo> inputSourcelist = GetInputSourcelist(monitorInfo).Result;
                                List<ushort> subInputListRet = GetSubInputList(monitorInfo).Result;
                                //var currentResolution = _DisplayManagerPlugin.GetMonitorCurrentResolution(monitorInfo).Result;
                                //GetUSBKVMPCsList(monitorInfo);
                                bool isEdge = false;
                                string debugMsg = $"monitor:{monitorInfo.modelName}{monitorInfo.edid.SerialNumber}:currentScreen.WorkingAreaWidth={currentScreen.WorkingArea.Width}:inputCable={monitorInfo.inputCable};inputSource={monitorInfo.inputSource};{currentScreen.DeviceName};Primary:{currentScreen.Primary};WorkingArea.X:{currentScreen.WorkingArea.X};,X={cursorPosition.X},Y={cursorPosition.Y}";
                                if (monitorInfo.inputCable.Equals(monitorInfo.inputSource))
                                {
                                    // PBP as main inputSource ,screen is left
                                    writelog($"[USBKVM_Auto_Switch]as PBP main input:{debugMsg}");
                                    if (cursorPositionXSide("left", currentScreen, cursorPosition.X))
                                    {
                                        Debug.WriteLine($"left edge: approach");
                                        writelog($"[USBKVM_Auto_Switch] [left edge: approach]\r\n {debugMsg}");
                                        isEdge = true;
                                    }
                                }
                                else
                                {
                                    // PBP as sub imputSource,screen is right
                                    writelog($"[USBKVM_Auto_Switch]as PBP subinput:{debugMsg}");
                                    if (cursorPositionXSide("right", currentScreen, cursorPosition.X))
                                    {
                                        Debug.WriteLine($"right edge: approach");
                                        writelog($"[USBKVM_Auto_Switch] [right edge: approach] \r\n {debugMsg}");
                                        isEdge = true;
                                    }
                                }
                                if (isEdge)
                                {
                                    lock (USBKVM_PBPmode_lock)
                                    {
                                        UsbKvmPBP usbKvmPBP1 = usbKvmPBPs.SingleOrDefault(x => x.MonitorInfo.edid.ServiceTag.Equals(monitorInfo.edid.ServiceTag) && x.MonitorInfo.edid.SerialNumber.Equals(monitorInfo.edid.SerialNumber));
                                        if (usbKvmPBP1 != null && usbKvmPBP1.isPBPmode)
                                        {
                                            ret = true;
                                        }
                                        else
                                        {
                                            foreach (var item in usbKvmPBPs)
                                            {
                                                Debug.WriteLine($"{item.MonitorInfo.modelName}_{item.MonitorInfo.edid.ServiceTag},PBP mode={item.isPBPmode}");
                                                writelog($"[USBKVM_Auto_Switch]isUsbKvmCursorEdge:{item.MonitorInfo.modelName}_{item.MonitorInfo.edid.ServiceTag},PBP mode={item.isPBPmode}");
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                writelog($"[USBKVM_Auto_Switch]isUsbKvmCursorEdge:KvmAutoApply OFF");
                            }
                        }
                    }
                }
                else
                {
                    writelog($"[USBKVM_Auto_Switch]isUsbKvmCursorEdge:{monitorInfo.modelName}:{monitorInfo.edid.ServiceTag} USBKVM OFF");
                }
            }
            return ret;
        }

        private void Kvm_Auto_SwitchKbMsKey(MonitorInfo monitorInfo, Object[] param)
        {
            bool usbSwitch = UsbSwitch1(monitorInfo).Result;
            if (usbSwitch)
                isKvm_Auto_SwitchKbMsKey = false;
            writelog($"Kvm_Auto_SwitchKbMsKey:[{monitorInfo.edid.ModelName}:{monitorInfo.edid.SerialNumber}]" + (usbSwitch ? "success" : "fail"));
        }

        private bool cursorPositionXSide(string side, Screen screen, int x)
        {
            bool ret = false;
            int xPos = -1;
            if ("left".Equals(side))
            {
                int v = Math.Abs(screen.WorkingArea.X - x) - screen.WorkingArea.Width;
                xPos = Math.Abs(v);
            }
            if ("right".Equals(side))
            {
                int v = Math.Abs(screen.WorkingArea.X - x);
                xPos = Math.Abs(v);
            }
            if (xPos >= 0 && xPos <= 30)
                ret = true;
            return ret;
        }

        private List<UsbKvmPBP> usbKvmPBPs = new List<UsbKvmPBP>();
        private object USBKVM_PBPmode_lock = new object();

        private void updatePBPModeStatus(MonitorInfo monitorInfo, string vcpcode)
        {
            if ("E7".Equals(vcpcode, StringComparison.OrdinalIgnoreCase))
            {
                System.Drawing.Point cursorPosition = Cursor.Position;
                // retrieve the monitor object from cursor's position
                Screen currentScreen = Screen.FromPoint(cursorPosition);
                MonitorInfo monitorInfoFind = _AllInfoMonitors.Find(x => x.DisplayName.ToUpper().Equals(currentScreen.DeviceName.ToUpper()));
                if (monitorInfoFind != null)
                {
                    if (monitorInfoFind.edid.ServiceTag.Equals(monitorInfo.edid.ServiceTag) && monitorInfoFind.edid.SerialNumber.Equals(monitorInfo.edid.SerialNumber))
                    {
                        previousCursorPosition = cursorPosition;
                        KvmAutoSwitchCounter = 0;
                        writelog($"[USBKVM_Auto_Switch]updatePBPModeStatus:{monitorInfo.modelName}:{monitorInfo.edid.ServiceTag} =>vcpcode:E7; cursorPosition = {cursorPosition};Timer:{_USBKVMAutoSwitchTimer.Enabled}");
                    }
                }
            }
            if ("E9".Equals(vcpcode, StringComparison.OrdinalIgnoreCase) || vcpcode.Equals("2"))
            {
                if (!monitorInfo.CapabilityDic.ContainsKey("E9"))
                    return;
                ObjGetVCP ret = GetPxpMode(monitorInfo).Result;
                UsbKvmPBP usbKvmPBP = new UsbKvmPBP { MonitorInfo = monitorInfo, isPBPmode = false };
                UInt16 _curPxpMode = 0;
                if (ret != null && ret.result)
                {
                    _curPxpMode = Convert.ToUInt16(ret.value);
                    switch (_curPxpMode)
                    {
                        case 0x11:
                            usbKvmPBP.isPBPmode = false;
                            break;

                        case 0x12:
                            usbKvmPBP.isPBPmode = false;
                            break;

                        case 0x24:
                        case 0x2F:
                        case 0x26:
                        case 0x28:
                        case 0x2A:
                        case 0x2C:
                        case 0x2E:
                        case 0x25:
                        case 0x27:
                        case 0x29:
                        case 0x2B:
                        case 0x2D:
                        case 0x31:
                        case 0x32:
                        case 0x33:
                        case 0x34:
                        case 0x35:
                        case 0x41:
                        case 0x42:
                            usbKvmPBP.isPBPmode = true;
                            break;

                        default:
                            usbKvmPBP.isPBPmode = false;
                            break;
                    }
                    lock (USBKVM_PBPmode_lock)
                    {
                        if (usbKvmPBPs.Count > 0)
                        {
                            UsbKvmPBP usbKvmPBP1 = usbKvmPBPs.SingleOrDefault(x => x.MonitorInfo.edid.ServiceTag.Equals(monitorInfo.edid.ServiceTag) && x.MonitorInfo.edid.SerialNumber.Equals(monitorInfo.edid.SerialNumber));
                            if (usbKvmPBP1 != null)
                            {
                                usbKvmPBP1.isPBPmode = usbKvmPBP.isPBPmode;
                            }
                            else
                            {
                                usbKvmPBPs.Add(usbKvmPBP);
                            }
                        }
                        else
                        {
                            usbKvmPBPs.Add(usbKvmPBP);
                        }
                        if (usbKvmPBPs.Any(x => x.isPBPmode))
                        {
                            _USBKVMAutoSwitchTimer.Stop();
                            _USBKVMAutoSwitchTimer.Start();
                            writelog($"[USBKVM_Auto_Switch]updatePBPModeStatus:{monitorInfo.modelName}:{monitorInfo.edid.ServiceTag}:Timer:{_USBKVMAutoSwitchTimer.Enabled}, PBPmode ON");
                        }
                        else
                        {
                            _USBKVMAutoSwitchTimer.Stop();
                            writelog($"[USBKVM_Auto_Switch]updatePBPModeStatus:{monitorInfo.modelName}:{monitorInfo.edid.ServiceTag}:Timer:{_USBKVMAutoSwitchTimer.Enabled}, PBPmode OFF");
                        }
                    }
                }
            }
        }

        private void UXSystemParametersChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(UXSystemParameters.Instance.OSTheme))
            {
                OSThemeEnum oSTheme = UXSystemParameters.Instance.OSTheme;
                if (previousOsTheme == oSTheme)
                    return;
                var applicationSettings_Function = new ApplicationSettings_Function();
                string appModeTelementryData = loadResourceDictionary(oSTheme);
                if (string.IsNullOrEmpty(appModeTelementryData))
                {
                    writelog($"[appModeTelementryData] cannot be null", nameof(appModeTelementryData));
                    return;
                }
                Debug.WriteLine($"AppModeTelemetry=> {appModeTelementryData}");
                writelog("[DeviceMangerPlugin] [Telementry] Send Telementry for AppMode...");
                Task.Run(() => applicationSettings_Function.Send_AppMode_Telementry(_TelementryScheduler, _AllInfoMonitors, appModeTelementryData)).ConfigureAwait(false);
                /* if (rt) writelog("[DeviceMangerPlugin] [Telementry] Send Telementry for AppMode Success ...");
                 else writelog("[DeviceMangerPlugin] [Telementry] Send Telementry for AppMode Fail ...");*/
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
                    _pwr_Mon.SystemSuspend += OnSystemSuspend;
                    _pwr_Mon.SystemResume += OnSystemResume;
                    _pwr_Mon.Enable_Event();
                    _pwr_Mon.HotkeyPressed += HotkeyPressed;
                    _pwr_Mon.Enable_HotkeyHook();
                }
                System.Windows.Threading.Dispatcher.Run();
            });

            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();

            _disDevHelper = new DisplayDeviceHelper(Log);
        }

        private void HotkeyPressed(object sender, KeyPressedEventArgs e)
        {
            Debug.WriteLine($"HotkeyPressed===>id:{e.HotkeyInfo.ID},key:{e.KeyString}");
            if (isBypassHotkey)
            {
                writelog($"bypass HotkeyPressed,id={e.HotkeyInfo.ID},key:{e.KeyString}");
                return;
            }
            if (_hotkeySettings != null && _hotkeySettings.Count > 0)
            {
                foreach (var settings in _hotkeySettings)
                {
                    if (settings.HotkeyInfo != null)
                    {
                        HotkeyInfo hotkeyInfo = settings.HotkeyInfo.SingleOrDefault(x => x.ID.Equals(e.HotkeyInfo.ID));
                        if (hotkeyInfo != null && hotkeyInfo.Job != HotkeyType.None)
                        {
                            Debug.WriteLine($"[HotkeyPressed]Job matched:{hotkeyInfo.Job} => {hotkeyInfo.Description}: Hotkey(id:{hotkeyInfo.ID}){e.KeyString} => setting key:{string.Join("+", hotkeyInfo.Hotkey.Select(x => x + "(" + (int)x + ")").ToList())}");
                            writelog($"[HotkeyPressed]Job matched:{hotkeyInfo.Job} => {hotkeyInfo.Description}: Hotkey(id:{hotkeyInfo.ID}){e.KeyString} => setting key: {string.Join("+", hotkeyInfo.Hotkey.Select(x => x + "(" + (int)x + ")").ToList())}");
                            ExecHotkeyJob(settings, hotkeyInfo.Job);
                        }
                        else
                        {
                            writelog($"[HotkeyPressed]No Job matched:Hotkey(id:{hotkeyInfo.ID}):{e.KeyString}");
                        }
                    }
                    else
                    {
                        writelog($"[HotkeyPressed](id:{e.HotkeyInfo.ID}):{e.KeyString},HotkeyInfo is null");
                    }
                }
            }
            else
            {
                if (_hotkeySettings != null)
                {
                    Debug.WriteLine($"HotkeyPressed[job fail],id:{e.HotkeyInfo.ID},key:{e.KeyString} ==> _hotkeySettings:count = {_hotkeySettings.Count}");
                }
                else
                {
                    Debug.WriteLine($"HotkeyPressed[job fail],id:{e.HotkeyInfo.ID},key:{e.KeyString} ==> _hotkeySettings is null");
                }
            }
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

        /// <summary>
        /// A general event to UI from EABroker. Subagents can trigger this event with SentEANotify()
        /// </summary>
        public event EventHandler<EAArgs> EANotify;

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

        //Derek 1209
        //public event EventHandler GlobalSettingChangeEvent;
        public event EventHandler<UpdateUINotify> GlobalSettingChangeEvent;

        public event EventHandler SystemSuspend;

        public event EventHandler SystemResume;

        public event EventHandler SystemSessionEnd;

        #endregion

        #region ColorPreset implementation

        public Task<DDPM.SA.Common.IIC_Metadata> DownloadICCData(MonitorInfo m, bool blICCProfile = false, string savelPath = "")
        {
            DDPM.SA.Common.IIC_Metadata _ICC_Metadata = new DDPM.SA.Common.IIC_Metadata();

            if (_ColorPresetPlugin == null)
            {
                writelog("null _ColorPresetPlugin in [DownloadICCData]");
                return Task.FromResult(_ICC_Metadata);
            }
            else
            {
                _ICC_Metadata = _ColorPresetPlugin.DownloadICCData(m, _SettingsPlugin, blICCProfile, savelPath).Result;
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
        public async Task<bool> WriteColorPreset(MonitorInfo m, string ColorPreset_Name, int colorPresetRunType = 0, bool blIs_Game_DeviceName = false, bool blSmartHDR_ON = false, string reqAppName = null, bool showOSD = true)
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
            {
                r = _ColorPresetPlugin.WriteColorPreset(m, ColorPreset_Name, _SettingsPlugin, colorPresetRunType).Result;

                if (r == false)
                {
                    PopupContentPackage popupContentPackage = new PopupContentPackage()
                    {
                        Title = Strings.Dell_Display_and_Peripheral_Manager0,
                        Info = Strings.Unable_to_synchronize_the_corresponding_ICC_profile0 + m.modelName,
                        IsInfo = true,
                        IsOnlyUpdate = false,
                        StayOpen = false,
                        Timeout = 5,
                    };
                    CallPopup(this, popupContentPackage);
                }
            }
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

            // jim add  for The DDPM color profile can not be applied by DDPM on Smart HDR mode.(Gaming monitor ex: AW2724DM)
            if (blIs_Game_DeviceName && blSmartHDR_ON)
            {
                try
                {
                    uint title = (uint)Gaming_Supported.HDRType;
                    uint param = 0;

                    if (string.Equals(ColorPreset_Name, "Off", StringComparison.OrdinalIgnoreCase))
                        param = 0x00;
                    else if (string.Equals(ColorPreset_Name, "Desktop", StringComparison.OrdinalIgnoreCase))
                        param = 0x01;
                    else if (string.Equals(ColorPreset_Name, "Movie HDR", StringComparison.OrdinalIgnoreCase))
                        param = 0x02;
                    else if (string.Equals(ColorPreset_Name, "Game HDR", StringComparison.OrdinalIgnoreCase))
                        param = 0x03;
                    else if (string.Equals(ColorPreset_Name, "DisplayHDR", StringComparison.OrdinalIgnoreCase))
                        param = 0x04;
                    else if (string.Equals(ColorPreset_Name, "Custom Color HDR", StringComparison.OrdinalIgnoreCase))
                        param = 0x05;
                    else if (string.Equals(ColorPreset_Name, "HDR Peak 1000", StringComparison.OrdinalIgnoreCase))
                        param = 0x06;
                    else if (string.Equals(ColorPreset_Name, "Disable", StringComparison.OrdinalIgnoreCase))
                        param = 0x0E;

                    writelog($"[DeviceMangerPlugin] {nameof(SetGaming_HDRType)} title : {title}, param : {param}");
                    r = SetVCPCapability(m, VcpCodeList.VCPctr["Gaming"], title + param).Result;
                }
                catch (Exception ex)
                {
                    writelog($"[DeviceMangerPlugin] {nameof(SetGaming_HDRType)} Error : {ex.ToString()}");
                }
            }
            else
            {
                if (string.Equals(m.modelName, "G2723H", StringComparison.OrdinalIgnoreCase)) // Jim 20241211 to fix PIMS-327396 - The DDPM color profile list is not matching exactly with OSD.(G2723H)
                {
                    if (string.Equals(ColorPreset_Name, "FPS Game", StringComparison.OrdinalIgnoreCase))
                        r = SetVCPCapability(m, VcpCodeList.VCPctr["HDR Modes Specific"], VcpCodeList.VCPF0["FPS Game"]).Result;
                    else if (string.Equals(ColorPreset_Name, "RTS Game", StringComparison.OrdinalIgnoreCase))
                        r = SetVCPCapability(m, VcpCodeList.VCPctr["HDR Modes Specific"], VcpCodeList.VCPF0["RTS Game"]).Result;
                    else if (string.Equals(ColorPreset_Name, "RPG Game", StringComparison.OrdinalIgnoreCase))
                        r = SetVCPCapability(m, VcpCodeList.VCPctr["HDR Modes Specific"], VcpCodeList.VCPF0["RPG Game"]).Result;
                    else if (string.Equals(ColorPreset_Name, "SPORTS Game", StringComparison.OrdinalIgnoreCase))
                        r = SetVCPCapability(m, VcpCodeList.VCPctr["HDR Modes Specific"], VcpCodeList.VCPF0["SPORTS Game"]).Result;
                    else if (string.Equals(ColorPreset_Name, "Game2", StringComparison.OrdinalIgnoreCase))
                        r = SetVCPCapability(m, VcpCodeList.VCPctr["HDR Modes Specific"], VcpCodeList.VCPF0["Game2"]).Result;
                    else if (string.Equals(ColorPreset_Name, "Game3", StringComparison.OrdinalIgnoreCase))
                        r = SetVCPCapability(m, VcpCodeList.VCPctr["HDR Modes Specific"], VcpCodeList.VCPF0["Game3"]).Result;
                    else if (string.Equals(ColorPreset_Name, "Game1", StringComparison.OrdinalIgnoreCase))
                        r = SetVCPCapability(m, VcpCodeList.VCPctr["Display Application"], VcpCodeList.VCPDC["Game1"]).Result;
                    else
                        r = await Task.Run(() => SetVCPCapability(m, "colorpreset", ColorPreset_Name).Result).ConfigureAwait(false);
                }
                else if (string.Equals(m.modelName, "AW3225QF", StringComparison.OrdinalIgnoreCase)) // Jim 20241211 to fix PIMS-326656 - The DDPM color profile list is not matching exactly with OSD. "Game1" not show in DDPM.(AW3225QF)
                {
                    if (string.Equals(ColorPreset_Name, "Game1", StringComparison.OrdinalIgnoreCase))
                        r = SetVCPCapability(m, VcpCodeList.VCPctr["Display Application"], Convert.ToUInt32(VcpCodeList.VCPDC["Game1"])).Result;
                    else
                        r = await Task.Run(() => SetVCPCapability(m, "colorpreset", ColorPreset_Name).Result).ConfigureAwait(false);
                }
                else if (string.Equals(m.modelName, "G2724D", StringComparison.OrdinalIgnoreCase) || string.Equals(m.modelName, "G3223D", StringComparison.OrdinalIgnoreCase))
                {
                    if (string.Equals(ColorPreset_Name, "sRGB", StringComparison.OrdinalIgnoreCase))
                        r = SetVCPCapability(m, VcpCodeList.VCPctr["Basic Color Preset Select"], Convert.ToUInt32(VcpCodeList.VCP14["sRGB"])).Result;
                    else
                        r = await Task.Run(() => SetVCPCapability(m, "colorpreset", ColorPreset_Name).Result).ConfigureAwait(false);
                }
                else
                {
                    r = await Task.Run(() => SetVCPCapability(m, "colorpreset", ColorPreset_Name).Result).ConfigureAwait(false);
                    Trace.Write($"r = {r}");
                }

                writelog($"[DeviceMangerPlugin] WriteColorPreset {nameof(SetVCPCapability)} r = {r}");
            }

            // 11/23 Wayn Add
            bool result = false;
            result = await Task.Run(() => SyncPrimaryMonitorAndColorPresetStatus(m, ColorPreset_Name, colorPresetRunType).Result).ConfigureAwait(false);
            if (!result)
            {
                writelog("[DeviceMangerPlugin] SyncPrimaryMonitorAndColorPresetStatus ... False");
            }

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
                    if (rt)
                        writelog("[DeviceMangerPlugin] Send Telementry for Color_Preset_Auto Success ...");
                    else
                        writelog("[DeviceMangerPlugin] Send Telementry for Color_Preset_Auto Fail ...");
                }
            }
            else //Manual
            {
                if (!string.IsNullOrEmpty(ColorPreset_Name))
                {
                    writelog("[DeviceMangerPlugin] Send Telementry for Color_Preset_Manual...");
                    rt = Displaysettings_Function.Send_Color_Preset_Manual_Telementry(_TelementryScheduler, m, ColorPreset_Name, GetMonitorCurrentResolution(m), GetMonitorMaxResolution(m));
                    if (rt)
                        writelog("[DeviceMangerPlugin] Send Telementry for Color_Preset_Manual Success ...");
                    else
                        writelog("[DeviceMangerPlugin] Send Telementry for Color_Preset_Manual Fail ...");
                }
            }

            return r;
        }

        public Task<bool> SyncPrimaryMonitorAndColorPresetStatus(MonitorInfo m, string ColorPreset_Name, int colorPresetRunType)
        {
            bool blRet = true;
            writelog("[DeviceMangerPlugin] SyncPrimaryMonitorAndColorPresetStatus ... in");
            List<ALSConfig> existAlsConfig = _DisplayManagerPlugin.GetAllExistAlsConfig().Result;
            Trace.WriteLine($"SyncPrimaryMonitorAndColorPresetStatus = {m.edid.ModelName.ToString()} || existAlsConfig.Count = {existAlsConfig.Count.ToString()}");
            ALSConfig findconfig = existAlsConfig.Find(x => x.Edid.Equals(m.edid));

            Trace.WriteLine("Into MonitorInfo = " + m.edid.ModelName.ToString());
            for (int i = 0; i < existAlsConfig.Count; i++)
            {
                Trace.WriteLine("GetAllExistAlsConfig ModelName = " + i.ToString() + " : " + existAlsConfig[i].ModelName.ToString());
            }
            if (findconfig != null)
            {
                Trace.WriteLine("findconfig.ModelName = " + findconfig.ModelName.ToString() + " ; & findconfig.isPrimaryMonitorSync = " + findconfig.isPrimaryMonitorSync.ToString());
                if (findconfig != null && findconfig.isPrimaryMonitorSync)// Find PrimaryMonitorSync on Monitor
                {
                    for (int i = 0; i < _AllInfoMonitors.Count; i++)// Compare AllInfoMonitors
                    {
                        MonitorInfo mo = _AllInfoMonitors[i];

                        if (m.edid.Equals(mo.edid) == false)// Find different Monitor with PrimaryMonitorSync on Monitor
                        {
                            Trace.WriteLine("e.monitor.ModelName = " + m.edid.ModelName.ToString() + " ||  mo.ModelName = " + mo.edid.ModelName.ToString());
                            writelog("[DeviceMangerPlugin] SyncPrimaryMonitorAndColorPresetStatus ..... SetVCPCapability => " + mo.edid.ModelName.ToString() + ColorPreset_Name);
                            Task.Run(() => SetVCPCapability(mo, "colorpreset", ColorPreset_Name).Result).ConfigureAwait(false);
                        }
                    }
                }
                return Task.FromResult(blRet);
            }
            else
            {
                writelog("[DeviceMangerPlugin] SyncPrimaryMonitorAndColorPresetStatus ..... findconfig == null");
                return Task.FromResult(false);
            }
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
                if (rt)
                    writelog("[DeviceMangerPlugin] Send Telementry for NightLightStatus Success ...");
                else
                    writelog("[DeviceMangerPlugin] Send Telementry for NightLightStatus Fail ...");
            }

            return Task.FromResult(blRet);
        }

        public Task<bool> Send_NightLightschedulerStatus_Telementry_SA(MonitorInfo m, string NightLightschedulerStatus)
        {
            writelog("DeviceManagerPlugin received Send_NightLightschedulerStatus_Telementry_SA requested ...");

            bool blRet = true;

            var rt = false;
            var Displaysettings_Function = new Displaysettings_Function();

            if (!string.IsNullOrEmpty(NightLightschedulerStatus))
            {
                writelog("[DeviceMangerPlugin] Send Telementry for NightLightschedulerStatus...");
                rt = Displaysettings_Function.Send_NightLightschedulerStatus_Telementry(_TelementryScheduler, m, NightLightschedulerStatus, GetMonitorCurrentResolution(m), GetMonitorMaxResolution(m));
                if (rt)
                    writelog("[DeviceMangerPlugin] Send Telementry for NightLightschedulerStatus Success ...");
                else
                    writelog("[DeviceMangerPlugin] Send Telementry for NightLightschedulerStatus Fail ...");
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
        /// 啟動監視NightLight Scheduler Status
        /// </summary>
        public Task<bool> CheckNightLightScheduler()
        {
            writelog("DeviceManagerPlugin received CheckNightLightScheduler requested ...");

            if (_ColorPresetPlugin == null)
            {
                writelog("null _ColorPresetPlugin in [DeviceManagerPlugin - CheckNightLightScheduler]");
                return Task.FromResult(false);
            }

            var temp = _ColorPresetPlugin.CheckNightLightScheduler().Result;

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
        /// 停止監視NightLight Scheduler Status
        /// </summary>
        public Task<bool> StopRegistryMonitor_NightLightScheduler()
        {
            writelog("DeviceManagerPlugin received StopRegistryMonitor_NightLightScheduler requested ...");

            if (_ColorPresetPlugin == null)
            {
                writelog("null _ColorPresetPlugin in [DeviceManagerPlugin - StopRegistryMonitor_NightLightScheduler]");
                return Task.FromResult(false);
            }

            var temp = _ColorPresetPlugin.StopRegistryMonitor_NightLightScheduler().Result;

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
        public Task<bool> AutoSetColorPresetForMonitorConfig(MonitorInfo mo, string on_off, bool Is_Game_DeviceName = false, bool Islock = false)
        {
            writelog("DeviceManagerPlugin received AutoSetColorPresetForMonitorConfig requested ...");

            if (_ColorPresetPlugin == null)
            {
                writelog("null _ColorPresetPlugin in [DeviceManagerPlugin - AutoSetColorPresetForMonitorConfig]");
                return Task.FromResult(false);
            }

            bool SmartHDR_ON = GetHDRStatus(mo).Result;

            var temp = _ColorPresetPlugin.AutoSetColorPresetForMonitorConfig(mo, on_off, _SettingsPlugin, this, Is_Game_DeviceName, SmartHDR_ON).Result;

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

                if (_AllInfoMonitors != null)
                    _AllInfoMonitors.Clear();

                writelog("DeviceMangerPlugin received GetMonitors requested ...");
                //if (_AllInfoMonitorsRecord.Count == 0)
                //{
                if (_DisplayManagerPlugin == null)
                {
                    writelog("null _DisplayManagerPlugin in [GetMonitors], retrun empty monitor list");
                    return Task.FromResult(new List<MonitorInfo>());
                }

                _AllInfoMonitors = new List<MonitorInfo>(_DisplayManagerPlugin.GetMonitors().Result);

                //review monitor list to check duplicated data
                ReviewAllMonitorToAvoidDuplicatedInfo();

                InitMonitorSettings();

                Task.Run(() => //support last selected monitor info from settings
                {
                    if (_SettingsPlugin != null)
                    {
                        try
                        {
                            DDPMSettings data = _SettingsPlugin.ReloadAppConfigData().Result;
                            if (data != null && data.UserSettings != null)
                            {
                                DDPMSimpleMonitorRecord mo = data.UserSettings.lastUISelectedMonitor;
                                if (mo != null && !string.IsNullOrEmpty(mo.ModelName) && !string.IsNullOrEmpty(mo.ServiceTag))
                                {
                                    if (_AllInfoMonitors != null && _AllInfoMonitors.Count > 0)
                                    {
                                        int idx = _AllInfoMonitors.FindIndex(x => x.modelName.Equals(mo.ModelName) && x.edid.ServiceTag.Equals(mo.ServiceTag));
                                        if (idx >= 0)
                                            lastSelectedMonitor_UI = _AllInfoMonitors[idx];
                                        else
                                            lastSelectedMonitor_UI = null;
                                    }
                                }
                                else
                                    throw new ArgumentNullException("lastUISelectedMonitor");
                            }
                            else
                                throw new ArgumentNullException("data");
                        }
                        catch (Exception ex)
                        {
                            writelog($"Read last selected monitor from settings failed. ({ex.Message})");
                        }
                    }
                });

                return Task.FromResult(_AllInfoMonitors);
            }
        }

        public Task<List<MonitorInfo>> Re_GetMonitors()
        {
            writelog("DeviceMangerPlugin received Re_GetMonitors requested ...");
            _SystemEvents_DisplaySettingsChanged(null);
            return Task.FromResult(_AllInfoMonitors.ToList());
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

            //Telementry Collection
            var rt = false;
            var Displaysettings_Function = new Displaysettings_Function();
            switch (code)
            {
                case 0x10:

                    if (monitorInfo.CapabilityDic.ContainsKey("12"))
                    {
                        Task.Run(() =>
                        {
                            writelog("[DeviceMangerPlugin] Send Telementry for Brightness...");
                            rt = Displaysettings_Function.Send_Brightness_Telementry(_TelementryScheduler, monitorInfo, val, GetMonitorCurrentResolution(monitorInfo), GetMonitorMaxResolution(monitorInfo));
                            if (rt)
                                writelog("[DeviceMangerPlugin] Send Telementry for Brightness Success ...");
                            else
                                writelog("[DeviceMangerPlugin] Send Telementry for Brightness Fail ...");
                        }).ConfigureAwait(false);
                    }
                    else
                    {
                        Task.Run(() =>
                        {
                            writelog("[DeviceMangerPlugin] [Telementry] Send Telementry for Luminanc...");
                            rt = Displaysettings_Function.Send_Luminance_Telementry(_TelementryScheduler, monitorInfo, val, GetMonitorCurrentResolution(monitorInfo), GetMonitorMaxResolution(monitorInfo));
                            if (rt)
                                writelog("[DeviceMangerPlugin] [Telementry] Send  Telementry for Luminanc Success ...");
                            else
                                writelog("[DeviceMangerPlugin] [Telementry] Send  Telementry for Luminanc Fail ...");
                        }).ConfigureAwait(false);
                    }
                    break;

                case 0x12:

                    Task.Run(() =>
                    {
                        writelog("[DeviceMangerPlugin] [Telementry] Send Telementry for Contrast...");
                        rt = Displaysettings_Function.Send_Contrast_Telementry(_TelementryScheduler, monitorInfo, val, GetMonitorCurrentResolution(monitorInfo), GetMonitorMaxResolution(monitorInfo));
                        if (rt)
                            writelog("[DeviceMangerPlugin] [Telementry] Send Telementry for Contrast Success ...");
                        else
                            writelog("[DeviceMangerPlugin] [Telementry] Send Telementry for Contrast Fail ...");
                    }).ConfigureAwait(false);
                    break;

                case 0xE9:
                    {
                        NKVMVCPValue nKVMVCPValue = new NKVMVCPValue();
                        nKVMVCPValue.monitorInfo = monitorInfo;
                        nKVMVCPValue.value = (int)val;
                        if (_NKVMPlugin != null)
                        {
                            _NKVMPlugin.SaveVCPcode(nKVMVCPValue);
                        }
                    }
                    break;

                default:
                    break;
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
                                        //copyinputlist = inputSourcelist;
                                        //foreach (var input in inputSourcelist)
                                        //{
                                        //    string usbUpstream = GetUSBUpstream(monitorInfo, input.Key).Result;
                                        //    if (string.IsNullOrEmpty(usbUpstream))
                                        //    {
                                        //        writelog("[DeviceMangerPlugin] usbUpstream is null or empty ...");
                                        //    }
                                        //    else
                                        //    {
                                        //        input.Value.USBUpstream = usbUpstream;
                                        //    }
                                        //    //Maybe Migration...
                                        //    //if (input.Value.USBUpstream == string.Empty && monitorInfo.CapabilityDic.ContainsKey("EE") && monitorInfo.CapabilityDic.ContainsKey("E7"))
                                        //    //{
                                        //    //readinputlist = _DisplayManagerPlugin.GetInputSourcelist(monitorInfo).Result;
                                        //    //if (readinputlist != null)
                                        //    //{
                                        //    //    if (readinputlist.Count != 0)
                                        //    //    {
                                        //    //        foreach (var readinput in readinputlist)
                                        //    //        {
                                        //    //            foreach (var copyinput in copyinputlist)
                                        //    //            {
                                        //    //                if (readinput.Value.Code == copyinput.Value.Code)
                                        //    //                {
                                        //    //                    readinput.Value.InputName = copyinput.Value.InputName;
                                        //    //                    break;
                                        //    //                }
                                        //    //            }
                                        //    //        }
                                        //    //        bool b1 = SetInputSourcelist(monitorInfo, readinputlist).Result;
                                        //    //        return Task.FromResult(readinputlist);
                                        //    //    }
                                        //    //}
                                        //    //break;
                                        //    //}
                                        //}
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
            if (_DisplayManagerPlugin != null)
            {
                if (_DisplayManagerPlugin.SetUSBUpstream(monitorInfo, inputsource, upstream).Result)
                {
                    //Telementry Collection
                    var Displaysettings_Function = new Displaysettings_Function();
                    if (Displaysettings_Function.Send_USB_Telementry(_TelementryScheduler, monitorInfo, upstream, GetMonitorCurrentResolution(monitorInfo), GetMonitorMaxResolution(monitorInfo)))
                    {
                        writelog("[SetUSBUpstream] [Telementry] Send Telementry for USB Association Success ...");
                    }
                    else
                    {
                        writelog("[SetUSBUpstream] [Telementry] Send Telementry for USB Association Fail ...");
                    }
                    return Task.FromResult(true);
                }
            }
            else
            {
                writelog("[SetUSBUpstream] _DisplayManagerPlugin is null.");
            }

            return Task.FromResult(false);
        }

        public Task<string> GetUSBUpstream(MonitorInfo monitorInfo, string inputsource)
        {
            string usbUpstream = string.Empty;
            if (_DisplayManagerPlugin != null)
            {
                usbUpstream = _DisplayManagerPlugin.GetUSBUpstream(monitorInfo, inputsource).Result;
                if (!string.IsNullOrWhiteSpace(usbUpstream))
                {
                    Dictionary<string, InputInfo> inputSourceList = GetInputSourcelist(monitorInfo).Result;
                    if (inputSourceList != null)
                    {
                        InputInfo outinput = new InputInfo();
                        if (inputSourceList.TryGetValue(inputsource, out outinput))
                        {
                            inputSourceList[inputsource].USBUpstream = usbUpstream;
                            //string strInputList = InputSourceListSerialize(inputSourceList);
                            if (SetInputSourcelist(monitorInfo, inputSourceList).Result)
                            {
                                return Task.FromResult(usbUpstream);
                            }
                        }
                    }
                }
                else
                {
                    writelog("[GetUSBUpstream] USBUpstream is null or empty.");
                }
            }
            else
            {
                writelog("[GetUSBUpstream] _DisplayManagerPlugin is null.");
            }

            return Task.FromResult(usbUpstream);
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

        public Task<bool> isScreenPartition(MonitorInfo monitorInfo)
        {
            if (_DisplayManagerPlugin != null)
            {
                return _DisplayManagerPlugin.isScreenPartition(monitorInfo);
            }
            return Task.FromResult(false);
        }

        #endregion

        #endregion

        #region Peripherals implementation

        public async Task<DeviceHelper> GetDevices(bool Rescan = false)
        {
            DeviceHelper deviceHelper = await Task.Run(() => _PeripheralsPlugin.GetDevices(Rescan));
            ChangeSB725(deviceHelper);
            ChangeHeadset(deviceHelper);
            ChangeDock(deviceHelper);
            return await Task.Run(() => _PeripheralsPlugin.GetDevices(Rescan));
        }

        private static void ChangeSB725(DeviceHelper deviceHelper)
        {
            if (deviceHelper.deviceInfo.Any(x => x.Name.Contains("SB725")))
            {
                var GetDeviceInfos = deviceHelper.deviceInfo.Where(x => x.Name.Contains("SB725")).ToList();
                foreach (var deviceInfo in GetDeviceInfos)
                {
                    deviceInfo.Type = DeviceType.LogicalWiredAudio;
                    deviceInfo.ModelNumber = "SB725";
                    deviceInfo.LogicalDeviceType = "LogicalWiredAudio";
                }
            }
        }

        private void ChangeHeadset(DeviceHelper deviceHelper)
        {
            writelog("ChangeHeadset start");
            List<DeviceInfo> GetDeviceInfos = deviceHelper.deviceInfo.Where(x => x.LogicalDeviceType == "LogicalHeadset").ToList();
            if (GetDeviceInfos != null && GetDeviceInfos.Count >= 1)
            {
                writelog("ChangeHeadset go");
                foreach (var deviceInfo in GetDeviceInfos)
                {
                    string version = GetHeadsetFirmwareVersionAsync(deviceInfo.ID.ToString()).Result;
                    writelog($"GetHeadsetFirmwareVersionAsync : {version}");
                    if (!string.IsNullOrEmpty(version))
                    {
                        deviceInfo.FirmwareVersion = version;
                    }
                }
            }
            writelog("ChangeHeadset done");
        }

        private void ChangeDock(DeviceHelper deviceHelper)
        {
            writelog("ChangeDock start");
            List<DeviceInfo> GetDeviceInfos = deviceHelper.deviceInfo.FindAll(x => x.PhysicalDeviceType.Equals(DeviceType.LogicalDock) || x.PhysicalDeviceType.Equals(DeviceType.PhysicalWiredDock));
            if (GetDeviceInfos != null && GetDeviceInfos.Count >= 1)
            {
                writelog("ChangeDock go");
                foreach (var deviceInfo in GetDeviceInfos)
                {
                    string version = GetFirmwareVersionForDock(deviceInfo.ID.ToString()).Result;
                    string serviceTag = GetDockServiceTagForDock(deviceInfo.ID.ToString()).Result;
                    writelog($"GetFirmwareVersionForDock : {version}");
                    writelog($"GetDockServiceTagForDock : {serviceTag}");
                    if (!string.IsNullOrEmpty(version))
                    {
                        deviceInfo.DockPackageFwVersion = version;
                        deviceInfo.FirmwareVersion = version;
                    }
                    if (!string.IsNullOrEmpty(serviceTag))
                    {
                        deviceInfo.DockServiceTag = serviceTag;
                    }
                }
            }
            writelog("ChangeDock done");
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

        public Task SetMicNoiseCancellationForMito(bool newValue, Guid deviceId)
        {
            writelog("DeviceMangerPlugin received SetMicNoiseCancellationForMito requested ...");
            writelog($"Target DeviceID is {deviceId}");
            _PeripheralsPlugin.SetMicNoiseCancellationForMito(newValue, deviceId);
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
            return Task.CompletedTask;
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
                bool result = await _DTPProxyPlugin.SetMicNoiseCancellationAsync(guid, newValue);
                if (result)
                    writelog($"[DeviceManagerPlugin] [Headset] SetMicNoiseCancellationAsync Success");
                else
                    writelog($"[DeviceManagerPlugin] [Headset] SetMicNoiseCancellationAsync Fail");
                return result;
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
                bool result = await _DTPProxyPlugin.SetSidetoneAsync(guid, newValue);
                if (result)
                    writelog($"[DeviceManagerPlugin] [Headset] SetSidetoneAsync Success");
                else
                    writelog($"[DeviceManagerPlugin] [Headset] SetSidetoneAsync Fail");
                return result;
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
                bool result = await _DTPProxyPlugin.SetBusyLightAsync(guid, newValue);
                if (result)
                    writelog($"[DeviceManagerPlugin] [Headset] SetBusyLightAsync Success");
                else
                    writelog($"[DeviceManagerPlugin] [Headset] SetBusyLightAsync Fail");
                return result;
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
                bool result = await _DTPProxyPlugin.SetVoiceGuidanceAsync(guid, newValue);
                if (result)
                    writelog($"[DeviceManagerPlugin] [Headset] SetVoiceGuidanceAsync Success");
                else
                    writelog($"[DeviceManagerPlugin] [Headset] SetVoiceGuidanceAsync Fail");
                return result;
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
                bool result = await _DTPProxyPlugin.SetSelectedPresetAsync(guid, newValue);
                if (result)
                    writelog($"[DeviceManagerPlugin] [Headset] SetSelectedPresetAsync Success");
                else
                    writelog($"[DeviceManagerPlugin] [Headset] SetSelectedPresetAsync Fail");
                return result;
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
                bool result = await _DTPProxyPlugin.SetSidetoneLevelAsync(guid, newValue);
                if (result)
                    writelog($"[DeviceManagerPlugin] [Headset] SetSidetoneLevelAsync Success");
                else
                    writelog($"[DeviceManagerPlugin] [Headset] SetSidetoneLevelAsync Fail");
                return result;
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
                bool result = await _DTPProxyPlugin.SetBandsGainAsync(guid, newValue);
                if (result)
                    writelog($"[DeviceManagerPlugin] [Headset] SetBandsGainAsync Success");
                else
                    writelog($"[DeviceManagerPlugin] [Headset] SetBandsGainAsync Fail");
                return result;
            }
            catch (Exception ex)
            {
                writelog($"[DeviceManagerPlugin] [Headset] SetBandsGainAsync failed for GUID: {guid}, Error: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SetBand1GainAsync(string guid, int newValue)
        {
            try
            {
                bool result = await _DTPProxyPlugin.SetBand1GainAsync(guid, newValue);
                if (result)
                    writelog($"[DeviceManagerPlugin] [Headset] SetBand1GainAsync Success");
                else
                    writelog($"[DeviceManagerPlugin] [Headset] SetBand1GainAsync Fail");
                return result;
            }
            catch (Exception ex)
            {
                writelog($"[DeviceManagerPlugin] [Headset] SetBand1GainAsync failed for GUID: {guid}, Error: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SetBand2GainAsync(string guid, int newValue)
        {
            try
            {
                bool result = await _DTPProxyPlugin.SetBand2GainAsync(guid, newValue);
                if (result)
                    writelog($"[DeviceManagerPlugin] [Headset] SetBand2GainAsync Success");
                else
                    writelog($"[DeviceManagerPlugin] [Headset] SetBand2GainAsync Fail");
                return result;
            }
            catch (Exception ex)
            {
                writelog($"[DeviceManagerPlugin] [Headset] SetBand2GainAsync failed for GUID: {guid}, Error: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SetBand3GainAsync(string guid, int newValue)
        {
            try
            {
                bool result = await _DTPProxyPlugin.SetBand3GainAsync(guid, newValue);
                if (result)
                    writelog($"[DeviceManagerPlugin] [Headset] SetBand3GainAsync Success");
                else
                    writelog($"[DeviceManagerPlugin] [Headset] SetBand3GainAsync Fail");
                return true;
            }
            catch (Exception ex)
            {
                writelog($"[DeviceManagerPlugin] [Headset] SetBand3GainAsync failed for GUID: {guid}, Error: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SetBand4GainAsync(string guid, int newValue)
        {
            try
            {
                bool result = await _DTPProxyPlugin.SetBand4GainAsync(guid, newValue);
                if (result)
                    writelog($"[DeviceManagerPlugin] [Headset] SetBand4GainAsync Success");
                else
                    writelog($"[DeviceManagerPlugin] [Headset] SetBand4GainAsync Fail");
                return result;
            }
            catch (Exception ex)
            {
                writelog($"[DeviceManagerPlugin] [Headset] SetBand4GainAsync failed for GUID: {guid}, Error: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SetBand5GainAsync(string guid, int newValue)
        {
            try
            {
                bool result = await _DTPProxyPlugin.SetBand5GainAsync(guid, newValue);
                if (result)
                    writelog($"[DeviceManagerPlugin] [Headset] SetBand5GainAsync Success");
                else
                    writelog($"[DeviceManagerPlugin] [Headset] SetBand5GainAsync Fail");
                return result;
            }
            catch (Exception ex)
            {
                writelog($"[DeviceManagerPlugin] [Headset] SetBand5GainAsync failed for GUID: {guid}, Error: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SetAncModeAsync(string guid, int newValue)
        {
            try
            {
                bool result = await _DTPProxyPlugin.SetAncModeAsync(guid, newValue);
                if (result)
                    writelog($"[DeviceManagerPlugin] [Headset] SetAncModeAsync Success");
                else
                    writelog($"[DeviceManagerPlugin] [Headset] SetAncModeAsync Fail");
                return result;
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
                bool result = await _DTPProxyPlugin.SetAncGainAsync(guid, newValue);
                if (result)
                    writelog($"[DeviceManagerPlugin] [Headset] SetAncGainAsync Success");
                else
                    writelog($"[DeviceManagerPlugin] [Headset] SetAncGainAsync Fail");
                return result;
            }
            catch (Exception ex)
            {
                writelog($"[DeviceManagerPlugin] [Headset] SetAncGainAsync failed for GUID: {guid}, Error: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SetWearDetectionAsync(string guid, bool newValue)
        {
            try
            {
                bool result = await _DTPProxyPlugin.SetWearDetectionAsync(guid, newValue);
                if (result)
                    writelog($"[DeviceManagerPlugin] [Headset] SetWearDetectionAsync Success");
                else
                    writelog($"[DeviceManagerPlugin] [Headset] SetWearDetectionAsync Fail");
                return result;
            }
            catch (Exception ex)
            {
                writelog($"[DeviceManagerPlugin] [Headset] SetWearDetectionAsync failed for GUID: {guid}, Error: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SetIsWearDetectionMuteMicEnabledAsync(string guid, bool newValue)
        {
            try
            {
                bool result = await _DTPProxyPlugin.SetIsWearDetectionMuteMicEnabledAsync(guid, newValue);
                if (result)
                    writelog($"[DeviceManagerPlugin] [Headset] SetIsWearDetectionMuteMicEnabledAsync Success");
                else
                    writelog($"[DeviceManagerPlugin] [Headset] SetIsWearDetectionMuteMicEnabledAsync Fail");
                return result;
            }
            catch (Exception ex)
            {
                writelog($"[DeviceManagerPlugin] [Headset] SetIsWearDetectionMuteMicEnabledAsync failed for GUID: {guid}, Error: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SetIsWearDetectionPauseMusicEnabledAsync(string guid, bool newValue)
        {
            try
            {
                bool result = await _DTPProxyPlugin.SetIsWearDetectionPauseMusicEnabledAsync(guid, newValue);
                if (result)
                    writelog($"[DeviceManagerPlugin] [Headset] SetIsWearDetectionPauseMusicEnabledAsync Success");
                else
                    writelog($"[DeviceManagerPlugin] [Headset] SetIsWearDetectionPauseMusicEnabledAsync Fail");
                return result;
            }
            catch (Exception ex)
            {
                writelog($"[DeviceManagerPlugin] [Headset] SetIsWearDetectionPauseMusicEnabledAsync failed for GUID: {guid}, Error: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SetWearDetectionQuickPauseAsync(string guid, int newValue)
        {
            try
            {
                bool result = await _DTPProxyPlugin.SetWearDetectionQuickPauseAsync(guid, newValue);
                if (result)
                    writelog($"[DeviceManagerPlugin] [Headset] SetWearDetectionQuickPauseAsync Success");
                else
                    writelog($"[DeviceManagerPlugin] [Headset] SetWearDetectionQuickPauseAsync Fail");
                return result;
            }
            catch (Exception ex)
            {
                writelog($"[DeviceManagerPlugin] [Headset] SetWearDetectionQuickPauseAsync failed for GUID: {guid}, Error: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SetWearDetectionSensitivityAsync(string guid, int newValue)
        {
            try
            {
                bool result = await _DTPProxyPlugin.SetWearDetectionSensitivityAsync(guid, newValue);
                if (result)
                    writelog($"[DeviceManagerPlugin] [Headset] SetWearDetectionSensitivityAsync Success");
                else
                    writelog($"[DeviceManagerPlugin] [Headset] SetWearDetectionSensitivityAsync Fail");
                return result;
            }
            catch (Exception ex)
            {
                writelog($"[DeviceManagerPlugin] [Headset] SetWearDetectionSensitivityAsync failed for GUID: {guid}, Error: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SetMicNCIncomingAsync(string guid, bool newValue)
        {
            try
            {
                bool result = await _DTPProxyPlugin.SetMicNCIncomingAsync(guid, newValue);
                if (result)
                    writelog($"[DeviceManagerPlugin] [Headset] SetMicNCIncomingAsync Success");
                else
                    writelog($"[DeviceManagerPlugin] [Headset] SetMicNCIncomingAsync Fail");
                return result;
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
                bool result = await _DTPProxyPlugin.SetUnPairAsync(guid, newValue);
                if (result)
                    writelog($"[DeviceManagerPlugin] [Headset] SetUnPairAsync Success");
                else
                    writelog($"[DeviceManagerPlugin] [Headset] SetUnPairAsync Fail");
                return result;
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
                bool result = await _DTPProxyPlugin.SetFactoryResetAsyncValueForHeadset(guid, newValue);
                if (result)
                    writelog($"[DeviceManagerPlugin] [Headset] SetFactoryResetAsyncValueForHeadset Success");
                else
                    writelog($"[DeviceManagerPlugin] [Headset] SetFactoryResetAsyncValueForHeadset Fail");
                return result;
            }
            catch (Exception ex)
            {
                writelog($"[DeviceManagerPlugin] [Headset] SetFactoryResetAsync failed for GUID: {guid}, Error: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SetBoomMicAsync(string guid, bool newValue)
        {
            try
            {
                bool result = await _DTPProxyPlugin.SetBoomMicAsync(guid, newValue);
                if (result)
                    writelog($"[DeviceManagerPlugin] [Headset] SetBoomMicAsync Success");
                else
                    writelog($"[DeviceManagerPlugin] [Headset] SetBoomMicAsync Fail");
                return result;
            }
            catch (Exception ex)
            {
                writelog($"[DeviceManagerPlugin] [Headset] SetBoomMicAsync failed for GUID: {guid}, Error: {ex.Message}");
                return false;
            }
        }

        //////////////////////////////////// Get//////////////////////////////////////////

        public async Task<JArray> GetHeadsetDeviceItemsExAsync()
        {
            try
            {
                var result = await _DTPProxyPlugin.GetHeadsetDeviceItemsExAsync();
                if (result != null)
                {
                    writelog($"[DeviceManagerPlugin] [Headset] GetDeviceItemsExAsync Success");
                    return result;
                }
                else
                {
                    writelog($"[DeviceManagerPlugin] [Headset] GetDeviceItemsExAsync value is null");
                    return null;
                }
            }
            catch (Exception ex)
            {
                writelog($"[DeviceManagerPlugin] [Headset] GetDeviceItemsExAsync failed - Exception: {ex.Message}");
                return null;
            }
        }

        public async Task<DeviceInterfaceType> GetHeadsetInterfaceTypeAsync(string guid)
        {
            try
            {
                var result = await _DTPProxyPlugin.GetHeadsetInterfaceTypeAsync(guid);
                writelog($"[DeviceManagerPlugin] [Headset] GetHeadsetInterfaceTypeAsync succeeded, value is {result.ToString()}");
                return result;
            }
            catch (Exception ex)
            {
                writelog($"[DeviceManagerPlugin] [Headset] GetHeadsetInterfaceTypeAsync failed for {guid} - Exception: {ex.Message}");
                return default(DeviceInterfaceType);
            }
        }

        public async Task<string> GetHeadsetDeviceNameAsync(string guid)
        {
            try
            {
                var result = await _DTPProxyPlugin.GetHeadsetDeviceNameAsync(guid);
                if (result != null)
                {
                    writelog($"[DeviceManagerPlugin] [Headset] GetDeviceNameAsync Success");
                    return result;
                }
                else
                {
                    writelog($"[DeviceManagerPlugin] [Headset] GetHeadsetDeviceNameAsync value is null");
                    return null;
                }
            }
            catch (Exception ex)
            {
                writelog($"[DeviceManagerPlugin] [Headset] GetDeviceNameAsync failed for {guid} - Exception: {ex.Message}");
                return null;
            }
        }

        public async Task<string> GetHeadsetDeviceIdAsync(string guid)
        {
            try
            {
                var result = await _DTPProxyPlugin.GetHeadsetDeviceIdAsync(guid);
                if (result != null)
                {
                    writelog($"[DeviceManagerPlugin] [Headset] GetHeadsetDeviceIdAsync Success");
                    return result;
                }
                else
                {
                    writelog($"[DeviceManagerPlugin] [Headset] GetHeadsetDeviceIdAsync value is null");
                    return null;
                }
            }
            catch (Exception ex)
            {
                writelog($"[DeviceManagerPlugin] [Headset] GetDeviceIdAsync failed for {guid} - Exception: {ex.Message}");
                return null;
            }
        }

        public async Task<string> GetHeadsetPluginIdAsync(string guid)
        {
            try
            {
                var result = await _DTPProxyPlugin.GetHeadsetPluginIdAsync(guid);
                if (result != null)
                {
                    writelog($"[DeviceManagerPlugin] [Headset] GetHeadsetPluginIdAsync Success");
                    return result;
                }
                else
                {
                    writelog($"[DeviceManagerPlugin] [Headset] GetHeadsetPluginIdAsync value is null");
                    return null;
                }
            }
            catch (Exception ex)
            {
                writelog($"[DeviceManagerPlugin] [Headset] GetPluginIdAsync failed for {guid} - Exception: {ex.Message}");
                return null;
            }
        }

        public async Task<int> GetHeadsetODMIdAsync(string guid)
        {
            try
            {
                var result = await _DTPProxyPlugin.GetHeadsetODMIdAsync(guid);
                if (result != -1)
                {
                    writelog($"[DeviceManagerPlugin] [Headset] GetHeadsetODMIdAsync Success");
                    return result;
                }
                else
                {
                    writelog($"[DeviceManagerPlugin] [Headset] GetHeadsetODMIdAsync value is -1");
                    return -1;
                }
            }
            catch (Exception ex)
            {
                writelog($"[DeviceManagerPlugin] [Headset] GetODMIdAsync failed for {guid} - Exception: {ex.Message}");
                return -1;
            }
        }

        public async Task<string> GetHeadsetModelNumberAsync(string guid)
        {
            try
            {
                var result = await _DTPProxyPlugin.GetHeadsetModelNumberAsync(guid);
                if (result != null)
                {
                    writelog($"[DeviceManagerPlugin] [Headset] GetHeadsetModelNumberAsync Success");
                    return result;
                }
                else
                {
                    writelog($"[DeviceManagerPlugin] [Headset] GetHeadsetModelNumberAsync value is null");
                    return null;
                }
            }
            catch (Exception ex)
            {
                writelog($"[DeviceManagerPlugin] [Headset] GetModelNumberAsync failed for {guid} - Exception: {ex.Message}");
                return null;
            }
        }

        public async Task<int> GetHeadsetInstanceNumberAsync(string guid)
        {
            try
            {
                var result = await _DTPProxyPlugin.GetHeadsetInstanceNumberAsync(guid);
                if (result != -1)
                {
                    writelog($"[DeviceManagerPlugin] [Headset] GetHeadsetInstanceNumberAsync Success");
                    return result;
                }
                else
                {
                    writelog($"[DeviceManagerPlugin] [Headset] GetHeadsetInstanceNumberAsync value is -1");
                    return -1;
                }
            }
            catch (Exception ex)
            {
                writelog($"[DeviceManagerPlugin] [Headset] GetInstanceNumberAsync failed for {guid} - Exception: {ex.Message}");
                return -1;
            }
        }

        public async Task<int> GetHeadsetInstanceIdAsync(string guid)
        {
            try
            {
                var result = await _DTPProxyPlugin.GetHeadsetInstanceIdAsync(guid);
                if (result != -1)
                {
                    writelog($"[DeviceManagerPlugin] [Headset] GetHeadsetInstanceIdAsync Success");
                    return result;
                }
                else
                {
                    writelog($"[DeviceManagerPlugin] [Headset] GetHeadsetInstanceIdAsync value is -1");
                    return -1;
                }
            }
            catch (Exception ex)
            {
                writelog($"[DeviceManagerPlugin] [Headset] GetInstanceIdAsync failed for {guid} - Exception: {ex.Message}");
                return -1;
            }
        }

        public async Task<string> GetHeadsetFirmwareVersionAsync(string guid)
        {
            try
            {
                var result = await _DTPProxyPlugin.GetHeadsetFirmwareVersionAsync(guid);
                if (result != null)
                {
                    writelog($"[DeviceManagerPlugin] [Headset] GetHeadsetFirmwareVersionAsync Success");
                    return result;
                }
                else
                {
                    writelog($"[DeviceManagerPlugin] [Headset] GetHeadsetFirmwareVersionAsync value is null");
                    return null;
                }
            }
            catch (Exception ex)
            {
                writelog($"[DeviceManagerPlugin] [Headset] GetFirmwareVersionAsync failed for {guid} - Exception: {ex.Message}");
                return null;
            }
        }

        public async Task<string> GetHeadsetDeviceTypeAsync(string guid)
        {
            try
            {
                var result = await _DTPProxyPlugin.GetHeadsetDeviceTypeAsync(guid);
                if (result != null)
                {
                    writelog($"[DeviceManagerPlugin] [Headset] GetHeadsetDeviceTypeAsync Success");
                    return result;
                }
                else
                {
                    writelog($"[DeviceManagerPlugin] [Headset] GetHeadsetDeviceTypeAsync value is null");
                    return null;
                }
            }
            catch (Exception ex)
            {
                writelog($"[DeviceManagerPlugin] [Headset] GetDeviceTypeAsync failed for {guid} - Exception: {ex.Message}");
                return null;
            }
        }

        public async Task<string> GetHeadsetParentDeviceTypeAsync(string guid)
        {
            try
            {
                var result = await _DTPProxyPlugin.GetHeadsetParentDeviceTypeAsync(guid);
                if (result != null)
                {
                    writelog($"[DeviceManagerPlugin] [Headset] GetHeadsetParentDeviceTypeAsync Success");
                    return result;
                }
                else
                {
                    writelog($"[DeviceManagerPlugin] [Headset] GetHeadsetParentDeviceTypeAsync value is null");
                    return null;
                }
            }
            catch (Exception ex)
            {
                writelog($"[DeviceManagerPlugin] [Headset] GetParentDeviceTypeAsync failed for {guid} - Exception: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> GetHeadsetIsBatteryLevelSupportedAsync(string guid)
        {
            try
            {
                var result = await _DTPProxyPlugin.GetHeadsetIsBatteryLevelSupportedAsync(guid);
                if (result)
                    writelog($"[DeviceManagerPlugin] [Headset] GetHeadsetIsBatteryLevelSupportedAsync Success");
                else
                    writelog($"[DeviceManagerPlugin] [Headset] GetHeadsetIsBatteryLevelSupportedAsync Fail");
                return result;
            }
            catch (Exception ex)
            {
                writelog($"[DeviceManagerPlugin] [Headset] GetIsBatteryLevelSupportedAsync failed for {guid} - Exception: {ex.Message}");
                return false;
            }
        }

        public async Task<int> GetHeadsetBatteryLevelAsync(string guid)
        {
            try
            {
                var result = await _DTPProxyPlugin.GetHeadsetBatteryLevelAsync(guid);
                if (result != -1)
                {
                    writelog($"[DeviceManagerPlugin] [Headset] GetHeadsetBatteryLevelAsync Success");
                    return result;
                }
                else
                {
                    writelog($"[DeviceManagerPlugin] [Headset] GetHeadsetBatteryLevelAsync value is -1");
                    return -1;
                }
            }
            catch (Exception ex)
            {
                writelog($"[DeviceManagerPlugin] [Headset] GetBatteryLevelAsync failed for {guid} - Exception: {ex.Message}");
                return -1;
            }
        }

        public async Task<string> GetHeadsetDeviceBatteryStatusAsync(string guid)
        {
            try
            {
                var result = await _DTPProxyPlugin.GetHeadsetDeviceBatteryStatusAsync(guid);
                if (result != null)
                {
                    writelog($"[DeviceManagerPlugin] [Headset] GetHeadsetDeviceBatteryStatusAsync Success");
                    return result;
                }
                else
                {
                    writelog($"[DeviceManagerPlugin] [Headset] GetHeadsetDeviceBatteryStatusAsync value is null");
                    return null;
                }
            }
            catch (Exception ex)
            {
                writelog($"[DeviceManagerPlugin] [Headset] GetDeviceBatteryStatusAsync failed for {guid} - Exception: {ex.Message}");
                return null;
            }
        }

        public async Task<string> GetHeadsetPairingStatusAsync(string guid)
        {
            try
            {
                var result = await _DTPProxyPlugin.GetHeadsetPairingStatusAsync(guid);
                if (result != null)
                {
                    writelog($"[DeviceManagerPlugin] [Headset] GetHeadsetPairingStatusAsync Success");
                    return result;
                }
                else
                {
                    writelog($"[DeviceManagerPlugin] [Headset] GetHeadsetPairingStatusAsync value is null");
                    return null;
                }
            }
            catch (Exception ex)
            {
                writelog($"[DeviceManagerPlugin] [Headset] GetPairingStatusAsync failed for {guid} - Exception: {ex.Message}");
                return null;
            }
        }

        public async Task<string> GetHeadsetPairedHostName1Async(string guid)
        {
            try
            {
                var result = await _DTPProxyPlugin.GetHeadsetPairedHostName1Async(guid);
                if (result != null)
                {
                    writelog($"[DeviceManagerPlugin] [Headset] GetHeadsetPairedHostName1Async Success");
                    return result;
                }
                else
                {
                    writelog($"[DeviceManagerPlugin] [Headset] GetHeadsetPairedHostName1Async value is null");
                    return null;
                }
            }
            catch (Exception ex)
            {
                writelog($"[DeviceManagerPlugin] [Headset] GetPairedHostName1Async failed for {guid} - Exception: {ex.Message}");
                return null;
            }
        }

        public async Task<string> GetHeadsetPairedHostName2Async(string guid)
        {
            try
            {
                var result = await _DTPProxyPlugin.GetHeadsetPairedHostName2Async(guid);
                if (result != null)
                {
                    writelog($"[DeviceManagerPlugin] [Headset] GetHeadsetPairedHostName2Async Success");
                    return result;
                }
                else
                {
                    writelog($"[DeviceManagerPlugin] [Headset] GetHeadsetPairedHostName2Async value is null");
                    return null;
                }
            }
            catch (Exception ex)
            {
                writelog($"[DeviceManagerPlugin] [Headset] GetPairedHostName2Async failed for {guid} - Exception: {ex.Message}");
                return null;
            }
        }

        public async Task<string> GetHeadsetPairedHostName3Async(string guid)
        {
            try
            {
                var result = await _DTPProxyPlugin.GetHeadsetPairedHostName3Async(guid);
                if (result != null)
                {
                    writelog($"[DeviceManagerPlugin] [Headset] GetHeadsetPairedHostName3Async Success");
                    return result;
                }
                else
                {
                    writelog($"[DeviceManagerPlugin] [Headset] GetHeadsetPairedHostName3Async value is null");
                    return null;
                }
            }
            catch (Exception ex)
            {
                writelog($"[DeviceManagerPlugin] [Headset] GetPairedHostName3Async failed for {guid} - Exception: {ex.Message}");
                return null;
            }
        }

        public async Task<int> GetHeadsetMaxPairingSlotsAsync(string guid)
        {
            try
            {
                var result = await _DTPProxyPlugin.GetHeadsetMaxPairingSlotsAsync(guid);
                if (result != -1)
                {
                    writelog($"[DeviceManagerPlugin] [Headset] GetHeadsetMaxPairingSlotsAsync Success");
                    return result;
                }
                else
                {
                    writelog($"[DeviceManagerPlugin] [Headset] GetHeadsetMaxPairingSlotsAsync value is -1");
                    return -1;
                }
            }
            catch (Exception ex)
            {
                writelog($"[DeviceManagerPlugin] [Headset] GetMaxPairingSlotsAsync failed for {guid} - Exception: {ex.Message}");
                return -1;
            }
        }

        public async Task<int> GetHeadsetPairedDeviceCountAsync(string guid)
        {
            try
            {
                var result = await _DTPProxyPlugin.GetHeadsetPairedDeviceCountAsync(guid);
                if (result != -1)
                {
                    writelog($"[DeviceManagerPlugin] [Headset] GetHeadsetPairedDeviceCountAsync Success");
                    return result;
                }
                else
                {
                    writelog($"[DeviceManagerPlugin] [Headset] GetHeadsetPairedDeviceCountAsync value is -1");
                    return -1;
                }
            }
            catch (Exception ex)
            {
                writelog($"[DeviceManagerPlugin] [Headset] GetPairedDeviceCountAsync failed for {guid} - Exception: {ex.Message}");
                return -1;
            }
        }

        public async Task<int> GetHeadsetTotalNumberOfPairedHostNameAsync(string guid)
        {
            try
            {
                var result = await _DTPProxyPlugin.GetHeadsetTotalNumberOfPairedHostNameAsync(guid);
                if (result != -1)
                {
                    writelog($"[DeviceManagerPlugin] [Headset] GetHeadsetTotalNumberOfPairedHostNameAsync Success");
                    return result;
                }
                else
                {
                    writelog($"[DeviceManagerPlugin] [Headset] GetHeadsetTotalNumberOfPairedHostNameAsync value is -1");
                    return -1;
                }
            }
            catch (Exception ex)
            {
                writelog($"[DeviceManagerPlugin] [Headset] GetTotalNumberOfPairedHostNameAsync failed for {guid} - Exception: {ex.Message}");
                return -1;
            }
        }

        public async Task<string> GetHeadsetSerialNumberAsync(string guid)
        {
            try
            {
                var result = await _DTPProxyPlugin.GetHeadsetSerialNumberAsync(guid);
                if (result != null)
                {
                    writelog($"[DeviceManagerPlugin] [Headset] GetHeadsetSerialNumberAsync Success");
                    return result;
                }
                else
                {
                    writelog($"[DeviceManagerPlugin] [Headset] GetHeadsetSerialNumberAsync value is null");
                    return null;
                }
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
                if (result)
                    writelog($"[DeviceManagerPlugin] [Headset] GetIsReadyAsync Success");
                else
                    writelog($"[DeviceManagerPlugin] [Headset] GetIsReadyAsync Fail");
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
                if (result)
                    writelog($"[DeviceManagerPlugin] [Headset] GetIsDirtyAsync Success");
                else
                    writelog($"[DeviceManagerPlugin] [Headset] GetIsDirtyAsync Fail");
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
                if (result)
                    writelog($"[DeviceManagerPlugin] [Headset] GetIsMicNoiseCancellationSupportedAsync Success");
                else
                    writelog($"[DeviceManagerPlugin] [Headset] GetIsMicNoiseCancellationSupportedAsync Fail");
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
                if (result)
                    writelog($"[DeviceManagerPlugin] [Headset] GetIsSidetoneSupportedAsync Success");
                else
                    writelog($"[DeviceManagerPlugin] [Headset] GetIsSidetoneSupportedAsync Fail");
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
                if (result)
                    writelog($"[DeviceManagerPlugin] [Headset] GetIsBusyLightSupportedAsync Success");
                else
                    writelog($"[DeviceManagerPlugin] [Headset] GetIsBusyLightSupportedAsync Fail");
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
                if (result)
                    writelog($"[DeviceManagerPlugin] [Headset] GetIsVoiceGuidanceSupportedAsync Success");
                else
                    writelog($"[DeviceManagerPlugin] [Headset] GetIsVoiceGuidanceSupportedAsync Fail");
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
                if (result)
                    writelog($"[DeviceManagerPlugin] [Headset] GetIsPresetsSupportedAsync Success");
                else
                    writelog($"[DeviceManagerPlugin] [Headset] GetIsPresetsSupportedAsync Fail");
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
                if (result)
                    writelog($"[DeviceManagerPlugin] [Headset] GetIsEqualizerSupportedAsync Success");
                else
                    writelog($"[DeviceManagerPlugin] [Headset] GetIsEqualizerSupportedAsync Fail");
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
                //It's enum HeadsetConnectionType
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
                if (result)
                    writelog($"[DeviceManagerPlugin] [Headset] GetIsANCSupportedAsync Success");
                else
                    writelog($"[DeviceManagerPlugin] [Headset] GetIsANCSupportedAsync Fail");
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
                if (result)
                    writelog($"[DeviceManagerPlugin] [Headset] GetIsWearDetectionSupportedAsync Success");
                else
                    writelog($"[DeviceManagerPlugin] [Headset] GetIsWearDetectionSupportedAsync Fail");
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
                if (result)
                    writelog($"[DeviceManagerPlugin] [Headset] GetIsWearDetectionSensitivitySupportedAsync Success");
                else
                    writelog($"[DeviceManagerPlugin] [Headset] GetIsWearDetectionSensitivitySupportedAsync Fail");
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
                if (result)
                    writelog($"[DeviceManagerPlugin] [Headset] GetIsWearDetectionPauseMusicSupportedAsync Success");
                else
                    writelog($"[DeviceManagerPlugin] [Headset] GetIsWearDetectionPauseMusicSupportedAsync Fail");
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
                if (result)
                    writelog($"[DeviceManagerPlugin] [Headset] GetIsWearDetectionMuteMicSupportedAsync Success");
                else
                    writelog($"[DeviceManagerPlugin] [Headset] GetIsWearDetectionMuteMicSupportedAsync Fail");
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
                if (result)
                    writelog($"[DeviceManagerPlugin] [Headset] GetIsWearDetectionQuickPauseSupportedAsync Success");
                else
                    writelog($"[DeviceManagerPlugin] [Headset] GetIsWearDetectionQuickPauseSupportedAsync Fail");
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
                if (result)
                    writelog($"[DeviceManagerPlugin] [Headset] GetMicNoiseCancellationAsync Success");
                else
                    writelog($"[DeviceManagerPlugin] [Headset] GetMicNoiseCancellationAsync Fail");
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
                if (result)
                    writelog($"[DeviceManagerPlugin] [Headset] GetMicNCIncomingAsync Success");
                else
                    writelog($"[DeviceManagerPlugin] [Headset] GetMicNCIncomingAsync Fail");
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
                if (result)
                    writelog($"[DeviceManagerPlugin] [Headset] GetSidetoneAsync Success");
                else
                    writelog($"[DeviceManagerPlugin] [Headset] GetSidetoneAsync Fail");
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
                if (result)
                    writelog($"[DeviceManagerPlugin] [Headset] GetBusyLightAsync Success");
                else
                    writelog($"[DeviceManagerPlugin] [Headset] GetBusyLightAsync Fail");
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
                if (result)
                    writelog($"[DeviceManagerPlugin] [Headset] GetVoiceGuidanceAsync Success");
                else
                    writelog($"[DeviceManagerPlugin] [Headset] GetVoiceGuidanceAsync Fail");
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
                if (result != -1)
                    writelog($"[DeviceManagerPlugin] [Headset] GetVoiceGuidanceAsync Success");
                else
                    writelog($"[DeviceManagerPlugin] [Headset] GetVoiceGuidanceAsync Fail");
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
                if (result != -1)
                    writelog($"[DeviceManagerPlugin] [Headset] GetSidetoneLevelAsync Success");
                else
                    writelog($"[DeviceManagerPlugin] [Headset] GetSidetoneLevelAsync Fail");
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
                if (result)
                    writelog($"[DeviceManagerPlugin] [Headset] GetMuteStatusAsync Success");
                else
                    writelog($"[DeviceManagerPlugin] [Headset] GetMuteStatusAsync Fail");
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
                    writelog($"[DeviceManagerPlugin] [Headset] GetBandsGainAsync succeeded, result length is {result.Length}");
                else
                    writelog($"[DeviceManagerPlugin] [Headset] GetBandsGainAsync succeeded, but result is null");
                return result;
            }
            catch (Exception ex)
            {
                writelog($"[DeviceManagerPlugin] [Headset] GetBandsGainAsync failed for {guid} - Exception: {ex.Message}");
                return null;
            }
        }

        public async Task<int> GetBand1GainAsync(string guid)
        {
            try
            {
                var result = await _DTPProxyPlugin.GetBand1GainAsync(guid);
                if (result != -1)
                    writelog($"[DeviceManagerPlugin] [Headset] GetBand1GainAsync Success");
                else
                    writelog($"[DeviceManagerPlugin] [Headset] GetBand1GainAsync Fail");
                return result;
            }
            catch (Exception ex)
            {
                writelog($"[DeviceManagerPlugin] [Headset] GetBand1GainAsync failed for {guid} - Exception: {ex.Message}");
                return -1;
            }
        }

        public async Task<int> GetBand2GainAsync(string guid)
        {
            try
            {
                var result = await _DTPProxyPlugin.GetBand2GainAsync(guid);
                if (result != -1)
                    writelog($"[DeviceManagerPlugin] [Headset] GetBand2GainAsync Success");
                else
                    writelog($"[DeviceManagerPlugin] [Headset] GetBand2GainAsync Fail");
                return result;
            }
            catch (Exception ex)
            {
                writelog($"[DeviceManagerPlugin] [Headset] GetBand2GainAsync failed for {guid} - Exception: {ex.Message}");
                return -1;
            }
        }

        public async Task<int> GetBand3GainAsync(string guid)
        {
            try
            {
                var result = await _DTPProxyPlugin.GetBand3GainAsync(guid);
                if (result != -1)
                    writelog($"[DeviceManagerPlugin] [Headset] GetBand3GainAsync Success");
                else
                    writelog($"[DeviceManagerPlugin] [Headset] GetBand3GainAsync Fail");
                return result;
            }
            catch (Exception ex)
            {
                writelog($"[DeviceManagerPlugin] [Headset] GetBand3GainAsync failed for {guid} - Exception: {ex.Message}");
                return -1;
            }
        }

        public async Task<int> GetBand4GainAsync(string guid)
        {
            try
            {
                var result = await _DTPProxyPlugin.GetBand4GainAsync(guid);
                if (result != -1)
                    writelog($"[DeviceManagerPlugin] [Headset] GetBand4GainAsync Success");
                else
                    writelog($"[DeviceManagerPlugin] [Headset] GetBand4GainAsync Fail");
                return result;
            }
            catch (Exception ex)
            {
                writelog($"[DeviceManagerPlugin] [Headset] GetBand4GainAsync failed for {guid} - Exception: {ex.Message}");
                return -1;
            }
        }

        public async Task<int> GetBand5GainAsync(string guid)
        {
            try
            {
                var result = await _DTPProxyPlugin.GetBand5GainAsync(guid);
                if (result != -1)
                    writelog($"[DeviceManagerPlugin] [Headset] GetBand5GainAsync Success");
                else
                    writelog($"[DeviceManagerPlugin] [Headset] GetBand5GainAsync Fail");
                return result;
            }
            catch (Exception ex)
            {
                writelog($"[DeviceManagerPlugin] [Headset] GetBand5GainAsync failed for {guid} - Exception: {ex.Message}");
                return -1;
            }
        }

        public async Task<int> GetAncModeAsync(string guid)
        {
            try
            {
                var result = await _DTPProxyPlugin.GetAncModeAsync(guid);
                if (result != -1)
                    writelog($"[DeviceManagerPlugin] [Headset] GetAncModeAsync Success");
                else
                    writelog($"[DeviceManagerPlugin] [Headset] GetAncModeAsync Fail");
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
                if (result != -1)
                    writelog($"[DeviceManagerPlugin] [Headset] GetAncGainAsync Success");
                else
                    writelog($"[DeviceManagerPlugin] [Headset] GetAncGainAsync Fail");
                return result;
            }
            catch (Exception ex)
            {
                writelog($"[DeviceManagerPlugin] [Headset] GetAncGainAsync failed for {guid} - Exception: {ex.Message}");
                return -1;
            }
        }

        public async Task<bool> GetWearDetectionAsync(string guid)
        {
            try
            {
                var result = await _DTPProxyPlugin.GetWearDetectionAsync(guid);

                if (result)
                    writelog($"[DeviceManagerPlugin] [Headset] GetWearDetectionAsync Success");
                else
                    writelog($"[DeviceManagerPlugin] [Headset] GetWearDetectionAsync Fail");
                return result;
            }
            catch (Exception ex)
            {
                writelog($"[DeviceManagerPlugin] [Headset] GetWearDetectionAsync failed for {guid} - Exception: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> GetIsWearDetectionPauseMusicEnabledAsync(string guid)
        {
            try
            {
                var result = await _DTPProxyPlugin.GetIsWearDetectionPauseMusicEnabledAsync(guid);
                if (result)
                    writelog($"[DeviceManagerPlugin] [Headset] GetIsWearDetectionPauseMusicEnabledAsync Success");
                else
                    writelog($"[DeviceManagerPlugin] [Headset] GetIsWearDetectionPauseMusicEnabledAsync Fail");
                return result;
            }
            catch (Exception ex)
            {
                writelog($"[DeviceManagerPlugin] [Headset] GetIsWearDetectionPauseMusicEnabledAsync failed for {guid} - Exception: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> GetIsWearDetectionMuteMicEnabledAsync(string guid)
        {
            try
            {
                var result = await _DTPProxyPlugin.GetIsWearDetectionMuteMicEnabledAsync(guid);
                if (result)
                    writelog($"[DeviceManagerPlugin] [Headset] GetIsWearDetectionMuteMicEnabledAsync Success");
                else
                    writelog($"[DeviceManagerPlugin] [Headset] GetIsWearDetectionMuteMicEnabledAsync Fail");
                return result;
            }
            catch (Exception ex)
            {
                writelog($"[DeviceManagerPlugin] [Headset] GetIsWearDetectionMuteMicEnabledAsync failed for {guid} - Exception: {ex.Message}");
                return false;
            }
        }

        public async Task<int> GetWearDetectionSensitivityAsync(string guid)
        {
            try
            {
                var result = await _DTPProxyPlugin.GetWearDetectionSensitivityAsync(guid);
                if (result != -1)
                    writelog($"[DeviceManagerPlugin] [Headset] GetWearDetectionSensitivityAsync Success");
                else
                    writelog($"[DeviceManagerPlugin] [Headset] GetWearDetectionSensitivityAsync Fail");
                return result;
            }
            catch (Exception ex)
            {
                writelog($"[DeviceManagerPlugin] [Headset] GetWearDetectionSensitivityAsync failed for {guid} - Exception: {ex.Message}");
                return -1;
            }
        }

        public async Task<int> GetWearDetectionQuickPauseAsync(string guid)
        {
            try
            {
                var result = await _DTPProxyPlugin.GetWearDetectionQuickPauseAsync(guid);
                if (result != -1)
                    writelog($"[DeviceManagerPlugin] [Headset] GetWearDetectionQuickPauseAsync Success");
                else
                    writelog($"[DeviceManagerPlugin] [Headset] GetWearDetectionQuickPauseAsync Fail");
                return result;
            }
            catch (Exception ex)
            {
                writelog($"[DeviceManagerPlugin] [Headset] GetWearDetectionQuickPauseAsync failed for {guid} - Exception: {ex.Message}");
                return -1;
            }
        }

        public async Task<bool> GetIsMicNCIncomingSupportedAsync(string guid)
        {
            try
            {
                var result = await _DTPProxyPlugin.GetIsMicNCIncomingSupportedAsync(guid);
                if (result)
                    writelog($"[DeviceManagerPlugin] [Headset] GetIsMicNCIncomingSupportedAsync Success");
                else
                    writelog($"[DeviceManagerPlugin] [Headset] GetIsMicNCIncomingSupportedAsync Fail");
                return result;
            }
            catch (Exception ex)
            {
                writelog($"[DeviceManagerPlugin] [Headset] GetIsMicNCIncomingSupportedAsync failed for {guid} - Exception: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> GetIsBoomMicSupportedAsync(string guid)
        {
            try
            {
                var result = await _DTPProxyPlugin.GetIsBoomMicSupportedAsync(guid);
                if (result)
                    writelog($"[DeviceManagerPlugin] [Headset] GetIsBoomMicSupportedAsync Success");
                else
                    writelog($"[DeviceManagerPlugin] [Headset] GetIsBoomMicSupportedAsync Fail");
                return result;
            }
            catch (Exception ex)
            {
                writelog($"[DeviceManagerPlugin] [Headset] GetIsBoomMicSupportedAsync failed for {guid} - Exception: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> GetBoomMicAsync(string guid)
        {
            try
            {
                var result = await _DTPProxyPlugin.GetBoomMicAsync(guid);
                if (result)
                    writelog($"[DeviceManagerPlugin] [Headset] GetBoomMicAsync Success");
                else
                    writelog($"[DeviceManagerPlugin] [Headset] GetBoomMicAsync Fail");
                return result;
            }
            catch (Exception ex)
            {
                writelog($"[DeviceManagerPlugin] [Headset] GetBoomMicAsync failed for {guid} - Exception: {ex.Message}");
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
                bool result = await _DTPProxyPlugin.SetBassAsync(guid, newValue);
                if (result)
                    writelog($"[DeviceManagerPlugin] [Speaker] SetBassAsync Success");
                else
                    writelog($"[DeviceManagerPlugin] [Speaker] SetBassAsync Fail");
                return result;
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
                bool result = await _DTPProxyPlugin.SetMidRangeAsync(guid, newValue);
                if (result)
                    writelog($"[DeviceManagerPlugin] [Speaker] SetMidRangeAsync Success");
                else
                    writelog($"[DeviceManagerPlugin] [Speaker] SetMidRangeAsync Fail");
                return result;
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
                bool result = await _DTPProxyPlugin.SetTrebleAsync(guid, newValue);
                if (result)
                    writelog($"[DeviceManagerPlugin] [Speaker] SetTrebleAsync Success");
                else
                    writelog($"[DeviceManagerPlugin] [Speaker] SetTrebleAsync Fail");
                return result;
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
                bool result = await _DTPProxyPlugin.SetProfileForSpeaker(guid, newValue);
                if (result)
                    writelog($"[DeviceManagerPlugin] [Speaker] SetProfileForSpeaker Success");
                else
                    writelog($"[DeviceManagerPlugin] [Speaker] SetProfileForSpeaker Fail");
                return result;
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
                bool result = await _DTPProxyPlugin.SetIsWiredAudioMicMuteSoundEnableAsync(guid, newValue);
                if (result)
                    writelog($"[DeviceManagerPlugin] [Speaker] SetIsWiredAudioMicMuteSoundEnableAsync Success");
                else
                    writelog($"[DeviceManagerPlugin] [Speaker] SetIsWiredAudioMicMuteSoundEnableAsync Fail");
                return result;
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
                bool result = await _DTPProxyPlugin.SetWiredAudioVolumeAdjustmentToneAsync(guid, newValue);
                if (result)
                    writelog($"[DeviceManagerPlugin] [Speaker] SetWiredAudioVolumeAdjustmentToneAsync Success");
                else
                    writelog($"[DeviceManagerPlugin] [Speaker] SetWiredAudioVolumeAdjustmentToneAsync Fail");
                return result;
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
                bool result = await _DTPProxyPlugin.SetIsWiredAudioIMicNSEnableAsync(guid, newValue);
                if (result)
                    writelog($"[DeviceManagerPlugin] [Speaker] SetIsWiredAudioIMicNSEnableAsync Success");
                else
                    writelog($"[DeviceManagerPlugin] [Speaker] SetIsWiredAudioIMicNSEnableAsync Fail");
                return result;
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
                bool result = await _DTPProxyPlugin.SetResetToDefaultAsyncForSoundbar(guid, newValue);
                if (result)
                    writelog($"[DeviceManagerPlugin] [Speaker] SetResetToDefaultAsyncForSoundbar Success");
                else
                    writelog($"[DeviceManagerPlugin] [Speaker] SetResetToDefaultAsyncForSoundbar Fail");
                return result;
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
                if (result != null)
                    writelog($"[DeviceManagerPlugin] [Headset] GetProfileAsync Success");
                else
                    writelog($"[DeviceManagerPlugin] [Headset] GetProfileAsync Fail");
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
                if (result != -1)
                    writelog($"[DeviceManagerPlugin] [Headset] GetBassAsync Success");
                else
                    writelog($"[DeviceManagerPlugin] [Headset] GetBassAsync Fail");
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
                if (result != -1)
                    writelog($"[DeviceManagerPlugin] [Headset] GetMidRangeAsync Success");
                else
                    writelog($"[DeviceManagerPlugin] [Headset] GetMidRangeAsync Fail");
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
                if (result != -1)
                    writelog($"[DeviceManagerPlugin] [Headset] GetTrebleAsync Success");
                else
                    writelog($"[DeviceManagerPlugin] [Headset] GetTrebleAsync Fail");
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
                if (result)
                    writelog($"[DeviceManagerPlugin] [Headset] GetIsWiredAudioMicMuteSoundEnableAsync Success");
                else
                    writelog($"[DeviceManagerPlugin] [Headset] GetIsWiredAudioMicMuteSoundEnableAsync Fail");
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
                if (result != -1)
                    writelog($"[DeviceManagerPlugin] [Headset] GetWiredAudioVolumeAdjustmentToneAsync Success");
                else
                    writelog($"[DeviceManagerPlugin] [Headset] GetWiredAudioVolumeAdjustmentToneAsync Fail");
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
                if (result)
                    writelog($"[DeviceManagerPlugin] [Headset] GetIsWiredAudioIMicNSEnableAsync Success");
                else
                    writelog($"[DeviceManagerPlugin] [Headset] GetIsWiredAudioIMicNSEnableAsync Fail");
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
                if (result)
                    writelog($"[DeviceManagerPlugin] [Headset] GetIsAudioEqualizerSupportedAsync Success");
                else
                    writelog($"[DeviceManagerPlugin] [Headset] GetIsAudioEqualizerSupportedAsync Fail");
                return result;
            }
            catch (Exception ex)
            {
                writelog($"[DeviceManagerPlugin] [Speaker] GetIsAudioEqualizerSupportedAsync failed for {guid} - Exception: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> GetMuteStatusAsyncForSpeaker(string guid)
        {
            try
            {
                var result = await _DTPProxyPlugin.GetMuteStatusAsyncForSpeaker(guid);
                if (result)
                    writelog($"[DeviceManagerPlugin] [Headset] GetMuteStatusAsyncForSpeaker Success");
                else
                    writelog($"[DeviceManagerPlugin] [Headset] GetMuteStatusAsyncForSpeaker Fail");
                return result;
            }
            catch (Exception ex)
            {
                writelog($"[DeviceManagerPlugin] [Speaker] GetMuteStatusAsyncForSpeaker failed for {guid} - Exception: {ex.Message}");
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
            //Derek 1125
            if (e != null && e != EventArgs.Empty && e.UI_Field_Name.StartsWith("WebcamEvent"))
                HandleQAMEvent(e.UI_Field_Name);

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
                if (rt)
                    writelog("[DeviceMangerPlugin] Send Telementry for GamingRefreshRate Success ...");
                else
                    writelog("[DeviceMangerPlugin] Send Telementry for GamingRefreshRate Fail ...");
            }
            return Task.FromResult(ret);
        }

        public Task<bool> SetOrientation(MonitorInfo monitorInfo, DisplayOrientation orientation)
        {
            bool ret = false;
            if (_DisplayManagerPlugin != null)
            {
                displayInOut = false;
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
            writelog("[DeviceMangerPlugin] LockRotate Start");
            bool ret = false;
            if (_SettingsPlugin != null)
            {
                writelog("[DeviceMangerPlugin] _SettingsPlugin.ReloadAppConfigData go");
                DDPMSettings config = _SettingsPlugin.ReloadAppConfigData().Result;
                if (config != null && _DisplayManagerPlugin != null)
                {
                    config.UserSettings.LockRotate = onoff;
                    writelog("[DeviceMangerPlugin] _DisplayManagerPlugin.SetEnableLockOrientation go");
                    _DisplayManagerPlugin.SetEnableLockOrientation(onoff);
                    writelog("[DeviceMangerPlugin] _SettingsPlugin.SetAppConfigData go");
                    ret = _SettingsPlugin.SetAppConfigData(config).Result;
                }

                Task.Run(() =>
                {
                    //Telementry Collection
                    var rt = false;
                    var ApplicationSettings_Function = new ApplicationSettings_Function();
                    writelog("[DeviceMangerPlugin] Send Telementry for LockRotation...");
                    rt = ApplicationSettings_Function.Send_LockRotation_Telementry(_TelementryScheduler, _AllInfoMonitors, onoff);
                    if (rt)
                        writelog("[DeviceMangerPlugin] Send Telementry for LockRotation Success ...");
                    else
                        writelog("[DeviceMangerPlugin] Send Telementry for LockRotation Fail ...");
                }).ConfigureAwait(false);
            }
            writelog("[DeviceMangerPlugin] LockRotate done");
            return Task.FromResult(ret);
        }

        //0606 Bruce 新增鎖定自動旋轉方向
        public Task<bool> GetLockRotateStatus()
        {
            writelog("[DeviceMangerPlugin] GetLockRotateStatus Start");
            bool ret = false;
            try
            {
                if (_SettingsPlugin != null && _DisplayManagerPlugin != null)
                {
                    writelog("[DeviceMangerPlugin] _SettingsPlugin.ReloadAppConfigData go");
                    DDPMSettings config = _SettingsPlugin.ReloadAppConfigData().Result;
                    if (config != null && config.UserSettings != null)
                    {
                        writelog("[DeviceMangerPlugin] _DisplayManagerPlugin.SetEnableLockOrientation go");
                        _DisplayManagerPlugin.SetEnableLockOrientation(config.UserSettings.LockRotate);
                        ret = config.UserSettings.LockRotate;
                    }
                }
            }
            catch (Exception ex)
            {
                writelog($"[DeviceMangerPlugin] GetLockRotateStatus Error : {ex.Message}");
            }
            writelog("[DeviceMangerPlugin] GetLockRotateStatus done");
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
                _NKVMPlugin.SetVCPNotify(monitorInfo, 0xE9, (int)(uint)modeCode).Wait();
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

        // add @ 20241202 stephen
        public void updateFWUpdateInfoPackage(FWUpdateInfoPackage pkg)
        {
            _FWUpdatePlugin.setFWUpdateInfoPackage(pkg);
        }

        public Task<FWUpdateInfoPackage> GetFWUpdateInfo(bool isShowNotify = true, bool isForce = false, bool isDefer = false, List<DeviceType> deviceTypeList = null, bool UODMode = false, bool isOnlyDisplay = false, bool reScan = true, bool isUITrigger = false, List<string> giuds = null, List<string> serviceTags = null, List<string> models = null, string minVersion = "")
        {
            writelog("[DeviceMangerPlugin] GetFWUpdateInfo start");
            if (_PeripheralsPlugin != null && _FWUpdatePlugin != null && _DisplayManagerPlugin != null && _SettingsPlugin != null)
            {
                _PeripheralsPlugin.CheckForUpdate();
                UpdateHelper updateHelper = _PeripheralsPlugin.GetFWUpdateInfo().Result;
                if (updateHelper == null || updateHelper.UpdateItems == null)
                {
                    writelog("[DeviceMangerPlugin] updateHelper is null");
                    updateHelper = new UpdateHelper();
                    updateHelper.UpdateItems = new List<UpdateItemInfo>();
                }
                else
                {
                    writelog("[DeviceMangerPlugin] updateHelper is no null");
                    for (int i = 0; i < updateHelper.UpdateItems.Count; i++)
                    {
                        writelog($"[DeviceMangerPlugin] GetFWUpdateInfo DeviceModelNumber : {updateHelper.UpdateItems[i].DeviceModelNumber}GetFWUpdateInfo NewVersion :{Convert.ToInt32(updateHelper.UpdateItems[i].NewVersion, 16).ToString("X8")}");
                    }
                }
                List<DeviceInfo> deviceInfos = _PeripheralsPlugin.GetDevices().Result.deviceInfo;
                if (deviceInfos == null)
                {
                    writelog("[DeviceMangerPlugin] deviceInfos is null");
                    deviceInfos = new List<DeviceInfo>();
                }
                DisplayUpdateHelper displayUpdateHelper = _DisplayManagerPlugin.GetDisplayFWUpdate(_IsSkipCA, _SettingsPlugin).Result;
                if (displayUpdateHelper == null || displayUpdateHelper.Firmwares == null)
                {
                    writelog("[DeviceMangerPlugin] displayUpdateHelper is null");
                    displayUpdateHelper = new DisplayUpdateHelper();
                    displayUpdateHelper.Firmwares = new List<Display_Firmwares_item>();
                }
                writelog("[DeviceMangerPlugin] _FWUpdatePlugin.GetFWUpdateInfo go");
                return Task.FromResult(_FWUpdatePlugin.GetFWUpdateInfo(updateHelper, deviceInfos, isShowNotify, isForce, isDefer, deviceTypeList, UODMode, displayUpdateHelper, isOnlyDisplay, reScan, isUITrigger, giuds, serviceTags, models, minVersion).Result);
            }
            writelog("[DeviceMangerPlugin] GetFWUpdateInfo done, But all obj is null");
            return Task.FromResult(new FWUpdateInfoPackage());
        }

        public Task<List<FWUpdateInfo>> DownloadAndInstall(List<FWUpdateInfo> fwUpdateInfos, bool isUITrigger = false, string installPath = "")
        {
            writelog("[DeviceMangerPlugin] DownloadAndInstall start");
            writelog($"[DeviceMangerPlugin] DownloadAndInstall isUITrigger : {isUITrigger}");
            GetDeviceinfos().Wait();
            if (_FWUpdatePlugin == null)
            {
                writelog("[DeviceMangerPlugin] _FWUpdatePlugin is null");
                return Task.FromResult(new List<FWUpdateInfo>());
            }
            _UpdateProgress = null;
            writelog($"[DeviceMangerPlugin] SetDelayFWUpdateInfoPackage go");
            SetDelayFWUpdateInfoPackage();
            if (isUITrigger)
            {
                writelog($"[DeviceMangerPlugin] CallUpdateProgressUI() go");
                CallUpdateProgressUI().Wait();
            }
            writelog($"[DeviceMangerPlugin] _FWUpdatePlugin.DownloadAndInstall go");
            List<FWUpdateInfo> tmpFWUpdateInfos = _FWUpdatePlugin.DownloadAndInstall(fwUpdateInfos, isUITrigger, installPath).Result;
            if (_UpdateProgress != null)
            {
                writelog($"[DeviceMangerPlugin] _UpdateProgress.CloseWindow go");
                _FWUpdatePlugin.ProgressUpdate_Notify -= show_fwProgressUpdateEvent;
                ProgressUpdate_Notify -= _UpdateProgress._FWUpdatePlugin_ProgressUpdate;
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
                if (rt)
                    writelog("[DeviceMangerPlugin] Send Telementry for DisplayDeviceFirmware Success ...");
                else
                    writelog("[DeviceMangerPlugin] Send Telementry for DisplayDeviceFirmware Fail ...");
            }
            if (PeripheralsList != null && PeripheralsList.Count > 0)
            {
                rt = false;
                writelog("[DeviceMangerPlugin] Send Telementry for PeripheralsDeviceFirmware...");
                rt = ApplicationSettings_Function.Send_PeripheralsDeviceFirmware_Telementry(_TelementryScheduler, PeripheralsList);
                if (rt)
                    writelog("[DeviceMangerPlugin] Send Telementry for PeripheralsDeviceFirmware Success ...");
                else
                    writelog("[DeviceMangerPlugin] Send Telementry for PeripheralsDeviceFirmware Fail ...");
            }
            return Task.FromResult(tmpFWUpdateInfos);
        }

        public Task<FWUErrorCode> Install(string installPath, bool isOnlyDisplay = false, DeviceType deviceType = DeviceType.Unknown)
        {
            writelog("[DeviceMangerPlugin] Install start");
            writelog($"[DeviceMangerPlugin] Install isOnlyDisplay : {isOnlyDisplay}");
            FWUErrorCode ret = FWUErrorCode.Unknow;
            if (_FWUpdatePlugin != null)
            {
                writelog($"[DeviceMangerPlugin] Install SetDelayFWUpdateInfoPackage go");
                SetDelayFWUpdateInfoPackage();
                //if (_UpdateProgress != null)
                //{
                writelog($"[DeviceMangerPlugin] Install _FWUpdatePlugin.Install go");
                ret = _FWUpdatePlugin.Install(installPath, isOnlyDisplay, deviceType).Result;
                //}
            }
            return Task.FromResult(ret);
        }

        public void SetUILockStatus(bool isLockFWU_UI)
        {
            writelog("[DeviceMangerPlugin] SetUILockStatus start");
            if (_SettingsPlugin != null)
            {
                writelog("[DeviceMangerPlugin] _SettingsPlugin.ReloadAppConfigData go");
                DDPMSettings config = _SettingsPlugin.ReloadAppConfigData().Result;
                if (config != null)
                {
                    config.UserSettings.LockFWU_UI = isLockFWU_UI;
                    writelog("[DeviceMangerPlugin] _SettingsPlugin.SetAppConfigData go");
                    _SettingsPlugin.SetAppConfigData(config).Wait();
                    OnUILockEvent(isLockFWU_UI);
                }
            }
            writelog("[DeviceMangerPlugin] SetUILockStatus done");
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
            writelog("[DeviceMangerPlugin] SetSkipCA start");
            writelog($"[DeviceMangerPlugin] SetSkipCA isSkipCA: {isSkipCA}");
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
            writelog("[DeviceMangerPlugin] SetSkipCA done");
            return Task.FromResult(ret);
        }

        public Task<bool> GetSkipCA()
        {
            writelog("[DeviceMangerPlugin] SetSkipCA start");
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
            writelog($"[DeviceMangerPlugin] SetSkipCA isSkipCA: {_IsSkipCA}");
            writelog("[DeviceMangerPlugin] SetSkipCA done");
            return Task.FromResult(_IsSkipCA);
        }

        private Task<bool> SetSkipSHA()
        {
            bool ret = false;
            writelog("[DeviceMangerPlugin] SetSkipSHA start");
            object o = ReadRegistryData(RegistryHive.LocalMachine, @"SOFTWARE\Dell\DDPM Subagent", "SkipSHA").Result;
            writelog($"[SetSkipSHA], o={o}.");
            if (o != null && o is string && !string.IsNullOrEmpty(o.ToString()))
            {
                bool _IsSkipSHA = o.ToString().Equals("1") ? true : false;
                if (_FWUpdatePlugin != null)
                {
                    _FWUpdatePlugin.SetSkipSHA(_IsSkipSHA);
                }
                if (_SWUpdatePlugin != null)
                {
                    _SWUpdatePlugin.SetSkipSHA(_IsSkipSHA);
                }
                writelog($"[DeviceMangerPlugin] SetSkipSHA SkipSHA: {_IsSkipSHA}");
            }
            writelog("[DeviceMangerPlugin] SetSkipSHA done");
            return Task.FromResult(ret);
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

        public Task<bool> MiniMizeDDPMUI()
        {
            writelog($"{nameof(MiniMizeDDPMUI)} start");
            bool ret = false;
            try
            {
                string processName = "DDPM";
                Process[] processes = Process.GetProcessesByName(processName);
                writelog($"{nameof(MiniMizeDDPMUI)} processes.Length {processes.Length}");
                if (processes.Length > 0)
                {
                    foreach (var process in processes)
                    {
                        IntPtr hwnd = CallUser32dll._FindWindow(null, process.MainWindowTitle);
                        if (hwnd != IntPtr.Zero)
                        {
                            writelog($"{process.ProcessName} SW_MINIMIZE go");
                            CallUser32dll._ShowWindow(hwnd, (int)CallUser32dll.WindowState.SW_MINIMIZE);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                writelog($"{nameof(MiniMizeDDPMUI)} Error : {ex.Message}");
            }
            writelog($"{nameof(MiniMizeDDPMUI)} done");
            return Task.FromResult(ret);
        }

        public Task<bool> RestoreDDPMUI()
        {
            writelog($"{nameof(RestoreDDPMUI)} start");
            bool ret = false;
            try
            {
                string processName = "DDPM";
                Process[] processes = Process.GetProcessesByName(processName);
                writelog($"{nameof(RestoreDDPMUI)} processes.Length {processes.Length}");
                if (processes.Length > 0)
                {
                    foreach (var process in processes)
                    {
                        IntPtr hwnd = CallUser32dll._FindWindow(null, process.MainWindowTitle);
                        if (hwnd != IntPtr.Zero)
                        {
                            writelog($"{process.ProcessName} SW_RESTORE go");
                            CallUser32dll._ShowWindow(hwnd, (int)CallUser32dll.WindowState.SW_RESTORE);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                writelog($"{nameof(RestoreDDPMUI)} Error : {ex.Message}");
            }
            writelog($"{nameof(RestoreDDPMUI)} done");
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
                _FWUpdatePlugin.SetDeviceinfo(_PeripheralsPlugin.GetDevices().Result.deviceInfo, _PeripheralsPlugin.GetDongleCount());
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
                _UpdateProgress = new UpdateProgress(new Logs(Log, "DeviceManager"));
                _UpdateProgress.Closed += (sender2, e2) =>
                {
                    _UpdateProgress.Dispatcher.InvokeShutdown();
                };
                _UpdateProgress.Dispatcher.Invoke(() => _UpdateProgress.Show());
                _FWUpdatePlugin.ProgressUpdate_Notify += show_fwProgressUpdateEvent;
                ProgressUpdate_Notify += _UpdateProgress._FWUpdatePlugin_ProgressUpdate;
                MiniMizeDDPMUI().Wait();
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
                            if (rt)
                                writelog("[DeviceMangerPlugin] Send Telementry for DisplayDeviceFirmware Success ...");
                            else
                                writelog("[DeviceMangerPlugin] Send Telementry for DisplayDeviceFirmware Fail ...");
                        }
                        if (PeripheralsList != null && PeripheralsList.Count > 0)
                        {
                            rt = false;
                            writelog("[DeviceMangerPlugin] Send Telementry for PeripheralsDeviceFirmware...");
                            rt = ApplicationSettings_Function.Send_PeripheralsDeviceFirmware_Telementry(_TelementryScheduler, PeripheralsList);
                            if (rt)
                                writelog("[DeviceMangerPlugin] Send Telementry for PeripheralsDeviceFirmware Success ...");
                            else
                                writelog("[DeviceMangerPlugin] Send Telementry for PeripheralsDeviceFirmware Fail ...");
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
            Dictionary<string, PCsInfo> USBKVMPCsList = new Dictionary<string, PCsInfo>();
            if (monitorInfo != null && inputList != null && subInputList != null)
            {
                if (inputList.Count != 0 && subInputList.Count != 0)
                {
                    USBKVMPCsList = _DisplayManagerPlugin.GetUSBKVMPCsList(monitorInfo, inputList, subInputList).Result;
                }
                else
                {
                    writelog("[GetUSBKVMPCsList]inputList or subInputList count is 0");
                }
            }
            else
            {
                writelog("[GetUSBKVMPCsList]monitorInfo or inputList or subInputList is null");
            }
            //if (inputList != null && subInputList != null)
            //{
            //    if (GetOnUSBKVM(monitorInfo).Result)
            //    {
            //        //get monitor settings
            //        List<DDPMMonitorSettings> settings = _SettingsPlugin.ReloadMonitorSettings(monitorInfo.modelName).Result;
            //        if (settings != null)
            //        {
            //            //get monitor setting
            //            DDPMMonitorSettings monitorSetting = settings.Find(x => x.ServiceTag == monitorInfo.edid.ServiceTag);
            //            if (monitorSetting == null)
            //            {
            //                USBKVMPCsList = _DisplayManagerPlugin.GetUSBKVMPCsList(monitorInfo, inputList, subInputList).Result;
            //                //bool b = SetUSBKVMPCsList(monitorInfo, USBKVMPCsList).Result;
            //            }
            //            else
            //            {
            //                try
            //                {
            //                    if (!string.IsNullOrEmpty(monitorSetting.KVM.strUSBKVMPCsList))
            //                    {
            //                        USBKVMPCsList = USBKVMPCsListDeserialize(monitorSetting.KVM.strUSBKVMPCsList);
            //                        if (USBKVMPCsList != null)
            //                        {
            //                            if (USBKVMPCsList.Count > 1)
            //                            {
            //                                foreach (var pc in USBKVMPCsList)
            //                                {
            //                                    if (string.IsNullOrEmpty(pc.Key) || pc.Value == null)
            //                                    {
            //                                        USBKVMPCsList = _DisplayManagerPlugin.GetUSBKVMPCsList(monitorInfo, inputList, subInputList).Result;
            //                                        break;
            //                                    }
            //                                    else
            //                                    {
            //                                        string usbUpstream = GetUSBUpstream(monitorInfo, pc.Value.InputType).Result;
            //                                        if (string.IsNullOrEmpty(usbUpstream))
            //                                        {
            //                                            writelog("[GetUSBKVMPCsList]usbUpstream is null or empty.");
            //                                        }
            //                                        else
            //                                        {
            //                                            pc.Value.USBUpstream = usbUpstream;
            //                                        }
            //                                    }
            //                                }
            //                            }
            //                            else
            //                            {
            //                                USBKVMPCsList = _DisplayManagerPlugin.GetUSBKVMPCsList(monitorInfo, inputList, subInputList).Result;
            //                            }
            //                        }
            //                    }
            //                    else
            //                    {
            //                        USBKVMPCsList = _DisplayManagerPlugin.GetUSBKVMPCsList(monitorInfo, inputList, subInputList).Result;
            //                        //bool b = SetUSBKVMPCsList(monitorInfo, USBKVMPCsList).Result;
            //                    }
            //                }
            //                catch (Exception e)
            //                {
            //                    USBKVMPCsList = _DisplayManagerPlugin.GetUSBKVMPCsList(monitorInfo, inputList, subInputList).Result;
            //                    //bool b = SetUSBKVMPCsList(monitorInfo, USBKVMPCsList).Result;
            //                }
            //            }
            //        }
            //    }
            //    else
            //    {
            //        USBKVMPCsList = _DisplayManagerPlugin.GetUSBKVMPCsList(monitorInfo, inputList, subInputList).Result;
            //    }
            //}
            //else
            //{
            //    writelog("[GetUSBKVMPCsList]inputList or subInputList is null");
            //}

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

        public Task<string> CheckisShowSynchronize(MonitorInfo currentMoInfo, List<ALSConfig> alsSynchronizeList)
        {
            if (_disDevHelper == null)
                return Task.FromResult("null");
            return Task.FromResult(_disDevHelper.CheckisShowSynchronize(_DisplayManagerPlugin, _AllInfoMonitors, currentMoInfo, alsSynchronizeList).Result);
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
                //_SupportedMonitorList = _NKVMPlugin.UpdateSupportMonitors().Result;
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
                                //_SupportedMonitorList = _NKVMPlugin.GetSupportedNKVM().Result;
                                _NKVMPlugin.OnNKVM().Wait();
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

        public Task CallShowNKVM(int num, int x, int y)
        {
            if (_NKVMPlugin != null)
            {
                _NKVMPlugin.CallShowNKVM(num, x, y);
            }
            return Task.CompletedTask;
        }

        #endregion

        #region EasyArrage

        #region Properties - EasyArrange
        /// <summary>
        /// The last error string after a EAPlugin method return error.
        /// </summary>
        public string EALastError
        {
            get
            {
                if (_DisplayManagerPlugin != null)
                    return "DisplayManagerPlugin is not constructed.";
                return _DisplayManagerPlugin.EALastError;
            }
        }
        #endregion Properties - EasyArrange

        #region EAFunctionEanbled - EasyArrange

        /// <summary>
        /// Robert_Lin, 2024-12-12, To be removed. Use 
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

        /// <summary>
        /// Robert_Lin, 2024-12-12, To be removed.
        /// Old method for CLI
        /// </summary>
        /// <returns></returns>
        public Task<ObjGetVCP> GetEAFunctionEnabled()
        {
            if (_DisplayManagerPlugin != null)
            {
                return _DisplayManagerPlugin.GetEAFunctionEnabled();
            }
            return Task.FromResult<ObjGetVCP>(new ObjGetVCP() { result = false, value = false });
        }
        #endregion EAFunctionEanbled - EasyArrange

        #region EzSettings - EasyArrange
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
        #endregion EzSettings - EasyArrange

        #region EA Custom List - EasyArrange
        //Robert_Ln, 2024-10-12, Added after move CustomList to UserSettings from MonitorSettings
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
            return Task.FromResult(Array.Empty<SplitJson>());
        }

        //Robert_Lin, 2024-10-12 added, move EACustomList to UserSettings from MonitorSettings
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
        #endregion EA Custom List - EasyArrange

        #region EAMonitorSettings - EasyArrange
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
        #endregion EAMonitorSettings - EasyArrange

        #region SelectedLayout - EasyArrange
        public Task<int> GetEASelectedLayout(MonitorInfo monitorInfo)
        {
            //Read the EAMonitorSettings from MonitorSettins.
            //If faile to read will return default settings. never return null.
            EAMonitorSettings eaSettings = ReadEAMonitorSettings(monitorInfo).Result;
            //Return the RAID of EAMonitorSettings.SelectedLayout
            return Task.FromResult(eaSettings.SelectedSplit.EAID);
        }

        public Task<bool> SetEASelectedLayout(MonitorInfo monitorInfo, int eaId)
        {
            if (_DisplayManagerPlugin != null)
            {
                writelog($"@ DeviceManager.SetEASelectedLayout({eaId})");
                return _DisplayManagerPlugin.SetEASelectedLayout(monitorInfo, eaId);
            }
            writelog($"@ DeviceManager.SetEASelectedLayout({eaId}): _DisplayManagerPlugin is null");
            return Task.FromResult(false);
        }

        // [OLD, Use NotifyEASelectedLayoutChanged() instead]
        public Task<bool> SetEAWrokSplit(MonitorInfo monitorInfo, int cellCount, char splitKey, List<double>? settings)
        {
            if (_DisplayManagerPlugin != null)
            {
                _DisplayManagerPlugin.SetEAWrokSplit(monitorInfo, cellCount, splitKey, settings);
                //Telemetry
                //Robert_Lin, 2024-11-15, add monitorInfo for PIMS-321601
                SendEasyArrangeTelemetry("Change_layout", monitorInfo);
            }
            return Task.FromResult(false);
        }

        public Task<bool> NotifyEASelectedLayoutChanged(MonitorInfo monitorInfo, SplitJson spJson)
        {
            if (_DisplayManagerPlugin != null)
            {
                _DisplayManagerPlugin.NotifyEASelectedLayoutChanged(monitorInfo, spJson);
                //Telemetry
                //Robert_Lin, 2024-11-15, add monitorInfo for PIMS-321601
                SendEasyArrangeTelemetry("Change_layout", monitorInfo);
            }
            return Task.FromResult(false);
        }

        /// <summary>
        /// [Unused Method]
        /// [OLD, Use SetEASelectedLayout(MonitorInfo monitorInfo, int eaId) instead]
        /// This method is reserved for CLI command usage. Howevent CLI does not define a command
        /// to set SelectedLayout (only select Off)
        /// </summary>
        /// <param name="monitorInfo"></param>
        /// <param name="spJson"></param>
        /// <returns></returns>
        public Task<bool> SetEASelectedLayout(MonitorInfo monitorInfo, SplitJson spJson)
        {
            if (_DisplayManagerPlugin != null)
            {
                return _DisplayManagerPlugin.SetEASelectedLayout(monitorInfo, spJson);
            }
            writelog("@ DeviceManager.SetEASelectedLayout(): _DisplayManagerPlugin is null");
            return Task.FromResult(false);
        }

        #endregion SelectedLayout - EasyArrange


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


        //Robert_Lin, 2024-11-18, a general method for Subagent to send event to UI
        /// <summary>
        /// A general method for EABroker, to send a notification to UI.
        /// It will trigger EANotify event.
        /// </summary>
        /// <param name="args"></param>
        /// <param name="monitorInfo"></param>
        public Task SendEANotify(EAArgs args)
        {
            if (EANotify != null)
            {
                Task.Run(() => EANotify.Invoke(this, args));
            }
            return Task.CompletedTask;
        }

        /// <summary>
        /// Return current Span across multiple monitor option is Enabled/Disabled;
        /// Note that it's different with EzSettings.IsSpanAcrossMultiMonitors (=ON|OFF)
        /// </summary>
        /// <returns>True=Enabled; False=Disabled</returns>
        public Task<bool> GetIsSpanEnabled()
        {
            if (_DisplayManagerPlugin != null)
            {
                return _DisplayManagerPlugin.GetIsSpanEnabled();
            }
            writelog("@ DeviceManaerPlugin.GetIsSpanEnabled(), _DisplayManagerPlugin is null.");
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

        public Task<InterruptScreenRoot> InterruptScreen_Metadata()
        {
            writelog("[InterruptScreen_Metadata], start.");
            InterruptScreenRoot result = null;
            if (_SettingsPlugin != null)
            {
                try
                {
                    writelog("[InterruptScreen_Metadata], creat logs.");
                    Logs logs = new Logs(Log);

                    writelog("[InterruptScreen_Metadata], SWUpdateSetting.InterruptScreen_Metadata go.");
                    InterruptScreenRoot temp = SWUpdateSetting.InterruptScreen_Metadata(_IsSkipCA, out string info, _SettingsPlugin, null, logs);
                    if (temp != null)
                    {
                        writelog("[InterruptScreen_Metadata], temp is not null");
                        writelog("[InterruptScreen_Metadata], _SettingsPlugin.ReadInterruptScreen go.");
                        InterruptScreenRoot temp2 = _SettingsPlugin.ReadInterruptScreen().Result;
                        if (temp2 == null || !temp.Equals(temp2))
                        {
                            result = temp.Clone();
                            foreach (FeaturesList interruptScreenRoot in temp.featuresList)
                            {
                                if (interruptScreenRoot != null && interruptScreenRoot.content != null)
                                {
                                    interruptScreenRoot.content.image = new byte[0];
                                }
                            }
                            writelog("[InterruptScreen_Metadata], _SettingsPlugin.WriteInterruptScreen go.");
                            bool ret = _SettingsPlugin.WriteInterruptScreen(temp).Result;
                        }
                    }
                    writelog($"[InterruptScreen_Metadata], SWUpdateSetting.InterruptScreen_Metadata info : {info}");
                    logs = null;
                }
                catch (Exception ex)
                {
                    writelog($"[InterruptScreen_Metadata], Error : {ex.Message}");
                }
            }
            writelog("[InterruptScreen_Metadata], done.");
            return Task.FromResult(result);
        }

        private Task<bool> SW_SetSWUpdateInfoPackage(SWUpdateInfoPackage swUpdateInfoPackage)
        {
            writelog("[SW_SetSWUpdateInfoPackage], start.");
            bool ret = false;
            if (_SettingsPlugin != null)
            {
                writelog("[SW_SetSWUpdateInfoPackage], _SettingsPlugin.ReloadAppConfigData go.");
                DDPMSettings config = _SettingsPlugin.ReloadAppConfigData().Result;
                if (config != null)
                {
                    foreach (SWUpdateInfo updateInfo in swUpdateInfoPackage.SWUpdateInfo)
                    {
                        updateInfo.ServerPath = "";
                        updateInfo.SHA256 = "";
                        updateInfo.SHA512 = "";
                        updateInfo.Thumbprint = "";
                    }
                    config.UserSettings.DelaySWUpdateInfoPackage = swUpdateInfoPackage;
                    writelog("[SW_SetSWUpdateInfoPackage], _SettingsPlugin.SetAppConfigData go.");
                    ret = (_SettingsPlugin.SetAppConfigData(config).Result);
                }
            }
            writelog("[SW_SetSWUpdateInfoPackage], done.");
            return Task.FromResult(ret);
        }

        private Task<bool> SW_CheckSWUpdate()
        {
            writelog("[SW_CheckSWUpdate], start.");
            bool ret = false;
            try
            {
                if (_SWUpdatePlugin != null && _GlobalSettingParam != null &&
                    _GlobalSettingParam.GlobalSetting_About != null &&
                    !string.IsNullOrEmpty(_GlobalSettingParam.GlobalSetting_About.SWVersion))
                {
                    SW_SetDelaySWUpdateInfoPackage();
                    SWUpdateInfoPackage swUpdateInfos = _SWUpdatePlugin.GetSWUpdateInfo(true, false, false, _GlobalSettingParam.GlobalSetting_About.SWVersion, true, false).Result;
                    ret = true;
                }
                else
                {
                    writelog($"[SW_CheckSWUpdate], _SWUpdatePlugin is null = {(_SWUpdatePlugin == null ? "Yes" : "No")}");
                    writelog($"[SW_CheckSWUpdate], _GlobalSettingParam is null = {(_GlobalSettingParam == null ? "Yes" : "No")}");
                    writelog($"[SW_CheckSWUpdate], _GlobalSettingParam.GlobalSetting_About is null = {(_GlobalSettingParam.GlobalSetting_About == null ? "Yes" : "No")}");
                    writelog($"[SW_CheckSWUpdate], _GlobalSettingParam.GlobalSetting_About.SWVersion Is NullOrEmpty = {(string.IsNullOrEmpty(_GlobalSettingParam.GlobalSetting_About.SWVersion) ? "Yes" : "No")}");
                }
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
                    string AppDataPath = WTSFunction.GetActiveUserLocalAppDataPath(Log);
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
                                    if (aLSConfig != null)
                                    {
                                        monitorSettings.ALSConfig = aLSConfig.AllValue;
                                    }
                                    else
                                    {
                                        writelog("[DisplayExportSettings]Export ALS : aLSConfig is null");
                                    }
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

                try
                {
                    //expot settings
                    if (_SettingsPlugin.DisplayExportSettings(monitorInfo.modelName, monitorInfo.edid.ServiceTag, settings, path).Result)
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
                                    else
                                    {
                                        writelog("[SentSettingstoTelementry] Send_Settings_Telementry is fail");
                                    }
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
            }
            else
            {
                writelog("[DisplayExportSettings]settings is null");
            }
            return Task.FromResult(false);
        }

        public Task<DisplayImportResultCode> DisplayImportSettings(MonitorInfo monitorInfo, bool isSameModel, string path)
        {
            writelog("[DisplayImportSettings] Import Settings");
            ImportVCP importVCP = new ImportVCP();
            ImpVCPSequence impVCPSequence = new ImpVCPSequence();
            if (_SettingsPlugin != null)
            {
                List<VCPCode> vcps = new List<VCPCode>();
                DisplayImportResultCode backendImportResult = _SettingsPlugin.DisplayImportSettings(path, isSameModel, monitorInfo.edid.ServiceTag, out DDPMImpExpSettings ImpExpSettings).Result;
                if ((int)backendImportResult > 0)
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
                                        return Task.FromResult(backendImportResult);
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
                                return Task.FromResult(DisplayImportResultCode.Error);
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
                                        if (monitorSettingsList.Find(ms => ms.easyArrangementDDPM.Desktops != null && ms.easyArrangementDDPM.Desktops.Count() > 0) != null) return Task.FromResult(DisplayImportResultCode.DoneWithEzMemoryCleared);
                                        return Task.FromResult(DisplayImportResultCode.Done);
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
                                return Task.FromResult(DisplayImportResultCode.Error);
                            }
                        }
                    }
                    else
                    {
                        writelog("[DisplayImportSettings]DDMImpSettingsFile is null");
                    }
                }
            }
            return Task.FromResult(DisplayImportResultCode.Error);
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

        public Task<DDPMImpExpSettings> ReadImportSettingsFile(string path)
        {
            DDPMImpExpSettings impExpSettings = new DDPMImpExpSettings();
            if (_SettingsPlugin != null && !string.IsNullOrEmpty(path))
            {
                impExpSettings = _SettingsPlugin.ReadImportSettingsFile(path).Result;
            }
            return Task.FromResult(impExpSettings);
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
                if (rt)
                    writelog("[DeviceMangerPlugin] Send Telementry for GamingEnhancementMode Success ...");
                else
                    writelog("[DeviceMangerPlugin] Send Telementry for GamingEnhancementMode Fail ...");
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
                if (rt)
                    writelog("[DeviceMangerPlugin] Send Telementry for GamingResponseTime Success ...");
                else
                    writelog("[DeviceMangerPlugin] Send Telementry for GamingResponseTime Fail ...");
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
                if (rt)
                    writelog("[DeviceMangerPlugin] Send Telementry for GamingDarkStabilizer Success ...");
                else
                    writelog("[DeviceMangerPlugin] Send Telementry for GamingDarkStabilizer Fail ...");
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
                if (rt)
                    writelog("[DeviceMangerPlugin] Send Telementry for GamingHDRType Success ...");
                else
                    writelog("[DeviceMangerPlugin] Send Telementry for GamingHDRType Fail ...");
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
                if (rt)
                    writelog("[DeviceMangerPlugin] Send Telementry for GamingVisionEngine Success ...");
                else
                    writelog("[DeviceMangerPlugin] Send Telementry for GamingVisionEngine Fail ...");
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

        public Task<bool> RestoreToDefaultMouse(string Guid, bool isFromCli = true)
        {
            writelog("DeviceMangerPlugin received RestoreToDefaultMouse requested ...");
            return _DTPProxyPlugin.RestoreToDefaultMouse(Guid, isFromCli);
        }

        public Task<bool> SetReportRate(string Guid, int newValue)
        {
            writelog("DeviceMangerPlugin received SetReportRate requested ...");
            return _DTPProxyPlugin.SetReportRate(Guid, newValue);
        }

        #endregion

        #region Pen

        public async Task<JArray> GetPenDeviceItemsEx()
        {
            return await Task.Run(() => _DTPProxyPlugin.GetPenDeviceItemsEx());
        }

        public Task<bool> StartKeyCapturePen()
        {
            return _DTPProxyPlugin.StartKeyCapturePen();
        }

        public Task<bool> FinishKeyCapturePen()
        {
            return _DTPProxyPlugin.FinishKeyCapturePen();
        }

        public Task<string> KeyCaptureData()
        {
            return _DTPProxyPlugin.KeyCaptureData();
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

        public Task<bool> RestoreToDefaultPen()
        {
            writelog("DeviceMangerPlugin received RestoreToDefaultPen requested ...");
            return _DTPProxyPlugin.RestoreToDefaultPen();
        }

        public Task<bool> RestoreRadialMenuToDefault()
        {
            writelog("DeviceMangerPlugin received RestoreRadialMenuToDefault requested ...");
            return _DTPProxyPlugin.RestoreRadialMenuToDefault();
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

        public async Task<string> GetKeyboardKeystrokeDisplayData(string Guid)
        {
            return await Task.Run(() => _DTPProxyPlugin.GetKeyboardKeystrokeDisplayData(Guid));
        }

        public async Task<bool> StartKeyboardKeystrokeRecording(string Guid)
        {
            return await Task.Run(() => _DTPProxyPlugin.StartKeyboardKeystrokeRecording(Guid));
        }

        public async Task<bool> StopKeyboardKeystrokeRecording(string Guid)
        {
            return await Task.Run(() => _DTPProxyPlugin.StopKeyboardKeystrokeRecording(Guid));
        }

        public Task SetKbAssignKeystrokeAction(string Guid, byte[] newValue)
        {
            writelog("DeviceMangerPlugin received SetKbAssignKeystrokeAction requested ...");
            _DTPProxyPlugin.SetKbAssignKeystrokeAction(Guid, newValue);
            return Task.FromResult(true);
        }

        public Task SetKbAssignDialogAction(string Guid, byte[] newValue)
        {
            writelog("DeviceMangerPlugin received SetKbAssignDialogAction requested ...");
            _DTPProxyPlugin.SetKbAssignDialogAction(Guid, newValue);
            return Task.FromResult(true);
        }

        public Task SetKbAssignedAction(string Guid, byte[] newValue)
        {
            writelog("DeviceMangerPlugin received SetKbAssignedAction requested ...");
            _DTPProxyPlugin.SetKbAssignedAction(Guid, newValue);
            return Task.FromResult(true);
        }

        public Task<bool> RestoreToDefaultKB(string Guid)
        {
            writelog("DeviceMangerPlugin received RestoreToDefaultKB requested ...");
            return _DTPProxyPlugin.RestoreToDefaultKB(Guid);
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

        public async Task<bool> GetIsWindowsHelloCapabilityVerified(string Guid)
        {
            return await Task.Run(() => _DTPProxyPlugin.GetIsWindowsHelloCapabilityVerified(Guid));
        }

        public async Task<bool> GetIsAllSupportedResolutionsFound(string Guid)
        {
            return await Task.Run(() => _DTPProxyPlugin.GetIsAllSupportedResolutionsFound(Guid));
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
            return await Task.Run(() => _DTPProxyPlugin.GeIsPropertyAntiFlickerSupported(Guid));
        }

        public async Task<int> GetAntiFlicker(string Guid)
        {
            return await Task.Run(() => _DTPProxyPlugin.GetAntiFlicker(Guid));
        }

        public async Task<bool> GetIsPropertyAutoFramingSupported(string Guid)
        {
            return await Task.Run(() => _DTPProxyPlugin.GetIsPropertyAutoFramingSupported(Guid));
        }

        public async Task<bool> GetIsESISupported(string Guid)
        {
            return await Task.Run(() => _DTPProxyPlugin.GetIsESISupported(Guid));
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

        public async Task<int> GetZoom(string Guid)
        {
            return await Task.Run(() => _DTPProxyPlugin.GetZoom(Guid));
        }

        public async Task<int> GetFocus(string Guid)
        {
            return await Task.Run(() => _DTPProxyPlugin.GetFocus(Guid));
        }

        public async Task<bool?> GetIsFocusOn(string Guid)
        {
            return await Task.Run(() => _DTPProxyPlugin.GetIsFocusOn(Guid));
        }

        public async Task<int> GetPriority(string Guid)
        {
            return await Task.Run(() => _DTPProxyPlugin.GetPriority(Guid));
        }

        public async Task<bool?> GetIsAutoFramingTransitionOn(string Guid)
        {
            return await Task.Run(() => _DTPProxyPlugin.GetIsAutoFramingTransitionOn(Guid));
        }

        public async Task<int> GetAutoFramingFrameSize(string Guid)
        {
            return await Task.Run(() => _DTPProxyPlugin.GetAutoFramingFrameSize(Guid));
        }

        public async Task<int> GetAutoFramingSensitivity(string Guid)
        {
            return await Task.Run(() => _DTPProxyPlugin.GetAutoFramingSensitivity(Guid));
        }

        public Task SetIsMicEnumerationOn(string guid, bool newValue)
        {
            writelog("DeviceMangerPlugin received SetIsMicEnumerationOn requested ...");
            writelog($"Target Guid is {guid}");
            writelog($"Target Value is {newValue}");
            _DTPProxyPlugin.SetIsMicEnumerationOn(guid, newValue);
            return Task.CompletedTask;
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

        public Task<bool> SetZoom(string guid, int newValue)
        {
            writelog("DeviceMangerPlugin received SetZoom requested ...");
            writelog($"Target Guid is {guid}");
            writelog($"Target Value is {newValue}");
            //_DTPProxyPlugin.SetZoom(guid, newValue);
            //return Task.FromResult(true);
            return _DTPProxyPlugin.SetZoom(guid, newValue);
        }

        public Task<bool> SetAutoFramingSensitivity(string guid, int newValue)
        {
            writelog("DeviceMangerPlugin received SetAutoFramingSensitivity requested ...");
            writelog($"Target Guid is {guid}");
            writelog($"Target Value is {newValue}");
            return _DTPProxyPlugin.SetAutoFramingSensitivity(guid, newValue);
        }

        public Task<bool> SetAutoFramingFrameSize(string guid, int newValue)
        {
            writelog("DeviceMangerPlugin received SetAutoFramingFrameSize requested ...");
            writelog($"Target Guid is {guid}");
            writelog($"Target Value is {newValue}");
            return _DTPProxyPlugin.SetAutoFramingFrameSize(guid, newValue);
        }

        public Task<bool> SetIsAutoFramingOn(string guid, bool newValue)
        {
            writelog("DeviceMangerPlugin received SetIsAutoFramingOn requested ...");
            writelog($"Target Guid is {guid}");
            writelog($"Target Value is {newValue}");
            return _DTPProxyPlugin.SetIsAutoFramingOn(guid, newValue);
        }

        public Task<bool> SetIsAutoFramingTransitionOn(string guid, bool newValue)
        {
            writelog("DeviceMangerPlugin received SetIsAutoFramingTransitionOn requested ...");
            writelog($"Target Guid is {guid}");
            writelog($"Target Value is {newValue}");
            return _DTPProxyPlugin.SetIsAutoFramingTransitionOn(guid, newValue);
        }

        public Task<bool> SetFieldOfView(string guid, int newValue)
        {
            writelog("DeviceMangerPlugin received SetFieldOfView requested ...");
            writelog($"Target Guid is {guid}");
            writelog($"Target Value is {newValue}");
            return _DTPProxyPlugin.SetFieldOfView(guid, newValue);
        }

        public Task<bool> SetIsFocusOn(string guid, bool newValue)
        {
            writelog("DeviceMangerPlugin received SetIsFocusOn requested ...");
            writelog($"Target Guid is {guid}");
            writelog($"Target Value is {newValue}");

            return _DTPProxyPlugin.SetIsFocusOn(guid, newValue);
        }

        public Task<bool> SetFocus(string guid, int newValue)
        {
            writelog("DeviceMangerPlugin received SetFocus requested ...");
            writelog($"Target Guid is {guid}");
            writelog($"Target Value is {newValue}");

            return _DTPProxyPlugin.SetFocus(guid, newValue);
        }

        public Task<bool> SetPriority(string guid, int newValue)
        {
            writelog("DeviceMangerPlugin received SetPriority requested ...");
            writelog($"Target Guid is {guid}");
            writelog($"Target Value is {newValue}");

            return _DTPProxyPlugin.SetPriority(guid, newValue);
        }

        public Task<bool> SetIsHDROn(string guid, bool newValue)
        {
            writelog("DeviceMangerPlugin received SetIsHDROn requested ...");
            writelog($"Target Guid is {guid}");
            writelog($"Target Value is {newValue}");
            return _DTPProxyPlugin.SetIsHDROn(guid, newValue);
        }

        public Task<bool> SetIsAutoWhiteBalanceOn(string guid, bool newValue)
        {
            writelog("DeviceMangerPlugin received SetIsAutoWhiteBalanceOn requested ...");
            writelog($"Target Guid is {guid}");
            writelog($"Target Value is {newValue}");

            return _DTPProxyPlugin.SetIsAutoWhiteBalanceOn(guid, newValue);
        }

        public Task<bool> SetAutoWhiteBalance(string guid, int newValue)
        {
            writelog("DeviceMangerPlugin received SetAutoWhiteBalance requested ...");
            writelog($"Target Guid is {guid}");
            writelog($"Target Value is {newValue}");
            return _DTPProxyPlugin.SetAutoWhiteBalance(guid, newValue);
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

        public async Task<bool?> GetIsPrioritizeExternalWebcam(string guid)
        {
            return await Task.Run(() => _DTPProxyPlugin.GetIsPrioritizeExternalWebcam(guid));
        }

        public async Task<bool> GetIsZoomMeetingActive(string guid)
        {
            return await Task.Run(() => _DTPProxyPlugin.GetIsZoomMeetingActive(guid));
        }

        public async Task<int> GetZoomMeetingType(string guid)
        {
            return await Task.Run(() => _DTPProxyPlugin.GetZoomMeetingTypeAsync(guid));
        }

        public async Task<bool> GetIsZoomScreenShareActive(string guid)
        {
            return await Task.Run(() => _DTPProxyPlugin.GetIsZoomScreenShareActive(guid));
        }

        public Task<DockData> GetDockData(string guid)
        {
            return Task.FromResult(_DTPProxyPlugin.GetDockData(guid).Result);
        }

        public Task<string> GetFirmwareVersionForDock(string guid)
        {
            return Task.FromResult(_DTPProxyPlugin.GetFirmwareVersionForDock(guid).Result);
        }

        public Task<string> GetDockServiceTagForDock(string guid)
        {
            return Task.FromResult(_DTPProxyPlugin.GetDockServiceTagForDock(guid).Result);
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
                if (rt)
                    writelog("[DeviceMangerPlugin] Send Telementry for DisplayLowBatteryLevel Success ...");
                else
                    writelog("[DeviceMangerPlugin] Send Telementry for DisplayLowBatteryLevel Fail ...");
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
                if (rt)
                    writelog("[DeviceMangerPlugin] Send Telementry for DisplayKeyboardLockKey Success ...");
                else
                    writelog("[DeviceMangerPlugin] Send Telementry for DisplayKeyboardLockKey Fail ...");
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
                if (rt)
                    writelog("[DeviceMangerPlugin] Send Telementry for DisplayWB7022CoverState Success ...");
                else
                    writelog("[DeviceMangerPlugin] Send Telementry for DisplayWB7022CoverState Fail ...");
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
                if (rt)
                    writelog("[DeviceMangerPlugin] Send Telementry for DisplayMuteState Success ...");
                else
                    writelog("[DeviceMangerPlugin] Send Telementry for DisplayMuteState Fail ...");
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
                if (rt)
                    writelog("[DeviceMangerPlugin] Send Telementry for DisplayColorPresetAndEasyMemory Success ...");
                else
                    writelog("[DeviceMangerPlugin] Send Telementry for DisplayColorPresetAndEasyMemory Fail ...");
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
                if (rt)
                    writelog("[DeviceMangerPlugin] Send Telementry for EnableQuickAccessWidget Success ...");
                else
                    writelog("[DeviceMangerPlugin] Send Telementry for EnableQuickAccessWidget Fail ...");
            }
            GlobalSettingChangeEvent?.Invoke(this, null);

            //Derek 1209 to handle QAM event
            HandleQAMV2();

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
                if (rt)
                    writelog("[DeviceMangerPlugin] Send Telementry for EnableQuickAccessWidget_Reminder Success ...");
                else
                    writelog("[DeviceMangerPlugin] Send Telementry for EnableQuickAccessWidget_Reminder Fail ...");
            }
            GlobalSettingChangeEvent?.Invoke(this, null);

            //Derek 1209 to handle OSD event
            HandleQAMOSDEvent();

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
            SetSkipSHA().Wait();
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
            foreach (var monitor in _AllInfoMonitors)
            {
                if (monitor.CapabilityDic.ContainsKey("E9"))
                    Task.Run(() => updatePBPModeStatus(monitor, "E9")).ConfigureAwait(false);
            }
            //register hotkey
            RegistHotkey(false);
            //
            _disDevHelper?.UpdateDDPMPluginInstances(_SettingsPlugin, this, _DisplayManagerPlugin);
        }

        private void RegistHotkey(bool unRegisterAll)
        {
            if (unRegisterAll)
                _pwr_Mon.UnRegisterAllHotKey();
            if (_hotkeySettings != null && _hotkeySettings.Count == 0)
            {
                _hotkeySettings = _SettingsPlugin.ReadHotkeySettings().Result;
            }
            if (_hotkeySettings != null && _hotkeySettings.Count > 0)
            {
                foreach (var settings in _hotkeySettings)
                {
                    if (settings.HotkeyInfo.Count > 0)
                    {
                        _pwr_Mon.RegisterHotKey(settings.HotkeyInfo);
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
            //regist osd key as hotkey
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
                    if (rt)
                        writelog("[DeviceMangerPlugin] Send Telementry for SaveMonitorAssetReport Success ...");
                    else
                        writelog("[DeviceMangerPlugin] Send Telementry for SaveMonitorAssetReport Fail ...");
                }
            }
            return Task.FromResult(ret);
        }

        public Task<bool> SaveLogFile(string saveFolderPath)
        {
            writelog($"{nameof(SaveLogFile)} start");
            bool ret = false;
            writelog($"{nameof(SaveLogFile)} saveFolderPath is null : {string.IsNullOrEmpty(saveFolderPath)}");
            if (_SettingsPlugin != null && !string.IsNullOrEmpty(saveFolderPath))
            {
                ret = _SettingsPlugin.SaveLog(saveFolderPath).Result;
            }
            //Telemetry Collection
            var ApplicationSettings_Function = new ApplicationSettings_Function();
            writelog("[DeviceMangerPlugin] Send Telemetry for SaveDiagnosticReport...");
            bool dtm = false;//[Dean 1126] Telemetry result should not impact original result
            dtm = ApplicationSettings_Function.Send_SaveDiagnosticReport_Telementry(_TelementryScheduler, _AllInfoMonitors, 1);
            if (dtm)
                writelog("[DeviceMangerPlugin] Send Telemetry for SaveDiagnosticReport Success ...");
            else
                writelog("[DeviceMangerPlugin] Send Telemetry for SaveDiagnosticReport Fail ...");

            writelog($"{nameof(SaveLogFile)} end");
            return Task.FromResult(ret);
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

        #endregion

        #endregion

        #region WebCamera

        public enum ZoomMeetingType
        {
            CONF_3RD_EVENT_MEETING = 0,
            CONF_3RD_EVENT_PHONE = 1,
            CONF_3RD_EVENT_WEBINAR = 2,
            CONF_3RD_EVENT_WEBINAR_VIEWONLY = 3,
            ZOOM_MEETING_TYPE_UNKNOW
        }

        private bool _IsZoomScreenShareActive = false;
        private bool _IsZoomMeetingActive = false;
        private ZoomMeetingType _ZoomMeetingType = ZoomMeetingType.ZOOM_MEETING_TYPE_UNKNOW;
        private bool isWindowsScreenNotLocked = true;
        private string QAMWebcamDeviceGuid = string.Empty;
        private bool isOpenOSDWhenQAMClosed = true; //Derek 1215 for PIMS 332040

        //private bool isHiddenConditionsMet = false;
        private int currentZoomValue = -1;

        private EventMsg eventMsg = new EventMsg();

        private void ResetQAMCondition()
        {
            _IsZoomScreenShareActive = false;
            _IsZoomMeetingActive = false;
            _ZoomMeetingType = ZoomMeetingType.ZOOM_MEETING_TYPE_UNKNOW;
            isWindowsScreenNotLocked = true;

            if (1 != GetWebcamDeviceCount())
                QAMWebcamDeviceGuid = string.Empty;
        }

        //Derek 1206
        private void HandleQAMOSDEvent()
        {
            writelog($"HandleQAMOSDEvent start");

            try
            {
                int devCnt = GetWebcamDeviceCount();
                writelog($"GetWebcamDeviceCount = {devCnt}, current webcam device ID = {QAMWebcamDeviceGuid}");

                if (1 != devCnt || _GlobalSettingParam == null || !isWindowsScreenNotLocked ||
                    _GlobalSettingParam.GlobalSetting_WidgetSettings == null ||
                    !_GlobalSettingParam.GlobalSetting_WidgetSettings.EnableQuickAccessWidget_Reminder
                    //QAMWebcamDeviceGuid == string.Empty ||
                    //_ZoomMeetingType != ZoomMeetingType.CONF_3RD_EVENT_MEETING
                    //Derek 1206 test condition due to we can't receive zoom meeting type changed event
                    //_ZoomMeetingType != ZoomMeetingType.ZOOM_MEETING_TYPE_UNKNOW
                    )
                {
                    CloseQAMOSD();

                    writelog($"CloseQAMOSD condition occur, close OSD if it's opened.");
                }
                //OSD just opened by QAM close event
                //else if (_GlobalSettingParam.GlobalSetting_WidgetSettings.EnableQuickAccessWidget_Reminder
                //            && _QAM == null)
                //{
                //    ShowOSD(Screen.PrimaryScreen.DeviceName, OSDType.QAM);

                //    writelog($"Open OSD due to QAM is inactive and global setting change to {_GlobalSettingParam.GlobalSetting_WidgetSettings.EnableQuickAccessWidget_Reminder}");
                //}
            }
            catch (Exception e)
            {
                writelog($"Catch exception[{e.Message}] when Handle HandleQAMOSDEvent process!");
            }

            writelog($"HandleQAMOSDEvent end");
        }

        private void HandleQAMV2()
        {
            writelog($"HandleQAMV2 start");

            try
            {
                int devCnt = GetWebcamDeviceCount();
                writelog($"GetWebcamDeviceCount = {devCnt}, current webcam device ID = {QAMWebcamDeviceGuid}");

                if (1 != devCnt || _GlobalSettingParam == null || !isWindowsScreenNotLocked ||
                    _GlobalSettingParam.GlobalSetting_WidgetSettings == null //||
                                                                             //QAMWebcamDeviceGuid == string.Empty ||
                                                                             //_ZoomMeetingType != ZoomMeetingType.CONF_3RD_EVENT_MEETING
                                                                             //Derek 1206 test condition due to we can't receive zoom meeting type changed event
                                                                             //_ZoomMeetingType != ZoomMeetingType.ZOOM_MEETING_TYPE_UNKNOW
                    )
                {
                    ResetQAMCondition();
                    QAMClose(false);
                    CloseQAMOSD();

                    writelog($"Abnormal condition occur, close QAM/OSD if it's opened.");

                    return;
                }

                _IsZoomScreenShareActive = _DTPProxyPlugin.GetIsZoomScreenShareActive(QAMWebcamDeviceGuid).Result;
                _IsZoomMeetingActive = _DTPProxyPlugin.GetIsZoomMeetingActive(QAMWebcamDeviceGuid).Result;
                _ZoomMeetingType = (ZoomMeetingType)_DTPProxyPlugin.GetZoomMeetingTypeAsync(QAMWebcamDeviceGuid).Result;
                writelog($"Zoom meeting condition, _IsZoomScreenShareActive = {_IsZoomScreenShareActive}, _IsZoomMeetingActive = {_IsZoomMeetingActive}, _ZoomMeetingType = {_ZoomMeetingType}");

                //OSD condition
                //if (!_GlobalSettingParam.GlobalSetting_WidgetSettings.EnableQuickAccessWidget_Reminder)
                //{
                //    CloseQAMOSD();

                //    writelog($"Close OSD due to global setting change to {_GlobalSettingParam.GlobalSetting_WidgetSettings.EnableQuickAccessWidget_Reminder}");
                //}
                //else if (null == _QAM && _GlobalSettingParam.GlobalSetting_WidgetSettings.EnableQuickAccessWidget_Reminder)
                //{
                //    ShowOSD(Screen.PrimaryScreen.DeviceName, OSDType.QAM);

                //    writelog($"Open OSD due to QAM is inactive and global setting change to {_GlobalSettingParam.GlobalSetting_WidgetSettings.EnableQuickAccessWidget_Reminder}");
                //}

                //QAM condition
                if (_GlobalSettingParam.GlobalSetting_WidgetSettings.EnableQuickAccessWidget)
                {
                    //Active
                    if (!_IsZoomScreenShareActive && _IsZoomMeetingActive)
                    {
                        writelog($"HandleQAM receive CallQAM_UI event");

                        CloseQAMOSD();
                        CallQAM_UI(this);
                    }
                    //Hide
                    else if (_IsZoomScreenShareActive && _IsZoomMeetingActive)
                    {
                        writelog($"HandleQAM receive QAMHide event");

                        CloseQAMOSD();
                        QAMHide();
                    }
                    else
                    {
                        writelog($"HandleQAM receive QAMClose event");

                        QAMClose(true);
                    }

                    isOpenOSDWhenQAMClosed = true;
                }
                else
                {
                    //Derek 1216
                    //PIMS 332041 OSD should not be seen on set Widget setting- Activae Quick Access widget
                    //during Zoom conference calls" = Off (uncheck)
                    CloseQAMOSD();

                    QAMClose(false);

                    writelog($"Close QAM/OSD due to global setting change to {_GlobalSettingParam.GlobalSetting_WidgetSettings.EnableQuickAccessWidget}");
                }
            }
            catch (Exception e)
            {
                ResetQAMCondition();
                writelog($"Catch exception[{e.Message}] when Handle QAM process!");
            }

            ResetQAMCondition();
            writelog($"HandleQAMV2 done");
        }

        private void CloseQAMOSD()
        {
            _OSD_Controler.QAMHotKeyWin_CloseWindow();
        }

        private void HandleQAM()
        {
            writelog($"HandleQAM start");
            //writelog($"HandleQAM: GetDevices_WithoutAwait go");
            List<DeviceInfo> deviceInfos = GetDevices_WithoutAwait().Result.deviceInfo.FindAll(x => (x.PhysicalDeviceType.Equals(DeviceType.LogicalWebcam) || x.PhysicalDeviceType.Equals(DeviceType.PhysicalWebcam)));
            writelog($"HandleQAM: deviceInfos.Count:{deviceInfos.Count}");

            if (deviceInfos == null || _GlobalSettingParam == null || _GlobalSettingParam.GlobalSetting_WidgetSettings == null)
            {
                writelog($"Get null object when handleQAM start");

                return;
            }

            writelog($"_IsZoomMeetingActive = {_IsZoomMeetingActive}, _ZoomMeetingType = {_ZoomMeetingType}, _IsZoomScreenShareActive = {_IsZoomScreenShareActive}");

            //Active state
            if (_IsZoomMeetingActive && _ZoomMeetingType == ZoomMeetingType.ZOOM_MEETING_TYPE_UNKNOW
                && deviceInfos.Count == 1 && _GlobalSettingParam.GlobalSetting_WidgetSettings.EnableQuickAccessWidget
                && isWindowsScreenNotLocked) //Derek 1206 test condition due to we can't receive zoom meeting type changed event
            //if (_IsZoomMeetingActive && _ZoomMeetingType == ZoomMeetingType.CONF_3RD_EVENT_MEETING
            //   && deviceInfos.Count == 1 && _GlobalSettingParam.GlobalSetting_WidgetSettings.EnableQuickAccessWidget
            //   && isWindowsScreenNotLocked)
            {
                writelog($"HandleQAM receive CallQAM_UI event");
                _IsZoomMeetingActive = false;
                CallQAM_UI(this);
            }
            //Hidden state
            else if (_IsZoomScreenShareActive)
            {
                writelog($"HandleQAM receive QAMHide event");

                QAMHide();
            }
            //OSD
            //else if (true)
            //{
            //    ShowOSD(Screen.PrimaryScreen.DeviceName, OSDType.QAM);
            //}
            else
            {
                writelog($"HandleQAM receive QAMClose event");

                QAMClose(false);
            }

            //if (_ZoomMeetingType == ZoomMeetingType.CONF_3RD_EVENT_MEETING)
            //{
            //    //if (_QAM == null && _GlobalSettingParam != null && _GlobalSettingParam.GlobalSetting_WidgetSettings != null)
            //    if (_QAM == null)
            //    {
            //        if (_GlobalSettingParam.GlobalSetting_WidgetSettings.EnableQuickAccessWidget_Reminder)
            //        {
            //            ShowOSD(Screen.PrimaryScreen.DeviceName, OSDType.QAM);
            //        }
            //        else if (deviceInfos.Count == 1 && _GlobalSettingParam.GlobalSetting_WidgetSettings.EnableQuickAccessWidget)
            //        {
            //            CallQAM_UI(this);
            //        }
            //    }
            //    else
            //    {
            //        if (deviceInfos != null && deviceInfos.Count == 1)
            //        {
            //            CallQAM_UI(this);
            //        }
            //        if (deviceInfos != null && deviceInfos.Count > 1)
            //        {
            //            QAMClose();
            //        }
            //        if (_IsZoomScreenShareActive)
            //        {
            //            QAMHide();
            //        }
            //        else
            //        {
            //            QAMShow();
            //        }
            //    }
            //}
            //else
            //{
            //    QAMClose();
            //}

            writelog($"HandleQAM done");
        }

        private void HandleQAMEvent(string msg)
        {
            eventMsg = EventMsg.CreateEventObjectFromEventMsg(msg);
            int WebcamDevCnt = GetWebcamDeviceCount();

            if (null == eventMsg)
                return;

            bool isQAMHandleEvent = false;

            switch (eventMsg.EventType)
            {
                case "Webcam_ZoomChanged":
                    //isQAMHandleEvent = true;
                    if (!int.TryParse(eventMsg.NewValue, out currentZoomValue))
                        currentZoomValue = -1;

                    break;

                case "Webcam_IsZoomMeetingActiveChanged":
                    isQAMHandleEvent = true;

                    if (1 == WebcamDevCnt)
                        QAMWebcamDeviceGuid = eventMsg.DeviceId;

                    //if (!bool.TryParse(eventMsg.NewValue, out _IsZoomMeetingActive))
                    //    _IsZoomMeetingActive = false;

                    break;

                case "Webcam_IsZoomScreenShareActiveChanged":
                    isQAMHandleEvent = true;

                    if (1 == WebcamDevCnt)
                        QAMWebcamDeviceGuid = eventMsg.DeviceId;

                    //if (!bool.TryParse(eventMsg.NewValue, out _IsZoomScreenShareActive))
                    //    _IsZoomScreenShareActive = false;

                    break;

                case "Webcam_ZoomMeetingTypeChanged":
                    isQAMHandleEvent = true;

                    if (1 == WebcamDevCnt)
                        QAMWebcamDeviceGuid = eventMsg.DeviceId;

                    //int type = (int)ZoomMeetingType.ZOOM_MEETING_TYPE_UNKNOW;

                    //if (int.TryParse(eventMsg.NewValue, out type))
                    //    _ZoomMeetingType = (ZoomMeetingType)type;
                    //else
                    //    _ZoomMeetingType = ZoomMeetingType.ZOOM_MEETING_TYPE_UNKNOW;

                    break;

                case "Webcam_Disconnected":
                    isQAMHandleEvent = true;
                    break;

                case "Webcam_Connected":
                    isQAMHandleEvent = true;

                    if (1 == WebcamDevCnt)
                        QAMWebcamDeviceGuid = eventMsg.DeviceId;
                    break;

                default:
                    break;
            }

            if (isQAMHandleEvent)
                HandleQAMV2();
        }

        //Marked by Derek 1125 because they had covered by WebcamEventHandler
        //private void ZoomChanged(object sender, ZoomChangedArgs e)
        //{
        //    writelog($"[DeviceManager] IsZoomScreenShareActiveChanged e == null: {e == null}");
        //    if (e != null)
        //    {
        //        writelog($"[DeviceManager] IsZoomScreenShareActiveChanged e.Zoom: {e.Zoom}");
        //    }
        //}
        //private void ZoomMeetingTypeChanged(object sender, ZoomMeetingTypeChangedArgs e)
        //{
        //    writelog($"[DeviceManager] ZoomMeetingTypeChanged e == null: {e == null}");
        //    if (e != null)
        //    {
        //        writelog($"[DeviceManager] ZoomMeetingTypeChanged e.ZoomMeetingType: {e.ZoomMeetingType}");
        //        _ZoomMeetingType = (ZoomMeetingType)e.ZoomMeetingType;
        //    }
        //}
        //private void IsZoomMeetingActiveChanged(object sender, IsZoomMeetingActiveChangedArgs e)
        //{
        //    writelog($"[DeviceManager] IsZoomMeetingActiveChanged e == null: {e == null}");
        //    if (e != null)
        //    {
        //        writelog($"[DeviceManager] IsZoomMeetingActiveChanged e.IsZoomMeetingActive: {e.IsZoomMeetingActive}");
        //        _IsZoomMeetingActive = e.IsZoomMeetingActive;
        //    }
        //}
        //private void IsZoomScreenShareActiveChanged(object sender, IsZoomScreenShareActiveChangedArgs e)
        //{
        //    writelog($"[DeviceManager] IsZoomScreenShareActiveChanged e == null: {e == null}");
        //    if (e != null)
        //    {
        //        writelog($"[DeviceManager] IsZoomScreenShareActiveChanged e.IsZoomScreenShareActive: {e.IsZoomScreenShareActive}");
        //        _IsZoomScreenShareActive = e.IsZoomScreenShareActive;
        //    }
        //}
        private void QAMCloseEvent(object o, EventArgs e)
        {
            if (_QAM != null)
            {
                QAM_Position = new Point(_QAM.Left, _QAM.Top);
                _QAM.Closed -= QAMCloseEvent;
                _QAM = null;

                if (_GlobalSettingParam != null && _GlobalSettingParam.GlobalSetting_WidgetSettings != null
                    && _GlobalSettingParam.GlobalSetting_WidgetSettings.EnableQuickAccessWidget_Reminder && 
                    isOpenOSDWhenQAMClosed)
                {
                    ShowOSD(Screen.PrimaryScreen.DeviceName, OSDType.QAM);
                }
            }
        }

        private Task QAMHide()
        {
            writelog($"QAMHide Start");

            if (null == _QAM)
                return Task.CompletedTask;

            try
            {
                writelog($"Try to run QAMHide");
                _QAM?.Dispatcher.Invoke(() => _QAM?.Hide());
                //Dispatcher.Run();

                //_QAM?.Dispatcher.Invoke(() => _QAM?.SetToBottomWindow());
            }
            catch (Exception e)
            {
                writelog($"Catch Exception[{e.Message}] when run QAMHide");
            }

            writelog($"QAMHide done");

            return Task.CompletedTask;
        }

        //private void QAMShow()
        //{
        //    writelog($"QAMShow Start");

        //    if (_QAM != null)
        //    {
        //        //writelog($"QAMHide QAMShow go");
        //        _QAM.Show();
        //        //writelog($"QAMHide QAMShow done");
        //    }

        //    writelog($"QAMShow done");
        //}
        private Task QAMClose(bool openQAMOSD)
        {
            if (null == _QAM)
                return Task.CompletedTask;

            writelog($"QAMClose Start");

            try
            {
                writelog($"Try to run QAMClose");
                _QAM?.Dispatcher.BeginInvoke(DispatcherPriority.Normal, () => _QAM?.Close());
                isOpenOSDWhenQAMClosed = openQAMOSD;
            }
            catch (Exception e)
            {
                writelog($"Catch Exception[{e.Message}] when run QAMClose");
            }

            writelog($"QAMClose done");

            return Task.CompletedTask;
        }

        private int GetWebcamDeviceCount()
        {
            int result = 0;

            try
            {
                List<DeviceInfo> deviceInfos = GetDevices_WithoutAwait().Result.deviceInfo.FindAll(x => x.PhysicalDeviceType.Equals(DeviceType.LogicalWebcam) ||
                x.PhysicalDeviceType.Equals(DeviceType.PhysicalWebcam));

                if (deviceInfos != null)
                    result = deviceInfos.Count;
            }
            catch (Exception e)
            {
                writelog($"Catch exception[{e.Message}] when GetWebcamDeviceCount");
            }

            return result;
        }

        private Task CallQAM_UI(DeviceMangerPlugin deviceMangerPlugin)
        {
            writelog($"CallQAM_UI: Start");

            if (_QAM == null)
            {
                //writelog($"CallQAM_UI: Go");
                //List<DeviceInfo> deviceInfos = GetDevices_WithoutAwait().Result.deviceInfo.FindAll(x => (x.PhysicalDeviceType.Equals(DeviceType.LogicalWebcam) || x.PhysicalDeviceType.Equals(DeviceType.PhysicalWebcam)));
                //writelog($"CallQAM_UI: deviceInfos.Count:{deviceInfos.Count}");
                //if (deviceInfos.Count == 1)
                {
                    writelog($"CallQAM_UI: have Webcam show QAM");

                    Thread threadQAM = new Thread(() =>
                    {
                        _QAM = new QAMPage(deviceMangerPlugin, Log);
                        _QAM.Closed += QAMCloseEvent;

                        //if (QAM_Position != null && (QAM_Position.X != 0 && QAM_Position.Y != 0))
                        if (QAM_Position.X != 0 && QAM_Position.Y != 0)
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

                    threadQAM.SetApartmentState(ApartmentState.STA);
                    threadQAM.Start();
                }
            }
            else
            {
                //_QAM.Show();
                _QAM?.Dispatcher.Invoke(() => _QAM?.Show());
                Dispatcher.Run();
            }

            //close DDPM UI
            //CloseDDPM();  //Derek 1209

            //close OSD
            CloseQAMOSD();

            writelog($"CallQAM_UI: done");

            return Task.CompletedTask;
        }

        private Task CloseDDPM()
        {
            UpdateUINotify e = new()
            {
                UI_Field_Name = "QAMEvent_QAMIsLaunched"
            };

            OnUIUpdateNotify(e);

            return Task.CompletedTask;
        }

        private Task NavigateDDPMToWidgetSettingPage()
        {
            UpdateUINotify e = new()
            {
                UI_Field_Name = "QAMEvent_NavigateToWidgetSettingPage"
            };

            OnUIUpdateNotify(e);

            return Task.CompletedTask;
        }

        //private void _QAM_UpdateUINotify(object sender, UpdateUINotify e)
        //{
        //    OnUIUpdateNotify(e);
        //}

        public Task<int> GetCurrentPollingRate()
        {
            return Task.FromResult(_millisecond);
        }

        public Task<bool> GetIsWidgetSettingPageLoadedByQAMAsync()
        {
            return Task.FromResult(isWidgetSettingPageLoadedByQAM);
        }

        public Task SetIsWidgetSettingPageLoadedByQAMAsync(bool newValue)
        {
            isWidgetSettingPageLoadedByQAM = newValue;
            writelog($"isWidgetSettingPageLoadedByQAM: {newValue}");

            if (isWidgetSettingPageLoadedByQAM)
            {
                NavigateDDPMToWidgetSettingPage();
            }

            return Task.CompletedTask;
        }

        public Task SetIsDDPMLaunchByQAMAsync(bool newValue)
        {
            isDDPMLaunchedByQAM = newValue;
            writelog($"IsDDPMLaunchByQAM: {newValue}");

            return Task.CompletedTask;
        }

        public Task<bool> GetIsDDPMLaunchByQAM()
        {
            writelog($"return IsDDPMLaunchByQAM: {isDDPMLaunchedByQAM}");

            return Task.FromResult(isDDPMLaunchedByQAM);
        }

        public Task SetIsDDPMHomepageReadyAsync(bool newValue)
        {
            isDDPMHomepageReady = newValue;

            writelog($"UI SetIsDDPMHomepageReadyAsync: {newValue}");

            //info homepage navigate to webcam preview page
            //if (isDDPMLaunchedByQAM && isDDPMHomepageReady)
            //{
            //    UpdateUINotify e = new UpdateUINotify();
            //    e.UI_Field_Name = "QAMEvent_StartPreview";

            //    OnUIUpdateNotify(e);
            //    isDDPMHomepageReady = false;
            //    //isDDPMLaunchedByQAM = false;

            //    QAMClose();
            //}

            //Derek 1209
            //QAMClose();

            //Derek 1216 info homepage navigate to widget setting page
            isDDPMHomepageReady = false;
            isDDPMLaunchedByQAM = false;
            SetIsWidgetSettingPageLoadedByQAMAsync(true);

            return Task.CompletedTask;
        }

        public Task CloseQAMByDDPM()
        {
            writelog($"UI send command CloseQAMByDDPM");

            QAMClose(true);

            return Task.CompletedTask;
        }

        //Derek 1212
        public Task SyncWebcamProfile(string profileName, bool isActionFromQAM = true)
        {
            UpdateUINotify e = new UpdateUINotify();

            if (isActionFromQAM)
                e.UI_Field_Name = $"WebcamProfileFromQAM:{profileName}"; //message to DDPM
            else
                e.UI_Field_Name = $"WebcamProfileFromDDPM:{profileName}";//message to QAM

            OnUIUpdateNotify(e);

            return Task.CompletedTask;
        }

        #endregion

        #region Private Methods

        private void SystemEvents_DisplaySettingsChanged(object sender, EventArgs e)
        {
            writelog("[DeviceMangerPlugin] YYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYY");

            _SystemEvents_DisplaySettingsChanged(new DebouncerArg()
            {
                sender = sender,
                eventArgs = e,
            });
        }

        private void _SystemEvents_DisplaySettingsChanged(object _arg)
        {
            try
            {
                DebouncerArg arg = null;

                if (_arg != null)
                {
                    arg = (DebouncerArg)_arg;
                    writelog($"Receive DisplaySettingsChanged: {arg.sender}, e:{arg.eventArgs}, rescan monitor");
                }
                else
                    writelog($"Receive Re-GetMonitor, rescan monitor");

                if (displayInOut)
                {
                    if (_AllInfoMonitors != null)
                        _AllInfoMonitors.Clear();

                    if (_UpdateProgress != null && _FWUpdatePlugin != null)
                    {
                        writelog($"DisplaySettingsChanged: Rrconnect FWU eventv go");
                        _FWUpdatePlugin.ProgressUpdate_Notify -= show_fwProgressUpdateEvent;
                        _FWUpdatePlugin.ProgressUpdate_Notify += show_fwProgressUpdateEvent;
                        ProgressUpdate_Notify -= _UpdateProgress._FWUpdatePlugin_ProgressUpdate;
                        ProgressUpdate_Notify += _UpdateProgress._FWUpdatePlugin_ProgressUpdate;
                        writelog($"DisplaySettingsChanged: Rrconnect FWU eventv don");
                    }
                    writelog($"DisplaySettingsChanged: displayInOut is true");

                    if (isLetDisplayServiceIdle == true)
                    {
                        writelog("The idle state is true to drop display settings change event, need caller to unblock this param");
                        return;
                    }

                    //writelog("[DeviceMangerPlugin] SystemEvents_DisplaySettingsChanged() Bruce count Screen Length ...");
                    //Bruce 08-09 Added judgment that if the number of screens does not change, the screen orientation adjustment function will not be performed. (For example: PxP change will trigger this event, but the screen is not actually plugged in or out)
                    //bool displayDeviceNumChange = false;
                    //int AllScreens = Screen.AllScreens.Length;
                    //if (_lastScreenCount != AllScreens)
                    //{
                    //    displayDeviceNumChange = true;
                    //    _lastScreenCount = AllScreens;
                    //}
                    //writelog("[DeviceMangerPlugin] SystemEvents_DisplaySettingsChanged() Bruce count Screen Length finish ...");

                    try
                    {
                        writelog("[DeviceMangerPlugin] XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX");
                        writelog("[DeviceMangerPlugin] SystemEvents_DisplaySettingsChanged() Ready trigger cancel ...");

                        if (_ReGetcancellationTokenSource != null)
                        {
                            writelog("[DeviceMangerPlugin] SystemEvents_DisplaySettingsChanged() _ReGetcancellationTokenSource trigger cancel ...");
                            _ReGetcancellationTokenSource.Cancel();
                        }
                    }
                    catch (TaskCanceledException)
                    {
                        writelog("[DeviceMangerPlugin] I_SystemEvents_DisplaySettingsChanged() trigger cancel cancellation happened ...");
                        _ReGetcancellationTokenSource.Dispose();
                    }
                    catch (OperationCanceledException)
                    {
                        writelog("[DeviceMangerPlugin] I_SystemEvents_DisplaySettingsChanged() trigger cancel cancellation happened ...");
                        _ReGetcancellationTokenSource.Dispose();
                    }
                    catch (Exception ex)
                    {
                        writelog($"[DeviceMangerPlugin] I_SystemEvents_DisplaySettingsChanged() ...there is an exception-- ({ex.Message})");
                        _ReGetcancellationTokenSource.Dispose();
                    }
                    finally
                    {
                        try
                        {
                            _ReGetcancellationTokenSource = new CancellationTokenSource();
                            var token = _ReGetcancellationTokenSource.Token;

                            writelog("[DeviceMangerPlugin] _SystemEvents_DisplaySettingsChanged() into Re-GetDevices ...");
                            //Call VCP to catch updated monitor info
                            _AllInfoMonitors = new List<MonitorInfo>(_DisplayManagerPlugin.Re_GetMonitors(token).Result);

                            token.ThrowIfCancellationRequested();
                            //review monitor list to check duplicated data
                            ReviewAllMonitorToAvoidDuplicatedInfo();

                            //List<MonitorInfo> new_mo = new List<MonitorInfo>();
                            //if (_AllInfoMonitors.Count > 0)
                            //    new_mo.AddRange(_AllInfoMonitors);

                            writelog($"[DeviceManager] _SystemEvents_DisplaySettingsChanged() Got event, monitor count {_AllInfoMonitors.Count}");

                            token.ThrowIfCancellationRequested();
                            if (_AllInfoMonitors.Count > 0)
                                OnDeviceChanged(_AllInfoMonitors[0], null, DeviceChangedType.NotifyOnly, token, "DisplayChanged");//DeviceChangedType.Display_PlugIn);
                            else
                                OnDeviceChanged(null, null, DeviceChangedType.NotifyOnly, token, "DisplayChanged");

                            writelog("[DeviceMangerPlugin] _SystemEvents_DisplaySettingsChanged() OnDeviceChanged finish ...");

                            //Robert_Lin, 2024-9-9 Signal a DisplaySettingsChanged event through Agent
                            //Anyone who would like to receive this event, you can add below code: (refer to EAPlugin.cs)
                            // _agent.RegisterForEvent(AgentEventNames.DisplaySettingsChanged, DisplaySettingsChangedHandler);
                            //
                            // private void DisplaySettingsChangedHandler(object sender, EventManagerArgs e)
                            // {
                            //    your handler code
                            // }
                            //

                            if (_agent != null && !token.IsCancellationRequested)
                                _agent.RaiseEvent(AgentEventNames.DisplaySettingsChanged, this, new EventManagerArgs());
                            writelog("[DeviceMangerPlugin] _SystemEvents_DisplaySettingsChanged() _agent.RaiseEvent finish ...");

                            if (_AllInfoMonitors.Count > 0 && !token.IsCancellationRequested)
                            {
                                Task.Run(() =>
                                {
                                    //Telementry Collection
                                    var rt = false;
                                    var DeviceTypeConnected_Function = new DeviceTypeConnected_Function();
                                    writelog("[DeviceMangerPlugin] [Telementry] Send Telementry for DeviceTypeConnected_Function...");
                                    rt = DeviceTypeConnected_Function.DeviceTypeConnected_Telementry(_TelementryScheduler, _AllInfoMonitors);
                                    if (rt)
                                        writelog("[DeviceMangerPlugin] [Telementry] Send Telementry for DeviceTypeConnected_Function Success ...");
                                    else
                                        writelog("[DeviceMangerPlugin] [Telementry] Send Telementry for DeviceTypeConnected_Function Fail ...");
                                }, token).ConfigureAwait(false);
                            }

                            ////1117 Bruce 不用自動旋轉把下兩行註解
                            //if (displayDeviceNumChange && _AllInfoMonitors.Count > 0)
                            //_DisplayManagerPlugin.SetDisplayOrientation(_AllInfoMonitors).Wait();
                            //writelog("[DeviceMangerPlugin] SystemEvents_DisplaySettingsChanged() SetDisplayOrientation finish ...");

                            token.ThrowIfCancellationRequested();
                            _DisplayManagerPlugin.UpdateExistAlsConfig(_AllInfoMonitors.ToList());
                            writelog("[DeviceMangerPlugin] _SystemEvents_DisplaySettingsChanged() UpdateExistAlsConfig finish ...");
                            writelog("[DeviceMangerPlugin] _SystemEvents_DisplaySettingsChanged() Re-GetDevices finish ...");

                            if (_AllInfoMonitors != null && _AllInfoMonitors.Count > 0 && !token.IsCancellationRequested)
                                Task.Run(() => _disDevHelper?.CheckAndTriggerToastWhileMonitorPlugged(_millisecond, _AllInfoMonitors.ToList(), _SettingsPlugin));
                        }
                        catch (TaskCanceledException)
                        {
                            writelog("[DeviceMangerPlugin] II_SystemEvents_DisplaySettingsChanged() trigger cancel cancellation happened ...");
                            OnDeviceChanged(null, null, DeviceChangedType.NotifyOnly, CancellationToken.None, "DisplayChanged");
                        }
                        catch (OperationCanceledException)
                        {
                            writelog("[DeviceMangerPlugin] II_SystemEvents_DisplaySettingsChanged() trigger cancel cancellation happened ...");
                            OnDeviceChanged(null, null, DeviceChangedType.NotifyOnly, CancellationToken.None, "DisplayChanged");
                        }
                        catch (Exception ex)
                        {
                            // Failed to complete due to e exception
                            writelog($"[DeviceMangerPlugin] II_SystemEvents_DisplaySettingsChanged() ...there is an exception-- ({ex.Message})");
                            OnDeviceChanged(null, null, DeviceChangedType.NotifyOnly, CancellationToken.None, "DisplayChanged");
                        }
                    }
                }
                else//Add by Bruce
                {
                    if (_arg != null)
                        writelog($"DisplaySettingsChanged: {arg.sender}, e:{arg.eventArgs}, By pass.");
                    else
                        writelog($"GetMonitor, By pass.");

                    displayInOut = true;
                }
            }
            catch (Exception x)
            {
                writelog($"[DeviceMangerPlugin] III_SystemEvents_DisplaySettingsChanged()...there is an exception-- ({x.Message})");
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
            //if (_DisplayManagerPlugin != null)
            //{
            //    displayInOut = false;
            //    _DisplayManagerPlugin.SetDisplayOrientation(e.monitors).Wait();
            //    displayInOut = true;
            //}
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
                //SupportedNKVMMonitors();
            }

            if (_AllInfoMonitors != null && _AllInfoMonitors.Count > 0)
                Task.Run(() => _disDevHelper?.CheckAndTriggerToastWhileMonitorPlugged(_millisecond, e.monitors, _SettingsPlugin));
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
            writelog($"{nameof(OnProgressUpdateEvent)} start");
            //ProgressUpdate_Notify?.Invoke(this, fWUpdateInfo);
            EventHandler<UpdateProgressInfo> handler = ProgressUpdate_Notify;
            writelog($"{nameof(OnProgressUpdateEvent)} handler : {handler}");
            if (handler != null)
            {
                writelog($"{nameof(OnProgressUpdateEvent)} {fWUpdateInfo.DeviceName} {fWUpdateInfo.TheLatestVersion} {fWUpdateInfo.ProcessName} {fWUpdateInfo.ProcessProgress} {DateTime.Now}");
                handler.Invoke(this, fWUpdateInfo);
            }
            writelog($"{nameof(OnProgressUpdateEvent)} done");
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
            DeviceChangedEventArgs _EventArgs = new DeviceChangedEventArgs();
            if (_UpdateProgress != null && _FWUpdatePlugin != null)
            {
                writelog($"OnDeviceChanged: Rrconnect FWU eventv go");
                _FWUpdatePlugin.ProgressUpdate_Notify -= show_fwProgressUpdateEvent;
                _FWUpdatePlugin.ProgressUpdate_Notify += show_fwProgressUpdateEvent;
                ProgressUpdate_Notify -= _UpdateProgress._FWUpdatePlugin_ProgressUpdate;
                ProgressUpdate_Notify += _UpdateProgress._FWUpdatePlugin_ProgressUpdate;
                writelog($"OnDeviceChanged: Rrconnect FWU eventv don");
            }

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
                Task.Run(() => handler.Invoke(this, _EventArgs), token).ConfigureAwait(false);

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
                    _NKVMPlugin.UpdateMonitorInfo(_AllInfoMonitors, token);
                    //SupportedNKVMMonitors();
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
                        if (_peripheralslist.FindAll(o => o.ID.Equals(deviceInfo.ID)).Count == 1)
                        {
                            dockCount++;
                        }
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
            Debug.WriteLine($"show_display=>monitor:{e.monitor.modelName}=={e.vcpcode}:{e.value}");
            //1106 add PBP mode status
            Task.Run(() => updatePBPModeStatus(e.monitor, e.vcpcode)).ConfigureAwait(false);
            //Jason add USB change
            if (e.vcpcode.Equals("E7"))
            {
                if (_DisplayManagerPlugin != null)
                {
                    Dictionary<string, InputInfo> inputSourceList = GetInputSourcelist(e.monitor).Result;
                    foreach (var input in inputSourceList)
                    {
                        string usbUpstream = GetUSBUpstream(e.monitor, input.Key).Result;
                        //Update USBKVM
                        if (GetOnUSBKVM(e.monitor).Result)
                        {
                            List<DDPMMonitorSettings> settings = _SettingsPlugin.ReloadMonitorSettings(e.monitor.modelName).Result;
                            if (settings != null)
                            {
                                //get monitor setting
                                DDPMMonitorSettings monitorSetting = settings.Find(x => x.ServiceTag == e.monitor.edid.ServiceTag);
                                if (monitorSetting != null)
                                {
                                    try
                                    {
                                        if (!string.IsNullOrEmpty(monitorSetting.KVM.strUSBKVMPCsList))
                                        {
                                            Dictionary<string, PCsInfo> USBKVMPCsList = USBKVMPCsListDeserialize(monitorSetting.KVM.strUSBKVMPCsList);
                                            if (USBKVMPCsList != null)
                                            {
                                                if (USBKVMPCsList.Count != 0)
                                                {
                                                    foreach (var pc in USBKVMPCsList)
                                                    {
                                                        if (!string.IsNullOrEmpty(pc.Key) && pc.Value != null)
                                                        {
                                                            if (pc.Value.InputType == input.Key)
                                                            {
                                                                pc.Value.USBUpstream = usbUpstream;
                                                                bool b = SetUSBKVMPCsList(e.monitor, USBKVMPCsList).Result;
                                                                break;
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                writelog("[show_displays] USBKVMPCsList is null");
                                            }
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        ;
                                    }
                                }
                                else
                                {
                                    writelog("[show_displays] monitorSetting is null");
                                }
                            }
                            else
                            {
                                writelog("[show_displays] settings is null");
                            }
                        }
                    }
                }
            }
            OnVCPchanged(_VCPchangedEventArgs);
        }

        private void show_DDCCIchangedEventArgs(object sender, DDCCIchangedEventArgs e)
        {
            writelog("Receive DDCCIStatuschanged Event Notify from DisplayManagerPlugin");
            writelog("Send DDCCIStatuschanged Event Notify from DeviceMangerPlugin");

            var monitor = _AllInfoMonitors.Find(x => x.edid.Equals(e.monitors.edid));
            monitor = e.monitors.Clone();

            DDCCIchangedEventArgs _DDCCIchangedEventArgs = new DDCCIchangedEventArgs();
            _DDCCIchangedEventArgs.DDCisON = e.DDCisON;
            _DDCCIchangedEventArgs.monitors = e.monitors.Clone();
            OnDDCCIStatuschanged(_DDCCIchangedEventArgs);
        }

        private void show_displays_changed(object sender, DisplaychangedEventArgs e)
        {
            writelog("Receive Displaychanged Event Notify from DisplayManagerPlugin");
            writelog("Send out Displaychanged Event Notify from DeviceMangerPlugin");

            _AllInfoMonitors = new List<MonitorInfo>(e.monitors);

            DisplaychangedEventArgs _displaychangedEventArgs = new DisplaychangedEventArgs();
            _displaychangedEventArgs.count = e.count;
            _displaychangedEventArgs.monitors = e.monitors.ToList();
            OnDisplaychanged(_displaychangedEventArgs);

            Task.Run(() =>
            {
                //Telementry Collection
                var rt = false;
                var DeviceTypeConnected_Function = new DeviceTypeConnected_Function();
                writelog("[DeviceMangerPlugin] [Telementry] Send Telementry for DeviceTypeConnected_Function...");
                rt = DeviceTypeConnected_Function.DeviceTypeConnected_Telementry(_TelementryScheduler, e.monitors);
                if (rt)
                    writelog("[DeviceMangerPlugin] [Telementry] Send Telementry for DeviceTypeConnected_Function Success ...");
                else
                    writelog("[DeviceMangerPlugin] [Telementry] Send Telementry for DeviceTypeConnected_Function Fail ...");
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

        //Derek 1210
        public Task WriteLog(string logMsg)
        {
            writelog(logMsg);

            return Task.CompletedTask;
        }

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

            text = $"[DeviceManager] {text}, Caller Name:{memberName}, Source Line {sourceLineNumber}";
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

            _disDevHelper?.UpdateDDPMPluginInstances(_SettingsPlugin, this, _DisplayManagerPlugin);
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
                        (_InfoMonitors.edid.SerialNumber.Trim() == config.SerialNumber.Trim() || _InfoMonitors.edid.ServiceTag.Trim() == config.ServiceTag.Trim()))
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
                    //DownloadICCData(_InfoMonitors);

                    //Check if actived monitor has its color preset section in config file
                    if (_InfoMonitors.edid.ModelName.Trim().IndexOf(config.ModelName.Trim()) >= 0 &&
                         (_InfoMonitors.edid.SerialNumber.Trim() == config.SerialNumber.Trim() || _InfoMonitors.edid.ServiceTag.Trim() == config.ServiceTag.Trim()))
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
                            //ToNKVM_SupportedMonitorList();
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
                        //ToNKVM_SupportedMonitorList();
                        //ToNKVM_initHotKeys();
                    }
                    else if (pluginCondition is PluginStartedCondition)
                    {
                        writelog($"{nameof(GetCurrentNKVMPluginCondition)} - NKVM Plugin is in a started condition");
                        //_NKVMPluginCondition = pluginCondition;
                        _NKVMPlugin.NKVMCLIEvent += NKVMCLIEvent;
                        _NKVMPlugin.NKVMSetHotkey += NKVMSetHotkey;
                        //ToNKVM_SupportedMonitorList();
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
                        //_DTPProxyPlugin.ZoomChanged_Notify += ZoomChanged;
                        //_DTPProxyPlugin.ZoomMeetingTypeChanged_Notify += ZoomMeetingTypeChanged;
                        //_DTPProxyPlugin.IsZoomMeetingActive_Notify += IsZoomMeetingActiveChanged;
                        //_DTPProxyPlugin.IsZoomScreenShareActive_Notify += IsZoomScreenShareActiveChanged;

                        //Derek 1119
                        _DTPProxyPlugin.DTPEventHandler += _DTPProxyPlugin_DTPEventHandler;
                    }
                    else if (pluginCondition is PluginStartedCondition)
                    {
                        //Marded by Derek 1121
                        //_DTPProxyPlugin.Esi_IsCameraSensorCover_ChangeEvent += OnEsi_IsCameraSensorCoverChangeHandler;
                        //_DTPProxyPlugin.WALSnoozeTimeLeftInSeconds_ChangeEvent += OnWALSnoozeTimeLeftInSecondsChangeHandler;
                        //_DTPProxyPlugin.Esi_IsWALLockCountdownStartedChanged_ChangeEvent += OnEsi_IsWALLockCountdownStartedStatusChangeHandler;
                        //_DTPProxyPlugin.Esi_WALLockCountdownChanged_ChangeEvent += OnEsi_WALLockCountdownChangeHandler;

                        writelog($"{nameof(GetCurrentDTPProxyPluginCondition)} - DTPProxy Plugin is in a started condition");
                        //_DTPProxyPlugin.ZoomChanged_Notify += ZoomChanged;
                        //_DTPProxyPlugin.ZoomMeetingTypeChanged_Notify += ZoomMeetingTypeChanged;
                        //_DTPProxyPlugin.IsZoomMeetingActive_Notify += IsZoomMeetingActiveChanged;
                        //_DTPProxyPlugin.IsZoomScreenShareActive_Notify += IsZoomScreenShareActiveChanged;

                        //Derek 1119
                        _DTPProxyPlugin.DTPEventHandler += _DTPProxyPlugin_DTPEventHandler;
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
                            Task.Run(() => TelemetryDdpmSwUpdater()).ConfigureAwait(false);
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
                            Task.Run(() => TelemetryDdpmSwUpdater()).ConfigureAwait(false);
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
                _USBKVMAutoSwitchTimer.Stop();
                _USBKVMAutoSwitchTimer.Start();
            }
            else
            {
                find.HotkeyOptions = hotkeySettings.HotkeyOptions;
                HotkeyOption hotkeyOption = hotkeySettings.HotkeyOptions.ElementAtOrDefault(0);
                if (hotkeyOption.Equals(HotkeyOption.KvmAutoApply))
                {
                    _USBKVMAutoSwitchTimer.Stop();
                    _USBKVMAutoSwitchTimer.Start();
                }
                else
                {
                    _USBKVMAutoSwitchTimer.Stop();
                }
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
            //RegistHotkey
            RegistHotkey(true);
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
                if (rt)
                    writelog("[DeviceMangerPlugin] [Telementry] Send Telementry for HotkeyTelemetry Success ...");
                else
                    writelog("[DeviceMangerPlugin] [Telementry] Send Telementry for HotkeyTelemetry Fail ...");
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

        private bool isReg = false;

        private string _latestBatterylowContent = string.Empty;
        private OSDType_Device _lastestBatterylowDevice = OSDType_Device.Unknown;

        private void showBatteryLowCombineOSD(string deviceName, OSDType oSDType, bool state)
        {
            //close batterylow osd
            OSDType_Device getOSDType_Device = getLatestBatterylowOSDAndCloseOthers();
            try
            {
                System.Windows.Forms.Screen sreen = System.Windows.Forms.Screen.AllScreens.FirstOrDefault(x => x.DeviceName == Screen.PrimaryScreen.DeviceName);
                var dpiXProperty = typeof(SystemParameters).GetProperty("DpiX", BindingFlags.NonPublic | BindingFlags.Static);
                var varX = (int)dpiXProperty.GetValue(null, null);
                double dpiX = (double)varX / (double)96;

                switch (getOSDType_Device)
                {
                    case OSDType_Device.Unknown:
                        //single osd
                        switch (oSDType)
                        {
                            case OSDType.CapsLock:
                                ShowOSD(Screen.PrimaryScreen.DeviceName, OSDType.CapsLock, state);
                                break;

                            case OSDType.ScrollLock:
                                ShowOSD(Screen.PrimaryScreen.DeviceName, OSDType.ScrollLock, state);
                                break;

                            case OSDType.NumLock:
                                ShowOSD(Screen.PrimaryScreen.DeviceName, OSDType.NumLock, state);
                                break;
                        }
                        break;

                    case OSDType_Device.Keyboard:
                        _OSD_Controler.KeyAndKeybordBatteryLowWin_ShowWindow(_latestBatterylowContent, (sreen.WorkingArea.Top / (double)dpiX), (sreen.WorkingArea.Left / (double)dpiX), oSDType, OSDType_Device.Keyboard, state);
                        break;

                    case OSDType_Device.Mouse:
                        _OSD_Controler.KeyAndKeybordBatteryLowWin_ShowWindow(_latestBatterylowContent, (sreen.WorkingArea.Top / (double)dpiX), (sreen.WorkingArea.Left / (double)dpiX), oSDType, OSDType_Device.Mouse, state);
                        break;

                    case OSDType_Device.Headset:
                        _OSD_Controler.KeyAndKeybordBatteryLowWin_ShowWindow(_latestBatterylowContent, (sreen.WorkingArea.Top / (double)dpiX), (sreen.WorkingArea.Left / (double)dpiX), oSDType, OSDType_Device.Headset, state);
                        break;
                }
                _latestBatterylowContent = string.Empty;
                _lastestBatterylowDevice = OSDType_Device.Unknown;
            }
            catch (Exception ex)
            {
                writelog($"[showBatteryLowCombineOSD] ERROR - deviceName:{deviceName},OSDType:{oSDType},state:{state};Exception Message: {ex.Message}");
            }
        }

        private OSDType_Device getLatestBatterylowOSDAndCloseOthers()
        {
            OSDType_Device ret = OSDType_Device.Unknown;
            try
            {
                IntPtr hwnd_KeybordBatteryLow = CallUser32dll._FindWindow(null, "68C62D1D-CDA5-4EC6-AFB2-6DA8331D7DDE-KeybordBatteryLowIWin");
                if (hwnd_KeybordBatteryLow != IntPtr.Zero)
                {
                    if (_lastestBatterylowDevice == OSDType_Device.Keyboard)
                    {
                        ret = OSDType_Device.Keyboard;
                    }
                    _OSD_Controler.KeybordBatteryLow_CloseWindow();
                }
                IntPtr hwnd_MouseBatteryLow = CallUser32dll._FindWindow(null, "AFF4035F-B0CA-4B8C-991F-AAEBEB625EEF-MouseBatteryLowIWin");
                if (hwnd_MouseBatteryLow != IntPtr.Zero)
                {
                    if (_lastestBatterylowDevice == OSDType_Device.Mouse)
                    {
                        ret = OSDType_Device.Mouse;
                    }
                    _OSD_Controler.MouseBatteryLow_CloseWindow();
                }
                IntPtr hwnd_HeadsetBatteryLow = CallUser32dll._FindWindow(null, "5A9DDC40-D1A7-4DC4-9F59-CB95DCD13945-HeadsetBatteryLowIWin");
                if (hwnd_HeadsetBatteryLow != IntPtr.Zero)
                {
                    if (_lastestBatterylowDevice == OSDType_Device.Headset)
                    {
                        ret = OSDType_Device.Headset;
                    }
                    _OSD_Controler.HeadsetBatteryLow_CloseWindow();
                }
            }
            catch (Exception ex)
            {
                writelog($"[getLatestBatterylowOSDAndCloseOthers] ERROR - {ex.Message}");
            }
            return ret;
        }

        //Derek 1205 for Debug
        private void CreateWebcamEventForDebug_ShowUI()
        {
            _IsZoomMeetingActive = true;
            _IsZoomScreenShareActive = false;
            _ZoomMeetingType = ZoomMeetingType.CONF_3RD_EVENT_MEETING;

            HandleQAMV2();
            //_IsZoomMeetingActive = false;
            //_ZoomMeetingType = ZoomMeetingType.ZOOM_MEETING_TYPE_UNKNOW;
        }

        private void CreateWebcamEventForDebug_HideUI()
        {
            _IsZoomScreenShareActive = true;
            _IsZoomMeetingActive = true;

            HandleQAMV2();
        }

        //Derek 1217 add Debounce for Keyboard_KeyUpProc
        private System.Timers.Timer _timerDebounce;
        //即刻执行，执行之后，在timeMs内再次调用无效
        public void KeyboardHook_Debounce<T>(int timeMs, ISynchronizeInvoke invoker, 
                        Action<T> action, T parameter)
        {
            System.Threading.Monitor.Enter(this);
            bool needExit = true;

            try
            {
                if (_timerDebounce == null)
                {
                    _timerDebounce = new System.Timers.Timer(timeMs);
                    _timerDebounce.AutoReset = false;
                    _timerDebounce.Elapsed += (o, e) =>
                    {
                        _timerDebounce.Stop();
                        _timerDebounce.Close();
                        _timerDebounce = null;
                    };
                    _timerDebounce.Start();

                    System.Threading.Monitor.Exit(this);
                    needExit = false;

                    InvokeAction(action, parameter, invoker);//can't lock this
                }
            }
            catch (Exception e)
            {
                writelog($"Catch exception[{e.Message}] when run KeyboardHook_Debounce");
            }
            finally
            {
                if (needExit)
                    System.Threading.Monitor.Exit(this);
            }
        }

        private void InvokeAction<T>(Action<T> action, T parameter, ISynchronizeInvoke invoker)
        {
            if (invoker == null)
            {
                action(parameter);
            }
            else
            {
                if (invoker.InvokeRequired)
                {
                    _ = invoker.Invoke(action, new object[] { parameter });
                }
                else
                {
                    action(parameter);
                }
            }
        }

        private void Keyboard_KeyUpProc(object sender, KeyEventArgs e)
        {
            KeyboardHook_Debounce(3000, null, KeyboardHook_KeyUpProc, e);
        }

        private void KeyboardHook_KeyUpProc(KeyEventArgs e)
        {
            string strKey = e.KeyCode.ToString().ToUpper();
            Debug.WriteLine($"Keyboard_KeyUpProc ---{strKey}");
            bool _altPressed = _HotkeyPlugin.IsKeyPushedDown(System.Windows.Forms.Keys.Menu);
            bool _ctrlPressed = _HotkeyPlugin.IsKeyPushedDown(System.Windows.Forms.Keys.ControlKey);
            bool _shiftPressed = _HotkeyPlugin.IsKeyPushedDown(System.Windows.Forms.Keys.ShiftKey);

            //will register as ALT+Z ?
            if (_altPressed && strKey.Equals("Z"))
            {
                int devCnt = GetWebcamDeviceCount();

                writelog($"ALT+Z conditons: devcnt = {devCnt}, " +
                    $"global setting is {_GlobalSettingParam.GlobalSetting_WidgetSettings.EnableQuickAccessWidget}");

                //Derek PIMS-329759 Problem 1
                if (1 == devCnt && _GlobalSettingParam != null &&
                    _GlobalSettingParam.GlobalSetting_WidgetSettings != null &&
                    _GlobalSettingParam.GlobalSetting_WidgetSettings.EnableQuickAccessWidget)
                {
                    CallQAM_UI(this);
                }

                return;
            }
            //Derek 1205 for Debug
            //else if (_altPressed && strKey.Equals("A"))
            //{
            //    CreateWebcamEventForDebug_ShowUI();

            //    return;
            //}
            //else if (_altPressed && strKey.Equals("H"))
            //{
            //    CreateWebcamEventForDebug_HideUI();

            //    return;
            //}

            //osd
            GlobalSettingParam result = GetGlobalSettingParam().Result;
            if (result != null)
            {
                Debug.WriteLine($"GlobalSettingParam.GlobalSetting_General.Keyboard_Lock_Key={result.GlobalSetting_General.Keyboard_Lock_Key}");
                if (result.GlobalSetting_General.Keyboard_Lock_Key)
                {
                    //test
                    /* if (!isReg)
                     {
                         ShowOSD(Screen.PrimaryScreen.DeviceName, OSDType.BatteryLow, OSDType_Device.Keyboard, "Dell Multi-Device Mouse - MS5320W");
                         isReg = true;
                     }*/
                    //showBatteryLowCombineOSD(_latestBatterylowContent);
                    //test end

                    if (e.KeyCode == Keys.CapsLock)
                    {
                        bool isCapsLockOn = (System.Windows.Input.Keyboard.GetKeyStates(System.Windows.Input.Key.CapsLock) & System.Windows.Input.KeyStates.Toggled) == System.Windows.Input.KeyStates.Toggled;
                        Debug.WriteLine($"Key.CapsLock={isCapsLockOn}");
                        if (isCapsLockOn)
                        {
                            showBatteryLowCombineOSD(Screen.PrimaryScreen.DeviceName, OSDType.CapsLock, true);
                            // ShowOSD(Screen.PrimaryScreen.DeviceName, OSDType.CapsLock, true);
                            //ShowOSD(Screen.PrimaryScreen.DeviceName, OSDType.CollaborationNotAvailable, OSDType_Device.Keyboard, "Collaboration controls are not available during multiple conference calls");
                        }
                        else
                        {
                            showBatteryLowCombineOSD(Screen.PrimaryScreen.DeviceName, OSDType.CapsLock, false);
                            //ShowOSD(Screen.PrimaryScreen.DeviceName, OSDType.CapsLock, false);
                        }
                        //_OSDKeyLock = true;
                        //e.Handled = true;
                    }
                    if (e.KeyCode == Keys.Scroll)
                    {
                        bool isScrollLockOn = (System.Windows.Input.Keyboard.GetKeyStates(System.Windows.Input.Key.Scroll) & System.Windows.Input.KeyStates.Toggled) == System.Windows.Input.KeyStates.Toggled;
                        Debug.WriteLine($"Key.Scroll={isScrollLockOn}");
                        if (isScrollLockOn)
                        {
                            showBatteryLowCombineOSD(Screen.PrimaryScreen.DeviceName, OSDType.ScrollLock, true);
                            //ShowOSD(Screen.PrimaryScreen.DeviceName, OSDType.ScrollLock, true);
                        }
                        else
                        {
                            showBatteryLowCombineOSD(Screen.PrimaryScreen.DeviceName, OSDType.ScrollLock, false);
                            //ShowOSD(Screen.PrimaryScreen.DeviceName, OSDType.ScrollLock, false);
                        }
                        //_OSDKeyLock = true;
                        //e.Handled = true;
                    }
                    if (e.KeyCode == Keys.NumLock)
                    {
                        bool isNumLockLockOn = (System.Windows.Input.Keyboard.GetKeyStates(System.Windows.Input.Key.NumLock) & System.Windows.Input.KeyStates.Toggled) == System.Windows.Input.KeyStates.Toggled;
                        Debug.WriteLine($"Key.NumLock={isNumLockLockOn}");
                        if (isNumLockLockOn)
                        {
                            showBatteryLowCombineOSD(Screen.PrimaryScreen.DeviceName, OSDType.NumLock, true);
                            //ShowOSD(Screen.PrimaryScreen.DeviceName, OSDType.NumLock, true);
                        }
                        else
                        {
                            showBatteryLowCombineOSD(Screen.PrimaryScreen.DeviceName, OSDType.NumLock, false);
                            //ShowOSD(Screen.PrimaryScreen.DeviceName, OSDType.NumLock, false);
                        }
                        //ShowOSD(Screen.PrimaryScreen.DeviceName, OSDType.NumLock, true);
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
            /*            if (_hotkeySettings != null && _hotkeySettings.Count == 0)
                        {
                            _hotkeySettings = _SettingsPlugin.ReadHotkeySettings().Result;
                        }
                        if (_hotkeySettings != null && _hotkeySettings.Count > 0)
                        {
                            foreach (var settings in _hotkeySettings)
                            {
                                foreach (var hotkeyInfo in settings.HotkeyInfo)
                                {
                                    if (!isReg)
                                    {
                                        var key = new HotKey(
                                              hotkeyInfo.ModifiersEnum,
                                              hotkeyInfo.KeyCode,
                                              _HotkeyPlugin.GetHookHandle(),
                                              (hotkey) =>
                                              {
                                                  Debug.WriteLine("hotkey was pressed======================================!");
                                              });
                                        isReg = true;
                                    }

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
                                        writelog($"Job matched:{hotkeyInfo.Job} => {hotkeyInfo.Description}: Hotkey => : {string.Join("+", hotkeyInfo.Hotkey.Select(x => x + "(" + (int)x + ")").ToList())}");
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
                        }*/
        }

        public Task SetLastSelectedMonitorFromUI(MonitorInfo mo)
        {
            lastSelectedMonitor_UI = mo;
            Task.Run(() =>
            {
                if (mo != null && _SettingsPlugin != null)
                {
                    DDPMSettings settings = _SettingsPlugin.ReloadAppConfigData().Result;
                    if (settings != null && settings.UserSettings != null)
                    {
                        settings.UserSettings.lastUISelectedMonitor = new DDPMSimpleMonitorRecord() { ModelName = mo.modelName, ServiceTag = mo.edid.ServiceTag };
                        _SettingsPlugin.SetAppConfigData(settings);

                        //Robert_Lin, 2024-12,4, Notify to EAPlugin
                        if (_DisplayManagerPlugin != null)
                        {
                            EAArgs eaArgs = new EAArgs()
                            {
                                Command = EAEMConstants.EACommand_LastSelectedMonitorChanged
                            };
                            _DisplayManagerPlugin.NotifyEAMessage(eaArgs);
                        }
                    }
                }
            });
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
            HotkeyInfo hotkeyInfoTmp = settings.HotkeyInfo.SingleOrDefault(x => x.Job == job);
            string hotkeyStr = string.Empty;
            if (hotkeyInfoTmp != null)
            {
                hotkeyStr = string.Join("+", hotkeyInfoTmp.Hotkey.Select(x => x + "(" + (int)x + ")").ToList());
            }
            writelog($"ExecHotkeyJob[{job}:{hotkeyStr}] mouse cursor on Monitor [ModelName={monitorInfo?.edid.ModelName},ServiceTag={monitorInfo?.edid.ServiceTag}, SerialNumber={monitorInfo?.edid.SerialNumber}]");

            bool getTargetMo = false;
            if (monitorInfo == null)
            {
                writelog($"[ExecHotkeyJob:{job}:{hotkeyStr}] null dell monitor get over mouse: locate at Screen({currentScreen.DeviceName})");
                //check (1)
                if (lastSelectedMonitor_UI == null)
                {
                    Debug.WriteLine($"[ExecHotkeyJob:{job}:{hotkeyStr}] UI didn't set any selected monitor");
                    writelog($"[ExecHotkeyJob:{job}:{hotkeyStr}] UI didn't set any selected monitor");
                    return Task.FromResult(false);
                }
                monitorInfo = _AllInfoMonitors.Find(x => x.modelName.Equals(lastSelectedMonitor_UI.modelName) && x.edid.ServiceTag.Equals(lastSelectedMonitor_UI.edid.ServiceTag));
                if (monitorInfo == null)
                {
                    writelog($"[ExecHotkeyJob:{job}:{hotkeyStr}] Selected monitor ({lastSelectedMonitor_UI.modelName}) from UI do not exist in current monitor list");
                    Debug.WriteLine($"[ExecHotkeyJob:{job}:{hotkeyStr}] Selected monitor ({lastSelectedMonitor_UI.modelName}) from UI do not exist in current monitor list");
                    return Task.FromResult(false);
                }
            }
            else
            {
                getTargetMo = true;
            }
            if (getTargetMo)
            {
                Debug.WriteLine($"ExecHotkeyJob[{job}:{hotkeyStr}] => TargetMonitor(from Mouse crusor), Monitor [ModelName={monitorInfo.edid.ModelName},ServiceTag={monitorInfo.edid.ServiceTag}, SerialNumber={monitorInfo.edid.SerialNumber}]");
            }
            else
            {
                Debug.WriteLine($"ExecHotkeyJob[{job}:{hotkeyStr}] => TargetMonitor(from UI seleted), Monitor [ModelName={monitorInfo.edid.ModelName},ServiceTag={monitorInfo.edid.ServiceTag}, SerialNumber={monitorInfo.edid.SerialNumber}]");
            }
            writelog($"ExecHotkeyJob[befrore:{job}:{hotkeyStr}] => getTargetMonitor: {getTargetMo}, Monitor [ModelName={monitorInfo.edid.ModelName},ServiceTag={monitorInfo.edid.ServiceTag}, SerialNumber={monitorInfo.edid.SerialNumber}]");
            switch (job)
            {
                case HotkeyType.BrightnessReduce:
                    /* HotkeyPopWrap hotkeyPopWrap1 = new HotkeyPopWrap() { monitorInfo = monitorInfo, hotkeyType = job };
                     HotkeyPopup(hotkeyPopWrap1);
                     break;*/
                    //ALS stand for Ambient Light Sensor, include 3 fearture [Auto brightness][Auto color Temperature][Primary monitor for Sync]
                    //DDPMW-764
                    if (IsALSautobrightness(monitorInfo))
                    {
                        writelog($"ExecHotkeyJob[{job}:{hotkeyStr} ,IsALSautobrightness=true,will Popup msg] => getTargetMonitor: {getTargetMo}, Monitor [ModelName={monitorInfo.edid.ModelName},ServiceTag={monitorInfo.edid.ServiceTag}, SerialNumber={monitorInfo.edid.SerialNumber}]");
                        HotkeyPopWrap hotkeyPopWrap = new HotkeyPopWrap() { monitorInfo = monitorInfo, hotkeyType = job };
                        HotkeyPopup(hotkeyPopWrap);
                    }
                    else
                    {
                        _hotkeyJobQueue.Enqueue(new JobInfo(1000, monitorInfo, null, Reduce_Brightness_Value));
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
                        _hotkeyJobQueue.Enqueue(new JobInfo(1000, monitorInfo, null, Increase_Brightness_Value));
                    }
                    break;

                case HotkeyType.ContrastReduce:
                    //DDPMW-764
                    _hotkeyJobQueue.Enqueue(new JobInfo(1000, monitorInfo, null, Reduce_Contrast_Value));
                    break;

                case HotkeyType.ContrastIncrease:
                    //DDPMW-764
                    _hotkeyJobQueue.Enqueue(new JobInfo(1000, monitorInfo, null, Increase_Contrast_Value));
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

        private void Toggle_EzRecentSetting(MonitorInfo monitorInfo, Object[] param)
        {
            //Validation
            //todo Toggle_EzRecentSetting
            //Read the EAMonitorSettings
            EAMonitorSettings eaSettings = ReadEAMonitorSettings(monitorInfo).Result;
            //Change selected layout to the latest item of RecentList
            if (eaSettings != null)
            {
                SplitJson[] recentList = eaSettings.RecentList;

                if (recentList == null)
                {
                    writelog("@ Toggle_EzRecentSetting(), EA RecentList is null");
                    return;
                }
                if (recentList.Length == 0)
                {
                    writelog("@ Toggle_EzRecentSetting(), EA RecentList is empty");
                    return;
                }
                else
                {
                    List<SplitJson> splitJsonsList = recentList.ToList();
                    List<SplitJson> splitJsonsTmp = new List<SplitJson>();
                    splitJsonsTmp.AddRange(splitJsonsList);
                    SplitJson LastSplitJson = splitJsonsTmp.ElementAt(splitJsonsList.Count - 1);
                    splitJsonsTmp.RemoveAt(splitJsonsList.Count - 1);
                    splitJsonsTmp.Insert(0, LastSplitJson);
                    splitJsonsList.Clear();
                    splitJsonsList.AddRange(splitJsonsTmp);
                    //save recentlist
                    eaSettings.RecentList = splitJsonsList.ToArray();
                    eaSettings.SelectedSplit = splitJsonsList.ElementAt(0);
                    bool result = WriteEAMonitorSettings(monitorInfo, eaSettings).Result;
                    if (result)
                    {
                        //Change selected layout to the latest item of RecentList
                        bool changed = NotifyEASelectedLayoutChanged(monitorInfo, splitJsonsList.ElementAt(0)).Result;
                        //notice UI
                        EAArgs eAArgs = new EAArgs();
                        eAArgs.Message = @"updateRecentSelected";
                        EASettingsChanged(this, eAArgs);
                        writelog($"Toggle_EzRecentSetting(),monitor:{monitorInfo.AliasDeviceName}={monitorInfo.edid.ServiceTag}, EA RecentList.Count={eaSettings.RecentList.Length}, toggle to [{splitJsonsList.ElementAt(0).CellCount},{splitJsonsList.ElementAt(0).SplitKey}],save eaSettings.RecentList success");
                    }
                    else
                    {
                        writelog($"Toggle_EzRecentSetting(),monitor:{monitorInfo.AliasDeviceName}={monitorInfo.edid.ServiceTag}, EA RecentList.Count={eaSettings.RecentList.Length}, toggle to [{splitJsonsList.ElementAt(0).CellCount},{splitJsonsList.ElementAt(0).SplitKey}],save eaSettings.RecentList fail, do nothing");
                    }
                }
            }

            //Force await to avoid reenter this method (it will update to MonitorSettings file)
            //bool isOKSetSelected = SetEASelectedLayout(monitorInfo, eaSettings.RecentList[idxRecent]).Result;

            //TO DO: invoke an event to UI to reload settings
            // TO be implement in EASettingsChanged event

            //writelog($"@ Toggle_EzRecentSetting(), result is {isOKSetSelected}");
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
            if (gamingDisplayProperties != null && gamingDisplayProperties.IsSupported_DualResolutionType)
            {
                gamingDisplayProperties.Current_DualResolutionType = GetCurrentGaming_DualResolutionType(monitorInfo).Result;
                Debug.WriteLine($"Gaming_DualResolutionToggle:monitor[{monitorInfo.edid.ModelName}({monitorInfo.edid.SerialNumber})]Current_DualResolutionType={gamingDisplayProperties.Current_DualResolutionType}");
                writelog($"Gaming_DualResolutionToggle:monitor[{monitorInfo.edid.ModelName}({monitorInfo.edid.SerialNumber})]Current_DualResolutionType={gamingDisplayProperties.Current_DualResolutionType}");
                List<Gaming_DualResolutionType> supported_DualResolutionType = gamingDisplayProperties.Supported_DualResolutionType;
                if (supported_DualResolutionType != null && supported_DualResolutionType.Count > 0 && gamingDisplayProperties.Current_DualResolutionType != null)
                {
                    Gaming_DualResolutionType current_DualResolutionType = (Gaming_DualResolutionType)gamingDisplayProperties.Current_DualResolutionType;
                    Gaming_DualResolutionType nextDualResolutionType = Gaming_DualResolutionType.Unknow;
                    if (Gaming_DualResolutionType.Unknow == current_DualResolutionType)
                    {
                        nextDualResolutionType = supported_DualResolutionType.ElementAtOrDefault(0);
                    }
                    else
                    {
                        for (int i = 0; i < supported_DualResolutionType.Count; i++)
                        {
                            Debug.WriteLine($"Gaming_DualResolutionToggle:monitor[{monitorInfo.edid.ModelName}({monitorInfo.edid.SerialNumber})]supported_DualResolutionType={supported_DualResolutionType[i]}");
                            writelog($"Gaming_DualResolutionToggle:monitor[{monitorInfo.edid.ModelName}({monitorInfo.edid.SerialNumber})]supported_DualResolutionType={supported_DualResolutionType[i]}");
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
            if (!GetOnUSBKVM(monitorInfo).Result)
            {
                writelog($"Kvm_SwitchInputSource:[{monitorInfo.edid.ModelName}:{monitorInfo.edid.SerialNumber}] USB KVM is off, do nothing");
                return;
            }
            Debug.WriteLine($"Kvm_SwitchInputSource:current inputsource= {monitorInfo.inputSource}");
            writelog($"Kvm_SwitchInputSource:current inputsource= {monitorInfo.inputSource}");
            HotkeyInfo hotkey = (HotkeyInfo)param.ElementAtOrDefault(0);
            List<InputSourceObj> list = (List<InputSourceObj>)param.ElementAtOrDefault(1);// GetInputSourceHotKeyData(monitorInfo);
            string crtInput = monitorInfo.inputSource;
            if (list == null || list.Count == 0)//hotkey.InputSource.Count == 0)
            {
                //hotkey.InputSource Count must not 0
                //update inputsources to current inputsoure and subinput
                List<InputSourceObj> subInputs = GetSubInputs(monitorInfo).Result;
                foreach (var item in subInputs)
                {
                    Debug.WriteLine($"Kvm_SwitchInputSource >subInputs: {item.Name}+{item.Code}");
                }
                List<InputSourceObj> defaultList = new List<InputSourceObj>();
                defaultList.Add(new InputSourceObj(crtInput));
                defaultList.AddRange(subInputs);
                if (subInputs == null || subInputs.Count == 0)
                {
                    writelog($"Kvm_SwitchInputSource:[{monitorInfo.edid.ModelName}:{monitorInfo.edid.SerialNumber}], USBKVM_switch_PCs inputsource is null and subInputs is empty, do nothing");
                    return;
                }
                else
                {
                    list.AddRange(defaultList);
                    writelog($"Kvm_SwitchInputSource:[{monitorInfo.edid.ModelName}:{monitorInfo.edid.SerialNumber}], USBKVM_switch_PCs inputsource is null, use [default] current inputsoure and subinputs");
                }
                if (hotkey != null)
                    GetInputSourceHotKeyDataAndSaveNewBack(monitorInfo, hotkey.Job, defaultList);
            }
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
            else
            {
                writelog($"Kvm_SwitchInputSource:[{monitorInfo.edid.ModelName}:{monitorInfo.edid.SerialNumber}], USBKVM_switch_PCs fail,cause [nextInput] is IsNullOrEmpty [do nothing]");
            }
        }

        private void Kvm_SwitchKbMsKey(MonitorInfo monitorInfo, Object[] param)
        {
            if (!GetOnUSBKVM(monitorInfo).Result)
            {
                writelog($"Kvm_SwitchKbMsKey:[{monitorInfo.edid.ModelName}:{monitorInfo.edid.SerialNumber}] USB KVM is off, do nothing");
                return;
            }
            bool usbSwitch = UsbSwitch1(monitorInfo).Result;
            writelog($"Kvm_SwitchKbMsKey:[{monitorInfo.edid.ModelName}:{monitorInfo.edid.SerialNumber}]" + (usbSwitch ? "success" : "fail"));
        }

        private void Kvm_ChangePIPPosition(MonitorInfo monitorInfo, Object[] param)
        {
            if (!GetOnUSBKVM(monitorInfo).Result)
            {
                writelog($"Kvm_ChangePIPPosition:[{monitorInfo.edid.ModelName}:{monitorInfo.edid.SerialNumber}] USB KVM is off, do nothing");
                return;
            }
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
            if (!mo.CapabilityDic.ContainsKey("E9"))
                return false;
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
                /* List<int> swapList = subInputs.Select(tmp => allInputs.IndexOf(allInputs.FirstOrDefault(x => x.Code.Equals(tmp.Code)))).ToList();
                 if (swapList.Count != 1 && swapList.Any(x => x.Equals(-1)))
                 {
                     return;
                 }*/
                // Trace.WriteLine($"Calling to VideoSwap(0,{swapList[0]})");
                //bool swapPxp = VideoSwap(monitorInfo, (UInt16)0, (UInt16)swapList[0]).Result;
                //SplitCountFromPxpMode==2 alway is this
                bool swapPxp = VideoSwap(monitorInfo, (UInt16)0, (UInt16)1).Result;
                writelog($"Swap_IputPIPPBP:[{monitorInfo.edid.ModelName}:{monitorInfo.edid.SerialNumber}](keys:{log_keys}) from [0] to [1]" + (swapPxp ? "success" : "fail"));
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
                string log_keys = string.Empty;
                if (param != null && param.Count() > 0)
                {
                    HotkeyInfo hotkey = (HotkeyInfo)param[0];
                    log_keys = string.Join("+", hotkey.Hotkey.Select(x => x + "(" + (int)x + ")").ToList());
                }
                List<InputSourceObj> list = (List<InputSourceObj>)param.ElementAtOrDefault(1);// GetInputSourceHotKeyData(monitorInfo);
                Debug.WriteLine($"Switch_InputSource [{monitorInfo.edid.ServiceTag}]");
                if (list == null || list.Count != 2)//hotkey.InputSource.Count == 0)
                {
                    //hotkey.InputSource Count must 2
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
                    if (inputSourceObjs == null || inputSourceObjs.Count() == 0)
                    {
                        //hotkey.InputSource Count must not 0.
                        Debug.WriteLine($"Switch_InputSource convert InputSource count is 0");
                        writelog($"Switch_InputSource[{log_keys}]:[{monitorInfo.edid.ModelName}:{monitorInfo.edid.SerialNumber}],migration hotkeyInfo:inputsoure is empty or can't convert ");
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
                    writelog($"Switch_InputSource[{log_keys}]:[{monitorInfo.edid.ModelName}:{monitorInfo.edid.SerialNumber}] from [{crtInput}] to [{switchTo.Name}]" + (setInput ? "success" : "fail"));
                }
            }
        }

        private void Favorite_InputSource(MonitorInfo monitorInfo, Object[] param)
        {
            if (!IsHotkeyFuncLock(HotkeyType.LockActiveInputSource))
            {
                string log_keys = string.Empty;
                if (param != null && param.Count() > 0)
                {
                    HotkeyInfo hotkey = (HotkeyInfo)param[0];
                    log_keys = string.Join("+", hotkey.Hotkey.Select(x => x + "(" + (int)x + ")").ToList());
                }
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
                writelog($"Favorite_InputSource[{log_keys}]:[{monitorInfo.edid.ModelName}:{monitorInfo.edid.SerialNumber}] to [{changeInput}]" + (setNextInput ? "success" : "fail"));
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
                List<InputInfo> inputInfos = result.Select(x => x.Value).ToList();
                Debug.WriteLine($"Toggle_InputSource,all inputsourc:[{monitorInfo.edid.ModelName}:{monitorInfo.edid.SerialNumber}] {string.Join("+", inputInfos.Select(x => x.InputName + "(" + x.Code + ")").ToList())}");
                writelog($"Toggle_InputSource,all inputsourc:[{monitorInfo.edid.ModelName}:{monitorInfo.edid.SerialNumber}] {string.Join("+", inputInfos.Select(x => x.InputName + "(" + x.Code + ")").ToList())}");
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

        private bool IsALSautobrightness(MonitorInfo monitorInfo)
        {
            List<ALSConfig> aLSConfigs = GetAllExistAlsConfig().Result;
            if (aLSConfigs != null)
            {
                ALSConfig find = aLSConfigs.FirstOrDefault(x => x.Edid.ServiceTag.Equals(monitorInfo.edid.ServiceTag) && x.isAutoBrightness);
                return find != null;
            }
            return false;
        }

        private void HotkeyPopup(object o)
        {
            Task.Run(() =>
            {
                PopupBaseManage popupBaseManage = new PopupBaseManage();
                popupBaseManage.LeftButtonClick += YesEvent;
                popupBaseManage.RightButtonClick += NoEvent;
                string title = LangHelper.Instance["Warning"];
                string info = LangHelper.Instance["Auto_Brightness_is_currently_enabled"];
                popupBaseManage.FWU_Show(title, info, LangHelper.Instance["Yes"], LangHelper.Instance["No"], o, true, -1);
            });
        }

        private void YesEvent(object o, object ob)
        {
            //Auto Brightness OFF & Auto OFF & Manual ON?
            HotkeyPopWrap hotkeyPopWrap = (HotkeyPopWrap)ob;
            List<ALSConfig> aLSConfigs = GetAllExistAlsConfig().Result;
            bool setALSFeature = false;
            if (aLSConfigs != null)
            {
                ALSConfig find = aLSConfigs.FirstOrDefault(x => x.Edid.ServiceTag.Equals(hotkeyPopWrap.monitorInfo.edid.ServiceTag) && x.isAutoBrightness);
                //diable autobrightness
                if (find != null)
                {
                    //setALSFeature = SetALSFeatureValue(hotkeyPopWrap.monitorInfo, find, ALSFeatureQueryType.AutoBrightness, "OFF").Result;
                    find.isAutoBrightness = false;
                    setALSFeature = SetALSFeatureValue(hotkeyPopWrap.monitorInfo, find, ALSFeatureQueryType.All, "").Result;
                    writelog($"IsALSautobrightness Yes_event[{hotkeyPopWrap.hotkeyType}:Monitor [ModelName={hotkeyPopWrap.monitorInfo.edid.ModelName},ServiceTag={hotkeyPopWrap.monitorInfo.edid.ServiceTag}],set ALS.AutoBrightness to off:" + (setALSFeature ? "success" : "fail"));
                }
                else
                {
                    writelog($"IsALSautobrightness Yes_event[{hotkeyPopWrap.hotkeyType}:Monitor [ModelName={hotkeyPopWrap.monitorInfo.edid.ModelName},ServiceTag={hotkeyPopWrap.monitorInfo.edid.ServiceTag}],ALS config not found");
                }
            }
            if (setALSFeature)
            {
                Task.Run(() =>
                  {
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
                  });
            }
            else
            {
                Debug.WriteLine($"IsALSautobrightness Yes_event[{hotkeyPopWrap.hotkeyType}:Monitor [ModelName={hotkeyPopWrap.monitorInfo.edid.ModelName},ServiceTag={hotkeyPopWrap.monitorInfo.edid.ServiceTag}],set ALS.AutoBrightness to off fail,skip this hotkey action");
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
                _disDevHelper.PerformHotKeyBrightnessContrastLuminanceAction(HotkeyType.BrightnessReduce, _AllInfoMonitors, monitorInfo, GetAllExistAlsConfig().Result);
            }
        }

        private void Increase_Brightness_Value(MonitorInfo monitorInfo, Object[] param)
        {
            if (!IsHotkeyFuncLock(HotkeyType.LockBriCont))
            {
                _disDevHelper.PerformHotKeyBrightnessContrastLuminanceAction(HotkeyType.BrightnessIncrease, _AllInfoMonitors, monitorInfo, GetAllExistAlsConfig().Result);
            }
        }

        private void Reduce_Contrast_Value(MonitorInfo monitorInfo, Object[] param)
        {
            if (!IsHotkeyFuncLock(HotkeyType.LockBriCont))
            {
                _disDevHelper.PerformHotKeyBrightnessContrastLuminanceAction(HotkeyType.ContrastReduce, _AllInfoMonitors, monitorInfo, GetAllExistAlsConfig().Result);
            }
        }

        private void Increase_Contrast_Value(MonitorInfo monitorInfo, Object[] param)
        {
            if (!IsHotkeyFuncLock(HotkeyType.LockBriCont))
            {
                _disDevHelper.PerformHotKeyBrightnessContrastLuminanceAction(HotkeyType.ContrastIncrease, _AllInfoMonitors, monitorInfo, GetAllExistAlsConfig().Result);
            }
        }

        private void Reduce_Luminance_Value(MonitorInfo monitorInfo, Object[] param)
        {
            if (!IsHotkeyFuncLock(HotkeyType.LockBriCont))
            {
                _disDevHelper.PerformHotKeyBrightnessContrastLuminanceAction(HotkeyType.LuminanceReduce, _AllInfoMonitors, monitorInfo, GetAllExistAlsConfig().Result);
            }
        }

        private void Increase_Luminance_Value(MonitorInfo monitorInfo, Object[] param)
        {
            if (!IsHotkeyFuncLock(HotkeyType.LockBriCont))
            {
                _disDevHelper.PerformHotKeyBrightnessContrastLuminanceAction(HotkeyType.LuminanceIncrease, _AllInfoMonitors, monitorInfo, GetAllExistAlsConfig().Result);
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
            Debug.WriteLine($"{powerNapSetting.ModelName}:{powerNapSetting.SerialNumber}:{powerNapSetting.ServiceTag}:{powerNapSetting.Status}:{powerNapSetting.RunType}");
            List<PowerNapSetting> saveList = new List<PowerNapSetting>();
            bool ret = false;
            List<DDPMMonitorSettings> monitorSettings = _SettingsPlugin.ReloadMonitorSettings(powerNapSetting.ModelName).Result;
            if (monitorSettings != null)
            {
                DDPMMonitorSettings updateSettings = monitorSettings.FirstOrDefault(x => x.ServiceTag.Equals(powerNapSetting.ServiceTag));
                if (updateSettings != null)
                {
                    updateSettings.PowerNap = powerNapSetting;
                    ret = _SettingsPlugin.WriteMonitorSettings(powerNapSetting.ModelName, monitorSettings).Result;
                }
            }
            else
            {
                writelog($"@ SavePowerNapSetting: ReloadMonitorSettings(model={powerNapSetting.ModelName}) return null.");
                ret = false;
            }

            /*saveList.Add(powerNapSetting);
            foreach (PowerNapSetting setting in allSettings)
            {
                if (saveList.Any(x => x.SerialNumber.Equals(setting.SerialNumber)))
                    continue;
                saveList.Add(setting);
            }
            WritePowerNapSettings(saveList);*/
            if (powerNapSetting.RunType == PowerNapType.Off)
            {
                //only save btn status
                ret = true;
            }
            else
            {
                //reset powerNaptimer and status
                _PowerNapTimer.Stop();
                _powerNapJobQueue.Clear();
                _screenSaver = false;
                _PowerNapTimer.Start();
            }
            //Telementry Collection
            var rt = false;
            var Displaysettings_Function = new Displaysettings_Function();
            MonitorInfo monitorInfo = _AllInfoMonitors.FirstOrDefault(x => x.edid.ServiceTag.Equals(powerNapSetting.ServiceTag));
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
                    if (rt)
                        writelog("[DeviceMangerPlugin] [Telementry] Send Telementry for PowerNap Success ...");
                    else
                        writelog("[DeviceMangerPlugin] [Telementry] Send Telementry for PowerNap Fail ...");
                }
            }
            return Task.FromResult(ret);
        }

        public Task<List<PowerNapSetting>> ReadPowerNapSettings()
        {
            List<PowerNapSetting> allSettings = _SettingsPlugin.ReadPowerNapSettings().Result;
            List<PowerNapSetting> saveList = new List<PowerNapSetting>();
            //update old place powerNap settings
            if (allSettings != null && allSettings.Count > 0)
            {
                foreach (PowerNapSetting powerNap in allSettings)
                {
                    List<DDPMMonitorSettings> monitorSettings = _SettingsPlugin.ReloadMonitorSettings(powerNap.ModelName).Result;
                    if (monitorSettings != null)
                    {
                        MonitorInfo monitorInfo1 = _AllInfoMonitors.FirstOrDefault(x => x.edid.SerialNumber.Equals(powerNap.SerialNumber));
                        if (monitorInfo1 != null)
                        {
                            DDPMMonitorSettings updateSettings = monitorSettings.FirstOrDefault(x => x.ServiceTag.Equals(monitorInfo1.edid.ServiceTag));
                            if (updateSettings != null)
                            {
                                updateSettings.PowerNap = powerNap;
                                bool s = _SettingsPlugin.WriteMonitorSettings(powerNap.ModelName, monitorSettings).Result;
                            }
                        }
                    }
                    else
                    {
                        saveList.Add(powerNap);
                    }
                }
                WritePowerNapSettings(saveList);
            }
            //return all
            List<PowerNapSetting> retList = new List<PowerNapSetting>();
            foreach (var mo in _AllInfoMonitors)
            {
                List<DDPMMonitorSettings> result = _SettingsPlugin.ReloadMonitorSettings(mo.modelName).Result;
                foreach (var item in result)
                {
                    if (item.PowerNap != null)
                    {
                        retList.Add(item.PowerNap);
                    }
                }
            }
            return Task.FromResult(retList);
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

                    List<MonitorInfo> _HadleMonitors = _AllInfoMonitors.ToList();
                    int nCount = _HadleMonitors.Count;

                    for (int n = 0; n < nCount; n++)
                    {
                        writelog($"[Original] Monitor: {_HadleMonitors[n].DisplayName}, ST: {_HadleMonitors[n].edid.ServiceTag}");
                    }

                    List<MonitorInfo> distinctMonitor = RemoveDuplicatesByDisplayName(_HadleMonitors);
                    nCount = distinctMonitor.Count;
                    for (int n = 0; n < nCount; n++)
                    {
                        writelog($"[Reviewed] Monitor: {distinctMonitor[n].DisplayName}, ST: {distinctMonitor[n].edid.ServiceTag}");
                    }

                    _AllInfoMonitors = new List<MonitorInfo>(distinctMonitor);
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
                    //_NKVMPlugin.ToNKVM_SupportedMonitorList(_SupportedMonitorList);
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
                            settings.ALSConfig = 0;

                            // scheduleInfo PIMS-302114
                            {
                                if (m.modelName.Equals("UP2720Q", StringComparison.OrdinalIgnoreCase))
                                {
                                    settings.scheduleInfo = new scheduleInfo()
                                    {
                                        model = m.modelName,
                                        serviceTag = m.edid.ServiceTag,
                                        Brightness1 = 150,
                                        Brightness2 = 150,
                                    };
                                }
                                else if (m.modelName.Equals("UP2720QA", StringComparison.OrdinalIgnoreCase))
                                {
                                    settings.scheduleInfo = new scheduleInfo()
                                    {
                                        model = m.modelName,
                                        serviceTag = m.edid.ServiceTag,
                                        Brightness1 = 150,
                                        Brightness2 = 150,
                                    };
                                }
                                else if (m.modelName.Equals("UP3221Q", StringComparison.OrdinalIgnoreCase))
                                {
                                    settings.scheduleInfo = new scheduleInfo()
                                    {
                                        model = m.modelName,
                                        serviceTag = m.edid.ServiceTag,
                                        Brightness1 = 230,
                                        Brightness2 = 230,
                                    };
                                }
                                else
                                {
                                    settings.scheduleInfo = new scheduleInfo()
                                    {
                                        model = m.modelName,
                                        serviceTag = m.edid.ServiceTag
                                    };
                                }
                            }

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
            Method method = new Method(Log);//Bruce 1213 Move the method to the common code
            bool ret = false;
            if (_DisplayManagerPlugin != null)
            {
                // 確保資料夾存在
                if (!Directory.Exists(savePath))
                {
                    Directory.CreateDirectory(savePath);
                }

                if (method.DirectoryContainsFiles(copyPath))//Bruce 1213 Move the method to the common code
                {
                    // 取得資料夾名稱
                    string folderName = method.GetFolderName(copyPath);//Bruce 1213 Move the method to the common code
                    // 複製指定的 log 文件到選擇的資料夾
                    method.CopyLogFolder(copyPath, savePath);//Bruce 1213 Move the method to the common code
                    writelog($"{nameof(CopyFile)} end");
                    return true;
                }
            }
            if (method != null)//Bruce 1213 Move the method to the common code
            {
                method.Dispose();
                method = null;
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
                    if (activeLayout >= 1000) //or EAEMConstants.EAID_FirstCustom
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

        //removed by Derek 1121
        //private void OnEsi_IsCameraSensorCoverChangeHandler(object sender, bool e)
        //{
        //    Esi_IsCameraSensorCover_ChangeEvent?.AsyncFireAndForget(this, e, System.Threading.CancellationToken.None);
        //}

        //private void OnWALSnoozeTimeLeftInSecondsChangeHandler(object sender, int e)
        //{
        //    WALSnoozeTimeLeftInSeconds_ChangeEvent?.AsyncFireAndForget(this, e, System.Threading.CancellationToken.None);
        //}

        //private void OnEsi_IsWALLockCountdownStartedStatusChangeHandler(object sender, bool e)
        //{
        //    Esi_IsWALLockCountdownStartedChanged_ChangeEvent?.AsyncFireAndForget(this, e, System.Threading.CancellationToken.None);
        //}

        //private void OnEsi_WALLockCountdownChangeHandler(object sender, int e)
        //{
        //    Esi_WALLockCountdownChanged_ChangeEvent?.AsyncFireAndForget(this, e, System.Threading.CancellationToken.None);
        //}

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
                                if (!string.IsNullOrWhiteSpace(Content) && _GlobalSettingParam.GlobalSetting_General.Low_Battery_Level)
                                    _showosd(monitorInfo, OSDType.BatteryLow, OSDType_Device.Headset, Content);
                                else
                                    writelog("[_showosd*******] Content error can't be NullOrWhiteSpace");
                                return Task.CompletedTask;
                            }
                            else if (Device is OSDType_Device.Keyboard)
                            {
                                if (!string.IsNullOrWhiteSpace(Content) && _GlobalSettingParam.GlobalSetting_General.Low_Battery_Level)
                                    _showosd(monitorInfo, OSDType.BatteryLow, OSDType_Device.Keyboard, Content);
                                else
                                    writelog("[_showosd*******] Content error can't be NullOrWhiteSpace");
                                return Task.CompletedTask;
                            }
                            else if (Device is OSDType_Device.Mouse)
                            {
                                if (!string.IsNullOrWhiteSpace(Content) && _GlobalSettingParam.GlobalSetting_General.Low_Battery_Level)
                                    _showosd(monitorInfo, OSDType.BatteryLow, OSDType_Device.Mouse, Content);
                                else
                                    writelog("[_showosd*******] Content error can't be NullOrWhiteSpace");
                                return Task.CompletedTask;
                            }
                            else
                                return Task.CompletedTask;
                        }
                    case OSDType.CollaborationNotAvailable:
                        {
                            if (Device is OSDType_Device.Headset)
                            {
                                if (!string.IsNullOrWhiteSpace(Content))
                                    _showosd(monitorInfo, OSDType.CollaborationNotAvailable, OSDType_Device.Headset, Content);
                                else
                                    writelog("[_showosd*******] Content error can't be NullOrWhiteSpace");
                                return Task.CompletedTask;
                            }
                            else if (Device is OSDType_Device.Keyboard)
                            {
                                if (!string.IsNullOrWhiteSpace(Content))
                                    _showosd(monitorInfo, OSDType.CollaborationNotAvailable, OSDType_Device.Keyboard, Content);
                                else
                                    writelog("[_showosd*******] Content error can't be NullOrWhiteSpace");
                                return Task.CompletedTask;
                            }
                            else if (Device is OSDType_Device.Mouse)
                            {
                                if (!string.IsNullOrWhiteSpace(Content))
                                    _showosd(monitorInfo, OSDType.CollaborationNotAvailable, OSDType_Device.Mouse, Content);
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

        public Task ShowOSD(object monitorInfo, OSDType type, string Content, bool State)
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
                    case OSDType.QAM:
                        {
                            _showosd(monitorInfo, OSDType.QAM, OSDType_Device.Unknown, string.Empty);
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
                                                    _latestBatterylowContent = Content;
                                                    _lastestBatterylowDevice = OSDType_Device.Headset;
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
                                                    //when keyboard battery low, press CapsLock/ScrollLock/NumLockLock combine with
                                                    _latestBatterylowContent = Content;
                                                    _lastestBatterylowDevice = OSDType_Device.Keyboard;
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
                                                    _latestBatterylowContent = Content;
                                                    _lastestBatterylowDevice = OSDType_Device.Mouse;
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

                                    case OSDType.QAM:
                                        {
                                            //if (State)
                                            {
                                                try
                                                {
                                                    _OSD_Controler.QAMHotKeyWin_CloseWindow();
                                                    _OSD_Controler.QAMHotKeyWin_ShowWindow((sreen.WorkingArea.Top / (double)dpiX), (sreen.WorkingArea.Left / (double)dpiX));
                                                }
                                                catch (Exception ex)
                                                {
                                                    writelog($"[_showosd] ERROR - OSDType.QAM: {ex.Message}, State:{State}");
                                                }
                                            }
                                            //else
                                            //{
                                            //    try
                                            //    {
                                            //        _OSD_Controler.QAMHotKeyWin_CloseWindow();
                                            //        _OSD_Controler.QAMHotKeyWin_ShowWindow((sreen.WorkingArea.Top / (double)dpiX), (sreen.WorkingArea.Left / (double)dpiX));
                                            //    }
                                            //    catch (Exception ex)
                                            //    {
                                            //        writelog($"[_showosd] ERROR - OSDType.QAM: {ex.Message}, State:{State}");
                                            //    }
                                            //}
                                        }
                                        break;

                                    case OSDType.CollaborationNotAvailable:
                                        {
                                            if (_DeviceType is OSDType_Device.Keyboard)
                                            {
                                                try
                                                {
                                                    _OSD_Controler.CollaborationNotAvailableWin_CloseWindow();
                                                    _OSD_Controler.CollaborationNotAvailableWin_ShowWindow(Content, (sreen.WorkingArea.Top / (double)dpiX), (sreen.WorkingArea.Left / (double)dpiX));
                                                }
                                                catch (Exception ex)
                                                {
                                                    writelog($"[_showosd] ERROR - OSDType_Device.Keyboard: {ex.Message}");
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

        public Task<bool> LaunchAndArrangeAppsWithEzArrange(Dictionary<String, Bind_AddFullPage_AppCollectionData> sortApps, MonitorInfo moInfo, int eAid)
        {
            //Robert_Lin, 2024-12-5, Change the implementation to EAPlugin
            //
            //OLD by Wayn_Chen
            //if (_IEzMemoryPlugin != null)
            //    return Task.FromResult(_IEzMemoryPlugin.LaunchAndArrangeAppsWithEzArrange(sortApps, moInfo, eAid).Result);
            //else
            //    return null;
            //
            //NEW by Robert_Lin
            if (_DisplayManagerPlugin != null)
            {
                return _DisplayManagerPlugin.LaunchAndArrangeAppsWithEzArrange(sortApps, moInfo, eAid);
                //return Task.FromResult(_DisplayManagerPlugin.LaunchAndArrangeAppsWithEzArrange(sortApps, moInfo, eAid));
            }
            else
            {
                return Task.FromResult(false);
            }
        }

        public Task<bool> CheckEAIDExit(MonitorInfo moinfo, int eAID)
        {
            if (_IEzMemoryPlugin != null)
                return Task.FromResult(_IEzMemoryPlugin.CheckEAIDExit(moinfo, eAID).Result);
            else
                return null;
        }

        public Task<bool> DeleteEAID(MonitorInfo moinfo, int eAID)
        {
            if (_IEzMemoryPlugin != null)
                return Task.FromResult(_IEzMemoryPlugin.DeleteEAID(moinfo, eAID).Result);
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
                if (mi == null)
                {
                    if ((_AllInfoMonitors != null) && (_AllInfoMonitors.Count > 0))
                    {
                        mi = _AllInfoMonitors[0];
                    }
                }
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
                        //if (_AllInfoMonitors != null)
                        //    _AllInfoMonitors.Clear();
                        //else
                        _AllInfoMonitors = new List<MonitorInfo>();
                        writelog("_DisplayManagerPlugin.Re_GetMonitors with token");
                        //_AllInfoMonitors.AddRange((Re_GetMonitors().Result).ToList());
                        SystemEvents_DisplaySettingsChanged(sender, e);
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

        public Task<bool> ByPassHotkey(bool bypass)
        {
            isBypassHotkey = bypass;
            return Task.FromResult(isBypassHotkey);
        }

        public Task<bool> UnRegistAllHotkey()
        {
            _pwr_Mon.UnRegisterAllHotKey();
            return Task.FromResult(true);
        }

        private string loadResourceDictionary(OSThemeEnum oSTheme)
        {
            //telemetry [Application Settings ==>AppMode : "Dark","Light"]
            Debug.WriteLine($"UXSystemParametersChanged:current theme= {oSTheme.ToString()}");
            //Telementry Collection
            //var rt = false;
            string appModeTelementryData = string.Empty;

            switch (oSTheme)
            {
                case OSThemeEnum.Light:
                    appModeTelementryData = "Light";
                    SACommonHelper.SwitchToLightMode();
                    break;

                case OSThemeEnum.Dark:
                    appModeTelementryData = "Dark";
                    SACommonHelper.SwitchToDarkMode();
                    break;

                default:
                    break;
            }
            previousOsTheme = oSTheme;
            return appModeTelementryData;
        }

        #region System Suspend & Resume & SessionEnd

        private void OnSystemSuspend(object sender, EventArgs e)
        {
            SystemSuspend?.Invoke(this, e);
        }

        private void OnSystemResume(object sender, EventArgs e)
        {
            SystemResume?.Invoke(this, e);
        }

        public Task FireSystemSessionEnd()
        {
            SystemSessionEnd?.Invoke(this, EventArgs.Empty);
            return Task.FromResult(true);
        }

        #endregion
    }
}