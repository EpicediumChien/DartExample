using DDPM.SA.Common;
using DDPM.UI.Module.WebCameraSettings;
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
using Dell.Client.Framework.UX.WPF.ResourceManager;
using System.Windows;

namespace DDPM.UI.Module.WebCameraSettings.Tests
{
    [TestFixture, Apartment(ApartmentState.STA)]
    public class WebCameraSettingsRightViewTests
    {
        private WebCameraSettingsRightView? webCameraSettingsRightView;
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
            vm = new WebCameraViewModel(console, log) { CurrentDeviceInfo = new DeviceInfo() };
            webCameraSettingsRightView = new WebCameraSettingsRightView(vm);
            privateObject = new PrivateObject(webCameraSettingsRightView);
        }

        [Test]
        public void TestConstructor_WebCameraSettingsRightView()
        {
            // Assert
            Assert.That(webCameraSettingsRightView, Is.Not.Null);
            Assert.That(privateObject!.GetFieldOrProperty("_vm"), Is.EqualTo(vm));
        }
    }
}
