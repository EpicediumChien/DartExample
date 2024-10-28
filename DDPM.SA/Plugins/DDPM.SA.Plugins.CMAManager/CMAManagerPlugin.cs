using DDPM.SA.Common;
using Dell.Client.Framework.Common.Annotations;
using Dell.Client.Framework.Interfaces;
using Microsoft;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.Common.PluginConditions;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Threading.Tasks;
using System.Xml.XPath;
using System.Threading;
using Windows.UI.Composition.Interactions;
using static DDPM.SA.Common.ICLICommandTable;
using Microsoft.VisualBasic.Logging;
using StreamJsonRpc;
using Newtonsoft.Json.Linq;
using static DDPM.SA.Plugins.CMAManager.Params;
using static DDPM.SA.Plugins.CMAManager.CMAManagerPlugin;
using Dell.Client.Framework.UX.WPF;
using VcpCore.Common;
using IDs = DDPM.SA.Common.IDs;
using DDPM.SA.Common.Interfaces;
using DDPM.RemoteManagement.Common.Interfaces;
using System.Windows.Documents;
using System.Collections;
using Newtonsoft.Json;

namespace DDPM.SA.Plugins.CMAManager
{
    [Plugin(IDs.CMA_Manager_Plugin, pluginName, PluginOrderGroupType.Core, Version = pluginVersion)]
    [Descriptor(Description = pluginDescription)]
    [Publisher(Name = publisherCompany, Website = publisherWebsite, Support = publisherSupport)]
    [PublishedUnelevatedInterface(new[] { typeof(ICMAManagerSA) })] //for DeviceManager of user subagent
    [PublishedInterface(new[] { typeof(IRemoteManagement) })] //for CLI subagent
    public class CMAManagerPlugin : BaseAgentPlugin, IDisposableObservable, ICMAManagerSA, IRemoteManagement
    {
        private static List<RemoteManagementResult> _result_list = new List<RemoteManagementResult>();
        private readonly object _resultLock = new object();

        #region Basic code for plugin
        public const string PluginLogId = "DDPMRemoteManager";

        private Queue<TaskInfo> taskInfoQueue = new Queue<TaskInfo>();
        private List<NotifyArgs> notifyArgsList = new List<NotifyArgs>();

        #region Private Members

        private const string pluginName = "DDPMRemoteManagerPlugin";
        private const string pluginVersion = "1.0.0";
        private const string pluginDescription = "This plugin implements Remote Manager Plugin.";
        private const string publisherCompany = "Wistron";
        private const string publisherWebsite = "https://www.wistron.com";
        private const string publisherSupport = "This plugin implements Remote Manager Plugin.";

        private IAgent _agent;
        private bool _IsAdministrator = ProcessSecurityHelperWrapper.IsCurrentProcessRunningElevated();

        private ICliManagerIT _CliManagerPlugin;
        private readonly object _PluginConditionLock_CliManager = new object();
        private List<string> commandinputs = new List<string>();

        // add @ 20241023 device manager for check connect disconnect devices
        private DeviceControlPannel deviceControlPannel;

        private enum log_type
        {
            info = 0,
            error
        }

        #endregion

        #region Constructor

        public CMAManagerPlugin(IAgent agent) : base(agent, PluginLogId)
        {
            _agent = agent;
            deviceControlPannel = new DeviceControlPannel();

            WriteLog($"CMA ManagerPlugin constructor ...(Admin:{_IsAdministrator})");
        }
        #endregion

        #region Overriding methods

        protected override void OnPluginStarting()
        {
            _agent.PluginManager.PluginsStarted += PluginManagerOnPluginsStarted;

            //Microsoft.Win32.SystemEvents.DisplaySettingsChanged += SystemEvents_DisplaySettingsChanged;

            PluginCondition = new PluginStartedCondition();
            WriteLog("[CMA Manager plugin] report started");
            InitializeCliManagerPlugin();
            //InitializeSettingsITPlugin();
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
                }

