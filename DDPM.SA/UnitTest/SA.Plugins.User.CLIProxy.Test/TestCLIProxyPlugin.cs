using DDPM.SA.Common;
using DDPM.SA.Plugin.User.CLIManager;
using DDPM.SA.Plugins.User.DisplayManager;
using Dell.Client.Framework.Interfaces;
using Dell.Client.Framework.UnitTestShared.Tests;
using Moq;
using VcpCore.Common;
using VcpCore.Interfaces;
using VcpCore.Plugins;
using IDs = DDPM.SA.Common.IDs;

namespace DDPM.SA.Plugin.User.CLIManage.Test
{
    public class TestCLIProxyPlugin
    {
        private Mock<IAgent> DisplayMangerAgent { get; } = new();
        private Mock<IAgent> VcpCoreAgent { get; } = new();
        private Mock<IAgent> CLIProxyPluginAgent { get; } = new();

        MonitorInfo monitorInfo = new MonitorInfo();
        private Mock<IDeviceManagerSA> DeviceManagerSAService { get; } = new();
        private IDeviceManagerSA? _deviceManagerPlugin;

        MonitorInfo monitorInfo1 = new MonitorInfo()
        {
            AliasDeviceName = "Dell U2724DE(HDMI)",
            IsDellMonitor = true,
            Index = 0,
            CapabilityString = "(prot(monitor)type(LCD)model(U2424H)cmds(01 02 03 07 0C E3 F3)vcp(02 04 05 08 10 12 14(01 04 05 06 08 09 0B 0C)E5 E7(02 03) E2(00 02 04 0C 0D 0F)",
            DisplayName = "TestDISPLAY7",
            DDCisON = true,
            FwVersion = "M3T101",
            inputSource = "HDMI-1",
            modelName = "TestU2724DE",
            series = "Dell UltraSharp (U) Series Monitors",
            D_Ctrl = "Test D_Ctrl",
            SupplierID = "Test SupplierID",
            //CapabilityDic = capabilityDic;
            CapabilityDic = new Dictionary<string, List<string>>(),
            edid = new EDID()
            {
                ManufactureID = "DEL",
                VendorID = "42DC",
                Year = 2023,
                Month = 5,
                Week = 22,
                ModelName = "DELLU2724DE",
                EdidVersion = "V1.3",
                VideoInputType = "Digital Signal",
                Size = 27.1510868f,
                ServiceTag = "CN073K0",
                SerialNumber = "808597589",
                Edid = "00FFFFFFFFFFFF0010ACDC425538323016210103803C2278EA62A5AD5046AB240E5054A54B00714F8180A940D1C081C0A9C001010101565E00A0A0A029503020350055502100001A000000FF00434E3037334B300A2020202020000000FC0044454C4C20553237323444450A000000FD0030781EB23C000A20202020202001ED"

            },
        };

        private CLIProxyPlugin CreateCLIProxyPlugin()
        {
            CLIProxyPluginAgent.Setup(x => x.PluginManager.FindPluginByGuid(Guid.Parse(IDs.DDPM_CLI_Proxy_Plugin)));

            return new CLIProxyPlugin(CLIProxyPluginAgent.Object);
        }

        private CLIProxyPlugin cLIProxyPlugin;
        private PrivateObject privateteCLIProxyPlugin;

        [OneTimeSetUp]
        public void Setup()
        {
            cLIProxyPlugin = CreateCLIProxyPlugin();
            privateteCLIProxyPlugin = new PrivateObject(cLIProxyPlugin);
        }

        [Test]
        public void TestCLIProxyPlugins()
        {
            Assert.IsNotNull(cLIProxyPlugin);
            PrivateObject privateteCLIProxyPlugin = new PrivateObject(cLIProxyPlugin);
            var agent2 = privateteCLIProxyPlugin.GetField("_agent") as IAgent;
            Assert.That(agent2, Is.EqualTo(CLIProxyPluginAgent.Object));
        }

        [Test]
        public void TestCLIProxyPluginLogId()
        {
            Assert.IsNotNull(cLIProxyPlugin);
            string pluginLogId = "CLIProxy";
            var CLIProxyPluginLogId_result = CLIProxyPlugin.PluginLogId;
            Assert.That(pluginLogId, Is.EqualTo(CLIProxyPluginLogId_result));
        }

        [Test]
        public void TestIsDisposed()
        {
            var Result = cLIProxyPlugin.IsDisposed;
            Assert.IsFalse(Result);
            PrivateObject privateteCLIProxyPluginObj = new PrivateObject(cLIProxyPlugin);
            privateteCLIProxyPluginObj.SetFieldOrProperty("IsDisposed", true);
            var Result2 = cLIProxyPlugin.IsDisposed;
            Assert.IsTrue(Result2);
        }

