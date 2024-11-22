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

namespace DDPM.UI.Module.HeadsetDeviceSettings.Tests
{
    [Apartment(ApartmentState.STA)]
    public class HeadsetDeviceSettingsModuleTests
    {
        private HeadsetDeviceSettingsModule? headsetDeviceSettingsModule;
        private HeadsetViewModel? vm;
        private Mock<ILog>? logMock;
        private ILog? log;
        private Mock<IDeviceManagerSA>? deviceManagerSAMock;
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
            ResourceManager res = new ResourceManager();
            var resourceDictionary = new ResourceDictionary();
            resourceDictionary.Source = new Uri("pack://application:,,,/DDPM.UI.Common;component/ModuleStyle.xaml");
            System.Windows.Application.Current.Resources.MergedDictionaries.Add(resourceDictionary);
            logMock = new Mock<ILog>();
            log = logMock.Object;
            var deviceManagerSAMock = new Mock<IDeviceManagerSA>();
            deviceManagerSA = deviceManagerSAMock.Object;
            DdpmCommonHelper.DeviceManagerSA = deviceManagerSA;
            consoleMock = new Mock<IConsole>();
            console = consoleMock.Object;
            vm = new HeadsetViewModel(console, log, deviceManagerSA);
            moduleOwnerMock = new Mock<IModuleOwner>();
            moduleOwner = moduleOwnerMock!.Object;
            DdpmCommonHelper.ModuleOwner = moduleOwner;
            var currentDeviceInfo = new DeviceInfo();
            vm.CurrentDeviceInfo = currentDeviceInfo;
            headsetDeviceSettingsModule = new HeadsetDeviceSettingsModule(vm);
            privateObject = new PrivateObject(headsetDeviceSettingsModule);
        }

        [Test]
        public void TestConstructor_HeadsetDeviceSettingsModule()
        {
            // Act
            var result = privateObject.GetFieldOrProperty("_rightView");

            // Assert
            Assert.That(headsetDeviceSettingsModule, Is.Not.Null);
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void TestIsModuleActive()
        {
            // Act
            headsetDeviceSettingsModule.IsModuleActive = true;

            // Assert
            Assert.That(headsetDeviceSettingsModule.IsModuleActive, Is.EqualTo(true));
        }

        [Test]
        public void TestModuleName()
        {
            // Act
            var result = headsetDeviceSettingsModule!.ModuleName;

            // Assert
            Assert.That(result, Is.EqualTo("HeadsetDeviceSettingsModule"));
        }

        [Test]
        public void TestGetLeftView()
        {
            // Act
            var result = headsetDeviceSettingsModule!.GetLeftView();

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public void TestGetRightView()
        {
            // Act
            var result = headsetDeviceSettingsModule!.GetRightView();

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
            headsetDeviceSettingsModule!.SelectedHomeDevice = homeDevice;
            var result = headsetDeviceSettingsModule.SelectedHomeDevice;

            // Assert
            Assert.That(result, Is.EqualTo(homeDevice));
        }

        [Test]
        public void TestModuleOwner()
        {
            headsetDeviceSettingsModule.ModuleOwner = moduleOwner;
            // Arrange
            Assert.That(headsetDeviceSettingsModule.ModuleOwner, Is.EqualTo(moduleOwner));
        }

        [Test]
        public void TestOnSelectedHomeDeviceChanged()
        {
            try
            {
                headsetDeviceSettingsModule.OnSelectedHomeDeviceChanged();
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
                headsetDeviceSettingsModule.OnActivated();
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
                headsetDeviceSettingsModule.OnDeactivated();
                Assert.True(true);
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }
        }
    }
}