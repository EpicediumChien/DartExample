using DDPM.SA.Common;
using DDPM.SA.Plugins.User.DTPProxy;
using DDPM.SA.Plugins.User.EasyArrange;
using Dell.Client.Framework.Agent;
using Dell.Client.Framework.Interfaces;
using Dell.Client.Framework.UnitTestShared.Tests;
using Dell.TechHub.Commodity;
using Dell.TechHub.Sdk.Common.Identifiers;
using Moq;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Reflection;
using VcpCore.Common;
using IDs = DDPM.SA.Common.IDs;

namespace DDPM.SA.Plugins.User.DTPProxy.Test
{
    public class TestDTPProxyPlugin
    {
        private Mock<IAgent> DisplayMangerAgent { get; } = new();
        private Mock<IAgent> VcpCoreAgent { get; } = new();
        private Mock<IAgent> DTPProxyPluginAgent { get; } = new();
        private Mock<ICommodityClientSdk> CommodityClientSdk { get; } = new();
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

        private DTPProxyPlugin CreateDTPProxyPlugin()
        {
            DTPProxyPluginAgent.Setup(x => x.PluginManager.FindPluginByGuid(Guid.Parse(IDs.DDPM_DTP_Proxy_Plugin)));

            return new DTPProxyPlugin(DTPProxyPluginAgent.Object);
        }

        private DTPProxyPlugin dTPProxyPlugin;
        private PrivateObject privateteDTPProxyPlugin;

        [OneTimeSetUp]
        public void Setup()
        {
            dTPProxyPlugin = CreateDTPProxyPlugin();
            privateteDTPProxyPlugin = new PrivateObject(dTPProxyPlugin);
            privateteDTPProxyPlugin.SetFieldOrProperty("_commSdk", CommodityClientSdk.Object);
        }

        [Test]
        public void TestDTPProxyPlugins()
        {
            Assert.IsNotNull(dTPProxyPlugin);
            var agent2 = privateteDTPProxyPlugin.GetFieldOrProperty("_agent") as IAgent;
            Assert.That(agent2, Is.EqualTo(DTPProxyPluginAgent.Object));
        }

        [Test]
        public void TestDTPProxyPluginLogId()
        {
            Assert.IsNotNull(dTPProxyPlugin);
            string pluginLogId = "DTPProxy";
            var DTPProxyPluginLogId_result = DTPProxyPlugin.PluginLogId;
            Assert.That(pluginLogId, Is.EqualTo(DTPProxyPluginLogId_result));
        }

        [Test]
        public void TestNotifyNow()
        {
            Mock<EventHandler<DeviceChangedEventArgs>> mockEventHandler = new Mock<EventHandler<DeviceChangedEventArgs>>();
            dTPProxyPlugin.Notify += mockEventHandler.Object;

            // Act
            dTPProxyPlugin.NotifyNow();

            // Assert
            mockEventHandler.Verify(h => h(It.IsAny<object>(), It.IsAny<DeviceChangedEventArgs>()), Times.Once);

        }

        [Test]
        public void TestOnUIUpdateNotify()
        {
            Mock<EventHandler<UpdateUINotify>> mockEventHandler = new Mock<EventHandler<UpdateUINotify>>();
            dTPProxyPlugin.DTPEventHandler += mockEventHandler.Object;
            UpdateUINotify e = new UpdateUINotify();
            // Act
            dTPProxyPlugin.OnUIUpdateNotify(e);

            // Assert
            mockEventHandler.Verify(h => h(It.IsAny<object>(), It.IsAny<UpdateUINotify>()), Times.Once);

        }

        [Test]
        public void TestGetDpiValue()
        {
            Guid MouseGuid = Guid.NewGuid();
            string Guid1 = MouseGuid.ToString();
            privateteDTPProxyPlugin.SetFieldOrProperty("_commSdk", CommodityClientSdk.Object);
            var GetDpiValue_result = dTPProxyPlugin.GetDpiValue(Guid1);
            Assert.IsNotNull(GetDpiValue_result);
            Assert.That(GetDpiValue_result.Result, Is.EqualTo(-1));
        }

