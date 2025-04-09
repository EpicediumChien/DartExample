using DDPM.SA.Common;
using DDPM.SA.Common.Display;
using DDPM.SA.Common.Settings;
using Dell.Client.Framework.Interfaces;
using Dell.Client.Framework.UnitTestShared.Tests;
using Moq;
using VcpCore.Common;
using static DDPM.SA.Common.ICLICommandTable;
using IDs = DDPM.SA.Common.IDs;

namespace DDPM.CLI.Plugins.Display.Test
{
    public class TestCLIDisplayPlugins
    {
        private Mock<IAgent> DisplayMangerAgent { get; } = new();
        private Mock<IAgent> VcpCoreAgent { get; } = new();
        private Mock<IAgent> CLIDisplayPluginsAgent { get; } = new();

        private MonitorInfo monitorInfo = new MonitorInfo();
        private Mock<IDeviceManagerSA> DeviceManagerSAService { get; } = new();
        private IDeviceManagerSA? _deviceManagerPlugin;

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

        private CLIDisplayPlugins CreateCLIDisplayPlugins()
        {
            CLIDisplayPluginsAgent.Setup(x => x.PluginManager.FindPluginByGuid(Guid.Parse(IDs.CLI_Plugin_Display)));

            return new CLIDisplayPlugins(CLIDisplayPluginsAgent.Object);
        }

        private CLIDisplayPlugins cLIDisplayPlugins;
        private PrivateObject privatetecLIDisplayPlugins;

        [OneTimeSetUp]
        public void Setup()
        {
            cLIDisplayPlugins = CreateCLIDisplayPlugins();
            privatetecLIDisplayPlugins = new PrivateObject(cLIDisplayPlugins);
        }

        [Test]
        public void TestCLIDisplayPlugin()
        {
            Assert.IsNotNull(cLIDisplayPlugins);
            PrivateObject privatetecLIDisplayPlugins = new PrivateObject(cLIDisplayPlugins);
            var agent2 = privatetecLIDisplayPlugins.GetField("_agent") as IAgent;
            Assert.That(agent2, Is.EqualTo(CLIDisplayPluginsAgent.Object));
        }

        [Test]
        public void TestCLIDisplayPluginsLogId()
        {
            Assert.IsNotNull(cLIDisplayPlugins);
            string pluginLogId = "CLIDisplay";
            var CLIProxyPluginLogId_result = CLIDisplayPlugins.PluginLogId;
            Assert.That(pluginLogId, Is.EqualTo(CLIProxyPluginLogId_result));
        }

        [Test]
        public void TestIsDisposed()
        {
            var Result = cLIDisplayPlugins.IsDisposed;
            Assert.IsFalse(Result);
            PrivateObject privatetecLIDisplayPlugins = new PrivateObject(cLIDisplayPlugins);
            privatetecLIDisplayPlugins.SetFieldOrProperty("IsDisposed", true);
            var Result2 = cLIDisplayPlugins.IsDisposed;
            Assert.IsTrue(Result2);
        }

        [Test]
        public void TestFirstCharSubstring()
        {
            string input1 = "";
            var FirstCharSubstring_Result1 = cLIDisplayPlugins.FirstCharSubstring(input1);  //input is null
            Assert.That(input1, Is.EqualTo(FirstCharSubstring_Result1));

            string input2 = "Test FirstCharSubstring";
            var FirstCharSubstring_Result2 = cLIDisplayPlugins.FirstCharSubstring(input2);  //input is not null
            Assert.That(input2, Is.EqualTo(FirstCharSubstring_Result2));
        }

        [Test]
        public void TestConvertPxpInputsFromString()
        {
            string str1 = "-1";
            ePxpInputs ret = new ePxpInputs();
            ret = ePxpInputs.invalid;
            var ConvertPxpInputsFromString_Result1 = CLIDisplayPlugins.ConvertPxpInputsFromString(str1);  //str1 is invalid
            Assert.That(ret, Is.EqualTo(ConvertPxpInputsFromString_Result1));

            string str2 = "1";
            ret = ePxpInputs.sub1;
            var ConvertPxpInputsFromString_Result2 = CLIDisplayPlugins.ConvertPxpInputsFromString(str2);  //str2 is sub1
            Assert.That(ret, Is.EqualTo(ConvertPxpInputsFromString_Result2));
        }

        [Test]
        public void TestIsHexNumeric()
        {
            string maybeHex1 = "0";
            bool isHexNumeric = true;
            var IsHexNumeric_Result1 = privatetecLIDisplayPlugins.Invoke("IsHexNumeric", maybeHex1);
            Assert.That(isHexNumeric, Is.EqualTo(IsHexNumeric_Result1));

            string maybeHex2 = "2";
            var IsHexNumeric_Result2 = privatetecLIDisplayPlugins.Invoke("IsHexNumeric", maybeHex2);
            Assert.That(isHexNumeric, Is.EqualTo(IsHexNumeric_Result2));
        }

        [Test]
        public void TestIsNumeric()
        {
            string str1 = "0";
            bool IsNumeric = true;
            var IsNumeric_Result1 = privatetecLIDisplayPlugins.Invoke("IsNumeric", str1);
            Assert.That(IsNumeric, Is.EqualTo(IsNumeric_Result1));

            string maybeHex2 = "4";
            var IsNumeric_Result2 = privatetecLIDisplayPlugins.Invoke("IsNumeric", maybeHex2);
            Assert.That(IsNumeric, Is.EqualTo(IsNumeric_Result2));
        }

        [Test]
        public void TestProcessListMonitorsOption()
        {
            IDeviceManagerSA? devMgr;
            devMgr = null;
            int device_manager = 1;
            if (devMgr == null)
            {
                var ProcessListMonitorsOptionAsync_Result1 = (Task<int>)privatetecLIDisplayPlugins.Invoke("ProcessListMonitorsOptionAsync", devMgr, false);  //devMgr == null
                Assert.IsNotNull(ProcessListMonitorsOptionAsync_Result1);
                Assert.That(device_manager, Is.EqualTo(ProcessListMonitorsOptionAsync_Result1.Result));
            }

            int device_manager2 = 0;
            Mock<IDeviceManagerSA> devMgr2 = new Mock<IDeviceManagerSA>();
            List<MonitorInfo> _allInfoMonitors = new List<MonitorInfo>();
            _allInfoMonitors.Add(monitorInfo1);
            devMgr2.Setup(m => m.GetMonitors()).Returns(Task.FromResult(_allInfoMonitors));
            if (devMgr2 != null)
            {
                var ProcessListMonitorsOptionAsync_Result2 = (Task<int>)privatetecLIDisplayPlugins.Invoke("ProcessListMonitorsOptionAsync", devMgr2.Object, false);  //devMgr2 != null
                Assert.IsNotNull(ProcessListMonitorsOptionAsync_Result2);
                Assert.That(device_manager2, Is.EqualTo(ProcessListMonitorsOptionAsync_Result2.Result));
            }
        }

        [Test]
        public void TestGetCapabilitiesString()
        {
            IDeviceManagerSA? devMgr;
            devMgr = null;
            string index = "0";
            int device_manager = 1;
            if (devMgr == null)
            {
                var GetCapabilitiesString_Result1 = (Task<int>)privatetecLIDisplayPlugins.Invoke("GetCapabilitiesString", devMgr, index);  //devMgr == null
                Assert.IsNotNull(GetCapabilitiesString_Result1);
                Assert.That(device_manager, Is.EqualTo(GetCapabilitiesString_Result1.Result));
            }

            int device_manager2 = 0;
            Mock<IDeviceManagerSA> devMgr2 = new Mock<IDeviceManagerSA>();
            List<MonitorInfo> _allInfoMonitors = new List<MonitorInfo>();
            _allInfoMonitors.Add(monitorInfo1);
            devMgr2.Setup(m => m.GetMonitors()).Returns(Task.FromResult(_allInfoMonitors));
            devMgr2.Setup(m => m.GetCapabilitiesString(It.IsAny<MonitorInfo>(), It.IsAny<Guid>(), It.IsAny<Priority>())).Returns(Task.FromResult(monitorInfo1.CapabilityString));
            if (devMgr2 != null)
            {
                var GetCapabilitiesString_Result2 = (Task<int>)privatetecLIDisplayPlugins.Invoke("GetCapabilitiesString", devMgr2.Object, index);   //devMgr2 != null
                Assert.IsNotNull(GetCapabilitiesString_Result2);
                Assert.That(device_manager2, Is.EqualTo(GetCapabilitiesString_Result2.Result));
            }
        }

        [Test]
        public void TestGetVCPCapabilities()
        {
            IDeviceManagerSA? devMgr;
            devMgr = null;
            string index = "0";
            int device_manager = 1;
            if (devMgr == null)
            {
                var GetVCPCapabilities_Result1 = (Task<int>)privatetecLIDisplayPlugins.Invoke("GetVCPCapabilities", devMgr, index); //devMgr == null
                Assert.IsNotNull(GetVCPCapabilities_Result1);
                Assert.That(device_manager, Is.EqualTo(GetVCPCapabilities_Result1.Result));
            }

            int device_manager2 = 0;
            Mock<IDeviceManagerSA> devMgr2 = new Mock<IDeviceManagerSA>();
            List<MonitorInfo> _allInfoMonitors = new List<MonitorInfo>();
            _allInfoMonitors.Add(monitorInfo1);
            devMgr2.Setup(m => m.GetMonitors()).Returns(Task.FromResult(_allInfoMonitors));
            devMgr2.Setup(m => m.GetVCPCapabilities(It.IsAny<MonitorInfo>(), It.IsAny<Guid>(), It.IsAny<Priority>())).Returns(Task.FromResult(monitorInfo1.CapabilityString));
            if (devMgr2 != null)
            {
                var GetVCPCapabilities_Result2 = (Task<int>)privatetecLIDisplayPlugins.Invoke("GetVCPCapabilities", devMgr2.Object, index);  //devMgr2 != null
                Assert.IsNotNull(GetVCPCapabilities_Result2);
                Assert.That(device_manager2, Is.EqualTo(GetVCPCapabilities_Result2.Result));
            }
        }

        [Test]
        public void Testget_inputsource_type()
        {
            string index;
            string inputsource_type;
            string get_inputsource_type_Result;

            //VGA input
            index = "VGA";
            inputsource_type = "VGA1";
            get_inputsource_type_Result = (string)privatetecLIDisplayPlugins.Invoke("get_inputsource_type", index);
            Assert.IsNotNull(get_inputsource_type_Result);
            Assert.That(inputsource_type, Is.EqualTo(get_inputsource_type_Result));

            index = "VGA1";
            inputsource_type = "VGA1";
            get_inputsource_type_Result = (string)privatetecLIDisplayPlugins.Invoke("get_inputsource_type", index);
            Assert.IsNotNull(get_inputsource_type_Result);
            Assert.That(inputsource_type, Is.EqualTo(get_inputsource_type_Result));

            index = "VGA-1";
            inputsource_type = "VGA1";
            get_inputsource_type_Result = (string)privatetecLIDisplayPlugins.Invoke("get_inputsource_type", index);
            Assert.IsNotNull(get_inputsource_type_Result);
            Assert.That(inputsource_type, Is.EqualTo(get_inputsource_type_Result));

            // VGA2 input
            index = "VGA2";
            inputsource_type = "VGA2";
            get_inputsource_type_Result = (string)privatetecLIDisplayPlugins.Invoke("get_inputsource_type", index);
            Assert.IsNotNull(get_inputsource_type_Result);
            Assert.That(inputsource_type, Is.EqualTo(get_inputsource_type_Result));

            index = "VGA-2";
            inputsource_type = "VGA2";
            get_inputsource_type_Result = (string)privatetecLIDisplayPlugins.Invoke("get_inputsource_type", index);
            Assert.IsNotNull(get_inputsource_type_Result);
            Assert.That(inputsource_type, Is.EqualTo(get_inputsource_type_Result));

            // HDMI input
            index = "HDMI";
            inputsource_type = "HDMI1";
            get_inputsource_type_Result = (string)privatetecLIDisplayPlugins.Invoke("get_inputsource_type", index);
            Assert.IsNotNull(get_inputsource_type_Result);
            Assert.That(inputsource_type, Is.EqualTo(get_inputsource_type_Result));

            index = "HDMI1";
            inputsource_type = "HDMI1";
            get_inputsource_type_Result = (string)privatetecLIDisplayPlugins.Invoke("get_inputsource_type", index);
            Assert.IsNotNull(get_inputsource_type_Result);
            Assert.That(inputsource_type, Is.EqualTo(get_inputsource_type_Result));

            index = "HDMI-1";
            inputsource_type = "HDMI1";
            get_inputsource_type_Result = (string)privatetecLIDisplayPlugins.Invoke("get_inputsource_type", index);
            Assert.IsNotNull(get_inputsource_type_Result);
            Assert.That(inputsource_type, Is.EqualTo(get_inputsource_type_Result));

            // HDMI2 input
            index = "HDMI2";
            inputsource_type = "HDMI2";
            get_inputsource_type_Result = (string)privatetecLIDisplayPlugins.Invoke("get_inputsource_type", index);
            Assert.IsNotNull(get_inputsource_type_Result);
            Assert.That(inputsource_type, Is.EqualTo(get_inputsource_type_Result));

            index = "HDMI-2";
            inputsource_type = "HDMI2";
            get_inputsource_type_Result = (string)privatetecLIDisplayPlugins.Invoke("get_inputsource_type", index);
            Assert.IsNotNull(get_inputsource_type_Result);
            Assert.That(inputsource_type, Is.EqualTo(get_inputsource_type_Result));

            // DP input
            index = "DP";
            inputsource_type = "DISPLAYPORT1";
            get_inputsource_type_Result = (string)privatetecLIDisplayPlugins.Invoke("get_inputsource_type", index);
            Assert.IsNotNull(get_inputsource_type_Result);
            Assert.That(inputsource_type, Is.EqualTo(get_inputsource_type_Result));

            index = "DP1";
            inputsource_type = "DISPLAYPORT1";
            get_inputsource_type_Result = (string)privatetecLIDisplayPlugins.Invoke("get_inputsource_type", index);
            Assert.IsNotNull(get_inputsource_type_Result);
            Assert.That(inputsource_type, Is.EqualTo(get_inputsource_type_Result));

            index = "DP-1";
            inputsource_type = "DISPLAYPORT1";
            get_inputsource_type_Result = (string)privatetecLIDisplayPlugins.Invoke("get_inputsource_type", index);
            Assert.IsNotNull(get_inputsource_type_Result);
            Assert.That(inputsource_type, Is.EqualTo(get_inputsource_type_Result));

            index = "DISPLAYPORT";
            inputsource_type = "DISPLAYPORT1";
            get_inputsource_type_Result = (string)privatetecLIDisplayPlugins.Invoke("get_inputsource_type", index);
            Assert.IsNotNull(get_inputsource_type_Result);
            Assert.That(inputsource_type, Is.EqualTo(get_inputsource_type_Result));

            index = "DISPLAYPORT1";
            inputsource_type = "DISPLAYPORT1";
            get_inputsource_type_Result = (string)privatetecLIDisplayPlugins.Invoke("get_inputsource_type", index);
            Assert.IsNotNull(get_inputsource_type_Result);
            Assert.That(inputsource_type, Is.EqualTo(get_inputsource_type_Result));

            index = "DISPLAYPORT-1";
            inputsource_type = "DISPLAYPORT1";
            get_inputsource_type_Result = (string)privatetecLIDisplayPlugins.Invoke("get_inputsource_type", index);
            Assert.IsNotNull(get_inputsource_type_Result);
            Assert.That(inputsource_type, Is.EqualTo(get_inputsource_type_Result));

            // 测试DP2相关输入源类型
            index = "DP2";
            inputsource_type = "DISPLAYPORT2";
            get_inputsource_type_Result = (string)privatetecLIDisplayPlugins.Invoke("get_inputsource_type", index);
            Assert.IsNotNull(get_inputsource_type_Result);
            Assert.That(inputsource_type, Is.EqualTo(get_inputsource_type_Result));

            index = "DP-2";
            inputsource_type = "DISPLAYPORT2";
            get_inputsource_type_Result = (string)privatetecLIDisplayPlugins.Invoke("get_inputsource_type", index);
            Assert.IsNotNull(get_inputsource_type_Result);
            Assert.That(inputsource_type, Is.EqualTo(get_inputsource_type_Result));

            index = "DISPLAYPORT2";
            inputsource_type = "DISPLAYPORT2";
            get_inputsource_type_Result = (string)privatetecLIDisplayPlugins.Invoke("get_inputsource_type", index);
            Assert.IsNotNull(get_inputsource_type_Result);
            Assert.That(inputsource_type, Is.EqualTo(get_inputsource_type_Result));

            index = "DISPLAYPORT-2";
            inputsource_type = "DISPLAYPORT2";
            get_inputsource_type_Result = (string)privatetecLIDisplayPlugins.Invoke("get_inputsource_type", index);
            Assert.IsNotNull(get_inputsource_type_Result);
            Assert.That(inputsource_type, Is.EqualTo(get_inputsource_type_Result));

            // USBC input
            index = "USBC";
            inputsource_type = "USB-C1";
            get_inputsource_type_Result = (string)privatetecLIDisplayPlugins.Invoke("get_inputsource_type", index);
            Assert.IsNotNull(get_inputsource_type_Result);
            Assert.That(inputsource_type, Is.EqualTo(get_inputsource_type_Result));

            index = "USBC1";
            inputsource_type = "USB-C1";
            get_inputsource_type_Result = (string)privatetecLIDisplayPlugins.Invoke("get_inputsource_type", index);
            Assert.IsNotNull(get_inputsource_type_Result);
            Assert.That(inputsource_type, Is.EqualTo(get_inputsource_type_Result));

            index = "USB-C";
            inputsource_type = "USB-C1";
            get_inputsource_type_Result = (string)privatetecLIDisplayPlugins.Invoke("get_inputsource_type", index);
            Assert.IsNotNull(get_inputsource_type_Result);
            Assert.That(inputsource_type, Is.EqualTo(get_inputsource_type_Result));

            // USBC2 input
            index = "USBC2";
            inputsource_type = "USB-C2";
            get_inputsource_type_Result = (string)privatetecLIDisplayPlugins.Invoke("get_inputsource_type", index);
            Assert.IsNotNull(get_inputsource_type_Result);
            Assert.That(inputsource_type, Is.EqualTo(get_inputsource_type_Result));

            index = "USB-C2";
            inputsource_type = "USB-C2";
            get_inputsource_type_Result = (string)privatetecLIDisplayPlugins.Invoke("get_inputsource_type", index);
            Assert.IsNotNull(get_inputsource_type_Result);
            Assert.That(inputsource_type, Is.EqualTo(get_inputsource_type_Result));

            // TBT input
            index = "TBT";
            inputsource_type = "Thunderbolt1";
            get_inputsource_type_Result = (string)privatetecLIDisplayPlugins.Invoke("get_inputsource_type", index);
            Assert.IsNotNull(get_inputsource_type_Result);
            Assert.That(inputsource_type, Is.EqualTo(get_inputsource_type_Result));

            index = "TBT1";
            inputsource_type = "Thunderbolt1";
            get_inputsource_type_Result = (string)privatetecLIDisplayPlugins.Invoke("get_inputsource_type", index);
            Assert.IsNotNull(get_inputsource_type_Result);
            Assert.That(inputsource_type, Is.EqualTo(get_inputsource_type_Result));

            index = "THUNDERBOLT";
            inputsource_type = "Thunderbolt1";
            get_inputsource_type_Result = (string)privatetecLIDisplayPlugins.Invoke("get_inputsource_type", index);
            Assert.IsNotNull(get_inputsource_type_Result);
            Assert.That(inputsource_type, Is.EqualTo(get_inputsource_type_Result));

            index = "THUNDERBOLT1";
            inputsource_type = "Thunderbolt1";
            get_inputsource_type_Result = (string)privatetecLIDisplayPlugins.Invoke("get_inputsource_type", index);
            Assert.IsNotNull(get_inputsource_type_Result);
            Assert.That(inputsource_type, Is.EqualTo(get_inputsource_type_Result));

            index = "THUNDERBOLT-1";
            inputsource_type = "Thunderbolt1";
            get_inputsource_type_Result = (string)privatetecLIDisplayPlugins.Invoke("get_inputsource_type", index);
            Assert.IsNotNull(get_inputsource_type_Result);
            Assert.That(inputsource_type, Is.EqualTo(get_inputsource_type_Result));

            // TBT2 input
            index = "TBT2";
            inputsource_type = "Thunderbolt2";
            get_inputsource_type_Result = (string)privatetecLIDisplayPlugins.Invoke("get_inputsource_type", index);
            Assert.IsNotNull(get_inputsource_type_Result);
            Assert.That(inputsource_type, Is.EqualTo(get_inputsource_type_Result));

            index = "THUNDERBOLT2";
            inputsource_type = "Thunderbolt2";
            get_inputsource_type_Result = (string)privatetecLIDisplayPlugins.Invoke("get_inputsource_type", index);
            Assert.IsNotNull(get_inputsource_type_Result);
            Assert.That(inputsource_type, Is.EqualTo(get_inputsource_type_Result));

            index = "THUNDERBOLT-2";
            inputsource_type = "Thunderbolt2";
            get_inputsource_type_Result = (string)privatetecLIDisplayPlugins.Invoke("get_inputsource_type", index);
            Assert.IsNotNull(get_inputsource_type_Result);
            Assert.That(inputsource_type, Is.EqualTo(get_inputsource_type_Result));

            index = "TestUnknown";
            inputsource_type = "Unknown";
            get_inputsource_type_Result = (string)privatetecLIDisplayPlugins.Invoke("get_inputsource_type", index);
            Assert.IsNotNull(get_inputsource_type_Result);
            Assert.That(inputsource_type, Is.EqualTo(get_inputsource_type_Result));
        }

        [Test]
        public void TestGetInputsList()
        {
            IDeviceManagerSA? devMgr;
            devMgr = null;
            string index = "0";
            List<string> inputs = new List<string>();
            if (devMgr == null)
            {
                var GetInputsList_Result1 = (Task<List<string>>)privatetecLIDisplayPlugins.Invoke("GetInputsList", devMgr, index); //devMgr == null
                Assert.IsNotNull(GetInputsList_Result1);
                Assert.That(inputs, Is.EqualTo(GetInputsList_Result1.Result));
            }

            Dictionary<string, InputInfo> inputSourceList2 = new Dictionary<string, InputInfo>();
            inputSourceList2.Add("input1", new InputInfo() { InputName = "HDMI", Code = 0X11, USBUpstream = "HDMI-1" });
            Mock<IDeviceManagerSA> devMgr2 = new Mock<IDeviceManagerSA>();
            List<MonitorInfo> _allInfoMonitors = new List<MonitorInfo>();
            _allInfoMonitors.Add(monitorInfo1);
            devMgr2.Setup(m => m.GetMonitors()).Returns(Task.FromResult(_allInfoMonitors));
            devMgr2.Setup(m => m.GetInputSourcelist(It.IsAny<MonitorInfo>())).Returns(Task.FromResult(inputSourceList2));
            List<string> inputs2 = new List<string>() { "INPUT1" };
            if (devMgr2 != null)
            {
                var GetInputsList_Result2 = (Task<List<string>>)privatetecLIDisplayPlugins.Invoke("GetInputsList", devMgr2.Object, index); //devMgr2 != null
                Assert.IsNotNull(GetInputsList_Result2);
                Assert.That(inputs2, Is.EqualTo(GetInputsList_Result2.Result));
            }
        }

