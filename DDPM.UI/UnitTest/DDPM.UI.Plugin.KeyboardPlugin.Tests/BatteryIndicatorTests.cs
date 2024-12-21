using DDPM.SA.Common;
using DDPM.UI.Common;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.UX.WPF;
using Moq;
using NGA.UnitTest.PrivateObject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDPM.UI.Plugin.KeyboardPlugin.Tests
{
    [Apartment(ApartmentState.STA)]
    public class BatteryIndicatorTests
    {
        private BatteryIndicator? batteryIndicator;
        private PrivateObject? privateObject;
 
        [SetUp]
        public void Setup()
        {
            batteryIndicator = new BatteryIndicator();
            privateObject = new PrivateObject(batteryIndicator);
        }

        [Test]
        public void TestConstructor_InitializeComponent()
        {
            Assert.That(batteryIndicator, Is.Not.Null);
        }

        [Test]
        public void TestBatteryLevel()
        {
            var batteryLevel = 1.1;
            batteryIndicator.BatteryLevel=batteryLevel;
            Assert.That(batteryIndicator.BatteryLevel, Is.EqualTo(batteryLevel));
        }
    }
}
