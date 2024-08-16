using DDPM.UI.Common;
using DDPM.UI.Common.Interfaces;
using DDPM.UI.Common.Models;
using Moq;
using NGA.UnitTest.PrivateObject;
using System.Windows.Controls;

namespace DDPM.UI.Module.Color.Tests
{
    [Apartment(ApartmentState.STA)]
    public class ColorModuleTests
    {
        private ColorModule? colorModule;
        private PrivateObject? privateObject;
        private Mock<IModuleOwner>? moduleOwnerMock;
        private IModuleOwner? moduleOwner;

        [SetUp]
        public void Setup()
        {
            moduleOwnerMock = new Mock<IModuleOwner>();
            moduleOwner = moduleOwnerMock!.Object;
            DdpmCommonHelper.ModuleOwner = moduleOwner;
            colorModule = new ColorModule();
            privateObject = new PrivateObject(colorModule);
        }

        [Test]
        public void TestModuleName()
        {
            // Act
            var result = colorModule!.ModuleName;

            // Assert
            Assert.That(result, Is.EqualTo("ColorModule"));
        }

        [Test]
        public void TestGetLeftView()
        {
            // Act
            var result = colorModule!.GetLeftView();

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public void TestGetRightView()
        {
            // Act
            var result = colorModule!.GetRightView();

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
            colorModule!.SelectedHomeDevice = homeDevice;
            var result = colorModule.SelectedHomeDevice;

            // Assert
            Assert.That(result, Is.EqualTo(homeDevice));
        }

        [Test]
        public void TestModuleOwner()
        {
            // Arrange
            colorModule.ModuleOwner = moduleOwner;
            Assert.That(colorModule.ModuleOwner, Is.EqualTo(moduleOwner));
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
                colorModule.OnSelectedHomeDeviceChanged();
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
                colorModule.OnActivated();
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
                colorModule.OnDeactivated();
                Assert.True(true);
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }
        }
    }
}