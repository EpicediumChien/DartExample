using DDPM.SA.Common;
using DDPM.SA.Common.Display;
using DDPM.SA.Plugins.User.DisplayManager;
using DDPM.SA.Plugins.User.PipPbpManger;
using Dell.Client.Framework.Interfaces;
using Dell.Client.Framework.UnitTestShared.Tests;
using Moq;
using VcpCore.Common;
using VcpCore.Interfaces;
using VcpCore.Plugins;

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
            DisplayMangerAgent.Setup(x => x.PluginManager.FindPluginByGuid(Guid.Parse(DDPM.SA.Common.IDs.Display_Manager_PLUGIN_ID)));

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
            VcpCoreService.Setup(x => x.GetMonitors()).Returns(Task.FromResult(_allInfoMonitors));
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
            if (getcapabilitiesString1 == "")
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

            if (signature == "E9(")
            {
                string capabilitiesString3 = "(prot(monitor)type(LCD)model(U2724DE)cmds(01 02 03 07 0C E3 F3)vcp(02 04 05 08 10 12 14(01 04 05 06 08 09 0B 0C) 16 18 1A 52 60(19 0F 11 ) E9(00 01 02 21 22 24 ) mccs_ver(2.1))";
                string getcapabilitiesString3 = "00 01 02 21 22 24 ";
                PrivateObject privatepipPbp = new PrivateObject(pipPbpMangerPlugin);
                DisplayManagerService.Setup(x => x.GetCapabilitiesString(It.IsAny<MonitorInfo>())).Returns(Task.FromResult(capabilitiesString3));
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
                UInt16[] pipPbpCaps2 = new UInt16[6] { 00, 01, 02, 33, 34, 36 };
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
            UInt16[] pipPbpCapsStringToWords2 = new UInt16[6] { 02, 04, 05, 08, 16, 18 };
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

        [Test]
        public void TestSetPipModeOff()
        {
            bool SetPipModeOff1 = false;
            bool SetPipModeOff2 = true;
            string _DisplayManagerPlugin2 = "E9(";
            UInt16[] pipPbpCapsStrushorts = new UInt16[0];
            string _DisplayManagerPlugin1 = string.Empty;

            if (_DisplayManagerPlugin1 == "")
            {
                PrivateObject privatepipPbp2 = new PrivateObject(pipPbpMangerPlugin);
                privatepipPbp2.SetFieldOrProperty("_DisplayManagerPlugin", _displayManagerPlugin);
                var SetPipModeOffResult1 = pipPbpMangerPlugin.SetPipModeOff(monitorInfo1).Result;
                Assert.That(SetPipModeOff1, Is.EqualTo(SetPipModeOffResult1));
            }

            if (_DisplayManagerPlugin2 != "")
            {
                PrivateObject privatepipPbp2 = new PrivateObject(pipPbpMangerPlugin);
                DisplayManagerService2.Setup(x => x.SetVCPCapability(It.IsAny<MonitorInfo>(), It.IsAny<byte>(), It.IsAny<uint>())).Returns(Task.FromResult(true));
                privatepipPbp2.SetFieldOrProperty("_DisplayManagerPlugin", DisplayManagerService2.Object);
                var SetPipModeOffResult2 = pipPbpMangerPlugin.SetPipModeOff(monitorInfo1).Result;
                Assert.That(SetPipModeOff2, Is.EqualTo(SetPipModeOffResult2));
            }
        }

        [Test]
        public void TestSetPipModeSmall()
        {
            bool SetPipModeSmall1 = false;
            bool SetPipModeSmall2 = true;

            string _DisplayManagerPlugin1 = string.Empty;
            string _DisplayManagerPlugin2 = "E9(";

            if (_DisplayManagerPlugin1 == "")
            {
                PrivateObject privatepipPbp2 = new PrivateObject(pipPbpMangerPlugin);
                privatepipPbp2.SetFieldOrProperty("_DisplayManagerPlugin", _displayManagerPlugin);
                var SetPipModeSmallResult1 = pipPbpMangerPlugin.SetPipModeSmall(monitorInfo1).Result;
                Assert.That(SetPipModeSmall1, Is.EqualTo(SetPipModeSmallResult1));
            }

            if (_DisplayManagerPlugin2 != "")
            {
                PrivateObject privatepipPbp2 = new PrivateObject(pipPbpMangerPlugin);
                DisplayManagerService2.Setup(x => x.SetVCPCapability(It.IsAny<MonitorInfo>(), It.IsAny<byte>(), It.IsAny<uint>())).Returns(Task.FromResult(true));
                privatepipPbp2.SetFieldOrProperty("_DisplayManagerPlugin", DisplayManagerService2.Object);
                var SetPipModeSmallResult2 = pipPbpMangerPlugin.SetPipModeSmall(monitorInfo1).Result;
                Assert.That(SetPipModeSmall2, Is.EqualTo(SetPipModeSmallResult2));
            }
        }

        [Test]
        public void TestSetPipModeLarge()
        {
            bool SetPipModeLarge1 = false;
            bool SetPipModeLarge2 = true;

            string _DisplayManagerPlugin1 = string.Empty;
            string _DisplayManagerPlugin2 = "E9(";

            if (_DisplayManagerPlugin1 == "")
            {
                PrivateObject privatepipPbp2 = new PrivateObject(pipPbpMangerPlugin);
                privatepipPbp2.SetFieldOrProperty("_DisplayManagerPlugin", _displayManagerPlugin);
                var SetPipModeLargeResult1 = pipPbpMangerPlugin.SetPipModeLarge(monitorInfo1).Result;
                Assert.That(SetPipModeLarge1, Is.EqualTo(SetPipModeLargeResult1));
            }

            if (_DisplayManagerPlugin2 != "")
            {
                PrivateObject privatepipPbp2 = new PrivateObject(pipPbpMangerPlugin);
                DisplayManagerService2.Setup(x => x.SetVCPCapability(It.IsAny<MonitorInfo>(), It.IsAny<byte>(), It.IsAny<uint>())).Returns(Task.FromResult(true));
                privatepipPbp2.SetFieldOrProperty("_DisplayManagerPlugin", DisplayManagerService2.Object);
                var SetPipModeLargeResult2 = pipPbpMangerPlugin.SetPipModeLarge(monitorInfo1).Result;
                Assert.That(SetPipModeLarge2, Is.EqualTo(SetPipModeLargeResult2));
            }
        }

        [Test]
        public void TestTogglePipSize()
        {
            bool TogglePipSize1 = false;
            bool TogglePipSize2 = true;

            string _DisplayManagerPlugin1 = string.Empty;
            string _DisplayManagerPlugin2 = "E9(";

            if (_DisplayManagerPlugin1 == "")
            {
                PrivateObject privatepipPbp2 = new PrivateObject(pipPbpMangerPlugin);
                privatepipPbp2.SetFieldOrProperty("_DisplayManagerPlugin", _displayManagerPlugin);
                var TogglePipSizeResult1 = pipPbpMangerPlugin.TogglePipSize(monitorInfo1).Result;
                Assert.That(TogglePipSize1, Is.EqualTo(TogglePipSizeResult1));
            }

            if (_DisplayManagerPlugin2 != "")
            {
                PrivateObject privatepipPbp2 = new PrivateObject(pipPbpMangerPlugin);
                DisplayManagerService2.Setup(x => x.SetVCPCapability(It.IsAny<MonitorInfo>(), It.IsAny<byte>(), It.IsAny<uint>())).Returns(Task.FromResult(true));
                privatepipPbp2.SetFieldOrProperty("_DisplayManagerPlugin", DisplayManagerService2.Object);
                var TogglePipSizeResult2 = pipPbpMangerPlugin.TogglePipSize(monitorInfo1).Result;
                Assert.That(TogglePipSize2, Is.EqualTo(TogglePipSizeResult2));
            }
        }

        [Test]
        public void TestTogglePipPosition()
        {
            bool TogglePipPosition1 = false;
            bool TogglePipPosition2 = true;

            string _DisplayManagerPlugin1 = string.Empty;
            string _DisplayManagerPlugin2 = "E9(";

            if (_DisplayManagerPlugin1 == "")
            {
                PrivateObject privatepipPbp2 = new PrivateObject(pipPbpMangerPlugin);
                privatepipPbp2.SetFieldOrProperty("_DisplayManagerPlugin", _displayManagerPlugin);
                var TogglePipPositionResult1 = pipPbpMangerPlugin.TogglePipPosition(monitorInfo1).Result;
                Assert.That(TogglePipPosition1, Is.EqualTo(TogglePipPositionResult1));
            }

            if (_DisplayManagerPlugin2 != "")
            {
                PrivateObject privatepipPbp2 = new PrivateObject(pipPbpMangerPlugin);
                DisplayManagerService2.Setup(x => x.SetVCPCapability(It.IsAny<MonitorInfo>(), It.IsAny<byte>(), It.IsAny<uint>())).Returns(Task.FromResult(true));
                privatepipPbp2.SetFieldOrProperty("_DisplayManagerPlugin", DisplayManagerService2.Object);
                var TogglePipPositionResult2 = pipPbpMangerPlugin.TogglePipPosition(monitorInfo1).Result;
                Assert.That(TogglePipPosition2, Is.EqualTo(TogglePipPositionResult2));
            }
        }

        [Test]
        public void TestSetPbpMode()
        {
            bool TogglePipPosition1 = false;
            bool TogglePipPosition2 = true;
            UInt16 modeCode = 0x60;
            string _DisplayManagerPlugin1 = string.Empty;
            string _DisplayManagerPlugin2 = "E9(";

            if (_DisplayManagerPlugin1 == "")
            {
                PrivateObject privatepipPbp2 = new PrivateObject(pipPbpMangerPlugin);
                privatepipPbp2.SetFieldOrProperty("_DisplayManagerPlugin", _displayManagerPlugin);
                var SetPbpModeResult1 = pipPbpMangerPlugin.SetPbpMode(monitorInfo1, modeCode).Result;
                Assert.That(TogglePipPosition1, Is.EqualTo(SetPbpModeResult1));
            }

            if (_DisplayManagerPlugin2 != "")
            {
                PrivateObject privatepipPbp2 = new PrivateObject(pipPbpMangerPlugin);
                DisplayManagerService2.Setup(x => x.SetVCPCapability(It.IsAny<MonitorInfo>(), It.IsAny<byte>(), It.IsAny<uint>())).Returns(Task.FromResult(true));
                privatepipPbp2.SetFieldOrProperty("_DisplayManagerPlugin", DisplayManagerService2.Object);
                var SetPbpModeResult2 = pipPbpMangerPlugin.SetPbpMode(monitorInfo1, modeCode).Result;
                Assert.That(TogglePipPosition2, Is.EqualTo(SetPbpModeResult2));
            }
        }

        [Test]
        public void TestVideoSwap()
        {
            bool VideoSwap1 = false;
            bool VideoSwap2 = true;
            UInt16 x = 2;
            UInt16 y = 3;  //x 0-3, y 0-3
            string _DisplayManagerPlugin1 = string.Empty;
            string _DisplayManagerPlugin2 = "E9(";

            if (_DisplayManagerPlugin1 == "")
            {
                PrivateObject privatepipPbp2 = new PrivateObject(pipPbpMangerPlugin);
                privatepipPbp2.SetFieldOrProperty("_DisplayManagerPlugin", _displayManagerPlugin);
                var VideoSwapResult1 = pipPbpMangerPlugin.VideoSwap(monitorInfo1, x, y).Result;
                Assert.That(VideoSwap1, Is.EqualTo(VideoSwapResult1));
            }

            if (_DisplayManagerPlugin2 != "")
            {
                PrivateObject privatepipPbp2 = new PrivateObject(pipPbpMangerPlugin);
                DisplayManagerService2.Setup(x => x.SetVCPCapability(It.IsAny<MonitorInfo>(), It.IsAny<byte>(), It.IsAny<uint>())).Returns(Task.FromResult(true));
                privatepipPbp2.SetFieldOrProperty("_DisplayManagerPlugin", DisplayManagerService2.Object);
                var VideoSwapResult2 = pipPbpMangerPlugin.VideoSwap(monitorInfo1, x, y).Result;
                Assert.That(VideoSwap2, Is.EqualTo(VideoSwapResult2));
            }
        }

        [Test]
        public void TestGetPxpMode()
        {
            ObjGetVCP GetPxpMode1 = new ObjGetVCP() { result = false, value = 255 };
            ObjGetVCP GetPxpMode2 = new ObjGetVCP() { result = true, value = 0x6f }; ;

            string _DisplayManagerPlugin1 = string.Empty;
            string _DisplayManagerPlugin2 = "E9(";

            if (_DisplayManagerPlugin1 == "")
            {
                PrivateObject privatepipPbp2 = new PrivateObject(pipPbpMangerPlugin);
                privatepipPbp2.SetFieldOrProperty("_DisplayManagerPlugin", _displayManagerPlugin);
                var GetPxpModeResult1 = pipPbpMangerPlugin.GetPxpMode(monitorInfo1).Result;
                Assert.That(GetPxpMode1.result, Is.EqualTo(GetPxpModeResult1.result));
                Assert.That(GetPxpMode1.value, Is.EqualTo(GetPxpModeResult1.value));
            }

            if (_DisplayManagerPlugin2 != "")
            {
                PrivateObject privatepipPbp2 = new PrivateObject(pipPbpMangerPlugin);
                DisplayManagerService2.Setup(x => x.GetVCPCapability(It.IsAny<MonitorInfo>(), It.IsAny<byte>(), It.IsAny<int>())).Returns(Task.FromResult(GetPxpMode2));
                privatepipPbp2.SetFieldOrProperty("_DisplayManagerPlugin", DisplayManagerService2.Object);
                var GetPxpModeResult2 = pipPbpMangerPlugin.GetPxpMode(monitorInfo1).Result;
                Assert.That(GetPxpMode2.result, Is.EqualTo(GetPxpModeResult2.result));
                Assert.That(GetPxpMode2.value, Is.EqualTo(GetPxpModeResult2.value));
            }
        }

        [Test]
        public void TestGetSubInputList()
        {
            ObjGetVCP SubInputList1 = new ObjGetVCP() { result = false, value = 255 };
            ObjGetVCP SubInputList2 = new ObjGetVCP() { result = true, value = 57151 };
            List<UInt16> GetSubInputList1 = new List<UInt16>() { };
            List<UInt16> GetSubInputList2 = new List<UInt16>() { 31, 25, 23 };
            string _DisplayManagerPlugin1 = string.Empty;
            string _DisplayManagerPlugin2 = "E9(";

            if (_DisplayManagerPlugin1 == "")
            {
                //GetSubInputList1.Clear();
                GetSubInputList1 = null;
                PrivateObject privatepipPbp2 = new PrivateObject(pipPbpMangerPlugin);
                privatepipPbp2.SetFieldOrProperty("_DisplayManagerPlugin", _displayManagerPlugin);
                var GetSubInputListResult1 = pipPbpMangerPlugin.GetSubInputList(monitorInfo1).Result;
                Assert.That(GetSubInputList1, Is.EqualTo(GetSubInputListResult1));
            }

            if (_DisplayManagerPlugin2 != "")
            {
                PrivateObject privatepipPbp2 = new PrivateObject(pipPbpMangerPlugin);
                DisplayManagerService2.Setup(x => x.GetVCPCapability(It.IsAny<MonitorInfo>(), It.IsAny<byte>(), It.IsAny<int>())).Returns(Task.FromResult(SubInputList2));
                privatepipPbp2.SetFieldOrProperty("_DisplayManagerPlugin", DisplayManagerService2.Object);
                var GetSubInputListResult2 = pipPbpMangerPlugin.GetSubInputList(monitorInfo1).Result;
                Assert.That(GetSubInputList2, Is.EqualTo(GetSubInputListResult2));
            }
        }

        [Test]
        public void TestGetSubInputs()
        {
            ObjGetVCP SubInputList1 = new ObjGetVCP() { result = false, value = 255 };
            ObjGetVCP SubInputList2 = new ObjGetVCP() { result = true, value = 57151 };
            List<InputSourceObj> GetSubInputs1 = new List<InputSourceObj>() { };
            GetSubInputs1 = null;
            List<InputSourceObj> GetSubInputs2 = new List<InputSourceObj>();
            GetSubInputs2.Add(new InputSourceObj(31, ""));
            GetSubInputs2.Add(new InputSourceObj(25, "Thunderbolt-1"));
            GetSubInputs2.Add(new InputSourceObj(23, "DisplayPort-3")); //Code=15,Name="DisplayPort-1";

            string _DisplayManagerPlugin1 = string.Empty;
            string _DisplayManagerPlugin2 = "E9(";

            if (GetSubInputs1 == null)
            {
                PrivateObject privatepipPbp2 = new PrivateObject(pipPbpMangerPlugin);
                privatepipPbp2.SetFieldOrProperty("_DisplayManagerPlugin", _displayManagerPlugin);
                var GetSubInputsResult1 = pipPbpMangerPlugin.GetSubInputs(monitorInfo1).Result;
                Assert.That(GetSubInputs1, Is.EqualTo(GetSubInputsResult1));
            }

            if (GetSubInputs2 != null)
            {
                PrivateObject privatepipPbp2 = new PrivateObject(pipPbpMangerPlugin);
                DisplayManagerService2.Setup(x => x.GetVCPCapability(It.IsAny<MonitorInfo>(), It.IsAny<byte>(), It.IsAny<int>())).Returns(Task.FromResult(SubInputList2));
                var DisplayManagerService2Object = DisplayManagerService2.Object;
                privatepipPbp2.SetFieldOrProperty("_DisplayManagerPlugin", DisplayManagerService2Object);   //GetVCPCapability 0x8E :value 15 ,true

                var GetSubInputsResult2 = pipPbpMangerPlugin.GetSubInputs(monitorInfo1).Result;
                Assert.That(GetSubInputs2[0].Code, Is.EqualTo(GetSubInputsResult2[0].Code));
                Assert.That(GetSubInputs2[0].Name, Is.EqualTo(GetSubInputsResult2[0].Name));
                Assert.That(GetSubInputs2[1].Code, Is.EqualTo(GetSubInputsResult2[1].Code));
                Assert.That(GetSubInputs2[1].Name, Is.EqualTo(GetSubInputsResult2[1].Name));
                Assert.That(GetSubInputs2[2].Code, Is.EqualTo(GetSubInputsResult2[2].Code));
                Assert.That(GetSubInputs2[2].Name, Is.EqualTo(GetSubInputsResult2[2].Name));
            }
        }

        [Test]
        public void TestSetSubInputs()
        {
            ObjGetVCP SubInputList1 = new ObjGetVCP() { result = false, value = 255 };
            ObjGetVCP SubInputList2 = new ObjGetVCP() { result = true, value = 57151 };
            List<InputSourceObj> GetSubInputs1 = new List<InputSourceObj>() { };
            GetSubInputs1 = null;
            List<InputSourceObj> GetSubInputs2 = new List<InputSourceObj>();
            GetSubInputs2.Add(new InputSourceObj(31, ""));
            GetSubInputs2.Add(new InputSourceObj(25, "Thunderbolt-1"));
            GetSubInputs2.Add(new InputSourceObj(23, "DisplayPort-3")); //Code=15,Name="DisplayPort-1";

            string _DisplayManagerPlugin1 = null;
            string _DisplayManagerPlugin2 = "E9(";

            bool SetSubInputs1 = false;
            bool SetSubInputs2 = true;

            InputSourceObj? sub1 = new InputSourceObj(0x11, "HDMI-1");          //0x11, "HDMI-1",
            InputSourceObj? sub2 = new InputSourceObj(0x0F, "DisplayPort-1");  //0x0F, "DisplayPort-1",
            InputSourceObj? sub3 = new InputSourceObj(0x1B, "USB-C1");        //0x1B, "USB-C1",

            if (_DisplayManagerPlugin1 == null)
            {
                PrivateObject privatepipPbp2 = new PrivateObject(pipPbpMangerPlugin);
                privatepipPbp2.SetFieldOrProperty("_DisplayManagerPlugin", _displayManagerPlugin);
                var SetSubInputsResult1 = pipPbpMangerPlugin.SetSubInputs(monitorInfo1, sub1, sub2, sub3).Result;
                Assert.That(SetSubInputs1, Is.EqualTo(SetSubInputsResult1));
            }

            if (_DisplayManagerPlugin2 != null)
            {
                PrivateObject privatepipPbp2 = new PrivateObject(pipPbpMangerPlugin);
                DisplayManagerService2.Setup(x => x.GetVCPCapability(It.IsAny<MonitorInfo>(), It.IsAny<byte>(), It.IsAny<int>())).Returns(Task.FromResult(SubInputList2));
                DisplayManagerService2.Setup(x => x.SetVCPCapability(It.IsAny<MonitorInfo>(), It.IsAny<byte>(), It.IsAny<uint>())).Returns(Task.FromResult(true));
                var DisplayManagerService2Object = DisplayManagerService2.Object;
                privatepipPbp2.SetFieldOrProperty("_DisplayManagerPlugin", DisplayManagerService2Object);   //GetVCPCapability 0x8E :value 15 ,true

                var SetSubInputsResult2 = pipPbpMangerPlugin.SetSubInputs(monitorInfo1, sub1, sub2, sub3).Result;
                Assert.That(SetSubInputs2, Is.EqualTo(SetSubInputsResult2));
            }
        }

        [Test]
        public void TestUsbSwitch()
        {
            ObjGetVCP SubInputList1 = new ObjGetVCP() { result = false, value = 255 };
            ObjGetVCP SubInputList2 = new ObjGetVCP() { result = true, value = 57151 };
            List<UInt16> GetSubInputList1 = new List<UInt16>() { };
            List<UInt16> GetSubInputList2 = new List<UInt16>() { 31, 25, 23 };
            UInt16 target1 = 5;
            UInt16 target2 = 2;
            bool UsbSwitch1 = true;
            bool UsbSwitch2 = false;
            string _DisplayManagerPlugin1 = string.Empty;
            string _DisplayManagerPlugin2 = "E9(";

            if (target1 > 4)
            {
                var UsbSwitchResult1 = pipPbpMangerPlugin.UsbSwitch(monitorInfo1, target1).Result;
                Assert.That(UsbSwitch2, Is.EqualTo(UsbSwitchResult1));
            }

            if (target2 <= 4)
            {
                if (_DisplayManagerPlugin1 == "")
                {
                    PrivateObject privatepipPbp2 = new PrivateObject(pipPbpMangerPlugin);
                    privatepipPbp2.SetFieldOrProperty("_DisplayManagerPlugin", _displayManagerPlugin);
                    var UsbSwitchResult2 = pipPbpMangerPlugin.UsbSwitch(monitorInfo1, target2).Result;
                    Assert.That(UsbSwitch2, Is.EqualTo(UsbSwitchResult2));
                }

                if (_DisplayManagerPlugin2 != "")
                {
                    PrivateObject privatepipPbp2 = new PrivateObject(pipPbpMangerPlugin);
                    DisplayManagerService2.Setup(x => x.SetVCPCapability(It.IsAny<MonitorInfo>(), It.IsAny<byte>(), It.IsAny<uint>())).Returns(Task.FromResult(true));
                    var displayManagerService2Object = DisplayManagerService2.Object;
                    privatepipPbp2.SetFieldOrProperty("_DisplayManagerPlugin", displayManagerService2Object);
                    var UsbSwitchResult3 = pipPbpMangerPlugin.UsbSwitch(monitorInfo1, target2).Result;
                    Assert.That(UsbSwitch1, Is.EqualTo(UsbSwitchResult3));
                }
            }
        }

        [Test]
        public void TestInitializeDisplayManagerPlugin()
        {
            PrivateObject privatepipPbpObject = new PrivateObject(pipPbpMangerPlugin);
            var result = privatepipPbpObject.Invoke("InitializeDisplayManagerPlugin");
            Assert.IsNotNull(displayPlugin);
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            displayPlugin.Dispose();
            vcpCorePlugin.Dispose();
            pipPbpMangerPlugin.Dispose();  //fix 2024-08-28 complier ERROR
        }
    }
}