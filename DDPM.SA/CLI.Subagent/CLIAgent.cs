#region LicenceHeader

//
// Copyright © 2022, Dell Inc., All Rights Reserved.
// This material is confidential and a trade secret.  Permission to use this
// work for any purpose must be obtained in writing from Dell Inc.
//
// StoreManagementAgent.cs created on 10/4/2022T3:37 PM
//

#endregion

using DDPM.SA.Common;
using Dell.Client.Framework.Agent;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.Common.PluginConditions;
using Dell.UnifiedAgent.Common;
using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using static DDPM.SA.Common.ICLICommandTable;
using DDPM.SA.Obfuscation;
using System.IO;
using System.Collections.Generic;
using DDPM.SA.Common.Defer;
using DDPM.SA.Common.Settings;

namespace CLI.Subagent
{
    public class CLIAgent
    {
        #region Fields

        private Guid _UniqueAgentGuid;
        private Guid _UserProcessMutexGuid;
        private string _ProductName = "CLI";
        private string _ServiceName = "CLI_subagent";
        private bool _IsAdministrator = ProcessSecurityHelperWrapper.IsCurrentProcessRunningElevated();

        private Agent _Agent;
        private ILog _Log = null;

        //DDPM.Subagent
        private ICliManagerIT _CliManagerPlugin;

        private readonly AutoResetEvent _PluginAvailabilityTrigger_CliManager = new(true);
        private readonly object _pluginConditionLock_CliManager = new object();
        private PluginCondition _CliManagerPluginCondition;

        private const int TIMEOUT_IN_SECONDS = 30;
        private int _exitcode = (int)CLI_ExitCode.unknow_command;

        //SDL to require log folder locate at user profile (user mode subagent)
        private static readonly string LogLocation = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Dell\\DDPM.CLI");

        #endregion

        #region Constructor

        public CLIAgent(Guid agentGuid, Guid mutexGuid)
        {
            _UniqueAgentGuid = agentGuid;
            _UserProcessMutexGuid = mutexGuid;
        }

        #endregion

        #region Method

        public int GetExitCode()
        {
            return _exitcode;
        }

        public async Task StartAsync(string[] args)
        {
            _PluginAvailabilityTrigger_CliManager.Reset();

            Assembly assembly = Assembly.GetExecutingAssembly();
            UnifiedAgentConfigWindows agentConfig = new UnifiedAgentConfigWindows(_UniqueAgentGuid, _UserProcessMutexGuid)
            {
                ProductName = _ProductName,
                ProductVersion = FileVersionInfo.GetVersionInfo(assembly.Location).FileVersion,
                ServiceName = _ServiceName,
                UserProcessMutexGuid = _UserProcessMutexGuid,
                LogPrefixName = _ServiceName,
                //PluginWildcards = new[] { "CLI.Plugins.*.dll" },
                //PluginsToPublish = new List<Guid>
                //{
                //new Guid(IDs.CLI_Plugin_Display), new Guid(IDs.CLI_Plugin_Peripherals),
                //},
                AllowUnelevatedExecution = true,
                MultiSessionAgent = false,
                LogDirectory = LogLocation
#if RELEASE
                ,
                ValidCertificateHashes = ThumbprintHash_CICD.certificateHash
#endif
            };

            _Agent = new Agent(agentConfig);
            _Log = _Agent.CreateLog("DDPM.CLI");

            _Agent.PluginManager.PluginsStarted += PluginsStarted;
            _Agent.StartThread();

            while (!_Agent.IsStarted)
                await Task.Delay(10);

            if (!DDPMFileSecurity.SetFolderPermissions_UserReadAndExecute(LogLocation, out string info))
            {
                WriteLog($"[StartAsync] error: {info}", log_type.error);
#if DEBUG
                Console.WriteLine("[StartAsync] " + info);
#endif
            }

            RunManagement(args, _IsAdministrator);

            _Agent.StopAgent();
        }

