using DDPM.SA.Common;
using DDPM.SA.Plugins.User.DisplayManager;
using DDPM.SA.Plugins.User.DisplayProperties;
using DDPM.SA.Plugins.User.PipPbpManger;
using Dell.Client.Framework.Interfaces;
using Dell.Client.Framework.UnitTestShared.Tests;
using Moq;

//using static DDPM.SA.Plugins.User.DisplayProperties.user32;
using System.ComponentModel;
using VcpCore.Common;
using VcpCore.Interfaces;

namespace VcpCore.Plugins.Test
{
    public class TestVCPCorePlugin
    {
        private Mock<IAgent> DisplayMangerAgent { get; } = new();
        private Mock<IAgent> VcpCoreAgent { get; } = new();
        private Mock<IAgent> PipPbpAgent = new();
        private Mock<IAgent> DisplayPropertiesAgent { get; } = new();
        private MonitorInfo monitorInfo = new MonitorInfo();

        private BackgroundWorker _taskQueueExecutor = new BackgroundWorker();

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

        private DisplayMangerPlugin displayPlugin;
        private VcpCorePlugin vcpCorePlugin;
        private PipPbpMangerPlugin pipPbpMangerPlugin;
        private Dictionary<string, Dictionary<string, string>> getstr;
        private DisplayPropertiesPlugins displayPropertiesPlugin;
        private List<(MonitorInfo_complex, MonitorInfo)> _AllInfoMonitors_mix = new List<(MonitorInfo_complex, MonitorInfo)>();

        private MonitorInfo_complex monitorInfoComplex1 = new MonitorInfo_complex()
        {
            //UnDefinedColorPreset,
            //ColorPresentDescription,
            //CapabilityDic,
            CapabilityDic = new Dictionary<string, List<string>>()
            {
                { "14", new List<string> { "value1" } },
                { "12", new List<string> { "value2" } }
            },
            AliasDeviceName = "Dell U2724DE (HDMI)",
            Handle = 0x0000000000000000,
            IsDellMonitor = true,
            Index = 0,
            CapabilityString = "(prot(monitor)type(LCD)model(U2724DE)cmds(01 02 03 07 0C E3 F3)vcp(02 04 05 08 10 12 14(01 04 05 06 08 09 0B 0C) 16 18 1A 52 60(19 0F 11 ) 66(0F02) 67 68 87 AA(00 01 02 04 )",
            hMonitor = 0x0000000000e315fe,
            hPhysicalMonitor = 0x0000000000000000,
            szPhysicalMonitorDescription = "Dell U2724DE (HDMI)",
            DisplayName = "\\\\.\\DISPLAY2",
            DDCisON = true,
            DDCCIFail = 0,
            edid = new EDID() { SerialNumber = "808597589", ModelName = "DELLU2724DE" },
            //DISPLAY_DEVICE displaydevice,
            //pMonitorInfoEx,
            //pDevmode,
            //ColorPresetSupportList,
            FwVersion = "M3T101",
            inputSource = "HDMI-1",
            //pathInfoTarget,
            //SmartHDRSupportList,
            modelName = "U2724DE",
            series = "Dell UltraSharp (U) Series Monitors",
        };

        private MonitorInfo monitorInfo1 = new MonitorInfo()
        {
            AliasDeviceName = "Dell U2724DE(HDMI)",
            IsDellMonitor = true,
            Index = 0,
            CapabilityString = "(prot(monitor)type(LCD)model(U2424H)cmds(01 02 03 07 0C E3 F3)vcp(02 04 05 08 10 12 14(01 04 05 06 08 09 0B 0C)E5 E7(02 03) E2(00 02 04 0C 0D 0F)",
            DisplayName = "\\\\.\\DISPLAY2",
            DDCisON = true,
            FwVersion = "M3T101",
            inputSource = "HDMI-1",
            modelName = "U2724DE",
            series = "Dell UltraSharp (U) Series Monitors",
            edid = new EDID() { SerialNumber = "808597589", ModelName = "DELLU2724DE" },
            //CapabilityDic = capabilityDic;
        };

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

