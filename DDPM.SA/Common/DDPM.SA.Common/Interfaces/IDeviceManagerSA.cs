using DdmLibrary.Utility;
using DDPM.SA.Common.Display;
using DDPM.SA.Common.Settings;
using Dell.Client.Framework.Common;
using DPeMPublic.Common.Enums;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VcpCore.Common;

namespace DDPM.SA.Common
{
    public enum DeviceChangedType
    {
        Display_SettingsChange = 0,
        Display_PlugIn = 1,
        Display_UnPlug = 2,
        Peripherals_SettingsChange = 3,
        Peripherals_PlugIn = 4,
        Peripherals_UnPlug = 5,
        NotifyOnly = 6
    }

    public class DeviceChangedEventArgs : EventArgs
    {
        public string deviceID { get; set; } //for display point to serial number, for peripherals point to Guid
        public DeviceChangedType type { get; set; }
        public MonitorInfo device_display { get; set; }
        public DeviceInfo device_peripherals { get; set; }
        public string changedProperty { get; set; }
    }

    public class UpdateUINotify : EventArgs
    {
        public string UI_Field_Name { get; set; } = string.Empty;
    }

    public interface IDeviceManagerSA : IFrameworkPlugin//, ISettingsManager
    {
        #region EaM

        Task<Dictionary<string, InstalledAppInfo>> GetAllAppList();

        Task<bool> LaunchAndArrangeApps(Dictionary<String, Bind_AddFullPage_AppCollectionData> sortApps);

        #endregion EaM

        #region public for SchedulerManger

        Task StartSchedulerManger(int millisecond);

        Task StopSchedulerManger();

        Task<scheduleInfo> ReadScheduleMonitorSettings(MonitorInfo monitorInfo);

        Task<bool> WriteScheduleMonitorSettings(MonitorInfo monitorInfo, scheduleInfo scheduleInfo);

        Task<bool> MigrateScheduleMonitorSettings(string Model, string ServiceTag, BriConSchedule DDMSetting);

        #endregion public for SchedulerManger

        #region public for ColorPreset

        event EventHandler<string> Coloreset_manual_ChangeEvent;

        Task<Dictionary<string, InstalledAppInfo>> FindAppsbyShell(bool isReload = false);

        void ShowOSD_ColoPreset(MonitorInfo m, string strMsg);

        Task<List<string>> ReadColorPreset(MonitorInfo m);

        //Task<bool> WriteColorPreset(MonitorInfo m, string ColorPreset_Name);
        Task<bool> WriteColorPreset(MonitorInfo m, string ColorPreset_Name, int colorPresetRunType = 0, bool showOSD = true);

        Task<bool> WriteColorPreset_AUTO(MonitorInfo m, string ColorPreset_Name);

        Task<bool> AddColorPresetForMonitorConfig(string index_monitor, string AppName, string ColorPreset_Name);

        void ChangeColorPresetForMonitorConfig(string index_monitor, string AppName, string ColorPreset_Name);

        void DeleteColorPresetForMonitorConfig(string index_monitor, string AppName);

        Task<bool> AutoSetColorPresetForMonitorConfig(MonitorInfo mo, string on_off, bool Islock = false);

        Task<string> GetMonitorProfile(MonitorInfo m);

        Task<bool> SetMonitorProfile(MonitorInfo m, string ColorPreset_Name);

        Task<bool> WriteColorPresetByColorProfile(MonitorInfo m, string ColorProfile_Name);

        //Dean add 0612
        public Task<string> ReadCurrentColorPreset(MonitorInfo m);

        //Jim add 0621
        Task<bool> Notify_refresh_app_list();

        //Jim add 20240801
        Task<IIC_Metadata> DownloadICCData(MonitorInfo m, string savelPath = "");

        //Jim add 20240904
        Task<string> GetAutoColorPresetStatus(MonitorInfo m);

        //Jim add 20240905
        Task<bool> AutoColorManagementForMonitorConfig(MonitorInfo monitorInfo, string off_bymonitor_byhost, string ColorPreset_Name = "", string ICC_profile_Name = "");

        Task<string> GetColorManagementStatus(MonitorInfo m);

        Task<string> GetColorPresetName(int Color_VCPCore_E2);

        Task<int> GetColorVCPCoreValue(string ColorPreset_Name);

        Task<string> Sync_ColorPresetName(MonitorInfo monitorInfo, string ColorPreset_Name);

        #endregion public for ColorPreset

        #region public for Displays

        Task Reset0x52TimerTick(int millisecond, int processID = -0xFF);

        Task<List<MonitorInfo>> GetMonitors();

        Task<List<MonitorInfo>> Re_GetMonitors();

        event EventHandler<VCPchangedEventArgs> VCPchanged;

        event EventHandler<DDCCIchangedEventArgs> DDCCIStatuschanged;

        event EventHandler<DisplaychangedEventArgs> Displaychanged;

        Task<string> GetCapabilitiesString(MonitorInfo monitorInfo);