        //If runMode = true, means run as elevated mode
        private void RunManagement(string[] args, bool runMode)
        {
            if (!runMode)//0724 only allow elevated privilege to perform action
            {
                WriteLog($"[RunManagement] runMode:{runMode}", log_type.error);
                _exitcode = ICLICommandTable.Response_UnelevatedError(_Log);
                return;
            }
            // 2024-08-28 Casper: move upper to let command parser work earlier
            ICLICommandTable iCLICommandTable = new ICLICommandTable(_Log);
            List<CommandLineInput> commandLineInputs = new List<CommandLineInput>();

            //support multi-command one line, Dean 1115
            if (iCLICommandTable.isContainMultipleCommand(args))
            {
                commandLineInputs = iCLICommandTable.StringProcessing_multi(args);
            }
            else
            {
                CommandLineInput commandLineInput = iCLICommandTable.StringProcessing(args);
                if (commandLineInput != null)
                {
                    commandLineInputs.Add(commandLineInput);
                }
            }
            //1115 Dean add null check
            if (commandLineInputs == null || commandLineInputs.Count == 0)
            {
                _exitcode = ICLICommandTable.Response_FormatError(_Log);
                return;
            }
            // 2024-06-07 Elie, we have to check if it's null before using it.
            //if (true != commandLineInput.isCliCommandsProcessCompleted)
            int idx = commandLineInputs.FindIndex(x => x.isCliCommandsProcessCompleted == false);
            if (idx >= 0)
            {
                _exitcode = ICLICommandTable.Response_FormatErrorRecommendation(commandLineInputs[idx], _Log); //parsing fail
                WriteLog($"Format error with ExitCode({_exitcode}), stage [Parsing]", log_type.error);
                return;
            }

            // 2024-08-24 Casper: Add Help command
            idx = commandLineInputs.FindIndex(x => x.Command.Equals("HELP"));
            if (idx >= 0)
            {
                _exitcode = ICLICommandTable.Response_HelpCommand(commandLineInputs[idx]);
                WriteLog($"Call help with ExitCode({_exitcode})");
                return;
            }
            idx = commandLineInputs.FindIndex(x => x.DeviceIndex.Contains("-1"));
            if (idx >= 0)
            {
                _exitcode = ICLICommandTable.Response_WrongIndex(commandLineInputs[idx], _Log);
                WriteLog($"Device index check with ExitCode({_exitcode})");
                return;
            }

            idx = commandLineInputs.FindIndex(x => x.TargetType != "DISPLAY" && x.SerialNumber.Count > 0);
            if (idx >= 0)
            {
                _exitcode = ICLICommandTable.Response_FormatError(_Log);
                WriteLog($"Format error with ExitCode({_exitcode}), stage [DISPLAY/SN_count]", log_type.error);
                return;
            }

            List<string> DeviceType = new List<string>()
            {
                "DISPLAY",
                "MOUSE",
                "KEYBOARD",
                "AUDIO",
                "PEN",
                "WEBCAM",
                "DOCK",
                "HEADSET",
                "SPEAKER",
                "SOUNDBAR",
            };

            List<string> Valid_Option_Name = new List<string>()
            {
                "VALUE",
                "INPUT",
                "OPCODE",
            };

            List<string> TargetFeature_WO_Value = new List<string>()
            {
                "RESTOREFACTORYDEFAULTS",
                "RESTORELEVELDEFAULTS",
                "RESTORECOLORDEFAULTS",
                "PXPZOOM",
                "SILENTFWUPDATE",
                "NETWORKKVMACCESSRESET",
            };

            idx = commandLineInputs.FindIndex(x => x.Command.Equals("SET"));
            if (idx >= 0)
            {
                //check command line has the option value with set command
                int tmp = TargetFeature_WO_Value.FindIndex(x => x.Equals(commandLineInputs[idx].TargetFeature));
                //Console.WriteLine($"idx: {idx}, option_count: {commandLineInputs[idx].Options.Count}, targetfeature: {commandLineInputs[idx].TargetFeature} {tmp}");
                if (commandLineInputs[idx].Options.Count <= 0 && TargetFeature_WO_Value.FindIndex(x => x.Equals(commandLineInputs[idx].TargetFeature)) < 0) //set command without option value --> fail
                {
                    _exitcode = ICLICommandTable.Response_FormatError(_Log);
                    WriteLog($"Format error with ExitCode({_exitcode}), stage [SET]", log_type.error);
                    return;
                }
                else
                {
                    //check if set command with correct targettype and targetfeature
                    var matchingItems = ICLICommandTable.CLIHelpCommandStructure.FeatureList.Where(dict =>
                        dict["TargetType"].ToString().Equals(commandLineInputs[idx].TargetType.ToString(), StringComparison.OrdinalIgnoreCase) &&
                        dict["TargetFeature"].ToString().Equals(commandLineInputs[idx].TargetFeature.ToString(), StringComparison.OrdinalIgnoreCase)).ToList();

                    if (matchingItems.Count <= 0) //targettype and targetfeature are not meet pre-defined value
                    {
                        _exitcode = ICLICommandTable.Response_FormatError(_Log);
                        WriteLog($"Format error with ExitCode({_exitcode}), stage [MatchItem]", log_type.error);
                        return;
                    }
                    else
                    {
                        //check if Option Value meet requirement
                        foreach (var option in commandLineInputs[idx].Options) //check each option value
                        {
                            string[] ov = option.Option_Value.Split(',');
                            if ((commandLineInputs[idx].TargetFeature.Equals("FIRMWAREUPDATE")))
                            {
                                if (!string.IsNullOrEmpty(ov[0]) && (DeviceType.FindIndex(x => x.Equals(ov[0])) >= 0))
                                {
                                    break;
                                }
                                else
                                {
                                    _exitcode = ICLICommandTable.Response_FormatError(_Log);
                                    WriteLog($"Format error with ExitCode({_exitcode}), stage [FWUpdate]", log_type.error);
                                    return;
                                }
                            }
                            //Console.WriteLine($"Option_Name: {option.Option_Name}");
                            if (Valid_Option_Name.FindIndex(x => x.Equals(option.Option_Name)) < 0)
                            {
                                _exitcode = ICLICommandTable.Response_FormatError(_Log);
                                WriteLog($"Format error with ExitCode({_exitcode}), stage [OptionName]", log_type.error);
                                return;
                            }
                        }
                    }
                }
            }

            InitializeCliManagerPlugin();
            if (_PluginAvailabilityTrigger_CliManager.WaitOne(TimeSpan.FromSeconds(TIMEOUT_IN_SECONDS)))
            {
                if (_CliManagerPlugin == null)
                {
                    _exitcode = (int)CLI_ExitCode.null_cli_manager;
                    WriteLog($"Seek cli manager and got timeout with ExitCode({_exitcode})", log_type.error);
                    return;
                }

                #region Defer, ForceWithNotice, ForceWithNoNotice command parsing
                if (commandLineInputs.Count == 1)
                {
                    var commandLineInput = commandLineInputs[0];
                    var isDefer = false;
                    var isForceWithNotice = false;
                    var isForceWithNoNotice = false;
                    var cmds = string.Join(" ", args.Select(_ => _.ToUpper()));

                    if (commandLineInput.Command == "SET")
                    {
                        // FWUpdate not support ForceWithNoNotice, and need check device is connected
                        if (IsFWUpdate(commandLineInput))
                        {
                            if (commandLineInput.Options.Any(_ => _.Option_Value.Contains(",FORCEWITHNONOTICE")))
                            {
                                _exitcode = ICLICommandTable.ResponseNotSupportValue(commandLineInput, _Log);
                                WriteLog($"Check key [FORCEWITHNONOTICE] with ExitCode({_exitcode})", log_type.error);
                                return;
                            }
                            if (!CLIFWUpdateCheckDevice(args, commandLineInput))
                            {
                                WriteLog($"Check device under [FORCEWITHNONOTICE]", log_type.error);
                                return;
                            }
                        }

                        // check if command not supported defer, forceWithNotice, forceWithNoNotice
                        if (IsNotSupportDeferCommand(commandLineInput))
                        {
                            WriteLog($"[SET][IsNotSupportDeferCommand] got exit return", log_type.error);
                            return;
                        }

                        /// FirmwareUpdate & Update
                        /// Default: defer
                        /// FirmwareUpdate support: defer, forceWithNotice
                        /// Update support: defer, forceWithNotice, forceWithNoNotice
                        /// Need to replace defer with empty strings for subsequent CLI use.
                        if (IsFWUpdate(commandLineInput) || IsSWUpdate(commandLineInput))
                        {
                            foreach (var option in commandLineInput.Options)
                            {
                                if (option.Option_Value.Contains(",DEFER"))
                                {
                                    isDefer = true;
                                    if (CLIDefer(args, commandLineInput, option))
                                    {
                                        WriteLog($"check [DEFER] and got exit return", log_type.error);
                                        return;
                                    }
                                    break;
                                }
                                else if (option.Option_Value.Contains(",FORCEWITHNOTICE"))
                                {
                                    isForceWithNotice = true;
                                    CLIForceWithNotice(args);
                                    WriteLog($"check [FORCEWITHNOTICE] and set to true", log_type.info);
                                    break;
                                }
                                else if (option.Option_Value.Contains(",FORCEWITHNONOTICE") && commandLineInput.TargetFeature == "UPDATE")
                                {
                                    isForceWithNoNotice = true;
                                    WriteLog($"check [FORCEWITHNONOTICE][UPDATE] and set to true", log_type.info);
                                    break;
                                }
                            }

                            if (!(isDefer || isForceWithNotice || isForceWithNoNotice) &&
                                CLIDefer(args, commandLineInput))
                            {
                                WriteLog($"check isDefer({isDefer}),isForceWithNotice({isForceWithNotice}),isForceWithNoNotice({isForceWithNoNotice}) and CLIDefer() be true, got exit return", log_type.error);
                                return;
                            }
                        }
                        /// Set commands
                        /// Default: forceWithNotice
                        /// Support: defer, forceWithNotice, forceWithNoNotice
                        /// Need to replace defer, forceWithNotice, forceWithNoNotice with empty strings for subsequent CLI use.
                        else if (commandLineInput.Options.Count > 0)
                        {
                            foreach (var option in commandLineInput.Options)
                            {
                                if (option.Option_Value.Contains("DEFER"))
                                {
                                    isDefer = true;
                                    if (CLIDefer(args, commandLineInput, option))
                                    {
                                        WriteLog($"contain [DEFER] and CLIDefer() be true, got exit return", log_type.error);
                                        return;
                                    }
                                    break;
                                }
                                else if (option.Option_Value.Contains("FORCEWITHNOTICE"))
                                {
                                    isForceWithNotice = true;
                                    WriteLog($"contain [FORCEWITHNOTICE] then call CLIForceWithNotice", log_type.info);
                                    CLIForceWithNotice(args, option);                                    
                                    break;
                                }
                                else if (option.Option_Value.Contains("FORCEWITHNONOTICE"))
                                {
                                    isForceWithNoNotice = true;
                                    WriteLog($"contain [FORCEWITHNONOTICE] then call CLIForceWithNoNotice", log_type.info);
                                    CLIForceWithNoNotice(option);                                    
                                    break;
                                }
                            }

                            if (!(isDefer || isForceWithNotice || isForceWithNoNotice))
                            {
                                WriteLog($"commandLineInput.Options.Count:{commandLineInput.Options.Count}, isDefer({isDefer}),isForceWithNotice({isForceWithNotice}),isForceWithNoNotice({isForceWithNoNotice})", log_type.info);
                                CLIForceWithNotice(args);
                            }

                            // After parsing, need to remove empty option
                            commandLineInput.Options.RemoveAll(_ => string.IsNullOrWhiteSpace(_.Option_Value));
                        }
                        else if (!commandLineInput.TargetFeature.Equals("SILENTFWUPDATE"))
                        {
                            WriteLog($"else not the [SILENTFWUPDATE], call CLIForceWithNotice()", log_type.info);
                            CLIForceWithNotice(args);
                        }
                    }
                }
                #endregion

                List<int> returnCode = new List<int>();
                foreach (CommandLineInput commandLineInput in commandLineInputs)
                {
                    commandLineInput.isCliRunAdmin = runMode;
                    //***
                    //Assign command line input to CLIManager and it will pass data to CLIProxy (Relay)
                    //CLIProxy should handle all possible condition and return json serialize string included in CLIEventResult
                    //***
                    CLIEventResult result = _CliManagerPlugin.PerformCommandLineRelay(commandLineInput).Result;
                    returnCode.Add(result.ExitCode);
                    Console.WriteLine(result.serialize_Json_response);
                    WriteLog($"[Pass to CLIProxy] Command ID:{result.command_guid_string}, ExitCode:{result.ExitCode}");
                    WriteLog($"[Response]: {result.serialize_Json_response}");
                }
                int n = returnCode.FindIndex(x => (x != (int)CLI_ExitCode.success));
                if (n >= 0)
                {
                    _exitcode = returnCode[n];
                }
                else
                    _exitcode = (int)CLI_ExitCode.success;
            }
            else
            {
                WriteLog($"{nameof(ICliManagerIT)} was not found after {TIMEOUT_IN_SECONDS}s.", log_type.error);
                _exitcode = ICLICommandTable.Response_TimeoutError(commandLineInputs[0], _Log);
                return;
            }
        }

