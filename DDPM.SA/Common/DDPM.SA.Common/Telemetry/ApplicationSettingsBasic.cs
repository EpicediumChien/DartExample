using DDPM.SA.Common.Telemetry;
using Newtonsoft.Json;
using System.Collections.Generic;

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
    public class ApplicationSettings_Toggle_2_input_sources_hotkey : ApplicationSettingsBasic
    {
        public string Toggle_2_input_sources_hotkey { get; set; } = string.Empty;
    }

    public class ApplicationSettings_Video_swap_hotkey : ApplicationSettingsBasic
    {
        public string Video_swap_hotkey { get; set; } = string.Empty;
    }

    public class ApplicationSettings_PIP_Toggle_hotkey : ApplicationSettingsBasic
    {
        public string PIP_Toggle_hotkey { get; set; } = string.Empty;
    }
    public class ApplicationSettings_EasyArrangeRecentHotkey : ApplicationSettingsBasic
    {
        public string EasyArrangeRecentHotkey { get; set; } = string.Empty;
    }

    public class ApplicationSettings_VisionEngineHotkey : ApplicationSettingsBasic
    {
        public string VisionEngineHotkey { get; set; } = string.Empty;
    }
    public class ApplicationSettings_BlackStablizer_hotkey : ApplicationSettingsBasic
    {
        public string BlackStablizer_hotkey { get; set; } = string.Empty;
    }

    public class ApplicationSettings_AppMode : ApplicationSettingsBasic
    {
        public string AppMode { get; set; } = string.Empty;
    }

    public class ApplicationSettings_UsedLanguage : ApplicationSettingsBasic
    {
        public string UsedLanguage { get; set; } = string.Empty;
    }


    public class ApplicationSettings_Settings : ApplicationSettingsBasic 
    {
        public string App_Copy_Settings { get; set; }
    }
}