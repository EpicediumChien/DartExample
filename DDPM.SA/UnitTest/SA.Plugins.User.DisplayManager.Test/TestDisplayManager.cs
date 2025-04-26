using DDPM.SA.Common;
using DDPM.SA.Common.Display;
using DDPM.SA.Common.Interfaces;
using DDPM.SA.Plugins.User.DisplayProperties;
using DDPM.SA.Plugins.User.EasyArrange;
using DDPM.SA.Plugins.User.PipPbpManger;
using Dell.Client.Framework.Interfaces;
using Dell.Client.Framework.UnitTestShared.Tests;
using Moq;
using VcpCore.Common;
using VcpCore.Interfaces;
using VcpCore.Plugins;
using Windows.Devices.Display.Core;

namespace DDPM.SA.Plugins.User.DisplayManager.Test
{
    public class TestDisplayManager
    {
        private Mock<IAgent> DisplayMangerAgent { get; } = new();
        private Mock<IAgent> VcpCoreAgent { get; } = new();
        private Mock<IAgent> PipPbpAgent = new();
        private Mock<IAgent> DisplayPropertiesAgent { get; } = new();
        private Mock<IAgent> EAPluginAgent { get; } = new();

        private MonitorInfo monitorInfo = new MonitorInfo();
        private Mock<IVcpCoreService> VcpCoreService { get; } = new();
        private Mock<IPipPbpService> PipPbpService { get; } = new();
        private Mock<IEasyArrangeService> EasyArrangeService { get; } = new();
        private Mock<IDisplayProperties> DisplayPropertiesService { get; } = new();

        private MonitorInfo monitorInfo1 = new MonitorInfo()
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

        private static List<ALSConfig> AllALSConfig;

        public static List<ALSConfig> _AllALSConfig
        {
            get => AllALSConfig;
            set => AllALSConfig = value;
        }

        private ALSConfig aconfig = new ALSConfig()
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
            ModelName = "DISPLAY7",
            Edid = new EDID() { SerialNumber = "808597589", ServiceTag = "123456" },
            AutoBrightnessRangeLevel = new AutoBrightnessRangeLevel() { level_name = "Low", level_value = 0 }
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

        private DisplayPropertiesPlugins CreateInitializedisplayPropertiesPlugin()
        {
            DisplayPropertiesAgent.Setup(x => x.PluginManager.FindPluginByGuid(Guid.Parse(DDPM.SA.Common.IDs.DisplayProperties_PLUGIN_ID)));

            return new DisplayPropertiesPlugins(DisplayPropertiesAgent.Object);
        }

        private EAPlugin CreateInitializeEasyArrangeplugin()
        {
            EAPluginAgent.Setup(x => x.PluginManager.FindPluginByGuid(Guid.Parse(DDPM.SA.Common.IDs.DDPM_EAPlugin_PLUGIN_ID)));

            return new EAPlugin(EAPluginAgent.Object);
        }

        private DisplayMangerPlugin displayPlugin;
        private DisplayDataManger displayData;
        private VcpCorePlugin vcpCorePlugin;
        private PipPbpMangerPlugin pipPbpMangerPlugin;
        private Dictionary<string, Dictionary<string, string>> getstr;
        private DisplayPropertiesPlugins displayPropertiesPlugin;
        private EAPlugin EasyArrangeplugin;

        [OneTimeSetUp]
        public void Setup()
        {
            displayPlugin = CreateInitializeDisplayMangerPlugin();
            displayData = new DisplayDataManger();
            vcpCorePlugin = CreateInitializeVcpCorePlugin();
            pipPbpMangerPlugin = CreateInitializePipPbpPlugin();
            displayPropertiesPlugin = CreateInitializedisplayPropertiesPlugin();
            EasyArrangeplugin = CreateInitializeEasyArrangeplugin();

            PrivateObject privateObject = new PrivateObject(displayPlugin);
            PrivateObject privatevcp = new PrivateObject(vcpCorePlugin);
            PrivateObject privatepippbp = new PrivateObject(pipPbpMangerPlugin);
            PrivateObject privatedisplayProperties = new PrivateObject(displayPropertiesPlugin);

            privateObject.SetField("_pipPbpService", pipPbpMangerPlugin as IPipPbpService);
            privateObject.SetField("_VcpCorePlugin", vcpCorePlugin as IVcpCoreService);

            privatepippbp.SetField("_DisplayManagerPlugin", displayPlugin as IDisplayService);
            privateObject.SetField("_DisplayPropertiesPlugin", displayPropertiesPlugin as IDisplayProperties);
            privateObject.SetField("_eaService", EasyArrangeplugin as IEasyArrangeService);
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
            privatedispalypluginObject.SetField("_VcpCorePlugin", VcpCoreServiceObject);//set _VcpCorePlugin为我们mock的对象
            var getMonitors = displayPlugin.GetMonitors().Result;
            Assert.Greater(getMonitors.Count, 0);
        }

        [Test]
        public void TestRe_GetMonitors()
        {
            List<MonitorInfo> _allInfoMonitors = new List<MonitorInfo>();
            _allInfoMonitors.Add(monitorInfo1);
            VcpCoreService.Setup(x => x.Re_GetMonitors(It.IsAny<CancellationToken>())).Returns(Task.FromResult(_allInfoMonitors)); //method update
            var VcpCoreServiceObject = VcpCoreService.Object;
            PrivateObject privatedispalypluginObject = new PrivateObject(displayPlugin);
            privatedispalypluginObject.SetField("_VcpCorePlugin", VcpCoreServiceObject);
            var re_GetMonitors = displayPlugin.Re_GetMonitors(new CancellationTokenSource().Token).Result;
            Assert.Greater(re_GetMonitors.Count, 0);
        }

        [Test]
        public void TestGetCapabilitiesString()
        {
            string capabilitiesString = "(prot(monitor)type(LCD)model(U2424H)cmds(01 02 03 07 0C E3 F3)vcp(02 04 05 08 10 12 14(01 04 05 06 08 09 0B 0C)";
            var VcpCoreServiceObject = VcpCoreService.Object;
            PrivateObject privatedispalypluginObject = new PrivateObject(displayPlugin);
            privatedispalypluginObject.SetFieldOrProperty("_VcpCorePlugin", VcpCoreServiceObject);
            VcpCoreService.Setup(x => x.GetCapabilitiesString(It.IsAny<MonitorInfo>(), It.IsAny<Guid>(), It.IsAny<Priority>())).Returns(Task.FromResult(capabilitiesString));
            var getCapabilitiesString = displayPlugin.GetCapabilitiesString(monitorInfo1).Result;
            Assert.Greater(capabilitiesString.Length, 0);
            Assert.That(capabilitiesString, Is.EqualTo(getCapabilitiesString));
        }

        [Test]
        public void TestGetVCPCapabilities()
        {
            string VcpCapabilities = "\"{\\r\\n  \\\"Index\\\": 0,\\r\\n  \\\"ModelName\\\": \\\"DELLU2424H\\\",\\r\\n  \\\"SerialNumber\\\": \\\"926168130\\\",\\r\\n  \\\"ServiceTag\\\": \\\"CN073K0\\\"";
            VcpCoreService.Setup(x => x.GetVCPCapabilities(It.IsAny<MonitorInfo>(), It.IsAny<Guid>(), It.IsAny<Priority>())).Returns(Task.FromResult(VcpCapabilities));
            var VcpCoreServiceObject = VcpCoreService.Object;
            PrivateObject privatedispalypluginObject = new PrivateObject(displayPlugin);
            privatedispalypluginObject.SetField("_VcpCorePlugin", VcpCoreServiceObject);
            var getVCPCapabilities = displayPlugin.GetVCPCapabilities(monitorInfo1).Result;
            Assert.Greater(getVCPCapabilities.Length, 0);
            Assert.That(VcpCapabilities, Is.EqualTo(getVCPCapabilities));
        }

        [Test]
        public void TestGetVCPCapability()
        {
            //FunctionName="colorpreset"
            string funName = "colorpreset";
            int opt = 0;
            MonitorInfo monitorInfo_ = monitorInfo1;
            ObjGetVCP ObjGetvcp = new ObjGetVCP() { result = true, value = "High Data Speed" };
            var ObjGetvcpValue = ObjGetvcp.value;
            VcpCoreService.Setup(x => x.GetVCPCapability(It.IsAny<MonitorInfo>(), It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<Priority>())).Returns(Task.FromResult(ObjGetvcp));
            var VcpCoreServiceObject = VcpCoreService.Object;
            PrivateObject privatedispalypluginObject = new PrivateObject(displayPlugin);
            privatedispalypluginObject.SetField("_VcpCorePlugin", VcpCoreServiceObject);
            privatedispalypluginObject.SetField("_displayDataManger", displayData);

            List<DisplayData> _displayData = new List<DisplayData>() { new DisplayData() { Model = monitorInfo_.modelName, ServiceTag = monitorInfo_.edid.ServiceTag, Color = new Color() { color_DisHDR = "Not HDR" } } };
            PrivateObject privatedispalypluginObject_ = new PrivateObject(displayData);
            privatedispalypluginObject_.SetFieldOrProperty("_displayData", _displayData);
            string capabilitystring = "(prot(monitor)type(LCD)model(U2424H)cmds(01 02 03 07 0C E3 F3)vcp(02 04 05 08 10 12 14(01 04 05 06 08 09 0B 0C)E5 E7(02 03) E2(00 02 04 0C 0D 0F)";

            monitorInfo_.CapabilityString = capabilitystring;
            var getVCPCapability = displayPlugin.GetVCPCapability(monitorInfo_, funName, opt: opt).Result;
            Assert.IsTrue(getVCPCapability.result);
            Assert.IsNotNull(getVCPCapability);

            //FunctionName ! ="colorpreset"
            funName = "Testcolorpreset";
            var getVCPCapability_result2 = displayPlugin.GetVCPCapability(monitorInfo_, funName, opt: opt).Result;
            Assert.IsNotNull(getVCPCapability_result2);
        }

        [Test]
        public void TestGetVCPCapability_()
        {
            byte code = 0xE7;
            int opt = 0;
            ObjGetVCP ObjGetvcp = new ObjGetVCP() { result = true, value = 10u }; //0xE7
            var ObjGetvcpValue = ObjGetvcp.value;
            VcpCoreService.Setup(x => x.GetVCPCapability(It.IsAny<MonitorInfo>(), It.IsAny<byte>(), It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<Priority>())).Returns(Task.FromResult(ObjGetvcp));
            var VcpCoreServiceObject = VcpCoreService.Object;
            PrivateObject privatedispalypluginObject = new PrivateObject(displayPlugin);
            privatedispalypluginObject.SetField("_VcpCorePlugin", VcpCoreServiceObject);
            var getVCPCapability_ = displayPlugin.GetVCPCapability(monitorInfo1, code, opt: opt).Result;
            Assert.IsTrue(getVCPCapability_.result);
            Assert.That(ObjGetvcpValue, Is.EqualTo(getVCPCapability_.value));
        }

        [Test]
        public void TestSetVCPCapability()
        {
            byte code = 0X21;
            uint val = 16;    //Brightness = 16
            bool setVCPCapability = true;
            VcpCoreService.Setup(x => x.SetVCPCapability(It.IsAny<MonitorInfo>(), It.IsAny<byte>(), It.IsAny<uint>(), It.IsAny<Guid>(), It.IsAny<Priority>())).Returns(Task.FromResult(setVCPCapability));
            var VcpCoreServiceObject = VcpCoreService.Object;
            PrivateObject privatedispalypluginObject = new PrivateObject(displayPlugin);
            privatedispalypluginObject.SetField("_VcpCorePlugin", VcpCoreServiceObject);
            var SetVCPCapabilityResult = displayPlugin.SetVCPCapability(monitorInfo1, code, val).Result;
            Assert.IsTrue(SetVCPCapabilityResult);
        }

        [Test]
        public void TestSetVCPCapability_()
        {
            string FuntionName = "colorpreset";
            string val = "Warm";
            bool setVCPCapability_ = true;
            VcpCoreService.Setup(x => x.SetVCPCapability(It.IsAny<MonitorInfo>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Priority>())).Returns(Task.FromResult(setVCPCapability_));
            var VcpCoreServiceObject = VcpCoreService.Object;
            PrivateObject privatedispalypluginObject = new PrivateObject(displayPlugin);
            privatedispalypluginObject.SetField("_VcpCorePlugin", VcpCoreServiceObject);
            var SetVCPCapabilityResult_ = displayPlugin.SetVCPCapability(monitorInfo1, FuntionName, val).Result;
            Assert.IsTrue(SetVCPCapabilityResult_);
        }

        [Test]
        public void TestGetInputSourcelist()
        {
            string getVcpCapabilities = "{'CapsDataMap' : {'Input Select': ['Thunderbolt-1', 'DisplayPort-1','HDMI-1']}}";
            VcpCoreService.Setup(x => x.GetVCPCapabilities(It.IsAny<MonitorInfo>(), It.IsAny<Guid>(), It.IsAny<Priority>())).Returns(Task.FromResult(getVcpCapabilities));
            var VcpCoreServiceObject = VcpCoreService.Object;
            PrivateObject privatedispalypluginObject = new PrivateObject(displayPlugin);
            privatedispalypluginObject.SetField("_VcpCorePlugin", VcpCoreServiceObject);

            //List<string> USBbUpstreamList = new List<string>();
            //USBbUpstreamList.Add("USB-C1");
            //USBbUpstreamList.Add("Thunderbolt-1");
            //USBbUpstreamList.Add("USB-B1");
            //USBbUpstreamList.Add("USB-B2");
            //privatedispalypluginObject.SetField("usbUpstreamList", USBbUpstreamList);

            Dictionary<string, List<string>> capabilityDic = new Dictionary<string, List<string>>();
            capabilityDic.Add("EE", new List<string> { "value1" });
            capabilityDic.Add("EF", new List<string> { "value2" });
            monitorInfo1.CapabilityDic = capabilityDic;  //GetUSBUpstreamList 里面包含EE

            List<InputSourceObject> inputSourceObjects = new List<InputSourceObject>();
            inputSourceObjects.Add(new InputSourceObject() { Name = "Thunderbolt", value = 25 });
            inputSourceObjects.Add(new InputSourceObject() { Name = "DisplayPort", value = 15 });
            inputSourceObjects.Add(new InputSourceObject() { Name = "HDMI", value = 17 });

            ObjGetVCP OBjbjGetVCP2 = new ObjGetVCP() { result = true, value = inputSourceObjects };//0xE7 add input list
            ObjGetVCP OBjbjGetVCP = new ObjGetVCP() { result = true, value = 35856u };//0XEE
            VcpCoreService.Setup(x => x.GetVCPCapability(It.IsAny<MonitorInfo>(), It.IsAny<byte>(), It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<Priority>())).Returns(Task.FromResult(OBjbjGetVCP));
            VcpCoreService.Setup(x => x.GetVCPCapability(It.IsAny<MonitorInfo>(), It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<Priority>())).Returns(Task.FromResult(OBjbjGetVCP2));
            var VcpCoreServiceObject1 = VcpCoreService.Object;
            privatedispalypluginObject.SetField("_VcpCorePlugin", VcpCoreServiceObject1);

            var GetInputSourcelist = displayPlugin.GetInputSourcelist(monitorInfo1).Result;
            Assert.IsNotNull(GetInputSourcelist);
            Assert.Greater(GetInputSourcelist.Count, 0);
        }

        [Test]
        public void TestSetInputSourcelist()
        {
            var inputList = new Dictionary<string, InputInfo>
                    {
                { "HDMI 1", new InputInfo { InputName = "HDMI-1", USBUpstream = "Upstream 1" } },
                { "HDMI 2", new InputInfo { InputName = "HDMI-2", USBUpstream = "Upstream 2" } }
                    };
            bool result = displayPlugin.SetInputSourcelist(inputList).Result;
            PrivateObject privateObject = new PrivateObject(displayPlugin);
            var inputSourcelist = privateObject.GetField("inputSourcelist") as Dictionary<string, InputInfo>;
            Assert.IsTrue(result);
            CollectionAssert.AreEqual(inputSourcelist, inputList);
        }

        [Test]
        public void TestGetUSBUpstreamList()
        {
            Dictionary<string, List<string>> capabilityDic = new Dictionary<string, List<string>>();
            capabilityDic.Add("EE", new List<string> { "value1" });
            capabilityDic.Add("E7", new List<string> { "value2" });
            monitorInfo1.CapabilityDic = capabilityDic;  //GetUSBUpstreamList 里面包含EE和E7

            PrivateObject privatedispalypluginObject = new PrivateObject(displayPlugin);
            ObjGetVCP objGetVCPEE = new ObjGetVCP() { result = true, value = 35856u };//0XEE
            VcpCoreService.Setup(x => x.GetVCPCapability(It.IsAny<MonitorInfo>(), It.IsAny<byte>(), It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<Priority>())).Returns(Task.FromResult(objGetVCPEE));
            var VcpCoreServiceObject1 = VcpCoreService.Object;
            privatedispalypluginObject.SetField("_VcpCorePlugin", VcpCoreServiceObject1);

            if (monitorInfo1.CapabilityDic.ContainsKey("EE") && monitorInfo1.CapabilityDic.ContainsKey("E7"))
            {
                var getUSBUpstreamList = displayPlugin.GetUSBUpstreamList(monitorInfo1).Result;
                Assert.IsNotNull(getUSBUpstreamList);
                Assert.Greater(getUSBUpstreamList.Count, 0);
            }
            else
            {
                var GetUSBUpstreamLists = displayPlugin.GetUSBUpstreamList(monitorInfo).Result;
                Assert.IsNotNull(GetUSBUpstreamLists);
            }
        }

