using DDPM.UI.Common.Interfaces;
using DDPM.UI.Common.Models;
using DDPM.UI.Common;
using DDPM.UI.Plugin.ViewModels;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.UX.WPF;
using NGA.UnitTest.PrivateObject;
using System.Windows;
using Moq;
using DDPM.SA.Common;
using System.Windows.Controls;
using Dell.Client.Framework.UX.WPF.ResourceManager;

namespace DDPM.UI.Module.WebCameraMicrophone.Tests
{
    [TestFixture, Apartment(ApartmentState.STA)]
    public class WebCameraMicrophoneModuleTests
    {
        private WebCameraMicrophoneModule? webCameraMicrophoneModule;
        private WebCameraViewModel? vm;
        private Mock<IConsole>? consoleMock;
        private IConsole? console;
        private Mock<ILog>? logMock;
        private ILog? log;
        private Mock<IDeviceManagerSA>? deviceManagerMock;
        private IDeviceManagerSA? deviceManager;
        private PrivateObject? privateObject;

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
            consoleMock = new Mock<IConsole>();
            console = consoleMock.Object;
            logMock = new Mock<ILog>();
            log = logMock.Object;
            deviceManagerMock = new Mock<IDeviceManagerSA>();
            deviceManager = deviceManagerMock.Object;
            DdpmCommonHelper.DeviceManagerSA = deviceManager;
            vm = new WebCameraViewModel(console, log) { CurrentDeviceInfo = new DeviceInfo() };
            webCameraMicrophoneModule = new WebCameraMicrophoneModule(vm);
            privateObject = new PrivateObject(webCameraMicrophoneModule);
        }

        [Test]
        public void TestConstructor_WebCameraColorImageModule()
        {
            Assert.That(webCameraMicrophoneModule, Is.Not.Null);
            Assert.That(privateObject.GetFieldOrProperty("_rightView"), Is.Not.Null);
        }

        [Test]
        public void TestModuleName()
        {
            // Act
            var result = webCameraMicrophoneModule!.ModuleName;

            // Assert
            Assert.That(result, Is.EqualTo("WebCameraMicrophoneModule"));
        }


        [Test]
        public void TestGetLeftView()
        {
            //var mockHomeDevice = new Mock<HomeDevice>();
            //var brightnessModule = new BrightnessModule(/*mockHomeDevice.Object*/);

            //Act:Call the methods to test
            var leftView = webCameraMicrophoneModule.GetLeftView();

            Assert.That(leftView, Is.Null);
        }

        [Test]
        public void TestGetRightView()
        {
            //var mockHomeDevice = new Mock<HomeDevice>();
            var rightView = webCameraMicrophoneModule.GetRightView();
            Assert.That(rightView, Is.InstanceOf<UserControl>());
        }

        [Test]//Test method
        public void TestSelectedHomeDevice()
        {
            //Arrange:Initialize object and define var info
            var homeDevice = new HomeDevice();

            // Act
            webCameraMicrophoneModule!.SelectedHomeDevice = homeDevice;
            var result = webCameraMicrophoneModule.SelectedHomeDevice;

            //Assert:Verify
            Assert.That(result, Is.EqualTo(homeDevice));
        }

        [Test]
        public void TestModuleOwner()
        {
            var mockModuleOwner = new Mock<IModuleOwner>();
            var mockHomeDevice = new Mock<HomeDevice>();

            webCameraMicrophoneModule.ModuleOwner = mockModuleOwner.Object;

            Assert.That(webCameraMicrophoneModule.ModuleOwner, Is.EqualTo(mockModuleOwner.Object));
        }

        [Test]
        public void TestOnSelectedHomeDeviceChanged()
        {
            webCameraMicrophoneModule.IsModuleActive = false;
            webCameraMicrophoneModule.OnSelectedHomeDeviceChanged();
            Assert.That(privateObject.GetFieldOrProperty("isSelectChanged"), Is.EqualTo(true));
        }

        [Test]
        public void TestOnSelectedHomeDeviceChangeda()
        {
            webCameraMicrophoneModule.IsModuleActive = true;
            webCameraMicrophoneModule.OnSelectedHomeDeviceChanged();
            Assert.That(privateObject.GetFieldOrProperty("isSelectChanged"), Is.EqualTo(false));
        }

        [Test]
        public void TestOnActivated()
        {
            webCameraMicrophoneModule.IsModuleActive = true;
            try
            {
                webCameraMicrophoneModule.OnActivated();
                Assert.True(true);
                Assert.That(privateObject.GetFieldOrProperty("isSelectChanged"),Is.EqualTo(false));
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
                webCameraMicrophoneModule.OnDeactivated();
                Assert.True(true);
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }
        }
    }
}