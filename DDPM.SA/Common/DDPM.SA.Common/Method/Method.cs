using Dell.Client.Framework.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Compression;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VcpCore.Common;
using DDPM.SA.Common.Settings;
using System.Runtime.InteropServices;

namespace DDPM.SA.Common.Method
{
    public class Method : IDisposable
    {
        ILog _Log;
        Logs _Logs;
        public Method(ILog log)
        {
            _Log = log;
        }
        public Method(Logs log)
        {
            _Logs = log;
        }
        public Method()
        {
            
        }
        public void Dispose()
        {

        }
        public bool CreateZipFile(string folderPath, string zipFilePath)
        {
            bool result = false;
            WriteLog($"{nameof(CreateZipFile)} start");
            try
            {
                if (File.Exists(zipFilePath))
                {
                    File.Delete(zipFilePath);
                }
                ZipFile.CreateFromDirectory(folderPath, zipFilePath, CompressionLevel.Fastest, includeBaseDirectory: true);
                result = true;
            }
            catch (Exception ex)
            {
                WriteLog($"{nameof(CreateZipFile)} Exception occurred while creating ZIP file: {ex.Message}");
            }
            WriteLog($"{nameof(CreateZipFile)} end");
            return result;
        }
        /// <summary>
        /// Get system events
        /// </summary>
        /// <param name="exportFilePath">save path</param>
        /// <param name="logName">"Application", "System", "Security"</param>
        /// <returns></returns>
        public bool ExecuteWevtutilCommand(string exportFilePath, string logName)
        {
            bool result = false;
            try
            {
                // 設定要查詢的日誌名稱

                // 獲取當前時間
                DateTime now = DateTime.UtcNow;

                // 設定開始和結束時間範圍（UTC）
                DateTime endTime = now;
                DateTime startTime = endTime.AddDays(-1);

                // 生成查詢語句
                string query = $"*[System[TimeCreated[@SystemTime>='{startTime:yyyy-MM-ddTHH:mm:ss.fffZ}' and @SystemTime<='{endTime:yyyy-MM-ddTHH:mm:ss.fffZ}']]]";
                // 建立我們要執行的命令
                string command = $"epl {logName} \"{exportFilePath}\" /ow:true /q:\"{query}\"";
                // 設定 ProcessStartInfo
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = "wevtutil",
                    Arguments = command,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                // 開啟進程
                using (Process process = Process.Start(startInfo))
                {
                    // 讀取標準輸出和錯誤輸出
                    string output = process.StandardOutput.ReadToEnd();
                    string error = process.StandardError.ReadToEnd();

                    // 等待進程結束
                    process.WaitForExit();

                    // 輸出結果
                    if (process.ExitCode == 0)
                    {
                        WriteLog("Events have been exported successfully.");
                    }
                    else
                    {
                        WriteLog($"Error exporting events: {error}");
                    }
                }
                result = true;
            }
            catch (Exception ex)
            {
                WriteLog($"Exception occurred: {ex.Message}");
            }
            return result;
        }
        public string GetFolderName(string path)
        {
            try
            {
                string folderName = System.IO.Path.GetFileName(path.TrimEnd(System.IO.Path.DirectorySeparatorChar));
                return folderName;
            }
            catch (Exception ex)
            {
                WriteLog($"Exception occurred: {ex.Message}");
                return null;
            }
        }
        public bool DirectoryContainsFiles(string folderPath)
        {
            bool ret = false;
            try
            {
                if (Directory.Exists(folderPath))
                {
                    // 檢查資料夾是否包含檔案
                    string[] files = Directory.GetFiles(folderPath);
                    // 檢查資料夾是否包含子資料夾
                    string[] directories = Directory.GetDirectories(folderPath);

                    // 如果檔案或子資料夾數量大於0，則返回 true
                    ret = files.Length > 0 || directories.Length > 0;
                }
            }
            catch(Exception e)
            {
                WriteLog($"[DirectoryContainsFiles] exception: {e.Message}");
            }
            return ret;
        }
        public bool CopyLogFolder(string sourceFolder, string destinationFolder)
        {
            bool result = false;
            try
            {
                if (Directory.Exists(sourceFolder))
                {
                    // 複製資料夾及其內容
                    if (!DirectoryCopy(sourceFolder, destinationFolder, true))
                    {
                        WriteLog("[DirectoryCopy] got some files copy failed");
                    }
                    else
                        result = true;
                    WriteLog("Log folder copy action finish.");
                }
                else
                {
                    WriteLog("Source folder does not exist.");
                }
            }
            catch (Exception ex)
            {
                WriteLog($"Exception occurred while copying log folder: {ex.Message}");
            }
            return result;
        }
        public bool DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            bool all_pass = true;
            // 確保目標資料夾存在
            try
            {
                Directory.CreateDirectory(destDirName);
            }
            catch (Exception ex)
            {
                WriteLog($"[DirectoryCopy] Failed to create destination directory {destDirName} : {ex.Message}");
                all_pass = false;
            }
            // 複製檔案，列出目錄下的所有檔案
            string[] files = null;
            try
            {
                files = Directory.GetFiles(sourceDirName);
            }
            catch (Exception ex)
            {
                WriteLog($"[DirectoryCopy] Failed to list files in directory {sourceDirName} : {ex.Message}");
                all_pass = false;
            }
            if (files != null)
            {
                foreach (var file in files)
                {
                    var destFile = Path.Combine(destDirName, Path.GetFileName(file));
                    try
                    {
                        File.Copy(file, destFile, true);
                    }
                    catch (Exception ex)
                    {
                        WriteLog($"[DirectoryCopy] Failed to copy file {file} to {destFile} : {ex.Message}");
                        all_pass = false;
                    }
                }
            }
            // 遞迴複製子資料夾
            if (copySubDirs)
            {
                string[] subDirs = null;
                try
                {
                    subDirs = Directory.GetDirectories(sourceDirName);
                }
                catch (Exception ex)
                {
                    WriteLog($"[DirectoryCopy] Failed to list sub-directories in directory {sourceDirName} : {ex.Message}");
                    all_pass = false;
                }
                if (subDirs != null)
                {
                    foreach (var subDir in subDirs)
                    {
                        var destSubDir = Path.Combine(destDirName, Path.GetFileName(subDir));
                        // 即使某個子資料夾複製失敗，也繼續處理其他子資料夾
                        if (!DirectoryCopy(subDir, destSubDir, copySubDirs))
                        {
                            all_pass = false;
                        }
                    }
                }
            }
            return all_pass;
        }
        //public bool DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        //{
        //    bool all_pass = true;
        //    // 確保目標資料夾存在
        //    Directory.CreateDirectory(destDirName);
        //    // 複製檔案
        //    try
        //    {
        //        foreach (string file in Directory.GetFiles(sourceDirName))
        //        {
        //            string destFile = Path.Combine(destDirName, Path.GetFileName(file));
        //            File.Copy(file, destFile, true);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        WriteLog($"[DirectoryCopy] Get files in folder failed, message: {ex.Message}");
        //        all_pass = false;
        //    }

