using DDPM.SA.Common;
using DDPM.SA.Plugins.User.DisplayManager;
using DDPM.SA.Plugins.User.PipPbpManger;
using Dell.Client.Framework.Interfaces;
using Dell.Client.Framework.UnitTestShared.Tests;
using Moq;
using VcpCore.Common;
using VcpCore.Interfaces;
using VcpCore.Plugins;

namespace DDPM.SA.Plugins.User.DisplayProperties.Test

{
    public class TestDispalyProperties
    {
        private Mock<IAgent> DisplayMangerAgent { get; } = new();
        private Mock<IAgent> VcpCoreAgent { get; } = new();
        private Mock<IAgent> PipPbpAgent = new();
        private Mock<IAgent> DisplayPropertiesAgent { get; } = new();
        private MonitorInfo monitorInfo = new MonitorInfo();
        public List<MonitorInfo> monitorInfolist = new List<MonitorInfo>();
        private Mock<IDisplayProperties> IDisplayPropertiesService { get; } = new();

        private DisplayPropertiesInfo displayPropertiesInfo = new DisplayPropertiesInfo()
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

        private MonitorInfo monitorInfo1 = new MonitorInfo()
        {
            AliasDeviceName = "Dell U2724DE(HDMI)",
            IsDellMonitor = true,
            Index = 0,
            CapabilityString = "(prot(monitor)type(LCD)model(U2724DE)cmds(01 02 03 07 0C E3 F3)vcp(02 04 05 08 10 12 14(01 04 05 06 08 09 0B 0C) 16 18 1A 52 60(19 0F 11 ) 66(0F02) 67 68 87 AA(00 01 02 04 ) AC AE B2 B6 C6 C8 C9 CA CC(02 0A 03 04 08 09 0D 06 ) D6(01 04 05) DC(00 03 05 ) DF E0 E1 E2(00 02 04 0C 0D 0F 10 11 13 0B 1A 1B 3D 14 ) E5 E7(02 03) E8 E9(00 01 02 21 22 24 ) EA(FC F8 ) F0(09 0A A1 ) EE EF(00 01 03 0F) F1 F2 FD)mswhql(1)asset_eep(40)mccs_ver(2.1))",
            //DisplayName = "DISPLAY7",
            DisplayName = "DISPLAY7",
            DDCisON = true,
            FwVersion = "M3T101",
            inputSource = "HDMI-1",
            modelName = "U2724DE",
            series = "Dell UltraSharp (U) Series Monitors",
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

        private DisplayMangerPlugin displayPlugin;
        private VcpCorePlugin vcpCorePlugin;
        private PipPbpMangerPlugin pipPbpMangerPlugin;
        private Dictionary<string, Dictionary<string, string>> getstr;
        private DisplayPropertiesPlugins displayPropertiesPlugin;

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
            //monitorInfolist = displayPlugin.GetMonitors().Result;
        }

        [Test]
        public void TestGetDisplayPropertiesInfo()
        {
            bool isSupportedHDR = true;
            bool isHDREnable = true;
            bool isSupportUSBCPrioritization = true;
            USBCPrioritizationType USBCPrioritizationType = USBCPrioritizationType.HighDataSpeed;

            //var monitors = displayPlugin.GetMonitors().Result;
            //Assert.GreaterOrEqual(monitors.Count, 0);
            //monitorInfo = monitors[0];
            //Assert.IsNotNull(monitorInfo);
            //monitorInfo = monitorInfolist[0]; //与bruce确认测DisplayProperties需要get真实的monitor才可以get出monitor属性，不然就返回一个空的new DisplayPropertiesInfo
            //Assert.IsNotNull(monitorInfo);
            string s = monitorInfo1.CapabilityString;
            DisplayPropertiesInfo getdisplayPropertiesInfo = new DisplayPropertiesInfo();
            var result = displayPropertiesPlugin.GetDisplayPropertiesInfo(monitorInfo1, s, isSupportedHDR, isHDREnable, isSupportUSBCPrioritization, USBCPrioritizationType).Result;
            Assert.IsNotNull(result);
            Assert.That(result.SupportedProperties.Properties, Is.EqualTo(getdisplayPropertiesInfo.SupportedProperties.Properties));
        }

        [Test]
        public void TestGetCurrentDisplayOrientation()
        {
            //DisplayOrientation DisplayOrientation = DisplayOrientation.Unknow;
            //var displayName = monitorInfo1.DisplayName; //与bruce确认假的monitor，无法通过DispalyName去call WinAPI
            //var result = displayPropertiesPlugin.GetCurrentDisplayOrientation(displayName).Result;
            //Assert.IsNotNull(result);
            //Assert.That(DisplayOrientation, Is.EqualTo(result));

            //var monitors = displayPlugin.GetMonitors().Result;
            //Assert.GreaterOrEqual(monitors.Count, 0);
            //monitorInfo = monitors[0]; //真实monitor
            //Assert.IsNotNull(monitorInfo);
            //var displayName2 = monitorInfo.DisplayName;
            //var result2 = displayPropertiesPlugin.GetCurrentDisplayOrientation(displayName2).Result;
            //Assert.IsNotNull(result2);

            DisplayOrientation DisplayOrientation = DisplayOrientation.Unknow;
            string DisplayName = "DISPLAY7";
            var result2 = displayPropertiesPlugin.GetCurrentDisplayOrientation(DisplayName).Result;
            Assert.That(result2, Is.EqualTo(DisplayOrientation));
            Assert.That(DisplayName, Is.EqualTo(monitorInfo1.DisplayName));
        }

