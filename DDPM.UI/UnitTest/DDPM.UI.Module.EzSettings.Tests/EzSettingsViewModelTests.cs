using DDPM.SA.Common;
using DDPM.UI.Common.Interfaces;
using DDPM.UI.Common.Models;
using DDPM.UI.Common;
using Moq;
using NGA.UnitTest.PrivateObject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Dell.Client.Framework.UX.WPF.ResourceManager;
using VcpCore.Common;
using DDPM.SA.Common.Display;
using DDPM.SA.Common.Settings;
using Dell.Client.Framework.Common;
using System.Globalization;

namespace DDPM.UI.Module.EzSettings.Tests
{
    [TestFixture, Apartment(ApartmentState.STA)]
    public class EzSettingsViewModelTests
    {
        private Mock<IModuleOwner>? moduleOwnerMock;
        private IModuleOwner? moduleOwner;
        private Mock<IDeviceManagerSA>? deviceManagerMock;
        private IDeviceManagerSA? deviceManagerSA;
        private HomeDevice? selectedHomeDevice;
        private EzSettingsViewModel? eSettingsViewModel;
        private PrivateObject? privateObject;
        //private GamingModule? myModule;

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
            moduleOwnerMock.Setup(m => m.SelectedHomeDevice).Returns(new HomeDevice() { MonitorInfo=new MonitorInfo()});
            moduleOwner = moduleOwnerMock!.Object;
            DdpmCommonHelper.ModuleOwner = moduleOwner;

            deviceManagerMock = new Mock<IDeviceManagerSA>();
            deviceManagerSA = deviceManagerMock.Object;
            deviceManagerMock.Setup(x => x.ReadCurrentHotkey(It.IsAny<MonitorInfo>())).Returns(Task.FromResult(new ValueTuple<HotkeySettings, List<HotkeyData>>(new HotkeySettings() { HotkeyInfo=new List<HotkeyInfo>() {new HotkeyInfo(), new HotkeyInfo() { Job= HotkeyType.ToggleEzRecentSetting ,Hotkey=new List<Windows.System.VirtualKey>()} } },new List<HotkeyData>())));
            deviceManagerMock.Setup(x => x.ReadEzSettings()).Returns(Task.FromResult(new SA.Common.Settings.EzSettings() { IsWidthoutGap = true, IsOnlyAllowWhenShiftKeyPressed = true, IsSpanAcrossMultiMonitors = true, IsAwsEnabled = true}));
            deviceManagerMock.Setup(x => x.GetIsSpanEnabled()).Returns(Task.FromResult(true));
            DdpmCommonHelper.DeviceManagerSA = deviceManagerSA;
            eSettingsViewModel = new EzSettingsViewModel(moduleOwner);
            privateObject = new PrivateObject(eSettingsViewModel);
            privateObject.SetFieldOrProperty("_deviceManagerSA", deviceManagerMock.Object);
        }

        [Test]
        public void TestConstructor_EzSettingsViewModel()
        {
            HomeDevice.DeviceManagerSA = null;
            eSettingsViewModel = new EzSettingsViewModel(moduleOwner);
            Assert.NotNull(eSettingsViewModel);
            Assert.That(privateObject.GetFieldOrProperty("_deviceManagerSA"), Is.EqualTo(deviceManagerSA));
        }

        [Test]
        public void TestModuleOwner()
        {
            // Arrange
            eSettingsViewModel.ModuleOwner = moduleOwner;
            Assert.That(eSettingsViewModel.ModuleOwner, Is.EqualTo(moduleOwner));
        }

        [Test]
        public void TestRefreshSettingsa()
        {
            if (CultureInfo.CurrentCulture.Name == "es-US")
            {
                eSettingsViewModel.RefreshSettings();
                Thread.Sleep(500);
                Assert.That(eSettingsViewModel.RecentHotkey, Is.EqualTo("None"));
                Assert.That(eSettingsViewModel.IsSpanAcrossEnabled, Is.EqualTo(true));
                Assert.That(eSettingsViewModel.IsWithoutGap, Is.EqualTo(true));
                Assert.That(eSettingsViewModel.IsOnlyAllowWhenShiftKeyPressed, Is.EqualTo(true));
                Assert.That(eSettingsViewModel.IsSpanAcrossMultiMonitors, Is.EqualTo(true));
                Assert.That(eSettingsViewModel.IsAwsEnabled, Is.EqualTo(true));
                Assert.That(eSettingsViewModel.IsBusy, Is.EqualTo(false));
            }
        }

