using DDPM.SA.Common;
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
using System.Windows;

namespace DDPM.UI.Module.HeadsetAutomatedActions.Tests
{
    [Apartment(ApartmentState.STA)]
    public class HeadsetAutomatedActionsRightViewTests
    {
        private HeadsetAutomatedActionsRightView? HeadsetAutomatedActionsRightView;
        private HeadsetViewModel? vm;
        private Mock<ILog>? logMock;
        private ILog? log;
        private Mock<IShowPluginManager>? showPluginManagerMock;
        private IShowPluginManager? showPluginManager;
        private Mock<IDeviceManagerSA>? deviceManagerSAMock;
        private IDeviceManagerSA? deviceManagerSA;
        private Mock<IConsole>? consoleMock;
        private IConsole? console;
        private PrivateObject? privateObject;

        [SetUp]
        public void Setup()
        {
            if (System.Windows.Application.Current == null)
            {
                new System.Windows.Application();
            }
            var resourceDictionary = new ResourceDictionary();
            resourceDictionary.Source = new Uri("pack://application:,,,/DDPM.UI.Common;component/ModuleStyle.xaml");
            System.Windows.Application.Current.Resources.MergedDictionaries.Add(resourceDictionary);
            logMock = new Mock<ILog>();
            log = logMock.Object;
            showPluginManagerMock = new Mock<IShowPluginManager>();
            showPluginManager = showPluginManagerMock.Object;
            deviceManagerSAMock = new Mock<IDeviceManagerSA>();
            deviceManagerSA = deviceManagerSAMock.Object;
            consoleMock = new Mock<IConsole>();
            console = consoleMock.Object;
            vm = new HeadsetViewModel(showPluginManager, console, log, deviceManagerSA);
            var currentDeviceInfo = new DeviceInfo();
            vm.CurrentDeviceInfo = currentDeviceInfo;
            HeadsetAutomatedActionsRightView = new HeadsetAutomatedActionsRightView(vm);
            privateObject = new PrivateObject(HeadsetAutomatedActionsRightView);
        }

        [Test]
        public void TestConstructor_HeadsetAutomatedActionsRightView()
        {
            // Act
            var _vm = privateObject.GetFieldOrProperty("_vm");

            // Assert
            Assert.That(HeadsetAutomatedActionsRightView, Is.Not.Null);
            Assert.That(_vm, Is.EqualTo(vm));
        }
    }
}
