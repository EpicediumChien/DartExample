using DDPM.SA.Common;
using DDPM.SA.Common.Interfaces;
using DDPM.UI.Common;
using DDPM.UI.Common.Interfaces;
using DDPM.UI.Common.Interfaces.ViewModels;
using DDPM.UI.Common.Models;
using DDPM.UI.Common.UserControls;
using DDPM.UI.Common.ViewModels;
using DDPM.UI.Interfaces;
using DDPM.UI.Module.Brightness;
using DDPM.UI.Plugin.Common.ViewModels;
using DDPM.UI.Plugin.ViewModels;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.UX.WPF;
using DPeMPublic.Common.Enums;
using Moq;
using NGA.UnitTest.PrivateObject;
using System;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using VcpCore.Common;
using Windows.System;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Menu;

namespace DDPM.UI.Plugin.Common.Tests
{
    [Apartment(ApartmentState.STA)]
    public class DisplayViewModelTests
    {
        private DisplayViewModel? displayViewModel;
        private PrivateObject? privateObject;
        private Mock<IModuleOwner>? moduleOwnerMock;
        private IModuleOwner? moduleOwner;
        private IConsole? console;
        private Mock<IConsole>? consoleMock;
        private IEasyArrangeService? easyArrangeService;
        private Mock<IEasyArrangeService>? easyArrangeServiceMock;
        private IDeviceManagerSA? deviceManagerSA;
        private Mock<IDeviceManagerSA>? deviceManagerSAMock;
        private ILog? log;
        private Mock<ILog>? logMock;
        private ModuleGroup moduleGroup;
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

            moduleOwnerMock = new Mock<IModuleOwner>();
            moduleOwner = moduleOwnerMock!.Object;
            DdpmCommonHelper.ModuleOwner = moduleOwner;
            consoleMock = new Mock<IConsole>();
            console = consoleMock.Object;
            easyArrangeServiceMock = new Mock<IEasyArrangeService>();
            easyArrangeService = easyArrangeServiceMock.Object;
            deviceManagerSAMock = new Mock<IDeviceManagerSA>();
            deviceManagerSA = deviceManagerSAMock.Object;
            logMock = new Mock<ILog>();
            log = logMock.Object;
            displayViewModel = new DisplayViewModel(console, log, deviceManagerSA, easyArrangeService);
            privateObject = new PrivateObject(displayViewModel);
            moduleGroup = new();
        }

        [Test]
        public void TestConstructor_DisplayViewModel()
        {
            // Assert
            Assert.That(displayViewModel, Is.Not.Null);
        }

        [Test]
        public void TestConsole()
        {
            // Assert
            Assert.That(displayViewModel.Console, Is.Not.Null);
        }

        [Test]
        public void TestLog()
        {
            // Assert
            Assert.That(displayViewModel.Log, Is.Not.Null);
        }

        [Test]
        public void TestDeviceManagerSA()
        {
            // Assert
            Assert.That(displayViewModel.DeviceManagerSA, Is.Not.Null);
        }

        [Test]
        public void TestEasyArrangeService()
        {
            // Assert
            Assert.That(displayViewModel.EasyArrangeService, Is.Not.Null);
        }
        

        [Test]
        public void TestSelectedHomeDevice()
        {
            // Act
            var selectedHomeDevice = new HomeDevice();
            displayViewModel.SelectedHomeDevice=selectedHomeDevice;
            // Assert
            Assert.That(displayViewModel.SelectedHomeDevice, Is.EqualTo(selectedHomeDevice));
        }

        [Test]
        public void TestHomeDevices()
        {
            // Act
            var homedevices = new List<HomeDevice>();
            displayViewModel.HomeDevices = homedevices;
            // Assert
            Assert.That(displayViewModel.HomeDevices, Is.EqualTo(homedevices));
        }

        [Test]
        public void TestGroupCount()
        {
            // Act
            privateObject.SetFieldOrProperty("_moduleGroups", new List<ModuleGroup>() { new ModuleGroup(), new ModuleGroup()});
            // Assert
            Assert.That(displayViewModel.GroupCount, Is.EqualTo(2));
        }

        [Test]
        public void TestVbarItems()
        {
            // Act
            privateObject.SetFieldOrProperty("_vbarItems", new List<VbarItem> { new VbarItem(1,null,""), new VbarItem(1, null, "") });
            // Assert
            Assert.That(displayViewModel.VbarItems.Count, Is.EqualTo(2));
        }

        [Test]
        public void TestVbarItems1()
        {
            // Act
            privateObject.SetFieldOrProperty("_vbarItems1", new List<VbarItem1>() { new VbarItem1(), new VbarItem1() });
            // Assert
            Assert.That(displayViewModel.VbarItems1.Count, Is.EqualTo(2));
        }

