using DDPM.SA.Common;
using DDPM.UI.Common;
using DDPM.UI.Common.Interfaces;
using DDPM.UI.Common.Models;
using Moq;
using NGA.UnitTest.PrivateObject;
using System.Windows;
using System.Windows.Controls;
using VcpCore.Common;
using Dell.Client.Framework.UX.WPF.ResourceManager;
using DDPM.SA.Common.Display;
using DDPM.SA.Common.Settings;
using Windows.System;
using Dell.Client.Framework.UX.WPF;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.UX.WPF.ResourceManager;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Assert = NUnit.Framework.Assert;
namespace DDPM.UI.Module.Kvm.Tests
{
    [Apartment(ApartmentState.STA)]
    public class KvmModuleTests
    {
        private KvmModule? kvmModule;
        private PrivateObject? privateObject;
        private Mock<IModuleOwner>? moduleOwnerMock;
        private IModuleOwner? moduleOwner;
        private Mock<IDeviceManagerSA>? deviceManagerMock;
        private IDeviceManagerSA? deviceManagerSA;
        private KvmViewModel? kvmViewModel;
        private KvmRightView? kvmRightView;
        private Mock<IConsole>? MyConsoleMock;
        private Mock<ILog>? MyLogMock;

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
            MyConsoleMock = new Mock<IConsole>();
            MyLogMock=new Mock<ILog>();
            MyConsoleMock.Setup(x => x.CreateLog(It.IsAny<string>())).Returns(MyLogMock.Object);
            DdpmCommonHelper.MyConsole = MyConsoleMock.Object;
            kvmViewModel = new KvmViewModel();
            deviceManagerMock = new Mock<IDeviceManagerSA>();
            deviceManagerSA = deviceManagerMock.Object;
            DdpmCommonHelper.DeviceManagerSA = deviceManagerSA;
            moduleOwnerMock = new Mock<IModuleOwner>();
            moduleOwner = moduleOwnerMock!.Object;
            DdpmCommonHelper.ModuleOwner = moduleOwner;
            moduleOwnerMock.Setup(x => x.SelectedHomeDevice).Returns(new HomeDevice());
            DdpmCommonHelper.ModuleOwner.SelectedHomeDevice.MonitorInfo = new VcpCore.Common.MonitorInfo();
            DdpmCommonHelper.ModuleOwner.SelectedHomeDevice.MonitorInfo.CapabilityDic = new Dictionary<string, List<string>>() { { "AA", new List<string>() }, { "EE", new List<string>() } };
            deviceManagerMock.Setup(x => x.ReadCurrentHotkey(It.IsAny<MonitorInfo>())).Returns(Task.FromResult(new ValueTuple<HotkeySettings, List<HotkeyData>>(new HotkeySettings() { HotkeyInfo = new List<HotkeyInfo>() { new HotkeyInfo() { Hotkey = new List<VirtualKey>() { VirtualKey.Q, VirtualKey.M } } } }, new List<HotkeyData>())));
            kvmModule = new KvmModule(moduleOwner);
            kvmViewModel.KvmModule = kvmModule;
            kvmViewModel.KvmModule.SelectedHomeDevice = new HomeDevice();
            kvmViewModel.KvmModule.SelectedHomeDevice.MonitorInfo = new VcpCore.Common.MonitorInfo();
            deviceManagerMock.Setup(x => x.GetOnUSBKVM(It.IsAny<MonitorInfo>())).Returns(Task.FromResult(true));
            kvmModule = new KvmModule(moduleOwner);
            privateObject = new PrivateObject(kvmModule);
        }

        [Test]
        public void TestModuleName()
        {
            // Act
            var result = kvmModule!.ModuleName;

            // Assert
            Assert.That(result, Is.EqualTo("KvmModule"));
        }

        [Test]
        public void TestGetLeftView()
        {
            // Act
            var result = kvmModule!.GetLeftView();

            // Assert
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void TestGetRightView()
        {
            // Act
            var result = kvmModule!.GetRightView();

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<UserControl>());
        }

        [Test]
        public void TestSelectedHomeDevice()
        {
            // Arrange
            var homeDevice = new HomeDevice();

            // Act
            kvmModule!.SelectedHomeDevice = homeDevice;
            var result = kvmModule.SelectedHomeDevice;

            // Assert
            Assert.That(result, Is.EqualTo(homeDevice));
        }

        [Test]
        public void TestModuleOwner()
        {
            // Arrange
            var moduleOwner = moduleOwnerMock!.Object;
            kvmModule!.ModuleOwner = moduleOwner;

            Assert.That(kvmModule.ModuleOwner, Is.EqualTo(moduleOwner));
        }

        [Test]
        public void TestViewModelInitialization()
        {
            // Act
            var viewModel = privateObject!.GetFieldOrProperty("vm");

            // Assert
            Assert.That(viewModel, Is.Not.Null);
        }

        [Test]
        public void TestOnSelectedHomeDeviceChanged()
        {
            try
            {
                kvmModule.OnSelectedHomeDeviceChanged();
                Assert.True(true);
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }
        }

        [Test]
        public void TestOnActivated()
        {
            try
            {
                kvmModule.OnActivated();
                Assert.True(true);
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }
        }

        [Test]
        public void TestOnDeactivated()
        {
            try
            {
                kvmModule.OnDeactivated();
                Assert.True(true);
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }
        }
    }
}