using DDPM.SA.Common;
using DDPM.UI.Common;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDPM.UI.Plugin.SettingsPlugin.Tests
{
    [Apartment(ApartmentState.STA)]
    public class UpdatesPageTests
    {
        private UpdatesPage? updatesPage;
        private Mock<IDeviceManagerSA>? deviceManagerSAMock;
        private IDeviceManagerSA? deviceManagerSA;


        [SetUp]
        public void Setup()
        {
            deviceManagerSAMock = new Mock<IDeviceManagerSA>();
            deviceManagerSA = deviceManagerSAMock.Object;
            DdpmCommonHelper.DeviceManagerSA = deviceManagerSA;
            updatesPage = new UpdatesPage();
        }

        [Test]
        public void TestConstructor_UpdatesPage()
        {
            Assert.That(updatesPage, Is.Not.Null);
        }
    }
}
