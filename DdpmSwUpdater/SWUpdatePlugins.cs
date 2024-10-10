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
using VcpCore.Common;
using static System.Reflection.Metadata.BlobBuilder;
using IDs = DDPM.SA.Common.IDs;
using JsonSerializer = System.Text.Json.JsonSerializer;
using RegistryHive = DDPM.SA.Common.Settings.RegistryHive;
using Timer = System.Timers.Timer;

namespace DdpmSwUpdater
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

        }
        /// <summary>
        /// 從伺服端下載更新檔，下載後會接續執行安裝方法
        /// </summary>
        /// <param name="swUpdateInfos">更新的裝置資訊表</param>
        /// <returns>回傳裝置資訊表(在這個方法裡將原本傳入的裝置資訊表，再寫入對應裝置的下載安裝的結果碼)</returns>
        public Task<List<SWUpdateInfo>> DownloadAndInstall(string installPath)
        {
            bool isSkipCA = GetCheckCAStatus();
            LogManage.LogMessage(nameof(DownloadAndInstall) + " start");
            SWUpdateHelper swUpdateHelper = SWUpdateSetting.GetSWMetadata(isSkipCA, out string getMetadataInfo);
            LogManage.LogMessage($"GetMetadata {getMetadataInfo}");
            List<SWUpdateInfo> swUpdateInfos = new List<SWUpdateInfo>();
            if (swUpdateHelper.Softwares != null && swUpdateHelper.Softwares.Count > 0)
            {
                for (int i = 0; i < swUpdateHelper.Softwares.Count; i++)
                {
                    SWUpdateInfo SWUpdateInfo = new SWUpdateInfo()
                    {
                        TheLatestVersion = Regex.Replace(Convert.ToInt32(swUpdateHelper.Softwares[i].SoftwareVersion).ToString("D4"), @"(.{1})(.{1})(.{1})(.{1})", "$1.$2.$3.$4"),
                        ServerPath = swUpdateHelper.Softwares[i].ServerPath,
                        SoftwareName = "DDPM",
                        FileSavepath = swUpdateHelper.Softwares[i].InstallPath,
                        SHA256 = swUpdateHelper.Softwares[i].SHA256,
                        SHA512 = swUpdateHelper.Softwares[i].SHA512,
                        Thumbprint = swUpdateHelper.Softwares[i].Thumbprint
                    };
                    swUpdateInfos.Add(SWUpdateInfo);
                }
            }
            else
            {
                swUpdateInfos.Add(new SWUpdateInfo()
                {
                    SoftwareName = "DDPM",
                    SWUErrorCode = SWUErrorCode.FileCheckFail
                });
                return Task.FromResult(swUpdateInfos);
            }
            LogManage.LogMessage($"swUpdateInfos ok");
            try
            {
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
                LogManage.LogMessage($"{savePath}");
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
                    LogManage.LogMessage(nameof(DownloadAndInstall) + " FileIsNoSafe:" + folderInfo + "--or--" + pathSymbolicLinInfo);
                    return Task.FromResult(swUpdateInfos);
                }
                LogManage.LogMessage($"CheckFold ok");
                LogManage.LogMessage($"swUpdateInfos.Count {swUpdateInfos.Count}");
                for (int i = 0; i < swUpdateInfos.Count; i++)
                {
                    LogManage.LogMessage(swUpdateInfos[i].SoftwareName + nameof(DownloadAndInstall) + " start");
                    _notificationStr = "";
                    _SWUpdateInfo = swUpdateInfos[i];
                    _updateErrorCode = SWUErrorCode.Unknow;
                    swUpdateInfos[i].SWUErrorCode = _updateErrorCode;
                    string url = swUpdateInfos[i].ServerPath;
                    if (!CheckFold(savePath, out folderInfo, out pathSymbolicLinInfo))//0815 Bruce Add Security
                    {
                        swUpdateInfos[i].SWUErrorCode = SWUErrorCode.FolderIsNotSafe;
                        LogManage.LogMessage(swUpdateInfos[i].SoftwareName + " FolderIsNotSafe:" + folderInfo + "--or--" + pathSymbolicLinInfo);
                        continue;
                    }
                    LogManage.LogMessage($"CheckFold2 ok");
                    _downloadTimer = new Timer();
                    _downloadTimer.Interval = 1000;
                    _downloadTimer.Elapsed += new ElapsedEventHandler(DownloadTimer_Elapsed);
                    _downloadTimer.Start();
                    download = new Download();
                    string downloadInfo = "";
                    // 將儲存路徑與從 URL 中提取的檔案名稱組合
                    string _installationFileStoragePath = Path.Combine(savePath + Path.GetFileName(url));
                    bool downloadRet = download.DownloadFile(url, _installationFileStoragePath, out downloadInfo, isSkipCA);
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
                        LogManage.LogMessage(swUpdateInfos[i].SoftwareName + " Download File Fail");
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
                        LogManage.LogMessage(swUpdateInfos[i].SoftwareName + " FolderIsNotSafe:" + folderInfo + "--or--" + pathSymbolicLinInfo);
                        continue;
                    }
                    string exeFilePath;
                    if (!Unzip(_installationFileStoragePath, extractPath, out exeFilePath))
                    {
                        LogManage.LogMessage(_SWUpdateInfo.SoftwareName + " Unzip Faile");
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
                LogManage.LogMessage(nameof(DownloadAndInstall) + " done");
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
                LogManage.LogMessage(nameof(DownloadAndInstall) + " Error：Update failed due to network error. Try again. ex:" + ex.Message); // 輸出錯誤訊息
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
        public SWUErrorCode Install(SWUpdateInfo swUpdateInfo)
        {
            try
            {
                _SWUpdateInfo = swUpdateInfo;
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
                //var sessionId = Kernel32.WTSGetActiveConsoleSessionId();
                //if (sessionId is Advapi32.InvalidSessionId) throw new InvalidOperationException($"Cannot get session id");
                //IntPtr token = UserImpersonator.GetTokenFromSession(sessionId, systemUser: false);
                //using (FileLock fileLock = new FileLock(swUpdateInfo.InstallPaths, PathCheckOption.None, lockNow: true))
                //{
                //    UserImpersonator.RunAsUser(token, () =>
                //    {
                //        using (Process clientProcess = new Process())
                //        {
                //            _clientProcess = new Process();
                //            _clientProcess.StartInfo.UseShellExecute = false;
                //            _clientProcess.StartInfo.FileName = swUpdateInfo.InstallPaths;
                //            _clientProcess.StartInfo.WorkingDirectory = Path.GetDirectoryName(_clientProcess.StartInfo.FileName);
                //            _clientProcess.StartInfo.Arguments = arguments;
                //            _clientProcess.Start();
                //            _clientProcess.WaitForExit();
                //        }
                //    });
                //}
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
                _updateErrorCode = SWUErrorCode.NoError;
                return _updateErrorCode;
            }
            catch (Exception ex)
            {
                _updateErrorCode = SWUErrorCode.Unknow;
                LogManage.LogMessage(swUpdateInfo.SoftwareName + nameof(Install) + " Error:" + ex.ToString());
                _notificationStr = $"{_SWUpdateInfo.SoftwareName} Service not running. Try again.";
                return _updateErrorCode;
            }
        }
        private bool GetCheckCAStatus()
        {
            bool isSkipCA = false;
            object o = DDPMRegistryHelper.ReadRegistryKey(RegistryHive.LocalMachine, "SOFTWARE\\Dell\\DDPM Subagent", "SkipCA");
            if (o != null && o is string && !string.IsNullOrEmpty(o.ToString()))
            {
                isSkipCA = o.ToString().Equals("1") ? true : false;
            }
            return isSkipCA;
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
                    LogManage.LogMessage(nameof(CheckFold) + " FolderSymbolicFolderIsNotSafe:" + pathSymbolicLinInfo + " Retry:" + (count++));
                }
                folderValid = DDPMFileSecurity.IsFolderPathValid(path, out folderInfo) && folderValid;
                if (!folderValid)
                {
                    LogManage.LogMessage(nameof(CheckFold) + " FolderIsNotSafe:" + folderInfo + " Retry:" + (count++));
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
            LogManage.LogMessage($"{_SWUpdateInfo.SoftwareName} {nameof(Unzip)} Start");
            _SWUpdateInfo.SWUErrorCode = SWUErrorCode.Unknow;
            bool ret = false;
            Unzip unzip = new Unzip();
            exeFilePath = "";
            string FileCAInfo = "Pass";
            if (unzip.CheckFileIsZip(filePath))
            {
                LogManage.LogMessage($"{_SWUpdateInfo.SoftwareName} File is zip.");
                LogManage.LogMessage($"{_SWUpdateInfo.SoftwareName} check SHA start.");
                if (CheckSHA(filePath, out FileCAInfo))
                {
                    LogManage.LogMessage($"{_SWUpdateInfo.SoftwareName} ExecuteUnzip start.");
                    if (unzip.ExecuteUnzip(filePath, extractPath, out exeFilePath))
                    {
                        if (!string.IsNullOrEmpty(exeFilePath))
                        {
                            LogManage.LogMessage($"{_SWUpdateInfo.SoftwareName} check Thumbprint start.");
                            CertificateCheck certificateCheck = new CertificateCheck();
                            if (certificateCheck.CheckFile_Thumbprint(exeFilePath, _SWUpdateInfo.Thumbprint, out FileCAInfo))
                            {
                                ret = true;
                                LogManage.LogMessage($"{_SWUpdateInfo.SoftwareName} check done.");
                            }
                            else
                            {
                                _SWUpdateInfo.SWUErrorCode = SWUErrorCode.FileCheckFail;
                                LogManage.LogMessage($"{_SWUpdateInfo.SoftwareName} File check Thumbprint fail. Ex: {FileCAInfo}");
                            }
                        }
                    }
                    else
                    {
                        LogManage.LogMessage($"{_SWUpdateInfo.SoftwareName} Unzip Faile");
                    }
                }
                else
                {
                    _SWUpdateInfo.SWUErrorCode = SWUErrorCode.FileCheckFail;
                    LogManage.LogMessage($"{_SWUpdateInfo.SoftwareName} File check SHA fail. Ex: {FileCAInfo}");
                }
            }
            else
            {
                LogManage.LogMessage($"{_SWUpdateInfo.SoftwareName} File is exe.");
                LogManage.LogMessage($"{_SWUpdateInfo.SoftwareName} check SHA start.");
                if (CheckSHA(filePath, out FileCAInfo))
                {
                    LogManage.LogMessage($"{_SWUpdateInfo.SoftwareName} check Thumbprint start.");
                    CertificateCheck certificateCheck = new CertificateCheck();
                    if (certificateCheck.CheckFile_Thumbprint(filePath, _SWUpdateInfo.Thumbprint, out FileCAInfo))
                    {
                        ret = true;
                        LogManage.LogMessage($"{_SWUpdateInfo.SoftwareName} check done.");
                    }
                    else
                    {
                        _SWUpdateInfo.SWUErrorCode = SWUErrorCode.FileCheckFail;
                        LogManage.LogMessage($"{_SWUpdateInfo.SoftwareName} File check Thumbprint fail. Ex: {FileCAInfo}");
                    }
                }
                else
                {
                    _SWUpdateInfo.SWUErrorCode = SWUErrorCode.FileCheckFail;
                    LogManage.LogMessage($"{_SWUpdateInfo.SoftwareName} File check SHA fail. Ex: {FileCAInfo}");
                }
                exeFilePath = filePath;
            }
            LogManage.LogMessage($"{_SWUpdateInfo.SoftwareName} {nameof(Unzip)} done");
            return ret;
        }
        private void NotificationFWupdate(string title, string info)
        {
            LogManage.LogMessage($"{title} Message:{info}");
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
                LogManage.LogMessage("Waiting for an event...");
                watcher.EventArrived += new EventArrivedEventHandler(OnRegistryValueChanged);
                watcher.Start();
            }
            catch (ManagementException ex)
            {
                LogManage.LogMessage($"ManagementException: {ex.Message}");
            }
            catch (Exception ex)
            {
                LogManage.LogMessage($"Exception: {ex.Message}");
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
            LogManage.LogMessage("sendMessageToEvent" + " " + fWUpdateInfo.ProcessName + " " + fWUpdateInfo.ProcessProgress + " " + DateTime.Now);
        }
    }
}