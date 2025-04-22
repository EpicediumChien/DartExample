using DDPM.SA.Common;
using DDPM.UI.Common;
using DDPM.UI.Common.Interfaces;
using DDPM.UI.Common.Models;
using Dell.Client.Framework.UX.WPF;
using Dell.Client.Framework.UX.WPF.ResourceManager;
using Moq;
using NGA.UnitTest.PrivateObject;
using NUnit.Framework;
using System.Globalization;
using System.Windows;
using VcpCore.Common;

namespace DDPM.UI.Module.Brightness.Tests
{
    [Apartment(ApartmentState.STA)]
    public class BrightnessViewModelTests
    {
        private BrightnessViewModel? brightnessViewModel;
        private PrivateObject? privateObject;
        private Mock<IModuleOwner>? moduleOwnerMock;
        private Mock<IDeviceManagerSA>? deviceManagerMock;
        private Mock<IConsole>? myConsoleMock;
        private IConsole? myConsole;
        private BrightnessModule? brightnessModule;

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
            if (!UriParser.IsKnownScheme("pack"))
            {
                new System.Windows.Application();
            }
            moduleOwnerMock = new Mock<IModuleOwner>();
            var moduleOwner = moduleOwnerMock!.Object;
            DdpmCommonHelper.ModuleOwner = moduleOwnerMock.Object;
            moduleOwnerMock.Setup(x => x.SelectedHomeDevice).Returns(new HomeDevice() { MonitorInfo = new MonitorInfo() });
            brightnessModule = new BrightnessModule();
            myConsoleMock = new Mock<IConsole>();
            myConsole = myConsoleMock.Object;
            DdpmCommonHelper.MyConsole = myConsole;
            brightnessViewModel = new BrightnessViewModel();
            brightnessViewModel.MyModule = brightnessModule;
        }

        [Test]
        public void TestMyModule()
        {
            Assert.That(brightnessViewModel.MyModule, Is.EqualTo(brightnessModule));
        }

        [Test]
        public void TestBrightnessImage()
        {
            var brightnessViewModel = new BrightnessViewModel();
            var myBrightnessViewModel = DdpmCommonHelper.GetImageSourceFromCommonResource("Resources/Product_KB900.png");
            brightnessViewModel.BrightnessImage = myBrightnessViewModel;
            Assert.That(brightnessViewModel.BrightnessImage, Is.EqualTo(myBrightnessViewModel));
        }

        [Test]
        public void TestContrastImage()
        {
            var brightnessViewModel = new BrightnessViewModel();
            var myBrightnessViewModel = DdpmCommonHelper.GetImageSourceFromCommonResource("Resources/Product_KB900.png");
            brightnessViewModel.ContrastImage = myBrightnessViewModel;
            Assert.That(brightnessViewModel.ContrastImage, Is.EqualTo(myBrightnessViewModel));
        }

        [Test]
        public void TestLuminanceImage()
        {
            var brightnessViewModel = new BrightnessViewModel();
            var myBrightnessViewModel = DdpmCommonHelper.GetImageSourceFromCommonResource("Resources/Product_KB900.png");
            brightnessViewModel.LuminanceImage = myBrightnessViewModel;
            Assert.That(brightnessViewModel.LuminanceImage, Is.EqualTo(myBrightnessViewModel));
        }

        [Test]
        public void TestIsSynchronize_String()
        {
            if (CultureInfo.CurrentCulture.Name == "es-US")
            {
                moduleOwnerMock.Setup(x => x.SelectedHomeDevice).Returns(new HomeDevice());

                var brightnessViewModel = new BrightnessViewModel();
                brightnessViewModel.IsSynchronize = true;
                var myBrightnessViewModel = brightnessViewModel.IsSynchronize_String;
                Assert.That(myBrightnessViewModel, Is.EqualTo("ON"));

                brightnessViewModel.IsSynchronize = false;
                myBrightnessViewModel = brightnessViewModel.IsSynchronize_String;
                Assert.That(myBrightnessViewModel, Is.EqualTo("OFF"));
            }
        }

        [Test]
        public void TestBrightnessEnable()
        {
            var brightnessViewModel = new BrightnessViewModel();
            Assert.That(brightnessViewModel.BrightnessEnable, Is.EqualTo(false));

            privateObject = new PrivateObject(brightnessViewModel);
            privateObject.SetFieldOrProperty("IsBrightnessEnable", true);
            Assert.That(brightnessViewModel.BrightnessEnable, Is.EqualTo(true));
        }

        [Test]
        public void TestBrightnessOpacity()
        {
            var brightnessViewModel = new BrightnessViewModel();
            Assert.That(brightnessViewModel.BrightnessOpacity, Is.EqualTo("0.5"));

            privateObject = new PrivateObject(brightnessViewModel);
            privateObject.SetFieldOrProperty("IsBrightnessEnable", true);
            Assert.That(brightnessViewModel.BrightnessOpacity, Is.EqualTo("1.0"));
        }

        [Test]
        public void TestGreayoutAlart()
        {
            var brightnessViewModel = new BrightnessViewModel();
            Assert.That(brightnessViewModel.GreayoutAlart, Is.EqualTo(Visibility.Visible));

            privateObject = new PrivateObject(brightnessViewModel);
            privateObject.SetFieldOrProperty("IsBrightnessEnable", true);
            Assert.That(brightnessViewModel.GreayoutAlart, Is.EqualTo(Visibility.Collapsed));
        }

