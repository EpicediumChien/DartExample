#region LicenceHeader

//
// Copyright © 2024, Dell Inc., All Rights Reserved.
// This material is confidential and a trade secret.  Permission to use this
// work for any purpose must be obtained in writing from Dell Inc.
//
// SettingsMangerPlugin.cs created on 06/05/2024T22:12 PM
//

#endregion

using DDPM.SA.Common;
using DDPM.SA.Common.Settings;
using DDPM.SA.Obfuscation;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.Common.Annotations;
using Dell.Client.Framework.Common.PluginConditions;
using Dell.Client.Framework.Interfaces;
using Dell.Client.Framework.Security;
using Microsoft;
using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading.Tasks;
using Windows.Media.AppBroadcasting;
using Windows.Storage;
using DDPM.SA.Obfuscation;
using System.Net.NetworkInformation;
using System.Windows.Interop;
using DDPMSettings = DDPM.SA.Common.Settings;

namespace DDPM.SA.Plugins.SettingsManager
{
    [Plugin(IDs.DDPM_SETTINGS_MANAGER_PLUGIN_ID, pluginName, PluginOrderGroupType.Core, Version = pluginVersion)]
    [Descriptor(Description = pluginDescription)]
    [Publisher(Name = publisherCompany, Website = publisherWebsite, Support = publisherSupport)]
    [PublishedInterface(new[] { typeof(ISettingsManagerIT) })]
    [PublishedUnelevatedInterface(new[] { typeof(ISettingsManagerSA) })]
    public class SettingsMangerPlugin : BaseAgentPlugin, IDisposableObservable, ISettingsManagerSA, ISettingsManagerIT
    {
        public const string PluginLogId = "SettingsManager";

        #region Private Members

        private const string pluginName = "SettingsManagerPlugin";
        private const string pluginVersion = "1.0.0";
        private const string pluginDescription = "This plugin implements Settings Manager Plugin.";
        private const string publisherCompany = "Wistron";
        private const string publisherWebsite = "https://www.wistron.com";
        private const string publisherSupport = "This plugin implements Settings Manager Plugin.";
        //private static string _settingsAccess = SettingsAccess.AppAccessInfo;
        private static string _settingsAccessVer = SettingsAccess.AppAccessVer;
        private static string _settingsAccessAddr = SettingsAccess.AppAccessAddr;

        private IAgent _agent;
        private bool _IsAdministrator = ProcessSecurityHelperWrapper.IsCurrentProcessRunningElevated();
        //private static Dell.Client.Framework.Common.Log _log;
        private enum log_type
        {
            info = 0,
            error
        }

        //Basic
        private static string path_programdata = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Dell");
        private static string folder_product = "Dell Display and Peripheral Manager";
        private static string filename_appsettings_IT = "DDPM.Configs.json";
        private static string filename_appsettings_Info = "DDPM.Infos.json";

        private DDPMITConfig _settings = new DDPMITConfig();
        private InfoObject _infos = new InfoObject();
        private string _settings_path = string.Empty;
        private string _info_path = string.Empty;

        #endregion

        #region Constructor

        public SettingsMangerPlugin(IAgent agent) : base(agent, PluginLogId)
        {
            _agent = agent;
            WriteLog($"SettingsManagerPlugin constructor ...(Admin:{_IsAdministrator})");
        }

        #endregion

        #region ISettingsManagerSA implementation

        public event EventHandler<ITSettingEventArgs> ITSettingsActionEvent;
        public event EventHandler<bool> FWSWUpdateSettingChange;

