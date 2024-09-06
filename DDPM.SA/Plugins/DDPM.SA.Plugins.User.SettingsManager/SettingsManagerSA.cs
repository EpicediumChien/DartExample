using DDPM.SA.Common;
using DDPM.SA.Common.Display;
using DDPM.SA.Common.Settings;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.Common.Annotations;
using Dell.Client.Framework.Common.PluginConditions;
using Dell.Client.Framework.Interfaces;
using Microsoft;
using Microsoft.VisualBasic.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Log = Dell.Client.Framework.Common.Log;

namespace DDPM.SA.Plugins.User.SettingsManager
{
    [Plugin(IDs.DDPM_SETTINGSMANAGER_SA_PLUGIN_ID, pluginName, PluginOrderGroupType.Core, Version = pluginVersion)]
    [Descriptor(Description = pluginDescription)]
    [Publisher(Name = publisherCompany, Website = publisherWebsite, Support = publisherSupport)]
    [PublishedUnelevatedInterface(new[] { typeof(ISettingsManagerDev) })]
    public class SettingsManagerSA : BaseAgentPlugin, IDisposableObservable, ISettingsManagerDev
    {
        public const string PluginLogId = "User.SettingsManager";

        #region Private Members

        private const string pluginName = "User.SettingsManager.Plugin";
        private const string pluginVersion = "1.0.0";
        private const string pluginDescription = "This plugin implements User.SettingsManager.Plugin.";
        private const string publisherCompany = "Wistron";
        private const string publisherWebsite = "https://www.wistron.com";
        private const string publisherSupport = "This plugin implements User.SettingsManager.Plugin.";
        private static Log _log;
        private IAgent _agent;

        private enum log_type
        {
            info = 0,
            error
        }

        //Folder/Path collection
        /*
         * SpecialFolder.ApplicationData:       C:\Users\{UserName}\AppData\Roaming
         * SpecialFolder.CommonApplicationData: C:\ProgramData
         * SpecialFolder.ProgramFiles:          C:\Program Files
         * SpecialFolder.CommonProgramFiles:    C:\Program Files\Common Files
         * SpecialFolder.DesktopDirectory:      C:\Users\{UserName}\Desktop
         * SpecialFolder.LocalApplicationData:  C:\Users\{UserName}\AppData\Local
         * SpecialFolder.MyDocuments:           C:\Users\{UserName}\Documents
         * SpecialFolder.System:                C:\Windows\system32
         * ...
         */

        //Basic
        //private static string path_programdata = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
        private static string folder_product = "Dell Display and Peripheral Manager";

        //Global
        //private static string folder_programdata_Applist = path_programdata + "\\" + folder_product + "\\DDPM\\AppLibrary";
        private static string folder_localappdata_Applist = "AppLibrary";

        private static string folder_localappdata_Appicon = "Icons";
        private static string folder_localappdata_Display = "Display";

        //private static string folder_programdata_DownloadInstaller = path_programdata + "\\" + folder_product + "\\Downloaded Installations";
        //private static string folder_programdata_DownloadInstallerLog = path_programdata + "\\" + folder_product + "\\InstallationLogs";
        //private static string folder_programdata_AppIcons = folder_programdata_Applist + "\\Icons";
        //private static string filepath_programdata_Applist = folder_programdata_Applist + "\\InstalledAppInfo.json";
        //private static string filepath_programdata_ColorSetting = folder_programdata_Applist + "\\ColorSetting.json";
        //private static string filepath_programdata_Common_AppSettings = path_programdata + "\\" + folder_product + "\\Common\\AppSettings.json";
        //private static string filepath_programdata_Common_AdminSettings = path_programdata + "\\" + folder_product + "\\Common\\AdminSettings.json";
        //Per user
        //private static string folder_usersettings = "ApplicationSettingImport";
        //private static string folder_colormanagement = "color";
        //private static string folder_fwupdatetool = "ISP";
        private static string filename_appsettings_peruser = "DDPM.Configs.json";

        private static string filename_colorpreset_peruser = "ColorSetting.json";
        private static string filename_hotkey_peruser = "HotkeySetting.json";
        private static string filename_powernap_peruser = "PowerNapSetting.json";
        private static string filename_GlobalSetting_peruser = "GlobalSetting.json";

        //---
        private ISettingsManagerSA? _SysSettingsPlugin;

        private readonly object _PluginConditionLock_SysSettings = new object();
        private bool relay_registered = false;

        private readonly object _Setting_Locker = new object();

        private DDPMSettings _settings { get; set; }
        private string _settings_path { get; set; } = string.Empty;
        private List<ColorPresetSettings> _colorPresetSettings { get; set; }
        private string _colorsettings_path { get; set; }
        private string _appiconfolder_path { get; set; }
        private Dictionary<string, InstalledAppInfo> _AllAppData = new Dictionary<string, InstalledAppInfo>();
        private Dictionary<string, List<DDPMMonitorSettings>> _AllMonitorSettings = new Dictionary<string, List<DDPMMonitorSettings>>();
        private string _display_path { get; set; }
        private List<ColorPresetSettings> _preset_settings = new List<ColorPresetSettings>();//Dean 0626 fix SAST issue

        private List<HotkeySettings> _hotkeySettings { get; set; }
        private string _hotkeysettings_path { get; set; }
        private static List<HotkeySettings> _present_hotkey_settings = new List<HotkeySettings>();

        private List<PowerNapSetting> _powerNapSettings { get; set; }
        private string _powerNapsettings_path { get; set; }
        private static List<PowerNapSetting> _present_powerNap_settings = new List<PowerNapSetting>();
        private static string _settingsAccessInfo = string.Empty;

        private string _GlobalSetting_path;
        private GlobalSettingParam _GlobalSettingParam = new GlobalSettingParam();

        #endregion Private Members

        #region Constructor

        public SettingsManagerSA(IAgent agent) : base(agent, PluginLogId)
        {
            _agent = agent;
        }

        #endregion Constructor

        #region Overriding methods

        protected override void OnPluginStarting()
        {
            _agent.PluginManager.PluginsStarted += PluginManagerOnPluginsStarted;

            PluginCondition = new PluginStartedCondition();
            WriteLog("User.SettingsManager plugin report started");

            InitializeSysSettingsPlugin();

            //Move to DoRelayRegister function since DDPM private key added
            /*InitDDPMUserConfigFile();
            InitColorPresetConfigFile();
            InitHotkeyConfigFile();
            InitPowerNapConfigFile();*/

            //Robert_Lin, 2024-9-3, removed, will use SettingsManagerSA.ReloadMonitorSettings() instead
            //EAMakeSureDirExist();
        }

        #endregion Overriding methods

        #region IDisposableObservable Support

        public bool IsDisposed { get; private set; }

        /// <summary>
        /// Override for Dispose
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            WriteLog($"Dispose: {disposing}");
            if (!IsDisposed)
            {
                if (disposing)
                {
                    _agent.PluginManager.PluginsStarted -= PluginManagerOnPluginsStarted;
                    _agent = null;

                    if (_SysSettingsPlugin != null)
                    {
                        _SysSettingsPlugin.ITSettingsActionEvent -= _SysSettingsPlugin_ActionEvent;
                        relay_registered = false;
                    }
                }

                IsDisposed = true;
            }
            base.Dispose(disposing);
        }

        #endregion IDisposableObservable Support

        #region Event Handler

