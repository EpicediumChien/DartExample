using DDPM.UI.Common.EAEM;
using DDPM.UI.Common.Interfaces;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.UX.WPF;
using Moq;

namespace DDPM.UI.Common.Tests
{
    [Apartment(ApartmentState.STA)]
    public class SplitCtrl3HTests
    {
        private SplitCtrl3H? splitCtrl3H;

        [SetUp]
        public void Setup()
        {
            splitCtrl3H = new SplitCtrl3H();
        }

        [Test]
        public void TestConstructor_SplitCtrl3B()
        {
            // Assert
            Assert.That(splitCtrl3H, Is.Not.Null);
        }

        [Test]
        public void TestNew()
        {
            // Act
            var result = splitCtrl3H.New();
            // Assert
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void TestGetSettings()
        {
            // Act
            var result = splitCtrl3H.GetSettings();
            // Assert
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void TestSetSettings()
        {
            // Act
            var result= splitCtrl3H.SetSettings(null);
            // Assert
            Assert.That(result, Is.EqualTo(false));

            // Act
            result = splitCtrl3H.SetSettings(new List<double>() { 2.0, 2.1, 2.2, 2.3, 2.4 });
            // Assert
            Assert.That(result, Is.EqualTo(true));
        }

        [Test]
        public void TestPbpCapabilityCode()
        {
            // Act
            splitCtrl3H.PbpCapabilityCode = 0x0001;

            // Assert
            Assert.That(splitCtrl3H.PbpCapabilityCode, Is.EqualTo(0x0001));
        }

        [Test]
        public void TestDescription()
        {
            //// Act
            //splitCtrl3H.Description = "";
            //splitCtrl3H.PbpCapabilityCode = 0x001;
            //var result = splitCtrl3H.Description;
            //// Assert
            //Assert.That(result, Is.EqualTo($"PBP Capability Code={splitCtrl3H.PbpCapabilityCode:X02}h"));

            // Act
            splitCtrl3H.Description = "Description";
            // Assert
            Assert.That(splitCtrl3H.Description, Is.EqualTo("Description"));
        }

    }
}