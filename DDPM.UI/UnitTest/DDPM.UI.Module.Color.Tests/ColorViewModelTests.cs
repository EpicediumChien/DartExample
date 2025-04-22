using DDPM.SA.Common;
using DDPM.SA.Common.Settings;
using DDPM.UI.Common;
using DDPM.UI.Common.Interfaces;
using DDPM.UI.Common.Models;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.UX.WPF;
using Moq;
using NGA.UnitTest.PrivateObject;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using VcpCore.Common;

namespace DDPM.UI.Module.Color.Tests
{
    [Apartment(ApartmentState.STA)]
    public class ColorViewModelTests
    {
        private ColorViewModel? colorViewModel;
        private PrivateObject? privateObject;
        private Mock<IModuleOwner>? moduleOwnerMock;
        private MonitorInfo? monitorInfo;
        private ColorModule? colorModule;
        private ColorModule? myModleMock;
        private IDeviceManagerSA? deviceManagerSA;
        private Mock<IDeviceManagerSA>? deviceManagerSAMock;

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
            var MyConsoleMock = new Mock<IConsole>();
            DdpmCommonHelper.MyConsole = MyConsoleMock.Object;
            colorViewModel = new ColorViewModel();
            //privateObject = new PrivateObject(monitorInfo);
            moduleOwnerMock = new Mock<IModuleOwner>();
            var moduleOwner = moduleOwnerMock!.Object;
            DdpmCommonHelper.ModuleOwner = moduleOwner;
            deviceManagerSAMock = new Mock<IDeviceManagerSA>();
            deviceManagerSA = deviceManagerSAMock.Object;
            DdpmCommonHelper.DeviceManagerSA = deviceManagerSA;
            privateObject = new PrivateObject(colorViewModel);
        }

        [Test]
        public void TestLog()
        {
            var logMock = new Mock<ILog>();
            ILog log = logMock.Object;
            colorViewModel.Log = log;
            var result = colorViewModel.Log;
            Assert.That(result, Is.EqualTo(log));
        }

        [Test]
        [Apartment(ApartmentState.STA)]
        public void TestModuleOwner()
        {
            var moduleOwner = moduleOwnerMock!.Object;
            colorViewModel.ModuleOwner = moduleOwner;
            Assert.That(colorViewModel.ModuleOwner, Is.EqualTo(moduleOwner));
        }

        [Test]
        public void TestMyModule()
        {
            var myColorModule = new ColorModule();
            colorViewModel.MyModule = myColorModule;
            Assert.That(colorViewModel.MyModule, Is.EqualTo(myColorModule));
        }

        [Test]
        public void TestSupportColorPresets()
        {
            var myColorModule = new List<string>();
            colorViewModel.SupportColorPresets = myColorModule;
            Assert.That(colorViewModel.SupportColorPresets, Is.EqualTo(myColorModule));
        }

        [Test]
        public void TestColorPresets_ItemsCollection()
        {
            var myColorModule = new List<string>();
            colorViewModel.ColorPresets_ItemsCollection = myColorModule;
            Assert.That(colorViewModel.ColorPresets_ItemsCollection, Is.EqualTo(myColorModule));
        }

        [Test]
        public void TestIsisAdvanced_Settings()
        {
            var myColorModule = new Visibility();
            colorViewModel.IsisAdvanced_Settings = myColorModule;
            Assert.That(colorViewModel.IsisAdvanced_Settings, Is.EqualTo(myColorModule));
        }

        [Test]
        public void TestNightlightStatus()
        {
            var myColorModule = new string("");
            colorViewModel.NightlightStatus = myColorModule;
            Assert.That(colorViewModel.NightlightStatus, Is.EqualTo(myColorModule));
        }

        [Test]
        public void TestColorPresetSelectedIndex()
        {
            var deviceManagerMock = new Mock<IDeviceManagerSA>();
            var deviceManagerSA = deviceManagerMock.Object;
            DdpmCommonHelper.DeviceManagerSA = deviceManagerSA;
            deviceManagerMock.Setup(x => x.ReloadAppConfigData(It.IsAny<bool>())).Returns(Task.FromResult(new DDPMSettings(new DDPMAppSettings(), new DDPMUserSettings(), new DDPMITConfig())));
            var moduleOwnerMock = new Mock<IModuleOwner>();
            var moduleOwner = moduleOwnerMock!.Object;
            DdpmCommonHelper.ModuleOwner = moduleOwner;
            moduleOwnerMock.Setup(x => x.SelectedHomeDevice).Returns(new HomeDevice());
            /* colorViewModel.MyModule = new ColorModule();
             colorViewModel.MyModule.SelectedHomeDevice.MonitorInfo = new MonitorInfo();*/
            deviceManagerMock.Setup(x => x.GetALSFeatureValue(It.IsAny<MonitorInfo>(), It.IsAny<ALSFeatureQueryType>(), It.IsAny<int>())).Returns(Task.FromResult(new ALSConfig()));
            colorViewModel.SupportColorPresets = new List<string>();
            var myColorModule = 1;
            colorViewModel.ColorPresetSelectedIndex = myColorModule;
            Assert.That(colorViewModel.ColorPresetSelectedIndex, Is.EqualTo(myColorModule));
        }

