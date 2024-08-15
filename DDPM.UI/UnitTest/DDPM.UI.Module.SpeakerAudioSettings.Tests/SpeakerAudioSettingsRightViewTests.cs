using DDPM.SA.Common;
using DDPM.UI.Common;
using DDPM.UI.Common.Interfaces;
using DDPM.UI.Plugin.ViewModels;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.UX.WPF;
using Moq;
using NGA.UnitTest.PrivateObject;

namespace DDPM.UI.Module.SpeakerAudioSettings.Tests
{
    [TestFixture, Apartment(ApartmentState.STA)]
    public class SpeakerAudioSettingsRightViewTests
    {
        private SpeakerAudioSettingsRightView? speakerAudioSettingsRightView;
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
        }

        [Test]
        public void TestConstructor_SpeakerAudioSettingsRightView()
        {
            //CurrentDeviceInfo = new DeviceInfo() { IsWiredAudioIMicNSEnable = true, IsWiredAudioMicMuteSoundEnable = true, WiredAudioVolumeAdjustmentTone = 1 };
            //vm.CurrentDeviceInfo = CurrentDeviceInfo;
            speakerAudioSettingsRightView = new SpeakerAudioSettingsRightView(vm);
            privateObject = new PrivateObject(speakerAudioSettingsRightView);
            // Assert
            Assert.That(speakerAudioSettingsRightView, Is.Not.Null);
            Assert.That(privateObject!.GetFieldOrProperty("_vm"), Is.EqualTo(vm));
        }
    }
}