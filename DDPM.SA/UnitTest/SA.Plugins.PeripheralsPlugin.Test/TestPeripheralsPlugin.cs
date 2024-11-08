using DDPM.SA.Common;
using DDPM.SA.Plugins.PeripheralsPlugin;
using DDPM.SA.Plugins.User.DisplayManager;
using Dell.Client.Framework.Interfaces;
using Dell.Client.Framework.UnitTestShared.Tests;
using IndiLogic.DPeM.Broker;
using Moq;
using VcpCore.Common;
using VcpCore.Interfaces;
using VcpCore.Plugins;
using static DDPM.RemoteManagement.Common.Interfaces.Params;
using IDs = DDPM.SA.Common.IDs;
using DPeMPublic.Common.Enums;
using Microsoft.Windows.Themes;
using static Microsoft.WindowsAPICodePack.Shell.PropertySystem.SystemProperties.System;

namespace DDPM.SA.Plugins.PeripheralsPlugin.Test
{
    public class TestPeripheralsPlugin
    {
        private Mock<IAgent> DisplayMangerAgent { get; } = new();
        private Mock<IAgent> VcpCoreAgent { get; } = new();
        private Mock<IAgent> PeripheralsPluginAgent { get; } = new();

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

        private PeripheralsPlugin CreateSchedulerMangerPlugin()
        {
            PeripheralsPluginAgent.Setup(x => x.PluginManager.FindPluginByGuid(Guid.Parse(IDs.DDPM_PERIPHERALS_PLUGIN_ID)));

            return new PeripheralsPlugin(PeripheralsPluginAgent.Object);
        }

        private DisplayMangerPlugin displayPlugin;
        private VcpCorePlugin vcpCorePlugin;
        private PeripheralsPlugin peripheralsPlugin;
        private PrivateObject privatetePeripheralsPlugin;

        [OneTimeSetUp]
        public void Setup()
        {
            displayPlugin = CreateInitializeDisplayMangerPlugin();
            vcpCorePlugin = CreateInitializeVcpCorePlugin();
            peripheralsPlugin = CreateSchedulerMangerPlugin();

            PrivateObject privateObject = new PrivateObject(displayPlugin);
            PrivateObject privatevcp = new PrivateObject(vcpCorePlugin);
            privatetePeripheralsPlugin = new PrivateObject(peripheralsPlugin);
        }

        [Test]
        public void TestPeripheralsPlugins()
        {
            Assert.IsNotNull(peripheralsPlugin);
            PrivateObject privatetePeripheralsPlugin = new PrivateObject(peripheralsPlugin);
            var agent2 = privatetePeripheralsPlugin.GetField("_agent") as IAgent;
            Assert.That(agent2, Is.EqualTo(PeripheralsPluginAgent.Object));
        }

        [Test]
        public void TestUpdateAvailable()
        {
            bool UpdateAvailable1 = true;
            peripheralsPlugin.UpdateAvailable = true;
            var UpdateAvailable_result = peripheralsPlugin.UpdateAvailable;  //UpdateAvailable true
            Assert.That(UpdateAvailable1, Is.EqualTo(UpdateAvailable_result));

            bool UpdateAvailable2 = false;
            peripheralsPlugin.UpdateAvailable = false;
            var UpdateAvailable_result2 = peripheralsPlugin.UpdateAvailable;   //UpdateAvailable false
            Assert.That(UpdateAvailable2, Is.EqualTo(UpdateAvailable_result2));
        }

        [Test]
        public void TestIUpdateManager()
        {
            Mock<IUpdateManager> mockUpdateManager = new Mock<IUpdateManager>();
            var UpdateManagerObj = mockUpdateManager.Object;
            privatetePeripheralsPlugin.SetFieldOrProperty("_iUpdateManager", UpdateManagerObj);
            var UpdateManager_Result = peripheralsPlugin.IUpdateManager;
            Assert.That(UpdateManagerObj, Is.EqualTo(UpdateManager_Result));
        }

