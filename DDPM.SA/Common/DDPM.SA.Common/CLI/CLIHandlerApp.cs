using DDPM.SA.Common.Settings;
using Dell.Client.Framework.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using static DDPM.SA.Common.ICLICommandTable;

namespace DDPM.SA.Common.CLI
{
    public class CLIHandlerApp
    {
        public static CLIEventResult CLI_Analytics_Consent(ILog Log, object inputData, object settingsPlugin, CommandLineInput commandLineInput, string action_guid)
        {
            //Expected format:
            // IT    > set -app=TelemetryConsent -value=on,lock / on,unlock / off,lock / off,unlock
            // Normal> set -app=TelemetryConsent -value=on / off
            // Normal> get -app=TelemetryConsent

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
                    response.Message = "Option is missing";
                    result.serialize_Json_response = JsonConvert.SerializeObject(response, Formatting.Indented);
                    result.ExitCode = (int)CLI_ExitCode.fail_no_analytics_options;
                    return result;
                }

                if (commandLineInput.Options.Count > 1)
                {
                    WriteLog(Log, "Telemetry Consent: doesn't support multiple value options");
                    response.Result = "Fail";
                    response.Message = "Telemetry Consent: doesn't support multiple value options";
                    result.ExitCode = (int)CLI_ExitCode.fail_analytics_option_notsupport;
                    result.serialize_Json_response = JsonConvert.SerializeObject(response, Formatting.Indented);
                    return result;
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
                Console.WriteLine($"Telemetry Consent: is Locked? => = {data_user.LockSettings.Lock_TelemetryConsent}");
                response.Value = (data_user.UserSettings.isTelemetryConsentOn ? "On," : "Off,") + (data_user.LockSettings.Lock_TelemetryConsent ? "Lock" : "Unlock");
                response.Result = "Completed";
                result.serialize_Json_response = JsonConvert.SerializeObject(response, Formatting.Indented);
                result.ExitCode = (int)CLI_ExitCode.success;

                return result;
            }
            //ex: cli.exe /set -app=TelemetryConsent -value=on / off <= for user
            //                                              lock / unlock <= for IT
            //                                              on,lock / on,unlock / off,lock / off,unlock <= for IT+user
            else if (commandLineInput.Command.Equals("SET"))
            {
                CommandType_Option op = commandLineInput.Options[0];

                if (!op.Option_Name.ToUpper().Equals("VALUE"))
                {
                    WriteLog(Log, $"Telemetry Consent: option name [{op.Option_Name}] not support");
                    response.Result = "Fail";
                    response.Value = op.Option_Value;
                    response.Message = $"Telemetry Consent: option name [{op.Option_Name}] not support";
                    result.ExitCode = (int)CLI_ExitCode.fail_analytics_option_notsupport;
                    result.serialize_Json_response = JsonConvert.SerializeObject(response, Formatting.Indented);
                    return result;
                }

                op.Option_Value.Replace(".", ",");
                List<string> values = op.Option_Value.Split(",").ToList();

                foreach (string value in values)
                {
                    if (value.ToUpper().Equals("ON"))
                    {
                        if (data_user != null)
                            data_user.UserSettings.isTelemetryConsentOn = true;
                    }
                    else if (value.ToUpper().Equals("OFF"))
                    {
                        if (data_user != null)
                            data_user.UserSettings.isTelemetryConsentOn = false;
                    }
                    else if (value.ToUpper().Equals("LOCK"))
                    {
                        if (data_IT != null)
                        {
                            data_IT.Lock_TelemetryConsent = true;
                        }
                        if (data_user != null)
                        {
                            data_user.LockSettings.Lock_TelemetryConsent = true;
                        }
                    }
                    else if (value.ToUpper().Equals("UNLOCK"))
                    {
                        if (data_IT != null)
                        {
                            data_IT.Lock_TelemetryConsent = false;
                        }
                        if (data_user != null)
                        {
                            data_user.LockSettings.Lock_TelemetryConsent = false;
                        }
                    }
                    else
                    {
                        response.Message = $"Telemetry Consent: value format error with [{value}]";
                        Console.WriteLine(response.Message);
                        response.Result = "FAILED";
                        result.serialize_Json_response = JsonConvert.SerializeObject(response, Formatting.Indented);
                        result.ExitCode = (int)CLI_ExitCode.fail_analytics_option_notsupport;
                        return result;
                    }
                    // continue; // 20240827 Refactor the containing loop
                }
                bool status = false;
                if (_SettingsPluginIT != null)
                {
                    if (data_IT != null)
                        status = _SettingsPluginIT.WriteITConfigData(data_IT, new List<string>() { "Lock_TelemetryConsent" }).Result;
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