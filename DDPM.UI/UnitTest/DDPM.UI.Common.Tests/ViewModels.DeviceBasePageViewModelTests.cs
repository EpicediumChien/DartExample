using DDPM.SA.Common;
using DDPM.UI.Common.Interfaces;
using DDPM.UI.Common.Models;
using DDPM.UI.Common.UserControls;
using DDPM.UI.Common.ViewModels;
using DDPM.UI.Common.Views;
using DDPM.UI.Interfaces;
using DDPM.UI.Module.AddHeadset_BL;
using DDPM.UI.Module.Brightness;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.UX.WPF;
using Microsoft.VisualBasic.Logging;
using Moq;
using NGA.UnitTest.PrivateObject;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using VcpCore.Common;

namespace DDPM.UI.Common.Tests
{
    [Apartment(ApartmentState.STA)]
    public class DeviceBasePageViewModelTests
    {
        private DeviceBasePageViewModel? deviceBasePageViewModel;
        private PrivateObject? privateObject;
        private Mock<ILog>? logMock;
        private ILog? log;
        private Mock<IConsole>? myConsoleMock;

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
            var myConsoleMock = new Mock<IConsole>();
            DdpmCommonHelper.MyConsole = myConsoleMock.Object;
            myConsoleMock.Setup(x => x.CreateLog(It.IsAny<string>())).Returns(log);
            deviceBasePageViewModel = new DeviceBasePageViewModel();
            privateObject = new PrivateObject(deviceBasePageViewModel);
        }

        [Test]
        public void TestConstructor_DeviceBasePageViewModel()
        {
            // Assert
            Assert.That(deviceBasePageViewModel, Is.Not.Null);
        }

        [Test]
        public void TestModuleGroups()
        {
            var dDdpmModuleMock = new Mock<IDdpmModule>();
            var _headers = new ObservableCollection<RightViewHeader>() { new RightViewHeader(1, "text") { DdpmModule = dDdpmModuleMock.Object } };
            var moduleGroup = new ModuleGroup();
            var privateObjecta = new PrivateObject(moduleGroup);
            privateObjecta.SetFieldOrProperty("_headers", _headers);
            var moduleGroups = new List<ModuleGroup>() { moduleGroup } ;
            deviceBasePageViewModel.ModuleGroups= moduleGroups;

            // Assert
            Assert.That(deviceBasePageViewModel.ModuleGroups, Is.EqualTo(moduleGroups));

        }

        [Test]
        public void TestGroupSelectedIndex()
        {
            var dDdpmModuleMock = new Mock<IDdpmModule>();
            var _headers = new ObservableCollection<RightViewHeader>() { new RightViewHeader(1, "text") { DdpmModule = dDdpmModuleMock.Object } };
            var moduleGroup = new ModuleGroup();
            var privateObjecta = new PrivateObject(moduleGroup);
            privateObjecta.SetFieldOrProperty("_headers", _headers);
            var moduleGroups = new List<ModuleGroup>() { moduleGroup,moduleGroup };
            deviceBasePageViewModel.ModuleGroups = moduleGroups;
            deviceBasePageViewModel.GroupSelectedIndex = -1;
            // Assert
            Assert.That(deviceBasePageViewModel.GroupSelectedIndex, Is.EqualTo(-1));

            deviceBasePageViewModel.GroupSelectedIndex = 5;
            // Assert
            Assert.That(deviceBasePageViewModel.GroupSelectedIndex, Is.EqualTo(5));

            var _vbarItems = new List<VbarItem1>() { new VbarItem1(), new VbarItem1() };
            privateObject.SetFieldOrProperty("_vbarItems", _vbarItems);
            deviceBasePageViewModel.GroupSelectedIndex = 1;
            // Assert
            Assert.That(deviceBasePageViewModel.GroupSelectedIndex, Is.EqualTo(1));

        }