        private void PluginManagerOnPluginsStarted(object sender, PluginsStartedEventArgs e)
        {
            if (e == null)
                return;
            if (e.ChangedPlugins == null)
                return;
            if (e.ChangedPlugins.Any() == false)
                return;

            if (e.ChangedPlugins.OfType<ISettingsManagerDev>().Any())
            {
                WriteLog("User SettingsManager plugin started.");
            }

            if (e.ChangedPlugins.OfType<ISettingsManagerSA>().Any())
                InitializeSysSettingsPlugin();
        }

        #endregion Event Handler

        #region Private methods

        /// <summary>
        /// //
        /// </summary>
        /// <param name="text"></param>
        /// <param name="log_type">0 means info, others means error</param>
        private void WriteLog(string text, log_type log_type = log_type.info)
        {
            text = "[User.SettingsManager] " + text;
            Console.WriteLine(text);
            if (log_type == log_type.info)
                Log.Info(text);
            else
                Log.Error(text);
        }

        private void InitializeSysSettingsPlugin()
        {
            if (_SysSettingsPlugin != null)
                return;

            _SysSettingsPlugin = _agent.PluginManager.FindPluginByType<ISettingsManagerSA>(PluginResolution.Dynamic);

            if (_SysSettingsPlugin is IFrameworkPluginConditionNotification pluginCondition)
            {
                pluginCondition.PluginConditionChangeHandler += OnSysSettingsManagerPluginConditionChangeHandler;
                GetCurrentSysSettingsManagerPluginCondition();
            }
        }

        private void OnSysSettingsManagerPluginConditionChangeHandler(object sender, EventArgs e)
        {
            GetCurrentSysSettingsManagerPluginCondition();
        }

        private void GetCurrentSysSettingsManagerPluginCondition()
        {
            _ = Task.Run(async () =>
            {
                var pluginCondition = await (_SysSettingsPlugin as IFrameworkPluginConditionNotification)?.CurrentConditionAsync();

                lock (_PluginConditionLock_SysSettings)
                {
                    if (pluginCondition is PluginErrorCondition)
                    {
                        WriteLog($"{nameof(GetCurrentSysSettingsManagerPluginCondition)} - Sys SettingsManager Plugin is in an error condition");
                    }
                    else if (pluginCondition is PluginRunningCondition || pluginCondition is PluginStartedCondition)
                    {
                        WriteLog($"{nameof(GetCurrentSysSettingsManagerPluginCondition)} - Sys SettingsManager Plugin is in a {nameof(pluginCondition)} condition");
                        if (!relay_registered && _SysSettingsPlugin != null)
                        {
                            DoRelayRegister();
                        }
                    }
                }
            });
        }

        private void DoRelayRegister()
        {
            if (_SysSettingsPlugin == null)
            {
                WriteLog("System Settings Manager is null, do not register its relay");
                return;
            }
            _SysSettingsPlugin.ITSettingsActionEvent += _SysSettingsPlugin_ActionEvent;
            relay_registered = true;

            _settingsAccessInfo = _SysSettingsPlugin.QueryAccessInfo().Result;
            InitDDPMUserConfigFile();
            InitColorPresetConfigFile();
            InitHotkeyConfigFile();
            InitPowerNapConfigFile();
            InitGlobalSettingConfigFile();
        }

        private void _SysSettingsPlugin_ActionEvent(object? sender, ITSettingEventArgs e)
        {
            _ = Task.Run(() =>
            {
                if (_SysSettingsPlugin == null)
                {
                    WriteLog("[From IT Settings event] null Device Manager object!");
                    return;
                }

                if (e == null || e == EventArgs.Empty)
                {
                    WriteLog("[From IT Settings event] Got Empty ITSettingEventArgs!");
                    return;
                }
                if (e.target_object == null)
                {
                    WriteLog("[From IT Settings event] Got Empty target_object!");
                    return;
                }
                //
                //Do Settings update
                //
                if (_settings.LockSettings == null)
                {
                    WriteLog("[From IT Settings event] Got Empty LockSettings of _settings!");
                    return;
                }

                foreach (string feature in e.IT_Feature_TriggerList)
                {
                    PropertyInfo propertyOrigin = _settings.LockSettings.GetType().GetProperty(feature);
                    PropertyInfo propertyUpdate = e.target_object.GetType().GetProperty(feature);
                    WriteLog($"[Settings at user] {feature} = (origin){propertyOrigin.GetValue(_settings.LockSettings)}; (update){propertyUpdate.GetValue(e.target_object)}");
                    if (propertyOrigin != null && propertyOrigin.CanWrite)
                    {
                        propertyOrigin.SetValue(_settings.LockSettings, propertyUpdate.GetValue(e.target_object));
                        WriteLog($"[Settings at user] Feature [{feature}] updated");
                    }
                    else
                    {
                        WriteLog($"propertyOrigin of {feature} not found or not writable.");
                    }
                }
                SetAppConfigData(_settings);

                //
                //Try to Notify UI for subscriber
                //
                OnITSettingsActionEventNotify(e);
            });
        }

        #endregion Private methods

        #region ISettingManagerDev implementation

        public event EventHandler<ITSettingEventArgs> ITSettingsActionEvent;

        //Event from SettingsManagerPlugin.cs and bypass to subscriber
        private void OnITSettingsActionEventNotify(ITSettingEventArgs e)
        {
            if (ITSettingsActionEvent == null || e == null || e == EventArgs.Empty)
                return;

            EventHandler<ITSettingEventArgs> Handler = ITSettingsActionEvent;
            if (Handler != null)
            {
                Handler.Invoke(this, e);
                WriteLog($"[User Settings plugin] ITSettingsActionEvent Invoked to subscriber");
            }
        }

        public Task<List<DDPMMonitorSettings>> InitDDPMMonitorConfigFile(string modelname)
        {
            string folder = GetActiveUserLocalAppDataPath();
            WriteLog($"GetActiveUserLocalAppDataPath: {folder}");
            string folder_appdatapath_display = folder + "\\" + folder_product + "\\" + folder_localappdata_Display;
            //create display folder if not exist
            _display_path = folder_appdatapath_display;
            List<DDPMMonitorSettings> monitorSettingList = new List<DDPMMonitorSettings>();
            try
            {
                if (!Directory.Exists(_display_path))
                {
                    DirectoryInfo di = System.IO.Directory.CreateDirectory(_display_path);
                    WriteLog($"create folder {_display_path} success");
                }
            }
            catch
            {
                WriteLog($"CreateDirectory with {_display_path} failed.");
                _AllMonitorSettings = null;
                return Task.FromResult(monitorSettingList);
            }
            //create monitor setting file if not exist
            string file_monitorconfig_path = _display_path + "\\" + modelname + ".json";
            WriteLog($"_monitorSettings_path is {file_monitorconfig_path}.");
            //string info;
            if (File.Exists(file_monitorconfig_path))
            {
                monitorSettingList = ReloadMonitorSettings(modelname).Result;
                if (_AllMonitorSettings != null)
                {
                    //find monitor settings
                    if (!_AllMonitorSettings.ContainsKey(modelname))
                    {
                        _AllMonitorSettings.Add(modelname, monitorSettingList);
                    }
                }
                else
                {
                    _AllMonitorSettings = new Dictionary<string, List<DDPMMonitorSettings>>();
                    _AllMonitorSettings.Add(modelname, monitorSettingList);
                }
            }
            else
            {
                FileInfo fileInfo = new FileInfo(file_monitorconfig_path);
                fileInfo.Create().Close();
                WriteLog("[InitMonitorConfigFile] settings file not exist, new an object");
                //init data to file
                if (WriteMonitorSettings(modelname, monitorSettingList).Result)
                {
                    WriteLog("[InitMonitorConfigFile] Monitor settings file create and write success");
                }
                else
                {
                    WriteLog("[InitMonitorConfigFile] Monitor settings file create and write failed");
                }
            }
            return Task.FromResult(monitorSettingList);
        }

