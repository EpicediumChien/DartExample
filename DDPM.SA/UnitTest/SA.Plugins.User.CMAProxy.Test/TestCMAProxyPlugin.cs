using DDPM.SA.Common;
using DDPM.SA.Plugins.User.CMAProxy;
using DDPM.SA.Plugins.User.EasyArrange;
using Dell.Client.Framework.Interfaces;
using Dell.Client.Framework.UnitTestShared.Tests;
using Moq;
using System.Windows.Input;
using VcpCore.Common;
using IDs = DDPM.SA.Common.IDs;

namespace DDPM.SA.Plugins.User.CMAProxy.Test
{
    public class TestCMAProxyPlugin
    {
        private Mock<IAgent> DisplayMangerAgent { get; } = new();
        private Mock<IAgent> VcpCoreAgent { get; } = new();
        private Mock<IAgent> CMAProxyPluginAgent { get; } = new();

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

        private CMAProxyPlugin CreateCMAProxyPlugin()
        {
            CMAProxyPluginAgent.Setup(x => x.PluginManager.FindPluginByGuid(Guid.Parse(IDs.DDPM_CMA_Proxy_Plugin)));

            return new CMAProxyPlugin(CMAProxyPluginAgent.Object);
        }

        private CMAProxyPlugin cMAProxyPlugin;
        private PrivateObject privateteCMAProxyPlugin;

        [OneTimeSetUp]
        public void Setup()
        {
            cMAProxyPlugin = CreateCMAProxyPlugin();
            privateteCMAProxyPlugin = new PrivateObject(cMAProxyPlugin);
        }

        [Test]
        public void TestCMAProxyPlugins()
        {
            Assert.IsNotNull(cMAProxyPlugin);
            PrivateObject privateteCMAProxyPlugin = new PrivateObject(cMAProxyPlugin);
            var agent2 = privateteCMAProxyPlugin.GetField("_agent") as IAgent;
            Assert.That(agent2, Is.EqualTo(CMAProxyPluginAgent.Object));
        }

        [Test]
        public void TestCMAProxyPluginLogId()
        {
            Assert.IsNotNull(cMAProxyPlugin);
            string pluginLogId = "CMAProxy";
            var CMAProxyPluginLogId_result = CMAProxyPlugin.PluginLogId;
            Assert.That(pluginLogId, Is.EqualTo(CMAProxyPluginLogId_result));
        }

        [Test]
        public void TestIsDisposed()
        {
            var Result = cMAProxyPlugin.IsDisposed;
            Assert.IsFalse(Result);
            PrivateObject privatetecMAProxyPluginObj = new PrivateObject(cMAProxyPlugin);
            privatetecMAProxyPluginObj.SetFieldOrProperty("IsDisposed", true);
            var Result2 = cMAProxyPlugin.IsDisposed;
            Assert.IsTrue(Result2);
        }

        [Test]
        public void TestInitializeCMAManagerPlugin()
        {
            Mock<ICMAManagerSA> mockCMAManagerSA = new Mock<ICMAManagerSA>();
            var CMAManagerSAObject = mockCMAManagerSA.Object;
            privateteCMAProxyPlugin.SetFieldOrProperty("_CMAManagerPlugin", CMAManagerSAObject);  //_CMAManagerPlugin not null
            privateteCMAProxyPlugin.Invoke("InitializeCMAManagerPlugin");
            var InitializeCMAManagerPlugin_result = privateteCMAProxyPlugin.GetFieldOrProperty("_CMAManagerPlugin");
            Assert.IsNotNull(InitializeCMAManagerPlugin_result);
        }

        [Test]
        public void TestInitializeDevManagerPlugin()
        {
            Mock<IDeviceManagerSA> mockDeviceManagerSA = new Mock<IDeviceManagerSA>();
            var DeviceManagerSAObject = mockDeviceManagerSA.Object;
            privateteCMAProxyPlugin.SetFieldOrProperty("_DevManagerPlugin", DeviceManagerSAObject);  //_DevManagerPlugin not null
            privateteCMAProxyPlugin.Invoke("InitializeDevManagerPlugin");
            var InitializeDevManagerPlugin_result = privateteCMAProxyPlugin.GetFieldOrProperty("_DevManagerPlugin");
            Assert.IsNotNull(InitializeDevManagerPlugin_result);
        }

