using DDPM.SA.Common;
using DDPM.UI.Common;
using DDPM.UI.Common.Interfaces;
using DDPM.UI.Common.Models;
using Moq;
using NGA.UnitTest.PrivateObject;
using System.Windows.Controls;
using VcpCore.Common;

namespace DDPM.UI.Module.Kvm.Tests
{
    [Apartment(ApartmentState.STA)]
    public class KvmModuleTests
    {
        private KvmModule? kvmModule;
        private PrivateObject? privateObject;
        private Mock<IModuleOwner>? moduleOwnerMock;
        private Mock<IDeviceManagerSA>? deviceManagerMock;
        private IDeviceManagerSA? deviceManagerSA;

        [SetUp]
        public void Setup()
        {
            moduleOwnerMock = new Mock<IModuleOwner>();
            var moduleOwner = moduleOwnerMock!.Object;
            DdpmCommonHelper.ModuleOwner = moduleOwner;
            deviceManagerMock = new Mock<IDeviceManagerSA>();
            deviceManagerSA = deviceManagerMock.Object;
            DdpmCommonHelper.DeviceManagerSA = deviceManagerSA;
            moduleOwnerMock.Setup(x => x.SelectedHomeDevice).Returns(new HomeDevice());
            deviceManagerMock.Setup(x => x.GetOnUSBKVM(It.IsAny<MonitorInfo>())).Returns(Task.FromResult(true));
            kvmModule = new KvmModule(moduleOwner);
            privateObject = new PrivateObject(kvmModule);
        }

        [Test]
        public void TestModuleName()
        {
            // Act
            var result = kvmModule!.ModuleName;

            // Assert
            Assert.That(result, Is.EqualTo("KvmModule"));
        }

        [Test]
        public void TestGetLeftView()
        {
            // Act
            var result = kvmModule!.GetLeftView();

            // Assert
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void TestGetRightView()
        {
            // Act
            var result = kvmModule!.GetRightView();

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
            kvmModule!.SelectedHomeDevice = homeDevice;
            var result = kvmModule.SelectedHomeDevice;

            // Assert
            Assert.That(result, Is.EqualTo(homeDevice));
        }

        [Test]
        public void TestModuleOwner()
        {
            // Arrange
            var moduleOwner = moduleOwnerMock!.Object;
            kvmModule!.ModuleOwner = moduleOwner;

            Assert.That(kvmModule.ModuleOwner, Is.EqualTo(moduleOwner));
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
                kvmModule.OnSelectedHomeDeviceChanged();
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
                kvmModule.OnActivated();
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
                kvmModule.OnDeactivated();
                Assert.True(true);
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }
        }
    }
}