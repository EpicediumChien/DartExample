using DDPM.SA.Common;
using DDPM.SA.Plugins.User.DisplayManager;
using DDPM.SA.Plugins.User.EasyArrange;
using DDPM.SA.Plugins.User.SchedulerManager;
using Dell.Client.Framework.Interfaces;
using Dell.Client.Framework.UnitTestShared.Tests;
using Moq;
using MS.WindowsAPICodePack.Internal;
using VcpCore.Common;
using VcpCore.Interfaces;
using VcpCore.Plugins;
using IDs = DDPM.SA.Common.IDs;

namespace DDPM.SA.Plugins.User.SchedulerManager.Test
{
    public class TestSchedulerMangerPlugin
    {
        private Mock<IAgent> DisplayMangerAgent { get; } = new();
        private Mock<IAgent> VcpCoreAgent { get; } = new();
        private Mock<IAgent> SchedulerMangerAgent { get; } = new();

        MonitorInfo monitorInfo = new MonitorInfo();
        private Mock<IVcpCoreService> VcpCoreService { get; } = new();
        private Mock<IDisplayProperties> DisplayPropertiesService { get; } = new();
        private Mock<IDeviceManagerSA> DeviceManagerSAService { get; } = new();
        private IDeviceManagerSA? _deviceManagerPlugin;

        MonitorInfo monitorInfo1 = new MonitorInfo()
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

        private DisplayMangerPlugin CreateInitializeDisplayMangerPlugin()
        {
            DisplayMangerAgent.Setup(x => x.PluginManager.FindPluginByGuid(Guid.Parse(IDs.Display_Manager_PLUGIN_ID)));

            return new DisplayMangerPlugin(DisplayMangerAgent.Object);
        }

        private VcpCorePlugin CreateInitializeVcpCorePlugin()
        {
            VcpCoreAgent.Setup(x => x.PluginManager.FindPluginByGuid(Guid.Parse(VcpCore.Common.IDs.VCP_CORE_PLUGIN_ID)));

            return new VcpCorePlugin(VcpCoreAgent.Object);
        }

        private SchedulerMangerPlugin CreateSchedulerMangerPlugin()
        {
            SchedulerMangerAgent.Setup(x => x.PluginManager.FindPluginByGuid(Guid.Parse(IDs.Scheduler_Manager_Plugin_ID)));

            return new SchedulerMangerPlugin(SchedulerMangerAgent.Object);
        }

        private DisplayMangerPlugin displayPlugin;
        private VcpCorePlugin vcpCorePlugin;
        private SchedulerMangerPlugin schedulerMangerPlugin;
        private PrivateObject privateSchedulerManger;
        [OneTimeSetUp]
        public void Setup()
        {
            displayPlugin = CreateInitializeDisplayMangerPlugin();
            vcpCorePlugin = CreateInitializeVcpCorePlugin();
            schedulerMangerPlugin = CreateSchedulerMangerPlugin();

            PrivateObject privateObject = new PrivateObject(displayPlugin);
            PrivateObject privatevcp = new PrivateObject(vcpCorePlugin);
            privateSchedulerManger = new PrivateObject(schedulerMangerPlugin);
            privateObject.SetField("_VcpCorePlugin", vcpCorePlugin as IVcpCoreService);
        }

        [Test]
        public void TestSchedulerMangerPlugins()
        {
            Assert.IsNotNull(schedulerMangerPlugin);
            PrivateObject privateSchedulerManger = new PrivateObject(schedulerMangerPlugin);
            var agent2 = privateSchedulerManger.GetField("_agent") as IAgent;
            Assert.That(agent2, Is.EqualTo(SchedulerMangerAgent.Object));
        }

        [Test]
        public void TestIsDisposed()
        {
            var Result = schedulerMangerPlugin.IsDisposed;
            Assert.IsFalse(Result);
            PrivateObject privateschedulerMangerObject = new PrivateObject(schedulerMangerPlugin);
            privateschedulerMangerObject.SetFieldOrProperty("IsDisposed", true);
            var Result2 = schedulerMangerPlugin.IsDisposed;
            Assert.IsTrue(Result2);
        }

