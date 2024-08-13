using DDPM.UI.Common.Interfaces;
using DDPM.UI.Common;
using Moq;
using NGA.UnitTest.PrivateObject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RegistryUtils;
using static DDPM.UI.Module.Color.ColorViewModel;
using DDPM.SA.Common;
using NSubstitute;
namespace DDPM.UI.Module.Color.Tests
{
    [Apartment(ApartmentState.STA)]
    public class MonitorProfileTests
    {

        private RegistryUtils.MonitorProfile? monitorProfile;
        private PrivateObject? privateObject;
        private Mock<IModuleOwner>? moduleOwnerMock;


        [SetUp]
        public void Setup()
        {
            moduleOwnerMock = new Mock<IModuleOwner>();
            var moduleOwner = moduleOwnerMock!.Object;
            DdpmCommonHelper.ModuleOwner = moduleOwner;
            monitorProfile = new RegistryUtils.MonitorProfile();
            privateObject = new PrivateObject(monitorProfile);
        }


        [Test]
        public void TestGetMonitorProfile()
        {
            // Act
            var profileName = RegistryUtils.MonitorProfile.GetMonitorProfile();

            // Assert
            Assert.That(monitorProfile, Is.Not.Null); 
            Assert.That(profileName, Is.Not.Null);
        }

        //[Test]
        //public void TestSetMonitorProfile()
        //{
            // Act
            //var bRes = RegistryUtils.MonitorProfile.SetMonitorProfile("Dell_U3224KB_Native_v2.icm");

            // Assert
            //Assert.IsFalse(bRes);

            //bRes = MonitorProfile.SetMonitorProfile("Dell_U3224KB_DisplayP3_v2.icm");
            //Assert.IsFalse(bRes);


            //bRes = MonitorProfile.SetMonitorProfile("Dell_U3224KB_DCIP3_v2.icm");
            //Assert.IsFalse(bRes);


            //bRes = MonitorProfile.SetMonitorProfile("Dell_U3224KB_sRGB_v2.icm");
            //Assert.IsFalse(bRes);


            //bRes = MonitorProfile.SetMonitorProfile("Dell_U3224KB_Rec709_v2.icm");
            //Assert.IsFalse(bRes);                            
       // }

        //[Test]
        //public void TestIntsallMonitorProfile()
        //{
        //    // Act
        //    //var colorViewModel = new ColorViewModel();
        //    var deviceManagerSAMock = new Mock<IDeviceManagerSA>();
        //    DdpmCommonHelper.DeviceManagerSA = deviceManagerSAMock.Object;
        //    deviceManagerSAMock.Setup(x => x.GetAppIconFolderPath()).Returns(Task.FromResult(""));
        //    string savePath = DdpmCommonHelper.DeviceManagerSA.GetAppIconFolderPath().Result;
        //    savePath = savePath.Substring(0, savePath.Length - 6);
        //    var strICM_Folder = savePath + "\\";
        //    var result = RegistryUtils.MonitorProfile.IntsallMonitorProfile(strICM_Folder + colorViewModel._ICC_Metadata._support_ICC_DeviceName["U3224KB"][0].File);

        //    // Assert
        //    //Assert.That(monitorProfile, Is.Not.Null);
        //}
        


        [Test]
        public void TestGetColorDirectory()
        {
            // Act
            var result = RegistryUtils.MonitorProfile.GetColorDirectory();

            // Assert
            Assert.That(result, Is.Not.Null);
        }


    }
}
