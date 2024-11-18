using DDPM.UI.Common.Interfaces;
using DDPM.UI.Common.Models;
using DDPM.UI.Common;
using DDPM.UI.Plugin.ViewModels;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.UX.WPF;
using NGA.UnitTest.PrivateObject;
using System.Windows.Controls;
using Moq;
using DDPM.SA.Common;

namespace DDPM.UI.Module.AddHeadset_Dongle.Tests
{
    [Apartment(ApartmentState.STA)]
    public class AddHeadset_DongleModuleTests
    {
        private AddHeadset_DongleModule? addHeadset_DongleModule;
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
            console = consoleMock.Object;
            showPluginManagerMock = new Mock<IShowPluginManager>();
            showPluginManager = showPluginManagerMock.Object;
            peripheralPluginMock = new Mock<IDeviceManagerSA>();
            peripheralPlugin = peripheralPluginMock.Object;
            logMock = new Mock<ILog>();
            log = logMock.Object;
            vm = new AddDeviceViewModel(showPluginManager, console, log);
            addHeadset_DongleModule = new AddHeadset_DongleModule(vm);
            privateObject = new PrivateObject(addHeadset_DongleModule);
        }

        [Test]
        public void TestConstructor_AddHeadset_DongleModule()
        {
            // Assert
            Assert.That(addHeadset_DongleModule, Is.Not.Null);
            Assert.That(privateObject.GetFieldOrProperty("_rightView"), Is.Not.Null);
        }

        [Test]
        public void TestIsModuleActive()
        {
            // Act
            addHeadset_DongleModule.IsModuleActive = true;

            // Assert
            Assert.That(addHeadset_DongleModule.IsModuleActive, Is.EqualTo(true));
        }

        [Test]
        public void TestModuleName()
        {
            // Act
            var result = addHeadset_DongleModule!.ModuleName;

            // Assert
            Assert.That(result, Is.EqualTo("AddHeadset_DongleModule"));
        }

        [Test]
        public void TestGetLeftView()
        {
            // Act
            var result = addHeadset_DongleModule!.GetLeftView();

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public void TestGetRightView()
        {
            // Act
            var result = addHeadset_DongleModule!.GetRightView();

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
            addHeadset_DongleModule!.SelectedHomeDevice = homeDevice;
            var result = addHeadset_DongleModule.SelectedHomeDevice;

            // Assert
            Assert.That(result, Is.EqualTo(homeDevice));
        }

        [Test]
        public void TestModuleOwner()
        {
            // Arrange
            addHeadset_DongleModule.ModuleOwner = moduleOwner;
            Assert.That(addHeadset_DongleModule.ModuleOwner, Is.EqualTo(moduleOwner));
        }

        [Test]
        public void TestOnSelectedHomeDeviceChanged()
        {
            try
            {
                addHeadset_DongleModule.OnSelectedHomeDeviceChanged();
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
                addHeadset_DongleModule.OnActivated();
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
                addHeadset_DongleModule.OnDeactivated();
                Assert.True(true);
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }
        }
    }
}