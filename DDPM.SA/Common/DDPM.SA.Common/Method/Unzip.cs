using DDPM.SA.Common.Settings;
using Dell.Client.Framework.Common.Exceptions;
using Dell.Client.Framework.Security;
using Dell.Client.Framework.Security.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security;
using VcpCore.Common;

namespace DDPM.SA.Common.Method
{
    public class Unzip
    {
        private Logs? _logs;

        public Unzip(Logs logs)
        {
            _logs = logs;
        }
        public bool CheckFileIsZip(string filePath)
        {
            string extension = Path.GetExtension(filePath).ToLower();
            bool isNeedUnzip = true;
            if (extension == ".exe")
            {
                isNeedUnzip = false;
            }
            else if (extension == ".zip")
            {
                isNeedUnzip = true;
            }
            return isNeedUnzip;
        }
        /// <summary>
        /// 解壓縮
        /// </summary>
        /// <param name="zipFilePath">壓縮檔路徑</param>
        /// <param name="extractPath">解壓縮資料夾路徑</param>
        /// <param name="recursive">是否搜尋子資料夾中的內容</param>
        /// <param name="exeFilePath">回傳解壓縮後資料夾中的exe檔案</param>
        /// <returns></returns>
        public bool ExecuteUnzip(string zipFilePath, string extractPath, bool recursive, out string exeFilePath)
        {
            //Elsa Add Security
            string FileInfo;
            if (!DDPMFileSecurity.IsFilePathValid(zipFilePath, out FileInfo))
            {
                _logs?.Info($"{nameof(ExecuteUnzip)} {FileInfo}");
                exeFilePath = "";
                return false;
            }

            try
            {
                _logs?.DebugMsg_1(nameof(ExecuteUnzip) + " start");

                using (FileLock fileLock = new FileLock(zipFilePath, PathCheckOption.None, lockNow: true))
                {
                    AclChecker aclChecker = new AclChecker();
                    if (aclChecker.ContainsUnprivilegedWriteAccess(fileLock))
                    {
                        throw new SecurityException($"File ACLs for {zipFilePath} contained unprivileged write access for one or more identity");
                    }
                    ZipFile.ExtractToDirectory(zipFilePath, extractPath, true);
                    _logs?.DebugMsg_1(nameof(ExecuteUnzip) + " done");
                    exeFilePath = GetExeFilePath(extractPath, recursive);
                    //Dean 1223 check output
                    if (string.IsNullOrEmpty(exeFilePath) || exeFilePath.Length == 0)
                    {
                        _logs?.DebugMsg_1(nameof(ExecuteUnzip) + ": search exe file got null/empty return");
                        return false;
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                _logs?.DebugMsg_1(nameof(ExecuteUnzip) + " fail: " + ex.Message);
                exeFilePath = "";
                return false;
            }
        }
        /// <summary>
        /// 解壓縮InApp檔案
        /// </summary>
        /// <param name="zipFilePath">InApp檔路徑</param>
        /// <param name="upgFilePath">回傳解壓縮後資料夾中的upg檔案</param>
        /// <returns></returns>
        public bool ExecuteUnzipForInAppUpdate(string zipFilePath, out string upgFilePath)
        {
            //Elsa Add Security
            string FileInfo;
            if (!DDPMFileSecurity.IsFilePathValid(zipFilePath, out FileInfo))
            {
                _logs?.Info($"{nameof(ExecuteUnzipForInAppUpdate)} {FileInfo}");
                upgFilePath = "";
                return false;
            }

            try
            {
                _logs?.DebugMsg_1(nameof(ExecuteUnzipForInAppUpdate) + " start");

                using (FileLock fileLock = new FileLock(zipFilePath, PathCheckOption.None, lockNow: true))
                {
                    AclChecker aclChecker = new AclChecker();
                    if (aclChecker.ContainsUnprivilegedWriteAccess(fileLock))
                    {
                        throw new SecurityException($"File ACLs for {zipFilePath} contained unprivileged write access for one or more identity");
                    }
                    string workingDirectory = DDPMFileSecurity.SanitizePath(Path.GetDirectoryName(zipFilePath), out string info);
                    _logs?.DebugMsg_1($"{nameof(ExecuteUnzipForInAppUpdate)} DDPMFileSecurity.SanitizePath info : {info}");
                    Process process = new Process();
                    process.StartInfo.FileName = zipFilePath; // 設置要執行的 .exe 檔案
                    process.StartInfo.Arguments = "-s -ext"; // 傳入的命令行參數
                    process.StartInfo.WorkingDirectory = workingDirectory;
                    process.StartInfo.UseShellExecute = false; // 禁用 Shell，啟用更可控的進程
                    process.StartInfo.CreateNoWindow = true; // 不顯示命令提示字元視窗
                    process.StartInfo.RedirectStandardOutput = true; // 重定向輸出，便於讀取
                    process.StartInfo.RedirectStandardError = true; // 重定向錯誤輸出
                    _logs?.DebugMsg_1($"{nameof(ExecuteUnzipForInAppUpdate)} process Start");
                    process.Start(); // 啟動進程
                    process.WaitForExit(); // 等待進程結束

                    // 確認退出代碼（0 表示成功）
                    if (process.ExitCode == 0)
                    {
                        _logs?.DebugMsg_1($"{nameof(ExecuteUnzipForInAppUpdate)} process done");
                    }
                    else
                    {
                        _logs?.DebugMsg_1($"{nameof(ExecuteUnzipForInAppUpdate)} process fail process.ExitCode : {process.ExitCode}");
                    }
                    _logs?.DebugMsg_1(nameof(ExecuteUnzipForInAppUpdate) + " done");
                    upgFilePath = GetUpgFilePath(workingDirectory, true);
                    //Dean 1223 check output
                    if (string.IsNullOrEmpty(upgFilePath) || upgFilePath.Length == 0)
                    {
                        _logs?.DebugMsg_1(nameof(ExecuteUnzipForInAppUpdate) + ": search upg file got null/empty return");
                        return false;
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                _logs?.DebugMsg_1(nameof(ExecuteUnzipForInAppUpdate) + " fail: " + ex.Message);
                upgFilePath = "";
                return false;
            }
        }

        private static List<string> SearchExeFileFromDirectory(DirectoryInfo directoryInfo, Logs _logs, bool recursive)
        {
            List<string> output = new List<string>();
            // Get all files in the directory
            FileInfo[] files = directoryInfo.GetFiles();
            _logs?.DebugMsg_1(nameof(SearchExeFileFromDirectory) + $": Folder [{directoryInfo.Name}]");
            foreach (FileInfo file in files)
            {
                if (file.Extension.Equals(".exe", StringComparison.OrdinalIgnoreCase))
                {
                    if (recursive)
                    {
                        recursive = false;
                    }
                    string fileTemp = DDPMFileSecurity.SanitizePath(file.FullName, out string info);
                    if (string.IsNullOrEmpty(fileTemp))
                    {
                        _logs?.DebugMsg_1(nameof(SearchExeFileFromDirectory) + $": Abnormal File is {file.Name}");
                    }
                    else
                    {
                        FileInfo fi = new FileInfo(fileTemp);
                        _logs?.DebugMsg_1(nameof(SearchExeFileFromDirectory) + $": Add file to list - {fi.Name}");
                        output.Add(fileTemp);
                    }
                }
            }
            if (recursive)
            {
                // Recursively get all matched files in the subdirectories
                DirectoryInfo[] subdirectories = directoryInfo.GetDirectories();
                foreach (DirectoryInfo subdirectory in subdirectories)
                {
                    List<string> temp = SearchExeFileFromDirectory(subdirectory, _logs, recursive);
                    if (temp != null && temp.Count > 0)
                    {
                        output.AddRange(temp);
                    }
                }
            }
            return output;
        }

        private string GetExeFilePath(string directory, bool recursive)
        {
            // 列舉資料夾中的所有 .exe 檔案
            string[] exeFiles = default;

            try
            {
                if (Directory.Exists(directory))
                {
                    //[Original]
                    //string absolutePath = Path.GetFullPath(directory);
                    //if (!string.IsNullOrEmpty(absolutePath))
                    //    exeFiles = Directory.GetFiles(absolutePath, "*.exe");

                    //for checkmarx test, [code part1]
                    //var options = new EnumerationOptions { RecurseSubdirectories = false, IgnoreInaccessible = true };
                    //exeFiles = Directory.GetFiles(directory, "*.exe", options);
                    //if(exeFiles.Length > 0)
                    //    _logs?.DebugMsg_1(nameof(Unzip) + " [part1] Get exe files in folder success");

                    //for checkmarx test, [code part2]
                    //foreach (var file in Directory.EnumerateFiles(directory, "*.exe"))
                    //{
                    //    if (Path.GetExtension(file).Equals(".exe", StringComparison.OrdinalIgnoreCase))
                    //    {
                    //        _logs?.DebugMsg_1(nameof(Unzip) + " [part2] Get exe files in folder success");
                    //        return file;
                    //    }
                    //}

                    //for checkmarx test, [code part3]
                    //string info = string.Empty;
                    //if (DDPMFileSecurity.ValidateFilePath(directory, out info))
                    //{
                    //    exeFiles = Directory.GetFiles(directory, "*.exe");
                    //}
                    //else
                    //{
                    //    _logs?.DebugMsg_1(nameof(Unzip) + "Get exe files in folder failed due to [ValidateFilePath] return false");
                    //    _logs?.DebugMsg_1(nameof(Unzip) + $"Detail: {info}");
                    //}

                    //for checkmarx check [code part4]
                    List<string> files = SearchExeFileFromDirectory(new DirectoryInfo(directory), _logs, recursive);
                    if (files != null && files.Count > 0)
                    {
                        exeFiles = files.ToArray();
                        _logs?.DebugMsg_1(nameof(GetExeFilePath) + $": Got {exeFiles.Length} executable file(s)");
                    }
                }
            }
            catch (Exception ex)
            {
                _logs?.DebugMsg_1(nameof(GetExeFilePath) + "Get exe files in folder fail: " + ex.Message);
            }

            if (exeFiles == null || exeFiles.Length == 0)
            {
                return string.Empty;
            }

            //Add white list comparison, Dean 1227
            if (exeFiles.Length > 1)
            {
                //white list
                List<string> whitelist = new List<string> {
                    "DellOTA",
                    "FWUpdater",
                    "OTSTestCilent",
                    "Burn.exe",
                    "CameraUpdate",
                    "OTATestServer",
                    "PriFWUpdate",
                    "BLE_RF_OTA",
                    "FWUpdateTool",
                    "DdpmSwUpdater",
                };

                string[] comparedList = exeFiles.Where(x => whitelist.Any(y => x.Contains(y, StringComparison.OrdinalIgnoreCase))).ToArray();
                if (comparedList != null && comparedList.Length > 0)
                {
                    _logs?.DebugMsg_1(nameof(GetExeFilePath) + $": Matching {comparedList.Length} executable file(s), return first one");
                    return comparedList[0];
                }
            }
            //no matched exe, return first one directly
            return exeFiles[0];
        }
        private static List<string> SearchUpgFileFromDirectory(DirectoryInfo directoryInfo, Logs _logs, bool recursive)
        {
            List<string> output = new List<string>();
            // Get all files in the directory
            FileInfo[] files = directoryInfo.GetFiles();
            _logs?.DebugMsg_1(nameof(SearchUpgFileFromDirectory) + $": Folder [{directoryInfo.Name}]");
            foreach (FileInfo file in files)
            {
                if (file.Extension.Equals(".upg", StringComparison.OrdinalIgnoreCase))
                {
                    if (recursive)
                    {
                        recursive = false;
                    }
                    string fileTemp = DDPMFileSecurity.SanitizePath(file.FullName, out string info);
                    if (string.IsNullOrEmpty(fileTemp))
                    {
                        _logs?.DebugMsg_1(nameof(SearchUpgFileFromDirectory) + $": Abnormal File is {file.Name}");
                    }
                    else
                    {
                        FileInfo fi = new FileInfo(fileTemp);
                        _logs?.DebugMsg_1(nameof(SearchUpgFileFromDirectory) + $": Add file to list - {fi.Name}");
                        output.Add(fileTemp);
                    }
                }
            }
            if (recursive)
            {
                // Recursively get all matched files in the subdirectories
                DirectoryInfo[] subdirectories = directoryInfo.GetDirectories();
                foreach (DirectoryInfo subdirectory in subdirectories)
                {
                    List<string> temp = SearchUpgFileFromDirectory(subdirectory, _logs, recursive);
                    if (temp != null && temp.Count > 0)
                    {
                        output.AddRange(temp);
                    }
                }
            }
            return output;
        }

        private string GetUpgFilePath(string directory, bool recursive)
        {
            // 列舉資料夾中的所有 .exe 檔案
            string[] upgFiles = default;

            try
            {
                if (Directory.Exists(directory))
                {
                    //for checkmarx check [code part4]
                    List<string> files = SearchUpgFileFromDirectory(new DirectoryInfo(directory), _logs, recursive);
                    if (files != null && files.Count > 0)
                    {
                        upgFiles = files.ToArray();
                        _logs?.DebugMsg_1(nameof(GetUpgFilePath) + $": Got {upgFiles.Length} upg file(s)");
                    }
                }
            }
            catch (Exception ex)
            {
                _logs?.DebugMsg_1(nameof(GetUpgFilePath) + "Get upg files in folder fail: " + ex.Message);
            }

            if (upgFiles == null || upgFiles.Length == 0)
            {
                return string.Empty;
            }

            //Add white list comparison, Dean 1227
            if (upgFiles.Length > 1)
            {
                //white list
                List<string> whitelist = new List<string> {
                    "DELL",
                };

                string[] comparedList = upgFiles.Where(x => whitelist.Any(y => x.Contains(y, StringComparison.OrdinalIgnoreCase))).ToArray();
                if (comparedList != null && comparedList.Length > 0)
                {
                    _logs?.DebugMsg_1(nameof(GetUpgFilePath) + $": Matching {comparedList.Length} upg file(s), return first one");
                    return comparedList[0];
                }
            }
            //no matched exe, return first one directly
            return upgFiles[0];
        }
    }
}