        private void TaskQueueExecutor_DoWork(object sender, DoWorkEventArgs e)
        {
            PrivateObject privatevcp = new PrivateObject(vcpCorePlugin);
            while (true)
            {
                TaskLockQueue<ParameterType> _taskQueue = (TaskLockQueue<ParameterType>)privatevcp.GetFieldOrProperty("_TaskQueue");
                ResultLockPool _TaskQueueResult = (ResultLockPool)privatevcp.GetFieldOrProperty("_TaskQueueResult");
                if (_taskQueue.IsEmpty())
                { continue; }
                ParameterType p = _taskQueue.Dequeue();

                switch (p.CommandType)
                {
                    case Queue_CommandType.GetCapabilitiesString:
                        {
                            Type_GetCapabilitiesString parameter = (Type_GetCapabilitiesString)p.Parameter;
                            var rt = "type(LCD)model(U2424H)cmds(01 02 03 07 0C E3 F3)vcp(02 04)";
                            _TaskQueueResult.Add(parameter.guid, rt);
                        }
                        break;

                    case Queue_CommandType.GetVCPCapabilities:
                        {
                            Type_GetVCPCapabilities parameter = (Type_GetVCPCapabilities)p.Parameter;
                            var rt = "DELLU2724DE 808597589";
                            _TaskQueueResult.Add(parameter.guid, rt);
                        }
                        break;

                    case Queue_CommandType.GetVCPCapability_I:
                        {
                            Type_GetVCPCapability_I parameter = (Type_GetVCPCapability_I)p.Parameter;
                            var rt = 12;
                            _TaskQueueResult.Add(parameter.guid, rt);
                        }
                        break;

                    case Queue_CommandType.GetVCPCapability_II:
                        {
                            Type_GetVCPCapability_II parameter = (Type_GetVCPCapability_II)p.Parameter;
                            var rt = 20;
                            _TaskQueueResult.Add(parameter.guid, rt);
                        }
                        break;

                    case Queue_CommandType.SetVCPCapability_I:
                        {
                            Type_SetVCPCapability_I parameter = (Type_SetVCPCapability_I)p.Parameter;
                            var rt = true;
                            _TaskQueueResult.Add(parameter.guid, rt);
                        }
                        break;

                    case Queue_CommandType.SetVCPCapability_II:
                        {
                            Type_SetVCPCapability_II parameter = (Type_SetVCPCapability_II)p.Parameter;
                            var rt = true;
                            _TaskQueueResult.Add(parameter.guid, rt);
                        }
                        break;

                    default:
                        break;
                }
            }
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
            string displayName = monitorInfo1.DisplayName;
            string modelName = monitorInfo1.modelName;
            _AllInfoMonitors_mix.Add((monitorInfoComplex1, monitorInfo1));
            PrivateObject privatevcp = new PrivateObject(vcpCorePlugin);
            privatevcp.SetFieldOrProperty("_AllInfoMonitors_Mix", _AllInfoMonitors_mix);
            privatevcp.SetFieldOrProperty("_Isinitializing", false);
            var getMonitors = vcpCorePlugin.GetMonitors().Result;
            Assert.Greater(getMonitors.Count, 0);
            Assert.That(displayName, Is.EqualTo(getMonitors[0].DisplayName));
            Assert.That(modelName, Is.EqualTo(getMonitors[0].modelName));
        }

        [Test]
        public void TestRe_GetMonitors()
        {
            List<MonitorInfo> _allDisplays = new List<MonitorInfo>();
            _AllInfoMonitors_mix.Add((monitorInfoComplex1, monitorInfo1));
            PrivateObject privatevcp = new PrivateObject(vcpCorePlugin);
            privatevcp.SetFieldOrProperty("_AllInfoMonitors_Mix", _AllInfoMonitors_mix);
            var getMonitors = vcpCorePlugin.Re_GetMonitors(new CancellationTokenSource().Token).Result;
            Assert.IsNotNull(getMonitors);
        }

