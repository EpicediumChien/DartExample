using DDPM.SA.Common;
using DDPM.SA.Common.Settings;
using DDPM.SA.Plugins.User.DisplayManager;
using DDPM.SA.Plugins.User.DisplayProperties;
using DDPM.SA.Plugins.User.EasyArrange;
using DDPM.SA.Plugins.User.EzMemory;
using Dell.Client.Framework.Interfaces;
using Dell.Client.Framework.UnitTestShared.Tests;
using Moq;
using System.Security.AccessControl;
using VcpCore.Common;
using VcpCore.Interfaces;
using VcpCore.Plugins;
using static Microsoft.WindowsAPICodePack.Shell.PropertySystem.SystemProperties.System;
using IDs = DDPM.SA.Common.IDs;
using Task = System.Threading.Tasks.Task;

namespace DDPM.SA.Plugins.User.EzMemory.Test
{
    public class TestEzMemoryPlugin
    {
        private Mock<IAgent> DisplayMangerAgent { get; } = new();
        private Mock<IAgent> VcpCoreAgent { get; } = new();
        private Mock<IAgent> EzMemoryAgent { get; } = new();

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

        private EzMemoryPlugin CreateInitializeEzMemoryPlugin()
        {
            EzMemoryAgent.Setup(x => x.PluginManager.FindPluginByGuid(Guid.Parse(IDs.DDPM_EMPlugin_PLUGIN_ID)));

            return new EzMemoryPlugin(EzMemoryAgent.Object);
        }

        DisplayMangerPlugin displayPlugin;
        VcpCorePlugin vcpCorePlugin;
        EzMemoryPlugin ezMemoryPlugin; 

        [OneTimeSetUp]
        public void Setup()
        {
            displayPlugin = CreateInitializeDisplayMangerPlugin();
            vcpCorePlugin = CreateInitializeVcpCorePlugin();
            ezMemoryPlugin= CreateInitializeEzMemoryPlugin();

            PrivateObject privateObject = new PrivateObject(displayPlugin);
            PrivateObject privatevcp = new PrivateObject(vcpCorePlugin);
            PrivateObject privateezMemory = new PrivateObject(ezMemoryPlugin);
            privateObject.SetField("_VcpCorePlugin", vcpCorePlugin as IVcpCoreService);
        }

        [Test]
        public void TestEzMemoryPlugins()
        {
            Assert.IsNotNull(ezMemoryPlugin);
            PrivateObject privateezMemory = new PrivateObject(ezMemoryPlugin);
            var agent2 = privateezMemory.GetField("_agent") as IAgent;
            Assert.That(agent2, Is.EqualTo(EzMemoryAgent.Object));
        }

        [Test]
        public void TestStopEzMemoryManger()
        {
            var StopEzMemoryManger_result = ezMemoryPlugin.StopEzMemoryManger();
            Assert.IsTrue(StopEzMemoryManger_result.IsCompletedSuccessfully);
        }

        [Test]
        public void TestStartEzMemoryManger()
        {
            int millisecond = 2000;
            var StartEzMemoryManger_result = ezMemoryPlugin.StartEzMemoryManger(millisecond);
            Assert.IsTrue(StartEzMemoryManger_result.IsCompletedSuccessfully);
        }

        [Test]
        public void TestDisposeEzMemoryManger()
        {
            var DisposeEzMemoryManger_result = ezMemoryPlugin.DisposeEzMemoryManger();
            Assert.IsTrue(DisposeEzMemoryManger_result.IsCompletedSuccessfully);
        }

        [Test]
        public void TestIsDisposed()
        {
            var Result = ezMemoryPlugin.IsDisposed;
            Assert.IsFalse(Result);
            PrivateObject privatehotkeyPluginObject = new PrivateObject(ezMemoryPlugin);
            privatehotkeyPluginObject.SetFieldOrProperty("IsDisposed", true);
            var Result2 = ezMemoryPlugin.IsDisposed;
            Assert.IsTrue(Result2);
        }

        [Test]
        public void TestInitializeEzMemoryPlugin()
        {
            Mock<ISettingsManagerDev> MockSettingsPlugin = new Mock<ISettingsManagerDev>();
            var SettingsPluginObj= MockSettingsPlugin.Object;
            PrivateObject privateezMemory = new PrivateObject(ezMemoryPlugin);
            privateezMemory.SetFieldOrProperty("_SettingsPlugin", SettingsPluginObj);
            privateezMemory.Invoke("InitializeEzMemoryPlugin");
            var EzMemoryPlugin_result = (ISettingsManagerDev)privateezMemory.GetFieldOrProperty("_SettingsPlugin");
            Assert.That(SettingsPluginObj,Is.EqualTo(EzMemoryPlugin_result));
        }

        [Test]
        public void TestInitializeMonitorInfo()
        {
            Mock<IDisplayService> Mock_DisplayManagerPlugin = new Mock<IDisplayService>();
            List<MonitorInfo> _allInfoMonitors = new List<MonitorInfo>();
            _allInfoMonitors.Add(monitorInfo1);
            Mock_DisplayManagerPlugin.Setup(x => x.GetMonitors()).Returns(Task.FromResult(_allInfoMonitors));
            var DisplayManagerPluginObj = Mock_DisplayManagerPlugin.Object;
            PrivateObject privateezMemory = new PrivateObject(ezMemoryPlugin);
            privateezMemory.SetFieldOrProperty("_DisplayManagerPlugin", DisplayManagerPluginObj);
            privateezMemory.Invoke("InitializeMonitorInfo");
            var AllInfoMonitors_result = (List<MonitorInfo>)privateezMemory.GetFieldOrProperty("_AllInfoMonitors");
            Assert.Greater(AllInfoMonitors_result.Count,0);
            Assert.That(_allInfoMonitors, Is.EqualTo(AllInfoMonitors_result));
        }
     
        [OneTimeTearDown]
        public void TearDown()
        {
            displayPlugin.Dispose();
            vcpCorePlugin.Dispose();
            ezMemoryPlugin.Dispose();
        }
    }
}