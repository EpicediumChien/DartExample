using DDPM.SA.Common;
using DDPM.SA.Common.Settings;
using DDPM.UI.Common;
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

namespace DDPM.UI.Module.WebCameraCapture.Tests
{
    [Apartment(ApartmentState.STA)]
    public class WebCameraCaptureRightViewTests
    {
        private WebCameraCaptureRightView? webCameraCaptureRightView;
        private PrivateObject? privateObject;
        private WebCameraViewModel? vm;
        private IConsole? console;
        private Mock<IConsole>? consoleMock;
        private IShowPluginManager? showPluginManager;
        private Mock<IShowPluginManager>? showPluginManagerMock;
        private IDeviceManagerSA? deviceManager;
        private Mock<IDeviceManagerSA>? deviceManagerMock;
        private ILog? log;
        private Mock<ILog>? logMock;
        private WebcamSettings? webcamSettings;


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
            showPluginManagerMock = new Mock<IShowPluginManager>();
            showPluginManager = showPluginManagerMock.Object;
            deviceManagerMock = new Mock<IDeviceManagerSA>();
            deviceManager = deviceManagerMock.Object;
            DdpmCommonHelper.DeviceManagerSA = deviceManager;
            logMock = new Mock<ILog>();
            log = logMock.Object;
            vm = new WebCameraViewModel(console, log);

            webcamSettings = new WebcamSettings();
            webcamSettings.SupportedFPSs = new Dictionary<string, List<string>>();
            webcamSettings.SupportedFPSs.Add("a", new List<string> { "a" });
            webcamSettings.SelectedResolution = "a";
            webcamSettings.SelectedFPSs = new Dictionary<string, string>();
            webcamSettings.SelectedFPSs.Add("a", "a");
            webcamSettings.Resolutions = new Dictionary<string, string>();
            webcamSettings.Resolutions.Add("a", "a");
            vm.WebcamSettings = webcamSettings;
            webCameraCaptureRightView = new WebCameraCaptureRightView(vm);
            privateObject = new PrivateObject(webCameraCaptureRightView);
        }

        [Test]
        public void TestConstructor_AddKnM_DongleRightView()
        {
            // Assert
            Assert.That(webCameraCaptureRightView, Is.Not.Null);
            Assert.That(privateObject.GetFieldOrProperty("_vm"), Is.EqualTo(vm));
        }

        [Test]
        public void TestViewModelInitialization()
        {
            // Act
            var viewModel = privateObject!.GetFieldOrProperty("_vm");

            // Assert
            Assert.That(viewModel, Is.Not.Null);
        }
    }
}
