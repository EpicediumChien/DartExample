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

        [Test]
        public void TestDeleteKeyboardAllAssignedActions()
        {
            Guid KeyboardGuid = Guid.NewGuid();
            string Guid1 = KeyboardGuid.ToString();
            privateteDTPProxyPlugin.SetFieldOrProperty("_keyboardMethodInfo", new Mock<MethodInfo>().Object);
            privateteDTPProxyPlugin.SetFieldOrProperty("_commSdk", CommodityClientSdk.Object);

            var DeleteKeyboardAllAssignedActions_result = dTPProxyPlugin.DeleteKeyboardAllAssignedActions(Guid1).Result;
            Assert.IsNotNull(DeleteKeyboardAllAssignedActions_result);
            Assert.That(DeleteKeyboardAllAssignedActions_result, Is.EqualTo(false));
        }

        [Test]
        public void TestGetKbAssignableActions()
        {
            Guid KeyboardGuid = Guid.NewGuid();
            string Guid1 = KeyboardGuid.ToString();
            privateteDTPProxyPlugin.SetFieldOrProperty("_keyboardMethodInfo", new Mock<MethodInfo>().Object);
            privateteDTPProxyPlugin.SetFieldOrProperty("_commSdk", CommodityClientSdk.Object);

            var GetKbAssignableActions_result = dTPProxyPlugin.GetKbAssignableActions(Guid1).Result;
            Assert.IsNotNull(GetKbAssignableActions_result);
            Assert.That(GetKbAssignableActions_result, Is.EqualTo(new JArray()));
        }

        [Test]
        public void TestGetKeyboardDeviceItemsEx()
        {
            privateteDTPProxyPlugin.SetFieldOrProperty("_keyboardMethodInfo", null);
            var GetKeyboardDeviceItemsEx_result = dTPProxyPlugin.GetKeyboardDeviceItemsEx().Result;  //_keyboardMethodInfo null 
            Assert.IsNotNull(GetKeyboardDeviceItemsEx_result);
            Assert.That(GetKeyboardDeviceItemsEx_result, Is.EqualTo(new JArray()));

            privateteDTPProxyPlugin.SetFieldOrProperty("_keyboardMethodInfo", new Mock<MethodInfo>().Object);
            privateteDTPProxyPlugin.SetFieldOrProperty("_commSdk", CommodityClientSdk.Object);
            var GetKeyboardDeviceItemsEx_result2 = dTPProxyPlugin.GetKeyboardDeviceItemsEx().Result;  //_keyboardMethodInfo not null 
            Assert.IsNotNull(GetKeyboardDeviceItemsEx_result2);
            Assert.That(GetKeyboardDeviceItemsEx_result2, Is.EqualTo(new JArray()));
        }

        [Test]
        public void TestGetKeyboardKeystrokeDisplayData()
        {
            Guid KeyboardGuid = Guid.NewGuid();
            string Guid1 = KeyboardGuid.ToString();
            privateteDTPProxyPlugin.SetFieldOrProperty("_keyboardMethodInfo", new Mock<MethodInfo>().Object);
            privateteDTPProxyPlugin.SetFieldOrProperty("_commSdk", CommodityClientSdk.Object);

            var GetKeyboardKeystrokeDisplayData_result = dTPProxyPlugin.GetKeyboardKeystrokeDisplayData(Guid1).Result;
            Assert.IsNotNull(GetKeyboardKeystrokeDisplayData_result);
            Assert.That(GetKeyboardKeystrokeDisplayData_result, Is.EqualTo(string.Empty));
        }

        [Test]
        public void TestStartKeyboardKeystrokeRecording()
        {
            Guid KeyboardGuid = Guid.NewGuid();
            string Guid1 = KeyboardGuid.ToString();
            privateteDTPProxyPlugin.SetFieldOrProperty("_keyboardMethodInfo", new Mock<MethodInfo>().Object);
            privateteDTPProxyPlugin.SetFieldOrProperty("_commSdk", CommodityClientSdk.Object);

            var StartKeyboardKeystrokeRecording_result = dTPProxyPlugin.StartKeyboardKeystrokeRecording(Guid1).Result;
            Assert.IsNotNull(StartKeyboardKeystrokeRecording_result);
            Assert.That(StartKeyboardKeystrokeRecording_result, Is.EqualTo(false));
        }

        [Test]
        public void TestStopKeyboardKeystrokeRecording()
        {
            Guid KeyboardGuid = Guid.NewGuid();
            string Guid1 = KeyboardGuid.ToString();
            privateteDTPProxyPlugin.SetFieldOrProperty("_keyboardMethodInfo", new Mock<MethodInfo>().Object);
            privateteDTPProxyPlugin.SetFieldOrProperty("_commSdk", CommodityClientSdk.Object);

            var StopKeyboardKeystrokeRecording_result = dTPProxyPlugin.StopKeyboardKeystrokeRecording(Guid1).Result;
            Assert.IsNotNull(StopKeyboardKeystrokeRecording_result);
            Assert.That(StopKeyboardKeystrokeRecording_result, Is.EqualTo(false));
        }

        [Test]
        public void TestSetKbAssignKeystrokeAction()
        {
            byte[] newValue = new byte[] { 0x01, 0x02 };
            Guid KeyboardGuid = Guid.NewGuid();
            string Guid1 = KeyboardGuid.ToString();
            privateteDTPProxyPlugin.SetFieldOrProperty("_keyboardMethodInfo", new Mock<MethodInfo>().Object);
            privateteDTPProxyPlugin.SetFieldOrProperty("_commSdk", CommodityClientSdk.Object);

            var SetKbAssignKeystrokeAction_result = dTPProxyPlugin.SetKbAssignKeystrokeAction(Guid1, newValue);
            Assert.IsNotNull(SetKbAssignKeystrokeAction_result);
        }

        [Test]
        public void TestSetKbAssignDialogAction()
        {
            byte[] newValue = new byte[] { 0x01, 0x03 };
            Guid KeyboardGuid = Guid.NewGuid();
            string Guid1 = KeyboardGuid.ToString();
            privateteDTPProxyPlugin.SetFieldOrProperty("_keyboardMethodInfo", new Mock<MethodInfo>().Object);
            privateteDTPProxyPlugin.SetFieldOrProperty("_commSdk", CommodityClientSdk.Object);

            var SetKbAssignDialogAction_result = dTPProxyPlugin.SetKbAssignDialogAction(Guid1, newValue);
            Assert.IsNotNull(SetKbAssignDialogAction_result);
        }

        [Test]
        public void TestSetKbAssignedAction()
        {
            byte[] newValue = new byte[] { 0x01, 0x02 };
            Guid KeyboardGuid = Guid.NewGuid();
            string Guid1 = KeyboardGuid.ToString();
            privateteDTPProxyPlugin.SetFieldOrProperty("_keyboardMethodInfo", new Mock<MethodInfo>().Object);
            privateteDTPProxyPlugin.SetFieldOrProperty("_commSdk", CommodityClientSdk.Object);

            var SetKbAssignedAction_result = dTPProxyPlugin.SetKbAssignedAction(Guid1, newValue);
            Assert.IsNotNull(SetKbAssignedAction_result);
        }

        [Test]
        public void TestRestoreToDefaultKB()
        {
            Guid KeyboardGuid = Guid.NewGuid();
            string Guid1 = KeyboardGuid.ToString();
            privateteDTPProxyPlugin.SetFieldOrProperty("IsDTPReady", false);

            var RestoreToDefaultKB_result = dTPProxyPlugin.RestoreToDefaultKB(Guid1).Result; //IsDTPReady false
            Assert.That(RestoreToDefaultKB_result, Is.EqualTo(false));

            privateteDTPProxyPlugin.SetFieldOrProperty("IsDTPReady", true);
            var RestoreToDefaultKB_result2 = dTPProxyPlugin.RestoreToDefaultKB(Guid1).Result; //IsDTPReady true
            Assert.IsNotNull(RestoreToDefaultKB_result2);
            Assert.That(RestoreToDefaultKB_result2, Is.EqualTo(false));
        }

        [Test]
        public void TestGetWebcamDeviceItemsEx()
        {
            byte[] newValue = new byte[] { 0x01, 0x02 };
            Guid KeyboardGuid = Guid.NewGuid();
            string Guid1 = KeyboardGuid.ToString();
            privateteDTPProxyPlugin.SetFieldOrProperty("_webcamMethodInfo", null);

            var GetWebcamDeviceItemsExAsync_result = dTPProxyPlugin.GetWebcamDeviceItemsExAsync().Result;  //_webcamMethodInfo null
            Assert.IsNotNull(GetWebcamDeviceItemsExAsync_result);
            Assert.That(GetWebcamDeviceItemsExAsync_result, Is.EqualTo(new JArray()));

            privateteDTPProxyPlugin.SetFieldOrProperty("_webcamMethodInfo", new Mock<MethodInfo>().Object);
            privateteDTPProxyPlugin.SetFieldOrProperty("_commSdk", CommodityClientSdk.Object);
            var GetWebcamDeviceItemsExAsync_result2 = dTPProxyPlugin.GetWebcamDeviceItemsExAsync().Result;  //_webcamMethodInfo null
            Assert.IsNotNull(GetWebcamDeviceItemsExAsync_result2);
            Assert.That(GetWebcamDeviceItemsExAsync_result2, Is.EqualTo(new JArray()));
        }

        [Test]
        public void TestGetPresetProfiles()
        {
            Guid WebcamGuid = Guid.NewGuid();
            string Guid1 = WebcamGuid.ToString();
            privateteDTPProxyPlugin.SetFieldOrProperty("_webcamMethodInfo", new Mock<MethodInfo>().Object);
            privateteDTPProxyPlugin.SetFieldOrProperty("_commSdk", CommodityClientSdk.Object);

            var GetPresetProfiles_result = dTPProxyPlugin.GetPresetProfiles(Guid1).Result;
            Assert.IsNotNull(GetPresetProfiles_result);
            Assert.That(GetPresetProfiles_result, Is.EqualTo(new JArray()));
        }

        [Test]
        public void TestGetCustomProfiles()
        {
            Guid WebcamGuid = Guid.NewGuid();
            string Guid1 = WebcamGuid.ToString();
            privateteDTPProxyPlugin.SetFieldOrProperty("_webcamMethodInfo", new Mock<MethodInfo>().Object);
            privateteDTPProxyPlugin.SetFieldOrProperty("_commSdk", CommodityClientSdk.Object);

            var GetCustomProfiles_result = dTPProxyPlugin.GetCustomProfiles(Guid1).Result;
            Assert.IsNotNull(GetCustomProfiles_result);
            Assert.That(GetCustomProfiles_result, Is.EqualTo(new JArray()));
        }

        [Test]
        public void TestGetProfileName()
        {
            Guid WebcamGuid = Guid.NewGuid();
            string Guid1 = WebcamGuid.ToString();
            privateteDTPProxyPlugin.SetFieldOrProperty("_webcamMethodInfo", new Mock<MethodInfo>().Object);
            privateteDTPProxyPlugin.SetFieldOrProperty("_commSdk", CommodityClientSdk.Object);

            var GetProfileName_result = dTPProxyPlugin.GetProfileName(Guid1).Result;
            Assert.IsNotNull(GetProfileName_result);
            Assert.That(GetProfileName_result, Is.EqualTo(string.Empty));
        }

        [Test]
        public void TestGetProfile()
        {
            Guid WebcamGuid = Guid.NewGuid();
            string Guid1 = WebcamGuid.ToString();
            privateteDTPProxyPlugin.SetFieldOrProperty("_webcamMethodInfo", new Mock<MethodInfo>().Object);
            privateteDTPProxyPlugin.SetFieldOrProperty("_commSdk", CommodityClientSdk.Object);

            var GetProfile_result = dTPProxyPlugin.GetProfile(Guid1).Result;
            Assert.IsNotNull(GetProfile_result);
        }

        [Test]
        public void TestGetBrightness()
        {
            Guid WebcamGuid = Guid.NewGuid();
            string Guid1 = WebcamGuid.ToString();
            privateteDTPProxyPlugin.SetFieldOrProperty("_webcamMethodInfo", new Mock<MethodInfo>().Object);
            privateteDTPProxyPlugin.SetFieldOrProperty("_commSdk", CommodityClientSdk.Object);

            var GetBrightness_result = dTPProxyPlugin.GetBrightness(Guid1).Result;
            Assert.IsNotNull(GetBrightness_result);
            Assert.That(GetBrightness_result, Is.EqualTo(-1));
        }

        [Test]
        public void TestGetCameraFirmwareVersion()
        {
            Guid WebcamGuid = Guid.NewGuid();
            string Guid1 = WebcamGuid.ToString();
            privateteDTPProxyPlugin.SetFieldOrProperty("_webcamMethodInfo", new Mock<MethodInfo>().Object);
            privateteDTPProxyPlugin.SetFieldOrProperty("_commSdk", CommodityClientSdk.Object);

            var GetCameraFirmwareVersion_result = dTPProxyPlugin.GetCameraFirmwareVersion(Guid1).Result;
            Assert.IsNotNull(GetCameraFirmwareVersion_result);
        }

        [Test]
        public void TestGetIsWindowsHelloCapabilityVerified()
        {
            Guid WebcamGuid = Guid.NewGuid();
            string Guid1 = WebcamGuid.ToString();
            privateteDTPProxyPlugin.SetFieldOrProperty("_webcamMethodInfo", new Mock<MethodInfo>().Object);
            privateteDTPProxyPlugin.SetFieldOrProperty("_commSdk", CommodityClientSdk.Object);

            var GetIsWindowsHelloCapabilityVerified_result = dTPProxyPlugin.GetIsWindowsHelloCapabilityVerified(Guid1).Result;
            Assert.IsNotNull(GetIsWindowsHelloCapabilityVerified_result);
            Assert.That(GetIsWindowsHelloCapabilityVerified_result, Is.EqualTo(false));
        }

        [Test]
        public void TestGetIsAllSupportedResolutionsFound()
        {
            Guid WebcamGuid = Guid.NewGuid();
            string Guid1 = WebcamGuid.ToString();
            privateteDTPProxyPlugin.SetFieldOrProperty("_webcamMethodInfo", new Mock<MethodInfo>().Object);
            privateteDTPProxyPlugin.SetFieldOrProperty("_commSdk", CommodityClientSdk.Object);

            var GetIsAllSupportedResolutionsFound_result = dTPProxyPlugin.GetIsAllSupportedResolutionsFound(Guid1).Result;
            Assert.IsNotNull(GetIsAllSupportedResolutionsFound_result);
            Assert.That(GetIsAllSupportedResolutionsFound_result, Is.EqualTo(false));
        }

        [Test]
        public void TestGetIsPropertyFOVSupported()
        {
            Guid WebcamGuid = Guid.NewGuid();
            string Guid1 = WebcamGuid.ToString();
            privateteDTPProxyPlugin.SetFieldOrProperty("_webcamMethodInfo", new Mock<MethodInfo>().Object);
            privateteDTPProxyPlugin.SetFieldOrProperty("_commSdk", CommodityClientSdk.Object);

            var GetIsPropertyFOVSupported_result = dTPProxyPlugin.GetIsPropertyFOVSupported(Guid1).Result;
            Assert.IsNotNull(GetIsPropertyFOVSupported_result);
            Assert.That(GetIsPropertyFOVSupported_result, Is.EqualTo(false));
        }

        [Test]
        public void TestGetFieldOfView()
        {
            Guid WebcamGuid = Guid.NewGuid();
            string Guid1 = WebcamGuid.ToString();
            privateteDTPProxyPlugin.SetFieldOrProperty("_webcamMethodInfo", new Mock<MethodInfo>().Object);
            privateteDTPProxyPlugin.SetFieldOrProperty("_commSdk", CommodityClientSdk.Object);

            var GetFieldOfView_result = dTPProxyPlugin.GetFieldOfView(Guid1).Result;
            Assert.IsNotNull(GetFieldOfView_result);
            Assert.That(GetFieldOfView_result, Is.EqualTo(-1));
        }

        [Test]
        public void TestGetIsPropertyHDRSupported()
        {
            Guid WebcamGuid = Guid.NewGuid();
            string Guid1 = WebcamGuid.ToString();
            privateteDTPProxyPlugin.SetFieldOrProperty("_webcamMethodInfo", new Mock<MethodInfo>().Object);
            privateteDTPProxyPlugin.SetFieldOrProperty("_commSdk", CommodityClientSdk.Object);

            var GetIsPropertyHDRSupported_result = dTPProxyPlugin.GetIsPropertyHDRSupported(Guid1).Result;
            Assert.IsNotNull(GetIsPropertyHDRSupported_result);
            Assert.That(GetIsPropertyHDRSupported_result, Is.EqualTo(false));
        }

        [Test]
        public void TestGetIsHDROnd()
        {
            Guid WebcamGuid = Guid.NewGuid();
            string Guid1 = WebcamGuid.ToString();
            privateteDTPProxyPlugin.SetFieldOrProperty("_webcamMethodInfo", new Mock<MethodInfo>().Object);
            privateteDTPProxyPlugin.SetFieldOrProperty("_commSdk", CommodityClientSdk.Object);

            var GetIsHDROn_result = dTPProxyPlugin.GetIsHDROn(Guid1).Result;
            Assert.IsNotNull(GetIsHDROn_result);
            Assert.That(GetIsHDROn_result, Is.EqualTo(false));
        }

        [Test]
        public void TestGeIsPropertyAntiFlickerSupported()
        {
            Guid WebcamGuid = Guid.NewGuid();
            string Guid1 = WebcamGuid.ToString();
            privateteDTPProxyPlugin.SetFieldOrProperty("_webcamMethodInfo", new Mock<MethodInfo>().Object);
            privateteDTPProxyPlugin.SetFieldOrProperty("_commSdk", CommodityClientSdk.Object);

            var GeIsPropertyAntiFlickerSupported_result = dTPProxyPlugin.GeIsPropertyAntiFlickerSupported(Guid1).Result;
            Assert.IsNotNull(GeIsPropertyAntiFlickerSupported_result);
            Assert.That(GeIsPropertyAntiFlickerSupported_result, Is.EqualTo(false));
        }

        public void TestGetAntiFlicker()
        {
            Guid WebcamGuid = Guid.NewGuid();
            string Guid1 = WebcamGuid.ToString();
            privateteDTPProxyPlugin.SetFieldOrProperty("_webcamMethodInfo", new Mock<MethodInfo>().Object);
            privateteDTPProxyPlugin.SetFieldOrProperty("_commSdk", CommodityClientSdk.Object);

            var GetAntiFlicker_result = dTPProxyPlugin.GetAntiFlicker(Guid1).Result;
            Assert.IsNotNull(GetAntiFlicker_result);
            Assert.That(GetAntiFlicker_result, Is.EqualTo(-1));
        }

        [Test]
        public void TestGetIsPropertyAutoFramingSupported()
        {
            Guid WebcamGuid = Guid.NewGuid();
            string Guid1 = WebcamGuid.ToString();
            privateteDTPProxyPlugin.SetFieldOrProperty("_webcamMethodInfo", new Mock<MethodInfo>().Object);
            privateteDTPProxyPlugin.SetFieldOrProperty("_commSdk", CommodityClientSdk.Object);

            var GetIsPropertyAutoFramingSupported_result = dTPProxyPlugin.GetIsPropertyAutoFramingSupported(Guid1).Result;
            Assert.IsNotNull(GetIsPropertyAutoFramingSupported_result);
            Assert.That(GetIsPropertyAutoFramingSupported_result, Is.EqualTo(false));
        }

        [Test]
        public void TestGetIsESISupported()
        {
            Guid WebcamGuid = Guid.NewGuid();
            string Guid1 = WebcamGuid.ToString();
            privateteDTPProxyPlugin.SetFieldOrProperty("_webcamMethodInfo", new Mock<MethodInfo>().Object);
            privateteDTPProxyPlugin.SetFieldOrProperty("_commSdk", CommodityClientSdk.Object);

            var GetIsESISupported_result = dTPProxyPlugin.GetIsESISupported(Guid1).Result;
            Assert.IsNotNull(GetIsESISupported_result);
            Assert.That(GetIsESISupported_result, Is.EqualTo(false));
        }

        [Test]
        public void TestGetIsAutoFramingOn()
        {
            Guid WebcamGuid = Guid.NewGuid();
            string Guid1 = WebcamGuid.ToString();
            privateteDTPProxyPlugin.SetFieldOrProperty("_webcamMethodInfo", new Mock<MethodInfo>().Object);
            privateteDTPProxyPlugin.SetFieldOrProperty("_commSdk", CommodityClientSdk.Object);

            var GetIsAutoFramingOn_result = dTPProxyPlugin.GetIsAutoFramingOn(Guid1).Result;
            Assert.IsNotNull(GetIsAutoFramingOn_result);
            Assert.That(GetIsAutoFramingOn_result, Is.EqualTo(false));
        }

        [Test]
        public void TestGetSupportedResolutions()
        {
            Guid WebcamGuid = Guid.NewGuid();
            string Guid1 = WebcamGuid.ToString();
            privateteDTPProxyPlugin.SetFieldOrProperty("_webcamMethodInfo", new Mock<MethodInfo>().Object);
            privateteDTPProxyPlugin.SetFieldOrProperty("_commSdk", CommodityClientSdk.Object);

            var GetSupportedResolutions_result = dTPProxyPlugin.GetSupportedResolutions(Guid1).Result;
            Assert.IsNotNull(GetSupportedResolutions_result);
        }

        [Test]
        public void TestGetSelectedResolution()
        {
            Guid WebcamGuid = Guid.NewGuid();
            string Guid1 = WebcamGuid.ToString();
            privateteDTPProxyPlugin.SetFieldOrProperty("_webcamMethodInfo", new Mock<MethodInfo>().Object);
            privateteDTPProxyPlugin.SetFieldOrProperty("_commSdk", CommodityClientSdk.Object);

            var GetSelectedResolution_result = dTPProxyPlugin.GetSelectedResolution(Guid1).Result;
            Assert.IsNotNull(GetSelectedResolution_result);
        }

        [Test]
        public void TestGetZoom()
        {
            Guid WebcamGuid = Guid.NewGuid();
            string Guid1 = WebcamGuid.ToString();
            privateteDTPProxyPlugin.SetFieldOrProperty("_webcamMethodInfo", new Mock<MethodInfo>().Object);
            privateteDTPProxyPlugin.SetFieldOrProperty("_commSdk", CommodityClientSdk.Object);

            var GetZoom_result = dTPProxyPlugin.GetZoom(Guid1).Result;
            Assert.IsNotNull(GetZoom_result);
            Assert.That(GetZoom_result, Is.EqualTo(-1));
        }

        [Test]
        public void TestGetFocus()
        {
            Guid WebcamGuid = Guid.NewGuid();
            string Guid1 = WebcamGuid.ToString();
            privateteDTPProxyPlugin.SetFieldOrProperty("_webcamMethodInfo", new Mock<MethodInfo>().Object);
            privateteDTPProxyPlugin.SetFieldOrProperty("_commSdk", CommodityClientSdk.Object);

            var GetFocus_result = dTPProxyPlugin.GetFocus(Guid1).Result;
            Assert.IsNotNull(GetFocus_result);
            Assert.That(GetFocus_result, Is.EqualTo(-1));
        }

        [Test]
        public void TestGetIsFocusOn()
        {
            Guid WebcamGuid = Guid.NewGuid();
            string Guid1 = WebcamGuid.ToString();
            privateteDTPProxyPlugin.SetFieldOrProperty("_webcamMethodInfo", new Mock<MethodInfo>().Object);
            privateteDTPProxyPlugin.SetFieldOrProperty("_commSdk", CommodityClientSdk.Object);

            var GetIsFocusOn_result = dTPProxyPlugin.GetIsFocusOn(Guid1).Result;
            Assert.IsNull(GetIsFocusOn_result);
        }

        [Test]
        public void TestGetPriority()
        {
            Guid WebcamGuid = Guid.NewGuid();
            string Guid1 = WebcamGuid.ToString();
            privateteDTPProxyPlugin.SetFieldOrProperty("_webcamMethodInfo", new Mock<MethodInfo>().Object);
            privateteDTPProxyPlugin.SetFieldOrProperty("_commSdk", CommodityClientSdk.Object);

            var GetPriority_result = dTPProxyPlugin.GetPriority(Guid1).Result;
            Assert.IsNotNull(GetPriority_result);
            Assert.That(GetPriority_result, Is.EqualTo(-1));
        }

        [Test]
        public void TestGetIsAutoFramingTransitionOn()
        {
            Guid WebcamGuid = Guid.NewGuid();
            string Guid1 = WebcamGuid.ToString();
            privateteDTPProxyPlugin.SetFieldOrProperty("_webcamMethodInfo", new Mock<MethodInfo>().Object);
            privateteDTPProxyPlugin.SetFieldOrProperty("_commSdk", CommodityClientSdk.Object);

            var GetIsAutoFramingTransitionOn_result = dTPProxyPlugin.GetIsAutoFramingTransitionOn(Guid1).Result;
            Assert.IsNull(GetIsAutoFramingTransitionOn_result);
        }

        [Test]
        public void TestGetAutoFramingFrameSize()
        {
            Guid WebcamGuid = Guid.NewGuid();
            string Guid1 = WebcamGuid.ToString();
            privateteDTPProxyPlugin.SetFieldOrProperty("_webcamMethodInfo", new Mock<MethodInfo>().Object);
            privateteDTPProxyPlugin.SetFieldOrProperty("_commSdk", CommodityClientSdk.Object);

            var GetAutoFramingFrameSize_result = dTPProxyPlugin.GetAutoFramingFrameSize(Guid1).Result;
            Assert.IsNotNull(GetAutoFramingFrameSize_result);
            Assert.That(GetAutoFramingFrameSize_result, Is.EqualTo(-1));
        }

        [Test]
        public void TestGetAutoFramingSensitivity()
        {
            Guid WebcamGuid = Guid.NewGuid();
            string Guid1 = WebcamGuid.ToString();
            privateteDTPProxyPlugin.SetFieldOrProperty("_webcamMethodInfo", new Mock<MethodInfo>().Object);
            privateteDTPProxyPlugin.SetFieldOrProperty("_commSdk", CommodityClientSdk.Object);

            var GetAutoFramingSensitivity_result = dTPProxyPlugin.GetAutoFramingSensitivity(Guid1).Result;
            Assert.IsNotNull(GetAutoFramingSensitivity_result);
            Assert.That(GetAutoFramingSensitivity_result, Is.EqualTo(-1));
        }

        [Test]
        public void TestSetProfile()
        {
            string newValue = "1";
            Guid WebcamGuid = Guid.NewGuid();
            string Guid1 = WebcamGuid.ToString();
            privateteDTPProxyPlugin.SetFieldOrProperty("_webcamMethodInfo", new Mock<MethodInfo>().Object);
            privateteDTPProxyPlugin.SetFieldOrProperty("_commSdk", CommodityClientSdk.Object);

            var SetProfile_result = dTPProxyPlugin.SetProfile(Guid1, newValue);
            Assert.IsNotNull(SetProfile_result);
        }

        [Test]
        public void TestSetProfileName()
        {
            string newValue = "2";
            Guid WebcamGuid = Guid.NewGuid();
            string Guid1 = WebcamGuid.ToString();
            privateteDTPProxyPlugin.SetFieldOrProperty("_webcamMethodInfo", new Mock<MethodInfo>().Object);
            privateteDTPProxyPlugin.SetFieldOrProperty("_commSdk", CommodityClientSdk.Object);

            var SetProfileName_result = dTPProxyPlugin.SetProfileName(Guid1, newValue);
            Assert.IsNotNull(SetProfileName_result);
        }

        [Test]
        public void TestCreateCustomProfile()
        {
            string newValue = "1";
            Guid WebcamGuid = Guid.NewGuid();
            string Guid1 = WebcamGuid.ToString();
            privateteDTPProxyPlugin.SetFieldOrProperty("_webcamMethodInfo", new Mock<MethodInfo>().Object);
            privateteDTPProxyPlugin.SetFieldOrProperty("_commSdk", CommodityClientSdk.Object);

            var CreateCustomProfile_result = dTPProxyPlugin.CreateCustomProfile(Guid1, newValue);
            Assert.IsNotNull(CreateCustomProfile_result);
        }

        [Test]
        public void TestDeleteProfile()
        {
            string newValue = "1";
            Guid WebcamGuid = Guid.NewGuid();
            string Guid1 = WebcamGuid.ToString();
            privateteDTPProxyPlugin.SetFieldOrProperty("_webcamMethodInfo", new Mock<MethodInfo>().Object);
            privateteDTPProxyPlugin.SetFieldOrProperty("_commSdk", CommodityClientSdk.Object);

            var DeleteProfile_result = dTPProxyPlugin.DeleteProfile(Guid1, newValue);
            Assert.IsNotNull(DeleteProfile_result);
        }

        [Test]
        public void TestSetZoom()
        {
            int newValue = 10;
            Guid WebcamGuid = Guid.NewGuid();
            string Guid1 = WebcamGuid.ToString();
            privateteDTPProxyPlugin.SetFieldOrProperty("_webcamMethodInfo", new Mock<MethodInfo>().Object);
            privateteDTPProxyPlugin.SetFieldOrProperty("_commSdk", CommodityClientSdk.Object);

            var SetZoom_result = dTPProxyPlugin.SetZoom(Guid1, newValue).Result;
            Assert.IsNotNull(SetZoom_result);
            Assert.That(SetZoom_result, Is.EqualTo(false));
        }

        [Test]
        public void TestSetAutoFramingSensitivity()
        {
            int newValue = 10;
            Guid WebcamGuid = Guid.NewGuid();
            string Guid1 = WebcamGuid.ToString();
            privateteDTPProxyPlugin.SetFieldOrProperty("_webcamMethodInfo", new Mock<MethodInfo>().Object);
            privateteDTPProxyPlugin.SetFieldOrProperty("_commSdk", CommodityClientSdk.Object);

            var SetAutoFramingSensitivity_result = dTPProxyPlugin.SetAutoFramingSensitivity(Guid1, newValue).Result;
            Assert.IsNotNull(SetAutoFramingSensitivity_result);
            Assert.That(SetAutoFramingSensitivity_result, Is.EqualTo(false));
        }

        [Test]
        public void TestSetAutoFramingFrameSize()
        {
            int newValue = 20;
            Guid WebcamGuid = Guid.NewGuid();
            string Guid1 = WebcamGuid.ToString();
            privateteDTPProxyPlugin.SetFieldOrProperty("_webcamMethodInfo", new Mock<MethodInfo>().Object);
            privateteDTPProxyPlugin.SetFieldOrProperty("_commSdk", CommodityClientSdk.Object);

            var SetAutoFramingFrameSize_result = dTPProxyPlugin.SetAutoFramingFrameSize(Guid1, newValue).Result;
            Assert.IsNotNull(SetAutoFramingFrameSize_result);
            Assert.That(SetAutoFramingFrameSize_result, Is.EqualTo(false));
        }

        [Test]
        public void TestSetIsAutoFramingOn()
        {
            bool newValue = false;
            Guid WebcamGuid = Guid.NewGuid();
            string Guid1 = WebcamGuid.ToString();
            privateteDTPProxyPlugin.SetFieldOrProperty("_webcamMethodInfo", new Mock<MethodInfo>().Object);
            privateteDTPProxyPlugin.SetFieldOrProperty("_commSdk", CommodityClientSdk.Object);

            var SetIsAutoFramingOn_result = dTPProxyPlugin.SetIsAutoFramingOn(Guid1, newValue).Result;
            Assert.IsNotNull(SetIsAutoFramingOn_result);
            Assert.That(SetIsAutoFramingOn_result, Is.EqualTo(false));
        }

        [Test]
        public void TestSetIsAutoFramingTransitionOn()
        {
            bool newValue = false;
            Guid WebcamGuid = Guid.NewGuid();
            string Guid1 = WebcamGuid.ToString();
            privateteDTPProxyPlugin.SetFieldOrProperty("_webcamMethodInfo", new Mock<MethodInfo>().Object);
            privateteDTPProxyPlugin.SetFieldOrProperty("_commSdk", CommodityClientSdk.Object);

            var SetIsAutoFramingTransitionOn_result = dTPProxyPlugin.SetIsAutoFramingTransitionOn(Guid1, newValue).Result;
            Assert.IsNotNull(SetIsAutoFramingTransitionOn_result);
            Assert.That(SetIsAutoFramingTransitionOn_result, Is.EqualTo(false));
        }

        [Test]
        public void TestSetFieldOfView()
        {
            int newValue = 1;
            Guid WebcamGuid = Guid.NewGuid();
            string Guid1 = WebcamGuid.ToString();
            privateteDTPProxyPlugin.SetFieldOrProperty("_webcamMethodInfo", new Mock<MethodInfo>().Object);
            privateteDTPProxyPlugin.SetFieldOrProperty("_commSdk", CommodityClientSdk.Object);

            var SetFieldOfView_result = dTPProxyPlugin.SetFieldOfView(Guid1, newValue).Result;
            Assert.IsNotNull(SetFieldOfView_result);
            Assert.That(SetFieldOfView_result, Is.EqualTo(false));
        }

        [Test]
        public void TestSetIsFocusOn()
        {
            bool newValue = false;
            Guid WebcamGuid = Guid.NewGuid();
            string Guid1 = WebcamGuid.ToString();
            privateteDTPProxyPlugin.SetFieldOrProperty("_webcamMethodInfo", new Mock<MethodInfo>().Object);
            privateteDTPProxyPlugin.SetFieldOrProperty("_commSdk", CommodityClientSdk.Object);

            var SetIsFocusOn_result = dTPProxyPlugin.SetIsFocusOn(Guid1, newValue).Result;
            Assert.IsNotNull(SetIsFocusOn_result);
            Assert.That(SetIsFocusOn_result, Is.EqualTo(false));
        }

        [Test]
        public void TestSetFocus()
        {
            int newValue = 2;
            Guid WebcamGuid = Guid.NewGuid();
            string Guid1 = WebcamGuid.ToString();
            privateteDTPProxyPlugin.SetFieldOrProperty("_webcamMethodInfo", new Mock<MethodInfo>().Object);
            privateteDTPProxyPlugin.SetFieldOrProperty("_commSdk", CommodityClientSdk.Object);

            var SetFocus_result = dTPProxyPlugin.SetFocus(Guid1, newValue).Result;
            Assert.IsNotNull(SetFocus_result);
            Assert.That(SetFocus_result, Is.EqualTo(false));
        }

        [Test]
        public void TestSetPriority()
        {
            int newValue = 2;
            Guid WebcamGuid = Guid.NewGuid();
            string Guid1 = WebcamGuid.ToString();
            privateteDTPProxyPlugin.SetFieldOrProperty("_webcamMethodInfo", new Mock<MethodInfo>().Object);
            privateteDTPProxyPlugin.SetFieldOrProperty("_commSdk", CommodityClientSdk.Object);

            var SetPriority_result = dTPProxyPlugin.SetPriority(Guid1, newValue).Result;
            Assert.IsNotNull(SetPriority_result);
            Assert.That(SetPriority_result, Is.EqualTo(false));
        }

        [Test]
        public void TestSetIsHDROn()
        {
            bool newValue = false;
            Guid WebcamGuid = Guid.NewGuid();
            string Guid1 = WebcamGuid.ToString();
            privateteDTPProxyPlugin.SetFieldOrProperty("_webcamMethodInfo", new Mock<MethodInfo>().Object);
            privateteDTPProxyPlugin.SetFieldOrProperty("_commSdk", CommodityClientSdk.Object);

            var SetIsHDROn_result = dTPProxyPlugin.SetIsHDROn(Guid1, newValue).Result;
            Assert.IsNotNull(SetIsHDROn_result);
            Assert.That(SetIsHDROn_result, Is.EqualTo(false));
        }

        [Test]
        public void TestSetIsAutoWhiteBalanceOn()
        {
            bool newValue = false;
            Guid WebcamGuid = Guid.NewGuid();
            string Guid1 = WebcamGuid.ToString();
            privateteDTPProxyPlugin.SetFieldOrProperty("_webcamMethodInfo", new Mock<MethodInfo>().Object);
            privateteDTPProxyPlugin.SetFieldOrProperty("_commSdk", CommodityClientSdk.Object);

            var SetIsAutoWhiteBalanceOn_result = dTPProxyPlugin.SetIsAutoWhiteBalanceOn(Guid1, newValue).Result;
            Assert.IsNotNull(SetIsAutoWhiteBalanceOn_result);
            Assert.That(SetIsAutoWhiteBalanceOn_result, Is.EqualTo(false));
        }

        [Test]
        public void TestSetAutoWhiteBalance()
        {
            int newValue = 2;
            Guid WebcamGuid = Guid.NewGuid();
            string Guid1 = WebcamGuid.ToString();
            privateteDTPProxyPlugin.SetFieldOrProperty("_webcamMethodInfo", new Mock<MethodInfo>().Object);
            privateteDTPProxyPlugin.SetFieldOrProperty("_commSdk", CommodityClientSdk.Object);

            var SetAutoWhiteBalance_result = dTPProxyPlugin.SetAutoWhiteBalance(Guid1, newValue).Result;
            Assert.IsNotNull(SetAutoWhiteBalance_result);
            Assert.That(SetAutoWhiteBalance_result, Is.EqualTo(false));
        }

        [Test]
        public void TestSetBrightness()
        {
            int newValue = 2;
            Guid WebcamGuid = Guid.NewGuid();
            string Guid1 = WebcamGuid.ToString();
            privateteDTPProxyPlugin.SetFieldOrProperty("_webcamMethodInfo", new Mock<MethodInfo>().Object);
            privateteDTPProxyPlugin.SetFieldOrProperty("_commSdk", CommodityClientSdk.Object);

            var SetBrightness_result = dTPProxyPlugin.SetBrightness(Guid1, newValue);
            Assert.IsNotNull(SetBrightness_result);
        }

        [Test]
        public void TestSetSharpness()
        {
            int newValue = 2;
            Guid WebcamGuid = Guid.NewGuid();
            string Guid1 = WebcamGuid.ToString();
            privateteDTPProxyPlugin.SetFieldOrProperty("_webcamMethodInfo", new Mock<MethodInfo>().Object);
            privateteDTPProxyPlugin.SetFieldOrProperty("_commSdk", CommodityClientSdk.Object);

            var SetSharpness_result = dTPProxyPlugin.SetSharpness(Guid1, newValue);
            Assert.IsNotNull(SetSharpness_result);
        }

        [Test]
        public void TestSetContrast()
        {
            int newValue = 1;
            Guid WebcamGuid = Guid.NewGuid();
            string Guid1 = WebcamGuid.ToString();
            privateteDTPProxyPlugin.SetFieldOrProperty("_webcamMethodInfo", new Mock<MethodInfo>().Object);
            privateteDTPProxyPlugin.SetFieldOrProperty("_commSdk", CommodityClientSdk.Object);

            var SetContrast_result = dTPProxyPlugin.SetContrast(Guid1, newValue);
            Assert.IsNotNull(SetContrast_result);
        }

        [Test]
        public void TestSetSaturation()
        {
            int newValue = 1;
            Guid WebcamGuid = Guid.NewGuid();
            string Guid1 = WebcamGuid.ToString();
            privateteDTPProxyPlugin.SetFieldOrProperty("_webcamMethodInfo", new Mock<MethodInfo>().Object);
            privateteDTPProxyPlugin.SetFieldOrProperty("_commSdk", CommodityClientSdk.Object);

            var SetSaturation_result = dTPProxyPlugin.SetSaturation(Guid1, newValue);
            Assert.IsNotNull(SetSaturation_result);
        }

        [Test]
        public void TestSetAntiFlicker()
        {
            int newValue = 1;
            Guid WebcamGuid = Guid.NewGuid();
            string Guid1 = WebcamGuid.ToString();
            privateteDTPProxyPlugin.SetFieldOrProperty("_webcamMethodInfo", new Mock<MethodInfo>().Object);
            privateteDTPProxyPlugin.SetFieldOrProperty("_commSdk", CommodityClientSdk.Object);

            var SetAntiFlicker_result = dTPProxyPlugin.SetAntiFlicker(Guid1, newValue);
            Assert.IsNotNull(SetAntiFlicker_result);
        }

        [Test]
        public void TestSetTilt()
        {
            int newValue = 2;
            Guid WebcamGuid = Guid.NewGuid();
            string Guid1 = WebcamGuid.ToString();
            privateteDTPProxyPlugin.SetFieldOrProperty("_webcamMethodInfo", new Mock<MethodInfo>().Object);
            privateteDTPProxyPlugin.SetFieldOrProperty("_commSdk", CommodityClientSdk.Object);

            var SetTilt_result = dTPProxyPlugin.SetTilt(Guid1, newValue);
            Assert.IsNotNull(SetTilt_result);
        }

        [Test]
        public void TestSetPan()
        {
            int newValue = 2;
            Guid WebcamGuid = Guid.NewGuid();
            string Guid1 = WebcamGuid.ToString();
            privateteDTPProxyPlugin.SetFieldOrProperty("_webcamMethodInfo", new Mock<MethodInfo>().Object);
            privateteDTPProxyPlugin.SetFieldOrProperty("_commSdk", CommodityClientSdk.Object);

            var SetPan_result = dTPProxyPlugin.SetPan(Guid1, newValue);
            Assert.IsNotNull(SetPan_result);
        }

        [Test]
        public void TestSetIsMicEnumerationOn()
        {
            bool newValue = false;
            Guid WebcamGuid = Guid.NewGuid();
            string Guid1 = WebcamGuid.ToString();
            privateteDTPProxyPlugin.SetFieldOrProperty("_webcamMethodInfo", new Mock<MethodInfo>().Object);
            privateteDTPProxyPlugin.SetFieldOrProperty("_commSdk", CommodityClientSdk.Object);

            var SetIsMicEnumerationOn_result = dTPProxyPlugin.SetIsMicEnumerationOn(Guid1, newValue);
            Assert.IsNotNull(SetIsMicEnumerationOn_result);
        }

        [Test]
        public void TestSetWALTime()
        {
            int newValue = 1;
            Guid WebcamGuid = Guid.NewGuid();
            string Guid1 = WebcamGuid.ToString();
            privateteDTPProxyPlugin.SetFieldOrProperty("_webcamMethodInfo", new Mock<MethodInfo>().Object);
            privateteDTPProxyPlugin.SetFieldOrProperty("_commSdk", CommodityClientSdk.Object);

            var SetWALTime_result = dTPProxyPlugin.SetWALTime(Guid1, newValue);
            Assert.IsNotNull(SetWALTime_result);
        }

        [Test]
        public void TestSetSnooze()
        {
            int newValue = 2;
            Guid WebcamGuid = Guid.NewGuid();
            string Guid1 = WebcamGuid.ToString();
            privateteDTPProxyPlugin.SetFieldOrProperty("_webcamMethodInfo", new Mock<MethodInfo>().Object);
            privateteDTPProxyPlugin.SetFieldOrProperty("_commSdk", CommodityClientSdk.Object);

            var SetSnooze_result = dTPProxyPlugin.SetSnooze(Guid1, newValue);
            Assert.IsNotNull(SetSnooze_result);
        }

        [Test]
        public void TestSetSnoozeLength()
        {
            int newValue = 2;
            Guid WebcamGuid = Guid.NewGuid();
            string Guid1 = WebcamGuid.ToString();
            privateteDTPProxyPlugin.SetFieldOrProperty("_webcamMethodInfo", new Mock<MethodInfo>().Object);
            privateteDTPProxyPlugin.SetFieldOrProperty("_commSdk", CommodityClientSdk.Object);

            var SetSnoozeLength_result = dTPProxyPlugin.SetSnoozeLength(Guid1, newValue);
            Assert.IsNotNull(SetSnoozeLength_result);
        }

        [Test]
        public void TestSetIsProximitySensorEnable()
        {
            bool newValue = false;
            Guid WebcamGuid = Guid.NewGuid();
            string Guid1 = WebcamGuid.ToString();
            privateteDTPProxyPlugin.SetFieldOrProperty("_webcamMethodInfo", new Mock<MethodInfo>().Object);
            privateteDTPProxyPlugin.SetFieldOrProperty("_commSdk", CommodityClientSdk.Object);

            var SetIsProximitySensorEnable_result = dTPProxyPlugin.SetIsProximitySensorEnable(Guid1, newValue);
            Assert.IsNotNull(SetIsProximitySensorEnable_result);
        }

        [Test]
        public void TestSetIsWakeonApproachEnable()
        {
            bool newValue = false;
            Guid WebcamGuid = Guid.NewGuid();
            string Guid1 = WebcamGuid.ToString();
            privateteDTPProxyPlugin.SetFieldOrProperty("_webcamMethodInfo", new Mock<MethodInfo>().Object);
            privateteDTPProxyPlugin.SetFieldOrProperty("_commSdk", CommodityClientSdk.Object);

            var SetIsWakeonApproachEnable_result = dTPProxyPlugin.SetIsWakeonApproachEnable(Guid1, newValue);
            Assert.IsNotNull(SetIsWakeonApproachEnable_result);
        }

        [Test]
        public void TestSetIsWalkAwayLockEnable()
        {
            bool newValue = false;
            Guid WebcamGuid = Guid.NewGuid();
            string Guid1 = WebcamGuid.ToString();
            privateteDTPProxyPlugin.SetFieldOrProperty("_webcamMethodInfo", new Mock<MethodInfo>().Object);
            privateteDTPProxyPlugin.SetFieldOrProperty("_commSdk", CommodityClientSdk.Object);

            var SetIsWalkAwayLockEnable_result = dTPProxyPlugin.SetIsWalkAwayLockEnable(Guid1, newValue);
            Assert.IsNotNull(SetIsWalkAwayLockEnable_result);
        }

        [Test]
        public void TestSetIsPrioritizeExternalWebcam()
        {
            bool newValue = false;
            Guid WebcamGuid = Guid.NewGuid();
            string Guid1 = WebcamGuid.ToString();
            privateteDTPProxyPlugin.SetFieldOrProperty("_webcamMethodInfo", new Mock<MethodInfo>().Object);
            privateteDTPProxyPlugin.SetFieldOrProperty("_commSdk", CommodityClientSdk.Object);

            var SetIsPrioritizeExternalWebcam_result = dTPProxyPlugin.SetIsPrioritizeExternalWebcam(Guid1, newValue);
            Assert.IsNotNull(SetIsPrioritizeExternalWebcam_result);
        }

        [Test]
        public void TestResetToDefault_webcam()
        {
            bool newValue = false;
            Guid WebcamGuid = Guid.NewGuid();
            string Guid1 = WebcamGuid.ToString();
            privateteDTPProxyPlugin.SetFieldOrProperty("_webcamMethodInfo", new Mock<MethodInfo>().Object);
            privateteDTPProxyPlugin.SetFieldOrProperty("_commSdk", CommodityClientSdk.Object);

            var ResetToDefault_webcam_result = dTPProxyPlugin.ResetToDefault_webcam(Guid1, newValue);
            Assert.IsNotNull(ResetToDefault_webcam_result);
        }

        [Test]
        public void TestGetWALTime()
        {
            Guid WebcamGuid = Guid.NewGuid();
            string Guid1 = WebcamGuid.ToString();
            privateteDTPProxyPlugin.SetFieldOrProperty("_webcamMethodInfo", new Mock<MethodInfo>().Object);
            privateteDTPProxyPlugin.SetFieldOrProperty("_commSdk", CommodityClientSdk.Object);

            var GetWALTime_result = dTPProxyPlugin.GetWALTime(Guid1).Result;
            Assert.IsNotNull(GetWALTime_result);
            Assert.That(GetWALTime_result, Is.EqualTo(-1));
        }

        [Test]
        public void TestGetSnooze()
        {
            Guid WebcamGuid = Guid.NewGuid();
            string Guid1 = WebcamGuid.ToString();
            privateteDTPProxyPlugin.SetFieldOrProperty("_webcamMethodInfo", new Mock<MethodInfo>().Object);
            privateteDTPProxyPlugin.SetFieldOrProperty("_commSdk", CommodityClientSdk.Object);

            var GetSnooze_result = dTPProxyPlugin.GetSnooze(Guid1).Result;
            Assert.IsNotNull(GetSnooze_result);
            Assert.That(GetSnooze_result, Is.EqualTo(-1));
        }

        [Test]
        public void TestGetSnoozeLength()
        {
            Guid WebcamGuid = Guid.NewGuid();
            string Guid1 = WebcamGuid.ToString();
            privateteDTPProxyPlugin.SetFieldOrProperty("_webcamMethodInfo", new Mock<MethodInfo>().Object);
            privateteDTPProxyPlugin.SetFieldOrProperty("_commSdk", CommodityClientSdk.Object);

            var GetSnoozeLength_result = dTPProxyPlugin.GetSnoozeLength(Guid1).Result;
            Assert.IsNotNull(GetSnoozeLength_result);
            Assert.That(GetSnoozeLength_result, Is.EqualTo(-1));
        }

        [Test]
        public void TestGetIsProximitySensorEnable()
        {
            Guid WebcamGuid = Guid.NewGuid();
            string Guid1 = WebcamGuid.ToString();
            privateteDTPProxyPlugin.SetFieldOrProperty("_webcamMethodInfo", new Mock<MethodInfo>().Object);
            privateteDTPProxyPlugin.SetFieldOrProperty("_commSdk", CommodityClientSdk.Object);

            var GetIsProximitySensorEnable_result = dTPProxyPlugin.GetIsProximitySensorEnable(Guid1).Result;
            Assert.IsNotNull(GetIsProximitySensorEnable_result);
            Assert.That(GetIsProximitySensorEnable_result, Is.EqualTo(false));
        }

        [Test]
        public void TestGetIsWakeonApproachEnable()
        {
            Guid WebcamGuid = Guid.NewGuid();
            string Guid1 = WebcamGuid.ToString();
            privateteDTPProxyPlugin.SetFieldOrProperty("_webcamMethodInfo", new Mock<MethodInfo>().Object);
            privateteDTPProxyPlugin.SetFieldOrProperty("_commSdk", CommodityClientSdk.Object);

            var GetIsWakeonApproachEnable_result = dTPProxyPlugin.GetIsWakeonApproachEnable(Guid1).Result;
            Assert.IsNotNull(GetIsWakeonApproachEnable_result);
            Assert.That(GetIsWakeonApproachEnable_result, Is.EqualTo(false));
        }

        [Test]
        public void TestGetIsWalkAwayLockEnable()
        {
            Guid WebcamGuid = Guid.NewGuid();
            string Guid1 = WebcamGuid.ToString();
            privateteDTPProxyPlugin.SetFieldOrProperty("_webcamMethodInfo", new Mock<MethodInfo>().Object);
            privateteDTPProxyPlugin.SetFieldOrProperty("_commSdk", CommodityClientSdk.Object);

            var GetIsWalkAwayLockEnable_result = dTPProxyPlugin.GetIsWalkAwayLockEnable(Guid1).Result;
            Assert.IsNotNull(GetIsWalkAwayLockEnable_result);
            Assert.That(GetIsWalkAwayLockEnable_result, Is.EqualTo(false));
        }

        [Test]
        public void TestGetIsPrioritizeExternalWebcam()
        {
            Guid WebcamGuid = Guid.NewGuid();
            string Guid1 = WebcamGuid.ToString();
            privateteDTPProxyPlugin.SetFieldOrProperty("_webcamMethodInfo", new Mock<MethodInfo>().Object);
            privateteDTPProxyPlugin.SetFieldOrProperty("_commSdk", CommodityClientSdk.Object);

            var GetIsPrioritizeExternalWebcam_result = dTPProxyPlugin.GetIsPrioritizeExternalWebcam(Guid1).Result;
            Assert.IsNull(GetIsPrioritizeExternalWebcam_result);
        }

        [Test]
        public void TestGetIsZoomMeetingActive()
        {
            Guid WebcamGuid = Guid.NewGuid();
            string Guid1 = WebcamGuid.ToString();
            privateteDTPProxyPlugin.SetFieldOrProperty("_webcamMethodInfo", new Mock<MethodInfo>().Object);
            privateteDTPProxyPlugin.SetFieldOrProperty("_commSdk", CommodityClientSdk.Object);

            var GetIsZoomMeetingActive_result = dTPProxyPlugin.GetIsZoomMeetingActive(Guid1).Result;
            Assert.IsNotNull(GetIsZoomMeetingActive_result);
            Assert.That(GetIsZoomMeetingActive_result, Is.EqualTo(false));
        }

        [Test]
        public void TestGetZoomMeetingType()
        {
            Guid WebcamGuid = Guid.NewGuid();
            string Guid1 = WebcamGuid.ToString();
            privateteDTPProxyPlugin.SetFieldOrProperty("_webcamMethodInfo", new Mock<MethodInfo>().Object);
            privateteDTPProxyPlugin.SetFieldOrProperty("_commSdk", CommodityClientSdk.Object);

            var GetZoomMeetingTypeAsync_result = dTPProxyPlugin.GetZoomMeetingTypeAsync(Guid1).Result;
            Assert.IsNotNull(GetZoomMeetingTypeAsync_result);
            Assert.That(GetZoomMeetingTypeAsync_result, Is.EqualTo(4));
        }

        [Test]
        public void TestGetIsZoomScreenShareActive()
        {
            Guid WebcamGuid = Guid.NewGuid();
            string Guid1 = WebcamGuid.ToString();
            privateteDTPProxyPlugin.SetFieldOrProperty("_webcamMethodInfo", new Mock<MethodInfo>().Object);
            privateteDTPProxyPlugin.SetFieldOrProperty("_commSdk", CommodityClientSdk.Object);

            var GetIsZoomScreenShareActive_result = dTPProxyPlugin.GetIsZoomScreenShareActive(Guid1).Result;
            Assert.IsNotNull(GetIsZoomScreenShareActive_result);
            Assert.That(GetIsZoomScreenShareActive_result, Is.EqualTo(false));
        }

        [Test]
        public void TestPairingPen()
        {
            Guid PenGuid = Guid.NewGuid();
            string Guid1 = PenGuid.ToString();
            privateteDTPProxyPlugin.SetFieldOrProperty("_penMethodInfo", new Mock<MethodInfo>().Object);
            privateteDTPProxyPlugin.SetFieldOrProperty("_commSdk", CommodityClientSdk.Object);

            var PairingPen_result = dTPProxyPlugin.PairingPen();
            Assert.IsNotNull(PairingPen_result);
        }

        [Test]
        public void TestUnPairPen()
        {
            Guid PenGuid = Guid.NewGuid();
            string Guid1 = PenGuid.ToString();
            privateteDTPProxyPlugin.SetFieldOrProperty("_penMethodInfo", new Mock<MethodInfo>().Object);
            privateteDTPProxyPlugin.SetFieldOrProperty("_commSdk", CommodityClientSdk.Object);

            var UnPairPen_result = dTPProxyPlugin.UnPairPen(Guid1);
            Assert.IsNotNull(UnPairPen_result);
        }

        [Test]
        public void TestGetPenDeviceItemsEx()
        {
            privateteDTPProxyPlugin.SetFieldOrProperty("_penMethodInfo", null);
            var UnPairPen_result = dTPProxyPlugin.GetPenDeviceItemsEx().Result;   //_penMethodInfo null
            Assert.IsNotNull(UnPairPen_result);
            Assert.That(UnPairPen_result, Is.EqualTo(new JArray()));

            privateteDTPProxyPlugin.SetFieldOrProperty("_penMethodInfo", new Mock<MethodInfo>().Object);  //_penMethodInfo not null
            privateteDTPProxyPlugin.SetFieldOrProperty("_commSdk", CommodityClientSdk.Object);
            var UnPairPen_result2 = dTPProxyPlugin.GetPenDeviceItemsEx().Result;
            Assert.IsNotNull(UnPairPen_result2);
            Assert.That(UnPairPen_result, Is.EqualTo(new JArray()));
        }

        [Test]
        public void TestGetEraserDoublePressValues()
        {
            privateteDTPProxyPlugin.SetFieldOrProperty("_penMethodInfo", null);
            var GetEraserDoublePressValues_result = dTPProxyPlugin.GetEraserDoublePressValues().Result;   //_penMethodInfo null
            Assert.IsNotNull(GetEraserDoublePressValues_result);

            privateteDTPProxyPlugin.SetFieldOrProperty("_penMethodInfo", new Mock<MethodInfo>().Object);  //_penMethodInfo not null
            privateteDTPProxyPlugin.SetFieldOrProperty("_commSdk", CommodityClientSdk.Object);
            var GetEraserDoublePressValues_result2 = dTPProxyPlugin.GetEraserDoublePressValues().Result;
            Assert.IsNotNull(GetEraserDoublePressValues_result2);
        }

        [Test]
        public void TestGetEraserSinglePressValues()
        {
            privateteDTPProxyPlugin.SetFieldOrProperty("_penMethodInfo", null);
            var GetEraserSinglePressValues_result = dTPProxyPlugin.GetEraserSinglePressValues().Result;   //_penMethodInfo null
            Assert.IsNotNull(GetEraserSinglePressValues_result);

            privateteDTPProxyPlugin.SetFieldOrProperty("_penMethodInfo", new Mock<MethodInfo>().Object);  //_penMethodInfo not null
            privateteDTPProxyPlugin.SetFieldOrProperty("_commSdk", CommodityClientSdk.Object);
            var GetEraserSinglePressValues_result2 = dTPProxyPlugin.GetEraserSinglePressValues().Result;
            Assert.IsNotNull(GetEraserSinglePressValues_result2);
        }

        [Test]
        public void TestGetEraserLongPressValues()
        {
            privateteDTPProxyPlugin.SetFieldOrProperty("_penMethodInfo", null);
            var GetEraserLongPressValues_result = dTPProxyPlugin.GetEraserLongPressValues().Result;   //_penMethodInfo null
            Assert.IsNotNull(GetEraserLongPressValues_result);

            privateteDTPProxyPlugin.SetFieldOrProperty("_penMethodInfo", new Mock<MethodInfo>().Object);  //_penMethodInfo not null
            privateteDTPProxyPlugin.SetFieldOrProperty("_commSdk", CommodityClientSdk.Object);
            var GetEraserLongPressValues_result2 = dTPProxyPlugin.GetEraserLongPressValues().Result;
            Assert.IsNotNull(GetEraserLongPressValues_result2);
        }

        [Test]
        public void TestGetSideSwitchSinglePressValues()
        {
            privateteDTPProxyPlugin.SetFieldOrProperty("_penMethodInfo", null);
            var GetSideSwitchSinglePressValues_result = dTPProxyPlugin.GetSideSwitchSinglePressValues().Result;   //_penMethodInfo null
            Assert.IsNotNull(GetSideSwitchSinglePressValues_result);

            privateteDTPProxyPlugin.SetFieldOrProperty("_penMethodInfo", new Mock<MethodInfo>().Object);  //_penMethodInfo not null
            privateteDTPProxyPlugin.SetFieldOrProperty("_commSdk", CommodityClientSdk.Object);
            var GetSideSwitchSinglePressValues_result2 = dTPProxyPlugin.GetSideSwitchSinglePressValues().Result;
            Assert.IsNotNull(GetSideSwitchSinglePressValues_result2);
        }

        [Test]
        public void TestGetMenuSinglePressValues()
        {
            privateteDTPProxyPlugin.SetFieldOrProperty("_penMethodInfo", null);
            var GetMenuSinglePressValues_result = dTPProxyPlugin.GetMenuSinglePressValues().Result;   //_penMethodInfo null
            Assert.IsNotNull(GetMenuSinglePressValues_result);

            privateteDTPProxyPlugin.SetFieldOrProperty("_penMethodInfo", new Mock<MethodInfo>().Object);  //_penMethodInfo not null
            privateteDTPProxyPlugin.SetFieldOrProperty("_commSdk", CommodityClientSdk.Object);
            var GetMenuSinglePressValues_result2 = dTPProxyPlugin.GetMenuSinglePressValues().Result;
            Assert.IsNotNull(GetMenuSinglePressValues_result2);
        }

        [Test]
        public void TestGetLaunchableAppValues()
        {
            privateteDTPProxyPlugin.SetFieldOrProperty("_penMethodInfo", null);
            var GetLaunchableAppValues_result = dTPProxyPlugin.GetLaunchableAppValues().Result;   //_penMethodInfo null
            Assert.IsNotNull(GetLaunchableAppValues_result);

            privateteDTPProxyPlugin.SetFieldOrProperty("_penMethodInfo", new Mock<MethodInfo>().Object);  //_penMethodInfo not null
            privateteDTPProxyPlugin.SetFieldOrProperty("_commSdk", CommodityClientSdk.Object);
            var GetLaunchableAppValues_result2 = dTPProxyPlugin.GetLaunchableAppValues().Result;
            Assert.IsNotNull(GetLaunchableAppValues_result2);
        }

        [Test]
        public void TestGetEraserDoublePressSetting()
        {
            privateteDTPProxyPlugin.SetFieldOrProperty("_penMethodInfo", null);
            var GetEraserDoublePressSetting_result = dTPProxyPlugin.GetEraserDoublePressSetting().Result;   //_penMethodInfo null
            Assert.IsNotNull(GetEraserDoublePressSetting_result);

            privateteDTPProxyPlugin.SetFieldOrProperty("_penMethodInfo", new Mock<MethodInfo>().Object);  //_penMethodInfo not null
            privateteDTPProxyPlugin.SetFieldOrProperty("_commSdk", CommodityClientSdk.Object);
            var GetEraserDoublePressSetting_result2 = dTPProxyPlugin.GetEraserDoublePressSetting().Result;
            Assert.IsNotNull(GetEraserDoublePressSetting_result2);
        }

        [Test]
        public void TestGetEraserSinglePressSetting()
        {
            privateteDTPProxyPlugin.SetFieldOrProperty("_penMethodInfo", null);
            var GetEraserSinglePressSetting_result = dTPProxyPlugin.GetEraserSinglePressSetting().Result;   //_penMethodInfo null
            Assert.IsNotNull(GetEraserSinglePressSetting_result);

            privateteDTPProxyPlugin.SetFieldOrProperty("_penMethodInfo", new Mock<MethodInfo>().Object);  //_penMethodInfo not null
            privateteDTPProxyPlugin.SetFieldOrProperty("_commSdk", CommodityClientSdk.Object);
            var GetEraserSinglePressSetting_result2 = dTPProxyPlugin.GetEraserSinglePressSetting().Result;
            Assert.IsNotNull(GetEraserSinglePressSetting_result2);
        }

        [Test]
        public void TestGetEraserLongPressSetting()
        {
            privateteDTPProxyPlugin.SetFieldOrProperty("_penMethodInfo", null);
            var GetEraserLongPressSetting_result = dTPProxyPlugin.GetEraserLongPressSetting().Result;   //_penMethodInfo null
            Assert.IsNotNull(GetEraserLongPressSetting_result);

            privateteDTPProxyPlugin.SetFieldOrProperty("_penMethodInfo", new Mock<MethodInfo>().Object);  //_penMethodInfo not null
            privateteDTPProxyPlugin.SetFieldOrProperty("_commSdk", CommodityClientSdk.Object);
            var GetEraserLongPressSetting_result2 = dTPProxyPlugin.GetEraserLongPressSetting().Result;
            Assert.IsNotNull(GetEraserLongPressSetting_result2);
        }

        [Test]
        public void TestGetSideTopSwitchSinglePressSetting()
        {
            privateteDTPProxyPlugin.SetFieldOrProperty("_penMethodInfo", null);
            var GetSideTopSwitchSinglePressSetting_result = dTPProxyPlugin.GetSideTopSwitchSinglePressSetting().Result;   //_penMethodInfo null
            Assert.IsNotNull(GetSideTopSwitchSinglePressSetting_result);

            privateteDTPProxyPlugin.SetFieldOrProperty("_penMethodInfo", new Mock<MethodInfo>().Object);  //_penMethodInfo not null
            privateteDTPProxyPlugin.SetFieldOrProperty("_commSdk", CommodityClientSdk.Object);
            var GetSideTopSwitchSinglePressSetting_result2 = dTPProxyPlugin.GetSideTopSwitchSinglePressSetting().Result;
            Assert.IsNotNull(GetSideTopSwitchSinglePressSetting_result2);
        }

        [Test]
        public void TestGetSideBottomSwitchSinglePressSetting()
        {
            privateteDTPProxyPlugin.SetFieldOrProperty("_penMethodInfo", null);
            var GetSideBottomSwitchSinglePressSetting_result = dTPProxyPlugin.GetSideBottomSwitchSinglePressSetting().Result;   //_penMethodInfo null
            Assert.IsNotNull(GetSideBottomSwitchSinglePressSetting_result);

            privateteDTPProxyPlugin.SetFieldOrProperty("_penMethodInfo", new Mock<MethodInfo>().Object);  //_penMethodInfo not null
            privateteDTPProxyPlugin.SetFieldOrProperty("_commSdk", CommodityClientSdk.Object);
            var GetSideBottomSwitchSinglePressSetting_result2 = dTPProxyPlugin.GetSideBottomSwitchSinglePressSetting().Result;
            Assert.IsNotNull(GetSideBottomSwitchSinglePressSetting_result2);
        }

        [Test]
        public void TestGetMenuSinglePressSetting()
        {
            privateteDTPProxyPlugin.SetFieldOrProperty("_penMethodInfo", null);
            var GetMenuSinglePressSetting_result = dTPProxyPlugin.GetMenuSinglePressSetting().Result;   //_penMethodInfo null
            Assert.IsNotNull(GetMenuSinglePressSetting_result);

            privateteDTPProxyPlugin.SetFieldOrProperty("_penMethodInfo", new Mock<MethodInfo>().Object);  //_penMethodInfo not null
            privateteDTPProxyPlugin.SetFieldOrProperty("_commSdk", CommodityClientSdk.Object);
            var GetMenuSinglePressSetting_result2 = dTPProxyPlugin.GetMenuSinglePressSetting().Result;
            Assert.IsNotNull(GetMenuSinglePressSetting_result2);
        }

        [Test]
        public void TestGetMenuCenterRightClickSetting()
        {
            privateteDTPProxyPlugin.SetFieldOrProperty("_penMethodInfo", null);
            var GetMenuCenterRightClickSetting_result = dTPProxyPlugin.GetMenuCenterRightClickSetting().Result;   //_penMethodInfo null
            Assert.IsNotNull(GetMenuCenterRightClickSetting_result);
            Assert.That(GetMenuCenterRightClickSetting_result, Is.EqualTo(false));

            privateteDTPProxyPlugin.SetFieldOrProperty("_penMethodInfo", new Mock<MethodInfo>().Object);  //_penMethodInfo not null
            privateteDTPProxyPlugin.SetFieldOrProperty("_commSdk", CommodityClientSdk.Object);
            var GetMenuCenterRightClickSetting_result2 = dTPProxyPlugin.GetMenuCenterRightClickSetting().Result;
            Assert.IsNotNull(GetMenuCenterRightClickSetting_result2);
            Assert.That(GetMenuCenterRightClickSetting_result2, Is.EqualTo(false));
        }

        [Test]
        public void TestGetIsSideTopButtonHoverClick()
        {
            privateteDTPProxyPlugin.SetFieldOrProperty("_penMethodInfo", null);
            var GetIsSideTopButtonHoverClick_result = dTPProxyPlugin.GetIsSideTopButtonHoverClick().Result;   //_penMethodInfo null
            Assert.IsNotNull(GetIsSideTopButtonHoverClick_result);
            Assert.That(GetIsSideTopButtonHoverClick_result, Is.EqualTo(false));

            privateteDTPProxyPlugin.SetFieldOrProperty("_penMethodInfo", new Mock<MethodInfo>().Object);  //_penMethodInfo not null
            privateteDTPProxyPlugin.SetFieldOrProperty("_commSdk", CommodityClientSdk.Object);
            var GetIsSideTopButtonHoverClick_result_result2 = dTPProxyPlugin.GetIsSideTopButtonHoverClick().Result;
            Assert.IsNotNull(GetIsSideTopButtonHoverClick_result);
            Assert.That(GetIsSideTopButtonHoverClick_result, Is.EqualTo(false));
        }

        [Test]
        public void TestGetIsSideBottomButtonHoverClick()
        {
            privateteDTPProxyPlugin.SetFieldOrProperty("_penMethodInfo", null);
            var GetIsSideBottomButtonHoverClick_result = dTPProxyPlugin.GetIsSideBottomButtonHoverClick().Result;   //_penMethodInfo null
            Assert.IsNotNull(GetIsSideBottomButtonHoverClick_result);
            Assert.That(GetIsSideBottomButtonHoverClick_result, Is.EqualTo(false));

            privateteDTPProxyPlugin.SetFieldOrProperty("_penMethodInfo", new Mock<MethodInfo>().Object);  //_penMethodInfo not null
            privateteDTPProxyPlugin.SetFieldOrProperty("_commSdk", CommodityClientSdk.Object);
            var GetIsSideBottomButtonHoverClick_result2 = dTPProxyPlugin.GetIsSideBottomButtonHoverClick().Result;
            Assert.IsNotNull(GetIsSideBottomButtonHoverClick_result2);
            Assert.That(GetIsSideBottomButtonHoverClick_result2, Is.EqualTo(false));
        }

        [Test]
        public void TestStartKeyCapturePen()
        {
            privateteDTPProxyPlugin.SetFieldOrProperty("_penMethodInfo", null);
            var StartKeyCapturePen_result = dTPProxyPlugin.StartKeyCapturePen().Result;   //_penMethodInfo null
            Assert.IsNotNull(StartKeyCapturePen_result);
            Assert.That(StartKeyCapturePen_result, Is.EqualTo(false));

            privateteDTPProxyPlugin.SetFieldOrProperty("_penMethodInfo", new Mock<MethodInfo>().Object);  //_penMethodInfo not null
            privateteDTPProxyPlugin.SetFieldOrProperty("_commSdk", CommodityClientSdk.Object);
            var StartKeyCapturePen_result2 = dTPProxyPlugin.StartKeyCapturePen().Result;
            Assert.IsNotNull(StartKeyCapturePen_result2);
            Assert.That(StartKeyCapturePen_result2, Is.EqualTo(false));
        }

        [Test]
        public void TestFinishKeyCapturePen()
        {
            privateteDTPProxyPlugin.SetFieldOrProperty("_penMethodInfo", null);
            var FinishKeyCapturePen_result = dTPProxyPlugin.FinishKeyCapturePen().Result;   //_penMethodInfo null
            Assert.IsNotNull(FinishKeyCapturePen_result);
            Assert.That(FinishKeyCapturePen_result, Is.EqualTo(false));

            privateteDTPProxyPlugin.SetFieldOrProperty("_penMethodInfo", new Mock<MethodInfo>().Object);  //_penMethodInfo not null
            privateteDTPProxyPlugin.SetFieldOrProperty("_commSdk", CommodityClientSdk.Object);
            var FinishKeyCapturePen_result2 = dTPProxyPlugin.FinishKeyCapturePen().Result;
            Assert.IsNotNull(FinishKeyCapturePen_result2);
            Assert.That(FinishKeyCapturePen_result2, Is.EqualTo(false));
        }

        [Test]
        public void TestKeyCaptureData()
        {
            privateteDTPProxyPlugin.SetFieldOrProperty("_penMethodInfo", null);
            var KeyCaptureData_result = dTPProxyPlugin.KeyCaptureData().Result;   //_penMethodInfo null
            Assert.IsNotNull(KeyCaptureData_result);

            privateteDTPProxyPlugin.SetFieldOrProperty("_penMethodInfo", new Mock<MethodInfo>().Object);  //_penMethodInfo not null
            privateteDTPProxyPlugin.SetFieldOrProperty("_commSdk", CommodityClientSdk.Object);
            var KeyCaptureData_result2 = dTPProxyPlugin.KeyCaptureData().Result;
            Assert.IsNotNull(KeyCaptureData_result2);
        }

        [Test]
        public void TestSetEraserDoublePressSetting()
        {
            string itemID = "TestPenItemID";
            byte[] newValue = new byte[] { 1, 2 };

            privateteDTPProxyPlugin.SetFieldOrProperty("_penMethodInfo", null);
            var SetEraserDoublePressSetting_result = dTPProxyPlugin.SetEraserDoublePressSetting(itemID, newValue);   //_penMethodInfo null
            Assert.IsNotNull(SetEraserDoublePressSetting_result);
            var ItemID = privateteDTPProxyPlugin.GetFieldOrProperty("_itemID");
            Assert.IsNotNull(itemID);
            Assert.IsTrue(true);

            privateteDTPProxyPlugin.SetFieldOrProperty("_penMethodInfo", new Mock<MethodInfo>().Object);  //_penMethodInfo not null
            privateteDTPProxyPlugin.SetFieldOrProperty("_commSdk", CommodityClientSdk.Object);
            var SetEraserDoublePressSetting_result2 = dTPProxyPlugin.SetEraserDoublePressSetting(itemID, newValue);
            Assert.IsNotNull(SetEraserDoublePressSetting_result2);
            var ItemID2 = privateteDTPProxyPlugin.GetFieldOrProperty("_itemID");
            Assert.IsNotNull(ItemID2);
            Assert.IsTrue(true);
        }

        [Test]
        public void TestSetEraserLongPressSetting()
        {
            string itemID = "TestPenItemID";
            byte[] newValue = new byte[] { 1, 2 };

            privateteDTPProxyPlugin.SetFieldOrProperty("_penMethodInfo", null);
            var SetEraserLongPressSetting_result = dTPProxyPlugin.SetEraserLongPressSetting(itemID, newValue);   //_penMethodInfo null
            Assert.IsNotNull(SetEraserLongPressSetting_result);
            var ItemID = privateteDTPProxyPlugin.GetFieldOrProperty("_itemID");
            Assert.IsNotNull(itemID);
            Assert.IsTrue(true);

            privateteDTPProxyPlugin.SetFieldOrProperty("_penMethodInfo", new Mock<MethodInfo>().Object);  //_penMethodInfo not null
            privateteDTPProxyPlugin.SetFieldOrProperty("_commSdk", CommodityClientSdk.Object);
            var SetEraserLongPressSetting_result2 = dTPProxyPlugin.SetEraserLongPressSetting(itemID, newValue);
            Assert.IsNotNull(SetEraserLongPressSetting_result2);
            var ItemID2 = privateteDTPProxyPlugin.GetFieldOrProperty("_itemID");
            Assert.IsNotNull(ItemID2);
            Assert.IsTrue(true);
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            dTPProxyPlugin.Dispose();
        }
    }
}