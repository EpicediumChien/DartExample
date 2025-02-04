using DDPM.SA.Common;
using DDPM.SA.Common.Method;
using DDPM.SA.Common.Security;
using DDPM.SA.Common.Settings;
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
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Security;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using VcpCore.Common;
using IDs = DDPM.SA.Common.IDs;
using JsonSerializer = System.Text.Json.JsonSerializer;
using RegistryHive = DDPM.SA.Common.Settings.RegistryHive;
using Timer = System.Timers.Timer;

namespace DDPM.SA.Plugins.SWUpdate
{
    [Plugin(IDs.SWUpdate_PLUGIN_ID, pluginName, PluginOrderGroupType.Core, Version = pluginVersion)]
    [Descriptor(Description = pluginDescription)]
    [Publisher(Name = publisherCompany, Website = publisherWebsite, Support = publisherSupport)]
    [PublishedUnelevatedInterface(new[] { typeof(ISWUpdateService) })]
    [DependencyKnownTypes(new[] { typeof(ISWUpdateService), typeof(ISettingsManagerSA) })]
    public class SWUpdatePlugins : BaseAgentPlugin, ISWUpdateService
    {
        #region Private Members

        private const string pluginName = "SWUpdatePlugin";
        private const string pluginVersion = "1.0.0";
        private const string pluginDescription = "This plugin implements SW Update Plugin.";
        private const string publisherCompany = "Dell Inc.";
        private const string publisherWebsite = "https://www.dell.com";
        private const string publisherSupport = "This plugin implements SW Update Plugin.";

        private IAgent _agent;

        #endregion Private Members

        public const string PluginLogId = "SWUpdate";

        private Logs _logs;

        static bool _IsSkipCA = false;
        static bool _IsSkipSHA = false;
        private ISettingsManagerSA _SettingsPlugin;
        private readonly object _PluginConditionLock_Settings = new object();

        /// <summary>
        /// 現在正在進行下載或安裝流程的裝置資訊
        /// </summary>
        private SWUpdateInfo _SWUpdateInfo = new SWUpdateInfo();

        /// <summary>
        /// 給UI或是CLI的全部軟體更新包
        /// </summary>
        private SWUpdateInfoPackage _SWUpdateInfoPackage;

        private Download? download = null;

        //安裝更新檔使用的命名管道伺服器
        private Timer _downloadTimer = new Timer();
        private string _notificationStr = "";
        private SWUErrorCode _updateErrorCode;
        private bool _IsShowNotify = true;
        private bool _IsUITrigger = false;
        private bool _bFirstInstance;
        private Mutex? _instanceMutex;
        private string? _applicationName;

        #region Events

        /// <summary>
        /// 呼叫Popup通知
        /// </summary>
        public event EventHandler<PopupContentPackage> CallPopup;

        /// <summary>
        /// 回傳更新事件結果提供給CLI使用
        /// </summary>
        public event EventHandler<List<SWUpdateInfo>> DownloadAndInstall_Result_Notify;
        public event EventHandler<(string, string, bool)> CallOSD;

        #endregion Events

        public SWUpdatePlugins(IAgent agent) : base(agent, PluginLogId)
        {
            _agent = agent;
            _logs ??= new Logs(Log, PluginLogId);
            _agent.PluginManager.PluginsStarted += PluginManagerOnPluginsStarted;
            InitializeSettingsPlugin();
            _SWUpdateInfoPackage = new SWUpdateInfoPackage();
        }
        #region Overriding methods

        #region IDisposableObservable Support

        /// <summary>
        /// To detect redundant calls
        /// </summary>
        public bool IsDisposed { get; private set; }

        /// <summary>
        /// Override for Dispose
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
#if DEBUG
            Console.WriteLine($"Dispose: {disposing}");
#endif
            if (!IsDisposed)
            {
                if (disposing)
                {
                    _agent.PluginManager.PluginsStarted -= PluginManagerOnPluginsStarted;
                    _agent = null;
                }

                IsDisposed = true;
            }
            base.Dispose(disposing);
        }

