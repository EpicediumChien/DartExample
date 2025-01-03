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
using Dell.Client.Framework.UX.WPF.ResourceManager;
using System.Windows.Controls;

namespace DDPM.UI.Module.KeyCustomization.Tests
{
    [TestFixture, Apartment(ApartmentState.STA)]
    public class KeyCustomizationModuleTests
    {
        private KeyCustomizationModule? keyCustomizationModule;
        private KeyboardViewModel? vm;
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
            vm = new KeyboardViewModel(console, log);
            keyCustomizationModule = new KeyCustomizationModule(vm);
            privateObject = new PrivateObject(keyCustomizationModule);
        }

        [Test]
        public void TestConstructor_KeyCustomizationModule()
        {
            Assert.NotNull(keyCustomizationModule);
            Assert.NotNull(privateObject.GetFieldOrProperty("_rightView"));
            Assert.That(privateObject.GetFieldOrProperty("_rightView"), Is.InstanceOf<UserControl>());
        }

        [Test]
        public void TestModuleName()
        {
            // Act
            var result = keyCustomizationModule!.ModuleName;

            // Assert
            Assert.That(result, Is.EqualTo("KeyCustomizationModule"));
        }


        [Test]
        public void TestGetLeftView()
        {
            //var mockHomeDevice = new Mock<HomeDevice>();
            //var brightnessModule = new BrightnessModule(/*mockHomeDevice.Object*/);

            //Act:Call the methods to test
            var leftView = keyCustomizationModule.GetLeftView();

            Assert.That(leftView, Is.Null);
        }

        [Test]
        public void TestGetRightView()
        {
            //var mockHomeDevice = new Mock<HomeDevice>();
            var rightView = keyCustomizationModule.GetRightView();
            Assert.That(rightView, Is.Not.Null);
            Assert.That(rightView, Is.InstanceOf<UserControl>());
        }

        [Test]//Test method
        public void TestSelectedHomeDevice()
        {
            //Arrange:Initialize object and define var info
            var homeDevice = new HomeDevice();

            // Act
            keyCustomizationModule!.SelectedHomeDevice = homeDevice;
            var result = keyCustomizationModule.SelectedHomeDevice;

            //Assert:Verify
            Assert.That(result, Is.EqualTo(homeDevice));
        }

        [Test]
        public void TestModuleOwner()
        {
            var mockModuleOwner = new Mock<IModuleOwner>();
            var mockHomeDevice = new Mock<HomeDevice>();

            keyCustomizationModule.ModuleOwner = mockModuleOwner.Object;

            Assert.That(keyCustomizationModule.ModuleOwner, Is.EqualTo(null));
        }

        [Test]
        public void TestOnSelectedHomeDeviceChanged()
        {
            keyCustomizationModule.IsModuleActive = false;
            keyCustomizationModule.OnSelectedHomeDeviceChanged();
            Assert.That(privateObject.GetFieldOrProperty("isSelectChanged"), Is.EqualTo(true));
        }

        [Test]
        public void TestOnSelectedHomeDeviceChangeda()
        {
            keyCustomizationModule.IsModuleActive = true;
            keyCustomizationModule.OnSelectedHomeDeviceChanged();
            Assert.That(privateObject.GetFieldOrProperty("isSelectChanged"), Is.EqualTo(false));
        }

        [Test]
        public void TestOnActivated()
        {
            keyCustomizationModule.IsModuleActive = true;
            try
            {
                keyCustomizationModule.OnActivated();
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
                keyCustomizationModule.OnDeactivated();
                Assert.True(true);
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }
        }
    }
}