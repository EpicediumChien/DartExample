using CommunityToolkit.Mvvm.ComponentModel;
using DDPM.SA.Common;
using DDPM.SA.Common.Settings;
using DDPM.UI.Common;
using DDPM.UI.Common.Interfaces;
using DDPM.UI.Common.Models;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.UX.WPF.Controls;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Management;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Windows;
using System.Windows.Controls;
using VcpCore.Common;
using static System.Net.WebRequestMethods;

using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.IO.Compression;
using System.Security.Policy;
using System.Net.Http;
using static DDPM.UI.Module.Color.ColorViewModel;

using System.Security.Cryptography;
using Dell.Client.Framework.UX.WPF.Controls;
using System.Windows.Input;
using ABI.System;
using Microsoft.Win32;
//using RegistryUtils = DDPM.SA.Common.;
using System.Management;
using System.Windows.Media.Animation;
using MonitorProfile = DDPM.SA.Common.MonitorProfile;
using System.Runtime.CompilerServices;
using Dell.Client.Framework.Common;
using DDPM.UI.Common.Models;
using System.Reflection;
using System.Diagnostics;
using System;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Threading;
using System.Windows.Markup;
using Microsoft;


//using System.Management;
//using System.Runtime.CompilerServices;
//using System.Security.Cryptography.X509Certificates;

[assembly: InternalsVisibleTo("DDPM.UI.Module.Color.Tests")]
namespace DDPM.UI.Module.Color
{
    internal class ColorViewModel : ObservableObject, INotifyPropertyChanged
    {
        #region Log
        private ILog? _log;
        public ILog? Log { get; set; }

        #endregion Log

      
        // add jim 20240604
        //public RegistryMonitor_NightLight registryMonitor_NightLight = null;
        //public RegistryMonitor_ICC registryMonitor_ICC = null;     

        // jim mofidy 20240606
        public ManagementEventWatcher startWatcher;
        public ManagementEventWatcher endProcWatcher;

        public DDPM.SA.Common.IIC_Metadata _ICC_Metadata = new DDPM.SA.Common.IIC_Metadata();

        public IModuleOwner? ModuleOwner { get; set; }
        public ColorModule MyModule { get; set; }

        // jim modify 20240604
        //public List<string> SupportColorPresets { get; set; } = new List<string>();
        public List<string> SupportColorPresets { get; set; }

        // jim modify 20240604
        //public List<string> ColorPresets_ItemsCollection { get; set; } = new List<string>();
        public List<string> ColorPresets_ItemsCollection { get; set; }

        //private static List<ColorPresetSettings> AddAppist = new List<ColorPresetSettings>();
      
        public bool Is_Game_DeviceName { get; set; } = false;

        //List<string> HDR_ColorPresetNameList = new List<string>() { "Standard HDR", "Movie HDR", "Game HDR", "Vivid HDR", "Desktop", "Reference", "Multiscreen Match", "DisplayHDR", "HDR10", "HLG" };

        public bool SmartHDR_ON { get; set; } = false;

        private bool IsColorEnable = false;

        public bool Is_ColorPreset_ManualFirst = true;

        public bool ColorEnable
        {
            get
            {
                OnPropertyChanged(nameof(ColorOpacity));
                OnPropertyChanged(nameof(GreayoutAlart));
                return (IsColorEnable || !Is_Game_DeviceName);
            }
        }

        public string ColorOpacity
        {
            get
            {
                if (IsColorEnable || !Is_Game_DeviceName)
                {
                    return "1.0";
                }
                return "0.5";
            }
        }

        public Visibility GreayoutAlart
        {
            get
            {
                if (IsColorEnable || !Is_Game_DeviceName)
                {
                    return Visibility.Collapsed;
                }
                return Visibility.Visible;
            }
        }


        // 20240920 jim add
        private bool showLockMask = false;

        public bool ShowLockMask
        {
            get { return showLockMask; }
            set
            {
                showLockMask = value;
                LockMaskVisible = showLockMask ? Visibility.Visible : Visibility.Collapsed;
                OnPropertyChanged("ShowLockMask");
            }
        }

        private Visibility lockMaskVisible = Visibility.Collapsed;

        public Visibility LockMaskVisible
        {
            get { return lockMaskVisible; }
            set
            {
                lockMaskVisible = value;
                OnPropertyChanged("LockMaskVisible");
            }
        }

        private bool _isTabStoppable;

        public bool isTabStoppable
        {
            get { return _isTabStoppable; }
            set
            {
                _isTabStoppable = value;
                OnPropertyChanged("isTabStoppable");
            }
        }

        private string _TabNavigation = "Cycle";

        public string TabNavigation
        {
            get { return _TabNavigation; }
            set
            {
                _TabNavigation = value;
                OnPropertyChanged("TabNavigation");
            }
        }

        private Visibility isAdvanced_Settings;

        public Visibility IsisAdvanced_Settings
        {
            get { return isAdvanced_Settings; }
            set
            {
                isAdvanced_Settings = value;
                OnPropertyChanged("IsisAdvanced_Settings");
            }
        }

        private string nightlightstatus;

        public string NightlightStatus
        {
            get { return nightlightstatus; }
            set
            {
                nightlightstatus = value;
                OnPropertyChanged("NightlightStatus");
            }
        }

        public bool IsAutoColorPreset_Lock { get; set; }

        //
        //Dean 0612 add for ALS syncup
        //
        private int _colorPresetSelectedIndex = -1;//no choose

        public int ColorPresetSelectedIndex
        {
            get
            {
                return _colorPresetSelectedIndex;
            }
            set
            {
                int temp = _colorPresetSelectedIndex;
                if (CheckIfDisableALSFeature())
                {
                    _colorPresetSelectedIndex = value;
                    SetColorPresetBySelection();
                }
                else
                {
                    _colorPresetSelectedIndex = temp;
                }
                OnPropertyChanged("ColorPresetSelectedIndex");
            }
        }

        public ColorViewModel()
        {            
            DdpmCommonHelper.MyConsole.RegisterForEvent("DisplayHDRStatusChanged", OnHDRChangedEvent);
        }

        private void OnHDRChangedEvent(object? sender, EventManagerArgs e)
        {
            IsColorEnable = !(bool)e.Tag;
            OnPropertyChanged(nameof(ColorEnable));

            SmartHDR_ON = !IsColorEnable;
        }

        public void UpdateHDRStatus()
        {
            bool HDRStatus = DdpmCommonHelper.DeviceManagerSA.GetHDRStatus(MyModule.SelectedHomeDevice.MonitorInfo).Result;
            IsColorEnable = !HDRStatus;
            OnPropertyChanged(nameof(ColorEnable));

            SmartHDR_ON = HDRStatus;
        }


        //User to update selected index but do not trigger set VCP
        public void UpdateColorPresetSelectedIndex(int selIndex)
        {
            _colorPresetSelectedIndex = selIndex;
            OnPropertyChanged("ColorPresetSelectedIndex");
        }

