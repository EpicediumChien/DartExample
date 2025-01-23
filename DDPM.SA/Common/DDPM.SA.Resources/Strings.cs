using DDPM.SA.Resources;
using System.Globalization;
//using System.Windows.Controls.Primitives;
using System.Windows.Input;
//using System.Windows.Navigation;
using Windows.ApplicationModel.Resources.Core;
using Windows.Devices.HumanInterfaceDevice;
using ResourceManager = System.Resources.ResourceManager;

namespace DDPM.SA.Common
{
    [Obsolete("please use DDPM.SA.Resources.Helper.LangHelper if static text", false)] //jim 2024/11/11
    public static class Strings
    {
        private static ResourceManager resManager = Resources.Resources.ResourceManager;
        private static string GetString(string key)
        {

            //CultureInfo cultureInfo = CultureInfo.CreateSpecificCulture("fr-FR");
            //string str = resManager.GetString(key, cultureInfo) ?? resManager.GetString(key, CultureInfo.InvariantCulture) ?? "";

            //Robert_Lin 2025-1-22 PIMS-331191 With DDPM installed, observe language in DDPM UI not change for other langauges of Other countries
            //Add a CultureInfoMap to convert (mapped) CultureInfo.CurrentUICulture to the supported cultureInfo of DDPM
            //OLD:
            //string str = resManager.GetString(key, CultureInfo.CurrentUICulture) ?? resManager.GetString(key, CultureInfo.InvariantCulture) ?? "";
            //NEW:
            string str = resManager.GetString(key, DdpmCultureMap.MappedCultureInfo) ?? resManager.GetString(key, CultureInfo.InvariantCulture) ?? "";

            return System.Text.RegularExpressions.Regex.Unescape(str);
        }

