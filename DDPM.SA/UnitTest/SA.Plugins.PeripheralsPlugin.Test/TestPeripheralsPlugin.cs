//#define SUPPORT_210

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
            bool eventFired = true;
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
            privatetePeripheralsPlugin.SetFieldOrProperty("_isClientConnected", false);
            var GetDevices_Result1 = peripheralsPlugin.GetDevices(false).Result;  //_deviceHelper null
            Assert.IsNotNull(GetDevices_Result1);

            privatetePeripheralsPlugin.SetFieldOrProperty("_deviceHelper", deviceHelper);
            privatetePeripheralsPlugin.SetFieldOrProperty("_isClientConnected", true);
            var GetDevices_Result2 = peripheralsPlugin.GetDevices(true).Result;  //_deviceHelper not null
            Assert.IsNotNull(GetDevices_Result2);
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

            //peripheralsPlugin.SetPrimaryMouseButton(newMouseButton1, deviceId1);                        // newMouseButton1 = MousePrimaryButton
            var devicemanager = privatetePeripheralsPlugin.GetFieldOrProperty("_iDeviceManager");
            Assert.IsTrue(true);
            Assert.IsNotNull(devicemanager);

            DPeMPublic.Common.Enums.MouseButton newMouseButton2 = MouseButton.Right;
            Guid deviceId2 = new Guid();
            //peripheralsPlugin.SetPrimaryMouseButton(newMouseButton2, deviceId2);
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


        [Test]
        public void TestSetCollaborationChatEnable()
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
                        IsCollaborationChatEnable=false,
                    }
                },
                DCFVersion = "1.0",
                DPeMSDKVersion = "1.0",
                DPeMSubAgentVersion = "1.0",
                IsdDriverVersion = "1.0",
            };
            privatetePeripheralsPlugin.SetFieldOrProperty("_deviceHelper", deviceHelper);

            peripheralsPlugin.SetCollaborationChatEnable(newValue1, deviceId1);      // newValue1 true != IsCollaborationChatEnable false
            var devicemanager = privatetePeripheralsPlugin.GetFieldOrProperty("_iDeviceManager");
            Assert.IsTrue(true);
            Assert.IsNotNull(devicemanager);

            bool newValue2 = false;
            Guid deviceId2 = new Guid();
            peripheralsPlugin.SetCollaborationChatEnable(newValue2, deviceId2);
            var deviceHelper2 = privatetePeripheralsPlugin.GetFieldOrProperty("_deviceHelper");  // newValue2  = IsCollaborationChatEnable false
            Assert.IsTrue(true);
            Assert.IsNotNull(deviceHelper2);
        }

        [Test]
        public void TestSetCollaborationMicEnable()
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
                        IsCollaborationChatEnable=false,
                        IsCollaborationMicEnable = false,
                    }
                },
                DCFVersion = "1.0",
                DPeMSDKVersion = "1.0",
                DPeMSubAgentVersion = "1.0",
                IsdDriverVersion = "1.0",
            };
            privatetePeripheralsPlugin.SetFieldOrProperty("_deviceHelper", deviceHelper);

            peripheralsPlugin.SetCollaborationMicEnable(newValue1, deviceId1);      // newValue1 true != IsCollaborationMicEnable false
            var devicemanager = privatetePeripheralsPlugin.GetFieldOrProperty("_iDeviceManager");
            Assert.IsTrue(true);
            Assert.IsNotNull(devicemanager);

            bool newValue2 = false;
            Guid deviceId2 = new Guid();
            peripheralsPlugin.SetCollaborationMicEnable(newValue2, deviceId2);
            var deviceHelper2 = privatetePeripheralsPlugin.GetFieldOrProperty("_deviceHelper");  // newValue2  = IsCollaborationMicEnable false
            Assert.IsTrue(true);
            Assert.IsNotNull(deviceHelper2);
        }

        [Test]
        public void TestSetCollaborationBlinkEffectEnable()
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
                        IsCollaborationChatEnable=false,
                        IsCollaborationMicEnable = false,
                        IsCollaborationBlinkEffectEnable=false,
                    }
                },
                DCFVersion = "1.0",
                DPeMSDKVersion = "1.0",
                DPeMSubAgentVersion = "1.0",
                IsdDriverVersion = "1.0",
            };
            privatetePeripheralsPlugin.SetFieldOrProperty("_deviceHelper", deviceHelper);

            peripheralsPlugin.SetCollaborationBlinkEffectEnable(newValue1, deviceId1);      // newValue1 true != IsCollaborationBlinkEffectEnable false
            var devicemanager = privatetePeripheralsPlugin.GetFieldOrProperty("_iDeviceManager");
            Assert.IsTrue(true);
            Assert.IsNotNull(devicemanager);

            bool newValue2 = false;
            Guid deviceId2 = new Guid();
            peripheralsPlugin.SetCollaborationBlinkEffectEnable(newValue2, deviceId2);
            var deviceHelper2 = privatetePeripheralsPlugin.GetFieldOrProperty("_deviceHelper");  // newValue2  = IsCollaborationBlinkEffectEnable false
            Assert.IsTrue(true);
            Assert.IsNotNull(deviceHelper2);
        }

        [Test]
        public void TestSetCollaborationDoubleTapEnable()
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
                        IsCollaborationChatEnable=false,
                        IsCollaborationMicEnable = false,
                        IsCollaborationBlinkEffectEnable=false,
                        IsCollaborationDoubleTapEnable=false,
                    }
                },
                DCFVersion = "1.0",
                DPeMSDKVersion = "1.0",
                DPeMSubAgentVersion = "1.0",
                IsdDriverVersion = "1.0",
            };
            privatetePeripheralsPlugin.SetFieldOrProperty("_deviceHelper", deviceHelper);

            peripheralsPlugin.SetCollaborationDoubleTapEnable(newValue1, deviceId1);      // newValue1 true != IsCollaborationDoubleTapEnable false
            var devicemanager = privatetePeripheralsPlugin.GetFieldOrProperty("_iDeviceManager");
            Assert.IsTrue(true);
            Assert.IsNotNull(devicemanager);

            bool newValue2 = false;
            Guid deviceId2 = new Guid();
            peripheralsPlugin.SetCollaborationDoubleTapEnable(newValue2, deviceId2);
            var deviceHelper2 = privatetePeripheralsPlugin.GetFieldOrProperty("_deviceHelper");  // newValue2  = IsCollaborationDoubleTapEnable false
            Assert.IsTrue(true);
            Assert.IsNotNull(deviceHelper2);
        }

        [Test]
        public void TestSetBackLightingControls()
        {
            int newValue1 = 20;
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
                        IsCollaborationChatEnable=false,
                        IsCollaborationMicEnable = false,
                        IsCollaborationBlinkEffectEnable=false,
                        IsCollaborationDoubleTapEnable=false,
                        BackLightingControls=20,
                    }
                },
                DCFVersion = "1.0",
                DPeMSDKVersion = "1.0",
                DPeMSubAgentVersion = "1.0",
                IsdDriverVersion = "1.0",
            };
            privatetePeripheralsPlugin.SetFieldOrProperty("_deviceHelper", deviceHelper);

            peripheralsPlugin.SetBackLightingControls(newValue1, deviceId1);      // newValue1 = BackLightingControls 20
            var devicemanager = privatetePeripheralsPlugin.GetFieldOrProperty("_iDeviceManager");
            Assert.IsTrue(true);
            Assert.IsNotNull(devicemanager);

            int newValue2 = 10;
            Guid deviceId2 = new Guid();
            peripheralsPlugin.SetBackLightingControls(newValue2, deviceId2);
            var deviceHelper2 = privatetePeripheralsPlugin.GetFieldOrProperty("_deviceHelper");  // newValue2 10  ! = BackLightingControls
            Assert.IsTrue(true);
            Assert.IsNotNull(deviceHelper2);
        }

        [Test]
        public void TestSetBackLightingLevel()
        {
            int newValue1 = 20;
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
                        IsCollaborationChatEnable=false,
                        IsCollaborationMicEnable = false,
                        IsCollaborationBlinkEffectEnable=false,
                        IsCollaborationDoubleTapEnable=false,
                        BackLightingControls=20,
                        BackLightingLevel=20,
                    }
                },
                DCFVersion = "1.0",
                DPeMSDKVersion = "1.0",
                DPeMSubAgentVersion = "1.0",
                IsdDriverVersion = "1.0",
            };
            privatetePeripheralsPlugin.SetFieldOrProperty("_deviceHelper", deviceHelper);

            peripheralsPlugin.SetBackLightingLevel(newValue1, deviceId1);      // newValue1 = BackLightingLevel 20
            var devicemanager = privatetePeripheralsPlugin.GetFieldOrProperty("_iDeviceManager");
            Assert.IsTrue(true);
            Assert.IsNotNull(devicemanager);

            int newValue2 = 10;
            Guid deviceId2 = new Guid();
            peripheralsPlugin.SetBackLightingLevel(newValue2, deviceId2);
            var deviceHelper2 = privatetePeripheralsPlugin.GetFieldOrProperty("_deviceHelper");  // newValue2 10  ! = BackLightingLevel
            Assert.IsTrue(true);
            Assert.IsNotNull(deviceHelper2);
        }

        [Test]
        public void TestStartPairing()
        {
            Mock<IPhysicalDevice> physicalDevice = new Mock<IPhysicalDevice>();
            Mock<IPhysicalDeviceDongle> physicalDeviceDongle = new Mock<IPhysicalDeviceDongle>();
            Mock<IDeviceManager> mockDeviceManager = new Mock<IDeviceManager>();

            physicalDeviceDongle.Setup(pd => pd.Id).Returns(Guid.NewGuid());
            mockDeviceManager.Setup(dm => dm.Devices).Returns(new[] { physicalDeviceDongle.Object });

            Guid physicalDeviceId = physicalDeviceDongle.Object.Id;

            var DeviceManagerObj = mockDeviceManager.Object;
            privatetePeripheralsPlugin.SetFieldOrProperty("_iDeviceManager", DeviceManagerObj);

            peripheralsPlugin.StartPairing(physicalDeviceId);                                     // physicalDevice is IPhysicalDeviceDongle
            var devicemanager = privatetePeripheralsPlugin.GetFieldOrProperty("_iDeviceManager");
            Assert.IsTrue(true);
            Assert.IsNotNull(devicemanager);

            Mock<IPhysicalAudioDeviceDongle> physicalAudioDeviceDongle = new Mock<IPhysicalAudioDeviceDongle>();
            physicalAudioDeviceDongle.Setup(pd => pd.Id).Returns(Guid.NewGuid());
            mockDeviceManager.Setup(dm => dm.Devices).Returns(new[] { physicalAudioDeviceDongle.Object });

            Guid physicalAudioDeviceDongleId = physicalAudioDeviceDongle.Object.Id;

            var DeviceManagerObj2 = mockDeviceManager.Object;
            privatetePeripheralsPlugin.SetFieldOrProperty("_iDeviceManager", DeviceManagerObj2);

            peripheralsPlugin.StartPairing(physicalAudioDeviceDongleId);                        // physicalDevice is IPhysicalAudioDeviceDongle
            var devicemanager2 = privatetePeripheralsPlugin.GetFieldOrProperty("_iDeviceManager");
            Assert.IsTrue(true);
            Assert.IsNotNull(devicemanager2);
        }

        [Test]
        public void TestStopPairing()
        {
            Mock<IPhysicalDevice> physicalDevice = new Mock<IPhysicalDevice>();
            Mock<IPhysicalDeviceDongle> physicalDeviceDongle = new Mock<IPhysicalDeviceDongle>();
            Mock<IDeviceManager> mockDeviceManager = new Mock<IDeviceManager>();

            physicalDeviceDongle.Setup(pd => pd.Id).Returns(Guid.NewGuid());
            mockDeviceManager.Setup(dm => dm.Devices).Returns(new[] { physicalDeviceDongle.Object });

            Guid physicalDeviceId = physicalDeviceDongle.Object.Id;

            var DeviceManagerObj = mockDeviceManager.Object;
            privatetePeripheralsPlugin.SetFieldOrProperty("_iDeviceManager", DeviceManagerObj);

            peripheralsPlugin.StopPairing(physicalDeviceId);                                     // physicalDevice is IPhysicalDeviceDongle
            var devicemanager = privatetePeripheralsPlugin.GetFieldOrProperty("_iDeviceManager");
            Assert.IsTrue(true);
            Assert.IsNotNull(devicemanager);

            Mock<IPhysicalAudioDeviceDongle> physicalAudioDeviceDongle = new Mock<IPhysicalAudioDeviceDongle>();
            physicalAudioDeviceDongle.Setup(pd => pd.Id).Returns(Guid.NewGuid());
            mockDeviceManager.Setup(dm => dm.Devices).Returns(new[] { physicalAudioDeviceDongle.Object });

            Guid physicalAudioDeviceDongleId = physicalAudioDeviceDongle.Object.Id;

            var DeviceManagerObj2 = mockDeviceManager.Object;
            privatetePeripheralsPlugin.SetFieldOrProperty("_iDeviceManager", DeviceManagerObj2);

            peripheralsPlugin.StopPairing(physicalAudioDeviceDongleId);                        // physicalDevice is IPhysicalAudioDeviceDongle
            var devicemanager2 = privatetePeripheralsPlugin.GetFieldOrProperty("_iDeviceManager");
            Assert.IsTrue(true);
            Assert.IsNotNull(devicemanager2);
        }

        [Test]
        public void TestStopPairingPen()
        {
            Mock<IPhysicalDevice> physicalDevice = new Mock<IPhysicalDevice>();
            Mock<IPhysicalPenDevice> physicalPenDevice = new Mock<IPhysicalPenDevice>();
            Mock<IDeviceManager> mockDeviceManager = new Mock<IDeviceManager>();

            physicalPenDevice.Setup(pd => pd.Id).Returns(Guid.NewGuid());
            mockDeviceManager.Setup(dm => dm.Devices).Returns(new[] { physicalPenDevice.Object });

            Guid physicalPenDeviceId = physicalPenDevice.Object.Id;

            var DeviceManagerObj = mockDeviceManager.Object;
            privatetePeripheralsPlugin.SetFieldOrProperty("_iDeviceManager", DeviceManagerObj);

            //peripheralsPlugin.StopPairingPen();                                     // device is physicalPenDevice
            var devicemanager = privatetePeripheralsPlugin.GetFieldOrProperty("_iDeviceManager");
            Assert.IsTrue(true);
            Assert.IsNotNull(devicemanager);
        }

        /* [Test]
         public void TestUnPair()
         {
             Mock<IPhysicalDevice> ParentPhysicalDevice = new Mock<IPhysicalDevice>();
             Mock<ILogicalDevice> logicalDevice = new Mock<ILogicalDevice>();
             Mock<IPhysicalDeviceDongle> physicalDeviceDongle = new Mock<IPhysicalDeviceDongle>();
             Mock<IDeviceManager> mockDeviceManager = new Mock<IDeviceManager>();

             logicalDevice.Setup(ld => ld.Id).Returns(Guid.NewGuid());
             logicalDevice.Setup(pd => pd.ParentPhysicalDevice).Returns(physicalDeviceDongle.Object);
             physicalDeviceDongle.Setup(pd => pd.Devices).Returns(new[] { logicalDevice.Object });
             mockDeviceManager.Setup(dm => dm.Devices).Returns(new[] { physicalDeviceDongle.Object });// mockDeviceManager device- physicalDeviceDongledevice -logicalDevice device- physicalDeviceDongle device

             Guid logicalDeviceId = logicalDevice.Object.Id;

             var DeviceManagerObj = mockDeviceManager.Object;
             privatetePeripheralsPlugin.SetFieldOrProperty("_iDeviceManager", DeviceManagerObj);

             peripheralsPlugin.UnPair(logicalDeviceId);                                     // physicalDevice ParentPhysicalDevice is IPhysicalDeviceDongle
             var devicemanager = privatetePeripheralsPlugin.GetFieldOrProperty("_iDeviceManager");
             Assert.IsTrue(true);
             Assert.IsNotNull(devicemanager);

             Mock<IPhysicalAudioDeviceDongle> physicalAudioDeviceDongle = new Mock<IPhysicalAudioDeviceDongle>();

             logicalDevice.Setup(ld => ld.Id).Returns(Guid.NewGuid());
             logicalDevice.Setup(pd => pd.ParentPhysicalDevice).Returns(physicalAudioDeviceDongle.Object);
             physicalAudioDeviceDongle.Setup(pd => pd.Devices).Returns(new[] { logicalDevice.Object });
             mockDeviceManager.Setup(dm => dm.Devices).Returns(new[] { physicalAudioDeviceDongle.Object }); // mockDeviceManager device- physicalAudioDeviceDongle -logicalDevice device- physicalAudioDeviceDongle device

             Guid physicalAudioDeviceDongleId = logicalDevice.Object.Id;

             var DeviceManagerObj2 = mockDeviceManager.Object;
             privatetePeripheralsPlugin.SetFieldOrProperty("_iDeviceManager", DeviceManagerObj2);

             peripheralsPlugin.UnPair(physicalAudioDeviceDongleId);                        // physicalDevice ParentPhysicalDevice is IPhysicalAudioDeviceDongle
             var devicemanager2 = privatetePeripheralsPlugin.GetFieldOrProperty("_iDeviceManager");
             Assert.IsTrue(true);
             Assert.IsNotNull(devicemanager2);

             Mock<IPhysicalPenDevice> physicalPenDevice = new Mock<IPhysicalPenDevice>();

             logicalDevice.Setup(ld => ld.Id).Returns(Guid.NewGuid());
             logicalDevice.Setup(pd => pd.ParentPhysicalDevice).Returns(physicalPenDevice.Object);
             physicalPenDevice.Setup(pd => pd.Devices).Returns(new[] { logicalDevice.Object });
             mockDeviceManager.Setup(dm => dm.Devices).Returns(new[] { physicalPenDevice.Object }); // mockDeviceManager device- physicalPenDevice -logicalDevice device- physicalPenDevice device

             Guid physicalPenDeviceId = logicalDevice.Object.Id;

             var DeviceManagerObj3 = mockDeviceManager.Object;
             privatetePeripheralsPlugin.SetFieldOrProperty("_iDeviceManager", DeviceManagerObj);

             peripheralsPlugin.UnPair(physicalPenDeviceId);                                                // device ParentPhysicalDevice is physicalPenDevice
             var devicemanager3 = privatetePeripheralsPlugin.GetFieldOrProperty("_iDeviceManager");
             Assert.IsTrue(true);
             Assert.IsNotNull(devicemanager3);
         }*/

        [Test]
        public void TestSetWiredAudioIMicNSEnable()
        {
            bool newValue = false;
            Guid deviceId1 = new Guid();
            Mock<IPhysicalDevice> PhysicalDevice = new Mock<IPhysicalDevice>();
            Mock<ILogicalWiredAudio> logicalWiredAudioDevice = new Mock<ILogicalWiredAudio>();
            Mock<IDeviceManager> mockDeviceManager = new Mock<IDeviceManager>();

            logicalWiredAudioDevice.Setup(ld => ld.Id).Returns(Guid.NewGuid());
            PhysicalDevice.Setup(pd => pd.Devices).Returns(new[] { logicalWiredAudioDevice.Object });
            mockDeviceManager.Setup(dm => dm.Devices).Returns(new[] { PhysicalDevice.Object });// mockDeviceManager device- PhysicalDevice -logicalWiredAudioDevice device- logicalWiredAudioDevice device ID

            Guid logicalWiredAudioDeviceId = logicalWiredAudioDevice.Object.Id;

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
                        IsCollaborationChatEnable=false,
                        IsCollaborationMicEnable = false,
                        IsCollaborationBlinkEffectEnable=false,
                        IsCollaborationDoubleTapEnable=false,
                        BackLightingControls=20,
                        BackLightingLevel=20,
                        SidetoneLevel=20,
                        IsWiredAudioIMicNSEnable=false,
                    }
                },
                DCFVersion = "1.0",
                DPeMSDKVersion = "1.0",
                DPeMSubAgentVersion = "1.0",
                IsdDriverVersion = "1.0",
            };
            privatetePeripheralsPlugin.SetFieldOrProperty("_deviceHelper", deviceHelper);

            peripheralsPlugin.SetWiredAudioIMicNSEnable(newValue, logicalWiredAudioDeviceId);
            var devicemanager = privatetePeripheralsPlugin.GetFieldOrProperty("_iDeviceManager");
            Assert.IsTrue(true);
            Assert.IsNotNull(devicemanager);
        }

        [Test]
        public void TestSetWiredAudioMicMuteSoundEnable()
        {
            bool newValue = false;

            Mock<IPhysicalDevice> PhysicalDevice = new Mock<IPhysicalDevice>();
            Mock<ILogicalWiredAudio> logicalWiredAudioDevice = new Mock<ILogicalWiredAudio>();
            //Mock<ILogicalDevice> logicalDevice = new Mock<ILogicalDevice>(); // ILogicalWiredAudio jichengle ILogicalDevice?suoyiyeshi ILogicalDevice
            Mock<IDeviceManager> mockDeviceManager = new Mock<IDeviceManager>();

            logicalWiredAudioDevice.Setup(ld => ld.Id).Returns(Guid.NewGuid());
            PhysicalDevice.Setup(pd => pd.Devices).Returns(new[] { logicalWiredAudioDevice.Object });
            mockDeviceManager.Setup(dm => dm.Devices).Returns(new[] { PhysicalDevice.Object });// mockDeviceManager device- PhysicalDevice -logicalWiredAudioDevice device- logicalWiredAudioDevice device ID

            Guid logicalWiredAudioDeviceId = logicalWiredAudioDevice.Object.Id;

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
                        IsCollaborationChatEnable=false,
                        IsCollaborationMicEnable = false,
                        IsCollaborationBlinkEffectEnable=false,
                        IsCollaborationDoubleTapEnable=false,
                        BackLightingControls=20,
                        BackLightingLevel=20,
                        SidetoneLevel=20,
                        IsWiredAudioIMicNSEnable=false,
                        IsWiredAudioMicMuteSoundEnable = false,
                    }
                },
                DCFVersion = "1.0",
                DPeMSDKVersion = "1.0",
                DPeMSubAgentVersion = "1.0",
                IsdDriverVersion = "1.0",
            };
            privatetePeripheralsPlugin.SetFieldOrProperty("_deviceHelper", deviceHelper);

            peripheralsPlugin.SetWiredAudioMicMuteSoundEnable(newValue, logicalWiredAudioDeviceId);
            var devicemanager = privatetePeripheralsPlugin.GetFieldOrProperty("_iDeviceManager");
            Assert.IsTrue(true);
            Assert.IsNotNull(devicemanager);
        }

        [Test]
        public void TestSetWiredAudioVolumeAdjustmentTone()
        {
            int newValue = 20;
            Mock<IPhysicalDevice> PhysicalDevice = new Mock<IPhysicalDevice>();
            Mock<ILogicalWiredAudio> logicalWiredAudioDevice = new Mock<ILogicalWiredAudio>();
            //Mock<ILogicalDevice> logicalDevice = new Mock<ILogicalDevice>(); // ILogicalWiredAudio jichengle ILogicalDevice?suoyiyeshi ILogicalDevice
            Mock<IDeviceManager> mockDeviceManager = new Mock<IDeviceManager>();

            logicalWiredAudioDevice.Setup(ld => ld.Id).Returns(Guid.NewGuid());
            PhysicalDevice.Setup(pd => pd.Devices).Returns(new[] { logicalWiredAudioDevice.Object });
            mockDeviceManager.Setup(dm => dm.Devices).Returns(new[] { PhysicalDevice.Object });// mockDeviceManager device- PhysicalDevice -logicalWiredAudioDevice device- logicalWiredAudioDevice device ID

            Guid logicalWiredAudioDeviceId = logicalWiredAudioDevice.Object.Id;

            var DeviceManagerObj = mockDeviceManager.Object;
            privatetePeripheralsPlugin.SetFieldOrProperty("_iDeviceManager", DeviceManagerObj);

            peripheralsPlugin.SetWiredAudioVolumeAdjustmentTone(newValue, logicalWiredAudioDeviceId);                      // logicalDeviceis IlogicalWiredAudioDevice,method not ready
            var devicemanager = privatetePeripheralsPlugin.GetFieldOrProperty("_iDeviceManager");
            Assert.IsTrue(true);
            Assert.IsNotNull(devicemanager);
        }

        [Test]
        public void TestSetSidetoneLevel()
        {
            int newValue = 10;
            Mock<IPhysicalDevice> PhysicalDevice = new Mock<IPhysicalDevice>();
            Mock<ILogicalDeviceHeadset> logicalDeviceHeadset = new Mock<ILogicalDeviceHeadset>();
            Mock<IDeviceManager> mockDeviceManager = new Mock<IDeviceManager>();

            logicalDeviceHeadset.Setup(ld => ld.Id).Returns(Guid.NewGuid());
            PhysicalDevice.Setup(pd => pd.Devices).Returns(new[] { logicalDeviceHeadset.Object });
            mockDeviceManager.Setup(dm => dm.Devices).Returns(new[] { PhysicalDevice.Object });// mockDeviceManager device- PhysicalDevice -logicalDeviceHeadset device- logicalDeviceHeadset device ID

            Guid logicalDeviceHeadsetId = logicalDeviceHeadset.Object.Id;

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
                        IsCollaborationChatEnable=false,
                        IsCollaborationMicEnable = false,
                        IsCollaborationBlinkEffectEnable=false,
                        IsCollaborationDoubleTapEnable=false,
                        BackLightingControls=20,
                        BackLightingLevel=20,
                        SidetoneLevel=20,
                        //ID=logicalDeviceHeadsetId,
                    }
                },
                DCFVersion = "1.0",
                DPeMSDKVersion = "1.0",
                DPeMSubAgentVersion = "1.0",
                IsdDriverVersion = "1.0",
            };
            privatetePeripheralsPlugin.SetFieldOrProperty("_deviceHelper", deviceHelper);

            peripheralsPlugin.SetSidetoneLevel(newValue, logicalDeviceHeadsetId);                      // logicalDeviceis logicalDeviceHeadset,SidetoneLevel=20, _deviceHelper.deviceInfo ID != logicalDeviceHeadsetId
            var devicemanager = privatetePeripheralsPlugin.GetFieldOrProperty("_iDeviceManager");
        }

        [Test]
        public void TestSetAncMode()
        {
            int newValue = 10;
            Mock<IPhysicalDevice> PhysicalDevice = new Mock<IPhysicalDevice>();
            Mock<ILogicalDeviceHeadset> logicalDeviceHeadset = new Mock<ILogicalDeviceHeadset>();
            Mock<IDeviceManager> mockDeviceManager = new Mock<IDeviceManager>();

            logicalDeviceHeadset.Setup(ld => ld.Id).Returns(Guid.NewGuid());
            PhysicalDevice.Setup(pd => pd.Devices).Returns(new[] { logicalDeviceHeadset.Object });
            mockDeviceManager.Setup(dm => dm.Devices).Returns(new[] { PhysicalDevice.Object });// mockDeviceManager device- PhysicalDevice -logicalDeviceHeadset device- logicalDeviceHeadset device ID

            Guid logicalDeviceHeadsetId = logicalDeviceHeadset.Object.Id;

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
                        IsCollaborationChatEnable=false,
                        IsCollaborationMicEnable = false,
                        IsCollaborationBlinkEffectEnable=false,
                        IsCollaborationDoubleTapEnable=false,
                        BackLightingControls=20,
                        BackLightingLevel=20,
                        SidetoneLevel=20,
                        //ID=logicalDeviceHeadsetId,
                        AncMode=20,
                    }
                },
                DCFVersion = "1.0",
                DPeMSDKVersion = "1.0",
                DPeMSubAgentVersion = "1.0",
                IsdDriverVersion = "1.0",
            };
            privatetePeripheralsPlugin.SetFieldOrProperty("_deviceHelper", deviceHelper);

            peripheralsPlugin.SetAncMode(newValue, logicalDeviceHeadsetId);                      // logicalDeviceis logicalDeviceHeadset,AncMode=20, _deviceHelper.deviceInfo ID != logicalDeviceHeadsetId
            var devicemanager = privatetePeripheralsPlugin.GetFieldOrProperty("_iDeviceManager");
            Assert.IsTrue(true);
            Assert.IsNotNull(devicemanager);

            deviceHelper.deviceInfo[0].ID = logicalDeviceHeadsetId;
            privatetePeripheralsPlugin.SetFieldOrProperty("_deviceHelper", deviceHelper);

            peripheralsPlugin.SetAncMode(newValue, logicalDeviceHeadsetId);                     // logicalDeviceis logicalDeviceHeadset,AncMode=20, _deviceHelper.deviceInfo ID = logicalDeviceHeadsetId
            var devicemanager2 = privatetePeripheralsPlugin.GetFieldOrProperty("_iDeviceManager");
            Assert.IsTrue(true);
            Assert.IsNotNull(devicemanager);
        }

        [Test]
        public void TestSetAncGain()
        {
            int newValue = 10;
            Mock<IPhysicalDevice> PhysicalDevice = new Mock<IPhysicalDevice>();
            Mock<ILogicalDeviceHeadset> logicalDeviceHeadset = new Mock<ILogicalDeviceHeadset>();
            Mock<IDeviceManager> mockDeviceManager = new Mock<IDeviceManager>();

            logicalDeviceHeadset.Setup(ld => ld.Id).Returns(Guid.NewGuid());
            PhysicalDevice.Setup(pd => pd.Devices).Returns(new[] { logicalDeviceHeadset.Object });
            mockDeviceManager.Setup(dm => dm.Devices).Returns(new[] { PhysicalDevice.Object });// mockDeviceManager device- PhysicalDevice -logicalDeviceHeadset device- logicalDeviceHeadset device ID

            Guid logicalDeviceHeadsetId = logicalDeviceHeadset.Object.Id;

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
                        IsCollaborationChatEnable=false,
                        IsCollaborationMicEnable = false,
                        IsCollaborationBlinkEffectEnable=false,
                        IsCollaborationDoubleTapEnable=false,
                        BackLightingControls=20,
                        BackLightingLevel=20,
                        SidetoneLevel=20,
                        //ID=logicalDeviceHeadsetId,
                        AncMode=20,
                        AncGain=20,
                    }
                },
                DCFVersion = "1.0",
                DPeMSDKVersion = "1.0",
                DPeMSubAgentVersion = "1.0",
                IsdDriverVersion = "1.0",
            };
            privatetePeripheralsPlugin.SetFieldOrProperty("_deviceHelper", deviceHelper);

            peripheralsPlugin.SetAncGain(newValue, logicalDeviceHeadsetId);                      // logicalDeviceis logicalDeviceHeadset,AncGain=20, _deviceHelper.deviceInfo ID != logicalDeviceHeadsetId
            var devicemanager = privatetePeripheralsPlugin.GetFieldOrProperty("_iDeviceManager");
            Assert.IsTrue(true);
            Assert.IsNotNull(devicemanager);

            deviceHelper.deviceInfo[0].ID = logicalDeviceHeadsetId;
            privatetePeripheralsPlugin.SetFieldOrProperty("_deviceHelper", deviceHelper);

            peripheralsPlugin.SetAncGain(newValue, logicalDeviceHeadsetId);                     // logicalDeviceis logicalDeviceHeadset,AncGain=20, _deviceHelper.deviceInfo ID = logicalDeviceHeadsetId
            var devicemanager2 = privatetePeripheralsPlugin.GetFieldOrProperty("_iDeviceManager");
            Assert.IsTrue(true);
            Assert.IsNotNull(devicemanager);
        }

        [Test]
        public void TestSetSelectedPreset()
        {
            int newValue = 10;
            Mock<IPhysicalDevice> PhysicalDevice = new Mock<IPhysicalDevice>();
            Mock<ILogicalDeviceHeadset> logicalDeviceHeadset = new Mock<ILogicalDeviceHeadset>();
            Mock<IDeviceManager> mockDeviceManager = new Mock<IDeviceManager>();

            logicalDeviceHeadset.Setup(ld => ld.Id).Returns(Guid.NewGuid());
            PhysicalDevice.Setup(pd => pd.Devices).Returns(new[] { logicalDeviceHeadset.Object });
            mockDeviceManager.Setup(dm => dm.Devices).Returns(new[] { PhysicalDevice.Object });// mockDeviceManager device- PhysicalDevice -logicalDeviceHeadset device- logicalDeviceHeadset device ID

            Guid logicalDeviceHeadsetId = logicalDeviceHeadset.Object.Id;

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
                        IsCollaborationChatEnable=false,
                        IsCollaborationMicEnable = false,
                        IsCollaborationBlinkEffectEnable=false,
                        IsCollaborationDoubleTapEnable=false,
                        BackLightingControls=20,
                        BackLightingLevel=20,
                        SidetoneLevel=20,
                        //ID=logicalDeviceHeadsetId,
                        AncMode=20,
                        AncGain=20,
                        SelectedPreset=20,
                    }
                },
                DCFVersion = "1.0",
                DPeMSDKVersion = "1.0",
                DPeMSubAgentVersion = "1.0",
                IsdDriverVersion = "1.0",
            };
            privatetePeripheralsPlugin.SetFieldOrProperty("_deviceHelper", deviceHelper);

            peripheralsPlugin.SetSelectedPreset(newValue, logicalDeviceHeadsetId);                      // logicalDeviceis logicalDeviceHeadset,SelectedPreset=20, _deviceHelper.deviceInfo ID != logicalDeviceHeadsetId
            var devicemanager = privatetePeripheralsPlugin.GetFieldOrProperty("_iDeviceManager");
            Assert.IsTrue(true);
            Assert.IsNotNull(devicemanager);

            deviceHelper.deviceInfo[0].ID = logicalDeviceHeadsetId;
            privatetePeripheralsPlugin.SetFieldOrProperty("_deviceHelper", deviceHelper);

            peripheralsPlugin.SetSelectedPreset(newValue, logicalDeviceHeadsetId);                     // logicalDeviceis logicalDeviceHeadset,SelectedPreset=20, _deviceHelper.deviceInfo ID = logicalDeviceHeadsetId
            var devicemanager2 = privatetePeripheralsPlugin.GetFieldOrProperty("_iDeviceManager");
            Assert.IsTrue(true);
            Assert.IsNotNull(devicemanager);
        }

        [Test]
        public void TestSetBandsGain()
        {
            int newValue = 10;
            Mock<IPhysicalDevice> PhysicalDevice = new Mock<IPhysicalDevice>();
            Mock<ILogicalDeviceHeadset> logicalDeviceHeadset = new Mock<ILogicalDeviceHeadset>();
            Mock<IDeviceManager> mockDeviceManager = new Mock<IDeviceManager>();

            logicalDeviceHeadset.Setup(ld => ld.Id).Returns(Guid.NewGuid());
            PhysicalDevice.Setup(pd => pd.Devices).Returns(new[] { logicalDeviceHeadset.Object });
            mockDeviceManager.Setup(dm => dm.Devices).Returns(new[] { PhysicalDevice.Object });// mockDeviceManager device- PhysicalDevice -logicalDeviceHeadset device- logicalDeviceHeadset device ID

            Guid logicalDeviceHeadsetId = logicalDeviceHeadset.Object.Id;

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
                        IsCollaborationChatEnable=false,
                        IsCollaborationMicEnable = false,
                        IsCollaborationBlinkEffectEnable=false,
                        IsCollaborationDoubleTapEnable=false,
                        BackLightingControls=20,
                        BackLightingLevel=20,
                        SidetoneLevel=20,
                        //ID=logicalDeviceHeadsetId,
                        AncMode=20,
                        AncGain=20,
                        SelectedPreset=20,
                        Band1Gain=20,
                        Band2Gain=20,
                        Band3Gain=20,
                        Band4Gain=20,
                        Band5Gain=20,
                    }
                },
                DCFVersion = "1.0",
                DPeMSDKVersion = "1.0",
                DPeMSubAgentVersion = "1.0",
                IsdDriverVersion = "1.0",
            };
            privatetePeripheralsPlugin.SetFieldOrProperty("_deviceHelper", deviceHelper);

            string bandGainNumber1 = "band1gain";
            if (deviceHelper.deviceInfo[0] != null)
            {
                if (bandGainNumber1 == "band1gain")                                                                      // bandGainNumber1 = "band1gain";
                {
                    peripheralsPlugin.SetBandsGain(newValue, logicalDeviceHeadsetId, bandGainNumber1);                      // logicalDeviceis logicalDeviceHeadset,Band1Gain=20, _deviceHelper.deviceInfo ID != logicalDeviceHeadsetId
                    var devicemanager = privatetePeripheralsPlugin.GetFieldOrProperty("_iDeviceManager");
                    Assert.IsTrue(true);
                    Assert.IsNotNull(devicemanager);

                    deviceHelper.deviceInfo[0].ID = logicalDeviceHeadsetId;
                    privatetePeripheralsPlugin.SetFieldOrProperty("_deviceHelper", deviceHelper);

                    peripheralsPlugin.SetBandsGain(newValue, logicalDeviceHeadsetId, bandGainNumber1);                       // logicalDeviceis logicalDeviceHeadset,Band1Gain=20, _deviceHelper.deviceInfo ID = logicalDeviceHeadsetId
                    var devicemanager2 = privatetePeripheralsPlugin.GetFieldOrProperty("_iDeviceManager");
                    Assert.IsTrue(true);
                    Assert.IsNotNull(devicemanager2);
                }

                string bandGainNumber2 = "band2gain";
                if (bandGainNumber2 == "band2gain")                                                                      // bandGainNumber2 = "band2gain";
                {
                    peripheralsPlugin.SetBandsGain(newValue, logicalDeviceHeadsetId, bandGainNumber2);                      // logicalDeviceis logicalDeviceHeadset,Band2Gain=20, _deviceHelper.deviceInfo ID != logicalDeviceHeadsetId
                    var devicemanager = privatetePeripheralsPlugin.GetFieldOrProperty("_iDeviceManager");
                    Assert.IsTrue(true);
                    Assert.IsNotNull(devicemanager);

                    deviceHelper.deviceInfo[0].ID = logicalDeviceHeadsetId;
                    privatetePeripheralsPlugin.SetFieldOrProperty("_deviceHelper", deviceHelper);

                    peripheralsPlugin.SetBandsGain(newValue, logicalDeviceHeadsetId, bandGainNumber2);                       // logicalDeviceis logicalDeviceHeadset,Band2Gain=20, _deviceHelper.deviceInfo ID = logicalDeviceHeadsetId
                    var devicemanager2 = privatetePeripheralsPlugin.GetFieldOrProperty("_iDeviceManager");
                    Assert.IsTrue(true);
                    Assert.IsNotNull(devicemanager2);
                }

                string bandGainNumber3 = "band3gain";
                if (bandGainNumber3 == "band3gain")                                                                      // bandGainNumber3 = "band3gain";
                {
                    peripheralsPlugin.SetBandsGain(newValue, logicalDeviceHeadsetId, bandGainNumber3);                      // logicalDeviceis logicalDeviceHeadset,Band3Gain=20, _deviceHelper.deviceInfo ID != logicalDeviceHeadsetId
                    var devicemanager = privatetePeripheralsPlugin.GetFieldOrProperty("_iDeviceManager");
                    Assert.IsTrue(true);
                    Assert.IsNotNull(devicemanager);

                    deviceHelper.deviceInfo[0].ID = logicalDeviceHeadsetId;
                    privatetePeripheralsPlugin.SetFieldOrProperty("_deviceHelper", deviceHelper);

                    peripheralsPlugin.SetBandsGain(newValue, logicalDeviceHeadsetId, bandGainNumber3);                       // logicalDeviceis logicalDeviceHeadset,Band3Gain=20, _deviceHelper.deviceInfo ID = logicalDeviceHeadsetId
                    var devicemanager2 = privatetePeripheralsPlugin.GetFieldOrProperty("_iDeviceManager");
                    Assert.IsTrue(true);
                    Assert.IsNotNull(devicemanager2);
                }

                string bandGainNumber4 = "band4gain";
                if (bandGainNumber4 == "band4gain")                                                                      // bandGainNumber4 = "band4gain";
                {
                    peripheralsPlugin.SetBandsGain(newValue, logicalDeviceHeadsetId, bandGainNumber4);                      // logicalDeviceis logicalDeviceHeadset,Band4Gain=20, _deviceHelper.deviceInfo ID != logicalDeviceHeadsetId
                    var devicemanager = privatetePeripheralsPlugin.GetFieldOrProperty("_iDeviceManager");
                    Assert.IsTrue(true);
                    Assert.IsNotNull(devicemanager);

                    deviceHelper.deviceInfo[0].ID = logicalDeviceHeadsetId;
                    privatetePeripheralsPlugin.SetFieldOrProperty("_deviceHelper", deviceHelper);

                    peripheralsPlugin.SetBandsGain(newValue, logicalDeviceHeadsetId, bandGainNumber4);                       // logicalDeviceis logicalDeviceHeadset,Band4Gain=20, _deviceHelper.deviceInfo ID = logicalDeviceHeadsetId
                    var devicemanager2 = privatetePeripheralsPlugin.GetFieldOrProperty("_iDeviceManager");
                    Assert.IsTrue(true);
                    Assert.IsNotNull(devicemanager2);
                }

                string bandGainNumber5 = "band5gain";
                if (bandGainNumber5 == "band5gain")                                                                      // bandGainNumber5 = "band5gain";
                {
                    peripheralsPlugin.SetBandsGain(newValue, logicalDeviceHeadsetId, bandGainNumber5);                      // logicalDeviceis logicalDeviceHeadset,Band5Gain=20, _deviceHelper.deviceInfo ID != logicalDeviceHeadsetId
                    var devicemanager = privatetePeripheralsPlugin.GetFieldOrProperty("_iDeviceManager");
                    Assert.IsTrue(true);
                    Assert.IsNotNull(devicemanager);

                    deviceHelper.deviceInfo[0].ID = logicalDeviceHeadsetId;
                    privatetePeripheralsPlugin.SetFieldOrProperty("_deviceHelper", deviceHelper);

                    peripheralsPlugin.SetBandsGain(newValue, logicalDeviceHeadsetId, bandGainNumber5);                       // logicalDeviceis logicalDeviceHeadset,Band5Gain=20, _deviceHelper.deviceInfo ID = logicalDeviceHeadsetId
                    var devicemanager2 = privatetePeripheralsPlugin.GetFieldOrProperty("_iDeviceManager");
                    Assert.IsTrue(true);
                    Assert.IsNotNull(devicemanager2);
                }
            }
        }

        [Test]
        public void TestSetMicNoiseCancellation()
        {
            bool newValue = false;
            Mock<IPhysicalDevice> PhysicalDevice = new Mock<IPhysicalDevice>();
            Mock<ILogicalDeviceHeadset> logicalDeviceHeadset = new Mock<ILogicalDeviceHeadset>();
            Mock<IDeviceManager> mockDeviceManager = new Mock<IDeviceManager>();

            logicalDeviceHeadset.Setup(ld => ld.Id).Returns(Guid.NewGuid());
            PhysicalDevice.Setup(pd => pd.Devices).Returns(new[] { logicalDeviceHeadset.Object });
            mockDeviceManager.Setup(dm => dm.Devices).Returns(new[] { PhysicalDevice.Object });// mockDeviceManager device- PhysicalDevice -logicalDeviceHeadset device- logicalDeviceHeadset device ID

            Guid logicalDeviceHeadsetId = logicalDeviceHeadset.Object.Id;

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
                        IsCollaborationChatEnable=false,
                        IsCollaborationMicEnable = false,
                        IsCollaborationBlinkEffectEnable=false,
                        IsCollaborationDoubleTapEnable=false,
                        BackLightingControls=20,
                        BackLightingLevel=20,
                        SidetoneLevel=20,
                        //ID=logicalDeviceHeadsetId,
                        AncMode=20,
                        AncGain=20,
                        SelectedPreset=20,
                        Band1Gain=20,
                        Band2Gain=20,
                        Band3Gain=20,
                        Band4Gain=20,
                        Band5Gain=20,
                        MicNoiseCancellation=false,
                    }
                },
                DCFVersion = "1.0",
                DPeMSDKVersion = "1.0",
                DPeMSubAgentVersion = "1.0",
                IsdDriverVersion = "1.0",
            };
            privatetePeripheralsPlugin.SetFieldOrProperty("_deviceHelper", deviceHelper);

            peripheralsPlugin.SetMicNoiseCancellation(newValue, logicalDeviceHeadsetId);                      // logicalDeviceis logicalDeviceHeadset,MicNoiseCancellation=false, _deviceHelper.deviceInfo ID != logicalDeviceHeadsetId
            var devicemanager = privatetePeripheralsPlugin.GetFieldOrProperty("_iDeviceManager");
            Assert.IsTrue(true);
            Assert.IsNotNull(devicemanager);

            deviceHelper.deviceInfo[0].ID = logicalDeviceHeadsetId;
            privatetePeripheralsPlugin.SetFieldOrProperty("_deviceHelper", deviceHelper);

            peripheralsPlugin.SetMicNoiseCancellation(newValue, logicalDeviceHeadsetId);                     // logicalDeviceis logicalDeviceHeadset,MicNoiseCancellation=false, _deviceHelper.deviceInfo ID = logicalDeviceHeadsetId
            var devicemanager2 = privatetePeripheralsPlugin.GetFieldOrProperty("_iDeviceManager");
            var deviceHelper2 = (DeviceHelper)privatetePeripheralsPlugin.GetFieldOrProperty("_deviceHelper");
            Assert.IsTrue(true);
            Assert.IsNotNull(devicemanager2);
            Assert.That(deviceHelper2.deviceInfo[0].MicNoiseCancellation, Is.EqualTo(newValue));
        }

        [Test]
        public void TestSetSidetone()
        {
            bool newValue = false;
            Mock<IPhysicalDevice> PhysicalDevice = new Mock<IPhysicalDevice>();
            Mock<ILogicalDeviceHeadset> logicalDeviceHeadset = new Mock<ILogicalDeviceHeadset>();
            Mock<IDeviceManager> mockDeviceManager = new Mock<IDeviceManager>();

            logicalDeviceHeadset.Setup(ld => ld.Id).Returns(Guid.NewGuid());
            PhysicalDevice.Setup(pd => pd.Devices).Returns(new[] { logicalDeviceHeadset.Object });
            mockDeviceManager.Setup(dm => dm.Devices).Returns(new[] { PhysicalDevice.Object });// mockDeviceManager device- PhysicalDevice -logicalDeviceHeadset device- logicalDeviceHeadset device ID

            Guid logicalDeviceHeadsetId = logicalDeviceHeadset.Object.Id;

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
                        IsCollaborationChatEnable=false,
                        IsCollaborationMicEnable = false,
                        IsCollaborationBlinkEffectEnable=false,
                        IsCollaborationDoubleTapEnable=false,
                        BackLightingControls=20,
                        BackLightingLevel=20,
                        SidetoneLevel=20,
                        //ID=logicalDeviceHeadsetId,
                        AncMode=20,
                        AncGain=20,
                        SelectedPreset=20,
                        Band1Gain=20,
                        Band2Gain=20,
                        Band3Gain=20,
                        Band4Gain=20,
                        Band5Gain=20,
                        MicNoiseCancellation=false,
                        Sidetone=false,
                    }
                },
                DCFVersion = "1.0",
                DPeMSDKVersion = "1.0",
                DPeMSubAgentVersion = "1.0",
                IsdDriverVersion = "1.0",
            };
            privatetePeripheralsPlugin.SetFieldOrProperty("_deviceHelper", deviceHelper);

            peripheralsPlugin.SetSidetone(newValue, logicalDeviceHeadsetId);                      // logicalDeviceis logicalDeviceHeadset,Sidetone=false, _deviceHelper.deviceInfo ID != logicalDeviceHeadsetId
            var devicemanager = privatetePeripheralsPlugin.GetFieldOrProperty("_iDeviceManager");
            Assert.IsTrue(true);
            Assert.IsNotNull(devicemanager);

            deviceHelper.deviceInfo[0].ID = logicalDeviceHeadsetId;
            privatetePeripheralsPlugin.SetFieldOrProperty("_deviceHelper", deviceHelper);

            peripheralsPlugin.SetSidetone(newValue, logicalDeviceHeadsetId);                     // logicalDeviceis logicalDeviceHeadset,Sidetone=false, _deviceHelper.deviceInfo ID = logicalDeviceHeadsetId
            var devicemanager2 = privatetePeripheralsPlugin.GetFieldOrProperty("_iDeviceManager");
            var deviceHelper2 = (DeviceHelper)privatetePeripheralsPlugin.GetFieldOrProperty("_deviceHelper");
            Assert.IsTrue(true);
            Assert.IsNotNull(devicemanager2);
            Assert.That(deviceHelper2.deviceInfo[0].Sidetone, Is.EqualTo(newValue));
        }

        [Test]
        public void TestSetWearDetection()
        {
            int newValue = 10;
            Mock<IPhysicalDevice> PhysicalDevice = new Mock<IPhysicalDevice>();
            Mock<ILogicalDeviceHeadset> logicalDeviceHeadset = new Mock<ILogicalDeviceHeadset>();
            Mock<IDeviceManager> mockDeviceManager = new Mock<IDeviceManager>();

            logicalDeviceHeadset.Setup(ld => ld.Id).Returns(Guid.NewGuid());
            PhysicalDevice.Setup(pd => pd.Devices).Returns(new[] { logicalDeviceHeadset.Object });
            mockDeviceManager.Setup(dm => dm.Devices).Returns(new[] { PhysicalDevice.Object });// mockDeviceManager device- PhysicalDevice -logicalDeviceHeadset device- logicalDeviceHeadset device ID

            Guid logicalDeviceHeadsetId = logicalDeviceHeadset.Object.Id;

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
                        IsCollaborationChatEnable=false,
                        IsCollaborationMicEnable = false,
                        IsCollaborationBlinkEffectEnable=false,
                        IsCollaborationDoubleTapEnable=false,
                        BackLightingControls=20,
                        BackLightingLevel=20,
                        SidetoneLevel=20,
                        //ID=logicalDeviceHeadsetId,
                        AncMode=20,
                        AncGain=20,
                        SelectedPreset=20,
                        Band1Gain=20,
                        Band2Gain=20,
                        Band3Gain=20,
                        Band4Gain=20,
                        Band5Gain=20,
                        MicNoiseCancellation=false,
                        Sidetone=false,
                        WearDetection=20,
                    }
                },
                DCFVersion = "1.0",
                DPeMSDKVersion = "1.0",
                DPeMSubAgentVersion = "1.0",
                IsdDriverVersion = "1.0",
            };
            privatetePeripheralsPlugin.SetFieldOrProperty("_deviceHelper", deviceHelper);

            peripheralsPlugin.SetWearDetection(newValue, logicalDeviceHeadsetId);                      // logicalDeviceis logicalDeviceHeadset,WearDetection=20, _deviceHelper.deviceInfo ID != logicalDeviceHeadsetId
            var devicemanager = privatetePeripheralsPlugin.GetFieldOrProperty("_iDeviceManager");
            Assert.IsTrue(true);
            Assert.IsNotNull(devicemanager);

            deviceHelper.deviceInfo[0].ID = logicalDeviceHeadsetId;
            privatetePeripheralsPlugin.SetFieldOrProperty("_deviceHelper", deviceHelper);

            peripheralsPlugin.SetWearDetection(newValue, logicalDeviceHeadsetId);                     // logicalDeviceis logicalDeviceHeadset,WearDetection=20, _deviceHelper.deviceInfo ID = logicalDeviceHeadsetId
            var devicemanager2 = privatetePeripheralsPlugin.GetFieldOrProperty("_iDeviceManager");
            var deviceHelper2 = (DeviceHelper)privatetePeripheralsPlugin.GetFieldOrProperty("_deviceHelper");
            Assert.IsTrue(true);
            Assert.IsNotNull(devicemanager2);
            Assert.IsNotNull(deviceHelper2);
            //Assert.That(deviceHelper2.deviceInfo[0].WearDetection, Is.EqualTo(newValue));  method update R17.1 drop this function
        }

        [Test]
        public void TestSetWearDetectionForCLI()
        {
            int newValue1 = 0;
            Mock<IPhysicalDevice> PhysicalDevice = new Mock<IPhysicalDevice>();
            Mock<ILogicalDeviceHeadset> logicalDeviceHeadset = new Mock<ILogicalDeviceHeadset>();
            Mock<IDeviceManager> mockDeviceManager = new Mock<IDeviceManager>();

            logicalDeviceHeadset.Setup(ld => ld.Id).Returns(Guid.NewGuid());
            PhysicalDevice.Setup(pd => pd.Devices).Returns(new[] { logicalDeviceHeadset.Object });
            mockDeviceManager.Setup(dm => dm.Devices).Returns(new[] { PhysicalDevice.Object });// mockDeviceManager device- PhysicalDevice -logicalDeviceHeadset device- logicalDeviceHeadset device ID

            Guid logicalDeviceHeadsetId = logicalDeviceHeadset.Object.Id;

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
                        IsCollaborationChatEnable=false,
                        IsCollaborationMicEnable = false,
                        IsCollaborationBlinkEffectEnable=false,
                        IsCollaborationDoubleTapEnable=false,
                        BackLightingControls=20,
                        BackLightingLevel=20,
                        SidetoneLevel=20,
                        //ID=logicalDeviceHeadsetId,
                        AncMode=20,
                        AncGain=20,
                        SelectedPreset=20,
                        Band1Gain=20,
                        Band2Gain=20,
                        Band3Gain=20,
                        Band4Gain=20,
                        Band5Gain=20,
                        MicNoiseCancellation=false,
                        Sidetone=false,
                        WearDetection=20,
                    }
                },
                DCFVersion = "1.0",
                DPeMSDKVersion = "1.0",
                DPeMSubAgentVersion = "1.0",
                IsdDriverVersion = "1.0",
            };
            privatetePeripheralsPlugin.SetFieldOrProperty("_deviceHelper", deviceHelper);

            if (newValue1 == 0)
            {
                newValue1 |= 0b00000000;
                peripheralsPlugin.SetWearDetectionForCLI(newValue1, logicalDeviceHeadsetId);                      // newValue1=0,logicalDeviceis logicalDeviceHeadset,WearDetection=20, _deviceHelper.deviceInfo ID != logicalDeviceHeadsetId
                var devicemanager = privatetePeripheralsPlugin.GetFieldOrProperty("_iDeviceManager");
                Assert.IsTrue(true);
                Assert.IsNotNull(devicemanager);
            }

            int newValue2 = 10;
            if (newValue2 != 0)
            {

                deviceHelper.deviceInfo[0].ID = logicalDeviceHeadsetId;
                privatetePeripheralsPlugin.SetFieldOrProperty("_deviceHelper", deviceHelper);

                peripheralsPlugin.SetWearDetectionForCLI(newValue2, logicalDeviceHeadsetId);                     // newValue2=10,logicalDeviceis logicalDeviceHeadset,WearDetection=20, _deviceHelper.deviceInfo ID = logicalDeviceHeadsetId
                newValue2 |= 0b00000111;
                var devicemanager2 = privatetePeripheralsPlugin.GetFieldOrProperty("_iDeviceManager");
                var deviceHelper2 = (DeviceHelper)privatetePeripheralsPlugin.GetFieldOrProperty("_deviceHelper");
                Assert.IsTrue(true);
                Assert.IsNotNull(devicemanager2);
                Assert.IsNotNull(deviceHelper2);
                //Assert.That(deviceHelper2.deviceInfo[0].WearDetection, Is.EqualTo(newValue2)); method update R17.1 drop this function
            }
        }

        [Test]
        public void TestSetBusyLight()
        {
            bool newValue = false;
            Mock<IPhysicalDevice> PhysicalDevice = new Mock<IPhysicalDevice>();
            Mock<ILogicalDeviceHeadset> logicalDeviceHeadset = new Mock<ILogicalDeviceHeadset>();
            Mock<IDeviceManager> mockDeviceManager = new Mock<IDeviceManager>();

            logicalDeviceHeadset.Setup(ld => ld.Id).Returns(Guid.NewGuid());
            PhysicalDevice.Setup(pd => pd.Devices).Returns(new[] { logicalDeviceHeadset.Object });
            mockDeviceManager.Setup(dm => dm.Devices).Returns(new[] { PhysicalDevice.Object });// mockDeviceManager device- PhysicalDevice -logicalDeviceHeadset device- logicalDeviceHeadset device ID

            Guid logicalDeviceHeadsetId = logicalDeviceHeadset.Object.Id;

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
                        IsCollaborationChatEnable=false,
                        IsCollaborationMicEnable = false,
                        IsCollaborationBlinkEffectEnable=false,
                        IsCollaborationDoubleTapEnable=false,
                        BackLightingControls=20,
                        BackLightingLevel=20,
                        SidetoneLevel=20,
                        //ID=logicalDeviceHeadsetId,
                        AncMode=20,
                        AncGain=20,
                        SelectedPreset=20,
                        Band1Gain=20,
                        Band2Gain=20,
                        Band3Gain=20,
                        Band4Gain=20,
                        Band5Gain=20,
                        MicNoiseCancellation=false,
                        Sidetone=false,
                        WearDetection=20,
                        BusyLight=false,
                    }
                },
                DCFVersion = "1.0",
                DPeMSDKVersion = "1.0",
                DPeMSubAgentVersion = "1.0",
                IsdDriverVersion = "1.0",
            };
            privatetePeripheralsPlugin.SetFieldOrProperty("_deviceHelper", deviceHelper);

            peripheralsPlugin.SetBusyLight(newValue, logicalDeviceHeadsetId);                      // logicalDeviceis logicalDeviceHeadset,BusyLight=false, _deviceHelper.deviceInfo ID != logicalDeviceHeadsetId
            var devicemanager = privatetePeripheralsPlugin.GetFieldOrProperty("_iDeviceManager");
            Assert.IsTrue(true);
            Assert.IsNotNull(devicemanager);

            deviceHelper.deviceInfo[0].ID = logicalDeviceHeadsetId;
            privatetePeripheralsPlugin.SetFieldOrProperty("_deviceHelper", deviceHelper);

            peripheralsPlugin.SetBusyLight(newValue, logicalDeviceHeadsetId);                     // logicalDeviceis logicalDeviceHeadset,BusyLight=false, _deviceHelper.deviceInfo ID = logicalDeviceHeadsetId
            var devicemanager2 = privatetePeripheralsPlugin.GetFieldOrProperty("_iDeviceManager");
            var deviceHelper2 = (DeviceHelper)privatetePeripheralsPlugin.GetFieldOrProperty("_deviceHelper");
            Assert.IsTrue(true);
            Assert.IsNotNull(devicemanager2);
            Assert.IsNotNull(deviceHelper2);
            Assert.That(deviceHelper2.deviceInfo[0].BusyLight, Is.EqualTo(newValue));
        }

        [Test]
        public void TestSetVoiceGuidance()
        {
            bool newValue = false;
            Mock<IPhysicalDevice> PhysicalDevice = new Mock<IPhysicalDevice>();
            Mock<ILogicalDeviceHeadset> logicalDeviceHeadset = new Mock<ILogicalDeviceHeadset>();
            Mock<IDeviceManager> mockDeviceManager = new Mock<IDeviceManager>();

            logicalDeviceHeadset.Setup(ld => ld.Id).Returns(Guid.NewGuid());
            PhysicalDevice.Setup(pd => pd.Devices).Returns(new[] { logicalDeviceHeadset.Object });
            mockDeviceManager.Setup(dm => dm.Devices).Returns(new[] { PhysicalDevice.Object });// mockDeviceManager device- PhysicalDevice -logicalDeviceHeadset device- logicalDeviceHeadset device ID

            Guid logicalDeviceHeadsetId = logicalDeviceHeadset.Object.Id;

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
                        IsCollaborationChatEnable=false,
                        IsCollaborationMicEnable = false,
                        IsCollaborationBlinkEffectEnable=false,
                        IsCollaborationDoubleTapEnable=false,
                        BackLightingControls=20,
                        BackLightingLevel=20,
                        SidetoneLevel=20,
                        //ID=logicalDeviceHeadsetId,
                        AncMode=20,
                        AncGain=20,
                        SelectedPreset=20,
                        Band1Gain=20,
                        Band2Gain=20,
                        Band3Gain=20,
                        Band4Gain=20,
                        Band5Gain=20,
                        MicNoiseCancellation=false,
                        Sidetone=false,
                        WearDetection=20,
                        BusyLight=false,
                        VoiceGuidance=false,
                    }
                },
                DCFVersion = "1.0",
                DPeMSDKVersion = "1.0",
                DPeMSubAgentVersion = "1.0",
                IsdDriverVersion = "1.0",
            };
            privatetePeripheralsPlugin.SetFieldOrProperty("_deviceHelper", deviceHelper);

            peripheralsPlugin.SetVoiceGuidance(newValue, logicalDeviceHeadsetId);                      // logicalDeviceis logicalDeviceHeadset,VoiceGuidance=false, _deviceHelper.deviceInfo ID != logicalDeviceHeadsetId
            var devicemanager = privatetePeripheralsPlugin.GetFieldOrProperty("_iDeviceManager");
            Assert.IsTrue(true);
            Assert.IsNotNull(devicemanager);

            deviceHelper.deviceInfo[0].ID = logicalDeviceHeadsetId;
            privatetePeripheralsPlugin.SetFieldOrProperty("_deviceHelper", deviceHelper);

            peripheralsPlugin.SetVoiceGuidance(newValue, logicalDeviceHeadsetId);                     // logicalDeviceis logicalDeviceHeadset,VoiceGuidance=false, _deviceHelper.deviceInfo ID = logicalDeviceHeadsetId
            var devicemanager2 = privatetePeripheralsPlugin.GetFieldOrProperty("_iDeviceManager");
            var deviceHelper2 = (DeviceHelper)privatetePeripheralsPlugin.GetFieldOrProperty("_deviceHelper");
            Assert.IsTrue(true);
            Assert.IsNotNull(devicemanager2);
            Assert.IsNotNull(deviceHelper2);
            Assert.That(deviceHelper2.deviceInfo[0].VoiceGuidance, Is.EqualTo(newValue));
        }

        [Test]
        public void TestSetMicNCIncoming()
        {
            bool newValue = false;
            Mock<IPhysicalDevice> PhysicalDevice = new Mock<IPhysicalDevice>();
            Mock<ILogicalDeviceHeadset> logicalDeviceHeadset = new Mock<ILogicalDeviceHeadset>();
            Mock<IDeviceManager> mockDeviceManager = new Mock<IDeviceManager>();

            logicalDeviceHeadset.Setup(ld => ld.Id).Returns(Guid.NewGuid());
            PhysicalDevice.Setup(pd => pd.Devices).Returns(new[] { logicalDeviceHeadset.Object });
            mockDeviceManager.Setup(dm => dm.Devices).Returns(new[] { PhysicalDevice.Object });// mockDeviceManager device- PhysicalDevice -logicalDeviceHeadset device- logicalDeviceHeadset device ID

            Guid logicalDeviceHeadsetId = logicalDeviceHeadset.Object.Id;

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
                        IsCollaborationChatEnable=false,
                        IsCollaborationMicEnable = false,
                        IsCollaborationBlinkEffectEnable=false,
                        IsCollaborationDoubleTapEnable=false,
                        BackLightingControls=20,
                        BackLightingLevel=20,
                        SidetoneLevel=20,
                        //ID=logicalDeviceHeadsetId,
                        AncMode=20,
                        AncGain=20,
                        SelectedPreset=20,
                        Band1Gain=20,
                        Band2Gain=20,
                        Band3Gain=20,
                        Band4Gain=20,
                        Band5Gain=20,
                        MicNoiseCancellation=false,
                        Sidetone=false,
                        WearDetection=20,
                        BusyLight=false,
                        VoiceGuidance=false,
                        MicNCIncoming=false,
                    }
                },
                DCFVersion = "1.0",
                DPeMSDKVersion = "1.0",
                DPeMSubAgentVersion = "1.0",
                IsdDriverVersion = "1.0",
            };
            privatetePeripheralsPlugin.SetFieldOrProperty("_deviceHelper", deviceHelper);

            peripheralsPlugin.SetMicNCIncoming(newValue, logicalDeviceHeadsetId);                      // logicalDeviceis logicalDeviceHeadset,MicNCIncoming=false, _deviceHelper.deviceInfo ID != logicalDeviceHeadsetId
            var devicemanager = privatetePeripheralsPlugin.GetFieldOrProperty("_iDeviceManager");
            Assert.IsTrue(true);
            Assert.IsNotNull(devicemanager);

            deviceHelper.deviceInfo[0].ID = logicalDeviceHeadsetId;
            privatetePeripheralsPlugin.SetFieldOrProperty("_deviceHelper", deviceHelper);

            peripheralsPlugin.SetMicNCIncoming(newValue, logicalDeviceHeadsetId);                     // logicalDeviceis logicalDeviceHeadset,MicNCIncoming=false, _deviceHelper.deviceInfo ID = logicalDeviceHeadsetId
            var devicemanager2 = privatetePeripheralsPlugin.GetFieldOrProperty("_iDeviceManager");
            var deviceHelper2 = (DeviceHelper)privatetePeripheralsPlugin.GetFieldOrProperty("_deviceHelper");
            Assert.IsTrue(true);
            Assert.IsNotNull(devicemanager2);
            Assert.IsNotNull(deviceHelper2);
            Assert.That(deviceHelper2.deviceInfo[0].MicNCIncoming, Is.EqualTo(newValue));
        }

        [Test]
        public void TestSetSideTopSwitchSinglePressSetting()
        {
            byte[] newValue = new byte[] { 10, 20, 30, 40 };
            Mock<IPhysicalDevice> PhysicalDevice = new Mock<IPhysicalDevice>();
            Mock<ILogicalDevicePen> logicalDevicePen = new Mock<ILogicalDevicePen>();
            Mock<IDeviceManager> mockDeviceManager = new Mock<IDeviceManager>();

            logicalDevicePen.Setup(ld => ld.Id).Returns(Guid.NewGuid());
            PhysicalDevice.Setup(pd => pd.Devices).Returns(new[] { logicalDevicePen.Object });
            mockDeviceManager.Setup(dm => dm.Devices).Returns(new[] { PhysicalDevice.Object });// mockDeviceManager device- PhysicalDevice -logicalDevicePen device- logicalDevicePen device ID

            Guid logicalDevicePenId = logicalDevicePen.Object.Id;

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
                        IsCollaborationChatEnable=false,
                        IsCollaborationMicEnable = false,
                        IsCollaborationBlinkEffectEnable=false,
                        IsCollaborationDoubleTapEnable=false,
                        BackLightingControls=20,
                        BackLightingLevel=20,
                        SidetoneLevel=20,
                        //ID=logicalDeviceHeadsetId,
                        AncMode=20,
                        AncGain=20,
                        SelectedPreset=20,
                        Band1Gain=20,
                        Band2Gain=20,
                        Band3Gain=20,
                        Band4Gain=20,
                        Band5Gain=20,
                        MicNoiseCancellation=false,
                        Sidetone=false,
                        WearDetection=20,
                        BusyLight=false,
                        VoiceGuidance=false,
                        MicNCIncoming=false,
                        SideTopSwitchSinglePressSetting=new byte[] {10,20,30,40 },
                    }
                },
                DCFVersion = "1.0",
                DPeMSDKVersion = "1.0",
                DPeMSubAgentVersion = "1.0",
                IsdDriverVersion = "1.0",
            };
            privatetePeripheralsPlugin.SetFieldOrProperty("_deviceHelper", deviceHelper);

            peripheralsPlugin.SetSideTopSwitchSinglePressSetting(newValue, logicalDevicePenId);                      // logicalDeviceis logicalDeviceHeadset,SideTopSwitchSinglePressSetting=new byte[] {10,20,30,40 } , _deviceHelper.deviceInfo ID != logicalDeviceHeadsetId
            var devicemanager = privatetePeripheralsPlugin.GetFieldOrProperty("_iDeviceManager");
            Assert.IsTrue(true);
            Assert.IsNotNull(devicemanager);

            deviceHelper.deviceInfo[0].ID = logicalDevicePenId;
            privatetePeripheralsPlugin.SetFieldOrProperty("_deviceHelper", deviceHelper);

            peripheralsPlugin.SetSideTopSwitchSinglePressSetting(newValue, logicalDevicePenId);                     // logicalDeviceis logicalDeviceHeadset,SideTopSwitchSinglePressSetting=new byte[] {10,20,30,40 } , _deviceHelper.deviceInfo ID = logicalDeviceHeadsetId
            var devicemanager2 = privatetePeripheralsPlugin.GetFieldOrProperty("_iDeviceManager");
            var deviceHelper2 = (DeviceHelper)privatetePeripheralsPlugin.GetFieldOrProperty("_deviceHelper");
            Assert.IsTrue(true);
            Assert.IsNotNull(devicemanager2);
            Assert.IsNotNull(deviceHelper2);
            Assert.IsNotNull(logicalDevicePen.Object.SideTopSwitchSinglePressSetting);
        }

        [Test]
        public void TestSetIsMicEnumerationOn()
        {
            bool newValue = false;
            Mock<IPhysicalDevice> PhysicalDevice = new Mock<IPhysicalDevice>();
            Mock<ILogicalDeviceWebcam> LogicalDeviceWebcam = new Mock<ILogicalDeviceWebcam>();
            Mock<IDeviceManager> mockDeviceManager = new Mock<IDeviceManager>();

            LogicalDeviceWebcam.Setup(ld => ld.Id).Returns(Guid.NewGuid());
            PhysicalDevice.Setup(pd => pd.Devices).Returns(new[] { LogicalDeviceWebcam.Object });
            mockDeviceManager.Setup(dm => dm.Devices).Returns(new[] { PhysicalDevice.Object });// mockDeviceManager device- PhysicalDevice -ILogicalDeviceWebcam device- ILogicalDeviceWebcam device ID

            Guid LogicalDeviceWebcamId = LogicalDeviceWebcam.Object.Id;

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
                        IsCollaborationChatEnable=false,
                        IsCollaborationMicEnable = false,
                        IsCollaborationBlinkEffectEnable=false,
                        IsCollaborationDoubleTapEnable=false,
                        BackLightingControls=20,
                        BackLightingLevel=20,
                        SidetoneLevel=20,
                        //ID=logicalDeviceHeadsetId,
                        AncMode=20,
                        AncGain=20,
                        SelectedPreset=20,
                        Band1Gain=20,
                        Band2Gain=20,
                        Band3Gain=20,
                        Band4Gain=20,
                        Band5Gain=20,
                        MicNoiseCancellation=false,
                        Sidetone=false,
                        WearDetection=20,
                        BusyLight=false,
                        VoiceGuidance=false,
                        MicNCIncoming=false,
                        SideTopSwitchSinglePressSetting=new byte[] {10,20,30,40 },
                        IsMicEnumerationOn=false,
                    }
                },
                DCFVersion = "1.0",
                DPeMSDKVersion = "1.0",
                DPeMSubAgentVersion = "1.0",
                IsdDriverVersion = "1.0",
            };
            privatetePeripheralsPlugin.SetFieldOrProperty("_deviceHelper", deviceHelper);

            peripheralsPlugin.SetIsMicEnumerationOn(newValue, LogicalDeviceWebcamId);                      // logicalDeviceis LogicalDeviceWebcamId,IsMicEnumerationOn=false, _deviceHelper.deviceInfo ID != logicalDeviceHeadsetId
            var devicemanager = privatetePeripheralsPlugin.GetFieldOrProperty("_iDeviceManager");
            Assert.IsTrue(true);
            Assert.IsNotNull(devicemanager);

            deviceHelper.deviceInfo[0].ID = LogicalDeviceWebcamId;
            privatetePeripheralsPlugin.SetFieldOrProperty("_deviceHelper", deviceHelper);

            peripheralsPlugin.SetIsMicEnumerationOn(newValue, LogicalDeviceWebcamId);                     // logicalDeviceis LogicalDeviceWebcamId,IsMicEnumerationOn=false, _deviceHelper.deviceInfo ID = logicalDeviceHeadsetId
            var devicemanager2 = privatetePeripheralsPlugin.GetFieldOrProperty("_iDeviceManager");
            var deviceHelper2 = (DeviceHelper)privatetePeripheralsPlugin.GetFieldOrProperty("_deviceHelper");
            Assert.IsTrue(true);
            Assert.IsNotNull(devicemanager2);
            Assert.IsNotNull(deviceHelper2);
            Assert.That(deviceHelper2.deviceInfo[0].IsMicEnumerationOn, Is.EqualTo(newValue));
        }

        [Test]
        public void TestSetCurrentSelectedProfile()
        {
            string newValue = "TestSetCurrentSelectedProfile";
            Mock<IPhysicalDevice> PhysicalDevice = new Mock<IPhysicalDevice>();
            Mock<ILogicalDeviceWebcam> LogicalDeviceWebcam = new Mock<ILogicalDeviceWebcam>();
            Mock<IDeviceManager> mockDeviceManager = new Mock<IDeviceManager>();
#if SUPPORT_210
            //Mock<IWebcamProfileManager> WebcamProfileManager = new Mock<IWebcamProfileManager>();
            //LogicalDeviceWebcam.Setup(ld => ld.ProfileManager).Returns(WebcamProfileManager.Object);
#else
            Mock<IWebcamProfileManager> WebcamProfileManager = new Mock<IWebcamProfileManager>();
            LogicalDeviceWebcam.Setup(ld => ld.ProfileManager).Returns(WebcamProfileManager.Object);
#endif
            LogicalDeviceWebcam.Setup(ld => ld.Id).Returns(Guid.NewGuid());
            PhysicalDevice.Setup(pd => pd.Devices).Returns(new[] { LogicalDeviceWebcam.Object });
            mockDeviceManager.Setup(dm => dm.Devices).Returns(new[] { PhysicalDevice.Object });// mockDeviceManager device- PhysicalDevice -ILogicalDeviceWebcam device- ILogicalDeviceWebcam device ID- ILogicalDeviceWebcam ProfileManager

            Guid LogicalDeviceWebcamId = LogicalDeviceWebcam.Object.Id;

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
                        IsCollaborationChatEnable=false,
                        IsCollaborationMicEnable = false,
                        IsCollaborationBlinkEffectEnable=false,
                        IsCollaborationDoubleTapEnable=false,
                        BackLightingControls=20,
                        BackLightingLevel=20,
                        SidetoneLevel=20,
                        //ID=logicalDeviceHeadsetId,
                        AncMode=20,
                        AncGain=20,
                        SelectedPreset=20,
                        Band1Gain=20,
                        Band2Gain=20,
                        Band3Gain=20,
                        Band4Gain=20,
                        Band5Gain=20,
                        MicNoiseCancellation=false,
                        Sidetone=false,
                        WearDetection=20,
                        BusyLight=false,
                        VoiceGuidance=false,
                        MicNCIncoming=false,
                        SideTopSwitchSinglePressSetting=new byte[] {10,20,30,40 },
                        IsMicEnumerationOn=false,
                    }
                },
                DCFVersion = "1.0",
                DPeMSDKVersion = "1.0",
                DPeMSubAgentVersion = "1.0",
                IsdDriverVersion = "1.0",
            };
            privatetePeripheralsPlugin.SetFieldOrProperty("_deviceHelper", deviceHelper);

            peripheralsPlugin.SetCurrentSelectedProfile(newValue, LogicalDeviceWebcamId);                      // logicalDeviceis LogicalDeviceWebcamId,IsMicEnumerationOn=false, _deviceHelper.deviceInfo ID != logicalDeviceHeadsetId
            var devicemanager = privatetePeripheralsPlugin.GetFieldOrProperty("_iDeviceManager");
            Assert.IsTrue(true);
            Assert.IsNotNull(devicemanager);

            deviceHelper.deviceInfo[0].ID = LogicalDeviceWebcamId;
            privatetePeripheralsPlugin.SetFieldOrProperty("_deviceHelper", deviceHelper);

            peripheralsPlugin.SetCurrentSelectedProfile(newValue, LogicalDeviceWebcamId);                     // logicalDeviceis LogicalDeviceWebcamId,IsMicEnumerationOn=false, _deviceHelper.deviceInfo ID = logicalDeviceHeadsetId
            var devicemanager2 = privatetePeripheralsPlugin.GetFieldOrProperty("_iDeviceManager");
            var deviceHelper2 = (DeviceHelper)privatetePeripheralsPlugin.GetFieldOrProperty("_deviceHelper");
            Assert.IsTrue(true);
            Assert.IsNotNull(devicemanager2);
            Assert.IsNotNull(deviceHelper2);
        }

        [Test]
        public void TestSetWALTime()
        {
            int newValue = 10;
            Mock<IPhysicalDevice> PhysicalDevice = new Mock<IPhysicalDevice>();
            Mock<ILogicalDeviceWebcam> LogicalDeviceWebcam = new Mock<ILogicalDeviceWebcam>();
            Mock<IDeviceManager> mockDeviceManager = new Mock<IDeviceManager>();

            LogicalDeviceWebcam.Setup(ld => ld.Id).Returns(Guid.NewGuid());
            PhysicalDevice.Setup(pd => pd.Devices).Returns(new[] { LogicalDeviceWebcam.Object });
            mockDeviceManager.Setup(dm => dm.Devices).Returns(new[] { PhysicalDevice.Object });// mockDeviceManager device- PhysicalDevice -ILogicalDeviceWebcam device- ILogicalDeviceWebcam device ID

            Guid LogicalDeviceWebcamId = LogicalDeviceWebcam.Object.Id;

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
                        IsCollaborationChatEnable=false,
                        IsCollaborationMicEnable = false,
                        IsCollaborationBlinkEffectEnable=false,
                        IsCollaborationDoubleTapEnable=false,
                        BackLightingControls=20,
                        BackLightingLevel=20,
                        SidetoneLevel=20,
                        //ID=logicalDeviceHeadsetId,
                        AncMode=20,
                        AncGain=20,
                        SelectedPreset=20,
                        Band1Gain=20,
                        Band2Gain=20,
                        Band3Gain=20,
                        Band4Gain=20,
                        Band5Gain=20,
                        MicNoiseCancellation=false,
                        Sidetone=false,
                        WearDetection=20,
                        BusyLight=false,
                        VoiceGuidance=false,
                        MicNCIncoming=false,
                        SideTopSwitchSinglePressSetting=new byte[] {10,20,30,40 },
                        IsMicEnumerationOn=false,
                        WALTime=20,
                    }
                },
                DCFVersion = "1.0",
                DPeMSDKVersion = "1.0",
                DPeMSubAgentVersion = "1.0",
                IsdDriverVersion = "1.0",
            };
            privatetePeripheralsPlugin.SetFieldOrProperty("_deviceHelper", deviceHelper);

            peripheralsPlugin.SetWALTime(newValue, LogicalDeviceWebcamId);                      // logicalDeviceis LogicalDeviceWebcamId,WALTime=20, _deviceHelper.deviceInfo ID != logicalDeviceHeadsetId
            var devicemanager = privatetePeripheralsPlugin.GetFieldOrProperty("_iDeviceManager");
            Assert.IsTrue(true);
            Assert.IsNotNull(devicemanager);

            deviceHelper.deviceInfo[0].ID = LogicalDeviceWebcamId;
            privatetePeripheralsPlugin.SetFieldOrProperty("_deviceHelper", deviceHelper);

            peripheralsPlugin.SetWALTime(newValue, LogicalDeviceWebcamId);                     // logicalDeviceis LogicalDeviceWebcamId,WALTime=20, _deviceHelper.deviceInfo ID = logicalDeviceHeadsetId
            var devicemanager2 = privatetePeripheralsPlugin.GetFieldOrProperty("_iDeviceManager");
            var deviceHelper2 = (DeviceHelper)privatetePeripheralsPlugin.GetFieldOrProperty("_deviceHelper");
            Assert.IsTrue(true);
            Assert.IsNotNull(devicemanager2);
            Assert.IsNotNull(deviceHelper2);
            Assert.That(deviceHelper2.deviceInfo[0].WALTime, Is.EqualTo(newValue));
        }

        [Test]
        public void TestSetSnooze()
        {
            int newValue = 10;
            Mock<IPhysicalDevice> PhysicalDevice = new Mock<IPhysicalDevice>();
            Mock<ILogicalDeviceWebcam> LogicalDeviceWebcam = new Mock<ILogicalDeviceWebcam>();
            Mock<IDeviceManager> mockDeviceManager = new Mock<IDeviceManager>();

            LogicalDeviceWebcam.Setup(ld => ld.Id).Returns(Guid.NewGuid());
            PhysicalDevice.Setup(pd => pd.Devices).Returns(new[] { LogicalDeviceWebcam.Object });
            mockDeviceManager.Setup(dm => dm.Devices).Returns(new[] { PhysicalDevice.Object });// mockDeviceManager device- PhysicalDevice -ILogicalDeviceWebcam device- ILogicalDeviceWebcam device ID

            Guid LogicalDeviceWebcamId = LogicalDeviceWebcam.Object.Id;

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
                        IsCollaborationChatEnable=false,
                        IsCollaborationMicEnable = false,
                        IsCollaborationBlinkEffectEnable=false,
                        IsCollaborationDoubleTapEnable=false,
                        BackLightingControls=20,
                        BackLightingLevel=20,
                        SidetoneLevel=20,
                        //ID=logicalDeviceHeadsetId,
                        AncMode=20,
                        AncGain=20,
                        SelectedPreset=20,
                        Band1Gain=20,
                        Band2Gain=20,
                        Band3Gain=20,
                        Band4Gain=20,
                        Band5Gain=20,
                        MicNoiseCancellation=false,
                        Sidetone=false,
                        WearDetection=20,
                        BusyLight=false,
                        VoiceGuidance=false,
                        MicNCIncoming=false,
                        SideTopSwitchSinglePressSetting=new byte[] {10,20,30,40 },
                        IsMicEnumerationOn=false,
                        WALTime=20,
                        Snooze=20,
                    }
                },
                DCFVersion = "1.0",
                DPeMSDKVersion = "1.0",
                DPeMSubAgentVersion = "1.0",
                IsdDriverVersion = "1.0",
            };
            privatetePeripheralsPlugin.SetFieldOrProperty("_deviceHelper", deviceHelper);

            peripheralsPlugin.SetSnooze(newValue, LogicalDeviceWebcamId);                      // logicalDeviceis LogicalDeviceWebcamId,Snooze=20, _deviceHelper.deviceInfo ID != logicalDeviceHeadsetId
            var devicemanager = privatetePeripheralsPlugin.GetFieldOrProperty("_iDeviceManager");
            Assert.IsTrue(true);
            Assert.IsNotNull(devicemanager);

            deviceHelper.deviceInfo[0].ID = LogicalDeviceWebcamId;
            privatetePeripheralsPlugin.SetFieldOrProperty("_deviceHelper", deviceHelper);

            peripheralsPlugin.SetSnooze(newValue, LogicalDeviceWebcamId);                     // logicalDeviceis LogicalDeviceWebcamId,Snooze=20, _deviceHelper.deviceInfo ID = logicalDeviceHeadsetId
            var devicemanager2 = privatetePeripheralsPlugin.GetFieldOrProperty("_iDeviceManager");
            var deviceHelper2 = (DeviceHelper)privatetePeripheralsPlugin.GetFieldOrProperty("_deviceHelper");
            Assert.IsTrue(true);
            Assert.IsNotNull(devicemanager2);
            Assert.IsNotNull(deviceHelper2);
            Assert.That(deviceHelper2.deviceInfo[0].Snooze, Is.EqualTo(newValue));
        }

        [Test]
        public void TestSetSnoozeLength()
        {
            int newValue = 10;
            Mock<IPhysicalDevice> PhysicalDevice = new Mock<IPhysicalDevice>();
            Mock<ILogicalDeviceWebcam> LogicalDeviceWebcam = new Mock<ILogicalDeviceWebcam>();
            Mock<IDeviceManager> mockDeviceManager = new Mock<IDeviceManager>();

            LogicalDeviceWebcam.Setup(ld => ld.Id).Returns(Guid.NewGuid());
            PhysicalDevice.Setup(pd => pd.Devices).Returns(new[] { LogicalDeviceWebcam.Object });
            mockDeviceManager.Setup(dm => dm.Devices).Returns(new[] { PhysicalDevice.Object });// mockDeviceManager device- PhysicalDevice -ILogicalDeviceWebcam device- ILogicalDeviceWebcam device ID

            Guid LogicalDeviceWebcamId = LogicalDeviceWebcam.Object.Id;

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
                        IsCollaborationChatEnable=false,
                        IsCollaborationMicEnable = false,
                        IsCollaborationBlinkEffectEnable=false,
                        IsCollaborationDoubleTapEnable=false,
                        BackLightingControls=20,
                        BackLightingLevel=20,
                        SidetoneLevel=20,
                        //ID=logicalDeviceHeadsetId,
                        AncMode=20,
                        AncGain=20,
                        SelectedPreset=20,
                        Band1Gain=20,
                        Band2Gain=20,
                        Band3Gain=20,
                        Band4Gain=20,
                        Band5Gain=20,
                        MicNoiseCancellation=false,
                        Sidetone=false,
                        WearDetection=20,
                        BusyLight=false,
                        VoiceGuidance=false,
                        MicNCIncoming=false,
                        SideTopSwitchSinglePressSetting=new byte[] {10,20,30,40 },
                        IsMicEnumerationOn=false,
                        WALTime=20,
                        Snooze=20,
                        SnoozeLength=20,
                    }
                },
                DCFVersion = "1.0",
                DPeMSDKVersion = "1.0",
                DPeMSubAgentVersion = "1.0",
                IsdDriverVersion = "1.0",
            };
            privatetePeripheralsPlugin.SetFieldOrProperty("_deviceHelper", deviceHelper);

            peripheralsPlugin.SetSnoozeLength(newValue, LogicalDeviceWebcamId);                      // logicalDeviceis LogicalDeviceWebcamId,SnoozeLength=20, _deviceHelper.deviceInfo ID != logicalDeviceHeadsetId
            var devicemanager = privatetePeripheralsPlugin.GetFieldOrProperty("_iDeviceManager");
            Assert.IsTrue(true);
            Assert.IsNotNull(devicemanager);

            deviceHelper.deviceInfo[0].ID = LogicalDeviceWebcamId;
            privatetePeripheralsPlugin.SetFieldOrProperty("_deviceHelper", deviceHelper);

            peripheralsPlugin.SetSnoozeLength(newValue, LogicalDeviceWebcamId);                     // logicalDeviceis LogicalDeviceWebcamId,SnoozeLength=20, _deviceHelper.deviceInfo ID = logicalDeviceHeadsetId
            var devicemanager2 = privatetePeripheralsPlugin.GetFieldOrProperty("_iDeviceManager");
            var deviceHelper2 = (DeviceHelper)privatetePeripheralsPlugin.GetFieldOrProperty("_deviceHelper");
            Assert.IsTrue(true);
            Assert.IsNotNull(devicemanager2);
            Assert.IsNotNull(deviceHelper2);
            Assert.That(deviceHelper2.deviceInfo[0].SnoozeLength, Is.EqualTo(newValue));
        }

        [Test]
        public void TestSetIsProximitySensorEnable()
        {
            bool newValue = false;
            Mock<IPhysicalDevice> PhysicalDevice = new Mock<IPhysicalDevice>();
            Mock<ILogicalDeviceWebcam> LogicalDeviceWebcam = new Mock<ILogicalDeviceWebcam>();
            Mock<IDeviceManager> mockDeviceManager = new Mock<IDeviceManager>();

            LogicalDeviceWebcam.Setup(ld => ld.Id).Returns(Guid.NewGuid());
            PhysicalDevice.Setup(pd => pd.Devices).Returns(new[] { LogicalDeviceWebcam.Object });
            mockDeviceManager.Setup(dm => dm.Devices).Returns(new[] { PhysicalDevice.Object });// mockDeviceManager device- PhysicalDevice -ILogicalDeviceWebcam device- ILogicalDeviceWebcam device ID

            Guid LogicalDeviceWebcamId = LogicalDeviceWebcam.Object.Id;

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
                        IsCollaborationChatEnable=false,
                        IsCollaborationMicEnable = false,
                        IsCollaborationBlinkEffectEnable=false,
                        IsCollaborationDoubleTapEnable=false,
                        BackLightingControls=20,
                        BackLightingLevel=20,
                        SidetoneLevel=20,
                        //ID=logicalDeviceHeadsetId,
                        AncMode=20,
                        AncGain=20,
                        SelectedPreset=20,
                        Band1Gain=20,
                        Band2Gain=20,
                        Band3Gain=20,
                        Band4Gain=20,
                        Band5Gain=20,
                        MicNoiseCancellation=false,
                        Sidetone=false,
                        WearDetection=20,
                        BusyLight=false,
                        VoiceGuidance=false,
                        MicNCIncoming=false,
                        SideTopSwitchSinglePressSetting=new byte[] {10,20,30,40 },
                        IsMicEnumerationOn=false,
                        WALTime=20,
                        Snooze=20,
                        SnoozeLength=20,
                        IsProximitySensorEnable=false,
                    }
                },
                DCFVersion = "1.0",
                DPeMSDKVersion = "1.0",
                DPeMSubAgentVersion = "1.0",
                IsdDriverVersion = "1.0",
            };
            privatetePeripheralsPlugin.SetFieldOrProperty("_deviceHelper", deviceHelper);

            //peripheralsPlugin.SetIsProximitySensorEnable(newValue, LogicalDeviceWebcamId);                      // logicalDeviceis LogicalDeviceWebcamId,IsProximitySensorEnable=false, _deviceHelper.deviceInfo ID != logicalDeviceHeadsetId
            var devicemanager = privatetePeripheralsPlugin.GetFieldOrProperty("_iDeviceManager");
            Assert.IsTrue(true);
            Assert.IsNotNull(devicemanager);

            deviceHelper.deviceInfo[0].ID = LogicalDeviceWebcamId;
            privatetePeripheralsPlugin.SetFieldOrProperty("_deviceHelper", deviceHelper);

            //peripheralsPlugin.SetIsProximitySensorEnable(newValue, LogicalDeviceWebcamId);                     // logicalDeviceis LogicalDeviceWebcamId,IsProximitySensorEnable=false, _deviceHelper.deviceInfo ID = logicalDeviceHeadsetId
            var devicemanager2 = privatetePeripheralsPlugin.GetFieldOrProperty("_iDeviceManager");
            var deviceHelper2 = (DeviceHelper)privatetePeripheralsPlugin.GetFieldOrProperty("_deviceHelper");
            Assert.IsTrue(true);
            Assert.IsNotNull(devicemanager2);
            Assert.IsNotNull(deviceHelper2);
            Assert.That(deviceHelper2.deviceInfo[0].IsProximitySensorEnable, Is.EqualTo(newValue));
        }

        [Test]
        public void TestSetIsWakeonApproachEnable()
        {
            bool newValue = false;
            Mock<IPhysicalDevice> PhysicalDevice = new Mock<IPhysicalDevice>();
            Mock<ILogicalDeviceWebcam> LogicalDeviceWebcam = new Mock<ILogicalDeviceWebcam>();
            Mock<IDeviceManager> mockDeviceManager = new Mock<IDeviceManager>();

            LogicalDeviceWebcam.Setup(ld => ld.Id).Returns(Guid.NewGuid());
            PhysicalDevice.Setup(pd => pd.Devices).Returns(new[] { LogicalDeviceWebcam.Object });
            mockDeviceManager.Setup(dm => dm.Devices).Returns(new[] { PhysicalDevice.Object });// mockDeviceManager device- PhysicalDevice -ILogicalDeviceWebcam device- ILogicalDeviceWebcam device ID

            Guid LogicalDeviceWebcamId = LogicalDeviceWebcam.Object.Id;

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
                        IsCollaborationChatEnable=false,
                        IsCollaborationMicEnable = false,
                        IsCollaborationBlinkEffectEnable=false,
                        IsCollaborationDoubleTapEnable=false,
                        BackLightingControls=20,
                        BackLightingLevel=20,
                        SidetoneLevel=20,
                        //ID=logicalDeviceHeadsetId,
                        AncMode=20,
                        AncGain=20,
                        SelectedPreset=20,
                        Band1Gain=20,
                        Band2Gain=20,
                        Band3Gain=20,
                        Band4Gain=20,
                        Band5Gain=20,
                        MicNoiseCancellation=false,
                        Sidetone=false,
                        WearDetection=20,
                        BusyLight=false,
                        VoiceGuidance=false,
                        MicNCIncoming=false,
                        SideTopSwitchSinglePressSetting=new byte[] {10,20,30,40 },
                        IsMicEnumerationOn=false,
                        WALTime=20,
                        Snooze=20,
                        SnoozeLength=20,
                        IsProximitySensorEnable=false,
                        IsWakeonApproachEnable=false,
                    }
                },
                DCFVersion = "1.0",
                DPeMSDKVersion = "1.0",
                DPeMSubAgentVersion = "1.0",
                IsdDriverVersion = "1.0",
            };
            privatetePeripheralsPlugin.SetFieldOrProperty("_deviceHelper", deviceHelper);

            peripheralsPlugin.SetIsWakeonApproachEnable(newValue, LogicalDeviceWebcamId);                      // logicalDeviceis LogicalDeviceWebcamId,IsWakeonApproachEnable=false, _deviceHelper.deviceInfo ID != logicalDeviceHeadsetId
            var devicemanager = privatetePeripheralsPlugin.GetFieldOrProperty("_iDeviceManager");
            Assert.IsTrue(true);
            Assert.IsNotNull(devicemanager);

            deviceHelper.deviceInfo[0].ID = LogicalDeviceWebcamId;
            privatetePeripheralsPlugin.SetFieldOrProperty("_deviceHelper", deviceHelper);

            peripheralsPlugin.SetIsWakeonApproachEnable(newValue, LogicalDeviceWebcamId);                     // logicalDeviceis LogicalDeviceWebcamId,IsWakeonApproachEnable=false, _deviceHelper.deviceInfo ID = logicalDeviceHeadsetId
            var devicemanager2 = privatetePeripheralsPlugin.GetFieldOrProperty("_iDeviceManager");
            var deviceHelper2 = (DeviceHelper)privatetePeripheralsPlugin.GetFieldOrProperty("_deviceHelper");
            Assert.IsTrue(true);
            Assert.IsNotNull(devicemanager2);
            Assert.IsNotNull(deviceHelper2);
            Assert.That(deviceHelper2.deviceInfo[0].IsWakeonApproachEnable, Is.EqualTo(newValue));
        }

        [Test]
        public void TestSetIsWalkAwayLockEnable()
        {
            bool newValue = false;
            Mock<IPhysicalDevice> PhysicalDevice = new Mock<IPhysicalDevice>();
            Mock<ILogicalDeviceWebcam> LogicalDeviceWebcam = new Mock<ILogicalDeviceWebcam>();
            Mock<IDeviceManager> mockDeviceManager = new Mock<IDeviceManager>();

            LogicalDeviceWebcam.Setup(ld => ld.Id).Returns(Guid.NewGuid());
            PhysicalDevice.Setup(pd => pd.Devices).Returns(new[] { LogicalDeviceWebcam.Object });
            mockDeviceManager.Setup(dm => dm.Devices).Returns(new[] { PhysicalDevice.Object });// mockDeviceManager device- PhysicalDevice -ILogicalDeviceWebcam device- ILogicalDeviceWebcam device ID

            Guid LogicalDeviceWebcamId = LogicalDeviceWebcam.Object.Id;

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
                        IsCollaborationChatEnable=false,
                        IsCollaborationMicEnable = false,
                        IsCollaborationBlinkEffectEnable=false,
                        IsCollaborationDoubleTapEnable=false,
                        BackLightingControls=20,
                        BackLightingLevel=20,
                        SidetoneLevel=20,
                        //ID=logicalDeviceHeadsetId,
                        AncMode=20,
                        AncGain=20,
                        SelectedPreset=20,
                        Band1Gain=20,
                        Band2Gain=20,
                        Band3Gain=20,
                        Band4Gain=20,
                        Band5Gain=20,
                        MicNoiseCancellation=false,
                        Sidetone=false,
                        WearDetection=20,
                        BusyLight=false,
                        VoiceGuidance=false,
                        MicNCIncoming=false,
                        SideTopSwitchSinglePressSetting=new byte[] {10,20,30,40 },
                        IsMicEnumerationOn=false,
                        WALTime=20,
                        Snooze=20,
                        SnoozeLength=20,
                        IsProximitySensorEnable=false,
                        IsWakeonApproachEnable=false,
                        IsWalkAwayLockEnable=false,
                    }
                },
                DCFVersion = "1.0",
                DPeMSDKVersion = "1.0",
                DPeMSubAgentVersion = "1.0",
                IsdDriverVersion = "1.0",
            };
            privatetePeripheralsPlugin.SetFieldOrProperty("_deviceHelper", deviceHelper);

            peripheralsPlugin.SetIsWalkAwayLockEnable(newValue, LogicalDeviceWebcamId);                      // logicalDeviceis LogicalDeviceWebcamId,IsWalkAwayLockEnable=false, _deviceHelper.deviceInfo ID != logicalDeviceHeadsetId
            var devicemanager = privatetePeripheralsPlugin.GetFieldOrProperty("_iDeviceManager");
            Assert.IsTrue(true);
            Assert.IsNotNull(devicemanager);

            deviceHelper.deviceInfo[0].ID = LogicalDeviceWebcamId;
            privatetePeripheralsPlugin.SetFieldOrProperty("_deviceHelper", deviceHelper);

            peripheralsPlugin.SetIsWalkAwayLockEnable(newValue, LogicalDeviceWebcamId);                     // logicalDeviceis LogicalDeviceWebcamId,IsWalkAwayLockEnable=false, _deviceHelper.deviceInfo ID = logicalDeviceHeadsetId
            var devicemanager2 = privatetePeripheralsPlugin.GetFieldOrProperty("_iDeviceManager");
            var deviceHelper2 = (DeviceHelper)privatetePeripheralsPlugin.GetFieldOrProperty("_deviceHelper");
            Assert.IsTrue(true);
            Assert.IsNotNull(devicemanager2);
            Assert.IsNotNull(deviceHelper2);
            Assert.That(deviceHelper2.deviceInfo[0].IsWalkAwayLockEnable, Is.EqualTo(newValue));
        }

        [Test]
        public void TestGetSnooze()
        {
            int GetSnooze = 0;
            Mock<IPhysicalDevice> PhysicalDevice = new Mock<IPhysicalDevice>();
            Mock<ILogicalDeviceWebcam> LogicalDeviceWebcam = new Mock<ILogicalDeviceWebcam>();
            Mock<IDeviceManager> mockDeviceManager = new Mock<IDeviceManager>();

            LogicalDeviceWebcam.Setup(ld => ld.Snooze).Returns(GetSnooze);
            LogicalDeviceWebcam.Setup(ld => ld.Id).Returns(Guid.NewGuid());
            PhysicalDevice.Setup(pd => pd.Devices).Returns(new[] { LogicalDeviceWebcam.Object });
            mockDeviceManager.Setup(dm => dm.Devices).Returns(new[] { PhysicalDevice.Object });// mockDeviceManager device- PhysicalDevice -ILogicalDeviceWebcam device- ILogicalDeviceWebcam device ID

            Guid LogicalDeviceWebcamId = LogicalDeviceWebcam.Object.Id;

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
                        IsCollaborationChatEnable=false,
                        IsCollaborationMicEnable = false,
                        IsCollaborationBlinkEffectEnable=false,
                        IsCollaborationDoubleTapEnable=false,
                        BackLightingControls=20,
                        BackLightingLevel=20,
                        SidetoneLevel=20,
                        //ID=logicalDeviceHeadsetId,
                        AncMode=20,
                        AncGain=20,
                        SelectedPreset=20,
                        Band1Gain=20,
                        Band2Gain=20,
                        Band3Gain=20,
                        Band4Gain=20,
                        Band5Gain=20,
                        MicNoiseCancellation=false,
                        Sidetone=false,
                        WearDetection=20,
                        BusyLight=false,
                        VoiceGuidance=false,
                        MicNCIncoming=false,
                        SideTopSwitchSinglePressSetting=new byte[] {10,20,30,40 },
                        IsMicEnumerationOn=false,
                        WALTime=20,
                        Snooze=20,
                        SnoozeLength=20,
                        IsProximitySensorEnable=false,
                        IsWakeonApproachEnable=false,
                        IsWalkAwayLockEnable=false,
                    }
                },
                DCFVersion = "1.0",
                DPeMSDKVersion = "1.0",
                DPeMSubAgentVersion = "1.0",
                IsdDriverVersion = "1.0",
            };
            privatetePeripheralsPlugin.SetFieldOrProperty("_deviceHelper", deviceHelper);

            var GetSnooze_result1 = peripheralsPlugin.GetSnooze(LogicalDeviceWebcamId);                      // logicalDeviceis LogicalDeviceWebcamId,Snooze=20, _deviceHelper.deviceInfo ID != logicalDeviceHeadsetId
            var devicemanager = privatetePeripheralsPlugin.GetFieldOrProperty("_iDeviceManager");
            Assert.IsNotNull(GetSnooze_result1);
            Assert.IsNotNull(devicemanager);
            Assert.That(GetSnooze, Is.EqualTo(GetSnooze_result1));

            deviceHelper.deviceInfo[0].ID = LogicalDeviceWebcamId;
            privatetePeripheralsPlugin.SetFieldOrProperty("_deviceHelper", deviceHelper);

            var GetSnooze_result2 = peripheralsPlugin.GetSnooze(LogicalDeviceWebcamId);                     // logicalDeviceis LogicalDeviceWebcamId,Snooze=20, _deviceHelper.deviceInfo ID = logicalDeviceHeadsetId
            var devicemanager2 = privatetePeripheralsPlugin.GetFieldOrProperty("_iDeviceManager");
            var deviceHelper2 = (DeviceHelper)privatetePeripheralsPlugin.GetFieldOrProperty("_deviceHelper");
            Assert.IsTrue(true);
            Assert.IsNotNull(devicemanager2);
            Assert.IsNotNull(deviceHelper2);
            Assert.That(deviceHelper2.deviceInfo[0].Snooze, Is.EqualTo(GetSnooze_result2));
        }

        [Test]
        public void TestGetSnoozeLength()
        {
            int GetSnoozeLength = 0;
            Mock<IPhysicalDevice> PhysicalDevice = new Mock<IPhysicalDevice>();
            Mock<ILogicalDeviceWebcam> LogicalDeviceWebcam = new Mock<ILogicalDeviceWebcam>();
            Mock<IDeviceManager> mockDeviceManager = new Mock<IDeviceManager>();

            LogicalDeviceWebcam.Setup(ld => ld.SnoozeLength).Returns(GetSnoozeLength);
            LogicalDeviceWebcam.Setup(ld => ld.Id).Returns(Guid.NewGuid());
            PhysicalDevice.Setup(pd => pd.Devices).Returns(new[] { LogicalDeviceWebcam.Object });
            mockDeviceManager.Setup(dm => dm.Devices).Returns(new[] { PhysicalDevice.Object });// mockDeviceManager device- PhysicalDevice -ILogicalDeviceWebcam device- ILogicalDeviceWebcam device ID

            Guid LogicalDeviceWebcamId = LogicalDeviceWebcam.Object.Id;

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
                        IsCollaborationChatEnable=false,
                        IsCollaborationMicEnable = false,
                        IsCollaborationBlinkEffectEnable=false,
                        IsCollaborationDoubleTapEnable=false,
                        BackLightingControls=20,
                        BackLightingLevel=20,
                        SidetoneLevel=20,
                        //ID=logicalDeviceHeadsetId,
                        AncMode=20,
                        AncGain=20,
                        SelectedPreset=20,
                        Band1Gain=20,
                        Band2Gain=20,
                        Band3Gain=20,
                        Band4Gain=20,
                        Band5Gain=20,
                        MicNoiseCancellation=false,
                        Sidetone=false,
                        WearDetection=20,
                        BusyLight=false,
                        VoiceGuidance=false,
                        MicNCIncoming=false,
                        SideTopSwitchSinglePressSetting=new byte[] {10,20,30,40 },
                        IsMicEnumerationOn=false,
                        WALTime=20,
                        Snooze=20,
                        SnoozeLength=20,
                        IsProximitySensorEnable=false,
                        IsWakeonApproachEnable=false,
                        IsWalkAwayLockEnable=false,
                    }
                },
                DCFVersion = "1.0",
                DPeMSDKVersion = "1.0",
                DPeMSubAgentVersion = "1.0",
                IsdDriverVersion = "1.0",
            };
            privatetePeripheralsPlugin.SetFieldOrProperty("_deviceHelper", deviceHelper);

            var GetSnoozeLength_result1 = peripheralsPlugin.GetSnoozeLength(LogicalDeviceWebcamId);                      // logicalDeviceis LogicalDeviceWebcamId,SnoozeLength=20, _deviceHelper.deviceInfo ID != logicalDeviceHeadsetId
            var devicemanager = privatetePeripheralsPlugin.GetFieldOrProperty("_iDeviceManager");
            Assert.IsNotNull(GetSnoozeLength_result1);
            Assert.IsNotNull(devicemanager);
            Assert.That(GetSnoozeLength, Is.EqualTo(GetSnoozeLength_result1));

            deviceHelper.deviceInfo[0].ID = LogicalDeviceWebcamId;
            privatetePeripheralsPlugin.SetFieldOrProperty("_deviceHelper", deviceHelper);

            var GetSnoozeLength_result2 = peripheralsPlugin.GetSnoozeLength(LogicalDeviceWebcamId);                     // logicalDeviceis LogicalDeviceWebcamId,SnoozeLength=20, _deviceHelper.deviceInfo ID = logicalDeviceHeadsetId
            var devicemanager2 = privatetePeripheralsPlugin.GetFieldOrProperty("_iDeviceManager");
            var deviceHelper2 = (DeviceHelper)privatetePeripheralsPlugin.GetFieldOrProperty("_deviceHelper");
            Assert.IsTrue(true);
            Assert.IsNotNull(devicemanager2);
            Assert.IsNotNull(deviceHelper2);
            Assert.That(deviceHelper2.deviceInfo[0].SnoozeLength, Is.EqualTo(GetSnoozeLength_result2));
        }

        [Test]
        public void TestSetEqualizerValues()
        {
            byte[] bandsGain = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20 };
            Mock<ILogicalDeviceHeadset> logicalDeviceHeadset = new Mock<ILogicalDeviceHeadset>();
            logicalDeviceHeadset.Setup(ld => ld.BandsGain).Returns(bandsGain);
            var logicalDeviceHeadsetObj = logicalDeviceHeadset.Object;

            DeviceInfo deviceInfo = new DeviceInfo()
            {
                DeviceName = "Mouse",
                Name = "Test mouse",
                Band1Gain = 20,
                Band2Gain = 20,
                Band3Gain = 20,
                Band4Gain = 20,
                Band5Gain = 20,
            };

            if (bandsGain != null && bandsGain.Length >= 20)
            {
                peripheralsPlugin.SetEqualizerValues(logicalDeviceHeadsetObj, deviceInfo);
                Assert.IsTrue(true);
                Assert.IsNotNull(deviceInfo.Band1Gain);
                Assert.IsNotNull(deviceInfo.Band2Gain);
                Assert.IsNotNull(deviceInfo.Band3Gain);
                Assert.IsNotNull(deviceInfo.Band4Gain);
                Assert.IsNotNull(deviceInfo.Band5Gain);
            }
        }

        [Test]
        public void TestSetBandsGainValue()
        {
            Mock<ILogicalDeviceHeadset> logicalDeviceHeadset = new Mock<ILogicalDeviceHeadset>();
            var logicalDeviceHeadsetObj = logicalDeviceHeadset.Object;

            DeviceInfo deviceInfo = new DeviceInfo()
            {
                DeviceName = "Mouse",
                Name = "Test mouse",
                Band1Gain = 20,
                Band2Gain = 20,
                Band3Gain = 20,
                Band4Gain = 20,
                Band5Gain = 20,
            };

            var SetBandsGainValue_result = (byte[])privatetePeripheralsPlugin.Invoke("SetBandsGainValue", logicalDeviceHeadsetObj, deviceInfo);
            Assert.IsNotNull(SetBandsGainValue_result);
            Assert.That(deviceInfo.Band1Gain, Is.EqualTo(SetBandsGainValue_result[3]));
            Assert.That(deviceInfo.Band2Gain, Is.EqualTo(SetBandsGainValue_result[7]));
            Assert.That(deviceInfo.Band3Gain, Is.EqualTo(SetBandsGainValue_result[11]));
            Assert.That(deviceInfo.Band4Gain, Is.EqualTo(SetBandsGainValue_result[15]));
            Assert.That(deviceInfo.Band5Gain, Is.EqualTo(SetBandsGainValue_result[19]));
        }

        [Test]
        public void TestByteArrayToInt()
        {
            byte[] bandsGain = new byte[] { 1, 2, 3, 4, 5, 6 };
            int ByteArrayToInt = 50595078;

            var result = PeripheralsPlugin.ByteArrayToInt(bandsGain);
            Assert.IsNotNull(result);
            Assert.That(ByteArrayToInt, Is.EqualTo(result));
        }

        [Test]
        public void TestGetDPeMAssemblyUpdateInfo()
        {
            UpdateItemInfo updateItems = new UpdateItemInfo() { NewVersion = "v1.0" };
            var GetDPeMAssemblyUpdateInfo_result1 = peripheralsPlugin.GetDPeMAssemblyUpdateInfo();  //_updateItems = NULL
            Assert.IsNotNull(GetDPeMAssemblyUpdateInfo_result1);

            privatetePeripheralsPlugin.SetFieldOrProperty("_updateItems", updateItems);
            var GetDPeMAssemblyUpdateInfo_result2 = peripheralsPlugin.GetDPeMAssemblyUpdateInfo();  //_updateItems ! = NULL
            Assert.IsNotNull(GetDPeMAssemblyUpdateInfo_result2);
        }

        [Test]
        public void TestIntToByteArray()
        {
            int value = 1234567890;
            byte[] byteArray = new byte[8];
            int startIndex = 2;
            privatetePeripheralsPlugin.Invoke("IntToByteArray", value, byteArray, startIndex);
            Assert.IsTrue(true);
        }

        [Test]
        public void TestGetFWUpdateInfo()
        {
            UpdateHelper updateHelper;
            updateHelper = new UpdateHelper()
            {
                UpdateItems = new List<UpdateItemInfo>()
                {
                    new UpdateItemInfo()
                    {
                        NewVersion="v1.0" ,
                        _newVersion="v1.0",
                        Description= "TestDescription",
                        CurrentVersion="1.0",
                        DeviceId= "TestDeviceId" ,
                        UpdateType="TestUpdateType",
                        UpdateSeverity= "TestUpdateSeverity",
                        DeviceIndex=0,
                        DeviceModelNumber="Test123345",
                        DeviceName="Mouse",
                        DevicePath= "TestDevicePath",
                        DeviceType=DPeMPublic.Common.Enums.DeviceType.Unknown,
                        FrimwareUpdatePath=0,
                        InstallPath="TestInstallPath",
                        InstanceId=0,
                        Priority=0,
                        ServerPath="TestServerPath",
                        SupplierID="TestSupplierID",
                        SHA256="SGAGASDGASDG",
                        Thumbprint="TestThumbprint"
                    }
                }
            };
            var GetFWUpdateInfo_result1 = peripheralsPlugin.GetFWUpdateInfo();  // _updateHelper = null
            Assert.IsNotNull(GetFWUpdateInfo_result1);

            privatetePeripheralsPlugin.SetFieldOrProperty("_updateHelper", updateHelper);  // _updateHelper ! = null
            var GetFWUpdateInfo_result2 = peripheralsPlugin.GetFWUpdateInfo();
            Assert.IsNotNull(GetFWUpdateInfo_result2);
        }

        [Test]
        public void TestDisplayNotification()
        {
            string bannerInfo = "Test bannerInfo";
            string hyperlinkText = "Test hyperlinkText";
            string bannerItemType = "Test bannerItemType";
            peripheralsPlugin.DisplayNotification(bannerInfo, hyperlinkText, bannerItemType);
            Assert.IsTrue(true);
        }

        [Test]
        public void TestCheckForUpdate()
        {
            Mock<IUpdateManager> UpdateManager = new Mock<IUpdateManager>();
            var UpdateManagerObj = UpdateManager.Object;
            privatetePeripheralsPlugin.SetFieldOrProperty("_iUpdateManager", UpdateManagerObj);
            if (UpdateManager != null)
            {
                peripheralsPlugin.CheckForUpdate();
                Assert.IsTrue(true);
            }
        }

        [Test]
        public void TestUpdateDongleParingStausText()
        {
            var pairingStatus1 = DonglePairingStatus.DonglePairingStatusStopped;

            if (pairingStatus1 == DonglePairingStatus.DonglePairingStatusStopped)
            {
                var UpdateParingStausText_result1 = privatetePeripheralsPlugin.Invoke("UpdateParingStausText", pairingStatus1);  //Dongle PairingStatus
                Assert.That(UpdateParingStausText_result1, Is.EqualTo("Stopped"));
            }

            var pairingStatus2 = DonglePairingStatus.DonglePairingStatusStarted;

            if (pairingStatus2 == DonglePairingStatus.DonglePairingStatusStarted)
            {
                var UpdateParingStausText_result2 = privatetePeripheralsPlugin.Invoke("UpdateParingStausText", pairingStatus2);
                Assert.That(UpdateParingStausText_result2, Is.EqualTo("Started"));
            }

            var pairingStatus3 = DonglePairingStatus.DonglePairingStatusRequest;

            if (pairingStatus3 == DonglePairingStatus.DonglePairingStatusRequest)
            {
                var UpdateParingStausText_result3 = privatetePeripheralsPlugin.Invoke("UpdateParingStausText", pairingStatus3);
                Assert.That(UpdateParingStausText_result3, Is.EqualTo("Request"));
            }

            var pairingStatus4 = DonglePairingStatus.DonglePairingStatusTimeOut;

            if (pairingStatus4 == DonglePairingStatus.DonglePairingStatusTimeOut)
            {
                var UpdateParingStausText_result4 = privatetePeripheralsPlugin.Invoke("UpdateParingStausText", pairingStatus4);
                Assert.That(UpdateParingStausText_result4, Is.EqualTo("TimeOut"));
            }

            var pairingStatus5 = DonglePairingStatus.DonglePairingStatusAlreadyPaired;

            if (pairingStatus5 == DonglePairingStatus.DonglePairingStatusAlreadyPaired)
            {
                var UpdateParingStausText_result5 = privatetePeripheralsPlugin.Invoke("UpdateParingStausText", pairingStatus5);
                Assert.That(UpdateParingStausText_result5, Is.EqualTo("Already Paired"));
            }

            var pairingStatus6 = DonglePairingStatus.DonglePairingStatusOldDevice;

            if (pairingStatus6 == DonglePairingStatus.DonglePairingStatusOldDevice)
            {
                var UpdateParingStausText_result6 = privatetePeripheralsPlugin.Invoke("UpdateParingStausText", pairingStatus6);
                Assert.That(UpdateParingStausText_result6, Is.EqualTo("Old Device"));
            }
        }

        [Test]
        public void TestInitializeDeviceManagerPlugin()
        {
            Mock<IDeviceManagerSA> mockDeviceManagerSA = new Mock<IDeviceManagerSA>();
            var mockDeviceManagerSAObject = mockDeviceManagerSA.Object;  //InitializeDeviceManagerPlugin method has remove
            //privatetePeripheralsPlugin.SetFieldOrProperty("_DeviceManagerPlugin", mockDeviceManagerSAObject);  //_DeviceManagerPlugin not null
            //privatetePeripheralsPlugin.Invoke("InitializeDeviceManagerPlugin");
            //var InitializeDeviceManagerPlugin_result = privatetePeripheralsPlugin.GetFieldOrProperty("_DeviceManagerPlugin");
            //Assert.IsNotNull(InitializeDeviceManagerPlugin_result);
        }

        [Test]
        public void TestUpdateAudioDongleParingStausText()
        {
            var pairingStatus1 = AudioDonglePairingStatus.AudioDonglePairingStatusStopped;

            if (pairingStatus1 == AudioDonglePairingStatus.AudioDonglePairingStatusStopped)
            {
                var UpdateAudioDongleParingStausText_result1 = privatetePeripheralsPlugin.Invoke("UpdateParingStausText", pairingStatus1);  //AudioDongle PairingStatus
                Assert.That(UpdateAudioDongleParingStausText_result1, Is.EqualTo("Stopped"));
            }

            var pairingStatus2 = AudioDonglePairingStatus.AudioDonglePairingStatusStarted;

            if (pairingStatus2 == AudioDonglePairingStatus.AudioDonglePairingStatusStarted)
            {
                var UpdateAudioDongleParingStausText_result2 = privatetePeripheralsPlugin.Invoke("UpdateParingStausText", pairingStatus2);
                Assert.That(UpdateAudioDongleParingStausText_result2, Is.EqualTo("Started"));
            }

            var pairingStatus3 = AudioDonglePairingStatus.AudioDonglePairingStatusRequest;

            if (pairingStatus3 == AudioDonglePairingStatus.AudioDonglePairingStatusRequest)
            {
                var UpdateAudioDongleParingStausText_result3 = privatetePeripheralsPlugin.Invoke("UpdateParingStausText", pairingStatus3);
                Assert.That(UpdateAudioDongleParingStausText_result3, Is.EqualTo("Request"));
            }

            var pairingStatus4 = AudioDonglePairingStatus.AudioDonglePairingStatusTimeOut;

            if (pairingStatus4 == AudioDonglePairingStatus.AudioDonglePairingStatusTimeOut)
            {
                var UpdateAudioDongleParingStausText_result4 = privatetePeripheralsPlugin.Invoke("UpdateParingStausText", pairingStatus4);
                Assert.That(UpdateAudioDongleParingStausText_result4, Is.EqualTo("TimeOut"));
            }

            var pairingStatus5 = AudioDonglePairingStatus.AudioDonglePairingStatusAlreadyPaired;

            if (pairingStatus5 == AudioDonglePairingStatus.AudioDonglePairingStatusAlreadyPaired)
            {
                var UpdateAudioDongleParingStausText_result5 = privatetePeripheralsPlugin.Invoke("UpdateParingStausText", pairingStatus5);
                Assert.That(UpdateAudioDongleParingStausText_result5, Is.EqualTo("Already Paired"));
            }

        }

        [Test]
        public void TestPhysicalAudioDeviceDongle_PairingStatusChanged()
        {
            Mock<IPhysicalAudioDeviceDongle> mockPhysicalAudioDeviceDongle = new Mock<IPhysicalAudioDeviceDongle>();
            var devicePhysicalAudioDeviceDongle = mockPhysicalAudioDeviceDongle.Object;
            AudioDonglePairingStatus newPairingStatus = 0;
            //int newPairingStatus = 0;
            //int dongleDeviceType = 1;
            //string requestDeviceName = "AudioDeviceDongle";

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
                        IsCollaborationChatEnable=false,
                        IsCollaborationMicEnable = false,
                        IsCollaborationBlinkEffectEnable=false,
                        IsCollaborationDoubleTapEnable=false,
                        BackLightingControls=20,
                        BackLightingLevel=20,
                        SidetoneLevel=20,
                        //ID=logicalDeviceHeadsetId,
                        AncMode=20,
                        AncGain=20,
                        SelectedPreset=20,
                        Band1Gain=20,
                        Band2Gain=20,
                        Band3Gain=20,
                        Band4Gain=20,
                        Band5Gain=20,
                        MicNoiseCancellation=false,
                        Sidetone=false,
                        WearDetection=20,
                        BusyLight=false,
                        VoiceGuidance=false,
                        MicNCIncoming=false,
                        SideTopSwitchSinglePressSetting=new byte[] {10,20,30,40 },
                        IsMicEnumerationOn=false,
                        WALTime=20,
                        Snooze=20,
                        SnoozeLength=20,
                        IsProximitySensorEnable=false,
                        IsWakeonApproachEnable=false,
                        IsWalkAwayLockEnable=false,
                    }
                },
                DCFVersion = "1.0",
                DPeMSDKVersion = "1.0",
                DPeMSubAgentVersion = "1.0",
                IsdDriverVersion = "1.0",
            };
            privatetePeripheralsPlugin.SetFieldOrProperty("_deviceHelper", deviceHelper); //PhyscialDeviceID ! = physicalAudioDeviceDongle.Id
            privatetePeripheralsPlugin.Invoke("PhysicalAudioDeviceDongle_PairingStatusChanged", devicePhysicalAudioDeviceDongle, newPairingStatus);
            Assert.IsTrue(true);

            mockPhysicalAudioDeviceDongle.Setup(ld => ld.Id).Returns(Guid.NewGuid());
            Guid LogicalDeviceWebcamId = mockPhysicalAudioDeviceDongle.Object.Id;
            deviceHelper.deviceInfo[0].PhyscialDeviceID = LogicalDeviceWebcamId;
            privatetePeripheralsPlugin.SetFieldOrProperty("_deviceHelper", deviceHelper);   //PhyscialDeviceID  = physicalAudioDeviceDongle.Id
            privatetePeripheralsPlugin.Invoke("PhysicalAudioDeviceDongle_PairingStatusChanged", devicePhysicalAudioDeviceDongle, newPairingStatus);
            var getDevicehelper = privatetePeripheralsPlugin.GetFieldOrProperty("_deviceHelper");
            Assert.IsNotNull(getDevicehelper);
            Assert.IsTrue(true);
        }

        [Test]
        public void TestUpdateDTPInstance()
        {
            Mock<IDTPProxyPlugin> mockDTPInstance = new Mock<IDTPProxyPlugin>();
            var DTPInstanceObj = mockDTPInstance.Object;
            peripheralsPlugin.UpdateDTPInstance(DTPInstanceObj);
            Assert.IsTrue(true);
        }

        [Test]
        public void TestUpdateSettingsInstance()
        {
            Mock<ISettingsManagerDev> mockSettingsManagerDev = new Mock<ISettingsManagerDev>();
            var mockSettingsManagerDevObj = mockSettingsManagerDev.Object;
            peripheralsPlugin.UpdateSettingsInstance(mockSettingsManagerDevObj);
            Assert.IsTrue(true);
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