        [Test]
        public void TestGetMouseAssignableActions()
        {
            Guid MouseGuid = Guid.NewGuid();
            string Guid1 = MouseGuid.ToString();
            privateteDTPProxyPlugin.SetFieldOrProperty("_mouseMethodInfo", new Mock<MethodInfo>().Object);
            privateteDTPProxyPlugin.SetFieldOrProperty("_commSdk", CommodityClientSdk.Object);

            var GetMouseAssignableActions_result = dTPProxyPlugin.GetMouseAssignableActions(Guid1);
            Assert.IsNotNull(GetMouseAssignableActions_result);
            Assert.That(GetMouseAssignableActions_result.Result, Is.EqualTo(new JArray()));
        }

        [Test]
        public void TestGetMouseProgrammableKeys()
        {
            Guid MouseGuid = Guid.NewGuid();
            string Guid1 = MouseGuid.ToString();
            privateteDTPProxyPlugin.SetFieldOrProperty("_mouseMethodInfo", new Mock<MethodInfo>().Object);
            privateteDTPProxyPlugin.SetFieldOrProperty("_commSdk", CommodityClientSdk.Object);

            var GetMouseProgrammableKeys_result = dTPProxyPlugin.GetMouseProgrammableKeys(Guid1).Result;
            Assert.IsNotNull(GetMouseProgrammableKeys_result);
            Assert.That(GetMouseProgrammableKeys_result, Is.EqualTo(new JArray()));
        }

        [Test]
        public void TestDeleteMouseAllAssignedActions()
        {
            Guid MouseGuid = Guid.NewGuid();
            string Guid1 = MouseGuid.ToString();
            privateteDTPProxyPlugin.SetFieldOrProperty("_mouseMethodInfo", new Mock<MethodInfo>().Object);
            privateteDTPProxyPlugin.SetFieldOrProperty("_commSdk", CommodityClientSdk.Object);

            var DeleteMouseAllAssignedActions_result = dTPProxyPlugin.DeleteMouseAllAssignedActions(Guid1).Result;
            Assert.IsNotNull(DeleteMouseAllAssignedActions_result);
            Assert.That(DeleteMouseAllAssignedActions_result, Is.EqualTo(false));
        }

        [Test]
        public void TestGetAppSpecificProfiles()
        {
            Guid MouseGuid = Guid.NewGuid();
            string Guid1 = MouseGuid.ToString();
            privateteDTPProxyPlugin.SetFieldOrProperty("_mouseMethodInfo", new Mock<MethodInfo>().Object);
            privateteDTPProxyPlugin.SetFieldOrProperty("_commSdk", CommodityClientSdk.Object);

            var GetAppSpecificProfiles_result = dTPProxyPlugin.GetAppSpecificProfiles(Guid1).Result;
            Assert.IsNotNull(GetAppSpecificProfiles_result);
            Assert.That(GetAppSpecificProfiles_result, Is.EqualTo(new JArray()));
        }

        [Test]
        public void TestGetMouseKeystrokeDisplayData()
        {
            Guid MouseGuid = Guid.NewGuid();
            string Guid1 = MouseGuid.ToString();
            privateteDTPProxyPlugin.SetFieldOrProperty("_mouseMethodInfo", new Mock<MethodInfo>().Object);
            privateteDTPProxyPlugin.SetFieldOrProperty("_commSdk", CommodityClientSdk.Object);

            var GetMouseKeystrokeDisplayData_result = dTPProxyPlugin.GetMouseKeystrokeDisplayData(Guid1).Result;
            Assert.IsNotNull(GetMouseKeystrokeDisplayData_result);
            Assert.That(GetMouseKeystrokeDisplayData_result, Is.EqualTo(string.Empty));
        }

        [Test]
        public void TestStartMouseKeystrokeRecording()
        {
            Guid MouseGuid = Guid.NewGuid();
            string Guid1 = MouseGuid.ToString();
            privateteDTPProxyPlugin.SetFieldOrProperty("_mouseMethodInfo", new Mock<MethodInfo>().Object);
            privateteDTPProxyPlugin.SetFieldOrProperty("_commSdk", CommodityClientSdk.Object);

            var StartMouseKeystrokeRecording_result = dTPProxyPlugin.StartMouseKeystrokeRecording(Guid1).Result;
            Assert.IsNotNull(StartMouseKeystrokeRecording_result);
            Assert.That(StartMouseKeystrokeRecording_result, Is.EqualTo(false));
        }

