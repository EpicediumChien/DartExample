using System.Windows;
using System.Windows.Media;

namespace DDPM.UI.Plugin.Common
{
    public static class Utility
    {
        public static T? FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            DependencyObject parentObject = VisualTreeHelper.GetParent(child);

            if (parentObject == null)
                return null;

            if (parentObject is T parent)
            {
                return parent;
            }
            else
            {
                return FindParent<T>(parentObject);
            }
        }

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

        public static double GetTextWidth(string text, double fontSize, string fontFamily = "Roboto")
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
            return formattedText.Width;
        }
    }
}