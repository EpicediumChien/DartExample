using DDPM.UI.Plugin.DisplayPlugin.Views;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.UX.WPF;
using Moq;
using NGA.UnitTest.PrivateObject;

namespace DDPM.UI.Plugin.DisplayPlugin.Tests
{
    [Apartment(ApartmentState.STA)]
    public class DisplayPageTests
    {
        private DisplayPage? displayPage;
        private PrivateObject? privateObject;
        private DisplayPlugin? displayPlugin;
        private Mock<IWindowLayout>? windowLayoutMock;
        private IWindowLayout? windowLayout;
        private Mock<IConsole>? consoleMock;
        private IConsole? console;
        private Mock<IDispatcherWrapper>? dispatcherWrapperMock;
        private IDispatcherWrapper? dispatcherWrapper;
        private Mock<ILog>? _logMock;
        private ILog? _log;
        private Mock<IPluginManager>? pluginManagerMock;
        private IPluginManager? pluginManager;

        [SetUp]
        public void Setup()
        {
            windowLayoutMock = new Mock<IWindowLayout>();
            windowLayout = windowLayoutMock.Object;
            consoleMock = new Mock<IConsole>();
            console = consoleMock.Object;
            dispatcherWrapperMock = new Mock<IDispatcherWrapper>();
            dispatcherWrapper = dispatcherWrapperMock.Object;
            pluginManagerMock = new Mock<IPluginManager>();
            pluginManager = pluginManagerMock.Object;
            _logMock = new Mock<ILog>();
            _log = _logMock.Object;
            consoleMock.Setup(x => x.CreateLog(It.IsAny<string>())).Returns(_logMock.Object);
            displayPlugin = new DisplayPlugin(windowLayout, console, dispatcherWrapper, pluginManager);

            //displayPage = new DisplayPage();
            //privateObject = new PrivateObject(displayPage);
        }

        //[Test]
        //public void TestConstructor_InitializesComponent()
        //{
        //    Assert.That(displayPage, Is.Not.Null);
        //    //Assert.That(_console, Is.EqualTo(console));
        //   // Assert.That(_pluginManager, Is.EqualTo(pluginManager));

        //}
    }
}