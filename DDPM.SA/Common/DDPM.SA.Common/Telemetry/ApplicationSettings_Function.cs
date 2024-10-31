using DDPM.SA.Common.Display;
using DDPM.SA.Common.Settings;
using System.Collections.Generic;
using System.Linq;
using VcpCore.Common;
using static VcpCore.Common.User32;

namespace DDPM.SA.Common
{
    public class ApplicationSettings_Function
    {
        public bool Send_LockRotation_Telementry(ITelementryScheduler plugin, List<MonitorInfo> monitorInfos, bool LockRotation)
        {
            var rt = false;
            List<string> models = new List<string>();
            List<string> D_Ctrls = new List<string>();
            List<string> DisplayServiceTags = new List<string>();

            foreach (var monitorInfo in monitorInfos)
            {
                models.Add(monitorInfo.modelName);
                D_Ctrls.Add(monitorInfo.D_Ctrl);
                DisplayServiceTags.Add(monitorInfo.edid.ServiceTag);
            }

            var TelemetryDta_LockRotation = new ApplicationSettings_LockRotation();
            TelemetryDta_LockRotation.DisplayModelname = models.ToList();
            TelemetryDta_LockRotation.D_Ctrl = D_Ctrls.ToList();
            TelemetryDta_LockRotation.DisplayServiceTag = DisplayServiceTags.ToList();
            TelemetryDta_LockRotation.LockRotation = LockRotation ? "On" : "Off";

            if (plugin != null)
                rt = plugin.ReceiveTelemetryInfo("ApplicationSettings", TelemetryDta_LockRotation.ToJson(), Telementry_Frequency.RealTime).Result;

            return rt;
        }

        public bool Send_Hotkey_Telementry(ITelementryScheduler plugin, List<MonitorInfo> monitorInfos, string val, HotkeyType hotkeyType)
        {
            var rt = false;
            if (string.IsNullOrEmpty(val))
                return rt;
            if (monitorInfos != null && monitorInfos.Count == 0)
                return rt;
            List<string> models = new List<string>();
            List<string> D_Ctrls = new List<string>();
            List<string> DisplayServiceTags = new List<string>();

            foreach (var monitorInfo in monitorInfos)
            {
                models.Add(monitorInfo.modelName);
                D_Ctrls.Add(monitorInfo.D_Ctrl);
                DisplayServiceTags.Add(monitorInfo.edid.ServiceTag);
            }
            string TelemetryDta = string.Empty;
            switch (hotkeyType)
            {
                case HotkeyType.ToggleInputSource:
                    var TelemetryDta_ToggleInputSource = new ApplicationSettings_Toggle_2_input_sources_hotkey();
                    TelemetryDta_ToggleInputSource.DisplayModelname = models.ToList();
                    TelemetryDta_ToggleInputSource.D_Ctrl = D_Ctrls.ToList();
                    TelemetryDta_ToggleInputSource.DisplayServiceTag = DisplayServiceTags.ToList();
                    TelemetryDta_ToggleInputSource.Toggle_2_input_sources_hotkey = val;
                    TelemetryDta = TelemetryDta_ToggleInputSource.ToJson();
                    break;
                case HotkeyType.SwitchInputSource:
                    var TelemetryDta_SwitchInputSource = new ApplicationSettings_Video_swap_hotkey();
                    TelemetryDta_SwitchInputSource.DisplayModelname = models.ToList();
                    TelemetryDta_SwitchInputSource.D_Ctrl = D_Ctrls.ToList();
                    TelemetryDta_SwitchInputSource.DisplayServiceTag = DisplayServiceTags.ToList();
                    TelemetryDta_SwitchInputSource.Video_swap_hotkey = val;
                    TelemetryDta = TelemetryDta_SwitchInputSource.ToJson();
                    break;
                case HotkeyType.ChangePIPPosition:
                    var TelemetryDta_ChangePIPPosition = new ApplicationSettings_PIP_Toggle_hotkey();
                    TelemetryDta_ChangePIPPosition.DisplayModelname = models.ToList();
                    TelemetryDta_ChangePIPPosition.D_Ctrl = D_Ctrls.ToList();
                    TelemetryDta_ChangePIPPosition.DisplayServiceTag = DisplayServiceTags.ToList();
                    TelemetryDta_ChangePIPPosition.PIP_Toggle_hotkey = val;
                    TelemetryDta = TelemetryDta_ChangePIPPosition.ToJson();
                    break;
                case HotkeyType.ToggleEzRecentSetting:
                    var TelemetryDta_ToggleEzRecentSetting = new ApplicationSettings_EasyArrangeRecentHotkey();
                    TelemetryDta_ToggleEzRecentSetting.DisplayModelname = models.ToList();
                    TelemetryDta_ToggleEzRecentSetting.D_Ctrl = D_Ctrls.ToList();
                    TelemetryDta_ToggleEzRecentSetting.DisplayServiceTag = DisplayServiceTags.ToList();
                    TelemetryDta_ToggleEzRecentSetting.EasyArrangeRecentHotkey = val;
                    TelemetryDta = TelemetryDta_ToggleEzRecentSetting.ToJson();
                    break;
                case HotkeyType.VisionEngineToggle:
                    var TelemetryDta_VisionEngineToggle = new ApplicationSettings_VisionEngineHotkey();
                    TelemetryDta_VisionEngineToggle.DisplayModelname = models.ToList();
                    TelemetryDta_VisionEngineToggle.D_Ctrl = D_Ctrls.ToList();
                    TelemetryDta_VisionEngineToggle.DisplayServiceTag = DisplayServiceTags.ToList();
                    TelemetryDta_VisionEngineToggle.VisionEngineHotkey = val;
                    TelemetryDta = TelemetryDta_VisionEngineToggle.ToJson();
                    break;
                case HotkeyType.DarkStabilizerToggle:
                    var TelemetryDta_DarkStabilizerToggle = new ApplicationSettings_BlackStablizer_hotkey();
                    TelemetryDta_DarkStabilizerToggle.DisplayModelname = models.ToList();
                    TelemetryDta_DarkStabilizerToggle.D_Ctrl = D_Ctrls.ToList();
                    TelemetryDta_DarkStabilizerToggle.DisplayServiceTag = DisplayServiceTags.ToList();
                    TelemetryDta_DarkStabilizerToggle.BlackStablizer_hotkey = val;
                    TelemetryDta = TelemetryDta_DarkStabilizerToggle.ToJson();
                    break;
            }
            if (string.IsNullOrEmpty(TelemetryDta))
                return rt;
            if (plugin != null)
                rt = plugin.ReceiveTelemetryInfo("ApplicationSettings", TelemetryDta, Telementry_Frequency.RealTime).Result;

            return rt;
        }

