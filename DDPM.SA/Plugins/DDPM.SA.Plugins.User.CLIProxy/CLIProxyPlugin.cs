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
using DDPM.SA.Common.Defer;
using DDPM.SA.Common.Settings;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.Common.Annotations;
using Dell.Client.Framework.Common.PluginConditions;
using Dell.Client.Framework.Interfaces;
using Microsoft;
using Microsoft.Toolkit.Uwp.Notifications;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
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
        private const string publisherCompany = "Dell Technologies";
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

        //2025/4/24 mark this function as [Obsolete] due to checkmarx report the line sw.WriteLine() cause issue "Information Exposure Through an Error Message"
        [Obsolete]
        private static void OutputLog(string output, CommandLineInput commandLineInput)
        {
            if (!string.IsNullOrEmpty(commandLineInput.LogPath))
            {
                if (!Directory.Exists(Path.GetDirectoryName(commandLineInput.LogPath)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(commandLineInput.LogPath));
                }

                try
                {
                    using (StreamWriter sw = new StreamWriter(commandLineInput.LogPath, true))
                    {
                        sw.WriteLine(DateTime.Now);
                        sw.WriteLine(output);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[CLIProxyPlugin] OutputLog failed, Message: {ex.Message}");
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

            // add @ 20250116 stephen
            _CliManagerPlugin.CLIDeviceCheckEvent += _CliManagerPlugin_CLIDeviceCheckEvent;

            relay_registered = true;

            if (relay_registered)
            {
                _CliManagerPlugin.SendNKVMCommand();
            }
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
                    WriteLog($"command ID: {e.command_guid_string}, command target type: {e.commandLineInput.TargetType}, command target feature: {e.commandLineInput.TargetFeature}, fromCMA: {e.commandLineInput.fromcma}");

                    if (commandLineInput.PluginsType.Equals("APP") && commandLineInput.TargetFeature.Equals("RESTOREFACTORYDEFAULTS"))
                    {
                        IDeviceManagerSA _devMgr = _DevManagerPlugin;
                        var _AllInfoMonitors = _devMgr.GetMonitors().Result;
                        List<DeviceInfo> _deviceinfo = _devMgr.GetDevices().Result.deviceInfo;
                        //List<CLIEventResult> cliEventResults = new List<CLIEventResult>();
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
                                Task.Delay(5000).Wait();
                            }
                        }
                        _deviceinfo.ForEach(x =>
                        {
                            var result = "";
                            var msg = new CLI_PeripheralRESPONSE($"{x.ID}", commandLineInput.Command, commandLineInput.TargetFeature, "", "", x.Name, x.ModelNumber, x.DockServiceTag);
                            switch (x.LogicalDeviceType.ToUpper())
                            {
                                case "LOGICALHEADSET":
                                    result = RunAsyncTimeout(_devMgr.SetFactoryResetAsyncValueForHeadsetForCLI(x.ID.ToString(), true)).Result;
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
                            Task.Delay(5000).Wait();
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
                        if (commandLineInput.Command == "SET" && commandLineInput.TargetFeature == "NETWORKKVM" && commandLineInput.Options.Count > 0 && commandLineInput.Options[0].Option_Value == "ON")
                        {
                            WriteLog("_DevManagerPlugin.CreatNewNamedpipe() entry");
                            _DevManagerPlugin.CreatNewNamedpipe();
                            WriteLog("_DevManagerPlugin.CreatNewNamedpipe() exit");
                            _CliManagerPlugin.WriteCommandResult(Response_NKVMOn(commandLineInput, e.command_guid_string));
                            return;
                        }

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
                                if (e.commandLineInput.TargetFeature.ToUpper().Equals("CONNECTEDDEVICES"))
                                    WriteLog($"Execute ConnectedDevices");
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
                    //else if (commandLineInput.Command.Equals("HELP"))
                    //{
                    //    CLIEventResult result;
                    //    IIC_Metadata iIC_Metadata = new IIC_Metadata();
                    //    if (commandLineInput.TargetFeature.ToUpper() == "DISPLAY")
                    //    {
                    //        var monitorInfos = _DevManagerPlugin.GetMonitors().Result;
                    //        string output = string.Empty;
                    //        if (monitorInfos == null || monitorInfos.Count == 0)
                    //        {
                    //            CLI_RESPONSE cLI_RESPONSE = new CLI_RESPONSE()
                    //            {
                    //                Model = "N/A",
                    //                SerialNumber = "N/A",
                    //                Command = "N/A",
                    //                TargetFeature = "N/A",
                    //                Result = "no monitor connected",
                    //                Index = "N/A",
                    //                ServiceTag = "N/A",
                    //                Value = "N/A",
                    //                Message = "no monitor connected"
                    //            };
                    //            result = new CLIEventResult()
                    //            {
                    //                command_guid_string = e.command_guid_string,
                    //                serialize_Json_response = JsonConvert.SerializeObject(cLI_RESPONSE, Formatting.Indented),
                    //                ExitCode = (int)CLI_ExitCode.no_monitor_connected,
                    //                ticket = DateTime.Now
                    //            };
                    //            _CliManagerPlugin.WriteCommandResult(result);
                    //            return;
                    //        }
                    //        foreach (var monitorInfo in monitorInfos)
                    //        {
                    //            iIC_Metadata = _DevManagerPlugin.DownloadICCData(monitorInfo).Result;
                    //            var results = ICLICommandTable.Response_HelpCommand_ByDisplay(commandLineInput.TargetFeature, monitorInfo.CapabilityDic, iIC_Metadata.Is_Support_ICC_DeviceName);
                    //            output += "\n" + results;
                    //        }
                    //        result = new CLIEventResult()
                    //        {
                    //            command_guid_string = e.command_guid_string,
                    //            serialize_Json_response = output,
                    //            ExitCode = (int)CLI_ExitCode.success,
                    //            ticket = DateTime.Now
                    //        };
                    //        _CliManagerPlugin.WriteCommandResult(result);
                    //        return;
                    //    }
                    //    else
                    //    {
                    //        WriteLog($"{nameof(ICLIDisplay)} was missing.");
                    //        _CliManagerPlugin.WriteCommandResult(Response_PluginNotReady(commandLineInput, nameof(ICLIDisplay), e.command_guid_string));
                    //        return;
                    //    }
                    //}
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
                            Message = $"Functional error ({ex.GetType().ToString()})"//Dean 20250411 fix SDL issue: Information Exposure Through an Error Message
                        }, Formatting.Indented),
                        ExitCode = (int)CLI_ExitCode.functional_error,
                        ticket = DateTime.Now
                    };
                }

                //write result back
                if (cliEventResult != null)
                {
                    WriteLog($"[commandLineInput]: {JToken.FromObject(commandLineInput).ToString()}");
                    WriteLog($"[Response]: {cliEventResult.serialize_Json_response}");
                    _CliManagerPlugin.WriteCommandResult(cliEventResult);
                    //OutputLog(cliEventResult.serialize_Json_response, commandLineInput); //This function may cause message with exception content to streambuilder, replace it by ILog
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
            var header = string.Empty;
            bool is_model = false;
            WriteLog($"commandLine: {e.defer_item.commanddata}");

            string command_data = string.IsNullOrEmpty(e.cli_command_format) ? e.defer_item.commanddata : e.cli_command_format;
            WriteLog($"is from CMA:({string.IsNullOrEmpty(e.cli_command_format)}), command:({command_data})");

            //if (e.defer_item.commanddata.Contains("app=firmwareupdate", StringComparison.OrdinalIgnoreCase) || e.defer_item.commanddata.Contains("dock=fwupdate", StringComparison.OrdinalIgnoreCase))
            if (command_data.Contains("app=firmwareupdate", StringComparison.OrdinalIgnoreCase) ||
               command_data.Contains("dock=fwupdate", StringComparison.OrdinalIgnoreCase))
            {
                WriteLog($"Firmware Update");
                var deviceType = command_data.ToLower()// e.defer_item.commanddata.ToLower()
                                                         .Split()
                                                         .FirstOrDefault(_ => _.Contains("value"));
                
                var devicemodel = command_data.ToLower()//e.defer_item.commanddata.ToLower()
                                                         .Split()
                                                         .FirstOrDefault(_ => _.Contains("model"));
                var deviceName = "[Device Marketing Name with Model in parenthesis]";

                if (!string.IsNullOrWhiteSpace(deviceType))
                {
                    deviceType = deviceType.Split('=')[1].Split(',')[0];

                    if (deviceType.Equals("display"))
                    {
                        deviceName = _DevManagerPlugin.GetMonitors().Result.FirstOrDefault()?.modelName ?? deviceType;
                    }
                    else
                    {
                        deviceName = _DevManagerPlugin.GetDevices().Result?.deviceInfo.FirstOrDefault(_ => _.LogicalDeviceType.Contains(deviceType, StringComparison.OrdinalIgnoreCase))?.ModelNumber ?? deviceType;
                    }
                }

                if (!string.IsNullOrWhiteSpace(devicemodel))
                {
                    devicemodel = devicemodel.Split('=')[1].Split(',')[0];

                    if (deviceType.Equals("display"))
                    {
                        devicemodel = _DevManagerPlugin.GetMonitors().Result.FirstOrDefault()?.modelName ?? devicemodel;
                    }
                    else
                    {
                        devicemodel = _DevManagerPlugin.GetDevices().Result?.deviceInfo.FirstOrDefault(_ => _.LogicalDeviceType.Contains(devicemodel, StringComparison.OrdinalIgnoreCase))?.ModelNumber ?? devicemodel;
                    }
                    is_model = true;
                }

                var delldevicetype = getdevicetype(deviceName);
                WriteLog($"device Type: {deviceType}, device model: {devicemodel} device name: {deviceName} dell device name: {delldevicetype}");

                if (is_model && deviceType.Equals("display"))
                    header = e.is_defer ? $"Dell Display ({devicemodel.ToUpper()}) firmware update" : "Update will be applied";
                else if (is_model && !deviceType.Equals("display"))
                    header = e.is_defer ? $"{delldevicetype} ({devicemodel.ToUpper()}) firmware update" : "Update will be applied";
                else
                    header = e.is_defer ? $"{delldevicetype} ({deviceName}) firmware update" : "Update will be applied";

                e.toast_message = e.is_defer ? $"During update, device usage may be intermittent. Do not disconnect the device. This update can be deferred {e.defer_item.count + 1} times." : $"There is a required firmware update for {deviceName}. During update, device may be intermittently available. Do not disconnect the device during the update.";
            }
            else if (e.toast_message.Contains("app=update", StringComparison.OrdinalIgnoreCase))
            {
                WriteLog($"SW Update");
                header = e.is_defer ? "Update available" : "Update will be applied";
                e.toast_message = e.is_defer ? $"Dell Display and Peripheral Manager has a pending update. This update can be deferred {e.defer_item.count + 1} times before it is required." : "There is a required software update for Dell Display and Peripheral Manager.";
            }
            //else if (e.defer_item.commanddata.Contains("app=firmwareupdate", StringComparison.OrdinalIgnoreCase))
            //{
            //    header = e.is_defer ? "Pending Changes to settings" : "Changes to settings will be applied";
            //    e.toast_message = e.is_defer ? $"Settings are being configured for your Dell Display(s) and/or Peripheral(s) by your system administrator.\nThe configuration can be deferred {e.defer_item.count + 1} times before it is required." : "Settings are being configured for your Dell Display(s) and/or Peripheral(s) by your system administrator.";
            //}
            else
            {
                header = e.is_defer ? "Pending Changes to settings" : "Changes to settings will be applied";
                e.toast_message = e.is_defer ? $"Settings are being configured for your Dell devices by your system administrator. \nThis notice can be deferred {e.defer_item.count + 1} times." : "Settings are being configured for your Dell devices by your system administrator.";
            }
            //throw new NotImplementedException();
            //Console.WriteLine($"value = {CLIEventToastArgs.toast_message}");
#if DEBUG
            Console.WriteLine($"e.is_defer:({e.is_defer}), e.defer_item.commanddata:({e.defer_item.commanddata}), message:({e.toast_message})");
#endif
            bool contain_fwupdate = e.defer_item.commanddata.Contains("app=firmwareupdate", StringComparison.OrdinalIgnoreCase) || e.cli_command_format.Contains("app=firmwareupdate", StringComparison.OrdinalIgnoreCase);
            if (e.is_defer && contain_fwupdate)
            {              
                FWshowToast(e.defer_id, header, e.toast_message);
            }
            else if (e.is_defer)
            {
                showToast(e.defer_id, header, e.toast_message);
            }
            else
            {
                showNotification(e.defer_id, header, e.toast_message);
            }


        }

        private string getdevicetype(string devicetype)
        {
            if (devicetype.Contains("WB", StringComparison.OrdinalIgnoreCase))
            {
                devicetype = "Dell Webcam";
            }
            else if (devicetype.Contains("WL", StringComparison.OrdinalIgnoreCase))
            {
                devicetype = "Dell Headset";
            }
            else if (devicetype.Contains("SP", StringComparison.OrdinalIgnoreCase))
            {
                devicetype = "Dell Soundbar";
            }
            else if (devicetype.Contains("MS", StringComparison.OrdinalIgnoreCase))
            {
                devicetype = "Dell Mouse";
            }
            else if (devicetype.Contains("KB", StringComparison.OrdinalIgnoreCase))
            {
                devicetype = "Dell Keyboard";
            }
            else
            {
                devicetype = "Dell Display";
            }
            return devicetype;
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
                    Console.WriteLine("@@ args[\"deferid\"] = " + args["deferid"]);
                    derferid = args["deferid"];
                }

                if (args.Contains("action"))
                {
                    if (args["action"] == "defer")
                    {
                        Console.WriteLine("@@ isDefer = true");
                        WriteLog($"[Defer selection] : Defer");
                        _CliManagerPlugin.sendToastResult(derferid, true);
                    }
                    else if (args["action"] == "runnow")
                    {
                        Console.WriteLine("@@ isDefer = false");
                        WriteLog($"[Defer selection] : Update Now");
                        _CliManagerPlugin.sendToastResult(derferid, false);
                    }
                    else
                    {
                        Console.WriteLine("@@ isDefer = false");
                        WriteLog($"[ForceWithNotice selection] : ForceWithNotice");
                        _CliManagerPlugin.sendToastResult(derferid, true);
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

        /*
	Header - Update will be applied
	Body - There is a required firmware update for [Device Marketing Name with Model in parenthesis]. During update, device may be intermittently available. Do not disconnect the device during the update.
	Button Option - “Ok”
*/


        /*
        	Header - Update available
        	Body - [Device Marketing Name with Model in parenthesis] has a pending firmware update. During update, device may be intermittently available. Do not disconnect the device during the update. This update can be deferred [x] times before it is required.
        	Button Options - “Update now” / “Defer”
        */

        private const string NOTIFICATION_MSG_HEADER = @"Update available";
        private const string NOTIFICATION_MSG_BODY = @"There is a required update for Device. During update, device may be intermittently available. Do not disconnect the device during the update.";

        private const string DEFER_MSG_HEADER = @"Update available";
        private const string DEFER_MSG_BODY = @"There has a pending update. During update, device may be intermittently available. Do not disconnect the device during the update. This update can be deferred before it is required.";

        public void showNotification(string id, string header, string msg)
        {

            new ToastContentBuilder()
                .SetToastScenario(ToastScenario.Reminder)
                .AddArgument("deferid", id)
                .AddText(header)
                .AddText(msg)
                .AddButton(new ToastButton()
                    .SetContent("Ok")
                .AddArgument("action", "ok")
                )
                .Show(toast =>
                {
                    toast.ExpirationTime = DateTime.Now.AddSeconds(300);
                }
                );
        }
        public void showToast(string id, string header, string msg)
        {

            new ToastContentBuilder()
                .SetToastScenario(ToastScenario.Reminder)
                .AddArgument("deferid", id)
                .AddText(header)
                .AddText(msg)
                .AddButton(new ToastButton()
                    .SetContent("Make changes")
                    .AddArgument("action", "runnow")
                )
                .AddButton(new ToastButton()
                    .SetContent("Defer")
                    .AddArgument("action", "defer")
                )

                .Show(toast =>
                {
                    toast.ExpirationTime = DateTime.Now.AddSeconds(300);
                }
                );

        }
        public void FWshowToast(string id, string header, string msg)
        {

            new ToastContentBuilder()
                .SetToastScenario(ToastScenario.Reminder)
                .AddArgument("deferid", id)
                .AddText(header)
                .AddText(msg)
                .AddButton(new ToastButton()
                    .SetContent("Update now")
                    .AddArgument("action", "runnow")
                )
                .AddButton(new ToastButton()
                    .SetContent("Defer")
                    .AddArgument("action", "defer")
                )

                .Show(toast =>
                {
                    toast.ExpirationTime = DateTime.Now.AddSeconds(300);
                }
                );

        }

        #endregion

        // add @ 20250116 stephen
        private void _CliManagerPlugin_CLIDeviceCheckEvent(object? sender, CLIEventDeviceConnArgs e)
        {
            //throw new NotImplementedException();
            //Console.WriteLine($"value = {CLIEventToastArgs.toast_message}");
            bool result = checkDeviceConn(e.commands);
        }

        private bool checkDeviceConn(string data)
        {
            bool isDeviceConn = false;

            FwRule rule = genFwRule(data);

            bool result = _DevManagerPlugin.checkDeviceConnStatus(rule).Result;
            try
            {
                _CliManagerPlugin.sendDeviceCheckResult(result);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            return result;
        }

        private FwRule genFwRule(string data)
        {
            WriteLog($"genFwRule call.");
            FwRule rule = new FwRule();

            rule.devicetype = string.Empty;
            rule.model = string.Empty;
            rule.servicetag = string.Empty;

            string[] args = data.ToLower().Split(' ');
            foreach (string arg in args)
            {
                if (arg.Contains("value"))
                {
                    string[] rootArg = arg.Split('=');

                    foreach (string subAge in rootArg)
                    {
                        if (subAge.Contains("defer") || subAge.Contains("force"))
                        {
                            rule.devicetype = subAge.Split(',')[0];
                            continue;
                        }

                        // modified start @ 20250202 stephen
                        /*if(!subAge.Contains("defer") && !subAge.Contains("force") && !subAge.Contains("model") && !subAge.Contains("servicetag") && !subAge.Contains("value"))
                        {
                            rule.devicetype = rootArg[1];
                            continue;
                        }*/

                        // modified @ 20250423 stephen : fix device not display but add to fw job
                        /*if (rule.devicetype.Equals(string.Empty) && (!subAge.Contains(",")))
                        {
                            rule.devicetype = subAge;
                            continue;
                        }*/

                        // modified end @ 20250202 stephen

                        if (subAge.Contains("model"))
                        {
                            rule.model = subAge.Split(',')[0];
                            continue;
                        }

                        if (subAge.Contains("servicetag"))
                        {
                            rule.servicetag = subAge.Split(',')[0];
                            continue;
                        }
                    }


                }
            }

            return rule;
        }

    }
}