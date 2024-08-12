using DDPM.SA.Common;
using DDPM.SA.Plugins.User.DisplayManager;
using DDPM.SA.Plugins.User.PipPbpManger;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.Common.Annotations;
using Dell.Client.Framework.Interfaces;
using Dell.Client.Framework.UnitTestShared.Tests;
using Moq;
using System.Data;
using System.Threading;
using VcpCore.Common;
using VcpCore.Interfaces;
using VcpCore.Plugins;
using WinCopies;
using Windows.Media.AppBroadcasting;
using Windows.UI.ViewManagement;
using static VcpCore.Common.User32;
using DDPM.SA.Plugins.User.DisplayProperties;
using static DDPM.SA.Plugins.User.DisplayProperties.user32;
using static VcpCore.Common.dxva2;

namespace VcpCore.Plugins.Test
{
    public class TestVCPCorePlugin
    {
        private Mock<IAgent> DisplayMangerAgent { get; } = new();
        private Mock<IAgent> VcpCoreAgent { get; } = new();
        private Mock<IAgent> PipPbpAgent = new();
        private Mock<IAgent> DisplayPropertiesAgent { get; } = new();
        MonitorInfo monitorInfo = new MonitorInfo();


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

        private PipPbpMangerPlugin CreateInitializePipPbpPlugin()
        {
            PipPbpAgent.Setup(x => x.PluginManager.FindPluginByGuid(Guid.Parse(DDPM.SA.Common.IDs.PipPbp_Manager_PLUGIN_ID)));

            return new PipPbpMangerPlugin(PipPbpAgent.Object);
        }

        private DisplayPropertiesPlugins CreateInitializedisplayPropertiesPlugin()
        {
            DisplayPropertiesAgent.Setup(x => x.PluginManager.FindPluginByGuid(Guid.Parse(DDPM.SA.Common.IDs.DisplayProperties_PLUGIN_ID)));

            return new DisplayPropertiesPlugins(DisplayPropertiesAgent.Object);
        }

        DisplayMangerPlugin displayPlugin;
        VcpCorePlugin vcpCorePlugin;
        PipPbpMangerPlugin pipPbpMangerPlugin;
        Dictionary<string, Dictionary<string, string>> getstr;
        DisplayPropertiesPlugins displayPropertiesPlugin;

        [OneTimeSetUp]
        public void Setup()
        {
            displayPlugin = CreateInitializeDisplayMangerPlugin();


            vcpCorePlugin = CreateInitializeVcpCorePlugin();
            pipPbpMangerPlugin = CreateInitializePipPbpPlugin();
            displayPropertiesPlugin = CreateInitializedisplayPropertiesPlugin();

            PrivateObject privateObject = new PrivateObject(displayPlugin);
            PrivateObject privatevcp = new PrivateObject(vcpCorePlugin);
            PrivateObject privatepippbp = new PrivateObject(pipPbpMangerPlugin);
            PrivateObject privatedisplayProperties = new PrivateObject(displayPropertiesPlugin);

            privateObject.SetField("_pipPbpService", pipPbpMangerPlugin as IPipPbpService);
            privateObject.SetField("_VcpCorePlugin", vcpCorePlugin as IVcpCoreService);

            privatepippbp.SetField("_DisplayManagerPlugin", displayPlugin as IDisplayService);
            privateObject.SetField("_DisplayPropertiesPlugin", displayPropertiesPlugin as IDisplayProperties);
            //privatedisplayProperties.SetField("_displayPropertiesInfo", displayPropertiesPlugin as IDisplayProperties);
            getstr = (Dictionary<string, Dictionary<string, string>>)privatevcp.GetField("_ColorPresets");

        }

        [Test]
        public void TestVcpCorePlugin()
        {
            //DisplayMangerPlugin displayMangerPlugin = new DisplayMangerPlugin(DisplayMangerAgent.Object);
            Assert.IsNotNull(vcpCorePlugin);
            PrivateObject privatevcp = new PrivateObject(vcpCorePlugin);
            var agent2 = privatevcp.GetField("_agent") as IAgent;
            Assert.That(agent2, Is.EqualTo(VcpCoreAgent.Object));
        }