        [Test]
        public void TestBrightnessMinsKey()
        {
            moduleOwnerMock.Setup(x => x.SelectedHomeDevice).Returns(new HomeDevice());

            var brightnessViewModel = new BrightnessViewModel();
            var myBrightnessViewModel = "BrightnessMinsKey";
            brightnessViewModel.BrightnessMinsKey = myBrightnessViewModel;
            Assert.That(brightnessViewModel.BrightnessMinsKey, Is.EqualTo(myBrightnessViewModel));
        }

        [Test]
        public void TestBrightnessAddKey()
        {
            moduleOwnerMock.Setup(x => x.SelectedHomeDevice).Returns(new HomeDevice());

            var brightnessViewModel = new BrightnessViewModel();
            var myBrightnessViewModel = "BrightnessAddKey";
            brightnessViewModel.BrightnessAddKey = myBrightnessViewModel;
            Assert.That(brightnessViewModel.BrightnessAddKey, Is.EqualTo(myBrightnessViewModel));
        }

        [Test]
        public void TestContrastMinsKey()
        {
            moduleOwnerMock.Setup(x => x.SelectedHomeDevice).Returns(new HomeDevice());

            var brightnessViewModel = new BrightnessViewModel();
            var myBrightnessViewModel = "ContrastMinsKey";
            brightnessViewModel.ContrastMinsKey = myBrightnessViewModel;
            Assert.That(brightnessViewModel.ContrastMinsKey, Is.EqualTo(myBrightnessViewModel));
        }

        [Test]
        public void TestContrastAddKey()
        {
            moduleOwnerMock.Setup(x => x.SelectedHomeDevice).Returns(new HomeDevice());

            var brightnessViewModel = new BrightnessViewModel();
            var myBrightnessViewModel = "ContrastAddKey";
            brightnessViewModel.ContrastAddKey = myBrightnessViewModel;
            Assert.That(brightnessViewModel.ContrastAddKey, Is.EqualTo(myBrightnessViewModel));
        }

        [Test]
        public void TestLuminanceMinsKey()
        {
            moduleOwnerMock.Setup(x => x.SelectedHomeDevice).Returns(new HomeDevice());

            var brightnessViewModel = new BrightnessViewModel();
            var myBrightnessViewModel = "LuminanceMinsKey";
            brightnessViewModel.LuminanceMinsKey = myBrightnessViewModel;
            Assert.That(brightnessViewModel.LuminanceMinsKey, Is.EqualTo(myBrightnessViewModel));
        }

        [Test]
        public void TestLuminanceAddKey()
        {
            moduleOwnerMock.Setup(x => x.SelectedHomeDevice).Returns(new HomeDevice());

            var brightnessViewModel = new BrightnessViewModel();
            var myBrightnessViewModel = "LuminanceAddKey";
            brightnessViewModel.LuminanceAddKey = myBrightnessViewModel;
            Assert.That(brightnessViewModel.LuminanceAddKey, Is.EqualTo(myBrightnessViewModel));
        }

        [Test]
        public void TestBrightnessViewModel()
        {
            moduleOwnerMock.Setup(x => x.SelectedHomeDevice).Returns(new HomeDevice());

            var brightnessViewModel = new BrightnessViewModel();
            Assert.That(brightnessViewModel, Is.Not.Null);
        }

        [Test]
        public void TestIsSynchronize()
        {
            moduleOwnerMock.Setup(x => x.SelectedHomeDevice).Returns(new HomeDevice());

            var brightnessViewModel = new BrightnessViewModel();
            var myIsSynchronize = true;
            brightnessViewModel.IsSynchronize = myIsSynchronize;
            Assert.That(brightnessViewModel.IsSynchronize, Is.EqualTo(myIsSynchronize));
        }

        [Test]
        public void TestBrightnessValue()
        {
            moduleOwnerMock = new Mock<IModuleOwner>();
            var moduleOwner = moduleOwnerMock!.Object;
            DdpmCommonHelper.ModuleOwner = moduleOwner;
            var deviceManagerMock = new Mock<IDeviceManagerSA>();
            var deviceManagerSA = deviceManagerMock.Object;
            DdpmCommonHelper.DeviceManagerSA = deviceManagerSA;
            var selectedHomeDevice = new HomeDevice();

            var brightnessViewModel = new BrightnessViewModel();
            moduleOwnerMock.Setup(x => x.SelectedHomeDevice).Returns(new HomeDevice());
            brightnessViewModel.ModuleOwner = moduleOwner;
            var monitorInfo = new MonitorInfo();
            brightnessViewModel.ModuleOwner.SelectedHomeDevice.MonitorInfo = monitorInfo;

            var myBrightnessValue = 2;
            brightnessViewModel.BrightnessValue = myBrightnessValue;
            Assert.That(brightnessViewModel.BrightnessValue, Is.EqualTo(myBrightnessValue));
        }

