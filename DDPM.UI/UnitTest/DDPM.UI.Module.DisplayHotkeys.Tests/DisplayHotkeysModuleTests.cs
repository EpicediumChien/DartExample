using DDPM.SA.Common;
using DDPM.UI.Common;
using DDPM.UI.Common.Interfaces;
using DDPM.UI.Common.Models;
using Moq;
using NGA.UnitTest.PrivateObject;
using System.Windows.Controls;
using VcpCore.Common;

namespace DDPM.UI.Module.DisplayHotkeys.Tests
{
    [TestFixture, Apartment(ApartmentState.STA)]
    public class DisplayHotkeysModuleTests
    {
        private DisplayHotkeysModule? displayHotkeysModule;
        private PrivateObject? privateObject;
        private Mock<IModuleOwner>? moduleOwnerMock;
        private Mock<IDeviceManagerSA>? deviceManagerMock;
        private DisplayHotkeysViewModel vm;

        [SetUp]
        public void Setup()
        {
            moduleOwnerMock = new Mock<IModuleOwner>();
            moduleOwnerMock.Setup(x => x.SelectedHomeDevice).Returns(new HomeDevice());
            var moduleOwner = moduleOwnerMock!.Object;
            DdpmCommonHelper.ModuleOwner = moduleOwner;
  
        }

        [Test]
        public void TestConstructor_DisplayHotkeysModule()
        {
            DisplayHotkeysViewModel vm = new DisplayHotkeysViewModel();
            vm.DisplayHotkeysModule = new DisplayHotkeysModule() { SelectedHomeDevice = new HomeDevice() { MonitorInfo = new MonitorInfo() { CapabilityDic = new Dictionary<string, List<string>>() } } };
            vm.DisplayHotkeysModule.SelectedHomeDevice.MonitorInfo = new MonitorInfo();
            vm.DisplayHotkeysModule.SelectedHomeDevice.MonitorInfo.CapabilityDic = new Dictionary<string, List<string>>();
            displayHotkeysModule = new DisplayHotkeysModule() { SelectedHomeDevice = new HomeDevice() { MonitorInfo=new MonitorInfo() { CapabilityDic=new Dictionary<string, List<string>>()} } };
            // Assert
            Assert.That(displayHotkeysModule.ModuleOwner, Is.Not.Null);
        }

        [Test]
        [Apartment(ApartmentState.STA)]
        public void TestViewModelInitialization()
        {
            // Act
            displayHotkeysModule = new DisplayHotkeysModule();
            privateObject = new PrivateObject(displayHotkeysModule);
            var viewModel = privateObject!.GetFieldOrProperty("vm");

            // Assert
            Assert.That(viewModel, Is.Not.Null);
            Assert.That(viewModel, Is.InstanceOf<DisplayHotkeysViewModel>());
        }

        [Test]
        public void TestModuleName()
        {
            // Act
            var displayHotkeysModule = new DisplayHotkeysModule();
            var result = displayHotkeysModule!.ModuleName;

            // Assert
            Assert.That(result, Is.EqualTo("DisplayHotkeysModule"));
        }

        [Test]//Test method
        public void TestSelectedHomeDevice()
        {
            var displayHotkeysModule = new DisplayHotkeysModule();
            var slectedHomeDevice = new HomeDevice();
            displayHotkeysModule.SelectedHomeDevice = slectedHomeDevice;
            Assert.That(displayHotkeysModule!.SelectedHomeDevice, Is.EqualTo(null));

            moduleOwnerMock = new Mock<IModuleOwner>();
            var moduleOwner = moduleOwnerMock!.Object;
            DdpmCommonHelper.ModuleOwner = moduleOwner;
            moduleOwnerMock.Setup(x => x.SelectedHomeDevice).Returns(new HomeDevice());
            displayHotkeysModule = new DisplayHotkeysModule();
            slectedHomeDevice = new HomeDevice();
            displayHotkeysModule.SelectedHomeDevice = slectedHomeDevice;
            Assert.That(displayHotkeysModule!.SelectedHomeDevice, Is.EqualTo(moduleOwner.SelectedHomeDevice));
        }

        [Test]
        public void TestGetLeftView()
        {
            var mockHomeDevice = new Mock<HomeDevice>();
            var displayHotkeysModule = new DisplayHotkeysModule(/*mockHomeDevice.Object*/);

            //Act:Call the methods to test
            var leftView = displayHotkeysModule.GetLeftView();

            Assert.That(leftView, Is.Null);
        }

        [Test]
        public void TestGetRightView()
        {
            var mockHomeDevice = new Mock<HomeDevice>();
            var displayHotkeysModule = new DisplayHotkeysModule(/*mockHomeDevice.Object*/);

            var rightView = displayHotkeysModule.GetRightView();

            Assert.That(rightView, Is.InstanceOf<UserControl>());
        }

        [Test]
        public void TestModuleOwner()
        {
            //var deviceManagerMock = new Mock<IDeviceManagerSA>();
            //var deviceManagerSA = deviceManagerMock.Object;
            //DdpmCommonHelper.DeviceManagerSA = deviceManagerSA;

            var moduleOwnerMock = new Mock<IModuleOwner>();
            var moduleOwner = moduleOwnerMock!.Object;
            DdpmCommonHelper.ModuleOwner = moduleOwner;

            var displayHotkeysModule = new DisplayHotkeysModule();
            Assert.That(displayHotkeysModule.ModuleOwner, Is.EqualTo(moduleOwner));
        }

        [Test]
        [Apartment(ApartmentState.STA)]
        public void TestOnSelectedHomeDeviceChanged()
        {
            var displayHotkeysModule = new DisplayHotkeysModule();
            displayHotkeysModule.OnSelectedHomeDeviceChanged();
            Assert.Pass();
        }

        [Test]
        [Apartment(ApartmentState.STA)]
        public void TestOnActivated()
        {
            var displayHotkeysModule = new DisplayHotkeysModule();
            displayHotkeysModule.OnActivated();
            Assert.Pass();
        }

        [Test]
        [Apartment(ApartmentState.STA)]
        public void TestOnDeactivated()
        {
            var displayHotkeysModule = new DisplayHotkeysModule();
            displayHotkeysModule.OnDeactivated();
            Assert.Pass();
        }
    }
}