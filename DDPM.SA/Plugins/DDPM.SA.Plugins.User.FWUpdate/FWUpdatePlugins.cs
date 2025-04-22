#define IL_Ready
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
using System.ServiceProcess;
using System.IO.Compression;
using DDPM.SA.Resources.Helper;
using System.Windows;
using System.Net.NetworkInformation;
using System.Globalization;
using DDDPM.SA.Common;
using Microsoft;
using System.Reflection;
using FirmwareUpdater;


namespace DDPM.SA.Plugins.User.FWUpdate
{
    [Plugin(IDs.FWUPDATE_PLUGIN_ID, pluginName, PluginOrderGroupType.Core, Version = pluginVersion)]
    [Descriptor(Description = pluginDescription)]
    [Publisher(Name = publisherCompany, Website = publisherWebsite, Support = publisherSupport)]
    [PublishedUnelevatedInterface(new[] { typeof(IFWUpdateService) })]
    [DependencyKnownTypes(new[] { typeof(IFWUpdateService), typeof(ISettingsManagerSA) })]
    public class FWUpdatePlugins : BaseAgentPlugin, IDisposableObservable, IFWUpdateService
    {
        public enum log_type
        {
            info = 0,
            error
        }
        /// <summary>
        /// //
        /// </summary>
        /// <param name="text"></param>
        /// <param name="log_type">0 means info, others means error</param>
        private void WriteLog(string text, log_type log_type = log_type.info,
            [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
            [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
            [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0)
        {
            if (string.IsNullOrEmpty(text))
                text = "";

            text = $"[FWUpdatePlugins] {text}, Caller Name:{memberName}, Source Line {sourceLineNumber}";
#if DEBUG
            Console.WriteLine(text);
#endif
            if (Log != null)
            {
                if (log_type == log_type.info)
                    Log.Info(text);
                else
                    Log.Error(text);
            }
        }

        public static readonly string[] ODM = new string[] { "Chicony", "Primax", "LiteON", "Darfon", "Wacom", "Luxshare", "Wistron", "Horn", "Tymphany", "Dell" };
        private static readonly object lockObject = new object();
        #region Private Members

        private const string pluginName = "FWUpdatePlugin";
        private const string pluginVersion = "1.0.0";
        private const string pluginDescription = "This plugin implements FW Update Plugin.";
        private const string publisherCompany = "Dell Technologies";
        private const string publisherWebsite = "https://www.dell.com";
        private const string publisherSupport = "This plugin implements FW Update Plugin.";

        private IAgent _agent;

        #endregion Private Members

        public const string PluginLogId = "FWUpdate";

        private Logs _logs;
        private Log? _ILogs;

        static bool _IsSkipCA = false;
        static bool _IsSkipSHA = false;
        private readonly object _PluginConditionLock_Settings = new object();
        /// <summary>
        /// 現在正在進行下載或安裝流程的裝置資訊
        /// </summary>
        private FWUpdateInfo _fWUpdateInfo = new FWUpdateInfo();

        /// <summary>
        /// 從DeviceManager取得的連接的裝置資訊列表，用於更新韌體前確認是否有插入多個Dock或裝置電量是否足夠
        /// </summary>
        private List<DeviceInfo> _DeviceInfos;
        /// <summary>
        /// 從DeviceManager取得的連接的IO Dongle，用於更新韌體前確認是否有插入多個Dongle
        /// </summary>
        private int _IODongleCount;

        /// <summary>
        /// 給UI或是CLI的全部韌體更新包
        /// </summary>
        private FWUpdateInfoPackage _fWUpdateInfoPackage;

        private Download? download = null;

        //安裝更新檔使用的命名管道伺服器
        private NamedPipeStreamServer? _namedPipeServer;

        private Process _clientProcess = new Process();
        private Timer _downloadTimer = new Timer();
        private Timer _checkUODTimer;
        private Timer _timerTimeOut;
        private Timer _DisplayProgressTimer;
        private string _notificationStr = "";
        private string _notificationTitle = "";
        private FWUErrorCode _updateErrorCode;
        private bool _IsShowNotify = true;
        string _ProgressLogPath = string.Empty;
        bool _IsUITrigger = true;
        string originalDirectory;

        /// <summary>
        /// 用於倒數次數計算
        /// </summary>
        private int _timeOutCount;

        /// <summary>
        /// 用於設定逾時時間預設120次/秒
        /// </summary>
        private int _fwTimeOutCount = 120;
        private bool _IsDownloadAndInsytall = false;
        private KeyGenerator? _KeyGenerator;
        private double _CurrentProcess = 0;
        /// <summary>
        /// 給沒有支援in-app的ISP使用，用於顯示進度%數
        /// </summary>
        private int _DisplayProgress = 0;

        #region Events

        /// <summary>
        /// 回傳更新事件進度
        /// </summary>
        public event EventHandler<UpdateProgressInfo>? ProgressUpdate_Notify;

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
        public event EventHandler<(string, string, bool)> CallOSD;

        #endregion Events

        public FWUpdatePlugins(IAgent agent) : base(agent, PluginLogId)
        {
            _agent = agent;
            _agent.PluginManager.PluginsStarted += PluginManagerOnPluginsStarted;
            _logs ??= new Logs(Log, PluginLogId);
            _fWUpdateInfoPackage = new FWUpdateInfoPackage();
            DdpmSACommonHelper.SAPluginReady(nameof(FWUpdatePlugins));
        }

        #region Overriding methods

        #region IDisposableObservable Support
        private CancellationTokenSource _CancellationTokenSource;

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
                IsDisposed = true;
                if (disposing)
                {
                    StartService();
                    if (_CancellationTokenSource != null)
                    {
                        _CancellationTokenSource.Cancel();
                    }
                    if (_checkUODTimer != null)
                    {
                        _checkUODTimer.Stop();
                        _checkUODTimer.Elapsed -= CheckDockUODScheduleTimer_Elapsed;
                        _checkUODTimer.Dispose();
                        _checkUODTimer = null;
                    }
                    resetState();
                    _agent.PluginManager.PluginsStarted -= PluginManagerOnPluginsStarted;
                    _agent = null;
                }
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
#if DEBUG
                Console.WriteLine("IFWUpdateService plugin started.");
#endif
            }
        }

        #endregion Event Handler

        protected override void OnPluginStarting()
        {
            _agent.PluginManager.PluginsStarted += PluginManagerOnPluginsStarted;
            PluginCondition = new PluginStartedCondition();
#if DEBUG
            Console.WriteLine("FWUpdate plugin report started");
#endif
        }

        #endregion Overriding methods


        public void SetDeviceinfo(List<DeviceInfo> DeviceInfos, int Gen3AgeDongleCount)
        {
            _IODongleCount = Gen3AgeDongleCount;
            if (DeviceInfos != null && DeviceInfos.Count > 0)
            {
                _DeviceInfos = DeviceInfos;
            }
        }
        public void SetLang(CultureInfo cultureInfo)
        {
            _logs.DebugMsg_1($"SetLang start");
            _logs.DebugMsg_1($"cultureInfo : {cultureInfo}");
            LangHelper.UserMappedCultureInfo = cultureInfo;
            _logs.DebugMsg_1($"SetLang done");
        }

        public void CheckUODFWUInfo(DokcUODUpdateInfoPackage UODFWUInfo, List<DeviceInfo>? DeviceInfos)
        {
            _logs.DebugMsg_1($"CheckUODFWUInfo start");
            string s = "";
            if (DeviceInfos != null && DeviceInfos.Count > 0 && UODFWUInfo != null && UODFWUInfo.FWUpdateInfo != null)
            {
                _logs.DebugMsg_1($"CheckUODFWUInfo if 1");
                foreach (DeviceInfo deviceInfo in DeviceInfos)
                {
                    if (IsDisposed)
                    {
                        _logs.DebugMsg_1($"CheckUODFWUInfo IsDisposed");
                        break;
                    }
                    _logs.DebugMsg_1($"CheckUODFWUInfo deviceInfo.DockServiceTag : {deviceInfo.DockServiceTag}");
                    _logs.DebugMsg_1($"CheckUODFWUInfo UODFWUInfo.FWUpdateInfo.ServiceTag : {UODFWUInfo.FWUpdateInfo.ServiceTag}");
                    if ((deviceInfo.PhysicalDeviceType == DeviceType.LogicalDock ||
                        deviceInfo.PhysicalDeviceType == DeviceType.PhysicalWiredDock) &&
                        deviceInfo.DockServiceTag.Equals(UODFWUInfo.FWUpdateInfo.ServiceTag))
                    {
                        _logs.DebugMsg_1($"CheckUODFWUInfo deviceInfo.FirmwareVersion : {deviceInfo.FirmwareVersion}");
                        _logs.DebugMsg_1($"CheckUODFWUInfo UODFWUInfo.FWUpdateInfo.TheLatestVersion : {UODFWUInfo.FWUpdateInfo.TheLatestVersion}");
                        string currentVer = deviceInfo.FirmwareVersion;
                        if (!string.IsNullOrEmpty(currentVer) &&
                                !currentVer.Contains("."))
                        {
                            if (currentVer.Length < 5) //長度小於5
                            {
                                if (currentVer.Length < 4) // 長度不足4,就補0在字首到長度為4
                                    currentVer = currentVer.PadLeft(4, '0');
                                currentVer = Regex.Replace(currentVer, ".{1}", "$0.").Substring(0, (currentVer.Length * 2) - 1);
                            }
                            else if (currentVer.Length > 4) // 長度大於4
                            {
                                //FF.FF.FF.FF(testing) or
                                //01004501 => 01.00.45.01 / 00011600 => 00.01.16.00(production)

                                if (currentVer.Length < 8) // 長度不足8,就補0在字首到長度為8
                                    currentVer = currentVer.PadLeft(8, '0');

                                //FF.FF.FF.FF(testing) or
                                //00001541 => 1.5.4.1; 00001064 => 1.0.6.4(production)
                                if (currentVer.StartsWith("0000")) // 檢查前4個字元是否都為0
                                {
                                    currentVer = currentVer.Substring(4);
                                    currentVer = Regex.Replace(currentVer, ".{1}", "$0.").Substring(0, (currentVer.Length * 2) - 1);
                                }
                                else
                                {
                                    string pattern = @"(.{2})(.{2})(.{2})(.{2})";
                                    string replacement = "$1.$2.$3.$4";
                                    currentVer = Regex.Replace(currentVer, pattern, replacement);
                                }
                            }
                            _logs.DebugMsg_1($" CheckUODFWUInfo(), currentVer (production output) = {currentVer}");
                        }
                        if (currentVer.Equals(UODFWUInfo.FWUpdateInfo.TheLatestVersion))
                        {
                            s = $"{deviceInfo.ModelNumber} UOD update completed.";
                        }
                        else
                        {
                            s = $"{deviceInfo.ModelNumber} UOD update fail.";
                        }
                        UODFWUInfo = new DokcUODUpdateInfoPackage();
                        CallSaveUODFWDeviceInfos?.AsyncFireAndForget(this, UODFWUInfo, System.Threading.CancellationToken.None);
                        if (_checkUODTimer != null)
                        {
                            _checkUODTimer.Stop();
                            _checkUODTimer.Elapsed -= CheckDockUODScheduleTimer_Elapsed;
                            _checkUODTimer.Dispose();
                            _checkUODTimer = null;
                        }
                    }
                }
            }
            else if (UODFWUInfo != null && UODFWUInfo.FWUpdateInfo != null && !string.IsNullOrEmpty(UODFWUInfo.FWUpdateInfo.ServiceTag))
            {
                _logs.DebugMsg_1($"CheckUODFWUInfo UODFWUInfo.FWUpdateInfo.ServiceTag is null : {string.IsNullOrEmpty(UODFWUInfo.FWUpdateInfo.ServiceTag)}");
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
                        _checkUODTimer.Elapsed += CheckDockUODScheduleTimer_Elapsed;
                        _checkUODTimer.Start();
                    }
                    if (UODFWUInfo.SaveTime == null)
                    {
                        s = LangHelper.Instance["Dock_FW_is_loaded"];
                        UODFWUInfo.SaveTime = DateTime.Now;
                        CallSaveUODFWDeviceInfos?.AsyncFireAndForget(this, UODFWUInfo, System.Threading.CancellationToken.None);
                    }
                    else
                    {
                        if (difference.TotalHours >= 24)
                        {
                            s = LangHelper.Instance["Dock_FW_is_loaded"];
                            UODFWUInfo.SaveTime = DateTime.Now;
                            CallSaveUODFWDeviceInfos?.AsyncFireAndForget(this, UODFWUInfo, System.Threading.CancellationToken.None);
                        }
                    }
                }
            }
            if (!string.IsNullOrEmpty(s))
            {
                NotificationFWupdate(LangHelper.Instance["Dock_UOD_FW_update_info"], s);
            }
            _logs.DebugMsg_1($"CheckUODFWUInfo done");
        }

        /// <summary>
        /// 取得更新的資訊包
        /// </summary>
        /// <param name="updateHelper">IL的更新資訊</param>
        /// <param name="isShowNotify">是否顯示右下角通知圖示</param>
        /// <returns>回傳更新資訊包</returns>
        public Task<FWUpdateInfoPackage> GetFWUpdateInfo(UpdateHelper updateHelper, List<DeviceInfo> deviceInfos, DisplayUpdateHelper displayUpdateHelper, bool reScan)
        {
            if (reScan && !_IsDownloadAndInsytall)
            {
                CheckUpdate(updateHelper, deviceInfos, displayUpdateHelper);
            }
            return Task.FromResult(_fWUpdateInfoPackage);
        }