        //
        //move function [cbManualPreset_SelectionChanged] from code behind to here as code inline binding as well
        private void SetColorPresetBySelection()
        {
            int idex = ColorPresetSelectedIndex;// cbManualPreset.SelectedIndex;

            int nSupportColorPresets_Count = SupportColorPresets.Count;

            if ( idex >= 0 && idex < nSupportColorPresets_Count)
            {
                DDPMSettings setting = DdpmCommonHelper.ReadDDPMSettings();//DeviceManagerSA.ReloadAppConfigData().Result;

                if (setting != null)
                {
                    //this.Dispatcher.Invoke((Action)(() =>
                    Task.Run(() =>
                    {
                        if (setting.UserSettings != null)
                        {
                            // 20240717 jim add
                            if (setting.UserSettings.IsSynchronizemonitor)
                            {
                                int count = VerifyDellMonitor_Count();

                                if (count == 1)
                                {
                                    DdpmCommonHelper.DeviceManagerSA?.WriteColorPreset(
                                      MyModule.SelectedHomeDevice?.MonitorInfo,
                                      SupportColorPresets[idex]);
                                }
                                else
                                {
                                    foreach (HomeDevice hd in DdpmCommonHelper.ModuleOwner.HomeDevices)
                                    {
                                        if (hd.MonitorInfo.IsDellMonitor)
                                            DdpmCommonHelper.DeviceManagerSA?.WriteColorPreset(hd.MonitorInfo, SupportColorPresets[idex], 0, null,false);
                                    }
                                }
                            }
                            else
                                //ColorViewModel vm = (ColorViewModel)DataContext;
                                // 20240619 jim modify
                                DdpmCommonHelper.DeviceManagerSA?.WriteColorPreset(
                                    MyModule.SelectedHomeDevice?.MonitorInfo,
                                    SupportColorPresets[idex]);
                        }
                        
                    });
                }               

                //this.Dispatcher.Invoke((Action)(() =>
                {
                    //ColorViewModel vm = (ColorViewModel)DataContext;

                    // if Auto-adjust the ICC color profile based on Color preset

                    if (_ICC_Metadata.Is_Support_ICC_DeviceName)
                    {
                        // add jim 20240604
                        if (DCM_Visibility == Visibility.Hidden)
                        {
                            if (ColorManagement_isChecked)
                            {
                                if (ICCprofile_based_Colorpreset_enable)
                                {
                                    // add jim 20240830
                                    DdpmCommonHelper.DeviceManagerSA?.WriteColorPreset(MyModule.SelectedHomeDevice?.MonitorInfo, SupportColorPresets[idex]);
                                    //DdpmCommonHelper.DeviceManagerSA?.SetMonitorProfile(MyModule.SelectedHomeDevice?.MonitorInfo, SupportColorPresets[idex]);
                                    //DdpmCommonHelper.DeviceManagerSA?.AutoColorManagementForMonitorConfig(MyModule.SelectedHomeDevice?.MonitorInfo,"BYMONITOR", SupportColorPresets[idex]);
                                }
                            }
                        }
                    }
                }
                //}));
            }

        }

        public bool ColorManagement_isChecked { get; set; } = false;
        public bool ICCprofile_based_Colorpreset_enable { get; set; } = false;

        private Visibility _DCM_Visibility = Visibility.Hidden;

        public Visibility DCM_Visibility
        {
            get
            {
                return _DCM_Visibility;
            }
            set
            {
                _DCM_Visibility = value;
                OnPropertyChanged("DCM_Visibility");
            }
        }

        private bool CheckIfDisableALSFeature()
        {
            ALSConfig cfg = DdpmCommonHelper.DeviceManagerSA?.GetALSFeatureValue(MyModule.SelectedHomeDevice?.MonitorInfo, ALSFeatureQueryType.All, 0).Result;
            if (cfg != null && cfg.isSupportALS > 0)
            {
                if (cfg.isAutoColorTemp)
                {
                    //Dean 0614 modify to meet figma
                    //if(MessageBox.Show("Auto Color Temperature is currently enabled. Do you wish to disable it to continue?", "Warning", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                    if (DdpmCommonHelper.DDPMMesssageBox("Warning", "Auto Color Temperature is currently enabled. Do you wish to disable it to continue?"))
                    {
                        //disable Auto color temp and return true to change color preset
                        cfg.isAutoColorTemp = false;
                        DdpmCommonHelper.DeviceManagerSA?.SetALSFeatureValue(
                            MyModule.SelectedHomeDevice?.MonitorInfo,
                            cfg, ALSFeatureQueryType.All, ""
                            );
                        return true;
                    }
                    else
                    {
                        //keep auto color temp on and return false that do not change color preset
                        return false;
                    }
                }
            }
            return true;
        }
        //Dean 0612 End
        //

        private ContentControl? _fullView;
        public ContentControl? FullView
        {
            get => _fullView;
            set => SetProperty(ref _fullView, value);
        }

        public void OpenFullView(ContentControl content)
        {
            FullView = content;
            FullView.Visibility = Visibility.Visible;
        }

        public void CloseFullView()
        {
            FullView = null;
        }

        public int get_index_of_json_config_for_cur_monitor(MonitorInfo mo)
        {
            int index = -1;

            if (Test_AddAppCollectionData.GetInstance()._monitorConfigs != null)
            {
                // chech if ModelName and SerialNumber is null
                if (Test_AddAppCollectionData.GetInstance()._monitorConfigs.Count > 0)
                {
                    for (int i = 0; i < Test_AddAppCollectionData.GetInstance()._monitorConfigs.Count; i++)
                    {
                        if (System.String.IsNullOrEmpty(Test_AddAppCollectionData.GetInstance()._monitorConfigs[i].ModelName))
                            return -1;

                        if (System.String.IsNullOrEmpty(Test_AddAppCollectionData.GetInstance()._monitorConfigs[i].SerialNumber))
                            return -1;
                    }
                }

                index = Test_AddAppCollectionData.GetInstance()._monitorConfigs.FindIndex(x =>
                                                      x.ModelName.Trim() == mo.edid.ModelName.Trim() &&
                                                      x.SerialNumber.Trim() == mo.edid.SerialNumber.Trim());

                if (index == -1)
                {
                    // chech if ModelName and ServiceTag is null
                    if (Test_AddAppCollectionData.GetInstance()._monitorConfigs.Count > 0)
                    {
                        for (int i = 0; i < Test_AddAppCollectionData.GetInstance()._monitorConfigs.Count; i++)
                        {
                            if (System.String.IsNullOrEmpty(Test_AddAppCollectionData.GetInstance()._monitorConfigs[i].ModelName))
                                return -1;

                            if (System.String.IsNullOrEmpty(Test_AddAppCollectionData.GetInstance()._monitorConfigs[i].ServiceTag))
                                return -1;
                        }
                    }

                    index = Test_AddAppCollectionData.GetInstance()._monitorConfigs.FindIndex(x =>
                                               x.ModelName.Trim() == mo.edid.ModelName.Trim() &&
                                               x.ServiceTag.Trim() == mo.edid.ServiceTag.Trim());
                }

                //int index = Test_AddAppCollectionData.GetInstance()._monitorConfigs.FindIndex(x =>
                //x.ModelName.Trim() == mo.edid.ModelName.Trim() &&
                //x.SerialNumber.Trim() == mo.edid.SerialNumber.Trim());
            }
            return index;
        }

        public ColorPresetSettings get_cur_monitor_preset_config(MonitorInfo mo, List<ColorPresetSettings> config)
        {
            // Jim, 20240819
            if (Test_AddAppCollectionData.GetInstance()._monitorConfigs != null)
                Test_AddAppCollectionData.GetInstance()._monitorConfigs.Clear();
            
            Test_AddAppCollectionData.GetInstance()._monitorConfigs = config;

            int index = get_index_of_json_config_for_cur_monitor(mo);

            if (index < 0)
            {
                Test_AddAppCollectionData.GetInstance()._monitorConfigs.Add(new ColorPresetSettings()
                {
                    ModelName = mo.edid.ModelName,
                    SerialNumber = mo.edid.SerialNumber,
                    ServiceTag = mo.edid.ServiceTag,
                    RunType = (int)ColorPresetRunType.Manual,
                    AppInfo = new Dictionary<string, ColorPresetSettings_AppInfo>(),
                    //PresetForManual = "Standard/Native",
                    ColorForManual = 0,
                    ColorManagement_Status = (int)ColorManagementStatus.Off,
                    ColorManagement_RunType = (int)ColorManagementRunType.Off
                });

                index = get_index_of_json_config_for_cur_monitor(mo);


                if (index >= 0)
                {
                    Test_AddAppCollectionData.GetInstance()._monitorConfigs[index].AppInfo.Add("Desktop Application", new ColorPresetSettings_AppInfo()
                    {
                        //ColorPresetName = "Standard/Native",
                        Color = 0,
                        HDRColor = -1,
                        IconName = "Assets/palette.png",
                    });

                    Test_AddAppCollectionData.GetInstance()._monitorConfigs[index].AppInfo.Add("UWP Application", new ColorPresetSettings_AppInfo()
                    {
                        //ColorPresetName = "Standard/Native",
                        Color = 0,
                        HDRColor = -1,
                        IconName = "Assets/palette.png",
                    });
                }
               
            }

            if (index < 0 || Test_AddAppCollectionData.GetInstance()._monitorConfigs == null || index >= Test_AddAppCollectionData.GetInstance()._monitorConfigs.Count)
                return null;
            else
                return Test_AddAppCollectionData.GetInstance()._monitorConfigs[index];
        }

