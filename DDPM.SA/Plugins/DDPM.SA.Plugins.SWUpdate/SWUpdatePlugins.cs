using Dell.Client.Framework.Common.Annotations;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.Interfaces;
using Microsoft;
using Dell.Client.Framework.Common.PluginConditions;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text.RegularExpressions;
using System.Text;
using System.Timers;
using System.Xml;
using DDPM.SA.Common;
using VcpCore.Common;
using IDs = DDPM.SA.Common.IDs;
using System.Threading.Tasks;
using System.Collections.Generic;
using DDPM.SA.Common.Popup;
using System;
using Dell.Client.Framework.Common.Extensions;
using Microsoft.Toolkit.Uwp.Notifications;
using Microsoft.Win32;
using System.Management;
using System.Net.Http;
using System.Threading;
using Timer = System.Timers.Timer;
using DDPM.SA.Plugins.SWUpdate;
using Newtonsoft.Json;
using System.Linq;

namespace DDPM.SA.Plugins.User.SWUpdate
{
    [Plugin(IDs.SWUpdate_PLUGIN_ID, pluginName, PluginOrderGroupType.Core, Version = pluginVersion)]
    [Descriptor(Description = pluginDescription)]
    [Publisher(Name = publisherCompany, Website = publisherWebsite, Support = publisherSupport)]
    [PublishedUnelevatedInterface(new[] { typeof(ISWUpdateService) })]
    [DependencyKnownTypes(new[] { typeof(ISWUpdateService) })]
    public class SWUpdatePlugins : BaseAgentPlugin, ISWUpdateService
    {
        //0531 Bruce 因應IL的現有安裝包修改底層邏輯，FWUpdatePlugins.cs有稍作大改
        //0531 Bruce 因使用者可能在執行前將裝置移除，故將檢查是否延期的功能修改到底層的排程中
        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);
        #region Private Members
        private const string pluginName = "SWUpdatePlugin";
        private const string pluginVersion = "1.0.0";
        private const string pluginDescription = "This plugin implements SW Update Plugin.";
        private const string publisherCompany = "Wistron";
        private const string publisherWebsite = "https://www.wistron.com";
        private const string publisherSupport = "This plugin implements SW Update Plugin.";

