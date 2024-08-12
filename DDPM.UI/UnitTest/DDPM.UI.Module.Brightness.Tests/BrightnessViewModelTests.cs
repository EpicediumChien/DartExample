using DDPM.UI.Common.Interfaces;
using DDPM.UI.Common;
using Moq;
using NGA.UnitTest.PrivateObject;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DDPM.SA.Common;
using System.Windows.Media;
using DDPM.UI.Common.Models;
using DDPM.UI.Common.ViewModels;
using System.Windows;
using VcpCore.Common;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Drawing;
using Newtonsoft.Json.Linq;

namespace DDPM.UI.Module.Brightness.Tests
{
    public class BrightnessViewModelTests
    {
        private BrightnessViewModel? brightnessViewModel;
        private PrivateObject? privateObject;
        private Mock<IModuleOwner>? moduleOwnerMock;
        private Mock<IDeviceManagerSA>? deviceManagerMock;

        [SetUp]
        public void Setup()
        {

            if (!UriParser.IsKnownScheme("pack"))
            {
                new System.Windows.Application();
            }

            moduleOwnerMock = new Mock<IModuleOwner>();
            var moduleOwner = moduleOwnerMock!.Object;
            DdpmCommonHelper.ModuleOwner = moduleOwner;

        }


        [Test]
        [Apartment(ApartmentState.STA)]
        public void TestBrightnessImage()
        {
            var brightnessViewModel = new BrightnessViewModel();
            var myBrightnessViewModel = DdpmCommonHelper.GetImageSourceFromCommonResource("Resources/Product_KB900.png");
            brightnessViewModel.BrightnessImage = myBrightnessViewModel;
            Assert.That(brightnessViewModel.BrightnessImage, Is.EqualTo(myBrightnessViewModel));

        }

        [Test]
        [Apartment(ApartmentState.STA)]
        public void TestContrastImage()
        {
            var brightnessViewModel = new BrightnessViewModel();
            var myBrightnessViewModel = DdpmCommonHelper.GetImageSourceFromCommonResource("Resources/Product_KB900.png");
            brightnessViewModel.ContrastImage = myBrightnessViewModel;
            Assert.That(brightnessViewModel.ContrastImage, Is.EqualTo(myBrightnessViewModel));
        }

        [Test]
        [Apartment(ApartmentState.STA)]
        public void TestLuminanceImage()
        {           
            var brightnessViewModel = new BrightnessViewModel();
            var myBrightnessViewModel = DdpmCommonHelper.GetImageSourceFromCommonResource("Resources/Product_KB900.png");
            brightnessViewModel.LuminanceImage = myBrightnessViewModel;
            Assert.That(brightnessViewModel.LuminanceImage, Is.EqualTo(myBrightnessViewModel));
        }

        [Test]
        [Apartment(ApartmentState.STA)]
        public void TestIsSynchronize_String()
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

        [Test]
        [Apartment(ApartmentState.STA)]
        public void TestBrightnessMinsKey()
        {
            moduleOwnerMock.Setup(x => x.SelectedHomeDevice).Returns(new HomeDevice());

            var brightnessViewModel = new BrightnessViewModel();            
            var myBrightnessViewModel = "BrightnessMinsKey";
            brightnessViewModel.BrightnessMinsKey = myBrightnessViewModel;
            Assert.That(brightnessViewModel.BrightnessMinsKey, Is.EqualTo(myBrightnessViewModel));
        }

        [Test]
        [Apartment(ApartmentState.STA)]
        public void TestBrightnessAddKey()
        {
            moduleOwnerMock.Setup(x => x.SelectedHomeDevice).Returns(new HomeDevice());

            var brightnessViewModel = new BrightnessViewModel();
            var myBrightnessViewModel = "BrightnessAddKey";
            brightnessViewModel.BrightnessAddKey = myBrightnessViewModel;
            Assert.That(brightnessViewModel.BrightnessAddKey, Is.EqualTo(myBrightnessViewModel));
        }


        [Test]
        [Apartment(ApartmentState.STA)]
        public void TestContrastMinsKey()
        {
            moduleOwnerMock.Setup(x => x.SelectedHomeDevice).Returns(new HomeDevice());

            var brightnessViewModel = new BrightnessViewModel();
            var myBrightnessViewModel = "ContrastMinsKey";
            brightnessViewModel.ContrastMinsKey = myBrightnessViewModel;
            Assert.That(brightnessViewModel.ContrastMinsKey, Is.EqualTo(myBrightnessViewModel));
        }


