using DDPM.UI.Common.Interfaces;
using DDPM.UI.Common.Models;
using Moq;
using NGA.UnitTest.PrivateObject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace DDPM.UI.Module.PipPbp.Tests
{
    [TestFixture, Apartment(ApartmentState.STA)]
    public class PipPbpModuleTests
    {
        private PipPbpModule? pipPbpModule;
        private PrivateObject? privateObject;
        private Mock<IModuleOwner>? moduleOwnerMock;

        [SetUp]
        public void SetUp()
        {
            moduleOwnerMock = new Mock<IModuleOwner>();
            var moduleOwner = moduleOwnerMock!.Object;
            pipPbpModule = new PipPbpModule();
            privateObject = new PrivateObject(pipPbpModule);
        }


        [Test]
        public void TestConstructor_PipPbpModule()
        {
            // Assert
            Assert.That(pipPbpModule, Is.Not.Null);

        }
        [Test]
        public void TestModuleName()
        {
            // Act
            var result = pipPbpModule!.ModuleName;

            // Assert
            Assert.That(result, Is.EqualTo("PipPbpModule"));
        }

        [Test]
        public void TestGetLeftView()
        {
            // Act
            var result = pipPbpModule!.GetLeftView();

            // Assert
            Assert.That(result, Is.Null);
        }


        [Test]
        public void TestGetRightView()
        {
            // Act
            var result = pipPbpModule!.GetRightView();

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
            pipPbpModule!.SelectedHomeDevice = homeDevice;
            var result = pipPbpModule.SelectedHomeDevice;

            // Assert
            Assert.That(result, Is.EqualTo(homeDevice));
        }

        [Test]
        public void TestModuleOwner()
        {
            // Arrange
            var moduleOwner = moduleOwnerMock!.Object;
            pipPbpModule.ModuleOwner = moduleOwner;
            Assert.That(pipPbpModule.ModuleOwner, Is.EqualTo(moduleOwner));
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
                pipPbpModule.OnSelectedHomeDeviceChanged();
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
                pipPbpModule.OnActivated();
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
                pipPbpModule.OnDeactivated();
                Assert.True(true);
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }

        }

    }
}
