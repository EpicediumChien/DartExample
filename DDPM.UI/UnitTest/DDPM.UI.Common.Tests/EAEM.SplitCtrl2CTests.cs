using DDPM.UI.Common.EAEM;
using DDPM.UI.Common.Interfaces;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.UX.WPF;
using Moq;

namespace DDPM.UI.Common.Tests
{
    [Apartment(ApartmentState.STA)]
    public class SplitCtrl2CTests
    {
        private SplitCtrl2C? splitCtrl2C;

        [SetUp]
        public void Setup()
        {
            splitCtrl2C = new SplitCtrl2C();
        }

        [Test]
        public void TestConstructor_SplitCtrl2C()
        {
            // Assert
            Assert.That(splitCtrl2C, Is.Not.Null);
        }

        [Test]
        public void TestNew()
        {
            // Act
            var result = splitCtrl2C.New();
            // Assert
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void TestGetSettings()
        {
            // Act
            var result = splitCtrl2C.GetSettings();
            // Assert
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void TestSetSettings()
        {
            // Act
            var result= splitCtrl2C.SetSettings(null);
            // Assert
            Assert.That(result, Is.EqualTo(false));

            // Act
            result = splitCtrl2C.SetSettings(new List<double>() { 2.0,2.1,2.2});
            // Assert
            Assert.That(result, Is.EqualTo(true));
        }

        [Test]
        public void TestPbpCapabilityCode()
        {
            // Act
            splitCtrl2C.PbpCapabilityCode = 0x0001;

            // Assert
            Assert.That(splitCtrl2C.PbpCapabilityCode, Is.EqualTo(0x0001));
        }

        [Test]
        public void TestDescription()
        {
            // Act
            splitCtrl2C.Description = "";
            splitCtrl2C.PbpCapabilityCode = 0x001;
            var result = splitCtrl2C.Description;
            // Assert
            Assert.That(result, Is.EqualTo($"PBP Capability Code={splitCtrl2C.PbpCapabilityCode:X02}h"));

            // Act
            splitCtrl2C.Description = "Description";
            // Assert
            Assert.That(splitCtrl2C.Description, Is.EqualTo("Description"));
        }

    }
}