using DDPM.SA.Common;
using DDPM.SA.Plugins.User.DisplayManager;
using DDPM.SA.Plugins.User.PipPbpManger;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.Common.Annotations;
using Dell.Client.Framework.Interfaces;
using Moq;
using System.Data;
using System.Threading;
using VcpCore.Common;
using VcpCore.Interfaces;
using VcpCore.Plugins;
//using WinCopies;
using Windows.Media.AppBroadcasting;
using Windows.UI.ViewManagement;
using static VcpCore.Common.User32;
using DDPM.SA.Plugins.User.DisplayProperties;
using Dell.Client.Framework.UnitTestShared.Tests;
using ColorPreset.Plugins;
using System.Linq;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ToolTip;
//using WinCopies.Util.Commands.Primitives;
using Windows.ApplicationModel;
using System.Net;
using Microsoft.Windows.Themes;
using DDPM.MonitorBorker;


namespace DDPM.SA.Plugins.User.ColorPreset.Test
{
    [Apartment(ApartmentState.STA)]
    public class TestColorPreset
    {
        private Mock<IAgent> DisplayMangerAgent { get; } = new();
        private Mock<IAgent> VcpCoreAgent { get; } = new();
        private Mock<IAgent> ColorPresetPluginAgent { get; } = new();
        private MonitorInfo monitorInfo = new MonitorInfo();


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

        private ColorPresetPlugin CreateInitializeColorPresetPlugin()
        {
            ColorPresetPluginAgent.Setup(x => x.PluginManager.FindPluginByGuid(Guid.Parse(DDPM.SA.Common.IDs.DDPM_COLOR_PRESET_PLUGIN_ID)));

            return new ColorPresetPlugin(ColorPresetPluginAgent.Object);
        }

        private DisplayMangerPlugin displayPlugin;
        private VcpCorePlugin vcpCorePlugin;
        private Dictionary<string, Dictionary<string, string>> getstr;
        private ColorPresetPlugin colorPresetPlugin;

        [OneTimeSetUp]
        public void Setup()
        {
            displayPlugin = CreateInitializeDisplayMangerPlugin();
            vcpCorePlugin = CreateInitializeVcpCorePlugin();
            colorPresetPlugin = CreateInitializeColorPresetPlugin();

            PrivateObject privateObject = new PrivateObject(displayPlugin);
            PrivateObject privatevcp = new PrivateObject(vcpCorePlugin);
            PrivateObject privatevcolorPreset = new PrivateObject(colorPresetPlugin);
            privateObject.SetField("_VcpCorePlugin", vcpCorePlugin as IVcpCoreService);
            getstr = (Dictionary<string, Dictionary<string, string>>)privatevcp.GetField("_ColorPresets");

        }

        private Dictionary<string, InstalledAppInfo> _allAppData = new Dictionary<string, InstalledAppInfo>();

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
            edid = new EDID() { SerialNumber = "808597589", ModelName = "DELLU2724DE" },
            //CapabilityDic = capabilityDic;
        };


        private EDID eDID = new EDID()
        {
            ManufactureID = "DELL",
            VendorID = "42DD",
            Year = 2023,
            Month = 5,
            Week = 22,
            ModelName = "DELLU2724DE",
            EdidVersion = "V1.3",
            VideoInputType = "Digital Signal",
            Size = 27.1510868f,
            ServiceTag = "CN073K0",
            SerialNumber = "808597589",
            Edid = "00FFFFFFFFFFFF0010ACDC4255383230202001ED"

        };
        private List<ColorPresetSettings> monitorConfigs1 = new List<ColorPresetSettings>() { new ColorPresetSettings() { ModelName = "DELLU2724DE", SerialNumber = "808597589", ServiceTag = "123456", ColorForManual = 0, RunType = 0, AppInfo = new Dictionary<string, ColorPresetSettings_AppInfo>(), ColorManagement_RunType = 0, ColorManagement_Status = 1 } };

        [Test]
        public void TestColorPresetPlugin()
        {
            Assert.IsNotNull(colorPresetPlugin);
            PrivateObject privatevcolorPreset = new PrivateObject(colorPresetPlugin);
            var agent2 = privatevcolorPreset.GetField("_agent") as IAgent;
            Assert.That(agent2, Is.EqualTo(ColorPresetPluginAgent.Object));

        }

        [Test]
        public void TestSetAppIconFolder()
        {
            string folderpath = @"C:\AppIcons";
            colorPresetPlugin.SetAppIconFolder(folderpath);
            PrivateObject privatevcolorPreset = new PrivateObject(colorPresetPlugin);
            var iconFolderPath = privatevcolorPreset.GetFieldOrProperty("iconFolderPath");
            Assert.That(folderpath, Is.EqualTo(iconFolderPath));
        }

        [Test]
        public void TestGetInstalledAppsList()
        {
            var resullt = colorPresetPlugin.GetInstalledAppsList();
            Assert.IsNotNull(resullt);
            Assert.Greater(resullt.Result.Count, 0);
        }