        [Test]
        public void TestReset0x52TimerTick()
        {
            int millisecond = 3000;
            var Reset0x52_result = vcpCorePlugin.Reset0x52TimerTick(millisecond);
            Assert.IsNotNull(Reset0x52_result);
            PrivateObject privatevcp = new PrivateObject(vcpCorePlugin);
            System.Timers.Timer CacheTimer = (System.Timers.Timer)privatevcp.GetField("_CacheTimer");
            Assert.That(millisecond, Is.EqualTo(CacheTimer.Interval));
            Assert.True(CacheTimer.Enabled);
        }


        [Test]
        public void TestGetMonitors()
        {
            var getMonitors = vcpCorePlugin.GetMonitors().Result;
            Assert.Greater(getMonitors.Count, 0);

        }

        [Test]
        public void TestRe_GetMonitors()
        {
            var getMonitors = vcpCorePlugin.Re_GetMonitors().Result;
            Assert.Greater(getMonitors.Count, 0);

        }

        [Test]
        public void TestGetCapabilitiesString()
        {
            var getMonitors = vcpCorePlugin.GetMonitors().Result;
            Assert.Greater(getMonitors.Count, 0);
            var monitorInfo = getMonitors[0];
            var getCapabilitiesString = vcpCorePlugin.GetCapabilitiesString(monitorInfo).Result;
            Assert.Greater(getCapabilitiesString.Length, 0);
        }

        [Test]
        public void TestGetVCPCapabilities()
        {
            var getMonitors = vcpCorePlugin.GetMonitors().Result;
            Assert.Greater(getMonitors.Count, 0);
            var monitorInfo = getMonitors[0];
            var getVCPCapabilities = vcpCorePlugin.GetVCPCapabilities(monitorInfo).Result;
            Assert.Greater(getVCPCapabilities.Length, 0);
        }

        [Test]
        public void TestGetVCPCapability_()
        {
            var getMonitors = vcpCorePlugin.GetMonitors().Result;
            Assert.Greater(getMonitors.Count, 0);
            var monitorInfo = getMonitors[0];
            var result = vcpCorePlugin.GetVCPCapability(monitorInfo, 20).Result;// Contrast = 18,  ColorSpace14 = 20
            if (result.result)
            {
                var getValue = result.value;
                uint uVCPCapability = System.Convert.ToUInt32(result.value.ToString());
                string getMointorColorToStr = uVCPCapability.ToString("X2");
                bool isGetVPCInVcpCode = getstr["14"].Any(x => x.Value.Equals(getMointorColorToStr));
                Assert.IsTrue(isGetVPCInVcpCode);
            }
            else
            {
                Assert.IsTrue(result.result);
            }
        }

        [Test]
        public void TestGetVCPCapability()
        {
            var getMonitors = vcpCorePlugin.GetMonitors().Result;
            Assert.Greater(getMonitors.Count, 0);
            var monitorInfo = getMonitors[0];
            var result = vcpCorePlugin.GetVCPCapability(monitorInfo, "226").Result;//ColorSpaceE2 = 226,
            if (result.result)
            {
                var getValue = result.value;
                uint uVCPCapability = System.Convert.ToUInt32(result.value.ToString());
                string getMointorColorToStr_ = uVCPCapability.ToString("X2");
                bool isGetVPCInVcpCode_ = getstr["E2"].Any(x => x.Value.Equals(getMointorColorToStr_));
                Assert.IsTrue(isGetVPCInVcpCode_);
            }
            else
            {
                Assert.IsTrue(result.result);
            }
        }


        [Test]
        public void TestSetVCPCapability()
        {
            var getMonitors = vcpCorePlugin.GetMonitors().Result;
            Assert.Greater(getMonitors.Count, 0);
            var monitorInfo = getMonitors[0];
            var getMonitorVCP = vcpCorePlugin.GetVCPCapability(monitorInfo, 18, 0).Result.value; // Brightness = 16,Contrast = 18,             
            int newContrast;
            do { newContrast = new Random().Next(50, 80); } while (newContrast.ToString() == getMonitorVCP.ToString());
            bool setBrightness = vcpCorePlugin.SetVCPCapability(monitorInfo, 18, (uint)newContrast).Result;
            Assert.IsTrue(setBrightness);
            var regetMonitorVCP = vcpCorePlugin.GetVCPCapability(monitorInfo, 18, 0).Result.value;
            Assert.That(regetMonitorVCP.ToString(), Is.EqualTo(newContrast.ToString()));

        }


