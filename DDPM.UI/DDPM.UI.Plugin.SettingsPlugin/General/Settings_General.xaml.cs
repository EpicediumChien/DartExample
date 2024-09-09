using DDPM.UI.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    /// Interaction logic for Settings_General.xaml
    /// </summary>
    public partial class Settings_General : UserControl
    {
        public Settings_General()
        {
            InitializeComponent();
        }
        private void LowBatteryLevel_Click(object sender, RoutedEventArgs e)
        {
            SettingsPageViewModel vm = (SettingsPageViewModel)DataContext;
            Debug.WriteLine(vm.GlobalSettingParam.GlobalSetting_General.Low_Battery_Level);
            DdpmCommonHelper.DeviceManagerSA.Set_GlobalSetting_DisplayLowBatteryLevel(vm.GlobalSettingParam.GlobalSetting_General.Low_Battery_Level);
        }
        private void KeyboardLockKey_Click(object sender, RoutedEventArgs e)
        {
            SettingsPageViewModel vm = (SettingsPageViewModel)DataContext;
            DdpmCommonHelper.DeviceManagerSA.Set_GlobalSetting_DisplayKeyboardLockKey(vm.GlobalSettingParam.GlobalSetting_General.Keyboard_Lock_Key);
        }
        private void WB7022CoverState_Click(object sender, RoutedEventArgs e)
        {
            SettingsPageViewModel vm = (SettingsPageViewModel)DataContext;
            DdpmCommonHelper.DeviceManagerSA.Set_GlobalSetting_DisplayWB7022CoverState(vm.GlobalSettingParam.GlobalSetting_General.Webcam_WB7022_Presence_Detection_Sensor_Cover_State);
        }
        private void MuteState_Click(object sender, RoutedEventArgs e)
        {
            SettingsPageViewModel vm = (SettingsPageViewModel)DataContext;
            DdpmCommonHelper.DeviceManagerSA.Set_GlobalSetting_DisplayMuteState(vm.GlobalSettingParam.GlobalSetting_General.Display_MuteState);
        }
        private void DisplayCPAndEM_Click(object sender, RoutedEventArgs e)
        {
            SettingsPageViewModel vm = (SettingsPageViewModel)DataContext;
            DdpmCommonHelper.DeviceManagerSA.Set_GlobalSetting_DisplayColorPresetAndEasyMemory(vm.GlobalSettingParam.GlobalSetting_General.Display_Color_Preset_and_Easy_Memory);
        }
        private void SaveDiagnosticReport_Click(object sender, MouseButtonEventArgs e)
        {
        }
        private void SaveMonitorAssetReport_Click(object sender, MouseButtonEventArgs e)
        {
        }
    }
}
