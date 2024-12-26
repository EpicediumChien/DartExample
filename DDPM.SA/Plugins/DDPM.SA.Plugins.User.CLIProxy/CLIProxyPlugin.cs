#region LicenceHeader

//
// Copyright © 2024, Dell Inc., All Rights Reserved.
// This material is confidential and a trade secret.  Permission to use this
// work for any purpose must be obtained in writing from Dell Inc.
//
// CLIManagerPlugin.cs created on 24/07/2024T08:04 PM
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
using Microsoft.Toolkit.Uwp.Notifications;
using Newtonsoft.Json;
using static DDPM.SA.Common.ICLICommandTable;

namespace DDPM.SA.Plugin.User.CLIManager
{
    [Plugin(IDs.DDPM_CLI_Proxy_Plugin, pluginName, PluginOrderGroupType.Core, Version = pluginVersion)]
    [Descriptor(Description = pluginDescription)]
    [Publisher(Name = publisherCompany, Website = publisherWebsite, Support = publisherSupport)]
    public class CLIProxyPlugin : BaseAgentPlugin, IDisposableObservable, ICliProxy
    {
        public const string PluginLogId = "CLIProxy";

        #region Private Members

        private const string pluginName = "CLIProxyPlugin";
        private const string pluginVersion = "1.0.0";
        private const string pluginDescription = "This plugin implements CLI Proxy Plugin.";
        private const string publisherCompany = "Dell Inc.";
        private const string publisherWebsite = "https://www.dell.com";
        private const string publisherSupport = "This plugin implements CLI Proxy Plugin.";

        private IAgent _agent;

        private enum log_type
        {
            info = 0,
            error
        }

        private ICLIDisplay _CLIDisplay;
        private ICLIPeripherals _CLIPeripherals;
        private readonly object _pluginConditionLock_Display = new object();
        private readonly object _pluginConditionLock_Peripherals = new object();
        private PluginCondition _CLIDisplayPluginCondition;
        private PluginCondition _CLIPeripheralsPluginCondition;
        private const int TIMEOUT_IN_SECONDS = 60;

        private ICliManagerSA _CliManagerPlugin;
        private IDeviceManagerSA _DevManagerPlugin;
        private readonly object _PluginConditionLock_CliManager = new object();
        private readonly object _PluginConditionLock_DevManager = new object();
        private bool relay_registered = false;

        #endregion

        #region Constructor

        public CLIProxyPlugin(IAgent agent) : base(agent, PluginLogId)
        {
            _agent = agent;
        }

        #endregion

        #region Overriding methods

        protected override void OnPluginStarting()
        {
            _agent.PluginManager.PluginsStarted += PluginManagerOnPluginsStarted;

            PluginCondition = new PluginStartedCondition();
            WriteLog("Cli Proxy plugin report started");

            InitializeDevManagerPlugin();
            InitializeCLIDisplayPlugin();
            InitializeCLIPeripheralsPlugin();

            InitializeCliManagerPlugin();

            // add @ 20241215 stephen
            //initToastOnActivated();
            initOnActivated();
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

                    if (_DevManagerPlugin != null && _CliManagerPlugin != null)
                    {
                        _CliManagerPlugin.CLIActionEvent -= _CliManagerPlugin_CLIActionEvent;
                        relay_registered = false;
                    }
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

            if (e.ChangedPlugins.OfType<ICliProxy>().Any())
            {
                WriteLog("ICliProxy plugin started.");
            }

            if (e.ChangedPlugins.OfType<ICliManagerSA>().Any())
                InitializeCliManagerPlugin();

            if (e.ChangedPlugins.OfType<IDeviceManagerSA>().Any())
                InitializeDevManagerPlugin();

            if (e.ChangedPlugins.OfType<ICLIDisplay>().Any())
                InitializeCLIDisplayPlugin();

            if (e.ChangedPlugins.OfType<ICLIPeripherals>().Any())
                InitializeCLIPeripheralsPlugin();
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
            text = "[CLIProxyPlugin] " + text;
            Console.WriteLine(text);
            if (Log != null)
            {
                if (log_type == log_type.info)
                    Log.Info(text);
                else
                    Log.Error(text);
            }
        }

