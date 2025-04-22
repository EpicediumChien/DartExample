using DDPM.SA.Common;
using DDPM.UI.Common.EAEM;
using DDPM.UI.Common.Interfaces;
using DDPM.UI.Common.Models;
using DDPM.UI.Common.ViewModels;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.UX.WPF;
using DPeMPublic.Common.Enums;
using Moq;
using System.Globalization;
using System.Runtime.Intrinsics.X86;
using System.Security.Policy;
using System.Windows.Media;
using System.Xml.Linq;
using VcpCore.Common;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;

namespace DDPM.UI.Common.Tests
{
    [Apartment(ApartmentState.STA)]
    public class HomeDeviceTests
    {
        private HomeDevice? homeDevice;

        [SetUp]
        public void Setup()
        {
            if (System.Windows.Application.Current == null)
            {
                new System.Windows.Application();
            }
            homeDevice = new HomeDevice();
        }

        [Test]
        public void TestConstructor_HomeDevice()
        {
            // Assert
            Assert.That(homeDevice, Is.Not.Null);
        }

        [Test]
        public void TestDeviceImage()
        {
            // Act
            var deviceImage = DdpmCommonHelper.GetImageSourceFromCommonResource("Resources/Product_Display.png");
            homeDevice.DeviceImage=deviceImage;
            // Assert
            Assert.That(homeDevice.DeviceImage, Is.EqualTo(deviceImage));
        }

        [Test]
        public void TestDeviceName()
        {
            // Act
            homeDevice.DeviceName = "Display 1";
            // Assert
            Assert.That(homeDevice.DeviceName, Is.EqualTo("Display 1"));
        }

        [Test]
        public void TestDeviceCategory()
        {
            // Act
            homeDevice.DeviceCategory = eDeviceCategory.KB;
            // Assert
            Assert.That(homeDevice.DeviceCategory, Is.EqualTo(eDeviceCategory.KB));
        }

        [Test]
        public void TestMonitorInfo()
        {
            // Act
            var monitorInfo = new MonitorInfo();
            homeDevice.MonitorInfo = monitorInfo;
            // Assert
            Assert.That(homeDevice.MonitorInfo, Is.EqualTo(monitorInfo));
        }

        [Test]
        public void TestDeviceInfo()
        {
            // Act
            var deviceInfo = new DeviceInfo() { ID = new Guid(), Name = "A" };
            homeDevice.DeviceInfo = deviceInfo;
            // Assert
            Assert.That(homeDevice.DeviceInfo, Is.EqualTo(deviceInfo));
        }


        [Test]
        public void TestIsSamePeripheralDevice()
        {
            // Act
            var result = homeDevice.IsSamePeripheralDevice(null);
            // Assert
            Assert.That(result, Is.EqualTo(false));

            // Act
            var deviceInfo = new DeviceInfo() { ID = new Guid(),Name="A" };
            homeDevice.DeviceInfo = deviceInfo;
            result = homeDevice.IsSamePeripheralDevice(deviceInfo);
            // Assert
            Assert.That(result, Is.EqualTo(true));
        }

        [Test]
        public void TestNormalWidth()
        {
            // Act
            homeDevice.NormalWidth = 2;
            // Assert
            Assert.That(homeDevice.NormalWidth, Is.EqualTo(2));
        }

        [Test]
        public void TestHoverWidth()
        {
            // Act
            homeDevice.NormalWidth = 2;
            // Assert
            Assert.That(homeDevice.HoverWidth, Is.EqualTo(2.20));
        }

        [Test]
        public void TestItemWidth()
        {
            // Act
            homeDevice.NormalWidth = 2;
            // Assert
            Assert.That(homeDevice.ItemWidth, Is.EqualTo(2.32));
        }

        [Test]
        public void TestFwVer()
        {
            // Assert
            Assert.That(homeDevice.FwVer, Is.EqualTo("(N/A)"));

            // Act
            homeDevice.MonitorInfo = new MonitorInfo() { FwVersion = "1A" };
            // Assert
            Assert.That(homeDevice.FwVer, Is.EqualTo("1A"));
        }

