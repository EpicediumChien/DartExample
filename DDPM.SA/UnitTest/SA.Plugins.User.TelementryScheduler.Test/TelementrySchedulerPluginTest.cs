using DDPM.SA.Common;
using DDPM.SA.Common.Display;
using DDPM.SA.Common.Settings;
using DDPM.SA.Plugins.User.DisplayManager;
using DDPM.SA.Plugins.User.TelementryScheduler;
using Dell.Client.Framework.Interfaces;
using Dell.Client.Framework.UnitTestShared.Tests;
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities.ObjectModel;
using Moq;
using System;
using System.Windows.Controls;
using VcpCore.Common;
using VcpCore.Interfaces;
using VcpCore.Plugins;
using static DDPM.SA.Common.Settings.DDPMUserSettings;
using IDs = DDPM.SA.Common.IDs;

namespace DDPM.SA.Plugins.User.TelementryScheduler.Test
{
    public class TelementrySchedulerPluginTest
    {
        private Mock<IAgent> DisplayMangerAgent { get; } = new();
        private Mock<IAgent> VcpCoreAgent { get; } = new();
        private Mock<IAgent> TelementrySchedulerAgent { get; } = new();

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

        private DDPMAppSettings Ddpm_app_ = new DDPMAppSettings();
        private DDPMUserSettings Ddpm_user_ = new DDPMUserSettings()
        {
            Version = 1.0,
            Language = (int)Languages.en,
            IsSynchronizemonitor = false,
            Schedule = string.Empty,
            DelayFWUpdateInfoPackage = new FWUpdateInfoPackage(),
            LockRotate = true,
            TelementryFrequency = new FrequencyDateTime() { Month1stDay = DateTime.Now, PerDay = DateTime.Now, Weekly = DateTime.Now, },
            LockFWU_UI = true,
            UODFWUInfoPackage = new DokcUODUpdateInfoPackage(),
            SupportedMonitorList = new List<string> { "Testmonitor1" },
            DelaySWUpdateInfoPackage = new SWUpdateInfoPackage(),
            HotkeySettings = new HotkeySettings(),
            isDisplayConsentPage = false,
            EAProfile = new List<EAProfileDDPM> { new EAProfileDDPM() },
            EzSettings = new EzSettings(),
            EACustomList = new SplitJson[] { new SplitJson() },
        };
        private DDPMITConfig Ddpm_it_ = new DDPMITConfig()
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

        private TelementrySchedulerPlugin CreateSchedulerMangerPlugin()
        {
            TelementrySchedulerAgent.Setup(x => x.PluginManager.FindPluginByGuid(Guid.Parse(IDs.Telementry_Scheduler_Plugin_ID)));

            return new TelementrySchedulerPlugin(TelementrySchedulerAgent.Object);
        }

        private DisplayMangerPlugin displayPlugin;
        private VcpCorePlugin vcpCorePlugin;
        private TelementrySchedulerPlugin telementrySchedulerPlugin;
        private PrivateObject privatetelementrySchedulerPlugin;
        [OneTimeSetUp]
        public void Setup()
        {
            displayPlugin = CreateInitializeDisplayMangerPlugin();
            vcpCorePlugin = CreateInitializeVcpCorePlugin();
            telementrySchedulerPlugin = CreateSchedulerMangerPlugin();

            PrivateObject privateObject = new PrivateObject(displayPlugin);
            PrivateObject privatevcp = new PrivateObject(vcpCorePlugin);
            privatetelementrySchedulerPlugin = new PrivateObject(telementrySchedulerPlugin);
        }

        [Test]
        public void TestTelementrySchedulerPlugin()
        {
            Assert.IsNotNull(telementrySchedulerPlugin);
            PrivateObject privatetelementrySchedulerPlugin = new PrivateObject(telementrySchedulerPlugin);
            var agent2 = privatetelementrySchedulerPlugin.GetField("_agent") as IAgent;
            Assert.That(agent2, Is.EqualTo(TelementrySchedulerAgent.Object));
        }

