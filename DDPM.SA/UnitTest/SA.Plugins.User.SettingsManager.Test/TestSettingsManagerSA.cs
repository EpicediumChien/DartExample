
using DDPM.SA.Plugins.User.SettingsManager;
using DDPM.SA.Common.Interfaces;
using DDPM.SA.Common;
using DDPM.SA.Plugins.User.DisplayManager;
using DDPM.SA.Plugins.User.DisplayProperties;
using DDPM.SA.Plugins.User.EasyArrange;
using DDPM.SA.Plugins.User.PipPbpManger;
using Dell.Client.Framework.Interfaces;
using Dell.Client.Framework.UnitTestShared.Tests;
using Moq;
using VcpCore.Common;
using VcpCore.Interfaces;
using VcpCore.Plugins;
using DDPM.SA.Common.Settings;
using System.Net.Http.Json;
using Newtonsoft.Json;
using Windows.Devices.Display.Core;
using Windows.Globalization;
using static DDPM.SA.Common.Settings.DDPMUserSettings;
using DDPM.SA.Common.Display;

namespace DDPM.SA.Plugins.User.SettingsManager.Test
{
    public class TestSettingsManagerSA
    {
        private Mock<IAgent> DisplayMangerAgent { get; } = new();
        private Mock<IAgent> VcpCoreAgent { get; } = new();
        private Mock<IAgent> SettingsManagerPluginAgent { get; } = new();

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

        private SettingsManagerSA CreateInitializeSettingsManagerSAPlugin()
        {
            SettingsManagerPluginAgent.Setup(x => x.PluginManager.FindPluginByGuid(Guid.Parse(DDPM.SA.Common.IDs.DDPM_SETTINGSMANAGER_SA_PLUGIN_ID)));

            return new SettingsManagerSA(SettingsManagerPluginAgent.Object);
        }

        private DisplayMangerPlugin displayPlugin;
        private VcpCorePlugin vcpCorePlugin;
        private SettingsManagerSA SettingsManagerSAPlugin;

        [OneTimeSetUp]
        public void Setup()
        {
            displayPlugin = CreateInitializeDisplayMangerPlugin();
            vcpCorePlugin = CreateInitializeVcpCorePlugin();
            SettingsManagerSAPlugin = CreateInitializeSettingsManagerSAPlugin();
            PrivateObject privateObject = new PrivateObject(displayPlugin);
            PrivateObject privatevcp = new PrivateObject(vcpCorePlugin);
            privateObject.SetField("_VcpCorePlugin", vcpCorePlugin as IVcpCoreService);
        }

        [Test]
        public void TestSettingsManagerPlugin()
        {
            SettingsManagerSA settingsManagerSA_=new SettingsManagerSA(SettingsManagerPluginAgent.Object);
            Assert.IsNotNull(SettingsManagerSAPlugin);
            PrivateObject privateSettingsManagerObj = new PrivateObject(SettingsManagerSAPlugin);
            var agent2 = privateSettingsManagerObj.GetField("_agent") as IAgent;
            Assert.That(agent2, Is.EqualTo(SettingsManagerPluginAgent.Object));
        }

        [Test]
        public void TestIsDisposed()
        {
            var Result = SettingsManagerSAPlugin.IsDisposed;  
            Assert.IsFalse(Result);
            PrivateObject privateSettingsManagerObject = new PrivateObject(SettingsManagerSAPlugin);
            privateSettingsManagerObject.SetFieldOrProperty("IsDisposed", true);  //IsDisposed is true
            var Result2 = SettingsManagerSAPlugin.IsDisposed;
            Assert.IsTrue(Result2);
        }

        [Test]
        public void TestInitDDPMUserConfigFile()
        {
            PrivateObject privateSettingsManagerObject = new PrivateObject(SettingsManagerSAPlugin);
            var result= privateSettingsManagerObject.Invoke("InitDDPMUserConfigFile");
            var InitDDPMSettings = privateSettingsManagerObject.GetFieldOrProperty("_settings");
            Assert.That(InitDDPMSettings,Is.EqualTo(result));
        }

        [Test]
        public void TestGetActiveUserLocalAppDataPath()
        {
            PrivateObject privateSettingsManagerObject = new PrivateObject(SettingsManagerSAPlugin);
            var result = (string)privateSettingsManagerObject.Invoke("GetActiveUserLocalAppDataPath");
            Assert.That(!string.IsNullOrEmpty(result));
        }

        [Test]
        public void TestGetAppIconFolderPath()
        {
            string appIconFolderPath_ = "test_path";
            PrivateObject privateSettingsManagerObject = new PrivateObject(SettingsManagerSAPlugin);
            privateSettingsManagerObject.SetFieldOrProperty("_appiconfolder_path", appIconFolderPath_);
            var GetAppIconFolderPathResult = SettingsManagerSAPlugin.GetAppIconFolderPath().Result;
            Assert.That(appIconFolderPath_, Is.EqualTo(GetAppIconFolderPathResult));
        }

