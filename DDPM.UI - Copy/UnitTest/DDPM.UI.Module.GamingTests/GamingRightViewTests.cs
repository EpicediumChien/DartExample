namespace DDPM.UI.Module.Gaming.Tests
{
    [TestFixture, Apartment(ApartmentState.STA)]
    public class GamingRightViewTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void TestConstructor_InitializesComponent()
        {
            GamingRightView gamingRightView = new GamingRightView();
            // Assert
            Assert.That(gamingRightView, Is.Not.Null);
        }
    }
}