        [Test]
        public void TestStopMouseKeystrokeRecording()
        {
            Guid MouseGuid = Guid.NewGuid();
            string Guid1 = MouseGuid.ToString();
            privateteDTPProxyPlugin.SetFieldOrProperty("_mouseMethodInfo", new Mock<MethodInfo>().Object);
            privateteDTPProxyPlugin.SetFieldOrProperty("_commSdk", CommodityClientSdk.Object);

            var StopMouseKeystrokeRecording_result = dTPProxyPlugin.StopMouseKeystrokeRecording(Guid1).Result;
            Assert.IsNotNull(StopMouseKeystrokeRecording_result);
            Assert.That(StopMouseKeystrokeRecording_result, Is.EqualTo(false));
        }

        [Test]
        public void TestSetDpiValue()
        {
            int newValue = 1;
            Guid MouseGuid = Guid.NewGuid();
            string Guid1 = MouseGuid.ToString();
            privateteDTPProxyPlugin.SetFieldOrProperty("_mouseMethodInfo", new Mock<MethodInfo>().Object);
            privateteDTPProxyPlugin.SetFieldOrProperty("_commSdk", CommodityClientSdk.Object);

            var SetDpiValueg_result = dTPProxyPlugin.SetDpiValue(Guid1, newValue);
            Assert.IsNotNull(SetDpiValueg_result);
        }

        [Test]
        public void TestSetMouseAction()
        {
            byte[] newValue = new byte[] { 0x01, 0x02, 0x03 };
            Guid MouseGuid = Guid.NewGuid();
            string Guid1 = MouseGuid.ToString();
            privateteDTPProxyPlugin.SetFieldOrProperty("_mouseMethodInfo", new Mock<MethodInfo>().Object);
            privateteDTPProxyPlugin.SetFieldOrProperty("_commSdk", CommodityClientSdk.Object);

            var SetDpiValueg_result = dTPProxyPlugin.SetMouseAction(Guid1, newValue);
            Assert.IsNotNull(SetDpiValueg_result);
        }

        [Test]
        public void TestSetCurrentSelectedAppSpecificProfile()
        {
            string newValue = "123";
            Guid MouseGuid = Guid.NewGuid();
            string Guid1 = MouseGuid.ToString();
            privateteDTPProxyPlugin.SetFieldOrProperty("_mouseMethodInfo", new Mock<MethodInfo>().Object);
            privateteDTPProxyPlugin.SetFieldOrProperty("_commSdk", CommodityClientSdk.Object);

            var SetCurrentSelectedAppSpecificProfile_result = dTPProxyPlugin.SetCurrentSelectedAppSpecificProfile(Guid1, newValue);
            Assert.IsNotNull(SetCurrentSelectedAppSpecificProfile_result);
        }

        [Test]
        public void TestDeleteMouseAssignedAction()
        {
            int newValue = 1;
            Guid MouseGuid = Guid.NewGuid();
            string Guid1 = MouseGuid.ToString();
            privateteDTPProxyPlugin.SetFieldOrProperty("_mouseMethodInfo", new Mock<MethodInfo>().Object);
            privateteDTPProxyPlugin.SetFieldOrProperty("_commSdk", CommodityClientSdk.Object);

            var DeleteMouseAssignedAction_result = dTPProxyPlugin.DeleteMouseAssignedAction(Guid1, newValue);
            Assert.IsNotNull(DeleteMouseAssignedAction_result);
        }

        [Test]
        public void TestSetMouseAssignDialogAction()
        {
            byte[] newValue = new byte[] { 0x01, 0x02 };
            Guid MouseGuid = Guid.NewGuid();
            string Guid1 = MouseGuid.ToString();
            privateteDTPProxyPlugin.SetFieldOrProperty("_mouseMethodInfo", new Mock<MethodInfo>().Object);
            privateteDTPProxyPlugin.SetFieldOrProperty("_commSdk", CommodityClientSdk.Object);

            var SetMouseAssignDialogAction_result = dTPProxyPlugin.SetMouseAssignDialogAction(Guid1, newValue);
            Assert.IsNotNull(SetMouseAssignDialogAction_result);
        }

