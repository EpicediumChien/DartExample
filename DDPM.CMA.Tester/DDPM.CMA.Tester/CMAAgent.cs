using DDPM.RemoteManagement.Common.Interfaces;
using System.Diagnostics;
using System.Reflection;
using Dell.UnifiedAgent.Common;
using Dell.Client.Framework.Agent;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.Common.PluginConditions;
using System.Text.RegularExpressions;
using System.Text.Json;

namespace DDPM.CMA.Tester
{
    public static class StringExtensions
    {
        public static string PlainJsonString(this string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return input;
            }

            return input.Replace("\n", "").Replace("\r", "").Replace(" ", "");
        }
    }

    public class CMAAgent
    {
        #region Fields
        private static Stopwatch stopwatch = new Stopwatch();
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

        private void WriteLog(string text, bool isError = false)
        {
            string logString = $"[CMA.Tester] {text}";
            Console.WriteLine(text);
            if (_Log != null)
            {
                if (!isError)
                    _Log.Info(logString);
                else
                    _Log.Error(logString);
            }
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
                MultiSessionAgent = false
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

                //_CMAManagerPlugin.Notify += Notification;

                //Console.WriteLine("Reg");

                // while (!isresponse) { }
                runRequest(args);



                return;
            }
            else
            {
                //Means timeout here
                WriteLog($"{"Device "} was not found after {TIMEOUT_IN_SECONDS}s.");

                return;
            }

        }

        private async void runRequest(string[] args)
        {
            Boolean isRunning = true;
            #region Load Options
            List<Task<string>> loadJsonTasks = new List<Task<string>>() {
                ReadJsonFileAsync("ActHours"),
                ReadJsonFileAsync("GetDisplayMulti"),
                ReadJsonFileAsync("FwDisplay"),
                ReadJsonFileAsync("FwDock"),
                ReadJsonFileAsync("FwKb"),
                ReadJsonFileAsync("FwMouse"),
                ReadJsonFileAsync("Device"),
                ReadJsonFileAsync("DeviceData"),
                ReadJsonFileAsync("Report"),
                ReadJsonFileAsync("DeviceConfig"),
                ReadJsonFileAsync("DeviceConfig2"),
                ReadJsonFileAsync("DeviceConfig3"),
                ReadJsonFileAsync("ConfigLess"),
                ReadJsonFileAsync("DeviceDataDisplay"),
                ReadJsonFileAsync("Test"),
                ReadJsonFileAsync("FwDisplayByOption"),
                ReadJsonFileAsync("FwDisplayTest"),
                ReadJsonFileAsync("TestLock"),
                ReadJsonFileAsync("ShowDock"),
                ReadJsonFileAsync("DockConfiguration"),
                ReadJsonFileAsync("DockDiagnosticReport"),
                ReadJsonFileAsync("DockDeviceDataOptions")
            };

            string[] loadResult = await Task.WhenAll(loadJsonTasks);
            string jsonActHours = loadResult[0].PlainJsonString();
            string jsonGetDisplayMulti = loadResult[1].PlainJsonString();
            string jsonFwDisplay = loadResult[2].PlainJsonString();
            string jsonFwDock = loadResult[3].PlainJsonString();
            string jsonFwKb = loadResult[4].PlainJsonString();
            string jsonFwMouse = loadResult[5].PlainJsonString();
            string jsonDevice = loadResult[6].PlainJsonString();
            string jsonDeviceData = loadResult[7].PlainJsonString();
            string jsonReport = loadResult[8].PlainJsonString();
            string deviceConfig = loadResult[9].PlainJsonString();
            // DeviceConfig2 DEVICECONFIGURATION value is required
            string jsonDeviceConfig2 = loadResult[10].PlainJsonString();
            string jsonDeviceConfig3 = loadResult[11].PlainJsonString();
            string configLess = loadResult[12].PlainJsonString();
            string jsonDeviceDataDisplay = loadResult[13].PlainJsonString();
            string test = loadResult[14].PlainJsonString();
            string jsonFwDisplayByOption = loadResult[15].PlainJsonString();
            string fwDisplayTest = loadResult[16].PlainJsonString();
            string testLock = loadResult[17].PlainJsonString();
            string showDock = loadResult[18].PlainJsonString();
            string dockConfiguration = loadResult[19].PlainJsonString();
            string dockDiagnosticReport = loadResult[20].PlainJsonString();
            string dockDeviceDataOptions = loadResult[21].PlainJsonString();
            #endregion

            #region Subscribe Events
            RemoteRequestArgs cmaRequest = new RemoteRequestArgs();

            _CMAManagerPlugin.Notify += Notification;
            _CMAManagerPlugin.DisplayConnected += DisplayConnected;
            _CMAManagerPlugin.DisplayDisconnected += DisplayDisconnected;
            WriteLog("Subscribe Event Success");
            #endregion

            // manager.Info(json);
            PrintOptions();

            while (isRunning)
            {
                string input = Console.ReadLine() ?? string.Empty;
                if(Int32.TryParse(input, out int sel)) { 
                    stopwatch.Reset();
                    switch (sel)
                    {
                        case 1:
                            cmaRequest.remote_request = jsonDevice;
                            break;

                        case 2:
                            cmaRequest.remote_request = jsonFwDisplay;
                            break;

                        case 3:
                            cmaRequest.remote_request = jsonDeviceData;
                            break;

                        case 4:
                            cmaRequest.remote_request = jsonDeviceConfig2;
                            break;

                        case 5:
                            cmaRequest.remote_request = jsonReport;
                            break;

                        case 6:
                            cmaRequest.remote_request = jsonGetDisplayMulti;
                            break;

                        case 7:
                            cmaRequest.remote_request = jsonFwDock;
                            break;

                        case 8:
                            cmaRequest.remote_request = jsonFwKb;
                            break;

                        case 9:
                            cmaRequest.remote_request = jsonFwMouse;
                            break;

                        case 21:
                            cmaRequest.remote_request = test;
                            break;

                        case 22:
                            cmaRequest.remote_request = jsonDeviceConfig3;
                            break;

                        case 23:
                            cmaRequest.remote_request = jsonDeviceDataDisplay;
                            break;

                        case 24:
                            cmaRequest.remote_request = jsonFwDisplayByOption;
                            break;

                        case 31:
                            cmaRequest.remote_request = fwDisplayTest;
                            break;

                        case 32:
                            cmaRequest.remote_request = testLock;
                            break;

                        case 41:
                            cmaRequest.remote_request = showDock;
                            break;

                        case 42:
                            cmaRequest.remote_request = dockConfiguration;
                            break;

                        case 43:
                            cmaRequest.remote_request = dockDiagnosticReport;
                            break;

                        case 44:
                            cmaRequest.remote_request = dockDeviceDataOptions;
                            break;

                        default:
                            isRunning = false;
                            break;
                    }
                    stopwatch.Start();
                    WriteLog($"json String = {cmaRequest.remote_request}");
                    _CMAManagerPlugin.Info(cmaRequest);
                }
                while (!isresponse) { }
            }
        }

        private void InitializeCMAManagerPlugin()
        {
            if (_CMAManagerPlugin != null)
            {
                WriteLog($"{nameof(PluginsStarted)} _CMAManagerPlugin still null");
                return;
            }
            WriteLog($"{nameof(PluginsStarted)} arrived for {nameof(IRemoteManagement)}");

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
                        WriteLog($"{nameof(GetCurrentCMAManagerPluginCondition)} - CMA Manager Plugin is in an error condition");
                    }
                    else if (pluginCondition is PluginRunningCondition)//cross subagent
                    {
                        WriteLog($"{nameof(GetCurrentCMAManagerPluginCondition)} - CMA Manager Plugin is in running condition");
                        _PluginAvailabilityTrigger_CMAManager.Set();
                    }
                }
            });
        }

        private async Task<string> ReadJsonFileAsync(string fileName)
        {
            string filePath = $"..\\..\\..\\json\\{fileName}.json";
            if (File.Exists(filePath))
            {
                // Read the entire file content as text
                return await File.ReadAllTextAsync(filePath);
            }
            else
            {
                return string.Empty;
            }
        }

        private void PrintOptions()
        {
            Console.WriteLine("\nCMAManagerPlugin Demo: ");
            Console.WriteLine("1. Show ConnectedDevices.");
            Console.WriteLine("2. Show Display FWUpdate.");
            Console.WriteLine("3. Show DeviceData All (No filter).");
            Console.WriteLine("4. Show DeviceConfiguration with json in value.");
            Console.WriteLine("5. Show DiagnosticsReport export to c:\\temp\\.");
            Console.WriteLine("6. Show Multi-command get display's activehour and brightnesslevel.");
            Console.WriteLine("7. Show Dock FWUpdate.");
            Console.WriteLine("8. Show Keyboard FWUpdate.");
            Console.WriteLine("9. Show Mouse FWUpdate.");
            Console.WriteLine("10. Show Mouse FWUpdate.");

            Console.WriteLine("21. Show 2 task.");
            Console.WriteLine("22. DeviceConfiguration with only few attributes.");
            Console.WriteLine("23. Show DeviceData - Display.");
            Console.WriteLine("24. Show DeviceData - Display with Options.");

            Console.WriteLine("31. Show Fwupdate Display with options.");
            Console.WriteLine("32. Show Lock/Unlock.");

            Console.WriteLine("41. Show Dock.");
            Console.WriteLine("42. Get DockConfiguration.");
            Console.WriteLine("43. Get Dock Diagnostic Report.");
            Console.WriteLine("44. Dock Device Data update firmware with Options.");

            Console.WriteLine("0. Exit.");

            Console.WriteLine("Enter the number to run ?");
        }
        #endregion

        #region Event Handlers

        private void PluginsStarted(object? sender, PluginsStartedEventArgs e)
        {
            if (e == null)
                return;
            if (e.ChangedPlugins == null)
                return;
            if (e.ChangedPlugins.Any() == false)
                return;

            if (e.ChangedPlugins.OfType<IRemoteManagement>().Any())
            {
                InitializeCMAManagerPlugin();
            }
        }

        private void OnCMAManagerPluginConditionChangeHandler(object? sender, EventArgs e)
        {
            InitializeCMAManagerPlugin();
        }

        private void Notification(object? sender, NotifyArgs e)
        {
            WriteLog("CMA Notification Alert");
            WriteLog("CMA Notification Alert event type : " + e.eventType);
            WriteLog("CMA Notification Alert notification : " + e.notification);
            isresponse = true;
            stopwatch.Stop();
            WriteLog($"Request finished in {stopwatch.Elapsed.TotalSeconds} seconds.");
            PrintOptions();
        }

        private void DisplayConnected(object? sender, NotifyArgs e)
        {
            WriteLog("CMA DisplayConnected Alert");
            WriteLog("CMA DisplayConnected Alert eventtype : " + e.eventType);
            WriteLog("CMA DisplayConnected Alert notification : " + e.notification);

            isresponse = true;
        }

        private void DisplayDisconnected(object? sender, NotifyArgs e)
        {

            WriteLog("CMA DisplayDisconnected Alert");
            WriteLog("CMA DisplayDisconnected Alert eventtype : " + e.eventType);
            WriteLog("CMA DisplayDisconnected Alert notification : " + e.notification);

            isresponse = true;
        }
        #endregion
    }
}