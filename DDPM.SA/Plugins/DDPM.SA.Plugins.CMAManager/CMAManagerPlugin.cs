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

namespace DDPM.SA.Plugins.CMAManager
{
    [Plugin(IDs.CMA_Manager_Plugin, pluginName, PluginOrderGroupType.Core, Version = pluginVersion)]
    [Descriptor(Description = pluginDescription)]
    [Publisher(Name = publisherCompany, Website = publisherWebsite, Support = publisherSupport)]
    [PublishedUnelevatedInterface(new[] { typeof(ICMAManagerSA) })] //for DeviceManager of user subagent
    [PublishedInterface(new[] { typeof(ICMAManagerIT) })] //for CLI subagent
    public class CMAManagerPlugin : BaseAgentPlugin, IDisposableObservable, ICMAManagerSA, ICMAManagerIT
    {
        private static List<CMAResult> _result_list = new List<CMAResult>();
        private readonly object _resultLock = new object();

        #region Basic code for plugin
        public const string PluginLogId = "CMAManager";

        #region Private Members

        private const string pluginName = "CMAManagerPlugin";
        private const string pluginVersion = "1.0.0";
        private const string pluginDescription = "This plugin implements CMA Manager Plugin.";
        private const string publisherCompany = "Wistron";
        private const string publisherWebsite = "https://www.wistron.com";
        private const string publisherSupport = "This plugin implements CMA Manager Plugin.";

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

            Microsoft.Win32.SystemEvents.DisplaySettingsChanged += SystemEvents_DisplaySettingsChanged;

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