        Task<string> GetVCPCapabilities(MonitorInfo monitorInfo);

        Task<ObjGetVCP> GetVCPCapability(MonitorInfo monitorInfo, byte code, int opt = 0);

        Task<ObjGetVCP> GetVCPCapability(MonitorInfo monitorInfo, string FunctionName, int opt = 0);

        Task<bool> SetVCPCapability(MonitorInfo monitorInfo, byte code, uint val);

        Task<bool> SetVCPCapability(MonitorInfo monitorInfoX, string FunctionName, string val);

        //
        //The idle state of DisplayService is used to block the handling of system display change event
        void SetDisplayServiceIdle(bool isIdle);

        Task<bool> GetDisplayServiceIdleState();

        //

        #region public for InputSource

        Task<Dictionary<string, InputInfo>> GetInputSourcelist(MonitorInfo monitorInfo);

        Task<bool> SetInputSourcelist(MonitorInfo monitorInfo, Dictionary<string, InputInfo> inputlist);

        //Task<string> GetCurrentInput(MonitorInfo monitorInfo, byte code, int opt = 0);
        //Task<bool> SetCurrentInput(MonitorInfo monitorInfo, byte code, string input);
        Task<string> GetInputName(MonitorInfo monitorInfo, string input);

        Task<bool> SetInputName(MonitorInfo monitorInfo, string input, string name);

        Task<List<string>> GetUSBUpstreamList(MonitorInfo monitorInfo);

        Task<bool> SetUSBUpstream(MonitorInfo monitorInfo, string inputsource, string upstream);

        Task<bool> USBSwitch(MonitorInfo monitorInfo, string inputsource1, string upstream1, string inputsource2, string upstream2);

        #endregion public for InputSource

        #region PIP/PBP

        //PIP/PBP
        //

        Task<UInt16[]> GetPipPbpCapabilitiesWords(MonitorInfo monitorInfo);

        public Task<bool> SetPipModeOff(MonitorInfo monitorInfo);

        public Task<bool> SetPipModeSmall(MonitorInfo monitorInfo);

        public Task<bool> SetPipModeLarge(MonitorInfo monitorInfo);

        public Task<bool> TogglePipSize(MonitorInfo monitorInfo);

        public Task<bool> TogglePipPosition(MonitorInfo monitorInfo);

        public Task<bool> SetPbpMode(MonitorInfo monitorInfo, UInt16 modeCode);

        public Task<bool> VideoSwap(MonitorInfo monitorInfo, UInt16 x, UInt16 y);

        public Task<ObjGetVCP> GetPxpMode(MonitorInfo monitorInfo);

        public Task<List<UInt16>> GetSubInputList(MonitorInfo monitorInfo);

        public Task<List<InputSourceObj>> GetSubInputs(MonitorInfo monitorInfo);

        public Task<bool> SetSubInputs(MonitorInfo monitorInfo, InputSourceObj? sub1, InputSourceObj? sub2, InputSourceObj? sub3);

        public Task<bool> UsbSwitch1(MonitorInfo monitorInfo, UInt16 target = 0);

        #endregion PIP/PBP

        #region public for USBKVM

        Task<Dictionary<string, PCsInfo>> GetUSBKVMPCsList(MonitorInfo monitorInfo, Dictionary<string, InputInfo> inputList, List<InputSourceObj> subInputList);

        Task<bool> SetUSBKVMPCsList(MonitorInfo monitorInfo, Dictionary<string, PCsInfo> pcsList);

        Task<Dictionary<string, PCsInfo>> PCInfoSwap(Dictionary<string, PCsInfo> pcsList, string swapPC1, string swapPC2);

        Task<bool> GetOnUSBKVM(MonitorInfo monitorInfo);

        Task<bool> SetOnUSBKVM(MonitorInfo monitorInfo, bool isON);

        #endregion public for USBKVM

        #region EasyArrange

        /// <summary>
        /// Enable/Disable EasyArrange function for all monitors.
        /// When Disabled (isEnable=false), DDPM will not show the WorkWindow (to arrange window),
        /// but user can edit/setup in DDPM.UI and save their settings.
        /// </summary>
        /// <param name="isEnabled"></param>
        /// <returns></returns>
        public Task<bool> SetEAFunctionEnabled(bool isEnabled);

        public Task<ObjGetVCP> GetEAFunctionEnabled();

        public event EventHandler<EAArgs> EASettingsChanged;

        public Task<bool> SetEAWrokSplit(MonitorInfo monitorInfo, int cellCount, char splitKey, List<double>? settings);

        //Robert_Lin, 2024-9-13 Remove unused interfaces
        //public Task<bool> RequestEditSplit(MonitorInfo monitorInfo, int cellCount, char splitKey, string customName, List<double>? settings = null);

        //Robert_Lin, 2024-9-13 Remove unused interfaces
        //public event EventHandler<string> EAEditCompleted;

        public event EventHandler<string> EAEditStarted;

        //Task<string> WriteEasyArrangeSettings(EAMonitorSettings eaMonitorSettings);

