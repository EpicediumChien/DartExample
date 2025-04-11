using DDPM.SA.Common;
using DDPM.UI.Common;
using DDPM.UI.Plugin.ViewModels;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.UX.WPF;
using Microsoft.VisualBasic.Logging;
using Moq;
using NGA.UnitTest.PrivateObject;
using System;
using System.Windows.Input;

namespace DDPM.UI.Plugin.AddDevicePlugin.Tests
{
    [Apartment(ApartmentState.STA)]
    public class AddDevicePluginTests
    {
        private AddDevicePlugin? addDevicePlugin;
        private Mock<IShowPluginManager>? showPluginManagerMock;
        private Mock<IPluginManager>? pluginManagerMock;
        private Mock<IConsole>? consoleMock;
        private Mock<IGearMenu>? gearMenuMock;
        private PrivateObject? privateObject;
        private Mock<ILog>? _logMock;
        private ILog? _log;
        private Mock<IDeviceManagerSA>? deviceManagerSAMock;

        [SetUp]
        public void Setup()
        {
            showPluginManagerMock = new Mock<IShowPluginManager>();
            pluginManagerMock = new Mock<IPluginManager>();
            consoleMock = new Mock<IConsole>();
            gearMenuMock = new Mock<IGearMenu>();
            _logMock = new Mock<ILog>();
            _log = _logMock.Object;
            consoleMock.Setup(x => x.CreateLog(It.IsAny<string>())).Returns(_logMock.Object);
            deviceManagerSAMock = new Mock<IDeviceManagerSA>();
            DdpmCommonHelper.DeviceManagerSA = deviceManagerSAMock.Object;
            addDevicePlugin = new AddDevicePlugin(showPluginManagerMock.Object, pluginManagerMock.Object, consoleMock.Object, gearMenuMock.Object);
            privateObject = new PrivateObject(addDevicePlugin);
        }

        [Test]
        public void TestConstructor_AddDevicePlugin()
        {
            Assert.That(addDevicePlugin, Is.Not.Null);
        }

        [Test]
        public void TestHeaderText()
        {
            Assert.That(addDevicePlugin.HeaderText, Is.EqualTo("Add Device"));
        }

        [Test]
        public void TestPageType()
        {
            Assert.That(addDevicePlugin.PageType, Is.EqualTo(typeof(AddDeviceView)));
        }

        [Test]
        public void TestOnActivated()
        {
            try
            {
                addDevicePlugin.OnActivated();
                Assert.True(true);
                Assert.Null(Mouse.OverrideCursor);

            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }
        }

        [Test]
        public void TestOnDeactivated()
        {
            var viewModel = new AddDeviceViewModel(consoleMock.Object);
            privateObject.SetFieldOrProperty("_viewModel", viewModel);
            try
            {
                addDevicePlugin.OnDeactivated();
                Assert.True(true);
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }
        }
    }
}