        [Test]
        public void TestInitializeCliManagerPlugin()
        {
            Mock<ICliManagerSA> mockCliManagerSA = new Mock<ICliManagerSA>();
            var CliManagerSAObject = mockCliManagerSA.Object;
            privateteCLIProxyPlugin.SetFieldOrProperty("_CliManagerPlugin", CliManagerSAObject);  //_CliManagerPlugin not null
            privateteCLIProxyPlugin.Invoke("InitializeCliManagerPlugin");
            var InitializeCliManagerPlugin_result = privateteCLIProxyPlugin.GetFieldOrProperty("_CliManagerPlugin");
            Assert.IsNotNull(InitializeCliManagerPlugin_result);
        }

        [Test]
        public void TestInitializeDevManagerPlugin()
        {
            Mock<IDeviceManagerSA> mockDeviceManagerSA = new Mock<IDeviceManagerSA>();
            var DeviceManagerSAObject = mockDeviceManagerSA.Object;
            privateteCLIProxyPlugin.SetFieldOrProperty("_DevManagerPlugin", DeviceManagerSAObject);  //_DevManagerPlugin not null
            privateteCLIProxyPlugin.Invoke("InitializeDevManagerPlugin");
            var InitializeDevManagerPlugin_result = privateteCLIProxyPlugin.GetFieldOrProperty("_DevManagerPlugin");
            Assert.IsNotNull(InitializeDevManagerPlugin_result);
        }

        [Test]
        public void TestInitializeCLIDisplayPlugin()
        {
            Mock<ICLIDisplay> mockCLIDisplay = new Mock<ICLIDisplay>();
            var CLIDisplayObject = mockCLIDisplay.Object;
            privateteCLIProxyPlugin.SetFieldOrProperty("_CLIDisplay", CLIDisplayObject);  //_CLIDisplay not null
            privateteCLIProxyPlugin.Invoke("InitializeCLIDisplayPlugin");
            var InitializeCLIDisplayPlugin_result = privateteCLIProxyPlugin.GetFieldOrProperty("_CLIDisplay");
            Assert.IsNotNull(InitializeCLIDisplayPlugin_result);
        }

        [Test]
        public void TestInitializeCLIPeripheralsPlugin()
        {
            Mock<ICLIPeripherals> mockCLIPeripherals = new Mock<ICLIPeripherals>();
            var CLIPeripheralsObject = mockCLIPeripherals.Object;
            privateteCLIProxyPlugin.SetFieldOrProperty("_CLIPeripherals", CLIPeripheralsObject);  //_CLIPeripherals not null
            privateteCLIProxyPlugin.Invoke("InitializeCLIPeripheralsPlugin");
            var InitializeCLIPeripheralsPlugin_result = privateteCLIProxyPlugin.GetFieldOrProperty("_CLIPeripherals");
            Assert.IsNotNull(InitializeCLIPeripheralsPlugin_result);
        }

        [Test]
        public void TestDoRelayRegister()
        {
            ICliManagerSA? CliManagerSA = null;
            IDeviceManagerSA? DevManagerPlugin = null;
            privateteCLIProxyPlugin.SetFieldOrProperty("_DevManagerPlugin", DevManagerPlugin);
            privateteCLIProxyPlugin.SetFieldOrProperty("_CliManagerPlugin", CliManagerSA);
            privateteCLIProxyPlugin.Invoke("DoRelayRegister");
            var DevManagerPlugin_result = privateteCLIProxyPlugin.GetFieldOrProperty("_DevManagerPlugin");   //_CliManagerPlugin  null
            var CliManagerPlugin_result = privateteCLIProxyPlugin.GetFieldOrProperty("_CliManagerPlugin");
            var relay_registered_result = (bool)privateteCLIProxyPlugin.GetFieldOrProperty("relay_registered");
            Assert.IsNull(DevManagerPlugin_result);
            Assert.IsNull(CliManagerPlugin_result);
            Assert.IsFalse(relay_registered_result);

            var mockCMAManagerPlugin = new Mock<ICliManagerSA>();
            var mockDevManagerPlugin = new Mock<IDeviceManagerSA>();

            var CMAManagerPluginobj = mockCMAManagerPlugin.Object;
            var DevManagerPluginobj = mockDevManagerPlugin.Object;
            privateteCLIProxyPlugin.SetFieldOrProperty("_DevManagerPlugin", DevManagerPluginobj);
            privateteCLIProxyPlugin.SetFieldOrProperty("_CliManagerPlugin", CMAManagerPluginobj);

            privateteCLIProxyPlugin.Invoke("DoRelayRegister");
            var DevManagerPlugin_result2 = privateteCLIProxyPlugin.GetFieldOrProperty("_DevManagerPlugin");   //_CliManagerPlugin not null
            var CliManagerPlugin_result2 = privateteCLIProxyPlugin.GetFieldOrProperty("_CliManagerPlugin");
            var relay_registered_result2 = (bool)privateteCLIProxyPlugin.GetFieldOrProperty("relay_registered");
            Assert.IsNotNull(DevManagerPlugin_result2);
            Assert.IsNotNull(CliManagerPlugin_result2);
            Assert.IsTrue(relay_registered_result2);
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            cLIProxyPlugin.Dispose();
        }
    }
}