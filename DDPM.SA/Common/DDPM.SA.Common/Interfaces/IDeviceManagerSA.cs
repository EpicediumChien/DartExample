using DDPM.SA.Common.Display;
using DDPM.SA.Common.Settings;
using Dell.Client.Framework.Common;
using DPeMPublic.Common.Enums;
using IndiLogic.DPeM.Broker;
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
        #region public for SchedulerManger

        Task StartSchedulerManger(int millisecond);

        Task StopSchedulerManger();

        #endregion public for SchedulerManger

        #region public for ColorPreset

        Task<Dictionary<string, InstalledAppInfo>> FindAppsbyShell(bool isReload = false);

        void ShowOSD_ColoPreset(MonitorInfo m, string strMsg);//, bool isMainUI = false);

        Task<List<string>> ReadColorPreset(MonitorInfo m);

        // 20240619 jim modify
        //Task<bool> WriteColorPreset(string monitor_index, MonitorInfo m, string ColorPreset_Name);
        //Task<bool> WriteColorPreset_AUTO(string monitor_index, MonitorInfo m, string ColorPreset_Name);
        Task<bool> WriteColorPreset(MonitorInfo m, string ColorPreset_Name);

        Task<bool> WriteColorPreset_AUTO(MonitorInfo m, string ColorPreset_Name);

        Task<bool> AddColorPresetForMonitorConfig(string index_monitor, string AppName, string ColorPreset_Name);

        void ChangeColorPresetForMonitorConfig(string index_monitor, string AppName, string ColorPreset_Name);

        void DeleteColorPresetForMonitorConfig(string index_monitor, string AppName);

        void AutoSetColorPresetForMonitorConfig(MonitorInfo mo, string on_off, bool Islock = false);

        Task<string> GetMonitorProfile(MonitorInfo m);

        Task<bool> SetMonitorProfile(MonitorInfo m, string ColorPreset_Name);

        Task<bool> WriteColorPresetByColorProfile(MonitorInfo m, string ColorProfile_Name);

        //Dean add 0612
        public Task<string> ReadCurrentColorPreset(MonitorInfo m);

        //Jim add 0621
        Task<bool> Notify_refresh_app_list();

        //Jim add 20240801
        Task<IIC_Metadata> DownloadICCData(MonitorInfo m, string savelPath = "");

        //Jim add 20240820
        Task<bool> ColorManagement_Off(MonitorInfo mo);
        Task<bool> ColorManagement_Bymonitor(MonitorInfo mo);
        Task<bool> ColorManagement_Byhost(MonitorInfo mo);

        #endregion public for ColorPreset

        #region public for Displays

        Task Reset0x52TimerTick(int millisecond);

        Task<List<MonitorInfo>> GetMonitors(bool reScan = false);

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

        public Task<bool> SetEAWrokSplit(MonitorInfo monitorInfo, int cellCount, char splitKey, List<double>? settings);

        public Task<bool> RequestEditSplit(MonitorInfo monitorInfo, int cellCount, char splitKey, string customName, List<double>? settings = null);

        public event EventHandler<string> EAEditCompleted;

        public event EventHandler<string> EAEditStarted;

        Task<string> WriteEasyArrangeSettings(EAMonitorSettings eaMonitorSettings);

        public Task<EAMonitorSettings> ReadEasyArrangeSettings(string monitorModel, string serialNumber);

        //Robert_Lin, 2024-8-4 new added
        public Task<bool> EAEditCommand(MonitorInfo monitorInfo, EAArgs args);

        public event EventHandler<EAArgs> EAEditReturn;

        #endregion EasyArrange

        #endregion public for Displays

        #region public for Peripherals

        Task<DeviceHelper> GetDevices();
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

        Task SetBusyLight(bool newValue, Guid deviceId);

        Task SetVoiceGuidance(bool newValue, Guid deviceId);

        Task SetMicNCIncoming(bool newValue, Guid deviceId);

        //Task SetEqualizerValues(ILogicalDeviceHeadset logicalDeviceHeadset, DeviceInfo info);
        Task SetIsMicEnumerationOn(bool newValue, Guid deviceId);

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

        #endregion public for settings

        #region public for hotkey

        Task<List<HotkeySettings>> ReadHotkeySettings();

        Task<bool> WriteHotkeySettings(List<HotkeySettings> hotkeySettings);

        public Task<HotkeySettings> ReadCurrentHotkey(EDID monitorEdid);

        public Task<bool> ReloadHotkeyConfigData();

        public Task<HotkeyWarning> GetHotkeyConflicts(HotkeyInfo hotkeyInfo);

        public Task<bool> UnHook();

        public Task<bool> Hook();

        public Task<bool> SaveHotkeySetting(EDID monitorEdid, HotkeyInfo info);

        public Task<bool> SaveHotkeyOptionOnly(HotkeySettings hotkeySettings);

        #endregion public for hotkey

        #region public for PowerNap

        public Task<bool> SavePowerNapSetting(PowerNapSetting powerNapSettings);

        public Task<List<PowerNapSetting>> ReadPowerNapSettings();

        #endregion public for PowerNap

        #region public for FW Update by Bruce

        event EventHandler<FWUpdateInfo> ProgressUpdate_Notify;

        event EventHandler<bool> FWU_UILock_Notify;

        /// <summary>
        /// 供CLI使用
        /// </summary>
        event EventHandler<List<FWUpdateInfo>> DownloadAndInstall_Result_Notify;

        Task<FWUpdateInfoPackage> GetFWUpdateInfo(bool isShowNotify = true, bool isForce = false, bool isDefer = false, List<DeviceType> deviceTypeList = null, bool UODMode = false);

        //0531 Bruce 因應IL的現有安裝包修改判斷，IDeviceManagerSA.cs中三個關於FWUpdate的方法移除並修改DownloadAndInstall回傳值
        Task<List<FWUpdateInfo>> DownloadAndInstall(List<FWUpdateInfo> fwUpdateInfos, string installPath = "");

        void SetUILockStatus(bool isLockFWU_UI);

        Task<bool> GetUILockStatus();

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

        Task SupportedNKVMMonitors();

        Task<bool> GetOnNKVM(MonitorInfo monitorInfo);

        Task SetOnNKVM(MonitorInfo monitorInfo, bool ison);

        Task<bool> isNKVMSupportMonitor(MonitorInfo monitorInfo);

        Task NKVM_ChangeMonitorIndex(MonitorInfo monitorInfo);

        #endregion for NKVM

        #region public for SW Update

        Task<SWUpdateInfoPackage> SW_GetSWUpdateInfo(bool isShowNotify = true, bool isForce = false, bool isDefer = false);

        Task<List<SWUpdateInfo>> SW_DownloadAndInstall(List<SWUpdateInfo> swUpdateInfos, string installPath = "");

        #endregion public for SW Update

        #region public for ImpExpSettings
        Task<bool> DisplayExportSettings(MonitorInfo monitorInfo, string path);
        #endregion
        //public for GUI to get the changes of display and peripherals
        event EventHandler<DeviceChangedEventArgs> DeviceChanged;

        #region public for IT lock event
        //IT lock
        event EventHandler<ITSettingEventArgs> ITSettingsActionEvent;
        #endregion
        #region Gaming
        event EventHandler<GamingDisplayPropertiesInfo> GamingChangeEvent;
        Task<GamingDisplayPropertiesInfo> GetGamingProperties(MonitorInfo monitorInfo);
        Task<bool> SetGameEnhancementMode(MonitorInfo monitorInfo, Gaming_GameEnhancementMode GameEnhancementMode);
        Task<bool> SetGaming_ResponseTime(MonitorInfo monitorInfo, Gaming_ResponseTime ResponseTime);
        Task<bool> SetGaming_DarkStabilizer(MonitorInfo monitorInfo, Gaming_DarkStabilizer DarkStabilizer);
        Task<bool> SetGaming_HDRType(MonitorInfo monitorInfo, Gaming_HDRType HDRType);
        Task<bool> SetGaming_DualResolutionType(MonitorInfo monitorInfo, Gaming_DualResolutionType DualResolutionType);
        Task<bool> SetGaming_VisionEngineEnableType(MonitorInfo monitorInfo, bool[] VisionEngineEnableType);
        #endregion


        #region public for DTPProxy

        Task<int> GetDpiValueByDTP(string itemID);
        Task SetDPIValueByDTP(string itemID, int newValue);
        Task SetEraserDoublePressSetting(string itemID, byte[] newValue);
        Task SetEraserLongPressSetting(string itemID, byte[] newValue);
        Task SetEraserSinglePressSetting(string itemID, byte[] newValue);
        Task SetIsSideBottomButtonHoverClick(string itemID, bool newValue);
        Task SetIsSideTopButtonHoverClick(string itemID, bool newValue);
        Task SetMenuSinglePressSetting(string itemID, byte[] newValue);
        Task SetMenuCenterRightClickSetting(string itemID, byte[] newValue);
        Task SetSideBottomSwitchSinglePressSetting(string itemID, byte[] newValue);
        Task SetSideTopSwitchSinglePressSetting(string itemID, byte[] newValue);
        Task SetTiltSensitivity(string itemID, int newValue);
        Task SetTipSensitivity(string itemID, int newValue);

        #endregion
    }
}