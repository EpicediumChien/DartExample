using DDPM.SA.Common.Display;
using Dell.Client.Framework.Common;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VcpCore.Common;

namespace DDPM.SA.Common
{
    public interface IDisplayService : IFrameworkPlugin, IInputSource
    {
        Task Reset0x52TimerTick(int millisecond, int processID = -0xFF);

        Task<List<MonitorInfo>> GetMonitors();

        Task<List<MonitorInfo>> Re_GetMonitors(CancellationToken Token);

        Task<Dictionary<EDID, Dictionary<object, object>>> GetVCPCacheTable();

        Task<string> GetCapabilitiesString(MonitorInfo monitorInfo);

        Task<string> GetVCPCapabilities(MonitorInfo monitorInfo);

        Task<ObjGetVCP> GetVCPCapability(MonitorInfo monitorInfo, byte code, int opt = 0);

        Task<ObjGetVCP> GetVCPCapability(MonitorInfo monitorInfo, string FunctionName, int opt = 0);

        Task<bool> SetVCPCapability(MonitorInfo monitorInfo, byte code, uint val);

        Task<bool> SetVCPCapability(MonitorInfo monitorInfoX, string FunctionName, string val);

        event EventHandler<VCPchangedEventArgs> VCPchanged;

        event EventHandler<DDCCIchangedEventArgs> DDCCIStatuschanged;

        event EventHandler<DisplaychangedEventArgs> Displaychanged;

        #region Bruce display properties

        /// <summary>
        /// HDR變更事件，回傳HDR狀態
        /// </summary>
        event EventHandler<bool> HDRChangeEvent;

        Task<DisplaySupportedProperties> GetDisplaySupportedProperties(MonitorInfo monitorInfo);

        Task<DisplayPropertiesInfo> GetDisplayPropertiesInfo(MonitorInfo monitorInfos);

        Task<DisplayCurrentPropertiesInfo> GetCurrentDisplayProperties(MonitorInfo monitorInfo);

        Task<bool> SetDisplayPropertiest(MonitorInfo monitorInfos, Properties properties, DisplayOrientation orientation);//Bruce 08-09 Modify the incoming value

        Task<bool> SetResolutions(MonitorInfo monitorInfos, Properties properties);

        Task<bool> SetOrientation(MonitorInfo monitorInfos, DisplayOrientation orientation);

        Task<bool> CallWindowsDisplaySetting();

        Task<bool> GetHDRStatus(MonitorInfo monitorInfos);

        Task<bool> SetHDRStatus(MonitorInfo monitorInfos, bool onoff);

        Task<bool> SetUSBCPrioritizationType(MonitorInfo monitorInfos, USBCPrioritizationType type);

        void SetEnableLockOrientation(bool isLock);

        Task<List<bool>> SetDisplayOrientation(List<MonitorInfo> monitorInfos);

        Task<string> GetOSDOrientation(MonitorInfo monitorInfos);

        Task<bool?> SetOSDOrientation(MonitorInfo monitorInfos, string Orientation);

        Task<DisplayOrientation> GetCurrentDisplayOrientation(string DisplayName);

        Task<string> GetMonitorCurrentResolution(MonitorInfo monitor);

        Task<string> GetMonitorMaxResolution(MonitorInfo monitor);

        Task<string> GetMonitorRefreshRate(MonitorInfo monitor);

        #endregion Bruce display properties

        #region PIP PBP Manager

        public Task<UInt16[]> GetPipPbpCapabilitiesWords(MonitorInfo monitorInfo);

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

        public Task<bool> UsbSwitch(MonitorInfo monitorInfo, UInt16 target = 0);

        #endregion PIP PBP Manager

        #region ALS functions

        Task<ALSConfig> GetALSFeatureValue(MonitorInfo monitorInfos, ALSFeatureQueryType type, int value);

        Task<bool> SetALSFeatureValue(MonitorInfo monitorInfos, ref ALSConfig param, ALSFeatureQueryType type, string value);

        Task<bool> UpdateALSFeatureValue(MonitorInfo monitorInfos);

        Task<List<ALSConfig>> GetConnectedALSConfig();

        Task<List<ALSConfig>> GetAllExistAlsConfig();

        Task<List<ALSConfig>> UpdateExistAlsConfig(List<MonitorInfo> monitorInfos);

        Task<bool> SynchronizeALSFeatureValue(ALSConfig monitorALS);