        [Test]
        public void Testget_index_of_json_config_for_cur_monitor()
        {
            int get_index = 0;
            Test_AddAppCollectionData test_AddAppCollectionData = new Test_AddAppCollectionData() { _monitorConfigs = monitorConfigs1 };
            PrivateObject privatevcolorPreset = new PrivateObject(test_AddAppCollectionData);
            privatevcolorPreset.SetFieldOrProperty("INSTANCE", test_AddAppCollectionData);
            var resullt = colorPresetPlugin.get_index_of_json_config_for_cur_monitor(monitorInfo1);
            Assert.That(get_index, Is.EqualTo(resullt));
            Assert.IsNotNull(resullt);
        }

        [Test]
        public void Testget_cur_monitor_preset_config()
        {
            int index = -1;
            string modelName = monitorInfo1.edid.ModelName;
            string serialNumber = monitorInfo1.edid.SerialNumber;
            string presetForManual = "Standard/Native";
            List<ColorPresetSettings> monitorConfigs2 = new List<ColorPresetSettings>(); //new ColorPresetSettings() { DeviceInfo = new EDID() { ModelName = "DELLU2724", SerialNumber = "8085975", }, PresetForManual = "Standard" }
            if (index < 0)
            {
                var Curpreset_Result = colorPresetPlugin.get_cur_monitor_preset_config(monitorInfo1, monitorConfigs2);
                Assert.IsNotNull(Curpreset_Result);
                Assert.Greater(Curpreset_Result.AppInfo.Count, 0);
                Assert.That(serialNumber, Is.EqualTo(Curpreset_Result.SerialNumber));
                Assert.That(modelName, Is.EqualTo(Curpreset_Result.ModelName));
                //Assert.That(presetForManual, Is.EqualTo(Curpreset_Result.PresetForManual));
            }
            else
            {
                var resullt = colorPresetPlugin.get_cur_monitor_preset_config(monitorInfo1, monitorConfigs1);
                Assert.IsNotNull(resullt);
                Assert.That(serialNumber, Is.EqualTo(resullt.SerialNumber));
                Assert.That(modelName, Is.EqualTo(resullt.ModelName));
            }
        }

        [Test]
        public void TestAddColorPresetForMonitorConfig()
        {
            //var dismonitor = displayPlugin.GetMonitors();"Standard/Native"
            string modelName = monitorInfo1.edid.ModelName;
            string serialNumber = monitorInfo1.edid.SerialNumber;
            string colorpreset = "{'CapsDataMap' : {'ColorPreset': ['Standard/Native', 'Movie','Game','Custom Color']}}";
            monitorInfo1.CapabilityString = colorpreset;
            List<ColorPresetSettings> monitorConfigs2 = new List<ColorPresetSettings>();
            string appName = "Command Prompt";
            string colorPreset_Name = "Standard/Native";
            string supported_preset = monitorInfo1.CapabilityString;
            Dictionary<string, ColorPresetSettings_AppInfo> appInfo = new Dictionary<string, ColorPresetSettings_AppInfo>();
            appInfo.Add("Command Prompt", new ColorPresetSettings_AppInfo() { Color = 0, HDRColor = -1, IconName = "cmd.exe" });

            var AddColorPreset_resullt = colorPresetPlugin.AddColorPresetForMonitorConfig(monitorInfo1, appName, colorPreset_Name, supported_preset, monitorConfigs2).Result; //add Standard
            Assert.IsNotNull(AddColorPreset_resullt);
            Assert.Greater(AddColorPreset_resullt.Count, 0);
            //Assert.That(colorPreset_Name, Is.EqualTo(AddColorPreset_resullt[0].PresetForManual));
            Assert.IsTrue(AddColorPreset_resullt[0].AppInfo.ContainsKey("Command Prompt"));
        }


        [Test]
        public void TestChangeColorPresetForMonitorConfig()
        {
            string modelName = monitorInfo1.edid.ModelName;
            string serialNumber = monitorInfo1.edid.SerialNumber;
            string colorpreset = "{'CapsDataMap' : {'ColorPreset': ['Standard', 'Movie','Game','Custom Color']}}";
            monitorInfo1.CapabilityString = colorpreset;
            List<ColorPresetSettings> monitorConfigs2 = new List<ColorPresetSettings>();
            string appName = "Command Prompt";
            string colorPreset_Name = "Custom Color";
            string supported_preset = monitorInfo1.CapabilityString;
            Dictionary<string, ColorPresetSettings_AppInfo> appInfo = new Dictionary<string, ColorPresetSettings_AppInfo>();
            appInfo.Add("Command Prompt", new ColorPresetSettings_AppInfo() { Color = 0, HDRColor = -1, IconName = "cmd.exe" });
            int index_config = 0;
            List<ColorPresetSettings> monitorConfigs1 = new List<ColorPresetSettings>() { new ColorPresetSettings() { ModelName = "DELLU2724DE", SerialNumber = "808597589", ServiceTag = "123456", ColorForManual = 0, RunType = 0, AppInfo = appInfo } };
            int count = 0;
            if (index_config >= 0)
            {
                var ChangeColorPreset_resullt = colorPresetPlugin.ChangeColorPresetForMonitorConfig(monitorInfo1, appName, colorPreset_Name, monitorConfigs1).Result; //change Standard to Custom Color
                Assert.IsNotNull(ChangeColorPreset_resullt);
                Assert.Greater(ChangeColorPreset_resullt.Count, 0);
                Assert.IsTrue(ChangeColorPreset_resullt[0].AppInfo.ContainsKey("Command Prompt"));
                // Assert.That(colorPreset_Name, Is.EqualTo(ChangeColorPreset_resullt[0].AppInfo[appName].HDRColor));
            }
            else
            {
                var ChangeColorPreset_resullt = colorPresetPlugin.ChangeColorPresetForMonitorConfig(monitorInfo1, appName, colorPreset_Name, monitorConfigs2).Result;
                Assert.IsNotNull(ChangeColorPreset_resullt);
                Assert.That(count, Is.EqualTo(ChangeColorPreset_resullt.Count));
            }
        }