        #endregion IDisposableObservable Support

        #region Event Handler

        private void PluginManagerOnPluginsStarted(object sender, PluginsStartedEventArgs e)
        {
            if (e == null)
                return;
            if (e.ChangedPlugins == null)
                return;
            if (e.ChangedPlugins.Any() == false)
                return;

            if (e.ChangedPlugins.OfType<ISettingsManagerSA>().Any())
                InitializeSettingsPlugin();
        }

        #endregion Event Handler

        protected override void OnPluginStarting()
        {
            _agent.PluginManager.PluginsStarted += PluginManagerOnPluginsStarted;
            PluginCondition = new PluginStartedCondition();
#if DEBUG
            Console.WriteLine("SWUpdate plugin report started");
#endif
        }

        #endregion Overriding methods

        /// <summary>
        /// 取得更新的資訊包
        /// </summary>
        /// <param name="updateHelper">IL的更新資訊</param>
        /// <param name="isShowNotify">是否顯示右下角通知圖示</param>
        /// <returns>回傳更新資訊包</returns>
        public Task<SWUpdateInfoPackage> GetSWUpdateInfo(bool isShowNotify, string currentVersion, bool reScan)
        {
            if (reScan)
            {
                CheckUpdate(isShowNotify, currentVersion);
            }
            return Task.FromResult(_SWUpdateInfoPackage);
        }