        public bool Send_AppMode_Telementry(ITelementryScheduler plugin, List<MonitorInfo> monitorInfos, string val)
        {
            var rt = false;
            if (string.IsNullOrEmpty(val))
                return rt;
            if (monitorInfos != null && monitorInfos.Count == 0)
                return rt;
            List<string> models = new List<string>();
            List<string> D_Ctrls = new List<string>();
            List<string> DisplayServiceTags = new List<string>();

            foreach (var monitorInfo in monitorInfos)
            {
                models.Add(monitorInfo.modelName);
                D_Ctrls.Add(monitorInfo.D_Ctrl);
                DisplayServiceTags.Add(monitorInfo.edid.ServiceTag);
            }

            var TelemetryDta_AppMode = new ApplicationSettings_AppMode();
            TelemetryDta_AppMode.DisplayModelname = models.ToList();
            TelemetryDta_AppMode.D_Ctrl = D_Ctrls.ToList();
            TelemetryDta_AppMode.DisplayServiceTag = DisplayServiceTags.ToList();
            TelemetryDta_AppMode.AppMode = val;

            if (plugin != null)
                rt = plugin.ReceiveTelemetryInfo("ApplicationSettings", TelemetryDta_AppMode.ToJson(), Telementry_Frequency.RealTime).Result;

            return rt;
        }
        public bool Send_UsedLanguage_Telementry(ITelementryScheduler plugin, List<MonitorInfo> monitorInfos, string val)
        {
            var rt = false;
            if (string.IsNullOrEmpty(val))
                return rt;
            if (monitorInfos != null && monitorInfos.Count == 0)
                return rt;
            List<string> models = new List<string>();
            List<string> D_Ctrls = new List<string>();
            List<string> DisplayServiceTags = new List<string>();

            foreach (var monitorInfo in monitorInfos)
            {
                models.Add(monitorInfo.modelName);
                D_Ctrls.Add(monitorInfo.D_Ctrl);
                DisplayServiceTags.Add(monitorInfo.edid.ServiceTag);
            }

            var TelemetryDta_UsedLanguage = new ApplicationSettings_UsedLanguage();
            TelemetryDta_UsedLanguage.DisplayModelname = models.ToList();
            TelemetryDta_UsedLanguage.D_Ctrl = D_Ctrls.ToList();
            TelemetryDta_UsedLanguage.DisplayServiceTag = DisplayServiceTags.ToList();
            TelemetryDta_UsedLanguage.UsedLanguage = val;

            if (plugin != null)
                rt = plugin.ReceiveTelemetryInfo("ApplicationSettings", TelemetryDta_UsedLanguage.ToJson(), Telementry_Frequency.RealTime).Result;

            return rt;
        }


        public bool Send_Settings_Telementry(ITelementryScheduler plugin, List<MonitorInfo> monitorInfos, string impexp)
        {
            var rt = false;
            List<string> models = new List<string>();
            List<string> D_Ctrls = new List<string>();
            List<string> DisplayServiceTags = new List<string>();

            foreach (var monitorInfo in monitorInfos)
            {
                models.Add(monitorInfo.modelName);
                D_Ctrls.Add(monitorInfo.D_Ctrl);
                DisplayServiceTags.Add(monitorInfo.edid.ServiceTag);
            }

            var TelemetryDta_Settings = new ApplicationSettings_Settings();
            TelemetryDta_Settings.DisplayModelname = models.ToList();
            TelemetryDta_Settings.D_Ctrl = D_Ctrls.ToList();
            TelemetryDta_Settings.DisplayServiceTag = DisplayServiceTags.ToList();
            TelemetryDta_Settings.App_Copy_Settings = impexp;

            if (plugin != null)
                rt = plugin.ReceiveTelemetryInfo("ApplicationSettings", TelemetryDta_Settings.ToJson(), Telementry_Frequency.RealTime).Result;

            return rt;
        }
    }
}