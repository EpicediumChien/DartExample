using DDPM.UI.Common.Interfaces;
using DDPM.UI.Common.Models;
using DDPM.UI.Common;
using DDPM.UI.Plugin.ViewModels;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.UX.WPF;
using NGA.UnitTest.PrivateObject;
using System.Windows;
using DDPM.UI.Common.ViewModels;
using DDPM.UI.Plugin.Common.ViewModels;
using Moq;
using DDPM.SA.Common.Interfaces;
using DDPM.SA.Common;
using DDPM.SA.Common.Settings;
using System.Windows.Controls;
using VcpCore.Common;
using Dell.Client.Framework.UX.WPF.ResourceManager;

namespace DDPM.UI.Module.EzMemory.Tests
{
    [TestFixture, Apartment(ApartmentState.STA)]
    public class EzMemoryModuleTests
    {
        private EzMemoryModule? ezMemoryModule;
        private PrivateObject? privateObject;
        private Mock<IModuleOwner>? moduleOwnerMock;
        private IModuleOwner? moduleOwner;
        private DisplayViewModel? vmDisplay;
        private IConsole? console;
        private Mock<IConsole>? consoleMock;
        private IEasyArrangeService? easyArrange;
        private Mock<IEasyArrangeService>? easyArrangeMock;
        private IDeviceManagerSA? deviceManagerSA;
        private Mock<IDeviceManagerSA>? deviceManagerSAMock;
        private ILog? log;
        private Mock<ILog>? logMock;


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
            consoleMock = new Mock<IConsole>();
            console = consoleMock.Object;
            DdpmCommonHelper.MyConsole = console;
            easyArrangeMock = new Mock<IEasyArrangeService>();
            easyArrange = easyArrangeMock.Object;
            deviceManagerSAMock = new Mock<IDeviceManagerSA>();
            //Robert_Lin, 2025-1-7, IsEAFunctionEnabled is deleted
            //deviceManagerSAMock.Setup(x => x.GetEAFunctionEnabled()).Returns(Task.FromResult(new ObjGetVCP() { result = true, value = true }));
            DdpmCommonHelper.DeviceManagerSA = deviceManagerSA;
            deviceManagerSA = deviceManagerSAMock.Object;
            HomeDevice.DeviceManagerSA = deviceManagerSAMock.Object;
            logMock = new Mock<ILog>();
            log = logMock.Object;
            consoleMock.Setup(x => x.CreateLog(It.IsAny<string>())).Returns(logMock.Object);
            ezMemoryModule = new EzMemoryModule(new DisplayViewModel(console, log, deviceManagerSA, easyArrange) { SelectedHomeDevice = new HomeDevice() { MonitorInfo = new MonitorInfo() { DisplayName = "AA" } } });
            privateObject = new PrivateObject(ezMemoryModule);
        }

        [Test]
        public void TestConstructor_EzMemoryModule()
        {
            // Assert
            Assert.That(ezMemoryModule, Is.Not.Null);
        }

        [Test]
        public void TestModuleName()
        {
            // Act
            var result = ezMemoryModule!.ModuleName;

            // Assert
            Assert.That(result, Is.EqualTo("EzMemoryModule"));
        }

        [Test]
        public void TestGetLeftView()
        {
            // Act
            var result = ezMemoryModule!.GetLeftView();

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public void TestGetRightView()
        {
            // Act
            var _vmDisplay = new DisplayViewModel(console, log, deviceManagerSA, easyArrange) { SelectedHomeDevice = new HomeDevice() { MonitorInfo = new MonitorInfo() { DisplayName = "displayName" }, vmEzArrange = new EzArrangeViewModel(new HomeDevice()) { SelectedSplitItem = new Common.UserControls.SplitItem() } } };
            privateObject.SetFieldOrProperty("_vmDisplay", _vmDisplay);
            var result = ezMemoryModule!.GetRightView();
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
            ezMemoryModule!.SelectedHomeDevice = homeDevice;
            var result = ezMemoryModule.SelectedHomeDevice;

            // Assert
            Assert.That(result, Is.EqualTo(homeDevice));
        }

        [Test]
        public void TestModuleOwner()
        {
            // Arrange
            ezMemoryModule.ModuleOwner = moduleOwner;
            Assert.That(ezMemoryModule.ModuleOwner, Is.EqualTo(moduleOwner));
        }

        [Test]
        public void TestOnSelectedHomeDeviceChanged()
        {
            ezMemoryModule.IsModuleActive = false;
            ezMemoryModule.OnSelectedHomeDeviceChanged();
            Assert.That(privateObject.GetFieldOrProperty("isSelectChanged"), Is.EqualTo(true));
        }

        [Test]
        public void TestOnSelectedHomeDeviceChangeda()
        {
            ezMemoryModule.IsModuleActive = true;
            ezMemoryModule.OnSelectedHomeDeviceChanged();
            Assert.That(privateObject.GetFieldOrProperty("isSelectChanged"), Is.EqualTo(false));
        }

        [Test]
        public void TestOnActivated()
        {
            privateObject.SetFieldOrProperty("isSelectChanged", true);
            try
            {
                ezMemoryModule.OnActivated();
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
                ezMemoryModule.OnDeactivated();
                Assert.True(true);
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }
        }
    }
}