        public Task<DDPMSettings> ReloadAppConfigData(bool force_reload = false)
        {
            lock (_Setting_Locker)
            {
                var ddpm_app = new DDPMAppSettings();
                var ddpm_user = new DDPMUserSettings();
                var ddpm_it = new DDPMITConfig();

                if (!string.IsNullOrEmpty(_settings_path)) // 2024-07-09, Elie: check string is null or empty before using.
                {
                    if (File.Exists(_settings_path))
                    {
                        if (_settings == null || force_reload == true)
                        {
                            string info;
                            string output = DDPMFileSecurity.GetSerializedJsonString(_settingsAccessInfo, _settings_path, out info);//, false);
                            _settings = JsonConvert.DeserializeObject<DDPMSettings>(output);
                        }
                    }
                    else
                    {
                        WriteLog("ReloadAppConfigData, but file is not exist");
                        _settings = null;
                    }
                }
                else
                    _settings = new DDPMSettings(ddpm_app, ddpm_user, ddpm_it);

                if (_settings != null)
                {
                    //0819 get IT config and apply it
                    if (_SysSettingsPlugin != null)
                    {
                        DDPMITConfig tmp = _SysSettingsPlugin.GetITGlobalConfigs(force_reload).Result;
                        if (tmp != null)
                            _settings.LockSettings = tmp;
                    }
                }

                return Task.FromResult(_settings);
            }
        }

        public Task<bool> SetAppConfigData(DDPMSettings data)
        {
            lock (_Setting_Locker)
            {
                if (data == null)
                    return Task.FromResult(false);
                _settings = data;

                /*bool result = DDPMSettings.exportSettingstoFile(_settings_path, data.AppSettings, data.UserSettings);
                if (!result)
                {
                    WriteLog("exportSettingstoFile failed.");
                }
                return Task.FromResult(result);*/

                string info;
                if (!DDPMFileSecurity.SetJsonContentFromSerializedString(_settingsAccessInfo, JObject.FromObject(_settings).ToString(), _settings_path, out info))//, false))
                {
                    WriteLog(info);
                    return Task.FromResult(false);
                }
                return Task.FromResult(true);
            }
        }

        #region Monitor Settings

        public Task<List<DDPMMonitorSettings>> ReloadMonitorSettings(string modelname)
        {
            List<DDPMMonitorSettings> monitorSettings = new List<DDPMMonitorSettings>();
            if (!string.IsNullOrEmpty(_settings_path))
            {
                string monitorSettings_path = _display_path + "\\" + modelname + ".json";
                if (File.Exists(monitorSettings_path))
                {
                    if (_AllMonitorSettings != null)
                    {
                        if (_AllMonitorSettings.ContainsKey(modelname))
                        {
                            return Task.FromResult(_AllMonitorSettings[modelname]);
                        }
                    }
                    string strReadJson = string.Empty;
                    using (var reader = new StreamReader(monitorSettings_path))
                    {
                        strReadJson = reader.ReadToEnd();
                    }

                    if (strReadJson == string.Empty || strReadJson.Length == 0)
                        return Task.FromResult(monitorSettings);
                    try
                    {
                        //string info;
                        //string output = DDPMFileSecurity.GetSerializedJsonString(monitorSettings_path, out info);//, false);
                        monitorSettings = RunMonitorListDeserializeObject(strReadJson);
                    }
                    catch (Exception)
                    {
                        ;
                    }
                }
            }
            return Task.FromResult(monitorSettings);
        }

        public Task<bool> WriteMonitorSettings(string modelname, List<DDPMMonitorSettings> monitorSettings)
        {
            if (monitorSettings == null)
            {
                return Task.FromResult(false);
            }
            string monitorSettings_path = _display_path + "\\" + modelname + ".json";
            //JArray jArray = new JArray();
            //jArray.Add(JObject.FromObject(monitorSettings));
            string str = RunSerializeObject(modelname, monitorSettings);
            if (string.IsNullOrEmpty(str))
            {
                return Task<bool>.FromResult(false);
            }
            //string info;
            //if (!DDPMFileSecurity.SetJsonContentFromSerializedString(jArray.ToString(), monitorSettings_path, out info))//, false))
            //{
            //    WriteLog(info);
            //    return Task.FromResult(false);
            //}
            _AllMonitorSettings[modelname] = monitorSettings;

            return Task.FromResult(true);
        }

        #endregion Monitor Settings

        #region color preset settings

        public Task<string> GetAppIconFolderPath()
        {
            return Task.FromResult(_appiconfolder_path);
        }

        public Task<List<ColorPresetSettings>> ReadColorPresetSettings()
        {
            _preset_settings.Clear();

            string strFilePath = _colorsettings_path;// GetMonitorColorPresetJsonPath();

            if (File.Exists(strFilePath))
            {
                string strReadJson = string.Empty;
                using (var reader = new StreamReader(strFilePath))
                {
                    strReadJson = reader.ReadToEnd();
                }

                if (strReadJson == string.Empty || strReadJson.Length == 0)
                    return Task.FromResult(_preset_settings);

                try
                {
                    _preset_settings = RunDeserializeObject(strReadJson);
                }
                catch (Exception)// ex)
                {
                    return Task.FromResult(_preset_settings);
                }
            }
            else
            {
                File.Create(strFilePath).Close();
            }

            return Task.FromResult(_preset_settings);
        }

        public Task<bool> WriteColorPresetSettings(List<ColorPresetSettings> colorPresetSettings)
        {
            if (colorPresetSettings == null)
                return Task.FromResult(false);

            string temp = RunSerializeObject(colorPresetSettings);
            if (!string.IsNullOrWhiteSpace(temp))
                return Task.FromResult(true);

            return Task.FromResult(false);
        }

        #endregion color preset settings

        #region hotkey settings

        public Task<List<HotkeySettings>> ReadHotkeySettings()
        {
            _hotkeySettings?.Clear();

            string strFilePath = _hotkeysettings_path;// GetMonitorColorPresetJsonPath();

            if (File.Exists(strFilePath))
            {
                string strReadJson = string.Empty;
                using (var reader = new StreamReader(strFilePath))
                {
                    strReadJson = reader.ReadToEnd();
                }

                if (strReadJson == string.Empty || strReadJson.Length == 0)
                    return Task.FromResult(_present_hotkey_settings);

                try
                {
                    _present_hotkey_settings = RunHotkeyDeserializeObject(strReadJson);
                }
                catch (Exception)// ex)
                {
                    return Task.FromResult(_present_hotkey_settings);
                }
            }
            else
            {
                File.Create(strFilePath).Close();
            }

            return Task.FromResult(_present_hotkey_settings);
        }

        public Task<bool> WriteHotkeySettings(List<HotkeySettings> hotkeySettings)
        {
            if (hotkeySettings == null)
                return Task.FromResult(false);

            string temp = RunSerializeObject(hotkeySettings);
            if (!string.IsNullOrWhiteSpace(temp))
                return Task.FromResult(true);

            return Task.FromResult(false);
        }