        //Target to notify User setting
        private void OnITSettingsActionEventNotify(ITSettingEventArgs e)
        {
            if (ITSettingsActionEvent == null || e == null || e == EventArgs.Empty || e.target_object == null)
                return;

            EventHandler<ITSettingEventArgs> Handler = ITSettingsActionEvent;
            if (Handler != null)
            {
                Handler.Invoke(this, e);
                WriteLog($"ITSettingsActionEvent Invoked");
            }

            foreach (var item in e.IT_Feature_TriggerList)
            {
                if (item.Equals("Lock_Settings_Updates"))
                {
                    EventHandler<bool> Handler2 = FWSWUpdateSettingChange;//FW/SW plugin should register this event
                    if (Handler != null)
                    {
                        Handler2.Invoke(this, e.target_object.Lock_Settings_Updates);
                        WriteLog($"FWSWUpdateSettingChange Invoked");
                    }
                }
            }
        }

        public Task<DDPMITConfig> GetITGlobalConfigs(bool force_reload = false)
        {
            WriteLog($"Got ISettingsManagerSA [GetITGlobalConfigs to ReadITConfigData]: Force reload={force_reload}");
            return ReadITConfigData(force_reload);
        }

        //public Task<string> QueryAccessInfo()
        //{
        //    return Task.FromResult(_settingsAccess);
        //}

        public Task<string> QueryAccessInfoVer()
        {
            return Task.FromResult(_settingsAccessVer);
        }

        public Task<string> QueryAccessInfoAddr()
        {
            return Task.FromResult(_settingsAccessAddr);
        }
        #endregion

        #region ISettingsManagerIT implementation        

        public Task<DDPMITConfig> ReadITConfigData(bool force_reload = false)
        {
            if (string.IsNullOrEmpty(_settings_path))
            {
                WriteLog($"ReadITConfigData: Empty system _settings_path, use default data");
                _settings = new DDPMITConfig(); //use it as default settings
                return Task.FromResult(_settings);
            }
            //if (string.IsNullOrEmpty(_settingsAccess))
            //{
            //    WriteLog($"ReadITConfigData: Empty system info access, use default data");
            //    _settings = new DDPMITConfig(); //use it as default settings
            //    return Task.FromResult(_settings);
            //}
            string info = "Success";
            if (force_reload)
            {
                WriteLog($"ReadITConfigData: Force reload");
                if (_settings != null)
                    return Task.FromResult(_settings);

                WriteLog($"ReadITConfigData: null settings, load data from file");
            }
            string serialized_string = DDPMFileSecurity.GetSerializedJsonString(_settings_path, out info);
            _settings = JsonConvert.DeserializeObject<DDPMITConfig>(serialized_string);
            return Task.FromResult(_settings);
        }

        public Task<bool> WriteGlobalSettingsToITConfig(GlobalSettingParam globalSettingParam)
        {
            if (globalSettingParam == null)
            {
                WriteLog($"WriteGlobalSettingsToITConfig: null data, failed");
                return Task.FromResult(false);
            }
            if (_settings == null || _settings.global_setting == null)
            {
                WriteLog($"WriteGlobalSettingsToITConfig: null cache, failed");
                return Task.FromResult(false);
            }
            _settings.global_setting = globalSettingParam;
            string info = string.Empty;
            bool result = DDPMFileSecurity.SetJsonContentFromSerializedString(JToken.FromObject(_settings).ToString(), _settings_path, out info);
            if (!result)
            {
                WriteLog($"WriteGlobalSettingsToITConfig: write failed, reason: {info}");
            }
            return Task.FromResult(result);
        }

