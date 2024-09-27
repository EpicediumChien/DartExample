using DDPM.UI.Common.EAEM;
using DDPM.UI.Common.Interfaces;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.UX.WPF;
using Moq;

namespace DDPM.UI.Common.Tests
{
    [Apartment(ApartmentState.STA)]
    public class SplitCtrl3BTests
    {
        private SplitCtrl3B? splitCtrl3B;

        [SetUp]
        public void Setup()
        {
            splitCtrl3B = new SplitCtrl3B();
        }

        [Test]
        public void TestConstructor_SplitCtrl3B()
        {
            // Assert
            Assert.That(splitCtrl3B, Is.Not.Null);
        }

        [Test]
        public void TestNew()
        {
            // Act
            var result = splitCtrl3B.New();
            // Assert
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void TestGetSettings()
        {
            // Act
            var result = splitCtrl3B.GetSettings();
            // Assert
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void TestSetSettings()
        {
            // Act
            var result= splitCtrl3B.SetSettings(null);
            // Assert
            Assert.That(result, Is.EqualTo(false));

            // Act
            result = splitCtrl3B.SetSettings(new List<double>() { 2.0,2.1,2.2});
            // Assert
            Assert.That(result, Is.EqualTo(true));
        }

        [Test]
        public void TestPbpCapabilityCode()
        {
            // Act
            splitCtrl3B.PbpCapabilityCode = 0x0001;

            // Assert
            Assert.That(splitCtrl3B.PbpCapabilityCode, Is.EqualTo(0x0001));
        }

        [Test]
        public void TestDescription()
        {
            // Act
            splitCtrl3B.Description = "";
            splitCtrl3B.PbpCapabilityCode = 0x001;
            var result = splitCtrl3B.Description;
            // Assert
            Assert.That(result, Is.EqualTo($"PBP Capability Code={splitCtrl3B.PbpCapabilityCode:X02}h"));

            // Act
            splitCtrl3B.Description = "Description";
            // Assert
            Assert.That(splitCtrl3B.Description, Is.EqualTo("Description"));
        }

    }
}