using DDPM.UI.Common.EAEM;
using DDPM.UI.Common.Interfaces;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.UX.WPF;
using Moq;

namespace DDPM.UI.Common.Tests
{
    [Apartment(ApartmentState.STA)]
    public class SplitCtrl3ETests
    {
        private SplitCtrl3E? splitCtrl3E;

        [SetUp]
        public void Setup()
        {
            splitCtrl3E = new SplitCtrl3E();
        }

        [Test]
        public void TestConstructor_SplitCtrl3E()
        {
            // Assert
            Assert.That(splitCtrl3E, Is.Not.Null);
        }

        [Test]
        public void TestNew()
        {
            // Act
            var result = splitCtrl3E.New();
            // Assert
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void TestGetSettings()
        {
            // Act
            var result = splitCtrl3E.GetSettings();
            // Assert
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void TestSetSettings()
        {
            // Act
            var result= splitCtrl3E.SetSettings(null);
            // Assert
            Assert.That(result, Is.EqualTo(false));

            // Act
            result = splitCtrl3E.SetSettings(new List<double>() { 2.0, 2.1, 2.2, 2.3, 2.4 });
            // Assert
            Assert.That(result, Is.EqualTo(true));
        }

        [Test]
        public void TestPbpCapabilityCode()
        {
            // Act
            splitCtrl3E.PbpCapabilityCode = 0x0001;

            // Assert
            Assert.That(splitCtrl3E.PbpCapabilityCode, Is.EqualTo(0x0001));
        }

        [Test]
        public void TestDescription()
        {
            // Act
            splitCtrl3E.Description = "";
            splitCtrl3E.PbpCapabilityCode = 0x001;
            var result = splitCtrl3E.Description;
            // Assert
            Assert.That(result, Is.EqualTo($"PBP Capability Code={splitCtrl3E.PbpCapabilityCode:X02}h"));

            // Act
            splitCtrl3E.Description = "Description";
            // Assert
            Assert.That(splitCtrl3E.Description, Is.EqualTo("Description"));
        }

    }
}