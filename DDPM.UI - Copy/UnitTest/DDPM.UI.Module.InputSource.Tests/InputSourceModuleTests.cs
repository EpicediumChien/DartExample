using DDPM.UI.Common;
using DDPM.UI.Common.Interfaces;
using DDPM.UI.Common.Models;
using Moq;
using NGA.UnitTest.PrivateObject;
using System.Windows.Controls;

namespace DDPM.UI.Module.InputSource.Tests
{
    [Apartment(ApartmentState.STA)]
    public class InputSourceModuleTests
    {
        private InputSourceModule? inputSourceModule;
        private PrivateObject? privateObject;
        private Mock<IModuleOwner>? moduleOwnerMock;

        [SetUp]
        public void Setup()
        {
            moduleOwnerMock = new Mock<IModuleOwner>();
            var moduleOwner = moduleOwnerMock!.Object;
            DdpmCommonHelper.ModuleOwner = moduleOwner;
            moduleOwnerMock.Setup(x => x.SelectedHomeDevice).Returns(new HomeDevice());
            inputSourceModule = new InputSourceModule();
            privateObject = new PrivateObject(inputSourceModule);
        }

        [Test]
        public void TestModuleName()
        {
            // Act
            var result = inputSourceModule!.ModuleName;

            // Assert
            Assert.That(result, Is.EqualTo("InputSourceModule"));
        }

        [Test]
        public void TestGetLeftView()
        {
            // Act
            var result = inputSourceModule!.GetLeftView();

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public void TestGetRightView()
        {
            // Act
            var result = inputSourceModule!.GetRightView();

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
            inputSourceModule!.SelectedHomeDevice = homeDevice;
            var result = inputSourceModule.SelectedHomeDevice;

            // Assert
            Assert.That(result, Is.EqualTo(homeDevice));
        }

        [Test]
        public void TestModuleOwner()
        {
            // Arrange
            var moduleOwner = moduleOwnerMock!.Object;
            inputSourceModule!.ModuleOwner = moduleOwner;

            Assert.That(inputSourceModule.ModuleOwner, Is.EqualTo(moduleOwner));
        }

        [Test]
        public void TestViewModelInitialization()
        {
            // Act
            var viewModel = privateObject!.GetFieldOrProperty("vm");

            // Assert
            Assert.That(viewModel, Is.Not.Null);
        }

        [Test]
        public void TestOnSelectedHomeDeviceChanged()
        {
            try
            {
                inputSourceModule.OnSelectedHomeDeviceChanged();
                Assert.True(true);
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }
        }

        [Test]
        public void TestOnActivated()
        {
            try
            {
                inputSourceModule.OnActivated();
                Assert.True(true);
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }
        }

        [Test]
        public void TestOnDeactivated()
        {
            try
            {
                inputSourceModule.OnDeactivated();
                Assert.True(true);
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }
        }
    }
}