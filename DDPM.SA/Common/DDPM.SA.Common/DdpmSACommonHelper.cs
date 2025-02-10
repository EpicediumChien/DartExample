using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace DDDPM.SA.Common
{
    public class DdpmSACommonHelper
    {
        private const string EventSource = "DDPM.SA.Launcher";

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

        public static void SAPluginReady(string NameOfPlugin)
        {
            SAPluginList[NameOfPlugin] = true;
            if (SAPluginList.Values.Where(v => v).Count() == SAPluginList.Count)
            {
                WriteToEventLog($"{SALaunchTimer.Elapsed.TotalMilliseconds:F2} ms - {nameof(SAPluginReady)} started.", EventLogEntryType.Information, $"[{nameof(SAPluginReady)}]");
                SALaunchTimer.Stop();
            }
        }

        public static void SAUserPluginReady(string NameOfPlugin)
        {
            SAUserPluginList[NameOfPlugin] = true;
            if (SAUserPluginList.Values.Where(v => v).Count() == SAPluginList.Count)
            {
                WriteToEventLog($"{SAUserLaunchTimer.Elapsed.TotalMilliseconds:F2} ms - {nameof(SAUserPluginReady)}.", EventLogEntryType.Information, $"[{nameof(SAUserPluginReady)}]");
                SAUserLaunchTimer.Stop();
            }
        }

        private static void WriteToEventLog(string message, EventLogEntryType type, string LogName)
        {
            using (EventLog eventLog = new EventLog(LogName))
            {
                eventLog.Source = EventSource;
                eventLog.WriteEntry(message, type);
            }
        }
    }
}
