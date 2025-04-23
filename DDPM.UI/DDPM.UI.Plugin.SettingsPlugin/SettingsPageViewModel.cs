using CommunityToolkit.Mvvm.ComponentModel;
using DDPM.SA.Common;
using DDPM.SA.Common.Method;
using DDPM.SA.Common.Popup;
using DDPM.SA.Common.Settings;
using DDPM.UI.Common;
using DDPM.UI.Common.Interfaces;
using DDPM.UI.Common.Models;
using DDPM.UI.Plugin.Common;
using DDPM.UI.Plugin.DdpmHomePlugin.Interfaces;
using DDPM.UI.Resources.Helper;
using Dell.Client.Framework.Common;
using DPeMPublic.Common.Enums;
using Microsoft.VisualBasic.Logging;
using Microsoft.Win32;
using Newtonsoft.Json.Linq;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Threading;
using VcpCore.Common;
using static DDPM.UI.Plugin.SettingsPlugin.GlobalSettingsParam;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;
using Application = System.Windows.Application;

namespace DDPM.UI.Plugin.SettingsPlugin
{
    public class SettingsPageViewModel : ObservableObject, ISettingsPageViewModel
    {
        private static readonly object lockObject = new object();
        private List<HomeDevice> _homeDevices = new List<HomeDevice>();
        public bool[] IsSelected { get; set; } = new bool[5];
        public ILog? Log { get; set; }

        public List<HomeDevice> HomeDevices
        {
            get => _homeDevices;
            set
            {
                SetProperty(ref _homeDevices, value);
                OnPropertyChanged("HomeDeviceCount");
            }
        }

        private HomeDevice? _selectedHomeDevice;

        public HomeDevice? SelectedHomeDevice
        {
            get => _selectedHomeDevice;
            set => SetProperty(ref _selectedHomeDevice, value);
        }

        public IModuleOwner? ModuleOwner { get; set; }
        private ContentControl? _fullView;

        public ContentControl? FullView
        {
            get => _fullView;
            set => SetProperty(ref _fullView, value);
        }

        //public ICommand? OpenFullViewCommand { get; set; }
        //public ICommand? CloseFullViewCommand { get; set; }
        Settings_General settings_General = null;
        UpdatesPage updatesPage = null;
        AnalyticsPage analyticsPage = null;
        Settings_WidgetSettings settings_WidgetSettings = null;
        Settings_About settings_About = null;
        public void SetSelected(int index)
        {
            for (int j = 0; j < IsSelected.Length; j++)
            {
                IsSelected[j] = false;
            }
            IsSelected[index] = true;
            dynamic vm = null;
            switch (index)
            {
                case 0:
                default:
                    if (settings_General == null)
                    {
                        settings_General = new Settings_General();
                    }
                    vm = settings_General;
                    break;
                case 1:
                    if (updatesPage == null)
                    {
                        updatesPage = new UpdatesPage();
                    }
                    vm = updatesPage;
                    break;
                case 2:
                    if (analyticsPage == null)
                    {
                        analyticsPage = new AnalyticsPage();
                    }
                    vm = analyticsPage;
                    break;
                case 3:
                    if (settings_WidgetSettings == null)
                    {
                        settings_WidgetSettings = new Settings_WidgetSettings();
                    }
                    vm = settings_WidgetSettings;
                    break;
                case 4:
                    if (settings_About == null)
                    {
                        settings_About = new Settings_About();
                    }
                    vm = settings_About;
                    break;
            }
            if (vm != null)
            {
                OpenFullView(vm);
            }
            OnPropertyChanged("IsSelected");
        }
        #region UI Enable Flags