        [Test]
        public void TestGetInputName()
        {
            IDeviceManagerSA? devMgr;
            devMgr = null;
            string index = "0";
            string input = "input1";
            int null_device_manager = 1;
            if (devMgr == null)
            {
                var GetInputName_Result1 = (Task<int>)privatetecLIDisplayPlugins.Invoke("GetInputName", devMgr, index, input); //devMgr == null
                Assert.IsNotNull(GetInputName_Result1);
                Assert.That(null_device_manager, Is.EqualTo(GetInputName_Result1.Result));
            }

            int device_manager = 0;
            Dictionary<string, InputInfo> inputSourceList2 = new Dictionary<string, InputInfo>();
            inputSourceList2.Add("input1", new InputInfo() { InputName = "HDMI", Code = 0X11, USBUpstream = "HDMI-1" });
            Mock<IDeviceManagerSA> devMgr2 = new Mock<IDeviceManagerSA>();
            List<MonitorInfo> _allInfoMonitors = new List<MonitorInfo>();
            _allInfoMonitors.Add(monitorInfo1);
            devMgr2.Setup(m => m.GetMonitors()).Returns(Task.FromResult(_allInfoMonitors));
            devMgr2.Setup(m => m.GetInputSourcelist(It.IsAny<MonitorInfo>())).Returns(Task.FromResult(inputSourceList2));

            if (devMgr2 != null)
            {
                var GetInputName_Result2 = (Task<int>)privatetecLIDisplayPlugins.Invoke("GetInputName", devMgr2.Object, index, input); //devMgr2 != null
                Assert.IsNotNull(GetInputName_Result2);
                Assert.That(device_manager, Is.EqualTo(GetInputName_Result2.Result));
            }
        }

        [Test]
        public void TestGetUSBUpstream()
        {
            IDeviceManagerSA? devMgr;
            devMgr = null;
            string index = "0";
            string input = "input1";
            int null_device_manager = 1;
            if (devMgr == null)
            {
                var GetUSBUpstream_Result1 = (Task<int>)privatetecLIDisplayPlugins.Invoke("GetUSBUpstream", devMgr, index, input); //devMgr == null
                Assert.IsNotNull(GetUSBUpstream_Result1);
                Assert.That(null_device_manager, Is.EqualTo(GetUSBUpstream_Result1.Result));
            }

            int device_manager = 0;
            Dictionary<string, InputInfo> inputSourceList2 = new Dictionary<string, InputInfo>();
            inputSourceList2.Add("input1", new InputInfo() { InputName = "HDMI", Code = 0X11, USBUpstream = "HDMI-1" });
            Mock<IDeviceManagerSA> devMgr2 = new Mock<IDeviceManagerSA>();
            List<MonitorInfo> _allInfoMonitors = new List<MonitorInfo>();
            _allInfoMonitors.Add(monitorInfo1);
            devMgr2.Setup(m => m.GetMonitors()).Returns(Task.FromResult(_allInfoMonitors));
            devMgr2.Setup(m => m.GetInputSourcelist(It.IsAny<MonitorInfo>())).Returns(Task.FromResult(inputSourceList2));

            if (devMgr2 != null)
            {
                var GetUSBUpstream_Result2 = (Task<int>)privatetecLIDisplayPlugins.Invoke("GetUSBUpstream", devMgr2.Object, index, input); //devMgr2 != null
                Assert.IsNotNull(GetUSBUpstream_Result2);
                Assert.That(device_manager, Is.EqualTo(GetUSBUpstream_Result2.Result));
            }
        }

        [Test]
        public void TestGetCurrentInput()
        {
            IDeviceManagerSA? devMgr;
            devMgr = null;
            string index = "0";
            string input = "";
            if (devMgr == null)
            {
                var GetCurrentInput_Result1 = (Task<string>)privatetecLIDisplayPlugins.Invoke("GetCurrentInput", devMgr, index); //devMgr == null
                Assert.IsNotNull(GetCurrentInput_Result1);
                Assert.That(input, Is.EqualTo(GetCurrentInput_Result1.Result));
            }

            string input2 = monitorInfo1.inputSource;

            Mock<IDeviceManagerSA> devMgr2 = new Mock<IDeviceManagerSA>();
            List<MonitorInfo> _allInfoMonitors = new List<MonitorInfo>();
            _allInfoMonitors.Add(monitorInfo1);
            devMgr2.Setup(m => m.GetMonitors()).Returns(Task.FromResult(_allInfoMonitors));

            if (devMgr2 != null)
            {
                var GetCurrentInput_Result2 = (Task<string>)privatetecLIDisplayPlugins.Invoke("GetCurrentInput", devMgr2.Object, index); //devMgr2 != null
                Assert.IsNotNull(GetCurrentInput_Result2);
                Assert.That(input2, Is.EqualTo(GetCurrentInput_Result2.Result));
            }
        }

        [Test]
        public void TestGetCurrentInput_()
        {
            IDeviceManagerSA? devMgr;
            devMgr = null;
            string index = "0";
            string input = "";
            if (devMgr == null)
            {
                var GetCurrentInput_Result1 = (Task<string>)privatetecLIDisplayPlugins.Invoke("GetCurrentInput", devMgr, monitorInfo1); //devMgr == null
                Assert.IsNotNull(GetCurrentInput_Result1);
                Assert.That(input, Is.EqualTo(GetCurrentInput_Result1.Result));
            }

            string input2 = monitorInfo1.inputSource;

            Mock<IDeviceManagerSA> devMgr2 = new Mock<IDeviceManagerSA>();
            List<MonitorInfo> _allInfoMonitors = new List<MonitorInfo>();
            _allInfoMonitors.Add(monitorInfo1);
            devMgr2.Setup(m => m.GetMonitors()).Returns(Task.FromResult(_allInfoMonitors));

            if (devMgr2 != null)
            {
                var GetCurrentInput_Result2 = (Task<string>)privatetecLIDisplayPlugins.Invoke("GetCurrentInput", devMgr2.Object, monitorInfo1); //devMgr2 != null
                Assert.IsNotNull(GetCurrentInput_Result2);
                Assert.That(input2, Is.EqualTo(GetCurrentInput_Result2.Result));
            }
        }

        [Test]
        public void Testget_inputsource_vcp()
        {
            string index;
            int inputsource_vcp;
            int get_inputsource_type_Result;

            // HDMI-1
            index = "HDMI-1";
            inputsource_vcp = 0x11;
            get_inputsource_type_Result = (int)privatetecLIDisplayPlugins.Invoke("get_inputsource_vcp", index);
            Assert.IsNotNull(get_inputsource_type_Result);
            Assert.That(inputsource_vcp, Is.EqualTo(get_inputsource_type_Result));

            // HDMI-2
            index = "HDMI-2";
            inputsource_vcp = 0x12;
            get_inputsource_type_Result = (int)privatetecLIDisplayPlugins.Invoke("get_inputsource_vcp", index);
            Assert.IsNotNull(get_inputsource_type_Result);
            Assert.That(inputsource_vcp, Is.EqualTo(get_inputsource_type_Result));

            // DISPLAYPORT-1
            index = "DISPLAYPORT-1";
            inputsource_vcp = 0x0f;
            get_inputsource_type_Result = (int)privatetecLIDisplayPlugins.Invoke("get_inputsource_vcp", index);
            Assert.IsNotNull(get_inputsource_type_Result);
            Assert.That(inputsource_vcp, Is.EqualTo(get_inputsource_type_Result));

            // DISPLAYPORT-2
            index = "DISPLAYPORT-2";
            inputsource_vcp = 0x13;
            get_inputsource_type_Result = (int)privatetecLIDisplayPlugins.Invoke("get_inputsource_vcp", index);
            Assert.IsNotNull(get_inputsource_type_Result);
            Assert.That(inputsource_vcp, Is.EqualTo(get_inputsource_type_Result));

            // USB-C1
            index = "USB-C1";
            inputsource_vcp = 0x1b;
            get_inputsource_type_Result = (int)privatetecLIDisplayPlugins.Invoke("get_inputsource_vcp", index);
            Assert.IsNotNull(get_inputsource_type_Result);
            Assert.That(inputsource_vcp, Is.EqualTo(get_inputsource_type_Result));

            // USB-C2
            index = "USB-C2";
            inputsource_vcp = 0x1c;
            get_inputsource_type_Result = (int)privatetecLIDisplayPlugins.Invoke("get_inputsource_vcp", index);
            Assert.IsNotNull(get_inputsource_type_Result);
            Assert.That(inputsource_vcp, Is.EqualTo(get_inputsource_type_Result));

            // Thunderbolt-1
            index = "Thunderbolt-1";
            inputsource_vcp = 0x19;
            get_inputsource_type_Result = (int)privatetecLIDisplayPlugins.Invoke("get_inputsource_vcp", index);
            Assert.IsNotNull(get_inputsource_type_Result);
            Assert.That(inputsource_vcp, Is.EqualTo(get_inputsource_type_Result));

            // Thunderbolt-2
            index = "Thunderbolt-2";
            inputsource_vcp = 0x1a;
            get_inputsource_type_Result = (int)privatetecLIDisplayPlugins.Invoke("get_inputsource_vcp", index);
            Assert.IsNotNull(get_inputsource_type_Result);
            Assert.That(inputsource_vcp, Is.EqualTo(get_inputsource_type_Result));

            // VGA-1
            index = "VGA-1";
            inputsource_vcp = 0x01;
            get_inputsource_type_Result = (int)privatetecLIDisplayPlugins.Invoke("get_inputsource_vcp", index);
            Assert.IsNotNull(get_inputsource_type_Result);
            Assert.That(inputsource_vcp, Is.EqualTo(get_inputsource_type_Result));

            // VGA-2
            index = "VGA-2";
            inputsource_vcp = 0x02;
            get_inputsource_type_Result = (int)privatetecLIDisplayPlugins.Invoke("get_inputsource_vcp", index);
            Assert.IsNotNull(get_inputsource_type_Result);
            Assert.That(inputsource_vcp, Is.EqualTo(get_inputsource_type_Result));

            // default
            index = "SomeOtherValue";
            inputsource_vcp = 0;
            get_inputsource_type_Result = (int)privatetecLIDisplayPlugins.Invoke("get_inputsource_vcp", index);
            Assert.IsNotNull(get_inputsource_type_Result);
            Assert.That(inputsource_vcp, Is.EqualTo(get_inputsource_type_Result));
        }

        [Test]
        public void Testget_language()
        {
            string index;
            string get_language;
            string get_language_Result;

            // Case 01
            index = "01";
            get_language = "Chinese";
            get_language_Result = (string)privatetecLIDisplayPlugins.Invoke("get_language", index);
            Assert.IsNotNull(get_language_Result);
            Assert.That(get_language, Is.EqualTo(get_language_Result));

            // Case 02
            index = "02";
            get_language = "English";
            get_language_Result = (string)privatetecLIDisplayPlugins.Invoke("get_language", index);
            Assert.IsNotNull(get_language_Result);
            Assert.That(get_language, Is.EqualTo(get_language_Result));

            // Case 03
            index = "03";
            get_language = "French";
            get_language_Result = (string)privatetecLIDisplayPlugins.Invoke("get_language", index);
            Assert.IsNotNull(get_language_Result);
            Assert.That(get_language, Is.EqualTo(get_language_Result));

            // Case 04
            index = "04";
            get_language = "German";
            get_language_Result = (string)privatetecLIDisplayPlugins.Invoke("get_language", index);
            Assert.IsNotNull(get_language_Result);
            Assert.That(get_language, Is.EqualTo(get_language_Result));

            // Case 05
            index = "05";
            get_language = "Italian";
            get_language_Result = (string)privatetecLIDisplayPlugins.Invoke("get_language", index);
            Assert.IsNotNull(get_language_Result);
            Assert.That(get_language, Is.EqualTo(get_language_Result));

            // Case 06
            index = "06";
            get_language = "Japanese";
            get_language_Result = (string)privatetecLIDisplayPlugins.Invoke("get_language", index);
            Assert.IsNotNull(get_language_Result);
            Assert.That(get_language, Is.EqualTo(get_language_Result));

            // Case 07
            index = "07";
            get_language = "Korean";
            get_language_Result = (string)privatetecLIDisplayPlugins.Invoke("get_language", index);
            Assert.IsNotNull(get_language_Result);
            Assert.That(get_language, Is.EqualTo(get_language_Result));

            // Case 08
            index = "08";
            get_language = "Portuguese";
            get_language_Result = (string)privatetecLIDisplayPlugins.Invoke("get_language", index);
            Assert.IsNotNull(get_language_Result);
            Assert.That(get_language, Is.EqualTo(get_language_Result));

            // Case 09
            index = "09";
            get_language = "Russian";
            get_language_Result = (string)privatetecLIDisplayPlugins.Invoke("get_language", index);
            Assert.IsNotNull(get_language_Result);
            Assert.That(get_language, Is.EqualTo(get_language_Result));

            // Case 0a=10
            index = "10";
            get_language = "Spanish";
            get_language_Result = (string)privatetecLIDisplayPlugins.Invoke("get_language", index);
            Assert.IsNotNull(get_language_Result);
            Assert.That(get_language, Is.EqualTo(get_language_Result));

            // Case 0b=11
            index = "11";
            get_language = "Swedish";
            get_language_Result = (string)privatetecLIDisplayPlugins.Invoke("get_language", index);
            Assert.IsNotNull(get_language_Result);
            Assert.That(get_language, Is.EqualTo(get_language_Result));

            //// Case 0c=12
            index = "12";
            get_language = "Turkish";
            get_language_Result = (string)privatetecLIDisplayPlugins.Invoke("get_language", index);
            Assert.IsNotNull(get_language_Result);
            Assert.That(get_language, Is.EqualTo(get_language_Result));

            //// Case 0d=13
            index = "13";
            get_language = "Chinese-Simplified";
            get_language_Result = (string)privatetecLIDisplayPlugins.Invoke("get_language", index);
            Assert.IsNotNull(get_language_Result);
            Assert.That(get_language, Is.EqualTo(get_language_Result));

            //// Case 0e=14
            index = "14";
            get_language = "BrazilianPortuguese";
            get_language_Result = (string)privatetecLIDisplayPlugins.Invoke("get_language", index);
            Assert.IsNotNull(get_language_Result);
            Assert.That(get_language, Is.EqualTo(get_language_Result));

            //// Case 0f=15
            index = "15";
            get_language = "Arabic";
            get_language_Result = (string)privatetecLIDisplayPlugins.Invoke("get_language", index);
            Assert.IsNotNull(get_language_Result);
            Assert.That(get_language, Is.EqualTo(get_language_Result));

            // Case 10=16
            index = "16";
            get_language = "Bulgarian";
            get_language_Result = (string)privatetecLIDisplayPlugins.Invoke("get_language", index);
            Assert.IsNotNull(get_language_Result);
            Assert.That(get_language, Is.EqualTo(get_language_Result));

            // Case 11=17
            index = "17";
            get_language = "Croatian";
            get_language_Result = (string)privatetecLIDisplayPlugins.Invoke("get_language", index);
            Assert.IsNotNull(get_language_Result);
            Assert.That(get_language, Is.EqualTo(get_language_Result));

            // Case 12=18
            index = "18";
            get_language = "Czech";
            get_language_Result = (string)privatetecLIDisplayPlugins.Invoke("get_language", index);
            Assert.IsNotNull(get_language_Result);
            Assert.That(get_language, Is.EqualTo(get_language_Result));

            // Case 13=19
            index = "19";
            get_language = "Danish";
            get_language_Result = (string)privatetecLIDisplayPlugins.Invoke("get_language", index);
            Assert.IsNotNull(get_language_Result);
            Assert.That(get_language, Is.EqualTo(get_language_Result));

            // Case 14=20
            index = "20";
            get_language = "Dutch";
            get_language_Result = (string)privatetecLIDisplayPlugins.Invoke("get_language", index);
            Assert.IsNotNull(get_language_Result);
            Assert.That(get_language, Is.EqualTo(get_language_Result));

            // Case 15=21
            index = "21";
            get_language = "Estonian";
            get_language_Result = (string)privatetecLIDisplayPlugins.Invoke("get_language", index);
            Assert.IsNotNull(get_language_Result);
            Assert.That(get_language, Is.EqualTo(get_language_Result));

            // Case 16=22
            index = "22";
            get_language = "Finnish";
            get_language_Result = (string)privatetecLIDisplayPlugins.Invoke("get_language", index);
            Assert.IsNotNull(get_language_Result);
            Assert.That(get_language, Is.EqualTo(get_language_Result));

            // Case 17=23
            index = "23";
            get_language = "Greek";
            get_language_Result = (string)privatetecLIDisplayPlugins.Invoke("get_language", index);
            Assert.IsNotNull(get_language_Result);
            Assert.That(get_language, Is.EqualTo(get_language_Result));

            // Case 18=24
            index = "24";
            get_language = "Hebrew";
            get_language_Result = (string)privatetecLIDisplayPlugins.Invoke("get_language", index);
            Assert.IsNotNull(get_language_Result);
            Assert.That(get_language, Is.EqualTo(get_language_Result));

            // Case 19=25
            index = "25";
            get_language = "Hindi";
            get_language_Result = (string)privatetecLIDisplayPlugins.Invoke("get_language", index);
            Assert.IsNotNull(get_language_Result);
            Assert.That(get_language, Is.EqualTo(get_language_Result));

            // Case 1a=26
            index = "26";
            get_language = "Hungarian";
            get_language_Result = (string)privatetecLIDisplayPlugins.Invoke("get_language", index);
            Assert.IsNotNull(get_language_Result);
            Assert.That(get_language, Is.EqualTo(get_language_Result));

            // Case 1b=27
            index = "27";
            get_language = "Latvian";
            get_language_Result = (string)privatetecLIDisplayPlugins.Invoke("get_language", index);
            Assert.IsNotNull(get_language_Result);
            Assert.That(get_language, Is.EqualTo(get_language_Result));

            // Case 1c=28
            index = "28";
            get_language = "Lithuanian";
            get_language_Result = (string)privatetecLIDisplayPlugins.Invoke("get_language", index);
            Assert.IsNotNull(get_language_Result);
            Assert.That(get_language, Is.EqualTo(get_language_Result));

            // Case 1d=29
            index = "29";
            get_language = "Norwegian";
            get_language_Result = (string)privatetecLIDisplayPlugins.Invoke("get_language", index);
            Assert.IsNotNull(get_language_Result);
            Assert.That(get_language, Is.EqualTo(get_language_Result));

            // Case 1e=30
            index = "30";
            get_language = "Polish";
            get_language_Result = (string)privatetecLIDisplayPlugins.Invoke("get_language", index);
            Assert.IsNotNull(get_language_Result);
            Assert.That(get_language, Is.EqualTo(get_language_Result));

            // Case 1f=31
            index = "31";
            get_language = "Romanian";
            get_language_Result = (string)privatetecLIDisplayPlugins.Invoke("get_language", index);
            Assert.IsNotNull(get_language_Result);
            Assert.That(get_language, Is.EqualTo(get_language_Result));

            // Case 20=32
            index = "32";
            get_language = "Serbian";
            get_language_Result = (string)privatetecLIDisplayPlugins.Invoke("get_language", index);
            Assert.IsNotNull(get_language_Result);
            Assert.That(get_language, Is.EqualTo(get_language_Result));

            // Case 21=33
            index = "33";
            get_language = "Slovak";
            get_language_Result = (string)privatetecLIDisplayPlugins.Invoke("get_language", index);
            Assert.IsNotNull(get_language_Result);
            Assert.That(get_language, Is.EqualTo(get_language_Result));

            // Case 22=34
            index = "34";
            get_language = "Slovenian";
            get_language_Result = (string)privatetecLIDisplayPlugins.Invoke("get_language", index);
            Assert.IsNotNull(get_language_Result);
            Assert.That(get_language, Is.EqualTo(get_language_Result));

            // Case 23=35
            index = "35";
            get_language = "Thai";
            get_language_Result = (string)privatetecLIDisplayPlugins.Invoke("get_language", index);
            Assert.IsNotNull(get_language_Result);
            Assert.That(get_language, Is.EqualTo(get_language_Result));

            // Case 24=36
            index = "36";
            get_language = "Ukrainian";
            get_language_Result = (string)privatetecLIDisplayPlugins.Invoke("get_language", index);
            Assert.IsNotNull(get_language_Result);
            Assert.That(get_language, Is.EqualTo(get_language_Result));

            // Case 25=37
            index = "37";
            get_language = "Vietnamese";
            get_language_Result = (string)privatetecLIDisplayPlugins.Invoke("get_language", index);
            Assert.IsNotNull(get_language_Result);
            Assert.That(get_language, Is.EqualTo(get_language_Result));

            // Default
            index = "38";
            get_language = "Unknown";
            get_language_Result = (string)privatetecLIDisplayPlugins.Invoke("get_language", index);
            Assert.IsNotNull(get_language_Result);
            Assert.That(get_language, Is.EqualTo(get_language_Result));
        }

        [Test]
        public void Testget_display_technology_type()
        {
            string index;
            string display_technology_type;
            string get_display_technology_type_Result;

            // Case 1
            index = "1";
            display_technology_type = "CRT (shadow mask)";
            get_display_technology_type_Result = (string)privatetecLIDisplayPlugins.Invoke("get_display_technology_type", index);
            Assert.IsNotNull(get_display_technology_type_Result);
            Assert.That(display_technology_type, Is.EqualTo(get_display_technology_type_Result));

            // Case 2
            index = "2";
            display_technology_type = "CRT (aperture grill)";
            get_display_technology_type_Result = (string)privatetecLIDisplayPlugins.Invoke("get_display_technology_type", index);
            Assert.IsNotNull(get_display_technology_type_Result);
            Assert.That(display_technology_type, Is.EqualTo(get_display_technology_type_Result));

            // Case 3
            index = "3";
            display_technology_type = "LCD (active matrix)";
            get_display_technology_type_Result = (string)privatetecLIDisplayPlugins.Invoke("get_display_technology_type", index);
            Assert.IsNotNull(get_display_technology_type_Result);
            Assert.That(display_technology_type, Is.EqualTo(get_display_technology_type_Result));

            // Case 4
            index = "4";
            display_technology_type = "LCoS";
            get_display_technology_type_Result = (string)privatetecLIDisplayPlugins.Invoke("get_display_technology_type", index);
            Assert.IsNotNull(get_display_technology_type_Result);
            Assert.That(display_technology_type, Is.EqualTo(get_display_technology_type_Result));

            // Case 5
            index = "5";
            display_technology_type = "Plasma";
            get_display_technology_type_Result = (string)privatetecLIDisplayPlugins.Invoke("get_display_technology_type", index);
            Assert.IsNotNull(get_display_technology_type_Result);
            Assert.That(display_technology_type, Is.EqualTo(get_display_technology_type_Result));

            // Case 6
            index = "6";
            display_technology_type = "OLED";
            get_display_technology_type_Result = (string)privatetecLIDisplayPlugins.Invoke("get_display_technology_type", index);
            Assert.IsNotNull(get_display_technology_type_Result);
            Assert.That(display_technology_type, Is.EqualTo(get_display_technology_type_Result));

            // Case 7
            index = "7";
            display_technology_type = "EL";
            get_display_technology_type_Result = (string)privatetecLIDisplayPlugins.Invoke("get_display_technology_type", index);
            Assert.IsNotNull(get_display_technology_type_Result);
            Assert.That(display_technology_type, Is.EqualTo(get_display_technology_type_Result));

            // Case 8
            index = "8";
            display_technology_type = "Dynamic MEM  e.g. DLP";
            get_display_technology_type_Result = (string)privatetecLIDisplayPlugins.Invoke("get_display_technology_type", index);
            Assert.IsNotNull(get_display_technology_type_Result);
            Assert.That(display_technology_type, Is.EqualTo(get_display_technology_type_Result));

            // Case 9
            index = "9";
            display_technology_type = "Static MEM  e.g. iMOD";
            get_display_technology_type_Result = (string)privatetecLIDisplayPlugins.Invoke("get_display_technology_type", index);
            Assert.IsNotNull(get_display_technology_type_Result);
            Assert.That(display_technology_type, Is.EqualTo(get_display_technology_type_Result));

            // Default
            index = "10";
            display_technology_type = "Unknown";
            get_display_technology_type_Result = (string)privatetecLIDisplayPlugins.Invoke("get_display_technology_type", index);
            Assert.IsNotNull(get_display_technology_type_Result);
            Assert.That(display_technology_type, Is.EqualTo(get_display_technology_type_Result));
        }

        [Test]
        public void Testget_osd()
        {
            string index;
            string get_osd;
            string get_osd_Result;

            // Case 01
            index = "01";
            get_osd = "OSDLock";
            get_osd_Result = (string)privatetecLIDisplayPlugins.Invoke("get_osd", index);
            Assert.IsNotNull(get_osd_Result);
            Assert.That(get_osd, Is.EqualTo(get_osd_Result));

            // Case 02
            index = "02";
            get_osd = "OSDUnlock";
            get_osd_Result = (string)privatetecLIDisplayPlugins.Invoke("get_osd", index);
            Assert.IsNotNull(get_osd_Result);
            Assert.That(get_osd, Is.EqualTo(get_osd_Result));

            // Case 03
            index = "03";
            get_osd = "Unknown";
            get_osd_Result = (string)privatetecLIDisplayPlugins.Invoke("get_osd", index);
            Assert.IsNotNull(get_osd_Result);
            Assert.That(get_osd, Is.EqualTo(get_osd_Result));
        }

