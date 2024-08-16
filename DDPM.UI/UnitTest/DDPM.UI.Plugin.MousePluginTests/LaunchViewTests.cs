using CommunityToolkit.Mvvm.DependencyInjection;
using DDPM.SA.Common;
using DDPM.UI.Interfaces;
using DDPM.UI.Module.ButtonSettings;
using DDPM.UI.Plugin.ViewModels;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.UX.WPF;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NGA.UnitTest.PrivateObject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDPM.UI.Plugin.MousePlugin.Tests
{
    [TestFixture, Apartment(ApartmentState.STA)]
    public class LaunchViewTests
    {
        private IPluginManager? pluginManager;
        private Mock<IPluginManager>? pluginManagerMock;
        private IConsole? console;
        private Mock<IConsole>? consoleMock;
        private Mouseplugin? mouseplugin;
        private ILog? _log;
        private Mock<ILog>? _logMock;
        private Mock<IDeviceManagerSA>? IDeviceManagerSAMock;
        private IDeviceManagerSA? iDeviceManagerSA;
        private LaunchView? launchView;
        private PrivateObject? privateObject;

        //[SetUp]
        //public void SetUp()
        //{
        //    pluginManagerMock = new Mock<IPluginManager>();
        //    pluginManager = pluginManagerMock.Object;
        //    consoleMock = new Mock<IConsole>();
        //    console = consoleMock.Object;
        //    _logMock = new Mock<ILog>();
        //    _log = _logMock.Object;
        //    IDeviceManagerSAMock = new Mock<IDeviceManagerSA>();
        //    iDeviceManagerSA = IDeviceManagerSAMock.Object;
        //    consoleMock.Setup(x => x.CreateLog(It.IsAny<string>())).Returns(_logMock.Object);
        //    mouseplugin = new Mouseplugin(pluginManager, console);
        //    privateObject = new PrivateObject(mouseplugin);

        //    var pluginIocMock = new Mock<IServiceProvider>();
        //    var IPeripheralViewModelMock = new Mock<IPeripheralViewModel>();
        //    var iPeripheralViewModel = IPeripheralViewModelMock.Object;

        //    pluginIocMock.Setup(x => x.GetService(It.IsAny<Type>())).Returns(iPeripheralViewModel);

        //    var ioc = new Ioc();
        //    privateObject = new PrivateObject(mouseplugin);
        //    privateObject.SetFieldOrProperty("PluginIoc", ioc);

        //    launchView = new LaunchView();
        //    privateObject = new PrivateObject(launchView);
        //}

        //[Test]
        //public void TestConstructor_LaunchView()
        //{
        //    var vm = privateObject.GetFieldOrProperty("_vm");

        //    Assert.That(vm, Is.Not.Null);
        //    Assert.That(launchView, Is.Not.Null);

        //}
    }
}