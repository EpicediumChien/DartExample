using DDPM.SA.Common;
using DDPM.SA.Common.Display;
using Dell.Client.Framework.UnitTestShared.Tests;
using Moq;
using VcpCore.Common;
using static DDPM.SA.Common.ICLICommandTable;

namespace CLI.Plugins.Display.Test
{
    public class TestCLIPxp
    {
        private CLIPxp cLIPxp;
        private PrivateObject PrivateObjectCLIPxp;

        private MonitorInfo monitorInfo1 = new MonitorInfo()
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

        [SetUp]
        public void Setup()
        {
            cLIPxp = new CLIPxp();
            PrivateObjectCLIPxp = new PrivateObject(cLIPxp);
        }

        public void TestResponse()
        {
            List<CLI_RESPONSE> Responses1 = new List<CLI_RESPONSE>() { new CLI_RESPONSE() { SerialNumber = "123456", Model = "Testmodel" } };
            PrivateObjectCLIPxp.SetFieldOrProperty("_responses", Responses1);
            var Response_result = CLIPxp.Responses;
            Assert.IsNotNull(Response_result);
            Assert.That(Responses1, Is.EqualTo(Response_result));
        }

        [Test]
        public void TestExecute()
        {
            int null_device_manager = 1;
            IDeviceManagerSA? devMgrNull;
            devMgrNull = null;

            CommandLineInput cmdLineInput;
            cmdLineInput = new CommandLineInput()
            {
                PluginsType = "AUDIO",
                Command = "Get",
                TargetType = "TestAPP",
                TargetFeature = "PxP",
                isCliRunAdmin = false,
                Options = new List<CommandType_Option>() { new CommandType_Option("PxP", "0x21") { Option_Name = "PxP" } },
            };
            var Execute_result1 = CLIPxp.Execute(devMgrNull, cmdLineInput);  //devMgr null
            Assert.IsNotNull(Execute_result1);
            Assert.That(Execute_result1, Is.EqualTo(null_device_manager));

            List<MonitorInfo> _allInfoMonitors = new List<MonitorInfo>();
            Dictionary<string, List<string>> CapabilityDic = new Dictionary<string, List<string>>();
            CapabilityDic.Add("E5", new List<string>() { "PxPZoom" });  //PxPZoom
            monitorInfo1.CapabilityDic = CapabilityDic;

            _allInfoMonitors.Add(monitorInfo1);
            UInt16[] caps = new ushort[1] { 0x21 };
            ObjGetVCP objGetVCPGetPxpMode = new ObjGetVCP() { result = true, value = 0x21 };
            List<InputSourceObj> inputSources = new List<InputSourceObj>() { new InputSourceObj() { Code = 0x21, Name = "SubInput" } };
            ObjGetVCP objGetVCPCapability = new ObjGetVCP() { result = true, value = 0x21 };
            bool SetPbpMode = true;
            bool SetSubInputs = true;
            bool SetVCPCapability = true;
            Mock<IDeviceManagerSA> devMgr = new Mock<IDeviceManagerSA>();
            devMgr.Setup(m => m.GetMonitors()).Returns(Task.FromResult(_allInfoMonitors));
            devMgr.Setup(m => m.GetPipPbpCapabilitiesWords(It.IsAny<MonitorInfo>())).Returns(Task.FromResult(caps));
            devMgr.Setup(m => m.GetPxpMode(It.IsAny<MonitorInfo>())).Returns(Task.FromResult(objGetVCPGetPxpMode));
            devMgr.Setup(m => m.GetSubInputs(It.IsAny<MonitorInfo>())).Returns(Task.FromResult(inputSources));
            devMgr.Setup(m => m.GetVCPCapability(It.IsAny<MonitorInfo>(), It.IsAny<byte>(), It.IsAny<Guid>(), It.IsAny<int>())).Returns(Task.FromResult(objGetVCPCapability));
            devMgr.Setup(m => m.SetPbpMode(It.IsAny<MonitorInfo>(), It.IsAny<UInt16>())).Returns(Task.FromResult(SetPbpMode));
            devMgr.Setup(m => m.SetSubInputs(It.IsAny<MonitorInfo>(), It.IsAny<InputSourceObj>(), It.IsAny<InputSourceObj>(), It.IsAny<InputSourceObj>())).Returns(Task.FromResult(SetSubInputs));
            devMgr.Setup(m => m.SetVCPCapability(It.IsAny<MonitorInfo>(), It.IsAny<byte>(), It.IsAny<uint>(), It.IsAny<Guid>())).Returns(Task.FromResult(SetVCPCapability));
            var devMgrObj = devMgr.Object;

            var Execute_result2 = CLIPxp.Execute(devMgrObj, cmdLineInput);  //devMgr not null,Get, TargetFeature PxP,"pip", 0x21, "PIP small"
            Assert.IsNotNull(Execute_result2);

            cmdLineInput = new CommandLineInput()
            {
                PluginsType = "AUDIO",
                Command = "Get",
                TargetType = "TestAPP",
                TargetFeature = "SubInput",
                isCliRunAdmin = false,
                Options = new List<CommandType_Option>() { new CommandType_Option("SubInput", "0x21") { Option_Name = "PxP" } },
            };

            var Execute_result3 = CLIPxp.Execute(devMgrObj, cmdLineInput);  //devMgr not null,Get, TargetFeature SubInput,"pip",
            Assert.IsNotNull(Execute_result3);

            cmdLineInput = new CommandLineInput()
            {
                PluginsType = "AUDIO",
                Command = "Get",
                TargetType = "TestAPP",
                TargetFeature = "PxPZoom",
                isCliRunAdmin = false,
                Options = new List<CommandType_Option>() { new CommandType_Option("PxPZoom", "0x21") { Option_Name = "PxP" } },
            };

            var Execute_result4 = CLIPxp.Execute(devMgrObj, cmdLineInput);  //devMgr not null, Get, TargetFeature PxPZoom,"pip",
            Assert.IsNotNull(Execute_result4);

            cmdLineInput = new CommandLineInput()
            {
                PluginsType = "AUDIO",
                Command = "Set",
                TargetType = "TestAPP",
                TargetFeature = "SwapVideo",
                isCliRunAdmin = false,
                Options = new List<CommandType_Option>() { new CommandType_Option("SwapVideo", "0x21") { Option_Name = "value" } },
            };

            var Execute_result5 = CLIPxp.Execute(devMgrObj, cmdLineInput);  //devMgr not null, Set, TargetFeature SwapVideo,"pip",
            Assert.IsNotNull(Execute_result5);

            cmdLineInput = new CommandLineInput()
            {
                PluginsType = "AUDIO",
                Command = "Set",
                TargetType = "TestAPP",
                TargetFeature = "PxP",
                isCliRunAdmin = false,
                Options = new List<CommandType_Option>() { new CommandType_Option("pip-small", "pip-small") { Option_Name = "value" } },
            };

            var Execute_result6 = CLIPxp.Execute(devMgrObj, cmdLineInput);  //devMgr not null, Set, TargetFeature PxP,"pip",
            Assert.IsNotNull(Execute_result6);

            cmdLineInput = new CommandLineInput()
            {
                PluginsType = "AUDIO",
                Command = "Set",
                TargetType = "TestAPP",
                TargetFeature = "SubInput",
                isCliRunAdmin = false,
                Options = new List<CommandType_Option>() { new CommandType_Option("SubInput", "HDMI") { Option_Name = "value" } },
            };

            var Execute_result7 = CLIPxp.Execute(devMgrObj, cmdLineInput);  //devMgr not null, Set, TargetFeature SubInput,"pip",
            Assert.IsNotNull(Execute_result7);

            cmdLineInput = new CommandLineInput()
            {
                PluginsType = "AUDIO",
                Command = "Set",
                TargetType = "TestAPP",
                TargetFeature = "PxPZoom",
                isCliRunAdmin = false,
                Options = new List<CommandType_Option>() { new CommandType_Option("PxPZoom", "HDMI") { Option_Name = "value" } },
            };

            var Execute_result8 = CLIPxp.Execute(devMgrObj, cmdLineInput);  //devMgr not null, Set, TargetFeature PxPZoom,"pip",
            Assert.IsNotNull(Execute_result8);

            cmdLineInput = new CommandLineInput()
            {
                PluginsType = "AUDIO",
                Command = "Set",
                TargetType = "TestAPP",
                TargetFeature = "SwapUSB",
                isCliRunAdmin = false,
                Options = new List<CommandType_Option>() { new CommandType_Option("SwapUSB", "HDMI") { Option_Name = "value" } },
            };

            var Execute_result9 = CLIPxp.Execute(devMgrObj, cmdLineInput);  //devMgr not null, Set, TargetFeature SwapUSB,"pip",
            Assert.IsNotNull(Execute_result9);

            cmdLineInput = new CommandLineInput()
            {
                PluginsType = "AUDIO",
                Command = "TestExecute",
                TargetType = "TestAPP",
                TargetFeature = "SwapUSB",
                isCliRunAdmin = false,
                Options = new List<CommandType_Option>() { new CommandType_Option("SwapUSB", "HDMI") { Option_Name = "value" } },
            };
            int fail_NotSupport = 103;
            var Execute_result10 = CLIPxp.Execute(devMgrObj, cmdLineInput);  //devMgr not null, Set, TargetFeature SwapUSB,"pip",
            Assert.IsNotNull(Execute_result10);
            Assert.That(fail_NotSupport, Is.EqualTo(Execute_result10));
        }

