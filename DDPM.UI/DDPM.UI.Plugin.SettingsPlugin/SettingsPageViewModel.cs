using CommunityToolkit.Mvvm.ComponentModel;
using DDPM.SA.Common;
using DDPM.SA.Common.Settings;
using DDPM.UI.Common;
using DDPM.UI.Common.Interfaces;
using DDPM.UI.Common.Models;
using Dell.Client.Framework.Common;
using DPeMPublic.Common.Enums;
using Microsoft.Win32;
using System.ComponentModel;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Threading;
using VcpCore.Common;

namespace DDPM.UI.Plugin.SettingsPlugin
{
    public class SettingsPageViewModel : ObservableObject, ISettingsPageViewModel
    {
        private List<HomeDevice> _homeDevices = new List<HomeDevice>();
        public bool[] IsSelected { get; set; } = new bool[5];

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
        public void SetSelected(int index)
        {
            for (int j = 0; j < IsSelected.Length; j++)
            {
                IsSelected[j] = false;
            }
            IsSelected[index] = true;
            switch (index)
            {
                case 0:
                default:
                    Settings_General settings_General = new Settings_General();
                    OpenFullView(settings_General);
                    break;
                case 1:
                    UpdatesPage updatesPage = new UpdatesPage();
                    OpenFullView(updatesPage);
                    break;
                case 2:
                    FullView = new AnalyticsPage();
                    break;
                case 3:
                    Settings_WidgetSettings settings_WidgetSettings = new Settings_WidgetSettings();
                    OpenFullView(settings_WidgetSettings);
                    break;
                case 4:
                    Settings_About settings_About = new Settings_About();
                    OpenFullView(settings_About);
                    break;
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

        private void DoWork_RefreshData(object sender, DoWorkEventArgs e)
        {
            try
            {
                GlobalSettingParam = DdpmCommonHelper.DeviceManagerSA.GetGlobalSettingParam().Result;
                DDPMSettings data = DdpmCommonHelper.ReadDDPMSettings();//DeviceManagerSA.ReloadAppConfigData().Result;
                Lock_AnalyticsPage = data.LockSettings.Lock_Settings_TelemetryConsent;
                Trace.WriteLine($"[SettingsPage] Apply TelemetryConsent(check) : {data.LockSettings.Lock_Settings_TelemetryConsent}");
                Lock_UpdatesPage = data.LockSettings.Lock_Settings_Updates;
                Trace.WriteLine($"[SettingsPage] Apply FW/SW Updates(check) : {data.LockSettings.Lock_Settings_Updates}");
                Lock_GeneralPage = data.LockSettings.Lock_Setting_ScreenNotification;
                Trace.WriteLine($"[SettingsPage] Apply General(check) : {data.LockSettings.Lock_Setting_ScreenNotification}");
                SetUpdateInfoUI(DdpmCommonHelper.DeviceManagerSA.GetFWUpdateInfo(false).Result, DdpmCommonHelper.DeviceManagerSA.SW_GetSWUpdateInfo(false).Result);
                RefreshUI();
            }
            catch (Exception)
            {
            }
        }

        private void Set_Page_Done(object sender, RunWorkerCompletedEventArgs e)
        {
            IsBusy = false;
            OnPropertyChanged("IsBusy");
        }
        #region General
        public GlobalSettingParam GlobalSettingParam { get; set; }
        public string EnableQuickAccessWidget_String
        {
            get
            {
                //avoid null
                if (GlobalSettingParam == null || GlobalSettingParam.GlobalSetting_WidgetSettings == null)
                    return "OFF";
                if (GlobalSettingParam.GlobalSetting_WidgetSettings.EnableQuickAccessWidget)
                {
                    return "ON";
                }
                return "OFF";
            }
        }
        public string EnableQuickAccessWidget_Reminder_String
        {
            get
            {
                //avoid null
                if (GlobalSettingParam == null || GlobalSettingParam.GlobalSetting_WidgetSettings == null)
                    return "OFF";
                if (GlobalSettingParam.GlobalSetting_WidgetSettings.EnableQuickAccessWidget_Reminder)
                {
                    return "ON";
                }
                return "OFF";
            }
        }
        public string SWVersion
        {
            get
            {
                return $"Software version: {GlobalSettingParam.GlobalSetting_About.SWVersion}";
            }
        }
        public string DriverVersion
        {
            get
            {
                return $"Driver version: {GlobalSettingParam.GlobalSetting_About.DriverVersion}";
            }
        }
        public void SaveMonitorAssetReport(string filePath)
        {
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
        }
        public void SaveDiagnosticReport(string filePath)
        {
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
        }
        private void Set_SaveMonitorAssetReport_Dowork(object sender, DoWorkEventArgs e)
        {
            string filePath = e.Argument.ToString();
            List<MonitorInfo> monitorInfos = DdpmCommonHelper.DeviceManagerSA.GetMonitors().Result;
            bool monitorAssetReports = DdpmCommonHelper.DeviceManagerSA.ExportMonitorAssetReport(monitorInfos, filePath).Result;
        }
        private void Set_SaveDiagnosticReport_Dowork(object sender, DoWorkEventArgs e)
        {
            string filePath = e.Argument.ToString();
            bool monitorAssetReports = DdpmCommonHelper.DeviceManagerSA.SaveLogFile(filePath).Result;
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

        public Visibility NoUpdateAlert
        {
            get => (Critical_UpdateList_UI?.Count <= 0 &&
                Recommended_UpdateList_UI?.Count <= 0 &&
                Optional_UpdateList_UI?.Count <= 0) ? Visibility.Visible : Visibility.Collapsed;
        }

        public Visibility NoNetwork { get; set; } = Visibility.Collapsed;
        public Visibility Critical_UpdateList { get => Critical_UpdateList_UI?.Count >= 1 ? Visibility.Visible : Visibility.Collapsed; }
        public Visibility Recommended_UpdateList { get => Recommended_UpdateList_UI?.Count >= 1 ? Visibility.Visible : Visibility.Collapsed; }
        public Visibility Optional_UpdateList { get => Optional_UpdateList_UI?.Count >= 1 ? Visibility.Visible : Visibility.Collapsed; }

        public Visibility IsAnyUpdate
        {
            get => (Critical_UpdateList_UI?.Count >= 1 ||
                Recommended_UpdateList_UI?.Count >= 1 ||
                Optional_UpdateList_UI?.Count >= 1) ? Visibility.Visible : Visibility.Collapsed;
        }
        public void CheckUpdate()
        {
            BackgroundWorker bw = new BackgroundWorker()
            {
                WorkerReportsProgress = false,
                WorkerSupportsCancellation = false
            };
            bw.DoWork += Set_CheckUpdate_Dowork;
            bw.RunWorkerCompleted += Set_Page_Done;
            bw.RunWorkerAsync();
            IsBusy = true;
            OnPropertyChanged("IsBusy");
        }
        private void Set_CheckUpdate_Dowork(object sender, DoWorkEventArgs e)
        {
            SetUpdateInfoUI(DdpmCommonHelper.DeviceManagerSA.GetFWUpdateInfo(false).Result, DdpmCommonHelper.DeviceManagerSA.SW_GetSWUpdateInfo(false).Result);
            RefreshUI();
        }
        public void SetUpdateInfoUI(FWUpdateInfoPackage fwUpdateInfoPackage, SWUpdateInfoPackage swUpdateInfoPackage)
        {
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
                foreach (FWUpdateInfo fwUpdateInfo in fwUpdateInfoPackage.FWUpdateInfo)
                {
                    UIUpdateInfo uiUpdateInfo = new UIUpdateInfo(fwUpdateInfo);
                    if ((!uiUpdateInfo.IsEnableCheckBox) && uiUpdateInfo.IsCheckUpdate)//如果不能選擇是否更新為強制更新
                    {
                        Critical_UpdateList_UI.Add(uiUpdateInfo);
                    }
                    else if (uiUpdateInfo.IsCheckUpdate)//如果為true為建議更新
                    {
                        Recommended_UpdateList_UI.Add(uiUpdateInfo);
                    }
                    else//剩下的為選用更新
                    {
                        Optional_UpdateList_UI.Add(uiUpdateInfo);
                    }
                }
                foreach (SWUpdateInfo swUpdateInfo in swUpdateInfoPackage.SWUpdateInfo)
                {
                    UIUpdateInfo uiUpdateInfo = new UIUpdateInfo(swUpdateInfo);
                    if ((!uiUpdateInfo.IsEnableCheckBox) && uiUpdateInfo.IsCheckUpdate)
                    {
                        Critical_UpdateList_UI.Add(uiUpdateInfo);
                    }
                }
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
    }
    public class UIUpdateInfo
    {
        public bool IsCheckUpdate { get; set; }
        public bool IsEnableCheckBox { get; set; }
        public string UpdateInfo { get; set; }
        public FWUpdateInfo FWUpdateInfo { get; set; }
        public SWUpdateInfo SWUpdateInfo { get; set; }
        public Visibility UXAlertItemVisibility { get; set; }
        public string UXAlertItemMessage { get; set; }
        public Visibility UXAlertItemVisibility_2 { get; set; }
        public string UXAlertItemMessage_2 { get; set; }

        public UIUpdateInfo(FWUpdateInfo fwUpdateInfo)
        {
            //0614 Bruce 將原本DeviceType型態是字串改成跟IL一樣這樣可以直接使用IL提供的矩陣做判斷
            DeviceType[] CriticalUpdates = new DeviceType[] { DeviceType.PhysicalAudioDongle, DeviceType.PhysicalDongle },
                     RecommendedUpdates = new DeviceType[] { DeviceType.LogicalMouse, DeviceType.LogicalKeyboard,
                         DeviceType.LogicalDock, DeviceType.PhysicalWiredDock,
                         DeviceType.PhysicalPen, DeviceType.PhysicalPen,
                         DeviceType.LogicalWebcam, DeviceType.PhysicalWebcam,
                         DeviceType.PhysicalWiredAudio, DeviceType.LogicalWiredAudio,
                         DeviceType.LogicalHeadset, DeviceType.PhysicalBluetoothAudio };
            FWUpdateInfo = fwUpdateInfo;
            this.IsCheckUpdate = true;
            this.IsEnableCheckBox = true;
            bool? b = null;
            UXAlertItemVisibility = Visibility.Collapsed;
            UXAlertItemVisibility_2 = Visibility.Collapsed;
            switch (fwUpdateInfo.DeviceType)
            {
                case DeviceType.LogicalMouse:
                    UXAlertItemVisibility = Visibility.Visible;
                    UXAlertItemMessage = "Press a button or a key on the device to enable this update";
                    break;

                case DeviceType.LogicalKeyboard:
                    UXAlertItemVisibility = Visibility.Collapsed;
                    break;

                case DeviceType.LogicalDock:
                    UXAlertItemVisibility = Visibility.Visible;
                    UXAlertItemMessage = "Ensure only one dock is connected to your system. Devices connected to dock may not be available during update.";
                    UXAlertItemVisibility_2 = Visibility.Visible;
                    UXAlertItemMessage_2 = "Connect PC to power source and ensure PC battery charge is above 10% to continue with update";
                    break;

                case DeviceType.PhysicalPen:
                    UXAlertItemVisibility = Visibility.Visible;
                    UXAlertItemMessage = "Battery level on the device is low. Replace/recharge battery to enable this update.";
                    break;
                default:
                    UXAlertItemVisibility = Visibility.Collapsed;
                    UXAlertItemMessage = "";
                    break;
            }
            foreach (DeviceType s in CriticalUpdates)
            {
                if (fwUpdateInfo.DeviceType.Equals(s))
                {
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
            UpdateInfo = $"Firmware update {fwUpdateInfo.TheLatestVersion} - {fwUpdateInfo.DeviceName}";
        }

        public UIUpdateInfo(SWUpdateInfo swUpdateInfo)
        {
            SWUpdateInfo = swUpdateInfo;
            this.IsCheckUpdate = true;
            this.IsEnableCheckBox = false;
            UXAlertItemVisibility = Visibility.Collapsed;
            UXAlertItemVisibility_2 = Visibility.Collapsed;
            UpdateInfo = $"Software update {swUpdateInfo.TheLatestVersion} - {swUpdateInfo.SoftwareName}";
        }
    }
}