        [Test]
        public void TestContrastValue()
        {
            moduleOwnerMock = new Mock<IModuleOwner>();
            var moduleOwner = moduleOwnerMock!.Object;
            DdpmCommonHelper.ModuleOwner = moduleOwner;
            var deviceManagerMock = new Mock<IDeviceManagerSA>();
            var deviceManagerSA = deviceManagerMock.Object;
            DdpmCommonHelper.DeviceManagerSA = deviceManagerSA;
            var selectedHomeDevice = new HomeDevice();

            var brightnessViewModel = new BrightnessViewModel();
            moduleOwnerMock.Setup(x => x.SelectedHomeDevice).Returns(new HomeDevice());
            brightnessViewModel.ModuleOwner = moduleOwner;
            var monitorInfo = new MonitorInfo();
            brightnessViewModel.ModuleOwner.SelectedHomeDevice.MonitorInfo = monitorInfo;
            //value < 25
            var myContrastValue = 2;
            brightnessViewModel.ContrastValue = myContrastValue;
            Assert.That(brightnessViewModel.ContrastValue, Is.EqualTo(25));
            //value >25
            myContrastValue = 28;
            brightnessViewModel.ContrastValue = myContrastValue;
            Assert.That(brightnessViewModel.ContrastValue, Is.EqualTo(myContrastValue));
        }

        [Test]
        public void TestLuminanceValue()
        {
            moduleOwnerMock = new Mock<IModuleOwner>();
            var moduleOwner = moduleOwnerMock!.Object;
            DdpmCommonHelper.ModuleOwner = moduleOwner;
            var deviceManagerMock = new Mock<IDeviceManagerSA>();
            var deviceManagerSA = deviceManagerMock.Object;
            DdpmCommonHelper.DeviceManagerSA = deviceManagerSA;
            var selectedHomeDevice = new HomeDevice();

            var brightnessViewModel = new BrightnessViewModel();
            moduleOwnerMock.Setup(x => x.SelectedHomeDevice).Returns(new HomeDevice());
            brightnessViewModel.ModuleOwner = moduleOwner;
            var monitorInfo = new MonitorInfo();
            brightnessViewModel.ModuleOwner.SelectedHomeDevice.MonitorInfo = monitorInfo;

            var myBrightnessValue = 2;
            brightnessViewModel.LuminanceValue = myBrightnessValue;
            Assert.That(brightnessViewModel.LuminanceValue, Is.EqualTo(myBrightnessValue));
        }

        [Test]
        public void TestLuminanceMaxValue()
        {
            var brightnessViewModel = new BrightnessViewModel();
            Assert.That(brightnessViewModel.LuminanceMaxValue, Is.Not.Null);
        }

        [Test]
        public void TestUpdateBrightnessContrast()
        {
            var brightnessViewModel = new BrightnessViewModel();
            brightnessViewModel.SelectedHomeDevice = new HomeDevice();
            var deviceManagerMock = new Mock<IDeviceManagerSA>();
            var deviceManagerSA = deviceManagerMock.Object;
            DdpmCommonHelper.DeviceManagerSA = deviceManagerSA;
            var monitorInfo = new MonitorInfo();
            brightnessViewModel.SelectedHomeDevice.MonitorInfo = monitorInfo;
            deviceManagerMock.Setup(x => x.GetVCPCapability(It.IsAny<MonitorInfo>(), It.IsAny<byte>(), It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<Priority>())).Returns(Task.FromResult(new ObjGetVCP() { result = true, value = Convert.ToInt64(6) }));

            brightnessViewModel.UpdateBrightnessContrast();
            var privateObject = new PrivateObject(brightnessViewModel);

            Assert.That(privateObject.GetFieldOrProperty("BrightnessValue"), Is.EqualTo(6));
            Assert.That(privateObject.GetFieldOrProperty("ContrastValue"), Is.EqualTo(6));
        }

        [Test]
        public void TestUpdateLuminance()
        {
            var brightnessViewModel = new BrightnessViewModel();
            brightnessViewModel.SelectedHomeDevice = new HomeDevice();
            var deviceManagerMock = new Mock<IDeviceManagerSA>();
            var deviceManagerSA = deviceManagerMock.Object;
            DdpmCommonHelper.DeviceManagerSA = deviceManagerSA;
            var monitorInfo = new MonitorInfo();
            brightnessViewModel.SelectedHomeDevice.MonitorInfo = monitorInfo;
            deviceManagerMock.Setup(x => x.GetVCPCapability(It.IsAny<MonitorInfo>(), It.IsAny<byte>(), It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<Priority>())).Returns(Task.FromResult(new ObjGetVCP() { result = true, value = Convert.ToInt64(10) }));

            brightnessViewModel.UpdateLuminance();
            var privateObject = new PrivateObject(brightnessViewModel);

            Assert.That(privateObject.GetFieldOrProperty("LuminanceValue"), Is.EqualTo(10));
            Assert.That(privateObject.GetFieldOrProperty("LuminanceMax_Value"), Is.EqualTo(10));
        }

        [Test]
        public void TestModuleOwner()
        {
            var mockModuleOwner = new Mock<IModuleOwner>();
            var brightnessViewModel = new BrightnessViewModel();
            brightnessViewModel.ModuleOwner = mockModuleOwner.Object;

            Assert.That(brightnessViewModel.ModuleOwner, Is.EqualTo(mockModuleOwner.Object));
        }

        [Test]
        public void TestExpanderGroup()
        {
            var brightnessViewModel = new BrightnessViewModel();
            var myExpanderGroup = 2;
            brightnessViewModel.ExpanderGroup = myExpanderGroup;

            Assert.That(brightnessViewModel.ExpanderGroup, Is.EqualTo(myExpanderGroup));
        }