        [Test]
        public void TestStopSchedulerManger()
        {
            var StopSchedulerManger_Result = schedulerMangerPlugin.StopSchedulerManger();  //_SchedulerCheckTimer.Enabled true
            Assert.IsTrue(StopSchedulerManger_Result.IsCompletedSuccessfully);
        }

        [Test]
        public void TestStartSchedulerManger()
        {
            int millisecond = 2000;
            var StartSchedulerManger_Result = schedulerMangerPlugin.StartSchedulerManger(millisecond);  //_SchedulerCheckTimer.Enabled true
            Assert.IsTrue(StartSchedulerManger_Result.IsCompletedSuccessfully);
        }

        [Test]
        public void TestReceiveScheduleInfo()
        {
            var scheduleInfoToSend = new scheduleInfo()
            {
                IsEnable = true,
                model = "TestModel",
                serviceTag = "TestTag",
                Pre1Name = "Period1",
                Pre2Name = "Period2",
                Hours1 = 2,
                Mins1 = 30,
                Duration1 = 60,
                Hours2 = 3,
                Mins2 = 45,
                Duration2 = 90,
                Brightness1 = 0.5,
                Contrast1 = 0.6,
                Brightness2 = 0.7,
                Contrast2 = 0.8
            };

            // Act
            var StartSchedulerManger_Result = schedulerMangerPlugin.ReceiveScheduleInfo(scheduleInfoToSend);  //info to _ScheduleMap
            Assert.IsTrue(StartSchedulerManger_Result.IsCompletedSuccessfully);

            var ScheduleMap_result = (scheduleInfo)privateSchedulerManger.GetFieldOrProperty("_ScheduleMap");
            var WaitTag_result = (bool)privateSchedulerManger.GetFieldOrProperty("_WaitTag");
            Assert.That(scheduleInfoToSend, Is.EqualTo(ScheduleMap_result));
            Assert.IsFalse(WaitTag_result);
        }

        [Test]
        public void TestInitializeMonitorInfo()
        {
            Mock<IDisplayService> mockDisplayService = new Mock<IDisplayService>();
            PrivateObject privatevschedulerMangerObject = new PrivateObject(schedulerMangerPlugin);
            List<MonitorInfo> _allInfoMonitors = new List<MonitorInfo>();
            _allInfoMonitors.Add(monitorInfo1);
            mockDisplayService.Setup(x => x.GetMonitors()).Returns(Task.FromResult(_allInfoMonitors));
            var DisplayServiceObject = mockDisplayService.Object;
            privatevschedulerMangerObject.SetFieldOrProperty("_DisplayManagerPlugin", DisplayServiceObject);
            privatevschedulerMangerObject.Invoke("InitializeMonitorInfo");
            var monitorlist = (List<MonitorInfo>)privatevschedulerMangerObject.GetFieldOrProperty("_AllInfoMonitors");
            Assert.IsNotNull(monitorlist);
            Assert.Greater(monitorlist.Count, 0);
        }

        [Test]
        public void TestInitializeDisplayManagerPlugin()
        {
            Mock<IDisplayService> mockDisplayService = new Mock<IDisplayService>();
            PrivateObject privatevschedulerMangerObject = new PrivateObject(schedulerMangerPlugin);
            var DisplayServiceObject = mockDisplayService.Object;
            privatevschedulerMangerObject.SetFieldOrProperty("_DisplayManagerPlugin", DisplayServiceObject);
            privatevschedulerMangerObject.Invoke("InitializeDisplayManagerPlugin");
            var InitializeDisplayManagerPlugin_result = privatevschedulerMangerObject.GetFieldOrProperty("_DisplayManagerPlugin");
            Assert.IsNotNull(InitializeDisplayManagerPlugin_result);
        }

        [Test]
        public void TestInitializeScheduleInfo()
        {
            PrivateObject privatevschedulerMangerObject = new PrivateObject(schedulerMangerPlugin);
            privatevschedulerMangerObject.Invoke("InitializeScheduleInfo", monitorInfo1);
            var WaitTag_result = (bool)privatevschedulerMangerObject.GetFieldOrProperty("_WaitTag");
            Assert.IsNotNull(WaitTag_result);
            Assert.IsTrue(WaitTag_result);
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            displayPlugin.Dispose();
            vcpCorePlugin.Dispose();
            schedulerMangerPlugin.Dispose();
        }
    }
}