        private IAgent _agent;
        #endregion

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
        long? _size = null;
        FileStream? _fileStream = null;
        //安裝更新檔使用的命名管道伺服器
        Timer _downloadTimer = new Timer();
        Timer _checkUpdateScheduleTimer;
        string _notificationStr = "";
        SWUErrorCode _updateErrorCode;
        bool _IsShowNotify = true;
        bool _isDefer = false;
        bool _isForce = false;
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
        #endregion

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
        #endregion
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
        #endregion
        protected override void OnPluginStarting()
        {
            _agent.PluginManager.PluginsStarted += PluginManagerOnPluginsStarted;
            PluginCondition = new PluginStartedCondition();
            Console.WriteLine("SWUpdate plugin report started");
        }
        #endregion
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
        public Task<SWUpdateInfoPackage> GetSWUpdateInfo(bool isShowNotify, bool isForce, bool isDefer)
        {
            _isDefer = isDefer;
            _isForce = isForce;
            _ = CheckUpdate(isShowNotify).Result;
            return Task.FromResult(_SWUpdateInfoPackage);
        }
        /// <summary>
        /// 檢查更新資訊
        /// </summary>
        /// <param name="updateHelper">IL的更新資訊</param>
        /// <param name="isShowNotify">是否顯示右下角通知圖示</param>
        /// <returns>回傳裝置資訊表(如果有需強制安裝更新的話，該裝置資訊表會被寫入對應裝置的安裝結果)</returns>
        public Task<List<SWUpdateInfo>> CheckUpdate(bool isShowNotify)
        {
            _IsShowNotify = isShowNotify;
            _logs.DebugMsg_1(nameof(CheckUpdate) + " start");
            _SWUpdateInfoPackage = new SWUpdateInfoPackage();
            _SWUpdateInfoPackage.TheLastCheckTime = DateTime.Now;
            return Task.FromResult(new List<SWUpdateInfo>());
            //if (updateHelper.UpdateItems != null && updateHelper.UpdateItems.Count > 0)
            {
                //for (int i = 0; i < updateHelper.UpdateItems.Count; i++)
                {
                    string newVer = "65535";
                    string CurrentVersion = "123";
                    if (!int.TryParse(newVer, out _))
                    {
                        newVer = Convert.ToInt32(newVer, 16).ToString();
                    }
                    SWUpdateInfo SWUpdateInfo = new SWUpdateInfo()
                    {
                        TheLatestVersion = Regex.Replace(Convert.ToInt32(newVer).ToString("D4"), ".{1}", "$0.").Substring(0, (Convert.ToInt32(newVer).ToString("D4").Length * 2) - 1),
                        SoftwareVersion = Regex.Replace(Convert.ToInt32(CurrentVersion).ToString("D4"), ".{1}", "$0.").Substring(0, (Convert.ToInt32(CurrentVersion).ToString("D4").Length * 2) - 1),
                        NeedUpdated = int.Parse(newVer) > int.Parse(CurrentVersion) ? true : false,
                        ServerPath = "",
                        FileSavepath = "C:\\Users\\Diablo16\\Downloads\\DDPM-Setup v2.0.0.28-NKVM-pwdDDPM\\DDPM-Setup v2.0.0.28-NKVM-TestingOnly.exe",
                        SoftwareName = "DDPM"
                    };
                    _SWUpdateInfoPackage.SWUpdateInfo.Add(SWUpdateInfo);
                }
                HandleUpdateInfo();
                _IsShowNotify = true;
                _isDefer = false;
                _isForce = false;
                _logs.DebugMsg_1(nameof(CheckUpdate) + " done.");
            }
            return Task.FromResult(new List<SWUpdateInfo>());
        }
        void HandleUpdateInfo()
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
            _notificationStr = "";
            int dockCount = 0;
            try
            {
                CACertificateCheck caCheck = new CACertificateCheck(_logs);
                for (int i = 0; i < swUpdateInfos.Count; i++)
                {
                    _notificationStr = "";
                    _SWUpdateInfo = swUpdateInfos[i];
                    _logs.DebugMsg_1(swUpdateInfos[i].SoftwareName + nameof(DownloadAndInstall) + " start");
                    _updateErrorCode = SWUErrorCode.Unknow;
                    swUpdateInfos[i].SWUErrorCode = _updateErrorCode;
                    //暫時註解 因現在使用測試伺服器故先將檢查CA註解
                    //if (caCheck.CheckCA(_sWUpdateInfo.ServerPath))
                    {
                        string url = swUpdateInfos[i].ServerPath;
                        //0627 Bruce 因CLI可能會自訂路徑顧新增傳入參數，變新增判斷
                        string savePath;
                        if (string.IsNullOrEmpty(installPath))
                        {
                            savePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\" + "Dell Display and Peripheral Manager" + "\\" + swUpdateInfos[i].FileSavepath + "\\";
                        }
                        else
                        {
                            savePath = installPath;
                        }
                        _logs.DebugMsg_1($"savePath: {savePath}");
                        _downloadTimer = new Timer();
                        _downloadTimer.Interval = 1000;
                        _downloadTimer.Elapsed += new ElapsedEventHandler(DownloadTimer_Elapsed);

                        //測試用，因現在使用測試伺服器，故先使用以下兩行繞過SSL檢查
                        HttpClientHandler handler = new HttpClientHandler();
                        handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true; //Dean 0626 SAST vulnerability
                                                                                                                            //Should enable server certificate validation on this SSL/TLS connection before formal release

                        HttpClient client = new HttpClient(handler);
                        client.Timeout = TimeSpan.FromMinutes(1);
                        // 發送 HTTP GET 請求到指定的 URL
                        HttpResponseMessage response = client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead).Result;
                        if (!Directory.Exists(savePath))
                        {
                            Directory.CreateDirectory(savePath);
                        }
                        // 將儲存路徑與從 URL 中提取的檔案名稱組合
                        string _installationFileStoragePath = Path.Combine(savePath + Path.GetFileName(url));
                        // 從 URL 中取得回應標頭
                        var header = client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead).Result;
                        // 從回應標頭中提取檔案大小
                        _size = header.Content.Headers.ContentLength;
                        // 取得包含 URL 內容的串流
                        var stream = client.GetStreamAsync(url).Result;
                        // 建立檔案串流以將下載的內容寫入
                        _fileStream = File.Create(_installationFileStoragePath);
                        _downloadTimer.Start();
                        // 將串流的內容複製到檔案中
                        stream.CopyToAsync(_fileStream).Wait();
                        _downloadTimer.Stop();
                        _fileStream.Close();
                        string exeFilePath;
                        Unzip(_fileStream.Name, _fileStream.Name.Substring(0, _fileStream.Name.Length - 4), out exeFilePath);

                        //測試用，OTATestClient(模擬正常安裝包流程)
                        //exeFilePath = "C:\\FW_SW_ICC_Update\\OTATestSampleCode From_IndiLogic\\src\\OTATestClient\\bin\\Debug\\OTATestClient.exe";

                        _fileStream = null;
                        //暫時註解 等待check sha512和CA
                        //if (caCheck.CheckFileCA(exeFilePath))
                        {
                            //Bruce 暫時使用Hotcode方式
                            swUpdateInfos[i].InstallPaths = "C:\\Users\\Diablo16\\Downloads\\DDPM-Setup v2.0.0.28-NKVM-pwdDDPM\\DDPM-Setup v2.0.0.28-NKVM-TestingOnly.exe";//exeFilePath;
                            swUpdateInfos[i].SWUErrorCode = Install(swUpdateInfos[i]);
                        }
                        if (swUpdateInfos[i].SWUErrorCode == SWUErrorCode.NoError)
                        {
                            NotificationFWupdate("SW info", _notificationStr);
                        }
                        else
                        {
                            NotificationFWupdate("Error", _notificationStr);
                        }
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
                foreach (SWUpdateInfo SoftwareInfo in swUpdateInfos)
                {
                    SoftwareInfo.SWUErrorCode = SWUErrorCode.NetworkDisconnection;
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

        bool Unzip(string zipFilePath, string extractPath, out string exeFilePath)
        {
            try
            {
                _logs.DebugMsg_1(nameof(Unzip) + " start");
                // 如果目標目錄不存在，則建立目錄
                if (!Directory.Exists(extractPath))
                {
                    Directory.CreateDirectory(extractPath);
                }
                // 解壓縮zip檔案，並覆蓋現有檔案
                ZipFile.ExtractToDirectory(zipFilePath, extractPath, true);
                _logs.DebugMsg_1(nameof(Unzip) + " done");
                exeFilePath = GetExeFilePath(extractPath);
                return true;
            }
            catch (Exception ex)
            {
                _logs.DebugMsg_1(nameof(Unzip) + "Unzip fail: " + ex.Message);
                exeFilePath = "";
                return false;
            }
        }
        string GetExeFilePath(string directory)
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
        /// <summary>
        /// 下載進度回傳事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void DownloadTimer_Elapsed(object? sender, ElapsedEventArgs e)
        {
            if (_fileStream != null)
            {
                if (_size == null)
                {
                    _size = 1;
                }
                double d = Math.Round(((double)_fileStream.Length / (double)_size) * 100.0, 2);
            }
        }
        /// <summary>
        /// 定期檢查更新排程
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void CheckUpdateScheduleTimer_Elapsed(object? sender, ElapsedEventArgs e)
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
        void NotificationFWupdate(string title, string info, bool isInfo = true, bool isOnlyUpdate = false, bool stayOpen = false, int timeout = 5)
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
                            _DelaySWUpdateInfoPackage.SWUpdateInfo.Add(newSWUpdateInfo);
                        }
                    }
                    _DelaySWUpdateInfoPackage.DelayTimesAvailable--;
                    CallSaveUpdateInfoPackage?.AsyncFireAndForget(this, _DelaySWUpdateInfoPackage, System.Threading.CancellationToken.None);
                }
                else if (_DelaySWUpdateInfoPackage != null && _DelaySWUpdateInfoPackage.SaveTime == null)
                {
                    _DelaySWUpdateInfoPackage = _SWUpdateInfoPackage;
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
            List<SWUpdateInfo> sWUpdateInfo = sWUpdateInfoPackage.SWUpdateInfo;
            DownloadAndInstall(sWUpdateInfo, "").Wait();
        }
        void OnPowerModeChanged(object sender, PowerModeChangedEventArgs e)
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
        SWUErrorCode Install(SWUpdateInfo swUpdateInfo)
        {
            try
            {
                _logs.DebugMsg_1(swUpdateInfo.SoftwareName + nameof(Install) + " start");
                // 要運行的安裝程式路徑和命令行參數
                string arguments = "";//"/silent" + " /pipename:" + _namedPipeName;

                ProcessStartInfo startInfo = new ProcessStartInfo()
                {
                    UseShellExecute = true,
                    Verb = "runas",
                    WindowStyle = ProcessWindowStyle.Normal,
                    FileName = swUpdateInfo.InstallPaths,
                    Arguments = arguments,
                    WorkingDirectory = Path.GetDirectoryName(swUpdateInfo.InstallPaths),
                    CreateNoWindow = false
                };
                Process _clientProcess = new Process();
                _clientProcess.StartInfo = startInfo;
                /*之後搬到System level 後要使用的虛擬代理人方法
                var sessionId = Kernel32.WTSGetActiveConsoleSessionId();
                if (sessionId is Advapi32.InvalidSessionId) throw new InvalidOperationException($"Cannot get session id");
                IntPtr token = UserImpersonator.GetTokenFromSession(sessionId, systemUser: false);
                UserImpersonator.RunAsUser(token, () =>
                {
                    using (Process clientProcess = new Process())
                    {
                        _clientProcess = new Process();
                        _clientProcess.StartInfo.UseShellExecute = false;
                        _clientProcess.StartInfo.FileName = fwUpdateInfo.InstallPaths;
                        _clientProcess.StartInfo.WorkingDirectory = Path.GetDirectoryName(_clientProcess.StartInfo.FileName);
                        _clientProcess.StartInfo.Arguments = arguments;
                    }
                });*/
                _clientProcess.Start();
                //_clientProcess.WaitForExit();
                _updateErrorCode = SWUErrorCode.NoError;
                return _updateErrorCode;
            }
            catch (Exception ex)
            {
                _updateErrorCode = SWUErrorCode.Unknow;
                _logs.DebugMsg_1(swUpdateInfo.SoftwareName + nameof(Install) + " Error:" + ex.ToString());
                _notificationStr = $"{_SWUpdateInfo.SoftwareName} Service not running. Try again.";
                return _updateErrorCode;
            }
        }
    }
}
