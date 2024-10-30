using System.Collections.Generic;
using System.Linq;
using System.Threading;
using VcpCore.Common;

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
        public bool Send_LowBatteryLevel_Telementry(ITelementryScheduler plugin, List<MonitorInfo> monitorInfos, bool LowBatteryLevel)
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

            var TelemetryDta_LowBatteryLevel = new ApplicationSettings_LowBatteryLevel();
            TelemetryDta_LowBatteryLevel.DisplayModelname = models.ToList();
            TelemetryDta_LowBatteryLevel.D_Ctrl = D_Ctrls.ToList();
            TelemetryDta_LowBatteryLevel.DisplayServiceTag = DisplayServiceTags.ToList();
            TelemetryDta_LowBatteryLevel.LowBatteryLevel = LowBatteryLevel ? "Yes" : "No";

            if (plugin != null)
                rt = plugin.ReceiveTelemetryInfo("ApplicationSettings", TelemetryDta_LowBatteryLevel.ToJson(), Telementry_Frequency.RealTime).Result;

            return rt;
        }
        public bool Send_KeyboardLockKey_Telementry(ITelementryScheduler plugin, List<MonitorInfo> monitorInfos, bool KeyboardLockKey)
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

            var TelemetryDta_KeyboardLockKey = new ApplicationSettings_KeyboardLockKey();
            TelemetryDta_KeyboardLockKey.DisplayModelname = models.ToList();
            TelemetryDta_KeyboardLockKey.D_Ctrl = D_Ctrls.ToList();
            TelemetryDta_KeyboardLockKey.DisplayServiceTag = DisplayServiceTags.ToList();
            TelemetryDta_KeyboardLockKey.KeyboardLockKey = KeyboardLockKey ? "Yes" : "No";

            if (plugin != null)
                rt = plugin.ReceiveTelemetryInfo("ApplicationSettings", TelemetryDta_KeyboardLockKey.ToJson(), Telementry_Frequency.RealTime).Result;

            return rt;
        }
        public bool Send_WB7022CoverState_Telementry(ITelementryScheduler plugin, List<MonitorInfo> monitorInfos, bool WB7022CoverState)
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

            var TelemetryDta_WB7022CoverState = new ApplicationSettings_WB7022CoverState();
            TelemetryDta_WB7022CoverState.DisplayModelname = models.ToList();
            TelemetryDta_WB7022CoverState.D_Ctrl = D_Ctrls.ToList();
            TelemetryDta_WB7022CoverState.DisplayServiceTag = DisplayServiceTags.ToList();
            TelemetryDta_WB7022CoverState.WB7022CoverState = WB7022CoverState ? "Yes" : "No";

            if (plugin != null)
                rt = plugin.ReceiveTelemetryInfo("ApplicationSettings", TelemetryDta_WB7022CoverState.ToJson(), Telementry_Frequency.RealTime).Result;

            return rt;
        }
        public bool Send_MuteState_Telementry(ITelementryScheduler plugin, List<MonitorInfo> monitorInfos, bool MuteState)
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

            var TelemetryDta_MuteState = new ApplicationSettings_MuteState();
            TelemetryDta_MuteState.DisplayModelname = models.ToList();
            TelemetryDta_MuteState.D_Ctrl = D_Ctrls.ToList();
            TelemetryDta_MuteState.DisplayServiceTag = DisplayServiceTags.ToList();
            TelemetryDta_MuteState.MuteState = MuteState ? "Yes" : "No";

            if (plugin != null)
                rt = plugin.ReceiveTelemetryInfo("ApplicationSettings", TelemetryDta_MuteState.ToJson(), Telementry_Frequency.RealTime).Result;

            return rt;
        }
        public bool Send_ColorPresetAndEasyMemory_Telementry(ITelementryScheduler plugin, List<MonitorInfo> monitorInfos, bool ColorPresetAndEasyMemory)
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

            var TelemetryDta_ColorPresetAndEasyMemory = new ApplicationSettings_ColorPresetAndEasyMemory();
            TelemetryDta_ColorPresetAndEasyMemory.DisplayModelname = models.ToList();
            TelemetryDta_ColorPresetAndEasyMemory.D_Ctrl = D_Ctrls.ToList();
            TelemetryDta_ColorPresetAndEasyMemory.DisplayServiceTag = DisplayServiceTags.ToList();
            TelemetryDta_ColorPresetAndEasyMemory.ColorPresetAndEasyMemory = ColorPresetAndEasyMemory ? "Yes" : "No";

            if (plugin != null)
                rt = plugin.ReceiveTelemetryInfo("ApplicationSettings", TelemetryDta_ColorPresetAndEasyMemory.ToJson(), Telementry_Frequency.RealTime).Result;

            return rt;
        }
        public bool Send_EnableQuickAccessWidget_Telementry(ITelementryScheduler plugin, List<MonitorInfo> monitorInfos, bool EnableQuickAccessWidget)
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

            var TelemetryDta_EnableQuickAccessWidget = new ApplicationSettings_EnableQuickAccessWidget();
            TelemetryDta_EnableQuickAccessWidget.DisplayModelname = models.ToList();
            TelemetryDta_EnableQuickAccessWidget.D_Ctrl = D_Ctrls.ToList();
            TelemetryDta_EnableQuickAccessWidget.DisplayServiceTag = DisplayServiceTags.ToList();
            TelemetryDta_EnableQuickAccessWidget.EnableQuickAccessWidget = EnableQuickAccessWidget ? "Yes" : "No";

            if (plugin != null)
                rt = plugin.ReceiveTelemetryInfo("ApplicationSettings", TelemetryDta_EnableQuickAccessWidget.ToJson(), Telementry_Frequency.RealTime).Result;

            return rt;
        }
        public bool Send_EnableQuickAccessWidget_Reminder_Telementry(ITelementryScheduler plugin, List<MonitorInfo> monitorInfos, bool EnableQuickAccessWidget_Reminder)
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

            var TelemetryDta_EnableQuickAccessWidget_Reminder = new ApplicationSettings_EnableQuickAccessWidget_Reminder();
            TelemetryDta_EnableQuickAccessWidget_Reminder.DisplayModelname = models.ToList();
            TelemetryDta_EnableQuickAccessWidget_Reminder.D_Ctrl = D_Ctrls.ToList();
            TelemetryDta_EnableQuickAccessWidget_Reminder.DisplayServiceTag = DisplayServiceTags.ToList();
            TelemetryDta_EnableQuickAccessWidget_Reminder.EnableQuickAccessWidget_Reminder = EnableQuickAccessWidget_Reminder ? "Yes" : "No";

            if (plugin != null)
                rt = plugin.ReceiveTelemetryInfo("ApplicationSettings", TelemetryDta_EnableQuickAccessWidget_Reminder.ToJson(), Telementry_Frequency.RealTime).Result;

            return rt;
        }
        public bool Send_SaveDiagnosticReport_Telementry(ITelementryScheduler plugin, List<MonitorInfo> monitorInfos, int count)
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

            var TelemetryDta_SaveDiagnosticReport_Reminder = new ApplicationSettings_SaveDiagnosticReport();
            TelemetryDta_SaveDiagnosticReport_Reminder.DisplayModelname = models.ToList();
            TelemetryDta_SaveDiagnosticReport_Reminder.D_Ctrl = D_Ctrls.ToList();
            TelemetryDta_SaveDiagnosticReport_Reminder.DisplayServiceTag = DisplayServiceTags.ToList();
            TelemetryDta_SaveDiagnosticReport_Reminder.SaveDiagnosticReport = count.ToString();

            if (plugin != null)
                rt = plugin.ReceiveTelemetryInfo("ApplicationSettings", TelemetryDta_SaveDiagnosticReport_Reminder.ToJson(), Telementry_Frequency.RealTime).Result;

            return rt;
        }
        public bool Send_SaveMonitorAssetReport_Telementry(ITelementryScheduler plugin, List<MonitorInfo> monitorInfos, int count)
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

            var TelemetryDta_SaveMonitorAssetReport_Reminder = new ApplicationSettings_SaveMonitorAssetReport();
            TelemetryDta_SaveMonitorAssetReport_Reminder.DisplayModelname = models.ToList();
            TelemetryDta_SaveMonitorAssetReport_Reminder.D_Ctrl = D_Ctrls.ToList();
            TelemetryDta_SaveMonitorAssetReport_Reminder.DisplayServiceTag = DisplayServiceTags.ToList();
            TelemetryDta_SaveMonitorAssetReport_Reminder.SaveMonitorAssetReport = count.ToString();

            if (plugin != null)
                rt = plugin.ReceiveTelemetryInfo("ApplicationSettings", TelemetryDta_SaveMonitorAssetReport_Reminder.ToJson(), Telementry_Frequency.RealTime).Result;

            return rt;
        }
    }
}