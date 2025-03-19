using DDPM.SA.Common.Security;
using DDPM.SA.Common.Settings;
using Microsoft.Win32;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using VcpCore.Common;

namespace DDPM.SA.Common.Method
{
    public class Download
    {
        private Logs? _logs;
        public long? DownloadFileSize = null;
        public FileStream? DownloadFileStream = null;

        public Download(Logs logs)
        {
            _logs = logs;
        }

        public static string GetTestServerURL()
        {
            string ret = GlobalDefinitions.major_url; // this is default value (production server)
            try
            {
                using (RegistryKey localKey64 = RegistryKey.OpenBaseKey(Microsoft.Win32.RegistryHive.LocalMachine, RegistryView.Registry64))
                {
                    if (localKey64 != null)
                    {
                        using (RegistryKey registryKey = localKey64.OpenSubKey("SOFTWARE\\Dell\\DDPM Subagent\\", false))
                        {
                            if (registryKey != null)
                            {
                                var obj = registryKey.GetValue("TestServerURL");
                                if (obj != null)
                                {
                                    string s = obj.ToString();
                                    if (!string.IsNullOrEmpty(s))
                                    {
                                        ret = s;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log or handle the exception as needed
#if DEBUG
                Console.WriteLine($"Error accessing registry: {ex.Message}");
#endif
            }
            return ret;
        }

        public bool DownloadFile(string URLPath, string SavePath, out string FailInfo, bool isSkipCA = false, CancellationTokenSource cts = null)
        {
            try
            {
                _logs?.DebugMsg_1(nameof(DownloadFile) + " start");
                CertificateCheck caCheck = new CertificateCheck(_logs);
                {
                    if (!isSkipCA)
                    {
                        if (!caCheck.CheckURLCACertificate(URLPath))//0815 Bruce Add Security
                        {
                            FailInfo = "CA check fail";
                            _logs?.DebugMsg_1(FailInfo);
                            return false;
                        }
                    }
                    else
                    {
                        FailInfo = "CA check skip";
                        _logs?.DebugMsg_1(FailInfo);
                    }
                    string url = URLPath;
                    string savePath = SavePath;
                    if (cts == null)
                    {
                        cts = new CancellationTokenSource();
                    }
                    HttpClient client = new HttpClient();
                    // 設定逾時
                    client.Timeout = TimeSpan.FromSeconds(10);
                    // 發送 HTTP GET 請求到指定的 URL
                    HttpResponseMessage response = client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cts.Token).Result;
                    // 從 URL 中取得回應標頭
                    var header = client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cts.Token).Result;
                    // 從回應標頭中提取檔案大小
                    DownloadFileSize = header.Content.Headers.ContentLength;
                    // 設定逾時
                    cts.CancelAfter(TimeSpan.FromSeconds(10));
                    // 取得包含 URL 內容的串流
                    var stream = client.GetStreamAsync(url, cts.Token).Result;
                    // 建立檔案串流以將下載的內容寫入
                    DownloadFileStream = File.Create(savePath);
                    // 將串流的內容複製到檔案中
                    stream.CopyToAsync(DownloadFileStream, cts.Token).Wait();
                }
                _logs?.DebugMsg_1(nameof(DownloadFile) + " done");
                FailInfo = "Pass";
                return true;
            }
            catch (TaskCanceledException ex)
            {
                FailInfo = $"DownloadFile timeout";
                _logs?.DebugMsg_1(nameof(DownloadFile) + " timeout : " + ex.Message);
                return false;
            }
            catch (OperationCanceledException ex)
            {
                FailInfo = $"DownloadFile Cancel";
                _logs?.DebugMsg_1(nameof(DownloadFile) + " cancel : " + ex.Message);
                return false;
            }
            catch (Exception ex)
            {
                FailInfo = $"DownloadFile fail : {ex.Message}";
                _logs?.DebugMsg_1(nameof(DownloadFile) + " fail:" + ex.Message);
                return false;
            }
            finally
            {
                if (DownloadFileStream != null)
                    DownloadFileStream.Close();
                else
                    _logs?.DebugMsg_1("[DownloadFile] [Finally] DownloadFileStream is null.");
                DownloadFileSize = null;
                DownloadFileStream = null;
            }
        }
        public bool DownloadFile_OnLocal(string URLPath, string SavePath, out string FailInfo)
        {
            try
            {
                _logs?.DebugMsg_1(nameof(DownloadFile_OnLocal) + " start");
                using (FileStream sourceStream = new FileStream(URLPath, FileMode.Open, FileAccess.Read))
                {
                    DownloadFileSize = sourceStream.Length;
                    DownloadFileStream = new FileStream(SavePath, FileMode.Create, FileAccess.Write);
                }
                FailInfo = "Pass";
                return true;
            }
            catch (Exception ex)
            {
                FailInfo = "Network fail";
                _logs?.DebugMsg_1(nameof(DownloadFile_OnLocal) + " fail:" + ex.ToString());
                DownloadFileStream.Close();
                return false;
            }
        }
        public double GetProgress()
        {
            double progress = 0;
            if (DownloadFileStream != null)
            {
                if (DownloadFileSize == null)
                {
                    DownloadFileSize = 1;
                }
                progress = Math.Round(((double)DownloadFileStream.Length / (double)DownloadFileSize) * 100.0, 2);
            }
            return progress;
        }
    }
}