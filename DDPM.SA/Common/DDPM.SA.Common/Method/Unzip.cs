using DDPM.SA.Common.Settings;
using Dell.Client.Framework.Common.Exceptions;
using Dell.Client.Framework.Security;
using Dell.Client.Framework.Security.Interfaces;
using System;
using System.Collections.Generic;
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
        /// <param name="exeFilePath">回傳解壓縮後資料夾中的exe檔案</param>
        /// <returns></returns>
        public bool ExecuteUnzip(string zipFilePath, string extractPath, out string exeFilePath)
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
                _logs?.DebugMsg_1(nameof(Unzip) + " start");

                using (FileLock fileLock = new FileLock(zipFilePath, PathCheckOption.None, lockNow: true))
                {
                    AclChecker aclChecker = new AclChecker();
                    if (aclChecker.ContainsUnprivilegedWriteAccess(fileLock))
                    {
                        throw new SecurityException($"File ACLs for {zipFilePath} contained unprivileged write access for one or more identity");
                    }    
                    ZipFile.ExtractToDirectory(zipFilePath, extractPath, true);
                    _logs?.DebugMsg_1(nameof(Unzip) + " done");
                    exeFilePath = GetExeFilePath(extractPath);
                    //Dean 1223 check output
                    if (string.IsNullOrEmpty(exeFilePath) || exeFilePath.Length == 0)
                    {
                        _logs?.DebugMsg_1(nameof(Unzip) + "Unzip: search exe file got null/empty return");
                        return false;
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                _logs?.DebugMsg_1(nameof(Unzip) + "Unzip fail: " + ex.Message);
                exeFilePath = "";
                return false;
            }
        }        

        private static List<string> SearchExeFileFromDirectory(DirectoryInfo directoryInfo, Logs _logs = null, bool recursive = false)
        {
            List<string> output = new List<string>();
            // Get all files in the directory
            FileInfo[] files = directoryInfo.GetFiles();
            _logs?.DebugMsg_1(nameof(SearchExeFileFromDirectory) + $": Folder [{directoryInfo.Name}]");
            foreach (FileInfo file in files) 
            { 
                if (file.Extension.Equals(".exe", StringComparison.OrdinalIgnoreCase)) 
                { 
                    string fileTemp = DDPMFileSecurity.SanitizePath(file.FullName, out string info); 
                    if(string.IsNullOrEmpty(fileTemp))
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

        private string GetExeFilePath(string directory)
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
                    List<string> files = SearchExeFileFromDirectory(new DirectoryInfo(directory), _logs);
                    if(files != null && files.Count > 0)
                    {
                        exeFiles = files.ToArray();
                        _logs?.DebugMsg_1(nameof(GetExeFilePath) + $": Got {exeFiles.Length} executable file(s)");
                    }
                }
            }
            catch  (Exception ex)
            { 
                _logs?.DebugMsg_1(nameof(GetExeFilePath) + "Get exe files in folder fail: " + ex.Message);
            }

            if (exeFiles == null || exeFiles.Length == 0)
            {
                return string.Empty;
            }
            
            //Add white list comparison, Dean 1227
            if(exeFiles.Length > 1)
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
                    "FWUpdateTool"
                };
                                
                string[] comparedList = exeFiles.Where( x => whitelist.Any(y => x.Contains(y, StringComparison.OrdinalIgnoreCase))).ToArray();
                if (comparedList != null && comparedList.Length > 0)
                {
                    _logs?.DebugMsg_1(nameof(GetExeFilePath) + $": Matching {comparedList.Length} executable file(s), return first one");
                    return comparedList[0];
                }
            }
            //no matched exe, return first one directly
            return exeFiles[0];
        }
    }
}