using DDPM.SA.Common.Display;
using Dell.Client.Framework.Common;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VcpCore.Common;

namespace DDPM.SA.Common
{
    public interface IDisplayService : IFrameworkPlugin, IInputSource
    {
        Task Reset0x52TimerTick(int millisecond);

        Task<List<MonitorInfo>> GetMonitors(bool renew = false);

        Task<List<MonitorInfo>> Re_GetMonitors();

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

        Task<DisplayPropertiesInfo> GetDisplayPropertiesInfo(MonitorInfo monitorInfos);

        Task<bool> SetDisplayPropertiest(MonitorInfo monitorInfos, Properties properties, DisplayOrientation orientation);//Bruce 08-09 Modify the incoming value

        Task<bool> CallWindowsDisplaySetting();

        Task<bool> GetHDRStatus(MonitorInfo monitorInfos);

        Task<bool> SetHDRStatus(MonitorInfo monitorInfos, bool onoff);

        Task<bool> SetUSBCPrioritizationType(MonitorInfo monitorInfos, USBCPrioritizationType type);

        void SetEnableLockOrientation(bool isLock);

        Task<List<bool>> SetDisplayOrientation(List<MonitorInfo> monitorInfos);

        Task<string> GetOSDOrientation(MonitorInfo monitorInfos);

        Task<bool?> SetOSDOrientation(MonitorInfo monitorInfos, string Orientation);

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

        #endregion USBKVM

        #region EasyArange

        public Task<bool> SetEAFunctionEnabled(bool isEnabled);

        public Task<ObjGetVCP> GetEAFunctionEnabled();

        public Task<bool> SetEAWrokSplit(MonitorInfo monitorInfo, int cellCount, char splitKey, List<double>? settings);

        public Task<bool> RequestEditSplit(MonitorInfo monitorInfo, int cellCount, char splitKey, string customName, List<double>? settings = null);

        public event EventHandler<string> EAEditCompleted;

        public event EventHandler<string> EAEditStarted;

        //Robert_Lin, 2024-8-4 added
        public Task<bool> EAEditCommand(MonitorInfo monitorInfo, EAArgs args);

        public event EventHandler<EAArgs> EAEditReturn;

        #endregion EasyArange

        #region Gaming

        event EventHandler<GamingDisplayPropertiesInfo> GamingChangeEvent;

        Task<GamingDisplayPropertiesInfo> GetGamingProperties(MonitorInfo monitorInfo);

        Task<bool> GetCurrentGameEnhancementMode(MonitorInfo monitorInfo, ref Gaming_GameEnhancementMode GameEnhancementMode);

        Task<bool> GetCurrentGaming_ResponseTime(MonitorInfo monitorInfo, ref Gaming_ResponseTime ResponseTime);

        Task<bool> GetCurrentGaming_DarkStabilizer(MonitorInfo monitorInfo, ref Gaming_DarkStabilizer DarkStabilizer);

        Task<bool> GetCurrentGaming_HDRType(MonitorInfo monitorInfo, ref Gaming_HDRType HDRType);

        Task<bool> GetCurrentGaming_DualResolutionType(MonitorInfo monitorInfo, ref Gaming_DualResolutionType DualResolutionType);

        Task<bool> GetCurrentGaming_VisionEngineEnableType(MonitorInfo monitorInfo, ref GamingDisplayPropertiesInfo gamingDisplayPropertiesInfo);

        Task<bool> SetGameEnhancementMode(MonitorInfo monitorInfo, Gaming_GameEnhancementMode GameEnhancementMode);

        Task<bool> SetGaming_ResponseTime(MonitorInfo monitorInfo, Gaming_ResponseTime ResponseTime);

        Task<bool> SetGaming_DarkStabilizer(MonitorInfo monitorInfo, Gaming_DarkStabilizer DarkStabilizer);

        Task<bool> SetGaming_HDRType(MonitorInfo monitorInfo, Gaming_HDRType HDRType);

        Task<bool> SetGaming_DualResolutionType(MonitorInfo monitorInfo, Gaming_DualResolutionType DualResolutionType);

        Task<bool> SetGaming_VisionEngineEnableType(MonitorInfo monitorInfo, bool[] VisionEngineEnableType);

        Task<bool> SwitchGaming_VisionEngineType(MonitorInfo monitorInfo, Gaming_VisionEngineType VisionEngineType);

        #endregion Gaming
    }
}