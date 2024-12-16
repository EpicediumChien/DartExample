using Dell.Client.Framework.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Compression;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDPM.SA.Common.Method
{
    public class Method : IDisposable
    {
        ILog _Log;
        public Method(ILog log)
        {
            _Log = log;
        }
        public void Dispose()
        {

        }
        public bool CreateZipFile(string folderPath, string zipFilePath)
        {
            bool result = false;
            _Log?.Info($"{nameof(CreateZipFile)} start");
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
                _Log?.Info($"{nameof(CreateZipFile)} Exception occurred while creating ZIP file: {ex.Message}");
            }
            _Log?.Info($"{nameof(CreateZipFile)} end");
            return result;
        }
        public bool ExecuteWevtutilCommand(string exportFilePath)
        {
            bool result = false;
            try
            {
                // 設定要查詢的日誌名稱
                string logName = "Application"; // 可選擇 "Application", "System", "Security"

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
                        Console.WriteLine("Events have been exported successfully.");
                    }
                    else
                    {
                        Console.WriteLine($"Error exporting events: {error}");
                    }
                }
                result = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception occurred: {ex.Message}");
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
                Console.WriteLine($"Exception occurred: {ex.Message}");
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
            catch
            {
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
                        _Log?.Info("[DirectoryCopy] got some files copy failed");
                    }
                    else
                        result = true;
                    _Log?.Info("Log folder copy action finish.");
                }
                else
                {
                    _Log?.Info("Source folder does not exist.");
                }
            }
            catch (Exception ex)
            {
                _Log?.Info($"Exception occurred while copying log folder: {ex.Message}");
            }
            return result;
        }
        public bool DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            bool all_pass = true;
            // 確保目標資料夾存在
            Directory.CreateDirectory(destDirName);
            // 複製檔案
            try
            {
                foreach (string file in Directory.GetFiles(sourceDirName))
                {
                    string destFile = Path.Combine(destDirName, Path.GetFileName(file));
                    File.Copy(file, destFile, true);
                }
            }
            catch (Exception ex)
            {
                _Log?.Info($"[DirectoryCopy] Get files in folder failed, message: {ex.Message}");
                all_pass = false;
            }

            // 複製子資料夾
            if (copySubDirs)
            {
                foreach (string subDir in Directory.GetDirectories(sourceDirName))
                {
                    string destSubDir = Path.Combine(destDirName, Path.GetFileName(subDir));
                    if (!DirectoryCopy(subDir, destSubDir, true))
                        all_pass = false;
                }
            }
            return all_pass;
        }
    }
}