                IsDisposed = true;
            }
            base.Dispose(disposing);
        }
        #endregion

        #region Event Handler
        public event EventHandler<List<NotifyArgs>> Notify;
        public event EventHandler<NotifyArgs> DisplayConnected;
        public event EventHandler<NotifyArgs> DisplayDisconnected;
        private void PluginManagerOnPluginsStarted(object sender, PluginsStartedEventArgs e)
        {
            if (e == null)
                return;
            if (e.ChangedPlugins == null)
                return;
            if (e.ChangedPlugins.Any() == false)
                return;

            //if (e.ChangedPlugins.OfType<ISettingsManagerIT>().Any())
            //{
            //    WriteLog("[Info] Settings Manager IT plugin with ISettingsManagerIT started.");
            //}

/*            if (e.ChangedPlugins.OfType<ICMAManagerSA>().Any())
            {
                WriteLog("[Info] CMA Manager plugin with ICMAManagerSA started.");
            }

            if (e.ChangedPlugins.OfType<ICMAManagerIT>().Any())
            {
                WriteLog("[Info]  CMA Manager plugin with ICMAManagerIT started.");
            }*/

            if(e.ChangedPlugins.OfType<ICliManagerIT>().Any())
            {
                WriteLog("[Info]  CLI Manager plugin with ICliManagerIT started.");
                InitializeCliManagerPlugin();
            }
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
            text = "[CMA Manager] " + text;
            Console.WriteLine(text);
            if (log_type == log_type.info)
                Log.Info(text);
            else
                Log.Error(text);
        }
        #endregion
        #endregion

        #region IRemoteManagement implementation
        internal struct TaskInfo {
            public string sid;
            public string gid;
            public int tid;
            public int eventtype;
            public string command;
            public string jsonconfig;
        }

        private string createCommandGet(CmaCommand.CmaTask task)
        {
            string command = string.Empty;

            command = command + ("get ");

            if (Params.App.ConnectedDevices.ToLower().Equals(task.command.ToLower()))
            {
                command = command + ("app=" + task.command);
                command = command + (" value=" + task.devicetype);
            }
            else if (Params.App.DeviceData.ToLower().Equals(task.command.ToLower()))
            {
                command = command + ("app=" + task.command);

                if (!Params.DeviceType.APP.ToLower().Equals(task.devicetype.ToLower()))
                {
                    command = command + (" value=" + task.devicetype);
                }
                
            }
            else
            {
                command = command + (task.devicetype + "=" + task.command);

                if (task.value != null && task.value.Length > 0)
                {
                    command = command + (" value=" + task.value);
                }
            }


            CmaCommand.CmaTaskOption option = new CmaCommand.CmaTaskOption(task.options);

            if (option.index != null && option.index.Length > 0)
            {
                command = command + (" index=" + option.index);
            }

            if (option.servicetag != null && option.servicetag.Length > 0)
            {
                command = command + (" servicetag=" + option.servicetag);
            }

            if (option.modelname != null && option.modelname.Length > 0)
            {
                command = command + (" model=" + option.modelname);
            }

            return command;
        }

        private string createCommandSet(CmaCommand.CmaTask task)
        {
            string command = string.Empty;

            command = command + ("set ");

            if (!Params.App.DeviceConfiguration.ToLower().Equals(task.command.ToLower()))
            {
                command = command + (task.devicetype + "=" + task.command);
                command = command + (" value=" + task.value);
            }
            else
            {
                command = command + ("app=" + task.command);
                command = command + (" value=" + task.devicetype + "," + ("x:\\config.json"));
            }

            CmaCommand.CmaTaskOption option = new CmaCommand.CmaTaskOption(task.options);

            if (option.index != null && option.index.Length > 0)
            {
                command = command + (" index=" + option.index);
            }

            if (option.servicetag != null && option.servicetag.Length > 0)
            {
                command = command + (" servicetag=" + option.servicetag);
            }

            if (option.modelname != null && option.modelname.Length > 0)
            {
                command = command + (" model=" + option.modelname);
            }

            return command;
        }

        private string createCommandFw(CmaCommand.CmaTask task)
        {

            const string ForceWithNotice = "forcewithnotice";
            const string ForceWithNonotice = "forcewithnonotice";
            const string Defer = "defer";

            string command = string.Empty;

            command = command + ("set ");

            if (Params.DeviceType.DOCK.ToLower().Equals(task.devicetype.ToLower()))
            {
                command = command + ("dock=silentfwupdate");
            }
            else
            {
                command = command + ("app=firmwareupdate");
                //command = command + (" value=" + task.devicetype + ",forcewithnotice");

                command = command + (" value=" + task.devicetype);

                bool hasOption = false;

                if (ForceWithNotice.ToLower().Equals(task.value.ToLower()))
                {
                    command = command + (",forcewithnotice");
                    hasOption = true;
                }

                if (ForceWithNonotice.ToLower().Equals(task.value.ToLower()))
                {
                    command = command + (",forcewithnonotice");
                    hasOption = true;
                }

                if (Defer.ToLower().Equals(task.value.ToLower()))
                {
                    command = command + (",Defer");
                    hasOption = true;
                }

                if (!hasOption)
                {
                    command = command + (",forcewithnotice");
                }

                CmaCommand.CmaTaskOption option = new CmaCommand.CmaTaskOption(task.options);

/*                if (option.forcewithnotice != null && option.forcewithnotice)
                {
                    command = command + (",forcewithnotice");
                    hasOption = true;
                }*/

/*                if (option.forcewithnonotice != null && option.forcewithnonotice)
                {
                    command = command + (",forcewithnonotice");
                    hasOption = true;
                }

                if (option.defer != null && option.defer)
                {
                    command = command + (",defer");
                    hasOption = true;
                }*/

                
            }

            

            return command;
        }

        private string createCommandLock(Boolean isLock, CmaCommand.CmaTask task)
        {
            string command = string.Empty;

            command = command + ("set ");

/*            if (!Params.App.DeviceConfiguration.ToLower().Equals(task.command.ToLower()))
            {
                command = command + (task.devicetype + "=" + task.command);
                command = command + (" value=" + task.value);
            }
            else
            {
                command = command + ("app=" + task.command);
                command = command + (" value=" + task.devicetype + "," + ("x:\\config.json"));
            }

            CmaCommand.CmaTaskOption option = new CmaCommand.CmaTaskOption(task.options);

            if (option.index != null && option.index.Length > 0)
            {
                command = command + (" index=" + option.index);
            }

            if (option.servicetag != null && option.servicetag.Length > 0)
            {
                command = command + (" servicetag=" + option.servicetag);
            }

            if (option.modelname != null && option.modelname.Length > 0)
            {
                command = command + (" model=" + option.modelname);
            }*/

            return command;
        }

        private void initCommandTask(String guid, String request)
        {

            CmaCommand cmd = new CmaCommand(guid, request);

            WriteLog($"[CMA] initCommandTask request = {request}");

            List<CmaCommand.CmaTask> tasks = new List<CmaCommand.CmaTask>();

            foreach (var s in cmd.req)
            {
                tasks.Add(new CmaCommand.CmaTask(cmd.sid, s.ToString().ToLower()));
            }

            foreach (CmaCommand.CmaTask task in tasks)
            {
                string command = "";
                int eventtype = 0;

                if ("get".Equals(task.active.ToLower()))
                {
                    eventtype = 1;
                    command = createCommandGet(task);
                }

                if ("set".Equals(task.active.ToLower()))
                {
                    eventtype = 2;
                    command = createCommandSet(task);

                    
                }

                if ("fw".Equals(task.active.ToLower()))
                {
                    eventtype = 5;

                    if (Params.DeviceType.DOCK.ToLower().Equals(task.devicetype.ToLower()))
                    {
                        command = command + ("set ");
                        command = command + ("dock=silentfwupdate");
                    }
                    else
                    {
                        command = command + ("set ");
                        command = command + ("app=firmwareupdate");
                        command = command + (" value=" + task.devicetype + ",forcewithnotice");
                    }

                }

                commandinputs.Add(command);

                TaskInfo taskInfo = new TaskInfo();
                taskInfo.sid = task.sid;
                taskInfo.gid = guid;
                taskInfo.tid = task.tid;
                taskInfo.eventtype = eventtype;
                taskInfo.command = command;
                taskInfo.jsonconfig = task.value;       

                taskInfoQueue.Enqueue(taskInfo);
                int lastProcessedTid = -1;
                while (taskInfoQueue.Count > 0)
                {
                    TaskInfo curTaskInfo = taskInfoQueue.Peek();
                    if (lastProcessedTid != curTaskInfo.tid)
                    {
                        lastProcessedTid = curTaskInfo.tid;
                        _ = Task.Run(async() => await ProcessQueueAsync(curTaskInfo));
                    }
                }
                EventHandler<List<NotifyArgs>> Handler = Notify;
                Handler.Invoke(this, notifyArgsList);
            }
        }

        private async Task ProcessQueueAsync(TaskInfo taskInfo)
        {
            Console.WriteLine($"taskInfoQueue = {taskInfoQueue.Count}");
            WriteLog($"[CMA]  before runCommandTask, taskinfo.sid = {taskInfo.sid} ; taskinfo.gid = {taskInfo.gid} ; taskinfo.tid = {taskInfo.tid} ; taskinfo.eventtype = {taskInfo.eventtype} ; taskinfo.command = {taskInfo.command}");
            await runCommandTaskAsync(taskInfo);
            Console.WriteLine($"Processing tid: {taskInfo.tid} TargetFeature: {taskInfo.command}");
        }

        private async Task<CLIEventResult> runCommandTaskAsync(object _taskinfo)
        {
            TaskInfo taskInfo = (TaskInfo)_taskinfo;

            if (null != _CliManagerPlugin && !string.IsNullOrEmpty(taskInfo.command))
            {
                ICLICommandTable iCLICommandTable = new ICLICommandTable(null);
                CommandLineInput commandLineInput = iCLICommandTable.StringProcessing(taskInfo.command.Split(' '));
                commandLineInput.isCliRunAdmin = true;
                commandLineInput.jsonDeviceConfig = taskInfo.jsonconfig;

                Console.WriteLine("[CMA] runCommandTask taskinfo.command = " + taskInfo.command);

                //_CliManagerPlugin.PerformCommandLineRelay
                CLIEventResult cliResult = await _CliManagerPlugin.PerformCommandLineRelay(commandLineInput);
                Boolean isSuccess = false;

                string responseMsg = String.Empty;

                string responseResult = String.Empty;

                NotifyArgs args = new NotifyArgs();
                args.eventType = taskInfo.eventtype.ToString();


                try
                {

                    JObject jObject = JObject.Parse(cliResult.serialize_Json_response);

                    responseMsg = (string)jObject["Message"];
                    responseResult = (string)jObject["Result"];

                    if (responseResult.Equals("Success"))
                    {
                        isSuccess = true;
                    }

                    if (responseResult.Equals("PASS"))
                    {
                        isSuccess = true;
                    }

                    if (isSuccess)
                    {
                        args.notification = "{\"sid\": \"" + taskInfo.sid + "\",\"gid\": \"" + taskInfo.gid + "\",\"response\": [{\"tid\": " + taskInfo.tid + ",\"result\": 0,\"msg\": \"\",\"data\": [" + cliResult.serialize_Json_response + "]}]}";
                    }
                    else
                    {
                        args.notification = "{\"sid\": \"" + taskInfo.sid + "\",\"gid\": \"" + taskInfo.gid + "\",\"response\": [{\"tid\": " + taskInfo.tid + ",\"result\": " + Params.Response.STATUS_COMMAND_ERROR_FORMAT_OR_PARAMS + ",\"msg\": \"" + responseMsg + "\",\"data\": [" + cliResult.serialize_Json_response + "]}]}";
                    }
                }
                catch
                {
                    responseMsg = "Exception: Unknow Result";
                    args.notification = "{\"sid\": \"" + taskInfo.sid + "\",\"gid\": \"" + taskInfo.gid + "\",\"response\": [{\"tid\": " + taskInfo.tid + ",\"result\": 0,\"msg\": \"\",\"data\": [" + cliResult.serialize_Json_response + "]}]}";
                }

                Console.WriteLine("[CMA] runCommandTask args.notification = " + cliResult.command_guid_string + "\n args.notification = " + args.notification);
                //Console.WriteLine("[CMA] );

                OnEventNotify(args);
                return cliResult;
            }
            return new CLIEventResult();
        }
        
        public Task<RemoteManagementResult> Info(RemoteRequestArgs request)
        {
            // 
            Guid uniqueAgentGuid = Guid.NewGuid();


            //Assign request ID per call
            RemoteManagementResult result = new RemoteManagementResult();
            result.cma_request_id = uniqueAgentGuid;

            if (request == null)
            {
                result.message = "Null parameter";
                result.output_result = "FAIL";
                return Task.FromResult(result);
            }

            WriteLog($"[CMA] initCommandTask request.cma_request = {request.remote_request}]");

            try
            {
                initCommandTask(uniqueAgentGuid.ToString(), request.remote_request);
            }
            catch (Exception e) {

                NotifyArgs args = new NotifyArgs();
                args.eventType = Params.EventType.UNKNOW_ERROR.ToString();
                args.notification = e.ToString() + "; " + request.remote_request;
                OnEventNotify(args);
            }

            result.message = "Request Got";
            result.output_result = "Executing";

            return Task.FromResult(result);


        }

        private RemoteManagementResult response_timeout(Guid id)
        {
            RemoteManagementResult result = new RemoteManagementResult();
            result.cma_request_id = id;
            result.message = "Wait for result timeout";
            result.output_result = "FAIL";
            return result;
        }

        private void OnInvokeCMARequestEvent(RemoteRequestArgs request, Guid id)
        {
            EventHandler<CMAEventArgs> Handler = CMARequestEvent;
            if (Handler != null)
            {
                CMAEventArgs input = new CMAEventArgs();
                input.input_param = "By your design here";
                input.cma_request_id = id;

                Handler.Invoke(this, input);
            }
        }
        #endregion


        private void InitializeCliManagerPlugin()
        {
            if (_CliManagerPlugin != null)
                return;

            _CliManagerPlugin = _agent.PluginManager.FindPluginByType<ICliManagerIT>(PluginResolution.Dynamic);

            if (_CliManagerPlugin is IFrameworkPluginConditionNotification pluginCondition)
            {
                pluginCondition.PluginConditionChangeHandler += OnCliManagerPluginConditionChangeHandler;
                GetCurrentCliManagerPluginCondition();
            }
        }

        private void OnCliManagerPluginConditionChangeHandler(object sender, EventArgs e)
        {
            GetCurrentCliManagerPluginCondition();
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
                        
                       /* if (!relay_registered && _DevManagerPlugin != null)
                        {
                            DoRelayRegister();
                        }*/
                    }
                }
            });
        }


        #region ICMAManagerSA implementation
        public Task WriteResult(RemoteManagementResult result)
        {
            lock(_resultLock)
            {
                _result_list.Add(result);
            }
            return Task.CompletedTask;
        }

        public event EventHandler<CMAEventArgs> CMARequestEvent;

        public Task Update_DeviceChanged(CMADeviceChanges data)
        {
            WriteLog("[ICMAManagerSA] Update_DeviceChanged() executed");

            // TODO: implement decice connect/disconnect information
            WriteLog("[ICMAManagerSA] Update_DeviceChanged() executed data.type = " + data.type);

            foreach (MonitorInfo info in data.mos) {
                WriteLog("[ICMAManagerSA] Update_DeviceChanged() executed MonitorInfo ToString = " + info.ToString());
                WriteLog("[ICMAManagerSA] Update_DeviceChanged() executed MonitorInfo AliasDeviceName = " + info.AliasDeviceName);
                WriteLog("[ICMAManagerSA] Update_DeviceChanged() executed MonitorInfo Index = " + info.Index);
                WriteLog("[ICMAManagerSA] Update_DeviceChanged() executed MonitorInfo CapabilityString = " + info.CapabilityString);
                WriteLog("[ICMAManagerSA] Update_DeviceChanged() executed MonitorInfo DisplayName = " + info.DisplayName);
                WriteLog("[ICMAManagerSA] Update_DeviceChanged() executed MonitorInfo DDCisON = " + info.DDCisON);
                WriteLog("[ICMAManagerSA] Update_DeviceChanged() executed MonitorInfo edid = " + info.edid);
                WriteLog("[ICMAManagerSA] Update_DeviceChanged() executed MonitorInfo FwVersion = " + info.FwVersion);
                WriteLog("[ICMAManagerSA] Update_DeviceChanged() executed MonitorInfo inputSource = " + info.inputSource);
                WriteLog("[ICMAManagerSA] Update_DeviceChanged() executed MonitorInfo inputCable = " + info.inputCable);
                WriteLog("[ICMAManagerSA] Update_DeviceChanged() executed MonitorInfo modelName = " + info.modelName);
                WriteLog("[ICMAManagerSA] Update_DeviceChanged() executed MonitorInfo series = " + info.series);
                WriteLog("[ICMAManagerSA] Update_DeviceChanged() executed MonitorInfo edid.PID = " + info.edid.PID);
                WriteLog("[ICMAManagerSA] Update_DeviceChanged() executed MonitorInfo edid.ModelNam = " + info.edid.ModelName);
                WriteLog("[ICMAManagerSA] Update_DeviceChanged() executed MonitorInfo edid.SerialNumber = " + info.edid.SerialNumber);
                WriteLog("[ICMAManagerSA] Update_DeviceChanged() executed MonitorInfo edid.ServiceTag = " + info.edid.ServiceTag);
                WriteLog("[ICMAManagerSA] Update_DeviceChanged() executed MonitorInfo modelName =========================================");

            }

           
            NotifyArgs args = deviceControlPannel.OnDeviceChnaged(data);

            args.notification = "{\"sid\": \"\",\"gid\": \"\",\"response\": [{\"tid\": ,\"result\": 0,\"msg\": \"\",\"data\": [" + args.notification + "]}]}";

            if (Params.EventType.DISPLAY_CONNECT.ToString().Equals(args.eventType))
            {
                OnEventDisplayConnect(args);
            }
            else 
            {
                OnEventDisplayDisconnect(args);
            }

            //OnEventNotify(args);

            return Task.CompletedTask;
        }
        #endregion

        private void OnEventNotify(NotifyArgs e)
        {
            taskInfoQueue.Dequeue();
            WriteLog($"[CMA] OnEventNotify e.notification = {e.notification}");
            notifyArgsList.Add(e);
        }

        private void OnEventDisplayConnect(NotifyArgs e)
        {
            EventHandler<NotifyArgs> Handler = DisplayConnected;
            if (Handler != null)
            {
                WriteLog($"[CMA] OnEventDisplayConnect e.notification = {e.notification}");
                Handler.Invoke(this, e);
            }
        }

        private void OnEventDisplayDisconnect(NotifyArgs e)
        {
            EventHandler<NotifyArgs> Handler = DisplayDisconnected;
            if (Handler != null)
            {
                WriteLog($"[CMA] OnEventDisplayDisonnect e.notification = {e.notification}");
                Handler.Invoke(this, e);
            }
        }
    }
}
