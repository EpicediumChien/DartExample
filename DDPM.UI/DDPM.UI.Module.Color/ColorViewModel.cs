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
using RegistryUtils;
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

        private List<X509Certificate2> TrustedPublisher = new List<X509Certificate2>();
        private List<X509Certificate2> TrustedRoot = new List<X509Certificate2>();

        //private readonly string[] Issuer = { "CN=Entrust Certification Authority - L1F, O=\"Entrust, C=US", "CN=localhost, O=DigiNow, C=US" };
        private readonly string[] Issuer = { "Entrust Certification Authority - L1F, OU=\"(c) 2016 Entrust, Inc. - for authorized use only\", OU=See www.entrust.net/legal-terms, O=\"Entrust, Inc.\", C=US" };
        private readonly string[] Subject = { "CN=content-cdn.dell.com, O=Dell, L=Round Rock, S=Texas, C=US" };

        private string[] Issuers;
        private string[] Subjects;

        // add jim 20240604
        public RegistryUtils.RegistryMonitor_NightLight registryMonitor_NightLight = null;
        public RegistryUtils.RegistryMonitor_ICC registryMonitor_ICC = null;

        // add jim 20240604
        //private ManagementEventWatcher startWatcher;
        //private ManagementEventWatcher endProcWatcher;

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

                if (_ICC_Metadata.Is_Support_ICC_DeviceName)
                {
                    // add jim 20240604
                    if (DCM_Visibility == Visibility.Hidden)
                    {
                        if (ColorManagement_isChecked)
                        {
                            if (ICCprofile_based_Colorpreset_enable)
                            {
                                // add jim 0607
                                if (_ICC_Metadata._support_ICC_DeviceName[MyModule.SelectedHomeDevice.MonitorInfo.modelName] != null)
                                {
                                    int count = _ICC_Metadata._support_ICC_DeviceName[MyModule.SelectedHomeDevice.MonitorInfo.modelName].Count;

                                    // add jim 20240725
                                    //int i = 0;
                                    //for (i = 0; i < count; i++)
                                    //{
                                    //    RegistryUtils.MonitorProfile.IntsallMonitorProfile(_ICC_Metadata.strICC_Folder + _ICC_Metadata._support_ICC_DeviceName[MyModule.SelectedHomeDevice.MonitorInfo.modelName][i].File);
                                    //}

                                    for (int i = 0; i < count; i++)
                                    {
                                        string[] separators = { "|" };
                                        string[] strICC_ColorPresets = _ICC_Metadata._support_ICC_DeviceName[MyModule.SelectedHomeDevice.MonitorInfo.modelName][i].ColorPreset.Split(separators, StringSplitOptions.None);

                                        if (string.Equals(SupportColorPresets[idex], "Standard/Native", StringComparison.OrdinalIgnoreCase))
                                        {
                                            if (string.Equals(strICC_ColorPresets[0], "Standard", StringComparison.OrdinalIgnoreCase) || string.Equals(strICC_ColorPresets[0], "Native", StringComparison.OrdinalIgnoreCase))
                                            {
                                                RegistryUtils.MonitorProfile.SetMonitorProfile(_ICC_Metadata._support_ICC_DeviceName[MyModule.SelectedHomeDevice.MonitorInfo.modelName][i].File);
                                                break;
                                            }
                                        }
                                        else if (string.Equals(SupportColorPresets[idex], "Game/Game1", StringComparison.OrdinalIgnoreCase))
                                        {
                                            if (string.Equals(strICC_ColorPresets[0], "Game", StringComparison.OrdinalIgnoreCase) || string.Equals(strICC_ColorPresets[0], "Game1", StringComparison.OrdinalIgnoreCase))
                                            {
                                                RegistryUtils.MonitorProfile.SetMonitorProfile(_ICC_Metadata._support_ICC_DeviceName[MyModule.SelectedHomeDevice.MonitorInfo.modelName][i].File);
                                                break;
                                            }
                                        }
                                        else if (string.Equals(SupportColorPresets[idex], "Rec. 709 / BT.709", StringComparison.OrdinalIgnoreCase))
                                        {
                                            if (strICC_ColorPresets[0].Contains("Rec", StringComparison.OrdinalIgnoreCase) || strICC_ColorPresets[0].Contains("BT.", StringComparison.OrdinalIgnoreCase) || strICC_ColorPresets[0].Contains("709", StringComparison.OrdinalIgnoreCase))
                                            {
                                                RegistryUtils.MonitorProfile.SetMonitorProfile(_ICC_Metadata._support_ICC_DeviceName[MyModule.SelectedHomeDevice.MonitorInfo.modelName][i].File);
                                                break;
                                            }
                                        }

                                        if (string.Equals(SupportColorPresets[idex], strICC_ColorPresets[0], StringComparison.OrdinalIgnoreCase))
                                        {
                                            RegistryUtils.MonitorProfile.SetMonitorProfile(_ICC_Metadata._support_ICC_DeviceName[MyModule.SelectedHomeDevice.MonitorInfo.modelName][i].File);
                                            break;
                                        }
                                    }

                                    //if (string.Equals(SupportColorPresets[idex], "Standard", StringComparison.OrdinalIgnoreCase))
                                    //    RegistryUtils.MonitorProfile.SetMonitorProfile("Dell_U3224KB_Native_v2.icm");
                                    //else if (string.Equals(SupportColorPresets[idex], "Display P3", StringComparison.OrdinalIgnoreCase))
                                    //    RegistryUtils.MonitorProfile.SetMonitorProfile("Dell_U3224KB_DisplayP3_v2.icm");
                                    //else if (string.Equals(SupportColorPresets[idex], "DCI-P3", StringComparison.OrdinalIgnoreCase))
                                    //    RegistryUtils.MonitorProfile.SetMonitorProfile("Dell_U3224KB_DCIP3_v2.icm");
                                    //else if (string.Equals(SupportColorPresets[idex], "sRGB", StringComparison.OrdinalIgnoreCase))
                                    //    RegistryUtils.MonitorProfile.SetMonitorProfile("Dell_U3224KB_sRGB_v2.icm");
                                    //else if (string.Equals(SupportColorPresets[idex], "Rec. 709", StringComparison.OrdinalIgnoreCase))
                                    //    RegistryUtils.MonitorProfile.SetMonitorProfile("Dell_U3224KB_Rec709_v2.icm");
                                }
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
                    RunType = (int)ColorPresetRunType.Auto,
                    AppInfo = new Dictionary<string, ColorPresetSettings_AppInfo>(),
                    PresetForManual = "Standard/Native"
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

                    if (registryMonitor_ICC != null)
                    {
                        if (registryMonitor_ICC.IsMonitoring)
                            registryMonitor_ICC.Dispose();
                        registryMonitor_ICC = null;
                    }
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
                            if (registryMonitor_ICC == null)
                            {
                                string keyName = string.Format("{0}\\{1}", "HKEY_CURRENT_USER", @"Software\Microsoft\Windows NT\CurrentVersion\ICM\ProfileAssociations\Display\{4d36e96e-e325-11ce-bfc1-08002be10318}");

                                registryMonitor_ICC = new RegistryUtils.RegistryMonitor_ICC(keyName);
                                registryMonitor_ICC.RegChanged += new EventHandler(OnRegChanged_ICC);
                                registryMonitor_ICC.Error += new System.IO.ErrorEventHandler(OnError_ICC);
                                registryMonitor_ICC.Start();
                            }
                        }
                    }
                }));
            }
        }

        private void DoWork_RefreshData(object sender, DoWorkEventArgs e)
        {
            try //2024-06-19 Elie, add try catch to get exception.
            {
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
                Test_AddAppCollectionData.GetInstance().AppsList.Clear();

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

                MyModule.GetRightView().Dispatcher.Invoke((Action)(() =>
                {
                    RefreshUI();
                    update_ui_over_runtype(config.RunType);
                }));

                //OnPropertyChanged("ColorPresets_ItemsCollection");
                //OnPropertyChanged("AppsList");
                //OnPropertyChanged("NightlightStatus");
                //OnPropertyChanged("IsisAdvanced_Settings");

                // remove jim 20240606
                //WatchForProcessStart();

                //WatchForProcessEnd();

                // ICC profiles mapping schema

                _ICC_Metadata = DdpmCommonHelper.DeviceManagerSA?.DownloadICCData(DdpmCommonHelper.ModuleOwner?.SelectedHomeDevice?.MonitorInfo).Result;

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

                //OSD control back event
                DdpmCommonHelper.DeviceManagerSA.VCPchanged += OnVCPChangedEvent;
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

            if (registryMonitor_ICC != null)
            {
                registryMonitor_ICC.Stop();
                registryMonitor_ICC.RegChanged -= new EventHandler(OnRegChanged_ICC);
                registryMonitor_ICC.Error -= new System.IO.ErrorEventHandler(OnError_ICC);
                registryMonitor_ICC = null;
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

        public void OnRegChanged_ICC(object sender, EventArgs e)
        {
            string Key_Profile_Name = string.Empty;
            Key_Profile_Name = RegistryUtils.MonitorProfile.GetMonitorProfile();

            int index = 0;
            int count = _ICC_Metadata._support_ICC_DeviceName[MyModule.SelectedHomeDevice.MonitorInfo.modelName].Count;

            MyModule.GetRightView().Dispatcher.Invoke((Action)(() =>
            {
                //ColorViewModel vm = (ColorViewModel)DataContext;

                // 20240725 jim add
                for (int i = 0; i < count; i++)
                {
                    if (string.Equals(Key_Profile_Name, _ICC_Metadata._support_ICC_DeviceName[MyModule.SelectedHomeDevice.MonitorInfo.modelName][i].File, StringComparison.OrdinalIgnoreCase))
                    {
                        // 20240619 jim modify

                        //string strICC_ColorPreset = _ICC_Metadata._support_ICC_DeviceName[MyModule.SelectedHomeDevice.MonitorInfo.modelName][i].ColorPreset;

                        string[] separators = { "," };
                        string[] strICC_ColorPresets = _ICC_Metadata._support_ICC_DeviceName[MyModule.SelectedHomeDevice.MonitorInfo.modelName][i].ColorPreset.Split(separators, StringSplitOptions.None);

                        if (string.Equals(strICC_ColorPresets[0], "Standard", StringComparison.OrdinalIgnoreCase) || string.Equals(strICC_ColorPresets[0], "Native", StringComparison.OrdinalIgnoreCase))
                            DdpmCommonHelper.DeviceManagerSA.WriteColorPreset(MyModule.SelectedHomeDevice.MonitorInfo, "Standard/Native");
                        else if (string.Equals(strICC_ColorPresets[0], "Game", StringComparison.OrdinalIgnoreCase) || string.Equals(strICC_ColorPresets[0], "Game1", StringComparison.OrdinalIgnoreCase))
                            DdpmCommonHelper.DeviceManagerSA.WriteColorPreset(MyModule.SelectedHomeDevice.MonitorInfo, "Game/Game1");
                        else if (strICC_ColorPresets[0].Contains("Rec", StringComparison.OrdinalIgnoreCase) || strICC_ColorPresets[0].Contains("BT.", StringComparison.OrdinalIgnoreCase) || strICC_ColorPresets[0].Contains("709", StringComparison.OrdinalIgnoreCase))
                            DdpmCommonHelper.DeviceManagerSA.WriteColorPreset(MyModule.SelectedHomeDevice.MonitorInfo, "Rec. 709 / BT.709");
                        else
                            DdpmCommonHelper.DeviceManagerSA.WriteColorPreset(MyModule.SelectedHomeDevice.MonitorInfo, strICC_ColorPresets[0]);

                        if (string.Equals(strICC_ColorPresets[0], "Standard", StringComparison.OrdinalIgnoreCase) || string.Equals(strICC_ColorPresets[0], "Native", StringComparison.OrdinalIgnoreCase))
                        {
                            foreach (string colorpreset in SupportColorPresets)
                            {
                                if (string.Equals(colorpreset, "Standard/Native", StringComparison.OrdinalIgnoreCase))
                                    break;
                                index++;
                            }
                        }
                        else if (string.Equals(strICC_ColorPresets[0], "Game", StringComparison.OrdinalIgnoreCase) || string.Equals(strICC_ColorPresets[0], "Game1", StringComparison.OrdinalIgnoreCase))
                        {
                            foreach (string colorpreset in SupportColorPresets)
                            {
                                if (string.Equals(colorpreset, "Game/Game1", StringComparison.OrdinalIgnoreCase))
                                    break;
                                index++;
                            }
                        }
                        else if (strICC_ColorPresets[0].Contains("Rec", StringComparison.OrdinalIgnoreCase) || strICC_ColorPresets[0].Contains("BT.", StringComparison.OrdinalIgnoreCase) || strICC_ColorPresets[0].Contains("709", StringComparison.OrdinalIgnoreCase))
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
                                if (string.Equals(colorpreset, strICC_ColorPresets[0], StringComparison.OrdinalIgnoreCase))
                                {
                                    break;
                                }
                                index++;
                            }
                        }

                        // jim modify 20240604
                        ((ComboBox)(MyModule.GetRightView().FindName("cbManualPreset"))).SelectedIndex = index;
                        break;
                    }
                }

                //if (string.Equals(Key_Profile_Name, "Dell_U3224KB_Native_v2.icm", StringComparison.OrdinalIgnoreCase))
                //{
                // 20240619 jim modify
                //    DdpmCommonHelper.DeviceManagerSA.WriteColorPreset(MyModule.SelectedHomeDevice.MonitorInfo, "Standard");

                //    foreach (string colorpreset in SupportColorPresets)
                //    {
                //        if (string.Equals(colorpreset, "Standard", StringComparison.OrdinalIgnoreCase))
                //        {
                //            break;
                //        }

                //        index++;
                //    }

                // jim modify 20240604
                //    ((ComboBox)(MyModule.GetRightView().FindName("cbManualPreset"))).SelectedIndex = index;

                //}
                //else if (string.Equals(Key_Profile_Name, "Dell_U3224KB_DisplayP3_v2.icm", StringComparison.OrdinalIgnoreCase))
                //{
                // 20240619 jim modify
                //    DdpmCommonHelper.DeviceManagerSA.WriteColorPreset(MyModule.SelectedHomeDevice.MonitorInfo, "Display P3");

                //    foreach (string colorpreset in SupportColorPresets)
                //    {
                //        if (string.Equals(colorpreset, "Display P3", StringComparison.OrdinalIgnoreCase))
                //        {
                //            break;
                //        }

                //        index++;
                //    }

                // jim modify 20240604
                //    ((ComboBox)(MyModule.GetRightView().FindName("cbManualPreset"))).SelectedIndex = index;

                //}
                //else if (string.Equals(Key_Profile_Name, "Dell_U3224KB_DCIP3_v2.icm", StringComparison.OrdinalIgnoreCase))
                //{
                // 20240619 jim modify
                //    DdpmCommonHelper.DeviceManagerSA.WriteColorPreset(MyModule.SelectedHomeDevice.MonitorInfo, "DCI-P3");

                //    foreach (string colorpreset in SupportColorPresets)
                //    {
                //        if (string.Equals(colorpreset, "DCI-P3", StringComparison.OrdinalIgnoreCase))
                //        {
                //            break;
                //        }

                //        index++;
                //    }

                // jim modify 20240604
                //    ((ComboBox)(MyModule.GetRightView().FindName("cbManualPreset"))).SelectedIndex = index;

                //}
                //else if (string.Equals(Key_Profile_Name, "Dell_U3224KB_sRGB_v2.icm", StringComparison.OrdinalIgnoreCase))
                //{
                // 20240619 jim modify
                //    DdpmCommonHelper.DeviceManagerSA.WriteColorPreset(MyModule.SelectedHomeDevice.MonitorInfo, "sRGB");

                //    foreach (string colorpreset in SupportColorPresets)
                //    {
                //        if (string.Equals(colorpreset, "sRGB", StringComparison.OrdinalIgnoreCase))
                //        {
                //            break;
                //        }

                //        index++;
                //    }

                // jim modify 20240604
                //    ((ComboBox)(MyModule.GetRightView().FindName("cbManualPreset"))).SelectedIndex = index;

                //}
                //else if (string.Equals(Key_Profile_Name, "Dell_U3224KB_Rec709_v2.icm", StringComparison.OrdinalIgnoreCase))
                //{
                // 20240619 jim modify
                //    DdpmCommonHelper.DeviceManagerSA.WriteColorPreset(MyModule.SelectedHomeDevice.MonitorInfo, "Rec. 709");

                //    foreach (string colorpreset in SupportColorPresets)
                //    {
                //        if (string.Equals(colorpreset, "Rec. 709", StringComparison.OrdinalIgnoreCase))
                //        {
                //            break;
                //        }

                //        index++;
                //    }

                // jim modify 20240604
                //    ((ComboBox)(MyModule.GetRightView().FindName("cbManualPreset"))).SelectedIndex = index;

                //}
                //else if (string.Equals(Key_Profile_Name, "Dell_U3224KB_HDR_v4_MHC2.icm", StringComparison.OrdinalIgnoreCase))
                //{
                // 20240619 jim modify
                //    DdpmCommonHelper.DeviceManagerSA.WriteColorPreset(MyModule.SelectedHomeDevice.MonitorInfo, "Rec. 709");

                //    foreach (string colorpreset in SupportColorPresets)
                //    {
                //        if (string.Equals(colorpreset, "Desktop", StringComparison.OrdinalIgnoreCase))
                //        {
                //            break;
                //        }

                //        index++;
                //    }

                // jim modify 20240604
                //    ((ComboBox)(MyModule.GetRightView().FindName("cbManualPreset"))).SelectedIndex = index;

                //}

                RefreshUI();
            }));

            return;
        }

        // add jim 20240604
        public void OnError_ICC(object sender, ErrorEventArgs e)
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

        private void update_ui_over_runtype(int runtype)
        {
            if (runtype == (int)ColorPresetRunType.Auto)
            {
                ((Expander)(MyModule.GetRightView().FindName("Expander_Manual"))).IsExpanded = false;
                ((Expander)(MyModule.GetRightView().FindName("Expander_Auto"))).IsExpanded = true;              
         
            }
            else
            {

                ((Expander)(MyModule.GetRightView().FindName("Expander_Manual"))).IsExpanded = true;
                ((Expander)(MyModule.GetRightView().FindName("Expander_Auto"))).IsExpanded = false;            
        
            }

            DDPMSettings setting = DdpmCommonHelper.DeviceManagerSA.ReloadAppConfigData().Result;

            if (setting.UserSettings.IsAutoColorPreset_Lock)
            {
                ((Expander)(MyModule.GetRightView().FindName("Expander_Auto"))).IsEnabled = false;
                ((ListBox)(MyModule.GetRightView().FindName("lb_AppList"))).IsEnabled = false;
                ((UXButton)(MyModule.GetRightView().FindName("btn_AddApp"))).IsEnabled = false;
                
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