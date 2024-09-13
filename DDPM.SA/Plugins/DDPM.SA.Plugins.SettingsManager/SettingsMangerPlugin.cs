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
using Windows.Storage;

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
        private static string _settingsAccess = SettingsAccess.AppAccessInfo;
        private static string _settingsAccessVer = SettingsAccess.AppAccessVer;
        private static string _settingsAccessAddr = SettingsAccess.AppAccessAddr;
        private static string _NKVM_Log_GUID = SettingsAccess.NKVM_Log;

        private IAgent _agent;
        private bool _IsAdministrator = ProcessSecurityHelperWrapper.IsCurrentProcessRunningElevated();
        //private static Dell.Client.Framework.Common.Log _log;
        private enum log_type
        {
            info = 0,
            error
        }

        //Basic
        private static string path_programdata = Path.Combine( Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Dell");
        private static string folder_product = "Dell Display and Peripheral Manager";
        private static string filename_appsettings_IT = "DDPM.Configs.json";

        private DDPMITConfig _settings;
        private string _settings_path;

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

        //Target to notify User setting
        private void OnITSettingsActionEventNotify(ITSettingEventArgs e)
        {
            if (ITSettingsActionEvent == null || e == null || e == EventArgs.Empty)
                return;

            EventHandler<ITSettingEventArgs> Handler = ITSettingsActionEvent;
            if (Handler != null)
            {
                Handler.Invoke(this, e);
                WriteLog($"ITSettingsActionEvent Invoked");
            }
        }

        public Task<DDPMITConfig> GetITGlobalConfigs(bool force_reload = false)
        {
            WriteLog($"Got ISettingsManagerSA [GetITGlobalConfigs to ReadITConfigData]: Force reload={force_reload}");
            return ReadITConfigData(force_reload);
        }

        public Task<string> QueryAccessInfo()
        {
            return Task.FromResult(_settingsAccess);
        }

        public Task<string> QueryAccessInfoVer()
        {
            return Task.FromResult(_settingsAccessVer);
        }
        
        public Task<string> QueryAccessInfoAddr()
        {
            return Task.FromResult(_settingsAccessAddr);
        }
        //0913 Add by Bruce
        public Task<string> QueryNKVMLog()
        {
            return Task.FromResult(_NKVM_Log_GUID);
        }
        #endregion

        #region ISettingsManagerIT implementation

        public Task<DDPMITConfig> ReadITConfigData(bool force_reload = false)
        {
            string info = "Success";
            if (force_reload)
            {
                WriteLog($"ReadITConfigData: Force reload");
                if (_settings != null)
                    return Task.FromResult(_settings);

                WriteLog($"ReadITConfigData: null settings, load data from file");
            }
            string serialized_string = DDPMFileSecurity.GetSerializedJsonString(_settingsAccess, _settings_path, out info);
            _settings = JsonConvert.DeserializeObject<DDPMITConfig>(serialized_string);
            return Task.FromResult(_settings);
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
            if (!DDPMFileSecurity.SetJsonContentFromSerializedString(_settingsAccess, JObject.FromObject(_settings).ToString(), _settings_path, out info))
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

        /// <summary>
        /// //
        /// </summary>
        /// <param name="text"></param>
        /// <param name="log_type">0 means info, others means error</param>
        private void WriteLog(string text, log_type log_type = log_type.info)
        {
            text = "[SettingsManager] " + text;
            Console.WriteLine(text);
            if (log_type == log_type.info)
                Log.Info(text);
            else
                Log.Error(text);
        }

        private DDPMITConfig InitDDPMITConfigFile()
        {
            string folder = path_programdata + "\\" + folder_product;
            WriteLog($"IT admin data folder path: {folder}");
            try
            {
                DirectoryInfo di = System.IO.Directory.CreateDirectory(folder);
                WriteLog($"create folder {folder} success");
            }
            catch
            {
                WriteLog($"CreateDirectory with {folder} failed.");
                _settings = null;
                return null;
            }

            //Dean: below code has creation procedure, no need file path check at here
            //Elsa Add Security
            //string FileInfo;
            //if (!DDPMFileSecurity.IsFolderPathValid(folder, out FileInfo))
            //{
            //    _log.Info($"{nameof(InitDDPMITConfigFile)} {FileInfo}");
            //    return null;
            //}

            //check if setting file contain illegal privilege
            //if yes, delete file and then apply right ACL
            //Apply symlink check here as well [Dean 0912]
            if (Directory.Exists(folder))
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(folder);
                if (directoryInfo == null)
                {
                    WriteLog("System config: retrieve Directory got null return");
                    Directory.Delete(folder, true);
                    directoryInfo = System.IO.Directory.CreateDirectory(folder);
                    WriteLog($"re-create system settings folder success");
                }
                AclChecker aclChecker = new AclChecker();
                if (aclChecker.ContainsUnprivilegedWriteAccess(directoryInfo))
                {
                    WriteLog("Directory ACLs for system setting contained unprivileged write access for one or more identity");
                    Directory.Delete(folder, true);
                    WriteLog("Exist folder deleted.");
                    directoryInfo = System.IO.Directory.CreateDirectory(folder);
                    WriteLog($"re-create system settings folder success");
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
                WriteLog($"Apply ACL to folder failed ({ex.Message})");
                return null;
            }
            //if (!DDPMFileSecurity.CheckFolderACL(folder, out info, true))
            //{
            //    WriteLog($"Apply ACL to folder failed. ({info})");
            //    return null;
            //}
            _settings_path = folder + "\\" + filename_appsettings_IT;
            //WriteLog($"_settings_path is {_settings_path}."); //SDL to remove (not allow path in log)

            //check if setting file contain illegal privilege
            //if yes, delete file and then apply right ACL
            if (File.Exists(_settings_path))
            {
                FileInfo fileInfo = new FileInfo(_settings_path);
                if (fileInfo == null)
                {
                    WriteLog("System config: retrieve FileInfo got null return");
                    File.Delete(_settings_path);
                    WriteLog("Exist file deleted.");
                }
                else
                {
                    AclChecker aclChecker = new AclChecker();
                    if (aclChecker.ContainsUnprivilegedWriteAccess(fileInfo))
                    {
                        WriteLog("File ACLs for system setting contained unprivileged write access for one or more identity");
                        File.Delete(_settings_path);
                        WriteLog("Exist file deleted.");
                    }                    
                }
            }

            DDPMITConfig ddpm_it = new DDPMITConfig();
            info = string.Empty;
            if (File.Exists(_settings_path))
            {
                string serialized_string = DDPMFileSecurity.GetSerializedJsonString(_settingsAccess, _settings_path, out info);
                if (!string.IsNullOrEmpty(serialized_string))
                    _settings = JsonConvert.DeserializeObject<DDPMITConfig>(serialized_string);
                else
                {
                    WriteLog("[InitDDPMITConfigFile] GetSerializedJsonString: " + info);
                    _settings = ddpm_it;
                    if (_settings != null)
                    {
                        WriteLog("[InitDDPMITConfigFile] *** Init cache from file fail, re-create default settings to file");
                        if (DDPMFileSecurity.SetJsonContentFromSerializedString(_settingsAccess, JObject.FromObject(_settings).ToString(), _settings_path, out info))
                            WriteLog("[InitDDPMITConfigFile] re-create file content OK");
                        else
                            WriteLog("[InitDDPMITConfigFile] save to file failed, please check file access right!!");
                    }
                }
            }
            else
            {
                FileInfo fileInfo = new FileInfo(_settings_path);

                WriteLog("[InitDDPMITConfigFile] settings file not exist, new an object");
                _settings = new DDPMITConfig();
                //init data to file
                if (DDPMFileSecurity.SetJsonContentFromSerializedString(_settingsAccess, JObject.FromObject(_settings).ToString(), _settings_path, out info))
                {
                    WriteLog("[InitDDPMITConfigFile] settings file create and write success");
                }
                else
                {
                    WriteLog("[InitDDPMITConfigFile] settings file create and write failed");
                }
            }
            //ACL apply
            //string info;
            if (!DDPMFileSecurity.ApplyFileACLUserReadOnly(_settings_path, out info))
                WriteLog($"[InitDDPMUserConfigFile] {info}");

            return _settings;
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
                return Task.FromResult( obj );
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
}