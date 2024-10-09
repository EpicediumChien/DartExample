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

namespace DDPM.UI.Module.AddKnM_BL.Tests
{
    [Apartment(ApartmentState.STA)]
    public class AddKnM_BLModuleTests
    {
        private AddKnM_BLModule? addKnM_BLModule;
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
            addKnM_BLModule = new AddKnM_BLModule(vm);
            privateObject = new PrivateObject(addKnM_BLModule);

        }

        [Test]
        public void TestConstructor_AddKnM_BLModule()
        {
            // Assert
            Assert.That(addKnM_BLModule, Is.Not.Null);
            Assert.That(privateObject.GetFieldOrProperty("_rightView"), Is.Not.Null);
        }

        [Test]
        public void TestIsModuleActive()
        {
            // Act
            addKnM_BLModule.IsModuleActive = true;

            // Assert
            Assert.That(addKnM_BLModule.IsModuleActive, Is.EqualTo(true));
        }


        [Test]
        public void TestModuleName()
        {
            // Act
            var result = addKnM_BLModule!.ModuleName;

            // Assert
            Assert.That(result, Is.EqualTo("AddKnM_BLModule"));
        }

        [Test]
        public void TestGetLeftView()
        {
            // Act
            var result = addKnM_BLModule!.GetLeftView();

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public void TestGetRightView()
        {
            // Act
            var result = addKnM_BLModule!.GetRightView();

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
            addKnM_BLModule!.SelectedHomeDevice = homeDevice;
            var result = addKnM_BLModule.SelectedHomeDevice;

            // Assert
            Assert.That(result, Is.EqualTo(homeDevice));
        }

        [Test]
        public void TestModuleOwner()
        {
            // Arrange
            addKnM_BLModule.ModuleOwner = moduleOwner;
            Assert.That(addKnM_BLModule.ModuleOwner, Is.EqualTo(moduleOwner));
        }

        [Test]
        public void TestOnSelectedHomeDeviceChanged()
        {
            try
            {
                addKnM_BLModule.OnSelectedHomeDeviceChanged();
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
                addKnM_BLModule.OnActivated();
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
                addKnM_BLModule.OnDeactivated();
                Assert.True(true);
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }
        }
    }
}