using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dell.Client.Framework.Agent;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.Common.PluginConditions;
using DDPM.SA.Common;
using System.Diagnostics;
using System.Reflection;
using Dell.UnifiedAgent.Common;

namespace DDPM.CMA.Tester
{
    public class CMAAgent
    {
        #region Fields

        private Guid _UniqueAgentGuid;
        private Guid _UserProcessMutexGuid;
        private string _ProductName = "CMA.Tester";
        private string _ServiceName = "CMA.Tester.Subagent";
        private bool _IsAdministrator = ProcessSecurityHelperWrapper.IsCurrentProcessRunningElevated();

        private Agent _Agent;
        private ILog _Log;

        //DDPM.Subagent
        private ICMAManagerIT _CMAManagerPlugin;

        private readonly AutoResetEvent _PluginAvailabilityTrigger_CMAManager = new(true);
        private readonly object _pluginConditionLock_CMAManager = new object();
        private PluginCondition _CMAManagerPluginCondition;

        private const int TIMEOUT_IN_SECONDS = 60;
        private static int _exitcode = 0;

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

        public async Task StartAsync(string[] args)
        {
            _PluginAvailabilityTrigger_CMAManager.Reset();

            Assembly assembly = Assembly.GetExecutingAssembly();
            UnifiedAgentConfigWindows agentConfig = new UnifiedAgentConfigWindows(_UniqueAgentGuid, _UserProcessMutexGuid)
            {
                ProductName = _ProductName,
                ProductVersion = FileVersionInfo.GetVersionInfo(assembly.Location).FileVersion,
                ServiceName = _ServiceName,
                UserProcessMutexGuid = _UserProcessMutexGuid,
                LogPrefixName = _ServiceName,
                AllowUnelevatedExecution = true,
                MultiSessionAgent = true
            };

            _Agent = new Agent(agentConfig);
            _Log = _Agent.CreateLog("DDPM.CMA.Tester");

            _Agent.PluginManager.PluginsStarted += PluginsStarted;
            _Agent.StartThread();

            while (!_Agent.IsStarted)
                await Task.Delay(10);

            RunManagement(args, _IsAdministrator);

            _Agent.StopAgent();
        }

        private void RunManagement(string[] args, bool runMode)
        {
            if (_CMAManagerPlugin == null)
            {
                Console.WriteLine("No CMA Manager be found");
                _exitcode = 1;
                return;
            }

            if(args.Length == 0)
            {
                Console.WriteLine("No command line input");
                _exitcode = 2;
                return;
            }

            CMARequestArgs input = new CMARequestArgs();
            input.cma_request = args[0];
            
            CMAResult result = _CMAManagerPlugin.PerformCMARequest(input).Result;
            if (result != null)
            {
                Console.WriteLine("Detail: " + result.message);
                Console.WriteLine("Detail: " + result.output_result);
                Console.WriteLine($"ID: {result.cma_request_id}");
            }
            else
            {
                Console.WriteLine("CMA Manager return null result");
            }
        }

        private void InitializeCMAManagerPlugin()
        {
            if (_CMAManagerPlugin != null)
                return;

            _Log.Info($"{nameof(PluginsStarted)} arrived for {nameof(ICMAManagerIT)}");

            _CMAManagerPlugin = _Agent.PluginManager.FindPluginByType<ICMAManagerIT>(PluginResolution.Dynamic);
            if (_CMAManagerPlugin is IFrameworkPluginConditionNotification condition)
            {
                condition.PluginConditionChangeHandler += OnCMAManagerPluginConditionChangeHandler;
                GetCurrentCMAManagerPluginCondition();
            }
        }

        private void GetCurrentCMAManagerPluginCondition()
        {
            _ = Task.Run(async () =>
            {
                var pluginCondition = await (_CMAManagerPlugin as IFrameworkPluginConditionNotification)?.CurrentConditionAsync();

                lock (_pluginConditionLock_CMAManager)
                {
                    _CMAManagerPluginCondition = pluginCondition;

                    if (pluginCondition is PluginErrorCondition)
                    {
                        _Log.Info($"{nameof(GetCurrentCMAManagerPluginCondition)} - CMA Manager Plugin is in an error condition");
                    }
                    else if (pluginCondition is PluginRunningCondition)//cross subagent
                    {
                        _Log.Info($"{nameof(GetCurrentCMAManagerPluginCondition)} - CMA Manager Plugin is in running condition");
                        _PluginAvailabilityTrigger_CMAManager.Set();
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

            if (e.ChangedPlugins.OfType<ICMAManagerIT>().Any())
            {
                InitializeCMAManagerPlugin();
            }
        }

        private void OnCMAManagerPluginConditionChangeHandler(object sender, EventArgs e)
        {
            InitializeCMAManagerPlugin();
        }

        #endregion
    }
}
