using NGA.UnitTest.PrivateObject;

namespace DDPM.UI.Module.DisplayProperties.Tests
{
    [Apartment(ApartmentState.STA)]
    public class DisplayPropertiesRightViewTests
    {
        private DisplayPropertiesRightView? displayPropertiesRightView;
        private PrivateObject? privateObject;

        [SetUp]
        public void SetUp()
        {
            displayPropertiesRightView = new DisplayPropertiesRightView();
            privateObject = new PrivateObject(displayPropertiesRightView);
        }

        [Test]
        public void TestConstructor_InitializesComponent()
        {
            // Assert
            Assert.That(displayPropertiesRightView, Is.Not.Null);
        }
    }
}