        [Test]
        public void TestInitColorPresetConfigFile()
        {
            PrivateObject privateSettingsManagerObject = new PrivateObject(SettingsManagerSAPlugin);
            var result = privateSettingsManagerObject.Invoke("InitColorPresetConfigFile");
            var InitColorPresetSettings = privateSettingsManagerObject.GetFieldOrProperty("_colorPresetSettings");
            Assert.IsNotNull(result);
            Assert.That(InitColorPresetSettings, Is.EqualTo(result));
        }

        [Test]
        public void TestReadColorPresetSettings()
        {
            string colorsettings_path1_ = "test_colorsettingPath.json";
            string jsonData = "[{\"DeviceInfo\":null,\"RunType\":0,\"AppInfo\":null,\"PresetForManual\":\"TestManual\"}]";
            //string jsonData = "[{'DeviceInfo':null,'RunType':0,'AppInfo':null,'PresetForManual':'TestManual'}]";
            string presetForManual_ = "TestManual";
            List<ColorPresetSettings> preset_settings1_ = new List<ColorPresetSettings>();
            PrivateObject privateSettingsManagerObject = new PrivateObject(SettingsManagerSAPlugin);
            privateSettingsManagerObject.SetFieldOrProperty("_colorsettings_path", colorsettings_path1_);
            privateSettingsManagerObject.SetFieldOrProperty("_preset_settings", preset_settings1_);
            if (!File.Exists(colorsettings_path1_))
            {
                var ReadColorPresetSettingsResult1 = SettingsManagerSAPlugin.ReadColorPresetSettings().Result;  // strFilePath is not Exists colorsettings_path ColorSetting.json"
                Assert.That(preset_settings1_, Is.EqualTo(ReadColorPresetSettingsResult1)); //run finnish will create colorsettings_path1_
            }

            if (File.Exists(colorsettings_path1_))
            {
                File.WriteAllText(colorsettings_path1_, jsonData); // mock data to temp data
                var ReadColorPresetSettingsResult2 = SettingsManagerSAPlugin.ReadColorPresetSettings().Result;
                Assert.Greater(ReadColorPresetSettingsResult2.Count, 0);
                Assert.That(presetForManual_, Is.EqualTo(ReadColorPresetSettingsResult2[0].PresetForManual));
                File.Delete(colorsettings_path1_);
            }
        }

        [Test]
        public void TestWriteColorPresetSettings()
        {
            bool writeColorPresetSettings_ = false;
            bool writeColorPresetSettings_succeed = true;
            List<ColorPresetSettings> colorPresetSettingsConfigsNull = null;
            List<ColorPresetSettings> colorPresetSettingsConfigs = new List<ColorPresetSettings>() { new ColorPresetSettings() { } };
            string colorsettings_path1_ = "test_writeCroPresetpath.json";
            string jsonData = "[{\"DeviceInfo\":null,\"RunType\":0,\"AppInfo\":null,\"PresetForManual\":\"TestManual\"}]";
            File.WriteAllText(colorsettings_path1_, jsonData);
            PrivateObject privateSettingsManagerObject = new PrivateObject(SettingsManagerSAPlugin);
            privateSettingsManagerObject.SetFieldOrProperty("_colorsettings_path", colorsettings_path1_);
            if (colorPresetSettingsConfigsNull == null)
            {
                var WriteColorPresetSettingsResult1 = SettingsManagerSAPlugin.WriteColorPresetSettings(colorPresetSettingsConfigsNull).Result;
                Assert.That(writeColorPresetSettings_, Is.EqualTo(WriteColorPresetSettingsResult1));
            }

            if (colorPresetSettingsConfigs != null)
            {
                var WriteColorPresetSettingsResult2 = SettingsManagerSAPlugin.WriteColorPresetSettings(colorPresetSettingsConfigs).Result;
                Assert.That(writeColorPresetSettings_succeed, Is.EqualTo(WriteColorPresetSettingsResult2));
                File.Delete(colorsettings_path1_);
            }
        }

