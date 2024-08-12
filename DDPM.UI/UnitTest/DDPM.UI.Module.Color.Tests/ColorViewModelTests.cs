using DDPM.UI.Common.Interfaces;
using DDPM.UI.Common;
using Moq;
using NGA.UnitTest.PrivateObject;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VcpCore.Common;
using DDPM.UI.Interfaces;
using DDPM.SA.Common;
using System.Windows;
using System.Windows.Controls;
using System.Reflection;
using Microsoft.Win32;
using System.Xml.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using System.ComponentModel;
using System.Collections.ObjectModel;
using Dell.Client.Framework.UX.WPF.Controls;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;
using System.Management;
using static System.Formats.Asn1.AsnWriter;
using System.Drawing;
using NSubstitute;
using static DDPM.UI.Module.Color.ColorViewModel;

namespace DDPM.UI.Module.Color.Tests
{
    public class ColorViewModelTests
    {
        private ColorViewModel? colorViewModel;
        private PrivateObject? privateObject;
        private Mock<IModuleOwner>? moduleOwnerMock;
        private MonitorInfo? monitorInfo;
        private ColorModule? colorModule;
        private ColorModule? myModleMock;



        [SetUp]
        public void Setup()
        {
            colorViewModel = new ColorViewModel();  
            //privateObject = new PrivateObject(monitorInfo);
            moduleOwnerMock = new Mock<IModuleOwner>();
            var moduleOwner = moduleOwnerMock!.Object;
            DdpmCommonHelper.ModuleOwner = moduleOwner;
            privateObject = new PrivateObject(colorViewModel);

        }

        [Test]
        [Apartment(ApartmentState.STA)]
        public void TestFile()
        {
            var file = new ICC_SupportDeviceName();
            var myFile = new string("");
            file.File = myFile;
            Assert.That(file.File, Is.EqualTo(myFile));
        }


        [Test]
        [Apartment(ApartmentState.STA)]
        public void TestColorPreset()
        {
            var colorPreset = new ICC_SupportDeviceName();
            var myColorPreset = new string("");
            colorPreset.ColorPreset = myColorPreset;
            Assert.That(colorPreset.ColorPreset, Is.EqualTo(myColorPreset));
        }

        [Test]
        [Apartment(ApartmentState.STA)]
        public void TestSHA256()
        {
            var sHA256 = new ICC_SupportDeviceName();
            var mySHA256 = new string("");
            sHA256.SHA256 = mySHA256;
            Assert.That(sHA256.ColorPreset, Is.EqualTo(mySHA256));
        }


        [Test]
        [Apartment(ApartmentState.STA)]
        public void TestModuleOwner()
        {
            var moduleOwner = moduleOwnerMock!.Object;
            Assert.That(DdpmCommonHelper.ModuleOwner, Is.EqualTo(moduleOwner));
        }


        [Test]
        [Apartment(ApartmentState.STA)]
        public void TestMyModule()
        {
            var myModle = new ColorViewModel();
            var myColorModule = new ColorModule();
            myModle.MyModule = myColorModule;
            Assert.That(myModle.MyModule, Is.EqualTo(myColorModule));
        }


        [Test]
        [Apartment(ApartmentState.STA)]
        public void TeststrICM_Folder()
        {       
            var strICM_Folder = new ColorViewModel();
            var myColorModule = new string("");
            strICM_Folder._ICC_Metadata.strICC_Folder = myColorModule;
            Assert.That(strICM_Folder._ICC_Metadata.strICC_Folder, Is.EqualTo(myColorModule));
        }

        [Test]
        [Apartment(ApartmentState.STA)]
        public void TestIs_SupportDeviceName()
        {
            var is_SupportDeviceName = new ColorViewModel();
            var myColorModule = new bool();
            is_SupportDeviceName._ICC_Metadata.Is_Support_ICC_DeviceName = myColorModule;
            Assert.That(is_SupportDeviceName._ICC_Metadata.Is_Support_ICC_DeviceName, Is.EqualTo(myColorModule));
        }
       


        [Test]
        [Apartment(ApartmentState.STA)]
        public void TestSupportColorPresets()
        {
            var supportColorPresets = new ColorViewModel();
            var myColorModule = new List<string>();
            supportColorPresets.SupportColorPresets = myColorModule;
            Assert.That(supportColorPresets.SupportColorPresets, Is.EqualTo(myColorModule));
        }


        [Test]
        [Apartment(ApartmentState.STA)]
        public void TestColorPresets_ItemsCollection()
        {
            var colorPresets_ItemsCollection = new ColorViewModel();
            var myColorModule = new List<string>();
            colorPresets_ItemsCollection.ColorPresets_ItemsCollection = myColorModule;
            Assert.That(colorPresets_ItemsCollection.ColorPresets_ItemsCollection, Is.EqualTo(myColorModule));
        }