        [Test]
        public void TestUpdateColorPresetSelectedIndex()
        {
            var deviceManagerMock = new Mock<IDeviceManagerSA>();
            var deviceManagerSA = deviceManagerMock.Object;
            DdpmCommonHelper.DeviceManagerSA = deviceManagerSA;
            deviceManagerMock.Setup(x => x.ReloadAppConfigData(It.IsAny<bool>())).Returns(Task.FromResult(new DDPMSettings(new DDPMAppSettings(), new DDPMUserSettings(), new DDPMITConfig())));
            var moduleOwnerMock = new Mock<IModuleOwner>();
            var moduleOwner = moduleOwnerMock!.Object;
            DdpmCommonHelper.ModuleOwner = moduleOwner;
            moduleOwnerMock.Setup(x => x.SelectedHomeDevice).Returns(new HomeDevice());
            /*colorViewModel.MyModule = new ColorModule();
            colorViewModel.MyModule.SelectedHomeDevice.MonitorInfo = new MonitorInfo();*/
            deviceManagerMock.Setup(x => x.GetALSFeatureValue(It.IsAny<MonitorInfo>(), It.IsAny<ALSFeatureQueryType>(), It.IsAny<int>())).Returns(Task.FromResult(new ALSConfig()));
            colorViewModel.SupportColorPresets = new List<string>() { "a", "b", "c" };
            colorViewModel.ColorPresetSelectedIndex = -1;
            colorViewModel.UpdateColorPresetSelectedIndex(1);
            Assert.That(colorViewModel.ColorPresetSelectedIndex, Is.EqualTo(1));
        }

        [Test]
        public void TestColorManagement_isChecked()
        {
            var myColorManagement_isChecked = new bool();
            colorViewModel.ColorManagement_isChecked = myColorManagement_isChecked;
            Assert.That(colorViewModel.ColorManagement_isChecked, Is.EqualTo(myColorManagement_isChecked));
        }

        [Test]
        [Apartment(ApartmentState.STA)]
        public void TestICCprofile_based_Colorpreset_enable()
        {
            var myICCprofile_based_Colorpreset_enable = true;
            colorViewModel.ICCprofile_based_Colorpreset_enable = myICCprofile_based_Colorpreset_enable;
            Assert.That(colorViewModel.ICCprofile_based_Colorpreset_enable, Is.EqualTo(myICCprofile_based_Colorpreset_enable));
        }

        [Test]
        public void TestDCM_Visibility()
        {
            var myColorModule = new Visibility();
            colorViewModel.DCM_Visibility = myColorModule;
            Assert.That(colorViewModel.DCM_Visibility, Is.EqualTo(myColorModule));
        }

        [Test]
        public void TestFullView()
        {
            var myColorModule = new ContentControl();
            colorViewModel.FullView = myColorModule;
            Assert.That(colorViewModel.FullView, Is.EqualTo(myColorModule));
        }

        [Test]
        public void TestOpenFullView()
        {
            ContentControl content = new ContentControl();
            try
            {
                colorViewModel.OpenFullView(content);
                Assert.True(true);
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
                colorViewModel.CloseFullView();
                Assert.True(true);
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }
        }

        [Test]
        public void Testget_index_of_json_config_for_cur_monitor()
        {
            List<ColorPresetSettings> temp = new List<ColorPresetSettings>();
            temp.Add(new ColorPresetSettings() { ModelName = "123", SerialNumber = "111" });
            monitorInfo = new MonitorInfo() { edid = new VcpCore.Common.EDID() { ModelName = "123", SerialNumber = "111" } };
            Test_AddAppCollectionData.GetInstance()._monitorConfigs = temp;

            var result = colorViewModel.get_index_of_json_config_for_cur_monitor(monitorInfo);

            // Assert
            Assert.AreNotEqual(-1, result);
        }

