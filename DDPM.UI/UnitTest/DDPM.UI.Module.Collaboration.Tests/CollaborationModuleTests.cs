using NUnit.Framework;
using Moq;
using System.Windows.Controls;
using DDPM.UI.Common.Interfaces;
using DDPM.UI.Common.Models;
using DDPM.UI.Module.Collaboration;
using DDPM.UI.Plugin.ViewModels;
using Dell.Client.Framework.Common;
using NGA.UnitTest.PrivateObject;
using Dell.Client.Framework.UX.WPF;
using DDPM.SA.Common;

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
            consoleMock = new Mock<IConsole>();
            logMock = new Mock<ILog>();
            deviceManagerMock = new Mock<IDeviceManagerSA>();

            keyboardViewModel = new KeyboardViewModel(consoleMock.Object, logMock.Object, deviceManagerMock.Object);

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
    }
}
