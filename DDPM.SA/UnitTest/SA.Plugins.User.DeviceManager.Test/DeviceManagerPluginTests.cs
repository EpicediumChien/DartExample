using DDPM.ColorApp;
using DDPM.MonitorBorker;
using DDPM.SA.Common;
using DDPM.SA.Common.Settings;
using DDPM.SA.Plugins.User.DeviceManager;
using Dell.Client.Framework.Interfaces;
using Dell.Client.Framework.UnitTestShared.Tests;
using Moq;
using VcpCore.Common;

namespace SA.Plugins.User.DeviceManager.Test
{
    [Apartment(ApartmentState.STA)]
    public class TestsDeviceManagerPlugin
    {
        private Mock<IAgent>? iAgentMock;
        private IAgent? iAgent;
#pragma warning disable NUnit1032 // An IDisposable field/property should be Disposed in a TearDown method
        private DeviceMangerPlugin? deviceMangerPlugin;
        private PrivateObject? privateObject;

        private MonitorInfo monitorInfo = new MonitorInfo()
        {
            AliasDeviceName = "Dell U2724DE(HDMI)",
            IsDellMonitor = true,
            Index = 0,
            CapabilityString = "(prot(monitor)type(LCD)model(U2724DE)cmds(01 02 03 07 0C E3 F3)vcp(02 04 05 08 10 12 14(01 04 05 06 08 09 0B 0C) 16 18 1A 52 60(19 0F 11 ) 66(0F02) 67 68 87 AA(00 01 02 04 ) AC AE B2 B6 C6 C8 C9 CA CC(02 0A 03 04 08 09 0D 06 ) D6(01 04 05) DC(00 03 05 ) DF E0 E1 E2(00 02 04 0C 0D 0F 10 11 13 0B 1A 1B 3D 14 ) E5 E7(02 03) E8 E9(00 01 02 21 22 24 ) EA(FC F8 ) F0(09 0A A1 ) EE EF(00 01 03 0F) F1 F2 FD)mswhql(1)asset_eep(40)mccs_ver(2.1))",
            //DisplayName = "DISPLAY7",
            DisplayName = "\\\\.\\DISPLAY1",
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

        [SetUp]
        public void Setup()
        {
            iAgentMock = new Mock<IAgent>();
            iAgent = iAgentMock.Object;
            deviceMangerPlugin = new DeviceMangerPlugin(iAgent);
            privateObject = new PrivateObject(deviceMangerPlugin);
        }

        [Test]
        public void TestConstructor_DeviceMangerPlugin()
        {
            Assert.That(deviceMangerPlugin, Is.Not.Null);
        }

        [Test]
        public void TestDownloadICCData()
        {
            //_ColorPresetPlugin == null
            var result = deviceMangerPlugin.DownloadICCData(monitorInfo, "");
            Assert.That(result, Is.Not.Null);

            //_ColorPresetPlugin != null
            var _ColorPresetPluginMock = new Mock<IColorPresetSA>();
            privateObject.SetFieldOrProperty("_ColorPresetPlugin", _ColorPresetPluginMock.Object);
            _ColorPresetPluginMock.Setup(x => x.DownloadICCData(It.IsAny<MonitorInfo>(), It.IsAny<string>())).Returns(Task.FromResult(new DDPM.SA.Common.IIC_Metadata()));
            result = deviceMangerPlugin.DownloadICCData(monitorInfo, "");
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void TestReadColorPreset()
        {
            //_SupportedColorPreset != null&& _SupportedColorPreset.Count == 0
            var result = deviceMangerPlugin.ReadColorPreset(monitorInfo).Result;
            Assert.That(result, Is.Not.Null);

            //_ColorPresetPlugin != null
            var _DisplayManagerPluginMock = new Mock<IDisplayService>();
            privateObject.SetFieldOrProperty("_DisplayManagerPlugin", _DisplayManagerPluginMock.Object);
            _DisplayManagerPluginMock.Setup(x => x.GetVCPCapabilities(It.IsAny<MonitorInfo>())).Returns(Task.FromResult("aa"));
            var _ColorPresetPluginMock = new Mock<IColorPresetSA>();
            privateObject.SetFieldOrProperty("_ColorPresetPlugin", _ColorPresetPluginMock.Object);
            _ColorPresetPluginMock.Setup(x => x.ReadColorPreset(It.IsAny<MonitorInfo>(), It.IsAny<string>())).Returns(Task.FromResult(new List<string>()));
            result = deviceMangerPlugin.ReadColorPreset(monitorInfo).Result;
            Assert.That(result, Is.Not.Null);

            //_SupportedColorPreset == null
            privateObject.SetFieldOrProperty("_SupportedColorPreset", null);
            result = deviceMangerPlugin.ReadColorPreset(monitorInfo).Result;
            Assert.That(result, Is.EqualTo(null));
        }

        //[Test]
        //public void TestGetMonitorProfile()
        //{
        //    var result = deviceMangerPlugin.GetMonitorProfile(monitorInfo).Result;
        //    Assert.That(result, Is.Not.Null);
        //}

        //[Test]
        ////lack "Dell_U3224KB_Native_v2.icm"
        //public void TestSetMonitorProfile()
        //{
        //    var result = deviceMangerPlugin.SetMonitorProfile(monitorInfo, "Standard").Result;

        //    //Assert.That(result, Is.Not.Null);
        //}

        [Test]
        public void TestReadCurrentColorPreset()
        {
            //_DisplayManagerPlugin == null
            var result = deviceMangerPlugin.ReadCurrentColorPreset(monitorInfo).Result;
            Assert.That(result, Is.EqualTo(""));

            //_DisplayManagerPlugin != null&&result.result==true
            var _DisplayManagerPluginMock = new Mock<IDisplayService>();
            var _DisplayManagerPlugin = _DisplayManagerPluginMock.Object;
            _DisplayManagerPluginMock.Setup(x => x.GetVCPCapability(It.IsAny<MonitorInfo>(), It.IsAny<string>(), It.IsAny<int>())).Returns(Task.FromResult(new ObjGetVCP() { result = true, value = "_DisplayManagerPlugin" }));
            privateObject.SetFieldOrProperty("_DisplayManagerPlugin", _DisplayManagerPlugin);
            result = deviceMangerPlugin.ReadCurrentColorPreset(monitorInfo).Result;
            Assert.That(result, Is.EqualTo("_DisplayManagerPlugin"));

            //_DisplayManagerPlugin != null&&result.result==false
            _DisplayManagerPluginMock.Setup(x => x.GetVCPCapability(It.IsAny<MonitorInfo>(), It.IsAny<string>(), It.IsAny<int>())).Returns(Task.FromResult(new ObjGetVCP() { result = false }));
            privateObject.SetFieldOrProperty("_DisplayManagerPlugin", _DisplayManagerPlugin);
            result = deviceMangerPlugin.ReadCurrentColorPreset(monitorInfo).Result;
            Assert.That(result, Is.EqualTo(""));
        }

        [Test]
        public void TestNotify_refresh_app_list()
        {
            //MonitorBorkerWin == null
            var result = deviceMangerPlugin.Notify_refresh_app_list().Result;
            Assert.That(result, Is.EqualTo(true));

            //MonitorBorkerWin != null
            var DeviceManagerSAMock = new Mock<IDeviceManagerSA>();
            var DeviceManagerSA = DeviceManagerSAMock.Object;
            var monitorBorkerWin = new MainWindow(DeviceManagerSA, monitorInfo);
            var MainWindow = new MainWindow(DeviceManagerSA, monitorInfo);
            PrivateObject privateObjecta = new PrivateObject(MainWindow);
            privateObjecta.SetFieldOrProperty("ColorPresetWin", new MonitorWin(DeviceManagerSA, monitorInfo));
            privateObject.SetFieldOrProperty("MonitorBorkerWin", monitorBorkerWin);
            result = deviceMangerPlugin.Notify_refresh_app_list().Result;
            Assert.That(result, Is.EqualTo(true));
        }

        [Test]
        public void TestWriteColorPreset()
        {
            //_ColorPresetPlugin == null
            var result = deviceMangerPlugin.WriteColorPreset(monitorInfo, "").Result;
            Assert.That(result, Is.EqualTo(false));

            //_ColorPresetPlugin != null
            var _ColorPresetPluginMock = new Mock<IColorPresetSA>();
            var _ColorPresetPlugin = _ColorPresetPluginMock.Object;
            _ColorPresetPluginMock.Setup(x => x.WriteColorPreset(It.IsAny<MonitorInfo>(), It.IsAny<string>(), It.IsAny<ISettingsManagerDev>(), It.IsAny<int>())).Returns(Task.FromResult(true));
            //_ColorPresetPluginMock.Setup(x => x.WriteColorPreset(It.IsAny<MonitorInfo>(), It.IsAny<string>(), It.IsAny<List<ColorPresetSettings>>())).Returns(Task.FromResult(new List<ColorPresetSettings>()));
            privateObject.SetFieldOrProperty("_ColorPresetPlugin", _ColorPresetPlugin);
            var _SettingsPluginMock = new Mock<ISettingsManagerDev>();
            var _SettingsPlugin = _SettingsPluginMock.Object;
            _SettingsPluginMock.Setup(x => x.WriteColorPresetSettings(It.IsAny<List<ColorPresetSettings>>())).Returns(Task.FromResult(true));
            privateObject.SetFieldOrProperty("_SettingsPlugin", _SettingsPlugin);
            var _DisplayManagerPluginMock = new Mock<IDisplayService>();
            var _DisplayManagerPlugin = _DisplayManagerPluginMock.Object;
            _DisplayManagerPluginMock.Setup(x => x.SetVCPCapability(It.IsAny<MonitorInfo>(), It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult(true));
            privateObject.SetFieldOrProperty("_DisplayManagerPlugin", _DisplayManagerPlugin);
            int colorPresetRunType = 0;
            string colorPreset_Name = "Game";
            int colorPresetRunType2 = (int)ColorPresetRunType.Auto;
            if (colorPresetRunType == 0)
            {
                result = deviceMangerPlugin.WriteColorPreset(monitorInfo, colorPreset_Name, colorPresetRunType).Result;
                Assert.That(result, Is.EqualTo(true));
            }

            if (colorPresetRunType2 == (int)ColorPresetRunType.Auto)
            {
                var result2 = deviceMangerPlugin.WriteColorPreset(monitorInfo, colorPreset_Name, colorPresetRunType2).Result;
                Assert.That(result2, Is.EqualTo(true));
            }
        }

        [Test]
        public void TestWriteColorPreset_AUTO()
        {
            //_ColorPresetPlugin == null
            var result = deviceMangerPlugin.WriteColorPreset_AUTO(monitorInfo, "").Result;
            Assert.That(result, Is.EqualTo(false));

            //_ColorPresetPlugin != null
            var _ColorPresetPluginMock = new Mock<IColorPresetSA>();
            var _ColorPresetPlugin = _ColorPresetPluginMock.Object;
            //_ColorPresetPluginMock.Setup(x => x.WriteColorPreset_AUTO(It.IsAny<MonitorInfo>(), It.IsAny<string>(), It.IsAny<List<ColorPresetSettings>>())).Returns(Task.FromResult(new List<ColorPresetSettings>()));
            privateObject.SetFieldOrProperty("_ColorPresetPlugin", _ColorPresetPlugin);
            //var _SettingsPluginMock = new Mock<ISettingsManagerDev>();
            //var _SettingsPlugin = _SettingsPluginMock.Object;
            //_SettingsPluginMock.Setup(x => x.WriteColorPresetSettings(It.IsAny<List<ColorPresetSettings>>())).Returns(Task.FromResult(true));
            //privateObject.SetFieldOrProperty("_SettingsPlugin", _SettingsPlugin);
            var _DisplayManagerPluginMock = new Mock<IDisplayService>();
            var _DisplayManagerPlugin = _DisplayManagerPluginMock.Object;
            _DisplayManagerPluginMock.Setup(x => x.SetVCPCapability(It.IsAny<MonitorInfo>(), It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult(true));
            privateObject.SetFieldOrProperty("_DisplayManagerPlugin", _DisplayManagerPlugin);
            result = deviceMangerPlugin.WriteColorPreset_AUTO(monitorInfo, "12").Result;
            Assert.That(result, Is.EqualTo(true));
        }

        [Test]
        public void TestWriteColorPresetByColorProfile()
        {
            //_ColorPresetPlugin == null
            var result = deviceMangerPlugin.WriteColorPresetByColorProfile(monitorInfo, "").Result;
            Assert.That(result, Is.EqualTo(false));

            //_ColorPresetPlugin != null
            var _ColorPresetPluginMock = new Mock<IColorPresetSA>();
            var _ColorPresetPlugin = _ColorPresetPluginMock.Object;
            //_ColorPresetPluginMock.Setup(x => x.WriteColorPreset_AUTO(It.IsAny<MonitorInfo>(), It.IsAny<string>(), It.IsAny<List<ColorPresetSettings>>())).Returns(Task.FromResult(new List<ColorPresetSettings>()));
            privateObject.SetFieldOrProperty("_ColorPresetPlugin", _ColorPresetPlugin);
            var _SettingsPluginMock = new Mock<ISettingsManagerDev>();
            var _SettingsPlugin = _SettingsPluginMock.Object;
            _SettingsPluginMock.Setup(x => x.WriteColorPresetSettings(It.IsAny<List<ColorPresetSettings>>())).Returns(Task.FromResult(true));
            privateObject.SetFieldOrProperty("_SettingsPlugin", _SettingsPlugin);
            var _DisplayManagerPluginMock = new Mock<IDisplayService>();
            var _DisplayManagerPlugin = _DisplayManagerPluginMock.Object;
            _DisplayManagerPluginMock.Setup(x => x.SetVCPCapability(It.IsAny<MonitorInfo>(), It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult(true));
            privateObject.SetFieldOrProperty("_DisplayManagerPlugin", _DisplayManagerPlugin);
            string colorProfile_Name1 = "Dell_U3224KB_Native_v2.icm";
            if (colorProfile_Name1 == "Dell_U3224KB_Native_v2.icm")
            {
                result = deviceMangerPlugin.WriteColorPresetByColorProfile(monitorInfo, colorProfile_Name1).Result;
                Assert.That(result, Is.EqualTo(false));
            }

            string colorProfile_Name2 = "Dell_U3224KB_DisplayP3_v2.icm";
            if (colorProfile_Name2 == "Dell_U3224KB_DisplayP3_v2.icm")
            {
                result = deviceMangerPlugin.WriteColorPresetByColorProfile(monitorInfo, colorProfile_Name2).Result;
                Assert.That(result, Is.EqualTo(false));
            }

            string colorProfile_Name3 = "Dell_U3224KB_DCIP3_v2.icm";
            if (colorProfile_Name3 == "Dell_U3224KB_DCIP3_v2.icm")
            {
                result = deviceMangerPlugin.WriteColorPresetByColorProfile(monitorInfo, colorProfile_Name3).Result;
                Assert.That(result, Is.EqualTo(false));
            }

            string colorProfile_Name4 = "Dell_U3224KB_sRGB_v2.icm";
            if (colorProfile_Name4 == "Dell_U3224KB_sRGB_v2.icm")
            {
                result = deviceMangerPlugin.WriteColorPresetByColorProfile(monitorInfo, colorProfile_Name4).Result;
                Assert.That(result, Is.EqualTo(false));
            }

            string colorProfile_Name5 = "Dell_U3224KB_Rec709_v2.icm";
            if (colorProfile_Name5 == "Dell_U3224KB_Rec709_v2.icm")
            {
                result = deviceMangerPlugin.WriteColorPresetByColorProfile(monitorInfo, colorProfile_Name5).Result;
                Assert.That(result, Is.EqualTo(false));
            }

            string colorProfile_Name6 = "Dell_U3224KB_HDR_v4_MHC2.icm";
            if (colorProfile_Name6 == "Dell_U3224KB_HDR_v4_MHC2.icm")
            {
                result = deviceMangerPlugin.WriteColorPresetByColorProfile(monitorInfo, colorProfile_Name6).Result;
                Assert.That(result, Is.EqualTo(false));
            }
        }

        [Test]
        public void TestFindAppsbyShell()
        {
            //_AllAppData.Count == 0 || isReload == true
            //_ColorPresetPlugin == null
            var result = deviceMangerPlugin.FindAppsbyShell(true).Result;
            Assert.That(result.Count, Is.EqualTo(0));

            //_ColorPresetPlugin != null
            var _ColorPresetPluginMock = new Mock<IColorPresetSA>();
            var _ColorPresetPlugin = _ColorPresetPluginMock.Object;
            _ColorPresetPluginMock.Setup(x => x.GetInstalledAppsList(It.IsAny<bool>())).Returns(Task.FromResult(new Dictionary<string, InstalledAppInfo>() { { "a", new InstalledAppInfo() }, { "b", new InstalledAppInfo() } }));
            privateObject.SetFieldOrProperty("_ColorPresetPlugin", _ColorPresetPlugin);
            var _SettingsPluginMock = new Mock<ISettingsManagerDev>();
            var _SettingsPlugin = _SettingsPluginMock.Object;
            _SettingsPluginMock.Setup(x => x.GetAppIconFolderPath()).Returns(Task.FromResult("12"));
            privateObject.SetFieldOrProperty("_SettingsPlugin", _SettingsPlugin);
            var _DisplayManagerPluginMock = new Mock<IDisplayService>();
            var _DisplayManagerPlugin = _DisplayManagerPluginMock.Object;
            _DisplayManagerPluginMock.Setup(x => x.SetVCPCapability(It.IsAny<MonitorInfo>(), It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult(true));
            privateObject.SetFieldOrProperty("_DisplayManagerPlugin", _DisplayManagerPlugin);
            result = deviceMangerPlugin.FindAppsbyShell(true).Result;
            Assert.That(result.Count, Is.EqualTo(2));
        }

        [Test]
        public void TestAddColorPresetForMonitorConfig()
        {
            //_ColorPresetPlugin == null
            var result = deviceMangerPlugin.AddColorPresetForMonitorConfig("0", "name1", "c1").Result;
            Assert.That(result, Is.EqualTo(false));

            //_ColorPresetPlugin != null,_SettingsPlugin == null
            var _ColorPresetPluginMock = new Mock<IColorPresetSA>();
            var _ColorPresetPlugin = _ColorPresetPluginMock.Object;
            _ColorPresetPluginMock.Setup(x => x.GetInstalledAppsList(It.IsAny<bool>())).Returns(Task.FromResult(new Dictionary<string, InstalledAppInfo>() { { "a", new InstalledAppInfo() }, { "b", new InstalledAppInfo() } }));
            privateObject.SetFieldOrProperty("_ColorPresetPlugin", _ColorPresetPlugin);
            result = deviceMangerPlugin.AddColorPresetForMonitorConfig("0", "name1", "c1").Result;
            Assert.That(result, Is.EqualTo(false));

            var _SettingsPluginMock = new Mock<ISettingsManagerDev>();
            var _SettingsPlugin = _SettingsPluginMock.Object;
            _SettingsPluginMock.Setup(x => x.GetAppIconFolderPath()).Returns(Task.FromResult(""));
            privateObject.SetFieldOrProperty("_SettingsPlugin", _SettingsPlugin);
            result = deviceMangerPlugin.AddColorPresetForMonitorConfig("0", "name1", "c1").Result;
            Assert.That(result, Is.EqualTo(false));

            _SettingsPluginMock.Setup(x => x.GetAppIconFolderPath()).Returns(Task.FromResult("hh"));
            result = deviceMangerPlugin.AddColorPresetForMonitorConfig("a", "name1", "c1").Result;
            Assert.That(result, Is.EqualTo(false));

            List<MonitorInfo> _allInfoMonitors = new List<MonitorInfo>();
            _allInfoMonitors.Add(monitorInfo);
            privateObject.SetFieldOrProperty("_AllInfoMonitors", _allInfoMonitors);
            var _DisplayManagerPluginMock = new Mock<IDisplayService>();
            var _DisplayManagerPlugin = _DisplayManagerPluginMock.Object;
            _DisplayManagerPluginMock.Setup(x => x.GetVCPCapabilities(It.IsAny<MonitorInfo>())).Returns(Task.FromResult("true"));
            privateObject.SetFieldOrProperty("_DisplayManagerPlugin", _DisplayManagerPlugin);
            _SettingsPluginMock.Setup(x => x.ReadColorPresetSettings()).Returns(Task.FromResult(new List<ColorPresetSettings>() { new ColorPresetSettings(), new ColorPresetSettings() }));
            _ColorPresetPluginMock.Setup(x => x.AddColorPresetForMonitorConfig(It.IsAny<MonitorInfo>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<ColorPresetSettings>>())).Returns(Task.FromResult(new List<ColorPresetSettings>() { new ColorPresetSettings(), new ColorPresetSettings() }));
            result = deviceMangerPlugin.AddColorPresetForMonitorConfig("0", "name1", "c1").Result;
            Assert.That(result, Is.EqualTo(true));
        }

        [Test]
        public void TestChangeColorPresetForMonitorConfig()
        {
            //_ColorPresetPlugin == null
            try
            {
                deviceMangerPlugin.ChangeColorPresetForMonitorConfig("0", "name1", "c1");
                Assert.True(true);
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }

            //_ColorPresetPlugin != null
            var _ColorPresetPluginMock = new Mock<IColorPresetSA>();
            var _ColorPresetPlugin = _ColorPresetPluginMock.Object;
            _ColorPresetPluginMock.Setup(x => x.ChangeColorPresetForMonitorConfig(It.IsAny<MonitorInfo>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<ColorPresetSettings>>())).Returns(Task.FromResult(new List<ColorPresetSettings>() { new ColorPresetSettings() { }, new ColorPresetSettings() { } }));
            privateObject.SetFieldOrProperty("_ColorPresetPlugin", _ColorPresetPlugin);
            List<MonitorInfo> _allInfoMonitors = new List<MonitorInfo>();
            _allInfoMonitors.Add(monitorInfo);
            privateObject.SetFieldOrProperty("_AllInfoMonitors", _allInfoMonitors);
            var _SettingsPluginMock = new Mock<ISettingsManagerDev>();
            var _SettingsPlugin = _SettingsPluginMock.Object;
            _SettingsPluginMock.Setup(x => x.ReadColorPresetSettings()).Returns(Task.FromResult(new List<ColorPresetSettings>()));
            privateObject.SetFieldOrProperty("_SettingsPlugin", _SettingsPlugin);
            try
            {
                deviceMangerPlugin.ChangeColorPresetForMonitorConfig("0", "name1", "c1");
                Assert.True(true);
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }
        }

        [Test]
        public void TestDeleteColorPresetForMonitorConfig()
        {
            //_ColorPresetPlugin == null
            try
            {
                deviceMangerPlugin.DeleteColorPresetForMonitorConfig("0", "name1");
                Assert.True(true);
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }

            //_ColorPresetPlugin != null
            var _ColorPresetPluginMock = new Mock<IColorPresetSA>();
            var _ColorPresetPlugin = _ColorPresetPluginMock.Object;
            _ColorPresetPluginMock.Setup(x => x.DeleteColorPresetForMonitorConfig(It.IsAny<MonitorInfo>(), It.IsAny<string>(), It.IsAny<List<ColorPresetSettings>>())).Returns(Task.FromResult(new List<ColorPresetSettings>() { new ColorPresetSettings() { }, new ColorPresetSettings() { } }));
            privateObject.SetFieldOrProperty("_ColorPresetPlugin", _ColorPresetPlugin);
            List<MonitorInfo> _allInfoMonitors = new List<MonitorInfo>();
            _allInfoMonitors.Add(monitorInfo);
            privateObject.SetFieldOrProperty("_AllInfoMonitors", _allInfoMonitors);
            var _SettingsPluginMock = new Mock<ISettingsManagerDev>();
            var _SettingsPlugin = _SettingsPluginMock.Object;
            _SettingsPluginMock.Setup(x => x.ReadColorPresetSettings()).Returns(Task.FromResult(new List<ColorPresetSettings>()));
            privateObject.SetFieldOrProperty("_SettingsPlugin", _SettingsPlugin);
            try
            {
                deviceMangerPlugin.DeleteColorPresetForMonitorConfig("0", "name1");
                Assert.True(true);
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }
        }

        [Test]
        public void Testget_index_of_json_config_for_cur_monitor()
        {
            List<ColorPresetSettings> temp = new List<ColorPresetSettings>();
            temp.Add(new ColorPresetSettings() { DeviceInfo = new VcpCore.Common.EDID() { ModelName = "123", SerialNumber = "111" } });
            var monitorInfo = new MonitorInfo() { edid = new VcpCore.Common.EDID() { ModelName = "123", SerialNumber = "111" } };
            Test_AddAppCollectionData.GetInstance()._monitorConfigs = temp;

            var result = deviceMangerPlugin.get_index_of_json_config_for_cur_monitor(monitorInfo);

            // Assert
            Assert.AreNotEqual(-1, result);
        }

        [Test]
        public void TestAutoSetColorPresetForMonitorConfig()
        {
            //_ColorPresetPlugin == null
            try
            {
                deviceMangerPlugin.AutoSetColorPresetForMonitorConfig(monitorInfo, "on");
                Assert.True(true);
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }

            //_ColorPresetPlugin != null
            var _SettingsPluginMock = new Mock<ISettingsManagerDev>();
            var _SettingsPlugin = _SettingsPluginMock.Object;
            _SettingsPluginMock.Setup(x => x.ReloadAppConfigData(It.IsAny<bool>())).Returns(Task.FromResult(new DDPMSettings(new DDPMAppSettings(), new DDPMUserSettings(), new DDPMITConfig())));
            var _ColorPresetPluginMock = new Mock<IColorPresetSA>();
            var _ColorPresetPlugin = _ColorPresetPluginMock.Object;
            //_ColorPresetPluginMock.Setup(x => x.AutoSetColorPresetForMonitorConfig(It.IsAny<MonitorInfo>(), It.IsAny<string>(), It.IsAny<List<ColorPresetSettings>>())).Returns(Task.FromResult(new List<ColorPresetSettings>() { new ColorPresetSettings() { }, new ColorPresetSettings() { } }));
            privateObject.SetFieldOrProperty("_ColorPresetPlugin", _ColorPresetPlugin);
            privateObject.SetFieldOrProperty("_SettingsPlugin", _SettingsPlugin);
            try
            {
                deviceMangerPlugin.AutoSetColorPresetForMonitorConfig(monitorInfo, "1");
                Assert.True(true);
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }

            //_ColorPresetPlugin != null,on_off=="off"
            List<MonitorInfo> _allInfoMonitors = new List<MonitorInfo>();
            _allInfoMonitors.Add(monitorInfo);
            privateObject.SetFieldOrProperty("_AllInfoMonitors", _allInfoMonitors);
            _SettingsPluginMock.Setup(x => x.ReadColorPresetSettings()).Returns(Task.FromResult(new List<ColorPresetSettings>()));
            privateObject.SetFieldOrProperty("_SettingsPlugin", _SettingsPlugin);
            privateObject.SetFieldOrProperty("newWindowThread_AutoSetColorPresetForMonitorConfig", new Thread(new ThreadStart(showmsg)));
            try
            {
                deviceMangerPlugin.AutoSetColorPresetForMonitorConfig(monitorInfo, "off");
                Assert.True(true);
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }

            //_ColorPresetPlugin != null,on_off=="on",
            try
            {
                deviceMangerPlugin.AutoSetColorPresetForMonitorConfig(monitorInfo, "on");
                Assert.True(true);
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }
        }

        private void showmsg()
        {
        }

        [Test]
        public void TestShowOSD_ColoPreset()
        {
            //_ColorPresetPlugin == null
            try
            {
                deviceMangerPlugin.ShowOSD_ColoPreset(monitorInfo, "msg");
                Assert.True(true);
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }

            //_ColorPresetPlugin != null
            var _ColorPresetPluginMock = new Mock<IColorPresetSA>();
            var _ColorPresetPlugin = _ColorPresetPluginMock.Object;
            privateObject.SetFieldOrProperty("_ColorPresetPlugin", _ColorPresetPlugin);
            try
            {
                deviceMangerPlugin.ShowOSD_ColoPreset(monitorInfo, "msg");
                Assert.True(true);
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }
        }

        [Test]
        public void TestStartSchedulerManger()
        {
            var _ScheduleManagerPluginMock = new Mock<ISchedulerManager>();
            var _ScheduleManagerPlugin = _ScheduleManagerPluginMock.Object;
            privateObject.SetFieldOrProperty("_ScheduleManagerPlugin", _ScheduleManagerPlugin);
            var result = deviceMangerPlugin.StartSchedulerManger(1);
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void TestStopSchedulerManger()
        {
            var _ScheduleManagerPluginMock = new Mock<ISchedulerManager>();
            var _ScheduleManagerPlugin = _ScheduleManagerPluginMock.Object;
            privateObject.SetFieldOrProperty("_ScheduleManagerPlugin", _ScheduleManagerPlugin);
            var result = deviceMangerPlugin.StopSchedulerManger();
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void TestReset0x52TimerTick()
        {
            var _DisplayManagerPluginMock = new Mock<IDisplayService>();
            var _DisplayManagerPlugin = _DisplayManagerPluginMock.Object;
            privateObject.SetFieldOrProperty("_DisplayManagerPlugin", _DisplayManagerPlugin);
            var result = deviceMangerPlugin.Reset0x52TimerTick(1);
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void TestGetMonitors()
        {
            //_DisplayManagerPlugin == null
            var result = deviceMangerPlugin.GetMonitors(false).Result;
            Assert.That(result.Count, Is.EqualTo(0));

            //_DisplayManagerPlugin != null
            var _DisplayManagerPluginMock = new Mock<IDisplayService>();
            var _DisplayManagerPlugin = _DisplayManagerPluginMock.Object;
            privateObject.SetFieldOrProperty("_DisplayManagerPlugin", _DisplayManagerPlugin);
            _DisplayManagerPluginMock.Setup(x => x.GetMonitors(It.IsAny<bool>())).Returns(Task.FromResult(new List<MonitorInfo>() { monitorInfo }));
            var _SettingsPluginMock = new Mock<ISettingsManagerDev>();
            var _SettingsPlugin = _SettingsPluginMock.Object;
            privateObject.SetFieldOrProperty("_SettingsPlugin", _SettingsPlugin);
            _SettingsPluginMock.Setup(x => x.InitDDPMMonitorConfigFile(It.IsAny<string>())).Returns(Task.FromResult(new List<DDPMMonitorSettings>()));
            _SettingsPluginMock.Setup(x => x.WriteMonitorSettings(It.IsAny<string>(), It.IsAny<List<DDPMMonitorSettings>>())).Returns(Task.FromResult(true));
            result = deviceMangerPlugin.GetMonitors(false).Result;
            Assert.Greater(result.Count, 0);
        }

        [Test]
        public void TestGetCapabilitiesString()
        {
            var capabilitiesString = "(prot(monitor)type(LCD)model(U2724DE)cmds(01 02 03 07 0C E3 F3)vcp(02 04 05 08 10 12 14(01 04 05 06 08 09 0B 0C) 16 18 1A 52 60(19 0F 11 ) 66(0F02) 67 68 87 AA(00 01 02 04 ) AC AE B2 B6 C6 C8 C9 CA CC(02 0A 03 04 08 09 0D 06 ) D6(01 04 05) DC(00 03 05 ) DF E0 E1 E2(00 02 04 0C 0D 0F 10 11 13 0B 1A 1B 3D 14 ) E5 E7(02 03) E8 E9(00 01 02 21 22 24 ) EA(FC F8 ) F0(09 0A A1 ) EE EF(00 01 03 0F) F1 F2 FD)mswhql(1)asset_eep(40)mccs_ver(2.1))";
            var _DisplayManagerPluginMock = new Mock<IDisplayService>();
            var _DisplayManagerPlugin = _DisplayManagerPluginMock.Object;
            privateObject.SetFieldOrProperty("_DisplayManagerPlugin", _DisplayManagerPlugin);
            _DisplayManagerPluginMock.Setup(x => x.GetCapabilitiesString(It.IsAny<MonitorInfo>())).Returns(Task.FromResult(capabilitiesString));
            var result = deviceMangerPlugin.GetCapabilitiesString(monitorInfo).Result;
            Assert.That(result, Is.EqualTo(monitorInfo.CapabilityString));
        }

        [Test]
        public void TestGetVCPCapabilities()
        {
            var VcpCapabilities = "\"{\\r\\n  \\\"Index\\\": 0,\\r\\n  \\\"ModelName\\\": \\\"DELLU2424H\\\",\\r\\n  \\\"SerialNumber\\\": \\\"926168130\\\",\\r\\n  \\\"ServiceTag\\\": \\\"CN073K0\\\"";
            var _DisplayManagerPluginMock = new Mock<IDisplayService>();
            var _DisplayManagerPlugin = _DisplayManagerPluginMock.Object;
            privateObject.SetFieldOrProperty("_DisplayManagerPlugin", _DisplayManagerPlugin);
            _DisplayManagerPluginMock.Setup(x => x.GetVCPCapabilities(It.IsAny<MonitorInfo>())).Returns(Task.FromResult(VcpCapabilities));
            var result = deviceMangerPlugin.GetVCPCapabilities(monitorInfo).Result;
            Assert.Greater(result.Length, 0);
            Assert.That(result, Is.EqualTo(VcpCapabilities));
        }

        [Test]
        public void TestGetVCPCapability()
        {
            byte code = 0xE7;
            ObjGetVCP ObjGetvcp = new ObjGetVCP() { result = true, value = "10u" };//0xE7
            var ObjGetvcpValue = ObjGetvcp.value;
            var _DisplayManagerPluginMock = new Mock<IDisplayService>();
            var _DisplayManagerPlugin = _DisplayManagerPluginMock.Object;
            privateObject.SetFieldOrProperty("_DisplayManagerPlugin", _DisplayManagerPlugin);
            _DisplayManagerPluginMock.Setup(x => x.GetVCPCapability(It.IsAny<MonitorInfo>(), It.IsAny<byte>(), It.IsAny<int>())).Returns(Task.FromResult(ObjGetvcp));
            var result = deviceMangerPlugin.GetVCPCapability(monitorInfo, code, 0).Result;
            Assert.IsTrue(result.result);
            Assert.That(ObjGetvcp.value, Is.EqualTo(result.value));
        }

        [Test]
        public void TestGetVCPCapability_()
        {
            string funName = "colorpreset";
            ObjGetVCP ObjGetvcp = new ObjGetVCP() { result = true, value = "E2" };
            var _DisplayManagerPluginMock = new Mock<IDisplayService>();
            var _DisplayManagerPlugin = _DisplayManagerPluginMock.Object;
            privateObject.SetFieldOrProperty("_DisplayManagerPlugin", _DisplayManagerPlugin);
            _DisplayManagerPluginMock.Setup(x => x.GetVCPCapability(It.IsAny<MonitorInfo>(), It.IsAny<string>(), It.IsAny<int>())).Returns(Task.FromResult(ObjGetvcp));
            var result = deviceMangerPlugin.GetVCPCapability(monitorInfo, funName, 0).Result;
            Assert.IsTrue(result.result);
            Assert.That(ObjGetvcp.value, Is.EqualTo(result.value));
        }

        [Test]
        public void TestSetVCPCapability()
        {
            byte code = 0X21;
            uint val = 16;    //Brightness = 16
            bool setVCPCapability = true;
            var _DisplayManagerPluginMock = new Mock<IDisplayService>();
            var _DisplayManagerPlugin = _DisplayManagerPluginMock.Object;
            privateObject.SetFieldOrProperty("_DisplayManagerPlugin", _DisplayManagerPlugin);
            _DisplayManagerPluginMock.Setup(x => x.SetVCPCapability(It.IsAny<MonitorInfo>(), It.IsAny<byte>(), It.IsAny<uint>())).Returns(Task.FromResult(setVCPCapability));
            var result = deviceMangerPlugin.SetVCPCapability(monitorInfo, code, val).Result;
            Assert.IsTrue(result);

            //r && code == 0x04 && _NKVMPlugin != null
            code = 0x04;
            var _NKVMPluginMock = new Mock<INKVMService>();
            var _NKVMPlugin = _NKVMPluginMock.Object;
            privateObject.SetFieldOrProperty("_NKVMPlugin", _NKVMPlugin);
            result = deviceMangerPlugin.SetVCPCapability(monitorInfo, code, val).Result;
            Assert.IsTrue(result);
        }

        [Test]
        public void TestSetVCPCapability_()
        {
            string funtionName = "colorpreset";
            string val = "Warm";
            bool setVCPCapability_ = true;
            var _DisplayManagerPluginMock = new Mock<IDisplayService>();
            var _DisplayManagerPlugin = _DisplayManagerPluginMock.Object;
            privateObject.SetFieldOrProperty("_DisplayManagerPlugin", _DisplayManagerPlugin);
            _DisplayManagerPluginMock.Setup(x => x.SetVCPCapability(It.IsAny<MonitorInfo>(), It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult(setVCPCapability_));
            var result = deviceMangerPlugin.SetVCPCapability(monitorInfo, funtionName, val).Result;
            Assert.IsTrue(result);

            //r && FunctionName == "Input Select"
            funtionName = "Input Select";
            _DisplayManagerPluginMock.Setup(x => x.GetMonitors(It.IsAny<bool>())).Returns(Task.FromResult(new List<MonitorInfo>() { monitorInfo }));
            var _SettingsPluginMock = new Mock<ISettingsManagerDev>();
            var _SettingsPlugin = _SettingsPluginMock.Object;
            privateObject.SetFieldOrProperty("_SettingsPlugin", _SettingsPlugin);
            _SettingsPluginMock.Setup(x => x.InitDDPMMonitorConfigFile(It.IsAny<string>())).Returns(Task.FromResult(new List<DDPMMonitorSettings>()));
            _SettingsPluginMock.Setup(x => x.WriteMonitorSettings(It.IsAny<string>(), It.IsAny<List<DDPMMonitorSettings>>())).Returns(Task.FromResult(true));
            result = deviceMangerPlugin.SetVCPCapability(monitorInfo, funtionName, val).Result;
            Assert.IsTrue(result);
            //_NKVMPlugin != null
            var _NKVMPluginMock = new Mock<INKVMService>();
            var _NKVMPlugin = _NKVMPluginMock.Object;
            privateObject.SetFieldOrProperty("_NKVMPlugin", _NKVMPlugin);
            ObjGetVCP ObjGetvcp = new ObjGetVCP() { result = true, value = (UInt32)0xff };
            _DisplayManagerPluginMock.Setup(x => x.GetVCPCapability(It.IsAny<MonitorInfo>(), It.IsAny<byte>(), It.IsAny<int>())).Returns(Task.FromResult(ObjGetvcp));
            result = deviceMangerPlugin.SetVCPCapability(monitorInfo, funtionName, val).Result;
            Assert.IsTrue(result);
        }

        [Test]
        public void TestSetDisplayServiceIdle()
        {
            try
            {
                deviceMangerPlugin.SetDisplayServiceIdle(true);
                Assert.True(true);
                Assert.That(privateObject.GetFieldOrProperty("isLetDisplayServiceIdle"), Is.EqualTo(true));
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }
        }

        [Test]
        public void TestGetDisplayServiceIdleState()
        {
            var result = deviceMangerPlugin.GetDisplayServiceIdleState().Result;
            Assert.IsFalse(result);
        }

        [Test]
        public void TestGetInputSourcelist()
        {
            //settings == null
            var _SettingsPluginMock = new Mock<ISettingsManagerDev>();
            var _SettingsPlugin = _SettingsPluginMock.Object;
            privateObject.SetFieldOrProperty("_SettingsPlugin", _SettingsPlugin);
            var result = deviceMangerPlugin.GetInputSourcelist(monitorInfo).Result;
            Assert.That(result.Count, Is.EqualTo(0));

            //monitorSetting == null
            _SettingsPluginMock.Setup(x => x.ReloadMonitorSettings(It.IsAny<string>())).Returns(Task.FromResult(new List<DDPMMonitorSettings>() { new DDPMMonitorSettings() { Input = new Input() { strInputSourceList = "0" } } }));
            var _DisplayManagerPluginMock = new Mock<IDisplayService>();
            var _DisplayManagerPlugin = _DisplayManagerPluginMock.Object;
            privateObject.SetFieldOrProperty("_DisplayManagerPlugin", _DisplayManagerPlugin);
            _DisplayManagerPluginMock.Setup(x => x.GetInputSourcelist(It.IsAny<MonitorInfo>())).Returns(Task.FromResult(new Dictionary<string, InputInfo>() { { "A", new InputInfo() { InputName = "A1", USBUpstream = "A2" } } }));
            result = deviceMangerPlugin.GetInputSourcelist(monitorInfo).Result;
            Assert.Greater(result.Count, 0);

            //monitorSetting != null,monitorSetting.Input != null
            string strInputSourceLista = "{\"input\": { \"InputName\":\"InputSourceA\",\"USBUpstream\":\"bbb\"}}";
            _SettingsPluginMock.Setup(x => x.ReloadMonitorSettings(It.IsAny<string>())).Returns(Task.FromResult(new List<DDPMMonitorSettings>() { new DDPMMonitorSettings() { ServiceTag = "CN073K0", Input = new Input() { strInputSourceList = strInputSourceLista } } }));
            result = deviceMangerPlugin.GetInputSourcelist(monitorInfo).Result;
            Assert.Greater(result.Count, 0);
            Assert.That(result["input"].InputName, Is.EqualTo("InputSourceA"));
        }

        [Test]
        public void TestSetInputSourcelist()
        {
            //settings == null
            Dictionary<string, InputInfo> inputlist = new Dictionary<string, InputInfo>();
            var _SettingsPluginMock = new Mock<ISettingsManagerDev>();
            var _SettingsPlugin = _SettingsPluginMock.Object;
            privateObject.SetFieldOrProperty("_SettingsPlugin", _SettingsPlugin);
            var result = deviceMangerPlugin.SetInputSourcelist(monitorInfo, inputlist).Result;
            Assert.That(result, Is.EqualTo(false));

            //settings != null && inputlist != null
            string strInputSourceLista = "{\"input\": { \"InputName\":\"InputSourceA\",\"USBUpstream\":\"bbb\"}}";
            inputlist = new Dictionary<string, InputInfo>() { { "keya", new InputInfo() { InputName = "xx", USBUpstream = "yy" } } };
            _SettingsPluginMock.Setup(x => x.ReloadMonitorSettings(It.IsAny<string>())).Returns(Task.FromResult(new List<DDPMMonitorSettings>() { new DDPMMonitorSettings() { ServiceTag = "CN073K0", Input = new Input() { strInputSourceList = strInputSourceLista } } }));
            result = deviceMangerPlugin.SetInputSourcelist(monitorInfo, inputlist).Result;
            Assert.That(result, Is.EqualTo(false));

            _SettingsPluginMock.Setup(x => x.WriteMonitorSettings(It.IsAny<string>(), It.IsAny<List<DDPMMonitorSettings>>())).Returns(Task.FromResult(true));
            result = deviceMangerPlugin.SetInputSourcelist(monitorInfo, inputlist).Result;
            Assert.That(result, Is.EqualTo(true));
        }

        [Test]
        public void TestGetInputName()
        {
            //inputSourceList == null
            var _SettingsPluginMock = new Mock<ISettingsManagerDev>();
            var _SettingsPlugin = _SettingsPluginMock.Object;
            privateObject.SetFieldOrProperty("_SettingsPlugin", _SettingsPlugin);
            var result = deviceMangerPlugin.GetInputName(monitorInfo, "input").Result;
            Assert.That(result, Is.EqualTo(""));

            //inputSourceList != null
            var strInputSourceLista = "{\"input\": { \"InputName\":\"InputSourceA\",\"USBUpstream\":\"bbb\"}}";
            _SettingsPluginMock.Setup(x => x.ReloadMonitorSettings(It.IsAny<string>())).Returns(Task.FromResult(new List<DDPMMonitorSettings>() { new DDPMMonitorSettings() { ServiceTag = "CN073K0", Input = new Input() { strInputSourceList = strInputSourceLista } } }));
            result = deviceMangerPlugin.GetInputName(monitorInfo, "input").Result;
            Assert.That(result, Is.EqualTo("InputSourceA"));
        }

        [Test]
        public void TestSetInputName()
        {
            //inputSourceList == null
            var _SettingsPluginMock = new Mock<ISettingsManagerDev>();
            var _SettingsPlugin = _SettingsPluginMock.Object;
            privateObject.SetFieldOrProperty("_SettingsPlugin", _SettingsPlugin);
            var result = deviceMangerPlugin.SetInputName(monitorInfo, "input", "A").Result;
            Assert.That(result, Is.EqualTo(false));

            var strInputSourceLista = "{\"input\": { \"InputName\":\"InputSourceA\",\"USBUpstream\":\"bbb\"}}";
            _SettingsPluginMock.Setup(x => x.ReloadMonitorSettings(It.IsAny<string>())).Returns(Task.FromResult(new List<DDPMMonitorSettings>() { new DDPMMonitorSettings() { ServiceTag = "CN073K0", Input = new Input() { strInputSourceList = strInputSourceLista } } }));
            result = deviceMangerPlugin.SetInputName(monitorInfo, "input", "A").Result;
            Assert.That(result, Is.EqualTo(false));

            //inputSourceList != null
            _SettingsPluginMock.Setup(x => x.WriteMonitorSettings(It.IsAny<string>(), It.IsAny<List<DDPMMonitorSettings>>())).Returns(Task.FromResult(true));
            strInputSourceLista = "{\"input\": { \"InputName\":\"InputSourceA\",\"USBUpstream\":\"bbb\"}}";
            _SettingsPluginMock.Setup(x => x.ReloadMonitorSettings(It.IsAny<string>())).Returns(Task.FromResult(new List<DDPMMonitorSettings>() { new DDPMMonitorSettings() { ServiceTag = "CN073K0", Input = new Input() { strInputSourceList = strInputSourceLista } } }));
            result = deviceMangerPlugin.SetInputName(monitorInfo, "input", "A").Result;
            Assert.That(result, Is.EqualTo(true));
        }

        [Test]
        public void TestGetUSBUpstreamList()
        {
            var _DisplayManagerPluginMock = new Mock<IDisplayService>();
            var _DisplayManagerPlugin = _DisplayManagerPluginMock.Object;
            privateObject.SetFieldOrProperty("_DisplayManagerPlugin", _DisplayManagerPlugin);
            var result = deviceMangerPlugin.GetUSBUpstreamList(monitorInfo).Result;
            Assert.That(result, Is.EqualTo(null));

            _DisplayManagerPluginMock.Setup(x => x.GetUSBUpstreamList(It.IsAny<MonitorInfo>())).Returns(Task.FromResult(new List<string>() { "aa", "bb", "cc" }));
            result = deviceMangerPlugin.GetUSBUpstreamList(monitorInfo).Result;
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void TestSetUSBUpstream()
        {
            var _DisplayManagerPluginMock = new Mock<IDisplayService>();
            var _DisplayManagerPlugin = _DisplayManagerPluginMock.Object;
            privateObject.SetFieldOrProperty("_DisplayManagerPlugin", _DisplayManagerPlugin);
            var result = deviceMangerPlugin.SetUSBUpstream(monitorInfo, "inputsource", "upstream").Result;
            Assert.That(result, Is.EqualTo(false));

            var _SettingsPluginMock = new Mock<ISettingsManagerDev>();
            var _SettingsPlugin = _SettingsPluginMock.Object;
            privateObject.SetFieldOrProperty("_SettingsPlugin", _SettingsPlugin);
            var strInputSourceLista = "{\"input\": { \"InputName\":\"inputsource\",\"USBUpstream\":\"bbb\"}}";
            _SettingsPluginMock.Setup(x => x.ReloadMonitorSettings(It.IsAny<string>())).Returns(Task.FromResult(new List<DDPMMonitorSettings>() { new DDPMMonitorSettings() { ServiceTag = "CN073K0", Input = new Input() { strInputSourceList = strInputSourceLista } } }));
            _DisplayManagerPluginMock.Setup(x => x.SetUSBUpstream(It.IsAny<MonitorInfo>(), It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult(true));
            _SettingsPluginMock.Setup(x => x.WriteMonitorSettings(It.IsAny<string>(), It.IsAny<List<DDPMMonitorSettings>>())).Returns(Task.FromResult(true));
            result = deviceMangerPlugin.SetUSBUpstream(monitorInfo, "input", "upstream").Result;
            Assert.That(result, Is.EqualTo(true));
        }

        [Test]
        public void TestUSBSwitch()
        {
            var _SettingsPluginMock = new Mock<ISettingsManagerDev>();
            var _SettingsPlugin = _SettingsPluginMock.Object;
            privateObject.SetFieldOrProperty("_SettingsPlugin", _SettingsPlugin);
            var _DisplayManagerPluginMock = new Mock<IDisplayService>();
            var _DisplayManagerPlugin = _DisplayManagerPluginMock.Object;
            privateObject.SetFieldOrProperty("_DisplayManagerPlugin", _DisplayManagerPlugin);
            var result = deviceMangerPlugin.USBSwitch(monitorInfo, "inputsource1", "upstream1", "inputsource2", "upstream2").Result;
            Assert.That(result, Is.EqualTo(false));

            var strInputSourceLista = "{\"inputsource1\": { \"InputName\":\"inputsource1\",\"USBUpstream\":\"aaa\"},\"inputsource2\": { \"InputName\":\"inputsource2\",\"USBUpstream\":\"bbb\"}}";
            _SettingsPluginMock.Setup(x => x.ReloadMonitorSettings(It.IsAny<string>())).Returns(Task.FromResult(new List<DDPMMonitorSettings>() { new DDPMMonitorSettings() { ServiceTag = "CN073K0", Input = new Input() { strInputSourceList = strInputSourceLista } } }));
            _DisplayManagerPluginMock.Setup(x => x.USBSwitch(It.IsAny<MonitorInfo>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult(true));
            _SettingsPluginMock.Setup(x => x.WriteMonitorSettings(It.IsAny<string>(), It.IsAny<List<DDPMMonitorSettings>>())).Returns(Task.FromResult(true));
            result = deviceMangerPlugin.USBSwitch(monitorInfo, "inputsource1", "upstream1", "inputsource2", "upstream2").Result;
            Assert.That(result, Is.EqualTo(true));
        }
    }
}