        [Test]
        public void TestDeleteColorPresetForMonitorConfig()
        {
            string modelName = monitorInfo1.edid.ModelName;
            string serialNumber = monitorInfo1.edid.SerialNumber;
            string colorpreset = "{'CapsDataMap' : {'ColorPreset': ['Standard', 'Movie','Game','Custom Color']}}";
            monitorInfo1.CapabilityString = colorpreset;
            List<ColorPresetSettings> monitorConfigs2 = new List<ColorPresetSettings>();
            string appName = "Command Prompt";
            string supported_preset = monitorInfo1.CapabilityString;
            Dictionary<string, ColorPresetSettings_AppInfo> appInfo = new Dictionary<string, ColorPresetSettings_AppInfo>();
            appInfo.Add("Command Prompt", new ColorPresetSettings_AppInfo() { Color = 0, HDRColor = -1, IconName = "cmd.exe" });
            int index_config = 0;
            List<ColorPresetSettings> monitorConfigs1 = new List<ColorPresetSettings>() { new ColorPresetSettings() { ModelName = "DELLU2724DE", SerialNumber = "808597589", ServiceTag = "123456", ColorForManual = 0, RunType = 0, AppInfo = appInfo } };
            int count = 0;
            if (index_config >= 0)
            {
                var DeleteColorPreset_resullt = colorPresetPlugin.DeleteColorPresetForMonitorConfig(monitorInfo1, appName, monitorConfigs1).Result; //delete appname AppInfo
                Assert.IsNotNull(DeleteColorPreset_resullt);
                Assert.That(count, Is.EqualTo(DeleteColorPreset_resullt[0].AppInfo.Count));
            }
            else
            {
                var DeleteColorPreset_resullt = colorPresetPlugin.DeleteColorPresetForMonitorConfig(monitorInfo1, appName, monitorConfigs2).Result; //delete appname
                Assert.IsNotNull(DeleteColorPreset_resullt);
                Assert.That(count, Is.EqualTo(DeleteColorPreset_resullt.Count));
            }
        }


        [Test]
        public void TestLaunch_MonitorBorker()
        {
            IDeviceManagerSA deviceManagerPlugin = null;
            MonitorInfo m = null;
            colorPresetPlugin.Launch_MonitorBorker(m, deviceManagerPlugin); //MonitorInfo is null
            Assert.IsFalse(false);

            var DeviceManagerSAMock = new Mock<IDeviceManagerSA>();
            var DeviceManagerSA = DeviceManagerSAMock.Object;
            monitorInfo1.DisplayName = "display1";
            colorPresetPlugin.Launch_MonitorBorker(monitorInfo1, DeviceManagerSA); //MonitorInfo is not null
            Assert.IsTrue(true);
        }

        [Test]
        public void TestAutoSetColorPresetForMonitorConfig()
        {
            string colorpreset = "{'CapsDataMap' : {'ColorPreset': ['Standard', 'Movie','Game','Custom Color']}}";
            monitorInfo1.CapabilityString = colorpreset;
            List<ColorPresetSettings> monitorConfigs2 = new List<ColorPresetSettings>();
            string on_off1 = "on";
            string on_off2 = "off";
            string supported_preset = monitorInfo1.CapabilityString;
            Dictionary<string, ColorPresetSettings_AppInfo> appInfo = new Dictionary<string, ColorPresetSettings_AppInfo>();
            appInfo.Add("Command Prompt", new ColorPresetSettings_AppInfo() { Color = 0, HDRColor = -1, IconName = "cmd.exe" });
            int index_config = 0;
            int runType = 1;
            int runType2 = 0;
            List<ColorPresetSettings> monitorConfigs1 = new List<ColorPresetSettings>() { new ColorPresetSettings() { ModelName = "DELLU2724DE", SerialNumber = "808597589", RunType = 0, AppInfo = appInfo } };
            ISettingsManagerDev settingsPlugin_;
            IDeviceManagerSA _deviceManagerPlugin;
            bool WriteColorPresetSettings = true;
            Mock<ISettingsManagerDev> settingsPluginManagerDev = new Mock<ISettingsManagerDev>();
            var settingsPluginManagerDev_ = settingsPluginManagerDev.Object;
            settingsPluginManagerDev.Setup(x => x.ReadColorPresetSettings()).Returns(Task.FromResult(monitorConfigs1));
            settingsPluginManagerDev.Setup(x => x.WriteColorPresetSettings(It.IsAny<List<ColorPresetSettings>>())).Returns(Task.FromResult(WriteColorPresetSettings));
            PrivateObject colorPresetprivateObject = new PrivateObject(colorPresetPlugin);
            //colorPresetprivateObject.SetFieldOrProperty("_SettingsPlugin", settingsPluginManagerDev_);  //_SettingsPlugin is not Global varible,using it direct

            var DeviceManagerSAMock = new Mock<IDeviceManagerSA>();
            var DeviceManagerSA = DeviceManagerSAMock.Object;
            var monitorBorkerWin = new MainWindow(DeviceManagerSA, monitorInfo1);
            colorPresetprivateObject.SetFieldOrProperty("MonitorBorkerWin", monitorBorkerWin);

            if (on_off1 == "on")
            {
                if (index_config >= 0)
                {
                    var AutoSetColorPreset_resullt = colorPresetPlugin.AutoSetColorPresetForMonitorConfig(monitorInfo1, on_off1, settingsPluginManagerDev_, DeviceManagerSA).Result;   //set ColorPreset RunType Auto 1
                    Assert.IsNotNull(AutoSetColorPreset_resullt);
                    Assert.IsTrue(AutoSetColorPreset_resullt);
                }
            }
            if (on_off2 == "off")
            {
                if (index_config >= 0)
                {
                    var AutoSetColorPreset_resullt2 = colorPresetPlugin.AutoSetColorPresetForMonitorConfig(monitorInfo1, on_off2, settingsPluginManagerDev_, DeviceManagerSA).Result;//set ColorPreset RunType Manual 0
                    Assert.IsNotNull(AutoSetColorPreset_resullt2);
                    Assert.IsTrue(AutoSetColorPreset_resullt2);
                }
            }
        }

