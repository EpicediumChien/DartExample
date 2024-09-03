using System.Windows;

namespace DDPM.UI.Plugin.DdpmHomePlugin.Tests
{
    [TestFixture, Apartment(ApartmentState.STA)]
    public class HomeDevicesPanelTests
    {
        private HomeDevicesPanel? homeDevicesPanel;

        [SetUp]
        public void SetUp()
        {
            homeDevicesPanel = new HomeDevicesPanel();
        }

        [Test]
        public void TestConstructor_HomeDevicesPanel()
        {
            Assert.IsNotNull(homeDevicesPanel);
        }

        [Test]
        public void TestHorizontalContentAlignment()
        {
            var horizontalContentAlignment = new HorizontalAlignment();
            homeDevicesPanel.HorizontalContentAlignment = horizontalContentAlignment;
            Assert.That(homeDevicesPanel.HorizontalContentAlignment, Is.EqualTo(horizontalContentAlignment));
        }
    }
}