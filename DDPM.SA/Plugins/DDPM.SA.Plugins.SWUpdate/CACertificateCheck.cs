using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Security;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;
using System.Windows.Documents;
using System.Security.Cryptography;
using System.IO;
using VcpCore.Common;

namespace DDPM.SA.Plugins.SWUpdate
{
    public class CACertificateCheck
    {
        private Logs _logs;
        bool SkippedCA;
        private List<string> CAkeys = new List<string>();
        private List<string> DisabledCAList = new List<string>();
        private List<X509Certificate2> TrustedPublisher = new List<X509Certificate2>();
        private List<X509Certificate2> TrustedRoot = new List<X509Certificate2>();
        private readonly string[] Issuer = { "CN=Entrust Certification Authority - L1F, O=\"Entrust, C=US", "CN=localhost, O=DigiNow, C=US" };
        private readonly string[] Subject = { "CN=content-cdn.dell.com, O=Dell, C=US", "CN=localhost, O=DigiNow, C=US" };
        private string[] Issuers = new string[1];
        private string[] Subjects = new string[10];
        public CACertificateCheck(Logs logs)
        {
            _logs = logs;
            for (int i = 0; i < Issuer.Length; i++)
            {
                Issuers = Issuer[i].Split(",");
                Subjects = Subject[i].Split(",");
            }
        }
        public bool CheckFileCA(string filePath)
        {
            _logs.DebugMsg_1(nameof(CheckFileCA) + " start");
            // 讀取憑證檔案並創建 X509Certificate2 物件
            X509Certificate2 certificate = new X509Certificate2(filePath);

            // 創建一個 X509Chain 物件
            X509Chain chain = new X509Chain();
            chain.Build(certificate);

            // 設置 SSL 策略錯誤為 None，因為我們在這裡不處理 SSL 策略錯誤
            SslPolicyErrors sslPolicyErrors = SslPolicyErrors.None;
            bool b = PinPublicKey(null, certificate, chain, sslPolicyErrors) && CheckSHA512(filePath);
            _logs.DebugMsg_1(nameof(CheckFileCA) + " done");
            return b;
        }
        bool CheckSHA512(string filePath)
        {
            try
            {
                // 期望的SHA-512雜湊值（假設已知）
                string expectedHash = "expected_SHA512_hash";

                // 計算檔案的SHA-512雜湊
                string computedHash = CalculateFileSHA512(filePath);

                // 比較計算的雜湊值和期望的雜湊值
                if (computedHash == expectedHash)
                {
                    _logs.DebugMsg_1(nameof(CheckSHA512) + " done");
                    return true;
                }
                else
                {
                    _logs.DebugMsg_1(nameof(CheckSHA512) + " fail");
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logs.DebugMsg_1(nameof(CheckFileCA) + " Error" + ex.ToString());
                return false;
            }
        }
        string CalculateFileSHA512(string filePath)
        {
            using (FileStream fileStream = File.OpenRead(filePath))
            {
                using (SHA512 sha512 = SHA512.Create())
                {
                    byte[] hashBytes = sha512.ComputeHash(fileStream);

                    // 將計算的雜湊值轉換為十六進制字符串
                    return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
                }
            }
        }
        //0613 Bruce 新增SHA256備用
        string CalculateFileSHA256(string filePath)
        {
            using (FileStream fileStream = File.OpenRead(filePath))
            {
                using (SHA256 sha256 = SHA256.Create())
                {
                    byte[] hashBytes = sha256.ComputeHash(fileStream);

                    // 將計算的雜湊值轉換為十六進制字符串
                    return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
                }
            }
        }
        public bool CheckCA(string URL)
        {
            _logs.DebugMsg_1(nameof(CheckCA) + " start");
            SkippedCA = false;
            bool flag = false;
            int num = 1;
            GetLocalTrustedCert();
            while (!flag && num > 0)
            {
                try
                {
                    //0603 Bruce 因VS顯示WebRequest.Create(URL)為過時寫法，故修改為以下方法
                    HttpClientHandler handler = new HttpClientHandler();
                    handler.ServerCertificateCustomValidationCallback = PinPublicKey;
                    using (HttpClient client = new HttpClient(handler))
                    {
                        HttpResponseMessage response = client.GetAsync(URL).Result;
                    }
                    flag = true;
                }
                catch (Exception ex)
                {
                    _logs.DebugMsg_1("[CheckCA] error:" + ex.Message.ToString());
                    flag = false;
                    _logs.DebugMsg_1(string.Format("[CheckCA] error, retry:" + num));
                    Thread.Sleep(1000);
                }
                num--;
            }
            _logs.DebugMsg_1(string.Format("[CheckCA] res:" + flag));
            if (!flag)
            {
                flag = CheckCAHTTP(URL);
            }
            if (!flag)
            {
                //Console.WriteLine(string.Format("[CheckCA][CheckCAHTTP] Fail, Send Telemetry." + flag));
                /*ClassMessage.sendMessage2WPF_Telemetry(14, new TelemEventData.SoftwareFailure
                {
                    ErrorCode = Convert.ToInt32(TelemEventData.ErrorCode.CheckCAFail).ToString(),
                    FailureMessage = "Check CA Fail."
                });*/
            }
            if (SkippedCA)
            {
                /*ClassMessage.sendMessage2WPF_Telemetry(14, new TelemEventData.SoftwareFailure
                {
                    ErrorCode = Convert.ToInt32(TelemEventData.ErrorCode.SkippedCA).ToString(),
                    FailureMessage = "Skipped CA."
                });*/
            }
            _logs.DebugMsg_1(nameof(CheckCA) + " done");
            return flag;
        }
        private bool CheckCAHTTP(string URL)
        {
            try
            {
                if (CheckHTTPAvailable(URL))
                {
                    HttpClientHandler httpClientHandler = new HttpClientHandler();
                    httpClientHandler.ServerCertificateCustomValidationCallback = ValidateCertificate;
                    HttpClient client = new HttpClient(httpClientHandler);
                    bool response = GetResponse(client, URL);
                    _logs.DebugMsg_1("[CheckCAHTTP] result:" + response);
                    return response;
                }
            }
            catch (Exception ex)
            {
                _logs.DebugMsg_1("[CheckCAHTTP] error:" + ex.Message.ToString());
            }
            return false;
        }
        private bool ValidateCertificate(HttpRequestMessage request, X509Certificate2? certificate, X509Chain? chain, SslPolicyErrors sslPolicyErrors)
        {
            if (certificate == null)
            {
                _logs.DebugMsg_1("[ValidateCertificate] certificate null.");
                return false;
            }

            if (request == null)
            {
                _logs.DebugMsg_1("[ValidateCertificate] request null.");
            }

            if (sslPolicyErrors != 0)
            {
                _logs.DebugMsg_1($"[ValidateCertificate] sslPolicyErrors is Error. {sslPolicyErrors}");
            }

            if (chain == null)
            {
                _logs.DebugMsg_1("[ValidateCertificate] chain null.");
                return false;
            }
            return CheckCertificateIsVaild(certificate) && CheckIssuerAndSubject(certificate);
        }
        private bool PinPublicKey(object? sender, X509Certificate? certificate, X509Chain? chain, SslPolicyErrors sslPolicyErrors)
        {
            if (certificate == null)
            {
                _logs.DebugMsg_1("[PinPublicKey] certificate null.");
                return false;
            }
            X509Certificate2 certificate2 = new X509Certificate2(certificate);
            HttpWebRequest? httpWebRequest = sender as HttpWebRequest;
            if (httpWebRequest == null)
            {
                _logs.DebugMsg_1("[PinPublicKey] request null.");
                return false;
            }
            if (chain == null)
            {
                _logs.DebugMsg_1("[PinPublicKey] chain null.");
                return false;
            }

            return CheckIssuerAndSubject(certificate2) && CheckCertificateIsVaild(certificate2);
        }
        private bool CheckHTTPAvailable(string URL)
        {
            try
            {
                HttpClientHandler handler = new HttpClientHandler();
                HttpClient httpClient = new HttpClient(handler);
                httpClient.GetAsync(URL).GetAwaiter().GetResult();
                return true;
            }
            catch (Exception ex)
            {
                _logs.DebugMsg_1("[CheckHTTPAvailable] error:" + ex.Message.ToString());
            }
            return false;
        }
        private bool GetResponse(HttpClient client, string URL)
        {
            bool flag = false;
            int num = 5;
            while (!flag && num > 0)
            {
                try
                {
                    HttpResponseMessage result = client.GetAsync(URL).GetAwaiter().GetResult();
                    _logs.DebugMsg_1("[GetResponse] statusCode:" + result.StatusCode);
                    flag = true;
                }
                catch (Exception ex)
                {
                    _logs.DebugMsg_1("[GetResponse] error:" + ex.Message.ToString());
                    flag = false;
                    _logs.DebugMsg_1(string.Format("[GetResponse] error, retry:" + num));
                    Thread.Sleep(1000);
                }
                num--;
            }
            _logs.DebugMsg_1(string.Format("[GetResponse] result:" + flag));
            return flag;
        }
        private void GetLocalTrustedCert()
        {
            try
            {
                X509Store x509Store = new X509Store(StoreName.TrustedPublisher, StoreLocation.LocalMachine);
                x509Store.Open(OpenFlags.ReadOnly);
                foreach (X509Certificate2 certificate in x509Store.Certificates)
                {
                    TrustedPublisher.Add(certificate);
                }
                x509Store = new X509Store(StoreName.Root, StoreLocation.LocalMachine);
                x509Store.Open(OpenFlags.ReadOnly);
                foreach (X509Certificate2 certificate2 in x509Store.Certificates)
                {
                    TrustedRoot.Add(certificate2);
                }
                x509Store.Close();
            }
            catch (Exception ex)
            {
                _logs.DebugMsg_1("[GetLocalTrustedCert] error:" + ex.Message.ToString());
            }
        }
        private bool CheckCertificateIsVaild(X509Certificate2 certificate)
        {
            bool result = false;
            try
            {
                X509Chain x509Chain = new X509Chain();
                // 設置憑證鏈的撤銷標誌為 EntireChain，表示整個鏈上的所有憑證都將被檢查撤銷狀態
                x509Chain.ChainPolicy.RevocationFlag = X509RevocationFlag.EntireChain;
                // 設置撤銷模式為 Online，表示將使用線上撤銷檢查來驗證憑證
                x509Chain.ChainPolicy.RevocationMode = X509RevocationMode.Online;
                x509Chain.ChainPolicy.UrlRetrievalTimeout = new TimeSpan(0, 1, 0);
                // 設置驗證標誌為 NoFlag，表示不使用任何驗證標誌
                x509Chain.ChainPolicy.VerificationFlags = X509VerificationFlags.NoFlag;
                // 使用 Build 方法來建構憑證鏈，如果成功建構鏈，則將 result 設置為 true，表示憑證有效
                result = x509Chain.Build(certificate);
                // 檢查憑證有效期
                DateTime now = DateTime.Now;
                result = (certificate.NotBefore < now && now < certificate.NotAfter) && result;
            }
            catch (Exception ex)
            {
                _logs.DebugMsg_1("[CheckCertificateIsVaild] error: " + ex.Message);
            }
            return result;
        }
        private bool CheckIssuerAndSubject(X509Certificate2 certificate)
        {
            try
            {
                foreach (string s in Issuers)
                {
                    if (!certificate.Issuer.Contains(s))
                    {
                        _logs.DebugMsg_1("[CheckIssuerAndSubject] Not match.");
                        return false;
                    }
                }
                foreach (string s in Subjects)
                {
                    if (!certificate.Subject.Contains(s))
                    {
                        _logs.DebugMsg_1("[CheckIssuerAndSubject] Not match.");
                        return false;
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                _logs.DebugMsg_1("[CheckIssuerAndSubject] Error: " + ex.Message);
            }
            return false;
        }
        private string CallCertificateCheck(string param)
        {
            string text = "";
            /*string text2 = Validator.SecurityPathFilter(Path.Combine(ProgramDataHandler.InstallPath, "CertificateCheck.exe"));
            if (ProgramDataHandler.InstallPath.Length > 0 && File.Exists(text2))
            {
                using Process process = new Process();
                process.StartInfo.FileName = text2;
                process.StartInfo.Arguments = param;
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                if (MISCLibrary.RunProcess(process) == MISCLibrary.RunProcessReturnCode.Success)
                {
                    process.WaitForExit();
                    text = process.StandardOutput.ReadLine();
                }
                if (text == null)
                {
                    return "";
                }
            }*/
            return text;
        }
    }
}
