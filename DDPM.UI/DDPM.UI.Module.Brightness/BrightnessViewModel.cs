using CommunityToolkit.Mvvm.ComponentModel;
using DDPM.SA.Common;
using DDPM.SA.Common.Display;
using DDPM.SA.Common.Settings;
using DDPM.UI.Common;
using DDPM.UI.Common.Interfaces;
using DDPM.UI.Common.Models;
using Dell.Client.Framework.Common;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using VcpCore.Common;
using Windows.System;
using static System.Net.Mime.MediaTypeNames;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

[assembly: InternalsVisibleTo("DDPM.UI.Module.Brightness.Tests")]

namespace DDPM.UI.Module.Brightness
{
    internal class BrightnessViewModel : ObservableObject, INotifyPropertyChanged
    {
        private static BrightnessViewModel INSTANCE = null;

        public static BrightnessViewModel GetInstance()
        {
            if (INSTANCE == null)
            {
                INSTANCE = new BrightnessViewModel();
            }
            return INSTANCE;
        }

        private string _TabSTOP = "Cycle";

        public string TabSTOP
        {
            get
            {
                return _TabSTOP;
            }
            set
            {
                _TabSTOP = value;
                NotifyPropertyChanged("TabSTOP");
            }
        }

        private Visibility _LockMaskVisible = Visibility.Collapsed;

        public Visibility LockMaskVisible
        {
            get
            {
                return _LockMaskVisible;
            }
            set
            {
                _LockMaskVisible = value;
                NotifyPropertyChanged("LockMaskVisible");
            }
        }

        private Visibility _synchronizeLock = Visibility.Collapsed;

        public Visibility synchronizeLock
        {
            get
            {
                return _synchronizeLock;
            }
            set
            {
                _synchronizeLock = value;
                NotifyPropertyChanged("synchronizeLock");
            }
        }

        private ImageSource? _BrightnessImage;
        private ImageSource? _ContrastImage;
        private ImageSource? _LuminanceImage;

        private double Luminance_Value = -1;
        private double Brightness_Value = -1;
        private double Contrast_Value = -1;
        private double LuminanceMax_Value = -1;

        //-----------------------------------------//

        private double PR1Brightness_Value = -1;
        private double PR1Contrast_Value = -1;
        private double PR2Brightness_Value = -1;
        private double PR2Contrast_Value = -1;

        private int dUration_1 = -1;
        private int mIns_1 = -1;
        private int hOurs_1 = -1;
        private int dUration_2 = -1;
        private int mIns_2 = -1;
        private int hOurs_2 = -1;

        public Debouncer PR1Brightness_Debouncer;
        public Debouncer PR1Contrast_Debouncer;
        public Debouncer PR2Brightness_Debouncer;
        public Debouncer PR2Contrast_Debouncer;

        private string PR1_Name = string.Empty;
        private string PR2_Name = string.Empty;

        public bool IsPR1Preview_ = false;
        public bool IsPR2Preview_ = false;

        public string PR1ButtonContent { get; set; } = "Preview Changes";
        public string PR2ButtonContent { get; set; } = "Preview Changes";

        public CancellationTokenSource PreviewToken = new CancellationTokenSource();

        public List<scheduleInfo> ScheduleMaps = new List<scheduleInfo>();

        public bool IsMouseEnterSchedule_1 { get; set; }
        public bool IsMouseEnterSchedule_2 { get; set; }

        public void UpdataScheduleBoaderUI()
        {
            NotifyPropertyChanged(nameof(IsMouseEnterSchedule_1));
            NotifyPropertyChanged(nameof(IsMouseEnterSchedule_2));
        }

        public List<int> hOurs { get; } = Enumerable.Range(1, 12).ToList();
        public List<int> mIns { get; } = Enumerable.Range(0, 60).ToList();
        public List<int> dUration { get; } = new List<int>() { 0, 15, 30, 45, 60 };

        //-----------------------------------------//

        private bool IsSynchronizeMonitor = false;
        private bool IsGetSynchronizeMonitor = false;
        private bool IsBrightnessEnable = false;

        public Debouncer Brightness_Debouncer;
        public Debouncer Contrast_Debouncer;
        public Debouncer Luminance_Debouncer;

        public BrightnessModule MyModule { get; set; }

        public ALSConfig Start_ALSConfig = new ALSConfig();
        internal HomeDevice? SelectedHomeDevice { get; set; }

        public new event PropertyChangedEventHandler? PropertyChanged;

        public ImageSource? BrightnessImage
        {
            get => _BrightnessImage;
            set => SetProperty(ref _BrightnessImage, value);
        }

        public ImageSource? ContrastImage
        {
            get => _ContrastImage;
            set => SetProperty(ref _ContrastImage, value);
        }

        public ImageSource? LuminanceImage
        {
            get => _LuminanceImage;
            set => SetProperty(ref _LuminanceImage, value);
        }

        public string IsSynchronize_String
        {
            get
            {
                return IsSynchronize ? "ON" : "OFF";
            }
        }

        public bool BrightnessEnable
        {
            get
            {
                NotifyPropertyChanged(nameof(BrightnessOpacity));
                NotifyPropertyChanged(nameof(GreayoutAlart));
                return IsBrightnessEnable;
            }
        }

        public string BrightnessOpacity
        {
            get
            {
                if (IsBrightnessEnable)
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
                if (IsBrightnessEnable)
                {
                    return Visibility.Collapsed;
                }
                return Visibility.Visible;
            }
        }

        #region hotkey property

        private string _brightnessMinsKey = "None";

        public string BrightnessMinsKey
        {
            get => _brightnessMinsKey;
            set
            {
                SetProperty(ref _brightnessMinsKey, value);
                //OnPropertyChanged("BrightnessMinsKey");
                NotifyPropertyChanged("BrightnessMinsKey");
            }
        }

        private string _brightnessAddKey = "None";

        public string BrightnessAddKey
        {
            get => _brightnessAddKey;
            set
            {
                SetProperty(ref _brightnessAddKey, value);
                //OnPropertyChanged("BrightnessAddKey");
                NotifyPropertyChanged("BrightnessAddKey");
            }
        }

        private string _contrastMinsKey = "None";

        public string ContrastMinsKey
        {
            get => _contrastMinsKey;
            set
            {
                SetProperty(ref _contrastMinsKey, value);
                //OnPropertyChanged("ContrastMinsKey");
                NotifyPropertyChanged("ContrastMinsKey");
            }
        }

        private string _contrastAddKey = "None";

        public string ContrastAddKey
        {
            get => _contrastAddKey;
            set
            {
                SetProperty(ref _contrastAddKey, value);
                //OnPropertyChanged("ContrastAddKey");
                NotifyPropertyChanged("ContrastAddKey");
            }
        }

        private string _luminanceMinsKey = "None";

        public string LuminanceMinsKey
        {
            get => _luminanceMinsKey;
            set
            {
                SetProperty(ref _luminanceMinsKey, value);
                //OnPropertyChanged("LuminanceMinsKey");
                NotifyPropertyChanged("LuminanceMinsKey");
            }
        }

        private string _luminanceAddKey = "None";

        public string LuminanceAddKey
        {
            get => _luminanceAddKey;
            set
            {
                SetProperty(ref _luminanceAddKey, value);
                //OnPropertyChanged("LuminanceAddKey");
                NotifyPropertyChanged("LuminanceAddKey");
            }
        }

        #endregion hotkey property

        public void Invoke_RefreshHotkeySettings()
        {
            BackgroundWorker bw = new BackgroundWorker()
            {
                WorkerReportsProgress = false,
                WorkerSupportsCancellation = false
            };
            bw.DoWork += DoWork_RefreshData;
            bw.RunWorkerCompleted += RunWorkerCompleted_RefreshData;
            bw.RunWorkerAsync(ApartmentState.STA);
        }

        private void DoWork_RefreshData(object sender, DoWorkEventArgs e)
        {
            HotkeySettings curHotkey = DdpmCommonHelper.DeviceManagerSA.ReadCurrentHotkey(this.SelectedHomeDevice.MonitorInfo.edid).Result;
            string swHortcutText = string.Empty;

            if (curHotkey.HotkeyInfo.Count > 0)
            {
                foreach (var hotkeyInfo in curHotkey.HotkeyInfo)
                {
                    List<VirtualKey> hotkeys = hotkeyInfo.Hotkey;
                    switch (hotkeyInfo.Job)
                    {
                        case HotkeyType.BrightnessReduce:
                            KeysHelper.ReSetHotKeyText(ref swHortcutText, ref hotkeys);
                            hotkeys.Clear();
                            BrightnessMinsKey = swHortcutText;
                            break;

                        case HotkeyType.BrightnessIncrease:
                            KeysHelper.ReSetHotKeyText(ref swHortcutText, ref hotkeys);
                            hotkeys.Clear();
                            BrightnessAddKey = swHortcutText;
                            break;

                        case HotkeyType.ContrastReduce:
                            KeysHelper.ReSetHotKeyText(ref swHortcutText, ref hotkeys);
                            hotkeys.Clear();
                            ContrastMinsKey = swHortcutText;
                            break;

                        case HotkeyType.ContrastIncrease:
                            KeysHelper.ReSetHotKeyText(ref swHortcutText, ref hotkeys);
                            hotkeys.Clear();
                            ContrastAddKey = swHortcutText;
                            break;

                        case HotkeyType.LuminanceReduce:
                            KeysHelper.ReSetHotKeyText(ref swHortcutText, ref hotkeys);
                            hotkeys.Clear();
                            LuminanceMinsKey = swHortcutText;
                            break;

                        case HotkeyType.LuminanceIncrease:
                            KeysHelper.ReSetHotKeyText(ref swHortcutText, ref hotkeys);
                            hotkeys.Clear();
                            LuminanceAddKey = swHortcutText;
                            break;
                    }
                }
            }
        }

        private void RunWorkerCompleted_RefreshData(object sender, RunWorkerCompletedEventArgs e)
        {
            //Handling the result and final process
            Debug.WriteLine("RefreshHotkeySettings done");
        }

        private void OnHDRChangedEvent(object? sender, EventManagerArgs e)
        {
            IsBrightnessEnable = !(bool)e.Tag;
            NotifyPropertyChanged(nameof(BrightnessEnable));
        }

        public void UpdateHDRStatus()
        {
            bool HDRStatus = DdpmCommonHelper.DeviceManagerSA.GetHDRStatus(MyModule.SelectedHomeDevice.MonitorInfo).Result;
            IsBrightnessEnable = !HDRStatus;
            NotifyPropertyChanged(nameof(BrightnessEnable));
        }

        /// <summary>
        /// Catch OSD menu event
        /// </summary>
        /// <param name="sender">object type</param>
        /// <param name="e">changed event</param>
        private void OnVCPChangedEvent(object? sender, VCPchangedEventArgs e)
        {
            if (e.vcpcode.Equals("66"))//ALS changes by OSD menu
            {
                MonitorInfo? mo = DdpmCommonHelper.ModuleOwner?.SelectedHomeDevice?.MonitorInfo;
                if (mo == null)
                    return;

                //1.check if the same as active monitor
                if (SelectedHomeDevice == null || SelectedHomeDevice.MonitorInfo == null)
                    return;
                if (!SelectedHomeDevice.MonitorInfo.DisplayName.ToUpper().Equals(mo.DisplayName.ToUpper()))
                    return;
                //if (!SelectedHomeDevice.MonitorInfo.DisplayName.Equals(mo.DisplayName))//0614 add
                //    return;

                List<ALSConfig> tmp = DdpmCommonHelper.DeviceManagerSA.GetAllExistAlsConfig().Result;
                if (tmp == null || tmp.Count == 0)
                    return;

                int idx = tmp.FindIndex(x => x.DisplayName.Equals(mo.DisplayName));
                if (idx < 0)
                    return;

                Start_ALSConfig = tmp[idx];

                //2.if yes, then update the vcp value to each option
                GetALSContentAndSyncUI(SelectedHomeDevice.MonitorInfo);
            }
            else if (e.vcpcode.Equals("10"))
            {
                MonitorInfo? mo = DdpmCommonHelper.ModuleOwner?.SelectedHomeDevice?.MonitorInfo;
                if (mo == null || (!(e.monitor.Equals(mo))))
                    return;
                //check if the same as active monitor and same vcp changed monitor
                if (SelectedHomeDevice == null || SelectedHomeDevice.MonitorInfo == null)
                    return;
                if (!SelectedHomeDevice.MonitorInfo.DisplayName.ToUpper().Equals(mo.DisplayName.ToUpper()))
                    return;
                if (!SelectedHomeDevice.MonitorInfo.DisplayName.ToUpper().Equals(e.monitor.DisplayName.ToUpper()))
                    return;

                Luminance_Value = Convert.ToDouble(e.value);
                Brightness_Value = Convert.ToDouble(e.value);

                NotifyPropertyChanged("LuminanceValue");
                NotifyPropertyChanged("BrightnessValue");
            }
            else if (e.vcpcode.Equals("12"))
            {
                MonitorInfo? mo = DdpmCommonHelper.ModuleOwner?.SelectedHomeDevice?.MonitorInfo;
                if (mo == null || (!(e.monitor.Equals(mo))))
                    return;
                //check if the same as active monitor and same vcp changed monitor
                if (SelectedHomeDevice == null || SelectedHomeDevice.MonitorInfo == null)
                    return;
                if (!SelectedHomeDevice.MonitorInfo.DisplayName.ToUpper().Equals(mo.DisplayName.ToUpper()))
                    return;
                if (!SelectedHomeDevice.MonitorInfo.DisplayName.ToUpper().Equals(e.monitor.DisplayName.ToUpper()))
                    return;

                Contrast_Value = Convert.ToDouble(e.value);

                NotifyPropertyChanged("ContrastValue");
            }
        }