        [Test]
        [Apartment(ApartmentState.STA)]
        public void TestSupportDeviceName()
        {
            var supportDeviceName = new ColorViewModel();
            var myColorModule = new List<string>() { "U4021QW", "U2723QE", "U3223QE", "U3223QZ", "U3423WE", "U3824DW", "U4924DW", "U3224KB", "U2724D", "U2724DE", "U3425WE", "U4025QW", "UP2720Q", "UP3221Q" };
            //supportDeviceName.Support_ICC_DeviceName = myColorModule;
            //Assert.That(supportDeviceName.Support_ICC_DeviceName, Is.EqualTo(myColorModule));
        }


        [Test]
        [Apartment(ApartmentState.STA)]
        public void TestIsisAdvanced_Settings()
        {
            var isisAdvanced_Settings = new ColorViewModel();
            var myColorModule = new Visibility();
            isisAdvanced_Settings.IsisAdvanced_Settings = myColorModule;
            Assert.That(isisAdvanced_Settings.IsisAdvanced_Settings, Is.EqualTo(myColorModule));
        }


        [Test]
        [Apartment(ApartmentState.STA)]
        public void TestUpdateColorPresetSelectedIndex()
        {
            colorViewModel.ColorPresetSelectedIndex = -1;
            colorViewModel.UpdateColorPresetSelectedIndex(1);
            Assert.That(colorViewModel.ColorPresetSelectedIndex, Is.EqualTo(1));
        }

        
        [Test]
        [Apartment(ApartmentState.STA)]
        public void TestColorManagement_isChecked()
        {
            var colorManagement_isChecked = new ColorViewModel();
            var myColorManagement_isChecked = new bool();
            colorManagement_isChecked.ColorManagement_isChecked = myColorManagement_isChecked;
            Assert.That(colorManagement_isChecked.ColorManagement_isChecked, Is.EqualTo(myColorManagement_isChecked));
        }

        [Test]
        [Apartment(ApartmentState.STA)]
        public void TestICCprofile_based_Colorpreset_enable()
        {
            var iCCprofile_based_Colorpreset_enable = new ColorViewModel();
            var myICCprofile_based_Colorpreset_enable = new bool();
            iCCprofile_based_Colorpreset_enable.ICCprofile_based_Colorpreset_enable = myICCprofile_based_Colorpreset_enable;
            Assert.That(iCCprofile_based_Colorpreset_enable.ICCprofile_based_Colorpreset_enable, Is.EqualTo(myICCprofile_based_Colorpreset_enable));
        }


        [Test]
        [Apartment(ApartmentState.STA)]
        public void TestNightlightStatus()
        {
            var nightlightStatus = new ColorViewModel();
            var myColorModule = new string("");
            nightlightStatus.NightlightStatus = myColorModule;
            Assert.That(nightlightStatus.NightlightStatus, Is.EqualTo(myColorModule));
        }

        [Test]
        [Apartment(ApartmentState.STA)]
        public void TestColorPresetSelectedIndex()
        {
            var colorPresetSelectedIndex = new ColorViewModel();
            var myColorModule = new int();
            colorPresetSelectedIndex.ColorPresetSelectedIndex = myColorModule;
            Assert.That(colorPresetSelectedIndex.ColorPresetSelectedIndex, Is.EqualTo(myColorModule));
        }

        [Test]
        [Apartment(ApartmentState.STA)]
        public void TestDCM_Visibility()
        {
            var dCM_Visibility = new ColorViewModel();
            var myColorModule = new Visibility();
            dCM_Visibility.DCM_Visibility = myColorModule;
            Assert.That(dCM_Visibility.DCM_Visibility, Is.EqualTo(myColorModule));
        }



        [Test]
        [Apartment(ApartmentState.STA)]
        public void TestFullView()
        {
            var fullView = new ColorViewModel();
            var myColorModule = new ContentControl();
            fullView.FullView = myColorModule;
            Assert.That(fullView.FullView, Is.EqualTo(myColorModule));
        }


        [Test]
        [Apartment(ApartmentState.STA)]
        public void Testget_index_of_json_config_for_cur_monitor()
        {

            List<ColorPresetSettings> temp = new List<ColorPresetSettings>();
            temp.Add(new ColorPresetSettings() { DeviceInfo = new VcpCore.Common.EDID() { ModelName = "123", SerialNumber = "111" } });
            monitorInfo = new MonitorInfo() { edid = new VcpCore.Common.EDID() { ModelName = "123", SerialNumber = "111" } };
            Test_AddAppCollectionData.GetInstance()._monitorConfigs = temp;

            var result = colorViewModel.get_index_of_json_config_for_cur_monitor(monitorInfo);

            // Assert
            Assert.AreNotEqual(-1,result);

        }
        


