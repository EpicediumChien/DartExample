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


        private static Boolean isresponse = false;
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
            InitializeCMAManagerPlugin();

            if (_PluginAvailabilityTrigger_CMAManager.WaitOne(TimeSpan.FromSeconds(TIMEOUT_IN_SECONDS)))
            {
                if (_CMAManagerPlugin == null)
                {
                    _exitcode = (int)CLI_ExitCode.null_cli_manager;
                    return;
                }
                isresponse = false;

                //_CMAManagerPlugin.Notify += Notification;

                //Console.WriteLine("Reg");

               // while (!isresponse) { }

                runRequest(args);

                

                return;
            }
            else
            {
                //Means timeout here
                Console.WriteLine($"{"Device "} was not found after {TIMEOUT_IN_SECONDS}s.");

                return;
            }
            
        }

        private void runRequest(string[] args)
        {
            string json = @"{""sid"":""1727362336"",""req"":[{""tid"":1,""active"":""get"",""devicetype"":""DISPLAY"",""command"":""ActiveHours"",""options"":{""index"":""1"",""uod"":true,""updatesilent"":""""}}]}";
            string jsonfwdisplay = @"{""sid"":""1728273741"",""req"":[{""tid"":1,""active"":""fw"",""devicetype"":""DISPLAY"",""options"":{""index"":""1"",""uod"":true,""updatesilent"":""""}}]}";
            string jsonfwdock = @"{""sid"":""1728273741"",""req"":[{""tid"":1,""active"":""fw"",""devicetype"":""DOCK"",""options"":{""index"":""1"",""uod"":true,""updatesilent"":""""}}]}";

            string jsondevice = @"{""sid"":""1728380239"",""req"":[{""tid"":1,""active"":""get"",""devicetype"":""APP"",""command"":""ConnectedDevices"",""options"":{}}]}";
            string jsondevicedata = @"{""sid"":""1728380239"",""req"":[{""tid"":1,""active"":""get"",""devicetype"":""APP"",""command"":""DeviceData"",""options"":{}}]}";
            string jsondeviceconfig = @"{""sid"":""1728380239"",""req"":[{""tid"":1,""active"":""set"",""devicetype"":""APP"",""command"":""DeviceConfiguration"",""options"":{}}]}";


            CMARequestArgs cmarequest = new CMARequestArgs();

            _CMAManagerPlugin.Notify += Notification;
            Console.WriteLine("Reg");

            // manager.Info(json);

            if (args.Length > 0)
            {
                switch (args[0].ToLower())
                {
                    case "display":
                        cmarequest.cma_request = jsonfwdisplay;
                        _CMAManagerPlugin.Info(cmarequest);
                        break;

                    case "dock":
                        cmarequest.cma_request = jsonfwdock;
                        _CMAManagerPlugin.Info(cmarequest);
                        break;

                    default:
                        cmarequest.cma_request = json;
                        _CMAManagerPlugin.Info(cmarequest);
                        break;
                }
            }
            else
            {
                cmarequest.cma_request = jsondevicedata;
                _CMAManagerPlugin.Info(cmarequest);
            }

            while (!isresponse) { }
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
            if (e == null)
                return;
            if (e.ChangedPlugins == null)
                return;
            if (e.ChangedPlugins.Any() == false)
                return;

            Console.WriteLine($"{e.ChangedPlugins.GetType().Name}");

            if (e.ChangedPlugins.OfType<ICMAManagerIT>().Any())
            {
                InitializeCMAManagerPlugin();
            }
        }

        private void OnCMAManagerPluginConditionChangeHandler(object sender, EventArgs e)
        {
            InitializeCMAManagerPlugin();
        }

        private static void Notification(object sender, NotifyArgs e)
        {

            Console.WriteLine("CMA Notification Alert");
            Console.WriteLine("CMA Notification Alert eventtype : " + e.eventtype);
            Console.WriteLine("CMA Notification Alert notification : " + e.notification);

            //isresponse = true;
        }
        #endregion
    }
}
