using DDPM.RemoteManagement.Common.Interfaces;
using DDPM.SA.Common;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.Common.Annotations;
using Dell.Client.Framework.Common.PluginConditions;
using Dell.Client.Framework.Interfaces;
using Microsoft;
using VcpCore.Common;
using IDs = DDPM.SA.Common.IDs;

namespace DDPM.SA.Plugins.User.CMAProxy
{
    [Plugin(IDs.DDPM_CMA_Proxy_Plugin, pluginName, PluginOrderGroupType.Core, Version = pluginVersion)]
    [Descriptor(Description = pluginDescription)]
    [Publisher(Name = publisherCompany, Website = publisherWebsite, Support = publisherSupport)]
    public class CMAProxyPlugin : BaseAgentPlugin, IDisposableObservable, ICMAProxy
    {
        public const string PluginLogId = "CMAProxy";

        #region Private Members

        private const string pluginName = "CMAProxyPlugin";
        private const string pluginVersion = "1.0.0";
        private const string pluginDescription = "This plugin implements CMA Proxy Plugin.";
        private const string publisherCompany = "Dell Technologies";
        private const string publisherWebsite = "https://www.dell.com";
        private const string publisherSupport = "This plugin implements CMA Proxy Plugin.";

        private IAgent _agent;

        private enum log_type
        {
            info = 0,
            error
        }

        private ICMAManagerSA _CMAManagerPlugin;
        private IDeviceManagerSA _DevManagerPlugin;
        private readonly object _PluginConditionLock_CMAManager = new object();
        private readonly object _PluginConditionLock_DevManager = new object();
        private bool relay_registered = false;
        #endregion

        #region Constructor

        public CMAProxyPlugin(IAgent agent) : base(agent, PluginLogId)
        {
            _agent = agent;
        }
        #endregion

        #region Overriding methods
        protected override void OnPluginStarting()
        {
            _agent.PluginManager.PluginsStarted += PluginManagerOnPluginsStarted;

            PluginCondition = new PluginStartedCondition();
            WriteLog("CMA Proxy plugin report started");

            InitializeDevManagerPlugin();
            InitializeCMAManagerPlugin();
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

                    if (_DevManagerPlugin != null && _CMAManagerPlugin != null)
                    {
                        _CMAManagerPlugin.CMARequestEvent -= _CMAManagerPlugin_CMARequestEvent;
                        relay_registered = false;
                    }
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

            if (e.ChangedPlugins.OfType<ICMAProxy>().Any())
            {
                WriteLog("ICMAProxy plugin started.");
            }

            if (e.ChangedPlugins.OfType<ICMAManagerSA>().Any())
                InitializeCMAManagerPlugin();

            if (e.ChangedPlugins.OfType<IDeviceManagerSA>().Any())
                InitializeDevManagerPlugin();
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
            text = "[CMAProxyPlugin] " + text;
            Console.WriteLine(text);
            if (Log != null)
            {
                if (log_type == log_type.info)
                    Log.Info(text);
                else
                    Log.Error(text);
            }
        }

        private void InitializeCMAManagerPlugin()
        {
            if (_CMAManagerPlugin != null)
                return;

            _CMAManagerPlugin = _agent.PluginManager.FindPluginByType<ICMAManagerSA>(PluginResolution.Dynamic);

            if (_CMAManagerPlugin is IFrameworkPluginConditionNotification pluginCondition)
            {
                pluginCondition.PluginConditionChangeHandler += OnCMAManagerPluginConditionChangeHandler;
                GetCurrentCMAManagerPluginCondition();
            }
        }

        private void InitializeDevManagerPlugin()
        {
            if (_DevManagerPlugin != null)
                return;

            _DevManagerPlugin = _agent.PluginManager.FindPluginByType<IDeviceManagerSA>(PluginResolution.Dynamic);

            if (_DevManagerPlugin is IFrameworkPluginConditionNotification pluginCondition)
            {
                pluginCondition.PluginConditionChangeHandler += OnDevManagerPluginConditionChangeHandler;
                GetCurrentDevManagerPluginCondition();
            }
        }

