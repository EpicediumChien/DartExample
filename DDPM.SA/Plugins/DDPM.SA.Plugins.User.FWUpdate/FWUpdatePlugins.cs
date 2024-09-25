using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

#if WINDOWS

using System.Diagnostics;

#endif

using System.Timers;
using System.Text;
using System.Xml;
using System.Security.Policy;
using DDPM.SA.Common;
using Dell.Client.Framework.Common.Annotations;
using Dell.Client.Framework.Common.Extensions;
using Dell.Client.Framework.Interfaces;
using Timer = System.Timers.Timer;
using IDs = DDPM.SA.Common.IDs;
using System.Text.RegularExpressions;
using VcpCore.Common;
using Microsoft.Win32;
using System.Runtime.InteropServices;
using Microsoft.Toolkit.Uwp.Notifications;
using System.Runtime.Versioning;
using DPeMPublic.Common.Enums;
using System.Threading;
using System.Management;
using Dell.Client.Framework.Common.PluginConditions;
using Dell.Client.Framework.Common;
using Newtonsoft.Json;
using System.Linq;
using PInvoke;
using DDPM.SA.Common.Settings;
using Dell.Client.Framework.Security.Interfaces;
using Dell.Client.Framework.Security;
using System.Security;
using DDPM.SA.Common.Method;
using DDPM.SA.Common.Security;

