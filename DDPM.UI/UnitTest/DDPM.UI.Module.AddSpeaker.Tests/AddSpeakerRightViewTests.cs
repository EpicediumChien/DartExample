using DDPM.SA.Common;
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

namespace DDPM.UI.Module.AddSpeaker.Tests
{
    [Apartment(ApartmentState.STA)]
    public class AddSpeakerRightViewTests
    {
        private AddSpeakerRightView? addSpeakerRightView;
        private PrivateObject? privateObject;
        private AddDeviceViewModel? vm;
        private IConsole? console;
        private Mock<IConsole>? consoleMock;
        private IShowPluginManager? showPluginManager;
        private Mock<IShowPluginManager>? showPluginManagerMock;
        private IDeviceManagerSA? peripheralPlugin;
        private Mock<IDeviceManagerSA>? peripheralPluginMock;
        private ILog? log;
        private Mock<ILog>? logMock;

        [SetUp]
        public void Setup()
        {
            consoleMock = new Mock<IConsole>();
            console = consoleMock.Object;
            showPluginManagerMock = new Mock<IShowPluginManager>();
            showPluginManager = showPluginManagerMock.Object;
            peripheralPluginMock = new Mock<IDeviceManagerSA>();
            peripheralPlugin = peripheralPluginMock.Object;
            logMock = new Mock<ILog>();
            log = logMock.Object;
            vm = new AddDeviceViewModel(showPluginManager, console, log);
            addSpeakerRightView = new AddSpeakerRightView(vm);
            privateObject = new PrivateObject(addSpeakerRightView);
        }

        [Test]
        public void TestConstructor_AddSpeakerRightView()
        {
            // Assert
            Assert.That(addSpeakerRightView, Is.Not.Null);
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
