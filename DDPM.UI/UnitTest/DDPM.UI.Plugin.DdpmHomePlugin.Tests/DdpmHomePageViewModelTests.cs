using DDPM.SA.Common;
using DDPM.UI.Common;
using DDPM.UI.Common.Models;
using DDPM.UI.Plugin.DdpmHomePlugin.Interfaces;
using DDPM.UI.Plugin.DdpmHomePlugin.Model;
using DDPM.UI.Plugin.DdpmHomePlugin.ViewModels;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.UX.WPF;
using Microsoft.VisualBasic.Logging;
using Moq;
using NGA.UnitTest.PrivateObject;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using VcpCore.Common;

namespace DDPM.UI.Plugin.DdpmHomePlugin.Tests
{
    [TestFixture, Apartment(ApartmentState.STA)]
    public class DdpmHomePageViewModelTests
    {
        private DdpmHomePageViewModel? ddpmHomePageViewModel;
        private Mock<IDeviceInfo>? deviceInfoMock;
        private IDeviceInfo? deviceInfo;
        private Mock<IConsole>? consoleMock;
        private IConsole? console;
        private Mock<ILog>? logMock;
        private ILog? log;
        private PrivateObject?privateObject;

        [SetUp]
        public void Setup()
        {
            deviceInfoMock = new Mock<IDeviceInfo>();
            deviceInfo = deviceInfoMock.Object;
            consoleMock = new Mock<IConsole>();
            console = consoleMock.Object;
            logMock = new Mock<ILog>();
            log = logMock.Object;
            ddpmHomePageViewModel = new DdpmHomePageViewModel(console, log);
            privateObject=new PrivateObject(ddpmHomePageViewModel);
            
        }

        [Test]
        public void TestConstructor_DdpmHomePageViewModel()
        {
            Assert.That(ddpmHomePageViewModel, Is.Not.Null);
            Assert.That(privateObject.GetFieldOrProperty("_console"),Is.Not.Null);
            Assert.That(privateObject.GetFieldOrProperty("_log"), Is.Not.Null);
            Assert.That(privateObject.GetFieldOrProperty("_connectButtonClickCommand"), Is.Not.Null);
        }

        [Test]
        public void TestLog()
        {
            Assert.That(ddpmHomePageViewModel.Log, Is.EqualTo(privateObject.GetFieldOrProperty("_log")));
        }


        [Test]
        public void TestHomeDevices()
        {
            var homeDevices = new ObservableCollection<HomeDevice>();
            ddpmHomePageViewModel.HomeDevices = homeDevices;
            Assert.That(ddpmHomePageViewModel.HomeDevices, Is.EqualTo(homeDevices));
        }


        [Test]
        public void TestSelectedHomeDevice()
        {
            var selectedHomeDevice = new HomeDevice();
            ddpmHomePageViewModel.SelectedHomeDevice = selectedHomeDevice;
            Assert.That(ddpmHomePageViewModel.SelectedHomeDevice, Is.EqualTo(selectedHomeDevice));
        }

        [Test]
        public void TestHomeDeviceCount()
        {
            var homeDevices = new ObservableCollection<HomeDevice>() {  new HomeDevice() };
            ddpmHomePageViewModel.HomeDevices = homeDevices;
            Assert.That(ddpmHomePageViewModel.HomeDeviceCount, Is.EqualTo(homeDevices.Count));
        }


        [Test]
        public void TestPrepareMonitorInfos()
        {
            List<MonitorInfo> monitorInfos=new List<MonitorInfo>();
            try
            {
                ddpmHomePageViewModel.PrepareMonitorInfos(monitorInfos);
                Assert.True(true);
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }
        }

        [Test]
        public void TestPrepareDeviceInfos()
        {
            List<DeviceInfo> deviceInfos = new List<DeviceInfo> ();
            try
            {
                ddpmHomePageViewModel.PrepareDeviceInfos(deviceInfos);
                Assert.True(true);
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }
        }

        [Test]
        public void TestResetDevices()
        {
            try
            {
                ddpmHomePageViewModel.ResetDevices();
                Assert.True(true);
                Assert.That(ddpmHomePageViewModel.HomeDevices, Is.Not.Null);
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }
        }

        [Test]
        public void TestRefreshCollectionView()
        {
            try
            {
                ddpmHomePageViewModel.RefreshCollectionView();
                Assert.True(true);
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }
        }

        [Test]
        public void TestConnectButtonClickCommand()
        {
            var ICommandMock=new Mock<ICommand>();
            var ConnectButtonClickCommand=ICommandMock.Object;
            ddpmHomePageViewModel.ConnectButtonClickCommand = ConnectButtonClickCommand;
            Assert.That(ddpmHomePageViewModel.ConnectButtonClickCommand, Is.EqualTo(ConnectButtonClickCommand));
        }

        [Test]
        public void TestRaiseShowAddDevicePlugin()
        {
            try
            {
                ddpmHomePageViewModel.RaiseShowAddDevicePlugin();
                Assert.True(true);
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }
        }

        [Test]
        public void TestAddDemoHomeDevice()
        {
            var device = new HomeDevice();
            try
            {
                ddpmHomePageViewModel.AddDemoHomeDevice(device);
                Assert.True(true);
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }
        }


        [Test]
        public void TestcxItem()
        {
            ddpmHomePageViewModel.cxItem = 1.0;
            Assert.That(ddpmHomePageViewModel.cxItem, Is.EqualTo(1.0));
        }
        

    }
}
