using DDPM.SA.Common;
using DDPM.UI.Common;
using DDPM.UI.Common.Interfaces;
using DDPM.UI.Plugin.ViewModels;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.UX.WPF;
using Moq;
using NGA.UnitTest.PrivateObject;
using System.Windows;
using Dell.Client.Framework.UX.WPF.ResourceManager;


namespace DDPM.UI.Module.SpeakerInteractions.Tests
{
    [TestFixture, Apartment(ApartmentState.STA)]
    public class SpeakerInteractionsRightViewTests
    {
        private SpeakerInteractionsRightView? speakerInteractionsRightView;
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
            DdpmCommonHelper.DeviceManagerSA = deviceManager;
            deviceManagerMock.Setup(x => x.GetAllAppList()).Returns(Task.FromResult(new Dictionary<string, InstalledAppInfo> { }));
            consoleMock = new Mock<IConsole>();
            console = consoleMock.Object;
            DdpmCommonHelper.MyConsole = console;
            logMock = new Mock<ILog>();
            log = logMock.Object;
            showPluginManagerMock = new Mock<IShowPluginManager>();
            showPluginManager = showPluginManagerMock.Object;
            vm = new SoundBarViewModel(console, log, deviceManager);
        }

        [Test]
        public void TestConstructor_SpeakerAudioSettingsRightView()
        {
            //CurrentDeviceInfo = new DeviceInfo() { IsWiredAudioIMicNSEnable = true, IsWiredAudioMicMuteSoundEnable = true, WiredAudioVolumeAdjustmentTone = 1 };
            //vm.CurrentDeviceInfo = CurrentDeviceInfo;
            speakerInteractionsRightView = new SpeakerInteractionsRightView(vm);
            privateObject = new PrivateObject(speakerInteractionsRightView);
            // Assert
            Assert.That(speakerInteractionsRightView, Is.Not.Null);
            Assert.That(privateObject!.GetFieldOrProperty("_vm"), Is.EqualTo(vm));
        }
    }
}