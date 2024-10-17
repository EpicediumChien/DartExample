using DDPM.UI.Common.EAEM;
using DDPM.UI.Common.Interfaces;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.UX.WPF;
using Moq;

namespace DDPM.UI.Common.Tests
{
    [Apartment(ApartmentState.STA)]
    public class SplitCtrl4DTests
    {
        private SplitCtrl4D? splitCtrl4D;

        [SetUp]
        public void Setup()
        {
            splitCtrl4D = new SplitCtrl4D();
        }

        [Test]
        public void TestConstructor_SplitCtrl4D()
        {
            // Assert
            Assert.That(splitCtrl4D, Is.Not.Null);
        }

        [Test]
        public void TestNew()
        {
            // Act
            var result = splitCtrl4D.New();
            // Assert
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void TestGetSettings()
        {
            // Act
            var result = splitCtrl4D.GetSettings();
            // Assert
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void TestSetSettings()
        {
            // Act
            var result= splitCtrl4D.SetSettings(null);
            // Assert
            Assert.That(result, Is.EqualTo(false));

            // Act
            result = splitCtrl4D.SetSettings(new List<double>() { 2.0,2.1,2.2,2.3,2.4});
            // Assert
            Assert.That(result, Is.EqualTo(true));
        }

        [Test]
        public void TestPbpCapabilityCode()
        {
            // Act
            splitCtrl4D.PbpCapabilityCode = 0x0001;

            // Assert
            Assert.That(splitCtrl4D.PbpCapabilityCode, Is.EqualTo(0x0001));
        }

        [Test]
        public void TestDescription()
        {
            // Act
            splitCtrl4D.Description = "";
            splitCtrl4D.PbpCapabilityCode = 0x001;
            var result = splitCtrl4D.Description;
            // Assert
            Assert.That(result, Is.EqualTo($"PBP Capability Code={splitCtrl4D.PbpCapabilityCode:X02}h"));

            // Act
            splitCtrl4D.Description = "Description";
            // Assert
            Assert.That(splitCtrl4D.Description, Is.EqualTo("Description"));
        }

    }
}