using DDPM.SA.Common;
using DDPM.UI.Common;
using DDPM.UI.Plugin.DockPlugin.Views;
using DDPM.UI.Plugin.ViewModels;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.UX.WPF;
using Moq;
using NGA.UnitTest.PrivateObject;
using System.Windows.Input;

namespace DDPM.UI.Plugin.DockPlugin.Tests
{
    [Apartment(ApartmentState.STA)]
    public class DockPluginTests
    {
        private DockPlugin? dockPlugin;
        private Mock<IShowPluginManager>? showPluginManagerMock;
        private Mock<IPluginManager>? pluginManagerMock;
        private Mock<IConsole>? consoleMock;
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
            _logMock = new Mock<ILog>();
            _log = _logMock.Object;
            consoleMock.Setup(x => x.CreateLog(It.IsAny<string>())).Returns(_logMock.Object);
            deviceManagerSAMock = new Mock<IDeviceManagerSA>();
            DdpmCommonHelper.DeviceManagerSA = deviceManagerSAMock.Object;
            dockPlugin = new DockPlugin(pluginManagerMock.Object, consoleMock.Object, showPluginManagerMock.Object);
            privateObject = new PrivateObject(dockPlugin);
        }

        [Test]
        public void TestConstructor_DockPlugin()
        {
            Assert.That(dockPlugin, Is.Not.Null);
        }

        [Test]
        public void TestHeaderText()
        {
            Assert.That(dockPlugin.HeaderText, Is.EqualTo("Dell Dock"));
        }

        [Test]
        public void TestPageType()
        {
            Assert.That(dockPlugin.PageType, Is.EqualTo(typeof(DockPage)));
        }

        [Test]
        public void TestOnActivated()
        {
            privateObject.SetFieldOrProperty("_deviceManagerPlugin", deviceManagerSAMock.Object);
            try
            {
                dockPlugin.OnActivated();
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
            privateObject.SetFieldOrProperty("_deviceManagerPlugin", deviceManagerSAMock.Object);
            try
            {
                dockPlugin.OnDeactivated();
                Assert.True(true);
                Assert.That(Mouse.OverrideCursor,Is.EqualTo(Cursors.Wait));
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }
        }
    }
}