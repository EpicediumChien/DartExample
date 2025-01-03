using DDPM.SA.Common;
using DDPM.UI.Common;
using DDPM.UI.Common.Interfaces;
using DDPM.UI.Common.Models;
using Moq;
using System.Windows;
using Dell.Client.Framework.UX.WPF.ResourceManager;
using DDPM.SA.Common.Display;
using DDPM.SA.Common.Settings;
using VcpCore.Common;
using Windows.System;
using Dell.Client.Framework.UX.WPF;
using Dell.Client.Framework.Common;

namespace DDPM.UI.Module.Kvm.Tests
{
    [TestFixture, Apartment(ApartmentState.STA)]
    public class KvmRightViewTests
    {
        private Mock<IDeviceManagerSA>? deviceManagerMock;
        private IDeviceManagerSA? deviceManagerSA;
        private IModuleOwner? moduleOwner;
        private Mock<IModuleOwner>? moduleOwnerMock;
        private KvmViewModel? kvmViewModel;
        private KvmRightView? kvmRightView;
        private Mock<IConsole>? MyConsoleMock;
        private Mock<ILog>? logMock;
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
            DdpmCommonHelper.MyConsole = MyConsoleMock.Object;
            logMock=new Mock<ILog>();
            MyConsoleMock.Setup(x => x.CreateLog(It.IsAny<string>())).Returns(logMock.Object);
            kvmViewModel = new KvmViewModel();
            deviceManagerMock = new Mock<IDeviceManagerSA>();
            deviceManagerSA = deviceManagerMock.Object;
            DdpmCommonHelper.DeviceManagerSA = deviceManagerSA;
            moduleOwnerMock = new Mock<IModuleOwner>();
            moduleOwner = moduleOwnerMock!.Object;
            DdpmCommonHelper.ModuleOwner = moduleOwner;
            moduleOwnerMock.Setup(x => x.SelectedHomeDevice).Returns(new HomeDevice());
            //DdpmCommonHelper.ModuleOwner.SelectedHomeDevice.MonitorInfo = new VcpCore.Common.MonitorInfo();
            //DdpmCommonHelper.ModuleOwner.SelectedHomeDevice.MonitorInfo.CapabilityDic = new Dictionary<string, List<string>>() { { "AA", new List<string>() }, { "EE", new List<string>() } };
            //deviceManagerMock.Setup(x => x.ReadCurrentHotkey(It.IsAny<MonitorInfo>())).Returns(Task.FromResult(new ValueTuple<HotkeySettings, List<HotkeyData>>(new HotkeySettings() { HotkeyInfo = new List<HotkeyInfo>() { new HotkeyInfo() { Hotkey = new List<VirtualKey>() { VirtualKey.Q, VirtualKey.M } } } }, new List<HotkeyData>())));
            KvmModule kvmModule = new KvmModule(moduleOwner);
            kvmViewModel.KvmModule = kvmModule;
            kvmViewModel.KvmModule.SelectedHomeDevice = new HomeDevice();
            kvmViewModel.KvmModule.SelectedHomeDevice.MonitorInfo = new VcpCore.Common.MonitorInfo();

            kvmRightView = new KvmRightView(kvmViewModel);
        }

        [Test]
        public void TestConstructor_KvmRightView()
        {
            Assert.That(kvmRightView, Is.Not.Null);
            Assert.That(kvmRightView.DataContext, Is.EqualTo(kvmViewModel));
        }
    }
}