        #endregion hotkey settings

        #region powerNap settings

        public Task<List<PowerNapSetting>> ReadPowerNapSettings()
        {
            _powerNapSettings?.Clear();

            string strFilePath = _powerNapsettings_path;

            if (File.Exists(strFilePath))
            {
                string strReadJson = string.Empty;
                using (var reader = new StreamReader(strFilePath))
                {
                    strReadJson = reader.ReadToEnd();
                }

                if (strReadJson == string.Empty || strReadJson.Length == 0)
                    return Task.FromResult(_present_powerNap_settings);

                try
                {
                    _present_powerNap_settings = RunPowerNapDeserializeObject(strReadJson);
                }
                catch (Exception)// ex)
                {
                    return Task.FromResult(_present_powerNap_settings);
                }
            }
            else
            {
                File.Create(strFilePath).Close();
            }

            return Task.FromResult(_present_powerNap_settings);
        }

        public Task<bool> WritePowerNapSettings(List<PowerNapSetting> powerNapSettings)
        {
            if (powerNapSettings == null)
                return Task.FromResult(false);

            string temp = RunSerializeObject(powerNapSettings);
            if (!string.IsNullOrWhiteSpace(temp))
                return Task.FromResult(true);

            return Task.FromResult(false);
        }

        public Task<List<PowerNapSetting>> ImportPowerNapSettings(string filePath)
        {
            //0905 Elsa Add Security
            string FileInfo;
            if (!DDPMFileSecurity.IsFilePathValid(filePath, out FileInfo))
            {
                _log.Info($"{nameof(ImportPowerNapSettings)} {FileInfo}");
                return Task.FromResult(_present_powerNap_settings);
            }

            string strReadJson = string.Empty;
            using (var reader = new StreamReader(filePath))
            {
                strReadJson = reader.ReadToEnd();
            }

            if (strReadJson == string.Empty || strReadJson.Length == 0)
                return Task.FromResult(_present_powerNap_settings);
            try
            {
                _present_powerNap_settings = RunPowerNapDeserializeObject(strReadJson);
                _present_powerNap_settings.RemoveAll(x => x.SerialNumber == null);
            }
            catch (Exception)// ex)
            {
            }
            return Task.FromResult(_present_powerNap_settings);
        }

        public Task<bool> ExportPowerNapSettings(List<PowerNapSetting> powerNapSettings, string filePath)
        {
            if (powerNapSettings == null)
                return Task.FromResult(false);

            string temp = RunSerializeObject(powerNapSettings, filePath);
            if (!string.IsNullOrWhiteSpace(temp))
                return Task.FromResult(true);

            return Task.FromResult(false);
        }

        #endregion powerNap settings

        #region EasyArrange Settings - 2024-8-26 unused, use DeviceManager instead

        public Task<string> WriteEasyArrangeSettings(EAMonitorSettings eaMonitorSettings)
        {
            ////Dir: C:\Users\{UserName}\AppData\Local\Dell Display and Peripheral Manager\EA
            //string dir = GetEaUserSettingsDir();
            ////Filename: EA-{MonitorModel}_{SerialNumber}.json
            //string fileName = eaMonitorSettings.GetFileName();
            //string pathName = Path.Combine(dir, fileName);

            //string jsonString = JsonConvert.SerializeObject(eaMonitorSettings);

            //try
            //{
            //    using (StreamWriter writer = new StreamWriter(pathName))
            //    {
            //        writer.Write(jsonString);
            //    }
            //}
            //catch (Exception e1)
            //{
            //    return Task.FromResult("EXCEPTION: " + e1.Message);
            //}

            return Task.FromResult("OK");
        }

        public Task<EAMonitorSettings> ReadEasyArrangeSettings(string monitorModel, string serialNumber)
        {
            ////Dir: C:\Users\{UserName}\AppData\Local\Dell Display and Peripheral Manager\EA
            //string dir = GetEaUserSettingsDir();
            ////Filename: EA-{MonitorModel}_{SerialNumber}.json
            //string fileName = EAMonitorSettings.GetFileName(monitorModel, serialNumber);
            //string pathName = Path.Combine(dir, fileName);

            //if (string.IsNullOrWhiteSpace(pathName))
            //{
            //    WriteLog($"ReadEasyArrangeSettings({monitorModel},{serialNumber}) pathName is empty.");
            //    return Task.FromResult<EAMonitorSettings>(null);
            //}

            //if (!File.Exists(pathName))
            //{
            //    WriteLog($"ReadEasyArrangeSettings({monitorModel},{serialNumber}) pathName({pathName}) not exist.");
            //    return Task.FromResult<EAMonitorSettings>(null);
            //}

            //EAMonitorSettings? eaSettings;
            //try
            //{
            //    string jsonString = String.Empty;
            //    using (var reader = new StreamReader(pathName))
            //    {
            //        jsonString = reader.ReadToEnd();
            //    }
            //    if (String.IsNullOrEmpty(jsonString))
            //    {
            //        WriteLog($"ReadEasyArrangeSettings({monitorModel},{serialNumber}) read file error, file is empty.");
            //        return Task.FromResult<EAMonitorSettings>(null);
            //    }

            //    eaSettings = JsonConvert.DeserializeObject<EAMonitorSettings>(jsonString);
            //}
            //catch (Exception e1)
            //{
            //    WriteLog($"ReadEasyArrangeSettings({monitorModel},{serialNumber}) EXCEPTION: {e1.Message}");
            //    return Task.FromResult<EAMonitorSettings>(null);
            //}

            //if (eaSettings != null)
            //    return Task.FromResult<EAMonitorSettings>(eaSettings);
            //else
            //{
            //    WriteLog($"ReadEasyArrangeSettings({monitorModel},{serialNumber}) Deserialize return null.");
            //    return Task.FromResult<EAMonitorSettings>(null);
            //}
            return Task.FromResult<EAMonitorSettings>(null);
        }

        #endregion EasyArrange Settings - 2024-8-26 unused, use DeviceManager instead

        #region DisplayImpExpSettings

        public Task<bool> DisplayExportSettings(string modelname, string seriveTag, string path)
        {
            DDPMImpExpSettings impexpSettings = new DDPMImpExpSettings();

            DDPMSettings settings = ReloadAppConfigData().Result;

            impexpSettings.AppSettings = settings.AppSettings;
            impexpSettings.UserSettings = settings.UserSettings;

            List<DDPMMonitorSettings> monitorSettings = ReloadMonitorSettings(modelname).Result;

            if (path.Substring(path.Length - 5, 5) != ".json")
            {
                path = path + ".json";
            }

            foreach (DDPMMonitorSettings _settings in monitorSettings)
            {
                if (_settings.ServiceTag == seriveTag)
                {
                    impexpSettings.MonitorSettings = _settings;

                    //export file
                    FileInfo fileInfo = new FileInfo(path);
                    fileInfo.Create().Close();
                    //init data to file
                    if (WriteImpExpSettings(path, impexpSettings))
                    {
                        WriteLog("[ExportSettingsFile] Monitor settings file create and write success");
                        return Task.FromResult<bool>(true);
                    }
                    else
                    {
                        WriteLog("[ExportSettingsFile] Monitor settings file create and write failed");
                        return Task.FromResult<bool>(false);
                    }
                }
            }

            return Task.FromResult<bool>(false);
        }