        ~BrightnessViewModel()
        {
            if (DdpmCommonHelper.DeviceManagerSA != null)
                DdpmCommonHelper.DeviceManagerSA.VCPchanged -= OnVCPChangedEvent;
        }

        public BrightnessViewModel()
        {
            isNormalBrightness = Visibility.Collapsed;
            isAlsSupported = Visibility.Collapsed;
            isLuminanceSupport = Visibility.Collapsed;
            isScheduleSupport = Visibility.Collapsed;

            //Task.Run(() => {
            //IsBusy = true;
            BrightnessImage = DdpmCommonHelper.GetImageSourceFromCommonResource("Resources/Brightness.png");
            ContrastImage = DdpmCommonHelper.GetImageSourceFromCommonResource("Resources/Contrast.png");
            LuminanceImage = DdpmCommonHelper.GetImageSourceFromCommonResource("Resources/Luminance.png");

            SelectedHomeDevice = DdpmCommonHelper.ModuleOwner.SelectedHomeDevice;
            Contrast_Debouncer = new Debouncer(1000, Set_Contrast_Value);
            Brightness_Debouncer = new Debouncer(1000, Set_Brightness_Value);
            Luminance_Debouncer = new Debouncer(1000, Set_Luminance_Value);

            PR1Contrast_Debouncer = new Debouncer(1000, Set_Contrast_Value);
            PR1Brightness_Debouncer = new Debouncer(1000, Set_Brightness_Value);
            PR2Contrast_Debouncer = new Debouncer(1000, Set_Contrast_Value);
            PR2Brightness_Debouncer = new Debouncer(1000, Set_Brightness_Value);

            DdpmCommonHelper.MyConsole.RegisterForEvent("DisplayHDRStatusChanged", OnHDRChangedEvent);
        }

        private void RefreshUI()
        {
            NotifyPropertyChanged("isNormalBrightness");
            NotifyPropertyChanged("isLuminanceSupport");
            NotifyPropertyChanged("isAlsSupported");
            NotifyPropertyChanged("isScheduleSupport");

            if (isLuminanceSupport == Visibility.Visible)
            {
                NotifyPropertyChanged("LuminanceValue");
                NotifyPropertyChanged("LuminanceMaxValue");
            }
            else
            {
                NotifyPropertyChanged("BrightnessValue");
                NotifyPropertyChanged("ContrastValue");
                NotifyPropertyChanged("IsSynchronize");
            }

            if (isAlsSupported == Visibility.Visible)
            {
                Update_AutoBrightnessStatus(Start_ALSConfig.isAutoBrightness);
                Update_AutoColorTempStatus(Start_ALSConfig.isAutoColorTemp);
                Update_PrimaryMonitorSyncStatus(Start_ALSConfig.isPrimaryMonitorSync);
                Update_AutoBrightnessRangeLevelStatus(Start_ALSConfig.AutoBrightnessRangeLevel);
                Update_SupportedPrimaryMonitorSync(Start_ALSConfig.isAutoBrightness, Start_ALSConfig.isAutoColorTemp);
            }
        }

        private bool _isSynchronizeDisabled;

        public bool IsSynchronizeDisabled
        {
            get { return _isSynchronizeDisabled; }
            set
            {
                _isSynchronizeDisabled = value;
                OnPropertyChanged(nameof(IsSynchronizeDisabled));
            }
        }

        private void InitComponentData()
        {
            Trace.WriteLine($"1. {DateTime.Now.ToString("MM/dd/yyyy hh:mm ss fff")}");
            if (SelectedHomeDevice == null)
            {
                IsBusy = false;
                NotifyPropertyChanged("IsBusy");
                return;
            }

            Trace.WriteLine($"2. {DateTime.Now.ToString("MM/dd/yyyy hh:mm ss fff")}");
            if (DdpmCommonHelper.DeviceManagerSA != null)
            {
                if (DdpmCommonHelper.ModuleOwner != null)
                    isShowSynchronize = DdpmCommonHelper.ModuleOwner.HomeDevices.Count > 1 ? Visibility.Visible : Visibility.Collapsed;
                else
                    isShowSynchronize = Visibility.Collapsed;
                NotifyPropertyChanged("isShowSynchronize");

                Trace.WriteLine($"3. {DateTime.Now.ToString("MM/dd/yyyy hh:mm ss fff")}");
                isLuminanceSupport = Visibility.Collapsed;

                List<string> strings = new List<string>();
                if (!SelectedHomeDevice.MonitorInfo.CapabilityDic.ContainsKey("12"))
                {
                    isLuminanceSupport = Visibility.Visible;
                    //string Capabilities_STR = DdpmCommonHelper.DeviceManagerSA.GetVCPCapabilities(SelectedHomeDevice.MonitorInfo).Result;
                    //Trace.WriteLine($"4. {DateTime.Now.ToString("MM/dd/yyyy hh:mm ss fff")}");
                    //if (!string.IsNullOrEmpty(Capabilities_STR))
                    //{
                    //    var Capabilities = (JObject)JsonConvert.DeserializeObject(Capabilities_STR);
                    //    if (Capabilities.ContainsKey("CapsDataMap"))
                    //    {
                    //        var CapsDataMap = (JObject)Capabilities["CapsDataMap"];
                    //        if (!CapsDataMap.ContainsKey("Contrast"))
                    //            isLuminanceSupport = Visibility.Visible;
                    //    }
                    //}
                }

                Trace.WriteLine($"5. {DateTime.Now.ToString("MM/dd/yyyy hh:mm ss fff")}");
                //Check synchronize setting
                if (isLuminanceSupport != Visibility.Visible)
                {
                    Get_Synchronize();
                }
                Trace.WriteLine($"6. {DateTime.Now.ToString("MM/dd/yyyy hh:mm ss fff")}");
                //Check if support ALS
                if (SelectedHomeDevice.MonitorInfo.CapabilityDic.ContainsKey("66"))
                {
                    if (SelectedHomeDevice != null && SelectedHomeDevice.MonitorInfo != null)
                    {
                        //fixed releate PIMS-287891
                        List<ALSConfig> alsList = new List<ALSConfig>();
                        alsList = DdpmCommonHelper.DeviceManagerSA.GetAllExistAlsConfig().Result;
                        if (alsList.Count > 0) // Check Start_ALSConfig whether exist
                        {
                            for (int i = 0; i < alsList.Count; i++)
                            {
                                if (alsList[i].DisplayName == SelectedHomeDevice.MonitorInfo.DisplayName && alsList[i].serialNumber == SelectedHomeDevice.MonitorInfo.edid.SerialNumber)
                                {
                                    Start_ALSConfig = alsList[i];
                                }
                            }
                        }
                        //Re-Get Start_ALSConfig
                        if (Start_ALSConfig.AllValue == 0)//Need to Re-Get value
                            Start_ALSConfig = DdpmCommonHelper.DeviceManagerSA.GetALSFeatureValue(SelectedHomeDevice.MonitorInfo, ALSFeatureQueryType.All, 0).Result;
                        GetALSContentAndSyncUI(SelectedHomeDevice.MonitorInfo);
                        CheckisShowSynchronize(alsList);
                    }
                }

                //OSD control back event
                DdpmCommonHelper.DeviceManagerSA.VCPchanged += OnVCPChangedEvent;
                Trace.WriteLine($"7 {DateTime.Now.ToString("MM/dd/yyyy hh:mm ss fff")}");

                //Lock/unlock mask and tabstop init here
                DDPMSettings data = DdpmCommonHelper.ReadDDPMSettings();// DeviceManagerSA.ReloadAppConfigData().Result;//Be careful if spend much time here
                Update_ALSLockStatus(data.LockSettings.Lock_Display_AutoBriTemp);
                Update_BriContLockStatus(data.LockSettings.Lock_Display_BriCont);
                Update_SyncLockStatus((data.LockSettings.Lock_Display_BriCont || data.LockSettings.Lock_Display_ColorPreset || data.LockSettings.Lock_Display_AutoBriTemp));
                //ex: vm.LockMaskVisible = data.LockSettings.Lock_Display_BriCont ? Visibility.Visible : Visibility.Collapsed;
                //Read user default lock value, these values are synced from IT lock event
                Trace.WriteLine($"[SettingsPage] Apply Brightness/Contrast(Lock) : {data.LockSettings.Lock_Display_BriCont}");
                Trace.WriteLine($"[SettingsPage] Apply Auto Brightness(Lock) : {data.LockSettings.Lock_Display_AutoBriTemp}");
                Trace.WriteLine($"[SettingsPage] Apply Synchroniz Button(Lock) : {(data.LockSettings.Lock_Display_BriCont || data.LockSettings.Lock_Display_ColorPreset || data.LockSettings.Lock_Display_AutoBriTemp)}");
            }
        }

