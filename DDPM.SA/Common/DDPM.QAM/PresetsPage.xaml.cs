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
    /// Interaction logic for PresetsPage.xaml
    /// </summary>
    public partial class PresetsPage : UserControl
    {
        public PresetsPage()
        {
            InitializeComponent();

            if (DdpmCommonHelper.QAMPageViewModel != null)
            {
                DataContext = DdpmCommonHelper.QAMPageViewModel;
            }
        }
        private void SetProfile_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.DataContext is UI_Profile selectedProfile)
            {
                if (DataContext is QAMPageViewModel vm)
                {
                    vm.SetProfile(selectedProfile);
                    vm.SendSelectProfileToDDPM();
                }
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is QAMPageViewModel vm)
            {
                vm.SetProfile();
            }
        }
    }
}
