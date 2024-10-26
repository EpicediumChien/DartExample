using DDPM.SA.Common;
using DDPM.SA.Common.Settings;
using DDPM.UI.Common;
using DDPM.UI.Common.Interfaces;
using DDPM.UI.Common.Models;
using DDPM.UI.Plugin.ViewModels;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.UX.WPF;
using Moq;
using NGA.UnitTest.PrivateObject;
using System.Windows;
using System.Windows.Controls;

namespace DDPM.UI.Module.WebCameraPresenceDetection.Tests
{
    [TestFixture, Apartment(ApartmentState.STA)]
    public class WebCameraPresenceDetectionModuleTests
    {
        private WebCameraPresenceDetectionModule? webCameraPresenceDetectionModule;
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
            deviceManagerMock.Setup(x => x.GetIsProximitySensorEnable(It.IsAny<string>())).Returns(Task.FromResult(true));
            deviceManagerMock.Setup(x => x.GetIsWakeonApproachEnable(It.IsAny<string>())).Returns(Task.FromResult(true));
            deviceManagerMock.Setup(x => x.GetIsWalkAwayLockEnable(It.IsAny<string>())).Returns(Task.FromResult(true));
            deviceManagerMock.Setup(x => x.GetWALTime(It.IsAny<string>())).Returns(Task.FromResult(1));

            deviceManagerMock.Setup(x => x.GetSnooze(It.IsAny<string>())).Returns(Task.FromResult(1));
            deviceManagerMock.Setup(x => x.GetSnoozeLength(It.IsAny<string>())).Returns(Task.FromResult(1));
            deviceManagerMock.Setup(x => x.ReloadAppConfigData(It.IsAny<bool>())).Returns(Task.FromResult(new DDPMSettings(new DDPMAppSettings (), new DDPMUserSettings (), new DDPMITConfig ())));
            var CurrentDeviceInfo = new DeviceInfo() { ID=new Guid()};
            vm = new WebCameraViewModel(console, log);
            vm.CurrentDeviceInfo = CurrentDeviceInfo;
            webCameraPresenceDetectionModule = new WebCameraPresenceDetectionModule(vm);
            privateObject = new PrivateObject(webCameraPresenceDetectionModule);
        }

        [Test]
        public void TestConstructor_WebCameraPresenceDetectionModule()
        {
            Assert.That(webCameraPresenceDetectionModule, Is.Not.Null);
            Assert.That(privateObject.GetFieldOrProperty("_rightView"), Is.Not.Null);
        }

        [Test]
        public void TestModuleName()
        {
            // Act
            var result = webCameraPresenceDetectionModule!.ModuleName;

            // Assert
            Assert.That(result, Is.EqualTo("WebCameraPresenceDetectionModule"));
        }


        [Test]
        public void TestGetLeftView()
        {
            //var mockHomeDevice = new Mock<HomeDevice>();
            //var brightnessModule = new BrightnessModule(/*mockHomeDevice.Object*/);

            //Act:Call the methods to test
            var leftView = webCameraPresenceDetectionModule.GetLeftView();

            Assert.That(leftView, Is.Null);
        }

        [Test]
        public void TestGetRightView()
        {
            //var mockHomeDevice = new Mock<HomeDevice>();
            var rightView = webCameraPresenceDetectionModule.GetRightView();
            Assert.That(rightView, Is.InstanceOf<UserControl>());
        }

        [Test]//Test method
        public void TestSelectedHomeDevice()
        {
            //Arrange:Initialize object and define var info
            var homeDevice = new HomeDevice();

            // Act
            webCameraPresenceDetectionModule!.SelectedHomeDevice = homeDevice;
            var result = webCameraPresenceDetectionModule.SelectedHomeDevice;

            //Assert:Verify
            Assert.That(result, Is.EqualTo(homeDevice));
        }

        [Test]
        public void TestModuleOwner()
        {
            var mockModuleOwner = new Mock<IModuleOwner>();
            var mockHomeDevice = new Mock<HomeDevice>();

            webCameraPresenceDetectionModule.ModuleOwner = mockModuleOwner.Object;

            Assert.That(webCameraPresenceDetectionModule.ModuleOwner, Is.EqualTo(mockModuleOwner.Object));
        }

        [Test]
        public void TestOnSelectedHomeDeviceChanged()
        {
            try
            {
                webCameraPresenceDetectionModule.OnSelectedHomeDeviceChanged();
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
                webCameraPresenceDetectionModule.OnActivated();
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
                webCameraPresenceDetectionModule.OnDeactivated();
                Assert.True(true);
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }
        }
    }
}