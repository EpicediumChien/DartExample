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
    public class DockPageViewModelTests
    {
        private DockPageViewModel? dockPageViewModel;
        private PrivateObject? privateObject;
        private Mock<IModuleOwner>? moduleOwnerMock;
        private IModuleOwner? moduleOwner;
        private IConsole? console;
        private Mock<IConsole>? consoleMock;
        private IDeviceManagerSA? deviceManagerSA;
        private Mock<IDeviceManagerSA>? deviceManagerSAMock;
        private ILog? log;
        private Mock<ILog>? logMock;
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
            deviceManagerSAMock = new Mock<IDeviceManagerSA>();
            deviceManagerSA = deviceManagerSAMock.Object;
            logMock = new Mock<ILog>();
            log = logMock.Object;
            dockPageViewModel = new DockPageViewModel(console, log, deviceManagerSA);
            privateObject = new PrivateObject(dockPageViewModel);
        }

        [Test]
        public void TestConstructor_DockPageViewModel()
        {
            // Assert
            Assert.That(dockPageViewModel, Is.Not.Null);
        }

        [Test]
        public void TestTabOffClickedCommand()
        {
            // Assert
            Assert.That(dockPageViewModel.TabOffClickedCommand, Is.EqualTo(null));
        }

        [Test]
        public void TestTabAdaptiveLightClickedCommand()
        {
            // Assert
            Assert.That(dockPageViewModel.TabAdaptiveLightClickedCommand, Is.EqualTo(null));
        }

        [Test]
        public void TestTabManualClickedCommand()
        {
            // Assert
            Assert.That(dockPageViewModel.TabManualClickedCommand, Is.EqualTo(null));
        }

        [Test]
        public void TestIsEnableUpdate()
        {
            // Assert
            Assert.That(dockPageViewModel.IsEnableUpdate, Is.EqualTo(false));
        }
        
        [Test]
        public void TestOnPropertyChanged()
        {
            try
            {
                dockPageViewModel.OnPropertyChanged("");
                Assert.True(true);
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }
        }

        [Test]
        public void TestPrepareDeviceInfo()
        {
            var deviceInfos = new List<DeviceInfo>() { new DeviceInfo() { LogicalDeviceType = "LogicalMouse" },new DeviceInfo() { LogicalDeviceType = "Dock",ID=new Guid() } }; 
            try
            {
                dockPageViewModel.PrepareDeviceInfo(deviceInfos);
                Assert.True(true);
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }
        }

        [Test]
        public void TestSetCurrentDevice()
        {
            string guid=  "3c6863f9-d8d6-4045-9403-8c3ace7df488";
            var result = dockPageViewModel.SetCurrentDevice(guid);
            // Assert
            Assert.That(result, Is.EqualTo(false));

            deviceManagerSAMock.Setup(x => x.GetFWUpdateInfo(It.IsAny<bool>())).Returns(Task.FromResult(new FWUpdateInfoPackage() {FWUpdateInfo=new List<FWUpdateInfo>() { new FWUpdateInfo() { DeviceId=""},new FWUpdateInfo() { DeviceId= "{3c6863f9-d8d6-4045-9403-8c3ace7df488}" } } }));
            privateObject.SetFieldOrProperty("_deviceManager", deviceManagerSAMock.Object);           
            dockPageViewModel.DeviceInfos = new Dictionary<Guid, DeviceInfo>() { { new Guid(), new DeviceInfo() }, { new Guid(guid), new DeviceInfo() { ModelNumber = "KB740",Name= "HEADSET", FirmwareVersion = "AA", PhysicalDeviceType = DeviceType.PhysicalWebcam, PhysicalDeviceFirmwareVersion="A0" , DockPackageFwVersion = "DockPackageFwVersion", DockServiceTag = "DockServiceTag" } } };
            result = dockPageViewModel.SetCurrentDevice("3c6863f9-d8d6-4045-9403-8c3ace7df488");
            Assert.That(result, Is.EqualTo(true));
        }

        [Test]
        public void TestHandleNotification()
        {
            var changeType = new DeviceChangedType();
            var di = new DeviceInfo() { ID = Guid.NewGuid(),Name= "HEADSET" };
            try
            {
                dockPageViewModel.HandleNotification(DeviceChangedType.Peripherals_SettingsChange, di, "");
                Assert.True(true);
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }

            string guid = "3c6863f9-d8d6-4045-9403-8c3ace7df488";
            var peripheralViewModel = new PeripheralViewModel(console, log, deviceManagerSA);
            dockPageViewModel.DeviceInfos = new Dictionary<Guid, DeviceInfo>() { { di.ID, di }, { new Guid(), di } };
            dockPageViewModel.CurrentDeviceID = di.ID;
            try
            {
                dockPageViewModel.HandleNotification(DeviceChangedType.Peripherals_SettingsChange, di, "");
                Assert.True(true);
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
                //}
            }

        }
    }
    
}