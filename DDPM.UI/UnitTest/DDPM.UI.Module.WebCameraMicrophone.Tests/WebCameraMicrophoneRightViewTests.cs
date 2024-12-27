using DDPM.SA.Common;
using DDPM.SA.Common.Settings;
using DDPM.UI.Common;
using DDPM.UI.Plugin.ViewModels;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.UX.WPF;
using Dell.Client.Framework.UX.WPF.ResourceManager;
using Microsoft.VisualBasic.Logging;
using Moq;
using NGA.UnitTest.PrivateObject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DDPM.UI.Module.WebCameraMicrophone.Tests
{
    [TestFixture, Apartment(ApartmentState.STA)]
    public class WebCameraMicrophoneRightViewTests
    {
        private WebCameraMicrophoneRightView? webCameraMicrophoneRightView;
        private WebCameraViewModel? vm;
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
            vm = new WebCameraViewModel(console, log) { CurrentDeviceInfo = new DeviceInfo() { IsMicEnumerationSupported =true} };
            
        }

        [Test]
        public void TestConstructor_WebCameraMicrophoneRightViewa()
        {
            webCameraMicrophoneRightView = new WebCameraMicrophoneRightView(vm);
            privateObject = new PrivateObject(webCameraMicrophoneRightView);
            // Assert
            Assert.That(webCameraMicrophoneRightView, Is.Not.Null);
            Assert.That(privateObject!.GetFieldOrProperty("_vm"), Is.EqualTo(vm));
        }

        [Test]
        public void TestConstructor_WebCameraMicrophoneRightViewb()
        {
            deviceManagerMock.Setup(x => x.ReloadAppConfigData(It.IsAny<bool>())).Returns(Task.FromResult(new DDPMSettings(new DDPMAppSettings (), new DDPMUserSettings(),new DDPMITConfig())));
            webCameraMicrophoneRightView = new WebCameraMicrophoneRightView(vm);
            privateObject = new PrivateObject(webCameraMicrophoneRightView);
            // Assert
            Assert.That(webCameraMicrophoneRightView, Is.Not.Null);
            Assert.That(privateObject!.GetFieldOrProperty("_vm"), Is.EqualTo(vm));
            Assert.That(vm.TabNavigation, Is.EqualTo("Cycle"));
            Assert.That(vm.LockMaskVisible, Is.EqualTo(Visibility.Collapsed));
        }

        [Test]
        public void TestConstructor_WebCameraMicrophoneRightViewc()
        {
            deviceManagerMock.Setup(x => x.ReloadAppConfigData(It.IsAny<bool>())).Returns(Task.FromResult(new DDPMSettings(new DDPMAppSettings(), new DDPMUserSettings(), new DDPMITConfig()) { LockSettings=new DDPMITConfig() { Lock_Webcam_MicSwitch =true} }));
            webCameraMicrophoneRightView = new WebCameraMicrophoneRightView(vm);
            privateObject = new PrivateObject(webCameraMicrophoneRightView);
            // Assert
            Assert.That(webCameraMicrophoneRightView, Is.Not.Null);
            Assert.That(privateObject!.GetFieldOrProperty("_vm"), Is.EqualTo(vm));
            Assert.That(vm.TabNavigation, Is.EqualTo("None"));
            Assert.That(vm.LockMaskVisible, Is.EqualTo(Visibility.Visible));
        }
    }


}
