using CommunityToolkit.Mvvm.ComponentModel;
using DDPM.SA.Common;
using DDPM.SA.Common.Settings;
using DDPM.UI.Common;
using DDPM.UI.Common.Interfaces;
using DDPM.UI.Common.Models;
using Newtonsoft.Json;
using DDPM.UI.Common.Views;
using Dell.Client.Framework.UX.WPF.Dialogs.WPF;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using VcpCore.Common;
using static DDPM.UI.Common.Views.DDPMMsgBox;
using System.Diagnostics;
using DDPM.SA.Common.Display;
using Windows.System;
using System.Reflection;
using System.Runtime.CompilerServices;
using Dell.Client.Framework.Common;

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

        private ImageSource? _BrightnessImage;
        private ImageSource? _ContrastImage;
        private ImageSource? _LuminanceImage;

        private double Luminance_Value = -1;
        private double Brightness_Value = -1;
        private double Contrast_Value = -1;
        private double LuminanceMax_Value = -1;

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
        #endregion

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
                if (mo == null)
                    return;

                Luminance_Value = Convert.ToDouble(e.value);
                Brightness_Value = Convert.ToDouble(e.value);

                NotifyPropertyChanged("LuminanceValue");
                NotifyPropertyChanged("BrightnessValue");
            }
            else if (e.vcpcode.Equals("12"))
            {
                MonitorInfo? mo = DdpmCommonHelper.ModuleOwner?.SelectedHomeDevice?.MonitorInfo;
                if (mo == null)
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
                    }
                }

                //OSD control back event
                DdpmCommonHelper.DeviceManagerSA.VCPchanged += OnVCPChangedEvent;
                Trace.WriteLine($"7 {DateTime.Now.ToString("MM/dd/yyyy hh:mm ss fff")}");
            }
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
                Contrast_Value = value;
                Contrast_Debouncer.Debounce(value);
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
                DdpmCommonHelper.Settings_Cache = DdpmCommonHelper.DeviceManagerSA.ReloadAppConfigData().Result;
            IsSynchronizeMonitor = DdpmCommonHelper.Settings_Cache.UserSettings.IsSynchronizemonitor;
            IsGetSynchronizeMonitor = true;
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


        private void Set_Luminance_Value(double value)
        {
            uint nNewValue = Convert.ToUInt32(value);

            DdpmCommonHelper.DeviceManagerSA.SetVCPCapability(ModuleOwner.SelectedHomeDevice.MonitorInfo, 0x10, nNewValue);

            //NotifyPropertyChanged("LuminanceValue");
        }

        private void Set_Brightness_Value(double value)
        {
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

        private void Set_Contrast_Value(double value)
        {

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
        #endregion

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

        #endregion

        public Visibility isShowSynchronize { get; set; } = Visibility.Visible;

        #region UI Enable Flags
        private bool _isBusy = true;
        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }
        #endregion
    }
}
