using DDPM.SA.Common.Settings;
using Dell.Client.Framework.Security;
using Dell.Client.Framework.Security.Interfaces;
using System;
using System.IO;
using System.IO.Compression;
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

        private string GetExeFilePath(string directory)
        {
            // 列舉資料夾中的所有 .exe 檔案
            string[] exeFiles = default;

            try
            {
                exeFiles = Directory.GetFiles(directory, "*.exe");
            }
            catch  (Exception ex)
            { 
                _logs?.DebugMsg_1(nameof(Unzip) + "Get exe files in folder fail: " + ex.Message);
            }

            // 如果存在 .exe 檔案，則返回第一個 .exe 檔案的路徑
            if (exeFiles.Length > 0)
            {
                return exeFiles[0];
            }
            else
            {
                return "";
            }
        }
    }
}