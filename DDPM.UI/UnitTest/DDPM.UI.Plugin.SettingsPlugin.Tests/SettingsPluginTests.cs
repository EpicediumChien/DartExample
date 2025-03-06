using Dell.Client.Framework.Common;
using Dell.Client.Framework.UX.WPF;
using Moq;

namespace DDPM.UI.Plugin.SettingsPlugin.Tests
{
    [Apartment(ApartmentState.STA)]
    public class SettingsPluginTests
    {
        private SettingsPlugin? settingsPlugin;
        private Mock<IConsole>? consoleMock;
        private IConsole console;
        private Mock<IGearMenu>? gearMenuMock;
        private IGearMenu? gearMenu;
        private Mock<ILog>? _logMock;
        private ILog? _log;
        private IShowPluginManager? showPluginManager;
        private Mock<IShowPluginManager>? showPluginManagerMock;

        [SetUp]
        public void Setup()
        {
            //Robert_Lin 2025-3-5 change for SettingsPlugin ctor arguments changed
            showPluginManagerMock = new Mock<IShowPluginManager>();
            showPluginManager = showPluginManagerMock.Object;
            consoleMock = new Mock<IConsole>();
            console = consoleMock.Object;
            gearMenuMock = new Mock<IGearMenu>();
            gearMenu = gearMenuMock.Object;
            _logMock = new Mock<ILog>();
            _log = _logMock.Object;
            consoleMock.Setup(x => x.CreateLog(It.IsAny<string>())).Returns(_logMock.Object);

            settingsPlugin = new SettingsPlugin(showPluginManager, console, gearMenu);
        }

        [Test]
        public void TestHeaderText()
        {
            Assert.That(settingsPlugin.HeaderText, Is.EqualTo("Settings Page"));
        }

        [Test]
        public void TestPageType()
        {
            var res = typeof(SettingsPage);
            Assert.That(settingsPlugin.PageType, Is.EqualTo(res));
        }

        [Test]
        public void TestConstructor_SettingsPlugin()
        {
            Assert.That(settingsPlugin, Is.Not.Null);
        }

        [Test]
        public void TestOnActivated()
        {
            try
            {
                settingsPlugin.OnActivated();
                Assert.True(true);
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
                settingsPlugin.OnDeactivated();
                Assert.True(true);
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }
        }
    }
}