        [Test]
        public void Testchange_0base_to_1base()
        {
            string value = "123";
            string change_0base_to_1base = "124";
            var result = CLIPxp.change_0base_to_1base(value);
            Assert.IsNotNull(result);
            Assert.That(result, Is.EqualTo(change_0base_to_1base));
        }

        [Test]
        public void TestIsNumeric()
        {
            string str = "10";
            var result = (bool)PrivateObjectCLIPxp.Invoke("IsNumeric", str);
            Assert.IsNotNull(result);
            Assert.IsTrue(result);
        }

        [Test]
        public void TestIsHexNumeric()
        {
            string str = "14";
            var result = (bool)PrivateObjectCLIPxp.Invoke("IsHexNumeric", str);
            Assert.IsNotNull(result);
            Assert.IsTrue(result);
        }

        [Test]
        public void Testget_inputsource_type()
        {
            string index;
            string inputsource_type;
            string get_inputsource_type_Result;

            // VGA
            index = "VGA";
            inputsource_type = "VGA-1";
            get_inputsource_type_Result = (string)PrivateObjectCLIPxp.Invoke("get_inputsource_type", index);
            Assert.IsNotNull(get_inputsource_type_Result);
            Assert.That(inputsource_type, Is.EqualTo(get_inputsource_type_Result));

            index = "VGA1";
            inputsource_type = "VGA-1";
            get_inputsource_type_Result = (string)PrivateObjectCLIPxp.Invoke("get_inputsource_type", index);
            Assert.IsNotNull(get_inputsource_type_Result);
            Assert.That(inputsource_type, Is.EqualTo(get_inputsource_type_Result));

            index = "VGA-1";
            inputsource_type = "VGA-1";
            get_inputsource_type_Result = (string)PrivateObjectCLIPxp.Invoke("get_inputsource_type", index);
            Assert.IsNotNull(get_inputsource_type_Result);
            Assert.That(inputsource_type, Is.EqualTo(get_inputsource_type_Result));

            index = "VGA2";
            inputsource_type = "VGA-2";
            get_inputsource_type_Result = (string)PrivateObjectCLIPxp.Invoke("get_inputsource_type", index);
            Assert.IsNotNull(get_inputsource_type_Result);
            Assert.That(inputsource_type, Is.EqualTo(get_inputsource_type_Result));

            index = "VGA-2";
            inputsource_type = "VGA-2";
            get_inputsource_type_Result = (string)PrivateObjectCLIPxp.Invoke("get_inputsource_type", index);
            Assert.IsNotNull(get_inputsource_type_Result);
            Assert.That(inputsource_type, Is.EqualTo(get_inputsource_type_Result));

            // HDMI
            index = "HDMI";
            inputsource_type = "HDMI-1";
            get_inputsource_type_Result = (string)PrivateObjectCLIPxp.Invoke("get_inputsource_type", index);
            Assert.IsNotNull(get_inputsource_type_Result);
            Assert.That(inputsource_type, Is.EqualTo(get_inputsource_type_Result));

            index = "HDMI1";
            inputsource_type = "HDMI-1";
            get_inputsource_type_Result = (string)PrivateObjectCLIPxp.Invoke("get_inputsource_type", index);
            Assert.IsNotNull(get_inputsource_type_Result);
            Assert.That(inputsource_type, Is.EqualTo(get_inputsource_type_Result));

            index = "HDMI-1";
            inputsource_type = "HDMI-1";
            get_inputsource_type_Result = (string)PrivateObjectCLIPxp.Invoke("get_inputsource_type", index);
            Assert.IsNotNull(get_inputsource_type_Result);
            Assert.That(inputsource_type, Is.EqualTo(get_inputsource_type_Result));

            index = "HDMI2";
            inputsource_type = "HDMI-2";
            get_inputsource_type_Result = (string)PrivateObjectCLIPxp.Invoke("get_inputsource_type", index);
            Assert.IsNotNull(get_inputsource_type_Result);
            Assert.That(inputsource_type, Is.EqualTo(get_inputsource_type_Result));

            index = "HDMI-2";
            inputsource_type = "HDMI-2";
            get_inputsource_type_Result = (string)PrivateObjectCLIPxp.Invoke("get_inputsource_type", index);
            Assert.IsNotNull(get_inputsource_type_Result);
            Assert.That(inputsource_type, Is.EqualTo(get_inputsource_type_Result));

            // DP
            index = "DP";
            inputsource_type = "DISPLAYPORT-1";
            get_inputsource_type_Result = (string)PrivateObjectCLIPxp.Invoke("get_inputsource_type", index);
            Assert.IsNotNull(get_inputsource_type_Result);
            Assert.That(inputsource_type, Is.EqualTo(get_inputsource_type_Result));

            index = "DP1";
            inputsource_type = "DISPLAYPORT-1";
            get_inputsource_type_Result = (string)PrivateObjectCLIPxp.Invoke("get_inputsource_type", index);
            Assert.IsNotNull(get_inputsource_type_Result);
            Assert.That(inputsource_type, Is.EqualTo(get_inputsource_type_Result));

            index = "DP-1";
            inputsource_type = "DISPLAYPORT-1";
            get_inputsource_type_Result = (string)PrivateObjectCLIPxp.Invoke("get_inputsource_type", index);
            Assert.IsNotNull(get_inputsource_type_Result);
            Assert.That(inputsource_type, Is.EqualTo(get_inputsource_type_Result));

            index = "DISPLAYPORT";
            inputsource_type = "DISPLAYPORT-1";
            get_inputsource_type_Result = (string)PrivateObjectCLIPxp.Invoke("get_inputsource_type", index);
            Assert.IsNotNull(get_inputsource_type_Result);
            Assert.That(inputsource_type, Is.EqualTo(get_inputsource_type_Result));

            index = "DISPLAYPORT1";
            inputsource_type = "DISPLAYPORT-1";
            get_inputsource_type_Result = (string)PrivateObjectCLIPxp.Invoke("get_inputsource_type", index);
            Assert.IsNotNull(get_inputsource_type_Result);
            Assert.That(inputsource_type, Is.EqualTo(get_inputsource_type_Result));

            index = "DISPLAYPORT-1";
            inputsource_type = "DISPLAYPORT-1";
            get_inputsource_type_Result = (string)PrivateObjectCLIPxp.Invoke("get_inputsource_type", index);
            Assert.IsNotNull(get_inputsource_type_Result);
            Assert.That(inputsource_type, Is.EqualTo(get_inputsource_type_Result));

            index = "DP2";
            inputsource_type = "DISPLAYPORT-2";
            get_inputsource_type_Result = (string)PrivateObjectCLIPxp.Invoke("get_inputsource_type", index);
            Assert.IsNotNull(get_inputsource_type_Result);
            Assert.That(inputsource_type, Is.EqualTo(get_inputsource_type_Result));

            index = "DP-2";
            inputsource_type = "DISPLAYPORT-2";
            get_inputsource_type_Result = (string)PrivateObjectCLIPxp.Invoke("get_inputsource_type", index);
            Assert.IsNotNull(get_inputsource_type_Result);
            Assert.That(inputsource_type, Is.EqualTo(get_inputsource_type_Result));

            index = "DISPLAYPORT2";
            inputsource_type = "DISPLAYPORT-2";
            get_inputsource_type_Result = (string)PrivateObjectCLIPxp.Invoke("get_inputsource_type", index);
            Assert.IsNotNull(get_inputsource_type_Result);
            Assert.That(inputsource_type, Is.EqualTo(get_inputsource_type_Result));

            index = "DISPLAYPORT-2";
            inputsource_type = "DISPLAYPORT-2";
            get_inputsource_type_Result = (string)PrivateObjectCLIPxp.Invoke("get_inputsource_type", index);
            Assert.IsNotNull(get_inputsource_type_Result);
            Assert.That(inputsource_type, Is.EqualTo(get_inputsource_type_Result));

            // USBC
            index = "USBC";
            inputsource_type = "USB-C1";
            get_inputsource_type_Result = (string)PrivateObjectCLIPxp.Invoke("get_inputsource_type", index);
            Assert.IsNotNull(get_inputsource_type_Result);
            Assert.That(inputsource_type, Is.EqualTo(get_inputsource_type_Result));

            index = "USBC1";
            inputsource_type = "USB-C1";
            get_inputsource_type_Result = (string)PrivateObjectCLIPxp.Invoke("get_inputsource_type", index);
            Assert.IsNotNull(get_inputsource_type_Result);
            Assert.That(inputsource_type, Is.EqualTo(get_inputsource_type_Result));

            index = "USB-C";
            inputsource_type = "USB-C1";
            get_inputsource_type_Result = (string)PrivateObjectCLIPxp.Invoke("get_inputsource_type", index);
            Assert.IsNotNull(get_inputsource_type_Result);
            Assert.That(inputsource_type, Is.EqualTo(get_inputsource_type_Result));

            index = "USB-C1";
            inputsource_type = "USB-C1";
            get_inputsource_type_Result = (string)PrivateObjectCLIPxp.Invoke("get_inputsource_type", index);
            Assert.IsNotNull(get_inputsource_type_Result);
            Assert.That(inputsource_type, Is.EqualTo(get_inputsource_type_Result));

            index = "USBC2";
            inputsource_type = "USB-C2";
            get_inputsource_type_Result = (string)PrivateObjectCLIPxp.Invoke("get_inputsource_type", index);
            Assert.IsNotNull(get_inputsource_type_Result);
            Assert.That(inputsource_type, Is.EqualTo(get_inputsource_type_Result));

            index = "USB-C2";
            inputsource_type = "USB-C2";
            get_inputsource_type_Result = (string)PrivateObjectCLIPxp.Invoke("get_inputsource_type", index);
            Assert.IsNotNull(get_inputsource_type_Result);
            Assert.That(inputsource_type, Is.EqualTo(get_inputsource_type_Result));

            // TBT
            index = "TBT";
            inputsource_type = "Thunderbolt-1";
            get_inputsource_type_Result = (string)PrivateObjectCLIPxp.Invoke("get_inputsource_type", index);
            Assert.IsNotNull(get_inputsource_type_Result);
            Assert.That(inputsource_type, Is.EqualTo(get_inputsource_type_Result));

            index = "TBT1";
            inputsource_type = "Thunderbolt-1";
            get_inputsource_type_Result = (string)PrivateObjectCLIPxp.Invoke("get_inputsource_type", index);
            Assert.IsNotNull(get_inputsource_type_Result);
            Assert.That(inputsource_type, Is.EqualTo(get_inputsource_type_Result));

            index = "THUNDERBOLT";
            inputsource_type = "Thunderbolt-1";
            get_inputsource_type_Result = (string)PrivateObjectCLIPxp.Invoke("get_inputsource_type", index);
            Assert.IsNotNull(get_inputsource_type_Result);
            Assert.That(inputsource_type, Is.EqualTo(get_inputsource_type_Result));

            index = "THUNDERBOLT1";
            inputsource_type = "Thunderbolt-1";
            get_inputsource_type_Result = (string)PrivateObjectCLIPxp.Invoke("get_inputsource_type", index);
            Assert.IsNotNull(get_inputsource_type_Result);
            Assert.That(inputsource_type, Is.EqualTo(get_inputsource_type_Result));

            index = "THUNDERBOLT-1";
            inputsource_type = "Thunderbolt-1";
            get_inputsource_type_Result = (string)PrivateObjectCLIPxp.Invoke("get_inputsource_type", index);
            Assert.IsNotNull(get_inputsource_type_Result);
            Assert.That(inputsource_type, Is.EqualTo(get_inputsource_type_Result));

            index = "TBT2";
            inputsource_type = "Thunderbolt-2";
            get_inputsource_type_Result = (string)PrivateObjectCLIPxp.Invoke("get_inputsource_type", index);
            Assert.IsNotNull(get_inputsource_type_Result);
            Assert.That(inputsource_type, Is.EqualTo(get_inputsource_type_Result));

            index = "THUNDERBOLT2";
            inputsource_type = "Thunderbolt-2";
            get_inputsource_type_Result = (string)PrivateObjectCLIPxp.Invoke("get_inputsource_type", index);
            Assert.IsNotNull(get_inputsource_type_Result);
            Assert.That(inputsource_type, Is.EqualTo(get_inputsource_type_Result));

            index = "THUNDERBOLT-2";
            inputsource_type = "Thunderbolt-2";
            get_inputsource_type_Result = (string)PrivateObjectCLIPxp.Invoke("get_inputsource_type", index);
            Assert.IsNotNull(get_inputsource_type_Result);
            Assert.That(inputsource_type, Is.EqualTo(get_inputsource_type_Result));

            // default
            index = "SomeUnknownValue";
            inputsource_type = "Unknown";
            get_inputsource_type_Result = (string)PrivateObjectCLIPxp.Invoke("get_inputsource_type", index);
            Assert.IsNotNull(get_inputsource_type_Result);
            Assert.That(inputsource_type, Is.EqualTo(get_inputsource_type_Result));
        }