        [Test]
        public void TestGetCapabilitiesString()
        {
            string capabilitiesString = string.Empty;
            monitorInfo = null;
            if (monitorInfo == null)
            {
                var getCapabilitiesString = vcpCorePlugin.GetCapabilitiesString(monitorInfo1).Result;
                Assert.That(capabilitiesString, Is.EqualTo(getCapabilitiesString));
            }
            if (monitorInfo1 != null)
            {
                List<MonitorInfo_complex> _allInfoMonitors = new List<MonitorInfo_complex>();
                _allInfoMonitors.Add(monitorInfoComplex1);
                PrivateObject privatevcp = new PrivateObject(vcpCorePlugin);
                privatevcp.SetFieldOrProperty("_AllInfoMonitors", _allInfoMonitors);

                _AllInfoMonitors_mix.Add((monitorInfoComplex1, monitorInfo1));
                privatevcp.SetFieldOrProperty("_AllInfoMonitors_Mix", _AllInfoMonitors_mix);
                string capabilitiesStrings = "type(LCD)model(U2424H)cmds(01 02 03 07 0C E3 F3)vcp(02 04)";

                _taskQueueExecutor.DoWork += TaskQueueExecutor_DoWork;
                privatevcp.SetFieldOrProperty("_TaskQueueExecutor", _taskQueueExecutor);

                var getCapabilitiesString2 = vcpCorePlugin.GetCapabilitiesString(monitorInfo1).Result;
                Assert.Greater(getCapabilitiesString2.Length, 0);
                Assert.That(capabilitiesStrings, Is.EqualTo(getCapabilitiesString2));
            }
        }

        [Test]
        public void TestGetVCPCapabilities()
        {
            var VCPCapabilities2 = "DELLU2724DE 808597589";
            List<MonitorInfo_complex> _allInfoMonitors = new List<MonitorInfo_complex>();
            _allInfoMonitors.Add(monitorInfoComplex1);
            PrivateObject privatevcp = new PrivateObject(vcpCorePlugin);
            privatevcp.SetFieldOrProperty("_AllInfoMonitors", _allInfoMonitors);

            List<(MonitorInfo_complex, MonitorInfo)> _AllInfoMonitors_mix1 = new List<(MonitorInfo_complex, MonitorInfo)>();
            _AllInfoMonitors_mix1.Add((monitorInfoComplex1, monitorInfo1));
            privatevcp.SetFieldOrProperty("_AllInfoMonitors_Mix", _AllInfoMonitors_mix1);

            _taskQueueExecutor.DoWork += TaskQueueExecutor_DoWork;
            privatevcp.SetFieldOrProperty("_TaskQueueExecutor", _taskQueueExecutor);

            var getVCPCapabilities2 = vcpCorePlugin.GetVCPCapabilities(monitorInfo1).Result;
            Assert.That(VCPCapabilities2, Is.EqualTo(getVCPCapabilities2));
        }

        [Test]
        public void TestGetVCPCapability_()
        {
            bool objGetVCP = false;
            monitorInfo = null;
            byte code = 20;

            int value = 12;
            bool getvcpresult = true;
            List<MonitorInfo_complex> _allInfoMonitors = new List<MonitorInfo_complex>();
            _allInfoMonitors.Add(monitorInfoComplex1);
            PrivateObject privatevcp = new PrivateObject(vcpCorePlugin);
            privatevcp.SetFieldOrProperty("_AllInfoMonitors", _allInfoMonitors);

            List<(MonitorInfo_complex, MonitorInfo)> _AllInfoMonitors_mix1 = new List<(MonitorInfo_complex, MonitorInfo)>();
            _AllInfoMonitors_mix1.Add((monitorInfoComplex1, monitorInfo1));
            privatevcp.SetFieldOrProperty("_AllInfoMonitors_Mix", _AllInfoMonitors_mix1);

            _taskQueueExecutor.DoWork += TaskQueueExecutor_DoWork;
            privatevcp.SetFieldOrProperty("_TaskQueueExecutor", _taskQueueExecutor);

            var GetVCPCapabilityresult2 = vcpCorePlugin.GetVCPCapability(monitorInfo1, code).Result;// ColorSpace14 = 20, get value=12
            Assert.That(getvcpresult, Is.EqualTo(GetVCPCapabilityresult2.result));
            Assert.That(value, Is.EqualTo(GetVCPCapabilityresult2.value));
        }

        [Test]
        public void TestGetVCPCapability()
        {
            bool objGetVCP = false;
            monitorInfo = null;
            string funcName = "226"; //ColorSpaceE2 = 226,
            int value = 20;
            bool getVCPCapabilityResult = true;

            List<MonitorInfo_complex> _allInfoMonitors = new List<MonitorInfo_complex>();
            _allInfoMonitors.Add(monitorInfoComplex1);
            PrivateObject privatevcp = new PrivateObject(vcpCorePlugin);
            privatevcp.SetFieldOrProperty("_AllInfoMonitors", _allInfoMonitors);

            List<(MonitorInfo_complex, MonitorInfo)> _AllInfoMonitors_mix1 = new List<(MonitorInfo_complex, MonitorInfo)>();
            _AllInfoMonitors_mix1.Add((monitorInfoComplex1, monitorInfo1));
            privatevcp.SetFieldOrProperty("_AllInfoMonitors_Mix", _AllInfoMonitors_mix1);
            bool getvcpresult = true;

            _taskQueueExecutor.DoWork += TaskQueueExecutor_DoWork;
            privatevcp.SetFieldOrProperty("_TaskQueueExecutor", _taskQueueExecutor);

            var GetVCPCapabilityresult2 = vcpCorePlugin.GetVCPCapability(monitorInfo1, funcName).Result;
            Assert.That(getVCPCapabilityResult, Is.EqualTo(GetVCPCapabilityresult2.result));
            Assert.That(value, Is.EqualTo(GetVCPCapabilityresult2.value));
        }