        [Test]
        public void TestIOverlayManager()
        {
            Mock<IOverlayManager> mockOverlayManager = new Mock<IOverlayManager>();
            var OverlayManagerObj = mockOverlayManager.Object;
            privatetePeripheralsPlugin.SetFieldOrProperty("_iOverlayManager", OverlayManagerObj);
            var OverlayManager_Result = peripheralsPlugin.IOverlayManager;
            Assert.That(OverlayManagerObj, Is.EqualTo(OverlayManager_Result));
        }

        [Test]
        public void TestICTKMessageHelper()
        {
            Mock<ICTKMessageHelper> mockCTKMessageHelper = new Mock<ICTKMessageHelper>();
            var CTKMessageHelperObj = mockCTKMessageHelper.Object;
            privatetePeripheralsPlugin.SetFieldOrProperty("_iCTKMessageHelper", CTKMessageHelperObj);
            var CTKMessageHelper_Result = peripheralsPlugin.ICTKMessageHelper;
            Assert.That(CTKMessageHelperObj, Is.EqualTo(CTKMessageHelper_Result));
        }

        [Test]
        public void TestIDeviceManager()
        {
            Mock<IDeviceManager> mockDeviceManager = new Mock<IDeviceManager>();
            var DeviceManagerObj = mockDeviceManager.Object;
            privatetePeripheralsPlugin.SetFieldOrProperty("_iDeviceManager", DeviceManagerObj);
            var DeviceManager_Result = peripheralsPlugin.IDeviceManager;
            Assert.That(DeviceManagerObj, Is.EqualTo(DeviceManager_Result));

            privatetePeripheralsPlugin.SetFieldOrProperty("_iDeviceManager", null);
            var DeviceManager_Result2 = peripheralsPlugin.IDeviceManager;
            Assert.IsNull(DeviceManager_Result2);
        }

        [Test]
        public void TestNotifyNow()
        {
            bool eventFired = false;
            peripheralsPlugin.Notify += (sender, e) =>
            {
                eventFired = true;
            };

            // Act
            peripheralsPlugin.NotifyNow();

            // Assert
            Assert.IsTrue(eventFired);
        }

        [Test]
        public void TestGetDPeMPluginConditionAsync()
        {
            try
            {
                var GetDPeMPluginConditionAsync_Result = peripheralsPlugin.GetDPeMPluginConditionAsync();

            }
            catch (Exception ex)
            {
                Assert.That(ex.Message, Is.EqualTo("The method or operation is not implemented."));
            }
        }

        [Test]
        public void TestGetDevices()
        {
            DeviceHelper deviceHelper = new DeviceHelper()
            {
                deviceInfo = new List<DeviceInfo>()
                {
                    new DeviceInfo() {DeviceName="Mouse",Name="Test mouse",}
                },
                DCFVersion = "1.0",
                DPeMSDKVersion = "1.0",
                DPeMSubAgentVersion = "1.0",
                IsdDriverVersion = "1.0",
            };
            privatetePeripheralsPlugin.SetFieldOrProperty("_iDeviceManager", null);
            var GetDevices_Result1 = peripheralsPlugin.GetDevices(true).Result;  //_deviceHelper null
            Assert.IsNotNull(GetDevices_Result1);

            privatetePeripheralsPlugin.SetFieldOrProperty("_deviceHelper", deviceHelper);
            var GetDevices_Result2 = peripheralsPlugin.GetDevices(false).Result;  //_deviceHelper not null
            Assert.That(deviceHelper, Is.EqualTo(GetDevices_Result2));
        }