        public void Invoke_RefreshData()
        {
            BackgroundWorker bw = new BackgroundWorker()
            {
                WorkerReportsProgress = false,
                WorkerSupportsCancellation = false
            };
            bw.DoWork += DoWork_RefreshData;
            bw.RunWorkerCompleted += RunWorkerCompleted_RefreshData;
            Log?.Info("RunWorkerCompleted_RefreshData start...");
            bw.RunWorkerAsync();
            IsBusy = true;
        }

        public void Invoke_DownloadICCData()
        {
            BackgroundWorker bw_icc = new BackgroundWorker()
            {
                WorkerReportsProgress = false,
                WorkerSupportsCancellation = false
            };
            bw_icc.DoWork += DoWork_DownloadICCData;
            bw_icc.RunWorkerCompleted += RunWorkerCompleted_DownloadICCData;
            Log?.Info("RunWorkerCompleted_DownloadICCData start...");
            //IsBusy = true;
            bw_icc.RunWorkerAsync();
        }

        // add jim 20240604
        public void WatchForProcessStart()
        {
            string queryString =
                "SELECT TargetInstance" +
                "  FROM __InstanceCreationEvent " +
                "WITHIN  .025 " +
                " WHERE TargetInstance ISA 'Win32_Process' "
                + "   AND TargetInstance.Name like '%'";

            // The dot in the scope means use the current machine
            string scope = @"\\.\root\CIMV2";

            // Create a watcher and listen for events
            startWatcher = new ManagementEventWatcher(scope, queryString);
            startWatcher.EventArrived += startWatcher_EventArrived;
            startWatcher.Start();
        }

        // add jim 20240606
        public void WatchForProcessStart_Stop()
        {
            string queryString =
                "SELECT TargetInstance" +
                "  FROM __InstanceCreationEvent " +
                "WITHIN  .025 " +
                " WHERE TargetInstance ISA 'Win32_Process' "
                + "   AND TargetInstance.Name like '%'";

            // The dot in the scope means use the current machine
            string scope = @"\\.\root\CIMV2";

            // Create a watcher and listen for events
            //startWatcher = new ManagementEventWatcher(scope, queryString);

            //avoid exception
            if (startWatcher != null)
            {
                startWatcher.EventArrived -= startWatcher_EventArrived;
                startWatcher.Stop();
            }
        }

        // add jim 20240604
        void startWatcher_EventArrived(object sender, EventArrivedEventArgs e)
        {
            ManagementBaseObject targetInstance = (ManagementBaseObject)e.NewEvent.Properties["TargetInstance"].Value;
            string processName = targetInstance.Properties["Name"].Value.ToString();

            if (processName.Contains("ColorManagement"))
            {
                MyModule.GetRightView().Dispatcher.Invoke((Action)(() =>
                {
                    ((Expander)(MyModule.GetRightView().FindName("Expander_Advanced_Settings"))).IsEnabled = false;
                    ((Expander)(MyModule.GetRightView().FindName("Expander_Advanced_Settings"))).IsExpanded = false;

                    ((StackPanel)(MyModule.GetRightView().FindName("stackpanel_DCM"))).Visibility = Visibility.Visible;
                    DCM_Visibility = Visibility.Visible;

                    /*
                    if (registryMonitor_ICC != null)
                    {
                        if (registryMonitor_ICC.IsMonitoring)
                            registryMonitor_ICC.Dispose();
                        registryMonitor_ICC = null;
                    }
                    */
                }));
            }
        }

        // jim modify 20240606
        public void WatchForProcessEnd()
        {
            string queryString =
                "SELECT TargetInstance" +
                "  FROM __InstanceDeletionEvent " +
                "WITHIN  .025 " +
                " WHERE TargetInstance ISA 'Win32_Process' "
                + "   AND TargetInstance.Name like '%'";

            string scope = @"\\.\root\CIMV2";

            // Create a watcher and listen for events
            endProcWatcher = new ManagementEventWatcher(scope, queryString);
            endProcWatcher.EventArrived += ProcessEnded;
            endProcWatcher.Start();
        }

        // jim modify 20240606
        public void WatchForProcessEnd_Stop()
        {
            string queryString =
                "SELECT TargetInstance" +
                "  FROM __InstanceDeletionEvent " +
                "WITHIN  .025 " +
                " WHERE TargetInstance ISA 'Win32_Process' "
                + "   AND TargetInstance.Name like '%'";

            string scope = @"\\.\root\CIMV2";

            // Create a watcher and listen for events
            //endProcWatcher = new ManagementEventWatcher(scope, queryString);

            if (endProcWatcher != null)
            {
                endProcWatcher.EventArrived -= ProcessEnded;
                endProcWatcher.Stop();
            }
        }

        //  jim  add - modify  20240604
        private void ProcessEnded(object sender, EventArrivedEventArgs e)
        {
            ManagementBaseObject targetInstance = (ManagementBaseObject)e.NewEvent.Properties["TargetInstance"].Value;
            string processName = targetInstance.Properties["Name"].Value.ToString();

            if (processName.Contains("ColorManagement"))
            {
                MyModule.GetRightView().Dispatcher.Invoke((Action)(() =>
                {
                    ((Expander)(MyModule.GetRightView().FindName("Expander_Advanced_Settings"))).IsEnabled = true;
                    ((StackPanel)(MyModule.GetRightView().FindName("stackpanel_DCM"))).Visibility = Visibility.Hidden;
                    DCM_Visibility = Visibility.Hidden;

                    if (((UXToggleSwitch)(MyModule.GetRightView().FindName("ColorManagement_ToggleSwitch"))).IsChecked == true)
                    {
                        if (((UXRadioButton)(MyModule.GetRightView().FindName("rb_Colorpreset_based_ICCprofile"))).IsChecked == true)
                        {
                            /*
                            if (registryMonitor_ICC == null)
                            {
                                string keyName = string.Format("{0}\\{1}", "HKEY_CURRENT_USER", @"Software\Microsoft\Windows NT\CurrentVersion\ICM\ProfileAssociations\Display\{4d36e96e-e325-11ce-bfc1-08002be10318}");

                                registryMonitor_ICC = new RegistryMonitor_ICC(keyName);
                                registryMonitor_ICC.RegChanged += new EventHandler(OnRegChanged_ICC);
                                registryMonitor_ICC.Error += new System.IO.ErrorEventHandler(OnError_ICC);
                                registryMonitor_ICC.Start();
                            }
                            */
                        }
                    }
                }));
            }
        }

        //If caller is not the same as UI main thread, please using Dispatcher to execute it
        private void PerformLockUnlockUIAction(bool isColorLocked, bool isAutoBriTempLocked)
        {
            ALSConfig cfg = DdpmCommonHelper.DeviceManagerSA?.GetALSFeatureValue(
                DdpmCommonHelper.ModuleOwner.SelectedHomeDevice.MonitorInfo,
                ALSFeatureQueryType.All, 0).Result;
            //Lock functionality:
            //Locking InAppAutoBriTemp' with "Auto Color Temperature" as "on" should lock 
            // 1) "Auto Color Temperature", 
            // 2) Manual color controls, Auto color controls, Color management found under the "Color" Tab,
            // 3) the conditions listed in the first 3 bullet points
            if (cfg != null && cfg.isAutoColorTemp && isAutoBriTempLocked)
            {
                isColorLocked = true;
            }

            ShowLockMask = isColorLocked;
            isTabStoppable = !isColorLocked;

            if (ShowLockMask)
                TabNavigation = "None";
            else
                TabNavigation = "Cycle";

            LockMaskVisible = isColorLocked ? Visibility.Visible : Visibility.Collapsed;

        }

