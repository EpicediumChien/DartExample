using DDPM.SA.Common;
using DDPM.UI.Common.Interfaces;
using DDPM.UI.Common;
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
using DDPM.UI.Common.ViewModels;
using Dell.Client.Framework.UX.WPF.ResourceManager;
using DDPM.UI.Plugin.Common.ViewModels;
using DDPM.SA.Common.Interfaces;
using DDPM.UI.Common.Models;
using DDPM.UI.Common.EAEM;
using VcpCore.Common;
using DDPM.SA.Common.Settings;

namespace DDPM.UI.Module.EzArrange.Tests
{
    [Apartment(ApartmentState.STA)]
    public class EzArrangeRightVierwTests
    {
        private EzArrangeRightVierw? ezArrangeRightVierw;
        private PrivateObject? privateObject;
        private Mock<IModuleOwner>? moduleOwnerMock;
        private Mock<IDeviceManagerSA>? deviceManagerMock;
        private IDeviceManagerSA? deviceManagerSA;
        private Mock<IConsole>? consoleMock;
        private IConsole? console;
        private Mock<ILog>? logMock;
        private ILog? log;
        private EzArrangeViewModel? vm;
        private DisplayViewModel? vmDisplay;
        private Mock<IEasyArrangeService>? easyArrange;


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
            var moduleOwner = moduleOwnerMock!.Object;
            DdpmCommonHelper.ModuleOwner = moduleOwner;
            moduleOwnerMock.Setup(x => x.SelectedHomeDevice).Returns(new HomeDevice() { MonitorInfo = new MonitorInfo() { DisplayName = "NAME" } });
            deviceManagerMock = new Mock<IDeviceManagerSA>();
            //Robert_Lin, 2025-1-7, IsEAFunctionEnabled is deleted.
            //deviceManagerMock.Setup(x => x.GetEAFunctionEnabled()).Returns(Task.FromResult(new VcpCore.Common.ObjGetVCP()));
            deviceManagerSA = deviceManagerMock.Object;
            DdpmCommonHelper.DeviceManagerSA = deviceManagerSA;
            consoleMock = new Mock<IConsole>();
            console = consoleMock.Object;
            DdpmCommonHelper.MyConsole = console;
            logMock = new Mock<ILog>();
            log = logMock.Object;
            easyArrange = new Mock<IEasyArrangeService>();
            HomeDevice.DeviceManagerSA = deviceManagerSA;
            vmDisplay = new DisplayViewModel(console, log, deviceManagerSA, easyArrange.Object) { SelectedHomeDevice = new Common.Models.HomeDevice() { vmEzArrange = new EzArrangeViewModel(new Common.Models.HomeDevice()) { SelectedSplitItem = new Common.UserControls.SplitItem() }, MonitorInfo = new VcpCore.Common.MonitorInfo() { DisplayName = "NAME" } } };
            ezArrangeRightVierw = new EzArrangeRightVierw(vmDisplay);
            privateObject = new PrivateObject(ezArrangeRightVierw);
        }

        [Test]
        public void TestConstructor_EzArrangeRightVierw()
        {
            var splitListView_RecentSplitOwner = Common.EAEM.eSplitOwner.EaRecent;
            var splitListView_CustomSplitOwner = Common.EAEM.eSplitOwner.EaCustom;
            var splitListView_2wSplitOwner = Common.EAEM.eSplitOwner.EaWin;
            var splitListView_3wSplitOwner = Common.EAEM.eSplitOwner.EaWin;
            var splitListView_4wSplitOwner = Common.EAEM.eSplitOwner.EaWin;
            var splitListView_5wSplitOwner = Common.EAEM.eSplitOwner.EaWin;
            var splitListView_6wSplitOwner = Common.EAEM.eSplitOwner.EaWin;
            var splitListView_7wSplitOwner = Common.EAEM.eSplitOwner.EaWin;
            Assert.That(ezArrangeRightVierw, Is.Not.Null);
            Assert.That(ezArrangeRightVierw.DataContext, Is.InstanceOf<EzArrangeViewModel>());
            Assert.That(splitListView_RecentSplitOwner, Is.EqualTo(eSplitOwner.EaRecent));
            Assert.That(splitListView_CustomSplitOwner, Is.EqualTo(eSplitOwner.EaCustom));
            Assert.That(splitListView_2wSplitOwner, Is.EqualTo(eSplitOwner.EaWin));
            Assert.That(splitListView_3wSplitOwner, Is.EqualTo(eSplitOwner.EaWin));
            Assert.That(splitListView_4wSplitOwner, Is.EqualTo(eSplitOwner.EaWin));
            Assert.That(splitListView_5wSplitOwner, Is.EqualTo(eSplitOwner.EaWin));
            Assert.That(splitListView_6wSplitOwner, Is.EqualTo(eSplitOwner.EaWin));
            Assert.That(splitListView_7wSplitOwner, Is.EqualTo(eSplitOwner.EaWin));
        }

        [Test]
        public void TestGenerateCustomId()
        {
            var result = EzArrangeRightVierw.GenerateCustomId();
            Assert.NotZero(result);
        }

        [Test]
        public void TestHandleSelectedHomeDeviceChanged()
        {
            privateObject.SetFieldOrProperty("_deviceManagerSA", null);
            ezArrangeRightVierw.HandleSelectedHomeDeviceChanged();
            Assert.That(privateObject.GetFieldOrProperty("_vm"), Is.InstanceOf<EzArrangeViewModel>());
            Assert.That(ezArrangeRightVierw.DataContext, Is.InstanceOf<EzArrangeViewModel>());
        }

        [TearDown]
        public void TearDown()
        {
            // Dispose of splitItem after each test
            if (ezArrangeRightVierw != null)
            {
                ezArrangeRightVierw.Dispose();
                ezArrangeRightVierw = null;
            }
        }
    }
}
