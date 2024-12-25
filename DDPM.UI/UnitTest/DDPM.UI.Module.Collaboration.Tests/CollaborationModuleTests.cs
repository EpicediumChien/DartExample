using DDPM.SA.Common;
using DDPM.UI.Common;
using DDPM.UI.Common.Interfaces;
using DDPM.UI.Common.Models;
using DDPM.UI.Plugin.ViewModels;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.UX.WPF;
using Moq;
using NGA.UnitTest.PrivateObject;
using NUnit.Framework;
using System.Windows.Controls;
using Dell.Client.Framework.UX.WPF.ResourceManager;
using System.Windows;
namespace DDPM.UI.Module.Collaboration.Test
{
    [TestFixture, Apartment(ApartmentState.STA)]
    public class CollaborationModuleTests
    {
        private Mock<IConsole>? consoleMock;
        private Mock<ILog>? logMock;
        private Mock<IDeviceManagerSA>? deviceManagerMock;
        private KeyboardViewModel? keyboardViewModel;
        private CollaborationModule? collaborationModule;
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
            keyboardViewModel = new KeyboardViewModel(consoleMock.Object, logMock.Object);
            collaborationModule = new CollaborationModule(keyboardViewModel);
            privateObject = new PrivateObject(collaborationModule);
        }

        [Test]
        public void TestConstructor()
        {
            // Act
            var moduleName = collaborationModule!.ModuleName;

            // Assert
            Assert.That(moduleName, Is.EqualTo("CollaborationModule"));
        }

        [Test]
        public void TestGetLeftView()
        {
            // Act
            var leftView = collaborationModule!.GetLeftView();

            // Assert
            Assert.That(leftView, Is.Null);
        }

        [Test]
        public void TestGetRightView()
        {
            // Act
            var rightView = collaborationModule!.GetRightView();

            // Assert
            Assert.That(rightView, Is.Not.Null);
            Assert.That(rightView, Is.InstanceOf<UserControl>());
        }

        [Test]
        public void TestSelectedHomeDevice()
        {
            // Arrange
            var homeDevice = new HomeDevice();

            // Act
            collaborationModule!.SelectedHomeDevice = homeDevice;
            var selectedHomeDevice = collaborationModule.SelectedHomeDevice;

            // Assert
            Assert.That(selectedHomeDevice, Is.EqualTo(homeDevice));
        }

        [Test]
        public void TestModuleOwner()
        {
            // Arrange
            var moduleOwnerMock = new Mock<IModuleOwner>();

            // Act
            collaborationModule!.ModuleOwner = moduleOwnerMock.Object;
            var moduleOwner = collaborationModule.ModuleOwner;

            // Assert
            Assert.That(moduleOwner, Is.EqualTo(moduleOwnerMock.Object));
        }

        [Test]
        public void TestOnSelectedHomeDeviceChanged()
        {
            collaborationModule.IsModuleActive = false;
            collaborationModule.OnSelectedHomeDeviceChanged();
            Assert.That(privateObject.GetFieldOrProperty("isSelectChanged"), Is.EqualTo(true));
        }

        [Test]
        public void TestOnSelectedHomeDeviceChangeda()
        {
            collaborationModule.IsModuleActive = true;
            collaborationModule.OnSelectedHomeDeviceChanged();
            Assert.That(privateObject.GetFieldOrProperty("isSelectChanged"), Is.EqualTo(false));
        }

        [Test]
        public void TestOnActivated()
        {
            privateObject.SetFieldOrProperty("isSelectChanged", true);
            collaborationModule.OnActivated();
            Assert.That(privateObject.GetFieldOrProperty("isSelectChanged"), Is.EqualTo(false));
        }

        [Test]
        public void TestOnDeactivated()
        {
            collaborationModule.OnDeactivated();
            Assert.Pass();
        }
    }
}