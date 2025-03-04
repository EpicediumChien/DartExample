using DDPM.SA.Common.Settings;
using Dell.Client.Framework.Common;
using System;
using System.IO;

namespace DDPM.SA.Common
{
    public static class DiagnosticReport
    {
        public static bool SaveLogFile(string saveFolderPath, ILog log)
        {
            log.Info($"{nameof(SaveLogFile)} start");
            bool ret = true;
            log.Info($"{nameof(SaveLogFile)} WTSFunction._WTSGetActiveConsoleSessionId() : {WTSFunction._WTSGetActiveConsoleSessionId()}");
            if (WTSFunction._WTSGetActiveConsoleSessionId() >= 1)
            {
                string fail_info = "[SaveLogFile] : saveFolderPath : " + saveFolderPath + ", ";
                string success_info = "[SaveLogFile] : saveFolderPath : " + saveFolderPath + ", ";
                if (!string.IsNullOrEmpty(saveFolderPath))
                {
                    log.Info($"{nameof(SaveLogFile)} saveFolderPath : {saveFolderPath}");
                    DDPM.SA.Common.Method.Method method = new Method.Method(log);
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
                    //log.Info($"folderPath - programdataPath Line 60: programdataPath is null : {string.IsNullOrEmpty(programdataPath)}");
                    string appDataPath = WTSFunction.GetActiveUserLocalAppDataPath(log);
                    //log.Info($"folderPath - appDataPath Line 62: appDataPath is null : {string.IsNullOrEmpty(appDataPath)}");
                    try
                    {
                        // AppDataPath
                        if (!string.IsNullOrEmpty(appDataPath))
                        {
                            log.Info($"folderPath - appDataPath Line 68: appDataPath  : {appDataPath}");
                            // DDPM.Subagent.User Log
                            string LogFolder = @$"{appDataPath}{GlobalDefinitions.LogDDPMUSERSA}";//\Dell\Dell Display and Peripheral Manager\Log\DDPM.Subagent.User";
                            log.Info("folderPath - LogFolder Line 71: " + LogFolder);
                            if (method.DirectoryContainsFiles(LogFolder))
                            {
                                // 取得資料夾名稱
                                string folderName = method.GetFolderName(LogFolder);
                                string savePath = Path.Combine(saveFolderPath, folderName);
                                // 複製指定的 log 文件到選擇的資料夾
                                if (!method.CopyLogFolder(LogFolder, savePath))
                                {
                                    fail_info += "[DDPM.Subagent.User] : Fail, ";
                                    log.Info("SaveLogFile - DDPM.Subagent.User : Fail ");
                                    ret = false;
                                }
                                else
                                {
                                    success_info += "[DDPM.Subagent.User] : Success, ";
                                    log.Info("SaveLogFile - DDPM.Subagent.User : Success ");
                                }
                            }
                            else
                            {
                                success_info += "[DDPM.Subagent.User] : No Log File, ";
                                log.Info("SaveLogFile - DDPM.Subagent.User : No Log File ");
                            }

                            // DDPM.GUI Log
                            LogFolder = @$"{appDataPath}{GlobalDefinitions.LogDDPMGUI}";//\Dell\Dell Display and Peripheral Manager\Log\DDPM.GUI";
                            log.Info("folderPath - LogFolder Line 98: " + LogFolder);
                            if (method.DirectoryContainsFiles(LogFolder))
                            {
                                // 取得資料夾名稱
                                string folderName = method.GetFolderName(LogFolder);
                                string savePath = Path.Combine(saveFolderPath, folderName);
                                // 複製指定的 log 文件到選擇的資料夾
                                if (!method.CopyLogFolder(LogFolder, savePath))
                                {
                                    fail_info += "[DDPM.GUI] : Fail, ";
                                    log.Info("SaveLogFile - DDPM.GUI : Fail ");
                                    ret = false;
                                }
                                else
                                {
                                    success_info += "[DDPM.GUI] : Success, ";
                                    log.Info("SaveLogFile - DDPM.GUI : Success ");
                                }
                            }
                            else
                            {
                                success_info += "[DDPM.GUI] : No Log File, ";
                                log.Info("SaveLogFile - DDPM.GUI : No Log File ");
                            }
                        }
                        else
                        {
                            fail_info += "SaveLogFile - [AppDataPath] null, ";
                            log.Error("SaveLogFile - AppDataPath is null, it means is no active user currently");
                        }

                        // ProgramDataPath
                        if (!string.IsNullOrEmpty(programdataPath))
                        {
                            log.Info($"folderPath - ProgramDataPath Line 131: programdataPath  : {programdataPath}");
                            // DDPM.Subagent Log
                            string LogFolder = @$"{programdataPath}{GlobalDefinitions.LogDDPMSYSSA}";//\Dell\DDPM.Subagent";
                            log.Info("folderPath - LogFolder Line 135: " + LogFolder);
                            if (method.DirectoryContainsFiles(LogFolder))
                            {
                                // 取得資料夾名稱
                                string folderName = method.GetFolderName(LogFolder);
                                string savePath = Path.Combine(saveFolderPath, folderName);
                                // 複製指定的 log 文件到選擇的資料夾
                                if (!method.CopyLogFolder(LogFolder, savePath))
                                {
                                    fail_info += "[DDPM.Subagent] : Fail, ";
                                    log.Info("SaveLogFile - DDPM.Subagent : Fail ");
                                    ret = false;
                                }
                                else
                                {
                                    success_info += "[DDPM.Subagent] : Success, ";
                                    log.Info("SaveLogFile - DDPM.Subagent : Success ");
                                }
                            }
                            else
                            {
                                success_info += "[DDPM.Subagent] : No Log File, ";
                                log.Info("SaveLogFile - [DDPM.Subagent : No Log File ");
                            }
                            // Dell TechHub Log
                            LogFolder = @$"{programdataPath}{GlobalDefinitions.LogDTH}";//\Dell\Dell TechHub";
                            log.Info("folderPath - LogFolder Line 161: " + LogFolder);
                            if (method.DirectoryContainsFiles(LogFolder))
                            {
                                // 取得資料夾名稱
                                string folderName = method.GetFolderName(LogFolder);
                                string savePath = Path.Combine(saveFolderPath, folderName);
                                // 複製指定的 log 文件到選擇的資料夾
                                if (!method.CopyLogFolder(LogFolder, savePath))
                                {
                                    fail_info += "[Dell TechHub] : Fail, ";
                                    log.Info("SaveLogFile - Dell TechHub : Fail ");
                                    ret = false;
                                }
                                else
                                {
                                    success_info += "[Dell TechHub] : Success, ";
                                    log.Info("SaveLogFile - Dell TechHub : Success ");
                                }
                            }
                            else
                            {  
                                fail_info +="[Dell TechHub] : No Log File, ";
                                log.Info("SaveLogFile - Dell TechHub : No Log File ");
                            }
                            // DTP Log
                            LogFolder = @$"{programdataPath}{GlobalDefinitions.LogDTP}";//\Dell\DTP\Logs";
                            log.Info("folderPath - LogFolder Line 187: " + LogFolder);
                            if (method.DirectoryContainsFiles(LogFolder))
                            {
                                // 取得資料夾名稱
                                string folderName = "DTP_Log";
                                string savePath = Path.Combine(saveFolderPath, folderName);
                                // 複製指定的 log 文件到選擇的資料夾
                                if (!method.CopyLogFolder(LogFolder, savePath))
                                {
                                    fail_info += "[DTP_log] : Fail, ";
                                    log.Info("SaveLogFile - DTP_log : Fail ");
                                    ret = false;
                                }
                                else
                                {
                                    success_info += "[DTP_log] : Success, ";
                                    log.Info("SaveLogFile - DTP_log : Success ");
                                }
                            }
                            else
                            {
                                fail_info += "[DTP_log] : No Log File, ";
                                log.Info("SaveLogFile - DTP_log : No Log File ");
                            }
                            // DDPMW-NKVM Log
                            string registryKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\DDPMW-NKVM";
                            object o = DDPMRegistryHelper.ReadRegistryKey(RegistryHive.LocalMachine, registryKey, "GUID");
                            if (o != null && o is string && !string.IsNullOrEmpty(o.ToString()))
                            {
                                LogFolder = @$"{programdataPath}\{o.ToString()}\DDPMW-NKVM";
                                log.Info("folderPath - LogFolder Line 217: " + LogFolder);
                                if (method.DirectoryContainsFiles(LogFolder))
                                {
                                    // 取得資料夾名稱
                                    string folderName = method.GetFolderName(LogFolder);
                                    string savePath = Path.Combine(saveFolderPath, folderName);
                                    // 複製指定的 log 文件到選擇的資料夾
                                    if (!method.CopyLogFolder(LogFolder, savePath))
                                    {
                                        fail_info += "[DDPMW-NKVM] : Fail, ";
                                        log.Info("SaveLogFile - DDPMW-NKVM : Fail ");
                                        ret = false;
                                    }
                                    else
                                    {
                                        success_info += "[DDPMW-NKVM] : Success, ";
                                        log.Info("SaveLogFile - DDPMW-NKVM: Success ");
                                    }
                                }
                            }
                            else
                            {
                                fail_info += "[DDPMW-NKVM] : No Log File, ";
                                log.Info("SaveLogFile - DDPMW-NKVM : No Log File ");
                            }
                            // DPMService Log
                            LogFolder = @$"{programdataPath}{GlobalDefinitions.LogDPMService}";//\Dell\Dell Peripheral Manager\DPMService\Log";
                            log.Info("folderPath - LogFolder Line 244: " + LogFolder);
                            if (method.DirectoryContainsFiles(LogFolder))
                            {
                                // 取得資料夾名稱
                                string folderName = "DPMService_Log";
                                string savePath = Path.Combine(saveFolderPath, folderName);
                                // 複製指定的 log 文件到選擇的資料夾
                                if (!method.CopyLogFolder(LogFolder, savePath))
                                {
                                    fail_info += "[DPMService_Log] : Fail, ";
                                    log.Info("SaveLogFile - DPMService_Log : Fail ");
                                    ret = false;
                                }
                                else
                                {
                                    success_info += "[DPMService_Log] : Success, ";
                                    log.Info("SaveLogFile - DPMService_Log : Success ");
                                }
                            }
                            else
                            {
                                fail_info += "[DPMService_Log] : No Log File, ";
                                log.Info("SaveLogFile - DPMService_Log : No Log File ");
                            }
                            // DPM Log
                            LogFolder = @$"{programdataPath}{GlobalDefinitions.LogDPM}";//\Dell\Dell Peripheral Manager\DPM\Log";
                            log.Info("folderPath - LogFolder Line 270: " + LogFolder);
                            if (method.DirectoryContainsFiles(LogFolder))
                            {
                                // 取得資料夾名稱
                                string folderName = "DPM_Log";
                                string savePath = Path.Combine(saveFolderPath, folderName);
                                // 複製指定的 log 文件到選擇的資料夾
                                if (!method.CopyLogFolder(LogFolder, savePath))
                                {
                                    fail_info += "[DPM_Log] : Fail, ";
                                    log.Info("SaveLogFile - DPM_Log : Fail ");
                                    ret = false;
                                }
                                else
                                {
                                    success_info += "[DPM_Log] : Success, ";
                                    log.Info("SaveLogFile - DPM_Log : Success ");
                                }
                            }
                            else
                            {
                                fail_info += "[DPM_Log] : No Log File, ";
                                log.Info("SaveLogFile - DPM_Log : No Log File ");
                            }
                            // DPeMSDK Log
                            LogFolder = @$"{programdataPath}{GlobalDefinitions.LogDPeM}";//\Dell\Dell Peripheral Manager\DPeMSDK\Log";
                            log.Info("folderPath - LogFolder Line 296: " + LogFolder);
                            if (method.DirectoryContainsFiles(LogFolder))
                            {
                                // 取得資料夾名稱
                                string folderName = "DPeMSDK_Log";
                                string savePath = Path.Combine(saveFolderPath, folderName);
                                // 複製指定的 log 文件到選擇的資料夾
                                if (!method.CopyLogFolder(LogFolder, savePath))
                                {
                                    fail_info += "[DPeMSDK_Log] : Fail, ";
                                    log.Info("SaveLogFile - DPeMSDK_Log : Fail ");
                                    ret = false;
                                }
                                else
                                {
                                    success_info += "[DPeMSDK_Log] : Success, ";
                                    log.Info("SaveLogFile - DPeMSDK_Log : Success ");
                                }
                            }
                            else
                            {
                                fail_info += "[DPeMSDK_Log] : No Log File, ";
                                log.Info("SaveLogFile - DPeMSDK_Log : No Log File ");
                            }
                            // SwUpdater Log
                            LogFolder = @$"{programdataPath}{GlobalDefinitions.LogSwUpdater}";//\Dell\DdpmSwUpdater";
                            log.Info("folderPath - LogFolder Line 322: " + LogFolder);
                            if (method.DirectoryContainsFiles(LogFolder))
                            {
                                // 取得資料夾名稱
                                string folderName = method.GetFolderName(LogFolder);
                                string savePath = Path.Combine(saveFolderPath, folderName);
                                // 複製指定的 log 文件到選擇的資料夾
                                if (!method.CopyLogFolder(LogFolder, savePath))
                                {
                                    fail_info += "[DDPM.SwUpdater] : Fail, ";
                                    log.Info("SaveLogFile - DDPM.SwUpdater : Fail ");
                                    ret = false;
                                }
                                else
                                {
                                    success_info += "[DDPM.SwUpdater] : Success, ";
                                    log.Info("SaveLogFile - DDPM.SwUpdater : Success ");
                                }
                            }
                            else
                            {
                                fail_info += "[DDPM.SwUpdater] : No Log File, ";
                                log.Info("SaveLogFile - DDPM.SwUpdater : No Log File ");
                            }
                            // FwUpdater Log
                            LogFolder = @$"{programdataPath}{GlobalDefinitions.LogFwUpdater}";//\Dell\FWUpdateLog";
                            log.Info("folderPath - LogFolder Line 348: " + LogFolder);
                            if (method.DirectoryContainsFiles(LogFolder))
                            {
                                // 取得資料夾名稱
                                string folderName = method.GetFolderName(LogFolder);
                                string savePath = Path.Combine(saveFolderPath, folderName);
                                // 複製指定的 log 文件到選擇的資料夾
                                if (!method.CopyLogFolder(LogFolder, savePath))
                                {
                                    fail_info += "[DDPM.FwUpdate] : Fail, ";
                                    log.Info("SaveLogFile - DDPM.FwUpdate : Fail ");
                                    ret = false;
                                }
                                else
                                {
                                    success_info += "[DDPM.FwUpdate] : Success, ";
                                    log.Info("SaveLogFile - DDPM.FwUpdate : Success ");
                                }
                            }
                            else
                            {
                                fail_info += "[DDPM.FwUpdate] : No Log File, ";
                                log.Info("SaveLogFile - DDPM.FwUpdate : No Log File ");
                            }
                        }
                        else
                        {
                            fail_info += "[ProgramDataPath] null, ";
                            log.Error("ProgramDataPath is null, it means is no active user currently");
                        }
                        // EventLog
                        string logFileName = "EventLog.evtx";
                        string logFilePath = Path.Combine(saveFolderPath, logFileName);
                        if (!method.ExecuteWevtutilCommand(logFilePath))
                        {
                            fail_info += "[EventLog] : Fail, ";
                            log.Info("SaveLogFile - EventLog : Fail ");
                        }
                        else
                        {
                            success_info += "[EventLog] : Success, ";
                            log.Info("SaveLogFile - EventLog : Success ");
                        }

                        // NKVM install.log
                        string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                        // 組合相對路徑
                        string sourcePath = Path.Combine(baseDir, "Plugins", "NKVM");
                        string fileName = "install.log";
                        string sourceFile = Path.Combine(sourcePath, fileName);
                        log.Info("SourceFile - SourceFile Line 398: " + sourceFile);
                        if (method.DirectoryContainsFiles(sourcePath))
                        {
                            string savePath = Path.Combine(saveFolderPath, "NKVM_Log");
                            // 複製指定的 log 文件到選擇的資料夾
                            if (!method.CopyLogFolder(sourceFile, savePath))
                            {
                                fail_info += "[NKVM/install.log] : Fail, ";
                                log.Info("SaveLogFile - NKVM/install.log : Fail ");
                                ret = false;
                            }
                            else
                            {
                                success_info += "[NKVM/install.log] : Success, ";
                                log.Info("SaveLogFile -NKVM/install.log : Success ");
                            }
                        }
                        else
                        {
                            fail_info += "[NKVM/install.log] : No Log File, ";
                            log.Info("SaveLogFile - NKVM/install.log : No Log File ");
                        }
                        // Wayn add Dell registry record file
                        string dellRegFolderPath = Path.Combine(saveFolderPath, "Dell_Reg");
                        if (!Directory.Exists(dellRegFolderPath))
                        {
                            // 建一個File放Reg_Record.txt
                            Directory.CreateDirectory(dellRegFolderPath);
                            log.Info("SaveLogFile - Create Dell_Reg File ");
                        }

                        // 讀取 HKEY_LOCAL_MACHINE\SOFTWARE\Dell
                        string fullRegistryPath = @"HKEY_LOCAL_MACHINE\SOFTWARE\Dell";
                        var regValues = DDPMRegistryHelper.ReadAllRegistryValuesRecursively(fullRegistryPath);

                        // 寫入 Dell_Reg\Reg_Record.txt
                        string regTxtFile = Path.Combine(dellRegFolderPath, "Reg_Record.txt");
                        using (var writer = new StreamWriter(regTxtFile, false))
                        {
                            if (regValues.Count == 0)
                            {
                                writer.WriteLine("No registry values found or path does not exist.");
                                fail_info += "[Dell Reg] : No Reg value, ";
                                log.Info("SaveLogFile - No registry values found or path does not exist.");
                            }
                            else
                            {
                                success_info += "[Dell Reg] : Get Reg value, ";
                                foreach (var kvp in regValues)
                                {
                                    writer.WriteLine($"{kvp.Key} = {kvp.Value ?? "(null)"}");
                                }
                            }
                        }
                        log.Info("SaveLogFile - Dell registry values record done.");
                        // Dell registry record end

                        // Write SaveLogFile log
                        string resultInfoFile = Path.Combine(saveFolderPath, "SaveLogInfo.txt");
                        using (var writer = new StreamWriter(resultInfoFile, false))
                        {
                            // Success
                            writer.WriteLine("=== Success Info ===");
                            if (!string.IsNullOrEmpty(success_info))
                            {
                                // 逗號換行
                                var successItems = success_info.Split(',', StringSplitOptions.RemoveEmptyEntries);
                                foreach (string item in successItems)
                                {
                                    writer.WriteLine(item.Trim());
                                }
                            }
                            else
                            {
                                writer.WriteLine("No success info.");
                            }

                            writer.WriteLine(); // 空一行

                            // Fail
                            writer.WriteLine("=== Fail Info ===");
                            if (!string.IsNullOrEmpty(fail_info))
                            {
                                var failItems = fail_info.Split(',', StringSplitOptions.RemoveEmptyEntries);
                                foreach (string item in failItems)
                                {
                                    writer.WriteLine(item.Trim());
                                }
                            }
                            else
                            {
                                writer.WriteLine("No fail info.");
                            }
                        }

                        // zip
                        string zipFilePath = saveFolderPath + ".zip";
                        if (!method.CreateZipFile(saveFolderPath, zipFilePath))// 壓縮資料夾
                            fail_info += "[Compression]";

                        if (DDPMFileSecurity.ValidateFilePath(saveFolderPath, out string info))
                            Directory.Delete(saveFolderPath, true);
                        else
                            log.Error($"[SaveLog] skip delete temp folder due to: {info}");

                        ret = true;
                        //if (fail_info.Length > 0)
                        //{
                        //    //ret = false;
                        //    log.Error($"SaveLog was failed at following step(s): {fail_info}");
                        //}
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
                else
                {
                    ret = false;
                    log.Info($"{nameof(SaveLogFile)} saveFolderPath is null : {string.IsNullOrEmpty(saveFolderPath)}");
                }
            }
            else
            {
                log.Error($"{nameof(SaveLogFile)} WTSFunction._WTSGetActiveConsoleSessionId get <=0");
                ret = false;
            }
            log.Info($"{nameof(SaveLogFile)} : {ret.ToString()} end");
            return ret;
        }

    }
}