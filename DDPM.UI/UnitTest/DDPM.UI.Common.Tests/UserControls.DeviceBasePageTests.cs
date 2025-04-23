using DDPM.UI.Common.Models;
using DDPM.UI.Common.UserControls;
using DDPM.UI.Common.ViewModels;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.UX.WPF;
using Moq;
using NGA.UnitTest.PrivateObject;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using Dell.Client.Framework.UX.WPF.ResourceManager;
namespace DDPM.UI.Common.Tests
{
    [Apartment(ApartmentState.STA)]
    public class DeviceBasePageTests
    {
        private DeviceBasePage? deviceBasePage;
        private PrivateObject? privateObject;
        private Mock<ILog>? logMock;
        private ILog? log;
        private Mock<IConsole>? myConsoleMock;
        private ModuleGroup moduleGroup_1;
        private ModuleGroup moduleGroup_2;


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
            logMock = new Mock<ILog>();
            log = logMock.Object;
            var myConsoleMock = new Mock<IConsole>();
            DdpmCommonHelper.MyConsole = myConsoleMock.Object;
            myConsoleMock.Setup(x => x.CreateLog(It.IsAny<string>())).Returns(log);
            deviceBasePage = new DeviceBasePage();
            privateObject = new PrivateObject(deviceBasePage);
            moduleGroup_1 = moduleGroup_1 ?? new();
            moduleGroup_2 = moduleGroup_2 ?? new();
        }

        [Test]
        public void TestConstructor_DeviceBasePage()
        {
            // Assert
            Assert.That(deviceBasePage, Is.Not.Null);
            Assert.That(deviceBasePage.DataContext, Is.EqualTo(privateObject.GetFieldOrProperty("_viewModel")));

        }

        [Test]
        public void TestSetModuleGroupList()
        {
            var groupList = new List<ModuleGroup>() { new ModuleGroup() { GroupName = "GroupNameA" }, new ModuleGroup() { GroupName = "GroupNameB" } };
            try
            {
                deviceBasePage.SetModuleGroupList(groupList);
                DeviceBasePageViewModel viewModelam = (DeviceBasePageViewModel)privateObject.GetFieldOrProperty("_viewModel");
                Assert.That(deviceBasePage.DataContext, Is.EqualTo(privateObject.GetFieldOrProperty("_viewModel")));
                Assert.That(viewModelam.ModuleGroups, Is.EqualTo(groupList));
                Assert.True(true);
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }
        }

        [Test]
        public void TestSetLeftFrameWidth()
        {
            try
            {
                deviceBasePage.SetLeftFrameWidth(2.0);
                DeviceBasePageViewModel viewModelam = (DeviceBasePageViewModel)privateObject.GetFieldOrProperty("_viewModel");
                Assert.That(viewModelam.LeftFrameWidth, Is.EqualTo(2));
                Assert.True(true);
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }
        }

        [Test]
        public void TestSetHomeDevices()
        {
            var devices = new List<HomeDevice>() { new HomeDevice(), new HomeDevice() };
            try
            {
                deviceBasePage.SetHomeDevices(devices);
                DeviceBasePageViewModel viewModelam = (DeviceBasePageViewModel)privateObject.GetFieldOrProperty("_viewModel");
                Assert.That(viewModelam.HomeDevices, Is.EqualTo(devices));
                Assert.True(true);
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }
        }

        [Test]
        public void TestSetSelectedHomeDevice()
        {
            var devices = new HomeDevice();
            try
            {
                deviceBasePage.SetSelectedHomeDevice(devices);
                DeviceBasePageViewModel viewModelam = (DeviceBasePageViewModel)privateObject.GetFieldOrProperty("_viewModel");
                Assert.That(viewModelam.SelectedHomeDevice, Is.EqualTo(devices));
                Assert.True(true);
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }
        }

        [Test]
        public void TestSelectGroupByIndex()
        {
            //groupIndex < 0
            try
            {
                deviceBasePage.SelectGroupByIndex(-1);
                Assert.True(true);
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }

            try
            {
                moduleGroup_1 = new();
                moduleGroup_2 = new();
                var deviceBasePageViewModel = new DeviceBasePageViewModel() { ModuleGroups = new List<ModuleGroup>() { moduleGroup_1, moduleGroup_2 } };
                var privateobject = new PrivateObject(deviceBasePageViewModel);
                privateObject.SetFieldOrProperty("_viewModel", deviceBasePageViewModel);
                DeviceBasePageViewModel viewModelam = (DeviceBasePageViewModel)privateObject.GetFieldOrProperty("_viewModel");
                viewModelam = (DeviceBasePageViewModel)privateObject.GetFieldOrProperty("_viewModel");
                deviceBasePage.SelectGroupByIndex(1);
                Assert.That(viewModelam.GroupSelectedIndex, Is.EqualTo(1));
                Assert.True(true);
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }
        }

        [Test]
        public void TestSetDefaultLeftView()
        {
            var leftView = new System.Windows.Controls.UserControl();
            try
            {
                deviceBasePage.SetDefaultLeftView(leftView);
                DeviceBasePageViewModel viewModelam = (DeviceBasePageViewModel)privateObject.GetFieldOrProperty("_viewModel");
                Assert.That(viewModelam.DefaultLeftView, Is.EqualTo(leftView));
                Assert.That(viewModelam.DefaultLeftView.DataContext, Is.EqualTo(viewModelam.SelectedHomeDevice));
                Assert.True(true);
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }
        }

        [Test]
        public void TestSetLockModuleGroup()
        {
            var deviceBasePageViewModel = new DeviceBasePageViewModel() { ModuleGroups = new List<ModuleGroup>() { new ModuleGroup(), new ModuleGroup() } };
            var privateobject = new PrivateObject(deviceBasePageViewModel);
            privateObject.SetFieldOrProperty("_viewModel", deviceBasePageViewModel);
            var result = deviceBasePage.SetLockModuleGroup(Constants.GroupName_InputSource, true);
            Assert.That(result, Is.EqualTo(false));

            moduleGroup_1 = new ModuleGroup() { GroupName = "InputSource" };
            moduleGroup_2 = new();
            deviceBasePageViewModel = new DeviceBasePageViewModel() { ModuleGroups = new List<ModuleGroup>() { moduleGroup_1, moduleGroup_2 } };
            privateObject.SetFieldOrProperty("_viewModel", deviceBasePageViewModel);
            result = deviceBasePage.SetLockModuleGroup(Constants.GroupName_InputSource, true);
            Assert.That(result, Is.EqualTo(true));
        }


        [TearDown]
        public void TearDown()
        {
            if (deviceBasePage != null)
            {
                deviceBasePage.Dispose();
                deviceBasePage = null;
            }

            moduleGroup_1?.Dispose();
            moduleGroup_1 = null;
            moduleGroup_2?.Dispose();
            moduleGroup_2 = null;
        }



    }
}