        [Test]
        public void TestGCD()
        {
            ulong a;
            ulong b;
            a = 20;
            b = 10;
            if (a > b)
            {
                var GCD_Result = (ulong)privatetecLIDisplayPlugins.Invoke("GCD", a, b);
                Assert.IsNotNull(GCD_Result);
                Assert.That(b, Is.EqualTo(GCD_Result));
            }

            a = 10;
            b = 20;
            if (a < b)
            {
                var GCD_Result = (ulong)privatetecLIDisplayPlugins.Invoke("GCD", a, b);
                Assert.IsNotNull(GCD_Result);
                Assert.That(a, Is.EqualTo(GCD_Result));
            }
        }

        [Test]
        public void TestGet_AR()
        {
            double value;
            string Get_AR;
            string Get_AR_Result;

            // Case 01  n == 1.25
            value = 1.25;
            Get_AR = "5:4";
            Get_AR_Result = (string)privatetecLIDisplayPlugins.Invoke("Get_AR", value);
            Assert.IsNotNull(Get_AR_Result);
            Assert.That(Get_AR, Is.EqualTo(Get_AR_Result));

            // Case 02  n > 1.25 && n < 1.5://1.333
            value = 1.333;
            Get_AR = "4:3";
            Get_AR_Result = (string)privatetecLIDisplayPlugins.Invoke("Get_AR", value);
            Assert.IsNotNull(Get_AR_Result);
            Assert.That(Get_AR, Is.EqualTo(Get_AR_Result));

            // Case 03  n == 1.5
            value = 1.5;
            Get_AR = "3:2";
            Get_AR_Result = (string)privatetecLIDisplayPlugins.Invoke("Get_AR", value);
            Assert.IsNotNull(Get_AR_Result);
            Assert.That(Get_AR, Is.EqualTo(Get_AR_Result));

            // Case 04  n == 1.6
            value = 1.6;
            Get_AR = "16:10";
            Get_AR_Result = (string)privatetecLIDisplayPlugins.Invoke("Get_AR", value);
            Assert.IsNotNull(Get_AR_Result);
            Assert.That(Get_AR, Is.EqualTo(Get_AR_Result));

            // Case 05  n > 1.6 && n < 1.7://1.666
            value = 1.666;
            Get_AR = "15:9";
            Get_AR_Result = (string)privatetecLIDisplayPlugins.Invoke("Get_AR", value);
            Assert.IsNotNull(Get_AR_Result);
            Assert.That(Get_AR, Is.EqualTo(Get_AR_Result));

            // Case 06  (n > 1.7 && n < 2.0)://1.777
            value = 1.777;
            Get_AR = "16:9";
            Get_AR_Result = (string)privatetecLIDisplayPlugins.Invoke("Get_AR", value);
            Assert.IsNotNull(Get_AR_Result);
            Assert.That(Get_AR, Is.EqualTo(Get_AR_Result));

            // Case 07  n == 2.0:
            value = 2.0;
            Get_AR = "18:9";
            Get_AR_Result = (string)privatetecLIDisplayPlugins.Invoke("Get_AR", value);
            Assert.IsNotNull(Get_AR_Result);
            Assert.That(Get_AR, Is.EqualTo(Get_AR_Result));

            // Case 08  n > 2.0 && n < 2.3://2.222
            value = 2.222;
            Get_AR = "20:9";
            Get_AR_Result = (string)privatetecLIDisplayPlugins.Invoke("Get_AR", value);
            Assert.IsNotNull(Get_AR_Result);
            Assert.That(Get_AR, Is.EqualTo(Get_AR_Result));

            // Case 09  n > 2.2 && n < 3.5://2.3....
            value = 2.3;
            Get_AR = "21:9";
            Get_AR_Result = (string)privatetecLIDisplayPlugins.Invoke("Get_AR", value);
            Assert.IsNotNull(Get_AR_Result);
            Assert.That(Get_AR, Is.EqualTo(Get_AR_Result));

            // Case 10   n > 3.5://3.555
            value = 3.555;
            Get_AR = "32:9";
            Get_AR_Result = (string)privatetecLIDisplayPlugins.Invoke("Get_AR", value);
            Assert.IsNotNull(Get_AR_Result);
            Assert.That(Get_AR, Is.EqualTo(Get_AR_Result));

            // default
            value = 1;
            Get_AR = "N/A";
            Get_AR_Result = (string)privatetecLIDisplayPlugins.Invoke("Get_AR", value);
            Assert.IsNotNull(Get_AR_Result);
            Assert.That(Get_AR, Is.EqualTo(Get_AR_Result));
        }

        [Test]
        public void TestGetOSDLanguage_index()
        {
            string language;
            int GetOSDLanguage_index;
            int GetOSDLanguage_index_Result;

            // Case 01
            language = "Chinese";
            GetOSDLanguage_index = 0x01;
            GetOSDLanguage_index_Result = (int)privatetecLIDisplayPlugins.Invoke("GetOSDLanguage_index", language);
            Assert.IsNotNull(GetOSDLanguage_index_Result);
            Assert.That(GetOSDLanguage_index, Is.EqualTo(GetOSDLanguage_index_Result));

            // Case 02
            language = "English";
            GetOSDLanguage_index = 0x02;
            GetOSDLanguage_index_Result = (int)privatetecLIDisplayPlugins.Invoke("GetOSDLanguage_index", language);
            Assert.IsNotNull(GetOSDLanguage_index_Result);
            Assert.That(GetOSDLanguage_index, Is.EqualTo(GetOSDLanguage_index_Result));

            // Case 03
            language = "French";
            GetOSDLanguage_index = 0x03;
            GetOSDLanguage_index_Result = (int)privatetecLIDisplayPlugins.Invoke("GetOSDLanguage_index", language);
            Assert.IsNotNull(GetOSDLanguage_index_Result);
            Assert.That(GetOSDLanguage_index, Is.EqualTo(GetOSDLanguage_index_Result));

            // Case 04
            language = "German";
            GetOSDLanguage_index = 0x04;
            GetOSDLanguage_index_Result = (int)privatetecLIDisplayPlugins.Invoke("GetOSDLanguage_index", language);
            Assert.IsNotNull(GetOSDLanguage_index_Result);
            Assert.That(GetOSDLanguage_index, Is.EqualTo(GetOSDLanguage_index_Result));

            // Case 05
            language = "Italian";
            GetOSDLanguage_index = 0x05;
            GetOSDLanguage_index_Result = (int)privatetecLIDisplayPlugins.Invoke("GetOSDLanguage_index", language);
            Assert.IsNotNull(GetOSDLanguage_index_Result);
            Assert.That(GetOSDLanguage_index, Is.EqualTo(GetOSDLanguage_index_Result));

            // Case 06
            language = "Japanese";
            GetOSDLanguage_index = 0x06;
            GetOSDLanguage_index_Result = (int)privatetecLIDisplayPlugins.Invoke("GetOSDLanguage_index", language);
            Assert.IsNotNull(GetOSDLanguage_index_Result);
            Assert.That(GetOSDLanguage_index, Is.EqualTo(GetOSDLanguage_index_Result));

            // Case 07
            language = "Korean";
            GetOSDLanguage_index = 0x07;
            GetOSDLanguage_index_Result = (int)privatetecLIDisplayPlugins.Invoke("GetOSDLanguage_index", language);
            Assert.IsNotNull(GetOSDLanguage_index_Result);
            Assert.That(GetOSDLanguage_index, Is.EqualTo(GetOSDLanguage_index_Result));

            // Case 08
            language = "Portuguese";
            GetOSDLanguage_index = 0x08;
            GetOSDLanguage_index_Result = (int)privatetecLIDisplayPlugins.Invoke("GetOSDLanguage_index", language);
            Assert.IsNotNull(GetOSDLanguage_index_Result);
            Assert.That(GetOSDLanguage_index, Is.EqualTo(GetOSDLanguage_index_Result));

            // Case 09
            language = "Russian";
            GetOSDLanguage_index = 0x09;
            GetOSDLanguage_index_Result = (int)privatetecLIDisplayPlugins.Invoke("GetOSDLanguage_index", language);
            Assert.IsNotNull(GetOSDLanguage_index_Result);
            Assert.That(GetOSDLanguage_index, Is.EqualTo(GetOSDLanguage_index_Result));

            // Case 0a
            language = "Spanish";
            GetOSDLanguage_index = 0x0a;
            GetOSDLanguage_index_Result = (int)privatetecLIDisplayPlugins.Invoke("GetOSDLanguage_index", language);
            Assert.IsNotNull(GetOSDLanguage_index_Result);
            Assert.That(GetOSDLanguage_index, Is.EqualTo(GetOSDLanguage_index_Result));

            // Case 0b
            language = "Swedish";
            GetOSDLanguage_index = 0x0b;
            GetOSDLanguage_index_Result = (int)privatetecLIDisplayPlugins.Invoke("GetOSDLanguage_index", language);
            Assert.IsNotNull(GetOSDLanguage_index_Result);
            Assert.That(GetOSDLanguage_index, Is.EqualTo(GetOSDLanguage_index_Result));

            // Case 0c
            language = "Turkish";
            GetOSDLanguage_index = 0x0c;
            GetOSDLanguage_index_Result = (int)privatetecLIDisplayPlugins.Invoke("GetOSDLanguage_index", language);
            Assert.IsNotNull(GetOSDLanguage_index_Result);
            Assert.That(GetOSDLanguage_index, Is.EqualTo(GetOSDLanguage_index_Result));

            // Case 0d
            language = "Chinese-Simplified";
            GetOSDLanguage_index = 0x0d;
            GetOSDLanguage_index_Result = (int)privatetecLIDisplayPlugins.Invoke("GetOSDLanguage_index", language);
            Assert.IsNotNull(GetOSDLanguage_index_Result);
            Assert.That(GetOSDLanguage_index, Is.EqualTo(GetOSDLanguage_index_Result));

            // Case 0e
            language = "BrazilianPortuguese";
            GetOSDLanguage_index = 0x0e;
            GetOSDLanguage_index_Result = (int)privatetecLIDisplayPlugins.Invoke("GetOSDLanguage_index", language);
            Assert.IsNotNull(GetOSDLanguage_index_Result);
            Assert.That(GetOSDLanguage_index, Is.EqualTo(GetOSDLanguage_index_Result));

            // Case 0f
            language = "Arabic";
            GetOSDLanguage_index = 0x0f;
            GetOSDLanguage_index_Result = (int)privatetecLIDisplayPlugins.Invoke("GetOSDLanguage_index", language);
            Assert.IsNotNull(GetOSDLanguage_index_Result);
            Assert.That(GetOSDLanguage_index, Is.EqualTo(GetOSDLanguage_index_Result));

            // Case 10
            language = "Bulgarian";
            GetOSDLanguage_index = 0x10;
            GetOSDLanguage_index_Result = (int)privatetecLIDisplayPlugins.Invoke("GetOSDLanguage_index", language);
            Assert.IsNotNull(GetOSDLanguage_index_Result);
            Assert.That(GetOSDLanguage_index, Is.EqualTo(GetOSDLanguage_index_Result));

            // Case 11
            language = "Croatian";
            GetOSDLanguage_index = 0x11;
            GetOSDLanguage_index_Result = (int)privatetecLIDisplayPlugins.Invoke("GetOSDLanguage_index", language);
            Assert.IsNotNull(GetOSDLanguage_index_Result);
            Assert.That(GetOSDLanguage_index, Is.EqualTo(GetOSDLanguage_index_Result));

            // Case 12
            language = "Czech";
            GetOSDLanguage_index = 0x12;
            GetOSDLanguage_index_Result = (int)privatetecLIDisplayPlugins.Invoke("GetOSDLanguage_index", language);
            Assert.IsNotNull(GetOSDLanguage_index_Result);
            Assert.That(GetOSDLanguage_index, Is.EqualTo(GetOSDLanguage_index_Result));

            // Case 13
            language = "Danish";
            GetOSDLanguage_index = 0x13;
            GetOSDLanguage_index_Result = (int)privatetecLIDisplayPlugins.Invoke("GetOSDLanguage_index", language);
            Assert.IsNotNull(GetOSDLanguage_index_Result);
            Assert.That(GetOSDLanguage_index, Is.EqualTo(GetOSDLanguage_index_Result));

            // Case 14
            language = "Dutch";
            GetOSDLanguage_index = 0x14;
            GetOSDLanguage_index_Result = (int)privatetecLIDisplayPlugins.Invoke("GetOSDLanguage_index", language);
            Assert.IsNotNull(GetOSDLanguage_index_Result);
            Assert.That(GetOSDLanguage_index, Is.EqualTo(GetOSDLanguage_index_Result));

            // Case 15
            language = "Estonian";
            GetOSDLanguage_index = 0x15;
            GetOSDLanguage_index_Result = (int)privatetecLIDisplayPlugins.Invoke("GetOSDLanguage_index", language);
            Assert.IsNotNull(GetOSDLanguage_index_Result);
            Assert.That(GetOSDLanguage_index, Is.EqualTo(GetOSDLanguage_index_Result));

            // Case 16
            language = "Finnish";
            GetOSDLanguage_index = 0x16;
            GetOSDLanguage_index_Result = (int)privatetecLIDisplayPlugins.Invoke("GetOSDLanguage_index", language);
            Assert.IsNotNull(GetOSDLanguage_index_Result);
            Assert.That(GetOSDLanguage_index, Is.EqualTo(GetOSDLanguage_index_Result));

            // Case 17
            language = "Greek";
            GetOSDLanguage_index = 0x17;
            GetOSDLanguage_index_Result = (int)privatetecLIDisplayPlugins.Invoke("GetOSDLanguage_index", language);
            Assert.IsNotNull(GetOSDLanguage_index_Result);
            Assert.That(GetOSDLanguage_index, Is.EqualTo(GetOSDLanguage_index_Result));

            // Case 18
            language = "Hebrew";
            GetOSDLanguage_index = 0x18;
            GetOSDLanguage_index_Result = (int)privatetecLIDisplayPlugins.Invoke("GetOSDLanguage_index", language);
            Assert.IsNotNull(GetOSDLanguage_index_Result);
            Assert.That(GetOSDLanguage_index, Is.EqualTo(GetOSDLanguage_index_Result));

            // Case 19
            language = "Hindi";
            GetOSDLanguage_index = 0x19;
            GetOSDLanguage_index_Result = (int)privatetecLIDisplayPlugins.Invoke("GetOSDLanguage_index", language);
            Assert.IsNotNull(GetOSDLanguage_index_Result);
            Assert.That(GetOSDLanguage_index, Is.EqualTo(GetOSDLanguage_index_Result));

            // Case 1a
            language = "Hungarian";
            GetOSDLanguage_index = 0x1a;
            GetOSDLanguage_index_Result = (int)privatetecLIDisplayPlugins.Invoke("GetOSDLanguage_index", language);
            Assert.IsNotNull(GetOSDLanguage_index_Result);
            Assert.That(GetOSDLanguage_index, Is.EqualTo(GetOSDLanguage_index_Result));

            // Case 1b
            language = "Latvian";
            GetOSDLanguage_index = 0x1b;
            GetOSDLanguage_index_Result = (int)privatetecLIDisplayPlugins.Invoke("GetOSDLanguage_index", language);
            Assert.IsNotNull(GetOSDLanguage_index_Result);
            Assert.That(GetOSDLanguage_index, Is.EqualTo(GetOSDLanguage_index_Result));

            // Case 1c
            language = "Lithuanian";
            GetOSDLanguage_index = 0x1c;
            GetOSDLanguage_index_Result = (int)privatetecLIDisplayPlugins.Invoke("GetOSDLanguage_index", language);
            Assert.IsNotNull(GetOSDLanguage_index_Result);
            Assert.That(GetOSDLanguage_index, Is.EqualTo(GetOSDLanguage_index_Result));

            // Case 1d
            language = "Norwegian";
            GetOSDLanguage_index = 0x1d;
            GetOSDLanguage_index_Result = (int)privatetecLIDisplayPlugins.Invoke("GetOSDLanguage_index", language);
            Assert.IsNotNull(GetOSDLanguage_index_Result);
            Assert.That(GetOSDLanguage_index, Is.EqualTo(GetOSDLanguage_index_Result));

            // Case 1e
            language = "Polish";
            GetOSDLanguage_index = 0x1e;
            GetOSDLanguage_index_Result = (int)privatetecLIDisplayPlugins.Invoke("GetOSDLanguage_index", language);
            Assert.IsNotNull(GetOSDLanguage_index_Result);
            Assert.That(GetOSDLanguage_index, Is.EqualTo(GetOSDLanguage_index_Result));

            // Case 1f
            language = "Romanian";
            GetOSDLanguage_index = 0x1f;
            GetOSDLanguage_index_Result = (int)privatetecLIDisplayPlugins.Invoke("GetOSDLanguage_index", language);
            Assert.IsNotNull(GetOSDLanguage_index_Result);
            Assert.That(GetOSDLanguage_index, Is.EqualTo(GetOSDLanguage_index_Result));

            // Case 20
            language = "Serbian";
            GetOSDLanguage_index = 0x20;
            GetOSDLanguage_index_Result = (int)privatetecLIDisplayPlugins.Invoke("GetOSDLanguage_index", language);
            Assert.IsNotNull(GetOSDLanguage_index_Result);
            Assert.That(GetOSDLanguage_index, Is.EqualTo(GetOSDLanguage_index_Result));

            // Case 21
            language = "Slovak";
            GetOSDLanguage_index = 0x21;
            GetOSDLanguage_index_Result = (int)privatetecLIDisplayPlugins.Invoke("GetOSDLanguage_index", language);
            Assert.IsNotNull(GetOSDLanguage_index_Result);
            Assert.That(GetOSDLanguage_index, Is.EqualTo(GetOSDLanguage_index_Result));

            // Case 22
            language = "Slovenian";
            GetOSDLanguage_index = 0x22;
            GetOSDLanguage_index_Result = (int)privatetecLIDisplayPlugins.Invoke("GetOSDLanguage_index", language);
            Assert.IsNotNull(GetOSDLanguage_index_Result);
            Assert.That(GetOSDLanguage_index, Is.EqualTo(GetOSDLanguage_index_Result));

            // Case 23
            language = "Thai";
            GetOSDLanguage_index = 0x23;
            GetOSDLanguage_index_Result = (int)privatetecLIDisplayPlugins.Invoke("GetOSDLanguage_index", language);
            Assert.IsNotNull(GetOSDLanguage_index_Result);
            Assert.That(GetOSDLanguage_index, Is.EqualTo(GetOSDLanguage_index_Result));

            // Case 24
            language = "Ukrainian";
            GetOSDLanguage_index = 0x24;
            GetOSDLanguage_index_Result = (int)privatetecLIDisplayPlugins.Invoke("GetOSDLanguage_index", language);
            Assert.IsNotNull(GetOSDLanguage_index_Result);
            Assert.That(GetOSDLanguage_index, Is.EqualTo(GetOSDLanguage_index_Result));

            // Case 25
            language = "Vietnamese";
            GetOSDLanguage_index = 0x25;
            GetOSDLanguage_index_Result = (int)privatetecLIDisplayPlugins.Invoke("GetOSDLanguage_index", language);
            Assert.IsNotNull(GetOSDLanguage_index_Result);
            Assert.That(GetOSDLanguage_index, Is.EqualTo(GetOSDLanguage_index_Result));

            // Default
            language = "OtherLanguage";
            GetOSDLanguage_index = 0xff;
            GetOSDLanguage_index_Result = (int)privatetecLIDisplayPlugins.Invoke("GetOSDLanguage_index", language);
            Assert.IsNotNull(GetOSDLanguage_index_Result);
            Assert.That(GetOSDLanguage_index, Is.EqualTo(GetOSDLanguage_index_Result));
        }

        [Test]
        public void Testmodify_Manufactur()
        {
            string temp;
            string modify_Manufactur;
            string modify_Manufactur_Result;

            // Case 01
            temp = "del";
            modify_Manufactur = "Dell";
            modify_Manufactur_Result = (string)privatetecLIDisplayPlugins.Invoke("modify_Manufactur", temp);
            Assert.IsNotNull(modify_Manufactur_Result);
            Assert.That(modify_Manufactur, Is.EqualTo(modify_Manufactur_Result));

            // Case 02
            temp = "aw";
            modify_Manufactur = "Alienware";
            modify_Manufactur_Result = (string)privatetecLIDisplayPlugins.Invoke("modify_Manufactur", temp);
            Assert.IsNotNull(modify_Manufactur_Result);
            Assert.That(modify_Manufactur, Is.EqualTo(modify_Manufactur_Result));

            // Case 03
            temp = "Test Dell";
            modify_Manufactur = "Test Dell";
            modify_Manufactur_Result = (string)privatetecLIDisplayPlugins.Invoke("modify_Manufactur", temp);
            Assert.IsNotNull(modify_Manufactur_Result);
            Assert.That(modify_Manufactur, Is.EqualTo(modify_Manufactur_Result));
        }

        [Test]
        public void Testget_ScreenOrientation_code()
        {
            string Orientation;
            string get_ScreenOrientation_code;
            string get_ScreenOrientation_code_Result;

            // Case 01  Landscape
            Orientation = "Landscape";
            get_ScreenOrientation_code = "1";
            get_ScreenOrientation_code_Result = (string)privatetecLIDisplayPlugins.Invoke("get_ScreenOrientation_code", Orientation);
            Assert.IsNotNull(get_ScreenOrientation_code_Result);
            Assert.That(get_ScreenOrientation_code, Is.EqualTo(get_ScreenOrientation_code_Result));

            // Case 02 Portrait
            Orientation = "Portrait";
            get_ScreenOrientation_code = "2";
            get_ScreenOrientation_code_Result = (string)privatetecLIDisplayPlugins.Invoke("get_ScreenOrientation_code", Orientation);
            Assert.IsNotNull(get_ScreenOrientation_code_Result);
            Assert.That(get_ScreenOrientation_code, Is.EqualTo(get_ScreenOrientation_code_Result));

            // Case 03 Landscape_flipped
            Orientation = "Landscape_flipped";
            get_ScreenOrientation_code = "3";
            get_ScreenOrientation_code_Result = (string)privatetecLIDisplayPlugins.Invoke("get_ScreenOrientation_code", Orientation);
            Assert.IsNotNull(get_ScreenOrientation_code_Result);
            Assert.That(get_ScreenOrientation_code, Is.EqualTo(get_ScreenOrientation_code_Result));

            // Case 04 Portrait_flipped
            Orientation = "Portrait_flipped";
            get_ScreenOrientation_code = "4";
            get_ScreenOrientation_code_Result = (string)privatetecLIDisplayPlugins.Invoke("get_ScreenOrientation_code", Orientation);
            Assert.IsNotNull(get_ScreenOrientation_code_Result);
            Assert.That(get_ScreenOrientation_code, Is.EqualTo(get_ScreenOrientation_code_Result));

            // Case 05 default
            Orientation = "TestOrientation";
            get_ScreenOrientation_code = "1";
            get_ScreenOrientation_code_Result = (string)privatetecLIDisplayPlugins.Invoke("get_ScreenOrientation_code", Orientation);
            Assert.IsNotNull(get_ScreenOrientation_code_Result);
            Assert.That(get_ScreenOrientation_code, Is.EqualTo(get_ScreenOrientation_code_Result));
        }

