using DDPM.SA.Common.Settings;
using Dell.Client.Framework.Common;
using MS.WindowsAPICodePack.Internal;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using static DDPM.SA.Common.ICLICommandTable;

namespace DDPM.SA.Common.CLI
{
    public class CLIHandlerApp
    {
        public static CLIEventResult CLI_Response_CompleteWithSuccess(CommandLineInput commandLineInput, CLIEventResult rst)
        {
            CLI_RESPONSE response = new CLI_RESPONSE();
            response.TargetFeature = commandLineInput.TargetFeature;
            response.Command = commandLineInput.Command;
            response.Message = "Operation Completed";
            response.Result = "Success";
            response.Value = "N/A";
            rst.serialize_Json_response = JsonConvert.SerializeObject(response, Formatting.Indented);
            rst.ExitCode = (int)CLI_ExitCode.success;
            return rst;
        }
        public static CLIEventResult CLI_Response_CommandNotSupport(CommandLineInput commandLineInput, CLIEventResult rst)
        {
            CLI_RESPONSE response = new CLI_RESPONSE();
            response.TargetFeature = commandLineInput.TargetFeature;
            response.Command = commandLineInput.Command;
            response.Message = $"Command not support: {commandLineInput.Command}";
            response.Result = "FAIL";
            response.Value = "N/A";
            rst.serialize_Json_response = JsonConvert.SerializeObject(response, Formatting.Indented);
            rst.ExitCode = (int)CLI_ExitCode.command_not_support;
            return rst;
        }

        public static CLIEventResult CLI_Response_TypeNotSupport(CommandLineInput commandLineInput, CLIEventResult rst)
        {
            CLI_RESPONSE response = new CLI_RESPONSE();
            response.TargetFeature = commandLineInput.TargetFeature;
            response.Command = commandLineInput.Command;
            response.Message = $"Command \"{commandLineInput.PluginsType}\" doesn't support for feature \"{commandLineInput.TargetFeature}\".";
            response.Result = "FAIL";
            response.Value = "N/A";
            rst.serialize_Json_response = JsonConvert.SerializeObject(response, Formatting.Indented);
            rst.ExitCode = (int)CLI_ExitCode.command_targettype_not_support;
            return rst;
        }

        public static CLIEventResult CLI_Response_OptionMissing(CommandLineInput commandLineInput, CLIEventResult rst)
        {
            CLI_RESPONSE response = new CLI_RESPONSE();
            response.TargetFeature = commandLineInput.TargetFeature;
            response.Command = commandLineInput.Command;
            response.Result = "FAIL";
            response.Value = "N/A";
            response.Message = "Option is missing";
            rst.serialize_Json_response = JsonConvert.SerializeObject(response, Formatting.Indented);
            rst.ExitCode = (int)CLI_ExitCode.fail_option_missing;
            return rst;
        }

        public static CLIEventResult CLI_Response_NotSupport_MultipleOptions(CommandLineInput commandLineInput, CLIEventResult rst)
        {
            CLI_RESPONSE response = new CLI_RESPONSE();
            response.TargetFeature = commandLineInput.TargetFeature;
            response.Command = commandLineInput.Command;
            response.Result = "FAIL";
            response.Message = $"{commandLineInput.TargetFeature}: doesn't support multiple options";
            response.Value = "N/A";
            rst.ExitCode = (int)CLI_ExitCode.fail_analytics_option_notsupport;
            rst.serialize_Json_response = JsonConvert.SerializeObject(response, Formatting.Indented);
            return rst;
        }

        public static CLIEventResult CLI_Response_OptionNameNotSupport(CommandLineInput commandLineInput, CLIEventResult rst, CommandType_Option op)
        {
            CLI_RESPONSE response = new CLI_RESPONSE();
            response.TargetFeature = commandLineInput.TargetFeature;
            response.Command = commandLineInput.Command;
            response.Result = "FAIL";
            response.Message = $"Option name [{op.Option_Name}] not support";
            response.Value = "N/A";
            rst.ExitCode = (int)CLI_ExitCode.fail_option_name;
            rst.serialize_Json_response = JsonConvert.SerializeObject(response, Formatting.Indented);
            return rst;
        }
        