        [Test]
        public void TestisScheduleSupport()
        {
            var brightnessViewModel = new BrightnessViewModel();
            var myBrightnessViewModel = new Visibility();
            brightnessViewModel.isScheduleSupport = myBrightnessViewModel;
            Assert.That(brightnessViewModel.isScheduleSupport, Is.EqualTo(myBrightnessViewModel));
        }

        [Test]
        public void TestisNormalBrightness()
        {
            var brightnessViewModel = new BrightnessViewModel();
            Visibility myBrightnessViewModel = Visibility.Visible;
            brightnessViewModel.isNormalBrightness = myBrightnessViewModel;
            Assert.That(brightnessViewModel.isNormalBrightness, Is.EqualTo(myBrightnessViewModel));

            myBrightnessViewModel = Visibility.Hidden;
            brightnessViewModel.isNormalBrightness = myBrightnessViewModel;
            Assert.That(brightnessViewModel.isNormalBrightness, Is.EqualTo(myBrightnessViewModel));

            myBrightnessViewModel = Visibility.Collapsed;
            brightnessViewModel.isNormalBrightness = myBrightnessViewModel;
            Assert.That(brightnessViewModel.isNormalBrightness, Is.EqualTo(myBrightnessViewModel));
        }

        [Test]
        public void TestisLuminanceSupport()
        {
            var brightnessViewModel = new BrightnessViewModel();
            Visibility myBrightnessViewModel = Visibility.Visible;
            brightnessViewModel.isLuminanceSupport = myBrightnessViewModel;
            Assert.That(brightnessViewModel.isLuminanceSupport, Is.EqualTo(myBrightnessViewModel));

            myBrightnessViewModel = Visibility.Hidden;
            brightnessViewModel.isLuminanceSupport = myBrightnessViewModel;
            Assert.That(brightnessViewModel.isLuminanceSupport, Is.EqualTo((Visibility)2));

            myBrightnessViewModel = Visibility.Collapsed;
            brightnessViewModel.isLuminanceSupport = myBrightnessViewModel;
            Assert.That(brightnessViewModel.isLuminanceSupport, Is.EqualTo(myBrightnessViewModel));
        }

        [Test]
        public void TestisAlsSupported()
        {
            var brightnessViewModel = new BrightnessViewModel();
            Visibility myBrightnessViewModel = Visibility.Visible;
            brightnessViewModel.isLuminanceSupport = myBrightnessViewModel;
            Assert.That(brightnessViewModel.isLuminanceSupport, Is.EqualTo(myBrightnessViewModel));

            myBrightnessViewModel = Visibility.Hidden;
            brightnessViewModel.isAlsSupported = myBrightnessViewModel;
            Assert.That(brightnessViewModel.isAlsSupported, Is.EqualTo(myBrightnessViewModel));

            myBrightnessViewModel = Visibility.Collapsed;
            brightnessViewModel.isAlsSupported = myBrightnessViewModel;
            Assert.That(brightnessViewModel.isAlsSupported, Is.EqualTo(myBrightnessViewModel));
        }

        [Test]
        public void TestSetALSAll()
        {
            var brightnessViewModel = new BrightnessViewModel();
            brightnessViewModel.SelectedHomeDevice = new HomeDevice();
            var deviceManagerMock = new Mock<IDeviceManagerSA>();
            var deviceManagerSA = deviceManagerMock.Object;
            DdpmCommonHelper.DeviceManagerSA = deviceManagerSA;
            var monitorInfo = new MonitorInfo();
            brightnessViewModel.SelectedHomeDevice.MonitorInfo = monitorInfo;
            deviceManagerMock.Setup(x => x.SetALSFeatureValue(It.IsAny<MonitorInfo>(), It.IsAny<ALSConfig>(), It.IsAny<ALSFeatureQueryType>(), It.IsAny<string>())).Returns(Task.FromResult(true));

            bool result = brightnessViewModel.SetALSAll(new ALSConfig(), ALSFeatureQueryType.All, 0);
            Assert.That(result, Is.EqualTo(true));
        }

        [Test]
        public void TestSupportedAutoBrightness()
        {
            var brightnessViewModel = new BrightnessViewModel();
            var myBrightnessViewModel = true;
            brightnessViewModel.SupportedAutoBrightness = myBrightnessViewModel;
            Assert.That(brightnessViewModel.SupportedAutoBrightness, Is.EqualTo(myBrightnessViewModel));
            myBrightnessViewModel = false;
            brightnessViewModel.SupportedAutoBrightness = myBrightnessViewModel;
            Assert.That(brightnessViewModel.SupportedAutoBrightness, Is.EqualTo(myBrightnessViewModel));
        }