        [Test]
        public void Testget_RangeLevel()
        {
            string level;
            string get_RangeLevel;
            string get_RangeLevel_Result;

            // Case 01  LOW
            level = "LOW";
            get_RangeLevel = "0";
            get_RangeLevel_Result = (string)privatetecLIDisplayPlugins.Invoke("get_RangeLevel", level);
            Assert.IsNotNull(get_RangeLevel_Result);
            Assert.That(get_RangeLevel, Is.EqualTo(get_RangeLevel_Result));

            // Case 02  MID
            level = "MID";
            get_RangeLevel = "1";
            get_RangeLevel_Result = (string)privatetecLIDisplayPlugins.Invoke("get_RangeLevel", level);
            Assert.IsNotNull(get_RangeLevel_Result);
            Assert.That(get_RangeLevel, Is.EqualTo(get_RangeLevel_Result));

            // Case 03  HIGH
            level = "HIGH";
            get_RangeLevel = "2";
            get_RangeLevel_Result = (string)privatetecLIDisplayPlugins.Invoke("get_RangeLevel", level);
            Assert.IsNotNull(get_RangeLevel_Result);
            Assert.That(get_RangeLevel, Is.EqualTo(get_RangeLevel_Result));

            // default
            level = "Testlevel";
            get_RangeLevel = "1";
            get_RangeLevel_Result = (string)privatetecLIDisplayPlugins.Invoke("get_RangeLevel", level);
            Assert.IsNotNull(get_RangeLevel_Result);
            Assert.That(get_RangeLevel, Is.EqualTo(get_RangeLevel_Result));
        }

        [Test]
        public void Testget_MicrophoneControl()
        {
            string status;
            int value;
            string get_MicrophoneControl;
            string get_MicrophoneControl_Result;

            // Case 01  OSDDISABLE,0x01
            status = "OSDDISABLE";
            value = 0x01;
            get_MicrophoneControl = "1";
            get_MicrophoneControl_Result = (string)privatetecLIDisplayPlugins.Invoke("get_MicrophoneControl", status, value);
            Assert.IsNotNull(get_MicrophoneControl_Result);
            Assert.That(get_MicrophoneControl, Is.EqualTo(get_MicrophoneControl_Result));

            // Case 02  OSDENABLE,0x02
            status = "OSDENABLE";
            value = 0x02;
            get_MicrophoneControl = "2";
            get_MicrophoneControl_Result = (string)privatetecLIDisplayPlugins.Invoke("get_MicrophoneControl", status, value);
            Assert.IsNotNull(get_MicrophoneControl_Result);
            Assert.That(get_MicrophoneControl, Is.EqualTo(get_MicrophoneControl_Result));

            // Case 03  unknown_command,0x02
            status = "TestMicrophoneControl";
            value = 0x03;
            get_MicrophoneControl = "unknown_command";
            get_MicrophoneControl_Result = (string)privatetecLIDisplayPlugins.Invoke("get_MicrophoneControl", status, value);
            Assert.IsNotNull(get_MicrophoneControl_Result);
            Assert.That(get_MicrophoneControl, Is.EqualTo(get_MicrophoneControl_Result));
        }

        [Test]
        public void Testget_MicrophoneControl_status()
        {
            int value;
            string get_MicrophoneControl_status;
            string get_MicrophoneControl_status_Result;

            // Case 01  OSDDISABLE,0x01
            value = 0x01;
            get_MicrophoneControl_status = "OSDDISABLE";
            get_MicrophoneControl_status_Result = (string)privatetecLIDisplayPlugins.Invoke("get_MicrophoneControl_status", value);
            Assert.IsNotNull(get_MicrophoneControl_status_Result);
            Assert.That(get_MicrophoneControl_status, Is.EqualTo(get_MicrophoneControl_status_Result));

            // Case 02  OSDENABLE,0x02
            value = 0x02;
            get_MicrophoneControl_status = "OSDENABLE";
            get_MicrophoneControl_status_Result = (string)privatetecLIDisplayPlugins.Invoke("get_MicrophoneControl_status", value);
            Assert.IsNotNull(get_MicrophoneControl_status_Result);
            Assert.That(get_MicrophoneControl_status, Is.EqualTo(get_MicrophoneControl_status_Result));

            // Case 03  unknown_command,0x02
            value = 0x03;
            get_MicrophoneControl_status = "";
            get_MicrophoneControl_status_Result = (string)privatetecLIDisplayPlugins.Invoke("get_MicrophoneControl_status", value);
            Assert.IsNotNull(get_MicrophoneControl_status_Result);
            Assert.That(get_MicrophoneControl_status, Is.EqualTo(get_MicrophoneControl_status_Result));
        }

        [Test]
        public void Testget_SpeakerVolume()
        {
            string status;
            int value;
            string get_MicrophoneControl;
            string get_MicrophoneControl_Result;

            // Case 01  OSDDISABLE,0x10
            status = "OSDDISABLE";
            value = 0x10;
            get_MicrophoneControl = "255";
            get_MicrophoneControl_Result = (string)privatetecLIDisplayPlugins.Invoke("get_SpeakerVolume", status, value);
            Assert.IsNotNull(get_MicrophoneControl_Result);
            Assert.That(get_MicrophoneControl, Is.EqualTo(get_MicrophoneControl_Result));

            // Case 02  OSDENABLE,0x20
            status = "OSDENABLE";
            value = 0x20;
            get_MicrophoneControl = "254";
            get_MicrophoneControl_Result = (string)privatetecLIDisplayPlugins.Invoke("get_SpeakerVolume", status, value);
            Assert.IsNotNull(get_MicrophoneControl_Result);
            Assert.That(get_MicrophoneControl, Is.EqualTo(get_MicrophoneControl_Result));

            // Case 03  unknown_command,0x30
            status = "TestMicrophoneControl";
            value = 0x30;
            get_MicrophoneControl = "unknown_command";
            get_MicrophoneControl_Result = (string)privatetecLIDisplayPlugins.Invoke("get_SpeakerVolume", status, value);
            Assert.IsNotNull(get_MicrophoneControl_Result);
            Assert.That(get_MicrophoneControl, Is.EqualTo(get_MicrophoneControl_Result));
        }

        [Test]
        public void Testget_SpeakerVolume_status()
        {
            int value;
            string get_SpeakerVolume_status;
            string get_SpeakerVolume_status_Result;

            // Case 01  OSDDISABLE,0x00FF
            value = 0x00FF;
            get_SpeakerVolume_status = "OSDDISABLE";
            get_SpeakerVolume_status_Result = (string)privatetecLIDisplayPlugins.Invoke("get_SpeakerVolume_status", value);
            Assert.IsNotNull(get_SpeakerVolume_status_Result);
            Assert.That(get_SpeakerVolume_status, Is.EqualTo(get_SpeakerVolume_status_Result));

            // Case 02  OSDENABLE,0x00FF
            value = 0x00FE;
            get_SpeakerVolume_status = "OSDENABLE";
            get_SpeakerVolume_status_Result = (string)privatetecLIDisplayPlugins.Invoke("get_SpeakerVolume_status", value);
            Assert.IsNotNull(get_SpeakerVolume_status_Result);
            Assert.That(get_SpeakerVolume_status, Is.EqualTo(get_SpeakerVolume_status_Result));

            // Case 03  OSDENABLE,Volume:52
            value = 0x1234;
            get_SpeakerVolume_status = "Volume:52";
            get_SpeakerVolume_status_Result = (string)privatetecLIDisplayPlugins.Invoke("get_SpeakerVolume_status", value);
            Assert.IsNotNull(get_SpeakerVolume_status_Result);
            Assert.That(get_SpeakerVolume_status, Is.EqualTo(get_SpeakerVolume_status_Result));
        }

        [Test]
        public void Testget_SpeakerMicrophone()
        {
            string status;
            int value;
            string get_SpeakerMicrophone;
            string get_SpeakerMicrophone_Result;

            // Case 01  OSDDISABLE,0x01
            status = "OSDDISABLE";
            value = 0x01;
            get_SpeakerMicrophone = "0x4001";
            get_SpeakerMicrophone_Result = (string)privatetecLIDisplayPlugins.Invoke("get_SpeakerMicrophone", status, value);
            Assert.IsNotNull(get_SpeakerMicrophone_Result);
            Assert.That(get_SpeakerMicrophone, Is.EqualTo(get_SpeakerMicrophone_Result));

            // Case 02  OSDENABLE,0x02
            status = "OSDENABLE";
            value = 0x02;
            get_SpeakerMicrophone = "0x2";
            get_SpeakerMicrophone_Result = (string)privatetecLIDisplayPlugins.Invoke("get_SpeakerMicrophone", status, value);
            Assert.IsNotNull(get_SpeakerMicrophone_Result);
            Assert.That(get_SpeakerMicrophone, Is.EqualTo(get_SpeakerMicrophone_Result));

            // Case 03  OSDUNLOCK,0x03
            status = "OSDUNLOCK";
            value = 0x03;
            get_SpeakerMicrophone = "0x3";
            get_SpeakerMicrophone_Result = (string)privatetecLIDisplayPlugins.Invoke("get_SpeakerMicrophone", status, value);
            Assert.IsNotNull(get_SpeakerMicrophone_Result);
            Assert.That(get_SpeakerMicrophone, Is.EqualTo(get_SpeakerMicrophone_Result));

            // Case 04  OSDLOCK,0x04
            status = "OSDLOCK";
            value = 0x04;
            get_SpeakerMicrophone = "0x8004";
            get_SpeakerMicrophone_Result = (string)privatetecLIDisplayPlugins.Invoke("get_SpeakerMicrophone", status, value);
            Assert.IsNotNull(get_SpeakerMicrophone_Result);
            Assert.That(get_SpeakerMicrophone, Is.EqualTo(get_SpeakerMicrophone_Result));

            // Case 05  OSDUNLOCK,OSDDISABLE,0x05
            status = "OSDUNLOCK,OSDDISABLE";
            value = 0x05;
            get_SpeakerMicrophone = "0x4005";
            get_SpeakerMicrophone_Result = (string)privatetecLIDisplayPlugins.Invoke("get_SpeakerMicrophone", status, value);
            Assert.IsNotNull(get_SpeakerMicrophone_Result);
            Assert.That(get_SpeakerMicrophone, Is.EqualTo(get_SpeakerMicrophone_Result));

            // Case 06  OSDUNLOCK,OSDENABLE,0x06
            status = "OSDUNLOCK,OSDENABLE";
            value = 0x06;
            get_SpeakerMicrophone = "0x6";
            get_SpeakerMicrophone_Result = (string)privatetecLIDisplayPlugins.Invoke("get_SpeakerMicrophone", status, value);
            Assert.IsNotNull(get_SpeakerMicrophone_Result);
            Assert.That(get_SpeakerMicrophone, Is.EqualTo(get_SpeakerMicrophone_Result));

            // Case 07  OSDLOCK,OSDDISABLE,0x07
            status = "OSDLOCK,OSDDISABLE";
            value = 0x07;
            get_SpeakerMicrophone = "0xC007";
            get_SpeakerMicrophone_Result = (string)privatetecLIDisplayPlugins.Invoke("get_SpeakerMicrophone", status, value);
            Assert.IsNotNull(get_SpeakerMicrophone_Result);
            Assert.That(get_SpeakerMicrophone, Is.EqualTo(get_SpeakerMicrophone_Result));

            // Case 08  OSDLOCK,OSDENABLE,0x08
            status = "OSDLOCK,OSDENABLE";
            value = 0x08;
            get_SpeakerMicrophone = "0x8008";
            get_SpeakerMicrophone_Result = (string)privatetecLIDisplayPlugins.Invoke("get_SpeakerMicrophone", status, value);
            Assert.IsNotNull(get_SpeakerMicrophone_Result);
            Assert.That(get_SpeakerMicrophone, Is.EqualTo(get_SpeakerMicrophone_Result));

            // Case 09  OSDUNLOCK,0x03
            status = "Test get_SpeakerMicrophone";
            value = 0x09;
            get_SpeakerMicrophone = "unknown_command";
            get_SpeakerMicrophone_Result = (string)privatetecLIDisplayPlugins.Invoke("get_SpeakerMicrophone", status, value);
            Assert.IsNotNull(get_SpeakerMicrophone_Result);
            Assert.That(get_SpeakerMicrophone, Is.EqualTo(get_SpeakerMicrophone_Result));
        }

        [Test]
        public void Testget_SpeakerMicrophone_status()
        {
            int value;
            string get_SpeakerMicrophone_status;
            string get_SpeakerMicrophone_status_Result;

            // Case 01  OSDLOCK,OSDDISABLE,0x8000
            value = 0x8000;
            get_SpeakerMicrophone_status = "OSDLOCK,OSDENABLE";
            get_SpeakerMicrophone_status_Result = (string)privatetecLIDisplayPlugins.Invoke("get_SpeakerMicrophone_status", value);
            Assert.IsNotNull(get_SpeakerMicrophone_status_Result);
            Assert.That(get_SpeakerMicrophone_status, Is.EqualTo(get_SpeakerMicrophone_status_Result));

            // Case 01  OSDUNLOCK,OSDENABLE,0x4000
            value = 0x4000;
            get_SpeakerMicrophone_status = "OSDUNLOCK,OSDDISABLE";
            get_SpeakerMicrophone_status_Result = (string)privatetecLIDisplayPlugins.Invoke("get_SpeakerMicrophone_status", value);
            Assert.IsNotNull(get_SpeakerMicrophone_status_Result);
            Assert.That(get_SpeakerMicrophone_status, Is.EqualTo(get_SpeakerMicrophone_status_Result));
        }

        [Test]
        public void Testget_Uniformity()
        {
            string status;
            string get_Uniformity;
            string get_Uniformity_Result;

            // Case 01 OFF
            status = "off";
            get_Uniformity = "0";
            get_Uniformity_Result = (string)privatetecLIDisplayPlugins.Invoke("get_Uniformity", status);
            Assert.IsNotNull(get_Uniformity_Result);
            Assert.That(get_Uniformity, Is.EqualTo(get_Uniformity_Result));

            // Case 02 high
            status = "high";
            get_Uniformity = "1";
            get_Uniformity_Result = (string)privatetecLIDisplayPlugins.Invoke("get_Uniformity", status);
            Assert.IsNotNull(get_Uniformity_Result);
            Assert.That(get_Uniformity, Is.EqualTo(get_Uniformity_Result));

            // Case 03 Low
            status = "Low";
            get_Uniformity = "2";
            get_Uniformity_Result = (string)privatetecLIDisplayPlugins.Invoke("get_Uniformity", status);
            Assert.IsNotNull(get_Uniformity_Result);
            Assert.That(get_Uniformity, Is.EqualTo(get_Uniformity_Result));

            // Case 04 on
            status = "on";
            get_Uniformity = "2";
            get_Uniformity_Result = (string)privatetecLIDisplayPlugins.Invoke("get_Uniformity", status);
            Assert.IsNotNull(get_Uniformity_Result);
            Assert.That(get_Uniformity, Is.EqualTo(get_Uniformity_Result));

            // default
            status = "Test get_Uniformity";
            get_Uniformity = "0";
            get_Uniformity_Result = (string)privatetecLIDisplayPlugins.Invoke("get_Uniformity", status);
            Assert.IsNotNull(get_Uniformity_Result);
            Assert.That(get_Uniformity, Is.EqualTo(get_Uniformity_Result));
        }

        [Test]
        public void Testget_ColorManagement()
        {
            string priority;
            ColorManagementRunType get_ColorManagement;
            ColorManagementRunType get_ColorManagement_Result;

            // Case 01 OFF
            priority = "OFF";
            get_ColorManagement = ColorManagementRunType.Off;
            get_ColorManagement_Result = (ColorManagementRunType)privatetecLIDisplayPlugins.Invoke("get_ColorManagement", priority);
            Assert.IsNotNull(get_ColorManagement_Result);
            Assert.That(get_ColorManagement, Is.EqualTo(get_ColorManagement_Result));

            // Case 02 BYMONITOR
            priority = "BYMONITOR";
            get_ColorManagement = ColorManagementRunType.Bymonitor;
            get_ColorManagement_Result = (ColorManagementRunType)privatetecLIDisplayPlugins.Invoke("get_ColorManagement", priority);
            Assert.IsNotNull(get_ColorManagement_Result);
            Assert.That(get_ColorManagement, Is.EqualTo(get_ColorManagement_Result));

            // Case 03 BYHOST
            priority = "BYHOST";
            get_ColorManagement = ColorManagementRunType.Byhost;
            get_ColorManagement_Result = (ColorManagementRunType)privatetecLIDisplayPlugins.Invoke("get_ColorManagement", priority);
            Assert.IsNotNull(get_ColorManagement_Result);
            Assert.That(get_ColorManagement, Is.EqualTo(get_ColorManagement_Result));

            // default
            priority = "Test priority";
            get_ColorManagement = ColorManagementRunType.Off;
            get_ColorManagement_Result = (ColorManagementRunType)privatetecLIDisplayPlugins.Invoke("get_ColorManagement", priority);
            Assert.IsNotNull(get_ColorManagement_Result);
            Assert.That(get_ColorManagement, Is.EqualTo(get_ColorManagement_Result));
        }

        [Test]
        public void Testget_USBCPrioritization()
        {
            string priority;
            USBCPrioritizationType get_USBCPrioritization;
            USBCPrioritizationType get_USBCPrioritization_Result;

            // Case 01 High Speed
            priority = "High Speed";
            get_USBCPrioritization = USBCPrioritizationType.HighDataSpeed;
            get_USBCPrioritization_Result = (USBCPrioritizationType)privatetecLIDisplayPlugins.Invoke("get_USBCPrioritization", priority);
            Assert.IsNotNull(get_USBCPrioritization_Result);
            Assert.That(get_USBCPrioritization, Is.EqualTo(get_USBCPrioritization_Result));

            // Case 02 High Data Speed
            priority = "High Data Speed";
            get_USBCPrioritization = USBCPrioritizationType.HighDataSpeed;
            get_USBCPrioritization_Result = (USBCPrioritizationType)privatetecLIDisplayPlugins.Invoke("get_USBCPrioritization", priority);
            Assert.IsNotNull(get_USBCPrioritization_Result);
            Assert.That(get_USBCPrioritization, Is.EqualTo(get_USBCPrioritization_Result));

            // Case 03 High Resolution
            priority = "High Resolution";
            get_USBCPrioritization = USBCPrioritizationType.HighResolution;
            get_USBCPrioritization_Result = (USBCPrioritizationType)privatetecLIDisplayPlugins.Invoke("get_USBCPrioritization", priority);
            Assert.IsNotNull(get_USBCPrioritization_Result);
            Assert.That(get_USBCPrioritization, Is.EqualTo(get_USBCPrioritization_Result));

            // default
            priority = "Test priority";
            get_USBCPrioritization = USBCPrioritizationType.Unknow;
            get_USBCPrioritization_Result = (USBCPrioritizationType)privatetecLIDisplayPlugins.Invoke("get_USBCPrioritization", priority);
            Assert.IsNotNull(get_USBCPrioritization_Result);
            Assert.That(get_USBCPrioritization, Is.EqualTo(get_USBCPrioritization_Result));
        }

        [Test]
        public void Testget_PowerNapType_code()
        {
            string code;
            PowerNapType get_PowerNapType_code;
            PowerNapType get_PowerNapType_code_Result;

            // Case 01 Off
            code = "Off";
            get_PowerNapType_code = PowerNapType.Off;
            get_PowerNapType_code_Result = (PowerNapType)privatetecLIDisplayPlugins.Invoke("get_PowerNapType_code", code);
            Assert.IsNotNull(get_PowerNapType_code_Result);
            Assert.That(get_PowerNapType_code, Is.EqualTo(get_PowerNapType_code_Result));

            // Case 02 REDUCEBRIGHTNESS
            code = "REDUCEBRIGHTNESS";
            get_PowerNapType_code = PowerNapType.ReduceBrightness;
            get_PowerNapType_code_Result = (PowerNapType)privatetecLIDisplayPlugins.Invoke("get_PowerNapType_code", code);
            Assert.IsNotNull(get_PowerNapType_code_Result);
            Assert.That(get_PowerNapType_code, Is.EqualTo(get_PowerNapType_code_Result));

            // Case 03 SLEEP
            code = "SLEEP";
            get_PowerNapType_code = PowerNapType.SleepIfRunning;
            get_PowerNapType_code_Result = (PowerNapType)privatetecLIDisplayPlugins.Invoke("get_PowerNapType_code", code);
            Assert.IsNotNull(get_PowerNapType_code_Result);
            Assert.That(get_PowerNapType_code, Is.EqualTo(get_PowerNapType_code_Result));

            // default
            code = "Test get_PowerNapType_code";
            get_PowerNapType_code = PowerNapType.SleepIfRunning;
            get_PowerNapType_code_Result = (PowerNapType)privatetecLIDisplayPlugins.Invoke("get_PowerNapType_code", code);
            Assert.IsNotNull(get_PowerNapType_code_Result);
            Assert.That(get_PowerNapType_code, Is.EqualTo(get_PowerNapType_code_Result));
        }

