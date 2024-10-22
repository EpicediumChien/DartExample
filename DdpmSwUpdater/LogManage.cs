using DDPM.SA.Common;
using DDPM.SA.Common.Settings;
using Dell.Client.Framework.Security;
using PInvoke;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DdpmSwUpdater
{
    public class LogManage
    {
        static string logFilePath = "DdpmSwUpdater.log";
        public static void SetPath()
        {
            DDPMFileSecurity DDPMFileSecurity = new DDPMFileSecurity();
            string AppDataPath = DDPMFileSecurity.GetActiveUserLocalAppDataPath();
            if (!string.IsNullOrEmpty(AppDataPath))
            {
                string path = AppDataPath + "\\Dell\\Dell Display and Peripheral Manager\\Log\\DDPM-Setup-DdpmSwUpdater";
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                logFilePath = path + "\\" + logFilePath;
            }
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            LogMessage($"DdpmSwUpdater Ver:{version}");
        }

        public static void LogMessage(string message)
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(logFilePath, true))
                {
                    writer.WriteLine($"{DateTime.Now}: {message}");
                }
#if DEBUG
                Console.WriteLine($"{DateTime.Now}: {message}");
#endif
            }
            catch
            {

            }
        }
    }
}
