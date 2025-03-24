using DDPM.SA.Common;
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
using Dell.Client.Framework.UX.WPF.ResourceManager;

namespace DDPM.UI.Module.HeadsetAudioSettings.Tests
{
    [Apartment(ApartmentState.STA)]
    public class HeadsetAudioSettingsModuleTests
    {
        private HeadsetAudioSettingsModule? headsetAudioSettingsModule;
        private HeadsetViewModel? vm;
        private Mock<ILog>? logMock;
        private ILog? log;
        private Mock<IShowPluginManager>? showPluginManagerMock;
        private IShowPluginManager? showPluginManager;
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
            showPluginManagerMock = new Mock<IShowPluginManager>();
            showPluginManager = showPluginManagerMock.Object;
            vm = new HeadsetViewModel(console, log, deviceManagerSA);
            moduleOwnerMock = new Mock<IModuleOwner>();
            moduleOwner = moduleOwnerMock!.Object;
            DdpmCommonHelper.ModuleOwner = moduleOwner;
            var currentDeviceInfo = new DeviceInfo();
            vm.CurrentDeviceInfo = currentDeviceInfo;
            headsetAudioSettingsModule = new HeadsetAudioSettingsModule(vm);
            privateObject = new PrivateObject(headsetAudioSettingsModule);
        }

        [Test]
        public void TestConstructor_HeadsetAudioSettingsModule()
        {
            // Act
            var result = privateObject.GetFieldOrProperty("_rightView");

            // Assert
            Assert.That(headsetAudioSettingsModule, Is.Not.Null);
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void TestModuleName()
        {
            // Act
            var result = headsetAudioSettingsModule!.ModuleName;

            // Assert
            Assert.That(result, Is.EqualTo("HeadsetAudioSettingsModule"));
        }

        [Test]
        public void TestGetLeftView()
        {
            // Act
            var result = headsetAudioSettingsModule!.GetLeftView();

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public void TestGetRightView()
        {
            // Act
            var result = headsetAudioSettingsModule!.GetRightView();

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
            headsetAudioSettingsModule!.SelectedHomeDevice = homeDevice;
            var result = headsetAudioSettingsModule.SelectedHomeDevice;

            // Assert
            Assert.That(result, Is.EqualTo(homeDevice));
        }

        [Test]
        public void TestModuleOwner()
        {
            headsetAudioSettingsModule.ModuleOwner = moduleOwner;
            // Arrange
            Assert.That(headsetAudioSettingsModule.ModuleOwner, Is.EqualTo(moduleOwner));
        }

        [Test]
        public void TestOnSelectedHomeDeviceChanged()
        {
            headsetAudioSettingsModule.IsModuleActive = false;
            headsetAudioSettingsModule.OnSelectedHomeDeviceChanged();
            Assert.That(privateObject.GetFieldOrProperty("isSelectChanged"), Is.EqualTo(true));
        }

        [Test]
        public void TestOnSelectedHomeDeviceChangeda()
        {
            headsetAudioSettingsModule.IsModuleActive = true;
            headsetAudioSettingsModule.OnSelectedHomeDeviceChanged();
            Assert.That(privateObject.GetFieldOrProperty("isSelectChanged"), Is.EqualTo(false));
        }

        [Test]
        public void TestOnActivated()
        {
            try
            {
                headsetAudioSettingsModule.OnActivated();
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
                headsetAudioSettingsModule.OnDeactivated();
                Assert.True(true);
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }
        }
    }
}