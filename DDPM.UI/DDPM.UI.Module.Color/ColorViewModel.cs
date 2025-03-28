using CommunityToolkit.Mvvm.ComponentModel;
using DDPM.SA.Common;
using DDPM.SA.Common.Settings;
using DDPM.UI.Common;
using DDPM.UI.Common.Interfaces;
using DDPM.UI.Common.Models;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.UX.WPF.Controls;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Management;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using VcpCore.Common;
using System.Windows.Input;
using System.Diagnostics;
using CommunityToolkit.Mvvm.Input;

[assembly: InternalsVisibleTo("DDPM.UI.Module.Color.Tests")]
namespace DDPM.UI.Module.Color
{
    internal class ColorViewModel : ObservableObject, INotifyPropertyChanged
    {
        #region Log
        private ILog? _log = null;
        public ILog? Log { get; set; }
        #endregion Log

        #region private data object
        private ManagementEventWatcher? startWatcher = null;
        private ManagementEventWatcher? endProcWatcher = null;
        private DDPM.SA.Common.IIC_Metadata _ICC_Metadata = new DDPM.SA.Common.IIC_Metadata();
        private BackgroundWorker? bw = null;
        private bool IsColorEnable = false;
        #endregion private data object

        public Guid? guid { get; set; } = Guid.NewGuid();
        public IModuleOwner? ModuleOwner { get; set; } = null;
        public ColorModule MyModule { get; set; }
        public List<string> SupportColorPresets { get; set; }
        public List<string> ColorPresets_ItemsCollection { get; set; }
        public bool Is_Game_DeviceName { get; set; } = false;
        public bool SmartHDR_ON { get; set; } = false;
        public bool Is_ColorPreset_ManualFirst { get; set; } = true;
        public bool ColorManagement_isChecked { get; set; } = false;
        public bool ICCprofile_based_Colorpreset_enable { get; set; } = false;

        //Robert_Lin 2025-2-26 Narrator. The default value of KeyboardNavigation.TabNavigation is "Conntinue".
        //Howerever, I add a constant string below to restore back to original value.
        public const string DefaultTabNavigation = "Continue"; //"Cycle";

        public bool ColorEnable
        {
            get
            {
                OnPropertyChanged(nameof(ColorOpacity));
                OnPropertyChanged(nameof(GreayoutAlart));
                //return (IsColorEnable || !Is_Game_DeviceName);
                return true; // jim 20241207 modify for The DDPM color profile can not be applied by DDPM on Smart HDR mode.(Gaming monitor ex: AW2724DM)
            }
        }

        public string ColorOpacity
        {
            get
            {
                // jim 20241207 modify for The DDPM color profile can not be applied by DDPM on Smart HDR mode.(Gaming monitor ex: AW2724DM)
                //if (IsColorEnable || !Is_Game_DeviceName)
                //{
                return "1.0";
                //}
                //return "0.5";
            }
        }