        public void CheckisShowSynchronize(List<ALSConfig> alsSynchronizeList)//PIMS-285802 PIMS-285804
        {
            if (SelectedHomeDevice == null || DdpmCommonHelper.DeviceManagerSA == null)
                return;

            //Re-Check isShowSynchronize
            if (DdpmCommonHelper.ModuleOwner.HomeDevices.Count > 1)//Only check if there is more than one monitor.
            {
                if (alsSynchronizeList.Count == 0)
                {
                    alsSynchronizeList = DdpmCommonHelper.DeviceManagerSA.GetAllExistAlsConfig().Result;
                }
                //int _isMutliAlsMonitorCount = 0;
                //bool _isAlSON = true;
                //foreach (var al in alsSynchronizeList)//ALS monitor count
                //{
                //    if (al.isSupportALS == 2)
                //        _isMutliAlsMonitorCount++;
                //    if (al.isAutoBrightness == true || al.isAutoColorTemp == true)
                //        _isAlSON = true;
                //}
                //It is mean over 2 monitors.
                else if (alsSynchronizeList.Count == 2)//Test case for 2 monitors
                {
                    //25 Test Scenario : 2 same monitors with ALS Function
                    if (alsSynchronizeList[0].ModelName == alsSynchronizeList[1].ModelName)
                    {
                        if (alsSynchronizeList[0].isSupportALS == 2 && alsSynchronizeList[1].isSupportALS == 2)
                        {
                            if (CheckALSOnOff(alsSynchronizeList) == false)
                            {
                                SynchronizeBtnExpectedResult("C");
                                return;
                            }
                            else
                            {
                                SynchronizeBtnExpectedResult("D");
                                return;
                            }
                        }
                    }
                    //26 Test Scenario : 2 different monitors with ALS Function
                    if (alsSynchronizeList[0].ModelName != alsSynchronizeList[1].ModelName)
                    {
                        if (alsSynchronizeList[0].isSupportALS == 2 && alsSynchronizeList[1].isSupportALS == 2)
                        {
                            if (CheckALSOnOff(alsSynchronizeList) == false)
                            {
                                SynchronizeBtnExpectedResult("C");
                                return;
                            }
                            else
                            {
                                SynchronizeBtnExpectedResult("D");
                                return;
                            }
                        }
                    }
                    //13 Test Scenario : 2 same UP series monitors
                    if (alsSynchronizeList[0].ModelName.Contains("UP") && alsSynchronizeList[1].ModelName.Contains("UP"))
                    {
                        if (alsSynchronizeList[0].ModelName == alsSynchronizeList[1].ModelName)
                        {
                            SynchronizeBtnExpectedResult("A");
                            return;
                        }
                    }
                    //14 Test Scenario : 2 same non UP series monitors without ALS function
                    if (!alsSynchronizeList[0].ModelName.Contains("UP") && !alsSynchronizeList[1].ModelName.Contains("UP"))
                    {
                        if (alsSynchronizeList[0].ModelName == alsSynchronizeList[1].ModelName)
                        {
                            if (alsSynchronizeList[0].isSupportALS == 0 && alsSynchronizeList[1].isSupportALS == 0)
                            {
                                //Expected Result B:
                                SynchronizeBtnExpectedResult("B");
                                return;
                            }
                        }
                    }
                    //15 Test Scenario : 2 different UP series monitors
                    if (alsSynchronizeList[0].ModelName.Contains("UP") && alsSynchronizeList[1].ModelName.Contains("UP"))
                    {
                        if (alsSynchronizeList[0].ModelName != alsSynchronizeList[1].ModelName)
                        {
                            //Expected Result E.
                            SynchronizeBtnExpectedResult("E");
                            return;
                        }
                    }
                    //16 Test Scenario : 2 different Non UP series monitors without ALS function
                    if (!alsSynchronizeList[0].ModelName.Contains("UP") && !alsSynchronizeList[1].ModelName.Contains("UP"))
                    {
                        if (alsSynchronizeList[0].isSupportALS == 0 && alsSynchronizeList[1].isSupportALS == 0)
                        {
                            //Expected Result B.
                            SynchronizeBtnExpectedResult("B");
                            return;
                        }
                    }
                    //17 Test Scenario : UP monitor and Non UP series monitor without ALS function
                    if ((alsSynchronizeList[0].ModelName.Contains("UP") || alsSynchronizeList[1].ModelName.Contains("UP")) && (!alsSynchronizeList[0].ModelName.Contains("UP") || !alsSynchronizeList[1].ModelName.Contains("UP")))
                    {
                        if (alsSynchronizeList[0].isSupportALS == 0 && alsSynchronizeList[1].isSupportALS == 0)
                        {
                            //"Synchronize between monitors" is NOT displayed.
                            SynchronizeBtnExpectedResult("E");
                            return;
                        }
                    }
                    //18 Test Scenario : UP monitor and ALS function monitor
                    if (alsSynchronizeList[0].ModelName.Contains("UP") || alsSynchronizeList[1].ModelName.Contains("UP"))
                    {
                        if ((alsSynchronizeList[0].isSupportALS == 2) ^ (alsSynchronizeList[1].isSupportALS == 2))
                        {
                            //"Synchronize between monitors" is NOT displayed.
                            SynchronizeBtnExpectedResult("E");
                            return;
                        }
                    }
                    //else
                    //{
                    //19 Test Scenario : G series monitor and S series monitor
                    //20 Test Scenario : AW series Freesync monitor and U series monitor
                    //21 Test Scenario : C series, SE series, E series and P series monitors
                    //Expected Result B:
                    SynchronizeBtnExpectedResult("B");
                    return;
                    //}
                }
                else if (alsSynchronizeList.Count == 3)//Test case for 3 monitors
                {
                    //0x12 = non Luminance
                    //22 Test Scenario : 2 monitors with Brightness/Contrast and 1 monitor with Luminance
                    if (CheckLuminanceMonitorCount() == 2)
                    {
                        ObjGetVCP obj = DdpmCommonHelper.DeviceManagerSA.GetVCPCapability(SelectedHomeDevice.MonitorInfo, 0x12, 0).Result;
                        if (obj.result)
                        {
                            //Result same as Expected Result B and not apply to DUT3.
                            SynchronizeBtnExpectedResult("B");
                            return;
                        }
                        else
                        {
                            //"Synchronize between monitors" is NOT displayed.
                            SynchronizeBtnExpectedResult("E");
                            return;
                        }
                    }
                    //23 Test Scenario : 1 monitor with Brightness/Contrast and 2 monitors with Luminance
                    if (CheckLuminanceMonitorCount() == 1)
                    {
                        ObjGetVCP obj = DdpmCommonHelper.DeviceManagerSA.GetVCPCapability(SelectedHomeDevice.MonitorInfo, 0x12, 0).Result;
                        if (obj.result)
                        {
                            //Result same as Expected Result B and not apply to DUT3.
                            SynchronizeBtnExpectedResult("A");
                            return;
                        }
                        else
                        {
                            //"Synchronize between monitors" is NOT displayed.
                            SynchronizeBtnExpectedResult("E");
                            return;
                        }
                    }
                    //27 Test Scenario : 1 ALS monitor(ALS = ON) and 2 non ALS monitors
                    //28 Test Scenario : 1 ALS monitor(ALS = OFF) and 2 non ALS monitors
                    if (CheckALSMonitorCount(alsSynchronizeList) == 1)
                    {
                        if (Start_ALSConfig.isSupportALS == 2)
                        {
                            if (Start_ALSConfig.isAutoBrightness == true || Start_ALSConfig.isAutoColorTemp == true)
                            {
                                //a) DUT3 is monitor with ALS function.
                                //"Synchronize between monitors" is displayed on DUT3 but greyed out.
                                SynchronizeBtnExpectedResult("D");
                                return;
                            }
                            else
                            {
                                SynchronizeBtnExpectedResult("B");
                                return;
                            }
                        }
                        else
                        {
                            //b) DUT1 and DUT2 are monitors without ALS function.
                            SynchronizeBtnExpectedResult("B");
                            return;
                        }
                    }
                    //29 Test Scenario : 2 ALS monitors(ALS = ON) and 1 non ALS monitor
                    //30 Test Scenario : 2 ALS monitors(ALS = OFF) and 1 non ALS monitor
                    if (CheckALSMonitorCount(alsSynchronizeList) == 2)
                    {
                        if (CheckALSOnOff(alsSynchronizeList))
                        {
                            //a) DUT1 and DUT2 are monitors with ALS function.
                            //"Synchronize between monitors" is displayed on DUT1 and DUT2 but greyed out.
                            SynchronizeBtnExpectedResult("D");
                            return;
                        }
                        else
                        {
                            //"Synchronize between monitors" is displayed.no greyed out.
                            SynchronizeBtnExpectedResult("B");
                            return;
                        }
                    }
                }
                else//Test case for 4 monitors
                {
                    //24 Test Scenario : 2 monitors with Brightness/Contrast and 2 monitors with Luminance
                    if (CheckLuminanceMonitorCount() == 2)
                    {
                        ObjGetVCP obj = DdpmCommonHelper.DeviceManagerSA.GetVCPCapability(SelectedHomeDevice.MonitorInfo, 0x12, 0).Result;
                        if (obj.result)
                        {
                            //Result same as Expected Result B and not apply to DUT3.
                            SynchronizeBtnExpectedResult("B");
                            return;
                        }
                        else
                        {
                            //"Synchronize between monitors" is NOT displayed.
                            SynchronizeBtnExpectedResult("A");
                            return;
                        }
                    }
                    //31 Test Scenario : 2 ALS monitors(ALS = ON) and 2 non ALS monitor
                    if (CheckALSMonitorCount(alsSynchronizeList) == 2)
                    {
                        //d) Turn on ALS Function on DUT1 and DUT2. Go to Software > Brightness / Contrast > Auto > Turn On Auto Brightness / Auto Color Temperature.
                        if (CheckALSOnOff(alsSynchronizeList))
                        {
                            //"Synchronize between monitors" is displayed but greyed out on both DUT1 and DUT2.
                            SynchronizeBtnExpectedResult("D");
                            return;
                        }
                        else
                        {
                            SynchronizeBtnExpectedResult("B");
                            return;
                        }
                    }
                    //32 Test Scenario : 4 same non-UP models without ALS function
                    //33 Test Scenario : 4 same UP models
                    if (CheckALSMonitorCount(alsSynchronizeList) == 0)
                    {
                        //"Synchronize between monitors" is displayed and not greyed out with default is OFF.
                        //"Synchronize between monitors" is displayed and not greyed out with default is OFF.
                        SynchronizeBtnExpectedResult("B");
                        return;
                    }
                }
            }
            else//It is mean only 1 monitors.
            {
                //3 Test Scenario : Non UP series Monitor does not support ALS
                if (!alsSynchronizeList[0].ModelName.Contains("UP") && alsSynchronizeList[0].isSupportALS == 0)
                {
                    //Make sure "Synchronize between monitors" is NOT displayed on both Manual and Schedule.
                    SynchronizeBtnExpectedResult("E");
                    return;
                }
                //4 Test Scenario : Monitor support ALS
                if (alsSynchronizeList[0].isSupportALS == 2)
                {
                    //Make sure "Synchronize between monitors" is NOT displayed.
                    SynchronizeBtnExpectedResult("E");
                    return;
                }
                //5 Test Scenario : UP series Monitor
                if (alsSynchronizeList[0].ModelName.Contains("UP"))
                {
                    //Make sure "Synchronize between monitors" is NOT displayed on both Manual and Schedule.
                    SynchronizeBtnExpectedResult("E");
                    return;
                }
            }
            SynchronizeBtnExpectedResult("default");
        }

        /// <summary>
        ///  Check the number of Luminance Monitor.0x12 = non Luminance
        /// </summary>
        /// <returns>Return Luminance count</returns>
        private int CheckLuminanceMonitorCount()
        {
            int _isLuminanceCount = 0;
            if (ModuleOwner != null)
            {
                foreach (HomeDevice hd in ModuleOwner!.HomeDevices!)
                {
                    if (hd.MonitorInfo!.CapabilityDic.ContainsKey("12"))
                    {
                        _isLuminanceCount++;
                    }
                }
            }
            return _isLuminanceCount;
        }

        /// <summary>
        /// Check if AutoBrightness/AutoColorTemp is enabled.
        /// </summary>
        /// <param name="aLSList">ALS value list</param>
        /// <returns>Return true or false</returns>
        private bool CheckALSOnOff(List<ALSConfig> aLSList)
        {
            bool _isAlSON = false;
            foreach (var als in aLSList)
            {
                if (als.isAutoBrightness == true || als.isAutoColorTemp == true)
                {
                    _isAlSON = true;
                    break;
                }
            }
            return _isAlSON;
        }

        /// <summary>
        /// Check the number of ALS Monitor.
        /// </summary>
        /// <param name="aLSList">ALS value list</param>
        /// <returns>Return ALS Monitor count</returns>
        private int CheckALSMonitorCount(List<ALSConfig> aLSList)
        {
            int _isMutliAlsMonitorCount = 0;
            foreach (var als in aLSList)
            {
                if (als.isSupportALS == 2)
                    _isMutliAlsMonitorCount++;
            }
            return _isMutliAlsMonitorCount;
        }

        ///<summary>
        ///Expected Result A: "Synchronize between monitors" is displayed and not greyed out with default is OFF.
        ///Expected Result B: "Synchronize between monitors" is displayed and not greyed out with default is OFF.
        ///Expected Result C: "Synchronize between monitors" is displayed and default is OFF.
        ///Expected Result D: "Synchronize between monitors" is displayed but greyed out.
        ///Expected Result E:  "Synchronize between monitors" is NOT displayed.
        ///</summary>
        private void SynchronizeBtnExpectedResult(string str)
        {
            switch (str)
            {
                case "A":
                case "B":
                case "C":
                    IsSynchronizeDisabled = false;
                    isShowSynchronize = Visibility.Visible;
                    break;

                case "D":
                    IsSynchronizeDisabled = true;
                    isShowSynchronize = Visibility.Visible;
                    break;

                case "E":
                    isShowSynchronize = Visibility.Collapsed;
                    break;

                default:
                    isShowSynchronize = Visibility.Visible;
                    break;
            };
            NotifyPropertyChanged("isShowSynchronize");
            NotifyPropertyChanged("IsSynchronizeDisabled");
        }

        public void Invoke_RefreshBrightnessPage()
        {
            BackgroundWorker bw = new BackgroundWorker()
            {
                WorkerReportsProgress = false,
                WorkerSupportsCancellation = false
            };
            bw.DoWork += DoWork_RefreshBrightnessPage;
            bw.RunWorkerCompleted += RunWorkerCompleted_RefreshBrightnessPage;
            bw.RunWorkerAsync();
            IsBusy = true;
        }

        private void DoWork_RefreshBrightnessPage(object sender, DoWorkEventArgs e)
        {
            InitComponentData();
        }

        private void RunWorkerCompleted_RefreshBrightnessPage(object sender, RunWorkerCompletedEventArgs e)
        {
            MyModule.GetRightView().Dispatcher.Invoke((Action)(() =>
            {
                RefreshUI();
                IsBusy = false;
                NotifyPropertyChanged("IsBusy");
            }));

            Invoke_RefreshHotkeySettings();
            Invoke_RefreshManualValue();

            UpdateHDRStatus();
        }

        public void Invoke_RefreshManualValue()
        {
            BackgroundWorker bw = new BackgroundWorker()
            {
                WorkerReportsProgress = false,
                WorkerSupportsCancellation = false
            };
            bw.DoWork += DoWork_RefreshManualValue;
            bw.RunWorkerCompleted += RunWorkerCompleted_RefreshManualValue;
            bw.RunWorkerAsync();
        }

