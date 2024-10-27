using CommunityToolkit.Mvvm.DependencyInjection;
using DDPM.SA.Common;
using DDPM.SA.Common.Settings;
using DDPM.UI.Common;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.UX.WPF;
using DPeMPublic.Common.Enums;
using Microsoft.Extensions.DependencyInjection;
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
            deviceManagerMock.Setup(x => x.GetFWUpdateInfo(It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<List<DeviceType>>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>())).Returns(Task.FromResult(new FWUpdateInfoPackage()));
            deviceManagerMock.Setup(x => x.SW_GetSWUpdateInfo(It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>())).Returns(Task.FromResult(new SWUpdateInfoPackage()));
            deviceManagerMock.Setup(x => x.ReloadAppConfigData(It.IsAny<bool>())).Returns(Task.FromResult(new DDPMSettings(appSettings, userSettings, lockSettings)));
            //var consoleMock=new Mock<IConsole>();
            //var gearMenuMock=new Mock<IGearMenu>();
            var ISettingsPageViewModelMock=new Mock<ISettingsPageViewModel>();
            var PluginIocMock=new Mock<IServiceProvider>();

            PluginIocMock.Setup(x=>x.GetService(It.IsAny<Type>())).Returns(new SettingsPageViewModel());
            //PluginIoc:read only
            //settingsPage = new SettingsPage();
        }

        [Test]
        public void TestConstructor_SettingsPage()
        {
            //Assert.That(settingsPage, Is.Not.Null);
            //Assert.That(settingsPage.DataContext, Is.Not.Null);
        }
    }
}