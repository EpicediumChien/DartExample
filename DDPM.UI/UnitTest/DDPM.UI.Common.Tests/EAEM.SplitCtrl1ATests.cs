using DDPM.UI.Common.EAEM;
using DDPM.UI.Common.Interfaces;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.UX.WPF;
using Moq;

namespace DDPM.UI.Common.Tests
{
    [Apartment(ApartmentState.STA)]
    public class SplitCtrl1ATests
    {
        private SplitCtrl1A? splitCtrl1A;
        private ISplit? iSplit;
        private Mock<ISplit>? iSplitMock;

        [SetUp]
        public void Setup()
        {
            iSplitMock= new Mock<ISplit>();
            iSplit=iSplitMock.Object;
            splitCtrl1A = new SplitCtrl1A();
        }

        [Test]
        public void TestConstructor_SplitCtrl1A()
        {
            // Assert
            Assert.That(splitCtrl1A, Is.Not.Null);
        }

        [Test]
        public void TestNew()
        {
            // Act
            var result = splitCtrl1A.New();
            // Assert
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void TestGetSettings()
        {
            // Act
            var result = splitCtrl1A.GetSettings();
            // Assert
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void TestSetSettings()
        {
            // Act
            var result= splitCtrl1A.SetSettings(new List<double>());
            // Assert
            Assert.That(result, Is.EqualTo(true));
        }

        [Test]
        public void TestPbpCapabilityCode()
        {
            // Act
            splitCtrl1A.PbpCapabilityCode = 0x0001;

            // Assert
            Assert.That(splitCtrl1A.PbpCapabilityCode, Is.EqualTo(0x0001));
        }

        [Test]
        public void TestDescription()
        {
            // Act
            splitCtrl1A.Description = "";
            // Assert
            Assert.That(splitCtrl1A.Description, Is.EqualTo($"PIP Small"));

            // Act
            splitCtrl1A.Description = "0x0001";
            // Assert
            Assert.That(splitCtrl1A.Description, Is.EqualTo("0x0001"));
        }
        





    }
}