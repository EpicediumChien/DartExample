using DDPM.UI.Common.EAEM;
using DDPM.UI.Common.Interfaces;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.UX.WPF;
using Moq;

namespace DDPM.UI.Common.Tests
{
    [Apartment(ApartmentState.STA)]
    public class SplitCtrl2BTests
    {
        private SplitCtrl2B? splitCtrl2B;

        [SetUp]
        public void Setup()
        {
            splitCtrl2B = new SplitCtrl2B();
        }

        [Test]
        public void TestConstructor_SplitCtrl2B()
        {
            // Assert
            Assert.That(splitCtrl2B, Is.Not.Null);
        }

        [Test]
        public void TestNew()
        {
            // Act
            var result = splitCtrl2B.New();
            // Assert
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void TestGetSettings()
        {
            // Act
            var result = splitCtrl2B.GetSettings();
            // Assert
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void TestSetSettings()
        {
            // Act
            var result= splitCtrl2B.SetSettings(null);
            // Assert
            Assert.That(result, Is.EqualTo(false));

            // Act
            result = splitCtrl2B.SetSettings(new List<double>() { 2.0,2.1,2.2});
            // Assert
            Assert.That(result, Is.EqualTo(true));
        }

        [Test]
        public void TestPbpCapabilityCode()
        {
            // Act
            splitCtrl2B.PbpCapabilityCode = 0x0001;

            // Assert
            Assert.That(splitCtrl2B.PbpCapabilityCode, Is.EqualTo(0x0001));
        }

        [Test]
        public void TestDescription()
        {
            //// Act
            //splitCtrl2B.Description = "";
            //splitCtrl2B.PbpCapabilityCode = 0x001;
            //var result = splitCtrl2B.Description;
            //// Assert
            //Assert.That(result, Is.EqualTo($"PBP Capability Code={splitCtrl2B.PbpCapabilityCode:X02}h"));

            // Act
            splitCtrl2B.Description = "Description";
            // Assert
            Assert.That(splitCtrl2B.Description, Is.EqualTo("Description"));
        }

    }
}