        private void DoWork_RefreshData(object sender, DoWorkEventArgs e)
        {
            try //2024-06-19 Elie, add try catch to get exception.
            {
                DDPMSettings data = DdpmCommonHelper.ReadDDPMSettings();//DeviceManagerSA.ReloadAppConfigData().Result;

                // if data = null, represents read setting file (ColorSetting.json) has something went wrong 

                //if (data == null)
                //    return;
                //if (data.LockSettings == null)
                //    return;
                
                if (data != null)
                    PerformLockUnlockUIAction(data.LockSettings.Lock_Display_ColorPreset, data.LockSettings.Lock_Display_AutoBriTemp);

                UpdateHDRStatus();

                //if (MyModule.SelectedHomeDevice.MonitorInfo.modelName.StartsWith("AW") || MyModule.SelectedHomeDevice.MonitorInfo.modelName.StartsWith("G"))
                //    Is_Game_DeviceName = true;

                //OSD control back event
                DdpmCommonHelper.DeviceManagerSA.VCPchanged += OnVCPChangedEvent;

                DdpmCommonHelper.DeviceManagerSA.Coloreset_manual_ChangeEvent += OnColoresetManualChangeHandler;

                DdpmCommonHelper.DeviceManagerSA.NightLightStatus_ChangeEvent += OnNightLightStatusChangeHandler;

                // -- begin add jim 20240604
                //DdpmCommonHelper.DeviceManagerSA.SyncNightlightStatus();
                DdpmCommonHelper.DeviceManagerSA.CheckNightLightStatus();
                DdpmCommonHelper.DeviceManagerSA.CheckNightLightScheduler();
                //SyncNightlightStatus();

                // jim remove
                //WatchForProcessStart();
                //WatchForProcessEnd();

                // -- end

                // -- begin add jim 20240604
                SupportColorPresets = new List<string>();
                SupportColorPresets = DdpmCommonHelper.DeviceManagerSA.ReadColorPreset(MyModule.SelectedHomeDevice.MonitorInfo).Result;

                /*
                if (IsColorEnable) // HDR off
                {
                    Sync_SupportColorPresets();                 

                }
                else // // HDR on
                {
                    List<string> common_ColorPreset = SupportColorPresets.Intersect(HDR_ColorPresetNameList).ToList();                    

                    SupportColorPresets.Clear();

                    foreach (string info in common_ColorPreset)
                    {
                        SupportColorPresets.Add(new string(info));
                    }
                }
                */

                //Dean 0612 add
                string curPreset = DdpmCommonHelper.DeviceManagerSA?.ReadCurrentColorPreset(DdpmCommonHelper.ModuleOwner?.SelectedHomeDevice?.MonitorInfo).Result;
                string strSync_CurrentColorPreset = string.Empty;
                //strSync_CurrentColorPreset = Sync_CurrentColorPreset(curPreset);
                strSync_CurrentColorPreset = DdpmCommonHelper.DeviceManagerSA?.Sync_ColorPresetName(DdpmCommonHelper.ModuleOwner?.SelectedHomeDevice?.MonitorInfo, curPreset).Result;

                ColorPresets_ItemsCollection = new List<string>();

                MyModule.GetRightView().Dispatcher.Invoke((Action)(() =>
                {
                    foreach (string info in SupportColorPresets)
                    {
                        ColorPresets_ItemsCollection.Add(new string(info));
                    }

                    //Dean 0612 add
                    if (!string.IsNullOrEmpty(strSync_CurrentColorPreset))
                    {
                        int idx = ColorPresets_ItemsCollection.FindIndex(x => x.ToUpper().Equals(strSync_CurrentColorPreset.ToUpper()));
                        if (idx >= 0)
                        {
                            UpdateColorPresetSelectedIndex(idx);
                        }
                    }
                    //End
                }));
                // -- end

                //Robert_Lin, 20240528
                //OLD Code:
                //Test_AddAppCollectionData.GetInstance().AppsList.Clear();
                //NEW Code:

                //Jim, 20240819, Fixed for applist increase repeatedly when change a different Monitor.
                //Test_AddAppCollectionData.GetInstance().AppsList.Clear();

                List<AppData> tempList = new List<AppData>();

                ColorPresetSettings config = get_cur_monitor_preset_config(MyModule.SelectedHomeDevice.MonitorInfo, DdpmCommonHelper.DeviceManagerSA.ReadColorPresetSettings().Result);

                // Add to check if  config setting is null
                if (config != null)
                {
                    if (config.AppInfo.Count <= 0)
                    {
                        config.AppInfo.Add("Desktop Application", new ColorPresetSettings_AppInfo()
                        {
                            //ColorPresetName = "Standard/Native",
                            Color = 0,
                            HDRColor = -1,
                            IconName = "Assets/palette.png",
                        });
                        config.AppInfo.Add("UWP Application", new ColorPresetSettings_AppInfo()
                        {
                            //ColorPresetName = "Standard/Native",
                            Color = 0,
                            HDRColor = -1,
                            IconName = "Assets/palette.png",
                        });
                    }

                    DdpmCommonHelper.DeviceManagerSA.WriteColorPresetSettings(Test_AddAppCollectionData.GetInstance()._monitorConfigs);
                    Thread.Sleep(100);

                    foreach (string key in config.AppInfo.Keys)
                    {
                        ColorPresetSettings_AppInfo value = config.AppInfo[key];

                        string strColorPresetName = string.Empty;

                        if (SmartHDR_ON)
                            strColorPresetName = DdpmCommonHelper.DeviceManagerSA.GetColorPresetName(value.HDRColor).Result;
                        else
                            strColorPresetName = DdpmCommonHelper.DeviceManagerSA.GetColorPresetName(value.Color).Result;

                        //string strSync_ColorPresetName = string.Empty;
                        //strSync_ColorPresetName = Sync_CurrentColorPreset(strColorPresetName);
                        //strSync_CurrentColorPreset = DdpmCommonHelper.DeviceManagerSA?.Sync_ColorPresetName(DdpmCommonHelper.ModuleOwner?.SelectedHomeDevice?.MonitorInfo, curPreset).Result;
                        if (DdpmCommonHelper.ModuleOwner?.SelectedHomeDevice?.MonitorInfo != null)
                            strSync_CurrentColorPreset = DdpmCommonHelper.DeviceManagerSA?.Sync_ColorPresetName(DdpmCommonHelper.ModuleOwner?.SelectedHomeDevice?.MonitorInfo, strColorPresetName).Result;

                        //int pIdx = SupportColorPresets.FindIndex(x =>
                        //                    x.Trim() == value.ColorPresetName.Trim());

                        //int pIdx = SupportColorPresets.FindIndex(x =>
                        //                    x.Trim() == strSync_ColorPresetName.Trim());

                        int pIdx = SupportColorPresets.FindIndex(x =>
                                            x.Trim() == strSync_CurrentColorPreset.Trim());


                        Visibility vis = (key.Trim() == "Desktop Application" || key.Trim() == "UWP Application") ?
                            Visibility.Collapsed : Visibility.Visible;

                        if (pIdx <= 0)
                            pIdx = 0;

                        if ((key.Trim() == "Desktop Application" || key.Trim() == "UWP Application"))
                        {
                            //Test_AddAppCollectionData.GetInstance().AppsList.Add(new AppData
                            //  //Robert_Lin, 20240528
                            //OLD Code:
                            //Test_AddAppCollectionData.GetInstance().AppsList.Add(new AppData
                            //NEW Code:
                            tempList.Add(new AppData
                            {
                                AppName = key,
                                AppPresetIdx = pIdx,
                                IsDeleteAble = vis,
                                SupportPreset = SupportColorPresets,
                                AppIcon = "Assets/palette.png"
                            });
                        }
                        else
                        {
                            AppData new_Appdata = new AppData();
                            new_Appdata.AppName = key;
                            new_Appdata.AppPresetIdx = pIdx;
                            new_Appdata.IsDeleteAble = vis;
                            new_Appdata.SupportPreset = SupportColorPresets;

                            if (System.IO.File.Exists(value.IconName))
                            {
                                new_Appdata.AppIcon = value.IconName;
                            }
                            else
                            {
                                new_Appdata.AppIcon = "Assets/palette.png";
                            }
                            //Robert_Lin, 20240528
                            //OLD Code:
                            //Test_AddAppCollectionData.GetInstance().AppsList.Add(new_Appdata);
                            //NEW Code:
                            tempList.Add(new_Appdata);
                        }
                    }

                    //DdpmCommonHelper.DeviceManagerSA.SyncNightlightStatus();

                    //Robert_Lin, 20240528
                    //NEW Added code:
                    Test_AddAppCollectionData.GetInstance().AppsList = new ObservableCollection<AppData>(tempList);

                    AppsList = Test_AddAppCollectionData.GetInstance().AppsList;

                    _ICC_Metadata = DdpmCommonHelper.DeviceManagerSA?.DownloadICCData(DdpmCommonHelper.ModuleOwner?.SelectedHomeDevice?.MonitorInfo).Result;

                    MyModule.GetRightView().Dispatcher.Invoke((Action)(() =>
                    {                        
                        update_ui_over_runtype(config);                        
                    }));
                                 
                }

                // // if data = null, represents read setting file (ColorSetting.json) has something went wrong 
                if (data != null)
                {
                    LockMaskVisible = data.LockSettings.Lock_Display_ColorPreset ? Visibility.Visible : Visibility.Collapsed;
                    ShowLockMask = data.LockSettings.Lock_Display_ColorPreset;
                    isTabStoppable = !data.LockSettings.Lock_Display_ColorPreset;

                    //Read user default lock value, these values are synced from IT lock event          
                    Trace.WriteLine($"[SettingsPage] Color right page(Lock) : {data.LockSettings.Lock_Display_ColorPreset}");
                }

                // Update UI
                MyModule.GetRightView().Dispatcher.Invoke((Action)(() =>
                {
                    DdpmCommonHelper.DeviceManagerSA.SyncNightlightStatus();
                    //update_ui_over_runtype(config);
                    RefreshUI();
                }));

                //OnPropertyChanged("ColorPresets_ItemsCollection");
                //OnPropertyChanged("AppsList");
                //OnPropertyChanged("NightlightStatus");
                //OnPropertyChanged("IsisAdvanced_Settings");

                // remove jim 20240606
                //WatchForProcessStart();

                //WatchForProcessEnd();

                // ICC profiles mapping schema

                //_ICC_Metadata = DdpmCommonHelper.DeviceManagerSA?.DownloadICCData(DdpmCommonHelper.ModuleOwner?.SelectedHomeDevice?.MonitorInfo).Result;

                /*
                Visibility vis_ad;

                // 20240627 jim modify
                if (_ICC_Metadata != null)
                {
                    if (_ICC_Metadata.Is_Support_ICC_DeviceName)
                    {
                        vis_ad = Visibility.Visible;

                        
                        DDPMSettings setting = DdpmCommonHelper.DeviceManagerSA.ReloadAppConfigData().Result;

                        if (setting.UserSettings.ColorManagement_off)
                        {
                            ((UXToggleSwitch)(MyModule.GetRightView().FindName("ColorManagement_ToggleSwitch"))).IsChecked = false; 
                        }
                        else
                        {
                            ((UXToggleSwitch)(MyModule.GetRightView().FindName("ColorManagement_ToggleSwitch"))).IsChecked = true;
                        }

                        if (setting.UserSettings.ColorManagement_bymonitor)
                        {
                            //((UXToggleSwitch)(MyModule.GetRightView().FindName("ColorManagement_ToggleSwitch"))).IsChecked = true;
                            ((UXRadioButton)(MyModule.GetRightView().FindName("rb_ICCprofile_based_Colorpreset"))).IsChecked = true;
                            ((UXRadioButton)(MyModule.GetRightView().FindName("rb_Colorpreset_based_ICCprofile"))).IsChecked = false;
                        }

                        if (setting.UserSettings.ColorManagement_byhost)
                        {
                            //((UXToggleSwitch)(MyModule.GetRightView().FindName("ColorManagement_ToggleSwitch"))).IsChecked = true;
                            ((UXRadioButton)(MyModule.GetRightView().FindName("rb_ICCprofile_based_Colorpreset"))).IsChecked = false;
                            ((UXRadioButton)(MyModule.GetRightView().FindName("rb_Colorpreset_based_ICCprofile"))).IsChecked = true;
                        }
                        


                    }
                    else
                        vis_ad = Visibility.Hidden;                  

                    IsisAdvanced_Settings = vis_ad;               
                }
                */

                //Lock/unlock mask and tabstop init here
                //DDPMSettings data = DdpmCommonHelper.ReadDDPMSettings();//DeviceManagerSA.ReloadAppConfigData().Result;//Be careful if spend much time here                
                //ex: vm.LockMaskVisible = data.LockSettings.Lock_Display_ColorPreset ? Visibility.Visible : Visibility.Collapsed;

                //LockMaskVisible = data.LockSettings.Lock_Display_ColorPreset ? Visibility.Visible : Visibility.Collapsed;
                //ShowLockMask = data.LockSettings.Lock_Display_ColorPreset;
                //isTabStoppable = !data.LockSettings.Lock_Display_ColorPreset;

                //Read user default lock value, these values are synced from IT lock event          
                //Trace.WriteLine($"[SettingsPage] Color right page(Lock) : {data.LockSettings.Lock_Display_ColorPreset}"); 
            }
            catch (System.Exception)
            {
            }
        }