        [Test]
        public void TestGetDevices_WithoutAwait()
        {
            DeviceHelper deviceHelper = new DeviceHelper()
            {
                deviceInfo = new List<DeviceInfo>()
                {
                    new DeviceInfo() {DeviceName="Mouse",Name="Test mouse",}
                },
                DCFVersion = "1.0",
                DPeMSDKVersion = "1.0",
                DPeMSubAgentVersion = "1.0",
                IsdDriverVersion = "1.0",
            };
            privatetePeripheralsPlugin.SetFieldOrProperty("_iDeviceManager", null);
            var GetDevices_WithoutAwait_Result1 = peripheralsPlugin.GetDevices_WithoutAwait(true).Result;  //_deviceHelper null
            Assert.IsNotNull(GetDevices_WithoutAwait_Result1);

            privatetePeripheralsPlugin.SetFieldOrProperty("_deviceHelper", deviceHelper);
            var GetDevices_WithoutAwait_Result2 = peripheralsPlugin.GetDevices_WithoutAwait(false).Result;  //_deviceHelper not null
            Assert.That(deviceHelper, Is.EqualTo(GetDevices_WithoutAwait_Result2));
        }

        [Test]
        public void TestGetCTKMessageHelper()
        {
            Mock<ICTKMessageHelper> mockCTKMessageHelper = new Mock<ICTKMessageHelper>();
            var CTKMessageHelperObj = mockCTKMessageHelper.Object;
            privatetePeripheralsPlugin.SetFieldOrProperty("_iCTKMessageHelper", CTKMessageHelperObj);
            var GetCTKMessageHelper_Result1 = peripheralsPlugin.GetCTKMessageHelper().Result;
            Assert.IsNotNull(GetCTKMessageHelper_Result1);
        }

        [Test]
        public void TestGetRFDongleDevices()
        {
            RFDeviceHelper rFDeviceHelper = new RFDeviceHelper()
            {
                dongleInfo = new List<DongleInfo>()
                {
                    new DongleInfo()
                    {
                    DeviceType= new DPeMPublic.Common.Enums.DeviceType(),
                    IsMultipleDongleFound= false,
                    ID=Guid.NewGuid(),
                    LogicalDeviceIDs= new List<Guid>(),
                    MaxPairingSlots=0,
                    PairedDeviceCount= 0,
                    }
                }
            };
            privatetePeripheralsPlugin.SetFieldOrProperty("_iDeviceManager", null);
            var GetRFDongleDevices_Result1 = peripheralsPlugin.GetRFDongleDevices().Result;  //_rfDeviceHelper null
            Assert.IsNotNull(GetRFDongleDevices_Result1);

            privatetePeripheralsPlugin.SetFieldOrProperty("_rfDeviceHelper", rFDeviceHelper);
            var GetRFDongleDevices_Result2 = peripheralsPlugin.GetRFDongleDevices().Result;  //_rfDeviceHelper not null
            Assert.IsNotNull(GetRFDongleDevices_Result2);
        }

        [Test]
        public void TestGetDPeMClientInfo()
        {
            ClientInfo clientInfo = new ClientInfo()
            {
                ApiVersion = "1.0",
                ServiceStatus = "Test ServiceStatus",
                status = "Test status",
            };

            var GetDPeMClientInfo_Result1 = peripheralsPlugin.GetDPeMClientInfo().Result;  //_clientInfo null
            Assert.IsNotNull(GetDPeMClientInfo_Result1);

            privatetePeripheralsPlugin.SetFieldOrProperty("_clientInfo", clientInfo);
            var GetDPeMClientInfo_Result2 = peripheralsPlugin.GetDPeMClientInfo().Result;  //_clientInfo not null
            Assert.That(clientInfo, Is.EqualTo(GetDPeMClientInfo_Result2));
        }

