using DDPM.UI.Module.DisplayOthers;
using NGA.UnitTest.PrivateObject;
using NUnit.Framework;

namespace DDPM.UI.Module.DisplayProperties.Tests
{
    [TestFixture, Apartment(ApartmentState.STA)]
    public class DisplayOthersRightViewTests
    {
        private DisplayOthersRightView? displayOthersRightView;
        private PrivateObject? privateObject;

        [SetUp]
        public void SetUp()
        {
            DisplayOthersViewModel vm=new DisplayOthersViewModel();
            displayOthersRightView = new DisplayOthersRightView(vm);
            privateObject = new PrivateObject(displayOthersRightView);
        }

        [Test]
        public void TestConstructor_InitializesComponent()
        {
            // Assert
            Assert.That(displayOthersRightView, Is.Not.Null);
        }
    }
}