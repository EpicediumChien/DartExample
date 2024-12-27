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
        //private string[] Issuer = new string[] { "Entrust Certification Authority - L1F" };
        private string[] Subject = new string[]
        {
            "content-cdn.dell.com",
            "www.dell.com",
            "downloads.dell.com",
            "ftp.dell.com",
            "clientperipherals.dell.com"
        };//change from *.dell.com to www.dell.com
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
                    string fileSHA512 = DDPMFileSecurity.GetFileSHA_512(CertificateFilePath, out Info);
                    ret = fileSHA512.ToLower().Equals(Stande_SHA512.ToLower());
                    _logs?.DebugMsg_1("[CheckFile_Thumbprint] Stande_SHA512 : " + Stande_SHA512.ToLower());
                    _logs?.DebugMsg_1("[CheckFile_Thumbprint] fileSHA512 : " + fileSHA512.ToLower());
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
                    string fileSHA256 = DDPMFileSecurity.GetFileSHA_256(CertificateFilePath, out Info);
                    ret = fileSHA256.ToLower().Equals(Stande_SHA256.ToLower());
                    _logs?.DebugMsg_1("[CheckFile_Thumbprint] Stande_SHA256 : " + Stande_SHA256.ToLower());
                    _logs?.DebugMsg_1("[CheckFile_Thumbprint] fileSHA256 : " + fileSHA256.ToLower());
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
                    if (!DDPMFileSecurity.VerifyExecutableFileSignature(CertificateFilePath, out Info)) //SDL CheckMarx
                    {
#if DEBUG
                        Console.WriteLine(Info);
#endif
                        return false;
                    }
                    // 讀取憑證檔案並創建 X509Certificate2 物件
                    //X509Certificate2 certificate = DDPMFileSecurity.LoadFileCertificate(CertificateFilePath);//new X509Certificate2(CertificateFilePath);
                    //ret = certificate.Thumbprint.ToLower().Equals(Stande_Thumbprint.ToLower());
                    ret = DDPMFileSecurity.VerifyFileCertWithThumbprint(CertificateFilePath, Stande_Thumbprint, out Info);
                    _logs?.DebugMsg_1("[CheckFile_Thumbprint] Stande_Thumbprint : " + Stande_Thumbprint.ToLower());
                    _logs?.DebugMsg_1($"[CheckFile_Thumbprint] Using WinTrustVerify result is [{ret}]" + (ret ? "." : $" Fail with {Info}"));
                    if (!ret)
                        Info = "Load file cert to check thumbprint and the result is not matched";
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
                    //X509Certificate2 certificate = DDPMFileSecurity.LoadFileCertificate(CertificateFilePath); //new X509Certificate2(CertificateFilePath);

                    for (int i = 0; i < Stande_Thumbprint.Count; i++)
                    {
                        //ret = certificate.Thumbprint.ToLower().Equals(Stande_Thumbprint[i].ToLower());
                        ret = DDPMFileSecurity.SignedFileThumbprintVerifier(null, CertificateFilePath, Stande_Thumbprint[i], out Info);
                        if (ret)
                        {
                            break;
                        }
                    }
                    if (!ret)
                        Info = "Load file cert to check thumbprint and the result is not matched";
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
            _logs?.DebugMsg_1($"CheckURLCACertificate start");
            bool flag = false;
            _logs?.DebugMsg_1($"CheckURLCACertificate URL.IsNullOrEmpty : {string.IsNullOrEmpty(URL)}");
            if (!string.IsNullOrEmpty(URL))
            {
                //Bruce 1210 Take the complete URL and only take a screenshot of the first network segment
                Uri uri = new Uri(URL);
                string baseUrl = uri.GetLeftPart(UriPartial.Authority);
                _logs?.DebugMsg_1($"CheckURLCACertificate Url.IsNullOrEmpty : {string.IsNullOrEmpty(baseUrl)}");
                if (!string.IsNullOrEmpty(baseUrl))
                {
                    _logs?.DebugMsg_1($"CheckURLCACertificate Url : {baseUrl}");
                    int num = 1;
                    while (!flag && num > 0)
                    {
                        try
                        {
                            _logs?.DebugMsg_1($"CheckURLCACertificate HttpClientHandler initialization");
                            HttpClientHandler handler = new HttpClientHandler();
                            handler.ServerCertificateCustomValidationCallback = PinPublicKey;
                            using (HttpClient client = new HttpClient(handler))
                            {
                                _logs?.DebugMsg_1($"CheckURLCACertificate client.GetAsync go");
                                HttpResponseMessage response = client.GetAsync(baseUrl).Result;
                            }
                            flag = true;
                            _logs?.DebugMsg_1($"CheckURLCACertificate client.GetAsync finish");
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
                        _logs?.DebugMsg_1($"CheckURLCACertificate CheckCAHTTP go");
                        flag = CheckCAHTTP(baseUrl);
                        _logs?.DebugMsg_1($"CheckURLCACertificate CheckCAHTTP finish");
                    }
                    if (!flag)
                    {
                        _logs?.DebugMsg_1(string.Format("[CheckURLCACertificate][CheckCAHTTP] Fail, Send Telemetry." + flag));
                    }
                }
            }
            _logs?.DebugMsg_1($"CheckURLCACertificate finish");
            return flag;
        }
        private bool CheckCAHTTP(string URL)
        {
            _logs?.DebugMsg_1($"CheckCAHTTP start");
            try
            {
                if (CheckHTTPAvailable(URL))
                {
                    HttpClientHandler httpClientHandler = new HttpClientHandler();
                    httpClientHandler.ServerCertificateCustomValidationCallback = ValidateCertificate;
                    HttpClient client = new HttpClient(httpClientHandler);
                    _logs?.DebugMsg_1($"CheckCAHTTP GetResponse go");
                    bool response = GetResponse(client, URL);
                    _logs?.DebugMsg_1($"CheckCAHTTP GetResponse finish");
                    _logs?.DebugMsg_1("[CheckCAHTTP] result:" + response);
                    _logs?.DebugMsg_1($"CheckCAHTTP finish");
                    return response;
                }
            }
            catch (Exception ex)
            {
                _logs?.DebugMsg_1("[CheckCAHTTP] error:" + ex.Message.ToString());
            }
            _logs?.DebugMsg_1($"CheckCAHTTP finish");
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
        private bool PinPublicKey(object sender, X509Certificate2 certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            //X509Certificate2 certificate2 = new X509Certificate2(certificate);
            if (certificate == null)
            {
                _logs?.DebugMsg_1("[PinPublicKey] certificate null.");
                return false;
            }

            if (!CheckCertificateIsVaild(certificate))
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
            flag = CheckCertificateExpiration(certificate) && CheckCertificateRevocation(certificate) && CheckIssuerAndSubject(certificate, chain);
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
            _logs?.DebugMsg_1($"GetResponse start");
            bool flag = false;
            int num = 5;
            while (!flag && num > 0)
            {
                try
                {
                    _logs?.DebugMsg_1($"GetResponse client.GetAsync go");
                    HttpResponseMessage result = client.GetAsync(URL).GetAwaiter().GetResult();
                    _logs?.DebugMsg_1($"GetResponse client.GetAsync finish");
                    _logs?.DebugMsg_1("GetResponse statusCode:" + result.StatusCode);
                    flag = true;
                }
                catch (Exception ex)
                {
                    _logs?.DebugMsg_1("GetResponse error:" + ex.Message.ToString());
                    flag = false;
                    _logs?.DebugMsg_1(string.Format("GetResponse error, retry:" + num));
                    Thread.Sleep(1000);
                }
                num--;
            }
            _logs?.DebugMsg_1(string.Format("GetResponse result:" + flag));
            _logs?.DebugMsg_1($"GetResponse finish");
            return flag;
        }

        public bool CheckCertificateIsVaild(X509Certificate2 certificate)
        {
            _logs?.DebugMsg_1($"CheckCertificateIsVaild start");
            bool result = false;
            try
            {
                _logs?.DebugMsg_1($"CheckCertificateIsVaild x509Chain go");
                X509Chain x509Chain = new X509Chain();
                x509Chain.ChainPolicy.RevocationFlag = X509RevocationFlag.EntireChain;
                x509Chain.ChainPolicy.RevocationMode = X509RevocationMode.Online;
                x509Chain.ChainPolicy.UrlRetrievalTimeout = new TimeSpan(0, 1, 0);
                x509Chain.ChainPolicy.VerificationFlags = X509VerificationFlags.NoFlag;
                _logs?.DebugMsg_1($"CheckCertificateIsVaild x509Chain finish");
                _logs?.DebugMsg_1($"CheckCertificateIsVaild x509Chain.Build go");
                result = x509Chain.Build(certificate);
                _logs?.DebugMsg_1($"CheckCertificateIsVaild x509Chain.Build finish");
            }
            catch (Exception ex)
            {
                _logs?.DebugMsg_1("[CheckCertificateIsVaild] error: " + ex.Message);
            }
            _logs?.DebugMsg_1($"CheckCertificateIsVaild finish");
            return result;
        }
        private bool CheckIssuerAndSubject(X509Certificate2 certificate, X509Chain chain)
        {
            _logs?.DebugMsg_1($"CheckIssuerAndSubject start");
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
                        _logs?.DebugMsg_1($"[CheckIssuerAndSubject] certificate.Subject:{certificate.Subject} matched");
                    }
                }
                //[Dean 1119 remove Issuer check by Wendy's commit]
                //foreach (string iss in Issuer)
                //{
                //    if (ExtractCN(certificate.Issuer).Equals(iss))
                //    {
                //        isIssuerCNMatch = true;
                //    }
                //}
                var sanList = GetSubjectAlternativeNames(certificate);
                //_logs?.DebugMsg_1("Subject Alternative Names:");
                //_logs?.DebugMsg_1("---SAN---");
                foreach (var san in sanList)
                {
                    //_logs?.DebugMsg_1(san);
                    _logs?.DebugMsg_1($"CheckIssuerAndSubject ContainsAny go");
                    bool containsAny = ContainsAny(san, Subject);
                    _logs?.DebugMsg_1($"CheckIssuerAndSubject ContainsAny finish");
                    if (containsAny)
                    {
                        _logs?.DebugMsg_1("[CheckIssuerAndSubject] Subject is included in the SAN.");
                        isSANCNMatch = true;
                    }
                    else
                    {
                        _logs?.DebugMsg_1("[CheckIssuerAndSubject] Subject is NOT included in the SAN.");
                    }
                }
                _logs?.DebugMsg_1("---SAN END---");
                isCNMatch = isSubjectCNMatch && /*isIssuerCNMatch &&*/ isSANCNMatch;
                if (isCNMatch)
                {
                    _logs?.DebugMsg_1("[CheckIssuerAndSubject] cert info matched.");
                }
                else
                {
                    if (!isSubjectCNMatch)
                    {
                        _logs?.DebugMsg_1("[CheckIssuerAndSubject] Subject is NOT match.");
                    }
                    if (!isSANCNMatch)
                    {
                        _logs?.DebugMsg_1("[CheckIssuerAndSubject] SAN/CN is NOT match.");
                    }
                    //if (!isIssuerCNMatch)
                    //{
                    //_logs?.DebugMsg_1("[CheckIssuerAndSubject] Issuer is NOT match.");
                    //}

                    // Additional logic to handle proxy certificates if the above checks failed
                    /*                     
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
                    }*/
                    //[Dean] Use certificate verify to replace the usage like cert store
                    bool temp = true;
                    if (!IsValidCertificate(certificate, chain))
                    {
                        _logs?.DebugMsg_1("[CheckIssuerAndSubject][IsValidCertificate] cert/chain invalid.");
                        temp = false;
                    }
                    if (!certificate.Verify())
                    {
                        _logs?.DebugMsg_1("[CheckIssuerAndSubject] cert verify return fail.");
                        temp = false;
                    }
                    isCNMatch = temp;
                }
                //_logs?.DebugMsg_1("--------------CheckIssuerAndSubject------------------");
                _logs?.DebugMsg_1($"CheckIssuerAndSubject finish");
                return isCNMatch;
            }
            catch (Exception ex)
            {
                _logs?.DebugMsg_1("[CheckIssuerAndSubject] Error." + ex.ToString());
            }
            _logs?.DebugMsg_1($"CheckIssuerAndSubject finish");
            return false;
        }
        /*private bool ValidateProxyCertificate(StoreName storeName, StoreLocation storeLocation, X509Certificate2 certificate, X509Chain chain)
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
                //[Dean 1119 remove issuer check by Wendy's commit]
                //if (cert1.Certificate.Issuer == cert1.Certificate.Subject)
                //{
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
                //}
            }
            // Close the store
            store.Close();
            return trustedRootMatched; //proxy certitifacate is invalid
        }*/
        private string ExtractCN(string subject)
        {
            _logs?.DebugMsg_1($"ExtractCN start");
            if (string.IsNullOrEmpty(subject))
            {
                return null;
            }

            // Split the subject string into its components
            string[] subjectParts = subject.Split(',');

            foreach (string part in subjectParts)
            {
                _logs?.DebugMsg_1($"ExtractCN foreach go");
                // Trim and check if it starts with CN=
                string trimmedPart = part.Trim();
                if (trimmedPart.StartsWith("CN=", StringComparison.OrdinalIgnoreCase))
                {
                    _logs?.DebugMsg_1($"trimmedPart.Substring(3).Trim() : {trimmedPart.Substring(3).Trim()}");
                    // Return the value after CN=
                    _logs?.DebugMsg_1($"ExtractCN finish");
                    return trimmedPart.Substring(3).Trim();
                }
            }
            _logs?.DebugMsg_1($"ExtractCN finish");
            // CN not found
            return null;
        }
        private string[] GetSubjectAlternativeNames(X509Certificate2 certificate)
        {
            _logs?.DebugMsg_1($"GetSubjectAlternativeNames start");
            var sanList = new System.Collections.Generic.List<string>();

            foreach (var extension in certificate.Extensions)
            {
                if (extension is X509Extension x509Extension &&
                    x509Extension != null && 
                    x509Extension.Oid != null)
                {
                    // Subject Alternative Name (SAN) extension OID: 2.5.29.17                    
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
            _logs?.DebugMsg_1($"GetSubjectAlternativeNames sanList.Count : {sanList.Count}");
            _logs?.DebugMsg_1($"GetSubjectAlternativeNames finish");
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