#region LicenceHeader

//
// Copyright © 2024, Dell Inc., All Rights Reserved.
// This material is confidential and a trade secret.  Permission to use this
// work for any purpose must be obtained in writing from Dell Inc.
//
// CLIManagerPlugin.cs created on 24/07/2024T04:24 PM
//

#endregion

using DDPM.SA.Common;
using DDPM.SA.Common.CLI;
using DDPM.SA.Common.Settings;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.Common.Annotations;
using Dell.Client.Framework.Common.PluginConditions;
using Dell.Client.Framework.Interfaces;
using Microsoft;
using Newtonsoft.Json;
using static DDPM.SA.Common.ICLICommandTable;

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
            if (commandLineInput == null)
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

            //Dean 0816, check if IT admin command and do not relay to CLIProxy if it's IT lock command.
            if (commandLineInput.isITCommands)
            {
                CLIEventResult rst = HandleITCommands(commandLineInput, arg.command_guid_string);
                if (!commandLineInput.isNormalCommands)//only IT command, return directly
                    return Task.FromResult(rst);

                if (rst.ExitCode != (int)CLI_ExitCode.success)//IT command failed
                    return Task.FromResult(rst);
            }

            //Include IT command, invoke to user plugin for command processing
            if (commandLineInput.isNormalCommands)
                OnCLIActionEventNotify(arg);

            CLIEventResult result = new CLIEventResult();
            int counter = 0;
            while (true)
            {
                Sleep(1000);
                lock (_resultLock)
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

        private CLIEventResult HandleITCommands(CommandLineInput commandLineInput, string command_guid)
        {
            CLIEventResult rst = new CLIEventResult();
            rst.ticket = DateTime.Now;
            rst.ExitCode = (int)CLI_ExitCode.IT_Command_Not_Support;
            rst.command_guid_string = command_guid;

            CLI_RESPONSE response = new CLI_RESPONSE();//for fail return using
            response.TargetFeature = commandLineInput.TargetFeature;
            response.Command = commandLineInput.Command;

            //Get IT data before passing to function
            DDPMITConfig data;
            var tmp = RetrieveITSettings(out data);
            if (tmp.code != (int)CLI_ExitCode.success)
            {
                response.Message = tmp.msg;
                response.Result = "FAIL";
                rst.serialize_Json_response = JsonConvert.SerializeObject(response, Formatting.Indented);
                return rst;
            }

            if (commandLineInput.PluginsType.Equals("APP"))
            {
                // !!!
                // Implement this switch-case, should match the Support_Lock_Feature in ICLICommandTable.cs
                // !!!
                switch (commandLineInput.TargetFeature)
                {
                    case "TELEMETRYCONSENT":
                        //If return null means command isn't belong to IT, bypass to user SA(CLIProxy).
                        rst = CLIHandlerApp.CLI_Analytics_Consent(Log, data, _SettingsPluginIT, commandLineInput, command_guid);
                        //rst = CLI_Analytics_Consent(commandLineInput, command_guid);
                        return rst;

                    case "INAPPUPDATE":              //InAppUpdate               DDPMW-1329/1330
                        break;
                    case "INAPPBRICONT":             //InAppBriCont              DDPMW-1342/1343
                        break;
                    case "INAPPAUTOBRITEMP":         //InAppAutoBriTemp          DDPMW-1341
                        break;
                    case "INAPPRESTOREDEFAULTS":     //InAppRestoreDefaults      DDPMW-1333
                        break;
                    case "INAPPRESTORE":             //InAppRestore              Same as InAppRestoreDefaults
                        break;
                    case "RESTOREFACTORYDEFAULTS":   //RestoreFactoryDefaults    DDPMW-2013/2014/2015/2111/2114
                        break;
                    case "SCREENNOTIFICATION":       //ScreenNotification        DDPMW-1901
                        break;
                    case "RESOLUTIONREFRESHRATE":    //ResolutionRefreshRate     DDPMW-1344
                        break;
                    case "USBCPRIORITIZATION":       //USBCPrioritization        DDPMW-1345
                        break;
                    case "ACTIVEINPUTSOURCE":        //ActiveInputSource         DDPMW-1346
                        break;
                    case "USBKVM":                   //USBKVM                    DDPMW-1347
                        break;
                    case "INAPPNETWORKKVM":          //InAppNetworkKVM           DDPMW-1599
                        break;
                    case "EASYARRANGELAYOUT":        //EasyArrangeLayout         DDPMW-1350
                        break;
                    case "INAPPCOLORPRESET":         //InAppColorPreset          DDPMW-1351/1352
                        break;
                    case "POWERNAP":                 //PowerNap                  DDPMW-1361
                        break;
                    case "INAPPEXPORTSETTINGS":      //InAppExportSettings       DDPMW-1335
                        break;
                    case "COLLABSCREENSHARE":        //CollabScreenShare         DDPMW-1843
                        break;
                    case "HDR":                      //hdr                       DDPMW-1729
                        break;
                    case "ANTIFLICKER":              //AntiFlicker               DDPMW-1735
                        break;
                    case "MICSWITCH":                //MicSwitch                 DDPMW-1736
                        break;
                    case "AIAUTOFRAMING":            //AIAutoFraming             DDPMW-1742
                        break;
                    case "PRESENCEDETECTION":        //PresenceDetection         DDPMW-1747
                        break;
                    case "ANCMODE":                  //ancMode                   DDPMW-1853
                        break;
                    case "MICNOISECANCELLATION":     //micNoiseCancellation      DDPMW-1855
                        break;
                    case "WEARDETECTION":             //wearDetection             DDPMW-2093
                        break;
                    default:
                        response.Message = $"Feature {commandLineInput.TargetFeature} doesn't in global setting support list";
                        response.Result = "FAIL";
                        rst.serialize_Json_response = JsonConvert.SerializeObject(response, Formatting.Indented);
                        break;
                }
            }
            else
            {
                //CLI_RESPONSE response = new CLI_RESPONSE();
                //response.TargetFeature = commandLineInput.TargetFeature;
                //response.Command = commandLineInput.Command;
                response.Message = $"{commandLineInput.TargetFeature} doesn't support as global setting";
                response.Result = "FAIL";
                rst.serialize_Json_response = JsonConvert.SerializeObject(response, Formatting.Indented);
            }

            return rst;
        }

        #endregion

        #region ICliManagerSA implementation

        public event EventHandler<CLIEventArgs> CLIActionEvent;

        //Target to notify CLIProxy
        private void OnCLIActionEventNotify(CLIEventArgs e)
        {
            Task.Run(() =>
            {
                if (CLIActionEvent == null || e == null || e == EventArgs.Empty)
                    return;

                EventHandler<CLIEventArgs> Handler = CLIActionEvent;
                if (Handler != null)
                {
                    Handler.Invoke(this, e);
                    WriteLog($"CLIActionEvent Invoked: ID:{e.command_guid_string}");
                }
            });
        }

        public Task WriteCommandResult(CLIEventResult result)
        {
            if (result == null)
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
            if (_SettingsPluginIT == null)
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
        /*private CLIEventResult CLI_Analytics_Consent(CommandLineInput commandLineInput, string action_guid)
        {
            //Expected format: /set -app=TelemetryConsent -value=on,lock / on,unlock / off,lock / off,unlock

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
            if (commandLineInput.Options.Count > 1)
            {
                WriteLog("Telemetry Consent: doesn't support multiple value options");
                response.Result = "Fail";
                response.Message = "Telemetry Consent: doesn't support multiple value options";
                result.ExitCode = (int)CLI_ExitCode.fail_analytics_option_notsupport;
                result.serialize_Json_response = JsonConvert.SerializeObject(response, Formatting.Indented);
                return result;
            }
            CommandType_Option op = commandLineInput.Options[0];

            //Retrieve IT settings first
            DDPMITConfig data = new DDPMITConfig();
            var get_IT_setting_result = RetrieveITSettings(out data);
            if (get_IT_setting_result.code != (int)CLI_ExitCode.success)
            {
                response.Message = get_IT_setting_result.msg;
                result.serialize_Json_response = JsonConvert.SerializeObject(response, Formatting.Indented);
                result.ExitCode = get_IT_setting_result.code;
                return result;
            }

            if (!commandLineInput.Command.Equals("SET"))
            {
                WriteLog("Telemetry Consent: for global IT setting the command should be SET");
                response.Result = "Fail";
                response.Value = op.Option_Value;
                response.Message = "Telemetry Consent: LOCK/UNLOCK only support SET command";
                result.ExitCode = (int)CLI_ExitCode.fail_analytics_command_notsupport;
                result.serialize_Json_response = JsonConvert.SerializeObject(response, Formatting.Indented);
                return result;
            }

            result.ExitCode = (int)CLI_ExitCode.success;

            if (!op.Option_Name.ToUpper().Equals("VALUE"))
            {
                WriteLog($"Telemetry Consent: option name [{op.Option_Name}] not support");
                response.Result = "Fail";
                response.Value = op.Option_Value;
                response.Message = $"Telemetry Consent: option name [{op.Option_Name}] not support";
                result.ExitCode = (int)CLI_ExitCode.fail_analytics_option_notsupport;
                result.serialize_Json_response = JsonConvert.SerializeObject(response, Formatting.Indented);
                return result;
            }
            bool status = true;
            List<string> ops = op.Option_Value.Split(",").ToList();
            if (ops.FindIndex(x => x.ToUpper().Trim().Equals("UNLOCK"))>= 0)
            {
                if (data.Lock_TelemetryConsent != false)
                {
                    data.Lock_TelemetryConsent = false;
                    status = _SettingsPluginIT.WriteITConfigData(data, new List<string>() { "Lock_TelemetryConsent" }).Result;
                }
                else
                {
                    response.Message = "TelemetryConsent is UNLOCKed already";
                    status = true;
                }
            }
            else if (ops.FindIndex(x => x.ToUpper().Trim().Equals("LOCK")) >= 0)
            {
                if (data.Lock_TelemetryConsent != true)
                {
                    data.Lock_TelemetryConsent = true;
                    status = _SettingsPluginIT.WriteITConfigData(data, new List<string>() { "Lock_TelemetryConsent" }).Result;
                }
                else
                {
                    response.Message = "TelemetryConsent is LOCKed already";
                    status = true;
                }
            }
            else
            {
                WriteLog($"Telemetry Consent: option value [{op.Option_Value}] not support");
                response.Result = "Fail";
                response.Message = $"Telemetry Consent: option value [{op.Option_Value}] not support";
                response.Value = op.Option_Value;
                result.ExitCode = (int)CLI_ExitCode.fail_analytics_option_notsupport;
                result.serialize_Json_response = JsonConvert.SerializeObject(response, Formatting.Indented);
                return result;
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
                response.Message = "Write to IT config failed";
                response.Result = "FAIL";
                response.Value = op.Option_Value;
                result.serialize_Json_response = JsonConvert.SerializeObject(response, Formatting.Indented);
                result.ExitCode = (int)CLI_ExitCode.fail_SetSettings_ITSettingsValue;
                return result;
            }
        }*/

        #endregion
    }
}