        private void DoWork_RefreshManualValue(object sender, DoWorkEventArgs e)
        {
            if (isLuminanceSupport == Visibility.Visible)
                UpdateLuminance();
            else
                UpdateBrightnessContrast();
        }

        private void RunWorkerCompleted_RefreshManualValue(object sender, RunWorkerCompletedEventArgs e)
        {
            MyModule.GetRightView().Dispatcher.Invoke((Action)(() =>
            {
                if (isLuminanceSupport == Visibility.Visible)
                {
                    NotifyPropertyChanged("LuminanceValue");
                    NotifyPropertyChanged("LuminanceMaxValue");
                }
                else
                {
                    NotifyPropertyChanged("BrightnessValue");
                    NotifyPropertyChanged("ContrastValue");
                }
            }));

            Invoke_RefreshScheduleValue();
        }

        public void Invoke_RefreshScheduleValue()
        {
            BackgroundWorker bw = new BackgroundWorker()
            {
                WorkerReportsProgress = false,
                WorkerSupportsCancellation = false
            };
            bw.DoWork += DoWork_RefreshScheduleValue;
            bw.RunWorkerCompleted += RunWorkerCompleted_RefreshScheduleValue;
            bw.RunWorkerAsync();
        }

        private void DoWork_RefreshScheduleValue(object sender, DoWorkEventArgs e)
        {
            if (isLuminanceSupport == Visibility.Visible)
            {
                //UpdateLuminance();
            }
            else
                UpdateScheduleInfo();
        }

        private void RunWorkerCompleted_RefreshScheduleValue(object sender, RunWorkerCompletedEventArgs e)
        {
            MyModule.GetRightView().Dispatcher.Invoke((Action)(() =>
            {
                if (isLuminanceSupport == Visibility.Visible)
                {
                    //NotifyPropertyChanged("LuminanceValue");
                    //NotifyPropertyChanged("LuminanceMaxValue");
                }
                else
                {
                    NotifyPropertyChanged("PR1Name");
                    NotifyPropertyChanged("PR2Name");
                    NotifyPropertyChanged("hOurs1");
                    NotifyPropertyChanged("mIns1");
                    NotifyPropertyChanged("dUration1");
                    NotifyPropertyChanged("hOurs2");
                    NotifyPropertyChanged("mIns2");
                    NotifyPropertyChanged("dUration2");
                    NotifyPropertyChanged("PR1BrightnessValue");
                    NotifyPropertyChanged("PR1ContrastValue");
                    NotifyPropertyChanged("PR2BrightnessValue");
                    NotifyPropertyChanged("PR2ContrastValue");
                }
            }));
        }

        private void GetALSContentAndSyncUI(MonitorInfo mo)
        {
            if (DdpmCommonHelper.DeviceManagerSA == null)
                return;

            if (Start_ALSConfig == null)
                isAlsSupported = Visibility.Collapsed;
            else
            {
                if (Start_ALSConfig.isSupportALS == 0)
                {
                    isAlsSupported = Visibility.Collapsed;
                }
                else
                {
                    isAlsSupported = Visibility.Visible;
                    Update_AutoBrightnessStatus(Start_ALSConfig.isAutoBrightness);
                    Update_AutoColorTempStatus(Start_ALSConfig.isAutoColorTemp);
                    Update_PrimaryMonitorSyncStatus(Start_ALSConfig.isPrimaryMonitorSync);
                    Update_AutoBrightnessRangeLevelStatus(Start_ALSConfig.AutoBrightnessRangeLevel);
                    Update_SupportedPrimaryMonitorSync(Start_ALSConfig.isAutoBrightness, Start_ALSConfig.isAutoColorTemp);
                }
            }
        }

        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~//

        public bool IsPR1Preview
        {
            get
            {
                return IsPR1Preview_;
            }
            set
            {
                IsPR1Preview_ = value;
                PR1ButtonContent = (value) ? "Stop Preview" : "Preview Changes";
                NotifyPropertyChanged("IsPR1Preview");
                NotifyPropertyChanged("IsPRPreview");
                NotifyPropertyChanged("PR1ButtonContent");
            }
        }

        public bool IsPR2Preview
        {
            get
            {
                return IsPR2Preview_;
            }
            set
            {
                IsPR2Preview_ = value;
                PR2ButtonContent = (value) ? "Stop Preview" : "Preview Changes";
                NotifyPropertyChanged("IsPR2Preview");
                NotifyPropertyChanged("IsPRPreview");
                NotifyPropertyChanged("PR2ButtonContent");
            }
        }

        public bool IsPRPreview
        {
            get
            {
                return (IsPR1Preview || IsPR2Preview);
            }
        }

        public string PR1Name
        {
            get
            {
                return PR1_Name;
            }
            set
            {
                PR1_Name = value;
                NotifyPropertyChanged("PR1Name");
            }
        }

        public string PR2Name
        {
            get
            {
                return PR2_Name;
            }
            set
            {
                PR2_Name = value;
                NotifyPropertyChanged("PR2Name");
            }
        }

        public int hOurs1
        {
            get
            {
                return hOurs_1;
            }
            set
            {
                hOurs_1 = value;
                NotifyPropertyChanged("hOurs1");
            }
        }

        public int mIns1
        {
            get
            {
                return mIns_1;
            }
            set
            {
                mIns_1 = value;
                NotifyPropertyChanged("mIns1");
            }
        }

        public int dUration1
        {
            get
            {
                return dUration_1;
            }
            set
            {
                dUration_1 = value;
                NotifyPropertyChanged("dUration1");
            }
        }

        public int hOurs2
        {
            get
            {
                return hOurs_2;
            }
            set
            {
                hOurs_2 = value;
                NotifyPropertyChanged("hOurs2");
            }
        }

        public int mIns2
        {
            get
            {
                return mIns_2;
            }
            set
            {
                mIns_2 = value;
                NotifyPropertyChanged("mIns2");
            }
        }

        public int dUration2
        {
            get
            {
                return dUration_2;
            }
            set
            {
                dUration_2 = value;
                NotifyPropertyChanged("dUration2");
            }
        }

        public double PR1BrightnessValue
        {
            get
            {
                if (PR1Brightness_Value < 0)
                {
                    if (isLuminanceSupport.Equals(Visibility.Visible))
                    {
                        PR1Brightness_Value = 75;
                        //NotifyPropertyChanged("BrightnessValue");

                        return PR1Brightness_Value;
                    }
                    else
                    {
                        //if (isNormalBrightness.Equals(Visibility.Visible))
                        //    return Get_Brightness_Value();
                        //else
                        return 0;
                    }
                }
                else
                    return PR1Brightness_Value;
            }

            set
            {
                PR1Brightness_Value = value;
                PR1Brightness_Debouncer.Debounce(value);
                NotifyPropertyChanged("PR1BrightnessValue");
            }
        }

        public double PR2BrightnessValue
        {
            get
            {
                if (PR2Brightness_Value < 0)
                {
                    if (isLuminanceSupport.Equals(Visibility.Visible))
                    {
                        PR2Brightness_Value = 75;
                        //NotifyPropertyChanged("BrightnessValue");

                        return PR2Brightness_Value;
                    }
                    else
                    {
                        //if (isNormalBrightness.Equals(Visibility.Visible))
                        //    return Get_Brightness_Value();
                        //else
                        return 0;
                    }
                }
                else
                    return PR2Brightness_Value;
            }

            set
            {
                PR2Brightness_Value = value;
                PR2Brightness_Debouncer.Debounce(value);
                NotifyPropertyChanged("PR2BrightnessValue");
            }
        }

        public double PR1ContrastValue
        {
            get
            {
                if (PR1Contrast_Value < 0)
                {
                    if (isLuminanceSupport.Equals(Visibility.Visible))
                    {
                        PR1Contrast_Value = 75;
                        //NotifyPropertyChanged("ContrastValue");

                        return PR1Contrast_Value;
                    }
                    else
                    {
                        //if (isNormalBrightness.Equals(Visibility.Visible))
                        //    return Get_Contrast_Value();
                        //else
                        return 0;
                    }
                }
                else
                    return PR1Contrast_Value;
            }

            set
            {
                PR1Contrast_Value = (value < 25) ? 25 : value;
                PR1Contrast_Debouncer.Debounce(PR1Contrast_Value);
                NotifyPropertyChanged("PR1ContrastValue");
            }
        }

        public double PR2ContrastValue
        {
            get
            {
                if (PR2Contrast_Value < 0)
                {
                    if (isLuminanceSupport.Equals(Visibility.Visible))
                    {
                        PR2Contrast_Value = 75;
                        //NotifyPropertyChanged("ContrastValue");

                        return PR2Contrast_Value;
                    }
                    else
                    {
                        //if (isNormalBrightness.Equals(Visibility.Visible))
                        //    return Get_Contrast_Value();
                        //else
                        return 0;
                    }
                }
                else
                    return PR2Contrast_Value;
            }

            set
            {
                PR2Contrast_Value = (value < 25) ? 25 : value;
                PR2Contrast_Debouncer.Debounce(PR2Contrast_Value);
                NotifyPropertyChanged("PR2ContrastValue");
            }
        }

        public void GetScheduleInfo()
        {
            var ScheduleMaps_string = string.Empty;

            if (SelectedHomeDevice == null)
                return;

            if (DdpmCommonHelper.Settings_Cache == null)
                DdpmCommonHelper.Settings_Cache = DdpmCommonHelper.ReadDDPMSettings();// DeviceManagerSA.ReloadAppConfigData().Result;

            if (ScheduleMaps == null)
                ScheduleMaps = new List<scheduleInfo>();

            if (ScheduleMaps.Count < 1)
            {
                ScheduleMaps_string = DdpmCommonHelper.Settings_Cache.UserSettings.Schedule;
                if (!string.IsNullOrWhiteSpace(ScheduleMaps_string))
                    ScheduleMaps.AddRange(JsonConvert.DeserializeObject<List<scheduleInfo>>(ScheduleMaps_string));
            }

            if (ScheduleMaps != null && ScheduleMaps.Count > 0)
            {
                bool find = false;

                foreach (scheduleInfo TMP in ScheduleMaps)
                {
                    if (TMP.Monitor.Equals(SelectedHomeDevice.MonitorInfo.edid))
                    {
                        find = true;

                        PR1Name = TMP.Pre1Name;
                        PR2Name = TMP.Pre2Name;
                        hOurs1 = TMP.Hours1;
                        mIns1 = TMP.Mins1;
                        dUration1 = TMP.Duration1;
                        hOurs2 = TMP.Hours2;
                        mIns2 = TMP.Mins2;
                        dUration2 = TMP.Duration2;
                        PR1Brightness_Value = TMP.Brightness1;
                        PR1Contrast_Value = TMP.Contrast1;
                        PR2Brightness_Value = TMP.Brightness2;
                        PR2Contrast_Value = TMP.Contrast2;

                        break;
                    }
                }

                if (!find)
                {
                    PR1Name = string.Empty;
                    PR2Name = string.Empty;
                    hOurs1 = 8;
                    mIns1 = 0;
                    dUration1 = 60;
                    hOurs2 = 5;
                    mIns2 = 0;
                    dUration2 = 60;
                    PR1Brightness_Value = 75;
                    PR1Contrast_Value = 75;
                    PR2Brightness_Value = 50;
                    PR2Contrast_Value = 50;
                }
            }
            else
            {
                PR1Name = string.Empty;
                PR2Name = string.Empty;
                hOurs1 = 8;
                mIns1 = 0;
                dUration1 = 60;
                hOurs2 = 5;
                mIns2 = 0;
                dUration2 = 60;
                PR1Brightness_Value = 75;
                PR1Contrast_Value = 75;
                PR2Brightness_Value = 50;
                PR2Contrast_Value = 50;
            }

            NotifyPropertyChanged("PR1Name");
            NotifyPropertyChanged("PR2Name");
            NotifyPropertyChanged("hOurs1");
            NotifyPropertyChanged("mIns1");
            NotifyPropertyChanged("dUration1");
            NotifyPropertyChanged("hOurs2");
            NotifyPropertyChanged("mIns2");
            NotifyPropertyChanged("dUration2");
            NotifyPropertyChanged("PR1BrightnessValue");
            NotifyPropertyChanged("PR1ContrastValue");
            NotifyPropertyChanged("PR2BrightnessValue");
            NotifyPropertyChanged("PR2ContrastValue");
        }