        private void SystemEvents_DisplaySettingsChanged(object sender, EventArgs e)
        {
            Console.WriteLine("[CMA] SystemEvents_DisplaySettingsChanged " + DateTime.Now);

           // Thread.Sleep(10000);

/*            NotifyArgs args = new NotifyArgs();
            args.eventtype = Params.EventType.DISPLAY_CONNECT.ToString();
            args.notification = "DisplaySettingsChanged";
            OnEventNotify(args);*/
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
        internal struct TaskInfo {
            public string sid;
            public string gid;
            public int tid;
            public int eventtype;
            public string command;
        }

        //private List<TaskInfo> taskInfos = new List<TaskInfo>();
        //

        private void initCommandTask(String guid, String request)
        {
            List<TaskInfo> taskInfos = new List<TaskInfo>();

            CmaCommand cmd = new CmaCommand(guid, request);

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

                if ("get".Equals(task.active))
                {
                    eventtype = Params.EventType.GET;
                    command = command + ("get ");
                    command = command + (task.devicetype + "=" + task.command);
                    if ((command.ToLower()).Equals(Params.App.ConnectedDevices.ToLower()))
                    {
                        command = command + ("value=display");
                    }
                }

                if ("set".Equals(task.active))
                {
                    eventtype = Params.EventType.SET;
                    command = command + ("set ");
                    command = command + (task.devicetype + "=" + task.command);
                    command = command + (" value=" + task.value);
                }

                if ("fw".Equals(task.active))
                {
                    eventtype = Params.EventType.FW;
                    command = command + ("set ");
                    command = command + ("app=firmwareupdate");
                    if (!Params.DeviceType.DISPLAY.Equals(task.devicetype))
                    {
                        command = command + (" value=" + task.devicetype);
                    }
                }

                commandinputs.Add(command);

                TaskInfo taskinfo = new TaskInfo();
                taskinfo.sid = task.sid;
                taskinfo.gid = guid;
                taskinfo.tid = task.tid;
                taskinfo.eventtype = eventtype;
                taskinfo.command = command; 

                taskInfos.Add(taskinfo);


                /*if (null != _CliManagerPlugin)
                {
                    ICLICommandTable iCLICommandTable = new ICLICommandTable(null);
                    CommandLineInput commandLineInput = iCLICommandTable.StringProcessing(command.Split(' '));

                    //_CliManagerPlugin.PerformCommandLineRelay
                    CLIEventResult cliResult = _CliManagerPlugin.PerformCommandLineRelay(commandLineInput).Result;

                    JObject jObject = JObject.Parse(cliResult.serialize_Json_response);

                    string responseMsg = (string)jObject["Message"];
                    string responseResult = (string)jObject["Result"];

                    Boolean isSuccess = false;
                    if (responseResult.Equals("Success"))
                    {
                        isSuccess = true;
                    }


                    NotifyArgs args = new NotifyArgs();
                    args.eventtype = eventtype.ToString();
                    //args.notification = cliResult.serialize_Json_response;
                    //args.notification = "{\r\n   \"sid\": \"" + task.sid + "\",\r\n   \"gid\": \"" + guid + "\",\r\n   \"response\": [\r\n      \r\n      {\r\n         \"id\": " + task.tid + ",\r\n         \"result\": 0,\r\n         \"msg\": \"\",\r\n         \"data\": [\r\n            " + cliResult.serialize_Json_response + "\r\n         ]\r\n      }\r\n   ]\r\n}";
                    if (isSuccess)
                    {
                        args.notification = "{\"sid\": \"" + task.sid + "\",\"gid\": \"" + guid + "\",\"response\": [{\"tid\": " + task.tid + ",\"result\": 0,\"msg\": \"\",\"data\": [" + cliResult.serialize_Json_response + "]}]}";

                    }
                    else
                    {
                        args.notification = "{\"sid\": \"" + task.sid + "\",\"gid\": \"" + guid + "\",\"response\": [{\"tid\": " + task.tid + ",\"result\": " + Params.Response.STATUS_COMMAND_ERROR_FORMAT_OR_PARAMS + ",\"msg\": \"" + responseMsg + "\",\"data\": []}]}";

                    }
                    OnEventNotify(args);
                }*/

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

                //_CliManagerPlugin.PerformCommandLineRelay
                CLIEventResult cliResult = _CliManagerPlugin.PerformCommandLineRelay(commandLineInput).Result;

                JObject jObject = JObject.Parse(cliResult.serialize_Json_response);

                string responseMsg = (string)jObject["Message"];
                string responseResult = (string)jObject["Result"];

                Boolean isSuccess = false;
                if (responseResult.Equals("Success"))
                {
                    isSuccess = true;
                }


                NotifyArgs args = new NotifyArgs();
                args.eventtype = taskinfo.eventtype.ToString();
                //args.notification = cliResult.serialize_Json_response;
                //args.notification = "{\r\n   \"sid\": \"" + task.sid + "\",\r\n   \"gid\": \"" + guid + "\",\r\n   \"response\": [\r\n      \r\n      {\r\n         \"id\": " + task.tid + ",\r\n         \"result\": 0,\r\n         \"msg\": \"\",\r\n         \"data\": [\r\n            " + cliResult.serialize_Json_response + "\r\n         ]\r\n      }\r\n   ]\r\n}";
                if (isSuccess)
                {
                    args.notification = "{\"sid\": \"" + taskinfo.sid + "\",\"gid\": \"" + taskinfo.gid + "\",\"response\": [{\"tid\": " + taskinfo.tid + ",\"result\": 0,\"msg\": \"\",\"data\": [" + cliResult.serialize_Json_response + "]}]}";
                }
                else
                {
                    args.notification = "{\"sid\": \"" + taskinfo.sid + "\",\"gid\": \"" + taskinfo.gid + "\",\"response\": [{\"tid\": " + taskinfo.tid + ",\"result\": " + Params.Response.STATUS_COMMAND_ERROR_FORMAT_OR_PARAMS + ",\"msg\": \"" + responseMsg + "\",\"data\": []}]}";
                }
                OnEventNotify(args);
            }
        }