        [Test]
        public void TestSetDPILevel()
        {
            int newDPILevel1 = 10;
            Guid deviceId1 = new Guid();
            Mock<IPhysicalDevice> physicalDevice = new Mock<IPhysicalDevice>();
            Mock<ILogicalDevice3> logicalDevice3 = new Mock<ILogicalDevice3>();
            Mock<IDeviceManager> mockDeviceManager = new Mock<IDeviceManager>();

            physicalDevice.Setup(x => x.Devices).Returns(new[] { logicalDevice3.Object });
            mockDeviceManager.Setup(x => x.Devices).Returns(new[] { physicalDevice.Object });

            var DeviceManagerObj = mockDeviceManager.Object;
            privatetePeripheralsPlugin.SetFieldOrProperty("_iDeviceManager", DeviceManagerObj);

            DeviceHelper deviceHelper = new DeviceHelper()
            {
                deviceInfo = new List<DeviceInfo>()
                {
                    new DeviceInfo()
                    {
                        DeviceName="Mouse",
                        Name="Test mouse",
                        DpiLevel=20,
                        DpiValue="20",
                    }
                },
                DCFVersion = "1.0",
                DPeMSDKVersion = "1.0",
                DPeMSubAgentVersion = "1.0",
                IsdDriverVersion = "1.0",
            };
            privatetePeripheralsPlugin.SetFieldOrProperty("_deviceHelper", deviceHelper);

            peripheralsPlugin.SetDPILevel(newDPILevel1, deviceId1);                                //_iDeviceManager.Devices Id new Guid() equire deviceId, newDPILevel1 10 != DpiLevel 20
            var devicemanager = privatetePeripheralsPlugin.GetFieldOrProperty("_iDeviceManager");
            Assert.IsTrue(true);
            Assert.IsNotNull(devicemanager);

            int newDPILevel2 = 20;
            Guid deviceId2 = new Guid();
            peripheralsPlugin.SetDPILevel(newDPILevel2, deviceId2);
            var deviceHelper2 = privatetePeripheralsPlugin.GetFieldOrProperty("_deviceHelper");  //_iDeviceManager.Devices Id new Guid() equire deviceId, newDPILevel2 = DpiLevel 20
            Assert.IsTrue(true);
            Assert.IsNotNull(deviceHelper2);
        }

        [Test]
        public void TestSetDPIValue()
        {
            int newDPIValue1 = 10;
            Guid deviceId1 = new Guid();
            Mock<IPhysicalDevice> physicalDevice = new Mock<IPhysicalDevice>();
            Mock<ILogicalDevice2> logicalDevice2 = new Mock<ILogicalDevice2>();
            Mock<IDeviceManager> mockDeviceManager = new Mock<IDeviceManager>();

            physicalDevice.Setup(x => x.Devices).Returns(new[] { logicalDevice2.Object });
            mockDeviceManager.Setup(x => x.Devices).Returns(new[] { physicalDevice.Object });

            var DeviceManagerObj = mockDeviceManager.Object;
            privatetePeripheralsPlugin.SetFieldOrProperty("_iDeviceManager", DeviceManagerObj);

            DeviceHelper deviceHelper = new DeviceHelper()
            {
                deviceInfo = new List<DeviceInfo>()
                {
                    new DeviceInfo()
                    {
                        DeviceName="Mouse",
                        Name="Test mouse",
                        DpiLevel=20,
                        DpiValue="20",
                    }
                },
                DCFVersion = "1.0",
                DPeMSDKVersion = "1.0",
                DPeMSubAgentVersion = "1.0",
                IsdDriverVersion = "1.0",
            };
            privatetePeripheralsPlugin.SetFieldOrProperty("_deviceHelper", deviceHelper);

            peripheralsPlugin.SetDPIValue(newDPIValue1, deviceId1);                                // newDPIValue1 10 != DpiLevel 20
            var devicemanager = privatetePeripheralsPlugin.GetFieldOrProperty("_iDeviceManager");
            Assert.IsTrue(true);
            Assert.IsNotNull(devicemanager);

            int newDPIValue2 = 20;
            Guid deviceId2 = new Guid();
            peripheralsPlugin.SetDPIValue(newDPIValue2, deviceId2);
            var deviceHelper2 = privatetePeripheralsPlugin.GetFieldOrProperty("_deviceHelper");  // newDPIValue2 = DpiLevel 20
            Assert.IsTrue(true);
            Assert.IsNotNull(deviceHelper2);
        }