        public Task<bool> DisplayImportSettings(string path, /*bool isSameModel, */out List<VCP> vcps)
        {
            WriteLog("[DisplayImportSettings] path :" + path);
            List<DDPMMonitorSettings> monitorSettingsList = new List<DDPMMonitorSettings>();
            DDPMImpExpSettings ImpExpSettings = ReadImportSettingsFile(path);
            if (ImpExpSettings != null)
            {
                DDPMMonitorSettings monitorSettings = new DDPMMonitorSettings();
                monitorSettings = ImpExpSettings.MonitorSettings;
                WriteLog("[DisplayImportSettings] monitorSettings.Model :" + monitorSettings.Model);
                string monitorSettings_path = _display_path + "\\" + monitorSettings.Model + ".json";
                WriteLog("[DisplayImportSettings] monitorSettings_path :" + monitorSettings_path);
                if (File.Exists(monitorSettings_path))
                {
                    monitorSettingsList = ReloadMonitorSettings(monitorSettings.Model).Result;
                    foreach (DDPMMonitorSettings settings in monitorSettingsList)
                    {
                        if (settings.ServiceTag == monitorSettings.ServiceTag/* || isSameModel*/)
                        {
                            settings.Input = monitorSettings.Input;
                            settings.KVM = monitorSettings.KVM;
                            settings.VCPs = monitorSettings.VCPs;
                            settings.EA = monitorSettings.EA;
                            if (WriteMonitorSettings(settings.Model, monitorSettingsList).Result)
                            {
                                vcps = monitorSettings.VCPs;
                                //if (!isSameModel)
                                //{
                                    return Task.FromResult<bool>(true);
                                //}
                            }
                            else
                            {
                                WriteLog("[DisplayImportSettings] Import settings Fail...");
                                break;
                            }
                        }
                    }
                }
                else
                {
                    WriteLog("[DisplayImportSettings] Not Find Fail...");
                }
            }
            else
            {
                WriteLog("[DisplayImportSettings] Settings is null...");
            }
            vcps = new List<VCP>();
            return Task.FromResult<bool>(false);
        }

        #endregion DisplayImpExpSettings

        #endregion ISettingManagerDev implementation

        private string GetActiveUserLocalAppDataPath()
        {
            string localAppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            Console.WriteLine("Local App Data Path: " + localAppDataPath);
            return localAppDataPath;
        }

        #region EasyArrange Settings

        //private string _dir_ddpmUserSettings = String.Empty;
        //private string _dir_eaMonitorSettings = String.Empty;
        //private const string _dirName_EA = "EA";

        /// <summary>
        /// The directory of DDPM per user settings, should be %LocalAppData%\Dell Display and Peripheral Manager
        /// Or "C:\Users\{UserName}\AppData\Local\Dell Display and Peripheral Manager"
        /// </summary>
        //private string GetDdpmUserSettingsDir()
        //{
        //    if (String.IsNullOrEmpty(_dir_ddpmUserSettings))
        //    {
        //        // C:\Users\{UserName}\AppData\Local
        //        string dir_localAppData = GetActiveUserLocalAppDataPath();
        //        // C:\Users\{UserName}\AppData\Local\Dell Display and Peripheral Manager
        //        _dir_ddpmUserSettings = System.IO.Path.Combine(dir_localAppData, folder_product);
        //    }
        //    return _dir_ddpmUserSettings;
        //}

        // C:\Users\{UserName}\AppData\Local\Dell Display and Peripheral Manager\EA
        //private string GetEaUserSettingsDir()
        //{
        //    if (String.IsNullOrEmpty(_dir_eaMonitorSettings))
        //    {
        //        _dir_eaMonitorSettings = System.IO.Path.Combine(GetDdpmUserSettingsDir(), _dirName_EA);
        //    }
        //    return _dir_eaMonitorSettings;
        //}

        //Robert_Lin, 2024-9-3 Unused
        /// <summary>
        /// Make sure the EA directory exist before save/load EA settings.
        /// Call this method at init stage of SettingsManagerPlugin
        /// EA Dir: %LocalAppData%\Dell Display and Peripheral Manager\EA
        /// Or after expanded: "C:\Users\{UserName}\AppData\Local\Dell Display and Peripheral Manager\EA"
        /// </summary>
        //private bool EAMakeSureDirExist()
        //{
        //    try
        //    {
        //        DirectoryInfo di = System.IO.Directory.CreateDirectory(GetEaUserSettingsDir());
        //    }
        //    catch (Exception e1)
        //    {
        //        WriteLog($"CreateDirectory({GetEaUserSettingsDir()}) exception: {e1.Message}");
        //        return false;
        //    }
        //    return true;
        //}

        #endregion EasyArrange Settings

        #region powerNap settings

        private string RunSerializeObject(List<PowerNapSetting> powerNapSettings, string filePath)
        {

            string jsonString = string.Empty;
            jsonString = JsonConvert.SerializeObject(powerNapSettings);
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                writer.Write(jsonString);
            }