        #region ICMAManagerIT implementation
        public Task<CMAResult> Info(CMARequestArgs request)
        {
            // 
            Guid uniqueAgentGuid = Guid.NewGuid();
            

            //Assign request ID per call
            CMAResult result = new CMAResult();
            result.cma_request_id = uniqueAgentGuid;

            if (request == null)
            {
                result.message = "Null parameter";
                result.output_result = "FAIL";
                return Task.FromResult(result);
            }

            initCommandTask(uniqueAgentGuid.ToString(), request.cma_request);

            /*foreach (string input in commandinputs) 
            {
                if (null != _CliManagerPlugin)
                {
                    ICLICommandTable iCLICommandTable = new ICLICommandTable(null);
                    CommandLineInput commandLineInput = iCLICommandTable.StringProcessing(input.Split(' '));

                    //_CliManagerPlugin.PerformCommandLineRelay
                    CLIEventResult cliResult = _CliManagerPlugin.PerformCommandLineRelay(commandLineInput).Result;

*//*                    NotifyArgs args = new NotifyArgs();
                    args.eventtype = e.ExitCode.ToString();
                    args.notification = e.response;
                    OnEventNotify(args);*//*
                }
            }*/

            /*CmaCommand cmd = new CmaCommand(uniqueAgentGuid.ToString(), request.cma_request);

            List<CmaCommand.CmaTask> tasks = new List<CmaCommand.CmaTask>();
            foreach (var s in cmd.req)
            {
                //Console.WriteLine(s.ToString());
                tasks.Add(new CmaCommand.CmaTask(cmd.sid, s.ToString()));
            }

            foreach (CmaCommand.CmaTask task in tasks)
            {

                string command = "";

                if ("get".Equals(task.active))
                {
                    command = command + ("get ");
                    command = command + (task.devicetype + "=" + task.command);
                }

                if ("set".Equals(task.active))
                {
                    command = command + ("set ");
                    command = command + (task.devicetype + "=" + task.command);
                }

                if ("fw".Equals(task.active))
                {
                    command = command + ("set ");
                    command = command + ("display=firmwareupdate");
                    if (!Params.DeviceType.DISPLAY.Equals(task.devicetype))
                    {
                        command = command + (" value=" + task.devicetype);
                    }
                }

                commandinputs.Add(command);

                if (null != _CliManagerPlugin)
                {
                    ICLICommandTable iCLICommandTable = new ICLICommandTable(null);
                    CommandLineInput commandLineInput = iCLICommandTable.StringProcessing(command.Split(' '));

                    //_CliManagerPlugin.PerformCommandLineRelay
                    CLIEventResult cliResult = _CliManagerPlugin.PerformCommandLineRelay(commandLineInput).Result;

                    NotifyArgs args = new NotifyArgs();
                    args.eventtype = cliResult.ExitCode.ToString();
                    args.notification = cliResult.serialize_Json_response;
                    args.notification = "{\r\n   \"sid\": \"" + task.sid + "\",\r\n   \"gid\": \"" + uniqueAgentGuid + "\",\r\n   \"response\": [\r\n      \r\n      {\r\n         \"id\": \"" + task.tid + "\",\r\n         \"result\": 0,\r\n         \"msg\": \"\",\r\n         \"data\": [\r\n            " + cliResult.serialize_Json_response + "\r\n         ]\r\n      }\r\n   ]\r\n}";
                    OnEventNotify(args);
                }

            }*/
            result.message = "Request Got";
            result.output_result = "Executing";

            return Task.FromResult(result);


            /*OnInvokeCMARequestEvent(request, result.cma_request_id);
            int time = 0;
            while(true)
            {
                Thread.Sleep(1000);
                time++;
                lock(_resultLock)
                {
                    int idx = _result_list.FindIndex(x => x.cma_request_id.Equals(result.cma_request_id));
                    if(idx >= 0)
                    {
                        result.message = _result_list[idx].message;
                        result.output_result = _result_list[idx].output_result;
                        return Task.FromResult(result);
                    }
                }
                if (time > 60)//use 60 sec as default
                    return Task.FromResult(response_timeout(result.cma_request_id));
            }*/
        }

        private CMAResult response_timeout(Guid id)
        {
            CMAResult result = new CMAResult();
            result.cma_request_id = id;
            result.message = "Wait for result timeout";
            result.output_result = "FAIL";
            return result;
        }

        private void OnInvokeCMARequestEvent(CMARequestArgs request, Guid id)
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
        public Task WriteResult(CMAResult result)
        {
            lock(_resultLock)
            {
                _result_list.Add(result);
            }
            return Task.CompletedTask;
        }

        public event EventHandler<CMAEventArgs> CMARequestEvent;
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

        public event EventHandler<NotifyArgs> Notify;
    }
}
