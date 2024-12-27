using System;
using System.Collections.Generic;
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