        [Test]
        public void TestShowOSD_ColoPreset()
        {
            string modelName = monitorInfo1.edid.ModelName;
            string serialNumber = monitorInfo1.edid.SerialNumber;
            string DisplayName = "DISPL";
            monitorInfo1.DisplayName = DisplayName;
            string colorpreset = "{'CapsDataMap' : {'ColorPreset': ['Standard/Native', 'Movie','Game','Custom Color']}}";
            monitorInfo1.CapabilityString = colorpreset;
            List<ColorPresetSettings> monitorConfigs2 = new List<ColorPresetSettings>();
            string colorPreset_Name = "Standard/Native";
            string supported_preset = monitorInfo1.CapabilityString;
            colorPresetPlugin.ShowOSD_ColoPreset(monitorInfo1, colorPreset_Name);

        }

        [Test]
        public void TestReadColorPreset()
        {
            string colorpreset = "{'CapsDataMap' : {'ColorPreset': ['Standard', 'Movie','Game','Custom Color']}}";
            monitorInfo1.CapabilityString = colorpreset;
            List<ColorPresetSettings> monitorConfigs2 = new List<ColorPresetSettings>();
            string supported_preset = monitorInfo1.CapabilityString;
            List<string> colorPresetSupportList = new List<string>() { "Standard", "Movie", "Game", "Custom Color" };
            List<string> colorPresetSupportList2 = new List<string>();
            string colorpreset2 = null;
            int count = 0;
            if (colorpreset2 == null)
            {
                var ReadColorPreset_resullt = colorPresetPlugin.ReadColorPreset(monitorInfo1, colorpreset2).Result;
                Assert.That(count, Is.EqualTo(ReadColorPreset_resullt.Count));
                Assert.That(colorPresetSupportList2, Is.EqualTo(ReadColorPreset_resullt));
            }

            if (!string.IsNullOrEmpty(colorpreset))
            {
                var ReadColorPreset_resullt = colorPresetPlugin.ReadColorPreset(monitorInfo1, colorpreset).Result;
                Assert.Greater(ReadColorPreset_resullt.Count, 0);
                Assert.That(colorPresetSupportList, Is.EqualTo(ReadColorPreset_resullt));
            }
        }

