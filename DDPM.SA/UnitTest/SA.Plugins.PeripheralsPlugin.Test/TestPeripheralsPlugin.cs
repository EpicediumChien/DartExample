using DDPM.SA.Common;
using DDPM.SA.Plugins.PeripheralsPlugin;
using DDPM.SA.Plugins.User.DisplayManager;
using Dell.Client.Framework.Interfaces;
using Dell.Client.Framework.UnitTestShared.Tests;
using IndiLogic.DPeM.Broker;
using Moq;
using VcpCore.Common;
using VcpCore.Interfaces;
using VcpCore.Plugins;
using static DDPM.RemoteManagement.Common.Interfaces.Params;
using IDs = DDPM.SA.Common.IDs;
using DPeMPublic.Common.Enums;
using Microsoft.Windows.Themes;
using static Microsoft.WindowsAPICodePack.Shell.PropertySystem.SystemProperties.System;

namespace DDPM.SA.Plugins.PeripheralsPlugin.Test
{
    public class TestPeripheralsPlugin
    {
        private Mock<IAgent> DisplayMangerAgent { get; } = new();
        private Mock<IAgent> VcpCoreAgent { get; } = new();
        private Mock<IAgent> PeripheralsPluginAgent { get; } = new();

        MonitorInfo monitorInfo = new MonitorInfo();
        private Mock<IVcpCoreService> VcpCoreService { get; } = new();
        private Mock<IDisplayProperties> DisplayPropertiesService { get; } = new();
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



        private DisplayMangerPlugin CreateInitializeDisplayMangerPlugin()
        {
            DisplayMangerAgent.Setup(x => x.PluginManager.FindPluginByGuid(Guid.Parse(IDs.Display_Manager_PLUGIN_ID)));

            return new DisplayMangerPlugin(DisplayMangerAgent.Object);
        }

        private VcpCorePlugin CreateInitializeVcpCorePlugin()
        {
            VcpCoreAgent.Setup(x => x.PluginManager.FindPluginByGuid(Guid.Parse(VcpCore.Common.IDs.VCP_CORE_PLUGIN_ID)));

            return new VcpCorePlugin(VcpCoreAgent.Object);
        }

        private PeripheralsPlugin CreateSchedulerMangerPlugin()
        {
            PeripheralsPluginAgent.Setup(x => x.PluginManager.FindPluginByGuid(Guid.Parse(IDs.DDPM_PERIPHERALS_PLUGIN_ID)));

            return new PeripheralsPlugin(PeripheralsPluginAgent.Object);
        }

        private DisplayMangerPlugin displayPlugin;
        private VcpCorePlugin vcpCorePlugin;
        private PeripheralsPlugin peripheralsPlugin;
        private PrivateObject privatetePeripheralsPlugin;

        [OneTimeSetUp]
        public void Setup()
        {
            displayPlugin = CreateInitializeDisplayMangerPlugin();
            vcpCorePlugin = CreateInitializeVcpCorePlugin();
            peripheralsPlugin = CreateSchedulerMangerPlugin();

            PrivateObject privateObject = new PrivateObject(displayPlugin);
            PrivateObject privatevcp = new PrivateObject(vcpCorePlugin);
            privatetePeripheralsPlugin = new PrivateObject(peripheralsPlugin);
        }

        [Test]
        public void TestPeripheralsPlugins()
        {
            Assert.IsNotNull(peripheralsPlugin);
            PrivateObject privatetePeripheralsPlugin = new PrivateObject(peripheralsPlugin);
            var agent2 = privatetePeripheralsPlugin.GetField("_agent") as IAgent;
            Assert.That(agent2, Is.EqualTo(PeripheralsPluginAgent.Object));
        }

        [Test]
        public void TestUpdateAvailable()
        {
            bool UpdateAvailable1 = true;
            peripheralsPlugin.UpdateAvailable = true;
            var UpdateAvailable_result = peripheralsPlugin.UpdateAvailable;  //UpdateAvailable true
            Assert.That(UpdateAvailable1, Is.EqualTo(UpdateAvailable_result));

            bool UpdateAvailable2 = false;
            peripheralsPlugin.UpdateAvailable = false;
            var UpdateAvailable_result2 = peripheralsPlugin.UpdateAvailable;   //UpdateAvailable false
            Assert.That(UpdateAvailable2, Is.EqualTo(UpdateAvailable_result2));
        }

        [Test]
        public void TestIUpdateManager()
        {
            Mock<IUpdateManager> mockUpdateManager = new Mock<IUpdateManager>();
            var UpdateManagerObj = mockUpdateManager.Object;
            privatetePeripheralsPlugin.SetFieldOrProperty("_iUpdateManager", UpdateManagerObj);
            var UpdateManager_Result = peripheralsPlugin.IUpdateManager;
            Assert.That(UpdateManagerObj, Is.EqualTo(UpdateManager_Result));
        }

        [Test]
        public void TestIOverlayManager()
        {
            Mock<IOverlayManager> mockOverlayManager = new Mock<IOverlayManager>();
            var OverlayManagerObj = mockOverlayManager.Object;
            privatetePeripheralsPlugin.SetFieldOrProperty("_iOverlayManager", OverlayManagerObj);
            var OverlayManager_Result = peripheralsPlugin.IOverlayManager;
            Assert.That(OverlayManagerObj, Is.EqualTo(OverlayManager_Result));
        }


        [OneTimeTearDown]
        public void TearDown()
        {
            displayPlugin.Dispose();
            vcpCorePlugin.Dispose();
            peripheralsPlugin.Dispose();
        }

    }
}