using System.Windows;
using System.Windows.Media;

namespace DDPM.UI.Common
{
    public static class Utility
    {
        public static string CheckTextLength(string text, double width, double fontSize, string fontFamily = "Roboto")
        {
            var typeface = new Typeface(new System.Windows.Media.FontFamily(fontFamily), FontStyles.Normal, FontWeights.Normal, FontStretches.Normal);

            var formattedText = new FormattedText(
                text,
                System.Globalization.CultureInfo.CurrentUICulture,
                System.Windows.FlowDirection.LeftToRight,
                typeface,
                fontSize,
                System.Windows.Media.Brushes.Black,
            new NumberSubstitution(),
                1.0);
            if (formattedText.Width <= width)
            { return text; }

            while (formattedText.Width > width)
            {
                text = text.Substring(0, text.Length - 2);
                formattedText = new FormattedText(
                  $"{text}...",
                  System.Globalization.CultureInfo.CurrentUICulture,
                  System.Windows.FlowDirection.LeftToRight,
                  typeface,
                  16,
                  System.Windows.Media.Brushes.Black,
                  new NumberSubstitution(),
                  1.0);
            }
            return $"{text}...";
        }
    }
}
