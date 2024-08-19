using DDPM.SA.Common;
using DDPM.SA.Common.Display;
using DDPM.SA.Common.Settings;
using Dell.Client.Framework.Common;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static DDPM.SA.Common.ICLICommandTable;

namespace DDPM.SA.Common
{
    public class ITSettingEventArgs : EventArgs //definition for ICliManagerIT
    {
        public List<string> IT_Feature_TriggerList { get; set; }
        //force to use string as data transfer format
        public DDPMITConfig target_object { get; set; }
    }

    /// <summary>
    /// Interface, for SettingManager to use, it means CLI.Subagent use FindPluginByType with this to get system SettingManager
    /// Publish via Elevated privilege
    /// </summary>
    public interface ISettingsManagerIT : IFrameworkPlugin
    {
        Task<DDPMITConfig> ReadITConfigData(bool force_reload = false);
        Task<bool> WriteITConfigData(DDPMITConfig data, List<string> IT_Feature_list);
    }

    /// <summary>
    /// Interface, for SettingManager to use, it means User.SettingManager use FindPluginByType with this to get system SettingManager
    /// Publish via Unelevated privilege
    /// </summary>
    public interface ISettingsManagerSA : IFrameworkPlugin
    {
        event EventHandler<ITSettingEventArgs> ITSettingsActionEvent;
    }

    /// <summary>
    /// Interface, for User.SettingManager to use, it means User.DeviceManager use FindPluginByType with this to get User.SettingManager
    /// Publish via Unelevated privilege
    /// </summary>
    public interface ISettingsManagerDev : IFrameworkPlugin
    {
        //App config
        Task<DDPMSettings> ReloadAppConfigData(bool force_reload = false);
        Task<bool> SetAppConfigData(DDPMSettings data);
        Task<string> GetAppIconFolderPath();
        Task<List<DDPMMonitorSettings>> InitDDPMMonitorConfigFile(string modelname);
        Task<List<DDPMMonitorSettings>> ReloadMonitorSettings(string modelname);
        Task<bool> WriteMonitorSettings(string modelname, List<DDPMMonitorSettings> monitorSettings);
        Task<List<ColorPresetSettings>> ReadColorPresetSettings();
        Task<bool> WriteColorPresetSettings(List<ColorPresetSettings> colorPresetSettings);

        Task<List<HotkeySettings>> ReadHotkeySettings();
        Task<bool> WriteHotkeySettings(List<HotkeySettings> hotkeySettings);

        Task<List<PowerNapSetting>> ReadPowerNapSettings();
        Task<bool> WritePowerNapSettings(List<PowerNapSetting> powerNapSettings);

        Task<List<PowerNapSetting>> ImportPowerNapSettings(string filePath);
        Task<bool> ExportPowerNapSettings(List<PowerNapSetting> powerNapSettings, string filePath);

        //Easy Arrange
        public Task<string> WriteEasyArrangeSettings(EAMonitorSettings eaMonitorSettings);
        public Task<EAMonitorSettings> ReadEasyArrangeSettings(string monitorModel, string serialNumber);

        //ImpExpSettings
        Task<bool> DisplayExportSettings(string modelname, string seriveTag, string path);
        
        //IT lock
        event EventHandler<ITSettingEventArgs> ITSettingsActionEvent;
    }
}
