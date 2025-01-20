using DDPM.SA.Common.Display;
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
using VcpCore.Common;
using static DDPM.SA.Common.ICLICommandTable;

namespace DDPM.SA.Common.CLI
{
    public class CLIHandlerApp
    {
        public static CLIEventResult CLI_Response_CompleteWithSuccess(CommandLineInput commandLineInput, CLIEventResult rst)
        {
            APP_RESPONSE response = new APP_RESPONSE();
            response.TargetFeature = commandLineInput.TargetFeature;
            response.Command = commandLineInput.Command;
            response.Message = "Operation Completed";
            response.Result = "Pass";
            response.Value = !string.IsNullOrWhiteSpace(commandLineInput.Options[0].Option_Value) ? commandLineInput.Options[0].Option_Value : "N/A";
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
            Debug.WriteLine($"debug");
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
                    response.Result = "Pass";
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
            if (string.IsNullOrEmpty(op.Option_Name) || 
                !op.Option_Name.ToUpper().Equals("VALUE"))
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
            if (_SettingsPluginIT != null && data_IT != null)
            {
                status = _SettingsPluginIT.WriteITConfigData(data_IT, new List<string>() { "Lock_Settings_Updates" }).Result;
            }
            if (_DeviceManagerPlugin != null && data_user != null)
            {
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
            return CLI_Common_LockUlockWithUserAction(Log, inputData, settingsPlugin, commandLineInput, action_guid);
        }

        public static CLIEventResult CLI_App_LockUnlock(ILog Log, object inputData, object settingsPlugin, CommandLineInput commandLineInput, string action_guid)
        {
            return CLI_Common_LockUnlock_WithoutUserAction(Log, inputData, settingsPlugin, commandLineInput, action_guid);
        }

        public static CLIEventResult CLI_Common_LockUnlock_WithoutUserAction(ILog Log, object inputData, object settingsPlugin, CommandLineInput commandLineInput, string action_guid)
        {
            //Expected format:
            // IT > /configure -app=InAppUpdate -value=lock / unlock
            // IT > /get -app=InAppUpdate
            CLIEventResult result = new CLIEventResult();
            result.ticket = DateTime.Now;
            result.command_guid_string = action_guid;

            APP_RESPONSE response = new APP_RESPONSE();
            response.Command = commandLineInput.Command;
            response.TargetFeature = commandLineInput.TargetFeature;

            List<string> CMDLine_Command_Check = new List<string>()
            {
                "GET",
                "SET",
            };

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
            if(_DeviceManagerPlugin != null)
            {
                if (_DeviceManagerPlugin.GetMonitors().Result.Count == 0 && commandLineInput.PluginsType.ToUpper() == "DISPLAY")
                {
                    response.Message = "No devices found";
                    result.serialize_Json_response = JsonConvert.SerializeObject(response, Formatting.Indented);
                result.ExitCode = (int)CLI_ExitCode.no_monitor_connected;
                    return result;
                }
                if (_DeviceManagerPlugin.GetDevices().Result.deviceInfo.Count == 0 && commandLineInput.PluginsType.ToUpper() != "DISPLAY" && commandLineInput.PluginsType.ToUpper() != "APP")
                {
                    response.Message = "No devices found";
                    result.serialize_Json_response = JsonConvert.SerializeObject(response, Formatting.Indented);
                    result.ExitCode = (int)CLI_ExitCode.null_device_manager;
                    return result;
                }
            }
            if (commandLineInput.Command.Equals("GET"))
            {
                if (data_user != null)
                {
                    data_IT = data_user != null ? data_user.LockSettings : null;
                }
                if (data_IT != null)
                {
                    response.Message = "Operation Completed";
                    response.Result = "Pass";
                    switch (commandLineInput.TargetFeature)
                    {
                        case "INAPPUPDATE":
                            response.Value = data_IT.Lock_Settings_Updates ? "Lock" : "Unlock";
                            break;
                        case "INAPPEXPORTIMPORT":
                            response.Value = data_IT.Lock_Display_ExportSettings ? "Lock" : "Unlock";
                            break;
                        case "RESTOREFACTORYDEFAULTS":
                            switch (commandLineInput.PluginsType.ToUpper())
                            {
                                case "DISPLAY":
                                    response.Value = data_IT.Lock_Display_RestoreFactoryDefaults ? "Lock" : "Unlock";
                                    break;
                                case "PEN":
                                    response.Value = data_IT.Lock_Pen_RestoreFactoryDefaults ? "Lock" : "Unlock";
                                    break;
                                case "WEBCAM":
                                    response.Value = data_IT.Lock_Webcam_RestoreFactoryDefaults ? "Lock" : "Unlock";
                                    break;
                                case "KEYBOARD":
                                    response.Value = data_IT.Lock_Keyboard_RestoreFactoryDefaults ? "Lock" : "Unlock";
                                    break;
                                case "MOUSE":
                                    response.Value = data_IT.Lock_Mouse_RestoreFactoryDefaults ? "Lock" : "Unlock";
                                    break;
                                case "AUDIO":
                                    response.Value = data_IT.Lock_Audio_RestoreFactoryDefaults ? "Lock" : "Unlock";
                                    break;
                            }
                            break;
                        case "INAPPRESTOREDEFAULTS":
                            response.Value = data_IT.Lock_Setting_RestoreDefaults ? "Lock" : "Unlock";
                            break;
                        case "INAPPBRICONT":
                            response.Value = data_IT.Lock_Display_BriCont ? "Lock" : "Unlock";
                            break;
                        //case "INAPPAUTOBRITEMP":
                        case "INAPPAUTOBRIGHTNESSCOLOR"://1004 InAppAutoBrightnessColor DDPMW1341
                            response.Value = data_IT.Lock_Display_AutoBriTemp ? "Lock" : "Unlock";
                            break;
                        case "INAPPNETWORKKVM":
                            response.Value = data_IT.Lock_Display_NetworkKVM ? "Enable" : "Disable";
                            break;
                        case "INAPPCOLORPRESET":
                            response.Value = data_IT.Lock_Display_ColorPreset ? "Lock" : "Unlock";
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
            if (string.IsNullOrEmpty(op.Option_Name) || 
                !op.Option_Name.ToUpper().Equals("VALUE"))
            {
                WriteLog(Log, $"{commandLineInput.TargetFeature}: option name [{op.Option_Name}] not support");
                return CLI_Response_OptionNameNotSupport(commandLineInput, result, op);
            }
            string value = op.Option_Value;
            if (value.ToUpper().Equals("LOCK") || value.ToUpper().Equals("ON") )//value.ToUpper().Equals("ENABLE"))
            {
                if (data_IT != null)
                {
                    switch (commandLineInput.TargetFeature)
                    {
                        case "INAPPUPDATE":
                            data_IT.Lock_Settings_Updates = true;
                            break;
                        case "INAPPEXPORTIMPORT":
                            data_IT.Lock_Display_ExportSettings = true;
                            break;
                        case "RESTOREFACTORYDEFAULTS":
                            switch (commandLineInput.PluginsType.ToUpper())
                            {
                                case "DISPLAY":
                                    data_IT.Lock_Display_RestoreFactoryDefaults = true;
                                    break;
                                case "PEN":
                                    data_IT.Lock_Pen_RestoreFactoryDefaults = true;
                                    break;
                                case "WEBCAM":
                                    data_IT.Lock_Webcam_RestoreFactoryDefaults = true;
                                    break;
                                case "KEYBOARD":
                                    data_IT.Lock_Keyboard_RestoreFactoryDefaults = true;
                                    break;
                                case "MOUSE":
                                    data_IT.Lock_Mouse_RestoreFactoryDefaults = true;
                                    break;
                                case "AUDIO":
                                    data_IT.Lock_Audio_RestoreFactoryDefaults = true;
                                    break;
                            }
                            break;
                        case "INAPPRESTOREDEFAULTS":
                            data_IT.Lock_Setting_RestoreDefaults = true;
                            break;
                        case "INAPPBRICONT":
                            data_IT.Lock_Display_BriCont = true;
                            break;
                        //case "INAPPAUTOBRITEMP":
                        case "INAPPAUTOBRIGHTNESSCOLOR"://1004 InAppAutoBrightnessColor DDPMW1341
                            data_IT.Lock_Display_AutoBriTemp = true;
                            break;
                        case "INAPPNETWORKKVM":
                            data_IT.Lock_Display_NetworkKVM = true;
                            break;
                        case "INAPPCOLORPRESET":
                            data_IT.Lock_Display_ColorPreset = true;
                            break;
                            //case "POWERNAP":
                            //data_IT.Lock_Display_PowerNap = true;
                            //break;
                    }
                }
                if (data_user != null)
                {
                    switch (commandLineInput.TargetFeature)
                    {
                        case "INAPPUPDATE":
                            data_user.LockSettings.Lock_Settings_Updates = true;
                            break;
                        case "INAPPEXPORTIMPORT":
                            data_user.LockSettings.Lock_Display_ExportSettings = true;
                            break;
                        case "RESTOREFACTORYDEFAULTS":
                            switch (commandLineInput.PluginsType.ToUpper())
                            {
                                case "DISPLAY":
                                    data_user.LockSettings.Lock_Display_RestoreFactoryDefaults = true;
                                    break;
                                case "PEN":
                                    data_user.LockSettings.Lock_Pen_RestoreFactoryDefaults = true;
                                    break;
                                case "WEBCAM":
                                    data_user.LockSettings.Lock_Webcam_RestoreFactoryDefaults = true;
                                    break;
                                case "KEYBOARD":
                                    data_user.LockSettings.Lock_Keyboard_RestoreFactoryDefaults = true;
                                    break;
                                case "MOUSE":
                                    data_user.LockSettings.Lock_Mouse_RestoreFactoryDefaults = true;
                                    break;
                                case "AUDIO":
                                    data_user.LockSettings.Lock_Audio_RestoreFactoryDefaults = true;
                                    break;
                            }
                            break;
                        case "INAPPRESTOREDEFAULTS":
                            data_user.LockSettings.Lock_Setting_RestoreDefaults = true;
                            break;
                        case "INAPPBRICONT":
                            data_user.LockSettings.Lock_Display_BriCont = true;
                            break;
                        //case "INAPPAUTOBRITEMP":
                        case "INAPPAUTOBRIGHTNESSCOLOR"://1004 InAppAutoBrightnessColor DDPMW1341
                            data_user.LockSettings.Lock_Display_AutoBriTemp = true;
                            break;
                        case "INAPPNETWORKKVM":
                            data_user.LockSettings.Lock_Display_NetworkKVM = true;
                            break;
                        case "INAPPCOLORPRESET":
                            data_user.LockSettings.Lock_Display_ColorPreset = true;
                            break;
                            //case "POWERNAP":
                            //data_user.LockSettings.Lock_Display_PowerNap = true;
                            //break;
                    }
                }
            }
            else if (value.ToUpper().Equals("UNLOCK") || value.ToUpper().Equals("OFF"))//value.ToUpper().Equals("DISABLE")
            {
                if (data_IT != null)
                {
                    switch (commandLineInput.TargetFeature)
                    {
                        case "INAPPUPDATE":
                            data_IT.Lock_Settings_Updates = false;
                            break;
                        case "INAPPEXPORTIMPORT":
                            data_IT.Lock_Display_ExportSettings = false;
                            break;
                        case "RESTOREFACTORYDEFAULTS":
                            switch (commandLineInput.PluginsType.ToUpper())
                            {
                                case "DISPLAY":
                                    data_IT.Lock_Display_RestoreFactoryDefaults = false;
                                    break;
                                case "PEN":
                                    data_IT.Lock_Pen_RestoreFactoryDefaults = false;
                                    break;
                                case "WEBCAM":
                                    data_IT.Lock_Webcam_RestoreFactoryDefaults = false;
                                    break;
                                case "KEYBOARD":
                                    data_IT.Lock_Keyboard_RestoreFactoryDefaults = false;
                                    break;
                                case "MOUSE":
                                    data_IT.Lock_Mouse_RestoreFactoryDefaults = false;
                                    break;
                                case "AUDIO":
                                    data_IT.Lock_Audio_RestoreFactoryDefaults = false;
                                    break;
                            }
                            break;
                        case "INAPPRESTOREDEFAULTS":
                            data_IT.Lock_Setting_RestoreDefaults = false;
                            break;
                        case "INAPPBRICONT":
                            data_IT.Lock_Display_BriCont = false;
                            break;
                        //case "INAPPAUTOBRITEMP":
                        case "INAPPAUTOBRIGHTNESSCOLOR"://1004 InAppAutoBrightnessColor DDPMW1341
                            data_IT.Lock_Display_AutoBriTemp = false;
                            break;
                        case "INAPPNETWORKKVM":
                            data_IT.Lock_Display_NetworkKVM = false;
                            break;
                        case "INAPPCOLORPRESET":
                            data_IT.Lock_Display_ColorPreset = false;
                            break;
                            //case "POWERNAP":
                            //data_IT.Lock_Display_PowerNap = false;
                            //break;
                    }
                }
                if (data_user != null)
                {
                    switch (commandLineInput.TargetFeature)
                    {
                        case "INAPPUPDATE":
                            data_user.LockSettings.Lock_Settings_Updates = false;
                            break;
                        case "INAPPEXPORTIMPORT":
                            data_user.LockSettings.Lock_Display_ExportSettings = false;
                            break;
                        case "RESTOREFACTORYDEFAULTS":
                            switch (commandLineInput.PluginsType.ToUpper())
                            {
                                case "DISPLAY":
                                    data_user.LockSettings.Lock_Display_RestoreFactoryDefaults = false;
                                    break;
                                case "PEN":
                                    data_user.LockSettings.Lock_Pen_RestoreFactoryDefaults = false;
                                    break;
                                case "WEBCAM":
                                    data_user.LockSettings.Lock_Webcam_RestoreFactoryDefaults = false;
                                    break;
                                case "KEYBOARD":
                                    data_user.LockSettings.Lock_Keyboard_RestoreFactoryDefaults = false;
                                    break;
                                case "MOUSE":
                                    data_user.LockSettings.Lock_Mouse_RestoreFactoryDefaults = false;
                                    break;
                                case "AUDIO":
                                    data_user.LockSettings.Lock_Audio_RestoreFactoryDefaults = false;
                                    break;
                            }
                            break;
                        case "INAPPRESTOREDEFAULTS":
                            data_user.LockSettings.Lock_Setting_RestoreDefaults = false;
                            break;
                        case "INAPPBRICONT":
                            data_user.LockSettings.Lock_Display_BriCont = false;
                            break;
                        //case "INAPPAUTOBRITEMP":
                        case "INAPPAUTOBRIGHTNESSCOLOR"://1004 InAppAutoBrightnessColor DDPMW1341
                            data_user.LockSettings.Lock_Display_AutoBriTemp = false;
                            break;
                        case "INAPPNETWORKKVM":
                            data_user.LockSettings.Lock_Display_NetworkKVM = false;
                            break;
                        case "INAPPCOLORPRESET":
                            data_user.LockSettings.Lock_Display_ColorPreset = false;
                            break;
                            //case "POWERNAP":
                            //data_user.LockSettings.Lock_Display_PowerNap = false;
                            //break;
                    }
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
                        case "INAPPEXPORTIMPORT":
                            status = _SettingsPluginIT.WriteITConfigData(data_IT, new List<string>() { $"Lock_Display_ExportSettings" }).Result;
                            break;
                        case "RESTOREFACTORYDEFAULTS":
                            switch (commandLineInput.PluginsType.ToUpper())
                            {
                                case "DISPLAY":
                                    status = _SettingsPluginIT.WriteITConfigData(data_IT, new List<string>() { $"Lock_Display_RestoreFactoryDefaults" }).Result;
                                    break;
                                case "PEN":
                                    status = _SettingsPluginIT.WriteITConfigData(data_IT, new List<string>() { $"Lock_Pen_RestoreFactoryDefaults" }).Result;
                                    break;
                                case "WEBCAM":
                                    status = _SettingsPluginIT.WriteITConfigData(data_IT, new List<string>() { $"Lock_Webcam_RestoreFactoryDefaults" }).Result;
                                    break;
                                case "KEYBOARD":
                                    status = _SettingsPluginIT.WriteITConfigData(data_IT, new List<string>() { $"Lock_Keyboard_RestoreFactoryDefaults" }).Result;
                                    break;
                                case "MOUSE":
                                    status = _SettingsPluginIT.WriteITConfigData(data_IT, new List<string>() { $"Lock_Mouse_RestoreFactoryDefaults" }).Result;
                                    break;
                                case "AUDIO":
                                    status = _SettingsPluginIT.WriteITConfigData(data_IT, new List<string>() { $"Lock_Audio_RestoreFactoryDefaults" }).Result;
                                    break;
                            }
                            break;
                        case "INAPPRESTOREDEFAULTS":
                            status = _SettingsPluginIT.WriteITConfigData(data_IT, new List<string>() { $"Lock_Setting_RestoreDefaults" }).Result;
                            break;
                        case "INAPPBRICONT":
                            status = _SettingsPluginIT.WriteITConfigData(data_IT, new List<string>() { $"Lock_Display_BriCont" }).Result;
                            break;
                        //case "INAPPAUTOBRITEMP":
                        case "INAPPAUTOBRIGHTNESSCOLOR"://1004 InAppAutoBrightnessColor DDPMW1341
                            status = _SettingsPluginIT.WriteITConfigData(data_IT, new List<string>() { $"Lock_Display_AutoBriTemp" }).Result;
                            break;
                        case "INAPPNETWORKKVM":
                            status = _SettingsPluginIT.WriteITConfigData(data_IT, new List<string>() { $"Lock_Display_NetworkKVM" }).Result;
                            break;
                        case "INAPPCOLORPRESET":
                            status = _SettingsPluginIT.WriteITConfigData(data_IT, new List<string>() { $"Lock_Display_ColorPreset" }).Result;
                            break;
                            //case "POWERNAP":
                            //status = _SettingsPluginIT.WriteITConfigData(data_IT, new List<string>() { $"Lock_Display_PowerNap" }).Result;
                            //break;
                    }
                }
                Debug.WriteLine($"{status}");
            }
            if (_DeviceManagerPlugin != null && data_user != null)
            {
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

        public static CLIEventResult CLI_Common_LockUlockWithUserAction(ILog Log, object inputData, object settingsPlugin, CommandLineInput commandLineInput, string action_guid)
        {
            //Example:
            // Expected format:
            //  IT    > /set -app=TelemetryConsent -value=true,lock / true,unlock / false,lock / false,unlock
            //  Normal> /set -app=TelemetryConsent -value=true / false
            //  Normal> /get -app=TelemetryConsent
            CLIEventResult result = new CLIEventResult();
            result.ticket = DateTime.Now;
            result.command_guid_string = action_guid;

            APP_RESPONSE response = new APP_RESPONSE();
            response.Command = commandLineInput.Command;
            response.TargetFeature = commandLineInput.TargetFeature;

            if (commandLineInput.Command.Equals("SET"))//for SET command, option should not be empty.
            {
                if (commandLineInput.Options == null || commandLineInput.Options.Count == 0)
                {
                    WriteLog(Log, $"{commandLineInput.TargetFeature}: command SET without option, fail");
                    return CLI_Response_OptionMissing(commandLineInput, result);
                }
                if (commandLineInput.Options.Count > 1)
                {
                    WriteLog(Log, $"{commandLineInput.TargetFeature}: doesn't support multiple value options");
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
            
            GlobalSettingParam param = null;
            if (_DeviceManagerPlugin != null)
            {
                param = _DeviceManagerPlugin.GetGlobalSettingParam().Result;
            }
            //GET is for user mode using
            if (commandLineInput.Command.Equals("GET")) //ex: cli.exe /get -app=TelemetryConsent
            {
                if (data_user == null && data_IT == null)
                {
                    response.Message = "Fail to read application setting";
                    result.serialize_Json_response = JsonConvert.SerializeObject(response, Formatting.Indented);
                    result.ExitCode = (int)CLI_ExitCode.fail_read_settings;
                    return result;
                }
                if (data_user != null && (data_user.UserSettings == null || data_user.LockSettings == null))
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

                switch (commandLineInput.TargetFeature)
                {
                    case "TELEMETRYCONSENT":
                        DDPMITConfig tmp;                        
                        if (data_user != null)
                        {
                            Console.WriteLine($"{commandLineInput.TargetFeature}: is function enable? => {param.isTelemetryConsentOn}");
                            response.Value = (param.isTelemetryConsentOn ? "true," : "false,") + (data_user.LockSettings.Lock_Settings_TelemetryConsent ? "Lock" : "Unlock");
                        }
                        else//IT
                        {
                            Console.WriteLine($"{commandLineInput.TargetFeature}: is Locked? => = {data_IT.Lock_Settings_TelemetryConsent}");
                            response.Value = (data_IT.Lock_Settings_TelemetryConsent ? "Lock" : "Unlock");
                        }
                        break;
                    case "POWERNAP": //assume only IT command enter here
                        Console.WriteLine($"{commandLineInput.TargetFeature}: is Locked? => = {data_IT.Lock_Display_PowerNap}");
                        response.Value = (data_IT.Lock_Display_PowerNap ? "Lock" : "Unlock");
                        break;
                    case "RESOLUTIONREFRESHRATE": //assume only IT command enter here
                        Console.WriteLine($"{commandLineInput.TargetFeature}: is Locked? => = {data_IT.Lock_Display_ResolutionRefreshRate}");
                        response.Value = (data_IT.Lock_Display_ResolutionRefreshRate ? "Lock" : "Unlock");
                        break;
                    case "USBCPRIORITIZATION": //assume only IT command enter here
                        Console.WriteLine($"{commandLineInput.TargetFeature}: is Locked? => = {data_IT.Lock_Display_USBCPrioritization}");
                        response.Value = (data_IT.Lock_Display_USBCPrioritization ? "Lock" : "Unlock"); 
                        break;
                    case "ACTIVEINPUTSOURCE": //assume only IT command enter here
                        Console.WriteLine($"{commandLineInput.TargetFeature}: is Locked? => = {data_IT.Lock_Display_ActiveInputSource}");
                        response.Value = (data_IT.Lock_Display_ActiveInputSource ? "Lock" : "Unlock");
                        break;
                    case "COLLABSCREENSHARE": //assume only IT command enter here
                        Console.WriteLine($"{commandLineInput.TargetFeature}: is Locked? => = {data_IT.Lock_Keyboard_CollabScreenShare}");
                        response.Value = (data_IT.Lock_Keyboard_CollabScreenShare ? "Lock" : "Unlock");
                        break;
                    case "INAPPUSBKVM": //assume only IT command enter here
                        Console.WriteLine($"{commandLineInput.TargetFeature}: is Locked? => = {data_IT.Lock_Display_USBKVM}");
                        response.Value = (data_IT.Lock_Display_USBKVM ? "Lock" : "Unlock");
                        break;
                    default:
                        return CLI_Response_TypeNotSupport(commandLineInput, result);
                }
                response.Result = "Pass";
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

                if ( string.IsNullOrEmpty(op.Option_Name) ||
                     !op.Option_Name.ToUpper().Equals("VALUE"))
                {
                    WriteLog(Log, $"{commandLineInput.TargetFeature}: option name [{op.Option_Name}] not support");
                    return CLI_Response_OptionNameNotSupport(commandLineInput, result, op);
                }
 

                //op.Option_Value.Replace(".", ",");
                List<string> values = op.Option_Value.Replace(".", ",").Split(",").ToList();
                List<string> inputSourceList = new List<string>()
                {
                    "HDMI",
                    "DP",
                    "DISPLAYPORT",
                    "USBC",
                    "USB-C",
                    "TBT",
                    "THUNDERBOLT",
                };

                foreach (string value in values)
                {
                    if (value.ToUpper().Equals("TRUE"))
                    {
                        if (commandLineInput.TargetFeature.Equals("TELEMETRYCONSENT"))
                        {
                            if (data_user != null)
                            {
                                param.isTelemetryConsentOn = true;
                                data_user.LockSettings.Lock_Settings_TelemetryConsent = true;
                            }
                            if (data_IT != null)
                                data_IT.Lock_Settings_TelemetryConsent = true;
                        }
                        else
                            return CLI_Response_TypeNotSupport(commandLineInput, result);
                    }
                    else if (value.ToUpper().Equals("FALSE"))
                    {
                        if (commandLineInput.TargetFeature.Equals("TELEMETRYCONSENT"))
                        {
                            if (data_user != null)
                            {
                                param.isTelemetryConsentOn = false;
                                data_user.LockSettings.Lock_Settings_TelemetryConsent = true;
                            }
                            if (data_IT != null)
                                data_IT.Lock_Settings_TelemetryConsent = true;
                        }
                        else
                            return CLI_Response_TypeNotSupport(commandLineInput, result);
                    }
                    else if (value.ToUpper().Equals("LOCK") || value.ToUpper().Equals("UNLOCK"))
                    {
                        bool target = false;
                        if (value.ToUpper().Equals("LOCK"))
                            target = true;
                        if (value.ToUpper().Equals("UNLOCK"))
                            target = false;

                        if (commandLineInput.TargetFeature.Equals("TELEMETRYCONSENT"))
                        {
                            if (data_IT != null)
                                data_IT.Lock_Settings_TelemetryConsent = target;
                            if (data_user != null)
                                data_user.LockSettings.Lock_Settings_TelemetryConsent = target;
                        }
                        else if (commandLineInput.TargetFeature.Equals("POWERNAP"))
                        {
                            if (data_IT != null)
                                data_IT.Lock_Display_PowerNap = target;
                            if (data_user != null)
                                data_user.LockSettings.Lock_Display_PowerNap = target;
                        }
                        else if (commandLineInput.TargetFeature.Equals("RESOLUTIONREFRESHRATE"))
                        {
                            if (data_IT != null)
                                data_IT.Lock_Display_ResolutionRefreshRate = target;
                            if (data_user != null)
                                data_user.LockSettings.Lock_Display_ResolutionRefreshRate = target;
                        }
                        else if (commandLineInput.TargetFeature.Equals("USBCPRIORITIZATION"))
                        {
                            if (data_IT != null)
                                data_IT.Lock_Display_USBCPrioritization = target;
                            if (data_user != null)
                                data_user.LockSettings.Lock_Display_USBCPrioritization = target;
                        }
                        else if (commandLineInput.TargetFeature.Equals("ACTIVEINPUTSOURCE"))
                        {
                            if (data_IT != null)
                                data_IT.Lock_Display_ActiveInputSource = target;
                            if (data_user != null)
                                data_user.LockSettings.Lock_Display_ActiveInputSource = target;
                        }
                        else if (commandLineInput.TargetFeature.Equals("COLLABSCREENSHARE"))
                        {
                            if (data_IT != null)
                                data_IT.Lock_Keyboard_CollabScreenShare = target;
                            if (data_user != null)
                                data_user.LockSettings.Lock_Keyboard_CollabScreenShare = target;
                        }
                        else if (commandLineInput.TargetFeature.Equals("MICNOISECANCELLATION"))
                        {
                            if (data_IT != null)
                                data_IT.Lock_Audio_micNoiseCancellation = target;
                            if (data_user != null)
                                data_user.LockSettings.Lock_Audio_micNoiseCancellation = target;
                        }
                        else if (commandLineInput.TargetFeature.Equals("SCREENNOTIFICATION"))
                        {
                            if (data_IT != null)
                                data_IT.Lock_Setting_ScreenNotification = target;
                            if (data_user != null)
                                data_user.LockSettings.Lock_Setting_ScreenNotification = target;
                        }
                        else if (commandLineInput.TargetFeature.Equals("HDR"))
                        {
                            if (data_IT != null)
                                data_IT.Lock_Webcam_hdr = target;
                            if (data_user != null)
                                data_user.LockSettings.Lock_Webcam_hdr = target;
                        }
                        else if (commandLineInput.TargetFeature.Equals("ANTIFLICKER"))
                        {
                            if (data_IT != null)
                                data_IT.Lock_Webcam_AntiFlicker = target;
                            if (data_user != null)
                                data_user.LockSettings.Lock_Webcam_AntiFlicker = target;
                        }
                        else if (commandLineInput.TargetFeature.Equals("AIAUTOFRAMING"))
                        {
                            if (data_IT != null)
                                data_IT.Lock_Webcam_AIAutoFraming = target;
                            if (data_user != null)
                                data_user.LockSettings.Lock_Webcam_AIAutoFraming = target;
                        }
                        else if (commandLineInput.TargetFeature.Equals("MICSWITCH"))
                        {
                            if (data_IT != null)
                                data_IT.Lock_Webcam_MicSwitch = target;
                            if (data_user != null)
                                data_user.LockSettings.Lock_Webcam_MicSwitch = target;
                        }
                        else
                            return CLI_Response_TypeNotSupport(commandLineInput, result);
                    }
                    else if (value.ToUpper().Equals("ENABLE") || value.ToUpper().Equals("DISABLE"))
                    {
                        bool target = false;
                        if (value.ToUpper().Equals("DISABLE"))
                            target = true;
                        if (value.ToUpper().Equals("ENABLE"))
                            target = false;
                        if (commandLineInput.TargetFeature.Equals("INAPPUSBKVM"))
                        {
                            if (data_IT != null)
                                data_IT.Lock_Display_USBKVM = target;
                            if (data_user != null)
                                data_user.LockSettings.Lock_Display_USBKVM = target;
                            continue;
                        }
                        else if (commandLineInput.TargetFeature.Equals("EASYARRANGELAYOUT"))
                        {
                            if (data_IT != null)
                                data_IT.Lock_Display_EasyArrangeLayout = target;
                            if (data_user != null)
                                data_user.LockSettings.Lock_Display_EasyArrangeLayout = target;
                            continue;
                        }
                        else if (commandLineInput.TargetFeature.Equals("ANCMODE"))
                        {
                            if (data_IT != null)
                                data_IT.Lock_Audio_ancMode = target;
                            if (data_user != null)
                                data_user.LockSettings.Lock_Audio_ancMode = target;
                            continue;
                        }
                        else if (commandLineInput.TargetFeature.Equals("WEARDETECTION"))
                        {
                            if (data_IT != null)
                                data_IT.Lock_Audio_wearDetection = target;
                            if (data_user != null)
                                data_user.LockSettings.Lock_Audio_wearDetection = target;
                            continue;
                        }
                        else if (commandLineInput.TargetFeature.Equals("PRESENCEDETECTION"))
                        {
                            if (data_IT != null)
                                data_IT.Lock_Webcam_PresenceDetection = target;
                            if (data_user != null)
                                data_user.LockSettings.Lock_Webcam_PresenceDetection = target;
                            continue;
                        }
                    }
                    else
                    {
                        //Judgement the user action here, if the value is supported then bypass this loop. (the data store should be processed at user proxy plugin)
                        if (commandLineInput.TargetFeature.Equals("POWERNAP"))
                        {
                            if (value.ToUpper().Equals("OFF") || value.ToUpper().Equals("SLEEP") || value.ToUpper().Equals("REDUCEBRIGHTNESS"))
                                continue;
                        }
                        else if (commandLineInput.TargetFeature.Equals("RESOLUTIONREFRESHRATE"))
                        {
                            if (value.ToUpper().Contains("X") && value.ToUpper().Contains("@"))
                                continue;
                        }
                        else if (commandLineInput.TargetFeature.Equals("USBCPRIORITIZATION"))
                        {
                            if (value.ToUpper().Equals("HIGHSPEED") || value.ToUpper().Equals("HIGHRESOLUTION") || value.ToUpper().Equals("HIGHDATASPEED"))
                                continue;
                        }
                        else if (commandLineInput.TargetFeature.Equals("ACTIVEINPUTSOURCE"))
                        {
                            if (inputSourceList.FindIndex(x => value.ToUpper().Trim().Contains(x.ToUpper().Trim())) >= 0)
                            {
                                WriteLog(Log, $"[IT]Feature:{commandLineInput.TargetFeature} get the value [{value}] indeed in support list");
                                continue;
                            }
                        }
                        else if (commandLineInput.TargetFeature.Equals("COLLABSCREENSHARE"))
                        {
                            if (value.ToUpper().Equals("ON") || value.ToUpper().Equals("OFF"))
                                continue;
                        }
                        else if (commandLineInput.TargetFeature.Equals("MICNOISECANCELLATION"))
                        {
                            if (value.ToUpper().Equals("ON") || value.ToUpper().Equals("OFF"))
                                continue;
                        }
                        else if (commandLineInput.TargetFeature.Equals("SCREENNOTIFICATION"))
                        {
                            if (value.ToUpper().Equals("ON") || value.ToUpper().Equals("OFF"))
                                continue;
                        }
                        else if (commandLineInput.TargetFeature.Equals("HDR"))
                        {
                            if (value.ToUpper().Equals("ON") || value.ToUpper().Equals("OFF"))
                                continue;
                        }
                        else if (commandLineInput.TargetFeature.Equals("ANTIFLICKER"))
                        {
                            if (value.ToString().Equals("50") || value.ToString().Equals("60"))
                                continue;
                        }
                        else if (commandLineInput.TargetFeature.Equals("AIAUTOFRAMING"))
                        {
                            if (value.ToUpper().Equals("ON") || value.ToUpper().Equals("OFF"))
                                continue;
                        }
                        else if (commandLineInput.TargetFeature.Equals("MICSWITCH"))
                        {
                            if (value.ToUpper().Equals("ON") || value.ToUpper().Equals("OFF"))
                                continue;
                        }
                        //No pre-definition be found, means fail
                        response.Message = $"{commandLineInput.TargetFeature}: value format error with [{value}]";
                        Debug.WriteLine(response.Message);
                        Console.WriteLine(response.Message);
                        WriteLog(Log, response.Message);
                        return CLI_Response_OptionValueNotSupport(commandLineInput, result, op);
                    }
                    // continue; // 20240827 Refactor the containing loop
                }
                bool status = false;
                if (_SettingsPluginIT != null && data_IT != null)
                {
                    switch (commandLineInput.TargetFeature)
                    {
                        case "TELEMETRYCONSENT":
                            status = _SettingsPluginIT.WriteITConfigData(data_IT, new List<string>() { "Lock_Settings_TelemetryConsent" }).Result;
                            break;
                        case "POWERNAP":
                            status = _SettingsPluginIT.WriteITConfigData(data_IT, new List<string>() { "Lock_Display_PowerNap" }).Result;
                            break;
                        case "RESOLUTIONREFRESHRATE":
                            status = _SettingsPluginIT.WriteITConfigData(data_IT, new List<string>() { "Lock_Display_ResolutionRefreshRate" }).Result;
                            break;
                        case "USBCPRIORITIZATION":
                            status = _SettingsPluginIT.WriteITConfigData(data_IT, new List<string>() { "Lock_Display_USBCPrioritization" }).Result;
                            break;
                        case "ACTIVEINPUTSOURCE":
                            status = _SettingsPluginIT.WriteITConfigData(data_IT, new List<string>() { "Lock_Display_ActiveInputSource" }).Result;
                            break;
                        case "COLLABSCREENSHARE":
                            status = _SettingsPluginIT.WriteITConfigData(data_IT, new List<string>() { "Lock_Keyboard_CollabScreenShare" }).Result;
                            break;
                        case "INAPPUSBKVM":
                            status = _SettingsPluginIT.WriteITConfigData(data_IT, new List<string>() { "Lock_Display_USBKVM" }).Result;
                            break;
                        case "EASYARRANGELAYOUT":
                            status = _SettingsPluginIT.WriteITConfigData(data_IT, new List<string>() { "Lock_Display_EasyArrangeLayout" }).Result;
                            break;
                        case "ANCMODE":
                            status = _SettingsPluginIT.WriteITConfigData(data_IT, new List<string>() { "Lock_Audio_ancMode" }).Result;
                            break;
                        case "MICNOISECANCELLATION":
                            status = _SettingsPluginIT.WriteITConfigData(data_IT, new List<string>() { "Lock_Audio_micNoiseCancellation" }).Result;
                            break;
                        case "WEARDETECTION":
                            status = _SettingsPluginIT.WriteITConfigData(data_IT, new List<string>() { "Lock_Audio_wearDetection" }).Result;
                            break;
                        case "SCREENNOTIFICATION":
                            status = _SettingsPluginIT.WriteITConfigData(data_IT, new List<string>() { "Lock_Setting_ScreenNotification" }).Result;
                            break;
                        case "HDR":
                            status = _SettingsPluginIT.WriteITConfigData(data_IT, new List<string>() { "Lock_Webcam_hdr" }).Result;
                            break;
                        case "ANTIFLICKER":
                            status = _SettingsPluginIT.WriteITConfigData(data_IT, new List<string>() { "Lock_Webcam_AntiFlicker" }).Result;
                            break;
                        case "AIAUTOFRAMING":
                            status = _SettingsPluginIT.WriteITConfigData(data_IT, new List<string>() { "Lock_Webcam_AIAutoFraming" }).Result;
                            break;
                        case "MICSWITCH":
                            status = _SettingsPluginIT.WriteITConfigData(data_IT, new List<string>() { "Lock_Webcam_MicSwitch" }).Result;
                            break;
                        case "PRESENCEDETECTION":
                            status = _SettingsPluginIT.WriteITConfigData(data_IT, new List<string>() { "Lock_Webcam_PresenceDetection" }).Result;
                            break;
                    }
                    
                }
                if (_DeviceManagerPlugin != null)
                {
                    if (data_user != null)
                        status = _DeviceManagerPlugin.SetAppConfigData(data_user).Result;
                    if(param != null && commandLineInput.TargetFeature.Equals("TELEMETRYCONSENT"))
                    {
                        _DeviceManagerPlugin.Set_GlobalSetting_EnableTelemetryConsent(param.isTelemetryConsentOn);
                    }
                }

                if (status)
                {
                    response.Result = "Pass";
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

        public static CLIEventResult CLI_Common_DisableCA(ILog Log, object settingsPlugin, CommandLineInput commandLineInput, string action_guid)
        {
            //Expected format:
            // IT > /set -app=DisableCA -value=1 / 0
            CLIEventResult result = new CLIEventResult();
            result.ticket = DateTime.Now;
            result.command_guid_string = action_guid;

            CLI_RESPONSE response = new CLI_RESPONSE();
            response.Command = commandLineInput.Command;
            response.TargetFeature = commandLineInput.TargetFeature;


            Type type2 = settingsPlugin.GetType();
            WriteLog(Log, $"Output interface log: [{settingsPlugin.GetType()}],[{settingsPlugin.GetType().Name}]");
            IDeviceManagerSA _DeviceManagerPlugin = type2.Name == "DeviceMangerPlugin" ? (IDeviceManagerSA)settingsPlugin : null;

            if (commandLineInput.Options == null || commandLineInput.Options.Count == 0)
            {
                WriteLog(Log, "Disable CA: command SET without option, fail");
                return CLI_Response_OptionMissing(commandLineInput, result);
            }

            CommandType_Option op = commandLineInput.Options[0];
            if (string.IsNullOrEmpty(op.Option_Name) || 
                !op.Option_Name.ToUpper().Equals("VALUE"))
            {
                WriteLog(Log, $"Disable CA: option name [{op.Option_Name}] not support");
                return CLI_Response_OptionNameNotSupport(commandLineInput, result, op);
            }
            string value = op.Option_Value;
            bool isSkipCA = false;
            if (value.ToUpper().Equals("0"))
            {
                isSkipCA = false;
            }
            else if (value.ToUpper().Equals("1"))
            {
                isSkipCA = true;
            }
            else
            {
                WriteLog(Log, $"Disable CA: option value [{value}] not support");
                return CLI_Response_OptionValueNotSupport(commandLineInput, result, op);
            }

            bool status = false;
            if (_DeviceManagerPlugin != null)
            {
                status = _DeviceManagerPlugin.SetSkipCA(isSkipCA).Result;
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



        // add @ stephen
        public static CLIEventResult CLI_FW_Update(ILog Log, object inputData, object settingsPlugin, CommandLineInput commandLineInput, string action_guid)
        {
            //Expected format:
            // IT > /configure -app=InAppUpdate -value=lock / unlock
            // IT > /get -app=InAppUpdate

            Console.WriteLine($"@@Stephen CLI_FW_Update called ");
            CLIEventResult result = new CLIEventResult();
            result.ticket = DateTime.Now;
            result.command_guid_string = action_guid;

            CLI_RESPONSE response = new CLI_RESPONSE();
            response.Command = commandLineInput.Command;
            response.TargetFeature = commandLineInput.TargetFeature;

            if (!commandLineInput.Command.Equals("SET"))
            {
                WriteLog(Log, $"CLI_FW_Update: the command should be configure or get, fail");
                return CLI_Response_CommandNotSupport(commandLineInput, result);
            }

            Type type = inputData.GetType();
            Type type2 = settingsPlugin.GetType();
            DDPMSettings data_user = type == typeof(DDPMSettings) ? (DDPMSettings)inputData : null;
            DDPMITConfig data_IT = type == typeof(DDPMITConfig) ? (DDPMITConfig)inputData : null;
            WriteLog(Log, $"Output interface log: [{settingsPlugin.GetType()}],[{settingsPlugin.GetType().Name}]");
            ISettingsManagerIT _SettingsPluginIT = type2.Name == "SettingsMangerPlugin" ? (ISettingsManagerIT)settingsPlugin : null;
            IDeviceManagerSA _DeviceManagerPlugin = type2.Name == "DeviceMangerPlugin" ? (IDeviceManagerSA)settingsPlugin : null;

            return CLI_Response_CompleteWithSuccess(commandLineInput, result);
        }

        private static void ApplyValueToBoolObject(ref bool config, bool value)
        {
            if (config != null && value != null)
            {
                config = value;
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