        public static CLIEventResult CLI_Response_OptionValueNotSupport(CommandLineInput commandLineInput, CLIEventResult rst, CommandType_Option op)
        {
            CLI_RESPONSE response = new CLI_RESPONSE();
            response.TargetFeature = commandLineInput.TargetFeature;
            response.Command = commandLineInput.Command;
            response.Result = "FAIL";
            response.Message = $"Option value [{op.Option_Value}] not support";
            response.Value = "N/A";
            rst.ExitCode = (int)CLI_ExitCode.fail_option_value;
            rst.serialize_Json_response = JsonConvert.SerializeObject(response, Formatting.Indented);
            return rst;
        }

        public static CLIEventResult CLI_SW_FW_Update(ILog Log, object inputData, object settingsPlugin, CommandLineInput commandLineInput, string action_guid)
        {
            //Expected format:
            // IT > /configure -app=InAppUpdate -value=lock / unlock
            // IT > /get -app=InAppUpdate
            CLIEventResult result = new CLIEventResult();
            result.ticket = DateTime.Now;
            result.command_guid_string = action_guid;

            CLI_RESPONSE response = new CLI_RESPONSE();
            response.Command = commandLineInput.Command;
            response.TargetFeature = commandLineInput.TargetFeature;

            if (!commandLineInput.Command.Equals("CONFIGURE") && !commandLineInput.Command.Equals("GET"))
            {
                WriteLog(Log, $"FW/SW update: the command should be configure or get, fail");
                return CLI_Response_CommandNotSupport(commandLineInput, result);
            }            

            Type type = inputData.GetType();
            Type type2 = settingsPlugin.GetType();
            DDPMSettings data_user = type == typeof(DDPMSettings) ? (DDPMSettings)inputData : null;
            DDPMITConfig data_IT = type == typeof(DDPMITConfig) ? (DDPMITConfig)inputData : null;
            WriteLog(Log, $"Output interface log: [{settingsPlugin.GetType()}],[{settingsPlugin.GetType().Name}]");
            ISettingsManagerIT _SettingsPluginIT = type2.Name == "SettingsMangerPlugin" ? (ISettingsManagerIT)settingsPlugin : null;
            IDeviceManagerSA _DeviceManagerPlugin = type2.Name == "DeviceMangerPlugin" ? (IDeviceManagerSA)settingsPlugin : null;

            if (commandLineInput.Command.Equals("GET"))
            {
                if (data_user != null)
                { 
                    data_IT = data_user != null ? data_user.LockSettings : null;
                }
                if (data_IT != null)
                {
                    response.Message = "Operation Completed";
                    response.Result = "Success";
                    response.Value = data_IT.Lock_Settings_Updates ? "Lock" : "Unlock";
                    result.serialize_Json_response = JsonConvert.SerializeObject(response, Formatting.Indented);
                    result.ExitCode = (int)CLI_ExitCode.success;
                    return result;
                }
                else
                {
                    response.Message = "Operation failed";
                    response.Result = "Retrieve data failed";
                    response.Value = "N/A";
                    result.serialize_Json_response = JsonConvert.SerializeObject(response, Formatting.Indented);
                    result.ExitCode = (int)CLI_ExitCode.success;
                    return result;
                }
            }

            if (commandLineInput.Options == null || commandLineInput.Options.Count == 0)
            {
                WriteLog(Log, "FW/SW update: command SET without option, fail");
                return CLI_Response_OptionMissing(commandLineInput, result);
            }

            CommandType_Option op = commandLineInput.Options[0];
            if (!op.Option_Name.ToUpper().Equals("VALUE"))
            {
                WriteLog(Log, $"FW/SW update: option name [{op.Option_Name}] not support");
                return CLI_Response_OptionNameNotSupport(commandLineInput, result, op);
            }
            string value = op.Option_Value;
            if (value.ToUpper().Equals("LOCK"))
            {
                if (data_IT != null)
                {
                    data_IT.Lock_Settings_Updates = true;
                }
                if (data_user != null)
                {
                    data_user.LockSettings.Lock_Settings_Updates = true;
                }
            }
            else if (value.ToUpper().Equals("UNLOCK"))
            {
                if (data_IT != null)
                {
                    data_IT.Lock_Settings_Updates = false;
                }
                if (data_user != null)
                {
                    data_user.LockSettings.Lock_Settings_Updates = false;
                }
            }
            else
            {
                WriteLog(Log, $"FW/SW update: option value [{value}] not support");
                return CLI_Response_OptionValueNotSupport(commandLineInput, result, op);
            }

            bool status = false;
            if (_SettingsPluginIT != null)
            {
                if (data_IT != null)
                    status = _SettingsPluginIT.WriteITConfigData(data_IT, new List<string>() { "Lock_Settings_Updates" }).Result;
            }
            if (_DeviceManagerPlugin != null)
            {
                if (data_user != null)
                    status = _DeviceManagerPlugin.SetAppConfigData(data_user).Result;
            }

            if (!status)
            {             
                response.Message = "Failed to update config";
                response.Result = "FAIL";
                response.Value = op.Option_Value;
                result.serialize_Json_response = JsonConvert.SerializeObject(response, Formatting.Indented);
                result.ExitCode = (int)CLI_ExitCode.fail_SetSettings_ITSettingsValue;
                return result;
            }

            return CLI_Response_CompleteWithSuccess(commandLineInput, result);
        }

