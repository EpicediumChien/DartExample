using DDPM.SA.Common;
using DDPM.UI.Common;
using DPeMPublic.Common.Enums;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DDPM.UI.Plugin.SettingsPlugin.Tests
{
    [TestFixture, Apartment(ApartmentState.STA)]
    public class SettingsPageTests
    {
        SettingsPage? settingsPage;

        [SetUp]
        public void Setup()
        {
            var deviceManagerMock = new Mock<IDeviceManagerSA>();
            var deviceManagerSA = deviceManagerMock.Object;
            DdpmCommonHelper.DeviceManagerSA = deviceManagerSA;
            deviceManagerMock.Setup(x => x.GetFWUpdateInfo(It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<List<DeviceType>>(), It.IsAny<bool>())).Returns(Task.FromResult(new FWUpdateInfoPackage()));
            deviceManagerMock.Setup(x => x.SW_GetSWUpdateInfo(It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>())).Returns(Task.FromResult(new SWUpdateInfoPackage()));
            settingsPage = new SettingsPage();
        }

        [Test]
        public void TestConstructor_SettingsPage()
        {
            Assert.That(settingsPage, Is.Not.Null);
            Assert.That(settingsPage.DataContext, Is.Not.Null);

        }
    }
}