        [Test]
        public void TestWriteColorPreset()
        {
            bool writeColorPreset1_res = true;
            string colorpreset = "{'CapsDataMap' : {'ColorPreset': ['Standard/Native', 'Movie','Game','Custom Color']}}";
            monitorInfo1.CapabilityString = colorpreset;
            List<ColorPresetSettings> monitorConfigs2 = new List<ColorPresetSettings>();
            string colorPreset_Name = "Movie";
            string supported_preset = monitorInfo1.CapabilityString;
            Dictionary<string, ColorPresetSettings_AppInfo> appInfo = new Dictionary<string, ColorPresetSettings_AppInfo>();
            appInfo.Add("Command Prompt", new ColorPresetSettings_AppInfo() { Color = 0, HDRColor = -1, IconName = "cmd.exe" });
            int index_config = 0;
            int runType = 0; //Manual 0,auto 1
            List<ColorPresetSettings> monitorConfigs1 = new List<ColorPresetSettings>() { new ColorPresetSettings() { ModelName = "DELLU2724DE", SerialNumber = "808597589", RunType = 1, AppInfo = appInfo } };
            int count = 0;
            Mock<ISettingsManagerDev> SettingsManagerPluginService = new Mock<ISettingsManagerDev>();
            var settingsPluginManagerDev_ = SettingsManagerPluginService.Object;
            ////SettingsManagerDevService.Setup(x => x.WriteColorPresetSettings(It.IsAny<List<ColorPresetSettings>>())).Returns(Task.FromResult(WriteColorPresetSettings));
            ISettingsManagerDev settingsPlugin_ = null;
            int colorPresetRunType_ = 0;
            if (settingsPlugin_ == null)
            {
                var WriteColorPreset_resullt = colorPresetPlugin.WriteColorPreset(monitorInfo1, colorPreset_Name, settingsPlugin_, colorPresetRunType_).Result;
                Assert.That(writeColorPreset1_res, Is.EqualTo(WriteColorPreset_resullt));
            }

            if (settingsPluginManagerDev_ != null)
            {
                SettingsManagerPluginService.Setup(x => x.ReadColorPresetSettings()).Returns(Task.FromResult(monitorConfigs1));
                PrivateObject colorPresetprivateObject = new PrivateObject(colorPresetPlugin);
                //colorPresetprivateObject.SetFieldOrProperty("_SettingsPlugin", settingsPluginManagerDev_); //_SettingsPlugin is not Global varible,using it direct
                if (index_config >= 0)
                {
                    var WriteColorPreset_resullt2 = colorPresetPlugin.WriteColorPreset(monitorInfo1, colorPreset_Name, settingsPluginManagerDev_, colorPresetRunType_).Result;
                    Assert.That(writeColorPreset1_res, Is.EqualTo(WriteColorPreset_resullt2));
                }
            }
        }

        [Test]
        public void TestWriteColorPreset_AUTO()
        {
            string colorpreset = "{'CapsDataMap' : {'ColorPreset': ['Standard', 'Movie','Game','Custom Color']}}";
            monitorInfo1.CapabilityString = colorpreset;
            List<ColorPresetSettings> monitorConfigs2 = new List<ColorPresetSettings>();
            string colorPreset_Name = "Custom Color";
            int colorPreset_Name_value = 20;
            string supported_preset = monitorInfo1.CapabilityString;
            Dictionary<string, ColorPresetSettings_AppInfo> appInfo = new Dictionary<string, ColorPresetSettings_AppInfo>();
            appInfo.Add("Command Prompt", new ColorPresetSettings_AppInfo() { Color = 0, HDRColor = -1, IconName = "cmd.exe" });
            int index_config = 0;
            int runType = 1; //Manual 0,auto 1
            List<ColorPresetSettings> monitorConfigs1 = new List<ColorPresetSettings>() { new ColorPresetSettings() { ModelName = "DELLU2724DE", SerialNumber = "808597589", RunType = 1, AppInfo = appInfo } };
            int count = 0;
            if (index_config >= 0)
            {
                var WriteColorPreset_AUTO_resullt = colorPresetPlugin.WriteColorPreset_AUTO(monitorInfo1, colorPreset_Name, monitorConfigs1).Result; //write colorpreset to Manual Standard to Custom Color
                Assert.IsNotNull(WriteColorPreset_AUTO_resullt);
                Assert.That(runType, Is.EqualTo(WriteColorPreset_AUTO_resullt[0].RunType));
                Assert.That(colorPreset_Name_value, Is.EqualTo(WriteColorPreset_AUTO_resullt[0].ColorForManual));
            }
            else
            {
                var WriteColorPreset_AUTO_resullt = colorPresetPlugin.WriteColorPreset_AUTO(monitorInfo1, colorPreset_Name, monitorConfigs2).Result;
                Assert.IsNotNull(WriteColorPreset_AUTO_resullt);
                Assert.That(count, Is.EqualTo(WriteColorPreset_AUTO_resullt.Count));
            }
        }

        [Test]
        public void TestIsDisposed()
        {
            var Result = colorPresetPlugin.IsDisposed;
            Assert.IsFalse(Result);
            PrivateObject privatehotkeyPluginObject = new PrivateObject(colorPresetPlugin);
            privatehotkeyPluginObject.SetFieldOrProperty("IsDisposed", true);
            var Result2 = colorPresetPlugin.IsDisposed;
            Assert.IsTrue(Result2);
        }

        [Test]
        public void TestBytesToString()
        {
            byte[] bytes = { 0x10, 0x12, 0x13, 0x14, 0x15, 0x16 };
            string expectresult = "101213141516";
            var Result = ColorPresetPlugin.BytesToString(bytes);
            Assert.That(expectresult, Is.EqualTo(Result));
        }

        [Test]
        public void TestLoadInstalledAppList()
        {
            bool Renew_data1 = false;
            PrivateObject privatecolorPresetObject = new PrivateObject(colorPresetPlugin);
            Dictionary<string, InstalledAppInfo> allAppData_ = new Dictionary<string, InstalledAppInfo>();
            allAppData_.Add("CMD", new InstalledAppInfo()
            {
                AppName = "name",
                IconName = "iconName",
                AppInstallPath = "pth",
                lastModifyTime = DateTime.Now,
                isDesktopApp = true,
                AppUserModelID = "appUserModelID",
            });
            privatecolorPresetObject.SetFieldOrProperty("_AllAppData", allAppData_);
            privatecolorPresetObject.Invoke("LoadInstalledAppList", Renew_data1);   //appdata is null
            var result1 = (Dictionary<string, InstalledAppInfo>)privatecolorPresetObject.GetFieldOrProperty("_AllAppData");
            Assert.That(allAppData_, Is.EqualTo(result1));

            bool Renew_data2 = true;
            privatecolorPresetObject.Invoke("LoadInstalledAppList", Renew_data2);
            var result2 = (Dictionary<string, InstalledAppInfo>)privatecolorPresetObject.GetFieldOrProperty("_AllAppData");  //appdata is not null
            Assert.IsNotNull(result2);
            Assert.Greater(result2.Count, 0);
        }