        [Test]
        public void TestSetGroupCount()
        {
            var dDdpmModuleMock = new Mock<IDdpmModule>();
            var _headers = new ObservableCollection<RightViewHeader>() { new RightViewHeader(1, "text") { DdpmModule = dDdpmModuleMock.Object } };
            var moduleGroup = new ModuleGroup();
            var privateObjecta = new PrivateObject(moduleGroup);
            privateObjecta.SetFieldOrProperty("_headers", _headers);
            var moduleGroups = new List<ModuleGroup>() { moduleGroup };
            deviceBasePageViewModel.ModuleGroups = moduleGroups;
            // Assert
            Assert.That(deviceBasePageViewModel.GroupCount, Is.EqualTo(1));      
        }

        [Test]
        public void TestSelectedGroup()
        {
            var dDdpmModuleMock = new Mock<IDdpmModule>();
            var _headers = new ObservableCollection<RightViewHeader>() { new RightViewHeader(1, "text") { DdpmModule = dDdpmModuleMock.Object } };
            var moduleGroup = new ModuleGroup();
            var privateObjecta = new PrivateObject(moduleGroup);
            privateObjecta.SetFieldOrProperty("_headers", _headers);
            var moduleGroups = new List<ModuleGroup>();
            deviceBasePageViewModel.ModuleGroups = moduleGroups;
            // Assert
            Assert.That(deviceBasePageViewModel.SelectedGroup, Is.EqualTo(null));

            moduleGroups = new List<ModuleGroup>() { moduleGroup,moduleGroup};
            deviceBasePageViewModel.ModuleGroups = moduleGroups;
            var _vbarItems = new List<VbarItem1>() { new VbarItem1(), new VbarItem1() };
            privateObject.SetFieldOrProperty("_vbarItems", _vbarItems);
            deviceBasePageViewModel.GroupSelectedIndex = 1;
            // Assert
            Assert.That(deviceBasePageViewModel.SelectedGroup, Is.Not.Null);

            deviceBasePageViewModel.GroupSelectedIndex = 3;
            moduleGroups = new List<ModuleGroup>() { moduleGroup };
            deviceBasePageViewModel.ModuleGroups = moduleGroups;
            // Assert
            Assert.That(deviceBasePageViewModel.SelectedGroup, Is.EqualTo(null));
        }

        [Test]
        public void TestIsLandingMode()
        {
            deviceBasePageViewModel.GroupSelectedIndex = -1;
            Assert.That(deviceBasePageViewModel.IsLandingMode, Is.EqualTo(true));

            deviceBasePageViewModel.GroupSelectedIndex = 5;
            Assert.That(deviceBasePageViewModel.IsLandingMode, Is.EqualTo(false));
        }

        [Test]
        public void TestFindGroupIndexByGroupName()
        {
            var result = deviceBasePageViewModel.FindGroupIndexByGroupName("groupName");
            Assert.That(result, Is.EqualTo(-1));

            var dDdpmModuleMock = new Mock<IDdpmModule>();
            var _headers = new ObservableCollection<RightViewHeader>() { new RightViewHeader(1, "text") { DdpmModule = dDdpmModuleMock.Object } };
            var moduleGroup = new ModuleGroup() { GroupName = "groupName" };
            var privateObjecta = new PrivateObject(moduleGroup);
            privateObjecta.SetFieldOrProperty("_headers", _headers);
            var moduleGroups = new List<ModuleGroup>() {new ModuleGroup(),new ModuleGroup(), moduleGroup };
            deviceBasePageViewModel.ModuleGroups = moduleGroups;
            result = deviceBasePageViewModel.FindGroupIndexByGroupName("groupName");
            Assert.That(result, Is.EqualTo(2));
        }

