using DDPM.SA.Common;
using DDPM.UI.Common;
using DDPM.UI.Common.Interfaces;
using DDPM.UI.Common.Models;
using Dell.Client.Framework.UX.WPF;
using Moq;
using NGA.UnitTest.PrivateObject;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using VcpCore.Common;
using Dell.Client.Framework.UX.WPF.ResourceManager;

namespace DDPM.UI.Module.Color.Tests
{
    [Apartment(ApartmentState.STA)]
    public class ColorModuleTests
    {
        private ColorModule? colorModule;
        private PrivateObject? privateObject;
        private Mock<IModuleOwner>? moduleOwnerMock;
        private IModuleOwner? moduleOwner;
        private Mock<IModuleOwner>? myconsoleMock;
        private Mock<IModuleOwner>? DeviceManagerSAMock;

        [SetUp]
        public void Setup()
        {
            if (System.Windows.Application.Current == null)
            {
                new System.Windows.Application();
            }
            ResourceManager res = new ResourceManager();
            var resourceDictionary = new ResourceDictionary();
            resourceDictionary.Source = new Uri("pack://application:,,,/DDPM.UI.Common;component/ModuleStyle.xaml");
            System.Windows.Application.Current.Resources.MergedDictionaries.Add(resourceDictionary);
            moduleOwnerMock = new Mock<IModuleOwner>();
            moduleOwner = moduleOwnerMock!.Object;
            moduleOwnerMock.Setup(x => x.SelectedHomeDevice).Returns(new HomeDevice());
            DdpmCommonHelper.ModuleOwner = moduleOwner;
            var myconsoleMock = new Mock<IConsole>();
            DdpmCommonHelper.MyConsole = myconsoleMock.Object;
            var DeviceManagerSAMock = new Mock<IDeviceManagerSA>();
            DdpmCommonHelper.DeviceManagerSA = DeviceManagerSAMock.Object;
            DeviceManagerSAMock.Setup(x => x.GetHDRStatus(It.IsAny<MonitorInfo>())).Returns(Task.FromResult(true));
            colorModule = new ColorModule();
            colorModule.SelectedHomeDevice = new HomeDevice();
            colorModule.SelectedHomeDevice.MonitorInfo = new MonitorInfo() { modelName = "AWA" };
            privateObject = new PrivateObject(colorModule);
            privateObject.SetFieldOrProperty("vm", new ColorViewModel());

        }

        [Test]
        public void TestModuleName()
        {
            // Act
            var result = colorModule!.ModuleName;

            // Assert
            Assert.That(result, Is.EqualTo("ColorModule"));
        }

        [Test]
        public void TestGetLeftView()
        {
            // Act
            var result = colorModule!.GetLeftView();

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public void TestGetRightView()
        {
            // Act
            var result = colorModule!.GetRightView();

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
            colorModule!.SelectedHomeDevice = homeDevice;
            var result = colorModule.SelectedHomeDevice;

            // Assert
            Assert.That(result, Is.EqualTo(homeDevice));
        }

        [Test]
        public void TestModuleOwner()
        {

            // Arrange
            colorModule.ModuleOwner = moduleOwner;
            Assert.That(colorModule.ModuleOwner, Is.EqualTo(moduleOwner));
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
            colorModule.IsModuleActive = false;
            colorModule.OnSelectedHomeDeviceChanged();
            Assert.That(privateObject.GetFieldOrProperty("isSelectChanged"), Is.EqualTo(true));
        }

        [Test]
        public void TestOnSelectedHomeDeviceChangeda()
        {
            colorModule.IsModuleActive = true;
            colorModule.OnSelectedHomeDeviceChanged();
            Assert.That(privateObject.GetFieldOrProperty("isSelectChanged"), Is.EqualTo(false));
        }

        [Test]
        public void TestOnActivated()
        {
            try
            {
                colorModule.OnActivated();
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
                colorModule.OnDeactivated();
                Assert.True(true);
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }
        }
    }
}