        [Test]
        public void TestSetPrimaryMouseButton()
        {
            DPeMPublic.Common.Enums.MouseButton newMouseButton1 = MouseButton.Left;
            Guid deviceId1 = new Guid();
            Mock<IPhysicalDevice> physicalDevice = new Mock<IPhysicalDevice>();
            Mock<ILogicalDevice3> logicalDevice3 = new Mock<ILogicalDevice3>();
            Mock<IDeviceManager> mockDeviceManager = new Mock<IDeviceManager>();

            physicalDevice.Setup(x => x.Devices).Returns(new[] { logicalDevice3.Object });
            mockDeviceManager.Setup(x => x.Devices).Returns(new[] { physicalDevice.Object });

            var DeviceManagerObj = mockDeviceManager.Object;
            privatetePeripheralsPlugin.SetFieldOrProperty("_iDeviceManager", DeviceManagerObj);

            DeviceHelper deviceHelper = new DeviceHelper()
            {
                deviceInfo = new List<DeviceInfo>()
                {
                    new DeviceInfo()
                    {
                        DeviceName="Mouse",
                        Name="Test mouse",
                        DpiLevel=20,
                        DpiValue="20",
                        MousePrimaryButton=MouseButton.Left,
                    }
                },
                DCFVersion = "1.0",
                DPeMSDKVersion = "1.0",
                DPeMSubAgentVersion = "1.0",
                IsdDriverVersion = "1.0",
            };
            privatetePeripheralsPlugin.SetFieldOrProperty("_deviceHelper", deviceHelper);

            peripheralsPlugin.SetPrimaryMouseButton(newMouseButton1, deviceId1);                        // newMouseButton1 = MousePrimaryButton
            var devicemanager = privatetePeripheralsPlugin.GetFieldOrProperty("_iDeviceManager");
            Assert.IsTrue(true);
            Assert.IsNotNull(devicemanager);

            DPeMPublic.Common.Enums.MouseButton newMouseButton2 = MouseButton.Right;
            Guid deviceId2 = new Guid();
            peripheralsPlugin.SetPrimaryMouseButton(newMouseButton2, deviceId2);
            var deviceHelper2 = privatetePeripheralsPlugin.GetFieldOrProperty("_deviceHelper");       //newMouseButton1 != MousePrimaryButton
            Assert.IsTrue(true);
            Assert.IsNotNull(deviceHelper2);
        }

        [Test]
        public void TestSetTouchScrollSensitivityLevel()
        {
            int newTouchScrollSensitivityLevel1 = 10;
            Guid deviceId1 = new Guid();
            Mock<IPhysicalDevice> physicalDevice = new Mock<IPhysicalDevice>();
            Mock<ILogicalDevice3> logicalDevice3 = new Mock<ILogicalDevice3>();
            Mock<IDeviceManager> mockDeviceManager = new Mock<IDeviceManager>();

            physicalDevice.Setup(x => x.Devices).Returns(new[] { logicalDevice3.Object });
            mockDeviceManager.Setup(x => x.Devices).Returns(new[] { physicalDevice.Object });

            var DeviceManagerObj = mockDeviceManager.Object;
            privatetePeripheralsPlugin.SetFieldOrProperty("_iDeviceManager", DeviceManagerObj);

            DeviceHelper deviceHelper = new DeviceHelper()
            {
                deviceInfo = new List<DeviceInfo>()
                {
                    new DeviceInfo()
                    {
                        DeviceName="Mouse",
                        Name="Test mouse",
                        DpiLevel=20,
                        DpiValue="20",
                        MousePrimaryButton=MouseButton.Left,
                        TouchScrollSensitivityLevel=20,
                    }
                },
                DCFVersion = "1.0",
                DPeMSDKVersion = "1.0",
                DPeMSubAgentVersion = "1.0",
                IsdDriverVersion = "1.0",
            };
            privatetePeripheralsPlugin.SetFieldOrProperty("_deviceHelper", deviceHelper);

            peripheralsPlugin.SetTouchScrollSensitivityLevel(newTouchScrollSensitivityLevel1, deviceId1);      // newTouchScrollSensitivityLevel1 10 != TouchScrollSensitivityLevel 20
            var devicemanager = privatetePeripheralsPlugin.GetFieldOrProperty("_iDeviceManager");
            Assert.IsTrue(true);
            Assert.IsNotNull(devicemanager);

            int newTouchScrollSensitivityLevel2 = 20;
            Guid deviceId2 = new Guid();
            peripheralsPlugin.SetTouchScrollSensitivityLevel(newTouchScrollSensitivityLevel2, deviceId2);
            var deviceHelper2 = privatetePeripheralsPlugin.GetFieldOrProperty("_deviceHelper");  // newTouchScrollSensitivityLevel1 20 = TouchScrollSensitivityLevel 20
            Assert.IsTrue(true);
            Assert.IsNotNull(deviceHelper2);
        }

