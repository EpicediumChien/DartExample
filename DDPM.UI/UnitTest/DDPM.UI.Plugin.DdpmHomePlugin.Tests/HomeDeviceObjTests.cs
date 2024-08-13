using DDPM.UI.Common;
using DDPM.UI.Plugin.DdpmHomePlugin.Interfaces;
using DDPM.UI.Plugin.DdpmHomePlugin.Model;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VcpCore.Common;

namespace DDPM.UI.Plugin.DdpmHomePlugin.Tests
{
    [TestFixture, Apartment(ApartmentState.STA)]
    public class HomeDeviceObjTests
    {
        private HomeDeviceObj_Unused? homeDeviceObj_Unused;
        private Mock<IDeviceInfo>? deviceInfoMock;
        private IDeviceInfo? deviceInfo;

        [SetUp]
        public void Setup()
        {
            deviceInfoMock = new Mock<IDeviceInfo>();
            deviceInfo = deviceInfoMock.Object;
            homeDeviceObj_Unused = new HomeDeviceObj_Unused();
        }

        [Test]
        public void TestDeviceImage()
        {
            var deviceImage = DdpmCommonHelper.GetImageSourceFromCommonResource("Resources/Product_KB900.png");
            homeDeviceObj_Unused.DeviceImage = deviceImage;
            Assert.That(homeDeviceObj_Unused.DeviceImage, Is.EqualTo(deviceImage));
        }

        [Test]
        public void TestDeviceName()
        {
            var deviceName = "AA";
            homeDeviceObj_Unused.DeviceName = deviceName;
            Assert.That(homeDeviceObj_Unused.DeviceName, Is.EqualTo(deviceName));
        }

        [Test]
        public void TestDeviceCategory()
        {
            var deviceCategory = new eDeviceCategory();
            homeDeviceObj_Unused.DeviceCategory = deviceCategory;
            Assert.That(homeDeviceObj_Unused.DeviceCategory, Is.EqualTo(deviceCategory));
        }


        [Test]
        public void TestNormalWidth()
        {            
            homeDeviceObj_Unused.NormalWidth = 2.00;
            Assert.That(homeDeviceObj_Unused.NormalWidth, Is.EqualTo(2.00));
        }

        [Test]
        public void TestHoverWidth()
        {
            homeDeviceObj_Unused.NormalWidth = 2.00;
            Assert.That(homeDeviceObj_Unused.HoverWidth, Is.EqualTo(2.00 * 1.0));
        }

        [Test]
        public void TestItemWidth()
        {
            homeDeviceObj_Unused.NormalWidth = 2.00;
            Assert.That(homeDeviceObj_Unused.ItemWidth, Is.EqualTo(2.00 * 1.1));
        }

        [Test]
        public void TestMonitorInfo()
        {
            var monitorInfo = new MonitorInfo();
            homeDeviceObj_Unused.MonitorInfo = monitorInfo;
            Assert.That(homeDeviceObj_Unused.MonitorInfo, Is.EqualTo(monitorInfo));
        }

        [Test]
        public void TestFwVer()
        {
            Assert.That(homeDeviceObj_Unused.FwVer, Is.EqualTo("(N/A)"));
        }

        [Test]
        public void TestServiceTag()
        {
            Assert.That(homeDeviceObj_Unused.ServiceTag, Is.EqualTo("(null)"));

            //MonitorInfo != null
            var monitorInfo = new MonitorInfo() { edid = new VcpCore.Common.EDID() {ServiceTag="11" } };
            homeDeviceObj_Unused.MonitorInfo = monitorInfo;
            Assert.That(homeDeviceObj_Unused.ServiceTag, Is.EqualTo(monitorInfo.edid.ServiceTag));
        }

        [Test]
        public void TestMfgDate()
        {
            Assert.That(homeDeviceObj_Unused.MfgDate, Is.EqualTo("(N/A)"));

            //MonitorInfo != null
            var monitorInfo = new MonitorInfo() { edid = new VcpCore.Common.EDID() { Year = 2024,Month=7} };
            homeDeviceObj_Unused.MonitorInfo = monitorInfo;
            Assert.That(homeDeviceObj_Unused.MfgDate, Is.Not.Null);
            Assert.That(homeDeviceObj_Unused.MfgDate, Is.EqualTo("Jul 2024"));
        }
        

    }
}
