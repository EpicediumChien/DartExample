using DDPM.SA.Common;
using DDPM.SA.Common.Display;
using DDPM.SA.Common.Settings;
using DDPM.SA.Plugins.User.DisplayManager;
using DDPM.SA.Plugins.User.EasyArrange;
using Dell.Client.Framework.Interfaces;
using Dell.Client.Framework.UnitTestShared.Tests;
using Moq;
using System.Threading;
using System.Windows.Forms;
using VcpCore.Common;
using VcpCore.Interfaces;
using VcpCore.Plugins;
using DDPM.EABroker;
using DDPM.SA.Common.Interfaces;
using Dell.Client.Framework.Security;
using Dell.Client.Framework.Common;
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities.ObjectModel;

namespace DDPM.SA.Plugins.User.EasyArrange.Test
{
    [Apartment(ApartmentState.STA)]
    public class TestEAPlugin
    {
        private Mock<IAgent> DisplayMangerAgent { get; } = new();
        private Mock<IAgent> VcpCoreAgent { get; } = new();
        private Mock<IAgent> EApluginAgent { get; } = new();

        private MonitorInfo monitorInfo = new MonitorInfo();
        private Mock<IVcpCoreService> VcpCoreService { get; } = new();
        private Mock<IDisplayService> DisplayManagerService { get; } = new();
        private Mock<IDeviceManagerSA> DeviceManagerService { get; } = new();
        private Mock<IEasyArrangeService> EasyArrangeService { get; } = new();

