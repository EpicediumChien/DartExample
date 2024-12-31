using DDPM.SA.Common;
using DDPM.SA.Common.Display;
using DDPM.SA.Common.Settings;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.Common.Annotations;
using Dell.Client.Framework.Common.PluginConditions;
using Dell.Client.Framework.Interfaces;
using Microsoft;
using Microsoft.VisualBasic.Logging;
using MS.WindowsAPICodePack.Internal;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Linq.Expressions;
using Windows.Devices.Bluetooth.Background;
using Windows.Web.Http;
using DDPM.SA.Obfuscation;
using System.Security.Cryptography;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Windows.Media.AppBroadcasting;
using DdmLibrary.Utility;
using System.Globalization;

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
        private const string publisherCompany = "Dell Inc.";
        private const string publisherWebsite = "https://www.dell.com";
        private const string publisherSupport = "This plugin implements User.SettingsManager.Plugin.";
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
        private static string folder_localappdata_Applist = "AppLibrary";
        private static string folder_localappdata_Appicon = "Icons";
        private static string folder_localappdata_Display = "Display";
        private static string folder_localappdata_Migration = "Migration";
        private static string folder_localappdata_Export = "Export";
        private static string filename_appsettings_peruser = "DDPM.Configs.json";

        private static string filename_colorpreset_peruser = "ColorSetting.json";
        private static string filename_hotkey_peruser = "HotkeySetting.json";
        private static string filename_powernap_peruser = "PowerNapSetting.json";
        private static string filename_GlobalSetting_peruser = "GlobalSetting.json";
        private static string filename_InterruptScreen_peruser = "InterruptScreen.json";


        //---
        private ISettingsManagerSA? _SysSettingsPlugin;

        private readonly object _PluginConditionLock_SysSettings = new object();
        private bool relay_registered = false;

        private readonly object _Setting_Locker = new object();

        private DDPMSettings _settings { get; set; }
        private string _settings_path { get; set; } = string.Empty;
        private List<ColorPresetSettings> _colorPresetSettings { get; set; }
        private string _colorsettings_path { get; set; } = string.Empty;
        private string _appiconfolder_path { get; set; } = string.Empty;
        private Dictionary<string, InstalledAppInfo> _AllAppData = new Dictionary<string, InstalledAppInfo>();
        private Dictionary<string, List<DDPMMonitorSettings>>? _AllMonitorSettings = new Dictionary<string, List<DDPMMonitorSettings>>();
        private string _display_path { get; set; } = string.Empty;
        private string _export_path { get; set; } = string.Empty;
        private List<ColorPresetSettings> _preset_settings = new List<ColorPresetSettings>();//Dean 0626 fix SAST issue

        private List<HotkeySettings> _hotkeySettings { get; set; }
        private string _hotkeysettings_path { get; set; } = string.Empty;
        //private static List<HotkeySettings> _present_hotkey_settings = new List<HotkeySettings>();

        //private List<PowerNapSetting> _powerNapSettings { get; set; }
        //private string _powerNapsettings_path { get; set; } = string.Empty;
        //private static List<PowerNapSetting> _present_powerNap_settings = new List<PowerNapSetting>();
        //private static string _settingsAccessInfo = string.Empty;
        private static string _settingsAccessInfoVer = string.Empty;
        private static string _settingsAccessInfoAddr = string.Empty;

        private string _GlobalSetting_path { get; set; } = string.Empty;
        private GlobalSettingParam _GlobalSettingParam = new GlobalSettingParam();
        public event EventHandler SettingReadyEvent;

        private string _InterruptScreen_path { get; set; } = string.Empty;
        private InterruptScreenRoot _InterruptScreenParam = new InterruptScreenRoot();

        private static DDPMITConfig _DDPMITConfig { get; set; } = new DDPMITConfig();

        private static bool _isAllSettingsReady = false;
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
        private void WriteLog(string text, log_type log_type = log_type.info,
            [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
            [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
            [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0)
        {
            if (string.IsNullOrEmpty(text))
                text = "";

            text = $"[User.SettingsManager] {text}, Caller Name:{memberName}, Source Line {sourceLineNumber}";
            Console.WriteLine(text);
            if (Log != null)
            {
                if (log_type == log_type.info)
                    Log.Info(text);
                else
                    Log.Error(text);
            }
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

            //_settingsAccessInfo = _SysSettingsPlugin.QueryAccessInfo().Result;
            _settingsAccessInfoVer = _SysSettingsPlugin.QueryAccessInfoVer().Result;
            _settingsAccessInfoAddr = _SysSettingsPlugin.QueryAccessInfoAddr().Result;
            InitDDPMUserConfigFile();
            InitColorPresetConfigFile();
            InitHotkeyConfigFile();
            //InitPowerNapConfigFile();
            InitGlobalSettingConfigFile();
            InitInterruptScreenFile();

            updateITandGlobalSetting();

            SettingReadyEvent?.Invoke(this, new EventArgs());
            _isAllSettingsReady = true;
        }

        /*private void HMAC_Secret_Test()
        {
            DateTimeOffset utcNow = DateTimeOffset.UtcNow;
            string strRandom = SettingsAccess.GenerateReferenceInfo();
            string strTicket = SettingsAccess.GenerateReferenceTicket(utcNow);
            string strTicketToFile = utcNow.ToString();

            GlobalSettingParam temp = new GlobalSettingParam();

            string content = string.Empty;
            JToken token = JToken.FromObject(temp);
            if (token.Type == JTokenType.Object)
            {
                JObject obj = (JObject)token;
                // Handle object
                content = obj.ToString();
            }
            else if (token.Type == JTokenType.Array)
            {
                JArray array = (JArray)token;
                // Handle array
                content = array.ToString();
            }

            string signature = SettingsAccess.GenerateSignature(strTicket, strRandom, content);
            bool result = SettingsAccess.VerifySignature(strTicketToFile, strRandom, content, signature);
            Console.WriteLine($"The comparison result is {result}");
        }*/

        public Task<bool> QuerySettingsStatus()
        {
            return Task.FromResult(_isAllSettingsReady);
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

        private void updateITandGlobalSetting()
        {
            if (_SysSettingsPlugin == null)
                return;
            try
            {
                DDPMITConfig tmp = _SysSettingsPlugin.GetITGlobalConfigs().Result;
                if (tmp != null)
                {
                    _DDPMITConfig = tmp;
                    if (tmp.global_setting != null)
                    {
                        _GlobalSettingParam = _DDPMITConfig.global_setting;
                        WriteGlobalSettings(_GlobalSettingParam, false);
                    }
                }
            }
            catch (Exception ex)
            {
                //report exception and keep last data
                WriteLog($"[updateITandGlobalSetting] exception: {ex.Message}");
            }

            if (_DDPMITConfig == null)
                _DDPMITConfig = new DDPMITConfig();

            if (_GlobalSettingParam == null)
            {
                if (_DDPMITConfig == null)
                    _GlobalSettingParam = new GlobalSettingParam();
                else if (_DDPMITConfig.global_setting == null)
                    _GlobalSettingParam = new GlobalSettingParam();
                else
                    _GlobalSettingParam = _DDPMITConfig.global_setting;
            }
        }

        #region ISettingManagerDev implementation

        public event EventHandler<ITSettingEventArgs> ITSettingsActionEvent;

        //Event from SettingsManagerPlugin.cs and bypass to subscriber
        private void OnITSettingsActionEventNotify(ITSettingEventArgs e)
        {
            if (ITSettingsActionEvent == null || e == null || e == EventArgs.Empty)
                return;

            updateITandGlobalSetting();


            EventHandler<ITSettingEventArgs> Handler = ITSettingsActionEvent;
            if (Handler != null)
            {
                Handler.Invoke(this, e);
                WriteLog($"[User Settings plugin] ITSettingsActionEvent Invoked to subscriber");
            }
        }

        /// <summary>
        /// InitDDPMMonitorConfigFile: Query content of monitor settings file with model name
        /// </summary>
        /// <param name="modelname">IN: key item to check which model should be used to query</param>
        /// <param name="binit">OUT: request to init a new monitor data if true, otherwise means query success</param>
        /// <returns></returns>
        public Task<List<DDPMMonitorSettings>> InitDDPMMonitorConfigFile(string modelname, out bool binit)
        {
            binit = false; //default: do not request to init a new data
            List<DDPMMonitorSettings> monitorSettingList = new List<DDPMMonitorSettings>();
            //if (!relay_registered && _SysSettingsPlugin != null)
            //{
            string folder = GetActiveUserLocalAppDataPath();
            WriteLog($"GetActiveUserLocalAppDataPath: {folder}");
            string folder_appdatapath_display = folder + "\\" + folder_product + "\\" + folder_localappdata_Display;
            string folder_appdatapath_export = folder + "\\" + folder_product + "\\" + folder_localappdata_Export;
            //create display folder if not exist
            _display_path = folder_appdatapath_display;
            _export_path = folder_appdatapath_export;
            string folderInfo = string.Empty, info = string.Empty;
            try
            {
                if (!Directory.Exists(_display_path))
                {
                    DirectoryInfo di = System.IO.Directory.CreateDirectory(_display_path);
                    WriteLog($"create folder {_display_path} success");
                }
                if (!DDPMFileSecurity.CheckFold(_display_path, out folderInfo, out info))
                {
                    WriteLog($"CreateDirectory path check result is invalid: {_display_path}, {info}.");
                    _AllMonitorSettings = null;
                    binit = true; //request to create new data
                    return Task.FromResult(monitorSettingList);
                }
            }
            catch
            {
                WriteLog($"CreateDirectory with {_display_path} failed.");
                _AllMonitorSettings = null;
                binit = true; //request to create new data
                return Task.FromResult(monitorSettingList);
            }
            try
            {
                if (!Directory.Exists(_export_path))
                {
                    DirectoryInfo di = System.IO.Directory.CreateDirectory(_export_path);
                    WriteLog($"create folder {_export_path} success");
                }
                if (!DDPMFileSecurity.CheckFold(_export_path, out folderInfo, out info))
                {
                    WriteLog($"CreateDirectory path check result is invalid: {_export_path}, {info}.");
                    _AllMonitorSettings = null;
                    binit = false; 
                    return Task.FromResult(monitorSettingList);
                }
            }
            catch
            {
                WriteLog($"CreateDirectory with {_export_path} failed.");
                _AllMonitorSettings = null;
                binit = false; 
                return Task.FromResult(monitorSettingList);
            }
            //create monitor setting file if not exist
            string file_monitorconfig_path = _display_path + "\\" + modelname + ".json";
            WriteLog($"_monitorSettings_path is {file_monitorconfig_path}.");
            if (File.Exists(file_monitorconfig_path))
            {
                monitorSettingList = ReloadMonitorSettings(modelname).Result;
                if (monitorSettingList != null)
                {
                    if (_AllMonitorSettings == null)
                        _AllMonitorSettings = new Dictionary<string, List<DDPMMonitorSettings>>();
                    //find monitor settings
                    if (_AllMonitorSettings.ContainsKey(modelname))
                    {
                        _AllMonitorSettings[modelname] = monitorSettingList;
                        binit = false; //has data, no need to re-create
                    }
                    else
                    {
                        if (monitorSettingList.Count > 0)
                        {
                            _AllMonitorSettings.Add(modelname, monitorSettingList);
                            binit = false;
                        }
                        else
                            binit = true;
                    }                    
                }
                else
                {
                    //Dean 1230 do not force add a empty data since there are multiple objects in it
                    //_AllMonitorSettings = new Dictionary<string, List<DDPMMonitorSettings>>();
                    //_AllMonitorSettings.Add(modelname, monitorSettingList);
                    binit = true; 
                }
            }
            else
            {
                //FileInfo fileInfo = new FileInfo(file_monitorconfig_path);
                //fileInfo.Create().Close();
                //WriteLog("[InitMonitorConfigFile] settings file not exist, new an object");
                ////init data to file
                //if (WriteMonitorSettings(modelname, monitorSettingList).Result)
                //{
                //    WriteLog("[InitMonitorConfigFile] Monitor settings file create and write success");
                //}
                //else
                //{
                //    WriteLog("[InitMonitorConfigFile] Monitor settings file create and write failed");                    
                //}
                monitorSettingList = null;
                WriteLog("[InitMonitorConfigFile] Monitor settings file didn't exist");
                binit = true;
            }
            //}
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
                            string output = DDPMFileSecurity.GetSerializedJsonString(_settings_path, out info);//, false);
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
                {
                    //Robert_Lin, 2024-10-20 add log to trace the user settings file can not be loaded at starting up
                    WriteLog($"@ReloadAppConfigData({force_reload}): _settings_path is empty, return default settings.");
                    _settings = new DDPMSettings(ddpm_app, ddpm_user, ddpm_it);
                }

                if (_settings != null)
                {
                    //0819 get IT config and apply it
                    if (_SysSettingsPlugin != null)
                    {
                        DDPMITConfig tmp = null;
                        try
                        {
                            tmp = _SysSettingsPlugin.GetITGlobalConfigs(force_reload).Result;
                            _DDPMITConfig = tmp;
                        }
                        catch (Exception e)
                        {
                            WriteLog($"[ReloadAppConfigData][_SysSettingsPlugin.GetITGlobalConfigs] exception: {e.Message}");
                        }
                        if (tmp != null)
                            _settings.LockSettings = tmp;
                    }
                }
                else
                {
                    _settings = new DDPMSettings(ddpm_app, ddpm_user, ddpm_it);
                    WriteLog($"[ReloadAppConfigData] allocate new object due to null data");
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
                if (!DDPMFileSecurity.SetJsonContentFromSerializedString(JObject.FromObject(_settings).ToString(), _settings_path, out info))//, false))
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
            if (!string.IsNullOrEmpty(_display_path))
            {
                string monitorSettings_path = _display_path + "\\" + modelname + ".json";
                if (File.Exists(monitorSettings_path))
                {
                    if (_AllMonitorSettings != null)
                    {
                        if (_AllMonitorSettings.ContainsKey(modelname))
                        {
                            monitorSettings = _AllMonitorSettings[modelname];
                            return Task.FromResult(monitorSettings);
                        }
                    }
                    string strReadJson = string.Empty;

                    /*try
                    {
                        using (var reader = new StreamReader(monitorSettings_path))
                        {
                            strReadJson = reader.ReadToEnd();
                        }
                    }
                    catch (Exception ex)
                    {
                        WriteLog("[ReloadMonitorSettings] exception, message: " + ex.Message);
                    }

                    if (strReadJson == string.Empty || strReadJson.Length == 0)
                        return Task.FromResult(monitorSettings);*/
                    try
                    {
                        string info = string.Empty;
                        strReadJson = DDPMFileSecurity.GetSerializedJsonString(monitorSettings_path, out info);//, false);
                        if(string.IsNullOrEmpty(strReadJson))
                        {
                            monitorSettings = null;
                            WriteLog($"[ReloadMonitorSettings][GetSerializedJsonString] return empty data: {info}");
                            ////force write back new data to replace file which has problem
                            //bool temp = WriteMonitorSettings(modelname, monitorSettings).Result;
                            //WriteLog($"[ReloadMonitorSettings][WriteMonitorSettings] replace by default data: result({temp})");

                            //Dean 1230 do not force write empty data since there is multiple objects should be inited before write data
                            return Task.FromResult(monitorSettings);
                        }
                        else
                            monitorSettings = RunMonitorListDeserializeObject(strReadJson);
                    }
                    catch (Exception e)
                    {
                        WriteLog($"[ReloadMonitorSettings] exception: {e.Message}");
                    }
                }
                else
                {
                    //if file not exist then create new with default values
                    WriteMonitorSettings(modelname, monitorSettings);
                    return Task.FromResult(monitorSettings);
                }
            }
            else
            {
                WriteLog("ReloadMonitorSettings, but _display_path is not exist");
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
            string str = RunSerializeObject_MonitorSettings(modelname, monitorSettings);
            if (string.IsNullOrEmpty(str))
            {
                WriteLog("[WriteMonitorSettings] fail with empty serialized string");
                return Task<bool>.FromResult(false);
            }
            string info;
            if (!DDPMFileSecurity.SetJsonContentFromSerializedString(str, monitorSettings_path, out info))//, false))
            {
                WriteLog($"[WriteMonitorSettings] fail with ({info})");
                return Task.FromResult(false);
            }
            if (_AllMonitorSettings == null)
            {
                _AllMonitorSettings = new Dictionary<string, List<DDPMMonitorSettings>>();
                WriteLog($"[WriteMonitorSettings] _AllMonitorSettings object is null, create it");
            }
            if (_AllMonitorSettings.ContainsKey(modelname))
            {
                _AllMonitorSettings[modelname] = monitorSettings;
                WriteLog($"[WriteMonitorSettings] _AllMonitorSettings has key {modelname}, update it");
            }
            else
            {
                _AllMonitorSettings.Add(modelname, monitorSettings);
                WriteLog($"[WriteMonitorSettings] _AllMonitorSettings no exist key, add it");
            }
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
            //_preset_settings.Clear();
            if (string.IsNullOrEmpty(_colorsettings_path))
                return Task.FromResult(_preset_settings);

            string strFilePath = _colorsettings_path;// GetMonitorColorPresetJsonPath();

            if (File.Exists(strFilePath))
            {
                string strReadJson = string.Empty;
                //using (var reader = new StreamReader(strFilePath))
                //{
                //    strReadJson = reader.ReadToEnd();
                //}
                string info;
                strReadJson = DDPMFileSecurity.GetSerializedJsonString(strFilePath, out info);

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

            string info = string.Empty;
            bool result = false;

            JToken token = JToken.FromObject(colorPresetSettings);
            if (token.Type == JTokenType.Object)
            {
                JObject obj = (JObject)token;
                // Handle object
                result = DDPMFileSecurity.SetJsonContentFromSerializedString(obj.ToString(), _colorsettings_path, out info);
            }
            else if (token.Type == JTokenType.Array)
            {
                JArray array = (JArray)token;
                // Handle array
                result = DDPMFileSecurity.SetJsonContentFromSerializedString(array.ToString(), _colorsettings_path, out info);
            }
            if(!result)
            {
                WriteLog($"[WriteColorPresetSettings] failed: {info}");
            }

            return Task.FromResult(result);
        }

        #endregion color preset settings

        #region hotkey settings

        public Task<List<HotkeySettings>> ReadHotkeySettings()
        {
            //_hotkeySettings?.Clear();
            if (string.IsNullOrEmpty(_hotkeysettings_path))
                _hotkeySettings = null;
            else
            {
                string strFilePath = _hotkeysettings_path;

                if (File.Exists(strFilePath))
                {
                    string strReadJson = string.Empty;
                    string info;
                    strReadJson = DDPMFileSecurity.GetSerializedJsonString(strFilePath, out info);

                    if (strReadJson == string.Empty || strReadJson.Length == 0)
                    {
                        _hotkeySettings = null;
                        return Task.FromResult(_hotkeySettings);
                    }
                    try
                    {
                        _hotkeySettings = RunHotkeyDeserializeObject(strReadJson);
                        //clean all null object
                        if (_hotkeySettings != null && _hotkeySettings.Count > 0)
                        {
                            int count = _hotkeySettings.Count;
                            for (int i = (count - 1); i >= 0; i--)
                            {
                                if (_hotkeySettings[i].ModelName == null ||
                                    _hotkeySettings[i].SerialNumber == null ||
                                    _hotkeySettings[i].ServiceTag == null)
                                    _hotkeySettings.RemoveAt(i);
                            }
                        }
                    }
                    catch (Exception)// ex)
                    {
                        _hotkeySettings = null;
                    }
                }
                else
                {
                    _hotkeySettings = null;
                }
            }
            return Task.FromResult(_hotkeySettings);

            /*string strFilePath = _hotkeysettings_path;// GetMonitorColorPresetJsonPath();

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

            return Task.FromResult(_present_hotkey_settings);*/
        }

        public Task<bool> WriteHotkeySettings(List<HotkeySettings> hotkeySettings)
        {
            /*if (hotkeySettings == null)
                return Task.FromResult(false);

            string info;
            bool result = false;

            JToken token = JToken.FromObject(hotkeySettings);
            if (token.Type == JTokenType.Object)
            {
                JObject obj = (JObject)token;
                // Handle object
                result = DDPMFileSecurity.SetJsonContentFromSerializedString(_settingsAccessInfo, obj.ToString(), _hotkeysettings_path, out info);
            }
            else if (token.Type == JTokenType.Array)
            {
                JArray array = (JArray)token;
                // Handle array
                result = DDPMFileSecurity.SetJsonContentFromSerializedString(_settingsAccessInfo, array.ToString(), _hotkeysettings_path, out info);
            }*/

            bool result = WriteSettings_Common(hotkeySettings, "hotkey");

            return Task.FromResult(result);
            /*if (hotkeySettings == null)
                return Task.FromResult(false);

            string temp = RunSerializeObject(hotkeySettings);
            if (!string.IsNullOrWhiteSpace(temp))
                return Task.FromResult(true);

            return Task.FromResult(false);*/
        }

        #endregion hotkey settings

        #region powerNap settings

        /*public Task<List<PowerNapSetting>> ReadPowerNapSettings()
        {
            _powerNapSettings?.Clear();
            if (string.IsNullOrEmpty(_powerNapsettings_path))
                _powerNapSettings = null;
            else
            {
                string strFilePath = _powerNapsettings_path;

                if (File.Exists(strFilePath))
                {
                    //string strReadJson = string.Empty;
                    //using (var reader = new StreamReader(strFilePath))
                    //{
                    //    strReadJson = reader.ReadToEnd();
                    //}

                    string strReadJson = string.Empty;
                    string info;
                    strReadJson = DDPMFileSecurity.GetSerializedJsonString(strFilePath, out info);

                    if (strReadJson == string.Empty || strReadJson.Length == 0)
                    {
                        _powerNapSettings = null;
                        return Task.FromResult(_powerNapSettings);
                    }
                    try
                    {
                        _powerNapSettings = RunPowerNapDeserializeObject(strReadJson);
                    }
                    catch (Exception)// ex)
                    {
                        _powerNapSettings = null;
                        return Task.FromResult(_powerNapSettings);
                    }
                }
                else
                {
                    //File.Create(strFilePath).Close();
                    _powerNapSettings = null;
                }
            }
            return Task.FromResult(_powerNapSettings);
        }*/

        /*public Task<bool> WritePowerNapSettings(List<PowerNapSetting> powerNapSettings)
        {
            bool result = WriteSettings_Common(powerNapSettings, "powernap");

            return Task.FromResult(result);
        }*/

        /*public Task<List<PowerNapSetting>> ImportPowerNapSettings(string filePath)
        {
            //Elsa Add Security
            string FileInfo;
            if (!DDPMFileSecurity.IsFilePathValid(filePath, out FileInfo))
            {
                WriteLog($"{nameof(ImportPowerNapSettings)} {FileInfo}");
                return Task.FromResult(_powerNapSettings);// _present_powerNap_settings);
            }

            string strReadJson = string.Empty;
            try
            {
                using (var reader = new StreamReader(filePath))
                {
                    strReadJson = reader.ReadToEnd();
                }
            }
            catch (Exception ex)
            {
                WriteLog("[ImportPowerNapSettings] exception, message: " + ex.Message);
            }


            if (strReadJson == string.Empty || strReadJson.Length == 0)
                return Task.FromResult(_powerNapSettings);// _present_powerNap_settings);
            try
            {
                //_present_powerNap_settings = RunPowerNapDeserializeObject(strReadJson);
                //_present_powerNap_settings.RemoveAll(x => x.SerialNumber == null);
                _powerNapSettings = RunPowerNapDeserializeObject(strReadJson);
                _powerNapSettings.RemoveAll(x => x.SerialNumber == null);
            }
            catch (Exception)// ex)
            {
            }
            return Task.FromResult(_powerNapSettings);// _present_powerNap_settings);
        }*/

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

        //#region EasyArrange Settings - 2024-8-26 unused, use DeviceManager instead

        //Robert_Lin, 2024-10-10, remove unused method
        //public Task<string> WriteEasyArrangeSettings(EAMonitorSettings eaMonitorSettings)
        //{
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

        //return Task.FromResult("OK");
        //}

        //Robert_Lin, 2024-10-10, remove unused method
        //public Task<EAMonitorSettings> ReadEasyArrangeSettings(string monitorModel, string serialNumber)
        //{
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
        //return Task.FromResult<EAMonitorSettings>(null);
        //}

        //#endregion EasyArrange Settings - 2024-8-26 unused, use DeviceManager instead

        #region DisplayImpExpSettings

        public Task<bool> DisplayExportSettings(string modelname, string seriveTag, List<DDPMMonitorSettings> monitorSettings, string path)
        {
            WriteLog("[ExportSettingsFile] modelname: " + modelname);
            WriteLog("[ExportSettingsFile] seriveTag: " + seriveTag);
            WriteLog("[ExportSettingsFile] FilePath: " + path);

            DDPMImpExpSettings impexpSettings = new DDPMImpExpSettings();

            DDPMSettings settings = ReloadAppConfigData().Result;
            if (settings != null)
            {
                //impexpSettings.AppSettings = settings.AppSettings;
                impexpSettings.UserSettings = settings.UserSettings;
                // PIMS-328022 to renew EzMemory
                impexpSettings.UserSettings.EAProfile = new List<EAProfileDDPM>();
                List<HotkeySettings> hotkeySettings = new List<HotkeySettings>();
                hotkeySettings = ReadHotkeySettings().Result;
                if (hotkeySettings != null)
                {
                    if (hotkeySettings.Count == 1)
                    {
                        impexpSettings.UserSettings.HotkeySettings = hotkeySettings[0];
                    }
                    else
                    {
                        WriteLog("[ExportSettingsFile] hotkeySettings count is " + hotkeySettings.Count.ToString());
                    }
                }
                else
                {
                    WriteLog("[ExportSettingsFile] hotkeySettings is null ");
                }

                //List<DDPMMonitorSettings> monitorSettings = ReloadMonitorSettings(modelname).Result;

                if (monitorSettings != null)
                {
                    if (path.Length > 6)
                    {
                        if ((path.Substring(path.Length - 5, 5).ToUpper() != ".json".ToUpper()))
                        {
                            path = path + ".json";
                        }
                    }
                    else
                    {
                        path = path + ".json";
                    }

                    //Dean 0912 file not ready at here, remove check
                    //Elsa Add Security
                    //string FileInfo;
                    //if (!DDPMFileSecurity.IsFilePathValid(path, out FileInfo))
                    //{
                    //    _log.Info($"{nameof(DisplayExportSettings)} {FileInfo}");
                    //    return Task.FromResult<bool>(false);
                    //}

                    foreach (DDPMMonitorSettings singleMonitorSetting in monitorSettings)
                    {
                        if (singleMonitorSetting.ServiceTag == seriveTag)
                        {
                            impexpSettings.MonitorSettings = singleMonitorSetting;
                            // PIMS-328022 to renew EzMemory
                            impexpSettings.MonitorSettings.easyArrangementDDPM.Desktops = new List<DesktopDDPM>();

                            //export file
                            FileInfo fileInfo = new FileInfo(path);
                            fileInfo.Create().Close();
                            //init data to file
                            if (WriteImpExpSettings(path, impexpSettings))
                            {
                                WriteLog("[ExportSettingsFile] Monitor settings file create and write success");
                                if (!string.IsNullOrEmpty(_export_path))
                                {
                                    string _exportpath = _export_path + "\\" + modelname + ".json";
                                    if (WriteImpExpSettings(_exportpath, impexpSettings))
                                    {
                                        WriteLog("[ExportSettingsFile] Monitor settings file create and write to export file success");
                                    }
                                    else
                                    {
                                        WriteLog("[ExportSettingsFile] Monitor settings file create and write to export file failed");
                                    }
                                }
                                return Task.FromResult<bool>(true);
                            }
                            else
                            {
                                WriteLog("[ExportSettingsFile] Monitor settings file create and write failed");
                                return Task.FromResult<bool>(false);
                            }
                        }
                    }
                }
                else
                {
                    WriteLog("[ExportSettingsFile] monitorSettings file is null");
                }
            }
            else
            {
                WriteLog("[ExportSettingsFile] Usersettings file is null");
            }

            return Task.FromResult<bool>(false);
        }

        public Task<DisplayImportResultCode> DisplayImportSettings(string path, bool isSameModel, string serviceTag, out DDPMImpExpSettings ImpExpSettings)
        {
            bool isEzMemoryOverride = false;
            WriteLog("[DisplayImportSettings] path :" + path);
            List<DDPMMonitorSettings> monitorSettingsList = new List<DDPMMonitorSettings>();
            ImpExpSettings = ReadImportSettingsFile(path).Result;
            //List<VCPCode> vcps = new List<VCPCode>();
            if (ImpExpSettings != null)
            {
                if (ImpExpSettings.UserSettings != null)
                {
                    DDPMUserSettings userSettings = new DDPMUserSettings();
                    userSettings = ImpExpSettings.UserSettings;
                    List<HotkeySettings> hotkeySettings = new List<HotkeySettings>();
                    hotkeySettings.Add(userSettings.HotkeySettings);
                    WriteHotkeySettings(hotkeySettings);
                    DDPMSettings settings = ReloadAppConfigData().Result;
                    if (settings != null)
                    {
                        if(settings.UserSettings.EAProfile != null
                            && settings.UserSettings.EAProfile.Count() > 0)
                            isEzMemoryOverride = true;
                        settings.UserSettings = userSettings;
                        if (SetAppConfigData(settings).Result)
                        {
                            WriteLog("[DisplayImportSettings] SetAppConfigData is success ");
                        }
                        else
                        {
                            WriteLog("[DisplayImportSettings] SetAppConfigData is fail ");
                        }
                    }
                    else
                    {
                        WriteLog("[DisplayImportSettings] DDPMSettings is null ");
                    }
                }
                else
                {
                    WriteLog("[DisplayImportSettings] UserSettings is null ");
                }
                if (ImpExpSettings.MonitorSettings != null)
                {
                    //if (modelName != "Skip")//Elsa add to fix PIMS-313843
                    //{
                    //    if (modelName != ImpExpSettings.MonitorSettings.Model)
                    //    { return Task.FromResult<bool>(false); }
                    //}
                    DDPMMonitorSettings monitorSettings = new DDPMMonitorSettings();
                    monitorSettings = ImpExpSettings.MonitorSettings;
                    WriteLog("[DisplayImportSettings] monitorSettings.Model :" + monitorSettings.Model);
                    string monitorSettings_path = _display_path + "\\" + monitorSettings.Model + ".json";
                    WriteLog("[DisplayImportSettings] monitorSettings_path :" + monitorSettings_path);
                    if (File.Exists(monitorSettings_path))
                    {
                        monitorSettingsList = ReloadMonitorSettings(monitorSettings.Model).Result;
                        if (monitorSettingsList != null)
                        {
                            if (monitorSettingsList.Count != 0)
                            {
                                foreach (DDPMMonitorSettings settings in monitorSettingsList)
                                {
                                    //if ((settings.ServiceTag == monitorSettings.ServiceTag && isSameModel == false) || 
                                    //(settings.ServiceTag == serviceTag && isSameModel == true))
                                    if ((isSameModel == false) || (isSameModel == true))//Elsa add to fix PIMS-313793
                                    {
                                        settings.Input = monitorSettings.Input;
                                        settings.KVM = monitorSettings.KVM;
                                        settings.VCPs = monitorSettings.VCPs;
                                        settings.EA = monitorSettings.EA;
                                        settings.DisplayPropertiesInfo = monitorSettings.DisplayPropertiesInfo;
                                        settings.scheduleInfo = monitorSettings.scheduleInfo;
                                        if (WriteMonitorSettings(settings.Model, monitorSettingsList).Result)
                                        {
                                            //vcps = monitorSettings.VCPs;
                                            if (!isSameModel)
                                            {
                                                if(isEzMemoryOverride) return Task.FromResult(DisplayImportResultCode.DoneWithEzMemoryCleared);
                                                return Task.FromResult(DisplayImportResultCode.Done);
                                            }
                                        }
                                        else
                                        {
                                            WriteLog("[DisplayImportSettings] ServiceTag : " + settings.ServiceTag);
                                            WriteLog("[DisplayImportSettings] Import settings Fail...");
                                            if (!isSameModel)
                                            {
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                WriteLog("[DisplayImportSettings] monitorSettingsList Count = 0...");
                            }
                        }
                        else
                        {
                            WriteLog("[DisplayImportSettings] monitorSettingsList is null...");
                        }
                    }
                    else
                    {
                        WriteLog("[DisplayImportSettings] Find monitor settings Fail...");
                    }
                }
            }
            else
            {
                WriteLog("[DisplayImportSettings] Settings is not DDPMSettings...");
            }
            
            return Task.FromResult(DisplayImportResultCode.Error);
        }

        public Task<bool> DisplayImpDDMSettings(string path, bool isSameModel, out DDMImpSettings impSettings)
        {
            impSettings = new DDMImpSettings();
            if (impSettings != null)
            {
                DDMMonitorSettings monitorSettings = new DDMMonitorSettings();
                monitorSettings = impSettings.MonitorSettings;
                WriteLog("[DisplayImportSettings] monitorSettings.Model :" + monitorSettings.Model);
                string monitorSettings_path = _display_path + "\\" + monitorSettings.Model + ".json";
                WriteLog("[DisplayImportSettings] monitorSettings_path :" + monitorSettings_path);
                if (File.Exists(monitorSettings_path))
                {
                    impSettings = ReadDDMImpSettingsFile(path).Result;
                    if (impSettings != null)
                    {
                        return Task.FromResult(true);
                    }
                    //if (impSettings != null)
                    //{
                    //    if (impSettings.Count != 0)
                    //    {
                    //        foreach (DDPMMonitorSettings settings in monitorSettingsList)
                    //        {
                    //            if (settings.ServiceTag == monitorSettings.ServiceTag || isSameModel)
                    //            {
                    //                settings.Input = monitorSettings.Input;
                    //                settings.KVM = monitorSettings.KVM;
                    //                settings.VCPs = monitorSettings.VCPs;
                    //                settings.EA = monitorSettings.EA;
                    //                if (WriteMonitorSettings(settings.Model, monitorSettingsList).Result)
                    //                {
                    //                    vcps = monitorSettings.VCPs;
                    //                    if (!isSameModel)
                    //                    {
                    //                        return Task.FromResult<bool>(true);
                    //                    }
                    //                }
                    //                else
                    //                {
                    //                    WriteLog("[DisplayImportSettings] ServiceTag : " + settings.ServiceTag);
                    //                    WriteLog("[DisplayImportSettings] Import settings Fail...");
                    //                    if (!isSameModel)
                    //                    {
                    //                        break;
                    //                    }
                    //                }
                    //            }
                    //        }
                    //        if (isSameModel)
                    //        {
                    //            return Task.FromResult(true);
                    //        }
                    //    }
                    //    else
                    //    {
                    //        WriteLog("[DisplayImportSettings] monitorSettingsList Count = 0...");
                    //    }
                    //}
                    //else
                    //{
                    //    WriteLog("[DisplayImportSettings] monitorSettingsList is null...");
                    //}
                }
                else
                {
                    WriteLog("[DisplayImportSettings] Find monitor settings Fail...");
                }
            }
            else
            {
                WriteLog("[DisplayImportSettings] Settings is not DDMSettings...");
            }
            return Task<bool>.FromResult<bool>(false);
        }

        #endregion DisplayImpExpSettings

        #region Migration
        public Task<bool> isDDMMigration(out string folder_appdatapath_migration)
        {
            string folder = GetActiveUserLocalAppDataPath();
            WriteLog($"GetActiveUserLocalAppDataPath: {folder}");
            folder_appdatapath_migration = folder + "\\" + folder_product + "\\" + folder_localappdata_Migration;
            string folder_path = folder_appdatapath_migration + "\\UserFoler";
            if (Directory.Exists(folder_path))
            {
                return Task<bool>.FromResult(true);
            }
            return Task<bool>.FromResult(false);
        }

        public Task<bool> ReadDDMMonitorSettings(string path, ref DDMMonitorSettings DDMmonitorsettings)
        {
            try
            {
                if (File.Exists(path))
                {
                    if (DDMMonitorSettings.restoreDDMMonitorSettings(ref DDMmonitorsettings, path))
                    {
                        return Task<bool>.FromResult(true);
                    }
                }
            }
            catch
            {
                ;
            }
            return Task<bool>.FromResult(false);
        }

        public Task<bool> ReadDDMUserSettings(string path, ref DDMUserSettings DDMusersettings)
        {
            try
            {
                if (File.Exists(path))
                {
                    if (DDMUserSettings.restoreDDMUserSettings(ref DDMusersettings, path))
                    {
                        return Task<bool>.FromResult(true);
                    }
                }
            }
            catch
            {
                ;
            }
            return Task<bool>.FromResult(false);
        }
        #endregion Migration

        #endregion ISettingManagerDev implementation

        private string GetActiveUserLocalAppDataPath()
        {
            string localAppDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Dell");
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
            //[Dean 0912] file could be not exist at here, avoid settings fail
            //Elsa Add Security
            //string FileInfo;
            //if (!DDPMFileSecurity.IsFilePathValid(filePath, out FileInfo))
            //{
            //    _log.Info($"{nameof(RunSerializeObject)} {FileInfo}");
            //    return string.Empty;
            //}

            string jsonString = string.Empty;
            jsonString = JsonConvert.SerializeObject(powerNapSettings);
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                writer.Write(jsonString);
            }

            return jsonString;
        }

        /*private string RunSerializeObject(List<PowerNapSetting> powerNapSettings)
        {
            string jsonString = string.Empty;
            jsonString = JsonConvert.SerializeObject(powerNapSettings);
            string jsonpath = _powerNapsettings_path;
            //[Dean 0912] file could be not exist at here, avoid settings fail
            //Elsa Add Security
            //string FileInfo;
            //if (!DDPMFileSecurity.IsFilePathValid(jsonpath, out FileInfo))
            //{
            //    _log.Info($"{nameof(RunSerializeObject)} {FileInfo}");
            //    return string.Empty;
            //}
            using (StreamWriter writer = new StreamWriter(jsonpath))
            {
                writer.Write(jsonString);
            }

            return jsonString; ;
        }*/

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

        /*private string RunSerializeObject(List<HotkeySettings> hotkeySettings)
        {
            string jsonString = string.Empty;
            jsonString = JsonConvert.SerializeObject(hotkeySettings);
            string jsonpath = _hotkeysettings_path;
            //[Dean 0912] file could be not exist at here, avoid settings fail
            //Elsa Add Security
            //string FileInfo;
            //if (!DDPMFileSecurity.IsFilePathValid(jsonpath, out FileInfo))
            //{
            //    _log.Info($"{nameof(RunSerializeObject)} {FileInfo}");
            //    return string.Empty;
            //}
            using (StreamWriter writer = new StreamWriter(jsonpath))
            {
                writer.Write(jsonString);
            }

            return jsonString; ;
        }*/

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
        /*
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
            //[Dean 0912] file could be not exist at here, avoid settings fail
            //Elsa Add Security
            //string FileInfo;
            //if (!DDPMFileSecurity.IsFilePathValid(jsonpath, out FileInfo))
            //{
            //    _log.Info($"{nameof(RunSerializeObject)} {FileInfo}");
            //    return string.Empty;
            //}
            using (StreamWriter writer = new StreamWriter(jsonpath))
            {
                writer.Write(jsonString);
            }

            return jsonString; ;
        }*/

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

            string[] files = default;

            try
            {
                files = Directory.GetFiles(_display_path, "*.json");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Dictionary] Get files in folder failed, message: {ex.Message}");
            }

            return _AllMonitorSettings;
        }

        private string RunSerializeObject_MonitorSettings(string modelname, List<DDPMMonitorSettings> monitorSettings)
        {
            string jsonString = string.Empty;
            string monitorSettings_path = _display_path + "\\" + modelname + ".json";

            try
            {
                //jsonString = JsonConvert.SerializeObject(monitorSettings);
                /*JToken token = JToken.FromObject(monitorSettings);
                if (token.Type == JTokenType.Object)
                {
                    JObject obj = (JObject)token;
                    // Handle object
                    jsonString = obj.ToString();
                }
                else if (token.Type == JTokenType.Array)
                {
                    JArray array = (JArray)token;
                    // Handle array
                    jsonString = array.ToString();
                }*/
                jsonString = DDPMFileSecurity.ConvertObjectToSerializedString(monitorSettings, out string info);
                if(string.IsNullOrEmpty(jsonString))
                {
                    WriteLog("[RunSerializeObject_MonitorSettings] got empty output");
                }
            }
            catch (Exception e)
            {
                jsonString = string.Empty;
                WriteLog($"[RunSerializeObject_MonitorSettings] exception: {e.Message}");
            }
            //using (StreamWriter writer = new StreamWriter(monitorSettings_path))
            //{
            //    writer.Write(jsonString);
            //}

            return jsonString;
        }

        private List<DDPMMonitorSettings> RunMonitorListDeserializeObject(string value)
        {
            List<DDPMMonitorSettings> monitorSettingsList = new List<DDPMMonitorSettings>();

            if(string.IsNullOrEmpty(value))
            {
                WriteLog($"[RunMonitorListDeserializeObject] empty value, return default data");
                return monitorSettingsList;
            }
            try
            {
                monitorSettingsList = JsonConvert.DeserializeObject<List<DDPMMonitorSettings>>(value);
            }
            catch (Exception e)
            {
                WriteLog($"[RunMonitorListDeserializeObject] for monitor settings with exception: {e.Message}");
            }
            if (monitorSettingsList == null)
            {
                monitorSettingsList = new List<DDPMMonitorSettings>();
                WriteLog($"[RunMonitorListDeserializeObject] empty value, return default data 2");
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
            //[Dean 0912] file could be not exist at here, avoid settings fail
            //Elsa Add Security
            //string FileInfo;
            //if (!DDPMFileSecurity.IsFilePathValid(path, out FileInfo))
            //{
            //    _log.Info($"{nameof(RunSerializeObject)} {FileInfo}");
            //    return string.Empty;
            //}
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
            catch (Exception ex)
            {
                WriteLog("[RunImpExpDeserializeObject] ex : " + ex.Message.ToString());
                WriteLog("[RunImpExpDeserializeObject] value is " + value);
            }

            return impexpSettings;
        }

        private DDMImpSettings RunDDMImpDeserializeObject(string value)
        {
            DDMImpSettings impSettings = new DDMImpSettings();

            try
            {
                impSettings = JsonConvert.DeserializeObject<DDMImpSettings>(value);
            }
            catch (Exception)
            {
                ;
            }

            return impSettings;
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
            string info;
            if (!DDPMFileSecurity.SetJsonContentFromSerializedString(JObject.FromObject(impexpSettings).ToString(), path, out info))
            {
                WriteLog($"[WriteImpExpSettings][SetJsonContentFromSerializedString] failed with {info}");
                return false;
            }

            return true;
        }

        public Task<DDPMImpExpSettings> ReadImportSettingsFile(string path)
        {
            DDPMImpExpSettings ImpSettings = new DDPMImpExpSettings();

            if (!string.IsNullOrEmpty(path))
            {
                if (File.Exists(path))
                {
                    //Elsa Add Security
                    string FileInfo;
                    if (!DDPMFileSecurity.IsFilePathValid(path, out FileInfo))
                    {
                        WriteLog($"{nameof(ReadImportSettingsFile)} {FileInfo}");
                        return Task.FromResult(ImpSettings);
                    }
                    string strReadJson = string.Empty;
                    //using (var reader = new StreamReader(path))
                    //{
                    //    strReadJson = reader.ReadToEnd();
                    //}
                    //security SA
                    string info;
                    strReadJson = DDPMFileSecurity.GetSerializedJsonString(path, out info);//, false);

                    if (strReadJson == string.Empty || strReadJson.Length == 0)
                    {
                        WriteLog("[ReadImportSettingsFile] strReadJson is empty or length is 0.");
                        return Task.FromResult(ImpSettings);
                    }
                    try
                    {
                        //WriteLog($"[ReadImportSettingsFile]strReadJson: " + strReadJson);
                        ImpSettings = RunImpExpDeserializeObject(strReadJson);
                    }
                    catch (Exception)
                    {
                        ;
                    }
                }
                else
                {
                    WriteLog("[ReadImportSettingsFile] file isn't exist : " + path);
                }
            }
            return Task.FromResult(ImpSettings);
        }
        public Task<DDMImpSettings> ReadDDMImpSettingsFile(string path)
        {
            DDMImpSettings ImpSettings = new DDMImpSettings();

            if (!string.IsNullOrEmpty(path))
            {
                if (File.Exists(path))
                {
                    //Elsa Add Security
                    string FileInfo;
                    if (!DDPMFileSecurity.IsFilePathValid(path, out FileInfo))
                    {
                        WriteLog($"{nameof(ReadDDMImpSettingsFile)} {FileInfo}");
                        return Task.FromResult(ImpSettings);
                    }
                    string strReadJson = string.Empty;
                    //using (var reader = new StreamReader(path))
                    //{
                    //    strReadJson = reader.ReadToEnd();
                    //}
                    //security SA
                    strReadJson = DDPMFileSecurity.GetSerializedJsonString(path, out string info);

                    //if (strReadJson == string.Empty || strReadJson.Length == 0)
                    if (string.IsNullOrEmpty(strReadJson))
                    {
                        WriteLog($"[ReadDDMImpSettingsFile] empty output: {info}");
                        return Task.FromResult(ImpSettings);
                    }
                    try
                    {
                        WriteLog($"[ReadImportSettingsFile]strReadJson: " + strReadJson);
                        ImpSettings = RunDDMImpDeserializeObject(strReadJson);
                    }
                    catch (Exception e)
                    {
                        WriteLog($"[ReadImportSettingsFile] exception: {e.Message}");
                    }
                }
                else
                    WriteLog($"[ReadImportSettingsFile]strReadJson: selected file isn't exist" + path);
            }

            return Task.FromResult(ImpSettings);
        }

        #endregion ImpExpSettings

        #region Global settings
        public Task<GlobalSettingParam> ReadGlobalSettings()
        {
            if (string.IsNullOrEmpty(_GlobalSetting_path))
            {
                _GlobalSettingParam = null;
            }
            else
            {
                string strFilePath = _GlobalSetting_path;

                if (File.Exists(strFilePath))
                {
                    string strReadJson = string.Empty;
                    string info;
                    strReadJson = DDPMFileSecurity.GetSerializedJsonString(strFilePath, out info);

                    if (strReadJson == string.Empty || strReadJson.Length == 0)
                    {
                        _GlobalSettingParam = null;
                        return Task.FromResult(_GlobalSettingParam);
                    }
                    try
                    {
                        _GlobalSettingParam = RunGlobalSettinDeserializeObject(strReadJson);
                        _GlobalSettingParam.GlobalSetting_About.SWVersion = _settingsAccessInfoVer;
                    }
                    catch (Exception)// ex)
                    {
                        _GlobalSettingParam = null;
                        //return Task.FromResult(_GlobalSettingParam);
                    }
                }
                else
                {
                    _GlobalSettingParam = null;// File.Create(strFilePath).Close();
                }
            }
            return Task.FromResult(_GlobalSettingParam);
        }

        public Task<bool> WriteGlobalSettings(GlobalSettingParam globalSettingParam, bool writeToSys = true)
        {
            bool result = WriteSettings_Common(globalSettingParam, "global");
            if(!result)
            {
                WriteLog(("Call [WriteSettings_Common] to write global setting failed"));
            }

            //apply setting to system IT config
            if (_GlobalSettingParam != null && result)
            {
                if (_SysSettingsPlugin != null)
                {
                    if (writeToSys)
                    {
                        try
                        {
                            result = _SysSettingsPlugin.WriteGlobalSettingsToITConfig(_GlobalSettingParam, true).Result;
                            WriteLog(($"Call sys plugin to write global setting ") + (result ? "PASS" : "FAIL"));
                        }
                        catch(Exception e)
                        {
                            WriteLog(($"Call sys plugin to write global setting [exception]: {e.Message}"));
                        }
                    }
                }
            }

            return Task.FromResult(result);
        }

        private GlobalSettingParam RunGlobalSettinDeserializeObject(string value)
        {
            GlobalSettingParam retList = new GlobalSettingParam();
            try
            {
                retList = JsonConvert.DeserializeObject<GlobalSettingParam>(value);
            }
            catch (Exception e)
            {
                WriteLog($"[RunGlobalSettinDeserializeObject] exception: {e.Message}");
            }
            return retList;
        }
        #endregion Global settings

        #region InterruptScreen
        public Task<InterruptScreenRoot> ReadInterruptScreen()
        {
            if (string.IsNullOrEmpty(_InterruptScreen_path))
            {
                _InterruptScreenParam = null;
            }
            else
            {
                string strFilePath = _InterruptScreen_path;

                if (File.Exists(strFilePath))
                {
                    string strReadJson = string.Empty;
                    string info;
                    strReadJson = DDPMFileSecurity.GetSerializedJsonString(strFilePath, out info);

                    if (strReadJson == string.Empty || strReadJson.Length == 0)
                    {
                        _InterruptScreenParam = null;
                        return Task.FromResult(_InterruptScreenParam);
                    }
                    try
                    {
                        _InterruptScreenParam = RunInterruptScreenDeserializeObject(strReadJson);
                    }
                    catch (Exception)// ex)
                    {
                        _InterruptScreenParam = null;
                        //return Task.FromResult(_GlobalSettingParam);
                    }
                }
                else
                {
                    _InterruptScreenParam = null;// File.Create(strFilePath).Close();
                }
            }
            return Task.FromResult(_InterruptScreenParam);
        }

        public Task<bool> WriteInterruptScreen(InterruptScreenRoot interruptScreenParam)
        {
            bool result = WriteSettings_Common(interruptScreenParam, "interrupt");

            //apply setting to system IT config
            if (interruptScreenParam != null && result)
            {
                string info;
                if (!DDPMFileSecurity.SetJsonContentFromSerializedString(JObject.FromObject(interruptScreenParam).ToString(), _InterruptScreen_path, out info))
                {
                    WriteLog(info);
                    result = false;
                }
            }

            return Task.FromResult(result);
        }
        /*private string RunSerializeObject(GlobalSettingParam globalSettingParam, string filePath)
        {
            string jsonString = string.Empty;
            jsonString = JsonConvert.SerializeObject(globalSettingParam);
            //[Dean 0912] file could be not exist at here, avoid settings fail
            //Elsa Add Security
            //string FileInfo;
            //if (!DDPMFileSecurity.IsFilePathValid(filePath, out FileInfo))
            //{
            //    _log.Info($"{nameof(RunSerializeObject)} {FileInfo}");
            //    return string.Empty;
            //}
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
            //[Dean 0912] file could be not exist at here, avoid settings fail
            //Elsa Add Security
            //string FileInfo;
            //if (!DDPMFileSecurity.IsFilePathValid(jsonpath, out FileInfo))
            //{
            //    _log.Info($"{nameof(RunSerializeObject)} {FileInfo}");
            //    return string.Empty;
            //}
            using (StreamWriter writer = new StreamWriter(jsonpath))
            {
                writer.Write(jsonString);
            }
            return jsonString;
        }*/
        private InterruptScreenRoot RunInterruptScreenDeserializeObject(string value)
        {
            InterruptScreenRoot retList = new InterruptScreenRoot();
            try
            {
                retList = JsonConvert.DeserializeObject<InterruptScreenRoot>(value);
            }
            catch (Exception)
            {

            }
            return retList;
        }
        #endregion InterruptScreen

        private DDPMSettings InitDDPMUserConfigFile()
        {
            string folder = GetActiveUserLocalAppDataPath();
            WriteLog($"GetActiveUserLocalAppDataPath: {folder}");
            string folder_appdatapath_ddpm = folder + "\\" + folder_product;
            WriteLog($"folder_appdatapath_ddpm: {folder_appdatapath_ddpm}");
            string folderInfo = string.Empty, info = string.Empty;
            try
            {
                DirectoryInfo di = System.IO.Directory.CreateDirectory(folder_appdatapath_ddpm);
                WriteLog($"create folder {folder_appdatapath_ddpm} success");
                if (!DDPMFileSecurity.CheckFold(folder_appdatapath_ddpm, out folderInfo, out info))
                {
                    WriteLog($"CreateDirectory path check result is invalid: {folder_appdatapath_ddpm}, {info}.");
                    _settings = null;
                    return null;
                }
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

            if (File.Exists(file_appdatapath_userconfig))
            {
                // DDPMSettings.getSettingsforImport(file_appdatapath_userconfig, ref ddpm_app, ref ddpm_user);
                string serialized_string = DDPMFileSecurity.GetSerializedJsonString(file_appdatapath_userconfig, out info);//, false);
                if (!string.IsNullOrEmpty(serialized_string))
                    _settings = JsonConvert.DeserializeObject<DDPMSettings>(serialized_string);
                else
                {
                    WriteLog("[InitDDPMUserConfigFile] GetSerializedJsonString: " + info);
                    _settings = new DDPMSettings(new DDPMAppSettings(), new DDPMUserSettings(), new DDPMITConfig());
                    if (_settings != null)
                    {
                        WriteLog("[InitDDPMUserConfigFile] *** Init cache from file fail, re-create default settings to file");
                        if (DDPMFileSecurity.SetJsonContentFromSerializedString(JObject.FromObject(_settings).ToString(), file_appdatapath_userconfig, out info))
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

        private bool WriteSettings_Common(object dataObj, string type)
        {
            if (dataObj == null)
            {
                return false;
            }
            string settings_path = string.Empty;
            switch (type)
            {
                case "global":
                    settings_path = _GlobalSetting_path;
                    break;
                case "hotkey":
                    settings_path = _hotkeysettings_path;
                    break;
                //case "powernap":
                //    settings_path = _powerNapsettings_path;
                //    break;
                case "interrupt":
                    settings_path = _InterruptScreen_path;
                    break;
                default:
                    WriteLog($"[WriteSettings_Common] type:{type} is not defined in common code!");
                    return false;
            }

            string info = string.Empty;
            bool result = false;
            JToken token = JToken.FromObject(dataObj);
            if (token.Type == JTokenType.Object)
            {
                JObject obj = (JObject)token;
                // Handle object
                result = DDPMFileSecurity.SetJsonContentFromSerializedString(obj.ToString(), settings_path, out info);
            }
            else if (token.Type == JTokenType.Array)
            {
                JArray array = (JArray)token;
                // Handle array
                result = DDPMFileSecurity.SetJsonContentFromSerializedString(array.ToString(), settings_path, out info);
            }
            else
            {
                WriteLog($"[WriteSettings_Common] unknow json type in program: {token.Type}");
                return false;
            }
            if (!result)
                WriteLog($"[SetJsonContentFromSerializedString] failed with info: {info}");

            switch (type)
            {
                case "global":
                    _GlobalSettingParam = (GlobalSettingParam)dataObj;
                    break;
                case "hotkey":
                    _hotkeySettings = (List<HotkeySettings>)dataObj;
                    break;
                //case "powernap":
                //    _powerNapSettings = (List<PowerNapSetting>)dataObj;
                //    break;
                case "interrupt":
                    _InterruptScreenParam = (InterruptScreenRoot)dataObj;
                    break;
                default:
                    WriteLog($"[WriteSettings_Common] type:{type} is not defined in common code!");
                    return false;
            }

            return result;
        }

        private object InitDDPMUserSettings_Common(string ConfigPath, string config_type)
        {
            string info;

            //[Dean 1206] Here return null would cause the no data init, remove it
            //if(!File.Exists(ConfigPath))
            //{
            //    WriteLog($"[InitDDPMUserSettings_Common]: File:{ConfigPath}, empty file info");
            //    return null;
            //}

            FileInfo fileInfo = default;
            try
            {
                fileInfo = new FileInfo(ConfigPath);
            }
            catch (Exception ex)
            {
                WriteLog($"[InitDDPMUserSettings_Common]: File:{ConfigPath}, get file info failed, message: {ex.Message}");
                return null;
            }

            //if (fileInfo == null)
            //{
            //    WriteLog($"[InitDDPMUserSettings_Common]: File:{ConfigPath}, empty file info");
            //    return null;
            //}
            if (string.IsNullOrEmpty(fileInfo.DirectoryName))
            {
                WriteLog($"[InitDDPMUserSettings_Common]: empty DirectoryName of fileInfo");
                return null;
            }
            string fileFolder = fileInfo.DirectoryName;
            try
            {
                DirectoryInfo di = System.IO.Directory.CreateDirectory(fileFolder);
                WriteLog($"create folder {fileFolder} success");
            }
            catch
            {
                WriteLog($"CreateDirectory with {fileFolder} failed.");
                return null;
            }
            if (!DDPMFileSecurity.IsFolderPathValid(fileFolder, out info))
            {
                WriteLog($"{nameof(InitDDPMUserSettings_Common)} {info}");
                return null;
            }

            //[Dean 1123] merge the check into DDPMFileSecurity.IsFolderPathValid and don't remove symlink to avoid attack
            //if (!DDPMFileSecurity.SRemoveSymbolicFolder(fileFolder, out info))
            //{
            //    WriteLog($"{nameof(InitDDPMUserSettings_Common)} {info}");
            //    return null;
            //}

            //WriteLog($"config path is {ConfigPath}.");
            object new_obj = null;
            bool need_new = false;
            if (File.Exists(ConfigPath))
            {
                if (!DDPMFileSecurity.IsFilePathValid(ConfigPath, out info))
                {
                    WriteLog($"{nameof(InitDDPMUserSettings_Common)}[IsFilePathValid] {info}");
                    //File.Delete(ConfigPath);
                    return null;
                }
                else
                {
                    //Read from settings
                    switch (config_type)
                    {
                        case "color":
                            List<ColorPresetSettings> color = ReadColorPresetSettings().Result;
                            new_obj = color;
                            if (color != null && color.Count == 0)
                                need_new = true;
                            break;
                        case "global":
                            GlobalSettingParam global = ReadGlobalSettings().Result;
                            new_obj = global;
                            break;
                        case "hotkey":
                            List<HotkeySettings> hotkey = ReadHotkeySettings().Result;
                            new_obj = hotkey;
                        //    if (hotkey != null && hotkey.Count == 0)
                        //        need_new = true;
                            break;
                        //case "powernap":
                        //    List<PowerNapSetting> pnap = ReadPowerNapSettings().Result;
                        //    new_obj = pnap;
                        //    if (pnap != null && pnap.Count == 0)
                        //        need_new = true;
                        //    break;
                        case "interrupt":
                            InterruptScreenRoot interrupt = ReadInterruptScreen().Result;
                            new_obj = interrupt;
                            //if (interrupt != null)
                            //    need_new = true;
                            break;
                        default:
                            WriteLog($"[InitDDPMUserSettings_Common] config type {config_type} not support!!");
                            return null;
                    }
                }
            }
            if (new_obj == null || need_new == true)
            {
                //create new setting file then save it
                WriteLog($"[InitDDPMUserSettings_Common] {config_type} settings object is null, new an object");
                //init data to file
                switch (config_type)
                {
                    case "color":
                        List<ColorPresetSettings> color = new List<ColorPresetSettings>();
                        if (!WriteColorPresetSettings(color).Result)
                        {
                            WriteLog("[WriteColorPresetSettings] write new color preset setting failed");
                        }
                        new_obj = color;//keep memory data to allow program work properly
                        break;
                    case "global":
                        GlobalSettingParam global = new GlobalSettingParam();
                        if (!WriteGlobalSettings(global).Result)
                        {
                            WriteLog("[WriteGlobalSettings] write new global setting failed");
                        }
                        new_obj = global;//keep memory data to allow program work properly
                        break;
                    case "hotkey":
                        List<HotkeySettings> hotkey = new List<HotkeySettings>();
                        if (!WriteHotkeySettings(hotkey).Result)
                        {
                            WriteLog("[WriteHotkeySettings] write new hotkey setting failed");
                        }
                        new_obj = hotkey;//keep memory data to allow program work properly
                        break;
                    //case "powernap":
                    //    List<PowerNapSetting> pnap = new List<PowerNapSetting>();
                    //    if (!WritePowerNapSettings(pnap).Result)
                    //    {
                    //        WriteLog("[WriteHotkeySettings] write new power nap setting failed");
                    //    }
                    //    new_obj = pnap;//keep memory data to allow program work properly
                    //    break;
                    case "interrupt":
                        InterruptScreenRoot interrupt = new InterruptScreenRoot();
                        if (!WriteInterruptScreen(interrupt).Result)
                        {
                            WriteLog("[WriteInterruptScreen] write new Interrupt ScreenRoot setting failed");
                        }
                        new_obj = interrupt;
                        break;
                    default:
                        new_obj = null;
                        break;
                }
            }

            if (File.Exists(ConfigPath))
            {
                //ACL function to check exist rule and apply rule if not exist
                if (!DDPMFileSecurity.ApplyFileACLNormalUser(ConfigPath, out info))
                    WriteLog($"[InitDDPMUserConfigFile] {config_type}: {info}");
                else
                    WriteLog($"[InitDDPMUserConfigFile] {config_type} setting apply ACL success");
            }
            return new_obj;
        }

        private List<ColorPresetSettings> InitColorPresetConfigFile()
        {
            string info = string.Empty;
            string folder = GetActiveUserLocalAppDataPath();
            WriteLog($"InitColorPresetConfigFile: appdata path: {folder}");
            string file_path = Path.Combine(folder, folder_product, folder_localappdata_Applist, filename_colorpreset_peruser);
            _colorsettings_path = file_path;
            //
            // Security check (folder)
            string tmp_path = Path.Combine(folder, folder_product);
            if (!DDPMFileSecurity.ValidateFilePath(tmp_path, out info))
            {
                WriteLog($"[InitColorPresetConfigFile]ValidateFilePath root folder: {info}");
                if (_colorPresetSettings == null)
                    _colorPresetSettings = new List<ColorPresetSettings>();
                return _colorPresetSettings;
            }
            tmp_path = Path.Combine(folder, folder_product, folder_localappdata_Applist);
            if (!DDPMFileSecurity.ValidateFilePath(tmp_path, out info))
            {
                WriteLog($"[InitColorPresetConfigFile]ValidateFilePath AppLibrary folder: {info}");
                if (_colorPresetSettings == null)
                    _colorPresetSettings = new List<ColorPresetSettings>();
                return _colorPresetSettings;
            }
            //End check
            //
            _colorPresetSettings = (List<ColorPresetSettings>)InitDDPMUserSettings_Common(_colorsettings_path, "color");
            if (_colorPresetSettings == null)
            {
                WriteLog($"InitColorPresetConfigFile: *** no object created, null return ***");
            }

            //create app icon folder if not exist
            string folder_appicon_path = Path.Combine(folder, folder_product, folder_localappdata_Applist, folder_localappdata_Appicon);
            try
            {
                DirectoryInfo di = System.IO.Directory.CreateDirectory(folder_appicon_path);
                WriteLog($"create folder {folder_appicon_path} success");
                _appiconfolder_path = folder_appicon_path;
                                
                if (!DDPMFileSecurity.IsFolderPathValid(folder_appicon_path, out info))
                {
                    WriteLog($"{nameof(InitColorPresetConfigFile)} {info}");
                }
            }
            catch
            {
                WriteLog($"CreateDirectory with {folder_appicon_path} failed.");
            }
            if (_colorPresetSettings == null)
                _colorPresetSettings = new List<ColorPresetSettings>();
            return _colorPresetSettings;
        }

        private List<HotkeySettings> InitHotkeyConfigFile()
        {
            string folder = GetActiveUserLocalAppDataPath();
            WriteLog($"InitHotkeyConfigFile: appdata path: {folder}");
            string file_path = Path.Combine(folder, folder_product, filename_hotkey_peruser);
            _hotkeysettings_path = file_path;
            //
            // Security check (folder)
            string tmp_path = Path.Combine(folder, folder_product);
            if (!DDPMFileSecurity.ValidateFilePath(tmp_path, out string info))
            {
                WriteLog($"[InitHotkeyConfigFile]ValidateFilePath root folder: {info}");
                if (_hotkeySettings == null)
                    _hotkeySettings = new List<HotkeySettings>();
                return _hotkeySettings;
            }
            //End check
            //
            _hotkeySettings = (List<HotkeySettings>)InitDDPMUserSettings_Common(_hotkeysettings_path, "hotkey");
            if (_hotkeySettings == null)
            {
                WriteLog($"InitHotkeyConfigFile: *** no object created, null return ***");
            }
            if (_hotkeySettings == null)
                _hotkeySettings = new List<HotkeySettings>();
            return _hotkeySettings;
        }

        /*private List<PowerNapSetting> InitPowerNapConfigFile()
        {
            string folder = GetActiveUserLocalAppDataPath();
            WriteLog($"InitPowerNapConfigFile: appdata path: {folder}");
            string file_path = Path.Combine(folder, folder_product, filename_powernap_peruser);
            _powerNapsettings_path = file_path;
            //
            // Security check (folder)
            string tmp_path = Path.Combine(folder, folder_product);
            if (!DDPMFileSecurity.ValidateFilePath(tmp_path, out string info))
            {
                WriteLog($"[InitPowerNapConfigFile]ValidateFilePath root folder: {info}");
                if (_powerNapSettings == null)
                    _powerNapSettings = new List<PowerNapSetting>();
                return _powerNapSettings;
            }
            //End check
            //
            _powerNapSettings = (List<PowerNapSetting>)InitDDPMUserSettings_Common(_powerNapsettings_path, "powernap");
            if (_powerNapSettings == null)
            {
                WriteLog($"InitPowerNapConfigFile: *** no object created, null return ***");
            }
            if (_powerNapSettings == null)
                _powerNapSettings = new List<PowerNapSetting>();
            return _powerNapSettings;
        }*/

        private GlobalSettingParam InitGlobalSettingConfigFile()
        {
            string folder = GetActiveUserLocalAppDataPath();
            WriteLog($"InitGlobalSettingConfigFile: appdata path: {folder}");
            string file_path = Path.Combine(folder, folder_product, filename_GlobalSetting_peruser);
            _GlobalSetting_path = file_path;
            //
            // Security check (folder)
            string tmp_path = Path.Combine(folder, folder_product);
            if (!DDPMFileSecurity.ValidateFilePath(tmp_path, out string info))
            {
                WriteLog($"[InitGlobalSettingConfigFile]ValidateFilePath root folder: {info}");
                if (_GlobalSettingParam == null)
                    _GlobalSettingParam = new GlobalSettingParam();
                return _GlobalSettingParam;
            }
            //End check
            //
            _GlobalSettingParam = (GlobalSettingParam)InitDDPMUserSettings_Common(_GlobalSetting_path, "global");
            if (_GlobalSettingParam == null)
            {
                WriteLog($"InitGlobalSettingConfigFile: *** no object created, null return ***");
            }
            if (_GlobalSettingParam == null)
                _GlobalSettingParam = new GlobalSettingParam();
            return _GlobalSettingParam;
        }
        private InterruptScreenRoot InitInterruptScreenFile()
        {
            string folder = GetActiveUserLocalAppDataPath();
            WriteLog($"InitInterruptScreenFile: appdata path: {folder}");
            string file_path = Path.Combine(folder, folder_product, filename_InterruptScreen_peruser);
            _InterruptScreen_path = file_path;
            //
            // Security check (folder)
            string tmp_path = Path.Combine(folder, folder_product);
            if (!DDPMFileSecurity.ValidateFilePath(tmp_path, out string info))
            {
                WriteLog($"[InitInterruptScreenFile]ValidateFilePath root folder: {info}");
                if (_InterruptScreenParam == null)
                    _InterruptScreenParam = new InterruptScreenRoot();
                return _InterruptScreenParam;
            }
            //End check
            //
            _InterruptScreenParam = (InterruptScreenRoot)InitDDPMUserSettings_Common(_InterruptScreen_path, "interrupt");
            if (_InterruptScreenParam == null)
            {
                WriteLog($"InitInterruptScreenFile: *** no object created, null return ***");
            }
            if (_InterruptScreenParam == null)
                _InterruptScreenParam = new InterruptScreenRoot();
            return _InterruptScreenParam;
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
            object resvalue = null;
            try
            {
                if (hive == RegistryHive.LocalMachine || hive == RegistryHive.CurrentUser)
                {
                    if (_SysSettingsPlugin == null)
                    {
                        WriteLog($"[User setting plugin] null system settings plugin, can't access local machine registry");
                        return Task.FromResult(resvalue);
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
                    return Task.FromResult(resvalue); ;
                }
            }
            catch (Exception e)
            {
                WriteLog($"[User setting plugin] WARNING: read registry cause exception ({e.Message})");
                return Task.FromResult(resvalue); ;
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
                    if (result)
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
        public Task<bool> SaveLog(string saveFolderPath)
        {
            WriteLog($"[User setting plugin] SaveLog start");
            bool result = false;
            try
            {
                if (_SysSettingsPlugin != null)
                {
                    result = _SysSettingsPlugin.SaveLog(saveFolderPath).Result;
                }
                else
                {
                    WriteLog($"[User setting plugin] null system settings plugin, can't access local machine registry");
                }
            }
            catch (Exception e)
            {
                WriteLog($"[User setting plugin] WARNING: write registry cause exception ({e.Message})");
            }
            WriteLog($"[User setting plugin] SaveLog {(result ? "success" : "failed")}");
            WriteLog($"[User setting plugin] SaveLog end");
            return Task.FromResult(result);
        }
        #endregion

        #region common read/write json file interface
        public Task<string> ReadSerializedContentFromFile(string filePath)
        {
            FileInfo fileInfo = new FileInfo(filePath);
            string result = null;
            string info = string.Empty;
            if (!File.Exists(filePath))
            {
                WriteLog($"[ReadSerializedContentFromFile][File.Exists] File:{fileInfo.Name}, failed with(file is not exist)");
                return Task.FromResult(result);
            }
            //if (DDPMFileSecurity.IsPathSymbolicLinked(filePath, out info))
            if (!DDPMFileSecurity.IsFilePathValid(filePath, out info))
            {
                WriteLog($"[ReadSerializedContentFromFile][IsFilePathValid] File:{fileInfo.Name}, failed with({info})");
                return Task.FromResult(result);
            }
            result = DDPMFileSecurity.GetSerializedJsonString(filePath, out info);
            if (string.IsNullOrEmpty(result))
            {
                WriteLog($"[ReadSerializedContentFromFile] Result is empty, failed with ({info})");
            }
            return Task.FromResult(result);
        }

        public Task<bool> WriteSerializedContentToFile(string filePath, string content)
        {
            bool result = false;
            string info = string.Empty;
            FileInfo fileInfo = new FileInfo(filePath);
            //if (DDPMFileSecurity.IsPathSymbolicLinked(filePath, out info))
            if (!DDPMFileSecurity.IsFilePathValid(filePath, out info, Dell.Client.Framework.Security.Interfaces.PathCheckOption.IgnoreFileExists))
            {
                WriteLog($"[WriteSerializedContentToFile][IsFilePathValid] File:{fileInfo.Name}, failed with({info})");
                //File.Delete(filePath);
                return Task.FromResult(result);
            }

            result = DDPMFileSecurity.SetJsonContentFromSerializedString(content, filePath, out info);
            if (!result)
            {
                WriteLog($"[WriteSerializedContentToFile] failed with ({info})");
            }
            return Task.FromResult(result);
        }
        #endregion

        #region Info Key
        public Task AddInfo(string info)
        {
            if (_SysSettingsPlugin != null)
            {
                _SysSettingsPlugin.AddInfo(info);
            }
            return Task.CompletedTask;
        }

        public Task<List<string>> GetInfos(bool force_reload = false)
        {
            List<string> infos = new List<string>();
            if (_SysSettingsPlugin != null)
            {
                infos = _SysSettingsPlugin.GetInfos(force_reload).Result;
            }
            if (infos == null || infos.Count == 0)
            {
                infos = new List<string>(InfoHash.Info_Hash);
                //infos.Add(InfoHash.Info_Hash);
            }
            return Task.FromResult(infos);
        }
        #endregion
    }
}