        //public Task<EAMonitorSettings> ReadEasyArrangeSettings(string monitorModel, string serialNumber);

        //Robert_Lin, 2024-8-4 new added
        public Task<bool> EAEditCommand(MonitorInfo monitorInfo, EAArgs args);

        public event EventHandler<EAArgs> EAEditReturn;

        public Task<bool> WriteEAMonitorSettings(MonitorInfo monitorInfo, EAMonitorSettings eaSettings);

        public Task<EAMonitorSettings> ReadEAMonitorSettings(MonitorInfo monitorInfo);

        //public Task<bool> EAReloadMonitorSettings(MonitorInfo monitorInfo);
        //public Task<bool> EASaveOptions(MonitorInfo monitorInfo, EAMonitorSettings eaSettings);

        //Robert_Lin, 2024-9-18 added for EzSettings
        public Task<EzSettings> ReadEzSettings();

        public Task<bool> WriteEzSettings_IsWidthoutGap(bool newValue);

        public Task<bool> WriteEzSettings_IsOnlyAllowWhenShiftKeyPressed(bool newValue);

        public Task<bool> WriteEzSettings_IsSpanAcrossMultiMonitors(bool newValue);

        public Task<bool> WriteEzSettings_IsAwsEnabled(bool newValue);

        public Task<bool> SetEASelectedLayout(MonitorInfo monitorInfo, SplitJson spJson);
        //Robert_Lin, 2024-10-12 added, move EACustomList to UserSettings from MonitorSettings
        public Task<SplitJson[]> ReadEACustomList();
        public Task<bool> WriteEACustomList(SplitJson[] customList);
        #endregion EasyArrange

        #region EasyMemory

        public Task<bool> WriteMonitorEasyArrangement(MonitorInfo monitorInfo, EasyArrangementDDPM easyArrangementDDPM);

        public Task<bool> UpdateMonitorEzProfileSettingDDPM(MonitorInfo monitorInfo, EzProfileSettingDDPM profileSettingDDPM);

        public Task<EasyArrangementDDPM> ReadMonitorEasyArrangement(MonitorInfo monitorInfo);

        public Task<bool> WriteUserListEAProfileDDPM(List<EAProfileDDPM> eaProfileList);

        public Task<bool> WriteUserEAProfileDDPM(EAProfileDDPM eaProfile);

        public Task<bool> UpdateUserEAProfileDDPM(EAProfileDDPM eaProfile);

        public Task<List<EAProfileDDPM>> ReadUserEAProfileDDPM();

        public Task<bool> CleanUserEzProfiles();

        #endregion EasyMemory

        #endregion public for Displays

        #region public for Peripherals

        Task<DeviceHelper> GetDevices(bool Rescan = false);

        Task<CTKMessageHelper> GetCTKMessageHelper();

        Task<RFDeviceHelper> GetRFDongleDevices();

        //event EventHandler<DeviceChangedEventArgs> Peripherals_Notify;
        event EventHandler<bool> Peripherals_UpdateNotify;

        Task SetBackLightingControls(int newValue, Guid deviceId);

        Task SetBackLightingLevel(int newValue, Guid deviceId);

        Task SetCollaborationBlinkEffectEnable(bool newValue, Guid deviceId);

        Task SetCollaborationCameraEnable(bool newValue, Guid deviceId);

        Task SetCollaborationChatEnable(bool newValue, Guid deviceId);

        Task SetCollaborationDoubleTapEnable(bool newValue, Guid deviceId);

        Task SetCollaborationKeyEnable(bool newValue, Guid deviceId);

        Task SetCollaborationMicEnable(bool newValue, Guid deviceId);

        Task SetCollaborationScreenShareEnable(bool newValue, Guid deviceId);

        Task SetDPILevel(int newValue, Guid deviceId);

        Task SetDPIValue(int newValue, Guid deviceId);

        Task SetPrimaryMouseButton(MouseButton newMouseButton, Guid deviceId);

        Task SetTouchScrollSensitivityLevel(int newTouchScrollSensitivityLevel, Guid deviceId);

        Task UnPair(Guid deviceId);

        Task StartPairing(Guid deviceId);

        Task StopPairing(Guid deviceId);

        Task StopPairingPen();

        Task SetWiredAudioIMicNSEnable(bool newValue, Guid deviceId);

        Task SetWiredAudioMicMuteSoundEnable(bool newValue, Guid deviceId);

        Task SetWiredAudioVolumeAdjustmentTone(int newValue, Guid deviceId);

        Task SetAncMode(int newValue, Guid deviceId);

        Task SetAncGain(int newValue, Guid deviceId);

        Task SetSelectedPreset(int newValue, Guid deviceId);

        Task SetBandsGain(int newValue, Guid deviceId, string bandGainNumber);

        Task SetMicNoiseCancellation(bool newValue, Guid deviceId);

        Task SetSidetone(bool newValue, Guid deviceId);

        Task SetSidetoneLevel(int newValue, Guid deviceId);