        [Test]
        public void TestSetVCPCapability()
        {
            bool objGetVCP = false;
            monitorInfo = null;
            string funcName = "226"; //ColorSpaceE2 = 226,
            int value = 30;
            bool setVCPCapabilityResult = true;
            uint newContrast = 0x50;
            List<MonitorInfo_complex> _allInfoMonitors = new List<MonitorInfo_complex>();
            _allInfoMonitors.Add(monitorInfoComplex1);
            PrivateObject privatevcp = new PrivateObject(vcpCorePlugin);
            privatevcp.SetFieldOrProperty("_AllInfoMonitors", _allInfoMonitors);

            List<(MonitorInfo_complex, MonitorInfo)> _AllInfoMonitors_mix1 = new List<(MonitorInfo_complex, MonitorInfo)>();
            _AllInfoMonitors_mix1.Add((monitorInfoComplex1, monitorInfo1));
            privatevcp.SetFieldOrProperty("_AllInfoMonitors_Mix", _AllInfoMonitors_mix1);

            _taskQueueExecutor.DoWork += TaskQueueExecutor_DoWork;
            privatevcp.SetFieldOrProperty("_TaskQueueExecutor", _taskQueueExecutor);

            bool SetVCPCapabilityResult2 = vcpCorePlugin.SetVCPCapability(monitorInfo1, 18, newContrast).Result;
            Assert.IsTrue(SetVCPCapabilityResult2);
            Assert.That(setVCPCapabilityResult, Is.EqualTo(SetVCPCapabilityResult2));
        }

        [Test]
        public void TestSetVCPCapability_()
        {
            ObjGetVCP objGetVCP = new ObjGetVCP() { value = null, result = false };
            monitorInfo = null;
            string FunctionName = "colorpreset";
            string val = "Warm";
            bool SetVCPCapabilityResult_ = true;
            uint newContrast = 0x50;

            List<MonitorInfo_complex> _allInfoMonitors = new List<MonitorInfo_complex>();
            _allInfoMonitors.Add(monitorInfoComplex1);
            PrivateObject privatevcp = new PrivateObject(vcpCorePlugin);
            privatevcp.SetFieldOrProperty("_AllInfoMonitors", _allInfoMonitors);

            List<(MonitorInfo_complex, MonitorInfo)> _AllInfoMonitors_mix1 = new List<(MonitorInfo_complex, MonitorInfo)>();
            _AllInfoMonitors_mix1.Add((monitorInfoComplex1, monitorInfo1));
            privatevcp.SetFieldOrProperty("_AllInfoMonitors_Mix", _AllInfoMonitors_mix1);

            _taskQueueExecutor.DoWork += TaskQueueExecutor_DoWork;
            privatevcp.SetFieldOrProperty("_TaskQueueExecutor", _taskQueueExecutor);

            bool setColor = vcpCorePlugin.SetVCPCapability(monitorInfo1, "colorpreset", "Warm").Result;
            Assert.IsTrue(setColor);
            Assert.That(SetVCPCapabilityResult_, Is.EqualTo(setColor));
        }