        public static CLIEventResult CLI_App_LockUnlock(ILog Log, object inputData, object settingsPlugin, CommandLineInput commandLineInput, string action_guid)
        {
            //Expected format:
            // IT > /configure -app=InAppUpdate -value=lock / unlock
            // IT > /get -app=InAppUpdate
            CLIEventResult result = new CLIEventResult();
            result.ticket = DateTime.Now;
            result.command_guid_string = action_guid;

            CLI_RESPONSE response = new CLI_RESPONSE();
            response.Command = commandLineInput.Command;
            response.TargetFeature = commandLineInput.TargetFeature;

            List<string> CMDLine_Command_Check = new List<string>()
                {
                    "GET",
                    "SET",
                    "CONFIGURE",
                };
            Debug.WriteLine($"command {commandLineInput.Command}");
            //if (!commandLineInput.Command.Equals("CONFIGURE") && !commandLineInput.Command.Equals("GET"))
            if (CMDLine_Command_Check.FindIndex(x => x.Equals(commandLineInput.Command)) < 0)
            {
                WriteLog(Log, $"{commandLineInput.Command}: the command should be configure, set or get, fail");
                return CLI_Response_CommandNotSupport(commandLineInput, result);
            }

            Type type = inputData.GetType();
            Type type2 = settingsPlugin.GetType();
            DDPMSettings data_user = type == typeof(DDPMSettings) ? (DDPMSettings)inputData : null;
            DDPMITConfig data_IT = type == typeof(DDPMITConfig) ? (DDPMITConfig)inputData : null;
            WriteLog(Log, $"Output interface log: [{settingsPlugin.GetType()}],[{settingsPlugin.GetType().Name}]");
            ISettingsManagerIT _SettingsPluginIT = type2.Name == "SettingsMangerPlugin" ? (ISettingsManagerIT)settingsPlugin : null;
            IDeviceManagerSA _DeviceManagerPlugin = type2.Name == "DeviceMangerPlugin" ? (IDeviceManagerSA)settingsPlugin : null;

            if (commandLineInput.Command.Equals("GET"))
            {
                if (data_user != null)
                {
                    data_IT = data_user != null ? data_user.LockSettings : null;
                }
                if (data_IT != null)
                {
                    response.Message = "Operation Completed";
                    response.Result = "Success";
                    switch (commandLineInput.TargetFeature)
                    {
                        case "INAPPUPDATE":
                            response.Value = data_IT.Lock_Settings_Updates ? "Lock" : "Unlock";
                            break;
                        case "INAPPEXPORTSETTINGS":
                            response.Value = data_IT.Lock_Display_ExportSettings ? "Lock" : "Unlock";
                            break;
                    }
                    result.serialize_Json_response = JsonConvert.SerializeObject(response, Formatting.Indented);
                    result.ExitCode = (int)CLI_ExitCode.success;
                    return result;
                }
                else
                {
                    response.Message = "Operation failed";
                    response.Result = "Retrieve data failed";
                    response.Value = "N/A";
                    result.serialize_Json_response = JsonConvert.SerializeObject(response, Formatting.Indented);
                    result.ExitCode = (int)CLI_ExitCode.success;
                    return result;
                }
            }

            if (commandLineInput.Options == null || commandLineInput.Options.Count == 0)
            {
                WriteLog(Log, $"{commandLineInput.TargetFeature}: command SET without option, fail");
                return CLI_Response_OptionMissing(commandLineInput, result);
            }

            CommandType_Option op = commandLineInput.Options[0];
            if (!op.Option_Name.ToUpper().Equals("VALUE"))
            {
                WriteLog(Log, $"{commandLineInput.TargetFeature}: option name [{op.Option_Name}] not support");
                return CLI_Response_OptionNameNotSupport(commandLineInput, result, op);
            }
            string value = op.Option_Value;
            if (value.ToUpper().Equals("LOCK"))
            {
                if (data_IT != null)
                {
                    switch (commandLineInput.TargetFeature)
                    {
                        case "INAPPUPDATE":
                            data_IT.Lock_Settings_Updates = true;
                            break;
                        case "INAPPEXPORTSETTINGS":
                            data_IT.Lock_Display_ExportSettings = true;
                            break;
                    }
                    Debug.WriteLine($"{data_IT.Lock_Settings_Updates}");
                }
                if (data_user != null)
                {
                    switch (commandLineInput.TargetFeature)
                    {
                        case "INAPPUPDATE":
                            data_user.LockSettings.Lock_Settings_Updates = true;
                            break;
                        case "INAPPEXPORTSETTINGS":
                            data_user.LockSettings.Lock_Display_ExportSettings = true;
                            break;
                    }
                    Debug.WriteLine($"{data_user.LockSettings.Lock_Settings_Updates}");
                }
            }
            else if (value.ToUpper().Equals("UNLOCK"))
            {
                if (data_IT != null)
                {
                    switch (commandLineInput.TargetFeature)
                    {
                        case "INAPPUPDATE":
                            data_IT.Lock_Settings_Updates = false;
                            break;
                        case "INAPPEXPORTSETTINGS":
                            data_IT.Lock_Display_ExportSettings = false;
                            break;
                    }
                }
                if (data_user != null)
                {
                    switch (commandLineInput.TargetFeature)
                    {
                        case "INAPPUPDATE":
                            data_user.LockSettings.Lock_Settings_Updates = false;
                            break;
                        case "INAPPEXPORTSETTINGS":
                            data_user.LockSettings.Lock_Display_ExportSettings = false;
                            break;
                    }
                    data_user.LockSettings.Lock_Settings_Updates = false;
                }
            }
            else
            {
                WriteLog(Log, $"{commandLineInput.TargetFeature}: option value [{value}] not support");
                return CLI_Response_OptionValueNotSupport(commandLineInput, result, op);
            }

            bool status = false;
            if (_SettingsPluginIT != null)
            {
                if (data_IT != null)
                {
                    switch (commandLineInput.TargetFeature)
                    {
                        case "INAPPUPDATE":
                            status = _SettingsPluginIT.WriteITConfigData(data_IT, new List<string>() { $"Lock_Settings_Updates" }).Result;
                            break;
                        case "INAPPEXPORTSETTINGS":
                            status = _SettingsPluginIT.WriteITConfigData(data_IT, new List<string>() { $"Lock_Display_ExportSettings" }).Result;
                            break;
                    }
                }
                Debug.WriteLine($"{status}");
            }
            if (_DeviceManagerPlugin != null)
            {
                if (data_user != null)
                    status = _DeviceManagerPlugin.SetAppConfigData(data_user).Result;
            }

            if (!status)
            {
                response.Message = "Failed to update config";
                response.Result = "FAIL";
                response.Value = op.Option_Value;
                result.serialize_Json_response = JsonConvert.SerializeObject(response, Formatting.Indented);
                result.ExitCode = (int)CLI_ExitCode.fail_SetSettings_ITSettingsValue;
                return result;
            }

            return CLI_Response_CompleteWithSuccess(commandLineInput, result);
        }