        Task SetWearDetection(int newValue, Guid deviceId);

        Task SetWearDetectionForCLI(int newValue, Guid deviceId);

        Task SetBusyLight(bool newValue, Guid deviceId);

        Task SetVoiceGuidance(bool newValue, Guid deviceId);

        Task SetMicNCIncoming(bool newValue, Guid deviceId);

        //Task SetEqualizerValues(ILogicalDeviceHeadset logicalDeviceHeadset, DeviceInfo info);
        Task SetIsMicEnumerationOn(bool newValue, Guid deviceId);

        // webcam presence detection
        Task SetWALTime(int newValue, Guid deviceId);

        Task SetSnooze(int newValue, Guid deviceId);

        Task SetSnoozeLength(int newValue, Guid deviceId);

        Task SetIsProximitySensorEnable(bool newValue, Guid deviceId);

        Task SetIsWakeonApproachEnable(bool newValue, Guid deviceId);

        Task SetIsWalkAwayLockEnable(bool newValue, Guid deviceId);

        Task<int> GetSnooze(Guid deviceId);

        Task<int> GetSnoozeLength(Guid deviceId);

        #endregion public for Peripherals

        #region public for CMA/CLI

        //Task<CommandOutput_DeviceConnection> queryConnectedDeviceInfo(CommandInput_notifyDeviceConnection input);
        //Task<List<CommandResult>> listConnectedDeviceInfo();

        event EventHandler<UpdateUINotify> UIUpdateNotify;

        void OnUIUpdateNotify(UpdateUINotify e);

        #endregion public for CMA/CLI

        #region public for display properties

        /// <summary>
        /// HDR change event，return HDR status
        /// </summary>
        event EventHandler<bool> HDRChangeEvent;

        Task<DisplayPropertiesInfo> GetDisplayPropertiesInfo(MonitorInfo monitorInfos);

        Task<bool> SetDisplayPropertiest(MonitorInfo monitorInfos, Properties properties, DisplayOrientation orientation);//Bruce 08-09 Modify the incoming value

        Task<bool> SetResolutions(MonitorInfo monitorInfos, Properties properties);

        Task<bool> SetOrientation(MonitorInfo monitorInfos, DisplayOrientation orientation);

        Task<bool> CallWindowsDisplaySetting();

        Task<bool> GetHDRStatus(MonitorInfo monitorInfos);

        Task<bool> SetHDRStatus(MonitorInfo monitorInfos, bool onoff);

        Task<bool> SetUSBCPrioritizationType(MonitorInfo monitorInfos, USBCPrioritizationType type);

        //0606 Bruce 新增鎖定自動旋轉方向
        Task<bool> LockRotate(bool onoff);

        //0606 Bruce 新增鎖定自動旋轉方向
        Task<bool> GetLockRotateStatus();

        Task<string> GetOSDOrientation(MonitorInfo monitorInfos);

        Task<bool?> SetOSDOrientation(MonitorInfo monitorInfos, string Orientation);

        #endregion public for display properties

        #region public for settings

        Task<DDPMSettings> ReloadAppConfigData(bool force_reload = false);

        Task<bool> SetAppConfigData(DDPMSettings data);

        Task<string> GetAppIconFolderPath();

        Task<List<ColorPresetSettings>> ReadColorPresetSettings();

        Task<bool> WriteColorPresetSettings(List<ColorPresetSettings> colorPresetSettings);

        Task<object> ReadRegistryData(RegistryHive hive, string keyPath, string keyName);

        Task<bool> WriteRegistryData(RegistryHive hive, string keyPath, string keyName, object value);

        #endregion public for settings

        #region public for hotkey

        Task<List<HotkeySettings>> ReadHotkeySettings();

        Task<bool> WriteHotkeySettings(List<HotkeySettings> hotkeySettings);

        public Task<(HotkeySettings, List<HotkeyData>)> ReadCurrentHotkey(MonitorInfo mo);// EDID monitorEdid);

        public Task<bool> ReloadHotkeyConfigData();

        public Task<HotkeyWarning> GetHotkeyConflicts(HotkeyInfo hotkeyInfo);

        public Task<bool> UnHook();

        public Task<bool> Hook();

        //public Task<bool> SaveHotkeySetting(EDID monitorEdid, HotkeyInfo info);
        public Task<bool> SaveHotkeySetting(MonitorInfo mo, HotkeyInfo info);

        public Task<bool> SaveHotkeyOptionOnly(HotkeySettings hotkeySettings);

        public Task SetLastSelectedMonitorFromUI(MonitorInfo mo);

        #endregion public for hotkey

        #region public for PowerNap

        public Task<bool> SavePowerNapSetting(PowerNapSetting powerNapSettings);

        public Task<List<PowerNapSetting>> ReadPowerNapSettings();

        #endregion public for PowerNap

        #region public for FW Update by Bruce

        event EventHandler<UpdateProgressInfo> ProgressUpdate_Notify;

        event EventHandler<bool> FWU_UILock_Notify;

