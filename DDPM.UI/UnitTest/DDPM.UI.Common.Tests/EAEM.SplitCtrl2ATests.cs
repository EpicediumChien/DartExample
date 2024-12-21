using DDPM.UI.Common.EAEM;
using DDPM.UI.Common.Interfaces;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.UX.WPF;
using Moq;

namespace DDPM.UI.Common.Tests
{
    [Apartment(ApartmentState.STA)]
    public class SplitCtrl2ATests
    {
        private SplitCtrl2A? splitCtrl2A;

        [SetUp]
        public void Setup()
        {
            splitCtrl2A = new SplitCtrl2A();
        }

        [Test]
        public void TestConstructor_SplitCtrl2A()
        {
            // Assert
            Assert.That(splitCtrl2A, Is.Not.Null);
        }

        [Test]
        public void TestNew()
        {
            // Act
            var result = splitCtrl2A.New();
            // Assert
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void TestGetSettings()
        {
            // Act
            var result = splitCtrl2A.GetSettings();
            // Assert
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void TestSetSettings()
        {
            // Act
            var result= splitCtrl2A.SetSettings(null);
            // Assert
            Assert.That(result, Is.EqualTo(false));

            // Act
            result = splitCtrl2A.SetSettings(new List<double>() { 2.0,2.1,2.2});
            // Assert
            Assert.That(result, Is.EqualTo(true));
        }

        [Test]
        public void TestPbpCapabilityCode()
        {
            // Act
            splitCtrl2A.PbpCapabilityCode = 0x0001;

            // Assert
            Assert.That(splitCtrl2A.PbpCapabilityCode, Is.EqualTo(0x0001));
        }

        [Test]
        public void TestDescription()
        {
            //// Act
            //splitCtrl2A.Description = "";
            //splitCtrl2A.PbpCapabilityCode = 0x001;
            //var result = splitCtrl2A.Description;
            //// Assert
            //Assert.That(result, Is.EqualTo($"PBP Capability Code={splitCtrl2A.PbpCapabilityCode:X02}h"));

            // Act
            splitCtrl2A.Description = "Description";
            // Assert
            Assert.That(splitCtrl2A.Description, Is.EqualTo("Description"));
        }

    }
}