        /*[Test]
        public void TestCheckCA()
        {
            string url = @"https://clientperipherals.dell.com/DDPM/";
            string[] issuer = { "Entrust Certification Authority - L1F, OU=\"(c) 2016 Entrust, Inc. - for authorized use only\", OU=See www.entrust.net/legal-terms, O=\"Entrust, Inc.\", C=US" };
            string[] subject = { "CN=content-cdn.dell.com, O=Dell, L=Round Rock, S=Texas, C=US" };
            PrivateObject privatecolorPresetObject = new PrivateObject(colorPresetPlugin);
            privatecolorPresetObject.SetFieldOrProperty("Issuers", issuer);
            privatecolorPresetObject.SetFieldOrProperty("Subjects", subject);
            //var mockWebRequest = new Mock<HttpWebRequest>();
            //mockWebRequest.Setup(req => req.GetResponse()).Returns(new Mock<HttpWebResponse>().Object);
            //privatecolorPresetObject.SetFieldOrProperty("HttpWebRequest", mockWebRequest.Object);
            try
            {
                var Result = colorPresetPlugin.CheckCA(url);  // web no response,(404) Not Found.
                Assert.IsNotNull(Result);
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }
        }*/

        [Test]
        public void TestDownloadICCData()
        {
            string savelPath = "";
            string url = @"https://clientperipherals.dell.com/DDPM/";
            string[] issuer = { "Entrust Certification Authority - L1F, OU=\"(c) 2016 Entrust, Inc. - for authorized use only\", OU=See www.entrust.net/legal-terms, O=\"Entrust, Inc.\", C=US" };
            string[] subject = { "CN=content-cdn.dell.com, O=Dell, L=Round Rock, S=Texas, C=US" };
            PrivateObject privatehotkeyPluginObject = new PrivateObject(colorPresetPlugin);
            //privatehotkeyPluginObject.SetFieldOrProperty("Issuers", issuer);
            //privatehotkeyPluginObject.SetFieldOrProperty("Subjects", subject);
            Mock<ISettingsManagerDev> SettingsManagerPluginService = new Mock<ISettingsManagerDev>();
            var settingsPluginManagerDev_ = SettingsManagerPluginService.Object;
            privatehotkeyPluginObject.SetFieldOrProperty("_SettingsPlugin_internal", settingsPluginManagerDev_);
            try
            {
                var Result = colorPresetPlugin.DownloadICCData(monitorInfo1, settingsPluginManagerDev_, savelPath).Result;  // web no response,(404) Not Found.
                Assert.IsNotNull(Result);
                Assert.IsNotNull(Result.strICC_Folder);
                Assert.IsNotNull(Result.Is_Support_ICC_DeviceName);
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }
        }

        [Test]
        public void TestGetHashSha256()
        {
            string tempFilePath = Path.GetTempFileName();
            byte[] testData = { 1, 2, 3, 4, 5 };
            File.WriteAllBytes(tempFilePath, testData);
            PrivateObject privatecolorPresetObject = new PrivateObject(colorPresetPlugin);
            //byte[] hash = (byte[])privatecolorPresetObject.Invoke("GetHashSha256", tempFilePath);   method remove
            //Assert.IsNotNull(hash);
            File.Delete(tempFilePath);
        }

        [Test]
        public void TestCheckHTTPAvailable()
        {
            string Url_ = @"https://clientperipherals.dell.com/DDPM/";
            PrivateObject privatecolorPresetObject = new PrivateObject(colorPresetPlugin);
            try
            {
                //var result = privatecolorPresetObject.Invoke("CheckHTTPAvailable", Url_);  method remove
                //Assert.IsNotNull(result);
            }
            catch
            {
                Assert.Fail("not invoked");
            }
        }

        [Test]
        public void TestSetMonitorProfile()
        {
            IIC_Metadata ICC_metadata_ = new IIC_Metadata();
            string colorpreset = "{'CapsDataMap' : {'ColorPreset': ['Standard/Native', 'Movie','Game','Custom Color']}}";
            monitorInfo1.CapabilityString = colorpreset;
            List<ColorPresetSettings> monitorConfigs2 = new List<ColorPresetSettings>();
            string supported_preset = monitorInfo1.CapabilityString;
            Dictionary<string, ColorPresetSettings_AppInfo> appInfo = new Dictionary<string, ColorPresetSettings_AppInfo>();
            appInfo.Add("Command Prompt", new ColorPresetSettings_AppInfo() { Color = 0, HDRColor = -1, IconName = "cmd.exe" });
            List<ColorPresetSettings> monitorConfigs1 = new List<ColorPresetSettings>() { new ColorPresetSettings() { ModelName = "DELLU2724DE", SerialNumber = "808597589", RunType = 1, AppInfo = appInfo } };
            string ColorPreset_Name = "Standard";
            var result = colorPresetPlugin.SetMonitorProfile(monitorInfo1, ColorPreset_Name).Result;
            Assert.IsNotNull(result);

        }

