using Dell.Client.Framework.Common;
using System;

namespace VcpCore.Common
{
    [Serializable]
    public class Logs
    {
        private ILog Logg;

        private string _PluginLogId = string.Empty;

        public Logs(ILog Logx)
        { Logg = Logx; }

        public Logs(ILog Logx, string PluginLogId)
        { Logg = Logx; _PluginLogId = PluginLogId; }

        public void DebugMsg(string DebugMsg)
        {
            if (Logg != null) // Elie, check if it's null or not.
                Logg.Info("[INFO] " + DebugMsg);
#if DEBUG
            Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss.fff") + " " + DebugMsg);
#endif
        }

        public void DebugMsg_1(string DebugMsg)
        {
            string s = $"[{_PluginLogId}][INFO] " + DebugMsg;
            if (Logg != null) // Elie, check if it's null or not.
                Logg.Info(s);
#if DEBUG
            Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss.fff") + " " + s);
#endif
        }

        public void Info(string DebugMsg)
        {
            string s = $"[{_PluginLogId}] " + DebugMsg;
            if (Logg != null) // Elie, check if it's null or not.
                Logg.Info(s);
#if DEBUG
            Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss.fff") + " " + s);
#endif
        }

        public void Error(string DebugMsg)
        {
            string s = $"[{_PluginLogId}]" + DebugMsg;
            if (Logg != null) // Elie, check if it's null or not.
                Logg.Error(s);
#if DEBUG
            Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss.fff") + " " + s);
#endif
        }
    }
}