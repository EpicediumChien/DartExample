using DDPM.UI.Common.Models;
using DDPM.UI.Common.UserControls;
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
    public class MonitorIndicatorTests
    {
        private MonitorIndicator? monitorIndicator;

        [SetUp]
        public void Setup()
        {
            monitorIndicator = new MonitorIndicator();
        }

        [Test]
        public void TestConstructor_MonitorIndicator()
        {
            // Assert
            Assert.That(monitorIndicator, Is.Not.Null);
        }

        [Test]
        public void TestInputSource()
        {
            monitorIndicator.InputSource = "InputSource";
            // Assert
            Assert.That(monitorIndicator.InputSource, Is.EqualTo("InputSource"));
        }
    }
}