        [Test]
        public void TestSetModuleGroups()
        {
            var ddpmModuleMock = new Mock<IDdpmModule?>();
            moduleGroup=new ModuleGroup();
            PrivateObject pro = new PrivateObject(moduleGroup);
            var headers = new ObservableCollection<RightViewHeader>() { new RightViewHeader(1, "text") { DdpmModule = ddpmModuleMock.Object } };
            pro.SetFieldOrProperty("_headers", headers);
            var moduleGroups = new List<ModuleGroup>() {new ModuleGroup(), moduleGroup };
            try
            {
                displayViewModel.SetModuleGroups(moduleGroups);
                Assert.True(true);
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }
        }

        [Test]
        public void TestGroupSelectedIndex()
        {
            displayViewModel.GroupSelectedIndex = 1;
            // Assert
            Assert.That(displayViewModel.GroupSelectedIndex, Is.EqualTo(1));

            var dDdpmModuleMock = new Mock<IDdpmModule>();
            var _headers = new ObservableCollection<RightViewHeader>() { new RightViewHeader(1, "text") { DdpmModule = dDdpmModuleMock.Object } };
            moduleGroup = new ModuleGroup();
            var privateObjecta = new PrivateObject(moduleGroup);
            privateObjecta.SetFieldOrProperty("_headers", _headers);
            var moduleGroups = new List<ModuleGroup>() { moduleGroup, moduleGroup };
            privateObject.SetFieldOrProperty("_moduleGroups", moduleGroups);
            displayViewModel.GroupSelectedIndex = 1;
            // Assert
            Assert.That(displayViewModel.GroupSelectedIndex, Is.EqualTo(1));

            var _vbarItems = new List<VbarItem>() { new VbarItem(1, null, ""), new VbarItem(1, null, "") };
            privateObject.SetFieldOrProperty("_vbarItems", _vbarItems);
            displayViewModel.GroupSelectedIndex = 1;
            // Assert
            Assert.That(displayViewModel.GroupSelectedIndex, Is.EqualTo(1));
        }

        [Test]
        public void TestSelectedGroup()
        {
            // Assert
            Assert.That(displayViewModel.SelectedGroup, Is.EqualTo(null));

            var dDdpmModuleMock = new Mock<IDdpmModule>();
            var _headers = new ObservableCollection<RightViewHeader>() { new RightViewHeader(1, "text") { DdpmModule = dDdpmModuleMock.Object } };
            moduleGroup = new ModuleGroup();
            var privateObjecta = new PrivateObject(moduleGroup);
            privateObjecta.SetFieldOrProperty("_headers", _headers);
            var moduleGroups = new List<ModuleGroup>() { moduleGroup, moduleGroup };
            privateObject.SetFieldOrProperty("_moduleGroups", moduleGroups);
            displayViewModel.GroupSelectedIndex = 5;
            // Assert
            Assert.That(displayViewModel.SelectedGroup, Is.EqualTo(null));

            displayViewModel.GroupSelectedIndex = 1;
            // Assert
            Assert.That(displayViewModel.SelectedGroup, Is.Not.Null);
        }

        [Test]
        public void TestIsLandingMode()
        {
            // Act
            displayViewModel.GroupSelectedIndex = 1;
            // Assert
            Assert.That(displayViewModel.IsLandingMode, Is.EqualTo(false));

            // Act
            displayViewModel.GroupSelectedIndex = -1;
            // Assert
            Assert.That(displayViewModel.IsLandingMode, Is.EqualTo(true));
        }

        [Test]
        public void TestRightViewHeaders()
        {
            // Act
            displayViewModel.RightViewHeaderChanged += new RoutedEventHandler(newRoutedEventArgs);
            var rightViewHeaders = new ObservableCollection<RightViewHeader>();
            displayViewModel.RightViewHeaders= rightViewHeaders;
            // Assert
            Assert.That(displayViewModel.RightViewHeaders, Is.Not.Null);
            Assert.That(i, Is.EqualTo(2));
        }

        int i = 0;
        private void newRoutedEventArgs(object? sender, RoutedEventArgs e)
        {
            i = 2;
        }

        [Test]
        public void TestRightViewHeaderSelectedIndex()
        {
            // Act
            displayViewModel.RightViewHeaderSelectedIndex = -1;
            // Assert
            Assert.That(displayViewModel.RightViewHeaderSelectedIndex, Is.EqualTo(0));

            // Act
            var moduleGroups = new List<ModuleGroup>() { new ModuleGroup(), new ModuleGroup() };
            privateObject.SetFieldOrProperty("_moduleGroups", moduleGroups);
            displayViewModel.GroupSelectedIndex = 1;
            displayViewModel.RightViewHeaderSelectedIndex = 1;
            // Assert
            Assert.That(displayViewModel.RightViewHeaderSelectedIndex, Is.EqualTo(1));
        }

