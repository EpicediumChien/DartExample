
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
using static DDPM.RemoteManagement.Common.Interfaces.Params;
using Dell.Client.Framework.Common;
using DdmLibrary;
using DdmLibrary.Utility;
using static Dell.Client.Framework.Common.Platform;
using static DdmLibrary.Utility.DDMUserSettings;
using Languages = DDPM.SA.Common.Settings.DDPMUserSettings.Languages;

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
            DisplayMangerAgent.Setup(x => x.PluginManager.FindPluginByGuid(Guid.Parse(DDPM.SA.Common.IDs.Display_Manager_PLUGIN_ID)));

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
            SettingsManagerSA settingsManagerSA_ = new SettingsManagerSA(SettingsManagerPluginAgent.Object);
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
            var result = privateSettingsManagerObject.Invoke("InitDDPMUserConfigFile");
            var InitDDPMSettings = privateSettingsManagerObject.GetFieldOrProperty("_settings");
            Assert.That(InitDDPMSettings, Is.EqualTo(result));
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
            string jsonData = "[{\"ModelName\":\"TestU2724\",\"SerialNumber\":\"123456\",\"RunType\":0,\"ColorManagement_Status\":0,\"ColorManagement_RunType\":1,\"AppInfo\":null,\"PresetForManual\":\"TestManual\"}]";
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
                var ReadColorPresetSettingsResult2 = SettingsManagerSAPlugin.ReadColorPresetSettings().Result; //strFilePath is  Exists, get strReadJson length is null
                Assert.That(preset_settings1_, Is.EqualTo(ReadColorPresetSettingsResult2));
            }
            if (File.Exists(colorsettings_path1_) && jsonData != null)
            {
                string settingsAccessInfo = "Test settings AccessInfo Calculate for Test verify";
                string colorsettings_path3 = Environment.CurrentDirectory + "\\" + colorsettings_path1_;
                privateSettingsManagerObject.SetFieldOrProperty("_colorsettings_path", colorsettings_path3);
                //privateSettingsManagerObject.SetFieldOrProperty("_settingsAccessInfo", settingsAccessInfo);
                var ReadColorPresetSettingsResult3 = SettingsManagerSAPlugin.ReadColorPresetSettings().Result; //strFilePath is  Exists, get strReadJson length is not null
                Assert.IsNotNull(colorsettings_path3);
                Assert.That(preset_settings1_, Is.EqualTo(ReadColorPresetSettingsResult3));
                File.Delete(colorsettings_path1_);
            }
        }

        [Test]
        public void TestWriteColorPresetSettings()
        {
            bool writeColorPresetSettings_ = false;
            bool writeColorPresetSettings_succeed = true;
            List<ColorPresetSettings> colorPresetSettingsConfigsNull = null;
            List<ColorPresetSettings> colorPresetSettingsConfigs = new List<ColorPresetSettings>();
            ColorPresetSettings colorPresetSettings = new ColorPresetSettings()
            {
                AppInfo = new Dictionary<string, ColorPresetSettings_AppInfo>(),
                ColorManagement_RunType = 1,
                ColorManagement_Status = 0,
                ModelName = "TestU2724",
                //PresetForManual = "TestPresetForManual",
                SerialNumber = "123456",
                RunType = 0
            };
            colorPresetSettingsConfigs.Add(colorPresetSettings);
            string colorsettings_path1_ = "test_writeCroPresetpath.json";
            string jsonData = "[{\"ModelName\":\"TestU2724\",\"SerialNumber\":\"123456\",\"RunType\":0,\"ColorManagement_Status\":0,\"ColorManagement_RunType\":1,\"AppInfo\":null}]";
            File.WriteAllText(colorsettings_path1_, jsonData);
            string settingsAccessInfo = "Test settings AccessInfo Calculate for Test verify";
            PrivateObject privateSettingsManagerObject = new PrivateObject(SettingsManagerSAPlugin);
            string colorsettings_path2 = Environment.CurrentDirectory + "\\" + colorsettings_path1_;
            privateSettingsManagerObject.SetFieldOrProperty("_colorsettings_path", colorsettings_path2);
            //privateSettingsManagerObject.SetFieldOrProperty("_settingsAccessInfo", settingsAccessInfo);
            if (colorPresetSettingsConfigsNull == null)
            {
                var WriteColorPresetSettingsResult1 = SettingsManagerSAPlugin.WriteColorPresetSettings(colorPresetSettingsConfigsNull).Result;
                Assert.That(writeColorPresetSettings_, Is.EqualTo(WriteColorPresetSettingsResult1));
            }

            if (colorPresetSettingsConfigs != null)
            {
                var WriteColorPresetSettingsResult2 = SettingsManagerSAPlugin.WriteColorPresetSettings(colorPresetSettingsConfigs).Result;
                Assert.IsNotNull(WriteColorPresetSettingsResult2);
                Assert.That(writeColorPresetSettings_succeed, Is.EqualTo(WriteColorPresetSettingsResult2));
                File.Delete(colorsettings_path1_);
            }
        }

        [Test]
        public void TestRunDeserializeColorPresetSettingsObject()
        {
            List<ColorPresetSettings> colorPresetSettingsConfigs = new List<ColorPresetSettings>() { new ColorPresetSettings() { ModelName = "DELLU3224KB", SerialNumber = "808792396", RunType = 1, AppInfo = new Dictionary<string, ColorPresetSettings_AppInfo>() } };
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

        // RunSerializeObject ColorPresetSettings method had been remove in SettingManagerSA.cs
        [Test]
        public void TestRunSerializeColorPresetSettingsObject()
        {
            List<ColorPresetSettings> colorPresetSettingsConfigs = new List<ColorPresetSettings>() { new ColorPresetSettings() { ModelName = "DELLU3224KB", SerialNumber = "808792396", RunType = 1, AppInfo = new Dictionary<string, ColorPresetSettings_AppInfo>() } };
            string colorpresettingsObject_path1_ = "test_writeCroPresetpath.json";
            string jsonData = "[{\"DeviceInfo\":null,\"RunType\":0,\"AppInfo\":null,\"PresetForManual\":\"TestManual\"}]";
            File.WriteAllText(colorpresettingsObject_path1_, jsonData);
            PrivateObject privateSettingsManagerObject = new PrivateObject(SettingsManagerSAPlugin);
            privateSettingsManagerObject.SetFieldOrProperty("_colorsettings_path", colorpresettingsObject_path1_);
            //var RunSerializeObjectResult = (string)privateSettingsManagerObject.Invoke("RunSerializeObject", colorPresetSettingsConfigs);  
            //Assert.Greater(RunSerializeObjectResult.Length, 0);
            //File.Delete(colorpresettingsObject_path1_);
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
            privateSettingsManagerObject.SetFieldOrProperty("_hotkeySettings", presetHotkey_settings_);
            List<HotkeySettings> _hotkeySettings = new List<HotkeySettings>();
            _hotkeySettings = null;
            if (!File.Exists(hotkeysettings_path1_))
            {
                var ReadHotkeySettingssResult1 = SettingsManagerSAPlugin.ReadHotkeySettings().Result;  // strFilePath is not Exists hotkeysettings_path1_ 
                Assert.That(_hotkeySettings, Is.EqualTo(ReadHotkeySettingssResult1)); //run finnish will create hotkeysettings_path1_
            }
            File.WriteAllText(hotkeysettings_path1_, jsonData);
            string hotkeysettings_path2_ = Environment.CurrentDirectory + "\\" + hotkeysettings_path1_;
            privateSettingsManagerObject.SetFieldOrProperty("_hotkeysettings_path", hotkeysettings_path2_);
            string settingsAccessInfo = "Test settings AccessInfo Calculate for Test verify";
            //privateSettingsManagerObject.SetFieldOrProperty("_settingsAccessInfo", settingsAccessInfo);
            if (File.Exists(hotkeysettings_path1_))
            {
                var ReadHotkeySettingssResult2 = SettingsManagerSAPlugin.ReadHotkeySettings().Result;
                var hotkeySettings = (List<HotkeySettings>)privateSettingsManagerObject.GetFieldOrProperty("_hotkeySettings");
                Assert.That(hotkeySettings, Is.EqualTo(ReadHotkeySettingssResult2));
                File.Delete(hotkeysettings_path1_);
            }
        }

        [Test]
        public void TestWriteHotkeySettings()
        {
            bool writeHotkeySettings_ = false;
            bool writeHotkeySettingsSettings_succeed = true;
            List<HotkeySettings> hotkeySettingsNull = null;
            //List<HotkeySettings> hotkeySettingsConfig = new List<HotkeySettings>() { new HotkeySettings() { DeviceInfo = new EDID(), HotkeyInfo = new List<HotkeyInfo>(), HotkeyOptions = new List<HotkeyOption>() } };
            List<HotkeySettings> hotkeySettingsConfig = new List<HotkeySettings>() { new HotkeySettings() { ModelName = "DDOM", SerialNumber = "DDPM", ServiceTag = "DDPM", HotkeyInfo = new List<HotkeyInfo>(), HotkeyOptions = new List<HotkeyOption>() } };
            List<ColorPresetSettings> colorPresetSettingsConfigs = new List<ColorPresetSettings>() { new ColorPresetSettings() { } };
            string WriteHotkeySettings_path1_ = "test_WriteHotkeySettingspath.json";
            string jsonData = "{\"hotkeySettings\":[{\"DeviceInfo\":null,\"HotkeyOptions\":[],\"HotkeyInfo\":[{\"Description\":\"Testhotkey\",\"Hotkey\":[],\"Status\":\"Registered\",\"Job\":\"BrightnessIncrease\",\"InputSource\":[]}]}]}";
            File.WriteAllText(WriteHotkeySettings_path1_, jsonData);
            PrivateObject privateSettingsManagerObject = new PrivateObject(SettingsManagerSAPlugin);
            string WriteHotkeySettings_path2_ = Environment.CurrentDirectory + "\\" + WriteHotkeySettings_path1_;
            privateSettingsManagerObject.SetFieldOrProperty("_hotkeysettings_path", WriteHotkeySettings_path2_);
            string settingsAccessInfo = "Test settings AccessInfo Calculate for Test verify";
            //privateSettingsManagerObject.SetFieldOrProperty("_settingsAccessInfo", settingsAccessInfo);
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
            List<HotkeySettings> hotkeySettingsConfig = new List<HotkeySettings>() { new HotkeySettings() { ModelName = "DDPM", SerialNumber = "DDPM", ServiceTag = "DDPM", HotkeyInfo = new List<HotkeyInfo>(), HotkeyOptions = new List<HotkeyOption>() } };
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
            GlobalSettingParam _GlobalSettingParam = new GlobalSettingParam();
            _GlobalSettingParam = null;
            if (!File.Exists(GlobalSettings_path1_))
            {
                ReadGlobalSettingsResult1 = SettingsManagerSAPlugin.ReadGlobalSettings().Result;  // strFilePath is not Exists GlobalSettings_path1_ GlobalSetting.json"
                Assert.That(_GlobalSettingParam, Is.EqualTo(ReadGlobalSettingsResult1)); //run finnish will create path1_
            }
            File.WriteAllText(GlobalSettings_path1_, jsonData);
            string GlobalSettings_path2_ = Environment.CurrentDirectory + "\\" + GlobalSettings_path1_;
            privateSettingsManagerObject.SetFieldOrProperty("_GlobalSetting_path", GlobalSettings_path2_);
            string settingsAccessInfo = "Test settings AccessInfo Calculate for Test verify";
            //privateSettingsManagerObject.SetFieldOrProperty("_settingsAccessInfo", settingsAccessInfo);

            if (File.Exists(GlobalSettings_path1_))
            {
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
            GlobalSettingParam? globalSettingParamNull;
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
            string writeglobalSettings_path2_ = Environment.CurrentDirectory + "\\" + writeglobalSettings_path1_;
            privateSettingsManagerObject.SetFieldOrProperty("_GlobalSetting_path", writeglobalSettings_path2_);
            string settingsAccessInfo = "Test settings AccessInfo Calculate for Test verify";
            //privateSettingsManagerObject.SetFieldOrProperty("_settingsAccessInfo", settingsAccessInfo);
            if (globalSettingParamNull == null)
            {
                var WriteGlobalSettingsResult1 = SettingsManagerSAPlugin.WriteGlobalSettings(globalSettingParamNull).Result;  //globalSettingParam is null
                Assert.That(writeGlobalSettings_, Is.EqualTo(WriteGlobalSettingsResult1));
            }

            if (globalSettingParam != null)
            {
                Mock<ISettingsManagerSA> mock_SysSettingsPlugin = new Mock<ISettingsManagerSA>();
                mock_SysSettingsPlugin.Setup(x => x.WriteGlobalSettingsToITConfig(It.IsAny<GlobalSettingParam>(), false)).Returns(Task.FromResult(true));
                var mock_SysSettingsPluginObj = mock_SysSettingsPlugin.Object;
                privateSettingsManagerObject.SetFieldOrProperty("_SysSettingsPlugin", mock_SysSettingsPluginObj);
                privateSettingsManagerObject.SetFieldOrProperty("_GlobalSettingParam", globalSettingParam);
                var WriteGlobalSettingsResult2 = SettingsManagerSAPlugin.WriteGlobalSettings(globalSettingParam).Result;  //globalSettingParam is not  null
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
            //var RunSerializeObjectresult = (string)privateSettingsManagerObject.Invoke("RunSerializeObject", globalSettingParam);  //RunSerializeGlobalSettingObject method is removed in SettingsManagerSA.cs
            //Assert.Greater(RunSerializeObjectresult.Length, 0); 
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

        /*[Test]
        public void TestReadPowerNapSettings()
        {
            string ReadPowerNapSettings_path1_ = "test_ReadPowerNapSettingsPath.json";
            string PowerNapSettingsjsonData = "[{\"ModelName\":\"TestU2724\",\"SerialNumber\":\"123456789\",\"Status\":false,\"RunType\":0}]";
            List<PowerNapSetting> powerNapSettings_ = new List<PowerNapSetting>();
            PrivateObject privateSettingsManagerObject = new PrivateObject(SettingsManagerSAPlugin);
            privateSettingsManagerObject.SetFieldOrProperty("_powerNapsettings_path", ReadPowerNapSettings_path1_);
            privateSettingsManagerObject.SetFieldOrProperty("_powerNapSettings", powerNapSettings_);
            List<PowerNapSetting> _powerNapSettings = new List<PowerNapSetting>();
            _powerNapSettings = null;
            if (!File.Exists(ReadPowerNapSettings_path1_))
            {
                var ReadPowerNapSettingsResult1 = SettingsManagerSAPlugin.ReadPowerNapSettings().Result;  // strFilePath is not Exists PowerNapSettings.json"
                Assert.That(_powerNapSettings, Is.EqualTo(ReadPowerNapSettingsResult1)); //run finnish will create path
            }
            File.WriteAllText(ReadPowerNapSettings_path1_, PowerNapSettingsjsonData);
            if (File.Exists(ReadPowerNapSettings_path1_))
            {
                var ReadPowerNapSettingsResult2 = SettingsManagerSAPlugin.ReadPowerNapSettings().Result;
                var powerNap_settings = (List<PowerNapSetting>)privateSettingsManagerObject.GetFieldOrProperty("_powerNapSettings");
                Assert.That(powerNap_settings, Is.EqualTo(ReadPowerNapSettingsResult2));
                File.Delete(ReadPowerNapSettings_path1_);
            }
        }*/

        /*[Test]
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
            string WritePowerNapSettings_path2_ = Environment.CurrentDirectory + "\\" + WritePowerNapSettings_path1_;
            privateSettingsManagerObject.SetFieldOrProperty("_powerNapsettings_path", WritePowerNapSettings_path2_);
            string settingsAccessInfo = "Test settings AccessInfo Calculate for Test verify";
            //privateSettingsManagerObject.SetFieldOrProperty("_settingsAccessInfo", settingsAccessInfo);

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
        }*/

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

        /*[Test]
        public void TestImportPowerNapSettings()
        {
            //string ImportPowerNapSettings_path = "C:\\Users\\win1020231116\\AppData\\Local\\Dell\\Dell Display and Peripheral Manager";
            List<PowerNapSetting> powerNapSettingsConfig = new List<PowerNapSetting>() { new PowerNapSetting() { ModelName = "TestU2724", SerialNumber = "123456789", Status = false, RunType = 0 } };
            string ImportPowerNapSettings_path1_ = "test_ImportPowerNapSettingsPath.json";
            string PowerNapSettingsjsonData = "[{\"ModelName\":\"TestU2724\",\"SerialNumber\":\"123456789\",\"Status\":false,\"RunType\":0}]";
            File.WriteAllText(ImportPowerNapSettings_path1_, PowerNapSettingsjsonData);
            PrivateObject privateSettingsManagerObject = new PrivateObject(SettingsManagerSAPlugin);
            privateSettingsManagerObject.SetFieldOrProperty("_powerNapSettings", powerNapSettingsConfig);
            var ImportPowerNapSettingsResult1 = SettingsManagerSAPlugin.ImportPowerNapSettings(ImportPowerNapSettings_path1_).Result;
            Assert.Greater(ImportPowerNapSettingsResult1.Count, 0);
            File.Delete(ImportPowerNapSettings_path1_);
        }*/

        /*[Test]
        public void TestExportPowerNapSettings()
        {
            bool ExportPowerNapSettings_ = false;
            bool ExportPowerNapSettings_succeed = true;
            List<PowerNapSetting> powerNapSettingsNull;
            powerNapSettingsNull = null;
            List<PowerNapSetting> powerNapSettingsConfig = new List<PowerNapSetting>() { new PowerNapSetting() { ModelName = "TestU2724", SerialNumber = "123456789", Status = false, RunType = 0 } };
            string ExportPowerNapSettings_path1_ = "test_ExportPowerNapSettingsPath.json";
            string PowerNapSettingsjsonData = "[{\"ModelName\":\"TestU2724\",\"SerialNumber\":\"123456789\",\"Status\":false,\"RunType\":0}]";
            File.WriteAllText(ExportPowerNapSettings_path1_, PowerNapSettingsjsonData);
            PrivateObject privateSettingsManagerObject = new PrivateObject(SettingsManagerSAPlugin);
            privateSettingsManagerObject.SetFieldOrProperty("_powerNapsettings_path", ExportPowerNapSettings_path1_);
            if (powerNapSettingsNull == null)
            {
                var ExportPowerNapSettings_Result1 = SettingsManagerSAPlugin.ExportPowerNapSettings(powerNapSettingsNull, ExportPowerNapSettings_path1_).Result;
                Assert.That(ExportPowerNapSettings_, Is.EqualTo(ExportPowerNapSettings_Result1));
            }

            if (powerNapSettingsConfig != null)
            {
                var ExportPowerNapSettings_Result2 = SettingsManagerSAPlugin.ExportPowerNapSettings(powerNapSettingsConfig, ExportPowerNapSettings_path1_).Result;
                Assert.That(ExportPowerNapSettings_succeed, Is.EqualTo(ExportPowerNapSettings_Result2));
                File.Delete(ExportPowerNapSettings_path1_);
            }
        }*/

        //Robert_Lin, 2024-10-10, SettingsManagerSAPlugin.ReadEasyArrangeSettings() has been removed,
        //please use DeviceManagerPlugin.ReadEAMonitorSettings() instead.
        //[Test]
        //public void TestReadEasyArrangeSettings()
        //{
        //    string monitorModel = "TestU2724";
        //    string serialNumber = "123456789";
        //    EAMonitorSettings eAMonitorSettings = new EAMonitorSettings();
        //    eAMonitorSettings = null;
        //    var ReadEasyArrangeSettingsResult = SettingsManagerSAPlugin.ReadEasyArrangeSettings(monitorModel, serialNumber).Result;
        //    Assert.That(eAMonitorSettings, Is.EqualTo(ReadEasyArrangeSettingsResult));
        //}


        //Robert_Lin, 2024-10-10, SettingsManagerSAPlugin.ReadEasyArrangeSettings() has been removed,
        //please use DeviceManagerPlugin.WriteEAMonitorSettings() instead.
        //[Test]
        //public void TestWriteEasyArrangeSettings()
        //{
        //    EAMonitorSettings eAMonitorSettings1 = new EAMonitorSettings()
        //    {
        //        CustomList = new List<SplitJson>(),
        //        //Robert_Lin, 2024-9-24 below 3 properties has been moved to EzSettings class
        //        // Which is one member of DDPMUserSetting (original in DDPMMonitorSettings)
        //        //
        //        //IsOnlyAllowWhenShiftKeyPressed = false,
        //        //IsSpanAcrossMultiMonitors = false,
        //        //IsWidthoutGap = true,
        //        RecentList = new List<SplitJson>(),
        //        SelectedSplit = new SplitJson()
        //    };
        //    string WriteEasyArrangeSettings_Success = "OK";
        //    var WriteEasyArrangeSettingsResult = SettingsManagerSAPlugin.WriteEasyArrangeSettings(eAMonitorSettings1).Result;
        //    Assert.That(WriteEasyArrangeSettings_Success, Is.EqualTo(WriteEasyArrangeSettingsResult));
        //}

        [Test]
        public void TestInitializeSysSettingsPlugin()
        {
            Mock<ISettingsManagerSA> SysSettingsPlugin_ = new Mock<ISettingsManagerSA>();
            PrivateObject privateSettingsManagerObject = new PrivateObject(SettingsManagerSAPlugin);
            var SysSettingsPlugin_Obj = SysSettingsPlugin_.Object;
            privateSettingsManagerObject.SetFieldOrProperty("_SysSettingsPlugin", SysSettingsPlugin_Obj);
            privateSettingsManagerObject.Invoke("InitializeSysSettingsPlugin");
            var result = privateSettingsManagerObject.GetFieldOrProperty("_SysSettingsPlugin");
            Assert.That(SysSettingsPlugin_Obj, Is.EqualTo(result));
        }

        [Test]
        public void TestReadRegistryData()
        {
            RegistryHive hive = RegistryHive.CurrentUser;
            string keyPath = "TestReadRegistry_Path";
            string keyName = "TestReadRegistry_Name";
            object ReadRegistryDataObj;
            ReadRegistryDataObj = new object();
            Mock<ISettingsManagerSA> SysSettingsPlugin = new Mock<ISettingsManagerSA>();
            SysSettingsPlugin.Setup(x => x.ReadRegistryData(It.IsAny<RegistryHive>(), It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult(ReadRegistryDataObj));
            PrivateObject privateSettingsManagerObject = new PrivateObject(SettingsManagerSAPlugin);
            var SysSettingsPluginObj = SysSettingsPlugin.Object;
            privateSettingsManagerObject.SetFieldOrProperty("_SysSettingsPlugin", SysSettingsPluginObj);
            if (hive == RegistryHive.LocalMachine || hive == RegistryHive.CurrentUser)
            {
                if (SysSettingsPlugin != null)
                {
                    var ReadRegistryData_Result = SettingsManagerSAPlugin.ReadRegistryData(hive, keyPath, keyName).Result;  //SysSettingsPlugin not null;
                    Assert.That(ReadRegistryDataObj, Is.EqualTo(ReadRegistryData_Result));
                }
            }

            var SysSettingsPluginNull = SysSettingsPlugin;
            SysSettingsPluginNull = null;
            RegistryHive hive2 = RegistryHive.LocalMachine;
            privateSettingsManagerObject.SetFieldOrProperty("_SysSettingsPlugin", SysSettingsPluginNull);
            object resvalue = null;
            if (hive2 == RegistryHive.LocalMachine || hive2 == RegistryHive.CurrentUser)
            {
                if (SysSettingsPluginNull == null)
                {
                    var ReadRegistryData_Result2 = SettingsManagerSAPlugin.ReadRegistryData(hive2, keyPath, keyName).Result;  //SysSettingsPlugin null;
                    Assert.That(resvalue, Is.EqualTo(ReadRegistryData_Result2));
                }
            }
        }

        [Test]
        public void TestWriteRegistryData()
        {
            bool writeRegistryDataFail = false;
            bool writeRegistryDataSuccess = true;
            RegistryHive hive = RegistryHive.CurrentUser;
            string keyPath = "TestWriteRegistry_Path";
            string keyName = "TestWriteRegistry_Name";
            object WriteRegistryDataObj;
            WriteRegistryDataObj = new object();
            Mock<ISettingsManagerSA> SysSettingsPlugin = new Mock<ISettingsManagerSA>();
            SysSettingsPlugin.Setup(x => x.WriteRegistryData(It.IsAny<RegistryHive>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object>())).Returns(Task.FromResult(writeRegistryDataSuccess));
            PrivateObject privateSettingsManagerObject = new PrivateObject(SettingsManagerSAPlugin);
            var SysSettingsPluginObj = SysSettingsPlugin.Object;
            privateSettingsManagerObject.SetFieldOrProperty("_SysSettingsPlugin", SysSettingsPluginObj);
            if (hive == RegistryHive.LocalMachine || hive == RegistryHive.CurrentUser)
            {
                if (SysSettingsPlugin != null)
                {
                    var WriteRegistryData_Result = SettingsManagerSAPlugin.WriteRegistryData(hive, keyPath, keyName, WriteRegistryDataObj).Result;  //SysSettingsPlugin not null;
                    Assert.That(writeRegistryDataSuccess, Is.EqualTo(WriteRegistryData_Result));
                }
            }

            RegistryHive hive2 = RegistryHive.LocalMachine;
            var SysSettingsPluginNull = SysSettingsPlugin;
            SysSettingsPluginNull = null;
            privateSettingsManagerObject.SetFieldOrProperty("_SysSettingsPlugin", SysSettingsPluginNull);
            if (hive2 == RegistryHive.LocalMachine || hive2 == RegistryHive.CurrentUser)
            {
                if (SysSettingsPluginNull == null)
                {
                    var WriteRegistryData_Result2 = SettingsManagerSAPlugin.WriteRegistryData(hive, keyPath, keyName, WriteRegistryDataObj).Result;  //SysSettingsPlugin null;
                    Assert.That(writeRegistryDataFail, Is.EqualTo(WriteRegistryData_Result2));
                }
            }
        }

        [Test]
        public void TestSetAppConfigData()
        {
            PrivateObject privateSettingsManagerObject = new PrivateObject(SettingsManagerSAPlugin);
            var Ddpm_app_ = new DDPMAppSettings();
            var Ddpm_user_ = new DDPMUserSettings()
            {
                Version = 1.0,
                Language = (int)Languages.en,
                IsSynchronizemonitor = false,
                Schedule = string.Empty,
                DelayFWUpdateInfoPackage = new FWUpdateInfoPackage(),
                LockRotate = true,
                //isTelemetryConsentOn = true,
                LockFWU_UI = true,
                UODFWUInfoPackage = new DokcUODUpdateInfoPackage(),
                SupportedMonitorList = new List<string> { "Testmonitor1", "TestMonitor2" },
                DelaySWUpdateInfoPackage = new SWUpdateInfoPackage(),
            };
            var Ddpm_it_ = new DDPMITConfig()
            {
                Lock_Settings_TelemetryConsent = false,
                Lock_Settings_Updates = false,
                Lock_Display_ExportSettings = false,
                Lock_Setting_RestoreDefaults = false,
                Lock_Display_BriCont = false,
                Lock_Display_AutoBriTemp = false,
                Lock_Display_NetworkKVM = false,
                Lock_Display_ColorPreset = false,
                Lock_Display_PowerNap = false,
                Lock_Display_ResolutionRefreshRate = false,
                Lock_Display_USBCPrioritization = false,
                Lock_Display_ActiveInputSource = false,
                Lock_Webcam_RestoreFactoryDefaults = false,
                Lock_Audio_RestoreFactoryDefaults = false,
                Lock_Keyboard_RestoreFactoryDefaults = false,
                Lock_Mouse_RestoreFactoryDefaults = false,
                Lock_Pen_RestoreFactoryDefaults = false,
                Lock_Keyboard_CollabScreenShare = false,
            };
            bool SetAppConfigData_ = false;
            var settings_Data = new DDPMSettings(Ddpm_app_, Ddpm_user_, Ddpm_it_);
            DDPMSettings data_ = null;
            if (data_ == null)
            {
                var SetAppConfigDataResult1 = SettingsManagerSAPlugin.SetAppConfigData(data_).Result; //DDPMSettings data_ is null 
                Assert.That(SetAppConfigDataResult1, Is.EqualTo(SetAppConfigData_));
            }
            string accessInfo_ = "testaccessinfo";
            string serialized_string = "{\"key\":\"value\"}";
            string target_file = "testSetAppConfigDatafile.json";
            File.WriteAllText(target_file, serialized_string);
            //privateSettingsManagerObject.SetFieldOrProperty("_settingsAccessInfo", accessInfo_);
            privateSettingsManagerObject.SetFieldOrProperty("_settings_path", target_file);
            if (settings_Data != null)
            {
                var SetAppConfigDataResult2 = SettingsManagerSAPlugin.SetAppConfigData(settings_Data).Result; //DDPMSettings is  not null 
                Assert.That(SetAppConfigDataResult2, Is.EqualTo(SetAppConfigData_));
                File.Delete(target_file);
            }
        }

        [Test]
        public void TestInitDDPMMonitorConfigFile()
        {
            string modelname = "TestU2724DD";
            bool binit = false;
            List<DDPMMonitorSettings> dDPMMonitorSettingsList = new List<DDPMMonitorSettings>();
            var Result = SettingsManagerSAPlugin.InitDDPMMonitorConfigFile(modelname, out binit).Result;  // ReloadMonitorSettings is null
            PrivateObject privatesettingsManagerObj = new PrivateObject(SettingsManagerSAPlugin);
            var _allDDPMMonitorSettings = (Dictionary<string, List<DDPMMonitorSettings>>)privatesettingsManagerObj.GetFieldOrProperty("_AllMonitorSettings");
            Assert.IsNotNull(Result);
            Assert.Greater(_allDDPMMonitorSettings.Count, 0);
            Assert.IsTrue(_allDDPMMonitorSettings.ContainsKey(modelname));
        }

        [Test]
        public void TestReloadAppConfigData()
        {
            bool force_reload = false;
            PrivateObject privateSettingsManagerObject = new PrivateObject(SettingsManagerSAPlugin);
            var Ddpm_app_ = new DDPMAppSettings();
            var Ddpm_user_ = new DDPMUserSettings()
            {
                Version = 1.0,
                Language = (int)Languages.en,
                IsSynchronizemonitor = false,
                Schedule = string.Empty,
                DelayFWUpdateInfoPackage = new FWUpdateInfoPackage(),
                LockRotate = true,
                //isTelemetryConsentOn = true,
                LockFWU_UI = true,
                UODFWUInfoPackage = new DokcUODUpdateInfoPackage(),
                SupportedMonitorList = new List<string> { "Testmonitor1", "TestMonitor2" },
                DelaySWUpdateInfoPackage = new SWUpdateInfoPackage(),
            };
            var Ddpm_it_ = new DDPMITConfig()
            {
                Lock_Settings_TelemetryConsent = false,
                Lock_Settings_Updates = false,
                Lock_Display_ExportSettings = false,
                Lock_Setting_RestoreDefaults = false,
                Lock_Display_BriCont = false,
                Lock_Display_AutoBriTemp = false,
                Lock_Display_NetworkKVM = false,
                Lock_Display_ColorPreset = false,
                Lock_Display_PowerNap = false,
                Lock_Display_ResolutionRefreshRate = false,
                Lock_Display_USBCPrioritization = false,
                Lock_Display_ActiveInputSource = false,
                Lock_Webcam_RestoreFactoryDefaults = false,
                Lock_Audio_RestoreFactoryDefaults = false,
                Lock_Keyboard_RestoreFactoryDefaults = false,
                Lock_Mouse_RestoreFactoryDefaults = false,
                Lock_Pen_RestoreFactoryDefaults = false,
                Lock_Keyboard_CollabScreenShare = false,
            };
            var settings_Data = new DDPMSettings(Ddpm_app_, Ddpm_user_, Ddpm_it_);
            string _settings_pathNull = string.Empty;
            privateSettingsManagerObject.SetFieldOrProperty("_settings_path", _settings_pathNull);
            privateSettingsManagerObject.SetFieldOrProperty("_SysSettingsPlugin", null);
            var ddpm_it = new DDPMITConfig();

            var ReloadAppConfigDataResult0 = SettingsManagerSAPlugin.ReloadAppConfigData(force_reload).Result; //_settings_pathNull  is null , _settings not null,_SysSettingsPlugin is null
            Assert.IsNotNull(ReloadAppConfigDataResult0);

            Mock<ISettingsManagerSA> SysSettingsPlugin = new Mock<ISettingsManagerSA>();
            SysSettingsPlugin.Setup(x => x.GetITGlobalConfigs(It.IsAny<bool>())).Returns(Task.FromResult(Ddpm_it_));
            var SysSettingsPluginObj = SysSettingsPlugin.Object;
            privateSettingsManagerObject.SetFieldOrProperty("_SysSettingsPlugin", SysSettingsPluginObj);  //_SysSettingsPlugin is not null

            //_settings_pathNull null            
            var ReloadAppConfigDataResult1 = SettingsManagerSAPlugin.ReloadAppConfigData(force_reload).Result; //_settings_pathNull  is null , _settings not null
            Assert.IsNotNull(ReloadAppConfigDataResult1);
            Assert.That(ReloadAppConfigDataResult1.LockSettings, Is.EqualTo(settings_Data.LockSettings));

            string accessInfo_ = "testaccessinfo";
            string serialized_string = "{\"key\":\"value\"}";
            string settings_path_target_file = "testReloadAppConfigDatafile.json";
            File.WriteAllText(settings_path_target_file, serialized_string);

            privateSettingsManagerObject.SetFieldOrProperty("_settings_path", settings_path_target_file);
            privateSettingsManagerObject.SetFieldOrProperty("_settings", null);

            // settings_path_target_file Is not NullOrEmpty
            var ReloadAppConfigDataResult2 = SettingsManagerSAPlugin.ReloadAppConfigData(force_reload).Result; //settings_path_target_file is  not null , _settings=null
            Assert.IsNotNull(ReloadAppConfigDataResult2);

            //settings_Data not null
            privateSettingsManagerObject.SetFieldOrProperty("_settings", settings_Data);
            var ReloadAppConfigDataResult3 = SettingsManagerSAPlugin.ReloadAppConfigData(force_reload).Result; //settings_path_target_file is  not null , _settings not null
            Assert.IsNotNull(ReloadAppConfigDataResult3);
            Assert.That(ReloadAppConfigDataResult3.LockSettings, Is.EqualTo(settings_Data.LockSettings));
            File.Delete(settings_path_target_file);
        }

        [Test]
        public void TestRunMonitorListDeserializeObject()
        {
            double version = 1.0;
            string model = "TestModel";
            string serviceTag = "12345";
            PrivateObject privatesettingsManagerObj = new PrivateObject(SettingsManagerSAPlugin);
            string MonitorListjsonData = "[{\"Version\":1.0,\"Model\":\"TestModel\",\"ServiceTag\":\"12345\",\"Input\":{},\"KVM\":{},\"VCPs\":[],\"EA\":{}}]";
            var MonitorListDesResult = (List<DDPMMonitorSettings>)privatesettingsManagerObj.Invoke("RunMonitorListDeserializeObject", MonitorListjsonData);
            Assert.IsNotNull(MonitorListDesResult);
            Assert.That(version, Is.EqualTo(MonitorListDesResult[0].Version));
            Assert.That(model, Is.EqualTo(MonitorListDesResult[0].Model));
            Assert.That(serviceTag, Is.EqualTo(MonitorListDesResult[0].ServiceTag));
        }

        [Test]
        public void TestReloadMonitorSettings()
        {
            string modelname = "TestU2724DD";
            string Settings_path1 = string.Empty;
            List<DDPMMonitorSettings> dDPMMonitorSettingsList = new List<DDPMMonitorSettings>();
            PrivateObject privatesettingsManagerObj = new PrivateObject(SettingsManagerSAPlugin);
            string monitorSettings_path = "TestU2724DD.json";
            string displayPath = "TestDisplayPath";
            double Version3 = 1.0f;
            string Model3 = "TestU2724DF";
            string ServiceTag3 = "12345";
            string MonitorListjsonData = "[{\"Version\":1.0,\"Model\":\"TestModel\",\"ServiceTag\":\"12345\",\"Input\":{},\"KVM\":{},\"VCPs\":[],\"EA\":{}}]";
            Dictionary<string, List<DDPMMonitorSettings>> _allMonitorSettings = new Dictionary<string, List<DDPMMonitorSettings>>();
            DDPMMonitorSettings settings = new DDPMMonitorSettings
            {
                Version = 1.0f,
                Model = "TestU2724DF",
                ServiceTag = "12345",
                Input = new InputSource() { strInputSourceList = "HDMI-1" },
                KVM = new KVMSettings() { strUSBKVMPCsList = "teststrUSBKVMPCsList", isOnUSBKVM = true, isOnNKVM = false },
                VCPs = new List<VCPCode> { new VCPCode() { Code = 10, Value = new List<int>(20) } },
                //EA = new EAMonitorSettings() { IsWidthoutGap = true, }
            };
            _allMonitorSettings.Add("TestU2724DD", new List<DDPMMonitorSettings> { settings });
            Dictionary<string, List<DDPMMonitorSettings>> _allMonitorSettings4 = new Dictionary<string, List<DDPMMonitorSettings>>();
            _allMonitorSettings4 = null;
            double version4 = 1.0;
            string model4 = "TestModel";
            string serviceTag4 = "12345";
            if (!string.IsNullOrEmpty(displayPath))
            {
                privatesettingsManagerObj.SetFieldOrProperty("_display_path", displayPath);
                privatesettingsManagerObj.SetFieldOrProperty("_AllMonitorSettings", _allMonitorSettings);
                if (!File.Exists(monitorSettings_path))
                {
                    var ReloadMonitorSettingsResult2 = SettingsManagerSAPlugin.ReloadMonitorSettings(modelname).Result;  // displayPath is not null,file no monitorSettings_path
                    Assert.That(dDPMMonitorSettingsList, Is.EqualTo(ReloadMonitorSettingsResult2));
                }
                File.WriteAllText(monitorSettings_path, MonitorListjsonData);
                var displayPath3 = Environment.CurrentDirectory;
                if (File.Exists(monitorSettings_path))
                {
                    privatesettingsManagerObj.SetFieldOrProperty("_display_path", displayPath3);
                    var ReloadMonitorSettingsResult3 = SettingsManagerSAPlugin.ReloadMonitorSettings(modelname).Result;  // displayPath is not null, file exist monitorSettings_path, _AllMonitorSettings is not null 
                    Assert.Greater(ReloadMonitorSettingsResult3.Count, 0);
                    Assert.That(Version3, Is.EqualTo(ReloadMonitorSettingsResult3[0].Version));
                    Assert.That(Model3, Is.EqualTo(ReloadMonitorSettingsResult3[0].Model));
                    Assert.That(ServiceTag3, Is.EqualTo(ReloadMonitorSettingsResult3[0].ServiceTag));
                }

                if (_allMonitorSettings4 == null)
                {
                    privatesettingsManagerObj.SetFieldOrProperty("_AllMonitorSettings", _allMonitorSettings4);
                    var ReloadMonitorSettingsResult4 = SettingsManagerSAPlugin.ReloadMonitorSettings(modelname).Result;  // displayPath is not null, file exist monitorSettings_path, _AllMonitorSettings is null 
                    Assert.Greater(ReloadMonitorSettingsResult4.Count, 0);
                    Assert.That(version4, Is.EqualTo(ReloadMonitorSettingsResult4[0].Version));
                    Assert.That(model4, Is.EqualTo(ReloadMonitorSettingsResult4[0].Model));
                    Assert.That(serviceTag4, Is.EqualTo(ReloadMonitorSettingsResult4[0].ServiceTag));
                    File.Delete(monitorSettings_path);
                }
            }
            if (string.IsNullOrEmpty(Settings_path1))
            {
                privatesettingsManagerObj.SetFieldOrProperty("_settings_path", Settings_path1);
                var ReloadMonitorSettingsResult1 = SettingsManagerSAPlugin.ReloadMonitorSettings(modelname).Result;  // Settings_path1 is null
                Assert.That(dDPMMonitorSettingsList, Is.EqualTo(ReloadMonitorSettingsResult1));
            }
        }

        [Test]
        public void TestWriteMonitorSettings()
        {
            string modelname = "TestU2724DD";
            List<DDPMMonitorSettings> dDPMMonitorSettingsList = new List<DDPMMonitorSettings>();
            dDPMMonitorSettingsList = null;
            bool WriteMonitorSettingsF = false;
            bool WriteMonitorSettingsP = true;
            List<DDPMMonitorSettings> dDPMMonitorSettingsList2 = new List<DDPMMonitorSettings>();
            PrivateObject privatesettingsManagerObj = new PrivateObject(SettingsManagerSAPlugin);
            string MonitorListjsonData = "[{\"Version\":1.0,\"Model\":\"TestModel\",\"ServiceTag\":\"12345\",\"Input\":{},\"KVM\":{},\"VCPs\":[],\"EA\":{}}]";
            DDPMMonitorSettings settings = new DDPMMonitorSettings
            {
                Version = 1.0f,
                Model = "TestU2724DD",
                ServiceTag = "12345",
                Input = new InputSource() { strInputSourceList = "HDMI-1" },
                KVM = new KVMSettings() { strUSBKVMPCsList = "teststrUSBKVMPCsList", isOnUSBKVM = true, isOnNKVM = false },
                VCPs = new List<VCPCode> { new VCPCode() { Code = 10, Value = new List<int>(20) } },
                //EA = new EAMonitorSettings() { IsWidthoutGap = true, }
            };
            dDPMMonitorSettingsList2.Add(settings);
            Dictionary<string, List<DDPMMonitorSettings>> _allMonitorSettings2 = new Dictionary<string, List<DDPMMonitorSettings>>();
            if (dDPMMonitorSettingsList == null)
            {
                var WriteMonitorSettingsResult = SettingsManagerSAPlugin.WriteMonitorSettings(modelname, dDPMMonitorSettingsList).Result;  // dDPMMonitorSettingsList is null
                Assert.That(WriteMonitorSettingsF, Is.EqualTo(WriteMonitorSettingsResult));
            }
            privatesettingsManagerObj.SetFieldOrProperty("_AllMonitorSettings", _allMonitorSettings2);
            if (dDPMMonitorSettingsList2 != null)
            {
                string _display_path = "TestU2724DD.json";
                string _display_path2 = string.Empty;
                privatesettingsManagerObj.SetFieldOrProperty("_display_path", _display_path2);
                File.WriteAllText(_display_path, MonitorListjsonData);
                var WriteMonitorSettingsResult2 = SettingsManagerSAPlugin.WriteMonitorSettings(modelname, dDPMMonitorSettingsList2).Result;  // dDPMMonitorSettingsList is not null,
                var Write_AllMonitorSettings = (Dictionary<string, List<DDPMMonitorSettings>>)privatesettingsManagerObj.GetFieldOrProperty("_AllMonitorSettings");
                Assert.That(WriteMonitorSettingsP, Is.EqualTo(WriteMonitorSettingsResult2));
                Assert.Greater(Write_AllMonitorSettings.Count, 0);
                Assert.That(Write_AllMonitorSettings.ContainsKey(modelname));
                File.Delete(_display_path);
            }
        }

        [Test]
        public void TestRunDDPMMonitorSettingsSerializeObject()
        {
            string _display_path = "TestDDPMMonitorSettingsSerial.json";
            string modelname = "TestU2724DD";
            List<DDPMMonitorSettings> dDPMMonitorSettingsList = new List<DDPMMonitorSettings>();
            PrivateObject privatesettingsManagerObj = new PrivateObject(SettingsManagerSAPlugin);
            string MonitorListjsonData = "[{\"Version\":1.0,\"Model\":\"TestModel\",\"ServiceTag\":\"12345\",\"Input\":{},\"KVM\":{},\"VCPs\":[],\"EA\":{}}]";
            DDPMMonitorSettings settings = new DDPMMonitorSettings
            {
                Version = 1.0f,
                Model = "TestU2724DD",
                ServiceTag = "12345",
                Input = new InputSource() { strInputSourceList = "HDMI-1" },
                KVM = new KVMSettings() { strUSBKVMPCsList = "teststrUSBKVMPCsList", isOnUSBKVM = true, isOnNKVM = false },
                VCPs = new List<VCPCode> { new VCPCode() { Code = 10, Value = new List<int>(20) } },
                //EA = new EAMonitorSettings() { IsWidthoutGap = true, }
            };
            dDPMMonitorSettingsList.Add(settings);
            string _display_path2 = string.Empty;
            privatesettingsManagerObj.SetFieldOrProperty("_display_path", _display_path2);
            File.WriteAllText(_display_path, MonitorListjsonData);
            var DDPMMonitorSettingsSerializ = (string)privatesettingsManagerObj.Invoke("RunSerializeObject", modelname, dDPMMonitorSettingsList);
            Assert.Greater(DDPMMonitorSettingsSerializ.Length, 0);
            File.Delete(_display_path);
        }

        [Test]
        public void TestRunMonitorListRunSerializeObject()
        {
            string modelname = "TestU2724";
            List<DDPMMonitorSettings> monitorSettings = new List<DDPMMonitorSettings>();
            DDPMMonitorSettings settings = new DDPMMonitorSettings
            {
                Version = 1.0f,
                Model = "TestU2724DD",
                ServiceTag = "12345",
                Input = new InputSource() { strInputSourceList = "HDMI-1" },
                KVM = new KVMSettings() { strUSBKVMPCsList = "teststrUSBKVMPCsList", isOnUSBKVM = true, isOnNKVM = false },
                VCPs = new List<VCPCode> { new VCPCode() { Code = 10, Value = new List<int>(20) } },
                //EA = new EAMonitorSettings() { IsWidthoutGap = true, }
            };
            monitorSettings.Add(settings);
            PrivateObject privatesettingsManagerObj = new PrivateObject(SettingsManagerSAPlugin);
            var MonitorListSerialResult = (string)privatesettingsManagerObj.Invoke("RunSerializeObject", modelname, monitorSettings);
            Assert.IsNotNull(MonitorListSerialResult);
            Assert.Greater(MonitorListSerialResult.Length, 0);
        }

        [Test]
        public void TestReadAllMonitorSettings()
        {
            string _display_path2 = "ReadAllMonitorSettings.json";
            string MonitorListjsonData = "[{\"Version\":1.0,\"Model\":\"TestModel\",\"ServiceTag\":\"12345\",\"Input\":{},\"KVM\":{},\"VCPs\":[],\"EA\":{}}]";
            File.WriteAllText(_display_path2, MonitorListjsonData);
            var displayPath2 = Environment.CurrentDirectory;
            Dictionary<string, List<DDPMMonitorSettings>> _allMonitorSettings = new Dictionary<string, List<DDPMMonitorSettings>>();
            DDPMMonitorSettings settings = new DDPMMonitorSettings
            {
                Version = 1.0f,
                Model = "TestU2724DF",
                ServiceTag = "12345",
                Input = new InputSource() { strInputSourceList = "HDMI-1" },
                KVM = new KVMSettings() { strUSBKVMPCsList = "teststrUSBKVMPCsList", isOnUSBKVM = true, isOnNKVM = false },
                VCPs = new List<VCPCode> { new VCPCode() { Code = 10, Value = new List<int>(20) } },
                //EA = new EAMonitorSettings() { IsWidthoutGap = true, }
            };
            _allMonitorSettings.Add("TestU2724DD", new List<DDPMMonitorSettings> { settings });
            PrivateObject privatesettingsManagerObj = new PrivateObject(SettingsManagerSAPlugin);
            privatesettingsManagerObj.SetFieldOrProperty("_AllMonitorSettings", _allMonitorSettings);
            privatesettingsManagerObj.SetFieldOrProperty("_display_path", displayPath2);
            var ReadAllMonitorSettingsResult = (Dictionary<string, List<DDPMMonitorSettings>>)privatesettingsManagerObj.Invoke("ReadAllMonitorSettings");
            Assert.IsNotNull(ReadAllMonitorSettingsResult);
            Assert.Greater(ReadAllMonitorSettingsResult.Count, 0);
            Assert.That(_allMonitorSettings, Is.EqualTo(_allMonitorSettings));
        }

        [Test]
        public void TestReadImportSettingsFile()
        {
            DDPMImpExpSettings ImpSettings = new DDPMImpExpSettings();
            string path1 = "";
            if (!string.IsNullOrEmpty(path1))
            {
                var ReadImportSettingsFile_result1 = SettingsManagerSAPlugin.ReadImportSettingsFile(path1).Result;
                Assert.IsNotNull(ReadImportSettingsFile_result1);
            }

            string path2 = "Testpath2";
            if (!string.IsNullOrEmpty(path2))
            {
                if (!File.Exists(path2))
                {
                    var ReadImportSettingsFile_result2 = SettingsManagerSAPlugin.ReadImportSettingsFile(path2).Result;
                    Assert.IsNotNull(ReadImportSettingsFile_result2);
                }
            }

            string WriteDDPMImpExpSet_path3_ = "test_WriteDDPMImpExpSetjsonDataPath.json";
            string DDPMImpExpSetjsonData = "{\"AppSettings\":{\"Version\":2.0},\"UserSettings\":{\"Version\":1.5,\"Language\":1},\"MonitorSettings\":{\"Version\":1.2,\"Model\":\"TestModel\",\"ServiceTag\":\"12345\",\"Input\":{},\"KVM\":{},\"VCPs\":[],\"EA\":{}}}";
            File.WriteAllText(WriteDDPMImpExpSet_path3_, DDPMImpExpSetjsonData);
            PrivateObject privateSettingsManagerObject = new PrivateObject(SettingsManagerSAPlugin);

            string WriteDDPMImpExpSet_path33_ = Environment.CurrentDirectory + "\\" + WriteDDPMImpExpSet_path3_;
            string settingsAccessInfo = "Test settings AccessInfo Calculate for Test verify";
            //privateSettingsManagerObject.SetFieldOrProperty("_settingsAccessInfo", settingsAccessInfo);

            if (!string.IsNullOrEmpty(WriteDDPMImpExpSet_path33_))
            {
                if (File.Exists(WriteDDPMImpExpSet_path33_))
                {
                    var ReadImportSettingsFile_result3 = SettingsManagerSAPlugin.ReadImportSettingsFile(WriteDDPMImpExpSet_path33_).Result;
                    Assert.IsNotNull(ReadImportSettingsFile_result3);
                    File.Delete(WriteDDPMImpExpSet_path33_);
                }
            }

        }

        [Test]
        public void TestRunImpExpDeserializeObject()
        {
            double Version = 2.0;
            double Version2 = 1.5;
            double Version3 = 1.2;
            int Language = 1;
            string Model = "TestModel";
            string ServiceTag = "12345";
            string DDPMImpExpSetjsonData = "{\"AppSettings\":{\"Version\":2.0},\"UserSettings\":{\"Version\":1.5,\"Language\":1},\"MonitorSettings\":{\"Version\":1.2,\"Model\":\"TestModel\",\"ServiceTag\":\"12345\",\"Input\":{},\"KVM\":{},\"VCPs\":[],\"EA\":{}}}";
            PrivateObject privatesettingsManagerObj = new PrivateObject(SettingsManagerSAPlugin);
            var RunDDPMImpExpSettingsDes_Result = (DDPMImpExpSettings)privatesettingsManagerObj.Invoke("RunImpExpDeserializeObject", DDPMImpExpSetjsonData);
            Assert.IsNotNull(RunDDPMImpExpSettingsDes_Result);
            Assert.IsNotNull(RunDDPMImpExpSettingsDes_Result.AppSettings);
            Assert.That(Version, Is.EqualTo(RunDDPMImpExpSettingsDes_Result.AppSettings.Version));
            Assert.IsNotNull(RunDDPMImpExpSettingsDes_Result.UserSettings);
            Assert.That(Version2, Is.EqualTo(RunDDPMImpExpSettingsDes_Result.UserSettings.Version));
            Assert.That(Language, Is.EqualTo(RunDDPMImpExpSettingsDes_Result.UserSettings.Language));
            Assert.IsNotNull(RunDDPMImpExpSettingsDes_Result.MonitorSettings);
            Assert.That(Version3, Is.EqualTo(RunDDPMImpExpSettingsDes_Result.MonitorSettings.Version));
            Assert.That(Model, Is.EqualTo(RunDDPMImpExpSettingsDes_Result.MonitorSettings.Model));
            Assert.That(ServiceTag, Is.EqualTo(RunDDPMImpExpSettingsDes_Result.MonitorSettings.ServiceTag));
        }

        //Robert_Lin, 2024-10-11, EAMonitorSettings property changed
        [Test]
        public void TestWriteImpExpSettings()
        {
            bool WriteImpExpSettingsF = false;
            string WriteImpExpSet_path2 = "WriteImpExpSet.json";
            string MonitorListjsonData = "[{\"Version\":1.0,\"Model\":\"TestModel\",\"ServiceTag\":\"12345\",\"Input\":{},\"KVM\":{},\"VCPs\":[],\"EA\":{}}]";
            File.WriteAllText(WriteImpExpSet_path2, MonitorListjsonData);
            DDPMImpExpSettings dDPMImpExpSettings = new DDPMImpExpSettings()
            {
                AppSettings = new DDPMAppSettings() { Version = 1.0 },
                MonitorSettings = new DDPMMonitorSettings()
                {
                    Version = 2.0,
                    Model = "TestMode",
                    ServiceTag = "123456",
                    EA = new EAMonitorSettings()
                    {
                        SelectedSplit = new SplitJson(),
                        RecentList = new SplitJson[] { },
                        //RecentList = new List<SplitJson>(),
                    },
                    Input = new InputSource()
                    {
                        strInputSourceList = "HDMI=1"
                    },
                    KVM = new KVMSettings()
                    {
                        strUSBKVMPCsList = "TestUSBKVM",
                        isOnNKVM = false,
                        isOnUSBKVM = true,
                    },
                    VCPs = new List<VCPCode>()
            {
              new VCPCode()
              {
                Code=0X12,
                Value=new List<int>() { 1,2}
              }
            },
                }
            };
            DDPMImpExpSettings dDPMImpExpSettingsNull = new DDPMImpExpSettings();
            dDPMImpExpSettingsNull = null;
            bool WriteImpExpSettingsT = true;
            PrivateObject privatesettingsManagerObj = new PrivateObject(SettingsManagerSAPlugin);
            string WriteImpExpSet_path22 = Environment.CurrentDirectory + "\\" + WriteImpExpSet_path2;
            // privatesettingsManagerObj.SetFieldOrProperty("_GlobalSetting_path", WriteImpExpSet_path22);
            string settingsAccessInfo = "Test settings AccessInfo Calculate for Test verify";
            //privatesettingsManagerObj.SetFieldOrProperty("_settingsAccessInfo", settingsAccessInfo);
            if (dDPMImpExpSettingsNull == null)
            {
                var WriteImpExpSettings_Result = (bool)privatesettingsManagerObj.Invoke("WriteImpExpSettings", WriteImpExpSet_path22, dDPMImpExpSettingsNull); //dDPMImpExpSettingsNull null
                Assert.IsNotNull(WriteImpExpSettings_Result);
                Assert.That(WriteImpExpSettingsF, Is.EqualTo(WriteImpExpSettings_Result));
            }
            if (dDPMImpExpSettings != null)
            {
                var WriteImpExpSettings_Result2 = (bool)privatesettingsManagerObj.Invoke("WriteImpExpSettings", WriteImpExpSet_path22, dDPMImpExpSettings); //dDPMImpExpSettings not null
                Assert.IsNotNull(WriteImpExpSettings_Result2);
                Assert.That(WriteImpExpSettingsT, Is.EqualTo(WriteImpExpSettings_Result2));
                File.Delete(WriteImpExpSet_path2);
            }
        }

        [Test]
        public void TestDisplayImportSettings()
        {
            string DisplayImportSettings_path = "TestDDPMImpExpSettings.json";
            string DisplayImportSettings_path2 = "TestU2724DD.json";
            bool isSameModel = false;
            DDPMImpExpSettings TestImpExpSettings = new DDPMImpExpSettings();
            bool DisplayImportSettings1 = false;
            bool DisplayImportSettings2 = true;
            string DDPMImpExpSetjsonData = "{\"AppSettings\":{\"Version\":2.0},\"UserSettings\":{\"Version\":1.5,\"Language\":1},\"MonitorSettings\":{\"Version\":1.2,\"Model\":\"TestU2724DD\",\"ServiceTag\":\"12345\",\"Input\":{},\"KVM\":{},\"VCPs\":[],\"EA\":{}}}";
            File.WriteAllText(DisplayImportSettings_path, DDPMImpExpSetjsonData);
            string DisplayImportSettings_path1 = Environment.CurrentDirectory + "\\" + DisplayImportSettings_path;
            PrivateObject privatesettingsManagerObj = new PrivateObject(SettingsManagerSAPlugin);
            string monitorSettings_path = string.Empty;
            privatesettingsManagerObj.SetFieldOrProperty("_display_path", DisplayImportSettings_path);
            string settingsAccessInfo = "Test settings AccessInfo Calculate for Test verify";
            //privatesettingsManagerObj.SetFieldOrProperty("_settingsAccessInfo", settingsAccessInfo);

            string monitorSettings_path2 = Environment.CurrentDirectory + "\\" + DisplayImportSettings_path2;
            privatesettingsManagerObj.SetFieldOrProperty("_display_path", Environment.CurrentDirectory);
            string MonitorListjsonData = "[{\"Version\":1.0,\"Model\":\"TestModel\",\"ServiceTag\":\"12345\",\"Input\":{},\"KVM\":{},\"VCPs\":[],\"EA\":{}}]";
            File.WriteAllText(DisplayImportSettings_path2, MonitorListjsonData);
            List<DDPMMonitorSettings> dDPMMonitorSettingsList = new List<DDPMMonitorSettings>();
            Dictionary<string, List<DDPMMonitorSettings>> _allMonitorSettings = new Dictionary<string, List<DDPMMonitorSettings>>();
            DDPMMonitorSettings settings = new DDPMMonitorSettings
            {
                Version = 1.0f,
                Model = "TestU2724DF",
                ServiceTag = "12345",
                Input = new InputSource() { strInputSourceList = "HDMI-1" },
                KVM = new KVMSettings() { strUSBKVMPCsList = "teststrUSBKVMPCsList", isOnUSBKVM = true, isOnNKVM = false },
                VCPs = new List<VCPCode> { new VCPCode() { Code = 10, Value = new List<int>(20) } },
                EA = new EAMonitorSettings(),
            };
            _allMonitorSettings.Add("TestU2724DD", new List<DDPMMonitorSettings> { settings }); //ReloadMonitorSettings
            privatesettingsManagerObj.SetFieldOrProperty("_AllMonitorSettings", _allMonitorSettings);

            if (monitorSettings_path2 != null)
            {
                //var DisplayImportSettings_result2 = SettingsManagerSAPlugin.DisplayImportSettings(DisplayImportSettings_path, isSameModel, out TestImpExpSettings).Result; //file exist monitorSettings_path
                // Assert.That(DisplayImportSettings2, Is.EqualTo(DisplayImportSettings_result2));
            }
            File.Delete(DisplayImportSettings_path);
            File.Delete(DisplayImportSettings_path2);

        }

        [Test]
        public void TestDisplayExportSettings()
        {
            string modelname = "TestU2724DD";
            string seriveTag = "12345";
            string path = "testpath";

            PrivateObject privateSettingsManagerObject = new PrivateObject(SettingsManagerSAPlugin);
            var Ddpm_app_ = new DDPMAppSettings();
            var Ddpm_user_ = new DDPMUserSettings()
            {
                Version = 1.0,
                Language = (int)Languages.en,
                IsSynchronizemonitor = false,
                Schedule = string.Empty,
                DelayFWUpdateInfoPackage = new FWUpdateInfoPackage(),
                LockRotate = true,
                //isTelemetryConsentOn = true,
                LockFWU_UI = true,
                UODFWUInfoPackage = new DokcUODUpdateInfoPackage(),
                SupportedMonitorList = new List<string> { "Testmonitor1", "TestMonitor2" },
                DelaySWUpdateInfoPackage = new SWUpdateInfoPackage(),
            };
            var Ddpm_it_ = new DDPMITConfig()
            {
                Lock_Settings_TelemetryConsent = false,
                Lock_Settings_Updates = false,
                Lock_Display_ExportSettings = false,
                Lock_Setting_RestoreDefaults = false,
                Lock_Display_BriCont = false,
                Lock_Display_AutoBriTemp = false,
                Lock_Display_NetworkKVM = false,
                Lock_Display_ColorPreset = false,
                Lock_Display_PowerNap = false,
                Lock_Display_ResolutionRefreshRate = false,
                Lock_Display_USBCPrioritization = false,
                Lock_Display_ActiveInputSource = false,
                Lock_Webcam_RestoreFactoryDefaults = false,
                Lock_Audio_RestoreFactoryDefaults = false,
                Lock_Keyboard_RestoreFactoryDefaults = false,
                Lock_Mouse_RestoreFactoryDefaults = false,
                Lock_Pen_RestoreFactoryDefaults = false,
                Lock_Keyboard_CollabScreenShare = false,
            };
            var settings_Data = new DDPMSettings(Ddpm_app_, Ddpm_user_, Ddpm_it_);
            var ddpm_it = new DDPMITConfig();
            Mock<ISettingsManagerSA> SysSettingsPlugin = new Mock<ISettingsManagerSA>();
            SysSettingsPlugin.Setup(x => x.GetITGlobalConfigs(It.IsAny<bool>())).Returns(Task.FromResult(Ddpm_it_));
            var SysSettingsPluginObj = SysSettingsPlugin.Object;
            privateSettingsManagerObject.SetFieldOrProperty("_SysSettingsPlugin", SysSettingsPluginObj);

            string accessInfo_ = "testaccessinfo";
            string serialized_string = "{\"key\":\"value\"}";
            string settings_path_target_file = "testReloadAppConfigDatafile.json";  //ReloadAppConfigData
            File.WriteAllText(settings_path_target_file, serialized_string);
            //privateSettingsManagerObject.SetFieldOrProperty("_settingsAccessInfo", accessInfo_);
            privateSettingsManagerObject.SetFieldOrProperty("_settings_path", settings_path_target_file);
            privateSettingsManagerObject.SetFieldOrProperty("_settings", settings_Data); //settings_path_target_file is  not null , _settings not null

            string DisplayImportSettings_path2 = "TestU2724DD.json";
            PrivateObject privatesettingsManagerObj = new PrivateObject(SettingsManagerSAPlugin);
            string monitorSettings_path2 = Environment.CurrentDirectory + "\\" + DisplayImportSettings_path2; //ReloadMonitorSettings
            privatesettingsManagerObj.SetFieldOrProperty("_display_path", Environment.CurrentDirectory);
            string MonitorListjsonData = "[{\"Version\":1.0,\"Model\":\"TestModel\",\"ServiceTag\":\"12345\",\"Input\":{},\"KVM\":{},\"VCPs\":[],\"EA\":{}}]";
            File.WriteAllText(DisplayImportSettings_path2, MonitorListjsonData);
            List<DDPMMonitorSettings> dDPMMonitorSettingsList = new List<DDPMMonitorSettings>();
            Dictionary<string, List<DDPMMonitorSettings>> _allMonitorSettings = new Dictionary<string, List<DDPMMonitorSettings>>();
            DDPMMonitorSettings settings = new DDPMMonitorSettings
            {
                Version = 1.0f,
                Model = "TestU2724DF",
                ServiceTag = "12345",
                Input = new InputSource() { strInputSourceList = "HDMI-1" },
                KVM = new KVMSettings() { strUSBKVMPCsList = "teststrUSBKVMPCsList", isOnUSBKVM = true, isOnNKVM = false },
                VCPs = new List<VCPCode> { new VCPCode() { Code = 10, Value = new List<int>(20) } },
                EA = new EAMonitorSettings(),
            };
            _allMonitorSettings.Add("TestU2724DD", new List<DDPMMonitorSettings> { settings }); //ReloadMonitorSettings
            privatesettingsManagerObj.SetFieldOrProperty("_AllMonitorSettings", _allMonitorSettings);
            string settingsAccessInfo = "Test settings AccessInfo Calculate for Test verify";
            //privateSettingsManagerObject.SetFieldOrProperty("_settingsAccessInfo", settingsAccessInfo);
            var result = SettingsManagerSAPlugin.DisplayExportSettings(modelname, seriveTag, _allMonitorSettings["TestU2724DD"], path).Result;
            Assert.That(result, Is.False);
            File.Delete(settings_path_target_file);
            File.Delete(DisplayImportSettings_path2);
        }

        [Test]
        public void TestQuerySettingsStatus()
        {
            PrivateObject privatesettingsManagerObj = new PrivateObject(SettingsManagerSAPlugin);
            privatesettingsManagerObj.SetFieldOrProperty("_isAllSettingsReady", true);
            var QuerySettingsStatus_result = SettingsManagerSAPlugin.QuerySettingsStatus().Result;  //_isAllSettingsReady true
            Assert.That(QuerySettingsStatus_result, Is.True);

            privatesettingsManagerObj.SetFieldOrProperty("_isAllSettingsReady", false);
            var QuerySettingsStatus_result2 = SettingsManagerSAPlugin.QuerySettingsStatus().Result;  //_isAllSettingsReady false
            Assert.That(QuerySettingsStatus_result2, Is.False);
        }

        [Test]
        public void TestReadDDMImpSettingsFile()
        {
            string path1 = "";
            DDMImpSettings ImpSettings = new DDMImpSettings();
            var ReadDDMImpSettingsFile_result1 = SettingsManagerSAPlugin.ReadDDMImpSettingsFile(path1).Result; // path is not exist
            Assert.IsNotNull(ReadDDMImpSettingsFile_result1);

            PrivateObject privatesettingsManagerObj = new PrivateObject(SettingsManagerSAPlugin);
            string DDMImpSettingsFile_path2 = "TestU2724DD.json";
            string monitorSettings_path2 = Environment.CurrentDirectory + "\\" + DDMImpSettingsFile_path2;
            string MonitorListjsonData = "[{\"Version\":1.0,\"Model\":\"TestModel\",\"ServiceTag\":\"12345\",\"Input\":{},\"KVM\":{},\"VCPs\":[],\"Display\":{}}]";
            File.WriteAllText(DDMImpSettingsFile_path2, MonitorListjsonData);

            var ReadDDMImpSettingsFile_result2 = SettingsManagerSAPlugin.ReadDDMImpSettingsFile(monitorSettings_path2).Result; // path is exist
            Assert.IsNotNull(ReadDDMImpSettingsFile_result2);
            File.Delete(DDMImpSettingsFile_path2);
        }

        [Test]
        public void TestisDDMMigration()
        {
            string folder_appdatapath_migration = "Test folder_appdatapath_migration";
            var isDDMMigration_result1 = SettingsManagerSAPlugin.isDDMMigration(out folder_appdatapath_migration).Result;
            Assert.IsNotNull(isDDMMigration_result1);
        }

        [Test]
        public void TestReadDDMMonitorSettings()
        {
            string DDMImpSettingsFile_path2 = "TestU2724DD.json";
            DDMMonitorSettings DDMmonitorsettings;
            DDMmonitorsettings = new DDMMonitorSettings()
            {
                Version = 1.2,
                OS = "Windows",
                Model = "TestU2724DD",
                ServiceTag = "123456",
                Input = new Input(),
                Display = new DdmLibrary.Utility.Display(),
                ColorPreset = new DdmLibrary.Utility.ColorPreset(),
                EasyArrangement = new EasyArrangement(),
                KVM = new KVM(),
                Personalize = new Personalize(),
                Others = new Others(),
                VCPs = new List<VCP>(),
                DisplayInfo = new DisplayInfo(),
                BriConSchedule = new BriConSchedule(),
            };

            string MonitorListjsonData = "[{\"Version\":1.0,\"Model\":\"TestU2724DD\",\"ServiceTag\":\"123456\",\"Input\":{},\"KVM\":{},\"VCPs\":[],\"Display\":{}}]";
            File.WriteAllText(DDMImpSettingsFile_path2, MonitorListjsonData);
            var ReadDDMMonitorSettings_result1 = SettingsManagerSAPlugin.ReadDDMMonitorSettings(DDMImpSettingsFile_path2, ref DDMmonitorsettings).Result;
            Assert.IsNotNull(ReadDDMMonitorSettings_result1);
            File.Delete(DDMImpSettingsFile_path2);
        }

        [Test]
        public void TestReadDDMUserSettings()
        {
            string ReadDDMUserSettings_path2 = "TestU2724DD.json";
            DdmLibrary.Utility.DDMUserSettings dDMUserSettings = new DDMUserSettings()
            {
                Version = 1.7,
                OS = "Windows",
                SnapEnable = false,
                DisplayMatrixEnable = false,
                ColorSchemes = 0,
                EAWithoutGap = true,
                EAWithShiftKey = false,
                EASpan = false,
                Language = 11,
                AutoStart = true,
                OnScreenNotification = true,
                AutoCheckUpdate = true,
                AllowTelemetry = false,
                TelemetryInit = true,
                ShowTelemetryUI = true,
                SilentShowTelemetry = false,
                ImportPermission = true,
                Hotkeys = new List<Hotkey>(),
                CustLayouts = new List<CustLayout>(),
                OTAPrompt = new Dictionary<string, string>(),
                OTANextCheckTime = null,
                AutoRestoreWindowLayout = false,
                LockRotate = false,
                Profiles = new List<EAProfile>(),
                //SilentShowTelemetry = false,
                ScheduleBriConIsSync = false,
                LastImportedMonitor = null,
                NetworkDataAccess = NetworkDataAccessState.Unknow,
            };
            string MonitorListjsonData = "[{\"Version\":1.7,\"OS\":\"Windows\",\"SnapEnable\":false,\"Hotkey\":{},\"CustLayout\":{}}]";
            File.WriteAllText(ReadDDMUserSettings_path2, MonitorListjsonData);
            var ReadDDMUserSettings_result1 = SettingsManagerSAPlugin.ReadDDMUserSettings(ReadDDMUserSettings_path2, ref dDMUserSettings).Result;
            Assert.IsNotNull(ReadDDMUserSettings_result1);
            File.Delete(ReadDDMUserSettings_path2);
        }

        [Test]
        public void TestReadSerializedContentFromFile()
        {
            string WriteImpExpSet_path2 = "WriteImpExpSet.json";
            string ReadSerializedContentFromFile = null;
            var ReadSerializedContentFromFile_result = SettingsManagerSAPlugin.ReadSerializedContentFromFile(WriteImpExpSet_path2).Result;
            Assert.That(ReadSerializedContentFromFile_result, Is.EqualTo(ReadSerializedContentFromFile));

            string writeglobalSettings_path1_ = "test_WriteGlobalSettingsPath.json";
            string jsonData = "{\"GlobalSetting_General\":{\"Low_Battery_Level\":true,\"Keyboard_Lock_Key\":false,\"Webcam_WB7022_Presence_Detection_Sensor_Cover_State\":true,\"Display_MuteState\":true,\"Display_Color_Preset_and_Easy_Memory\":true},\"GlobalSetting_WidgetSettings\":{\"EnableQuickAccessWidget\":false,\"EnableQuickAccessWidget_Reminder\":false},\"GlobalSetting_About\":{\"SWVersion\":\"\",\"DriverVersion\":\"0000\"}}";
            File.WriteAllText(writeglobalSettings_path1_, jsonData);
            PrivateObject privateSettingsManagerObject = new PrivateObject(SettingsManagerSAPlugin);

            string writeglobalSettings_path2_ = Environment.CurrentDirectory + "\\" + writeglobalSettings_path1_;
            string settingsAccessInfo = "Test settings AccessInfo Calculate for Test verify";
            //privateSettingsManagerObject.SetFieldOrProperty("_settingsAccessInfo", settingsAccessInfo);

            var ReadSerializedContentFromFile_result2 = SettingsManagerSAPlugin.ReadSerializedContentFromFile(writeglobalSettings_path2_).Result;
            Assert.IsNotNull(ReadSerializedContentFromFile_result2);
            File.Delete(writeglobalSettings_path1_);
        }

        [Test]
        public void TestWriteSerializedContentToFile()
        {
            string writeglobalSettings_path1_ = "test_WriteGlobalSettingsPath.json";
            string jsonData = "{\"GlobalSetting_General\":{\"Low_Battery_Level\":true,\"Keyboard_Lock_Key\":false,\"Webcam_WB7022_Presence_Detection_Sensor_Cover_State\":true,\"Display_MuteState\":true,\"Display_Color_Preset_and_Easy_Memory\":true},\"GlobalSetting_WidgetSettings\":{\"EnableQuickAccessWidget\":false,\"EnableQuickAccessWidget_Reminder\":false},\"GlobalSetting_About\":{\"SWVersion\":\"\",\"DriverVersion\":\"0000\"}}";
            File.WriteAllText(writeglobalSettings_path1_, jsonData);
            PrivateObject privateSettingsManagerObject = new PrivateObject(SettingsManagerSAPlugin);

            string writeglobalSettings_path2_ = Environment.CurrentDirectory + "\\" + writeglobalSettings_path1_;
            string settingsAccessInfo = "Test settings AccessInfo Calculate for Test verify";
            //privateSettingsManagerObject.SetFieldOrProperty("_settingsAccessInfo", settingsAccessInfo);

            var WriteSerializedContentToFile_result2 = SettingsManagerSAPlugin.WriteSerializedContentToFile(writeglobalSettings_path2_, jsonData).Result;
            Assert.IsNotNull(WriteSerializedContentToFile_result2);
            Assert.IsTrue(WriteSerializedContentToFile_result2);
            File.Delete(writeglobalSettings_path1_);
        }

        [Test]
        public void TestAddInfo()
        {
            string info = "Test Add info";
            Mock<ISettingsManagerSA> mockSettingsManagerSA = new Mock<ISettingsManagerSA>();
            var SettingsManagerSAObj = mockSettingsManagerSA.Object;
            PrivateObject privateSettingsManagerObject = new PrivateObject(SettingsManagerSAPlugin);
            privateSettingsManagerObject.SetFieldOrProperty("_SysSettingsPlugin", SettingsManagerSAObj);

            var AddInfo_result = SettingsManagerSAPlugin.AddInfo(info);
            Assert.IsNotNull(AddInfo_result);
        }

        [Test]
        public void TestGetInfos()
        {
            List<string> infos = new List<string>();
            bool boolInfo = false;
            Mock<ISettingsManagerSA> mockSettingsManagerSA = new Mock<ISettingsManagerSA>();
            mockSettingsManagerSA.Setup(x => x.GetInfos(It.IsAny<bool>())).Returns(Task.FromResult(infos));
            var SettingsManagerSAObj = mockSettingsManagerSA.Object;
            PrivateObject privateSettingsManagerObject = new PrivateObject(SettingsManagerSAPlugin);
            privateSettingsManagerObject.SetFieldOrProperty("_SysSettingsPlugin", SettingsManagerSAObj);

            if (infos == null || infos.Count == 0)
            {
                var GetInfos_result = SettingsManagerSAPlugin.GetInfos(boolInfo).Result;
                Assert.IsNotNull(GetInfos_result);
            }

            List<string> infos2 = new List<string>() { "Test info1", "Test info2" };
            mockSettingsManagerSA.Setup(x => x.GetInfos(It.IsAny<bool>())).Returns(Task.FromResult(infos2));
            var SettingsManagerSAObj2 = mockSettingsManagerSA.Object;
            privateSettingsManagerObject.SetFieldOrProperty("_SysSettingsPlugin", SettingsManagerSAObj2);

            if (infos2.Count > 0)
            {
                var GetInfos_result2 = SettingsManagerSAPlugin.GetInfos(boolInfo).Result;
                Assert.IsNotNull(GetInfos_result2);
                Assert.That(infos2, Is.EqualTo(GetInfos_result2));
            }
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