        /// <summary>
        /// Write IT feature to config file
        /// </summary>
        /// <param name="data"></param>
        /// <param name="IT_Feature_list">If this param not null or count > 0, tell user settings that IT config some data</param>
        /// <returns></returns>
        public Task<bool> WriteITConfigData(DDPMITConfig data, List<string> IT_Feature_list)
        {
            if (data == null)
            {
                WriteLog($"WriteITConfigData: null data, failed");
                return Task.FromResult(false);
            }
            _settings = data;
            string info = "Success";
            if (!DDPMFileSecurity.SetJsonContentFromSerializedString(JObject.FromObject(_settings).ToString(), _settings_path, out info))
            {
                WriteLog($"WriteITConfigData: write failed. Info({info})");
                return Task.FromResult(false);
            }

            if (IT_Feature_list != null && IT_Feature_list.Count > 0)
            {
                ITSettingEventArgs e = new ITSettingEventArgs();
                List<string> IT_Feature_TriggerList = new List<string>();
                IT_Feature_TriggerList.AddRange(IT_Feature_list);
                e.IT_Feature_TriggerList = IT_Feature_TriggerList;
                e.target_object = data;

                foreach (string feature in IT_Feature_list)
                {
                    PropertyInfo propertyInfo = data.GetType().GetProperty(feature);
                    WriteLog($"[System settings event] {feature} : {propertyInfo.GetValue(_settings)}");
                }
                OnITSettingsActionEventNotify(e);
            }
            return Task.FromResult(true);
        }

        #endregion

        #region Overriding methods

        protected override void OnPluginStarting()
        {
            _agent.PluginManager.PluginsStarted += PluginManagerOnPluginsStarted;

            PluginCondition = new PluginStartedCondition();
            WriteLog("SettingsManager plugin report started");

            InitDDPMITConfigFile();
            InitInfoConfigFile();
            InitRegUpdateLock();
            GetTelemetryRegistryAndApplyData();
        }

        #endregion

        #region IDisposableObservable Support

        /// <summary>
        /// To detect redundant calls
        /// </summary>
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
                }