        [Test]
        public void TestSelRightViewHeader()
        {
            // Assert
            Assert.That(displayViewModel.SelRightViewHeader, Is.EqualTo(null));

            // Act
            var _headers = new ObservableCollection<RightViewHeader>() { new RightViewHeader(1, "text") };
            moduleGroup = new ModuleGroup();
            var privateObjecta = new PrivateObject(moduleGroup);
            privateObjecta.SetFieldOrProperty("_headers", _headers);
            var moduleGroups = new List<ModuleGroup>() { moduleGroup, moduleGroup };
            privateObject.SetFieldOrProperty("_moduleGroups", moduleGroups);
            displayViewModel.GroupSelectedIndex = 1;
            // Assert
            Assert.That(displayViewModel.SelRightViewHeader, Is.Not.Null);
        }

        [Test]
        public void TestDefaultLeftView()
        {
            // Act
            var defaultLeftView = new System.Windows.Controls.UserControl();
            displayViewModel.DefaultLeftView = defaultLeftView;
            // Assert
            Assert.That(displayViewModel.DefaultLeftView, Is.EqualTo(defaultLeftView));
        }

        [Test]
        public void TestLeftView()
        {
            // Act
            var defaultLeftView = new System.Windows.Controls.UserControl();
            displayViewModel.DefaultLeftView = defaultLeftView;
            // Assert
            Assert.That(displayViewModel.LeftView, Is.EqualTo(defaultLeftView));

            // Act
            var dDdpmModuleMock = new Mock<IDdpmModule>();
            dDdpmModuleMock.Setup(x=>x.GetLeftView()).Returns(defaultLeftView);
            var _headers = new ObservableCollection<RightViewHeader>() { new RightViewHeader(1, "text") { DdpmModule = dDdpmModuleMock.Object } };
            moduleGroup = new ModuleGroup();
            var privateObjecta = new PrivateObject(moduleGroup);
            privateObjecta.SetFieldOrProperty("_headers", _headers);
            var moduleGroups = new List<ModuleGroup>() { moduleGroup, moduleGroup };
            privateObject.SetFieldOrProperty("_moduleGroups", moduleGroups);
            displayViewModel.GroupSelectedIndex = 1;
            // Assert
            Assert.That(displayViewModel.LeftView, Is.EqualTo(defaultLeftView));
        }

        [Test]
        public void TestRightView()
        {
            // Act

            // Assert
            Assert.That(displayViewModel.RightView, Is.EqualTo(null));

            // Act
            var dDdpmModuleMock = new Mock<IDdpmModule>();
            dDdpmModuleMock.Setup(x => x.GetRightView()).Returns(new System.Windows.Controls.UserControl());
            var _headers = new ObservableCollection<RightViewHeader>() { new RightViewHeader(1, "text") { DdpmModule = dDdpmModuleMock.Object } };
            moduleGroup = new ModuleGroup();
            var privateObjecta = new PrivateObject(moduleGroup);
            privateObjecta.SetFieldOrProperty("_headers", _headers);
            var moduleGroups = new List<ModuleGroup>() { moduleGroup, moduleGroup };
            privateObject.SetFieldOrProperty("_moduleGroups", moduleGroups);
            displayViewModel.GroupSelectedIndex = 1;
            // Assert
            Assert.That(displayViewModel.RightView, Is.Not.Null);

            // Act
            var IConsoleMock = new Mock<IConsole>();
            DdpmCommonHelper.MyConsole = IConsoleMock.Object;
            var ModuleOwnerMock = new Mock<IModuleOwner>();
            DdpmCommonHelper.ModuleOwner = ModuleOwnerMock.Object;
            ModuleOwnerMock.Setup(x => x.SelectedHomeDevice).Returns(new HomeDevice());
            var DeviceManagerSAMock = new Mock<IDeviceManagerSA>();
            DdpmCommonHelper.DeviceManagerSA = DeviceManagerSAMock.Object;
            DeviceManagerSAMock.Setup(x => x.GetHDRStatus(It.IsAny<MonitorInfo>())).Returns(Task.FromResult(true));
            ModuleOwnerMock.Setup(x => x.HomeDevices).Returns(new List<HomeDevice>() { new HomeDevice(), new HomeDevice() });
            var brightnessViewModel = new BrightnessViewModel();
            brightnessViewModel.SelectedHomeDevice = new HomeDevice();
            var monitorInfo = new MonitorInfo();
            brightnessViewModel.SelectedHomeDevice.MonitorInfo = monitorInfo;
            _headers = new ObservableCollection<RightViewHeader>() { new RightViewHeader(1, "text") { ModuleType = typeof(BrightnessModule) } };
            moduleGroup = new ModuleGroup();
            privateObjecta = new PrivateObject(moduleGroup);
            privateObjecta.SetFieldOrProperty("_headers", _headers);
            moduleGroups = new List<ModuleGroup>() { moduleGroup, moduleGroup };
            privateObject.SetFieldOrProperty("_moduleGroups", moduleGroups);
            displayViewModel.GroupSelectedIndex = 1;
            displayViewModel.RightViewHeaderSelectedIndex = 0;
            // Assert
            Assert.That(displayViewModel.RightView, Is.Not.Null);
        }