        public void CloseSchedule()
        {
            var ScheduleMaps_string = string.Empty;

            if (DdpmCommonHelper.Settings_Cache == null)
                DdpmCommonHelper.Settings_Cache = DdpmCommonHelper.ReadDDPMSettings();// DeviceManagerSA.ReloadAppConfigData().Result;

            if (ScheduleMaps == null)
                ScheduleMaps = new List<scheduleInfo>();

            if (ScheduleMaps.Count < 1)
            {
                ScheduleMaps_string = DdpmCommonHelper.Settings_Cache.UserSettings.Schedule;
                if (!string.IsNullOrWhiteSpace(ScheduleMaps_string))
                    ScheduleMaps.AddRange(JsonConvert.DeserializeObject<List<scheduleInfo>>(ScheduleMaps_string));
            }

            if (ScheduleMaps != null && ScheduleMaps.Count > 0)
            {
                foreach (scheduleInfo TMP in ScheduleMaps)
                {
                    if (TMP.Monitor.Equals(SelectedHomeDevice.MonitorInfo.edid))
                    {
                        TMP.IsEnable = false;
                        break;
                    }
                }

                ScheduleMaps_string = JsonConvert.SerializeObject(ScheduleMaps, Formatting.Indented);
            }

            DdpmCommonHelper.Settings_Cache.UserSettings.Schedule = ScheduleMaps_string;
            DdpmCommonHelper.WriteDDPMSettings(DdpmCommonHelper.Settings_Cache);//DeviceManagerSA.SetAppConfigData(DdpmCommonHelper.Settings_Cache);
        }

        public bool CheckIsTimeOverlap()
        {
            bool rc = false;
            if (hOurs1 > -1 && hOurs2 > -1 && mIns1 > -1 && mIns2 > -1 && dUration1 > -1 && dUration2 > -1)
            {
                var CurDateTime = DateTime.Now;
                var Hour_PR1 = (hOurs1 < 12) ? hOurs1 : (hOurs1 - 12);
                var Hour_PR2 = (hOurs2 < 12) ? (hOurs2 + 12) : hOurs2;
                var Min_PR1 = mIns1;
                var Min_PR2 = mIns2;
                var Duration_PR1 = dUration1;
                var Duration_PR2 = dUration2;
                var PR1Time = new DateTime(CurDateTime.Year, CurDateTime.Month, CurDateTime.Day, Hour_PR1, Min_PR1, 0);
                var Pre_PR1Time = new DateTime(CurDateTime.Year, CurDateTime.Month, CurDateTime.Day, Hour_PR1, Min_PR1, 0).AddMinutes(Duration_PR1 * -1);
                var PR1Time_ADD1D = new DateTime(CurDateTime.Year, CurDateTime.Month, CurDateTime.Day, Hour_PR1, Min_PR1, 0).AddDays(1);
                var Pre_PR1Timee_ADD1D = (new DateTime(CurDateTime.Year, CurDateTime.Month, CurDateTime.Day, Hour_PR1, Min_PR1, 0).AddMinutes(Duration_PR1 * -1)).AddDays(1);
                var PR2Time = new DateTime(CurDateTime.Year, CurDateTime.Month, CurDateTime.Day, Hour_PR2, Min_PR2, 0);
                var Pre_PR2Time = new DateTime(CurDateTime.Year, CurDateTime.Month, CurDateTime.Day, Hour_PR2, Min_PR2, 0).AddMinutes(Duration_PR2 * -1);

                bool IsPR1NotCorrectSet = (DateTime.Compare(Pre_PR2Time, PR1Time) < 0);
                bool IsPR2NotCorrectSet = (DateTime.Compare(Pre_PR1Timee_ADD1D, PR2Time) < 0);

                if (IsPR1NotCorrectSet || IsPR2NotCorrectSet)
                    rc = true;
                else rc = false;
            }
            return rc;
        }

        public void CalculateNowValue()
        {
            if (hOurs1 > -1 && hOurs2 > -1 && mIns1 > -1 && mIns2 > -1 && dUration1 > -1 && dUration2 > -1)
            {
                var CurDateTime = DateTime.Now;
                var Brightness_PR1 = PR1BrightnessValue;
                var Brightness_PR2 = PR2BrightnessValue;
                var Brightness_difference = Brightness_PR1 - Brightness_PR2;
                var Contrast_PR1 = PR1ContrastValue;
                var Contrast_PR2 = PR2ContrastValue;
                var Contrast_difference = Contrast_PR1 - Contrast_PR2;
                bool IsBrightnessPR1Plus = Brightness_difference > 0 ? true : false;
                bool IsBrightnessPR2Plus = Brightness_difference > 0 ? false : true;
                bool IsContrastPR1Plus = Contrast_difference > 0 ? true : false;
                bool IsContrastPR2Plus = Contrast_difference > 0 ? false : true;
                var Hour_PR1 = (hOurs1 < 12) ? hOurs1 : (hOurs1 - 12);
                var Hour_PR2 = (hOurs2 < 12) ? (hOurs2 + 12) : hOurs2;
                var Min_PR1 = mIns1;
                var Min_PR2 = mIns2;
                var Duration_PR1 = dUration1;
                var Duration_PR2 = dUration2;
                var PR1Time = new DateTime(CurDateTime.Year, CurDateTime.Month, CurDateTime.Day, Hour_PR1, Min_PR1, 0);
                var Pre_PR1Time = new DateTime(CurDateTime.Year, CurDateTime.Month, CurDateTime.Day, Hour_PR1, Min_PR1, 0).AddMinutes(Duration_PR1 * -1);
                var PR1Time_ADD1D = new DateTime(CurDateTime.Year, CurDateTime.Month, CurDateTime.Day, Hour_PR1, Min_PR1, 0).AddDays(1);
                var Pre_PR1Timee_ADD1D = (new DateTime(CurDateTime.Year, CurDateTime.Month, CurDateTime.Day, Hour_PR1, Min_PR1, 0).AddMinutes(Duration_PR1 * -1)).AddDays(1);
                var PR2Time = new DateTime(CurDateTime.Year, CurDateTime.Month, CurDateTime.Day, Hour_PR2, Min_PR2, 0);
                var Pre_PR2Time = new DateTime(CurDateTime.Year, CurDateTime.Month, CurDateTime.Day, Hour_PR2, Min_PR2, 0).AddMinutes(Duration_PR2 * -1);
                if (Brightness_difference < 0) Brightness_difference = Brightness_difference * -1;
                if (Contrast_difference < 0) Contrast_difference = Contrast_difference * -1;
                var PerStepValue = 5;
                if (CurDateTime.Hour < 12)  // AM
                {
                    if (DateTime.Compare(CurDateTime, PR1Time) > 0) //CurDateTime is later than PR1Time.
                    {
                        if (DateTime.Compare(CurDateTime, Pre_PR2Time) > 0)  //CurDateTime is later than Pre_PR2Time.
                        {
                            var BrightnessSteps = Brightness_difference / PerStepValue;
                            var BrightnessSteps_min = (PR2Time - Pre_PR2Time).TotalMinutes / BrightnessSteps;
                            var Brightness_steps = Convert.ToInt32((CurDateTime - Pre_PR2Time).TotalMinutes / BrightnessSteps_min);

                            var ContrastSteps = Contrast_difference / PerStepValue;
                            var ContrastSteps_min = (PR2Time - Pre_PR2Time).TotalMinutes / ContrastSteps;
                            var Contrast_steps = Convert.ToInt32((CurDateTime - Pre_PR2Time).TotalMinutes / ContrastSteps_min);

                            if (IsBrightnessPR2Plus)
                            {
                                if ((!IsMouseEnterSchedule_1 && !IsMouseEnterSchedule_2) && (!IsPR1Preview && !IsPR2Preview))
                                    BrightnessValue = Brightness_PR1 + (PerStepValue * Brightness_steps);
                            }
                            else
                            {
                                if ((!IsMouseEnterSchedule_1 && !IsMouseEnterSchedule_2) && (!IsPR1Preview && !IsPR2Preview))
                                    BrightnessValue = Brightness_PR1 - (PerStepValue * Brightness_steps);
                            }

                            if (IsContrastPR2Plus)
                            {
                                if ((!IsMouseEnterSchedule_1 && !IsMouseEnterSchedule_2) && (!IsPR1Preview && !IsPR2Preview))
                                    ContrastValue = Contrast_PR1 + (PerStepValue * Contrast_steps);
                            }
                            else
                            {
                                if ((!IsMouseEnterSchedule_1 && !IsMouseEnterSchedule_2) && (!IsPR1Preview && !IsPR2Preview))
                                    ContrastValue = Contrast_PR1 - (PerStepValue * Contrast_steps);
                            }
                        }
                        else  //CurDateTime is same as or earlier than Pre_PR2Time
                        {
                            if ((!IsMouseEnterSchedule_1 && !IsMouseEnterSchedule_2) && (!IsPR1Preview && !IsPR2Preview))
                            {
                                BrightnessValue = Brightness_PR1;
                                ContrastValue = Contrast_PR1;
                            }
                        }
                    }
                    else if (DateTime.Compare(CurDateTime, PR1Time) == 0) //The same as PR1Time
                    {
                        if ((!IsMouseEnterSchedule_1 && !IsMouseEnterSchedule_2) && (!IsPR1Preview && !IsPR2Preview))
                        {
                            BrightnessValue = Brightness_PR1;
                            ContrastValue = Contrast_PR1;
                        }
                    }
                    else //CurDateTime is earlier than PR1Time
                    {
                        if (DateTime.Compare(CurDateTime, Pre_PR1Time) > 0)  //CurDateTime is later than Pre_PR1Time.
                        {
                            var BrightnessSteps = Brightness_difference / PerStepValue;
                            var BrightnessSteps_min = (PR1Time - Pre_PR1Time).TotalMinutes / BrightnessSteps;
                            var Brightness_steps = Convert.ToInt32((CurDateTime - Pre_PR1Time).TotalMinutes / BrightnessSteps_min);

                            var ContrastSteps = Contrast_difference / PerStepValue;
                            var ContrastSteps_min = (PR1Time - Pre_PR1Time).TotalMinutes / ContrastSteps;
                            var Contrast_steps = Convert.ToInt32((CurDateTime - Pre_PR1Time).TotalMinutes / ContrastSteps_min);

                            if (IsBrightnessPR1Plus)
                            {
                                if ((!IsMouseEnterSchedule_1 && !IsMouseEnterSchedule_2) && (!IsPR1Preview && !IsPR2Preview))
                                    BrightnessValue = Brightness_PR2 + (PerStepValue * Brightness_steps);
                            }
                            else
                            {
                                if ((!IsMouseEnterSchedule_1 && !IsMouseEnterSchedule_2) && (!IsPR1Preview && !IsPR2Preview))
                                    BrightnessValue = Brightness_PR2 - (PerStepValue * Brightness_steps);
                            }

                            if (IsContrastPR1Plus)
                            {
                                if ((!IsMouseEnterSchedule_1 && !IsMouseEnterSchedule_2) && (!IsPR1Preview && !IsPR2Preview))
                                    ContrastValue = Contrast_PR2 + (PerStepValue * Contrast_steps);
                            }
                            else
                            {
                                if ((!IsMouseEnterSchedule_1 && !IsMouseEnterSchedule_2) && (!IsPR1Preview && !IsPR2Preview))
                                    ContrastValue = Contrast_PR2 - (PerStepValue * Contrast_steps);
                            }
                        }
                        else  //CurDateTime is same as  or  earlier than Pre_PR1Time
                        {
                            if ((!IsMouseEnterSchedule_1 && !IsMouseEnterSchedule_2) && (!IsPR1Preview && !IsPR2Preview))
                            {
                                BrightnessValue = Brightness_PR2;
                                ContrastValue = Contrast_PR2;
                            }
                        }
                    }
                }
                else //PM
                {
                    if (DateTime.Compare(CurDateTime, PR2Time) > 0) //CurDateTime is later than PR2Time.
                    {
                        if (DateTime.Compare(CurDateTime, Pre_PR1Timee_ADD1D) > 0)  //CurDateTime is later than Pre_PR1Timee_ADD1D.
                        {
                            var BrightnessSteps = Brightness_difference / PerStepValue;
                            var BrightnessSteps_min = (PR1Time - Pre_PR1Time).TotalMinutes / BrightnessSteps;
                            var Brightness_steps = Convert.ToInt32((CurDateTime - Pre_PR1Time).TotalMinutes / BrightnessSteps_min);

                            var ContrastSteps = Contrast_difference / PerStepValue;
                            var ContrastSteps_min = (PR1Time - Pre_PR1Time).TotalMinutes / ContrastSteps;
                            var Contrast_steps = Convert.ToInt32((CurDateTime - Pre_PR1Time).TotalMinutes / ContrastSteps_min);

                            if (IsBrightnessPR1Plus)
                            {
                                if ((!IsMouseEnterSchedule_1 && !IsMouseEnterSchedule_2) && (!IsPR1Preview && !IsPR2Preview))
                                    BrightnessValue = Brightness_PR2 + (PerStepValue * Brightness_steps);
                            }
                            else
                            {
                                if ((!IsMouseEnterSchedule_1 && !IsMouseEnterSchedule_2) && (!IsPR1Preview && !IsPR2Preview))
                                    BrightnessValue = Brightness_PR2 - (PerStepValue * Brightness_steps);
                            }

                            if (IsContrastPR1Plus)
                            {
                                if ((!IsMouseEnterSchedule_1 && !IsMouseEnterSchedule_2) && (!IsPR1Preview && !IsPR2Preview))
                                    ContrastValue = Contrast_PR2 + (PerStepValue * Contrast_steps);
                            }
                            else
                            {
                                if ((!IsMouseEnterSchedule_1 && !IsMouseEnterSchedule_2) && (!IsPR1Preview && !IsPR2Preview))
                                    ContrastValue = Contrast_PR2 - (PerStepValue * Contrast_steps);
                            }
                        }
                        else  //CurDateTime is same as or earlier than Pre_PR1Timee_ADD1D
                        {
                            if ((!IsMouseEnterSchedule_1 && !IsMouseEnterSchedule_2) && (!IsPR1Preview && !IsPR2Preview))
                            {
                                BrightnessValue = Brightness_PR2;
                                ContrastValue = Contrast_PR2;
                            }
                        }
                    }
                    else if (DateTime.Compare(CurDateTime, PR2Time) == 0) //The same as PR2Time.
                    {
                        if ((!IsMouseEnterSchedule_1 && !IsMouseEnterSchedule_2) && (!IsPR1Preview && !IsPR2Preview))
                        {
                            BrightnessValue = Brightness_PR2;
                            ContrastValue = Contrast_PR2;
                        }
                    }
                    else //CurDateTime is earlier than PR2Time.
                    {
                        if (DateTime.Compare(CurDateTime, Pre_PR2Time) > 0)  //CurDateTime is later than Pre_PR2Time.
                        {
                            var BrightnessSteps = Brightness_difference / PerStepValue;
                            var BrightnessSteps_min = (PR2Time - Pre_PR2Time).TotalMinutes / BrightnessSteps;
                            var Brightness_steps = Convert.ToInt32((CurDateTime - Pre_PR2Time).TotalMinutes / BrightnessSteps_min);

                            var ContrastSteps = Contrast_difference / PerStepValue;
                            var ContrastSteps_min = (PR2Time - Pre_PR2Time).TotalMinutes / ContrastSteps;
                            var Contrast_steps = Convert.ToInt32((CurDateTime - Pre_PR2Time).TotalMinutes / ContrastSteps_min);

                            if (IsBrightnessPR2Plus)
                            {
                                if ((!IsMouseEnterSchedule_1 && !IsMouseEnterSchedule_2) && (!IsPR1Preview && !IsPR2Preview))
                                    BrightnessValue = Brightness_PR1 + (PerStepValue * Brightness_steps);
                            }
                            else
                            {
                                if ((!IsMouseEnterSchedule_1 && !IsMouseEnterSchedule_2) && (!IsPR1Preview && !IsPR2Preview))
                                    BrightnessValue = Brightness_PR1 - (PerStepValue * Brightness_steps);
                            }

                            if (IsContrastPR2Plus)
                            {
                                if ((!IsMouseEnterSchedule_1 && !IsMouseEnterSchedule_2) && (!IsPR1Preview && !IsPR2Preview))
                                    ContrastValue = Contrast_PR1 + (PerStepValue * Contrast_steps);
                            }
                            else
                            {
                                if ((!IsMouseEnterSchedule_1 && !IsMouseEnterSchedule_2) && (!IsPR1Preview && !IsPR2Preview))
                                    ContrastValue = Contrast_PR1 - (PerStepValue * Contrast_steps);
                            }
                        }
                        else  //CurDateTime is same as  or  earlier than Pre_PR2Time
                        {
                            if ((!IsMouseEnterSchedule_1 && !IsMouseEnterSchedule_2) && (!IsPR1Preview && !IsPR2Preview))
                            {
                                BrightnessValue = Brightness_PR1;
                                ContrastValue = Contrast_PR1;
                            }
                        }
                    }
                }
            }
        }

