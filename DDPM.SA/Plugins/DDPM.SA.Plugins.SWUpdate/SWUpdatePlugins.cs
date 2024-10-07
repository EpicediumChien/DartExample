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
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Timers;
using VcpCore.Common;
using IDs = DDPM.SA.Common.IDs;
using JsonSerializer = System.Text.Json.JsonSerializer;
using Timer = System.Timers.Timer;

namespace DDPM.SA.Plugins.SWUpdate
{
    [Plugin(IDs.SWUpdate_PLUGIN_ID, pluginName, PluginOrderGroupType.Core, Version = pluginVersion)]
    [Descriptor(Description = pluginDescription)]
    [Publisher(Name = publisherCompany, Website = publisherWebsite, Support = publisherSupport)]
    [PublishedUnelevatedInterface(new[] { typeof(ISWUpdateService) })]
    [DependencyKnownTypes(new[] { typeof(ISWUpdateService) })]
    public class SWUpdatePlugins : BaseAgentPlugin, ISWUpdateService
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

        private Logs _logs;

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

        //安裝更新檔使用的命名管道伺服器
        private Timer _downloadTimer = new Timer();

        private Timer _checkUpdateScheduleTimer;
        private string _notificationStr = "";
        private SWUErrorCode _updateErrorCode;
        private bool _IsShowNotify = true;
        private bool _isDefer = false;
        private bool _isForce = false;


        #region Events

        /// <summary>
        /// 呼叫DeviceManager呼叫我的檢查更新方法，用於排成定期檢查
        /// </summary>
        public event EventHandler? CollCheckUpdate;

        /// <summary>
        /// 將延遲更新包傳給DeviceManager進行儲存
        /// </summary>
        public event EventHandler<SWUpdateInfoPackage>? CallSaveUpdateInfoPackage;

        /// <summary>
        /// 呼叫Popup通知
        /// </summary>
        public event EventHandler<PopupContentPackage> CallPopup;

        #endregion Events

        public SWUpdatePlugins(IAgent agent) : base(agent, PluginLogId)
        {
            _agent = agent;
            _logs ??= new Logs(Log, PluginLogId);
            SystemEvents.PowerModeChanged += OnPowerModeChanged;
            _SWUpdateInfoPackage = new SWUpdateInfoPackage();
            _checkUpdateScheduleTimer = new Timer();
            _checkUpdateScheduleTimer.Interval = TimeSpan.FromMinutes(0.5).TotalMilliseconds;
            _checkUpdateScheduleTimer.Elapsed += new ElapsedEventHandler(CheckUpdateScheduleTimer_Elapsed);
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
            Console.WriteLine($"Dispose: {disposing}");
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

            if (e.ChangedPlugins.OfType<IFWUpdateService>().Any())
            {
                Console.WriteLine("ISWUpdateService plugin started.");
            }
        }

        #endregion Event Handler

        protected override void OnPluginStarting()
        {
            _agent.PluginManager.PluginsStarted += PluginManagerOnPluginsStarted;
            PluginCondition = new PluginStartedCondition();
            Console.WriteLine("SWUpdate plugin report started");
        }

        #endregion Overriding methods

        /// <summary>
        /// 啟動檢查更新排程
        /// </summary>
        public void StartCheckUpdateScheduleTimer()
        {
            _checkUpdateScheduleTimer.Start();
        }

        /// <summary>
        /// 設定檔儲存的延遲更新資訊包
        /// </summary>
        /// <param name="DelaySWUpdateInfoPackage">設定檔儲存的延遲更新資訊包</param>
        public void SetDelaySWUpdateInfoPackage(SWUpdateInfoPackage DelaySWUpdateInfoPackage)
        {
            if (DelaySWUpdateInfoPackage != null)
            {
                _DelaySWUpdateInfoPackage = DelaySWUpdateInfoPackage;
            }
            else
            {
                _DelaySWUpdateInfoPackage = new SWUpdateInfoPackage();
            }
        }

