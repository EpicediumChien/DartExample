using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DDPM.SA.Common.Interfaces;
using DDPM.SA.Common;
using DDPM.UI.Common.Interfaces;
using DDPM.UI.Common.Models;
using DDPM.UI.Common;
using DDPM.UI.Plugin.Common.ViewModels;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.UX.WPF;
using System.Windows;
using Dell.Client.Framework.UX.WPF.ResourceManager;
using Moq;
using NGA.UnitTest.PrivateObject;
using VcpCore.Common;
using DDPM.UI.Common.ViewModels;

namespace DDPM.UI.Module.EzMemory.Tests
{
    [TestFixture, Apartment(ApartmentState.STA)]
    public class EzMemoryAddApplicationTests
    {
        private EzMemoryAddApplication? ezMemoryAddApplication;
        private PrivateObject? privateObject;
        private Mock<IModuleOwner>? moduleOwnerMock;
        private IModuleOwner? moduleOwner;
        private DisplayViewModel? vmDisplay;
        private IConsole? console;
        private Mock<IConsole>? consoleMock;
        private IEasyArrangeService? easyArrange;
        private Mock<IEasyArrangeService>? easyArrangeMock;
        private IDeviceManagerSA? deviceManagerSA;
        private Mock<IDeviceManagerSA>? deviceManagerSAMock;
        private ILog? log;
        private Mock<ILog>? logMock;
        private EzArrangeViewModel? vm;


        [SetUp]
        public void Setup()
        {
            if (System.Windows.Application.Current == null)
            {
                new System.Windows.Application();
            }
            ResourceManager res = new ResourceManager();
            var resourceDictionary = new ResourceDictionary();
            resourceDictionary.Source = new Uri("pack://application:,,,/DDPM.UI.Common;component/ModuleStyle.xaml");
            System.Windows.Application.Current.Resources.MergedDictionaries.Add(resourceDictionary);
            moduleOwnerMock = new Mock<IModuleOwner>();
            moduleOwner = moduleOwnerMock!.Object;
            moduleOwnerMock.Setup(x => x.SelectedHomeDevice).Returns(new HomeDevice());
            DdpmCommonHelper.ModuleOwner = moduleOwner;
            consoleMock = new Mock<IConsole>();
            console = consoleMock.Object;
            DdpmCommonHelper.MyConsole = console;
            easyArrangeMock = new Mock<IEasyArrangeService>();
            easyArrange = easyArrangeMock.Object;
            deviceManagerSAMock = new Mock<IDeviceManagerSA>();
            //Robert_Lin, 2025-1-7, IsEAFunctionEnabled is deleted
            //deviceManagerSAMock.Setup(x => x.GetEAFunctionEnabled()).Returns(Task.FromResult(new ObjGetVCP() { result = true, value = true }));
            DdpmCommonHelper.DeviceManagerSA = deviceManagerSA;
            deviceManagerSA = deviceManagerSAMock.Object;
            HomeDevice.DeviceManagerSA = deviceManagerSAMock.Object;
            vm = new EzArrangeViewModel(new HomeDevice());
            logMock = new Mock<ILog>();
            log = logMock.Object;
            consoleMock.Setup(x => x.CreateLog(It.IsAny<string>())).Returns(logMock.Object);
            vmDisplay = new DisplayViewModel(console, log, deviceManagerSA, easyArrange) { SelectedHomeDevice = new HomeDevice() { MonitorInfo = new MonitorInfo() { DisplayName = "AA" } } };
            ezMemoryAddApplication = new EzMemoryAddApplication(vmDisplay, vm,new HomeDevice());
            privateObject = new PrivateObject(ezMemoryAddApplication);
        }

        [Test]
        public void TestConstructor_EzMemoryAddApplication()
        {
            // Assert
            Assert.That(ezMemoryAddApplication, Is.Not.Null);
            Assert.That(privateObject.GetFieldOrProperty("_vm"), Is.InstanceOf<EzArrangeViewModel>());
            Assert.That(ezMemoryAddApplication.DataContext, Is.InstanceOf<EzArrangeViewModel>());
        }
    }
}