        private bool _isBusy = false;

        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                SetProperty(ref _isBusy, value);
            }
        }
        private bool _IsBusy_UpdatePage = false;

        public bool IsBusy_UpdatePage
        {
            get => _IsBusy_UpdatePage;
            set
            {
                SetProperty(ref _IsBusy_UpdatePage, value);
            }
        }

        #endregion UI Enable Flags
        public void OpenFullView(ContentControl content)
        {
            //if (OpenFullViewCommand != null)
            //    OpenFullViewCommand?.Execute(this);
            FullView = content;
            FullView.Visibility = Visibility.Visible;
        }

        public void CloseFullView()
        {
            FullView = null;
        }
        public void Invoke_RefreshData()
        {
            Log?.Info($"Invoke_RefreshData start");
            BackgroundWorker bw = new BackgroundWorker()
            {
                WorkerReportsProgress = false,
                WorkerSupportsCancellation = false
            };
            bw.DoWork += DoWork_RefreshData;
            bw.RunWorkerCompleted += Set_Page_Done;
            bw.RunWorkerAsync(); //myArg is the optional argument
            IsBusy = true;
            OnPropertyChanged("IsBusy");
        }
        public void Invoke_RefreshData_1()
        {
            Log?.Info($"Invoke_RefreshData_1 start");
            BackgroundWorker bw = new BackgroundWorker()
            {
                WorkerReportsProgress = false,
                WorkerSupportsCancellation = false
            };
            IsBusy_UpdatePage = true;
            OnPropertyChanged("IsBusy_UpdatePage");
            bw.DoWork += DoWork_RefreshData_1;
            bw.RunWorkerCompleted += Set_Page_Done_1;
            bw.RunWorkerAsync(); //myArg is the optional argument
            //IsBusy_UpdatePage = true;
            //OnPropertyChanged("IsBusy_UpdatePage");
        }

        private void DoWork_RefreshData(object sender, DoWorkEventArgs e)
        {
            try
            {
                Log?.Info($"Invoke_RefreshData go");
                //bool GetPdemFile=DdpmCommonHelper.DeviceManagerSA.CheckInstallFirstOpen().Result;
                Global.SettingParam = DdpmCommonHelper.DeviceManagerSA.GetGlobalSettingParam().Result;
                GlobalSettingParam = Global.SettingParam;
                DDPMSettings data = DdpmCommonHelper.ReadDDPMSettings();//DeviceManagerSA.ReloadAppConfigData().Result;
                Lock_AnalyticsPage = data.LockSettings.Lock_Settings_TelemetryConsent;
                Trace.WriteLine($"[SettingsPage] Apply TelemetryConsent(check) : {data.LockSettings.Lock_Settings_TelemetryConsent}");
                Lock_UpdatesPage = data.LockSettings.Lock_Settings_Updates;
                Trace.WriteLine($"[SettingsPage] Apply FW/SW Updates(check) : {data.LockSettings.Lock_Settings_Updates}");
                Lock_GeneralPage = data.LockSettings.Lock_Setting_ScreenNotification;
                Trace.WriteLine($"[SettingsPage] Apply General(check) : {data.LockSettings.Lock_Setting_ScreenNotification}");
                RefreshUI();
            }
            catch (Exception ex)
            {
                Log?.Error($"[SettingsPageViewModel][DoWork_RefreshData] exception: {ex}");
            }
        }

        private void Set_Page_Done(object sender, RunWorkerCompletedEventArgs e)
        {
            IsBusy = false;
            OnPropertyChanged("IsBusy");
            Log?.Info($"Invoke_RefreshData done");
        }
        private void DoWork_RefreshData_1(object sender, DoWorkEventArgs e)
        {
            Log?.Info($"Invoke_RefreshData_1 go");
            try
            {
                Log?.Info($"Invoke_RefreshData done");
                if (CheckLockStateToSeeIfSkipUpdateCheck())
                {
                    IsBusy_UpdatePage = false;
                    OnPropertyChanged("IsBusy_UpdatePage");
                    return;
                }
                SetUpdateInfoUI(DdpmCommonHelper.DeviceManagerSA.GetFWUpdateInfo(true).Result, DdpmCommonHelper.DeviceManagerSA.SW_GetSWUpdateInfo(false, true).Result);
                RefreshUI();
            }
            catch (Exception ex)
            {
                Log?.Error($"[SettingsPageViewModel][DoWork_RefreshData_1] exception: {ex}");
                IsBusy_UpdatePage = false;
                OnPropertyChanged("IsBusy_UpdatePage");
            }
        }

        private void Set_Page_Done_1(object sender, RunWorkerCompletedEventArgs e)
        {
            if (SWUpdateInfoPackage != null && SWUpdateInfoPackage.SWUpdateInfo != null && SWUpdateInfoPackage.SWUpdateInfo.Count >= 1)
            {
                InterruptScreenRoot myDeserializedClass = null;
                BackgroundWorker bw = new BackgroundWorker
                {
                    WorkerReportsProgress = false,
                    WorkerSupportsCancellation = false
                };
                bw.DoWork += delegate
                {
                    IsBusy_UpdatePage = true;
                    OnPropertyChanged("IsBusy_UpdatePage");
                    myDeserializedClass = DdpmCommonHelper.DeviceManagerSA.InterruptScreen_Metadata().Result;
                };
                bw.RunWorkerCompleted += delegate
                {
                    Dispatcher.CurrentDispatcher.BeginInvoke(new Action(() =>
                    {
                        if (myDeserializedClass != null)
                        {
                            bool? b = false;

                            InterruptScreen interruptScreen = new InterruptScreen(SWUpdateInfoPackage.SWUpdateInfo[0].TheLatestVersion, myDeserializedClass, DdpmCommonHelper.DeviceManagerSA, SWUpdateInfoPackage, Log);
                            interruptScreen.Owner = System.Windows.Application.Current.MainWindow;
                            interruptScreen.Show();
                            /*b = interruptScreen.ShowDialog();
                            if (b == true)
                            {
                                Log?.Info("CheckIfSwFwUpdateAvailable SW_DownloadAndInstall go");
                                List<SWUpdateInfo> swUpdateInfos = DdpmCommonHelper.DeviceManagerSA.SW_DownloadAndInstall(SWUpdateInfoPackage.SWUpdateInfo, true, "").Result;
                                Log?.Info("CheckIfSwFwUpdateAvailable SW_DownloadAndInstall finish");
                                //SetSelected(1);
                            }*/

                        }
                    }));
                    IsBusy_UpdatePage = false;
                    OnPropertyChanged("IsBusy_UpdatePage");
                };
                bw.RunWorkerAsync();
            }
            else
            {
                IsBusy_UpdatePage = false;
                OnPropertyChanged("IsBusy_UpdatePage");
            }
            Log?.Info($"Invoke_RefreshData_1 done");
        }
        #region General
        public GlobalSettingParam GlobalSettingParam { get; set; }
        public string EnableQuickAccessWidget_String
        {
            get
            {
                //avoid null
                if (GlobalSettingParam == null || GlobalSettingParam.GlobalSetting_WidgetSettings == null)
                    return LangHelper.Instance["Off"];
                if (GlobalSettingParam.GlobalSetting_WidgetSettings.EnableQuickAccessWidget)
                {
                    return LangHelper.Instance["On"];
                }
                return LangHelper.Instance["Off"];
            }
        }
        public string EnableQuickAccessWidget_Reminder_String
        {
            get
            {
                //avoid null
                if (GlobalSettingParam == null || GlobalSettingParam.GlobalSetting_WidgetSettings == null)
                    return LangHelper.Instance["Off"];
                if (GlobalSettingParam.GlobalSetting_WidgetSettings.EnableQuickAccessWidget_Reminder)
                {
                    return LangHelper.Instance["On"];
                }
                return LangHelper.Instance["Off"];
            }
        }
        public string SWVersion
        {
            get
            {
                return $"{LangHelper.Instance["Software_version"]}: {GlobalSettingParam.GlobalSetting_About.SWVersion}";
            }
        }
        public string DriverVersion
        {
            get
            {
                return $"{LangHelper.Instance["Driver_version"]}: {GlobalSettingParam.GlobalSetting_About.DriverVersion}";
            }
        }
        public void SaveMonitorAssetReport(string filePath)
        {
            Log?.Info($"SaveMonitorAssetReport start");
            BackgroundWorker bw = new BackgroundWorker()
            {
                WorkerReportsProgress = false,
                WorkerSupportsCancellation = false
            };
            bw.DoWork += Set_SaveMonitorAssetReport_Dowork;
            bw.RunWorkerCompleted += Set_Page_Done;
            bw.RunWorkerAsync(filePath);
            IsBusy = true;
            OnPropertyChanged("IsBusy");
            Log?.Info($"SaveMonitorAssetReport done");
        }
        public void SaveDiagnosticReport(string filePath)
        {
            Log?.Info($"SaveDiagnosticReport start");
            BackgroundWorker bw = new BackgroundWorker()
            {
                WorkerReportsProgress = false,
                WorkerSupportsCancellation = false
            };
            bw.DoWork += Set_SaveDiagnosticReport_Dowork;
            bw.RunWorkerCompleted += Set_Page_Done;
            bw.RunWorkerAsync(filePath);
            IsBusy = true;
            OnPropertyChanged("IsBusy");
            Log?.Info($"SaveDiagnosticReport done");
        }
        private void Set_SaveMonitorAssetReport_Dowork(object sender, DoWorkEventArgs e)
        {
            Log?.Info($"SaveMonitorAssetReport_Dowork start");
            string filePath = e.Argument.ToString();
            List<MonitorInfo> monitorInfos = DdpmCommonHelper.DeviceManagerSA.GetMonitors().Result;
            bool monitorAssetReports = DdpmCommonHelper.DeviceManagerSA.ExportMonitorAssetReport(monitorInfos, filePath).Result;
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                if (!monitorAssetReports)
                {
                    MessageModalDialog msgBox = new MessageModalDialog(LangHelper.Instance["Error"], LangHelper.Instance["MonitorAssetReport_SaveFail"], LangHelper.Instance["OK"]);
                    Window mainWindow = System.Windows.Application.Current.MainWindow;
                    if (mainWindow != null)
                    {
                        msgBox.Owner = mainWindow;
                        msgBox.Left = mainWindow.Left + (mainWindow!.ActualWidth - 417) / 2;
                        msgBox.Top = mainWindow.Top + 300;
                        msgBox.ShowDialog();
                    }
                }
            }));
            Log?.Info($"SaveMonitorAssetReport_Dowork done");
        }
        private void Set_SaveDiagnosticReport_Dowork(object sender, DoWorkEventArgs e)
        {
            Log?.Info($"SaveDiagnosticReport_Dowork start");
            //Install software information
            CommonFunctions.IsServiceRunning(GlobalDefinitions.DPeMServiceName, Log);//add log before save
            Log?.Info($"DPeM installed ver: {CommonFunctions.GetInstalledSoftwareVersion(GlobalDefinitions.InstalledName_DPeM, Log)}");
            Log?.Info($"NKVM installed ver: {CommonFunctions.GetInstalledSoftwareVersion(GlobalDefinitions.InstalledName_NKVM, Log)}");

            string filePath = e.Argument.ToString();
            bool monitorAssetReports = DiagnosticReport.SaveLogFile(filePath, Log); //DdpmCommonHelper.DeviceManagerSA.SaveLogFile(filePath).Result;
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                if (!monitorAssetReports)
                {
                    MessageModalDialog msgBox = new MessageModalDialog(LangHelper.Instance["Error"], LangHelper.Instance["DiagnosticReport_SaveFail"], LangHelper.Instance["OK"]);
                    Window mainWindow = System.Windows.Application.Current.MainWindow;
                    if (mainWindow != null)
                    {
                        msgBox.Owner = mainWindow;
                        msgBox.Left = mainWindow.Left + (mainWindow.ActualWidth - 417) / 2;
                        msgBox.Top = mainWindow.Top + 300;
                        msgBox.ShowDialog();
                    }
                }
            }));
            Log?.Info($"SaveDiagnosticReport_Dowork done");
        }

        #endregion
        #region Update
        public FWUpdateInfoPackage FWUpdateInfoPackage { get; set; }
        public SWUpdateInfoPackage SWUpdateInfoPackage { get; set; }
        public List<UIUpdateInfo> Critical_UpdateList_UI { get; set; }
        public List<UIUpdateInfo> Recommended_UpdateList_UI { get; set; }
        public List<UIUpdateInfo> Optional_UpdateList_UI { get; set; }
        public string LastCheckDate { get; set; }
        public string UpdateTitle { get; set; }
        public string UpdateVersion { get; set; }
        public string ProgressStr { get; set; }
        public int ProgressValue { get; set; }
        public bool Progress_IsAnimated { get; set; }

        public Visibility NoUpdateAlert { get; set; }

        public Visibility NoNetwork { get; set; } = Visibility.Collapsed;
        public Visibility Critical_UpdateList { get => Critical_UpdateList_UI?.Count >= 1 ? Visibility.Visible : Visibility.Collapsed; }
        public Visibility Recommended_UpdateList { get => Recommended_UpdateList_UI?.Count >= 1 ? Visibility.Visible : Visibility.Collapsed; }
        public Visibility Optional_UpdateList { get => Optional_UpdateList_UI?.Count >= 1 ? Visibility.Visible : Visibility.Collapsed; }

        public Visibility IsAnyUpdate
        {
            get => ((Critical_UpdateList_UI?.Count >= 1 ||
                Recommended_UpdateList_UI?.Count >= 1 ||
                Optional_UpdateList_UI?.Count >= 1) && _Lock_UpdatesPage == false) ? Visibility.Visible : Visibility.Collapsed;
        }

        private bool CheckLockStateToSeeIfSkipUpdateCheck()
        {
            DDPMSettings data = DdpmCommonHelper.ReadDDPMSettings();
            if (data != null)
            {
                if (data.LockSettings != null)
                {
                    if (data.LockSettings.Lock_Settings_Updates)
                    {
                        Log?.Info($"[CheckUpdate] UI locked, drop the update checking");
                        IsBusy_UpdatePage = false;
                        OnPropertyChanged("IsBusy_UpdatePage");
                        return true;
                    }
                    else
                    {
                        Log?.Info($"[CheckUpdate] UI unlocked, continue to check update");
                    }
                }
                else
                {
                    Log?.Info($"[CheckUpdate] can't read LockSettings to check if UI locked, continue to check update");
                }
            }
            else
            {
                Log?.Info($"[CheckUpdate] can't read DDPM settings to check if UI locked, continue to check update");
            }
            return false;
        }

        public void CheckUpdate()
        {
            if (CheckLockStateToSeeIfSkipUpdateCheck())
                return;
            Log?.Info($"CheckUpdate start");
            BackgroundWorker bw = new BackgroundWorker()
            {
                WorkerReportsProgress = false,
                WorkerSupportsCancellation = false
            };
            bw.DoWork += Set_CheckUpdate_Dowork;
            bw.RunWorkerCompleted += Set_Page_Done_1;
            bw.RunWorkerAsync();
            IsBusy_UpdatePage = true;
            OnPropertyChanged("IsBusy_UpdatePage");
        }
        private void Set_CheckUpdate_Dowork(object sender, DoWorkEventArgs e)
        {
            if (CheckLockStateToSeeIfSkipUpdateCheck())
            {
                IsBusy_UpdatePage = false;
                OnPropertyChanged("IsBusy_UpdatePage");
                return;
            }
            Log?.Info($"CheckUpdate start");
            SetUpdateInfoUI(DdpmCommonHelper.DeviceManagerSA.GetFWUpdateInfo(true).Result, DdpmCommonHelper.DeviceManagerSA.SW_GetSWUpdateInfo(false, true).Result);
            RefreshUI();
            Log?.Info($"CheckUpdate done");
        }

        public void SetUpdateInfoUI(FWUpdateInfoPackage fwUpdateInfoPackage, SWUpdateInfoPackage swUpdateInfoPackage)
        {
            lock (lockObject)
            {
                Log?.Info($"SetUpdateInfoUI start");
                LastCheckDate = fwUpdateInfoPackage.TheLastCheckTime.ToString();
                FWUpdateInfoPackage = fwUpdateInfoPackage;
                SWUpdateInfoPackage = swUpdateInfoPackage;
                Critical_UpdateList_UI = new List<UIUpdateInfo>();
                Recommended_UpdateList_UI = new List<UIUpdateInfo>();
                Optional_UpdateList_UI = new List<UIUpdateInfo>();
                if (!NetworkInterface.GetIsNetworkAvailable())
                {
                    NoNetwork = Visibility.Visible;
                }
                else
                {
                    NoNetwork = Visibility.Collapsed;
                    List<DeviceInfo> deviceInfos = DdpmCommonHelper.DeviceManagerSA.GetDevices().Result.deviceInfo;
                    int ioDongleCount = DdpmCommonHelper.DeviceManagerSA.GetIODongleCountGen3AgoCount().Result;
                    Log?.Info($"SetUpdateInfoUI fwUpdateInfoPackage.FWUpdateInfo.Count : {fwUpdateInfoPackage.FWUpdateInfo.Count}");
                    Log?.Info($"SetUpdateInfoUI ioDongleCount : {ioDongleCount}");
                    foreach (FWUpdateInfo fwUpdateInfo in fwUpdateInfoPackage.FWUpdateInfo)
                    {
                        UIUpdateInfo uiUpdateInfo = new UIUpdateInfo(fwUpdateInfo, deviceInfos, ioDongleCount);
                        if (Critical_UpdateList_UI.Any(item => item.UpdateInfo == uiUpdateInfo.UpdateInfo) ||
                            Recommended_UpdateList_UI.Any(item => item.UpdateInfo == uiUpdateInfo.UpdateInfo) ||
                            Optional_UpdateList_UI.Any(item => item.UpdateInfo == uiUpdateInfo.UpdateInfo))
                        {
                            Log?.Info($"SetUpdateInfoUI item is exist : {uiUpdateInfo.UpdateInfo}");
                            continue; // 跳過此項目
                        }
                        if (uiUpdateInfo.IsCritical)//如果不能選擇是否更新為強制更新
                        {
                            Log?.Info($"SetUpdateInfoUI Critical_UpdateList_UI.Add {uiUpdateInfo.UpdateInfo}");
                            Critical_UpdateList_UI.Add(uiUpdateInfo);
                        }
                        else if (uiUpdateInfo.IsRecommended)//如果為true為建議更新
                        {
                            Log?.Info($"SetUpdateInfoUI Recommended_UpdateList_UI.Add {uiUpdateInfo.UpdateInfo}");
                            Recommended_UpdateList_UI.Add(uiUpdateInfo);
                        }
                        else//剩下的為選用更新
                        {
                            Log?.Info($"SetUpdateInfoUIOptional_UpdateList_UI.Add {uiUpdateInfo.UpdateInfo}");
                            Optional_UpdateList_UI.Add(uiUpdateInfo);
                        }
                    }
                    Log?.Info($"SetUpdateInfoUI swUpdateInfoPackage.SWUpdateInfo.Count : {swUpdateInfoPackage.SWUpdateInfo.Count}");
                    foreach (SWUpdateInfo swUpdateInfo in swUpdateInfoPackage.SWUpdateInfo)
                    {
                        UIUpdateInfo uiUpdateInfo = new UIUpdateInfo(swUpdateInfo);
                        if (Critical_UpdateList_UI.Any(item => item.UpdateInfo == uiUpdateInfo.UpdateInfo) ||
                            Recommended_UpdateList_UI.Any(item => item.UpdateInfo == uiUpdateInfo.UpdateInfo) ||
                            Optional_UpdateList_UI.Any(item => item.UpdateInfo == uiUpdateInfo.UpdateInfo))
                        {
                            Log?.Info($"SetUpdateInfoUI item is exist : {uiUpdateInfo.UpdateInfo}");
                            continue;
                        }
                        if ((!uiUpdateInfo.IsEnableCheckBox) && uiUpdateInfo.IsCheckUpdate)
                        {
                            Log?.Info($"SetUpdateInfoUI Critical_UpdateList_UI.Add {uiUpdateInfo.UpdateInfo}");
                            Critical_UpdateList_UI.Add(uiUpdateInfo);
                        }
                    }
                    if ((Critical_UpdateList_UI?.Count <= 0 &&
                        Recommended_UpdateList_UI?.Count <= 0 &&
                        Optional_UpdateList_UI?.Count <= 0))
                    {
                        NoUpdateAlert = Visibility.Visible;
                    }
                    else
                    {
                        NoUpdateAlert = Visibility.Collapsed;
                    }
                }
                if (NoNetwork == Visibility.Visible || NoUpdateAlert == Visibility.Visible)
                {
                    System.Timers.Timer timer = new System.Timers.Timer();
                    timer.Interval = TimeSpan.FromSeconds(5).TotalMilliseconds;
                    timer.Elapsed += (sender, args) =>
                    {
                        timer.Stop();
                        NoNetwork = Visibility.Collapsed;
                        NoUpdateAlert = Visibility.Collapsed;
                        RefreshUI();
                    };
                    timer.Start();
                }
                Log?.Info($"SetUpdateInfoUI Critical_UpdateList_UI.Count : {Critical_UpdateList_UI.Count}");
                Log?.Info($"SetUpdateInfoUI Recommended_UpdateList_UI.Count : {Recommended_UpdateList_UI.Count}");
                Log?.Info($"SetUpdateInfoUI Optional_UpdateList_UI.Count : {Optional_UpdateList_UI.Count}");
                Log?.Info($"SetUpdateInfoUI done");
            }
        }

        public bool IsCanUpdate()
        {
            if ((Critical_UpdateList_UI != null && Critical_UpdateList_UI.Count > 0) ||
                (Recommended_UpdateList_UI != null && Recommended_UpdateList_UI.Count > 0) ||
                (Optional_UpdateList_UI != null && Optional_UpdateList_UI.Count > 0))
            {
                return true;
            }
            return false;
        }

        public void SetVMUpdate()
        {
            Log?.Info($"SetVMUpdate start");
            List<FWUpdateInfo> fwUpdateInfos = new List<FWUpdateInfo>();
            List<SWUpdateInfo> swUpdateInfos = new List<SWUpdateInfo>();
            foreach (UIUpdateInfo uiUpdateInfo in Critical_UpdateList_UI)
            {
                if (uiUpdateInfo.IsCheckUpdate)
                {
                    if (uiUpdateInfo.SWUpdateInfo != null)
                    {
                        swUpdateInfos.Add(uiUpdateInfo.SWUpdateInfo);
                    }
                    else
                    {
                        fwUpdateInfos.Add(uiUpdateInfo.FWUpdateInfo);
                    }
                }
            }
            foreach (UIUpdateInfo uiUpdateInfo in Recommended_UpdateList_UI)
            {
                if (uiUpdateInfo.IsCheckUpdate)
                {
                    fwUpdateInfos.Add(uiUpdateInfo.FWUpdateInfo);
                }
            }
            foreach (UIUpdateInfo uiUpdateInfo in Optional_UpdateList_UI)
            {
                if (uiUpdateInfo.IsCheckUpdate)
                {
                    fwUpdateInfos.Add(uiUpdateInfo.FWUpdateInfo);
                }
            }
            FWUpdateInfoPackage.FWUpdateInfo.Clear();
            FWUpdateInfoPackage.FWUpdateInfo = fwUpdateInfos;
            SWUpdateInfoPackage.SWUpdateInfo.Clear();
            SWUpdateInfoPackage.SWUpdateInfo = swUpdateInfos;
            Log?.Info($"SetVMUpdate done");
        }
        #endregion
        #region Lock/Unlock
        #region General
        private bool _Lock_GeneralPage;
        public bool Lock_GeneralPage
        {
            get
            {
                return _Lock_GeneralPage;
            }
            set
            {
                _Lock_GeneralPage = value;
                OnPropertyChanged("GeneralUI_IsTabStoppable");
                OnPropertyChanged("GeneralUI_Opacity");
                OnPropertyChanged("GeneralUI_LockTooltip");
                OnPropertyChanged("GeneralPageLock_Opacity");
            }
        }
        public string GeneralPageLock_Opacity
        {
            get
            {
                return _Lock_GeneralPage ? "0.5" : "1.0";
            }
        }
        public bool GeneralUI_IsTabStoppable
        {
            get
            {
                return _Lock_GeneralPage ? false : true;
            }
        }
        public string GeneralUI_Opacity
        {
            get
            {
                return _Lock_GeneralPage ? "0.5" : "1.0";
            }
        }
        public Visibility GeneralUI_LockTooltip
        {
            get
            {
                return _Lock_GeneralPage ? Visibility.Visible : Visibility.Collapsed;
            }
        }
        #endregion
        #region UpdatePage
        private bool _Lock_UpdatesPage;
        public bool Lock_UpdatesPage
        {
            get
            {
                return _Lock_UpdatesPage;
            }
            set
            {
                _Lock_UpdatesPage = value;
                OnPropertyChanged("UpdatesPageUI_IsEnable");
                OnPropertyChanged("UpdatesPageUI_Opacity");
                OnPropertyChanged("UpdatesPageUI_LockTooltip");
                OnPropertyChanged("IsAnyUpdate");
            }
        }
        public bool UpdatesPageUI_IsEnable
        {
            get
            {
                return _Lock_UpdatesPage ? false : true;
            }
        }
        public string UpdatesPageUI_Opacity
        {
            get
            {
                return _Lock_UpdatesPage ? "0.5" : "1.0";
            }
        }
        public Visibility UpdatesPageUI_LockTooltip
        {
            get
            {
                return _Lock_UpdatesPage ? Visibility.Visible : Visibility.Collapsed;
            }
        }
        #endregion
        #region AnalyticsPage
        private bool _Lock_AnalyticsPage;
        public bool Lock_AnalyticsPage
        {
            get
            {
                return _Lock_AnalyticsPage;
            }
            set
            {
                _Lock_AnalyticsPage = value;
                OnPropertyChanged("AnalyticsPage_IsEnable");
                OnPropertyChanged("AnalyticsPage_Opacity");
                OnPropertyChanged("AnalyticsPage_LockTooltip");
            }
        }
        public bool AnalyticsPage_IsEnable
        {
            get
            {
                return _Lock_AnalyticsPage ? false : true;
            }
        }
        public string AnalyticsPage_Opacity
        {
            get
            {
                return _Lock_AnalyticsPage ? "0.5" : "1.0";
            }
        }
        public Visibility AnalyticsPage_LockTooltip
        {
            get
            {
                return _Lock_AnalyticsPage ? Visibility.Visible : Visibility.Collapsed;
            }
        }
        #endregion
        #endregion
        public void RefreshUI()
        {
            OnPropertyChanged("Lock_GeneralPage");
            OnPropertyChanged("UpdatesPageUI_IsEnable");
            OnPropertyChanged("Critical_UpdateList_UI");
            OnPropertyChanged("Recommended_UpdateList_UI");
            OnPropertyChanged("Optional_UpdateList_UI");
            OnPropertyChanged("LastCheckDate");
            OnPropertyChanged("UpdatesPageUI_Enable");
            OnPropertyChanged("NoUpdateAlert");
            OnPropertyChanged("NoNetwork");
            OnPropertyChanged("Critical_UpdateList");
            OnPropertyChanged("Recommended_UpdateList");
            OnPropertyChanged("Optional_UpdateList");
            OnPropertyChanged("IsAnyUpdate");
            OnPropertyChanged("LockMaskVisible");
            OnPropertyChanged("LockMaskVisible_Updates");
            OnPropertyChanged("GlobalSettingParam");
            OnPropertyChanged("EnableQuickAccessWidget_String");
            OnPropertyChanged("EnableQuickAccessWidget_Reminder_String");
            OnPropertyChanged("SWVersion");
            OnPropertyChanged("DriverVersion");
        }
        public void RefreshProcessUI()
        {
            OnPropertyChanged("UpdateTitle");
            OnPropertyChanged("UpdateVersion");
            OnPropertyChanged("ProgressValue");
            OnPropertyChanged("Progress_IsAnimated");
            OnPropertyChanged("ProgressStr");
        }
        /// <summary>
        /// When the device battery status changes, the Alert in the list is also updated
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="deviceChangedEventArgs"></param>
        /// Fix PIMS-337514 [DDPM Win 2.0][R19] Status of devices change but message in Update page not change immediately
        public void DeviceChanged(object? sender, DeviceChangedEventArgs deviceChangedEventArgs)
        {
            if (deviceChangedEventArgs.changedProperty == "BatteryStatusChanged" &&
                deviceChangedEventArgs.type == DeviceChangedType.Peripherals_SettingsChange)
            {
                Log?.Info($"DeviceChanged deviceChangedEventArgs.deviceID : {deviceChangedEventArgs.deviceID}");
                foreach (UIUpdateInfo uiUpdateInfo in Critical_UpdateList_UI)
                {
                    if (uiUpdateInfo.FWUpdateInfo != null &&
                        !string.IsNullOrEmpty(uiUpdateInfo.FWUpdateInfo.DeviceId) &&
                        uiUpdateInfo.FWUpdateInfo.DeviceId.Replace("{", "").Replace("}", "") == deviceChangedEventArgs.deviceID)
                    {
                        ChangeStatus(uiUpdateInfo, deviceChangedEventArgs);
                        uiUpdateInfo.Refresh();
                        OnPropertyChanged("Critical_UpdateList_UI");
                        return;
                    }
                }
                foreach (UIUpdateInfo uiUpdateInfo in Recommended_UpdateList_UI)
                {
                    if (uiUpdateInfo.FWUpdateInfo != null &&
                        !string.IsNullOrEmpty(uiUpdateInfo.FWUpdateInfo.DeviceId) &&
                        uiUpdateInfo.FWUpdateInfo.DeviceId.Replace("{", "").Replace("}", "") == deviceChangedEventArgs.deviceID)
                    {
                        ChangeStatus(uiUpdateInfo, deviceChangedEventArgs);
                        uiUpdateInfo.Refresh();
                        OnPropertyChanged("Recommended_UpdateList_UI");
                        return;
                    }
                }
                foreach (UIUpdateInfo uiUpdateInfo in Optional_UpdateList_UI)
                {
                    if (uiUpdateInfo.FWUpdateInfo != null &&
                        !string.IsNullOrEmpty(uiUpdateInfo.FWUpdateInfo.DeviceId) &&
                        uiUpdateInfo.FWUpdateInfo.DeviceId.Replace("{", "").Replace("}", "") == deviceChangedEventArgs.deviceID)
                    {
                        ChangeStatus(uiUpdateInfo, deviceChangedEventArgs);
                        uiUpdateInfo.Refresh();
                        OnPropertyChanged("Optional_UpdateList_UI");
                        return;
                    }
                }
            }
        }
        private void ChangeStatus(UIUpdateInfo uiUpdateInfo, DeviceChangedEventArgs deviceChangedEventArgs)
        {

            if (uiUpdateInfo != null)
            {
                uiUpdateInfo.UXAlertItemVisibility = Visibility.Collapsed;
                uiUpdateInfo.UXAlertItemMessage = "";
                uiUpdateInfo.UXAlertItemVisibility_2 = Visibility.Collapsed;
                uiUpdateInfo.UXAlertItemMessage_2 = "";
                bool? deviceBatteryLow = false;
                Log?.Info($"DeviceChanged ChangeStatus ModelNumber: {deviceChangedEventArgs.device_peripherals.ModelNumber}");
                Log?.Info($"DeviceChanged IsBatteryLevelSupported : {deviceChangedEventArgs.device_peripherals.IsBatteryLevelSupported}");
                if (deviceChangedEventArgs.device_peripherals.IsBatteryLevelSupported)
                {
                    Log?.Info($"DeviceChanged BatteryStatus : {deviceChangedEventArgs.device_peripherals.BatteryStatus}");
                    Log?.Info($"DeviceChanged BatteryLevel : {deviceChangedEventArgs.device_peripherals.BatteryLevel}");
                    if (deviceChangedEventArgs.device_peripherals.BatteryLevel <= 20 && deviceChangedEventArgs.device_peripherals.BatteryLevel >= 0)
                    {
                        deviceBatteryLow = true;
                    }
                    else if (deviceChangedEventArgs.device_peripherals.BatteryLevel < 0)
                    {
                        deviceBatteryLow = null;
                    }
                }
                switch (deviceChangedEventArgs.device_peripherals.Type)
                {
                    case DeviceType.LogicalMouse:

                        if (deviceBatteryLow == true)
                        {
                            uiUpdateInfo.UXAlertItemVisibility = Visibility.Visible;
                            uiUpdateInfo.UXAlertItemMessage = LangHelper.Instance["Update_BatteryLow_Alert"];
                        }
                        else if (deviceBatteryLow == null)
                        {
                            uiUpdateInfo.UXAlertItemVisibility = Visibility.Visible;
                            uiUpdateInfo.UXAlertItemMessage = LangHelper.Instance["Update_Mouse_Alert"];
                        }
                        break;

                    case DeviceType.LogicalKeyboard:
                        if (deviceBatteryLow == true)
                        {
                            uiUpdateInfo.UXAlertItemVisibility = Visibility.Visible;
                            uiUpdateInfo.UXAlertItemMessage = LangHelper.Instance["Update_BatteryLow_Alert"];
                        }
                        else if (deviceBatteryLow == null)
                        {
                            uiUpdateInfo.UXAlertItemVisibility = Visibility.Visible;
                            uiUpdateInfo.UXAlertItemMessage = LangHelper.Instance["Update_Mouse_Alert"];
                        }
                        break;
                    case DeviceType.PhysicalPen:
                    case DeviceType.LogicalPen:
                        if (deviceBatteryLow == true)
                        {
                            uiUpdateInfo.UXAlertItemVisibility = Visibility.Visible;
                            uiUpdateInfo.UXAlertItemMessage = LangHelper.Instance["Update_BatteryLow_Alert"];
                        }
                        break;
                    default:
                        uiUpdateInfo.UXAlertItemVisibility = Visibility.Collapsed;
                        uiUpdateInfo.UXAlertItemMessage = "";
                        uiUpdateInfo.UXAlertItemVisibility_2 = Visibility.Collapsed;
                        uiUpdateInfo.UXAlertItemMessage_2 = "";
                        break;
                }
            }
        }
    }
    public class UIUpdateInfo : INotifyPropertyChanged
    {
        public bool IsCritical { get; set; } = false;
        public bool IsRecommended { get; set; } = false;
        public bool IsCheckUpdate { get; set; }
        public bool IsEnableCheckBox { get; set; }
        public string UpdateInfo { get; set; }
        public FWUpdateInfo FWUpdateInfo { get; set; }
        public SWUpdateInfo SWUpdateInfo { get; set; }
        public Visibility UXAlertItemVisibility { get; set; }
        public string UXAlertItemMessage { get; set; }
        public Visibility UXAlertItemVisibility_2 { get; set; }
        public string UXAlertItemMessage_2 { get; set; }
        /// <summary>
        /// for display
        /// </summary>
        public Visibility UXAlertItemVisibility_3 { get; set; }
        public string UXAlertItemMessage_3 { get; set; }
        /// <summary>
        /// for Dock ARM
        /// </summary>
        public Visibility UXAlertItemVisibility_4 { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public UIUpdateInfo(FWUpdateInfo fwUpdateInfo, List<DeviceInfo> deviceInfos, int IODongle)
        {
            //0614 Bruce 將原本DeviceType型態是字串改成跟IL一樣這樣可以直接使用IL提供的矩陣做判斷
            DeviceType[] CriticalUpdates = new DeviceType[] { DeviceType.PhysicalAudioDongle, DeviceType.PhysicalDongle },
                     RecommendedUpdates = new DeviceType[] { DeviceType.LogicalMouse, DeviceType.LogicalKeyboard,
                         DeviceType.LogicalDock, DeviceType.PhysicalWiredDock,
                         DeviceType.PhysicalPen, DeviceType.PhysicalPen,
                         //DeviceType.LogicalWebcam, DeviceType.PhysicalWebcam,
                         DeviceType.PhysicalWiredAudio, DeviceType.LogicalWiredAudio,
                         DeviceType.LogicalHeadset, DeviceType.PhysicalBluetoothAudio };
            FWUpdateInfo = fwUpdateInfo;
            this.IsCheckUpdate = true;
            this.IsEnableCheckBox = true;
            bool? b = null;
            UXAlertItemVisibility = Visibility.Collapsed;
            UXAlertItemMessage = "";
            UXAlertItemVisibility_2 = Visibility.Collapsed;
            UXAlertItemMessage_2 = "";
            UXAlertItemVisibility_3 = Visibility.Collapsed;
            UXAlertItemMessage_3 = "";
            UXAlertItemVisibility_4 = Visibility.Collapsed;
            bool? deviceBatteryLow = false;
            bool isNoSupport = false;
            if (!fwUpdateInfo.IsDisplay)
            {
                if (deviceInfos != null)
                {
                    DeviceInfo? deviceInfo = deviceInfos.Find(o => o.ID.ToString().Equals(fwUpdateInfo.DeviceId.Replace("{", "").Replace("}", "")));
                    Debug.WriteLine($"deviceInfo is null : {(deviceInfo == null ? "Yes" : "No")}");
                    if (deviceInfo != null)
                    {
                        Debug.WriteLine($"deviceInfos.IsBatteryLevelSupported : {deviceInfo.IsBatteryLevelSupported}");
                        if (deviceInfo.IsBatteryLevelSupported)
                        {
                            Debug.WriteLine($"deviceInfos.BatteryStatus : {deviceInfo.BatteryStatus}");
                            Debug.WriteLine($"deviceInfos.BatteryLevel : {deviceInfo.BatteryLevel}");
                            if (deviceInfo.BatteryLevel <= 20 && deviceInfo.BatteryLevel >= 0)
                            {
                                deviceBatteryLow = true;
                            }
                            else if (deviceInfo.BatteryLevel < 0)
                            {
                                deviceBatteryLow = null;
                            }
                        }
                    }
                }
                switch (fwUpdateInfo.DeviceType)
                {
                    case DeviceType.PhysicalDongle:
                        if (fwUpdateInfo.DeviceName.ToLower().Equals(GlobalDefinitions.Dongle_BeforeGen2_Name.ToLower()) && IODongle > 1)
                        {
                            UXAlertItemVisibility = Visibility.Visible;
                            UXAlertItemMessage = LangHelper.Instance["Update_Firmware_update_of_multiple_USB_wireless_receivers"];
                        }
                        break;
                    case DeviceType.LogicalMouse:

                        if (deviceBatteryLow == true)
                        {
                            UXAlertItemVisibility = Visibility.Visible;
                            UXAlertItemMessage = LangHelper.Instance["Update_BatteryLow_Alert"];
                        }
                        else if (deviceBatteryLow == null)
                        {
                            UXAlertItemVisibility = Visibility.Visible;
                            UXAlertItemMessage = LangHelper.Instance["Update_Mouse_Alert"];
                        }
                        break;

                    case DeviceType.LogicalKeyboard:
                        if (deviceBatteryLow == true)
                        {
                            UXAlertItemVisibility = Visibility.Visible;
                            UXAlertItemMessage = LangHelper.Instance["Update_BatteryLow_Alert"];
                        }
                        else if (deviceBatteryLow == null)
                        {
                            UXAlertItemVisibility = Visibility.Visible;
                            UXAlertItemMessage = LangHelper.Instance["Update_Mouse_Alert"];
                        }
                        break;

                    case DeviceType.LogicalDock:
                    case DeviceType.PhysicalWiredDock:
                        Method method = new Method();
                        if (method.GetSystemArchitecture().Equals("ARM"))
                        {
                            isNoSupport = true;
                            UXAlertItemVisibility_4 = Visibility.Visible;
                        }
                        else
                        {
                            UXAlertItemVisibility = Visibility.Visible;
                            UXAlertItemMessage = LangHelper.Instance["Update_Dock_Alert_2"];
                            using (BatteryInfo batteryInfo = new BatteryInfo())
                            {
                                batteryInfo.GetBatteryInfo(out var battery);
                                if (battery.BatteryLifePercent <= 10)
                                {
                                    UXAlertItemVisibility_2 = Visibility.Visible;
                                    UXAlertItemMessage_2 = LangHelper.Instance["Update_PCBatteryLow_Alert"];
                                }
                            }
                        }
                        method = null;
                        break;
                    case DeviceType.PhysicalPen:
                    case DeviceType.LogicalPen:
                        if (deviceBatteryLow == true)
                        {
                            UXAlertItemVisibility = Visibility.Visible;
                            UXAlertItemMessage = LangHelper.Instance["Update_BatteryLow_Alert"];
                        }
                        break;
                    case DeviceType.LogicalWebcam:
                    case DeviceType.PhysicalWebcam:
                        /*if (fwUpdateInfo.Model.Contains("7022"))
                        {
                            UXAlertItemVisibility = Visibility.Visible;
                            UXAlertItemMessage = LangHelper.Instance["Update_Webcam_Alert"];
                        }*/
                        break;
                    default:
                        UXAlertItemVisibility = Visibility.Collapsed;
                        UXAlertItemMessage = "";
                        UXAlertItemVisibility_2 = Visibility.Collapsed;
                        UXAlertItemMessage_2 = "";
                        break;
                }
            }
            else
            {
                UXAlertItemVisibility = Visibility.Collapsed;
                UXAlertItemMessage = "";
                UXAlertItemVisibility_2 = Visibility.Collapsed;
                UXAlertItemMessage_2 = "";
                UXAlertItemVisibility_3 = Visibility.Visible;
                UXAlertItemMessage_3 = LangHelper.Instance["Update_Display_Alert"];
            }
            foreach (DeviceType s in CriticalUpdates)
            {
                if (fwUpdateInfo.DeviceType.Equals(s))
                {
                    this.IsCritical = true;
                    b = true;
                    break;
                }
            }
            if (b == null)
            {
                foreach (DeviceType s in RecommendedUpdates)
                {
                    if (fwUpdateInfo.DeviceType.Equals(s))
                    {
                        this.IsRecommended = true;
                        b = false;
                        break;
                    }
                }
            }
            if (b == true)
            {
                this.IsEnableCheckBox = false;
            }
            else if (b == null)
            {
                this.IsCheckUpdate = false;
            }
            if (isNoSupport)
            {
                this.IsCheckUpdate = false;
                this.IsEnableCheckBox = false;
            }
            //PIMS-316061 display add service tag to recognize.
            //12/25 add Model
            if (fwUpdateInfo.IsDisplay)
            {
                UpdateInfo = $"{LangHelper.Instance["Firmware_update"]} {fwUpdateInfo.TheLatestVersion} - {fwUpdateInfo.DeviceName} {fwUpdateInfo.Model} ({LangHelper.Instance["ServiceTag"]} {fwUpdateInfo.ServiceTag})";
            }
            else
            {
                if (!fwUpdateInfo.DeviceName.Equals(fwUpdateInfo.Model))//Added by Bruce Dongle name and model are repeated. Added a new check to prevent duplication if they are the same
                {
                    UpdateInfo = $"{LangHelper.Instance["Firmware_update"]} {fwUpdateInfo.TheLatestVersion} - {fwUpdateInfo.DeviceName} {fwUpdateInfo.Model}";
                }
                else
                {
                    UpdateInfo = $"{LangHelper.Instance["Firmware_update"]} {fwUpdateInfo.TheLatestVersion} - {fwUpdateInfo.DeviceName}";
                }
            }
        }

        public UIUpdateInfo(SWUpdateInfo swUpdateInfo)
        {
            SWUpdateInfo = swUpdateInfo;
            this.IsCheckUpdate = true;
            this.IsEnableCheckBox = false;
            UXAlertItemVisibility = Visibility.Collapsed;
            UXAlertItemMessage = "";
            UXAlertItemVisibility_2 = Visibility.Collapsed;
            UXAlertItemMessage_2 = "";
            UXAlertItemVisibility_3 = Visibility.Collapsed;
            UXAlertItemMessage_3 = "";
            UXAlertItemVisibility_4 = Visibility.Collapsed;
            UpdateInfo = $"{LangHelper.Instance["Software_update"]} {swUpdateInfo.TheLatestVersion} - {swUpdateInfo.SoftwareName}";
        }
        public void Refresh()
        {
            OnPropertyChanged(nameof(IsCheckUpdate));
            OnPropertyChanged(nameof(IsEnableCheckBox));
            OnPropertyChanged(nameof(UpdateInfo));
            OnPropertyChanged(nameof(UXAlertItemVisibility));
            OnPropertyChanged(nameof(UXAlertItemMessage));
            OnPropertyChanged(nameof(UXAlertItemVisibility_2));
            OnPropertyChanged(nameof(UXAlertItemMessage_2));
            OnPropertyChanged(nameof(UXAlertItemVisibility_3));
            OnPropertyChanged(nameof(UXAlertItemMessage_3));
            OnPropertyChanged(nameof(UXAlertItemVisibility_4));
        }
    }
}