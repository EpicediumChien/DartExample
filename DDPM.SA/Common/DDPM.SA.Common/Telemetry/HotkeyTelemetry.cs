using DDPM.SA.Common.Display;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using VcpCore.Common;
using Windows.Storage.AccessCache;
using Windows.System;

namespace DDPM.SA.Common.Telemetry
{
    public class HotkeyTelemetry
    {
        public string DisplayModelname { get; set; }
        public string DisplayServiceTag { get; set; }
        public string D_Ctrl { get; set; }

        public HotkeyTelemetry()
        {
            DisplayModelname = string.Empty;
            DisplayServiceTag = string.Empty;
            D_Ctrl = string.Empty;
        }
        public string ToJson()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
    }
    public class HotkeyTelemetry_Toggle_2_input_sources_hotkey : HotkeyTelemetry
    {
        public string Toggle_2_input_sources_hotkey { get; set; } = string.Empty;
    }

    public class HotkeyTelemetry_Video_swap_hotkey : HotkeyTelemetry
    {
        public string Video_swap_hotkey { get; set; } = string.Empty;
    }

    public class HotkeyTelemetry_PIP_Toggle_hotkey : HotkeyTelemetry
    {
        public string PIP_Toggle_hotkey { get; set; } = string.Empty;
    }
    public class HotkeyTelemetry_EasyArrangeRecentHotkey : HotkeyTelemetry
    {
        public string EasyArrangeRecentHotkey { get; set; } = string.Empty;
    }

    public class HotkeyTelemetry_VisionEngineHotkey : HotkeyTelemetry
    {
        public string VisionEngineHotkey { get; set; } = string.Empty;
    }
    public class HotkeyTelemetry_BlackStablizer_hotkey : HotkeyTelemetry
    {
        public string BlackStablizer_hotkey { get; set; } = string.Empty;
    }

    public class AppModeTelemetry : HotkeyTelemetry
    {
        public string AppMode { get; set; } = string.Empty;
    }

    public class LanguageTelemetry
    {
        public string Language { get; set; } = string.Empty;

