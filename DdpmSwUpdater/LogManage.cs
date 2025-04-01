using DDPM.SA.Common;
using DDPM.SA.Common.Settings;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.Security;
using PInvoke;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using VcpCore.Common;
using static System.Reflection.Metadata.BlobBuilder;

namespace DdpmSwUpdater
{
    public class LogManage
    {
        static string logFilePath = "DdpmSwUpdater.log";
        static string path = string.Empty;

        private static Logs? logs = null;
        public static Logs Logs { get => logs; set => logs = value; }

        private static string version = string.Empty;
        public static string Version { get => version; set => version = value; }

        private static bool fromDDPM = true;
        public static bool FromDDPM { get => fromDDPM; set => fromDDPM = value; }

        public static void SetPath()
        {
            //DDPMFileSecurity DDPMFileSecurity = new DDPMFileSecurity();
            //[Dean] 20250120 change log location to be C:\ProgramData\Dell\DdpmSwUpdater
            string ProgramDataPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);// WTSFunction.GetActiveUserLocalAppDataPath(null);
            if (!string.IsNullOrEmpty(ProgramDataPath))
            {
                path = ProgramDataPath + GlobalDefinitions.LogSwUpdater; //@"\Dell\Dell Display and Peripheral Manager\DdpmSwUpdater";// "\\Dell\\Dell Display and Peripheral Manager\\Log\\DDPM-Setup-DdpmSwUpdater";
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                DDPMFileSecurity.SetFolderPermissions_UserReadAndExecute(path, out string errorMsg);
                logFilePath = path + "\\" + logFilePath;
                Log log = new Log("DdpmSwUpdater", new LogFile(logFilePath), "DdpmSwUpdater");
                logs = new Logs(log, "DdpmSwUpdater");
                LogMessage($"DdpmSwUpdater Ver:{Assembly.GetExecutingAssembly().GetName().Version}");

                //Dean 0124 According to log move into %programdata%\Dell\Dell Display and Peripheral Manager, using oridignal ACL as well 
                /*#if RELEASE
                                try
                                {
                                    bool acl = DDPMFileSecurity.CheckFolderACL(path, out string info);
                                    if(!acl)
                                    {
                                        LogMessage($"SetPath error : {info}");
                                        return;
                                    }
                                }
                                catch (Exception ex)
                                {
                                    LogMessage($"SetPath error : {ex.Message}");
                                    return;
                                }
                #endif*/

            }

        }
        public static SWUpdateHelper GetSWUMetadata(bool isSkipCA)
        {
            SWUpdateHelper swUpdateHelper = new SWUpdateHelper();
            try
            {
                List<string> InfoPkey = new List<string>(DDPM.SA.Obfuscation.InfoHash.Info_Hash);
                //InfoPkey.Add(DDPM.SA.Obfuscation.InfoHash.Info_Hash);
                swUpdateHelper = SWUpdateSetting.GetSWMetadata(isSkipCA, out string getMetadataInfo, null, InfoPkey, LogManage.Logs);
                LogMessage($"GetMetadata {getMetadataInfo}");
            }
            catch (Exception ex)
            {
                LogMessage($"GetMetadata error : {ex.Message}");
            }

            return swUpdateHelper;
        }
        public static bool GetCheckCAStatus()
        {
            bool isSkipCA = false;
            object o = DDPMRegistryHelper.ReadRegistryKey(RegistryHive.LocalMachine, "SOFTWARE\\Dell\\DDPM Subagent", "SkipCA");
            if (o != null && o is string && !string.IsNullOrEmpty(o.ToString()))
            {
                isSkipCA = o.Equals("1") ? true : false;
            }
            return isSkipCA;
        }
        public static bool GetCheckSHAStatus()
        {
            bool isSkipSHA = false;
            object o = DDPMRegistryHelper.ReadRegistryKey(RegistryHive.LocalMachine, "SOFTWARE\\Dell\\DDPM Subagent", "SkipSHA");
            if (o != null && o is string && !string.IsNullOrEmpty(o.ToString()))
            {
                isSkipSHA = o.Equals("1") ? true : false;
            }
            return isSkipSHA;
        }
        public static Logs RetrieveLogObject()
        {
            return Logs;
        }

        public static void LogMessage(string message)
        {
            try
            {
                //using (StreamWriter writer = new StreamWriter(logFilePath, true))
                //{
                //    writer.WriteLine($"{DateTime.Now}: {message}");
                //}
                //if (!Directory.Exists(path))
                //{
                //    Directory.CreateDirectory(path);
                //}

                // [Checkmarx] Avoid Log Forging
                string safeMessage = message.Replace("\n", "").Replace("\r", "");
                safeMessage = System.Security.SecurityElement.Escape(safeMessage);

                if (Logs != null)
                {
                    Logs.DebugMsg_1(safeMessage);
                }
#if DEBUG
                Console.WriteLine($"{DateTime.Now}: {safeMessage}");
#endif
            }
            catch (Exception ex)
            {
#if DEBUG
                Console.WriteLine($"LogMessage exception, message: {ex.Message}");
#endif
            }
        }
    }
}