        [Test]
        [Apartment(ApartmentState.STA)]
        public void TestContrastAddKey()
        {
            moduleOwnerMock.Setup(x => x.SelectedHomeDevice).Returns(new HomeDevice());

            var brightnessViewModel = new BrightnessViewModel();
            var myBrightnessViewModel = "ContrastAddKey";
            brightnessViewModel.ContrastAddKey = myBrightnessViewModel;
            Assert.That(brightnessViewModel.ContrastAddKey, Is.EqualTo(myBrightnessViewModel));
        }

        [Test]
        [Apartment(ApartmentState.STA)]
        public void TestLuminanceMinsKey()
        {
            moduleOwnerMock.Setup(x => x.SelectedHomeDevice).Returns(new HomeDevice());

            var brightnessViewModel = new BrightnessViewModel();
            var myBrightnessViewModel = "LuminanceMinsKey";
            brightnessViewModel.LuminanceMinsKey = myBrightnessViewModel;
            Assert.That(brightnessViewModel.LuminanceMinsKey, Is.EqualTo(myBrightnessViewModel));
        }

        [Test]
        [Apartment(ApartmentState.STA)]
        public void TestLuminanceAddKey()
        {
            moduleOwnerMock.Setup(x => x.SelectedHomeDevice).Returns(new HomeDevice());

            var brightnessViewModel = new BrightnessViewModel();
            var myBrightnessViewModel = "LuminanceAddKey";
            brightnessViewModel.LuminanceAddKey = myBrightnessViewModel;
            Assert.That(brightnessViewModel.LuminanceAddKey, Is.EqualTo(myBrightnessViewModel));
        }

        [Test]
        [Apartment(ApartmentState.STA)]
        public void TestBrightnessViewModel()
        {
            moduleOwnerMock.Setup(x => x.SelectedHomeDevice).Returns(new HomeDevice());

            var brightnessViewModel = new BrightnessViewModel();
            Assert.That(brightnessViewModel, Is.Not.Null);
        }


        [Test]
        [Apartment(ApartmentState.STA)]
        public void TestIsSynchronize()
        {
            moduleOwnerMock.Setup(x => x.SelectedHomeDevice).Returns(new HomeDevice());

            var brightnessViewModel = new BrightnessViewModel();
            var myIsSynchronize = true;
            brightnessViewModel.IsSynchronize = myIsSynchronize;
            Assert.That(brightnessViewModel.IsSynchronize, Is.EqualTo(myIsSynchronize));
        }


        [Test]
        [Apartment(ApartmentState.STA)]
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
        [Apartment(ApartmentState.STA)]
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

            var myContrastValue = 2;
            brightnessViewModel.ContrastValue = myContrastValue;
            Assert.That(brightnessViewModel.ContrastValue, Is.EqualTo(myContrastValue));
        }

        [Test]
        [Apartment(ApartmentState.STA)]
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
        [Apartment(ApartmentState.STA)]
        public void TestLuminanceMaxValue()
        {
            var brightnessViewModel = new BrightnessViewModel();
            Assert.That(brightnessViewModel.LuminanceMaxValue, Is.Not.Null);
        }

        [Test]
        [Apartment(ApartmentState.STA)]
        public void TestUpdateBrightnessContrast()
        {
            var brightnessViewModel=new BrightnessViewModel();
            brightnessViewModel.SelectedHomeDevice = new HomeDevice();
            var deviceManagerMock = new Mock<IDeviceManagerSA>();
            var deviceManagerSA = deviceManagerMock.Object;
            DdpmCommonHelper.DeviceManagerSA = deviceManagerSA;
            var monitorInfo = new MonitorInfo();
            brightnessViewModel.SelectedHomeDevice.MonitorInfo= monitorInfo;
            deviceManagerMock.Setup(x => x.GetVCPCapability(It.IsAny<MonitorInfo>(),It.IsAny<byte>(),It.IsAny<int>())).Returns(Task.FromResult(new ObjGetVCP() {result=true,value= Convert.ToInt64(6)}));
 
            brightnessViewModel.UpdateBrightnessContrast();
            var privateObject = new PrivateObject(brightnessViewModel);

            Assert.That(privateObject.GetFieldOrProperty("BrightnessValue"), Is.EqualTo(6));
            Assert.That(privateObject.GetFieldOrProperty("ContrastValue"), Is.EqualTo(6));

        }


        [Test]
        [Apartment(ApartmentState.STA)]
        public void TestUpdateLuminance()
        {
            var brightnessViewModel = new BrightnessViewModel();
            brightnessViewModel.SelectedHomeDevice = new HomeDevice();
            var deviceManagerMock = new Mock<IDeviceManagerSA>();
            var deviceManagerSA = deviceManagerMock.Object;
            DdpmCommonHelper.DeviceManagerSA = deviceManagerSA;
            var monitorInfo = new MonitorInfo();
            brightnessViewModel.SelectedHomeDevice.MonitorInfo = monitorInfo;
            deviceManagerMock.Setup(x => x.GetVCPCapability(It.IsAny<MonitorInfo>(), It.IsAny<byte>(), It.IsAny<int>())).Returns(Task.FromResult(new ObjGetVCP() { result = true, value = Convert.ToInt64(10) }));

            brightnessViewModel.UpdateLuminance();
            var privateObject = new PrivateObject(brightnessViewModel);

            Assert.That(privateObject.GetFieldOrProperty("LuminanceValue"), Is.EqualTo(10));
            Assert.That(privateObject.GetFieldOrProperty("LuminanceMax_Value"), Is.EqualTo(10));

        }