        private void DoWork_DownloadICCData(object sender, DoWorkEventArgs e)
        {
            try
            {  
                ColorPresetSettings config = get_cur_monitor_preset_config(MyModule.SelectedHomeDevice.MonitorInfo, DdpmCommonHelper.DeviceManagerSA.ReadColorPresetSettings().Result);

                _ICC_Metadata = DdpmCommonHelper.DeviceManagerSA?.DownloadICCData(DdpmCommonHelper.ModuleOwner?.SelectedHomeDevice?.MonitorInfo).Result;

                MyModule.GetRightView().Dispatcher.Invoke((Action)(() =>
                {
                    update_ui_over_runtype(config);
                    RefreshUI();
                }));
            }
            catch (System.Exception)
            {
            }
        }

        //Jim 0816  
        /// <summary>
        /// Catch OSD menu event
        /// </summary>
        /// <param name="sender">object type</param>
        /// <param name="e">changed event</param>
        private void OnVCPChangedEvent(object? sender, VCPchangedEventArgs e)
        {   
            Trace.WriteLine($"VCPchangedEventArgs e.vcpcode = {e.vcpcode}");

            if (e.vcpcode.Equals("DC") || e.vcpcode.Equals("F0") || e.vcpcode.Equals("14") || e.vcpcode.Equals("E2") || e.vcpcode.Equals("F4")) // Color changes by OSD menu
            {
                
                string curPreset = DdpmCommonHelper.DeviceManagerSA?.ReadCurrentColorPreset(DdpmCommonHelper.ModuleOwner?.SelectedHomeDevice?.MonitorInfo).Result;
                string strSync_CurrentColorPreset = string.Empty;
                //strSync_CurrentColorPreset = Sync_CurrentColorPreset(curPreset);
                strSync_CurrentColorPreset = DdpmCommonHelper.DeviceManagerSA?.Sync_ColorPresetName(DdpmCommonHelper.ModuleOwner?.SelectedHomeDevice?.MonitorInfo, curPreset).Result;

                MyModule.GetRightView().Dispatcher.Invoke((Action)(() =>
                {  
                    if (!string.IsNullOrEmpty(strSync_CurrentColorPreset))
                    {
                        int idx = ColorPresets_ItemsCollection.FindIndex(x => x.ToUpper().Equals(strSync_CurrentColorPreset.ToUpper()));
                        if (idx >= 0)
                        {
                            UpdateColorPresetSelectedIndex(idx);
                        }
                    }
                    
                }));
            }         
        }

        ~ColorViewModel()
        {
            if (DdpmCommonHelper.DeviceManagerSA != null)
            {
                DdpmCommonHelper.DeviceManagerSA.VCPchanged -= OnVCPChangedEvent;
                DdpmCommonHelper.DeviceManagerSA.Coloreset_manual_ChangeEvent -= OnColoresetManualChangeHandler;
                DdpmCommonHelper.DeviceManagerSA.NightLightStatus_ChangeEvent -= OnNightLightStatusChangeHandler;
            }
        }

