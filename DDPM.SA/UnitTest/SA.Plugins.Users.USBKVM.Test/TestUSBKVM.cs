using DDPM.SA.Common;
using DDPM.SA.Common.Display;
using DDPM.SA.Common.Interfaces;
using DDPM.SA.Plugins.User.DisplayManager;
using DDPM.SA.Plugins.User.DisplayProperties;
using DDPM.SA.Plugins.User.EasyArrange;
using DDPM.SA.Plugins.User.PipPbpManger;
using DDPM.SA.Plugins.User.USBKVM;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.Interfaces;
using Dell.Client.Framework.UnitTestShared.Tests;
using Moq;
using System.Windows.Input;
using VcpCore.Common;
using VcpCore.Interfaces;
using VcpCore.Plugins;
//using WinCopies;
//using WinCopies.Util;


namespace SA.Plugins.User.USBKVM.Test
{
    public class TestUSBKVM
    {

        private Mock<IAgent> DisplayMangerAgent { get; } = new();
        private Mock<IAgent> VcpCoreAgent { get; } = new();
        private Mock<IAgent> USBKVMAgent { get; } = new();

        MonitorInfo monitorInfo = new MonitorInfo();
        private Mock<IVcpCoreService> VcpCoreService { get; } = new();
        private Mock<IDisplayProperties> DisplayPropertiesService { get; } = new();
        private Mock<IDeviceManagerSA> DeviceManagerSAService { get; } = new();
        private IDeviceManagerSA _deviceManagerPlugin;

        MonitorInfo monitorInfo1 = new MonitorInfo()
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
            DisplayMangerAgent.Setup(x => x.PluginManager.FindPluginByGuid(Guid.Parse(DDPM.SA.Common.IDs.Display_Manager_PLUGIN_ID)));

            return new DisplayMangerPlugin(DisplayMangerAgent.Object);
        }

        private VcpCorePlugin CreateInitializeVcpCorePlugin()
        {
            VcpCoreAgent.Setup(x => x.PluginManager.FindPluginByGuid(Guid.Parse(VcpCore.Common.IDs.VCP_CORE_PLUGIN_ID)));

            return new VcpCorePlugin(VcpCoreAgent.Object);
        }

        private USBKVMPlugin CreateInitializeusbKVMPlugin()
        {
            USBKVMAgent.Setup(x => x.PluginManager.FindPluginByGuid(Guid.Parse(DDPM.SA.Common.IDs.DDPM_EAPlugin_PLUGIN_ID)));

            return new USBKVMPlugin(USBKVMAgent.Object);
        }

        DisplayMangerPlugin displayPlugin;
        VcpCorePlugin vcpCorePlugin;
        USBKVMPlugin usbKVMPlugin;

        [OneTimeSetUp]
        public void Setup()
        {
            displayPlugin = CreateInitializeDisplayMangerPlugin();
            vcpCorePlugin = CreateInitializeVcpCorePlugin();
            usbKVMPlugin = CreateInitializeusbKVMPlugin();

            PrivateObject privateObject = new PrivateObject(displayPlugin);
            PrivateObject privatevcp = new PrivateObject(vcpCorePlugin);
            privateObject.SetField("_VcpCorePlugin", vcpCorePlugin as IVcpCoreService);
        }

        [Test]
        public void TestUSBKVMPlugin()
        {
            Assert.IsNotNull(usbKVMPlugin);
            PrivateObject privatepipPbp = new PrivateObject(usbKVMPlugin);
            var agent2 = privatepipPbp.GetField("_agent") as IAgent;
            Assert.That(agent2, Is.EqualTo(USBKVMAgent.Object));
        }

        [Test]
        public void TestGetUSBKVMPCsList()
        {
            var inputlist1 = new Dictionary<string, InputInfo>()
                    {
                { "Thunderbolt-1", new InputInfo { InputName = "Thunderbolt-1", USBUpstream = "Thunderbolt-1" } },
                { "DisplayPort-1", new InputInfo { InputName = "DisplayPort-1", USBUpstream = "USB-B1" } },
                { "HDMI-1", new InputInfo { InputName = "HDMI-1", USBUpstream = "USB-C" } }
                    };
            List<UInt16> SubInputList2 = new List<UInt16>() { 31, 25, 23 };

            var InputName1 = inputlist1["HDMI-1"].InputName;
            var USBUpstream1 = inputlist1["HDMI-1"].USBUpstream;
            var InputName2 = inputlist1["Thunderbolt-1"].InputName;
            var USBUpstream2 = inputlist1["Thunderbolt-1"].USBUpstream;

            MonitorInfo monitorInfo = monitorInfo1;
            PrivateObject privateUSBKVM = new PrivateObject(usbKVMPlugin);
            DeviceManagerSAService.Setup(x => x.GetSubInputList(It.IsAny<MonitorInfo>())).Returns(Task.FromResult(SubInputList2));
            privateUSBKVM.SetFieldOrProperty("_DeviceManagerPlugin", DeviceManagerSAService.Object);

            var GetUSBKVMPCsListResult = usbKVMPlugin.GetUSBKVMPCsList(monitorInfo, inputlist1).Result;

            Assert.Greater(GetUSBKVMPCsListResult.Count,0);
            Assert.IsTrue(GetUSBKVMPCsListResult.ContainsKey("PC1"));
            Assert.IsTrue(GetUSBKVMPCsListResult.ContainsKey("PC2"));
            Assert.That(InputName1, Is.EqualTo(GetUSBKVMPCsListResult["PC1"].InputName));
            Assert.That(USBUpstream1, Is.EqualTo(GetUSBKVMPCsListResult["PC1"].USBUpstream));
            Assert.That(InputName2, Is.EqualTo(GetUSBKVMPCsListResult["PC2"].InputName));
            Assert.That(USBUpstream2, Is.EqualTo(GetUSBKVMPCsListResult["PC2"].USBUpstream));

        }