        [Test]
        public void TestGetUSBUpstream()
        {
            Dictionary<string, List<string>> capabilityDic = new Dictionary<string, List<string>>();
            capabilityDic.Add("EE", new List<string> { "value1" });
            capabilityDic.Add("EF", new List<string> { "value2" });
            capabilityDic.Add("E7", new List<string> { "value4" });
            monitorInfo1.CapabilityDic = capabilityDic;  //GetUSBUpstreamList 里面包含EE
            string inputsource1 = "HDMI-1";
            PrivateObject privatedispalypluginObject = new PrivateObject(displayPlugin);
            ObjGetVCP objGetVCPEE = new ObjGetVCP() { result = true, value = 188u };//0xE7 48128u
            VcpCoreService.Setup(x => x.GetVCPCapability(It.IsAny<MonitorInfo>(), It.IsAny<byte>(), It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<Priority>())).Returns(Task.FromResult(objGetVCPEE));
            var VcpCoreServiceObject1 = VcpCoreService.Object;
            privatedispalypluginObject.SetField("_VcpCorePlugin", VcpCoreServiceObject1);

            string getVcpCapabilities = "{'CapsDataMap' : {'Input Select': ['Thunderbolt-1', 'DisplayPort-1','HDMI-1']}}";
            VcpCoreService.Setup(x => x.GetVCPCapabilities(It.IsAny<MonitorInfo>(), It.IsAny<Guid>(), It.IsAny<Priority>())).Returns(Task.FromResult(getVcpCapabilities));
            var VcpCoreServiceObject = VcpCoreService.Object;
            privatedispalypluginObject.SetField("_VcpCorePlugin", VcpCoreServiceObject);

            Dictionary<string, string> Usbpstream = new Dictionary<string, string>();
            Usbpstream.Add("USB-C1", "11");
            Usbpstream.Add("Thunderbolt-1", "10");
            Usbpstream.Add("USB-B1", "01");
            Usbpstream.Add("USB-B2", "00");
            privatedispalypluginObject.SetField("USBUpstream", Usbpstream);
            string Usbpstream1 = Usbpstream.Keys.First<string>();
            if (monitorInfo1.CapabilityDic.ContainsKey("E7"))
            {
                if (monitorInfo1.CapabilityDic.ContainsKey("EE"))
                {
                    var GetUSBUpstreamResulit = displayPlugin.GetUSBUpstream(monitorInfo1, inputsource1).Result;
                    Assert.IsNotNull(GetUSBUpstreamResulit);
                    //Assert.That(Usbpstream1, Is.EqualTo(GetUSBUpstreamResulit));
                }
                else
                {
                    var GetUSBUpstreamResulit = displayPlugin.GetUSBUpstream(monitorInfo1, inputsource1).Result;
                    Assert.IsNull(GetUSBUpstreamResulit);
                }
            }
            else
            {
                var GetUSBUpstreamResulit = displayPlugin.GetUSBUpstream(monitorInfo1, inputsource1).Result;
                Assert.IsNull(GetUSBUpstreamResulit);
            }
        }

        /*[Test]
        public void TestSetUSBUpstream()
        {
            Dictionary<string, List<string>> capabilityDic = new Dictionary<string, List<string>>();
            capabilityDic.Add("EE", new List<string> { "value1" });
            capabilityDic.Add("EF", new List<string> { "value2" });
            monitorInfo1.CapabilityDic = capabilityDic;  //GetUSBUpstreamList 里面包含EE
            string inputsource1 = "HDMI-1";
            PrivateObject privatedispalypluginObject = new PrivateObject(displayPlugin);
            ObjGetVCP objGetVCPEE = new ObjGetVCP() { result = true, value = 188u };//0xE7 48128u
            VcpCoreService.Setup(x => x.GetVCPCapability(It.IsAny<MonitorInfo>(), It.IsAny<byte>(), It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<Priority>())).Returns(Task.FromResult(objGetVCPEE));
            var VcpCoreServiceObject1 = VcpCoreService.Object;
            privatedispalypluginObject.SetField("_VcpCorePlugin", VcpCoreServiceObject1);

            string getVcpCapabilities = "{'CapsDataMap' : {'Input Select': ['Thunderbolt-1', 'DisplayPort-1','HDMI-1']}}";
            VcpCoreService.Setup(x => x.GetVCPCapabilities(It.IsAny<MonitorInfo>(), It.IsAny<Guid>(), It.IsAny<Priority>())).Returns(Task.FromResult(getVcpCapabilities));
            var VcpCoreServiceObject = VcpCoreService.Object;
            privatedispalypluginObject.SetField("_VcpCorePlugin", VcpCoreServiceObject);

            bool setVCPCapability = true;
            VcpCoreService.Setup(x => x.SetVCPCapability(It.IsAny<MonitorInfo>(), It.IsAny<byte>(), It.IsAny<uint>(), It.IsAny<Guid>(), It.IsAny<Priority>())).Returns(Task.FromResult(setVCPCapability));
            var VcpCoreServiceObject2 = VcpCoreService.Object;
            privatedispalypluginObject.SetField("_VcpCorePlugin", VcpCoreServiceObject2);

            var inputlist = new Dictionary<string, InputInfo>
                    {
                { "Thunderbolt-1", new InputInfo { InputName = "Thunderbolt-1", USBUpstream = "Thunderbolt-1" } },
                { "DisplayPort-1", new InputInfo { InputName = "DisplayPort-1", USBUpstream = "USB-B1" } },
                { "HDMI-1", new InputInfo { InputName = "HDMI-1", USBUpstream = "USB-C" } }
                    };
            privatedispalypluginObject.SetField("inputSourcelist", inputlist);

            //var upstream= inputlist["HDMI-1"].USBUpstream;
            var upstream = "USB-C4";
            //Dictionary<string, string> UsbUpstream = new Dictionary<string, string>() { { "USB-C", "11" }, { "USB-B1", "00" } };
            //privatedispalypluginObject.SetField("USBUpstream", UsbUpstream);
            Dictionary<string, string> Usbpstream = new Dictionary<string, string>();
            Usbpstream.Add("USB-C1", "11");
            Usbpstream.Add("Thunderbolt-1", "10");
            Usbpstream.Add("USB-B1", "01");
            Usbpstream.Add("USB-B2", "00");
            privatedispalypluginObject.SetField("USBUpstream", Usbpstream);
            string Usbpstream1 = Usbpstream.Keys.First<string>();

            bool SetUSBUpstreamresult = displayPlugin.SetUSBUpstream(monitorInfo1, inputsource1, upstream).Result;  //Not monitorInfo.CapabilityDic.ContainsKey("E7")
            Assert.IsFalse(SetUSBUpstreamresult);

            capabilityDic.Add("E7", new List<string> { "value4" });
            monitorInfo1.CapabilityDic = capabilityDic;
            bool SetUSBUpstreamresult2 = displayPlugin.SetUSBUpstream(monitorInfo1, inputsource1, upstream).Result;  // monitorInfo.CapabilityDic.ContainsKey("E7")
            Assert.IsTrue(SetUSBUpstreamresult2);
        }*/

        [Test]
        public void TestChangeCurrentInput()
        {
            var inputlist = new Dictionary<string, InputInfo>
                    {
                { "Thunderbolt-1", new InputInfo { InputName = "Thunderbolt-1", USBUpstream = "Thunderbolt-1" } },
                { "DisplayPort-1", new InputInfo { InputName = "DisplayPort-1", USBUpstream = "USB-B1" } },
                { "HDMI-1", new InputInfo { InputName = "HDMI-1", USBUpstream = "USB-C" } }
                    };
            string input = "HDMI-1";
            Task result = displayPlugin.ChangeCurrentInput(inputlist, input);

            Assert.IsTrue(result.IsCompleted);
        }

        [Test]
        public void TestUSBSwitch()
        {
            Dictionary<string, List<string>> capabilityDic = new Dictionary<string, List<string>>();
            capabilityDic.Add("EE", new List<string> { "value1" });
            capabilityDic.Add("EF", new List<string> { "value2" });
            monitorInfo1.CapabilityDic = capabilityDic;
            string inputsource1 = "HDMI-1";
            string inputsource2 = "DisplayPort-1";
            PrivateObject privatedispalypluginObject = new PrivateObject(displayPlugin);
            ObjGetVCP objGetVCPEE = new ObjGetVCP() { result = true, value = 48128u };//0xE7
            VcpCoreService.Setup(x => x.GetVCPCapability(It.IsAny<MonitorInfo>(), It.IsAny<byte>(), It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<Priority>())).Returns(Task.FromResult(objGetVCPEE));
            var VcpCoreServiceObject1 = VcpCoreService.Object;
            privatedispalypluginObject.SetField("_VcpCorePlugin", VcpCoreServiceObject1);

            string getVcpCapabilities = "{'CapsDataMap' : {'Input Select': ['Thunderbolt-1', 'DisplayPort-1','HDMI-1']}}";
            VcpCoreService.Setup(x => x.GetVCPCapabilities(It.IsAny<MonitorInfo>(), It.IsAny<Guid>(), It.IsAny<Priority>())).Returns(Task.FromResult(getVcpCapabilities));
            var VcpCoreServiceObject = VcpCoreService.Object;
            privatedispalypluginObject.SetField("_VcpCorePlugin", VcpCoreServiceObject);

            bool setVCPCapability = true;
            VcpCoreService.Setup(x => x.SetVCPCapability(It.IsAny<MonitorInfo>(), It.IsAny<byte>(), It.IsAny<uint>(), It.IsAny<Guid>(), It.IsAny<Priority>())).Returns(Task.FromResult(setVCPCapability));
            var VcpCoreServiceObject2 = VcpCoreService.Object;
            privatedispalypluginObject.SetField("_VcpCorePlugin", VcpCoreServiceObject2);

            var inputlist = new Dictionary<string, InputInfo>
                    {
                { "Thunderbolt-1", new InputInfo { InputName = "Thunderbolt-1", USBUpstream = "Thunderbolt-1" } },
                { "DisplayPort-1", new InputInfo { InputName = "DisplayPort-1", USBUpstream = "USB-B1" } },
                { "HDMI-1", new InputInfo { InputName = "HDMI-1", USBUpstream = "USB-C" } }
                    };
            privatedispalypluginObject.SetField("inputSourcelist", inputlist);

            //var upstream= inputlist["HDMI-1"].USBUpstream;
            string upstream1 = "USB-C";
            string upstream2 = "USB-B1";

            Dictionary<string, string> UsbUpstream = new Dictionary<string, string>() { { "USB-C", "11" }, { "USB-B1", "00" } };
            privatedispalypluginObject.SetField("USBUpstream", UsbUpstream);

            bool USBSwitchResult = displayPlugin.USBSwitch(monitorInfo, inputsource1, upstream1, inputsource2, upstream2).Result;
            Assert.IsNotNull(USBSwitchResult);
            Assert.IsTrue(USBSwitchResult);
        }

        [Test]
        public void TestGetPipPbpCapabilitiesWords()
        {
            ushort[] arrayGetCapabilitiesWords = new ushort[] { 00, 20, 45, 33, 40, 26 };
            PipPbpService.Setup(x => x.GetCapabilitiesWords(It.IsAny<MonitorInfo>())).Returns(Task.FromResult(arrayGetCapabilitiesWords));
            var PipPbpServiceObject = PipPbpService.Object;
            PrivateObject privatedispalypluginObject = new PrivateObject(displayPlugin);
            privatedispalypluginObject.SetField("_pipPbpService", PipPbpServiceObject);

            if (PipPbpService != null)
            {
                var PipPbpCapabilitiesWords = displayPlugin.GetPipPbpCapabilitiesWords(monitorInfo1).Result;
                Assert.IsNotNull(PipPbpCapabilitiesWords);
                Assert.That(arrayGetCapabilitiesWords, Is.EqualTo(PipPbpCapabilitiesWords));
                PipPbpService.Verify(s => s.GetCapabilitiesWords(monitorInfo1), Times.Once);
            }
            else
            {
                var PipPbpCapabilitiesWords = displayPlugin.GetPipPbpCapabilitiesWords(monitorInfo1).Result;
                Assert.IsNull(PipPbpCapabilitiesWords);
            }
        }

        [Test]
        public void TestSetPipModeOff()
        {
            bool setPipModeOff = true;
            PipPbpService.Setup(x => x.SetPipModeOff(It.IsAny<MonitorInfo>())).Returns(Task.FromResult(setPipModeOff));
            var PipPbpServiceObject = PipPbpService.Object;
            PrivateObject privatedispalypluginObject = new PrivateObject(displayPlugin);
            privatedispalypluginObject.SetField("_pipPbpService", PipPbpServiceObject);
            if (PipPbpService != null)
            {
                var PipModeOffResult = displayPlugin.SetPipModeOff(monitorInfo1).Result;
                Assert.That(setPipModeOff, Is.EqualTo(PipModeOffResult));
                PipPbpService.Verify(s => s.SetPipModeOff(monitorInfo1), Times.Once);
            }
            else
            {
                var PipModeOffResult = displayPlugin.SetPipModeOff(monitorInfo1).Result;
                Assert.IsFalse(PipModeOffResult);
            }
        }

        [Test]
        public void TestSetPipModeSmall()
        {
            bool setPipModeSmall = true;
            PipPbpService.Setup(x => x.SetPipModeSmall(It.IsAny<MonitorInfo>())).Returns(Task.FromResult(setPipModeSmall));
            var PipPbpServiceObject = PipPbpService.Object;
            PrivateObject privatedispalypluginObject = new PrivateObject(displayPlugin);
            privatedispalypluginObject.SetField("_pipPbpService", PipPbpServiceObject);

            if (PipPbpService != null)
            {
                var SetPipModeSmallResult = displayPlugin.SetPipModeSmall(monitorInfo1).Result;
                Assert.That(setPipModeSmall, Is.EqualTo(SetPipModeSmallResult));
                PipPbpService.Verify(s => s.SetPipModeSmall(monitorInfo1), Times.Once);
            }
            else
            {
                var SetPipModeSmallResult = displayPlugin.SetPipModeSmall(monitorInfo1).Result;
                Assert.IsFalse(SetPipModeSmallResult);
            }
        }

        [Test]
        public void TestSetPipModeLarge()
        {
            bool setPipModeLarge = true;
            PipPbpService.Setup(x => x.SetPipModeLarge(It.IsAny<MonitorInfo>())).Returns(Task.FromResult(setPipModeLarge));
            var PipPbpServiceObject = PipPbpService.Object;
            PrivateObject privatedispalypluginObject = new PrivateObject(displayPlugin);
            privatedispalypluginObject.SetField("_pipPbpService", PipPbpServiceObject);

            if (PipPbpService != null)
            {
                var SetPipModeLargeResult = displayPlugin.SetPipModeLarge(monitorInfo1).Result;
                Assert.That(setPipModeLarge, Is.EqualTo(SetPipModeLargeResult));
                PipPbpService.Verify(s => s.SetPipModeLarge(monitorInfo1), Times.Once);
            }
            else
            {
                var SetPipModeLargeResult = displayPlugin.SetPipModeLarge(monitorInfo1).Result;
                Assert.IsFalse(SetPipModeLargeResult);
            }
        }

        [Test]
        public void TestTogglePipSize()
        {
            bool togglePipSize = true;
            PipPbpService.Setup(x => x.TogglePipSize(It.IsAny<MonitorInfo>())).Returns(Task.FromResult(togglePipSize));
            var PipPbpServiceObject = PipPbpService.Object;
            PrivateObject privatedispalypluginObject = new PrivateObject(displayPlugin);
            privatedispalypluginObject.SetField("_pipPbpService", PipPbpServiceObject);

            if (PipPbpService != null)
            {
                var TogglePipSizeResult = displayPlugin.TogglePipSize(monitorInfo1).Result;
                Assert.That(togglePipSize, Is.EqualTo(TogglePipSizeResult));
                PipPbpService.Verify(s => s.TogglePipSize(monitorInfo1), Times.Once);
            }
            else
            {
                var TogglePipSizeResult = displayPlugin.TogglePipSize(monitorInfo1).Result;
                Assert.IsFalse(TogglePipSizeResult);
            }
        }