        [Test]
        [Apartment(ApartmentState.STA)]
        public void TestModuleOwner()
        {          
            var mockModuleOwner = new Mock<IModuleOwner>();
            var brightnessViewModel = new BrightnessViewModel();
            brightnessViewModel.ModuleOwner = mockModuleOwner.Object;

            Assert.That(brightnessViewModel.ModuleOwner, Is.EqualTo(mockModuleOwner.Object));
        }


        [Test]
        [Apartment(ApartmentState.STA)]
        public void TestExpanderGroup()
        {       
            var brightnessViewModel = new BrightnessViewModel();
            var myExpanderGroup = 2;
            brightnessViewModel.ExpanderGroup = myExpanderGroup;

            Assert.That(brightnessViewModel.ExpanderGroup, Is.EqualTo(myExpanderGroup));
        }
        


        [Test]
        [Apartment(ApartmentState.STA)]
        public void TestisScheduleSupport()
        {
            var brightnessViewModel = new BrightnessViewModel();
            var myBrightnessViewModel = new Visibility();
            brightnessViewModel.isScheduleSupport = myBrightnessViewModel;
            Assert.That(brightnessViewModel.isScheduleSupport, Is.EqualTo(myBrightnessViewModel));
        }

        [Test]
        [Apartment(ApartmentState.STA)]
        public void TestisNormalBrightness()
        {
            var brightnessViewModel = new BrightnessViewModel();
            Visibility myBrightnessViewModel = 0;
            brightnessViewModel.isNormalBrightness = myBrightnessViewModel;
            Assert.That(brightnessViewModel.isNormalBrightness, Is.EqualTo(myBrightnessViewModel));

            myBrightnessViewModel = (Visibility)1;
            brightnessViewModel.isNormalBrightness = myBrightnessViewModel;
            Assert.That(brightnessViewModel.isNormalBrightness, Is.EqualTo(myBrightnessViewModel));

            myBrightnessViewModel = (Visibility)2;
            brightnessViewModel.isNormalBrightness = myBrightnessViewModel;
            Assert.That(brightnessViewModel.isNormalBrightness, Is.EqualTo(myBrightnessViewModel));
        }


        [Test]
        [Apartment(ApartmentState.STA)]
        public void TestisLuminanceSupport()
        {
            var brightnessViewModel = new BrightnessViewModel();
            Visibility myBrightnessViewModel = 0;
            brightnessViewModel.isLuminanceSupport = myBrightnessViewModel;
            Assert.That(brightnessViewModel.isLuminanceSupport, Is.EqualTo(myBrightnessViewModel));

            myBrightnessViewModel = (Visibility)1;
            brightnessViewModel.isLuminanceSupport = myBrightnessViewModel;
            Assert.That(brightnessViewModel.isLuminanceSupport, Is.EqualTo((Visibility)2));

            myBrightnessViewModel = (Visibility)2;
            brightnessViewModel.isLuminanceSupport = myBrightnessViewModel;
            Assert.That(brightnessViewModel.isLuminanceSupport, Is.EqualTo(myBrightnessViewModel));
        }

        [Test]
        [Apartment(ApartmentState.STA)]
        public void TestisAlsSupported()
        {
            var brightnessViewModel = new BrightnessViewModel();
            Visibility myBrightnessViewModel = 0;
            brightnessViewModel.isLuminanceSupport = myBrightnessViewModel;
            Assert.That(brightnessViewModel.isLuminanceSupport, Is.EqualTo(myBrightnessViewModel));

            myBrightnessViewModel = (Visibility)1;
            brightnessViewModel.isAlsSupported = myBrightnessViewModel;
            Assert.That(brightnessViewModel.isAlsSupported, Is.EqualTo(myBrightnessViewModel));

            myBrightnessViewModel = (Visibility)2;
            brightnessViewModel.isAlsSupported = myBrightnessViewModel;
            Assert.That(brightnessViewModel.isAlsSupported, Is.EqualTo(myBrightnessViewModel));
        }

