using DDPM.SA.Common.Settings;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using System.Threading;

namespace DDPM.SA.Common.Security
{
    public class CertificateCheck
    {
        private List<X509Certificate2> TrustedPublisher = new List<X509Certificate2>();
        private List<X509Certificate2> TrustedRoot = new List<X509Certificate2>();
        private string[] Issuer = new string[] { "Entrust Certification Authority - L1F" };
        private string[] Subject = new string[] { "content-cdn.dell.com", "*.dell.com" };
        public bool CheckFile_SHA512(string CertificateFilePath, string Stande_SHA512, string Stande_Thumbprint, out string Info)
        {
            bool ret = false;
            Info = "";
            try
            {
                // 讀取憑證檔案並創建 X509Certificate2 物件
                X509Certificate2 certificate = new X509Certificate2(CertificateFilePath);
                ret = certificate.Thumbprint.ToLower().Equals(Stande_Thumbprint.ToLower());
                string info = string.Empty;
                ret = DDPMFileSecurity.GetFileSHA_512(CertificateFilePath, out info).ToLower().Equals(Stande_SHA512.ToLower()) && ret;
            }
            catch (Exception ex)
            {
                Info = "No signature Ex:" + ex.ToString();
            }
            return ret;
        }
        public bool CheckFile_SHA256(string CertificateFilePath, string Stande_SHA256, string Stande_Thumbprint, out string Info)
        {
            bool ret = false;
            Info = "";
            try
            {
                // 讀取憑證檔案並創建 X509Certificate2 物件
                X509Certificate2 certificate = new X509Certificate2(CertificateFilePath);
                ret = certificate.Thumbprint.ToLower().Equals(Stande_Thumbprint.ToLower());
                string info = string.Empty;
                ret = DDPMFileSecurity.GetFileSHA_256(CertificateFilePath, out info).ToLower().Equals(Stande_SHA256.ToLower()) && ret;
            }
            catch (Exception ex)
            {
                Info = "No signature Ex:" + ex.ToString();
            }
            return ret;
        }
        public bool CheckURLCACertificate(string URL)
        {
            bool flag = false;
            int num = 1;
            GetLocalTrustedCert();
            while (!flag && num > 0)
            {
                try
                {
                    HttpClientHandler handler = new HttpClientHandler();
                    ////測試Proxy使用
                    //var proxy = new WebProxy("http://192.168.137.31:808/", true)
                    //{
                    //    Credentials = new NetworkCredential("user-001", "user")
                    //};
                    //handler = new HttpClientHandler
                    //{
                    //    Proxy = proxy,
                    //    UseProxy = true
                    //};
                    handler.ServerCertificateCustomValidationCallback = PinPublicKey;
                    using (HttpClient client = new HttpClient(handler))
                    {
                        HttpResponseMessage response = client.GetAsync(URL).Result;
                    }
                    flag = true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("[CheckURLCACertificate] error:" + ex.Message.ToString());
                    flag = false;
                    Console.WriteLine(string.Format("[CheckURLCACertificate] error, retry:" + num));
                    Thread.Sleep(1000);
                }


                num--;
            }
            Console.WriteLine(string.Format("[CheckURLCACertificate] res:" + flag));
            if (!flag)
            {
                flag = CheckCAHTTP(URL);
            }
            if (!flag)
            {
                Console.WriteLine(string.Format("[CheckURLCACertificate][CheckCAHTTP] Fail, Send Telemetry." + flag));
            }
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
                    Console.WriteLine("[CheckCAHTTP] result:" + response);
                    return response;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("[CheckCAHTTP] error:" + ex.Message.ToString());
            }
            return false;
        }
        private bool ValidateCertificate(HttpRequestMessage request, X509Certificate2 certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            if (certificate == null)
            {
                Console.WriteLine("[ValidateCertificate] certificate null.");
                return false;
            }

            if (request == null)
            {
                Console.WriteLine("[ValidateCertificate] request null.");
            }

            if (sslPolicyErrors != 0)
            {
                Console.WriteLine($"[ValidateCertificate] sslPolicyErrors is Error. {sslPolicyErrors}");
            }

            if (chain == null)
            {
                Console.WriteLine("[ValidateCertificate] chain null.");
                return false;
            }
            return CheckCertificateExpiration(certificate) && CheckCertificateRevocation(certificate) && CheckIssuerAndSubject(certificate);
        }
        private bool PinPublicKey(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            X509Certificate2 certificate2 = new X509Certificate2(certificate);
            if (certificate == null)
            {
                Console.WriteLine("[PinPublicKey] certificate null.");
                return false;
            }
            HttpClient httpClient = sender as HttpClient;
            if (httpClient == null)
            {
                Console.WriteLine("[PinPublicKey] request null.");
            }
            if (chain == null)
            {
                Console.WriteLine("[PinPublicKey] chain null.");
                return false;
            }
            bool flag = false;
            flag = CheckCertificateExpiration(certificate2) && CheckCertificateRevocation(certificate2) && CheckIssuerAndSubject(certificate2);
            return flag;
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
                Console.WriteLine("[CheckHTTPAvailable] error:" + ex.Message.ToString());
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
                    Console.WriteLine("[GetResponse] statusCode:" + result.StatusCode);
                    flag = true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("[GetResponse] error:" + ex.Message.ToString());
                    flag = false;
                    Console.WriteLine(string.Format("[GetResponse] error, retry:" + num));
                    Thread.Sleep(1000);
                }
                num--;
            }
            Console.WriteLine(string.Format("[GetResponse] result:" + flag));
            return flag;
        }
        private void GetLocalTrustedCert()
        {
            try
            {
                X509Store x509Store_LocalMachine = new X509Store(StoreName.TrustedPublisher, StoreLocation.LocalMachine);
                X509Store x509Store_CurrentUser = new X509Store(StoreName.TrustedPublisher, StoreLocation.CurrentUser);
                x509Store_LocalMachine.Open(OpenFlags.ReadOnly);
                foreach (X509Certificate2 certificate in x509Store_LocalMachine.Certificates)
                {
                    TrustedPublisher.Add(certificate);
                }
                x509Store_CurrentUser.Open(OpenFlags.ReadOnly);
                foreach (X509Certificate2 certificate in x509Store_CurrentUser.Certificates)
                {
                    TrustedPublisher.Add(certificate);
                }
                x509Store_LocalMachine = new X509Store(StoreName.Root, StoreLocation.LocalMachine);
                x509Store_CurrentUser = new X509Store(StoreName.Root, StoreLocation.CurrentUser);
                x509Store_LocalMachine.Open(OpenFlags.ReadOnly);
                foreach (X509Certificate2 certificate2 in x509Store_LocalMachine.Certificates)
                {
                    TrustedRoot.Add(certificate2);
                }
                x509Store_LocalMachine.Close();
                x509Store_CurrentUser.Open(OpenFlags.ReadOnly);
                foreach (X509Certificate2 certificate2 in x509Store_CurrentUser.Certificates)
                {
                    TrustedRoot.Add(certificate2);
                }
                x509Store_CurrentUser.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine("[GetLocalTrustedCert] error:" + ex.Message.ToString());
            }
        }
        private bool CheckCertificateIsVaild(X509Certificate2 certificate)
        {
            bool result = false;
            try
            {
                X509Chain x509Chain = new X509Chain();
                x509Chain.ChainPolicy.RevocationFlag = X509RevocationFlag.EntireChain;
                x509Chain.ChainPolicy.RevocationMode = X509RevocationMode.Online;
                x509Chain.ChainPolicy.UrlRetrievalTimeout = new TimeSpan(0, 1, 0);
                x509Chain.ChainPolicy.VerificationFlags = X509VerificationFlags.NoFlag;
                result = x509Chain.Build(certificate);
            }
            catch (Exception ex)
            {
                Console.WriteLine("[CheckCertificateIsVaild] error: " + ex.Message);
            }
            return result;
        }
        private bool CheckIssuerAndSubject(X509Certificate2 certificate)
        {
            try
            {
                bool isSubjectCNMatch = false;
                bool isIssuerCNMatch = false;
                bool isSANCNMatch = false;
                bool isCNMatch = false;
                //Console.WriteLine("--------------CheckIssuerAndSubject------------------");
                //Console.WriteLine($"Issuer:{certificate.Issuer.ToString()}");
                //Console.WriteLine($"Subject:{certificate.Subject.ToString()}");
                //Console.WriteLine($"SubjectName-Name:{certificate.SubjectName.Name}");
                //Console.WriteLine($"IssuerName:{certificate.GetIssuerName()}");
                //Console.WriteLine($"NotAfter:{certificate.NotAfter.ToString()}");
                //Console.WriteLine($"NotBefore:{certificate.NotBefore.ToString()}");
                //Console.WriteLine($"PublicKey:{certificate.PublicKey}");
                //Console.WriteLine($"PublicKey:{certificate.GetPublicKeyString()}");
                //Console.WriteLine($"PrivateKey:{certificate.PrivateKey?.ToString()}");
                foreach (string sub in Subject)
                {
                    if (ExtractCN(certificate.Subject).Equals(sub))
                    {
                        isSubjectCNMatch = true;
                    }
                }
                foreach (string iss in Issuer)
                {
                    if (ExtractCN(certificate.Issuer).Equals(iss))
                    {
                        isIssuerCNMatch = true;
                    }
                }
                var sanList = GetSubjectAlternativeNames(certificate);
                //Console.WriteLine("Subject Alternative Names:");
                //Console.WriteLine("---SAN---");
                foreach (var san in sanList)
                {
                    //Console.WriteLine(san);
                    bool containsAny = ContainsAny(san, Subject);
                    if (containsAny)
                    {
                        //Console.WriteLine("[CheckIssuerAndSubject] Subject is included in the SAN.");
                        isSANCNMatch = true;
                    }
                    else
                    {
                        //Console.WriteLine("[CheckIssuerAndSubject] Subject is NOT included in the SAN.");
                    }
                }
                //Console.WriteLine("---SAN END---");
                isCNMatch = isSubjectCNMatch && isIssuerCNMatch && isSANCNMatch;
                if (isCNMatch)
                {
                    //Console.WriteLine("[CheckIssuerAndSubject] Is match.");
                }
                else
                {
                    if (!isSubjectCNMatch)
                    {
                        //Console.WriteLine("[CheckIssuerAndSubject] Subject is NOT match.");
                    }
                    if (!isIssuerCNMatch)
                    {
                        //Console.WriteLine("[CheckIssuerAndSubject] Issuer is NOT match.");
                    }
                    isCNMatch = ValidateProxyCertificate(certificate);
                    if (isCNMatch)
                    {
                        //Console.WriteLine("[CheckIssuerAndSubject] Proxy is match.");
                    }
                    else
                    {
                        //Console.WriteLine("[CheckIssuerAndSubject] Not match.");
                    }
                }
                //Console.WriteLine("--------------CheckIssuerAndSubject------------------");
                return isCNMatch;
            }
            catch (Exception ex)
            {
                Console.WriteLine("[CheckIssuerAndSubject] Error." + ex.ToString());
            }
            return false;
        }
        private bool ValidateProxyCertificate(X509Certificate2 certificate)
        {
            // Implement custom validation logic for proxy certificates
            // For example, check specific attributes or extensions
            Console.WriteLine("Going through Proxy");
            // List all certificates in the store
            foreach (X509Certificate2 cert in TrustedRoot)
            {
                Console.WriteLine("-------------Local Root-------------------");
                Console.WriteLine("CN: " + ExtractCN(cert.Subject));
                Console.WriteLine("Subject: " + cert.Subject);
                Console.WriteLine("Issuer: " + cert.Issuer);
                Console.WriteLine("Thumbprint: " + cert.Thumbprint);
                Console.WriteLine("Effective Date: " + cert.NotBefore);
                Console.WriteLine("Expiration Date: " + cert.NotAfter);
                Console.WriteLine("--------------------------------");
            }
            foreach (X509Certificate2 cert in TrustedPublisher)
            {
                Console.WriteLine("-------------Trusted Publisher-------------------");
                Console.WriteLine("CN: " + ExtractCN(cert.Subject));
                Console.WriteLine("Subject: " + cert.Subject);
                Console.WriteLine("Issuer: " + cert.Issuer);
                Console.WriteLine("Thumbprint: " + cert.Thumbprint);
                Console.WriteLine("Effective Date: " + cert.NotBefore);
                Console.WriteLine("Expiration Date: " + cert.NotAfter);
                Console.WriteLine("--------------------------------");
            }
            Console.WriteLine("Proxy check end");
            return true; // Assuming the proxy certificate is valid
        }
        private string ExtractCN(string subject)
        {
            if (string.IsNullOrEmpty(subject))
            {
                return null;
            }

            // Split the subject string into its components
            string[] subjectParts = subject.Split(',');

            foreach (string part in subjectParts)
            {
                // Trim and check if it starts with CN=
                string trimmedPart = part.Trim();
                if (trimmedPart.StartsWith("CN=", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine(trimmedPart.Substring(3).Trim());
                    // Return the value after CN=
                    return trimmedPart.Substring(3).Trim();
                }
            }

            // CN not found
            return null;
        }
        private string[] GetSubjectAlternativeNames(X509Certificate2 certificate)
        {
            var sanList = new System.Collections.Generic.List<string>();

            foreach (var extension in certificate.Extensions)
            {
                if (extension is X509Extension x509Extension)
                {
                    // Subject Alternative Name (SAN) extension OID: 2.5.29.17
                    if (x509Extension != null && x509Extension.Oid != null)
                    {
                        if (x509Extension.Oid.Value == "2.5.29.17" || x509Extension.Oid.Value == "Subject Alternative Name")
                        {
                            var sanExtension = new AsnEncodedData(x509Extension.Oid, x509Extension.RawData);
                            var sanString = sanExtension.Format(true);

                            // 解析 SAN 字符串
                            var regex = new Regex(@"DNS Name=(?<san>[^,]+)");
                            var matches = regex.Matches(sanString);
                            foreach (Match match in matches)
                            {
                                sanList.Add(match.Groups["san"].Value);
                            }
                        }
                    }

                }
            }
            return sanList.ToArray();
        }
        private bool ContainsAny(string mainString, string[] searchArray)
        {
            foreach (string searchTerm in searchArray)
            {
                if (mainString.Contains(searchTerm))
                {
                    return true;
                }
            }
            return false;
        }
        bool CheckCertificateExpiration(X509Certificate2 certificate)
        {
            bool ret = false;
            DateTime now = DateTime.Now;

            if (now < certificate.NotBefore)
            {
                Console.WriteLine("The certificate is not yet valid.");
            }
            else if (now > certificate.NotAfter)
            {
                Console.WriteLine("Certificate has expired.");
            }
            else
            {
                ret = true;
                Console.WriteLine("Certificate is valid.");
            }
            return ret;
        }
        bool CheckCertificateRevocation(X509Certificate2 certificate)
        {
            bool ret = false;
            X509Chain chain = new X509Chain();
            chain.ChainPolicy.RevocationMode = X509RevocationMode.Online; // 使用線上檢查
            chain.ChainPolicy.RevocationFlag = X509RevocationFlag.ExcludeRoot; // 不檢查根憑證的吊銷狀態

            bool isChainValid = chain.Build(certificate);
            if (isChainValid)
            {
                ret = true;
                Console.WriteLine("Credential has not been revoked.");
            }
            else
            {
                Console.WriteLine("Credentials may be revoked.");
                foreach (X509ChainStatus status in chain.ChainStatus)
                {
                    Console.WriteLine($"Error: {status.StatusInformation}");
                }
            }
            return ret;
        }
    }
}