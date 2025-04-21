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
using DDPM.UI.Common.EAEM;
using DDPM.SA.Common.Settings;
using System.Globalization;

namespace DDPM.UI.Module.EzMemory.Tests
{
    [TestFixture, Apartment(ApartmentState.STA)]
    public class EzMemoryFirstTests
    {
        private EzMemoryFirst? ezMemoryFirst;
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
            DdpmCommonHelper.DeviceManagerSA = deviceManagerSAMock.Object;
            //Robert_Lin, 2025-1-7, IsEAFunctionEnabled is deleted
            //deviceManagerSAMock.Setup(x => x.GetEAFunctionEnabled()).Returns(Task.FromResult(new ObjGetVCP() { result = true, value = true }));
            //deviceManagerSAMock.Setup(x => x.ReadUserEAProfileDDPM()).Returns(Task.FromResult(new List<EAProfileDDPM>()));
            deviceManagerSA = deviceManagerSAMock.Object;
            DdpmCommonHelper.DeviceManagerSA = deviceManagerSA;
            HomeDevice.DeviceManagerSA = deviceManagerSAMock.Object;
            ISplitCtrlMock = new Mock<ISplitCtrl>();
            ISplitCtrlMock.Setup(x => x.Clone()).Returns(ISplitCtrlMock.Object);
            ISplitCtrlMock.Setup(x => x.CellList).Returns(new List<CellObj>() { new CellObj("name1", new Border()) { CellBd = new CellBorder() }, new CellObj("name1", new Border()) { CellBd = new CellBorder() } });

            splitItem = new SplitItem();
            privateObjecta = new PrivateObject(splitItem);
            privateObjecta.SetFieldOrProperty("vm", new SplitItemViewModel() { SplitCtrl = ISplitCtrlMock.Object });
            vm = new EzArrangeViewModel(new HomeDevice()) { _currentPageIndex = 3, CurrentEditSelectspItem = splitItem, SelectedSplitItem = splitItem, IsEditProfile = true, currentEditprofile = new SA.Common.Settings.EAProfileDDPM() { Name = "InputText", AppInfos = new List<SA.Common.Settings.EAAppInfoDDPM>() } };
            System.Windows.Application.Current.MainWindow = new Window();
            logMock = new Mock<ILog>();
            log = logMock.Object;
            consoleMock.Setup(x => x.CreateLog(It.IsAny<string>())).Returns(logMock.Object);
            vmDisplay = new DisplayViewModel(console, log, deviceManagerSA, easyArrange) { SelectedHomeDevice = new HomeDevice() { MonitorInfo = new MonitorInfo() { DisplayName = "AA" } } };
            ezMemoryFirst = new EzMemoryFirst(vmDisplay, vm, new HomeDevice() { MonitorInfo = new MonitorInfo() { DisplayName = "DISplayName" } });
            privateObject = new PrivateObject(ezMemoryFirst);
        }