        [Test]
        public void TestAutoBrightnessStatus()
        {
            brightnessViewModel.SelectedHomeDevice = new HomeDevice();
            var deviceManagerMock = new Mock<IDeviceManagerSA>();
            var deviceManagerSA = deviceManagerMock.Object;
            DdpmCommonHelper.DeviceManagerSA = deviceManagerSA;
            var monitorInfo = new MonitorInfo();
            brightnessViewModel.SelectedHomeDevice.MonitorInfo = monitorInfo;
            deviceManagerMock.Setup(x => x.GetVCPCapability(It.IsAny<MonitorInfo>(), It.IsAny<byte>(), It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<Priority>())).Returns(Task.FromResult(new ObjGetVCP() { result = true, value = Convert.ToInt64(10) }));
            deviceManagerMock.Setup(x => x.GetAllExistAlsConfig()).Returns(Task.FromResult(new List<ALSConfig> { new ALSConfig() { isSupportALS = 2 } }));

            brightnessViewModel.MyModule = new BrightnessModule();
            var myAutoBrightnessStatus = false;
            moduleOwnerMock = new Mock<IModuleOwner>();
            var moduleOwner = moduleOwnerMock!.Object;
            DdpmCommonHelper.ModuleOwner = moduleOwner;
            moduleOwnerMock.Setup(x => x.SelectedHomeDevice).Returns(new HomeDevice());
            brightnessViewModel.ModuleOwner = moduleOwner;
            brightnessViewModel.AutoBrightnessStatus = myAutoBrightnessStatus;
            Assert.That(brightnessViewModel.AutoBrightnessStatus, Is.EqualTo(myAutoBrightnessStatus));

            myAutoBrightnessStatus = true;
            brightnessViewModel.ModuleOwner.SelectedHomeDevice.MonitorInfo = monitorInfo;
            deviceManagerMock.Setup(x => x.ReadColorPreset(It.IsAny<MonitorInfo>())).Returns(Task.FromResult(new List<string> { "CUSTOM", "123" }));
            deviceManagerMock.Setup(x => x.SetVCPCapability(It.IsAny<MonitorInfo>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Priority>())).Returns(Task.FromResult(true));

            brightnessViewModel.AutoBrightnessStatus = myAutoBrightnessStatus;
            Assert.That(brightnessViewModel.AutoBrightnessStatus, Is.EqualTo(myAutoBrightnessStatus));
        }

        [Test]
        public void TestUpdate_AutoBrightnessStatus()
        {
            var brightnessViewModel = new BrightnessViewModel();
            brightnessViewModel.Update_AutoBrightnessStatus(true);
            var result = brightnessViewModel.AutoBrightnessStatus;
            Assert.That(result, Is.EqualTo(false));

            brightnessViewModel.Update_AutoBrightnessStatus(false);
            result = brightnessViewModel.AutoBrightnessStatus;
            Assert.That(result, Is.EqualTo(false));
        }

        [Test]
        public void TestAutoBrightness_String()
        {
            if (CultureInfo.CurrentCulture.Name == "es-US")
            {
                var brightnessViewModel = new BrightnessViewModel();
                var result = brightnessViewModel.AutoBrightness_String;
                Assert.That(result, Is.EqualTo("OFF"));

                brightnessViewModel.Start_ALSConfig.isPrimaryMonitorSync = true;
                result = brightnessViewModel.PrimaryMonitorSync_String;
                Assert.That(result, Is.EqualTo("ON"));
            }
        }

        [Test]
        public void TestSupportedAutoColorTemp()
        {
            var brightnessViewModel = new BrightnessViewModel();
            bool supportedAutoColorTemp = true;
            brightnessViewModel.SupportedAutoColorTemp = supportedAutoColorTemp;
            var result = brightnessViewModel.SupportedAutoColorTemp;
            Assert.That(result, Is.EqualTo(true));
        }

        [Test]
        public void TestAutoColorTempStatus()
        {
            var deviceManagerSAMock = new Mock<IDeviceManagerSA>();
            DdpmCommonHelper.DeviceManagerSA = deviceManagerSAMock.Object;
            deviceManagerSAMock.Setup(x => x.GetAllExistAlsConfig()).Returns(Task.FromResult(new List<ALSConfig>()));
            deviceManagerSAMock.Setup(x => x.SetALSFeatureValue(It.IsAny<MonitorInfo>(), It.IsAny<ALSConfig>(), It.IsAny<ALSFeatureQueryType>(), It.IsAny<string>())).Returns(Task.FromResult(true));
            brightnessViewModel.SelectedHomeDevice = new HomeDevice();
            brightnessViewModel.SelectedHomeDevice.MonitorInfo = new MonitorInfo();
            bool autoColorTempStatus = true;
            brightnessViewModel.MyModule = new BrightnessModule();
            brightnessViewModel.AutoColorTempStatus = autoColorTempStatus;
            var result = brightnessViewModel.AutoColorTempStatus;
            Assert.That(result, Is.EqualTo(true));
        }

        [Test]
        public void TestUpdate_AutoColorTempStatus()
        {
            var brightnessViewModel = new BrightnessViewModel();
            PrivateObject privateObject = new PrivateObject(brightnessViewModel);
            brightnessViewModel.Update_AutoColorTempStatus(true);
            var result = privateObject.GetFieldOrProperty("_autoColorTempStatus");
            Assert.That(result, Is.EqualTo(true));
        }

        [Test]
        public void TestAutoColorTemp_String()
        {
            if (CultureInfo.CurrentCulture.Name == "es-US")
            {
                var brightnessViewModel = new BrightnessViewModel();
                var result = brightnessViewModel.AutoColorTemp_String;
                Assert.That(result, Is.EqualTo("OFF"));

                brightnessViewModel.Start_ALSConfig.isAutoColorTemp = true;
                result = brightnessViewModel.AutoColorTemp_String;
                Assert.That(result, Is.EqualTo("ON"));
            }
        }

