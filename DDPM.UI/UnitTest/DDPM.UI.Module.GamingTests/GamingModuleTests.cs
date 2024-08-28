using DDPM.SA.Common;
using DDPM.UI.Common;
using DDPM.UI.Common.Interfaces;
using DDPM.UI.Common.Models;
using Moq;
using NGA.UnitTest.PrivateObject;
using System.Reflection;
using System.Windows.Controls;
using VcpCore.Common;

namespace DDPM.UI.Module.Gaming.Tests
{
    [TestFixture, Apartment(ApartmentState.STA)]
    public class GamingModuleTests
    {
        private GamingModule? gamingModule;
        private PrivateObject? privateObject;
        private Mock<IModuleOwner>? moduleOwnerMock;
        private IModuleOwner? moduleOwner;
        private Mock<IDeviceManagerSA>? deviceManagerSAMock;
        private IDeviceManagerSA? deviceManagerSA;
        private HomeDevice? selectedHomeDevice;
        private GamingViewModel? gamingViewModel;
        private GamingModule? myModule;

        [SetUp]
        public void Setup()
        {
            moduleOwnerMock = new Mock<IModuleOwner>();
            moduleOwner = moduleOwnerMock!.Object;
            DdpmCommonHelper.ModuleOwner = moduleOwner;
            deviceManagerSAMock= new Mock<IDeviceManagerSA>();
            deviceManagerSA= deviceManagerSAMock.Object;
            DdpmCommonHelper.DeviceManagerSA = deviceManagerSA;
            selectedHomeDevice = new HomeDevice();
            moduleOwnerMock.Setup(x => x.SelectedHomeDevice).Returns(selectedHomeDevice);
            myModule = new GamingModule();
            GamingViewModel gamingViewModel = new GamingViewModel();
            gamingViewModel.MyModule = myModule;
            gamingViewModel.MyModule.SelectedHomeDevice = selectedHomeDevice;
            gamingViewModel.MyModule.SelectedHomeDevice.MonitorInfo = new MonitorInfo() { modelName="gx" };
            gamingModule = new GamingModule();
            gamingModule.SelectedHomeDevice=selectedHomeDevice;
            privateObject = new PrivateObject(gamingModule);
        }

        [Test]
        public void TestTestConstructor_GamingModule()
        {
            // Assert
            Assert.That(gamingModule, Is.Not.Null);
            UserControl _rightView =(UserControl)privateObject.GetFieldOrProperty("_rightView");
            Assert.That(_rightView.DataContext, Is.EqualTo(privateObject.GetFieldOrProperty("vm")));
        }

        [Test]
        public void TestModuleName()
        {
            // Act
            var result = gamingModule!.ModuleName;

            // Assert
            Assert.That(result, Is.EqualTo("GamingModule"));
        }

        [Test]
        public void TestGetLeftView()
        {
            // Act
            var result = gamingModule!.GetLeftView();

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public void TestGetRightView()
        {
            // Act
            var result = gamingModule!.GetRightView();

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
            gamingModule!.SelectedHomeDevice = homeDevice;
            var result = gamingModule.SelectedHomeDevice;

            // Assert
            Assert.That(result, Is.EqualTo(homeDevice));
        }

        [Test]
        public void TestModuleOwner()
        {
            gamingModule.ModuleOwner = moduleOwner;
            // Arrange
            Assert.That(gamingModule.ModuleOwner, Is.EqualTo(moduleOwner));
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
                gamingModule.OnSelectedHomeDeviceChanged();
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
            
            deviceManagerSAMock.Setup(x => x.GetGamingProperties(It.IsAny<MonitorInfo>())).Returns(Task.FromResult(new GamingDisplayPropertiesInfo()));
            try
            {
                gamingModule.OnActivated();
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
                gamingModule.OnDeactivated();
                Assert.True(true);
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }
        }
    }
}