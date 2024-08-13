using DDPM.UI.Common;
using DDPM.UI.Module.Color;
using DDPM.UI.Plugin.DisplayPlugin.ViewModels;
using DDPM.UI.Plugin.ViewModels;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.UX.WPF;
using Microsoft.VisualBasic.Logging;
using Moq;
using NGA.UnitTest.PrivateObject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDPM.UI.Plugin.DisplayPlugin.Tests
{
    [Apartment(ApartmentState.STA)]
    public class DisplayPluginTests
    {
        private DisplayPlugin? displayPlugin;
        private Mock<IWindowLayout>? windowLayoutMock;
        private IWindowLayout? windowLayout;
        private Mock<IConsole>? consoleMock;
        private IConsole? console;
        private Mock<IDispatcherWrapper>? dispatcherWrapperMock;
        private IDispatcherWrapper? dispatcherWrapper;
        private PrivateObject? privateObject;
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
            pluginManagerMock=new Mock<IPluginManager>();
            pluginManager = pluginManagerMock.Object;
            _logMock = new Mock<ILog>();
            _log = _logMock.Object;
            consoleMock.Setup(x => x.CreateLog(It.IsAny<string>())).Returns(_logMock.Object);
            displayPlugin = new DisplayPlugin(windowLayout, console, dispatcherWrapper, pluginManager);
            privateObject = new PrivateObject(displayPlugin);
        }

        [Test]
        public void TestConstructor_InitializesComponent()
        {
            var _console = privateObject.GetFieldOrProperty("_console");
            var _pluginManager= privateObject.GetFieldOrProperty("_pluginManager");
            Assert.That(displayPlugin, Is.Not.Null);
            Assert.That(_console, Is.EqualTo(console));
            Assert.That(_pluginManager, Is.EqualTo(pluginManager));
        }

        [Test]
        public void TestOnActivated()
        {
            try
            {
                displayPlugin.OnActivated();
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
                displayPlugin.OnDeactivated();
                Assert.True(true);
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }

        }

        //[Test]
        //public void TestOnShown()
        //{
        //    try
        //    {
        //        displayPlugin.OnShown();
        //        Assert.True(true);
        //    }
        //    catch (Exception ex)
        //    {
        //        Assert.Fail("not invoked");
        //    }

        //}


    }
}