        private void OnColoresetManualChangeHandler(object sender, string e)
        {
            int index = 0;      

            MyModule.GetRightView().Dispatcher.Invoke((Action)(() =>
            {
                if (string.Equals(e, "Standard", StringComparison.OrdinalIgnoreCase) || string.Equals(e, "Native", StringComparison.OrdinalIgnoreCase))
                {
                    foreach (string colorpreset in SupportColorPresets)
                    {
                        if (string.Equals(colorpreset, "Standard", StringComparison.OrdinalIgnoreCase))
                            break;
                        index++;
                    }
                }
                else if (string.Equals(e, "Game", StringComparison.OrdinalIgnoreCase) || string.Equals(e, "Game1", StringComparison.OrdinalIgnoreCase))
                {
                    foreach (string colorpreset in SupportColorPresets)
                    {
                        if (string.Equals(colorpreset, "Game", StringComparison.OrdinalIgnoreCase))
                            break;
                        index++;
                    }
                }
                else if (e.Contains("Rec", StringComparison.OrdinalIgnoreCase) || e.Contains("BT.", StringComparison.OrdinalIgnoreCase) || e.Contains("709", StringComparison.OrdinalIgnoreCase) || e.Contains("2020", StringComparison.OrdinalIgnoreCase))
                {
                    foreach (string colorpreset in SupportColorPresets)
                    {
                        if (string.Equals(colorpreset, "Rec.709 / BT.709", StringComparison.OrdinalIgnoreCase))
                            break;
                        else if (string.Equals(colorpreset, "Rec.709", StringComparison.OrdinalIgnoreCase))
                            break;
                        else if (string.Equals(colorpreset, "BT.709", StringComparison.OrdinalIgnoreCase))
                            break;
                        else if (string.Equals(colorpreset, "Rec.2020 / BT.2020", StringComparison.OrdinalIgnoreCase))
                            break;
                        else if (string.Equals(colorpreset, "Rec.2020", StringComparison.OrdinalIgnoreCase))
                            break;
                        else if (string.Equals(colorpreset, "BT.2020", StringComparison.OrdinalIgnoreCase))
                            break;

                        index++;
                    }
                }
                else
                {
                    foreach (string colorpreset in SupportColorPresets)
                    {
                        if (string.Equals(colorpreset, e, StringComparison.OrdinalIgnoreCase))
                        {
                            break;
                        }
                        index++;
                    }
                }

                // jim modify 20240604
                ((ComboBox)(MyModule.GetRightView().FindName("cbManualPreset"))).SelectedIndex = index;                     

                RefreshUI();
            }));
        }

        private void OnNightLightStatusChangeHandler(object sender, string e)
        {
            NightlightStatus = e;

            MyModule.GetRightView().Dispatcher.Invoke((Action)(() =>
            { 
                RefreshUI();
            }));
        }

        private void RunWorkerCompleted_RefreshData(object sender, RunWorkerCompletedEventArgs e)
        {
            //Handling the result and final process

            IsBusy = false;

            //If BackgroundWorker. WorkerSupportsCancellation is true, and you set e.Cancel=true in DoWorker
            if (e.Cancelled)
            {
                Log?.Info("** RefreshData is cancelled.");
                return;
            }
            if (e.Error != null)
            {
                //The message is e.Error.Message
                Log?.Info($"** RefreshData stopped by an exception: {e.Error.Message}");
                return;
            }
            //
            if (e.Result == null)
            {
                //In case that you never set value to e-Result
                Log?.Info("** RefreshData abnormal stopped unknown reason.");
            }
            else
            {
                Log?.Info($"** RefreshData result: {e.Result}");

                if (e.Result == "OK")
                {
                    //Result is passed.
                }
                else
                {
                    //Result is failed.
                }
            }

            WatchForProcessStart();
            WatchForProcessEnd();

            //UpdateHDRStatus();
        }

        private void RunWorkerCompleted_DownloadICCData(object sender, RunWorkerCompletedEventArgs e)
        {
            //Handling the result and final process

            //IsBusy = false;

            //If BackgroundWorker. WorkerSupportsCancellation is true, and you set e.Cancel=true in DoWorker
            if (e.Cancelled)
            {
                Log?.Info("** DownloadICCData is cancelled.");
                return;
            }
            if (e.Error != null)
            {
                //The message is e.Error.Message
                Log?.Info($"** DownloadICCData stopped by an exception: {e.Error.Message}");
                return;
            }
            //
            if (e.Result == null)
            {
                //In case that you never set value to e-Result
                Log?.Info("** DownloadICCData abnormal stopped unknown reason.");
            }
            else
            {
                Log?.Info($"** DownloadICCData result: {e.Result}");

                if (e.Result == "OK")
                {
                    //Result is passed.
                }
                else
                {
                    //Result is failed.
                }
            }

            WatchForProcessStart();
            WatchForProcessEnd();

            //UpdateHDRStatus();
        }
        
        /*
        private void SyncNightlightStatus()
        {
            // 20240627 jim modify

            RegistryKey localKey64 = RegistryKey.OpenBaseKey(Microsoft.Win32.RegistryHive.CurrentUser, RegistryView.Registry64);

            //using RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\CloudStore\\Store\\DefaultAccount\\Current\\default$windows.data.bluelightreduction.bluelightreductionstate\\windows.data.bluelightreduction.bluelightreductionstate");

            if (localKey64 != null)
            {
                RegistryKey registryKey = localKey64.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\CloudStore\\Store\\DefaultAccount\\Current\\default$windows.data.bluelightreduction.bluelightreductionstate\\windows.data.bluelightreduction.bluelightreductionstate", false);
                if (registryKey != null)
                {
                    object obj = registryKey?.GetValue("Data");
                    if (obj != null)
                    {
                        byte[] array = (byte[])obj;

                        //bool nightLightIsOn = false;

                        if (array.Length >= 18)
                        {
                            int ch = array[18];

                            if (ch == 0x15)
                            {
                                //nightLightIsOn = true;

                                // jim modify 20240604
                                MyModule.GetRightView().Dispatcher.Invoke((Action)(() =>
                                {
                                    NightlightStatus = "On";
                                }));
                            }
                            else if (ch == 0x13)
                            {
                                //nightLightIsOn = false;

                                // jim modify 20240604
                                MyModule.GetRightView().Dispatcher.Invoke((Action)(() =>
                                {
                                    NightlightStatus = "Off";
                                }));
                            }
                        }
                    }
                    else
                    {
                        // jim add 20240711
                        MyModule.GetRightView().Dispatcher.Invoke((Action)(() =>
                        {
                            NightlightStatus = "Off";
                        }));
                    }
                }
                else
                {
                    // jim add 20240711
                    MyModule.GetRightView().Dispatcher.Invoke((Action)(() =>
                    {
                        NightlightStatus = "Off";
                    }));
                }
            }
            else
            {
                // jim add 20240711
                MyModule.GetRightView().Dispatcher.Invoke((Action)(() =>
                {
                    NightlightStatus = "Off";
                }));
            }
        }
        */

        /*
        // add jim 20240604
        public void StopRegistryMonitor()
        {
            if (registryMonitor_NightLight != null)
            {
                registryMonitor_NightLight.Stop();
                registryMonitor_NightLight.RegChanged -= new EventHandler(OnRegChanged_NightLight);
                registryMonitor_NightLight.Error -= new System.IO.ErrorEventHandler(OnError_NightLight);
                registryMonitor_NightLight = null;
            }     
        }

        // add jim 20240604
        public void OnRegChanged_NightLight(object sender, EventArgs e)
        {
            SyncNightlightStatus();
            return;
        }

        // add jim 20240604
        public void OnError_NightLight(object sender, ErrorEventArgs e)
        {
            StopRegistryMonitor();
        }   
        */
        private ObservableCollection<AppData> _appsList;

        public ObservableCollection<AppData> AppsList
        {
            get { return _appsList; }
            set
            {
                _appsList = value;
                OnPropertyChanged("AppsList");
            }
        }