        public Visibility GreayoutAlart
        {
            get
            {
                // // jim 20241207 modify for The DDPM color profile can not be applied by DDPM on Smart HDR mode.(Gaming monitor ex: AW2724DM)
                //if (IsColorEnable || !Is_Game_DeviceName)
                //{
                return Visibility.Collapsed;
                //}
                //return Visibility.Visible;
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

        private string _TabNavigation = DefaultTabNavigation;// "Cycle";
        public string TabNavigation
        {
            get { return _TabNavigation; }
            set
            {
                _TabNavigation = value;
                OnPropertyChanged("TabNavigation");
            }
        }

        private Visibility isAdvanced_Settings = Visibility.Collapsed;
        public Visibility IsisAdvanced_Settings
        {
            //Robert)Lin debug 2025-1-18
            //get => Visibility.Visible;
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

        public bool IsAutoColorPreset_Lock { get; set; } = false;
        public string last_selected_value { get; set; } = string.Empty;

        //
        //Dean 0612 add for ALS syncup
        //
        private int _colorPresetSelectedIndex { get; set; } = -1;//no choose
        public int ColorPresetSelectedIndex
        {
            get
            {
                return _colorPresetSelectedIndex;
            }
            set
            {
                int temp = _colorPresetSelectedIndex;
                if (ColorPresets_ItemsCollection != null && temp < ColorPresets_ItemsCollection.Count)//before
                    last_selected_value = ColorPresets_ItemsCollection[temp];
                //if (CheckIfDisableALSFeature()) //Do not need to check ALS feature PIMS-345895
                //{
                _colorPresetSelectedIndex = value;
                SetColorPresetBySelection();
                //}
                //else
                //{
                //    _colorPresetSelectedIndex = temp;
                //}
                OnPropertyChanged("ColorPresetSelectedIndex");
                if (ColorPresets_ItemsCollection != null && _colorPresetSelectedIndex < ColorPresets_ItemsCollection.Count)//after: keep or change
                    last_selected_value = ColorPresets_ItemsCollection[_colorPresetSelectedIndex];
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

            if (ColorPresets_ItemsCollection != null && selIndex < ColorPresets_ItemsCollection.Count)
                last_selected_value = ColorPresets_ItemsCollection[selIndex];

            OnPropertyChanged("ColorPresetSelectedIndex"); // Jim 20250211 fix 0x52 color preset no synchronization issue.
        }

        //
        //move function [cbManualPreset_SelectionChanged] from code behind to here as code inline binding as well
        private void SetColorPresetBySelection()
        {
            int idex = ColorPresetSelectedIndex;// cbManualPreset.SelectedIndex;

            int nSupportColorPresets_Count = SupportColorPresets.Count;

            if (idex >= 0 && idex < nSupportColorPresets_Count)
            {
                DDPMSettings setting = DdpmCommonHelper.ReadDDPMSettings();

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
                                      SupportColorPresets[idex], 0, Is_Game_DeviceName, SmartHDR_ON);
                                }
                                else
                                {
                                    foreach (HomeDevice hd in DdpmCommonHelper.ModuleOwner.HomeDevices)
                                    {
                                        // Jim 20250107 modify for PIMS-314608 U2725QEt Wistron- P3:DDPM(Windows)-Shine a torch or cover the sensor of DUT1,DUT2 screen has not changed
                                        if (hd.MonitorInfo.IsDellMonitor)
                                            DdpmCommonHelper.DeviceManagerSA?.WriteColorPreset(hd.MonitorInfo, SupportColorPresets[idex], 0, Is_Game_DeviceName, SmartHDR_ON, null, true);

                                    }
                                }
                            }
                            else
                                //ColorViewModel vm = (ColorViewModel)DataContext;
                                // 20240619 jim modify
                                DdpmCommonHelper.DeviceManagerSA?.WriteColorPreset(
                                    MyModule.SelectedHomeDevice?.MonitorInfo,
                                    SupportColorPresets[idex], 0, Is_Game_DeviceName, SmartHDR_ON); // jim 20241207 modify for The DDPM color profile can not be applied by DDPM on Smart HDR mode.(Gaming monitor ex: AW2724DM)
                        }

                    });
                }
            }
        }

        private Visibility _DCM_Visibility = Visibility.Collapsed;
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

        [Obsolete]
        private bool CheckIfDisableALSFeature()
        {
            ALSConfig cfg = DdpmCommonHelper.DeviceManagerSA?.GetALSFeatureValue(MyModule.SelectedHomeDevice?.MonitorInfo, ALSFeatureQueryType.All, 0).Result;
            if (cfg != null && cfg.isSupportALS > 0 &&
                cfg.isAutoColorTemp)
            {
                //Dean 0614 modify to meet figma
                //if(MessageBox.Show("Auto Color Temperature is currently enabled. Do you wish to disable it to continue?", "Warning", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                if (DdpmCommonHelper.DDPMMesssageBox(Strings.ImpExp_Warning, Strings.Auto_Color_Temperature_MSG))
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
            bw = new BackgroundWorker()
            {
                WorkerReportsProgress = true,
                WorkerSupportsCancellation = true
            };
            guid = Guid.NewGuid();
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
                "WITHIN 1 " +
                " WHERE TargetInstance ISA 'Win32_Process' "
                + "   AND TargetInstance.Name like 'ColorManagement.exe'";

            // The dot in the scope means use the current machine
            string scope = @"\\.\root\CIMV2";

            if (startWatcher == null)
            {
                // Create a watcher and listen for events
                startWatcher = new ManagementEventWatcher(scope, queryString);
                startWatcher.EventArrived += startWatcher_EventArrived;
                startWatcher.Start();
            }
        }

        // add jim 20240606
        public void WatchForProcessStart_Stop()
        {
            DdpmCommonHelper.WriteUILog("[ColorViewModel] [WatchForProcessStart_Stop] WatchForProcessStart_Stop() Begin");
            /*string queryString =
                "SELECT TargetInstance" +
                "  FROM __InstanceCreationEvent " +
                "WITHIN 1 " +
                " WHERE TargetInstance ISA 'Win32_Process' "
                + "   AND TargetInstance.Name like 'ColorManagement.exe'";

            // The dot in the scope means use the current machine
            string scope = @"\\.\root\CIMV2";

            // Create a watcher and listen for events
            //startWatcher = new ManagementEventWatcher(scope, queryString);*/

            //avoid exception
            if (startWatcher != null)
            {
                startWatcher.EventArrived -= startWatcher_EventArrived;
                startWatcher.Stop();
                startWatcher.Dispose();
                startWatcher = null;
            }
            DdpmCommonHelper.WriteUILog("[ColorViewModel] [WatchForProcessStart_Stop] WatchForProcessStart_Stop() end");
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
                }));
            }
        }

        // jim modify 20240606
        public void WatchForProcessEnd()
        {
            try
            {
                string queryString =
                    "SELECT TargetInstance" +
                    "  FROM __InstanceDeletionEvent " +
                    "WITHIN 1 " +
                    " WHERE TargetInstance ISA 'Win32_Process' "
                    + "   AND TargetInstance.Name like 'ColorManagement.exe'";

                string scope = @"\\.\root\CIMV2";

                if (endProcWatcher == null)
                {
                    // Create a watcher and listen for events
                    endProcWatcher = new ManagementEventWatcher(scope, queryString);
                    endProcWatcher.EventArrived += ProcessEnded;
                    endProcWatcher.Start();
                }
            }
            catch (System.Exception ex)
            {
                Log?.Error("[WatchForProcessEnd] exception with: " + ex.Message);
            }
        }

        // jim modify 20240606
        public void WatchForProcessEnd_Stop()
        {
            DdpmCommonHelper.WriteUILog("[ColorViewModel] [WatchForProcessEnd_Stop] WatchForProcessEnd_Stop() Begin");
            /*string queryString =
                "SELECT TargetInstance" +
                "  FROM __InstanceDeletionEvent " +
                "WITHIN 1 " +
                " WHERE TargetInstance ISA 'Win32_Process' "
                + "   AND TargetInstance.Name like 'ColorManagement.exe'";

            string scope = @"\\.\root\CIMV2";

            // Create a watcher and listen for events
            //endProcWatcher = new ManagementEventWatcher(scope, queryString);*/

            if (endProcWatcher != null)
            {
                endProcWatcher.EventArrived -= ProcessEnded;
                endProcWatcher.Stop();
                endProcWatcher.Dispose();
                endProcWatcher = null;
            }
            DdpmCommonHelper.WriteUILog("[ColorViewModel] [WatchForProcessEnd_Stop] WatchForProcessEnd_Stop() end");
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
                    ((StackPanel)(MyModule.GetRightView().FindName("stackpanel_DCM"))).Visibility = Visibility.Collapsed;
                    DCM_Visibility = Visibility.Collapsed;

                    //Dean20250328 mark below code since never use.
                    //if (((UXToggleSwitch)(MyModule.GetRightView().FindName("ColorManagement_ToggleSwitch"))).IsChecked == true)
                    //{
                    //    if (((UXRadioButton)(MyModule.GetRightView().FindName("rb_Colorpreset_based_ICCprofile"))).IsChecked == true)
                    //    {
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
                    //    }
                    //}
                }));
            }
        }

        //If caller is not the same as UI main thread, please using Dispatcher to execute it
        private void PerformLockUnlockUIAction(bool isColorLocked, bool isAutoBriTempLocked)
        {
            MyModule.GetRightView().Dispatcher.Invoke((Action)(() =>
            {
                DdpmCommonHelper.WriteUILog("[ColorViewModel][PerformLockUnlockUIAction] check lock settings");
                ALSConfig cfg = DdpmCommonHelper.DeviceManagerSA?.GetALSFeatureValue(
                    DdpmCommonHelper.ModuleOwner.SelectedHomeDevice.MonitorInfo, ALSFeatureQueryType.All, 0).Result;
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
                    TabNavigation = DefaultTabNavigation; // "Cycle";

                LockMaskVisible = isColorLocked ? Visibility.Visible : Visibility.Collapsed;
            }));
        }

        private void DoWork_RefreshData(object sender, DoWorkEventArgs e)
        {
            try //2024-06-19 Elie, add try catch to get exception.
            {
                //Jason 20250314 add
                BackgroundWorker bwk = (BackgroundWorker)sender;
                
                //check if watcher still alive then stop them
                WatchForProcessStart_Stop();
                WatchForProcessEnd_Stop();

                //check if need to lock color related UI elements
                DDPMSettings data = DdpmCommonHelper.ReadDDPMSettings();
                if (data != null && data.LockSettings != null)
                    PerformLockUnlockUIAction(data.LockSettings.Lock_Display_ColorPreset, data.LockSettings.Lock_Display_AutoBriTemp);
                else
                    DdpmCommonHelper.WriteUILog("[ColorViewModel][DoWork_RefreshData] checked that DDPM or lock setting object is null");

                //check if need to cancel the refresh
                if (Cancelled_RefreshData(e, bwk))
                {
                    return;
                }
                UpdateHDRStatus();
                DdpmCommonHelper.WriteUILog("[ColorViewModel] [DoWork_RefreshData] UpdateHDRStatus() called End");

                // jim 20241207 add and modify for The DDPM color profile can not be applied by DDPM on Smart HDR mode.(Gaming monitor ex: AW2724DM)
                if (MyModule.SelectedHomeDevice.MonitorInfo.CapabilityDic.ContainsKey("F4"))
                    Is_Game_DeviceName = true;

                DdpmCommonHelper.WriteUILog($"[DoWork_RefreshData] Has Gaming Capability={Is_Game_DeviceName}, HDR Status = {SmartHDR_ON}");

                //OSD control back event
                DdpmCommonHelper.DeviceManagerSA.VCPchanged += OnVCPChangedEvent;
                DdpmCommonHelper.DeviceManagerSA.Coloreset_manual_ChangeEvent += OnColoresetManualChangeHandler;
                DdpmCommonHelper.DeviceManagerSA.NightLightStatus_ChangeEvent += OnNightLightStatusChangeHandler;

                // Jim 20250120 add more log
                DdpmCommonHelper.WriteUILog("[ColorViewModel] [DoWork_RefreshData] CheckNightLightStatus() call");
                DdpmCommonHelper.DeviceManagerSA.CheckNightLightStatus();
                // Jim 20250120 add more log
                DdpmCommonHelper.WriteUILog("[ColorViewModel] [DoWork_RefreshData] CheckNightLightScheduler() call");
                DdpmCommonHelper.DeviceManagerSA.CheckNightLightScheduler();

                // Jim 20250120 add more log
                DdpmCommonHelper.WriteUILog("[ColorViewModel] [DoWork_RefreshData] ReadColorPreset() call Begin");
                SupportColorPresets = DdpmCommonHelper.DeviceManagerSA.ReadColorPreset(MyModule.SelectedHomeDevice.MonitorInfo).Result;//cache in user SA           
                if (Cancelled_RefreshData(e, bwk))
                {
                    return;
                }

                DdpmCommonHelper.WriteUILog("[ColorViewModel] [DoWork_RefreshData] ReadCurrentColorPreset() call");
                string curPreset = DdpmCommonHelper.DeviceManagerSA?.ReadCurrentColorPreset(MyModule.SelectedHomeDevice.MonitorInfo, (Guid)guid, Priority.High).Result;               

                if (Cancelled_RefreshData(e, bwk))
                {
                    return;
                }
                // Jim 20250120 add more log
                DdpmCommonHelper.WriteUILog("[ColorViewModel] [DoWork_RefreshData] Sync_ColorPresetName() call");
                string strSync_CurrentColorPreset = string.Empty;
                strSync_CurrentColorPreset = DdpmCommonHelper.DeviceManagerSA?.Sync_ColorPresetName(DdpmCommonHelper.ModuleOwner?.SelectedHomeDevice?.MonitorInfo, curPreset).Result;

                ColorPresets_ItemsCollection = new List<string>();
                MyModule.GetRightView().Dispatcher.Invoke((Action)(() =>
                {
                    foreach (string info in SupportColorPresets)
                    {
                        ColorPresets_ItemsCollection.Add(new string(info));
                    }
                    if (!string.IsNullOrEmpty(strSync_CurrentColorPreset))
                    {
                        int idx = ColorPresets_ItemsCollection.FindIndex(x => x.ToUpper().Equals(strSync_CurrentColorPreset.ToUpper()));
                        if (idx >= 0)
                        {
                            UpdateColorPresetSelectedIndex(idx);
                        }
                    }
                }));
                if (Cancelled_RefreshData(e, bwk))
                {
                    return;
                }
                //Jim, 20240819, Fixed for applist increase repeatedly when change a different Monitor.
                ColorPresetSettings config = get_cur_monitor_preset_config(MyModule.SelectedHomeDevice.MonitorInfo, DdpmCommonHelper.DeviceManagerSA.ReadColorPresetSettings().Result);

                if (Cancelled_RefreshData(e, bwk))
                {
                    return;
                }
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

                    // Jim 20250120 add more log
                    DdpmCommonHelper.WriteUILog("[ColorViewModel] [DoWork_RefreshData] WriteColorPresetSettings() called Begin");
                    if (Cancelled_RefreshData(e, bwk))
                    {
                        return;
                    }
                    DdpmCommonHelper.DeviceManagerSA.WriteColorPresetSettings(Test_AddAppCollectionData.GetInstance()._monitorConfigs);
                    DdpmCommonHelper.WriteUILog("[ColorViewModel] [DoWork_RefreshData] WriteColorPresetSettings() called End");
                    Task.Delay(100).Wait();

                    List<AppData> tempList = new List<AppData>();

                    foreach (string key in config.AppInfo.Keys)
                    {
                        if (Cancelled_RefreshData(e, bwk))
                        {
                            return;
                        }

                        ColorPresetSettings_AppInfo value = config.AppInfo[key];
                        string strColorPresetName = string.Empty;

                        if (SmartHDR_ON)
                        {
                            // Jim 20250120 add more log
                            DdpmCommonHelper.WriteUILog("[ColorViewModel] [DoWork_RefreshData] SmartHDR_ON = true GetColorPresetName() called Begin");
                            if (Cancelled_RefreshData(e, bwk))
                            {
                                return;
                            }
                            strColorPresetName = DdpmCommonHelper.DeviceManagerSA.GetColorPresetName(value.HDRColor).Result;
                            DdpmCommonHelper.WriteUILog("[ColorViewModel] [DoWork_RefreshData] SmartHDR_ON = true GetColorPresetName() called End");
                        }
                        else
                        {
                            // Jim 20250120 add more log
                            DdpmCommonHelper.WriteUILog("[ColorViewModel] [DoWork_RefreshData] SmartHDR_ON = false GetColorPresetName() called Begin");
                            if (Cancelled_RefreshData(e, bwk))
                            {
                                return;
                            }
                            strColorPresetName = DdpmCommonHelper.DeviceManagerSA.GetColorPresetName(value.Color).Result;
                            DdpmCommonHelper.WriteUILog("[ColorViewModel] [DoWork_RefreshData] SmartHDR_ON = false GetColorPresetName() called End");
                        }

                        if (DdpmCommonHelper.ModuleOwner?.SelectedHomeDevice?.MonitorInfo != null)
                        {
                            // Jim 20250120 add more log
                            DdpmCommonHelper.WriteUILog("[ColorViewModel] [DoWork_RefreshData] Sync_ColorPresetName() called Begin");
                            if (Cancelled_RefreshData(e, bwk))
                            {
                                return;
                            }
                            strSync_CurrentColorPreset = DdpmCommonHelper.DeviceManagerSA?.Sync_ColorPresetName(DdpmCommonHelper.ModuleOwner?.SelectedHomeDevice?.MonitorInfo, strColorPresetName).Result;
                            DdpmCommonHelper.WriteUILog("[ColorViewModel] [DoWork_RefreshData] Sync_ColorPresetName() called End");
                        }

                        int pIdx = SupportColorPresets.FindIndex(x => x.Trim() == strSync_CurrentColorPreset.Trim());
                        if (pIdx <= 0)
                            pIdx = 0;

                        Visibility vis = (key.Trim() == "Desktop Application" || key.Trim() == "UWP Application") ? Visibility.Collapsed : Visibility.Visible;
                        if (Cancelled_RefreshData(e, bwk))
                        {
                            return;
                        }
                        if ((key.Trim() == "Desktop Application" || key.Trim() == "UWP Application"))
                        {
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
                            tempList.Add(new_Appdata);
                        }
                    }
                    Test_AddAppCollectionData.GetInstance().AppsList = new ObservableCollection<AppData>(tempList);
                    AppsList = Test_AddAppCollectionData.GetInstance().AppsList;

                    // Jim 20250120 add more log
                    DdpmCommonHelper.WriteUILog("[ColorViewModel] [DoWork_RefreshData] DownloadICCData() called Begin");
                    if (Cancelled_RefreshData(e, bwk))
                    {
                        return;
                    }
                    _ICC_Metadata = DdpmCommonHelper.DeviceManagerSA?.DownloadICCData(DdpmCommonHelper.ModuleOwner?.SelectedHomeDevice?.MonitorInfo, true).Result;
                    DdpmCommonHelper.WriteUILog("[ColorViewModel] [DoWork_RefreshData] DownloadICCData() called End");

                    if (Cancelled_RefreshData(e, bwk))
                    {
                        return;
                    }
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
                if (Cancelled_RefreshData(e, bwk))
                {
                    return;
                }

                // Update UI
                MyModule.GetRightView().Dispatcher.Invoke((Action)(() =>
                {
                    // Jim 20250120 add more log
                    DdpmCommonHelper.WriteUILog("[ColorViewModel] [DoWork_RefreshData] SyncNightlightStatus() called Begin");
                    DdpmCommonHelper.DeviceManagerSA.SyncNightlightStatus();
                    DdpmCommonHelper.WriteUILog("[ColorViewModel] [DoWork_RefreshData] SyncNightlightStatus() called End");

                    if (System.String.IsNullOrEmpty(NightlightStatus))
                        NightlightStatus = Strings.Off;
                    //update_ui_over_runtype(config);
                    RefreshUI();
                }));
            }
            catch (System.Exception ex)
            {
                DdpmCommonHelper.WriteUILog("[ColorViewModel] [DoWork_RefreshData] exception " + ex.ToString());
            }
        }

        private void DoWork_DownloadICCData(object sender, DoWorkEventArgs e)
        {
            try
            {
                ColorPresetSettings config = get_cur_monitor_preset_config(MyModule.SelectedHomeDevice.MonitorInfo, DdpmCommonHelper.DeviceManagerSA.ReadColorPresetSettings().Result);

                _ICC_Metadata = DdpmCommonHelper.DeviceManagerSA?.DownloadICCData(DdpmCommonHelper.ModuleOwner?.SelectedHomeDevice?.MonitorInfo, true).Result;

                MyModule.GetRightView().Dispatcher.Invoke((Action)(() =>
                {
                    update_ui_over_runtype(config);
                    RefreshUI();
                }));
            }
            catch (System.Exception ex)
            {
                DdpmCommonHelper.WriteUILog("[ColorViewModel] [DoWork_RefreshData] exception " + ex.ToString());
            }
        }

        private bool Cancelled_RefreshData(DoWorkEventArgs e, BackgroundWorker bw)
        {
            DdpmCommonHelper.WriteUILog("[ColorViewModel][Cancelled_RefreshData] cancel Begin");
            if (bw != null && bw.CancellationPending)
            {
                DdpmCommonHelper.WriteUILog("[ColorViewModel][Cancelled_RefreshData] set cancel to true");
                e.Cancel = true;
                return true;
            }
            return false;
        }

        public void CallCancel()
        {
            if (bw != null && bw.IsBusy)
            {
                _log?.Info("[InputSource] CallCancel.");
                bw.CancelAsync();
                if (guid != null)
                    DdpmCommonHelper.DeviceManagerSA.CancelVcpTask((Guid)guid);
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
            Trace.WriteLine($"[ColorViewModel] VCPchangedEventArgs e.vcpcode = {e.vcpcode}");

            if (e.vcpcode.Equals("DC") || e.vcpcode.Equals("F0") || e.vcpcode.Equals("14") || e.vcpcode.Equals("E2") || e.vcpcode.Equals("F4")) // Color changes by OSD menu
            {
                string curPreset = DdpmCommonHelper.DeviceManagerSA?.ReadCurrentColorPreset(DdpmCommonHelper.ModuleOwner?.SelectedHomeDevice?.MonitorInfo).Result;
                string strSync_CurrentColorPreset = string.Empty;
                if(string.IsNullOrEmpty(curPreset))
                {
                    DdpmCommonHelper.WriteUILog("[ColorViewModel][OnVCPChangedEvent] curPreset is empty, drop vcp event check");
                    return;
                }
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
            WatchForProcessStart_Stop();
            WatchForProcessEnd_Stop();

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
            }));
        }

        private void OnNightLightStatusChangeHandler(object sender, string e)
        {
            NightlightStatus = e;

            if (NightlightStatus.Equals("On", StringComparison.OrdinalIgnoreCase))
                NightlightStatus = Strings.On;
            else
                NightlightStatus = Strings.Off;

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

                if (e.Result.ToString() == "OK")
                {
                    //Result is passed.
                }
                else
                {
                    //Result is failed.
                }
            }

            // PIMS-288131
            try
            {
                Process[] processes = Process.GetProcessesByName("ColorManagement");

                if (processes != null && processes.Length > 0)
                {
                    // Is running
                    bool blIsPass = true;
                    foreach (Process process in processes)
                    {
                        string filepath = process.MainModule.FileName;

                        if (!System.String.IsNullOrEmpty(filepath))
                        {
                            string Info = "ColorManagement File Signature Is Null Or Empty";
                            if (!DDPMFileSecurity.VerifyExecutableFileSignature(filepath, out Info))
                            {
                                blIsPass = false;
                                string log = $"[RunWorkerCompleted_RefreshData] VerifyExecutableFileSignature : {Info}\n";
                                DdpmCommonHelper.WriteUILog(log);
                                break;
                            }
                        }
                    }

                    if (blIsPass)
                    {
                        MyModule.GetRightView().Dispatcher.Invoke((Action)(() =>
                        {
                            ((Expander)(MyModule.GetRightView().FindName("Expander_Advanced_Settings"))).IsEnabled = false;
                            ((Expander)(MyModule.GetRightView().FindName("Expander_Advanced_Settings"))).IsExpanded = false;

                            ((StackPanel)(MyModule.GetRightView().FindName("stackpanel_DCM"))).Visibility = Visibility.Visible;
                            DCM_Visibility = Visibility.Visible;

                        }));
                    }
                }
            }
            catch (System.Exception ex)
            {
                string log = $"[RunWorkerCompleted_RefreshData] Exception thrown when Process.GetProcessesByName : {ex.Message}\nStack Trace: {ex.StackTrace}";
                DdpmCommonHelper.WriteUILog(log);
            }

            WatchForProcessStart();
            WatchForProcessEnd();
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

                if (e.Result.ToString() == "OK")
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
            OnPropertyChanged("TabNavigation");
            OnPropertyChanged("isTabStoppable");
        }

        private void update_ui_over_runtype(ColorPresetSettings config)
        {
            if (config.RunType == (int)ColorPresetRunType.Auto)
            {
                ((Expander)(MyModule.GetRightView().FindName("Expander_Manual"))).IsExpanded = false;
                ((Expander)(MyModule.GetRightView().FindName("Expander_Auto"))).IsExpanded = false;
                ((Expander)(MyModule.GetRightView().FindName("Expander_Auto"))).IsExpanded = true;
            }
            else
            {
                ((Expander)(MyModule.GetRightView().FindName("Expander_Auto"))).IsExpanded = false;
                ((Expander)(MyModule.GetRightView().FindName("Expander_Manual"))).IsExpanded = false;
                ((Expander)(MyModule.GetRightView().FindName("Expander_Manual"))).IsExpanded = true;
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
                    vis_ad = Visibility.Collapsed;

                IsisAdvanced_Settings = vis_ad;
            }

        }

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

        //Robert_Lin 2025-1-18 added to handle Advanced Settings / ICC profile hylerlink click command
        private ICommand? _ICC_profile_hyperlink_ClickCommand;
        public ICommand ICC_profile_hyperlink_ClickCommand
        {
            get
            {
                if (_ICC_profile_hyperlink_ClickCommand == null)
                {
                    _ICC_profile_hyperlink_ClickCommand = new RelayCommand(Handle_ICC_profile_hyperlink_ClickCommand);
                }
                return _ICC_profile_hyperlink_ClickCommand;
            }
        }
        //Robert_Lin 2025-2-26 to support Narrator, click hyperlink with [Enter] key
        //It's handled by PreviewKeyDown in RightView, so change this method to public
        public void Handle_ICC_profile_hyperlink_ClickCommand()
        {
            var psi = new System.Diagnostics.ProcessStartInfo();

            psi.FileName = "ms-settings:display";
            psi.UseShellExecute = true;

            //System.Diagnostics.Process.Start(psi);
            DDPM.SA.Common.Settings.DDPMFileSecurity.StartProcessSafely(null, psi);
        }
        //Robert_Lin 2025-1-18 added to handle Advanced Settings / ICC profile hylerlink click command
        ////////////////////////////
    }
}