        /// <summary>
        /// 供CLI使用
        /// </summary>
        event EventHandler<List<FWUpdateInfo>> DownloadAndInstall_Result_Notify;

        //Task<FWUpdateInfoPackage> GetFWUpdateInfo(bool isShowNotify = true, bool isForce = false, bool isDefer = false, List<DeviceType> deviceTypeList = null, bool UODMode = false);

        Task<FWUpdateInfoPackage> GetFWUpdateInfo(bool isShowNotify = true, bool isForce = false, bool isDefer = false, List<DeviceType> deviceTypeList = null, bool UODMode = false, bool isOnlyDisplay = false);
        Task<FWUErrorCode> Install(string installPath, bool isOnlyDisplay = false);

        //0531 Bruce 因應IL的現有安裝包修改判斷，IDeviceManagerSA.cs中三個關於FWUpdate的方法移除並修改DownloadAndInstall回傳值
        Task<List<FWUpdateInfo>> DownloadAndInstall(List<FWUpdateInfo> fwUpdateInfos, string installPath = "");

        void SetUILockStatus(bool isLockFWU_UI);

        Task<bool> GetUILockStatus();

        Task<bool> SetSkipCA(bool isSkipCA);

        Task<bool> GetSkipCA();

        #endregion public for FW Update by Bruce

        #region public ALS functions

        Task<ALSConfig> GetALSFeatureValue(MonitorInfo monitorInfos, ALSFeatureQueryType type, int value);

        Task<bool> SetALSFeatureValue(MonitorInfo monitorInfos, ALSConfig param, ALSFeatureQueryType type, string value);

        Task<List<ALSConfig>> GetConnectedALSConfig();

        Task<List<ALSConfig>> GetAllExistAlsConfig();

        Task<List<ALSConfig>> UpdateExistAlsConfig(List<MonitorInfo> monitorInfoMain);

        Task<bool> SynchronizeALSFeatureValue(ALSConfig monitorALS);

        #endregion public ALS functions

        #region for NKVM

        event EventHandler<NKVMRespone> NKVMCLIRespone;

        Task CreatNewNamedpipe();

        Task<bool> IsNamedpipeConnected();

        Task SupportedNKVMMonitors();

        Task<bool> GetOnNKVM(MonitorInfo monitorInfo);

        Task SetOnNKVM(MonitorInfo monitorInfo, bool ison);

        Task<bool> isNKVMSupportMonitor(MonitorInfo monitorInfo);

        Task NKVM_ChangeMonitorIndex(MonitorInfo monitorInfo);

        Task GetNKVMVersion();

        Task GetNKVMStatus();

        Task GetNKVMAutoConnect();

        Task GetNKVMContentTransfer();

        Task GetNKVMIncommingPort();

        Task GetNKVMOutgoingPort();

        Task GetNKVMContentTransferPort();

        Task GetNKVMSettings();

        Task NKVM_State(bool state);

        Task CallNKVMConnent();

        #endregion for NKVM

        #region public for SW Update

        Task<SWUpdateInfoPackage> SW_GetSWUpdateInfo(bool isShowNotify = true, bool isForce = false, bool isDefer = false);

        Task<List<SWUpdateInfo>> SW_DownloadAndInstall(List<SWUpdateInfo> swUpdateInfos, string installPath = "");

        #endregion public for SW Update

        #region public for ImpExpSettings

        Task<bool> DisplayExportSettings(MonitorInfo monitorInfo, string path);

        Task<bool> DisplayImportSettings(MonitorInfo monitorInfo, bool isSameModel, string path);

        Task SetSameModel(MonitorInfo monitorInfo, bool isSameModel);

        Task<bool> GetSameModel(MonitorInfo monitorInfo);

        #endregion public for ImpExpSettings

        //public for GUI to get the changes of display and peripherals
        event EventHandler<DeviceChangedEventArgs> DeviceChanged;

        #region public for IT lock event

        //IT lock
        event EventHandler<ITSettingEventArgs> ITSettingsActionEvent;

        #endregion public for IT lock event

        #region Gaming

        event EventHandler<GamingDisplayPropertiesInfo> GamingChangeEvent;

        Task<GamingDisplayPropertiesInfo> GetGamingProperties_SupportedList(MonitorInfo monitorInfo);

        Task<Gaming_GameEnhancementMode> GetCurrentGame_EnhancementMode(MonitorInfo monitorInfo);

        Task<Gaming_ResponseTime> GetCurrentGaming_ResponseTime(MonitorInfo monitorInfo);

        Task<Gaming_DarkStabilizer> GetCurrentGaming_DarkStabilizer(MonitorInfo monitorInfo);

        Task<Gaming_HDRType> GetCurrentGaming_HDRType(MonitorInfo monitorInfo);

        Task<Gaming_DualResolutionType> GetCurrentGaming_DualResolutionType(MonitorInfo monitorInfo);

        Task<Gaming_VisionEngineType> GetCurrentGaming_VisionEngineType(MonitorInfo monitorInfo);