        [Test]
        public void TestTogglePipPosition()
        {
            bool togglePipPosition = true;
            PipPbpService.Setup(x => x.TogglePipPosition(It.IsAny<MonitorInfo>())).Returns(Task.FromResult(togglePipPosition));
            var PipPbpServiceObject = PipPbpService.Object;
            PrivateObject privatedispalypluginObject = new PrivateObject(displayPlugin);
            privatedispalypluginObject.SetField("_pipPbpService", PipPbpServiceObject);

            if (PipPbpService != null)
            {
                var TogglePipPositionResult = displayPlugin.TogglePipPosition(monitorInfo1).Result;
                Assert.That(togglePipPosition, Is.EqualTo(TogglePipPositionResult));
                PipPbpService.Verify(s => s.TogglePipPosition(monitorInfo1), Times.Once);
            }
            else
            {
                var TogglePipPositionResult = displayPlugin.TogglePipPosition(monitorInfo1).Result;
                Assert.IsFalse(TogglePipPositionResult);
            }
        }

        [Test]
        public void TestSetPbpMode()
        {
            bool setPbpMode = true;
            UInt16 modeCode = 20;
            PipPbpService.Setup(x => x.SetPbpMode(It.IsAny<MonitorInfo>(), It.IsAny<UInt16>())).Returns(Task.FromResult(setPbpMode));
            var PipPbpServiceObject = PipPbpService.Object;
            PrivateObject privatedispalypluginObject = new PrivateObject(displayPlugin);
            privatedispalypluginObject.SetField("_pipPbpService", PipPbpServiceObject);

            if (PipPbpService != null)
            {
                var SetPbpModeResult = displayPlugin.SetPbpMode(monitorInfo1, modeCode).Result;
                Assert.That(setPbpMode, Is.EqualTo(SetPbpModeResult));
                PipPbpService.Verify(s => s.SetPbpMode(monitorInfo1, modeCode), Times.Once);
            }
            else
            {
                var SetPbpModeResult = displayPlugin.SetPbpMode(monitorInfo1, modeCode).Result;
                Assert.IsFalse(SetPbpModeResult);
            }
        }

        [Test]
        public void TestVideoSwap()
        {
            bool VideoSwap = true;
            UInt16 x = 1;
            UInt16 y = 2;
            PipPbpService.Setup(x => x.VideoSwap(It.IsAny<MonitorInfo>(), It.IsAny<UInt16>(), It.IsAny<UInt16>())).Returns(Task.FromResult(VideoSwap));
            var PipPbpServiceObject = PipPbpService.Object;
            PrivateObject privatedispalypluginObject = new PrivateObject(displayPlugin);
            privatedispalypluginObject.SetField("_pipPbpService", PipPbpServiceObject);

            if (PipPbpService != null)
            {
                var VideoSwapResult = displayPlugin.VideoSwap(monitorInfo1, x, y).Result;
                Assert.That(VideoSwap, Is.EqualTo(VideoSwapResult));
                PipPbpService.Verify(s => s.VideoSwap(monitorInfo1, x, y), Times.Once);
            }
            else
            {
                var VideoSwapResult = displayPlugin.VideoSwap(monitorInfo1, x, y).Result;
                Assert.IsFalse(VideoSwapResult);
            }
        }

        [Test]
        public void TestGetPxpMode()
        {
            ObjGetVCP GetPxpMode = new ObjGetVCP() { result = true, value = 0x22 };
            var GetPxpModeVCPValue = GetPxpMode.value;
            ObjGetVCP GetPxpMode2 = new ObjGetVCP() { result = false, value = 0xff };
            PipPbpService.Setup(x => x.GetPxpMode(It.IsAny<MonitorInfo>())).Returns(Task.FromResult(GetPxpMode));
            var PipPbpServiceObject = PipPbpService.Object;
            PrivateObject privatedispalypluginObject = new PrivateObject(displayPlugin);
            privatedispalypluginObject.SetField("_pipPbpService", PipPbpServiceObject);

            if (PipPbpService != null)
            {
                var GetPxpModeResult = displayPlugin.GetPxpMode(monitorInfo1).Result;
                Assert.That(GetPxpMode, Is.EqualTo(GetPxpModeResult));
                Assert.That(GetPxpModeVCPValue, Is.EqualTo(GetPxpModeResult.value));
                PipPbpService.Verify(s => s.GetPxpMode(monitorInfo1), Times.Once);
            }
            else
            {
                var GetPxpModeResult = displayPlugin.GetPxpMode(monitorInfo1).Result;
                Assert.That(GetPxpMode2, Is.EqualTo(GetPxpModeResult));
            }
        }

        /*[Test]
        public void TestGetSubInputList()
        {
            List<UInt16> SubInputList = new List<UInt16>() { 0x11, 0x1B, 0x19, 0x0F };
            List<UInt16> SubInputList2 = new List<UInt16>();
            PipPbpService.Setup(x => x.GetSubInputList(It.IsAny<MonitorInfo>())).Returns(Task.FromResult(SubInputList));
            var PipPbpServiceObject = PipPbpService.Object;
            PrivateObject privatedispalypluginObject = new PrivateObject(displayPlugin);
            privatedispalypluginObject.SetField("_pipPbpService", PipPbpServiceObject);

            if (PipPbpService != null)
            {
                var GetSubInputListResult = displayPlugin.GetSubInputList(monitorInfo1).Result;   //capabilityDic 里面不包含E8
                Assert.IsNotNull(GetSubInputListResult);
            }
            else
            {
                var GetSubInputListResult = displayPlugin.GetSubInputList(monitorInfo1).Result;
                Assert.That(SubInputList2, Is.EqualTo(GetSubInputListResult));
            }

            Dictionary<string, List<string>> capabilityDic = new Dictionary<string, List<string>>();  //monitorInfo1.CapabilityDic = capabilityDic;  里面包含E8
            capabilityDic.Add("E8", new List<string> { "value3" });
            monitorInfo1.CapabilityDic = capabilityDic;
            var GetSubInputListResult3 = displayPlugin.GetSubInputList(monitorInfo1).Result;
            Assert.That(SubInputList, Is.EqualTo(GetSubInputListResult3));
            PipPbpService.Verify(s => s.GetSubInputList(monitorInfo1), Times.Once);
        }*/

        [Test]
        public void TestGetSubInputs()
        {
            List<InputSourceObj> getSubInputs = new List<InputSourceObj>();
            getSubInputs.Add(new InputSourceObj(0x11, "HDMI-1"));
            getSubInputs.Add(new InputSourceObj(0x1B, "USB-C1"));
            getSubInputs.Add(new InputSourceObj(0x19, "Thunderbolt-1"));
            getSubInputs.Add(new InputSourceObj(0x0F, "DisplayPort-1"));
            List<InputSourceObj> getSubInputs2 = new List<InputSourceObj>();
            PipPbpService.Setup(x => x.GetSubInputs(It.IsAny<MonitorInfo>())).Returns(Task.FromResult(getSubInputs));
            var PipPbpServiceObject = PipPbpService.Object;
            PrivateObject privatedispalypluginObject = new PrivateObject(displayPlugin);
            privatedispalypluginObject.SetField("_pipPbpService", PipPbpServiceObject);

            if (PipPbpService != null)
            {
                var GetSubInputListResult = displayPlugin.GetSubInputs(monitorInfo1).Result;
                Assert.That(getSubInputs, Is.EqualTo(GetSubInputListResult));
                PipPbpService.Verify(s => s.GetSubInputs(monitorInfo1), Times.Once);
            }
            else
            {
                var GetSubInputListResult = displayPlugin.GetSubInputs(monitorInfo1).Result;
                Assert.That(getSubInputs2, Is.EqualTo(GetSubInputListResult));
            }
        }

        [Test]
        public void TestSetSubInputs()
        {
            bool setSubInputs = true;
            InputSourceObj? sub1 = new InputSourceObj(0x11, "HDMI-1");
            InputSourceObj? sub2 = new InputSourceObj(0x1B, "USB-C1");
            InputSourceObj? sub3 = new InputSourceObj(0x0F, "DisplayPort-1");
            PipPbpService.Setup(x => x.SetSubInputs(It.IsAny<MonitorInfo>(), It.IsAny<InputSourceObj?>(), It.IsAny<InputSourceObj?>(), It.IsAny<InputSourceObj?>())).Returns(Task.FromResult(setSubInputs));
            var PipPbpServiceObject = PipPbpService.Object;
            PrivateObject privatedispalypluginObject = new PrivateObject(displayPlugin);
            privatedispalypluginObject.SetField("_pipPbpService", PipPbpServiceObject);

            if (PipPbpService != null)
            {
                var SetPbpModeResult = displayPlugin.SetSubInputs(monitorInfo1, sub1, sub2, sub3).Result;
                Assert.That(setSubInputs, Is.EqualTo(SetPbpModeResult));
                PipPbpService.Verify(s => s.SetSubInputs(monitorInfo1, sub1, sub2, sub3), Times.Once);
            }
            else
            {
                var SetPbpModeResult = displayPlugin.SetSubInputs(monitorInfo1, sub1, sub2, sub3).Result;
                Assert.IsFalse(SetPbpModeResult);
            }
        }

        /*[Test]
        public void TestGetUSBKVMPCsList()
        {
            var inputlist1 = new Dictionary<string, InputInfo>
                    {
                { "Thunderbolt-1", new InputInfo { InputName = "Thunderbolt-1", USBUpstream = "Thunderbolt-1" } },
                { "DisplayPort-1", new InputInfo { InputName = "DisplayPort-1", USBUpstream = "USB-B1" } },
                { "HDMI-1", new InputInfo { InputName = "HDMI-1", USBUpstream = "USB-C" } }
                    };
            var InputName1 = inputlist1["HDMI-1"].InputName;
            var USBUpstream1 = inputlist1["HDMI-1"].USBUpstream;
            var InputName2 = inputlist1["DisplayPort-1"].InputName;
            var USBUpstream2 = inputlist1["DisplayPort-1"].USBUpstream;
            var subInputList = new List<InputSourceObj>
            {
            new InputSourceObj { Name = "DisplayPort-1" }
            };
            string InputType1 = "HDMI-1";
            string InputType2 = "DisplayPort-1";
            var result = displayPlugin.GetUSBKVMPCsList(monitorInfo1, inputlist1, subInputList).Result;
            Assert.IsNotNull(result);
            Assert.Greater(result.Count, 0);
            Assert.IsTrue(result.ContainsKey("PC1"));
            Assert.IsTrue(result.ContainsKey("PC2"));
            Assert.That(InputName1, Is.EqualTo(result["PC1"].InputName));
            Assert.That(InputType1, Is.EqualTo(result["PC1"].InputType));
            Assert.That(InputName2, Is.EqualTo(result["PC2"].InputName));
            Assert.That(InputType2, Is.EqualTo(result["PC2"].InputType));
        }*/

        //Robert_Lin, 2025-1-7 EAPlugin.IsFunctionEnabled has been deleted.
        [Test]
        public void TestSetEAFunctionEnabled()
        {
            //bool isEnabled = true;
            //bool isDisabled = false;
            //if (EasyArrangeplugin != null)
            //{
            //    var SetEAFunctionEnabledResult = displayPlugin.SetEAFunctionEnabled(isEnabled).Result;
            //    Assert.That(isEnabled, Is.EqualTo(SetEAFunctionEnabledResult));
            //}
            //else
            //{
            //    var SetEAFunctionEnabledResult = displayPlugin.SetEAFunctionEnabled(isEnabled).Result;
            //    Assert.That(isDisabled, Is.EqualTo(SetEAFunctionEnabledResult));
            //}
        }

        //Robert_Lin, 2025-1-7 EAPlugin.IsFunctionEnabled has been deleted.
        [Test]
        public void TestGetEAFunctionEnabled()
        {
            //bool result = false;
            //bool value = false;
            //if (EasyArrangeplugin != null)
            //{
            //    var GetEAFunctionEnableddResult = displayPlugin.GetEAFunctionEnabled().Result;
            //    Assert.That(result, Is.EqualTo(GetEAFunctionEnableddResult.result));
            //    Assert.That(EasyArrangeplugin.IsFunctionEnabled, Is.EqualTo(GetEAFunctionEnableddResult.value));
            //}
            //else
            //{
            //    var GetEAFunctionEnableddResult = displayPlugin.GetEAFunctionEnabled().Result;
            //    Assert.That(result, Is.EqualTo(GetEAFunctionEnableddResult.result));
            //    Assert.That(value, Is.EqualTo(GetEAFunctionEnableddResult.value));
            //}
        }

        [Test]
        public void TestSetEAWrokSplit()
        {
            bool result = true;
            bool result2 = false;

            int cellCount = 1;
            char splitKey = 'A';
            List<double>? settings = new List<double>();
            EasyArrangeService.Setup(x => x.SetEAWrokSplit(It.IsAny<MonitorInfo>(), It.IsAny<int>(), It.IsAny<char>(), It.IsAny<List<double>?>())).Returns(Task.FromResult(result));
            var EasyArrangeServiceObject_eaService = EasyArrangeService.Object;
            PrivateObject privatedispalypluginObject = new PrivateObject(displayPlugin);
            privatedispalypluginObject.SetField("_eaService", EasyArrangeServiceObject_eaService);

            if (EasyArrangeServiceObject_eaService != null)
            {
                var SetEAWrokSplitResult = displayPlugin.SetEAWrokSplit(monitorInfo1, cellCount, splitKey, settings).Result;
                Assert.That(result, Is.EqualTo(SetEAWrokSplitResult));
            }
            else
            {
                var SetEAWrokSplitResult = displayPlugin.SetEAWrokSplit(monitorInfo1, cellCount, splitKey, settings).Result;
                Assert.That(result2, Is.EqualTo(SetEAWrokSplitResult));
            }
        }

        //Robert_Lin 2024-9-24 this interface has been removed
        /*
        [Test]
        public void TestRequestEditSplit()
        {
            bool result = true;
            bool result2 = false;

            int cellCount = 2;
            char splitKey = 'B';
            List<double>? settings = new List<double>();
            string customName = "Custom";
            EasyArrangeService.Setup(x => x.RequestEditSplit(It.IsAny<MonitorInfo>(), It.IsAny<int>(), It.IsAny<char>(), It.IsAny<string>(), It.IsAny<List<double>?>())).Returns(Task.FromResult(result));
            var EasyArrangeServiceObject_eaService = EasyArrangeService.Object;
            PrivateObject privatedispalypluginObject = new PrivateObject(displayPlugin);
            privatedispalypluginObject.SetField("_eaService", EasyArrangeServiceObject_eaService);

            if (EasyArrangeServiceObject_eaService != null)
            {
                var SetEAWrokSplitResult = displayPlugin.RequestEditSplit(monitorInfo1, cellCount, splitKey, customName, settings).Result;
                Assert.That(result, Is.EqualTo(SetEAWrokSplitResult));
            }
            else
            {
                var SetEAWrokSplitResult = displayPlugin.RequestEditSplit(monitorInfo1, cellCount, splitKey, customName, settings).Result;
                Assert.That(result2, Is.EqualTo(SetEAWrokSplitResult));
            }
        }
        */

        [Test]
        public void TestGetALSFeatureValue()
        {
            var type = ALSFeatureQueryType.AutoBrightnessRangeLevel;  //AutoBrightness = 3
            var val = 3;
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
                AutoBrightnessRangeLevel = new AutoBrightnessRangeLevel() { level_name = "Low", level_value = 0 }
            };
            _AllALSConfig = new List<ALSConfig>();
            _AllALSConfig.Add(aconfig);
            DisplayMangerPlugin.AllALSConfig = _AllALSConfig;
            var allALSConfig_ = DisplayMangerPlugin.AllALSConfig;

            if (type != ALSFeatureQueryType.MMS)
            {
                if (allALSConfig_ != null)
                {
                    ALSConfig GetALSFeatureValue_result = displayPlugin.GetALSFeatureValue(monitorInfo1, type, val).Result;
                    Assert.IsNotNull(GetALSFeatureValue_result);
                    Assert.That(aconfig.DisplayName, Is.EqualTo(GetALSFeatureValue_result.DisplayName));
                    Assert.That(aconfig.serialNumber, Is.EqualTo(GetALSFeatureValue_result.serialNumber));
                }
                else
                {
                    var allALSConfig2 = DisplayMangerPlugin.AllALSConfig;
                    allALSConfig2 = null;
                    ALSConfig GetALSFeatureValue_result = displayPlugin.GetALSFeatureValue(monitorInfo1, type, val).Result;
                    Assert.IsNotNull(GetALSFeatureValue_result);
                    Assert.That(aconfig.DisplayName, Is.EqualTo(monitorInfo1.edid.SerialNumber));
                    Assert.That(aconfig.serialNumber, Is.EqualTo(monitorInfo1.DisplayName));
                }
            }
            else
            {
                ALSConfig GetALSFeatureValue_result = displayPlugin.GetALSFeatureValue(monitorInfo, type, val).Result;
                Assert.IsNotNull(GetALSFeatureValue_result);
            }
        }

