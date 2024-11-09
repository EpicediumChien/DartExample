using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Power;

namespace DDPM.SA.Common
{
    public class GlobalSettingParam
    {
        public GlobalSetting_General GlobalSetting_General { get; set; }
        public GlobalSetting_WidgetSettings GlobalSetting_WidgetSettings { get; set; }
        public GlobalSetting_About GlobalSetting_About { get; set; }
        public bool isTelemetryConsentOn { get; set; } = false;//1030 set to false as default
        public bool isSetTelemetryOverInstaller { get; set; } = false;

        public GlobalSettingParam()
        {
            GlobalSetting_General = new GlobalSetting_General();
            GlobalSetting_WidgetSettings = new GlobalSetting_WidgetSettings();
            GlobalSetting_About = new GlobalSetting_About();
        }
    }
    public class GlobalSetting_General
    {
        public bool Low_Battery_Level { get; set; }
        public bool Keyboard_Lock_Key { get; set; }
        public bool Webcam_WB7022_Presence_Detection_Sensor_Cover_State { get; set; }
        public bool Display_MuteState { get; set; }
        public bool Display_Color_Preset_and_Easy_Memory { get; set; }
        public GlobalSetting_General()
        {
            Low_Battery_Level = true;
            Keyboard_Lock_Key = false;
            Webcam_WB7022_Presence_Detection_Sensor_Cover_State = true;
            Display_MuteState = true;
            Display_Color_Preset_and_Easy_Memory = true;
        }
    }
    public class GlobalSetting_WidgetSettings
    {
        public bool EnableQuickAccessWidget { get; set; }
        public bool EnableQuickAccessWidget_Reminder { get; set; }
        public GlobalSetting_WidgetSettings()
        {
            EnableQuickAccessWidget = true;
            EnableQuickAccessWidget_Reminder = true;
        }
    }
    public class GlobalSetting_About
    {
        public string SWVersion { get; set; }
        public string DriverVersion { get; set; }
        public GlobalSetting_About()
        {
            SWVersion = "0000";
            DriverVersion = "0000";
        }
    }
}