        [Test]
        public void TestSupportedPrimaryMonitorSync()
        {
            var brightnessViewModel = new BrightnessViewModel();
            bool supportedPrimaryMonitorSync = true;
            brightnessViewModel.SupportedPrimaryMonitorSync = supportedPrimaryMonitorSync;
            Assert.That(brightnessViewModel.SupportedPrimaryMonitorSync, Is.EqualTo(true));
        }

        [Test]
        public void TestPrimaryMonitorSyncStatus()
        {
            var deviceManagerSAMock = new Mock<IDeviceManagerSA>();
            DdpmCommonHelper.DeviceManagerSA = deviceManagerSAMock.Object;
            deviceManagerSAMock.Setup(x => x.GetAllExistAlsConfig()).Returns(Task.FromResult(new List<ALSConfig>()));
            deviceManagerSAMock.Setup(x => x.SetALSFeatureValue(It.IsAny<MonitorInfo>(), It.IsAny<ALSConfig>(), It.IsAny<ALSFeatureQueryType>(), It.IsAny<string>())).Returns(Task.FromResult(true));
            var brightnessViewModel = new BrightnessViewModel();
            brightnessViewModel.SelectedHomeDevice = new HomeDevice();
            brightnessViewModel.SelectedHomeDevice.MonitorInfo = new MonitorInfo();
            bool primaryMonitorSyncStatus = true;
            brightnessViewModel.PrimaryMonitorSyncStatus = primaryMonitorSyncStatus;
            Assert.That(brightnessViewModel.PrimaryMonitorSyncStatus, Is.EqualTo(true));
        }

        [Test]
        public void TestUpdate_PrimaryMonitorSyncStatus()
        {
            var brightnessViewModel = new BrightnessViewModel();
            PrivateObject privateObject = new PrivateObject(brightnessViewModel);
            brightnessViewModel.Update_PrimaryMonitorSyncStatus(true);
            var result = privateObject.GetFieldOrProperty("_primaryMonitorSyncStatus");
            Assert.That(result, Is.EqualTo(true));

            brightnessViewModel.Update_PrimaryMonitorSyncStatus(false);
            result = privateObject.GetFieldOrProperty("_primaryMonitorSyncStatus");
            Assert.That(result, Is.EqualTo(false));
        }

        [Test]
        public void TestPrimaryMonitorSync_String()
        {
            if (CultureInfo.CurrentCulture.Name == "es-US")
            {
                var brightnessViewModel = new BrightnessViewModel();
                var result = brightnessViewModel.PrimaryMonitorSync_String;
                Assert.That(result, Is.EqualTo("OFF"));

                brightnessViewModel.Start_ALSConfig.isPrimaryMonitorSync = true;
                result = brightnessViewModel.PrimaryMonitorSync_String;
                Assert.That(result, Is.EqualTo("ON"));
            }
        }

        [Test]
        public void TestAutoBrightnessRangeLevelVisible()
        {
            var result = brightnessViewModel.AutoBrightnessRangeLevelVisible;
            Assert.That(result, Is.EqualTo(Visibility.Collapsed));

            brightnessViewModel.MyModule = new BrightnessModule();
            brightnessViewModel.SelectedHomeDevice = new HomeDevice();
            brightnessViewModel.SelectedHomeDevice.MonitorInfo = new MonitorInfo();
            var deviceManagerSAMock = new Mock<IDeviceManagerSA>();
            DdpmCommonHelper.DeviceManagerSA = deviceManagerSAMock.Object;
            deviceManagerSAMock.Setup(x => x.GetAllExistAlsConfig()).Returns(Task.FromResult(new List<ALSConfig>()));
            deviceManagerSAMock.Setup(x => x.SetALSFeatureValue(It.IsAny<MonitorInfo>(), It.IsAny<ALSConfig>(), It.IsAny<ALSFeatureQueryType>(), It.IsAny<string>())).Returns(Task.FromResult(true));
            brightnessViewModel.ModuleOwner = moduleOwnerMock.Object;
            deviceManagerSAMock.Setup(x => x.ReadColorPreset(It.IsAny<MonitorInfo>())).Returns(Task.FromResult(new List<string>()));
            brightnessViewModel.AutoBrightnessStatus = true;
            result = brightnessViewModel.AutoBrightnessRangeLevelVisible;
            Assert.That(result, Is.EqualTo(Visibility.Visible));
        }

        [Test]
        public void TestAutoBrightnessRangeLevelVisible_invert()
        {
            var result = brightnessViewModel.AutoBrightnessRangeLevelVisible_invert;
            Assert.That(result, Is.EqualTo(Visibility.Visible));

            brightnessViewModel.MyModule = new BrightnessModule();
            brightnessViewModel.SelectedHomeDevice = new HomeDevice();
            brightnessViewModel.SelectedHomeDevice.MonitorInfo = new MonitorInfo();
            var deviceManagerSAMock = new Mock<IDeviceManagerSA>();
            DdpmCommonHelper.DeviceManagerSA = deviceManagerSAMock.Object;
            deviceManagerSAMock.Setup(x => x.GetAllExistAlsConfig()).Returns(Task.FromResult(new List<ALSConfig>()));
            deviceManagerSAMock.Setup(x => x.SetALSFeatureValue(It.IsAny<MonitorInfo>(), It.IsAny<ALSConfig>(), It.IsAny<ALSFeatureQueryType>(), It.IsAny<string>())).Returns(Task.FromResult(true));
            moduleOwnerMock = new Mock<IModuleOwner>();
            var moduleOwner = moduleOwnerMock!.Object;
            brightnessViewModel.ModuleOwner = moduleOwner;

            brightnessViewModel.AutoBrightnessStatus = true;
            result = brightnessViewModel.AutoBrightnessRangeLevelVisible_invert;
            Assert.That(result, Is.EqualTo(Visibility.Collapsed));
        }

