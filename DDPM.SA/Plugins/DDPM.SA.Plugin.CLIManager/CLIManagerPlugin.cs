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
using DDPM.SA.Common.Defer;
using DDPM.SA.Common.Settings;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.Common.Annotations;
using Dell.Client.Framework.Common.PluginConditions;
using Dell.Client.Framework.Interfaces;
using Microsoft;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Threading;
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
        private const string publisherCompany = "Dell Inc.";
        private const string publisherWebsite = "https://www.dell.com";
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
            WriteLog($"CLIMamagerPlugin constructor ...(Admin:{_IsAdministrator})");
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
            if (text.Length >= 500)
                text = text.Substring(0, 500);
            string logString = $"[CLIManager] {System.Security.SecurityElement.Escape(text)}";
            //text = "[CLIManager] " + text;
            Console.WriteLine(logString);
            if (Log != null)
            {
                if (log_type == log_type.info)
                    Log.Info(logString);
                else
                    Log.Error(logString);
            }
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
                command_guid_string = string.IsNullOrEmpty(commandLineInput.remote_mgr_guid) ? Guid.NewGuid().ToString() : commandLineInput.remote_mgr_guid, //support remote command GUID
                commandLineInput = commandLineInput
            };

            WriteLog($"Got CLI request: ID:{arg.command_guid_string}, command: {arg.commandLineInput.Command}, target type: {arg.commandLineInput.TargetType}");
            Trace.WriteLine($"check is ITcommand: {commandLineInput.isITCommands}, option value: {commandLineInput.Options.ToString()}");
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
                //1028 change to timeout value that be customized by each command.Default is 60s.
                if (counter >= commandLineInput.nTimeOutValue)//60)//means timeout
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

            APP_RESPONSE response = new APP_RESPONSE();//for fail return using
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
                rst.ExitCode = tmp.code;
                return rst;
            }

            List<string> peripheral_DeviceType = new List<string>()
                {
                    "MOUSE",
                    "KEYBOARD",
                    "AUDIO",
                    "PEN",
                    "WEBCAM",
                };

            // for NetworkKVM commands other than InAppNetworkKVM
            if (commandLineInput.TargetFeature.Contains("NETWORKKVM") && commandLineInput.TargetFeature != "INAPPNETWORKKVM")
            {
                if (commandLineInput.TargetType == "DISPLAY")
                {
                    WriteLog("ExecuteNetworkKVM Entry");
                    var _ = ExecuteNetworkKVM(commandLineInput);
                    WriteLog("ExecuteNetworkKVM Exit");
                    rst.ExitCode = _.code;
                    rst.serialize_Json_response = _.result;

                    if (_.code == (int)CLI_ExitCode.success && commandLineInput.TargetFeature == "NETWORKKVM")
                    {
                        if (commandLineInput.Command == "GET")
                        {
                            rst.serialize_Json_response = JsonConvert.SerializeObject(new NKVM_RESPONSE
                            {
                                Command = commandLineInput.Command,
                                TargetFeature = commandLineInput.TargetFeature,
                                Result = "PASS",
                                Value = (data.Enable_Display_NetworkKVM ? "ON" : "OFF") + (data.Lock_Display_NetworkKVM ? ", DISABLE" : ", ENABLE"),
                            }, Formatting.Indented);
                        }
                        else if (commandLineInput.Command == "SET")
                        {
                            bool? result;

                            if (commandLineInput.Options[0].Option_Value == "ON" || commandLineInput.Options[0].Option_Value == "OFF")
                            {
                                data.Enable_Display_NetworkKVM = commandLineInput.Options[0].Option_Value == "ON";
                                WriteLog($"WriteITConfigData Enable_Display_NetworkKVM: {data.Enable_Display_NetworkKVM} Entry");
                                result = _SettingsPluginIT?.WriteITConfigData(data, new List<string>() { "Enable_Display_NetworkKVM" }).Result;
                                WriteLog("WriteITConfigData Enable_Display_NetworkKVM Exit");
                            }
                            else
                            {
                                data.Lock_Display_NetworkKVM = commandLineInput.Options[0].Option_Value == "DISABLE";
                                WriteLog($"WriteITConfigData Lock_Display_NetworkKVM: {data.Lock_Display_NetworkKVM} Entry");
                                result = _SettingsPluginIT?.WriteITConfigData(data, new List<string>() { "Lock_Display_NetworkKVM" }).Result;
                                WriteLog("WriteITConfigData Lock_Display_NetworkKVM Exit");
                            }

                            if (result != true)
                            {
                                rst.serialize_Json_response = JsonConvert.SerializeObject(new NKVM_RESPONSE
                                {
                                    Command = commandLineInput.Command,
                                    TargetFeature = commandLineInput.TargetFeature,
                                    Result = "FAIL",
                                    Value = commandLineInput.Options[0].Option_Value,
                                    Message = "Failed to update config"
                                }, Formatting.Indented);
                                rst.ExitCode = (int)CLI_ExitCode.fail_SetSettings_ITSettingsValue;
                            }
                        }
                    }
                }
                else
                {
                    rst = CLIHandlerDisplay.CLI_Response_TypeNotSupport(commandLineInput, rst);
                }

                return rst;
            }

            // !!!
            // Implement this switch-case, should match the Support_Lock_Feature in ICLICommandTable.cs
            // !!!
            switch (commandLineInput.TargetFeature)
            {
                case "TELEMETRYCONSENT":
                    //If return null means command isn't belong to IT, bypass to user SA(CLIProxy).
                    if (commandLineInput.PluginsType.Equals("APP"))
                        rst = CLIHandlerApp.CLI_Analytics_Consent(Log, data, _SettingsPluginIT, commandLineInput, command_guid);
                    else
                        rst = CLIHandlerApp.CLI_Response_TypeNotSupport(commandLineInput, rst);
                    return rst;

                case "INAPPUPDATE":              //InAppUpdate               DDPMW-1329/1330
                    if (commandLineInput.PluginsType.Equals("APP"))
                        rst = CLIHandlerApp.CLI_App_LockUnlock(Log, data, _SettingsPluginIT, commandLineInput, command_guid);
                    else
                        rst = CLIHandlerApp.CLI_Response_TypeNotSupport(commandLineInput, rst);
                    return rst;
                //break;
                case "INAPPBRICONT":             //InAppBriCont              DDPMW-1342/1343
                    if (commandLineInput.PluginsType.Equals("DISPLAY"))
                        rst = CLIHandlerDisplay.CLI_Display_LockUnlock(Log, data, _SettingsPluginIT, commandLineInput, command_guid);
                    else
                        rst = CLIHandlerDisplay.CLI_Response_TypeNotSupport(commandLineInput, rst);
                    return rst;
                //break;
                //case "INAPPAUTOBRITEMP":         //InAppAutoBriTemp          DDPMW-1341
                case "INAPPAUTOBRIGHTNESSCOLOR"://1004 InAppAutoBrightnessColor DDPMW1341
                    if (commandLineInput.PluginsType.Equals("DISPLAY"))
                        rst = CLIHandlerDisplay.CLI_Display_LockUnlock(Log, data, _SettingsPluginIT, commandLineInput, command_guid);
                    else
                        rst = CLIHandlerDisplay.CLI_Response_TypeNotSupport(commandLineInput, rst);
                    return rst;
                //break;
                case "INAPPRESTOREDEFAULTS":     //InAppRestoreDefaults      DDPMW-1333
                    if (commandLineInput.PluginsType.Equals("APP"))
                        rst = CLIHandlerApp.CLI_App_LockUnlock(Log, data, _SettingsPluginIT, commandLineInput, command_guid);
                    else
                        rst = CLIHandlerDisplay.CLI_Response_TypeNotSupport(commandLineInput, rst);
                    return rst;
                //break;
                //case "INAPPRESTORE":             //InAppRestore              Same as InAppRestoreDefaults
                //break;
                case "RESTOREFACTORYDEFAULTS":   //RestoreFactoryDefaults    DDPMW-2013/2014/2015/2111/2114
                    if (peripheral_DeviceType.FindIndex(x => x.Equals(commandLineInput.PluginsType)) >= 0)
                        rst = CLIHandlerPeripheral.CLI_Peripheral_RestoreFactoryDefault(Log, data, _SettingsPluginIT, commandLineInput, command_guid);
                    else if (commandLineInput.PluginsType.Equals("DISPLAY"))
                        rst = CLIHandlerDisplay.CLI_Display_RestoreFactoryDefault(Log, data, _SettingsPluginIT, commandLineInput, command_guid);
                    else
                        rst = CLIHandlerPeripheral.CLI_Response_TypeNotSupport(commandLineInput, rst);
                    return rst;
                //break;
                case "SCREENNOTIFICATION":       //ScreenNotification        DDPMW-1901
                    if (commandLineInput.PluginsType.Equals("APP"))
                        rst = CLIHandlerApp.CLI_Common_LockUlockWithUserAction(Log, data, _SettingsPluginIT, commandLineInput, command_guid);
                    else
                        rst = CLIHandlerApp.CLI_Response_TypeNotSupport(commandLineInput, rst);
                    break;
                case "RESOLUTIONREFRESHRATE":    //ResolutionRefreshRate     DDPMW-1344
                    if (commandLineInput.PluginsType.Equals("DISPLAY"))
                        rst = CLIHandlerDisplay.CLI_Display_LockUnlockWithUserAction(Log, data, _SettingsPluginIT, commandLineInput, command_guid);
                    else
                        rst = CLIHandlerDisplay.CLI_Response_TypeNotSupport(commandLineInput, rst);
                    break;
                case "USBCPRIORITIZATION":       //USBCPrioritization        DDPMW-1345
                    if (commandLineInput.PluginsType.Equals("DISPLAY"))
                        rst = CLIHandlerDisplay.CLI_Display_LockUnlockWithUserAction(Log, data, _SettingsPluginIT, commandLineInput, command_guid);
                    else
                        rst = CLIHandlerDisplay.CLI_Response_TypeNotSupport(commandLineInput, rst);
                    break;
                case "ACTIVEINPUTSOURCE":        //ActiveInputSource         DDPMW-1346
                    if (commandLineInput.PluginsType.Equals("DISPLAY"))
                        rst = CLIHandlerDisplay.CLI_Display_LockUnlockWithUserAction(Log, data, _SettingsPluginIT, commandLineInput, command_guid);
                    else
                        rst = CLIHandlerDisplay.CLI_Response_TypeNotSupport(commandLineInput, rst);
                    break;
                case "INAPPUSBKVM":                   //InAppUSBKVM                    DDPMW-1347
                    if (commandLineInput.PluginsType.Equals("DISPLAY"))
                        rst = CLIHandlerDisplay.CLI_Display_LockUnlockWithUserAction(Log, data, _SettingsPluginIT, commandLineInput, command_guid);
                    else
                        rst = CLIHandlerDisplay.CLI_Response_TypeNotSupport(commandLineInput, rst);
                    break;
                case "INAPPNETWORKKVM":          //InAppNetworkKVM           DDPMW-1599
                    if (commandLineInput.PluginsType.Equals("DISPLAY"))
                    {
                        WriteLog("INAPPNETWORKKVM CLI_Display_LockUnlock Entry");
                        rst = CLIHandlerDisplay.CLI_Display_LockUnlock(Log, data, _SettingsPluginIT, commandLineInput, command_guid);
                        WriteLog("INAPPNETWORKKVM CLI_Display_LockUnlock Exit");
                    }
                    else
                        rst = CLIHandlerDisplay.CLI_Response_TypeNotSupport(commandLineInput, rst);
                    return rst;
                //break;
                case "EASYARRANGELAYOUT":        //EasyArrangeLayout         DDPMW-1350
                    if (commandLineInput.PluginsType.Equals("DISPLAY"))
                        rst = CLIHandlerDisplay.CLI_Display_LockUnlockWithUserAction(Log, data, _SettingsPluginIT, commandLineInput, command_guid);
                    else
                        rst = CLIHandlerDisplay.CLI_Response_TypeNotSupport(commandLineInput, rst);
                    break;
                case "INAPPCOLORPRESET":         //InAppColorPreset          DDPMW-1351/1352
                    if (commandLineInput.PluginsType.Equals("DISPLAY"))
                        rst = CLIHandlerDisplay.CLI_Display_LockUnlock(Log, data, _SettingsPluginIT, commandLineInput, command_guid);
                    else
                        rst = CLIHandlerDisplay.CLI_Response_TypeNotSupport(commandLineInput, rst);
                    return rst;
                //break;
                case "POWERNAP":                 //PowerNap                  DDPMW-1361
                    if (commandLineInput.PluginsType.Equals("DISPLAY"))
                        rst = CLIHandlerDisplay.CLI_Display_LockUnlockWithUserAction(Log, data, _SettingsPluginIT, commandLineInput, command_guid);
                    else
                        rst = CLIHandlerDisplay.CLI_Response_TypeNotSupport(commandLineInput, rst);
                    return rst;
                //break;
                case "INAPPEXPORTIMPORT":      //INAPPEXPORTIMPORT       DDPMW-1335
                    if (commandLineInput.PluginsType.Equals("DISPLAY"))
                        rst = CLIHandlerDisplay.CLI_Display_LockUnlock(Log, data, _SettingsPluginIT, commandLineInput, command_guid);
                    else
                        rst = CLIHandlerDisplay.CLI_Response_TypeNotSupport(commandLineInput, rst);
                    return rst;
                //break;
                case "COLLABSCREENSHARE":        //CollabScreenShare         DDPMW-1843
                    if (commandLineInput.PluginsType.Equals("KEYBOARD"))
                        rst = CLIHandlerPeripheral.CLI_Peripheral_LockUlockWithUserAction(Log, data, _SettingsPluginIT, commandLineInput, command_guid);
                    else
                        rst = CLIHandlerPeripheral.CLI_Response_TypeNotSupport(commandLineInput, rst);
                    break;
                case "HDR":                      //hdr                       DDPMW-1729
                    if (commandLineInput.PluginsType.Equals("WEBCAM"))
                        rst = CLIHandlerPeripheral.CLI_Peripheral_LockUlockWithUserAction(Log, data, _SettingsPluginIT, commandLineInput, command_guid);
                    else
                        rst = CLIHandlerPeripheral.CLI_Response_TypeNotSupport(commandLineInput, rst);
                    break;
                case "ANTIFLICKER":              //AntiFlicker               DDPMW-1735
                    if (commandLineInput.PluginsType.Equals("WEBCAM"))
                        rst = CLIHandlerPeripheral.CLI_Peripheral_LockUlockWithUserAction(Log, data, _SettingsPluginIT, commandLineInput, command_guid);
                    else
                        rst = CLIHandlerPeripheral.CLI_Response_TypeNotSupport(commandLineInput, rst);
                    break;
                case "MICSWITCH":                //MicSwitch                 DDPMW-1736
                    if (commandLineInput.PluginsType.Equals("WEBCAM"))
                        rst = CLIHandlerPeripheral.CLI_Peripheral_LockUlockWithUserAction(Log, data, _SettingsPluginIT, commandLineInput, command_guid);
                    else
                        rst = CLIHandlerPeripheral.CLI_Response_TypeNotSupport(commandLineInput, rst);
                    break;
                case "AIAUTOFRAMING":            //AIAutoFraming             DDPMW-1742
                    if (commandLineInput.PluginsType.Equals("WEBCAM"))
                        rst = CLIHandlerPeripheral.CLI_Peripheral_LockUlockWithUserAction(Log, data, _SettingsPluginIT, commandLineInput, command_guid);
                    else
                        rst = CLIHandlerPeripheral.CLI_Response_TypeNotSupport(commandLineInput, rst);
                    break;
                case "PRESENCEDETECTION":        //PresenceDetection         DDPMW-1747
                    if (commandLineInput.PluginsType.Equals("WEBCAM"))
                        rst = CLIHandlerPeripheral.CLI_Peripheral_LockUlockWithUserAction(Log, data, _SettingsPluginIT, commandLineInput, command_guid);
                    else
                        rst = CLIHandlerPeripheral.CLI_Response_TypeNotSupport(commandLineInput, rst);
                    break;
                case "ANCMODE":                  //ancMode                   DDPMW-1853
                    if (commandLineInput.PluginsType.Equals("AUDIO"))
                        rst = CLIHandlerPeripheral.CLI_Peripheral_LockUlockWithUserAction(Log, data, _SettingsPluginIT, commandLineInput, command_guid);
                    else
                        rst = CLIHandlerPeripheral.CLI_Response_TypeNotSupport(commandLineInput, rst);
                    break;
                case "MICNOISECANCELLATION":     //micNoiseCancellation      DDPMW-1855
                    if (commandLineInput.PluginsType.Equals("AUDIO"))
                        rst = CLIHandlerPeripheral.CLI_Peripheral_LockUlockWithUserAction(Log, data, _SettingsPluginIT, commandLineInput, command_guid);
                    else
                        rst = CLIHandlerPeripheral.CLI_Response_TypeNotSupport(commandLineInput, rst);
                    break;
                case "WEARDETECTION":             //wearDetection             DDPMW-2093
                    if (commandLineInput.PluginsType.Equals("AUDIO"))
                        rst = CLIHandlerPeripheral.CLI_Peripheral_LockUlockWithUserAction(Log, data, _SettingsPluginIT, commandLineInput, command_guid);
                    else
                        rst = CLIHandlerPeripheral.CLI_Response_TypeNotSupport(commandLineInput, rst);
                    break;
                default:
                    response.Message = $"Feature {commandLineInput.TargetFeature} doesn't in global setting support list";
                    response.Result = "FAIL";
                    rst.serialize_Json_response = JsonConvert.SerializeObject(response, Formatting.Indented);
                    break;
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

        public event EventHandler<CLIEventResult> CLIActionResult;

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
                Task.Run(() =>
                {
                    EventHandler<CLIEventResult> handler = CLIActionResult;
                    handler?.Invoke(this, result);
                });
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
        #endregion

        #region NetworkKVM
        private CommandLineInput _commandLineInput;

        private (int code, string result) NotSupportResponse()
        {
            var response = new NKVM_RESPONSE
            {
                Command = _commandLineInput.Command,
                TargetFeature = _commandLineInput.TargetFeature,
                Result = "FAIL",
                Message = "Un-supported command or value"
            };
            return ((int)CLI_ExitCode.unknow_command, response.ToJson());
        }

        public (int code, string result) ExecuteNetworkKVM(CommandLineInput commandLineInput)
        {
            _commandLineInput = commandLineInput;

            switch (_commandLineInput.TargetFeature)
            {
                case "NETWORKKVMVERSION":
                    return NetworkKVMVersion();
                case "NETWORKKVM":
                case "NETWORKKVMAUTOCONNECT":
                case "NETWORKKVMCONTENTTRANSFER":
                    return EntryNetworkKVM(_commandLineInput.TargetFeature);
                case "NETWORKKVMINCOMINGPORT":
                case "NETWORKKVMOUTGOINGPORT":
                case "NETWORKKVMCONTENTTRANSFERPORT":
                    return EntryNetworkKVMPort(_commandLineInput.TargetFeature);
                case "NETWORKKVMACCESSRESET":
                    return NetworkKVMAccessReset(_commandLineInput.TargetFeature);
                default:
                    return NotSupportResponse();
            }
        }

        private (int exitCode, string value, string message) RunDDMCommand(string command)
        {
            WriteLog($"RunDDMCommand({command}) Entry");
            string filePath = @"C:\Program Files\Dell\Dell Display and Peripheral Manager\Plugins\NKVM\DDM.exe";
            int exitCode = -1;
            string value = _commandLineInput.Options.Count > 0 ? _commandLineInput.Options[0].Option_Value : "N/A";
            string message = "N/A";

            if (!File.Exists(filePath))
            {
                value = "Not Supported";
                message = "NetworkKVM is not support";
            }
            else
            {
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = filePath,
                    Arguments = command.ToLower(),
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                using (Process process = new Process { StartInfo = startInfo })
                {
                    process.Start();
                    process.WaitForExit();
                    exitCode = process.ExitCode;
                }

                Trace.WriteLine($"{DateTime.Now} {command} (Exit code: {exitCode})");
            }

            WriteLog($"RunDDMCommand({command}) Exit, ExitCode: {exitCode}");
            return (exitCode, value, message);
        }

        private (int code, string result) NetworkKVMVersion()
        {
            WriteLog("NetworkKVMVersion Entry");
            if (_commandLineInput.Command != "GET" || _commandLineInput.Options.Count > 0)
            {
                return NotSupportResponse();
            }

            string output = string.Empty;
            bool retcode = false;

            var response = new NKVM_RESPONSE
            {
                Command = _commandLineInput.Command,
                TargetFeature = _commandLineInput.TargetFeature
            };

            string filePath = @"C:\Program Files\Dell\Dell Display and Peripheral Manager\Plugins\NKVM\DDM.exe";

            if (File.Exists(filePath))
            {
                FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(filePath);
                string version = fileVersionInfo.FileVersion;

                if (!string.IsNullOrWhiteSpace(version))
                {
                    response.Value = version;
                    response.Result = "PASS";
                    retcode = true;
                }
                else
                {
                    response.Result = "FAIL";
                    retcode = false;
                }
            }
            else
            {
                response.Result = "FAIL";
                response.Message = "NetworkKVM is not support";
                response.Value = "Not Supported";
            }

            Console.WriteLine(JsonConvert.SerializeObject(response, Formatting.Indented));
            output = JsonConvert.SerializeObject(response, Formatting.Indented);

            WriteLog("NetworkKVMVersion Exit");
            return (retcode ? (int)CLI_ExitCode.success : (int)CLI_ExitCode.functional_error, output);
        }

        private (int code, string result) EntryNetworkKVM(string command)
        {
            WriteLog($"EntryNetworkKVM({command}) Entry");
            var validOptions = new List<string> { "ON", "OFF" };

            if (command == "NETWORKKVM")
            {
                validOptions.AddRange(["ENABLE", "DISABLE"]);
            }

            string output = string.Empty;
            bool retcode = false;

            var response = new NKVM_RESPONSE
            {
                Command = _commandLineInput.Command,
                TargetFeature = _commandLineInput.TargetFeature
            };

            if (_commandLineInput.Command == "SET" && _commandLineInput.Options.Count > 0 && !string.IsNullOrWhiteSpace(_commandLineInput.Options[0].Option_Value))
            {
                response.Value = _commandLineInput.Options[0].Option_Value;

                if (!validOptions.Contains(_commandLineInput.Options[0].Option_Value.ToUpper()))
                {
                    retcode = false;
                    response.Result = "FAIL";
                    response.Message = "Invalid option value";
                }
                else
                {
                    var cmds = new List<string> { $"{command} {_commandLineInput.Options[0].Option_Value}", "exit" };
                    retcode = true;
                    response.Result = "PASS";

                    foreach (var cmd in cmds)
                    {
                        var commandResult = RunDDMCommand($"/{cmd}");

                        if (commandResult.exitCode != 0)
                        {
                            retcode = false;
                            response.Result = "FAIL";
                            response.Message = commandResult.message;
                            response.Value = commandResult.value;
                        }
                    }
                }
            }
            else if (_commandLineInput.Command == "GET" && _commandLineInput.Options.Count == 0)
            {
                var commandResult = RunDDMCommand($"/get {command}");

                if (command == "NETWORKKVM")
                {
                    switch (commandResult.exitCode)
                    {
                        case 0x0110:
                            retcode = true;
                            response.Result = "PASS";
                            response.Value = "ENABLE, ON";
                            break;

                        case 0x0010:
                            retcode = true;
                            response.Result = "PASS";
                            response.Value = "DISABLE, ON";
                            break;

                        case 0x0100:
                            retcode = true;
                            response.Result = "PASS";
                            response.Value = "ENABLE, OFF";
                            break;

                        case 0x0000:
                            retcode = true;
                            response.Result = "PASS";
                            response.Value = "DISABLE, OFF";
                            break;

                        default:
                            retcode = false;
                            response.Result = "FAIL";
                            response.Message = commandResult.message;
                            response.Value = commandResult.value;
                            break;
                    }
                }
                else
                {
                    switch (commandResult.exitCode)
                    {
                        case 0:
                            retcode = true;
                            response.Result = "PASS";
                            response.Value = "OFF";
                            break;

                        case 1:
                            retcode = true;
                            response.Result = "PASS";
                            response.Value = "ON";
                            break;

                        default:
                            retcode = false;
                            response.Result = "FAIL";
                            response.Message = commandResult.message;
                            response.Value = commandResult.value;
                            break;
                    }
                }
            }
            else
            {
                return NotSupportResponse();
            }

            Console.WriteLine(JsonConvert.SerializeObject(response, Formatting.Indented));
            output += "\n" + JsonConvert.SerializeObject(response, Formatting.Indented);

            WriteLog($"EntryNetworkKVM({command}) Exit");
            return (retcode ? (int)CLI_ExitCode.success : (int)CLI_ExitCode.functional_error, output);
        }

        private (int code, string result) EntryNetworkKVMPort(string command)
        {
            WriteLog($"EntryNetworkKVMPort({command}) Entry");
            string output = string.Empty;
            bool retcode = false;

            var response = new NKVM_RESPONSE
            {
                Command = _commandLineInput.Command,
                TargetFeature = _commandLineInput.TargetFeature
            };

            if (_commandLineInput.Command == "SET" && _commandLineInput.Options.Count > 0 && !string.IsNullOrWhiteSpace(_commandLineInput.Options[0].Option_Value))
            {
                response.Value = _commandLineInput.Options[0].Option_Value;

                if (int.TryParse(_commandLineInput.Options[0].Option_Value, out int port))
                {
                    var cmds = new List<string> { $"{command} {_commandLineInput.Options[0].Option_Value}", "exit" };
                    retcode = true;
                    response.Result = "PASS";

                    foreach (var cmd in cmds)
                    {
                        var commandResult = RunDDMCommand($"/{cmd}");

                        if (commandResult.exitCode != 0)
                        {
                            retcode = false;
                            response.Result = "FAIL";
                            response.Message = commandResult.message;
                            response.Value = commandResult.value;
                        }
                    }
                }
                else
                {
                    retcode = false;
                    response.Result = "FAIL";
                    response.Message = "Invalid option value";
                }
            }
            else if (_commandLineInput.Command == "GET" && _commandLineInput.Options.Count == 0)
            {
                command = $"/get {command}";
                var commandResult = RunDDMCommand(command);

                if (commandResult.exitCode == -1)
                {
                    retcode = false;
                    response.Result = "FAIL";
                    response.Message = commandResult.message;
                    response.Value = commandResult.value;
                }
                else
                {
                    retcode = true;
                    response.Result = "PASS";
                    response.Value = commandResult.exitCode.ToString();
                }
            }
            else
            {
                return NotSupportResponse();
            }

            Console.WriteLine(JsonConvert.SerializeObject(response, Formatting.Indented));
            output += "\n" + JsonConvert.SerializeObject(response, Formatting.Indented);

            WriteLog($"EntryNetworkKVMPort({command}) Exit");
            return (retcode ? (int)CLI_ExitCode.success : (int)CLI_ExitCode.functional_error, output);
        }

        private (int code, string result) NetworkKVMAccessReset(string command)
        {
            WriteLog($"NetworkKVMAccessReset({command}) Entry");
            string output = string.Empty;
            bool retcode = false;

            var response = new NKVM_RESPONSE
            {
                Command = _commandLineInput.Command,
                TargetFeature = _commandLineInput.TargetFeature
            };

            if (_commandLineInput.Command == "SET" && _commandLineInput.Options.Count == 0)
            {
                retcode = true;
                response.Result = "PASS";

                var commandResult = RunDDMCommand($"/{command}");

                if (commandResult.exitCode != 0)
                {
                    retcode = false;
                    response.Result = "FAIL";
                    response.Message = commandResult.message;
                    response.Value = commandResult.value;
                }
            }
            else
            {
                return NotSupportResponse();
            }

            Console.WriteLine(JsonConvert.SerializeObject(response, Formatting.Indented));
            output += "\n" + JsonConvert.SerializeObject(response, Formatting.Indented);

            WriteLog($"NetworkKVMAccessReset({command}) Exit");
            return (retcode ? (int)CLI_ExitCode.success : (int)CLI_ExitCode.functional_error, output);
        }
        #endregion

        #region Defer Implement
        // add @ 20241210 stephen: check defer with toast notification

        private List<DeferItem> DeferItems = new List<DeferItem>();
        private const int MAX_DEFER_WAIT_TIME_SEC = 300;
        private Dictionary<string, bool> deferResponse = new Dictionary<string, bool>();

        public void sendToastResult(string defer_id, bool isDefer)
        {
            WriteLog("@@ CLIManagerPlugin::sendToastResult defer_id = " + defer_id);
            WriteLog("@@ CLIManagerPlugin::sendToastResult isDefer = " + isDefer);
            if (!deferResponse.ContainsKey(defer_id))
            {
                deferResponse.Add(defer_id, isDefer);
            }
        }
        public Task<bool> checkDefer(int from, string guid, string commanddata)
        {

            DeferItem item = new DeferItem(from, guid, commanddata);
            string did = item.deferid;

            onCLIToastEventNotify(new CLIEventToastArgs()
            {
                defer_id = did,
                toast_message = commanddata,
                is_defer = true
            });

            bool result = checkToastResult(did, item);

            return Task.FromResult(result);
        }

        public Task<bool> checkDeferSchedule(int from, string guid, DeferItem item)
        {
            string did = item.deferid;

            onCLIToastEventNotify(new CLIEventToastArgs()
            {
                defer_id = did,
                toast_message = item.commanddata,
                is_defer = true
            });

            bool result = checkToastResult(did, item);

            return Task.FromResult(result);
        }

        private bool checkToastResult(string key, DeferItem item)
        {
            for (int i = 0; i <= MAX_DEFER_WAIT_TIME_SEC; i++)
            {
                Thread.Sleep(1000);
                // check defer response
                WriteLog("@@ CLIManagerPlugin::checkToastResult Sleep(1000)");
                WriteLog($"@@ CLIManagerPlugin::checkToastResult deferResponse.ContainsKey({key}) = " + deferResponse.ContainsKey(key));

                if (deferResponse.ContainsKey(key))
                {
                    WriteLog("@@ CLIManagerPlugin::checkToastResult deferResponse.ContainsKey " + key);
                    if (deferResponse[key])
                    {
                        WriteLog("@@ CLIManagerPlugin::checkToastResult deferResponse[did] =  " + deferResponse[key]);
                        deferResponse.Remove(key);
                        // add deferitem to deferControlPanel
                        DeferControlPanel.addToSchedule(item);
                        return true;
                    }
                    WriteLog("@@ CLIManagerPlugin::checkToastResult deferResponse[did]2 =  " + deferResponse[key]);
                    deferResponse.Remove(key);
                    break;
                }
            }

            return false;
        }

        public Task showNotification(int from, string guid, DeferItem item)
        {
            string did = item.deferid == string.Empty ? "NULL" : item.deferid;

            onCLIToastEventNotify(new CLIEventToastArgs()
            {
                defer_id = did,
                toast_message = item.commanddata,
                is_defer = false
            });

            return Task.CompletedTask;
        }
        /*
                public void showNotification(int from, string guid, string args)
                {
                    string did = item.deferid;

                    onCLIToastEventNotify(new CLIEventToastArgs()
                    {
                        defer_id = did,
                        toast_message = item.commanddata,
                        is_defer = false
                    });
                }*/

        // add @ 20241210 stephen
        public event EventHandler<CLIEventToastArgs> CLIToastEvent;

        /*        public void regEvent(EventHandler<EventArgs> e) {
                    eventToast += e;
                }*/


        private void onCLIToastEventNotify(CLIEventToastArgs e)
        {
            EventHandler<CLIEventToastArgs> Handler = CLIToastEvent;
            if (Handler != null)
            {
                WriteLog($"@@ CLIManagerPlugin::onCLIToastEventNotify CLIEventToastArgs e.defer_id = {e.defer_id}");
                WriteLog($"@@ CLIManagerPlugin::onCLIToastEventNotify CLIEventToastArgs e.toast_message = {e.toast_message}");
                Handler.Invoke(this, e);
            }
        }
        #endregion

        // add @ 20250116 stephen

        private int MAX_SECOND_WAIT_RESULT = 10;
        private bool resultDeviceConn = false;
        private bool resultReset = false;

        // add @ 20250116 stephen
        private bool checkDeviceConnResult(DeferItem item)
        {
            // true: device is connected
            //FwJobControlPanel.addToSchedule(item);

            for (int i = 0; i < MAX_SECOND_WAIT_RESULT; i++)
            {
                Thread.Sleep(1000);
                WriteLog($"@@  CLIManagerPlugin::checkDeviceConnResult wait = {i} ");
                if (resultReset)
                {
                    Thread.Sleep(1000);
                    if (!resultDeviceConn)
                    {
                        FwJobControlPanel.addToSchedule(item);
                    }
                    break;
                }
            }
            WriteLog($"@@  CLIManagerPlugin::checkDeviceConnResult resultReset = {resultReset}");
            WriteLog($"@@  CLIManagerPlugin::checkDeviceConnResult resultDeviceCheck = {resultDeviceConn}");

            return resultDeviceConn;

        }

        public Task<bool> checkDeviceConn(int from, string guid, string commanddata, string str_command)
        {
            DeferItem item = new DeferItem(from, guid, commanddata);

            initResultDeviceCheck();

            onCLIDeviceCheckEventNotify(new CLIEventDeviceConnArgs()
            {
                commands = str_command
            });

            bool result = checkDeviceConnResult(item);

            return Task.FromResult(result);
        }

        private void initResultDeviceCheck()
        {
            resultDeviceConn = false;
            resultReset = false;
        }

        public void sendDeviceCheckResult(bool result)
        {
            Console.Write("@@  CLIManagerPlugin::sendDeviceCheckResult result = " + result);

            resultDeviceConn = result;
            resultReset = true;
        }

        public event EventHandler<CLIEventDeviceConnArgs> CLIDeviceCheckEvent;

        private void onCLIDeviceCheckEventNotify(CLIEventDeviceConnArgs e)
        {
            EventHandler<CLIEventDeviceConnArgs> Handler = CLIDeviceCheckEvent;
            if (Handler != null)
            {
                WriteLog($"@@  CLIManagerPlugin::onCLIDeviceCheckEventNotify CLIEventDeviceConnArgs e.commands = {e.commands}");
                Handler.Invoke(this, e);
            }
        }

    }
}