using DDPM.SA.Common;
using DDPM.SA.Common.Settings;
using DDPM.UI.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using VcpCore.Common;
using UserControl = System.Windows.Controls.UserControl;

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

            //lock/unlock init, 9/23 add OSD lock
            if (DdpmCommonHelper.DeviceManagerSA != null)
            {
                DdpmCommonHelper.DeviceManagerSA.ITSettingsActionEvent += DeviceManagerSA_ITSettingsActionEvent;

                DDPMSettings data = DdpmCommonHelper.DeviceManagerSA.ReloadAppConfigData().Result;
                if (data != null)
                {
                    //if (data.LockSettings.Lock_Setting_ScreenNotification)
                    //{
                        //_vm.lockIcon = Visibility.Visible;
                        //_vm.viewMask = Visibility.Visible;
                        //_vm.tabStop = false;
                    //}
                    //else
                    //{
                        //_vm.lockIcon = Visibility.Collapsed;
                        //_vm.viewMask = Visibility.Collapsed;
                        //_vm.tabStop = true;
                    //}
                }
            }
        }
        ~Settings_General()
        {
            if (DdpmCommonHelper.DeviceManagerSA != null)
            {
                DdpmCommonHelper.DeviceManagerSA.ITSettingsActionEvent -= DeviceManagerSA_ITSettingsActionEvent;
            }
        }
        private void DeviceManagerSA_ITSettingsActionEvent(object? sender, SA.Common.ITSettingEventArgs e)
        {
            bool? rst = DdpmCommonHelper.GetUINotifyPropertyValue_Boolean("Lock_Setting_ScreenNotification", e);
            Dispatcher.Invoke(new Action(() =>
            {
                //if (_vm != null)
                {
                    bool locked = false;
                    if (rst != null && rst == true)
                        locked = true;

                    //_vm.lockIcon = locked ? Visibility.Visible : Visibility.Collapsed;
                    //_vm.viewMask = locked ? Visibility.Visible : Visibility.Collapsed;
                    //_vm.tabStop = !locked;
                }
            }));
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
            SettingsPageViewModel vm = (SettingsPageViewModel)DataContext;
            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Filter = "zip Files (*.zip)|*.zip|All Files (*.*)|*.*"; // 檔案類型過濾
                saveFileDialog.Title = "Save Diagnostic Report";
                saveFileDialog.DefaultExt = "zip";

                // 顯示對話框並檢查用戶是否按了「儲存」按鈕
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    // 獲取選擇的檔案路徑
                    string filePath = saveFileDialog.FileName;
                    Debug.Write(filePath);
                    filePath = filePath.Substring(0, filePath.IndexOf("."));
                    Debug.Write(filePath);
                    vm.SaveDiagnosticReport(filePath);
                }
            }
        }
        private void SaveMonitorAssetReport_Click(object sender, MouseButtonEventArgs e)
        {
            SettingsPageViewModel vm = (SettingsPageViewModel)DataContext;
            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Filter = "MIF Files (*.mif)|*.mif|All Files (*.*)|*.*"; // 檔案類型過濾
                saveFileDialog.Title = "Save Monitor Asset Report";
                saveFileDialog.DefaultExt = "mif";

                // 顯示對話框並檢查用戶是否按了「儲存」按鈕
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    // 獲取選擇的檔案路徑
                    string filePath = saveFileDialog.FileName;
                    // 如果檔案路徑不以 .mif 結尾，則附加 .mif 副檔名
                    if (!filePath.EndsWith(".mif", StringComparison.OrdinalIgnoreCase))
                    {
                        Debug.Write(filePath);
                        filePath = filePath.Substring(0, filePath.IndexOf("."));
                        Debug.Write(filePath);
                        filePath += ".mif";
                    }
                    vm.SaveMonitorAssetReport(filePath);
                }
            }
        }
    }
}