        #endregion ALS functions

        #region USBKVM

        Task<Dictionary<string, PCsInfo>> GetUSBKVMPCsList(MonitorInfo monitorInfo, Dictionary<string, InputInfo> inputList, List<InputSourceObj> subInputList);

        Task<bool> isScreenPartition(MonitorInfo monitorInfo);

        #endregion USBKVM

        #region EasyArange

        #region Properties - EasyArange
        /// <summary>
        /// The last error string after a EAPlugin method return error.
        /// </summary>
        public string EALastError { get; }
        #endregion Properties - EasyArange

        public Task<bool> SetEAFunctionEnabled(bool isEnabled);

        public Task<ObjGetVCP> GetEAFunctionEnabled();

        public event EventHandler<EAArgs> EASettingsChanged;

        public Task<bool> SetEAWrokSplit(MonitorInfo monitorInfo, int cellCount, char splitKey, List<double>? settings);
        public Task<bool> NotifyEASelectedLayoutChanged(MonitorInfo monitorInfo, SplitJson spJson);

        //Robert_Lin, 2024-9-13 Remove unused interfaces
        //public Task<bool> RequestEditSplit(MonitorInfo monitorInfo, int cellCount, char splitKey, string customName, List<double>? settings = null);

        //Robert_Lin, 2024-9-13 Remove unused interfaces
        //public event EventHandler<string> EAEditCompleted;

        public event EventHandler<string> EAEditStarted;

        //Robert_Lin, 2024-8-4 added
        public Task<bool> EAEditCommand(MonitorInfo monitorInfo, EAArgs args);

        public event EventHandler<EAArgs> EAEditReturn;

        //        public Task<bool> EAReloadMonitorSettings(MonitorInfo monitorInfo);

        /// <summary>
        /// Called by DeviceManagerSA only, when one of EzSettings is changed from DDPM.UI.
        /// It will call to EAPlugin IEasyArrangeService.ReloadEzSettings() to notify 
        /// EAPlugin reload EzSettings and refresh to its ViewModel.
        /// </summary>
        /// <returns></returns>
        public Task<bool> ReloadEzSettings();

        public Task<bool> SetEASelectedLayout(MonitorInfo monitorInfo, SplitJson spJson);
        public Task<bool> SetEASelectedLayout(MonitorInfo monitorInfo, int eaId);

        /// <summary>
        /// Return current Span across multiple monitor option is Enabled/Disabled;
        /// Note that it's different with EzSettings.IsSpanAcrossMultiMonitors (=ON|OFF)
        /// </summary>
        /// <returns>True=Enabled; False=Disabled</returns>
        public Task<bool> GetIsSpanEnabled();

        /// <summary>
        /// General notification  to EAPlugin from other Plugins inside DDPM.SA.User
        /// </summary>
        /// <param name="eaArgs"></param>
        /// <returns></returns>
        public Task<bool> NotifyEAMessage(EAArgs eaArgs);

        /// <summary>
        /// Launch Apps in the specified EasyMemory Profile, and arrange their window to the EasyArrange layout.
        /// This method is moved from EzMemoryPlugin. Can be called from UI (EzMemory module) and SA (EzMemoryPlugin).
        /// </summary>
        /// <param name="sortApps">List of AppInfos which are load from EM profile.</param>
        /// <param name="moInfo">MonitorInfo to specify the target monitor to be arranged.</param>
        /// <param name="eAid">EAID of a EasyArrange layout. [1~49] are preset layout, [1000~1004] are saved custom layout.</param>
        /// <returns></returns>
        public Task<bool> LaunchAndArrangeAppsWithEzArrange(Dictionary<String, Bind_AddFullPage_AppCollectionData> sortApps, MonitorInfo moInfo, int eAid);
        #endregion EasyArange

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

        Task<bool> SwitchGaming_VisionEngineType(MonitorInfo monitorInfo, Gaming_VisionEngineType VisionEngineType);

        #endregion Gaming

        #region OutReport

        Task<List<MonitorAssetReport>> GetMonitorAssetReport(List<MonitorInfo> monitorInfos);

        #endregion OutReport

        #region Display FWU Metadata

        Task<DisplayUpdateHelper> GetDisplayFWUpdate(bool isSkipCA, ISettingsManagerDev settingsPlugin);

        #endregion Display FWU Metadata
    }
}