        [Test]
        [Apartment(ApartmentState.STA)]
        public void Testget_cur_monitor_preset_config()
        {

            List<ColorPresetSettings> temp = new List<ColorPresetSettings>();
            temp.Add(new ColorPresetSettings() { DeviceInfo = new VcpCore.Common.EDID() { ModelName = "123", SerialNumber = "111" } });
            monitorInfo = new MonitorInfo() { edid = new VcpCore.Common.EDID() { ModelName = "123", SerialNumber = "111" } };
            Test_AddAppCollectionData.GetInstance()._monitorConfigs = temp;

            var result =  colorViewModel.get_cur_monitor_preset_config(monitorInfo, temp);

            // Assert
            Assert.That(result, Is.Not.Null);

        }

        [Test]
        [Apartment(ApartmentState.STA)]
        public void TestBytesToString()
        {
            byte[] bytes = new byte[] { 97, 98, 99 };
            //var result = ColorViewModel.BytesToString(bytes);

            //Assert.That(result, Is.EqualTo("616263"));
        }


        [Test]
        [Apartment(ApartmentState.STA)]
        public void TestSyncNightlightStatusOn()
        {
            //using RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\CloudStore\\Store\\DefaultAccount\\Current\\default$windows.data.bluelightreduction.bluelightreductionstate\\windows.data.bluelightreduction.bluelightreductionstate");
            //object obj = registryKey?.GetValue("Data");
            //byte[] array = (byte[])obj;
            //array[18] = 0x13;
            //object valueset=array.ToString();
            //registryKey?.SetValue("Data", valueset);

            colorViewModel.MyModule = new ColorModule();
            PrivateObject pObj = new PrivateObject(colorViewModel);
            pObj.Invoke("SyncNightlightStatus", null);
            
            Assert.That(colorViewModel.NightlightStatus, Is.EqualTo("On"));
        }


        [Test]
        [Apartment(ApartmentState.STA)]
        public void TestStopRegistryMonitor()
        {
            colorViewModel.registryMonitor_NightLight = new RegistryUtils.RegistryMonitor_NightLight("HKEY_CLASSES_ROOT");
            colorViewModel.registryMonitor_ICC = new RegistryUtils.RegistryMonitor_ICC("HKEY_CLASSES_ROOT");
            colorViewModel.StopRegistryMonitor();
            Assert.That(colorViewModel.registryMonitor_NightLight, Is.EqualTo(null));
            Assert.That(colorViewModel.registryMonitor_ICC, Is.EqualTo(null));

        }


        [Test]
        [Apartment(ApartmentState.STA)]
        public void TestOnRegChanged_NightLight()
        {            
            colorViewModel.MyModule = new ColorModule();
            colorViewModel.OnRegChanged_NightLight(null,null);

            Assert.That(colorViewModel.NightlightStatus, Is.EqualTo("On"));
        }


        [Test]
        [Apartment(ApartmentState.STA)]
        public void TestOnError_NightLight()
        {
            colorViewModel.registryMonitor_NightLight = new RegistryUtils.RegistryMonitor_NightLight("HKEY_CLASSES_ROOT");
            colorViewModel.registryMonitor_ICC = new RegistryUtils.RegistryMonitor_ICC("HKEY_CLASSES_ROOT");
            colorViewModel.MyModule = new ColorModule();
            colorViewModel.OnError_NightLight(null, null);

            Assert.That(colorViewModel.registryMonitor_NightLight, Is.EqualTo(null));
            Assert.That(colorViewModel.registryMonitor_ICC, Is.EqualTo(null));
        }


        [Test]
        [Apartment(ApartmentState.STA)]
        public void TestOnError_ICC()
        {
            colorViewModel.registryMonitor_NightLight = new RegistryUtils.RegistryMonitor_NightLight("HKEY_CLASSES_ROOT");
            colorViewModel.registryMonitor_ICC = new RegistryUtils.RegistryMonitor_ICC("HKEY_CLASSES_ROOT");
            colorViewModel.OnError_ICC(null,null);
            Assert.That(colorViewModel.registryMonitor_NightLight, Is.EqualTo(null));
            Assert.That(colorViewModel.registryMonitor_ICC, Is.EqualTo(null));
        }


        [Test]
        [Apartment(ApartmentState.STA)]
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



    }




}



