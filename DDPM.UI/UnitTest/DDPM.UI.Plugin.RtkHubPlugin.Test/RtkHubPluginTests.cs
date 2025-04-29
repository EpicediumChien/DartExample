using DDPM.SA.Common;
using DDPM.UI.Common;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.UX.WPF;
using Moq;
using NGA.UnitTest.PrivateObject;
using System.Windows.Input;

namespace DDPM.UI.Plugin.RtkHubPlugin.Test
{
    [Apartment(ApartmentState.STA)]
    public class HeadsetPluginTests
    {
        private RtkHubPlugin? rtkhubPlugin;
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
            rtkhubPlugin = new RtkHubPlugin(pluginManagerMock.Object, consoleMock.Object, showPluginManagerMock.Object);
            privateObject = new PrivateObject(rtkhubPlugin);
        }

        [Test]
        public void TestConstructor_HeadsetPlugin()
        {
            Assert.That(rtkhubPlugin, Is.Not.Null);
        }

        [Test]
        public void TestHeaderText()
        {
            Assert.That(rtkhubPlugin.HeaderText, Is.EqualTo("Dell RtkHub"));
        }

        [Test]
        public void TestPageType()
        {
            Assert.That(rtkhubPlugin.PageType, Is.EqualTo(typeof(LaunchView)));
        }

        [Test]
        public void TestOnActivated()
        {
            try
            {
                rtkhubPlugin.OnActivated();
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
            privateObject.SetFieldOrProperty("IsEventRegistered", true);
            try
            {
                rtkhubPlugin.OnDeactivated();
                Assert.True(true);
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }
        }
    }
}