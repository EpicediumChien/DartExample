using DDPM.UI.Common.Interfaces;
using DDPM.UI.Common.Models;
using DDPM.UI.Common;
using DDPM.UI.Plugin.ViewModels;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.UX.WPF;
using NGA.UnitTest.PrivateObject;
using System.Windows;
using System.Windows.Controls;
using Moq;
using DDPM.SA.Common;
using Dell.Client.Framework.UX.WPF.ResourceManager;

namespace DDPM.UI.Module.MouseSettings.Tests
{
    [TestFixture, Apartment(ApartmentState.STA)]
    public class MouseSettingsModuleTests
    {
        private MouseSettingsModule? mouseSettingsModule;
        private MouseViewModel? vm;
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
            vm = new MouseViewModel(console, log);
            mouseSettingsModule = new MouseSettingsModule(vm);
            privateObject = new PrivateObject(mouseSettingsModule);
        }

        [Test]
        public void TestConstructor_MouseSettingsModule()
        {
            Assert.NotNull(mouseSettingsModule);
            Assert.NotNull(privateObject.GetFieldOrProperty("_rightView"));
            Assert.That(privateObject.GetFieldOrProperty("_rightView"), Is.InstanceOf<MouseSettingsRightView>());
        }

        [Test]
        public void TestModuleName()
        {
            // Act
            var result = mouseSettingsModule!.ModuleName;

            // Assert
            Assert.That(result, Is.EqualTo("MouseSettingsModule"));
        }


        [Test]
        public void TestGetLeftView()
        {
            //var mockHomeDevice = new Mock<HomeDevice>();
            //var brightnessModule = new BrightnessModule(/*mockHomeDevice.Object*/);

            //Act:Call the methods to test
            var leftView = mouseSettingsModule.GetLeftView();

            Assert.That(leftView, Is.Null);
        }

        [Test]
        public void TestGetRightView()
        {
            //var mockHomeDevice = new Mock<HomeDevice>();
            var rightView = mouseSettingsModule.GetRightView();
            Assert.That(rightView, Is.Not.Null);
            Assert.That(rightView, Is.InstanceOf<UserControl>());
        }

        [Test]//Test method
        public void TestSelectedHomeDevice()
        {
            //Arrange:Initialize object and define var info
            var homeDevice = new HomeDevice();

            // Act
            mouseSettingsModule!.SelectedHomeDevice = homeDevice;
            var result = mouseSettingsModule.SelectedHomeDevice;

            //Assert:Verify
            Assert.That(result, Is.EqualTo(homeDevice));
        }

        [Test]
        public void TestModuleOwner()
        {
            var mockModuleOwner = new Mock<IModuleOwner>();
            var mockHomeDevice = new Mock<HomeDevice>();

            mouseSettingsModule.ModuleOwner = mockModuleOwner.Object;

            Assert.That(mouseSettingsModule.ModuleOwner, Is.EqualTo(mockModuleOwner.Object));
        }

        [Test]
        public void TestOnSelectedHomeDeviceChanged()
        {
            mouseSettingsModule.IsModuleActive = false;
            mouseSettingsModule.OnSelectedHomeDeviceChanged();
            Assert.That(privateObject.GetFieldOrProperty("isSelectChanged"), Is.EqualTo(true));
        }

        [Test]
        public void TestOnSelectedHomeDeviceChangeda()
        {
            mouseSettingsModule.IsModuleActive = true;
            mouseSettingsModule.OnSelectedHomeDeviceChanged();
            Assert.That(privateObject.GetFieldOrProperty("isSelectChanged"), Is.EqualTo(false));
        }

        [Test]
        public void TestOnActivated()
        {
            mouseSettingsModule.IsModuleActive = true;
            try
            {
                mouseSettingsModule.OnActivated();
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
                mouseSettingsModule.OnDeactivated();
                Assert.True(true);
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }
        }
    }
}