        [Test]
        public void TestCheckisPrimaryMonitorSyncOnOff()
        {
            ALSConfig monitorALS = new ALSConfig()
            {
                DisplayName = "DISPLAY7",
                serialNumber = "808597589",
                isSupportALS = 2,
                isMMSEnable = false,
                isPrimaryMonitorSync = false,
                isAutoBrightness = false,
                isAutoColorTemp = false,
                LiftTone = 0,
                AllValue = 32,
                result = false,
                AutoBrightnessRangeLevel = new AutoBrightnessRangeLevel() { level_name = "Low", level_value = 0 }
            };
            //List<ALSConfig> allALSConfig = new List<ALSConfig>();
            //allALSConfig.Add(monitorALS);
            //DisplayMangerPlugin.AllALSConfig = allALSConfig;
            //var allALSConfig_ = DisplayMangerPlugin.AllALSConfig;
            //var GetConnectedALSConfig_ = allALSConfig_[0];

            _AllALSConfig = new List<ALSConfig>();
            _AllALSConfig.Add(aconfig);
            DisplayMangerPlugin.AllALSConfig = _AllALSConfig;
            var allALSConfig_ = DisplayMangerPlugin.AllALSConfig;
            var GetConnectedALSConfig_ = allALSConfig_[0];

            List<MonitorInfo> _allInfoMonitors = new List<MonitorInfo>();
            _allInfoMonitors.Add(monitorInfo1);
            VcpCoreService.Setup(x => x.GetMonitors()).Returns(Task.FromResult(_allInfoMonitors));  //mock get monitor
            var VcpCoreServiceObject = VcpCoreService.Object;
            PrivateObject privatedispalypluginObject = new PrivateObject(displayPlugin);
            privatedispalypluginObject.SetField("_VcpCorePlugin", VcpCoreServiceObject);
            uint GetBitsValue = 1;
            if (GetBitsValue == 1)
            {
                var CheckisPrimaryMonitorSyncOnOffResult = displayPlugin.CheckisPrimaryMonitorSyncOnOff(monitorInfo1, monitorALS, "0").Result;
                Assert.IsTrue(CheckisPrimaryMonitorSyncOnOffResult);
                Assert.That(allALSConfig_[0].serialNumber, Is.EqualTo(monitorInfo1.edid.SerialNumber));
                Assert.That(allALSConfig_[0].DisplayName, Is.EqualTo(monitorInfo1.DisplayName));
            }
            else
            {
                var CheckisPrimaryMonitorSyncOnOffResult = displayPlugin.CheckisPrimaryMonitorSyncOnOff(monitorInfo1, aconfig, "0").Result;
                Assert.IsTrue(CheckisPrimaryMonitorSyncOnOffResult);
            }
        }

        [Test]
        public void TestGetConnectedALSConfig()
        {
            List<MonitorInfo> _allInfoMonitors = new List<MonitorInfo>();
            _allInfoMonitors.Add(monitorInfo1);
            VcpCoreService.Setup(x => x.GetMonitors()).Returns(Task.FromResult(_allInfoMonitors));
            var VcpCoreServiceObject = VcpCoreService.Object;
            PrivateObject privatedispalypluginObject = new PrivateObject(displayPlugin);
            privatedispalypluginObject.SetField("_VcpCorePlugin", VcpCoreServiceObject);

            var GetConnectedALSConfig_ = displayPlugin.GetConnectedALSConfig().Result;
            var GetConnectedALSConfig_2 = GetConnectedALSConfig_[0];
            Assert.Greater(GetConnectedALSConfig_.Count, 0);
            Assert.That(GetConnectedALSConfig_2.serialNumber, Is.EqualTo(monitorInfo1.edid.SerialNumber));
            Assert.That(GetConnectedALSConfig_2.DisplayName, Is.EqualTo(monitorInfo1.DisplayName));
        }

        [Test]
        public void TestGetAllExistAlsConfig()
        {
            //List<ALSConfig> allALSConfig = new List<ALSConfig>();
            //allALSConfig.Add(aconfig);
            //DisplayMangerPlugin.AllALSConfig = allALSConfig;
            //var allALSConfig_ = DisplayMangerPlugin.AllALSConfig;

            _AllALSConfig = new List<ALSConfig>();
            _AllALSConfig.Add(aconfig);
            DisplayMangerPlugin.AllALSConfig = _AllALSConfig;
            var allALSConfig_ = DisplayMangerPlugin.AllALSConfig;

            var result = displayPlugin.GetAllExistAlsConfig().Result;
            Assert.IsNotNull(result);
            Assert.Greater(DisplayMangerPlugin.AllALSConfig.Count, 0);
        }

        //PrivateObject privateObject = new PrivateObject(displayPlugin);
        //privateObject.Invoke("InitializeAllALSInfo", new object[] { });

        [Test]
        public void TestSynchronizeALSFeatureValue()
        {
            ALSConfig monitorALS = new ALSConfig()
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
                AutoBrightnessRangeLevel = new AutoBrightnessRangeLevel() { level_name = "Low", level_value = 0 }
            };
            //List<ALSConfig> allALSConfig = new List<ALSConfig>();
            //allALSConfig.Add(monitorALS);
            //DisplayMangerPlugin.AllALSConfig = allALSConfig;
            //var allALSConfig_ = DisplayMangerPlugin.AllALSConfig;
            //var GetConnectedALSConfig_ = allALSConfig_[0];

            _AllALSConfig = new List<ALSConfig>();
            _AllALSConfig.Add(aconfig);
            DisplayMangerPlugin.AllALSConfig = _AllALSConfig;
            var allALSConfig_ = DisplayMangerPlugin.AllALSConfig;
            var GetConnectedALSConfig_ = allALSConfig_[0];

