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
using System.Reflection;
namespace DDPM.UI.Module.PenSettings.Tests
{
    [TestFixture, Apartment(ApartmentState.STA)]
    public class PenSettingsModuleTests
    {
        private PenSettingsModule? penSettingsModule;
        private PenViewModel? vm;
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
            deviceManagerMock.Setup(x => x.GetEraserDoublePressSetting()).Returns(Task.FromResult("{\r\n    \"actionId\": 90,\r\n    \"actionName\": \"Screen Snipping\"\r\n}"));
            deviceManagerMock.Setup(x => x.GetEraserSinglePressSetting()).Returns(Task.FromResult("{\r\n    \"actionId\": 90,\r\n    \"actionName\": \"Screen Snipping\"\r\n}"));
            deviceManagerMock.Setup(x => x.GetEraserLongPressSetting()).Returns(Task.FromResult("{\r\n    \"actionId\": 90,\r\n    \"actionName\": \"Screen Snipping\"\r\n}"));
            deviceManagerMock.Setup(x => x.GetSideTopSwitchSinglePressSetting()).Returns(Task.FromResult("{\r\n    \"actionId\": 90,\r\n    \"actionName\": \"Screen Snipping\"\r\n}"));
            deviceManagerMock.Setup(x => x.GetSideBottomSwitchSinglePressSetting()).Returns(Task.FromResult("{\r\n    \"actionId\": 90,\r\n    \"actionName\": \"Screen Snipping\"\r\n}"));
            deviceManagerMock.Setup(x => x.GetMenuSinglePressSetting()).Returns(Task.FromResult("{\r\n    \"actionId\": 90,\r\n    \"actionName\": \"Screen Snipping\"\r\n}"));
            vm = new PenViewModel(console, log) { CurrentDeviceInfo = new DeviceInfo() };
            penSettingsModule = new PenSettingsModule(vm);
            privateObject = new PrivateObject(penSettingsModule);
        }

        [Test]
        public void TestConstructor_PenSettingsModule()
        {
            Assert.NotNull(penSettingsModule);
            Assert.NotNull(privateObject.GetFieldOrProperty("_rightView"));
            Assert.That(privateObject.GetFieldOrProperty("_rightView"),Is.InstanceOf<PenSettingsRightView>());
        }

        [Test]
        public void TestModuleName()
        {
            // Act
            var result = penSettingsModule!.ModuleName;

            // Assert
            Assert.That(result, Is.EqualTo("PenSettingsModule"));
        }


        [Test]
        public void TestGetLeftView()
        {
            //var mockHomeDevice = new Mock<HomeDevice>();
            //var brightnessModule = new BrightnessModule(/*mockHomeDevice.Object*/);

            //Act:Call the methods to test
            var leftView = penSettingsModule.GetLeftView();

            Assert.That(leftView, Is.Null);
        }

        [Test]
        public void TestGetRightView()
        {
            //var mockHomeDevice = new Mock<HomeDevice>();
            var rightView = penSettingsModule.GetRightView();
            Assert.That(rightView, Is.Not.Null);
            Assert.That(rightView, Is.InstanceOf<UserControl>());
        }

        [Test]//Test method
        public void TestSelectedHomeDevice()
        {
            //Arrange:Initialize object and define var info
            var homeDevice = new HomeDevice();

            // Act
            penSettingsModule!.SelectedHomeDevice = homeDevice;
            var result = penSettingsModule.SelectedHomeDevice;

            //Assert:Verify
            Assert.That(result, Is.EqualTo(homeDevice));
        }

        [Test]
        public void TestModuleOwner()
        {
            var mockModuleOwner = new Mock<IModuleOwner>();
            var mockHomeDevice = new Mock<HomeDevice>();

            penSettingsModule.ModuleOwner = mockModuleOwner.Object;

            Assert.That(penSettingsModule.ModuleOwner, Is.EqualTo(mockModuleOwner.Object));
        }

        [Test]
        public void TestOnSelectedHomeDeviceChanged()
        {
            penSettingsModule.IsModuleActive=false;
            penSettingsModule.OnSelectedHomeDeviceChanged();
            Assert.That(privateObject.GetFieldOrProperty("isSelectChanged"),Is.EqualTo(true));
        }

        [Test]
        public void TestOnSelectedHomeDeviceChangeda()
        {
            penSettingsModule.IsModuleActive = true;
            penSettingsModule.OnSelectedHomeDeviceChanged();
            Assert.That(privateObject.GetFieldOrProperty("isSelectChanged"), Is.EqualTo(false));
        }

        [Test]
        public void TestOnActivated()
        {
            penSettingsModule.IsModuleActive = true;
            try
            {
                penSettingsModule.OnActivated();
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
                penSettingsModule.OnDeactivated();
                Assert.True(true);
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }
        }
    }
}