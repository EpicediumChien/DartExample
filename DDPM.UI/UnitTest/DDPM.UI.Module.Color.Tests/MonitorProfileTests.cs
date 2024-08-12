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
namespace DDPM.UI.Module.Color.Tests
{
    public class MonitorProfileTests
    {

        [SetUp]
        public void Setup()
        {

        }


        [Test]
        [Apartment(ApartmentState.STA)]
        public void TestGetMonitorProfile()
        {
            // Act
            var profileName = MonitorProfile.GetMonitorProfile();

            // Assert
            Assert.That(profileName, Is.Not.Null);
        }


        //[Test]
        //[Apartment(ApartmentState.STA)]
        //public void TestSetMonitorProfile()    //only support Dell_U3224KB monitor
        //{

        // Act
        //var bRes = MonitorProfile.SetMonitorProfile("Dell_U3224KB_Native_v2.icm");

        // Assert
        //Assert.IsTrue(bRes);

        //}


        [Test]
        [Apartment(ApartmentState.STA)]
        public void TestIntsallMonitorProfile()
        {
            var profileName = MonitorProfile.GetMonitorProfile();
            Assert.That(profileName, Is.Not.Null);

            var result = MonitorProfile.IntsallMonitorProfile(profileName);
            Assert.IsTrue(result);
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