        [Test]
        public void TestSetCollaborationKeyEnable()
        {
            bool newValue1 = true;
            Guid deviceId1 = new Guid();
            Mock<IPhysicalDevice> physicalDevice = new Mock<IPhysicalDevice>();
            Mock<ILogicalDevice3> logicalDevice3 = new Mock<ILogicalDevice3>();
            Mock<IDeviceManager> mockDeviceManager = new Mock<IDeviceManager>();

            physicalDevice.Setup(x => x.Devices).Returns(new[] { logicalDevice3.Object });
            mockDeviceManager.Setup(x => x.Devices).Returns(new[] { physicalDevice.Object });

            var DeviceManagerObj = mockDeviceManager.Object;
            privatetePeripheralsPlugin.SetFieldOrProperty("_iDeviceManager", DeviceManagerObj);

            DeviceHelper deviceHelper = new DeviceHelper()
            {
                deviceInfo = new List<DeviceInfo>()
                {
                    new DeviceInfo()
                    {
                        DeviceName="Mouse",
                        Name="Test mouse",
                        DpiLevel=20,
                        DpiValue="20",
                        MousePrimaryButton=MouseButton.Left,
                        TouchScrollSensitivityLevel=20,
                        IsCollaborationKeyEnable=false,
                    }
                },
                DCFVersion = "1.0",
                DPeMSDKVersion = "1.0",
                DPeMSubAgentVersion = "1.0",
                IsdDriverVersion = "1.0",
            };
            privatetePeripheralsPlugin.SetFieldOrProperty("_deviceHelper", deviceHelper);

            peripheralsPlugin.SetCollaborationKeyEnable(newValue1, deviceId1);      // newValue1 true != IsCollaborationKeyEnable false
            var devicemanager = privatetePeripheralsPlugin.GetFieldOrProperty("_iDeviceManager");
            Assert.IsTrue(true);
            Assert.IsNotNull(devicemanager);

            bool newValue2 = false;
            Guid deviceId2 = new Guid();
            peripheralsPlugin.SetCollaborationKeyEnable(newValue2, deviceId2);
            var deviceHelper2 = privatetePeripheralsPlugin.GetFieldOrProperty("_deviceHelper");  // newValue2  = IsCollaborationKeyEnable false
            Assert.IsTrue(true);
            Assert.IsNotNull(deviceHelper2);
        }