        [Test]
        public void Testget_InputSource_code()
        {
            string input;
            string get_InputSource_code;
            string get_InputSource_code_Result;

            // Case "VGA-1"
            input = "VGA1";
            get_InputSource_code = "0x01";
            get_InputSource_code_Result = (string)privatetecLIDisplayPlugins.Invoke("get_InputSource_code", input);
            Assert.IsNotNull(get_InputSource_code_Result);
            Assert.That(get_InputSource_code, Is.EqualTo(get_InputSource_code_Result));

            // Case "VGA-2"
            input = "VGA2";
            get_InputSource_code = "0x02";
            get_InputSource_code_Result = (string)privatetecLIDisplayPlugins.Invoke("get_InputSource_code", input);
            Assert.IsNotNull(get_InputSource_code_Result);
            Assert.That(get_InputSource_code, Is.EqualTo(get_InputSource_code_Result));

            // Case "DVI-1"
            input = "DVI-1";
            get_InputSource_code = "0x03";
            get_InputSource_code_Result = (string)privatetecLIDisplayPlugins.Invoke("get_InputSource_code", input);
            Assert.IsNotNull(get_InputSource_code_Result);
            Assert.That(get_InputSource_code, Is.EqualTo(get_InputSource_code_Result));

            // Case "DVI-2"
            input = "DVI-2";
            get_InputSource_code = "0x04";
            get_InputSource_code_Result = (string)privatetecLIDisplayPlugins.Invoke("get_InputSource_code", input);
            Assert.IsNotNull(get_InputSource_code_Result);
            Assert.That(get_InputSource_code, Is.EqualTo(get_InputSource_code_Result));

            // Case "Composite video 1"
            input = "Composite video 1";
            get_InputSource_code = "0x05";
            get_InputSource_code_Result = (string)privatetecLIDisplayPlugins.Invoke("get_InputSource_code", input);
            Assert.IsNotNull(get_InputSource_code_Result);
            Assert.That(get_InputSource_code, Is.EqualTo(get_InputSource_code_Result));

            // Case "Composite video 2"
            input = "Composite video 2";
            get_InputSource_code = "0x06";
            get_InputSource_code_Result = (string)privatetecLIDisplayPlugins.Invoke("get_InputSource_code", input);
            Assert.IsNotNull(get_InputSource_code_Result);
            Assert.That(get_InputSource_code, Is.EqualTo(get_InputSource_code_Result));

            // Case "S-Video-1"
            input = "S-Video-1";
            get_InputSource_code = "0x07";
            get_InputSource_code_Result = (string)privatetecLIDisplayPlugins.Invoke("get_InputSource_code", input);
            Assert.IsNotNull(get_InputSource_code_Result);
            Assert.That(get_InputSource_code, Is.EqualTo(get_InputSource_code_Result));

            // Case "S-Video-2"
            input = "S-Video-2";
            get_InputSource_code = "0x08";
            get_InputSource_code_Result = (string)privatetecLIDisplayPlugins.Invoke("get_InputSource_code", input);
            Assert.IsNotNull(get_InputSource_code_Result);
            Assert.That(get_InputSource_code, Is.EqualTo(get_InputSource_code_Result));

            // Case "Tuner-1"
            input = "Tuner-1";
            get_InputSource_code = "0x09";
            get_InputSource_code_Result = (string)privatetecLIDisplayPlugins.Invoke("get_InputSource_code", input);
            Assert.IsNotNull(get_InputSource_code_Result);
            Assert.That(get_InputSource_code, Is.EqualTo(get_InputSource_code_Result));

            // Case "Tuner-2"
            input = "Tuner-2";
            get_InputSource_code = "0x0a";
            get_InputSource_code_Result = (string)privatetecLIDisplayPlugins.Invoke("get_InputSource_code", input);
            Assert.IsNotNull(get_InputSource_code_Result);
            Assert.That(get_InputSource_code, Is.EqualTo(get_InputSource_code_Result));

            // Case "Tuner-3"
            input = "Tuner-3";
            get_InputSource_code = "0x0b";
            get_InputSource_code_Result = (string)privatetecLIDisplayPlugins.Invoke("get_InputSource_code", input);
            Assert.IsNotNull(get_InputSource_code_Result);
            Assert.That(get_InputSource_code, Is.EqualTo(get_InputSource_code_Result));

            // Case "Component video (YPrPb/YCrCb) 1"
            input = "Component video (YPrPb/YCrCb) 1";
            get_InputSource_code = "0x0c";
            get_InputSource_code_Result = (string)privatetecLIDisplayPlugins.Invoke("get_InputSource_code", input);
            Assert.IsNotNull(get_InputSource_code_Result);
            Assert.That(get_InputSource_code, Is.EqualTo(get_InputSource_code_Result));

            // Case "Component video (YPrPb/YCrCb) 2"
            input = "Component video (YPrPb/YCrCb) 2";
            get_InputSource_code = "0x0d";
            get_InputSource_code_Result = (string)privatetecLIDisplayPlugins.Invoke("get_InputSource_code", input);
            Assert.IsNotNull(get_InputSource_code_Result);
            Assert.That(get_InputSource_code, Is.EqualTo(get_InputSource_code_Result));

            // Case "Component video (YPrPb/YCrCb) 3"
            input = "Component video (YPrPb/YCrCb) 3";
            get_InputSource_code = "0x0e";
            get_InputSource_code_Result = (string)privatetecLIDisplayPlugins.Invoke("get_InputSource_code", input);
            Assert.IsNotNull(get_InputSource_code_Result);
            Assert.That(get_InputSource_code, Is.EqualTo(get_InputSource_code_Result));

            // Case "DISPLAYPORT-1"
            input = "DISPLAYPORT1";
            get_InputSource_code = "0x0f";
            get_InputSource_code_Result = (string)privatetecLIDisplayPlugins.Invoke("get_InputSource_code", input);
            Assert.IsNotNull(get_InputSource_code_Result);
            Assert.That(get_InputSource_code, Is.EqualTo(get_InputSource_code_Result));

            // Case "Mini DisplayPort-1"
            input = "Mini DisplayPort-1";
            get_InputSource_code = "0x10";
            get_InputSource_code_Result = (string)privatetecLIDisplayPlugins.Invoke("get_InputSource_code", input);
            Assert.IsNotNull(get_InputSource_code_Result);
            Assert.That(get_InputSource_code, Is.EqualTo(get_InputSource_code_Result));

            // Case "HDMI-1"
            input = "HDMI1";
            get_InputSource_code = "0x11";
            get_InputSource_code_Result = (string)privatetecLIDisplayPlugins.Invoke("get_InputSource_code", input);
            Assert.IsNotNull(get_InputSource_code_Result);
            Assert.That(get_InputSource_code, Is.EqualTo(get_InputSource_code_Result));

            // Case "HDMI-2"
            input = "HDMI2";
            get_InputSource_code = "0x12";
            get_InputSource_code_Result = (string)privatetecLIDisplayPlugins.Invoke("get_InputSource_code", input);
            Assert.IsNotNull(get_InputSource_code_Result);
            Assert.That(get_InputSource_code, Is.EqualTo(get_InputSource_code_Result));

            // Case "DISPLAYPORT-2"
            input = "DISPLAYPORT2";
            get_InputSource_code = "0x13";
            get_InputSource_code_Result = (string)privatetecLIDisplayPlugins.Invoke("get_InputSource_code", input);
            Assert.IsNotNull(get_InputSource_code_Result);
            Assert.That(get_InputSource_code, Is.EqualTo(get_InputSource_code_Result));

            // Case "Mini DisplayPort-2"
            input = "Mini DisplayPort-2";
            get_InputSource_code = "0x14";
            get_InputSource_code_Result = (string)privatetecLIDisplayPlugins.Invoke("get_InputSource_code", input);
            Assert.IsNotNull(get_InputSource_code_Result);
            Assert.That(get_InputSource_code, Is.EqualTo(get_InputSource_code_Result));

            // Case "HDMI3"
            input = "HDMI3";
            get_InputSource_code = "0x15";
            get_InputSource_code_Result = (string)privatetecLIDisplayPlugins.Invoke("get_InputSource_code", input);
            Assert.IsNotNull(get_InputSource_code_Result);
            Assert.That(get_InputSource_code, Is.EqualTo(get_InputSource_code_Result));

            // Case "HDMI4"
            input = "HDMI4";
            get_InputSource_code = "0x16";
            get_InputSource_code_Result = (string)privatetecLIDisplayPlugins.Invoke("get_InputSource_code", input);
            Assert.IsNotNull(get_InputSource_code_Result);
            Assert.That(get_InputSource_code, Is.EqualTo(get_InputSource_code_Result));

            // Case "DISPLAYPORT-3"
            input = "DISPLAYPORT3";
            get_InputSource_code = "0x17";
            get_InputSource_code_Result = (string)privatetecLIDisplayPlugins.Invoke("get_InputSource_code", input);
            Assert.IsNotNull(get_InputSource_code_Result);
            Assert.That(get_InputSource_code, Is.EqualTo(get_InputSource_code_Result));

            // Case "Mini DisplayPort-3"
            input = "Mini DisplayPort-3";
            get_InputSource_code = "0x18";
            get_InputSource_code_Result = (string)privatetecLIDisplayPlugins.Invoke("get_InputSource_code", input);
            Assert.IsNotNull(get_InputSource_code_Result);
            Assert.That(get_InputSource_code, Is.EqualTo(get_InputSource_code_Result));

            // Case "Thunderbolt-1"
            input = "Thunderbolt1";
            get_InputSource_code = "0x19";
            get_InputSource_code_Result = (string)privatetecLIDisplayPlugins.Invoke("get_InputSource_code", input);
            Assert.IsNotNull(get_InputSource_code_Result);
            Assert.That(get_InputSource_code, Is.EqualTo(get_InputSource_code_Result));

            // Case "Thunderbolt-2"
            input = "Thunderbolt2";
            get_InputSource_code = "0x1a";
            get_InputSource_code_Result = (string)privatetecLIDisplayPlugins.Invoke("get_InputSource_code", input);
            Assert.IsNotNull(get_InputSource_code_Result);
            Assert.That(get_InputSource_code, Is.EqualTo(get_InputSource_code_Result));

            // Case "USB-C1"
            input = "USB-C1";
            get_InputSource_code = "0x1b";
            get_InputSource_code_Result = (string)privatetecLIDisplayPlugins.Invoke("get_InputSource_code", input);
            Assert.IsNotNull(get_InputSource_code_Result);
            Assert.That(get_InputSource_code, Is.EqualTo(get_InputSource_code_Result));

            // Case "USB-C2"
            input = "USB-C2";
            get_InputSource_code = "0x1c";
            get_InputSource_code_Result = (string)privatetecLIDisplayPlugins.Invoke("get_InputSource_code", input);
            Assert.IsNotNull(get_InputSource_code_Result);
            Assert.That(get_InputSource_code, Is.EqualTo(get_InputSource_code_Result));

            // Case "USB-C3"
            input = "USB-C3";
            get_InputSource_code = "0x1d";
            get_InputSource_code_Result = (string)privatetecLIDisplayPlugins.Invoke("get_InputSource_code", input);
            Assert.IsNotNull(get_InputSource_code_Result);
            Assert.That(get_InputSource_code, Is.EqualTo(get_InputSource_code_Result));

            // Case "USB-C4"
            input = "USB-C4";
            get_InputSource_code = "0x1e";
            get_InputSource_code_Result = (string)privatetecLIDisplayPlugins.Invoke("get_InputSource_code", input);
            Assert.IsNotNull(get_InputSource_code_Result);
            Assert.That(get_InputSource_code, Is.EqualTo(get_InputSource_code_Result));

            // Case "USB Comm from USB1 (Type-B, port 1)"
            input = "USB Comm from USB1 (Type-B, port 1)";
            get_InputSource_code = "0x80";
            get_InputSource_code_Result = (string)privatetecLIDisplayPlugins.Invoke("get_InputSource_code", input);
            Assert.IsNotNull(get_InputSource_code_Result);
            Assert.That(get_InputSource_code, Is.EqualTo(get_InputSource_code_Result));

            // Case "USB Comm from USB2 (Type-B, port 2)"
            input = "USB Comm from USB2 (Type-B, port 2)";
            get_InputSource_code = "0x81";
            get_InputSource_code_Result = (string)privatetecLIDisplayPlugins.Invoke("get_InputSource_code", input);
            Assert.IsNotNull(get_InputSource_code_Result);
            Assert.That(get_InputSource_code, Is.EqualTo(get_InputSource_code_Result));

            // Case "USB Comm from USB-C1 (Type-C, port 1)"
            input = "USB Comm from USB-C1 (Type-C, port 1)";
            get_InputSource_code = "0x82";
            get_InputSource_code_Result = (string)privatetecLIDisplayPlugins.Invoke("get_InputSource_code", input);
            Assert.IsNotNull(get_InputSource_code_Result);
            Assert.That(get_InputSource_code, Is.EqualTo(get_InputSource_code_Result));

            // Case "USB Comm from USB-C2 (Type-C, port 2)"
            input = "USB Comm from USB-C2 (Type-C, port 2)";
            get_InputSource_code = "0x83";
            get_InputSource_code_Result = (string)privatetecLIDisplayPlugins.Invoke("get_InputSource_code", input);
            Assert.IsNotNull(get_InputSource_code_Result);
            Assert.That(get_InputSource_code, Is.EqualTo(get_InputSource_code_Result));

            // Case "USB Comm from USB-C3 (Type-C, port 3)"
            input = "USB Comm from USB-C3 (Type-C, port 3)";
            get_InputSource_code = "0x84";
            get_InputSource_code_Result = (string)privatetecLIDisplayPlugins.Invoke("get_InputSource_code", input);
            Assert.IsNotNull(get_InputSource_code_Result);
            Assert.That(get_InputSource_code, Is.EqualTo(get_InputSource_code_Result));

            // Case "USB Comm from USB-C4 (Type-C, port 4)"
            input = "USB Comm from USB-C4 (Type-C, port 4)";
            get_InputSource_code = "0x85";
            get_InputSource_code_Result = (string)privatetecLIDisplayPlugins.Invoke("get_InputSource_code", input);
            Assert.IsNotNull(get_InputSource_code_Result);
            Assert.That(get_InputSource_code, Is.EqualTo(get_InputSource_code_Result));

            // Default case
            input = "UnknownInput";
            get_InputSource_code = "0x11";
            get_InputSource_code_Result = (string)privatetecLIDisplayPlugins.Invoke("get_InputSource_code", input);
            Assert.IsNotNull(get_InputSource_code_Result);
            Assert.That(get_InputSource_code, Is.EqualTo(get_InputSource_code_Result));
        }

        [Test]
        public void TestSetCurrentInput()
        {
            IDeviceManagerSA? devMgr;
            devMgr = null;
            string index = "0";
            string input = "INPUT1";
            int null_device_manager = 1;
            if (devMgr == null)
            {
                var SetCurrentInput_Result1 = (Task<int>)privatetecLIDisplayPlugins.Invoke("SetCurrentInput", devMgr, index, input); //devMgr == null
                Assert.IsNotNull(SetCurrentInput_Result1);
                Assert.That(null_device_manager, Is.EqualTo(SetCurrentInput_Result1.Result));
            }

            string input2 = monitorInfo1.inputSource;
            int SetCurrentInput = 0;
            Mock<IDeviceManagerSA> devMgr2 = new Mock<IDeviceManagerSA>();
            List<MonitorInfo> _allInfoMonitors = new List<MonitorInfo>();
            _allInfoMonitors.Add(monitorInfo1);
            devMgr2.Setup(m => m.GetMonitors()).Returns(Task.FromResult(_allInfoMonitors));
            devMgr2.Setup(m => m.SetVCPCapability(It.IsAny<MonitorInfo>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Priority>())).Returns(Task.FromResult(true));
            if (devMgr2 != null)
            {
                var SetCurrentInput_Result2 = (Task<int>)privatetecLIDisplayPlugins.Invoke("SetCurrentInput", devMgr2.Object, index, input); //devMgr2 != null
                Assert.IsNotNull(SetCurrentInput_Result2);
                Assert.That(SetCurrentInput, Is.EqualTo(SetCurrentInput_Result2.Result));
            }
        }

        [Test]
        public void Testget_PowerSetting_code()
        {
            string code;
            string get_PowerSetting_code;
            string get_PowerSetting_code_Result;

            // Case "On"
            code = "On";
            get_PowerSetting_code = "0";
            get_PowerSetting_code_Result = (string)privatetecLIDisplayPlugins.Invoke("get_PowerSetting_code", code);
            Assert.IsNotNull(get_PowerSetting_code_Result);
            Assert.That(get_PowerSetting_code, Is.EqualTo(get_PowerSetting_code_Result));

            // Case "Off"
            code = "Off";
            get_PowerSetting_code = "1";
            get_PowerSetting_code_Result = (string)privatetecLIDisplayPlugins.Invoke("get_PowerSetting_code", code);
            Assert.IsNotNull(get_PowerSetting_code_Result);
            Assert.That(get_PowerSetting_code, Is.EqualTo(get_PowerSetting_code_Result));

            // Case "STANDBY"
            code = "STANDBY";
            get_PowerSetting_code = "2";
            get_PowerSetting_code_Result = (string)privatetecLIDisplayPlugins.Invoke("get_PowerSetting_code", code);
            Assert.IsNotNull(get_PowerSetting_code_Result);
            Assert.That(get_PowerSetting_code, Is.EqualTo(get_PowerSetting_code_Result));

            // default
            code = "Test get_PowerSetting_code";
            get_PowerSetting_code = "unknown_command";
            get_PowerSetting_code_Result = (string)privatetecLIDisplayPlugins.Invoke("get_PowerSetting_code", code);
            Assert.IsNotNull(get_PowerSetting_code_Result);
            Assert.That(get_PowerSetting_code, Is.EqualTo(get_PowerSetting_code_Result));
        }

        [Test]
        public void Testget_Energysaver_code()
        {
            string code;
            string get_Energysaver_code;
            string get_Energysaver_code_Result;

            // Case "Off"
            code = "Off";
            get_Energysaver_code = "0";
            get_Energysaver_code_Result = (string)privatetecLIDisplayPlugins.Invoke("get_Energysaver_code", code);
            Assert.IsNotNull(get_Energysaver_code_Result);
            Assert.That(get_Energysaver_code, Is.EqualTo(get_Energysaver_code_Result));

            // Case "On"
            code = "On";
            get_Energysaver_code = "4";
            get_Energysaver_code_Result = (string)privatetecLIDisplayPlugins.Invoke("get_Energysaver_code", code);
            Assert.IsNotNull(get_Energysaver_code_Result);
            Assert.That(get_Energysaver_code, Is.EqualTo(get_Energysaver_code_Result));

            // Case "OFF,LOCK"
            code = "OFF,LOCK";
            get_Energysaver_code = "8";
            get_Energysaver_code_Result = (string)privatetecLIDisplayPlugins.Invoke("get_Energysaver_code", code);
            Assert.IsNotNull(get_Energysaver_code_Result);
            Assert.That(get_Energysaver_code, Is.EqualTo(get_Energysaver_code_Result));

            // Case "ON,LOCK"
            code = "ON,LOCK";
            get_Energysaver_code = "12";
            get_Energysaver_code_Result = (string)privatetecLIDisplayPlugins.Invoke("get_Energysaver_code", code);
            Assert.IsNotNull(get_Energysaver_code_Result);
            Assert.That(get_Energysaver_code, Is.EqualTo(get_Energysaver_code_Result));

            // default
            code = "Test get_Energysaver_code";
            get_Energysaver_code = "unknown_command";
            get_Energysaver_code_Result = (string)privatetecLIDisplayPlugins.Invoke("get_Energysaver_code", code);
            Assert.IsNotNull(get_Energysaver_code_Result);
            Assert.That(get_Energysaver_code, Is.EqualTo(get_Energysaver_code_Result));
        }

        [Test]
        public void Testget_PowerNapType_code_ToUpper()
        {
            string code;
            string get_PowerNapType_code_ToUpper;
            string get_PowerNapType_code_ToUpper_Result;

            // Case "Off"
            code = "Off";
            get_PowerNapType_code_ToUpper = "OFF";
            get_PowerNapType_code_ToUpper_Result = (string)privatetecLIDisplayPlugins.Invoke("get_PowerNapType_code_ToUpper", code);
            Assert.IsNotNull(get_PowerNapType_code_ToUpper_Result);
            Assert.That(get_PowerNapType_code_ToUpper, Is.EqualTo(get_PowerNapType_code_ToUpper_Result));

            // Case "REDUCEBRIGHTNESS"
            code = "REDUCEBRIGHTNESS";
            get_PowerNapType_code_ToUpper = "REDUCE";
            get_PowerNapType_code_ToUpper_Result = (string)privatetecLIDisplayPlugins.Invoke("get_PowerNapType_code_ToUpper", code);
            Assert.IsNotNull(get_PowerNapType_code_ToUpper_Result);
            Assert.That(get_PowerNapType_code_ToUpper, Is.EqualTo(get_PowerNapType_code_ToUpper_Result));

            // Case "SLEEP"
            code = "SLEEP";
            get_PowerNapType_code_ToUpper = "SLEEP";
            get_PowerNapType_code_ToUpper_Result = (string)privatetecLIDisplayPlugins.Invoke("get_PowerNapType_code_ToUpper", code);
            Assert.IsNotNull(get_PowerNapType_code_ToUpper_Result);
            Assert.That(get_PowerNapType_code_ToUpper, Is.EqualTo(get_PowerNapType_code_ToUpper_Result));

            // default
            code = "Test get_PowerNapType_code_ToUpper";
            get_PowerNapType_code_ToUpper = "SLEEP";
            get_PowerNapType_code_ToUpper_Result = (string)privatetecLIDisplayPlugins.Invoke("get_PowerNapType_code_ToUpper", code);
            Assert.IsNotNull(get_PowerNapType_code_ToUpper_Result);
            Assert.That(get_PowerNapType_code_ToUpper, Is.EqualTo(get_PowerNapType_code_ToUpper_Result));
        }

