using DDPM.SA.Common.Security;
using System;
using System.IO;
using System.Net.Http;
using VcpCore.Common;

namespace DDPM.SA.Common.Method
{
    public class Download
    {
        private Logs _logs;
        public long? DownloadFileSize = null;
        public FileStream? DownloadFileStream = null;

        public Download(Logs logs)
        {
            _logs = logs;
        }

        public bool DownloadFile(string URLPath, string SavePath, out string FailInfo)
        {
            try
            {
                _logs.DebugMsg_1(nameof(DownloadFile) + " start");
                CertificateCheck caCheck = new CertificateCheck();
                {
                    if (!caCheck.CheckURLCACertificate(URLPath))//0815 Bruce Add Security
                    {
                        FailInfo = "CA check fail";
                        _logs.DebugMsg_1(FailInfo);
                        return false;
                    }
                    string url = URLPath;
                    string savePath = SavePath;
                    HttpClient client = new HttpClient();
                    client.Timeout = TimeSpan.FromMinutes(1);
                    // 發送 HTTP GET 請求到指定的 URL
                    HttpResponseMessage response = client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead).Result;
                    // 從 URL 中取得回應標頭
                    var header = client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead).Result;
                    // 從回應標頭中提取檔案大小
                    DownloadFileSize = header.Content.Headers.ContentLength;
                    // 取得包含 URL 內容的串流
                    var stream = client.GetStreamAsync(url).Result;
                    // 建立檔案串流以將下載的內容寫入
                    DownloadFileStream = File.Create(savePath);
                    // 將串流的內容複製到檔案中
                    stream.CopyToAsync(DownloadFileStream).Wait();
                    DownloadFileStream.Close();
                    DownloadFileSize = null;
                    DownloadFileStream = null;
                }
                _logs.DebugMsg_1(nameof(DownloadFile) + " done");
                FailInfo = "";
                return true;
            }
            catch (Exception ex)
            {
                FailInfo = "Network fail";
                _logs.DebugMsg_1(nameof(DownloadFile) + " fail:" + ex.ToString());
                return false;
            }
        }
    }
}