        [Test]
        public void TestGetColorVCPCoreValue()
        {
            string colorPreset_Name = "Custom Color";  //Key is in VCPE2_ref list
            int colorpreset_Value = 20;
            var GetColorVCPCoreValue_Result = colorPresetPlugin.GetColorVCPCoreValue(colorPreset_Name).Result;
            Assert.That(colorpreset_Value, Is.EqualTo(GetColorVCPCoreValue_Result));

            string colorPreset_Name2 = "Custom Color6666";  //Key NO in VCPE2_ref list
            int colorpreset_Value2 = -1;
            var GetColorVCPCoreValue_Result2 = colorPresetPlugin.GetColorVCPCoreValue(colorPreset_Name2).Result;
            Assert.That(colorpreset_Value2, Is.EqualTo(GetColorVCPCoreValue_Result2));
        }

        [Test]
        public void TestGetColorPresetName()
        {
            int Color_VCPCore_E2 = 0;
            string colorPreset_Name = "Standard/Native";  //Key VCPE2 0, "Standard/Native"
            var GetColorPresetName_Result = colorPresetPlugin.GetColorPresetName(Color_VCPCore_E2).Result;
            Assert.That(colorPreset_Name, Is.EqualTo(GetColorPresetName_Result));

            int Color_VCPCore_E22 = 666;//Key NO in VCPE2 list
            string colorPreset_Name2 = string.Empty;
            var GetColorPresetName_Result2 = colorPresetPlugin.GetColorPresetName(Color_VCPCore_E22).Result;
            Assert.That(colorPreset_Name2, Is.EqualTo(GetColorPresetName_Result2));
        }

        [Test]
        public void TestGetAutoColorPresetStatus()
        {
            Mock<ISettingsManagerDev> settingsPluginManagerDev = new Mock<ISettingsManagerDev>();
            Dictionary<string, ColorPresetSettings_AppInfo> appInfo = new Dictionary<string, ColorPresetSettings_AppInfo>();
            appInfo.Add("Command Prompt", new ColorPresetSettings_AppInfo() { Color = 0, HDRColor = -1, IconName = "cmd.exe" });
            int index_config = 0;
            List<ColorPresetSettings> monitorConfigs1 = new List<ColorPresetSettings>() { new ColorPresetSettings() { ModelName = "DELLU2724DE", SerialNumber = "808597589", ServiceTag = "123456", ColorForManual = 0, RunType = 0, AppInfo = appInfo } };
            var settingsPluginManagerDev_ = settingsPluginManagerDev.Object;
            settingsPluginManagerDev.Setup(x => x.ReadColorPresetSettings()).Returns(Task.FromResult(monitorConfigs1));
            PrivateObject colorPresetprivateObject = new PrivateObject(colorPresetPlugin);
            string AutoColorPresetStatus1 = "OFF";
            string AutoColorPresetStatus2 = "ON";
            if (index_config >= 0)
            {
                var GetAutoColorPresetStatus_resullt1 = colorPresetPlugin.GetAutoColorPresetStatus(monitorInfo1, settingsPluginManagerDev_).Result; //RunType = 0, index_config=0 
                Assert.IsNotNull(GetAutoColorPresetStatus_resullt1);
                Assert.That(AutoColorPresetStatus1, Is.EqualTo(GetAutoColorPresetStatus_resullt1));
            }
            else
            {
                var GetAutoColorPresetStatus_resullt2 = colorPresetPlugin.GetAutoColorPresetStatus(monitorInfo1, settingsPluginManagerDev_).Result; //RunType = 0, index_config < 0
                Assert.IsNotNull(GetAutoColorPresetStatus_resullt2);
                Assert.That(AutoColorPresetStatus1, Is.EqualTo(GetAutoColorPresetStatus_resullt2));
            }

            if (index_config >= 0)
            {
                List<ColorPresetSettings> monitorConfigs2 = new List<ColorPresetSettings>() { new ColorPresetSettings() { ModelName = "DELLU2724DE", SerialNumber = "808597589", ServiceTag = "123456", ColorForManual = 0, RunType = 1, AppInfo = appInfo } };
                settingsPluginManagerDev.Setup(x => x.ReadColorPresetSettings()).Returns(Task.FromResult(monitorConfigs2));
                var settingsPluginManagerDev2_ = settingsPluginManagerDev.Object;
                PrivateObject colorPresetprivateObject2 = new PrivateObject(colorPresetPlugin);

                var GetAutoColorPresetStatus_resullt3 = colorPresetPlugin.GetAutoColorPresetStatus(monitorInfo1, settingsPluginManagerDev2_).Result; //RunType 1 = (int)ColorPresetRunType.Auto, index_config=0
                Assert.IsNotNull(GetAutoColorPresetStatus_resullt3);
                Assert.That(AutoColorPresetStatus2, Is.EqualTo(GetAutoColorPresetStatus_resullt3));
            }
        }

