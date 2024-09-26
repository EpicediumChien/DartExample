using DDPM.UI.Common.EAEM;
using DDPM.UI.Common.Interfaces;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.UX.WPF;
using Moq;

namespace DDPM.UI.Common.Tests
{
    [Apartment(ApartmentState.STA)]
    public class SplitCtrl4ATests
    {
        private SplitCtrl4A? splitCtrl4A;

        [SetUp]
        public void Setup()
        {
            splitCtrl4A = new SplitCtrl4A();
        }

        [Test]
        public void TestConstructor_SplitCtrl3B()
        {
            // Assert
            Assert.That(splitCtrl4A, Is.Not.Null);
        }

        [Test]
        public void TestNew()
        {
            // Act
            var result = splitCtrl4A.New();
            // Assert
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void TestGetSettings()
        {
            // Act
            var result = splitCtrl4A.GetSettings();
            // Assert
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void TestSetSettings()
        {
            // Act
            var result= splitCtrl4A.SetSettings(null);
            // Assert
            Assert.That(result, Is.EqualTo(false));

            // Act
            result = splitCtrl4A.SetSettings(new List<double>() { 2.0, 2.1, 2.2, 2.3, 2.4 });
            // Assert
            Assert.That(result, Is.EqualTo(true));
        }

        [Test]
        public void TestPbpCapabilityCode()
        {
            // Act
            splitCtrl4A.PbpCapabilityCode = 0x0001;

            // Assert
            Assert.That(splitCtrl4A.PbpCapabilityCode, Is.EqualTo(0x0001));
        }

        [Test]
        public void TestDescription()
        {
            // Act
            splitCtrl4A.Description = "";
            splitCtrl4A.PbpCapabilityCode = 0x001;
            var result = splitCtrl4A.Description;
            // Assert
            Assert.That(result, Is.EqualTo($"PBP Capability Code={splitCtrl4A.PbpCapabilityCode:X02}h"));

            // Act
            splitCtrl4A.Description = "Description";
            // Assert
            Assert.That(splitCtrl4A.Description, Is.EqualTo("Description"));
        }

    }
}