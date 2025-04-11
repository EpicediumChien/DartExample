using System.IO;
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

        public static bool IsFileNameValid(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return false;
            if (fileName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
                return false;

            var name = Path.GetFileNameWithoutExtension(fileName).ToUpperInvariant();
            return !ReservedNames.Contains(name);
        }

        public static bool IsPathValid(string fullPath)
        {
            if (string.IsNullOrWhiteSpace(fullPath))
                return false;
            if (fullPath.IndexOfAny(Path.GetInvalidPathChars()) >= 0)
                return false;

            var parts = fullPath.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            //return parts.All(p => string.IsNullOrWhiteSpace(p) || IsFileNameValid(p));
            return IsFileNameValid(parts[parts.Length - 1]);
        }

        private static readonly string[] ReservedNames = {
            "CON", "PRN", "AUX", "NUL",
            "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9",
            "LPT1", "LPT2", "LPT3", "LPT4", "LPT5", "LPT6", "LPT7", "LPT8", "LPT9"
        };
    }
}