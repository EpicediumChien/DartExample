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
using System.Diagnostics;
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
                    break;
                case "INAPPAUTOBRITEMP":         //InAppAutoBriTemp          DDPMW-1341
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
                    else if(commandLineInput.PluginsType.Equals("DISPLAY"))
                        rst = CLIHandlerDisplay.CLI_Display_RestoreFactoryDefault(Log, data, _SettingsPluginIT, commandLineInput, command_guid);
                    else
                        rst = CLIHandlerPeripheral.CLI_Response_TypeNotSupport(commandLineInput, rst);
                    return rst;
                    //break;
                case "SCREENNOTIFICATION":       //ScreenNotification        DDPMW-1901
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
                        rst = CLIHandlerDisplay.CLI_Display_LockUnlock(Log, data, _SettingsPluginIT, commandLineInput, command_guid);
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
                case "INAPPEXPORTSETTINGS":      //InAppExportSettings       DDPMW-1335
                    if (commandLineInput.PluginsType.Equals("APP"))
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
        #endregion
    }
}