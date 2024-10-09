using DDPM.SA.Common;
using DDPM.SA.Common.Settings;
using DDPM.UI.Common;
using Dell.Client.Framework.Common;
using DPeMPublic.Common.Enums;
using Moq;

namespace DDPM.UI.Plugin.SettingsPlugin.Tests
{
    [TestFixture, Apartment(ApartmentState.STA)]
    public class SettingsPageTests
    {
        private SettingsPage? settingsPage;

        [SetUp]
        public void Setup()
        {
            var appSettings = new DDPMAppSettings();
            var userSettings = new DDPMUserSettings();
            var lockSettings = new DDPMITConfig();
            var deviceManagerMock = new Mock<IDeviceManagerSA>();
            var deviceManagerSA = deviceManagerMock.Object;
            DdpmCommonHelper.DeviceManagerSA = deviceManagerSA;
            //deviceManagerMock.Setup(x => x.GetFWUpdateInfo(It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<List<DeviceType>>(), It.IsAny<bool>())).Returns(Task.FromResult(new FWUpdateInfoPackage()));
            deviceManagerMock.Setup(x => x.SW_GetSWUpdateInfo(It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>())).Returns(Task.FromResult(new SWUpdateInfoPackage()));
            deviceManagerMock.Setup(x => x.ReloadAppConfigData(It.IsAny<bool>())).Returns(Task.FromResult(new DDPMSettings(appSettings, userSettings, lockSettings)));
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