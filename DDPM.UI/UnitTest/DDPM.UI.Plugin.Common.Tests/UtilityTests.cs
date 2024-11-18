using DDPM.UI.Common.UserControls;
using NGA.UnitTest.PrivateObject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using static System.Net.Mime.MediaTypeNames;

namespace DDPM.UI.Plugin.Common.Tests
{
    [Apartment(ApartmentState.STA)]
    public class UtilityTests
    {

        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void TestFindParent()
        {
            var result = Utility.FindParent<DependencyObject>(new ItemsControl());
            // Assert
            Assert.That(result, Is.EqualTo(null));
        }

        [Test]
        public void TestParameter()
        {
            var result = Utility.CheckTextLength("dfda", 10, 2.0, "Roboto");
            // Assert
            Assert.That(result, Is.EqualTo("dfda"));
        }
    }
}
