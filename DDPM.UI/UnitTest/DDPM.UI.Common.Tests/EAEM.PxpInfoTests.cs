using DDPM.UI.Common.EAEM;
using DDPM.UI.Common.Interfaces;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.UX.WPF;

namespace DDPM.UI.Common.Tests
{
    [Apartment(ApartmentState.STA)]
    public class PxpInfoTests
    {
        private PxpInfo? pxpInfo;

        [SetUp]
        public void Setup()
        {
            pxpInfo = new PxpInfo();
        }

        [Test]
        public void TestConstructor_PxpInfo()
        {
            // Assert
            Assert.That(pxpInfo, Is.Not.Null);
        }

        [Test]
        public void TestDescription()
        {
            // Act
            pxpInfo.Description = "Description";
            // Assert
            Assert.That(pxpInfo.Description, Is.EqualTo("Description"));
        }       
    }
}