        [Test]
        public void TestIsDisposed()
        {
            var Result = telementrySchedulerPlugin.IsDisposed;
            Assert.IsFalse(Result);
            PrivateObject privatetelementrySchedulerObject = new PrivateObject(telementrySchedulerPlugin);
            privatetelementrySchedulerObject.SetFieldOrProperty("IsDisposed", true);
            var Result2 = telementrySchedulerPlugin.IsDisposed;
            Assert.IsTrue(Result2);
        }

        [Test]
        public void TestStartTelemetrySchedulerManger()
        {
            bool StartTelemetrySchedulerManger1 = true;
            var StartTelemetrySchedulerManger_Result1 = telementrySchedulerPlugin.StartTelemetrySchedulerManger(StartTelemetrySchedulerManger1);  //IsStartTelementry true
            Assert.IsTrue(StartTelemetrySchedulerManger_Result1.IsCompletedSuccessfully);

            bool StartTelemetrySchedulerManger2 = true;
            var StartTelemetrySchedulerManger_Result2 = telementrySchedulerPlugin.StartTelemetrySchedulerManger(StartTelemetrySchedulerManger2);  //IsStartTelementry false
            Assert.IsTrue(StartTelemetrySchedulerManger_Result2.IsCompletedSuccessfully);
        }

