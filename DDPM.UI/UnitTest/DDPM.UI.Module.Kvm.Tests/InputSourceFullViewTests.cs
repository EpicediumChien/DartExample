namespace DDPM.UI.Module.Kvm.Tests
{
    [TestFixture, Apartment(ApartmentState.STA)]
    public class InputSourceFullViewTests
    {
        private InputSourceFullView? inputSourceFullView;

        [SetUp]
        public void Setup()
        {
            inputSourceFullView = new InputSourceFullView();
        }

        [Test]
        public void TestConstructor_InitializeComponent()
        {
            Assert.That(inputSourceFullView, Is.Not.Null);
        }
    }
}