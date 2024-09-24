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


//using System.Management;
//using System.Runtime.CompilerServices;
//using System.Security.Cryptography.X509Certificates;

[assembly: InternalsVisibleTo("DDPM.UI.Module.Color.Tests")]
namespace DDPM.UI.Module.Color
{
    internal class ColorViewModel : ObservableObject
    {
        #region Log
        private ILog? _log;
        public ILog? Log { get; set; }

        #endregion Log

      
        // add jim 20240604
        public RegistryMonitor_NightLight registryMonitor_NightLight = null;
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

        private string _isTabStoppable = "Cycle";

        public string isTabStoppable
        {
            get { return _isTabStoppable; }
            set
            {
                _isTabStoppable = value;
                OnPropertyChanged("isTabStoppable");
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

            DDPMSettings setting = DdpmCommonHelper.DeviceManagerSA.ReloadAppConfigData().Result;

            //this.Dispatcher.Invoke((Action)(() =>
            Task.Run(() =>
            {
                // 20240717 jim add
                if (setting.UserSettings.IsSynchronizemonitor)
                {
                    foreach (HomeDevice hd in DdpmCommonHelper.ModuleOwner.HomeDevices)
                    {
                        if (hd.MonitorInfo.IsDellMonitor)
                            DdpmCommonHelper.DeviceManagerSA?.WriteColorPreset(hd.MonitorInfo, SupportColorPresets[idex]);
                    }
                }
                else
                    //ColorViewModel vm = (ColorViewModel)DataContext;
                    // 20240619 jim modify
                    DdpmCommonHelper.DeviceManagerSA?.WriteColorPreset(
                        MyModule.SelectedHomeDevice?.MonitorInfo,
                        SupportColorPresets[idex]);
            });

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
                if (Test_AddAppCollectionData.GetInstance()._monitorConfigs.Count > 0)
                {
                    index = Test_AddAppCollectionData.GetInstance()._monitorConfigs.FindIndex(x =>
                                                    x.DeviceInfo.ModelName.Trim() == mo.edid.ModelName.Trim() &&
                                                    x.DeviceInfo.SerialNumber.Trim() == mo.edid.SerialNumber.Trim());
                }
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
                    DeviceInfo = mo.edid,
                    RunType = (int)ColorPresetRunType.Manual,
                    AppInfo = new Dictionary<string, ColorPresetSettings_AppInfo>(),
                    PresetForManual = "Standard/Native",
                    ColorManagement_Status = (int)ColorManagementStatus.Off,
                    ColorManagement_RunType = (int)ColorManagementRunType.Off
                });

                index = get_index_of_json_config_for_cur_monitor(mo);

                Test_AddAppCollectionData.GetInstance()._monitorConfigs[index].AppInfo.Add("Desktop Application", new ColorPresetSettings_AppInfo()
                {
                    ColorPresetName = "Standard/Native",
                    IconName = "Assets/palette.png",
                });

                Test_AddAppCollectionData.GetInstance()._monitorConfigs[index].AppInfo.Add("UWP Application", new ColorPresetSettings_AppInfo()
                {
                    ColorPresetName = "Standard/Native",
                    IconName = "Assets/palette.png",
                });
            }
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
            IsBusy = true;
            bw.RunWorkerAsync();
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
            startWatcher.EventArrived -= startWatcher_EventArrived;
            startWatcher.Stop();
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
            endProcWatcher.EventArrived -= ProcessEnded;
            endProcWatcher.Stop();
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

