using DDPM.SA.Common;
using DDPM.UI.Common;
using DDPM.UI.Common.Interfaces;
using DDPM.UI.Common.Models;
using Moq;
using NGA.UnitTest.PrivateObject;
using NUnit.Framework;
using System.Windows;
using System.Windows.Controls;
using Dell.Client.Framework.UX.WPF.ResourceManager;

namespace DDPM.UI.Module.DisplayOthers.Tests
{
    [Apartment(ApartmentState.STA)]
    [TestFixture]
    public class DisplayOthersModuleTests
    {
        private DisplayOthersModule? displayOthersModule;
        private PrivateObject? privateObject;
        private Mock<IModuleOwner>? moduleOwnerMock;
        private Mock<IDeviceManagerSA>? DeviceManagerSAMock;


        [SetUp]
        public void SetUp()
        {
            if (System.Windows.Application.Current == null)
            {
                new System.Windows.Application();
            }
            ResourceManager res = new ResourceManager();
            var resourceDictionary = new ResourceDictionary();
            resourceDictionary.Source = new Uri("pack://application:,,,/DDPM.UI.Common;component/ModuleStyle.xaml");
            System.Windows.Application.Current.Resources.MergedDictionaries.Add(resourceDictionary);
            var moduleOwerMock = new Mock<IModuleOwner>();
            var moduleOwer = moduleOwerMock.Object;
            DdpmCommonHelper.ModuleOwner = moduleOwer;
            DeviceManagerSAMock = new Mock<IDeviceManagerSA>();
            DdpmCommonHelper.DeviceManagerSA = DeviceManagerSAMock.Object;
            var selectedHomeDevice = new HomeDevice();
            moduleOwerMock.Setup(x => x.SelectedHomeDevice).Returns(new HomeDevice());
            displayOthersModule = new DisplayOthersModule();
            displayOthersModule.SelectedHomeDevice = selectedHomeDevice;
            displayOthersModule.SelectedHomeDevice.MonitorInfo=new VcpCore.Common.MonitorInfo();
            displayOthersModule.SelectedHomeDevice.MonitorInfo.edid = new VcpCore.Common.EDID();
            displayOthersModule.SelectedHomeDevice.MonitorInfo.edid.SerialNumber = "sdfad";
            privateObject = new PrivateObject(displayOthersModule);
            moduleOwnerMock = new Mock<IModuleOwner>();
        }

        [Test]
        public void TestConstructor_DisplayOthersModule()
        {
            // Assert
            Assert.That(displayOthersModule, Is.Not.Null);
        }

        [Test]
        public void TestModuleName()
        {
            // Act
            var result = displayOthersModule!.ModuleName;

            // Assert
            Assert.That(result, Is.EqualTo("DisplayOthersModule"));
        }

        [Test]
        public void TestGetLeftView()
        {
            // Act
            var result = displayOthersModule!.GetLeftView();

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public void TestGetRightView()
        {
            // Act
            var result = displayOthersModule!.GetRightView();

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
            displayOthersModule!.SelectedHomeDevice = homeDevice;
            var result = displayOthersModule.SelectedHomeDevice;

            // Assert
            Assert.That(result, Is.EqualTo(homeDevice));
        }

        [Test]
        public void TestModuleOwner()
        {
            // Arrange
            var moduleOwner = moduleOwnerMock!.Object;

            // Act
            privateObject!.SetProperty("ModuleOwner", moduleOwner);
            var result = privateObject.GetProperty("ModuleOwner");

            // Assert
            Assert.That(result, Is.EqualTo(moduleOwner));
        }

        [Test]
        public void TestViewModelInitialization()
        {
            // Act
            var viewModel = privateObject!.GetFieldOrProperty("vm") as DisplayOthersViewModel;

            // Assert
            Assert.That(viewModel, Is.Not.Null);
        }

        [Test]
        public void TestOnSelectedHomeDeviceChanged()
        {
            displayOthersModule.IsModuleActive = false;
            displayOthersModule.OnSelectedHomeDeviceChanged();
            Assert.That(privateObject.GetFieldOrProperty("isSelectChanged"), Is.EqualTo(true));
        }

        [Test]
        public void TestOnSelectedHomeDeviceChangeda()
        {
            displayOthersModule.IsModuleActive = true;
            displayOthersModule.OnSelectedHomeDeviceChanged();
            Assert.That(privateObject.GetFieldOrProperty("isSelectChanged"), Is.EqualTo(false));
        }

        [Test]
        public void TestOnActivated()
        {
            var displayOthersModule = new DisplayOthersModule();
            displayOthersModule.OnActivated();
            Assert.Pass();
        }

        [Test]
        public void TestOnDeactivated()
        {
            var displayOthersModule = new DisplayOthersModule();
            displayOthersModule.OnDeactivated();
            Assert.Pass("The method executed without exceptions");
        }
    }
}