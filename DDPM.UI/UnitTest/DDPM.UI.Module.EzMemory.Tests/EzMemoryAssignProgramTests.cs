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

namespace DDPM.UI.Module.EzMemory.Tests
{
    [TestFixture, Apartment(ApartmentState.STA)]
    public class EzMemoryAssignProgramTests
    {
        private EzMemoryAssignProgram? ezMemoryAssignProgram;
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
            deviceManagerSAMock.Setup(x => x.GetEAFunctionEnabled()).Returns(Task.FromResult(new ObjGetVCP() { result = true, value = true }));
            DdpmCommonHelper.DeviceManagerSA = deviceManagerSA;
            deviceManagerSA = deviceManagerSAMock.Object;
            HomeDevice.DeviceManagerSA = deviceManagerSAMock.Object;
            ISplitCtrlMock = new Mock<ISplitCtrl>();
            ISplitCtrlMock.Setup(x => x.Clone()).Returns(ISplitCtrlMock.Object);
            ISplitCtrlMock.Setup(x => x.CellList).Returns(new List<CellObj>() { new CellObj("name1", new Border()) { CellBd = new CellBorder() }, new CellObj("name1", new Border()) { CellBd = new CellBorder() } });

            splitItem = new SplitItem();
            privateObjecta = new PrivateObject(splitItem);
            privateObjecta.SetFieldOrProperty("vm", new SplitItemViewModel() { SplitCtrl = ISplitCtrlMock.Object });
            vm = new EzArrangeViewModel(new HomeDevice()) { SelectedSplitItem = splitItem, IsEditProfile=true , currentEditprofile =new SA.Common.Settings.EAProfileDDPM() { AppInfos =new List<SA.Common.Settings.EAAppInfoDDPM>()} };
            logMock = new Mock<ILog>();
            log = logMock.Object;
            consoleMock.Setup(x => x.CreateLog(It.IsAny<string>())).Returns(logMock.Object);
            vmDisplay = new DisplayViewModel(console, log, deviceManagerSA, easyArrange) { SelectedHomeDevice = new HomeDevice() { MonitorInfo = new MonitorInfo() { DisplayName = "AA" } } };
            ezMemoryAssignProgram = new EzMemoryAssignProgram(vmDisplay, vm, new HomeDevice());
            privateObject = new PrivateObject(ezMemoryAssignProgram);
        }

        [Test]
        public void TestConstructor_EzMemoryAssignProgram()
        {
            // Assert
            var vma = privateObject.GetFieldOrProperty("_vm");
            Assert.That(ezMemoryAssignProgram, Is.Not.Null);
            Assert.That(vm, Is.InstanceOf<EzArrangeViewModel>());
            Assert.That(ezMemoryAssignProgram.DataContext, Is.InstanceOf<EzArrangeViewModel>());
        }

        [Test]
        public void TestConstructor_EzMemoryAssignPrograma()
        {
            ISplitCtrlMock.Setup(x => x.CellList).Returns(new List<CellObj>() { new CellObj("name1", new Border()) { CellBd = new CellBorder() }, new CellObj("EzMemory", new Border()) { CellBd = new CellBorder() }, new CellObj("name3", new Border()) { CellBd = new CellBorder() } });
            ezMemoryAssignProgram = new EzMemoryAssignProgram(vmDisplay, vm, new HomeDevice());
            // Assert
            var vma = privateObject.GetFieldOrProperty("_vm");
            Assert.That(ezMemoryAssignProgram, Is.Not.Null);
            Assert.That(vm, Is.InstanceOf<EzArrangeViewModel>());
            Assert.That(ezMemoryAssignProgram.DataContext, Is.InstanceOf<EzArrangeViewModel>());
        }

        [Test]
        public void TestInitializePagea()
        {
            ezMemoryAssignProgram.InitializePage();
            var pageData=new EzMemoryPageData() {MainText= "Assign programs", SubText= "Assign applications/documents to windows or drag the application icon to the respective partition.\r\n\r\nNote: Easy Arrange Memory usability may vary according to application type and launch behavior." };
            var MainTextText = pageData.MainText!;
            var SubTextText = pageData.SubText!;
            EzArrangeViewModel vm = (EzArrangeViewModel)privateObject.GetFieldOrProperty("_vm");
            Assert.That(vm.IsRightGridPage2Visible,Is.EqualTo(true)); 
            Assert.That(vm.SelectedValue, Is.EqualTo(2));
            Assert.That(MainTextText, Is.EqualTo("Assign programs"));
            Assert.That(SubTextText, Is.EqualTo("Assign applications/documents to windows or drag the application icon to the respective partition.\r\n\r\nNote: Easy Arrange Memory usability may vary according to application type and launch behavior."));
        }

