using DDPM.SA.Common;
using DDPM.UI.Common;
using DDPM.UI.Common.Interfaces;
using DDPM.UI.Common.Models;
using DDPM.UI.Interfaces;
using Dell.Client.Framework.UX.WPF;
using Moq;
using NGA.UnitTest.PrivateObject;
using NUnit.Framework;
using System.Reflection;
using System.Windows.Controls;
using VcpCore.Common;

namespace DDPM.UI.Module.Brightness.Tests
{
    [TestFixture, Apartment(ApartmentState.STA)]//Uni Test Class
    public class BrightnessModuleTests
    {

        private BrightnessModule? brightnessModule;
        private PrivateObject? privateObject;
        private Mock<IModuleOwner>? moduleOwnerMock;
        private Mock<IDeviceManagerSA>? deviceManagerMock;
        private IDeviceManagerSA? deviceManagerSA;
        private Mock<IConsole>? myConsoleMock;
        private IConsole? myConsole;

        [SetUp]
        public void Setup()
        {
            moduleOwnerMock = new Mock<IModuleOwner>();
            var moduleOwner = moduleOwnerMock!.Object;
            DdpmCommonHelper.ModuleOwner = moduleOwner;
            myConsoleMock=new Mock<IConsole>();
            myConsole=myConsoleMock.Object;
            DdpmCommonHelper.MyConsole=myConsole;
            deviceManagerMock = new Mock<IDeviceManagerSA>();
            deviceManagerSA = deviceManagerMock.Object;
            DdpmCommonHelper.DeviceManagerSA = deviceManagerSA;
            brightnessModule = new BrightnessModule(moduleOwner);
            privateObject = new PrivateObject(brightnessModule);
        }

        [Test]
        public void TestConstructor_BrightnessModule()
        {
            // Assert
            var vm = privateObject.GetFieldOrProperty("vm");
            Assert.That(vm, Is.Not.Null);
            Assert.That(vm, Is.Not.SameAs(brightnessModule));
            Assert.That(brightnessModule.ModuleOwner, Is.Not.Null);

        }

        [Test]
        [Apartment(ApartmentState.STA)]
        public void TestViewModelInitialization()
        {
            // Act
            var viewModel = privateObject!.GetFieldOrProperty("vm") as BrightnessViewModel;

            // Assert
            Assert.That(viewModel, Is.Not.Null);
        }


        [Test]
        public void TestModuleName()
        {
            // Act
            var result = brightnessModule!.ModuleName;

            // Assert
            Assert.That(result, Is.EqualTo("BrightnessModule"));
        }


        [Test]//Test method
        public void TestSelectedHomeDevice()
        {

            //Arrange:Initialize object and define var info
            var homeDevice = new HomeDevice();

            // Act
            brightnessModule!.SelectedHomeDevice = homeDevice;
            var result = brightnessModule.SelectedHomeDevice;

            //Assert:Verify
            Assert.That(result, Is.EqualTo(homeDevice));

        }

        [Test]
        public void TestGetLeftView()
        {
            var mockHomeDevice = new Mock<HomeDevice>();
            //var brightnessModule = new BrightnessModule(/*mockHomeDevice.Object*/);

            //Act:Call the methods to test
            var leftView = brightnessModule.GetLeftView();

            Assert.That(leftView, Is.Null);
        }

        [Test]
        public void TestGetRightView()
        {
            var mockHomeDevice = new Mock<HomeDevice>();

            var rightView = brightnessModule.GetRightView();

            Assert.That(rightView, Is.InstanceOf<UserControl>());
        }

        [Test]
        public void TestModuleOwner()
        {
            var mockModuleOwner = new Mock<IModuleOwner>();
            var mockHomeDevice = new Mock<HomeDevice>();

            brightnessModule.ModuleOwner = mockModuleOwner.Object;

            Assert.That(brightnessModule.ModuleOwner, Is.EqualTo(mockModuleOwner.Object));
        }


        [Test]
        public void TestOnSelectedHomeDeviceChanged()
        {
            brightnessModule.OnSelectedHomeDeviceChanged();
            Assert.Pass();
        }

        [Test]
        public void TestOnActivated()
        {
            var brightnessViewModel = new BrightnessViewModel();
            var myModule = brightnessModule;
            brightnessViewModel.MyModule = myModule;
            brightnessViewModel.MyModule.SelectedHomeDevice = new HomeDevice();
            brightnessViewModel.MyModule.SelectedHomeDevice.MonitorInfo = new MonitorInfo();
            deviceManagerMock.Setup(x => x.GetHDRStatus(It.IsAny<MonitorInfo>())).Returns(Task.FromResult(true));
            brightnessModule.OnActivated();
            Assert.Pass();
        }


        [Test]
        public void TestOnDeactivated()
        {
            brightnessModule.OnDeactivated();
            Assert.Pass();
        }
    }
}