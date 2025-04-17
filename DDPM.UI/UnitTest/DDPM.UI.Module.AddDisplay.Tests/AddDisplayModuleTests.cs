using DDPM.UI.Common.Interfaces;
using DDPM.UI.Common.Models;
using DDPM.UI.Common;
using DDPM.UI.Plugin.ViewModels;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.UX.WPF;
using NGA.UnitTest.PrivateObject;
using Moq;
using DDPM.SA.Common;
using System.Windows.Controls;

namespace DDPM.UI.Module.AddDisplay.Tests
{
    [Apartment(ApartmentState.STA)]
    public class AddDisplayModuleTests
    {
        private AddDisplayModule? addDisplayModule;
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
            vm = new AddDeviceViewModel(console);
            addDisplayModule = new AddDisplayModule(vm);
            privateObject = new PrivateObject(addDisplayModule);

        }

        [Test]
        public void TestConstructor_AddDisplayModule()
        {
            // Assert
            Assert.That(addDisplayModule, Is.Not.Null);
            Assert.That(privateObject.GetFieldOrProperty("_rightView"), Is.Not.Null);
        }

        [Test]
        public void TestIsModuleActive()
        {
            // Act
            addDisplayModule.IsModuleActive = true;

            // Assert
            Assert.That(addDisplayModule.IsModuleActive, Is.EqualTo(true));
        }

        [Test]
        public void TestModuleName()
        {
            // Act
            var result = addDisplayModule!.ModuleName;

            // Assert
            Assert.That(result, Is.EqualTo("AddDisplayModule"));
        }

        [Test]
        public void TestGetLeftView()
        {
            // Act
            var result = addDisplayModule!.GetLeftView();

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public void TestGetRightView()
        {
            // Act
            var result = addDisplayModule!.GetRightView();

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
            addDisplayModule!.SelectedHomeDevice = homeDevice;
            var result = addDisplayModule.SelectedHomeDevice;

            // Assert
            Assert.That(result, Is.EqualTo(homeDevice));
        }

        [Test]
        public void TestModuleOwner()
        {
            // Arrange
            addDisplayModule.ModuleOwner = moduleOwner;
            Assert.That(addDisplayModule.ModuleOwner, Is.EqualTo(moduleOwner));
        }

        [Test]
        public void TestOnSelectedHomeDeviceChanged()
        {
            addDisplayModule.IsModuleActive = false;
            addDisplayModule.OnSelectedHomeDeviceChanged();
            Assert.That(privateObject.GetFieldOrProperty("isSelectChanged"), Is.EqualTo(true));
        }

        [Test]
        public void TestOnSelectedHomeDeviceChangeda()
        {
            addDisplayModule.IsModuleActive = true;
            addDisplayModule.OnSelectedHomeDeviceChanged();
            Assert.That(privateObject.GetFieldOrProperty("isSelectChanged"), Is.EqualTo(false));
        }

        [Test]
        public void TestOnActivated()
        {
            try
            {
                addDisplayModule.OnActivated();
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
                addDisplayModule.OnDeactivated();
                Assert.True(true);
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }
        }
    }
}