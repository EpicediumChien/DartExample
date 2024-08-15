using NGA.UnitTest.PrivateObject;

namespace DDPM.UI.Module.PipPbp.Tests
{
    [Apartment(ApartmentState.STA)]
    public class PipPbpLeftViewTests
    {
        private PipPbpLeftView? pipPbpLeftView;
        private PrivateObject? privateObject;

        [SetUp]
        public void Setup()
        {
            pipPbpLeftView = new PipPbpLeftView();
            privateObject = new PrivateObject(pipPbpLeftView);
        }

        [Test]
        public void TestPipPbpLeftViewInitialization()
        {
            // Assert
            Assert.That(pipPbpLeftView, Is.Not.Null);
        }
    }
}