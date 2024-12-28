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
using DDPM.SA.Common.Settings;
using Dell.Client.Framework.UX.WPF.ResourceManager;
using System.Windows;

namespace DDPM.UI.Module.WebCameraCapture.Tests
{
    [Apartment(ApartmentState.STA)]
    public class WebCameraCaptureModuleTests
    {
        private WebCameraCaptureModule? webCameraCaptureModule;
        private PrivateObject? privateObject;
        private Mock<IModuleOwner>? moduleOwnerMock;
        private IModuleOwner? moduleOwner;
        private WebCameraViewModel? vm;
        private IConsole? console;
        private Mock<IConsole>? consoleMock;
        private IShowPluginManager? showPluginManager;
        private Mock<IShowPluginManager>? showPluginManagerMock;
        private IDeviceManagerSA? deviceManager;
        private Mock<IDeviceManagerSA>? deviceManagerMock;
        private ILog? log;
        private Mock<ILog>? logMock;
        private WebcamSettings? webcamSettings;
        private WebCameraCaptureRightView? webCameraCaptureRightView;

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
            consoleMock = new Mock<IConsole>();
            console = consoleMock.Object;
            showPluginManagerMock = new Mock<IShowPluginManager>();
            showPluginManager = showPluginManagerMock.Object;
            deviceManagerMock = new Mock<IDeviceManagerSA>();
            deviceManager = deviceManagerMock.Object;
            DdpmCommonHelper.DeviceManagerSA = deviceManager;
            logMock = new Mock<ILog>();
            log = logMock.Object;
            vm = new WebCameraViewModel(console, log);
            webcamSettings = new WebcamSettings();
            webcamSettings.SupportedFPSs = new Dictionary<string, List<string>>();
            webcamSettings.SupportedFPSs.Add("a",new List<string> { "a"});
            webcamSettings.Selected_Resolution = "a";
            webcamSettings.SelectedFPSs = new Dictionary<string, string>();
            webcamSettings.SelectedFPSs.Add("a", "a");
            webcamSettings.Resolutions = new Dictionary<string, string>();
            webcamSettings.Resolutions.Add("a", "a");           
            vm.WebcamSettings = webcamSettings;
            webCameraCaptureRightView = new WebCameraCaptureRightView(vm);
            webCameraCaptureModule = new WebCameraCaptureModule(vm);
            privateObject = new PrivateObject(webCameraCaptureModule);
        }

        [Test]
        public void TestConstructor_WebCameraCaptureModule()
        {
            // Assert
            Assert.That(webCameraCaptureModule, Is.Not.Null);
            Assert.That(privateObject.GetFieldOrProperty("_rightView"), Is.Not.Null);
        }

        [Test]
        public void TestModuleName()
        {
            // Act
            var result = webCameraCaptureModule!.ModuleName;

            // Assert
            Assert.That(result, Is.EqualTo("WebCameraCaptureModule"));
        }

        [Test]
        public void TestGetLeftView()
        {
            // Act
            var result = webCameraCaptureModule!.GetLeftView();

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public void TestGetRightView()
        {
            // Act
            var result = webCameraCaptureModule!.GetRightView();

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
            webCameraCaptureModule!.SelectedHomeDevice = homeDevice;
            var result = webCameraCaptureModule.SelectedHomeDevice;

            // Assert
            Assert.That(result, Is.EqualTo(homeDevice));
        }

        [Test]
        public void TestModuleOwner()
        {
            // Arrange
            webCameraCaptureModule.ModuleOwner = moduleOwner;
            Assert.That(webCameraCaptureModule.ModuleOwner, Is.EqualTo(moduleOwner));
        }

        [Test]
        public void TestOnSelectedHomeDeviceChanged()
        {
            webCameraCaptureModule.IsModuleActive = false;
            webCameraCaptureModule.OnSelectedHomeDeviceChanged();
            Assert.That(privateObject.GetFieldOrProperty("isSelectChanged"), Is.EqualTo(true));
        }

        [Test]
        public void TestOnSelectedHomeDeviceChangeda()
        {
            webCameraCaptureModule.IsModuleActive = true;
            webCameraCaptureModule.OnSelectedHomeDeviceChanged();
            Assert.That(privateObject.GetFieldOrProperty("isSelectChanged"), Is.EqualTo(false));
        }

        [Test]
        public void TestOnActivated()
        {
            try
            {
                webCameraCaptureModule.OnActivated();
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
                webCameraCaptureModule.OnDeactivated();
                Assert.True(true);
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }
        }
    }
}