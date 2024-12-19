using DDPM.SA.Common;
using DDPM.UI.Common;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.UX.WPF;
using Moq;
using NGA.UnitTest.PrivateObject;
using System.Windows.Input;

namespace DDPM.UI.Plugin.KeyboardPlugin.Tests
{
    [Apartment(ApartmentState.STA)]
    public class KeyboardpluginTests
    {
        private Keyboardplugin? keyboardplugin;
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
            keyboardplugin = new Keyboardplugin(pluginManagerMock.Object, consoleMock.Object);
            privateObject = new PrivateObject(keyboardplugin);
        }

        [Test]
        public void TestConstructor_Keyboardplugin()
        {
            Assert.That(keyboardplugin, Is.Not.Null);
        }

        [Test]
        public void TestHeaderText()
        {
            Assert.That(keyboardplugin.HeaderText, Is.EqualTo("Dell Keyboard"));
        }

        [Test]
        public void TestPageType()
        {
            Assert.That(keyboardplugin.PageType, Is.EqualTo(typeof(LaunchView)));
        }

        [Test]
        public void TestOnActivated()
        {
            try
            {
                keyboardplugin.OnActivated();
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
                keyboardplugin.OnDeactivated();
                Assert.True(true);
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }
        }
    }
}