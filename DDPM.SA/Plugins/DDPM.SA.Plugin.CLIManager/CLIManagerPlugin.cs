#region LicenceHeader
//
// Copyright © 2024, Dell Inc., All Rights Reserved.
// This material is confidential and a trade secret.  Permission to use this
// work for any purpose must be obtained in writing from Dell Inc.
//
// CLIManagerPlugin.cs created on 24/07/2024T04:24 PM
//
#endregion 

using Dell.Client.Framework.Common;
using Dell.Client.Framework.Common.Annotations;
using Dell.Client.Framework.Interfaces;
using Dell.Client.Framework.Common.PluginConditions;
using DDPM.SA.Common;
using Microsoft;
using static DDPM.SA.Common.ICLICommandTable;
using System.Windows.Controls.Primitives;
using Windows.Security.Authentication.OnlineId;
using Dell.Client.Framework.Agent;
using Microsoft.VisualBasic.Logging;
using DDPM.SA.Common.Settings;
using Newtonsoft.Json;

namespace DDPM.SA.Plugin.CLIManager
{
    [Plugin(IDs.CLI_Manager_Plugin, pluginName, PluginOrderGroupType.Core, Version = pluginVersion)]
    [Descriptor(Description = pluginDescription)]
    [Publisher(Name = publisherCompany, Website = publisherWebsite, Support = publisherSupport)]
    [PublishedUnelevatedInterface(new[] { typeof(ICliManagerSA) })] //for DeviceManager of user subagent
    [PublishedInterface(new[] { typeof(ICliManagerIT) })] //for CLI subagent

    public class CLIManagerPlugin : BaseAgentPlugin, IDisposableObservable, ICliManagerSA, ICliManagerIT
    {
        public const string PluginLogId = "CLIManager";

        #region Private Members
        private const string pluginName = "CLIManagerPlugin";
        private const string pluginVersion = "1.0.0";
        private const string pluginDescription = "This plugin implements CLI Manager Plugin.";
        private const string publisherCompany = "Wistron";
        private const string publisherWebsite = "https://www.wistron.com";
        private const string publisherSupport = "This plugin implements CLI Manager Plugin.";

        private IAgent _agent;
        private bool _IsAdministrator = ProcessSecurityHelperWrapper.IsCurrentProcessRunningElevated();
        private static List<CLIEventResult> _result_list = new List<CLIEventResult>();
        private readonly object _resultLock = new object();

        private ISettingsManagerIT? _SettingsPluginIT;
        private readonly object _pluginConditionLock = new object();
        private PluginCondition _SettingsITPluginCondition;

        private enum log_type
        {
            info = 0,
            error
        }
        #endregion

        #region Constructor
        public CLIManagerPlugin(IAgent agent) : base(agent, PluginLogId)
        {
            _agent = agent;
            WriteLog($"SettingsManagerPlugin constructor ...(Admin:{_IsAdministrator})");
        }

        #endregion

        #region Overriding methods
        protected override void OnPluginStarting()
        {
            _agent.PluginManager.PluginsStarted += PluginManagerOnPluginsStarted;

            PluginCondition = new PluginStartedCondition();
            WriteLog("Cli Manager plugin report started");

            InitializeSettingsITPlugin();
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
                WriteLog("[Info] Settings Manager IT plugin with ISettingsManagerIT started.");
            }

            if (e.ChangedPlugins.OfType<ICliManagerSA>().Any())
            {
                WriteLog("[Info] ICliManager plugin with ICliManagerSA started.");
            }

            if (e.ChangedPlugins.OfType<ICliManagerIT>().Any())
            {
                WriteLog("[Info] ICliManager plugin with ICliManagerIT started.");
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
            text = "[CLIManager] " + text;
            Console.WriteLine(text);
            if (log_type == log_type.info)
                Log.Info(text);
            else
                Log.Error(text);
        }
        #endregion

        #region ICliManagerIT implementation