        [Test]
        public void Testget_inputsource_vcp()
        {
            string index;
            int get_inputsource_vcp;
            int get_inputsource_vcp_Result;

            // HDMI-1
            index = "HDMI-1";
            get_inputsource_vcp = 0x11;
            get_inputsource_vcp_Result = (int)PrivateObjectCLIPxp.Invoke("get_inputsource_vcp", index);
            Assert.IsNotNull(get_inputsource_vcp_Result);
            Assert.That(get_inputsource_vcp, Is.EqualTo(get_inputsource_vcp_Result));

            // HDMI-2
            index = "HDMI-2";
            get_inputsource_vcp = 0x12;
            get_inputsource_vcp_Result = (int)PrivateObjectCLIPxp.Invoke("get_inputsource_vcp", index);
            Assert.IsNotNull(get_inputsource_vcp_Result);
            Assert.That(get_inputsource_vcp, Is.EqualTo(get_inputsource_vcp_Result));

            // DISPLAYPORT-1
            index = "DISPLAYPORT-1";
            get_inputsource_vcp = 0x0f;
            get_inputsource_vcp_Result = (int)PrivateObjectCLIPxp.Invoke("get_inputsource_vcp", index);
            Assert.IsNotNull(get_inputsource_vcp_Result);
            Assert.That(get_inputsource_vcp, Is.EqualTo(get_inputsource_vcp_Result));

            // DISPLAYPORT-2
            index = "DISPLAYPORT-2";
            get_inputsource_vcp = 0x13;
            get_inputsource_vcp_Result = (int)PrivateObjectCLIPxp.Invoke("get_inputsource_vcp", index);
            Assert.IsNotNull(get_inputsource_vcp_Result);
            Assert.That(get_inputsource_vcp, Is.EqualTo(get_inputsource_vcp_Result));

            // USB-C1
            index = "USB-C1";
            get_inputsource_vcp = 0x1b;
            get_inputsource_vcp_Result = (int)PrivateObjectCLIPxp.Invoke("get_inputsource_vcp", index);
            Assert.IsNotNull(get_inputsource_vcp_Result);
            Assert.That(get_inputsource_vcp, Is.EqualTo(get_inputsource_vcp_Result));

            // USB-C2
            index = "USB-C2";
            get_inputsource_vcp = 0x1c;
            get_inputsource_vcp_Result = (int)PrivateObjectCLIPxp.Invoke("get_inputsource_vcp", index);
            Assert.IsNotNull(get_inputsource_vcp_Result);
            Assert.That(get_inputsource_vcp, Is.EqualTo(get_inputsource_vcp_Result));

            // Thunderbolt-1
            index = "Thunderbolt-1";
            get_inputsource_vcp = 0x19;
            get_inputsource_vcp_Result = (int)PrivateObjectCLIPxp.Invoke("get_inputsource_vcp", index);
            Assert.IsNotNull(get_inputsource_vcp_Result);
            Assert.That(get_inputsource_vcp, Is.EqualTo(get_inputsource_vcp_Result));

            // Thunderbolt-2
            index = "Thunderbolt-2";
            get_inputsource_vcp = 0x1a;
            get_inputsource_vcp_Result = (int)PrivateObjectCLIPxp.Invoke("get_inputsource_vcp", index);
            Assert.IsNotNull(get_inputsource_vcp_Result);
            Assert.That(get_inputsource_vcp, Is.EqualTo(get_inputsource_vcp_Result));

            // Default
            index = "SomeUnknownValue";
            get_inputsource_vcp = 0;
            get_inputsource_vcp_Result = (int)PrivateObjectCLIPxp.Invoke("get_inputsource_vcp", index);
            Assert.IsNotNull(get_inputsource_vcp_Result);
            Assert.That(get_inputsource_vcp, Is.EqualTo(get_inputsource_vcp_Result));
        }

