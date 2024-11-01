using DDPM.UI.Common.EAEM;
using DDPM.UI.Common.Interfaces;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.UX.WPF;

namespace DDPM.UI.Common.Tests
{
    [Apartment(ApartmentState.STA)]
    public class EDIDTests
    {
        private EDID? eDID;

        [SetUp]
        public void Setup()
        {
            eDID = new EDID();
        }

        [Test]
        public void TestConstructor_EDID()
        {
            // Assert
            Assert.That(eDID, Is.Not.Null);
        }

        [Test]
        public void TestManufactureID()
        {
            // Act
            eDID.ManufactureID = "ManufactureID";
            // Assert
            Assert.That(eDID.ManufactureID, Is.EqualTo("ManufactureID"));
        }

        [Test]
        public void TestVendorID()
        {
            // Act
            eDID.VendorID = "VendorID";
            // Assert
            Assert.That(eDID.VendorID, Is.EqualTo("VendorID"));
        }

        [Test]
        public void TestYear()
        {
            // Act
            eDID.Year = 2024;
            // Assert
            Assert.That(eDID.Year, Is.EqualTo(2024));
        }

        [Test]
        public void TestModelName()
        {
            // Act
            eDID.ModelName = "ModelName";
            // Assert
            Assert.That(eDID.ModelName, Is.EqualTo("ModelName"));
        }

        [Test]
        public void TestSize()
        {
            // Act
            eDID.Size = 1.2f;
            // Assert
            Assert.That(eDID.Size, Is.EqualTo(1.2f));
        }

        [Test]
        public void TestServiceTag()
        {
            // Act
            eDID.ServiceTag = "ServiceTag";
            // Assert
            Assert.That(eDID.ServiceTag, Is.EqualTo("ServiceTag"));
        }

        [Test]
        public void TestSerialNumber()
        {
            // Act
            eDID.SerialNumber = "SerialNumber";
            // Assert
            Assert.That(eDID.SerialNumber, Is.EqualTo("SerialNumber"));
        }

        [Test]
        public void TestEdid()
        {
            // Act
            eDID.Edid = "Edid";
            // Assert
            Assert.That(eDID.Edid, Is.EqualTo("Edid"));
        }
    }
}