        public static CLIEventResult CLI_Analytics_Consent(ILog Log, object inputData, object settingsPlugin, CommandLineInput commandLineInput, string action_guid)
        {
            //Expected format:
            // IT    > /set -app=TelemetryConsent -value=true,lock / true,unlock / false,lock / false,unlock
            // Normal> /set -app=TelemetryConsent -value=true / false
            // Normal> /get -app=TelemetryConsent
            CLIEventResult result = new CLIEventResult();
            result.ticket = DateTime.Now;
            result.command_guid_string = action_guid;

            CLI_RESPONSE response = new CLI_RESPONSE();
            response.Command = commandLineInput.Command;
            response.TargetFeature = commandLineInput.TargetFeature;

            if (commandLineInput.Command.Equals("SET"))//for SET command, option should not be empty.
            {
                if (commandLineInput.Options == null || commandLineInput.Options.Count == 0)
                {
                    WriteLog(Log, "Telemetry Consent: command SET without option, fail");
                    return CLI_Response_OptionMissing(commandLineInput, result);
                }
                if (commandLineInput.Options.Count > 1)
                {
                    WriteLog(Log, "Telemetry Consent: doesn't support multiple value options");
                    return CLI_Response_NotSupport_MultipleOptions(commandLineInput, result);
                }
            }
            Type type = inputData.GetType();
            Type type2 = settingsPlugin.GetType();
            DDPMSettings data_user = type == typeof(DDPMSettings) ? (DDPMSettings)inputData : null;
            DDPMITConfig data_IT = type == typeof(DDPMITConfig) ? (DDPMITConfig)inputData : null;
            WriteLog(Log, $"Output interface log: [{settingsPlugin.GetType()}],[{settingsPlugin.GetType().Name}]");
            ISettingsManagerIT _SettingsPluginIT = type2.Name == "SettingsMangerPlugin" ? (ISettingsManagerIT)settingsPlugin : null;
            IDeviceManagerSA _DeviceManagerPlugin = type2.Name == "DeviceMangerPlugin" ? (IDeviceManagerSA)settingsPlugin : null;

            //GET is for user mode using
            if (commandLineInput.Command.Equals("GET")) //ex: cli.exe /get -app=TelemetryConsent
            {
                if (data_user == null)
                {
                    response.Message = "Fail to read application setting";
                    result.serialize_Json_response = JsonConvert.SerializeObject(response, Formatting.Indented);
                    result.ExitCode = (int)CLI_ExitCode.fail_read_settings;
                    return result;
                }
                if (data_user.UserSettings == null || data_user.LockSettings == null)
                {
                    response.Message = "Got empty setting";
                    result.serialize_Json_response = JsonConvert.SerializeObject(response, Formatting.Indented);
                    result.ExitCode = (int)CLI_ExitCode.fail_read_settings;
                    return result;
                }
                if (commandLineInput.Options.Count > 0)
                {
                    response.Message = "GET command doesn't support options";
                    result.serialize_Json_response = JsonConvert.SerializeObject(response, Formatting.Indented);
                    result.ExitCode = (int)CLI_ExitCode.fail_analytics_option_notsupport;
                    return result;
                }

                Console.WriteLine($"Telemetry Consent: is function enable? => {data_user.UserSettings.isTelemetryConsentOn}");
                Console.WriteLine($"Telemetry Consent: is Locked? => = {data_user.LockSettings.Lock_Settings_TelemetryConsent}");
                response.Value = (data_user.UserSettings.isTelemetryConsentOn ? "true," : "false,") + (data_user.LockSettings.Lock_Settings_TelemetryConsent ? "Lock" : "Unlock");
                response.Result = "Completed";
                result.serialize_Json_response = JsonConvert.SerializeObject(response, Formatting.Indented);
                result.ExitCode = (int)CLI_ExitCode.success;

                return result;
            }
            //ex: cli.exe /set -app=TelemetryConsent -value=true / false <= for user
            //                                              lock / unlock <= for IT
            //                                              true,lock / true,unlock / false,lock / false,unlock <= for IT+user
            else if (commandLineInput.Command.Equals("SET"))
            {
                CommandType_Option op = commandLineInput.Options[0];

                if (!op.Option_Name.ToUpper().Equals("VALUE"))
                {
                    WriteLog(Log, $"Telemetry Consent: option name [{op.Option_Name}] not support");
                    return CLI_Response_OptionNameNotSupport(commandLineInput, result, op);
                }

                op.Option_Value.Replace(".", ",");
                List<string> values = op.Option_Value.Split(",").ToList();

                foreach (string value in values)
                {
                    if (value.ToUpper().Equals("TRUE"))
                    {
                        if (data_user != null)
                            data_user.UserSettings.isTelemetryConsentOn = true;
                    }
                    else if (value.ToUpper().Equals("FALSE"))
                    {
                        if (data_user != null)
                            data_user.UserSettings.isTelemetryConsentOn = false;
                    }
                    else if (value.ToUpper().Equals("LOCK"))
                    {
                        if (data_IT != null)
                        {
                            data_IT.Lock_Settings_TelemetryConsent = true;
                        }
                        if (data_user != null)
                        {
                            data_user.LockSettings.Lock_Settings_TelemetryConsent = true;
                        }
                    }
                    else if (value.ToUpper().Equals("UNLOCK"))
                    {
                        if (data_IT != null)
                        {
                            data_IT.Lock_Settings_TelemetryConsent = false;
                        }
                        if (data_user != null)
                        {
                            data_user.LockSettings.Lock_Settings_TelemetryConsent = false;
                        }
                    }
                    else
                    {
                        response.Message = $"Telemetry Consent: value format error with [{value}]";
                        Console.WriteLine(response.Message);
                        WriteLog(Log, response.Message);
                        return CLI_Response_OptionValueNotSupport(commandLineInput, result, op);
                    }
                    // continue; // 20240827 Refactor the containing loop
                }
                bool status = false;
                if (_SettingsPluginIT != null)
                {
                    if (data_IT != null)
                        status = _SettingsPluginIT.WriteITConfigData(data_IT, new List<string>() { "Lock_Settings_TelemetryConsent" }).Result;
                }
                if (_DeviceManagerPlugin != null)
                {
                    if (data_user != null)
                        status = _DeviceManagerPlugin.SetAppConfigData(data_user).Result;
                }

                if (status)
                {
                    response.Result = "Completed";
                    response.Value = op.Option_Value;
                    result.serialize_Json_response = JsonConvert.SerializeObject(response, Formatting.Indented);
                    result.ExitCode = (int)CLI_ExitCode.success;
                    return result;
                }
                else
                {
                    response.Message = "Failed to update config";
                    response.Result = "FAIL";
                    response.Value = op.Option_Value;
                    result.serialize_Json_response = JsonConvert.SerializeObject(response, Formatting.Indented);
                    result.ExitCode = (int)CLI_ExitCode.fail_SetSettings_ITSettingsValue;
                    return result;
                }
            }
            else
            {
                response.Message = $"Command not support: {commandLineInput.Command}";
                response.Result = "FAIL";
                response.Value = "N/A";
                result.serialize_Json_response = JsonConvert.SerializeObject(response, Formatting.Indented);
                result.ExitCode = (int)CLI_ExitCode.command_not_support;
                return result;
            }
        }

        /// <summary>
        /// //
        /// </summary>
        /// <param name="text"></param>
        /// <param name="log_type">0 means info, others means error</param>
        private static void WriteLog(ILog Log, string text, log_type log_type = log_type.info)
        {
            text = "[CLIHandlerApp] " + text;
            Console.WriteLine(text);
            if (log_type == log_type.info)
                Log.Info(text);
            else
                Log.Error(text);
        }

        private enum log_type
        {
            info = 0,
            error
        }
    }
}