        #region Defer, ForceWithNotice, ForceWithNoNotice command parsing function
        private bool IsFWUpdate(CommandLineInput commandLineInput)
        {
            return commandLineInput.TargetType.Equals("APP") && commandLineInput.TargetFeature.Equals("FIRMWAREUPDATE");
        }

        private bool IsNotSupportDeferCommand(CommandLineInput commandLineInput)
        {
            var notSupportTargetFeatures = new List<string> { "SILENTFWUPDATE", "UPDATESOURCELOCATION", "IMPORTSETTINGS", "TELEMETRYCONSENT", "INAPPUPDATE" };

            if (commandLineInput.Options.Count > 0 &&
                commandLineInput.Options.Any(_ => _.Option_Value.Contains("DEFER") || _.Option_Value.Contains("FORCEWITHNOTICE")) &&
                notSupportTargetFeatures.Any(_ => _.Equals(commandLineInput.TargetFeature)))
            {
                _exitcode = ICLICommandTable.ResponseNotSupportValue(commandLineInput, _Log);
                WriteLog($"[IsNotSupportDeferCommand] ResponseNotSupportValue, exit({_exitcode})");
                return true;
            }
            if (commandLineInput.Options.Count > 0 &&
                !commandLineInput.Options.Any(_ => _.Option_Value.Contains("FORCEWITHNONOTICE")) &&
                notSupportTargetFeatures.Any(_ => _.Equals(commandLineInput.TargetFeature)))
            {
                commandLineInput.Options[0].Option_Value += ",FORCEWITHNONOTICE";
            }
            return false;
        }