        public Task<CLIEventResult> PerformCommandLineRelay(CommandLineInput commandLineInput)
        {            
            if(commandLineInput == null)
            {
                WriteLog("Empty command input from CLI subagent");
                return Task.FromResult(Response_EmptyCommandInput());
            }

            CLIEventArgs arg = new CLIEventArgs()
            {
                command_guid_string = Guid.NewGuid().ToString(),
                commandLineInput = commandLineInput
            };

            WriteLog($"Got CLI request: ID:{arg.command_guid_string}, command: {arg.commandLineInput.Command}, target type: {arg.commandLineInput.TargetType}");

            //Dean 0812, check if IT admin command and do not relay to CLIProxy if it's IT lock command.
            bool isITCommand = false;
            if (commandLineInput.PluginsType.Equals("APP"))
            {
                CLIEventResult rst;
                switch (commandLineInput.TargetFeature)
                {
                    case "TELEMETRYCONSENT":
                        //If return null means command isn't belong to IT, bypass to user SA(CLIProxy).
                        rst = CLI_Analytics_Consent(commandLineInput, arg.command_guid_string);
                        if (rst == null)
                            break;
                        else
                        {                            
                            //return to IT for the result
                            return Task.FromResult(rst);
                        }
                    default:
                        break;
                }
            }
            //Not IT command, invoke to user plugin for command processing
            OnCLIActionEventNotify(arg);

            CLIEventResult result = new CLIEventResult();
            int counter = 0;
            while (true)
            {
                Sleep(1000);
                lock(_resultLock)
                {
                    int idx = _result_list.FindIndex(x => x.command_guid_string.Trim().ToLower().Equals(arg.command_guid_string.ToLower().Trim()));
                    if (idx >= 0)//result found
                    {
                        result.ticket = _result_list[idx].ticket;
                        result.command_guid_string = _result_list[idx].command_guid_string;
                        result.serialize_Json_response = _result_list[idx].serialize_Json_response;
                        result.ExitCode = _result_list[idx].ExitCode;
                        //clear the processed result
                        _result_list.Remove(result);
                        break;
                    }
                }
                counter++;
                if (counter >= 60)//means timeout
                {
                    result.command_guid_string = arg.command_guid_string;
                    result.serialize_Json_response = Response_NoResultTimeout(commandLineInput);
                    result.ticket = DateTime.Now;
                    result.ExitCode = (int)CLI_ExitCode.wait_command_result_timeout;
                    break;
                }
            }
            //remove result if exist more than 1 hour (memory concern)
            _result_list.RemoveAll(x => DateTime.Now.Subtract(x.ticket).TotalMinutes > 60);

            return Task.FromResult(result);//temp return: it should has timer to check write back result list
        }

        
        #endregion

        #region ICliManagerSA implementation
        public event EventHandler<CLIEventArgs> CLIActionEvent;

        //Target to notify CLIProxy
        private void OnCLIActionEventNotify(CLIEventArgs e)
        {
            if (CLIActionEvent == null || e == null || e == EventArgs.Empty)
                return;

            EventHandler<CLIEventArgs> Handler = CLIActionEvent;
            if (Handler != null)
            {
                Handler.Invoke(this, e);
                WriteLog($"CLIActionEvent Invoked: ID:{e.command_guid_string}");
            }
        }

        public Task WriteCommandResult(CLIEventResult result)
        {
            if(result == null)
            {
                WriteLog("Got null command result from Proxy");
                return Task.FromResult(false);
            }
            lock (_resultLock)
            {
                if (_result_list.Find(x => x.command_guid_string.Trim().ToLower().Equals(result.command_guid_string.ToLower().Trim())) == null)
                {

                    _result_list.Add(result);
                }

                else
                {
                    WriteLog($"Duplicated result from Proxy: ID:{result.command_guid_string}");
                }
                return Task.FromResult(true);
            }
        }
        #endregion