        [Test]
        public void TestSetCommandArgs()
        {
            Mock<IDeviceManagerSA> devMgr = new Mock<IDeviceManagerSA>();
            List<MonitorInfo> _allInfoMonitors = new List<MonitorInfo>();
            _allInfoMonitors.Add(monitorInfo1);

            DeviceHelper deviceHelper = new DeviceHelper
            {
                deviceInfo = new List<DeviceInfo>
                {
                new DeviceInfo {LogicalDeviceType = "LogicalHeadset",DeviceName = "LogicalHeadset",},
                new DeviceInfo {LogicalDeviceType = "LogicalWiredAudio",DeviceName = "LogicalWiredAudio",}
                }
            };
            string path = "TestPath";
            SWUpdateInfoPackage swUpdateInfoPackage = new SWUpdateInfoPackage() { SWUpdateInfo = new List<SWUpdateInfo>() { new SWUpdateInfo() { SoftwareName = "TestSoftwareName", SoftwareVersion = "1.0", TheLatestVersion = "1.0", NeedUpdated = false, } }, };
            List<SWUpdateInfo> SWUpdateInfo = new List<SWUpdateInfo>() { new SWUpdateInfo() { SoftwareName = "TestSoftwareName", SoftwareVersion = "1.0", TheLatestVersion = "1.0", NeedUpdated = false, } };
            DDPMSettings data = new DDPMSettings(new DDPMAppSettings(), new DDPMUserSettings(), new DDPMITConfig());
            Dictionary<string, InputInfo> inputList = new Dictionary<string, InputInfo>();
            inputList.Add("HDMI 1", new InputInfo { InputName = "HDMI-1", USBUpstream = "Upstream 1" });
            DisplayPropertiesInfo displayPropertiesInfo = new DisplayPropertiesInfo() { DisplayName = monitorInfo1.DisplayName, isHDREnable = false, SupportedHDR = false, SupportedProperties = new DisplaySupportedProperties() };
            string GetOSDOrientation = "Horizontal";
            string ReadCurrentColorPreset = "Standard";
            bool WriteColorPreset = true;
            ALSConfig aLSConfig = new ALSConfig() { DisplayName = monitorInfo1.DisplayName, result = true, isSupportALS = 0, };
            string MonitorProfile = "Test MonitorProfile";
            bool WriteColorPresetByColorProfile = true;
            string GetColorManagementStatus = "Standard";
            bool SetVCPCapability = true;
            ObjGetVCP objGetVCP = new ObjGetVCP() { result = true, value = "02" };  //unlock
            UInt16[] ushorts = new UInt16[1];
            ObjGetVCP objGetPxpMode = new ObjGetVCP() { result = true, value = "0" };  //off", 0x00, "PIP/PBP off, full screen"
            List<PowerNapSetting> powerNapSettings = new List<PowerNapSetting>() { new PowerNapSetting() { ModelName = monitorInfo1.modelName, RunType = PowerNapType.Off, SerialNumber = monitorInfo1.edid.SerialNumber, ServiceTag=monitorInfo1.edid.ServiceTag, Status = false } };
            DDPMSettings ddpmSettings = new DDPMSettings(new DDPMAppSettings(), new DDPMUserSettings(), new DDPMITConfig());
            bool SetAppConfigDataDDPMSettings = true;
            bool AutoSetColorPresetForMonitorConfig = true;
            string GetVCPCapabilities = @"{""CapsDataMap"":{""On Screen Display Language"":[""English""]}}";
            ObjGetVCP objGetGetEAFunctionEnabled = new ObjGetVCP() { result = true, value = "02" };
            bool objGetOnUSBKVM = true;
            bool DisplayExportSettings = true;

            devMgr.Setup(m => m.GetMonitors()).Returns(Task.FromResult(_allInfoMonitors));
            devMgr.Setup(m => m.GetDevices(It.IsAny<bool>())).Returns(Task.FromResult(deviceHelper));
            devMgr.Setup(m => m.ReloadAppConfigData(It.IsAny<bool>())).Returns(Task.FromResult(data));
            devMgr.Setup(m => m.GetInputSourcelist(It.IsAny<MonitorInfo>())).Returns(Task.FromResult(inputList));
            devMgr.Setup(m => m.GetDisplayPropertiesInfo(It.IsAny<MonitorInfo>())).Returns(Task.FromResult(displayPropertiesInfo));
            devMgr.Setup(m => m.GetOSDOrientation(It.IsAny<MonitorInfo>())).Returns(Task.FromResult(GetOSDOrientation));
            devMgr.Setup(m => m.ReadCurrentColorPreset(It.IsAny<MonitorInfo>(), It.IsAny<Guid>(), It.IsAny<Priority>())).Returns(Task.FromResult(ReadCurrentColorPreset));
            devMgr.Setup(m => m.WriteColorPreset(It.IsAny<MonitorInfo>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<bool>())).Returns(Task.FromResult(WriteColorPreset));
            devMgr.Setup(m => m.GetALSFeatureValue(It.IsAny<MonitorInfo>(), It.IsAny<ALSFeatureQueryType>(), It.IsAny<int>())).Returns(Task.FromResult(aLSConfig));
            devMgr.Setup(m => m.GetMonitorProfile(It.IsAny<MonitorInfo>())).Returns(Task.FromResult(MonitorProfile));
            devMgr.Setup(m => m.WriteColorPresetByColorProfile(It.IsAny<MonitorInfo>(), It.IsAny<string>())).Returns(Task.FromResult(WriteColorPresetByColorProfile));
            devMgr.Setup(m => m.GetColorManagementStatus(It.IsAny<MonitorInfo>())).Returns(Task.FromResult(GetColorManagementStatus));
            devMgr.Setup(m => m.SetVCPCapability(It.IsAny<MonitorInfo>(), It.IsAny<byte>(), It.IsAny<uint>(), It.IsAny<Guid>(), It.IsAny<Priority>())).Returns(Task.FromResult(SetVCPCapability));
            devMgr.Setup(m => m.GetVCPCapability(It.IsAny<MonitorInfo>(), It.IsAny<byte>(), It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<Priority>())).Returns(Task.FromResult(objGetVCP));
            devMgr.Setup(m => m.GetPipPbpCapabilitiesWords(It.IsAny<MonitorInfo>())).Returns(Task.FromResult(ushorts));
            devMgr.Setup(m => m.GetPxpMode(It.IsAny<MonitorInfo>())).Returns(Task.FromResult(objGetPxpMode)); //off", 0x00, "PIP/PBP off, full screen"
            devMgr.Setup(m => m.ReadPowerNapSettings()).Returns(Task.FromResult(powerNapSettings));
            devMgr.Setup(m => m.ReloadAppConfigData(It.IsAny<bool>())).Returns(Task.FromResult(ddpmSettings));
            devMgr.Setup(m => m.SetAppConfigData(It.IsAny<DDPMSettings>())).Returns(Task.FromResult(SetAppConfigDataDDPMSettings));
            devMgr.Setup(m => m.AutoSetColorPresetForMonitorConfig(It.IsAny<MonitorInfo>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>())).Returns(Task.FromResult(AutoSetColorPresetForMonitorConfig));
            devMgr.Setup(m => m.GetVCPCapabilities(It.IsAny<MonitorInfo>(), It.IsAny<Guid>(), It.IsAny<Priority>())).Returns(Task.FromResult(GetVCPCapabilities));
            //Robert_Lin, 2025-1-7 comment-out due to GetEAFunctionEnabled() is deleted.
            //devMgr.Setup(m => m.GetEAFunctionEnabled()).Returns(Task.FromResult(objGetGetEAFunctionEnabled));
            devMgr.Setup(m => m.GetOnUSBKVM(It.IsAny<MonitorInfo>())).Returns(Task.FromResult(objGetOnUSBKVM));
            devMgr.Setup(m => m.GetNKVMStatus()).Returns(Task.FromResult(Task.CompletedTask));
            devMgr.Setup(m => m.DisplayExportSettings(It.IsAny<MonitorInfo>(), It.IsAny<string>())).Returns(Task.FromResult(DisplayExportSettings));

            var devMgrObj = devMgr.Object;

            CommandLineInput commandLineInput;
            CLIEventArgs input1 = new CLIEventArgs()
            {
                commandLineInput = new CommandLineInput()
                {
                    PluginsType = "AUDIO",
                    Command = "GET",
                    TargetType = "APP",
                    TargetFeature = "CONNECTEDDEVICES",
                    isCliRunAdmin = false,
                },
                command_guid_string = "test_guid"
            };
            commandLineInput = input1.commandLineInput;
            if (commandLineInput.TargetFeature == "CONNECTEDDEVICES")
            {
                var SetCommandArgs_Result1 = cLIDisplayPlugins.SetCommandArgs(input1, devMgrObj);  //TargetFeature CONNECTEDDEVICES, TargetType = "APP",Command = "GET",
                Assert.IsNotNull(SetCommandArgs_Result1);
                Assert.IsNotNull(SetCommandArgs_Result1.command_guid_string);
                Assert.IsNotNull(SetCommandArgs_Result1.serialize_Json_response);
            }

            CLIEventArgs input2 = new CLIEventArgs()
            {
                commandLineInput = new CommandLineInput()
                {
                    PluginsType = "AUDIO",
                    Command = "GET",
                    TargetType = "TestAPP",
                    TargetFeature = "CONNECTEDDEVICES",
                    isCliRunAdmin = false,
                },
                command_guid_string = "test_guid"
            };
            commandLineInput = input2.commandLineInput;
            if (commandLineInput.TargetFeature == "CONNECTEDDEVICES")
            {
                var SetCommandArgs_Result2 = cLIDisplayPlugins.SetCommandArgs(input2, devMgrObj);  //TargetFeature CONNECTEDDEVICES, TargetType = "TestAPP",Command = "GET", Not APP
                Assert.IsNotNull(SetCommandArgs_Result2);
                Assert.IsNotNull(SetCommandArgs_Result2.command_guid_string);
                Assert.IsNotNull(SetCommandArgs_Result2.serialize_Json_response);
            }

            CLIEventArgs input3 = new CLIEventArgs()
            {
                commandLineInput = new CommandLineInput()
                {
                    PluginsType = "AUDIO",
                    Command = "SET",
                    TargetType = "APP",
                    TargetFeature = "DETECTMONITORS",
                    isCliRunAdmin = false,
                },
                command_guid_string = "test_guid"
            };
            commandLineInput = input3.commandLineInput;
            if (commandLineInput.TargetFeature == "DETECTMONITORS")
            {
                var SetCommandArgs_Result3 = cLIDisplayPlugins.SetCommandArgs(input3, devMgrObj);  //TargetFeature DETECTMONITORS, TargetType = "APP",Command = "SET"
                Assert.IsNotNull(SetCommandArgs_Result3);
                Assert.IsNotNull(SetCommandArgs_Result3.command_guid_string);
                Assert.IsNotNull(SetCommandArgs_Result3.serialize_Json_response);
            }

            CLIEventArgs input4 = new CLIEventArgs()
            {
                commandLineInput = new CommandLineInput()
                {
                    PluginsType = "AUDIO",
                    Command = "SET",
                    TargetType = "APP",
                    TargetFeature = "BRIGHTNESSLEVEL",
                    isCliRunAdmin = false,
                },
                command_guid_string = "test_guid"
            };
            commandLineInput = input4.commandLineInput;
            if (commandLineInput.TargetFeature == "BRIGHTNESSLEVEL")
            {
                var SetCommandArgs_Result4 = cLIDisplayPlugins.SetCommandArgs(input4, devMgrObj);  //TargetFeature BRIGHTNESSLEVEL, TargetType = "APP",Command = "SET"
                Assert.IsNotNull(SetCommandArgs_Result4);
                Assert.IsNotNull(SetCommandArgs_Result4.command_guid_string);
                Assert.IsNotNull(SetCommandArgs_Result4.serialize_Json_response);
            }

            CLIEventArgs input5 = new CLIEventArgs()
            {
                commandLineInput = new CommandLineInput()
                {
                    PluginsType = "AUDIO",
                    Command = "SET",
                    TargetType = "APP",
                    TargetFeature = "CONTRASTLEVEL",
                    isCliRunAdmin = false,
                },
                command_guid_string = "test_guid"
            };
            commandLineInput = input5.commandLineInput;
            if (commandLineInput.TargetFeature == "CONTRASTLEVEL")
            {
                var SetCommandArgs_Result5 = cLIDisplayPlugins.SetCommandArgs(input5, devMgrObj);  //TargetFeature CONTRASTLEVEL, TargetType = "APP",Command = "GET"
                Assert.IsNotNull(SetCommandArgs_Result5);
                Assert.IsNotNull(SetCommandArgs_Result5.command_guid_string);
                Assert.IsNotNull(SetCommandArgs_Result5.serialize_Json_response);
            }

            CLIEventArgs input6 = new CLIEventArgs()
            {
                commandLineInput = new CommandLineInput()
                {
                    PluginsType = "AUDIO",
                    Command = "SET",
                    TargetType = "APP",
                    TargetFeature = "EDID",
                    isCliRunAdmin = false,
                },
                command_guid_string = "test_guid"
            };
            commandLineInput = input6.commandLineInput;
            if (commandLineInput.TargetFeature == "EDID")
            {
                var SetCommandArgs_Result6 = cLIDisplayPlugins.SetCommandArgs(input6, devMgrObj);  //TargetFeature EDID, TargetType = "APP",Command = "GET"
                Assert.IsNotNull(SetCommandArgs_Result6);
                Assert.IsNotNull(SetCommandArgs_Result6.command_guid_string);
                Assert.IsNotNull(SetCommandArgs_Result6.serialize_Json_response);
            }

            CLIEventArgs input7 = new CLIEventArgs()
            {
                commandLineInput = new CommandLineInput()
                {
                    PluginsType = "AUDIO",
                    Command = "GET",
                    TargetType = "APP",
                    TargetFeature = "DECODEDEDID",
                    isCliRunAdmin = false,
                },
                command_guid_string = "test_guid"
            };
            commandLineInput = input7.commandLineInput;
            if (commandLineInput.TargetFeature == "DECODEDEDID")
            {
                var SetCommandArgs_Result7 = cLIDisplayPlugins.SetCommandArgs(input7, devMgrObj);  //TargetFeature DECODEDEDID, TargetType = "APP",Command = "GET"
                Assert.IsNotNull(SetCommandArgs_Result7);
                Assert.IsNotNull(SetCommandArgs_Result7.command_guid_string);
                Assert.IsNotNull(SetCommandArgs_Result7.serialize_Json_response);
            }

            CLIEventArgs input8 = new CLIEventArgs()
            {
                commandLineInput = new CommandLineInput()
                {
                    PluginsType = "AUDIO",
                    Command = "GET",
                    TargetType = "APP",
                    TargetFeature = "FWVERSION",
                    isCliRunAdmin = false,
                },
                command_guid_string = "test_guid"
            };
            commandLineInput = input8.commandLineInput;
            if (commandLineInput.TargetFeature == "FWVERSION")
            {
                var SetCommandArgs_Result8 = cLIDisplayPlugins.SetCommandArgs(input8, devMgrObj);  //TargetFeature FWVERSION, TargetType = "APP",Command = "GET"
                Assert.IsNotNull(SetCommandArgs_Result8);
                Assert.IsNotNull(SetCommandArgs_Result8.command_guid_string);
                Assert.IsNotNull(SetCommandArgs_Result8.serialize_Json_response);
            }

            CLIEventArgs input9 = new CLIEventArgs()
            {
                commandLineInput = new CommandLineInput()
                {
                    PluginsType = "AUDIO",
                    Command = "GET",
                    TargetType = "APP",
                    TargetFeature = "FIRMWAREUPDATE",
                    isCliRunAdmin = false,
                },
                command_guid_string = "test_guid"
            };
            commandLineInput = input9.commandLineInput;
            if (commandLineInput.TargetFeature == "FIRMWAREUPDATE")
            {
                var SetCommandArgs_Result9 = cLIDisplayPlugins.SetCommandArgs(input9, devMgrObj);  //TargetFeature FIRMWAREUPDATE, TargetType = "APP",Command = "GET"
                Assert.IsNotNull(SetCommandArgs_Result9);
                Assert.IsNotNull(SetCommandArgs_Result9.command_guid_string);
                Assert.IsNotNull(SetCommandArgs_Result9.serialize_Json_response);
            }

            CLIEventArgs input10 = new CLIEventArgs()
            {
                commandLineInput = new CommandLineInput()
                {
                    PluginsType = "AUDIO",
                    Command = "GET",
                    TargetType = "APP",
                    TargetFeature = "ACTIVEINPUTSOURCE",
                    isCliRunAdmin = false,
                },
                command_guid_string = "test_guid"
            };
            commandLineInput = input10.commandLineInput;
            if (commandLineInput.TargetFeature == "ACTIVEINPUTSOURCE")
            {
                var SetCommandArgs_Result10 = cLIDisplayPlugins.SetCommandArgs(input10, devMgrObj);  //TargetFeature ACTIVEINPUTSOURCE, TargetType = "APP",Command = "GET",DDPMSettings data
                Assert.IsNotNull(SetCommandArgs_Result10);
                Assert.IsNotNull(SetCommandArgs_Result10.command_guid_string);
                Assert.IsNotNull(SetCommandArgs_Result10.serialize_Json_response);
            }

            CLIEventArgs input11 = new CLIEventArgs()
            {
                commandLineInput = new CommandLineInput()
                {
                    PluginsType = "AUDIO",
                    Command = "GET",
                    TargetType = "APP",
                    TargetFeature = "INPUTSOURCELIST",
                    isCliRunAdmin = false,
                },
                command_guid_string = "test_guid"
            };
            commandLineInput = input11.commandLineInput;
            if (commandLineInput.TargetFeature == "INPUTSOURCELIST")
            {
                var SetCommandArgs_Result11 = cLIDisplayPlugins.SetCommandArgs(input11, devMgrObj);  //TargetFeature INPUTSOURCELIST, TargetType = "APP",Command = "GET",DDPMSettings data
                Assert.IsNotNull(SetCommandArgs_Result11);
                Assert.IsNotNull(SetCommandArgs_Result11.command_guid_string);
                Assert.IsNotNull(SetCommandArgs_Result11.serialize_Json_response);
            }

            CLIEventArgs input12 = new CLIEventArgs()
            {
                commandLineInput = new CommandLineInput()
                {
                    PluginsType = "AUDIO",
                    Command = "GET",
                    TargetType = "APP",
                    TargetFeature = "ROTATEOSDMENU",
                    isCliRunAdmin = false,
                },
                command_guid_string = "test_guid"
            };
            commandLineInput = input12.commandLineInput;
            if (commandLineInput.TargetFeature == "ROTATEOSDMENU")
            {
                var SetCommandArgs_Result12 = cLIDisplayPlugins.SetCommandArgs(input12, devMgrObj);  //TargetFeature ROTATEOSDMENU, TargetType = "APP",Command = "GET",DisplayPropertiesInfo displayPropertiesInfo
                Assert.IsNotNull(SetCommandArgs_Result12);
                Assert.IsNotNull(SetCommandArgs_Result12.command_guid_string);
                Assert.IsNotNull(SetCommandArgs_Result12.serialize_Json_response);
            }

            CLIEventArgs input13 = new CLIEventArgs()
            {
                commandLineInput = new CommandLineInput()
                {
                    PluginsType = "AUDIO",
                    Command = "GET",
                    TargetType = "APP",
                    TargetFeature = "COLORPRESET",
                    isCliRunAdmin = false,
                },
                command_guid_string = "test_guid"
            };
            commandLineInput = input13.commandLineInput;
            if (commandLineInput.TargetFeature == "COLORPRESET")
            {
                var SetCommandArgs_Result13 = cLIDisplayPlugins.SetCommandArgs(input13, devMgrObj);  //TargetFeature COLORPRESET, TargetType = "APP",Command = "GET",ReadCurrentColorPreset
                Assert.IsNotNull(SetCommandArgs_Result13);
                Assert.IsNotNull(SetCommandArgs_Result13.command_guid_string);
                Assert.IsNotNull(SetCommandArgs_Result13.serialize_Json_response);
            }

            CLIEventArgs input14 = new CLIEventArgs()
            {
                commandLineInput = new CommandLineInput()
                {
                    PluginsType = "AUDIO",
                    Command = "SET",
                    TargetType = "APP",
                    TargetFeature = "COLORPRESET",
                    isCliRunAdmin = false,
                    Options = new List<CommandType_Option>() { new CommandType_Option("Brightness", "50") { Option_Name = "VALUE" } },
                },
                command_guid_string = "test_guid"
            };
            commandLineInput = input14.commandLineInput;
            if (commandLineInput.TargetFeature == "COLORPRESET")
            {
                var SetCommandArgs_Result14 = cLIDisplayPlugins.SetCommandArgs(input14, devMgrObj);  //TargetFeature COLORPRESET, TargetType = "APP",Command = "SET",WriteColorPreset
                Assert.IsNotNull(SetCommandArgs_Result14);
                Assert.IsNotNull(SetCommandArgs_Result14.command_guid_string);
                Assert.IsNotNull(SetCommandArgs_Result14.serialize_Json_response);
            }

            CLIEventArgs input15 = new CLIEventArgs()
            {
                commandLineInput = new CommandLineInput()
                {
                    PluginsType = "AUDIO",
                    Command = "GET",
                    TargetType = "APP",
                    TargetFeature = "MULTIMONITORSYNC",
                    isCliRunAdmin = false,
                    //Options = new List<CommandType_Option>() { new CommandType_Option("Brightness", "50") { Option_Name = "VALUE" } },
                },
                command_guid_string = "test_guid"
            };
            commandLineInput = input15.commandLineInput;
            if (commandLineInput.TargetFeature == "MULTIMONITORSYNC")
            {
                var SetCommandArgs_Result15 = cLIDisplayPlugins.SetCommandArgs(input15, devMgrObj);  //TargetFeature MULTIMONITORSYNC, TargetType = "APP",Command = "GET",GetALSFeatureValue
                Assert.IsNotNull(SetCommandArgs_Result15);
                Assert.IsNotNull(SetCommandArgs_Result15.command_guid_string);
                Assert.IsNotNull(SetCommandArgs_Result15.serialize_Json_response);
            }

            CLIEventArgs input16 = new CLIEventArgs()
            {
                commandLineInput = new CommandLineInput()
                {
                    PluginsType = "AUDIO",
                    Command = "GET",
                    TargetType = "APP",
                    TargetFeature = "COLORPROFILE",
                    isCliRunAdmin = false,
                },
                command_guid_string = "test_guid"
            };
            commandLineInput = input16.commandLineInput;
            if (commandLineInput.TargetFeature == "COLORPROFILE")
            {
                var SetCommandArgs_Result16 = cLIDisplayPlugins.SetCommandArgs(input16, devMgrObj);  //TargetFeature COLORPROFILE, TargetType = "APP",Command = "GET",GetMonitorProfile
                Assert.IsNotNull(SetCommandArgs_Result16);
                Assert.IsNotNull(SetCommandArgs_Result16.command_guid_string);
                Assert.IsNotNull(SetCommandArgs_Result16.serialize_Json_response);
            }

            CLIEventArgs input17 = new CLIEventArgs()
            {
                commandLineInput = new CommandLineInput()
                {
                    PluginsType = "AUDIO",
                    Command = "SET",
                    TargetType = "APP",
                    TargetFeature = "ICCPROFILEBASEDONCOLORPRESET",
                    isCliRunAdmin = false,
                    Options = new List<CommandType_Option>() { new CommandType_Option("Brightness", "50") { Option_Name = "VALUE" } },
                },
                command_guid_string = "test_guid"
            };
            commandLineInput = input17.commandLineInput;
            if (commandLineInput.TargetFeature == "ICCPROFILEBASEDONCOLORPRESET")
            {
                var SetCommandArgs_Result17 = cLIDisplayPlugins.SetCommandArgs(input17, devMgrObj);  //TargetFeature ICCPROFILEBASEDONCOLORPRESET, TargetType = "APP",Command = "SET",GetMonitorProfile
                Assert.IsNotNull(SetCommandArgs_Result17);
                Assert.IsNotNull(SetCommandArgs_Result17.command_guid_string);
                Assert.IsNotNull(SetCommandArgs_Result17.serialize_Json_response);
            }

            CLIEventArgs input18 = new CLIEventArgs()
            {
                commandLineInput = new CommandLineInput()
                {
                    PluginsType = "AUDIO",
                    Command = "SET",
                    TargetType = "APP",
                    TargetFeature = "COLORPRESETBASEDONICCPROFILE",
                    isCliRunAdmin = false,
                    Options = new List<CommandType_Option>() { new CommandType_Option("Brightness", "50") { Option_Name = "VALUE" } },
                },
                command_guid_string = "test_guid"
            };
            commandLineInput = input18.commandLineInput;
            if (commandLineInput.TargetFeature == "COLORPRESETBASEDONICCPROFILE")
            {
                var SetCommandArgs_Result18 = cLIDisplayPlugins.SetCommandArgs(input18, devMgrObj);  //TargetFeature COLORPRESETBASEDONICCPROFILE, TargetType = "APP",Command = "SET",WriteColorPresetByColorProfile
                Assert.IsNotNull(SetCommandArgs_Result18);
                Assert.IsNotNull(SetCommandArgs_Result18.command_guid_string);
                Assert.IsNotNull(SetCommandArgs_Result18.serialize_Json_response);
            }

            CLIEventArgs input19 = new CLIEventArgs()
            {
                commandLineInput = new CommandLineInput()
                {
                    PluginsType = "AUDIO",
                    Command = "SET",
                    TargetType = "APP",
                    TargetFeature = "COLORMANAGEMENT",
                    isCliRunAdmin = false,
                    Options = new List<CommandType_Option>() { new CommandType_Option("Brightness", "50") { Option_Name = "VALUE" } },
                },
                command_guid_string = "test_guid"
            };
            commandLineInput = input19.commandLineInput;
            if (commandLineInput.TargetFeature == "COLORMANAGEMENT")
            {
                var SetCommandArgs_Result19 = cLIDisplayPlugins.SetCommandArgs(input19, devMgrObj);  //TargetFeature COLORMANAGEMENT, TargetType = "APP",Command = "SET",WriteColorPresetByColorProfile
                Assert.IsNotNull(SetCommandArgs_Result19);
                Assert.IsNotNull(SetCommandArgs_Result19.command_guid_string);
                Assert.IsNotNull(SetCommandArgs_Result19.serialize_Json_response);
            }

            CLIEventArgs input20 = new CLIEventArgs()
            {
                commandLineInput = new CommandLineInput()
                {
                    PluginsType = "AUDIO",
                    Command = "GET",
                    TargetType = "APP",
                    TargetFeature = "COLORMANAGEMENT",
                    isCliRunAdmin = false,
                    Options = new List<CommandType_Option>() { new CommandType_Option("Brightness", "50") { Option_Name = "VALUE" } },
                },
                command_guid_string = "test_guid"
            };
            commandLineInput = input20.commandLineInput;
            if (commandLineInput.TargetFeature == "COLORMANAGEMENT")
            {
                var SetCommandArgs_Result20 = cLIDisplayPlugins.SetCommandArgs(input20, devMgrObj);  //TargetFeature COLORMANAGEMENT, TargetType = "APP",Command = "GET",GetColorManagementStatus
                Assert.IsNotNull(SetCommandArgs_Result20);
                Assert.IsNotNull(SetCommandArgs_Result20.command_guid_string);
                Assert.IsNotNull(SetCommandArgs_Result20.serialize_Json_response);
            }

            CLIEventArgs input21 = new CLIEventArgs()
            {
                commandLineInput = new CommandLineInput()
                {
                    PluginsType = "AUDIO",
                    Command = "GET",
                    TargetType = "APP",
                    TargetFeature = "RESTOREFACTORYDEFAULTS",
                    isCliRunAdmin = false,
                    Options = new List<CommandType_Option>() { new CommandType_Option("Brightness", "50") { Option_Name = "VALUE" } },
                },
                command_guid_string = "test_guid"
            };
            commandLineInput = input21.commandLineInput;
            if (commandLineInput.TargetFeature == "RESTOREFACTORYDEFAULTS")
            {
                var SetCommandArgs_Result21 = cLIDisplayPlugins.SetCommandArgs(input21, devMgrObj);  //TargetFeature RESTOREFACTORYDEFAULTS, TargetType = "APP",Command = "GET",GetColorManagementStatus
                Assert.IsNotNull(SetCommandArgs_Result21);
                Assert.IsNotNull(SetCommandArgs_Result21.command_guid_string);
                Assert.IsNotNull(SetCommandArgs_Result21.serialize_Json_response);
            }

            CLIEventArgs input22 = new CLIEventArgs()
            {
                commandLineInput = new CommandLineInput()
                {
                    PluginsType = "AUDIO",
                    Command = "SET",
                    TargetType = "APP",
                    TargetFeature = "RESTOREFACTORYDEFAULTS",
                    isCliRunAdmin = false,
                    Options = new List<CommandType_Option>() { new CommandType_Option("Brightness", "50") { Option_Name = "VALUE" } },
                },
                command_guid_string = "test_guid"
            };
            commandLineInput = input22.commandLineInput;
            if (commandLineInput.TargetFeature == "RESTOREFACTORYDEFAULTS")
            {
                var SetCommandArgs_Result22 = cLIDisplayPlugins.SetCommandArgs(input22, devMgrObj);  //TargetFeature RESTOREFACTORYDEFAULTS, TargetType = "APP",Command = "SET",SetVCPCapability
                Assert.IsNotNull(SetCommandArgs_Result22);
                Assert.IsNotNull(SetCommandArgs_Result22.command_guid_string);
                Assert.IsNotNull(SetCommandArgs_Result22.serialize_Json_response);
            }

            CLIEventArgs input23 = new CLIEventArgs()
            {
                commandLineInput = new CommandLineInput()
                {
                    PluginsType = "AUDIO",
                    Command = "SET",
                    TargetType = "APP",
                    TargetFeature = "RESTORELEVELDEFAULTS",
                    isCliRunAdmin = false,
                    Options = new List<CommandType_Option>() { new CommandType_Option("Brightness", "50") { Option_Name = "VALUE" } },
                },
                command_guid_string = "test_guid"
            };
            commandLineInput = input23.commandLineInput;
            if (commandLineInput.TargetFeature == "RESTORELEVELDEFAULTS")
            {
                var SetCommandArgs_Result23 = cLIDisplayPlugins.SetCommandArgs(input23, devMgrObj);  //TargetFeature RESTORELEVELDEFAULTS, TargetType = "APP",Command = "SET",SetVCPCapability
                Assert.IsNotNull(SetCommandArgs_Result23);
                Assert.IsNotNull(SetCommandArgs_Result23.command_guid_string);
                Assert.IsNotNull(SetCommandArgs_Result23.serialize_Json_response);
            }

            CLIEventArgs input24 = new CLIEventArgs()
            {
                commandLineInput = new CommandLineInput()
                {
                    PluginsType = "AUDIO",
                    Command = "GET",
                    TargetType = "APP",
                    TargetFeature = "RESTORELEVELDEFAULTS",
                    isCliRunAdmin = false,
                    Options = new List<CommandType_Option>() { new CommandType_Option("Brightness", "50") { Option_Name = "VALUE" } },
                },
                command_guid_string = "test_guid"
            };
            commandLineInput = input24.commandLineInput;
            if (commandLineInput.TargetFeature == "RESTORELEVELDEFAULTS")
            {
                var SetCommandArgs_Result24 = cLIDisplayPlugins.SetCommandArgs(input24, devMgrObj);  //TargetFeature RESTORELEVELDEFAULTS, TargetType = "APP",Command = "GET",SetVCPCapability
                Assert.IsNotNull(SetCommandArgs_Result24);
                Assert.IsNotNull(SetCommandArgs_Result24.command_guid_string);
                Assert.IsNotNull(SetCommandArgs_Result24.serialize_Json_response);
            }

            CLIEventArgs input25 = new CLIEventArgs()
            {
                commandLineInput = new CommandLineInput()
                {
                    PluginsType = "AUDIO",
                    Command = "GET",
                    TargetType = "APP",
                    TargetFeature = "RESTORECOLORDEFAULTS",
                    isCliRunAdmin = false,
                    Options = new List<CommandType_Option>() { new CommandType_Option("Brightness", "50") { Option_Name = "VALUE" } },
                },
                command_guid_string = "test_guid"
            };
            commandLineInput = input25.commandLineInput;
            if (commandLineInput.TargetFeature == "RESTORECOLORDEFAULTS")
            {
                var SetCommandArgs_Result25 = cLIDisplayPlugins.SetCommandArgs(input25, devMgrObj);  //TargetFeature RESTORECOLORDEFAULTS, TargetType = "APP",Command = "GET",SetVCPCapability
                Assert.IsNotNull(SetCommandArgs_Result25);
                Assert.IsNotNull(SetCommandArgs_Result25.command_guid_string);
                Assert.IsNotNull(SetCommandArgs_Result25.serialize_Json_response);
            }

            CLIEventArgs input26 = new CLIEventArgs()
            {
                commandLineInput = new CommandLineInput()
                {
                    PluginsType = "AUDIO",
                    Command = "SET",
                    TargetType = "APP",
                    TargetFeature = "RESTORECOLORDEFAULTS",
                    isCliRunAdmin = false,
                    Options = new List<CommandType_Option>() { new CommandType_Option("Brightness", "50") { Option_Name = "VALUE" } },
                },
                command_guid_string = "test_guid"
            };
            commandLineInput = input26.commandLineInput;
            if (commandLineInput.TargetFeature == "RESTORECOLORDEFAULTS")
            {
                var SetCommandArgs_Result26 = cLIDisplayPlugins.SetCommandArgs(input26, devMgrObj);  //TargetFeature RESTORECOLORDEFAULTS, TargetType = "APP",Command = "SET",SetVCPCapability
                Assert.IsNotNull(SetCommandArgs_Result26);
                Assert.IsNotNull(SetCommandArgs_Result26.command_guid_string);
                Assert.IsNotNull(SetCommandArgs_Result26.serialize_Json_response);
            }

            CLIEventArgs input27 = new CLIEventArgs()
            {
                commandLineInput = new CommandLineInput()
                {
                    PluginsType = "AUDIO",
                    Command = "GET",
                    TargetType = "APP",
                    TargetFeature = "OSDACCESS",
                    isCliRunAdmin = false,
                    Options = new List<CommandType_Option>() { new CommandType_Option("Brightness", "50") { Option_Name = "VALUE" } },
                },
                command_guid_string = "test_guid"
            };
            commandLineInput = input27.commandLineInput;
            if (commandLineInput.TargetFeature == "OSDACCESS")
            {
                var SetCommandArgs_Result27 = cLIDisplayPlugins.SetCommandArgs(input27, devMgrObj);  //TargetFeature OSDACCESS, TargetType = "APP",Command = "GET",GetVCPCapability
                Assert.IsNotNull(SetCommandArgs_Result27);
                Assert.IsNotNull(SetCommandArgs_Result27.command_guid_string);
                Assert.IsNotNull(SetCommandArgs_Result27.serialize_Json_response);
            }

            CLIEventArgs input28 = new CLIEventArgs()
            {
                commandLineInput = new CommandLineInput()
                {
                    PluginsType = "AUDIO",
                    Command = "SET",
                    TargetType = "APP",
                    TargetFeature = "OSDACCESS",
                    isCliRunAdmin = false,
                    Options = new List<CommandType_Option>() { new CommandType_Option("Brightness", "50") { Option_Name = "VALUE" } },
                },
                command_guid_string = "test_guid"
            };
            commandLineInput = input28.commandLineInput;
            if (commandLineInput.TargetFeature == "OSDACCESS")
            {
                var SetCommandArgs_Result28 = cLIDisplayPlugins.SetCommandArgs(input28, devMgrObj);  //TargetFeature OSDACCESS, TargetType = "APP",Command = "SET",GetVCPCapability
                Assert.IsNotNull(SetCommandArgs_Result28);
                Assert.IsNotNull(SetCommandArgs_Result28.command_guid_string);
                Assert.IsNotNull(SetCommandArgs_Result28.serialize_Json_response);
            }

            CLIEventArgs input29 = new CLIEventArgs()
            {
                commandLineInput = new CommandLineInput()
                {
                    PluginsType = "AUDIO",
                    Command = "GET",
                    TargetType = "APP",
                    TargetFeature = "PXP",
                    isCliRunAdmin = false,
                    Options = new List<CommandType_Option>() { new CommandType_Option("Brightness", "50") { Option_Name = "VALUE" } },
                },
                command_guid_string = "test_guid"
            };
            commandLineInput = input29.commandLineInput;
            if (commandLineInput.TargetFeature == "PXP")
            {
                var SetCommandArgs_Result29 = cLIDisplayPlugins.SetCommandArgs(input29, devMgrObj);  //TargetFeature PXP, TargetType = "APP",Command = "GET", off", 0x00, "PIP/PBP off, full screen"
                Assert.IsNotNull(SetCommandArgs_Result29);
                Assert.IsNotNull(SetCommandArgs_Result29.command_guid_string);
                Assert.IsNotNull(SetCommandArgs_Result29.serialize_Json_response);
            }

            CLIEventArgs input30 = new CLIEventArgs()
            {
                commandLineInput = new CommandLineInput()
                {
                    PluginsType = "AUDIO",
                    Command = "GET",
                    TargetType = "APP",
                    TargetFeature = "POWERNAP",
                    isCliRunAdmin = false,
                    Options = new List<CommandType_Option>() { new CommandType_Option("Brightness", "34") { Option_Name = "VALUE" } },
                },
                command_guid_string = "test_guid"
            };
            commandLineInput = input30.commandLineInput;
            if (commandLineInput.TargetFeature == "POWERNAP")
            {
                var SetCommandArgs_Result30 = cLIDisplayPlugins.SetCommandArgs(input30, devMgrObj);  //TargetFeature POWERNAP, TargetType = "APP",Command = "GET"
                Assert.IsNotNull(SetCommandArgs_Result30);
                Assert.IsNotNull(SetCommandArgs_Result30.command_guid_string);
                Assert.IsNotNull(SetCommandArgs_Result30.serialize_Json_response);
            }

            CLIEventArgs input31 = new CLIEventArgs()
            {
                commandLineInput = new CommandLineInput()
                {
                    PluginsType = "AUDIO",
                    Command = "SET",
                    TargetType = "APP",
                    TargetFeature = "POWERNAP",
                    isCliRunAdmin = false,
                    Options = new List<CommandType_Option>() { new CommandType_Option("POWERNAP", "UNLOCK") { Option_Name = "VALUE" } },
                },
                command_guid_string = "test_guid"
            };
            commandLineInput = input31.commandLineInput;
            if (commandLineInput.TargetFeature == "POWERNAP")
            {
                var SetCommandArgs_Result31 = cLIDisplayPlugins.SetCommandArgs(input31, devMgrObj);  //TargetFeature POWERNAP, TargetType = "APP",Command = "SET",SetAppConfigData
                Assert.IsNotNull(SetCommandArgs_Result31);
                Assert.IsNotNull(SetCommandArgs_Result31.command_guid_string);
                Assert.IsNotNull(SetCommandArgs_Result31.serialize_Json_response);
            }

            CLIEventArgs input32 = new CLIEventArgs()
            {
                commandLineInput = new CommandLineInput()
                {
                    PluginsType = "AUDIO",
                    Command = "GET",
                    TargetType = "APP",
                    TargetFeature = "DEVICEDATA",
                    isCliRunAdmin = false,
                    Options = new List<CommandType_Option>() { new CommandType_Option("DEVICEDATA", "UNLOCK") { Option_Name = "VALUE" } },
                },
                command_guid_string = "test_guid"
            };
            commandLineInput = input32.commandLineInput;
            if (commandLineInput.TargetFeature == "DEVICEDATA")
            {
                var SetCommandArgs_Result32 = cLIDisplayPlugins.SetCommandArgs(input32, devMgrObj);  //TargetFeature DEVICEDATA, TargetType = "APP",Command = "GET",SetAppConfigData
                Assert.IsNotNull(SetCommandArgs_Result32);
                Assert.IsNotNull(SetCommandArgs_Result32.command_guid_string);
                Assert.IsNotNull(SetCommandArgs_Result32.serialize_Json_response);
            }

            CLIEventArgs input33 = new CLIEventArgs()
            {
                commandLineInput = new CommandLineInput()
                {
                    PluginsType = "AUDIO",
                    Command = "GET",
                    TargetType = "TestAPP",
                    TargetFeature = "DEVICEDATA",
                    isCliRunAdmin = false,
                    Options = new List<CommandType_Option>() { new CommandType_Option("DEVICEDATA", "UNLOCK") { Option_Name = "VALUE" } },
                },
                command_guid_string = "test_guid"
            };
            commandLineInput = input33.commandLineInput;
            if (commandLineInput.TargetFeature == "DEVICEDATA")
            {
                var SetCommandArgs_Result33 = cLIDisplayPlugins.SetCommandArgs(input33, devMgrObj);  //TargetFeature DEVICEDATA, TargetType = "TestAPP",Command = "GET",SetAppConfigData
                Assert.IsNotNull(SetCommandArgs_Result33);
                Assert.IsNotNull(SetCommandArgs_Result33.command_guid_string);
                Assert.IsNotNull(SetCommandArgs_Result33.serialize_Json_response);
            }

            CLIEventArgs input34 = new CLIEventArgs()
            {
                commandLineInput = new CommandLineInput()
                {
                    PluginsType = "AUDIO",
                    Command = "GET",
                    TargetType = "APP",
                    TargetFeature = "SCREENNOTIFICATION",
                    isCliRunAdmin = false,
                    Options = new List<CommandType_Option>() { new CommandType_Option("SCREENNOTIFICATION", "UNLOCK") { Option_Name = "VALUE" } },
                },
                command_guid_string = "test_guid"
            };
            commandLineInput = input34.commandLineInput;
            if (commandLineInput.TargetFeature == "SCREENNOTIFICATION")
            {
                var SetCommandArgs_Result34 = cLIDisplayPlugins.SetCommandArgs(input34, devMgrObj);  //TargetFeature SCREENNOTIFICATION, TargetType = "APP",Command = "GET",SetAppConfigData
                Assert.IsNotNull(SetCommandArgs_Result34);
                Assert.IsNotNull(SetCommandArgs_Result34.command_guid_string);
                Assert.IsNotNull(SetCommandArgs_Result34.serialize_Json_response);
            }

            CLIEventArgs input35 = new CLIEventArgs()
            {
                commandLineInput = new CommandLineInput()
                {
                    PluginsType = "AUDIO",
                    Command = "GET",
                    TargetType = "TestAPP",
                    TargetFeature = "SCREENNOTIFICATION",
                    isCliRunAdmin = false,
                    Options = new List<CommandType_Option>() { new CommandType_Option("SCREENNOTIFICATION", "UNLOCK") { Option_Name = "VALUE" } },
                },
                command_guid_string = "test_guid"
            };
            commandLineInput = input35.commandLineInput;
            if (commandLineInput.TargetFeature == "SCREENNOTIFICATION")
            {
                var SetCommandArgs_Result35 = cLIDisplayPlugins.SetCommandArgs(input35, devMgrObj);  //TargetFeature SCREENNOTIFICATION, TargetType = "TestAPP",Command = "GET",SetAppConfigData
                Assert.IsNotNull(SetCommandArgs_Result35);
                Assert.IsNotNull(SetCommandArgs_Result35.command_guid_string);
                Assert.IsNotNull(SetCommandArgs_Result35.serialize_Json_response);
            }

            CLIEventArgs input36 = new CLIEventArgs()
            {
                commandLineInput = new CommandLineInput()
                {
                    PluginsType = "AUDIO",
                    Command = "SET",
                    TargetType = "TestAPP",
                    TargetFeature = "CAPABILITIESSTRING",
                    isCliRunAdmin = false,
                    Options = new List<CommandType_Option>() { new CommandType_Option("CAPABILITIESSTRING", "UNLOCK") { Option_Name = "VALUE" } },
                },
                command_guid_string = "test_guid"
            };
            commandLineInput = input36.commandLineInput;
            if (commandLineInput.TargetFeature == "CAPABILITIESSTRING")
            {
                var SetCommandArgs_Result36 = cLIDisplayPlugins.SetCommandArgs(input36, devMgrObj);  //TargetFeature CAPABILITIESSTRING, TargetType = "TestAPP",Command = "SET"
                Assert.IsNotNull(SetCommandArgs_Result36);
                Assert.IsNotNull(SetCommandArgs_Result36.command_guid_string);
                Assert.IsNotNull(SetCommandArgs_Result36.serialize_Json_response);
            }

            CLIEventArgs input37 = new CLIEventArgs()
            {
                commandLineInput = new CommandLineInput()
                {
                    PluginsType = "AUDIO",
                    Command = "SET",
                    TargetType = "TestAPP",
                    TargetFeature = "AUTOCOLORPRESET",
                    isCliRunAdmin = false,
                    Options = new List<CommandType_Option>() { new CommandType_Option("CAPABILITIESSTRING", "OFF") { Option_Name = "VALUE" } },
                },
                command_guid_string = "test_guid"
            };
            commandLineInput = input37.commandLineInput;
            if (commandLineInput.TargetFeature == "AUTOCOLORPRESET")
            {
                var SetCommandArgs_Result37 = cLIDisplayPlugins.SetCommandArgs(input37, devMgrObj);  //TargetFeature AUTOCOLORPRESET, TargetType = "TestAPP",Command = "SET",AutoSetColorPresetForMonitorConfig
                Assert.IsNotNull(SetCommandArgs_Result37);
                Assert.IsNotNull(SetCommandArgs_Result37.command_guid_string);
                Assert.IsNotNull(SetCommandArgs_Result37.serialize_Json_response);
            }

            CLIEventArgs input38 = new CLIEventArgs()
            {
                commandLineInput = new CommandLineInput()
                {
                    PluginsType = "AUDIO",
                    Command = "SET",
                    TargetType = "TestAPP",
                    TargetFeature = "OSDLANGUAGE",
                    isCliRunAdmin = false,
                    Options = new List<CommandType_Option>() { new CommandType_Option("CAPABILITIESSTRING", "english") { Option_Name = "VALUE" } },
                },
                command_guid_string = "test_guid"
            };
            commandLineInput = input38.commandLineInput;
            if (commandLineInput.TargetFeature == "OSDLANGUAGE")
            {
                var SetCommandArgs_Result38 = cLIDisplayPlugins.SetCommandArgs(input38, devMgrObj);  //TargetFeature OSDLANGUAGE, TargetType = "TestAPP",Command = "SET",AutoSetColorPresetForMonitorConfig
                Assert.IsNotNull(SetCommandArgs_Result38);
                Assert.IsNotNull(SetCommandArgs_Result38.command_guid_string);
                Assert.IsNotNull(SetCommandArgs_Result38.serialize_Json_response);
            }

            CLIEventArgs input39 = new CLIEventArgs()
            {
                commandLineInput = new CommandLineInput()
                {
                    PluginsType = "AUDIO",
                    Command = "SET",
                    TargetType = "TestAPP",
                    TargetFeature = "MONITORCOUNT",
                    isCliRunAdmin = false,
                    Options = new List<CommandType_Option>() { new CommandType_Option("CAPABILITIESSTRING", "english") { Option_Name = "VALUE" } },
                },
                command_guid_string = "test_guid"
            };
            commandLineInput = input39.commandLineInput;
            if (commandLineInput.TargetFeature == "MONITORCOUNT")
            {
                var SetCommandArgs_Result39 = cLIDisplayPlugins.SetCommandArgs(input39, devMgrObj);  //TargetFeature MONITORCOUNT, TargetType = "TestAPP",Command = "SET",AutoSetColorPresetForMonitorConfig
                Assert.IsNotNull(SetCommandArgs_Result39);
                Assert.IsNotNull(SetCommandArgs_Result39.command_guid_string);
                Assert.IsNotNull(SetCommandArgs_Result39.serialize_Json_response);
            }

            CLIEventArgs input40 = new CLIEventArgs()
            {
                commandLineInput = new CommandLineInput()
                {
                    PluginsType = "AUDIO",
                    Command = "GET",
                    TargetType = "TestAPP",
                    TargetFeature = "MONITORCOUNT",
                    isCliRunAdmin = false,
                    //Options = new List<CommandType_Option>() { new CommandType_Option("CAPABILITIESSTRING", "english") { Option_Name = "VALUE" } },
                },
                command_guid_string = "test_guid"
            };
            commandLineInput = input40.commandLineInput;
            if (commandLineInput.TargetFeature == "MONITORCOUNT")
            {
                var SetCommandArgs_Result40 = cLIDisplayPlugins.SetCommandArgs(input40, devMgrObj);  //TargetFeature MONITORCOUNT, TargetType = "TestAPP",Command = "GET",
                Assert.IsNotNull(SetCommandArgs_Result40);
                Assert.IsNotNull(SetCommandArgs_Result40.command_guid_string);
                Assert.IsNotNull(SetCommandArgs_Result40.serialize_Json_response);
            }

            CLIEventArgs input41 = new CLIEventArgs()
            {
                commandLineInput = new CommandLineInput()
                {
                    PluginsType = "AUDIO",
                    Command = "GET",
                    TargetType = "TestAPP",
                    TargetFeature = "DIAGNOSTICSREPORT",
                    isCliRunAdmin = false,
                    Options = new List<CommandType_Option>() { new CommandType_Option("DIAGNOSTICSREPORT", "english") { Option_Name = "VALUE" } },
                },
                command_guid_string = "test_guid"
            };
            commandLineInput = input41.commandLineInput;
            if (commandLineInput.TargetFeature == "DIAGNOSTICSREPORT")
            {
                var SetCommandArgs_Result41 = cLIDisplayPlugins.SetCommandArgs(input41, devMgrObj);  //TargetFeature DIAGNOSTICSREPORT, TargetType = "TestAPP",Command = "GET",
                Assert.IsNotNull(SetCommandArgs_Result41);
                Assert.IsNotNull(SetCommandArgs_Result41.command_guid_string);
                Assert.IsNotNull(SetCommandArgs_Result41.serialize_Json_response);
            }

            CLIEventArgs input42 = new CLIEventArgs()
            {
                commandLineInput = new CommandLineInput()
                {
                    PluginsType = "AUDIO",
                    Command = "SET",
                    TargetType = "TestAPP",
                    TargetFeature = "ACTIVEHOURS",
                    isCliRunAdmin = false,
                    Options = new List<CommandType_Option>() { new CommandType_Option("DIAGNOSTICSREPORT", "english") { Option_Name = "VALUE" } },
                },
                command_guid_string = "test_guid"
            };
            commandLineInput = input42.commandLineInput;
            if (commandLineInput.TargetFeature == "ACTIVEHOURS")
            {
                var SetCommandArgs_Result42 = cLIDisplayPlugins.SetCommandArgs(input42, devMgrObj);  //TargetFeature ACTIVEHOURS, TargetType = "TestAPP",Command = "SET",
                Assert.IsNotNull(SetCommandArgs_Result42);
                Assert.IsNotNull(SetCommandArgs_Result42.command_guid_string);
                Assert.IsNotNull(SetCommandArgs_Result42.serialize_Json_response);
            }

            CLIEventArgs input43 = new CLIEventArgs()
            {
                commandLineInput = new CommandLineInput()
                {
                    PluginsType = "AUDIO",
                    Command = "GET",
                    TargetType = "TestAPP",
                    TargetFeature = "ACTIVEHOURS",
                    isCliRunAdmin = false,
                    //Options = new List<CommandType_Option>() { new CommandType_Option("DIAGNOSTICSREPORT", "english") { Option_Name = "VALUE" } },
                },
                command_guid_string = "test_guid"
            };
            commandLineInput = input43.commandLineInput;
            if (commandLineInput.TargetFeature == "ACTIVEHOURS")
            {
                var SetCommandArgs_Result43 = cLIDisplayPlugins.SetCommandArgs(input43, devMgrObj);  //TargetFeature ACTIVEHOURS, TargetType = "TestAPP",Command = "GET",
                Assert.IsNotNull(SetCommandArgs_Result43);
                Assert.IsNotNull(SetCommandArgs_Result43.command_guid_string);
                Assert.IsNotNull(SetCommandArgs_Result43.serialize_Json_response);
            }

            CLIEventArgs input44 = new CLIEventArgs()
            {
                commandLineInput = new CommandLineInput()
                {
                    PluginsType = "AUDIO",
                    Command = "GET",
                    TargetType = "TestAPP",
                    TargetFeature = "DEVICECONFIGURATION",
                    isCliRunAdmin = false,
                    Options = new List<CommandType_Option>() { new CommandType_Option("DIAGNOSTICSREPORT", "english") { Option_Name = "VALUE" } },
                },
                command_guid_string = "test_guid"
            };
            commandLineInput = input44.commandLineInput;
            if (commandLineInput.TargetFeature == "DEVICECONFIGURATION")
            {
                var SetCommandArgs_Result44 = cLIDisplayPlugins.SetCommandArgs(input44, devMgrObj);  //TargetFeature DEVICECONFIGURATION, TargetType = "TestAPP",Command = "GET",
                Assert.IsNotNull(SetCommandArgs_Result44);
                Assert.IsNotNull(SetCommandArgs_Result44.command_guid_string);
                Assert.IsNotNull(SetCommandArgs_Result44.serialize_Json_response);
            }

            CLIEventArgs input45 = new CLIEventArgs()
            {
                commandLineInput = new CommandLineInput()
                {
                    PluginsType = "AUDIO",
                    Command = "SET",
                    TargetType = "TestAPP",
                    TargetFeature = "DEVICECONFIGURATION",
                    isCliRunAdmin = false,
                    Options = new List<CommandType_Option>() { new CommandType_Option("DIAGNOSTICSREPORT", "english") { Option_Name = "VALUE" } },
                },
                command_guid_string = "test_guid"
            };
            commandLineInput = input45.commandLineInput;
            if (commandLineInput.TargetFeature == "DEVICECONFIGURATION")
            {
                var SetCommandArgs_Result45 = cLIDisplayPlugins.SetCommandArgs(input45, devMgrObj);  //TargetFeature DEVICECONFIGURATION, TargetType = "TestAPP",Command = "SET",
                Assert.IsNotNull(SetCommandArgs_Result45);
                Assert.IsNotNull(SetCommandArgs_Result45.command_guid_string);
                Assert.IsNotNull(SetCommandArgs_Result45.serialize_Json_response);
            }

            CLIEventArgs input46 = new CLIEventArgs()
            {
                commandLineInput = new CommandLineInput()
                {
                    PluginsType = "AUDIO",
                    Command = "SET",
                    TargetType = "TestAPP",
                    TargetFeature = "POWERSETTING",
                    isCliRunAdmin = false,
                    Options = new List<CommandType_Option>() { new CommandType_Option("POWERSETTING", "english") { Option_Name = "VALUE" } },
                },
                command_guid_string = "test_guid"
            };
            commandLineInput = input46.commandLineInput;
            if (commandLineInput.TargetFeature == "POWERSETTING")
            {
                var SetCommandArgs_Result46 = cLIDisplayPlugins.SetCommandArgs(input46, devMgrObj);  //TargetFeature POWERSETTING, TargetType = "TestAPP",Command = "SET",
                Assert.IsNotNull(SetCommandArgs_Result46);
                Assert.IsNotNull(SetCommandArgs_Result46.command_guid_string);
                Assert.IsNotNull(SetCommandArgs_Result46.serialize_Json_response);
            }

            CLIEventArgs input47 = new CLIEventArgs()
            {
                commandLineInput = new CommandLineInput()
                {
                    PluginsType = "AUDIO",
                    Command = "GET",
                    TargetType = "TestAPP",
                    TargetFeature = "POWERSETTING",
                    isCliRunAdmin = false,
                    //Options = new List<CommandType_Option>() { new CommandType_Option("POWERSETTING", "english") { Option_Name = "VALUE" } },
                },
                command_guid_string = "test_guid"
            };
            commandLineInput = input47.commandLineInput;
            if (commandLineInput.TargetFeature == "POWERSETTING")
            {
                var SetCommandArgs_Result47 = cLIDisplayPlugins.SetCommandArgs(input47, devMgrObj);  //TargetFeature POWERSETTING, TargetType = "TestAPP",Command = "GET",
                Assert.IsNotNull(SetCommandArgs_Result47);
                Assert.IsNotNull(SetCommandArgs_Result47.command_guid_string);
                Assert.IsNotNull(SetCommandArgs_Result47.serialize_Json_response);
            }

            CLIEventArgs input48 = new CLIEventArgs()
            {
                commandLineInput = new CommandLineInput()
                {
                    PluginsType = "AUDIO",
                    Command = "SET",
                    TargetType = "TestAPP",
                    TargetFeature = "SPEAKERMICROPHONE",
                    isCliRunAdmin = false,
                    Options = new List<CommandType_Option>() { new CommandType_Option("SPEAKERMICROPHONE", "english") { Option_Name = "VALUE" } },
                },
                command_guid_string = "test_guid"
            };
            commandLineInput = input48.commandLineInput;
            if (commandLineInput.TargetFeature == "SPEAKERMICROPHONE")
            {
                var SetCommandArgs_Result48 = cLIDisplayPlugins.SetCommandArgs(input48, devMgrObj);  //TargetFeature SPEAKERMICROPHONE, TargetType = "TestAPP",Command = "SET",
                Assert.IsNotNull(SetCommandArgs_Result48);
                Assert.IsNotNull(SetCommandArgs_Result48.command_guid_string);
                Assert.IsNotNull(SetCommandArgs_Result48.serialize_Json_response);
            }

            CLIEventArgs input49 = new CLIEventArgs()
            {
                commandLineInput = new CommandLineInput()
                {
                    PluginsType = "AUDIO",
                    Command = "GET",
                    TargetType = "TestAPP",
                    TargetFeature = "ADVANCEDCONTROL",
                    isCliRunAdmin = false,
                    Options = new List<CommandType_Option>() { new CommandType_Option("ADVANCEDCONTROL", "0X01") { Option_Name = "OPCODE" } },
                },
                command_guid_string = "test_guid"
            };
            commandLineInput = input49.commandLineInput;
            if (commandLineInput.TargetFeature == "ADVANCEDCONTROL")
            {
                var SetCommandArgs_Result49 = cLIDisplayPlugins.SetCommandArgs(input49, devMgrObj);  //TargetFeature ADVANCEDCONTROL, TargetType = "TestAPP",Command = "SET",
                Assert.IsNotNull(SetCommandArgs_Result49);
                Assert.IsNotNull(SetCommandArgs_Result49.command_guid_string);
                Assert.IsNotNull(SetCommandArgs_Result49.serialize_Json_response);
            }

            CLIEventArgs input50 = new CLIEventArgs()
            {
                commandLineInput = new CommandLineInput()
                {
                    PluginsType = "AUDIO",
                    Command = "SET",
                    TargetType = "TestAPP",
                    TargetFeature = "ACTIVEHOUR",
                    isCliRunAdmin = false,
                    Options = new List<CommandType_Option>() { new CommandType_Option("ACTIVEHOUR", "0X01") { Option_Name = "OPCODE" } },
                },
                command_guid_string = "test_guid"
            };
            commandLineInput = input50.commandLineInput;
            if (commandLineInput.TargetFeature == "ACTIVEHOUR")
            {
                var SetCommandArgs_Result50 = cLIDisplayPlugins.SetCommandArgs(input50, devMgrObj);  //TargetFeature ACTIVEHOUR, TargetType = "TestAPP",Command = "SET",
                Assert.IsNotNull(SetCommandArgs_Result50);
                Assert.IsNotNull(SetCommandArgs_Result50.command_guid_string);
                Assert.IsNotNull(SetCommandArgs_Result50.serialize_Json_response);
            }

            CLIEventArgs input51 = new CLIEventArgs()
            {
                commandLineInput = new CommandLineInput()
                {
                    PluginsType = "AUDIO",
                    Command = "GET",
                    TargetType = "TestAPP",
                    TargetFeature = "ACTIVEHOUR",
                    isCliRunAdmin = false,
                    //Options = new List<CommandType_Option>() { new CommandType_Option("ACTIVEHOUR", "0X01") { Option_Name = "OPCODE" } },
                },
                command_guid_string = "test_guid"
            };
            commandLineInput = input51.commandLineInput;
            if (commandLineInput.TargetFeature == "ACTIVEHOUR")
            {
                var SetCommandArgs_Result51 = cLIDisplayPlugins.SetCommandArgs(input51, devMgrObj);  //TargetFeature ACTIVEHOUR, TargetType = "TestAPP",Command = "GET",
                Assert.IsNotNull(SetCommandArgs_Result51);
                Assert.IsNotNull(SetCommandArgs_Result51.command_guid_string);
                Assert.IsNotNull(SetCommandArgs_Result51.serialize_Json_response);
            }

            CLIEventArgs input52 = new CLIEventArgs()
            {
                commandLineInput = new CommandLineInput()
                {
                    PluginsType = "AUDIO",
                    Command = "GET",
                    TargetType = "TestAPP",
                    TargetFeature = "EASYARRANGELAYOUT",
                    isCliRunAdmin = false,
                    Options = new List<CommandType_Option>() { new CommandType_Option("EASYARRANGELAYOUT", "0X01") { Option_Name = "OPCODE" } },
                },
                command_guid_string = "test_guid"
            };
            commandLineInput = input52.commandLineInput;
            if (commandLineInput.TargetFeature == "EASYARRANGELAYOUT")
            {
                var SetCommandArgs_Result52 = cLIDisplayPlugins.SetCommandArgs(input52, devMgrObj);  //TargetFeature EASYARRANGELAYOUT, TargetType = "TestAPP",Command = "GET",GetEAFunctionEnabled
                Assert.IsNotNull(SetCommandArgs_Result52);
                Assert.IsNotNull(SetCommandArgs_Result52.command_guid_string);
                Assert.IsNotNull(SetCommandArgs_Result52.serialize_Json_response);
            }

            CLIEventArgs input53 = new CLIEventArgs()
            {
                commandLineInput = new CommandLineInput()
                {
                    PluginsType = "AUDIO",
                    Command = "GET",
                    TargetType = "TestAPP",
                    TargetFeature = "INAPPUSBKVM",
                    isCliRunAdmin = false,
                    Options = new List<CommandType_Option>() { new CommandType_Option("INAPPUSBKVM", "0X01") { Option_Name = "OPCODE" } },
                },
                command_guid_string = "test_guid"
            };
            commandLineInput = input53.commandLineInput;
            if (commandLineInput.TargetFeature == "INAPPUSBKVM")
            {
                var SetCommandArgs_Result53 = cLIDisplayPlugins.SetCommandArgs(input53, devMgrObj);  //TargetFeature INAPPUSBKVM, TargetType = "TestAPP",Command = "GET",GetOnUSBKVM
                Assert.IsNotNull(SetCommandArgs_Result53);
                Assert.IsNotNull(SetCommandArgs_Result53.command_guid_string);
                Assert.IsNotNull(SetCommandArgs_Result53.serialize_Json_response);
            }

            CLIEventArgs input54 = new CLIEventArgs()
            {
                commandLineInput = new CommandLineInput()
                {
                    PluginsType = "AUDIO",
                    Command = "GET",
                    TargetType = "TestAPP",
                    TargetFeature = "NETWORKKVMVERSION",
                    isCliRunAdmin = false,
                    Options = new List<CommandType_Option>() { new CommandType_Option("NETWORKKVMVERSION", "0X01") { Option_Name = "OPCODE" } },
                },
                command_guid_string = "test_guid"
            };
            commandLineInput = input54.commandLineInput;
            if (commandLineInput.TargetFeature == "NETWORKKVMVERSION")
            {
                var SetCommandArgs_Result54 = cLIDisplayPlugins.SetCommandArgs(input54, devMgrObj);  //TargetFeature NETWORKKVMVERSION, TargetType = "TestAPP",Command = "GET",GetOnUSBKVM
                Assert.IsNotNull(SetCommandArgs_Result54);
                Assert.IsNotNull(SetCommandArgs_Result54.command_guid_string);
                Assert.IsNotNull(SetCommandArgs_Result54.serialize_Json_response);
            }

            CLIEventArgs input55 = new CLIEventArgs()
            {
                commandLineInput = new CommandLineInput()
                {
                    PluginsType = "AUDIO",
                    Command = "SET",
                    TargetType = "TestAPP",
                    TargetFeature = "NETWORKKVM",
                    isCliRunAdmin = false,
                    Options = new List<CommandType_Option>() { new CommandType_Option("NETWORKKVM", "0X01") { Option_Name = "OPCODE" } },
                },
                command_guid_string = "test_guid"
            };
            commandLineInput = input55.commandLineInput;
            if (commandLineInput.TargetFeature == "NETWORKKVM")
            {
                var SetCommandArgs_Result55 = cLIDisplayPlugins.SetCommandArgs(input55, devMgrObj);  //TargetFeature NETWORKKVM, TargetType = "TestAPP",Command = "GET",GetOnUSBKVM,wait 10s
                Assert.IsNotNull(SetCommandArgs_Result55);
                Assert.IsNotNull(SetCommandArgs_Result55.command_guid_string);
                Assert.IsNotNull(SetCommandArgs_Result55.serialize_Json_response);
            }

            CLIEventArgs input56 = new CLIEventArgs()
            {
                commandLineInput = new CommandLineInput()
                {
                    PluginsType = "AUDIO",
                    Command = "SET",
                    TargetType = "TestAPP",
                    TargetFeature = "NETWORKKVM",
                    isCliRunAdmin = false,
                    Options = new List<CommandType_Option>() { new CommandType_Option("NETWORKKVM", "0X01") { Option_Name = "OPCODE" } },
                },
                command_guid_string = "test_guid"
            };
            commandLineInput = input56.commandLineInput;
            if (commandLineInput.TargetFeature == "NETWORKKVM")
            {
                var SetCommandArgs_Result56 = cLIDisplayPlugins.SetCommandArgs(input56, devMgrObj);  //TargetFeature NETWORKKVM, TargetType = "TestAPP",Command = "SET",GetOnUSBKVM
                Assert.IsNotNull(SetCommandArgs_Result56);
                Assert.IsNotNull(SetCommandArgs_Result56.command_guid_string);
                Assert.IsNotNull(SetCommandArgs_Result56.serialize_Json_response);
            }

            CLIEventArgs input57 = new CLIEventArgs()
            {
                commandLineInput = new CommandLineInput()
                {
                    PluginsType = "AUDIO",
                    Command = "GET",
                    TargetType = "TestAPP",
                    TargetFeature = "NETWORKKVMAUTOCONNECT",
                    isCliRunAdmin = false,
                    Options = new List<CommandType_Option>() { new CommandType_Option("NETWORKKVMAUTOCONNECT", "0X01") { Option_Name = "OPCODE" } },
                },
                command_guid_string = "test_guid"
            };
            commandLineInput = input57.commandLineInput;
            if (commandLineInput.TargetFeature == "NETWORKKVMAUTOCONNECT")
            {
                var SetCommandArgs_Result57 = cLIDisplayPlugins.SetCommandArgs(input57, devMgrObj);  //TargetFeature NETWORKKVMAUTOCONNECT, TargetType = "TestAPP",Command = "SET"
                Assert.IsNotNull(SetCommandArgs_Result57);
                Assert.IsNotNull(SetCommandArgs_Result57.command_guid_string);
                Assert.IsNotNull(SetCommandArgs_Result57.serialize_Json_response);
            }

            CLIEventArgs input58 = new CLIEventArgs()
            {
                commandLineInput = new CommandLineInput()
                {
                    PluginsType = "AUDIO",
                    Command = "GET",
                    TargetType = "TestAPP",
                    TargetFeature = "NETWORKKVMCONTENTTRANSFER",
                    isCliRunAdmin = false,
                    Options = new List<CommandType_Option>() { new CommandType_Option("NETWORKKVMCONTENTTRANSFER", "0X01") { Option_Name = "OPCODE" } },
                },
                command_guid_string = "test_guid"
            };
            commandLineInput = input58.commandLineInput;
            if (commandLineInput.TargetFeature == "NETWORKKVMCONTENTTRANSFER")
            {
                var SetCommandArgs_Result58 = cLIDisplayPlugins.SetCommandArgs(input58, devMgrObj);  //TargetFeature NETWORKKVMCONTENTTRANSFER, TargetType = "TestAPP",Command = "SET"
                Assert.IsNotNull(SetCommandArgs_Result58);
                Assert.IsNotNull(SetCommandArgs_Result58.command_guid_string);
                Assert.IsNotNull(SetCommandArgs_Result58.serialize_Json_response);
            }

            CLIEventArgs input59 = new CLIEventArgs()
            {
                commandLineInput = new CommandLineInput()
                {
                    PluginsType = "AUDIO",
                    Command = "SET",
                    TargetType = "TestAPP",
                    TargetFeature = "NETWORKKVMCONTENTTRANSFER",
                    isCliRunAdmin = false,
                    Options = new List<CommandType_Option>() { new CommandType_Option("NETWORKKVMCONTENTTRANSFER", "0X01") { Option_Name = "OPCODE" } },
                },
                command_guid_string = "test_guid"
            };
            commandLineInput = input59.commandLineInput;
            if (commandLineInput.TargetFeature == "NETWORKKVMCONTENTTRANSFER")
            {
                var SetCommandArgs_Result59 = cLIDisplayPlugins.SetCommandArgs(input59, devMgrObj);  //TargetFeature NETWORKKVMCONTENTTRANSFER, TargetType = "TestAPP",Command = "SET",
                Assert.IsNotNull(SetCommandArgs_Result59);
                Assert.IsNotNull(SetCommandArgs_Result59.command_guid_string);
                Assert.IsNotNull(SetCommandArgs_Result59.serialize_Json_response);
            }

            CLIEventArgs input60 = new CLIEventArgs()
            {
                commandLineInput = new CommandLineInput()
                {
                    PluginsType = "AUDIO",
                    Command = "SET",
                    TargetType = "TestAPP",
                    TargetFeature = "NETWORKKVMINCOMINGPORT",
                    isCliRunAdmin = false,
                    Options = new List<CommandType_Option>() { new CommandType_Option("NETWORKKVMINCOMINGPORT", "100") { Option_Name = "OPCODE" } },
                },
                command_guid_string = "test_guid"
            };
            commandLineInput = input60.commandLineInput;
            if (commandLineInput.TargetFeature == "NETWORKKVMINCOMINGPORT")
            {
                var SetCommandArgs_Result60 = cLIDisplayPlugins.SetCommandArgs(input60, devMgrObj);  //TargetFeature NETWORKKVMINCOMINGPORT, TargetType = "TestAPP",Command = "SET"
                Assert.IsNotNull(SetCommandArgs_Result60);
                Assert.IsNotNull(SetCommandArgs_Result60.command_guid_string);
                Assert.IsNotNull(SetCommandArgs_Result60.serialize_Json_response);
            }

            CLIEventArgs input61 = new CLIEventArgs()
            {
                commandLineInput = new CommandLineInput()
                {
                    PluginsType = "AUDIO",
                    Command = "GET",
                    TargetType = "TestAPP",
                    TargetFeature = "NETWORKKVMOUTGOINGPORT",
                    isCliRunAdmin = false,
                    Options = new List<CommandType_Option>() { new CommandType_Option("NETWORKKVMOUTGOINGPORT", "100") { Option_Name = "OPCODE" } },
                },
                command_guid_string = "test_guid"
            };
            commandLineInput = input61.commandLineInput;
            if (commandLineInput.TargetFeature == "NETWORKKVMOUTGOINGPORT")
            {
                var SetCommandArgs_Result61 = cLIDisplayPlugins.SetCommandArgs(input61, devMgrObj);  //TargetFeature NETWORKKVMOUTGOINGPORT, TargetType = "TestAPP",Command = "GET"
                Assert.IsNotNull(SetCommandArgs_Result61);
                Assert.IsNotNull(SetCommandArgs_Result61.command_guid_string);
                Assert.IsNotNull(SetCommandArgs_Result61.serialize_Json_response);
            }

            CLIEventArgs input62 = new CLIEventArgs()
            {
                commandLineInput = new CommandLineInput()
                {
                    PluginsType = "AUDIO",
                    Command = "GET",
                    TargetType = "TestAPP",
                    TargetFeature = "NETWORKKVMCONTENTTRANSFERPORT",
                    isCliRunAdmin = false,
                    Options = new List<CommandType_Option>() { new CommandType_Option("NETWORKKVMCONTENTTRANSFERPORT", "100") { Option_Name = "OPCODE" } },
                },
                command_guid_string = "test_guid"
            };
            commandLineInput = input62.commandLineInput;
            if (commandLineInput.TargetFeature == "NETWORKKVMCONTENTTRANSFERPORT")
            {
                var SetCommandArgs_Result62 = cLIDisplayPlugins.SetCommandArgs(input62, devMgrObj);  //TargetFeature NETWORKKVMCONTENTTRANSFERPORT, TargetType = "TestAPP",Command = "GET"
                Assert.IsNotNull(SetCommandArgs_Result62);
                Assert.IsNotNull(SetCommandArgs_Result62.command_guid_string);
                Assert.IsNotNull(SetCommandArgs_Result62.serialize_Json_response);
            }

            CLIEventArgs input63 = new CLIEventArgs()
            {
                commandLineInput = new CommandLineInput()
                {
                    PluginsType = "AUDIO",
                    Command = "GET",
                    TargetType = "TestAPP",
                    TargetFeature = "NETWORKKVMACCESSRESET",
                    isCliRunAdmin = false,
                    Options = new List<CommandType_Option>() { new CommandType_Option("NETWORKKVMACCESSRESET", "100") { Option_Name = "OPCODE" } },
                },
                command_guid_string = "test_guid"
            };
            commandLineInput = input63.commandLineInput;
            if (commandLineInput.TargetFeature == "NETWORKKVMACCESSRESET")
            {
                var SetCommandArgs_Result63 = cLIDisplayPlugins.SetCommandArgs(input63, devMgrObj);  //TargetFeature NETWORKKVMACCESSRESET, TargetType = "TestAPP",Command = "GET"
                Assert.IsNotNull(SetCommandArgs_Result63);
                Assert.IsNotNull(SetCommandArgs_Result63.command_guid_string);
                Assert.IsNotNull(SetCommandArgs_Result63.serialize_Json_response);
            }

            CLIEventArgs input64 = new CLIEventArgs()
            {
                commandLineInput = new CommandLineInput()
                {
                    PluginsType = "AUDIO",
                    Command = "GET",
                    TargetType = "TestAPP",
                    TargetFeature = "EXPORTSETTINGS",
                    isCliRunAdmin = false,
                    Options = new List<CommandType_Option>() { new CommandType_Option("EXPORTSETTINGS", "100") { Option_Name = "OPCODE" } },
                },
                command_guid_string = "test_guid"
            };
            commandLineInput = input64.commandLineInput;
            if (commandLineInput.TargetFeature == "EXPORTSETTINGS")
            {
                var SetCommandArgs_Result64 = cLIDisplayPlugins.SetCommandArgs(input64, devMgrObj);  //TargetFeature EXPORTSETTINGS, TargetType = "TestAPP",Command = "GET"
                Assert.IsNotNull(SetCommandArgs_Result64);
                Assert.IsNotNull(SetCommandArgs_Result64.command_guid_string);
                Assert.IsNotNull(SetCommandArgs_Result64.serialize_Json_response);
            }

            CLIEventArgs input65 = new CLIEventArgs()
            {
                commandLineInput = new CommandLineInput()
                {
                    PluginsType = "AUDIO",
                    Command = "SET",
                    TargetType = "TestAPP",
                    TargetFeature = "EXPORTSETTINGS",
                    isCliRunAdmin = false,
                    Options = new List<CommandType_Option>() { new CommandType_Option("EXPORTSETTINGS", "100") { Option_Name = "OPCODE" } },
                },
                command_guid_string = "test_guid"
            };
            commandLineInput = input65.commandLineInput;
            if (commandLineInput.TargetFeature == "EXPORTSETTINGS")
            {
                var SetCommandArgs_Result65 = cLIDisplayPlugins.SetCommandArgs(input65, devMgrObj);  //TargetFeature EXPORTSETTINGS, TargetType = "TestAPP",Command = "SET"
                Assert.IsNotNull(SetCommandArgs_Result65);
                Assert.IsNotNull(SetCommandArgs_Result65.command_guid_string);
                Assert.IsNotNull(SetCommandArgs_Result65.serialize_Json_response);
            }

            CLIEventArgs input66 = new CLIEventArgs()
            {
                commandLineInput = new CommandLineInput()
                {
                    PluginsType = "AUDIO",
                    Command = "SET",
                    TargetType = "TestAPP",
                    TargetFeature = "IMPORTSETTINGS",
                    isCliRunAdmin = false,
                    Options = new List<CommandType_Option>() { new CommandType_Option("IMPORTSETTINGS", "100") { Option_Name = "OPCODE" } },
                },
                command_guid_string = "test_guid"
            };
            commandLineInput = input66.commandLineInput;
            if (commandLineInput.TargetFeature == "IMPORTSETTINGS")
            {
                var SetCommandArgs_Result66 = cLIDisplayPlugins.SetCommandArgs(input66, devMgrObj);  //TargetFeature IMPORTSETTINGS, TargetType = "TestAPP",Command = "SET"
                Assert.IsNotNull(SetCommandArgs_Result66);
                Assert.IsNotNull(SetCommandArgs_Result66.command_guid_string);
                Assert.IsNotNull(SetCommandArgs_Result66.serialize_Json_response);
            }

            CLIEventArgs input67 = new CLIEventArgs()
            {
                commandLineInput = new CommandLineInput()
                {
                    PluginsType = "AUDIO",
                    Command = "SET",
                    TargetType = "TestAPP",
                    TargetFeature = "TestCommand",
                    isCliRunAdmin = false,
                    Options = new List<CommandType_Option>() { new CommandType_Option("TestCommand", "100") { Option_Name = "OPCODE" } },
                },
                command_guid_string = "test_guid"
            };
            commandLineInput = input67.commandLineInput;
            if (commandLineInput.TargetFeature == "TestCommand")
            {
                var SetCommandArgs_Result67 = cLIDisplayPlugins.SetCommandArgs(input67, devMgrObj);  //default
                Assert.IsNotNull(SetCommandArgs_Result67);
                Assert.IsNotNull(SetCommandArgs_Result67.command_guid_string);
                Assert.IsNotNull(SetCommandArgs_Result67.serialize_Json_response);
            }
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            cLIDisplayPlugins.Dispose();
        }
    }
}