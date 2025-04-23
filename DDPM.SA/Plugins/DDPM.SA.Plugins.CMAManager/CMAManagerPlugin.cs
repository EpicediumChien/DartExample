using DDPM.RemoteManagement.Common.Interfaces;
using DDPM.SA.Common;
using DDPM.SA.Common.Defer;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.Common.Annotations;
using Dell.Client.Framework.Common.PluginConditions;
using Dell.Client.Framework.Interfaces;
using Microsoft;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using VcpCore.Common;
using static DDPM.SA.Common.ICLICommandTable;
using static DDPM.SA.Plugins.CMAManager.CmdResponse;
using IDs = DDPM.SA.Common.IDs;

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
        private bool QueueProcessingFlag = false;
        private NotifyArgs notifyArgs = new NotifyArgs();

        #region Private Members

        private const string pluginName = "DDPMRemoteManagerPlugin";
        private const string pluginVersion = "1.0.0";
        private const string pluginDescription = "This plugin implements Remote Manager Plugin.";
        private const string publisherCompany = "Dell Technologies";
        private const string publisherWebsite = "https://www.dell.com";
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

            // add @ 20241213 stephen: init DeferControlPanel
            initDeferControlPanel();

            // add @ 20250117 stephen
            initFwJobControlPanel();

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
                    if (_CliManagerPlugin != null)
                        _CliManagerPlugin.CLIActionResult -= OnCLIManagerResultHandler;

                    _agent.PluginManager.PluginsStarted -= PluginManagerOnPluginsStarted;
                    _agent = null;
                }

                IsDisposed = true;
            }
            base.Dispose(disposing);
        }
        #endregion

        #region Event Handler
        public event EventHandler<NotifyArgs> Notify;
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

            if (e.ChangedPlugins.OfType<ICliManagerIT>().Any())
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
            if (Log != null)
            {
                if (log_type == log_type.info)
                    Log.Info(text);
                else
                    Log.Error(text);
            }
        }
        #endregion
        #endregion

        #region IRemoteManagement implementation
        internal struct TaskInfo
        {
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

                if (!task.devicetype.ToLower().Equals(Params.DeviceType.APP.ToLower()))
                {
                    command = command + (" value=" + task.devicetype);
                }

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

            if (option.model != null && option.model.Length > 0)
            {
                command = command + (" model=" + option.model);
            }

            return command;
        }
        private string createCommandSet(CmaCommand.CmaTask task)
        {
            CmaCommand.CmaTaskOption option = new CmaCommand.CmaTaskOption(task.options);

            string command = string.Empty;

            command = command + ("set ");

            // special case
            if (Params.App.DeviceConfiguration.ToLower().Equals(task.command.ToLower()))
            {
                if (String.IsNullOrEmpty(task.value) || task.value.Length <= 0)
                {
                    throw new ArgumentException("Command 'value' can't be empty");
                }

                command = command + ("app=" + task.command);
                command = command + (" value=" + task.devicetype + "," + ("x:\\config.json"));

                // modefied start @ 20250206 stephen : base on CLI spec change
                /*if (option.index != null && option.index.Length > 0)
                {
                    command = command + (" index=" + option.index);
                }
                else
                {
                    command = command + (" index=1");
                }

                return command;*/
            }
            else
            {
                command = command + (task.devicetype + "=" + task.command);
                command = command + (" value=" + task.value);
            }

            //command = command + (task.devicetype + "=" + task.command);
            //command = command + (" value=" + task.value);
            // modified end @ 20250206 stephen

            // check options

            if (option.index != null && option.index.Length > 0)
            {
                command = command + (" index=" + option.index);
            }

            if (option.servicetag != null && option.servicetag.Length > 0)
            {
                command = command + (" servicetag=" + option.servicetag);
            }

            if (option.model != null && option.model.Length > 0)
            {
                command = command + (" model=" + option.model);
            }

            return command;
        }

        private string createCommandFw(CmaCommand.CmaTask task)
        {

            //  move to Params @ 20241029 stephen
            /*            const string ForceWithNotice = "forcewithnotice";
                        const string ForceWithNonotice = "forcewithnonotice";
                        const string Defer = "defer";*/

            string command = string.Empty;

            command = command + ("set ");

            // modified @ 20241112 stephen : change dock fwupdate command format
            /*            if (Params.DeviceType.DOCK.ToLower().Equals(task.devicetype.ToLower()))
                        {
                            command = command + ("dock=silentfwupdate");
                            command = command + (" value=" + task.value);
                        }
                        else
                        {


                        }*/

            command = command + ("app=firmwareupdate");
            //command = command + (" value=" + task.devicetype + ",forcewithnotice");

            command = command + (" value=" + task.devicetype);

            bool hasOption = false;

            if (Params.FwUpdateValues.ForceWithNotice.ToLower().Equals(task.value.ToLower()))
            {
                command = command + (",forcewithnotice");
                hasOption = true;
            }

            if (Params.FwUpdateValues.ForceWithNonotice.ToLower().Equals(task.value.ToLower()))
            {
                command = command + (",forcewithnonotice");
                hasOption = true;
            }

            if (Params.FwUpdateValues.Defer.ToLower().Equals(task.value.ToLower()))
            {
                command = command + (",Defer");
                hasOption = true;
            }

            if (!hasOption)
            {
                command = command + (",forcewithnotice");
            }

            CmaCommand.CmaTaskOption option = new CmaCommand.CmaTaskOption(task.options);
            /*if (option.index != null && option.index.Length > 0)
            {
                command = command + (" index=" + option.index);
            }*/

            // modified @ 20241112 stephen : remove servicetag
            /*            if (option.servicetag != null && option.servicetag.Length > 0)
                        {
                            command = command + (" value=" + option.servicetag + ",servicetag");
                        }*/

            if (option.minversion != null && option.minversion.Length > 0)
            {
                // add start @ 20241111 stephen
                if (option.upgradetolatest)
                {
                    throw new ArgumentException("Command 'minversion' and 'upgradetolatest' can't be exist in the same task");
                }
                // add end @ 20241111 stephen

                // *****CLI use 'miniversion'*****
                command = command + (" value=" + option.minversion + ",miniversion");
            }

            if (option.model != null && option.model.Length > 0)
            {
                command = command + (" value=" + option.model + ",model");
            }

            // add @ 20241113 stephen
            if (option.uod)
            {
                command = command + (" value=" + option.uod + ",uod");
            }

            return command;
        }

        private string createCommandLock(Boolean isLock, CmaCommand.CmaTask task)
        {
            string command = string.Empty;
            string comLock = isLock ? Params.Active.LOCK : Params.Active.UNLOCK;

            comLock = comLock.ToLower();

            command = command + ("set ");
            command = command + (task.devicetype + "=" + task.command);

            switch (task.command.ToLower())
            {
                case Params.Lock.PrimaryMonitorSync:  // #5.14.9
                    if (isLock)
                    {
                        command = command + (" value=on");
                    }
                    else
                    {
                        command = command + (" value=off");
                    }
                    break;

                case Params.Lock.InAppUSBKVM:   // # 5.14.12
                case Params.Lock.PresenceDetection: // 5.14.22 Enable/Disable
                case Params.Lock.ancMode:   // 5.14.23 Enable/Disable
                case Params.Lock.wearDetection: // 5.14.25
                case Params.Lock.IsProximitySensorEnable:   // add @ 20241116 stephen the same as 'wearDetection'
                    if (isLock)
                    {
                        command = command + (" value=enable");
                    }
                    else
                    {
                        command = command + (" value=disable");
                    }
                    break;

                default:
                    command = command + (" value=" + comLock);
                    break;

            }

            return command;

            /*switch (task.command.ToLower())
            {
                case Params.Lock.InAppUpdate:
                    command = command + ("app=" + task.command);
                    //command = command + (" value=" + comLock);
                    break;

                case Params.Lock.InAppRestoreDefault:
                    command = command + ("app=" + task.command);
                    break;

                case Params.Lock.RestoreFactoryDefaults:
                    command = command + (task.devicetype + "=" + task.command);
                    break;

                case Params.Lock.ScreenNotification:
                    command = command + ("app=" + task.command);
                    break;

                case Params.Lock.TelemetryConsent:
                    command = command + ("app=" + task.command);
                    //command = command + (" value=" + comLock);
                    break;

                case Params.Lock.InAppBriCont:
                    command = command + (task.devicetype + "=" + task.command);
                    break;

                case Params.Lock.InAppAutoBriTemp:    // #5.14.7
                    command = command + (task.devicetype + "=" + task.command);
                    break;

                *//*case Params.Lock.InAppBriCont:  // #5.14.8
                    command = command + (task.devicetype + "=" + task.command);
                    break;*//*

                case Params.Lock.PrimaryMonitorSync:  // #5.14.9
                    command = command + (task.devicetype + "=" + task.command);
                    break;

                case Params.Lock.ResolutionRefreshRate:
                    command = command + (task.devicetype + "=" + task.command);
                    break;

                case Params.Lock.USBCPrioritization:
                    command = command + (task.devicetype + "=" + task.command);
                    break;

                case Params.Lock.InAppUSBKVM:   // # 5.14.12
                    command = command + (task.devicetype + "=" + task.command);
                    break;


                case Params.Lock.InAppNetworkKVM:
                    command = command + (task.devicetype + "=" + task.command);
                    break;

                case Params.Lock.InAppColorPreset:
                    command = command + (task.devicetype + "=" + task.command);
                    break;

                case Params.Lock.PowerNap:
                    command = command + (task.devicetype + "=" + task.command);
                    break;

                case Params.Lock.InAppExportSettings:
                    command = command + (task.devicetype + "=" + task.command);
                    break;

                case Params.Lock.CollabScreenShare:
                    command = command + (task.devicetype + "=" + task.command);
                    break;


                case Params.Lock.hdr:
                    command = command + (task.devicetype + "=" + task.command);
                    break;

                case Params.Lock.AntiFlicker:
                    command = command + (task.devicetype + "=" + task.command);
                    break;

                case Params.Lock.MicSwitch:
                    command = command + (task.devicetype + "=" + task.command);
                    break;

                case Params.Lock.AIAutoFraming:
                    command = command + (task.devicetype + "=" + task.command);
                    break;

                case Params.Lock.PresenceDetection: // 5.14.22 Enable/Disable
                    command = command + (task.devicetype + "=" + task.command);
                    break;

                case Params.Lock.ancMode:   // 5.14.23 Enable/Disable
                    command = command + (task.devicetype + "=" + task.command);
                    break;

                case Params.Lock.micNoiseCancellation:  // 5.14.24
                    command = command + (task.devicetype + "=" + task.command);
                    break;

                case Params.Lock.wearDetection: // 5.14.25
                    command = command + (task.devicetype + "=" + task.command);
                    break;


                    // default: // TODO: Error Command

            }*/



            //command = command + (" value=" + comLock);

            //return command;
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

                if (Params.Active.GET.ToLower().Equals(task.active.ToLower()))
                {
                    eventtype = 1;
                    command = createCommandGet(task);
                }

                if (Params.Active.SET.ToLower().Equals(task.active.ToLower()))
                {
                    eventtype = 2;
                    command = createCommandSet(task);
                }

                if (Params.Active.LOCK.ToLower().Equals(task.active.ToLower()))
                {
                    eventtype = 3;
                    command = createCommandLock(true, task);

                }

                if (Params.Active.UNLOCK.ToLower().Equals(task.active.ToLower()))
                {
                    eventtype = 4;
                    command = createCommandLock(false, task);

                }

                if (Params.Active.FW.ToLower().Equals(task.active.ToLower()))
                {
                    eventtype = 5;
                    command = createCommandFw(task);

                }

                commandinputs.Add(command);

                TaskInfo taskInfo = new TaskInfo();
                taskInfo.sid = task.sid;
                taskInfo.gid = guid;
                taskInfo.tid = task.tid;
                taskInfo.eventtype = eventtype;
                taskInfo.command = command;
                taskInfo.jsonconfig = task.value;

                WriteLog($"[CMA] initCommandTask command = {command}");
                WriteLog($"[CMA] initCommandTask taskinfo.jsonconfig = {taskInfo.jsonconfig}");

                taskInfoQueue.Enqueue(taskInfo);
            }
        }

        private async Task<NotifyArgs> runCommandTaskAsync(string sid, string gid)
        {
            Boolean isSuccess = false;
            string responseMsg = String.Empty;
            string responseResult = String.Empty;
            string jsonResult = String.Empty;   // add @ 20241217 stephen
            string finalResult = String.Empty;

            TaskInfo taskInfo = new TaskInfo();

            ICLICommandTable iCLICommandTable;
            CommandLineInput commandLineInput;

            CLIEventResult? cliResult = null;
            JObject? cliResp = null;
            int resultCount = 0;

            if (null != _CliManagerPlugin)
            {
                WriteLog($"[CMA] runCommandTaskAsync taskInfoQueue.Count = {taskInfoQueue.Count}");

                while (taskInfoQueue.Count > 0)
                {
                    WriteLog($"[CMA] runCommandTaskAsync taskInfoQueue.Count = {taskInfoQueue.Count}");

                    isSuccess = false;
                    responseMsg = String.Empty;
                    responseResult = String.Empty;

                    taskInfo = taskInfoQueue.Peek();

                    iCLICommandTable = new ICLICommandTable(null);
                    commandLineInput = iCLICommandTable.StringProcessing(taskInfo.command.ToUpper().Split(' '));

                    WriteLog($"[CMA] runCommandTaskAsync CommandType_Option.Count = {commandLineInput.Options.Count}");

                    foreach (CommandType_Option s in commandLineInput.Options)
                    {
                        WriteLog($"[CMA] runCommandTaskAsync CommandType_Option.Option_Name = {s.Option_Name.ToString()}");
                        WriteLog($"[CMA] runCommandTaskAsync CommandType_Option.Option_Value = {s.Option_Value.ToString()}");
                    }


                    commandLineInput.isCliRunAdmin = true;
                    commandLineInput.jsonDeviceConfig = taskInfo.jsonconfig;
                    commandLineInput.remote_mgr_guid = gid;
                    commandLineInput.fromcma = true;

                    cliResult = await _CliManagerPlugin.PerformCommandLineRelay(commandLineInput);

                    // add @ 20250409 stephen for debug
                    WriteLog($"[CMA] runCommandTaskAsync cliResult = {cliResult.serialize_Json_response}");

                    try
                    {
                        /*cliResp = JObject.Parse(cliResult.serialize_Json_response);
                        responseMsg = (string?)cliResp["Message"] ?? string.Empty;
                        responseResult = ((string)cliResp["Result"]).ToLower()?? string.Empty;

                        if (responseResult.Equals("success"))
                        {
                            isSuccess = true;
                        }

                        if (responseResult.Equals("pass"))
                        {
                            isSuccess = true;
                        }*/

                        // add @ 20241217 stephen
                        jsonResult = checkCliResponse(cliResult.serialize_Json_response);

                        isSuccess = checkResult(jsonResult, out responseMsg);

                        if (resultCount > 0)
                        {
                            finalResult = finalResult + ",";
                        }


                        if (isSuccess)
                        {
                            finalResult = finalResult + "{\"tid\": " + taskInfo.tid + ",\"result\": 0,\"msg\": \"\",\"data\": " + jsonResult + "}";
                        }
                        else
                        {
                            finalResult = finalResult + "{\"tid\": " + taskInfo.tid + ",\"result\": " + Params.Response.STATUS_COMMAND_ERROR_FORMAT_OR_PARAMS + ",\"msg\": \"" + responseMsg + "\",\"data\": [" + cliResult.serialize_Json_response + "]}";
                        }
                    }
                    catch (Exception e)
                    {
                        responseMsg = "Exception: Unknow Result ";
                        finalResult = finalResult + "{\"tid\": " + taskInfo.tid + ",\"result\": " + Params.Response.STATUS_COMMAND_ERROR_RESULT_EXCEPTION + ",\"msg\": \"" + responseMsg + e.ToString() + "\",\"data\": [" + cliResult?.serialize_Json_response + "]}";
                    }
                    resultCount = resultCount + 1;
                    taskInfoQueue.Dequeue();

                }

                NotifyArgs args = new NotifyArgs();
                args.eventType = taskInfo.eventtype.ToString();
                args.notification = "{\"sid\": \"" + sid + "\",\"gid\": \"" + gid + "\",\"response\": [" + finalResult + "]}";

                Console.WriteLine("[CMA] runCommandTask args.notification = " + cliResult?.command_guid_string + "\n args.notification = " + args.notification);
                //Console.WriteLine("[CMA] );

                // add @ 20250121 stephen : for fw update response format
                WriteLog("[CMA] runCommandTask args.notification = " + args.notification);

                if (taskInfo.eventtype == Params.EventType.FW)
                {
                    CmdResponse response = new CmdResponse(args.notification);
                    args.notification = response.genResponseFw();

                    WriteLog("[CMA] runCommandTask response.genResponseFw() = " + args.notification);


                    response.writeToFile(gid, args.notification);

                    // add @ 20250326 stephen
                    WriteLog("[CMA] runCommandTask response.writeToFile getErrorMsg()= " + response.getErrorMsg());
                }
                // add end @ 20250121

                OnEventNotify(args);

                return args;
            }

            return new NotifyArgs();
        }

        // add @ 20241217 stephen
        private string checkCliResponse(string cliResponse)
        {
            try
            {
                JArray jarray;
                jarray = JArray.Parse(cliResponse);
            }
            catch (Exception e)
            {
                WriteLog($"[CMA] Exception: checkResult src is not json array.\nException is {e.ToString()}");

                // fix CLI response as json string
                int index = cliResponse.IndexOf("{", 0);

                do
                {
                    index = cliResponse.IndexOf("{", index + 2);
                    if (index > 0)
                    {
                        cliResponse = cliResponse.Insert(index, ",");
                    }
                } while (index > 0);

                // modified @ 20241111 stepohen
                //src = "[" + src + "]";

                if (!cliResponse.StartsWith("["))
                {
                    cliResponse = "[" + cliResponse + "]";
                }
                // modified end @ 20241111

                WriteLog($"[CMA] checkResult fixed src = {cliResponse}");
            }
            return cliResponse;
        }

        private bool checkResult(string src, out string msg)
        {
            bool isSuccess = false;
            msg = string.Empty;

            WriteLog($"[CMA] checkResult src = {src}");

            JArray jarray;
            string strResult = string.Empty;

            try
            {
                jarray = JArray.Parse(src);

                // check command result is success or not
                foreach (JToken item in jarray)
                {

                    try
                    {
                        JObject jobj = item as JObject;
                        strResult = ((string)jobj["Result"]).ToLower() ?? string.Empty;

                        if (strResult.Equals("success"))
                        {
                            isSuccess = true;
                            continue;
                        }

                        if (strResult.Equals("pass"))
                        {
                            isSuccess = true;
                            continue;
                        }

                        if (strResult.Equals("completed"))
                        {
                            isSuccess = true;
                            continue;
                        }

                        isSuccess = false;
                        msg = (string)jobj["Message"];

                        return isSuccess;
                    }
                    catch (Exception e)
                    {
                        msg = e.ToString();

                        return isSuccess;
                    }
                }
            }
            catch (Exception e)
            {

                if (src.ToLower().Contains("success") || src.ToLower().Contains("pass") || src.ToLower().Contains("completed"))
                {
                    isSuccess = true;
                    return isSuccess;
                }

                isSuccess = false;
                msg = e.ToString();
            }

            return isSuccess;
        }

        private string checkRemoteRequest(string src)
        {


            string data = src;

            int index = src.IndexOf('\\', 0);

            if (index < 0)
            {
                return data;
            }

            WriteLog($"[CMA] checkRemoteRequest src.IndexOf('\\', 0) = {index}");

            do
            {
                if (data[index + 1] != '\\')
                {
                    data = data.Insert(index, "\\");
                }

                index = data.IndexOf('\\', index + 2);

            } while (index > 0);

            WriteLog($"[CMA] checkRemoteRequest returns = {data}");

            return data;
        }

        public Task<RemoteManagementResult> Info(RemoteRequestArgs request)
        {
            Guid uniqueAgentGuid = Guid.NewGuid();

            //Assign request ID per call
            RemoteManagementResult result = new RemoteManagementResult();
            result.cma_request_id = uniqueAgentGuid;

            // add @ 20250311 stephen : disable queue with multi-tasks
            if (taskInfoQueue.Count > 0) {
                result.message = "A command is running.";
                result.output_result = "FAIL";

                sendErrorNotify(uniqueAgentGuid.ToString(), request.remote_request);

                return Task.FromResult(result);
            }

            if (request == null)
            {
                result.message = "Null parameter";
                result.output_result = "FAIL";
                return Task.FromResult(result);
            }

            if (string.IsNullOrEmpty(request.remote_request))
            {
                result.message = "Empty request";
                result.output_result = "FAIL";
                return Task.FromResult(result);
            }
            WriteLog($"[CMA] initCommandTask request.cma_request = {request.remote_request}");

            string remoteRequest = checkRemoteRequest(request.remote_request.ToLower());

            try
            {
                initCommandTask(uniqueAgentGuid.ToString(), remoteRequest);

                TaskInfo taskInfo = taskInfoQueue.Peek();
                WriteLog($"[CMA] before runCommandTask, taskInfo.sid = {taskInfo.sid} ; taskInfo.gid = {taskInfo.gid} ; taskInfo.tid = {taskInfo.tid} ; taskInfo.eventtype = {taskInfo.eventtype} ; taskInfo.command = {taskInfo.command}");

                if (taskInfo.command.ToLower().Contains("firmwareupdate"))
                {
                    // !check firmware device is exist
                    if (!_CliManagerPlugin.checkDeviceConn(DeferControlPanel.SRC_FROM_CMA, uniqueAgentGuid.ToString(), request.remote_request, taskInfo.command).Result)
                    {

                        WriteLog($"[CMA] _CliManagerPlugin.checkDeviceConn = false, do not run command");

                        taskInfoQueue.Dequeue();    // add @ 20250124 stephen: bug fix

                        // modified @ 20502024 stephen : send fwjob event
                        sendFwJobNotify(uniqueAgentGuid.ToString(), request.remote_request);

                        result.message = "Device not found";
                        result.output_result = "FAIL";
                        return Task.FromResult(result);

                    }
                }

                // moved and modify @ 20250114 stephen
                // add @ 20241210 stephen: check is defer
                //_CliManagerPlugin.checkDefer(DeferControlPanel.SRC_FROM_CMA, uniqueAgentGuid.ToString(), request.remote_request);
                if (request.remote_request.ToLower().Contains("defer") && 
                    _CliManagerPlugin.checkDefer(DeferControlPanel.SRC_FROM_CMA, uniqueAgentGuid.ToString(), request.remote_request, taskInfo.command.ToLower()).Result)
                {
                    WriteLog($"[CMA] _CliManagerPlugin.checkDefer = true, do not run command");

                    taskInfoQueue.Dequeue();    // add @ 20250124 stephen: bug fix

                    // feedback event to show in defer status
                    sendDeferNotify(uniqueAgentGuid.ToString(), request.remote_request);

                    result.message = "tasks defer";
                    result.output_result = "DeferSchedule";

                    return Task.FromResult(result);
                }


                _ = Task.Run(async () => await runCommandTaskAsync(taskInfo.sid, taskInfo.gid));
            }
            catch (Exception e)
            {

                NotifyArgs args = new NotifyArgs();
                args.eventType = Params.EventType.UNKNOWN_ERROR.ToString();
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

                        _CliManagerPlugin.CLIActionResult += OnCLIManagerResultHandler;
                    }
                }
            });
        }

        private void OnCLIManagerResultHandler(object sender, CLIEventResult e)
        {
            //Paring the result
        }

        #region ICMAManagerSA implementation
        public Task WriteResult(RemoteManagementResult result)
        {
            lock (_resultLock)
            {
                _result_list.Add(result);
            }
            return Task.CompletedTask;
        }

        public event EventHandler<CMAEventArgs> CMARequestEvent;

        // add @ 20250220 stephen : add fwupdate result return code
        private int responseFwResultCode(int code, out string msg)
        {

            int resultCode = -1;

            msg = string.Empty;

            switch (code)
            {
                case (int)FWUErrorCode.NoError:
                    resultCode = Params.Response.STATUS_FW_UPDATE_SUCCESS;
                    break;

                case (int)FWUErrorCode.DeviceDisconnected:
                    resultCode = Params.Response.STATUS_FW_UPDATE_DEVICE_NOT_CONNECTED;
                    msg = "STATUS_FW_UPDATE_DEVICE_NOT_CONNECTED";
                    break;

                case (int)FWUErrorCode.Unknow:
                    resultCode = Params.Response.UNKNOWN_ERROR;
                    msg = "UNKNOWN_ERROR";
                    break;

                default:
                    resultCode = Params.Response.STATUS_FW_UPDATE_ERROR;
                    msg = "STATUS_FW_UPDATE_ERROR";
                    break;

            }

            return resultCode;
        }


        // add @ 20241129 stephen
        public Task UpdateFwStatus(List<FWUpdateInfo> datas)
        {
            WriteLog("UpdateFwStatus() executed");

            foreach (FWUpdateInfo data in datas)
            {
                WriteLog("UpdateFwStatus() data.Guid = " + data.Guid);
                WriteLog("UpdateFwStatus() data.Update_date = " + data.Update_date);
                WriteLog("UpdateFwStatus() data.DeviceName = " + data.DeviceName);
                WriteLog("UpdateFwStatus() data.DeviceId = " + data.DeviceId);
                WriteLog("UpdateFwStatus() data.FWUErrorCode = " + data.FWUErrorCode);
                WriteLog("UpdateFwStatus() data.Model = " + data.Model);
                WriteLog("UpdateFwStatus() data.IsUOD = " + data.IsUOD);
                WriteLog("UpdateFwStatus() data.TheLatestVersion = " + data.TheLatestVersion);

                NotifyArgs args = new NotifyArgs();
                args.eventType = Params.EventType.FW.ToString();
                args.notification = "{\"sid\": \"" + "sid" + "\",\"gid\": \"" + data.Guid + "\",\"response\": [" + data.FWUErrorCode + "<" + (int)data.FWUErrorCode + ">" + "(" + data.DeviceName + ", " + data.Model + ")" + "]}";

                /*// modified start @ 20250213 stephen : error handle while guid is empty 
                try
                {
                    // add @ 20250121 stephen
                    CmdResponse cmdResponse = new CmdResponse(data.Guid, true, data);
                    args.notification = cmdResponse.genResponseFwUpdate();

                    cmdResponse.writeToFile(data.Guid, args.notification);

                }
                catch (Exception e)
                {

                    WriteLog("UpdateFwStatus() CmdResponse exception = " + e.ToString());

                    string response = string.Empty;

                    response = "{\"sid\":\"N/A\",\"gid\":\"" + data.Guid + "\",\"response\":[{\"tid\":1,\"result\":" + responseFwResultCode((int)data.FWUErrorCode) + ",\"msg\":\"E\",\"data\":";
                    response = response + "[{";
                    response = response + "\"seqnum\":" + 2 + ",";
                    response = response + "\"index\":\"" + data.DeviceIndex + "\",";
                    response = response + "\"model\":\"" + data.Model + "\",";
                    response = response + "\"servicetag\":\"" + data.ServiceTag + "\",";
                    response = response + "\"marketingname\":\"" + "N/A" + "\",";
                    response = response + "\"serialnumber\":\"" + "N/A" + "\",";
                    response = response + "\"fwversion\":\"" + data.TheLatestVersion + "\",";
                    response = response + "\"fwupdateresponse\":\"" + string.Empty + "\"";
                    response = response + "}]";
                    response = response + "}]}";

                    args.notification = response;

                }*/

                // modified @ 20250308 stephe : fix sid
                WriteLog("[CMA] UpdateFwStatus() Response");
                string sid = string.Empty;

                // add @ 20250422 stephen
                string marketingname = string.Empty;
                string serialnumber = string.Empty;

                CmdResponse cmdResponse = new CmdResponse(data.Guid, true, null);

                // add @ 20250326 stephen
                WriteLog("[CMA] UpdateFwStatus() Response cmdResponse.getErrorMsg() = " + cmdResponse.getErrorMsg());


                if (cmdResponse.getErrorMsg().Equals("success"))
                {
                    WriteLog("[CMA] UpdateFwStatus() Response cmdResponse.getErrorMsg() = success");
                    InfoResponse infoResponse = new InfoResponse(cmdResponse.getData(), false);
                    sid = infoResponse.sid;
                    // add @ 20250326 stephen
                    // add @ 20250326 stephen
                    WriteLog("[CMA] UpdateFwStatus() Response cmdResponse.getErrorMsg() = " + cmdResponse.getErrorMsg());

                    // add @ 20250422 stephen
                    foreach (var s in infoResponse.response)
                    {

                        WriteLog("[CMA] UpdateFwStatus() Response infoResponse.response s.ToString() = " + s.ToString());

                        InfoTask infoTask = new InfoTask(s.ToString(), false);

                        WriteLog("[CMA] UpdateFwStatus() Response infoTask.data = " + infoTask.data);

                        foreach (var d in infoTask.data)
                        {

                            WriteLog("[CMA] UpdateFwStatus() Response infoResponse.response InfoData(d.ToString(), false) = " + d.ToString());
                            //WriteLog("[CMA] UpdateFwStatus() Response infoResponse.response InfoData(d.ToString(), false) = " + d.ToString().Replace("\\u0000", string.Empty));

                            //InfoData infodata = new InfoData(d.ToString().Replace("\\u0000", string.Empty), true);
                            InfoData infodata = new InfoData(d.ToString(), true);


                            WriteLog("[CMA] UpdateFwStatus() Response infodata.seqnum = " + infodata.seqnum);
                            WriteLog("[CMA] UpdateFwStatus() Response infodata.index = " + infodata.index);
                            WriteLog("[CMA] UpdateFwStatus() Response infodata.model = " + infodata.model);
                            WriteLog("[CMA] UpdateFwStatus() Response infodata.servicetag = " + infodata.servicetag);
                            WriteLog("[CMA] UpdateFwStatus() Response infodata.marketingname = " + infodata.marketingname);
                            WriteLog("[CMA] UpdateFwStatus() Response infodata.serialnumber = " + infodata.serialnumber);
                            WriteLog("[CMA] UpdateFwStatus() Response infodata.fwversion = " + infodata.fwversion);
                            WriteLog("[CMA] UpdateFwStatus() Response infodata.fwupdateresponse = " + infodata.fwupdateresponse);

                            marketingname = infodata.marketingname;
                            serialnumber = infodata.serialnumber;
                        }



                    }

                }

            


                string response = string.Empty;
                string errMsg = string.Empty;

                response = "{\"sid\":\"" + sid + "\",\"gid\":\"" + data.Guid + "\",\"response\":[{\"tid\":1,\"result\":" + responseFwResultCode((int)data.FWUErrorCode, out errMsg) + ",\"msg\":\"" + errMsg + "\",\"data\":";
                response = response + "[{";
                response = response + "\"seqnum\":" + 2 + ",";
                response = response + "\"index\":\"" + data.DeviceIndex + "\",";
                response = response + "\"model\":\"" + data.Model + "\",";
                response = response + "\"servicetag\":\"" + data.ServiceTag + "\",";
                response = response + "\"marketingname\":\"" + marketingname + "\",";
                response = response + "\"serialnumber\":\"" + serialnumber + "\",";
                response = response + "\"fwversion\":\"[" + data.TheLatestVersion + "]\",";
                response = response + "\"fwupdateresponse\":[\"" + string.Empty + "\"]";
                response = response + "}]";
                response = response + "}]}";

                args.notification = response;

                WriteLog("UpdateFwStatus() CmdResponse args.notification final = " + args.notification);
                // modified end @ 20250213


                OnEventNotify(args);

            }

            /*            NotifyArgs args = new NotifyArgs();
                        args.eventType = Params.EventType.UNKNOWN_ERROR.ToString();
                        args.notification = "UpdateFwStatus()";
                        OnEventNotify(args);*/
            return Task.CompletedTask;
        }

        public Task Update_DeviceChanged(CMADeviceChanges data)
        {
            WriteLog("[ICMAManagerSA] Update_DeviceChanged() executed");

            // TODO: implement decice connect/disconnect information
            WriteLog("[ICMAManagerSA] Update_DeviceChanged() executed data.type = " + data.type);

            if (!data.type.ToLower().Equals("display"))
            {
                return Task.CompletedTask;
            }

            if (data.mos != null)
            {
                foreach (MonitorInfo info in data.mos)
                {
                    if (info == null)
                    {
                        WriteLog("[ICMAManagerSA] Update_DeviceChanged() executed MonitorInfo is null");
                        continue;
                    }

                    WriteLog("[ICMAManagerSA] Update_DeviceChanged() executed MonitorInfo =======================================================");
                    WriteLog("[ICMAManagerSA] Update_DeviceChanged() executed MonitorInfo ToString = " + info.ToString());
                    WriteLog("[ICMAManagerSA] Update_DeviceChanged() executed MonitorInfo AliasDeviceName = " + info.AliasDeviceName);
                    WriteLog("[ICMAManagerSA] Update_DeviceChanged() executed MonitorInfo Index = " + info.Index);
                    WriteLog("[ICMAManagerSA] Update_DeviceChanged() executed MonitorInfo CapabilityString = " + info.CapabilityString);
                    WriteLog("[ICMAManagerSA] Update_DeviceChanged() executed MonitorInfo DisplayName = " + info.DisplayName);
                    WriteLog("[ICMAManagerSA] Update_DeviceChanged() executed MonitorInfo DDCisON = " + info.DDCisON);
                    WriteLog("[ICMAManagerSA] Update_DeviceChanged() executed MonitorInfo edid = " + (info.edid != null ? info.edid.ToString() : "null"));
                    WriteLog("[ICMAManagerSA] Update_DeviceChanged() executed MonitorInfo FwVersion = " + info.FwVersion);
                    WriteLog("[ICMAManagerSA] Update_DeviceChanged() executed MonitorInfo inputSource = " + info.inputSource);
                    WriteLog("[ICMAManagerSA] Update_DeviceChanged() executed MonitorInfo inputCable = " + info.inputCable);
                    WriteLog("[ICMAManagerSA] Update_DeviceChanged() executed MonitorInfo modelName = " + info.modelName);
                    WriteLog("[ICMAManagerSA] Update_DeviceChanged() executed MonitorInfo series = " + info.series);

                    if (info.edid != null)
                    {
                        WriteLog("[ICMAManagerSA] Update_DeviceChanged() executed MonitorInfo edid.PID = " + info.edid.PID);
                        WriteLog("[ICMAManagerSA] Update_DeviceChanged() executed MonitorInfo edid.ModelName = " + info.edid.ModelName);
                        WriteLog("[ICMAManagerSA] Update_DeviceChanged() executed MonitorInfo edid.SerialNumber = " + info.edid.SerialNumber);
                        WriteLog("[ICMAManagerSA] Update_DeviceChanged() executed MonitorInfo edid.ServiceTag = " + info.edid.ServiceTag);
                    }
                    else
                    {
                        WriteLog("[ICMAManagerSA] Update_DeviceChanged() executed MonitorInfo edid is null");
                    }

                    WriteLog("[ICMAManagerSA] Update_DeviceChanged() executed MonitorInfo =======================================================");
                }
            }
            else
            {
                WriteLog("[ICMAManagerSA] Update_DeviceChanged() data.mos is null");
            }


            NotifyArgs args = deviceControlPannel.OnDeviceChnaged(data);

            // modified @ 20250213 stephen : add create time
            // modified @ 20250213 stephen : fix bug with empty event

            WriteLog("[ICMAManagerSA] Update_DeviceChanged() executed args.notification.Equals(string.Empty) = " + args.notification.Equals(string.Empty));

            if (!args.notification.Equals(string.Empty))
            {

                WriteLog("[ICMAManagerSA] Update_DeviceChanged() executed args.notification = " + args.notification);

                args.notification = "{\"sid\": \"\",\"gid\": \"\",\"response\": [{\"tid\": 0,\"result\": 0,\"msg\": \" " + DateTimeOffset.Now.ToString() + " \",\"data\": [" + args.notification + "]}]}";

                if (Params.EventType.DISPLAY_CONNECT.ToString().Equals(args.eventType))
                {
                    OnEventDisplayConnect(args);
                }
                else
                {
                    OnEventDisplayDisconnect(args);
                }
            }

            // add @ 20250206 stephen
            Task.Delay(3000).Wait();

            // add @ 20250204 stephen
            WriteLog("[ICMAManagerSA] Update_DeviceChanged() executed isDoFwJobChecking = " + isDoFwJobChecking);


            // add @ 20250117 stephen
            // modify @ 20250119 stephen
            if (!isDoFwJobChecking)
            {
                doFwJobChecking(data.mos);
            }

            //OnEventNotify(args);

            return Task.CompletedTask;
        }
        #endregion

        private void OnEventNotify(NotifyArgs e)
        {
            EventHandler<NotifyArgs> Handler = Notify;
            if (Handler != null)
            {
                WriteLog($"[CMA] OnEventNotify e.notification = {e.notification}");
                Handler.Invoke(this, e);
            }
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

        #region Defer implement
        // add @ 20241213 stephen

        /*private const long DAY_IN_SECONDS = 24 * 60 * 60;    // 24 hours
        private const int INTERVAL_CHECK_SECONDS = 10 * 60 * 1000; // 10 mins*/

        // test
        private const long DAY_IN_SECONDS = 24 * 60 * 60;    // 24 hours
        private const int INTERVAL_CHECK_SECONDS = 10 * 60 * 1000; // 10 mins
        private System.Timers.Timer timerDefer;

        private void initDeferControlPanel()
        {
            WriteLog($"[CMA] initDeferControlPanel()");
            DeferControlPanel.init();
            startDeferTimer();

            int i = 0;
            foreach (string str in (DeferControlPanel.getList()))
            {
                WriteLog($"[CMA] str[{i++}] = " + str);
            }
        }
        private void startDeferTimer()
        {
            WriteLog($"[CMA] startDeferTimer()");
            timerDefer = new System.Timers.Timer();
            timerDefer.Interval = INTERVAL_CHECK_SECONDS;
            timerDefer.Elapsed += Timer_Elapsed;

            timerDefer.Start();
        }

        // add @ 20250122 stephen
        private void sendFwJobNotify(string guid, string command)
        {
            CmaCommand cmd = new CmaCommand(guid, command);

            NotifyArgs args = new NotifyArgs();
            args.eventType = Params.EventType.FW_JOB.ToString();
            args.notification = "{\"sid\": \"" + cmd.sid + "\",\"gid\": \"" + guid + "\",\"response\": [" + command + "]}";
            OnEventNotify(args);
        }

        private void sendDeferNotify(string guid, string command)
        {
            CmaCommand cmd = new CmaCommand(guid, command);

            NotifyArgs args = new NotifyArgs();
            args.eventType = Params.EventType.DEFER.ToString();
            args.notification = "{\"sid\": \"" + cmd.sid + "\",\"gid\": \"" + guid + "\",\"response\": [" + command + "]}";
            OnEventNotify(args);
        }

        // add @ 20250311 stephen
        private void sendErrorNotify(string guid, string command)
        {
            CmaCommand cmd = new CmaCommand(guid, command);

            NotifyArgs args = new NotifyArgs();
            args.eventType = Params.EventType.ANOTHER_COMMAND_EXECUTE.ToString();
            args.notification = "{\"sid\": \"" + cmd.sid + "\",\"gid\": \"" + guid + "\",\"response\": [" + command + "]}";
            OnEventNotify(args);
        }

        private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            WriteLog($"[CMA] Timer_Elapsed()");
            checkDeferSchedule(DeferControlPanel.getList());
        }

        private void checkDeferSchedule(List<string> list)
        {
            WriteLog($"[CMA] checkDeferSchedule()");

            // add @ 20250304 stephen : check null
            try
            {
                if (null == list)
                {
                    WriteLog($"[CMA] checkDeferSchedule() list is null, do nothing.");
                    return;
                }
            }
            catch (Exception e) {
                WriteLog($"[CMA] checkDeferSchedule() list is null Exception: " + e.ToString());
                return;
            }
                
            int i = 0;
            foreach (string str in list)
            {
                WriteLog($"[CMA] str[{i++}] = " + str);
            }

            long currentDateTimeSecond = DateTimeOffset.Now.ToUnixTimeSeconds();
            int counter = 0;
            //List<string> newItems = new List<string>();
            bool isRuncommand = false;

            WriteLog($"[CMA] currentDateTimeSecond = {currentDateTimeSecond}");

            List<string> deferlist = new List<string>();

            foreach (string str in list)
            {
                deferlist.Add(str);
            }

            // add @ 20250304 stephen : regural experssion for command string
            string pattern = @"(\\[^bfrnt\\/‘\""])";
            string strReplace = string.Empty;
            DeferItem data;


            foreach (string item in deferlist)
            {
                WriteLog($"[CMA] item[{counter}] in list = {item}");

                //DeferItem data;
                strReplace = Regex.Replace(item, pattern, "\\$1");

                try
                {
                    data = new DeferItem(strReplace);
                }
                catch (Exception e)
                {
                    WriteLog($"[CMA] Exception: data = new DeferItem(item); item = {strReplace}. [e:{e.ToString()}]");
                    counter = counter + 1;
                    continue;
                }

                // modified @ 20250304 stephen
                //long newDeferId = ((long)Convert.ToDouble(data.deferid)) + DAY_IN_SECONDS;

                double deferIdDouble;
                long newDeferId;
                if (double.TryParse(data.deferid.ToString(), out deferIdDouble))
                {
                    newDeferId = Convert.ToInt64(data.deferid) + DAY_IN_SECONDS;
                    // Use newDeferId as needed
                }
                else
                {
                    // Handle the case where deferid is not a valid number
                    newDeferId = 0L;
                    WriteLog($"[CMA] Invalid deferid format. newDeferId = 0L;");
                }
                // modified end @ 20250304 stephen

                if (currentDateTimeSecond < newDeferId)
                {
                    WriteLog($"[CMA] {currentDateTimeSecond} < {newDeferId}");
                    break;
                }

                WriteLog($"[CMA] list.Count = {deferlist.Count}");
                DeferControlPanel.removeItem(item);
                WriteLog($"[CMA] list.Count2 = {deferlist.Count}");

                counter = counter + 1;
                data.deferid = newDeferId.ToString();
                data.count = data.count - 1;

                if (data.count < 0)
                {
                    isRuncommand = true;
                    _CliManagerPlugin.showNotification(data.commandfrom, data.guid, data);
                }
                else
                {
                    isRuncommand = !(_CliManagerPlugin.checkDeferSchedule(data.commandfrom, data.guid, data).Result);
                }

                WriteLog($"[CMA] checkDeferSchedule::isRuncommand = {isRuncommand}");

                if (isRuncommand)
                {
                    //removeItems.Add(item);


                    switch (data.commandfrom)
                    {
                        case DeferControlPanel.SRC_FROM_CLI:

                            WriteLog($"[CMA] checkDeferSchedule::DeferControlPanel.SRC_FROM_CLI");
                            ICLICommandTable iCLICommandTable = new ICLICommandTable(null);
                            CommandLineInput commandLineInput = iCLICommandTable.StringProcessing(data.commanddata.Split(' '));

                            commandLineInput.isCliRunAdmin = true;

                            CLIEventResult result = _CliManagerPlugin.PerformCommandLineRelay(commandLineInput).Result;

                            break;

                        case DeferControlPanel.SRC_FROM_CMA:
                            WriteLog($"[CMA] checkDeferSchedule::DeferControlPanel.SRC_FROM_CMA");
                            try
                            {
                                initCommandTask(data.guid.ToString(), data.commanddata);

                                TaskInfo taskInfo = taskInfoQueue.Peek();
                                WriteLog($"[CMA] before runCommandTask, taskInfo.sid = {taskInfo.sid} ; taskInfo.gid = {taskInfo.gid} ; taskInfo.tid = {taskInfo.tid} ; taskInfo.eventtype = {taskInfo.eventtype} ; taskInfo.command = {taskInfo.command}");
                                _ = Task.Run(async () => await runCommandTaskAsync(taskInfo.sid, taskInfo.gid));
                            }
                            catch (Exception e)
                            {

                                NotifyArgs args = new NotifyArgs();
                                args.eventType = Params.EventType.UNKNOWN_ERROR.ToString();
                                args.notification = e.ToString() + "; " + data.commanddata;
                                OnEventNotify(args);
                            }
                            break;
                    }
                }
                else
                {
                    WriteLog($"[CMA] checkDeferSchedule::isRuncommand({isRuncommand}), sendDeferNotify({data.guid}, {data.commanddata})");
                    sendDeferNotify(data.guid, data.commanddata);
                    /*newItems.Add( item );
                    //DeferControlPanel.addToSchedule(data);*/
                }

            }

        }

        #endregion

        #region FW Job implement
        // add @ 20250117 stephen
        private void initFwJobControlPanel()
        {
            WriteLog($"[CMA] initFwJobControlPanel()");
            FwJobControlPanel.init();
            //startDeferTimer();

            int i = 0;
            foreach (string str in (FwJobControlPanel.getList()))
            {
                WriteLog($"[CMA] str[{i++}] = " + str);
            }
        }

        private bool isDoFwJobChecking = false;
        private void doFwJobChecking(List<MonitorInfo> monitors)
        {
            WriteLog($"[CMA] doFwJobChecking");

            if (isDoFwJobChecking)
            {
                return;
            }

            isDoFwJobChecking = true;

            List<string> listFwJobs = new List<string>();

            try
            {
                listFwJobs = FwJobControlPanel.displayConnected(monitors);
            }
            catch (Exception ex)
            {
                //250303 Elie: Stephen, please make a correct response.
                WriteLog($"[CMA] displayConnected got exception. {ex.ToString()}");
                return;
            }


            // add @ 20250204 stephen : fix list is null
            if (null == listFwJobs)
            {
                WriteLog("[CMA] doFwJobChecking: No FwJob.");
                isDoFwJobChecking = false;
                return;
            }

            WriteLog("[CMA] doFwJobChecking strDefers.Count = " + listFwJobs.Count);
            // add end @ 20250204

            DeferItem deferItem = null;
            // do defer
            foreach (string strFwJob in listFwJobs)
            {
                WriteLog("[CMA] doFwJobChecking strFwJob = " + strFwJob);

                try
                {
                    deferItem = new DeferItem(strFwJob);
                }
                catch (Exception ex)
                {
                    //250303 Elie: Stephen, please make a correct response.
                    WriteLog($"[CMA] DeferItem got exception. {ex.ToString()}");
                    return;
                }


                // modified start @ 20250204 stephen : check defer in command
                if (deferItem.commanddata.ToLower().Contains("defer"))
                {
                    // modified start @ 20250213 stephen : fix bug if fwjob from cli and contain defer
                    /*if (_CliManagerPlugin.checkDefer(DeferControlPanel.SRC_FROM_CMA, deferItem.guid.ToString(), deferItem.commanddata).Result)
                    {
                        WriteLog($"[CMA] _CliManagerPlugin.checkDefer = true, do not run command");

                        // feedback event to show in defer status
                        sendDeferNotify(deferItem.guid.ToString(), deferItem.commanddata);
                        isDoFwJobChecking = false;
                        return;
                    }*/

                    bool isDeferSelect = false;

                    try
                    {
                        switch (deferItem.commandfrom)
                        {
                            case DeferControlPanel.SRC_FROM_CLI:
                                try
                                {
                                    if (_CliManagerPlugin.checkDefer(DeferControlPanel.SRC_FROM_CLI, deferItem.guid.ToString(), deferItem.commanddata).Result)
                                    {
                                        WriteLog($"[CMA] _CliManagerPlugin.checkDefer::DeferControlPanel.SRC_FROM_CLI = true, do not run command");
                                        isDeferSelect = true;
                                        //return;
                                    }
                                }
                                catch (Exception ex)
                                {
                                    WriteLog($"[CMA] Error in _CliManagerPlugin.checkDefer::DeferControlPanel.SRC_FROM_CLI: {ex.Message}", log_type.error);
                                }
                                break;

                            case DeferControlPanel.SRC_FROM_CMA:
                                try
                                {
                                    if (_CliManagerPlugin.checkDefer(DeferControlPanel.SRC_FROM_CMA, deferItem.guid.ToString(), deferItem.commanddata).Result)
                                    {
                                        WriteLog($"[CMA] _CliManagerPlugin.checkDefer::DeferControlPanel.SRC_FROM_CMA = true, do not run command");

                                        // feedback event to show in defer status
                                        sendDeferNotify(deferItem.guid.ToString(), deferItem.commanddata);
                                        isDeferSelect = true;
                                    }
                                }
                                catch (Exception ex)
                                {
                                    WriteLog($"[CMA] Error in _CliManagerPlugin.checkDefer::DeferControlPanel.SRC_FROM_CMA: {ex.Message}", log_type.error);
                                }
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        WriteLog($"[CMA] Unexpected error in deferItem processing: {ex.Message}", log_type.error);
                    }

                    if (isDeferSelect)
                    {
                        isDoFwJobChecking = false;
                        return;
                    }
                    // modoified end @ 20250213
                }

                try
                {
                    switch (deferItem.commandfrom)
                    {
                        case DeferControlPanel.SRC_FROM_CLI:
                            WriteLog($"[CMA] checkDeferSchedule::DeferControlPanel.SRC_FROM_CLI");
                            try
                            {
                                ICLICommandTable iCLICommandTable = new ICLICommandTable(Log);
                                CommandLineInput commandLineInput = iCLICommandTable.StringProcessing(deferItem.commanddata.Split(' '));

                                commandLineInput.isCliRunAdmin = true;

                                CLIEventResult result = _CliManagerPlugin.PerformCommandLineRelay(commandLineInput).Result;

                                WriteLog($"[CMA] PerformCommandLineRelay result: {result}");
                            }
                            catch (Exception ex)
                            {
                                WriteLog($"[CMA] Error in PerformCommandLineRelay::DeferControlPanel.SRC_FROM_CLI: {ex.Message}", log_type.error);
                                NotifyArgs args = new NotifyArgs
                                {
                                    eventType = Params.EventType.UNKNOWN_ERROR.ToString(),
                                    notification = ex.ToString() + "; " + deferItem.commanddata
                                };
                                OnEventNotify(args);
                            }
                            break;

                        case DeferControlPanel.SRC_FROM_CMA:
                            WriteLog($"[CMA] checkDeferSchedule::DeferControlPanel.SRC_FROM_CMA");
                            try
                            {
                                initCommandTask(deferItem.guid.ToString(), deferItem.commanddata);

                                TaskInfo taskInfo = taskInfoQueue.Peek();
                                WriteLog($"[CMA] before runCommandTask, taskInfo.sid = {taskInfo.sid} ; taskInfo.gid = {taskInfo.gid} ; taskInfo.tid = {taskInfo.tid} ; taskInfo.eventtype = {taskInfo.eventtype} ; taskInfo.command = {taskInfo.command}");
                                _ = Task.Run(async () => await runCommandTaskAsync(taskInfo.sid, taskInfo.gid));
                            }
                            catch (Exception ex)
                            {
                                WriteLog($"[CMA] Error in runCommandTaskAsync::DeferControlPanel.SRC_FROM_CMA: {ex.Message}", log_type.error);
                                NotifyArgs args = new NotifyArgs
                                {
                                    eventType = Params.EventType.UNKNOWN_ERROR.ToString(),
                                    notification = ex.ToString() + "; " + deferItem.commanddata
                                };
                                OnEventNotify(args);
                            }
                            break;
                    }
                }
                catch (Exception ex)
                {
                    WriteLog($"[CMA] Unexpected error in deferItem processing: {ex.Message}", log_type.error);
                    NotifyArgs args = new NotifyArgs
                    {
                        eventType = Params.EventType.UNKNOWN_ERROR.ToString(),
                        notification = ex.ToString() + "; " + deferItem.commanddata
                    };
                    OnEventNotify(args);
                }
                // modified end @ 20250204 stephen


            }

            Task.Delay(3000).Wait(); // For test 30000 change to 3000
            isDoFwJobChecking = false;
        }
        #endregion

        // add @ 20250417 stephen
        #region DTP device information implement

        public Task UpdateDtpDeviceInfo(CmaDeviceInfo data)
        {

            WriteLog($"[CMA] UpdateDtpDeviceInfo called");

            WriteLog($"[CMA] UpdateDtpDeviceInfo CmaDeviceInfo data.type = {data.type}");
            WriteLog($"[CMA] UpdateDtpDeviceInfo CmaDeviceInfo data.guid = {data.guid}");
            WriteLog($"[CMA] UpdateDtpDeviceInfo CmaDeviceInfo data.model = {data.model}");
            WriteLog($"[CMA] UpdateDtpDeviceInfo CmaDeviceInfo data.serialnumber = {data.serialnumber}");
            WriteLog($"[CMA] UpdateDtpDeviceInfo CmaDeviceInfo data.fwversion = {data.fwversion}");

            deviceControlPannel.updateCmaDeviceList(data);

            return Task.CompletedTask;
        }

        #endregion
    }
}
