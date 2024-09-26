using DDPM.SA.Common;
using DDPM.SA.Common.Method;
using DDPM.SA.Common.Security;
using DDPM.SA.Common.Settings;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.Common.Annotations;
using Dell.Client.Framework.Common.Extensions;
using Dell.Client.Framework.Common.PluginConditions;
using Dell.Client.Framework.Interfaces;
using Dell.Client.Framework.Security;
using Dell.Client.Framework.Security.Interfaces;
using Microsoft.Win32;
using Newtonsoft.Json;
using PInvoke;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Net.Http;
using System.Security;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Timers;
using static System.Reflection.Metadata.BlobBuilder;
using IDs = DDPM.SA.Common.IDs;
using JsonSerializer = System.Text.Json.JsonSerializer;
using Timer = System.Timers.Timer;

namespace MiniInstaller
{
    public class SWUpdatePlugins
    {
        #region Private Members

        private const string pluginName = "SWUpdatePlugin";
        private const string pluginVersion = "1.0.0";
        private const string pluginDescription = "This plugin implements SW Update Plugin.";
        private const string publisherCompany = "Wistron";
        private const string publisherWebsite = "https://www.wistron.com";
        private const string publisherSupport = "This plugin implements SW Update Plugin.";

        private IAgent _agent;

        #endregion Private Members

        public const string PluginLogId = "SWUpdate";

        //private Logs _logs;

        /// <summary>
        /// 現在正在進行下載或安裝流程的裝置資訊
        /// </summary>
        private SWUpdateInfo _SWUpdateInfo = new SWUpdateInfo();

        /// <summary>
        /// 給UI或是CLI的全部軟體更新包
        /// </summary>
        private SWUpdateInfoPackage _SWUpdateInfoPackage;

        /// <summary>
        /// 從SettingsManager取得的延遲更新包，用於比對是否延遲次數為0
        /// </summary>
        private SWUpdateInfoPackage _DelaySWUpdateInfoPackage;
        private Download? download = null;
        private Timer _downloadTimer = new Timer();

        private string _notificationStr = "";
        private SWUErrorCode _updateErrorCode;
        private string URL = $"https://clientperipherals.dell.com/DDPM/";
        private string URL_Folder = $"/Windows/Application/";
        private string TestURL_Folder = $"/ddpm/Application/";
        #region Events
        public event EventHandler<UpdateProgressInfo>? ProgressUpdate_Notify;

        #endregion Events