        public void RefreshUI()
        {           
            OnPropertyChanged("ColorPresets_ItemsCollection");
            OnPropertyChanged("AppsList");
            OnPropertyChanged("NightlightStatus");
            OnPropertyChanged("IsisAdvanced_Settings");

            OnPropertyChanged("TabNavigation");
            OnPropertyChanged("isTabStoppable");
        }

        private void update_ui_over_runtype(ColorPresetSettings config)
        {
            if (!(IsColorEnable || !Is_Game_DeviceName))
            {
                DdpmCommonHelper.DeviceManagerSA.AutoSetColorPresetForMonitorConfig(DdpmCommonHelper.ModuleOwner.SelectedHomeDevice.MonitorInfo, "OFF", IsAutoColorPreset_Lock);
                return;
            }


            if (config.RunType == (int)ColorPresetRunType.Auto)
            {
                ((Expander)(MyModule.GetRightView().FindName("Expander_Manual"))).IsExpanded = false;
                ((Expander)(MyModule.GetRightView().FindName("Expander_Auto"))).IsExpanded = true;
                //DdpmCommonHelper.DeviceManagerSA.AutoSetColorPresetForMonitorConfig(DdpmCommonHelper.ModuleOwner.SelectedHomeDevice.MonitorInfo, "ON", IsAutoColorPreset_Lock);

            }
            else
            {
                ((Expander)(MyModule.GetRightView().FindName("Expander_Manual"))).IsExpanded = true;
                ((Expander)(MyModule.GetRightView().FindName("Expander_Auto"))).IsExpanded = false;
                //DdpmCommonHelper.DeviceManagerSA.AutoSetColorPresetForMonitorConfig(DdpmCommonHelper.ModuleOwner.SelectedHomeDevice.MonitorInfo, "OFF", IsAutoColorPreset_Lock);
            }

            Visibility vis_ad;

            // 20240627 jim modify
            if (_ICC_Metadata != null)
            {
                if (_ICC_Metadata.Is_Support_ICC_DeviceName)
                {
                    vis_ad = Visibility.Visible;                    

                    if (config.ColorManagement_Status == (int)ColorManagementStatus.Off)
                    {
                        ((UXToggleSwitch)(MyModule.GetRightView().FindName("ColorManagement_ToggleSwitch"))).IsChecked = false;
                        DdpmCommonHelper.DeviceManagerSA.AutoColorManagementForMonitorConfig(DdpmCommonHelper.ModuleOwner.SelectedHomeDevice.MonitorInfo, "OFF");

                        if (config.ColorManagement_RunType == (int)ColorManagementRunType.Bymonitor)
                        {
                            //((UXToggleSwitch)(MyModule.GetRightView().FindName("ColorManagement_ToggleSwitch"))).IsChecked = true;
                            ((UXRadioButton)(MyModule.GetRightView().FindName("rb_ICCprofile_based_Colorpreset"))).IsChecked = true;
                            ((UXRadioButton)(MyModule.GetRightView().FindName("rb_Colorpreset_based_ICCprofile"))).IsChecked = false;
                            ((UXRadioButton)(MyModule.GetRightView().FindName("rb_ICCprofile_based_Colorpreset"))).IsEnabled = false;
                            ((UXRadioButton)(MyModule.GetRightView().FindName("rb_Colorpreset_based_ICCprofile"))).IsEnabled = false;
                           
                        }
                        else if (config.ColorManagement_RunType == (int)ColorManagementRunType.Byhost)
                        {
                            //((UXToggleSwitch)(MyModule.GetRightView().FindName("ColorManagement_ToggleSwitch"))).IsChecked = true;
                            ((UXRadioButton)(MyModule.GetRightView().FindName("rb_ICCprofile_based_Colorpreset"))).IsChecked = false;
                            ((UXRadioButton)(MyModule.GetRightView().FindName("rb_Colorpreset_based_ICCprofile"))).IsChecked = true;
                            ((UXRadioButton)(MyModule.GetRightView().FindName("rb_ICCprofile_based_Colorpreset"))).IsEnabled = false;
                            ((UXRadioButton)(MyModule.GetRightView().FindName("rb_Colorpreset_based_ICCprofile"))).IsEnabled = false;
                           
                        }
                        else if (config.ColorManagement_RunType == (int)ColorManagementRunType.Off)
                        {
                            //((UXToggleSwitch)(MyModule.GetRightView().FindName("ColorManagement_ToggleSwitch"))).IsChecked = true;
                            ((UXRadioButton)(MyModule.GetRightView().FindName("rb_ICCprofile_based_Colorpreset"))).IsChecked = false;
                            ((UXRadioButton)(MyModule.GetRightView().FindName("rb_Colorpreset_based_ICCprofile"))).IsChecked = false;
                            ((UXRadioButton)(MyModule.GetRightView().FindName("rb_ICCprofile_based_Colorpreset"))).IsEnabled = false;
                            ((UXRadioButton)(MyModule.GetRightView().FindName("rb_Colorpreset_based_ICCprofile"))).IsEnabled = false;
                        }
                    }
                    else if (config.ColorManagement_Status == (int)ColorManagementStatus.On)
                    {
                        ((UXToggleSwitch)(MyModule.GetRightView().FindName("ColorManagement_ToggleSwitch"))).IsChecked = true;

                        if (config.ColorManagement_RunType == (int)ColorManagementRunType.Bymonitor)
                        {
                            //((UXToggleSwitch)(MyModule.GetRightView().FindName("ColorManagement_ToggleSwitch"))).IsChecked = true;
                            ((UXRadioButton)(MyModule.GetRightView().FindName("rb_ICCprofile_based_Colorpreset"))).IsChecked = true;
                            ((UXRadioButton)(MyModule.GetRightView().FindName("rb_Colorpreset_based_ICCprofile"))).IsChecked = false;
                            ((UXRadioButton)(MyModule.GetRightView().FindName("rb_ICCprofile_based_Colorpreset"))).IsEnabled = true;
                            ((UXRadioButton)(MyModule.GetRightView().FindName("rb_Colorpreset_based_ICCprofile"))).IsEnabled = true;
                            DdpmCommonHelper.DeviceManagerSA.AutoColorManagementForMonitorConfig(DdpmCommonHelper.ModuleOwner.SelectedHomeDevice.MonitorInfo, "BYMONITOR");
                        }
                        else if (config.ColorManagement_RunType == (int)ColorManagementRunType.Byhost)
                        {
                            //((UXToggleSwitch)(MyModule.GetRightView().FindName("ColorManagement_ToggleSwitch"))).IsChecked = true;
                            ((UXRadioButton)(MyModule.GetRightView().FindName("rb_ICCprofile_based_Colorpreset"))).IsChecked = false;
                            ((UXRadioButton)(MyModule.GetRightView().FindName("rb_Colorpreset_based_ICCprofile"))).IsChecked = true;
                            ((UXRadioButton)(MyModule.GetRightView().FindName("rb_ICCprofile_based_Colorpreset"))).IsEnabled = true;
                            ((UXRadioButton)(MyModule.GetRightView().FindName("rb_Colorpreset_based_ICCprofile"))).IsEnabled = true;
                            DdpmCommonHelper.DeviceManagerSA.AutoColorManagementForMonitorConfig(DdpmCommonHelper.ModuleOwner.SelectedHomeDevice.MonitorInfo, "BYHOST");
                        }
                        else if (config.ColorManagement_RunType == (int)ColorManagementRunType.Off)
                        {
                            //((UXToggleSwitch)(MyModule.GetRightView().FindName("ColorManagement_ToggleSwitch"))).IsChecked = true;
                            ((UXRadioButton)(MyModule.GetRightView().FindName("rb_ICCprofile_based_Colorpreset"))).IsChecked = false;
                            ((UXRadioButton)(MyModule.GetRightView().FindName("rb_Colorpreset_based_ICCprofile"))).IsChecked = false;
                            ((UXRadioButton)(MyModule.GetRightView().FindName("rb_ICCprofile_based_Colorpreset"))).IsEnabled = true;
                            ((UXRadioButton)(MyModule.GetRightView().FindName("rb_Colorpreset_based_ICCprofile"))).IsEnabled = true;
                        }
                    }                    

                }
                else
                    vis_ad = Visibility.Hidden;

                IsisAdvanced_Settings = vis_ad;
            }

        }

