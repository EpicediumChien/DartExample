using DDPM.UI.Common;
using Dell.Client.Framework.Common;
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
        private ILog? _log;
        public Settings_WidgetSettings()
        {
            _log = SettingsPlugin.PluginIoc?.GetService<ILog>();
            _log?.Info("Settings_WidgetSettings initialize start");
            InitializeComponent();
            _log?.Info("Settings_WidgetSettings initialize done");
        }

        private void EnableQuickAccessWidget_Click(object sender, RoutedEventArgs e)
        {
            SettingsPageViewModel vm = (SettingsPageViewModel)DataContext;
            //DdpmCommonHelper.DeviceManagerSA.Set_GlobalSetting_EnableQuickAccessWidget(vm.GlobalSettingParam.GlobalSetting_WidgetSettings.EnableQuickAccessWidget);
            DdpmCommonHelper.Set_GlobalSettings(DdpmCommonHelper.GlobalSettingsType.EnableQAW, vm.GlobalSettingParam.GlobalSetting_WidgetSettings.EnableQuickAccessWidget);
            vm.RefreshUI();
        }

        private void QuickAccessWidget_Reminder_Click(object sender, RoutedEventArgs e)
        {
            SettingsPageViewModel vm = (SettingsPageViewModel)DataContext;
            //DdpmCommonHelper.DeviceManagerSA.Set_GlobalSetting_EnableQuickAccessWidget_Reminder(vm.GlobalSettingParam.GlobalSetting_WidgetSettings.EnableQuickAccessWidget_Reminder);
            DdpmCommonHelper.Set_GlobalSettings(DdpmCommonHelper.GlobalSettingsType.EnableQAWReminder, vm.GlobalSettingParam.GlobalSetting_WidgetSettings.EnableQuickAccessWidget_Reminder);
            vm.RefreshUI();
        }

        // 10/12/2024   Derek  for RWD  -- not tested yet
        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            activateTextBlock.Width = borderContent.ActualWidth - 100;
            enableShortcutTextBlock.Width = borderContent.ActualWidth - 100;
        }
    }
}