        /// <summary>
        /// 檢查更新資訊
        /// </summary>
        /// <param name="updateHelper">IL的更新資訊</param>
        /// <param name="isShowNotify">是否顯示右下角通知圖示</param>
        /// <returns>回傳裝置資訊表(如果有需強制安裝更新的話，該裝置資訊表會被寫入對應裝置的安裝結果)</returns>
        private void CheckUpdate(UpdateHelper updateHelper, List<DeviceInfo> deviceInfos, DisplayUpdateHelper displayUpdateHelper)
        {
            _logs.DebugMsg_1(nameof(CheckUpdate) + " start");
            _fWUpdateInfoPackage.Clear();
            try
            {
                _fWUpdateInfoPackage.TheLastCheckTime = DateTime.Now;
                if (updateHelper != null && updateHelper.UpdateItems != null && updateHelper.UpdateItems.Count > 0 &&
                    deviceInfos != null && deviceInfos.Count > 0)
                {
                    _logs.DebugMsg_1($"updateHelper.UpdateItems.Count = {updateHelper.UpdateItems.Count}");
                    for (int i = 0; i < updateHelper.UpdateItems.Count; i++)
                    {
                        if (IsDisposed)
                        {
                            _logs.DebugMsg_1($"CheckUpdate updateHelper IsDisposed");
                            break;
                        }
                        try
                        {
                            _logs.DebugMsg_1($"updateHelper.UpdateItems[{i}].DeviceName = {updateHelper.UpdateItems[i].DeviceName}");
                            _logs.DebugMsg_1($"updateHelper.UpdateItems[{i}].DeviceModelNumber = {updateHelper.UpdateItems[i].DeviceModelNumber}");
                            DeviceInfo? deviceInfo = deviceInfos.Find(o => o.ID.ToString().Equals(updateHelper.UpdateItems[i].DeviceId.Replace("{", "").Replace("}", "")));
                            string deviceConnectivity = string.Empty;
                            string deviceSupplierID = updateHelper.UpdateItems[i].SupplierID;
                            _logs.DebugMsg_1($"updateHelper.UpdateItems[{i}].SupplierID = {deviceSupplierID}");
                            string oldVer = updateHelper.UpdateItems[i].CurrentVersion;
                            if (deviceInfo != null)
                            {
                                _logs.DebugMsg_1($"{nameof(CheckUpdate)} {nameof(deviceInfo)} is no null");
                                deviceConnectivity = GetConnected(deviceInfo.PhysicalDeviceType);
                                if (string.IsNullOrEmpty(deviceSupplierID))
                                {
                                    deviceSupplierID = GetODM(deviceInfo.OdmId);
                                    _logs.DebugMsg_1($"GetODM deviceSupplierID = {deviceSupplierID}");
                                }
                                oldVer = deviceInfo.FirmwareVersion;
                            }
                            _logs.DebugMsg_1($"updateHelper.UpdateItems[{i}].NewVersion = {updateHelper.UpdateItems[i].NewVersion}");
                            string newVer = updateHelper.UpdateItems[i].NewVersion;
                            if (!string.IsNullOrEmpty(newVer))
                            {
                                // Jim 20241227 Comment out
                                //newVer = Regex.Replace(updateHelper.UpdateItems[i].NewVersion, ".{1}", "$0.").Substring(0, (updateHelper.UpdateItems[i].NewVersion.Length * 2) - 1);

                                // Jim 20241227 add to  PIMS-329393 DDPM is sending smart dock version as f.f.f.f.f.f.f.f
                                string strBackup = string.Empty;
                                string strTemp = string.Empty;

                                strBackup = newVer;
                                _logs.DebugMsg_1($" CheckUpdate(), newVer (original input) = {newVer}");
                                _logs.DebugMsg_1($" CheckUpdate(), strBackup = {strBackup}");

                                if (!string.IsNullOrEmpty(strBackup))
                                {
                                    if (strBackup.Length < 5) //長度小於5
                                    {
                                        if (strBackup.Length < 4) // 長度不足4,就補0在字首到長度為4
                                            strBackup = strBackup.PadLeft(4, '0');
                                        newVer = Regex.Replace(strBackup, ".{1}", "$0.").Substring(0, (strBackup.Length * 2) - 1);

                                    }
                                    else if (strBackup.Length > 4) // 長度大於4
                                    {
                                        //FF.FF.FF.FF(testing) or
                                        //01004501 => 01.00.45.01 / 00011600 => 00.01.16.00(production)

                                        if (strBackup.Length < 8) // 長度不足8,就補0在字首到長度為8
                                            strBackup = strBackup.PadLeft(8, '0');

                                        //FF.FF.FF.FF(testing) or
                                        //00001541 => 1.5.4.1; 00001064 => 1.0.6.4(production)
                                        if (strBackup.StartsWith("0000")) // 檢查前4個字元是否都為0
                                        {
                                            strTemp = strBackup.Substring(4);
                                            newVer = Regex.Replace(strTemp, ".{1}", "$0.").Substring(0, (strTemp.Length * 2) - 1);
                                        }
                                        else
                                        {
                                            string pattern = @"(.{2})(.{2})(.{2})(.{2})";
                                            string replacement = "$1.$2.$3.$4";
                                            newVer = Regex.Replace(strBackup, pattern, replacement);
                                        }
                                    }
                                }
                                _logs.DebugMsg_1($" CheckUpdate(), newVer (production output) = {newVer}");
                            }
                            _logs.DebugMsg_1($"updateHelper.UpdateItems[{i}].oldVer = {oldVer}");
                            if (!string.IsNullOrEmpty(oldVer) &&
                                !oldVer.Contains("."))
                            {
                                if (oldVer.Length < 5) //長度小於5
                                {
                                    if (oldVer.Length < 4) // 長度不足4,就補0在字首到長度為4
                                        oldVer = oldVer.PadLeft(4, '0');
                                    oldVer = Regex.Replace(oldVer, ".{1}", "$0.").Substring(0, (oldVer.Length * 2) - 1);
                                }
                                else if (oldVer.Length > 4) // 長度大於4
                                {
                                    //FF.FF.FF.FF(testing) or
                                    //01004501 => 01.00.45.01 / 00011600 => 00.01.16.00(production)

                                    if (oldVer.Length < 8) // 長度不足8,就補0在字首到長度為8
                                        oldVer = oldVer.PadLeft(8, '0');

                                    //FF.FF.FF.FF(testing) or
                                    //00001541 => 1.5.4.1; 00001064 => 1.0.6.4(production)
                                    if (oldVer.StartsWith("0000")) // 檢查前4個字元是否都為0
                                    {
                                        oldVer = oldVer.Substring(4);
                                        oldVer = Regex.Replace(oldVer, ".{1}", "$0.").Substring(0, (oldVer.Length * 2) - 1);
                                    }
                                    else
                                    {
                                        string pattern = @"(.{2})(.{2})(.{2})(.{2})";
                                        string replacement = "$1.$2.$3.$4";
                                        oldVer = Regex.Replace(oldVer, pattern, replacement);
                                    }
                                }
                                _logs.DebugMsg_1($" CheckUpdate(), oldVer (production output) = {oldVer}");
                            }
                            _logs.DebugMsg_1($"updateHelper.UpdateItems[{i}] go add list");
                            FWUpdateInfo fWUpdateInfo = new FWUpdateInfo()
                            {
                                TheLatestVersion = newVer,
                                DeviceVersion = oldVer,
                                NeedUpdated = true,
                                ServerPath = updateHelper.UpdateItems[i].ServerPath,
                                FileSavepath = updateHelper.UpdateItems[i].InstallPath,
                                Model = updateHelper.UpdateItems[i].DeviceModelNumber,
                                DeviceName = updateHelper.UpdateItems[i].DeviceName,
                                DeviceType = updateHelper.UpdateItems[i].DeviceType,
                                DeviceId = updateHelper.UpdateItems[i].DeviceId,
                                DevicePath = updateHelper.UpdateItems[i].DevicePath,
                                DeviceIndex = updateHelper.UpdateItems[i].DeviceIndex,
                                SHA256 = updateHelper.UpdateItems[i].SHA256,
                                //SHA512 = updateHelper.UpdateItems[i].SHA512,
                                Thumbprint = updateHelper.UpdateItems[i].Thumbprint,
                                //IsUOD = (isUODMode &&
                                //     (updateHelper.UpdateItems[i].DeviceType == DeviceType.PhysicalWiredDock ||
                                //     updateHelper.UpdateItems[i].DeviceType == DeviceType.LogicalDock)),
                                IsDisplay = false,
                                SupplierID = deviceSupplierID,
                                InstanceId = updateHelper.UpdateItems[i].InstanceId,
                                Connectivity = deviceConnectivity,
                                UpdateTime = "",
                                Available_date = _fWUpdateInfoPackage.TheLastCheckTime.ToString("yyyy/MM/dd HH:mm:ss"),
                                ServiceTag = ((updateHelper.UpdateItems[i].DeviceType == DeviceType.PhysicalWiredDock ||
                                     updateHelper.UpdateItems[i].DeviceType == DeviceType.LogicalDock) && deviceInfo != null) ? deviceInfo.DockServiceTag : "",
                                IsESISupported = ((updateHelper.UpdateItems[i].DeviceType == DeviceType.PhysicalWebcam ||
                                     updateHelper.UpdateItems[i].DeviceType == DeviceType.LogicalWebcam) && deviceInfo != null) ? deviceInfo.IsESISupported : false,

                            };
                            if (Check_CanBeOTAUpdate(fWUpdateInfo))
                            {
                                _logs.DebugMsg_1($"Peripheral _fWUpdateInfoPackage.FWUpdateInfo.Add : {fWUpdateInfo.Model}");
                                _fWUpdateInfoPackage.FWUpdateInfo.Add(fWUpdateInfo);
                            }
                        }
                        catch (Exception ex)
                        {
                            //_logs.Error($"updateHelper.UpdateItems[{i}] Error : {ex.Message}");
                            WriteLog($"updateHelper.UpdateItems[{i}] Error : {ex.Message}", log_type.error);
                        }
                    }
                }
                if (displayUpdateHelper != null && displayUpdateHelper.Firmwares != null && displayUpdateHelper.Firmwares.Count > 0)
                {
                    _logs.DebugMsg_1($"displayUpdateHelper.Firmwares.Count = {displayUpdateHelper.Firmwares.Count}");
                    for (int i = 0; i < displayUpdateHelper.Firmwares.Count; i++)
                    {
                        if (IsDisposed)
                        {
                            _logs.DebugMsg_1($"CheckUpdate displayUpdateHelper IsDisposed");
                            break;
                        }
                        try
                        {
                            _logs.DebugMsg_1($"displayUpdateHelper.Firmwares[{i}].id(DeviceName) = {displayUpdateHelper.Firmwares[i].id}");
                            _logs.DebugMsg_1($"displayUpdateHelper.Firmwares[{i}].TheLastVersion = {displayUpdateHelper.Firmwares[i].TheLastVersion}");
                            _logs.DebugMsg_1($"displayUpdateHelper.Firmwares[{i}].CurrentVersion = {displayUpdateHelper.Firmwares[i].CurrentVersion}");
                            FWUpdateInfo fWUpdateInfo = new FWUpdateInfo()
                            {
                                TheLatestVersion = displayUpdateHelper.Firmwares[i].TheLastVersion,
                                DeviceVersion = displayUpdateHelper.Firmwares[i].CurrentVersion,
                                NeedUpdated = true,
                                DeviceType = DeviceType.Unknown,
                                ServerPath = displayUpdateHelper.Firmwares[i].url,
                                Model = displayUpdateHelper.Firmwares[i].id,
                                DeviceName = "Dell Monitor",
                                SHA256 = displayUpdateHelper.Firmwares[i].SHA256,
                                //SHA512 = displayUpdateHelper.Firmwares[i].SHA512,
                                Thumbprint = displayUpdateHelper.Firmwares[i].Thumbprint,
                                ServiceTag = displayUpdateHelper.Firmwares[i].ServiceTag,
                                IsUOD = false,
                                IsDisplay = true,
                                SupplierID = displayUpdateHelper.Firmwares[i].SupplierID,
                                D_Ctrl = displayUpdateHelper.Firmwares[i].D_Ctrl,
                                UpdateTime = displayUpdateHelper.Firmwares[i].UpdateTime,
                                Available_date = _fWUpdateInfoPackage.TheLastCheckTime.ToString("yyyy/MM/dd HH:mm:ss")
                            };
                            _logs.DebugMsg_1($"Display _fWUpdateInfoPackage.FWUpdateInfo.Add : {fWUpdateInfo.Model}");
                            _fWUpdateInfoPackage.FWUpdateInfo.Add(fWUpdateInfo);
                        }
                        catch (Exception ex)
                        {
                            _logs.DebugMsg_1($"displayUpdateHelper.Firmwares[{i}] error : {ex.Message}");
                        }
                    }
                }
                _logs.DebugMsg_1($"_fWUpdateInfoPackage.FWUpdateInfo.Count : {_fWUpdateInfoPackage.FWUpdateInfo.Count}");
                if (_fWUpdateInfoPackage.FWUpdateInfo.Count > 0)
                {
                    if (deviceInfos != null)
                    {
                        //Bruce 02/19 If multiple docks are docked consecutively, all docks will remove
                        int isDockCanFWUCount = _fWUpdateInfoPackage.FWUpdateInfo.FindAll(x => x.DeviceType.Equals(DeviceType.LogicalDock) || x.DeviceType.Equals(DeviceType.PhysicalWiredDock)).Count;
                        int isDockConnectCount = deviceInfos.FindAll(x => x.PhysicalDeviceType.Equals(DeviceType.LogicalDock) || x.PhysicalDeviceType.Equals(DeviceType.PhysicalWiredDock)).Count;
                        _logs.DebugMsg_1($"isDockCanFWUCount : {isDockCanFWUCount}");
                        _logs.DebugMsg_1($"isDockConnectCount : {isDockConnectCount}");
                        if (isDockCanFWUCount >= 1 && isDockConnectCount <= 0)
                        {
                            _fWUpdateInfoPackage.FWUpdateInfo.RemoveAll(x => x.DeviceType.Equals(DeviceType.LogicalDock) || x.DeviceType.Equals(DeviceType.PhysicalWiredDock));
                        }
                    }
                    _logs.DebugMsg_1(nameof(CheckUpdate) + " done.");
                }
                else
                {
                    _logs.DebugMsg_1(nameof(CheckUpdate) + " no updates available.");
                }
            }
            catch (Exception ex)
            {
                _logs.DebugMsg_1($"{nameof(CheckUpdate)} Error: {ex.Message}");
            }
        }
        /// <summary>
        /// 從伺服端下載更新檔，下載後會接續執行安裝方法
        /// </summary>
        /// <param name="fwUpdateInfos">更新的裝置資訊表</param>
        /// <returns>回傳裝置資訊表(在這個方法裡將原本傳入的裝置資訊表，再寫入對應裝置的下載安裝的結果碼)</returns>
        public Task<List<FWUpdateInfo>> DownloadAndInstall(List<FWUpdateInfo> fwUpdateInfos, List<DeviceInfo> currentDevice, int IODongleCountGen3AgoCount, bool isUITrigger, bool isShowNotify, string installPath)
        {
            lock (lockObject)
            {
                _IsDownloadAndInsytall = true;
                _IsUITrigger = isUITrigger;
                _IsShowNotify = isShowNotify;
                originalDirectory = DDPMFileSecurity.SanitizePath(Directory.GetCurrentDirectory(), out string info);
                _logs.DebugMsg_1($"{nameof(DownloadAndInstall)} DDPMFileSecurity.SanitizePath info : {info}");
                Method method = new Method(_logs);
                try
                {
                    if (fwUpdateInfos.Count > 0)
                    {
                        UpdateProgressInfo updateProgressInfo = new UpdateProgressInfo()
                        {
                            DeviceName = fwUpdateInfos[0].DeviceName,
                            Model = fwUpdateInfos[0].Model,
                            IsDisplay = fwUpdateInfos[0].IsDisplay,
                            UpdateTime = fwUpdateInfos[0].UpdateTime,
                            TheLatestVersion = _fWUpdateInfo.TheLatestVersion,
                            ProcessName = "Downloading",
                            ProcessProgress = 0,
                        };
                        sendMessageToEvent(updateProgressInfo);
                    }
                    _logs.DebugMsg_1(nameof(DownloadAndInstall) + " all start");
                    _logs.DebugMsg_1(nameof(DownloadAndInstall) + " fwUpdateInfos.Count : " + fwUpdateInfos.Count);
                    List<FWUpdateInfo> temp_FWUpdateInfo = fwUpdateInfos.FindAll(o => o.IsDisplay);
                    //判斷是否有非Display更新，有的話停止DPM
                    if (temp_FWUpdateInfo.Count != fwUpdateInfos.Count)
                    {
                        StopService();
                    }
                    _logs.DebugMsg_1($"{nameof(DownloadAndInstall)} Rearrange go");
                    fwUpdateInfos = Rearrange(fwUpdateInfos);
                    _logs.DebugMsg_1($"{nameof(DownloadAndInstall)} Rearrange done");
                    for (int i = 0; i < fwUpdateInfos.Count; i++)
                    {
                        _fWUpdateInfo = fwUpdateInfos[i];
                        if (IsDisposed)
                        {
                            _logs.DebugMsg_1($"DownloadAndInstall IsDisposed");
                            break;
                        }
                        _logs.DebugMsg_1($"{nameof(DownloadAndInstall)} DeviceName : {_fWUpdateInfo.DeviceName} Model : {_fWUpdateInfo.Model} start");
                        UpdateProgressInfo updateProgressInfo = new UpdateProgressInfo()
                        {
                            DeviceName = _fWUpdateInfo.DeviceName,
                            Model = _fWUpdateInfo.Model,
                            IsDisplay = _fWUpdateInfo.IsDisplay,
                            UpdateTime = _fWUpdateInfo.UpdateTime,
                            TheLatestVersion = _fWUpdateInfo.TheLatestVersion,
                            ProcessName = "Downloading",
                            ProcessProgress = 0,
                        };
                        sendMessageToEvent(updateProgressInfo);
                        string path_programdata = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
                        string saveFolderName = Guid.NewGuid().ToString();
                        string savePath;
                        DDPMFileSecurity DDPMFileSecurity = new DDPMFileSecurity();
                        if (string.IsNullOrEmpty(installPath))
                        {
                            if (!string.IsNullOrEmpty(path_programdata))
                            {
                                savePath = path_programdata + "\\Dell\\Dell Display and Peripheral Manager" + "\\" + saveFolderName + "\\";
                            }
                            else
                            {
                                foreach (FWUpdateInfo fwUpdateInfo in fwUpdateInfos)
                                {
                                    fwUpdateInfo.FWUErrorCode = FWUErrorCode.FileCheckFail;
                                }
                                _logs.DebugMsg_1($"{nameof(DownloadAndInstall)} path_programdata get error");
                                return Task.FromResult(fwUpdateInfos);
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
                        //0926 Bruce Add Security
                        //if (!CheckFold(savePath, out string FolderInfo, out string PathSymbolicLinInfo))
                        if (!DDPMFileSecurity.CheckFold(savePath, out string FolderInfo, out string PathSymbolicLinInfo))
                        {
                            foreach (FWUpdateInfo fwUpdateInfo in fwUpdateInfos)
                            {
                                fwUpdateInfo.FWUErrorCode = FWUErrorCode.FolderIsNotSafe;
                            }
                            _notificationStr = LangHelper.Instance["Firmware_update_unsuccessful"];
                            NotificationFWupdate(LangHelper.Instance["Error"], _notificationStr);
                            _logs.DebugMsg_1(nameof(DownloadAndInstall) + " savePath FolderIsNotSafe:" + FolderInfo + "--or--" + PathSymbolicLinInfo);
                            method.DeleteFolder(savePath);
                            return Task.FromResult(fwUpdateInfos);
                        }
                        _notificationStr = "";
                        _notificationTitle = "";
                        _updateErrorCode = FWUErrorCode.Unknow;
                        fwUpdateInfos[i].FWUErrorCode = _updateErrorCode;
                        if (!fwUpdateInfos[i].IsDisplay &&
                            CheckDeviceStatus_IsStopUpdate(fwUpdateInfos[i], currentDevice, IODongleCountGen3AgoCount, out FWUErrorCode isStopUpdateError))
                        {
                            fwUpdateInfos[i].FWUErrorCode = isStopUpdateError;
                            NotificationFWupdate(LangHelper.Instance["Error"], _notificationStr);
                            method.DeleteFolder(savePath);
                            continue;
                        }
                        if (CheckPCBattery_IsStopUpdate(fwUpdateInfos[i]))
                        {
                            fwUpdateInfos[i].FWUErrorCode = FWUErrorCode.PCBatteryTooLow;
                            NotificationFWupdate(LangHelper.Instance["Error"], _notificationStr);
                            method.DeleteFolder(savePath);
                            continue;
                        }
                        string url = fwUpdateInfos[i].ServerPath;
                        //0926 Bruce Add Security
                        //if (!CheckFold(savePath, out FolderInfo, out PathSymbolicLinInfo))
                        if (!DDPMFileSecurity.CheckFold(savePath, out FolderInfo, out PathSymbolicLinInfo))
                        {
                            fwUpdateInfos[i].FWUErrorCode = FWUErrorCode.FolderIsNotSafe;
                            _notificationStr = LangHelper.Instance["Firmware_update_unsuccessful"];
                            NotificationFWupdate(LangHelper.Instance["Error"], _notificationStr);
                            _logs.DebugMsg_1($"{nameof(DownloadAndInstall)} savePath FolderIsNotSafe - FolderInfo : {FolderInfo}");
                            _logs.DebugMsg_1($"{nameof(DownloadAndInstall)} savePath FolderIsNotSafe - PathSymbolicLinInfo : {PathSymbolicLinInfo}");
                            method.DeleteFolder(savePath);
                            continue;
                        }
                        if (!NetworkInterface.GetIsNetworkAvailable())
                        {
                            fwUpdateInfos[i].FWUErrorCode = FWUErrorCode.NetworkDisconnection;
                            _notificationStr = LangHelper.Instance["Update_failed_due_to_network_error"];
                            NotificationFWupdate(LangHelper.Instance["Error"], _notificationStr);
                            _logs.DebugMsg_1(fwUpdateInfos[i].DeviceName + " Download File Fail : " + _notificationStr);
                            method.DeleteFolder(savePath);
                            continue;
                        }
                        string _installationFileStoragePath;
                        try
                        {
                            _CancellationTokenSource = new CancellationTokenSource();
                            _downloadTimer = new Timer();
                            _downloadTimer.Interval = 1000;
                            _downloadTimer.Elapsed += DownloadTimer_Elapsed;
                            _downloadTimer.Start();
                            download = new Download(_logs);
                            string downloadInfo = "";
                            // 將儲存路徑與從 URL 中提取的檔案名稱組合
                            if (_IsSkipSHA)
                            {
                                _logs.DebugMsg_1($"{nameof(DownloadAndInstall)} ServerPath : {GlobalDefinitions.GetLogPrintServerName(url)}");
                            }
                            _installationFileStoragePath = Path.Combine(savePath + Path.GetFileName(url));
                            _logs.DebugMsg_1($"{nameof(DownloadAndInstall)} download.DownloadFile go");
                            bool downloadRet = download.DownloadFile(url, _installationFileStoragePath, out downloadInfo, _IsSkipCA, _CancellationTokenSource);
                            _logs.DebugMsg_1($"{nameof(DownloadAndInstall)} download.DownloadFile finish");
                            _downloadTimer.Stop();
                            _downloadTimer.Elapsed -= DownloadTimer_Elapsed;
                            _downloadTimer.Dispose();
                            _downloadTimer = null;
                            if (!downloadRet)
                            {
                                if (downloadInfo.Equals("CA check fail"))
                                {
                                    _logs.DebugMsg_1($"{nameof(DownloadAndInstall)} CA check fail");
                                    fwUpdateInfos[i].FWUErrorCode = FWUErrorCode.CAFail;
                                    _notificationStr = LangHelper.Instance["Firmware_update_unsuccessful"];
                                    NotificationFWupdate(LangHelper.Instance["Error"], _notificationStr);
                                }
                                else
                                {
                                    _logs.DebugMsg_1($"{nameof(DownloadAndInstall)} Download fail: {downloadInfo}");
                                    fwUpdateInfos[i].FWUErrorCode = FWUErrorCode.NetworkDisconnection;
                                    _notificationStr = LangHelper.Instance["Update_failed_due_to_network_error"];
                                    NotificationFWupdate(LangHelper.Instance["Error"], _notificationStr);
                                }
                                _logs.DebugMsg_1(fwUpdateInfos[i].DeviceName + " Download File Fail");
                                method.DeleteFolder(savePath);
                                continue;
                            }
                            updateProgressInfo = new UpdateProgressInfo()
                            {
                                DeviceName = fwUpdateInfos[i].DeviceName,
                                Model = fwUpdateInfos[i].Model,
                                IsDisplay = fwUpdateInfos[i].IsDisplay,
                                UpdateTime = fwUpdateInfos[i].UpdateTime,
                                TheLatestVersion = _fWUpdateInfo.TheLatestVersion,
                                ProcessName = "Downloading",
                                ProcessProgress = 100,
                            };
                            sendMessageToEvent(updateProgressInfo);
                        }
                        catch (Exception ex)
                        {
                            resetState();
                            _logs.DebugMsg_1(fwUpdateInfos[i].DeviceName + " Download error : " + ex.Message);
                            fwUpdateInfos[i].FWUErrorCode = FWUErrorCode.NetworkDisconnection;
                            _notificationStr = LangHelper.Instance["Update_failed_due_to_network_error"];
                            NotificationFWupdate(LangHelper.Instance["Error"], _notificationStr);
                            method.DeleteFolder(savePath);
                            continue;
                        }

                        string extractPath = Path.Combine(savePath + Path.GetFileName(url).Substring(0, Path.GetFileName(url).Length - 4));
                        if (!Directory.Exists(extractPath))
                        {
                            Directory.CreateDirectory(extractPath);
                        }
                        //0926 Bruce Add Security
                        //if (!CheckFold(extractPath, out FolderInfo, out PathSymbolicLinInfo))
                        if (!DDPMFileSecurity.CheckFold(extractPath, out FolderInfo, out PathSymbolicLinInfo))
                        {
                            fwUpdateInfos[i].FWUErrorCode = FWUErrorCode.FolderIsNotSafe;
                            _logs.DebugMsg_1($"{fwUpdateInfos[i].DeviceName} extractPath FolderIsNotSafe - FolderInfo : {FolderInfo}");
                            _logs.DebugMsg_1($"{fwUpdateInfos[i].DeviceName} extractPath FolderIsNotSafe - PathSymbolicLinInfo : {PathSymbolicLinInfo}");
                            _notificationStr = LangHelper.Instance["Firmware_update_unsuccessful"];
                            NotificationFWupdate(LangHelper.Instance["Error"], _notificationStr);
                            method.DeleteFolder(savePath);
                            continue;
                        }
                        try
                        {
                            using (FileLock fileLock = new FileLock(_installationFileStoragePath, PathCheckOption.None, lockNow: true))
                            {
                                string exeFilePath;
                                if (!Unzip(_installationFileStoragePath, extractPath, out exeFilePath))
                                {
                                    fwUpdateInfos[i].FWUErrorCode = FWUErrorCode.FolderIsNotSafe;
                                    _logs.DebugMsg_1($"{fwUpdateInfos[i].DeviceName} Unzip Fail");
                                    _notificationStr = LangHelper.Instance["Firmware_update_unsuccessful"];
                                    NotificationFWupdate(LangHelper.Instance["Error"], _notificationStr);
                                    fileLock.Unlock();
                                    method.DeleteFolder(savePath);
                                    continue;
                                }
                                using (FileLock fileLock_2 = new FileLock(exeFilePath, PathCheckOption.None, lockNow: true))
                                {
                                    if (!CheckThumbprint(exeFilePath, _fWUpdateInfo.Thumbprint, out string FileCAInfo))
                                    {
                                        _fWUpdateInfo.FWUErrorCode = FWUErrorCode.FileCheckFail;
                                        _logs.DebugMsg_1($"{fwUpdateInfos[i].DeviceName} CheckThumbprint Faile");
                                        _notificationStr = LangHelper.Instance["Firmware_update_unsuccessful"];
                                        NotificationFWupdate(LangHelper.Instance["Error"], _notificationStr);
                                        fileLock_2.Unlock();
                                        fileLock.Unlock();
                                        method.DeleteFolder(savePath);
                                        continue;
                                    }
                                    fwUpdateInfos[i].InstallPaths = exeFilePath;
                                    fwUpdateInfos[i].FWUErrorCode = Install(fwUpdateInfos[i]);
                                    fwUpdateInfos[i].Update_date = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
                                    if (_ILogs != null)
                                    {
                                        _ILogs = null;
                                    }
                                }
                                _notificationStr = $"{_notificationStr.Replace("[XXXXXX]", $"{fwUpdateInfos[i].DeviceName} {fwUpdateInfos[i].Model}")}";
                                //_notificationStr = $"{fwUpdateInfos[i].DeviceName} {fwUpdateInfos[i].Model} {_notificationStr}";
                                if (fwUpdateInfos[i].FWUErrorCode == FWUErrorCode.NoError)
                                {
                                    if (_IsUITrigger)
                                    {
                                        NotificationFWupdate(LangHelper.Instance["Success"], _notificationStr);
                                    }
                                    else//for CLI
                                    {
                                        _notificationStr = LangHelper.Instance["Update_successful_body"].Replace("[XXXXXX]", $"{_fWUpdateInfo.DeviceName} ({_fWUpdateInfo.Model})");
                                        NotificationFWupdate(LangHelper.Instance["Update_successful"], _notificationStr);
                                    }
                                }
                                else
                                {
                                    if (_IsUITrigger)
                                    {
                                        if (string.IsNullOrWhiteSpace(_notificationTitle))
                                        {
                                            _notificationTitle = LangHelper.Instance["Error"];
                                        }
                                        NotificationFWupdate(_notificationTitle, _notificationStr);
                                    }
                                    else//for CLI
                                    {
                                        _notificationStr = LangHelper.Instance["Update_failed_body"].Replace("[XXXXXX]", $"{_fWUpdateInfo.DeviceName} ({_fWUpdateInfo.Model})");
                                        NotificationFWupdate(LangHelper.Instance["Update_failed"], _notificationStr);
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            _logs.DebugMsg_1($"{fwUpdateInfos[i].DeviceName} FileLock Error: {ex.Message}");
                            _notificationStr = LangHelper.Instance["Firmware_update_unsuccessful"];
                            NotificationFWupdate(LangHelper.Instance["Error"], _notificationStr);
                        }
                        method.DeleteFolder(savePath);
                        _logs.DebugMsg_1($"{nameof(DownloadAndInstall)} DeviceName : {fwUpdateInfos[i].DeviceName} Model : {fwUpdateInfos[i].Model} done");
                    }
                    _logs.DebugMsg_1($"{nameof(DownloadAndInstall)}, All done");
                    //判斷是否有非Display更新，有的話停止DPM
                    if (temp_FWUpdateInfo.Count != fwUpdateInfos.Count)
                    {
                        StartService();
                    }
                    // 設定當前工作目錄
                    Directory.SetCurrentDirectory(originalDirectory);
                    _IsDownloadAndInsytall = false;

                    // add @ 20250220 stephen : send fwupdate result event to cma
                    DownloadAndInstall_Result_Notify?.AsyncFireAndForget(this, fwUpdateInfos, System.Threading.CancellationToken.None);

                    return Task.FromResult(fwUpdateInfos);
                }
                catch (Exception ex)
                {
                    // 設定當前工作目錄
                    Directory.SetCurrentDirectory(originalDirectory);
                    resetState();
                    _notificationStr = LangHelper.Instance["Firmware_update_unsuccessful"];
                    NotificationFWupdate(LangHelper.Instance["Error"], _notificationStr);
                    _logs.DebugMsg_1($"{nameof(DownloadAndInstall)} {_fWUpdateInfo.DeviceName} {_fWUpdateInfo.Model} Error : {ex.Message}"); // 輸出錯誤訊息
                    StartService();
                    _IsDownloadAndInsytall = false;
                    return Task.FromResult(fwUpdateInfos);
                }
                finally
                {
                    method.Dispose();
                }
            }
        }

        public Task<FWUErrorCode> Install(string installPath, bool isOnlyDisplay, DeviceType deviceType)
        {
            _logs.DebugMsg_1($"{nameof(Install)} start");
            _logs.DebugMsg_1($"{nameof(Install)} installPath : ***");
            _logs.DebugMsg_1($"{nameof(Install)} deviceType : {deviceType}");
            FWUErrorCode ret = FWUErrorCode.Unknow;
            if (!string.IsNullOrEmpty(installPath))
            {
                try
                {
                    using (FileLock fileLock_2 = new FileLock(installPath, PathCheckOption.None, lockNow: true))
                    {
                        FWUpdateInfo fWUpdateInfo = new FWUpdateInfo()
                        {
                            DeviceType = deviceType,
                            InstallPaths = installPath,
                            IsDisplay = isOnlyDisplay
                        };
                        if (!isOnlyDisplay)
                        {
                            StopService();
                        }
                        ret = Install(fWUpdateInfo);
                        if (!isOnlyDisplay)
                        {
                            StartService();
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logs.DebugMsg_1($"{nameof(Install)} Error : {ex.Message}");
                }
            }
            else
            {
                _logs.DebugMsg_1($"{nameof(Install)} installPath is null");
            }
            _logs.DebugMsg_1($"{nameof(Install)} done");
            return Task.FromResult(ret);
        }
        public Task<bool> RestartService()
        {
            bool ret = false;
            string serviceName = "DPMService";
            try
            {
                using (ServiceController service = new ServiceController(serviceName))
                {
                    if (service.Status == ServiceControllerStatus.Running)
                    {
                        _logs.DebugMsg_1($"{nameof(RestartService)} is running");
                        service.Stop();
                        service.WaitForStatus(ServiceControllerStatus.Stopped);
                        _logs.DebugMsg_1($"{nameof(RestartService)} is stopped");
                    }
                    _logs.DebugMsg_1($"{nameof(RestartService)} is start");
                    service.Start();
                    service.WaitForStatus(ServiceControllerStatus.Running);
                    _logs.DebugMsg_1($"{nameof(RestartService)} is restart");
                }
                ret = true;
            }
            catch (Exception ex)
            {
                _logs.DebugMsg_1($"{nameof(RestartService)} Error: {ex.Message}");
            }
            return Task.FromResult(ret);
        }
        public Task<bool> StopService()
        {
            bool ret = false;
            string serviceName = "DPMService";
            try
            {
                using (ServiceController service = new ServiceController(serviceName))
                {
                    if (service.Status == ServiceControllerStatus.Running)
                    {
                        _logs.DebugMsg_1($"{nameof(StopService)} go");
                        service.Stop();
                        service.WaitForStatus(ServiceControllerStatus.Stopped);
                        _logs.DebugMsg_1($"{nameof(StopService)} done");
                    }
                    else
                    {
                        _logs.DebugMsg_1($"{nameof(StartService)} service is not Running");
                    }
                }
                ret = true;
            }
            catch (Exception ex)
            {
                _logs.DebugMsg_1($"{nameof(StopService)} Error: {ex.Message}");
            }
            return Task.FromResult(ret);
        }
        public Task<bool> StartService()
        {
            bool ret = false;
            string serviceName = "DPMService";
            try
            {
                using (ServiceController service = new ServiceController(serviceName))
                {
                    if (service.Status != ServiceControllerStatus.Running)
                    {
                        _logs.DebugMsg_1($"{nameof(StartService)} start go");
                        service.Start();
                        service.WaitForStatus(ServiceControllerStatus.Running);
                        _logs.DebugMsg_1($"{nameof(StartService)} start done");
                    }
                    else
                    {
                        _logs.DebugMsg_1($"{nameof(StartService)} service is Running");
                    }
                }
                ret = true;
            }
            catch (Exception ex)
            {
                _logs.DebugMsg_1($"{nameof(StartService)} Error: {ex.Message}");
            }
            return Task.FromResult(ret);
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
                UpdateProgressInfo updateProgressInfo = new UpdateProgressInfo()
                {
                    DeviceName = _fWUpdateInfo.DeviceName,
                    Model = _fWUpdateInfo.Model,
                    IsDisplay = _fWUpdateInfo.IsDisplay,
                    UpdateTime = _fWUpdateInfo.UpdateTime,
                    TheLatestVersion = _fWUpdateInfo.TheLatestVersion,
                    ProcessName = "Downloading",
                    ProcessProgress = download.GetProgress(),
                };
                sendMessageToEvent(updateProgressInfo);
            }
        }

        /// <summary>
        /// Check whether there are multiple docks plugged in, multiple of the same model, and whether the device has sufficient power.
        /// </summary>
        /// <param name="currentFWInfo">Firmware information currently to be updated</param>
        /// <returns>Two Docks are connected at the same time return true; otherwise return false.</returns>
        private bool CheckDeviceStatus_IsStopUpdate(FWUpdateInfo currentFWInfo, List<DeviceInfo> deviceInfos, int ioDongleCount, out FWUErrorCode fWUErrorCode)
        {
            _logs.DebugMsg_1($"{nameof(CheckDeviceStatus_IsStopUpdate)} start");
            bool ret = false;
            fWUErrorCode = FWUErrorCode.NoError;
            _logs.DebugMsg_1($"{nameof(CheckDeviceStatus_IsStopUpdate)} currentFWInfo.DeviceType: {currentFWInfo.DeviceType}");
            if (deviceInfos != null)
            {
                if (currentFWInfo.DeviceType == DeviceType.LogicalDock ||
                    currentFWInfo.DeviceType == DeviceType.PhysicalWiredDock)
                {
                    List<DeviceInfo> dock_deviceInfos = deviceInfos.FindAll(o => o.PhysicalDeviceType.Equals(DeviceType.LogicalDock) ||
                    o.PhysicalDeviceType.Equals(DeviceType.PhysicalWiredDock));
                    _logs.DebugMsg_1($"{nameof(CheckDeviceStatus_IsStopUpdate)} dock_deviceInfos is null : {(dock_deviceInfos == null ? "Yes" : "No")}");
                    if (dock_deviceInfos != null)
                    {
                        _logs.DebugMsg_1($"{nameof(CheckDeviceStatus_IsStopUpdate)} dock_deviceInfos.Count: {dock_deviceInfos.Count}");
                        if (dock_deviceInfos.Count >= 2)
                        {
                            fWUErrorCode = FWUErrorCode.ConnectMultipleDocks;
                            _notificationStr = $"{LangHelper.Instance["Multiple_docks_are_detected"]}";
                            _logs.DebugMsg_1($"{nameof(CheckDeviceStatus_IsStopUpdate)} LogicalDock: Multiple docks are detected. Keep only one dock connected to prevent damage to your docks");
                            ret = true;
                        }
                    }
                }
                else if (currentFWInfo.DeviceType == DeviceType.PhysicalDongle && currentFWInfo.DeviceName.ToLower().Equals(GlobalDefinitions.Dongle_BeforeGen2_Name.ToLower()))
                {
                    //List<DeviceInfo> dongle_deviceInfos = _DeviceInfos.FindAll(o => o.PhysicalDeviceType.Equals(DeviceType.PhysicalAudioDongle) ||
                    //o.PhysicalDeviceType.Equals(DeviceType.PhysicalDongle));
                    //_logs.DebugMsg_1($"{nameof(CheckDeviceStatus_IsStopUpdate)} dongle_deviceInfos is null : {(dongle_deviceInfos == null ? "Yes" : "No")}");
                    //if (dongle_deviceInfos != null)
                    {
                        _logs.DebugMsg_1($"{nameof(CheckDeviceStatus_IsStopUpdate)} ioDongleCount : {ioDongleCount}");
                        if (ioDongleCount >= 2)
                        {
                            fWUErrorCode = FWUErrorCode.ConnectMultipleSameModels;
                            _notificationStr = $"{LangHelper.Instance["Firmware_update_aborted_same_model_is_connected"]}";
                            _logs.DebugMsg_1($"{nameof(CheckDeviceStatus_IsStopUpdate)} currentFWInfo.Model: {currentFWInfo.Model}: Multiple devices of the same model are plugged in");
                            ret = true;
                        }
                    }
                }
                else
                {
                    DeviceInfo? deviceInfo = deviceInfos.Find(o => o.ID.ToString().Equals(currentFWInfo.DeviceId.Replace("{", "").Replace("}", "")));
                    _logs.DebugMsg_1($"{nameof(CheckDeviceStatus_IsStopUpdate)} deviceInfo is null : {(deviceInfo == null ? "Yes" : "No")}");
                    if (deviceInfo != null)
                    {
                        _logs.DebugMsg_1($"{nameof(CheckDeviceStatus_IsStopUpdate)} deviceInfos.IsBatteryLevelSupported : {deviceInfo.IsBatteryLevelSupported}");
                        if (deviceInfo.IsBatteryLevelSupported)
                        {
                            _logs.DebugMsg_1($"{nameof(CheckDeviceStatus_IsStopUpdate)} deviceInfos.BatteryStatus : {deviceInfo.BatteryStatus}");
                            _logs.DebugMsg_1($"{nameof(CheckDeviceStatus_IsStopUpdate)} deviceInfos.BatteryLevel : {deviceInfo.BatteryLevel}");

                            if (deviceInfo.BatteryLevel <= 20 && deviceInfo.BatteryLevel >= 0)
                            {
                                if (currentFWInfo.DeviceType == DeviceType.LogicalKeyboard || currentFWInfo.DeviceType == DeviceType.LogicalMouse)//Fix PIMS-344498
                                {
                                    fWUErrorCode = FWUErrorCode.DeviceBatteryTooLow;
                                    _notificationStr = $"{LangHelper.Instance["Firmware_update_unsuccessful"]}";
                                    ret = true;
                                }
                                else
                                {
                                    _logs.DebugMsg_1($"{nameof(CheckDeviceStatus_IsStopUpdate)} device battery <= 20% but device is no KB or MS so no need stop");
                                }
                            }
                            else if (deviceInfo.BatteryLevel < 0)
                            {
                                if (currentFWInfo.DeviceType == DeviceType.LogicalKeyboard || currentFWInfo.DeviceType == DeviceType.LogicalMouse)//Fix PIMS-344498
                                {
                                    fWUErrorCode = FWUErrorCode.DeviceIsEnterSleepMode;
                                    _notificationStr = $"{LangHelper.Instance["Firmware_update_unsuccessful"]}";
                                    ret = true;
                                }
                                else
                                {
                                    _logs.DebugMsg_1($"{nameof(CheckDeviceStatus_IsStopUpdate)} device battery < 0% but device is no KB or MS so no need stop");
                                }
                            }
                        }
                    }
                }
            }
            _logs.DebugMsg_1($"{nameof(CheckDeviceStatus_IsStopUpdate)} ret : {ret}");
            _logs.DebugMsg_1($"{nameof(CheckDeviceStatus_IsStopUpdate)} done");
            return ret;
        }

        /// <summary>
        /// Determine whether the computer battery power is less than 10% for Dock firmware update
        /// </summary>
        /// <param name="currentFWInfo">Firmware information currently to be updated</param>
        /// <returns>Computer power is less than 10% returns true; otherwise it returns false.</returns>
        public bool CheckPCBattery_IsStopUpdate(FWUpdateInfo currentFWInfo)
        {
            bool ret = false;
            try
            {
                _logs.DebugMsg_1($"{nameof(CheckPCBattery_IsStopUpdate)} start");
                _logs.DebugMsg_1($"{nameof(CheckPCBattery_IsStopUpdate)} currentFWInfo.DeviceType : {currentFWInfo.DeviceType}");
                if (currentFWInfo.DeviceType == DeviceType.LogicalDock ||
                    currentFWInfo.DeviceType == DeviceType.PhysicalWiredDock)
                {
                    using (BatteryInfo batteryInfo = new BatteryInfo())
                    {
                        batteryInfo.GetBatteryInfo(out var battery);
                        _logs.DebugMsg_1($"{nameof(CheckPCBattery_IsStopUpdate)} PC battery life percent ： {battery.BatteryLifePercent}");
                        if (battery.BatteryLifePercent <= 10)
                        {
                            _notificationStr = $"{currentFWInfo.DeviceName} {currentFWInfo.Model} {LangHelper.Instance["Firmware_update_unsuccessful"]}";
                            _logs.DebugMsg_1($"{nameof(CheckPCBattery_IsStopUpdate)} {_fWUpdateInfo.DeviceName} {_fWUpdateInfo.Model} update download cancel, because PC battery too low.");
                            ret = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logs.DebugMsg_1($"{nameof(CheckPCBattery_IsStopUpdate)} error : {ex.Message}");
            }
            _logs.DebugMsg_1($"{nameof(CheckPCBattery_IsStopUpdate)} done");
            return ret;
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
        private void NotificationFWupdate(string title, string info, bool stayOpen = false, bool isNeedButton = false)
        {
            try
            {
                info = info.Replace("[XXXXXX]", $"{_fWUpdateInfo.DeviceName} {_fWUpdateInfo.Model}");
                _logs.DebugMsg_1($"{nameof(NotificationFWupdate)} title : {title}, info : {info}");
                _logs.DebugMsg_1($"{nameof(NotificationFWupdate)} _IsUITrigger : {_IsUITrigger}");
                _logs.DebugMsg_1($"{nameof(NotificationFWupdate)} _IsShowNotify : {_IsShowNotify}");
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
                            IsNeedButton = isNeedButton,
                            Object = _fWUpdateInfoPackage,
                            PopupType = PopupContentPackage_Enum.FWU
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
            catch (Exception ex)
            {
                _logs.DebugMsg_1($"{nameof(NotificationFWupdate)} error : {ex.Message}");
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
        private FWUErrorCode Install(FWUpdateInfo fwUpdateInfo)
        {
            try
            {
                _logs.DebugMsg_1($"{nameof(Install)} fwUpdateInfo.DeviceName : {fwUpdateInfo.DeviceName}  start");
                CertificateCheck certificateCheck = new CertificateCheck(_logs);
                if (!fwUpdateInfo.IsDisplay)
                {
                    string FileCAInfo = string.Empty;
                    string FileInfo;
                    if (!DDPMFileSecurity.IsFilePathValid(fwUpdateInfo.InstallPaths, out FileInfo))//0815 Bruce Add Security
                    {
                        _notificationStr = LangHelper.Instance["Firmware_update_unsuccessful"];
                        _logs.DebugMsg_1(fwUpdateInfo.DeviceName + " FileIsNoSafe:" + FileInfo);
                        return FWUErrorCode.FileIsNoSafe;
                    }
                }
                else
                {
                    string FileInfo;
                    if (!DDPMFileSecurity.IsFilePathValid(fwUpdateInfo.InstallPaths, out FileInfo))//0815 Bruce Add Security
                    {
                        _notificationStr = LangHelper.Instance["Firmware_update_unsuccessful"];
                        _logs.DebugMsg_1(fwUpdateInfo.DeviceName + " FileIsNoSafe:" + FileInfo);
                        return FWUErrorCode.FileIsNoSafe;
                    }
                }
                string arguments;
                //string AppDataPath = WTSFunction.GetActiveUserLocalAppDataPath(Log);
                string programData = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
                string logPath = "";
                _ProgressLogPath = string.Empty;
                _logs.DebugMsg_1(fwUpdateInfo.DeviceName + " create log path start");
                if (!string.IsNullOrEmpty(programData))
                {
                    string path;
                    try
                    {
                        _logs.DebugMsg_1($"fwUpdateInfo.ServiceTag is : {fwUpdateInfo.ServiceTag}");
                        //path = @$"{programData}\Dell\FWUpdateLog\{fwUpdateInfo.DeviceName}_{fwUpdateInfo.ServiceTag}_{DateTime.Now.ToString("yy-MM-dd_HH_mm_ss")}";
                        path = @$"{programData}{GlobalDefinitions.LogFwUpdater}\{fwUpdateInfo.DeviceName}_{fwUpdateInfo.Model}_{fwUpdateInfo.ServiceTag}_{DateTime.Now.ToString("yy-MM-dd_HH_mm_ss")}";
                        if (!Directory.Exists(path))
                        {
                            Directory.CreateDirectory(path);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logs.DebugMsg_1($"{fwUpdateInfo.DeviceName} {fwUpdateInfo.Model} create log Error : {ex.Message}");
                        path = @$"{programData}{GlobalDefinitions.LogFwUpdater}\ex_{DateTime.Now.ToString("yy-MM-dd_HH_mm_ss")}"; //move to %programdata%\Dell\Dell Display and Peripheral Manager\
                        if (!Directory.Exists(path))
                        {
                            Directory.CreateDirectory(path);
                        }
                    }
                    DDPMFileSecurity.SetFolderPermissions_UserReadAndExecute(path, out string errorMsg);
                    _logs.DebugMsg_1($"DDPMFileSecurity.SetFolderPermissions_UserReadAndExecute errorMsg : {errorMsg}");
                    if (!DDPMFileSecurity.ValidateFilePath(@$"{programData}{GlobalDefinitions.LogFwUpdater}", out string info))
                    {
                        _notificationStr = LangHelper.Instance["Firmware_update_unsuccessful"];
                        _logs.DebugMsg_1($"{fwUpdateInfo.DeviceName}[FWUpdateLog] log path Error : {info}");
                        return FWUErrorCode.FolderIsNotSafe;
                    }
                    /*if (!DDPMFileSecurity.CheckFolderACL(@$"{programData}{GlobalDefinitions.LogFwUpdater}", out info, true))
                    {
                        _logs.DebugMsg_1($"{fwUpdateInfo.DeviceName}[FWUpdateLog] log path ACL Error : {info}");
                        return FWUErrorCode.FolderIsNotSafe;
                    }*/
                    if (!string.IsNullOrEmpty(path))
                    {
                        logPath = path;
                        _ProgressLogPath = $"{logPath}\\PrgoressResult";
                        _ILogs = new Log("ISP", new LogFile(_ProgressLogPath), "ISP");
                        _logs.DebugMsg_1(fwUpdateInfo.DeviceName + " create log path done");
                    }
                    else
                    {
                        _logs.DebugMsg_1(fwUpdateInfo.DeviceName + " create log path done but path is null or empty");
                    }
                }
                if (fwUpdateInfo.IsUOD)
                {
                    NotificationFWupdate(LangHelper.Instance["Dock_FW_info"], $"{fwUpdateInfo.DeviceName} {fwUpdateInfo.Model} {LangHelper.Instance["Dock_FW_is_being_loaded"]}");
                }
                else
                {
                    string s = LangHelper.Instance["Update_in_progress_body"].Replace("[XXXXXX]", $"{_fWUpdateInfo.DeviceName} ({_fWUpdateInfo.Model})");
                    NotificationFWupdate(LangHelper.Instance["Update_in_progress"], s, false, false);
                }
                if (fwUpdateInfo.IsDisplay && IsISPInApp(fwUpdateInfo.InstallPaths, out string upgPath))//新版螢幕韌體更新
                {
                    using (FileLock fileLock = new FileLock(fwUpdateInfo.InstallPaths, PathCheckOption.None, lockNow: true))
                    {
                        _logs.DebugMsg_1($"upgPath : {upgPath}");
                        Action<Result, int, int> callback = (result, expectedTime, progress) =>
                        {
                            foreach (string msg in result.ISPMessage)
                            {
                                if (!string.IsNullOrEmpty(msg))
                                {
                                    WriteLog($"DeviceName : {fwUpdateInfo.DeviceName} Model : {fwUpdateInfo.Model} to ver : {fwUpdateInfo.TheLatestVersion} ISP msg : {msg}");
                                }
                            }
                            if (result.ErrorCode >= 0)
                            {
                                WriteLog($"DeviceName : {fwUpdateInfo.DeviceName} Model : {fwUpdateInfo.Model} to ver : {fwUpdateInfo.TheLatestVersion} ErrorCode: {result.ErrorCode}, WriteProtection: {result.WriteProtection}");
                                CheckDisplayErrorCode(result.ErrorCode);
                            }
                            UpdateProgressInfo updateProgressInfo = new UpdateProgressInfo()
                            {
                                DeviceName = _fWUpdateInfo.DeviceName,
                                Model = _fWUpdateInfo.Model,
                                IsDisplay = _fWUpdateInfo.IsDisplay,
                                UpdateTime = _fWUpdateInfo.UpdateTime,
                                TheLatestVersion = _fWUpdateInfo.TheLatestVersion,
                                ProcessName = "Installing",
                                ProcessProgress = progress,
                            };
                            sendMessageToEvent(updateProgressInfo);
                        };

                        UpdateProgressInfo updateProgressInfo = new UpdateProgressInfo()
                        {
                            DeviceName = _fWUpdateInfo.DeviceName,
                            Model = _fWUpdateInfo.Model,
                            IsDisplay = _fWUpdateInfo.IsDisplay,
                            UpdateTime = _fWUpdateInfo.UpdateTime,
                            TheLatestVersion = _fWUpdateInfo.TheLatestVersion,
                            ProcessName = "Installing",
                            ProcessProgress = 0,
                        };
                        sendMessageToEvent(updateProgressInfo);
                        WriteLog($"DeviceName : {fwUpdateInfo.DeviceName} Model : {fwUpdateInfo.Model} to ver : {fwUpdateInfo.TheLatestVersion} ===========[START]==========");

                        try
                        {
                            string workingDirectory = Path.GetDirectoryName(fwUpdateInfo.InstallPaths);
                            WriteLog($"workingDirectory : {workingDirectory}");
                            string dllPath = $@"{workingDirectory}\FirmwareUpdaterDll.dll";
                            string dllPathInfo = "";
                            if (DDPMFileSecurity.ValidateFilePath(dllPath, out dllPathInfo))
                            {
                                if (!DDPMFileSecurity.VerifyExecutableFileSignature(dllPath, out dllPathInfo))
                                {
                                    bool result = runISP(dllPath, fwUpdateInfo.Model, fwUpdateInfo.ServiceTag,
                                    upgPath, true, callback);
                                }
                                else
                                {
                                    _notificationStr = LangHelper.Instance["Firmware_update_unsuccessful"];
                                    _logs.DebugMsg_1($"DeviceName : {fwUpdateInfo.DeviceName} Model : {fwUpdateInfo.Model} DDPMFileSecurity.VerifyExecutableFileSignature fail. Info : {dllPathInfo}");
                                    WriteLog($"DeviceName : {fwUpdateInfo.DeviceName} Model : {fwUpdateInfo.Model} DDPMFileSecurity.VerifyExecutableFileSignature fail. Info : {dllPathInfo}");
                                    return FWUErrorCode.FileIsNoSafe;
                                }
                            }
                            else
                            {
                                _notificationStr = LangHelper.Instance["Firmware_update_unsuccessful"];
                                _logs.DebugMsg_1($"DeviceName : {fwUpdateInfo.DeviceName} Model : {fwUpdateInfo.Model} DDPMFileSecurity.ValidateFilePath fail. Info : {dllPathInfo}");
                                WriteLog($"DeviceName : {fwUpdateInfo.DeviceName} Model : {fwUpdateInfo.Model} DDPMFileSecurity.ValidateFilePath fail. Info : {dllPathInfo}");
                                return FWUErrorCode.FolderIsNotSafe;
                            }
                        }
                        finally
                        {
                            WriteLog($"DeviceName : {fwUpdateInfo.DeviceName} Model : {fwUpdateInfo.Model} to ver : {fwUpdateInfo.TheLatestVersion}============[END]===========");
                        }
                    }
                }
                else//周邊裝置和舊版螢幕韌體更新
                {
                    // 生成唯一的管道名稱
                    string _namedPipeName = Guid.NewGuid().ToString("D");
                    if (!fwUpdateInfo.IsDisplay)
                    {
                        _CurrentProcess = 0;
                        if (_fWUpdateInfo.DeviceType == DeviceType.LogicalKeyboard ||
                            _fWUpdateInfo.DeviceType == DeviceType.LogicalMouse)
                        {
                            _fwTimeOutCount = 60;
                        }
                        else
                        {
                            _fwTimeOutCount = 120;
                        }
                        _timeOutCount = _fwTimeOutCount;
                        _timerTimeOut = new Timer();
                        _timerTimeOut.Interval = TimeSpan.FromSeconds(1).TotalMilliseconds;
                        _timerTimeOut.Elapsed += _timerTimeOut_Tick;
                        _namedPipeServer = new NamedPipeStreamServer(_namedPipeName, fwUpdateInfo.Thumbprint, _IsSkipSHA, _logs); // 創建命名管道伺服器
                        _namedPipeServer.MessageReceived += _namedPipeServer_MessageReceived;
                        _namedPipeServer.ClientConnectedEvent += _namedPipeServer_ClientConnectedEvent;
                        _namedPipeServer.ClientDisconnectedEvent += _namedPipeServer_ClientDisconnectedEvent;
                        _logs.DebugMsg_1(fwUpdateInfo.DeviceName + nameof(_namedPipeServer) + " ready");

                    }
                    arguments = BuildArgs(fwUpdateInfo, _namedPipeName, logPath);
                    _logs.DebugMsg_1($"arguments : ***");
                    if (_timerTimeOut != null)
                    {
                        _timerTimeOut.Enabled = true;
                    }
                    var sessionId = Kernel32.WTSGetActiveConsoleSessionId();
                    if (sessionId is Advapi32.InvalidSessionId) throw new InvalidOperationException($"Cannot get session id");
                    IntPtr token = UserImpersonator.GetTokenFromSession(sessionId, systemUser: false);

                    int exitCode = 1;
                    using (FileLock fileLock = new FileLock(fwUpdateInfo.InstallPaths, PathCheckOption.None, lockNow: true))
                    {
                        AclChecker aclChecker = new AclChecker();
                        if (aclChecker.ContainsUnprivilegedWriteAccess(fileLock))
                        {
                            throw new SecurityException($"File ACLs for {fwUpdateInfo.InstallPaths} contained unprivileged write access for one or more identity");
                        }
                        //WTSFunction.RunElevatedProcess(fwUpdateInfo.InstallPaths, arguments);
                        if (fwUpdateInfo.IsDisplay)
                        {
                            _DisplayProgress = 0;
                            UpdateProgressInfo updateProgressInfo = new UpdateProgressInfo()
                            {
                                DeviceName = _fWUpdateInfo.DeviceName,
                                Model = _fWUpdateInfo.Model,
                                IsDisplay = _fWUpdateInfo.IsDisplay,
                                UpdateTime = _fWUpdateInfo.UpdateTime,
                                TheLatestVersion = _fWUpdateInfo.TheLatestVersion,
                                ProcessName = "Installing",
                                ProcessProgress = _DisplayProgress,
                            };
                            sendMessageToEvent(updateProgressInfo);
                            int needTime_Minutes = 30;
                            if (!string.IsNullOrEmpty(_fWUpdateInfo.UpdateTime))
                            {
                                _logs.DebugMsg_1($"DeviceName : {fwUpdateInfo.DeviceName} Model : {fwUpdateInfo.Model} _fWUpdateInfo.UpdateTime : {_fWUpdateInfo.UpdateTime}");
                                if (!int.TryParse(_fWUpdateInfo.UpdateTime, out needTime_Minutes))
                                {
                                    needTime_Minutes = 30;
                                }
                            }
                            _logs.DebugMsg_1($"DeviceName : {fwUpdateInfo.DeviceName} Model : {fwUpdateInfo.Model} needTime : {needTime_Minutes}");
                            int totalSeconds = needTime_Minutes * 60; // 總秒數
                            int intervalMilliseconds = totalSeconds / 100;
                            _logs.DebugMsg_1($"DeviceName : {fwUpdateInfo.DeviceName} Model : {fwUpdateInfo.Model} totalSeconds : {totalSeconds}");
                            _logs.DebugMsg_1($"DeviceName : {fwUpdateInfo.DeviceName} Model : {fwUpdateInfo.Model} intervalMilliseconds : {intervalMilliseconds}");
                            _DisplayProgressTimer = new Timer(); // 設置計時器，每 interval 秒觸發一次
                            _DisplayProgressTimer.Interval = TimeSpan.FromSeconds(intervalMilliseconds).TotalMilliseconds;
                            _DisplayProgressTimer.Elapsed += DisplayProgress_Tick;
                            _DisplayProgressTimer.Start();
                        }
                        _logs.DebugMsg_1(fwUpdateInfo.DeviceName + " StartProcessAndBypassUACWithAdmin go");
                        PInvoke.PROCESS_INFORMATION procInfo;
                        string arguments_Final = fwUpdateInfo.InstallPaths + " " + arguments;
                        _logs.DebugMsg_1($"arguments_Final : {arguments_Final}");
                        string workingDirectory = DDPMFileSecurity.SanitizePath(Path.GetDirectoryName(fwUpdateInfo.InstallPaths), out string info);
                        if (!string.IsNullOrEmpty(workingDirectory))
                        {
                            _logs.DebugMsg_1($"workingDirectory is not null");
                            if (DDPMFileSecurity.ValidateFilePath(workingDirectory, out info))
                            {
                                _updateErrorCode = FWUErrorCode.Service_not_running_Try_again;

                                bool b = WTSFunction.StartProcessAndBypassUACWithAdmin(arguments_Final, workingDirectory, out procInfo);
                                string processName = Path.GetFileNameWithoutExtension(fwUpdateInfo.InstallPaths);
                                _logs.DebugMsg_1($"{nameof(Install)} {fwUpdateInfo.DeviceName} Searching for process: {processName}");
                                Process[] processes = Process.GetProcessesByName(processName);
                                if (processes != null && processes.Length > 0)
                                {
                                    _logs.DebugMsg_1($"{nameof(Install)} {fwUpdateInfo.DeviceName} {processName}.Length: {processes.Length}");
                                    _clientProcess = processes[0];

                                    {
                                        _clientProcess.EnableRaisingEvents = true;
                                        _clientProcess.Exited += (sender, e) =>
                                        {
                                            Process p = (Process)sender;
                                            if (fwUpdateInfo.IsDisplay && p != null)
                                            {
                                                _logs.DebugMsg_1($"{processName} (Process)sender.ExitCode go");
                                                exitCode = p.ExitCode;
                                                _logs.DebugMsg_1($"{processName} (Process)sender.ExitCode done");
                                            }
                                        };
                                    }
                                    _clientProcess.WaitForExit();
                                    if (_fWUpdateInfo.DeviceType == DeviceType.LogicalHeadset &&
                                        _fWUpdateInfo.Model.Contains("7024") &&
                                        _CurrentProcess >= 100)
                                    {
                                        _updateErrorCode = FWUErrorCode.NoError;
                                        _notificationStr = LangHelper.Instance["A2_Firmware_update_successful"];
                                        _logs.DebugMsg_1("process is done and is WL7024FWU and _CurrentProcess is 100% so successful");
                                        UpdateProgressInfo updateProgressInfo = new UpdateProgressInfo()
                                        {
                                            DeviceName = _fWUpdateInfo.DeviceName,
                                            Model = _fWUpdateInfo.Model,
                                            IsDisplay = _fWUpdateInfo.IsDisplay,
                                            UpdateTime = _fWUpdateInfo.UpdateTime,
                                            TheLatestVersion = _fWUpdateInfo.TheLatestVersion,
                                            ProcessName = "A2 Firmware update successful",
                                        };
                                        sendMessageToEvent(updateProgressInfo);
                                    }
                                    _logs.DebugMsg_1($"{processName} process is done.");
                                }
                                else
                                {
                                    _logs.DebugMsg_1($"{processName} process not found.");
                                    resetState();
                                    _updateErrorCode = FWUErrorCode.Service_not_running_Try_again;
                                    _logs.DebugMsg_1($"{fwUpdateInfo.DeviceName} {nameof(Install)} {LangHelper.Instance["Service_not_running_Try_again"]}");
                                    _notificationStr = LangHelper.Instance["Service_not_running_Try_again"];
                                    return _updateErrorCode;
                                }
                                _logs.DebugMsg_1(fwUpdateInfo.DeviceName + " StartProcessAndBypassUACWithAdmin done b : " + b);
                            }
                            else
                            {
                                _logs.DebugMsg_1($"{nameof(DownloadAndInstall)} {fwUpdateInfo.DeviceName} FilePathIsNotSafe - result : {info}");
                                _notificationStr = LangHelper.Instance["Firmware_update_unsuccessful"];
                                NotificationFWupdate(LangHelper.Instance["Error"], _notificationStr);
                                _updateErrorCode = FWUErrorCode.FileIsNoSafe;
                                return _updateErrorCode;
                            }
                        }
                        else
                        {
                            _logs.DebugMsg_1($"{nameof(Install)} workingDirectory is null, info : {info}");
                        }
                        //UserImpersonator.RunAsUser(token, () =>
                        //{
                        //    using (_clientProcess = new Process())
                        //    {
                        //        _clientProcess.StartInfo.UseShellExecute = false;
                        //        _clientProcess.StartInfo.FileName = fwUpdateInfo.InstallPaths;
                        //        _clientProcess.StartInfo.WorkingDirectory = Path.GetDirectoryName(_clientProcess.StartInfo.FileName);
                        //        _clientProcess.StartInfo.Arguments = arguments;
                        //        _clientProcess.Start();
                        //        _clientProcess.WaitForExit();
                        //        if (fwUpdateInfo.IsDisplay && _clientProcess != null)
                        //        {
                        //            exitCode = _clientProcess.ExitCode;
                        //        }
                        //    }
                        //});

                        if (_updateErrorCode == FWUErrorCode.Unknow)
                        {
                            _notificationStr = LangHelper.Instance["Firmware_update_unsuccessful"];
                        }
                        else if (_updateErrorCode == FWUErrorCode.Service_not_running_Try_again)
                        {
                            _notificationStr = LangHelper.Instance["Service_not_running_Try_again"];
                        }
                    }
                    _logs.DebugMsg_1($"{DateTime.Now}--DeviceName : {fwUpdateInfo.DeviceName} Model : {fwUpdateInfo.Model} to ver : {fwUpdateInfo.TheLatestVersion} exitCode : {exitCode}");
                    if (fwUpdateInfo.IsDisplay)
                    {
                        _logs.DebugMsg_1($"{_fWUpdateInfo.DeviceName} {fwUpdateInfo.Model} exitCode : {exitCode}");
                        if (exitCode == 0)
                        {
                            if (_DisplayProgressTimer != null)
                            {
                                _logs.DebugMsg_1($"{_fWUpdateInfo.DeviceName} {fwUpdateInfo.Model} _DisplayProgressTimer is NOT null");
                                _DisplayProgressTimer.Interval = TimeSpan.FromSeconds(0.5).TotalMilliseconds;
                                do
                                {
                                    int temp = _DisplayProgress;
                                    Task.Delay(TimeSpan.FromSeconds(1)).Wait();
                                    if (_DisplayProgress == temp)
                                    {
                                        break;
                                    }
                                } while (_DisplayProgress < 100);
                                _logs.DebugMsg_1($"{_fWUpdateInfo.DeviceName} {fwUpdateInfo.Model} do while done");
                            }
                        }
                        _logs.DebugMsg_1($"{_fWUpdateInfo.DeviceName} {fwUpdateInfo.Model} CheckDisplayErrorCode go");
                        CheckDisplayErrorCode(exitCode);
                        _logs.DebugMsg_1($"{_fWUpdateInfo.DeviceName} {fwUpdateInfo.Model} CheckDisplayErrorCode done");
                    }
                    else
                    {
                        if (_namedPipeServer != null && _namedPipeServer.IsNamedPipeServerIsNoSafe)
                        {
                            if (!_IsSkipSHA)
                            {
                                _updateErrorCode = FWUErrorCode.NamedPipeServerIsNoSafe;
                                _notificationStr = LangHelper.Instance["Firmware_update_unsuccessful"];
                                _logs.DebugMsg_1(fwUpdateInfo.DeviceName + " Named Pipe Server Is No Safe.");
                            }
                            else
                            {
                                _logs.DebugMsg_1(fwUpdateInfo.DeviceName + " Named Pipe Server Is No Safe. But skip");
                            }
                        }
                    }
                    if (fwUpdateInfo.IsUOD)
                    {
                        string ret = "";
                        string ret_2 = "";
                        if (_updateErrorCode == FWUErrorCode.NoError)
                        {
                            ret = LangHelper.Instance["Dock_FW_is_loaded_successful"];
                            ret_2 = "Dock FW is loaded successful";
                            _updateErrorCode = FWUErrorCode.NoError;
                            fwUpdateInfo.PNPDeviceID = GetDevicePNPDeviceID(fwUpdateInfo.Model);
                            DokcUODUpdateInfoPackage dokcUODUpdateInfoPackage = new DokcUODUpdateInfoPackage();
                            dokcUODUpdateInfoPackage.FWUpdateInfo = fwUpdateInfo;
                            CheckUODFWUInfo(dokcUODUpdateInfoPackage, null);
                        }
                        else
                        {
                            ret = LangHelper.Instance["Dock_FW_loaded_failed"];
                            ret_2 = "Dock FW loaded failed";
                            _updateErrorCode = FWUErrorCode.FirmwareUpdateFailed;
                        }
                        UpdateProgressInfo updateProgressInfo = new UpdateProgressInfo()
                        {
                            DeviceName = _fWUpdateInfo.DeviceName,
                            Model = _fWUpdateInfo.Model,
                            IsDisplay = _fWUpdateInfo.IsDisplay,
                            UpdateTime = _fWUpdateInfo.UpdateTime,
                            TheLatestVersion = _fWUpdateInfo.TheLatestVersion,
                            ProcessName = ret_2
                        };
                        _notificationStr = ret;
                        sendMessageToEvent(updateProgressInfo);
                    }
                }

                resetState();
                _logs.DebugMsg_1($"{nameof(Install)} DeviceName : {fwUpdateInfo.DeviceName}, Model : {fwUpdateInfo.Model} _updateErrorCode : {_updateErrorCode}");
                _logs.DebugMsg_1($"{nameof(Install)} {fwUpdateInfo.DeviceName} _notificationStr : {_notificationStr}");
                _logs.DebugMsg_1($"{nameof(Install)} done");
                WriteLog($"DeviceName : {fwUpdateInfo.DeviceName} Model : {fwUpdateInfo.Model} to ver : {fwUpdateInfo.TheLatestVersion} Result : {_updateErrorCode}");
                return _updateErrorCode;
            }
            catch (Exception ex)
            {
                _updateErrorCode = FWUErrorCode.Unknow;
                _logs.DebugMsg_1(fwUpdateInfo.DeviceName + nameof(Install) + " Error:" + ex.ToString());
                _notificationStr = LangHelper.Instance["Service_not_running_Try_again"];
                return _updateErrorCode;
            }
            finally
            {
                resetState();
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
                foreach (ManagementObject m in queryCollection.Cast<ManagementObject>())
                {
                    if (IsDisposed)
                    {
                        _logs.DebugMsg_1($"GetDevicePNPDeviceID IsDisposed");
                        break;
                    }
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

        private void _timerTimeOut_Tick(object? sender, EventArgs e)
        {
            if (_timeOutCount < _fwTimeOutCount)
            {
                UpdateProgressInfo fWUpdateInfo = new UpdateProgressInfo()
                {
                    DeviceName = _fWUpdateInfo.DeviceName,
                    Model = _fWUpdateInfo.Model,
                    IsDisplay = _fWUpdateInfo.IsDisplay,
                    UpdateTime = _fWUpdateInfo.UpdateTime,
                    TheLatestVersion = _fWUpdateInfo.TheLatestVersion,
                    ProcessName = "Timeout",
                    ProcessProgress = _timeOutCount,
                };
                sendMessageToEvent(fWUpdateInfo);
            }
            if (_timeOutCount <= 60 &&
                _fWUpdateInfo.DeviceType == DeviceType.LogicalHeadset &&
                _fWUpdateInfo.Model.Contains("7024") &&
                _CurrentProcess >= 100)
            {
                _updateErrorCode = FWUErrorCode.NoError;
                _notificationStr = LangHelper.Instance["A2_Firmware_update_successful"];
                _logs.DebugMsg_1("_timeOutCount <= 60 and is WL7024FWU and _CurrentProcess is 100% so successful");
                UpdateProgressInfo updateProgressInfo = new UpdateProgressInfo()
                {
                    DeviceName = _fWUpdateInfo.DeviceName,
                    Model = _fWUpdateInfo.Model,
                    IsDisplay = _fWUpdateInfo.IsDisplay,
                    UpdateTime = _fWUpdateInfo.UpdateTime,
                    TheLatestVersion = _fWUpdateInfo.TheLatestVersion,
                    ProcessName = "A2 Firmware update successful",
                };
                sendMessageToEvent(updateProgressInfo);
                resetState();
            }
            _timeOutCount--;
            if (_namedPipeServer != null && _namedPipeServer.IsNamedPipeServerIsNoSafe &&
                !_IsSkipSHA)
            {
                _updateErrorCode = FWUErrorCode.NamedPipeServerIsNoSafe;
                _notificationStr = LangHelper.Instance["Firmware_update_unsuccessful"];
                _logs.DebugMsg_1($"_timerTimeOut_Tick Named Pipe Server Is No Safe");
                resetState();
            }
            if (_timeOutCount == 0)
            {
                _updateErrorCode = FWUErrorCode.FirmwareUpdateTimeout;
                _notificationStr = LangHelper.Instance["Timeout_error"];
                _logs.DebugMsg_1($"_timeOutCount == 0 name pipe no response received within {_fwTimeOutCount} seconds so Firmware update timeout");
                resetState();
            }
        }
        private void DisplayProgress_Tick(object? sender, EventArgs e)
        {
            UpdateProgressInfo updateProgressInfo = new UpdateProgressInfo()
            {
                DeviceName = _fWUpdateInfo.DeviceName,
                Model = _fWUpdateInfo.Model,
                IsDisplay = _fWUpdateInfo.IsDisplay,
                UpdateTime = _fWUpdateInfo.UpdateTime,
                TheLatestVersion = _fWUpdateInfo.TheLatestVersion,
                ProcessName = "Installing",
            };
            if (_DisplayProgress >= 100)
            {
                updateProgressInfo.ProcessProgress = 100;
            }
            else
            {
                updateProgressInfo.ProcessProgress = ++_DisplayProgress;
            }
            sendMessageToEvent(updateProgressInfo);
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
            message = Regex.Replace(message, @"(<.*?>)", match => match.Value.ToUpper(new CultureInfo("en-US", false)));
            string messageWithRoot = "<Root>" + message;
            messageWithRoot += "</Root>";

            XmlDocument xmlDoc = new XmlDocument();
            try
            {
                xmlDoc.LoadXml(messageWithRoot);
            }
            catch (Exception ex)
            {
                _logs.DebugMsg_1($"xmlDoc.LoadXml Error : {ex.Message}");
                _logs.DebugMsg_1($"xmlDoc.LoadXml message : {message}");
            }
            XmlNode? msg1Node;//鍵盤滑鼠才會觸發
            XmlNode? progressNode;
            XmlNode? buttonCaptionNode;
            XmlNode? buttonStateNode;
            XmlNode? stateFlowNode;
            XmlNode? timeOut;//新的FW安裝包都有
            XmlNode? tPubKeyDev1;
            XmlNode? encBlock;

            //if (message.Contains("InvokeDisplay".ToUpper(new CultureInfo("en-US", false))))
            {
                msg1Node = xmlDoc.SelectSingleNode("Root/" + "InvokeDisplay/MSG1".ToUpper(new CultureInfo("en-US", false)));//鍵盤滑鼠才會觸發
                /*progressNode = xmlDoc.SelectSingleNode("Root/" + "InvokeDisplay/Progress".ToUpper(new CultureInfo("en-US", false)));
                buttonCaptionNode = xmlDoc.SelectSingleNode("Root/" + "InvokeDisplay/Button-Caption".ToUpper(new CultureInfo("en-US", false)));
                buttonStateNode = xmlDoc.SelectSingleNode("Root/" + "InvokeDisplay/Button-State".ToUpper(new CultureInfo("en-US", false)));*/
                if (msg1Node != null)
                {
                    if (msg1Node.InnerText == "M1")
                    {
                        if (_fWUpdateInfo.DeviceType == DeviceType.LogicalMouse)
                        {
                            UpdateProgressInfo updateProgressInfo = new UpdateProgressInfo()
                            {
                                DeviceName = _fWUpdateInfo.DeviceName,
                                Model = _fWUpdateInfo.Model,
                                IsDisplay = _fWUpdateInfo.IsDisplay,
                                UpdateTime = _fWUpdateInfo.UpdateTime,
                                TheLatestVersion = _fWUpdateInfo.TheLatestVersion,
                                ProcessName = "M1"
                            };
                            if (_IsUITrigger)
                            {
                                sendMessageToEvent(updateProgressInfo);
                            }
                            else//for CLI
                            {
                                string s = LangHelper.Instance["Update_in_progress_body"].Replace("[XXXXXX]", $"{_fWUpdateInfo.DeviceName} ({_fWUpdateInfo.Model})");
                                s += $"\r\n{LangHelper.Instance["M1_Please_double_click_mouse_left_button_to_start_firmware_update"]}";
                                NotificationFWupdate(LangHelper.Instance["Update_in_progress"], s, false, false);
                            }
                            //NotificationFWupdate(LangHelper.Instance["FW_info"], LangHelper.Instance["M1_Please_double_click_mouse_left_button_to_start_firmware_update"]);
                            _logs.DebugMsg_1("Get M1:Please double click mouse left button to start firmware update");
                        }
                        else
                        {
                            _logs.DebugMsg_1("Get M1: but device is not mouse");
                        }
                    }
                    else if (msg1Node.InnerText == "M2")
                    {
                        if (_fWUpdateInfo.DeviceType == DeviceType.LogicalKeyboard)
                        {
                            UpdateProgressInfo updateProgressInfo = new UpdateProgressInfo()
                            {
                                DeviceName = _fWUpdateInfo.DeviceName,
                                Model = _fWUpdateInfo.Model,
                                IsDisplay = _fWUpdateInfo.IsDisplay,
                                UpdateTime = _fWUpdateInfo.UpdateTime,
                                TheLatestVersion = _fWUpdateInfo.TheLatestVersion,
                                ProcessName = "M2"
                            };
                            if (_IsUITrigger)
                            {
                                sendMessageToEvent(updateProgressInfo);
                            }
                            else//for CLI
                            {
                                string s = LangHelper.Instance["Update_in_progress_body"].Replace("[XXXXXX]", $"{_fWUpdateInfo.DeviceName} ({_fWUpdateInfo.Model})");
                                s += $"\r\n{LangHelper.Instance["M2_Please_press_key_on_keyboard_to_start_firmware_update"]}";
                                NotificationFWupdate(LangHelper.Instance["Update_in_progress"], s, false, false);
                            }
                            //NotificationFWupdate(LangHelper.Instance["FW_info"], LangHelper.Instance["M2_Please_press_key_on_keyboard_to_start_firmware_update"]);
                            _logs.DebugMsg_1("Get M2:Please press \"U\" key on keyboard to start firmware update");
                        }
                        else
                        {
                            _logs.DebugMsg_1("Get M2: but device is not keyboard");
                        }
                    }
                    else if (msg1Node.InnerText == "M3")
                    {
                        _logs.DebugMsg_1("Get M3:Firmware Update Successfully completed");
                    }
                    else if (msg1Node.InnerText == "M0")
                    {
                        _logs.DebugMsg_1("Get M0:Display contents passed in with ");
                    }
                    else if (msg1Node.InnerText == "M4")
                    {
                        _logs.DebugMsg_1("Get M4:Do not power off and disturb device");
                    }
                    else if (msg1Node.InnerText == "M5")
                    {
                        _logs.DebugMsg_1("Get M5:Not detecting Target Device");
                    }
                    else if (msg1Node.InnerText == "M7")
                    {
                        _logs.DebugMsg_1("Get M7:Firmware Update of multiple dongles of same kind is not supported. Keep one Target RF dongle of same kind on computer only.");
                    }
                    else if (msg1Node.InnerText == "M6")
                    {
                        _logs.DebugMsg_1("Get M6:Firmware Update started, do not power off RF Dongle under Firmware Update");
                    }
                    else
                    {
                        _logs.DebugMsg_1("Should got M1 or M2 but got : " + msg1Node.InnerText + Environment.NewLine);
                    }
                }
                /*if (msg1Node == null && progressNode == null && buttonCaptionNode == null && buttonStateNode == null)
                {
                    _logs.DebugMsg_1("Can't heandle: " + message + Environment.NewLine);
                }*/
            }
            //else
            {
                progressNode = xmlDoc.SelectSingleNode("Root/" + "progress".ToUpper(new CultureInfo("en-US", false)));
                buttonCaptionNode = xmlDoc.SelectSingleNode("Root/" + "button-caption".ToUpper(new CultureInfo("en-US", false)));
                buttonStateNode = xmlDoc.SelectSingleNode("Root/" + "button-state".ToUpper(new CultureInfo("en-US", false)));
                stateFlowNode = xmlDoc.SelectSingleNode("Root/" + "stateflow".ToUpper(new CultureInfo("en-US", false)));
                timeOut = xmlDoc.SelectSingleNode("Root/" + "timeout".ToUpper(new CultureInfo("en-US", false)));
                tPubKeyDev1 = xmlDoc.SelectSingleNode("Root/" + "tpubkeydev1".ToUpper(new CultureInfo("en-US", false)));
                encBlock = xmlDoc.SelectSingleNode("Root/" + "encblock".ToUpper(new CultureInfo("en-US", false)));

                if (msg1Node == null && progressNode == null && buttonCaptionNode == null && buttonStateNode == null && stateFlowNode == null && timeOut == null && tPubKeyDev1 == null && encBlock == null)
                {
                    _logs.DebugMsg_1("Can't handle: " + message + Environment.NewLine);
                }
                if (stateFlowNode != null)
                {
                    if (stateFlowNode.InnerText == "A0")
                    {
                        _logs.DebugMsg_1("Get A0:Device connected");
                        UpdateProgressInfo updateProgressInfo = new UpdateProgressInfo()
                        {
                            DeviceName = _fWUpdateInfo.DeviceName,
                            Model = _fWUpdateInfo.Model,
                            IsDisplay = _fWUpdateInfo.IsDisplay,
                            UpdateTime = _fWUpdateInfo.UpdateTime,
                            TheLatestVersion = _fWUpdateInfo.TheLatestVersion,
                            ProcessName = "A0 Device connected",
                        };
                        sendMessageToEvent(updateProgressInfo);
                    }
                    else if (stateFlowNode.InnerText == "A1")
                    {
                        _logs.DebugMsg_1("Get A1:Firmware update started");
                        UpdateProgressInfo updateProgressInfo = new UpdateProgressInfo()
                        {
                            DeviceName = _fWUpdateInfo.DeviceName,
                            Model = _fWUpdateInfo.Model,
                            IsDisplay = _fWUpdateInfo.IsDisplay,
                            UpdateTime = _fWUpdateInfo.UpdateTime,
                            TheLatestVersion = _fWUpdateInfo.TheLatestVersion,
                            ProcessName = "A1 Firmware update started",
                        };
                        sendMessageToEvent(updateProgressInfo);
                    }
                    else if (stateFlowNode.InnerText == "A2")
                    {
                        _updateErrorCode = FWUErrorCode.NoError;
                        _notificationStr = LangHelper.Instance["A2_Firmware_update_successful"];
                        _logs.DebugMsg_1("Get A2:Firmware update successful");
                        UpdateProgressInfo updateProgressInfo = new UpdateProgressInfo()
                        {
                            DeviceName = _fWUpdateInfo.DeviceName,
                            Model = _fWUpdateInfo.Model,
                            IsDisplay = _fWUpdateInfo.IsDisplay,
                            UpdateTime = _fWUpdateInfo.UpdateTime,
                            TheLatestVersion = _fWUpdateInfo.TheLatestVersion,
                            ProcessName = "A2 Firmware update successful",
                        };
                        sendMessageToEvent(updateProgressInfo);
                        resetState();
                    }
                    else if (stateFlowNode.InnerText == "AF")
                    {
                        _logs.DebugMsg_1("Get AF:");
                        var errorCodeNode = xmlDoc.SelectSingleNode("Root/" + "ErrorCode".ToUpper(new CultureInfo("en-US", false)));
                        if (errorCodeNode != null)
                        {
                            if (errorCodeNode.InnerText == "E1")
                            {
                                _updateErrorCode = FWUErrorCode.FirmwareUpdateFailed;
                                _notificationStr = LangHelper.Instance["Firmware_update_unsuccessful"];
                                _logs.DebugMsg_1($"{_fWUpdateInfo.DeviceName} Get E1:Firmware update In progress. Fail to abort");
                            }
                            else if (errorCodeNode.InnerText == "E2")
                            {
                                _updateErrorCode = FWUErrorCode.FirmwareUpdateFailed;
                                _notificationStr = LangHelper.Instance["Firmware_update_unsuccessful"];
                                _logs.DebugMsg_1($"{_fWUpdateInfo.DeviceName} Get E2:Firmware update unsuccessful");
                            }
                            else if (errorCodeNode.InnerText == "E3")
                            {
                                _updateErrorCode = FWUErrorCode.FirmwareUpdateFailed;
                                _notificationStr = LangHelper.Instance["Firmware_update_unsuccessful"];
                                _logs.DebugMsg_1($"{_fWUpdateInfo.DeviceName} Get E3:User Abort Firmware Update");
                            }
                            else if (errorCodeNode.InnerText == "E4")
                            {
                                _updateErrorCode = FWUErrorCode.FirmwareUpdatNotSupportedForThisDevice;
                                _notificationStr = LangHelper.Instance["USB_wireless_receiver_firmware_is_unable_to_support_device_firmware_upgrade"];
                                _logs.DebugMsg_1($"{_fWUpdateInfo.DeviceName} Get E4:USB wireless receiver firmware is unable to support device firmware upgrade");
                            }
                            else if (errorCodeNode.InnerText == "E5")
                            {
                                _updateErrorCode = FWUErrorCode.FirmwareUpdateTimeout;
                                _notificationStr = LangHelper.Instance["Timeout_error"];
                                _logs.DebugMsg_1($"{_fWUpdateInfo.DeviceName} Get E5:Firmware update timeout");
                            }
                            else if (errorCodeNode.InnerText == "E6")
                            {
                                _updateErrorCode = FWUErrorCode.FirmwareUpdateTimeout;
                                _notificationStr = LangHelper.Instance["Timeout_error"];
                                _logs.DebugMsg_1($"{_fWUpdateInfo.DeviceName} Get E6:Firmware update timeout");
                            }
                            else if (errorCodeNode.InnerText == "E7")
                            {
                                _updateErrorCode = FWUErrorCode.FirmwareUpdateFailed;
                                _notificationStr = $"{LangHelper.Instance["Firmware_update_aborted_same_model_is_connected"]}";
                                _logs.DebugMsg_1($"{_fWUpdateInfo.DeviceName} Get E7:Firmware Update aborted. Found more than One(1) same device type exists on the same computer.");
                            }
                            else
                            {
                                _updateErrorCode = FWUErrorCode.Unknow;
                                _notificationStr = LangHelper.Instance["Firmware_update_unsuccessful"];
                                _logs.DebugMsg_1($"{_fWUpdateInfo.DeviceName} Get undefined error code, ErrorCode : " + errorCodeNode.InnerText);
                            }
                            UpdateProgressInfo updateProgressInfo = new UpdateProgressInfo()
                            {
                                DeviceName = _fWUpdateInfo.DeviceName,
                                Model = _fWUpdateInfo.Model,
                                IsDisplay = _fWUpdateInfo.IsDisplay,
                                UpdateTime = _fWUpdateInfo.UpdateTime,
                                TheLatestVersion = _fWUpdateInfo.TheLatestVersion,
                                ProcessName = "Error Code:" + errorCodeNode.InnerText,
                            };
                            sendMessageToEvent(updateProgressInfo);
                        }
                        else
                        {
                            _updateErrorCode = FWUErrorCode.Unknow;
                            _notificationStr = LangHelper.Instance["Firmware_update_unsuccessful"];
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
                        _notificationStr = LangHelper.Instance["Firmware_update_in_progress_Fail_to_abort"];
                        _logs.DebugMsg_1("Get 0xF000:Can not abort update at this time");
                    }
                    else if (stateFlowNode.InnerText == "0xF001")
                    {
                        //
                        // Abort success 
                        //
                        _updateErrorCode = FWUErrorCode.UserAborted;
                        _notificationStr = LangHelper.Instance["User_aborted_firmware_update"];
                        _logs.DebugMsg_1("Get 0xF001:User aborted firmware update");
                        resetState();
                    }
                    else if (stateFlowNode.InnerText == "U9")
                    {
                        _updateErrorCode = FWUErrorCode.NoError;
                        _notificationStr = LangHelper.Instance["A2_Firmware_update_successful"];
                        _logs.DebugMsg_1("Get U9:Firmware update successful");
                        UpdateProgressInfo updateProgressInfo = new UpdateProgressInfo()
                        {
                            DeviceName = _fWUpdateInfo.DeviceName,
                            Model = _fWUpdateInfo.Model,
                            IsDisplay = _fWUpdateInfo.IsDisplay,
                            UpdateTime = _fWUpdateInfo.UpdateTime,
                            TheLatestVersion = _fWUpdateInfo.TheLatestVersion,
                            ProcessName = "A2 Firmware update successful",
                        };
                        sendMessageToEvent(updateProgressInfo);
                        resetState();
                    }
                    else if (stateFlowNode.InnerText == "X0" || stateFlowNode.InnerText == "X2")
                    {
                        _logs.DebugMsg_1($"Get {stateFlowNode.InnerText}");
                    }
                    else
                    {
                        _logs.DebugMsg_1("Can't handle: " + message);
                    }
                }
                if (tPubKeyDev1 != null)
                {
                    _logs.DebugMsg_1($"{_fWUpdateInfo.DeviceName} string.IsNullOrEmpty(tPubKeyDev1.InnerText) : {string.IsNullOrEmpty(tPubKeyDev1.InnerText)}");
                    if (!string.IsNullOrEmpty(tPubKeyDev1.InnerText))
                    {
                        try
                        {
                            string s = $"<StateFlow>X1</StateFlow><TPubKeyPC1></TPubKeyPC1>";
                            _KeyGenerator = new KeyGenerator(Log);
                            string result_string = _KeyGenerator.ProcessX0State(tPubKeyDev1.InnerText);
                            if (!string.IsNullOrEmpty(result_string))
                            {
                                s = $"<StateFlow>X1</StateFlow><TPubKeyPC1>{result_string.ToLower(new CultureInfo("en-US", false)).Replace("-", "")}</TPubKeyPC1>";
                                _logs.DebugMsg_1("SendMessage : " + s);
                            }
                            if (_namedPipeServer != null)
                            {
                                _namedPipeServer.SendMessage(s);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logs.DebugMsg_1($"{_fWUpdateInfo.DeviceName} _KeyGenerator ProcessX0State error : {ex.Message}");
                        }

                    }
                }
                if (encBlock != null)
                {
                    _logs.DebugMsg_1($"{_fWUpdateInfo.DeviceName} string.IsNullOrEmpty(encBlock.InnerText) : {string.IsNullOrEmpty(encBlock.InnerText)}");
                    if (!string.IsNullOrEmpty(encBlock.InnerText))
                    {
                        try
                        {
                            if (_KeyGenerator != null)
                            {
                                string s = $"<StateFlow>X3</StateFlow><K2EncBlock></K2EncBlock><CCMTAG>4Bytes</CCMTag><version>01</version>";
                                string result_string = _KeyGenerator.ProcessX2State(encBlock.InnerText);
                                _KeyGenerator = null;
                                if (!string.IsNullOrEmpty(result_string))
                                {
                                    s = $"<StateFlow>X3</StateFlow><K2EncBlock>{result_string.ToLower(new CultureInfo("en-US", false)).Replace("-", "")}</K2EncBlock><CCMTAG>4Bytes</CCMTag><version>01</version>";
                                    _logs.DebugMsg_1("SendMessage : " + s);
                                }
                                if (_namedPipeServer != null)
                                {
                                    _namedPipeServer.SendMessage(s);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            _logs.DebugMsg_1($"{_fWUpdateInfo.DeviceName} _KeyGenerator ProcessX2State error : {ex.Message}");
                        }
                    }
                    else
                    {
                        if (_KeyGenerator != null)
                        {
                            _KeyGenerator = null;
                        }
                    }
                }
                if (progressNode != null)
                {
                    UpdateProgressInfo updateProgressInfo = new UpdateProgressInfo()
                    {
                        DeviceName = _fWUpdateInfo.DeviceName,
                        Model = _fWUpdateInfo.Model,
                        IsDisplay = _fWUpdateInfo.IsDisplay,
                        UpdateTime = _fWUpdateInfo.UpdateTime,
                        TheLatestVersion = _fWUpdateInfo.TheLatestVersion,
                        ProcessName = "Installing",
                        ProcessProgress = (_fWUpdateInfo.DeviceType == DeviceType.LogicalDock ||
                        _fWUpdateInfo.DeviceType == DeviceType.PhysicalWiredDock) ? 101 : int.Parse(progressNode.InnerText),//Fix PIMS-349453
                    };
                    if (_fWUpdateInfo.DeviceType == DeviceType.LogicalHeadset &&
                       _fWUpdateInfo.Model.Contains("7024"))
                    {
                        _CurrentProcess = updateProgressInfo.ProcessProgress;
                    }
                    sendMessageToEvent(updateProgressInfo);
                }
                if (timeOut != null)
                {
                    int.TryParse(timeOut.InnerText, out _fwTimeOutCount);
                    _logs.DebugMsg_1($"{_fWUpdateInfo.DeviceName} Get timeOut value : {_fwTimeOutCount}");
                    if (_fWUpdateInfo.DeviceType == DeviceType.LogicalKeyboard ||
                        _fWUpdateInfo.DeviceType == DeviceType.LogicalMouse)
                    {
                        if (_fwTimeOutCount < 60)
                        {
                            _fwTimeOutCount = 60;
                        }
                    }
                    else
                    {
                        if (_fwTimeOutCount < 120)
                        {
                            _fwTimeOutCount = 120;
                        }
                    }
                    UpdateProgressInfo updateProgressInfo = new UpdateProgressInfo()
                    {
                        DeviceName = _fWUpdateInfo.DeviceName,
                        Model = _fWUpdateInfo.Model,
                        IsDisplay = _fWUpdateInfo.IsDisplay,
                        UpdateTime = _fWUpdateInfo.UpdateTime,
                        TheLatestVersion = _fWUpdateInfo.TheLatestVersion,
                        ProcessName = "Timeout",
                        ProcessProgress = _fwTimeOutCount,
                    };
                    sendMessageToEvent(updateProgressInfo);
                }
            }
        }

        [SupportedOSPlatform("windows")]
        [SupportedOSPlatform("windows10.0.19041.0")]
        private void resetState([System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0)
        {
            _logs.DebugMsg_1($"{nameof(resetState)} start sourceLineNumber : {sourceLineNumber}");
            if (_timerTimeOut != null)
            {
                _logs.DebugMsg_1($"{nameof(resetState)} _timerTimeOut is no null");
                _timerTimeOut.Stop();
                _timerTimeOut.Enabled = false;
                _timerTimeOut.Elapsed -= _timerTimeOut_Tick;
                _timerTimeOut.Dispose();
                _logs.DebugMsg_1($"{nameof(resetState)} _timerTimeOut.Stop()");
                _timerTimeOut = null;
            }
            if (_downloadTimer != null)
            {
                _logs.DebugMsg_1($"{nameof(resetState)} _downloadTimer is no null");
                _downloadTimer.Stop();
                _downloadTimer.Enabled = false;
                _downloadTimer.Elapsed -= DownloadTimer_Elapsed;
                _downloadTimer.Dispose();
                _logs.DebugMsg_1($"{nameof(resetState)} _downloadTimer.Stop()");
                _downloadTimer = null;
            }
            if (_DisplayProgressTimer != null)
            {
                _logs.DebugMsg_1($"{nameof(resetState)} _DisplayProgressTimer is no null");
                _DisplayProgressTimer.Stop();
                _DisplayProgressTimer.Enabled = false;
                _DisplayProgressTimer.Elapsed -= DisplayProgress_Tick;
                _DisplayProgressTimer.Dispose();
                _logs.DebugMsg_1($"{nameof(resetState)} _DisplayProgressTimer.Stop()");
                _DisplayProgressTimer = null;
            }
            if (_namedPipeServer != null)
            {
                _logs.DebugMsg_1($"{nameof(resetState)} _namedPipeServer is no null");
                _logs.DebugMsg_1($"{nameof(resetState)} _namedPipeServer remove event go");
                _namedPipeServer.MessageReceived -= _namedPipeServer_MessageReceived;
                _namedPipeServer.ClientConnectedEvent -= _namedPipeServer_ClientConnectedEvent;
                _namedPipeServer.ClientDisconnectedEvent -= _namedPipeServer_ClientDisconnectedEvent;
                _namedPipeServer.Dispose();
                _namedPipeServer = null;
                _logs.DebugMsg_1($"{nameof(resetState)} _namedPipeServer remove event done");
            }
            if (OperatingSystem.IsWindowsVersionAtLeast(10, 0, 19041))
            {
                _logs.DebugMsg_1($"{nameof(resetState)} _clientProcess go");
                if (_clientProcess != null)
                {
                    _logs.DebugMsg_1($"{nameof(resetState)} _clientProcess is no null");
                    try
                    {
                        _clientProcess.Kill();
                        _clientProcess.Dispose();
                        _clientProcess = null;
                    }
                    catch (Exception ex)
                    {
                        _logs.DebugMsg_1($"{nameof(resetState)} _clientProcess Error : {ex.Message}");
                    }
                }
            }
        }

        private void sendMessageToEvent(UpdateProgressInfo fWUpdateInfo)
        {
            if (_IsShowNotify)
            {
                ProgressUpdate_Notify?.AsyncFireAndForget(this, fWUpdateInfo, System.Threading.CancellationToken.None);
            }
            _logs.DebugMsg_1($"sendMessageToEvent _IsShowNotify : {_IsShowNotify}, {fWUpdateInfo.DeviceName} {fWUpdateInfo.Model} {fWUpdateInfo.TheLatestVersion} {fWUpdateInfo.ProcessName} {fWUpdateInfo.ProcessProgress} {DateTime.Now}");
            WriteLog($"_IsShowNotify : {_IsShowNotify}, --DeviceName : {fWUpdateInfo.DeviceName} Model : {fWUpdateInfo.Model} to ver : {fWUpdateInfo.TheLatestVersion} ProcessName : {fWUpdateInfo.ProcessName}...{fWUpdateInfo.ProcessProgress}%");
        }
        private void WriteLog(string s)
        {
            try
            {
                if (_ILogs != null)
                {
                    _ILogs.Info($"{s}");
                }
            }
            catch (Exception ex)
            {
                _logs?.DebugMsg_1($"WriteLog ex: {ex.Message}");
            }
        }

        private bool CheckSHA(string filePath, out string fileCAInfo)
        {
            CertificateCheck certificateCheck = new CertificateCheck(_logs);
            bool isCheckSHA = false;
            fileCAInfo = "Error";
            //if (!string.IsNullOrEmpty(_fWUpdateInfo.SHA512))
            //{
            //    isCheckSHA = certificateCheck.CheckFile_SHA512(filePath, _fWUpdateInfo.SHA512, out fileCAInfo);
            //}
            //else
            {
                isCheckSHA = certificateCheck.CheckFile_SHA256(filePath, _fWUpdateInfo.SHA256, out fileCAInfo);
            }
            if (isCheckSHA)
            {
                _logs.DebugMsg_1($"{_fWUpdateInfo.DeviceName} CheckSHA pass");
            }
            else
            {
                if (!_IsSkipSHA)
                {
                    _logs.DebugMsg_1($"{_fWUpdateInfo.DeviceName} CheckSHA fail fileCAInfo : {fileCAInfo}");
                    _fWUpdateInfo.FWUErrorCode = FWUErrorCode.FileCheckFail;
                }
                else
                {
                    _logs.DebugMsg_1($"{_fWUpdateInfo.DeviceName} CheckSHA fail fileCAInfo : {fileCAInfo} BUT SKIP");
                    isCheckSHA = true;//Wait IL R14 force true
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
                _logs.DebugMsg_1($"{_fWUpdateInfo.DeviceName} CheckFile_Thumbprint pass");
            }
            else
            {
                if (!_IsSkipSHA)
                {
                    _logs.DebugMsg_1($"{_fWUpdateInfo.DeviceName} CheckFile_Thumbprint fail FileCAInfo : {FileCAInfo}");
                    _fWUpdateInfo.FWUErrorCode = FWUErrorCode.FileCheckFail;
                }
                else
                {
                    _logs.DebugMsg_1($"{_fWUpdateInfo.DeviceName} CheckFile_Thumbprint fail FileCAInfo : {FileCAInfo} BUT SKIP");
                    ishumbprint = true;//Wait IL R14 force true
                }
            }
            return ishumbprint;
        }
        private bool Unzip(string filePath, string extractPath, out string exeFilePath)
        {
            _logs.DebugMsg_1($"{_fWUpdateInfo.DeviceName} {nameof(Unzip)} Start");
            _fWUpdateInfo.FWUErrorCode = FWUErrorCode.Unknow;
            bool ret = false;
            Unzip unzip = new Unzip(_logs);
            exeFilePath = "";
            string FileCAInfo = "Pass";
            _logs.DebugMsg_1($"{_fWUpdateInfo.DeviceName} CheckSHA go.");
            if (CheckSHA(filePath, out FileCAInfo))
            {
                ret = true;
                if (unzip.CheckFileIsZip(filePath))
                {
                    _logs.DebugMsg_1($"{_fWUpdateInfo.DeviceName} File is zip.");
                    _logs.DebugMsg_1($"{_fWUpdateInfo.DeviceName} ExecuteUnzip go.");
                    if (unzip.ExecuteUnzip(filePath, extractPath, false, out exeFilePath))
                    {
                        if (string.IsNullOrEmpty(exeFilePath))
                        {
                            ret = false;
                            _logs.DebugMsg_1($"{_fWUpdateInfo.DeviceName} exeFilePath IsNullOrEmpty.");
                        }
                    }
                    else
                    {
                        _logs.DebugMsg_1($"{_fWUpdateInfo.DeviceName} Unzip Faile");
                    }
                }
                else
                {
                    _logs.DebugMsg_1($"{_fWUpdateInfo.DeviceName} File is exe.");
                    exeFilePath = filePath;
                }
            }
            _logs.DebugMsg_1($"{_fWUpdateInfo.DeviceName} {nameof(Unzip)} done");
            return ret;
        }

        private string GetODM(int index)
        {
            _logs.DebugMsg_1($"GetODM start");
            _logs.DebugMsg_1($"GetODM index : {index}");
            string ret = "Unknow";
            if (index - 1 < ODM.Length)
            {
                _logs.DebugMsg_1($"GetODM search go");
                ret = ODM[index - 1];
            }
            _logs.DebugMsg_1($"GetODM ret : {ret}");
            _logs.DebugMsg_1($"GetODM done");
            return ret;
        }
        private string GetConnected(DeviceType deviceType)
        {
            _logs.DebugMsg_1($"GetConnected start");
            _logs.DebugMsg_1($"GetConnected deviceType : {deviceType}");
            string ConnectionType = "Unknow";
            switch (deviceType)
            {
                case DeviceType.PhysicalWired:
                case DeviceType.PhysicalWebcam:
                case DeviceType.PhysicalWiredAudio:
                case DeviceType.PhysicalWiredDock:
                    ConnectionType = "Wired";
                    break;
                case DeviceType.PhysicalDongle:
                case DeviceType.PhysicalAudioDongle:
                    ConnectionType = "RF";
                    break;

                case DeviceType.PhysicalBluetooth:
                case DeviceType.PhysicalBluetoothAudio:
                case DeviceType.PhysicalPen:
                    ConnectionType = "Bluetooth";
                    break;
            }
            _logs.DebugMsg_1($"GetConnected ConnectionType : {ConnectionType}");
            _logs.DebugMsg_1($"GetConnected done");
            return ConnectionType;
        }
        string BuildArgs(FWUpdateInfo fwUpdateInfo, string namedPipeName, string logPath)
        {
            _logs.DebugMsg_1($"BuildArgs start");
            string arguments = "";
            if (!fwUpdateInfo.IsDisplay)
            {
                // 要運行的安裝程式路徑和命令行參數
                if (fwUpdateInfo.DeviceType != DeviceType.LogicalDock &&
                    fwUpdateInfo.DeviceType != DeviceType.PhysicalWiredDock)
                {
                    arguments = "/silent" + " /pipename:" + namedPipeName;
                    //deviceIndex commandLine
                    _logs.DebugMsg_1($"BuildArgs deviceIndex go");
                    arguments += $" /deviceIndex:" + fwUpdateInfo.DeviceIndex;
                    _logs.DebugMsg_1($"BuildArgs deviceIndex done");

                    //updatepath commandLine
                    _logs.DebugMsg_1($"BuildArgs updatepath go");
                    switch (fwUpdateInfo.Connectivity)
                    {
                        case "Wired":
                            arguments += $" /updatepath:Wired";
                            _logs.DebugMsg_1($"BuildArgs Add : /updatepath:Wired");
                            break;
                        case "RF":
                            arguments += $" /updatepath:RF";
                            _logs.DebugMsg_1($"BuildArgs Add : /updatepath:RF");
                            _logs.DebugMsg_1($"GetDeviceID go");
                            string deviceID = GetDeviceID(fwUpdateInfo);
                            _logs.DebugMsg_1($"GetDeviceID done");
                            if (!string.IsNullOrEmpty(deviceID))
                            {
                                _logs.DebugMsg_1($"GetDeviceID deviceID is not null");
                                _logs.DebugMsg_1($"BuildArgs Add :{deviceID}");
                                arguments += deviceID;
                            }
                            break;
                        case "Bluetooth":
                            arguments += $" /updatepath:BLE";
                            _logs.DebugMsg_1($"BuildArgs Add : /updatepath:BLE");
                            break;
                    }
                    _logs.DebugMsg_1($"BuildArgs updatepath done");

                    _logs.DebugMsg_1($"BuildArgs DeviceType go");
                    //DeviceType commandLine
                    switch (fwUpdateInfo.DeviceType)
                    {
                        case DeviceType.LogicalMouse:
                            arguments += $" /DeviceType:Mouse";
                            _logs.DebugMsg_1($"BuildArgs Add : /DeviceType:Mouse");
                            break;
                        case DeviceType.LogicalKeyboard:
                            arguments += $" /DeviceType:Keyboard";
                            _logs.DebugMsg_1($"BuildArgs Add : /DeviceType:Keyboard");
                            break;
                    }
                    _logs.DebugMsg_1($"BuildArgs DeviceType done");

                    //devicePath commandLine
                    _logs.DebugMsg_1($"BuildArgs devicePath go");
                    arguments += $" /devicePath:" + fwUpdateInfo.DevicePath;
                    _logs.DebugMsg_1($"BuildArgs devicePath done");
                }
                else
                {
                    _logs.DebugMsg_1($"BuildArgs Dock go");
                    arguments = (fwUpdateInfo.IsUOD ? "/uod " : "") + "/s" + " /pipename:" + namedPipeName;
                    if (_IsSkipSHA)
                    {
                        _logs.DebugMsg_1($"BuildArgs _IsSkipSHA is true so add /f");
                        arguments += $" /f";
                    }
                    if (!string.IsNullOrEmpty(logPath))
                    {
                        arguments += $" /debuglog /l=\"{logPath}\\{DateTime.Now.ToString("yyyy-MM-dd_HH_mm_ss")}\" /dp";
                        _logs.DebugMsg_1($"BuildArgs Add : /debuglog /l=\"{logPath}\\{DateTime.Now.ToString("yyyy-MM-dd_HH_mm_ss")}\" /dp");
                    }
                    _logs.DebugMsg_1($"BuildArgs Dock done");
                }
            }
            else
            {
                _logs.DebugMsg_1($"BuildArgs display go");
                arguments = $"-q --force -f";
                if (!string.IsNullOrEmpty(logPath))
                {
                    _logs.DebugMsg_1($"BuildArgs Log go");
                    arguments += $" \"{logPath}\"";
                    _logs.DebugMsg_1($"BuildArgs Add : \"{logPath}\"");
                    _logs.DebugMsg_1($"BuildArgs Log done");
                }
                _logs.DebugMsg_1($"BuildArgs display done");
            }
            _logs.DebugMsg_1($"BuildArgs done");
            return arguments;
        }
        /// <summary>
        /// Check whether the device to be updated supports OTA firmware updates
        /// </summary>
        /// <param name="fwUpdateInfo"></param>
        /// <returns></returns>
        public bool Check_CanBeOTAUpdate(FWUpdateInfo fwUpdateInfo)
        {
            _logs.DebugMsg_1($"Check_CanBeOTAUpdate start");
            bool ret = true;
            if ((fwUpdateInfo.DeviceType == DeviceType.LogicalWebcam ||
                fwUpdateInfo.DeviceType == DeviceType.PhysicalWebcam) &&
                fwUpdateInfo.Model.Contains("7022"))
            {
                _logs.DebugMsg_1($"Check_CanBeOTAUpdate DeviceType is WB7022");
                ret = false;
                //作業系統必須是Windows10 20H2 以上
                //或是Windows11 22H2以上
                if (WinVersion.GetVersion(out var info))
                {
                    //win11以上
                    if (info.BuildNum >= (uint)(BuildNumber.Windows_11_22H2))
                    {
                        _logs.DebugMsg_1($"Check_CanBeOTAUpdate OS is Windows11 22H2 or higher");
                        ret = true;
                    }
                    //win10以上
                    else if (info.BuildNum < (uint)(BuildNumber.Windows_11_21H2) && info.BuildNum >= (uint)(BuildNumber.Windows_10_20H2))
                    {
                        _logs.DebugMsg_1($"Check_CanBeOTAUpdate OS is Windows10 20H2 or higher");
                        ret = true;
                    }
                }
                bool isUPD = true;
                if (!string.IsNullOrEmpty(fwUpdateInfo.TheLatestVersion))
                {
                    _logs.DebugMsg_1($"Check_CanBeOTAUpdate Webcam TheLatestVersion is : {fwUpdateInfo.TheLatestVersion}");
                    if (fwUpdateInfo.TheLatestVersion.Contains("93") || fwUpdateInfo.TheLatestVersion.Contains("95") ||
                        fwUpdateInfo.TheLatestVersion.Contains("9.3") || fwUpdateInfo.TheLatestVersion.Contains("9.5"))
                    {
                        isUPD = false;
                        _logs.DebugMsg_1($"Check_CanBeOTAUpdate Webcam TheLatestVersion is MPS");
                    }
                }
                _logs.DebugMsg_1($"Check_CanBeOTAUpdate Webcam FW is support HPD : {fwUpdateInfo.IsESISupported}");
                //Updates can only be displayed if the firmware is HPD and the OS supports MPS.
                ret = fwUpdateInfo.IsESISupported && ret && isUPD;
            }
            if (!GlobalDefinitions.isSupport210)
            {
                if (fwUpdateInfo.DeviceType == DeviceType.LogicalAirAudio)//0205 Added by Bruce, to skip CADI FWU.
                {
                    _logs.DebugMsg_1($"Check_CanBeOTAUpdate DeviceType is DeviceType.LogicalAirAudio can not be update");
                    ret = false;
                }
            }
            _logs.DebugMsg_1($"Check_CanBeOTAUpdate finish. ret : {ret}");
            return ret;
        }
        string GetDeviceID(FWUpdateInfo fwUpdateInfo)
        {
            _logs.DebugMsg_1($"GetDeviceID start");
            string ret = string.Empty;
            try
            {
                if (fwUpdateInfo.DeviceType != DeviceType.LogicalHeadset)
                {
                    // 提取 deviceId 並將三個字節組合起來，作為參數傳遞給 FW 更新程序
                    int deviceId = fwUpdateInfo.InstanceId;

                    byte device_id_1 = (byte)(deviceId & 0xFF);
                    byte device_id_2 = (byte)((deviceId >> 8) & 0xFF);
                    byte device_id_3 = (byte)((deviceId >> 16) & 0xFF);

                    // 組合三個字節
                    ret = $" /deviceID:{device_id_1:X2}{device_id_2:X2}{device_id_3:X2}";
                }
                else
                {
                    // 對於耳機，將 instance id 視為 deviceHandle 並傳遞給耳機的 FW 更新程序
                    int deviceHandleId = fwUpdateInfo.InstanceId;

                    ret = $" /deviceID:{deviceHandleId:X4}";
                }
            }
            catch (Exception ex)
            {
                _logs.DebugMsg_1($"GetDeviceID eroor : {ex.Message}");
            }
            _logs.DebugMsg_1($"GetDeviceID ret : {ret}");
            _logs.DebugMsg_1($"GetDeviceID done");
            return ret;
            /*IL Sample
            if (updaterItemModel->_deviceType != IDevice::DeviceTypeLogicalHeadset) // Check if device is not headset type
            {
                //
                // Extract the deviceId and combine the three bytes to pass to FW updater as arguments.
                //
                int deviceId = updaterItemModel->_instanceId;

                uchar device_id_1 = (deviceId & 0xFF);
                uchar device_id_2 = ((deviceId >> 8) & 0xFF);
                uchar device_id_3 = ((deviceId >> 16) & 0xFF);

                //
                // combine the 3 bytes
                //
                arguments << ("/deviceID:" + QString("%1").arg(QString::number(device_id_1, 16), 2, QChar('0')) + QString("%1").arg(QString::number(device_id_2, 16), 2, QChar('0')) + QString("%1").arg(QString::number(device_id_3, 16), 2, QChar('0')));
            }
            else
            {
                //
                // For headset treat the instance id as deviceHandle and pass it to FW updater of headset
                //
                int deviceHandleId = updaterItemModel->_instanceId;

                arguments << ("/deviceID:" + QString("%1").arg(QString::number(deviceHandleId, 16), 4, QChar('0')));
            }*/
        }
        /// <summary>
        /// Rearrange firmware update order
        /// </summary>
        /// <param name="fWUpdateInfos">Original array</param>
        /// <returns>Arranged array</returns>
        List<FWUpdateInfo> Rearrange(List<FWUpdateInfo> fWUpdateInfos)
        {
            _logs.DebugMsg_1($"{nameof(Rearrange)} start");
            try
            {
                Dictionary<DeviceType, int> priority;
                //Define sorting priorities
                if (GlobalDefinitions.isSupport210)
                {
                    priority = new Dictionary<DeviceType, int>
                    {
                    { DeviceType.PhysicalDongle, 1 },
                    { DeviceType.PhysicalAudioDongle, 2 },

                    { DeviceType.LogicalMouse, 3 },
                    { DeviceType.LogicalKeyboard, 4 },

                    { DeviceType.LogicalWebcam, 5 },
                    { DeviceType.PhysicalWebcam, 6 },

                    { DeviceType.LogicalPen, 7 },
                    { DeviceType.PhysicalPen, 8 },

                    { DeviceType.LogicalWiredAudio, 9 },
                    { DeviceType.PhysicalWiredAudio, 10 },
                    { DeviceType.PhysicalBluetoothAudio, 11 },
                    { DeviceType.LogicalHeadset, 12 },

                    { DeviceType.PhysicalBootloader, 13 },
                    { DeviceType.LogicalBootloader, 14 },

                    { DeviceType.LogicalAirAudio, 15 },

                    { DeviceType.Unknown, 96 }, // Display
                    { DeviceType.PhysicalWiredDock, 98 },
                    { DeviceType.LogicalDock, 99 }
                    };
                }
                else
                {
                    priority = new Dictionary<DeviceType, int>
                    {
                    { DeviceType.PhysicalDongle, 1 },
                    { DeviceType.PhysicalAudioDongle, 2 },

                    { DeviceType.LogicalMouse, 3 },
                    { DeviceType.LogicalKeyboard, 4 },

                    { DeviceType.LogicalWebcam, 5 },
                    { DeviceType.PhysicalWebcam, 6 },

                    { DeviceType.LogicalPen, 7 },
                    { DeviceType.PhysicalPen, 8 },

                    { DeviceType.LogicalWiredAudio, 9 },
                    { DeviceType.PhysicalWiredAudio, 10 },
                    { DeviceType.PhysicalBluetoothAudio, 11 },
                    { DeviceType.LogicalHeadset, 12 },

                    { DeviceType.PhysicalBootloader, 13 },
                    { DeviceType.LogicalBootloader, 14 },

                    { DeviceType.Unknown, 96 }, // Display
                    { DeviceType.PhysicalWiredDock, 98 },
                    { DeviceType.LogicalDock, 99 }
                    };
                }


                fWUpdateInfos.Sort((x, y) =>
                {
                    int xPriority = x.IsDisplay ? priority[DeviceType.Unknown] : priority.GetValueOrDefault(x.DeviceType, int.MaxValue);
                    int yPriority = y.IsDisplay ? priority[DeviceType.Unknown] : priority.GetValueOrDefault(y.DeviceType, int.MaxValue);

                    return xPriority.CompareTo(yPriority);
                });
            }
            catch (Exception ex)
            {
                _logs.DebugMsg_1($"{nameof(Rearrange)} error : {ex.Message}");
            }
            _logs.DebugMsg_1($"{nameof(Rearrange)} done");
            return fWUpdateInfos;
        }
        bool IsISPInApp(string filePath, out string upgFilePath)
        {
            _logs.DebugMsg_1($"{nameof(IsISPInApp)} start");
            bool ret = false;
            upgFilePath = "";
            try
            {
                if (!string.IsNullOrEmpty(filePath))
                {
                    // 取得檔案版本資訊
                    FileVersionInfo fileInfo = FileVersionInfo.GetVersionInfo(filePath);
                    if (fileInfo != null)
                    {
                        // 取得 File Description
                        string? fileDescription = fileInfo.FileDescription;
                        if (!string.IsNullOrEmpty(fileDescription))
                        {
                            // 輸出 File Description
                            _logs.DebugMsg_1($"{nameof(IsISPInApp)} fileDescription : {fileDescription}");
                            ret = fileDescription.ToLower(CultureInfo.InvariantCulture).Equals("In-app ISP=true;".ToLower(CultureInfo.InvariantCulture));
                            if (ret)
                            {
                                Unzip unzip = new Unzip(_logs);
                                _logs.DebugMsg_1($"{nameof(IsISPInApp)} unzip.ExecuteUnzipForInAppUpdate go");
                                if (unzip.ExecuteUnzipForInAppUpdate(filePath, out upgFilePath))
                                {
                                    _logs.DebugMsg_1($"{nameof(IsISPInApp)} unzip.ExecuteUnzipForInAppUpdate done");
                                }
                                else
                                {
                                    _logs.DebugMsg_1($"{nameof(IsISPInApp)} unzip.ExecuteUnzipForInAppUpdate fail");
                                }
                                unzip = null;
                            }
                            else
                            {
                                _logs.DebugMsg_1($"{nameof(IsISPInApp)} this ISP is not In-app");
                            }
                        }
                        else
                        {
                            _logs.DebugMsg_1($"{nameof(IsISPInApp)} fileDescription IsNullOrEmpty");
                        }
                    }
                    else
                    {
                        _logs.DebugMsg_1($"{nameof(IsISPInApp)} fileInfo is null");
                    }
                }
                else
                {
                    _logs.DebugMsg_1($"{nameof(IsISPInApp)} filePath IsNullOrEmpty");
                }
            }
            catch (Exception ex)
            {
                _logs.DebugMsg_1($"{nameof(IsISPInApp)} error : {ex.Message}");
            }
            _logs.DebugMsg_1($"{nameof(IsISPInApp)} done, ret : {ret}");
            return ret;
        }
        private bool runISP(string dllFilename, string ModelName, string ServiceTag, string UpgPath, bool EnableDebugMode, Action<Result, int, int> callback)
        {
            bool ret = false;

            if (File.Exists(dllFilename))
            {
                try
                {
                    string directory = Path.GetDirectoryName(dllFilename);
                    // 設定當前工作目錄
                    Directory.SetCurrentDirectory(directory);
                    // Step 1 & 2: Load the assembly and get the type
                    Assembly assembly = Assembly.LoadFrom(dllFilename);

                    Type? dllType = assembly.GetType("FirmwareUpdater.FirmwareUpdaterDllWrapper", true);

                    if (dllType != null)
                    {
                        // Step 3: Get the MethodInfo object for the StartISP method
                        MethodInfo? methodInfo = dllType.GetMethod("StartISP");

                        if (methodInfo != null)
                        {
                            // Step 4: Create an instance of the type
                            object? instance = Activator.CreateInstance(dllType);

                            if (instance != null)
                            {
                                object[] parameters = new object[] { ModelName, ServiceTag, UpgPath, callback, EnableDebugMode };

                                // Step 5: Invoke the method
                                methodInfo.Invoke(instance, parameters);

                                ret = true;
                                instance = null;
                            }
                            methodInfo = null;
                        }

                        dllType = null;
                    }
                }
                catch (TargetInvocationException tie)
                {
                    // This exception is thrown if the invoked method throws an exception
                    _logs.DebugMsg_1($"{DateTime.Now}--DeviceName : {_fWUpdateInfo.DeviceName} Model : {_fWUpdateInfo.Model} to ver : {_fWUpdateInfo.TheLatestVersion} Caught TargetInvocationException: {tie.InnerException?.Message}");
                    // Handle the inner exception
                }
                catch (Exception ex)
                {
                    _logs.DebugMsg_1($"{DateTime.Now}--DeviceName : {_fWUpdateInfo.DeviceName} Model : {_fWUpdateInfo.Model} to ver : {_fWUpdateInfo.TheLatestVersion} nCaught Exception: {ex.Message}");
                }
            }
            else
            {
                _logs.DebugMsg_1($"{DateTime.Now}--DeviceName : {_fWUpdateInfo.DeviceName} Model : {_fWUpdateInfo.Model} to ver : {_fWUpdateInfo.TheLatestVersion} nUnable to load: {dllFilename}");
            }

            return ret;
        }
        private void CheckDisplayErrorCode(int errorCode)
        {
            _notificationTitle = $"{LangHelper.Instance["Error"]} {errorCode}";
            switch (errorCode)
            {
                case 0:
                    _updateErrorCode = FWUErrorCode.NoError;
                    _notificationTitle = LangHelper.Instance["Success"];
                    _notificationStr = LangHelper.Instance["A2_Firmware_update_successful"];
                    _logs.DebugMsg_1($"{_fWUpdateInfo.DeviceName} {_fWUpdateInfo.Model} Get errorCode : {errorCode}:Success");
                    break;
                case 1:
                case 2:
                    _updateErrorCode = FWUErrorCode.FirmwareUpdateFailed;
                    _notificationStr = LangHelper.Instance["Display_FWU_Error_1"];
                    _logs.DebugMsg_1($"{_fWUpdateInfo.DeviceName} {_fWUpdateInfo.Model} Get errorCode : {errorCode}:Firmware utility package issue, may due to the file corrupted. ");
                    break;
                case 3:
                case 4:
                case 5:
                case 12:
                case 61:
                case 62:
                    _updateErrorCode = FWUErrorCode.FirmwareUpdateFailed;
                    _notificationStr = LangHelper.Instance["Display_FWU_Error_5"];
                    _logs.DebugMsg_1($"{_fWUpdateInfo.DeviceName} {_fWUpdateInfo.Model} Get errorCode : {errorCode}:The firmware update encountered an error. To resolve this issue, disconnect and reconnect the USB cable and power cycle the monitor. Then, retry the firmware update.");
                    break;
                /*case 12:
                    _updateErrorCode = FWUErrorCode.FirmwareUpdateFailed;
                    _notificationStr = LangHelper.Instance["Display_FWU_Error_12"];
                    _logs.DebugMsg_1($"{_fWUpdateInfo.DeviceName} {_fWUpdateInfo.Model} Get errorCode : {errorCode}:Monitor restart failed. Ask the user to power cycle the monitor and retry if the firmware is outdated.");
                    break;*/
                case 7:
                case 8:
                    _updateErrorCode = FWUErrorCode.FirmwareUpdateFailed;
                    _notificationStr = LangHelper.Instance["Display_FWU_Error_7"];
                    _logs.DebugMsg_1($"{_fWUpdateInfo.DeviceName} {_fWUpdateInfo.Model} Get errorCode : {errorCode}:The firmware update was unsuccessful. The firmware code has been erased, and verification of the firmware code failed.");
                    break;
                /*case 3:
                case 4:
                    _updateErrorCode = FWUErrorCode.FirmwareUpdateFailed;
                    _notificationStr = LangHelper.Instance["Display_FWU_Error_3"];
                    _logs.DebugMsg_1($"{_fWUpdateInfo.DeviceName} {_fWUpdateInfo.Model} Get errorCode : {errorCode}:The firmware update utility is unable to establish communication with the monitor. Please reconnect the USB cable and power cycle the monitor before attempting the firmware update again.");
                    break;*/
                case 9:
                    _updateErrorCode = FWUErrorCode.FirmwareUpdateFailed;
                    _notificationStr = LangHelper.Instance["Display_FWU_Error_9"];
                    _logs.DebugMsg_1($"{_fWUpdateInfo.DeviceName} {_fWUpdateInfo.Model} Get errorCode : {errorCode}:The downloaded package version is older than the firmware version running in the monitor. Monitor firmware updated.");
                    break;
                case 10:
                    _updateErrorCode = FWUErrorCode.FirmwareUpdateFailed;
                    _notificationStr = LangHelper.Instance["Display_FWU_Error_10"];
                    _logs.DebugMsg_1($"{_fWUpdateInfo.DeviceName} {_fWUpdateInfo.Model} Get errorCode : {errorCode}:The Firmware Utility may currently be active. This indicates that another instance of the Firmware Update is running. Please close the Firmware Update utility.");
                    break;
                case 201:
                    if (string.IsNullOrEmpty(_fWUpdateInfo.UpdateTime))
                    {
                        _notificationStr = LangHelper.Instance["Display_FWU_Error_201"].Replace("xx", "30");
                    }
                    else
                    {
                        _notificationStr = LangHelper.Instance["Display_FWU_Error_201"].Replace("xx", _fWUpdateInfo.UpdateTime);
                    }
                    _logs.DebugMsg_1($"{_fWUpdateInfo.DeviceName} {_fWUpdateInfo.Model} Get errorCode : {errorCode}:The update may take up to xx minutes. Keep the device powered on and connected during this time.");
                    break;
                case 501:
                    _updateErrorCode = FWUErrorCode.FirmwareUpdateFailed;
                    _notificationStr = LangHelper.Instance["Display_FWU_Error_501"];
                    _logs.DebugMsg_1($"{_fWUpdateInfo.DeviceName} {_fWUpdateInfo.Model} Get errorCode : {errorCode}:Firmware version detected on monitor is not an official release");
                    break;
                /*case 61:
                case 62:
                    _updateErrorCode = FWUErrorCode.FirmwareUpdateFailed;
                    _notificationStr = LangHelper.Instance["Display_FWU_Error_61"];
                    _logs.DebugMsg_1($"{_fWUpdateInfo.DeviceName} {_fWUpdateInfo.Model} Get errorCode : {errorCode}:Firmware update failed. If it continues, contact Dell support.");
                    break;*/
                default:
                    _updateErrorCode = FWUErrorCode.Unknow;
                    _notificationStr = LangHelper.Instance["Display_FWU_Error_Other"];
                    _logs.DebugMsg_1($"{_fWUpdateInfo.DeviceName} {_fWUpdateInfo.Model} Get errorCode : {errorCode}:The firmware update was unsuccessful.");
                    break;
            }
        }
        // add @ 20241202 stephen
        public void setFWUpdateInfoPackage(FWUpdateInfoPackage pkg)
        {
            _fWUpdateInfoPackage = pkg;
        }
    }
}
