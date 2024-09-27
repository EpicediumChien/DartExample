using DDPM.UI.Common.EAEM;
using DDPM.UI.Common.Interfaces;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.UX.WPF;

namespace DDPM.UI.Common.Tests
{
    [Apartment(ApartmentState.STA)]
    public class PxpItemTests
    {
        private PxpItem? pxpItem;

        [SetUp]
        public void Setup()
        {
            pxpItem = new PxpItem();
        }

        [Test]
        public void TestConstructor_PxpItem()
        {
            // Assert
            Assert.That(pxpItem, Is.Not.Null);
        }

        [Test]
        public void TestInnerContent()
        {
            // Act
            var InnerContent = new object();
            pxpItem.InnerContent= InnerContent;
            // Assert
            Assert.That(pxpItem.InnerContent, Is.EqualTo(InnerContent));
        }

        [Test]
        public void TestInnerContentProperty()
        {
            // Act
            var result=PxpItem.InnerContentProperty;
            // Assert
            Assert.That(result, Is.Not.Null);
        }

        
    }
}