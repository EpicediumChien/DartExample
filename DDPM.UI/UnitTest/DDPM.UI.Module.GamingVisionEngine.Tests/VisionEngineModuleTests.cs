using DDPM.SA.Common;
using DDPM.UI.Common;
using DDPM.UI.Common.Interfaces;
using DDPM.UI.Common.Models;
using Moq;
using NGA.UnitTest.PrivateObject;
using System.Windows;
using System.Windows.Controls;
using VcpCore.Common;
using Dell.Client.Framework.UX.WPF.ResourceManager;

namespace DDPM.UI.Module.GamingVisionEngine.Tests
{
    [Apartment(ApartmentState.STA)]
    public class VisionEngineModuleTests
    {
        private VisionEngineModule? visionEngineModule;
        private PrivateObject? privateObject;
        private Mock<IModuleOwner>? moduleOwnerMock;
        private IModuleOwner? moduleOwner;
        private Mock<IDeviceManagerSA>? deviceManagerSAMock;
        private IDeviceManagerSA? deviceManagerSA;
        private HomeDevice? selectedHomeDevice;
        //private GamingViewModel? gamingViewModel;
        // private GamingModule? myModule;

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
            DdpmCommonHelper.ModuleOwner = moduleOwner;
            deviceManagerSAMock = new Mock<IDeviceManagerSA>();
            deviceManagerSA = deviceManagerSAMock.Object;
            DdpmCommonHelper.DeviceManagerSA = deviceManagerSA;
            selectedHomeDevice = new HomeDevice();
            moduleOwnerMock.Setup(x => x.SelectedHomeDevice).Returns(selectedHomeDevice);
            deviceManagerSAMock.Setup(x => x.GetGamingProperties_SupportedList(It.IsAny<MonitorInfo>())).Returns(Task.FromResult(new GamingDisplayPropertiesInfo()));
            visionEngineModule = new VisionEngineModule();
            privateObject = new PrivateObject(visionEngineModule);
        }

        [Test]
        public void TestTestConstructor_VisionEngineModule()
        {
            // Assert
            Assert.That(visionEngineModule, Is.Not.Null);
            UserControl _rightView = (UserControl)privateObject.GetFieldOrProperty("_rightView");
            Assert.That(_rightView.DataContext, Is.EqualTo(privateObject.GetFieldOrProperty("vm")));
        }

        [Test]
        public void TestModuleName()
        {
            // Act
            var result = visionEngineModule!.ModuleName;

            // Assert
            Assert.That(result, Is.EqualTo("VisionEngineModule"));
        }

        [Test]
        public void TestGetLeftView()
        {
            // Act
            var result = visionEngineModule!.GetLeftView();

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public void TestGetRightView()
        {
            // Act
            var result = visionEngineModule!.GetRightView();

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
            visionEngineModule!.SelectedHomeDevice = homeDevice;
            var result = visionEngineModule.SelectedHomeDevice;

            // Assert
            Assert.That(result, Is.EqualTo(homeDevice));
        }

        [Test]
        public void TestModuleOwner()
        {
            visionEngineModule.ModuleOwner = moduleOwner;
            // Arrange
            Assert.That(visionEngineModule.ModuleOwner, Is.EqualTo(moduleOwner));
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
            try
            {
                visionEngineModule.OnSelectedHomeDeviceChanged();
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
                visionEngineModule.OnActivated();
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
                visionEngineModule.OnDeactivated();
                Assert.True(true);
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }
        }
    }
}