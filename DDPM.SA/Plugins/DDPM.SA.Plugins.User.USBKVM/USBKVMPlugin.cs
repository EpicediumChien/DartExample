using DDPM.SA.Common;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.Common.Annotations;
using Dell.Client.Framework.Common.PluginConditions;
using Dell.Client.Framework.Interfaces;
using VcpCore.Common;
using IDs = DDPM.SA.Common.IDs;

namespace DDPM.SA.Plugins.User.USBKVM
{
    [Plugin(IDs.DDPM_USBKVM_PLUGIN_ID, pluginName, PluginOrderGroupType.Core, Version = pluginVersion)]
    [Descriptor(Description = pluginDescription)]
    [Publisher(Name = publisherCompany, Website = publisherWebsite, Support = publisherSupport)]
    [PublishedUnelevatedInterface(new[] { typeof(IUSBKVMService) })]
    public class USBKVMPlugin : BaseAgentPlugin, IUSBKVMService
    {
        #region Private Members

        private const string pluginName = "USBKVMPlugin";
        private const string pluginVersion = "1.0.0";
        private const string pluginDescription = "This plugin implements USBKVM Plugin.";
        private const string publisherCompany = "Wistron";
        private const string publisherWebsite = "https://www.wistron.com";
        private const string publisherSupport = "This plugin implements USBKVM Plugin.";

        private IAgent _agent;
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
            InitializeDeviceManagerPlugin();
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

        private void InitializeDeviceManagerPlugin()
        {
            if (_DeviceManagerPlugin != null)
                return;

            _DeviceManagerPlugin = _agent.PluginManager.FindPluginByType<IDeviceManagerSA>(PluginResolution.Dynamic);
            if (_DeviceManagerPlugin is IFrameworkPluginConditionNotification condition)
            {
                condition.PluginConditionChangeHandler += OnDeviceManagerPluginConditionChangeHandler;
                GetCurrentDeviceManagerPluginCondition();
            }
        }

        private void OnDeviceManagerPluginConditionChangeHandler(object sender, EventArgs e)
        {
            InitializeDeviceManagerPlugin();
        }

        private void GetCurrentDeviceManagerPluginCondition()
        {
            _ = Task.Run(async () =>
            {
                var pluginCondition = await (_DeviceManagerPlugin as IFrameworkPluginConditionNotification)?.CurrentConditionAsync();

                lock (_pluginConditionLock)
                {
                    _DeviceManagerPluginCondition = pluginCondition;
                }
            });
        }

        #endregion Private Methods

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

            if (e.ChangedPlugins.OfType<IDeviceManagerSA>().Any())
            {
                InitializeDeviceManagerPlugin();
            }
        }

        #endregion Event Handler
    }
}