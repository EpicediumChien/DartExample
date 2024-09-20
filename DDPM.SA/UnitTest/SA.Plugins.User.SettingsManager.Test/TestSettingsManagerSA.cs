
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

        [Test]
        public void TestInitGlobalSettingConfigFile()
        {
            PrivateObject privateSettingsManagerObject = new PrivateObject(SettingsManagerSAPlugin);
            var result = privateSettingsManagerObject.Invoke("InitGlobalSettingConfigFile");
            var InitGlobalSettingParam = privateSettingsManagerObject.GetFieldOrProperty("_GlobalSettingParam");
            Assert.That(InitGlobalSettingParam, Is.EqualTo(result));
        }

        [Test]
        public void TestReadGlobalSettings()
        {
            GlobalSettingParam ReadGlobalSettingsResult1;
            string GlobalSettings_path1_ = "test_ReadGlobalSettingsPath.json";
            string jsonData = "{\"GlobalSetting_General\":{\"Low_Battery_Level\":true,\"Keyboard_Lock_Key\":false,\"Webcam_WB7022_Presence_Detection_Sensor_Cover_State\":true,\"Display_MuteState\":true,\"Display_Color_Preset_and_Easy_Memory\":true},\"GlobalSetting_WidgetSettings\":{\"EnableQuickAccessWidget\":false,\"EnableQuickAccessWidget_Reminder\":false},\"GlobalSetting_About\":{\"SWVersion\":\"\",\"DriverVersion\":\"0000\"}}";
            GlobalSettingParam globalSettingParam_ = new GlobalSettingParam();
            PrivateObject privateSettingsManagerObject = new PrivateObject(SettingsManagerSAPlugin);
            privateSettingsManagerObject.SetFieldOrProperty("_GlobalSetting_path", GlobalSettings_path1_);
            privateSettingsManagerObject.SetFieldOrProperty("_GlobalSettingParam", globalSettingParam_);
            if (!File.Exists(GlobalSettings_path1_))
            {
                ReadGlobalSettingsResult1 = SettingsManagerSAPlugin.ReadGlobalSettings().Result;  // strFilePath is not Exists GlobalSettings_path1_ GlobalSetting.json"
                Assert.That(globalSettingParam_, Is.EqualTo(ReadGlobalSettingsResult1)); //run finnish will create path1_
            }

            if (File.Exists(GlobalSettings_path1_))
            {
                File.WriteAllText(GlobalSettings_path1_, jsonData);
                var ReadColorPresetSettingsResult2 = SettingsManagerSAPlugin.ReadGlobalSettings().Result;
                var globalSettingParam = (GlobalSettingParam)privateSettingsManagerObject.GetFieldOrProperty("_GlobalSettingParam");
                Assert.That(globalSettingParam, Is.EqualTo(ReadColorPresetSettingsResult2));
                File.Delete(GlobalSettings_path1_);
            }
        }

        [Test]
        public void TestWriteGlobalSettings()
        {
            bool writeGlobalSettings_ = false;
            bool writeGlobalSettings_2 = true;
            GlobalSettingParam globalSettingParamNull;
            globalSettingParamNull = null;
            GlobalSettingParam globalSettingParam = new GlobalSettingParam()
            {
                GlobalSetting_About = new GlobalSetting_About(),
                GlobalSetting_General = new GlobalSetting_General(),
                GlobalSetting_WidgetSettings = new GlobalSetting_WidgetSettings()
            };
            string writeglobalSettings_path1_ = "test_WriteGlobalSettingsPath.json";
            string jsonData = "{\"GlobalSetting_General\":{\"Low_Battery_Level\":true,\"Keyboard_Lock_Key\":false,\"Webcam_WB7022_Presence_Detection_Sensor_Cover_State\":true,\"Display_MuteState\":true,\"Display_Color_Preset_and_Easy_Memory\":true},\"GlobalSetting_WidgetSettings\":{\"EnableQuickAccessWidget\":false,\"EnableQuickAccessWidget_Reminder\":false},\"GlobalSetting_About\":{\"SWVersion\":\"\",\"DriverVersion\":\"0000\"}}";
            File.WriteAllText(writeglobalSettings_path1_, jsonData);
            PrivateObject privateSettingsManagerObject = new PrivateObject(SettingsManagerSAPlugin);
            privateSettingsManagerObject.SetFieldOrProperty("_GlobalSetting_path", writeglobalSettings_path1_);
            if (globalSettingParamNull == null)
            {
                var WriteGlobalSettingsResult1 = SettingsManagerSAPlugin.WriteGlobalSettings(globalSettingParamNull).Result;  //globalSettingParam is not null
                Assert.That(writeGlobalSettings_, Is.EqualTo(WriteGlobalSettingsResult1));
            }

            if (globalSettingParam != null)
            {
                var WriteGlobalSettingsResult2 = SettingsManagerSAPlugin.WriteGlobalSettings(globalSettingParam).Result;  //globalSettingParam is null
                Assert.That(writeGlobalSettings_2, Is.EqualTo(WriteGlobalSettingsResult2));
                File.Delete(writeglobalSettings_path1_);
            }
        }

        [Test]
        public void TestRunSerializeGlobalSettingObject()
        {
            string globalSettings_path1_ = "test_GlobalSettingsPath.json";
            string GlobalSettingsjsonData = "{\"GlobalSetting_General\":{\"Low_Battery_Level\":true,\"Keyboard_Lock_Key\":false,\"Webcam_WB7022_Presence_Detection_Sensor_Cover_State\":true,\"Display_MuteState\":true,\"Display_Color_Preset_and_Easy_Memory\":true},\"GlobalSetting_WidgetSettings\":{\"EnableQuickAccessWidget\":false,\"EnableQuickAccessWidget_Reminder\":false},\"GlobalSetting_About\":{\"SWVersion\":\"\",\"DriverVersion\":\"0000\"}}";
            File.WriteAllText(globalSettings_path1_, GlobalSettingsjsonData);
            PrivateObject privateSettingsManagerObject = new PrivateObject(SettingsManagerSAPlugin);
            privateSettingsManagerObject.SetFieldOrProperty("_GlobalSetting_path", globalSettings_path1_);
            GlobalSettingParam globalSettingParam = new GlobalSettingParam()
            {
                GlobalSetting_About = new GlobalSetting_About(),
                GlobalSetting_General = new GlobalSetting_General(),
                GlobalSetting_WidgetSettings = new GlobalSetting_WidgetSettings()
            };
            var RunSerializeObjectresult = (string)privateSettingsManagerObject.Invoke("RunSerializeObject", globalSettingParam);
            Assert.Greater(RunSerializeObjectresult.Length, 0);
            File.Delete(globalSettings_path1_);
        }

        [Test]
        public void TestRunGlobalSettinDeserializeObject()
        {
            string globalSettingsDes_path1_ = "test_GlobalSettingsPath.json";
            string GlobalSettingsDesjsonData = "{\"GlobalSetting_General\":{\"Low_Battery_Level\":true,\"Keyboard_Lock_Key\":false,\"Webcam_WB7022_Presence_Detection_Sensor_Cover_State\":true,\"Display_MuteState\":true,\"Display_Color_Preset_and_Easy_Memory\":true},\"GlobalSetting_WidgetSettings\":{\"EnableQuickAccessWidget\":false,\"EnableQuickAccessWidget_Reminder\":false},\"GlobalSetting_About\":{\"SWVersion\":\"0000\",\"DriverVersion\":\"0000\"}}";
            File.WriteAllText(globalSettingsDes_path1_, GlobalSettingsDesjsonData);
            PrivateObject privateSettingsManagerObject = new PrivateObject(SettingsManagerSAPlugin);
            privateSettingsManagerObject.SetFieldOrProperty("_GlobalSetting_path", globalSettingsDes_path1_);
            GlobalSettingParam globalSettingParamDes = new GlobalSettingParam()
            {
                GlobalSetting_About = new GlobalSetting_About() { SWVersion = "0000", DriverVersion = "0000", },
                GlobalSetting_General = new GlobalSetting_General() { Low_Battery_Level = true, Keyboard_Lock_Key = false, Webcam_WB7022_Presence_Detection_Sensor_Cover_State = true, Display_Color_Preset_and_Easy_Memory = true, Display_MuteState = true },
                GlobalSetting_WidgetSettings = new GlobalSetting_WidgetSettings() { EnableQuickAccessWidget = false, EnableQuickAccessWidget_Reminder = false }
            };
            var RunGlobalSettinDeserializeObjectResult = (GlobalSettingParam)privateSettingsManagerObject.Invoke("RunGlobalSettinDeserializeObject", GlobalSettingsDesjsonData);
            Assert.That(globalSettingParamDes.GlobalSetting_About.SWVersion, Is.EqualTo(RunGlobalSettinDeserializeObjectResult.GlobalSetting_About.SWVersion));
            Assert.That(globalSettingParamDes.GlobalSetting_General.Low_Battery_Level, Is.EqualTo(RunGlobalSettinDeserializeObjectResult.GlobalSetting_General.Low_Battery_Level));
            Assert.That(globalSettingParamDes.GlobalSetting_General.Webcam_WB7022_Presence_Detection_Sensor_Cover_State, Is.EqualTo(RunGlobalSettinDeserializeObjectResult.GlobalSetting_General.Webcam_WB7022_Presence_Detection_Sensor_Cover_State));
            Assert.That(globalSettingParamDes.GlobalSetting_WidgetSettings.EnableQuickAccessWidget, Is.EqualTo(RunGlobalSettinDeserializeObjectResult.GlobalSetting_WidgetSettings.EnableQuickAccessWidget));
            File.Delete(globalSettingsDes_path1_);
        }

        [Test]
        public void TestInitPowerNapConfigFile()
        {
            PrivateObject privateSettingsManagerObject = new PrivateObject(SettingsManagerSAPlugin);
            var result = privateSettingsManagerObject.Invoke("InitPowerNapConfigFile");
            var InitPowerNapSetting = privateSettingsManagerObject.GetFieldOrProperty("_powerNapSettings");
            Assert.That(InitPowerNapSetting, Is.EqualTo(result));
        }

        [Test]
        public void TestReadPowerNapSettings()
        {
            string ReadPowerNapSettings_path1_ = "test_ReadPowerNapSettingsPath.json";
            string PowerNapSettingsjsonData = "[{\"ModelName\":\"TestU2724\",\"SerialNumber\":\"123456789\",\"Status\":false,\"RunType\":0}]";
            List<PowerNapSetting> powerNapSettings_ = new List<PowerNapSetting>();
            PrivateObject privateSettingsManagerObject = new PrivateObject(SettingsManagerSAPlugin);
            privateSettingsManagerObject.SetFieldOrProperty("_powerNapsettings_path", ReadPowerNapSettings_path1_);
            privateSettingsManagerObject.SetFieldOrProperty("_present_powerNap_settings", powerNapSettings_);
            if (!File.Exists(ReadPowerNapSettings_path1_))
            {
                var ReadPowerNapSettingsResult1 = SettingsManagerSAPlugin.ReadPowerNapSettings().Result;  // strFilePath is not Exists PowerNapSettings.json"
                Assert.That(powerNapSettings_, Is.EqualTo(ReadPowerNapSettingsResult1)); //run finnish will create path
            }

            if (File.Exists(ReadPowerNapSettings_path1_))
            {
                File.WriteAllText(ReadPowerNapSettings_path1_, PowerNapSettingsjsonData);
                var ReadPowerNapSettingsResult2 = SettingsManagerSAPlugin.ReadPowerNapSettings().Result;
                var powerNap_settings = (List<PowerNapSetting>)privateSettingsManagerObject.GetFieldOrProperty("_present_powerNap_settings");
                Assert.That(powerNap_settings, Is.EqualTo(ReadPowerNapSettingsResult2));
                Assert.Greater(ReadPowerNapSettingsResult2.Count, 0);
                File.Delete(ReadPowerNapSettings_path1_);
            }
        }

        [Test]
        public void TestWritePowerNapSettings()
        {
            bool WritePowerNapSettings_ = false;
            bool WritePowerNapSettings_succeed = true;
            List<PowerNapSetting> powerNapSettingsNull;
            powerNapSettingsNull = null;
            List<PowerNapSetting> powerNapSettingsConfig = new List<PowerNapSetting>() { new PowerNapSetting() { ModelName = "TestU2724", SerialNumber = "123456789", Status = false, RunType = 0 } };
            string WritePowerNapSettings_path1_ = "test_WritePowerNapSettingsPath.json";
            string PowerNapSettingsjsonData = "[{\"ModelName\":\"TestU2724\",\"SerialNumber\":\"123456789\",\"Status\":false,\"RunType\":0}]";
            File.WriteAllText(WritePowerNapSettings_path1_, PowerNapSettingsjsonData);
            PrivateObject privateSettingsManagerObject = new PrivateObject(SettingsManagerSAPlugin);
            privateSettingsManagerObject.SetFieldOrProperty("_powerNapsettings_path", WritePowerNapSettings_path1_);
            if (powerNapSettingsNull == null)
            {
                var WritePowerNapSettingsResult1 = SettingsManagerSAPlugin.WritePowerNapSettings(powerNapSettingsNull).Result;
                Assert.That(WritePowerNapSettings_, Is.EqualTo(WritePowerNapSettingsResult1));
            }

            if (powerNapSettingsConfig != null)
            {
                var WritePowerNapSettingsResult2 = SettingsManagerSAPlugin.WritePowerNapSettings(powerNapSettingsConfig).Result;
                Assert.That(WritePowerNapSettings_succeed, Is.EqualTo(WritePowerNapSettingsResult2));
                File.Delete(WritePowerNapSettings_path1_);
            }
        }

        [Test]
        public void TestRunPowerNapDeserializeObject()
        {
            List<PowerNapSetting> powerNapSettingsConfig = new List<PowerNapSetting>() { new PowerNapSetting() { ModelName = "TestU2724", SerialNumber = "123456789", Status = false, RunType = 0 } };
            string SerializePowerNapSettings_path1_ = "test_SerializePowerNapSettingsPath.json";
            string PowerNapSettingsjsonData = "[{\"ModelName\":\"TestU2724\",\"SerialNumber\":\"123456789\",\"Status\":false,\"RunType\":0}]";
            File.WriteAllText(SerializePowerNapSettings_path1_, PowerNapSettingsjsonData);
            PrivateObject privateSettingsManagerObject = new PrivateObject(SettingsManagerSAPlugin);
            privateSettingsManagerObject.SetFieldOrProperty("_powerNapsettings_path", SerializePowerNapSettings_path1_);
            var RunDeserialObjectResult = (string)privateSettingsManagerObject.Invoke("RunSerializeObject", powerNapSettingsConfig);
            Assert.Greater(RunDeserialObjectResult.Length, 0);
            File.Delete(SerializePowerNapSettings_path1_);
        }

        [Test]
        public void TestRunPowerNapSettingDeserializeObject()
        {
            List<PowerNapSetting> powerNapSettingsConfig = new List<PowerNapSetting>() { new PowerNapSetting() { ModelName = "TestU2724", SerialNumber = "123456789", Status = false, RunType = 0 } };
            string DeserialPowerNapSettings_path1_ = "test_DeserialPowerNapSettingsPath.json";
            string PowerNapSettingsjsonData = "[{\"ModelName\":\"TestU2724\",\"SerialNumber\":\"123456789\",\"Status\":false,\"RunType\":0}]";
            File.WriteAllText(DeserialPowerNapSettings_path1_, PowerNapSettingsjsonData);
            PrivateObject privateSettingsManagerObject = new PrivateObject(SettingsManagerSAPlugin);
            privateSettingsManagerObject.SetFieldOrProperty("_powerNapsettings_path", DeserialPowerNapSettings_path1_);
            var RunPowerNapDeserializeObjectResult = (List<PowerNapSetting>)privateSettingsManagerObject.Invoke("RunPowerNapDeserializeObject", PowerNapSettingsjsonData);
            Assert.Greater(RunPowerNapDeserializeObjectResult.Count, 0);
            Assert.That(powerNapSettingsConfig[0].ModelName, Is.EqualTo(RunPowerNapDeserializeObjectResult[0].ModelName));
            Assert.That(powerNapSettingsConfig[0].SerialNumber, Is.EqualTo(RunPowerNapDeserializeObjectResult[0].SerialNumber));
            Assert.That(powerNapSettingsConfig[0].Status, Is.EqualTo(RunPowerNapDeserializeObjectResult[0].Status));
            File.Delete(DeserialPowerNapSettings_path1_);
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