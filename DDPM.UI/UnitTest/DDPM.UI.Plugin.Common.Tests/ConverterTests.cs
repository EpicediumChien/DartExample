using DDPM.SA.Common;
using DDPM.UI.Common.Interfaces;
using DDPM.UI.Common;
using DDPM.UI.Plugin.ViewModels;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.UX.WPF;
using Moq;
using NGA.UnitTest.PrivateObject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Reflection.Metadata;

namespace DDPM.UI.Plugin.Common.Tests
{
    [Apartment(ApartmentState.STA)]
    public class BoolToVisibilityConverterTests
    {
        private BoolToVisibilityConverter? boolToVisibilityConverter;

        [SetUp]
        public void Setup()
        {

        }

        [Test]
        public void TestBoolToVisibilityConvert()
        {
            var boolToVisibilityConverter = new BoolToVisibilityConverter();
            var result = boolToVisibilityConverter.Convert(Visibility.Visible, null, null, null);
            Assert.IsNotNull(boolToVisibilityConverter);
            Assert.That(result, Is.EqualTo(Visibility.Collapsed));

            result = boolToVisibilityConverter.Convert(true, null, null, null);
            Assert.That(result, Is.EqualTo(Visibility.Visible));
        }

        [Test]
        public void TestBoolToHiddenConverte()
        {
            var boolToHiddenConverter = new BoolToHiddenConverter();
            var result = boolToHiddenConverter.Convert(Visibility.Visible, null, null, null);
            Assert.IsNotNull(boolToHiddenConverter);
            Assert.That(result, Is.EqualTo(Visibility.Hidden));

            result = boolToHiddenConverter.Convert(true, null, null, null);
            Assert.That(result, Is.EqualTo(Visibility.Visible));
        }

        [Test]
        public void TestThicknessConverter()
        {
            var thicknessConverter = new ThicknessConverter();
            var result = thicknessConverter.Convert(1.1, null, null, null);
            Assert.IsNotNull(thicknessConverter);
            Assert.That(result.ToString, Is.EqualTo("1.1,1.1,1.1,1.1"));

            var val = new double[0] { };
            result = thicknessConverter.Convert(val, null, null, null);
            Assert.That(result.ToString, Is.EqualTo("0,0,0,0"));

            val = new double[1] { 1 };
            result = thicknessConverter.Convert(val, null, null, null);
            Assert.That(result.ToString, Is.EqualTo("1,1,1,1"));

            val = new double[2] { 1, 2 };
            result = thicknessConverter.Convert(val, null, null, null);
            Assert.That(result.ToString, Is.EqualTo("1,2,1,2"));

            val = new double[4] { 1, 2, 3, 4 };
            result = thicknessConverter.Convert(val, null, null, null);
            Assert.That(result.ToString, Is.EqualTo("1,2,3,4"));
        }

        [Test]
        public void TestCenterToolTipConverter()
        {
            var val = new object[2] { 1, DependencyProperty.UnsetValue };
            var centerToolTipConverter = new CenterToolTipConverter();
            var result = centerToolTipConverter.Convert(val, null, null, null);
            Assert.IsNotNull(centerToolTipConverter);

            // << 250429  Updated by Hess
            //Assert.That(result.ToString, Is.EqualTo("NaN"));
            Assert.That(result, Is.EqualTo(double.NaN));
            // >>

            var valu = new object[2] { 1.0, 2.0 };
            result = centerToolTipConverter.Convert(valu, null, null, null);
            Assert.That(result.ToString, Is.EqualTo("-0.5"));
        }

        [Test]
        public void TestCenterVToolTipConverter()
        {
            var val = new object[2] { 1, DependencyProperty.UnsetValue };
            var centerVToolTipConverter = new CenterVToolTipConverter();
            var result = centerVToolTipConverter.Convert(val, null, null, null);
            Assert.IsNotNull(centerVToolTipConverter);

            // << 250429  Updated by Hess
            //Assert.That(result.ToString, Is.EqualTo("NaN"));
            Assert.That(result, Is.EqualTo(double.NaN));
            // >>

            var valu = new object[2] { 1.0, 2.0 };
            result = centerVToolTipConverter.Convert(valu, null, null, null);
            Assert.That(result.ToString, Is.EqualTo("-0.5"));
        }

        [Test]
        public void TestEqualityConverter()
        {
            var equalityConverter = new EqualityConverter();
            var result = equalityConverter.Convert(false, null, false, null);
            Assert.IsNotNull(equalityConverter);
            Assert.That(result, Is.EqualTo(true));

            result = equalityConverter.Convert(true, null, false, null);
            Assert.That(result, Is.EqualTo(false));
        }

        [Test]
        public void TestEqualityConvertBack()
        {
            var equalityConverter = new EqualityConverter();
            var result = equalityConverter.ConvertBack(false, null, false, null);
            Assert.That(result.ToString, Is.EqualTo("{Binding.DoNothing}"));

            result = equalityConverter.ConvertBack(true, null, true, null);
            Assert.That(result, Is.EqualTo(true));
        }

        [Test]
        public void TestHeightConverter()
        {
            var heightConverter = new HeightConverter();
            var result = heightConverter.Convert(new object[2] { 1, 1 }, null, false, null);
            Assert.IsNotNull(heightConverter);

            // << 250429  Updated by Hess
            //Assert.That(result.ToString, Is.EqualTo("NaN"));
            Assert.That(result, Is.EqualTo(double.NaN));
            // >>

            result = heightConverter.Convert(new object[2] { 2.0, 1.2 }, null, "1.2", null);
            Assert.That(result, Is.EqualTo(0.8));
        }

        [Test]
        public void TestBooleanToInverseConverter()
        {
            var booleanToInverseConverter = new BooleanToInverseConverter();
            var result = booleanToInverseConverter.Convert(new object[2] { 1, 1 }, null, false, null);
            Assert.IsNotNull(booleanToInverseConverter);
            Assert.That(result, Is.EqualTo(false));

            result = booleanToInverseConverter.Convert(false, null, "1.2", null);
            Assert.That(result, Is.EqualTo(true));
        }

        [Test]
        public void TestBooleanToInverseConvertBack()
        {
            var booleanToInverseConverter = new BooleanToInverseConverter();
            var result = booleanToInverseConverter.ConvertBack(new object[2] { 1, 1 }, null, false, null);
            Assert.IsNotNull(booleanToInverseConverter);
            Assert.That(result, Is.EqualTo(false));

            result = booleanToInverseConverter.ConvertBack(false, null, "1.2", null);
            Assert.That(result, Is.EqualTo(true));
        }
    }
}
