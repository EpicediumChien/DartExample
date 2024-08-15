using DDPM.UI.Common;
using Dell.Client.Framework.UX.WPF;
using System.Windows.Controls;
using System.Windows.Input;

namespace DDPM.UI.Plugin.SettingsPlugin
{
    /// <summary>
    /// SettingsPage.xaml 的互動邏輯
    /// </summary>
    public partial class SettingsPage : UserControl
    {
        private SettingsPageViewModel vm
        {
            get { return (SettingsPageViewModel)DataContext; }
        }
        public SettingsPage()
        {
            InitializeComponent();
            DataContext = new SettingsPageViewModel();
            if (DdpmCommonHelper.DeviceManagerSA != null)
            {
                vm.SetUpdateInfoUI(DdpmCommonHelper.DeviceManagerSA.GetFWUpdateInfo(false).Result, DdpmCommonHelper.DeviceManagerSA.SW_GetSWUpdateInfo(false).Result);
                vm.RefreshUI();
            }
        }

        private void leftArrow_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            IConsole? console = SettingsPlugin.PluginIoc.GetService<IConsole>();
            console?.ShowPluginById(DDPM.UI.Common.Constants.DdpmHomePluginId);
        }
        //0614 將按鈕改成UXTextBlock，事件也變更，讓風格更像figma，不影響功能作動
        private void GeneralButton_Click(object sender, MouseButtonEventArgs e)
        {
            vm.SetSelected(0);
            vm.FullView = null;
        }
        private void UpdatesButton_Click(object sender, MouseButtonEventArgs e)
        {
            vm.SetSelected(1);
            UpdatesPage updatesPage = new UpdatesPage();
            vm.OpenFullView(updatesPage);
        }
        private void AnalyticsButton_Click(object sender, MouseButtonEventArgs e)
        {
            //Dean 0618 add analytics page
            vm.SetSelected(2);
            vm.FullView = new AnalyticsPage();
        }

        private void QuickSettingsButton_Click(object sender, MouseButtonEventArgs e)
        {
            vm.SetSelected(3);
            vm.FullView = null;
        }

        private void AboutButton_Click(object sender, MouseButtonEventArgs e)
        {
            vm.SetSelected(4);
            vm.FullView = null;
        }
    }
}