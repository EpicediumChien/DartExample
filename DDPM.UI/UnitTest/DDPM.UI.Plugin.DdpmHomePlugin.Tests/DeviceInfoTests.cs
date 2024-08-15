using DDPM.UI.Common;
using DDPM.UI.Plugin.DdpmHomePlugin.Model;

namespace DDPM.UI.Plugin.DdpmHomePlugin.Tests
{
    [TestFixture, Apartment(ApartmentState.STA)]
    public class DeviceInfoTests
    {
        private DeviceInfo_Unused? deviceInfo_Unused;

        [SetUp]
        public void Setup()
        {
            deviceInfo_Unused = new DeviceInfo_Unused();
        }

        [Test]
        public void TestDeviceImage()
        {
            var deviceImage = DdpmCommonHelper.GetImageSourceFromCommonResource("Resources/Product_KB900.png");
            deviceInfo_Unused.DeviceImage = deviceImage;
            Assert.That(deviceInfo_Unused.DeviceImage, Is.EqualTo(deviceImage));
        }

        [Test]
        public void TestDeviceName()
        {
            var deviceName = "AA";
            deviceInfo_Unused.DeviceName = deviceName;
            Assert.That(deviceInfo_Unused.DeviceName, Is.EqualTo(deviceName));
        }

        [Test]
        public void TestDeviceCategory()
        {
            var deviceCategory = new eDeviceCategory();
            deviceInfo_Unused.DeviceCategory = deviceCategory;
            Assert.That(deviceInfo_Unused.DeviceCategory, Is.EqualTo(deviceCategory));
        }
    }
}