        [Test]
        public void TestDoRelayRegister()
        {
            ICMAManagerSA? CMAManagerSA = null;
            IDeviceManagerSA? DevManagerPlugin = null;
            privateteCMAProxyPlugin.SetFieldOrProperty("_DevManagerPlugin", DevManagerPlugin);
            privateteCMAProxyPlugin.SetFieldOrProperty("_CMAManagerPlugin", CMAManagerSA);
            privateteCMAProxyPlugin.Invoke("DoRelayRegister");
            var DevManagerPlugin_result = privateteCMAProxyPlugin.GetFieldOrProperty("_DevManagerPlugin");    //_CMAManagerPlugin null
            var CMAManagerPlugin_result = privateteCMAProxyPlugin.GetFieldOrProperty("_CMAManagerPlugin");
            var relay_registered_result = (bool)privateteCMAProxyPlugin.GetFieldOrProperty("relay_registered");
            Assert.IsNull(DevManagerPlugin_result);
            Assert.IsNull(CMAManagerPlugin_result);
            Assert.IsFalse(relay_registered_result);

            var mockCMAManagerPlugin = new Mock<ICMAManagerSA>();
            var mockDevManagerPlugin = new Mock<IDeviceManagerSA>();

            var CMAManagerPluginobj = mockCMAManagerPlugin.Object;
            var DevManagerPluginobj = mockDevManagerPlugin.Object;
            privateteCMAProxyPlugin.SetFieldOrProperty("_DevManagerPlugin", DevManagerPluginobj);
            privateteCMAProxyPlugin.SetFieldOrProperty("_CMAManagerPlugin", CMAManagerPluginobj);

            privateteCMAProxyPlugin.Invoke("DoRelayRegister");
            var DevManagerPlugin_result2 = privateteCMAProxyPlugin.GetFieldOrProperty("_DevManagerPlugin");   ////_CMAManagerPlugin not null
            var CMAManagerPlugin_result2 = privateteCMAProxyPlugin.GetFieldOrProperty("_CMAManagerPlugin");
            var relay_registered_result2 = (bool)privateteCMAProxyPlugin.GetFieldOrProperty("relay_registered");
            Assert.IsNotNull(DevManagerPlugin_result2);
            Assert.IsNotNull(CMAManagerPlugin_result2);
            Assert.IsTrue(relay_registered_result2);
        }

        /*[Test]
        public void TestGetDdpmDevices()
        {
            IDeviceManagerSA? DevManagerPlugin = null;
            privateteCMAProxyPlugin.SetFieldOrProperty("_DevManagerPlugin", DevManagerPlugin);
            privateteCMAProxyPlugin.Invoke("GetDdpmDevices", "all");
            var DevManagerPlugin_result = privateteCMAProxyPlugin.GetFieldOrProperty("_DevManagerPlugin");    //_DevManagerPlugin null
            Assert.IsNull(DevManagerPlugin_result);

            List<MonitorInfo> monitorInfos = new List<MonitorInfo>();
            monitorInfos.Add(monitorInfo1);

            DeviceHelper deviceHelper = new DeviceHelper()
            {
                deviceInfo = new List<DeviceInfo>()
                {
                    new DeviceInfo()
                    {
                        DeviceName="Mouse",
                        Name="Test mouse",
                        DpiLevel=20,
                        DpiValue="20",
                    }
                },
                DCFVersion = "1.0",
                DPeMSDKVersion = "1.0",
                DPeMSubAgentVersion = "1.0",
                IsdDriverVersion = "1.0",
            };

            string condition = "displaychanged";
            Mock<ICMAManagerSA> mockCMAManagerPlugin = new Mock<ICMAManagerSA>();
            Mock<IDeviceManagerSA> mockDevManagerPlugin = new Mock<IDeviceManagerSA>();

            mockDevManagerPlugin.Setup(x => x.GetMonitors()).Returns(Task.FromResult(monitorInfos));
            mockDevManagerPlugin.Setup(x => x.GetDevices(It.IsAny<bool>())).Returns(Task.FromResult(deviceHelper));
            mockCMAManagerPlugin.Setup(x => x.Update_DeviceChanged(It.IsAny<CMADeviceChanges>()));

            var CMAManagerPluginobj = mockCMAManagerPlugin.Object;
            var DevManagerPluginobj = mockDevManagerPlugin.Object;
            privateteCMAProxyPlugin.SetFieldOrProperty("_DevManagerPlugin", DevManagerPluginobj);   //_DevManagerPlugin not null,_CMAManagerPlugin not null
            privateteCMAProxyPlugin.SetFieldOrProperty("_CMAManagerPlugin", CMAManagerPluginobj);

            privateteCMAProxyPlugin.Invoke("GetDdpmDevices", condition);
            Assert.IsTrue(true);
            mockDevManagerPlugin.Verify(p => p.GetMonitors(), Times.Once);
            mockCMAManagerPlugin.Verify(p => p.Update_DeviceChanged(It.Is<CMADeviceChanges>(c => c.type == "display" && c.mos == monitorInfos && c.devices == null)), Times.Once);
        }*/

        [OneTimeTearDown]
        public void TearDown()
        {
            cMAProxyPlugin.Dispose();
        }
    }
}