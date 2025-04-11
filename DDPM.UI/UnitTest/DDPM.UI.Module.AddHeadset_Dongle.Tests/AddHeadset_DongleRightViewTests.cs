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

namespace DDPM.UI.Module.AddHeadset_Dongle.Tests
{
    [Apartment(ApartmentState.STA)]
    public class AddHeadset_DongleRightViewTests
    {
        private AddHeadset_DongleRightView? addHeadset_DongleRightView;
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
            vm = new AddDeviceViewModel(console);
            addHeadset_DongleRightView = new AddHeadset_DongleRightView(vm);
            privateObject = new PrivateObject(addHeadset_DongleRightView);
        }

        [Test]
        public void TestConstructor_AddHeadset_DongleRightView()
        {
            // Assert
            Assert.That(addHeadset_DongleRightView, Is.Not.Null);
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
