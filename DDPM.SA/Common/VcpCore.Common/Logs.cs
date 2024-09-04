using Dell.Client.Framework.Common;
using System;

namespace VcpCore.Common
{
    [Serializable]
    public class Logs
    {
        private const bool _IsDebugEnable = true;
        private ILog Logg;

        private string _PluginLogId = string.Empty;

        public Logs(ILog Logx)
        { Logg = Logx; }

        public Logs(ILog Logx, string PluginLogId)
        { Logg = Logx; _PluginLogId = PluginLogId; }

        public void DebugMsg(string DebugMsg, bool IsDebugEnable = _IsDebugEnable)
        {
            if (IsDebugEnable)
            {
                if (Logg != null) // Elie, check if it's null or not.
                    Logg.Info("[VcpCore_DebugMsg][INFO] " + DebugMsg);

                Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss.fff") + " " + DebugMsg);
            }
        }

        public void DebugMsg_1(string DebugMsg, bool IsDebugEnable = _IsDebugEnable)
        {
            if (IsDebugEnable)
            {
                string s = $"[{_PluginLogId}][INFO] " + DebugMsg;
                if (Logg != null) // Elie, check if it's null or not.
                    Logg.Info(s);

                Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss.fff") + " " + s);
            }
        }

        public void Info(string DebugMsg, bool IsDebugEnable = _IsDebugEnable)
        {
            if (IsDebugEnable)
            {
                string s = $"[{_PluginLogId}] " + DebugMsg;
                if (Logg != null) // Elie, check if it's null or not.
                    Logg.Info(s);

                Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss.fff") + " " + s);
            }
        }

        public void Error(string DebugMsg, bool IsDebugEnable = _IsDebugEnable)
        {
            if (IsDebugEnable)
            {
                string s = $"[{_PluginLogId}]" + DebugMsg;
                if (Logg != null) // Elie, check if it's null or not.
                    Logg.Error(s);

                Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss.fff") + " " + s);
            }
        }
    }
}