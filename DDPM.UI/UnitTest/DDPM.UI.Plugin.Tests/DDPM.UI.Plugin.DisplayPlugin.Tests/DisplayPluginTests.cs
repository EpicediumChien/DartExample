using DDPM.UI.Common;
using DDPM.UI.Plugin.DisplayPlugin.ViewModels;
using DDPM.UI.Plugin.ViewModels;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.UX.WPF;
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

        //[SetUp]
        //public void Setup()
        //{
        //    windowLayoutMock=new Mock<IWindowLayout>();
        //    windowLayout=windowLayoutMock.Object;
        //    consoleMock=new Mock<IConsole>();
        //    console=consoleMock.Object;
        //    dispatcherWrapperMock=new Mock<IDispatcherWrapper>();
        //    dispatcherWrapper=dispatcherWrapperMock.Object;
        //    displayPlugin = new DisplayPlugin(windowLayout, console, dispatcherWrapper);
        //    privateObject= new PrivateObject(displayPlugin);
        //}

        //[Test]
        //public void TestConstructor_InitializesComponent()
        //{
        //    //var console = privateObject.GetFieldOrProperty("_console");
        //    //var logMock=new Mock<ILog>();
        //    //var log=logMock.Object;
        //    //privateObject.SetFieldOrProperty("_log", log);
        //    //Assert.That(displayPlugin, Is.Not.Null);
        //    //Assert.That(console, Is.EqualTo(console));

        //}

    }
}