        [Test]
        [Apartment(ApartmentState.STA)]
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
        [Apartment(ApartmentState.STA)]
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
        [Apartment(ApartmentState.STA)]
        public void TestAutoBrightnessStatus()
        {
            var brightnessViewModel = new BrightnessViewModel();
            brightnessViewModel.SelectedHomeDevice = new HomeDevice();
            var deviceManagerMock = new Mock<IDeviceManagerSA>();
            var deviceManagerSA = deviceManagerMock.Object;
            DdpmCommonHelper.DeviceManagerSA = deviceManagerSA;
            var monitorInfo = new MonitorInfo();
            brightnessViewModel.SelectedHomeDevice.MonitorInfo = monitorInfo;
            deviceManagerMock.Setup(x => x.GetVCPCapability(It.IsAny<MonitorInfo>(), It.IsAny<byte>(), It.IsAny<int>())).Returns(Task.FromResult(new ObjGetVCP() { result = true, value = Convert.ToInt64(10) }));
            deviceManagerMock.Setup(x=> x.GetAllExistAlsConfig()).Returns(Task.FromResult(new List<ALSConfig> { new ALSConfig() { isSupportALS = 2 } }));
            
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
            deviceManagerMock.Setup(x => x.ReadColorPreset(It.IsAny<MonitorInfo>())).Returns(Task.FromResult(new List<string> { "CUSTOM","123" }));
            deviceManagerMock.Setup(x => x.SetVCPCapability(It.IsAny<MonitorInfo>(), It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult(true));

            brightnessViewModel.AutoBrightnessStatus = myAutoBrightnessStatus;
            Assert.That(brightnessViewModel.AutoBrightnessStatus, Is.EqualTo(myAutoBrightnessStatus));
        }


        [Test]
        [Apartment(ApartmentState.STA)]
        public void TestUpdate_AutoBrightnessStatus()
        {
            var brightnessViewModel = new BrightnessViewModel();
            brightnessViewModel.Update_AutoBrightnessStatus(true);
            var result= brightnessViewModel.AutoBrightnessStatus;
            Assert.That(result, Is.EqualTo(false));

            brightnessViewModel.Update_AutoBrightnessStatus(false);
            result = brightnessViewModel.AutoBrightnessStatus;
            Assert.That(result, Is.EqualTo(false));

        }


        [Test]
        [Apartment(ApartmentState.STA)]
        public void TestUpdate_AutoBrightnessRangeLevelStatus()
        {
            var brightnessViewModel = new BrightnessViewModel();
            var myAutoBrightnessLevel = new List<AutoBrightnessRangeLevel> { new AutoBrightnessRangeLevel() { level_name = "12", level_value = 34 } };
            brightnessViewModel.Update_AutoBrightnessRangeLevelStatus(myAutoBrightnessLevel);
            privateObject = new PrivateObject(brightnessViewModel);
            var _autoBrightnessRangeLevel = privateObject.GetFieldOrProperty("_autoBrightnessRangeLevel");
            
            Assert.That(_autoBrightnessRangeLevel, Is.Not.Null);
        }



        [Test]
        [Apartment(ApartmentState.STA)]
        public void TestAutoBrightnessSelectedIndex()
        {
            var brightnessViewModel = new BrightnessViewModel();
            var myAutoBrightnessSelectedIndex = 1;
            brightnessViewModel.AutoBrightnessSelectedIndex = myAutoBrightnessSelectedIndex;

            Assert.That(brightnessViewModel.AutoBrightnessSelectedIndex, Is.EqualTo(myAutoBrightnessSelectedIndex));
        }


        [Test]
        [Apartment(ApartmentState.STA)]
        public void TestAutoBrightnessRangeLevel()
        {
            var brightnessViewModel = new BrightnessViewModel();
            var myAutoBrightnessRangeLevel = new List<string>() { "Low", "Mid", "High" };
            brightnessViewModel.AutoBrightnessRangeLevel=myAutoBrightnessRangeLevel;
            
            Assert.That(brightnessViewModel.AutoBrightnessRangeLevel, Is.EqualTo(myAutoBrightnessRangeLevel));
        }




        [Test]
        [Apartment(ApartmentState.STA)]
        public void TestPrimaryMonitorForSyncVisible()
        {
            var brightnessViewModel = new BrightnessViewModel();
            Visibility result = brightnessViewModel.PrimaryMonitorForSyncVisible;
            Assert.That(result, Is.EqualTo((Visibility)0));
        }       


        [Test]
        [Apartment(ApartmentState.STA)]
        public void TestPrimaryMonitorForSyncVisible_invert()
        {
            var brightnessViewModel = new BrightnessViewModel();
            Visibility result= brightnessViewModel.PrimaryMonitorForSyncVisible_invert;
            Assert.That(result, Is.EqualTo((Visibility)2));
        }



        [Test]
        [Apartment(ApartmentState.STA)]
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
        [Apartment(ApartmentState.STA)]
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
