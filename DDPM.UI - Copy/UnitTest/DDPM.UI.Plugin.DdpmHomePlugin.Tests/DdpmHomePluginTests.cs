using Dell.Client.Framework.Common;
using Dell.Client.Framework.UX.WPF;
using Moq;
using NGA.ThickClient.Interfaces;
using NGA.UnitTest.PrivateObject;

namespace DDPM.UI.Plugin.DdpmHomePlugin.Tests
{
    [TestFixture, Apartment(ApartmentState.STA)]
    public class DdpmHomePluginTests
    {
        private DdpmHomePlugin? ddpmHomePlugin;
        private Mock<IWindowLayout>? windowLayoutMock;
        private IWindowLayout? windowLayout;
        private Mock<IShowPluginManager>? showPluginManagerMock;
        private IShowPluginManager? showPluginManager;
        private Mock<IPluginManager>? pluginManagerMock;
        private IPluginManager? pluginManager;
        private Mock<IConsole>? consoleMock;
        private IConsole? console;
        private Mock<IGearMenu>? gearMenuMock;
        private IGearMenu? gearMenu;
        private Mock<ILog>? _logMock;
        private ILog? _log;
        private PrivateObject? privateObject;

        [SetUp]
        public void Setup()
        {
            windowLayoutMock = new Mock<IWindowLayout>();
            windowLayout = windowLayoutMock.Object;
            showPluginManagerMock = new Mock<IShowPluginManager>();
            showPluginManager = showPluginManagerMock.Object;
            pluginManagerMock = new Mock<IPluginManager>();
            pluginManager = pluginManagerMock.Object;
            consoleMock = new Mock<IConsole>();
            console = consoleMock.Object;
            gearMenuMock = new Mock<IGearMenu>();
            gearMenu = gearMenuMock.Object;
            _logMock = new Mock<ILog>();
            _log = _logMock.Object;
            consoleMock.Setup(x => x.CreateLog(It.IsAny<string>())).Returns(_logMock.Object);
            ddpmHomePlugin = new DdpmHomePlugin(windowLayout, showPluginManager, pluginManager, console, gearMenu);
            privateObject = new PrivateObject(ddpmHomePlugin);
        }

        [Test]
        public void TestConstructor_DdpmHomePlugin()
        {
            Assert.IsNotNull(ddpmHomePlugin);
        }

        [Test]
        public void TestHeaderText()
        {
            Assert.That(ddpmHomePlugin.HeaderText, Is.EqualTo("DDPM Homepage"));
        }

        [Test]
        public void TestPageType()
        {
            Assert.That(ddpmHomePlugin.PageType, Is.EqualTo(typeof(DdpmHomePage)));
        }

        [Test]
        public void TestOnActivated()
        {
            try
            {
                ddpmHomePlugin.OnActivated();
                Assert.True(true);
                Assert.That(privateObject.GetFieldOrProperty("_isActived"), Is.EqualTo(true));
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
                ddpmHomePlugin.OnDeactivated();
                Assert.True(true);
                Assert.That(privateObject.GetFieldOrProperty("_isActived"), Is.EqualTo(false));
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }
        }

        [Test]
        public void TestOnShown()
        {
            try
            {
                ddpmHomePlugin.OnShown();
                Assert.True(true);
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }
        }

        [Test]
        public void TestAddTileToHomePage()
        {
            var tileModel = new TileModel();
            try
            {
                ddpmHomePlugin.AddTileToHomePage(tileModel, 2, null);
                Assert.True(true);
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }
        }

        [Test]
        public void TestFindTileOnHomePage()
        {
            var result = ddpmHomePlugin.FindTileOnHomePage(null, null);
            Assert.That(result, Is.EqualTo(null));
        }

        [Test]
        public void TestFindTileOnHomePageAtPosition()
        {
            var result = ddpmHomePlugin.FindTileOnHomePageAtPosition(1, null);
            Assert.That(result, Is.EqualTo(null));
        }

        [Test]
        public void TestRemoveTileFromHomePage()
        {
            var tileModel = new TileModel();
            try
            {
                ddpmHomePlugin.RemoveTileFromHomePage(tileModel, null);
                Assert.True(true);
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }
        }

        [Test]
        public void TestAddContentAsync()
        {
            var result = ddpmHomePlugin.AddContentAsync(null);
            Assert.That(result, Is.EqualTo(Task.FromResult(false)));
        }

        //[Test]
        //public void TestGetHomeDevices()
        //{
        //    var pluginIocMock = new Mock<IServiceProvider>();
        //    var IDdpmHomePageViewModelMock=new Mock<IDdpmHomePageViewModel>();
        //    var IDdpmHomePageViewModel = IDdpmHomePageViewModelMock.Object;
        //    pluginIocMock.Setup(x => x.GetService(It.IsAny<Type>())).Returns(IDdpmHomePageViewModel);
        //    var result= DdpmHomePlugin.GetHomeDevices();

        //   Assert.That(result, Is.Not.Null);
        //}

        //[Test]
        //public void TestGetSelectedHomeDevice()
        //{
        //    var pluginIocMock = new Mock<IServiceProvider>();
        //    var IDdpmHomePageViewModelMock=new Mock<IDdpmHomePageViewModel>();
        //    var IDdpmHomePageViewModel = IDdpmHomePageViewModelMock.Object;
        //    pluginIocMock.Setup(x => x.GetService(It.IsAny<Type>())).Returns(IDdpmHomePageViewModel);
        //    var result= DdpmHomePlugin.GetSelectedHomeDevice();

        //   Assert.That(result, Is.Not.Null);
        //}

        [Test]
        public void TestDispose()
        {
            var tileModel = new TileModel();
            try
            {
                ddpmHomePlugin.Dispose();
                Assert.True(true);
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }
        }
    }
}