        [Test]
        public void TestSetColorPreset()
        {
            //var getMonitors = vcpCorePlugin.GetMonitors().Result;
            //Assert.Greater(getMonitors.Count, 0);
            //var monitorInfo = getMonitors[0];
            //PrivateObject privatevcp = new PrivateObject(vcpCorePlugin);
            //List<(MonitorInfo_complex, MonitorInfo)> AllInfoMonitors_Mix = (List<(MonitorInfo_complex, MonitorInfo)>)privatevcp.GetField("_AllInfoMonitors_Mix");
            ////var AllInfoMonitors= _AllInfoMonitors.
            //foreach (var moX in AllInfoMonitors_Mix)
            //{
            //    bool setColor = vcpCorePlugin.SetColorPreset(moX.Item1, "Warm");//SetVCPCapability_ FunctionName.ToLower() case "colorpreset":
            //    Assert.IsTrue(setColor);
            //}
            //var getMonitorColor = vcpCorePlugin.GetVCPCapability(monitorInfo, 20, 0).Result.value;//VcpCode : byte , VcpCode.ColorSpace14 = 20,
            //uint uMonitorColor = System.Convert.ToUInt32(getMonitorColor.ToString());// convert to uint
            //string getMointorColorToStr = uMonitorColor.ToString("X2");    //change to 16 hex, and two number show
            //var getVcpCoreColor = getstr["14"].Where(x => x.Value.Equals(getMointorColorToStr)).FirstOrDefault().Value;
            //Assert.That(actual: getVcpCoreColor, Is.EqualTo(getMointorColorToStr));
            //foreach (var moX in AllInfoMonitors_Mix)
            //{
            //    bool setColor = vcpCorePlugin.SetColorPreset(moX.Item1, "Custom Color");//SetVCPCapability_ FunctionName.ToLower() case "colorpreset":
            //    Assert.IsTrue(setColor);
            //}
            //var reGetMonitorColor = vcpCorePlugin.GetVCPCapability(monitorInfo, 20, 0).Result.value;
            //uint urMonitorColor = System.Convert.ToUInt32(reGetMonitorColor.ToString());
            //string regetMointorColorToStr = urMonitorColor.ToString("X2");
            //var reGetVcpCoreColor = getstr["14"].Where(x => x.Value.Equals(regetMointorColorToStr)).FirstOrDefault().Value;
            //Assert.That(actual: reGetVcpCoreColor, Is.EqualTo(regetMointorColorToStr));

            MonitorInfo_complex monitorInfoComplex2 = new MonitorInfo_complex()
            {
                //UnDefinedColorPreset,
                //ColorPresentDescription,
                //CapabilityDic,
                AliasDeviceName = "Dell U2725DE (HDMI)",
                Handle = 0x0000000000000000,
                IsDellMonitor = true,
                Index = 0,
                CapabilityString = "(prot(monitor)type(LCD)model(U2724DE)cmds(01 02 03 07 0C E3 F3)vcp(02 04 05 08 10 12 14(01 04 05 06 08 09 0B 0C) 16 18 1A 52 60(19 0F 11 ) 66(0F02) 67 68 87 AA(00 01 02 04 )",
                //hMonitor = 0x0000000000e315fe,
                //hPhysicalMonitor = 0x0000000000000000,
                hMonitor = 0x0000000000e415ab,
                hPhysicalMonitor = 0x0000000000000123,
                szPhysicalMonitorDescription = "Dell U2725DE (HDMI)",
                DisplayName = "\\\\.\\DISPLAY2",
                DDCisON = true,
                DDCCIFail = 0,
                edid = new EDID() { SerialNumber = "808597599", ModelName = "DELLU2725DE" },
                //DISPLAY_DEVICE displaydevice,
                //pMonitorInfoEx,
                //pDevmode,
                //ColorPresetSupportList,
                FwVersion = "M3T101",
                inputSource = "HDMI-1",
                //pathInfoTarget,
                //SmartHDRSupportList,
                modelName = "U2725DE",
                series = "Dell UltraSharp (U) Series Monitors",
            };

            monitorInfoComplex2.ColorPresentDescription = getstr;
            List<MonitorInfo_complex> _allInfoMonitors = new List<MonitorInfo_complex>();
            _allInfoMonitors.Add(monitorInfoComplex2);
            PrivateObject privatevcp = new PrivateObject(vcpCorePlugin);
            privatevcp.SetFieldOrProperty("_AllInfoMonitors", _allInfoMonitors);

            _AllInfoMonitors_mix.Add((monitorInfoComplex2, monitorInfo1));
            privatevcp.SetFieldOrProperty("_AllInfoMonitors_Mix", _AllInfoMonitors_mix);

            Dictionary<string, string> VCPE2 = new Dictionary<string, string>();

            bool setColor = vcpCorePlugin.SetColorPreset(monitorInfoComplex2, "Warm"); //SetVCPFeature call dxva2.dll
            Assert.IsFalse(setColor);
        }
    }
}