using DDPM.UI.Common.EAEM;
using DDPM.UI.Common.Interfaces;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.UX.WPF;
using Moq;

namespace DDPM.UI.Common.Tests
{
    [Apartment(ApartmentState.STA)]
    public class SplitCtrl3DTests
    {
        private SplitCtrl3D? splitCtrl3D;

        [SetUp]
        public void Setup()
        {
            splitCtrl3D = new SplitCtrl3D();
        }

        [Test]
        public void TestConstructor_SplitCtrl3D()
        {
            // Assert
            Assert.That(splitCtrl3D, Is.Not.Null);
        }

        [Test]
        public void TestNew()
        {
            // Act
            var result = splitCtrl3D.New();
            // Assert
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void TestGetSettings()
        {
            // Act
            var result = splitCtrl3D.GetSettings();
            // Assert
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void TestSetSettings()
        {
            // Act
            var result= splitCtrl3D.SetSettings(null);
            // Assert
            Assert.That(result, Is.EqualTo(false));

            // Act
            result = splitCtrl3D.SetSettings(new List<double>() { 2.0, 2.1, 2.2, 2.3, 2.4 });
            // Assert
            Assert.That(result, Is.EqualTo(true));
        }

        [Test]
        public void TestPbpCapabilityCode()
        {
            // Act
            splitCtrl3D.PbpCapabilityCode = 0x0001;

            // Assert
            Assert.That(splitCtrl3D.PbpCapabilityCode, Is.EqualTo(0x0001));
        }

        [Test]
        public void TestDescription()
        {
            //// Act
            //splitCtrl3D.Description = "";
            //splitCtrl3D.PbpCapabilityCode = 0x001;
            //var result = splitCtrl3D.Description;
            //// Assert
            //Assert.That(result, Is.EqualTo($"PBP Capability Code={splitCtrl3D.PbpCapabilityCode:X02}h"));

            // Act
            splitCtrl3D.Description = "Description";
            // Assert
            Assert.That(splitCtrl3D.Description, Is.EqualTo("Description"));
        }

    }
}