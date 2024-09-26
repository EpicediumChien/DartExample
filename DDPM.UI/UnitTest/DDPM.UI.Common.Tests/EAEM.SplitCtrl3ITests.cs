using DDPM.UI.Common.EAEM;
using DDPM.UI.Common.Interfaces;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.UX.WPF;
using Moq;

namespace DDPM.UI.Common.Tests
{
    [Apartment(ApartmentState.STA)]
    public class SplitCtrl3ITests
    {
        private SplitCtrl3I? splitCtrl3I;

        [SetUp]
        public void Setup()
        {
            splitCtrl3I = new SplitCtrl3I();
        }

        [Test]
        public void TestConstructor_SplitCtrl3I()
        {
            // Assert
            Assert.That(splitCtrl3I, Is.Not.Null);
        }

        [Test]
        public void TestNew()
        {
            // Act
            var result = splitCtrl3I.New();
            // Assert
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void TestGetSettings()
        {
            // Act
            var result = splitCtrl3I.GetSettings();
            // Assert
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void TestSetSettings()
        {
            // Act
            var result= splitCtrl3I.SetSettings(null);
            // Assert
            Assert.That(result, Is.EqualTo(false));

            // Act
            result = splitCtrl3I.SetSettings(new List<double>() { 2.0, 2.1, 2.2, 2.3, 2.4 });
            // Assert
            Assert.That(result, Is.EqualTo(true));
        }

        [Test]
        public void TestPbpCapabilityCode()
        {
            // Act
            splitCtrl3I.PbpCapabilityCode = 0x0001;

            // Assert
            Assert.That(splitCtrl3I.PbpCapabilityCode, Is.EqualTo(0x0001));
        }

        [Test]
        public void TestDescription()
        {
            // Act
            splitCtrl3I.Description = "";
            splitCtrl3I.PbpCapabilityCode = 0x001;
            var result = splitCtrl3I.Description;
            // Assert
            Assert.That(result, Is.EqualTo($"PBP Capability Code={splitCtrl3I.PbpCapabilityCode:X02}h"));

            // Act
            splitCtrl3I.Description = "Description";
            // Assert
            Assert.That(splitCtrl3I.Description, Is.EqualTo("Description"));
        }

    }
}