        [Test]
        public void TestRightViewModuleName()
        {
            // Act
            // Assert
            Assert.That(displayViewModel.RightViewModuleName, Is.EqualTo("(ERROR)"));

            // Act
            var dDdpmModuleMock = new Mock<IDdpmModule>();
            dDdpmModuleMock.Setup(x => x.ModuleName).Returns("ModuleName");
            var _headers = new ObservableCollection<RightViewHeader>() { new RightViewHeader(1, "text") { DdpmModule = dDdpmModuleMock.Object } };
            moduleGroup = new ModuleGroup();
            var privateObjecta = new PrivateObject(moduleGroup);
            privateObjecta.SetFieldOrProperty("_headers", _headers);
            var moduleGroups = new List<ModuleGroup>() { moduleGroup, moduleGroup };
            privateObject.SetFieldOrProperty("_moduleGroups", moduleGroups);
            displayViewModel.GroupSelectedIndex = 1;
            // Assert
            Assert.That(displayViewModel.RightViewModuleName, Is.EqualTo("ModuleName"));
        }

        [Test]
        public void TestActiveModule()
        {
            var ddpmModuleMock = new Mock<IDdpmModule>();
            var activeModule = ddpmModuleMock.Object;
            displayViewModel.ActiveModule = activeModule;
            Assert.That(displayViewModel.ActiveModule, Is.EqualTo(activeModule));

            privateObject.SetFieldOrProperty("_activeModule", activeModule);
            displayViewModel.ActiveModule = activeModule;
            Assert.That(displayViewModel.ActiveModule, Is.EqualTo(activeModule));

            privateObject.SetFieldOrProperty("_activeModule", activeModule);
            ddpmModuleMock = new Mock<IDdpmModule>();
            displayViewModel.ActiveModule = ddpmModuleMock.Object;
            Assert.That(displayViewModel.ActiveModule, Is.EqualTo(ddpmModuleMock.Object));
        }

        [Test]
        public void TestLeftFrameWidth()
        {
            displayViewModel.LeftFrameWidth = 1.1;
            Assert.That(displayViewModel.LeftFrameWidth, Is.EqualTo(1.1));
        }

        [Test]
        public void TestFullView()
        {
            var fullView = new ContentControl();
            displayViewModel.FullView = fullView;
            Assert.That(displayViewModel.FullView, Is.EqualTo(fullView));
        }

        [Test]
        public void TestOpenFullView()
        {
            var con=new ContentControl();
            try
            {
                displayViewModel.OpenFullView(con);
                Assert.True(true);
                Assert.That(displayViewModel.FullView, Is.EqualTo(con));
                Assert.That(displayViewModel.FullView.Visibility, Is.EqualTo(Visibility.Visible));
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }
        }

        [Test]
        public void TestCloseFullView()
        {
            try
            {
                displayViewModel.CloseFullView();
                Assert.True(true);
                Assert.That(displayViewModel.FullView, Is.EqualTo(null));
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }
        }

        [Test]
        public void TestHandleSelectedHomeDeviceChanged()
        {
            var ddpmModuleMock = new Mock<IDdpmModule>();
            var _headers = new ObservableCollection<RightViewHeader>() { new RightViewHeader(1, "text") { DdpmModule = ddpmModuleMock.Object } };
            moduleGroup = new ModuleGroup();
            var privateObjecta = new PrivateObject(moduleGroup);
            privateObjecta.SetFieldOrProperty("_headers", _headers);
            var moduleGroups = new List<ModuleGroup>() { moduleGroup, moduleGroup };
            privateObject.SetFieldOrProperty("_moduleGroups", moduleGroups);
            var DeviceManagerSAMock = new Mock<IDeviceManagerSA>();
            privateObject.SetFieldOrProperty("_deviceMnagerSA", DeviceManagerSAMock.Object);
            var homedev=new HomeDevice() { MonitorInfo=new MonitorInfo()};
            privateObject.SetFieldOrProperty("_selectedHomeDevice", homedev);

            try
            {
                displayViewModel.HandleSelectedHomeDeviceChanged();
                Assert.True(true);
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }
        }

        [TearDown]
        public void TearDown()
        {
            moduleGroup.Dispose();
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            moduleGroup.Dispose();
        }

    }
    
}