        private void OnCMAManagerPluginConditionChangeHandler(object sender, EventArgs e)
        {
            GetCurrentCMAManagerPluginCondition();
        }

        private void OnDevManagerPluginConditionChangeHandler(object sender, EventArgs e)
        {
            GetCurrentDevManagerPluginCondition();
        }

        private void GetCurrentCMAManagerPluginCondition()
        {
            _ = Task.Run(async () =>
            {
                var pluginCondition = await (_CMAManagerPlugin as IFrameworkPluginConditionNotification)?.CurrentConditionAsync();

                lock (_PluginConditionLock_CMAManager)
                {
                    if (pluginCondition is PluginErrorCondition)
                    {
                        WriteLog($"{nameof(GetCurrentCMAManagerPluginCondition)} - CMA Manager Plugin is in an error condition");
                    }
                    else if (pluginCondition is PluginRunningCondition || pluginCondition is PluginStartedCondition)
                    {
                        WriteLog($"{nameof(GetCurrentCMAManagerPluginCondition)} - CMA Manager Plugin is in a running/started condition");
                        if (!relay_registered && _DevManagerPlugin != null)
                        {
                            DoRelayRegister();
                        }
                    }
                }
            });
        }

        private void GetCurrentDevManagerPluginCondition()
        {
            _ = Task.Run(async () =>
            {
                var pluginCondition = await (_DevManagerPlugin as IFrameworkPluginConditionNotification)?.CurrentConditionAsync();

                lock (_PluginConditionLock_DevManager)
                {
                    if (pluginCondition is PluginErrorCondition)
                    {
                        WriteLog($"{nameof(GetCurrentDevManagerPluginCondition)} - DeviceManager Plugin is in an error condition");
                    }
                    else if (pluginCondition is PluginRunningCondition || pluginCondition is PluginStartedCondition)
                    {
                        WriteLog($"{nameof(GetCurrentDevManagerPluginCondition)} - DeviceManager Plugin is in a running/started condition");
                        if (!relay_registered && _CMAManagerPlugin != null)
                        {
                            DoRelayRegister();
                        }
                    }
                }
            });
        }

        private void DoRelayRegister()
        {
            if (_DevManagerPlugin == null || _CMAManagerPlugin == null)
            {
                WriteLog("One of Device Manager and CMA Manager is null, do not register CMA relay");
                return;
            }
            _CMAManagerPlugin.CMARequestEvent += _CMAManagerPlugin_CMARequestEvent;
            // modified @ 20250303 stephen
            //_DevManagerPlugin.DeviceChanged += _deviceManager_DeviceChanged;

            // add @ 20250303 stephen
            _DevManagerPlugin.Displaychanged += _deviceManager_Displaychanged;


            _DevManagerPlugin.DownloadAndInstall_Result_Notify += _FwUpdateStatus;  // add @ 20241129 stephen

            relay_registered = true;
        }

        private void _CMAManagerPlugin_CMARequestEvent(object? sender, CMAEventArgs e)
        {
            _ = Task.Run(() =>
            {
                if (_DevManagerPlugin == null)
                {
                    WriteLog("[Command line event] null Device Manager object!");
                    RemoteManagementResult result = new RemoteManagementResult();
                    result.cma_request_id = e.cma_request_id;
                    result.output_result = "FAIL";
                    result.message = "Can't connect to DDPM device manager";

                    _CMAManagerPlugin.WriteResult(result);
                    return;
                }

                if (e == null || string.IsNullOrEmpty(e.input_param))
                {
                    WriteLog("[Command line event] Got Empty CMAEventArgs!");
                    RemoteManagementResult result = new RemoteManagementResult();
                    result.cma_request_id = e.cma_request_id;
                    result.output_result = "FAIL";
                    result.message = "CMA Proxy got null event argument";

                    _CMAManagerPlugin.WriteResult(result);
                    return;
                }

                //Do calculation here
            });
        }

        // add @ 20241129 stephen
        private async void _FwUpdateStatus(object? sender, List<FWUpdateInfo> e)
        {
            _CMAManagerPlugin.UpdateFwStatus(e);
        }