        [Test]
        [Apartment(ApartmentState.STA)]
        public void Testget_cur_monitor_preset_config()
        {
            List<ColorPresetSettings> temp = new List<ColorPresetSettings>();
            temp.Add(new ColorPresetSettings() { ModelName = "123", SerialNumber = "111" });
            monitorInfo = new MonitorInfo() { edid = new VcpCore.Common.EDID() { ModelName = "123", SerialNumber = "111" } };
            Test_AddAppCollectionData.GetInstance()._monitorConfigs = temp;

            var result = colorViewModel.get_cur_monitor_preset_config(monitorInfo, temp);

            // Assert
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void TestInvoke_RefreshData()
        {
            try
            {
                colorViewModel.Invoke_RefreshData();
                Assert.True(true);
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }
        }

        //[Test]
        //public void TestWatchForProcessStart()
        //{
        //    try
        //    {
        //        colorViewModel.WatchForProcessStart();
        //        Assert.True(true);
        //    }
        //    catch (Exception ex)
        //    {
        //        Assert.Fail("not invoked");
        //    }
        //}

        //[Test]
        //public void TestWatchForProcessEnd()
        //{
        //    try
        //    {
        //        colorViewModel.WatchForProcessEnd();
        //        Assert.True(true);
        //    }
        //    catch (Exception ex)
        //    {
        //        Assert.Fail("not invoked");
        //    }
        //}



        //[Test]
        //public void TestSyncNightlightStatus()
        //{
        //    //using RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\CloudStore\\Store\\DefaultAccount\\Current\\default$windows.data.bluelightreduction.bluelightreductionstate\\windows.data.bluelightreduction.bluelightreductionstate");
        //    //object obj = registryKey?.GetValue("Data");
        //    //byte[] array = (byte[])obj;
        //    //array[18] = 0x13;
        //    //object valueset=array.ToString();
        //    //registryKey?.SetValue("Data", valueset);

        //    colorViewModel.MyModule = new ColorModule();
        //    PrivateObject pObj = new PrivateObject(colorViewModel);
        //    pObj.Invoke("SyncNightlightStatus", null);

        //    Assert.That(colorViewModel.NightlightStatus, Is.Not.Null);
        //}

        //[Test]
        //public void TestStopRegistryMonitor()
        //{
        //colorViewModel.registryMonitor_NightLight = new RegistryMonitor_NightLight("HKEY_CLASSES_ROOT");
        //colorViewModel.registryMonitor_ICC = new RegistryMonitor_ICC("HKEY_CLASSES_ROOT");
        //colorViewModel.StopRegistryMonitor();
        //Assert.That(colorViewModel.registryMonitor_NightLight, Is.EqualTo(null));
        //Assert.That(colorViewModel.registryMonitor_ICC, Is.EqualTo(null));
        //}

        //[Test]
        //public void TestOnRegChanged_NightLight()
        //{
        //    //colorViewModel.MyModule = new ColorModule();
        //    //colorViewModel.OnRegChanged_NightLight(null, null);

        //    //Assert.That(colorViewModel.NightlightStatus, Is.Not.Null);
        //}

        //[Test]
        //[Apartment(ApartmentState.STA)]
        //public void TestOnError_NightLight()
        //{
        //    //colorViewModel.registryMonitor_NightLight = new RegistryMonitor_NightLight("HKEY_CLASSES_ROOT");
        //    //colorViewModel.registryMonitor_ICC = new RegistryMonitor_ICC("HKEY_CLASSES_ROOT");
        //    //colorViewModel.MyModule = new ColorModule();
        //    //colorViewModel.OnError_NightLight(null, null);

        //    //Assert.That(colorViewModel.registryMonitor_NightLight, Is.EqualTo(null));
        //    //Assert.That(colorViewModel.registryMonitor_ICC, Is.EqualTo(null));
        //}

        //[Test]
        //public void TestOnRegChanged_ICC()
        //{
        //    var MyModule = new ColorModule();
        //    colorViewModel.MyModule = MyModule;
        //    var SelectedHomeDevice = new HomeDevice();
        //    var colorModule=new ColorModule();
        //    colorModule.SelectedHomeDevice= SelectedHomeDevice;
        //    colorViewModel.MyModule.SelectedHomeDevice=SelectedHomeDevice;
        //    colorViewModel.MyModule.SelectedHomeDevice.MonitorInfo = new MonitorInfo() { modelName = "abc" };
        //    try
        //    {
        //        colorViewModel.OnRegChanged_ICC(null,null);
        //        Assert.True(true);
        //    }
        //    catch (Exception ex)
        //    {
        //        Assert.Fail("not invoked");
        //    }
        //}

        //[Test]
        //[Apartment(ApartmentState.STA)]
        //public void TestOnError_ICC()
        //{
        //    colorViewModel.registryMonitor_NightLight = new RegistryMonitor_NightLight("HKEY_CLASSES_ROOT");
        //    //colorViewModel.registryMonitor_ICC = new RegistryMonitor_ICC("HKEY_CLASSES_ROOT");
        //    //colorViewModel.OnError_ICC(null, null);
        //    Assert.That(colorViewModel.registryMonitor_NightLight, Is.EqualTo(null));
        //    //Assert.That(colorViewModel.registryMonitor_ICC, Is.EqualTo(null));
        //}

        [Test]
        public void TestAppsList()
        {
            ObservableCollection<AppData> observableCollection = new ObservableCollection<AppData>();
            colorViewModel.AppsList = observableCollection;
            Assert.That(colorViewModel.AppsList, Is.EqualTo(observableCollection));
        }

        [Test]
        public void TestRefreshUI()
        {
            var appsListBef = privateObject.GetFieldOrProperty("AppsList");
            var appdata = new ObservableCollection<AppData>();
            //var appList = colorViewModel.AppsList;
            colorViewModel.AppsList = appdata;
            colorViewModel.RefreshUI();
            var appsListAft = privateObject.GetFieldOrProperty("AppsList");

            var colorPresets_ItemsCollectionBef = privateObject.GetFieldOrProperty("ColorPresets_ItemsCollection");
            var colorPresets_ItemsCollection = new List<string>();
            colorViewModel.ColorPresets_ItemsCollection = colorPresets_ItemsCollection;
            var colorPresets_ItemsCollectionAft = privateObject.GetFieldOrProperty("ColorPresets_ItemsCollection");

            Assert.AreNotSame(appsListBef, appsListAft);
            Assert.AreNotSame(colorPresets_ItemsCollectionBef, colorPresets_ItemsCollectionAft);
        }

        [Test]
        public void TestIsBusy()
        {
            colorViewModel.IsBusy = true;
            Assert.That(colorViewModel.IsBusy, Is.EqualTo(true));
        }
        [Test]
        public void TestHDRStatus_String()
        {
            if (CultureInfo.CurrentCulture.Name == "es-US")
            {
                var result = colorViewModel!.HDRStatus_String;
                Assert.That(result, Is.EqualTo("OFF"));

                deviceManagerSAMock.Setup(x => x.GetDisplayPropertiesInfo(It.IsAny<MonitorInfo>())).Returns(Task.FromResult(new DisplayPropertiesInfo()));
                deviceManagerSAMock.Setup(x => x.SetHDRStatus(It.IsAny<MonitorInfo>(), It.IsAny<bool>())).Returns(Task.FromResult(true));
                var moduleOwnerMock = new Mock<IModuleOwner>();
                var moduleOwner = moduleOwnerMock!.Object;
                DdpmCommonHelper.ModuleOwner = moduleOwner;
                moduleOwnerMock.Setup(x => x.SelectedHomeDevice).Returns(new HomeDevice());
                colorViewModel.HDRStatus = true;
                result = colorViewModel!.HDRStatus_String;
                Assert.That(result, Is.EqualTo("ON"));
            }
        }

        [Test]
        public void TestSupportedHDR()
        {
            var result = colorViewModel!.SupportedHDR.ToString();
            Assert.That(result, Is.EqualTo("Visible"));

            privateObject.SetFieldOrProperty("_SupportedHDR", false);
            result = colorViewModel!.SupportedHDR.ToString();
            Assert.That(result, Is.EqualTo("Collapsed"));
        }        

        [Test]
        public void TestHDRStatus()
        {
            deviceManagerSAMock.Setup(x => x.GetDisplayPropertiesInfo(It.IsAny<MonitorInfo>())).Returns(Task.FromResult(new DisplayPropertiesInfo()));
            deviceManagerSAMock.Setup(x => x.SetHDRStatus(It.IsAny<MonitorInfo>(), It.IsAny<bool>())).Returns(Task.FromResult(true));
            bool myHDRStatus = true;
            var moduleOwnerMock = new Mock<IModuleOwner>();
            var moduleOwner = moduleOwnerMock!.Object;
            DdpmCommonHelper.ModuleOwner = moduleOwner;
            moduleOwnerMock.Setup(x => x.SelectedHomeDevice).Returns(new HomeDevice());
            colorViewModel.HDRStatus = myHDRStatus;

            Assert.That(colorViewModel.HDRStatus, Is.EqualTo(myHDRStatus));
        }
    }
}