        private bool IsSWUpdate(CommandLineInput commandLineInput)
        {
            return commandLineInput.TargetType.Equals("APP") && commandLineInput.TargetFeature.Equals("UPDATE");
        }

        private bool CLIFWUpdateCheckDevice(string[] args, CommandLineInput commandLineInput)
        {
            var cmds = string.Join(" ", args.Select(_ => _.ToUpper()));

            if (!_CliManagerPlugin.checkDeviceConn(DeferControlPanel.SRC_FROM_CLI, _UniqueAgentGuid.ToString(), cmds.Trim(), cmds).Result)
            {
                WriteLog("[CLIFWUpdateCheckDevice] _CliManagerPlugin.checkDeviceConn got false return");
                _exitcode = ICLICommandTable.ResponseFWUpdateDeviceNotConnected(commandLineInput);
                return false;
            }
            return true;
        }

        /// <summary>
        /// Replace DEFER option to empty string if it's not null and run checkDefer
        /// </summary>
        /// <param name="args"></param>
        /// <param name="commandLineInput"></param>
        /// <param name="option"></param>
        /// <returns></returns>
        private bool CLIDefer(string[] args, CommandLineInput commandLineInput, CommandType_Option option = null)
        {
            if (option != null)
            {
                option.Option_Value = option.Option_Value.Replace(",DEFER", "")
                                                         .Replace("DEFER", "");
            }
            var cmds = string.Join(" ", args.Select(_ => _.ToUpper()
                                                          .Replace(",DEFER", "")
                                                          .Replace("DEFER", ""))
                                            .Where(_ => !_.Equals("-VALUE=") && !_.Equals("VALUE=")));

            if (_CliManagerPlugin.checkDefer(DeferControlPanel.SRC_FROM_CLI, _UniqueAgentGuid.ToString(), cmds).Result)
            {
                _exitcode = ICLICommandTable.ResponseDefer(commandLineInput, _Log);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Replace FORCEWITHNOTICE option to empty string if it's not null and run showNotification
        /// </summary>
        /// <param name="args"></param>
        /// <param name="option"></param>
        private void CLIForceWithNotice(string[] args, CommandType_Option option = null)
        {
            if (option != null)
            {
                option.Option_Value = option.Option_Value.Replace(",FORCEWITHNOTICE", "")
                                                         .Replace("FORCEWITHNOTICE", "");
            }
            var cmds = string.Join(" ", args.Select(_ => _.ToUpper()
                                                          .Replace(",FORCEWITHNOTICE", "")
                                                          .Replace("FORCEWITHNOTICE", "")));

            try
            {
                var deferItem = new DeferItem(DeferControlPanel.SRC_FROM_CLI, _UniqueAgentGuid.ToString(), cmds);
                _CliManagerPlugin.showNotification(DeferControlPanel.SRC_FROM_CLI, _UniqueAgentGuid.ToString(), deferItem);
            }
            catch (Exception ex)
            {
                WriteLog($"[CLI] CLIForceWithNotice exception: {ex.ToString}", log_type.error);
#if DEBUG
                Console.WriteLine($"[CLI] CLIForceWithNotice exception: {ex.ToString}");
#endif
            }
        }
        #endregion

        /// <summary>
        /// Replace FORCEWITHNONOTICE option to empty string if it's not null
        /// </summary>
        /// <param name="option"></param>
        private void CLIForceWithNoNotice(CommandType_Option option)
        {
            if (option != null)
            {
                option.Option_Value = option.Option_Value.Replace(",FORCEWITHNONOTICE", "")
                                                         .Replace("FORCEWITHNONOTICE", "");
            }
        }

        private void InitializeCliManagerPlugin()
        {
            if (_CliManagerPlugin != null)
                return;

            _Log.Info($"{nameof(PluginsStarted)} arrived for {nameof(ICliManagerIT)}");

            _CliManagerPlugin = _Agent.PluginManager.FindPluginByType<ICliManagerIT>(PluginResolution.Dynamic);
            if (_CliManagerPlugin is IFrameworkPluginConditionNotification condition)
            {
                condition.PluginConditionChangeHandler += OnCliManagerPluginConditionChangeHandler;
                GetCurrentCliManagerPluginCondition();
            }
        }

        private void GetCurrentCliManagerPluginCondition()
        {
            _ = Task.Run(async () =>
            {
                var pluginCondition = await (_CliManagerPlugin as IFrameworkPluginConditionNotification)?.CurrentConditionAsync();

                lock (_pluginConditionLock_CliManager)
                {
                    _CliManagerPluginCondition = pluginCondition;

                    if (pluginCondition is PluginErrorCondition)
                    {
                        _Log?.Info($"{nameof(GetCurrentCliManagerPluginCondition)} - Cli Manager Plugin is in an error condition");
                    }
                    else if (pluginCondition is PluginRunningCondition)//cross subagent
                    {
                        _Log?.Info($"{nameof(GetCurrentCliManagerPluginCondition)} - Cli Manager Plugin is in running condition");
                        _PluginAvailabilityTrigger_CliManager.Set();
                    }
                }
            });
        }

        #endregion

        #region Event Handlers

        private void PluginsStarted(object sender, PluginsStartedEventArgs e)
        {
            if (e?.ChangedPlugins == null)
                return;
            if (e.ChangedPlugins.Any() == false)
                return;

            if (e.ChangedPlugins.OfType<ICliManagerIT>().Any())
            {
                InitializeCliManagerPlugin();
            }
        }

        private void OnCliManagerPluginConditionChangeHandler(object sender, EventArgs e)
        {
            InitializeCliManagerPlugin();
        }

        #endregion

        #region private method
        private void WriteLog(string text, log_type log_type = log_type.info)
        {
            string logString = $"[CLI.Subagent] {text}";
            if (_Log != null)
            {
                if (log_type == log_type.info)
                    _Log.Info(logString);
                else
                    _Log.Error(logString);
            }
        }

        private enum log_type
        {
            info = 0,
            error
        }
        #endregion
    }
}