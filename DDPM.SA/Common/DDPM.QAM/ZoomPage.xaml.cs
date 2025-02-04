using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;

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
            if (DataContext is QAMPageViewModel vm)
            {
                vm.IsSliderDragging = true;
            }
        }

        private void ZoomSlider_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            if (DataContext is QAMPageViewModel vm)
            {
                vm.IsSliderDragging = false;
                vm.isStatusChangeByDDPM = false;
                vm.SetZoom();

                //Derek 2025/01/24 Save Zoom value to none profile if current select is none
                vm.SaveNoneProfileZOOM();
            }
        }
    }
    public class SliderWidthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double sliderValue && DdpmCommonHelper.QAMPageViewModel != null)
            {
                return (DdpmCommonHelper.QAMPageViewModel.ZoomMin - sliderValue) / (DdpmCommonHelper.QAMPageViewModel.ZoomMin - DdpmCommonHelper.QAMPageViewModel.ZoomMax) * 200;
            }
            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