        public LanguageTelemetry()
        {
            Language = string.Empty;
        }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
    }
    public class HotkeyTelemetry_Function
    {
        public bool Send_Hotkey_Telementry(ITelementryScheduler plugin, MonitorInfo monitorInfo, string val, HotkeyType hotkeyType)
        {
            var rt = false;
            string telemetryData = string.Empty;
            if (monitorInfo != null)
            {
                switch (hotkeyType)
                {
                    case HotkeyType.ToggleInputSource:
                        var telToggleInputSource = new HotkeyTelemetry_Toggle_2_input_sources_hotkey();
                        telToggleInputSource.DisplayModelname = monitorInfo.modelName;
                        telToggleInputSource.DisplayServiceTag = monitorInfo.edid.ServiceTag;
                        telToggleInputSource.D_Ctrl = monitorInfo.D_Ctrl;
                        telToggleInputSource.Toggle_2_input_sources_hotkey = val;
                        telemetryData = telToggleInputSource.ToJson();
                        break;
                    case HotkeyType.SwitchInputSource:
                        var telSwitchInputSource = new HotkeyTelemetry_Video_swap_hotkey();
                        telSwitchInputSource.DisplayModelname = monitorInfo.modelName;
                        telSwitchInputSource.DisplayServiceTag = monitorInfo.edid.ServiceTag;
                        telSwitchInputSource.D_Ctrl = monitorInfo.D_Ctrl;
                        telSwitchInputSource.Video_swap_hotkey = val;
                        telemetryData = telSwitchInputSource.ToJson();
                        break;
                    case HotkeyType.ChangePIPPosition:
                        var telChangePIPPosition = new HotkeyTelemetry_PIP_Toggle_hotkey();
                        telChangePIPPosition.DisplayModelname = monitorInfo.modelName;
                        telChangePIPPosition.DisplayServiceTag = monitorInfo.edid.ServiceTag;
                        telChangePIPPosition.D_Ctrl = monitorInfo.D_Ctrl;
                        telChangePIPPosition.PIP_Toggle_hotkey = val;
                        telemetryData = telChangePIPPosition.ToJson();
                        break;
                    case HotkeyType.ToggleEzRecentSetting:
                        var telToggleEzRecentSetting = new HotkeyTelemetry_EasyArrangeRecentHotkey();
                        telToggleEzRecentSetting.DisplayModelname = monitorInfo.modelName;
                        telToggleEzRecentSetting.DisplayServiceTag = monitorInfo.edid.ServiceTag;
                        telToggleEzRecentSetting.D_Ctrl = monitorInfo.D_Ctrl;
                        telToggleEzRecentSetting.EasyArrangeRecentHotkey = val;
                        telemetryData = telToggleEzRecentSetting.ToJson();
                        break;
                    case HotkeyType.VisionEngineToggle:
                        var telVisionEngineToggle = new HotkeyTelemetry_VisionEngineHotkey();
                        telVisionEngineToggle.DisplayModelname = monitorInfo.modelName;
                        telVisionEngineToggle.DisplayServiceTag = monitorInfo.edid.ServiceTag;
                        telVisionEngineToggle.D_Ctrl = monitorInfo.D_Ctrl;
                        telVisionEngineToggle.VisionEngineHotkey = val;
                        telemetryData = telVisionEngineToggle.ToJson();
                        break;
                    case HotkeyType.DarkStabilizerToggle:
                        var telDarkStabilizerToggle = new HotkeyTelemetry_BlackStablizer_hotkey();
                        telDarkStabilizerToggle.DisplayModelname = monitorInfo.modelName;
                        telDarkStabilizerToggle.DisplayServiceTag = monitorInfo.edid.ServiceTag;
                        telDarkStabilizerToggle.D_Ctrl = monitorInfo.D_Ctrl;
                        telDarkStabilizerToggle.BlackStablizer_hotkey = val;
                        telemetryData = telDarkStabilizerToggle.ToJson();
                        break;
                }
                if (plugin != null)
                    rt = plugin.ReceiveTelemetryInfo("ApplicationSettings", telemetryData, Telementry_Frequency.RealTime).Result;
            }
            return rt;
        }
        public Task<bool> Send_AppMode_Telementry(ITelementryScheduler plugin, List<MonitorInfo> monitorInfos, string val)
        {
            var rt = false;
            string telemetryData = string.Empty;
            List<MonitorInfo> mList = new List<MonitorInfo>();
            mList.AddRange(monitorInfos);
            foreach (MonitorInfo monitorInfo in mList)
            {
                var telAppMode = new AppModeTelemetry();
                telAppMode.DisplayModelname = monitorInfo.modelName;
                telAppMode.DisplayServiceTag = monitorInfo.edid.ServiceTag;
                telAppMode.D_Ctrl = monitorInfo.D_Ctrl;
                telAppMode.AppMode = val;
                telemetryData = telAppMode.ToJson();

                if (plugin != null)
                    rt &= plugin.ReceiveTelemetryInfo("ApplicationSettings", telemetryData, Telementry_Frequency.RealTime).Result;
            }
            return Task.FromResult(rt);
        }
        public bool Send_UsedLanguage_Telementry(ITelementryScheduler plugin, string val)
        {
            var rt = false;
            string telemetryData = string.Empty;
            var telUsedLanguage = new LanguageTelemetry();
            telUsedLanguage.Language = val;
            telemetryData = telUsedLanguage.ToJson();

            if (plugin != null)
                rt = plugin.ReceiveTelemetryInfo("UsedLanguage", telemetryData, Telementry_Frequency.FirstDayofMonth).Result;
            return rt;
        }
    }

