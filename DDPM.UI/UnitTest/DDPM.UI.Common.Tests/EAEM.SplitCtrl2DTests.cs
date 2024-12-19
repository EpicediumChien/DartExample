using DDPM.UI.Common.EAEM;
using DDPM.UI.Common.Interfaces;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.UX.WPF;
using Moq;

namespace DDPM.UI.Common.Tests
{
    [Apartment(ApartmentState.STA)]
    public class SplitCtrl2DTests
    {
        private SplitCtrl2D? splitCtrl2D;

        [SetUp]
        public void Setup()
        {
            splitCtrl2D = new SplitCtrl2D();
        }

        [Test]
        public void TestConstructor_SplitCtrl2D()
        {
            // Assert
            Assert.That(splitCtrl2D, Is.Not.Null);
        }

        [Test]
        public void TestNew()
        {
            // Act
            var result = splitCtrl2D.New();
            // Assert
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void TestGetSettings()
        {
            // Act
            var result = splitCtrl2D.GetSettings();
            // Assert
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void TestSetSettings()
        {
            // Act
            var result= splitCtrl2D.SetSettings(null);
            // Assert
            Assert.That(result, Is.EqualTo(false));

            // Act
            result = splitCtrl2D.SetSettings(new List<double>() { 2.0,2.1,2.2});
            // Assert
            Assert.That(result, Is.EqualTo(true));
        }

        [Test]
        public void TestPbpCapabilityCode()
        {
            // Act
            splitCtrl2D.PbpCapabilityCode = 0x0001;

            // Assert
            Assert.That(splitCtrl2D.PbpCapabilityCode, Is.EqualTo(0x0001));
        }

        [Test]
        public void TestDescription()
        {
            //// Act
            //splitCtrl2D.Description = "";
            //splitCtrl2D.PbpCapabilityCode = 0x001;
            //var result = splitCtrl2D.Description;
            //// Assert
            //Assert.That(result, Is.EqualTo($"PBP Capability Code={splitCtrl2D.PbpCapabilityCode:X02}h"));

            // Act
            splitCtrl2D.Description = "Description";
            // Assert
            Assert.That(splitCtrl2D.Description, Is.EqualTo("Description"));
        }

    }
}