using DDPM.SA.Common;
using DDPM.UI.Common;
using DDPM.UI.Common.Interfaces;
using DDPM.UI.Common.Models;
using DDPM.UI.Plugin.ViewModels;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.UX.WPF;
using Moq;
using NGA.UnitTest.PrivateObject;
using System.Windows.Controls;

namespace DDPM.UI.Module.SpeakerInteractions.Tests
{
    [TestFixture, Apartment(ApartmentState.STA)]
    public class SpeakerInteractionsModuleTests
    {
        private SpeakerInteractionsModule? speakerInteractionsModule;
        private PrivateObject? privateObject;
        private Mock<IModuleOwner>? moduleOwnerMock;
        private Mock<IDeviceManagerSA>? deviceManagerMock;
        private IDeviceManagerSA? deviceManager;
        private Mock<IConsole>? consoleMock;
        private IConsole? console;
        private Mock<ILog>? logMock;
        private ILog? log;
        private SoundBarViewModel? vm;
        private DeviceInfo? CurrentDeviceInfo;

        [SetUp]
        public void Setup()
        {
            moduleOwnerMock = new Mock<IModuleOwner>();
            var moduleOwner = moduleOwnerMock!.Object;
            DdpmCommonHelper.ModuleOwner = moduleOwner;
            deviceManagerMock = new Mock<IDeviceManagerSA>();
            deviceManager = deviceManagerMock.Object;
            consoleMock = new Mock<IConsole>();
            console = consoleMock.Object;
            DdpmCommonHelper.MyConsole = console;
            logMock = new Mock<ILog>();
            log = logMock.Object;
            vm = new SoundBarViewModel(console, log, deviceManager);
            CurrentDeviceInfo = new DeviceInfo() { IsWiredAudioIMicNSEnable = true, IsWiredAudioMicMuteSoundEnable = true, WiredAudioVolumeAdjustmentTone = 1 };
            vm.CurrentDeviceInfo = CurrentDeviceInfo;
            speakerInteractionsModule = new SpeakerInteractionsModule(vm);
            privateObject = new PrivateObject(speakerInteractionsModule);
        }

        [Test]
        public void TestConstructor_SpeakerInteractionsModule()
        {
            // Assert
            Assert.That(speakerInteractionsModule, Is.Not.Null);
            Assert.That(privateObject!.GetFieldOrProperty("_rightView"), Is.Not.Null);
        }

        [Test]
        public void TestModuleName()
        {
            // Act
            var result = speakerInteractionsModule!.ModuleName;

            // Assert
            Assert.That(result, Is.EqualTo("SpeakerInteractionsModule"));
        }

        [Test]//Test method
        public void TestSelectedHomeDevice()
        {
            //Arrange:Initialize object and define var info
            var homeDevice = new HomeDevice();

            // Act
            speakerInteractionsModule!.SelectedHomeDevice = homeDevice;
            var result = speakerInteractionsModule.SelectedHomeDevice;

            //Assert:Verify
            Assert.That(result, Is.EqualTo(homeDevice));
        }

        [Test]
        public void TestGetLeftView()
        {
            //var mockHomeDevice = new Mock<HomeDevice>();
            //var brightnessModule = new BrightnessModule(/*mockHomeDevice.Object*/);

            //Act:Call the methods to test
            var leftView = speakerInteractionsModule.GetLeftView();

            Assert.That(leftView, Is.Null);
        }

        [Test]
        public void TestGetRightView()
        {
            //var mockHomeDevice = new Mock<HomeDevice>();
            var rightView = speakerInteractionsModule.GetRightView();
            Assert.That(rightView, Is.InstanceOf<UserControl>());
        }

        [Test]
        public void TestModuleOwner()
        {
            var mockModuleOwner = new Mock<IModuleOwner>();
            var mockHomeDevice = new Mock<HomeDevice>();

            speakerInteractionsModule.ModuleOwner = mockModuleOwner.Object;

            Assert.That(speakerInteractionsModule.ModuleOwner, Is.EqualTo(mockModuleOwner.Object));
        }

        [Test]
        public void TestOnSelectedHomeDeviceChanged()
        {
            try
            {
                speakerInteractionsModule.OnSelectedHomeDeviceChanged();
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
                speakerInteractionsModule.OnActivated();
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
                speakerInteractionsModule.OnDeactivated();
                Assert.True(true);
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }
        }
    }
}