        Task<bool[]> GetCurrentGaming_VisionEngineEnableType(MonitorInfo monitorInfo, GamingDisplayPropertiesInfo gamingDisplayPropertiesInfo);

        Task<bool> SetGameEnhancementMode(MonitorInfo monitorInfo, Gaming_GameEnhancementMode GameEnhancementMode);

        Task<bool> SetGaming_ResponseTime(MonitorInfo monitorInfo, Gaming_ResponseTime ResponseTime);

        Task<bool> SetGaming_DarkStabilizer(MonitorInfo monitorInfo, Gaming_DarkStabilizer DarkStabilizer);

        Task<bool> SetGaming_HDRType(MonitorInfo monitorInfo, Gaming_HDRType HDRType);

        Task<bool> SetGaming_DualResolutionType(MonitorInfo monitorInfo, Gaming_DualResolutionType DualResolutionType);

        Task<bool> SetGaming_VisionEngineEnableType(MonitorInfo monitorInfo, bool[] VisionEngineEnableType);

        #endregion Gaming

        #region public for DTPProxy

        #region Mouse

        Task<int> GetDpiValue(string Guid);
        Task<JArray> GetMouseProgrammableKeys(string Guid);
        Task<JArray> GetAppSpecificProfiles(string Guid);
        Task<bool> DeleteMouseAllAssignedActions(string Guid);
        Task<JArray> GetMouseAssignableActions(string Guid);

        Task SetDPIValue(string Guid, int newValue);
        Task SetMouseAction(string Guid, byte[] newValue);
        Task SetCurrentSelectedAppSpecificProfile(string Guid, string newValue);
        Task DeleteMouseAssignedAction(string Guid, int newValue);
        Task SetMouseAssignDialogAction(string Guid, byte[] newValue);
        Task SetMouseAssignKeystrokeAction(string Guid, byte[] newValue);

        #endregion

        #region Keyboard
        Task SetKbDeleteAssignedAction(string Guid, int newValue);
        Task<JArray> GetKeyboardDeviceItemsEx();
        Task<JArray> GetKbProgrammableKeys(string Guid);
        Task<JArray> GetKbAssignableActions(string Guid);
        Task SetKbAssignedAction(string Guid, string newValue);
        Task SetKbAssignDialogAction(string Guid, string newValue);
        Task SetKbAssignKeystrokeAction(string Guid, string newValue);

        #endregion

        #region Pen
        Task<JArray> GetPenDeviceItemsEx();
        Task<string> GetEraserDoublePressValues();
        Task<string> GetEraserSinglePressValues();
        Task<string> GetEraserLongPressValues();
        Task<string> GetSideSwitchSinglePressValues();
        Task<string> GetMenuSinglePressValues();
        Task<string> GetLaunchableAppValues();
        Task<string> GetEraserDoublePressSetting();
        Task<string> GetEraserSinglePressSetting();
        Task<string> GetEraserLongPressSetting();
        Task<string> GetSideTopSwitchSinglePressSetting();
        Task<string> GetSideBottomSwitchSinglePressSetting();
        Task<string> GetMenuSinglePressSetting();
        Task<bool> GetMenuCenterRightClickSetting();
        Task<bool> GetIsSideTopButtonHoverClick();
        Task<bool> GetIsSideBottomButtonHoverClick();
        Task<string> PairingPen();
        Task UnPairPen(string Guid);

        Task SetEraserDoublePressSetting(string itemID, byte[] newValue);

        Task SetEraserLongPressSetting(string itemID, byte[] newValue);

        Task SetEraserSinglePressSetting(string itemID, byte[] newValue);

        Task SetIsSideBottomButtonHoverClick(string itemID, bool newValue);

        Task SetIsSideTopButtonHoverClick(string itemID, bool newValue);

        Task SetMenuSinglePressSetting(string itemID, byte[] newValue);

        Task SetMenuCenterRightClickSetting(string itemID, bool newValue);

        Task SetSideBottomSwitchSinglePressSetting(string itemID, byte[] newValue);

        Task SetSideTopSwitchSinglePressSetting(string itemID, byte[] newValue);

        Task SetTiltSensitivity(string itemID, int newValue);

        Task SetTipSensitivity(string itemID, int newValue);

        #endregion Pen

        #region Webcam

        Task<JArray> GetPresetProfiles(string Guid);

        Task<JArray> GetCustomProfiles(string Guid);

        Task<string> GetProfile(string Guid);

        Task<string> GetProfileName(string Guid);

        Task<int> GetBrightness(string Guid);

        Task<string> GetCameraFirmwareVersionByDTP(string Guid);

        Task<bool> GetIsPropertyFOVSupportedByDTP(string Guid);

        Task<int> GetFieldOfView(string Guid);

        Task<bool> GetIsPropertyHDRSupported(string Guid);

        Task<bool> GetIsHDROn(string Guid);

        Task<bool> GetIsPropertyAntiFlickerSupported(string Guid);

