using DDPM.SA.Common;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.Common.Annotations;
using Dell.Client.Framework.Interfaces;
using Dell.DTM.Client.Platinum;
using Dell.TechHub.Common;
using Dell.TechHub.Common.PluginInterfaces.Transmission;
using Dell.TechHub.Sdk.Common.Identifiers;
using Microsoft;
using VcpCore.Common;
using IDs = DDPM.SA.Common.IDs;

namespace DDPM.SA.Plugin.PlatinumSDK
{
    [Plugin(Id, Name, Description = Description)]
    [PluginRequires(Id = PluginInformation.Id)]
    [PublishedUnelevatedInterface(new[] { typeof(IPlatinumSDKService) })]
    public class PlatinumSDKPlugin : BaseAgentPlugin, IDisposableObservable, IPlatinumSDKService
    {
        public const string PluginLogId = "PlatinumSDK";

        #region Private Members

        private const string pluginName = "PlatinumSDKPlugin";
        private const string pluginVersion = "1.0.0";
        private const string pluginDescription = "This plugin implements PlatinumSDK Plugin.";
        private const string publisherCompany = "Dell Technologies";
        private const string publisherWebsite = "https://www.dell.com";
        private const string publisherSupport = "This plugin implements PlatinumSDK Plugin.";

        private const string Id = IDs.PlatinumSDK_Plugin;
        private const string Name = pluginName;
        private const string Description = pluginDescription;

        private static Logs _logs;
        private static bool IsSucessInitializeAsync = false;

        private readonly object _lockobject = new object();

        private static readonly AgentPluginInfo _agentPluginInfo = new AgentPluginInfo()
        {
            PluginGuid = Guid.Parse(IDs.PlatinumSDK_Plugin),
            PluginName = pluginName,
            PluginVersion = pluginVersion,
            PluginDescription = pluginDescription,
            PluginEnabled = true,
        };

        private IAgent _agent;
        private bool _IsAdministrator = ProcessSecurityHelperWrapper.IsCurrentProcessRunningElevated();
        private IPlatinumClientSdk _platinumClientSdk;

        private static bool IsTelemetryConsentOn = false;

        #endregion Private Members

        #region Constructor

        public PlatinumSDKPlugin(IAgent agent) : base(agent, PluginLogId)
        {
            _agent = agent;
            _logs ??= new Logs(Log, PluginLogId);
            FindPlatinumClientSdkService();
            _logs.DebugMsg_1($"[PlatinumSDKPlugin] PlatinumSDKPlugin constructor ...(Admin:{_IsAdministrator})");
        }

        #endregion Constructor

        #region IPlatinumSDKService implementation

        public Task<bool> UpdateEventValue(string EventTag, string EventValue, DataClassificationId _dcid = DataClassificationId.Restricted)
        {
            try
            {
                if (IsTelemetryConsentOn)
                {
                    if (_platinumClientSdk != null)
                    {
                        TransmissionId transmissionId;

                        if (string.IsNullOrWhiteSpace(EventValue))
                            transmissionId = _platinumClientSdk.LogEventAsync(EventTag, _dcid).Result;
                        else
                            transmissionId = _platinumClientSdk.LogEventAsync(EventTag, EventValue, _dcid).Result;

                        _logs.DebugMsg_1($"[PlatinumSDKPlugin] UpdateEventValue Logged event with transmission ID {transmissionId}");

                        TransmissionStatus status = _platinumClientSdk.GetTransmissionStatusAsync(transmissionId).Result;

                        switch (status.State)
                        {
                            case TransmissionState.Queued:
                                return Task.FromResult(true);

                            case TransmissionState.Successful:
                                return Task.FromResult(true);

                            case TransmissionState.FailedAndEnqueued:
                                return Task.FromResult(false);

                            case TransmissionState.FailedAndIgnored:
                                return Task.FromResult(false);

                            default:
                                return Task.FromResult(false);
                        }
                    }
                    else
                        return Task.FromResult(false);
                }

                _logs.DebugMsg_1("[PlatinumSDKPlugin] IsTelemetryConsentOn is False");
                return Task.FromResult(false);
            }
            catch (Exception ex)
            {
                _logs.DebugMsg_1($"[PlatinumSDKPlugin] UpdateEventValue ex {ex.Message}");
                return Task.FromResult(false);
            }
        }

