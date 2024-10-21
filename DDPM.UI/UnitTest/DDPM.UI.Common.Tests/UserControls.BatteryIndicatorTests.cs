using DDPM.UI.Common.Models;
using Moq;
using NGA.UnitTest.PrivateObject;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;

namespace DDPM.UI.Common.Tests
{
    [Apartment(ApartmentState.STA)]
    public class BatteryIndicatorTests
    {
        private BatteryIndicator? batteryIndicator;

        [SetUp]
        public void Setup()
        {
            batteryIndicator = new BatteryIndicator();
        }

        [Test]
        public void TestConstructor_BatteryIndicator()
        {
            // Assert
            Assert.That(batteryIndicator, Is.Not.Null);
        }

        [Test]
        public void TestBatteryLevel()
        {
            // Act
            batteryIndicator.BatteryLevel = 1.1;
            // Assert
            Assert.That(batteryIndicator.BatteryLevel, Is.EqualTo(1.1));
        }

        [Test]
        public void TestBatteryStatus()
        {
            // Act
            batteryIndicator.BatteryStatus = "BatteryStatus";
            // Assert
            Assert.That(batteryIndicator.BatteryStatus, Is.EqualTo("BatteryStatus"));
        }

        [Test]
        public void TestConnectionType()
        {
            // Act
            batteryIndicator.ConnectionType = "ConnectionType";
            // Assert
            Assert.That(batteryIndicator.ConnectionType, Is.EqualTo("ConnectionType"));
        }

        [Test]
        public void TestText1()
        {
            // Act
            batteryIndicator.Text1 = "Text1";
            // Assert
            Assert.That(batteryIndicator.Text1, Is.EqualTo("Text1"));
        }

        [Test]
        public void TestNoBattery()
        {
            // Act
            batteryIndicator.NoBattery = false;
            // Assert
            Assert.That(batteryIndicator.NoBattery, Is.EqualTo(false));
        }
    }
}