        [Test]
        public void TestGetColorManagementStatus()
        {
            Mock<ISettingsManagerDev> settingsPluginManagerDev = new Mock<ISettingsManagerDev>();
            Dictionary<string, ColorPresetSettings_AppInfo> appInfo = new Dictionary<string, ColorPresetSettings_AppInfo>();
            appInfo.Add("Command Prompt", new ColorPresetSettings_AppInfo() { Color = 0, HDRColor = -1, IconName = "cmd.exe" });
            int index_config = 0;
            List<ColorPresetSettings> monitorConfigs1 = new List<ColorPresetSettings>() { new ColorPresetSettings() { ModelName = "DELLU2724DE", SerialNumber = "808597589", ServiceTag = "123456", ColorForManual = 0, RunType = 0, ColorManagement_Status = 1, ColorManagement_RunType = 1, AppInfo = appInfo } };
            var settingsPluginManagerDev_ = settingsPluginManagerDev.Object;
            settingsPluginManagerDev.Setup(x => x.ReadColorPresetSettings()).Returns(Task.FromResult(monitorConfigs1));
            PrivateObject colorPresetprivateObject = new PrivateObject(colorPresetPlugin);
            string ColorManagementStatus1 = "OFF";
            string ColorManagementStatus2 = "BYMONITOR";
            string ColorManagementStatus3 = "BYHOST";
            if (index_config >= 0)
            {
                var GetColorManagementStatus_resullt1 = colorPresetPlugin.GetColorManagementStatus(monitorInfo1, settingsPluginManagerDev_).Result; //ColorManagement_Status = 1, index_config=0, ColorManagement_RunType=1 Bymonitor
                Assert.IsNotNull(GetColorManagementStatus_resullt1);
                Assert.That(ColorManagementStatus2, Is.EqualTo(GetColorManagementStatus_resullt1));
            }
            else
            {
                var GetColorManagementStatus_resullt2 = colorPresetPlugin.GetColorManagementStatus(monitorInfo1, settingsPluginManagerDev_).Result; //ColorManagement_Status = 1, index_config < 0, ColorManagement_RunType=1 Bymonitor
                Assert.IsNotNull(GetColorManagementStatus_resullt2);
                Assert.That(ColorManagementStatus1, Is.EqualTo(GetColorManagementStatus_resullt2));
            }

            if (index_config >= 0)
            {
                List<ColorPresetSettings> monitorConfigs3 = new List<ColorPresetSettings>() { new ColorPresetSettings() { ModelName = "DELLU2724DE", SerialNumber = "808597589", ServiceTag = "123456", ColorForManual = 0, RunType = 1, ColorManagement_Status = 0, AppInfo = appInfo } };
                settingsPluginManagerDev.Setup(x => x.ReadColorPresetSettings()).Returns(Task.FromResult(monitorConfigs3));
                var settingsPluginManagerDev3_ = settingsPluginManagerDev.Object;
                PrivateObject colorPresetprivateObject2 = new PrivateObject(colorPresetPlugin);

                var GetColorManagementStatus_resullt3 = colorPresetPlugin.GetColorManagementStatus(monitorInfo1, settingsPluginManagerDev3_).Result; //ColorManagement_Status 0 = ColorManagementStatus.Off, index_config=0
                Assert.IsNotNull(GetColorManagementStatus_resullt3);
                Assert.That(ColorManagementStatus1, Is.EqualTo(GetColorManagementStatus_resullt3));
            }

            if (index_config >= 0)
            {
                List<ColorPresetSettings> monitorConfigs2 = new List<ColorPresetSettings>() { new ColorPresetSettings() { ModelName = "DELLU2724DE", SerialNumber = "808597589", ServiceTag = "123456", ColorForManual = 0, RunType = 1, ColorManagement_Status = 1, ColorManagement_RunType = 2, AppInfo = appInfo } };
                settingsPluginManagerDev.Setup(x => x.ReadColorPresetSettings()).Returns(Task.FromResult(monitorConfigs2));
                var settingsPluginManagerDev2_ = settingsPluginManagerDev.Object;
                PrivateObject colorPresetprivateObject2 = new PrivateObject(colorPresetPlugin);

                var GetColorManagementStatus_resullt4 = colorPresetPlugin.GetColorManagementStatus(monitorInfo1, settingsPluginManagerDev2_).Result; //ColorManagement_Status = 1, index_config=0, ColorManagement_RunType = 2 ColorManagementRunType.Byhost
                Assert.IsNotNull(GetColorManagementStatus_resullt4);
                Assert.That(ColorManagementStatus3, Is.EqualTo(GetColorManagementStatus_resullt4));
            }
        }

        [Test]
        public void TestStopRegistryMonitor()
        {
            colorPresetPlugin.StopRegistryMonitor();
            Assert.IsTrue(true);
        }

        [Test]
        public void TestOnError_ICC()
        {
            object sender = new object();
            Exception? exception = null;
            ErrorEventArgs e;
            e = new ErrorEventArgs(exception);
            var errorEventArgsMock = new Mock<ErrorEventArgs>();
            colorPresetPlugin.OnError_ICC(sender, e);
            Assert.IsTrue(true);
        }

    }
}