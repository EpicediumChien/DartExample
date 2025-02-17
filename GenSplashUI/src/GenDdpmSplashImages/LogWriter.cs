using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;
using System.Runtime.InteropServices;

/* Usage:
 * 
 * using nsLogWriter;
 * 
 * //InitLogFile
 * LogWriter.Enabled = true;
 * LogWriter.LogPathName = Application.StartupPath + "Mylogfile.log";
 *
 * //Write to log file
 * LogWriter.LogLine("Start Testing...");
 * 
*/
namespace nsLogWriter
{
    //A static log file class, write to the same for overall project
    class LogWriter
    {
        const string DateTimeFormat = "yyyy-MM-dd HH:mm:ss";

        private static string m_strLogPathName;

        //Get/Set LogPathName
        public static string LogPathName
        {
            get { return LogWriter.m_strLogPathName;  }
            set { if (value.Length > 0) LogWriter.m_strLogPathName = value;  }
        }

        public static bool IsLogFieNameInited { get { return (LogPathName != null); } }

        //Enable flag
        private static bool m_blEnabled = false;
        public static bool Enabled
        {
            get { return LogWriter.m_blEnabled; }
            set { LogWriter.m_blEnabled = value; }
        }

        [System.ComponentModel.DefaultValue(true)]
        public static bool BeginWithDateTime { get; set; }

        //Flush log file contents
        public static void Flush()
        {
            if (Enabled)
                File.WriteAllText(LogWriter.LogPathName, string.Empty);
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public static extern void OutputDebugString(string message);

        //Log one line of message
        public static bool LogLine(string strMsg)
        {
            if (!Enabled)
                return true;

            if (strMsg.Length > 0)
            {
                try
                {
                    using (StreamWriter sw = File.AppendText(LogWriter.LogPathName))
                    {
                        if (BeginWithDateTime)
                            sw.WriteLine("[{0}] {1}", DateTime.Now.ToString(DateTimeFormat), strMsg); // DateTime.Now.ToString("[MM/dd,HH:mm:ss]"), strMsg);
                        else
                            sw.WriteLine(strMsg);
                        sw.Flush();
                    }
                } //try
                catch (Exception ex)
                {
                    OutputDebugString("!Exception, LogWriter.LogLine(), Msg=[" + ex.Message + "]");
                    return false;
                }
            }
            return true;
        }
    }
}
