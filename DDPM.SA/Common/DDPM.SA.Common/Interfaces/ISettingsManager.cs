using DDPM.SA.Common.Display;
using DDPM.SA.Common.Settings;
using Dell.Client.Framework.Common;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DdmLibrary;
using DdmLibrary.Utility;

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

        Task<DDPMITConfig> GetITGlobalConfigs(bool force_reload = false);
        //Service to read/write current_user and local_machine
        Task<object> ReadRegistryData(RegistryHive hive, string keyPath, string keyName);
        Task<bool> WriteRegistryData(RegistryHive hive, string keyPath, string keyName, object value);
        Task<string> QueryAccessInfo();
        Task<string> QueryAccessInfoVer();
        Task<string> QueryAccessInfoAddr();
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

        Task<List<DDPMMonitorSettings>> InitDDPMMonitorConfigFile(string modelname, out bool binit);

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

        Task<bool> DisplayImportSettings(string path, bool isSameModel, out DDPMImpExpSettings ImpExpSettings);

        //GlobalSettings
        Task<GlobalSettingParam> ReadGlobalSettings();
        Task<bool> WriteGlobalSettings(GlobalSettingParam globalSettingParam);

        //IT lock
        event EventHandler<ITSettingEventArgs> ITSettingsActionEvent;

        //Service to read/write current_user and dispatch local_machine to ISettingsManagerSA
        Task<object> ReadRegistryData(RegistryHive hive, string keyPath, string keyName);
        Task<bool> WriteRegistryData(RegistryHive hive, string keyPath, string keyName, object value);

        event EventHandler SettingReadyEvent;

        //Migration
        Task<bool> isDDMMigration(out string folder_appdatapath_migration);
        Task<bool> ReadDDMMonitorSettings(string path, ref DDMMonitorSettings DDMmonitorsettings);
        Task<bool> ReadDDMUserSettings(string path, ref DDMUserSettings DDMusersettings);
        Task<DDMImpSettings> ReadDDMImpSettingsFile(string path);

        //For common json file read/write
        Task<string> ReadSerializedContentFromFile(string filePath);
        Task<bool> WriteSerializedContentToFile(string filePath, string content);
    }
}