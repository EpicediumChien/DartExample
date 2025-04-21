using DDPM.SA.Common;
using DDPM.UI.Common;

using DDPM.UI.Common.Interfaces;
using DDPM.UI.Common.Models;
using DDPM.UI.Plugin.ViewModels;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.UX.WPF;
using Dell.Client.Framework.UX.WPF.ResourceManager;
using Moq;
using NGA.UnitTest.PrivateObject;
using System.Windows;
using System.Windows.Controls;
using static DDPM.UI.Plugin.ViewModels.SoundBarViewModel;

namespace DDPM.UI.Module.SpeakerAudioPreset.Tests
{
    [TestFixture, Apartment(ApartmentState.STA)]
    public class SpeakerAudioPresetModuleTests
    {
        private SpeakerAudioPresetModule? speakerAudioPresetModule;
        private PrivateObject? privateObject;
        private Mock<IModuleOwner>? moduleOwnerMock;
        private Mock<IDeviceManagerSA>? deviceManagerMock;
        private IDeviceManagerSA? deviceManager;
        private Mock<IConsole>? consoleMock;
        private IConsole? console;
        private Mock<ILog>? logMock;
        private ILog? log;
        private Mock<IShowPluginManager>? showPluginManagerMock;
        private IShowPluginManager? showPluginManager;
        private SoundBarViewModel? vm;
        private DeviceInfo? CurrentDeviceInfo;

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
            var moduleOwner = moduleOwnerMock!.Object;
            DdpmCommonHelper.ModuleOwner = moduleOwner;
            deviceManagerMock = new Mock<IDeviceManagerSA>();
            deviceManager = deviceManagerMock.Object;
            DdpmCommonHelper.DeviceManagerSA= deviceManager;
            consoleMock = new Mock<IConsole>();
            console = consoleMock.Object;
            DdpmCommonHelper.MyConsole = console;
            logMock = new Mock<ILog>();
            log = logMock.Object;
            showPluginManagerMock = new Mock<IShowPluginManager>();
            showPluginManager = showPluginManagerMock.Object;
            vm = new SoundBarViewModel(console, log, deviceManager);
            //vm.SpeakerInfoValueDTP.SpeakerProfile = "SpeakerProfile";
            CurrentDeviceInfo = new DeviceInfo() { IsWiredAudioIMicNSEnable = true, IsWiredAudioMicMuteSoundEnable = true, WiredAudioVolumeAdjustmentTone = 1 };
            vm.CurrentDeviceInfo = CurrentDeviceInfo;
            deviceManagerMock.Setup(x => x.GetProfileAsync(It.IsAny<string>())).Returns(Task.FromResult("SpeakerProfile"));
            speakerAudioPresetModule = new SpeakerAudioPresetModule(vm);
            privateObject = new PrivateObject(speakerAudioPresetModule);
        }

        [Test]
        public void TestConstructor_SpeakerAudioPresetModule()
        {
            // Assert
            Assert.That(speakerAudioPresetModule, Is.Not.Null);
            Assert.That(privateObject!.GetFieldOrProperty("_rightView"), Is.Not.Null);
        }

        [Test]
        public void TestModuleName()
        {
            // Act
            var result = speakerAudioPresetModule!.ModuleName;

            // Assert
            Assert.That(result, Is.EqualTo("SpeakerAudioPresetModule"));
        }

        [Test]//Test method
        public void TestSelectedHomeDevice()
        {
            //Arrange:Initialize object and define var info
            var homeDevice = new HomeDevice();

            // Act
            speakerAudioPresetModule!.SelectedHomeDevice = homeDevice;
            var result = speakerAudioPresetModule.SelectedHomeDevice;

            //Assert:Verify
            Assert.That(result, Is.EqualTo(homeDevice));
        }

        [Test]
        public void TestGetLeftView()
        {
            //var mockHomeDevice = new Mock<HomeDevice>();
            //var brightnessModule = new BrightnessModule(/*mockHomeDevice.Object*/);

            //Act:Call the methods to test
            var leftView = speakerAudioPresetModule.GetLeftView();

            Assert.That(leftView, Is.Null);
        }

        [Test]
        public void TestGetRightView()
        {
            //var mockHomeDevice = new Mock<HomeDevice>();
            var rightView = speakerAudioPresetModule.GetRightView();
            Assert.That(rightView, Is.InstanceOf<UserControl>());
        }

        [Test]
        public void TestModuleOwner()
        {
            var mockModuleOwner = new Mock<IModuleOwner>();
            var mockHomeDevice = new Mock<HomeDevice>();

            speakerAudioPresetModule.ModuleOwner = mockModuleOwner.Object;

            Assert.That(speakerAudioPresetModule.ModuleOwner, Is.EqualTo(mockModuleOwner.Object));
        }

        [Test]
        public void TestOnSelectedHomeDeviceChanged()
        {
            speakerAudioPresetModule.IsModuleActive = false;
            speakerAudioPresetModule.OnSelectedHomeDeviceChanged();
            Assert.That(privateObject.GetFieldOrProperty("isSelectChanged"), Is.EqualTo(true));
        }

        [Test]
        public void TestOnSelectedHomeDeviceChangeda()
        {
            speakerAudioPresetModule.IsModuleActive = true;
            speakerAudioPresetModule.OnSelectedHomeDeviceChanged();
            Assert.That(privateObject.GetFieldOrProperty("isSelectChanged"), Is.EqualTo(false));
        }

        [Test]
        public void TestOnActivated()
        {
            try
            {
                speakerAudioPresetModule.OnActivated();
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
                speakerAudioPresetModule.OnDeactivated();
                Assert.True(true);
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }
        }
    }
}