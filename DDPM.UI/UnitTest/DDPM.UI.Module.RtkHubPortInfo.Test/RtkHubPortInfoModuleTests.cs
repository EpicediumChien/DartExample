using DDPM.UI.Common.Interfaces;
using DDPM.UI.Common;
using DDPM.UI.Plugin.ViewModels;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.UX.WPF;
using NGA.UnitTest.PrivateObject;
using Moq;
using DDPM.SA.Common;
using DDPM.UI.Common.Models;
using System.Windows.Controls;
using Dell.Client.Framework.UX.WPF.ResourceManager;
using System.Windows;

namespace DDPM.UI.Module.RtkHubPortInfo.Test
{
    [Apartment(ApartmentState.STA)]
    public class RtkHubPortInfoModuleTests
    {
        private RtkHubPortInfoModule? rtkhubPortInfoModule;
        private RtkHubViewModel? vm;
        private Mock<ILog>? logMock;
        private ILog? log;
        private Mock<IShowPluginManager>? showPluginManagerMock;
        private IDeviceManagerSA? deviceManagerSA;
        private Mock<IConsole>? consoleMock;
        private IConsole? console;
        private PrivateObject? privateObject;
        private Mock<IModuleOwner>? moduleOwnerMock;
        private IModuleOwner? moduleOwner;

        [SetUp]
        public void Setup()
        {
            if (System.Windows.Application.Current == null)
            {
                new System.Windows.Application();
            }
            var resourceDictionary = new ResourceManager();
            var moduleStyle = new ResourceDictionary{Source = new Uri("pack://application:,,,/DDPM.UI.Common;component/ModuleStyle.xaml", UriKind.Absolute)};
            System.Windows.Application.Current.Resources.MergedDictionaries.Add(moduleStyle);
            logMock = new Mock<ILog>();
            log = logMock.Object;
            var deviceManagerSAMock = new Mock<IDeviceManagerSA>();
            deviceManagerSA = deviceManagerSAMock.Object;
            DdpmCommonHelper.DeviceManagerSA = deviceManagerSA;
            consoleMock = new Mock<IConsole>();
            console = consoleMock.Object;
            showPluginManagerMock = new Mock<IShowPluginManager>();
            vm = new RtkHubViewModel(console, log, deviceManagerSA);
            moduleOwnerMock = new Mock<IModuleOwner>();
            moduleOwner = moduleOwnerMock!.Object;
            DdpmCommonHelper.ModuleOwner = moduleOwner;
            var currentDeviceInfo = new DeviceInfo();
            vm.CurrentDeviceInfo = currentDeviceInfo;
            rtkhubPortInfoModule = new RtkHubPortInfoModule(vm);
            privateObject = new PrivateObject(rtkhubPortInfoModule);
        }

        [Test]
        public void TestConstructorRtkHubPortInfoModule()
        {
            // Act
            var result = privateObject.GetFieldOrProperty("_rightView");

            // Assert
            Assert.That(rtkhubPortInfoModule, Is.Not.Null);
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void TestIsModuleActive()
        {
            // Act
            rtkhubPortInfoModule.IsModuleActive = true;

            // Assert
            Assert.That(rtkhubPortInfoModule.IsModuleActive, Is.EqualTo(true));
        }

        [Test]
        public void TestModuleName()
        {
            // Act
            var result = rtkhubPortInfoModule!.ModuleName;

            // Assert
            Assert.That(result, Is.EqualTo("RtkHubPortInfoModule"));
        }

        [Test]
        public void TestGetLeftView()
        {
            // Act
            var result = rtkhubPortInfoModule!.GetLeftView();

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public void TestGetRightView()
        {
            // Act
            var result = rtkhubPortInfoModule!.GetRightView();

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
            rtkhubPortInfoModule!.SelectedHomeDevice = homeDevice;
            var result = rtkhubPortInfoModule.SelectedHomeDevice;

            // Assert
            Assert.That(result, Is.EqualTo(homeDevice));
        }

        [Test]
        public void TestModuleOwner()
        {
            rtkhubPortInfoModule.ModuleOwner = moduleOwner;
            // Arrange
            Assert.That(rtkhubPortInfoModule.ModuleOwner, Is.EqualTo(moduleOwner));
        }

        [Test]
        public void TestOnSelectedHomeDeviceChanged()
        {
            rtkhubPortInfoModule.IsModuleActive = false;
            rtkhubPortInfoModule.OnSelectedHomeDeviceChanged();
            Assert.That(privateObject.GetFieldOrProperty("isSelectChanged"), Is.EqualTo(true));
        }

        [Test]
        public void TestOnSelectedHomeDeviceChangeda()
        {
            rtkhubPortInfoModule.IsModuleActive = true;
            rtkhubPortInfoModule.OnSelectedHomeDeviceChanged();
            Assert.That(privateObject.GetFieldOrProperty("isSelectChanged"), Is.EqualTo(false));
        }

        [Test]
        public void TestOnActivated()
        {
            try
            {
                rtkhubPortInfoModule.OnActivated();
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
                rtkhubPortInfoModule.OnDeactivated();
                Assert.True(true);
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }
        }
    }
}
