using DDPM.UI.Common.Interfaces;
using DDPM.UI.Common;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DDPM.UI.Module.Brightness.Tests
{
    [Apartment(ApartmentState.STA)]
    public class RadioBoolToIntConverterTests
    {

        [SetUp]
        public void Setup()
        {

        }

        [Test]
        public void TestRadioCanConvert()
        {
            var radioBoolToIntConverter = new RadioBoolToIntConverter();
            var result = radioBoolToIntConverter.Convert(10,null,10,null);
            Assert.That(result,Is.EqualTo(true));

            result = radioBoolToIntConverter.Convert(-1, null, 100, null);
            Assert.That(result, Is.EqualTo(false));
        }

        [Test]
        public void TestRadioConvertBack()
        {
            var radioBoolToIntConverter = new RadioBoolToIntConverter();
            var result = radioBoolToIntConverter.ConvertBack(null, null, 10, null);
            Assert.That(result, Is.EqualTo(10));
        }


        [Test]
        public void TestConvert()
        {
            var boolReverseToVisibilityConverter = new BoolReverseToVisibilityConverter();
            var result = boolReverseToVisibilityConverter.Convert(Visibility.Visible, null, null, null);
            Assert.That(result, Is.EqualTo(Visibility.Collapsed));

            result = boolReverseToVisibilityConverter.Convert(Visibility.Hidden, null, null, null);
            Assert.That(result, Is.EqualTo(Visibility.Visible));
        }

        [Test]
        public void TestConvertBack()
        {
            var boolReverseToVisibilityConverter = new BoolReverseToVisibilityConverter();
            try {
                  object result1 = boolReverseToVisibilityConverter.ConvertBack(null, null, null, null);
                }
            catch (Exception ex)
            {
                Assert.That(ex.Message,Is.EqualTo("Specified method is not supported."));
            }
        }
        
    }
}