        #region require setting manager IT
        private void InitializeSettingsITPlugin()
        {
            if (_SettingsPluginIT != null)
                return;

            _SettingsPluginIT = _agent.PluginManager.FindPluginByType<ISettingsManagerIT>(PluginResolution.Dynamic);
            if (_SettingsPluginIT is IFrameworkPluginConditionNotification condition)
            {
                condition.PluginConditionChangeHandler += OnSettingsITPluginConditionChangeHandler;
                GetCurrentSettingsITPluginCondition();
            }
        }

        private void GetCurrentSettingsITPluginCondition()
        {
            _ = Task.Run(async () =>
            {
                var pluginCondition = await (_SettingsPluginIT as IFrameworkPluginConditionNotification)?.CurrentConditionAsync();

                lock (_pluginConditionLock)
                {
                    _SettingsITPluginCondition = pluginCondition;

                    if (pluginCondition is PluginErrorCondition)
                    {
                        WriteLog($"{nameof(GetCurrentSettingsITPluginCondition)} - Settings Manager IT Plugin is in an error condition");
                    }
                    else if (pluginCondition is PluginRunningCondition || pluginCondition is PluginStartedCondition)
                    {
                        WriteLog($"{nameof(GetCurrentSettingsITPluginCondition)} - Settings Manager IT Plugin is in {nameof(pluginCondition)} condition");
                    }
                }
            });
        }

        private void OnSettingsITPluginConditionChangeHandler(object sender, EventArgs e)
        {
            InitializeSettingsITPlugin();
        }
        #endregion

        #region CLI operation for IT
        private (int code, string msg) RetrieveITSettings(out DDPMITConfig data)
        {
            if(_SettingsPluginIT == null)
            {
                data = null;
                string reason = "Fail to read IT settings caused by null settings plugin";
                return ((int)CLI_ExitCode.null_settings_plugin_IT, reason);
            }
            data = _SettingsPluginIT.ReadITConfigData().Result;
            if (data == null)
            {
                string reason = "Fail to read IT settings";              
                return ((int)CLI_ExitCode.fail_read_settings, reason);
            }

            return ((int)CLI_ExitCode.success, "Success");
        }

