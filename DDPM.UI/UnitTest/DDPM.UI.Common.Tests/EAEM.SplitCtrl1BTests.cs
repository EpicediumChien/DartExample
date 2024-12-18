using DDPM.UI.Common.EAEM;
using DDPM.UI.Common.Interfaces;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.UX.WPF;
using Moq;

namespace DDPM.UI.Common.Tests
{
    [Apartment(ApartmentState.STA)]
    public class SplitCtrl1BTests
    {
        private SplitCtrl1B? splitCtrl1B;

        [SetUp]
        public void Setup()
        {
            splitCtrl1B = new SplitCtrl1B();
        }

        [Test]
        public void TestConstructor_SplitCtrl1B()
        {
            // Assert
            Assert.That(splitCtrl1B, Is.Not.Null);
        }

        [Test]
        public void TestNew()
        {
            // Act
            var result = splitCtrl1B.New();
            // Assert
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void TestGetSettings()
        {
            // Act
            var result = splitCtrl1B.GetSettings();
            // Assert
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void TestSetSettings()
        {
            // Act
            var result= splitCtrl1B.SetSettings(new List<double>());
            // Assert
            Assert.That(result, Is.EqualTo(true));
        }

        [Test]
        public void TestPbpCapabilityCode()
        {
            // Act
            splitCtrl1B.PbpCapabilityCode = 0x0001;

            // Assert
            Assert.That(splitCtrl1B.PbpCapabilityCode, Is.EqualTo(0x0001));
        }

        [Test]
        public void TestDescription()
        {
            // Act
            splitCtrl1B.Description = "";
            // Assert
            Assert.That(splitCtrl1B.Description, Is.EqualTo($""));

            // Act
            splitCtrl1B.Description = "Description";
            // Assert
            Assert.That(splitCtrl1B.Description, Is.EqualTo("Description"));
        }
        





    }
}