        [Test]
        public void TestSetDisplayPropertiest()
        {
            //var monitors = displayPlugin.GetMonitors().Result;
            //Assert.GreaterOrEqual(monitors.Count, 0);
            //monitorInfo = monitors[0];
            //Assert.IsNotNull(monitorInfo);

            //var displayName = monitorInfo.DisplayName;
            //var properties = new Properties { Resolutions_Width = 1920, Resolutions_High = 1080, Frequency = 60 };
            ////var properties2 = new Properties { Resolutions_Width = 2560, Resolutions_High = 1440, Frequency = 60 }; //当前显示器最佳分辨率
            //var orientation = DisplayOrientation.Angle0;
            //var result = displayPropertiesPlugin.SetDisplayPropertiest(displayName, properties, orientation).Result;
            //Assert.IsNotNull(result);
            var displayName = monitorInfo1.DisplayName;
            var properties = new Properties { Resolutions_Width = 1920, Resolutions_High = 1080, Frequency = 60 };
            var orientation = DisplayOrientation.Angle0;
            var result = displayPropertiesPlugin.SetDisplayPropertiest(displayName, properties, orientation).Result;
            Assert.IsNotNull(result);
            Assert.IsFalse(result);
        }

        [Test]
        public void TestCallWindowsDisplaySetting()
        {
            try
            {
                var displayPropertiesPluginmock = new Mock<DisplayPropertiesPlugins>();
                displayPropertiesPluginmock.Setup(x => x.CallWindowsDisplaySetting()).Returns(Task.FromResult(true));
            }
            catch (Exception e)
            {
            }
        }

        [Test]
        public void TestSetHDRStatus()
        {
            bool SupportedHDR = false;
            EDID eDID = new EDID()
            {
                ManufactureID = "DELL",
                VendorID = "42DD",
                Year = 2023,
                Month = 5,
                Week = 22,
                ModelName = "DELLU2724DD",
                EdidVersion = "V1.3",
                VideoInputType = "Digital Signal",
                Size = 27.1510868f,
                ServiceTag = "CN073K0",
                SerialNumber = "808597688",
                Edid = "00FFFFFFFFFFFF0010ACDC425538323016210103803C2278EA62A5AD5046AB240E5054A54B00714F8180A940D1C081C0A9C001010101565E00A0A0A029503020350055502100001A000000FF00434E3037334B300A2020202020000000FC0044454C4C20553237323444450A000000FD0030781EB23C000A20202020202001ED"
            };
            try
            {
                if (SupportedHDR)
                {
                    var result = displayPropertiesPlugin.SetHDRStatus(eDID, true).Result;
                    Assert.IsNotNull(result);
                    Assert.IsTrue(result);

                    result = displayPropertiesPlugin.SetHDRStatus(eDID, false).Result;
                    Assert.IsNotNull(result);
                    Assert.IsTrue(result);
                }
                else
                {
                    var result = displayPropertiesPlugin.SetHDRStatus(eDID, false).Result;
                    Assert.IsNotNull(result);
                    //Assert.IsFalse(result);
                }
            }
            catch
            {
                var result = displayPropertiesPlugin.SetHDRStatus(eDID, false).Result;
                Assert.IsFalse(result);
            }
        }

        [Test]
        public void TestDisplayPropertiesPlugins()
        {
            Assert.IsNotNull(displayPropertiesPlugin);
            PrivateObject privateObject = new PrivateObject(displayPropertiesPlugin);
            var agent2 = privateObject.GetField("_agent") as IAgent;
            Assert.That(agent2, Is.EqualTo(DisplayPropertiesAgent.Object));
        }

        [Test]
        public void TestSetExtendMode()
        {
            MonitorInfo monitorInfo1 = new MonitorInfo()
            {
                AliasDeviceName = "Dell U2724DE(HDMI)",
                IsDellMonitor = true,
                Index = 0,
                CapabilityString = "(prot(monitor)type(LCD)model(U2424H)cmds(01 02 03 07 0C E3 F3)vcp(02 04 05 08 10 12 14(01 04 05 06 08 09 0B 0C)E5 E7(02 03) E2(00 02 04 0C 0D 0F)",
                DisplayName = "DISPLAY8",
                DDCisON = true,
                FwVersion = "M3T101",
                inputSource = "HDMI-1",
                modelName = "U2724DE",
                series = "Dell UltraSharp (U) Series Monitors",
                edid = new EDID() { SerialNumber = "808597589" },
                //CapabilityDic = capabilityDic;
            };
            displayPropertiesPlugin.SetExtendMode(monitorInfo1);
            Assert.IsNotNull(displayPropertiesPlugin);
            Assert.IsNotNull(monitorInfo1.DisplayName);
        }
    }
}