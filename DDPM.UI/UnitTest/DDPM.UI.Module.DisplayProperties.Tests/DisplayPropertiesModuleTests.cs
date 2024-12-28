using DDPM.UI.Common;
using DDPM.UI.Common.Interfaces;
using DDPM.UI.Common.Models;
using Moq;
using NGA.UnitTest.PrivateObject;
using System.Windows;
using System.Windows.Controls;
using Dell.Client.Framework.UX.WPF.ResourceManager;
using DDPM.SA.Common;
using VcpCore.Common;
using Dell.Client.Framework.UX.WPF;

namespace DDPM.UI.Module.DisplayProperties.Tests
{
    [Apartment(ApartmentState.STA)]
    public class DisplayPropertiesModuleTests
    {
        private DisplayPropertiesModule? displayPropertiesModule;
        private PrivateObject? privateObject;
        private Mock<IModuleOwner>? moduleOwnerMock;
        private Mock<IDeviceManagerSA>? deviceManagerSAMock;
        private Mock<IConsole> consoleMock;

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
            var moduleOwner = moduleOwnerMock!.Object;
            DdpmCommonHelper.ModuleOwner = moduleOwner;
            moduleOwnerMock.Setup(x => x.SelectedHomeDevice).Returns(new HomeDevice());
            DdpmCommonHelper.ModuleOwner.SelectedHomeDevice.MonitorInfo = new VcpCore.Common.MonitorInfo();
            deviceManagerSAMock = new Mock<IDeviceManagerSA>();
            DdpmCommonHelper.DeviceManagerSA = deviceManagerSAMock.Object;
            deviceManagerSAMock.Setup(x=>x.GetDisplayPropertiesInfo(It.IsAny<MonitorInfo>())).Returns(Task.FromResult( new DisplayPropertiesInfo() { SupportedHDR =true}));
            deviceManagerSAMock.Setup(x => x.GetPxpMode(It.IsAny<MonitorInfo>())).Returns(Task.FromResult(new ObjGetVCP()));
            consoleMock = new Mock<IConsole>();
            DdpmCommonHelper.MyConsole= consoleMock.Object;
            displayPropertiesModule = new DisplayPropertiesModule();
            privateObject = new PrivateObject(displayPropertiesModule);
        }

        [Test]
        public void TestModuleName()
        {
            // Act
            var result = displayPropertiesModule!.ModuleName;

            // Assert
            Assert.That(result, Is.EqualTo("DisplayPropertiesModule"));
        }

        [Test]
        public void TestGetLeftView()
        {
            // Act
            var result = displayPropertiesModule!.GetLeftView();

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public void TestGetRightView()
        {
            // Act
            var result = displayPropertiesModule!.GetRightView();

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
            displayPropertiesModule!.SelectedHomeDevice = homeDevice;
            var result = displayPropertiesModule.SelectedHomeDevice;

            // Assert
            Assert.That(result, Is.EqualTo(homeDevice));
        }

        [Test]
        public void TestModuleOwner()
        {
            // Arrange
            var moduleOwner = moduleOwnerMock!.Object;
            displayPropertiesModule.ModuleOwner = moduleOwner;
            Assert.That(displayPropertiesModule.ModuleOwner, Is.EqualTo(moduleOwner));
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
            displayPropertiesModule.IsModuleActive = false;
            displayPropertiesModule.OnSelectedHomeDeviceChanged();
            Assert.That(privateObject.GetFieldOrProperty("isSelectChanged"), Is.EqualTo(true));
        }

        [Test]
        public void TestOnSelectedHomeDeviceChangeda()
        {
            displayPropertiesModule.IsModuleActive = true;
            displayPropertiesModule.OnSelectedHomeDeviceChanged();
            Assert.That(privateObject.GetFieldOrProperty("isSelectChanged"), Is.EqualTo(false));
        }

        [Test]
        public void TestOnActivated()
        {
            privateObject.SetFieldOrProperty("isSelectChanged", true);
            try
            {
                displayPropertiesModule.OnActivated();
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
            var colorModule = new DisplayPropertiesModule();
            try
            {
                displayPropertiesModule.OnDeactivated();
                Assert.True(true);
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }
        }
    }
}