        public static readonly string A0_Device_connected0 = GetString("A0_Device_connected");
        public static readonly string A1_Firmware_update_started0 = GetString("A1_Firmware_update_started");
        public static readonly string A2_Firmware_update_successful0 = GetString("A2_Firmware_update_successful");
        public static readonly string AI_Auto_Framing0 = GetString("AI_Auto_Framing");
        public static readonly string App_Name0 = GetString("App_Name");
        public static readonly string Arrange_Windows0 = GetString("Arrange_Windows");
        public static readonly string Auto_Brightness_is_currently_enabled0 = GetString("Auto_Brightness_is_currently_enabled");
        public static readonly string Auto_framing0 = GetString("Auto_framing");
        public static readonly string Battery_Low0 = GetString("Battery_Low");
        public static readonly string CA_check_fail0 = GetString("CA_check_fail");
        public static readonly string can_be_updated_to0 = GetString("can_be_updated_to");
        public static readonly string Cancel_00 = GetString("Cancel_0");
        public static readonly string Caps_Lock_Off0 = GetString("Caps_Lock_Off");
        public static readonly string Caps_Lock_On0 = GetString("Caps_Lock_On");
        public static readonly string ComfortView_00 = GetString("ComfortView_0");
        public static readonly string Cool_00 = GetString("Cool_0");
        public static readonly string Custom_10 = GetString("Custom_1");
        public static readonly string Custom_20 = GetString("Custom_2");
        public static readonly string Custom_30 = GetString("Custom_3");
        public static readonly string Custom_Color0 = GetString("Custom_Color");
        public static readonly string Custom_layout0 = GetString("Custom_layout");
        public static readonly string DDPM_will_reopen_soon_after_update0 = GetString("DDPM_will_reopen_soon_after_update");
        public static readonly string Default0 = GetString("Default");
        public static readonly string Defer0 = GetString("Defer");
        public static readonly string Delay0 = GetString("Delay");
        public static readonly string Dell_Display_and_Peripheral_Manager0 = GetString("Dell_Display_and_Peripheral_Manager");
        public static readonly string Desktop_00 = GetString("Desktop_0");
        public static readonly string Dock_FW_info0 = GetString("Dock_FW_info");
        public static readonly string Dock_FW_is_being_loaded0 = GetString("Dock_FW_is_being_loaded");
        public static readonly string Dock_FW_is_loaded0 = GetString("Dock_FW_is_loaded");
        public static readonly string Dock_FW_is_loaded_successful0 = GetString("Dock_FW_is_loaded_successful");
        public static readonly string Dock_FW_loaded_failed0 = GetString("Dock_FW_loaded_failed");
        public static readonly string Dock_UOD_FW_update_info0 = GetString("Dock_UOD_FW_update_info");
        public static readonly string Downloading_and_installing0 = GetString("Downloading_and_installing");
        public static readonly string E4_USB_wireless_receiver_firmware_is_unable_to_support_device_firmware_upgrade0 = GetString("E4_USB_wireless_receiver_firmware_is_unable_to_support_device_firmware_upgrade");
        public static readonly string EA_MSG_00 = GetString("EA_MSG_0");
        public static readonly string Error0 = GetString("Error");
        public static readonly string Export_MSG_00 = GetString("Export_MSG_0");
        public static readonly string Failed_to_open_application0 = GetString("Failed_to_open_application");
        public static readonly string Field_of_View0 = GetString("Field_of_View");
        public static readonly string Firmware_Update0 = GetString("Firmware_Update");
        public static readonly string Firmware_update_aborted_same_model_is_connected0 = GetString("Firmware_update_aborted_same_model_is_connected");
        public static readonly string Firmware_update_in_progress_Fail_to_abort0 = GetString("Firmware_update_in_progress_Fail_to_abort");
        public static readonly string Firmware_update_unsuccessful0 = GetString("Firmware_update_unsuccessful");
        public static readonly string FW_info0 = GetString("FW_info");
        public static readonly string FW_is_being_Installing0 = GetString("FW_is_being_Installing");
        public static readonly string Game_00 = GetString("Game_0");
        public static readonly string Game_10 = GetString("Game_1");
        public static readonly string Game_20 = GetString("Game_2");
        public static readonly string Game_30 = GetString("Game_3");
        public static readonly string Game_HDR0 = GetString("Game_HDR");
        public static readonly string Go_to_Widget_Settings0 = GetString("Go_to_Widget_Settings");
        public static readonly string ImpExp_Message00 = GetString("ImpExp_Message.0");
        public static readonly string Installing0 = GetString("Installing");
        public static readonly string Invalid_ID0 = GetString("Invalid_ID");
        public static readonly string is_muted0 = GetString("is_muted");
        public static readonly string is_Unmuted0 = GetString("is_Unmuted");
        public static readonly string M1_Please_double_click_mouse_left_button_to_start_firmware_update0 = GetString("M1_Please_double_click_mouse_left_button_to_start_firmware_update");
        public static readonly string M2_Please_press_key_on_keyboard_to_start_firmware_update0 = GetString("M2_Please_press_key_on_keyboard_to_start_firmware_update");
        public static readonly string Metro_00 = GetString("Metro_0");
        public static readonly string Movie_00 = GetString("Movie_0");
        public static readonly string Movie_HDR0 = GetString("Movie_HDR");
        public static readonly string Multimedia_00 = GetString("Multimedia_0");
        public static readonly string Multiple_docks_are_detected0 = GetString("Multiple_docks_are_detected");
        public static readonly string Native_00 = GetString("Native_0");
        public static readonly string Nature_00 = GetString("Nature_0");
        public static readonly string Network_fail0 = GetString("Network_fail");
        public static readonly string No0 = GetString("No");
        public static readonly string no_updates_available0 = GetString("no_updates_available");
        public static readonly string Num_Lock_Off0 = GetString("Num_Lock_Off");
        public static readonly string Num_Lock_On0 = GetString("Num_Lock_On");
        public static readonly string OFF0 = GetString("OFF");
        public static readonly string Ok0 = GetString("Ok");
        public static readonly string ON0 = GetString("ON");
        public static readonly string Paper_00 = GetString("Paper_0");
        public static readonly string Presence_detection_sensor_is_covered0 = GetString("Presence_detection_sensor_is_covered");
        public static readonly string Presets0 = GetString("Presets");
        public static readonly string Processing0 = GetString("Processing");
        public static readonly string QAM_OSD_Msg0 = GetString("QAM_OSD_Msg");
        public static readonly string Reference_00 = GetString("Reference_0");
        public static readonly string Save_00 = GetString("Save_0");
        public static readonly string Scroll_Lock_Off0 = GetString("Scroll_Lock_Off");
        public static readonly string Scroll_Lock_On0 = GetString("Scroll_Lock_On");
        public static readonly string Service_not_running_Try_again0 = GetString("Service_not_running_Try_again");
        public static readonly string Smooth0 = GetString("Smooth");
        public static readonly string Software_Update0 = GetString("Software_Update");
        public static readonly string Software_update_unsuccessful0 = GetString("Software_update_unsuccessful");
        public static readonly string Sport_00 = GetString("Sport_0");
        public static readonly string SPORTS_Game0 = GetString("SPORTS_Game");
        public static readonly string Standard_00 = GetString("Standard_0");
        public static readonly string Standard_HDR0 = GetString("Standard_HDR");
        public static readonly string SW_info0 = GetString("SW_info");
        public static readonly string Text_00 = GetString("Text_0");
        public static readonly string Timeout0 = GetString("Timeout");
        public static readonly string Timeout_error0 = GetString("Timeout_error");
        public static readonly string Unable_to_detect_target_device0 = GetString("Unable_to_detect_target_device");
        public static readonly string Unable_to_synchronize_the_corresponding_ICC_profile0 = GetString("Unable_to_synchronize_the_corresponding_ICC_profile");
        public static readonly string UOD_update_completed0 = GetString("UOD_update_completed");
        public static readonly string UOD_update_fail0 = GetString("UOD_update_fail");
        public static readonly string Update0 = GetString("Update");
        public static readonly string update_download_cancel0 = GetString("update_download_cancel");
        public static readonly string Update_failed_due_to_network_error0 = GetString("Update_failed_due_to_network_error");
        public static readonly string update_failed_with_unknown_error0 = GetString("update_failed_with_unknown_error");
        public static readonly string UpdateAvailable0 = GetString("UpdateAvailable");
        public static readonly string UpdateAvailable_Info0 = GetString("UpdateAvailable_Info");
        public static readonly string UpdateNow0 = GetString("UpdateNow");
        public static readonly string Updates_info0 = GetString("Updates_info");
        public static readonly string UpdateWillBeApplied0 = GetString("UpdateWillBeApplied");
        public static readonly string UpdateWillBeApplied_Info0 = GetString("UpdateWillBeApplied_Info");
        public static readonly string Updating_firmware_Do_not_remove_or_power_off_the_device_Leave_the_device_undisturbed0 = GetString("Updating_firmware_Do_not_remove_or_power_off_the_device_Leave_the_device_undisturbed");
        public static readonly string Updating_Software_Do_not_power_off_this_PC0 = GetString("Updating_Software_Do_not_power_off_this_PC");
        public static readonly string USB_wireless_receiver_firmware_is_unable_to_support_device_firmware_upgrade0 = GetString("USB_wireless_receiver_firmware_is_unable_to_support_device_firmware_upgrade");
        public static readonly string User_10 = GetString("User_1");
        public static readonly string User_20 = GetString("User_2");
        public static readonly string User_30 = GetString("User_3");
        public static readonly string User_aborted_firmware_update0 = GetString("User_aborted_firmware_update");
        public static readonly string Version0 = GetString("Version");
        public static readonly string Vibrant0 = GetString("Vibrant");
        public static readonly string Vivid_HDR0 = GetString("Vivid_HDR");
        public static readonly string Walk_Away_Lock0 = GetString("Walk_Away_Lock");
        public static readonly string Warm0 = GetString("Warm");
        public static readonly string Warm_00 = GetString("Warm_0");
        public static readonly string Warning0 = GetString("Warning");
        public static readonly string will_be_updated_to0 = GetString("will_be_updated_to");
        public static readonly string Yes0 = GetString("Yes");
        public static readonly string Zoom0 = GetString("Zoom");

    }
}