        public void StopScheduleManger()
        {
            if (DdpmCommonHelper.DeviceManagerSA != null)
                DdpmCommonHelper.DeviceManagerSA.StopSchedulerManger();
        }

        public void StartScheduleManger(int millisecond)
        {
            if (DdpmCommonHelper.DeviceManagerSA != null)
                DdpmCommonHelper.DeviceManagerSA.StartSchedulerManger(millisecond);
        }

        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~//

        public bool IsSynchronize
        {
            get
            {
                //if (!IsGetSynchronizeMonitor)
                //{
                //    return Get_Synchronize();
                //}
                //else
                return IsSynchronizeMonitor;
            }
            set
            {
                IsSynchronizeMonitor = value;
                NotifyPropertyChanged("IsSynchronize");
            }
        }

        public double BrightnessValue
        {
            get
            {
                if (Brightness_Value < 0)
                {
                    if (isLuminanceSupport.Equals(Visibility.Visible))
                    {
                        Brightness_Value = 75;
                        //NotifyPropertyChanged("BrightnessValue");

                        return Brightness_Value;
                    }
                    else
                    {
                        //if (isNormalBrightness.Equals(Visibility.Visible))
                        //    return Get_Brightness_Value();
                        //else
                        return 0;
                    }
                }
                else
                    return Brightness_Value;
            }

            set
            {
                if (Start_ALSConfig != null && Start_ALSConfig.isSupportALS > 0)
                {
                    if (_autoBrightnessStatus)
                    {
                        string pop_string = "Auto Brightness is currently enabled. Do you wish to disable it to continue?";
                        if (DdpmCommonHelper.DDPMMesssageBox("Warning", pop_string))
                        {
                            AutoBrightnessStatus = _autoBrightnessStatus = false;
                        }
                        else
                            return;
                    }
                }

                Brightness_Value = value;
                Brightness_Debouncer.Debounce(value);
                NotifyPropertyChanged("BrightnessValue");
            }
        }

        /// <summary>
        /// Detect the status of the PrimaryMonitorSync and obtain the current number of monitors that support the ALS function.
        /// </summary>
        /// <param name="onoff">UI PrimaryMonitorSync status</param>
        private void ALSSettingsChangesOnNonPrimary(bool onoff)
        {
            if (!Start_ALSConfig.isPrimaryMonitorSync && CheckMonitorALSStatus())// user change non-Primary
            {
                string pop_string = "This is not your primary monitor. Do you want to proceed with the change and set this as primary Monitor for Sync??";
                if (DdpmCommonHelper.DDPMMesssageBox("Warning", pop_string))
                {
                    Start_ALSConfig.isPrimaryMonitorSync = onoff;
                    SetALSAll(Start_ALSConfig, ALSFeatureQueryType.All, 0);
                    NotifyPropertyChanged("PrimaryMonitorSyncStatus");
                    NotifyPropertyChanged("PrimaryMonitorSync_String");
                }
                else
                    SetALSAll(Start_ALSConfig, ALSFeatureQueryType.All, 0);
            }
            else
                SetALSAll(Start_ALSConfig, ALSFeatureQueryType.All, 0);
        }

        public double ContrastValue
        {
            get
            {
                if (Contrast_Value < 0)
                {
                    if (isLuminanceSupport.Equals(Visibility.Visible))
                    {
                        Contrast_Value = 75;
                        //NotifyPropertyChanged("ContrastValue");

                        return Contrast_Value;
                    }
                    else
                    {
                        //if (isNormalBrightness.Equals(Visibility.Visible))
                        //    return Get_Contrast_Value();
                        //else
                        return 0;
                    }
                }
                else
                    return Contrast_Value;
            }

            set
            {
                Contrast_Value = (value < 25) ? 25 : value;
                Contrast_Debouncer.Debounce(Contrast_Value);
                NotifyPropertyChanged("ContrastValue");
            }
        }

        public double LuminanceValue
        {
            get
            {
                if (Luminance_Value < 0)
                {
                    //if (isLuminanceSupport.Equals(Visibility.Visible))
                    //    return Get_Luminance_Value();
                    //else
                    //{
                    Luminance_Value = 100;
                    //NotifyPropertyChanged("LuminanceValue");

                    return Luminance_Value;
                    //}
                }
                else
                    return Luminance_Value;
            }

            set
            {
                Luminance_Value = value;
                Luminance_Debouncer.Debounce(value);
                NotifyPropertyChanged("LuminanceValue");
            }
        }

        public double LuminanceMaxValue
        {
            get
            {
                /*if (LuminanceMax_Value < 0)
                {
                    //if (isLuminanceSupport.Equals(Visibility.Visible))
                    //    return Get_LuminanceMax_Value();
                    //else
                    //{
                        LuminanceMax_Value = 100;
                        //NotifyPropertyChanged("LuminanceMaxValue");

                        return LuminanceMax_Value;
                    //}
                }
                else*/ //0703 update to fix luminance issue since get property is earlier than here
                return LuminanceMax_Value;
            }
        }

        private bool Get_Synchronize()
        {
            if (DdpmCommonHelper.Settings_Cache == null)
                DdpmCommonHelper.Settings_Cache = DdpmCommonHelper.ReadDDPMSettings();//DeviceManagerSA.ReloadAppConfigData().Result;
            if (DdpmCommonHelper.Settings_Cache != null)
            {
                IsSynchronizeMonitor = DdpmCommonHelper.Settings_Cache.UserSettings.IsSynchronizemonitor;
                IsGetSynchronizeMonitor = true;
            }
            return IsSynchronizeMonitor;
        }

        private double Get_Brightness_Value()
        {
            if (SelectedHomeDevice == null)
                return 0;

            ObjGetVCP obj = DdpmCommonHelper.DeviceManagerSA.GetVCPCapability(SelectedHomeDevice.MonitorInfo, 0x10, 0).Result;
            if (obj.result)
            {
                //if (Brightness_Value < 0)
                //{
                //    Brightness_Value = Convert.ToDouble((uint)(long)obj.value);
                //}
                //else
                Brightness_Value = Convert.ToDouble((uint)(long)obj.value);
            }
            NotifyPropertyChanged("BrightnessValue");
            return Brightness_Value;
        }

        public void UpdateScheduleInfo()
        {
            if (PR1Brightness_Value < 0 || PR2Brightness_Value < 0 || PR1Contrast_Value < 0 || PR2Contrast_Value < 0 || hOurs_1 < 0 || hOurs_2 < 0 || mIns_1 < 0 || mIns_2 < 0 || dUration_1 < 0 || dUration_2 < 0)
                GetScheduleInfo();
        }

        public void UpdateBrightnessContrast()
        {
            Trace.WriteLine($"2.1. {DateTime.Now.ToString("MM/dd/yyyy hh:mm ss fff")}");
            if (Brightness_Value < 0)
                Get_Brightness_Value();
            Trace.WriteLine($"2.2. {DateTime.Now.ToString("MM/dd/yyyy hh:mm ss fff")}");
            //NotifyPropertyChanged("BrightnessValue");
            if (Contrast_Value < 0)
                Get_Contrast_Value();
            Trace.WriteLine($"2.3. {DateTime.Now.ToString("MM/dd/yyyy hh:mm ss fff")}");
            //NotifyPropertyChanged("ContrastValue");
            //Trace.WriteLine($"2.4. {DateTime.Now.ToString("MM/dd/yyyy hh:mm ss fff")}");
        }

        public void UpdateLuminance()
        {
            if (Luminance_Value < 0)
                Get_Luminance_Value(); //0703 fix update luminance issue
            //NotifyPropertyChanged("LuminanceValue");
            if (LuminanceMax_Value < 0)
                Get_LuminanceMax_Value();
            //NotifyPropertyChanged("LuminanceMaxValue");
        }

        private double Get_Luminance_Value()
        {
            if (SelectedHomeDevice == null)
                return 0;

            ObjGetVCP obj = DdpmCommonHelper.DeviceManagerSA.GetVCPCapability(SelectedHomeDevice.MonitorInfo, 0x10, 0).Result;
            if (obj.result)
            {
                //if (Luminance_Value < 0)
                //{
                //    Luminance_Value = Convert.ToDouble((uint)(long)obj.value);
                //}
                //else
                Luminance_Value = Convert.ToDouble((uint)(long)obj.value);
            }
            NotifyPropertyChanged("LuminanceValue");
            return Luminance_Value;
        }

        private double Get_LuminanceMax_Value()
        {
            if (SelectedHomeDevice == null)
                return 0;

            ObjGetVCP obj = DdpmCommonHelper.DeviceManagerSA.GetVCPCapability(SelectedHomeDevice.MonitorInfo, 0x10, 1).Result;
            if (obj.result)
                LuminanceMax_Value = Convert.ToDouble((uint)(long)obj.value);

            NotifyPropertyChanged("LuminanceMaxValue");
            return LuminanceMax_Value;
        }

