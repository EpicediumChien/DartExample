using DDPM.UI.Common.Interfaces;
using DDPM.UI.Common.Models;
using DDPM.UI.Common;
using NGA.UnitTest.PrivateObject;
using Moq;
using System.Windows.Controls;
using DDPM.UI.Plugin.ViewModels;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.UX.WPF;
using System;
using DDPM.SA.Common;

namespace DDPM.UI.Module.AddKnM_Dongle.Tests
{
    [Apartment(ApartmentState.STA)]
    public class AddKnM_DongleModuleTests
    {
        private AddKnM_DongleModule? addKnM_DongleModule;
        private PrivateObject? privateObject;
        private Mock<IModuleOwner>? moduleOwnerMock;
        private IModuleOwner? moduleOwner;
        private AddDeviceViewModel? vm;
        private IConsole? console;
        private Mock<IConsole>? consoleMock;
        private IShowPluginManager? showPluginManager;
        private Mock<IShowPluginManager>? showPluginManagerMock;
        private IDeviceManagerSA? peripheralPlugin;
        private Mock<IDeviceManagerSA>? peripheralPluginMock;
        private ILog? log;
        private Mock<ILog>? logMock;

        [SetUp]
        public void Setup()
        {
            moduleOwnerMock = new Mock<IModuleOwner>();
            moduleOwner = moduleOwnerMock!.Object;
            DdpmCommonHelper.ModuleOwner = moduleOwner;
            consoleMock = new Mock<IConsole>();
            console= consoleMock.Object;
            showPluginManagerMock = new Mock<IShowPluginManager>();
            showPluginManager= showPluginManagerMock.Object;
            peripheralPluginMock = new Mock<IDeviceManagerSA>();
            peripheralPlugin = peripheralPluginMock.Object;
            logMock= new Mock<ILog>();
            log= logMock.Object;
            vm = new AddDeviceViewModel(showPluginManager,  console,  log,  peripheralPlugin);
            addKnM_DongleModule = new AddKnM_DongleModule(vm);
            privateObject = new PrivateObject(addKnM_DongleModule);

        }

        [Test]
        public void TestConstructor_AddKnM_DongleModule()
        {
            // Assert
            Assert.That(addKnM_DongleModule, Is.Not.Null);
            Assert.That(privateObject.GetFieldOrProperty("_rightView"), Is.Not.Null);
        }

        [Test]
        public void TestModuleName()
        {
            // Act
            var result = addKnM_DongleModule!.ModuleName;

            // Assert
            Assert.That(result, Is.EqualTo("AddKnM_DongleModule"));
        }

        [Test]
        public void TestGetLeftView()
        {
            // Act
            var result = addKnM_DongleModule!.GetLeftView();

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public void TestGetRightView()
        {
            // Act
            var result = addKnM_DongleModule!.GetRightView();

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
            addKnM_DongleModule!.SelectedHomeDevice = homeDevice;
            var result = addKnM_DongleModule.SelectedHomeDevice;

            // Assert
            Assert.That(result, Is.EqualTo(homeDevice));
        }

        [Test]
        public void TestModuleOwner()
        {
            // Arrange
            addKnM_DongleModule.ModuleOwner = moduleOwner;
            Assert.That(addKnM_DongleModule.ModuleOwner, Is.EqualTo(moduleOwner));
        }

        [Test]
        public void TestOnSelectedHomeDeviceChanged()
        {
            try
            {
                addKnM_DongleModule.OnSelectedHomeDeviceChanged();
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
                addKnM_DongleModule.OnActivated();
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
                addKnM_DongleModule.OnDeactivated();
                Assert.True(true);
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }
        }
    }
}