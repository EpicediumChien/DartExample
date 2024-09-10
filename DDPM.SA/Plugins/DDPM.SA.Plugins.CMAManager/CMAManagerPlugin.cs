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
        //private ISettingsManagerIT? _SettingsPluginIT;
        //private readonly object _pluginConditionLock = new object();
        //private PluginCondition _SettingsITPluginCondition;

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

            PluginCondition = new PluginStartedCondition();
            WriteLog("Cli Manager plugin report started");

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

            if (e.ChangedPlugins.OfType<ICMAManagerSA>().Any())
            {
                WriteLog("[Info] CMA Manager plugin with ICMAManagerSA started.");
            }

            if (e.ChangedPlugins.OfType<ICMAManagerIT>().Any())
            {
                WriteLog("[Info]  CMA Manager plugin with ICMAManagerIT started.");
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

        #region ICMAManagerIT implementation
        public Task<CMAResult> PerformCMARequest(CMARequestArgs request)
        {
            //Assign request ID per call
            CMAResult result = new CMAResult();
            result.cma_request_id = new Guid();

            if (request == null)
            {
                result.message = "Null parameter";
                result.output_result = "FAIL";
                return Task.FromResult(result);
            }

            OnInvokeCMARequestEvent(request, result.cma_request_id);
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
            }
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
    }
}
