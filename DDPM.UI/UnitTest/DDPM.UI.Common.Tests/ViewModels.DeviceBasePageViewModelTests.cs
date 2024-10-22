using DDPM.UI.Common.Models;
using DDPM.UI.Common.UserControls;
using DDPM.UI.Common.ViewModels;
using DDPM.UI.Interfaces;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.UX.WPF;
using Moq;
using NGA.UnitTest.PrivateObject;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;

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
            var _headers = new ObservableCollection<RightViewHeader>() { new RightViewHeader(1,"text") { DdpmModule= dDdpmModuleMock.Object }  };
            var moduleGroup = new ModuleGroup();
            var privateObjecta = new PrivateObject(moduleGroup);
            privateObjecta.SetFieldOrProperty("_headers", _headers);
            var moduleGroups = new List<ModuleGroup>() { moduleGroup} };
            deviceBasePageViewModel.ModuleGroups= moduleGroups;

            // Assert
            Assert.That(deviceBasePageViewModel.ModuleGroups, Is.EqualTo(moduleGroups));

        }

        //[Test]
        //public void TestSetLeftFrameWidth()
        //{
        //    try
        //    {
        //        deviceBasePageViewModel.SetLeftFrameWidth(2.0);
        //        DeviceBasePageViewModel viewModelam = (DeviceBasePageViewModel)privateObject.GetFieldOrProperty("viewModel");
        //        Assert.That(viewModelam.LeftFrameWidth, Is.EqualTo(2));
        //        Assert.True(true);
        //    }
        //    catch (Exception ex)
        //    {
        //        Assert.Fail("not invoked");
        //    }
        //}

        //[Test]
        //public void TestSetHomeDevices()
        //{
        //    var devices = new List<HomeDevice>() { new HomeDevice(), new HomeDevice() };
        //    try
        //    {
        //        deviceBasePageViewModel.SetHomeDevices(devices);
        //        DeviceBasePageViewModel viewModelam = (DeviceBasePageViewModel)privateObject.GetFieldOrProperty("viewModel");
        //        Assert.That(viewModelam.HomeDevices, Is.EqualTo(devices));
        //        Assert.True(true);
        //    }
        //    catch (Exception ex)
        //    {
        //        Assert.Fail("not invoked");
        //    }
        //}

        //[Test]
        //public void TestSetSelectedHomeDevice()
        //{
        //    var devices = new HomeDevice();
        //    try
        //    {
        //        deviceBasePageViewModel.SetSelectedHomeDevice(devices);
        //        DeviceBasePageViewModel viewModelam = (DeviceBasePageViewModel)privateObject.GetFieldOrProperty("viewModel");
        //        Assert.That(viewModelam.SelectedHomeDevice, Is.EqualTo(devices));
        //        Assert.True(true);
        //    }
        //    catch (Exception ex)
        //    {
        //        Assert.Fail("not invoked");
        //    }
        //}

        //[Test]
        //public void TestSelectGroupByIndex()
        //{
        //    //groupIndex < 0
        //    try
        //    {
        //        deviceBasePageViewModel.SelectGroupByIndex(-1);
        //        Assert.True(true);
        //    }
        //    catch (Exception ex)
        //    {
        //        Assert.Fail("not invoked");
        //    }

        //    try
        //    {
        //        var deviceBasePageViewModel = new DeviceBasePageViewModel() { ModuleGroups = new List<ModuleGroup>() { new ModuleGroup(), new ModuleGroup() } };
        //        var privateobject = new PrivateObject(deviceBasePageViewModel);
        //        privateObject.SetFieldOrProperty("viewModel", deviceBasePageViewModel);
        //        DeviceBasePageViewModel viewModelam = (DeviceBasePageViewModel)privateObject.GetFieldOrProperty("viewModel");
        //        viewModelam = (DeviceBasePageViewModel)privateObject.GetFieldOrProperty("viewModel");
        //        deviceBasePageViewModel.SelectGroupByIndex(1);
        //        Assert.That(viewModelam.GroupSelectedIndex, Is.EqualTo(1));
        //        Assert.True(true);
        //    }
        //    catch (Exception ex)
        //    {
        //        Assert.Fail("not invoked");
        //    }
        //}

        //[Test]
        //public void TestSetDefaultLeftView()
        //{
        //    var leftView = new System.Windows.Controls.UserControl();
        //    try
        //    {
        //        deviceBasePageViewModel.SetDefaultLeftView(leftView);
        //        DeviceBasePageViewModel viewModelam = (DeviceBasePageViewModel)privateObject.GetFieldOrProperty("viewModel");
        //        Assert.That(viewModelam.DefaultLeftView, Is.EqualTo(leftView));
        //        Assert.That(viewModelam.DefaultLeftView.DataContext, Is.EqualTo(viewModelam.SelectedHomeDevice));
        //        Assert.True(true);
        //    }
        //    catch (Exception ex)
        //    {
        //        Assert.Fail("not invoked");
        //    }
        //}

        //[Test]
        //public void TestSetLockModuleGroup()
        //{
        //    var deviceBasePageViewModel = new DeviceBasePageViewModel() { ModuleGroups = new List<ModuleGroup>() { new ModuleGroup() , new ModuleGroup() } };
        //    var privateobject = new PrivateObject(deviceBasePageViewModel);
        //    privateObject.SetFieldOrProperty("viewModel", deviceBasePageViewModel);
        //    var result = deviceBasePageViewModel.SetLockModuleGroup(Constants.GroupName_InputSource, true);
        //    Assert.That(result, Is.EqualTo(false));

        //    deviceBasePageViewModel = new DeviceBasePageViewModel() { ModuleGroups = new List<ModuleGroup>() { new ModuleGroup() { GroupName = "InputSource" }, new ModuleGroup() } };
        //    privateObject.SetFieldOrProperty("viewModel", deviceBasePageViewModel);
        //    result = deviceBasePageViewModel.SetLockModuleGroup(Constants.GroupName_InputSource, true);
        //    Assert.That(result, Is.EqualTo(true));
        //}

    }
}
