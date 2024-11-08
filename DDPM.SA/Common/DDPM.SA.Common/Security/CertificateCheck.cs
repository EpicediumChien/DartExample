using DDPM.SA.Common.Settings;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Security;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using System.Threading;
using VcpCore.Common;

namespace DDPM.SA.Common.Security
{
    public class CertificateCheck
    {
        private Logs? _logs;
        private string[] Issuer = new string[] { "Entrust Certification Authority - L1F" };
        private string[] Subject = new string[] { "content-cdn.dell.com", "*.dell.com" };
        public CertificateCheck(Logs logs)
        {
            _logs = logs;
        }
        public bool CheckFile_SHA512(string CertificateFilePath, string Stande_SHA512, out string Info)
        {
            bool ret = false;
            Info = "SHA512 Is Null Or Empty";
            if (!string.IsNullOrEmpty(Stande_SHA512))
            {
                try
                {
                    ret = DDPMFileSecurity.GetFileSHA_512(CertificateFilePath, out Info).ToLower().Equals(Stande_SHA512.ToLower());
                    if (Info.Equals("Complete"))
                    {
                        Info = ret ? "Check ok" : "Check fail";
                    }
                }
                catch (Exception ex)
                {
                    Info = "No signature Ex:" + ex.ToString();
                }
            }
            return ret;
        }
        public bool CheckFile_SHA256(string CertificateFilePath, string Stande_SHA256, out string Info)
        {
            bool ret = false;
            Info = "SHA256 Is Null Or Empty";
            if (!string.IsNullOrEmpty(Stande_SHA256))
            {
                try
                {
                    ret = DDPMFileSecurity.GetFileSHA_256(CertificateFilePath, out Info).ToLower().Equals(Stande_SHA256.ToLower());
                    if (Info.Equals("Complete"))
                    {
                        Info = ret ? "Check ok" : "Check fail";
                    }
                }
                catch (Exception ex)
                {
                    Info = "No signature Ex:" + ex.ToString();
                }
            }
            return ret;
        }
        public bool CheckFile_Thumbprint(string CertificateFilePath, string Stande_Thumbprint, out string Info)
        {
            bool ret = false;
            Info = "Thumbprint Is Null Or Empty";
            if (!string.IsNullOrEmpty(Stande_Thumbprint))
            {
                try
                {
                    if (!DDPMFileSecurity.VerifyExecutableFileSignature(CertificateFilePath, out Info))
                    {
#if DEBUG
                        Console.WriteLine(Info);
#endif
                        return false;
                    }
                    // 讀取憑證檔案並創建 X509Certificate2 物件
                    X509Certificate2 certificate = new X509Certificate2(CertificateFilePath);

                    //if(!CheckCertificateIsVaild(certificate))
                    //{
                    //    return ret;
                    //}

                    ret = certificate.Thumbprint.ToLower().Equals(Stande_Thumbprint.ToLower());
                }
                catch (Exception ex)
                {
                    Info = "No signature Ex:" + ex.ToString();
                }
            }
            return ret;
        }
        public bool CheckFile_Thumbprint_List(string CertificateFilePath, List<string> Stande_Thumbprint, out string Info)
        {
            bool ret = false;
            Info = "Thumbprint Is Null Or Empty";
            if (Stande_Thumbprint != null && Stande_Thumbprint.Count > 0)
            {
                try
                {
                    if (!DDPMFileSecurity.VerifyExecutableFileSignature(CertificateFilePath, out Info))
                    {
#if DEBUG
                        Console.WriteLine(Info);
#endif
                        return false;
                    }
                    // 讀取憑證檔案並創建 X509Certificate2 物件
                    X509Certificate2 certificate = new X509Certificate2(CertificateFilePath);

                    //if(!CheckCertificateIsVaild(certificate))
                    //{ 
                    //    return ret; 
                    //}

                    for (int i = 0; i < Stande_Thumbprint.Count; i++)
                    {
                        ret = certificate.Thumbprint.ToLower().Equals(Stande_Thumbprint[i].ToLower());
                        if (ret)
                        {
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Info = "No signature Ex:" + ex.ToString();
                }
            }
            return ret;
        }
        public bool CheckURLCACertificate(string URL)
        {
            bool flag = false;
            int num = 1;
            while (!flag && num > 0)
            {
                try
                {
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
                    _logs?.DebugMsg_1("[CheckURLCACertificate] error:" + ex.Message.ToString());
                    flag = false;
                    _logs?.DebugMsg_1(string.Format("[CheckURLCACertificate] error, retry:" + num));
                    Thread.Sleep(1000);
                }
                num--;
            }
            _logs?.DebugMsg_1(string.Format("[CheckURLCACertificate] res:" + flag));
            if (!flag)
            {
                flag = CheckCAHTTP(URL);
            }
            if (!flag)
            {
                _logs?.DebugMsg_1(string.Format("[CheckURLCACertificate][CheckCAHTTP] Fail, Send Telemetry." + flag));
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
                    _logs?.DebugMsg_1("[CheckCAHTTP] result:" + response);
                    return response;
                }
            }
            catch (Exception ex)
            {
                _logs?.DebugMsg_1("[CheckCAHTTP] error:" + ex.Message.ToString());
            }
            return false;
        }
        private bool ValidateCertificate(HttpRequestMessage request, X509Certificate2 certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            if (certificate == null)
            {
                _logs?.DebugMsg_1("[ValidateCertificate] certificate null.");
                return false;
            }

            if (request == null)
            {
                _logs?.DebugMsg_1("[ValidateCertificate] request null.");
            }

            if (sslPolicyErrors != 0)
            {
                _logs?.DebugMsg_1($"[ValidateCertificate] sslPolicyErrors is Error. {sslPolicyErrors}");
            }

            if (chain == null)
            {
                _logs?.DebugMsg_1("[ValidateCertificate] chain null.");
                return false;
            }
            return CheckCertificateExpiration(certificate) && CheckCertificateRevocation(certificate) && CheckIssuerAndSubject(certificate, chain);
        }
        private bool PinPublicKey(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            X509Certificate2 certificate2 = new X509Certificate2(certificate);
            if (certificate == null)
            {
                _logs?.DebugMsg_1("[PinPublicKey] certificate null.");
                return false;
            }

            if(!CheckCertificateIsVaild(certificate2))
            {
                return false;
            }

            HttpClient httpClient = sender as HttpClient;
            if (httpClient == null)
            {
                _logs?.DebugMsg_1("[PinPublicKey] request null.");
            }
            if (chain == null)
            {
                _logs?.DebugMsg_1("[PinPublicKey] chain null.");
                return false;
            }
            bool flag = false;
            flag = CheckCertificateExpiration(certificate2) && CheckCertificateRevocation(certificate2) && CheckIssuerAndSubject(certificate2, chain);
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
                _logs?.DebugMsg_1("[CheckHTTPAvailable] error:" + ex.Message.ToString());
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
                    _logs?.DebugMsg_1("[GetResponse] statusCode:" + result.StatusCode);
                    flag = true;
                }
                catch (Exception ex)
                {
                    _logs?.DebugMsg_1("[GetResponse] error:" + ex.Message.ToString());
                    flag = false;
                    _logs?.DebugMsg_1(string.Format("[GetResponse] error, retry:" + num));
                    Thread.Sleep(1000);
                }
                num--;
            }
            _logs?.DebugMsg_1(string.Format("[GetResponse] result:" + flag));
            return flag;
        }