        [Test]
        public void TestSetMouseAssignKeystrokeAction()
        {
            byte[] newValue = new byte[] { 0x01, 0x02, 0x03 };
            Guid MouseGuid = Guid.NewGuid();
            string Guid1 = MouseGuid.ToString();
            privateteDTPProxyPlugin.SetFieldOrProperty("_mouseMethodInfo", new Mock<MethodInfo>().Object);
            privateteDTPProxyPlugin.SetFieldOrProperty("_commSdk", CommodityClientSdk.Object);

            var SetMouseAssignKeystrokeAction_result = dTPProxyPlugin.SetMouseAssignKeystrokeAction(Guid1, newValue);
            Assert.IsNotNull(SetMouseAssignKeystrokeAction_result);
        }

        [Test]
        public void TestRestoreToDefaultMouse()
        {
            bool isFromCli;
            Guid MouseGuid = Guid.NewGuid();
            string Guid1 = MouseGuid.ToString();
            isFromCli = false;
            privateteDTPProxyPlugin.SetFieldOrProperty("IsDTPReady", false);

            var RestoreToDefaultMouse_result = dTPProxyPlugin.RestoreToDefaultMouse(Guid1, isFromCli).Result; //IsDTPReady false
            Assert.That(RestoreToDefaultMouse_result, Is.EqualTo(false));

            privateteDTPProxyPlugin.SetFieldOrProperty("IsDTPReady", true);
            var RestoreToDefaultMouse_result2 = dTPProxyPlugin.RestoreToDefaultMouse(Guid1, isFromCli).Result; //IsDTPReady true
            Assert.IsNotNull(RestoreToDefaultMouse_result2);
            Assert.That(RestoreToDefaultMouse_result2, Is.EqualTo(false));
        }

        [Test]
        public void TestSetReportRate()
        {
            int newValue = 2;
            Guid MouseGuid = Guid.NewGuid();
            string Guid1 = MouseGuid.ToString();
            privateteDTPProxyPlugin.SetFieldOrProperty("_mouseMethodInfo", new Mock<MethodInfo>().Object);
            privateteDTPProxyPlugin.SetFieldOrProperty("_commSdk", CommodityClientSdk.Object);

            var SetReportRate_result = dTPProxyPlugin.SetReportRate(Guid1, newValue).Result;
            Assert.IsNotNull(SetReportRate_result);
            Assert.That(SetReportRate_result, Is.EqualTo(false));
        }

        [Test]
        public void TestDeleteKeyboardAssignedAction()
        {
            int newValue = 2;
            Guid KeyboardGuid = Guid.NewGuid();
            string Guid1 = KeyboardGuid.ToString();
            privateteDTPProxyPlugin.SetFieldOrProperty("_mouseMethodInfo", new Mock<MethodInfo>().Object);
            privateteDTPProxyPlugin.SetFieldOrProperty("_commSdk", CommodityClientSdk.Object);

            var DeleteKeyboardAssignedAction_result = dTPProxyPlugin.DeleteKeyboardAssignedAction(Guid1, newValue);
            Assert.IsNotNull(DeleteKeyboardAssignedAction_result);
        }

        [Test]
        public void TestGetKbProgrammableKeys()
        {
            Guid KeyboardGuid = Guid.NewGuid();
            string Guid1 = KeyboardGuid.ToString();
            privateteDTPProxyPlugin.SetFieldOrProperty("_keyboardMethodInfo", new Mock<MethodInfo>().Object);
            privateteDTPProxyPlugin.SetFieldOrProperty("_commSdk", CommodityClientSdk.Object);

            var GetKbProgrammableKeys_result = dTPProxyPlugin.GetKbProgrammableKeys(Guid1).Result;
            Assert.IsNotNull(GetKbProgrammableKeys_result);
            Assert.That(GetKbProgrammableKeys_result, Is.EqualTo(new JArray()));
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            dTPProxyPlugin.Dispose();
        }
    }
}