        [Test]
        public void TestRunDeserializeColorPresetSettingsObject()
        {
            List<ColorPresetSettings> colorPresetSettingsConfigs = new List<ColorPresetSettings>() { new ColorPresetSettings() { DeviceInfo = new EDID(), RunType = 1, AppInfo = new Dictionary<string, ColorPresetSettings_AppInfo>(), PresetForManual = "testpre" } };
            string colorpresettingsObject_path1_ = "test_writeCroPresetpath.json";
            string jsonData = "[{\"DeviceInfo\":null,\"RunType\":0,\"AppInfo\":null,\"PresetForManual\":\"TestManual\"}]";
            File.WriteAllText(colorpresettingsObject_path1_, jsonData);
            PrivateObject privateSettingsManagerObject = new PrivateObject(SettingsManagerSAPlugin);
            privateSettingsManagerObject.SetFieldOrProperty("_colorsettings_path", colorpresettingsObject_path1_);
            //using (var reader = new StreamReader(colorpresettingsObject_path1_))
            //{
            //    colorpresettingsObject_path1_ = reader.ReadToEnd();
            //}
            var RunSerializeObjectResult = (List<ColorPresetSettings>)privateSettingsManagerObject.Invoke("RunDeserializeObject", jsonData);
            Assert.Greater(RunSerializeObjectResult.Count, 0);
            File.Delete(colorpresettingsObject_path1_);
        }

        [Test]
        public void TestRunSerializeColorPresetSettingsObject()
        {
            List<ColorPresetSettings> colorPresetSettingsConfigs = new List<ColorPresetSettings>() { new ColorPresetSettings() { DeviceInfo = new EDID(), RunType = 1, AppInfo = new Dictionary<string, ColorPresetSettings_AppInfo>(), PresetForManual = "testpre" } };
            string colorpresettingsObject_path1_ = "test_writeCroPresetpath.json";
            string jsonData = "[{\"DeviceInfo\":null,\"RunType\":0,\"AppInfo\":null,\"PresetForManual\":\"TestManual\"}]";
            File.WriteAllText(colorpresettingsObject_path1_, jsonData);
            PrivateObject privateSettingsManagerObject = new PrivateObject(SettingsManagerSAPlugin);
            privateSettingsManagerObject.SetFieldOrProperty("_colorsettings_path", colorpresettingsObject_path1_);
            var RunSerializeObjectResult = (string)privateSettingsManagerObject.Invoke("RunSerializeObject", colorPresetSettingsConfigs);
            Assert.Greater(RunSerializeObjectResult.Length, 0);
            File.Delete(colorpresettingsObject_path1_);
        }

        [Test]
        public void TestInitHotkeyConfigFile()
        {
            PrivateObject privateSettingsManagerObject = new PrivateObject(SettingsManagerSAPlugin);
            var result = privateSettingsManagerObject.Invoke("InitHotkeyConfigFile");
            var InitHotkeySettings = privateSettingsManagerObject.GetFieldOrProperty("_hotkeySettings");
            Assert.That(InitHotkeySettings, Is.EqualTo(result));
        }

        [Test]
        public void TestReadHotkeySettings()
        {
            string hotkeysettings_path1_ = "test_hotkeyPath.json";
            string jsonData = "{\"hotkeySettings\":[{\"DeviceInfo\":null,\"HotkeyOptions\":[],\"HotkeyInfo\":[{\"Description\":\"Testhotkey\",\"Hotkey\":[],\"Status\":\"Registered\",\"Job\":\"BrightnessIncrease\",\"InputSource\":[]}]}]}";
            List<HotkeySettings> presetHotkey_settings_ = new List<HotkeySettings>();
            PrivateObject privateSettingsManagerObject = new PrivateObject(SettingsManagerSAPlugin);
            privateSettingsManagerObject.SetFieldOrProperty("_hotkeysettings_path", hotkeysettings_path1_);
            privateSettingsManagerObject.SetFieldOrProperty("_present_hotkey_settings", presetHotkey_settings_);
            if (!File.Exists(hotkeysettings_path1_))
            {
                var ReadHotkeySettingssResult1 = SettingsManagerSAPlugin.ReadHotkeySettings().Result;  // strFilePath is not Exists hotkeysettings_path1_ 
                Assert.That(presetHotkey_settings_, Is.EqualTo(ReadHotkeySettingssResult1)); //run finnish will create hotkeysettings_path1_
            }

            if (File.Exists(hotkeysettings_path1_))
            {
                File.WriteAllText(hotkeysettings_path1_, jsonData);
                var ReadHotkeySettingssResult2 = SettingsManagerSAPlugin.ReadHotkeySettings().Result;
                Assert.That(presetHotkey_settings_, Is.EqualTo(ReadHotkeySettingssResult2));
                File.Delete(hotkeysettings_path1_);
            }
        }

