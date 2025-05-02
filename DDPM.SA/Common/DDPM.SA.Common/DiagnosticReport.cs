using DDPM.SA.Common.Settings;
using Dell.Client.Framework.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace DDPM.SA.Common
{
    public static class DiagnosticReport
    {
        public static bool SaveLogFile(string saveFolderPath, ILog log)
        {
            log.Info($"{nameof(SaveLogFile)} start");

            //Install software information
            CommonFunctions.IsServiceRunning(GlobalDefinitions.DPeMServiceName, log);//add log before save
            log.Info($"DPeM installed ver: {CommonFunctions.GetInstalledSoftwareVersion(GlobalDefinitions.InstalledName_DPeM, log)}");
            log.Info($"NKVM installed ver: {CommonFunctions.GetInstalledSoftwareVersion(GlobalDefinitions.InstalledName_NKVM, log)}");

            bool ret = true;
            log.Info($"{nameof(SaveLogFile)} WTSFunction._WTSGetActiveConsoleSessionId() : {WTSFunction._WTSGetActiveConsoleSessionId()}");
            if (WTSFunction._WTSGetActiveConsoleSessionId() >= 1)
            {
                string startTimestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                string fail_info = string.Empty;
                string success_info = "[*****************StartTime] : " + startTimestamp + ", ";
                string path_info = "[************SaveFolderPath] = " + saveFolderPath + ", ";
                if (!string.IsNullOrEmpty(saveFolderPath))
                {
                    log.Info($"{nameof(SaveLogFile)} saveFolderPath : {saveFolderPath}");
                    //DDPM.SA.Common.Method.Method method = new Method.Method(log);
                    using (DDPM.SA.Common.Method.Method method = new Method.Method(log))
                    {
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
                        string appDataPath = WTSFunction.GetActiveUserLocalAppDataPath(log);
                        if (!string.IsNullOrEmpty(appDataPath))
                        {
                            path_info += "[***************AppDataPath] = From WTS, ";
                            log.Info($"folderPath - WTSFunction appDataPath = {appDataPath}");
                        }
                        else
                        {
                            path_info += "[***************AppDataPath] = WTSFunction : null, ";
                            log.Info($"folderPath - ActiveUserLocalAppDataPath is null or empty. Fallback to Environment.LocalApplicationData.");

                            // win32 重取 AppDataPath
                            path_info += "[***************AppDataPath] From Common, ";
                            appDataPath = CommonFunctions.GetLocalApplicationData(log);
                            log.Info($"folderPath - Environment SpecialFolder LocalApplicationData");
                        }

                        try
                        {
                            string LogFolder = string.Empty;
                            // AppDataPath
                            if (!string.IsNullOrEmpty(appDataPath))
                            {
                                path_info += "[***************AppDataPath] = " + appDataPath + ", ";
                                log.Info($"folderPath - appDataPath : {appDataPath}");
                                // DDPM.Subagent.User Log
                                try
                                {
                                    LogFolder = @$"{appDataPath}{GlobalDefinitions.LogDDPMUSERSA}";//\Dell\Dell Display and Peripheral Manager\Log\DDPM.Subagent.User";
                                    path_info += "[********DDPM.Subagent.User] = " + LogFolder + ", ";
                                    log.Info("folderPath - LogFolder : " + LogFolder);
                                    if (method.DirectoryContainsFiles(LogFolder))
                                    {
                                        // 取得資料夾名稱
                                        string folderName = method.GetFolderName(LogFolder);
                                        string savePath = Path.Combine(saveFolderPath, folderName);
                                        // 複製指定的 log 文件到選擇的資料夾
                                        if (!method.CopyLogFolder(LogFolder, savePath))
                                        {
                                            fail_info += "[********DDPM.Subagent.User] : Some File Denied, ";
                                            log.Info("SaveLogFile - DDPM.Subagent.User : Some File Denied ");
                                            ret = false;
                                        }
                                        else
                                        {
                                            success_info += "[********DDPM.Subagent.User] : Success, ";
                                            log.Info("SaveLogFile - DDPM.Subagent.User : Success ");
                                        }
                                    }
                                    else
                                    {
                                        success_info += "[********DDPM.Subagent.User] : No Log File, ";
                                        log.Info("SaveLogFile - DDPM.Subagent.User : No Log File ");
                                    }
                                }
                                catch (Exception ex)
                                {
                                    fail_info += "[********DDPM.Subagent.User] : Exception Fail, ";
                                    log.Error($"SaveLogFile - DDPM.Subagent.User : {ex.Message}");
                                }

                                // DDPM.GUI Log
                                try
                                {
                                    LogFolder = @$"{appDataPath}{GlobalDefinitions.LogDDPMGUI}";//\Dell\Dell Display and Peripheral Manager\Log\DDPM.GUI";
                                    path_info += "[******************DDPM.GUI] = " + LogFolder + ", ";
                                    log.Info("folderPath - LogFolder : " + LogFolder);
                                    if (method.DirectoryContainsFiles(LogFolder))
                                    {
                                        // 取得資料夾名稱
                                        string folderName = method.GetFolderName(LogFolder);
                                        string savePath = Path.Combine(saveFolderPath, folderName);
                                        // 複製指定的 log 文件到選擇的資料夾
                                        if (!method.CopyLogFolder(LogFolder, savePath))
                                        {
                                            fail_info += "[******************DDPM.GUI] : Some File Denied, ";
                                            log.Info("SaveLogFile - DDPM.GUI : Some File Denied ");
                                            ret = false;
                                        }
                                        else
                                        {
                                            success_info += "[******************DDPM.GUI] : Success, ";
                                            log.Info("SaveLogFile - DDPM.GUI : Success ");
                                        }
                                    }
                                    else
                                    {
                                        success_info += "[******************DDPM.GUI] : No Log File, ";
                                        log.Info("SaveLogFile - DDPM.GUI : No Log File ");
                                    }
                                }
                                catch (Exception ex)
                                {
                                    fail_info += "[******************DDPM.GUI] : Exception Fail, ";
                                    log.Error($"SaveLogFile - DDPM.GUI : {ex.Message}");
                                }
                            }
                            else
                            {
                                path_info += "[***************AppDataPath] : null, ";
                                fail_info += "[***************AppDataPath] : null, ";
                                log.Error("SaveLogFile - AppDataPath is null, it means is no active user currently");
                            }

                            // ProgramDataPath
                            if (!string.IsNullOrEmpty(programdataPath))
                            {
                                path_info += "[***********ProgramDataPath] = " + programdataPath + ", ";
                                log.Info($"folderPath - ProgramDataPath : programdataPath  : {programdataPath}");
                                // DDPM.Subagent Log
                                try
                                {
                                    LogFolder = @$"{programdataPath}{GlobalDefinitions.LogDDPMSYSSA}";//\Dell\DDPM.Subagent";
                                    path_info += "[*************DDPM.Subagent] = " + LogFolder + ", ";
                                    log.Info("folderPath - LogFolder : " + LogFolder);
                                    if (method.DirectoryContainsFiles(LogFolder))
                                    {
                                        // 取得資料夾名稱
                                        string folderName = method.GetFolderName(LogFolder);
                                        string savePath = Path.Combine(saveFolderPath, folderName);
                                        // 複製指定的 log 文件到選擇的資料夾
                                        if (!method.CopyLogFolder(LogFolder, savePath))
                                        {
                                            fail_info += "[*************DDPM.Subagent] : Some File Denied, ";
                                            log.Info("SaveLogFile - DDPM.Subagent : Some File Denied ");
                                            ret = false;
                                        }
                                        else
                                        {
                                            success_info += "[*************DDPM.Subagent] : Success, ";
                                            log.Info("SaveLogFile - DDPM.Subagent : Success ");
                                        }
                                    }
                                    else
                                    {
                                        success_info += "[*************DDPM.Subagent] : No Log File, ";
                                        log.Info("SaveLogFile - [DDPM.Subagent : No Log File ");
                                    }
                                }
                                catch (Exception ex)
                                {
                                    fail_info += "[*************DDPM.Subagent] : Exception Fail, ";
                                    log.Error($"SaveLogFile - DDPM.Subagent : {ex.Message}");
                                }

                                // Dell TechHub Log
                                try
                                {
                                    LogFolder = @$"{programdataPath}{GlobalDefinitions.LogDTH}";//\Dell\Dell TechHub";
                                    path_info += "[**************Dell TechHub] = " + LogFolder + ", ";
                                    log.Info("folderPath - LogFolder : " + LogFolder);
                                    if (method.DirectoryContainsFiles(LogFolder))
                                    {
                                        // 取得資料夾名稱
                                        string folderName = method.GetFolderName(LogFolder);
                                        string savePath = Path.Combine(saveFolderPath, folderName);
                                        // 複製指定的 log 文件到選擇的資料夾
                                        if (!method.CopyLogFolder(LogFolder, savePath))
                                        {
                                            fail_info += "[**************Dell TechHub] : Some File Denied, ";
                                            log.Info("SaveLogFile - Dell TechHub : Some File Denied ");
                                            ret = false;
                                        }
                                        else
                                        {
                                            success_info += "[**************Dell TechHub] : Success, ";
                                            log.Info("SaveLogFile - Dell TechHub : Success ");
                                        }
                                    }
                                    else
                                    {
                                        fail_info += "[**************Dell TechHub] : No Log File, ";
                                        log.Info("SaveLogFile - Dell TechHub : No Log File ");
                                    }
                                }
                                catch (Exception ex)
                                {
                                    fail_info += "[**************Dell TechHub] : Exception Fail, ";
                                    log.Error($"SaveLogFile - Dell TechHub : {ex.Message}");
                                }

                                // DTP Log
                                try
                                {
                                    LogFolder = @$"{programdataPath}{GlobalDefinitions.LogDTP}";//\Dell\DTP\Logs";
                                    path_info += "[*******************DTP_log] = " + LogFolder + ", ";
                                    log.Info("folderPath - LogFolder : " + LogFolder);
                                    if (method.DirectoryContainsFiles(LogFolder))
                                    {
                                        // 取得資料夾名稱
                                        string folderName = "DTP_Log";
                                        string savePath = Path.Combine(saveFolderPath, folderName);
                                        // 複製指定的 log 文件到選擇的資料夾
                                        if (!method.CopyLogFolder(LogFolder, savePath))
                                        {
                                            fail_info += "[*******************DTP_log] : Some File Denied, ";
                                            log.Info("SaveLogFile - DTP_log : Some File Denied ");
                                            ret = false;
                                        }
                                        else
                                        {
                                            success_info += "[*******************DTP_log] : Success, ";
                                            log.Info("SaveLogFile - DTP_log : Success ");
                                        }
                                    }
                                    else
                                    {
                                        fail_info += "[*******************DTP_log] : No Log File, ";
                                        log.Info("SaveLogFile - DTP_log : No Log File ");
                                    }
                                }
                                catch (Exception ex)
                                {
                                    fail_info += "[*******************DTP_log] : Exception Fail, ";
                                    log.Error($"SaveLogFile - DTP_log : {ex.Message}");
                                }

                                // DDPMW-NKVM Log
                                try
                                {
                                    string registryKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\DDPMW-NKVM";
                                    object o = DDPMRegistryHelper.ReadRegistryKey(RegistryHive.LocalMachine, registryKey, "GUID");
                                    if (o != null && o is string && !string.IsNullOrEmpty(o.ToString()))
                                    {
                                        LogFolder = @$"{programdataPath}\{o.ToString()}\DDPMW-NKVM";
                                        path_info += "[****************DDPMW-NKVM] = " + LogFolder + ", ";
                                        log.Info("folderPath - LogFolder : " + LogFolder);
                                        if (method.DirectoryContainsFiles(LogFolder))
                                        {
                                            // 取得資料夾名稱
                                            string folderName = method.GetFolderName(LogFolder);
                                            string savePath = Path.Combine(saveFolderPath, folderName);
                                            // 複製指定的 log 文件到選擇的資料夾
                                            if (!method.CopyLogFolder(LogFolder, savePath))
                                            {
                                                fail_info += "[****************DDPMW-NKVM] : Some File Denied, ";
                                                log.Info("SaveLogFile - DDPMW-NKVM : Some File Denied ");
                                                ret = false;
                                            }
                                            else
                                            {
                                                success_info += "[****************DDPMW-NKVM] : Success, ";
                                                log.Info("SaveLogFile - DDPMW-NKVM: Success ");
                                            }
                                        }
                                    }
                                    else
                                    {
                                        fail_info += "[****************DDPMW-NKVM] : No Log File, ";
                                        log.Info("SaveLogFile - DDPMW-NKVM : No Log File ");
                                    }
                                }
                                catch (Exception ex)
                                {
                                    fail_info += "[****************DDPMW-NKVM] : Exception Fail, ";
                                    log.Error($"SaveLogFile - DDPMW-NKVM : {ex.Message}");
                                }

                                // DPMService Log
                                try
                                {
                                    LogFolder = @$"{programdataPath}{GlobalDefinitions.LogDPMService}";//\Dell\Dell Peripheral Manager\DPMService\Log";
                                    path_info += "[************DPMService_Log] = " + LogFolder + ", ";
                                    log.Info("folderPath - LogFolder : " + LogFolder);
                                    if (method.DirectoryContainsFiles(LogFolder))
                                    {
                                        // 取得資料夾名稱
                                        string folderName = "DPMService_Log";
                                        string savePath = Path.Combine(saveFolderPath, folderName);
                                        // 複製指定的 log 文件到選擇的資料夾
                                        if (!method.CopyLogFolder(LogFolder, savePath))
                                        {
                                            fail_info += "[************DPMService_Log] : Some File Denied, ";
                                            log.Info("SaveLogFile - DPMService_Log : Some File Denied ");
                                            ret = false;
                                        }
                                        else
                                        {
                                            success_info += "[************DPMService_Log] : Success, ";
                                            log.Info("SaveLogFile - DPMService_Log : Success ");
                                        }
                                    }
                                    else
                                    {
                                        fail_info += "[************DPMService_Log] : No Log File, ";
                                        log.Info("SaveLogFile - DPMService_Log : No Log File ");
                                    }
                                }
                                catch (Exception ex)
                                {
                                    fail_info += "[************DPMService_Log] : Exception Fail, ";
                                    log.Error($"SaveLogFile - DPMService_Log : {ex.Message}");
                                }

                                // DPM Log
                                try
                                {
                                    LogFolder = @$"{programdataPath}{GlobalDefinitions.LogDPM}";//\Dell\Dell Peripheral Manager\DPM\Log";
                                    path_info += "[*******************DPM_Log] = " + LogFolder + ", ";
                                    log.Info("folderPath - LogFolder : " + LogFolder);
                                    if (method.DirectoryContainsFiles(LogFolder))
                                    {
                                        // 取得資料夾名稱
                                        string folderName = "DPM_Log";
                                        string savePath = Path.Combine(saveFolderPath, folderName);
                                        // 複製指定的 log 文件到選擇的資料夾
                                        if (!method.CopyLogFolder(LogFolder, savePath))
                                        {
                                            fail_info += "[*******************DPM_Log] : Some File Denied, ";
                                            log.Info("SaveLogFile - DPM_Log : Some File Denied ");
                                            ret = false;
                                        }
                                        else
                                        {
                                            success_info += "[*******************DPM_Log] : Success, ";
                                            log.Info("SaveLogFile - DPM_Log : Success ");
                                        }
                                    }
                                    else
                                    {
                                        fail_info += "[*******************DPM_Log] : No Log File, ";
                                        log.Info("SaveLogFile - DPM_Log : No Log File ");
                                    }
                                }
                                catch (Exception ex)
                                {
                                    fail_info += "[*******************DPM_Log] : Exception Fail, ";
                                    log.Error($"SaveLogFile - DPM_Log : {ex.Message}");
                                }

                                // DPeMSDK Log
                                try
                                {
                                    LogFolder = @$"{programdataPath}{GlobalDefinitions.LogDPeM}";//\Dell\Dell Peripheral Manager\DPeMSDK\Log";
                                    path_info += "[***************DPeMSDK_Log] = " + LogFolder + ", ";
                                    log.Info("folderPath - LogFolder : " + LogFolder);
                                    if (method.DirectoryContainsFiles(LogFolder))
                                    {
                                        // 取得資料夾名稱
                                        string folderName = "DPeMSDK_Log";
                                        string savePath = Path.Combine(saveFolderPath, folderName);
                                        // 複製指定的 log 文件到選擇的資料夾
                                        if (!method.CopyLogFolder(LogFolder, savePath))
                                        {
                                            fail_info += "[***************DPeMSDK_Log] : Some File Denied, ";
                                            log.Info("SaveLogFile - DPeMSDK_Log : Some File Denied ");
                                            ret = false;
                                        }
                                        else
                                        {
                                            success_info += "[***************DPeMSDK_Log] : Success, ";
                                            log.Info("SaveLogFile - DPeMSDK_Log : Success ");
                                        }
                                    }
                                    else
                                    {
                                        fail_info += "[***************DPeMSDK_Log] : No Log File, ";
                                        log.Info("SaveLogFile - DPeMSDK_Log : No Log File ");
                                    }
                                }
                                catch (Exception ex)
                                {
                                    fail_info += "[***************DPeMSDK_Log] : Exception Fail, ";
                                    log.Error($"SaveLogFile - DPeMSDK_Log : {ex.Message}");
                                }

                                // LogDDPM Log, merge SwUpdater Log and FwUpdater Log
                                try
                                {
                                    LogFolder = @$"{programdataPath}{GlobalDefinitions.LogDDPM}";//\Dell\Dell Display and Peripheral Manager";
                                    path_info += "[**********************DDPM] = " + LogFolder + ", ";
                                    log.Info("folderPath - LogFolder : " + LogFolder);
                                    if (method.DirectoryContainsFiles(LogFolder))
                                    {
                                        // 取得資料夾名稱
                                        string folderName = method.GetFolderName(LogFolder);
                                        string savePath = Path.Combine(saveFolderPath, folderName);
                                        // 複製指定的 log 文件到選擇的資料夾
                                        if (!method.CopyLogFolder(LogFolder, savePath))
                                        {
                                            fail_info += "[**********************DDPM] : Some File Denied, ";
                                            log.Info("SaveLogFile - DDPM : Fail ");
                                            ret = false;
                                        }
                                        else
                                        {
                                            success_info += "[**********************DDPM] : Success, ";
                                            log.Info("SaveLogFile - DDPM : Success ");
                                        }
                                    }
                                    else
                                    {
                                        fail_info += "[**********************DDPM] : No Log File, ";
                                        log.Info("SaveLogFile - DDPM : No Log File ");
                                    }
                                }
                                catch (Exception ex)
                                {
                                    fail_info += "[**********************DDPM] : Exception Fail, ";
                                    log.Error($"SaveLogFile - DDPM : {ex.Message}");
                                }
                            }
                            else
                            {
                                path_info += "[***********ProgramDataPath] : null, ";
                                fail_info += "[***********ProgramDataPath] : null, ";
                                log.Error("SaveLogFile - ProgramDataPath is null, it means is no active user currently");
                            }

                            // Application EventLog
                            try
                            {
                                string logFileName = "Application_EventLog.evtx";
                                string logFilePath = Path.Combine(saveFolderPath, logFileName);
                                path_info += "[******Application EventLog] = " + logFilePath + ", ";
                                if (!method.ExecuteWevtutilCommand(logFilePath, "Application"))
                                {
                                    fail_info += "[******Application EventLog] : Fail, ";
                                    log.Info("SaveLogFile - EventLog : Fail ");
                                }
                                else
                                {
                                    success_info += "[******Application EventLog] : Success, ";
                                    log.Info("SaveLogFile - Application EventLog : Success ");
                                }
                            }
                            catch (Exception ex)
                            {
                                fail_info += "[******Application EventLog] : Exception Fail, ";
                                log.Error($"SaveLogFile - Application EventLog : {ex.Message}");
                            }
                            // System EventLog
                            try
                            {
                                string logFileName = "System_EventLog.evtx";
                                string logFilePath = Path.Combine(saveFolderPath, logFileName);
                                path_info += "[***********System EventLog] = " + logFilePath + ", ";
                                if (!method.ExecuteWevtutilCommand(logFilePath, "System"))
                                {
                                    fail_info += "[***********System EventLog] : Fail, ";
                                    log.Info("SaveLogFile - System EventLog : Fail ");
                                }
                                else
                                {
                                    success_info += "[***********System EventLog] : Success, ";
                                    log.Info("SaveLogFile - System EventLog : Success ");
                                }
                            }
                            catch (Exception ex)
                            {
                                fail_info += "[***********System EventLog] : Exception Fail, ";
                                log.Error($"SaveLogFile - System EventLog : {ex.Message}");
                            }

                            // Wayn add Dell registry record file
                            try
                            {
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
                                        fail_info += "[******************Dell Reg] : No Reg value, ";
                                        log.Info("SaveLogFile - No registry values found or path does not exist.");
                                    }
                                    else
                                    {
                                        success_info += "[******************Dell Reg] : Success, ";
                                        foreach (var kvp in regValues)
                                        {
                                            writer.WriteLine($"{kvp.Key} = {kvp.Value ?? "(null)"}");
                                        }
                                    }
                                }
                                log.Info("SaveLogFile - Dell registry values record done.");
                            }
                            catch (Exception ex)
                            {
                                fail_info += "[******************Dell Reg] : Exception Fail, ";
                                log.Error($"SaveLogFile - Dell Reg.log : {ex.Message}");
                            }
                            // Dell registry record end

                            // Wayn add Dell DDPM file name record
                            try
                            {
                                string ddpmFileFolder = Path.Combine(saveFolderPath, "DDPM_File");
                                Directory.CreateDirectory(ddpmFileFolder);
                                log.Info($"SaveLogFile - Created DDPM_File folder: {ddpmFileFolder}");

                                string rootPath = AppDomain.CurrentDomain.BaseDirectory;
                                //string rootPath = @"C:\Program Files\Dell\Dell Display and Peripheral Manager"; //For Test
                                string recordFile = Path.Combine(ddpmFileFolder, "FileName_Record.txt");

                                using (var writer = new StreamWriter(recordFile, false))
                                {
                                    void WriteEntries(string path, int level)
                                    {
                                        string indent = new string(' ', level * 4); // 階層空格
                                        writer.WriteLine($"{indent}[{path}]");
                                        log.Info($"SaveLogFile - DDPM_File {indent}[{path}]");
                                        string[] files;
                                        try
                                        {
                                            files = Directory.GetFiles(path);
                                            log.Info($"SaveLogFile - DDPM_File GetFiles path, {files}");
                                        }
                                        catch (Exception ex)
                                        {
                                            log.Error($"SaveLogFile - Failed to record files in '{path}': {ex.Message}");
                                            return;
                                        }
                                        foreach (var file in files)
                                        {
                                            var fileName = Path.GetFileName(file);
                                            string versionInfo;
                                            try
                                            {
                                                // 取檔案版本
                                                var vInfo = FileVersionInfo.GetVersionInfo(file);
                                                versionInfo = string.IsNullOrEmpty(vInfo.FileVersion) ? "None" : vInfo.FileVersion;
                                            }
                                            catch (Exception ex)
                                            {
                                                versionInfo = $"Exception: {ex.Message}";
                                            }
                                            writer.WriteLine($"{indent}    {fileName}    Version: {versionInfo}");
                                        }

                                        //foreach (var file in files) 
                                        //{
                                        //    // 只取檔名
                                        //    writer.WriteLine($"{indent}    {Path.GetFileName(file)}");
                                        //    log.Error($"SaveLogFile - DDPM_File : {indent}    {Path.GetFileName(file)}");
                                        //}

                                        // 遞迴子資料夾
                                        string[] dirs;
                                        try
                                        {
                                            dirs = Directory.GetDirectories(path);
                                        }
                                        catch (Exception ex)
                                        {
                                            log.Error($"SaveLogFile - Failed to record directories in '{path}': {ex.Message}");
                                            return;
                                        }
                                        foreach (var dir in dirs)
                                        {
                                            WriteEntries(dir, level + 1);
                                        }
                                    }

                                    // 遞迴
                                    if (Directory.Exists(rootPath))
                                    {
                                        WriteEntries(rootPath, 0);
                                        success_info += "[*****************DDPM_File] : Success, ";
                                        log.Info($"SaveLogFile - FileName_Record generated: {recordFile}");
                                    }
                                    else
                                    {
                                        writer.WriteLine($"Log path not found: {rootPath}");
                                        fail_info += "[*****************DDPM_File] : Log folder not found, ";
                                        log.Error($"SaveLogFile - DDPM log folder does not exist: {rootPath}");
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                fail_info += "[*****************DDPM_File] : Exception Fail, ";
                                log.Error($"SaveLogFile - DDPM_File record exception: {ex.Message}");
                            }
                            // Wayn add Dell DDPM file name end


                            // Install Shell and DCS Log
                            try
                            {
                                string registryKey = @"SOFTWARE\Dell\Dell Display and Peripheral Manager";
                                object o = DDPMRegistryHelper.ReadRegistryKey(RegistryHive.LocalMachine, registryKey, "LogPath");
                                if (o != null && o is string && !string.IsNullOrEmpty(o.ToString()))
                                {
                                    LogFolder = Convert.ToString(o);
                                    path_info += "[DDPMW-InstallShell_DCS Log] = " + LogFolder + ", ";
                                    log.Info("folderPath - LogFolder : " + LogFolder);
                                    if (method.DirectoryContainsFiles(LogFolder))
                                    {
                                        // 取得資料夾名稱
                                        string folderName = method.GetFolderName(LogFolder);
                                        string savePath = Path.Combine(saveFolderPath, folderName);
                                        log.Info($"folderPath - folderName : {folderName} and savePath : {savePath}, Line 645 ");
                                        // 複製指定的 log 文件到選擇的資料夾
                                        if (!method.CopyLogFolder(o.ToString(), savePath))
                                        {
                                            fail_info += "[DDPMW-InstallShell_DCS Log] : Some File Denied, ";
                                            log.Info("SaveLogFile - DDPMW-InstallShell_DCS Log : Some File Denied ");
                                            ret = false;
                                        }
                                        else
                                        {
                                            success_info += "[DDPMW-InstallShell_DCS Log] : Success, ";
                                            log.Info("SaveLogFile - DDPMW-InstallShell_DCS Log: Success ");
                                        }
                                    }
                                }
                                else
                                {
                                    fail_info += "[DDPMW-InstallShell_DCS Log] : No Log File, ";
                                    log.Info("SaveLogFile - DDPMW-InstallShell_DCS Log : No Log File ");
                                }
                            }
                            catch (Exception ex)
                            {
                                fail_info += "[DDPMW-InstallShell_DCS Log] : Exception Fail, ";
                                log.Error($"SaveLogFile - DDPMW-InstallShell_DCS Log : {ex.Message}");
                            }

                            // NKVM install.log
                            try
                            {
                                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                                // 組合相對路徑
                                string sourcePath = Path.Combine(baseDir, "Plugins", "NKVM");
                                string fileName = "install.log";
                                string sourceFile = Path.Combine(sourcePath, fileName);
                                path_info += "[**********NKVM/install.log] = " + sourceFile + ", ";
                                log.Info("SourceFile - SourceFile : " + sourceFile);
                                if (method.DirectoryContainsFiles(sourcePath))
                                {
                                    string savePath = Path.Combine(saveFolderPath, "NKVM_Log");
                                    // 複製指定的 log 文件到選擇的資料夾
                                    if (!method.CopyLogFolder(sourceFile, savePath))
                                    {
                                        fail_info += "[**********NKVM/install.log] : Some File Denied, ";
                                        log.Info("SaveLogFile - NKVM/install.log : Some File Denied ");
                                        ret = false;
                                    }
                                    else
                                    {
                                        success_info += "[**********NKVM/install.log] : Success, ";
                                        log.Info("SaveLogFile -NKVM/install.log : Success ");
                                    }
                                }
                                else
                                {
                                    fail_info += "[**********NKVM/install.log] : No Log File, ";
                                    log.Info("SaveLogFile - NKVM/install.log : No Log File ");
                                }
                            }
                            catch (Exception ex)
                            {
                                fail_info += "[**********NKVM/install.log] : Exception Fail, ";
                                log.Error($"SaveLogFile - NKVM/install.log : {ex.Message}");
                            }

                            // Write SaveLogFile log
                            string resultInfoFile = Path.Combine(saveFolderPath, "SaveLogInfo.txt");
                            try
                            {
                                using (var writer = new StreamWriter(resultInfoFile, false))
                                {
                                    // Success
                                    writer.WriteLine("======================================== Success Info ========================================");
                                    if (!string.IsNullOrEmpty(success_info))
                                    {
                                        // 逗號換行
                                        var successItems = success_info.Split(',', StringSplitOptions.RemoveEmptyEntries);
                                        foreach (string item in successItems)
                                        {
                                            writer.WriteLine(item.Trim());
                                            log.Info($"SaveLogFile - {item.Trim()}");
                                        }
                                    }
                                    else
                                    {
                                        writer.WriteLine("No success info.");
                                        log.Info($"SaveLogFile - No fail info.");
                                    }

                                    writer.WriteLine(); // 空一行

                                    // Fail
                                    writer.WriteLine("======================================== Fail Info ========================================");
                                    if (!string.IsNullOrEmpty(fail_info))
                                    {
                                        var failItems = fail_info.Split(',', StringSplitOptions.RemoveEmptyEntries);
                                        foreach (string item in failItems)
                                        {
                                            writer.WriteLine(item.Trim());
                                            log.Info($"SaveLogFile - {item.Trim()}");
                                        }
                                    }
                                    else
                                    {
                                        writer.WriteLine("No fail info.");
                                        log.Info($"SaveLogFile - No fail info.");
                                    }

                                    writer.WriteLine(); // 空一行

                                    // Path
                                    writer.WriteLine("======================================== Path Info ========================================");
                                    if (!string.IsNullOrEmpty(path_info))
                                    {
                                        var pathItems = path_info.Split(',', StringSplitOptions.RemoveEmptyEntries);
                                        foreach (string item in pathItems)
                                        {
                                            writer.WriteLine(item.Trim());
                                            log.Info($"SaveLogFile - {item.Trim()}");
                                        }
                                    }
                                    else
                                    {
                                        writer.WriteLine("No path info.");
                                        log.Info($"SaveLogFile - No path info.");
                                    }

                                    writer.WriteLine(); // 空一行

                                    // EXE
                                    var exeList = new List<(string ProcessName, string DisplayName)>
                                {
                                    ("DDPM",               "******************DDPM.exe"),
                                    ("DDPM.Subagent",      "*********DDPM.Subagent.exe"),
                                    ("DDPM.Subagent.User", "****DDPM.Subagent.User.exe"),
                                    ("Dell.TechHub",       "**********Dell.TechHub.exe"),
                                    ("DPMService",         "************DPMService.exe")
                                };

                                    writer.WriteLine("======================================== EXE Info ========================================");

                                    foreach (var (processName, displayName) in exeList)
                                    {
                                        var process = Process.GetProcessesByName(processName).FirstOrDefault();
                                        if (process == null)
                                        {
                                            writer.WriteLine($"[{displayName}] : Fail");
                                        }
                                        else
                                        {
                                            try
                                            {
                                                var versionInfo = process.MainModule.FileVersionInfo;
                                                writer.WriteLine($"[{displayName}] : True, Ver = {versionInfo.FileVersion}");
                                                log.Info($"SaveLogFile - [{displayName}] : True, Ver = {versionInfo.FileVersion}");
                                            }
                                            //Dean 20250411, try to solve checkmarx issue: "Information Exposure Through an Error Message"
                                            //catch (Exception ex)
                                            //{
                                            //    writer.WriteLine($"[{displayName}] : True, Ver = ({ex.Message})");
                                            //    log.Info($"SaveLogFile - [{displayName}] : True, Ver = ({ex.Message})");
                                            //}
                                            catch (UnauthorizedAccessException)
                                            {
                                                writer.WriteLine($"[{displayName}] : True, Ver = (Access denied)");
                                            }
                                            catch (Exception)
                                            {
                                                writer.WriteLine($"[{displayName}] : True, Ver = (log exception)");
                                            }
                                            //End fix
                                        }
                                    }

                                }
                            }
                            catch (Exception ex)
                            {
                                fail_info += "[***************SaveLogInfo] : " + resultInfoFile + " ; Exception Fail, ";
                                log.Error($"SaveLogFile - SaveLogInfo : {ex.Message}");
                            }

                            // zip
                            string zipFilePath = saveFolderPath + ".zip";
                            try
                            {
                                if (!method.CreateZipFile(saveFolderPath, zipFilePath))// 壓縮資料夾
                                {
                                    fail_info += "[***************Compression]";
                                    log.Info("SaveLogFile - CreateZipFile fail.");
                                }
                                else
                                {
                                    log.Info($"SaveLogFile - CreateZipFile : {zipFilePath}, succcess.");
                                }

                                if (DDPMFileSecurity.ValidateFilePath(saveFolderPath, out string info))
                                    Directory.Delete(saveFolderPath, true);
                                else
                                    log.Error($"[SaveLog] skip delete temp folder due to: {info}");
                            }
                            catch (Exception ex)
                            {
                                fail_info += "[***************Compression] : " + zipFilePath + "; Exception Fail, ";
                                log.Error($"SaveLogFile - Compression : {ex.Message}");
                            }

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
                        //if (method != null)
                        //{
                        //    method.Dispose();
                        //    method = null;
                        //}
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