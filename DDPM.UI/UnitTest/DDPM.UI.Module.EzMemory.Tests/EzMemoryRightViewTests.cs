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
using DDPM.Easy.Common;
using DDPM.UI.Common.UserControls;
using System.Windows.Forms;
using System.Windows.Controls;
using System.Xml.Linq;
using static System.Net.Mime.MediaTypeNames;
using System.Drawing.Drawing2D;
using Windows.ApplicationModel;
using System.Security.AccessControl;
using System.Runtime.Intrinsics.X86;

namespace DDPM.UI.Module.EzMemory.Tests
{
    [TestFixture, Apartment(ApartmentState.STA)]
    public class EzMemoryRightViewTests
    {
        private EzMemoryRightView? ezMemoryRightView;
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
        private SplitItem? splitItem;
        private Mock<ISplitCtrl>? ISplitCtrlMock;
        private PrivateObject? privateObjecta;


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
            deviceManagerSA = deviceManagerSAMock.Object;
            DdpmCommonHelper.DeviceManagerSA = deviceManagerSA;
            HomeDevice.DeviceManagerSA = deviceManagerSAMock.Object;
            ISplitCtrlMock = new Mock<ISplitCtrl>();
            ISplitCtrlMock.Setup(x => x.Clone()).Returns(ISplitCtrlMock.Object);
            ISplitCtrlMock.Setup(x => x.CellList).Returns(new List<CellObj>() { new CellObj("name1", new Border()) { CellBd = new CellBorder() }, new CellObj("name1", new Border()) { CellBd = new CellBorder() } });

            splitItem = new SplitItem();
            privateObjecta = new PrivateObject(splitItem);
            privateObjecta.SetFieldOrProperty("vm", new SplitItemViewModel() { SplitCtrl = ISplitCtrlMock.Object });
            vm = new EzArrangeViewModel(new HomeDevice()) { currentEditprofileSetting = new SA.Common.Settings.EzProfileSettingDDPM(1, true, 2, true) { Auto = true }, SelectedSplitItem = splitItem, IsEditProfile = true, currentEditprofile = new SA.Common.Settings.EAProfileDDPM() { AppInfos = new List<SA.Common.Settings.EAAppInfoDDPM>() } };
            logMock = new Mock<ILog>();
            log = logMock.Object;
            consoleMock.Setup(x => x.CreateLog(It.IsAny<string>())).Returns(logMock.Object);
            vmDisplay = new DisplayViewModel(console, log, deviceManagerSA, easyArrange) { SelectedHomeDevice = new HomeDevice() { MonitorInfo = new MonitorInfo() { DisplayName = "AA" } } };
            ezMemoryRightView = new EzMemoryRightView(vmDisplay);
            privateObject = new PrivateObject(ezMemoryRightView);
        }

        [Test]
        public void TestConstructor_EzMemoryRightView()
        {
            // Assert
            EzArrangeViewModel vma = (EzArrangeViewModel)privateObject.GetFieldOrProperty("_vm");
            Assert.That(ezMemoryRightView, Is.Not.Null);
            Assert.That(vma, Is.InstanceOf<EzArrangeViewModel>());
            Assert.That(ezMemoryRightView.DataContext, Is.InstanceOf<EzArrangeViewModel>());
        }

        [Test]
        public void TestHandleSelectedHomeDeviceChanged()
        {
            ezMemoryRightView.HandleSelectedHomeDeviceChanged();
            EzArrangeViewModel vma = (EzArrangeViewModel)privateObject.GetFieldOrProperty("_vm");
            Assert.That(vma, Is.InstanceOf<EzArrangeViewModel>());
            Assert.That(ezMemoryRightView.DataContext, Is.InstanceOf<EzArrangeViewModel>());
        }


        [TearDown]
        public void TearDown()
        {
            // Dispose of splitItem after each test
            if (splitItem != null)
            {
                splitItem.Dispose();
                splitItem = null;
            }
        }

    }
}
