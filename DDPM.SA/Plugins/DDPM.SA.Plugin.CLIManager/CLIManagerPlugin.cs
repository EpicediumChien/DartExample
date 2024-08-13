#region LicenceHeader
//
// Copyright © 2024, Dell Inc., All Rights Reserved.
// This material is confidential and a trade secret.  Permission to use this
// work for any purpose must be obtained in writing from Dell Inc.
//
// CLIManagerPlugin.cs created on 24/07/2024T04:24 PM
//
#endregion 

using Dell.Client.Framework.Common;
using Dell.Client.Framework.Common.Annotations;
using Dell.Client.Framework.Interfaces;
using Dell.Client.Framework.Common.PluginConditions;
using DDPM.SA.Common;
using Microsoft;
using static DDPM.SA.Common.ICLICommandTable;
using System.Windows.Controls.Primitives;
using Windows.Security.Authentication.OnlineId;

namespace DDPM.SA.Plugin.CLIManager
{
    [Plugin(IDs.CLI_Manager_Plugin, pluginName, PluginOrderGroupType.Core, Version = pluginVersion)]
    [Descriptor(Description = pluginDescription)]
    [Publisher(Name = publisherCompany, Website = publisherWebsite, Support = publisherSupport)]
    [PublishedUnelevatedInterface(new[] { typeof(ICliManagerSA) })] //for DeviceManager of user subagent
    [PublishedInterface(new[] { typeof(ICliManagerIT) })] //for CLI subagent

    public class CLIManagerPlugin : BaseAgentPlugin, IDisposableObservable, ICliManagerSA, ICliManagerIT
    {
        public const string PluginLogId = "CLIManager";

        #region Private Members
        private const string pluginName = "CLIManagerPlugin";
        private const string pluginVersion = "1.0.0";
        private const string pluginDescription = "This plugin implements CLI Manager Plugin.";
        private const string publisherCompany = "Wistron";
        private const string publisherWebsite = "https://www.wistron.com";
        private const string publisherSupport = "This plugin implements CLI Manager Plugin.";

        private IAgent _agent;
        private bool _IsAdministrator = ProcessSecurityHelperWrapper.IsCurrentProcessRunningElevated();
        private static List<CLIEventResult> _result_list = new List<CLIEventResult>();
        private readonly object _resultLock = new object();

        private enum log_type
        {
            info = 0,
            error
        }
        #endregion

        #region Constructor
        public CLIManagerPlugin(IAgent agent) : base(agent, PluginLogId)
        {
            _agent = agent;
            WriteLog($"SettingsManagerPlugin constructor ...(Admin:{_IsAdministrator})");

        }

        #endregion

        #region Overriding methods
        protected override void OnPluginStarting()
        {
            _agent.PluginManager.PluginsStarted += PluginManagerOnPluginsStarted;

            PluginCondition = new PluginStartedCondition();
            WriteLog("Cli Manager plugin report started");
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

            if (e.ChangedPlugins.OfType<ICliManagerSA>().Any())
            {
                WriteLog("ICliManager plugin with ICliManagerSA started.");
            }

            if (e.ChangedPlugins.OfType<ICliManagerIT>().Any())
            {
                WriteLog("ICliManager plugin with ICliManagerIT started.");
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
            text = "[CLIManager] " + text;
            Console.WriteLine(text);
            if (log_type == log_type.info)
                Log.Info(text);
            else
                Log.Error(text);
        }
        #endregion

        #region ICliManagerIT implementation

        public Task<CLIEventResult> PerformCommandLineRelay(CommandLineInput commandLineInput)
        {            
            if(commandLineInput == null)
            {
                WriteLog("Empty command input from CLI subagent");
                return Task.FromResult(Response_EmptyCommandInput());
            }

            CLIEventArgs arg = new CLIEventArgs()
            {
                command_guid_string = Guid.NewGuid().ToString(),
                commandLineInput = commandLineInput
            };

            WriteLog($"Got CLI request: ID:{arg.command_guid_string}, command: {arg.commandLineInput.Command}, target type: {arg.commandLineInput.TargetType}");
            OnCLIActionEventNotify(arg);

            CLIEventResult result = new CLIEventResult();
            int counter = 0;
            while (true)
            {
                Sleep(1000);
                lock(_resultLock)
                {
                    int idx = _result_list.FindIndex(x => x.command_guid_string.Trim().ToLower().Equals(arg.command_guid_string.ToLower().Trim()));
                    if (idx >= 0)//result found
                    {
                        result.ticket = _result_list[idx].ticket;
                        result.command_guid_string = _result_list[idx].command_guid_string;
                        result.serialize_Json_response = _result_list[idx].serialize_Json_response;
                        result.ExitCode = _result_list[idx].ExitCode;
                        //clear the processed result
                        _result_list.Remove(result);
                        break;
                    }
                }
                counter++;
                if (counter >= 60)//means timeout
                {
                    result.command_guid_string = arg.command_guid_string;
                    result.serialize_Json_response = Response_NoResultTimeout(commandLineInput);
                    result.ticket = DateTime.Now;
                    result.ExitCode = (int)CLI_ExitCode.wait_command_result_timeout;
                    break;
                }
            }
            //remove result if exist more than 1 hour (memory concern)
            _result_list.RemoveAll(x => DateTime.Now.Subtract(x.ticket).TotalMinutes > 60);

            return Task.FromResult(result);//temp return: it should has timer to check write back result list
        }

        
        #endregion

        #region ICliManagerSA implementation
        public event EventHandler<CLIEventArgs> CLIActionEvent;

        //Target to notify CLIProxy
        private void OnCLIActionEventNotify(CLIEventArgs e)
        {
            if (CLIActionEvent == null || e == null || e == EventArgs.Empty)
                return;

            EventHandler<CLIEventArgs> Handler = CLIActionEvent;
            if (Handler != null)
            {
                Handler.Invoke(this, e);
                WriteLog($"CLIActionEvent Invoked: ID:{e.command_guid_string}");
            }
        }

        public Task WriteCommandResult(CLIEventResult result)
        {
            if(result == null)
            {
                WriteLog("Got null command result from Proxy");
                return Task.FromResult(false);
            }
            if(_result_list.Find(x => x.command_guid_string.Trim().ToLower().Equals(result.command_guid_string.ToLower().Trim())) == null)
            {
                lock (_resultLock)
                {
                    _result_list.Add(result);
                }
            }
            else
            {
                WriteLog($"Duplicated result from Proxy: ID:{result.command_guid_string}");
            }
            return Task.FromResult(true);
        }
        #endregion
    }
}