        [Test]
        public void TestWriteHotkeySettings()
        {
            bool writeHotkeySettings_ = false;
            bool writeHotkeySettingsSettings_succeed = true;
            List<HotkeySettings> hotkeySettingsNull = null;
            List<HotkeySettings> hotkeySettingsConfig = new List<HotkeySettings>() { new HotkeySettings() { DeviceInfo = new EDID(), HotkeyInfo = new List<HotkeyInfo>(), HotkeyOptions = new List<HotkeyOption>() } };
            List<ColorPresetSettings> colorPresetSettingsConfigs = new List<ColorPresetSettings>() { new ColorPresetSettings() { } };
            string WriteHotkeySettings_path1_ = "test_WriteHotkeySettingspath.json";
            string jsonData = "{\"hotkeySettings\":[{\"DeviceInfo\":null,\"HotkeyOptions\":[],\"HotkeyInfo\":[{\"Description\":\"Testhotkey\",\"Hotkey\":[],\"Status\":\"Registered\",\"Job\":\"BrightnessIncrease\",\"InputSource\":[]}]}]}";
            File.WriteAllText(WriteHotkeySettings_path1_, jsonData);
            PrivateObject privateSettingsManagerObject = new PrivateObject(SettingsManagerSAPlugin);
            privateSettingsManagerObject.SetFieldOrProperty("_hotkeysettings_path", WriteHotkeySettings_path1_);
            if (hotkeySettingsNull == null)
            {
                var WriteHotkeySettingssResult1 = SettingsManagerSAPlugin.WriteHotkeySettings(hotkeySettingsNull).Result;
                Assert.That(writeHotkeySettings_, Is.EqualTo(WriteHotkeySettingssResult1));
            }

            if (hotkeySettingsConfig != null)
            {
                var WriteHotkeySettingssResult2 = SettingsManagerSAPlugin.WriteHotkeySettings(hotkeySettingsConfig).Result;
                Assert.That(writeHotkeySettingsSettings_succeed, Is.EqualTo(WriteHotkeySettingssResult2));
                File.Delete(WriteHotkeySettings_path1_);
            }
        }

        [Test]
        public void TestRunSerializehotkeySettingsObject()
        {
            List<HotkeySettings> hotkeySettingsConfig = new List<HotkeySettings>() { new HotkeySettings() { DeviceInfo = new EDID(), HotkeyInfo = new List<HotkeyInfo>(), HotkeyOptions = new List<HotkeyOption>() } };
            string RunSerializehotkeySettingsObject_path1_ = "test_WriteHotkeySettingspath.json";
            string jsonData = "{\"hotkeySettings\":[{\"DeviceInfo\":null,\"HotkeyOptions\":[],\"HotkeyInfo\":[{\"Description\":\"Testhotkey\",\"Hotkey\":[],\"Status\":\"Registered\",\"Job\":\"BrightnessIncrease\",\"InputSource\":[]}]}]}";
            File.WriteAllText(RunSerializehotkeySettingsObject_path1_, jsonData);
            PrivateObject privateSettingsManagerObject = new PrivateObject(SettingsManagerSAPlugin);
            privateSettingsManagerObject.SetFieldOrProperty("_hotkeysettings_path", RunSerializehotkeySettingsObject_path1_);
            var RunSerializeObjectResult = (string)privateSettingsManagerObject.Invoke("RunSerializeObject", hotkeySettingsConfig);
            Assert.Greater(RunSerializeObjectResult.Length, 0);
            File.Delete(RunSerializehotkeySettingsObject_path1_);
        }

        [Test]
        public void TestRunHotkeyDeserializeObject()
        {
            List<HotkeySettings> hotkeySettingsConfig = new List<HotkeySettings>() { };
            string DeserializehotkeySettingsObject_path1_ = "test_RunHotkeyDeserializeObjectpath.json";
            string hotkeyjsonData = "{\"hotkeySettings\":[{\"DeviceInfo\":null,\"HotkeyOptions\":[],\"HotkeyInfo\":[{\"Description\":\"Testhotkey\",\"Hotkey\":[],\"Status\":\"Registered\",\"Job\":\"BrightnessIncrease\",\"InputSource\":[]}]}]}";
            File.WriteAllText(DeserializehotkeySettingsObject_path1_, hotkeyjsonData);
            PrivateObject privateSettingsManagerObject = new PrivateObject(SettingsManagerSAPlugin);
            privateSettingsManagerObject.SetFieldOrProperty("_hotkeysettings_path", DeserializehotkeySettingsObject_path1_);
            var RunHotkeyDeserializeObjectResult = (List<HotkeySettings>)privateSettingsManagerObject.Invoke("RunHotkeyDeserializeObject", hotkeyjsonData);
            Assert.That(hotkeySettingsConfig, Is.EqualTo(RunHotkeyDeserializeObjectResult));
            File.Delete(DeserializehotkeySettingsObject_path1_);
        }


        [OneTimeTearDown]
        public void TearDown()
        {
            displayPlugin.Dispose();
            vcpCorePlugin.Dispose();
            SettingsManagerSAPlugin.Dispose();
        }

    }
}