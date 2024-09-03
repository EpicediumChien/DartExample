using DDPM.UI.Plugin.ViewModels;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using UserControl = System.Windows.Controls.UserControl;

namespace DDPM.UI.Module.HeadsetAutomatedActions
{
    /// <summary>
    /// Interaction logic for HeadsetAudioSettingsRightView.xaml
    /// </summary>
    public partial class HeadsetAutomatedActionsRightView : UserControl
    {
        private readonly HeadsetViewModel _vm;

        public HeadsetAutomatedActionsRightView(HeadsetViewModel vm)
        {
            InitializeComponent();
            _vm = vm;
        }
    }

    public class BooleanToForegroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return boolValue ? Brushes.White : Brushes.Gray;
            }
            return Brushes.Gray;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}