using DDPM.SA.Common;
using DDPM.UI.Common.Interfaces;
using DDPM.UI.Plugin.ViewModels;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.UX.WPF;
using Moq;
using NGA.UnitTest.PrivateObject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Dell.Client.Framework.UX.WPF.ResourceManager;

namespace DDPM.UI.Module.HeadsetDeviceSettings.Tests
{
    [Apartment(ApartmentState.STA)]
    public class HeadsetDeviceSettingsRightViewTests
    {
        private HeadsetDeviceSettingsRightView? headsetDeviceSettingsRightView;
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
            deviceManagerSAMock = new Mock<IDeviceManagerSA>();
            deviceManagerSA = deviceManagerSAMock.Object;
            consoleMock = new Mock<IConsole>();
            console = consoleMock.Object;
            showPluginManagerMock = new Mock<IShowPluginManager>();
            showPluginManager = showPluginManagerMock.Object;
            vm = new HeadsetViewModel(console, log, deviceManagerSA);
            var currentDeviceInfo = new DeviceInfo();
            vm.CurrentDeviceInfo = currentDeviceInfo;
            headsetDeviceSettingsRightView = new HeadsetDeviceSettingsRightView(vm);
            privateObject = new PrivateObject(headsetDeviceSettingsRightView);
        }

        [Test]
        public void TestConstructor_HeadsetAudioSettingsModule()
        {
            // Act
            var _vm = privateObject.GetFieldOrProperty("_vm");

            // Assert
            Assert.That(headsetDeviceSettingsRightView, Is.Not.Null);
            Assert.That(_vm, Is.EqualTo(vm));
        }

    }
}