                IsDisposed = true;
            }
            base.Dispose(disposing);
        }

        #endregion

        #region Event Handler

        private void PluginManagerOnPluginsStarted(object sender, PluginsStartedEventArgs e)
        {
            if (e == null)
                return;
            if (e.ChangedPlugins == null)
                return;
            if (e.ChangedPlugins.Any() == false)
                return;

            if (e.ChangedPlugins.OfType<ISettingsManagerIT>().Any())
            {
                WriteLog("ISettingsManager IT plugin started.");
            }
            if (e.ChangedPlugins.OfType<ISettingsManagerSA>().Any())
            {
                WriteLog("ISettingsManager SA plugin started.");
            }
        }

        #endregion

        #region Private methods
        private void GetTelemetryRegistryAndApplyData()
        {
            bool ret = false;
            string URL = null;
            WriteLog($"{nameof(GetTelemetryRegistryAndApplyData)} start");

            string KeyPath = @"SOFTWARE\Dell\Dell Display And Peripheral Manager";
            string KeyName = @"TelemetryConsent";
            object o = ReadRegistryData(DDPMSettings.RegistryHive.LocalMachine, KeyPath, KeyName).Result;
            if (o == null)
            {
                WriteLog($"{nameof(GetTelemetryRegistryAndApplyData)} Telemetry consent key is null");
                return;
            }
            string result = o.ToString();
            if (result.ToUpper().Equals("TRUE") || result.ToUpper().Equals("FALSE"))
            {
                if (_settings != null && _settings.global_setting != null)
                {
                    _settings.global_setting.isSetTelemetryOverInstaller = true;//config that no consent page shown in UI, DDPMW-1254
                    if (result.ToUpper().Equals("TRUE"))
                        _settings.global_setting.isTelemetryConsentOn = true;
                    if (result.ToUpper().Equals("FALSE"))
                        _settings.global_setting.isTelemetryConsentOn = false;
                    WriteGlobalSettingsToITConfig(_settings.global_setting);
                    WriteRegistryData(DDPMSettings.RegistryHive.LocalMachine, KeyPath, KeyName, "DONE");
                }
                else
                {
                    WriteLog($"{nameof(GetTelemetryRegistryAndApplyData)} system _settings/global is null");
                }
            }
            else
            {
                WriteLog($"{nameof(GetTelemetryRegistryAndApplyData)} Telemetry consent is {o.ToString()}");
                return;
            }

            WriteLog($"{nameof(GetTelemetryRegistryAndApplyData)} done");
        }

        /// <summary>
        /// //
        /// </summary>
        /// <param name="text"></param>
        /// <param name="log_type">0 means info, others means error</param>
        private void WriteLog(string text, log_type log_type = log_type.info)
        {
            text = "[SettingsManager] " + text;
#if DEBUG
            Console.WriteLine(text);
#endif
            if (Log != null)
            {
                if (log_type == log_type.info)
                    Log.Info(text);
                else
                    Log.Error(text);
            }
        }

        private DDPMITConfig InitDDPMITConfigFile()
        {
            string folder = Path.Combine(path_programdata, folder_product);
            string filePath = Path.Combine(folder, filename_appsettings_IT);
            return (DDPMITConfig)InitSysSettingsData("ITConfig", filePath);
        }

        private InfoObject InitInfoConfigFile()
        {
            string folder = Path.Combine(path_programdata, folder_product);
            string filePath = Path.Combine(folder, filename_appsettings_Info);
            InitSysSettingsData("InfoConfig", filePath);

            AddInfo(InfoHash.Info_Hash.Trim());
            return _infos;
        }
        private void InitRegUpdateLock()
        {
            WriteLog($"[InitRegUpdateLock] start");
            if (SettingsAccess.QueryRegistryUpdateLock(out bool isUpdateLock, out string info))
            {
                WriteLog($"[InitRegUpdateLock] QueryRegistryUpdateLock is ok");
                WriteLog($"[InitRegUpdateLock] isUpdateLock is {isUpdateLock}");
                DDPMITConfig DDPMITConfig = ReadITConfigData().Result;
                DDPMITConfig.Lock_Settings_Updates = isUpdateLock;
                if (WriteITConfigData(DDPMITConfig, new List<string>() { "Lock_Settings_Updates" }).Result)
                {
                    WriteLog($"[InitRegUpdateLock] WriteITConfigData is ok");
                    if (SettingsAccess.DeleteRegistryUpdateLock(out string info_2))
                    {
                        WriteLog($"[InitRegUpdateLock] DeleteRegistryUpdateLock is ok");
                    }
                    else
                    {
                        WriteLog($"[InitRegUpdateLock] DeleteRegistryUpdateLock is fail info:{info_2}");
                    }
                }
            }
            else
            {
                WriteLog($"[InitRegUpdateLock] QueryRegistryUpdateLock is fail info:{info}");
            }
            WriteLog($"[InitRegUpdateLock] done");
        }

        public Task<List<string>> GetInfos(bool force_reload = false)
        {
            if (_infos == null || _infos.Infos == null || _infos.Infos.Count == 0)
            {
                _infos = new InfoObject();
                if (_infos.Infos == null)
                {
                    _infos.Infos = new List<string>();
                    _infos.Infos.Add(InfoHash.Info_Hash.Trim());
                    string msg = string.Empty;
                    if (!DDPMFileSecurity.SetJsonContentFromSerializedString(JToken.FromObject(_infos).ToString(), _info_path, out msg))
                    {
                        WriteLog($"[GetInfos] recover data failed: {msg}");
                    }
                    return Task.FromResult(_infos.Infos);
                }
            }
            if (force_reload)
            {
                string msg2 = string.Empty;
                string read = DDPMFileSecurity.GetSerializedJsonString(_info_path, out msg2);
                try
                {
                    InfoObject obj = JsonConvert.DeserializeObject<InfoObject>(read);
                    if (obj != null)
                    {
                        _infos = obj;
                        WriteLog($"[GetInfos] read info config ok");
                    }
                }
                catch (Exception ex)
                {
                    WriteLog($"[GetInfos] read info config failed: ({ex.Message})");
                }
            }
            return Task.FromResult(_infos.Infos);
        }

        public Task AddInfo(string info)
        {
            int idx = -1;
            if (_infos != null && _infos.Infos != null && _infos.Infos.Count > 0)
            {
                WriteLog($"[AddInfo] info count is ({_infos.Infos.Count})");
                idx = _infos.Infos.FindIndex(x => x.Trim().Equals(info));
            }
            if (idx < 0)//means new
            {
                _infos.Infos.Add(info);
                string msg = string.Empty;
                if (!DDPMFileSecurity.SetJsonContentFromSerializedString(JToken.FromObject(_infos).ToString(), _info_path, out msg))
                {
                    WriteLog($"[AddInfo] update data failed: {msg}");
                }
            }
            else
            {
                //exist, bypass
                WriteLog($"[AddInfo] Info exist in list");
            }
            return Task.CompletedTask;
        }

        private object InitSysSettingsData(string type, string filePath)
        {
            FileInfo fInfo = new FileInfo(filePath);
            string folder = fInfo.DirectoryName;// path_programdata + "\\" + folder_product;
            WriteLog($"[InitSysSubagentData][{type}] data folder path: {folder}");
            try
            {
                DirectoryInfo di = System.IO.Directory.CreateDirectory(folder);
                WriteLog($"[{type}]create folder {folder} success");
            }
            catch
            {
                WriteLog($"[{type}]CreateDirectory with {folder} failed.");
                //_settings = null;
                return null;
            }

            if (Directory.Exists(folder))
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(folder);
                if (directoryInfo == null)
                {
                    WriteLog($"[{type}]System config: retrieve Directory got null return");
                    Directory.Delete(folder, true);
                    directoryInfo = System.IO.Directory.CreateDirectory(folder);
                    WriteLog($"[{type}]re-create system settings folder success");
                }
                //AclChecker aclChecker = new AclChecker();
                //if (aclChecker.ContainsUnprivilegedWriteAccess(directoryInfo)) // apply acl at the bottom of function
                string info2 = string.Empty;
                if (DDPMFileSecurity.IsPathSymbolicLinked(folder, out info2))
                {
                    //WriteLog($"[{type}]Directory ACLs for system setting contained unprivileged write access for one or more identity");
                    WriteLog($"[{type}]Directory symbolic check got symlink ({info2})");
                    Directory.Delete(folder, true);
                    WriteLog($"[{type}]Exist folder deleted.");
                    directoryInfo = System.IO.Directory.CreateDirectory(folder);
                    WriteLog($"[{type}]re-create system settings folder success");
                }
            }
            string info = string.Empty;
            //Apply folder ACL
            try
            {
                DDPMFileSecurity.SetFolderPermissions_UserReadAndExecute(folder);
            }
            catch (Exception ex)
            {
                WriteLog($"[InitSysSettingsData][{type}]Apply ACL to folder failed ({ex.Message})");
                return null;
            }
            switch (type)
            {
                case "ITConfig":
                    _settings_path = filePath;
                    break;
                case "InfoConfig":
                    _info_path = filePath;
                    break;
                default:
                    WriteLog($"[InitSysSettingsData] type({type}) is not defined to support");
                    return null;
            }
            //check if setting file contain illegal privilege
            //if yes, delete file and then apply right ACL
            if (File.Exists(filePath))
            {
                FileInfo fileInfo = new FileInfo(filePath);
                if (fileInfo == null)
                {
                    WriteLog($"[{type}]System config: retrieve FileInfo got null return");
                    File.Delete(filePath);
                    WriteLog($"[{type}]Exist file deleted.");
                }
                else
                {
                    AclChecker aclChecker = new AclChecker();
                    if (aclChecker.ContainsUnprivilegedWriteAccess(fileInfo))
                    {
                        WriteLog($"[{type}]File ACLs for system setting contained unprivileged write access for one or more identity");
                        File.Delete(filePath);
                        WriteLog($"[{type}]Exist file deleted.");
                    }
                }
            }

            info = string.Empty;
            bool need_reWrite = true;
            if (File.Exists(filePath))
            {
                string serialized_string = DDPMFileSecurity.GetSerializedJsonString(filePath, out info);
                if (!string.IsNullOrEmpty(serialized_string))
                {
                    switch (type)
                    {
                        case "ITConfig":
                            _settings = JsonConvert.DeserializeObject<DDPMITConfig>(serialized_string);
                            break;
                        case "InfoConfig":
                            _infos = JsonConvert.DeserializeObject<InfoObject>(serialized_string);
                            break;
                        default:
                            WriteLog($"[InitSysSettingsData] type({type}) is not defined to support.2");
                            return null;
                    }
                    need_reWrite = false;
                }
                else
                {
                    WriteLog($"[InitSysSettingsData][{type}] GetSerializedJsonString: " + info);
                }
            }
            object result = null;
            if (need_reWrite)
            {
                bool write = false;
                WriteLog($"[InitSysSettingsData][{type}] *** Init cache from file fail, re-create default settings to file");
                switch (type)
                {
                    case "ITConfig":
                        result = _settings;
                        write = DDPMFileSecurity.SetJsonContentFromSerializedString(JToken.FromObject(_settings).ToString(), filePath, out info);
                        break;
                    case "InfoConfig":
                        result = _infos;
                        write = DDPMFileSecurity.SetJsonContentFromSerializedString(JToken.FromObject(_infos).ToString(), filePath, out info);
                        break;
                    default:
                        WriteLog($"[InitSysSettingsData] type({type}) is not defined to support.3");
                        return null;
                }
                if (!write)
                    WriteLog($"[InitSysSettingsData][{type}] save to file failed: {info}");
                else
                    WriteLog($"[InitSysSettingsData][{type}] re-create file content OK");
            }
            //ACL apply
            if (!DDPMFileSecurity.ApplyFileACLUserReadOnly(filePath, out info))
                WriteLog($"[InitSysSettingsData][{type}] {info}");
            else
                WriteLog($"[InitSysSettingsData][{type}] ACL apply success");

            WriteLog($"[InitSysSettingsData][{type}] finish");
            return result;
        }

        public Task<object> ReadRegistryData(Common.Settings.RegistryHive hive, string keyPath, string keyName)
        {
            try
            {
                object obj = null;
                if (hive == Common.Settings.RegistryHive.CurrentUser)
                {
                    obj = WTSFunction.ImpersonateUser_ReadRegistry(Log, keyPath, keyName);
                    WriteLog("[System setting plugin] Impersonate user read registry finish.");
                }
                else
                {
                    return Task.Run(() =>
                    {
                        obj = DDPMRegistryHelper.ReadRegistryKey(hive, keyPath, keyName);
                        return obj;
                    });
                    //WriteLog("[System setting plugin] read registry finish.");
                }
                if (obj == null)
                    WriteLog($"[System setting plugin] read registry value return null");
                return Task.FromResult(obj);
            }
            catch (Exception e)
            {
                WriteLog($"[System settings plugin] ReadRegistryData exception ({e.Message})");
                return null;
            }
        }

        public Task<bool> WriteRegistryData(Common.Settings.RegistryHive hive, string keyPath, string keyName, object value)
        {
            try
            {
                if (hive == Common.Settings.RegistryHive.CurrentUser)
                {
                    WTSFunction.ImpersonateUser_WriteRegistry(Log, keyPath, keyName, value);
                    WriteLog("[System setting plugin] Impersonate user write OK.");
                }
                else
                {
                    DDPMRegistryHelper.WriteRegistryKey(hive, keyPath, keyName, value);
                    WriteLog($"[System setting plugin] Write data {value} finish");
                }
                return Task.FromResult(true);
            }
            catch (Exception e)
            {
                WriteLog($"[System settings plugin] WriteRegistryData exception ({e.Message})");
                return Task.FromResult(false);
            }
        }

        #endregion
    }

    public class InfoObject
    {
        //string: info value, bool: isActived
        public List<string> Infos { get; set; } = new List<string>();
    }
}