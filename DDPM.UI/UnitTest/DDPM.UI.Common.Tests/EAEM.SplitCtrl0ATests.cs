using DDPM.UI.Common.EAEM;
using DDPM.UI.Common.Interfaces;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.UX.WPF;
using Moq;

namespace DDPM.UI.Common.Tests
{
    [Apartment(ApartmentState.STA)]
    public class SplitCtrl0ATests
    {
        private SplitCtrl0A? splitCtrl0A;
        private ISplit? iSplit;
        private Mock<ISplit>? iSplitMock;

        [SetUp]
        public void Setup()
        {
            iSplitMock= new Mock<ISplit>();
            iSplit=iSplitMock.Object;
            splitCtrl0A = new SplitCtrl0A();
        }

        [Test]
        public void TestConstructor_SplitCtrl0A()
        {
            // Assert
            Assert.That(splitCtrl0A, Is.Not.Null);
        }

        [Test]
        public void TestNew()
        {
            // Act
            var result = splitCtrl0A.New();
            // Assert
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void TestGetSettings()
        {
            // Act
            var result = splitCtrl0A.GetSettings();
            // Assert
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void TestSetSettings()
        {
            // Act
            var result= splitCtrl0A.SetSettings(new List<double>());
            // Assert
            Assert.That(result, Is.EqualTo(true));
        }

        [Test]
        public void TestPbpCapabilityCode()
        {
            // Act
            splitCtrl0A.PbpCapabilityCode = 0x0001;

            // Assert
            Assert.That(splitCtrl0A.PbpCapabilityCode, Is.EqualTo(0x0001));
        }

        [Test]
        public void TestDescription()
        {
            // Act
            splitCtrl0A.Description = "";
            // Assert
            Assert.That(splitCtrl0A.Description, Is.EqualTo($""));

            // Act
            splitCtrl0A.Description = "0x0001";
            // Assert
            Assert.That(splitCtrl0A.Description, Is.EqualTo("0x0001"));
        }
        





    }
}