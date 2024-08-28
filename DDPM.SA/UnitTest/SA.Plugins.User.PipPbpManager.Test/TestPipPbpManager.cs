using DDPM.SA.Common.Interfaces;
using DDPM.SA.Common;
using DDPM.SA.Plugins.User.DisplayManager;
using DDPM.SA.Plugins.User.PipPbpManger;
using Dell.Client.Framework.Interfaces;
using Dell.Client.Framework.UnitTestShared.Tests;
using Moq;
using VcpCore.Common;
using VcpCore.Interfaces;
using VcpCore.Plugins;
using Dell.Client.Framework.Agent;
using System.Security.Cryptography.X509Certificates;
using DDPM.SA.Common.Display;
using static VcpCore.Common.User32;

namespace SA.Plugins.User.PipPbpManager.Test
{
    public class TestPipPbpManager
    {
        private Mock<IAgent> DisplayMangerAgent { get; } = new();
        private Mock<IAgent> VcpCoreAgent { get; } = new();
        private Mock<IAgent> PipPbpAgent { get; } = new();

        MonitorInfo monitorInfo = new MonitorInfo();
        private Mock<IVcpCoreService> VcpCoreService { get; } = new();
        private Mock<IPipPbpService> PipPbpService { get; } = new();
        private Mock<IDisplayService> DisplayManagerService { get; } = new();
        private Mock<IDisplayService> DisplayManagerService2 { get; } = new();

        private IDisplayService _displayManagerPlugin;

        MonitorInfo monitorInfo1 = new MonitorInfo()
        {
            AliasDeviceName = "Dell U2724DE(HDMI)",
            IsDellMonitor = true,
            Index = 0,
            CapabilityString = "(prot(monitor)type(LCD)model(U2724DE)cmds(01 02 03 07 0C E3 F3)vcp(02 04 05 08 10 12 14(01 04 05 06 08 09 0B 0C) 16 18 1A 52 60(19 0F 11 ) E9(00 01 02 21 22 24 ) mccs_ver(2.1))",
            DisplayName = "DISPLAY7",
            DDCisON = true,
            FwVersion = "M3T101",
            inputSource = "HDMI-1",
            modelName = "U2724DE",
            series = "Dell UltraSharp (U) Series Monitors",
            edid = new EDID() { SerialNumber = "808597589" },
            //CapabilityDic = capabilityDic;
        };