        private static void OutputLog(string output, CommandLineInput commandLineInput)
        {
            if (!string.IsNullOrEmpty(commandLineInput.LogPath))
            {
                if (!Directory.Exists(Path.GetDirectoryName(commandLineInput.LogPath)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(commandLineInput.LogPath));
                }
                using (StreamWriter sw = new StreamWriter(commandLineInput.LogPath, true))
                {
                    sw.WriteLine(DateTime.Now);
                    sw.WriteLine(output);
                }
            }
        }

        private void InitializeCliManagerPlugin()
        {
            if (_CliManagerPlugin != null)
                return;

            _CliManagerPlugin = _agent.PluginManager.FindPluginByType<ICliManagerSA>(PluginResolution.Dynamic);

            if (_CliManagerPlugin is IFrameworkPluginConditionNotification pluginCondition)
            {
                pluginCondition.PluginConditionChangeHandler += OnCliManagerPluginConditionChangeHandler;
                GetCurrentCliManagerPluginCondition();
            }
        }

        private void InitializeDevManagerPlugin()
        {
            if (_DevManagerPlugin != null)
                return;

            _DevManagerPlugin = _agent.PluginManager.FindPluginByType<IDeviceManagerSA>(PluginResolution.Dynamic);

            if (_DevManagerPlugin is IFrameworkPluginConditionNotification pluginCondition)
            {
                pluginCondition.PluginConditionChangeHandler += OnDevManagerPluginConditionChangeHandler;
                GetCurrentDevManagerPluginCondition();
            }
        }

        private void InitializeCLIDisplayPlugin()
        {
            if (_CLIDisplay != null)
                return;

            WriteLog($"{nameof(_CLIDisplay)} arrived for {nameof(ICLIDisplay)}");

            _CLIDisplay = _agent.PluginManager.FindPluginByType<ICLIDisplay>(PluginResolution.Dynamic);
            if (_CLIDisplay is IFrameworkPluginConditionNotification condition)
            {
                condition.PluginConditionChangeHandler += OnCLIDisplayPluginConditionChangeHandler;
                GetCurrentCLIDisplayPluginCondition();
            }
        }

        private void InitializeCLIPeripheralsPlugin()
        {
            if (_CLIPeripherals != null)
                return;

            WriteLog($"{nameof(_CLIPeripherals)} arrived for {nameof(ICLIPeripherals)}");

            _CLIPeripherals = _agent.PluginManager.FindPluginByType<ICLIPeripherals>(PluginResolution.Dynamic);
            if (_CLIPeripherals is IFrameworkPluginConditionNotification condition)
            {
                condition.PluginConditionChangeHandler += OnCLIPeripheralsPluginConditionChangeHandler;
                GetCurrentCLIPeripheralsPluginCondition();
            }
        }

        private void OnCliManagerPluginConditionChangeHandler(object sender, EventArgs e)
        {
            GetCurrentCliManagerPluginCondition();
        }

        private void OnDevManagerPluginConditionChangeHandler(object sender, EventArgs e)
        {
            GetCurrentDevManagerPluginCondition();
        }

        private void OnCLIDisplayPluginConditionChangeHandler(object sender, EventArgs e)
        {
            InitializeCLIDisplayPlugin();
        }

        private void OnCLIPeripheralsPluginConditionChangeHandler(object sender, EventArgs e)
        {
            InitializeCLIPeripheralsPlugin();
        }

        private void GetCurrentCliManagerPluginCondition()
        {
            _ = Task.Run(async () =>
            {
                var pluginCondition = await (_CliManagerPlugin as IFrameworkPluginConditionNotification)?.CurrentConditionAsync();

                lock (_PluginConditionLock_CliManager)
                {
                    if (pluginCondition is PluginErrorCondition)
                    {
                        WriteLog($"{nameof(GetCurrentCliManagerPluginCondition)} - CliManager Plugin is in an error condition");
                    }
                    else if (pluginCondition is PluginRunningCondition || pluginCondition is PluginStartedCondition)
                    {
                        WriteLog($"{nameof(GetCurrentCliManagerPluginCondition)} - CliManager Plugin is in a running/started condition");
                        if (!relay_registered && _DevManagerPlugin != null)
                        {
                            DoRelayRegister();
                        }
                    }
                }
            });
        }