        /// <summary>
        /// 取得更新的資訊包
        /// </summary>
        /// <param name="updateHelper">IL的更新資訊</param>
        /// <param name="isShowNotify">是否顯示右下角通知圖示</param>
        /// <returns>回傳更新資訊包</returns>
        public Task<SWUpdateInfoPackage> GetSWUpdateInfo(bool isShowNotify, bool isForce, bool isDefer, string currentVersion)
        {
            _isDefer = isDefer;
            _isForce = isForce;
            _ = CheckUpdate(isShowNotify, currentVersion).Result;
            return Task.FromResult(_SWUpdateInfoPackage);
        }

        /// <summary>
        /// 檢查更新資訊
        /// </summary>
        /// <param name="updateHelper">IL的更新資訊</param>
        /// <param name="isShowNotify">是否顯示右下角通知圖示</param>
        /// <returns>回傳裝置資訊表(如果有需強制安裝更新的話，該裝置資訊表會被寫入對應裝置的安裝結果)</returns>
        public Task<List<SWUpdateInfo>> CheckUpdate(bool isShowNotify, string currentVersion)
        {
            _logs.DebugMsg_1(nameof(CheckUpdate) + " start");
            _SWUpdateInfoPackage = new SWUpdateInfoPackage();
            _SWUpdateInfoPackage.TheLastCheckTime = DateTime.Now;
            _IsShowNotify = isShowNotify;
            if (string.IsNullOrEmpty(currentVersion))
            {
                return Task.FromResult(new List<SWUpdateInfo>());
            }
            if (currentVersion.Contains("."))
            {
                currentVersion = currentVersion.Replace(".", "");
            }
            SWUpdateHelper swUpdateHelper = SWUpdateSetting.GetSWMetadata(out string getMetadataInfo);
            _logs.DebugMsg_1($"{nameof(CheckUpdate)} {getMetadataInfo}");
            if (swUpdateHelper.Softwares != null && swUpdateHelper.Softwares.Count > 0)
            {
                for (int i = 0; i < swUpdateHelper.Softwares.Count; i++)
                {
                    SWUpdateInfo SWUpdateInfo = new SWUpdateInfo()
                    {
                        TheLatestVersion = Regex.Replace(Convert.ToInt32(swUpdateHelper.Softwares[i].SoftwareVersion).ToString("D4"), @"(.{1})(.{1})(.{1})(.{1})", "$1.$2.$3.$4"),
                        SoftwareVersion = Regex.Replace(Convert.ToInt32(currentVersion).ToString("D4"), ".{1}", "$0.").Substring(0, (Convert.ToInt32(currentVersion).ToString("D4").Length * 2) - 1),
                        NeedUpdated = int.Parse(swUpdateHelper.Softwares[i].SoftwareVersion) > int.Parse(currentVersion) ? true : false,
                        ServerPath = swUpdateHelper.Softwares[i].MiniInstallerServer_path,
                        SoftwareName = "DDPM",
                        FileSavepath = swUpdateHelper.Softwares[i].InstallPath
                    };
                    if (SWUpdateInfo.NeedUpdated)
                    {
                        _SWUpdateInfoPackage.SWUpdateInfo.Add(SWUpdateInfo);
                    }
                }
                HandleUpdateInfo();
                _IsShowNotify = true;
                _isDefer = false;
                _isForce = false;
                _logs.DebugMsg_1(nameof(CheckUpdate) + " done.");
            }
            return Task.FromResult(new List<SWUpdateInfo>());
        }

