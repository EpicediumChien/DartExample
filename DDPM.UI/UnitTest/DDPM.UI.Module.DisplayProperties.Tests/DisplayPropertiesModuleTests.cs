using DDPM.UI.Common.Interfaces;
using DDPM.UI.Common.Models;
using DDPM.UI.Common;
using Moq;
using NGA.UnitTest.PrivateObject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace DDPM.UI.Module.DisplayProperties.Tests
{
    [Apartment(ApartmentState.STA)]
    public class DisplayPropertiesModuleTests
    {
        private DisplayPropertiesModule? displayPropertiesModule;
        private PrivateObject? privateObject;
        private Mock<IModuleOwner>? moduleOwnerMock;


        [SetUp]
        public void Setup()
        {
            moduleOwnerMock = new Mock<IModuleOwner>();
            var moduleOwner = moduleOwnerMock!.Object;
            DdpmCommonHelper.ModuleOwner = moduleOwner;
            displayPropertiesModule = new DisplayPropertiesModule();
            privateObject = new PrivateObject(displayPropertiesModule);
        }


        [Test]
        public void TestModuleName()
        {
            // Act
            var result = displayPropertiesModule!.ModuleName;

            // Assert
            Assert.That(result, Is.EqualTo("DisplayPropertiesModule"));
        }


        [Test]
        public void TestGetLeftView()
        {
            // Act
            var result = displayPropertiesModule!.GetLeftView();

            // Assert
            Assert.That(result, Is.Null);
        }


        [Test]
        public void TestGetRightView()
        {
            // Act
            var result = displayPropertiesModule!.GetRightView();

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
            displayPropertiesModule!.SelectedHomeDevice = homeDevice;
            var result = displayPropertiesModule.SelectedHomeDevice;

            // Assert
            Assert.That(result, Is.EqualTo(homeDevice));
        }


        [Test]
        public void TestModuleOwner()
        {
            // Arrange
            var moduleOwner = moduleOwnerMock!.Object;
            displayPropertiesModule.ModuleOwner=moduleOwner;
            Assert.That(displayPropertiesModule.ModuleOwner, Is.EqualTo(moduleOwner));
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
                displayPropertiesModule.OnSelectedHomeDeviceChanged();
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
                displayPropertiesModule.OnActivated();
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
            var colorModule = new DisplayPropertiesModule();
            try
            {
                displayPropertiesModule.OnDeactivated();
                Assert.True(true);
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }

        }


    }
}