        private void Set_Luminance_Value(object value_)
        {
            var value = Convert.ToDouble(value_);
            uint nNewValue = Convert.ToUInt32(value);

            DdpmCommonHelper.DeviceManagerSA.SetVCPCapability(ModuleOwner.SelectedHomeDevice.MonitorInfo, 0x10, nNewValue);

            //NotifyPropertyChanged("LuminanceValue");
        }

        private void Set_Brightness_Value(object value_)
        {
            var value = Convert.ToDouble(value_);
            uint nNewValue = Convert.ToUInt32(value);

            if (IsSynchronize)
            {
                foreach (HomeDevice hd in ModuleOwner.HomeDevices)
                {
                    if (hd.MonitorInfo.IsDellMonitor)
                        DdpmCommonHelper.DeviceManagerSA.SetVCPCapability(hd.MonitorInfo, 0x10, nNewValue);
                }
            }
            else
                DdpmCommonHelper.DeviceManagerSA.SetVCPCapability(ModuleOwner.SelectedHomeDevice.MonitorInfo, 0x10, nNewValue);

            //NotifyPropertyChanged("BrightnessValue");
        }

        private double Get_Contrast_Value()
        {
            if (SelectedHomeDevice == null)
                return 0;

            ObjGetVCP obj = DdpmCommonHelper.DeviceManagerSA.GetVCPCapability(SelectedHomeDevice.MonitorInfo, 0x12, 0).Result;
            if (obj.result)
            {
                //if (Contrast_Value < 0)
                //{
                //    Contrast_Value = Convert.ToDouble((uint)(long)obj.value);
                //}
                //else
                Contrast_Value = Convert.ToDouble((uint)(long)obj.value);
            }

            NotifyPropertyChanged("ContrastValue");
            return Contrast_Value;
        }

        private void Set_Contrast_Value(object value_)
        {
            var value = Convert.ToDouble(value_);

            uint nNewValue = Convert.ToUInt32(value);

            if (IsSynchronize)
            {
                foreach (HomeDevice hd in ModuleOwner.HomeDevices)
                {
                    if (hd.MonitorInfo.IsDellMonitor)
                        DdpmCommonHelper.DeviceManagerSA.SetVCPCapability(hd.MonitorInfo, 0x12, nNewValue);
                }
            }
            else
                DdpmCommonHelper.DeviceManagerSA.SetVCPCapability(ModuleOwner.SelectedHomeDevice.MonitorInfo, 0x12, nNewValue);

            //NotifyPropertyChanged("ContrastValue");
        }

