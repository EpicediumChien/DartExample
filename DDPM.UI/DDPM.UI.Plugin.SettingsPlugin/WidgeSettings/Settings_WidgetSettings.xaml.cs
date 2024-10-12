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
        private const int breakPoints = 670;
        private readonly double originActivateTextBlockWidth;
        private readonly double originEnableShortcutTextBlockWidth;

        public Settings_WidgetSettings()
        {
            InitializeComponent();

            originActivateTextBlockWidth = activateTextBlock.ActualWidth;
            originEnableShortcutTextBlockWidth = enableShortcutTextBlock.ActualWidth;
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

        // 10/12/2024   Derek   for RWD  -- not tested yet
        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (this.ActualWidth <= breakPoints)
            {
                activateTextBlock.Width = 350;
                enableShortcutTextBlock.Width = 350;
            }
            else {
                activateTextBlock.Width = originActivateTextBlockWidth;
                enableShortcutTextBlock.Width = originEnableShortcutTextBlockWidth;
            }
        }
    }
}
