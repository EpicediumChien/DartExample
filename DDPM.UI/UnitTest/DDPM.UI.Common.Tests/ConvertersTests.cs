using DDPM.Easy.Common;
using DDPM.SA.Common;
using DDPM.UI.Common.Interfaces;
using DDPM.UI.Common.Models;
using DDPM.UI.Common.UserControls;
using DDPM.UI.Common.ViewModels;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.UX.WPF;
using Moq;
using NGA.UnitTest.PrivateObject;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using VcpCore.Common;

namespace DDPM.UI.Common.Tests
{
    [Apartment(ApartmentState.STA)]
    public class BoolToVisibilityConverterTests
    {
        private BoolToVisibilityConverter? boolToVisibilityConverter;
        private PrivateObject? privateObject;


        [SetUp]
        public void Setup()
        {
            boolToVisibilityConverter = new BoolToVisibilityConverter();
            privateObject = new PrivateObject(boolToVisibilityConverter);
        }

        [Test]
        public void TestConstructor_BoolToVisibilityConverter()
        {
            // Assert
            Assert.That(boolToVisibilityConverter, Is.Not.Null);
        }

        [Test]
        public void TestConvert()
        {
            var result = boolToVisibilityConverter.Convert(true,typeof(string), "parameter",new CultureInfo(1));
            // Assert
            Assert.That(result, Is.EqualTo(Visibility.Visible));

            result = boolToVisibilityConverter.Convert(false, typeof(string), "parameter", new CultureInfo(1));
            // Assert
            Assert.That(result, Is.EqualTo(Visibility.Collapsed));
        }



    }
}
