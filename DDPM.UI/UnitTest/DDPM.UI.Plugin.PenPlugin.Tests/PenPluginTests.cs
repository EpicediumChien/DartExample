using DDPM.SA.Common;
using DDPM.UI.Common;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.UX.WPF;
using Moq;
using NGA.UnitTest.PrivateObject;
using System.Windows.Input;

namespace DDPM.UI.Plugin.PenPlugin.Tests
{
    [Apartment(ApartmentState.STA)]
    public class PenpluginTests
    {
        private Penplugin? penplugin;
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
            penplugin = new Penplugin(pluginManagerMock.Object, consoleMock.Object, gearMenuMock.Object);
            privateObject = new PrivateObject(penplugin);
        }

        [Test]
        public void TestConstructor_Penplugin()
        {
            Assert.That(penplugin, Is.Not.Null);
        }

        [Test]
        public void TestHeaderText()
        {
            Assert.That(penplugin.HeaderText, Is.EqualTo("Dell Pen"));
        }

        [Test]
        public void TestPageType()
        {
            Assert.That(penplugin.PageType, Is.EqualTo(typeof(LaunchView)));
        }

        [Test]
        public void TestOnActivated()
        {
            try
            {
                penplugin.OnActivated();
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
                penplugin.OnDeactivated();
                Assert.True(true);
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }
        }
    }
}