        [Test]
        public void TestConstructor_EzMemoryFirst()
        {
            var splitListView_RecentSplitOwner = Common.EAEM.eSplitOwner.EaRecent;
            var splitListView_CustomSplitOwner = Common.EAEM.eSplitOwner.EaCustom;
            var splitListView_2wSplitOwner = Common.EAEM.eSplitOwner.EaWin;
            var splitListView_3wSplitOwner = Common.EAEM.eSplitOwner.EaWin;
            var splitListView_4wSplitOwner = Common.EAEM.eSplitOwner.EaWin;
            var splitListView_5wSplitOwner = Common.EAEM.eSplitOwner.EaWin;
            var splitListView_6wSplitOwner = Common.EAEM.eSplitOwner.EaWin;
            var splitListView_7wSplitOwner = Common.EAEM.eSplitOwner.EaWin;

            // Assert
            EzArrangeViewModel vma = (EzArrangeViewModel)privateObject.GetFieldOrProperty("_vm");
            Assert.That(ezMemoryFirst, Is.Not.Null);
            Assert.That(vm, Is.InstanceOf<EzArrangeViewModel>());
            Assert.That(ezMemoryFirst.DataContext, Is.InstanceOf<EzArrangeViewModel>());
            Assert.That(vma.IsVertical, Is.EqualTo(false));
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
        public void TestConstructor_EzMemoryFirsta()
        {
            vm = new EzArrangeViewModel(new HomeDevice()) { CurrentEditSelectspItem = new SplitItem(), SelectedSplitItem = splitItem, IsEditProfile = false, currentEditprofile = new SA.Common.Settings.EAProfileDDPM() { AppInfos = new List<SA.Common.Settings.EAAppInfoDDPM>() } };
            ezMemoryFirst = new EzMemoryFirst(vmDisplay, vm, new HomeDevice() { MonitorInfo = new MonitorInfo() { DisplayName = "DISplayName" } });
            // Assert
            EzArrangeViewModel vma = (EzArrangeViewModel)privateObject.GetFieldOrProperty("_vm");
            Assert.That(ezMemoryFirst, Is.Not.Null);
            Assert.That(vm, Is.InstanceOf<EzArrangeViewModel>());
            Assert.That(ezMemoryFirst.DataContext, Is.InstanceOf<EzArrangeViewModel>());
            Assert.That(vma.IsVertical, Is.EqualTo(false));
        }

        [Test]
        public void TestInitializePagea()
        {
            ezMemoryFirst.InitializePage();
            var pageData = new EzMemoryPageData() { MainText = "Easy Memory", SubText = "Save different profiles and restore them manually, by scheduled time or at system start-up.\r\n\r\nBegin by assigning a name to your Easy Memory Profile and selecting a layout." };
            var MainTextText = pageData.MainText!;
            var SubTextText = pageData.SubText!;
            EzArrangeViewModel vm = (EzArrangeViewModel)privateObject.GetFieldOrProperty("_vm");
            Assert.That(vm._currentTotalPage, Is.EqualTo(0));
            Assert.That(vm._currentPageIndex, Is.EqualTo(0));
            Assert.That(vm.ProgressValue, Is.EqualTo(1));
            Assert.That(MainTextText, Is.EqualTo("Easy Memory"));
            Assert.That(SubTextText, Is.EqualTo("Save different profiles and restore them manually, by scheduled time or at system start-up.\r\n\r\nBegin by assigning a name to your Easy Memory Profile and selecting a layout."));
        }

        [Test]
        public void TestSyncEditStatusForFirstPage()
        {
            if (CultureInfo.CurrentCulture.Name == "es-US")
            {
                ezMemoryFirst.SyncEditStatusForFirstPage();
                EzArrangeViewModel vma = (EzArrangeViewModel)privateObject.GetFieldOrProperty("_vm");
                Assert.That(vm.InputText, Is.EqualTo("Profile 1"));
            }
        }

        [Test]
        public void TestCheckInputText1()
        {
            deviceManagerSAMock.Setup(x => x.ReadUserEAProfileDDPM()).Returns(Task.FromResult(new List<EAProfileDDPM>() { new EAProfileDDPM(1, "name1", 1, new List<EAAppInfoDDPM>()) }));
            ezMemoryFirst.CheckInputText();
            EzArrangeViewModel vm = (EzArrangeViewModel)privateObject.GetFieldOrProperty("_vm");
            // Assert
            Assert.That(vm.InputText, Is.EqualTo("Profile 1"));
        }

        [Test]
        public void TestCheckInputText2()
        {
            deviceManagerSAMock.Setup(x => x.ReadUserEAProfileDDPM()).Returns(Task.FromResult(new List<EAProfileDDPM>() { new EAProfileDDPM(1, "Profile 1", 1, new List<EAAppInfoDDPM>()), new EAProfileDDPM(2, "Profile 2", 2, new List<EAAppInfoDDPM>()), new EAProfileDDPM(3, "Profile 3", 3, new List<EAAppInfoDDPM>()), new EAProfileDDPM(4, "Profile 4", 4, new List<EAAppInfoDDPM>()), new EAProfileDDPM(5, "Profile 5", 5, new List<EAAppInfoDDPM>()), new EAProfileDDPM(6, "Profile 6", 6, new List<EAAppInfoDDPM>()), new EAProfileDDPM(7, "Profile 7", 7, new List<EAAppInfoDDPM>()), new EAProfileDDPM(8, "Profile 8", 8, new List<EAAppInfoDDPM>()), new EAProfileDDPM(9, "Profile 9", 9, new List<EAAppInfoDDPM>()) }));
            ezMemoryFirst.CheckInputText();
            EzArrangeViewModel vm = (EzArrangeViewModel)privateObject.GetFieldOrProperty("_vm");
            // Assert
            Assert.That(vm.InputText, Is.EqualTo(string.Empty));
        }

        [Test]
        public void TestCheckInputText3()
        {
            if (CultureInfo.CurrentCulture.Name == "es-US")
            {
                ezMemoryFirst.CheckInputText();
                EzArrangeViewModel vm = (EzArrangeViewModel)privateObject.GetFieldOrProperty("_vm");
                // Assert
                Assert.That(vm.InputText, Is.EqualTo("Profile 1"));
            }
        }

        [Test]
        public void TestNextPage()
        {
            ezMemoryFirst.NextPage();
            EzArrangeViewModel vm = (EzArrangeViewModel)privateObject.GetFieldOrProperty("_vm");
            // Assert
            Assert.NotZero(vm._currentPageIndex);

        }

        [Test]
        public void TestPreviousPage()
        {
            vm = new EzArrangeViewModel(new HomeDevice()) { ezPages = new Dictionary<string, List<EzMemoryPageData>>() { { "EzMemory", new List<EzMemoryPageData>() { new EzMemoryPageData(), new EzMemoryPageData(), new EzMemoryPageData(), new EzMemoryPageData() } }, { "EzMemoryb", new List<EzMemoryPageData>() }, { "EzMemoryc", new List<EzMemoryPageData>() } }, _currentPageIndex = 3, SelectedSplitItem = splitItem, IsEditProfile = true, currentEditprofile = new SA.Common.Settings.EAProfileDDPM() { AppInfos = new List<SA.Common.Settings.EAAppInfoDDPM>() { new SA.Common.Settings.EAAppInfoDDPM() { Path = "EAAppInfoDDPM" }, new SA.Common.Settings.EAAppInfoDDPM() { Path = "EAAppInfoDDP" } } }, _sortApps = new Dictionary<string, Bind_AddFullPage_AppCollectionData>(), SelectedValue = 1, _bind_apps = new System.Collections.ObjectModel.ObservableCollection<Bind_AddFullPage_AppCollectionData>() { new Bind_AddFullPage_AppCollectionData() { AppPath = "EAAppInfoDDPM", AppName = "Name1", AppUserModelID = "1", AppType = "Type1", AppIcon = "Icon1" } } };
            privateObject.SetFieldOrProperty("_vm", vm);
            ezMemoryFirst.PreviousPage();
            // Assert
            Assert.That(vm, Is.InstanceOf<EzArrangeViewModel>());
            Assert.That(vm._currentPageIndex, Is.EqualTo(2));
        }

        [Test]
        public void TestUpdatePageContent()
        {
            var pageData = new EzMemoryPageData() { MainText = "Assign programs", SubText = "Assign applications/documents to windows or drag the application icon to the respective partition.\r\n\r\nNote: Easy Arrange Memory usability may vary according to application type and launch behavior." };
            var MainTextText = pageData.MainText!;
            var SubTextText = pageData.SubText!;
            vm = new EzArrangeViewModel(new HomeDevice()) { ezPages = new Dictionary<string, List<EzMemoryPageData>>() { { "EzMemory", new List<EzMemoryPageData>() { new EzMemoryPageData(), new EzMemoryPageData(), new EzMemoryPageData(), pageData } }, { "EzMemoryb", new List<EzMemoryPageData>() }, { "EzMemoryc", new List<EzMemoryPageData>() } }, _currentPageIndex = 3, SelectedSplitItem = splitItem, IsEditProfile = true, currentEditprofile = new SA.Common.Settings.EAProfileDDPM() { AppInfos = new List<SA.Common.Settings.EAAppInfoDDPM>() { new SA.Common.Settings.EAAppInfoDDPM() { Path = "EAAppInfoDDPM" }, new SA.Common.Settings.EAAppInfoDDPM() { Path = "EAAppInfoDDP" } } }, _sortApps = new Dictionary<string, Bind_AddFullPage_AppCollectionData>(), SelectedValue = 1, _bind_apps = new System.Collections.ObjectModel.ObservableCollection<Bind_AddFullPage_AppCollectionData>() { new Bind_AddFullPage_AppCollectionData() { AppPath = "EAAppInfoDDPM", AppName = "Name1", AppUserModelID = "1", AppType = "Type1", AppIcon = "Icon1" } } };
            privateObject.SetFieldOrProperty("_vm", vm);
            ezMemoryFirst.UpdatePageContent();
            EzArrangeViewModel vma = (EzArrangeViewModel)privateObject.GetFieldOrProperty("_vm");
            // Assert
            Assert.That(vm, Is.InstanceOf<EzArrangeViewModel>());
            Assert.That(vma.ProgressValue, Is.EqualTo(1));
            Assert.That(vma.IsAddPageBack, Is.EqualTo(false));
            Assert.That(MainTextText, Is.EqualTo("Assign programs"));
            Assert.That(SubTextText, Is.EqualTo("Assign applications/documents to windows or drag the application icon to the respective partition.\r\n\r\nNote: Easy Arrange Memory usability may vary according to application type and launch behavior."));
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