        /*
        public void Sync_SupportColorPresets() // HDR off
        {
            SupportColorPresets.RemoveAll(r => HDR_ColorPresetNameList.Any(a => a == r));

            int index = -1;

            // check Color Preset Strings Standard or Native

            if (MyModule.SelectedHomeDevice.MonitorInfo.modelName.StartsWith("UP"))
            {               
                index = SupportColorPresets.FindIndex(x => x == "Standard/Native");
                if (index >= 0)
                    SupportColorPresets[index] = "Native";
            }
            else
            {
                index = SupportColorPresets.FindIndex(x => x == "Standard/Native");
                if (index >= 0)
                    SupportColorPresets[index] = "Standard";
            }

            // check Color Preset Strings Custom 1/2/3 or User 1/2/3

            if (MyModule.SelectedHomeDevice.MonitorInfo.modelName.StartsWith("UP3221Q"))
            {
                index = SupportColorPresets.FindIndex(x => x == "Custom 1 / User 1");
                if (index >= 0)
                    SupportColorPresets[index] = "User 1";

                index = SupportColorPresets.FindIndex(x => x == "Custom 2 / User 2");
                if (index >= 0)
                    SupportColorPresets[index] = "User 2";

                index = SupportColorPresets.FindIndex(x => x == "Custom 3 / User 3");
                if (index >= 0)
                    SupportColorPresets[index] = "User 3";
            }
            else
            {
                index = SupportColorPresets.FindIndex(x => x == "Custom 1 / User 1");
                if (index >= 0)
                    SupportColorPresets[index] = "Custom 1";

                index = SupportColorPresets.FindIndex(x => x == "Custom 2 / User 2");
                if (index >= 0)
                    SupportColorPresets[index] = "Custom 2";

                index = SupportColorPresets.FindIndex(x => x == "Custom 3 / User 3");
                if (index >= 0)
                    SupportColorPresets[index] = "Custom 3";
            }

            // check Color Preset Strings Game or Game1

            index = SupportColorPresets.FindIndex(x => x == "Game2");

            if (index >= 0)
            {
                index = SupportColorPresets.FindIndex(x => x == "Game/Game1");
                if (index >= 0)
                    SupportColorPresets[index] = "Game1";
            }
            else
            {

                index = SupportColorPresets.FindIndex(x => x == "Game/Game1");
                if(index >= 0) 
                    SupportColorPresets[index] = "Game";
            }

            // check Color Preset Strings Rec.709 or BT.709 / Rec.709 or BT.709

            string strFY = string.Empty;

            for (int i = 0; i < MyModule.SelectedHomeDevice.MonitorInfo.modelName.Length; i++) // loop over the complete modelName
            {
                if (Char.IsDigit(MyModule.SelectedHomeDevice.MonitorInfo.modelName[i])) //check if the current char is digit
                {
                    strFY = MyModule.SelectedHomeDevice.MonitorInfo.modelName.Substring(i + 2, 2);
                    break;
                }
                
            }

            if (SupportColorPresets.Contains("Rec.709 / BT.709"))
            {
                if (strFY == "23")
                {
                    index = SupportColorPresets.FindIndex(x => x == "Rec.709 / BT.709");
                    if (index >= 0)
                        SupportColorPresets[index] = "Rec.709";
                }
                else if (strFY == "25" )
                {
                    index = SupportColorPresets.FindIndex(x => x == "Rec.709 / BT.709");
                    if (index >= 0)
                        SupportColorPresets[index] = "BT.709";

                }
            }

            // check Color Preset Strings Rec.2020 or BT.2020 / Rec.2020 or BT.2020

            if (SupportColorPresets.Contains("Rec.2020 / BT.2020"))
            {
                if (strFY == "23")
                {
                    index = SupportColorPresets.FindIndex(x => x == "Rec.2020 / BT.2020");
                    if (index >= 0) 
                        SupportColorPresets[index] = "Rec.2020";
                }
                else if (strFY == "25")
                {
                    index = SupportColorPresets.FindIndex(x => x == "Rec.2020 / BT.2020");
                    if (index >= 0)
                        SupportColorPresets[index] = "BT.2020";

                }
            }

        }
        */

        /*
        public string Sync_CurrentColorPreset(string curcolorPreset)
        {
            string strSync_CurrentColorPreset = string.Empty;

            int index = -1;

            // check Color Preset Strings Standard or Native

            if (MyModule.SelectedHomeDevice.MonitorInfo.modelName.StartsWith("UP"))
            {               
                if (curcolorPreset == "Standard/Native")
                    strSync_CurrentColorPreset = "Native";
            }
            else
            {
                if (curcolorPreset == "Standard/Native")
                    strSync_CurrentColorPreset = "Standard";
            }

            // check Color Preset Strings Custom 1/2/3 or User 1/2/3

            if (MyModule.SelectedHomeDevice.MonitorInfo.modelName.StartsWith("UP3221Q"))
            {                
                if (curcolorPreset == "Custom 1 / User 1")
                    strSync_CurrentColorPreset = "User 1";
                else if (curcolorPreset == "Custom 2 / User 2")
                    strSync_CurrentColorPreset = "User 2";
                else if (curcolorPreset == "Custom 3 / User 3")
                    strSync_CurrentColorPreset = "User 3";
            }
            else
            {
                if (curcolorPreset == "Custom 1 / User 1")
                    strSync_CurrentColorPreset = "Custom 1";
                else if (curcolorPreset == "Custom 2 / User 2")
                    strSync_CurrentColorPreset = "Custom 2";
                else if (curcolorPreset == "Custom 3 / User 3")
                    strSync_CurrentColorPreset = "Custom 3";               
            }

            // check Color Preset Strings Game or Game1

            index = SupportColorPresets.FindIndex(x => x == "Game2");

            if (index >= 0)
            {               
                if (curcolorPreset == "Game/Game1")
                    strSync_CurrentColorPreset = "Game1";
            }
            else
            {               
                if (curcolorPreset == "Game/Game1")
                    strSync_CurrentColorPreset = "Game";
            }

            // check Color Preset Strings Rec.709 or BT.709 / Rec.709 or BT.709

            string strFY = string.Empty;

            for (int i = 0; i < MyModule.SelectedHomeDevice.MonitorInfo.modelName.Length; i++) // loop over the complete modelName
            {
                if (Char.IsDigit(MyModule.SelectedHomeDevice.MonitorInfo.modelName[i])) //check if the current char is digit
                {
                    strFY = MyModule.SelectedHomeDevice.MonitorInfo.modelName.Substring(i + 2, 2);
                    break;
                }

            }

            // check Color Preset Strings Rec.2020 or BT.2020 / Rec.2020 or BT.2020
            int numFY = 0;
            try
            {
                numFY = Int32.Parse(strFY);

                if (numFY <= 23)
                {
                    if (curcolorPreset == "Rec.709 / BT.709")
                        strSync_CurrentColorPreset = "Rec.709";

                    if (curcolorPreset == "Rec.2020 / BT.2020")
                        strSync_CurrentColorPreset = "Rec.2020";

                }
                else if (numFY >= 25)
                {
                    if (curcolorPreset == "Rec.709 / BT.709")
                        strSync_CurrentColorPreset = "BT.709";

                    if (curcolorPreset == "Rec.2020 / BT.2020")
                        strSync_CurrentColorPreset = "BT.2020";
                }

            }
            catch (FormatException e)
            {
                Log?.Error("check Color Preset Strings Rec.709 or BT.709 / Rec.2020 or BT.2020..." + e.Message);               
            }            

            if (System.String.IsNullOrEmpty(strSync_CurrentColorPreset))
                strSync_CurrentColorPreset = curcolorPreset;

            return strSync_CurrentColorPreset;
        }
        */

        private int VerifyDellMonitor_Count()
        {
            int count = 0;
            foreach (HomeDevice hd in DdpmCommonHelper.ModuleOwner.HomeDevices)
            {
                if (hd.MonitorInfo.IsDellMonitor)
                    count++;
            }
            return count;
        }

        #region UI Enable Flags
        private bool _isBusy = false;
        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }
        #endregion UI Enable Flags
    }
}