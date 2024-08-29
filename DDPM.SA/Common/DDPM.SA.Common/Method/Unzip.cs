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
        private Logs _logs;

        public Unzip(Logs logs)
        {
            _logs = logs;
        }

        public bool ExecuteUnzip(string zipFilePath, string extractPath, out string exeFilePath)
        {
            try
            {
                _logs.DebugMsg_1(nameof(Unzip) + " start");

                VerifierOption myVerifierOptions = VerifierOption.FailOnNoErrorsAndSelfSignedCert;
                SubjectPublicKeyInfoHashes hashes = new SubjectPublicKeyInfoHashes(HashType.Sha256);
                var constraints = new LeafCertConstraints(hashes)
                {
                    RequireAllCerts = false
                };
                PeAuthenticodeVerifier verifier = new PeAuthenticodeVerifier(myVerifierOptions, omitDefaultOptions: true)
                {
                    Constraints = constraints
                };
                using (FileLock fileLock = new FileLock(zipFilePath, PathCheckOption.None, lockNow: true))
                {
                    AclChecker aclChecker = new AclChecker();
                    if (aclChecker.ContainsUnprivilegedWriteAccess(fileLock))
                    {
                        throw new SecurityException($"File ACLs for {zipFilePath} contained unprivileged write access for one or more identity");
                    }
                    /*暫時註解 因還沒有簽章
                    var result = verifier.Verify(fileLock);
                    if (result != Win32ErrorCodes.ERROR_SUCCESS)
                    {
                        throw new SecurityException($"Signature validation failed for {zipFilePath}! Received the following return code {result}");
                    }*/
                    // 解壓縮zip檔案，並覆蓋現有檔案
                    ZipFile.ExtractToDirectory(zipFilePath, extractPath, true);
                    _logs.DebugMsg_1(nameof(Unzip) + " done");
                    exeFilePath = GetExeFilePath(extractPath);
                }
                return true;
            }
            catch (Exception ex)
            {
                _logs.DebugMsg_1(nameof(Unzip) + "Unzip fail: " + ex.Message);
                exeFilePath = "";
                return false;
            }
        }

        private string GetExeFilePath(string directory)
        {
            // 列舉資料夾中的所有 .exe 檔案
            string[] exeFiles = Directory.GetFiles(directory, "*.exe");
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