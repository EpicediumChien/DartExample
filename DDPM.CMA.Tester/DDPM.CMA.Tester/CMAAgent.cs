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
    public class CMAAgent
    {
        #region Fields
        private static List<int> requestNumList = new List<int>();
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

        private async void runRequest(string[] args)
        {
            Boolean isRunning = true;

            
            string jsonacthours = @"{""sid"":""1727362336"",""req"":[{""tid"":1,""active"":""get"",""devicetype"":""DISPLAY"",""command"":""ActiveHours"",""options"":{}}]}";
            // string jsonActHours = await ReadJsonFileAsync("ActHours");
            string jsongetdisplaymulti = @"{""sid"":""1727362335"",""req"":[{""tid"":1,""active"":""get"",""devicetype"":""DISPLAY"",""command"":""ActiveHours"",""options"":{}},{""tid"":2,""active"":""get"",""devicetype"":""DISPLAY"",""command"":""Brightnesslevel"",""options"":{}}]}";

            string jsonFwDisplay = @"{""sid"":""1728273741"",""req"":[{""tid"":1,""active"":""fw"",""devicetype"":""DISPLAY"",""options"":{}}]}";
            string jsonfwdock = @"{""sid"":""1728273741"",""req"":[{""tid"":1,""active"":""fw"",""devicetype"":""DOCK"",""options"":{}}]}";
            string jsonfwkb = @"{""sid"":""1728273741"",""req"":[{""tid"":1,""active"":""fw"",""devicetype"":""Keyboard"",""options"":{}}]}";
            string jsonfwmouse = @"{""sid"":""1728273741"",""req"":[{""tid"":1,""active"":""fw"",""devicetype"":""MOUSE"",""options"":{}}]}";

            string jsonDevice = @"{""sid"":""1728380239"",""req"":[{""tid"":1,""active"":""get"",""devicetype"":""display"",""command"":""ConnectedDevices"",""options"":{}}]}";
            string jsonDeviceData = @"{""sid"":""1728380255"",""req"":[{""tid"":1,""active"":""get"",""devicetype"":""APP"",""command"":""DeviceData"",""options"":{}}]}";


            string jsonReport = @"{""sid"":""1728647205"",""req"":[{""tid"":1,""active"":""get"",""devicetype"":""APP"",""command"":""DiagnosticsReport"",""value"":""C:\\temp"",""options"":{}}]}";

            string deviceconfig = "{\r\n  \"Index\": \"1\",\r\n  \"DeviceType\": \"Display\",\r\n  \"Model\": \"DELLC2722DE\",\r\n  \"SerialNumber\": \"808596812\",\r\n  \"ServiceTag\": \"CN073K0\",\r\n  \"Manufacturer\": \"Dell\",\r\n  \"ManufacturingYear\": \"2021\",\r\n  \"ManufacturingWeek\": \"ISO week 3\",\r\n  \"FirmwareVersion\": \"M3T112\",\r\n  \"MonitorActiveHour\": \"713 hours\",\r\n  \"DisplayTechnologyType\": \"LCD (active matrix)\",\r\n  \"ScreenSize\": \"600 x 340 mm (27.15 in)\",\r\n  \"OptimalResolution\": \"2560 x 1440 at 60.00Hz\",\r\n  \"Resolution\": \"1920 x 1200 at 120.00Hz\",\r\n  \"ActiveInputSource\": \"USB-C\",\r\n  \"ColorPreset\": \"Standard/Native\",\r\n  \"ScreenOrientation\": \"Landscape\",\r\n  \"BrightnessLevel\": \"90%\",\r\n  \"ContrastLevel\": \"90%\",\r\n  \"LuminanceLevel\": \"N/A\",\r\n  \"AutoBrightness\": \"off\",\r\n  \"AutoBrightnessRangeLevel\": \"N/A\",\r\n  \"AutoColorTemp\": \"off\",\r\n  \"PrimaryMonitorForSync\": \"off\",\r\n  \"AspectRatio\": \"16:9\",\r\n  \"USB_CPrioritization\": \"NOT SUPPORT\",\r\n  \"ColorManagement\": \"N/A\",\r\n  \"SpeakerMicrophone\": \"N/A\",\r\n  \"SpeakerVolume\": \"24\",\r\n  \"MicrophoneControl\": \"N/A\",\r\n  \"Uniformity\": \"N/A\",\r\n  \"PowerNap\": \"Off\",\r\n  \"OSD_language\": \"English\",\r\n  \"PID\": \"DEL421F\"\r\n}";
            // string deviceConfig = await ReadJsonFileAsync("DeviceConfig");
            string jsonDeviceConfig2 = "{\"sid\":\"1728380251\",\"req\":[{\"tid\":1,\"active\":\"set\",\"devicetype\":\"display\",\"command\":\"DeviceConfiguration\",\"value\":" + deviceconfig + ",\"options\":{}}]}";

            string configless = "{\"Index\": \"1\",\"DeviceType\": \"Display\",\"BrightnessLevel\": \"90%\",\"ContrastLevel\": \"90%\"}";
            string jsondeviceconfig3 = "{\"sid\":\"1728380278\",\"req\":[{\"tid\":1,\"active\":\"set\",\"devicetype\":\"display\",\"command\":\"DeviceConfiguration\",\"value\":" + configless + ",\"options\":{}}]}";

            string test = @"{""sid"":""1729840262"",""req"":[{""tid"":1,""active"":""get"",""devicetype"":""APP"",""command"":""DiagnosticsReport"",""value"":""c:\temp"",""options"":{""index"":""1"",""servicetag"":""aaaaa"",""devicemodel"":""dell ea"",""uod"":true,""forcewithnotice"":true,""updatesilent"":true,""minversion"":""1.0.0.5""}},{""tid"":2,""active"":""get"",""devicetype"":""DISPLAY"",""command"":""ConnectedDevices"",""options"":{}}]}";

            string jsondevicedatadisplay = @"{""sid"":""1728380255"",""req"":[{""tid"":1,""active"":""get"",""devicetype"":""display"",""command"":""DeviceData"",""options"":{}}]}";

            string fwdisplaytest = @"{""sid"":""1730373296"",""req"":[{""tid"":1,""active"":""fw"",""devicetype"":""display"",""value"":""forcewithnotice"",""options"":{""index"":""1"",""servicetag"":""aaaaa"",""model"":""dell ea"",""minversion"":""1.0.0.5""}}]}";
            string testlock = @"{""sid"":""1730375319"",""req"":[{""tid"":1,""active"":""lock"",""devicetype"":""app"",""command"":""inappupdate"",""options"":{}},{""tid"":2,""active"":""unlock"",""devicetype"":""app"",""command"":""telemetryconsent"",""value"":""True"",""options"":{}}]}";
            
            
            RemoteRequestArgs cmaRequest = new RemoteRequestArgs();

            _CMAManagerPlugin.Notify += Notification;
            _CMAManagerPlugin.DisplayConnected += DisplayConnected;
            _CMAManagerPlugin.DisplayDisconnected += DisplayDisconnected;
            Console.WriteLine("Subscribe Event Success");

            // manager.Info(json);

            while (isRunning)
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


                Console.WriteLine("21. Show 2 task.");
                Console.WriteLine("22. DeviceConfiguration with only few attributes.");
                Console.WriteLine("23. Show DeviceData - Display.");

                Console.WriteLine("31. Show Fwupdate Display with options.");
                Console.WriteLine("32. Show Lock/Unlock.");

                Console.WriteLine("0. Exit.");

                Console.WriteLine("Enter the number to run ?");
                string input = Console.ReadLine() ?? string.Empty;
                requestNumList.Clear();
                stopwatch.Reset();
                switch (Convert.ToInt32(input))
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
                        cmaRequest.remote_request = jsongetdisplaymulti;
                        break;

                    case 7:
                        cmaRequest.remote_request = jsonfwdock;
                        break;

                    case 8:
                        cmaRequest.remote_request = jsonfwkb;
                        break;

                    case 9:
                        cmaRequest.remote_request = jsonfwmouse;
                        break;

                    case 21:
                        cmaRequest.remote_request = test;
                        break;

                    case 22:
                        cmaRequest.remote_request = jsondeviceconfig3;
                        break;

                    case 23:
                        cmaRequest.remote_request = jsondevicedatadisplay;
                        break;

                    case 31:
                        cmaRequest.remote_request = fwdisplaytest;
                        break;

                    case 32:
                        cmaRequest.remote_request = testlock;
                        break;

                    default:
                        isRunning = false;
                        break;
                }
                stopwatch.Start();
                Console.WriteLine($"json String = {cmaRequest.remote_request}");
                _CMAManagerPlugin.Info(cmaRequest);
                while (!isresponse) { }
            }
        }

        private void InitializeCMAManagerPlugin()
        {
            if (_CMAManagerPlugin != null)
                return;
            Console.WriteLine($"{nameof(PluginsStarted)} arrived for {nameof(IRemoteManagement)}");

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

        private static void Notification(object? sender, NotifyArgs e)
        {
            Console.WriteLine("CMA Notification Alert");
            Console.WriteLine("CMA Notification Alert event type : " + e.eventType);
            Console.WriteLine("CMA Notification Alert notification : " + e.notification);
            isresponse = true;
            stopwatch.Stop();
            Console.WriteLine($"Request finished in {stopwatch.Elapsed.TotalSeconds} seconds.");
        }

        private static void DisplayConnected(object? sender, NotifyArgs e)
        {
            Console.WriteLine("CMA DisplayConnected Alert");
            Console.WriteLine("CMA DisplayConnected Alert eventtype : " + e.eventType);
            Console.WriteLine("CMA DisplayConnected Alert notification : " + e.notification);

            isresponse = true;
        }

        private static void DisplayDisconnected(object? sender, NotifyArgs e)
        {

            Console.WriteLine("CMA DisplayDisconnected Alert");
            Console.WriteLine("CMA DisplayDisconnected Alert eventtype : " + e.eventType);
            Console.WriteLine("CMA DisplayDisconnected Alert notification : " + e.notification);

            isresponse = true;
        }

        #endregion
    }
}