        [Test]
        public void TestInitializePageb()
        {
            ISplitCtrlMock.Setup(x => x.CellList).Returns(new List<CellObj>() { new CellObj("name1", new Border()) { CellBd = new CellBorder() }, new CellObj("name2", new Border()) { CellBd = new CellBorder() }, new CellObj("name3", new Border()) { CellBd = new CellBorder() } });
            ezMemoryAssignProgram = new EzMemoryAssignProgram(vmDisplay, vm, new HomeDevice());
            ezMemoryAssignProgram.InitializePage();
            EzArrangeViewModel vma = (EzArrangeViewModel)privateObject.GetFieldOrProperty("_vm");
            Assert.That(vm.IsRightGridPage2Visible, Is.EqualTo(false));
            Assert.That(vm.SelectedValue, Is.EqualTo(3));
            Assert.That(vm, Is.InstanceOf<EzArrangeViewModel>());
            Assert.That(ezMemoryAssignProgram.DataContext, Is.InstanceOf<EzArrangeViewModel>());
        }

        [Test]
        public void TestSyncEditStatusForAssignPage()
        {
            vm = new EzArrangeViewModel(new HomeDevice()) { SelectedSplitItem = splitItem, IsEditProfile = true, currentEditprofile = new SA.Common.Settings.EAProfileDDPM() { AppInfos = new List<SA.Common.Settings.EAAppInfoDDPM>() { new SA.Common.Settings.EAAppInfoDDPM() { Path= "EAAppInfoDDPM" },new SA.Common.Settings.EAAppInfoDDPM() { Path = "EAAppInfoDDP" } } }, _sortApps = new Dictionary<string, Bind_AddFullPage_AppCollectionData>(), SelectedValue = 1 , _bind_apps =new System.Collections.ObjectModel.ObservableCollection<Bind_AddFullPage_AppCollectionData>() { new Bind_AddFullPage_AppCollectionData() { AppPath= "EAAppInfoDDPM",AppName="Name1" , AppUserModelID ="1", AppType ="Type1",AppIcon="Icon1"} } };
            ISplitCtrlMock.Setup(x => x.CellList).Returns(new List<CellObj>() { new CellObj("name1", new Border()) { CellBd = new CellBorder() }, new CellObj("name2", new Border()) { CellBd = new CellBorder() }, new CellObj("name3", new Border()) { CellBd = new CellBorder() } });
            ezMemoryAssignProgram = new EzMemoryAssignProgram(vmDisplay, vm, new HomeDevice());
            ezMemoryAssignProgram.SyncEditStatusForAssignPage();
            EzArrangeViewModel vma = (EzArrangeViewModel)privateObject.GetFieldOrProperty("_vm");
            Assert.That(vm, Is.InstanceOf<EzArrangeViewModel>());
            Assert.That(ezMemoryAssignProgram.DataContext, Is.InstanceOf<EzArrangeViewModel>());
            Assert.That(vma._sortApps.Count,Is.EqualTo(0));
        }

        [Test]
        public void TestNextPage()
        {          
            ezMemoryAssignProgram.NextPage();
            EzArrangeViewModel vm = (EzArrangeViewModel)privateObject.GetFieldOrProperty("_vm");
            // Assert
            Assert.That(ezMemoryAssignProgram, Is.Not.Null);
            Assert.That(vm, Is.InstanceOf<EzArrangeViewModel>());
            Assert.That(ezMemoryAssignProgram.DataContext, Is.InstanceOf<EzArrangeViewModel>());
            Assert.That(vm.IsAddPageBack,Is.EqualTo(false));
            Assert.NotZero(vm._currentPageIndex);
        }