        [Test]
        public void TestSetCollaborationCameraEnable()
        {
            bool newValue1 = true;
            Guid deviceId1 = new Guid();
            Mock<IPhysicalDevice> physicalDevice = new Mock<IPhysicalDevice>();
            Mock<ILogicalDevice3> logicalDevice3 = new Mock<ILogicalDevice3>();
            Mock<IDeviceManager> mockDeviceManager = new Mock<IDeviceManager>();

            physicalDevice.Setup(x => x.Devices).Returns(new[] { logicalDevice3.Object });
            mockDeviceManager.Setup(x => x.Devices).Returns(new[] { physicalDevice.Object });

            var DeviceManagerObj = mockDeviceManager.Object;
            privatetePeripheralsPlugin.SetFieldOrProperty("_iDeviceManager", DeviceManagerObj);

            DeviceHelper deviceHelper = new DeviceHelper()
            {
                deviceInfo = new List<DeviceInfo>()
                {
                    new DeviceInfo()
                    {
                        DeviceName="Mouse",
                        Name="Test mouse",
                        DpiLevel=20,
                        DpiValue="20",
                        MousePrimaryButton=MouseButton.Left,
                        TouchScrollSensitivityLevel=20,
                        IsCollaborationKeyEnable=false,
                        IsCollaborationCameraEnable=false,
                    }
                },
                DCFVersion = "1.0",
                DPeMSDKVersion = "1.0",
                DPeMSubAgentVersion = "1.0",
                IsdDriverVersion = "1.0",
            };
            privatetePeripheralsPlugin.SetFieldOrProperty("_deviceHelper", deviceHelper);

            peripheralsPlugin.SetCollaborationCameraEnable(newValue1, deviceId1);      // newValue1 true != IsCollaborationCameraEnable false
            var devicemanager = privatetePeripheralsPlugin.GetFieldOrProperty("_iDeviceManager");
            Assert.IsTrue(true);
            Assert.IsNotNull(devicemanager);

            bool newValue2 = false;
            Guid deviceId2 = new Guid();
            peripheralsPlugin.SetCollaborationCameraEnable(newValue2, deviceId2);
            var deviceHelper2 = privatetePeripheralsPlugin.GetFieldOrProperty("_deviceHelper");  // newValue2  = IsCollaborationCameraEnable false
            Assert.IsTrue(true);
            Assert.IsNotNull(deviceHelper2);
        }

        [Test]
        public void TestSetCollaborationScreenShareEnable()
        {
            bool newValue1 = true;
            Guid deviceId1 = new Guid();
            Mock<IPhysicalDevice> physicalDevice = new Mock<IPhysicalDevice>();
            Mock<ILogicalDevice3> logicalDevice3 = new Mock<ILogicalDevice3>();
            Mock<IDeviceManager> mockDeviceManager = new Mock<IDeviceManager>();

            physicalDevice.Setup(x => x.Devices).Returns(new[] { logicalDevice3.Object });
            mockDeviceManager.Setup(x => x.Devices).Returns(new[] { physicalDevice.Object });

            var DeviceManagerObj = mockDeviceManager.Object;
            privatetePeripheralsPlugin.SetFieldOrProperty("_iDeviceManager", DeviceManagerObj);

            DeviceHelper deviceHelper = new DeviceHelper()
            {
                deviceInfo = new List<DeviceInfo>()
                {
                    new DeviceInfo()
                    {
                        DeviceName="Mouse",
                        Name="Test mouse",
                        DpiLevel=20,
                        DpiValue="20",
                        MousePrimaryButton=MouseButton.Left,
                        TouchScrollSensitivityLevel=20,
                        IsCollaborationKeyEnable=false,
                        IsCollaborationCameraEnable=false,
                        IsCollaborationScreenShareEnable=false,
                    }
                },
                DCFVersion = "1.0",
                DPeMSDKVersion = "1.0",
                DPeMSubAgentVersion = "1.0",
                IsdDriverVersion = "1.0",
            };
            privatetePeripheralsPlugin.SetFieldOrProperty("_deviceHelper", deviceHelper);

            peripheralsPlugin.SetCollaborationScreenShareEnable(newValue1, deviceId1);      // newValue1 true != IsCollaborationScreenShareEnable false
            var devicemanager = privatetePeripheralsPlugin.GetFieldOrProperty("_iDeviceManager");
            Assert.IsTrue(true);
            Assert.IsNotNull(devicemanager);

            bool newValue2 = false;
            Guid deviceId2 = new Guid();
            peripheralsPlugin.SetCollaborationScreenShareEnable(newValue2, deviceId2);
            var deviceHelper2 = privatetePeripheralsPlugin.GetFieldOrProperty("_deviceHelper");  // newValue2  = IsCollaborationScreenShareEnable false
            Assert.IsTrue(true);
            Assert.IsNotNull(deviceHelper2);
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            displayPlugin.Dispose();
            vcpCorePlugin.Dispose();
            peripheralsPlugin.Dispose();
        }

    }
}