        [Test]
        public void TestPCInfoSwap()
        {
            Dictionary<string, PCsInfo> pcsList=new Dictionary<string, PCsInfo>() 
            { 
                
                { "PCInfo1", new PCsInfo { InputType="HDMI-1",InputName = "HDMI-1", USBUpstream = "USB-C" } },
                { "PCInfo2", new PCsInfo { InputType="DisplayPort-1", InputName = "DisplayPort-1", USBUpstream = "USB-B1" } },
            };
            Dictionary<string, PCsInfo> pcsList2 = new Dictionary<string, PCsInfo>()
            {

                { "PCInfo1", new PCsInfo { InputType="DisplayPort-1", InputName = "DisplayPort-1", USBUpstream = "USB-B1" } },
                { "PCInfo2", new PCsInfo { InputType="HDMI-1",InputName = "HDMI-1", USBUpstream = "USB-C"  } },
            };

            string swapinput1= "PCInfo1";
            string swapinput2 = "PCInfo2";

            var PCInfoSwapResult = usbKVMPlugin.PCInfoSwap(pcsList, swapinput1, swapinput2).Result;

            Assert.That(pcsList2["PCInfo1"].InputType, Is.EqualTo(PCInfoSwapResult["PCInfo1"].InputType));
            Assert.That(pcsList2["PCInfo1"].InputName, Is.EqualTo(PCInfoSwapResult["PCInfo1"].InputName));
            Assert.That(pcsList2["PCInfo1"].USBUpstream, Is.EqualTo(PCInfoSwapResult["PCInfo1"].USBUpstream));
            Assert.That(pcsList2["PCInfo2"].InputType, Is.EqualTo(PCInfoSwapResult["PCInfo2"].InputType));
            Assert.That(pcsList2["PCInfo2"].InputName, Is.EqualTo(PCInfoSwapResult["PCInfo2"].InputName));
            Assert.That(pcsList2["PCInfo2"].USBUpstream, Is.EqualTo(PCInfoSwapResult["PCInfo2"].USBUpstream));
        }

        [Test]
        public void TestInitializeDeviceManagerPlugin()
        {
            var InitializeDeviceManagerPlugin=DeviceManagerSAService.Object;
            PrivateObject privateusbKVMPluginObject = new PrivateObject(usbKVMPlugin);
            privateusbKVMPluginObject.SetFieldOrProperty("_DeviceManagerPlugin", DeviceManagerSAService.Object);
            privateusbKVMPluginObject.Invoke("InitializeDeviceManagerPlugin");
            var result = privateusbKVMPluginObject.GetFieldOrProperty("_DeviceManagerPlugin");
            Assert.IsNotNull(result);
            Assert.That(InitializeDeviceManagerPlugin,Is.EqualTo(result));
            
        }


        [Test]
        public void TestOnDeviceManagerPluginConditionChangeHandler()
        {
            object sender=new object();
            EventArgs e=new EventArgs();
            var InitializeDeviceManagerPlugin = DeviceManagerSAService.Object;
            PrivateObject privateusbKVMPluginObject = new PrivateObject(usbKVMPlugin);
            privateusbKVMPluginObject.SetFieldOrProperty("_DeviceManagerPlugin", DeviceManagerSAService.Object);
            privateusbKVMPluginObject.Invoke("OnDeviceManagerPluginConditionChangeHandler", sender, e);
            var result2 = privateusbKVMPluginObject.GetFieldOrProperty("_DeviceManagerPlugin");
            Assert.IsNotNull(result2);
            Assert.That(InitializeDeviceManagerPlugin, Is.EqualTo(result2));

        }

        [Test]
        public void TestPluginManagerOnPluginsStarted()
        {
            object sender=new object();
            List<IFrameworkPlugin> startedPlugins = new List<IFrameworkPlugin>();
            PluginsStartedEventArgs e = new PluginsStartedEventArgs(startedPlugins);
            PrivateObject privateusbKVMPluginObject = new PrivateObject(usbKVMPlugin);
            privateusbKVMPluginObject.Invoke("PluginManagerOnPluginsStarted", sender, e);
        }

        [Test]
        public void TestPluginsStarted()
        {
            object sender = new object();
            List<IFrameworkPlugin> startedPlugins = new List<IFrameworkPlugin>();
            PluginsStartedEventArgs e = new PluginsStartedEventArgs(startedPlugins);
            PrivateObject privateusbKVMPluginObject = new PrivateObject(usbKVMPlugin);
            var result=privateusbKVMPluginObject.Invoke("PluginsStarted", sender, e);
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            displayPlugin.Dispose();
            vcpCorePlugin.Dispose();
            usbKVMPlugin.Dispose();
        }

    }
}