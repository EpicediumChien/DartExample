using DDPM.SA.Common;
using DDPM.UI.Common;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.UX.WPF;
using Moq;
using NGA.UnitTest.PrivateObject;
using System.Windows.Input;

namespace DDPM.UI.Plugin.WalkThroughPlugin.Tests
{
    [Apartment(ApartmentState.STA)]
    public class WalkThroughPluginTests
    {
        private WalkThroughPlugin? walkThroughPlugin;
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
            walkThroughPlugin = new WalkThroughPlugin(showPluginManagerMock.Object, pluginManagerMock.Object, consoleMock.Object, gearMenuMock.Object);
            privateObject = new PrivateObject(walkThroughPlugin);
        }

        [Test]
        public void TestConstructor_WalkThroughPlugin()
        {
            Assert.That(walkThroughPlugin, Is.Not.Null);
        }

        [Test]
        public void TestHeaderText()
        {
            Assert.That(walkThroughPlugin.HeaderText, Is.EqualTo("WalkThrough"));
        }

        [Test]
        public void TestPageType()
        {
            Assert.That(walkThroughPlugin.PageType, Is.EqualTo(typeof(WalkThroughPage)));
        }

        [Test]
        public void TestOnActivated()
        {
            try
            {
                walkThroughPlugin.OnActivated();
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
            try
            {
                walkThroughPlugin.OnDeactivated();
                Assert.True(true);
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }
        }
    }
}