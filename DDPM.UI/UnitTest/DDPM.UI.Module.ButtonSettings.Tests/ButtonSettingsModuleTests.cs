using DDPM.SA.Common;
using DDPM.UI.Common;
using DDPM.UI.Common.Interfaces;
using DDPM.UI.Common.Models;
using DDPM.UI.Plugin.ViewModels;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.UX.WPF;
using Moq;
using Newtonsoft.Json.Linq;
using NGA.UnitTest.PrivateObject;
using NUnit.Framework;
using System.Windows;
using System.Windows.Controls;
using Dell.Client.Framework.UX.WPF.ResourceManager;
namespace DDPM.UI.Module.ButtonSettings.Tests
{
    [TestFixture, Apartment(ApartmentState.STA)]
    public class ButtonSettingsModuleTests
    {
        private ButtonSettingsModule? buttonSettingsModule;
        private Mock<IConsole>? consoleMock;
        private Mock<ILog>? logMock;
        private Mock<IDeviceManagerSA>? deviceManagerMock;
        private MouseViewModel? mouseViewModel;
        private PrivateObject? privateObject;

        [SetUp]
        public void SetUp()
        {
            if (System.Windows.Application.Current == null)
            {
                new System.Windows.Application();
            }
            ResourceManager res = new ResourceManager();
            var resourceDictionary = new ResourceDictionary();
            resourceDictionary.Source = new Uri("pack://application:,,,/DDPM.UI.Common;component/ModuleStyle.xaml");
            System.Windows.Application.Current.Resources.MergedDictionaries.Add(resourceDictionary);
            consoleMock = new Mock<IConsole>();
            logMock = new Mock<ILog>();
            deviceManagerMock = new Mock<IDeviceManagerSA>();
            DdpmCommonHelper.DeviceManagerSA = deviceManagerMock.Object;
            mouseViewModel = new MouseViewModel(consoleMock.Object, logMock.Object);
            buttonSettingsModule = new ButtonSettingsModule(mouseViewModel);
            privateObject = new PrivateObject(buttonSettingsModule);
            DdpmCommonHelper.DeviceManagerSA= deviceManagerMock.Object;
            deviceManagerMock.Setup(x => x.SetCurrentSelectedAppSpecificProfile(It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult(""));
        }

        [Test]
        public void TestModuleName()
        {
            // Act
            var result = buttonSettingsModule!.ModuleName;

            // Assert
            Assert.That(result, Is.EqualTo("ButtonSettingsModule"));
        }

        [Test]
        public void TestGetLeftView()
        {
            // Act
            var result = buttonSettingsModule!.GetLeftView();

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public void TestGetRightView()
        {
            // Act
            var result = buttonSettingsModule!.GetRightView();

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<UserControl>());
        }

        [Test]
        public void TestSelectedHomeDevice()
        {
            // Arrange
            var homeDevice = new HomeDevice();

            // Act
            buttonSettingsModule!.SelectedHomeDevice = homeDevice;
            var result = buttonSettingsModule.SelectedHomeDevice;

            // Assert
            Assert.That(result, Is.EqualTo(homeDevice));
        }

        [Test]
        public void TestModuleOwner()
        {
            // Arrange
            var moduleOwnerMock = new Mock<IModuleOwner>();

            // Act
            buttonSettingsModule!.ModuleOwner = moduleOwnerMock.Object;
            var result = buttonSettingsModule.ModuleOwner;

            // Assert
            Assert.That(result, Is.EqualTo(moduleOwnerMock.Object));
        }

        [Test]
        public void TestViewModelInitialization()
        {
            // Act
            var viewModel = privateObject!.GetFieldOrProperty("_vm");

            // Assert
            Assert.That(viewModel, Is.Not.Null);
        }

        [Test]
        public void TestOnSelectedHomeDeviceChanged()
        {
            buttonSettingsModule.IsModuleActive = false;
            buttonSettingsModule.OnSelectedHomeDeviceChanged();
            Assert.That(privateObject.GetFieldOrProperty("isSelectChanged"), Is.EqualTo(true));
        }

        [Test]
        public void TestOnSelectedHomeDeviceChangeda()
        {
            buttonSettingsModule.IsModuleActive = true;
            buttonSettingsModule.OnSelectedHomeDeviceChanged();
            Assert.That(privateObject.GetFieldOrProperty("isSelectChanged"), Is.EqualTo(false));
        }

        [Test]
        public void TestOnActivated()
        {
            buttonSettingsModule.OnActivated();
            Assert.Pass();
        }

        [Test]
        public void TestOnDeactivated()
        {
            buttonSettingsModule.OnDeactivated();
            Assert.Pass();
        }
    }
}