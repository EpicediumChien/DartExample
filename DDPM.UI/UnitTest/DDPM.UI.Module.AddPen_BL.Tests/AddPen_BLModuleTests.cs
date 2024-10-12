using System;
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
namespace DDPM.UI.Module.AddPen_BL.Tests
{
    [Apartment(ApartmentState.STA)]
    public class AddPen_BLModuleTests
    {
        private AddPen_BLModule? addPen_BLModule;
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
            addPen_BLModule = new AddPen_BLModule(vm);
            privateObject = new PrivateObject(addPen_BLModule);

        }

        [Test]
        public void TestConstructor_AddPen_BLModule()
        {
            // Assert
            Assert.That(addPen_BLModule, Is.Not.Null);
            Assert.That(privateObject.GetFieldOrProperty("_rightView"), Is.Not.Null);
        }

        [Test]
        public void TestModuleName()
        {
            // Act
            var result = addPen_BLModule!.ModuleName;

            // Assert
            Assert.That(result, Is.EqualTo("AddPen_BLModule"));
        }

        [Test]
        public void TestGetLeftView()
        {
            // Act
            var result = addPen_BLModule!.GetLeftView();

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public void TestGetRightView()
        {
            // Act
            var result = addPen_BLModule!.GetRightView();

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
            addPen_BLModule!.SelectedHomeDevice = homeDevice;
            var result = addPen_BLModule.SelectedHomeDevice;

            // Assert
            Assert.That(result, Is.EqualTo(homeDevice));
        }

        [Test]
        public void TestModuleOwner()
        {
            // Arrange
            addPen_BLModule.ModuleOwner = moduleOwner;
            Assert.That(addPen_BLModule.ModuleOwner, Is.EqualTo(moduleOwner));
        }

        [Test]
        public void TestOnSelectedHomeDeviceChanged()
        {
            //IsModuleActive==false;
            try
            {
                addPen_BLModule.OnSelectedHomeDeviceChanged();
                Assert.True(true);
                Assert.That(privateObject.GetFieldOrProperty("isSelectChanged"), Is.EqualTo(true));
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }

            //IsModuleActive==true
            addPen_BLModule.IsModuleActive=true;
            try
            {
                addPen_BLModule.OnSelectedHomeDeviceChanged();
                Assert.True(true);
                Assert.That(privateObject.GetFieldOrProperty("isSelectChanged"), Is.EqualTo(false));
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }
        }

        [Test]
        public void TestOnActivated()
        {
            privateObject.SetFieldOrProperty("isSelectChanged", true);
            try
            {
                addPen_BLModule.OnActivated();
                Assert.True(true);
                Assert.That(privateObject.GetFieldOrProperty("isSelectChanged"), Is.EqualTo(false));
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
                addPen_BLModule.OnDeactivated();
                Assert.True(true);
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }
        }
    }
}