        [Test]
        public void TestUpdate_AutoBrightnessRangeLevelStatus()
        {
            var brightnessViewModel = new BrightnessViewModel();
            var myAutoBrightnessLevel = new AutoBrightnessRangeLevel() { level_name = "12", level_value = 34 };
            brightnessViewModel.Update_AutoBrightnessRangeLevelStatus(myAutoBrightnessLevel);
            privateObject = new PrivateObject(brightnessViewModel);
            var _autoBrightnessRangeLevel = privateObject.GetFieldOrProperty("_autoBrightnessRangeLevel");

            Assert.That(_autoBrightnessRangeLevel, Is.Not.Null);
        }

        [Test]
        public void TestAutoBrightnessSelectedIndex()
        {
            var brightnessViewModel = new BrightnessViewModel();
            var myAutoBrightnessSelectedIndex = 1;
            brightnessViewModel.AutoBrightnessSelectedIndex = myAutoBrightnessSelectedIndex;

            Assert.That(brightnessViewModel.AutoBrightnessSelectedIndex, Is.EqualTo(myAutoBrightnessSelectedIndex));
        }

        [Test]
        public void TestAutoBrightnessRangeLevel_String()
        {
            if (CultureInfo.CurrentCulture.Name == "es-US")
            {
                //Start_ALSConfig.AutoBrightnessLevel.Count == 0
                var brightnessViewModel = new BrightnessViewModel();
                Assert.That(brightnessViewModel.AutoBrightnessRangeLevel_String, Is.EqualTo("Brightness level: 0%"));

                //Start_ALSConfig.AutoBrightnessLevel[0].level_value == 0
                brightnessViewModel.Start_ALSConfig = new ALSConfig();
                AutoBrightnessRangeLevel AutoBrightnessLevel = new AutoBrightnessRangeLevel();
                AutoBrightnessLevel = new AutoBrightnessRangeLevel() { level_name = null, level_value = 0 };
                brightnessViewModel.Start_ALSConfig.AutoBrightnessRangeLevel = AutoBrightnessLevel;
                brightnessViewModel.isLuminanceSupport = Visibility.Visible;
                var res = brightnessViewModel.AutoBrightnessRangeLevel_String;
                Assert.That(brightnessViewModel.AutoBrightnessRangeLevel_String, Is.EqualTo("Brightness level: 75%"));

                //Start_ALSConfig.AutoBrightnessLevel[0].level_value == 1
                brightnessViewModel.Start_ALSConfig = new ALSConfig();
                AutoBrightnessLevel = new AutoBrightnessRangeLevel();
                AutoBrightnessLevel = new AutoBrightnessRangeLevel() { level_name = null, level_value = 1 };
                brightnessViewModel.Start_ALSConfig.AutoBrightnessRangeLevel = AutoBrightnessLevel;
                res = brightnessViewModel.AutoBrightnessRangeLevel_String;
                Assert.That(brightnessViewModel.AutoBrightnessRangeLevel_String, Is.EqualTo("Brightness level: 75%"));

                //else
                brightnessViewModel.Start_ALSConfig = new ALSConfig();
                AutoBrightnessLevel = new AutoBrightnessRangeLevel();
                AutoBrightnessLevel = new AutoBrightnessRangeLevel() { level_name = null, level_value = 2 };
                brightnessViewModel.Start_ALSConfig.AutoBrightnessRangeLevel = AutoBrightnessLevel;
                Assert.That(brightnessViewModel.AutoBrightnessRangeLevel_String, Is.EqualTo("Brightness level: 75%"));
            }
        }

