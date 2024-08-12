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
using DDPM.SA.Common;
using MonitorProfile = DDPM.SA.Common.MonitorProfile;


namespace DDPM.UI.Module.Color.Tests
{
    public class MonitorProfileTest
    {
        private MonitorProfile? monitorProfile;
        private PrivateObject? privateObject;
        private Mock<IModuleOwner>? moduleOwnerMock;


        [SetUp]
        public void Setup()
        {
             moduleOwnerMock = new Mock<IModuleOwner>();
             var moduleOwner = moduleOwnerMock!.Object;
             DdpmCommonHelper.ModuleOwner = moduleOwner;
             monitorProfile = new MonitorProfile();
             privateObject = new PrivateObject(monitorProfile);
        }


        [Test]
        [Apartment(ApartmentState.STA)]
        public void TestGetMonitorProfile()
        {
            // Act
            //var profileName= MonitorProfile.GetMonitorProfile();

            // Assert
            //Assert.That(monitorProfile, Is.Not.Null);
        }

        [Test]
        [Apartment(ApartmentState.STA)]
        public void TestSetMonitorProfile()
        {
            // Act
            var bRes = MonitorProfile.SetMonitorProfile("Dell_U3224KB_Native_v2.icm");

            // Assert
            Assert.IsFalse(bRes);

            //bRes = MonitorProfile.SetMonitorProfile("Dell_U3224KB_DisplayP3_v2.icm");
            //Assert.IsFalse(bRes);


            //bRes = MonitorProfile.SetMonitorProfile("Dell_U3224KB_DCIP3_v2.icm");
            //Assert.IsFalse(bRes);


            //bRes = MonitorProfile.SetMonitorProfile("Dell_U3224KB_sRGB_v2.icm");
            //Assert.IsFalse(bRes);


            //bRes = MonitorProfile.SetMonitorProfile("Dell_U3224KB_Rec709_v2.icm");
            //Assert.IsFalse(bRes);                            

        }


        [Test]
        [Apartment(ApartmentState.STA)]
        public void TestGetColorDirectory()
        {
            // Act
            var result = MonitorProfile.GetColorDirectory();

            // Assert
            Assert.That(result, Is.Not.Null);
        }


    }
}