        [Test]
        public void TestRefreshSettingsb()
        {
            if (CultureInfo.CurrentCulture.Name == "es-US")
            {
                DdpmCommonHelper.DeviceManagerSA = null;
                eSettingsViewModel.RefreshSettings();
                Thread.Sleep(500);
                Assert.That(eSettingsViewModel.RecentHotkey, Is.EqualTo("None"));
                Assert.That(eSettingsViewModel.IsSpanAcrossEnabled, Is.EqualTo(false));
                Assert.That(eSettingsViewModel.IsWithoutGap, Is.EqualTo(true));
                Assert.That(eSettingsViewModel.IsOnlyAllowWhenShiftKeyPressed, Is.EqualTo(false));
                Assert.That(eSettingsViewModel.IsSpanAcrossMultiMonitors, Is.EqualTo(false));
                Assert.That(eSettingsViewModel.IsAwsEnabled, Is.EqualTo(false));
                Assert.That(eSettingsViewModel.IsBusy, Is.EqualTo(false));
            }
        }

        [Test]
        public void TestRecentHotkey()
        {
            eSettingsViewModel.RecentHotkey = "RecentHotkey";

            Assert.That(eSettingsViewModel.RecentHotkey, Is.EqualTo("RecentHotkey"));
        }

        [Test]
        public void TestIsWithoutGapa()
        {
            deviceManagerMock.Setup(x => x.WriteEzSettings_IsWidthoutGap(It.IsAny<bool>())).Returns(Task.FromResult(true));
            eSettingsViewModel.IsWithoutGap=true;
            Thread.Sleep(500);
            Assert.That(eSettingsViewModel.IsWithoutGap, Is.EqualTo(true));
            Assert.That(eSettingsViewModel.IsBusy, Is.EqualTo(false));
        }

        [Test]
        public void TestIsWithoutGapb()
        {
            DdpmCommonHelper.DeviceManagerSA = null;
            //Thread.Sleep(500);
            eSettingsViewModel.IsWithoutGap = false;
            Assert.That(eSettingsViewModel.IsWithoutGap, Is.EqualTo(false));
            Assert.That(eSettingsViewModel.IsBusy, Is.EqualTo(true));
        }

        [Test]
        public void TestIsWithoutGap_StringON()
        {
            if (CultureInfo.CurrentCulture.Name == "es-US")
            {
                eSettingsViewModel.IsWithoutGap = true;

                Assert.That(eSettingsViewModel.IsWithoutGap_String, Is.EqualTo("ON"));
            }
        }

        [Test]
        public void TestIsWithoutGap_StringOFF()
        {
            if (CultureInfo.CurrentCulture.Name == "es-US")
            {
                eSettingsViewModel.IsWithoutGap = false;

                Assert.That(eSettingsViewModel.IsWithoutGap_String, Is.EqualTo("OFF"));
            }
        }


        [Test]
        public void TestIsOnlyAllowWhenShiftKeyPresseda()
        {
            deviceManagerMock.Setup(x => x.WriteEzSettings_IsOnlyAllowWhenShiftKeyPressed(It.IsAny<bool>())).Returns(Task.FromResult(true));
            eSettingsViewModel.IsOnlyAllowWhenShiftKeyPressed = true;
            Thread.Sleep(500);
            Assert.That(eSettingsViewModel.IsOnlyAllowWhenShiftKeyPressed, Is.EqualTo(true));
            Assert.That(eSettingsViewModel.IsBusy, Is.EqualTo(false));
        }

        [Test]
        public void TestIsOnlyAllowWhenShiftKeyPressedb()
        {
            DdpmCommonHelper.DeviceManagerSA = null;
            eSettingsViewModel.IsOnlyAllowWhenShiftKeyPressed = false;
            Assert.That(eSettingsViewModel.IsOnlyAllowWhenShiftKeyPressed, Is.EqualTo(false));
            Assert.That(eSettingsViewModel.IsBusy, Is.EqualTo(true));
        }

        [Test]
        public void TestIsOnlyAllowWhenShiftKeyPressed_StringON()
        {
            if (CultureInfo.CurrentCulture.Name == "es-US")
            {
                eSettingsViewModel.IsOnlyAllowWhenShiftKeyPressed = true;

                Assert.That(eSettingsViewModel.IsOnlyAllowWhenShiftKeyPressed_String, Is.EqualTo("ON"));
            }
        }

