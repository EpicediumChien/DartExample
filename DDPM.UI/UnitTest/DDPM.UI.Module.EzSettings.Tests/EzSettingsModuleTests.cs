using DDPM.UI.Common.Interfaces;
using DDPM.UI.Common.Models;
using DDPM.UI.Common;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.UX.WPF;
using NGA.UnitTest.PrivateObject;
using System.Windows;
using Dell.Client.Framework.UX.WPF.ResourceManager;
using Moq;
using DDPM.SA.Common;
using System.Windows.Controls;

namespace DDPM.UI.Module.EzSettings.Tests
{
    [TestFixture, Apartment(ApartmentState.STA)]
    public class EzSettingsModuleTests
    {
        private EzSettingsModule? ezSettingsModule;
        private EzSettingsViewModel? vm;
        private Mock<IConsole>? consoleMock;
        private IConsole? console;
        private Mock<ILog>? logMock;
        private ILog? log;
        private Mock<IDeviceManagerSA>? deviceManagerMock;
        private IDeviceManagerSA? deviceManager;
        private PrivateObject? privateObject;
        private Mock<IModuleOwner> moduleOwnerMock;

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
            logMock = new Mock<ILog>();
            log = logMock.Object;
            consoleMock = new Mock<IConsole>();
            console = consoleMock.Object;
            consoleMock.Setup(x => x.CreateLog(It.IsAny<string>())).Returns(log);
            deviceManagerMock = new Mock<IDeviceManagerSA>();
            deviceManager = deviceManagerMock.Object;
            DdpmCommonHelper.DeviceManagerSA = deviceManager;
            DdpmCommonHelper.MyConsole= console;
            moduleOwnerMock=new Mock<IModuleOwner>();
            vm = new EzSettingsViewModel(moduleOwnerMock.Object);
            ezSettingsModule = new EzSettingsModule(moduleOwnerMock.Object);
            privateObject = new PrivateObject(ezSettingsModule);
        }

        [Test]
        public void TestConstructor_EzSettingsModule()
        {
            Assert.NotNull(ezSettingsModule);
            Assert.NotNull(privateObject.GetFieldOrProperty("_rightView"));
            Assert.That(privateObject.GetFieldOrProperty("_rightView"), Is.InstanceOf<EzSettingsRightView>());
        }

        [Test]
        public void TestModuleName()
        {
            // Act
            var result = ezSettingsModule!.ModuleName;

            // Assert
            Assert.That(result, Is.EqualTo("EzSettingsModule"));
        }


        [Test]
        public void TestGetLeftView()
        {
            //var mockHomeDevice = new Mock<HomeDevice>();
            //var brightnessModule = new BrightnessModule(/*mockHomeDevice.Object*/);

            //Act:Call the methods to test
            var leftView = ezSettingsModule.GetLeftView();

            Assert.That(leftView, Is.Null);
        }

        [Test]
        public void TestGetRightView()
        {
            //var mockHomeDevice = new Mock<HomeDevice>();
            var rightView = ezSettingsModule.GetRightView();
            Assert.That(rightView, Is.Not.Null);
            Assert.That(rightView, Is.InstanceOf<UserControl>());
        }

        [Test]//Test method
        public void TestSelectedHomeDevice()
        {
            //Arrange:Initialize object and define var info
            var homeDevice = new HomeDevice();

            // Act
            ezSettingsModule!.SelectedHomeDevice = homeDevice;
            var result = ezSettingsModule.SelectedHomeDevice;

            //Assert:Verify
            Assert.That(result, Is.EqualTo(homeDevice));
        }

        [Test]
        public void TestModuleOwner()
        {
            var mockModuleOwner = new Mock<IModuleOwner>();
            var mockHomeDevice = new Mock<HomeDevice>();

            ezSettingsModule.ModuleOwner = mockModuleOwner.Object;

            Assert.That(ezSettingsModule.ModuleOwner, Is.EqualTo(mockModuleOwner.Object));
        }

        [Test]
        public void TestOnSelectedHomeDeviceChanged()
        {
            ezSettingsModule.IsModuleActive = false;
            ezSettingsModule.OnSelectedHomeDeviceChanged();
            Assert.That(privateObject.GetFieldOrProperty("isSelectChanged"), Is.EqualTo(true));
        }

        [Test]
        public void TestOnSelectedHomeDeviceChangeda()
        {
            ezSettingsModule.IsModuleActive = true;
            ezSettingsModule.OnSelectedHomeDeviceChanged();
            Assert.That(privateObject.GetFieldOrProperty("isSelectChanged"), Is.EqualTo(false));
        }

        [Test]
        public void TestOnActivated()
        {
            ezSettingsModule.IsModuleActive = true;
            try
            {
                ezSettingsModule.OnActivated();
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
                ezSettingsModule.OnDeactivated();
                Assert.True(true);
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }
        }
    }
}