        private void GetCurrentDevManagerPluginCondition()
        {
            _ = Task.Run(async () =>
            {
                var pluginCondition = await (_DevManagerPlugin as IFrameworkPluginConditionNotification)?.CurrentConditionAsync();

                lock (_PluginConditionLock_DevManager)
                {
                    if (pluginCondition is PluginErrorCondition)
                    {
                        WriteLog($"{nameof(GetCurrentDevManagerPluginCondition)} - DeviceManager Plugin is in an error condition");
                    }
                    else if (pluginCondition is PluginRunningCondition || pluginCondition is PluginStartedCondition)
                    {
                        WriteLog($"{nameof(GetCurrentDevManagerPluginCondition)} - DeviceManager Plugin is in a running/started condition");
                        if (!relay_registered && _CliManagerPlugin != null)
                        {
                            DoRelayRegister();
                        }
                    }
                }
            });
        }

        private void GetCurrentCLIDisplayPluginCondition()
        {
            _ = Task.Run(async () =>
            {
                var pluginCondition = await (_CLIDisplay as IFrameworkPluginConditionNotification)?.CurrentConditionAsync();

                lock (_pluginConditionLock_Display)
                {
                    _CLIDisplayPluginCondition = pluginCondition;

                    if (pluginCondition is PluginErrorCondition)
                    {
                        WriteLog($"{nameof(GetCurrentCLIDisplayPluginCondition)} - CLIDisplay Plugin is in an error condition");
                    }
                    else if (pluginCondition is PluginRunningCondition)
                    {
                        WriteLog($"{nameof(GetCurrentCLIDisplayPluginCondition)} - CLIDisplay Plugin is in running condition");
                        //_PluginAvailabilityTrigger_Display.Set();
                    }
                    else if (pluginCondition is PluginStartedCondition)
                    {
                        WriteLog($"{nameof(GetCurrentCLIDisplayPluginCondition)} - CLIDisplay Plugin is in started condition");
                        //_PluginAvailabilityTrigger_Display.Set();
                    }
                }
            });
        }

        private void GetCurrentCLIPeripheralsPluginCondition()
        {
            _ = Task.Run(async () =>
            {
                var pluginCondition = await (_CLIPeripherals as IFrameworkPluginConditionNotification)?.CurrentConditionAsync();

                lock (_pluginConditionLock_Peripherals)
                {
                    _CLIPeripheralsPluginCondition = pluginCondition;

                    if (pluginCondition is PluginErrorCondition)
                    {
                        WriteLog($"{nameof(GetCurrentCLIPeripheralsPluginCondition)} - CLIPeripherals Plugin is in an error condition");
                    }
                    else if (pluginCondition is PluginRunningCondition)
                    {
                        WriteLog($"{nameof(GetCurrentCLIPeripheralsPluginCondition)} - CLIPeripherals Plugin is in running condition");
                        //_PluginAvailabilityTrigger_Peripherals.Set();
                    }
                    else if (pluginCondition is PluginStartedCondition)
                    {
                        WriteLog($"{nameof(GetCurrentCLIPeripheralsPluginCondition)} - CLIPeripherals Plugin is in started condition");
                        //_PluginAvailabilityTrigger_Peripherals.Set();
                    }
                }
            });
        }

        private void DoRelayRegister()
        {
            if (_DevManagerPlugin == null || _CliManagerPlugin == null)
            {
                WriteLog("One of Device Manager and CLI Manager is null, do not register CLI relay");
                return;
            }
            _CliManagerPlugin.CLIActionEvent += _CliManagerPlugin_CLIActionEvent;

            // add @ 20241210 stephen
            _CliManagerPlugin.CLIToastEvent += _CliManagerPlugin_CLIToastEvent;

            relay_registered = true;
        }