        /// <summary>
        /// 檢查更新資訊
        /// </summary>
        /// <param name="updateHelper">IL的更新資訊</param>
        /// <param name="isShowNotify">是否顯示右下角通知圖示</param>
        /// <returns>回傳裝置資訊表(如果有需強制安裝更新的話，該裝置資訊表會被寫入對應裝置的安裝結果)</returns>
        private void CheckUpdate(bool isShowNotify, string currentVersion)
        {
            _logs.DebugMsg_1(nameof(CheckUpdate) + " start");
            if (_SettingsPlugin != null)
            {
                _SWUpdateInfoPackage = new SWUpdateInfoPackage();
                _SWUpdateInfoPackage.TheLastCheckTime = DateTime.Now;
                _IsShowNotify = isShowNotify;
                if (string.IsNullOrEmpty(currentVersion))
                {
                    _SWUpdateInfoPackage.SWUpdateInfo = new List<SWUpdateInfo>();
                }
                SWUpdateHelper swUpdateHelper = SWUpdateSetting.GetSWMetadata(_IsSkipCA, out string getMetadataInfo, _SettingsPlugin, null, _logs);
                _logs.DebugMsg_1($"{nameof(CheckUpdate)} {getMetadataInfo}");
                if (swUpdateHelper.Softwares != null && swUpdateHelper.Softwares.Count > 0)
                {
                    _logs.DebugMsg_1($"{nameof(CheckUpdate)} swUpdateHelper.Softwares.Count : {swUpdateHelper.Softwares.Count}");
                    for (int i = 0; i < swUpdateHelper.Softwares.Count; i++)
                    {
                        bool needUpdate = SWUpdateSetting.CompareVersions(currentVersion, swUpdateHelper.Softwares[i].SoftwareVersion, _logs);
                        SWUpdateInfo SWUpdateInfo = new SWUpdateInfo()
                        {
                            TheLatestVersion = swUpdateHelper.Softwares[i].SoftwareVersion,
                            SoftwareVersion = currentVersion,
                            NeedUpdated = needUpdate,
                            ServerPath = swUpdateHelper.Softwares[i].DdpmSwUpdaterServer_path,
                            SHA256 = swUpdateHelper.Softwares[i].DdpmSwUpdater_SHA256,
                            SHA512 = swUpdateHelper.Softwares[i].DdpmSwUpdater_SHA512,
                            Thumbprint = swUpdateHelper.Softwares[i].DdpmSwUpdater_Thumbprint,
                            SoftwareName = "DDPM",
                            FileSavepath = swUpdateHelper.Softwares[i].InstallPath,
                            Available_date = _SWUpdateInfoPackage.TheLastCheckTime.ToString("yyyy/MM/dd HH:mm:ss")
                        };
                        _logs.DebugMsg_1($"{nameof(CheckUpdate)} swUpdateHelper.Softwares[i].SoftwareVersion : {swUpdateHelper.Softwares[i].SoftwareVersion}");
                        _logs.DebugMsg_1($"{nameof(CheckUpdate)} currentVersion : {currentVersion}");
                        _logs.DebugMsg_1($"{nameof(CheckUpdate)} SWUpdateInfo.NeedUpdated : {SWUpdateInfo.NeedUpdated}");
                        if (SWUpdateInfo.NeedUpdated)
                        {
                            _SWUpdateInfoPackage.SWUpdateInfo.Add(SWUpdateInfo);
                        }
                        _logs.DebugMsg_1($"{nameof(CheckUpdate)} _SWUpdateInfoPackage.SWUpdateInfo.Count : {_SWUpdateInfoPackage.SWUpdateInfo.Count}");
                    }
                    _logs.DebugMsg_1(nameof(CheckUpdate) + " done.");
                }
            }
            else
            {
                _logs.DebugMsg_1(nameof(CheckUpdate) + " done but _SettingsPlugin is null");
            }
        }
        /// <summary>
        /// 從伺服端下載更新檔，下載後會接續執行安裝方法
        /// </summary>
        /// <param name="swUpdateInfos">更新的裝置資訊表</param>
        /// <returns>回傳裝置資訊表(在這個方法裡將原本傳入的裝置資訊表，再寫入對應裝置的下載安裝的結果碼)</returns>
        public Task<List<SWUpdateInfo>> DownloadAndInstall(List<SWUpdateInfo> swUpdateInfos, bool isUITrigger, string installPath)
        {
            Method method = new Method(_logs);
            try
            {
                _IsUITrigger = isUITrigger;
                string path_programdata = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
                _logs.DebugMsg_1(nameof(DownloadAndInstall) + " start");
                string appName = "DdpmSwUpdater.exe";
                _logs.DebugMsg_1($"appName : {appName}");
                _instanceMutex = new Mutex(false, appName, out _bFirstInstance);
                if (!_bFirstInstance)
                {
                    foreach (SWUpdateInfo swUpdateInfo in swUpdateInfos)
                    {
                        _logs.DebugMsg_1($"The program is already running and a new instance cannot be started");
                        swUpdateInfo.SWUErrorCode = SWUErrorCode.ServiceNotRunning;
                    }
                    return Task.FromResult(swUpdateInfos);
                }
                _logs.DebugMsg_1($"_instanceMutex?.Dispose() go");
                _instanceMutex?.Dispose();
                _logs.DebugMsg_1($"_instanceMutex?.Dispose() done");
                string saveFolderName = Guid.NewGuid().ToString();
                string savePath;
                CertificateCheck caCheck = new CertificateCheck(_logs);
                DDPMFileSecurity DDPMFileSecurity = new DDPMFileSecurity();
                if (string.IsNullOrEmpty(installPath))
                {
                    savePath = path_programdata + "\\Dell\\Dell Display and Peripheral Manager" + "\\" + saveFolderName + "\\";
                }
                else
                {
                    savePath = installPath;
                }
                if (!Directory.Exists(savePath))
                {
                    Directory.CreateDirectory(savePath);
                }
                //0926 Bruce Add Security
                if (!DDPMFileSecurity.CheckFold(savePath, out string FolderInfo, out string PathSymbolicLinInfo))
                {
                    foreach (SWUpdateInfo swUpdateInfo in swUpdateInfos)
                    {
                        swUpdateInfo.SWUErrorCode = SWUErrorCode.FolderIsNotSafe;
                    }
                    _notificationStr = LangHelper.Instance["Software_update_unsuccessful"];
                    NotificationFWupdate(LangHelper.Instance["Error"], _notificationStr);
                    _logs.DebugMsg_1(nameof(DownloadAndInstall) + " savePath FolderIsNotSafe:" + FolderInfo + "--or--" + PathSymbolicLinInfo);
                    method.DeleteFolder(savePath);
                    return Task.FromResult(swUpdateInfos);
                }
                for (int i = 0; i < swUpdateInfos.Count; i++)
                {
                    _updateErrorCode = SWUErrorCode.Unknow;
                    swUpdateInfos[i].SWUErrorCode = _updateErrorCode;
                    _SWUpdateInfo = swUpdateInfos[i];
                    _notificationStr = "";
                    _logs.DebugMsg_1(_SWUpdateInfo.SoftwareName + nameof(DownloadAndInstall) + " start");
                    string url = swUpdateInfos[i].ServerPath;
                    if (!DDPMFileSecurity.CheckFold(savePath, out FolderInfo, out PathSymbolicLinInfo))
                    {
                        swUpdateInfos[i].SWUErrorCode = SWUErrorCode.FolderIsNotSafe;
                        _notificationStr = LangHelper.Instance["Software_update_unsuccessful"];
                        NotificationFWupdate(LangHelper.Instance["Error"], _notificationStr);
                        _logs.DebugMsg_1(nameof(DownloadAndInstall) + " savePath FolderIsNotSafe:" + FolderInfo + "--or--" + PathSymbolicLinInfo);
                        method.DeleteFolder(savePath);
                        continue;
                    }
                    if (!NetworkInterface.GetIsNetworkAvailable())
                    {
                        swUpdateInfos[i].SWUErrorCode = SWUErrorCode.NetworkDisconnection;
                        _notificationStr = $"{swUpdateInfos[i].SoftwareName}{LangHelper.Instance["Update_failed_due_to_network_error"]}";
                        NotificationFWupdate(LangHelper.Instance["Error"], _notificationStr);
                        _logs.DebugMsg_1(swUpdateInfos[i].SoftwareName + " Download File Fail : " + _notificationStr);
                        method.DeleteFolder(savePath);
                        continue;
                    }
                    string _installationFileStoragePath;
                    try
                    {
                        _downloadTimer = new Timer();
                        _downloadTimer.Interval = 1000;
                        _downloadTimer.Elapsed += new ElapsedEventHandler(DownloadTimer_Elapsed);
                        _downloadTimer.Start();
                        download = new Download(_logs);
                        string downloadInfo = "";
                        // 將儲存路徑與從 URL 中提取的檔案名稱組合
                        if (_IsSkipSHA)
                        {
                            _logs.DebugMsg_1($"{nameof(DownloadAndInstall)} ServerPath : {GlobalDefinitions.GetLogPrintServerName(url)}");
                        }
                        _installationFileStoragePath = Path.Combine(savePath + Path.GetFileName(url));
                        bool downloadRet = download.DownloadFile(url, _installationFileStoragePath, out downloadInfo, _IsSkipCA);
                        _downloadTimer.Elapsed -= new ElapsedEventHandler(DownloadTimer_Elapsed);
                        _downloadTimer.Stop();
                        UpdateProgressInfo updateProgressInfo = new UpdateProgressInfo()
                        {
                            DeviceName = swUpdateInfos[i].SoftwareName,
                            TheLatestVersion = swUpdateInfos[i].TheLatestVersion,
                            ProcessName = LangHelper.Instance["Downloading_and_installing"],
                            ProcessProgress = 100,
                        };
                        if (!downloadRet)
                        {
                            if (downloadInfo.Equals("CA check fail"))
                            {
                                swUpdateInfos[i].SWUErrorCode = SWUErrorCode.CAFail;
                                _notificationStr = LangHelper.Instance["Software_update_unsuccessful"];
                                NotificationFWupdate(LangHelper.Instance["Error"], _notificationStr);
                                method.DeleteFolder(savePath);
                            }
                            else if (downloadInfo.Equals("Network fail"))
                            {
                                swUpdateInfos[i].SWUErrorCode = SWUErrorCode.NetworkDisconnection;
                                _notificationStr = LangHelper.Instance["Update_failed_due_to_network_error"];
                                NotificationFWupdate(LangHelper.Instance["Error"], _notificationStr);
                                method.DeleteFolder(savePath);
                            }
                            _logs.DebugMsg_1(swUpdateInfos[i].SoftwareName + " Download File Fail");
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
                        swUpdateInfos[i].SWUErrorCode = SWUErrorCode.NetworkDisconnection;
                        _notificationStr = LangHelper.Instance["Update_failed_due_to_network_error"];
                        NotificationFWupdate(LangHelper.Instance["Error"], _notificationStr);
                        _logs.DebugMsg_1(nameof(DownloadAndInstall) + " Error：Update failed due to network error. Try again. ex:" + ex.Message); // 輸出錯誤訊息
                        method.DeleteFolder(savePath);
                        continue;
                    }
                    string extractPath = Path.Combine(savePath + Path.GetFileName(url).Substring(0, Path.GetFileName(url).Length - 4));
                    if (!Directory.Exists(extractPath))
                    {
                        Directory.CreateDirectory(extractPath);
                    }
                    if (!DDPMFileSecurity.CheckFold(extractPath, out FolderInfo, out PathSymbolicLinInfo))
                    {
                        swUpdateInfos[i].SWUErrorCode = SWUErrorCode.FolderIsNotSafe;
                        _logs.DebugMsg_1(swUpdateInfos[i].SoftwareName + " extractPath FolderIsNotSafe:" + FolderInfo + "--or--" + PathSymbolicLinInfo);
                        _notificationStr = LangHelper.Instance["Software_update_unsuccessful"];
                        NotificationFWupdate(LangHelper.Instance["Error"], _notificationStr);
                        method.DeleteFolder(savePath);
                        continue;
                    }
                    string exeFilePath;
                    using (FileLock fileLock = new FileLock(_installationFileStoragePath, PathCheckOption.None, lockNow: true))
                    {
                        if (!Unzip(_installationFileStoragePath, extractPath, out exeFilePath))
                        {
                            _SWUpdateInfo.SWUErrorCode = SWUErrorCode.FileCheckFail;
                            _logs.DebugMsg_1(_SWUpdateInfo.SoftwareName + " Unzip Faile");
                            _notificationStr = LangHelper.Instance["Software_update_unsuccessful"];
                            NotificationFWupdate(LangHelper.Instance["Error"], _notificationStr);
                            fileLock.Unlock();
                            method.DeleteFolder(savePath);
                            continue;
                        }
                        using (FileLock fileLock_2 = new FileLock(exeFilePath, PathCheckOption.None, lockNow: true))
                        {
                            if (!CheckThumbprint(exeFilePath, _SWUpdateInfo.Thumbprint, out string FileCAInfo))
                            {
                                _SWUpdateInfo.SWUErrorCode = SWUErrorCode.FileCheckFail;
                                _logs.DebugMsg_1($"{_SWUpdateInfo.SoftwareName} CheckThumbprint Faile");
                                _notificationStr = LangHelper.Instance["Software_update_unsuccessful"];
                                NotificationFWupdate(LangHelper.Instance["Error"], _notificationStr);
                                fileLock_2.Unlock();
                                fileLock.Unlock();
                                method.DeleteFolder(savePath);
                                continue;
                            }
                            swUpdateInfos[i].InstallPaths = exeFilePath;
                            swUpdateInfos[i].SWUErrorCode = Install(swUpdateInfos[i]).Result;
                        }
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
                _logs.DebugMsg_1(nameof(DownloadAndInstall) + " done");
                method.Dispose();
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
                _logs.DebugMsg_1(nameof(DownloadAndInstall) + " Error：Update failed due to network error. Try again. ex:" + ex.Message); // 輸出錯誤訊息
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
            if (download != null &&
                download.DownloadFileStream != null)
            {
                UpdateProgressInfo updateProgressInfo = new UpdateProgressInfo()
                {
                    DeviceName = _SWUpdateInfo.SoftwareName,
                    TheLatestVersion = _SWUpdateInfo.TheLatestVersion,
                    ProcessName = LangHelper.Instance["Downloading_and_installing"],
                    ProcessProgress = download.GetProgress(),
                };
            }
        }

        /// <summary>
        /// 跳出通知
        /// </summary>
        private void NotificationFWupdate(string title, string info, bool stayOpen = false)
        {
            if (!_IsUITrigger)
            {
                if (!string.IsNullOrEmpty(info) && _IsShowNotify)
                {
                    PopupContentPackage popupContentPackage = new PopupContentPackage()
                    {
                        Title = title,
                        Info = info,
                        IsInfo = true,
                        IsOnlyUpdate = false,
                        StayOpen = stayOpen,
                        Timeout = 5,
                        Object = _SWUpdateInfoPackage,
                        PopupType = PopupContentPackage_Enum.SWU
                    };
                    CallPopup?.AsyncFireAndForget(this, popupContentPackage, System.Threading.CancellationToken.None);
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(info))
                {
                    CallOSD?.AsyncFireAndForget(this, (title, info, stayOpen), System.Threading.CancellationToken.None);
                }
            }
        }
        public void SetSkipCA(bool isSkipCA)
        {
            _logs.DebugMsg_1("SetSkipCA start");
            _logs.DebugMsg_1($"SetSkipCA isSkipCA : {isSkipCA}");
            _IsSkipCA = isSkipCA;
            _logs.DebugMsg_1("SetSkipCA done");
        }
        public void SetSkipSHA(bool isSkipSHA)
        {
            _logs.DebugMsg_1("SetSkipSHA start");
            _logs.DebugMsg_1($"SetSkipSHA isSkipSHA : {isSkipSHA}");
            _IsSkipSHA = isSkipSHA;
            _logs.DebugMsg_1("SetSkipSHA done");
        }
        /// <summary>
        /// 安裝下載好的更新檔
        /// </summary>
        private Task<SWUErrorCode> Install(SWUpdateInfo swUpdateInfos)
        {
            try
            {
                _logs.DebugMsg_1($"{nameof(Install)} start");
                string miniInstallPath = swUpdateInfos.InstallPaths;
                PInvoke.PROCESS_INFORMATION procInfo;
                string workingDirectory = DDPMFileSecurity.SanitizePath(Path.GetDirectoryName(miniInstallPath), out string info);
                if (!string.IsNullOrEmpty(workingDirectory))
                {
                    _logs.DebugMsg_1($"{nameof(Install)} workingDirectory is not null");
                    string arguments_Final = miniInstallPath + " /fromddpm";
                    //_logs.DebugMsg_1($"arguments_Final : {arguments_Final}");
                    WTSFunction.StartProcessAndBypassUACWithAdmin(arguments_Final, workingDirectory, out procInfo);
                }
                else
                {
                    _logs.DebugMsg_1($"{nameof(Install)} workingDirectory is null, info : {info}");
                }

                //var sessionId = Kernel32.WTSGetActiveConsoleSessionId();
                //if (sessionId is Advapi32.InvalidSessionId) throw new InvalidOperationException($"Cannot get session id");
                //IntPtr token = UserImpersonator.GetTokenFromSession(sessionId, systemUser: false);
                //using (FileLock fileLock = new FileLock(miniInstallPath, PathCheckOption.None, lockNow: true))
                //{
                //    UserImpersonator.RunAsUser(token, () =>
                //    {
                //        using (Process _clientProcess = new Process())
                //        {
                //            _clientProcess.StartInfo.UseShellExecute = false;
                //            _clientProcess.StartInfo.FileName = miniInstallPath;
                //            _clientProcess.StartInfo.WorkingDirectory = Path.GetDirectoryName(_clientProcess.StartInfo.FileName);
                //            _clientProcess.StartInfo.Arguments = arguments;
                //            _clientProcess.Start();
                //        }
                //    });
                //}
                _updateErrorCode = SWUErrorCode.NoError;
                _logs.DebugMsg_1($"{nameof(Install)} _notificationStr {_notificationStr}");
                _logs.DebugMsg_1($"{nameof(Install)} done");
                return Task.FromResult(_updateErrorCode);
            }
            catch (Exception ex)
            {
                _updateErrorCode = SWUErrorCode.Unknow;
                _logs.DebugMsg_1(nameof(Install) + " Error:" + ex.ToString());
                _notificationStr = LangHelper.Instance["Service_not_running_Try_again"];
                return Task.FromResult(_updateErrorCode);
            }
        }
        /*private bool CheckFold(string path, out string folderInfo, out string pathSymbolicLinInfo)
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
                //[Dean 1122] remove this action
                //folderValid = DDPMFileSecurity.SRemoveSymbolicFolder(path, out pathSymbolicLinInfo);//0924 Bruce Add Security
                //if (!folderValid)
                //{
                //    _logs.DebugMsg_1(nameof(DownloadAndInstall) + " FolderIsNotSafe:" + pathSymbolicLinInfo + " Retry:" + (count++));
                //}
                folderValid = DDPMFileSecurity.IsFolderPathValid(path, out folderInfo);// && folderValid;
                if (!folderValid)
                {
                    _logs.DebugMsg_1(nameof(DownloadAndInstall) + " FolderIsNotSafe:" + folderInfo + " Retry:" + (count++));
                    Directory.Delete(path, true);
                    Directory.CreateDirectory(path);
                }
            } while (!folderValid && count < 2);
            return folderValid;
        }*/
        private bool CheckSHA(string filePath, out string fileCAInfo)
        {
            CertificateCheck certificateCheck = new CertificateCheck(_logs);
            bool isCheckSHA = false;
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
                _logs.DebugMsg_1($"{_SWUpdateInfo.SoftwareName} CheckSHA pass");
            }
            else
            {
                if (!_IsSkipSHA)
                {
                    _logs.DebugMsg_1($"{_SWUpdateInfo.SoftwareName} CheckSHA fail fileCAInfo : {fileCAInfo}");
                    _SWUpdateInfo.SWUErrorCode = SWUErrorCode.FileCheckFail;
                }
                else
                {
                    _logs.DebugMsg_1($"{_SWUpdateInfo.SoftwareName} CheckSHA fail fileCAInfo : {fileCAInfo} BUT SKIP");
                    isCheckSHA = true;
                }
            }
            return isCheckSHA;
        }
        private bool CheckThumbprint(string filePath, string standThumbprint, out string fileThumbprintInfo)
        {
            CertificateCheck certificateCheck = new CertificateCheck(_logs);
            bool ishumbprint = false;
            fileThumbprintInfo = "Error";
            ishumbprint = certificateCheck.CheckFile_Thumbprint(filePath, standThumbprint, out string FileCAInfo);
            if (ishumbprint)
            {
                _logs.DebugMsg_1($"{_SWUpdateInfo.SoftwareName} CheckFile_Thumbprint pass");
            }
            else
            {
                if (!_IsSkipSHA)
                {
                    _logs.DebugMsg_1($"{_SWUpdateInfo.SoftwareName} File check Thumbprint fail. Ex: {FileCAInfo}");
                    _SWUpdateInfo.SWUErrorCode = SWUErrorCode.FileCheckFail;
                }
                else
                {
                    _logs.DebugMsg_1($"{_SWUpdateInfo.SoftwareName} File check Thumbprint fail. Ex: {FileCAInfo} BUT SKIP");
                    ishumbprint = true;
                }

            }
            return ishumbprint;
        }
        private bool Unzip(string filePath, string extractPath, out string exeFilePath)
        {
            _logs.DebugMsg_1($"{_SWUpdateInfo.SoftwareName} {nameof(Unzip)} Start");
            _SWUpdateInfo.SWUErrorCode = SWUErrorCode.Unknow;
            bool ret = false;
            Unzip unzip = new Unzip(_logs);
            exeFilePath = "";
            string FileCAInfo = "Pass";
            _logs.DebugMsg_1($"{_SWUpdateInfo.SoftwareName} check SHA go.");
            if (CheckSHA(filePath, out FileCAInfo))
            {
                ret = true;
                if (unzip.CheckFileIsZip(filePath))
                {
                    _logs.DebugMsg_1($"{_SWUpdateInfo.SoftwareName} File is zip.");
                    _logs.DebugMsg_1($"{_SWUpdateInfo.SoftwareName} ExecuteUnzip go.");
                    if (unzip.ExecuteUnzip(filePath, extractPath, true, out exeFilePath))
                    {
                        if (string.IsNullOrEmpty(exeFilePath))
                        {
                            ret = false;
                            _logs.DebugMsg_1($"{_SWUpdateInfo.SoftwareName} exeFilePath IsNullOrEmpty.");
                        }
                    }
                    else
                    {
                        _logs.DebugMsg_1($"{_SWUpdateInfo.SoftwareName} Unzip Faile");
                    }
                }
                else
                {
                    _logs.DebugMsg_1($"{_SWUpdateInfo.SoftwareName} File is exe.");
                    exeFilePath = filePath;
                    ret = true;
                }
            }
            _logs.DebugMsg_1($"{_SWUpdateInfo.SoftwareName} {nameof(Unzip)} done");
            return ret;
        }
        private void InitializeSettingsPlugin()
        {
            _logs.DebugMsg_1(nameof(InitializeSettingsPlugin) + " start");
            if (_SettingsPlugin != null)
                return;
            _logs.DebugMsg_1(nameof(InitializeSettingsPlugin) + " FindPluginByType");
            _SettingsPlugin = _agent.PluginManager.FindPluginByType<ISettingsManagerSA>(PluginResolution.Dynamic);

            if (_SettingsPlugin is IFrameworkPluginConditionNotification pluginCondition)
            {
                pluginCondition.PluginConditionChangeHandler += OnSettingsPluginConditionChangeHandler;
                GetCurrentSettingsPluginCondition();
            }
        }
        private void OnSettingsPluginConditionChangeHandler(object sender, EventArgs e)
        {
            GetCurrentSettingsPluginCondition();
        }
        private void GetCurrentSettingsPluginCondition()
        {
            _logs.DebugMsg_1($"{nameof(GetCurrentSettingsPluginCondition)} - start");
            _ = Task.Run(async () =>
            {
                var pluginCondition = await (_SettingsPlugin as IFrameworkPluginConditionNotification)?.CurrentConditionAsync();
                //PluginCondition _SettingsPluginCondition;
                lock (_PluginConditionLock_Settings)
                {
                    if (pluginCondition is PluginErrorCondition)
                    {
                        _logs.DebugMsg_1($"{nameof(GetCurrentSettingsPluginCondition)} - Settings Plugin is in an error condition");
                    }
                    else if (pluginCondition is PluginRunningCondition || pluginCondition is PluginStartedCondition)
                    {
                        _logs.DebugMsg_1($"{nameof(GetCurrentSettingsPluginCondition)} - Settings Plugin is in a running/started condition");
                    }
                    else
                    {
                        _logs.DebugMsg_1($"{nameof(GetCurrentSettingsPluginCondition)} - Settings Plugin is in unknow condition: {pluginCondition}");
                    }
                }
            });
        }
        public bool WriteRegistryData(RegistryHive hive, string keyPath, string keyName, object value)
        {
            _logs.DebugMsg_1($"WriteRegistryData start");
            bool settings = false;
            if (_SettingsPlugin != null)
            {
                _logs.DebugMsg_1($"WriteRegistryData _SettingsPlugin.WriteRegistryData go");
                settings = _SettingsPlugin.WriteRegistryData(hive, keyPath, keyName, value).Result;
            }
            _logs.DebugMsg_1($"WriteRegistryData done ret : {settings}");
            return settings;
        }
    }
}