using DDPM.SA.Common;
using Dell.Client.Framework.Agent;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.Common.PluginConditions;
using Dell.UnifiedAgent.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DDPM.SA.Obfuscation;
using static DDPM.SA.Common.ICLICommandTable;

namespace DDPM.SA.Plugins.CMAManager
{
    public class CMAAgent
    {
        #region Fields

        private Guid _UniqueAgentGuid;
        private Guid _UserProcessMutexGuid;
        private string _ProductName = "CMA";
        private string _ServiceName = "CMA_subagent";
        private bool _IsAdministrator = ProcessSecurityHelperWrapper.IsCurrentProcessRunningElevated();

        private Agent _Agent;
        private ILog _Log;

        //DDPM.Subagent
        private ICliManagerIT _CliManagerPlugin;

        private readonly AutoResetEvent _PluginAvailabilityTrigger_CliManager = new(true);
        private readonly object _pluginConditionLock_CliManager = new object();
        private PluginCondition _CliManagerPluginCondition;

        //private const int TIMEOUT_IN_SECONDS = 30;
        private const int TIMEOUT_IN_SECONDS = 10;
        private int _exitcode = (int)CLI_ExitCode.unknow_command;

        private string sid = String.Empty;
        private int taskid;

        #endregion

        #region Constructor

        public CMAAgent(Guid agentGuid, Guid mutexGuid)
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

        public async Task StartAsync(string[] args, string _sid, int _taskid)
        {
            //Console.WriteLine($"@@Stephen StartAsync()");

            sid = _sid;
            taskid = _taskid;

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
                MultiSessionAgent = true
#if RELEASE
                ,
                ValidCertificateHashes = ThumbprintHash.certificateHash
#endif
            };

            //Console.WriteLine($"@@Stephen _IsAdministrator = {_IsAdministrator}");

            _Agent = new Agent(agentConfig);
            _Log = _Agent.CreateLog("DDPM.CMA");

            _Agent.PluginManager.PluginsStarted += PluginsStarted;
            _Agent.StartThread();

            while (!_Agent.IsStarted)
                await Task.Delay(10);

            RunManagement(args, _IsAdministrator);

            _Agent.StopAgent();
        }