        public bool CheckCertificateIsVaild(X509Certificate2 certificate)
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
                _logs?.DebugMsg_1("[CheckCertificateIsVaild] error: " + ex.Message);
            }
            return result;
        }
        private bool CheckIssuerAndSubject(X509Certificate2 certificate, X509Chain chain)
        {
            try
            {
                bool isSubjectCNMatch = false;
                bool isIssuerCNMatch = false;
                bool isSANCNMatch = false;
                bool isCNMatch = false;
                //_logs?.DebugMsg_1("--------------CheckIssuerAndSubject------------------");
                //_logs?.DebugMsg_1($"Issuer:{certificate.Issuer.ToString()}");
                //_logs?.DebugMsg_1($"Subject:{certificate.Subject.ToString()}");
                //_logs?.DebugMsg_1($"SubjectName-Name:{certificate.SubjectName.Name}");
                //_logs?.DebugMsg_1($"IssuerName:{certificate.GetIssuerName()}");
                //_logs?.DebugMsg_1($"NotAfter:{certificate.NotAfter.ToString()}");
                //_logs?.DebugMsg_1($"NotBefore:{certificate.NotBefore.ToString()}");
                //_logs?.DebugMsg_1($"PublicKey:{certificate.PublicKey}");
                //_logs?.DebugMsg_1($"PublicKey:{certificate.GetPublicKeyString()}");
                //_logs?.DebugMsg_1($"PrivateKey:{certificate.PrivateKey?.ToString()}");
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
                //_logs?.DebugMsg_1("Subject Alternative Names:");
                //_logs?.DebugMsg_1("---SAN---");
                foreach (var san in sanList)
                {
                    //_logs?.DebugMsg_1(san);
                    bool containsAny = ContainsAny(san, Subject);
                    if (containsAny)
                    {
                        //_logs?.DebugMsg_1("[CheckIssuerAndSubject] Subject is included in the SAN.");
                        isSANCNMatch = true;
                    }
                    else
                    {
                        //_logs?.DebugMsg_1("[CheckIssuerAndSubject] Subject is NOT included in the SAN.");
                    }
                }
                //_logs?.DebugMsg_1("---SAN END---");
                isCNMatch = isSubjectCNMatch && isIssuerCNMatch && isSANCNMatch;
                if (isCNMatch)
                {
                    //_logs?.DebugMsg_1("[CheckIssuerAndSubject] Is match.");
                }
                else
                {
                    if (!isSubjectCNMatch)
                    {
                        //_logs?.DebugMsg_1("[CheckIssuerAndSubject] Subject is NOT match.");
                    }
                    if (!isIssuerCNMatch)
                    {
                        //_logs?.DebugMsg_1("[CheckIssuerAndSubject] Issuer is NOT match.");
                    }
                    // Additional logic to handle proxy certificates if the above checks failed
                    var storeNames = new[] { StoreName.Root, StoreName.TrustedPublisher };
                    var storeLocations = new[] { StoreLocation.LocalMachine, StoreLocation.CurrentUser };
                    // Implement custom validation logic for proxy certificates
                    _logs?.DebugMsg_1("Going through Proxy");
                    // Iterate through each combination of store name and location
                    foreach (var storeLocation in storeLocations)
                    {
                        if (!isCNMatch)
                        {
                            foreach (var storeName in storeNames)
                            {
                                if (ValidateProxyCertificate(storeName, storeLocation, certificate, chain))
                                {
                                    _logs?.DebugMsg_1($"store Name:  {storeName} {storeLocation}");
                                    isCNMatch = true;
                                    break;
                                }
                            }
                        }
                    }
                    if (isCNMatch)
                    {
                        //_logs?.DebugMsg_1("[CheckIssuerAndSubject] Proxy is match.");
                    }
                    else
                    {
                        //_logs?.DebugMsg_1("[CheckIssuerAndSubject] Not match.");
                    }
                }
                //_logs?.DebugMsg_1("--------------CheckIssuerAndSubject------------------");
                return isCNMatch;
            }
            catch (Exception ex)
            {
                _logs?.DebugMsg_1("[CheckIssuerAndSubject] Error." + ex.ToString());
            }
            return false;
        }
        private bool ValidateProxyCertificate(StoreName storeName, StoreLocation storeLocation, X509Certificate2 certificate, X509Chain chain)
        {
            // Implement custom validation logic for proxy certificates
            // For example, check specific attributes or extensions
            _logs?.DebugMsg_1("Going through Proxy");

            bool trustedRootMatched = false;

            // Open the store
            X509Store store = new X509Store(storeName, storeLocation);
            store.Open(OpenFlags.ReadOnly);

            foreach (var cert1 in chain.ChainElements)
            {
                if (cert1.Certificate.Issuer == cert1.Certificate.Subject)
                {
                    X509Certificate2 rootCertificate = cert1.Certificate;
                    //_logs?.DebugMsg_1($"Validate Root Certificate:  {cert1.Certificate.Issuer} {cert1.Certificate.Subject}");

                    // List all valid certificates in the store
                    foreach (X509Certificate2 cert in store.Certificates)
                    {
                        if (rootCertificate != null && IsValidCertificate(cert, chain))
                        {
                            if (rootCertificate.Issuer.Equals(cert.Issuer))
                            {
                                trustedRootMatched = true;
                                //_logs?.DebugMsg_1($"Matching Root Certificate:  {cert.Issuer} {rootCertificate.Subject}");
                                break;
                            }
                        }
                    }
                }
            }
            // Close the store
            store.Close();
            return trustedRootMatched; //proxy certitifacate is invalid
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
                    _logs?.DebugMsg_1(trimmedPart.Substring(3).Trim());
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
                _logs?.DebugMsg_1("The certificate is not yet valid.");
            }
            else if (now > certificate.NotAfter)
            {
                _logs?.DebugMsg_1("Certificate has expired.");
            }
            else
            {
                ret = true;
                _logs?.DebugMsg_1("Certificate is valid.");
            }
            return ret;
        }
        bool CheckCertificateRevocation(X509Certificate2 certificate)
        {
            bool ret = false;
            X509Chain chain = new X509Chain();
            chain.ChainPolicy.RevocationMode = X509RevocationMode.Online;
            chain.ChainPolicy.RevocationFlag = X509RevocationFlag.EntireChain;
            chain.ChainPolicy.UrlRetrievalTimeout = new TimeSpan(0, 1, 0); // 1 minute timeout
            chain.ChainPolicy.VerificationFlags = X509VerificationFlags.NoFlag;

            bool isChainValid = chain.Build(certificate);

            if (isChainValid)
            {
                _logs?.DebugMsg_1("Credential has been Build.");
                ret = true;
                foreach (X509ChainStatus status in chain.ChainStatus)
                {
                    if (status.Status == X509ChainStatusFlags.Revoked)
                    {
                        ret = false;
                        _logs?.DebugMsg_1($"{status.StatusInformation} Credentials may be revoked");
                        break;
                    }
                }
            }
            else
            {
                _logs?.DebugMsg_1("Credentials has not been Build.");
            }
            return ret;
        }
        bool IsValidCertificate(X509Certificate2 certificate, X509Chain chain)
        {
            try
            {
                // Check if the certificate is expired
                if (DateTime.Now > certificate.NotAfter || DateTime.Now < certificate.NotBefore)
                {
                    throw new Exception("Certificate is expired");
                }

                // Build the chain and check for revocation
                chain.ChainPolicy.RevocationMode = X509RevocationMode.Online;
                chain.ChainPolicy.RevocationFlag = X509RevocationFlag.EntireChain;
                chain.ChainPolicy.UrlRetrievalTimeout = new TimeSpan(0, 1, 0); // 1 minute timeout
                chain.ChainPolicy.VerificationFlags = X509VerificationFlags.NoFlag;

                bool isValid = chain.Build(certificate);
                if (isValid) _logs?.DebugMsg_1($"Certificate is valid: {certificate.PublicKey}");

                foreach (X509ChainStatus status in chain.ChainStatus)
                {
                    if (status.Status == X509ChainStatusFlags.Revoked)
                    {
                        throw new Exception("Certificate is revoked");
                    }
                }
                return isValid;
            }
            catch (Exception ex)
            {
                _logs?.DebugMsg_1(ex.Message);
                return false;
            }
        }
    }
}