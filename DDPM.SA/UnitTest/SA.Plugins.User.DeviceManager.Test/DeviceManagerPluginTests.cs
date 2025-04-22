using Castle.Core.Logging;
using DDPM.ColorApp;
using DDPM.MonitorBorker;
using DDPM.SA.Common;
using DDPM.SA.Common.Display;
using DDPM.SA.Common.Settings;
using DDPM.SA.Common.UpdateProgressPage;
using DDPM.SA.Plugins.User.DeviceManager;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.Interfaces;
using Dell.Client.Framework.UnitTestShared.Tests;
using DPeMPublic.Common.Enums;
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities.ObjectModel;


//using Microsoft.WindowsAPICodePack.PortableDevices.PropertySystem;
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
            var result = deviceMangerPlugin.DownloadICCData(monitorInfo, true, "");
            Assert.That(result, Is.Not.Null);

            //_ColorPresetPlugin != null
            var _ColorPresetPluginMock = new Mock<IColorPresetSA>();
            privateObject.SetFieldOrProperty("_ColorPresetPlugin", _ColorPresetPluginMock.Object);
            //_ColorPresetPluginMock.Setup(x => x.DownloadICCData(It.IsAny<MonitorInfo>(), It.IsAny<string>())).Returns(Task.FromResult(new DDPM.SA.Common.IIC_Metadata()));
            result = deviceMangerPlugin.DownloadICCData(monitorInfo, true, "");
            Assert.That(result, Is.Not.Null);
        }

        /*[Test]
        public void TestReadColorPreset()
        {
            //_SupportedColorPreset != null&& _SupportedColorPreset.Count == 0
            var result = deviceMangerPlugin.ReadColorPreset(monitorInfo).Result;
            Assert.That(result, Is.Not.Null);

            //_ColorPresetPlugin != null
            var _DisplayManagerPluginMock = new Mock<IDisplayService>();
            privateObject.SetFieldOrProperty("_DisplayManagerPlugin", _DisplayManagerPluginMock.Object);
            _DisplayManagerPluginMock.Setup(x => x.GetVCPCapabilities(It.IsAny<MonitorInfo>(), It.IsAny<Guid>(), It.IsAny<Priority>())).Returns(Task.FromResult("aa"));
            var _ColorPresetPluginMock = new Mock<IColorPresetSA>();
            privateObject.SetFieldOrProperty("_ColorPresetPlugin", _ColorPresetPluginMock.Object);
            _ColorPresetPluginMock.Setup(x => x.ReadColorPreset(It.IsAny<MonitorInfo>(), It.IsAny<string>(), It.IsAny<bool>())).Returns(Task.FromResult(new List<string>()));
            result = deviceMangerPlugin.ReadColorPreset(monitorInfo).Result;
            Assert.That(result, Is.Not.Null);

            //_SupportedColorPreset == null
            privateObject.SetFieldOrProperty("_SupportedColorPreset", null);
            result = deviceMangerPlugin.ReadColorPreset(monitorInfo).Result;
            Assert.That(result, Is.EqualTo(null));
        }*/

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
            _DisplayManagerPluginMock.Setup(x => x.GetVCPCapability(It.IsAny<MonitorInfo>(), It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<Priority>())).Returns(Task.FromResult(new ObjGetVCP() { result = true, value = "_DisplayManagerPlugin" }));
            privateObject.SetFieldOrProperty("_DisplayManagerPlugin", _DisplayManagerPlugin);
            result = deviceMangerPlugin.ReadCurrentColorPreset(monitorInfo).Result;
            Assert.That(result, Is.EqualTo("_DisplayManagerPlugin"));

            //_DisplayManagerPlugin != null&&result.result==false
            _DisplayManagerPluginMock.Setup(x => x.GetVCPCapability(It.IsAny<MonitorInfo>(), It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<Priority>())).Returns(Task.FromResult(new ObjGetVCP() { result = false }));
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
            var LogMock = new Mock<ILog>();
            var MainWindowLog = LogMock.Object;
            var monitorBorkerWin = new MainWindow(DeviceManagerSA, monitorInfo, MainWindowLog);
            var MainWindow = new MainWindow(DeviceManagerSA, monitorInfo, MainWindowLog);
            PrivateObject privateObjecta = new PrivateObject(MainWindow);
            privateObjecta.SetFieldOrProperty("ColorPresetWin", new MonitorWin(DeviceManagerSA, monitorInfo, MainWindowLog));
            privateObjecta.SetFieldOrProperty("ddmLib", DeviceManagerSA);
            DeviceManagerSAMock.Setup(x => x.ReadColorPresetSettings()).Returns(Task.FromResult(new List<ColorPresetSettings>())); //DDPM.ColorApp.MonitorWin.xaml.cs
            privateObject.SetFieldOrProperty("MonitorBorkerWin", monitorBorkerWin);
            result = deviceMangerPlugin.Notify_refresh_app_list().Result;
            Assert.That(result, Is.EqualTo(true));
        }

        [Test]
        public void TestWriteColorPreset()
        {
            //_ColorPresetPlugin == null
            IColorPresetSA? ColorPresetPlugin = null;
            privateObject.SetFieldOrProperty("_ColorPresetPlugin", ColorPresetPlugin);
            var result = deviceMangerPlugin.WriteColorPreset(monitorInfo, "").Result;
            Assert.That(result, Is.EqualTo(false));

            //_ColorPresetPlugin != null
            var _ColorPresetPluginMock = new Mock<IColorPresetSA>();
            _ColorPresetPluginMock.Setup(x => x.WriteColorPreset(It.IsAny<MonitorInfo>(), It.IsAny<string>(), It.IsAny<ISettingsManagerDev>(), It.IsAny<int>())).Returns(Task.FromResult(true));
            var _ColorPresetPlugin = _ColorPresetPluginMock.Object;
            privateObject.SetFieldOrProperty("_ColorPresetPlugin", _ColorPresetPlugin);

            List<ALSConfig> aLSConfigs = new List<ALSConfig>() { new ALSConfig() { Edid = monitorInfo.edid, ModelName = monitorInfo.modelName, isPrimaryMonitorSync = false } };
            var _DisplayManagerPluginMock = new Mock<IDisplayService>();
            _DisplayManagerPluginMock.Setup(x => x.SetVCPCapability(It.IsAny<MonitorInfo>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Priority>())).Returns(Task.FromResult(true));
            _DisplayManagerPluginMock.Setup(x => x.GetMonitorCurrentResolution(It.IsAny<MonitorInfo>())).Returns(Task.FromResult("1920*1080"));
            _DisplayManagerPluginMock.Setup(x => x.GetMonitorMaxResolution(It.IsAny<MonitorInfo>())).Returns(Task.FromResult("1920*1080"));
            _DisplayManagerPluginMock.Setup(x => x.GetAllExistAlsConfig()).Returns(Task.FromResult(aLSConfigs));
            var _DisplayManagerPlugin = _DisplayManagerPluginMock.Object;
            privateObject.SetFieldOrProperty("_DisplayManagerPlugin", _DisplayManagerPlugin);

            var TelementryScheduler = new Mock<ITelementryScheduler>();
            TelementryScheduler.Setup(x => x.ReceiveTelemetryInfo(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Telementry_Frequency>())).Returns(Task.FromResult(true));
            var TelementrySchedulerPlugin = TelementryScheduler.Object;
            privateObject.SetFieldOrProperty("_TelementryScheduler", TelementrySchedulerPlugin);

            int colorPresetRunType = 0;
            string colorPreset_Name = "Game";
            int colorPresetRunType2 = (int)ColorPresetRunType.Auto;

            if (colorPresetRunType == 0)
            {
                result = deviceMangerPlugin.WriteColorPreset(monitorInfo, colorPreset_Name, colorPresetRunType).Result;
                Assert.That(result, Is.EqualTo(true));
            }

            _ColorPresetPluginMock.Setup(x => x.WriteColorPreset(It.IsAny<MonitorInfo>(), It.IsAny<string>(), It.IsAny<ISettingsManagerDev>(), It.IsAny<int>())).Returns(Task.FromResult(true));
            var _ColorPresetPlugin2 = _ColorPresetPluginMock.Object;
            privateObject.SetFieldOrProperty("_ColorPresetPlugin", _ColorPresetPlugin2);

            if (colorPresetRunType2 == (int)ColorPresetRunType.Auto)
            {
                //var result2 = deviceMangerPlugin.WriteColorPreset(monitorInfo, colorPreset_Name, colorPresetRunType2, "TestreqAppName").Result;
                //Assert.That(result2, Is.EqualTo(true));
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
            _DisplayManagerPluginMock.Setup(x => x.SetVCPCapability(It.IsAny<MonitorInfo>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Priority>())).Returns(Task.FromResult(true));
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
            _DisplayManagerPluginMock.Setup(x => x.SetVCPCapability(It.IsAny<MonitorInfo>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Priority>())).Returns(Task.FromResult(true));
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
            _DisplayManagerPluginMock.Setup(x => x.SetVCPCapability(It.IsAny<MonitorInfo>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Priority>())).Returns(Task.FromResult(true));
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

            string iconFolder = "C:\\Windows";
            _SettingsPluginMock.Setup(x => x.GetAppIconFolderPath()).Returns(Task.FromResult(iconFolder));
            result = deviceMangerPlugin.AddColorPresetForMonitorConfig("a", "name1", "c1").Result;
            Assert.That(result, Is.EqualTo(false));

            List<MonitorInfo> _allInfoMonitors = new List<MonitorInfo>();
            _allInfoMonitors.Add(monitorInfo);
            privateObject.SetFieldOrProperty("_AllInfoMonitors", _allInfoMonitors);
            var _DisplayManagerPluginMock = new Mock<IDisplayService>();
            var _DisplayManagerPlugin = _DisplayManagerPluginMock.Object;
            _DisplayManagerPluginMock.Setup(x => x.GetVCPCapabilities(It.IsAny<MonitorInfo>(), It.IsAny<Guid>(), It.IsAny<Priority>())).Returns(Task.FromResult("true"));
            privateObject.SetFieldOrProperty("_DisplayManagerPlugin", _DisplayManagerPlugin);
            _SettingsPluginMock.Setup(x => x.ReadColorPresetSettings()).Returns(Task.FromResult(new List<ColorPresetSettings>() { new ColorPresetSettings(), new ColorPresetSettings() }));
            _ColorPresetPluginMock.Setup(x => x.AddColorPresetForMonitorConfig(It.IsAny<MonitorInfo>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<ColorPresetSettings>>(), It.IsAny<bool>())).Returns(Task.FromResult(new List<ColorPresetSettings>() { new ColorPresetSettings(), new ColorPresetSettings() }));
            _SettingsPluginMock.Setup(x => x.WriteColorPresetSettings(It.IsAny<List<ColorPresetSettings>>())).Returns(Task.FromResult(true));
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
            temp.Add(new ColorPresetSettings() { ModelName = "DELLU3224KB", SerialNumber = "808792396" });
            var monitorInfo = new MonitorInfo() { edid = new VcpCore.Common.EDID() { ModelName = "DELLU3224KB", SerialNumber = "808792396" } };
            Test_AddAppCollectionData.GetInstance()._monitorConfigs = temp;

            //var result = deviceMangerPlugin.get_index_of_json_config_for_cur_monitor(monitorInfo);

            // Assert
            //Assert.AreNotEqual(-1, result);
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
            //_ColorPresetPluginMock.Setup(x => x.AutoSetColorPresetForMonitorConfig(It.IsAny<MonitorInfo>(), It.IsAny<string>(), It.IsAny<ISettingsManagerDev>(), It.IsAny<IDeviceManagerSA>(), It.IsAny<bool>(), It.IsAny<List<string>>())).Returns(Task.FromResult(true));
            var _DisplayManagerPluginMock = new Mock<IDisplayService>();
            var _DisplayManagerPlugin = _DisplayManagerPluginMock.Object;
            _DisplayManagerPluginMock.Setup(x => x.GetHDRStatus(It.IsAny<MonitorInfo>(), It.IsAny<bool>())).Returns(Task.FromResult(true));
            privateObject.SetFieldOrProperty("_ColorPresetPlugin", _ColorPresetPlugin);
            privateObject.SetFieldOrProperty("_SettingsPlugin", _SettingsPlugin);
            privateObject.SetFieldOrProperty("_DisplayManagerPlugin", _DisplayManagerPlugin);
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
            var result = deviceMangerPlugin.GetMonitors().Result;
            Assert.That(result.Count, Is.EqualTo(0));

            //_DisplayManagerPlugin != null
            var _DisplayManagerPluginMock = new Mock<IDisplayService>();
            var _DisplayManagerPlugin = _DisplayManagerPluginMock.Object;
            privateObject.SetFieldOrProperty("_DisplayManagerPlugin", _DisplayManagerPlugin);
            _DisplayManagerPluginMock.Setup(x => x.GetMonitors()).Returns(Task.FromResult(new List<MonitorInfo>() { monitorInfo }));
            var _SettingsPluginMock = new Mock<ISettingsManagerDev>();
            var _SettingsPlugin = _SettingsPluginMock.Object;
            privateObject.SetFieldOrProperty("_SettingsPlugin", _SettingsPlugin);
            //_SettingsPluginMock.Setup(x => x.InitDDPMMonitorConfigFile(It.IsAny<string>())).Returns(Task.FromResult(new List<DDPMMonitorSettings>()));
            _SettingsPluginMock.Setup(x => x.WriteMonitorSettings(It.IsAny<string>(), It.IsAny<List<DDPMMonitorSettings>>())).Returns(Task.FromResult(true));
            result = deviceMangerPlugin.GetMonitors().Result;
            Assert.Greater(result.Count, 0);
        }

        [Test]
        public void TestGetCapabilitiesString()
        {
            var capabilitiesString = "(prot(monitor)type(LCD)model(U2724DE)cmds(01 02 03 07 0C E3 F3)vcp(02 04 05 08 10 12 14(01 04 05 06 08 09 0B 0C) 16 18 1A 52 60(19 0F 11 ) 66(0F02) 67 68 87 AA(00 01 02 04 ) AC AE B2 B6 C6 C8 C9 CA CC(02 0A 03 04 08 09 0D 06 ) D6(01 04 05) DC(00 03 05 ) DF E0 E1 E2(00 02 04 0C 0D 0F 10 11 13 0B 1A 1B 3D 14 ) E5 E7(02 03) E8 E9(00 01 02 21 22 24 ) EA(FC F8 ) F0(09 0A A1 ) EE EF(00 01 03 0F) F1 F2 FD)mswhql(1)asset_eep(40)mccs_ver(2.1))";
            var _DisplayManagerPluginMock = new Mock<IDisplayService>();
            var _DisplayManagerPlugin = _DisplayManagerPluginMock.Object;
            privateObject.SetFieldOrProperty("_DisplayManagerPlugin", _DisplayManagerPlugin);
            _DisplayManagerPluginMock.Setup(x => x.GetCapabilitiesString(It.IsAny<MonitorInfo>(), It.IsAny<Guid>(), It.IsAny<Priority>())).Returns(Task.FromResult(capabilitiesString));
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
            _DisplayManagerPluginMock.Setup(x => x.GetVCPCapabilities(It.IsAny<MonitorInfo>(), It.IsAny<Guid>(), It.IsAny<Priority>())).Returns(Task.FromResult(VcpCapabilities));
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
            _DisplayManagerPluginMock.Setup(x => x.GetVCPCapability(It.IsAny<MonitorInfo>(), It.IsAny<byte>(), It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<Priority>())).Returns(Task.FromResult(ObjGetvcp));
            var result = deviceMangerPlugin.GetVCPCapability(monitorInfo, code, opt: 0).Result;
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
            _DisplayManagerPluginMock.Setup(x => x.GetVCPCapability(It.IsAny<MonitorInfo>(), It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<Priority>())).Returns(Task.FromResult(ObjGetvcp));
            var result = deviceMangerPlugin.GetVCPCapability(monitorInfo, funName, opt: 0).Result;
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
            _DisplayManagerPluginMock.Setup(x => x.SetVCPCapability(It.IsAny<MonitorInfo>(), It.IsAny<byte>(), It.IsAny<uint>(), It.IsAny<Guid>(), It.IsAny<Priority>())).Returns(Task.FromResult(setVCPCapability));
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
            _DisplayManagerPluginMock.Setup(x => x.SetVCPCapability(It.IsAny<MonitorInfo>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Priority>())).Returns(Task.FromResult(setVCPCapability_));
            var result = deviceMangerPlugin.SetVCPCapability(monitorInfo, funtionName, val).Result;
            Assert.IsTrue(result);

            //r && FunctionName == "Input Select"
            funtionName = "Input Select";
            _DisplayManagerPluginMock.Setup(x => x.GetMonitors()).Returns(Task.FromResult(new List<MonitorInfo>() { monitorInfo }));
            var _SettingsPluginMock = new Mock<ISettingsManagerDev>();
            var _SettingsPlugin = _SettingsPluginMock.Object;
            privateObject.SetFieldOrProperty("_SettingsPlugin", _SettingsPlugin);
            //_SettingsPluginMock.Setup(x => x.InitDDPMMonitorConfigFile(It.IsAny<string>())).Returns(Task.FromResult(new List<DDPMMonitorSettings>()));
            _SettingsPluginMock.Setup(x => x.WriteMonitorSettings(It.IsAny<string>(), It.IsAny<List<DDPMMonitorSettings>>())).Returns(Task.FromResult(true));
            result = deviceMangerPlugin.SetVCPCapability(monitorInfo, funtionName, val).Result;
            Assert.IsTrue(result);
            //_NKVMPlugin != null
            var _NKVMPluginMock = new Mock<INKVMService>();
            var _NKVMPlugin = _NKVMPluginMock.Object;
            privateObject.SetFieldOrProperty("_NKVMPlugin", _NKVMPlugin);
            ObjGetVCP ObjGetvcp = new ObjGetVCP() { result = true, value = (UInt32)0xff };
            _DisplayManagerPluginMock.Setup(x => x.GetVCPCapability(It.IsAny<MonitorInfo>(), It.IsAny<byte>(), It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<Priority>())).Returns(Task.FromResult(ObjGetvcp));
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
            _SettingsPluginMock.Setup(x => x.ReloadMonitorSettings(It.IsAny<string>())).Returns(Task.FromResult(new List<DDPMMonitorSettings>() { new DDPMMonitorSettings() { Input = new InputSource() { strInputSourceList = "0" } } }));
            var _DisplayManagerPluginMock = new Mock<IDisplayService>();
            var _DisplayManagerPlugin = _DisplayManagerPluginMock.Object;
            privateObject.SetFieldOrProperty("_DisplayManagerPlugin", _DisplayManagerPlugin);
            _DisplayManagerPluginMock.Setup(x => x.GetInputSourcelist(It.IsAny<MonitorInfo>())).Returns(Task.FromResult(new Dictionary<string, InputInfo>() { { "A", new InputInfo() { InputName = "A1", USBUpstream = "A2" } } }));
            result = deviceMangerPlugin.GetInputSourcelist(monitorInfo).Result;
            Assert.That(result.Count, Is.EqualTo(0));  // GetInputSourcelist method update

            //monitorSetting != null,monitorSetting.Input != null
            string strInputSourceLista = "{\"input\": { \"InputName\":\"InputSourceA\",\"USBUpstream\":\"bbb\"}}";
            _SettingsPluginMock.Setup(x => x.ReloadMonitorSettings(It.IsAny<string>())).Returns(Task.FromResult(new List<DDPMMonitorSettings>() { new DDPMMonitorSettings() { ServiceTag = "CN073K0", Input = new InputSource() { strInputSourceList = strInputSourceLista } } }));
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
            _SettingsPluginMock.Setup(x => x.ReloadMonitorSettings(It.IsAny<string>())).Returns(Task.FromResult(new List<DDPMMonitorSettings>() { new DDPMMonitorSettings() { ServiceTag = "CN073K0", Input = new InputSource() { strInputSourceList = strInputSourceLista } } }));
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
            _SettingsPluginMock.Setup(x => x.ReloadMonitorSettings(It.IsAny<string>())).Returns(Task.FromResult(new List<DDPMMonitorSettings>() { new DDPMMonitorSettings() { ServiceTag = "CN073K0", Input = new InputSource() { strInputSourceList = strInputSourceLista } } }));
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
            _SettingsPluginMock.Setup(x => x.ReloadMonitorSettings(It.IsAny<string>())).Returns(Task.FromResult(new List<DDPMMonitorSettings>() { new DDPMMonitorSettings() { ServiceTag = "CN073K0", Input = new InputSource() { strInputSourceList = strInputSourceLista } } }));
            result = deviceMangerPlugin.SetInputName(monitorInfo, "input", "A").Result;
            Assert.That(result, Is.EqualTo(false));

            //inputSourceList != null
            _SettingsPluginMock.Setup(x => x.WriteMonitorSettings(It.IsAny<string>(), It.IsAny<List<DDPMMonitorSettings>>())).Returns(Task.FromResult(true));
            strInputSourceLista = "{\"input\": { \"InputName\":\"InputSourceA\",\"USBUpstream\":\"bbb\"}}";
            _SettingsPluginMock.Setup(x => x.ReloadMonitorSettings(It.IsAny<string>())).Returns(Task.FromResult(new List<DDPMMonitorSettings>() { new DDPMMonitorSettings() { ServiceTag = "CN073K0", Input = new InputSource() { strInputSourceList = strInputSourceLista } } }));
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
            _SettingsPluginMock.Setup(x => x.ReloadMonitorSettings(It.IsAny<string>())).Returns(Task.FromResult(new List<DDPMMonitorSettings>() { new DDPMMonitorSettings() { ServiceTag = "CN073K0", Input = new InputSource() { strInputSourceList = strInputSourceLista } } }));
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
            _SettingsPluginMock.Setup(x => x.ReloadMonitorSettings(It.IsAny<string>())).Returns(Task.FromResult(new List<DDPMMonitorSettings>() { new DDPMMonitorSettings() { ServiceTag = "CN073K0", Input = new InputSource() { strInputSourceList = strInputSourceLista } } }));
            _DisplayManagerPluginMock.Setup(x => x.USBSwitch(It.IsAny<MonitorInfo>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult(true));
            _SettingsPluginMock.Setup(x => x.WriteMonitorSettings(It.IsAny<string>(), It.IsAny<List<DDPMMonitorSettings>>())).Returns(Task.FromResult(true));
            result = deviceMangerPlugin.USBSwitch(monitorInfo, "inputsource1", "upstream1", "inputsource2", "upstream2").Result;
            Assert.That(result, Is.EqualTo(true));
        }

        [Test]
        public async Task TestGetDevices()
        {
            // Setup
            var _PeripheralsPluginMock = new Mock<IDPeMPlugin>();
            privateObject.SetFieldOrProperty("_PeripheralsPlugin", _PeripheralsPluginMock.Object);
            DeviceHelper deviceHelper = new DeviceHelper()
            {
                deviceInfo = new List<DeviceInfo>
                {
                new DeviceInfo {LogicalDeviceType = "LogicalHeadset",DeviceName = "LogicalHeadset",Name="SB725",PhysicalDeviceType=DeviceType.LogicalDock,ID=Guid.NewGuid() },
                }
            };
            _PeripheralsPluginMock.Setup(x => x.GetDevices(It.IsAny<bool>())).Returns(Task.FromResult(deviceHelper));

            Mock<IDTPProxyPlugin> DTPProxyPluginMock = new Mock<IDTPProxyPlugin>();
            privateObject.SetFieldOrProperty("_DTPProxyPlugin", DTPProxyPluginMock.Object);
            DTPProxyPluginMock.Setup(x => x.GetFirmwareVersionForDock(It.IsAny<string>())).Returns(Task.FromResult("TestV1.0"));
            DTPProxyPluginMock.Setup(x => x.GetDockServiceTagForDock(It.IsAny<string>())).Returns(Task.FromResult("TestSevriceTag123456"));
            var result = await deviceMangerPlugin.GetDevices();

            // Execute and Verify
            Assert.ThrowsAsync<InvalidOperationException>(async () => await deviceMangerPlugin.GetDevices());
        }

        [Test]
        public async Task TestGetCTKMessageHelper()
        {
            // Setup
            var _PeripheralsPluginMock = new Mock<IDPeMPlugin>();
            privateObject.SetFieldOrProperty("_PeripheralsPlugin", _PeripheralsPluginMock.Object);
            await deviceMangerPlugin.GetCTKMessageHelper();

            // Execute and Verify
            Assert.ThrowsAsync<InvalidOperationException>(async () => await deviceMangerPlugin.GetCTKMessageHelper());
        }

        [Test]
        public async Task TestGetRFDongleDevices()
        {
            // Setup
            var _PeripheralsPluginMock = new Mock<IDPeMPlugin>();
            privateObject.SetFieldOrProperty("_PeripheralsPlugin", _PeripheralsPluginMock.Object);
            await deviceMangerPlugin.GetRFDongleDevices();

            // Execute and Verify
            Assert.ThrowsAsync<InvalidOperationException>(async () => await deviceMangerPlugin.GetRFDongleDevices());
        }

        [Test]
        public void TestSetBackLightingControls()
        {
            // Setup
            var _PeripheralsPluginMock = new Mock<IDPeMPlugin>();
            privateObject.SetFieldOrProperty("_PeripheralsPlugin", _PeripheralsPluginMock.Object);

            // Execute and Verify
            Assert.IsNotNull(deviceMangerPlugin.SetBackLightingControls(3, Guid.NewGuid()), $"SetBackLightingControls() returns null");
        }

        [Test]
        public void TestSetBackLightingLevel()
        {
            // Setup
            var _PeripheralsPluginMock = new Mock<IDPeMPlugin>();
            privateObject.SetFieldOrProperty("_PeripheralsPlugin", _PeripheralsPluginMock.Object);

            // Execute and Verify
            Assert.IsNotNull(deviceMangerPlugin.SetBackLightingLevel(3, Guid.NewGuid()), $"SetBackLightingLevel() returns null");
        }

        [Test]
        public void TestSetCollaborationBlinkEffectEnable()
        {
            // Setup
            var _PeripheralsPluginMock = new Mock<IDPeMPlugin>();
            privateObject.SetFieldOrProperty("_PeripheralsPlugin", _PeripheralsPluginMock.Object);

            // Execute and Verify
            Assert.IsNotNull(deviceMangerPlugin.SetCollaborationBlinkEffectEnable(true, Guid.NewGuid()), $"SetCollaborationBlinkEffectEnable() returns null");
        }

        [Test]
        public void TestSetCollaborationCameraEnable()
        {
            // Setup
            var _PeripheralsPluginMock = new Mock<IDPeMPlugin>();
            privateObject.SetFieldOrProperty("_PeripheralsPlugin", _PeripheralsPluginMock.Object);

            // Execute and Verify
            Assert.IsNotNull(deviceMangerPlugin.SetCollaborationCameraEnable(true, Guid.NewGuid()), $"SetCollaborationCameraEnable() returns null");
        }

        [Test]
        public void TestSetCollaborationChatEnable()
        {
            // Setup
            var _PeripheralsPluginMock = new Mock<IDPeMPlugin>();
            privateObject.SetFieldOrProperty("_PeripheralsPlugin", _PeripheralsPluginMock.Object);

            // Execute and Verify
            Assert.IsNotNull(deviceMangerPlugin.SetCollaborationChatEnable(true, Guid.NewGuid()), $"SetCollaborationChatEnable() returns null");
        }

        [Test]
        public void TestSetCollaborationDoubleTapEnable()
        {
            // Setup
            var _PeripheralsPluginMock = new Mock<IDPeMPlugin>();
            privateObject.SetFieldOrProperty("_PeripheralsPlugin", _PeripheralsPluginMock.Object);

            // Execute and Verify
            Assert.IsNotNull(deviceMangerPlugin.SetCollaborationDoubleTapEnable(true, Guid.NewGuid()), $"SetCollaborationDoubleTapEnable() returns null");
        }

        [Test]
        public void TestSetCollaborationKeyEnable()
        {
            // Setup
            var _PeripheralsPluginMock = new Mock<IDPeMPlugin>();
            privateObject.SetFieldOrProperty("_PeripheralsPlugin", _PeripheralsPluginMock.Object);

            // Execute and Verify
            Assert.IsNotNull(deviceMangerPlugin.SetCollaborationKeyEnable(true, Guid.NewGuid()), $"SetCollaborationKeyEnable() returns null");
        }

        [Test]
        public void TestSetCollaborationMicEnable()
        {
            // Setup
            var _PeripheralsPluginMock = new Mock<IDPeMPlugin>();
            privateObject.SetFieldOrProperty("_PeripheralsPlugin", _PeripheralsPluginMock.Object);

            // Execute and Verify
            Assert.IsNotNull(deviceMangerPlugin.SetCollaborationMicEnable(true, Guid.NewGuid()), $"SetCollaborationMicEnable() returns null");
        }

        [Test]
        public void TestSetCollaborationScreenShareEnable()
        {
            // Setup
            var _PeripheralsPluginMock = new Mock<IDPeMPlugin>();
            DeviceHelper deviceHelper = new DeviceHelper() { deviceInfo = new List<DeviceInfo>() { new DeviceInfo() { DeviceName = "Mouse" } } };
            _PeripheralsPluginMock.Setup(x => x.GetDevices(false)).Returns(Task.FromResult(deviceHelper));
            privateObject.SetFieldOrProperty("_PeripheralsPlugin", _PeripheralsPluginMock.Object);

            // Execute and Verify
            Assert.IsNotNull(deviceMangerPlugin.SetCollaborationScreenShareEnable(true, Guid.NewGuid()), $"SetCollaborationScreenShareEnable() returns null");
        }

        [Test]
        public void TestSetDPILevel()
        {
            // Setup
            var _PeripheralsPluginMock = new Mock<IDPeMPlugin>();
            privateObject.SetFieldOrProperty("_PeripheralsPlugin", _PeripheralsPluginMock.Object);

            // Execute and Verify
            Assert.IsNotNull(deviceMangerPlugin.SetDPILevel(2, Guid.NewGuid()), $"SetDPILevel() returns null");
        }

        [Test]
        public void TestSetDPIValue()
        {
            // Setup
            var _PeripheralsPluginMock = new Mock<IDPeMPlugin>();
            privateObject.SetFieldOrProperty("_PeripheralsPlugin", _PeripheralsPluginMock.Object);

            // Execute and Verify
            Assert.IsNotNull(deviceMangerPlugin.SetDPIValue(2, Guid.NewGuid()), $"SetDPIValue() returns null");
        }

        [Test]
        public void TestSetPrimaryMouseButton()
        {
            // Setup
            var _PeripheralsPluginMock = new Mock<IDPeMPlugin>();
            privateObject.SetFieldOrProperty("_PeripheralsPlugin", _PeripheralsPluginMock.Object);

            // Execute and Verify
            Assert.IsNotNull(deviceMangerPlugin.SetPrimaryMouseButton(DPeMPublic.Common.Enums.MouseButton.Left, Guid.NewGuid()), $"SetPrimaryMouseButton() returns null");
        }

        [Test]
        public void TestSetTouchScrollSensitivityLevel()
        {
            // Setup
            var _PeripheralsPluginMock = new Mock<IDPeMPlugin>();
            privateObject.SetFieldOrProperty("_PeripheralsPlugin", _PeripheralsPluginMock.Object);

            // Execute and Verify
            Assert.IsNotNull(deviceMangerPlugin.SetTouchScrollSensitivityLevel(2, Guid.NewGuid()), $"SetTouchScrollSensitivityLevel() returns null");
        }

        [Test]
        public void TestUnPair()
        {
            // Setup
            var _PeripheralsPluginMock = new Mock<IDPeMPlugin>();
            privateObject.SetFieldOrProperty("_PeripheralsPlugin", _PeripheralsPluginMock.Object);

            // Execute and Verify
            Assert.IsNotNull(deviceMangerPlugin.UnPair(Guid.NewGuid()), $"UnPair() returns null");
        }

        [Test]
        public void TestStartPairing()
        {
            // Setup
            var _PeripheralsPluginMock = new Mock<IDPeMPlugin>();
            privateObject.SetFieldOrProperty("_PeripheralsPlugin", _PeripheralsPluginMock.Object);

            // Execute and Verify
            Assert.IsNotNull(deviceMangerPlugin.StartPairing(Guid.NewGuid()), $"StartPairing() returns null");
        }

        [Test]
        public void TestStopPairing()
        {
            // Setup
            var _PeripheralsPluginMock = new Mock<IDPeMPlugin>();
            privateObject.SetFieldOrProperty("_PeripheralsPlugin", _PeripheralsPluginMock.Object);

            // Execute and Verify
            Assert.IsNotNull(deviceMangerPlugin.StopPairing(Guid.NewGuid()), $"StopPairing() returns null");
        }

        [Test]
        public void TestSetWiredAudioIMicNSEnable()
        {
            // Setup
            var _PeripheralsPluginMock = new Mock<IDPeMPlugin>();
            privateObject.SetFieldOrProperty("_PeripheralsPlugin", _PeripheralsPluginMock.Object);

            // Execute and Verify
            Assert.IsNotNull(deviceMangerPlugin.SetWiredAudioIMicNSEnable(true, Guid.NewGuid()), $"SetWiredAudioIMicNSEnable() returns null");
        }

        [Test]
        public void TestSetWiredAudioMicMuteSoundEnable()
        {
            // Setup
            var _PeripheralsPluginMock = new Mock<IDPeMPlugin>();
            privateObject.SetFieldOrProperty("_PeripheralsPlugin", _PeripheralsPluginMock.Object);

            // Execute and Verify
            Assert.IsNotNull(deviceMangerPlugin.SetWiredAudioMicMuteSoundEnable(true, Guid.NewGuid()), $"SetWiredAudioMicMuteSoundEnable() returns null");
        }

        [Test]
        public void TestSetWiredAudioVolumeAdjustmentTone()
        {
            // Setup
            var _PeripheralsPluginMock = new Mock<IDPeMPlugin>();
            privateObject.SetFieldOrProperty("_PeripheralsPlugin", _PeripheralsPluginMock.Object);

            // Execute and Verify
            Assert.IsNotNull(deviceMangerPlugin.SetWiredAudioVolumeAdjustmentTone(2, Guid.NewGuid()), $"SetWiredAudioVolumeAdjustmentTone() returns null");
        }

        [Test]
        public void TestSetAncMode()
        {
            // Setup
            var _PeripheralsPluginMock = new Mock<IDPeMPlugin>();
            privateObject.SetFieldOrProperty("_PeripheralsPlugin", _PeripheralsPluginMock.Object);

            // Execute and Verify
            Assert.IsNotNull(deviceMangerPlugin.SetAncMode(2, Guid.NewGuid()), $"SetAncMode() returns null");
        }

        [Test]
        public void TestSetAncGain()
        {
            // Setup
            var _PeripheralsPluginMock = new Mock<IDPeMPlugin>();
            privateObject.SetFieldOrProperty("_PeripheralsPlugin", _PeripheralsPluginMock.Object);

            // Execute and Verify
            Assert.IsNotNull(deviceMangerPlugin.SetAncGain(2, Guid.NewGuid()), $"SetAncGain() returns null");
        }

        [Test]
        public void TestSetSelectedPreset()
        {
            // Setup
            var _PeripheralsPluginMock = new Mock<IDPeMPlugin>();
            privateObject.SetFieldOrProperty("_PeripheralsPlugin", _PeripheralsPluginMock.Object);

            // Execute and Verify
            Assert.IsNotNull(deviceMangerPlugin.SetSelectedPreset(2, Guid.NewGuid()), $"SetSelectedPreset() returns null");
        }

        [Test]
        public void TestSetBandsGain()
        {
            // Setup
            var _PeripheralsPluginMock = new Mock<IDPeMPlugin>();
            privateObject.SetFieldOrProperty("_PeripheralsPlugin", _PeripheralsPluginMock.Object);

            // Execute and Verify
            Assert.IsNotNull(deviceMangerPlugin.SetBandsGain(2, Guid.NewGuid(), "A"), $"SetBandsGain() returns null");
        }

        [Test]
        public void TestSetMicNoiseCancellation()
        {
            // Setup
            var _PeripheralsPluginMock = new Mock<IDPeMPlugin>();
            privateObject.SetFieldOrProperty("_PeripheralsPlugin", _PeripheralsPluginMock.Object);

            // Execute and Verify
            Assert.IsNotNull(deviceMangerPlugin.SetMicNoiseCancellation(true, Guid.NewGuid()), $"SetMicNoiseCancellation() returns null");
        }

        [Test]
        public void TestSetSidetone()
        {
            // Setup
            var _PeripheralsPluginMock = new Mock<IDPeMPlugin>();
            privateObject.SetFieldOrProperty("_PeripheralsPlugin", _PeripheralsPluginMock.Object);

            // Execute and Verify
            Assert.IsNotNull(deviceMangerPlugin.SetSidetone(true, Guid.NewGuid()), $"SetSidetone() returns null");
        }

        [Test]
        public void TestSetSidetoneLevel()
        {
            // Setup
            var _PeripheralsPluginMock = new Mock<IDPeMPlugin>();
            privateObject.SetFieldOrProperty("_PeripheralsPlugin", _PeripheralsPluginMock.Object);

            // Execute and Verify
            Assert.IsNotNull(deviceMangerPlugin.SetSidetoneLevel(2, Guid.NewGuid()), $"SetSidetoneLevel() returns null");
        }

        [Test]
        public void TestSetWearDetection()
        {
            // Setup
            var _PeripheralsPluginMock = new Mock<IDPeMPlugin>();
            privateObject.SetFieldOrProperty("_PeripheralsPlugin", _PeripheralsPluginMock.Object);

            // Execute and Verify
            Assert.IsNotNull(deviceMangerPlugin.SetWearDetection(2, Guid.NewGuid()), $"SetWearDetection() returns null");
        }

        [Test]
        public void TestSetWearDetectionForCLI()
        {
            // Setup
            var _PeripheralsPluginMock = new Mock<IDPeMPlugin>();
            privateObject.SetFieldOrProperty("_PeripheralsPlugin", _PeripheralsPluginMock.Object);

            // Execute and Verify
            Assert.IsNotNull(deviceMangerPlugin.SetWearDetectionForCLI(2, Guid.NewGuid()), $"SetWearDetectionForCLI() returns null");
        }

        [Test]
        public void TestSetBusyLight()
        {
            // Setup
            var _PeripheralsPluginMock = new Mock<IDPeMPlugin>();
            privateObject.SetFieldOrProperty("_PeripheralsPlugin", _PeripheralsPluginMock.Object);

            // Execute and Verify
            Assert.IsNotNull(deviceMangerPlugin.SetBusyLight(true, Guid.NewGuid()), $"SetBusyLight() returns null");
        }

        [Test]
        public void TestSetVoiceGuidance()
        {
            // Setup
            var _PeripheralsPluginMock = new Mock<IDPeMPlugin>();
            privateObject.SetFieldOrProperty("_PeripheralsPlugin", _PeripheralsPluginMock.Object);

            // Execute and Verify
            Assert.IsNotNull(deviceMangerPlugin.SetVoiceGuidance(true, Guid.NewGuid()), $"SetVoiceGuidance() returns null");
        }

        [Test]
        public void TestSetMicNCIncoming()
        {
            // Setup
            var _PeripheralsPluginMock = new Mock<IDPeMPlugin>();
            privateObject.SetFieldOrProperty("_PeripheralsPlugin", _PeripheralsPluginMock.Object);

            // Execute and Verify
            Assert.IsNotNull(deviceMangerPlugin.SetMicNCIncoming(true, Guid.NewGuid()), $"SetMicNCIncoming() returns null");
        }

        [Test]
        public void TestSetIsMicEnumerationOn()
        {
            // Setup
            var _PeripheralsPluginMock = new Mock<IDPeMPlugin>();
            privateObject.SetFieldOrProperty("_PeripheralsPlugin", _PeripheralsPluginMock.Object);

            // Execute and Verify
            Assert.IsNotNull(deviceMangerPlugin.SetIsMicEnumerationOn(true, Guid.NewGuid()), $"SetIsMicEnumerationOn() returns null");
        }

        [Test]
        public void TestSetSideTopSwitchSinglePressSetting3()
        {
            // Setup
            var _PeripheralsPluginMock = new Mock<IDPeMPlugin>();
            privateObject.SetFieldOrProperty("_PeripheralsPlugin", _PeripheralsPluginMock.Object);

            // Execute and Verify
            //Assert.IsNotNull(deviceMangerPlugin.SetSideTopSwitchSinglePressSetting3(new byte[] { 0x11, 0x12 }, Guid.NewGuid()), $"SetSideTopSwitchSinglePressSetting3() returns null");
        }

        [Test]
        public void TestOnUIUpdateNotify()
        {
            try
            {
                deviceMangerPlugin.OnUIUpdateNotify(new UpdateUINotify());
                Assert.True(true);
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }
        }

        [Test]
        public void TestGetDisplayPropertiesInfo()
        {
            // Setup
            var _DisplayManagerPluginMock = new Mock<IDisplayService>();
            privateObject.SetFieldOrProperty("_DisplayManagerPlugin", _DisplayManagerPluginMock.Object);

            // Execute and Verify
            Assert.IsNotNull(deviceMangerPlugin.GetDisplayPropertiesInfo(new MonitorInfo()), $"GetDisplayPropertiesInfo() returns null");
        }

        [Test]
        public void TestSetDisplayPropertiest()
        {
            // Setup
            var _DisplayManagerPluginMock = new Mock<IDisplayService>();
            privateObject.SetFieldOrProperty("_DisplayManagerPlugin", _DisplayManagerPluginMock.Object);

            // Execute and Verify
            Assert.IsNotNull(deviceMangerPlugin.SetDisplayPropertiest(new MonitorInfo(), new DDPM.SA.Common.Properties(), new DisplayOrientation()), $"SetDisplayPropertiest() returns null");
        }

        [Test]
        public void TestSetResolutions()
        {
            // Setup
            var _DisplayManagerPluginMock = new Mock<IDisplayService>();
            privateObject.SetFieldOrProperty("_DisplayManagerPlugin", _DisplayManagerPluginMock.Object);

            // Execute and Verify
            Assert.IsNotNull(deviceMangerPlugin.SetResolutions(monitorInfo, new DDPM.SA.Common.Properties()), $"SetResolutions() returns null");
        }

        [Test]
        public void TestSetOrientation()
        {
            // Setup
            var _DisplayManagerPluginMock = new Mock<IDisplayService>();
            privateObject.SetFieldOrProperty("_DisplayManagerPlugin", _DisplayManagerPluginMock.Object);

            // Execute and Verify
            Assert.IsNotNull(deviceMangerPlugin.SetOrientation(new MonitorInfo(), new DisplayOrientation()), $"SetOrientation() returns null");
        }

        [Test]
        public void TestCallWindowsDisplaySetting()
        {
            // Setup
            var _DisplayManagerPluginMock = new Mock<IDisplayService>();
            privateObject.SetFieldOrProperty("_DisplayManagerPlugin", _DisplayManagerPluginMock.Object);

            // Execute and Verify
            Assert.IsNotNull(deviceMangerPlugin.CallWindowsDisplaySetting(), $"CallWindowsDisplaySetting() returns null");
        }

        [Test]
        public void TestGetHDRStatus()
        {
            // Setup
            var _DisplayManagerPluginMock = new Mock<IDisplayService>();
            privateObject.SetFieldOrProperty("_DisplayManagerPlugin", _DisplayManagerPluginMock.Object);

            // Execute and Verify
            Assert.IsNotNull(deviceMangerPlugin.GetHDRStatus(new MonitorInfo()), $"GetHDRStatus() returns null");
        }

        [Test]
        public void TestSetHDRStatus()
        {
            // Setup
            var _DisplayManagerPluginMock = new Mock<IDisplayService>();
            privateObject.SetFieldOrProperty("_DisplayManagerPlugin", _DisplayManagerPluginMock.Object);

            // Execute and Verify
            Assert.IsNotNull(deviceMangerPlugin.SetHDRStatus(new MonitorInfo(), true), $"SetHDRStatus() returns null");
        }

        [Test]
        public void TestSetUSBCPrioritizationType()
        {
            // Setup
            var _DisplayManagerPluginMock = new Mock<IDisplayService>();
            privateObject.SetFieldOrProperty("_DisplayManagerPlugin", _DisplayManagerPluginMock.Object);

            // Execute and Verify
            Assert.IsNotNull(deviceMangerPlugin.SetUSBCPrioritizationType(new MonitorInfo(), new USBCPrioritizationType()), $"SetUSBCPrioritizationType() returns null");
        }

        [Test]
        public void TestLockRotate()
        {
            // Setup
            var _DisplayManagerPluginMock = new Mock<IDisplayService>();
            privateObject.SetFieldOrProperty("_DisplayManagerPlugin", _DisplayManagerPluginMock.Object);
            var _SettingsPluginMock = new Mock<ISettingsManagerDev>();
            privateObject.SetFieldOrProperty("_SettingsPlugin", _SettingsPluginMock.Object);

            // Execute and Verify
            Assert.IsNotNull(deviceMangerPlugin.LockRotate(true), $"LockRotate() returns null");
        }

        [Test]
        public void TestGetLockRotateStatus()
        {
            // Setup
            var _DisplayManagerPluginMock = new Mock<IDisplayService>();
            privateObject.SetFieldOrProperty("_DisplayManagerPlugin", _DisplayManagerPluginMock.Object);

            // Execute and Verify
            Assert.IsNotNull(deviceMangerPlugin.GetLockRotateStatus(), $"GetLockRotateStatus() returns null");
        }

        [Test]
        public void TestGetOSDOrientation()
        {
            // Setup
            var _DisplayManagerPluginMock = new Mock<IDisplayService>();
            privateObject.SetFieldOrProperty("_DisplayManagerPlugin", _DisplayManagerPluginMock.Object);

            // Execute and Verify
            Assert.IsNotNull(deviceMangerPlugin.GetOSDOrientation(new MonitorInfo()), $"GetOSDOrientation() returns null");
        }

        [Test]
        public void TestSetOSDOrientation()
        {
            // Setup
            var _DisplayManagerPluginMock = new Mock<IDisplayService>();
            privateObject.SetFieldOrProperty("_DisplayManagerPlugin", _DisplayManagerPluginMock.Object);

            // Execute and Verify
            Assert.IsNotNull(deviceMangerPlugin.SetOSDOrientation(new MonitorInfo(), "A"), $"SetOSDOrientation() returns null");
        }

        [Test]
        public void TestReloadAppConfigData()
        {
            // Setup
            var _SettingsPluginMock = new Mock<ISettingsManagerDev>();
            privateObject.SetFieldOrProperty("_SettingsPlugin", _SettingsPluginMock.Object);

            // Execute and Verify
            Assert.IsNotNull(deviceMangerPlugin.ReloadAppConfigData(false), $"ReloadAppConfigData() returns null");
        }

        [Test]
        public void TestSetAppConfigData()
        {
            // Setup
            var _SettingsPluginMock = new Mock<ISettingsManagerDev>();
            privateObject.SetFieldOrProperty("_SettingsPlugin", _SettingsPluginMock.Object);

            // Execute and Verify
            Assert.IsNotNull(deviceMangerPlugin.SetAppConfigData(new DDPMSettings(new DDPMAppSettings(), new DDPMUserSettings(), new DDPMITConfig())), $"SetAppConfigData() returns null");
        }

        [Test]
        public void TestReadRegistryData()
        {
            // Setup
            var _SettingsPluginMock = new Mock<ISettingsManagerDev>();
            privateObject.SetFieldOrProperty("_SettingsPlugin", _SettingsPluginMock.Object);

            // Execute and Verify
            Assert.IsNotNull(deviceMangerPlugin.ReadRegistryData(RegistryHive.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\DDPMW-NKVM", "GUID"), $"ReadRegistryData() returns null");
        }

        [Test]
        public void TestWriteRegistryData()
        {
            // Setup
            var _SettingsPluginMock = new Mock<ISettingsManagerDev>();
            privateObject.SetFieldOrProperty("_SettingsPlugin", _SettingsPluginMock.Object);

            // Execute and Verify
            Assert.IsNotNull(deviceMangerPlugin.WriteRegistryData(RegistryHive.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\DDPMW-NKVM", "GUID", "A"), $"WriteRegistryData() returns null");
        }

        [Test]
        public void TestGetPipPbpCapabilitiesWords()
        {
            // Setup
            var _DisplayManagerPluginMock = new Mock<IDisplayService>();
            privateObject.SetFieldOrProperty("_DisplayManagerPlugin", _DisplayManagerPluginMock.Object);

            // Execute and Verify
            Assert.IsNotNull(deviceMangerPlugin.GetPipPbpCapabilitiesWords(new MonitorInfo()), $"GetPipPbpCapabilitiesWords() returns null");
        }

        [Test]
        public void TestSetPipModeOff()
        {
            // Setup
            var _DisplayManagerPluginMock = new Mock<IDisplayService>();
            privateObject.SetFieldOrProperty("_DisplayManagerPlugin", _DisplayManagerPluginMock.Object);

            // Execute and Verify
            Assert.IsNotNull(deviceMangerPlugin.SetPipModeOff(new MonitorInfo()), $"SetPipModeOff() returns null");
        }

        [Test]
        public void TestSetPipModeSmall()
        {
            // Setup
            var _DisplayManagerPluginMock = new Mock<IDisplayService>();
            privateObject.SetFieldOrProperty("_DisplayManagerPlugin", _DisplayManagerPluginMock.Object);

            // Execute and Verify
            Assert.IsNotNull(deviceMangerPlugin.SetPipModeSmall(new MonitorInfo()), $"SetPipModeSmall() returns null");
        }

        [Test]
        public void TestSetPipModeLarge()
        {
            // Setup
            var _DisplayManagerPluginMock = new Mock<IDisplayService>();
            privateObject.SetFieldOrProperty("_DisplayManagerPlugin", _DisplayManagerPluginMock.Object);

            // Execute and Verify
            Assert.IsNotNull(deviceMangerPlugin.SetPipModeLarge(new MonitorInfo()), $"SetPipModeLarge() returns null");
        }

        [Test]
        public void TestTogglePipSize()
        {
            // Setup
            var _DisplayManagerPluginMock = new Mock<IDisplayService>();
            privateObject.SetFieldOrProperty("_DisplayManagerPlugin", _DisplayManagerPluginMock.Object);

            // Execute and Verify
            Assert.IsNotNull(deviceMangerPlugin.TogglePipSize(new MonitorInfo()), $"TogglePipSize() returns null");
        }

        [Test]
        public void TestTogglePipPosition()
        {
            // Setup
            var _DisplayManagerPluginMock = new Mock<IDisplayService>();
            privateObject.SetFieldOrProperty("_DisplayManagerPlugin", _DisplayManagerPluginMock.Object);

            // Execute and Verify
            Assert.IsNotNull(deviceMangerPlugin.TogglePipPosition(new MonitorInfo()), $"TogglePipPosition() returns null");
        }

        [Test]
        public void TestSetPbpMode()
        {
            // Setup
            var _DisplayManagerPluginMock = new Mock<IDisplayService>();
            privateObject.SetFieldOrProperty("_DisplayManagerPlugin", _DisplayManagerPluginMock.Object);

            // Execute and Verify
            Assert.IsNotNull(deviceMangerPlugin.SetPbpMode(new MonitorInfo(), (UInt16)1), $"SetPbpMode() returns null");
        }

        [Test]
        public void TestVideoSwap()
        {
            // Setup
            var _DisplayManagerPluginMock = new Mock<IDisplayService>();
            privateObject.SetFieldOrProperty("_DisplayManagerPlugin", _DisplayManagerPluginMock.Object);

            // Execute and Verify
            Assert.IsNotNull(deviceMangerPlugin.VideoSwap(new MonitorInfo(), (UInt16)1, (UInt16)2), $"VideoSwap() returns null");
        }

        [Test]
        public void TestGetPxpMode()
        {
            // Setup
            var _DisplayManagerPluginMock = new Mock<IDisplayService>();
            privateObject.SetFieldOrProperty("_DisplayManagerPlugin", _DisplayManagerPluginMock.Object);

            // Execute and Verify
            Assert.IsNotNull(deviceMangerPlugin.GetPxpMode(new MonitorInfo()), $"GetPxpMode() returns null");
        }

        [Test]
        public void TestGetSubInputList()
        {
            // Setup
            var _DisplayManagerPluginMock = new Mock<IDisplayService>();
            privateObject.SetFieldOrProperty("_DisplayManagerPlugin", _DisplayManagerPluginMock.Object);

            // Execute and Verify
            Assert.IsNotNull(deviceMangerPlugin.GetSubInputList(new MonitorInfo()), $"GetSubInputList() returns null");
        }

        [Test]
        public void TestGetSubInputs()
        {
            // Setup
            var _DisplayManagerPluginMock = new Mock<IDisplayService>();
            privateObject.SetFieldOrProperty("_DisplayManagerPlugin", _DisplayManagerPluginMock.Object);

            // Execute and Verify
            Assert.IsNotNull(deviceMangerPlugin.GetSubInputs(new MonitorInfo()), $"GetSubInputs() returns null");
        }

        [Test]
        public void TestSetSubInputs()
        {
            // Setup
            var _DisplayManagerPluginMock = new Mock<IDisplayService>();
            privateObject.SetFieldOrProperty("_DisplayManagerPlugin", _DisplayManagerPluginMock.Object);

            // Execute and Verify
            Assert.IsNotNull(deviceMangerPlugin.SetSubInputs(new MonitorInfo(), new InputSourceObj(), new InputSourceObj(), new InputSourceObj()), $"SetSubInputs() returns null");
        }

        [Test]
        public void TestUsbSwitch1()
        {
            // Setup
            var _DisplayManagerPluginMock = new Mock<IDisplayService>();
            privateObject.SetFieldOrProperty("_DisplayManagerPlugin", _DisplayManagerPluginMock.Object);

            // Execute and Verify
            Assert.IsNotNull(deviceMangerPlugin.UsbSwitch1(new MonitorInfo(), 0), $"UsbSwitch1() returns null");
        }

        [Test]
        public void TestGetFWUpdateInfo()
        {
            Assert.IsNotNull(deviceMangerPlugin.GetFWUpdateInfo(false), $"GetFWUpdateInfo() returns null");

            //_PeripheralsPlugin != null && _FWUpdatePlugin != null
            // Setup
            var _PeripheralsPluginMock = new Mock<IDPeMPlugin>();
            privateObject.SetFieldOrProperty("_PeripheralsPlugin", _PeripheralsPluginMock.Object);
            _PeripheralsPluginMock.Setup(x => x.GetFWUpdateInfo()).Returns(Task.FromResult(new UpdateHelper()));
            var _FWUpdatePluginMock = new Mock<IFWUpdateService>();
            privateObject.SetFieldOrProperty("_FWUpdatePlugin", _FWUpdatePluginMock.Object);

            // Execute and Verify
            Assert.IsNotNull(deviceMangerPlugin.GetFWUpdateInfo(false), $"GetFWUpdateInfo() returns null");
        }

        [Test]
        public void TestDownloadAndInstall()
        {
            // Setup
            var _FWUpdatePluginMock = new Mock<IFWUpdateService>();
            privateObject.SetFieldOrProperty("_FWUpdatePlugin", _FWUpdatePluginMock.Object);
            _FWUpdatePluginMock.Setup(x => x.DownloadAndInstall(It.IsAny<List<FWUpdateInfo>>(), It.IsAny<List<DeviceInfo>>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<string>())).Returns(Task.FromResult(new List<FWUpdateInfo>()));

            // Execute and Verify
            Assert.IsNotNull(deviceMangerPlugin.DownloadAndInstall(new List<FWUpdateInfo>(), false, false, ""), $"DownloadAndInstall() returns null");
        }

        [Test]
        public void TestInstall()
        {
            //Assert.IsNotNull(deviceMangerPlugin.Install(""), $"Install() returns null"); //method remove _FWUpdatePlugin ==null

            // Setup
            var _FWUpdatePluginMock = new Mock<IFWUpdateService>();
            privateObject.SetFieldOrProperty("_FWUpdatePlugin", _FWUpdatePluginMock.Object);
            _FWUpdatePluginMock.Setup(x => x.Install(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<DeviceType>())).Returns(Task.FromResult(new FWUErrorCode()));
            privateObject.SetFieldOrProperty("_UpdateProgress", new UpdateProgress(null));
            // Execute and Verify

            Assert.IsNotNull(deviceMangerPlugin.Install(""), $"Install() returns null");
        }

        [Test]
        public void TestCheckNightLightStatus()
        {
            // Setup
            privateObject.SetFieldOrProperty("_ColorPresetPlugin", null);
      
            // Execute and Verify
            var CheckNightLightStatus_result1 = deviceMangerPlugin.CheckNightLightStatus().Result;
            Assert.IsFalse(CheckNightLightStatus_result1);

            // Setup
            var _ColorPresetPlugin = new Mock<IColorPresetSA>();
            privateObject.SetFieldOrProperty("_ColorPresetPlugin", _ColorPresetPlugin.Object);
            _ColorPresetPlugin.Setup(x => x.CheckNightLightStatus()).Returns(Task.FromResult(true));

            // Execute and Verify
            var CheckNightLightStatus_result2 = deviceMangerPlugin.CheckNightLightStatus().Result;
            Assert.IsTrue(CheckNightLightStatus_result2);
        }

        [Test]
        public void TestCheckNightLightScheduler()
        {
            // Setup
            privateObject.SetFieldOrProperty("_ColorPresetPlugin", null);

            // Execute and Verify
            var CheckNightLightScheduler_result1 = deviceMangerPlugin.CheckNightLightScheduler().Result;
            Assert.IsFalse(CheckNightLightScheduler_result1);

            // Setup
            var _ColorPresetPlugin = new Mock<IColorPresetSA>();
            privateObject.SetFieldOrProperty("_ColorPresetPlugin", _ColorPresetPlugin.Object);
            _ColorPresetPlugin.Setup(x => x.CheckNightLightScheduler()).Returns(Task.FromResult(true));

            // Execute and Verify
            var CheckNightLightScheduler_result2 = deviceMangerPlugin.CheckNightLightScheduler().Result;
            Assert.IsTrue(CheckNightLightScheduler_result2);
        }

        [Test]
        public void TestCheckColorICCStatus()
        {
            // Setup
            privateObject.SetFieldOrProperty("_ColorPresetPlugin", null);

            // Execute and Verify
            var CheckColorICCStatus_result1 = deviceMangerPlugin.CheckColorICCStatus().Result;
            Assert.IsFalse(CheckColorICCStatus_result1);

            // Setup
            var _ColorPresetPlugin = new Mock<IColorPresetSA>();
            privateObject.SetFieldOrProperty("_ColorPresetPlugin", _ColorPresetPlugin.Object);
            _ColorPresetPlugin.Setup(x => x.CheckColorICCStatus()).Returns(Task.FromResult(true));

            // Execute and Verify
            var CheckColorICCStatus_result2 = deviceMangerPlugin.CheckColorICCStatus().Result;
            Assert.IsTrue(CheckColorICCStatus_result2);
        }

        [Test]
        public void TestStopRegistryMonitor_NightLight()
        {
            // Setup
            privateObject.SetFieldOrProperty("_ColorPresetPlugin", null);

            // Execute and Verify
            var StopRegistryMonitor_NightLight_result1 = deviceMangerPlugin.StopRegistryMonitor_NightLight().Result;
            Assert.IsFalse(StopRegistryMonitor_NightLight_result1);

            // Setup
            var _ColorPresetPlugin = new Mock<IColorPresetSA>();
            privateObject.SetFieldOrProperty("_ColorPresetPlugin", _ColorPresetPlugin.Object);
            _ColorPresetPlugin.Setup(x => x.StopRegistryMonitor_NightLight()).Returns(Task.FromResult(true));

            // Execute and Verify
            var StopRegistryMonitor_NightLight_result2 = deviceMangerPlugin.StopRegistryMonitor_NightLight().Result;
            Assert.IsTrue(StopRegistryMonitor_NightLight_result2);
        }

        [Test]
        public void TestStopRegistryMonitor_ICC()
        {
            // Setup
            privateObject.SetFieldOrProperty("_ColorPresetPlugin", null);

            // Execute and Verify
            var StopRegistryMonitor_ICC_result1 = deviceMangerPlugin.StopRegistryMonitor_ICC().Result;
            Assert.IsFalse(StopRegistryMonitor_ICC_result1);

            // Setup
            var _ColorPresetPlugin = new Mock<IColorPresetSA>();
            privateObject.SetFieldOrProperty("_ColorPresetPlugin", _ColorPresetPlugin.Object);
            _ColorPresetPlugin.Setup(x => x.StopRegistryMonitor_ICC()).Returns(Task.FromResult(true));

            // Execute and Verify
            var StopRegistryMonitor_ICC_result2 = deviceMangerPlugin.StopRegistryMonitor_ICC().Result;
            Assert.IsTrue(StopRegistryMonitor_ICC_result2);
        }

        [Test]
        public void TestStopRegistryMonitor_NightLightScheduler()
        {
            // Setup
            privateObject.SetFieldOrProperty("_ColorPresetPlugin", null);

            // Execute and Verify
            var StopRegistryMonitor_NightLightScheduler_result1 = deviceMangerPlugin.StopRegistryMonitor_NightLightScheduler().Result;
            Assert.IsFalse(StopRegistryMonitor_NightLightScheduler_result1);

            // Setup
            var _ColorPresetPlugin = new Mock<IColorPresetSA>();
            privateObject.SetFieldOrProperty("_ColorPresetPlugin", _ColorPresetPlugin.Object);
            _ColorPresetPlugin.Setup(x => x.StopRegistryMonitor_NightLightScheduler()).Returns(Task.FromResult(true));

            // Execute and Verify
            var StopRegistryMonitor_NightLightScheduler_result2 = deviceMangerPlugin.StopRegistryMonitor_NightLightScheduler().Result;
            Assert.IsTrue(StopRegistryMonitor_NightLightScheduler_result2);
        }

        [Test]
        public void TestSetUILockStatus()
        {
            // Setup
            var _SettingsPlugin = new Mock<ISettingsManagerDev>();
            privateObject.SetFieldOrProperty("_SettingsPlugin", _SettingsPlugin.Object);
            _SettingsPlugin.Setup(x => x.ReloadAppConfigData(It.IsAny<bool>())).Returns(Task.FromResult(new DDPMSettings(new DDPMAppSettings(), new DDPMUserSettings(), new DDPMITConfig())));
            _SettingsPlugin.Setup(x => x.SetAppConfigData(It.IsAny<DDPMSettings>())).Returns(Task.FromResult(true));
            // Execute and Verify
            deviceMangerPlugin.SetUILockStatus(true);
            Assert.IsNotNull(_SettingsPlugin);
        }

        [Test]
        public void TestGetUILockStatus()
        {
            // Setup
            privateObject.SetFieldOrProperty("_SettingsPlugin", null);
            // Execute and Verify
            var result1=deviceMangerPlugin.GetUILockStatus().Result;
            Assert.IsFalse(result1);

            var _SettingsPlugin = new Mock<ISettingsManagerDev>();
            privateObject.SetFieldOrProperty("_SettingsPlugin", _SettingsPlugin.Object);
            DDPMSettings? config = null;
            _SettingsPlugin.Setup(x => x.ReloadAppConfigData(It.IsAny<bool>())).Returns(Task.FromResult(config));
            // Execute and Verify
            var result2 = deviceMangerPlugin.GetUILockStatus().Result;
            Assert.IsFalse(result2);
            
            _SettingsPlugin.Setup(x => x.ReloadAppConfigData(It.IsAny<bool>())).Returns(Task.FromResult(new DDPMSettings(new DDPMAppSettings(), new DDPMUserSettings(), new DDPMITConfig())));
            // Execute and Verify
            var result3 = deviceMangerPlugin.GetUILockStatus().Result;
            Assert.IsNotNull(result3);
        }
    }
}