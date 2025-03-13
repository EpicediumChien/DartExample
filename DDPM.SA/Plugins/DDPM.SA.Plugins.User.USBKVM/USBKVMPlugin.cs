using DDPM.SA.Common;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.Common.Annotations;
using Dell.Client.Framework.Common.PluginConditions;
using Dell.Client.Framework.Interfaces;
using Microsoft;
using VcpCore.Common;
using IDs = DDPM.SA.Common.IDs;

namespace DDPM.SA.Plugins.User.USBKVM
{
    [Plugin(IDs.DDPM_USBKVM_PLUGIN_ID, pluginName, PluginOrderGroupType.Core, Version = pluginVersion)]
    [Descriptor(Description = pluginDescription)]
    [Publisher(Name = publisherCompany, Website = publisherWebsite, Support = publisherSupport)]
    [PublishedUnelevatedInterface(new[] { typeof(IUSBKVMService) })]
    public class USBKVMPlugin : BaseAgentPlugin, IUSBKVMService, IDisposableObservable
    {
        #region Private Members

        private const string pluginName = "USBKVMPlugin";
        private const string pluginVersion = "1.0.0";
        private const string pluginDescription = "This plugin implements USBKVM Plugin.";
        private const string publisherCompany = "Dell Technologies";
        private const string publisherWebsite = "https://www.dell.com";
        private const string publisherSupport = "This plugin implements USBKVM Plugin.";

        private IAgent _agent;
        private Logs _logs;
        public const string PluginLogId = "USBKVM";

        private IDeviceManagerSA _DeviceManagerPlugin;
        private readonly object _pluginConditionLock = new object();
        private PluginCondition _DeviceManagerPluginCondition;

        private Dictionary<string, PCsInfo> _PCsList = new Dictionary<string, PCsInfo>();
        private List<UInt16> _SubInputList = new List<UInt16>();
        //private Dictionary<string, InputInfo> _inputSourcelist = new Dictionary<string, InputInfo>();

        #endregion Private Members

        #region Constructor

        public USBKVMPlugin(IAgent agent) : base(agent, PluginLogId)
        {
            _agent = agent;
        }

        #endregion Constructor

        #region Overriding methods

        protected override void OnPluginStarting()
        {
            PluginCondition = new PluginStartedCondition();
            _agent.PluginManager.PluginsStarted += PluginManagerOnPluginsStarted;
        }

        #endregion Overriding methods

        #region IUSBKVM implementation

        public Task<Dictionary<string, PCsInfo>> GetUSBKVMPCsList(MonitorInfo monitorInfo, Dictionary<string, InputInfo> inputList)
        {
            _PCsList = new Dictionary<string, PCsInfo>();
            _SubInputList = new List<UInt16>();
            string currentInput = monitorInfo.inputSource;

            _SubInputList = _DeviceManagerPlugin.GetSubInputList(monitorInfo).Result;

            PCsInfo PC = new PCsInfo();
            PC.InputType = currentInput;
            PC.InputName = inputList[currentInput].InputName;
            PC.USBUpstream = inputList[currentInput].USBUpstream;
            _PCsList.Add("PC1", PC);

            int loop = _SubInputList.Count + 1; // get input sub
            for (int i = 1; i < loop; i++)
            {
                PC = new PCsInfo();
                foreach (var item in inputList)
                {
                    if (item.Key != currentInput)
                    {
                        PC.InputType = item.Key;
                        PC.InputName = item.Value.InputName;
                        PC.USBUpstream = item.Value.USBUpstream;
                        _PCsList.Add("PC" + (i + 1).ToString(), PC);
                        break;
                    }
                }
            }

            return Task.FromResult(_PCsList);
        }

        public Task<Dictionary<string, PCsInfo>> PCInfoSwap(Dictionary<string, PCsInfo> pcsList, string swapinput1, string swapinput2)
        {
            PCsInfo pcSwap1 = new PCsInfo();
            PCsInfo pcSwap2 = new PCsInfo();

            pcSwap1 = pcsList[swapinput1];
            pcSwap2 = pcsList[swapinput2];

            pcsList[swapinput1] = pcSwap2;
            pcsList[swapinput2] = pcSwap1;

            return Task.FromResult(pcsList);
        }

        #endregion IUSBKVM implementation

        #region Private Methods

        #endregion Private Methods

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
            if (!IsDisposed)
            {
                _logs.DebugMsg("[USBKVM] into Dispose ～～～～～～～～～～～～～～～～～！！！！！！！");

                IsDisposed = true;

                if (disposing)
                {
                    DisposeAction();

                    _agent.PluginManager.PluginsStarted -= PluginManagerOnPluginsStarted;
                    _agent = null;
                }
            }

            base.Dispose(disposing);
        }

        private void DisposeAction()
        {
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
            return;
        }

        private void PluginsStarted(object sender, PluginsStartedEventArgs e)
        {
            if (e?.ChangedPlugins == null)
                return;
            if (e.ChangedPlugins.Any() == false)
                return;
        }

        #endregion Event Handler
    }
}