        //    // 複製子資料夾
        //    if (copySubDirs)
        //    {
        //        foreach (string subDir in Directory.GetDirectories(sourceDirName))
        //        {
        //            string destSubDir = Path.Combine(destDirName, Path.GetFileName(subDir));
        //            if (!DirectoryCopy(subDir, destSubDir, true))
        //                all_pass = false;
        //        }
        //    }
        //    return all_pass;
        //}
        public bool DeleteFolder(string folderPath)
        {
            WriteLog($"{nameof(DeleteFolder)} start");
            bool result = false;
            try
            {
                if (DDPMFileSecurity.ValidateFilePath(folderPath, out string folderInfo))
                {
                    // 檢查資料夾是否存在
                    if (!string.IsNullOrEmpty(folderPath) && Directory.Exists(folderPath))
                    {
                        WriteLog($"{nameof(DeleteFolder)} Directory.Delete go");
                        // 刪除資料夾及其所有內容
                        Directory.Delete(folderPath, true);
                        WriteLog($"{nameof(DeleteFolder)} Directory.Delete done");
                    }
                    else
                    {
                        WriteLog($"{nameof(DeleteFolder)} folderPath Is Null : {string.IsNullOrEmpty(folderPath)}");
                        WriteLog($"{nameof(DeleteFolder)} folderPath is Exists : {Directory.Exists(folderPath)}");
                    }
                    result = true;
                }
                else
                {
                    WriteLog($"{nameof(DeleteFolder)} ValidateFilePath fail : {folderInfo}");
                }
            }
            catch (Exception ex)
            {
                WriteLog($"{nameof(DeleteFolder)} error : {ex.Message}");
                result = false;
            }
            WriteLog($"{nameof(DeleteFolder)} finish");
            return result;
        }
        public string GetSystemArchitecture()
        {
            WriteLog($"[DisplayMangerPlugin] {nameof(GetSystemArchitecture)} RuntimeInformation.ProcessArchitecture : {RuntimeInformation.OSArchitecture.ToString()}");
            if (RuntimeInformation.OSArchitecture == Architecture.X64)
            {
                return "Intel";//"Intel_x64";
            }
            else if (RuntimeInformation.OSArchitecture == Architecture.X86)
            {
                return "Intel";//"Intel_x86";
            }
            else if (RuntimeInformation.OSArchitecture == Architecture.Arm)
            {
                return "ARM";
            }
            else if (RuntimeInformation.OSArchitecture == Architecture.Arm64)
            {
                return "ARM";//"ARM_64";
            }
            else
            {
                return "Unknow";
            }
        }
        void WriteLog(string mes)
        {
            _Log?.Info(mes);
            _Logs?.DebugMsg_1(mes);
        }
    }
}
