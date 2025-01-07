using DDPM.SA.Common.Settings;
using DDPM.SA.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Compression;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Dell.Client.Framework.Common;
using DDPM.SA.Common.Method;
using System.Windows.Media.Animation;
using Windows.Devices.Geolocation;

namespace DDPM.SA.Plugins.SettingsManager
{
    public class DiagnosticReport
    {
        public static bool SaveLogFile(string saveFolderPath, ILog log)
        {
            log.Info($"{nameof(SaveLogFile)} start");
            bool ret = false;
            log.Info($"{nameof(SaveLogFile)} WTSFunction._WTSGetActiveConsoleSessionId() : {WTSFunction._WTSGetActiveConsoleSessionId()}");
            if (WTSFunction._WTSGetActiveConsoleSessionId() >= 1)
            {
                log.Info($"{nameof(SaveLogFile)} saveFolderPath is null : {string.IsNullOrEmpty(saveFolderPath)}");
                if (!string.IsNullOrEmpty(saveFolderPath))
                {
                    Method method = new Method(log);
                    // 確保資料夾存在
                    if (!Directory.Exists(saveFolderPath))
                    {
                        Directory.CreateDirectory(saveFolderPath);
                    }
                    //0913 Bruce Add Security
                    string FolderInfo;
                    string PathSymbolicLinInfo;
                    int count = 0;
                    bool folderValid = false;
                    do
                    {
                        FolderInfo = string.Empty;
                        PathSymbolicLinInfo = string.Empty;
                        folderValid = false;
                        /*folderValid = !DDPMFileSecurity.IsPathSymbolicLinked(saveFolderPath, out PathSymbolicLinInfo);
                        if (!folderValid)
                        {
                            log.Info(nameof(DownloadAndInstall) + " FolderIsNotSafe:" + PathSymbolicLinInfo + " Retry:" + (count++));
                            //Do remove Symbolic Link than delete folder
                            //Directory.Delete(saveFolderPath, true);
                            //Directory.CreateDirectory(saveFolderPath);
                        }*/
                        // The function call IsPathSymbolicLinked is merged to "IsFolderPathValid"
                        //folderValid = DDPMFileSecurity.IsFolderPathValid(saveFolderPath, out FolderInfo);// && folderValid;
                        folderValid = DDPMFileSecurity.ValidateFilePath(saveFolderPath, out FolderInfo); //[Dean 1216] Validate with sanitized string check
                        if (!folderValid)
                        {
                            log.Info(nameof(SaveLogFile) + " FolderIsNotSafe:" + FolderInfo + " Retry:" + (count++));
                            /*//Do remove Symbolic Link than delete folder
                            Directory.Delete(saveFolderPath, true);
                            Directory.CreateDirectory(saveFolderPath);*/
                            return (false); //[Dean] don't remove folder to avoid callback attack
                        }
                    } while (!folderValid && count < 2);

                    string programdataPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
                    log.Info($"folderPath - programdataPath Line 68: programdataPath is null : {string.IsNullOrEmpty(programdataPath)}");
                    string appDataPath = WTSFunction.GetActiveUserLocalAppDataPath(log);
                    log.Info($"folderPath - appDataPath Line 70: appDataPath is null : {string.IsNullOrEmpty(appDataPath)}");
                    string fail_info = string.Empty;
                    try
                    {
                        if (!string.IsNullOrEmpty(appDataPath))
                        {
                            log.Info($"folderPath - appDataPath Line 76: appDataPath  : {appDataPath}");
                            string LogFolder = @$"{appDataPath}\Dell\Dell Display and Peripheral Manager\Log\DDPM.Subagent.User";
                            log.Info("folderPath - LogFolder Line 78: " + LogFolder);
                            if (method.DirectoryContainsFiles(LogFolder))
                            {
                                // 取得資料夾名稱
                                string folderName = method.GetFolderName(LogFolder);
                                string savePath = Path.Combine(saveFolderPath, folderName);
                                // 複製指定的 log 文件到選擇的資料夾
                                if (!method.CopyLogFolder(LogFolder, savePath))
                                    fail_info += "[DDPM.Subagent.User]";
                            }
                            LogFolder = @$"{appDataPath}\Dell\Dell Display and Peripheral Manager\Log\DDPM.GUI";
                            log.Info("folderPath - LogFolder Line 89: " + LogFolder);
                            if (method.DirectoryContainsFiles(LogFolder))
                            {
                                // 取得資料夾名稱
                                string folderName = method.GetFolderName(LogFolder);
                                string savePath = Path.Combine(saveFolderPath, folderName);
                                // 複製指定的 log 文件到選擇的資料夾
                                if (!method.CopyLogFolder(LogFolder, savePath))
                                    fail_info += "[DDPM.GUI]";
                            }
                            LogFolder = @$"{appDataPath}\Dell\Dell Display and Peripheral Manager\Log\DDPM-Setup-DdpmSwUpdater";
                            log.Info("folderPath - LogFolder Line 100: " + LogFolder);
                            if (method.DirectoryContainsFiles(LogFolder))
                            {
                                // 取得資料夾名稱
                                string folderName = method.GetFolderName(LogFolder);
                                string savePath = Path.Combine(saveFolderPath, folderName);
                                // 複製指定的 log 文件到選擇的資料夾
                                if (!method.CopyLogFolder(LogFolder, savePath))
                                    fail_info += "[DDPM.SwUpdater]";
                            }
                            LogFolder = @$"{appDataPath}\Dell\Dell Display and Peripheral Manager\Log\FWUpdataLog";
                            log.Info("folderPath - LogFolder Line 111: " + LogFolder);
                            if (method.DirectoryContainsFiles(LogFolder))
                            {
                                // 取得資料夾名稱
                                string folderName = method.GetFolderName(LogFolder);
                                string savePath = Path.Combine(saveFolderPath, folderName);
                                // 複製指定的 log 文件到選擇的資料夾
                                if (!method.CopyLogFolder(LogFolder, savePath))
                                    fail_info += "[DDPM.FwUpdate]";
                            }
                        }
                        else
                        {
                            log.Error("appDataPath is null, it means is no active user currently");
                        }
                        if (!string.IsNullOrEmpty(programdataPath))
                        {
                            log.Info($"folderPath - appDataPath Line 128: programdataPath  : {programdataPath}");
                            string LogFolder = @$"{programdataPath}\Dell\DDPM.Subagent";
                            log.Info("folderPath - LogFolder Line 130: " + LogFolder);
                            if (method.DirectoryContainsFiles(LogFolder))
                            {
                                // 取得資料夾名稱
                                string folderName = method.GetFolderName(LogFolder);
                                string savePath = Path.Combine(saveFolderPath, folderName);
                                // 複製指定的 log 文件到選擇的資料夾
                                if (!method.CopyLogFolder(LogFolder, savePath))
                                    fail_info += "[DDPM.Subagent]";
                            }
                            LogFolder = @$"{programdataPath}\Dell\Dell TechHub";
                            log.Info("folderPath - LogFolder Line 141: " + LogFolder);
                            if (method.DirectoryContainsFiles(LogFolder))
                            {
                                // 取得資料夾名稱
                                string folderName = method.GetFolderName(LogFolder);
                                string savePath = Path.Combine(saveFolderPath, folderName);
                                // 複製指定的 log 文件到選擇的資料夾
                                if (!method.CopyLogFolder(LogFolder, savePath))
                                    fail_info += "[Dell TechHub]";
                            }
                            LogFolder = @$"{programdataPath}\Dell\DTP\Logs";
                            log.Info("folderPath - LogFolder Line 152: " + LogFolder);
                            if (method.DirectoryContainsFiles(LogFolder))
                            {
                                // 取得資料夾名稱
                                string folderName = "DTP_Log";
                                string savePath = Path.Combine(saveFolderPath, folderName);
                                // 複製指定的 log 文件到選擇的資料夾
                                if (!method.CopyLogFolder(LogFolder, savePath))
                                    fail_info += "[DTP_log]";
                            }
                            string registryKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\DDPMW-NKVM";
                            object o = DDPMRegistryHelper.ReadRegistryKey(RegistryHive.LocalMachine, registryKey, "GUID");
                            if (o != null && o is string && !string.IsNullOrEmpty(o.ToString()))
                            {
                                LogFolder = @$"{programdataPath}\{o.ToString()}\DDPMW-NKVM";
                                log.Info("folderPath - LogFolder Line 167: " + LogFolder);
                                if (method.DirectoryContainsFiles(LogFolder))
                                {
                                    // 取得資料夾名稱
                                    string folderName = method.GetFolderName(LogFolder);
                                    string savePath = Path.Combine(saveFolderPath, folderName);
                                    // 複製指定的 log 文件到選擇的資料夾
                                    if (!method.CopyLogFolder(LogFolder, savePath))
                                        fail_info += "[DDPMW-NKVM]";
                                }
                            }
                            LogFolder = @$"{programdataPath}\Dell\Dell Peripheral Manager\DPMService\Log";
                            log.Info("folderPath - LogFolder Line 179: " + LogFolder);
                            if (method.DirectoryContainsFiles(LogFolder))
                            {
                                // 取得資料夾名稱
                                string folderName = "DPMService_Log";
                                string savePath = Path.Combine(saveFolderPath, folderName);
                                // 複製指定的 log 文件到選擇的資料夾
                                if (!method.CopyLogFolder(LogFolder, savePath))
                                    fail_info += "[DPMService_Log]";
                            }
                            LogFolder = @$"{programdataPath}\Dell\Dell Peripheral Manager\DPM\Log";
                            log.Info("folderPath - LogFolder Line 190: " + LogFolder);
                            if (method.DirectoryContainsFiles(LogFolder))
                            {
                                // 取得資料夾名稱
                                string folderName = "DPM_Log";
                                string savePath = Path.Combine(saveFolderPath, folderName);
                                // 複製指定的 log 文件到選擇的資料夾
                                if (!method.CopyLogFolder(LogFolder, savePath))
                                    fail_info += "[DPM_Log]";
                            }
                            LogFolder = @$"{programdataPath}\Dell\Dell Peripheral Manager\DPeMSDK\Log";
                            log.Info("folderPath - LogFolder Line 201: " + LogFolder);
                            if (method.DirectoryContainsFiles(LogFolder))
                            {
                                // 取得資料夾名稱
                                string folderName = "DPeMSDK_Log";
                                string savePath = Path.Combine(saveFolderPath, folderName);
                                // 複製指定的 log 文件到選擇的資料夾
                                if (!method.CopyLogFolder(LogFolder, savePath))
                                    fail_info += "[DPeMSDK_Log]";
                            }
                        }
                        string logFileName = "EventLog.evtx";
                        string logFilePath = Path.Combine(saveFolderPath, logFileName);
                        if (!method.ExecuteWevtutilCommand(logFilePath))
                            fail_info += "[EventLog]";

                        string zipFilePath = saveFolderPath + ".zip";
                        if (!method.CreateZipFile(saveFolderPath, zipFilePath))// 壓縮資料夾
                            fail_info += "[Compression]";

                        if(DDPMFileSecurity.ValidateFilePath(saveFolderPath, out string info))
                            Directory.Delete(saveFolderPath, true);
                        else
                            log.Error($"[SaveLog] skip delete temp folder due to: {info}");

                        ret = true;
                        if (fail_info.Length > 0)
                        {
                            ret = false;
                            log.Error($"SaveLog was failed at following step(s): {fail_info}");
                        }
                    }
                    catch (Exception ex)
                    {
                        log.Error($"{nameof(SaveLogFile)} got exception ({ex.Message})");
                        ret = false;
                    }
                    if (method != null)
                    {
                        method.Dispose();
                        method = null;
                    }
                }
            }
            else
            {
                log.Error($"{nameof(SaveLogFile)} WTSFunction._WTSGetActiveConsoleSessionId get <=0");
            }
            log.Info($"{nameof(SaveLogFile)} end");
            return ret;
        }

    }
}