        private void DoWork_RefreshData(object sender, DoWorkEventArgs e)
        {
            try //2024-06-19 Elie, add try catch to get exception.
            {
                //OSD control back event
                DdpmCommonHelper.DeviceManagerSA.VCPchanged += OnVCPChangedEvent;

                DdpmCommonHelper.DeviceManagerSA.Coloreset_manual_ChangeEvent += OnColoresetManualChangeHandler;

                // -- begin add jim 20240604
                SyncNightlightStatus();

                // jim remove
                //WatchForProcessStart();
                //WatchForProcessEnd();

                // -- end

                // -- begin add jim 20240604
                SupportColorPresets = new List<string>();
                SupportColorPresets = DdpmCommonHelper.DeviceManagerSA.ReadColorPreset(MyModule.SelectedHomeDevice.MonitorInfo).Result;

                //Dean 0612 add
                string curPreset = DdpmCommonHelper.DeviceManagerSA?.ReadCurrentColorPreset(DdpmCommonHelper.ModuleOwner?.SelectedHomeDevice?.MonitorInfo).Result;

                ColorPresets_ItemsCollection = new List<string>();

                MyModule.GetRightView().Dispatcher.Invoke((Action)(() =>
                {
                    foreach (string info in SupportColorPresets)
                    {
                        ColorPresets_ItemsCollection.Add(new string(info));
                    }

                    //Dean 0612 add
                    if (!string.IsNullOrEmpty(curPreset))
                    {
                        int idx = ColorPresets_ItemsCollection.FindIndex(x => x.ToUpper().Equals(curPreset.ToUpper()));
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

                if (config.AppInfo.Count <= 0)
                {
                    config.AppInfo.Add("Desktop Application", new ColorPresetSettings_AppInfo()
                    {
                        ColorPresetName = "Standard/Native",
                        IconName = "Assets/palette.png",
                    });
                    config.AppInfo.Add("UWP Application", new ColorPresetSettings_AppInfo()
                    {
                        ColorPresetName = "Standard/Native",
                        IconName = "Assets/palette.png",
                    });
                }

                DdpmCommonHelper.DeviceManagerSA.WriteColorPresetSettings(Test_AddAppCollectionData.GetInstance()._monitorConfigs);
                Thread.Sleep(100);

                foreach (string key in config.AppInfo.Keys)
                {
                    ColorPresetSettings_AppInfo value = config.AppInfo[key];
                    int pIdx = SupportColorPresets.FindIndex(x =>
                                        x.Trim() == value.ColorPresetName.Trim());
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

                //Robert_Lin, 20240528
                //NEW Added code:
                Test_AddAppCollectionData.GetInstance().AppsList = new ObservableCollection<AppData>(tempList);

                AppsList = Test_AddAppCollectionData.GetInstance().AppsList;

                _ICC_Metadata = DdpmCommonHelper.DeviceManagerSA?.DownloadICCData(DdpmCommonHelper.ModuleOwner?.SelectedHomeDevice?.MonitorInfo).Result;

                MyModule.GetRightView().Dispatcher.Invoke((Action)(() =>
                {                    
                    update_ui_over_runtype(config);
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
                DDPMSettings data = DdpmCommonHelper.DeviceManagerSA.ReloadAppConfigData().Result;//Be careful if spend much time here                
                //ex: vm.LockMaskVisible = data.LockSettings.Lock_Display_ColorPreset ? Visibility.Visible : Visibility.Collapsed;
                //Read user default lock value, these values are synced from IT lock event          
                Trace.WriteLine($"[SettingsPage] Color right page(Lock) : {data.LockSettings.Lock_Display_ColorPreset}"); 
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

            if (e.vcpcode.Equals("DC") || e.vcpcode.Equals("F0") || e.vcpcode.Equals("14") || e.vcpcode.Equals("E2")) // Color changes by OSD menu
            {
                
                string curPreset = DdpmCommonHelper.DeviceManagerSA?.ReadCurrentColorPreset(DdpmCommonHelper.ModuleOwner?.SelectedHomeDevice?.MonitorInfo).Result;


                MyModule.GetRightView().Dispatcher.Invoke((Action)(() =>
                {  
                    if (!string.IsNullOrEmpty(curPreset))
                    {
                        int idx = ColorPresets_ItemsCollection.FindIndex(x => x.ToUpper().Equals(curPreset.ToUpper()));
                        if (idx >= 0)
                        {
                            UpdateColorPresetSelectedIndex(idx);
                        }
                    }
                    
                }));
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
                        if (string.Equals(colorpreset, "Standard/Native", StringComparison.OrdinalIgnoreCase))
                            break;
                        index++;
                    }
                }
                else if (string.Equals(e, "Game", StringComparison.OrdinalIgnoreCase) || string.Equals(e, "Game1", StringComparison.OrdinalIgnoreCase))
                {
                    foreach (string colorpreset in SupportColorPresets)
                    {
                        if (string.Equals(colorpreset, "Game/Game1", StringComparison.OrdinalIgnoreCase))
                            break;
                        index++;
                    }
                }
                else if (e.Contains("Rec", StringComparison.OrdinalIgnoreCase) || e.Contains("BT.", StringComparison.OrdinalIgnoreCase) || e.Contains("709", StringComparison.OrdinalIgnoreCase))
                {
                    foreach (string colorpreset in SupportColorPresets)
                    {
                        if (string.Equals(colorpreset, "Rec. 709 / BT.709", StringComparison.OrdinalIgnoreCase))
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
        }

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
        }

        private void update_ui_over_runtype(ColorPresetSettings config)
        {
            if (config.RunType == (int)ColorPresetRunType.Auto)
            {
                ((Expander)(MyModule.GetRightView().FindName("Expander_Manual"))).IsExpanded = false;
                ((Expander)(MyModule.GetRightView().FindName("Expander_Auto"))).IsExpanded = true;
                DdpmCommonHelper.DeviceManagerSA.AutoSetColorPresetForMonitorConfig(DdpmCommonHelper.ModuleOwner.SelectedHomeDevice.MonitorInfo, "ON", IsAutoColorPreset_Lock);

            }
            else
            {
                ((Expander)(MyModule.GetRightView().FindName("Expander_Manual"))).IsExpanded = true;
                ((Expander)(MyModule.GetRightView().FindName("Expander_Auto"))).IsExpanded = false;
                DdpmCommonHelper.DeviceManagerSA.AutoSetColorPresetForMonitorConfig(DdpmCommonHelper.ModuleOwner.SelectedHomeDevice.MonitorInfo, "OFF", IsAutoColorPreset_Lock);
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