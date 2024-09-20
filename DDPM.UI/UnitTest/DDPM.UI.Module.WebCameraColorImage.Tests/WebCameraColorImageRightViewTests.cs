using DDPM.SA.Common;
using DDPM.UI.Common.Interfaces;
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

namespace DDPM.UI.Module.WebCameraColorImage.Tests
{
    [TestFixture, Apartment(ApartmentState.STA)]
    public class WebCameraColorImageRightViewTests
    {
        private WebCameraColorImageRightView? webCameraColorImageRightView;
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
            consoleMock = new Mock<IConsole>();
            console = consoleMock.Object;
            logMock = new Mock<ILog>();
            log = logMock.Object;
            deviceManagerMock = new Mock<IDeviceManagerSA>();
            deviceManager = deviceManagerMock.Object;
            vm = new WebCameraViewModel(console, log);
            webCameraColorImageRightView = new WebCameraColorImageRightView(vm);
            privateObject = new PrivateObject(webCameraColorImageRightView);
        }

        [Test]
        public void TestConstructor_WebCameraColorImageRightView()
        {
            // Assert
            Assert.That(webCameraColorImageRightView, Is.Not.Null);
            Assert.That(privateObject!.GetFieldOrProperty("_vm"), Is.EqualTo(vm));
        }
    }
}