        public Task<bool> UpdateEventValueforPeripheral(string EventTag, IEnumerable<KeyValuePair<string, string>> EventValue, DataClassificationId _dcid = DataClassificationId.Restricted)
        {
            try
            {
                //----------- [To IndiLogic] -----------//

                _logs.DebugMsg_1(@"[PlatinumSDKPlugin][To IndiLogic] Hi IndiLogic, You have successfully reach the Telemetry's Update function => UpdateEventValueforPeripheral() ");
                _logs.DebugMsg_1($"[PlatinumSDKPlugin][To IndiLogic] UpdateEventValueforPeripheral() EventTag => {EventTag}");
                _logs.DebugMsg_1(@"[PlatinumSDKPlugin][To IndiLogic] UpdateEventValueforPeripheral() EventValue :");

                foreach (var item in EventValue)
                    _logs.DebugMsg_1($"[PlatinumSDKPlugin][To IndiLogic] UpdateEventValueforPeripheral() EventValue => {item.Key} : {item.Value}");

                //----------- [To IndiLogic] -----------//

                if (IsTelemetryConsentOn)
                {
                    if (_platinumClientSdk != null)
                    {
                        TransmissionId transmissionId;

                        if (EventValue != null && EventValue.Count() > 0)
                            transmissionId = _platinumClientSdk.LogEventAsync(EventTag, _dcid).Result;
                        else
                            transmissionId = _platinumClientSdk.LogEventAsync(EventTag, EventValue, _dcid).Result;

                        _logs.DebugMsg_1($"[PlatinumSDKPlugin] UpdateEventValueforPeripheral Logged event with transmission ID {transmissionId}");

                        TransmissionStatus status = _platinumClientSdk.GetTransmissionStatusAsync(transmissionId).Result;

                        switch (status.State)
                        {
                            case TransmissionState.Queued:
                                return Task.FromResult(true);

                            case TransmissionState.Successful:
                                return Task.FromResult(true);

                            case TransmissionState.FailedAndEnqueued:
                                return Task.FromResult(false);

                            case TransmissionState.FailedAndIgnored:
                                return Task.FromResult(false);

                            default:
                                return Task.FromResult(false);
                        }
                    }
                    else
                        return Task.FromResult(false);
                }

                _logs.DebugMsg_1("[PlatinumSDKPlugin] IsTelemetryConsentOn is False");
                return Task.FromResult(false);
            }
            catch (Exception ex)
            {
                _logs.DebugMsg_1($"[PlatinumSDKPlugin] UpdateEventValueforPeripheral ex {ex.Message}");
                return Task.FromResult(false);
            }
        }

        public Task SetGlobalsetting_IsTelemetryConsentOn(bool value)
        {
            _logs.DebugMsg_1($"[PlatinumSDKPlugin] received Globalsetting_IsTelemetryConsentOn : ${value.ToString()}");

            IsTelemetryConsentOn = value;

            return Task.FromResult(Task.CompletedTask);
        }

        #endregion IPlatinumSDKService implementation

        #region Overriding methods

        public override AgentPluginInfo GetAgentPluginInfo() => _agentPluginInfo;

        protected override void OnPluginStarting()
        {
            //---------------------------------------------------------
            try
            {
                _agent.PluginManager.PluginsStarted += PluginManagerOnPluginsStarted;
                base.OnPluginStarting();
                FindPlatinumClientSdkService();
            }
            catch (Exception ex)
            {
                _logs.DebugMsg_1($"[PlatinumSDKPlugin] OnPluginStarting ex: {ex.Message}");
            }
            //---------------------------------------------------------
        }

        #endregion Overriding methods

        #region Private methods

        private void FindPlatinumClientSdkService()
        {
            try
            {
                if (_platinumClientSdk != null)
                    return;

                _platinumClientSdk = (IPlatinumClientSdk)_agent.FindPluginByType(typeof(IPlatinumClientSdk));
            }
            catch (Exception ex)
            {
                _logs.DebugMsg_1("[PlatinumSDKPlugin] FindPlatinumClientSdkService ex: " + ex.Message);
            }
        }

        private void TryInitializePlatinumClientSdk()
        {
            //---------------------------------------------------------
            try
            {
                lock (_lockobject)
                {
                    FindPlatinumClientSdkService();

                    if (_platinumClientSdk != null)
                        _logs.DebugMsg_1("[PlatinumSDKPlugin] IPlatinumClientSdk is not null");
                    else
                        _logs.DebugMsg_1("[PlatinumSDKPlugin] IPlatinumClientSdk is null");

                    if (_platinumClientSdk != null && (!IsSucessInitializeAsync))
                    {
                        _platinumClientSdk.InitializeAsync(new ClientAppId(new Guid("b397b9b3-04cb-4cdf-8a79-852d63cf4801"))).Wait();
                        IsSucessInitializeAsync = true;
                        _logs.DebugMsg_1($"[PlatinumSDKPlugin] InitializeAsync correct ...");
                    }
                }
            }
            catch (Exception ex)
            {
                _logs.DebugMsg_1($"[PlatinumSDKPlugin] OnPluginStarted ex: {ex.Message}");
            }
            //---------------------------------------------------------
        }

        #endregion Private methods

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
            _logs.DebugMsg_1($"Dispose: {disposing}");
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

        #endregion IDisposableObservable Support

        #region Event Handler

        private void PluginManagerOnPluginsStarted(object sender, PluginsStartedEventArgs e)
        {
            if (e == null)
                return;
            if (e.ChangedPlugins == null)
                return;
            if (e.ChangedPlugins.Any() == false)
                return;

            TryInitializePlatinumClientSdk();
        }

        #endregion Event Handler
    }
}