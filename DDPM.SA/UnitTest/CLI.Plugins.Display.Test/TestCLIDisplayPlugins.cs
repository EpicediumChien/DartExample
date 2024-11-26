using DDPM.CLI.Plugins.Display;
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
            devMgr2.Setup(m => m.GetCapabilitiesString(It.IsAny<MonitorInfo>())).Returns(Task.FromResult(monitorInfo1.CapabilityString));
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
            devMgr2.Setup(m => m.GetVCPCapabilities(It.IsAny<MonitorInfo>())).Returns(Task.FromResult(monitorInfo1.CapabilityString));
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

        [OneTimeTearDown]
        public void TearDown()
        {
            cLIDisplayPlugins.Dispose();
        }
    }
}