        [Test]
        public void Testget_InputSource_code()
        {
            string index;
            string get_InputSource_code;
            string get_InputSource_code_Result;

            // VGA-1
            index = "VGA-1";
            get_InputSource_code = "0x01";
            get_InputSource_code_Result = (string)PrivateObjectCLIPxp.Invoke("get_InputSource_code", index);
            Assert.IsNotNull(get_InputSource_code_Result);
            Assert.That(get_InputSource_code, Is.EqualTo(get_InputSource_code_Result));

            // VGA-2
            index = "VGA-2";
            get_InputSource_code = "0x02";
            get_InputSource_code_Result = (string)PrivateObjectCLIPxp.Invoke("get_InputSource_code", index);
            Assert.IsNotNull(get_InputSource_code_Result);
            Assert.That(get_InputSource_code, Is.EqualTo(get_InputSource_code_Result));

            // DVI-1
            index = "DVI-1";
            get_InputSource_code = "0x03";
            get_InputSource_code_Result = (string)PrivateObjectCLIPxp.Invoke("get_InputSource_code", index);
            Assert.IsNotNull(get_InputSource_code_Result);
            Assert.That(get_InputSource_code, Is.EqualTo(get_InputSource_code_Result));

            // DVI-2
            index = "DVI-2";
            get_InputSource_code = "0x04";
            get_InputSource_code_Result = (string)PrivateObjectCLIPxp.Invoke("get_InputSource_code", index);
            Assert.IsNotNull(get_InputSource_code_Result);
            Assert.That(get_InputSource_code, Is.EqualTo(get_InputSource_code_Result));

            // Composite video 1
            index = "Composite video 1";
            get_InputSource_code = "0x05";
            get_InputSource_code_Result = (string)PrivateObjectCLIPxp.Invoke("get_InputSource_code", index);
            Assert.IsNotNull(get_InputSource_code_Result);
            Assert.That(get_InputSource_code, Is.EqualTo(get_InputSource_code_Result));

            // Composite video 2
            index = "Composite video 2";
            get_InputSource_code = "0x06";
            get_InputSource_code_Result = (string)PrivateObjectCLIPxp.Invoke("get_InputSource_code", index);
            Assert.IsNotNull(get_InputSource_code_Result);
            Assert.That(get_InputSource_code, Is.EqualTo(get_InputSource_code_Result));

            // S-Video-1
            index = "S-Video-1";
            get_InputSource_code = "0x07";
            get_InputSource_code_Result = (string)PrivateObjectCLIPxp.Invoke("get_InputSource_code", index);
            Assert.IsNotNull(get_InputSource_code_Result);
            Assert.That(get_InputSource_code, Is.EqualTo(get_InputSource_code_Result));

            // S-Video-2
            index = "S-Video-2";
            get_InputSource_code = "0x08";
            get_InputSource_code_Result = (string)PrivateObjectCLIPxp.Invoke("get_InputSource_code", index);
            Assert.IsNotNull(get_InputSource_code_Result);
            Assert.That(get_InputSource_code, Is.EqualTo(get_InputSource_code_Result));

            // Tuner-1
            index = "Tuner-1";
            get_InputSource_code = "0x09";
            get_InputSource_code_Result = (string)PrivateObjectCLIPxp.Invoke("get_InputSource_code", index);
            Assert.IsNotNull(get_InputSource_code_Result);
            Assert.That(get_InputSource_code, Is.EqualTo(get_InputSource_code_Result));

            // Tuner-2
            index = "Tuner-2";
            get_InputSource_code = "0x0a";
            get_InputSource_code_Result = (string)PrivateObjectCLIPxp.Invoke("get_InputSource_code", index);
            Assert.IsNotNull(get_InputSource_code_Result);
            Assert.That(get_InputSource_code, Is.EqualTo(get_InputSource_code_Result));

            // Tuner-3
            index = "Tuner-3";
            get_InputSource_code = "0x0b";
            get_InputSource_code_Result = (string)PrivateObjectCLIPxp.Invoke("get_InputSource_code", index);
            Assert.IsNotNull(get_InputSource_code_Result);
            Assert.That(get_InputSource_code, Is.EqualTo(get_InputSource_code_Result));

            // Component video (YPrPb/YCrCb) 1
            index = "Component video (YPrPb/YCrCb) 1";
            get_InputSource_code = "0x0c";
            get_InputSource_code_Result = (string)PrivateObjectCLIPxp.Invoke("get_InputSource_code", index);
            Assert.IsNotNull(get_InputSource_code_Result);
            Assert.That(get_InputSource_code, Is.EqualTo(get_InputSource_code_Result));

            // Component video (YPrPb/YCrCb) 2
            index = "Component video (YPrPb/YCrCb) 2";
            get_InputSource_code = "0x0d";
            get_InputSource_code_Result = (string)PrivateObjectCLIPxp.Invoke("get_InputSource_code", index);
            Assert.IsNotNull(get_InputSource_code_Result);
            Assert.That(get_InputSource_code, Is.EqualTo(get_InputSource_code_Result));

            // Component video (YPrPb/YCrCb) 3
            index = "Component video (YPrPb/YCrCb) 3";
            get_InputSource_code = "0x0e";
            get_InputSource_code_Result = (string)PrivateObjectCLIPxp.Invoke("get_InputSource_code", index);
            Assert.IsNotNull(get_InputSource_code_Result);
            Assert.That(get_InputSource_code, Is.EqualTo(get_InputSource_code_Result));

            // DISPLAYPORT-1
            index = "DISPLAYPORT-1";
            get_InputSource_code = "0x0f";
            get_InputSource_code_Result = (string)PrivateObjectCLIPxp.Invoke("get_InputSource_code", index);
            Assert.IsNotNull(get_InputSource_code_Result);
            Assert.That(get_InputSource_code, Is.EqualTo(get_InputSource_code_Result));

            // Mini DisplayPort-1
            index = "Mini DisplayPort-1";
            get_InputSource_code = "0x10";
            get_InputSource_code_Result = (string)PrivateObjectCLIPxp.Invoke("get_InputSource_code", index);
            Assert.IsNotNull(get_InputSource_code_Result);
            Assert.That(get_InputSource_code, Is.EqualTo(get_InputSource_code_Result));

            // HDMI-1
            index = "HDMI-1";
            get_InputSource_code = "0x11";
            get_InputSource_code_Result = (string)PrivateObjectCLIPxp.Invoke("get_InputSource_code", index);
            Assert.IsNotNull(get_InputSource_code_Result);
            Assert.That(get_InputSource_code, Is.EqualTo(get_InputSource_code_Result));

            // HDMI-2
            index = "HDMI-2";
            get_InputSource_code = "0x12";
            get_InputSource_code_Result = (string)PrivateObjectCLIPxp.Invoke("get_InputSource_code", index);
            Assert.IsNotNull(get_InputSource_code_Result);
            Assert.That(get_InputSource_code, Is.EqualTo(get_InputSource_code_Result));

            // DISPLAYPORT-2
            index = "DISPLAYPORT-2";
            get_InputSource_code = "0x13";
            get_InputSource_code_Result = (string)PrivateObjectCLIPxp.Invoke("get_InputSource_code", index);
            Assert.IsNotNull(get_InputSource_code_Result);
            Assert.That(get_InputSource_code, Is.EqualTo(get_InputSource_code_Result));

            // Mini DisplayPort-2
            index = "Mini DisplayPort-2";
            get_InputSource_code = "0x14";
            get_InputSource_code_Result = (string)PrivateObjectCLIPxp.Invoke("get_InputSource_code", index);
            Assert.IsNotNull(get_InputSource_code_Result);
            Assert.That(get_InputSource_code, Is.EqualTo(get_InputSource_code_Result));

            // HDMI3
            index = "HDMI3";
            get_InputSource_code = "0x15";
            get_InputSource_code_Result = (string)PrivateObjectCLIPxp.Invoke("get_InputSource_code", index);
            Assert.IsNotNull(get_InputSource_code_Result);
            Assert.That(get_InputSource_code, Is.EqualTo(get_InputSource_code_Result));

            // HDMI4
            index = "HDMI4";
            get_InputSource_code = "0x16";
            get_InputSource_code_Result = (string)PrivateObjectCLIPxp.Invoke("get_InputSource_code", index);
            Assert.IsNotNull(get_InputSource_code_Result);
            Assert.That(get_InputSource_code, Is.EqualTo(get_InputSource_code_Result));

            // DISPLAYPORT-3
            index = "DISPLAYPORT-3";
            get_InputSource_code = "0x17";
            get_InputSource_code_Result = (string)PrivateObjectCLIPxp.Invoke("get_InputSource_code", index);
            Assert.IsNotNull(get_InputSource_code_Result);
            Assert.That(get_InputSource_code, Is.EqualTo(get_InputSource_code_Result));

            // Mini DisplayPort-3
            index = "Mini DisplayPort-3";
            get_InputSource_code = "0x18";
            get_InputSource_code_Result = (string)PrivateObjectCLIPxp.Invoke("get_InputSource_code", index);
            Assert.IsNotNull(get_InputSource_code_Result);
            Assert.That(get_InputSource_code, Is.EqualTo(get_InputSource_code_Result));

            // Thunderbolt-1
            index = "Thunderbolt-1";
            get_InputSource_code = "0x19";
            get_InputSource_code_Result = (string)PrivateObjectCLIPxp.Invoke("get_InputSource_code", index);
            Assert.IsNotNull(get_InputSource_code_Result);
            Assert.That(get_InputSource_code, Is.EqualTo(get_InputSource_code_Result));

            // Thunderbolt-2
            index = "Thunderbolt-2";
            get_InputSource_code = "0x1a";
            get_InputSource_code_Result = (string)PrivateObjectCLIPxp.Invoke("get_InputSource_code", index);
            Assert.IsNotNull(get_InputSource_code_Result);
            Assert.That(get_InputSource_code, Is.EqualTo(get_InputSource_code_Result));

            // USB-C1
            index = "USB-C1";
            get_InputSource_code = "0x1b";
            get_InputSource_code_Result = (string)PrivateObjectCLIPxp.Invoke("get_InputSource_code", index);
            Assert.IsNotNull(get_InputSource_code_Result);
            Assert.That(get_InputSource_code, Is.EqualTo(get_InputSource_code_Result));

            // USB-C2
            index = "USB-C2";
            get_InputSource_code = "0x1c";
            get_InputSource_code_Result = (string)PrivateObjectCLIPxp.Invoke("get_InputSource_code", index);
            Assert.IsNotNull(get_InputSource_code_Result);
            Assert.That(get_InputSource_code, Is.EqualTo(get_InputSource_code_Result));

            // USB-C3
            index = "USB-C3";
            get_InputSource_code = "0x1d";
            get_InputSource_code_Result = (string)PrivateObjectCLIPxp.Invoke("get_InputSource_code", index);
            Assert.IsNotNull(get_InputSource_code_Result);
            Assert.That(get_InputSource_code, Is.EqualTo(get_InputSource_code_Result));

            // USB-C4
            index = "USB-C4";
            get_InputSource_code = "0x1e";
            get_InputSource_code_Result = (string)PrivateObjectCLIPxp.Invoke("get_InputSource_code", index);
            Assert.IsNotNull(get_InputSource_code_Result);
            Assert.That(get_InputSource_code, Is.EqualTo(get_InputSource_code_Result));

            // USB Comm from USB1 (Type-B, port 1)
            index = "USB Comm from USB1 (Type-B, port 1)";
            get_InputSource_code = "0x80";
            get_InputSource_code_Result = (string)PrivateObjectCLIPxp.Invoke("get_InputSource_code", index);
            Assert.IsNotNull(get_InputSource_code_Result);
            Assert.That(get_InputSource_code, Is.EqualTo(get_InputSource_code_Result));

            // USB Comm from USB2 (Type-B, port 2)
            index = "USB Comm from USB2 (Type-B, port 2)";
            get_InputSource_code = "0x81";
            get_InputSource_code_Result = (string)PrivateObjectCLIPxp.Invoke("get_InputSource_code", index);
            Assert.IsNotNull(get_InputSource_code_Result);
            Assert.That(get_InputSource_code, Is.EqualTo(get_InputSource_code_Result));

            // USB Comm from USB-C1 (Type-C, port 1)
            index = "USB Comm from USB-C1 (Type-C, port 1)";
            get_InputSource_code = "0x82";
            get_InputSource_code_Result = (string)PrivateObjectCLIPxp.Invoke("get_InputSource_code", index);
            Assert.IsNotNull(get_InputSource_code_Result);
            Assert.That(get_InputSource_code, Is.EqualTo(get_InputSource_code_Result));

            // USB Comm from USB-C2 (Type-C, port 2)
            index = "USB Comm from USB-C2 (Type-C, port 2)";
            get_InputSource_code = "0x83";
            get_InputSource_code_Result = (string)PrivateObjectCLIPxp.Invoke("get_InputSource_code", index);
            Assert.IsNotNull(get_InputSource_code_Result);
            Assert.That(get_InputSource_code, Is.EqualTo(get_InputSource_code_Result));

            // USB Comm from USB-C3 (Type-C, port 3)
            index = "USB Comm from USB-C3 (Type-C, port 3)";
            get_InputSource_code = "0x84";
            get_InputSource_code_Result = (string)PrivateObjectCLIPxp.Invoke("get_InputSource_code", index);
            Assert.IsNotNull(get_InputSource_code_Result);
            Assert.That(get_InputSource_code, Is.EqualTo(get_InputSource_code_Result));

            // USB Comm from USB-C4 (Type-C, port 4)
            index = "USB Comm from USB-C4 (Type-C, port 4)";
            get_InputSource_code = "0x85";
            get_InputSource_code_Result = (string)PrivateObjectCLIPxp.Invoke("get_InputSource_code", index);
            Assert.IsNotNull(get_InputSource_code_Result);
            Assert.That(get_InputSource_code, Is.EqualTo(get_InputSource_code_Result));

            // default
            index = "SomeUnknownValue";
            get_InputSource_code = "0x11";
            get_InputSource_code_Result = (string)PrivateObjectCLIPxp.Invoke("get_InputSource_code", index);
            Assert.IsNotNull(get_InputSource_code_Result);
            Assert.That(get_InputSource_code, Is.EqualTo(get_InputSource_code_Result));
        }
    }
}