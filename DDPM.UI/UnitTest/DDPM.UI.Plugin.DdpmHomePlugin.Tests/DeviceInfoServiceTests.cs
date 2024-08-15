using DDPM.UI.Plugin.DdpmHomePlugin.Interfaces;
using DDPM.UI.Plugin.DdpmHomePlugin.Model;
using Moq;

namespace DDPM.UI.Plugin.DdpmHomePlugin.Tests
{
    [TestFixture, Apartment(ApartmentState.STA)]
    public class DeviceInfoServiceTests
    {
        private DeviceInfoService? deviceInfoService;
        private Mock<IDeviceInfo>? deviceInfoMock;
        private IDeviceInfo? deviceInfo;

        [SetUp]
        public void Setup()
        {
            deviceInfoMock = new Mock<IDeviceInfo>();
            deviceInfo = deviceInfoMock.Object;
            deviceInfoService = new DeviceInfoService(deviceInfo);
        }

        [Test]
        public void TestGetDeviceInfo()
        {
            var result = deviceInfoService.GetDeviceInfo();
            Assert.IsNotNull(result);
        }
    }
}