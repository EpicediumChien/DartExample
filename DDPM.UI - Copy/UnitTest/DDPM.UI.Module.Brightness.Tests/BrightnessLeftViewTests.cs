using NUnit.Framework;

namespace DDPM.UI.Module.Brightness.Tests
{
    [Apartment(ApartmentState.STA)]
    public class BrightnessLeftViewTests
    {
        private BrightnessLeftView? brightnessLeftView;

        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void TestConstructor_InitializesComponent()
        {
            brightnessLeftView = new BrightnessLeftView();
            // Assert
            Assert.That(brightnessLeftView, Is.Not.Null);
        }
    }
}