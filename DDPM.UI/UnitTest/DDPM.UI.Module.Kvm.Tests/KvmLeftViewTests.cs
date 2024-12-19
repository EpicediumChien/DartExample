using DDPM.SA.Common;
using DDPM.UI.Common;
using DDPM.UI.Common.Interfaces;
using DDPM.UI.Common.Models;
using Moq;
using System.Windows;
using Dell.Client.Framework.UX.WPF.ResourceManager;
using VcpCore.Common;
using DDPM.SA.Common.Display;
using DDPM.SA.Common.Settings;
using Windows.System;
using Dell.Client.Framework.UX.WPF;
using Dell.Client.Framework.Common;
using Microsoft.VisualBasic.Logging;

namespace DDPM.UI.Module.Kvm.Tests
{
    [TestFixture, Apartment(ApartmentState.STA)]
    public class KvmLeftViewTests
    {
        private KvmViewModel? kvmViewModel;
        private KvmLeftView? kvmLeftView;
        private Mock<IConsole>? MyConsoleMock;
        private Mock<ILog>? _logMock;
        private ILog _log;

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
            _logMock=new Mock<ILog>();
            _log=_logMock.Object;
            MyConsoleMock = new Mock<IConsole>();
            DdpmCommonHelper.MyConsole = MyConsoleMock.Object;
            MyConsoleMock.Setup(x => x.CreateLog(It.IsAny<string>())).Returns(_logMock.Object);
            kvmViewModel = new KvmViewModel();
            var deviceManagerMock = new Mock<IDeviceManagerSA>();
            var deviceManagerSA = deviceManagerMock.Object;
            DdpmCommonHelper.DeviceManagerSA = deviceManagerSA;
            var moduleOwnerMock = new Mock<IModuleOwner>();
            var moduleOwner = moduleOwnerMock!.Object;
            DdpmCommonHelper.ModuleOwner = moduleOwner;
            moduleOwnerMock.Setup(x => x.SelectedHomeDevice).Returns(new HomeDevice());
            DdpmCommonHelper.ModuleOwner.SelectedHomeDevice.MonitorInfo=new VcpCore.Common.MonitorInfo();
            DdpmCommonHelper.ModuleOwner.SelectedHomeDevice.MonitorInfo.CapabilityDic = new Dictionary<string, List<string>>() { { "AA", new List<string>() }, { "EE", new List<string>() } };
            deviceManagerMock.Setup(x => x.ReadCurrentHotkey(It.IsAny<MonitorInfo>())).Returns(Task.FromResult(new ValueTuple<HotkeySettings, List<HotkeyData>>(new HotkeySettings() {HotkeyInfo=new List<HotkeyInfo>() { new HotkeyInfo() {Hotkey= new List<VirtualKey>() { VirtualKey.Q,VirtualKey.M} } } },new List<HotkeyData>()) ));
            KvmModule kvmModule = new KvmModule(moduleOwner);
            kvmViewModel.KvmModule = kvmModule;
            kvmViewModel.KvmModule.SelectedHomeDevice = new HomeDevice();
            kvmViewModel.KvmModule.SelectedHomeDevice.MonitorInfo= new VcpCore.Common.MonitorInfo();
            //kvmLeftView = new KvmLeftView(kvmViewModel);
        }
        //Elsa mark for _log readonly
        //[Test]
        //public void TestConstructor_kvmLeftView()
        //{
        //    Assert.That(kvmLeftView, Is.Not.Null);
        //    Assert.That(kvmLeftView.DataContext, Is.EqualTo(kvmViewModel));
        //}
    }
}