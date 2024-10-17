using DDPM.UI.Common.EAEM;
using DDPM.UI.Common.Interfaces;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.UX.WPF;
using Moq;

namespace DDPM.UI.Common.Tests
{
    [Apartment(ApartmentState.STA)]
    public class SplitCtrl5ATests
    {
        private SplitCtrl5A? splitCtrl5A;

        [SetUp]
        public void Setup()
        {
            splitCtrl5A = new SplitCtrl5A();
        }

        [Test]
        public void TestConstructor_SplitCtrl5A()
        {
            // Assert
            Assert.That(splitCtrl5A, Is.Not.Null);
        }

    }
}