        private void NotifyPropertyChanged(string info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        public IModuleOwner? ModuleOwner { get; set; }

        //Robert_Lin, 2024-6-3 added
        //

        #region Expander Groups - toggle Expander (Group)

        private int _expanderGroup = 0;

        public int ExpanderGroup
        {
            get => _expanderGroup;
            set => SetProperty(ref _expanderGroup, value);
        }

        #endregion Expander Groups - toggle Expander (Group)

        public Visibility isScheduleSupport { get; set; } = Visibility.Collapsed;
        public Visibility isNormalBrightness { get; set; } = Visibility.Visible;

        private bool _isLuminanceSupport = false;//Brightness's value inverse the Luminance's value

        public Visibility isLuminanceSupport
        {
            get
            {
                if (_isLuminanceSupport)
                {
                    isNormalBrightness = Visibility.Collapsed;
                    NotifyPropertyChanged("isNormalBrightness");
                    return Visibility.Visible;
                }
                isNormalBrightness = Visibility.Visible;
                NotifyPropertyChanged("isNormalBrightness");
                return Visibility.Collapsed;
            }
            set
            {
                if (value == Visibility.Visible)
                {
                    _isLuminanceSupport = true;
                    isNormalBrightness = Visibility.Collapsed;
                }
                else
                {
                    _isLuminanceSupport = false;
                    isNormalBrightness = Visibility.Visible;
                }
                NotifyPropertyChanged("isNormalBrightness");
                NotifyPropertyChanged("isLuminanceSupport");
            }
        }

        #region ALS functions

        private Visibility _AutoALSLock = Visibility.Collapsed;
        private Visibility _AutoBrightnessLock = Visibility.Collapsed;
        private Visibility _AutoBrigRangeLevelLock = Visibility.Collapsed;
        private Visibility _AutoTemperatureLock = Visibility.Collapsed;
        private Visibility _PrimaryMonitorSyncLock = Visibility.Collapsed;

        public Visibility AutoALSLock
        {
            get => _AutoALSLock;
            set
            {
                _AutoALSLock = value;
                NotifyPropertyChanged(nameof(AutoALSLock));
            }
        }

        public Visibility AutoBrightnessLock
        {
            get => _AutoBrightnessLock;
            set
            {
                _AutoBrightnessLock = value;
                NotifyPropertyChanged(nameof(AutoBrightnessLock));
            }
        }

        public Visibility AutoBrigRangeLevelLock
        {
            get => _AutoBrigRangeLevelLock;
            set
            {
                _AutoBrigRangeLevelLock = value;
                NotifyPropertyChanged(nameof(AutoBrigRangeLevelLock));
            }
        }

        public Visibility AutoTemperatureLock
        {
            get => _AutoTemperatureLock;
            set
            {
                _AutoTemperatureLock = value;
                NotifyPropertyChanged(nameof(AutoTemperatureLock));
            }
        }

        public Visibility PrimaryMonitorSyncLock
        {
            get => _PrimaryMonitorSyncLock;
            set
            {
                _PrimaryMonitorSyncLock = value;
                NotifyPropertyChanged(nameof(PrimaryMonitorSyncLock));
            }
        }

        /// <summary>
        /// Auto Brightness Binding data
        /// </summary>
        public void Update_ALSLockStatus(bool value)
        {
            if (value && _autoBrightnessStatus && !_autoColorTempStatus)
            {
                AutoALSLock = value ? Visibility.Visible : Visibility.Collapsed;
                IsAutoBrightnessLockMask = Visibility.Visible;
                IsAutoBrigRangeLevelLockMask = Visibility.Visible;
                IsAutoTemperatureLockMask = Visibility.Visible;
                IsPrimaryMonitorSyncLockMask = Visibility.Visible;
                IsAutoBrightnessRangeLevelStringLockMask = Visibility.Visible;
            }
            else if (value && _autoBrightnessStatus && _autoColorTempStatus)
            {
                AutoALSLock = value ? Visibility.Visible : Visibility.Collapsed;
                AutoTemperatureLock = value ? Visibility.Visible : Visibility.Collapsed;
                IsAutoTemperatureSwitchLockMask = Visibility.Visible;
            }
            else
            {
                AutoALSLock = value ? Visibility.Visible : Visibility.Collapsed;
                AutoBrightnessLock = value ? Visibility.Visible : Visibility.Collapsed;
                AutoBrigRangeLevelLock = value ? Visibility.Visible : Visibility.Collapsed;
                AutoTemperatureLock = value ? Visibility.Visible : Visibility.Collapsed;
                PrimaryMonitorSyncLock = value ? Visibility.Visible : Visibility.Collapsed;

                IsAutoBrightnessLockMask = Visibility.Collapsed;
                IsAutoBrigRangeLevelLockMask = Visibility.Collapsed;
                IsAutoTemperatureLockMask = Visibility.Collapsed;
                IsAutoTemperatureSwitchLockMask = Visibility.Collapsed;
                IsPrimaryMonitorSyncLockMask = Visibility.Collapsed;
                IsAutoBrightnessRangeLevelStringLockMask = Visibility.Collapsed;
            }
        }

        public void Update_BriContLockStatus(bool value)
        {
            LockMaskVisible = value ? Visibility.Visible : Visibility.Collapsed;
            TabSTOP = value ? "None" : "Cycle";
        }
        public void Update_SyncLockStatus(bool value)
        {
            synchronizeLock = value ? Visibility.Visible : Visibility.Collapsed;
        }

        private Visibility _isAutoBrightnessLockMask = Visibility.Collapsed;

        public Visibility IsAutoBrightnessLockMask
        {
            get { return _isAutoBrightnessLockMask; }
            set
            {
                if (_isAutoBrightnessLockMask != value)
                {
                    _isAutoBrightnessLockMask = value;
                    NotifyPropertyChanged(nameof(IsAutoBrightnessLockMask));
                }
            }
        }

        private Visibility _isAutoBrigRangeLevelLockMask = Visibility.Collapsed;

        public Visibility IsAutoBrigRangeLevelLockMask
        {
            get { return _isAutoBrigRangeLevelLockMask; }
            set
            {
                if (_isAutoBrigRangeLevelLockMask != value)
                {
                    _isAutoBrigRangeLevelLockMask = value;
                    NotifyPropertyChanged(nameof(IsAutoBrigRangeLevelLockMask));
                }
            }
        }

        private Visibility _isAutoTemperatureLockMask = Visibility.Collapsed;

        public Visibility IsAutoTemperatureLockMask
        {
            get { return _isAutoTemperatureLockMask; }
            set
            {
                if (_isAutoTemperatureLockMask != value)
                {
                    _isAutoTemperatureLockMask = value;
                    NotifyPropertyChanged(nameof(IsAutoTemperatureLockMask));
                }
            }
        }

        private Visibility _isAutoTemperatureSwitchLockMask = Visibility.Collapsed;

        public Visibility IsAutoTemperatureSwitchLockMask
        {
            get { return _isAutoTemperatureSwitchLockMask; }
            set
            {
                if (_isAutoTemperatureSwitchLockMask != value)
                {
                    _isAutoTemperatureSwitchLockMask = value;
                    NotifyPropertyChanged(nameof(IsAutoTemperatureSwitchLockMask));
                }
            }
        }

        private Visibility _isPrimaryMonitorSyncLockMask = Visibility.Collapsed;

        public Visibility IsPrimaryMonitorSyncLockMask
        {
            get { return _isPrimaryMonitorSyncLockMask; }
            set
            {
                if (_isPrimaryMonitorSyncLockMask != value)
                {
                    _isPrimaryMonitorSyncLockMask = value;
                    NotifyPropertyChanged(nameof(IsPrimaryMonitorSyncLockMask));
                }
            }
        }

        private Visibility _isAutoBrightnessRangeLevelStringLockMask = Visibility.Collapsed;

        public Visibility IsAutoBrightnessRangeLevelStringLockMask
        {
            get { return _isAutoBrightnessRangeLevelStringLockMask; }
            set
            {
                if (_isAutoBrightnessRangeLevelStringLockMask != value)
                {
                    _isAutoBrightnessRangeLevelStringLockMask = value;
                    NotifyPropertyChanged(nameof(IsAutoBrightnessRangeLevelStringLockMask));
                }
            }
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////////

        private bool _isAutoBrightnessTextGrayedOut;

        public bool IsAutoBrightnessTextGrayedOut
        {
            get => _isAutoBrightnessTextGrayedOut;
            set
            {
                _isAutoBrightnessTextGrayedOut = value;
                NotifyPropertyChanged(nameof(IsAutoBrightnessTextGrayedOut));
            }
        }

        private bool _isAutoColorTempTextGrayedOut;

        public bool IsAutoColorTempTextGrayedOut
        {
            get => _isAutoColorTempTextGrayedOut;
            set
            {
                _isAutoColorTempTextGrayedOut = value;
                NotifyPropertyChanged(nameof(IsAutoColorTempTextGrayedOut));
            }
        }

        private bool _isAutoBrightnessRangeLevelTextGrayedOut;

        public bool IsAutoBrightnessRangeLevelTextGrayedOut
        {
            get => _isAutoBrightnessRangeLevelTextGrayedOut;
            set
            {
                _isAutoBrightnessRangeLevelTextGrayedOut = value;
                NotifyPropertyChanged(nameof(IsAutoBrightnessRangeLevelTextGrayedOut));
            }
        }

        private bool _isAutoPrimaryMonitorForSyncTextGrayedOut;

        public bool IsAutoPrimaryMonitorForSyncTextGrayedOut
        {
            get => _isAutoPrimaryMonitorForSyncTextGrayedOut;
            set
            {
                _isAutoPrimaryMonitorForSyncTextGrayedOut = value;
                NotifyPropertyChanged(nameof(IsAutoPrimaryMonitorForSyncTextGrayedOut));
            }
        }

        private bool _isAutoBrightnessRangeLevelStringGrayedOut;

        public bool IsAutoBrightnessRangeLevelStringGrayedOut
        {
            get => _isAutoBrightnessRangeLevelStringGrayedOut;
            set
            {
                _isAutoBrightnessRangeLevelStringGrayedOut = value;
                NotifyPropertyChanged(nameof(IsAutoBrightnessRangeLevelStringGrayedOut));
            }
        }

        public Visibility isAlsSupported { get; set; } = Visibility.Collapsed;

        /// <summary>
        /// Set VCP command to monitor
        /// </summary>
        /// <param name="alsConfig">Monitor Info</param>
        /// <param name="type">ALS Feature Query Type</param>
        /// <param name="val">empty</param>
        /// <returns>Sucess or Fail</returns>
        public bool SetALSAll(ALSConfig alsConfig, ALSFeatureQueryType type, int val)
        {
            return DdpmCommonHelper.DeviceManagerSA.SetALSFeatureValue(SelectedHomeDevice.MonitorInfo, Start_ALSConfig, ALSFeatureQueryType.All, "").Result;
        }

        public bool SupportedAutoBrightness { get; set; } = true;

        private bool _autoBrightnessStatus = false;

        /// <summary>
        /// Binding Auto Brightness element
        /// </summary>
        public bool AutoBrightnessStatus
        {
            get
            {
                _autoBrightnessStatus = Start_ALSConfig.isAutoBrightness;
                return _autoBrightnessStatus;
            }
            set
            {
                _autoBrightnessStatus = value;
                Start_ALSConfig.isAutoBrightness = value;
                ALSSettingsChangesOnNonPrimary(value);
                NotifyPropertyChanged("AutoBrightnessStatus");
                NotifyPropertyChanged("AutoBrightness_String");
                NotifyPropertyChanged("AutoBrightnessRangeLevelVisible");
                NotifyPropertyChanged("AutoBrightnessRangeLevelVisible_invert");
                Update_SupportedPrimaryMonitorSync(value, _autoColorTempStatus);
                if (_autoBrightnessStatus == false)
                {
                    //DDPMW-770
                    //CheckIfNeedToTurnPrimarySyncOff();
                    return;
                }
                //story: https://jira.cpg.dell.com/browse/DDPMW-769, set color preset to custom at the same time
                if (DdpmCommonHelper.DeviceManagerSA != null && ModuleOwner.SelectedHomeDevice != null)
                {
                    MonitorInfo? mo = ModuleOwner.SelectedHomeDevice.MonitorInfo;
                    List<string> presets = DdpmCommonHelper.DeviceManagerSA.ReadColorPreset(mo).Result;
                    int idx = presets.FindIndex(x => x.ToUpper().Contains("CUSTOM"));
                    if (idx >= 0)
                    {
                        _ = DdpmCommonHelper.DeviceManagerSA.SetVCPCapability(mo, "colorpreset", presets[idx]).Result;
                    }
                }
            }
        }

        /// <summary>
        /// Auto Brightness Binding data
        /// </summary>
        public void Update_AutoBrightnessStatus(bool value)
        {
            _autoBrightnessStatus = value;
            NotifyPropertyChanged("AutoBrightnessStatus");
            NotifyPropertyChanged("AutoBrightness_String");
            NotifyPropertyChanged("AutoBrightnessRangeLevelVisible");
            NotifyPropertyChanged("AutoBrightnessRangeLevelVisible_invert");
        }

        /// <summary>
        /// Binding Auto Brightness String
        /// </summary>
        public string AutoBrightness_String
        {
            get => Start_ALSConfig.isAutoBrightness ? "ON" : "OFF";
        }

        public bool SupportedAutoColorTemp { get; set; } = true;

        private bool _autoColorTempStatus = false;

        /// <summary>
        /// Auto Color Temp Binding data
        /// </summary>
        public bool AutoColorTempStatus
        {
            get
            {
                _autoColorTempStatus = Start_ALSConfig.isAutoColorTemp;
                return _autoColorTempStatus;
            }
            set
            {
                _autoColorTempStatus = value;
                Start_ALSConfig.isAutoColorTemp = value;
                ALSSettingsChangesOnNonPrimary(value);
                NotifyPropertyChanged("AutoColorTempStatus");
                NotifyPropertyChanged("AutoColorTemp_String");
                //Update_SupportedPrimaryMonitorSync(_autoBrightnessStatus, value);
            }
        }

        /// <summary>
        /// Auto Color Temp Binding data
        /// </summary>
        public void Update_AutoColorTempStatus(bool value)
        {
            _autoColorTempStatus = value;
            NotifyPropertyChanged("AutoColorTempStatus");
            NotifyPropertyChanged("AutoColorTemp_String");
        }

        /// <summary>
        /// Binding AutoColorTemp String element
        /// </summary>
        public string AutoColorTemp_String
        {
            get => Start_ALSConfig.isAutoColorTemp ? "ON" : "OFF";
        }

        private bool _supportedPrimaryMonitorSync = true;

        /// <summary>
        /// Comtrol PrimaryMonitorSyncStatus element
        /// </summary>
        public bool SupportedPrimaryMonitorSync
        {
            get
            {
                return _supportedPrimaryMonitorSync;
            }
            set
            {
                _supportedPrimaryMonitorSync = value;
                NotifyPropertyChanged("PrimaryMonitorForSyncVisible");
                NotifyPropertyChanged("PrimaryMonitorForSyncVisible_invert");
            }
        }

        private bool _primaryMonitorSyncStatus = false;

        /// <summary>
        /// Binding PrimaryMonitorSyncStatus element
        /// </summary>
        public bool PrimaryMonitorSyncStatus
        {
            get
            {
                _primaryMonitorSyncStatus = Start_ALSConfig.isPrimaryMonitorSync;
                return _primaryMonitorSyncStatus;
            }
            set
            {
                _primaryMonitorSyncStatus = value;
                Start_ALSConfig.isPrimaryMonitorSync = value;
                ALSSettingsChangesOnNonPrimary(value);
                NotifyPropertyChanged("PrimaryMonitorSyncStatus");
                NotifyPropertyChanged("PrimaryMonitorSync_String");
            }
        }

        /// <summary>
        /// Update PrimaryMonitorSyncStatus element
        /// </summary>
        /// <param name="value"></param>
        public void Update_PrimaryMonitorSyncStatus(bool value)
        {
            _primaryMonitorSyncStatus = value;
            NotifyPropertyChanged("PrimaryMonitorSyncStatus");
            NotifyPropertyChanged("PrimaryMonitorSync_String");
        }

        /// <summary>
        /// PrimaryMonitorSync String Binding data
        /// </summary>
        public string PrimaryMonitorSync_String
        {
            get => Start_ALSConfig.isPrimaryMonitorSync ? "ON" : "OFF";
        }

        /// <summary>
        /// Control AutoBrightnessRangeLevel component
        /// </summary>
        public Visibility AutoBrightnessRangeLevelVisible
        {
            get
            {
                if (AutoBrightnessStatus)
                    return Visibility.Visible;
                else
                    return Visibility.Collapsed;
            }
        }

        /// <summary>
        /// Control AutoBrightnessRangeLevel component
        /// </summary>
        public Visibility AutoBrightnessRangeLevelVisible_invert
        {
            get
            {
                if (AutoBrightnessStatus)
                    return Visibility.Collapsed;
                else
                    return Visibility.Visible;
            }
        }

        private List<AutoBrightnessRangeLevel> _autoBrightnessRangeLevel { get; set; }

        public void Update_AutoBrightnessRangeLevelStatus(List<AutoBrightnessRangeLevel> value)
        {
            if (_autoBrightnessRangeLevel == null || _autoBrightnessRangeLevel[0].level_value != value[0].level_value)
            {
                _autoBrightnessRangeLevel = value;
                _autoBrightnessRangeLevel[0].level_value = value[0].level_value;
                if (value[0].level_value == 0)
                    _autoBrightnessRangeLevel[0].level_name = "Low";
                else if (value[0].level_value == 1)
                    _autoBrightnessRangeLevel[0].level_name = "Mid";
                else
                    _autoBrightnessRangeLevel[0].level_name = "High";
                NotifyPropertyChanged("AutoBrightnessRangeLevel_SelectedIndex");
                NotifyPropertyChanged("AutoBrightnessRangeLevel_String");
            }
        }

        public int AutoBrightnessSelectedIndex { get; set; } = 0;
        public List<string> AutoBrightnessRangeLevel { get; set; } = new List<string>() { "Low", "Mid", "High" }; //mapping to 40%, 60%, 100%

        /// <summary>
        /// AutoBrightnessRangeLevel String Binding data
        /// </summary>
        public string AutoBrightnessRangeLevel_String// { get; set; } = "Brightness level: 40%";
        {
            get
            {
                if (Start_ALSConfig.AutoBrightnessRangeLevel.Count == 0)
                    return "";
                if (Start_ALSConfig.AutoBrightnessRangeLevel[0].level_value == 0)
                    return "Brightness level: 40%";
                else if (Start_ALSConfig.AutoBrightnessRangeLevel[0].level_value == 1)
                    return "Brightness level: 60%";
                else
                    return "Brightness level: 100%";
            }
        }

        /// <summary>
        /// AutoBrightnessRangeLevel SelectedIndex Binding source
        /// </summary>
        public int AutoBrightnessRangeLevel_SelectedIndex
        {
            get
            {
                if (Start_ALSConfig.AutoBrightnessRangeLevel.Count == 0)
                    return 0;
                return (int)Start_ALSConfig.AutoBrightnessRangeLevel[0].level_value;
            }
            set
            {
                if ((int)Start_ALSConfig.AutoBrightnessRangeLevel[0].level_value != value)
                {
                    Start_ALSConfig.AutoBrightnessRangeLevel[0].level_value = value;
                    if (value == 0)
                        Start_ALSConfig.AutoBrightnessRangeLevel[0].level_name = "Low";
                    else if (value == 1)
                        Start_ALSConfig.AutoBrightnessRangeLevel[0].level_name = "Mid";
                    else
                        Start_ALSConfig.AutoBrightnessRangeLevel[0].level_name = "High";
                    SetALSAll(Start_ALSConfig, ALSFeatureQueryType.PrimaryMonitorSync, 0);
                    NotifyPropertyChanged("AutoBrightnessRangeLevel_String");
                }
            }
        }

        /// <summary>
        /// Control PrimaryMonitorSync component
        /// </summary>
        public Visibility PrimaryMonitorForSyncVisible
        {
            get
            {
                if (SupportedPrimaryMonitorSync)
                    return Visibility.Visible;
                else
                    return Visibility.Collapsed;
            }
        }

        /// <summary>
        /// Control PrimaryMonitorSync component
        /// </summary>
        public Visibility PrimaryMonitorForSyncVisible_invert
        {
            get
            {
                if (SupportedPrimaryMonitorSync)
                    return Visibility.Collapsed;
                else
                    return Visibility.Visible;
            }
        }

        public void Update_SupportedPrimaryMonitorSync(bool isAutoBrightness, bool isAutoColorTemp)
        {
            if (isAutoBrightness == false && isAutoColorTemp == false)
            {
                _supportedPrimaryMonitorSync = false;
                NotifyPropertyChanged("PrimaryMonitorForSyncVisible");
                NotifyPropertyChanged("PrimaryMonitorForSyncVisible_invert");
                NotifyPropertyChanged("SupportedPrimaryMonitorSync");
            }
            else
            {
                _supportedPrimaryMonitorSync = true;
                NotifyPropertyChanged("PrimaryMonitorForSyncVisible");
                NotifyPropertyChanged("PrimaryMonitorForSyncVisible_invert");
                NotifyPropertyChanged("SupportedPrimaryMonitorSync");
            }
        }

        /// <summary>
        /// DDPMW-784 Check All Monitor ALS Status
        /// </summary>
        /// <returns>True is > 2 ALS monitor and some one monitor PrimaryMonitorSync on</returns>
        public bool CheckMonitorALSStatus()
        {
            List<ALSConfig> alsList = new List<ALSConfig>();
            alsList = DdpmCommonHelper.DeviceManagerSA.GetAllExistAlsConfig().Result;
            int alsSupportCount = 0;
            int alsPrimaryMSOn = 0;
            foreach (var als in alsList)
            {
                if (als.isSupportALS == 2)
                {
                    alsSupportCount++;
                    if (als.isPrimaryMonitorSync)
                        alsPrimaryMSOn++;
                }
            }
            if (alsSupportCount <= 1)//1 ALS monitor Not processed, 0 or 1 ALS monitor
                return false;
            else
            {
                if (alsPrimaryMSOn == 0)// > 2 LS monitor, but PrimaryMonitorSync all off
                    return false;
                else
                {
                    return true;
                }
            }
        }

        #endregion ALS functions

        public Visibility isShowSynchronize { get; set; } = Visibility.Visible;

        #region UI Enable Flags

        private bool _isBusy = true;

        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        #endregion UI Enable Flags
    }
}