using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DDPM.QAM
{
    /// <summary>
    /// Interaction logic for ZoomPage.xaml
    /// </summary>
    public partial class ZoomPage : UserControl
    {
        public ZoomPage()
        {
            InitializeComponent();
            if (DdpmCommonHelper.QAMPageViewModel != null)
            {
                DataContext = DdpmCommonHelper.QAMPageViewModel;
            }
        }
        private void ZoomSlider_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {
            QAMPageViewModel vm = DataContext as QAMPageViewModel;
            if (vm != null)
            {
                vm.IsSliderDragging = true;
            }
        }

        private void ZoomSlider_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            QAMPageViewModel vm = DataContext as QAMPageViewModel;
            if (vm != null)
            {
                vm.IsSliderDragging = false;
                vm.SetZoom();
            }
        }
    }
    public class SliderWidthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double sliderValue)
            {
                if (DdpmCommonHelper.QAMPageViewModel != null)
                {
                    return (DdpmCommonHelper.QAMPageViewModel.ZoomMin - sliderValue) / (DdpmCommonHelper.QAMPageViewModel.ZoomMin - DdpmCommonHelper.QAMPageViewModel.ZoomMax) * 200;
                }
            }
            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
