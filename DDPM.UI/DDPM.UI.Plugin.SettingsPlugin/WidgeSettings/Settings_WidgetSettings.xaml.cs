using DDPM.UI.Common;
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

namespace DDPM.UI.Plugin.SettingsPlugin
{
    /// <summary>
    /// Interaction logic for Settings_WidgetSettings.xaml
    /// </summary>
    public partial class Settings_WidgetSettings : UserControl
    {
        public Settings_WidgetSettings()
        {
            InitializeComponent();
        }

        private void EnableQuickAccessWidget_Click(object sender, RoutedEventArgs e)
        {
            SettingsPageViewModel vm = (SettingsPageViewModel)DataContext;
            DdpmCommonHelper.DeviceManagerSA.Set_GlobalSetting_EnableQuickAccessWidget(vm.GlobalSettingParam.GlobalSetting_WidgetSettings.EnableQuickAccessWidget);
            vm.RefreshUI();
        }

        private void QuickAccessWidget_Reminder_Click(object sender, RoutedEventArgs e)
        {
            SettingsPageViewModel vm = (SettingsPageViewModel)DataContext;
            DdpmCommonHelper.DeviceManagerSA.Set_GlobalSetting_EnableQuickAccessWidget_Reminder(vm.GlobalSettingParam.GlobalSetting_WidgetSettings.EnableQuickAccessWidget_Reminder);
            vm.RefreshUI();
        }
    }
}
