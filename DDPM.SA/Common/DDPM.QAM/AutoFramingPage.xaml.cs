using System.Windows;
using System.Windows.Controls;

namespace DDPM.QAM
{
    /// <summary>
    /// Interaction logic for AutoFramingPage.xaml
    /// </summary>
    public partial class AutoFramingPage : UserControl
    {
        public AutoFramingPage()
        {
            InitializeComponent();
            if (DdpmCommonHelper.QAMPageViewModel != null)
            {
                DataContext = DdpmCommonHelper.QAMPageViewModel;
            }
        }

        private void ToggleButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is QAMPageViewModel vm)
            {
                vm.isStatusChagneByDDPM = false;
                //DdpmCommonHelper.DeviceManagerSA?.WriteLog($"ToggleButton_Click -> {vm.isStatusChagneByDDPM}");
                vm.SetAutoFramingStatus();
            }
        }
    }
}
