using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows;
using System.Windows.Media;
using FontFamily = System.Windows.Media.FontFamily;
using FlowDirection=System.Windows.FlowDirection;
using Brushes=System.Windows.Media.Brushes;
using FormattedText=System.Windows.Media.FormattedText;

namespace DDPM.UI.Common
{
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isVisible && isVisible)
                return Visibility.Visible;
            else
                return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    /// <summary>
    /// If the string is empty or blank then convert to Collapsed. Robert_Lin, 2024-11-21
    /// </summary>
    public class StringEmptyToVisibilityConverter: IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((value is string) || (value == null))
                return String.IsNullOrWhiteSpace(value as string) ? Visibility.Collapsed : Visibility.Visible;
            else 
                return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    /// <summary>
    /// Convert to the TextBlock.MaxHeight property based on the number of lines and the font size.
    /// Assume the FontFamily is "Roboto".
    /// </summary>
    /// <param name="value">The TextBlock.Text value to be converted.</param>
    /// <param name="parameter">Specify the fontSize(12) and MaxLines(2) with below example: 
    ///    <TextBlock Text="Your text" Width="200" TextWrapping="Wrap"
    ///               MaxHeight="{Binding ElementName=YourTextBlock, Path=Text, Converter={StaticResource MaxHeightConverter}, ConverterParameter='12,2'}"/>
    /// </param>
    public class MaxHeightConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string text)
            {
                double fontSize = 12; // Default font size
                double maxLines = 1; // Default max lines
                FontFamily fontFamily = new FontFamily("Roboto"); // Default font family

                //Parsing parameter: "12,2" => fontSize=12, maxLines=2
                if (parameter is string param)
                {
                    var parts = param.Split(',');
                    if (parts.Length > 0 && double.TryParse(parts[0], out double parsedFontSize))
                    {
                        fontSize = parsedFontSize;
                    }
                    if (parts.Length > 1 && double.TryParse(parts[1], out double parsedMaxLines))
                    {
                        maxLines = parsedMaxLines;
                    }
                }

                var formattedText = new FormattedText(
                    text,
                    CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight,
                    new Typeface(fontFamily, FontStyles.Normal, FontWeights.ExtraBold, FontStretches.Normal),
                    fontSize,
                    Brushes.Black, 1.25);
                formattedText.Trimming = TextTrimming.None;

                double lineHeight = formattedText.Height;
                return lineHeight * (maxLines+0.5);
            }

            return DependencyProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
