using DDPM.SA.Common;
using DDPM.UI.Common;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.UX.WPF;
using Moq;
using NGA.UnitTest.PrivateObject;
using System.Windows.Input;

namespace DDPM.UI.Plugin.WebCameraPlugin.Tests
{
    [Apartment(ApartmentState.STA)]
    public class WebCamerapluginTests
    {
        private WebCameraplugin? webCameraplugin;
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
            webCameraplugin = new WebCameraplugin(pluginManagerMock.Object, consoleMock.Object, gearMenuMock.Object);
            privateObject = new PrivateObject(webCameraplugin);
        }

        [Test]
        public void TestConstructor_WebCameraplugin()
        {
            Assert.That(webCameraplugin, Is.Not.Null);
        }

        [Test]
        public void TestHeaderText()
        {
            Assert.That(webCameraplugin.HeaderText, Is.EqualTo("Dell WebCamera"));
        }

        [Test]
        public void TestPageType()
        {
            Assert.That(webCameraplugin.PageType, Is.EqualTo(typeof(LaunchView)));
        }

        [Test]
        public void TestOnActivated()
        {
            try
            {
                webCameraplugin.OnActivated();
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
                webCameraplugin.OnDeactivated();
                Assert.True(true);
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }
        }
    }
}