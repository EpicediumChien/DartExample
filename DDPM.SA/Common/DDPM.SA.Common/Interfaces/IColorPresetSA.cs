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

        event EventHandler<VCPchangedEventArgs> VCPchanged;

        void ShowOSD_ColoPreset(MonitorInfo m, string strMsg, bool is_ShowUI = true, bool is_AUTO = false);

        Task<List<string>> ReadColorPreset(MonitorInfo m, string vcp_capbilities);

        Task<List<ColorPresetSettings>> AddColorPresetForMonitorConfig(MonitorInfo mo, string AppName, string ColorPreset_Name, string supported_preset, List<ColorPresetSettings> config);

        Task<List<ColorPresetSettings>> ChangeColorPresetForMonitorConfig(MonitorInfo mo, string AppName, string ColorPreset_Name, List<ColorPresetSettings> config);

        Task<List<ColorPresetSettings>> DeleteColorPresetForMonitorConfig(MonitorInfo mo, string AppName, List<ColorPresetSettings> config);

        //Task<List<ColorPresetSettings>> AutoSetColorPresetForMonitorConfig(MonitorInfo mo, string on_off, List<ColorPresetSettings> config);
        Task<bool> AutoSetColorPresetForMonitorConfig(MonitorInfo mo, string on_off, ISettingsManagerDev _SettingsPlugin, IDeviceManagerSA _DeviceManagerPlugin);

        Task<List<ColorPresetSettings>> WriteColorPreset(MonitorInfo m, string ColorPreset_Name, List<ColorPresetSettings> config);

        Task<List<ColorPresetSettings>> WriteColorPreset_AUTO(MonitorInfo m, string ColorPreset_Name, List<ColorPresetSettings> config);

        void SetAppIconFolder(string folder_path);

        Task<Dictionary<string, InstalledAppInfo>> GetInstalledAppsList(bool isReload = false);

        Task<IIC_Metadata> DownloadICCData(MonitorInfo m, string savelPath = "");

        Task<bool> SetMonitorProfile(MonitorInfo m, string ColorPreset_Name);

        Task<string> GetAutoColorPresetStatus(MonitorInfo mo, ISettingsManagerDev _SettingsPlugin);

        #endregion public for  Color Preset Plugin
    }
}