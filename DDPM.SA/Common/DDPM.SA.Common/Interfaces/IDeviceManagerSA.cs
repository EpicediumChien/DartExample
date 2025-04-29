using DdmLibrary.Utility;
using DDPM.SA.Common.Defer;
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
        None = -1,
        Display_SettingsChange = 0,
        Display_PlugIn = 1,
        Display_UnPlug = 2,
        Peripherals_SettingsChange = 3,
        Peripherals_PlugIn = 4,
        Peripherals_UnPlug = 5,
        NotifyOnly = 6,
        PleaseWait = 7,
    }

    public class DeviceChangedEventArgs : EventArgs
    {
        public string deviceID { get; set; } = string.Empty;//for display point to serial number, for peripherals point to Guid
        public DeviceChangedType type { get; set; } = DeviceChangedType.None;
        public MonitorInfo device_display { get; set; }
        public DeviceInfo device_peripherals { get; set; }
        public string changedProperty { get; set; } = string.Empty;
    }

    public class UpdateUINotify : EventArgs
    {
        public string UI_Field_Name { get; set; } = string.Empty;
    }

    public class CMAIDEventArgs : EventArgs
    {
        public string deviceType { get; set; } = string.Empty;
        public string guid { get; set; } = string.Empty;
        public string snNumber { get; set; } = string.Empty;
        public string model { get; set; } = string.Empty;
        public string fwVersion { get; set; } = string.Empty;
    }

    public class UpdateDTPProxyNotify : EventArgs
    {
        public string State { get; set; } = string.Empty;
    }

    public interface IDeviceManagerSA : IFrameworkPlugin//, ISettingsManager
    {
        #region TelemetryScheduler

        Task StartTelemetrySchedulerManger(bool IsStart);

        Task<bool> ReceiveTelemetryInfo(string EventTag, string EventValue, Telementry_Frequency Frequency);

        Task<bool> SentKVMtoTelementry(MonitorInfo monitorInfo, string mode, string val);

        #endregion TelemetryScheduler

        #region EaM

        Task<Dictionary<string, InstalledAppInfo>> GetAllAppList();

        //Task<bool> LaunchAndArrangeApps(Dictionary<String, Bind_AddFullPage_AppCollectionData> sortApps);

        Task<bool> LaunchAndArrangeAppsWithEzArrange(Dictionary<String, Bind_AddFullPage_AppCollectionData> sortApps, MonitorInfo moInfo, int eAid);

        Task<bool> CheckEAIDExit(MonitorInfo moinfo, int eAID);

        Task<bool> DeleteEAID(MonitorInfo moinfo, int eAID);

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

        event EventHandler<string> NightLightStatus_ChangeEvent;

        Task<Dictionary<string, InstalledAppInfo>> FindAppsbyShell(bool isReload = false);

        void ShowOSD_ColoPreset(MonitorInfo m, string strMsg);

        Task<List<string>> ReadColorPreset(MonitorInfo m);

        //Task<bool> WriteColorPreset(MonitorInfo m, string ColorPreset_Name);
        // jim 20241207 modify for The DDPM color profile can not be applied by DDPM on Smart HDR mode.(Gaming monitor ex: AW2724DM)
        Task<bool> WriteColorPreset(MonitorInfo m, string ColorPreset_Name, int colorPresetRunType = 0, bool blIs_Game_DeviceName = false, bool blSmartHDR_ON = false, string reqAppName = null, bool showOSD = true);

        Task<bool> WriteColorPreset_AUTO(MonitorInfo m, string ColorPreset_Name);

        Task<bool> AddColorPresetForMonitorConfig(string index_monitor, string AppName, string ColorPreset_Name);

        void ChangeColorPresetForMonitorConfig(string index_monitor, string AppName, string ColorPreset_Name);

        void DeleteColorPresetForMonitorConfig(string index_monitor, string AppName);

        // jim 20241207 modify for The DDPM color profile can not be applied by DDPM on Smart HDR mode.(Gaming monitor ex: AW2724DM)
        Task<bool> AutoSetColorPresetForMonitorConfig(MonitorInfo mo, string on_off, bool Is_Game_DeviceName = false, bool Islock = false);

        Task<string> GetMonitorProfile(MonitorInfo m);

        Task<bool> SetMonitorProfile(MonitorInfo m, string ColorPreset_Name);

        Task<bool> WriteColorPresetByColorProfile(MonitorInfo m, string ColorProfile_Name);

        //Dean add 0612
        public Task<string> ReadCurrentColorPreset(MonitorInfo m, Guid guid = default, Priority priority = Priority.Low);

        //Jason add 0410
        Task<string> ReadCurrentColorPresettoVCP(MonitorInfo m, Guid guid = default, Priority priority = Priority.Low);

        //Jim add 0621
        Task<bool> Notify_refresh_app_list();

        //Jim add 20240801
        Task<IIC_Metadata> DownloadICCData(MonitorInfo m, bool blICCProfile = false, string savelPath = "");

        //Jim add 20240904
        Task<string> GetAutoColorPresetStatus(MonitorInfo m);

        //Jim add 20240905
        Task<bool> AutoColorManagementForMonitorConfig(MonitorInfo monitorInfo, string off_bymonitor_byhost, string ColorPreset_Name = "", string ICC_profile_Name = "");

        Task<string> GetColorManagementStatus(MonitorInfo m);

        Task<string> GetColorPresetName(int Color_VCPCore_E2);

        Task<int> GetColorVCPCoreValue(string ColorPreset_Name);

        Task<string> Sync_ColorPresetName(MonitorInfo monitorInfo, string ColorPreset_Name);

        Task<bool> SyncNightlightStatus();

        Task<bool> CheckNightLightStatus();

        Task<bool> CheckNightLightScheduler();

        Task<bool> CheckColorICCStatus();

        Task<bool> StopRegistryMonitor_NightLight();

        Task<bool> StopRegistryMonitor_NightLightScheduler();

        Task<bool> StopRegistryMonitor_ICC();

        Task<bool> Send_NightLightStatus_Telementry_SA(MonitorInfo m, string NightLightStatus);

        Task<bool> Send_NightLightschedulerStatus_Telementry_SA(MonitorInfo m, string NightLightStatus);

        #endregion public for ColorPreset

        #region public for Displays

        Task Reset0x52TimerTick(int millisecond, int processID = -0xFF);

        Task SetIsUserActive(bool IsUserActive);

        Task CancelVcpTask(Guid user_guid);

        Task<List<MonitorInfo>> GetMonitors();

        Task<List<MonitorInfo>> Re_GetMonitors();

        Task ReGetMonitors();

        event EventHandler<VCPchangedEventArgs> VCPchanged;

        event EventHandler<DDCCIchangedEventArgs> DDCCIStatuschanged;

        event EventHandler<DisplaychangedEventArgs> Displaychanged;

        event EventHandler<MonitorinfoUpdateEventArgs> MonitorinfoUpdated;

        Task<List<MultiCommandArch>> MultiCommandsRun(List<MultiCommandArch> _multiCommands);

        Task<string> GetCapabilitiesString(MonitorInfo monitorInfo, Guid guid = default, Priority priority = Priority.Low);

        Task<string> GetVCPCapabilities(MonitorInfo monitorInfo, Guid guid = default, Priority priority = Priority.Low);

        Task<ObjGetVCP> GetVCPCapability(MonitorInfo monitorInfo, byte code, Guid guid = default, int opt = 0, Priority priority = Priority.Low);

        Task<ObjGetVCP> GetVCPCapability(MonitorInfo monitorInfo, string FunctionName, Guid guid = default, int opt = 0, Priority priority = Priority.Low);

        Task<bool> SetVCPCapability(MonitorInfo monitorInfo, byte code, uint val, Guid guid = default, Priority priority = Priority.Low);

        Task<bool> SetVCPCapability(MonitorInfo monitorInfoX, string FunctionName, string val, Guid guid = default, Priority priority = Priority.Low);

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

        Task<string> GetUSBUpstream(MonitorInfo monitorInfo, string inputsource);

        Task<bool> SetUSBUpstream(MonitorInfo monitorInfo, string inputsource, string upstream);

        Task<bool> SetAllUSBUpstream(MonitorInfo monitorInfo, string input1, string usb1, string input2, string usb2,
                                                                    string input3 = "", string usb3 = "", string input4 = "", string usb4 = "");

        Task<bool> USBSwitch(MonitorInfo monitorInfo, string inputsource1, string upstream1, string inputsource2, string upstream2);

        Task<string> GetCurrentInput(MonitorInfo monitorInfo, Guid guid = default, Priority priority = Priority.Low);

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

        Task<bool> GetNoKVM(MonitorInfo monitorInfo);

        Task<bool> SetNoKVM(MonitorInfo monitorInfo, bool isON);

        Task<bool> isScreenPartition(MonitorInfo monitorInfo, Guid guid = default, Priority priority = Priority.Low);

        Task<Dictionary<string, PCsInfo>> ChangePC(MonitorInfo monitorInfo, Dictionary<string, PCsInfo> pcsList, List<UInt16> subInputList, bool isNext);

        #endregion public for USBKVM

        #region EasyArrange

        //Robert_Lin, 2024-12-15 rearrange order of interfaces and grouping in sub-Regions

        #region Properties - EasyArrange

        //Robert_Lin, 2024-12-15, comment-out temporary
        /// <summary>
        /// The last error string after a EAPlugin method return error.
        /// </summary>
        //public string EALastError { get; }

        #endregion Properties - EasyArrange

        #region Events - EasyArrange

        /// <summary>
        /// Notify DDPM.UI to RefreshData when EAMonitorSettings are changed (by DDPM.SA).
        /// </summary>
        /// <remarks>
        /// <para>
        /// EAArgs.Command=<b>EACommand_LastSelectedMonitorChanged</b>:<br/>
        /// * Notify EAPlugin when SelectedMonitor is changed.<br/>
        /// * New SelectedMonitor should has been updated to UserSettings.lastUISelectedMonitor.
        /// </para>
        /// </remarks>
        ///
        public event EventHandler<EAArgs> EASettingsChanged;

        /// <summary>
        /// A general event from SA to UI, using EAAgs to pass information.
        /// Callers in DDPM.SA can call <c>IDeviceManagerSA.SendEANotify()</c> to invoke this event.
        /// </summary>
        /// <remarks>
        /// Use <c>EAArgs.Command</c> to pass the Commands/Messages, which are defined in
        /// <c>DDPM.SA.Common.Display.EAEMCostants</c> class.
        /// <para>
        /// <c>EA_Command_SetIsSplanEnabled</c>:<br/>
        /// When EAPlugin detects current monitor configuration should enable or disable the
        /// "Span across multiple monitors" options in EzSettings.
        /// </para>
        /// </remarks>
        public event EventHandler<EAArgs> EANotify;

        /// <summary>
        /// Send from EAPlugin to the EAEditCommand() initiator.
        /// The event argument (string) will contain the error message, said why the
        /// EditCommand is aborted.<br/>
        /// If the argument is a String.Empty (""), then means that the command is continued.
        /// EAPlugin should has open its UI to interact with user, so DDPM.UI should minimized
        /// itself, and wait until a EditReturn event is signaled.
        /// </summary>
        public event EventHandler<string> EAEditStarted;

        /// <summary>
        /// Send from EAPlugin when the EditCommand procedure is completed, and send the result back to DDPM.UI.<br/>
        /// The returned arguments in EAArgs:<br/>
        /// <c>Command="EditReturn"</c>;<br/>
        /// <c>Return=false</c>: User cancel the edit by clicking "Cancel" button. or check <c>Message</c> for the detail.<br/>
        /// <c>Return=true</c>: User click "Save" button, and below are the key return values:<br/>
        /// <para>
        /// 1 Below arguments in <c>SplitJson</c> are not changed, will be the same with initiating EAEditCommand():<br/>
        ///   <c>CellCount</c>, and <c>SplitKey</c>.<br/>
        /// 2 <c>EAID</c>: The user selected custom layout item which would like to replace. If it's 0, then means that
        /// the layout item is edited from preset layout and user does not change from the drop-down list.<br/>
        /// 3 <c>Settings</c>: The new layout settings.<br/>
        /// 4 <c>CustomName</c>: User input Custom Name.
        /// </para>
        /// </summary>
        public event EventHandler<EAArgs> EAEditReturn;

        #endregion Events - EasyArrange

        #region EAFunctionEnabled - EasyArrange

        //Robert_Lin, 2025-1-7 EAPlugin.IsFunctionEnabled is deleted.
        // All related calling chain should be deleted, too.
        ///// <summary>
        ///// Enable/Disable EasyArrange function for all monitors.
        ///// When Disabled (isEnable=false), DDPM will not show the WorkWindow (to arrange window),
        ///// but user can edit/setup in DDPM.UI and save their settings.
        ///// </summary>
        ///// <param name="isEnabled"></param>
        ///// <returns></returns>
        //public Task<bool> SetEAFunctionEnabled(bool isEnabled);

        //public Task<ObjGetVCP> GetEAFunctionEnabled();

        #endregion EAFunctionEnabled - EasyArrange

        #region EzSettings - EasyArrange

        //Robert_Lin, 2024-9-18 added for EzSettings
        /// <summary>
        /// Read the settings in DDPM.UI Easy Arrange / Settings page. Inlcude
        /// 1. Allow app to split side by side without gap
        /// 2. Only allow zone positioning when SHIFT is pressed
        /// 3. Span across multiple monitors
        /// 4. Application Window Snap
        /// But not include "Hotkey: Recent"
        /// These settings will be loaded from UserSettings file.
        /// </summary>
        /// <returns></returns>
        public Task<EzSettings> ReadEzSettings();

        /// <summary>
        /// Write new value to EasyArrange/Settings/Allow app to split side by side without gap
        /// The new value will write to UserSettings file, and notify EAPlugin to reload
        /// new settings with IDisplayService.ReloadEzSettings()
        /// </summary>
        /// <param name="newValue"></param>
        /// <returns></returns>
        public Task<bool> WriteEzSettings_IsWidthoutGap(bool newValue);

        /// <summary>
        /// Write new value to EasyArrange/Settings/Only allow zone positioning when SHIFT is pressed
        /// The new value will write to UserSettings file, and notify EAPlugin to reload
        /// new settings with IDisplayService.ReloadEzSettings()
        /// </summary>
        /// <param name="newValue"></param>
        /// <returns></returns>
        public Task<bool> WriteEzSettings_IsOnlyAllowWhenShiftKeyPressed(bool newValue);

        /// <summary>
        /// Write new value to EasyArrange/Settings/Span across multiple monitors
        /// The new value will write to UserSettings file, and notify EAPlugin to reload
        /// new settings with IDisplayService.ReloadEzSettings()
        /// </summary>
        /// <param name="newValue"></param>
        /// <returns></returns>
        public Task<bool> WriteEzSettings_IsSpanAcrossMultiMonitors(bool newValue);

        /// <summary>
        /// Write new value to EasyArrange/Settings/Application Window Snap
        /// The new value will write to UserSettings file, and notify EAPlugin to reload
        /// new settings with IDisplayService.ReloadEzSettings()
        /// </summary>
        /// <param name="newValue"></param>
        /// <returns></returns>
        public Task<bool> WriteEzSettings_IsAwsEnabled(bool newValue);

        #endregion EzSettings - EasyArrange

        #region EA Custom List - EasyArrange

        //Robert_Lin, 2024-10-12 added, move EACustomList to UserSettings from MonitorSettings
        /// <summary>
        /// Read EasyArrange Custom layout list from UserSettings file.
        /// Implement in DeviceManagerPlugin.
        /// </summary>
        /// <returns></returns>
        public Task<SplitJson[]> ReadEACustomList();

        //Robert_Lin, 2024-10-12 added, move EACustomList to UserSettings from MonitorSettings
        /// <summary>
        /// Write EasyArrange Custom layout list to UserSettings file.
        /// Implement in DeviceManagerPlugin.
        /// It's called by DDPM.UI. It's no need to notify SA EAPlugin.
        /// </summary>
        /// <returns></returns>
        public Task<bool> WriteEACustomList(SplitJson[] customList);

        #endregion EA Custom List - EasyArrange

        #region EAMonitorSettings - EasyArrange

        /// <summary>
        /// Write new values to EA MonitorSettings file. A basic write file function, no any notification support.
        /// </summary>
        /// <param name="monitorInfo"></param>
        /// <param name="eaSettings"></param>
        /// <returns></returns>
        public Task<bool> WriteEAMonitorSettings(MonitorInfo monitorInfo, EAMonitorSettings eaSettings);

        /// <summary>
        /// Read setting values from EA MonitorSettings file.
        /// </summary>
        /// <param name="monitorInfo"></param>
        /// <param name="eaSettings"></param>
        /// <returns></returns>
        public Task<EAMonitorSettings> ReadEAMonitorSettings(MonitorInfo monitorInfo);

        #endregion EAMonitorSettings - EasyArrange

        #region SelectedLayout - EasyArrange

        /// <summary>
        /// Return the EAID of current selected layout. Called by CLI.
        /// UI/SA should call ReadEAMonitorSettings() to get the full properties of SelectedLayout.
        /// </summary>
        /// <param name="monitorInfo">Specified the monitor</param>
        /// <returns>
        /// <b>0</b>=Off (Empty Layout), <b>[1~49]</b>=Preset layout,
        /// <b>[1000~1004]</b>=Custom layout, <b>others</b>(shold be a negavtive value)=error
        /// </returns>
        public Task<int> GetEASelectedLayout(MonitorInfo monitorInfo);

        /// <summary>
        /// Set Selected EA Layout by EAID (Robert_Lin, 2024-12-13, wait for CLI verification)
        /// Fully simulate the secnario that user select a layout from DDPM UI. with below steps<br/>
        /// 1. Set the specified layout (by EAID) as selected layout.<br/>
        /// 2. (if not exist then) Add to Recent list.<br/>
        /// 3. Save the changed to EAMonitorSettings.<br/>
        /// 4. Notify SA.EAPlugin (EABroker) to update/refresh.<br/>
        /// 5. User will see the selected layout shown and auto fade-out animation.<br/>
        /// 6. Notify UI to reload settings.<br/>
        /// </summary>
        /// <param name="monitorInfo">It can set to null, if eaId>=1000. </param>
        /// <param name="eaId">0=Off, [1~49]=Preset layout, [1000~1004]=Custom Layout.</param>
        /// <returns></returns>
        public Task<bool> SetEASelectedLayout(MonitorInfo monitorInfo, int eaId);

        /// <summary>
        /// The major method for DDPM.UI to notify SA.EAPlugin to refresh the SelectedLayout.
        /// The selection will not be saved to settings file with this method.
        /// </summary>
        /// <param name="monitorInfo"></param>
        /// <param name="spJson"></param>
        /// <returns></returns>
        public Task<bool> NotifyEASelectedLayoutChanged(MonitorInfo monitorInfo, SplitJson spJson);

        /// <summary>
        /// [OLD, Use NotifyEASelectedLayoutChanged() instead]
        /// Set the SelectedLayout from UI to EAPlugin, apply to all of related runtime objects.
        /// The new settings will be save to Settings file by UI with WriteEAMonitorSettings(),
        /// Not included in this method.
        /// </summary>
        /// <param name="monitorInfo"></param>
        /// <param name="cellCount"></param>
        /// <param name="splitKey"></param>
        /// <param name="settings"></param>
        /// <returns></returns>
        public Task<bool> SetEAWrokSplit(MonitorInfo monitorInfo, int cellCount, char splitKey, List<double>? settings);

        /// <summary>
        /// [OLD, Use SetEASelectedLayout(MonitorInfo monitorInfo, int eaId) instead]
        /// </summary>
        /// <param name="monitorInfo"></param>
        /// <param name="spJson"></param>
        /// <returns></returns>
        public Task<bool> SetEASelectedLayout(MonitorInfo monitorInfo, SplitJson spJson);

        #endregion SelectedLayout - EasyArrange

        #region EditCommand - EasyArrange

        //Robert_Lin, 2024-8-4 new added
        /// <summary>
        /// Initiate a EditCommand to EAPlugin from DDPM.UI.
        /// </summary>
        /// <param name="monitorInfo">The MonitorInfo of the monitor</param>
        /// <param name="args">
        /// <c>args.Command</c>="EditCommand"<br/>
        /// <c>args.SplitJson.CellCount,SplitKey,Settings</c>=The ISplitCtrl settings to be edited.<br/>
        /// <c>args.SplitJson.EAID</c>=The original EAID which is selected for editing.<br/>
        /// For example, if the edit is initiated by clicking the pencil icon of a preset layout, then EAID=0.<br/>
        /// But if it's initiated from a custom layout, then EAID will be in range of [1000~10004].
        /// </param>
        /// <returns>
        /// <c>true</c>: EAPlugin has accept the EditCommand, and initiate an internal Edit Procedure.
        /// The caller (DDPM.UI) should wait for a EditStarted event.<br/>
        /// <c>false</c>: The EditCommand is rejected by EAPlugin.
        /// </returns>
        public Task<bool> EAEditCommand(MonitorInfo monitorInfo, EAArgs args);

        #endregion EditCommand - EasyArrange

        #region SendEANotify -EasyArrange

        /// <summary>
        /// General notification  to EAPlugin from other Plugins inside DDPM.SA.User
        /// </summary>
        /// <param name="args">
        /// eaArgs.Command=EAEMConstants.EACommand_LastSelectedMonitorChanged:
        ///     Notify EAPlugin when SelectedMonitor is changed,
        /// </param>
        /// <returns></returns>
        public Task SendEANotify(EAArgs args);

        #endregion SendEANotify -EasyArrange

        #region Span across multiple monitors - EasyArrange

        /// <summary>
        /// Return current Span across multiple monitor option is Enabled/Disabled;
        /// Note that it's different with EzSettings.IsSpanAcrossMultiMonitors (=ON|OFF)
        /// </summary>
        /// <returns>True=Enabled; False=Disabled</returns>
        public Task<bool> GetIsSpanEnabled();

        #endregion Span across multiple monitors - EasyArrange

        #endregion EasyArrange

        #region EasyMemory

        public Task<bool> WriteMonitorEasyArrangement(MonitorInfo monitorInfo, EasyArrangementDDPM easyArrangementDDPM);

        public Task<bool> UpdateMonitorEzProfileSettingDDPM(MonitorInfo monitorInfo, EzProfileSettingDDPM profileSettingDDPM);

        public Task<EasyArrangementDDPM> ReadMonitorEasyArrangement(MonitorInfo monitorInfo);

        public Task<bool> WriteUserListEAProfileDDPM(List<EAProfileDDPM> eaProfileList);

        public Task<bool> WriteUserEAProfileDDPM(MonitorInfo monitorInfo, EAProfileDDPM eaProfile);

        public Task<bool> UpdateUserEAProfileDDPM(MonitorInfo monitorInfo, EAProfileDDPM eaProfile);

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

        Task SetWiredAudioIMicNSEnable(bool newValue, Guid deviceId);

        Task SetWiredAudioMicMuteSoundEnable(bool newValue, Guid deviceId);

        Task SetWiredAudioVolumeAdjustmentTone(int newValue, Guid deviceId);

        Task SetAncMode(int newValue, Guid deviceId);

        Task SetAncGain(int newValue, Guid deviceId);

        Task SetSelectedPreset(int newValue, Guid deviceId);

        Task SetBandsGain(int newValue, Guid deviceId, string bandGainNumber);

        Task SetMicNoiseCancellation(bool newValue, Guid deviceId);

        Task SetMicNoiseCancellationForMito(bool newValue, Guid deviceId);

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

        //Task SetIsProximitySensorEnable(bool newValue, Guid deviceId);

        Task SetIsWakeonApproachEnable(bool newValue, Guid deviceId);

        Task SetIsWalkAwayLockEnable(bool newValue, Guid deviceId);

        Task<int> GetSnooze(Guid deviceId);

        Task<int> GetSnoozeLength(Guid deviceId);

        Task<bool> StartCopilotRegistryMonitor();

        Task<bool> StopCopilotRegistryMonitor();

        Task<int> GetIODongleCountGen3AgoCount();

        #endregion public for Peripherals

        #region public for CMA/CLI

        //Task<CommandOutput_DeviceConnection> queryConnectedDeviceInfo(CommandInput_notifyDeviceConnection input);
        //Task<List<CommandResult>> listConnectedDeviceInfo();

        event EventHandler<UpdateUINotify> UIUpdateNotify;

        void OnUIUpdateNotify(UpdateUINotify e);

        event EventHandler<CMAIDEventArgs> DTPEventForCMAChanged;


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

        Task<bool> CheckInstallFirstOpen();

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

        public Task<bool> SaveHotkeyOptionOnly(MonitorInfo mo, HotkeyOption hotkeyOption);

        public Task<HotkeyOption> ReadHotkeyOption(MonitorInfo mo);

        public Task SetLastSelectedMonitorFromUI(MonitorInfo mo);

        public Task<bool> ByPassHotkey(bool bypass);

        public Task<bool> UnRegistAllHotkey();

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

        Task<FWUpdateInfoPackage> GetFWUpdateInfo(bool reScan);

        Task<FWUErrorCode> Install(string installPath, bool isOnlyDisplay = false, DeviceType deviceType = DeviceType.Unknown);

        //0531 Bruce 因應IL的現有安裝包修改判斷，IDeviceManagerSA.cs中三個關於FWUpdate的方法移除並修改DownloadAndInstall回傳值
        Task<List<FWUpdateInfo>> DownloadAndInstall(List<FWUpdateInfo> fwUpdateInfos, bool isUITrigger = false, bool isShowNotify = false, string installPath = "");

        void SetUILockStatus(bool isLockFWU_UI);

        Task<bool> GetUILockStatus();

        Task<bool> SetSkipCA(bool isSkipCA);

        Task<bool> GetSkipCA();

        Task<bool> SetServerURL(string url);

        Task<string> GetServerURL();

        Task<bool> CallDDPMUI(string DDPMPath);

        #endregion public for FW Update by Bruce

        #region public ALS functions

        Task<ALSConfig> GetALSFeatureValue(MonitorInfo monitorInfos, ALSFeatureQueryType type, int value);

        Task<bool> SetALSFeatureValue(MonitorInfo monitorInfos, ALSConfig param, ALSFeatureQueryType type, string value);

        Task<List<ALSConfig>> GetConnectedALSConfig();

        Task<List<ALSConfig>> GetAllExistAlsConfig();

        Task<List<ALSConfig>> UpdateExistAlsConfig(List<MonitorInfo> monitorInfoMain);

        Task<bool> SynchronizeALSFeatureValue(ALSConfig monitorALS);

        Task<String> CheckisShowSynchronize(MonitorInfo currentMoInfo, List<ALSConfig> alsSynchronizeList);

        Task<bool> CheckIsSyncBriCon(MonitorInfo SourceMonitor, MonitorInfo TargetMonitor);

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

        Task CallShowNKVM(int num, int x, int y);

        #endregion for NKVM

        #region public for SW Update

        Task<SWUpdateInfoPackage> SW_GetSWUpdateInfo(bool isShowNotify, bool reScan = true);

        Task<List<SWUpdateInfo>> SW_DownloadAndInstall(List<SWUpdateInfo> swUpdateInfos, bool isUITrigger = false, string installPath = "");

        Task<InterruptScreenRoot> InterruptScreen_Metadata();

        #endregion public for SW Update

        #region public for ImpExpSettings

        Task<bool> DisplayExportSettings(MonitorInfo monitorInfo, string path);

        Task<DisplayImportResultCode> DisplayImportSettings(MonitorInfo monitorInfo, bool isSameModel, string path);

        Task SetSameModel(MonitorInfo monitorInfo, bool isSameModel);

        Task<bool> GetSameModel(MonitorInfo monitorInfo);

        Task<DDPMImpExpSettings> ReadImportSettingsFile(string path);

        /// <summary>
        /// For auto import to read if we need to skip notification
        /// </summary>
        /// <param name="path"></param>
        /// <param name="modelName"></param>
        /// <returns>Boolean</returns>
        Task<bool> ReadSameModelAutoApplySameModelFlag(string path, string modelName);

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

        Task<JArray> GetMouseAssignedActions(string Guid);

        Task<string> GetMouseKeystrokeDisplayData(string Guid);

        Task<bool> StartMouseKeystrokeRecording(string Guid);

        Task<bool> StopMouseKeystrokeRecording(string Guid);

        Task<int> GetTouchScrollSensitivityLevel(string Guid);

        Task SetDPIValue(string Guid, int newValue);

        Task SetMouseAction(string Guid, byte[] newValue);

        Task SetCurrentSelectedAppSpecificProfile(string Guid, string newValue);

        Task DeleteMouseAssignedAction(string Guid, int newValue);

        Task SetMouseAssignDialogAction(string Guid, byte[] newValue);

        Task SetMouseAssignKeystrokeAction(string Guid, byte[] newValue);

        Task<bool> RestoreToDefaultMouse(string Guid, bool isFromCli = true);

        Task<bool> SetReportRate(string Guid, int newValue);

        Task<bool> SetTouchScrollSensitivityLevel(string Guid, int newValue);

        #endregion Mouse

        #region Keyboard

        Task<JArray> GetKeyboardDeviceItemsEx();

        Task<JArray> GetKbProgrammableKeys(string Guid);

        Task<bool> DeleteKeyboardAllAssignedActions(string Guid);

        Task<JArray> GetKbAssignableActions(string Guid);

        Task<JArray> GetKbAssignedActions(string Guid);

        Task<string> GetKeyboardKeystrokeDisplayData(string Guid);

        Task<bool> StartKeyboardKeystrokeRecording(string Guid);

        Task<bool> StopKeyboardKeystrokeRecording(string Guid);

        Task DeleteKeyboardAssignedAction(string Guid, int newValue);

        Task SetKbAssignedAction(string Guid, byte[] newValue);

        Task SetKbAssignDialogAction(string Guid, byte[] newValue);

        Task SetKbAssignKeystrokeAction(string Guid, byte[] newValue);

        Task<bool> RestoreToDefaultKB(string Guid);

        #endregion Keyboard

        #region Pen

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

        Task<JArray> GetPenDeviceItemsEx();

        Task<bool> StartKeyCapturePen();

        Task<bool> FinishKeyCapturePen();

        Task<string> KeyCaptureData();

        Task<string> GetIsdDriverVersion();

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

        Task<bool> RestoreToDefaultPen();

        Task<bool> RestoreRadialMenuToDefault();

        #endregion Pen

        #region Webcam

        //event EventHandler<bool>? Esi_IsCameraSensorCover_ChangeEvent;
        //event EventHandler<int>? WALSnoozeTimeLeftInSeconds_ChangeEvent;
        //event EventHandler<bool>? Esi_IsWALLockCountdownStartedChanged_ChangeEvent;
        //event EventHandler<int>? Esi_WALLockCountdownChanged_ChangeEvent;

        Task<JArray> GetPresetProfiles(string Guid);

        Task<JArray> GetCustomProfiles(string Guid);

        Task<string> GetProfile(string Guid);

        Task<string> GetProfileName(string Guid);

        Task<int> GetBrightness(string Guid);

        Task<string> GetCameraFirmwareVersionByDTP(string Guid);

        Task<bool> GetIsPropertyFOVSupportedByDTP(string Guid);

        Task<int> GetFieldOfView(string Guid);

        Task<bool> GetIsWindowsHelloCapabilityVerified(string Guid);

        Task<bool> GetIsAllSupportedResolutionsFound(string Guid);

        Task<bool> GetIsPropertyHDRSupported(string Guid);

        Task<bool> GetIsHDROn(string Guid);

        Task<bool> GetIsPropertyAntiFlickerSupported(string Guid);

        Task<int> GetAntiFlicker(string Guid);

        Task<bool> GetIsPropertyAutoFramingSupported(string Guid);

        Task<bool> GetIsAutoFramingOn(string Guid);

        Task<string> GetSupportedResolutions(string Guid);

        Task<string> GetSelectedResolution(string Guid);

        Task<int> GetZoom(string Guid);

        Task<int> GetFocus(string Guid);

        Task<bool?> GetIsFocusOn(string Guid);

        Task<int> GetPriority(string Guid);

        Task<bool?> GetIsAutoFramingTransitionOn(string Guid);

        Task<int> GetAutoFramingFrameSize(string Guid);

        Task<int> GetAutoFramingSensitivity(string Guid);

        Task<string> GetWebcamSerialNumber(string Guid);

        Task<int> GetBgBlur(string Guid);

        Task<bool> GetIsBgBlurEnable(string Guid);

        Task<bool> GetIsPropertyBgBlurSupported(string Guid);

        Task SetIsMicEnumerationOn(string Guid, bool newValue);

        Task SetProfile(string Guid, string newValue);

        Task SetProfileName(string Guid, string newValue);

        Task CreateCustomProfile(string Guid, string newValue);

        Task DeleteProfile(string Guid, string newValue);

        Task<bool> SetZoom(string Guid, int newValue);

        Task<bool> SetIsAutoFramingOn(string Guid, bool newValue);

        Task<bool> SetIsAutoFramingTransitionOn(string Guid, bool newValue);

        Task<bool> SetAutoFramingSensitivity(string Guid, int newValue);

        Task<bool> SetAutoFramingFrameSize(string Guid, int newValue);

        Task<bool> SetFieldOfView(string Guid, int newValue);

        Task<bool> SetIsFocusOn(string Guid, bool newValue);

        Task<bool> SetFocus(string Guid, int newValue);

        Task<bool> SetPriority(string Guid, int newValue);

        Task<bool> SetIsHDROn(string Guid, bool newValue);

        Task<bool> SetIsAutoWhiteBalanceOn(string Guid, bool newValue);

        Task<bool> SetAutoWhiteBalance(string Guid, int newValue);

        Task<bool> SetBrightness(string Guid, int newValue);

        Task<bool> SetSharpness(string Guid, int newValue);

        Task<bool> SetContrast(string Guid, int newValue);

        Task<bool> SetSaturation(string Guid, int newValue);

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

        Task<bool?> GetIsPrioritizeExternalWebcam(string Guid);

        Task<bool> GetIsESISupported(string Guid);

        Task<bool> SetIsBgBlurEnable(string Guid, bool newValue);

        Task<bool> SetBgBlur(string Guid, int newValue);

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

        Task<bool> SetBand1GainAsync(string Guid, int newValue);

        Task<bool> SetBand2GainAsync(string Guid, int newValue);

        Task<bool> SetBand3GainAsync(string Guid, int newValue);

        Task<bool> SetBand4GainAsync(string Guid, int newValue);

        Task<bool> SetBand5GainAsync(string Guid, int newValue);

        Task<bool> SetAncModeAsync(string Guid, int newValue);

        Task<bool> SetAncGainAsync(string Guid, int newValue);

        Task<bool> SetWearDetectionAsync(string Guid, bool newValue);

        Task<bool> SetIsWearDetectionMuteMicEnabledAsync(string Guid, bool newValue);

        Task<bool> SetIsWearDetectionPauseMusicEnabledAsync(string Guid, bool newValue);

        Task<bool> SetWearDetectionQuickPauseAsync(string Guid, int newValue);

        Task<bool> SetWearDetectionSensitivityAsync(string Guid, int newValue);

        Task<bool> SetMicNCIncomingAsync(string Guid, bool newValue);

        Task<bool> SetUnPairAsync(string Guid, bool newValue);

        Task<bool> SetFactoryResetAsyncValueForHeadset(string Guid, bool newValue);

        Task<bool> SetFactoryResetAsyncValueForHeadsetForCLI(string Guid, bool newValue);

        Task<bool> SetBoomMicAsync(string Guid, bool newValue);

        #endregion Headset Set

        #region Headset Get

        //Peripheral Common Properties Get
        Task<JArray> GetHeadsetDeviceItemsExAsync();

        Task<DeviceInterfaceType> GetHeadsetInterfaceTypeAsync(string Guid);

        Task<string> GetHeadsetDeviceNameAsync(string Guid);

        Task<string> GetHeadsetDeviceIdAsync(string Guid);

        Task<string> GetHeadsetPluginIdAsync(string Guid);

        Task<int> GetHeadsetODMIdAsync(string Guid);

        Task<string> GetHeadsetModelNumberAsync(string Guid);

        Task<int> GetHeadsetInstanceNumberAsync(string Guid);

        Task<int> GetHeadsetInstanceIdAsync(string Guid);

        Task<string> GetHeadsetFirmwareVersionAsync(string Guid);

        Task<string> GetHeadsetDeviceTypeAsync(string Guid);

        //Headset Get
        Task<string> GetHeadsetParentDeviceTypeAsync(string Guid);

        Task<bool> GetHeadsetIsBatteryLevelSupportedAsync(string Guid);

        Task<int> GetHeadsetBatteryLevelAsync(string Guid);

        Task<string> GetHeadsetDeviceBatteryStatusAsync(string Guid);

        Task<string> GetHeadsetPairingStatusAsync(string Guid);

        Task<string> GetHeadsetPairedHostName1Async(string Guid);

        Task<string> GetHeadsetPairedHostName2Async(string Guid);

        Task<string> GetHeadsetPairedHostName3Async(string Guid);

        Task<int> GetHeadsetMaxPairingSlotsAsync(string Guid);

        Task<int> GetHeadsetPairedDeviceCountAsync(string Guid);

        Task<int> GetHeadsetTotalNumberOfPairedHostNameAsync(string Guid);

        Task<string> GetHeadsetSerialNumberAsync(string Guid);

        Task<bool> GetIsReadyAsync(string Guid);

        Task<bool> GetIsDirtyAsync(string Guid);

        Task<bool> GetIsMicNoiseCancellationSupportedAsync(string Guid);

        Task<bool> GetIsSidetoneSupportedAsync(string Guid);

        Task<bool> GetIsBusyLightSupportedAsync(string Guid);

        Task<bool> GetIsVoiceGuidanceSupportedAsync(string Guid);

        Task<bool> GetIsPresetsSupportedAsync(string Guid);

        Task<bool> GetIsEqualizerSupportedAsync(string Guid);

        Task<HeadsetConnectionType> GetConnectionTypeAsync(string Guid);

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

        Task<int> GetBand1GainAsync(string Guid);

        Task<int> GetBand2GainAsync(string Guid);

        Task<int> GetBand3GainAsync(string Guid);

        Task<int> GetBand4GainAsync(string Guid);

        Task<int> GetBand5GainAsync(string Guid);

        Task<int> GetAncModeAsync(string Guid);

        Task<int> GetAncGainAsync(string Guid);

        Task<bool> GetWearDetectionAsync(string Guid);

        Task<bool> GetIsWearDetectionPauseMusicEnabledAsync(string Guid);

        Task<bool> GetIsWearDetectionMuteMicEnabledAsync(string Guid);

        Task<int> GetWearDetectionSensitivityAsync(string Guid);

        Task<int> GetWearDetectionQuickPauseAsync(string Guid);

        Task<bool> GetIsMicNCIncomingSupportedAsync(string Guid);

        Task<bool> GetIsBoomMicSupportedAsync(string Guid);

        Task<bool> GetBoomMicAsync(string Guid);

        #endregion Headset Get

        #endregion Headset

        #region Wires Audio

        Task<bool> SetBassAsync(string Guid, int newValue);

        Task<bool> SetMidRangeAsync(string Guid, int newValue);

        Task<bool> SetTrebleAsync(string Guid, int newValue);

        Task<bool> SetProfileForSpeaker(string Guid, string newValue);

        Task<bool> SetIsWiredAudioMicMuteSoundEnableAsync(string Guid, bool newValue);

        Task<bool> SetWiredAudioVolumeAdjustmentToneAsync(string Guid, int newValue);

        Task<bool> SetIsWiredAudioIMicNSEnableAsync(string Guid, bool newValue);

        Task<bool> SetResetToDefaultAsyncForSoundbar(string Guid, bool newValue);

        ////////////////////////////////Get////////////////////////////////

        Task<string> GetWiredAudioSerialNumberAsync(string item);

        Task<string> GetProfileNameAsync(string item);

        Task<string> GetProfileAsync(string item);

        Task<int> GetBassAsync(string Guid);

        Task<int> GetMidRangeAsync(string Guid);

        Task<int> GetTrebleAsync(string Guid);

        Task<bool> GetIsWiredAudioMicMuteSoundEnableAsync(string Guid);

        Task<int> GetWiredAudioVolumeAdjustmentToneAsync(string Guid);

        Task<bool> GetIsWiredAudioIMicNSEnableAsync(string Guid);

        Task<bool> GetIsAudioEqualizerSupportedAsync(string Guid);

        Task<bool> GetMuteStatusAsyncForSpeaker(string guid);

        Task<bool> GetIsIMicNSSupportedAsync(string Guid);

        Task<bool> GetIsVolumeAdjustmentToneSupportedAsync(string Guid);

        Task<bool> GetIsMicMuteSoundSupportedAsync(string Guid);

        Task<bool> GetPresetProfilesAsync(string Guid);

        Task<bool> GetIsBassEqualizerSupportedAsync(string Guid);

        Task<bool> GetIsMidRangeEqualizerSupportedAsync(string Guid);

        Task<bool> GetIsTrebleEqualizerSupportedAsync(string Guid);

        #endregion Wires Audio

        #region Dongle

        Task<string> GetFirmwareVersionAsyncForDongle(string Guid);

        Task<string> GetConnectedDeviceInfoAsyncForDongle(string Guid);

        Task<string> GetDeviceIdAsyncForDongle(string Guid);

        Task<string> GetPluginIdAsyncForDongle(string Guid);

        Task<JArray> GetDeviceItemsExAsyncForDongle(string Guid);

        #endregion Dongle

        #region Dock

        Task<DockData> GetDockData(string guid);

        Task<string> GetDockServiceTagForDock(string Guid);

        #endregion Dock

        #endregion public for DTPProxy

        #region OSD

        Task ShowOSD(object monitorInfo, OSDType type, OSDType_Device Device, string Content, Guid guid = default);

        Task ShowOSD(object monitorInfo, OSDType type, string Content, bool State);

        Task ShowOSD(object monitorInfo, OSDType type, bool State);

        Task ShowOSD(object monitorInfo, OSDType type);

        Task ShowOSD(object monitorInfo, OSDType type, bool State, (string, string, bool) args);

        #endregion OSD

        #region GlobalSetting

        //Derek 1209
        //event EventHandler GlobalSettingChangeEvent;
        event EventHandler<UpdateUINotify> GlobalSettingChangeEvent;

        Task InvokeGlobalSettingChangeUINotify(UpdateUINotify e);

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

        Task<bool> SaveLogFile(string saveFolderPath);

        #endregion OutReport

        //For common json file read/write
        Task<string> ReadSerializedContentFromFile(string filePath);

        Task<bool> WriteSerializedContentToFile(string filePath, string content);

        // add @ 20241202 stephen
        void updateFWUpdateInfoPackage(FWUpdateInfoPackage pkg);

        #endregion GlobalSetting

        #region QAM

        Task SetIsDDPMLaunchByQAMAsync(bool newValue);

        Task<bool> GetIsDDPMLaunchByQAM();

        Task SetIsDDPMHomepageReadyAsync(bool newValue);

        Task<int> GetCurrentPollingRate();

        Task CloseQAMByDDPM();

        Task<bool> GetIsWidgetSettingPageLoadedByQAMAsync();

        Task SetIsWidgetSettingPageLoadedByQAMAsync(bool newValue);

        Task SyncWebcamProfile(string profileName, bool isActionFromQAM = true); //Derek 1212

        Task WriteLog(string logMsg); //Derek 1210

        Task<string> GetWebcamDeviceID(); //Derek 1225

        Task<int> GetWebcamDeviceCountAsync(); //Derek 2025/02/19

        Task SetQAMOSDVisable(bool visable); //Derek 2025/01/02

        #endregion QAM

        #region System Suspend & Resume & SessionEnd

        event EventHandler SystemSuspend;

        event EventHandler SystemResume;

        event EventHandler SystemSessionEnd;

        Task FireSystemSessionEnd();

        #endregion System Suspend & Resume & SessionEnd

        #region globalperipheral

        Task<bool> GetIsLockKeyNotificationsEnabledValue();

        Task<bool> GetIsBatteryNotificationsEnabledValue();

        Task<bool> GetIsPresenceDetectionSensnorStateNotificationsEnabledValue();

        Task<bool> GetIsAnalyticsEnabledValue();

        Task<bool> GetIsQuickAccessMenuEnabledValue();

        Task<bool> GetIsMuteStatusNotificationsEnabledValue();

        Task<bool> GetIsQuickAccessMenuOSDEnabledValue();

        Task<bool> SetIsLockKeyNotificationsEnabledValue(bool newValue);

        Task<bool> SetIsBatteryNotificationsEnabledValue(bool newValue);

        Task<bool> SetIsPresenceDetectionSensnorStateNotificationsEnabledValue(bool newValue);

        Task<bool> SetIsAnalyticsEnabledValue(bool newValue);

        Task<bool> SetIsQuickAccessMenuEnabledValue(bool newValue);

        Task<bool> SetIsMuteStatusNotificationsEnabledValue(bool newValue);

        Task<bool> SetIsQuickAccessMenuOSDEnabledValue(bool newValue);

        Task<bool> checkDeviceConnStatus(FwRule rule);    //add @ 20250116 stephen

        #endregion globalperipheral

        #region IAirAudioCommodity

        Task<bool> GetDTPProxyPluginReady();

        #region Get

        Task<HeadsetConnectionType> GetAirAudioConnectionTypeAsync(string Guid);

        Task<JArray> GetAirAudioDeviceItemsAsync();

        Task<string> GetAirAudioSerialNumberAsync(string Guid);

        Task<string> GetAirAudioDeviceBatteryStatusAsync(string Guid);

        Task<string> GetAirAudioPairingHostName1Async(string Guid);

        Task<string> GetAirAudioPairingHostName2Async(string Guid);

        Task<string> GetAirAudioPairingHostName3Async(string Guid);

        Task<string> GetAirAudioPairingStatusNameAsync(string Guid);

        Task<string> GetAirAudioParentDeviceTypeAsync(string Guid);

        Task<string> GetAirAudioModelNumberAsync(string Guid);

        Task<string> GetAirAudioDeviceTypeAsync(string Guid);

        Task<string> GetAirAudioFirmwareVersionAsync(string Guid);

        Task<string> GetAirAudioPluginIdAsync(string Guid);

        Task<string> GetAirAudioDeviceIdAsync(string Guid);

        Task<string> GetAirAudioDeviceNameAsync(string Guid);

        Task<string> GetAirAudioSerialNumberCaseAsync(string Guid);

        Task<string> GetAirAudioBatteryStatusLeftAsync(string Guid);

        Task<string> GetAirAudioBatteryStatusRightAsync(string Guid);

        Task<string> GetAirAudioBatteryStatusCaseAsync(string Guid);

        Task<DeviceInterfaceType> GetAirAudioDeviceInterfaceTypeAsync(string Guid);

        //Task<bool> GetAirAudioIsWearDetectionAsync(string Guid);
        Task<bool> GetAirAudioIsAutoPowerOffEnabledAsync(string Guid);

        Task<bool> GetAirAudioMuteStatusAsync(string Guid);

        Task<bool> GetAirAudioBoomMicAsync(string Guid);

        Task<bool> GetAirAudioIsBoomMicSupportedAsync(string Guid);

        Task<bool> GetAirAudioWearDetectionAsync(string Guid);

        Task<bool> GetAirAudioVoiceGuidanceAsync(string Guid);

        Task<bool> GetAirAudioBusyLightAsync(string Guid);

        Task<bool> GetAirAudioSidetoneAsync(string Guid);

        Task<bool> GetAirAudioMicNCIncomingAsync(string Guid);

        Task<bool> GetAirAudioIsMicNCIncomingSupportedAsync(string Guid);

        Task<bool> GetAirAudioMicNoiseCancellationAsync(string Guid);

        Task<int> GetAirAudioWearDetectionQuickPauseAsync(string Guid);

        Task<bool> GetAirAudioIsWearDetectionMuteMicSupportedAsync(string Guid);

        Task<bool> GetAirAudioIsWearDetectionPauseMusicSupportedAsync(string Guid);

        Task<bool> GetAirAudioIsWearDetectionSensitivitySupportedAsync(string Guid);

        Task<bool> GetAirAudioIsWearDetectionSupportedAsync(string Guid);

        Task<bool> GetAirAudioIsANCSupportedAsync(string Guid);

        Task<bool> GetAirAudioIsEqualizerSupportedAsync(string Guid);

        Task<bool> GetAirAudioIsPresetsSupportedAsync(string Guid);

        Task<bool> GetAirAudioIsVoiceGuidanceSupportedAsync(string Guid);

        Task<bool> GetAirAudioIsBusyLightSupportedAsync(string Guid);

        Task<bool> GetAirAudioIsSidetoneSupportedAsync(string Guid);

        Task<bool> GetAirAudioIsMicNoiseCancellationSupportedAsync(string Guid);

        Task<bool> GetAirAudioIsDirtyAsync(string Guid);

        Task<bool> GetAirAudioIsReadyAsync(string Guid);

        Task<bool> GetAirAudioIsWearDetectionPauseMusicEnabledAsync(string Guid);

        Task<bool> GetAirAudioIsWearDetectionMuteMicEnabledAsync(string Guid);

        Task<bool> GetAirAudioIsWearDetectionAnswerCallsEnabledAsync(string Guid);

        Task<bool> GetAirAudioIsBatteryLevelSupportedAsync(string Guid);

        Task<int> GetAirAudioWearDetectionSensitivityAsync(string Guid);

        Task<int> GetAirAudioAutoPowerOffIntervalAsync(string Guid);

        Task<int> GetAirAudioIsWearDetectionQuickPauseAsync(string Guid);

        Task<int> GetAirAudioAncGainAsync(string Guid);

        Task<int> GetAirAudioAncModeAsync(string Guid);

        Task<int> GetAirAudioBand1GainAsync(string Guid);

        Task<int> GetAirAudioBand2GainAsync(string Guid);

        Task<int> GetAirAudioBand3GainAsync(string Guid);

        Task<int> GetAirAudioBand4GainAsync(string Guid);

        Task<int> GetAirAudioBand5GainAsync(string Guid);

        Task<int> GetAirAudioSidetoneLevelAsync(string Guid);

        Task<int> GetAirAudioSelectedPresetAsync(string Guid);

        Task<int> GetAirAudioBatteryLevelAsync(string Guid);

        Task<int> GetAirAudioPairedDeviceCountAsync(string Guid);

        Task<int> GetAirAudioMaxPairingSlotsAsync(string Guid);

        Task<int> GetAirAudioTotalNumberOfPairedHostNameAsync(string Guid);

        Task<int> GetAirAudioInstanceIdAsync(string Guid);

        Task<int> GetAirAudioInstanceNumberAsync(string Guid);

        Task<int> GetAirAudioODMIdAsync(string Guid);

        Task<int> GetAirAudioBatteryLevelLeftAsync(string Guid);

        Task<int> GetAirAudioBatteryLevelRightAsync(string Guid);

        Task<int> GetAirAudioBatteryLevelCaseAsync(string Guid);

        Task<int> GetAirAudioMaxAllowedPariedHost(string Guid);

        Task<bool> GetAirAudioIsConnectedAsync(string Guid);

        Task<bool> GetAirAudioIsConnectedLeftAsync(string Guid);

        Task<bool> GetAirAudioIsConnectedRightAsync(string Guid);

        #endregion Get

        #region Set

        Task<bool> SetAirAudioMicNoiseCancellationAsync(string Guid, bool newValue);

        Task<bool> SetAirAudioSidetoneAsync(string Guid, bool newValue);

        Task<bool> SetAirAudioBusyLightAsync(string Guid, bool newValue);

        Task<bool> SetAirAudioVoiceGuidanceAsync(string Guid, bool newValue);

        Task<bool> SetAirAudioSelectedPresetAsync(string Guid, int newValue);

        Task<bool> SetAirAudioSidetoneLevelAsync(string Guid, int newValue);

        Task<bool> SetAirAudioBandsGainAsync(string Guid, byte[] newValue);

        Task<bool> SetAirAudioBand1GainAsync(string Guid, int newValue);

        Task<bool> SetAirAudioBand2GainAsync(string Guid, int newValue);

        Task<bool> SetAirAudioBand3GainAsync(string Guid, int newValue);

        Task<bool> SetAirAudioBand4GainAsync(string Guid, int newValue);

        Task<bool> SetAirAudioBand5GainAsync(string Guid, int newValue);

        Task<bool> SetAirAudioAncModeAsync(string Guid, int newValue);

        Task<bool> SetAirAudioAncGainAsync(string Guid, int newValue);

        Task<bool> SetAirAudioWearDetectionAsync(string Guid, bool newValue);

        Task<bool> SetAirAudioFactoryResetAsync(string Guid, bool newValue);

        Task<bool> SetAirAudioIsBoomMicSupportedAsync(string Guid, bool newValue);

        Task<bool> SetAirAudioWearDetectionQuickPauseAsync(string Guid, int newValue);

        Task<bool> SetAirAudioWearDetectionSensitivityAsync(string Guid, int newValue);

        Task<bool> SetAirAudioMicNCIncomingAsync(string Guid, bool newValue);

        Task<bool> SetAirAudioUnPairAsync(string Guid, bool newValue);

        Task<bool> SetAirAudioIsWearDetectionPauseMusicEnabledAsync(string Guid, bool newValue);

        Task<bool> SetAirAudioIsWearDetectionMuteMicEnabledAsync(string Guid, bool newValue);

        Task<bool> SetFactoryResetAsyncValueForAirAudioAsync(string Guid, bool newValue);

        Task<bool> SetAirAudioIsAutoPowerOffEnabledAsync(string Guid, bool newValue);

        Task<bool> SetAirAudioIsWearDetectionAnswerCallsEnabledAsync(string Guid, bool newValue);

        Task<bool> SetAirAudioAutoPowerOffIntervalAsync(string Guid, int newValue);

        #endregion Set

        #endregion IAirAudioCommodity

        Task<List<MonitorInfo>> GetCurrentMonitorCache();
    }
}