        [Test]
        public void TestSetVCPCapability_()
        {
            var getMonitors = vcpCorePlugin.GetMonitors().Result;
            Assert.Greater(getMonitors.Count, 0);
            var monitorInfo = getMonitors[0];
            bool setColor = vcpCorePlugin.SetVCPCapability(monitorInfo, "colorpreset", "Warm").Result;//SetVCPCapability_ FunctionName.ToLower() case "colorpreset":
            Assert.IsTrue(setColor);
            var getMonitorColor = vcpCorePlugin.GetVCPCapability(monitorInfo, 20, 0).Result.value;//ColorSpace14 = 20,
            uint uMonitorColor = System.Convert.ToUInt32(getMonitorColor.ToString());
            string getMointorColorToStr = uMonitorColor.ToString("X2");
            var getVcpCoreColor = getstr["14"].Where(x => x.Value.Equals(getMointorColorToStr)).FirstOrDefault().Value;
            Assert.That(actual: getVcpCoreColor, Is.EqualTo(getMointorColorToStr));

            bool reSetColor = vcpCorePlugin.SetVCPCapability(monitorInfo, "colorpreset", "Custom Color").Result;
            Assert.IsTrue(reSetColor);
            var reGetMonitorColor = vcpCorePlugin.GetVCPCapability(monitorInfo, 20, 0).Result.value;
            uint urMonitorColor = System.Convert.ToUInt32(reGetMonitorColor.ToString());
            string regetMointorColorToStr = urMonitorColor.ToString("X2");
            var reGetVcpCoreColor = getstr["14"].Where(x => x.Value.Equals(regetMointorColorToStr)).FirstOrDefault().Value;
            Assert.That(actual: reGetVcpCoreColor, Is.EqualTo(regetMointorColorToStr));

        }


        [Test]
        public void TestSetColorPreset()
        {
            var getMonitors = vcpCorePlugin.GetMonitors().Result;
            Assert.Greater(getMonitors.Count, 0);
            var monitorInfo = getMonitors[0];
            PrivateObject privatevcp = new PrivateObject(vcpCorePlugin);
            List<(MonitorInfo_complex, MonitorInfo)> AllInfoMonitors_Mix = (List<(MonitorInfo_complex, MonitorInfo)>)privatevcp.GetField("_AllInfoMonitors_Mix");
            //var AllInfoMonitors= _AllInfoMonitors.
            foreach (var moX in AllInfoMonitors_Mix)
            {
                bool setColor = vcpCorePlugin.SetColorPreset(moX.Item1, "Warm");//SetVCPCapability_ FunctionName.ToLower() case "colorpreset":
                Assert.IsTrue(setColor);
            }
            var getMonitorColor = vcpCorePlugin.GetVCPCapability(monitorInfo, 20, 0).Result.value;//VcpCode : byte , VcpCode.ColorSpace14 = 20,
            uint uMonitorColor = System.Convert.ToUInt32(getMonitorColor.ToString());// convert to uint
            string getMointorColorToStr = uMonitorColor.ToString("X2");    //change to 16 hex, and two number show
            var getVcpCoreColor = getstr["14"].Where(x => x.Value.Equals(getMointorColorToStr)).FirstOrDefault().Value;
            Assert.That(actual: getVcpCoreColor, Is.EqualTo(getMointorColorToStr));
            foreach (var moX in AllInfoMonitors_Mix)
            {
                bool setColor = vcpCorePlugin.SetColorPreset(moX.Item1, "Custom Color");//SetVCPCapability_ FunctionName.ToLower() case "colorpreset":
                Assert.IsTrue(setColor);
            }
            var reGetMonitorColor = vcpCorePlugin.GetVCPCapability(monitorInfo, 20, 0).Result.value;
            uint urMonitorColor = System.Convert.ToUInt32(reGetMonitorColor.ToString());
            string regetMointorColorToStr = urMonitorColor.ToString("X2");
            var reGetVcpCoreColor = getstr["14"].Where(x => x.Value.Equals(regetMointorColorToStr)).FirstOrDefault().Value;
            Assert.That(actual: reGetVcpCoreColor, Is.EqualTo(regetMointorColorToStr));
        }
    }
}