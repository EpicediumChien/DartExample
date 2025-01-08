using System.Globalization;
using System.Windows.Data;

namespace DDPM.UI.Module.PipPbp
{
    internal class SpItemNameToIsSelectedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value != null && value is string &&
                parameter != null && parameter is string &&
                value == parameter)
            {             
                return true;                
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}