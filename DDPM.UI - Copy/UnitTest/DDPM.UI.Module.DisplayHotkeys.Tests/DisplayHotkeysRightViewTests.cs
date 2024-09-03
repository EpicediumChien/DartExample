using NGA.UnitTest.PrivateObject;

namespace DDPM.UI.Module.DisplayHotkeys.Tests
{
    [TestFixture, Apartment(ApartmentState.STA)]
    public class DisplayHotkeysRightViewTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void TestConstructor_DisplayHotkeysRightView()
        {
            var displayHotkeysViewModel = new DisplayHotkeysViewModel();
            var displayHotkeysRightView = new DisplayHotkeysRightView(displayHotkeysViewModel);
            var privateObject = new PrivateObject(displayHotkeysRightView);
            var result = privateObject.GetFieldOrProperty("vm");

            // Assert
            Assert.That(displayHotkeysRightView.DataContext, Is.EqualTo(result));
        }
    }
}