            //0905 Elsa Add Security
            string FileInfo;
            if (!DDPMFileSecurity.IsFilePathValid(filePath, out FileInfo))
            {
                _log.Info($"{nameof(RunSerializeObject)} {FileInfo}");
                return string.Empty;
            }
            return jsonString;
        }

        private string RunSerializeObject(List<PowerNapSetting> powerNapSettings)
        {
            string jsonString = string.Empty;
            jsonString = JsonConvert.SerializeObject(powerNapSettings);
            string jsonpath = _powerNapsettings_path;
            using (StreamWriter writer = new StreamWriter(jsonpath))
            {
                writer.Write(jsonString);
            }

            return jsonString; ;
        }

        private List<PowerNapSetting> RunPowerNapDeserializeObject(string value)
        {
            List<PowerNapSetting> retList = new List<PowerNapSetting>();

            try
            {
                retList = JsonConvert.DeserializeObject<List<PowerNapSetting>>(value);
            }
            catch (Exception)
            {
                return retList;
            }

            return retList;
        }

        #endregion powerNap settings

        #region hotkey settings

        private string RunSerializeObject(List<HotkeySettings> hotkeySettings)
        {
            string jsonString = string.Empty;
            jsonString = JsonConvert.SerializeObject(hotkeySettings);
            string jsonpath = _hotkeysettings_path;
            using (StreamWriter writer = new StreamWriter(jsonpath))
            {
                writer.Write(jsonString);
            }

            return jsonString; ;
        }

        private List<HotkeySettings> RunHotkeyDeserializeObject(string value)
        {
            List<HotkeySettings> retList = new List<HotkeySettings>();

            try
            {
                retList = JsonConvert.DeserializeObject<List<HotkeySettings>>(value);
            }
            catch (Exception)
            {
                return retList;
            }

            return retList;
        }

        #endregion hotkey settings

        #region ColorPreset settings

        private string RunSerializeObject(List<ColorPresetSettings> colorPresetSettings)//MonitorInfo mi, List<ColorPresetSettings> colorPresetSettings)
        {
            string jsonString = string.Empty;
            //ColorPresetSettings cp = new ColorPresetSettings();
            //cp.DeviceInfo = mi.edid;
            //cp.RunType = 1;
            //cp.AppInfo = AppPresetSettings;
            //
            //List<ColorPresetSettings> colorPresetSettings = new List<ColorPresetSettings>();
            //colorPresetSettings.Add(cp);
            //colorPresetSettings.Add(cp);

            jsonString = JsonConvert.SerializeObject(colorPresetSettings);
            string jsonpath = _colorsettings_path;// GetMonitorColorPresetJsonPath();
            using (StreamWriter writer = new StreamWriter(jsonpath))
            {
                writer.Write(jsonString);
            }

            return jsonString; ;
        }

        private List<ColorPresetSettings> RunDeserializeObject(string value)
        {
            List<ColorPresetSettings> retList = new List<ColorPresetSettings>();

            try
            {
                retList = JsonConvert.DeserializeObject<List<ColorPresetSettings>>(value);
            }
            catch (Exception)
            {
                return retList;
            }

            return retList;
        }

        #endregion ColorPreset settings

        #region Monitor Settings

        private Dictionary<string, List<DDPMMonitorSettings>> ReadAllMonitorSettings()
        {
            string[] files = Directory.GetFiles(_display_path, "*.json");

            return _AllMonitorSettings;
        }

        private string RunSerializeObject(string modelname, List<DDPMMonitorSettings> monitorSettings)
        {
            string jsonString = string.Empty;
            string monitorSettings_path = _display_path + "\\" + modelname + ".json";
            jsonString = JsonConvert.SerializeObject(monitorSettings);
            using (StreamWriter writer = new StreamWriter(monitorSettings_path))
            {
                writer.Write(jsonString);
            }

            return jsonString;
        }

        private List<DDPMMonitorSettings> RunMonitorListDeserializeObject(string value)
        {
            List<DDPMMonitorSettings> monitorSettingsList = new List<DDPMMonitorSettings>();

            try
            {
                monitorSettingsList = JsonConvert.DeserializeObject<List<DDPMMonitorSettings>>(value);
            }
            catch (Exception)
            {
                ;
            }

            return monitorSettingsList;
        }

        private DDPMImpExpSettings RunImpExpSettingsDeserializeObject(string value)
        {
            DDPMImpExpSettings ImpSettings = new DDPMImpExpSettings();

            try
            {
                ImpSettings = JsonConvert.DeserializeObject<DDPMImpExpSettings>(value);
                WriteLog($"Model: " + ImpSettings.MonitorSettings.Model);
                WriteLog($"ServiceTag: " + ImpSettings.MonitorSettings.ServiceTag);
            }
            catch (Exception)
            {
                ;
            }

            return ImpSettings;
        }

        #endregion Monitor Settings

        #region ImpExpSettings

        private string RunSerializeObject(string path, DDPMImpExpSettings impExpSettings)
        {
            string jsonString = string.Empty;
            jsonString = JsonConvert.SerializeObject(impExpSettings);
            using (StreamWriter writer = new StreamWriter(path))
            {
                writer.Write(jsonString);
            }

            return jsonString;
        }

        private DDPMImpExpSettings RunImpExpDeserializeObject(string value)
        {
            DDPMImpExpSettings impexpSettings = new DDPMImpExpSettings();

            try
            {
                impexpSettings = JsonConvert.DeserializeObject<DDPMImpExpSettings>(value);
            }
            catch (Exception)
            {
                ;
            }

            return impexpSettings;
        }

        private bool WriteImpExpSettings(string path, DDPMImpExpSettings impexpSettings)
        {
            if (impexpSettings == null)
            {
                return false;
            }
            //JArray jArray = new JArray();
            //jArray.Add(JObject.FromObject(monitorSettings));
            string str = RunSerializeObject(path, impexpSettings);
            if (string.IsNullOrEmpty(str))
            {
                return false;
            }
            //string info;
            //if (!DDPMFileSecurity.SetJsonContentFromSerializedString(jArray.ToString(), monitorSettings_path, out info))//, false))
            //{
            //    WriteLog(info);
            //    return Task.FromResult(false);
            //}

            return true;
        }

        private DDPMImpExpSettings ReadImportSettingsFile(string path)
        {
            DDPMImpExpSettings ImpSettings = new DDPMImpExpSettings();
            if (!string.IsNullOrEmpty(path))
            {
                if (File.Exists(path))
                {
                    string strReadJson = string.Empty;
                    using (var reader = new StreamReader(path))
                    {
                        strReadJson = reader.ReadToEnd();
                    }

                    if (strReadJson == string.Empty || strReadJson.Length == 0)
                        return ImpSettings;
                    try
                    {
                        //string info;
                        //string output = DDPMFileSecurity.GetSerializedJsonString(monitorSettings_path, out info);//, false);
                        WriteLog($"strReadJson: " + strReadJson);
                        ImpSettings = RunImpExpDeserializeObject(strReadJson);
                    }
                    catch (Exception)
                    {
                        ;
                    }
                }
            }
            return ImpSettings;
        }

        #endregion ImpExpSettings

        #region Global settings
        public Task<GlobalSettingParam> ReadGlobalSettings()
        {
            string strFilePath = _GlobalSetting_path;
            if (File.Exists(strFilePath))
            {
                string strReadJson = string.Empty;
                using (var reader = new StreamReader(strFilePath))
                {
                    strReadJson = reader.ReadToEnd();
                }
                if (strReadJson == string.Empty || strReadJson.Length == 0)
                {
                    return Task.FromResult(_GlobalSettingParam);
                }
                try
                {
                    _GlobalSettingParam = RunGlobalSettinDeserializeObject(strReadJson);
                }
                catch (Exception)// ex)
                {
                    return Task.FromResult(_GlobalSettingParam);
                }
            }
            else
            {
                File.Create(strFilePath).Close();
            }
            return Task.FromResult(_GlobalSettingParam);
        }
        public Task<bool> WriteGlobalSettings(GlobalSettingParam globalSettingParam)
        {
            if (globalSettingParam == null)
            {
                return Task.FromResult(false);
            }
            string temp = RunSerializeObject(globalSettingParam);
            if (!string.IsNullOrWhiteSpace(temp))
            {
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }
        private string RunSerializeObject(GlobalSettingParam globalSettingParam, string filePath)
        {
            string jsonString = string.Empty;
            jsonString = JsonConvert.SerializeObject(globalSettingParam);
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                writer.Write(jsonString);
            }
            return jsonString;
        }
        private string RunSerializeObject(GlobalSettingParam globalSettingParam)
        {
            string jsonString = string.Empty;
            jsonString = JsonConvert.SerializeObject(globalSettingParam);
            string jsonpath = _GlobalSetting_path;
            using (StreamWriter writer = new StreamWriter(jsonpath))
            {
                writer.Write(jsonString);
            }
            return jsonString;
        }
        private GlobalSettingParam RunGlobalSettinDeserializeObject(string value)
        {
            GlobalSettingParam retList = new GlobalSettingParam();
            try
            {
                retList = JsonConvert.DeserializeObject<GlobalSettingParam>(value);
            }
            catch (Exception)
            {

            }
            return retList;
        }
        #endregion Global settings

        private DDPMSettings InitDDPMUserConfigFile()
        {
            string folder = GetActiveUserLocalAppDataPath();
            WriteLog($"GetActiveUserLocalAppDataPath: {folder}");
            string folder_appdatapath_ddpm = folder + "\\" + folder_product;
            WriteLog($"folder_appdatapath_ddpm: {folder_appdatapath_ddpm}");
            try
            {
                DirectoryInfo di = System.IO.Directory.CreateDirectory(folder_appdatapath_ddpm);
                WriteLog($"create folder {folder_appdatapath_ddpm} success");
            }
            catch
            {
                WriteLog($"CreateDirectory with {folder_appdatapath_ddpm} failed.");
                _settings = null;
                return null;
            }
            string file_appdatapath_userconfig = folder_appdatapath_ddpm + "\\" + filename_appsettings_peruser;
            _settings_path = file_appdatapath_userconfig;
            WriteLog($"_settings_path is {_settings_path}.");

            DDPMAppSettings ddpm_app = new DDPMAppSettings();
            DDPMUserSettings ddpm_user = new DDPMUserSettings();
            string info;
            if (File.Exists(file_appdatapath_userconfig))
            {
                // DDPMSettings.getSettingsforImport(file_appdatapath_userconfig, ref ddpm_app, ref ddpm_user);
                string serialized_string = DDPMFileSecurity.GetSerializedJsonString(_settingsAccessInfo, file_appdatapath_userconfig, out info);//, false);
                if (!string.IsNullOrEmpty(serialized_string))
                    _settings = JsonConvert.DeserializeObject<DDPMSettings>(serialized_string);
                else
                {
                    WriteLog("[InitDDPMUserConfigFile] GetSerializedJsonString: " + info);
                    _settings = new DDPMSettings(new DDPMAppSettings(), new DDPMUserSettings(), new DDPMITConfig());
                    if (_settings != null)
                    {
                        WriteLog("[InitDDPMUserConfigFile] *** Init cache from file fail, re-create default settings to file");
                        if (DDPMFileSecurity.SetJsonContentFromSerializedString(_settingsAccessInfo, JObject.FromObject(_settings).ToString(), file_appdatapath_userconfig, out info))
                            WriteLog("[InitDDPMUserConfigFile] re-create file content OK");
                        else
                            WriteLog("[InitDDPMUserConfigFile] save to file failed, please check file access right!!");
                    }
                }
            }
            else
            {
                FileInfo fileInfo = new FileInfo(file_appdatapath_userconfig);

                WriteLog("[InitDDPMUserConfigFile] settings file not exist, new an object");
                _settings = new DDPMSettings(new DDPMAppSettings(), new DDPMUserSettings(), new DDPMITConfig());
                //init data to file
                if (SetAppConfigData(_settings).Result)
                {
                    WriteLog("[InitDDPMUserConfigFile] settings file create and write success");
                }
                else
                {
                    WriteLog("[InitDDPMUserConfigFile] settings file create and write failed");
                }
            }
            //ACL apply
            //string info;
            if (!DDPMFileSecurity.ApplyFileACLNormalUser(file_appdatapath_userconfig, out info))
                WriteLog($"[InitDDPMUserConfigFile] {info}");

            return _settings;
        }

        private List<ColorPresetSettings> InitColorPresetConfigFile()
        {
            string folder = GetActiveUserLocalAppDataPath();
            WriteLog($"GetActiveUserLocalAppDataPath: {folder}");
            string folder_appdatapath_colorpreset = folder + "\\" + folder_product;

            //create app list folder if not exist
            string folder_applist_path = folder_appdatapath_colorpreset + "\\" + folder_localappdata_Applist;
            try
            {
                DirectoryInfo di = System.IO.Directory.CreateDirectory(folder_applist_path);
                WriteLog($"create folder {folder_applist_path} success");
            }
            catch
            {
                WriteLog($"CreateDirectory with {folder_applist_path} failed.");
                _colorPresetSettings = null;
                return null;
            }
            //create app icon folder if not exist
            string folder_appicon_path = folder_applist_path + "\\" + folder_localappdata_Appicon;
            try
            {
                DirectoryInfo di = System.IO.Directory.CreateDirectory(folder_appicon_path);
                WriteLog($"create folder {folder_appicon_path} success");
                _appiconfolder_path = folder_appicon_path;
            }
            catch
            {
                WriteLog($"CreateDirectory with {folder_appicon_path} failed.");
                //_appiconfolder_path = "C:\\ProgramData\\Dell Display and Peripheral Manager\\Icons";
                _colorPresetSettings = null;
                return null;
            }

            //create colorpreset setting file if not exist
            string file_colorconfig_path = folder_applist_path + "\\" + filename_colorpreset_peruser;
            _colorsettings_path = file_colorconfig_path;
            WriteLog($"_colorsettings_path is {_colorsettings_path}.");

            List<ColorPresetSettings> colorPresetSettings = new List<ColorPresetSettings>();

            if (File.Exists(_colorsettings_path))
                _colorPresetSettings = ReadColorPresetSettings().Result;
            else
            {
                FileInfo fileInfo = new FileInfo(_colorsettings_path);

                WriteLog("[InitColorPresetConfigFile] settings file not exist, new an object");
                _colorPresetSettings = colorPresetSettings;
                //init data to file
                if (WriteColorPresetSettings(_colorPresetSettings).Result)
                {
                    WriteLog("[InitColorPresetConfigFile] colorpreset settings file create and write success");
                }
                else
                {
                    WriteLog("[InitColorPresetConfigFile] colorpreset settings file create and write failed");
                }
            }
            //ACL function to check exist rule and apply rule if not exist
            string info;
            if (!DDPMFileSecurity.ApplyFileACLNormalUser(_colorsettings_path, out info))
                WriteLog($"[InitDDPMUserConfigFile] {info}");

            return _colorPresetSettings;
        }

        private List<HotkeySettings> InitHotkeyConfigFile()
        {
            string folder = GetActiveUserLocalAppDataPath();
            WriteLog($"GetActiveUserLocalAppDataPath: {folder}");
            string folder_appdatapath_hotkey = folder + "\\" + folder_product;

            //create app list folder if not exist
            string folder_applist_path = folder_appdatapath_hotkey;
            try
            {
                if (!Directory.Exists(folder_applist_path))
                {
                    DirectoryInfo di = System.IO.Directory.CreateDirectory(folder_applist_path);
                    WriteLog($"create folder {folder_applist_path} success");
                }
            }
            catch
            {
                WriteLog($"CreateDirectory with {folder_applist_path} failed.");
                _hotkeySettings = null;
                return null;
            }
            //create hotkey setting file if not exist
            string file_hotkeyconfig_path = folder_applist_path + "\\" + filename_hotkey_peruser;
            _hotkeysettings_path = file_hotkeyconfig_path;
            WriteLog($"_hotkeysettings_path is {_hotkeysettings_path}.");

            List<HotkeySettings> hotkeySettings = new List<HotkeySettings>();

            if (File.Exists(_hotkeysettings_path))
                hotkeySettings = ReadHotkeySettings().Result;
            else
            {
                FileInfo fileInfo = new FileInfo(_hotkeysettings_path);
                fileInfo.Create().Close();
                WriteLog("[InitHotkeyConfigFile] hotkey settings file not exist, new an object");
                _hotkeySettings = hotkeySettings;
                //init data to file
                if (WriteHotkeySettings(_hotkeySettings).Result)
                {
                    WriteLog("[InitHotkeyConfigFile] hotkey settings file create and write success");
                }
                else
                {
                    WriteLog("[InitHotkeyConfigFile] hotkey settings file create and write failed");
                }
            }
            //ACL function to check exist rule and apply rule if not exist
            string info;
            if (!DDPMFileSecurity.ApplyFileACLNormalUser(_hotkeysettings_path, out info))
                WriteLog($"[InitDDPMUserConfigFile] {info}");

            return _hotkeySettings;
        }

        private List<PowerNapSetting> InitPowerNapConfigFile()
        {
            string folder = GetActiveUserLocalAppDataPath();
            WriteLog($"GetActiveUserLocalAppDataPath: {folder}");
            string folder_appdatapath_powernap = folder + "\\" + folder_product;

            //create app list folder if not exist
            string folder_applist_path = folder_appdatapath_powernap;
            try
            {
                if (!Directory.Exists(folder_applist_path))
                {
                    DirectoryInfo di = System.IO.Directory.CreateDirectory(folder_applist_path);
                    WriteLog($"create folder {folder_applist_path} success");
                }
            }
            catch
            {
                WriteLog($"CreateDirectory with {folder_applist_path} failed.");
                _powerNapSettings = null;
                return null;
            }
            //create powernap setting file if not exist
            string file_powernapconfig_path = folder_applist_path + "\\" + filename_powernap_peruser;
            _powerNapsettings_path = file_powernapconfig_path;
            WriteLog($"_powerNapsettings_path is {_powerNapsettings_path}.");

            List<PowerNapSetting> powerNapSettings = new List<PowerNapSetting>();

            if (File.Exists(_powerNapsettings_path))
                powerNapSettings = ReadPowerNapSettings().Result;
            else
            {
                FileInfo fileInfo = new FileInfo(_powerNapsettings_path);
                fileInfo.Create().Close();
                WriteLog("[InitPowerNapConfigFile] settings file not exist, new an object");
                _powerNapSettings = powerNapSettings;
                //init data to file
                if (WritePowerNapSettings(_powerNapSettings).Result)
                {
                    WriteLog("[InitPowerNapConfigFile] PowerNap settings file create and write success");
                }
                else
                {
                    WriteLog("[InitPowerNapConfigFile] PowerNap settings file create and write failed");
                }
            }
            //ACL function to check exist rule and apply rule if not exist
            string info;
            if (!DDPMFileSecurity.ApplyFileACLNormalUser(_powerNapsettings_path, out info))
                WriteLog($"[InitPowerNapConfigFile] {info}");

            return _powerNapSettings;
        }

        private GlobalSettingParam InitGlobalSettingConfigFile()
        {
            string folder = GetActiveUserLocalAppDataPath();
            WriteLog($"GetActiveUserLocalAppDataPath: {folder}");
            string folder_appdatapath = folder + "\\" + folder_product;

            //create app list folder if not exist
            string folder_applist_path = folder_appdatapath;
            try
            {
                if (!Directory.Exists(folder_applist_path))
                {
                    DirectoryInfo di = System.IO.Directory.CreateDirectory(folder_applist_path);
                    WriteLog($"create folder {folder_applist_path} success");
                }
            }
            catch
            {
                WriteLog($"CreateDirectory with {folder_applist_path} failed.");
                _powerNapSettings = null;
                return null;
            }
            //create powernap setting file if not exist
            string file_path = folder_applist_path + "\\" + filename_GlobalSetting_peruser;
            _GlobalSetting_path = file_path;
            WriteLog($"_GlobalSetting_path is {_GlobalSetting_path}.");


            if (File.Exists(_GlobalSetting_path))
                _GlobalSettingParam = ReadGlobalSettings().Result;
            else
            {
                FileInfo fileInfo = new FileInfo(_GlobalSetting_path);
                fileInfo.Create().Close();
                WriteLog("[InitGlobalSettingConfigFile] settings file not exist, new an object");
                //init data to file
                if (WriteGlobalSettings(_GlobalSettingParam).Result)
                {
                    WriteLog("[InitGlobalSettingConfigFile] PowerNap settings file create and write success");
                }
                else
                {
                    WriteLog("[InitGlobalSettingConfigFile] PowerNap settings file create and write failed");
                }
            }
            //ACL function to check exist rule and apply rule if not exist
            string info;
            if (!DDPMFileSecurity.ApplyFileACLNormalUser(_GlobalSetting_path, out info))
                WriteLog($"[InitGlobalSettingConfigFile] {info}");

            return _GlobalSettingParam;
        }

        #region Registry key read/write over system setting manager (only support local machine)
        /*
          Example:
            string keyPath = @"SOFTWARE\MyApp";
            string keyName = "MySetting";
            string keyValue = "Hello, Registry!";
            int keyValue2 = 2;

            // Write to registry key in LocalMachine
            WriteRegistryData(RegistryHive.LocalMachine, keyPath, keyName, keyValue2);
            WriteLog("Registry key value updated in LocalMachine.");

            // Read registry key from LocalMachine
            object value = ReadRegistryData(RegistryHive.LocalMachine, keyPath, keyName).Result;
            switch (value)
            {
                case int intValue:
                    WriteLog("Integer value: " + intValue);
                    break;
                case string stringValue:
                    WriteLog("String value: " + stringValue);
                    break;
                case byte[] byteArray:
                    WriteLog("Byte array value: " + BitConverter.ToString(byteArray));
                    break;
                case string[] stringArray:
                    WriteLog("String array value: " + string.Join(", ", stringArray));
                    break;
                case long longValue:
                    WriteLog("Long value: " + longValue);
                    break;
                default:
                    WriteLog("Unknown type: " + value.GetType());
                    break;
            }
         */
        public Task<object> ReadRegistryData(RegistryHive hive, string keyPath, string keyName)
        {
            try
            {
                if (hive == RegistryHive.LocalMachine || hive == RegistryHive.CurrentUser)
                {
                    if (_SysSettingsPlugin == null)
                    {
                        WriteLog($"[User setting plugin] null system settings plugin, can't access local machine registry");
                        return null;
                    }
                    object obj = _SysSettingsPlugin.ReadRegistryData(hive, keyPath, keyName).Result;
                    if (obj != null)
                        WriteLog($"[User setting plugin] Read data success");
                    else
                        WriteLog($"[User setting plugin] Read data failed");
                    return Task.FromResult(obj);
                }
                else
                {
                    WriteLog($"[User setting plugin] WARNING: un-defined registry hive ({hive})");
                    return null;
                }
            }
            catch (Exception e)
            {
                WriteLog($"[User setting plugin] WARNING: read registry cause exception ({e.Message})");
                return null;
            }
        }

        public Task<bool> WriteRegistryData(RegistryHive hive, string keyPath, string keyName, object value)
        {
            try
            {
                if (hive == RegistryHive.LocalMachine || hive == RegistryHive.CurrentUser)
                {
                    if (_SysSettingsPlugin == null)
                    {
                        WriteLog($"[User setting plugin] null system settings plugin, can't access local machine registry");
                        return Task.FromResult(false);
                    }
                    bool result = _SysSettingsPlugin.WriteRegistryData(hive, keyPath, keyName, value).Result;
                    if(result)
                        WriteLog($"[User setting plugin] Write data {value} success");
                    else
                        WriteLog($"[User setting plugin] Write data {value} failed");
                    return Task.FromResult(result);
                }
                else
                {
                    WriteLog($"[User setting plugin] WARNING: un-defined registry hive ({hive})");
                    return Task.FromResult(false);
                }
            }
            catch (Exception e)
            {
                WriteLog($"[User setting plugin] WARNING: write registry cause exception ({e.Message})");
                return Task.FromResult(false);
            }
        }
        #endregion
    }
}