        ALSConfig aconfig = new ALSConfig()
        {
            DisplayName = "DISPLAY7",
            serialNumber = "808597589",
            isSupportALS = 2,
            isMMSEnable = false,
            isPrimaryMonitorSync = false,
            isAutoBrightness = false,
            isAutoColorTemp = false,
            LiftTone = 0,
            AllValue = 0,
            result = false,
            AutoBrightnessRangeLevel = new List<AutoBrightnessRangeLevel>() { new AutoBrightnessRangeLevel() { level_name = "Low", level_value = 0 } }
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

        private PipPbpMangerPlugin CreateInitializePipPbpPlugin()
        {
            PipPbpAgent.Setup(x => x.PluginManager.FindPluginByGuid(Guid.Parse(DDPM.SA.Common.IDs.PipPbp_Manager_PLUGIN_ID)));

            return new PipPbpMangerPlugin(PipPbpAgent.Object);
        }

        DisplayMangerPlugin displayPlugin;
        VcpCorePlugin vcpCorePlugin;
        PipPbpMangerPlugin pipPbpMangerPlugin;
        Dictionary<string, Dictionary<string, string>> getstr;

        [OneTimeSetUp]
        public void Setup()
        {
            displayPlugin = CreateInitializeDisplayMangerPlugin();

            vcpCorePlugin = CreateInitializeVcpCorePlugin();
            pipPbpMangerPlugin = CreateInitializePipPbpPlugin();

            PrivateObject privateObject = new PrivateObject(displayPlugin);
            PrivateObject privatevcp = new PrivateObject(vcpCorePlugin);
            PrivateObject privatepippbp = new PrivateObject(pipPbpMangerPlugin);

            privateObject.SetField("_pipPbpService", pipPbpMangerPlugin as IPipPbpService);
            privateObject.SetField("_VcpCorePlugin", vcpCorePlugin as IVcpCoreService);

            privatepippbp.SetField("_DisplayManagerPlugin", displayPlugin as IDisplayService);
            getstr = (Dictionary<string, Dictionary<string, string>>)privatevcp.GetField("_ColorPresets");
        }

        [Test]
        public void TestGetMonitors()
        {
            List<MonitorInfo> _allInfoMonitors = new List<MonitorInfo>();
            _allInfoMonitors.Add(monitorInfo1);
            VcpCoreService.Setup(x => x.GetMonitors(It.IsAny<bool>())).Returns(Task.FromResult(_allInfoMonitors));
            var VcpCoreServiceObject = VcpCoreService.Object;
            PrivateObject privatedispalypluginObject = new PrivateObject(displayPlugin);
            privatedispalypluginObject.SetField("_VcpCorePlugin", VcpCoreServiceObject);
            var getMonitors = displayPlugin.GetMonitors().Result;
            Assert.Greater(getMonitors.Count, 0);
        }

        [Test]
        public void TestPipPbpMangerPlugin()
        {
            Assert.IsNotNull(pipPbpMangerPlugin);
            PrivateObject privatepipPbp = new PrivateObject(pipPbpMangerPlugin);
            var agent2 = privatepipPbp.GetField("_agent") as IAgent;
            Assert.That(agent2, Is.EqualTo(PipPbpAgent.Object));
        }

        [Test]
        public void TestLastError()
        {
            string error2 = "Some error message";
            PrivateObject privatepipPbp2 = new PrivateObject(pipPbpMangerPlugin);
            privatepipPbp2.SetFieldOrProperty("_lastError", error2);
            var LastErrorResult2 = pipPbpMangerPlugin.LastError;
            Assert.That(error2, Is.EqualTo(LastErrorResult2));
        }


        [Test]
        public void TestGetCapabilitiesString()
        {
            string getcapabilitiesString1 = "";
            string signature = "E9(";
            string signature1 = "E8(";
            if (getcapabilitiesString1=="")
            { 
            var GetCapabilitiesStringResult1 = pipPbpMangerPlugin.GetCapabilitiesString(monitorInfo1).Result;
            Assert.That(getcapabilitiesString1, Is.EqualTo(GetCapabilitiesStringResult1));
            }

            if (signature1 != "E9(")
            {
                string capabilitiesString2 = "(prot(monitor)type(LCD)model(U2724DE)cmds(01 02 03 07 0C E3 F3)vcp(02 04 05 08 10 12 14(01 04 05 06 08 09 0B 0C) 16 18 1A 52 60(19 0F 11 ) mccs_ver(2.1))";
                PrivateObject privatepipPbp = new PrivateObject(pipPbpMangerPlugin);
                DisplayManagerService.Setup(x => x.GetCapabilitiesString(It.IsAny<MonitorInfo>())).Returns(Task.FromResult(capabilitiesString2));
                privatepipPbp.SetFieldOrProperty("_DisplayManagerPlugin", DisplayManagerService.Object);
                var GetCapabilitiesStringResult2 = pipPbpMangerPlugin.GetCapabilitiesString(monitorInfo1).Result;
                Assert.That(getcapabilitiesString1, Is.EqualTo(GetCapabilitiesStringResult2));
            }

            if (signature== "E9(") 
            { 
            string capabilitiesString3 = "(prot(monitor)type(LCD)model(U2724DE)cmds(01 02 03 07 0C E3 F3)vcp(02 04 05 08 10 12 14(01 04 05 06 08 09 0B 0C) 16 18 1A 52 60(19 0F 11 ) E9(00 01 02 21 22 24 ) mccs_ver(2.1))";
            string getcapabilitiesString3 = "00 01 02 21 22 24 ";
            PrivateObject privatepipPbp = new PrivateObject(pipPbpMangerPlugin);
            DisplayManagerService.Setup(x=>x.GetCapabilitiesString(It.IsAny<MonitorInfo>())).Returns(Task.FromResult(capabilitiesString3));
            privatepipPbp.SetFieldOrProperty("_DisplayManagerPlugin", DisplayManagerService.Object);
            var GetCapabilitiesStringResult3 = pipPbpMangerPlugin.GetCapabilitiesString(monitorInfo1).Result;
            Assert.That(getcapabilitiesString3, Is.EqualTo(GetCapabilitiesStringResult3));
            }
        }

        [Test]
        public void TestGetCapabilitiesWords()
        {
            string pipPbpCapsStr = "";
            string pipPbpCapsStr2 = "E9(";
            UInt16[] pipPbpCapsStrushorts = new UInt16[0];
            if (pipPbpCapsStr == "")
            {
                PrivateObject privatepipPbp2 = new PrivateObject(pipPbpMangerPlugin);
                privatepipPbp2.SetFieldOrProperty("_DisplayManagerPlugin", _displayManagerPlugin);
                var GetCapabilitiesWordsResult1 = pipPbpMangerPlugin.GetCapabilitiesWords(monitorInfo1).Result;
                Assert.That(pipPbpCapsStrushorts, Is.EqualTo(GetCapabilitiesWordsResult1));
            }

            if (pipPbpCapsStr2 != "")
            {
                string capabilitiesString2 = "(prot(monitor)type(LCD)model(U2724DE)cmds(01 02 03 07 0C E3 F3)vcp(02 04 05 08 10 12 14(01 04 05 06 08 09 0B 0C) 16 18 1A 52 60(19 0F 11 ) E9(00 01 02 21 22 24 ) mccs_ver(2.1))";
                UInt16[] pipPbpCaps2=new UInt16[6] { 00 ,01 ,02 ,33 ,34, 36 };
                PrivateObject privatepipPbp2 = new PrivateObject(pipPbpMangerPlugin);
                DisplayManagerService2.Setup(x => x.GetCapabilitiesString(It.IsAny<MonitorInfo>())).Returns(Task.FromResult(capabilitiesString2));
                privatepipPbp2.SetFieldOrProperty("_DisplayManagerPlugin", DisplayManagerService2.Object);
                var GetCapabilitiesWordsResult2 = pipPbpMangerPlugin.GetCapabilitiesWords(monitorInfo1).Result;
                Assert.That(pipPbpCaps2, Is.EqualTo(GetCapabilitiesWordsResult2));
            }
        }

        [Test]
        public void TestParsingHexStringToWords()
        {
            string inStr = "";
            string inStr2 = "02 04 05 08 10 12";
            UInt16[] pipPbpCapsStringToWords = new UInt16[0];
            UInt16[] pipPbpCapsStringToWords2 = new UInt16[6] {02, 04, 05, 08, 16, 18 };
            if (inStr == "")
            {
                var ParsingHexStringToWordsResult1 = PipPbpMangerPlugin.ParsingHexStringToWords(inStr);
                Assert.That(pipPbpCapsStringToWords, Is.EqualTo(ParsingHexStringToWordsResult1));
            }

            if (inStr2 != "")
            {
                var ParsingHexStringToWordsResult2 = PipPbpMangerPlugin.ParsingHexStringToWords(inStr2);
                Assert.That(pipPbpCapsStringToWords2, Is.EqualTo(ParsingHexStringToWordsResult2));
            }
        }
    }
}