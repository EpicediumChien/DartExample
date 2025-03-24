using DDPM.SA.Common;
using DDPM.SA.Common.Method;
using DDPM.SA.Common.Security;
using DDPM.SA.Common.Settings;
using DDPM.SA.Obfuscation;
using DDPM.SA.Resources.Helper;
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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Management;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Security;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Timers;
using VcpCore.Common;
using Windows.ApplicationModel.Background;
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
        private bool _bFirstInstance;
        private Mutex? _instanceMutex;
        private string? _applicationName;
        bool _SkipSHA = false;
        int _CurrentProcess = 0;
        int _CurrentProcessLimit = 10;
        Timer _processTimer = new Timer();
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
        public Task<List<SWUpdateInfo>> DownloadAndInstall(string installPath, SWUpdateHelper swUpdateHelper, bool isSkipCA, bool skipSHA)
        {
            _SkipSHA = skipSHA;
            LogManage.LogMessage(nameof(DownloadAndInstall) + " start");
            List<SWUpdateInfo> swUpdateInfos = new List<SWUpdateInfo>();
            if (swUpdateHelper.Softwares != null && swUpdateHelper.Softwares.Count > 0)
            {
                for (int i = 0; i < swUpdateHelper.Softwares.Count; i++)
                {
                    SWUpdateInfo SWUpdateInfo = new SWUpdateInfo()
                    {
                        TheLatestVersion = swUpdateHelper.Softwares[i].SoftwareVersion,
                        ServerPath = swUpdateHelper.Softwares[i].ServerPath,
                        SoftwareName = "DDPM",
                        FileSavepath = swUpdateHelper.Softwares[i].InstallPath,
                        SHA256 = swUpdateHelper.Softwares[i].SHA256,
                        SHA512 = swUpdateHelper.Softwares[i].SHA512,
                        Thumbprint = swUpdateHelper.Softwares[i].Thumbprint
                    };
                    LogManage.Version = SWUpdateInfo.TheLatestVersion;
                    swUpdateInfos.Add(SWUpdateInfo);
                    UpdateProgressInfo fWUpdateInfo = new UpdateProgressInfo()
                    {
                        DeviceName = SWUpdateInfo.SoftwareName,
                        TheLatestVersion = SWUpdateInfo.TheLatestVersion,
                        ProcessName = "Downloading",
                        ProcessProgress = 0.0,
                    };
                    sendMessageToEvent(fWUpdateInfo);
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
            LogManage.LogMessage($"swUpdateInfos.Count {swUpdateInfos.Count}");
            foreach (SWUpdateInfo swUpdateInfo in swUpdateInfos)
            {
                string appName = Path.GetFileName(swUpdateInfo.ServerPath);
                LogManage.LogMessage($"appName : {appName}");
                _instanceMutex = new Mutex(false, appName, out _bFirstInstance);
                if (!_bFirstInstance)
                {
                    LogManage.LogMessage($"The program is already running and a new instance cannot be started");
                    swUpdateInfo.SWUErrorCode = SWUErrorCode.ServiceNotRunning;
                    return Task.FromResult(swUpdateInfos);
                }
            }
            LogManage.LogMessage($"_instanceMutex?.Dispose() go");
            _instanceMutex?.Dispose();
            LogManage.LogMessage($"_instanceMutex?.Dispose() done");
            Method method = new Method(LogManage.Logs);
            try
            {
                string saveFolderName = Guid.NewGuid().ToString();
                string path_programdata = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
                string savePath;
                CertificateCheck caCheck = new CertificateCheck(LogManage.Logs);
                DDPMFileSecurity DDPMFileSecurity = new DDPMFileSecurity();
                LogManage.LogMessage($"Initialize download path start");
                if (string.IsNullOrEmpty(installPath))
                {
                    if (!string.IsNullOrEmpty(path_programdata))
                    {
                        if (LogManage.FromDDPM)
                        {
                            savePath = path_programdata + "\\Dell\\Dell Display and Peripheral Manager" + "\\" + saveFolderName + "\\";
                        }
                        else
                        {
                            savePath = path_programdata + "\\Dell" + "\\" + saveFolderName + "\\";
                        }
                    }
                    else
                    {
                        foreach (SWUpdateInfo swUpdateInfo in swUpdateInfos)
                        {
                            swUpdateInfo.SWUErrorCode = SWUErrorCode.FileCheckFail;
                        }
                        LogManage.LogMessage(nameof(DownloadAndInstall) + " path_programdata is can not get");
                        return Task.FromResult(swUpdateInfos);
                    }
                }
                else
                {
                    savePath = installPath;
                }
                if (!Directory.Exists(savePath))
                {
                    Directory.CreateDirectory(savePath);
                }
                LogManage.LogMessage($"Initialize download path done");
                LogManage.LogMessage($"CheckFold1 start");
                if (!CheckFold(savePath, out string folderInfo, out string pathSymbolicLinInfo))//0815 Bruce Add Security
                {
                    foreach (SWUpdateInfo swUpdateInfo in swUpdateInfos)
                    {
                        swUpdateInfo.SWUErrorCode = SWUErrorCode.FolderIsNotSafe;
                    }
                    LogManage.LogMessage(nameof(DownloadAndInstall) + " FileIsNoSafe:" + folderInfo + "--or--" + pathSymbolicLinInfo);
                    method.DeleteFolder(savePath);
                    return Task.FromResult(swUpdateInfos);
                }
                LogManage.LogMessage($"CheckFold ok");
                for (int i = 0; i < swUpdateInfos.Count; i++)
                {
                    LogManage.LogMessage(swUpdateInfos[i].SoftwareName + nameof(DownloadAndInstall) + " start");
                    _notificationStr = "";
                    _SWUpdateInfo = swUpdateInfos[i];
                    _updateErrorCode = SWUErrorCode.Unknow;
                    swUpdateInfos[i].SWUErrorCode = _updateErrorCode;
                    string url = swUpdateInfos[i].ServerPath;
                    LogManage.LogMessage($"CheckFold2 start");
                    if (!CheckFold(savePath, out folderInfo, out pathSymbolicLinInfo))//0815 Bruce Add Security
                    {
                        swUpdateInfos[i].SWUErrorCode = SWUErrorCode.FolderIsNotSafe;
                        LogManage.LogMessage(swUpdateInfos[i].SoftwareName + " FolderIsNotSafe:" + folderInfo + "--or--" + pathSymbolicLinInfo);
                        method.DeleteFolder(savePath);
                        continue;
                    }
                    LogManage.LogMessage($"CheckFold2 ok");
                    if (!NetworkInterface.GetIsNetworkAvailable())
                    {
                        swUpdateInfos[i].SWUErrorCode = SWUErrorCode.NetworkDisconnection;
                        _notificationStr = $"{swUpdateInfos[i].SoftwareName} {LangHelper.Instance["Update_failed_due_to_network_error"]}";
                        NotificationFWupdate(LangHelper.Instance["Error"], _notificationStr);
                        LogManage.LogMessage(swUpdateInfos[i].SoftwareName + " Download File Fail : " + _notificationStr);
                        method.DeleteFolder(savePath);
                        continue;
                    }
                    string _installationFileStoragePath = string.Empty;
                    try
                    {
                        _downloadTimer = new Timer();
                        _downloadTimer.Interval = 1000;
                        _downloadTimer.Elapsed += new ElapsedEventHandler(DownloadTimer_Elapsed);
                        int count = 0;
                        do
                        {
                            swUpdateInfos[i].SWUErrorCode = SWUErrorCode.Unknow;
                            LogManage.LogMessage($"Download start try count : {count++}");
                            _downloadTimer.Start();
                            download = new Download(LogManage.Logs);
                            string downloadInfo = "";
                            // 將儲存路徑與從 URL 中提取的檔案名稱組合
                            _installationFileStoragePath = Path.Combine(savePath + Path.GetFileName(url));
                            bool downloadRet = download.DownloadFile(url, _installationFileStoragePath, out downloadInfo, isSkipCA);
                            _downloadTimer.Stop();
                            LogManage.LogMessage($"Download done");
                            if (!downloadRet)
                            {
                                if (downloadInfo.Equals("CA check fail"))
                                {
                                    swUpdateInfos[i].SWUErrorCode = SWUErrorCode.CAFail;
                                }
                                else
                                {
                                    swUpdateInfos[i].SWUErrorCode = SWUErrorCode.NetworkDisconnection;
                                }
                                LogManage.LogMessage($"{swUpdateInfos[i].SoftwareName} Download File Fail : {downloadInfo}");
                                //continue;
                            }
                        } while (swUpdateInfos[i].SWUErrorCode == SWUErrorCode.CAFail && count < 3);
                        if (swUpdateInfos[i].SWUErrorCode == SWUErrorCode.CAFail || string.IsNullOrEmpty(_installationFileStoragePath))
                        {
                            LogManage.LogMessage($"{swUpdateInfos[i].SoftwareName} Download File Fail retry 3 count");
                            method.DeleteFolder(savePath);
                            continue;
                        }
                        if (swUpdateInfos[i].SWUErrorCode == SWUErrorCode.NetworkDisconnection || string.IsNullOrEmpty(_installationFileStoragePath))
                        {
                            LogManage.LogMessage($"{swUpdateInfos[i].SoftwareName} Download File Fail NetworkDisconnection");
                            method.DeleteFolder(savePath);
                            continue;
                        }
                    }
                    catch (Exception ex)
                    {
                        if (_downloadTimer != null)
                        {
                            _downloadTimer.Elapsed -= new ElapsedEventHandler(DownloadTimer_Elapsed);
                            _downloadTimer.Stop();
                            _downloadTimer = null;
                        }
                        LogManage.LogMessage(swUpdateInfos[i].SoftwareName + " Download error : " + ex.Message);
                        swUpdateInfos[i].SWUErrorCode = SWUErrorCode.NetworkDisconnection;
                        _notificationStr = $"{swUpdateInfos[i].SoftwareName} {LangHelper.Instance["Update_failed_due_to_network_error"]}";
                        NotificationFWupdate(LangHelper.Instance["Error"], _notificationStr);
                        method.DeleteFolder(savePath);
                        continue;
                    }
                    LogManage.LogMessage($"Creat extractPath");
                    string extractPath = Path.Combine(savePath + Path.GetFileName(url).Substring(0, Path.GetFileName(url).Length - 4));
                    if (!Directory.Exists(extractPath))
                    {
                        Directory.CreateDirectory(extractPath);
                    }
                    LogManage.LogMessage($"CheckFold3 start");
                    if (!CheckFold(extractPath, out folderInfo, out pathSymbolicLinInfo))//0815 Bruce Add Security
                    {
                        swUpdateInfos[i].SWUErrorCode = SWUErrorCode.FolderIsNotSafe;
                        LogManage.LogMessage(swUpdateInfos[i].SoftwareName + " FolderIsNotSafe:" + folderInfo + "--or--" + pathSymbolicLinInfo);
                        method.DeleteFolder(savePath);
                        continue;
                    }
                    LogManage.LogMessage($"CheckFold3 ok");
                    try
                    {
                        using (FileLock fileLock = new FileLock(_installationFileStoragePath, PathCheckOption.None, lockNow: true))
                        {
                            string exeFilePath;
                            LogManage.LogMessage($"Unzip start");
                            if (!Unzip(_installationFileStoragePath, extractPath, out exeFilePath))
                            {
                                LogManage.LogMessage(_SWUpdateInfo.SoftwareName + " Unzip Faile");
                                _notificationStr = LangHelper.Instance["Software_update_unsuccessful"];
                                NotificationFWupdate(LangHelper.Instance["Error"], _notificationStr);
                                method.DeleteFolder(savePath);
                                continue;
                            }
                            LogManage.LogMessage($"Unzip done");
                            LogManage.LogMessage($"Install start");
                            using (FileLock fileLock_2 = new FileLock(exeFilePath, PathCheckOption.None, lockNow: true))
                            {
                                if (!CheckThumbprint(exeFilePath, _SWUpdateInfo.Thumbprint, out string FileCAInfo))
                                {
                                    _SWUpdateInfo.SWUErrorCode = SWUErrorCode.FileCheckFail;
                                    LogManage.LogMessage($"{_SWUpdateInfo.SoftwareName} CheckThumbprint Faile");
                                    _notificationStr = LangHelper.Instance["Software_update_unsuccessful"];
                                    NotificationFWupdate(LangHelper.Instance["Error"], _notificationStr);
                                    method.DeleteFolder(savePath);
                                    continue;
                                }
                                swUpdateInfos[i].InstallPaths = exeFilePath;
                                swUpdateInfos[i].SWUErrorCode = Install(swUpdateInfos[i]);
                            }
                            LogManage.LogMessage($"Install done");
                            if (swUpdateInfos[i].SWUErrorCode == SWUErrorCode.NoError)
                            {
                                NotificationFWupdate(LangHelper.Instance["SW_info"], _notificationStr);
                            }
                            else
                            {
                                NotificationFWupdate(LangHelper.Instance["Error"], _notificationStr);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        LogManage.LogMessage($"{_SWUpdateInfo.SoftwareName} FileLock Error: {ex.Message}");
                    }
                }
                method.DeleteFolder(savePath);
                method.Dispose();
                LogManage.LogMessage(nameof(DownloadAndInstall) + " done");
                return Task.FromResult(swUpdateInfos);
            }
            catch (Exception ex)
            {
                method.Dispose();
                foreach (SWUpdateInfo deviceInfo in swUpdateInfos)
                {
                    deviceInfo.SWUErrorCode = SWUErrorCode.NetworkDisconnection;
                }
                _notificationStr = LangHelper.Instance["Update_failed_due_to_network_error"];
                NotificationFWupdate(LangHelper.Instance["Error"], _notificationStr);
                LogManage.LogMessage(nameof(DownloadAndInstall) + " Error：" + ex.Message); // 輸出錯誤訊息
                return Task.FromResult(swUpdateInfos);
            }
        }
        /// <summary>
        /// 從伺服端下載更新檔，下載後會接續執行安裝方法
        /// </summary>
        /// <param name="swUpdateInfos">更新的裝置資訊表</param>
        /// <returns>回傳裝置資訊表(在這個方法裡將原本傳入的裝置資訊表，再寫入對應裝置的下載安裝的結果碼)</returns>
        public bool DownloadAndExecutionSwUpdater(SWUpdateHelper swUpdateHelper, bool isSkipCA, bool skipSHA)
        {
            List<SWUpdateInfo> swUpdateInfos = new List<SWUpdateInfo>();
            _SkipSHA = skipSHA;
            bool ret = false;
            if (swUpdateHelper.Softwares != null && swUpdateHelper.Softwares.Count > 0)
            {
                for (int i = 0; i < swUpdateHelper.Softwares.Count; i++)
                {
                    SWUpdateInfo SWUpdateInfo = new SWUpdateInfo()
                    {
                        TheLatestVersion = swUpdateHelper.Softwares[i].SoftwareVersion,
                        ServerPath = swUpdateHelper.Softwares[i].DdpmSwUpdaterServer_path,
                        SoftwareName = "DDPM",
                        FileSavepath = swUpdateHelper.Softwares[i].InstallPath,
                        SHA256 = swUpdateHelper.Softwares[i].DdpmSwUpdater_SHA256,
                        SHA512 = swUpdateHelper.Softwares[i].DdpmSwUpdater_SHA512,
                        Thumbprint = swUpdateHelper.Softwares[i].DdpmSwUpdater_Thumbprint
                    };
                    LogManage.Version = SWUpdateInfo.TheLatestVersion;
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
                return ret;
            }
            LogManage.LogMessage($"swUpdateInfos ok");
            LogManage.LogMessage($"swUpdateInfos.Count {swUpdateInfos.Count}");
            foreach (SWUpdateInfo swUpdateInfo in swUpdateInfos)
            {
                string appName = Path.GetFileName(swUpdateInfo.ServerPath);
                LogManage.LogMessage($"appName : {appName}");
                _instanceMutex = new Mutex(false, appName, out _bFirstInstance);
                if (!_bFirstInstance)
                {
                    LogManage.LogMessage($"The program is already running and a new instance cannot be started");
                    swUpdateInfo.SWUErrorCode = SWUErrorCode.ServiceNotRunning;
                    return ret;
                }
            }
            LogManage.LogMessage($"_instanceMutex?.Dispose() go");
            _instanceMutex?.Dispose();
            LogManage.LogMessage($"_instanceMutex?.Dispose() done");
            Method method = new Method(LogManage.Logs);
            try
            {
                string saveFolderName = Guid.NewGuid().ToString();
                string path_programdata = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
                string savePath;
                CertificateCheck caCheck = new CertificateCheck(LogManage.Logs);
                DDPMFileSecurity DDPMFileSecurity = new DDPMFileSecurity();
                LogManage.LogMessage($"Initialize download path start");
                if (!string.IsNullOrEmpty(path_programdata))
                {
                    if (LogManage.FromDDPM)
                    {
                        savePath = path_programdata + "\\Dell\\Dell Display and Peripheral Manager" + "\\" + saveFolderName + "\\";
                    }
                    else
                    {
                        savePath = path_programdata + "\\Dell" + "\\" + saveFolderName + "\\";
                    }
                    if (!Directory.Exists(savePath))
                    {
                        Directory.CreateDirectory(savePath);
                    }
                }
                else
                {
                    foreach (SWUpdateInfo swUpdateInfo in swUpdateInfos)
                    {
                        swUpdateInfo.SWUErrorCode = SWUErrorCode.FileCheckFail;
                    }
                    LogManage.LogMessage(nameof(DownloadAndExecutionSwUpdater) + " path_programdata is can not get");
                    return ret;
                }
                LogManage.LogMessage($"Initialize download path done");
                LogManage.LogMessage($"CheckFold1 start");
                if (!CheckFold(savePath, out string folderInfo, out string pathSymbolicLinInfo))//0815 Bruce Add Security
                {
                    foreach (SWUpdateInfo swUpdateInfo in swUpdateInfos)
                    {
                        swUpdateInfo.SWUErrorCode = SWUErrorCode.FolderIsNotSafe;
                    }
                    LogManage.LogMessage(nameof(DownloadAndExecutionSwUpdater) + " FileIsNoSafe:" + folderInfo + "--or--" + pathSymbolicLinInfo);
                    method.DeleteFolder(savePath);
                    return ret;
                }
                LogManage.LogMessage($"CheckFold ok");
                for (int i = 0; i < swUpdateInfos.Count; i++)
                {
                    LogManage.LogMessage(swUpdateInfos[i].SoftwareName + nameof(DownloadAndExecutionSwUpdater) + " start");
                    _notificationStr = "";
                    _SWUpdateInfo = swUpdateInfos[i];
                    _updateErrorCode = SWUErrorCode.Unknow;
                    swUpdateInfos[i].SWUErrorCode = _updateErrorCode;
                    string url = swUpdateInfos[i].ServerPath;
                    LogManage.LogMessage($"CheckFold2 start");
                    if (!CheckFold(savePath, out folderInfo, out pathSymbolicLinInfo))//0815 Bruce Add Security
                    {
                        swUpdateInfos[i].SWUErrorCode = SWUErrorCode.FolderIsNotSafe;
                        LogManage.LogMessage(swUpdateInfos[i].SoftwareName + " FolderIsNotSafe:" + folderInfo + "--or--" + pathSymbolicLinInfo);
                        method.DeleteFolder(savePath);
                        continue;
                    }
                    LogManage.LogMessage($"CheckFold2 ok");
                    _downloadTimer = new Timer();
                    _downloadTimer.Interval = 1000;
                    _downloadTimer.Elapsed += new ElapsedEventHandler(DownloadTimer_Elapsed);
                    int count = 0;
                    string _installationFileStoragePath = string.Empty;
                    do
                    {
                        swUpdateInfos[i].SWUErrorCode = SWUErrorCode.Unknow;
                        LogManage.LogMessage($"Download start try count : {count++}");
                        _downloadTimer.Start();
                        download = new Download(LogManage.Logs);
                        string downloadInfo = "";
                        // 將儲存路徑與從 URL 中提取的檔案名稱組合
                        _installationFileStoragePath = Path.Combine(savePath + Path.GetFileName(url));
                        bool downloadRet = download.DownloadFile(url, _installationFileStoragePath, out downloadInfo, isSkipCA);
                        _downloadTimer.Stop();
                        LogManage.LogMessage($"Download done");
                        if (!downloadRet)
                        {
                            if (downloadInfo.Equals("CA check fail"))
                            {
                                swUpdateInfos[i].SWUErrorCode = SWUErrorCode.CAFail;
                            }
                            else
                            {
                                swUpdateInfos[i].SWUErrorCode = SWUErrorCode.NetworkDisconnection;
                            }
                            LogManage.LogMessage($"{swUpdateInfos[i].SoftwareName} Download File Fail : {downloadInfo}");
                        }
                    } while (swUpdateInfos[i].SWUErrorCode == SWUErrorCode.CAFail && count < 3);
                    if (swUpdateInfos[i].SWUErrorCode == SWUErrorCode.CAFail || string.IsNullOrEmpty(_installationFileStoragePath))
                    {
                        LogManage.LogMessage($"{swUpdateInfos[i].SoftwareName} Download File Fail retry 3 count");
                        method.DeleteFolder(savePath);
                        continue;
                    }
                    if (swUpdateInfos[i].SWUErrorCode == SWUErrorCode.NetworkDisconnection || string.IsNullOrEmpty(_installationFileStoragePath))
                    {
                        LogManage.LogMessage($"{swUpdateInfos[i].SoftwareName} Download File Fail NetworkDisconnection");
                        method.DeleteFolder(savePath);
                        continue;
                    }
                    LogManage.LogMessage($"Creat extractPath");
                    string extractPath = Path.Combine(savePath + Path.GetFileName(url).Substring(0, Path.GetFileName(url).Length - 4));
                    if (!Directory.Exists(extractPath))
                    {
                        Directory.CreateDirectory(extractPath);
                    }
                    LogManage.LogMessage($"CheckFold3 start");
                    if (!CheckFold(extractPath, out folderInfo, out pathSymbolicLinInfo))//0815 Bruce Add Security
                    {
                        swUpdateInfos[i].SWUErrorCode = SWUErrorCode.FolderIsNotSafe;
                        LogManage.LogMessage(swUpdateInfos[i].SoftwareName + " FolderIsNotSafe:" + folderInfo + "--or--" + pathSymbolicLinInfo);
                        method.DeleteFolder(savePath);
                        continue;
                    }
                    LogManage.LogMessage($"CheckFold3 ok");
                    try
                    {
                        using (FileLock fileLock = new FileLock(_installationFileStoragePath, PathCheckOption.None, lockNow: true))
                        {
                            string exeFilePath;
                            LogManage.LogMessage($"Unzip start");
                            if (!Unzip(_installationFileStoragePath, extractPath, out exeFilePath))
                            {
                                LogManage.LogMessage(_SWUpdateInfo.SoftwareName + " Unzip Faile");
                                _notificationStr = LangHelper.Instance["Software_update_unsuccessful"];
                                NotificationFWupdate(LangHelper.Instance["Error"], _notificationStr);
                                fileLock.Unlock();
                                method.DeleteFolder(savePath);
                                continue;
                            }
                            LogManage.LogMessage($"Unzip done");
                            LogManage.LogMessage($"Install start");
                            using (FileLock fileLock_2 = new FileLock(exeFilePath, PathCheckOption.None, lockNow: true))
                            {
                                if (!CheckThumbprint(exeFilePath, _SWUpdateInfo.Thumbprint, out string FileCAInfo))
                                {
                                    _SWUpdateInfo.SWUErrorCode = SWUErrorCode.FileCheckFail;
                                    LogManage.LogMessage($"{_SWUpdateInfo.SoftwareName} CheckThumbprint Faile");
                                    _notificationStr = LangHelper.Instance["Software_update_unsuccessful"];
                                    NotificationFWupdate(LangHelper.Instance["Error"], _notificationStr);
                                    fileLock_2.Unlock();
                                    fileLock.Unlock();
                                    method.DeleteFolder(savePath);
                                    continue;
                                }
                                ProcessStartInfo startInfo = new ProcessStartInfo()
                                {
                                    UseShellExecute = false,
                                    FileName = exeFilePath,//fileFullPath,
                                    Arguments = "/fromddm"
                                };
                                Process clientProcess = new Process();
                                clientProcess.StartInfo = startInfo;
                                clientProcess.Start();
                                ret = true;
                            }
                            LogManage.LogMessage($"Execution done");
                        }
                    }
                    catch (Exception ex)
                    {
                        LogManage.LogMessage($"{_SWUpdateInfo.SoftwareName} FileLock Error: {ex.Message}");
                    }
                }
                method.Dispose();
                LogManage.LogMessage(nameof(DownloadAndExecutionSwUpdater) + " done");
                return ret;
            }
            catch (Exception ex)
            {
                method.Dispose();
                foreach (SWUpdateInfo deviceInfo in swUpdateInfos)
                {
                    deviceInfo.SWUErrorCode = SWUErrorCode.NetworkDisconnection;
                }
                LogManage.LogMessage(nameof(DownloadAndExecutionSwUpdater) + " Error：" + ex.Message); // 輸出錯誤訊息
                return ret;
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
            //[Dean 1206] modify for checkmarx test
            int exitCode = -1;
            try
            {
                _SWUpdateInfo = swUpdateInfo;
                // 要運行的安裝程式路徑和命令行參數
                string arguments = "/silent /SecLaunchOnEnd";
                //Process _clientProcess = new Process();
                UpdateProgressInfo updateProgressInfo = new UpdateProgressInfo()
                {
                    DeviceName = _SWUpdateInfo.SoftwareName,
                    TheLatestVersion = _SWUpdateInfo.TheLatestVersion,
                    ProcessName = "Installing",
                    ProcessProgress = 0.0,
                };
                sendMessageToEvent(updateProgressInfo);
                RegEvent();

                string? fileFullPath = swUpdateInfo.InstallPaths;

                using (Process _clientProcess = new Process())
                {
                    if (File.Exists(fileFullPath))
                    {
                        //For checkmarx test, [code part1]
                        /*if (!DDPMFileSecurity.IsFilePathValid(fileFullPath, out string fileCheckInfo))
                        {
                            LogManage.LogMessage($"{nameof(DownloadAndInstall)} {_SWUpdateInfo.SoftwareName} FileIsNoSafe - FileCheckInfo : {fileCheckInfo}");
                            _notificationStr = LangHelper.Instance["Firmware_update_unsuccessful"];
                            NotificationFWupdate(LangHelper.Instance["Error"], _notificationStr);
                            _updateErrorCode = SWUErrorCode.FileIsNoSafe;
                            return _updateErrorCode;
                        }
                        ProcessStartInfo startInfo = new ProcessStartInfo()
                        {
                            UseShellExecute = false,
                            FileName = fileFullPath,
                            Arguments = arguments
                        };
                        //_clientProcess = new Process();                        
                        _clientProcess.StartInfo = startInfo;
                        _clientProcess.Start();
                        _clientProcess.WaitForExit();*/

                        //For checkmarx test, [code part2]
                        string fileFullPath_sanitized = DDPMFileSecurity.SanitizePath(fileFullPath, out string info);
                        string workingDirectory = DDPMFileSecurity.SanitizePath(Path.GetDirectoryName(fileFullPath), out string workingDirectorInfo);

                        if (!string.IsNullOrEmpty(fileFullPath_sanitized) && !string.IsNullOrEmpty(workingDirectory))
                        {
                            if (DDPMFileSecurity.ValidateFilePath(fileFullPath, out info))
                            {
                                ProcessStartInfo startInfo = new ProcessStartInfo()
                                {
                                    UseShellExecute = false,
                                    FileName = fileFullPath_sanitized,//fileFullPath,
                                    Arguments = arguments,
                                    WorkingDirectory = workingDirectory
                                };
                                //_clientProcess = new Process();                        
                                _clientProcess.StartInfo = startInfo;
                                _clientProcess.Start();
                                _clientProcess.WaitForExit();
                                exitCode = _clientProcess.ExitCode;
                            }
                            else
                            {
                                LogManage.LogMessage($"{nameof(DownloadAndInstall)} {_SWUpdateInfo.SoftwareName} FilePathIsNotSafe - result : {info}");
                                _notificationStr = LangHelper.Instance["Firmware_update_unsuccessful"];
                                NotificationFWupdate(LangHelper.Instance["Error"], _notificationStr);
                                _updateErrorCode = SWUErrorCode.FileIsNoSafe;
                                return _updateErrorCode;
                            }
                        }
                        else
                        {
                            LogManage.LogMessage($"{nameof(DownloadAndInstall)} {_SWUpdateInfo.SoftwareName} FilePath sanitized check - result : {info}");
                            LogManage.LogMessage($"{nameof(DownloadAndInstall)} {_SWUpdateInfo.SoftwareName} workingDirectory sanitized check - result : {workingDirectorInfo}");
                            _notificationStr = LangHelper.Instance["Firmware_update_unsuccessful"];
                            NotificationFWupdate(LangHelper.Instance["Error"], _notificationStr);
                            _updateErrorCode = SWUErrorCode.FileIsNoSafe;
                            return _updateErrorCode;
                        }
                    }
                    else
                    {
                        LogManage.LogMessage($"{nameof(DownloadAndInstall)} {_SWUpdateInfo.SoftwareName} File.Exists return false");
                        _notificationStr = LangHelper.Instance["Firmware_update_unsuccessful"];
                        NotificationFWupdate(LangHelper.Instance["Error"], _notificationStr);
                        _updateErrorCode = SWUErrorCode.FileCheckFail;
                        return _updateErrorCode;
                    }
                }
                LogManage.LogMessage(swUpdateInfo.SoftwareName + nameof(Install) + " exit code : " + exitCode);
                if (exitCode == 0)
                {
                    _updateErrorCode = SWUErrorCode.NoError;
                }
                else
                {
                    _updateErrorCode = SWUErrorCode.Unknow;
                }
                CancelRegEvent();
                return _updateErrorCode;
            }
            catch (Exception ex)
            {
                _updateErrorCode = SWUErrorCode.Unknow;
                LogManage.LogMessage(swUpdateInfo.SoftwareName + nameof(Install) + " Error:" + ex.ToString());
                _notificationStr = LangHelper.Instance["Service_not_running_Try_again"];
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
                //folderValid = false;
                //folderValid = DDPMFileSecurity.SRemoveSymbolicFolder(path, out pathSymbolicLinInfo);//0924 Bruce Add Security
                //if (!folderValid)
                //{
                //    LogManage.LogMessage(nameof(CheckFold) + " FolderSymbolicFolderIsNotSafe:" + pathSymbolicLinInfo + " Retry:" + (count++));
                //}
                //folderValid = DDPMFileSecurity.IsFolderPathValid(path, out folderInfo) && folderValid;
                folderValid = DDPMFileSecurity.ValidateFilePath(path, out folderInfo);
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
            CertificateCheck certificateCheck = new CertificateCheck(LogManage.Logs);
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
            if (isCheckSHA)
            {
                LogManage.LogMessage($"{_SWUpdateInfo.SoftwareName} CheckSHA pass");
            }
            else
            {
                if (!_SkipSHA)
                {
                    LogManage.LogMessage($"{_SWUpdateInfo.SoftwareName} CheckSHA fail fileCAInfo : {fileCAInfo}");
                    _SWUpdateInfo.SWUErrorCode = SWUErrorCode.FileCheckFail;
                }
                else
                {
                    LogManage.LogMessage($"{_SWUpdateInfo.SoftwareName} CheckSHA fail fileCAInfo : {fileCAInfo} BUT SKIP");
                    isCheckSHA = true;
                }
            }
            return isCheckSHA;
        }
        private bool CheckThumbprint(string filePath, string standThumbprint, out string fileThumbprintInfo)
        {
            CertificateCheck certificateCheck = new CertificateCheck(LogManage.Logs);
            bool ishumbprint = false;
            fileThumbprintInfo = "Error";
            ishumbprint = certificateCheck.CheckFile_Thumbprint(filePath, standThumbprint, out string FileCAInfo);
            if (ishumbprint)
            {
                LogManage.LogMessage($"{_SWUpdateInfo.SoftwareName} CheckFile_Thumbprint pass");
            }
            else
            {
                if (!_SkipSHA)
                {
                    LogManage.LogMessage($"{_SWUpdateInfo.SoftwareName} File check Thumbprint fail. Ex: {FileCAInfo}");
                    _SWUpdateInfo.SWUErrorCode = SWUErrorCode.FileCheckFail;
                }
                else
                {
                    LogManage.LogMessage($"{_SWUpdateInfo.SoftwareName} CheckFile_Thumbprint fail FileCAInfo : {FileCAInfo} BUT SKIP");
                    ishumbprint = true;
                }

            }
            return ishumbprint;
        }
        private bool Unzip(string filePath, string extractPath, out string exeFilePath)
        {
            LogManage.LogMessage($"{_SWUpdateInfo.SoftwareName} {nameof(Unzip)} Start");
            _SWUpdateInfo.SWUErrorCode = SWUErrorCode.Unknow;
            bool ret = false;
            Unzip unzip = new Unzip(LogManage.Logs);
            exeFilePath = "";
            string FileCAInfo = "Pass";
            LogManage.LogMessage($"{_SWUpdateInfo.SoftwareName} check SHA go.");
            if (CheckSHA(filePath, out FileCAInfo))
            {
                ret = true;
                if (unzip.CheckFileIsZip(filePath))
                {
                    LogManage.LogMessage($"{_SWUpdateInfo.SoftwareName} File is zip.");
                    LogManage.LogMessage($"{_SWUpdateInfo.SoftwareName} ExecuteUnzip go.");
                    if (unzip.ExecuteUnzip(filePath, extractPath, true, out exeFilePath))
                    {
                        if (string.IsNullOrEmpty(exeFilePath))
                        {
                            ret = false;
                            LogManage.LogMessage($"{_SWUpdateInfo.SoftwareName} exeFilePath IsNullOrEmpty.");
                        }
                    }
                    else
                    {
                        LogManage.LogMessage($"{_SWUpdateInfo.SoftwareName} Unzip Faile");
                    }
                }
                else
                {
                    LogManage.LogMessage($"{_SWUpdateInfo.SoftwareName} File is exe.");
                    exeFilePath = filePath;
                    ret = true;
                }
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
            LogManage.LogMessage($"RegEvent start");
            try
            {
                LogManage.LogMessage($"RegEvent watcher go");
                // 將反斜線進行正確轉義
                WqlEventQuery query = new WqlEventQuery(
                         "SELECT * FROM RegistryValueChangeEvent WHERE " +
                         "Hive = 'HKEY_LOCAL_MACHINE'" +
                         @"AND KeyPath = 'SOFTWARE\\Dell\\Dell Display and Peripheral Manager' AND ValueName='NextProcess'");
                watcher = new ManagementEventWatcher(query);
                LogManage.LogMessage("Waiting for an event...");
                watcher.EventArrived += new EventArrivedEventHandler(OnRegistryValueChanged);
                watcher.Start();
                LogManage.LogMessage("Waiting for _processTimer event...");
                _processTimer.Interval = TimeSpan.FromSeconds(3).TotalMilliseconds;
                _processTimer.Elapsed += new ElapsedEventHandler(InstallingProcessTimer_Elapsed);
                _processTimer.Start();
                LogManage.LogMessage($"RegEvent watcher done");
            }
            catch (ManagementException ex)
            {
                LogManage.LogMessage($"ManagementException: {ex.Message}");
            }
            catch (Exception ex)
            {
                LogManage.LogMessage($"Exception: {ex.Message}");
            }
            LogManage.LogMessage($"RegEvent done");
        }
        private void CancelRegEvent()
        {
            LogManage.LogMessage($"CancelRegEvent start");
            if (watcher != null)
            {
                LogManage.LogMessage($"CancelRegEvent watcher is not null");
                LogManage.LogMessage($"CancelRegEvent stop watcher go");
                watcher.Stop();
                watcher.EventArrived -= new EventArrivedEventHandler(OnRegistryValueChanged);
                LogManage.LogMessage($"CancelRegEvent stop watcher done");
            }
            if (_processTimer != null)
            {
                LogManage.LogMessage($"CancelRegEvent _processTimer is not null");
                LogManage.LogMessage($"CancelRegEvent stop _processTimer go");
                _processTimer.Stop();
                _processTimer.Elapsed -= new ElapsedEventHandler(InstallingProcessTimer_Elapsed);
                LogManage.LogMessage($"CancelRegEvent stop _processTimer done");
            }
            LogManage.LogMessage($"CancelRegEvent done");
        }
        private void OnRegistryValueChanged(object sender, EventArrivedEventArgs e)
        {
            RegistryKey localKey64 = RegistryKey.OpenBaseKey(Microsoft.Win32.RegistryHive.LocalMachine, RegistryView.Registry64);
            if (localKey64 != null)
            {
                RegistryKey registryKey = localKey64.OpenSubKey("SOFTWARE\\Dell\\Dell Display and Peripheral Manager\\", false);
                if (registryKey != null)
                {
                    string curProcess_str = (registryKey.GetValue("Process")?.ToString());
                    string nextProcess_str = (registryKey.GetValue("NextProcess")?.ToString());
                    if (!string.IsNullOrEmpty(curProcess_str) && int.TryParse(curProcess_str, out int curProcess))
                    {
                        //_CurrentProcess = curProcess;
                        //UpdateProgressInfo updateProgressInfo = new UpdateProgressInfo()
                        //{
                        //    DeviceName = _SWUpdateInfo.SoftwareName,
                        //    TheLatestVersion = _SWUpdateInfo.TheLatestVersion,
                        //    ProcessName = "Installing",
                        //    ProcessProgress = _CurrentProcess,
                        //};
                        //sendMessageToEvent(updateProgressInfo);
                    }
                    if (!string.IsNullOrEmpty(nextProcess_str) && int.TryParse(nextProcess_str, out int nextProcess))
                    {
                        _CurrentProcessLimit = nextProcess;
                        if (_CurrentProcessLimit >= 90)
                        {
                            _CurrentProcessLimit = 100;
                        }
                    }
                    LogManage.LogMessage($"OnRegistryValueChanged curProcess :{curProcess_str}");
                    LogManage.LogMessage($"OnRegistryValueChanged nextProcess :{nextProcess_str}");
                    //ResetTimer();
                }
            }
        }
        private void InstallingProcessTimer_Elapsed(object? sender, ElapsedEventArgs e)
        {
            LogManage.LogMessage($"InstallingProcessTimer_Elapsed _CurrentProcess :{_CurrentProcess}");
            LogManage.LogMessage($"InstallingProcessTimer_Elapsed _CurrentProcessLimit :{_CurrentProcessLimit}");
            if (_CurrentProcess < _CurrentProcessLimit)
            {
                UpdateProgressInfo updateProgressInfo = new UpdateProgressInfo()
                {
                    DeviceName = _SWUpdateInfo.SoftwareName,
                    TheLatestVersion = _SWUpdateInfo.TheLatestVersion,
                    ProcessName = "Installing",
                    ProcessProgress = ++_CurrentProcess,
                };
                sendMessageToEvent(updateProgressInfo);
            }
            if (_processTimer != null)
            {
                float difference = (float)_CurrentProcess / (float)_CurrentProcessLimit;
                LogManage.LogMessage($"InstallingProcessTimer_Elapsed difference :{difference}");
                if (difference <= 0.75)
                {
                    _processTimer.Interval = TimeSpan.FromSeconds(0.5).TotalMilliseconds;
                }
                else if (difference <= 0.95)
                {
                    _processTimer.Interval = TimeSpan.FromSeconds(1).TotalMilliseconds;
                }
                else
                {
                    _processTimer.Interval = TimeSpan.FromSeconds(3).TotalMilliseconds;
                }
                if (_CurrentProcessLimit >= 100)
                {
                    _processTimer.Interval = TimeSpan.FromSeconds(1).TotalMilliseconds;
                }
                LogManage.LogMessage($"InstallingProcessTimer_Elapsed _processTimer.Interval :{_processTimer.Interval}");
            }
        }
        private void ResetTimer()
        {
            LogManage.LogMessage($"ResetTimer start");
            if (_processTimer != null)
            {
                LogManage.LogMessage($"ResetTimer go");
                _processTimer.Stop();
                _processTimer.Interval = TimeSpan.FromSeconds(3).TotalMilliseconds;
                _processTimer.Start();
            }
            LogManage.LogMessage($"ResetTimer done");
        }
        private void sendMessageToEvent(UpdateProgressInfo fWUpdateInfo)
        {
            ProgressUpdate_Notify?.AsyncFireAndForget(this, fWUpdateInfo, System.Threading.CancellationToken.None);
            LogManage.LogMessage("sendMessageToEvent" + " " + fWUpdateInfo.ProcessName + " " + fWUpdateInfo.ProcessProgress + " " + DateTime.Now);
        }
    }
}