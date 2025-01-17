using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

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
            //DdpmCommonHelper.DeviceManagerSA!.WriteLog($"PresetsPage -> SetProfile_Click");

            if (sender is Border border && 
                border.DataContext is UI_Profile selectedProfile && 
                DataContext is QAMPageViewModel vm)
            {
                vm.isStatusChangeByDDPM = false;
                vm.SetProfile(selectedProfile);
                vm.SendSelectProfileToDDPM();
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            DdpmCommonHelper.DeviceManagerSA!.WriteLog($"PresetsPage -> UserControl_Loaded");

            if (DataContext is QAMPageViewModel vm)
            {
                //vm.SetProfile();
            }
        }
    }
}