        //If runMode = true, means run as elevated mode
        private void RunManagement(string[] args, bool runMode)
        {
            //Console.WriteLine($"@@Stephen RunManagement");
            foreach (string arg in args)
            {
                //Console.WriteLine($"@@Stephen RunManagement() args[] = {arg}");
            }

            // 2024-08-28 Casper: move upper to let command parser work earlier
            ICLICommandTable iCLICommandTable = new ICLICommandTable(_Log);
            CommandLineInput commandLineInput = iCLICommandTable.StringProcessing(args);

            // 2024-06-07 Elie, we have to check if it's null before using it.
            if (true != commandLineInput.isCliCommandsProcessCompleted)
            {
                _exitcode = ICLICommandTable.Response_FormatErrorRecommendation(commandLineInput); //parsing fail
                //_exitcode = ICLICommandTable.Response_FormatError();
                return;
            }

            // 2024-08-24 Casper: Add Help command
            if (commandLineInput.Command.Equals("HELP"))
            {
                _exitcode = ICLICommandTable.Response_HelpCommand(commandLineInput);
                return;
            }

            if (commandLineInput.DeviceIndex.Contains("-1"))
            {
                _exitcode = ICLICommandTable.Response_WrongIndex(commandLineInput);
                return;
            }

            InitializeCliManagerPlugin();

            commandLineInput.isCliRunAdmin = runMode;
            if (!runMode)//0724 only allow elevated privilege to perform action
            {
                _exitcode = ICLICommandTable.Response_UnelevatedError(commandLineInput);
                return;
            }

            //Console.WriteLine($"@@Stephen CliManagerPlugin.PerformCommandLineRelay(commandLineInput).Result start");

            CMAAgentArgs cMAAgentArgs = new CMAAgentArgs();

            if (_PluginAvailabilityTrigger_CliManager.WaitOne(TimeSpan.FromSeconds(TIMEOUT_IN_SECONDS)))
            {

                //Console.WriteLine($"@@Stephen _PluginAvailabilityTrigger_CliManager.WaitOne true");
                if (_CliManagerPlugin == null)
                {
                    _exitcode = (int)CLI_ExitCode.null_cli_manager;
                    return;
                }

                Console.WriteLine($"@@Stephen Execute commandLineInput.Result");

                //***
                //Assign command line input to CLIManager and it will pass data to CLIProxy (Relay)
                //CLIProxy should handle all possible condition and return json serialize string included in CLIEventResult
                //***
                CLIEventResult result = _CliManagerPlugin.PerformCommandLineRelay(commandLineInput).Result;
                _exitcode = result.ExitCode;



                /*Console.WriteLine($"@@Stephen _CliManagerPlugin.PerformCommandLineRelay(commandLineInput).Result.ExitCode = {_exitcode}");
                Console.WriteLine($"@@Stephen _CliManagerPlugin.PerformCommandLineRelay(commandLineInput).Result.command_guid_string = {result.command_guid_string}");
                Console.WriteLine($"@@Stephen _CliManagerPlugin.PerformCommandLineRelay(commandLineInput).Result.serialize_Json_response = {result.serialize_Json_response}");*/

                System.Console.WriteLine(result.serialize_Json_response);

                cMAAgentArgs.command_guid_string = result.command_guid_string;
                cMAAgentArgs.ExitCode = result.ExitCode;
                cMAAgentArgs.serialize_Json_response = result.serialize_Json_response;

                cMAAgentArgs.response = "{\r\n   \"sid\": \"" + sid + "\",\r\n   \"gid\": \"" + _UniqueAgentGuid + "\",\r\n   \"response\": [\r\n      \r\n      {\r\n         \"id\": \"" + taskid + "\",\r\n         \"result\": 0,\r\n         \"msg\": \"\",\r\n         \"data\": [\r\n            " + result.serialize_Json_response + "\r\n         ]\r\n      }\r\n   ]\r\n}";

                OnCommandResult(cMAAgentArgs);
                return;
            }
            else
            {

                //_Log.Error($"{nameof(IDisplayService)} was not found after {TIMEOUT_IN_SECONDS}s.");
                _Log.Error($"{nameof(ICliManagerIT)} was not found after {TIMEOUT_IN_SECONDS}s.");
                _exitcode = ICLICommandTable.Response_TimeoutError(commandLineInput);

                //Console.WriteLine($"@@Stephen _PluginAvailabilityTrigger_CliManager.WaitOne false");
                //Console.WriteLine($"@@Stephen {nameof(ICliManagerIT)} was not found after {TIMEOUT_IN_SECONDS}s.");
                //Console.WriteLine($"@@Stephen _CliManagerPlugin.PerformCommandLineRelay(commandLineInput).Result.ExitCode = {_exitcode}");

                cMAAgentArgs.ExitCode = _exitcode;
                cMAAgentArgs.response = "{\r\n   \"sid\": \"" + sid + "\",\r\n   \"gid\": \"" + _UniqueAgentGuid + "\",\r\n   \"response\": [\r\n      \r\n      {\r\n         \"id\": \"" + taskid + "\",\r\n         \"result\": 1,\r\n         \"msg\": \"Time out\",\r\n         \"data\": [\r\n\r\n         ]\r\n      }\r\n   ]\r\n}";

                OnCommandResult(cMAAgentArgs);
                return;
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
        public event EventHandler<CMAAgentArgs> CMAAgentEvent;
        private void OnCommandResult(CMAAgentArgs e)
        {
            EventHandler<CMAAgentArgs> Handler = CMAAgentEvent;
            if (Handler != null)
            {
                Handler.Invoke(this, e);
            }
        }

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

    public class CMAAgentArgs : EventArgs
    {
        public string eventtype;
        public string notification;

        public string command_guid_string;
        public string serialize_Json_response;
        public int ExitCode;
        public DateTime ticket;
        public string response;
    }

}
