using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading;

namespace DDPM.SA.Common
{
    public class ApplicationSettingsBasic
    {
        public List<string> DisplayModelname { get; set; }
        public List<string> DisplayServiceTag { get; set; }
        public List<string> D_Ctrl { get; set; }

        public ApplicationSettingsBasic()
        {
            DisplayModelname = new List<string>();
            D_Ctrl = new List<string>();
            DisplayServiceTag = new List<string>();
        }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
    }

    public class ApplicationSettings_LockRotation : ApplicationSettingsBasic
    {
        public string LockRotation { get; set; }
    }
    public class ApplicationSettings_LowBatteryLevel : ApplicationSettingsBasic
    {
        public string LowBatteryLevel { get; set; }
    }
    public class ApplicationSettings_KeyboardLockKey : ApplicationSettingsBasic
    {
        public string KeyboardLockKey { get; set; }
    }
    public class ApplicationSettings_WB7022CoverState : ApplicationSettingsBasic
    {
        public string WB7022CoverState { get; set; }
    }
    public class ApplicationSettings_MuteState : ApplicationSettingsBasic
    {
        public string MuteState { get; set; }
    }
    public class ApplicationSettings_ColorPresetAndEasyMemory : ApplicationSettingsBasic
    {
        public string ColorPresetAndEasyMemory { get; set; }
    }
    public class ApplicationSettings_EnableQuickAccessWidget : ApplicationSettingsBasic
    {
        public string EnableQuickAccessWidget { get; set; }
    }
    public class ApplicationSettings_EnableQuickAccessWidget_Reminder : ApplicationSettingsBasic
    {
        public string EnableQuickAccessWidget_Reminder { get; set; }
    }
    public class ApplicationSettings_SaveDiagnosticReport : ApplicationSettingsBasic
    {
        public string SaveDiagnosticReport { get; set; }
    }
    public class ApplicationSettings_SaveMonitorAssetReport : ApplicationSettingsBasic
    {
        public string SaveMonitorAssetReport { get; set; }
    }
    public class ApplicationSettings_SoftwareUpdateBasic
    {
        public string UpdateVersion { get; set; }
        public string Results { get; set; }
        public List<string> DisplayModelname { get; set; }
        public List<string> DisplayServiceTag { get; set; }
        public List<string> D_Ctrl { get; set; }
        public string SW_Available_date { get; set; }
        public string SW_Update_date { get; set; }

        public ApplicationSettings_SoftwareUpdateBasic()
        {
            UpdateVersion = string.Empty;
            Results = string.Empty;
            DisplayModelname = new List<string>();
            DisplayServiceTag = new List<string>();
            D_Ctrl = new List<string>();
            SW_Available_date = string.Empty;
            SW_Update_date = string.Empty;
        }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
    }
    public class ApplicationSettings_SoftwareFailureBasic
    {
        public string ErrorCode { get; set; }
        public string FailureMessage { get; set; }
        public List<string> DisplayModelname { get; set; }
        public List<string> DisplayServiceTag { get; set; }
        public List<string> D_Ctrl { get; set; }
        public string SW_Available_date { get; set; }
        public string SW_Update_date { get; set; }

        public ApplicationSettings_SoftwareFailureBasic()
        {
            ErrorCode = string.Empty;
            FailureMessage = string.Empty;
            DisplayModelname = new List<string>();
            DisplayServiceTag = new List<string>();
            D_Ctrl = new List<string>();
            SW_Available_date = string.Empty;
            SW_Update_date = string.Empty;
        }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
    }
}