        Task<int> GetAntiFlickerValueByDTP(string Guid);

        Task<bool> GetIsPropertyAutoFramingSupported(string Guid);

        Task<bool> GetIsAutoFramingOn(string Guid);

        Task SetIsMicEnumerationOn(string Guid, bool newValue);

        Task SetProfile(string Guid, string newValue);

        Task SetProfileName(string Guid, string newValue);

        Task CreateCustomProfile(string Guid, string newValue);

        Task DeleteProfile(string Guid, string newValue);

        Task SetZoom(string Guid, int newValue);

        Task SetIsAutoFramingOn(string Guid, bool newValue);

        Task SetIsAutoFramingTransitionOn(string Guid, bool newValue);

        Task SetAutoFramingSensitivity(string Guid, int newValue);

        Task SetAutoFramingFrameSize(string Guid, int newValue);

        Task SetFieldOfView(string Guid, int newValue);

        Task SetIsFocusOn(string Guid, bool newValue);

        Task SetFocus(string Guid, int newValue);

        Task SetPriority(string Guid, int newValue);

        Task SetIsHDROn(string Guid, bool newValue);

        Task SetIsAutoWhiteBalanceOn(string Guid, bool newValue);

        Task SetAutoWhiteBalance(string Guid, int newValue);

        Task SetBrightness(string Guid, int newValue);

        Task SetSharpness(string Guid, int newValue);

        Task SetContrast(string Guid, int newValue);

        Task SetSaturation(string Guid, int newValue);

        Task SetAntiFlicker(string Guid, int newValue);

        Task SetTilt(string Guid, int newValue);

        Task SetPan(string Guid, int newValue);

        // webcam presence detection
        Task SetWALTime(string Guid, int newValue);

        Task SetSnooze(string Guid, int newValue);

        Task SetSnoozeLength(string Guid, int newValue);

        Task SetIsProximitySensorEnable(string Guid, bool newValue);

        Task SetIsWakeonApproachEnable(string Guid, bool newValue);

        Task SetIsWalkAwayLockEnable(string Guid, bool newValue);

        Task SetIsPrioritizeExternalWebcam(string Guid, bool newValue);

        Task ResetToDefault_webcam(string Guid, bool newValue);

        Task<int> GetWALTime(string Guid);

        Task<int> GetSnooze(string Guid);

        Task<int> GetSnoozeLength(string Guid);

        Task<bool> GetIsProximitySensorEnable(string Guid);

        Task<bool> GetIsWakeonApproachEnable(string Guid);

        Task<bool> GetIsWalkAwayLockEnable(string Guid);

        Task<bool> GetIsPrioritizeExternalWebcam(string Guid);

        #endregion Webcam

        #region Headset

        #region Headset Set
        Task<bool> SetMicNoiseCancellationAsync(string Guid, bool newValue);
        Task<bool> SetSidetoneAsync(string Guid, bool newValue);
        Task<bool> SetBusyLightAsync(string Guid, bool newValue);
        Task<bool> SetVoiceGuidanceAsync(string Guid, bool newValue);
        Task<bool> SetSelectedPresetAsync(string Guid, int newValue);
        Task<bool> SetSidetoneLevelAsync(string Guid, int newValue);
        Task<bool> SetBandsGainAsync(string Guid, byte[] newValue);
        Task<bool> SetAncModeAsync(string Guid, int newValue);
        Task<bool> SetAncGainAsync(string Guid, int newValue);
        Task<bool> SetWearDetectionAsync(string Guid, int newValue);
        Task<bool> SetMicNCIncomingAsync(string Guid, bool newValue);
        Task<bool> SetUnPairAsync(string Guid, bool newValue);
        Task<bool> SetFactoryResetAsyncValueForHeadset(string Guid, bool newValue);

        #endregion

        #region Headset Get