        [Test]
        public void TestServiceTag()
        {
            // Assert
            Assert.That(homeDevice.ServiceTag, Is.EqualTo("(N/A)"));

            // Act
            var edid = new VcpCore.Common.EDID();
            edid.ServiceTag = "2A";
            var monitorinfo= new MonitorInfo() { edid = edid };
            homeDevice.MonitorInfo= monitorinfo;
            // Assert
            Assert.That(homeDevice.ServiceTag, Is.EqualTo("2A"));
        }

        [Test]
        public void TestMfgDate()
        {
            if (CultureInfo.CurrentCulture.Name == "es-US")
            {
                // Assert
                Assert.That(homeDevice.MfgDate, Is.EqualTo("(N/A)"));

                // Act
                var edid = new VcpCore.Common.EDID();
                edid.Year = 2024;
                edid.Month = 1;
                var monitorinfo = new MonitorInfo() { edid = edid };
                homeDevice.MonitorInfo = monitorinfo;
                // Assert
                Assert.That(homeDevice.MfgDate, Is.EqualTo("Jan 2024"));
            }
        }

        [Test]
        public void TestTooltipModelNameKB()
        {
            // Assert
            homeDevice.DeviceCategory = eDeviceCategory.KB;
            var result = homeDevice.TooltipModelName;
            Assert.That(result, Is.EqualTo("KB"));
        }

        [Test]
        public void TestTooltipModelName()
        {
            var monitorinfo = new MonitorInfo() { MarketingName = "" };
            homeDevice.MonitorInfo = monitorinfo;
            var result = homeDevice.TooltipModelName;
            // Assert
            Assert.That(result, Is.EqualTo(""));
        }

        [Test]
        public void TestTooltipModelNameAa()
        {
            // Act
            var monitorinfo = new MonitorInfo() { MarketingName = "Aa" };
            homeDevice.MonitorInfo = monitorinfo;
            var result = homeDevice.TooltipModelName;
            // Assert
            Assert.That(result, Is.EqualTo("Aa"));
        }

        [Test]
        public void TestTooltipModelNameWD19()
        {
            // Act
            var deviceInfo = new DeviceInfo() { Name = "WD19", ModelNumber = "KB7120W" };
            homeDevice.DeviceInfo = deviceInfo;
            var result = homeDevice.TooltipModelName;
            // Assert
            Assert.That(result, Is.EqualTo("WD19 KB740"));
        }

        [Test]
        public void TestTooltipModelNameWD19S()
        {
            // Act
            var deviceInfo = new DeviceInfo() { Name = " WD19S", ModelNumber = "WK717" };
            homeDevice.DeviceInfo = deviceInfo;
            var result = homeDevice.TooltipModelName;
            // Assert
            Assert.That(result, Is.EqualTo(" WD19S"));
        }

        [Test]
        public void TestUpdateBatteryIndicator()
        {
            try
            {
                homeDevice.UpdateBatteryIndicator();
                Assert.True(true);
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }
        }

        [Test]
        public void TestBatteryLevel()
        {
            // Assert
            Assert.That(homeDevice.BatteryLevel, Is.EqualTo(0));

            // Act
            var deviceInfo = new DeviceInfo() { Name = "A", BatteryLevel =2 };
            homeDevice.DeviceInfo = deviceInfo;
            var result = homeDevice.BatteryLevel;
            // Assert
            Assert.That(result, Is.EqualTo(2));
        }

        [Test]
        public void TestBatteryStatus()
        {
            // Assert
            Assert.That(homeDevice.BatteryStatus, Is.EqualTo(string.Empty));

            // Act
            var monitorinfo = new MonitorInfo();
            homeDevice.MonitorInfo = monitorinfo;
            var result = homeDevice.BatteryStatus;
            // Assert
            Assert.That(result, Is.EqualTo(""));

            // Act
            var deviceInfo = new DeviceInfo() {Name ="A", BatteryStatus = "save" };
            homeDevice.DeviceInfo = deviceInfo;
            result = homeDevice.BatteryStatus;
            // Assert
            Assert.That(result, Is.EqualTo("save"));
        }

