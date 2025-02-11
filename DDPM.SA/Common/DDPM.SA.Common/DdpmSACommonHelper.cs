using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace DDDPM.SA.Common
{
    public class DdpmSACommonHelper
    {
        public static Stopwatch SALaunchTimer = new Stopwatch();

        public static Stopwatch SAUserLaunchTimer = new Stopwatch();

        private static Dictionary<string, bool> SAPluginList = new Dictionary<string, bool>
        {
            { "SettingsMangerPlugin", false},
            { "SWUpdatePlugins", false},
            { "FWUpdatePlugins", false}
        };

        private static Dictionary<string, bool> SAUserPluginList = new Dictionary<string, bool>
        {
            { "DeviceMangerPlugin", false}
        };

        private const string fullPath = $@"HKEY_LOCAL_MACHINE\SOFTWARE\Dell\Dell Display And Peripheral Manager\UserSettings";

        private const string keyName_DebugWinEventLogTime = "DebugWinEventLogTime";

        private static bool isTimerOn = false;

        static DdpmSACommonHelper()
        {
             isTimerOn = (string)Registry.GetValue(fullPath, keyName_DebugWinEventLogTime, null) == "1";
        }

        public static void SAPluginReady(string NameOfPlugin)
        {
            SAPluginList[NameOfPlugin] = true;
            if (SAPluginList.Values.Where(v => v).Count() == SAPluginList.Count)
            {
                if(isTimerOn)
                    WriteToEventLog($"{SALaunchTimer.Elapsed.TotalMilliseconds:F2} ms - {nameof(SAPluginReady)} started.", EventLogEntryType.Information
                        , "DDPM.SA.Launcher", $"{nameof(SAPluginReady)}");
                SALaunchTimer.Stop();
            }
        }

        public static void SAUserPluginReady(string NameOfPlugin)
        {
            SAUserPluginList[NameOfPlugin] = true;
            if (SAUserPluginList.Values.Where(v => v).Count() == SAUserPluginList.Count)
            {
                if (isTimerOn)
                    WriteToEventLog($"{SAUserLaunchTimer.Elapsed.TotalMilliseconds:F2} ms - {nameof(SAUserPluginReady)}.", EventLogEntryType.Information
                        , "DDPM.SA.User.Launcher", $"{nameof(SAUserPluginReady)}");
                SAUserLaunchTimer.Stop();
            }
        }

        private static void WriteToEventLog(string message, EventLogEntryType type, string eventSource, string LogName)
        {
            using (EventLog eventLog = new EventLog(LogName))
            {
                eventLog.Source = eventSource;
                eventLog.WriteEntry(message, type);
            }
        }
    }
}
