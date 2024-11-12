using DDPM.RemoteManagement.Common.Interfaces;
using System.Diagnostics;
using System.Reflection;
using Dell.UnifiedAgent.Common;
using Dell.Client.Framework.Agent;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.Common.PluginConditions;
using System.Windows.Controls;
using Windows.UI.ViewManagement.Core;
using System.Windows.Interop;
using System.Text;

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

        private IRemoteManagement _CMAManagerPlugin;

        private readonly AutoResetEvent _PluginAvailabilityTrigger_CMAManager = new(true);
        private readonly object _pluginConditionLock_CMAManager = new object();
        private PluginCondition _CMAManagerPluginCondition;

        private const int TIMEOUT_IN_SECONDS = 60;
        private static int _exitcode = 0;

        private static string txtOutput = string.Empty;

        private StringBuilder txtOutputStringBuilder = new StringBuilder();

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
                    _exitcode = 123;//Editable field //(int)CLI_ExitCode.null_cli_manager;
                    return;
                }
                isresponse = false;

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
            Boolean isRunning = true;
            
            const string PATH = @"C:\cmacmd\out\";

            List<string> list = new List<string>();
        
            DirectoryInfo dir = new DirectoryInfo(PATH);
            FileInfo[] files = dir.GetFiles("*");

            RemoteRequestArgs cmaRequest = new RemoteRequestArgs();

            _CMAManagerPlugin.Notify += Notification;

            Console.WriteLine("Subscribe Event Success");


            foreach (FileInfo file in files)
            {
                string[] alllines = File.ReadAllLines(file.FullName);

                txtOutput = string.Empty;
                isRunning = true;

                txtOutputStringBuilder = new StringBuilder();
                txtOutputStringBuilder.Append(DateTime.Now.ToString("yyyy MMM dd HH:mm:sss.fff") + "\t" + file.Name + " Start test.\n\n");

                Console.WriteLine(txtOutputStringBuilder.ToString());

                Thread.Sleep(5000);

                while (isRunning)
                {
                    for (int i = 0; i < alllines.Length; i++)
                    {
                        isresponse = false;

                        if (alllines[i].Length <= 0)
                        {
                            continue;
                        }

                        try
                        {
                            Console.WriteLine("Test Json = " + alllines[i]);
                            //txtOutput = txtOutput + "Test Json = " + alllines[i] + "\n";


                            txtOutputStringBuilder.Append("Test Json = " + alllines[i] + "\n");
                            cmaRequest.remote_request = alllines[i];

                            _CMAManagerPlugin.Info(cmaRequest);


                            while (!isresponse) { }

                            Thread.Sleep(3000);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message + ";" + alllines[i]);
                        }
                    }

                    isRunning = false;
                    break;
                }


                txtOutput = DateTime.Now.ToString("yyyy MMM dd HH:mm:sss.fff") + "\t" + file.Name + " End test.\n\n";
                Console.WriteLine(txtOutput);

                txtOutputStringBuilder.Append(txtOutput);

                // write files in the folder

                try
                {
                    if (!(System.IO.Directory.Exists(PATH + "result")))
                    {
                        System.IO.Directory.CreateDirectory(PATH + "result");
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("CreateDirectory Exception: " + e.Message);
                }

                File.WriteAllLines(PATH + "result\\result_" + file.Name, txtOutputStringBuilder.ToString().Split('\n'));

                Thread.Sleep(5000);

            }


        }

        private void InitializeCMAManagerPlugin()
        {
            if (_CMAManagerPlugin != null)
                return;
            //Console.WriteLine($"{nameof(PluginsStarted)} arrived for {nameof(IRemoteManagement)}");

            _CMAManagerPlugin = _Agent.PluginManager.FindPluginByType<IRemoteManagement>(PluginResolution.Dynamic);
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
                        Console.WriteLine($"{nameof(GetCurrentCMAManagerPluginCondition)} - CMA Manager Plugin is in an error condition");
                    }
                    else if (pluginCondition is PluginRunningCondition)//cross subagent
                    {
                        Console.WriteLine($"{nameof(GetCurrentCMAManagerPluginCondition)} - CMA Manager Plugin is in running condition");
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

            //Console.WriteLine($"{e.ChangedPlugins.GetType().Name}");

            if (e.ChangedPlugins.OfType<IRemoteManagement>().Any())
            {
                InitializeCMAManagerPlugin();
            }
        }

        private void OnCMAManagerPluginConditionChangeHandler(object sender, EventArgs e)
        {
            InitializeCMAManagerPlugin();
        }

        private void Notification(object? sender, NotifyArgs e)
        {

            Console.WriteLine("CMA Notification Alert");
            Console.WriteLine("CMA Notification Alert eventtype : " + e.eventType);
            Console.WriteLine("CMA Notification Alert notification : " + e.notification);

            txtOutputStringBuilder.Append("CMA Notification Alert eventtype : " + e.eventType + "\n");
            txtOutputStringBuilder.Append("CMA Notification Alert eventtype : " + e.notification + "\n");

            //txtOutput = txtOutput + "CMA Notification Alert eventtype : \"" + e.eventType + "\n";
            //txtOutput = txtOutput + "CMA Notification Alert eventtype : \"" + e.notification + "\n";

            isresponse = true;
        }

        #endregion
    }
}