        [Test]
        public void TestPreviousPage()
        {
            vm = new EzArrangeViewModel(new HomeDevice()) {_currentPageIndex=3, SelectedSplitItem = splitItem, IsEditProfile = true, currentEditprofile = new SA.Common.Settings.EAProfileDDPM() { AppInfos = new List<SA.Common.Settings.EAAppInfoDDPM>() { new SA.Common.Settings.EAAppInfoDDPM() { Path = "EAAppInfoDDPM" }, new SA.Common.Settings.EAAppInfoDDPM() { Path = "EAAppInfoDDP" } } }, _sortApps = new Dictionary<string, Bind_AddFullPage_AppCollectionData>(), SelectedValue = 1, _bind_apps = new System.Collections.ObjectModel.ObservableCollection<Bind_AddFullPage_AppCollectionData>() { new Bind_AddFullPage_AppCollectionData() { AppPath = "EAAppInfoDDPM", AppName = "Name1", AppUserModelID = "1", AppType = "Type1", AppIcon = "Icon1" } } };
            ezMemoryAssignProgram = new EzMemoryAssignProgram(vmDisplay, vm, new HomeDevice());
            ezMemoryAssignProgram.PreviousPage();
            // Assert
            Assert.That(ezMemoryAssignProgram, Is.Not.Null);
            Assert.That(vm._currentPageIndex, Is.EqualTo(2));
        }

        [Test]
        public void TestUpdatePageContent()
        {
            var pageData = new EzMemoryPageData() { MainText = "Assign programs", SubText = "Assign applications/documents to windows or drag the application icon to the respective partition.\r\n\r\nNote: Easy Arrange Memory usability may vary according to application type and launch behavior." };
            var MainTextText = pageData.MainText!;
            var SubTextText = pageData.SubText!;
            vm = new EzArrangeViewModel(new HomeDevice()) { ezPages=new Dictionary<string, List<EzMemoryPageData>>() { { "EzMemory", new List<EzMemoryPageData>() { new EzMemoryPageData(),new EzMemoryPageData(),new EzMemoryPageData(), pageData } }, { "EzMemoryb", new List<EzMemoryPageData>() },{ "EzMemoryc",new List<EzMemoryPageData>() } }, _currentPageIndex = 3, SelectedSplitItem = splitItem, IsEditProfile = true, currentEditprofile = new SA.Common.Settings.EAProfileDDPM() { AppInfos = new List<SA.Common.Settings.EAAppInfoDDPM>() { new SA.Common.Settings.EAAppInfoDDPM() { Path = "EAAppInfoDDPM" }, new SA.Common.Settings.EAAppInfoDDPM() { Path = "EAAppInfoDDP" } } }, _sortApps = new Dictionary<string, Bind_AddFullPage_AppCollectionData>(), SelectedValue = 1, _bind_apps = new System.Collections.ObjectModel.ObservableCollection<Bind_AddFullPage_AppCollectionData>() { new Bind_AddFullPage_AppCollectionData() { AppPath = "EAAppInfoDDPM", AppName = "Name1", AppUserModelID = "1", AppType = "Type1", AppIcon = "Icon1" } } };
            privateObject.SetFieldOrProperty("_vm", vm);
            ezMemoryAssignProgram.UpdatePageContent();
            EzArrangeViewModel vma = (EzArrangeViewModel)privateObject.GetFieldOrProperty("_vm");
            // Assert
            Assert.That(vm, Is.InstanceOf<EzArrangeViewModel>());
            Assert.That(vma.ProgressValue, Is.EqualTo(1));
            Assert.That(vma.IsAddPageBack, Is.EqualTo(false));
            Assert.That(MainTextText, Is.EqualTo("Assign programs"));
            Assert.That(SubTextText, Is.EqualTo("Assign applications/documents to windows or drag the application icon to the respective partition.\r\n\r\nNote: Easy Arrange Memory usability may vary according to application type and launch behavior."));
        }

        //class WindowGridVisibilityConverter
        [Test]
        public void TestConvert()
        {
            var windowGridVisibilityConverter=new WindowGridVisibilityConverter();
            var result = windowGridVisibilityConverter.Convert(null,null,null,null);
            // Assert
            Assert.That(result, Is.EqualTo(Visibility.Collapsed));
        }

        [Test]
        public void TestConverta()
        {
            var windowGridVisibilityConverter = new WindowGridVisibilityConverter();
            var result = windowGridVisibilityConverter.Convert(3, null, "1", null);
            // Assert
            Assert.That(result, Is.EqualTo(Visibility.Visible));
        }

        [Test]
        public void TestConvertb()
        {
            var windowGridVisibilityConverter = new WindowGridVisibilityConverter();
            var result = windowGridVisibilityConverter.Convert(1, null, "3", null);
            // Assert
            Assert.That(result, Is.EqualTo(Visibility.Collapsed));
        }
    }
}