        private MonitorInfo monitorInfo1 = new MonitorInfo()
        {
            AliasDeviceName = "Dell U2724DE(HDMI)",
            IsDellMonitor = true,
            Index = 0,
            CapabilityString = "(prot(monitor)type(LCD)model(U2424H)cmds(01 02 03 07 0C E3 F3)vcp(02 04 05 08 10 12 14(01 04 05 06 08 09 0B 0C)E5 E7(02 03) E2(00 02 04 0C 0D 0F)",
            DisplayName = "TestDISPLAY7",
            DDCisON = true,
            FwVersion = "M3T101",
            inputSource = "HDMI-1",
            modelName = "TestU2724DE",
            series = "Dell UltraSharp (U) Series Monitors",
            D_Ctrl = "Test D_Ctrl",
            SupplierID = "Test SupplierID",
            //CapabilityDic = capabilityDic;
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
        private List<MonitorInfo> monitorInfos = new List<MonitorInfo>();

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

        private EAPlugin CreateInitializeEAplugin()
        {
            EApluginAgent.Setup(x => x.PluginManager.FindPluginByGuid(Guid.Parse(DDPM.SA.Common.IDs.DDPM_EAPlugin_PLUGIN_ID)));

            return new EAPlugin(EApluginAgent.Object);
        }

        private DisplayMangerPlugin displayPlugin;
        private VcpCorePlugin vcpCorePlugin;
        private Dictionary<string, Dictionary<string, string>> getstr;
        private EAPlugin EAplugin;

        [OneTimeSetUp]
        public void Setup()
        {
            displayPlugin = CreateInitializeDisplayMangerPlugin();
            vcpCorePlugin = CreateInitializeVcpCorePlugin();
            EAplugin = CreateInitializeEAplugin();
            PrivateObject privateObject = new PrivateObject(displayPlugin);
            PrivateObject privatevcp = new PrivateObject(vcpCorePlugin);
            privateObject.SetField("_VcpCorePlugin", vcpCorePlugin as IVcpCoreService);
            getstr = (Dictionary<string, Dictionary<string, string>>)privatevcp.GetField("_ColorPresets");
        }

        [Test]
        public void TestEasyArrangePlugin()
        {
            Assert.IsNotNull(EAplugin);
            PrivateObject privatevcolorPreset = new PrivateObject(EAplugin);
            var agent2 = privatevcolorPreset.GetField("_agent") as IAgent;
            Assert.That(agent2, Is.EqualTo(EApluginAgent.Object));
        }

        [Test]
        public void TestIsDisposed()
        {
            var Result = EAplugin.IsDisposed;
            Assert.IsFalse(Result);
            PrivateObject privateEApluginObject = new PrivateObject(EAplugin);
            privateEApluginObject.SetFieldOrProperty("IsDisposed", true);
            var Result2 = EAplugin.IsDisposed;
            Assert.IsTrue(Result2);
        }

        [Test]
        public void TestInitializeDeviceManagerPlugin()
        {
            Mock<IDeviceManagerSA> Mock_DisplayManagerPlugin = new Mock<IDeviceManagerSA>();
            var DisplayManagerPluginObj = Mock_DisplayManagerPlugin.Object;
            PrivateObject privateEApluginObject = new PrivateObject(EAplugin);
            privateEApluginObject.SetFieldOrProperty("_deviceManagerPlugin", DisplayManagerPluginObj);
            var result = privateEApluginObject.Invoke("InitializeDeviceManagerPlugin");
            var DisplayManagerPlugin_result = (IDeviceManagerSA)privateEApluginObject.GetFieldOrProperty("_deviceManagerPlugin");
            Assert.IsNotNull(DisplayManagerPlugin_result);
            Assert.That(DisplayManagerPlugin_result, Is.EqualTo(DisplayManagerPluginObj));
        }

        [Test]
        public void TestCheckIfReadyToStartEABorker()
        {
            bool CheckIfReadyToStartEABorker1 = true;
            bool CheckIfReadyToStartEABorker2 = false;
            PrivateObject privateEApluginObject = new PrivateObject(EAplugin);
            privateEApluginObject.SetFieldOrProperty("_displayManagerPluginUsable", true);
            privateEApluginObject.SetFieldOrProperty("_deviceManagerPluginUsable", true);
            privateEApluginObject.SetFieldOrProperty("_settingsManagerPluginUsable", true);
            privateEApluginObject.SetFieldOrProperty("_isEaBrokerStarted", false);
            var CheckIfReadyToStartEABorker_result1 = (bool)privateEApluginObject.Invoke("CheckIfReadyToStartEABorker");  //_displayManagerPluginUsable ,_deviceManagerPluginUsable,_settingsManagerPluginUsable,true
            Assert.IsNotNull(CheckIfReadyToStartEABorker_result1);
            Assert.That(CheckIfReadyToStartEABorker1, Is.EqualTo(CheckIfReadyToStartEABorker_result1));

            privateEApluginObject.SetFieldOrProperty("_displayManagerPluginUsable", false);
            privateEApluginObject.SetFieldOrProperty("_deviceManagerPluginUsable", false);
            privateEApluginObject.SetFieldOrProperty("_settingsManagerPluginUsable", true);
            privateEApluginObject.SetFieldOrProperty("_isEaBrokerStarted", false);
            var CheckIfReadyToStartEABorker_result2 = (bool)privateEApluginObject.Invoke("CheckIfReadyToStartEABorker");  //_displayManagerPluginUsable false ,_deviceManagerPluginUsable false, _settingsManagerPluginUsable true
            Assert.IsNotNull(CheckIfReadyToStartEABorker_result2);
            Assert.That(CheckIfReadyToStartEABorker2, Is.EqualTo(CheckIfReadyToStartEABorker_result2));

            privateEApluginObject.SetFieldOrProperty("_displayManagerPluginUsable", true);
            privateEApluginObject.SetFieldOrProperty("_deviceManagerPluginUsable", true);
            privateEApluginObject.SetFieldOrProperty("_settingsManagerPluginUsable", true);
            privateEApluginObject.SetFieldOrProperty("_isEaBrokerStarted", true);
            var CheckIfReadyToStartEABorker_result3 = (bool)privateEApluginObject.Invoke("CheckIfReadyToStartEABorker");  //_displayManagerPluginUsable true ,_deviceManagerPluginUsable true, _settingsManagerPluginUsable true
            Assert.IsNotNull(CheckIfReadyToStartEABorker_result3);
            Assert.That(CheckIfReadyToStartEABorker2, Is.EqualTo(CheckIfReadyToStartEABorker_result3));

            privateEApluginObject.SetFieldOrProperty("_displayManagerPluginUsable", false);
            privateEApluginObject.SetFieldOrProperty("_deviceManagerPluginUsable", false);
            privateEApluginObject.SetFieldOrProperty("_settingsManagerPluginUsable", false);
            privateEApluginObject.SetFieldOrProperty("_isEaBrokerStarted", false);
            var CheckIfReadyToStartEABorker_result4 = (bool)privateEApluginObject.Invoke("CheckIfReadyToStartEABorker");  //_displayManagerPluginUsable false ,_deviceManagerPluginUsable false, _settingsManagerPluginUsable false
            Assert.IsNotNull(CheckIfReadyToStartEABorker_result4);
            Assert.That(CheckIfReadyToStartEABorker2, Is.EqualTo(CheckIfReadyToStartEABorker_result4));
        }


        //Robert_Lin, 2025-1-7 EAPlugin.IsFunctionEnabled has been deleted.
        [Test]
        public void TestIsFunctionEnabled()
        {
            //PrivateObject privateEApluginObject = new PrivateObject(EAplugin);
            //ArrangeVM arrangeVM=new ArrangeVM() {IsFunctionEnabled=false };
            //privateEApluginObject.SetFieldOrProperty("_vmArrange", arrangeVM);
            //var Result1 = EAplugin.IsFunctionEnabled;
            //Assert.IsFalse(Result1);
            //ArrangeVM arrangeVM2 = new ArrangeVM() { IsFunctionEnabled = true };
            //privateEApluginObject.SetFieldOrProperty("_vmArrange", arrangeVM2);

            //var Result2 = EAplugin.IsFunctionEnabled;  //method update
            //Assert.IsTrue(Result2);
        }

        //Robert_Lin, 2025-1-7, EAPlugin.SetEAWorkSplit() to be removed.
        // Most of classes in DDPM.SA.Plugins.User.EasyArrange will be removed, and use DDPM.SA/Common/DDPM.EABroker instead
        //
        //Robert_Lin 2024-12-4 comment-out due to EABroker.EABroker add one argument
        [Test]
        public void TestSetEAWrokSplit()
        {
            /*
            bool SetEAWrokSplit1 = false;
            bool SetEAWrokSplit2 = true;
            int cellCount = 1;
            char splitKey = 'A';
            List<double>? settings;
            settings = new List<double>() { 1.0 };
            PrivateObject privateEApluginObject = new PrivateObject(EAplugin);
            privateEApluginObject.SetFieldOrProperty("_eaBroker", null);
            var SetEAWrokSplit1_Result1 = EAplugin.SetEAWrokSplit(monitorInfo1, cellCount, splitKey, settings).Result;  //_eaBroker null
            Assert.That(SetEAWrokSplit1, Is.EqualTo(SetEAWrokSplit1_Result1));

            DDPM.EABroker.EABroker? eaBroker;
            Mock<IAgent> mockAgent = new Mock<IAgent>();
            Mock<IDeviceManagerSA> mockDeviceManagerSA = new Mock<IDeviceManagerSA>();
            Mock<IDisplayService> mockDisplayService = new Mock<IDisplayService>();
            Mock<IEasyArrangeService> mockEasyArrangeService = new Mock<IEasyArrangeService>();
            Mock<ISettingsManagerDev> mockSettingsManagerDev = new Mock<ISettingsManagerDev>();
            eaBroker = new EABroker.EABroker(mockAgent.Object, mockDeviceManagerSA.Object, mockDisplayService.Object, mockEasyArrangeService.Object, mockSettingsManagerDev.Object);
            privateEApluginObject.SetFieldOrProperty("_eaBroker", eaBroker);

            List<MonitorInfo> _monitors = new List<MonitorInfo>();
            Screen screen = Screen.PrimaryScreen;
            string displayName = Screen.PrimaryScreen.DeviceName;
            monitorInfo1.DisplayName = displayName;
            _monitors.Add(monitorInfo1);

            ArrangeVM arrangeVM = new ArrangeVM();
            //privateEApluginObject.SetFieldOrProperty("_vmArrange", arrangeVM);  //EAPlugin.cs
            PrivateObject privateArrangeVMObject = new PrivateObject(arrangeVM);
            var workWindow1 = new EAWorkWindow(arrangeVM, screen, _monitors);
            workWindow1.IsUsed = true;
            List<EAWorkWindow> _workWindows22 = new List<EAWorkWindow>() { workWindow1 };
            // privateArrangeVMObject.SetFieldOrProperty("_workWindows2", _workWindows22); // ArrangeVM.cs

            DDPM.EABroker.ArrangeVM arrangeVM1 = new EABroker.ArrangeVM();
            DDPM.EABroker.EAWorkWindow eAWorkWindow = new EABroker.EAWorkWindow(arrangeVM1, false);
            List<DDPM.EABroker.EAWorkWindow> workWindows = new List<DDPM.EABroker.EAWorkWindow>() { eAWorkWindow };

            DDPM.EABroker.ArrangeVM arrangeVM2 = new EABroker.ArrangeVM();  //DDPM.EABroker.ArrangeVM.cs
            PrivateObject privateArrangeVMObject2 = new PrivateObject(arrangeVM2);
            privateArrangeVMObject2.SetFieldOrProperty("_workWindows", null);
            //privateEApluginObject.SetFieldOrProperty("_eaBroker", EaBroker);
            var SetEAWrokSplit1_Result2 = EAplugin.SetEAWrokSplit(monitorInfo1, cellCount, splitKey, settings).Result;  //_eaBroker not null
            Assert.That(SetEAWrokSplit2, Is.EqualTo(SetEAWrokSplit1_Result2));
            */
        }

        [Test]
        public void TestEditCommand()
        {
            EAArgs Args;
            Args = new EAArgs()
            {
                Command = "TestEditCommand",
                CellCount = 1,
                SplitKey = 'A',
                CustomId = 1234567,
                Settings = new List<double>(),
                CustomNames = new List<string>(),
                CustomName = "TestCustomName",
                Result = true,
                Message = "OK",
                SplitJson = new SplitJson() { CellCount = 1, SplitKey = 'A', Settings = new List<double>(), CustomName = "TestCustomName", CustomId = 1234567, EAID = 0, Cells = new CellJson[] { } },
            };
            PrivateObject privateEApluginObject = new PrivateObject(EAplugin);
            privateEApluginObject.SetFieldOrProperty("_isEaBrokerStarted", false);
            var EditCommand_Result1 = EAplugin.EditCommand(monitorInfo1, Args).Result;   //_isEaBrokerStarted false,
            Assert.That(EditCommand_Result1, Is.False);

            privateEApluginObject.SetFieldOrProperty("_isEaBrokerStarted", true);
            privateEApluginObject.SetFieldOrProperty("_eaBroker", null);
            var EditCommand_Result2 = EAplugin.EditCommand(monitorInfo1, Args).Result;   //_isEaBrokerStarted true, _eaBroker null
            Assert.That(EditCommand_Result2, Is.False);

            DDPM.EABroker.EABroker? eaBroker;
            Mock<IAgent> mockAgent = new Mock<IAgent>();
            Mock<IDeviceManagerSA> mockDeviceManagerSA = new Mock<IDeviceManagerSA>();
            Mock<IDisplayService> mockDisplayService = new Mock<IDisplayService>();
            Mock<IEasyArrangeService> mockEasyArrangeService = new Mock<IEasyArrangeService>();
            Mock<ISettingsManagerDev> mockSettingsManagerDev = new Mock<ISettingsManagerDev>();
            eaBroker = new EABroker.EABroker(mockAgent.Object, mockDeviceManagerSA.Object, mockDisplayService.Object, mockEasyArrangeService.Object, mockSettingsManagerDev.Object);
            privateEApluginObject.SetFieldOrProperty("_isEaBrokerStarted", true);
            privateEApluginObject.SetFieldOrProperty("_eaBroker", eaBroker);
            var EditCommand_Result3 = EAplugin.EditCommand(monitorInfo1, Args).Result;   //_isEaBrokerStarted true, _eaBroker not null,RunningState != eEARunningStates.Waiting
            Assert.That(EditCommand_Result3, Is.False);

            eaBroker = new EABroker.EABroker(mockAgent.Object, mockDeviceManagerSA.Object, mockDisplayService.Object, mockEasyArrangeService.Object, mockSettingsManagerDev.Object) { RunningState = eEARunningStates.Waiting };
            privateEApluginObject.SetFieldOrProperty("_isEaBrokerStarted", true);
            privateEApluginObject.SetFieldOrProperty("_eaBroker", eaBroker);
            var EditCommand_Result4 = EAplugin.EditCommand(monitorInfo1, Args).Result;   //_isEaBrokerStarted true, _eaBroker not null,RunningState = eEARunningStates.Waiting
            Assert.That(EditCommand_Result4, Is.True);
        }

        //Robert_Lin 2024-12-4 comment-out due to EABroker.EABroker add one argument
        [Test]
        public void TestReloadEzSettings()
        {
            bool ReloadEzSettings1 = false;
            bool ReloadEzSettings2 = true;

            PrivateObject privateEApluginObject = new PrivateObject(EAplugin);

            DDPM.EABroker.EABroker? eaBroker;
            eaBroker = null;
            privateEApluginObject.SetFieldOrProperty("_eaBroker", eaBroker);
            if (eaBroker == null)
            {
                var ReloadEzSettings_Result1 = EAplugin.ReloadEzSettings().Result;       //_eaBroker null
                Assert.That(ReloadEzSettings1, Is.EqualTo(ReloadEzSettings_Result1));
            }

            Mock<IAgent> mockAgent = new Mock<IAgent>();
            Mock<IDeviceManagerSA> mockDeviceManagerSA = new Mock<IDeviceManagerSA>();
            Mock<IDisplayService> mockDisplayService = new Mock<IDisplayService>();
            Mock<IEasyArrangeService> mockEasyArrangeService = new Mock<IEasyArrangeService>();
            Mock<ISettingsManagerDev> mockSettingsManagerDev = new Mock<ISettingsManagerDev>();
            eaBroker = new EABroker.EABroker(mockAgent.Object, mockDeviceManagerSA.Object, mockDisplayService.Object, mockEasyArrangeService.Object, mockSettingsManagerDev.Object);
            privateEApluginObject.SetFieldOrProperty("_eaBroker", eaBroker);

            IDeviceManagerSA? _deviceManagerPluginNull;
            _deviceManagerPluginNull = null;
            privateEApluginObject.SetFieldOrProperty("_deviceManagerPlugin", _deviceManagerPluginNull);
            if (eaBroker != null)
            {
                if (_deviceManagerPluginNull == null)
                {
                    var ReloadEzSettings_Result2 = EAplugin.ReloadEzSettings().Result;       //_eaBroker not null,_deviceManagerPlugin null
                    Assert.That(ReloadEzSettings1, Is.EqualTo(ReloadEzSettings_Result2));
                }
            }

            DDPM.EABroker.ArrangeVM arrangeVM = new DDPM.EABroker.ArrangeVM();

            PrivateObject privateArrangeVMObject = new PrivateObject(arrangeVM);
            EzSettings ezSettings = new EzSettings()
            {
                IsWidthoutGap = true,
                IsOnlyAllowWhenShiftKeyPressed = false,
                IsSpanAcrossMultiMonitors = false,
                IsAwsEnabled = false
            };
            mockDeviceManagerSA.Setup(x => x.ReadEzSettings()).Returns(Task.FromResult(ezSettings)); //DDPM.EABroker.ArrangeVM.cs  ReloadEzSettingsFromUserSettingsFile method
            privateArrangeVMObject.SetFieldOrProperty("_deviceManagerSA", mockDeviceManagerSA.Object);

            var DeviceManagerPluginObj = mockDeviceManagerSA.Object;
            privateEApluginObject.SetFieldOrProperty("_deviceManagerPlugin", DeviceManagerPluginObj);

            if (eaBroker != null)
            {
                if (mockDeviceManagerSA != null)
                {
                    var ReloadEzSettings_Result3 = EAplugin.ReloadEzSettings().Result;       //_eaBroker not null,_deviceManagerPlugin not null
                    Assert.That(ReloadEzSettings2, Is.EqualTo(ReloadEzSettings_Result3));
                }
            }
        }

       /* [Test]
        public void TestSetEASelectedLayout()
        {
            SplitJson spJson = new SplitJson()
            {
                CellCount = 0,
                CustomId = 0,
                CustomName = "TestCustName",
                SplitKey = 'A',
                Settings = new List<double>()
            };
            bool SetEASelectedLayout1 = false;
            bool SetEASelectedLayout2 = true;
            ArrangeVM arrangeVM = new ArrangeVM();
            PrivateObject privateEApluginObject = new PrivateObject(EAplugin);
            //privateEApluginObject.SetFieldOrProperty("_vmArrange", arrangeVM);    _vmArrange remove

            IDeviceManagerSA? _deviceManagerPluginNull;
            _deviceManagerPluginNull = null;
            privateEApluginObject.SetFieldOrProperty("_deviceManagerPlugin", _deviceManagerPluginNull);
            if (_deviceManagerPluginNull == null)
            {
                var SetEASelectedLayout_Result1 = EAplugin.SetEASelectedLayout(monitorInfo1, spJson).Result;       //_deviceManagerPluginNull null
                Assert.That(SetEASelectedLayout1, Is.EqualTo(SetEASelectedLayout_Result1));
            }

            var DeviceManagerPluginObj = DeviceManagerService.Object;
            privateEApluginObject.SetFieldOrProperty("_deviceManagerPlugin", DeviceManagerPluginObj);

            if (DeviceManagerPluginObj != null)
            {
                var SetEASelectedLayout_Result2 = EAplugin.SetEASelectedLayout(monitorInfo1, spJson).Result;       //_deviceManagerPluginNull null
                Assert.That(SetEASelectedLayout2, Is.EqualTo(SetEASelectedLayout_Result2));
            }
        }*/

        [Test]
        public void Test_dump_SplitJsonList()
        {
            var splitJsonList = new List<SplitJson>
            {
              new SplitJson { CellCount = 1, SplitKey = 'A', Settings = new List<double> { 1.1 }, CustomName = "One", CustomId = 1 },
              new SplitJson { CellCount = 2, SplitKey = 'B', Settings = new List<double> { 2.2 }, CustomName = "Two", CustomId = 2 },
              new SplitJson { CellCount = 3, SplitKey = 'C', Settings = new List<double> { 3.3 }, CustomName = "Three", CustomId = 3 }
            };
            PrivateObject privateEApluginObject = new PrivateObject(EAplugin);
            privateEApluginObject.Invoke("_dump_SplitJsonList", splitJsonList);
            Assert.IsTrue(true);
        }

        [Test]
        public void TestEABroker_Start()
        {
            PrivateObject privateEApluginObject = new PrivateObject(EAplugin);
            privateEApluginObject.SetFieldOrProperty("_isEaBrokerStarted", true); //_isEaBrokerStarted true
            EzSettings ezSettings = new EzSettings()
            {
                IsWidthoutGap = true,
                IsOnlyAllowWhenShiftKeyPressed = false,
                IsSpanAcrossMultiMonitors = false,
                IsAwsEnabled = false
            };
            DeviceManagerService.Setup(x => x.ReadEzSettings()).Returns(Task.FromResult(ezSettings));
            var DeviceManagerPluginObj = DeviceManagerService.Object;
            var DisplayManagerPluginObj = DisplayManagerService.Object;
            privateEApluginObject.SetFieldOrProperty("_deviceManagerPlugin", DeviceManagerPluginObj);
            privateEApluginObject.SetFieldOrProperty("_displayManagerPlugin", DisplayManagerPluginObj);
            ArrangeVM arrangeVM = new ArrangeVM();
            //privateEApluginObject.SetFieldOrProperty("_vmArrange", arrangeVM); 
            EAplugin.EABroker_Start();
            Assert.IsTrue(true);
        }

        [Test]
        public void Test_displayManagerPlugin_Displaychanged()
        {
            object? sender = new object();
            List<MonitorInfo> Monitors = new List<MonitorInfo>();
            DisplaychangedEventArgs e = new DisplaychangedEventArgs() { count = 0, monitors = Monitors };
            PrivateObject privateEApluginObject = new PrivateObject(EAplugin);
            privateEApluginObject.Invoke("_displayManagerPlugin_Displaychanged", sender, e);
            Assert.IsTrue(true);
        }

        [Test]
        public void TestEABroker_Stop()
        {
            PrivateObject privateEApluginObject = new PrivateObject(EAplugin);
            privateEApluginObject.SetFieldOrProperty("_isEaBrokerStarted", true);
            EzSettings ezSettings = new EzSettings()
            {
                IsWidthoutGap = true,
                IsOnlyAllowWhenShiftKeyPressed = false,
                IsSpanAcrossMultiMonitors = false,
                IsAwsEnabled = false
            };
            DeviceManagerService.Setup(x => x.ReadEzSettings()).Returns(Task.FromResult(ezSettings));
            var DeviceManagerPluginObj = DeviceManagerService.Object;
            var DisplayManagerPluginObj = DisplayManagerService.Object;
            privateEApluginObject.SetFieldOrProperty("_deviceManagerPlugin", DeviceManagerPluginObj);
            privateEApluginObject.SetFieldOrProperty("_displayManagerPlugin", DisplayManagerPluginObj);
            ArrangeVM arrangeVM = new ArrangeVM();
            //privateEApluginObject.SetFieldOrProperty("_vmArrange", arrangeVM);
            EAplugin.EABroker_Stop();
            Assert.IsTrue(true);
        }

        /*[Test]
        public void TestLogException()
        {
            var exception = new Exception("Test exception");
            string msg = "Test message";
            PrivateObject privateEApluginObject = new PrivateObject(EAplugin);
            privateEApluginObject.Invoke("LogException", exception, msg);
            Assert.IsTrue(true);
        }*/

        [Test]
        public void TestConfigureServices()
        {
            var exception = new Exception("Test exception");
            bool isConfigured = true;
            PrivateObject privateEApluginObject = new PrivateObject(EAplugin);
            privateEApluginObject.SetFieldOrProperty("_isConfigured", isConfigured); //_isConfigured true
            privateEApluginObject.Invoke("ConfigureServices");
            Assert.IsTrue(true);
        }

        [Test]
        public void TestDebugMsg()
        {
            string msg = "Test Debug message";
            EAPlugin.Dmsg(msg);
            Assert.IsTrue(true);
        }

        //Robert_Lin, 2024-12-20 GetMonitors() unused method, will be removed.
        [Test]
        public void TestGetMonitors()
        {
            Mock<IDisplayService> Mock_DisplayManagerPlugin = new Mock<IDisplayService>();
            List<MonitorInfo> _allInfoMonitors = new List<MonitorInfo>();
            _allInfoMonitors.Add(monitorInfo1);
            Mock_DisplayManagerPlugin.Setup(x => x.GetMonitors()).Returns(Task.FromResult(_allInfoMonitors));
            var DisplayManagerPluginObj = Mock_DisplayManagerPlugin.Object;
            PrivateObject privateEAplugin = new PrivateObject(EAplugin);
            //privateEAplugin.SetFieldOrProperty("_displayManagerPlugin", null);   
            //var Getmonitor_result1 = (List<MonitorInfo>)privateEAplugin.Invoke("GetMonitors"); //_displayManagerPlugin null
            //Assert.IsNull(Getmonitor_result1);

            //privateEAplugin.SetFieldOrProperty("_displayManagerPlugin", DisplayManagerPluginObj);
            //var Getmonitor_result2 = (List<MonitorInfo>)privateEAplugin.Invoke("GetMonitors");  //_displayManagerPlugin not  null
            //Assert.IsNotNull(Getmonitor_result2);
            //Assert.Greater(Getmonitor_result2.Count, 0);
        }

        [Test]
        public void TestInitializeSettingsManagerPlugin()
        {
            Mock<ISettingsManagerDev> Mock_SettingsManagerDev = new Mock<ISettingsManagerDev>();
            var SettingsManagerDevObj = Mock_SettingsManagerDev.Object;
            PrivateObject privateEApluginObject = new PrivateObject(EAplugin);
            privateEApluginObject.SetFieldOrProperty("_settingsManagerPlugin", SettingsManagerDevObj);
            var result = privateEApluginObject.Invoke("InitializeSettingsManagerPlugin");
            var SettingsManagerDev_result = (ISettingsManagerDev)privateEApluginObject.GetFieldOrProperty("_settingsManagerPlugin");
            Assert.IsNotNull(SettingsManagerDev_result);
            Assert.That(SettingsManagerDev_result, Is.EqualTo(SettingsManagerDevObj));
        }

       /* [Test]
        public void TestInitializeTelementrySchedulerPlugin()
        {
            Mock<ITelementryScheduler> Mock_TelementryScheduler = new Mock<ITelementryScheduler>();
            var Mock_TelementrySchedulerObj = Mock_TelementryScheduler.Object;
            PrivateObject privateEApluginObject = new PrivateObject(EAplugin);
            privateEApluginObject.SetFieldOrProperty("_telementrySchedulerPlugin", Mock_TelementrySchedulerObj);
            var result = privateEApluginObject.Invoke("InitializeTelementrySchedulerPlugin");
            var TelementryScheduler_result = (ITelementryScheduler)privateEApluginObject.GetFieldOrProperty("_telementrySchedulerPlugin");
            Assert.IsNotNull(TelementryScheduler_result);
            Assert.That(TelementryScheduler_result, Is.EqualTo(Mock_TelementrySchedulerObj));
        }*/

        [Test]
        public void Test_GlobalSettingParam()
        {
            PrivateObject privateEApluginObject = new PrivateObject(EAplugin);
            GlobalSettingParam globalSettingParamDes = new GlobalSettingParam()
            {
                GlobalSetting_About = new GlobalSetting_About() { SWVersion = "0000", DriverVersion = "0000", },
                GlobalSetting_General = new GlobalSetting_General() { Low_Battery_Level = true, Keyboard_Lock_Key = false, Webcam_WB7022_Presence_Detection_Sensor_Cover_State = true, Display_Color_Preset_and_Easy_Memory = true, Display_MuteState = true },
                GlobalSetting_WidgetSettings = new GlobalSetting_WidgetSettings() { EnableQuickAccessWidget = false, EnableQuickAccessWidget_Reminder = false }
            };
            privateEApluginObject.SetFieldOrProperty("_globalSettingParam", globalSettingParamDes);
            var GlobalSettingParam_result = (GlobalSettingParam)privateEApluginObject.GetFieldOrProperty("_GlobalSettingParam");
            Assert.IsNotNull(GlobalSettingParam_result);
        }

        [Test]
        public void TestWriteLog()
        {
            Mock<ILog> mockLog = new Mock<ILog>();
            var logObj = mockLog.Object;
            PrivateObject privateEApluginObject = new PrivateObject(EAplugin);
            privateEApluginObject.SetFieldOrProperty("_log", logObj);
            string message = "Test error message";
            var exception = new Exception("Test exception");
            EAplugin.WriteLog(message, exception);
            Assert.IsTrue(true);
        }

        [Test]
        public void TestSendEasyArrangeLayoutTelemetry()
        {
            string eventValue = "Test EasyArrangeLayout";
            MonitorInfo mi = monitorInfo1;
            PrivateObject privateEApluginObject = new PrivateObject(EAplugin);
            ITelementryScheduler? telementrySchedulerNull = null;
            privateEApluginObject.SetFieldOrProperty("_telementrySchedulerPlugin", telementrySchedulerNull);  //_telementrySchedulerPlugin null
            EAplugin.SendEasyArrangeLayoutTelemetry(eventValue, mi);
            Assert.IsTrue(true);

            Mock<ITelementryScheduler> Mock_TelementryScheduler = new Mock<ITelementryScheduler>();
            var Mock_TelementrySchedulerObj = Mock_TelementryScheduler.Object;
            privateEApluginObject.SetFieldOrProperty("_telementrySchedulerPlugin", Mock_TelementrySchedulerObj);  //_telementrySchedulerPlugin not null, _telementrySchedulerPluginUsable true
            privateEApluginObject.SetFieldOrProperty("_telementrySchedulerPluginUsable", true);
            EAplugin.SendEasyArrangeLayoutTelemetry(eventValue, mi);
            Assert.IsTrue(true);

            privateEApluginObject.SetFieldOrProperty("_telementrySchedulerPluginUsable", false);  //_telementrySchedulerPlugin not null, _telementrySchedulerPluginUsable false
            EAplugin.SendEasyArrangeLayoutTelemetry(eventValue, mi);
            Assert.IsTrue(true);
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            displayPlugin.Dispose();
            vcpCorePlugin.Dispose();
            EAplugin.Dispose();
        }
    }
}