        //If return null means it's not IT command
        private CLIEventResult CLI_Analytics_Consent(CommandLineInput commandLineInput, string action_guid)
        {
            CLIEventResult result = new CLIEventResult();
            result.ticket = DateTime.Now;
            result.command_guid_string = action_guid;

            CLI_RESPONSE response = new CLI_RESPONSE();
            response.Command = commandLineInput.Command;
            response.TargetFeature = commandLineInput.TargetFeature;
                        
            if (commandLineInput.Options == null || commandLineInput.Options.Count == 0)
            {
                response.Message = "Option is missing";
                result.serialize_Json_response = JsonConvert.SerializeObject(response, Formatting.Indented);
                result.ExitCode = (int)CLI_ExitCode.fail_no_analytics_options;
                return result;
            }

            if (commandLineInput.Command.Equals("GET")) //ex: cli.exe /get -app=TelemetryConsent -value=isAllow
            {
                bool ever = false;
                result.ExitCode = (int)CLI_ExitCode.success;
                foreach (var op in commandLineInput.Options)
                {
                    if (op.Option_Name.ToUpper().Equals("VALUE"))
                    {
                        if (op.Option_Value.ToUpper().Equals("ISALLOW"))
                        {
                            DDPMITConfig data = new DDPMITConfig();
                            var tmp = RetrieveITSettings(out data);
                            if (tmp.code == (int)CLI_ExitCode.success)
                            {
                                WriteLog($"Telemetry Consent: isAllow {data.isTelemetryConsentAllow}");
                                response.Value = $"{data.isTelemetryConsentAllow}";
                                response.Result = $"isAllow = {data.isTelemetryConsentAllow}";
                                response.Message = tmp.msg;
                                result.ExitCode = tmp.code;
                                ever = true;
                            }                            
                        }
                        else
                        {
                            WriteLog($"Telemetry Consent: option value [{op.Option_Value}] not support");
                            response.Result = "Fail";
                            response.Message = $"Telemetry Consent: option value [{op.Option_Value}] not support";
                            result.ExitCode = (int)CLI_ExitCode.fail_analytics_option_notsupport;
                            ever = true;
                        }
                    }
                    else
                    {
                        WriteLog($"Telemetry Consent: option name [{op.Option_Name}] not support");
                        response.Result = "Fail";
                        response.Message = $"Telemetry Consent: option name [{op.Option_Name}] not support";
                        result.ExitCode = (int)CLI_ExitCode.fail_analytics_option_notsupport;
                        ever = true;
                    }
                }
                if (ever)
                {
                    result.serialize_Json_response = JsonConvert.SerializeObject(response, Formatting.Indented);                    
                    return result;
                }
            }
            else if (commandLineInput.Command.Equals("SET")) //ex: cli.exe /set -app=TelemetryConsent -Allow=yes(or no)
            {
                result.ExitCode = (int)CLI_ExitCode.success;
                foreach (var op in commandLineInput.Options)
                {
                    if (op.Option_Name.ToUpper().Equals("ALLOW"))
                    {
                        DDPMITConfig data = new DDPMITConfig();
                        var tmp = RetrieveITSettings(out data);
                        if (tmp.code != (int)CLI_ExitCode.success)
                        {
                            response.Message = tmp.msg;
                            result.serialize_Json_response = JsonConvert.SerializeObject(response, Formatting.Indented);
                            result.ExitCode = tmp.code;
                            return result;
                        }
                        bool status = true;
                        if (op.Option_Value.ToUpper().Equals("YES"))
                        {
                            if(data.isTelemetryConsentAllow != true)
                            {
                                data.isTelemetryConsentAllow = true;
                                status = _SettingsPluginIT.WriteITConfigData(data, new List<string>() { "isTelemetryConsentAllow" }).Result;
                            }
                            else
                            {
                                response.Message = "TelemetryConsent is ALLOW already";
                            }
                        }
                        else if (op.Option_Value.ToUpper().Equals("NO"))
                        {
                            if (data.isTelemetryConsentAllow != false)
                            {
                                data.isTelemetryConsentAllow = false;
                                status = _SettingsPluginIT.WriteITConfigData(data, new List<string>() { "isTelemetryConsentAllow" }).Result;  
                            }
                            else
                            {
                                response.Message = "TelemetryConsent is NOT ALLOW already";
                            }
                        }
                        else
                        {
                            WriteLog($"Telemetry Consent: option value [{op.Option_Value}] not support");
                            response.Result = "Fail";
                            response.Message = $"Telemetry Consent: option value [{op.Option_Value}] not support";
                            result.ExitCode = (int)CLI_ExitCode.fail_analytics_option_notsupport;
                            return result;
                        }

                        //WriteLog($"Telemetry Consent: set Allow to {data.isTelemetryConsentAllow}");
                        //response.Message = $"Set Allow of consent to be {op.Option_Value}";

                        if (status)
                        {                            
                            response.Result = "Completed";
                            result.serialize_Json_response = JsonConvert.SerializeObject(response, Formatting.Indented);
                            result.ExitCode = (int)CLI_ExitCode.success;
                            return result;
                        }
                        else
                        {
                            response.Message = "Write to IT config failed";
                            response.Result = "FAIL";
                            result.serialize_Json_response = JsonConvert.SerializeObject(response, Formatting.Indented);
                            result.ExitCode = (int)CLI_ExitCode.fail_SetSettings_ITSettingsValue;
                            return result;
                        }
                    }
                    else
                    {
                        WriteLog($"Telemetry Consent: option name [{op.Option_Name}] not support");
                        response.Result = "Fail";
                        response.Message = $"Telemetry Consent: option name [{op.Option_Name}] not support";
                        result.ExitCode = (int)CLI_ExitCode.fail_analytics_option_notsupport;
                        return result;
                    }
                }
            }

            return null;
        }
        #endregion
    }
}