        [Test]
        public void TestConnectionType()
        {
            // Assert
            Assert.That(homeDevice.ConnectionType, Is.EqualTo(string.Empty));

            // Act
            var monitorinfo = new MonitorInfo();
            homeDevice.MonitorInfo = monitorinfo;
            var result = homeDevice.ConnectionType;
            // Assert
            Assert.That(result, Is.EqualTo("Port"));

            // Act
            var deviceInfo = new DeviceInfo() { Name = "A", BatteryStatus = "save" };
            homeDevice.DeviceInfo = deviceInfo;
            result = homeDevice.ConnectionType;
            // Assert
            Assert.That(result, Is.EqualTo("Unknown"));
        }

        [Test]
        public void TestNoBattery()
        {
            // Assert
            Assert.That(homeDevice.NoBattery, Is.EqualTo(false));

            // Act
            var monitorinfo = new MonitorInfo();
            homeDevice.MonitorInfo = monitorinfo;
            var result = homeDevice.NoBattery;
            // Assert
            Assert.That(result, Is.EqualTo(true));

            // Act
            var deviceInfo = new DeviceInfo() { Name = "A", BatteryStatus = "save" };
            homeDevice.DeviceInfo = deviceInfo;
            result = homeDevice.NoBattery;
            // Assert
            Assert.That(result, Is.EqualTo(true));
        }


        [Test]
        public void TestText1()
        {
            // Assert
            Assert.That(homeDevice.Text1, Is.EqualTo(""));

            // Act
            var monitorinfo = new MonitorInfo() { inputCable = "inputCable" };
            homeDevice.MonitorInfo = monitorinfo;
            var result = homeDevice.Text1;
            // Assert
            Assert.That(result, Is.EqualTo("inputCable"));

            // Act
            var deviceInfo = new DeviceInfo() { Name = "A", BatteryStatus = "save" };
            homeDevice.DeviceInfo = deviceInfo;
            result = homeDevice.Text1;
            // Assert
            Assert.That(result, Is.EqualTo(""));
        }

        [Test]
        public void TestDisplayName()
        {
            // Assert
            Assert.That(homeDevice.DisplayName, Is.EqualTo(""));

            // Act
            var deviceInfo = new DeviceInfo() { Name = "A", ModelNumber = "ModelNumber" };
            homeDevice.DeviceInfo = deviceInfo;
            var result = homeDevice.DisplayName;
            // Assert
            Assert.That(result, Is.EqualTo("ModelNumber"));

            // Act
            var monitorinfo = new MonitorInfo() { inputSource = "inputSource" };
            homeDevice.MonitorInfo = monitorinfo;
            homeDevice.InstanceNo = 1;
            result = homeDevice.DisplayName;
            // Assert
            Assert.That(result, Is.EqualTo(" (1)"));
        }


        [Test]
        public void TestIsSameModel()
        {
            // Act
            var result = homeDevice.IsSameModel(new HomeDevice());
            // Assert
            Assert.That(result, Is.EqualTo(true));
        }

        [Test]
        public void TestIsSameModelstring()
        {
            // Act
            var result = homeDevice.IsSameModel(string.Empty);
            // Assert
            Assert.That(result, Is.EqualTo(true));
        }

        [Test]
        public void TestInstanceNo()
        {
            // Act
            homeDevice.InstanceNo=1;
            // Assert
            Assert.That(homeDevice.InstanceNo, Is.EqualTo(1));
        }

        [Test]
        public void TestIsSamePeripheralModel()
        {
            // Act
            var result = homeDevice.IsSamePeripheralModel(new HomeDevice() { DeviceName= "DeviceName" });
            // Assert
            Assert.That(result, Is.EqualTo(false));

            // Act
            var deviceInfo = new DeviceInfo() { Name = "A", ModelNumber = "ModelNumber" };
            homeDevice.DeviceInfo = deviceInfo;
            var other=new HomeDevice() { DeviceInfo = homeDevice.DeviceInfo };
            result = homeDevice.IsSamePeripheralModel(other);
            // Assert
            Assert.That(result, Is.EqualTo(true));
        }

