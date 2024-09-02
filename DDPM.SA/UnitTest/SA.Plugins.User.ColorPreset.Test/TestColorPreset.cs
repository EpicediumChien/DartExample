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
using WinCopies;
using Windows.Media.AppBroadcasting;
using Windows.UI.ViewManagement;
using static VcpCore.Common.User32;
using DDPM.SA.Plugins.User.DisplayProperties;
using Dell.Client.Framework.UnitTestShared.Tests;
using  ColorPreset.Plugins;
using System.Linq;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ToolTip;
using WinCopies.Util.Commands.Primitives;
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
            DisplayMangerAgent.Setup(x => x.PluginManager.FindPluginByGuid(Guid.Parse(VcpCore.Common.IDs.Display_Manager_PLUGIN_ID)));

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
        private List<ColorPresetSettings> monitorConfigs1 = new List<ColorPresetSettings>() { new ColorPresetSettings() { DeviceInfo = new EDID() { ModelName = "DELLU2724DE", SerialNumber = "808597589", }, PresetForManual = "Standard" } };

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
                Assert.That(serialNumber, Is.EqualTo(Curpreset_Result.DeviceInfo.SerialNumber));
                Assert.That(modelName, Is.EqualTo(Curpreset_Result.DeviceInfo.ModelName));
                Assert.That(presetForManual, Is.EqualTo(Curpreset_Result.PresetForManual));
            }
            else
            {
                var resullt = colorPresetPlugin.get_cur_monitor_preset_config(monitorInfo1, monitorConfigs1);
                Assert.IsNotNull(resullt);
                Assert.That(serialNumber, Is.EqualTo(resullt.DeviceInfo.SerialNumber));
                Assert.That(modelName, Is.EqualTo(resullt.DeviceInfo.ModelName));
            }
        }

    }
}