            var result = displayPlugin.SynchronizeALSFeatureValue(monitorALS).Result;
            for (int i = 0; i < allALSConfig_.Count; i++)
            {
                Assert.That(GetConnectedALSConfig_.AllValue, Is.EqualTo(monitorALS.AllValue));
                Assert.That(GetConnectedALSConfig_.isAutoBrightness, Is.EqualTo(monitorALS.isAutoBrightness));
                Assert.That(GetConnectedALSConfig_.isAutoColorTemp, Is.EqualTo(monitorALS.isAutoColorTemp));
                Assert.That(GetConnectedALSConfig_.AutoBrightnessRangeLevel, Is.EqualTo(monitorALS.AutoBrightnessRangeLevel));
            }
            if (GetConnectedALSConfig_.DisplayName.Equals(monitorALS.DisplayName) && GetConnectedALSConfig_.serialNumber.Equals(monitorALS.serialNumber))
            {
                Assert.IsTrue(GetConnectedALSConfig_.isPrimaryMonitorSync);
            }
            else
            {
                Assert.IsFalse(GetConnectedALSConfig_.isPrimaryMonitorSync);
            }
            Assert.IsTrue(result);
        }

        [Test]
        public void TestSetALSFeatureValue()
        {
            var type = ALSFeatureQueryType.MMS;
            string value = "On";
            ALSConfig monitorALS = new ALSConfig()
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
                AutoBrightnessRangeLevel = new AutoBrightnessRangeLevel() { level_name = "Low", level_value = 0 }
            };
            //List<ALSConfig> allALSConfig = new List<ALSConfig>();
            //allALSConfig.Add(monitorALS);
            //DisplayMangerPlugin.AllALSConfig = allALSConfig;
            //var allALSConfig_ = DisplayMangerPlugin.AllALSConfig;
            //var GetConnectedALSConfig_ = allALSConfig_[0];

            _AllALSConfig = new List<ALSConfig>();
            _AllALSConfig.Add(aconfig);
            DisplayMangerPlugin.AllALSConfig = _AllALSConfig;
            var allALSConfig_ = DisplayMangerPlugin.AllALSConfig;
            var GetConnectedALSConfig_ = allALSConfig_[0];

            var param = monitorALS;
            bool isMMSEnable = true;

            ObjGetVCP ObjGetvcp = new ObjGetVCP() { result = true, value = 1 }; //0xEF,value设置为0，对应isMMSEnable就是false, 1对应isMMSEnable就是true
            var ObjGetvcpValue = ObjGetvcp.value;
            VcpCoreService.Setup(x => x.GetVCPCapability(It.IsAny<MonitorInfo>(), It.IsAny<byte>(), It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<Priority>())).Returns(Task.FromResult(ObjGetvcp));
            var VcpCoreServiceObject = VcpCoreService.Object;
            PrivateObject privatedispalypluginObject = new PrivateObject(displayPlugin);
            privatedispalypluginObject.SetField("_VcpCorePlugin", VcpCoreServiceObject);

            bool setVCPCapability = true;
            VcpCoreService.Setup(x => x.SetVCPCapability(It.IsAny<MonitorInfo>(), It.IsAny<byte>(), It.IsAny<uint>(), It.IsAny<Guid>(), It.IsAny<Priority>())).Returns(Task.FromResult(setVCPCapability));
            var VcpCoreServiceObject1 = VcpCoreService.Object;
            privatedispalypluginObject.SetField("_VcpCorePlugin", VcpCoreServiceObject1);

            Dictionary<string, List<string>> capabilityDic = new Dictionary<string, List<string>>();  //monitorInfo1.CapabilityDic = capabilityDic;  里面包含EF
            capabilityDic.Add("66", new List<string> { "value1" });
            capabilityDic.Add("EF", new List<string> { "value2" });
            monitorInfo1.CapabilityDic = capabilityDic;

            if (type == ALSFeatureQueryType.MMS)
            {
                var SetALSFeatureValue_result1 = displayPlugin.SetALSFeatureValue(monitorInfo1, ref param, type, value).Result;
                Assert.True(SetALSFeatureValue_result1);
                Assert.That(isMMSEnable, Is.EqualTo(param.isMMSEnable));
            }
            else if (type == ALSFeatureQueryType.PrimaryMonitorSync)
            {
                var SetALSFeatureValue_result2 = displayPlugin.SetALSFeatureValue(monitorInfo1, ref param, type, value).Result;
                Assert.True(SetALSFeatureValue_result2);
            }
            else if (type == ALSFeatureQueryType.AutoColorTemperature)
            {
                var SetALSFeatureValue_result3 = displayPlugin.SetALSFeatureValue(monitorInfo1, ref param, type, value).Result;
                Assert.True(SetALSFeatureValue_result3);
            }
            else if (type == ALSFeatureQueryType.AutoBrightness)
            {
                var SetALSFeatureValue_result4 = displayPlugin.SetALSFeatureValue(monitorInfo1, ref param, type, value).Result;
                Assert.True(SetALSFeatureValue_result4);
            }
            else if (type == ALSFeatureQueryType.AutoBrightnessRangeLevel)
            {
                var SetALSFeatureValue_result5 = displayPlugin.SetALSFeatureValue(monitorInfo1, ref param, type, value).Result;
                Assert.True(SetALSFeatureValue_result5);
            }
            else if (type == ALSFeatureQueryType.All)
            {
                var SetALSFeatureValue_result6 = displayPlugin.SetALSFeatureValue(monitorInfo1, ref param, type, value).Result;
                Assert.True(SetALSFeatureValue_result6);
            }
            else
            {
                var SetALSFeatureValue_result7 = displayPlugin.SetALSFeatureValue(monitorInfo1, ref param, type, value).Result;
                Assert.False(SetALSFeatureValue_result7);
            }
        }

        [Test]
        public void TestUpdateALSFeatureValue()
        {
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
                AutoBrightnessRangeLevel = new AutoBrightnessRangeLevel() { level_name = "Low", level_value = 0 }
            };
            //List<ALSConfig> allALSConfig = new List<ALSConfig>();
            //allALSConfig.Add(aconfig);

            _AllALSConfig = new List<ALSConfig>();
            _AllALSConfig.Add(aconfig);

            ObjGetVCP ObjGetvcp = new ObjGetVCP() { result = true, value = 305u }; //0x66
            var ObjGetvcpValue = ObjGetvcp.value;
            VcpCoreService.Setup(x => x.GetVCPCapability(It.IsAny<MonitorInfo>(), It.IsAny<byte>(), It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<Priority>())).Returns(Task.FromResult(ObjGetvcp));
            var VcpCoreServiceObject = VcpCoreService.Object;
            PrivateObject privatedispalypluginObject = new PrivateObject(displayPlugin);
            privatedispalypluginObject.SetField("_VcpCorePlugin", VcpCoreServiceObject);

            string getVcpCapabilities = "{'CapsDataMap' : {'Ambient Light Sensor': ['ALS full function']}}";
            VcpCoreService.Setup(x => x.GetVCPCapabilities(It.IsAny<MonitorInfo>(), It.IsAny<Guid>(), It.IsAny<Priority>())).Returns(Task.FromResult(getVcpCapabilities)); //GetALSupport
            var VcpCoreServiceObject1 = VcpCoreService.Object;
            privatedispalypluginObject.SetField("_VcpCorePlugin", VcpCoreServiceObject1);

            ALSConfig aconfigNulll = new ALSConfig();
            aconfigNulll = null;

            if (aconfigNulll == null)
            {
                var AllALSConfig_ = DisplayMangerPlugin.AllALSConfig;
                var result = displayPlugin.UpdateALSFeatureValue(monitorInfo1).Result;
                Assert.IsTrue(result);
                Assert.That(AllALSConfig_[0].serialNumber, Is.EqualTo(monitorInfo1.edid.SerialNumber));
                Assert.That(AllALSConfig_[0].DisplayName, Is.EqualTo(monitorInfo1.DisplayName));
            }
            else
            {
                DisplayMangerPlugin.AllALSConfig = _AllALSConfig;
                var allALSConfig2 = DisplayMangerPlugin.AllALSConfig;
                var result = displayPlugin.UpdateALSFeatureValue(monitorInfo1).Result;
                Assert.IsTrue(result);
                Assert.Greater(allALSConfig2.Count, 0);
            }
        }

        [Test]
        public void TesGetDisplayPropertiesInfo()
        {
            string capabilitystring = "E2(25, 23, 24, 26, 27, 3A, 3B, 3C ) EA(F8, F800, F801)";
            monitorInfo1.CapabilityString = capabilitystring;
            bool supportedHDR = true;
            bool supportedUSBC = true;

            ObjGetVCP ObjGetvcp = new ObjGetVCP() { result = true, value = 0x27u }; //supportedHDR=0x27u
            var ObjGetvcpValue = ObjGetvcp.value;
            VcpCoreService.Setup(x => x.GetVCPCapability(It.IsAny<MonitorInfo>(), It.IsAny<byte>(), It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<Priority>())).Returns(Task.FromResult(ObjGetvcp));
            var VcpCoreServiceObject = VcpCoreService.Object;
            PrivateObject privatedispalypluginObject = new PrivateObject(displayPlugin);
            privatedispalypluginObject.SetField("_VcpCorePlugin", VcpCoreServiceObject);

            string setParam = "USB-C Prioritization";
            ObjGetVCP ObjGetvcp2 = new ObjGetVCP() { result = true, value = "High Data Speed" };//supportedUSBC="High Data Speed"
            var ObjGetvcpValue2 = ObjGetvcp.value;
            VcpCoreService.Setup(x => x.GetVCPCapability(It.IsAny<MonitorInfo>(), It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<Priority>())).Returns(Task.FromResult(ObjGetvcp2));
            var VcpCoreServiceObject2 = VcpCoreService.Object;
            privatedispalypluginObject.SetField("_VcpCorePlugin", VcpCoreServiceObject2);

            DisplayPropertiesInfo displayPropertiesInfo = new DisplayPropertiesInfo()
            {
                DisplayName = "DISPLAY7",
                SupportedHDR = true,
                isHDREnable = true,
                SupportedUSBCPrioritization = true,
                USBCPrioritizationType = USBCPrioritizationType.HighDataSpeed,
                CurrentOrientation = DisplayOrientation.Angle0,

                SupportedProperties = new DisplaySupportedProperties()
                {
                    Orientations = new DisplayOrientation[]
                    {
                     DisplayOrientation.Angle0,
                     DisplayOrientation.Angle90,
                     DisplayOrientation.Angle180,
                     DisplayOrientation.Angle270
                    },
                    Properties = new List<Properties>()
                    {
                     new Properties()
                     {
                          Resolutions_Width = 2560,
                          Resolutions_High = 1440,
                          Frequency = 60,
                          BitsPerPixel = 8,
                          isRecommended = true,
                          isCurrent = true,
                     }
                    }
                }
            };
            DisplayPropertiesService.Setup(x => x.GetDisplayPropertiesInfo(It.IsAny<MonitorInfo>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<USBCPrioritizationType>())).Returns(Task.FromResult(displayPropertiesInfo));
            var displayPropertiesServiceObject = DisplayPropertiesService.Object;
            privatedispalypluginObject.SetField("_DisplayPropertiesPlugin", displayPropertiesServiceObject);

            var result = displayPlugin.GetDisplayPropertiesInfo(monitorInfo1).Result;
            if (supportedHDR)
            {
                Assert.IsTrue(result.isHDREnable);
            }
            if (supportedUSBC)
            {
                Assert.IsTrue(result.SupportedUSBCPrioritization);
            }
            Assert.Greater(result.SupportedProperties.Properties.Count, 0);
        }

        [Test]
        public void TestSetDisplayOrientation()
        {
            string capabilitystring0 = "E2(25, 23, 24, 26, 27, 3A, 3B, 3C ) EA(F8, F800, F801)"; //capabilitystring no Contains("AA")
            monitorInfo1.CapabilityString = capabilitystring0;
            string Orientation = "Portrait";
            List<MonitorInfo> monitorInfos01 = new List<MonitorInfo>();
            monitorInfos01.Add(monitorInfo1);

            var SetOSDOrientation_result0 = displayPlugin.SetDisplayOrientation(monitorInfos01).Result;
            Assert.IsNotNull(SetOSDOrientation_result0);
            Assert.IsFalse(SetOSDOrientation_result0[0]);

            string capabilitystring = "E2(25, 23, 24, 26, 27, 3A, 3B, 3C ) EA(F8, F800, F801) AA(00)"; //capabilitystring need Contains("AA")
            monitorInfo1.CapabilityString = capabilitystring;

            List<MonitorInfo> monitorInfos = new List<MonitorInfo>();
            monitorInfos.Add(monitorInfo1);

            var isLock = false;
            PrivateObject privateObject = new PrivateObject(displayPlugin);
            displayPlugin.SetEnableLockOrientation(isLock);
            var isLockOrientation = (bool)privateObject.GetField("isLockOrientation");
            Assert.That(isLock, Is.EqualTo(isLockOrientation));
            bool IsLockOrientation = false;

            ObjGetVCP ObjGetvcp = new ObjGetVCP() { result = true, value = 2u }; //0xAA  "Screen Orientation" 1u=Angle0,2u=Angle90
            var ObjGetvcpValue = 2u;
            var ObjGetvcpResult = true;
            VcpCoreService.Setup(x => x.GetVCPCapability(It.IsAny<MonitorInfo>(), It.IsAny<byte>(), It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<Priority>())).Returns(Task.FromResult(ObjGetvcp));
            var VcpCoreServiceObject = VcpCoreService.Object;
            PrivateObject privatedispalypluginObject = new PrivateObject(displayPlugin);
            privatedispalypluginObject.SetField("_VcpCorePlugin", VcpCoreServiceObject);

            DisplayOrientation currentOrientation = DisplayOrientation.Angle0;
            DisplayPropertiesService.Setup(x => x.GetCurrentDisplayOrientation(It.IsAny<string>())).Returns(Task.FromResult(currentOrientation));
            var displayPropertiesServiceObject = DisplayPropertiesService.Object;
            privatedispalypluginObject.SetField("_DisplayPropertiesPlugin", displayPropertiesServiceObject);

            bool ActualResult = true;
            DisplayPropertiesService.Setup(x => x.SetDisplayPropertiest(It.IsAny<string>(), It.IsAny<Properties>(), It.IsAny<DisplayOrientation>())).Returns(Task.FromResult(ActualResult));
            var displayPropertiesServiceObject1 = DisplayPropertiesService.Object;
            privatedispalypluginObject.SetField("_DisplayPropertiesPlugin", displayPropertiesServiceObject1);

            var result1 = displayPlugin.SetDisplayOrientation(monitorInfos).Result;
            bool[] bools = new bool[monitorInfos.Count];
            if (!IsLockOrientation)
            {
                if (ObjGetvcpResult == true)
                {
                    uint retValue;

                    if (uint.TryParse(ObjGetvcpValue.ToString(), out retValue))
                    {
                        if (displayPropertiesPlugin != null)
                        {
                            DisplayOrientation currentOrientation1 = DisplayOrientation.Angle0;
                            DisplayOrientation orientation = (DisplayOrientation)(retValue - 1);
                            if (!currentOrientation1.Equals(orientation))
                            {
                                Properties properties = new Properties();
                                bools[0] = true;
                            }
                        }
                    }
                }
            }
            Assert.IsTrue(bools[0]);
            Assert.IsTrue(result1[0]);
        }

        [Test]
        public void TestGetOSDOrientation()
        {
            string capabilitystring = "E2(25, 23, 24, 26, 27, 3A, 3B, 3C ) EA(F8, F800, F801) AA(00)"; //capabilitystring need Contains("AA")
            monitorInfo1.CapabilityString = capabilitystring;

            List<MonitorInfo> monitorInfos = new List<MonitorInfo>();
            monitorInfos.Add(monitorInfo1);

            var isLock = false;
            PrivateObject privateObject = new PrivateObject(displayPlugin);
            displayPlugin.SetEnableLockOrientation(isLock);
            var isLockOrientation = (bool)privateObject.GetField("isLockOrientation");
            Assert.That(isLock, Is.EqualTo(isLockOrientation));
            bool IsLockOrientation = false;

            ObjGetVCP ObjGetvcp = new ObjGetVCP() { result = true, value = 2u }; //0xAA  "Screen Orientation" 1u=Angle0,2u=Angle90
            var ObjGetvcpValue = 2u;
            var ObjGetvcpResult = true;
            VcpCoreService.Setup(x => x.GetVCPCapability(It.IsAny<MonitorInfo>(), It.IsAny<byte>(), It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<Priority>())).Returns(Task.FromResult(ObjGetvcp));
            var VcpCoreServiceObject = VcpCoreService.Object;
            PrivateObject privatedispalypluginObject = new PrivateObject(displayPlugin);
            privatedispalypluginObject.SetField("_VcpCorePlugin", VcpCoreServiceObject);

            string[] OrientationString = new string[] { "", "Landscape", "Portrait", "Landscape_flipped", "Portrait_flipped" };
            string OrientationString_result;
            var OrientationString_result1 = displayPlugin.GetOSDOrientation(monitorInfo1).Result;
            if (!IsLockOrientation)
            {
                if (ObjGetvcpResult == true)
                {
                    uint retValue;

                    if (uint.TryParse(ObjGetvcpValue.ToString(), out retValue))
                    {
                        if (displayPropertiesPlugin != null)
                        {
                            OrientationString_result = OrientationString[retValue];
                            OrientationString[2] = "Portrait";
                        }
                    }
                }
            }
            Assert.IsNotNull(OrientationString_result1);
            Assert.That(OrientationString_result1, Is.EqualTo(OrientationString[2]));
            Assert.That(OrientationString[2], Is.EqualTo("Portrait"));
        }

        [Test]
        public void TestSetOSDOrientation()
        {
            bool? ret = null;
            string capabilitystring0 = "E2(25, 23, 24, 26, 27, 3A, 3B, 3C ) EA(F8, F800, F801)"; //capabilitystring no Contains("AA")
            monitorInfo1.CapabilityString = capabilitystring0;
            string Orientation = "Portrait";
            var SetOSDOrientation_result0 = displayPlugin.SetOSDOrientation(monitorInfo1, Orientation).Result;
            Assert.IsNull(SetOSDOrientation_result0);
            Assert.That(ret, Is.EqualTo(SetOSDOrientation_result0));

            string capabilitystring = "E2(25, 23, 24, 26, 27, 3A, 3B, 3C ) EA(F8, F800, F801) AA(00)"; //capabilitystring need Contains("AA")
            monitorInfo1.CapabilityString = capabilitystring;

            List<MonitorInfo> monitorInfos = new List<MonitorInfo>();
            monitorInfos.Add(monitorInfo1);

            var isLock = false;
            PrivateObject privateObject = new PrivateObject(displayPlugin);
            displayPlugin.SetEnableLockOrientation(isLock);
            var isLockOrientation = (bool)privateObject.GetField("isLockOrientation");
            Assert.That(isLock, Is.EqualTo(isLockOrientation));
            bool IsLockOrientation = false;

            var ObjGetvcpResult = true;
            VcpCoreService.Setup(x => x.SetVCPCapability(It.IsAny<MonitorInfo>(), It.IsAny<byte>(), It.IsAny<uint>(), It.IsAny<Guid>(), It.IsAny<Priority>())).Returns(Task.FromResult(ObjGetvcpResult));
            var VcpCoreServiceObject = VcpCoreService.Object;
            PrivateObject privatedispalypluginObject = new PrivateObject(displayPlugin);
            privatedispalypluginObject.SetField("_VcpCorePlugin", VcpCoreServiceObject);

            string[] OrientationString = new string[] { "", "Landscape", "Portrait", "Landscape_flipped", "Portrait_flipped" };

            var SetOSDOrientation_result1 = displayPlugin.SetOSDOrientation(monitorInfo1, Orientation).Result;
            if (!IsLockOrientation)
            {
                if (ObjGetvcpResult == true)
                {
                    ret = false;
                    for (int i = 1; i < OrientationString.Length; i++)
                    {
                        if (Orientation.ToUpper().Equals(OrientationString[i].ToUpper()))
                        {
                            ret = true;
                        }
                    }
                }
            }
            Assert.IsNotNull(SetOSDOrientation_result1);
            Assert.IsTrue(SetOSDOrientation_result1);
            Assert.IsTrue(ret);
        }

        [Test]
        public void TestSetDisplayPropertiest()
        {
            bool ActualResult = true;
            DisplayPropertiesService.Setup(x => x.SetDisplayPropertiest(It.IsAny<string>(), It.IsAny<Properties>(), It.IsAny<DisplayOrientation>())).Returns(Task.FromResult(ActualResult));
            var displayPropertiesServiceObject = DisplayPropertiesService.Object;
            PrivateObject privatedispalypluginObject = new PrivateObject(displayPlugin);
            privatedispalypluginObject.SetField("_DisplayPropertiesPlugin", displayPropertiesServiceObject);

            string DisplayName = monitorInfo1.DisplayName;
            Properties properties = new Properties()
            {
                Resolutions_Width = 1920,
                Resolutions_High = 1080,
                Frequency = 60,
                BitsPerPixel = 8,
                isRecommended = false,
                isCurrent = false,
            };
            DisplayOrientation orientation = DisplayOrientation.Angle0;
            var SetDisplayPropertiestResult = displayPlugin.SetDisplayPropertiest(monitorInfo1, properties, orientation).Result;//Bruce 08-09 Modify the incoming value
            Assert.That(ActualResult, Is.EqualTo(SetDisplayPropertiestResult));
        }

        [Test]
        public void TestCallWindowsDisplaySetting()
        {
            bool ActualResult = true;
            DisplayPropertiesService.Setup(x => x.CallWindowsDisplaySetting()).Returns(Task.FromResult(ActualResult));
            var displayPropertiesServiceObject = DisplayPropertiesService.Object;
            PrivateObject privatedispalypluginObject = new PrivateObject(displayPlugin);
            privatedispalypluginObject.SetField("_DisplayPropertiesPlugin", displayPropertiesServiceObject);
            var result = displayPlugin.CallWindowsDisplaySetting().Result;
            Assert.IsTrue(result);
        }

        [Test]
        public void TestSetEnableLockOrientation()
        {
            var isLock = true;
            PrivateObject privateObject = new PrivateObject(displayPlugin);
            displayPlugin.SetEnableLockOrientation(isLock);
            var isLockOrientation = (bool)privateObject.GetField("isLockOrientation");
            Assert.That(isLock, Is.EqualTo(isLockOrientation));
        }

        [Test]
        public void TestReset0x52TimerTick()
        {
            int millisecond = 3000;
            Task Reset0x52Result = Task.CompletedTask;
            VcpCoreService.Setup(x => x.Reset0x52TimerTick(It.IsAny<int>())).Returns(Task.FromResult(Reset0x52Result));
            var VcpCoreServiceObject = VcpCoreService.Object;
            PrivateObject privatedispalypluginObject = new PrivateObject(displayPlugin);
            privatedispalypluginObject.SetField("_VcpCorePlugin", VcpCoreServiceObject);
            Task result = displayPlugin.Reset0x52TimerTick(millisecond);
            Assert.IsNotNull(result);
            Assert.IsTrue(result.IsCompleted);
        }

        [Test]
        public void TestSetHDRStatus()
        {
            bool onoff = true;
            string capabilitystring = "E2(00 02 04 0C 0D 0F 10 11 13 0B 27 14 ) ";
            monitorInfo1.CapabilityString = capabilitystring;

            bool setVCPCapability = true;
            VcpCoreService.Setup(x => x.SetVCPCapability(It.IsAny<MonitorInfo>(), It.IsAny<byte>(), It.IsAny<uint>(), It.IsAny<Guid>(), It.IsAny<Priority>())).Returns(Task.FromResult(setVCPCapability));
            var VcpCoreServiceObject = VcpCoreService.Object;
            PrivateObject privatedispalypluginObject = new PrivateObject(displayPlugin);
            privatedispalypluginObject.SetField("_VcpCorePlugin", VcpCoreServiceObject);

            bool ActualResult = true;
            DisplayPropertiesService.Setup(x => x.SetHDRStatus(It.IsAny<EDID>(), It.IsAny<bool>())).Returns(Task.FromResult(ActualResult));
            var displayPropertiesServiceObject = DisplayPropertiesService.Object;
            privatedispalypluginObject.SetField("_DisplayPropertiesPlugin", displayPropertiesServiceObject);

            if (onoff)
            {
                if (monitorInfo1.CapabilityString != "" && monitorInfo1.CapabilityString.Length > 10)
                {
                    var result = displayPlugin.SetHDRStatus(monitorInfo1, onoff).Result;
                    Assert.IsTrue(result);
                }
                else
                {
                    var result = displayPlugin.SetHDRStatus(monitorInfo1, onoff).Result;
                    Assert.IsTrue(result);
                }
            }
        }

        [Test]
        public void TestSetUSBCPrioritizationType()
        {
            USBCPrioritizationType type = USBCPrioritizationType.HighDataSpeed;
            bool setVCPCapability_ = true;
            VcpCoreService.Setup(x => x.SetVCPCapability(It.IsAny<MonitorInfo>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Priority>())).Returns(Task.FromResult(setVCPCapability_));
            var VcpCoreServiceObject = VcpCoreService.Object;
            PrivateObject privatedispalypluginObject = new PrivateObject(displayPlugin);
            privatedispalypluginObject.SetField("_VcpCorePlugin", VcpCoreServiceObject);
            //bool result = true;
            try
            {
                if (type != USBCPrioritizationType.Unknow)
                {
                    string setParam = "USB-C Prioritization";
                    string PrioritizationType = type == USBCPrioritizationType.HighDataSpeed ? "High Data Speed" : "High Resolution";
                    var result = displayPlugin.SetUSBCPrioritizationType(monitorInfo1, type).Result;
                    if (result != true)
                    {
                        Assert.IsFalse(result);
                    }
                    Assert.IsTrue(result);
                }
                else
                {
                    var result = displayPlugin.SetUSBCPrioritizationType(monitorInfo1, type).Result;
                    Assert.IsFalse(result);
                }
            }
            catch
            {
                var result = displayPlugin.SetUSBCPrioritizationType(monitorInfo1, type).Result;
                Assert.IsFalse(result);
            }
        }

        [Test]
        public void TestDisplayMangerPlugin()
        {
            DisplayMangerPlugin displayMangerPlugin = new DisplayMangerPlugin(DisplayMangerAgent.Object);
            Assert.IsNotNull(displayMangerPlugin);
            PrivateObject privateObject = new PrivateObject(displayMangerPlugin);
            var agent2 = privateObject.GetField("_agent") as IAgent;
            Assert.That(agent2, Is.EqualTo(DisplayMangerAgent.Object));
        }

        [Test]
        public void TestInitializeEAPlugin()
        {
            PrivateObject privateObject = new PrivateObject(displayPlugin);
            privateObject.Invoke("InitializeEAPlugin");
            Assert.IsNotNull(EasyArrangeplugin);
        }

        [Test]
        public void TestInitializeAllALSInfo()
        {
            List<MonitorInfo> _allInfoMonitors = new List<MonitorInfo>();
            _allInfoMonitors.Add(monitorInfo1);
            VcpCoreService.Setup(x => x.GetMonitors()).Returns(Task.FromResult(_allInfoMonitors));
            var VcpCoreServiceObject = VcpCoreService.Object;
            PrivateObject privatedispalypluginObject = new PrivateObject(displayPlugin);
            privatedispalypluginObject.SetField("_VcpCorePlugin", VcpCoreServiceObject); //getmonitors

            string getVcpCapabilities = "{'CapsDataMap' : {'Ambient Light Sensor': ['ALS full function']}}";
            VcpCoreService.Setup(x => x.GetVCPCapabilities(It.IsAny<MonitorInfo>(), It.IsAny<Guid>(), It.IsAny<Priority>())).Returns(Task.FromResult(getVcpCapabilities)); //GetALSupport GetVCPCapabilities
            var VcpCoreServiceObject1 = VcpCoreService.Object;
            privatedispalypluginObject.SetField("_VcpCorePlugin", VcpCoreServiceObject1);

            ObjGetVCP ObjGetvcp = new ObjGetVCP() { result = true, value = 305u }; //0x66
            var ObjGetvcpValue = ObjGetvcp.value;
            VcpCoreService.Setup(x => x.GetVCPCapability(It.IsAny<MonitorInfo>(), It.IsAny<byte>(), It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<Priority>())).Returns(Task.FromResult(ObjGetvcp));
            var VcpCoreServiceObject2 = VcpCoreService.Object;
            privatedispalypluginObject.SetField("_VcpCorePlugin", VcpCoreServiceObject2);

            PrivateObject privateObject = new PrivateObject(displayPlugin);
            var result = privateObject.Invoke("InitializeAllALSInfo");

            var AllALSConfig_ = DisplayMangerPlugin.AllALSConfig;
            // var caption = (TextBlock)privateObject.GetFieldOrProperty("Caption");
            var caption = privateObject.GetFieldOrProperty("AllALSConfig");
            Assert.IsNotNull(caption);
            Assert.That(AllALSConfig_[0].serialNumber, Is.EqualTo(monitorInfo1.edid.SerialNumber));
            Assert.That(AllALSConfig_[0].DisplayName, Is.EqualTo(monitorInfo1.DisplayName));
        }

        [Test]
        public void TestUpdateALSFeatureByValue()
        {
            PrivateObject privatedispalypluginObject = new PrivateObject(displayPlugin);

            string getVcpCapabilities = "{'CapsDataMap' : {'Ambient Light Sensor': ['ALS full function']}}";
            VcpCoreService.Setup(x => x.GetVCPCapabilities(It.IsAny<MonitorInfo>(), It.IsAny<Guid>(), It.IsAny<Priority>())).Returns(Task.FromResult(getVcpCapabilities)); //GetALSupport GetVCPCapabilities
            var VcpCoreServiceObject1 = VcpCoreService.Object;
            privatedispalypluginObject.SetField("_VcpCorePlugin", VcpCoreServiceObject1);

            ObjGetVCP ObjGetvcp = new ObjGetVCP() { result = true, value = 305u }; //0x66
            var ObjGetvcpValue = ObjGetvcp.value;
            VcpCoreService.Setup(x => x.GetVCPCapability(It.IsAny<MonitorInfo>(), It.IsAny<byte>(), It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<Priority>())).Returns(Task.FromResult(ObjGetvcp));
            var VcpCoreServiceObject2 = VcpCoreService.Object;
            privatedispalypluginObject.SetField("_VcpCorePlugin", VcpCoreServiceObject2);
            uint value = 305u;

            ALSConfig aconfigNulll = new ALSConfig();
            aconfigNulll = null;
            PrivateObject privateObject = new PrivateObject(displayPlugin);

            if (aconfigNulll == null)
            {
                ALSConfig result = (ALSConfig)privateObject.Invoke("UpdateALSFeatureByValue", monitorInfo1, value);
                Assert.IsNotNull(result);
                var AllALSConfig_ = DisplayMangerPlugin.AllALSConfig;
                Assert.That(AllALSConfig_[0].serialNumber, Is.EqualTo(monitorInfo1.edid.SerialNumber));
                Assert.That(AllALSConfig_[0].DisplayName, Is.EqualTo(monitorInfo1.DisplayName));
            }
            else
            {
                List<ALSConfig> allALSConfig = new List<ALSConfig>();
                allALSConfig.Add(aconfig);
                DisplayMangerPlugin.AllALSConfig = allALSConfig;
                var allALSConfig2 = DisplayMangerPlugin.AllALSConfig;
                ALSConfig result = (ALSConfig)privateObject.Invoke("UpdateALSFeatureByValue", monitorInfo1, value);
                Assert.IsNotNull(result);
                Assert.Greater(allALSConfig2.Count, 0);
            }
        }

        [Test]
        public void TestGetALSupport()
        {
            PrivateObject privatedispalypluginObject = new PrivateObject(displayPlugin);
            string getVcpCapabilities = "{'CapsDataMap' : {'Ambient Light Sensor': ['ALS without sensor']}}";
            VcpCoreService.Setup(x => x.GetVCPCapabilities(It.IsAny<MonitorInfo>(), It.IsAny<Guid>(), It.IsAny<Priority>())).Returns(Task.FromResult(getVcpCapabilities)); //GetALSupport GetVCPCapabilities
            var VcpCoreServiceObject1 = VcpCoreService.Object;
            privatedispalypluginObject.SetField("_VcpCorePlugin", VcpCoreServiceObject1);
            ALSConfig aconfig2 = new ALSConfig()
            {
                DisplayName = "DISPLAY7",
                serialNumber = "808597589",
                isSupportALS = 0,
                isMMSEnable = false,
                isPrimaryMonitorSync = false,
                isAutoBrightness = false,
                isAutoColorTemp = false,
                LiftTone = 0,
                AllValue = 0,
                result = false,
                AutoBrightnessRangeLevel = new AutoBrightnessRangeLevel() { level_name = "Low", level_value = 0 }
            };
            PrivateObject privateObject = new PrivateObject(displayPlugin);
            int issupportAls = 0;
            if (!string.IsNullOrEmpty(getVcpCapabilities))
            {
                var result = privateObject.Invoke("GetALSupport", monitorInfo1, aconfig2);
                Assert.That(issupportAls, Is.EqualTo(aconfig2.isSupportALS));
                Assert.IsTrue(aconfig2.result);
            }
            else
            {
                var result = privateObject.Invoke("GetALSupport", monitorInfo1, aconfig2);
                var log = privateObject.GetFieldOrProperty("_logs");
                var log2 = "[DisplayMangerPlugin] ALSFeature into GetALSMMS ...";
                Assert.That(log, Is.EqualTo(log2));
                Assert.IsFalse(aconfig2.result);
            }
        }

        [Test]
        public void TestGetALSMMS()
        {
            ALSConfig monitorALS = new ALSConfig()
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
                AutoBrightnessRangeLevel = new AutoBrightnessRangeLevel() { level_name = "Low", level_value = 0 }
            };

            ObjGetVCP ObjGetvcp = new ObjGetVCP() { result = true, value = 1 }; //0xEF,value设置为0，对应isMMSEnable就是false, 1对应isMMSEnable就是true
            var ObjGetvcpValue = ObjGetvcp.value;
            VcpCoreService.Setup(x => x.GetVCPCapability(It.IsAny<MonitorInfo>(), It.IsAny<byte>(), It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<Priority>())).Returns(Task.FromResult(ObjGetvcp));
            var VcpCoreServiceObject = VcpCoreService.Object;
            PrivateObject privatedispalypluginObject = new PrivateObject(displayPlugin);
            privatedispalypluginObject.SetField("_VcpCorePlugin", VcpCoreServiceObject);

            bool IsMMSEnable = true;
            bool IsMMSEDisable = false;
            if (ObjGetvcp != null && ObjGetvcp.result)
            {
                var result = privatedispalypluginObject.Invoke("GetALSMMS", monitorInfo1, monitorALS);
                Assert.That(IsMMSEnable, Is.EqualTo(monitorALS.isMMSEnable));
                Assert.IsTrue(monitorALS.result);
            }
            else
            {
                var result = privatedispalypluginObject.Invoke("GetALSMMS", monitorInfo1, monitorALS);
                Assert.That(IsMMSEDisable, Is.EqualTo(monitorALS.isMMSEnable));
                Assert.IsFalse(monitorALS.result);
            }
        }

        [Test]
        public void TestSetALSMMS()
        {
            PrivateObject privatedispalypluginObject = new PrivateObject(displayPlugin);
            ObjGetVCP ObjGetvcp = new ObjGetVCP() { result = true, value = 1 }; //0xEF,value设置为0，对应isMMSEnable就是false, 1对应isMMSEnable就是true, Sync(MMS)==//0x00 MMS Off; 0x01 MMS On (DUT1);
            var ObjGetvcpValue = ObjGetvcp.value;
            bool SetVCPCapabilityValue = true;

            VcpCoreService.Setup(x => x.SetVCPCapability(It.IsAny<MonitorInfo>(), It.IsAny<byte>(), It.IsAny<uint>(), It.IsAny<Guid>(), It.IsAny<Priority>())).Returns(Task.FromResult(SetVCPCapabilityValue));
            var VcpCoreServiceObject = VcpCoreService.Object;
            privatedispalypluginObject.SetField("_VcpCorePlugin", VcpCoreServiceObject);
            PrivateObject privateObject = new PrivateObject(displayPlugin);
            string value = "ON";
            bool IsMMSEnable = true;
            bool IsMMSEDisable = false;

            Dictionary<string, List<string>> capabilityDic = new Dictionary<string, List<string>>();  //monitorInfo1.CapabilityDic = capabilityDic;  里面包含EF
            capabilityDic.Add("66", new List<string> { "value1" });
            capabilityDic.Add("EF", new List<string> { "value2" });
            monitorInfo1.CapabilityDic = capabilityDic;

            if (SetVCPCapabilityValue)
            {
                var result = privatedispalypluginObject.Invoke("SetALSMMS", monitorInfo1, aconfig, value);
                Assert.That(IsMMSEnable, Is.EqualTo(aconfig.isMMSEnable));
                Assert.IsTrue(aconfig.result);
            }
            else
            {
                var result = privatedispalypluginObject.Invoke("SetALSMMS", monitorInfo1, aconfig);
                Assert.That(IsMMSEDisable, Is.EqualTo(aconfig.isMMSEnable));
                Assert.IsFalse(aconfig.result);
            }
        }

        [Test]
        public void TestGetALSPrimaryMS()
        {
            ObjGetVCP ObjGetvcp = new ObjGetVCP() { result = true, value = 305u }; //0x66,value设置为305u，==Primary ==//Bit 5 : 0 = UnSelected, 1 = Selected
            var ObjGetvcpValue = ObjGetvcp.value;
            VcpCoreService.Setup(x => x.GetVCPCapability(It.IsAny<MonitorInfo>(), It.IsAny<byte>(), It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<Priority>())).Returns(Task.FromResult(ObjGetvcp));
            var VcpCoreServiceObject = VcpCoreService.Object;
            PrivateObject privatedispalypluginObject = new PrivateObject(displayPlugin);
            privatedispalypluginObject.SetField("_VcpCorePlugin", VcpCoreServiceObject);

            bool IsPrimaryMonitorSync = true;
            bool IsPrimaryMonitorSyncDisable = false;
            if (ObjGetvcp != null && ObjGetvcp.result)
            {
                var result = privatedispalypluginObject.Invoke("GetALSPrimaryMS", monitorInfo1, aconfig);
                Assert.That(IsPrimaryMonitorSync, Is.EqualTo(aconfig.isPrimaryMonitorSync));
                Assert.IsTrue(aconfig.result);
            }
            else
            {
                var result = privatedispalypluginObject.Invoke("GetALSPrimaryMS", monitorInfo1, aconfig);
                Assert.That(IsPrimaryMonitorSyncDisable, Is.EqualTo(aconfig.isPrimaryMonitorSync));
                Assert.IsFalse(aconfig.result);
            }
        }

        [Test]
        public void TestSetALSPrimaryMS()
        {
            ObjGetVCP ObjGetvcp = new ObjGetVCP() { result = true, value = 305u }; //0x66,value设置为305u，==Primary ==//Bit 5 : 0 = UnSelected, 1 = Selected
            var ObjGetvcpValue = ObjGetvcp.value;
            VcpCoreService.Setup(x => x.GetVCPCapability(It.IsAny<MonitorInfo>(), It.IsAny<byte>(), It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<Priority>())).Returns(Task.FromResult(ObjGetvcp));
            var VcpCoreServiceObject = VcpCoreService.Object;
            PrivateObject privatedispalypluginObject = new PrivateObject(displayPlugin);
            privatedispalypluginObject.SetField("_VcpCorePlugin", VcpCoreServiceObject);

            bool SetVCPCapabilityValue = true; //0x66
            VcpCoreService.Setup(x => x.SetVCPCapability(It.IsAny<MonitorInfo>(), It.IsAny<byte>(), It.IsAny<uint>(), It.IsAny<Guid>(), It.IsAny<Priority>())).Returns(Task.FromResult(SetVCPCapabilityValue));
            var VcpCoreServiceObject2 = VcpCoreService.Object;
            privatedispalypluginObject.SetField("_VcpCorePlugin", VcpCoreServiceObject2);

            string value = "ON";
            bool IsPrimaryMonitorSync = true;
            bool IsPrimaryMonitorSyncDisable = false;

            if (ObjGetvcp != null && ObjGetvcp.result)
            {
                if (SetVCPCapabilityValue)
                {
                    var result = privatedispalypluginObject.Invoke("SetALSPrimaryMS", monitorInfo1, aconfig, value);
                    Assert.That(IsPrimaryMonitorSync, Is.EqualTo(aconfig.isPrimaryMonitorSync));
                    Assert.IsTrue(aconfig.result);
                }
                else
                {
                    var result = privatedispalypluginObject.Invoke("SetALSPrimaryMS", monitorInfo1, aconfig);
                    Assert.That(IsPrimaryMonitorSyncDisable, Is.EqualTo(aconfig.isPrimaryMonitorSync));
                    Assert.IsFalse(aconfig.result);
                }
            }
            else
            {
                var result = privatedispalypluginObject.Invoke("SetALSPrimaryMS", monitorInfo1, aconfig);
                Assert.That(IsPrimaryMonitorSyncDisable, Is.EqualTo(aconfig.isPrimaryMonitorSync));
                Assert.IsFalse(aconfig.result);
            }
        }

        [Test]
        public void TestGetALSAutoColorTemp()
        {
            ObjGetVCP ObjGetvcp = new ObjGetVCP() { result = true, value = 305u }; //0x66,==Auto Color Temperature==//Bit 4 : 0 = Off, 1 = On就对应true
            var ObjGetvcpValue = ObjGetvcp.value;
            VcpCoreService.Setup(x => x.GetVCPCapability(It.IsAny<MonitorInfo>(), It.IsAny<byte>(), It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<Priority>())).Returns(Task.FromResult(ObjGetvcp));
            var VcpCoreServiceObject = VcpCoreService.Object;
            PrivateObject privatedispalypluginObject = new PrivateObject(displayPlugin);
            privatedispalypluginObject.SetField("_VcpCorePlugin", VcpCoreServiceObject);

            bool IsAutoColorTemp = true;
            bool IsAutoColorTempDisable = false;
            if (ObjGetvcp != null && ObjGetvcp.result)
            {
                var result = privatedispalypluginObject.Invoke("GetALSAutoColorTemp", monitorInfo1, aconfig);
                Assert.That(IsAutoColorTemp, Is.EqualTo(aconfig.isAutoColorTemp));
                Assert.IsTrue(aconfig.result);
            }
            else
            {
                var result = privatedispalypluginObject.Invoke("GetALSAutoColorTemp", monitorInfo1, aconfig);
                Assert.That(IsAutoColorTempDisable, Is.EqualTo(aconfig.isAutoColorTemp));
                Assert.IsFalse(aconfig.result);
            }
        }

        [Test]
        public void TestSetALSAutoColorTemp()
        {
            ObjGetVCP ObjGetvcp = new ObjGetVCP() { result = true, value = 305u }; //0x66,value设置为305u，==Auto Color Temperature==//Bit 4 : 0 = Off, 1 = On
            var ObjGetvcpValue = ObjGetvcp.value;
            VcpCoreService.Setup(x => x.GetVCPCapability(It.IsAny<MonitorInfo>(), It.IsAny<byte>(), It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<Priority>())).Returns(Task.FromResult(ObjGetvcp));
            var VcpCoreServiceObject = VcpCoreService.Object;
            PrivateObject privatedispalypluginObject = new PrivateObject(displayPlugin);
            privatedispalypluginObject.SetField("_VcpCorePlugin", VcpCoreServiceObject);

            bool SetVCPCapabilityValue = true; //0x66
            VcpCoreService.Setup(x => x.SetVCPCapability(It.IsAny<MonitorInfo>(), It.IsAny<byte>(), It.IsAny<uint>(), It.IsAny<Guid>(), It.IsAny<Priority>())).Returns(Task.FromResult(SetVCPCapabilityValue));
            var VcpCoreServiceObject2 = VcpCoreService.Object;
            privatedispalypluginObject.SetField("_VcpCorePlugin", VcpCoreServiceObject2);

            string value = "ON";
            bool setALSAutoColorTemp = true;
            bool setALSAutoColorTempDisable = false;

            if (ObjGetvcp != null && ObjGetvcp.result)
            {
                if (SetVCPCapabilityValue)
                {
                    var result = privatedispalypluginObject.Invoke("SetALSAutoColorTemp", monitorInfo1, aconfig, value);
                    Assert.That(setALSAutoColorTemp, Is.EqualTo(aconfig.isAutoColorTemp));
                    Assert.IsTrue(aconfig.result);
                }
                else
                {
                    var result = privatedispalypluginObject.Invoke("SetALSAutoColorTemp", monitorInfo1, aconfig);
                    Assert.That(setALSAutoColorTemp, Is.EqualTo(aconfig.isAutoColorTemp));
                    Assert.IsFalse(aconfig.result);
                }
            }
            else
            {
                var result = privatedispalypluginObject.Invoke("SetALSAutoColorTemp", monitorInfo1, aconfig);
                Assert.That(setALSAutoColorTempDisable, Is.EqualTo(aconfig.isAutoColorTemp));
                Assert.IsFalse(aconfig.result);
            }
        }

        [Test]
        public void TestGetALSAutoBrightness()
        {
            ObjGetVCP ObjGetvcp = new ObjGetVCP() { result = true, value = 0u }; //0x66,==AutoBrightness==//Bit 0: 0 = Reserved, 1 = AutoBrightness Off || Bit 1: 0 = Reserved, 1 = AutoBrightness On
            var ObjGetvcpValue = ObjGetvcp.value;
            VcpCoreService.Setup(x => x.GetVCPCapability(It.IsAny<MonitorInfo>(), It.IsAny<byte>(), It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<Priority>())).Returns(Task.FromResult(ObjGetvcp));
            var VcpCoreServiceObject = VcpCoreService.Object;
            PrivateObject privatedispalypluginObject = new PrivateObject(displayPlugin);
            privatedispalypluginObject.SetField("_VcpCorePlugin", VcpCoreServiceObject);

            bool IisAutoBrightness = true;
            bool IsAutoBrightnessDisable = false;
            if (ObjGetvcp != null && ObjGetvcp.result)
            {
                var result = privatedispalypluginObject.Invoke("GetALSAutoBrightness", monitorInfo1, aconfig);
                Assert.That(IisAutoBrightness, Is.EqualTo(aconfig.isAutoBrightness)); //val=0,on=true
                Assert.IsTrue(aconfig.result);
            }
            else
            {
                var result = privatedispalypluginObject.Invoke("GetALSAutoBrightness", monitorInfo1, aconfig);
                Assert.That(IsAutoBrightnessDisable, Is.EqualTo(aconfig.isAutoBrightness));
                Assert.IsFalse(aconfig.result);
            }
        }

        [Test]
        public void TestSetALSAutoBrightness()
        {
            ObjGetVCP ObjGetvcp = new ObjGetVCP() { result = true, value = 305u }; //0x66,value设置为305u，==AutoBrightness==//Bit 0: 0 = Reserved, 1 = AutoBrightness Off || Bit 1: 0 = Reserved, 1 = AutoBrightness On
            var ObjGetvcpValue = ObjGetvcp.value;
            VcpCoreService.Setup(x => x.GetVCPCapability(It.IsAny<MonitorInfo>(), It.IsAny<byte>(), It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<Priority>())).Returns(Task.FromResult(ObjGetvcp));
            var VcpCoreServiceObject = VcpCoreService.Object;
            PrivateObject privatedispalypluginObject = new PrivateObject(displayPlugin);
            privatedispalypluginObject.SetField("_VcpCorePlugin", VcpCoreServiceObject);

            bool SetVCPCapabilityValue = true; //0x66
            VcpCoreService.Setup(x => x.SetVCPCapability(It.IsAny<MonitorInfo>(), It.IsAny<byte>(), It.IsAny<uint>(), It.IsAny<Guid>(), It.IsAny<Priority>())).Returns(Task.FromResult(SetVCPCapabilityValue));
            var VcpCoreServiceObject2 = VcpCoreService.Object;
            privatedispalypluginObject.SetField("_VcpCorePlugin", VcpCoreServiceObject2);

            Dictionary<string, List<string>> capabilityDic = new Dictionary<string, List<string>>();  //monitorInfo1.CapabilityDic = capabilityDic;  里面包含66
            capabilityDic.Add("66", new List<string> { "value1" });
            capabilityDic.Add("EF", new List<string> { "value2" });
            monitorInfo1.CapabilityDic = capabilityDic;

            string value = "ON";
            bool setALSAutoBrightness = true;
            bool setALSAutoBrightnessDisable = false;

            if (ObjGetvcp != null && ObjGetvcp.result)
            {
                if (SetVCPCapabilityValue)
                {
                    var result = privatedispalypluginObject.Invoke("SetALSAutoBrightness", monitorInfo1, aconfig, value);
                    Assert.That(setALSAutoBrightness, Is.EqualTo(aconfig.isAutoBrightness));
                    Assert.IsTrue(aconfig.result);
                }
                else
                {
                    var result = privatedispalypluginObject.Invoke("SetALSAutoBrightness", monitorInfo1, aconfig);
                    Assert.That(setALSAutoBrightnessDisable, Is.EqualTo(aconfig.isAutoBrightness));
                    Assert.IsFalse(aconfig.result);
                }
            }
            else
            {
                var result = privatedispalypluginObject.Invoke("SetALSAutoBrightness", monitorInfo1, aconfig);
                Assert.That(setALSAutoBrightnessDisable, Is.EqualTo(aconfig.isAutoBrightness));
                Assert.IsFalse(aconfig.result);
            }
        }

        [Test]
        public void TestGetALSAutoBrightnessRangeLevel()
        {
            ALSConfig monitorALS = new ALSConfig()
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
                //AutoBrightnessRangeLevel = new List<AutoBrightnessRangeLevel>() { new AutoBrightnessRangeLevel() { level_name = "Mid", level_value = 1 } }
            };

            ObjGetVCP ObjGetvcp = new ObjGetVCP() { result = true, value = 305u }; //0x66,==Auto Brightness Range  Level==//Bit 6~7 : 0=Leve 1 | 1=Level 2 | 2=Level 3
            var ObjGetvcpValue = ObjGetvcp.value;
            VcpCoreService.Setup(x => x.GetVCPCapability(It.IsAny<MonitorInfo>(), It.IsAny<byte>(), It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<Priority>())).Returns(Task.FromResult(ObjGetvcp));
            var VcpCoreServiceObject = VcpCoreService.Object;
            PrivateObject privatedispalypluginObject = new PrivateObject(displayPlugin);
            privatedispalypluginObject.SetField("_VcpCorePlugin", VcpCoreServiceObject);

            List<AutoBrightnessRangeLevel> brightnessrangelevellist = new List<AutoBrightnessRangeLevel>();
            AutoBrightnessRangeLevel brightnessrangelevel = new AutoBrightnessRangeLevel() { level_name = "Low", level_value = 0 };
            brightnessrangelevellist.Add(brightnessrangelevel);

            if (ObjGetvcp != null && ObjGetvcp.result)
            {
                var result = privatedispalypluginObject.Invoke("GetALSAutoBrightnessRangeLevel", monitorInfo1, monitorALS);
                Assert.That(brightnessrangelevellist[0].level_name, Is.EqualTo(monitorALS.AutoBrightnessRangeLevel.level_name)); //level_name = "Low"
                Assert.That(brightnessrangelevellist[0].level_value, Is.EqualTo(monitorALS.AutoBrightnessRangeLevel.level_value)); //level_value = 0
                Assert.IsTrue(monitorALS.result);
            }
            else
            {
                var result = privatedispalypluginObject.Invoke("GetALSAutoBrightnessRangeLevel", monitorInfo1, monitorALS);
                Assert.IsFalse(monitorALS.result);
            }
        }

        /*[Test]
        public void TestSetALSAutoBrightnessRangeLevel()
        {
            ALSConfig monitorALS = new ALSConfig()
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
            };
            ObjGetVCP ObjGetvcp = new ObjGetVCP() { result = true, value = 305u }; //0x66,value设置为305u，
            var ObjGetvcpValue = ObjGetvcp.value;
            VcpCoreService.Setup(x => x.GetVCPCapability(It.IsAny<MonitorInfo>(), It.IsAny<byte>(), It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<Priority>())).Returns(Task.FromResult(ObjGetvcp));
            var VcpCoreServiceObject = VcpCoreService.Object;
            PrivateObject privatedispalypluginObject = new PrivateObject(displayPlugin);
            privatedispalypluginObject.SetField("_VcpCorePlugin", VcpCoreServiceObject);

            bool SetVCPCapabilityValue = true; //0x66
            VcpCoreService.Setup(x => x.SetVCPCapability(It.IsAny<MonitorInfo>(), It.IsAny<byte>(), It.IsAny<uint>(), It.IsAny<Guid>(), It.IsAny<Priority>())).Returns(Task.FromResult(SetVCPCapabilityValue));
            var VcpCoreServiceObject2 = VcpCoreService.Object;
            privatedispalypluginObject.SetField("_VcpCorePlugin", VcpCoreServiceObject2);

            string value = "1"; //0 = Low,1 = Mid,2 = High
            List<AutoBrightnessRangeLevel> brightnessrangelevellist = new List<AutoBrightnessRangeLevel>();
            AutoBrightnessRangeLevel brightnessrangelevel = new AutoBrightnessRangeLevel() { level_name = "Mid", level_value = 0 };
            brightnessrangelevellist.Add(brightnessrangelevel);

            Dictionary<string, List<string>> capabilityDic = new Dictionary<string, List<string>>();  //monitorInfo1.CapabilityDic = capabilityDic;  里面包含66
            capabilityDic.Add("66", new List<string> { "value1" });
            capabilityDic.Add("EF", new List<string> { "value2" });
            monitorInfo1.CapabilityDic = capabilityDic;

            if (ObjGetvcp != null && ObjGetvcp.result)
            {
                if (SetVCPCapabilityValue)
                {
                    var result = privatedispalypluginObject.Invoke("SetALSAutoBrightnessRangeLevel", monitorInfo1, monitorALS, value);
                    Assert.That(brightnessrangelevellist[0].level_name, Is.EqualTo(monitorALS.AutoBrightnessRangeLevel.level_name)); //set level_name = "Low"
                    Assert.That(brightnessrangelevellist[0].level_value, Is.EqualTo(monitorALS.AutoBrightnessRangeLevel.level_value)); //set level_value = 0
                    Assert.IsTrue(monitorALS.result);
                }
                else
                {
                    var result = privatedispalypluginObject.Invoke("SetALSAutoBrightnessRangeLevel", monitorInfo1, monitorALS);
                    Assert.IsFalse(monitorALS.result);
                }
            }
            else
            {
                var result = privatedispalypluginObject.Invoke("SetALSAutoBrightnessRangeLevel", monitorInfo1, monitorALS);
                Assert.IsFalse(monitorALS.result);
            }
        }*/

        [Test]
        public void TestGetALSAll()
        {
            ObjGetVCP ObjGetvcp = new ObjGetVCP() { result = true, value = 305u }; //0x66,==AutoBrightness==//Bit 0: 0 = Reserved, 1 = AutoBrightness Off || Bit 1: 0 = Reserved, 1 = AutoBrightness On
            var ObjGetvcpValue = ObjGetvcp.value;
            VcpCoreService.Setup(x => x.GetVCPCapability(It.IsAny<MonitorInfo>(), It.IsAny<byte>(), It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<Priority>())).Returns(Task.FromResult(ObjGetvcp));
            var VcpCoreServiceObject = VcpCoreService.Object;
            PrivateObject privatedispalypluginObject = new PrivateObject(displayPlugin);
            privatedispalypluginObject.SetField("_VcpCorePlugin", VcpCoreServiceObject);

            bool IisAutoBrightness = false;
            uint allValue = 305u;
            bool IsAutoColorTemp = true;
            bool IsPrimaryMonitorSync = true;
            List<AutoBrightnessRangeLevel> brightnessrangelevellist = new List<AutoBrightnessRangeLevel>();
            AutoBrightnessRangeLevel brightnessrangelevel = new AutoBrightnessRangeLevel() { level_name = "Low", level_value = 0 };
            brightnessrangelevellist.Add(brightnessrangelevel);
            monitorInfo1.CapabilityString = "(prot(monitor)type(LCD)model(U2424H)cmds(01 02 03 07 0C E3 F3)vcp(02 04 05 08 10 12 14(01 04 05 06 08 09 0B 0C)E5 E7(02 03) E2(00 02 04 0C 0D 0F) 66"; //CapabilityString Contains 66
            if (ObjGetvcp != null && ObjGetvcp.result)
            {
                var result = privatedispalypluginObject.Invoke("GetALSAll", monitorInfo1, aconfig);
                Assert.That(IisAutoBrightness, Is.EqualTo(aconfig.isAutoBrightness)); //val=0,on=true
                Assert.That(allValue, Is.EqualTo(aconfig.AllValue));
                Assert.That(IsAutoColorTemp, Is.EqualTo(aconfig.isAutoColorTemp));
                Assert.That(IsPrimaryMonitorSync, Is.EqualTo(aconfig.isPrimaryMonitorSync));
                Assert.That(brightnessrangelevellist[0].level_name, Is.EqualTo(aconfig.AutoBrightnessRangeLevel.level_name)); //set level_name = "Low"
                Assert.That(brightnessrangelevellist[0].level_value, Is.EqualTo(aconfig.AutoBrightnessRangeLevel.level_value)); //set level_value = 0
                Assert.IsTrue(aconfig.result);
            }
            else
            {
                var result = privatedispalypluginObject.Invoke("GetALSAll", monitorInfo1, aconfig);
                Assert.IsFalse(aconfig.result);
            }
        }

        [Test]
        public void TestParseBitDefineToAlsObject()
        {
            uint vcp_value = 305u;
            bool IisAutoBrightness = false;
            uint allValue = 305u;
            bool IsAutoColorTemp = true;
            bool IsPrimaryMonitorSync = true;
            List<AutoBrightnessRangeLevel> brightnessrangelevellist = new List<AutoBrightnessRangeLevel>();
            AutoBrightnessRangeLevel brightnessrangelevel = new AutoBrightnessRangeLevel() { level_name = "Low", level_value = 0 };
            brightnessrangelevellist.Add(brightnessrangelevel);

            PrivateObject privatedispalypluginObject = new PrivateObject(displayPlugin);
            privatedispalypluginObject.Invoke("ParseBitDefineToAlsObject", vcp_value, aconfig);
            Assert.That(IisAutoBrightness, Is.EqualTo(aconfig.isAutoBrightness)); //val=0,on=true
            Assert.That(allValue, Is.EqualTo(aconfig.AllValue));
            Assert.That(IsAutoColorTemp, Is.EqualTo(aconfig.isAutoColorTemp));
            Assert.That(IsPrimaryMonitorSync, Is.EqualTo(aconfig.isPrimaryMonitorSync));
            Assert.That(brightnessrangelevellist[0].level_name, Is.EqualTo(aconfig.AutoBrightnessRangeLevel.level_name)); //set level_name = "Low"
            Assert.That(brightnessrangelevellist[0].level_value, Is.EqualTo(aconfig.AutoBrightnessRangeLevel.level_value)); //set level_value = 0
        }

        [Test]
        public void TestSetALSAll()
        {
            ALSConfig monitorALS = new ALSConfig()
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
            };

            PrivateObject privatedispalypluginObject = new PrivateObject(displayPlugin);
            bool SetVCPCapabilityValue = true; //0x66
            VcpCoreService.Setup(x => x.SetVCPCapability(It.IsAny<MonitorInfo>(), It.IsAny<byte>(), It.IsAny<uint>(), It.IsAny<Guid>(), It.IsAny<Priority>())).Returns(Task.FromResult(SetVCPCapabilityValue));
            var VcpCoreServiceObject2 = VcpCoreService.Object;
            privatedispalypluginObject.SetField("_VcpCorePlugin", VcpCoreServiceObject2);

            string value = "1"; //0 = Low,1 = Mid,2 = High
            uint allvalue = 1u;
            List<AutoBrightnessRangeLevel> brightnessrangelevellist = new List<AutoBrightnessRangeLevel>();
            AutoBrightnessRangeLevel brightnessrangelevel = new AutoBrightnessRangeLevel() { level_name = "Mid", level_value = 0 };
            brightnessrangelevellist.Add(brightnessrangelevel);
            monitorInfo1.CapabilityString = "(prot(monitor)type(LCD)model(U2424H)cmds(01 02 03 07 0C E3 F3)vcp(02 04 05 08 10 12 14(01 04 05 06 08 09 0B 0C)E5 E7(02 03) E2(00 02 04 0C 0D 0F) 66"; //CapabilityString Contains 66
            if (SetVCPCapabilityValue)
            {
                var result = privatedispalypluginObject.Invoke("SetALSAll", monitorInfo1, monitorALS, value);
                Assert.That(allvalue, Is.EqualTo(monitorALS.AllValue));
                Assert.IsTrue(monitorALS.result);
            }
            else
            {
                var result = privatedispalypluginObject.Invoke("SetALSAll", monitorInfo1, monitorALS);
                Assert.IsFalse(monitorALS.result);
            }
        }

        [Test]
        public void TestUpdateAllValue()
        {
            ALSConfig monitorALS = new ALSConfig()
            {
                DisplayName = "DISPLAY7",
                serialNumber = "808597589",
                isSupportALS = 2,
                isMMSEnable = false,
                isPrimaryMonitorSync = true,
                isAutoBrightness = true,
                isAutoColorTemp = true,
                LiftTone = 0,
                AllValue = 305u,
                result = false,
                AutoBrightnessRangeLevel = new AutoBrightnessRangeLevel() { level_name = "Mid", level_value = 1 }
            };

            uint value = monitorALS.AllValue;
            // Rule 1
            if (monitorALS.isAutoBrightness)
            {
                value &= ~((uint)1 << 0); // Set bit0 to 0
                value |= (uint)1 << 1; // Set bit1 to 1
            }
            else
            {
                value |= (uint)1 << 0; // Set bit0 to 1
                value &= ~((uint)1 << 1); // Set bit1 to 0
            }
            // Rule 2
            if (monitorALS.isAutoColorTemp)
            {
                value |= (uint)1 << 4; // Set bit4 to 1
            }
            else
            {
                value &= ~((uint)1 << 4); // Clear bit4 to 0
            }
            // Rule 3
            if (monitorALS.isPrimaryMonitorSync)
            {
                value |= (uint)1 << 5; // Set bit5 to 1
            }
            else
            {
                value &= ~((uint)1 << 5); // Clear bit5 to 0
            }
            // Rules 4-6
            if (monitorALS.AutoBrightnessRangeLevel != null)
            {
                var level = monitorALS.AutoBrightnessRangeLevel;
                if (level.level_name == "Low" && level.level_value == 0)
                {
                    value &= ~((uint)1 << 6); // Clear bit6 to 0
                    value &= ~((uint)1 << 7); // Clear bit7 to 0
                }
                else if (level.level_name == "Mid" && level.level_value == 1)
                {
                    value |= (uint)1 << 6; // Set bit6 to 1
                    value &= ~((uint)1 << 7); // Clear bit7 to 0
                }
                else if (level.level_name == "High" && level.level_value == 2)
                {
                    value &= ~((uint)1 << 6); // Clear bit6 to 0
                    value |= (uint)1 << 7; // Set bit7 to 1
                }
            }

            PrivateObject privatedispalypluginObject = new PrivateObject(displayPlugin);
            var result = privatedispalypluginObject.Invoke("UpdateAllValue", monitorALS);
            Assert.That(value, Is.EqualTo(result));
        }

        [Test]
        public void TestSetBitsValue()
        {
            uint number = 305u;
            int startBitPosition = 5;
            int value2 = 1; // 1="On"
            PrivateObject privatedispalypluginObject = new PrivateObject(displayPlugin);
            var result = privatedispalypluginObject.Invoke("SetBitsValue", number, startBitPosition, value2);
            Assert.That(number, Is.EqualTo(result));
        }

        [Test]
        public void TestGetBitsValue()
        {
            uint number = 273u;
            int startBitPosition = 4;
            PrivateObject privatedispalypluginObject = new PrivateObject(displayPlugin);
            var result = privatedispalypluginObject.Invoke("GetBitsValue", number, startBitPosition);
            uint bitValue = ((number >> startBitPosition) & 0b11u); //getALSAutoColorTemp 1="On"
            Assert.That(bitValue, Is.EqualTo(result));
        }

        [Test]
        public void TestStrConvertOnOff()
        {
            string onoff = "ON";
            bool StrConvertOnOffResult = true;
            bool strConvertOnOffResult = false;
            if (onoff == "ON")
            {
                PrivateObject privatedispalypluginObject = new PrivateObject(displayPlugin);
                var result = privatedispalypluginObject.Invoke("StrConvertOnOff", onoff);
                Assert.That(StrConvertOnOffResult, Is.EqualTo(result));
            }
            else
            {
                PrivateObject privatedispalypluginObject = new PrivateObject(displayPlugin);
                var result = privatedispalypluginObject.Invoke("StrConvertOnOff", onoff);
                Assert.That(strConvertOnOffResult, Is.EqualTo(result));
            }
        }

        [Test]
        public void TestStrConvertUint()
        {
            string onoff = "ON";
            uint StrConvertUintResultON = 1;
            uint StrConvertUintResultOff = 0;
            if (onoff == "ON")
            {
                PrivateObject privatedispalypluginObject = new PrivateObject(displayPlugin);
                var result = privatedispalypluginObject.Invoke("StrConvertUint", onoff);
                Assert.That(StrConvertUintResultON, Is.EqualTo(result));
            }
            else
            {
                PrivateObject privatedispalypluginObject = new PrivateObject(displayPlugin);
                var result = privatedispalypluginObject.Invoke("StrConvertOnOff", onoff);
                Assert.That(StrConvertUintResultOff, Is.EqualTo(result));
            }
        }

        [Test]
        public void TestInitializeMonitorsList()
        {
            List<MonitorInfo> _allInfoMonitors = new List<MonitorInfo>();
            _allInfoMonitors.Add(monitorInfo1);
            VcpCoreService.Setup(x => x.GetMonitors()).Returns(Task.FromResult(_allInfoMonitors));
            var VcpCoreServiceObject = VcpCoreService.Object;
            PrivateObject privatedispalypluginObject = new PrivateObject(displayPlugin);
            privatedispalypluginObject.SetField("_VcpCorePlugin", VcpCoreServiceObject);
            privatedispalypluginObject.Invoke("InitializeMonitorsList");
            var monitor = (List<MonitorInfo>)privatedispalypluginObject.GetFieldOrProperty("_AllInfoMonitors");
            Assert.Greater(monitor.Count, 0);
        }

        [Test]
        public void TestInitializeVcpCorePlugin()
        {
            PrivateObject privatedispalypluginObject = new PrivateObject(displayPlugin);
            var result = privatedispalypluginObject.Invoke("InitializeMonitorsList");
            Assert.IsNotNull(vcpCorePlugin);
        }

        [Test]
        public void TestGetCurrentVcpCoreCondition()
        {
            PrivateObject privatedispalypluginObject = new PrivateObject(displayPlugin);
            var result = privatedispalypluginObject.Invoke("GetCurrentVcpCoreCondition");
            Assert.IsNotNull(vcpCorePlugin);
        }

        [Test]
        public void TestSetDisplayOrientation_()
        {
            VCPchangedEventArgs vCPchangedEventArgs = new VCPchangedEventArgs()  //0xAA  "Screen Orientation" value 1u=Angle0,2u=Angle90
            {
                vcpcode = "AA",
                value = "1",
                monitor = monitorInfo1
            };
            bool ActualResult = true;
            DisplayPropertiesService.Setup(x => x.SetDisplayPropertiest(It.IsAny<string>(), It.IsAny<Properties>(), It.IsAny<DisplayOrientation>())).Returns(Task.FromResult(ActualResult));
            var displayPropertiesServiceObject = DisplayPropertiesService.Object;
            PrivateObject privatedispalypluginObject = new PrivateObject(displayPlugin);
            privatedispalypluginObject.SetField("_DisplayPropertiesPlugin", displayPropertiesServiceObject);
            var result = (Task<bool>)privatedispalypluginObject.Invoke("SetDisplayOrientation", vCPchangedEventArgs);
            Assert.IsTrue(result.Result);
        }

        [Test]
        public void TestIsSupportHDR()
        {
            string capabilitystring = "E2(25, 23, 24, 26, 27, 3A, 3B, 3C )";
            PrivateObject privatedispalypluginObject = new PrivateObject(displayPlugin);
            var result = (bool)privatedispalypluginObject.Invoke("IsSupportHDR", capabilitystring);
            try
            {
                if (capabilitystring == "" || capabilitystring.Length < 10)
                {
                    Assert.IsFalse(result);
                }
                string[] ss = capabilitystring.Split("E2(");
                ss = ss[1].Split(")");
                ss = ss[0].Split(" ");
                string[] stand = new string[] { "25", "23", "24", "26", "27", "3A", "3B", "3C" };
                for (int i = 0; i < ss.Length; i++)
                {
                    for (int j = 0; j < stand.Length; j++)
                    {
                        if (ss[i].Equals(stand[j]))
                        {
                            Assert.IsTrue(result);
                        }
                    }
                }
                Assert.IsTrue(result);
            }
            catch
            {
                Assert.IsFalse(result);
            }
        }

        [Test]
        public void TestIsSupportUSBCPrioritization()
        {
            string capabilitystring = "EA(F8, F800, F801)";
            PrivateObject privatedispalypluginObject = new PrivateObject(displayPlugin);
            var result = (bool)privatedispalypluginObject.Invoke("IsSupportUSBCPrioritization", capabilitystring);
            try
            {
                if (capabilitystring == "" || capabilitystring.Length < 10)
                {
                    Assert.IsFalse(result);
                }
                string[] ss = capabilitystring.Split("EA(");
                ss = ss[1].Split(")");
                ss = ss[0].Split(" ");
                string[] stand = new string[] { "F8", "F800", "F801" };
                for (int i = 0; i < ss.Length; i++)
                {
                    for (int j = 0; j < stand.Length; j++)
                    {
                        if (ss[i].Equals(stand[j]))
                        {
                            Assert.IsTrue(result);
                        }
                    }
                }
                Assert.IsTrue(result);
            }
            catch
            {
                Assert.IsFalse(result);
            }
        }

        [Test]
        public void TestInitializeDisplayPropertiesPlugin()
        {
            PrivateObject privatedispalypluginObject = new PrivateObject(displayPlugin);
            var result = privatedispalypluginObject.Invoke("InitializeDisplayPropertiesPlugin");
            Assert.IsNotNull(displayPropertiesPlugin);
        }

        /* [Test]
         public void TestSyncPrimaryMonitorBrightnessAndColorTemp()
         {
             MonitorInfo monitorInfoMain = monitorInfo1;
             MonitorInfo monitorvalue = monitorInfo1;
             string vcpcode;
             ObjGetVCP val = new ObjGetVCP() { result = true, value = (uint)20 };
             vcpcode = "60";
             Mock<IVcpCoreService> mockVcpCoreService = new Mock<IVcpCoreService>();
             if (vcpcode != "67" || vcpcode != "68")
             {
                 var result = displayPlugin.SyncPrimaryMonitorBrightnessAndColorTemp(monitorInfoMain, monitorvalue, vcpcode, val.ToString()).Result;  // vcpcode = "60"
                 Assert.IsTrue(result);
             }

             vcpcode = "67";
             if (vcpcode != "67" || vcpcode != "68")
             {
                 var result = displayPlugin.SyncPrimaryMonitorBrightnessAndColorTemp(monitorInfoMain, monitorvalue, vcpcode, val.ToString()).Result;  // aconfig == null
                 Assert.IsFalse(result);
             }

             _AllALSConfig = new List<ALSConfig>();
             aconfig.Edid = monitorInfo1.edid;
             _AllALSConfig.Add(aconfig);
             DisplayMangerPlugin.AllALSConfig = _AllALSConfig;  // aconfig != null

             vcpcode = "68";
             PrivateObject privatedispalypluginObject = new PrivateObject(displayPlugin);
             mockVcpCoreService.Setup(x => x.SetVCPCapability(It.IsAny<MonitorInfo>(), It.IsAny<byte>(), It.IsAny<uint>(), It.IsAny<Guid>(), It.IsAny<Priority>())).Returns(Task.FromResult(true));
             privatedispalypluginObject.SetFieldOrProperty("_VcpCorePlugin", mockVcpCoreService.Object);

             if (vcpcode == "67" || vcpcode == "68")
             {
                 var result = displayPlugin.SyncPrimaryMonitorBrightnessAndColorTemp(monitorInfoMain, monitorvalue, vcpcode, val.ToString()).Result; // vcpcode = "68"
                 Assert.IsTrue(result);
             }

             vcpcode = "67";
             if (vcpcode == "67" || vcpcode == "68")
             {
                 var result = displayPlugin.SyncPrimaryMonitorBrightnessAndColorTemp(monitorInfoMain, monitorvalue, vcpcode, val.ToString()).Result; //vcpcode = "67"
                 Assert.IsTrue(result);
             }
         }*/

        [Test]
        public void TestSyncPrimaryMonitorValueToOtherMonitor()
        {
            MonitorInfo moMain = monitorInfo1;
            List<MonitorInfo> monitorAll = new List<MonitorInfo>();
            MonitorInfo monitorInfoALS = new MonitorInfo() { DisplayName = "TestDisplay12", AliasDeviceName = "TestDisplay12", edid = new EDID() { ServiceTag = "123456", SerialNumber = "123456" } };
            monitorAll.Add(monitorInfo1);
            monitorAll.Add(monitorInfoALS);
            aconfig.Edid = new EDID() { ServiceTag = "123456", SerialNumber = "123456" };
            _AllALSConfig = new List<ALSConfig>();
            _AllALSConfig.Add(aconfig);
            List<ALSConfig> exitAls = _AllALSConfig;
            ALSConfig moMainvalue = aconfig;
            string vcpcode = "20";

            Mock<IVcpCoreService> mockVcpCoreService = new Mock<IVcpCoreService>();
            PrivateObject privatedispalypluginObject = new PrivateObject(displayPlugin);
            privatedispalypluginObject.SetFieldOrProperty("_VcpCorePlugin", mockVcpCoreService.Object);
            ObjGetVCP obBrightnessContrast = new ObjGetVCP() { result = true, value = (uint)30 };
            ObjGetVCP obColor = new ObjGetVCP() { result = true, value = (uint)20 };
            mockVcpCoreService.Setup(x => x.GetVCPCapability(It.IsAny<MonitorInfo>(), It.IsAny<byte>(), It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<Priority>())).Returns(Task.FromResult(obBrightnessContrast));
            mockVcpCoreService.Setup(x => x.GetVCPCapability(It.IsAny<MonitorInfo>(), It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<Priority>())).Returns(Task.FromResult(obColor));
            mockVcpCoreService.Setup(x => x.SetVCPCapability(It.IsAny<MonitorInfo>(), It.IsAny<byte>(), It.IsAny<uint>(), It.IsAny<Guid>(), It.IsAny<Priority>())).Returns(Task.FromResult(true));
            mockVcpCoreService.Setup(x => x.SetVCPCapability(It.IsAny<MonitorInfo>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Priority>())).Returns(Task.FromResult(true));
            var result = displayPlugin.SyncPrimaryMonitorValueToOtherMonitor(moMain, monitorAll, ref exitAls, moMainvalue, vcpcode).Result;
            Assert.IsTrue(result);
        }

        [Test]
        public void TestUpdateExistAlsConfig()
        {
            MonitorInfo monitorInfoAL = new MonitorInfo() { DisplayName = "TestDisplay12", AliasDeviceName = "TestDisplay12", edid = new EDID() { ServiceTag = "123456", SerialNumber = "123456" } };
            List<MonitorInfo> monitorInfoMain = new List<MonitorInfo>();
            monitorInfoMain.Add(monitorInfoAL);
            aconfig.Edid = new EDID() { ServiceTag = "123456", SerialNumber = "123456" };
            _AllALSConfig = new List<ALSConfig>();
            _AllALSConfig.Add(aconfig);
            DisplayMangerPlugin.AllALSConfig = _AllALSConfig;
            var allALSConfig_ = DisplayMangerPlugin.AllALSConfig;

            var UpdateExistAlsConfig_result = displayPlugin.UpdateExistAlsConfig(monitorInfoMain).Result;
            Assert.IsNotNull(UpdateExistAlsConfig_result);
            Assert.Greater(UpdateExistAlsConfig_result.Count, 0);
        }

        [Test]
        public void TestisScreenPartition()
        {
            Mock<IVcpCoreService> mockVcpCoreService = new Mock<IVcpCoreService>();
            PrivateObject privatedispalypluginObject = new PrivateObject(displayPlugin);
            privatedispalypluginObject.SetFieldOrProperty("_VcpCorePlugin", mockVcpCoreService.Object);
            ObjGetVCP isNotScreenPartition = new ObjGetVCP() { result = false, value = (uint)30 };
            ObjGetVCP isScreenPartition = new ObjGetVCP() { result = true, value = (uint)256 };   //The eighth position is 1
            mockVcpCoreService.Setup(x => x.GetVCPCapability(It.IsAny<MonitorInfo>(), It.IsAny<byte>(), It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<Priority>())).Returns(Task.FromResult(isNotScreenPartition));
            if (isNotScreenPartition != null && !isNotScreenPartition.result)
            {
                var isScreenPartition_result1 = displayPlugin.isScreenPartition(monitorInfo1).Result;
                Assert.IsFalse(isScreenPartition_result1);
            }

            mockVcpCoreService.Setup(x => x.GetVCPCapability(It.IsAny<MonitorInfo>(), It.IsAny<byte>(), It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<Priority>())).Returns(Task.FromResult(isScreenPartition));
            if (isScreenPartition != null && isScreenPartition.result)
            {
                var isScreenPartition_result2 = displayPlugin.isScreenPartition(monitorInfo1).Result;
                Assert.IsTrue(isScreenPartition_result2);
            }
        }
    }
}