        [Test]
        public void TestReceiveTelemetryInfo()
        {
            string EventTag = "Test EventTag";
            string EventValue = "Test EventValue";

            Telementry_Frequency Frequency1 = Telementry_Frequency.RealTime; //RealTime 3
            Telementry_Frequency Frequency2 = Telementry_Frequency.FirstDayofMonth; //FirstDayofMonth 0
            Telementry_Frequency Frequency3 = Telementry_Frequency.PerDay; //PerDay 1
            Telementry_Frequency Frequency4 = Telementry_Frequency.Weekly; //Weekly 2

            bool ReceiveTelemetryInfoActual = true;
            bool ReceiveTelemetryInfoExpected = false;
            bool IsTelemetryConsentOn1 = true;
            bool IsTelemetryConsentOn2 = false;

            Mock<IPlatinumSDKService> mockPlatinumSDKService = new Mock<IPlatinumSDKService>();
            mockPlatinumSDKService.Setup(x => x.UpdateEventValue(It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult(true));
            var mockPlatinumSDKServiceObject = mockPlatinumSDKService.Object;
            privatetelementrySchedulerPlugin.SetFieldOrProperty("_PlatinumSDKPlugin", mockPlatinumSDKServiceObject);  //_PlatinumSDKPlugin not null
            privatetelementrySchedulerPlugin.SetFieldOrProperty("IsTelemetryConsentOn", IsTelemetryConsentOn1);

            if (IsTelemetryConsentOn1)
            {
                if (Frequency1 == Telementry_Frequency.RealTime)
                {
                    var ReceiveTelemetryInfo_Result1 = telementrySchedulerPlugin.ReceiveTelemetryInfo(EventTag, EventValue, Frequency1).Result;  // Frequency1 3  RealTime;_PlatinumSDKPlugin not null, true
                    Assert.That(ReceiveTelemetryInfoActual, Is.EqualTo(ReceiveTelemetryInfo_Result1));
                }

                if (Frequency2 == Telementry_Frequency.FirstDayofMonth)
                {
                    var ReceiveTelemetryInfo_Result2 = telementrySchedulerPlugin.ReceiveTelemetryInfo(EventTag, EventValue, Frequency2).Result;  // Frequency2 0  FirstDayofMonth; _PlatinumSDKPlugin not null,true
                    Assert.That(ReceiveTelemetryInfoActual, Is.EqualTo(ReceiveTelemetryInfo_Result2));
                }

                if (Frequency3 == Telementry_Frequency.PerDay)
                {
                    var ReceiveTelemetryInfo_Result3 = telementrySchedulerPlugin.ReceiveTelemetryInfo(EventTag, EventValue, Frequency3).Result;  // Frequency3 1  PerDay; _PlatinumSDKPlugin not null,true
                    Assert.That(ReceiveTelemetryInfoActual, Is.EqualTo(ReceiveTelemetryInfo_Result3));
                }

                if (Frequency4 == Telementry_Frequency.Weekly)
                {
                    var ReceiveTelemetryInfo_Result4 = telementrySchedulerPlugin.ReceiveTelemetryInfo(EventTag, EventValue, Frequency4).Result;  // Frequency4 2  Weekly; _PlatinumSDKPlugin not null,true
                    Assert.That(ReceiveTelemetryInfoActual, Is.EqualTo(ReceiveTelemetryInfo_Result4));
                }
            }

            privatetelementrySchedulerPlugin.SetFieldOrProperty("IsTelemetryConsentOn", IsTelemetryConsentOn2); //IsTelemetryConsentOn2 false
            if (!IsTelemetryConsentOn2)
            {
                var ReceiveTelemetryInfo_Result5 = telementrySchedulerPlugin.ReceiveTelemetryInfo(EventTag, EventValue, Frequency1).Result;  // Frequency1 3  RealTime;_PlatinumSDKPlugin not null, IsTelemetryConsentOn2 false, return true
                Assert.That(ReceiveTelemetryInfoExpected, Is.EqualTo(ReceiveTelemetryInfo_Result5));
            }

        }

        [Test]
        public void TestInitializePlatinumSDKPlugin()
        {
            Mock<IPlatinumSDKService> mockPlatinumSDKService = new Mock<IPlatinumSDKService>();
            var mockPlatinumSDKServiceObject = mockPlatinumSDKService.Object;
            privatetelementrySchedulerPlugin.SetFieldOrProperty("_PlatinumSDKPlugin", mockPlatinumSDKServiceObject);  //_PlatinumSDKPlugin not null
            privatetelementrySchedulerPlugin.Invoke("InitializePlatinumSDKPlugin");
            var InitializePlatinumSDKPlugin_result = privatetelementrySchedulerPlugin.GetFieldOrProperty("_PlatinumSDKPlugin");
            Assert.IsNotNull(InitializePlatinumSDKPlugin_result);
        }

        [Test]
        public void TestReceiveTelemetryInfo_()
        {
            string EventTag = "Test EventTag";

            Telementry_Frequency Frequency1 = Telementry_Frequency.RealTime; //RealTime 3
            Telementry_Frequency Frequency2 = Telementry_Frequency.FirstDayofMonth; //FirstDayofMonth 0

            bool ReceiveTelemetryInfoActual = true;
            bool ReceiveTelemetryInfoExpected = false;
            bool IsTelemetryConsentOn1 = true;
            bool IsTelemetryConsentOn2 = false;

            Mock<IPlatinumSDKService> mockPlatinumSDKService = new Mock<IPlatinumSDKService>();
            mockPlatinumSDKService.Setup(x => x.UpdateEventValue(It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult(true));
            var mockPlatinumSDKServiceObject = mockPlatinumSDKService.Object;
            privatetelementrySchedulerPlugin.SetFieldOrProperty("_PlatinumSDKPlugin", mockPlatinumSDKServiceObject);  //_PlatinumSDKPlugin not null
            privatetelementrySchedulerPlugin.SetFieldOrProperty("IsTelemetryConsentOn", IsTelemetryConsentOn1);

            if (IsTelemetryConsentOn1)
            {
                if (Frequency1 == Telementry_Frequency.RealTime)
                {
                    var ReceiveTelemetryInfo_Result1 = telementrySchedulerPlugin.ReceiveTelemetryInfo(EventTag, Frequency1).Result;  // Frequency1 3  RealTime;_PlatinumSDKPlugin not null, true
                    Assert.That(ReceiveTelemetryInfoActual, Is.EqualTo(ReceiveTelemetryInfo_Result1));
                }
            }

            privatetelementrySchedulerPlugin.SetFieldOrProperty("IsTelemetryConsentOn", IsTelemetryConsentOn2); //IsTelemetryConsentOn2 false
            if (!IsTelemetryConsentOn2)
            {
                var ReceiveTelemetryInfo_Result2 = telementrySchedulerPlugin.ReceiveTelemetryInfo(EventTag, Frequency1).Result;  // Frequency1 3  RealTime;_PlatinumSDKPlugin not null, IsTelemetryConsentOn2 false, return true
                Assert.That(ReceiveTelemetryInfoExpected, Is.EqualTo(ReceiveTelemetryInfo_Result2));
            }

        }

        [Test]
        public void TestInitializeSettingsPlugin()
        {
            Mock<ISettingsManagerDev> mockSettingsManagerDev = new Mock<ISettingsManagerDev>();
            var mockSettingsManagerDevObject = mockSettingsManagerDev.Object;
            privatetelementrySchedulerPlugin.SetFieldOrProperty("_SettingsPlugin", mockSettingsManagerDevObject);  //_SettingsPlugin not null
            privatetelementrySchedulerPlugin.Invoke("InitializeSettingsPlugin");
            var InitializeSettingsPlugin_result = privatetelementrySchedulerPlugin.GetFieldOrProperty("_SettingsPlugin");
            Assert.IsNotNull(InitializeSettingsPlugin_result);
        }

        [Test]
        public void TestGetGlobalsetting_IsTelemetryConsentOn()
        {
            bool GetGlobalsetting_IsTelemetryConsentOn1 = true;
            var GetGlobalsetting_IsTelemetryConsentOn_Result1 = telementrySchedulerPlugin.GetGlobalsetting_IsTelemetryConsentOn(GetGlobalsetting_IsTelemetryConsentOn1);  //GetGlobalsetting_IsTelemetryConsentOn_Result1 true
            Assert.IsTrue(GetGlobalsetting_IsTelemetryConsentOn_Result1.IsCompletedSuccessfully);

            bool GetGlobalsetting_IsTelemetryConsentOn2 = false;
            var GetGlobalsetting_IsTelemetryConsentOn_Result1r_Result2 = telementrySchedulerPlugin.GetGlobalsetting_IsTelemetryConsentOn(GetGlobalsetting_IsTelemetryConsentOn2);  //GetGlobalsetting_IsTelemetryConsentOn_Result1 false
            Assert.IsTrue(GetGlobalsetting_IsTelemetryConsentOn_Result1r_Result2.IsCompletedSuccessfully);
        }

        [Test]
        public void TestInitializeDisplayManagerPlugin()
        {
            Mock<IDisplayService> mockDisplayService = new Mock<IDisplayService>();
            PrivateObject privatetelementrySchedulerPluginObject = new PrivateObject(telementrySchedulerPlugin);
            var DisplayServiceObject = mockDisplayService.Object;
            privatetelementrySchedulerPluginObject.SetFieldOrProperty("_DisplayManagerPlugin", DisplayServiceObject);
            privatetelementrySchedulerPluginObject.Invoke("InitializeDisplayManagerPlugin");
            var InitializeDisplayManagerPlugin_result = privatetelementrySchedulerPluginObject.GetFieldOrProperty("_DisplayManagerPlugin");
            Assert.IsNotNull(InitializeDisplayManagerPlugin_result);
        }

        [Test]
        public void TestSet_FrequencyDateTime()
        {
            bool Set_FrequencyDateTime2 = true;

            var settings_Dataconfig = new DDPMSettings(Ddpm_app_, Ddpm_user_, Ddpm_it_);

            Mock<ISettingsManagerDev> mockSettingsManagerDev = new Mock<ISettingsManagerDev>();


            mockSettingsManagerDev.Setup(x => x.SetAppConfigData(It.IsAny<DDPMSettings>())).Returns(Task.FromResult(true));
            var mockSettingsManagerDevObject = mockSettingsManagerDev.Object;
            privatetelementrySchedulerPlugin.SetFieldOrProperty("_SettingsPlugin", mockSettingsManagerDevObject);

            var Set_FrequencyDateTime_result2 = (bool)privatetelementrySchedulerPlugin.Invoke("Set_FrequencyDateTime", settings_Dataconfig); //_SettingsPlugin not null
            Assert.That(Set_FrequencyDateTime2, Is.EqualTo(Set_FrequencyDateTime_result2));
        }

        [Test]
        public void TestGet_FrequencyDateTime()
        {
            bool Get_FrequencyDateTim2 = true;

            var Ddpm_app_ = new DDPMAppSettings();
            var Ddpm_user_ = new DDPMUserSettings()
            {
                Version = 1.0,
                Language = (int)Languages.en,
                IsSynchronizemonitor = false,
                Schedule = string.Empty,
                DelayFWUpdateInfoPackage = new FWUpdateInfoPackage(),
                LockRotate = true,
                TelementryFrequency = new FrequencyDateTime() { Month1stDay = DateTime.Now, PerDay = DateTime.Now, Weekly = DateTime.Now, },
                LockFWU_UI = true,
                UODFWUInfoPackage = new DokcUODUpdateInfoPackage(),
                SupportedMonitorList = new List<string> { "Testmonitor1" },
                DelaySWUpdateInfoPackage = new SWUpdateInfoPackage(),
                HotkeySettings = new HotkeySettings(),
                isDisplayConsentPage = false,
                EAProfile = new List<EAProfileDDPM> { new EAProfileDDPM() },
                EzSettings = new EzSettings(),
                EACustomList = new SplitJson[] { new SplitJson() },
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

            var settings_Dataconfig = new DDPMSettings(Ddpm_app_, Ddpm_user_, Ddpm_it_);

            Mock<ISettingsManagerDev> mockSettingsManagerDev = new Mock<ISettingsManagerDev>();


            mockSettingsManagerDev.Setup(x => x.ReloadAppConfigData(It.IsAny<bool>())).Returns(Task.FromResult(settings_Dataconfig));
            var mockSettingsManagerDevObject = mockSettingsManagerDev.Object;
            privatetelementrySchedulerPlugin.SetFieldOrProperty("_SettingsPlugin", mockSettingsManagerDevObject);

            var Get_FrequencyDateTime_result2 = (bool)privatetelementrySchedulerPlugin.Invoke("Get_FrequencyDateTime"); //_SettingsPlugin not null
            Assert.That(Get_FrequencyDateTim2, Is.EqualTo(Get_FrequencyDateTime_result2));
        }

        [Test]
        public void TestSettingsReady()
        {
            object obj = new object();
            EventArgs eventArgs = new EventArgs();

            var Ddpm_app_ = new DDPMAppSettings();
            var Ddpm_user_ = new DDPMUserSettings()
            {
                Version = 1.0,
                Language = (int)Languages.en,
                IsSynchronizemonitor = false,
                Schedule = string.Empty,
                DelayFWUpdateInfoPackage = new FWUpdateInfoPackage(),
                LockRotate = true,
                TelementryFrequency = new FrequencyDateTime() { Month1stDay = DateTime.Now, PerDay = DateTime.Now, Weekly = DateTime.Now, },
                LockFWU_UI = true,
                UODFWUInfoPackage = new DokcUODUpdateInfoPackage(),
                SupportedMonitorList = new List<string> { "Testmonitor1" },
                DelaySWUpdateInfoPackage = new SWUpdateInfoPackage(),
                HotkeySettings = new HotkeySettings(),
                isDisplayConsentPage = false,
                EAProfile = new List<EAProfileDDPM> { new EAProfileDDPM() },
                EzSettings = new EzSettings(),
                EACustomList = new SplitJson[] { new SplitJson() },
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

            var settings_Dataconfig = new DDPMSettings(Ddpm_app_, Ddpm_user_, Ddpm_it_);

            Mock<ISettingsManagerDev> mockSettingsManagerDev = new Mock<ISettingsManagerDev>();
            mockSettingsManagerDev.Setup(x => x.ReloadAppConfigData(It.IsAny<bool>())).Returns(Task.FromResult(settings_Dataconfig));
            var mockSettingsManagerDevObject = mockSettingsManagerDev.Object;
            privatetelementrySchedulerPlugin.SetFieldOrProperty("_SettingsPlugin", mockSettingsManagerDevObject);

            privatetelementrySchedulerPlugin.Invoke("SettingsReady", obj, eventArgs); //_SettingsPlugin not null
            Assert.IsTrue(true);
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            displayPlugin.Dispose();
            vcpCorePlugin.Dispose();
            telementrySchedulerPlugin.Dispose();
        }
    }
}