        [Test]
        public void TestAutoBrightnessRangeLevel_SelectedIndex()
        {
            brightnessViewModel.SelectedHomeDevice = new HomeDevice();
            brightnessViewModel.MyModule = new BrightnessModule();
            var deviceManagerMock = new Mock<IDeviceManagerSA>();
            var deviceManagerSA = deviceManagerMock.Object;
            moduleOwnerMock.Setup(x => x.SelectedHomeDevice).Returns(new HomeDevice());
            DdpmCommonHelper.DeviceManagerSA = deviceManagerSA;
            var monitorInfo = new MonitorInfo();
            brightnessViewModel.SelectedHomeDevice.MonitorInfo = monitorInfo;
            deviceManagerMock.Setup(x => x.SetALSFeatureValue(It.IsAny<MonitorInfo>(), It.IsAny<ALSConfig>(), It.IsAny<ALSFeatureQueryType>(), It.IsAny<string>())).Returns(Task.FromResult(true));
            deviceManagerMock.Setup(x => x.GetAllExistAlsConfig()).Returns(Task.FromResult(new List<ALSConfig>()));
            brightnessViewModel.Start_ALSConfig = new ALSConfig();
            AutoBrightnessRangeLevel AutoBrightnessLevel = new AutoBrightnessRangeLevel();
            brightnessViewModel.Start_ALSConfig.AutoBrightnessRangeLevel = AutoBrightnessLevel;
            //Start_ALSConfig.AutoBrightnessLevel.Count == 0
            Assert.That(brightnessViewModel.AutoBrightnessRangeLevel_SelectedIndex, Is.EqualTo(0));

            AutoBrightnessLevel = (new AutoBrightnessRangeLevel() { level_name = null, level_value = 10 });
            brightnessViewModel.Start_ALSConfig.AutoBrightnessRangeLevel = AutoBrightnessLevel;

            //value == 0
            brightnessViewModel.AutoBrightnessRangeLevel_SelectedIndex = 0;
            Assert.That(brightnessViewModel.AutoBrightnessRangeLevel_SelectedIndex, Is.EqualTo(brightnessViewModel.Start_ALSConfig.AutoBrightnessRangeLevel.level_value));
            if (CultureInfo.CurrentCulture.Name == "es-US")
            {
                Assert.That(brightnessViewModel.Start_ALSConfig.AutoBrightnessRangeLevel.level_name, Is.EqualTo("Low"));
            }

            //value == 1
            brightnessViewModel.AutoBrightnessRangeLevel_SelectedIndex = 1;
            Assert.That(brightnessViewModel.AutoBrightnessRangeLevel_SelectedIndex, Is.EqualTo(brightnessViewModel.Start_ALSConfig.AutoBrightnessRangeLevel.level_value));
            if (CultureInfo.CurrentCulture.Name == "es-US")
            {
                Assert.That(brightnessViewModel.Start_ALSConfig.AutoBrightnessRangeLevel.level_name, Is.EqualTo("Mid"));
            }

            //else
            brightnessViewModel.AutoBrightnessRangeLevel_SelectedIndex = 2;
            Assert.That(brightnessViewModel.AutoBrightnessRangeLevel_SelectedIndex, Is.EqualTo(brightnessViewModel.Start_ALSConfig.AutoBrightnessRangeLevel.level_value));
            if (CultureInfo.CurrentCulture.Name == "es-US")
            {
                Assert.That(brightnessViewModel.Start_ALSConfig.AutoBrightnessRangeLevel.level_name, Is.EqualTo("High"));
            }
        }

        [Test]
        public void TestAutoBrightnessRangeLevel()
        {
            var brightnessViewModel = new BrightnessViewModel();
            var myAutoBrightnessRangeLevel =
                new List<string>() { Strings.ALSRangeLevelLow, Strings.ALSRangeLevelMid, Strings.ALSRangeLevelHigh };//{ "Low", "Mid", "High" };
            brightnessViewModel.AutoBrightnessRangeLevel = myAutoBrightnessRangeLevel;

            Assert.That(brightnessViewModel.AutoBrightnessRangeLevel, Is.EqualTo(myAutoBrightnessRangeLevel));
        }

        [Test]
        public void TestPrimaryMonitorForSyncVisible()
        {
            var brightnessViewModel = new BrightnessViewModel();
            Visibility result = brightnessViewModel.PrimaryMonitorForSyncVisible;
            Assert.That(result, Is.EqualTo((Visibility)0));
        }

        [Test]
        public void TestPrimaryMonitorForSyncVisible_invert()
        {
            var brightnessViewModel = new BrightnessViewModel();
            Visibility result = brightnessViewModel.PrimaryMonitorForSyncVisible_invert;
            Assert.That(result, Is.EqualTo((Visibility)2));
        }

        [Test]
        public void TestisShowSynchronize()
        {
            var brightnessViewModel = new BrightnessViewModel();
            Visibility myBrightnessViewModel = 0;
            brightnessViewModel.isShowSynchronize = myBrightnessViewModel;
            Assert.That(brightnessViewModel.isShowSynchronize, Is.EqualTo(myBrightnessViewModel));

            myBrightnessViewModel = (Visibility)1;
            brightnessViewModel.isShowSynchronize = myBrightnessViewModel;
            Assert.That(brightnessViewModel.isShowSynchronize, Is.EqualTo(myBrightnessViewModel));

            myBrightnessViewModel = (Visibility)2;
            brightnessViewModel.isShowSynchronize = myBrightnessViewModel;
            Assert.That(brightnessViewModel.isShowSynchronize, Is.EqualTo(myBrightnessViewModel));
        }

        [Test]
        public void TestIsBusy()
        {
            var brightnessViewModel = new BrightnessViewModel();
            var myBrightnessViewModel = true;
            brightnessViewModel.IsBusy = myBrightnessViewModel;
            Assert.That(brightnessViewModel.IsBusy, Is.EqualTo(myBrightnessViewModel));
        }

        //[Test]
        //[Apartment(ApartmentState.STA)]
        //public void TestCheckMonitorALSStatus()
        //{
        //    moduleOwnerMock = new Mock<IModuleOwner>();
        //    var moduleOwner = moduleOwnerMock!.Object;
        //    DdpmCommonHelper.ModuleOwner = moduleOwner;

        //    deviceManagerMock = new Mock<IDeviceManagerSA>();
        //    brightnessViewModel = new BrightnessViewModel();
        //    privateObject = new PrivateObject(brightnessViewModel);

        //    DdpmCommonHelper.DeviceManagerSA = deviceManagerMock.Object;

        //    var status= brightnessViewModel.CheckMonitorALSStatus();
        //    // Assert
        //    Assert.That(status, Is.True);
        //}
    }
}