
using DDPM.SA.Plugins.User.SettingsManager;
using DDPM.SA.Common.Interfaces;
using DDPM.SA.Common;
using DDPM.SA.Plugins.User.DisplayManager;
using DDPM.SA.Plugins.User.DisplayProperties;
using DDPM.SA.Plugins.User.EasyArrange;
using DDPM.SA.Plugins.User.PipPbpManger;
using Dell.Client.Framework.Interfaces;
using Dell.Client.Framework.UnitTestShared.Tests;
using Moq;
using VcpCore.Common;
using VcpCore.Interfaces;
using VcpCore.Plugins;
using DDPM.SA.Common.Settings;
using System.Net.Http.Json;
using Newtonsoft.Json;
using Windows.Devices.Display.Core;
using Windows.Globalization;
using static DDPM.SA.Common.Settings.DDPMUserSettings;

namespace DDPM.SA.Plugins.User.SettingsManager.Test
{
    public class TestSettingsManagerSA
    {
        private Mock<IAgent> DisplayMangerAgent { get; } = new();
        private Mock<IAgent> VcpCoreAgent { get; } = new();
        private Mock<IAgent> SettingsManagerPluginAgent { get; } = new();

        private MonitorInfo monitorInfo = new MonitorInfo();
        private Mock<IVcpCoreService> VcpCoreService { get; } = new();
        private Mock<IPipPbpService> PipPbpService { get; } = new();
        private Mock<IEasyArrangeService> EasyArrangeService { get; } = new();
        private Mock<IDisplayProperties> DisplayPropertiesService { get; } = new();

        private MonitorInfo monitorInfo1 = new MonitorInfo()
        {
            AliasDeviceName = "Dell U2724DE(HDMI)",
            IsDellMonitor = true,
            Index = 0,
            CapabilityString = "(prot(monitor)type(LCD)model(U2424H)cmds(01 02 03 07 0C E3 F3)vcp(02 04 05 08 10 12 14(01 04 05 06 08 09 0B 0C)E5 E7(02 03) E2(00 02 04 0C 0D 0F)",
            DisplayName = "DISPLAY7",
            DDCisON = true,
            FwVersion = "M3T101",
            inputSource = "HDMI-1",
            modelName = "U2724DE",
            series = "Dell UltraSharp (U) Series Monitors",
            edid = new EDID() { SerialNumber = "808597589" },
            //CapabilityDic = capabilityDic;
        };

        private DisplayMangerPlugin CreateInitializeDisplayMangerPlugin()
        {
            DisplayMangerAgent.Setup(x => x.PluginManager.FindPluginByGuid(Guid.Parse(VcpCore.Common.IDs.Display_Manager_PLUGIN_ID)));

            return new DisplayMangerPlugin(DisplayMangerAgent.Object);
        }

        private VcpCorePlugin CreateInitializeVcpCorePlugin()
        {
            VcpCoreAgent.Setup(x => x.PluginManager.FindPluginByGuid(Guid.Parse(VcpCore.Common.IDs.VCP_CORE_PLUGIN_ID)));

            return new VcpCorePlugin(VcpCoreAgent.Object);
        }

        private SettingsManagerSA CreateInitializeSettingsManagerSAPlugin()
        {
            SettingsManagerPluginAgent.Setup(x => x.PluginManager.FindPluginByGuid(Guid.Parse(DDPM.SA.Common.IDs.DDPM_SETTINGSMANAGER_SA_PLUGIN_ID)));

            return new SettingsManagerSA(SettingsManagerPluginAgent.Object);
        }

        private DisplayMangerPlugin displayPlugin;
        private VcpCorePlugin vcpCorePlugin;
        private SettingsManagerSA SettingsManagerSAPlugin;

        [OneTimeSetUp]
        public void Setup()
        {
            displayPlugin = CreateInitializeDisplayMangerPlugin();
            vcpCorePlugin = CreateInitializeVcpCorePlugin();
            SettingsManagerSAPlugin = CreateInitializeSettingsManagerSAPlugin();
            PrivateObject privateObject = new PrivateObject(displayPlugin);
            PrivateObject privatevcp = new PrivateObject(vcpCorePlugin);
            privateObject.SetField("_VcpCorePlugin", vcpCorePlugin as IVcpCoreService);
        }

        [Test]
        public void TestSettingsManagerPlugin()
        {
            SettingsManagerSA settingsManagerSA_=new SettingsManagerSA(SettingsManagerPluginAgent.Object);
            Assert.IsNotNull(SettingsManagerSAPlugin);
            PrivateObject privateSettingsManagerObj = new PrivateObject(SettingsManagerSAPlugin);
            var agent2 = privateSettingsManagerObj.GetField("_agent") as IAgent;
            Assert.That(agent2, Is.EqualTo(SettingsManagerPluginAgent.Object));
        }

        [Test]
        public void TestIsDisposed()
        {
            var Result = SettingsManagerSAPlugin.IsDisposed;  
            Assert.IsFalse(Result);
            PrivateObject privateSettingsManagerObject = new PrivateObject(SettingsManagerSAPlugin);
            privateSettingsManagerObject.SetFieldOrProperty("IsDisposed", true);  //IsDisposed is true
            var Result2 = SettingsManagerSAPlugin.IsDisposed;
            Assert.IsTrue(Result2);
        }

        [Test]
        public void TestInitDDPMUserConfigFile()
        {
            PrivateObject privateSettingsManagerObject = new PrivateObject(SettingsManagerSAPlugin);
            var result= privateSettingsManagerObject.Invoke("InitDDPMUserConfigFile");
            var InitDDPMSettings = privateSettingsManagerObject.GetFieldOrProperty("_settings");
            Assert.That(InitDDPMSettings,Is.EqualTo(result));
        }


        [OneTimeTearDown]
        public void TearDown()
        {
            displayPlugin.Dispose();
            vcpCorePlugin.Dispose();
            SettingsManagerSAPlugin.Dispose();
        }

    }
}