namespace DDPM.SA.Plugins.User.FWUpdate
{
    [Plugin(IDs.FWUPDATE_PLUGIN_ID, pluginName, PluginOrderGroupType.Core, Version = pluginVersion)]
    [Descriptor(Description = pluginDescription)]
    [Publisher(Name = publisherCompany, Website = publisherWebsite, Support = publisherSupport)]
    [PublishedUnelevatedInterface(new[] { typeof(IFWUpdateService) })]
    [DependencyKnownTypes(new[] { typeof(IFWUpdateService) })]
    public class FWUpdatePlugins : BaseAgentPlugin, IFWUpdateService
    {
        //0531 Bruce 因應IL的現有安裝包修改底層邏輯，FWUpdatePlugins.cs有稍作大改
        //0531 Bruce 因使用者可能在執行前將裝置移除，故將檢查是否延期的功能修改到底層的排程中
        [DllImport("user32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        private static bool _ShowWindow(IntPtr hWnd, int nCmdShow)
        {
            return ShowWindow(hWnd, nCmdShow);
        }

        [DllImport("user32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        private static bool _SetForegroundWindow(IntPtr hWnd)
        {
            return SetForegroundWindow(hWnd);
        }

        #region Private Members

        private const string pluginName = "FWUpdatePlugin";
        private const string pluginVersion = "1.0.0";
        private const string pluginDescription = "This plugin implements FW Update Plugin.";
        private const string publisherCompany = "Wistron";
        private const string publisherWebsite = "https://www.wistron.com";
        private const string publisherSupport = "This plugin implements FW Update Plugin.";

        private IAgent _agent;

        #endregion Private Members

        public const string PluginLogId = "FWUpdate";

        private Logs _logs;

        /// <summary>
        /// 現在正在進行下載或安裝流程的裝置資訊
        /// </summary>
        private FWUpdateInfo _fWUpdateInfo = new FWUpdateInfo();

        /// <summary>
        /// 給UI或是CLI的全部韌體更新包
        /// </summary>
        private FWUpdateInfoPackage _fWUpdateInfoPackage;

        /// <summary>
        /// 從SettingsManager取得的延遲更新包，用於比對是否延遲次數為0
        /// </summary>
        private FWUpdateInfoPackage _DelayFWUpdateInfoPackage;

        /// <summary>
        /// 從DeviceManager取得的連接的裝置資訊列表，用於更新韌體前確認是否有插入多個Dock
        /// </summary>
        private List<DeviceInfo> _DeviceInfos;

        /// <summary>
        /// 要取得更新的裝置列表
        /// </summary>
        private List<DeviceType>? _DeviceTypeList;

        private Download? download = null;

        //安裝更新檔使用的命名管道伺服器
        private NamedPipeStreamServer? _namedPipeServer;

        private Process _clientProcess = new Process();
        private Timer _downloadTimer = new Timer();
        private Timer _checkUpdateScheduleTimer;
        private Timer _checkUODTimer;
        private Timer _timerTimeOut;
        private string _notificationStr = "";
        private FWUErrorCode _updateErrorCode;
        private bool _IsShowNotify = true;
        private bool _isDefer = false;
        private bool _isForce = false;

        /// <summary>
        /// 用於倒數次數計算
        /// </summary>
        private int _timeOutCount;

        /// <summary>
        /// 用於設定逾時時間預設60次/秒
        /// </summary>
        private int _fwTimeOutCount = 60;

        #region Events

        /// <summary>
        /// 呼叫DeviceManager呼叫我的檢查更新方法，用於排成定期檢查
        /// </summary>
        public event EventHandler? CollCheckUpdate;

        /// <summary>
        /// 回傳更新事件進度
        /// </summary>
        public event EventHandler<FWUpdateInfo>? ProgressUpdate_Notify;

        /// <summary>
        /// 將延遲更新包傳給DeviceManager進行儲存
        /// </summary>
        public event EventHandler<FWUpdateInfoPackage>? CallSaveUpdateInfoPackage;

        /// <summary>
        /// 跟DeviceManager取得DeviceInfos
        /// </summary>
        public event EventHandler? CallGetDeviceInfos;

        /// <summary>
        /// 將使用UOD模式資訊包傳給DeviceManager進行儲存
        /// </summary>
        public event EventHandler<DokcUODUpdateInfoPackage>? CallSaveUODFWDeviceInfos;

        /// <summary>
        /// 根據排程時間，呼叫DeviceManager檢查UOD模式資訊包
        /// </summary>
        public event EventHandler? CallCheckUODFWInfos;

        /// <summary>
        /// 回傳更新事件結果提供給CLI使用
        /// </summary>
        public event EventHandler<List<FWUpdateInfo>> DownloadAndInstall_Result_Notify;

        /// <summary>
        /// 呼叫Popup通知
        /// </summary>
        public event EventHandler<PopupContentPackage> CallPopup;

        #endregion Events

        public FWUpdatePlugins(IAgent agent) : base(agent, PluginLogId)
        {
            _agent = agent;
            _logs ??= new Logs(Log, PluginLogId);
            SystemEvents.PowerModeChanged += OnPowerModeChanged;
            _fWUpdateInfoPackage = new FWUpdateInfoPackage();
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
                Console.WriteLine("IFWUpdateService plugin started.");
            }
        }

        #endregion Event Handler

        protected override void OnPluginStarting()
        {
            _agent.PluginManager.PluginsStarted += PluginManagerOnPluginsStarted;
            PluginCondition = new PluginStartedCondition();
            Console.WriteLine("FWUpdate plugin report started");
        }

        #endregion Overriding methods

        /// <summary>
        /// 啟動檢查更新排程
        /// </summary>
        public void StartCheckUpdateScheduleTimer()
        {
            _checkUpdateScheduleTimer.Start();
        }

        public void SetDeviceinfo(List<DeviceInfo> DeviceInfos)
        {
            if (DeviceInfos != null && DeviceInfos.Count > 0)
            {
                _DeviceInfos = DeviceInfos;
            }
        }

        public void CheckUODFWUInfo(DokcUODUpdateInfoPackage UODFWUInfo, List<DeviceInfo>? DeviceInfos)
        {
            string s = "";
            if (DeviceInfos != null && DeviceInfos.Count > 0)
            {
                foreach (DeviceInfo deviceInfo in DeviceInfos)
                {
                    if (GetDevicePNPDeviceID(deviceInfo.Name).Equals(UODFWUInfo.FWUpdateInfo.PNPDeviceID))
                    {
                        string Ver = deviceInfo.FirmwareVersion;
                        if (!int.TryParse(Ver, out _))
                        {
                            Ver = Convert.ToInt32(Ver, 16).ToString();
                        }
                        string deviceVersion = Regex.Replace(Convert.ToInt32(Ver).ToString("D4"), ".{1}", "$0.").Substring(0, (Convert.ToInt32(Ver).ToString("D4").Length * 2) - 1);
                        if (deviceVersion.Equals(UODFWUInfo.FWUpdateInfo.TheLatestVersion))
                        {
                            s += $"{deviceInfo.Name} UOD update completed.";
                        }
                        else
                        {
                            s += $"{deviceInfo.Name} UOD update fail.";
                        }
                        UODFWUInfo = new DokcUODUpdateInfoPackage();
                        CallSaveUODFWDeviceInfos?.AsyncFireAndForget(this, UODFWUInfo, System.Threading.CancellationToken.None);
                        _checkUODTimer.Stop();
                        _checkUODTimer = null;
                    }
                }
            }
            else if (!string.IsNullOrEmpty(UODFWUInfo.FWUpdateInfo.PNPDeviceID))
            {
                TimeSpan difference = new TimeSpan(0);
                if (UODFWUInfo.SaveTime != null)
                {
                    difference = DateTime.Now - (DateTime)UODFWUInfo.SaveTime;
                }
                if (UODFWUInfo != null)
                {
                    if (_checkUODTimer == null)
                    {
                        _checkUODTimer = new Timer();
                        _checkUODTimer.Interval = TimeSpan.FromMinutes(0.5).TotalMilliseconds;
                        _checkUODTimer.Elapsed += new ElapsedEventHandler(CheckDockUODScheduleTimer_Elapsed);
                        _checkUODTimer.Start();
                    }
                    if (UODFWUInfo.SaveTime == null)
                    {
                        s += "Dock FW is loaded. Disconnect dock for completing FW application and reconnect dock after 1 min.";
                        UODFWUInfo.SaveTime = DateTime.Now;
                        CallSaveUODFWDeviceInfos?.AsyncFireAndForget(this, UODFWUInfo, System.Threading.CancellationToken.None);
                    }
                    else
                    {
                        if (difference.TotalHours >= 24)
                        {
                            s += "Dock FW is loaded. Disconnect dock for completing FW application and reconnect dock after 1 min.";
                            UODFWUInfo.SaveTime = DateTime.Now;
                            CallSaveUODFWDeviceInfos?.AsyncFireAndForget(this, UODFWUInfo, System.Threading.CancellationToken.None);
                        }
                    }
                }
            }
            if (!string.IsNullOrEmpty(s))
            {
                NotificationFWupdate("Dock UOD FW update info", s);
            }
        }

        /// <summary>
        /// 設定檔儲存的延遲更新資訊包
        /// </summary>
        /// <param name="DelayFWUpdateInfoPackage">設定檔儲存的延遲更新資訊包</param>
        public void SetDelayFWUpdateInfoPackage(FWUpdateInfoPackage DelayFWUpdateInfoPackage)
        {
            if (DelayFWUpdateInfoPackage != null)
            {
                _DelayFWUpdateInfoPackage = DelayFWUpdateInfoPackage;
            }
            else
            {
                _DelayFWUpdateInfoPackage = new FWUpdateInfoPackage();
            }
        }

        /// <summary>
        /// 取得更新的資訊包
        /// </summary>
        /// <param name="updateHelper">IL的更新資訊</param>
        /// <param name="isShowNotify">是否顯示右下角通知圖示</param>
        /// <returns>回傳更新資訊包</returns>
        public Task<FWUpdateInfoPackage> GetFWUpdateInfo(UpdateHelper updateHelper, bool isShowNotify, bool isForce, bool isDefer, List<DeviceType>? deviceTypeList, bool isUODMode, DisplayUpdateHelper displayUpdateHelper)
        {
            _isDefer = isDefer;
            _isForce = isForce;
            _DeviceTypeList = deviceTypeList;
            _ = CheckUpdate(updateHelper, isShowNotify, _DeviceTypeList, isUODMode, displayUpdateHelper).Result;
            return Task.FromResult(_fWUpdateInfoPackage);
        }

        /// <summary>
        /// 檢查更新資訊
        /// </summary>
        /// <param name="updateHelper">IL的更新資訊</param>
        /// <param name="isShowNotify">是否顯示右下角通知圖示</param>
        /// <returns>回傳裝置資訊表(如果有需強制安裝更新的話，該裝置資訊表會被寫入對應裝置的安裝結果)</returns>
        public Task<List<FWUpdateInfo>> CheckUpdate(UpdateHelper updateHelper, bool isShowNotify, List<DeviceType>? deviceTypeList, bool isUODMode, DisplayUpdateHelper displayUpdateHelper)
        {
            _IsShowNotify = isShowNotify;
            _logs.DebugMsg_1(nameof(CheckUpdate) + " start");
            _fWUpdateInfoPackage = new FWUpdateInfoPackage();
            _fWUpdateInfoPackage.TheLastCheckTime = DateTime.Now;
            if (updateHelper.UpdateItems != null && updateHelper.UpdateItems.Count > 0)
            {
                for (int i = 0; i < updateHelper.UpdateItems.Count; i++)
                {
                    //0614 Bruce 因版本號為16進制，可能為字母故新增轉換並判斷
                    string newVer = updateHelper.UpdateItems[i].NewVersion;
                    if (!int.TryParse(newVer, out _))
                    {
                        newVer = Convert.ToInt32(newVer, 16).ToString();
                    }
                    if (deviceTypeList == null)
                    {
                        FWUpdateInfo fWUpdateInfo = new FWUpdateInfo()
                        {
                            TheLatestVersion = Regex.Replace(Convert.ToInt32(newVer).ToString("D4"), ".{1}", "$0.").Substring(0, (Convert.ToInt32(newVer).ToString("D4").Length * 2) - 1),
                            DeviceVersion = Regex.Replace(Convert.ToInt32(updateHelper.UpdateItems[i].CurrentVersion).ToString("D4"), ".{1}", "$0.").Substring(0, (Convert.ToInt32(updateHelper.UpdateItems[i].CurrentVersion).ToString("D4").Length * 2) - 1),
                            NeedUpdated = int.Parse(newVer) > int.Parse(updateHelper.UpdateItems[i].CurrentVersion) ? true : false,
                            ServerPath = updateHelper.UpdateItems[i].ServerPath,
                            FileSavepath = updateHelper.UpdateItems[i].InstallPath,
                            Model = updateHelper.UpdateItems[i].DeviceModelNumber,
                            DeviceName = updateHelper.UpdateItems[i].DeviceName,
                            //0614 Bruce 將原本DeviceType型態是字串改成跟IL一樣這樣可以直接使用IL提供的矩陣做判斷，UI有個地方也會跟著異動
                            DeviceType = updateHelper.UpdateItems[i].DeviceType,
                            DeviceId = updateHelper.UpdateItems[i].DeviceId,
                            DevicePath = updateHelper.UpdateItems[i].DevicePath,
                            SHA512 = updateHelper.UpdateItems[i].SHA512,
                            Thumbprint = updateHelper.UpdateItems[i].Thumbprint,
                            IsUOD = (isUODMode &&
                            (updateHelper.UpdateItems[i].DeviceType == DeviceType.PhysicalWiredDock ||
                            updateHelper.UpdateItems[i].DeviceType == DeviceType.LogicalDock)),
                            IsDisplay = false
                        };
                        _fWUpdateInfoPackage.FWUpdateInfo.Add(fWUpdateInfo);
                    }
                    else if (deviceTypeList != null)
                    {
                        if (deviceTypeList.Exists(device => device.Equals(updateHelper.UpdateItems[i].DeviceType)))
                        {
                            FWUpdateInfo fWUpdateInfo = new FWUpdateInfo()
                            {
                                TheLatestVersion = Regex.Replace(Convert.ToInt32(newVer).ToString("D4"), ".{1}", "$0.").Substring(0, (Convert.ToInt32(newVer).ToString("D4").Length * 2) - 1),
                                DeviceVersion = Regex.Replace(Convert.ToInt32(updateHelper.UpdateItems[i].CurrentVersion).ToString("D4"), ".{1}", "$0.").Substring(0, (Convert.ToInt32(updateHelper.UpdateItems[i].CurrentVersion).ToString("D4").Length * 2) - 1),
                                NeedUpdated = int.Parse(newVer) > int.Parse(updateHelper.UpdateItems[i].CurrentVersion) ? true : false,
                                ServerPath = updateHelper.UpdateItems[i].ServerPath,
                                FileSavepath = updateHelper.UpdateItems[i].InstallPath,
                                Model = updateHelper.UpdateItems[i].DeviceModelNumber,
                                DeviceName = updateHelper.UpdateItems[i].DeviceName,
                                //0614 Bruce 將原本DeviceType型態是字串改成跟IL一樣這樣可以直接使用IL提供的矩陣做判斷，UI有個地方也會跟著異動
                                DeviceType = updateHelper.UpdateItems[i].DeviceType,
                                DeviceId = updateHelper.UpdateItems[i].DeviceId,
                                DevicePath = updateHelper.UpdateItems[i].DevicePath,
                                SHA512 = updateHelper.UpdateItems[i].SHA512,
                                Thumbprint = updateHelper.UpdateItems[i].Thumbprint,
                                IsUOD = (isUODMode &&
                                (updateHelper.UpdateItems[i].DeviceType == DeviceType.PhysicalWiredDock ||
                                updateHelper.UpdateItems[i].DeviceType == DeviceType.LogicalDock)),
                                IsDisplay = false
                            };
                            _fWUpdateInfoPackage.FWUpdateInfo.Add(fWUpdateInfo);
                        }
                    }
                }
            }
            if (displayUpdateHelper != null && displayUpdateHelper.Firmwares.Count > 0)
            {
                for (int i = 0; i < displayUpdateHelper.Firmwares.Count; i++)
                {
                    FWUpdateInfo fWUpdateInfo = new FWUpdateInfo()
                    {
                        TheLatestVersion = displayUpdateHelper.Firmwares[i].TheLastVersion,
                        DeviceVersion = displayUpdateHelper.Firmwares[i].CurrentVersion,
                        NeedUpdated = true,
                        DeviceType = DeviceType.Unknown,
                        IsDisplay = true,
                        ServerPath = displayUpdateHelper.Firmwares[i].url,
                        Model = displayUpdateHelper.Firmwares[i].id,
                        DeviceName = displayUpdateHelper.Firmwares[i].id,
                        SHA256 = displayUpdateHelper.Firmwares[i].SHA256,
                        IsUOD = false
                    };
                    _fWUpdateInfoPackage.FWUpdateInfo.Add(fWUpdateInfo);
                }
            }
            if (_fWUpdateInfoPackage.FWUpdateInfo.Count > 0)
            {
                HandleUpdateInfo();
                _logs.DebugMsg_1(nameof(CheckUpdate) + " done.");
            }
            else
            {
                _logs.DebugMsg_1(nameof(CheckUpdate) + " no updates available.");
            }
            _IsShowNotify = true;
            _isDefer = false;
            _isForce = false;
            return Task.FromResult(new List<FWUpdateInfo>());
        }

        private void HandleUpdateInfo()
        {
            _logs.DebugMsg_1("HandleUpdateInfo");
            if (_fWUpdateInfoPackage != null && _DelayFWUpdateInfoPackage != null)
            {
                List<FWUpdateInfo> _ForceUpdates = new List<FWUpdateInfo>();
                bool isUpdate = false;//判斷是否強制更新
                bool isOnlyInfo = false;
                string s = "";
                if (_fWUpdateInfoPackage.FWUpdateInfo.Count <= 0)
                {
                    isOnlyInfo = true;
                    s = "no updates available.";
                    _logs.DebugMsg_1("HandleUpdateInfo no updates available");
                }
                else
                {
                    foreach (FWUpdateInfo fwUpdateInfo in _fWUpdateInfoPackage.FWUpdateInfo)
                    {
                        if (_isForce)
                        {
                            isUpdate = true;
                            s += $"{fwUpdateInfo.Model} will be updated to {fwUpdateInfo.TheLatestVersion}\n";
                        }
                        if (_DelayFWUpdateInfoPackage.FWUpdateInfo.Exists(o => o.Equals(fwUpdateInfo)))
                        {
                            if (_DelayFWUpdateInfoPackage.SaveTime != null)
                            {
                                FWUpdateInfo? delayFUpdateInfo = _DelayFWUpdateInfoPackage.FWUpdateInfo.Find(o => o.Equals(fwUpdateInfo));
                                if (delayFUpdateInfo != null)
                                {
                                    delayFUpdateInfo.ServerPath = fwUpdateInfo.ServerPath;
                                    delayFUpdateInfo.SHA512 = fwUpdateInfo.SHA512;
                                    delayFUpdateInfo.Thumbprint = fwUpdateInfo.Thumbprint;
                                    TimeSpan difference = DateTime.Now - (DateTime)_DelayFWUpdateInfoPackage.SaveTime;
                                    if (_isDefer)
                                    {
                                        s += $"{fwUpdateInfo.Model} can be updated to {fwUpdateInfo.TheLatestVersion}\n";
                                    }
                                    else if (difference.TotalHours >= 24 && _DelayFWUpdateInfoPackage.DelayTimesAvailable > 0)
                                    {
                                        s += $"{fwUpdateInfo.Model} can be updated to {fwUpdateInfo.TheLatestVersion}\n";
                                    }
                                    else if (difference.TotalHours >= 24 && _DelayFWUpdateInfoPackage.DelayTimesAvailable <= 0)
                                    {
                                        _ForceUpdates.Add(delayFUpdateInfo);
                                        isUpdate = true;
                                        s += $"{fwUpdateInfo.Model} will be updated to {fwUpdateInfo.TheLatestVersion}\n";
                                    }
                                }
                            }
                        }
                        else
                        {
                            s += $"{fwUpdateInfo.Model} can be updated to {fwUpdateInfo.TheLatestVersion}\n";
                        }
                        _logs.DebugMsg_1(s);
                    }
                }
                NotificationFWupdate("Updates info", s, isOnlyInfo, isUpdate);
                _logs.DebugMsg_1("HandleUpdateInfo done");
            }
        }

        /// <summary>
        /// 從伺服端下載更新檔，下載後會接續執行安裝方法
        /// </summary>
        /// <param name="fwUpdateInfos">更新的裝置資訊表</param>
        /// <returns>回傳裝置資訊表(在這個方法裡將原本傳入的裝置資訊表，再寫入對應裝置的下載安裝的結果碼)</returns>
        public Task<List<FWUpdateInfo>> DownloadAndInstall(List<FWUpdateInfo> fwUpdateInfos, string installPath)
        {
            try
            {
                string path_programdata = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
                _logs.DebugMsg_1(nameof(DownloadAndInstall) + " start");
                string saveFolderName = Guid.NewGuid().ToString();
                string savePath;
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
                //0909 Bruce Add Security
                string FolderInfo;
                string PathSymbolicLinInfo;
                int count = 0;
                bool folderValid = false;
                do
                {
                    FolderInfo = string.Empty;
                    PathSymbolicLinInfo = string.Empty;
                    folderValid = false;
                    folderValid = DDPMFileSecurity.SRemoveSymbolicFolder(savePath, out PathSymbolicLinInfo);//0924 Bruce Add Security
                    if (!folderValid)
                    {
                        _logs.DebugMsg_1(nameof(DownloadAndInstall) + " FolderIsNotSafe:" + PathSymbolicLinInfo + " Retry:" + (count++));
                    }
                    folderValid = DDPMFileSecurity.IsFolderPathValid(savePath, out FolderInfo) && folderValid;
                    if (!folderValid)
                    {
                        _logs.DebugMsg_1(nameof(DownloadAndInstall) + " FolderIsNotSafe:" + FolderInfo + " Retry:" + (count++));
                        Directory.Delete(savePath, true);
                        Directory.CreateDirectory(savePath);
                    }
                } while (!folderValid && count < 2);
                if (!folderValid)
                {
                    foreach (FWUpdateInfo fwUpdateInfo in fwUpdateInfos)
                    {
                        fwUpdateInfo.FWUErrorCode = FWUErrorCode.FolderIsNotSafe;
                    }
                    _notificationStr = $"Firmware update unsuccessful.";
                    NotificationFWupdate("Error", _notificationStr);
                    _logs.DebugMsg_1(nameof(DownloadAndInstall) + " FolderIsNotSafe:" + FolderInfo);
                    return Task.FromResult(fwUpdateInfos);
                }
                for (int i = 0; i < fwUpdateInfos.Count; i++)
                {
                    _logs.DebugMsg_1(fwUpdateInfos[i].DeviceName + nameof(DownloadAndInstall) + " start");
                    _notificationStr = "";
                    _fWUpdateInfo = fwUpdateInfos[i];
                    _updateErrorCode = FWUErrorCode.Unknow;
                    fwUpdateInfos[i].FWUErrorCode = _updateErrorCode;
                    if (CheckSameDevice(fwUpdateInfos[i]))
                    {
                        fwUpdateInfos[i].FWUErrorCode = FWUErrorCode.ConnectMultipleDocks;
                        continue;
                    }
                    if (CheckPCBattery(fwUpdateInfos[i]))
                    {
                        fwUpdateInfos[i].FWUErrorCode = FWUErrorCode.PCBatteryTooLow;
                        continue;
                    }
                    string url = fwUpdateInfos[i].ServerPath;
                    //0909 Bruce Add Security
                    count = 0;
                    do
                    {
                        FolderInfo = string.Empty;
                        PathSymbolicLinInfo = string.Empty;
                        folderValid = false;
                        folderValid = DDPMFileSecurity.SRemoveSymbolicFolder(savePath, out PathSymbolicLinInfo);//0924 Bruce Add Security
                        if (!folderValid)
                        {
                            _logs.DebugMsg_1(nameof(DownloadAndInstall) + " FolderIsNotSafe:" + PathSymbolicLinInfo + " Retry:" + (count++));
                        }
                        folderValid = DDPMFileSecurity.IsFolderPathValid(savePath, out FolderInfo) && folderValid;
                        if (!folderValid)
                        {
                            _logs.DebugMsg_1(nameof(DownloadAndInstall) + " FolderIsNotSafe:" + FolderInfo + " Retry:" + (count++));
                            //Do remove Symbolic Link than delete folder
                            Directory.Delete(savePath, true);
                            Directory.CreateDirectory(savePath);
                        }
                    } while (!folderValid && count < 2);
                    if (!folderValid)
                    {
                        fwUpdateInfos[i].FWUErrorCode = FWUErrorCode.FolderIsNotSafe;
                        _notificationStr = $"Firmware update unsuccessful.";
                        NotificationFWupdate("Error", _notificationStr);
                        _logs.DebugMsg_1(nameof(DownloadAndInstall) + " FolderIsNotSafe:" + FolderInfo);
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
                    FWUpdateInfo fWUpdateInfo_Status = new FWUpdateInfo()
                    {
                        DeviceName = fwUpdateInfos[i].DeviceName,
                        TheLatestVersion = _fWUpdateInfo.TheLatestVersion,
                        ProcessName = "Downloading",
                        ProcessProgress = 100,
                    };
                    sendMessageToEvent(fWUpdateInfo_Status);
                    if (!downloadRet)
                    {
                        if (downloadInfo.Equals("CA check fail"))
                        {
                            fwUpdateInfos[i].FWUErrorCode = FWUErrorCode.CAFail;
                            _notificationStr = $"Firmware update unsuccessful.";
                            NotificationFWupdate("Error", _notificationStr);
                        }
                        else if (downloadInfo.Equals("Network fail"))
                        {
                            fwUpdateInfos[i].FWUErrorCode = FWUErrorCode.NetworkDisconnection;
                            _notificationStr = $"Update failed due to network error. Try again.";
                            NotificationFWupdate("Error", _notificationStr);
                        }
                        _logs.DebugMsg_1(fwUpdateInfos[i].DeviceName + " Download File Fail");
                        continue;
                    }
                    string extractPath = Path.Combine(savePath + Path.GetFileName(url).Substring(0, Path.GetFileName(url).Length - 4));
                    if (!Directory.Exists(extractPath))
                    {
                        Directory.CreateDirectory(extractPath);
                    }
                    //0909 Bruce Add Security
                    FolderInfo = string.Empty;
                    count = 0;
                    folderValid = false;
                    do
                    {
                        FolderInfo = string.Empty;
                        folderValid = DDPMFileSecurity.IsFolderPathValid(extractPath, out FolderInfo);
                        if (!folderValid)
                        {
                            _logs.DebugMsg_1(nameof(DownloadAndInstall) + " FolderIsNotSafe:" + FolderInfo + " Retry:" + (count++));
                            Directory.Delete(extractPath, true);
                            Directory.CreateDirectory(extractPath);
                        }
                    } while (!folderValid && count < 2);
                    if (!folderValid)
                    {
                        fwUpdateInfos[i].FWUErrorCode = FWUErrorCode.FolderIsNotSafe;
                        _logs.DebugMsg_1(fwUpdateInfos[i].DeviceName + " FolderIsNotSafe:" + FolderInfo);
                        _notificationStr = $"Firmware update unsuccessful.";
                        NotificationFWupdate("Error", _notificationStr);
                        continue;
                    }

                    CertificateCheck certificateCheck = new CertificateCheck();
                    bool isCheckSHA = false;
                    string FileCAInfo = string.Empty;
                    if (!fwUpdateInfos[i].IsDisplay)
                    {
                        //Bruce 0913 Add zip file check SHA512, SHA256.
                        if (!string.IsNullOrEmpty(fwUpdateInfos[i].SHA512))
                        {
                            isCheckSHA = certificateCheck.CheckFile_SHA512(_installationFileStoragePath, fwUpdateInfos[i].SHA512, out FileCAInfo);
                        }
                        else
                        {
                            isCheckSHA = certificateCheck.CheckFile_SHA256(_installationFileStoragePath, fwUpdateInfos[i].SHA256, out FileCAInfo);
                        }
                        if (!isCheckSHA)
                        {
                            fwUpdateInfos[i].FWUErrorCode = FWUErrorCode.FileCheckFail;
                            _logs.DebugMsg_1(fwUpdateInfos[i].DeviceName + " File check fail. Ex:" + FileCAInfo);
                            _notificationStr = $"Firmware update unsuccessful.";
                            NotificationFWupdate("Error", _notificationStr);
                            continue;
                        }
                        string exeFilePath;
                        Unzip unzip = new Unzip(_logs);
                        if (!unzip.ExecuteUnzip(_installationFileStoragePath, extractPath, out exeFilePath))
                        {
                            fwUpdateInfos[i].FWUErrorCode = FWUErrorCode.FolderIsNotSafe;
                            _logs.DebugMsg_1(fwUpdateInfos[i].DeviceName + " Unzip Faile");
                            _notificationStr = $"Firmware update unsuccessful.";
                            NotificationFWupdate("Error", _notificationStr);
                            continue;
                        }
                        //Bruce 0913 Add exe file check Thumbprint.
                        isCheckSHA = false;
                        FileCAInfo = string.Empty;
                        isCheckSHA = certificateCheck.CheckFile_Thumbprint(exeFilePath, fwUpdateInfos[i].Thumbprint, out FileCAInfo);
                        if (isCheckSHA)
                        {
                            fwUpdateInfos[i].InstallPaths = exeFilePath;
                            fwUpdateInfos[i].FWUErrorCode = Install(fwUpdateInfos[i]);
                        }
                        else
                        {
                            fwUpdateInfos[i].FWUErrorCode = FWUErrorCode.FileCheckFail;
                            _logs.DebugMsg_1(fwUpdateInfos[i].DeviceName + " File check fail. Ex:" + FileCAInfo);
                            _notificationStr = $"Firmware update unsuccessful.";
                        }
                    }
                    else//Display FWU
                    {
                        string exeFilePath = _installationFileStoragePath;
                        isCheckSHA = false;
                        FileCAInfo = string.Empty;
                        if (!string.IsNullOrEmpty(fwUpdateInfos[i].SHA512))
                        {
                            isCheckSHA = certificateCheck.CheckFile_SHA512(_installationFileStoragePath, fwUpdateInfos[i].SHA512, out FileCAInfo);
                        }
                        else
                        {
                            isCheckSHA = certificateCheck.CheckFile_SHA256(_installationFileStoragePath, fwUpdateInfos[i].SHA256, out FileCAInfo);
                        }
                        if (isCheckSHA)
                        {
                            fwUpdateInfos[i].InstallPaths = exeFilePath;
                            fwUpdateInfos[i].FWUErrorCode = Install(fwUpdateInfos[i]);
                        }
                        else
                        {
                            fwUpdateInfos[i].FWUErrorCode = FWUErrorCode.FileCheckFail;
                            _logs.DebugMsg_1(fwUpdateInfos[i].DeviceName + " File check fail. Ex:" + FileCAInfo);
                            _notificationStr = $"Firmware update unsuccessful.";
                        }
                    }
                    if (fwUpdateInfos[i].FWUErrorCode == FWUErrorCode.NoError)
                    {
                        NotificationFWupdate("FW info", _notificationStr);
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
                _logs.DebugMsg_1(nameof(DownloadAndInstall) + " done");
                if (_DelayFWUpdateInfoPackage != null && _DelayFWUpdateInfoPackage.FWUpdateInfo.Count <= 0)
                {
                    _DelayFWUpdateInfoPackage = new FWUpdateInfoPackage();
                    CallSaveUpdateInfoPackage?.AsyncFireAndForget(this, _DelayFWUpdateInfoPackage, System.Threading.CancellationToken.None);
                }
                else if (_DelayFWUpdateInfoPackage != null)
                {
                    CallSaveUpdateInfoPackage?.AsyncFireAndForget(this, _DelayFWUpdateInfoPackage, System.Threading.CancellationToken.None);
                }
                _IsShowNotify = true;
                _isDefer = false;
                _isForce = false;
                return Task.FromResult(fwUpdateInfos);
            }
            catch (Exception ex)
            {
                foreach (FWUpdateInfo deviceInfo in fwUpdateInfos)
                {
                    deviceInfo.FWUErrorCode = FWUErrorCode.NetworkDisconnection;
                }
                _notificationStr = $"{_fWUpdateInfo.DeviceName} Update failed due to network error. Try again.";
                NotificationFWupdate("Error", _notificationStr);
                _logs.DebugMsg_1(nameof(DownloadAndInstall) + " Error：Update failed due to network error. Try again. ex:" + ex.Message); // 輸出錯誤訊息
                _IsShowNotify = true;
                _isDefer = false;
                _isForce = false;
                return Task.FromResult(fwUpdateInfos);
            }
        }

        public Task<FWUErrorCode> Install(string installPath)
        {
            FWUpdateInfo fWUpdateInfo = new FWUpdateInfo()
            {
                InstallPaths = installPath
            };
            return Task.FromResult(Install(fWUpdateInfo));
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
                FWUpdateInfo fWUpdateInfo = new FWUpdateInfo()
                {
                    DeviceName = _fWUpdateInfo.DeviceName,
                    TheLatestVersion = _fWUpdateInfo.TheLatestVersion,
                    ProcessName = "Downloading",
                    ProcessProgress = download.GetProgress(),
                };
                sendMessageToEvent(fWUpdateInfo);
            }
        }

        /// <summary>
        /// Check if the same device is connected
        /// </summary>
        /// <param name="currentFWInfo">Firmware information currently to be updated</param>
        /// <returns>Two Docks are connected at the same time return true; otherwise return false.</returns>
        private bool CheckSameDevice(FWUpdateInfo currentFWInfo)
        {
            bool isDockUpdate = false;
            if (currentFWInfo.DeviceType == DeviceType.LogicalDock)
            {
                isDockUpdate = true;
            }
            int dockCount = 0;
            if (isDockUpdate)
            {
                CallGetDeviceInfos?.AsyncFireAndForget(this, new EventArgs(), System.Threading.CancellationToken.None);
                int count = 0;
                do
                {
                    Thread.Sleep(100);
                    count++;
                } while (_DeviceInfos == null && count <= 5);
                if (_DeviceInfos != null)
                {
                    foreach (DeviceInfo device in _DeviceInfos)
                    {
                        if (device != null)
                        {
                            if (device.Type == DeviceType.LogicalDock)
                            {
                                dockCount++;
                            }
                            if (dockCount >= 2)
                            {
                                break;
                            }
                        }
                    }
                }
                _DeviceInfos = null;
            }
            if (dockCount >= 2 && isDockUpdate)
            {
                _notificationStr = "Multiple docks are detected. Keep only one dock connected to prevent damage to your docks.";
                NotificationFWupdate("Error", _notificationStr);
                _logs.DebugMsg_1(nameof(DownloadAndInstall) + " Error：" + _notificationStr); // 輸出錯誤訊息
                return true;
            }
            return false;
        }

        /// <summary>
        /// Determine whether the computer battery power is less than 10% for Dock firmware update
        /// </summary>
        /// <param name="currentFWInfo">Firmware information currently to be updated</param>
        /// <returns>Computer power is less than 10% returns true; otherwise it returns false.</returns>
        private bool CheckPCBattery(FWUpdateInfo currentFWInfo)
        {
            bool isDockUpdate = false;
            if (currentFWInfo.DeviceType == DeviceType.LogicalDock)
            {
                isDockUpdate = true;
            }
            BatteryInfo batteryInfo = new BatteryInfo();
            batteryInfo.GetBatteryInfo(out var battery);
            _logs.DebugMsg_1(nameof(CheckPCBattery) + " battery life percent：" + battery.BatteryLifePercent);
            //0614 Bruce 將原本DeviceType型態是字串改成跟IL一樣這樣可以直接使用IL提供的矩陣做判斷，UI有個地方也會跟著異動
            if (isDockUpdate && battery.BatteryLifePercent <= 10)
            {
                _notificationStr = $"{_fWUpdateInfo.DeviceName} update download cancel, because PC battery too low.";
                NotificationFWupdate("Error", _notificationStr);
                _logs.DebugMsg_1(nameof(CheckPCBattery) + " Error：" + _notificationStr); // 輸出錯誤訊息
                return true;
            }
            return false;
        }

        /// <summary>
        /// 定期檢查更新排程
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CheckUpdateScheduleTimer_Elapsed(object? sender, ElapsedEventArgs e)
        {
            TimeSpan difference = DateTime.Now - _fWUpdateInfoPackage.TheLastCheckTime;
            //0612 Bruce 將檢查更新區間修改為5分鐘
            int checkTime = 5;
            if (difference.TotalMinutes > checkTime)
            {
                CollCheckUpdate?.AsyncFireAndForget(this, e, System.Threading.CancellationToken.None);
            }
        }

        /// <summary>
        /// 定期檢查是否有Dock韌體載入完成但還沒完成安裝
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CheckDockUODScheduleTimer_Elapsed(object? sender, ElapsedEventArgs e)
        {
            CallCheckUODFWInfos?.AsyncFireAndForget(this, e, System.Threading.CancellationToken.None);
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
                    Object = _fWUpdateInfoPackage
                };
                CallPopup?.AsyncFireAndForget(this, popupContentPackage, System.Threading.CancellationToken.None);
                //Task.Run(() =>
                //{
                //    PopupBaseManage popupBaseManage = new PopupBaseManage();
                //    popupBaseManage.LeftButtonClick += UpdateEvent;
                //    popupBaseManage.RightButtonClick += DelayEvent;
                //    if (isInfo)
                //    {
                //        popupBaseManage.FWU_Show(title, info, "", "", _fWUpdateInfoPackage, stayOpen, timeout);
                //    }
                //    else if (isOnlyUpdate)
                //    {
                //        popupBaseManage.Default_Event += UpdateEvent;
                //        popupBaseManage.FWU_Show(title, info, "Update", "", _fWUpdateInfoPackage, stayOpen, timeout);
                //    }
                //    else
                //    {
                //        popupBaseManage.Default_Event += DelayEvent;
                //        popupBaseManage.FWU_Show(title, info, "Update", "Delay", _fWUpdateInfoPackage, stayOpen, timeout);
                //    }

                //});
            }
        }

        private void CheckInput(ToastNotificationActivatedEventArgsCompat e)
        {
            /*if (e.Argument.Equals("Update"))
            {
                UpdateEvent();
            }
            else if (e.Argument.Equals("Delay"))
            {
                DelayEvent();
            }*/
        }

        /// <summary>
        /// NotificationFWupdate 呼叫DDPM UI事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CallDDPM()
        {
            string path = "..\\..\\..\\..\\..\\DDPM.UI\\bin\\net8.0-windows10.0.19041.0\\DDPM.exe";
            const int SW_SHOWMAXIMIZED = 3;
            string processName = "DDPM";

            if (OperatingSystem.IsWindows())
            {
                Process[] processes = Process.GetProcessesByName(processName);
                if (processes.Length > 0)
                {
                    IntPtr mainWindowHandle = processes[0].MainWindowHandle;
                    // 將窗口最大化
                    _ShowWindow(mainWindowHandle, SW_SHOWMAXIMIZED);
                    // 顯示到前景
                    _SetForegroundWindow(mainWindowHandle);
                }
                else
                {
                    Process.Start(path);
                }
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
            FWUpdateInfoPackage fWUpdateInfoPackage = JsonConvert.DeserializeObject<FWUpdateInfoPackage>(json);
            if (fWUpdateInfoPackage != null)
            {
                if (_DelayFWUpdateInfoPackage != null && _DelayFWUpdateInfoPackage.SaveTime != null && _DelayFWUpdateInfoPackage.FWUpdateInfo.Count > 0)
                {
                    foreach (FWUpdateInfo newFWUpdateInfo in fWUpdateInfoPackage.FWUpdateInfo)
                    {
                        if (!_DelayFWUpdateInfoPackage.FWUpdateInfo.Exists(o => o.Equals(newFWUpdateInfo)))
                        {
                            newFWUpdateInfo.ServerPath = "";
                            newFWUpdateInfo.SHA256 = "";
                            newFWUpdateInfo.SHA512 = "";
                            newFWUpdateInfo.Thumbprint = "";
                            _DelayFWUpdateInfoPackage.FWUpdateInfo.Add(newFWUpdateInfo);
                        }
                    }
                    _DelayFWUpdateInfoPackage.DelayTimesAvailable--;
                    CallSaveUpdateInfoPackage?.AsyncFireAndForget(this, _DelayFWUpdateInfoPackage, System.Threading.CancellationToken.None);
                }
                else if (_DelayFWUpdateInfoPackage != null && _DelayFWUpdateInfoPackage.SaveTime == null)
                {
                    _DelayFWUpdateInfoPackage = _fWUpdateInfoPackage;
                    foreach (FWUpdateInfo newFWUpdateInfo in _DelayFWUpdateInfoPackage.FWUpdateInfo)
                    {
                        newFWUpdateInfo.ServerPath = "";
                        newFWUpdateInfo.SHA256 = "";
                        newFWUpdateInfo.SHA512 = "";
                        newFWUpdateInfo.Thumbprint = "";
                    }
                    _DelayFWUpdateInfoPackage.DelayTimesAvailable = 2;
                    _DelayFWUpdateInfoPackage.SaveTime = DateTime.Now;
                    CallSaveUpdateInfoPackage?.AsyncFireAndForget(this, _DelayFWUpdateInfoPackage, System.Threading.CancellationToken.None);
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
            FWUpdateInfoPackage fWUpdateInfoPackage = JsonConvert.DeserializeObject<FWUpdateInfoPackage>(json);
            List<FWUpdateInfo> fWUpdateInfo = fWUpdateInfoPackage.FWUpdateInfo;
            DownloadAndInstall_Result_Notify?.AsyncFireAndForget(this, DownloadAndInstall(fWUpdateInfo, "").Result, System.Threading.CancellationToken.None);
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
        private FWUErrorCode Install(FWUpdateInfo fwUpdateInfo)
        {
            //0614 Bruce 新增Dock韌體安裝功能
            try
            {
                _logs.DebugMsg_1($"{fwUpdateInfo.DeviceName}  {nameof(Install)}  start");
                CertificateCheck certificateCheck = new CertificateCheck();
                if (!fwUpdateInfo.IsDisplay)
                {
                    //Bruce 0913 Add exe file check Thumbprint.
                    bool isCheckSHA = false;
                    string FileCAInfo = string.Empty;
                    isCheckSHA = certificateCheck.CheckFile_Thumbprint(fwUpdateInfo.InstallPaths, fwUpdateInfo.Thumbprint, out FileCAInfo);
                    if (!isCheckSHA)
                    {
                        _notificationStr = $"Firmware update unsuccessful.";
                        _logs.DebugMsg_1(fwUpdateInfo.DeviceName + " File check fail. Ex:" + FileCAInfo);
                        return FWUErrorCode.FileCheckFail;
                    }
                    string FileInfo;
                    if (!DDPMFileSecurity.IsFilePathValid(fwUpdateInfo.InstallPaths, out FileInfo))//0815 Bruce Add Security
                    {
                        _notificationStr = $"Firmware update unsuccessful.";
                        _logs.DebugMsg_1(fwUpdateInfo.DeviceName + " FileIsNoSafe:" + FileInfo);
                        return FWUErrorCode.FileIsNoSafe;
                    }
                }
                else
                {
                    bool isCheckSHA = false;
                    string FileCAInfo = string.Empty;
                    if (!string.IsNullOrEmpty(fwUpdateInfo.SHA512))
                    {
                        isCheckSHA = certificateCheck.CheckFile_SHA512(fwUpdateInfo.InstallPaths, fwUpdateInfo.SHA512, out FileCAInfo);
                    }
                    else
                    {
                        isCheckSHA = certificateCheck.CheckFile_SHA256(fwUpdateInfo.InstallPaths, fwUpdateInfo.SHA256, out FileCAInfo);
                    }
                    if (!isCheckSHA)
                    {
                        _notificationStr = $"Firmware update unsuccessful.";
                        _logs.DebugMsg_1(fwUpdateInfo.DeviceName + " File check fail. Ex:" + FileCAInfo);
                        return FWUErrorCode.FileCheckFail;
                    }
                    string FileInfo;
                    if (!DDPMFileSecurity.IsFilePathValid(fwUpdateInfo.InstallPaths, out FileInfo))//0815 Bruce Add Security
                    {
                        _notificationStr = $"Firmware update unsuccessful.";
                        _logs.DebugMsg_1(fwUpdateInfo.DeviceName + " FileIsNoSafe:" + FileInfo);
                        return FWUErrorCode.FileIsNoSafe;
                    }
                }
                string arguments;
                if (!fwUpdateInfo.IsDisplay)
                {
                    _timeOutCount = _fwTimeOutCount;
                    _timerTimeOut = new Timer();
                    _timerTimeOut.Interval = TimeSpan.FromSeconds(1).TotalMilliseconds;
                    _timerTimeOut.Elapsed += new ElapsedEventHandler(_timerTimeOut_Tick);
                    //foreach (FWUpdateInfo fwUpdateInfo in fwUpdateInfos)
                    string _namedPipeName = Guid.NewGuid().ToString("D"); // 生成唯一的管道名稱
                    _namedPipeServer = new NamedPipeStreamServer(_namedPipeName, fwUpdateInfo.Thumbprint); // 創建命名管道伺服器
                    _namedPipeServer.MessageReceived += _namedPipeServer_MessageReceived;
                    _namedPipeServer.ClientConnectedEvent += _namedPipeServer_ClientConnectedEvent;
                    _namedPipeServer.ClientDisconnectedEvent += _namedPipeServer_ClientDisconnectedEvent;
                    _logs.DebugMsg_1(fwUpdateInfo.DeviceName + nameof(_namedPipeServer) + " ready");
                    if (fwUpdateInfo.IsUOD)
                    {
                        NotificationFWupdate("Dock FW info", "Dock FW is being loaded. Do not disconnect the dock.");
                    }
                    else
                    {
                        NotificationFWupdate("FW info", fwUpdateInfo.DeviceName + " FW is being Installing. Do not disconnect the device.");
                    }
                    // 要運行的安裝程式路徑和命令行參數
                    arguments = (fwUpdateInfo.IsUOD ? "/uod " : "") + "/silent" + " /pipename:" + _namedPipeName;
                    _timerTimeOut.Enabled = true;
                }
                else
                {
                    string logPath = Path.GetDirectoryName(fwUpdateInfo.InstallPaths);
                    arguments = $"-s --force -f {logPath}";
                }
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
                int exitCode = 1;
                using (FileLock fileLock = new FileLock(fwUpdateInfo.InstallPaths, PathCheckOption.None, lockNow: true))
                {
                    AclChecker aclChecker = new AclChecker();
                    if (aclChecker.ContainsUnprivilegedWriteAccess(fileLock))
                    {
                        throw new SecurityException($"File ACLs for {fwUpdateInfo.InstallPaths} contained unprivileged write access for one or more identity");
                    }
                    /*暫時註解 因還沒有簽章
                    var result = verifier.Verify(fileLock);
                    if (result != Win32ErrorCodes.ERROR_SUCCESS)
                    {
                        throw new SecurityException($"Signature validation failed for {fwUpdateInfo.InstallPaths}! Received the following return code {result}");
                    }*/
                    //WTSFunction.RunElevatedProcess(fwUpdateInfo.InstallPaths, arguments);
                    if (fwUpdateInfo.IsDisplay)
                    {
                        FWUpdateInfo fWUpdateInfo = new FWUpdateInfo()
                        {
                            DeviceName = _fWUpdateInfo.DeviceName,
                            TheLatestVersion = _fWUpdateInfo.TheLatestVersion,
                            ProcessName = "Installing",
                            ProcessProgress = 50,
                        };
                        sendMessageToEvent(fWUpdateInfo);
                    }
                    UserImpersonator.RunAsUser(token, () =>
                    {
                        using (Process clientProcess = new Process())
                        {
                            _clientProcess = new Process();
                            _clientProcess.StartInfo.UseShellExecute = false;
                            _clientProcess.StartInfo.FileName = fwUpdateInfo.InstallPaths;
                            _clientProcess.StartInfo.WorkingDirectory = Path.GetDirectoryName(_clientProcess.StartInfo.FileName);
                            _clientProcess.StartInfo.Arguments = arguments;
                            _clientProcess.Start();
                            _clientProcess.WaitForExit();
                            exitCode = _clientProcess.ExitCode;
                        }
                    });
                }
                if (fwUpdateInfo.IsDisplay)
                {
                    if (exitCode == 0)
                    {
                        _updateErrorCode = FWUErrorCode.NoError;
                        _notificationStr = $"{_fWUpdateInfo.DeviceName} Firmware update successful";
                    }
                    else
                    {
                        switch (exitCode)
                        {
                            case 3:
                                _updateErrorCode = FWUErrorCode.DeviceDisconnected;
                                break;
                            case 4:
                                _updateErrorCode = FWUErrorCode.FirmwareUpdatNotSupportedForThisDevice;
                                break;
                            default:
                                _updateErrorCode = FWUErrorCode.FirmwareUpdateFailed;
                                break;
                        }
                        _notificationStr = $"{_fWUpdateInfo.DeviceName} Firmware update unsuccessful: code:{exitCode} {_updateErrorCode.ToString()}";
                        
                    }
                }
                else
                {
                    if (_namedPipeServer.IsNamedPipeServerIsNoSafe)
                    {
                        _notificationStr = $"Firmware update unsuccessful.";
                        _logs.DebugMsg_1(fwUpdateInfo.DeviceName + " Named Pipe Server Is No Safe.");
                        return FWUErrorCode.NamedPipeServerIsNoSafe;
                    }
                }
                if (fwUpdateInfo.IsUOD)
                {
                    string ret = "";
                    if (_updateErrorCode == FWUErrorCode.NoError)
                    {
                        ret = "Dock FW is loaded successful. Disconnect dock for completing FW application and reconnect dock after 1 min.";
                        _updateErrorCode = FWUErrorCode.NoError;
                        fwUpdateInfo.PNPDeviceID = GetDevicePNPDeviceID(fwUpdateInfo.DeviceName);
                        DokcUODUpdateInfoPackage dokcUODUpdateInfoPackage = new DokcUODUpdateInfoPackage();
                        dokcUODUpdateInfoPackage.FWUpdateInfo = fwUpdateInfo;
                        CheckUODFWUInfo(dokcUODUpdateInfoPackage, null);
                    }
                    else
                    {
                        ret = "Dock FW loaded failed.";
                        _updateErrorCode = FWUErrorCode.FirmwareUpdateFailed;
                    }
                    FWUpdateInfo fWUpdateInfo = new FWUpdateInfo()
                    {
                        DeviceName = _fWUpdateInfo.DeviceName,
                        TheLatestVersion = _fWUpdateInfo.TheLatestVersion,
                        ProcessName = ret
                    };
                    _notificationStr = ret;
                    sendMessageToEvent(fWUpdateInfo);
                }
                if (_DelayFWUpdateInfoPackage != null && _updateErrorCode == FWUErrorCode.NoError)
                {
                    _DelayFWUpdateInfoPackage.FWUpdateInfo.RemoveAll(obj => obj.DevicePath == fwUpdateInfo.DevicePath);
                }
                _logs.DebugMsg_1($"{_notificationStr}");
                return _updateErrorCode;
            }
            catch (Exception ex)
            {
                _updateErrorCode = FWUErrorCode.Unknow;
                _logs.DebugMsg_1(fwUpdateInfo.DeviceName + nameof(Install) + " Error:" + ex.ToString());
                _notificationStr = $"{_fWUpdateInfo.DeviceName} Service not running. Try again.";
                return _updateErrorCode;
            }
        }

        /// <summary>
        /// 取得特定裝置的裝置例項路徑
        /// </summary>
        /// <param name="VID">要取得裝置的VID</param>
        /// <param name="PID">要取得裝置的PID</param>
        /// <returns></returns>
        private string GetDevicePNPDeviceID(string deviceName)
        {
            try
            {
                string VID;
                string PID;
                // 定義正則表達式來匹配 VID 和 PID
                Regex regex = new Regex(@"VID\s*:\s*0x([\da-fA-F]+),\s*PID\s*:\s*0x([\da-fA-F]+)");

                // 在輸入字串中尋找匹配
                Match matchVIDPID = regex.Match(deviceName);

                // 檢查是否有匹配
                if (matchVIDPID.Success)
                {
                    // 取得 VID 和 PID 的十六進制字串值
                    VID = matchVIDPID.Groups[1].Value.ToUpper();
                    PID = matchVIDPID.Groups[2].Value.ToUpper();
                }
                else
                {
                    _logs.DebugMsg_1(nameof(GetDevicePNPDeviceID) + " Error:" + "VID and PID not found.");
                    return "";
                }
                // 定義 WMI 查詢，查詢 HIDClass 類別的裝置
                string query = "SELECT * FROM Win32_PnPEntity WHERE ClassGuid='{745a17a0-74d3-11d0-b6fe-00a0c90f57da}'";

                // 建立管理範圍和查詢
                ManagementObjectSearcher searcher = new ManagementObjectSearcher(query);
                ManagementObjectCollection queryCollection = searcher.Get();

                // 列舉查詢結果
                foreach (ManagementObject m in queryCollection)
                {
                    // 取得裝置識別碼 (Device ID)
                    string deviceId = m["DeviceID"] as string;
                    if (deviceId != null)
                    {
                        // 搜尋裝置識別碼中的 VID 和 PID
                        Match match = Regex.Match(deviceId, @"VID_(\w*)&PID_(\w*)");
                        if (match.Success)
                        {
                            string vid = match.Groups[1].Value;
                            string pid = match.Groups[2].Value;
                            if (vid.Equals(VID) && pid.Equals(PID))
                            {
                                // 取得裝置利項路徑 (Device Instance Path)
                                string deviceInstancePath = m["PNPDeviceID"].ToString();
                                if (deviceInstancePath != null && deviceInstancePath.Contains("USB"))
                                {
                                    return deviceInstancePath;
                                }
                            }
                        }
                    }
                }
            }
            catch (ManagementException e)
            {
                _logs.DebugMsg_1(nameof(GetDevicePNPDeviceID) + " Error:" + e.ToString());
            }
            return "";
        }

        private void _timerTimeOut_Tick(object sender, EventArgs e)
        {
            if (_timeOutCount < _fwTimeOutCount)
            {
                FWUpdateInfo fWUpdateInfo = new FWUpdateInfo()
                {
                    DeviceName = _fWUpdateInfo.DeviceName,
                    TheLatestVersion = _fWUpdateInfo.TheLatestVersion,
                    ProcessName = "Timeout",
                    ProcessProgress = _fwTimeOutCount,
                };
                sendMessageToEvent(fWUpdateInfo);
            }
            _timeOutCount--;
            if (_namedPipeServer.IsNamedPipeServerIsNoSafe)
            {
                resetState();
            }
            if (_timeOutCount == 0)
            {
                resetState();
                _updateErrorCode = FWUErrorCode.FirmwareUpdateTimeout;
                _notificationStr = $"{_fWUpdateInfo.DeviceName} E7:Timeout error";
                _logs.DebugMsg_1("Get E7:Firmware update timeout");
            }
        }

        private void _namedPipeServer_ClientDisconnectedEvent(object? sender, EventArgs e)
        {
            //Console.WriteLine("Client disconnected"); // 客戶端斷開連接提示
        }

        private void _namedPipeServer_ClientConnectedEvent(object? sender, EventArgs e)
        {
            //Console.WriteLine("Client connected"); // 客戶端連接成功提示
            //Console.WriteLine("Please connect or power on your device"); // 請求連接或開啟設備提示
        }

        private void _namedPipeServer_MessageReceived(object? sender, MessageEventArgs args)
        {
            _timeOutCount = _fwTimeOutCount;
            string message = Encoding.UTF8.GetString(args.Message); // 解析收到的訊息
            pasreMessage(message); // 解析訊息
        }

        private void pasreMessage(string message)
        {
            string messageWithRoot = "<Root>" + message;
            messageWithRoot += "</Root>";

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(messageWithRoot);
            XmlNode? msg1Node;//鍵盤滑鼠才會觸發
            XmlNode? progressNode;
            XmlNode? buttonCaptionNode;
            XmlNode? buttonStateNode;
            XmlNode? stateFlowNode;
            XmlNode? timeOut;//新的FW安裝包都有

            if (message.Contains("InvokeDisplay"))
            {
                msg1Node = xmlDoc.SelectSingleNode("Root/InvokeDisplay/MSG1");//鍵盤滑鼠才會觸發
                progressNode = xmlDoc.SelectSingleNode("Root/InvokeDisplay/Progress");
                buttonCaptionNode = xmlDoc.SelectSingleNode("Root/InvokeDisplay/Button-Caption");
                buttonStateNode = xmlDoc.SelectSingleNode("Root/InvokeDisplay/Button-State");
                if (msg1Node != null)
                {
                    if (msg1Node.InnerText == "M1")
                    {
                        FWUpdateInfo fWUpdateInfo = new FWUpdateInfo()
                        {
                            DeviceName = _fWUpdateInfo.DeviceName,
                            TheLatestVersion = _fWUpdateInfo.TheLatestVersion,
                            ProcessName = "Please double click mouse left button to start firmware update"
                        };
                        sendMessageToEvent(fWUpdateInfo);
                        _logs.DebugMsg_1("Get M1:Please double click mouse left button to start firmware update");
                    }
                    else if (msg1Node.InnerText == "M2")
                    {
                        FWUpdateInfo fWUpdateInfo = new FWUpdateInfo()
                        {
                            DeviceName = _fWUpdateInfo.DeviceName,
                            TheLatestVersion = _fWUpdateInfo.TheLatestVersion,
                            ProcessName = "Please press \"U\" key on keyboard to start firmware update"
                        };
                        sendMessageToEvent(fWUpdateInfo);
                        _logs.DebugMsg_1("Get M2:Please press \"U\" key on keyboard to start firmware update");
                    }
                    else
                    {
                        _logs.DebugMsg_1("Should got M1 or M2 but got : " + msg1Node.InnerText + Environment.NewLine);
                    }
                }
                if (msg1Node == null && progressNode == null && buttonCaptionNode == null && buttonStateNode == null)
                {
                    _logs.DebugMsg_1("Can't heandle: " + message + Environment.NewLine);
                }
            }
            else
            {
                progressNode = xmlDoc.SelectSingleNode("Root/Progress");
                buttonCaptionNode = xmlDoc.SelectSingleNode("Root/Button-Caption");
                buttonStateNode = xmlDoc.SelectSingleNode("Root/Button-State");
                stateFlowNode = xmlDoc.SelectSingleNode("Root/StateFlow");
                timeOut = xmlDoc.SelectSingleNode("Root/Timeout");//新的FW安裝包都有

                if (progressNode == null && buttonCaptionNode == null && buttonStateNode == null && stateFlowNode == null && timeOut == null)
                {
                    _logs.DebugMsg_1("Can't handle: " + message + Environment.NewLine);
                }
                if (stateFlowNode != null)
                {
                    if (stateFlowNode.InnerText == "A0")
                    {
                        _logs.DebugMsg_1("Get A0:Device connected");
                        FWUpdateInfo fWUpdateInfo = new FWUpdateInfo()
                        {
                            DeviceName = _fWUpdateInfo.DeviceName,
                            TheLatestVersion = _fWUpdateInfo.TheLatestVersion,
                            ProcessName = "Device connected",
                        };
                        sendMessageToEvent(fWUpdateInfo);
                    }
                    else if (stateFlowNode.InnerText == "A1")
                    {
                        _logs.DebugMsg_1("Get A1:Firmware update started");
                        FWUpdateInfo fWUpdateInfo = new FWUpdateInfo()
                        {
                            DeviceName = _fWUpdateInfo.DeviceName,
                            TheLatestVersion = _fWUpdateInfo.TheLatestVersion,
                            ProcessName = "Firmware update started",
                        };
                        sendMessageToEvent(fWUpdateInfo);
                    }
                    else if (stateFlowNode.InnerText == "A2")
                    {
                        _updateErrorCode = FWUErrorCode.NoError;
                        _notificationStr = $"{_fWUpdateInfo.DeviceName} A2:Firmware update successful";
                        _logs.DebugMsg_1("Get A2:Firmware update successful");
                        FWUpdateInfo fWUpdateInfo = new FWUpdateInfo()
                        {
                            DeviceName = _fWUpdateInfo.DeviceName,
                            TheLatestVersion = _fWUpdateInfo.TheLatestVersion,
                            ProcessName = "Firmware update successful",
                        };
                        sendMessageToEvent(fWUpdateInfo);
                        resetState();
                    }
                    else if (stateFlowNode.InnerText == "AF")
                    {
                        _logs.DebugMsg_1("Get AF:");
                        var errorCodeNode = xmlDoc.SelectSingleNode("Root/ErrorCode");
                        if (errorCodeNode != null)
                        {
                            if (errorCodeNode.InnerText == "E2")
                            {
                                _updateErrorCode = FWUErrorCode.FirmwareUpdateFailed;
                                _notificationStr = $"{_fWUpdateInfo.DeviceName} E2:Firmware update unsuccessful";
                                _logs.DebugMsg_1($"{_fWUpdateInfo.DeviceName} Get E2:Firmware update unsuccessful");
                            }
                            else if (errorCodeNode.InnerText == "E4")
                            {
                                _updateErrorCode = FWUErrorCode.FirmwareUpdatNotSupportedForThisDevice;
                                _notificationStr = $"{_fWUpdateInfo.DeviceName} E4:USB wireless receiver firmware is unable to support device firmware upgrade";
                                _logs.DebugMsg_1($"{_fWUpdateInfo.DeviceName} Get E4:USB wireless receiver firmware is unable to support device firmware upgrade");
                            }
                            else if (errorCodeNode.InnerText == "E5")
                            {
                                _updateErrorCode = FWUErrorCode.FirmwareUpdateTimeout;
                                _notificationStr = $"{_fWUpdateInfo.DeviceName} E5:Timeout error";
                                _logs.DebugMsg_1($"{_fWUpdateInfo.DeviceName} Get E5:Firmware update timeout");
                            }
                            else if (errorCodeNode.InnerText == "E6")
                            {
                                _updateErrorCode = FWUErrorCode.FirmwareUpdateTimeout;
                                _notificationStr = $"{_fWUpdateInfo.DeviceName} E6:Timeout error";
                                _logs.DebugMsg_1($"{_fWUpdateInfo.DeviceName} Get E6:Firmware update timeout");
                            }
                            else
                            {
                                _updateErrorCode = FWUErrorCode.Unknow;
                                _notificationStr = $"{_fWUpdateInfo.DeviceName} update failed with unknown error ";
                                _logs.DebugMsg_1($"{_fWUpdateInfo.DeviceName} ErrorCode should got E2,E4,E5 but got : " + errorCodeNode.InnerText);
                            }
                            FWUpdateInfo fWUpdateInfo = new FWUpdateInfo()
                            {
                                DeviceName = _fWUpdateInfo.DeviceName,
                                TheLatestVersion = _fWUpdateInfo.TheLatestVersion,
                                ProcessName = "Error Code:" + errorCodeNode.InnerText,
                            };
                            sendMessageToEvent(fWUpdateInfo);
                        }
                        else
                        {
                            _updateErrorCode = FWUErrorCode.Unknow;
                            _notificationStr = $"{_fWUpdateInfo.DeviceName} update failed with unknown error";
                            _logs.DebugMsg_1("ErrorCode missing : " + message);
                        }
                        resetState();
                    }
                    else if (stateFlowNode.InnerText == "0xF000")
                    {
                        //
                        // Abort failed
                        //
                        _updateErrorCode = FWUErrorCode.UserAbortedFail;
                        _notificationStr = $"{_fWUpdateInfo.DeviceName} update failed with unknown error";
                        _logs.DebugMsg_1("Get 0xF000:Can not abort update at this time");
                    }
                    else if (stateFlowNode.InnerText == "0xF001")
                    {
                        //
                        // Abort success
                        //
                        _updateErrorCode = FWUErrorCode.UserAborted;
                        _notificationStr = $"{_fWUpdateInfo.DeviceName} User aborted firmware update";
                        _logs.DebugMsg_1("Get 0xF001:User aborted firmware update");
                        resetState();
                    }
                    else if (stateFlowNode.InnerText == "U9")
                    {
                        _updateErrorCode = FWUErrorCode.NoError;
                        _notificationStr = $"{_fWUpdateInfo.DeviceName} U9:Firmware update successful";
                        _logs.DebugMsg_1("Get U9:Firmware update successful");
                        FWUpdateInfo fWUpdateInfo = new FWUpdateInfo()
                        {
                            DeviceName = _fWUpdateInfo.DeviceName,
                            TheLatestVersion = _fWUpdateInfo.TheLatestVersion,
                            ProcessName = "Firmware update successful",
                        };
                        sendMessageToEvent(fWUpdateInfo);
                        resetState();
                    }
                    else
                    {
                        _logs.DebugMsg_1("Can't handle: " + message);
                    }
                }
                if (progressNode != null)
                {
                    FWUpdateInfo fWUpdateInfo = new FWUpdateInfo()
                    {
                        DeviceName = _fWUpdateInfo.DeviceName,
                        TheLatestVersion = _fWUpdateInfo.TheLatestVersion,
                        ProcessName = "Installing",
                        ProcessProgress = int.Parse(progressNode.InnerText),
                    };
                    sendMessageToEvent(fWUpdateInfo);
                }
                if (timeOut != null)
                {
                    int.TryParse(timeOut.InnerText, out _fwTimeOutCount);
                    FWUpdateInfo fWUpdateInfo = new FWUpdateInfo()
                    {
                        DeviceName = _fWUpdateInfo.DeviceName,
                        TheLatestVersion = _fWUpdateInfo.TheLatestVersion,
                        ProcessName = "Timeout",
                        ProcessProgress = _fwTimeOutCount,
                    };
                    sendMessageToEvent(fWUpdateInfo);
                }
            }
        }

        [SupportedOSPlatform("windows")]
        [SupportedOSPlatform("windows10.0.19041.0")]
        private void resetState()
        {
            _timerTimeOut.Enabled = false;
            if (OperatingSystem.IsWindowsVersionAtLeast(10, 0, 19041))
            {
                if (_clientProcess != null)
                {
                    try
                    {
                        _clientProcess.Kill();
                        _clientProcess.Dispose();
                        _clientProcess = null;
                    }
                    catch { }
                }
            }
        }

        private void sendMessageToEvent(FWUpdateInfo fWUpdateInfo)
        {
            ProgressUpdate_Notify?.AsyncFireAndForget(this, fWUpdateInfo, System.Threading.CancellationToken.None);
            _logs.DebugMsg_1("sendMessageToEvent" + " " + fWUpdateInfo.ProcessName + " " + fWUpdateInfo.ProcessProgress + " " + DateTime.Now);
        }
    }
}