        [Test]
        public void TestvmEzArrange()
        {
            // Act
            var vmEzArrange = new EzArrangeViewModel(new HomeDevice());
            homeDevice.vmEzArrange=vmEzArrange;
            // Assert
            Assert.That(homeDevice.vmEzArrange, Is.EqualTo(vmEzArrange));
        }

        [Test]
        public void TestCompareTo()
        {
            // Act
            var result = homeDevice.CompareTo(null);
            // Assert
            Assert.That(result, Is.EqualTo(0));

            // Act
            homeDevice.DeviceCategory = eDeviceCategory.KB;
            result = homeDevice.CompareTo(new HomeDevice() { DeviceCategory = eDeviceCategory.KB });
            // Assert
            Assert.That(result, Is.EqualTo(0));

            // Act
            homeDevice.DeviceCategory = eDeviceCategory.Mouse;
            result = homeDevice.CompareTo(new HomeDevice() { DeviceCategory = eDeviceCategory.KB });
            // Assert
            Assert.That(result, Is.EqualTo(1));
        }

        [Test]
        public void TestSortOrder()
        {
            // Act
            homeDevice.SortOrder=1;
            // Assert
            Assert.That(homeDevice.SortOrder, Is.EqualTo(1));
        }

        [Test]
        public void TestDeviceManagerSA()
        {
            // Act
            var deviceManagerSA = new Mock<IDeviceManagerSA>();
            HomeDevice.DeviceManagerSA = deviceManagerSA.Object;
            // Assert
            Assert.That(HomeDevice.DeviceManagerSA, Is.EqualTo(deviceManagerSA.Object));
        }

        [Test]
        public void TestIsConnectionHoverViewShow()
        {
            // Act
            homeDevice.IsConnectionHoverViewShow=true;
            // Assert
            Assert.That(homeDevice.IsConnectionHoverViewShow, Is.EqualTo(true));
        }

        [Test]
        public void TestConnectionHoverMode()
        {
            // Act
            homeDevice.ConnectionHoverMode = "ConnectionHoverMode";
            // Assert
            Assert.That(homeDevice.ConnectionHoverMode, Is.EqualTo("ConnectionHoverMode"));
        }

        [Test]
        public void TestDongleHostText()
        {
            // Act
            if (CultureInfo.CurrentCulture.Name == "es-US")
            {
                // Assert
                Assert.That(homeDevice.DongleHostText, Is.EqualTo("USB Wireless Receiver"));
            }
        }

        [Test]
        public void TestDongleFwVersion()
        {
            // Act
            homeDevice.DongleFwVersion = "DongleFwVersion";
            // Assert
            Assert.That(homeDevice.DongleFwVersion, Is.EqualTo("DongleFwVersion"));
        }

        [Test]
        public void TestDongleSlot()
        {
            // Act
            homeDevice.DongleSlot = "DongleSlot";
            // Assert
            Assert.That(homeDevice.DongleSlot, Is.EqualTo("DongleSlot"));
        }

        [Test]
        public void TestBleHost1Style()
        {
            // Act
            homeDevice.BleHost1Style = "BleHost1Style";
            // Assert
            Assert.That(homeDevice.BleHost1Style, Is.EqualTo("BleHost1Style"));
        }

        [Test]
        public void TestBleHost2Style()
        {
            // Act
            homeDevice.BleHost2Style = "BleHost2Style";
            // Assert
            Assert.That(homeDevice.BleHost2Style, Is.EqualTo("BleHost2Style"));
        }

        [Test]
        public void TestBleHost3Style()
        {
            // Act
            homeDevice.BleHost3Style = "BleHost3Style";
            // Assert
            Assert.That(homeDevice.BleHost3Style, Is.EqualTo("BleHost3Style"));
        }

        [Test]
        public void TestBleHost1Text()
        {
            // Act
            homeDevice.BleHost1Text = "BleHost1Text";
            // Assert
            Assert.That(homeDevice.BleHost1Text, Is.EqualTo("BleHost1Text"));
        }

