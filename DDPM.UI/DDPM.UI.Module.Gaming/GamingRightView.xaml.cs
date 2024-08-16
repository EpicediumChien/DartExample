using DDPM.UI.Common;
using System.Windows;
using System.Windows.Controls;

namespace DDPM.UI.Module.Gaming
{
    /// <summary>
    /// Interaction logic for GamingRightView.xaml
    /// </summary>
    public partial class GamingRightView : UserControl
    {
        public GamingRightView()
        {
            InitializeComponent();
        }

        private void CallWindowsSettings_Click(object sender, RoutedEventArgs e)
        {
            DdpmCommonHelper.DeviceManagerSA.CallWindowsDisplaySetting();
        }

        private void RefreshUI()
        {
            GamingViewModel vm = (GamingViewModel)DataContext;
            vm.RefreshUI();
        }
    }
}