        [Test]
        public void TestIsOnlyAllowWhenShiftKeyPressed_StringOFF()
        {
            if (CultureInfo.CurrentCulture.Name == "es-US")
            {
                eSettingsViewModel.IsOnlyAllowWhenShiftKeyPressed = false;

                Assert.That(eSettingsViewModel.IsOnlyAllowWhenShiftKeyPressed_String, Is.EqualTo("OFF"));
            }
        }

        [Test]
        public void TestIsSpanAcrossMultiMonitorsa()
        {
            deviceManagerMock.Setup(x => x.WriteEzSettings_IsSpanAcrossMultiMonitors(It.IsAny<bool>())).Returns(Task.FromResult(true));
            eSettingsViewModel.IsSpanAcrossMultiMonitors = true;
            Thread.Sleep(500);
            Assert.That(eSettingsViewModel.IsSpanAcrossMultiMonitors, Is.EqualTo(true));
            Assert.That(eSettingsViewModel.IsBusy, Is.EqualTo(false));
        }

        [Test]
        public void TestIsSpanAcrossMultiMonitorsb()
        {
            DdpmCommonHelper.DeviceManagerSA = null;
            eSettingsViewModel.IsSpanAcrossMultiMonitors = false;
            Assert.That(eSettingsViewModel.IsSpanAcrossMultiMonitors, Is.EqualTo(false));
            Assert.That(eSettingsViewModel.IsBusy, Is.EqualTo(true));
        }

        [Test]
        public void TestIsSpanAcrossMultiMonitors_StringON()
        {
            if (CultureInfo.CurrentCulture.Name == "es-US")
            {
                eSettingsViewModel.IsSpanAcrossMultiMonitors = true;

                Assert.That(eSettingsViewModel.IsSpanAcrossMultiMonitors_String, Is.EqualTo("ON"));
            }
        }

        [Test]
        public void TestIsSpanAcrossMultiMonitors_StringOFF()
        {
            if (CultureInfo.CurrentCulture.Name == "es-US")
            {
                eSettingsViewModel.IsSpanAcrossMultiMonitors = false;

                Assert.That(eSettingsViewModel.IsSpanAcrossMultiMonitors_String, Is.EqualTo("OFF"));
            }
        }

        [Test]
        public void TestIsSpanAcrossEnabled()
        {
            eSettingsViewModel.IsSpanAcrossEnabled = true;

            Assert.That(eSettingsViewModel.IsSpanAcrossEnabled, Is.EqualTo(true));
        }

        [Test]
        public void TestIsAwsEnableda()
        {
            deviceManagerMock.Setup(x => x.WriteEzSettings_IsAwsEnabled(It.IsAny<bool>())).Returns(Task.FromResult(true));
            eSettingsViewModel.IsAwsEnabled = true;
            Thread.Sleep(500);
            Assert.That(eSettingsViewModel.IsAwsEnabled, Is.EqualTo(true));
            Assert.That(eSettingsViewModel.IsBusy, Is.EqualTo(false));
        }

        [Test]
        public void TestIsAwsEnabledb()
        {
            DdpmCommonHelper.DeviceManagerSA = null;
            eSettingsViewModel.IsAwsEnabled = false;
            Assert.That(eSettingsViewModel.IsAwsEnabled, Is.EqualTo(false));
            Assert.That(eSettingsViewModel.IsBusy, Is.EqualTo(true));
        }

        [Test]
        public void TestIsAwsEnabled_StringON()
        {
            if (CultureInfo.CurrentCulture.Name == "es-US")
            {
                eSettingsViewModel.IsAwsEnabled = true;

                Assert.That(eSettingsViewModel.IsAwsEnabled_String, Is.EqualTo("ON"));
            }
        }

        [Test]
        public void TestIsAwsEnabled_StringOFF()
        {
            if (CultureInfo.CurrentCulture.Name == "es-US")
            {
                eSettingsViewModel.IsAwsEnabled = false;

                Assert.That(eSettingsViewModel.IsAwsEnabled_String, Is.EqualTo("OFF"));
            }
        }

        [Test]
        public void TestIsBusy()
        {
            bool myIsBusy = true;
            eSettingsViewModel.IsBusy = myIsBusy;

            Assert.That(eSettingsViewModel.IsBusy, Is.EqualTo(myIsBusy));
        }

        [Test]
        public void TestLog()
        {
            var log = new Mock<ILog>();
            eSettingsViewModel.Log = log.Object;

            Assert.That(eSettingsViewModel.Log, Is.EqualTo(log.Object));
        }
        
    }
}