        // add start @ 20250303 stephen
        private async void _deviceManager_Displaychanged(object? sender, DisplaychangedEventArgs e)
        {
            WriteLog("_deviceManager_Displaychanged() executed");

            if (e != null)
            {
                List<MonitorInfo> mos = e.monitors;
                WriteLog($"Monitor count is ${mos.Count}");
                if (_CMAManagerPlugin != null)
                {
                    _CMAManagerPlugin.Update_DeviceChanged(new CMADeviceChanges() { type = "display", mos = mos, devices = null });
                }
                else
                {
                    WriteLog("_CMAManagerPlugin is null then can't pass call Update_DeviceChanged");
                }
            }
            else
            {
                WriteLog("_CMAManagerPlugin _deviceManager_Displaychanged, e == null.");
            }
        }
        // add end @ 20250303 stephen

        // remove start @ 20250304 stephen
        /*private async void _deviceManager_DeviceChanged(object? sender, DeviceChangedEventArgs e)
        {
            WriteLog("_deviceManager_DeviceChanged() executed");

            if ((e != null) && !string.IsNullOrEmpty(e.changedProperty))
            {
                WriteLog($"@ChangedProperty=[{e.changedProperty}], ChangedType=[{e.type}] DeviceID=[{e.deviceID}]");
                if (e.device_peripherals != null)
                {
                    WriteLog($"@DeviceName=[{e.device_peripherals.Name}]");
                }

                if (e.changedProperty.ToLower().Contains("remove") ||
                    (e.changedProperty.ToLower().Contains("add")) ||
                    (e.changedProperty.ToLower().Contains("batterystatuschanged")) ||
                    (e.changedProperty.ToLower().Contains("batterylevelchanged")) ||
                    (string.Compare(e.changedProperty, "DisplayChanged", true) == 0))
                {
                    GetDdpmDevices(e.changedProperty.ToLower());
                }
                else
                {
                    WriteLog($"skip : [{e.changedProperty.ToString()}]");
                }
            }
            else
            {
                if ((e == null))
                {
                    WriteLog("skip : e == null");
                }
                else
                {
                    if (string.IsNullOrEmpty(e.changedProperty))
                        WriteLog("skip : e.changedProperty == null");
                    else
                        WriteLog($"skip : e.changedProperty : {e.changedProperty}");
                }
            }
        }

        private void GetDdpmDevices(string condition = "all")
        {
            if (_DevManagerPlugin == null)
            {
                WriteLog("Could not establish communication with DeviceManager plugin!!");
                return;
            }
            WriteLog("GetDdpmDevices is invoked");

            if (condition.Equals("all") || condition.Equals("displaychanged"))
            {
                // add @ 20250214 stephen : wait for list ready
                Thread.Sleep(3000);

                List<MonitorInfo> mos = _DevManagerPlugin.GetMonitors().Result;
                WriteLog($"Monitor count is ${mos.Count}");
                if (_CMAManagerPlugin != null)
                {
                    _CMAManagerPlugin.Update_DeviceChanged(new CMADeviceChanges() { type = "display", mos = mos, devices = null });
                }
                else
                    WriteLog("_CMAManagerPlugin is null then can't pass call Update_DeviceChanged");
            }
            if (condition.Equals("all") || !condition.Equals("displaychanged"))
            {
                DeviceHelper deviceHelper = _DevManagerPlugin.GetDevices().Result;
                List<DeviceInfo> _deviceInfos = new List<DeviceInfo>();
                if ((deviceHelper != null) && (deviceHelper.deviceInfo != null))
                {
                    _deviceInfos = deviceHelper.deviceInfo;
                    if (_CMAManagerPlugin != null)
                    {
                        _CMAManagerPlugin.Update_DeviceChanged(new CMADeviceChanges() { type = "peripheral", mos = null, devices = _deviceInfos });
                    }
                    else
                        WriteLog("_CMAManagerPlugin is null then can't pass call Update_DeviceChanged");
                }
                WriteLog($"Peripheral count is ${_deviceInfos.Count}");
            }
        }*/
        // remove end @ 20250304 stephen
        #endregion

        #region ICLIProxy implementation

        //no action need in this plugin

        #endregion
    }
}