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
        private ILog _Log;

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
                MultiSessionAgent = true,
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
                _Log.Error($"[CLI][StartAsync] error: {info}");
#if DEBUG
                Console.WriteLine("[CLI][StartAsync] " + info);
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
                _exitcode = ICLICommandTable.Response_UnelevatedError();
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
                _exitcode = ICLICommandTable.Response_FormatError();
                return;
            }
            // 2024-06-07 Elie, we have to check if it's null before using it.
            //if (true != commandLineInput.isCliCommandsProcessCompleted)
            int idx = commandLineInputs.FindIndex(x => x.isCliCommandsProcessCompleted == false);
            if (idx >= 0)
            {
                _exitcode = ICLICommandTable.Response_FormatErrorRecommendation(commandLineInputs[idx]); //parsing fail
                return;
            }

            // 2024-08-24 Casper: Add Help command
            //idx = commandLineInputs.FindIndex(x => x.Command.Equals("HELP") && x.TargetFeature != "DISPLAY");
            idx = commandLineInputs.FindIndex(x => x.Command.Equals("HELP"));
            if (idx >= 0)
            {
                _exitcode = ICLICommandTable.Response_HelpCommand(commandLineInputs[idx]);
                return;
            }
            idx = commandLineInputs.FindIndex(x => x.DeviceIndex.Contains("-1"));
            if (idx >= 0)
            {
                _exitcode = ICLICommandTable.Response_WrongIndex(commandLineInputs[idx]);
                return;
            }

            InitializeCliManagerPlugin();
            if (_PluginAvailabilityTrigger_CliManager.WaitOne(TimeSpan.FromSeconds(TIMEOUT_IN_SECONDS)))
            {
                if (_CliManagerPlugin == null)
                {
                    _exitcode = (int)CLI_ExitCode.null_cli_manager;
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
                                _exitcode = ICLICommandTable.ResponseNotSupportValue(commandLineInput);
                                return;
                            }
                            if (!CLIFWUpdateCheckDevice(args, commandLineInput))
                                return;
                        }

                        // check if command not supported defer, forceWithNotice, forceWithNoNotice
                        if (IsNotSupportDeferCommand(commandLineInput))
                        {
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
                                        return;
                                    }
                                    break;
                                }
                                else if (option.Option_Value.Contains(",FORCEWITHNOTICE"))
                                {
                                    isForceWithNotice = true;
                                    CLIForceWithNotice(args);
                                    break;
                                }
                                else if (option.Option_Value.Contains(",FORCEWITHNONOTICE") && commandLineInput.TargetFeature == "UPDATE")
                                {
                                    isForceWithNoNotice = true;
                                    break;
                                }
                            }

                            if (!(isDefer || isForceWithNotice || isForceWithNoNotice) &&
                                CLIDefer(args, commandLineInput))
                            {
                                return;
                            }
                        }
                        /// Set commands
                        /// Default: forceWithNotice
                        /// Support: defer, forceWithNotice, forceWithNoNotice
                        /// Need to replace defer, forceWithNotice, forceWithNoNotice with empty strings for subsequent CLI use.
                        else if (commandLineInput.Options.Count > 0)
                        {
                            //if (IsTelemetryConsentInAppUpdateUpdateSourceLocation(commandLineInput))
                            //{
                            //    
                            //    if (commandLineInput.Options.Any(_ => _.Option_Value.Contains("DEFER") || _.Option_Value.Contains("FORCEWITHNOTICE")))
                            //    {
                            //        _exitcode = ICLICommandTable.ResponseNotSupportValue(commandLineInput);
                            //        return;
                            //    }

                            //    if (!commandLineInput.Options.Any(_ => _.Option_Value.Contains("FORCEWITHNONOTICE")))
                            //    {
                            //        commandLineInput.Options[0].Option_Value += ",FORCEWITHNONOTICE";
                            //    }
                            //}

                            foreach (var option in commandLineInput.Options)
                            {
                                if (option.Option_Value.Contains("DEFER"))
                                {
                                    isDefer = true;
                                    if (CLIDefer(args, commandLineInput, option))
                                    {
                                        return;
                                    }
                                    break;
                                }
                                else if (option.Option_Value.Contains("FORCEWITHNOTICE"))
                                {
                                    isForceWithNotice = true;
                                    CLIForceWithNotice(args, option);
                                    break;
                                }
                                else if (option.Option_Value.Contains("FORCEWITHNONOTICE"))
                                {
                                    isForceWithNoNotice = true;
                                    CLIForceWithNoNotice(option);
                                    break;
                                }
                            }

                            if (!(isDefer || isForceWithNotice || isForceWithNoNotice))
                            {
                                CLIForceWithNotice(args);
                            }

                            // After parsing, need to remove empty option
                            commandLineInput.Options.RemoveAll(_ => string.IsNullOrWhiteSpace(_.Option_Value));
                        }
                        else if (!commandLineInput.TargetFeature.Equals("SILENTFWUPDATE"))
                        {
                            CLIForceWithNotice(args);
                        }
                    }
                }
                #endregion

                /*
                // add start @ 20250120 stephen: test for defer and firmware with connect check
                bool hasDefer = false;
                bool hasFwUpdate = false;
                string cmds = string.Empty;


                foreach (string arg in args)
                {
                    //Console.WriteLine("@@@@stephen RunManagement arg = " + arg);

                    cmds = cmds + arg + " ";
                    if (arg.ToLower().Contains("firmwareupdate"))
                    {
                        hasFwUpdate = true;
                    }

                    if (arg.ToLower().Contains("defer"))
                    {
                        hasDefer = true;
                    }
                }

                if (hasFwUpdate) {
                    if (!_CliManagerPlugin.checkDeviceConn(DeferControlPanel.SRC_FROM_CLI, _UniqueAgentGuid.ToString(), cmds.Trim(), cmds).Result)
                    {
                        // true: device not found
                        Console.WriteLine("hasFwUpdate = true, checkDeviceConn = false, device not found ");
                        return;
                    }

                }

                if (hasDefer)
                {
                    //Console.WriteLine("@@@@stephen check Defer Result ");

                    if (_CliManagerPlugin.checkDefer(DeferControlPanel.SRC_FROM_CLI, _UniqueAgentGuid.ToString(), cmds.Trim()).Result)
                    {
                        Console.WriteLine("hasDefer = true, checkDefer = true, add to defer schedule ");
                        return;
                    }
                }
                // add end @ 20250120
                */

                List<int> returnCode = new List<int>();
                foreach (CommandLineInput commandLineInput in commandLineInputs)
                {
                    commandLineInput.isCliRunAdmin = runMode;
                    //***
                    //Assign command line input to CLIManager and it will pass data to CLIProxy (Relay)
                    //CLIProxy should handle all possible condition and return json serialize string included in CLIEventResult
                    //***
                    CLIEventResult result = _CliManagerPlugin.PerformCommandLineRelay(commandLineInput).Result;
                    //_exitcode = result.ExitCode;
                    returnCode.Add(result.ExitCode);
                    Console.WriteLine(result.serialize_Json_response);
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
                //_Log.Error($"{nameof(IDisplayService)} was not found after {TIMEOUT_IN_SECONDS}s.");
                _Log.Error($"{nameof(ICliManagerIT)} was not found after {TIMEOUT_IN_SECONDS}s.");
                _exitcode = ICLICommandTable.Response_TimeoutError(commandLineInputs[0]);
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
            var notSupportTargetFeatures = new List<string> { "SILENTFWUPDATE", "UPDATESOURCELOCATION", "IMPORTSETTINGS", "TELEMETRYCONSENT", "INAPPUPDATE"};

            if (commandLineInput.Options.Count > 0 &&
                commandLineInput.Options.Any(_ => _.Option_Value.Contains("DEFER") || _.Option_Value.Contains("FORCEWITHNOTICE")) &&
                notSupportTargetFeatures.Any(_ => _.Equals(commandLineInput.TargetFeature)))
            {
                _exitcode = ICLICommandTable.ResponseNotSupportValue(commandLineInput);
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

        //private bool IsTelemetryConsentInAppUpdateUpdateSourceLocation(CommandLineInput commandLineInput)
        //{
        //    var validFeatures = new HashSet<string>
        //    {
        //        "TELEMETRYCONSENT",
        //        "UPDATESOURCELOCATION",
        //        "INAPPUPDATE"
        //    };

        //    if (commandLineInput.TargetType.Equals("APP") && validFeatures.Contains(commandLineInput.TargetFeature))
        //        return true;
        //    else 
        //        return false;
        //}

        private bool CLIFWUpdateCheckDevice(string[] args, CommandLineInput commandLineInput)
        {
            var cmds = string.Join(" ", args.Select(_ => _.ToUpper()));

            if (!_CliManagerPlugin.checkDeviceConn(DeferControlPanel.SRC_FROM_CLI, _UniqueAgentGuid.ToString(), cmds.Trim(), cmds).Result)
            {
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
                _exitcode = ICLICommandTable.ResponseDefer(commandLineInput);
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
                _Log.Error($"[CLI] CLIForceWithNotice exception: {ex.ToString}");
#if DEBUG
                Console.WriteLine($"[CLI] CLIForceWithNotice exception: {ex.ToString}");
#endif
            }


            //Thread.Sleep(5000);
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
                        _Log.Info($"{nameof(GetCurrentCliManagerPluginCondition)} - Cli Manager Plugin is in an error condition");
                    }
                    else if (pluginCondition is PluginRunningCondition)//cross subagent
                    {
                        _Log.Info($"{nameof(GetCurrentCliManagerPluginCondition)} - Cli Manager Plugin is in running condition");
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
    }
}