    public static class HotkeyTelementryHelper
    {
        public static string getHotkeyNoStr(List<VirtualKey> hotKeys)
        {
            List<string> strList = new List<string>(4) { "1Key", " 2Keys", "3Keys", "4Keys" };
            string rt = string.Empty;
            if (hotKeys != null)
            {
                if (hotKeys.Count > 0 && hotKeys.Count <= 4)
                {
                    rt = strList[hotKeys.Count - 1];
                }
            }
            return rt;
        }
        public static string toHotKeyText(List<VirtualKey> newHotKeys)
        {
            string strTmpKey = string.Empty;
            if (newHotKeys.Count == 0)
            {
                return "None";
            }
            int ControlIndex = newHotKeys.IndexOf(VirtualKey.Control);
            int ControlIndexLeft = newHotKeys.IndexOf(VirtualKey.LeftControl);

            int MenuIndex = newHotKeys.IndexOf(VirtualKey.Menu);
            int MenuIndexLeft = newHotKeys.IndexOf(VirtualKey.LeftMenu);

            int ShiftIndex = newHotKeys.IndexOf(VirtualKey.Shift);
            int ShiftIndexLeft = newHotKeys.IndexOf(VirtualKey.LeftShift);

            if ((ControlIndex >= 0) || (ControlIndexLeft >= 0))
                strTmpKey += "Ctrl +";

            if ((MenuIndex >= 0) || (MenuIndexLeft >= 0))
                strTmpKey += "Alt +";

            if ((ShiftIndex >= 0) || (ShiftIndexLeft >= 0))
                strTmpKey += "Shift +";
            foreach (var item in newHotKeys)
            {
                if (item == VirtualKey.Control || item == VirtualKey.Menu || item == VirtualKey.Shift ||
                    item == VirtualKey.LeftControl || item == VirtualKey.LeftMenu || item == VirtualKey.LeftShift)
                {
                    continue;
                }
                if (strTmpKey != "" && !strTmpKey.EndsWith("+"))
                {
                    strTmpKey += "+";
                }
                strTmpKey = strTmpKey + " " + VirtualKeyToString(item);
                break;
            }

            return strTmpKey;
        }
        private static string VirtualKeyToString(VirtualKey vk)
        {
            string strRetKey = string.Empty;

            if (vk >= VirtualKey.Number0 && vk <= VirtualKey.Number9)
            {
                switch (vk)
                {
                    case VirtualKey.Number0:
                        strRetKey = "0";
                        break;

                    case VirtualKey.Number1:
                        strRetKey = "1";
                        break;

                    case VirtualKey.Number2:
                        strRetKey = "2";
                        break;

                    case VirtualKey.Number3:
                        strRetKey = "3";
                        break;

                    case VirtualKey.Number4:
                        strRetKey = "4";
                        break;

                    case VirtualKey.Number5:
                        strRetKey = "5";
                        break;

                    case VirtualKey.Number6:
                        strRetKey = "6";
                        break;

                    case VirtualKey.Number7:
                        strRetKey = "7";
                        break;

                    case VirtualKey.Number8:
                        strRetKey = "8";
                        break;

                    case VirtualKey.Number9:
                        strRetKey = "9";
                        break;
                }
            }
            else
            {
                strRetKey = vk.ToString();
                if (FullWidthCharactersHandler.isHalfWidthString(strRetKey))
                {
                    strRetKey = FullWidthCharactersHandler.convertFullWidthToHalfWidth(strRetKey);
                }
            }

            return strRetKey;
        }
        public static class FullWidthCharactersHandler
        {
            private static Dictionary<char, char> fullWidth2halfWidthDic;

            static FullWidthCharactersHandler()
            {
                fullWidth2halfWidthDic = new Dictionary<char, char>();
                string fullWidthChars = "０１２３４５６７８９ＡＢＣＤＥＦＧＨＩＪＫＬＭＮＯＰＱＲＳＴＵＶＷＸＹＺａｂｃｄｅｆｇｈｉｊｋｌｍｎｏｐｑｒｓｔｕｖｗｘｙｚ";
                string halfWidthChars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
                for (int i = 0; i < fullWidthChars.Length; i++)
                {
                    fullWidth2halfWidthDic.Add(fullWidthChars[i], halfWidthChars[i]);
                }
            }

            public static bool isHalfWidthString(string toTestString)
            {
                bool isHalfWidth = true;
                foreach (char ch in toTestString)
                {
                    if (fullWidth2halfWidthDic.ContainsKey(ch))
                    {
                        isHalfWidth = false;
                        break;
                    }
                }
                return isHalfWidth;
            }

            public static string convertFullWidthToHalfWidth(string theString)
            {
                StringBuilder sbResult = new StringBuilder(theString);
                for (int i = 0; i < theString.Length; i++)
                {
                    if (fullWidth2halfWidthDic.ContainsKey(theString[i]))
                    {
                        sbResult[i] = fullWidth2halfWidthDic[theString[i]];
                    }
                }
                return sbResult.ToString();
            }
        }

    }

}
