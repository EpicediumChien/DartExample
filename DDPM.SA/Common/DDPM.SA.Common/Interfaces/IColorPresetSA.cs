using Dell.Client.Framework.Common;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VcpCore.Common;

//using VcpCore.Common;
//using VcpCore.Interfaces;

namespace DDPM.SA.Common
{
    public interface IColorPresetSA : IFrameworkPlugin
    {
        #region public for  Color Preset Plugin

        //event EventHandler<VCPchangedEventArgs> VCPchanged;

        event EventHandler<string> Coloreset_manual_ChangeEvent;

        event EventHandler<string> NightLightStatus_ChangeEvent;

        void ShowOSD_ColoPreset(MonitorInfo m, string strMsg, bool is_ShowUI = true, bool is_AUTO = false);

        Task<List<string>> ReadColorPreset(MonitorInfo m, string vcp_capbilities, bool SmartHDR_ON = false);

        //Task<List<ColorPresetSettings>> AddColorPresetForMonitorConfig(MonitorInfo mo, string AppName, string ColorPreset_Name, string supported_preset, List<ColorPresetSettings> config);
        Task<List<ColorPresetSettings>> AddColorPresetForMonitorConfig(MonitorInfo mo, string AppName, string ColorPreset_Name, string supported_preset, List<ColorPresetSettings> config, bool SmartHDR_ON = false);

        Task<List<ColorPresetSettings>> ChangeColorPresetForMonitorConfig(MonitorInfo mo, string AppName, string ColorPreset_Name, List<ColorPresetSettings> config);

        Task<List<ColorPresetSettings>> DeleteColorPresetForMonitorConfig(MonitorInfo mo, string AppName, List<ColorPresetSettings> config);

        //Task<bool> AutoSetColorPresetForMonitorConfig(MonitorInfo mo, string on_off, ISettingsManagerDev _SettingsPlugin, IDeviceManagerSA _DeviceManagerPlugin , bool SmartHDR_ON = false);
        // jim 20241207  modify for The DDPM color profile can not be applied by DDPM on Smart HDR mode.(Gaming monitor ex: AW2724DM)
        Task<bool> AutoSetColorPresetForMonitorConfig(List<MonitorInfo> _AllInfoMonitors, MonitorInfo mo, string on_off, ISettingsManagerDev _SettingsPlugin, IDeviceManagerSA _DeviceManagerPlugin, bool Is_Game_DeviceName = false, bool SmartHDR_ON = false, List<string> ColorPresetSupportList = null);

        //Task<bool> WriteColorPreset(MonitorInfo m, string ColorPreset_Name, ISettingsManagerDev _SettingsPlugin = null);
        Task<bool> WriteColorPreset(MonitorInfo m, string ColorPreset_Name, ISettingsManagerDev _SettingsPlugin = null, int colorPresetRunType = 0);

        //Task<List<ColorPresetSettings>> WriteColorPreset(MonitorInfo m, string ColorPreset_Name, List<ColorPresetSettings> config);

        void SetAppIconFolder(string folder_path);

        Task<Dictionary<string, InstalledAppInfo>> GetInstalledAppsList(bool isReload = false);

        Task<IIC_Metadata> DownloadICCData(string modelName, string displayName, bool blICCProfile = false, string savelPath = "", bool UpdateMetadata = false);

        Task<bool> SetMonitorProfile(MonitorInfo m, string ColorPreset_Name);

        Task<string> GetAutoColorPresetStatus(MonitorInfo mo, ISettingsManagerDev _SettingsPlugin);

        Task<bool> AutoColorManagementForMonitorConfig(MonitorInfo monitorInfo, string off_bymonitor_byhost, ISettingsManagerDev _SettingsPlugin, string ColorPreset_Name = "", string ICC_profile_Name = "");

        Task<string> GetColorManagementStatus(MonitorInfo mo, ISettingsManagerDev _SettingsPlugin);

        Task<string> GetColorPresetName(int Color_VCPCore_E2);

        Task<int> GetColorVCPCoreValue(string ColorPreset_Name);

        Task<string> Sync_ColorPresetName(MonitorInfo monitorInfo, string ColorPreset_Name);

        Task<bool> Migration(DdmLibrary.Utility.ColorPreset colorPresetSetting_Migration, string Model, string ServiceTag, ISettingsManagerDev _SettingsPlugin, int ColorForManual_VCPE2Code_value = 0);

        Task<bool> Import(MonitorInfo MonitorInfo, ColorPresetSettings colorPresetSetting_Import, ISettingsManagerDev _SettingsPlugin);

        Task<ColorPresetSettings> Export(MonitorInfo MonitorInfo, ISettingsManagerDev _SettingsPlugin);

        Task<bool> SyncNightlightStatus();

        Task<bool> CheckNightLightStatus();

        Task<bool> CheckNightLightScheduler();

        Task<bool> CheckColorICCStatus();

        Task<bool> StopRegistryMonitor_NightLight();

        Task<bool> StopRegistryMonitor_NightLightScheduler();

        Task<bool> StopRegistryMonitor_ICC();

        Task<bool> CompareColorPresetSupportList(List<string> NewList);

        #endregion public for  Color Preset Plugin
    }
}