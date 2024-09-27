using DDPM.SA.Common;
using DDPM.SA.Common.Settings;
using Dell.Client.Framework.Security;
using PInvoke;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniInstaller
{
    public class LogManage
    {
        static string logFilePath = "log.txt"; // 日誌檔案路徑
        public static void SetPath()
        {
            DDPMFileSecurity DDPMFileSecurity = new DDPMFileSecurity();
            logFilePath = DDPMFileSecurity.GetActiveUserLocalAppDataPath() + "\\Dell\\Dell Display and Peripheral Manager\\Log\\DDPM-Setup-MiniInstall\\DDPM-Setup-MiniInstall.log";
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