        public SWUpdatePlugins()
        {
            RegistryKey localKey64 = RegistryKey.OpenBaseKey(Microsoft.Win32.RegistryHive.LocalMachine, RegistryView.Registry64);
            URL = URL + URL_Folder;
            if (localKey64 != null)
            {
                RegistryKey registryKey = localKey64.OpenSubKey("SOFTWARE\\Dell\\DDPM Subagent\\", false);
                if (registryKey != null)
                {
                    var obj = registryKey?.GetValue("TestServerURL");
                    if (obj != null)
                    {
                        string s = obj.ToString();
                        if (!string.IsNullOrEmpty(s))
                        {
                            URL = obj + TestURL_Folder;
                        }
                    }
                }
            }
        }
        /// <summary>
        /// 檢查更新資訊
        /// </summary>
        /// <param name="updateHelper">IL的更新資訊</param>
        /// <param name="isShowNotify">是否顯示右下角通知圖示</param>
        /// <returns>回傳裝置資訊表(如果有需強制安裝更新的話，該裝置資訊表會被寫入對應裝置的安裝結果)</returns>
        public Task<List<SWUpdateInfo>> CheckUpdate()
        {
            Debug.WriteLine(nameof(CheckUpdate) + " start");
            _SWUpdateInfoPackage = new SWUpdateInfoPackage();
            _SWUpdateInfoPackage.TheLastCheckTime = DateTime.Now;
            SWUpdateHelper swUpdateHelper = GetSWMetadata();
            if (swUpdateHelper.Softwares != null && swUpdateHelper.Softwares.Count > 0)
            {
                for (int i = 0; i < swUpdateHelper.Softwares.Count; i++)
                {
                    SWUpdateInfo SWUpdateInfo = new SWUpdateInfo()
                    {
                        TheLatestVersion = Regex.Replace(Convert.ToInt32(swUpdateHelper.Softwares[i].SoftwareVersion).ToString("D4"), @"(.{1})(.{1})(.{1})(.{1})", "$1.$2.$3.$4"),
                        ServerPath = swUpdateHelper.Softwares[i].ServerPath,
                        SoftwareName = "DDPM",
                        FileSavepath = swUpdateHelper.Softwares[i].InstallPath
                    };
                    _SWUpdateInfoPackage.SWUpdateInfo.Add(SWUpdateInfo);
                }
                Debug.WriteLine(nameof(CheckUpdate) + " done.");
            }
            return Task.FromResult(new List<SWUpdateInfo>());
        }
        private SWUpdateHelper GetSWMetadata()
        {
            Debug.WriteLine(nameof(GetSWMetadata) + " start.");
            CertificateCheck certificateCheck = new CertificateCheck();
            if (!certificateCheck.CheckURLCACertificate(URL))
            {
                Debug.WriteLine(nameof(GetSWMetadata) + " URL CA check fail");
                return new SWUpdateHelper();
            }
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    client.Timeout = TimeSpan.FromSeconds(5);
                    HttpResponseMessage response = client.GetAsync(URL + "MetaData.json").Result;
                    response.EnsureSuccessStatusCode();
                    string jsonString = response.Content.ReadAsStringAsync().Result;
                    jsonString = jsonString.Replace("%1/", URL);
                    SWUpdateHelper data = JsonSerializer.Deserialize<SWUpdateHelper>(jsonString);
                    foreach (Software software in data.Softwares)
                    {
                        string version =
                        Regex.Replace(Convert.ToInt32(software.SoftwareVersion).ToString("D4"), @"(.{1})(.{1})(.{1})(.{1})", "$1.$2.$3.$4");
                        software.ServerPath = software.ServerPath.Replace("%2", $"{software.SoftwareName}-Setup_v{version}");
                    }
                    Debug.WriteLine(nameof(GetSWMetadata) + " done.");
                    return data;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(nameof(GetSWMetadata) + " error: " + ex.Message);
                }
            }
            return new SWUpdateHelper();
        }
        /// <summary>
        /// 從伺服端下載更新檔，下載後會接續執行安裝方法
        /// </summary>
        /// <param name="swUpdateInfos">更新的裝置資訊表</param>
        /// <returns>回傳裝置資訊表(在這個方法裡將原本傳入的裝置資訊表，再寫入對應裝置的下載安裝的結果碼)</returns>
        public Task<List<SWUpdateInfo>> DownloadAndInstall(List<SWUpdateInfo> swUpdateInfos, string installPath)
        {
            try
            {
                Debug.WriteLine(nameof(DownloadAndInstall) + " start");
                string saveFolderName = Guid.NewGuid().ToString();
                string savePath;
                CertificateCheck caCheck = new CertificateCheck();
                DDPMFileSecurity DDPMFileSecurity = new DDPMFileSecurity();
                if (string.IsNullOrEmpty(installPath))
                {
                    savePath = DDPMFileSecurity.GetActiveUserLocalAppDataPath() + "\\Dell\\Dell Display and Peripheral Manager" + "\\" + saveFolderName + "\\";
                }
                else
                {
                    savePath = installPath;
                }
                if (!Directory.Exists(savePath))
                {
                    Directory.CreateDirectory(savePath);
                }
                if (!CheckFold(savePath, out string folderInfo, out string pathSymbolicLinInfo))//0815 Bruce Add Security
                {
                    foreach (SWUpdateInfo swUpdateInfo in swUpdateInfos)
                    {
                        swUpdateInfo.SWUErrorCode = SWUErrorCode.FolderIsNotSafe;
                    }
                    Debug.WriteLine(nameof(DownloadAndInstall) + " FileIsNoSafe:" + folderInfo + "--or--" + pathSymbolicLinInfo);
                    return Task.FromResult(swUpdateInfos);
                }
                for (int i = 0; i < swUpdateInfos.Count; i++)
                {
                    Debug.WriteLine(swUpdateInfos[i].SoftwareName + nameof(DownloadAndInstall) + " start");
                    _notificationStr = "";
                    _SWUpdateInfo = swUpdateInfos[i];
                    _updateErrorCode = SWUErrorCode.Unknow;
                    swUpdateInfos[i].SWUErrorCode = _updateErrorCode;
                    string url = swUpdateInfos[i].ServerPath;
                    if (!CheckFold(savePath, out folderInfo, out pathSymbolicLinInfo))//0815 Bruce Add Security
                    {
                        swUpdateInfos[i].SWUErrorCode = SWUErrorCode.FolderIsNotSafe;
                        Debug.WriteLine(swUpdateInfos[i].SoftwareName + " FolderIsNotSafe:" + folderInfo + "--or--" + pathSymbolicLinInfo);
                        continue;
                    }
                    _downloadTimer = new Timer();
                    _downloadTimer.Interval = 1000;
                    _downloadTimer.Elapsed += new ElapsedEventHandler(DownloadTimer_Elapsed);
                    _downloadTimer.Start();
                    download = new Download();
                    string downloadInfo = "";
                    // 將儲存路徑與從 URL 中提取的檔案名稱組合
                    string _installationFileStoragePath = Path.Combine(savePath + Path.GetFileName(url));
                    bool downloadRet = download.DownloadFile(url, _installationFileStoragePath, out downloadInfo);
                    _downloadTimer.Stop();
                    if (!downloadRet)
                    {
                        if (downloadInfo.Equals("CA check fail"))
                        {
                            swUpdateInfos[i].SWUErrorCode = SWUErrorCode.CAFail;
                        }
                        else if (downloadInfo.Equals("Network fail"))
                        {
                            swUpdateInfos[i].SWUErrorCode = SWUErrorCode.NetworkDisconnection;
                        }
                        Debug.WriteLine(swUpdateInfos[i].SoftwareName + " Download File Fail");
                        continue;
                    }
                    string extractPath = Path.Combine(savePath + Path.GetFileName(url).Substring(0, Path.GetFileName(url).Length - 4));
                    if (!Directory.Exists(extractPath))
                    {
                        Directory.CreateDirectory(extractPath);
                    }
                    if (!CheckFold(extractPath, out folderInfo, out pathSymbolicLinInfo))//0815 Bruce Add Security
                    {
                        swUpdateInfos[i].SWUErrorCode = SWUErrorCode.FolderIsNotSafe;
                        Debug.WriteLine(swUpdateInfos[i].SoftwareName + " FolderIsNotSafe:" + folderInfo + "--or--" + pathSymbolicLinInfo);
                        continue;
                    }
                    if (!CheckSHA(swUpdateInfos[i].InstallPaths, out string FileCAInfo))
                    {
                        swUpdateInfos[i].SWUErrorCode = SWUErrorCode.FileCheckFail;
                        Debug.WriteLine(swUpdateInfos[i].SoftwareName + " File check fail. Ex:" + FileCAInfo);
                        _notificationStr = $"Software update unsuccessful.";
                        NotificationFWupdate("Error", _notificationStr);
                        continue;
                    }
                    string exeFilePath;
                    if (!Unzip(_installationFileStoragePath, extractPath, out exeFilePath))
                    {
                        Debug.WriteLine(_SWUpdateInfo.SoftwareName + " Unzip Faile");
                        _notificationStr = $"Software update unsuccessful.";
                        NotificationFWupdate("Error", _notificationStr);
                        continue;
                    }
                    swUpdateInfos[i].InstallPaths = exeFilePath;
                    swUpdateInfos[i].SWUErrorCode = Install(swUpdateInfos[i]);
                    if (swUpdateInfos[i].SWUErrorCode == SWUErrorCode.NoError)
                    {
                        NotificationFWupdate("SW info", _notificationStr);
                    }
                    else
                    {
                        NotificationFWupdate("Error", _notificationStr);
                    }
                }
                // 檢查資料夾是否存在
                if (!string.IsNullOrEmpty(savePath) && Directory.Exists(savePath))
                {
                    // 刪除資料夾及其所有內容
                    Directory.Delete(savePath, true);
                }
                Debug.WriteLine(nameof(DownloadAndInstall) + " done");
                return Task.FromResult(swUpdateInfos);
            }
            catch (Exception ex)
            {
                foreach (SWUpdateInfo deviceInfo in swUpdateInfos)
                {
                    deviceInfo.SWUErrorCode = SWUErrorCode.NetworkDisconnection;
                }
                _notificationStr = $"{_SWUpdateInfo.SoftwareName} Update failed due to network error. Try again.";
                NotificationFWupdate("Error", _notificationStr);
                Debug.WriteLine(nameof(DownloadAndInstall) + " Error：Update failed due to network error. Try again. ex:" + ex.Message); // 輸出錯誤訊息
                return Task.FromResult(swUpdateInfos);
            }
        }
        /// <summary>
        /// 下載進度回傳事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DownloadTimer_Elapsed(object? sender, ElapsedEventArgs e)
        {
            if (download != null)
            {
                UpdateProgressInfo fWUpdateInfo = new UpdateProgressInfo()
                {
                    DeviceName = _SWUpdateInfo.SoftwareName,
                    TheLatestVersion = _SWUpdateInfo.TheLatestVersion,
                    ProcessName = "Downloading",
                    ProcessProgress = download.GetProgress(),
                };
                sendMessageToEvent(fWUpdateInfo);
            }
        }
        /// <summary>
        /// 安裝下載好的更新檔
        /// </summary>
        private SWUErrorCode Install(SWUpdateInfo swUpdateInfo)
        {
            try
            {
                _SWUpdateInfo = swUpdateInfo;
                Debug.WriteLine(swUpdateInfo.SoftwareName + nameof(Install) + " start");
                if (!CheckFold(swUpdateInfo.InstallPaths, out string folderInfo, out string pathSymbolicLinInfo))//0815 Bruce Add Security
                {
                    Debug.WriteLine(swUpdateInfo.SoftwareName + " FileIsNoSafe:" + folderInfo + "--or--" + pathSymbolicLinInfo);
                    return SWUErrorCode.FileIsNoSafe;
                }
                // 要運行的安裝程式路徑和命令行參數
                string arguments = "/silent";
                Process _clientProcess = new Process();
                UpdateProgressInfo updateProgressInfo = new UpdateProgressInfo()
                {
                    DeviceName = _SWUpdateInfo.SoftwareName,
                    TheLatestVersion = _SWUpdateInfo.TheLatestVersion,
                    ProcessName = "Installing",
                    ProcessProgress = 0.0,
                };
                sendMessageToEvent(updateProgressInfo);
                RegEvent();

                var sessionId = Kernel32.WTSGetActiveConsoleSessionId();
                if (sessionId is Advapi32.InvalidSessionId) throw new InvalidOperationException($"Cannot get session id");
                IntPtr token = UserImpersonator.GetTokenFromSession(sessionId, systemUser: false);
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
                using (FileLock fileLock = new FileLock(swUpdateInfo.InstallPaths, PathCheckOption.None, lockNow: true))
                {
                    AclChecker aclChecker = new AclChecker();
                    if (aclChecker.ContainsUnprivilegedWriteAccess(fileLock))
                    {
                        throw new SecurityException($"File ACLs for {swUpdateInfo.InstallPaths} contained unprivileged write access for one or more identity");
                    }
                    /*暫時註解 因還沒有簽章
                    var result = verifier.Verify(fileLock);
                    if (result != Win32ErrorCodes.ERROR_SUCCESS)
                    {
                        throw new SecurityException($"Signature validation failed for {fwUpdateInfo.InstallPaths}! Received the following return code {result}");
                    }*/
                    UserImpersonator.RunAsUser(token, () =>
                    {
                        using (Process clientProcess = new Process())
                        {
                            _clientProcess = new Process();
                            _clientProcess.StartInfo.UseShellExecute = false;
                            _clientProcess.StartInfo.FileName = swUpdateInfo.InstallPaths;
                            _clientProcess.StartInfo.WorkingDirectory = Path.GetDirectoryName(_clientProcess.StartInfo.FileName);
                            _clientProcess.StartInfo.Arguments = arguments;
                            _clientProcess.Start();
                            _clientProcess.WaitForExit();
                        }
                    });
                }
                _updateErrorCode = SWUErrorCode.NoError;
                return _updateErrorCode;
            }
            catch (Exception ex)
            {
                _updateErrorCode = SWUErrorCode.Unknow;
                Debug.WriteLine(swUpdateInfo.SoftwareName + nameof(Install) + " Error:" + ex.ToString());
                _notificationStr = $"{_SWUpdateInfo.SoftwareName} Service not running. Try again.";
                return _updateErrorCode;
            }
        }
        private bool CheckFold(string path, out string folderInfo, out string pathSymbolicLinInfo)
        {
            folderInfo = "Error";
            pathSymbolicLinInfo = "Error";
            int count = 0;
            bool folderValid = false;
            do
            {
                folderInfo = string.Empty;
                pathSymbolicLinInfo = string.Empty;
                folderValid = false;
                folderValid = DDPMFileSecurity.SRemoveSymbolicFolder(path, out pathSymbolicLinInfo);//0924 Bruce Add Security
                if (!folderValid)
                {
                    Debug.WriteLine(nameof(DownloadAndInstall) + " FolderIsNotSafe:" + pathSymbolicLinInfo + " Retry:" + (count++));
                }
                folderValid = DDPMFileSecurity.IsFolderPathValid(path, out folderInfo) && folderValid;
                if (!folderValid)
                {
                    Debug.WriteLine(nameof(DownloadAndInstall) + " FolderIsNotSafe:" + folderInfo + " Retry:" + (count++));
                    Directory.Delete(path, true);
                    Directory.CreateDirectory(path);
                }
            } while (!folderValid && count < 2);
            return folderValid;
        }
        private bool CheckSHA(string filePath, out string fileCAInfo)
        {
            CertificateCheck certificateCheck = new CertificateCheck();
            bool isCheckSHA = false;
            fileCAInfo = "Error";
            if (!string.IsNullOrEmpty(_SWUpdateInfo.SHA512))
            {
                isCheckSHA = certificateCheck.CheckFile_SHA512(filePath, _SWUpdateInfo.SHA512, out fileCAInfo);
            }
            else
            {
                isCheckSHA = certificateCheck.CheckFile_SHA256(filePath, _SWUpdateInfo.SHA256, out fileCAInfo);
            }
            return isCheckSHA;
        }
        private bool Unzip(string filePath, string extractPath, out string exeFilePath)
        {
            _SWUpdateInfo.SWUErrorCode = SWUErrorCode.Unknow;
            bool ret = false;
            Unzip unzip = new Unzip();
            exeFilePath = "";
            if (unzip.CheckFileIsZip(filePath))
            {
                if (!unzip.ExecuteUnzip(filePath, extractPath, out exeFilePath))
                {
                    Debug.WriteLine(_SWUpdateInfo.SoftwareName + " Unzip Faile");
                }
                if (!string.IsNullOrEmpty(exeFilePath))
                {
                    CertificateCheck certificateCheck = new CertificateCheck();
                    if (!certificateCheck.CheckFile_Thumbprint(exeFilePath, _SWUpdateInfo.Thumbprint, out string FileCAInfo))
                    {
                        Debug.WriteLine(_SWUpdateInfo.SoftwareName + " File check fail. Ex:" + FileCAInfo);
                        _SWUpdateInfo.SWUErrorCode = SWUErrorCode.FileCheckFail;
                    }
                }
            }
            else
            {
                exeFilePath = filePath;
                ret = true;
            }
            return ret;
        }
        private void NotificationFWupdate(string title, string info)
        {
            Debug.WriteLine($"{title} Message:{info}");
        }
        ManagementEventWatcher watcher;
        private void RegEvent()
        {
            try
            {
                // 將反斜線進行正確轉義
                WqlEventQuery query = new WqlEventQuery(
                         "SELECT * FROM RegistryValueChangeEvent WHERE " +
                         "Hive = 'HKEY_LOCAL_MACHINE'" +
                         @"AND KeyPath = 'SOFTWARE\\Dell Display and Peripheral Manager' AND ValueName='NextProcess'");
                watcher = new ManagementEventWatcher(query);
                Debug.WriteLine("Waiting for an event...");
                watcher.EventArrived += new EventArrivedEventHandler(OnRegistryValueChanged);
                watcher.Start();
            }
            catch (ManagementException ex)
            {
                Debug.WriteLine($"ManagementException: {ex.Message}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exception: {ex.Message}");
            }
        }
        private void CancelRegEvent()
        {
            watcher.Stop();
            watcher.EventArrived -= new EventArrivedEventHandler(OnRegistryValueChanged);
        }
        private void OnRegistryValueChanged(object sender, EventArrivedEventArgs e)
        {
            RegistryKey localKey64 = RegistryKey.OpenBaseKey(Microsoft.Win32.RegistryHive.LocalMachine, RegistryView.Registry64);
            if (localKey64 != null)
            {
                RegistryKey registryKey = localKey64.OpenSubKey("SOFTWARE\\Dell Display and Peripheral Manager\\", false);
                if (registryKey != null)
                {
                    string curProcess = (registryKey.GetValue("Process")?.ToString());
                    string nextProcess = (registryKey.GetValue("NextProcess")?.ToString());
                    UpdateProgressInfo updateProgressInfo = new UpdateProgressInfo()
                    {
                        DeviceName = _SWUpdateInfo.SoftwareName,
                        TheLatestVersion = _SWUpdateInfo.TheLatestVersion,
                        ProcessName = "Installing",
                        ProcessProgress = nextProcess != null ? int.Parse(nextProcess) : 0.0,
                    };
                    sendMessageToEvent(updateProgressInfo);
                }
            }
        }
        private void sendMessageToEvent(UpdateProgressInfo fWUpdateInfo)
        {
            ProgressUpdate_Notify?.AsyncFireAndForget(this, fWUpdateInfo, System.Threading.CancellationToken.None);
            Debug.WriteLine("sendMessageToEvent" + " " + fWUpdateInfo.ProcessName + " " + fWUpdateInfo.ProcessProgress + " " + DateTime.Now);
        }
    }
}