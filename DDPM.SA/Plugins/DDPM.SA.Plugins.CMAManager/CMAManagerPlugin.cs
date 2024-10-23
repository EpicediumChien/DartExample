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

        private void initCommandTask(String guid, String request)
        {
            List<TaskInfo> taskInfos = new List<TaskInfo>();

            CmaCommand cmd = new CmaCommand(guid, request);

            WriteLog($"[CMA] initCommandTask request = {request}]");

            List<CmaCommand.CmaTask> tasks = new List<CmaCommand.CmaTask>();

            foreach (var s in cmd.req)
            {
                //Console.WriteLine(s.ToString());
                tasks.Add(new CmaCommand.CmaTask(cmd.sid, s.ToString()));
            }

            foreach (CmaCommand.CmaTask task in tasks)
            {
                string command = "";
                int eventtype = 0;

                //Console.WriteLine("task.options = " + task.options);

                if ("get".Equals(task.active))
                {

                    eventtype = 1;
                    command = command + ("get ");

                    if (Params.App.ConnectedDevices.Equals(task.command))
                    {
                        command = command + ("app=" + task.command);
                        command = command + (" value=" + task.devicetype);
                    }
                    else
                    {
                        command = command + (task.devicetype + "=" + task.command);

                        if (task.value != null && task.value.Length > 0)
                        {
                            command = command + (" value=" + task.value);
                        }
                    }

                    
                }

                if ("set".Equals(task.active))
                {
                    eventtype = 2;
                    command = command + ("set ");
                    
                    if (!Params.App.DeviceConfiguration.Equals(task.command))
                    {
                        command = command + (task.devicetype + "=" + task.command);
                        command = command + (" value=" + task.value);
                    }
                    else
                    {
                        command = command + ("app=" + task.command);
                        command = command + (" value=" + task.devicetype + "," + ("x:\\config.json"));
                    }
                }

                if ("fw".Equals(task.active))
                {
                    eventtype = 5;
                    command = command + ("set ");
                    command = command + ("app=firmwareupdate");
                    command = command + (" value=" + task.devicetype + ",forcewithnotice");
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


                commandinputs.Add(command);

                TaskInfo taskinfo = new TaskInfo();
                taskinfo.sid = task.sid;
                taskinfo.gid = guid;
                taskinfo.tid = task.tid;
                taskinfo.eventtype = eventtype;
                taskinfo.command = command;
                taskinfo.jsonconfig = task.value;       

                Console.WriteLine("command = " + command);

                WriteLog($"[CMA] initCommandTask command = {command}]");

                taskInfos.Add(taskinfo);

            }

            foreach (TaskInfo taskinfo in taskInfos) {
                new Thread(runCommandTask).Start(taskinfo);
            }
        }

        private void runCommandTask(object _taskinfo)
        {
            TaskInfo taskinfo = (TaskInfo)_taskinfo;

            if (null != _CliManagerPlugin)
            {
                ICLICommandTable iCLICommandTable = new ICLICommandTable(null);
                CommandLineInput commandLineInput = iCLICommandTable.StringProcessing(taskinfo.command.Split(' '));
                commandLineInput.isCliRunAdmin = true;
                commandLineInput.jsonDeviceConfig = taskinfo.jsonconfig;

                Console.WriteLine("[CMA] runCommandTask taskinfo.command = " + taskinfo.command);

                //_CliManagerPlugin.PerformCommandLineRelay
                CLIEventResult cliResult = _CliManagerPlugin.PerformCommandLineRelay(commandLineInput).Result;
                Boolean isSuccess = false;

                string responseMsg = String.Empty;

                string responseResult = String.Empty;

                NotifyArgs args = new NotifyArgs();
                args.eventType = taskinfo.eventtype.ToString();


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
                        args.notification = "{\"sid\": \"" + taskinfo.sid + "\",\"gid\": \"" + taskinfo.gid + "\",\"response\": [{\"tid\": " + taskinfo.tid + ",\"result\": 0,\"msg\": \"\",\"data\": [" + cliResult.serialize_Json_response + "]}]}";
                    }
                    else
                    {
                        args.notification = "{\"sid\": \"" + taskinfo.sid + "\",\"gid\": \"" + taskinfo.gid + "\",\"response\": [{\"tid\": " + taskinfo.tid + ",\"result\": " + Params.Response.STATUS_COMMAND_ERROR_FORMAT_OR_PARAMS + ",\"msg\": \"" + responseMsg + "\",\"data\": [" + cliResult.serialize_Json_response + "]}]}";
                    }
                }
                catch
                {
                    responseMsg = "Exception: Unknow Result";
                    args.notification = "{\"sid\": \"" + taskinfo.sid + "\",\"gid\": \"" + taskinfo.gid + "\",\"response\": [{\"tid\": " + taskinfo.tid + ",\"result\": 0,\"msg\": \"\",\"data\": [" + cliResult.serialize_Json_response + "]}]}";
                }
                


                
                //args.notification = cliResult.serialize_Json_response;
                //args.notification = "{\r\n   \"sid\": \"" + task.sid + "\",\r\n   \"gid\": \"" + guid + "\",\r\n   \"response\": [\r\n      \r\n      {\r\n         \"id\": " + task.tid + ",\r\n         \"result\": 0,\r\n         \"msg\": \"\",\r\n         \"data\": [\r\n            " + cliResult.serialize_Json_response + "\r\n         ]\r\n      }\r\n   ]\r\n}";
                
                OnEventNotify(args);
            }
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
            
            return Task.CompletedTask;
        }
        #endregion
        private void OnEventNotify(NotifyArgs e)
        {
            EventHandler<NotifyArgs> Handler = Notify;
            if (Handler != null)
            {
                Handler.Invoke(this, e);
                //WriteLog($"CLIActionEvent Invoked: ID:{e.command_guid_string}");
            }
        }

        private void OnEventDisplayConnect(NotifyArgs e)
        {
            EventHandler<NotifyArgs> Handler = DisplayConnect;
            if (Handler != null)
            {
                Handler.Invoke(this, e);
                //WriteLog($"CLIActionEvent Invoked: ID:{e.command_guid_string}");
            }
        }

        private void OnEventDisplayDisConnect(NotifyArgs e)
        {
            EventHandler<NotifyArgs> Handler = DisplayDisConnect;
            if (Handler != null)
            {
                Handler.Invoke(this, e);
                //WriteLog($"CLIActionEvent Invoked: ID:{e.command_guid_string}");
            }
        }

        public event EventHandler<NotifyArgs> Notify;
        public event EventHandler<NotifyArgs> DisplayConnect;
        public event EventHandler<NotifyArgs> DisplayDisConnect;
    }
}