        private void HandleUpdateInfo()
        {
            _logs.DebugMsg_1("HandleUpdateInfo");
            if (_SWUpdateInfoPackage != null && _DelaySWUpdateInfoPackage != null)
            {
                List<SWUpdateInfo> _ForceUpdates = new List<SWUpdateInfo>();
                bool isUpdate = false;//判斷是否強制更新
                bool isOnlyInfo = false;
                string s = "";
                if (_SWUpdateInfoPackage.SWUpdateInfo.Count <= 0)
                {
                    isOnlyInfo = true;
                    s = "no updates available.";
                }
                else
                {
                    foreach (SWUpdateInfo swUpdateInfo in _SWUpdateInfoPackage.SWUpdateInfo)
                    {
                        if (_isForce)
                        {
                            isUpdate = true;
                            s += $"{swUpdateInfo.SoftwareName} will be updated to {swUpdateInfo.TheLatestVersion}\n";
                        }
                        if (_DelaySWUpdateInfoPackage.SWUpdateInfo.Exists(o => o.Equals(swUpdateInfo)))
                        {
                            if (_DelaySWUpdateInfoPackage.SaveTime != null)
                            {
                                SWUpdateInfo? delayFUpdateInfo = _DelaySWUpdateInfoPackage.SWUpdateInfo.Find(o => o.Equals(swUpdateInfo));
                                if (delayFUpdateInfo != null)
                                {
                                    delayFUpdateInfo.ServerPath = swUpdateInfo.ServerPath;
                                    delayFUpdateInfo.SHA256 = swUpdateInfo.SHA256;
                                    delayFUpdateInfo.SHA512 = swUpdateInfo.SHA512;
                                    delayFUpdateInfo.Thumbprint = swUpdateInfo.Thumbprint;
                                    TimeSpan difference = DateTime.Now - (DateTime)_DelaySWUpdateInfoPackage.SaveTime;
                                    if (_isDefer)
                                    {
                                        s += $"{swUpdateInfo.SoftwareName} can be updated to {swUpdateInfo.TheLatestVersion}\n";
                                    }
                                    else if (difference.TotalHours >= 24 && _DelaySWUpdateInfoPackage.DelayTimesAvailable > 0)
                                    {
                                        s += $"{swUpdateInfo.SoftwareName} can be updated to {swUpdateInfo.TheLatestVersion}\n";
                                    }
                                    else if (difference.TotalHours >= 24 && _DelaySWUpdateInfoPackage.DelayTimesAvailable <= 0)
                                    {
                                        _ForceUpdates.Add(delayFUpdateInfo);
                                        isUpdate = true;
                                        s += $"{swUpdateInfo.SoftwareName} will be updated to {swUpdateInfo.TheLatestVersion}\n";
                                    }
                                }
                            }
                        }
                        else
                        {
                            s += $"{swUpdateInfo.SoftwareName} can be updated to {swUpdateInfo.TheLatestVersion}\n";
                        }
                    }
                }
                NotificationFWupdate("Updates info", s, isOnlyInfo, isUpdate);
                _logs.DebugMsg_1("HandleUpdateInfo done");
            }
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
                _logs.DebugMsg_1(nameof(DownloadAndInstall) + " start");
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
                //0926 Bruce Add Security
                if (!CheckFold(savePath, out string FolderInfo, out string PathSymbolicLinInfo))
                {
                    foreach (SWUpdateInfo swUpdateInfo in swUpdateInfos)
                    {
                        swUpdateInfo.SWUErrorCode = SWUErrorCode.FolderIsNotSafe;
                    }
                    _notificationStr = $"Software update unsuccessful.";
                    NotificationFWupdate("Error", _notificationStr);
                    _logs.DebugMsg_1(nameof(DownloadAndInstall) + " FolderIsNotSafe:" + FolderInfo + "--or--" + PathSymbolicLinInfo);
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
                    if (!CheckFold(savePath, out FolderInfo, out PathSymbolicLinInfo))
                    {
                        swUpdateInfos[i].SWUErrorCode = SWUErrorCode.FolderIsNotSafe;
                        _notificationStr = $"Software update unsuccessful.";
                        NotificationFWupdate("Error", _notificationStr);
                        _logs.DebugMsg_1(nameof(DownloadAndInstall) + " FolderIsNotSafe:" + FolderInfo + "--or--" + PathSymbolicLinInfo);
                        continue;
                    }
                    _downloadTimer = new Timer();
                    _downloadTimer.Interval = 1000;
                    _downloadTimer.Elapsed += new ElapsedEventHandler(DownloadTimer_Elapsed);
                    _downloadTimer.Start();
                    download = new Download(_logs);
                    string downloadInfo = "";
                    // 將儲存路徑與從 URL 中提取的檔案名稱組合
                    string _installationFileStoragePath = Path.Combine(savePath + Path.GetFileName(url));
                    bool downloadRet = download.DownloadFile(url, _installationFileStoragePath, out downloadInfo);
                    _downloadTimer.Stop();
                    UpdateProgressInfo updateProgressInfo = new UpdateProgressInfo()
                    {
                        DeviceName = swUpdateInfos[i].SoftwareName,
                        TheLatestVersion = swUpdateInfos[i].TheLatestVersion,
                        ProcessName = "Downloading",
                        ProcessProgress = 100,
                    };
                    if (!downloadRet)
                    {
                        if (downloadInfo.Equals("CA check fail"))
                        {
                            swUpdateInfos[i].SWUErrorCode = SWUErrorCode.CAFail;
                            _notificationStr = $"Software update unsuccessful.";
                            NotificationFWupdate("Error", _notificationStr);
                        }
                        else if (downloadInfo.Equals("Network fail"))
                        {
                            swUpdateInfos[i].SWUErrorCode = SWUErrorCode.NetworkDisconnection;
                            _notificationStr = $"Update failed due to network error. Try again.";
                            NotificationFWupdate("Error", _notificationStr);
                        }
                        _logs.DebugMsg_1(swUpdateInfos[i].SoftwareName + " Download File Fail");
                        continue;
                    }
                    string extractPath = Path.Combine(savePath + Path.GetFileName(url).Substring(0, Path.GetFileName(url).Length - 4));
                    if (!Directory.Exists(extractPath))
                    {
                        Directory.CreateDirectory(extractPath);
                    }
                    if (!CheckFold(extractPath, out FolderInfo, out PathSymbolicLinInfo))
                    {
                        swUpdateInfos[i].SWUErrorCode = SWUErrorCode.FolderIsNotSafe;
                        _logs.DebugMsg_1(swUpdateInfos[i].SoftwareName + " FolderIsNotSafe:" + FolderInfo + "--or--" + PathSymbolicLinInfo);
                        _notificationStr = $"Software update unsuccessful.";
                        NotificationFWupdate("Error", _notificationStr);
                        continue;
                    }
                    /*Waiting for signature, temporary annotation
                    if (!CheckSHA(_installationFileStoragePath, out string FileCAInfo))
                    {
                        swUpdateInfos[i].SWUErrorCode = SWUErrorCode.FileCheckFail;
                        _logs.DebugMsg_1(swUpdateInfos[i].SoftwareName + " File check fail. Ex:" + FileCAInfo);
                        _notificationStr = $"Software update unsuccessful.";
                        NotificationFWupdate("Error", _notificationStr);
                        continue;
                    }*/
                    string exeFilePath;
                    if (!Unzip(_installationFileStoragePath, extractPath, out exeFilePath))
                    {
                        _logs.DebugMsg_1(_SWUpdateInfo.SoftwareName + " Unzip Faile");
                        _notificationStr = $"Software update unsuccessful.";
                        NotificationFWupdate("Error", _notificationStr);
                        continue;
                    }
                    swUpdateInfos[i].InstallPaths = exeFilePath;
                    swUpdateInfos[i].SWUErrorCode = Install(swUpdateInfos[i]).Result;
                    if (swUpdateInfos[i].SWUErrorCode == SWUErrorCode.NoError)
                    {
                        NotificationFWupdate("SW info", _notificationStr);
                    }
                    else
                    {
                        NotificationFWupdate("Error", _notificationStr);
                    }
                }
                _logs.DebugMsg_1(nameof(DownloadAndInstall) + " done");
                if (_DelaySWUpdateInfoPackage != null && _DelaySWUpdateInfoPackage.SWUpdateInfo.Count <= 0)
                {
                    _DelaySWUpdateInfoPackage = new SWUpdateInfoPackage();
                    CallSaveUpdateInfoPackage?.AsyncFireAndForget(this, _DelaySWUpdateInfoPackage, System.Threading.CancellationToken.None);
                }
                else if (_DelaySWUpdateInfoPackage != null)
                {
                    CallSaveUpdateInfoPackage?.AsyncFireAndForget(this, _DelaySWUpdateInfoPackage, System.Threading.CancellationToken.None);
                }
                _IsShowNotify = true;
                _isDefer = false;
                _isForce = false;
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
                _logs.DebugMsg_1(nameof(DownloadAndInstall) + " Error：Update failed due to network error. Try again. ex:" + ex.Message); // 輸出錯誤訊息
                _IsShowNotify = true;
                _isDefer = false;
                _isForce = false;
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
                if (download.DownloadFileStream != null)
                {
                    UpdateProgressInfo updateProgressInfo = new UpdateProgressInfo()
                    {
                        DeviceName = _SWUpdateInfo.SoftwareName,
                        TheLatestVersion = _SWUpdateInfo.TheLatestVersion,
                        ProcessName = "Downloading",
                        ProcessProgress = download.GetProgress(),
                    };
                }
            }
        }
        /// <summary>
        /// 定期檢查更新排程
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CheckUpdateScheduleTimer_Elapsed(object? sender, ElapsedEventArgs e)
        {
            TimeSpan difference = DateTime.Now - _SWUpdateInfoPackage.TheLastCheckTime;
            int checkTime = 5;
            if (difference.TotalMinutes > checkTime)
            {
                CollCheckUpdate?.AsyncFireAndForget(this, e, System.Threading.CancellationToken.None);
            }
        }

        /// <summary>
        /// 跳出通知
        /// </summary>
        private void NotificationFWupdate(string title, string info, bool isInfo = true, bool isOnlyUpdate = false, bool stayOpen = false, int timeout = 5)
        {
            if (!string.IsNullOrEmpty(info) && _IsShowNotify)
            {
                PopupContentPackage popupContentPackage = new PopupContentPackage()
                {
                    Title = title,
                    Info = info,
                    IsInfo = isInfo,
                    IsOnlyUpdate = isOnlyUpdate,
                    StayOpen = stayOpen,
                    Timeout = timeout,
                    Object = _SWUpdateInfoPackage
                };
                CallPopup?.AsyncFireAndForget(this, popupContentPackage, System.Threading.CancellationToken.None);
            }
        }

        /// <summary>
        /// NotificationFWupdate 延遲更新事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void DelayEvent(object e)
        {
            _logs.DebugMsg_1(nameof(DelayEvent));
            // 將 e 轉換成 JSON 字串
            string json = JsonConvert.SerializeObject(e);
            // 將 JSON 字串轉換成 FWUpdateInfoPackage 對象
            SWUpdateInfoPackage sWUpdateInfoPackage = JsonConvert.DeserializeObject<SWUpdateInfoPackage>(json);
            if (sWUpdateInfoPackage != null)
            {
                if (_DelaySWUpdateInfoPackage != null && _DelaySWUpdateInfoPackage.SaveTime != null && _DelaySWUpdateInfoPackage.SWUpdateInfo.Count > 0)
                {
                    foreach (SWUpdateInfo newSWUpdateInfo in sWUpdateInfoPackage.SWUpdateInfo)
                    {
                        if (!_DelaySWUpdateInfoPackage.SWUpdateInfo.Exists(o => o.Equals(newSWUpdateInfo)))
                        {
                            newSWUpdateInfo.ServerPath = "";
                            newSWUpdateInfo.SHA256 = "";
                            newSWUpdateInfo.SHA512 = "";
                            newSWUpdateInfo.Thumbprint = "";
                            _DelaySWUpdateInfoPackage.SWUpdateInfo.Add(newSWUpdateInfo);
                        }
                    }
                    _DelaySWUpdateInfoPackage.DelayTimesAvailable--;
                    CallSaveUpdateInfoPackage?.AsyncFireAndForget(this, _DelaySWUpdateInfoPackage, System.Threading.CancellationToken.None);
                }
                else if (_DelaySWUpdateInfoPackage != null && _DelaySWUpdateInfoPackage.SaveTime == null)
                {
                    _DelaySWUpdateInfoPackage = _SWUpdateInfoPackage;
                    foreach (SWUpdateInfo newSWUpdateInfo in _DelaySWUpdateInfoPackage.SWUpdateInfo)
                    {
                        newSWUpdateInfo.ServerPath = "";
                        newSWUpdateInfo.SHA256 = "";
                        newSWUpdateInfo.SHA512 = "";
                        newSWUpdateInfo.Thumbprint = "";
                    }
                    _DelaySWUpdateInfoPackage.DelayTimesAvailable = 2;
                    _DelaySWUpdateInfoPackage.SaveTime = DateTime.Now;
                    CallSaveUpdateInfoPackage?.AsyncFireAndForget(this, _DelaySWUpdateInfoPackage, System.Threading.CancellationToken.None);
                }
            }
        }
        /// <summary>
        /// NotificationFWupdate 立即更新事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void UpdateEvent(object e)
        {
            _logs.DebugMsg_1(nameof(UpdateEvent));
            // 將 e 轉換成 JSON 字串
            string json = JsonConvert.SerializeObject(e);
            // 將 JSON 字串轉換成 FWUpdateInfoPackage 對象
            SWUpdateInfoPackage sWUpdateInfoPackage = JsonConvert.DeserializeObject<SWUpdateInfoPackage>(json);
            _logs.DebugMsg_1(sWUpdateInfoPackage.SWUpdateInfo.Count.ToString());
            List<SWUpdateInfo> sWUpdateInfo = sWUpdateInfoPackage.SWUpdateInfo;
            DownloadAndInstall(sWUpdateInfo, "").Wait();
        }
        private void OnPowerModeChanged(object sender, PowerModeChangedEventArgs e)
        {
            switch (e.Mode)
            {
                case PowerModes.Suspend:
                    _checkUpdateScheduleTimer.Stop();
                    _logs.DebugMsg_1("PC is sleep");
                    break;

                case PowerModes.Resume:
                    _checkUpdateScheduleTimer.Start();
                    _logs.DebugMsg_1("PC is wakeup");
                    break;

                case PowerModes.StatusChange:
                    _logs.DebugMsg_1("PC is status change");
                    break;
            }
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
                WTSFunction.StartProcessAndBypassUACWithAdmin(miniInstallPath, out procInfo);
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
                _logs.DebugMsg_1($"{nameof(Install)} done");
                return Task.FromResult(_updateErrorCode);
            }
            catch (Exception ex)
            {
                _updateErrorCode = SWUErrorCode.Unknow;
                _logs.DebugMsg_1(nameof(Install) + " Error:" + ex.ToString());
                _notificationStr = $"{_SWUpdateInfo.SoftwareName} Service not running. Try again.";
                return Task.FromResult(_updateErrorCode);
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
                    _logs.DebugMsg_1(nameof(DownloadAndInstall) + " FolderIsNotSafe:" + pathSymbolicLinInfo + " Retry:" + (count++));
                }
                folderValid = DDPMFileSecurity.IsFolderPathValid(path, out folderInfo) && folderValid;
                if (!folderValid)
                {
                    _logs.DebugMsg_1(nameof(DownloadAndInstall) + " FolderIsNotSafe:" + folderInfo + " Retry:" + (count++));
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
            Unzip unzip = new Unzip(_logs);
            exeFilePath = "";
            if (unzip.CheckFileIsZip(filePath))
            {
                if (!unzip.ExecuteUnzip(filePath, extractPath, out exeFilePath))
                {
                    _logs.DebugMsg_1(_SWUpdateInfo.SoftwareName + " Unzip Faile");
                }
                /*Waiting for signature, temporary annotation
                 if (!string.IsNullOrEmpty(exeFilePath))
                {
                    CertificateCheck certificateCheck = new CertificateCheck();
                    if (!certificateCheck.CheckFile_Thumbprint(exeFilePath, _SWUpdateInfo.Thumbprint, out string FileCAInfo))
                    {
                        _logs.DebugMsg_1(_SWUpdateInfo.SoftwareName + " File check fail. Ex:" + FileCAInfo);
                        _SWUpdateInfo.SWUErrorCode = SWUErrorCode.FileCheckFail;
                    }
                    else
                    {
                        ret = true;
                    }
                }*/
                ret = true;//Waiting for signature, to remove
            }
            else
            {
                exeFilePath = filePath;
                ret = true;
            }
            return ret;
        }
    }
}