        Task<JArray> GetDeviceItemsExAsync(string Guid);
        //Task<DeviceInterfaceType> GetInterfaceTypeAsync(string Guid);
        Task<string> GetDeviceNameAsync(string Guid);
        Task<string> GetDeviceIdAsync(string Guid);
        Task<string> GetPluginIdAsync(string Guid);
        Task<int> GetODMIdAsync(string Guid);
        Task<string> GetModelNumberAsync(string Guid);
        Task<int> GetInstanceNumberAsync(string Guid);
        Task<int> GetInstanceIdAsync(string Guid);
        Task<string> GetFirmwareVersionAsync(string Guid);
        Task<string> GetDeviceTypeAsync(string Guid);
        Task<string> GetParentDeviceTypeAsync(string Guid);
        Task<bool> GetIsBatteryLevelSupportedAsync(string Guid);
        Task<int> GetBatteryLevelAsync(string Guid);
        Task<string> GetDeviceBatteryStatusAsync(string Guid);
        Task<string> GetPairingStatusAsync(string Guid);
        Task<int> GetMaxPairingSlotsAsync(string Guid);
        Task<int> GetPairedDeviceCountAsync(string Guid);
        Task<int> GetTotalNumberOfPairedHostNameAsync(string Guid);
        Task<string> GetSerialNumberAsync(string Guid);
        Task<bool> GetIsReadyAsync(string Guid);
        Task<bool> GetIsDirtyAsync(string Guid);
        Task<bool> GetIsMicNoiseCancellationSupportedAsync(string Guid);
        Task<bool> GetIsSidetoneSupportedAsync(string Guid);
        Task<bool> GetIsBusyLightSupportedAsync(string Guid);
        Task<bool> GetIsVoiceGuidanceSupportedAsync(string Guid);
        Task<bool> GetIsPresetsSupportedAsync(string Guid);
        Task<bool> GetIsEqualizerSupportedAsync(string Guid);
        //Task<HeadsetConnectionType> GetConnectionTypeAsync(string Guid);
        Task<bool> GetIsANCSupportedAsync(string Guid);
        Task<bool> GetIsWearDetectionSupportedAsync(string Guid);
        Task<bool> GetIsWearDetectionSensitivitySupportedAsync(string Guid);
        Task<bool> GetIsWearDetectionPauseMusicSupportedAsync(string Guid);
        Task<bool> GetIsWearDetectionMuteMicSupportedAsync(string Guid);
        Task<bool> GetIsWearDetectionQuickPauseSupportedAsync(string Guid);
        Task<bool> GetMicNoiseCancellationAsync(string Guid);
        Task<bool> GetMicNCIncomingAsync(string Guid);
        Task<bool> GetSidetoneAsync(string Guid);
        Task<bool> GetBusyLightAsync(string Guid);
        Task<bool> GetVoiceGuidanceAsync(string Guid);
        Task<int> GetSelectedPresetAsync(string Guid);
        Task<int> GetSidetoneLevelAsync(string Guid);
        Task<bool> GetMuteStatusAsync(string Guid);
        Task<byte[]> GetBandsGainAsync(string Guid);
        Task<int> GetAncModeAsync(string Guid);
        Task<int> GetAncGainAsync(string Guid);
        Task<int> GetWearDetectionAsync(string Guid);
        Task<bool> GetIsMicNCIncomingSupportedAsync(string Guid);

        #endregion

        #endregion

        #region Wires Audio

        Task<int> GetBassAsync(string guid);
        Task SetBassAsync(string guid, int newValue);
        Task<int> GetMidRangeAsync(string guid);
        Task SetMidRangeAsync(string guid, int newValue);
        Task<int> GetTrebleAsync(string guid);
        Task SetTrebleAsync(string guid, int newValue);
        Task SetIsWiredAudioMicMuteSoundEnableAsync(string guid, bool newValue);
        Task<bool> GetIsWiredAudioMicMuteSoundEnableAsync(string itemID);
        Task SetWiredAudioVolumeAdjustmentToneAsync(string guid, int newValue);
        Task<int> GetWiredAudioVolumeAdjustmentToneAsync(string itemID);
        Task<bool> GetIsWiredAudioIMicNSEnableAsync(string itemID);
        Task SetIsWiredAudioIMicNSEnableAsync(string itemID, bool newValue);
        Task SetResetToDefaultAsyncForSoundbar(string itemID, bool newValue);

        #endregion

        #endregion public for DTPProxy

        #region OSD

        Task ShowOSD(object monitorInfo, OSDType type, OSDType_Device Device, string Content);

        Task ShowOSD(object monitorInfo, OSDType type, string Content, bool State);

        Task ShowOSD(object monitorInfo, OSDType type, bool State);

        Task ShowOSD(object monitorInfo, OSDType type);

        #endregion OSD

        #region GlobalSetting

        event EventHandler GlobalSettingChangeEvent;

        Task<GlobalSettingParam> GetGlobalSettingParam();

        Task<bool> Set_GlobalSetting_DisplayLowBatteryLevel(bool isDisplay);

        Task<bool> Set_GlobalSetting_DisplayKeyboardLockKey(bool isDisplay);

        Task<bool> Set_GlobalSetting_DisplayWB7022CoverState(bool isDisplay);

        Task<bool> Set_GlobalSetting_DisplayMuteState(bool isDisplay);

        Task<bool> Set_GlobalSetting_DisplayColorPresetAndEasyMemory(bool isDisplay);

        Task<bool> Set_GlobalSetting_EnableQuickAccessWidget(bool isEnable);

        Task<bool> Set_GlobalSetting_EnableQuickAccessWidget_Reminder(bool isEnable);

        Task<bool> Set_GlobalSetting_EnableTelemetryConsent(bool isEnable);

        #region OutReport

        Task<bool> ExportMonitorAssetReport(List<MonitorInfo> monitorInfos, string savePath);

        Task<bool> SaveLogFile(string saveFolderPath = "");

        #endregion OutReport

        //For common json file read/write
        Task<string> ReadSerializedContentFromFile(string filePath);

        Task<bool> WriteSerializedContentToFile(string filePath, string content);

        #endregion GlobalSetting
    }
}