        [Test]
        public void TestBleHost2Text()
        {
            // Act
            homeDevice.BleHost2Text = "BleHost2Text";
            // Assert
            Assert.That(homeDevice.BleHost2Text, Is.EqualTo("BleHost2Text"));
        }

        [Test]
        public void TestBleHost3Text()
        {
            // Act
            homeDevice.BleHost3Text = "BleHost3Text";
            // Assert
            Assert.That(homeDevice.BleHost3Text, Is.EqualTo("BleHost3Text"));
        }

        [Test]
        public void TestAudioBleText()
        {
            // Act
            homeDevice.AudioBleText = "AudioBleText";
            // Assert
            Assert.That(homeDevice.AudioBleText, Is.EqualTo("AudioBleText"));
        }

        [Test]
        public void TestGetConnectionTypeFromDeviceInfo()
        {
            // Act
            var result = HomeDevice.GetConnectionTypeFromDeviceInfo(new DeviceInfo());
            // Assert
            Assert.That(result, Is.EqualTo("Unknown"));

            // Act
            result = HomeDevice.GetConnectionTypeFromDeviceInfo(new DeviceInfo() { PhysicalDeviceType = DeviceType.PhysicalWebcam });
            // Assert
            Assert.That(result, Is.EqualTo("Wired"));
        }

        [Test]
        public void TestGetDeviceCategoryFromDeviceInfo()
        {
            // Act
            var result = HomeDevice.GetDeviceCategoryFromDeviceInfo(new DeviceInfo() { Type= DeviceType.LogicalKeyboard });
            // Assert
            Assert.That(result, Is.EqualTo(eDeviceCategory.KB));

            // Act
            result = HomeDevice.GetDeviceCategoryFromDeviceInfo(new DeviceInfo());
            // Assert
            Assert.That(result, Is.EqualTo(eDeviceCategory.Unknown));
        }

        [Test]
        public void TestCreateFromDeviceInfo()
        {
            // Act
            var result = HomeDevice.CreateFromDeviceInfo(new DeviceInfo() { Name="A"});
            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result,Is.InstanceOf<HomeDevice>());
        }

        [Test]
        public void TestHasCapability_PipPbp()
        {
            // Act
            var result = homeDevice.HasCapability_PipPbp;
            // Assert
            Assert.That(result, Is.EqualTo(false));

            // Act
            homeDevice.MonitorInfo = new MonitorInfo() { CapabilityDic = new Dictionary<string, List<string>>() };
            result = homeDevice.HasCapability_PipPbp;
            // Assert
            Assert.That(result, Is.EqualTo(false));
        }

        [Test]
        public void TestHasCapability_KVM()
        {
            // Act
            var result = homeDevice.HasCapability_KVM;
            // Assert
            Assert.That(result, Is.EqualTo(false));
        }

        [Test]
        public void TestHasCapability_UsbKvma()
        {
            // Act
            var result = homeDevice.HasCapability_UsbKvm;
            // Assert
            Assert.That(result, Is.EqualTo(false));
        }

        [Test]
        public void TestHasCapability_UsbKvmb()
        {
            // Act
            homeDevice.MonitorInfo = new MonitorInfo() { CapabilityDic = null};
            var result = homeDevice.HasCapability_UsbKvm;
            // Assert
            Assert.That(result, Is.EqualTo(false));
        }

        [Test]
        public void TestHasCapability_UsbKvmc()
        {
            // Act
            var dictionary = new Dictionary<string, List<string>>();
            homeDevice.MonitorInfo = new MonitorInfo() { CapabilityDic = dictionary };
            var result = homeDevice.HasCapability_UsbKvm;
            // Assert
            Assert.That(result, Is.EqualTo(false));
        }

        [Test]
        public void TestHasCapability_UsbKvmd()
        {
            // Act
            var dictionary = new Dictionary<string, List<string>>();
            dictionary.Add("E7", new List<string>());
            homeDevice.MonitorInfo = new MonitorInfo() { CapabilityDic = dictionary };
            var result = homeDevice.HasCapability_UsbKvm;
            // Assert
            Assert.That(result, Is.EqualTo(true));
        }

