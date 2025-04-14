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
            try
            {
                //DdpmCommonHelper.DeviceManagerSA!.WriteLog($"PresetsPage -> SetProfile_Click");

                if (sender is Border border &&
                    border.DataContext is UI_Profile selectedProfile &&
                    DataContext is QAMPageViewModel vm)
                {
                    //Derek 2025/02/20
                    if (string.Equals(selectedProfile.Profile_Name_Key, vm.selectedProfileName, StringComparison.OrdinalIgnoreCase))
                    {
                        DdpmCommonHelper.DeviceManagerSA!.WriteLog($"return due to: {selectedProfile.Profile_Name_Key} = {vm.selectedProfileName}");
                        return;
                    }

                    vm.isStatusChangeByDDPM = false;
                    vm.SetProfile(selectedProfile);
                    vm.SendSelectProfileToDDPM();
                }
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.DeviceManagerSA?.WriteLog($"Exception in SetProfile_Click: {ex.Message}");
            }

        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            DdpmCommonHelper.DeviceManagerSA?.WriteLog($"PresetsPage -> UserControl_Loaded");

            if (DataContext is QAMPageViewModel vm)
            {
                //vm.SetProfile();
            }
        }
    }
}