        private async Task<string> RunAsyncTimeout(Task task)
        {
            try
            {
                Task completedTask = await Task.WhenAny(task, Task.Delay(TimeSpan.FromSeconds(5)));

                if (completedTask == task)
                {
                    await task;
                    return "0";
                }
                else
                {
                    return "1";
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        private void _CliManagerPlugin_CLIActionEvent(object? sender, CLIEventArgs e)
        {
            _ = Task.Run(() =>
            {
                if (_DevManagerPlugin == null)
                {
                    WriteLog("[Command line event] null Device Manager object!");
                    return;
                }

                if (e == null || e == EventArgs.Empty || e.commandLineInput == null)
                {
                    WriteLog("[Command line event] Got Empty CLIEventArgs!");
                    //_CliManagerPlugin.WriteCommandResult(Response_EmptyEventArg()); //no guid caused occupy memory
                    return;
                }

                //please edit upper table "peripheral_DeviceType" if new device supported
                List<string> peripheral_DeviceType = new List<string>()
                {
                    "MOUSE",
                    "KEYBOARD",
                    "AUDIO",
                    "PEN",
                    "DOCK",
                    "WEBCAM",
                };
                List<string> Display_Lock_WithoutAction = new List<string>()
                {
                    "INAPPBRICONT",
                    //"INAPPAUTOBRITEMP",
                    "INAPPAUTOBRIGHTNESSCOLOR",//1004 InAppAutoBrightnessColor DDPMW1341, same as INAPPAUTOBRITEMP
                    "INAPPNETWORKKVM",
                    "INAPPCOLORPRESET",
                    //"POWERNAP", //do not add powernap here, go throw normal process via CLI Display plugin as well
                };

                //Do command line action
                CLIEventResult? cliEventResult;
                CommandLineInput commandLineInput = e.commandLineInput;

                try
                {
                    if (commandLineInput.PluginsType.Equals("APP") && commandLineInput.TargetFeature.Equals("RESTOREFACTORYDEFAULTS"))
                    {
                        IDeviceManagerSA _devMgr = _DevManagerPlugin;
                        var _AllInfoMonitors = _devMgr.GetMonitors().Result;
                        List<DeviceInfo> _deviceinfo = _devMgr.GetDevices().Result.deviceInfo;
                        List<CLIEventResult> cliEventResults = new List<CLIEventResult>();
                        List<CLI_PeripheralRESPONSE> cliPeripheralEventResults = new List<CLI_PeripheralRESPONSE>();
                        var total_result = "";
                        if (_AllInfoMonitors.Count > 0)
                        {
                            total_result += "Display :";
                            List<string> stringList = new List<string>
                            {
                                "RESTOREFACTORYDEFAULTS",
                                "RESTORELEVELDEFAULTS",
                                "RESTORECOLORDEFAULTS"
                            };
                            foreach (string str in stringList)
                            {
                                e.commandLineInput.TargetFeature = str;
                                cliEventResult = _CLIDisplay.SetCommandArgs(e, _DevManagerPlugin);
                                total_result += $"\n{cliEventResult.serialize_Json_response}";
                                Thread.Sleep(5000);
                            }
                        }
                        _deviceinfo.ForEach(x =>
                        {
                            var result = "";
                            var msg = new CLI_PeripheralRESPONSE($"{x.ID}", commandLineInput.Command, commandLineInput.TargetFeature, "", "", x.Name, x.ModelNumber, x.DockServiceTag);
                            switch (x.LogicalDeviceType.ToUpper())
                            {
                                case "LOGICALHEADSET":
                                    result = RunAsyncTimeout(_devMgr.SetFactoryResetAsyncValueForHeadset(x.ID.ToString(), true)).Result;
                                    break;
                                case "LOGICALWIREDAUDIO":
                                    result = RunAsyncTimeout(_devMgr.SetResetToDefaultAsyncForSoundbar(x.ID.ToString(), true)).Result;
                                    break;
                                case "LOGICALWEBCAM":
                                    result = RunAsyncTimeout(_devMgr.ResetToDefault_webcam(x.ID.ToString(), true)).Result;
                                    break;
                                case "LOGICALKEYBOARD":
                                    result = RunAsyncTimeout(_devMgr.RestoreToDefaultKB(x.ID.ToString())).Result;
                                    break;
                                case "LOGICALMOUSE":
                                    result = RunAsyncTimeout(_devMgr.RestoreToDefaultMouse(x.ID.ToString())).Result;
                                    break;
                                case "LOGICALPEN":
                                    result = RunAsyncTimeout(_devMgr.RestoreToDefaultPen()).Result;
                                    break;
                                default:
                                    break;
                            }
                            if (result == "0")
                            {
                                msg.Result = "PASS";
                                //retcode_ = _devMgr.GetIsAutoFramingOn(ItemId).Result;
                                msg.Value = "SUCCESS";
                                //x.Value += "," + (data.LockSettings.Lock_Audio_RestoreFactoryDefaults ? "LOCK" : "UNLOCK");
                                msg.Message = "N/A";
                            }
                            else if (result == "1")
                            {
                                msg.Result = "FAIL";
                                msg.Message = "Timeout";
                            }
                            else
                            {
                                msg.Result = "FAIL";
                                msg.Message = result;
                            }
                            cliPeripheralEventResults.Add(msg);
                            Thread.Sleep(5000);
                        });
                        total_result += $"\n{JsonConvert.SerializeObject(cliPeripheralEventResults, Formatting.Indented)}";

                        #region Set Telemetry Consent to Default(False)
                        DDPMSettings data = _DevManagerPlugin.ReloadAppConfigData().Result;
                        commandLineInput.TargetFeature = "TELEMETRYCONSENT";
                        commandLineInput.Options = new List<CommandType_Option> { new CommandType_Option("VALUE", "FALSE") };
                        cliEventResult = CLIHandlerApp.CLI_Analytics_Consent(Log, data, _DevManagerPlugin, commandLineInput, e.command_guid_string);
                        total_result += $"\n{cliEventResult.serialize_Json_response}";
                        #endregion

                        #region Set ScreenNotification to Default(False)
                        e.commandLineInput.TargetFeature = "SCREENNOTIFICATION";
                        e.commandLineInput.Options = new List<CommandType_Option> { new CommandType_Option("VALUE", "ON") };
                        cliEventResult = _CLIDisplay.SetCommandArgs(e, _DevManagerPlugin);
                        total_result += $"\n{cliEventResult.serialize_Json_response}";
                        #endregion

                        var result = new CLIEventResult()
                        {
                            command_guid_string = e.command_guid_string,
                            serialize_Json_response = total_result,
                            ExitCode = (int)CLI_ExitCode.success,
                            ticket = DateTime.Now
                        };
                        _CliManagerPlugin.WriteCommandResult(result);
                        return;
                    }

                    if (commandLineInput.PluginsType.Equals("DISPLAY"))
                    {
                        if (_CLIDisplay != null)
                        {
                            if (Display_Lock_WithoutAction.FindIndex(x => x.Equals(commandLineInput.TargetFeature)) >= 0)
                            {
                                DDPMSettings data_inappdisplaylock = _DevManagerPlugin.ReloadAppConfigData().Result;
                                cliEventResult = CLIHandlerDisplay.CLI_Display_LockUnlock(Log, data_inappdisplaylock, _DevManagerPlugin, commandLineInput, e.command_guid_string);
                            }
                            else if (commandLineInput.TargetFeature == "INAPPEXPORTIMPORT")
                            {
                                WriteLog($"INAPPEXPORTIMPORT entry");
                                DDPMSettings data_exportsettings = _DevManagerPlugin.ReloadAppConfigData().Result;
                                cliEventResult = CLIHandlerDisplay.CLI_Display_LockUnlock(Log, data_exportsettings, _DevManagerPlugin, commandLineInput, e.command_guid_string);
                                WriteLog($"INAPPEXPORTIMPORT exit");
                            }
                            else
                                cliEventResult = _CLIDisplay.SetCommandArgs(e, _DevManagerPlugin);
                        }
                        else
                        {
                            WriteLog($"{nameof(ICLIDisplay)} was missing.");
                            _CliManagerPlugin.WriteCommandResult(Response_PluginNotReady(commandLineInput, nameof(ICLIDisplay), e.command_guid_string));
                            return;
                        }
                    }
                    //else if (commandLineInput.PluginsType.Equals("AUDIO") || commandLineInput.PluginsType.Equals("MOUSE") || commandLineInput.PluginsType.Equals("KEYBOARD") || commandLineInput.PluginsType.Equals("DOCK") || commandLineInput.PluginsType.Equals("HEADSET"))
                    else if (peripheral_DeviceType.FindIndex(x => x.Equals(commandLineInput.PluginsType)) >= 0)
                    {
                        if (_CLIPeripherals != null)
                        {
                            if (commandLineInput.TargetFeature.Equals("RESTOREFACTORYDEFAULTS") && commandLineInput.Options.Count == 0)
                            {
                                cliEventResult = _CLIPeripherals.SetCommandArgs(e, _DevManagerPlugin);
                            }
                            else if (commandLineInput.TargetFeature.Equals("RESTOREFACTORYDEFAULTS"))
                            {
                                DDPMSettings data_restorefactorydefault = _DevManagerPlugin.ReloadAppConfigData().Result;
                                cliEventResult = CLIHandlerPeripheral.CLI_Peripheral_RestoreFactoryDefault(Log, data_restorefactorydefault, _DevManagerPlugin, commandLineInput, e.command_guid_string);
                            }
                            else
                            {
                                cliEventResult = _CLIPeripherals.SetCommandArgs(e, _DevManagerPlugin);
                            }
                        }
                        else
                        {
                            WriteLog($"{nameof(ICLIPeripherals)} was missing.");
                            _CliManagerPlugin.WriteCommandResult(Response_PluginNotReady(commandLineInput, nameof(ICLIPeripherals), e.command_guid_string));
                            return;
                        }
                    }
                    else if (commandLineInput.PluginsType.Equals("APP"))
                    {
                        switch (commandLineInput.TargetFeature)
                        {
                            case "TELEMETRYCONSENT":
                                //cliEventResult = CLI_Analytics_Consent(commandLineInput, e.command_guid_string);
                                DDPMSettings data = _DevManagerPlugin.ReloadAppConfigData().Result;
                                cliEventResult = CLIHandlerApp.CLI_Analytics_Consent(Log, data, _DevManagerPlugin, commandLineInput, e.command_guid_string);
                                if (cliEventResult.ExitCode == (int)CLI_ExitCode.success)
                                {
                                    UpdateUINotify no = new UpdateUINotify();
                                    no.UI_Field_Name = "TELEMETRYCONSENT";
                                    _DevManagerPlugin.OnUIUpdateNotify(no);
                                }
                                break;

                            case "INAPPUPDATE":
                                //cliEventResult = CLI_Analytics_Consent(commandLineInput, e.command_guid_string);
                                DDPMSettings data_update = _DevManagerPlugin.ReloadAppConfigData().Result;
                                cliEventResult = CLIHandlerApp.CLI_App_LockUnlock(Log, data_update, _DevManagerPlugin, commandLineInput, e.command_guid_string);
                                //if (cliEventResult.ExitCode == (int)CLI_ExitCode.success)
                                //{
                                //UpdateUINotify no = new UpdateUINotify();
                                //no.UI_Field_Name = "INAPPUPDATE";
                                //_DevManagerPlugin.OnUIUpdateNotify(no);
                                //}
                                break;
                            //case "INAPPEXPORTIMPORT":
                            //    //cliEventResult = CLI_Analytics_Consent(commandLineInput, e.command_guid_string);
                            //    DDPMSettings data_exportsettings = _DevManagerPlugin.ReloadAppConfigData().Result;
                            //    cliEventResult = CLIHandlerDisplay.CLI_Display_LockUnlock(Log, data_exportsettings, _DevManagerPlugin, commandLineInput, e.command_guid_string);
                            //    break;

                            case "INAPPRESTOREDEFAULTS":
                                //cliEventResult = CLI_Analytics_Consent(commandLineInput, e.command_guid_string);
                                DDPMSettings data_restoredefaults = _DevManagerPlugin.ReloadAppConfigData().Result;
                                cliEventResult = CLIHandlerApp.CLI_App_LockUnlock(Log, data_restoredefaults, _DevManagerPlugin, commandLineInput, e.command_guid_string);
                                break;
                            case "DEVICEDATA":
                            case "DEVICECONFIGURATION":
                            case "CONNECTEDDEVICES":
                            case "SCREENNOTIFICATION":
                            case "DIAGNOSTICSREPORT":
                                //cliEventResult = CLI_Analytics_Consent(commandLineInput, e.command_guid_string);
                                cliEventResult = _CLIDisplay.SetCommandArgs(e, _DevManagerPlugin);
                                break;
                            case "FIRMWAREUPDATE":
                                cliEventResult = _CLIPeripherals.SetCommandArgs(e, _DevManagerPlugin);

                                // add @ stephen
                                //DDPMSettings data_fwupdate = _DevManagerPlugin.ReloadAppConfigData().Result;
                                //cliEventResult = CLIHandlerApp.CLI_FW_Update(Log, data_fwupdate, _DevManagerPlugin, commandLineInput, e.command_guid_string);
                                break;
                            case "DISABLECA":
                                cliEventResult = CLIHandlerApp.CLI_Common_DisableCA(Log, _DevManagerPlugin, commandLineInput, e.command_guid_string);
                                break;
                            case "UPDATE":
                                cliEventResult = _CLIPeripherals.SetCommandArgs(e, _DevManagerPlugin);
                                break;
                            case "UPDATESOURCELOCATION":
                                cliEventResult = _CLIPeripherals.SetCommandArgs(e, _DevManagerPlugin);
                                break;
                            default:
                                _CliManagerPlugin.WriteCommandResult(Response_TargetFeatureNotSupport(commandLineInput, e.command_guid_string));
                                return;
                        }
                    }
                    else if (commandLineInput.Command.Equals("HELP"))
                    {
                        CLIEventResult result;
                        IIC_Metadata iIC_Metadata = new IIC_Metadata();
                        if (commandLineInput.TargetFeature.ToUpper() == "DISPLAY")
                        {
                            var monitorInfos = _DevManagerPlugin.GetMonitors().Result;
                            string output = string.Empty;
                            if (monitorInfos == null || monitorInfos.Count == 0)
                            {
                                CLI_RESPONSE cLI_RESPONSE = new CLI_RESPONSE()
                                {
                                    Model = "N/A",
                                    SerialNumber = "N/A",
                                    Command = "N/A",
                                    TargetFeature = "N/A",
                                    Result = "no monitor connected",
                                    Index = "N/A",
                                    ServiceTag = "N/A",
                                    Value = "N/A",
                                    Message = "no monitor connected"
                                };
                                result = new CLIEventResult()
                                {
                                    command_guid_string = e.command_guid_string,
                                    serialize_Json_response = JsonConvert.SerializeObject(cLI_RESPONSE, Formatting.Indented),
                                    ExitCode = (int)CLI_ExitCode.no_monitor_connected,
                                    ticket = DateTime.Now
                                };
                                _CliManagerPlugin.WriteCommandResult(result);
                                return;
                            }
                            foreach (var monitorInfo in monitorInfos)
                            {
                                iIC_Metadata = _DevManagerPlugin.DownloadICCData(monitorInfo).Result;
                                var results = ICLICommandTable.Response_HelpCommand_ByDisplay(commandLineInput.TargetFeature, monitorInfo.CapabilityDic, iIC_Metadata.Is_Support_ICC_DeviceName);
                                output += "\n" + results;
                            }
                            result = new CLIEventResult()
                            {
                                command_guid_string = e.command_guid_string,
                                serialize_Json_response = output,
                                ExitCode = (int)CLI_ExitCode.success,
                                ticket = DateTime.Now
                            };
                            _CliManagerPlugin.WriteCommandResult(result);
                            return;
                        }
                        else
                        {
                            WriteLog($"{nameof(ICLIDisplay)} was missing.");
                            _CliManagerPlugin.WriteCommandResult(Response_PluginNotReady(commandLineInput, nameof(ICLIDisplay), e.command_guid_string));
                            return;
                        }
                    }
                    else
                    {
                        CLIEventResult result = new CLIEventResult()
                        {
                            command_guid_string = e.command_guid_string,
                            serialize_Json_response = Response_TargetTypeNotSupport(commandLineInput, commandLineInput.PluginsType),
                            ExitCode = (int)CLI_ExitCode.command_targettype_not_support,
                            ticket = DateTime.Now
                        };
                        _CliManagerPlugin.WriteCommandResult(result);
                        return;
                    }
                }
                catch (Exception ex)
                {
                    WriteLog($"[Command line event] Error: {ex.Message}");
                    cliEventResult = new CLIEventResult()
                    {
                        command_guid_string = e.command_guid_string,
                        serialize_Json_response = JsonConvert.SerializeObject(new APP_RESPONSE()
                        {
                            Command = commandLineInput.Command,
                            TargetFeature = commandLineInput.TargetFeature,
                            Result = "FAIL",
                            Message = $"Functional error ({ex.Message})"
                        }, Formatting.Indented),
                        ExitCode = (int)CLI_ExitCode.functional_error,
                        ticket = DateTime.Now
                    };
                }

                //write result back
                if (cliEventResult != null)
                {
                    _CliManagerPlugin.WriteCommandResult(cliEventResult);
                    OutputLog(cliEventResult.serialize_Json_response, commandLineInput);
                }
            });
        }
        #endregion

        #region ICLIProxy implementation

        //no action need in this plugin

        #endregion

        #region Defer implement

        // add @ 20241210 stephen
        //private void OnEventToast(object sender, EventArgs e) { }
        private void _CliManagerPlugin_CLIToastEvent(object? sender, CLIEventToastArgs e)
        {
            //throw new NotImplementedException();
            //Console.WriteLine($"value = {CLIEventToastArgs.toast_message}");
            if (e.is_defer)
            {
                showToast(e.defer_id, e.toast_message);
            }
            else
            {
                showNotification(e.defer_id, e.toast_message);
            }


        }

        private void initOnActivated()
        {

            ToastNotificationManagerCompat.OnActivated += toastArgs =>
            {
                ToastArguments args = ToastArguments.Parse(toastArgs.Argument);

                bool isDefer = false;
                string derferid = string.Empty;

                if (args.Contains("deferid"))
                {
                    Console.WriteLine("@@@stephen args[\"deferid\"] = " + args["deferid"]);
                    derferid = args["deferid"];
                }

                if (args.Contains("action"))
                {
                    if (args["action"] == "defer")
                    {
                        Console.WriteLine("@@@stephen isDefer = true");
                        _CliManagerPlugin.sendToastResult(derferid, true);
                    }
                    else
                    {
                        Console.WriteLine("@@@stephen isDefer = false");
                        _CliManagerPlugin.sendToastResult(derferid, false);
                    }
                }
            };
        }

        private void initToastOnActivated()
        {
            ToastNotificationManagerCompat.OnActivated -= toastDeferArgs => { };
            ToastNotificationManagerCompat.OnActivated += toastDeferArgs =>
            {
                // Obtain the arguments from the notification
                ToastArguments args = ToastArguments.Parse(toastDeferArgs.Argument);
                bool isDefer = false;
                long derferid = -1;



            };
        }

        public void showNotification(string id, string msg)
        {

            new ToastContentBuilder()
                .AddArgument("deferid", id)
                .AddText(msg)
                .AddButton(new ToastButton()
                    .SetContent("OK " + id)
                //.AddArgument("action", "OK")
                )
                .Show();
        }


        public void showToast(string id, string msg)
        {

            new ToastContentBuilder()
                .AddArgument("deferid", id)
                .AddText(msg)
                .AddButton(new ToastButton()
                    .SetContent("Defer " + id)
                    .AddArgument("action", "defer")
                )
                .AddButton(new ToastButton()
                    .SetContent("Run Now")
                    .AddArgument("action", "runnow")
                )
                .Show();

        }

        #endregion
    }
}