        [Test]
        public void TestHasCapability_NetworkKvm()
        {
            // Act
            var result = homeDevice.HasCapability_NetworkKvm;
            // Assert
            Assert.That(result, Is.EqualTo(false));
        }

        [Test]
        public void TestHasCapability_Gaming()
        {
            // Act
            var result = homeDevice.HasCapability_Gaming;
            // Assert
            Assert.That(result, Is.EqualTo(false));

            // Act
            var dictionary = new Dictionary<string, List<string>>();
            dictionary.Add("F4", new List<string>());
            homeDevice.MonitorInfo = new MonitorInfo() { CapabilityDic = dictionary };
            result = homeDevice.HasCapability_Gaming;
            // Assert
            Assert.That(result, Is.EqualTo(true));
        }

        [Test]
        public void TestHasCapability_VisionEngine()
        {
            // Act
            var result = homeDevice.HasCapability_VisionEngine;
            // Assert
            Assert.That(result, Is.EqualTo(false));

            // Act
            var dictionary = new Dictionary<string, List<string>>();
            dictionary.Add("EC", new List<string>());
            homeDevice.MonitorInfo = new MonitorInfo() { CapabilityDic = dictionary ,modelName="GS"};
            result = homeDevice.HasCapability_VisionEngine;
            // Assert
            Assert.That(result, Is.EqualTo(true));
        }

        [Test]
        public void TestHasCapability_Contrast()
        {
            // Act
            var result = homeDevice.HasCapability_Contrast;
            // Assert
            Assert.That(result, Is.EqualTo(false));

            // Act
            var dictionary = new Dictionary<string, List<string>>();
            dictionary.Add("12", new List<string>());
            homeDevice.MonitorInfo = new MonitorInfo() { CapabilityDic = dictionary };
            result = homeDevice.HasCapability_Contrast;
            // Assert
            Assert.That(result, Is.EqualTo(true));
        }

        [Test]
        public void TestIsSameMonitor()
        {
            // Act
            MonitorInfo mi1 = new MonitorInfo() {AliasDeviceName = "mi1" };
            MonitorInfo mi2 = new MonitorInfo() { };
            var result = HomeDevice.IsSameMonitor(mi1, mi2, "");
            // Assert
            Assert.That(result, Is.EqualTo(false));

            // Act
            mi1 = new MonitorInfo() {  };
            mi2 = new MonitorInfo() { };
            result = HomeDevice.IsSameMonitor(mi1, mi2, "");
            // Assert
            Assert.That(result, Is.EqualTo(true));

        }

        [Test]
        public void TestDumpInfoToLog()
        {
            try
            {
                homeDevice.DumpInfoToLog(null);
                Assert.True(true);
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }

            var logMock = new Mock<ILog>();
            var log = logMock.Object;
            homeDevice.DeviceInfo = new DeviceInfo() { Name = "Name" };
            try
            {
                homeDevice.DumpInfoToLog(log);
                Assert.True(true);
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }

            homeDevice.MonitorInfo = new MonitorInfo() { MarketingName = "MarketingName" };
            try
            {
                homeDevice.DumpInfoToLog(log);
                Assert.True(true);
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }
        }

        [Test]
        public void TestLandingMarketName()
        {
            if (CultureInfo.CurrentCulture.Name == "es-US")
            {
                // Act
                var result = homeDevice.LandingMarketName;
                // Assert
                Assert.That(result, Is.EqualTo("(Noname)"));

                // Act
                homeDevice.DeviceInfo = new DeviceInfo() { Name = "Name" };
                result = homeDevice.LandingMarketName;
                // Assert
                Assert.That(result, Is.EqualTo("Name"));

                // Act
                homeDevice.MonitorInfo = new MonitorInfo();
                result = homeDevice.LandingMarketName;
                // Assert
                Assert.That(result, Is.EqualTo("Display"));
                // Act

                homeDevice.MonitorInfo = new MonitorInfo() { MarketingName = "MarketingName" };
                result = homeDevice.LandingMarketName;
                // Assert
                Assert.That(result, Is.EqualTo("MarketingName"));
            }
        }

        
    }
}