        [Test]
        public void TestVbarItems()
        {
            var result = deviceBasePageViewModel.VbarItems;
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void TestRightViewHeaders()
        {
            deviceBasePageViewModel.RightViewHeaderChanged += new RoutedEventHandler(newRoutedEventArgs);
            deviceBasePageViewModel.RightViewHeaders = new ObservableCollection<RightViewHeader>();
            Assert.That(deviceBasePageViewModel.RightViewHeaders, Is.Not.Null);
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
            //SelectedGroup==null
            var dDdpmModuleMock = new Mock<IDdpmModule>();
            var _headers = new ObservableCollection<RightViewHeader>() { new RightViewHeader(1, "text") { DdpmModule = dDdpmModuleMock.Object } };
            var moduleGroup = new ModuleGroup();
            var privateObjecta = new PrivateObject(moduleGroup);
            privateObjecta.SetFieldOrProperty("_headers", _headers);
            var moduleGroups = new List<ModuleGroup>();
            deviceBasePageViewModel.ModuleGroups = moduleGroups;
            deviceBasePageViewModel.RightViewHeaderSelectedIndex = 1;
            // Assert
            Assert.That(deviceBasePageViewModel.RightViewHeaderSelectedIndex, Is.EqualTo(0));

            //SelectedGroup!=null
            moduleGroups = new List<ModuleGroup>() { moduleGroup, moduleGroup };
            deviceBasePageViewModel.ModuleGroups = moduleGroups;
            var _vbarItems = new List<VbarItem1>() { new VbarItem1(), new VbarItem1() };
            privateObject.SetFieldOrProperty("_vbarItems", _vbarItems);
            deviceBasePageViewModel.GroupSelectedIndex = 1;
            deviceBasePageViewModel.RightViewHeaderSelectedIndex = 1;
            // Assert
            Assert.That(deviceBasePageViewModel.RightViewHeaderSelectedIndex, Is.EqualTo(1));

        }

        [Test]
        public void TestSelRightViewHeader()
        {
            Assert.That(deviceBasePageViewModel.SelRightViewHeader, Is.EqualTo(null));

            //SelectedGroup != null
            var dDdpmModuleMock = new Mock<IDdpmModule>();
            var _headers = new ObservableCollection<RightViewHeader>() { new RightViewHeader(1, "text") { DdpmModule = dDdpmModuleMock.Object } };
            var moduleGroup = new ModuleGroup();
            var privateObjecta = new PrivateObject(moduleGroup);
            privateObjecta.SetFieldOrProperty("_headers", _headers);    
            var moduleGroups = new List<ModuleGroup>() { moduleGroup, moduleGroup };
            deviceBasePageViewModel.ModuleGroups = moduleGroups;
            var _vbarItems = new List<VbarItem1>() { new VbarItem1(), new VbarItem1() };
            privateObject.SetFieldOrProperty("_vbarItems", _vbarItems);
            deviceBasePageViewModel.GroupSelectedIndex = 1;
            deviceBasePageViewModel.RightViewHeaderSelectedIndex = 0;
            // Assert
            Assert.That(deviceBasePageViewModel.SelRightViewHeader, Is.Not.Null);
        }

        [Test]
        public void TestDefaultLeftView()
        {
            deviceBasePageViewModel.DefaultLeftView = new System.Windows.Controls.UserControl();
            Assert.That(deviceBasePageViewModel.DefaultLeftView, Is.Not.Null);
        }

        [Test]
        public void TestLeftView()
        {
            deviceBasePageViewModel.DefaultLeftView = new System.Windows.Controls.UserControl();
            Assert.That(deviceBasePageViewModel.LeftView, Is.EqualTo(deviceBasePageViewModel.DefaultLeftView));

            //SelectedGroup != null
            var dDdpmModuleMock = new Mock<IDdpmModule>();
            dDdpmModuleMock.Setup(x=>x.GetLeftView()).Returns(deviceBasePageViewModel.DefaultLeftView);
            var _headers = new ObservableCollection<RightViewHeader>() { new RightViewHeader(1, "text") { DdpmModule = dDdpmModuleMock.Object } };
            var moduleGroup = new ModuleGroup();
            var privateObjecta = new PrivateObject(moduleGroup);
            privateObjecta.SetFieldOrProperty("_headers", _headers);
            var moduleGroups = new List<ModuleGroup>() { moduleGroup, moduleGroup };
            deviceBasePageViewModel.ModuleGroups = moduleGroups;
            var _vbarItems = new List<VbarItem1>() { new VbarItem1(), new VbarItem1() };
            privateObject.SetFieldOrProperty("_vbarItems", _vbarItems);
            deviceBasePageViewModel.GroupSelectedIndex = 1;
            deviceBasePageViewModel.RightViewHeaderSelectedIndex = 0;
            // Assert
            Assert.That(deviceBasePageViewModel.LeftView, Is.EqualTo(deviceBasePageViewModel.DefaultLeftView));
        }

        [Test]
        public void TestRightView()
        {
            deviceBasePageViewModel.DefaultLeftView = new System.Windows.Controls.UserControl();
            Assert.That(deviceBasePageViewModel.RightView, Is.EqualTo(null));

            //SelectedGroup != null
            var dDdpmModuleMock = new Mock<IDdpmModule>();
            dDdpmModuleMock.Setup(x => x.GetRightView()).Returns(deviceBasePageViewModel.DefaultLeftView);
            var _headers = new ObservableCollection<RightViewHeader>() { new RightViewHeader(1, "text") { DdpmModule = dDdpmModuleMock.Object } };
            var moduleGroup = new ModuleGroup();
            var privateObjecta = new PrivateObject(moduleGroup);
            privateObjecta.SetFieldOrProperty("_headers", _headers);
            var moduleGroups = new List<ModuleGroup>() { moduleGroup, moduleGroup };
            deviceBasePageViewModel.ModuleGroups = moduleGroups;
            var _vbarItems = new List<VbarItem1>() { new VbarItem1(), new VbarItem1() };
            privateObject.SetFieldOrProperty("_vbarItems", _vbarItems);
            deviceBasePageViewModel.GroupSelectedIndex = 1;
            deviceBasePageViewModel.RightViewHeaderSelectedIndex = 0;
            // Assert
            Assert.That(deviceBasePageViewModel.RightView, Is.EqualTo(deviceBasePageViewModel.DefaultLeftView));

            var ModuleOwnerMock = new Mock<IModuleOwner>();
            DdpmCommonHelper.ModuleOwner=ModuleOwnerMock.Object;
            ModuleOwnerMock.Setup(x=>x.SelectedHomeDevice).Returns(new HomeDevice());
            var DeviceManagerSAMock=new Mock<IDeviceManagerSA>();
            DdpmCommonHelper.DeviceManagerSA=DeviceManagerSAMock.Object;
            DeviceManagerSAMock.Setup(x => x.GetHDRStatus(It.IsAny<MonitorInfo>())).Returns(Task.FromResult(true));
            ModuleOwnerMock.Setup(x => x.HomeDevices).Returns(new List<HomeDevice>() { new HomeDevice(), new HomeDevice() });  
            _headers = new ObservableCollection<RightViewHeader>() { new RightViewHeader(1, "text") {ModuleType=typeof(BrightnessModule) } };
            moduleGroup = new ModuleGroup();
            privateObjecta = new PrivateObject(moduleGroup);
            privateObjecta.SetFieldOrProperty("_headers", _headers);
            moduleGroups = new List<ModuleGroup>() { moduleGroup, moduleGroup };
            deviceBasePageViewModel.ModuleGroups = moduleGroups;
            _vbarItems = new List<VbarItem1>() { new VbarItem1(), new VbarItem1() };
            privateObject.SetFieldOrProperty("_vbarItems", _vbarItems);
            deviceBasePageViewModel.GroupSelectedIndex = 1;
            deviceBasePageViewModel.RightViewHeaderSelectedIndex = 0;
            // Assert
            Assert.That(deviceBasePageViewModel.RightView, Is.Not.Null);
        }

        [Test]
        public void TestRightViewModuleName()
        {
            Assert.That(deviceBasePageViewModel.RightViewModuleName, Is.EqualTo("(ERROR)"));

            //SelectedGroup != null
            var dDdpmModuleMock = new Mock<IDdpmModule>();
            dDdpmModuleMock.Setup(x => x.ModuleName).Returns("ModuleName");
            var _headers = new ObservableCollection<RightViewHeader>() { new RightViewHeader(1, "text") { DdpmModule = dDdpmModuleMock.Object } };
            var moduleGroup = new ModuleGroup();
            var privateObjecta = new PrivateObject(moduleGroup);
            privateObjecta.SetFieldOrProperty("_headers", _headers);
            var moduleGroups = new List<ModuleGroup>() { moduleGroup, moduleGroup };
            deviceBasePageViewModel.ModuleGroups = moduleGroups;
            var _vbarItems = new List<VbarItem1>() { new VbarItem1(), new VbarItem1() };
            privateObject.SetFieldOrProperty("_vbarItems", _vbarItems);
            deviceBasePageViewModel.GroupSelectedIndex = 1;
            deviceBasePageViewModel.RightViewHeaderSelectedIndex = 0;
            // Assert
            Assert.That(deviceBasePageViewModel.RightViewModuleName, Is.EqualTo("ModuleName"));
        }


        [Test]
        public void TestHomeDevices()
        {
            var homeDevices = new List<HomeDevice>();
            deviceBasePageViewModel.HomeDevices= homeDevices;
            Assert.That(deviceBasePageViewModel.HomeDevices, Is.EqualTo(homeDevices));
        }

        [Test]
        public void TestSelectedHomeDevice()
        {
            var selectedHomeDevice = new HomeDevice();
            deviceBasePageViewModel.SelectedHomeDevice = selectedHomeDevice;
            Assert.That(deviceBasePageViewModel.SelectedHomeDevice, Is.EqualTo(selectedHomeDevice));

            privateObject.SetFieldOrProperty("_selectedHomeDevice", new HomeDevice() { MonitorInfo=new MonitorInfo()});
            deviceBasePageViewModel.SelectedHomeDevice = selectedHomeDevice;
            Assert.That(deviceBasePageViewModel.SelectedHomeDevice, Is.EqualTo(selectedHomeDevice));
        }

        [Test]
        public void TestHomeDeviceCount()
        {
            Assert.That(deviceBasePageViewModel.HomeDeviceCount, Is.EqualTo(0));

            deviceBasePageViewModel.HomeDevices = new List<HomeDevice>() { new HomeDevice(),new HomeDevice()};
            Assert.That(deviceBasePageViewModel.HomeDeviceCount, Is.EqualTo(2));
        }

        [Test]
        public void TestHomeDevicesComboBoxVisibility()
        {
            Assert.That(deviceBasePageViewModel.HomeDevicesComboBoxVisibility, Is.EqualTo(Visibility.Collapsed));

            deviceBasePageViewModel.HomeDevices = new List<HomeDevice>() { new HomeDevice(), new HomeDevice() };
            Assert.That(deviceBasePageViewModel.HomeDevicesComboBoxVisibility, Is.EqualTo(Visibility.Visible));
        }

        [Test]
        public void TestSelectedHomeDeviceTextVisibiliity()
        {
            Assert.That(deviceBasePageViewModel.SelectedHomeDeviceTextVisibiliity, Is.EqualTo(Visibility.Collapsed));

            deviceBasePageViewModel.HomeDevices = new List<HomeDevice>() { new HomeDevice() };
            Assert.That(deviceBasePageViewModel.SelectedHomeDeviceTextVisibiliity, Is.EqualTo(Visibility.Visible));
        }

        [Test]
        public void TestLeftFrameWidth()
        {
            deviceBasePageViewModel.LeftFrameWidth = 2;
            Assert.That(deviceBasePageViewModel.LeftFrameWidth, Is.EqualTo(2));
        }

        [Test]
        public void TestFullView()
        {
            var fullView = new ContentControl();
            deviceBasePageViewModel.FullView = fullView;
            Assert.That(deviceBasePageViewModel.FullView, Is.EqualTo(fullView));
        }

        [Test]
        public void TestOpenFullView()
        {
            var content = new ContentControl();
            try
            {
                deviceBasePageViewModel.OpenFullView(content);
                Assert.True(true);
                Assert.That(deviceBasePageViewModel.FullView, Is.EqualTo(content));
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
                deviceBasePageViewModel.CloseFullView();
                Assert.True(true);
                Assert.That(deviceBasePageViewModel.FullView, Is.EqualTo(null));
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }
        }

        [Test]
        public void TestHandleSelectedHomeDeviceChanged()
        {
            //SelectedHomeDeviceChanged != null
            deviceBasePageViewModel.SelectedHomeDeviceChanged += new EventHandler(newEventHandler);
            var ModuleOwnerMock = new Mock<IModuleOwner>();
            DdpmCommonHelper.ModuleOwner = ModuleOwnerMock.Object;
            ModuleOwnerMock.Setup(x => x.SelectedHomeDevice).Returns(new HomeDevice());
            var DeviceManagerSAMock = new Mock<IDeviceManagerSA>();
            DdpmCommonHelper.DeviceManagerSA = DeviceManagerSAMock.Object;
            DdpmCommonHelper.ModuleOwner.SelectedHomeDevice.MonitorInfo = new MonitorInfo();
            var ddpmModuleMock=new Mock<IDdpmModule>();
            var _headers = new ObservableCollection<RightViewHeader>() { new RightViewHeader(1, "text") { DdpmModule=ddpmModuleMock.Object} };
            var moduleGroup = new ModuleGroup();
            var privateObjecta = new PrivateObject(moduleGroup);
            privateObjecta.SetFieldOrProperty("_headers", _headers);
            var moduleGroups = new List<ModuleGroup>() { moduleGroup, moduleGroup };
            deviceBasePageViewModel.ModuleGroups = moduleGroups;
            try
            {
                deviceBasePageViewModel.HandleSelectedHomeDeviceChanged();
                Assert.True(true);
                Assert.That(j, Is.EqualTo(2));
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }
        }

        int j = 0;
        private void newEventHandler(object? sender, EventArgs e)
        {
            j = 2;
        }

        [Test]
        public void TestActiveModule()
        {
            var ddpmModuleMock=new Mock<IDdpmModule>();
            var activeModule = ddpmModuleMock.Object;
            deviceBasePageViewModel.ActiveModule = activeModule;
            Assert.That(deviceBasePageViewModel.ActiveModule, Is.EqualTo(activeModule));

            privateObject.SetFieldOrProperty("_activeModule", activeModule);
            deviceBasePageViewModel.ActiveModule = activeModule;
            Assert.That(deviceBasePageViewModel.ActiveModule, Is.EqualTo(activeModule));

            privateObject.SetFieldOrProperty("_activeModule", activeModule);
            ddpmModuleMock = new Mock<IDdpmModule>();
            deviceBasePageViewModel.ActiveModule = ddpmModuleMock.Object;
            Assert.That(deviceBasePageViewModel.ActiveModule, Is.EqualTo(ddpmModuleMock.Object));
        }


        [Test]
        public void TestRefreshGroupManagerUIByModuleCapabilities()
        {
            //SelectedHomeDevice == null
            try
            {
                deviceBasePageViewModel.RefreshGroupManagerUIByModuleCapabilities();
                Assert.True(true);
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }

            //SelectedHomeDevice != null
            deviceBasePageViewModel.SelectedHomeDevice = new HomeDevice() { MonitorInfo = new MonitorInfo() { CapabilityDic = new Dictionary<string, List<string>>() { {"JE",new List<string>() {"E9" } } } } };
            var ddpmModuleMock = new Mock<IDdpmModule>();
            ddpmModuleMock.Setup(x => x.ModuleName).Returns("PipPbpModule");

            var _headers = new ObservableCollection<RightViewHeader>() {new RightViewHeader(0,"texta"), new RightViewHeader(1, "textb") { DdpmModule = ddpmModuleMock.Object } };
            _headers[0].ModuleType = typeof(HomeDevice);
            _headers[1].ModuleType = typeof(HomeDevice);
            var moduleGroup = new ModuleGroup() { HeaderSelectedIndex = 1 ,GroupName= "KVM" ,VbarText= "KVMVbarText" };            
            var privateObjecta = new PrivateObject(moduleGroup);
            privateObjecta.SetFieldOrProperty("_headers", _headers);
            var moduleGroups = new List<ModuleGroup>() { new ModuleGroup() { HeaderSelectedIndex = 1, GroupName = "Gaming", VbarText = "GamingVbarText" }, moduleGroup };
            var _vbarItems = new List<VbarItem1>() { new VbarItem1() { Text = "GamingVbarText" }, new VbarItem1() {Text= "KVMVbarText" } };
            privateObject.SetFieldOrProperty("_vbarItems", _vbarItems);
            privateObject.SetFieldOrProperty("_moduleGroups", moduleGroups);
            deviceBasePageViewModel.GroupSelectedIndex = 1;
            //deviceBasePageViewModel.ModuleGroups = ;
            try
            {
                deviceBasePageViewModel.RefreshGroupManagerUIByModuleCapabilities();
                Assert.True(true);
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }

            deviceBasePageViewModel.RightViewHeaderChanged += new RoutedEventHandler(newRoutedEventArgs);
            try
            {
                deviceBasePageViewModel.RefreshGroupManagerUIByModuleCapabilities();
                Assert.True(true);
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }
        }

        [Test]
        public void TestHandleDdcCiOffEvent()
        {
            var _vbarItems = new List<VbarItem1>() { new VbarItem1() { Text = "GamingVbarText",Visibility= Visibility.Collapsed }, new VbarItem1() { Text = "KVMVbarText" } };
            privateObject.SetFieldOrProperty("_vbarItems", _vbarItems);
            var moduleGroups = new List<ModuleGroup>() { new ModuleGroup() { HeaderSelectedIndex = 1, GroupName = "EasyArrange", VbarText = "EasyArrangeText" } };
            privateObject.SetFieldOrProperty("_moduleGroups", moduleGroups);
            deviceBasePageViewModel.GroupSelectedIndex = 1;
            try
            {
                deviceBasePageViewModel.HandleDdcCiOffEvent(false);
                Assert.True(true);
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }

            _vbarItems = new List<VbarItem1>() { new VbarItem1() { Text = "GamingVbarText", Visibility = Visibility.Collapsed, IsLocked = true }, new VbarItem1() { Text = "KVMVbarText" } };
            privateObject.SetFieldOrProperty("_vbarItems", _vbarItems);
            moduleGroups = new List<ModuleGroup>() { new ModuleGroup() { HeaderSelectedIndex = 1, GroupName = "EasyArrange", VbarText = "EasyArrangeText" } };
            privateObject.SetFieldOrProperty("_moduleGroups", moduleGroups);
            try
            {
                deviceBasePageViewModel.HandleDdcCiOffEvent(false);
                Assert.True(true);
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }

            try
            {
                deviceBasePageViewModel.HandleDdcCiOffEvent(true);
                Assert.True(true);
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }
        }



        [Test]
        public void TestInitLog()
        {
            var myConsoleMock = new Mock<IConsole>();
            DdpmCommonHelper.MyConsole=myConsoleMock.Object;
            try
            {
                deviceBasePageViewModel.InitLog();
                Assert.True(true);
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }
        }

        [Test]
        public void TestLogInfo()
        {
            var _logMock = new Mock<ILog>();
            privateObject.SetFieldOrProperty("_log", _logMock.Object);
            try
            {
                deviceBasePageViewModel.LogInfo("msg");
                Assert.True(true);
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }
        }

        [Test]
        public void TestGotoHomepage()
        {
            var myConsoleMock=new Mock<IConsole>();
            DdpmCommonHelper.MyConsole= myConsoleMock.Object;
            try
            {
                deviceBasePageViewModel.GotoHomepage();
                Assert.True(true);
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }
        }


    }
}
