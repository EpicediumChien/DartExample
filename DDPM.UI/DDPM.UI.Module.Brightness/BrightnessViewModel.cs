using CommunityToolkit.Mvvm.ComponentModel;
using DDPM.SA.Common;
using DDPM.SA.Common.Display;
using DDPM.SA.Common.Settings;
using DDPM.UI.Common;
using DDPM.UI.Common.Interfaces;
using DDPM.UI.Common.Models;
using DDPM.UI.Plugin.Common;
using DDPM.UI.Resources.Helper;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.UX.WPF.Controls;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;
using VcpCore.Common;
using Windows.System;

[assembly: InternalsVisibleTo("DDPM.UI.Module.Brightness.Tests")]
[assembly: InternalsVisibleTo("DDPM.UI.Common.Tests")]
[assembly: InternalsVisibleTo("DDPM.UI.Plugin.Common.Tests")]

namespace DDPM.UI.Module.Brightness
{
    internal class BrightnessViewModel : ObservableObject, INotifyPropertyChanged
    {
        public Debouncer Brightness_Debouncer;

        public Debouncer Contrast_Debouncer;

        public bool IsPR1_Luminance_Preview_ = false;

        public bool IsPR1Preview_ = false;

        public bool IsPR2_Luminance_Preview_ = false;

        public bool IsPR2Preview_ = false;

        public bool isUserControlUI = true;

        public Debouncer Luminance_Debouncer;

        public Debouncer PR1Brightness_Debouncer;

        public Debouncer PR1Contrast_Debouncer;

        public Debouncer PR1Luminance_Debouncer;

        public Debouncer PR2Brightness_Debouncer;

        public Debouncer PR2Contrast_Debouncer;

        public Debouncer PR2Luminance_Debouncer;

        public CancellationTokenSource PreviewToken = new CancellationTokenSource();

        public scheduleInfo ScheduleMap = new scheduleInfo();

        public ALSConfig Start_ALSConfig = new ALSConfig();

        private static readonly object ExecutorLock = new object();

        private static BrightnessViewModel INSTANCE = null;

        private static BackgroundWorker syncUIvalue_bw = new BackgroundWorker();

        private Visibility _AutoALSLock = Visibility.Collapsed;

        private Visibility _AutoBrightnessLock = Visibility.Collapsed;

        private bool _autoBrightnessStatus = false;

        private Visibility _AutoBrigRangeLevelLock = Visibility.Collapsed;

        private bool _autoColorTempStatus = false;

        private Visibility _AutoTemperatureLock = Visibility.Collapsed;

        private string _brightnessAddKey = LangHelper.Instance["None"];

        private ImageSource? _BrightnessImage;

        private string _brightnessMinsKey = LangHelper.Instance["None"];

        private string _contrastAddKey = LangHelper.Instance["None"];

        private ImageSource? _ContrastImage;

        private string _contrastMinsKey = LangHelper.Instance["None"];

        private int _expanderGroup = 0;

        private Visibility _isAutoBrightnessLockMask = Visibility.Collapsed;

        private bool _isAutoBrightnessRangeLevelStringGrayedOut;

        private Visibility _isAutoBrightnessRangeLevelStringLockMask = Visibility.Collapsed;

        private bool _isAutoBrightnessRangeLevelTextGrayedOut;

        private bool _isAutoBrightnessTextGrayedOut;

        private Visibility _isAutoBrigRangeLevelLockMask = Visibility.Collapsed;

        private bool _isAutoColorTempTextGrayedOut;

        private bool _isAutoPrimaryMonitorForSyncTextGrayedOut;

        private Visibility _isAutoTemperatureLockMask = Visibility.Collapsed;

        private Visibility _isAutoTemperatureSwitchLockMask = Visibility.Collapsed;

        private bool _isBusy = true;

        private bool _isBusyALS = false;

        private bool _isDarkTheme;

        private bool _isLuminanceSupport = false;

        private Visibility _isPrimaryMonitorSyncLockMask = Visibility.Collapsed;

        private bool _isSynchronizeDisabled;

        private Visibility _LockMaskVisible = Visibility.Collapsed;

        private string _luminanceAddKey = LangHelper.Instance["None"];

        private ImageSource? _LuminanceImage;

        private string _luminanceMinsKey = LangHelper.Instance["None"];

        private Visibility _PrimaryMonitorSyncLock = Visibility.Collapsed;

        private bool _primaryMonitorSyncStatus = false;

        private bool _supportedPrimaryMonitorSync = true;

        private Visibility _synchronizeLock = Visibility.Collapsed;

        private string _TabSTOP = "Cycle";

        private double Brightness_Value = -1;

        private double Contrast_Value = -1;

        private int dUration_1 = -1;

        private int dUration_2 = -1;

        private int hOurs_1 = -1;

        private int hOurs_2 = -1;

        private bool IsBrightnessEnable = false;

        private bool IsGetSynchronizeMonitor = false;

        private bool isLuminance = false;

        private bool IsSynchronizeMonitor = false;

        private bool IsSynchronizeMonitor_Scheduled = false;

        private double Luminance_Value = -1;

        private double LuminanceMax_Value = -1;

        private int mIns_1 = -1;

        private int mIns_2 = -1;

        private string PR1_Luminance_Name = string.Empty;

        private string PR1_Name = string.Empty;

        private double PR1Brightness_Value = -1;

        private double PR1Contrast_Value = -1;

        private double PR1Luminance_Value = -1;

        private string PR2_Luminance_Name = string.Empty;

        private string PR2_Name = string.Empty;

        private double PR2Brightness_Value = -1;

        private double PR2Contrast_Value = -1;

        private double PR2Luminance_Value = -1;

        public BrightnessViewModel()
        {
            DdpmCommonHelper.WriteUILog($"BrightnessViewModel in ...");
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
            Contrast_Debouncer = new Debouncer(1500, Set_Contrast_Value);
            Brightness_Debouncer = new Debouncer(1500, Set_Brightness_Value);
            Luminance_Debouncer = new Debouncer(1500, Set_Luminance_Value);
            PR1Contrast_Debouncer = new Debouncer(1500, Set_Contrast_Value);
            PR1Brightness_Debouncer = new Debouncer(1500, Set_Brightness_Value);
            PR2Contrast_Debouncer = new Debouncer(1500, Set_Contrast_Value);
            PR2Brightness_Debouncer = new Debouncer(1500, Set_Brightness_Value);
            PR1Luminance_Debouncer = new Debouncer(1500, Set_Luminance_Value);
            PR2Luminance_Debouncer = new Debouncer(1500, Set_Luminance_Value);

            DdpmCommonHelper.MyConsole?.RegisterForEvent("DisplayHDRStatusChanged", OnHDRChangedEvent);
            DdpmCommonHelper.BitmapImageUpdated += ALSFontColorUpdate;

            DdpmCommonHelper.WriteUILog($"BrightnessViewModel out ...");
        }

        ~BrightnessViewModel()
        {
            if (DdpmCommonHelper.DeviceManagerSA != null)
            {
                DdpmCommonHelper.DeviceManagerSA.VCPchanged -= OnVCPChangedEvent;
                DdpmCommonHelper.BitmapImageUpdated -= ALSFontColorUpdate;
            }

            DdpmCommonHelper.MyConsole.UnregisterForEvent("DisplayHDRStatusChanged", OnHDRChangedEvent);
        }

        public new event PropertyChangedEventHandler? PropertyChanged;

        public Visibility AutoALSLock
        {
            get => _AutoALSLock;
            set
            {
                _AutoALSLock = value;
                NotifyPropertyChanged(nameof(AutoALSLock));
            }
        }

        /// <summary>
        /// Binding Auto Brightness String
        /// </summary>
        public string AutoBrightness_String
        {
            get => Start_ALSConfig.isAutoBrightness ? LangHelper.Instance["On"] : LangHelper.Instance["Off"];
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

        public List<string> AutoBrightnessRangeLevel { get; set; } =
            new List<string>() { LangHelper.Instance["AutoBrightnessRangeLevel.0"], LangHelper.Instance["AutoBrightnessRangeLevel.1"], LangHelper.Instance["AutoBrightnessRangeLevel.2"] };

        /// <summary>
        /// AutoBrightnessRangeLevel SelectedIndex Binding source
        /// </summary>
        public int AutoBrightnessRangeLevel_SelectedIndex
        {
            get
            {
                if (Start_ALSConfig.AutoBrightnessRangeLevel == null)
                    return 0;
                return (int)Start_ALSConfig.AutoBrightnessRangeLevel.level_value;
            }
            set
            {
                IsBusy = true;
                NotifyPropertyChanged("IsBusy");
                DdpmCommonHelper.WriteUILog($"AutoBrightnessRangeLevel_SelectedIndex IsBusy : true ...");
                MyModule.GetRightView().Dispatcher.Invoke((Action)(() =>
                {
                    if ((int)Start_ALSConfig.AutoBrightnessRangeLevel.level_value != value)
                    {
                        bool popMessageResult = false;
                        ALSSettingsChangesOnNonPrimary(ref popMessageResult, "AUTOBRILEVEL", value);

                        if ((int)Start_ALSConfig.AutoBrightnessRangeLevel.level_value == value)//already apply, make string change
                        {
                            if (value == 0)
                                Start_ALSConfig.AutoBrightnessRangeLevel.level_name = LangHelper.Instance["AutoBrightnessRangeLevel.0"]; //"Low";
                            else if (value == 1)
                                Start_ALSConfig.AutoBrightnessRangeLevel.level_name = LangHelper.Instance["AutoBrightnessRangeLevel.1"]; //"Mid";
                            else
                                Start_ALSConfig.AutoBrightnessRangeLevel.level_name = LangHelper.Instance["AutoBrightnessRangeLevel.2"]; //"High";
                        }
                    }
                    NotifyPropertyChanged("AutoBrightnessRangeLevel_SelectedIndex");
                    NotifyPropertyChanged("AutoBrightnessRangeLevel_String");
                    IsBusy = false;
                    NotifyPropertyChanged("IsBusy");
                    DdpmCommonHelper.WriteUILog($"AutoBrightnessRangeLevel_SelectedIndex IsBusy : false ...");
                }));
            }
        }

        /// <summary>
        /// AutoBrightnessRangeLevel String Binding data
        /// </summary>
        public string AutoBrightnessRangeLevel_String// { get; set; } = "Brightness level: 40%";
        {
            get
            {
                if (Start_ALSConfig.AutoBrightnessRangeLevel == null)
                    return "";

                //all return same string? no need to use if-else
                /*
                if (Start_ALSConfig.AutoBrightnessRangeLevel[0].level_value == 0)
                    return LangHelper.Instance["BrightnessLevel"] + ": " + BrightnessValue.ToString() + "%";//40%";
                else if (Start_ALSConfig.AutoBrightnessRangeLevel[0].level_value == 1)
                    return LangHelper.Instance["BrightnessLevel"] + ": " + BrightnessValue.ToString() + "%";//60%";
                else
                    return LangHelper.Instance["BrightnessLevel"] + ": " + BrightnessValue.ToString() + "%";//100%";
                */

                return LangHelper.Instance["BrightnessLevel"] + ": " + BrightnessValue.ToString() + "%";
            }
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

        public int AutoBrightnessSelectedIndex { get; set; } = 0;

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
                IsBusy = true;
                NotifyPropertyChanged("IsBusy");
                MyModule.GetRightView().Dispatcher.Invoke((Action)(() =>
                {
                    bool popMessageResult = value;
                    _autoBrightnessStatus = value;
                    Start_ALSConfig.isAutoBrightness = value;
                    ALSSettingsChangesOnNonPrimary(ref popMessageResult);
                    NotifyPropertyChanged("AutoBrightnessStatus");
                    NotifyPropertyChanged("AutoBrightness_String");
                    NotifyPropertyChanged("AutoBrightnessRangeLevelVisible");
                    NotifyPropertyChanged("AutoBrightnessRangeLevelVisible_invert");
                    Update_SupportedPrimaryMonitorSync(value, _autoColorTempStatus);
                    if (_autoBrightnessStatus == false)
                    {
                        //DDPMW-770
                        //CheckIfNeedToTurnPrimarySyncOff();
                        IsBusy = false;
                        NotifyPropertyChanged("IsBusy");
                        DdpmCommonHelper.WriteUILog($"AutoBrightnessStatus IsBusy : false ...");
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
                    IsBusy = false;
                    NotifyPropertyChanged("IsBusy");
                    DdpmCommonHelper.WriteUILog($"AutoBrightnessStatus IsBusy : false ...");
                }));
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

        /// <summary>
        /// Binding AutoColorTemp String element
        /// </summary>
        public string AutoColorTemp_String
        {
            get => Start_ALSConfig.isAutoColorTemp ? LangHelper.Instance["On"] : LangHelper.Instance["Off"];
        }

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
                IsBusy = true;
                NotifyPropertyChanged("IsBusy");
                DdpmCommonHelper.WriteUILog($"AutoColorTempStatus IsBusy : true ...");
                MyModule.GetRightView().Dispatcher.Invoke((Action)(() =>
                {
                    bool popMessageResult = value;
                    _autoColorTempStatus = value;
                    Start_ALSConfig.isAutoColorTemp = value;
                    ALSSettingsChangesOnNonPrimary(ref popMessageResult, "AUTOCOLOR");
                    NotifyPropertyChanged("AutoColorTempStatus");
                    NotifyPropertyChanged("AutoColorTemp_String");
                    //Update_SupportedPrimaryMonitorSync(_autoBrightnessStatus, value);
                    IsBusy = false;
                    NotifyPropertyChanged("IsBusy");
                    DdpmCommonHelper.WriteUILog($"AutoColorTempStatus IsBusy : false ...");
                }));
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

        public bool BrightnessEnable
        {
            get
            {
                NotifyPropertyChanged(nameof(BrightnessOpacity));
                NotifyPropertyChanged(nameof(GreayoutAlart));
                return IsBrightnessEnable;
            }
        }

        public ImageSource? BrightnessImage
        {
            get => _BrightnessImage;
            set => SetProperty(ref _BrightnessImage, value);
        }

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
                Brightness_Value = value;
                Brightness_Debouncer.Debounce(value);
                NotifyPropertyChanged("BrightnessValue");
                NotifyPropertyChanged("AutoBrightnessRangeLevel_String");
            }
        }

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

        public ImageSource? ContrastImage
        {
            get => _ContrastImage;
            set => SetProperty(ref _ContrastImage, value);
        }

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

        public List<int> dUration { get; } = new List<int>() { 0, 15, 30, 45, 60 };

        public int dUration1
        {
            get
            {
                return dUration_1;
            }
            set
            {
                dUration_1 = value;

                if (ScheduleMap != null)
                    ScheduleMap.Duration1 = dUration_1;
                else
                    ScheduleMap = new scheduleInfo() { Duration1 = dUration_1 };

                NotifyPropertyChanged("dUration1");
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

                if (ScheduleMap != null)
                    ScheduleMap.Duration2 = dUration_2;
                else
                    ScheduleMap = new scheduleInfo() { Duration2 = dUration_2 };

                NotifyPropertyChanged("dUration2");
            }
        }

        //Robert_Lin, 2024-6-3 added
        //
        public int ExpanderGroup
        {
            get => _expanderGroup;
            set => SetProperty(ref _expanderGroup, value);
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

        public List<int> hOurs { get; } = Enumerable.Range(1, 12).ToList();

        public int hOurs1
        {
            get
            {
                return hOurs_1;
            }
            set
            {
                hOurs_1 = value;

                if (ScheduleMap != null)
                    ScheduleMap.Hours1 = hOurs_1;
                else
                    ScheduleMap = new scheduleInfo() { Hours1 = hOurs_1 };

                NotifyPropertyChanged("hOurs1");
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

                if (ScheduleMap != null)
                    ScheduleMap.Hours2 = hOurs_2;
                else
                    ScheduleMap = new scheduleInfo() { Hours2 = hOurs_2 };

                NotifyPropertyChanged("hOurs2");
            }
        }

        public Visibility isAlsSupported { get; set; } = Visibility.Collapsed;

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

        public bool IsAutoBrightnessRangeLevelStringGrayedOut
        {
            get => _isAutoBrightnessRangeLevelStringGrayedOut;
            set
            {
                _isAutoBrightnessRangeLevelStringGrayedOut = value;
                NotifyPropertyChanged(nameof(IsAutoBrightnessRangeLevelStringGrayedOut));
            }
        }

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

        public bool IsAutoBrightnessRangeLevelTextGrayedOut
        {
            get => _isAutoBrightnessRangeLevelTextGrayedOut;
            set
            {
                _isAutoBrightnessRangeLevelTextGrayedOut = value;
                NotifyPropertyChanged(nameof(IsAutoBrightnessRangeLevelTextGrayedOut));
            }
        }

        public bool IsAutoBrightnessTextGrayedOut
        {
            get => _isAutoBrightnessTextGrayedOut;
            set
            {
                _isAutoBrightnessTextGrayedOut = value;
                NotifyPropertyChanged(nameof(IsAutoBrightnessTextGrayedOut));
            }
        }

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

        public bool IsAutoColorTempTextGrayedOut
        {
            get => _isAutoColorTempTextGrayedOut;
            set
            {
                _isAutoColorTempTextGrayedOut = value;
                NotifyPropertyChanged(nameof(IsAutoColorTempTextGrayedOut));
            }
        }

        public bool IsAutoPrimaryMonitorForSyncTextGrayedOut
        {
            get => _isAutoPrimaryMonitorForSyncTextGrayedOut;
            set
            {
                _isAutoPrimaryMonitorForSyncTextGrayedOut = value;
                NotifyPropertyChanged(nameof(IsAutoPrimaryMonitorForSyncTextGrayedOut));
            }
        }

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

        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        public bool IsBusyALS
        {
            get => _isBusyALS;
            set => SetProperty(ref _isBusyALS, value);
        }

        public bool IsDarkTheme
        {
            get => _isDarkTheme;
            set
            {
                if (_isDarkTheme != value)
                {
                    _isDarkTheme = value;
                    OnPropertyChanged(nameof(IsDarkTheme));
                }
            }
        }

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

        public bool IsMouseEnterSchedule_1 { get; set; }

        public bool IsMouseEnterSchedule_2 { get; set; }

        public Visibility isNormalBrightness { get; set; } = Visibility.Visible;

        public bool IsPR_Luminance_Preview
        {
            get
            {
                return (IsPR1_Luminance_Preview || IsPR2_Luminance_Preview);
            }
        }

        public bool IsPR1_Luminance_Preview
        {
            get
            {
                return IsPR1_Luminance_Preview_;
            }
            set
            {
                IsPR1_Luminance_Preview_ = value;
                PR1_Luminance_ButtonContent = (value) ? "Stop Preview" : "Preview Changes";
                NotifyPropertyChanged("IsPR1_Luminance_Preview");
                NotifyPropertyChanged("IsPR_Luminance_Preview");
                NotifyPropertyChanged("PR1_Luminance_ButtonContent");
            }
        }

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

        public bool IsPR2_Luminance_Preview
        {
            get
            {
                return IsPR2_Luminance_Preview_;
            }
            set
            {
                IsPR2_Luminance_Preview_ = value;
                PR2_Luminance_ButtonContent = (value) ? "Stop Preview" : "Preview Changes";
                NotifyPropertyChanged("IsPR2_Luminance_Preview");
                NotifyPropertyChanged("IsPR_Luminance_Preview");
                NotifyPropertyChanged("PR2_Luminance_ButtonContent");
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

        public bool IsPRPreview
        {
            get
            {
                return (IsPR1Preview || IsPR2Preview);
            }
        }

        public Visibility IsScheduledLuminanceShow
        {
            get
            {
                if (isLuminanceSupport.Equals(Visibility.Visible))
                    return Visibility.Visible;
                else
                    return Visibility.Collapsed;
            }
        }

        public Visibility IsScheduledShow
        {
            get
            {
                if (isAlsSupported.Equals(Visibility.Collapsed) || isAlsSupported.Equals(Visibility.Hidden))
                {
                    if (isLuminanceSupport.Equals(Visibility.Visible))
                        return Visibility.Collapsed;
                    else
                        return Visibility.Visible;
                }
                else
                    return Visibility.Collapsed;
            }
        }

        public Visibility isScheduleSupport { get; set; } = Visibility.Collapsed;

        public Visibility isShowSynchronize { get; set; } = Visibility.Visible;

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
                NotifyPropertyChanged("IsSynchronize_String");
            }
        }

        public bool IsSynchronize_Scheduled
        {
            get
            {
                return IsSynchronizeMonitor_Scheduled;
            }
            set
            {
                IsSynchronizeMonitor_Scheduled = value;
                NotifyPropertyChanged("IsSynchronize_Scheduled");
                NotifyPropertyChanged("IsSynchronize_Scheduled_String");
            }
        }

        public string IsSynchronize_Scheduled_String
        {
            get
            {
                return IsSynchronize_Scheduled ? LangHelper.Instance["On"] : LangHelper.Instance["Off"];
            }
        }

        public string IsSynchronize_String
        {
            get
            {
                return IsSynchronize ? LangHelper.Instance["On"] : LangHelper.Instance["Off"];
            }
        }

        public bool IsSynchronizeDisabled
        {
            get { return _isSynchronizeDisabled; }
            set
            {
                _isSynchronizeDisabled = value;
                OnPropertyChanged(nameof(IsSynchronizeDisabled));
            }
        }

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

        public ImageSource? LuminanceImage
        {
            get => _LuminanceImage;
            set => SetProperty(ref _LuminanceImage, value);
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

        public bool ManualBCHotkeyBtn { get; set; } = true;

        public List<int> mIns { get; } = Enumerable.Range(0, 60).ToList();

        public int mIns1
        {
            get
            {
                return mIns_1;
            }
            set
            {
                mIns_1 = value;

                if (ScheduleMap != null)
                    ScheduleMap.Mins1 = mIns_1;
                else
                    ScheduleMap = new scheduleInfo() { Mins1 = mIns_1 };

                NotifyPropertyChanged("mIns1");
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

                if (ScheduleMap != null)
                    ScheduleMap.Mins2 = mIns_2;
                else
                    ScheduleMap = new scheduleInfo() { Mins2 = mIns_2 };

                NotifyPropertyChanged("mIns2");
            }
        }

        public IModuleOwner? ModuleOwner { get; set; }

        public BrightnessModule MyModule { get; set; }

        public string PR1_Luminance_ButtonContent { get; set; } = LangHelper.Instance["Preview_Changes"];

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

                if (ScheduleMap != null)
                    ScheduleMap.Brightness1 = PR1Brightness_Value;
                else
                    ScheduleMap = new scheduleInfo() { Brightness1 = PR1Brightness_Value };

                NotifyPropertyChanged("PR1BrightnessValue");
            }
        }

        public string PR1ButtonContent { get; set; } = LangHelper.Instance["Preview_Changes"];

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

                if (ScheduleMap != null)
                    ScheduleMap.Contrast1 = PR1Contrast_Value;
                else
                    ScheduleMap = new scheduleInfo() { Contrast1 = PR1Contrast_Value };

                NotifyPropertyChanged("PR1ContrastValue");
            }
        }

        public double PR1LuminanceValue
        {
            get
            {
                if (PR1Luminance_Value < 0)
                {
                    if (isNormalBrightness.Equals(Visibility.Visible))
                    {
                        PR1Luminance_Value = 100;
                        return PR1Luminance_Value;
                    }
                    else
                    {
                        return 45;
                    }
                }
                else
                    return PR1Luminance_Value;
            }

            set
            {
                PR1Luminance_Value = value;
                PR1Luminance_Debouncer.Debounce(value);

                if (ScheduleMap != null)
                    ScheduleMap.Brightness1 = PR1Luminance_Value;
                else
                    ScheduleMap = new scheduleInfo() { Brightness1 = PR1Luminance_Value };

                NotifyPropertyChanged("PR1LuminanceValue");
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

                if (ScheduleMap != null)
                    ScheduleMap.Pre1Name = PR1_Name;
                else
                    ScheduleMap = new scheduleInfo() { Pre1Name = PR1_Name };

                NotifyPropertyChanged("PR1Name");
            }
        }

        //"Preview Changes";
        public string PR2_Luminance_ButtonContent { get; set; } = LangHelper.Instance["Preview_Changes"];

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

                if (ScheduleMap != null)
                    ScheduleMap.Brightness2 = PR2Brightness_Value;
                else
                    ScheduleMap = new scheduleInfo() { Brightness2 = PR2Brightness_Value };

                NotifyPropertyChanged("PR2BrightnessValue");
            }
        }

        // "Preview Changes";
        public string PR2ButtonContent { get; set; } = LangHelper.Instance["Preview_Changes"];

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

                if (ScheduleMap != null)
                    ScheduleMap.Contrast2 = PR2Contrast_Value;
                else
                    ScheduleMap = new scheduleInfo() { Contrast2 = PR2Contrast_Value };

                NotifyPropertyChanged("PR2ContrastValue");
            }
        }

        public double PR2LuminanceValue
        {
            get
            {
                if (PR2Luminance_Value < 0)
                {
                    if (isNormalBrightness.Equals(Visibility.Visible))
                    {
                        PR2Luminance_Value = 100;
                        return PR2Luminance_Value;
                    }
                    else
                    {
                        return 45;
                    }
                }
                else
                    return PR2Luminance_Value;
            }

            set
            {
                PR2Luminance_Value = value;
                PR1Luminance_Debouncer.Debounce(value);

                if (ScheduleMap != null)
                    ScheduleMap.Brightness2 = PR2Luminance_Value;
                else
                    ScheduleMap = new scheduleInfo() { Brightness2 = PR2Luminance_Value };

                NotifyPropertyChanged("PR2LuminanceValue");
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

                if (ScheduleMap != null)
                    ScheduleMap.Pre2Name = PR2_Name;
                else
                    ScheduleMap = new scheduleInfo() { Pre2Name = PR2_Name };

                NotifyPropertyChanged("PR2Name");
            }
        }

        //{ "Low", "Mid", "High" }; //mapping to 40%, 60%, 100%
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

        /// <summary>
        /// PrimaryMonitorSync String Binding data
        /// </summary>
        public string PrimaryMonitorSync_String
        {
            get => Start_ALSConfig.isPrimaryMonitorSync ? LangHelper.Instance["On"] : LangHelper.Instance["Off"];
        }

        //Brightness's value inverse the Luminance's value
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
                //PIMS-328260
                //_primaryMonitorSyncStatus = value;
                //Start_ALSConfig.isPrimaryMonitorSync = value;
                bool popMessageResult = value;
                ALSSettingsChangesOnNonPrimary(ref popMessageResult, "PRISYNC");
                NotifyPropertyChanged("PrimaryMonitorSyncStatus");
                NotifyPropertyChanged("PrimaryMonitorSync_String");
            }
        }

        public bool SupportedAutoBrightness { get; set; } = true;

        public bool SupportedAutoColorTemp { get; set; } = true;

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

        internal HomeDevice? SelectedHomeDevice { get; set; }

        private AutoBrightnessRangeLevel _autoBrightnessRangeLevel { get; set; }

        public static BrightnessViewModel GetInstance()
        {
            if (INSTANCE == null)
            {
                INSTANCE = new BrightnessViewModel();
            }
            return INSTANCE;
        }

        public bool AreAllConfigsNotBusy(List<ALSConfig> configs)
        {
            if (configs == null || configs.Count == 0 || !configs.Where(cfg => cfg.isSupportALS > 0).Skip(1).Any()) // isSupportALS At least two. If no, then return. one ALS don't do Busy Animation.
            {
                DdpmCommonHelper.WriteUILog($"AreAllConfigsNotBusy : return true ...");
                return true;
            }
            DdpmCommonHelper.WriteUILog($"AreAllConfigsNotBusy : config isBusy = {configs.All(config => !config.isBusy).ToString()} ...");
            return configs.All(config => !config.isBusy);
        }

        //"Preview Changes";
        public void BR_Con_Sync()
        {
            BackgroundWorker bw = new BackgroundWorker()
            {
                WorkerReportsProgress = false,
                WorkerSupportsCancellation = false
            };
            bw.DoWork += BRConSync;
            bw.RunWorkerCompleted += BRConSync_finish;
            bw.RunWorkerAsync();
            IsBusy = true;
            NotifyPropertyChanged("IsBusy");
            DdpmCommonHelper.WriteUILog($"BR_Con_Sync IsBusy : true ...");
        }

        public void CalculateNowValue()
        {
            if (hOurs1 > -1 && hOurs2 > -1 && mIns1 > -1 && mIns2 > -1 && dUration1 > -1 && dUration2 > -1)
            {
                var CurDateTime = DateTime.Now;
                var Brightness_PR1 = PR1BrightnessValue;
                var Brightness_PR2 = PR2BrightnessValue;
                var Luminance_PR1 = PR1LuminanceValue;
                var Luminance_PR2 = PR2LuminanceValue;
                var Brightness_difference = (isLuminanceSupport == Visibility.Visible) ? (Luminance_PR1 - Luminance_PR2) : (Brightness_PR1 - Brightness_PR2);
                var Contrast_PR1 = PR1ContrastValue;
                var Contrast_PR2 = PR2ContrastValue;
                var Contrast_difference = Contrast_PR1 - Contrast_PR2;
                bool IsBrightnessPR1Plus = Brightness_difference > 0 ? true : false;
                bool IsBrightnessPR2Plus = Brightness_difference > 0 ? false : true;
                bool IsLuminancePR1Plus = Brightness_difference > 0 ? true : false;
                bool IsLuminancePR2Plus = Brightness_difference > 0 ? false : true;
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
                var PerStepValue = (isLuminanceSupport == Visibility.Visible) ? 20 : 5;
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
                                {
                                    if ((isLuminanceSupport == Visibility.Visible))
                                        LuminanceValue = Luminance_PR1 + (PerStepValue * Brightness_steps);
                                    else
                                        BrightnessValue = Brightness_PR1 + (PerStepValue * Brightness_steps);
                                }
                            }
                            else
                            {
                                if ((!IsMouseEnterSchedule_1 && !IsMouseEnterSchedule_2) && (!IsPR1Preview && !IsPR2Preview))
                                {
                                    if ((isLuminanceSupport == Visibility.Visible))
                                        LuminanceValue = Luminance_PR1 - (PerStepValue * Brightness_steps);
                                    else
                                        BrightnessValue = Brightness_PR1 - (PerStepValue * Brightness_steps);
                                }
                            }

                            if (!(isLuminanceSupport == Visibility.Visible))
                            {
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
                        }
                        else  //CurDateTime is same as or earlier than Pre_PR2Time
                        {
                            if ((!IsMouseEnterSchedule_1 && !IsMouseEnterSchedule_2) && (!IsPR1Preview && !IsPR2Preview))
                            {
                                if ((isLuminanceSupport == Visibility.Visible))
                                    LuminanceValue = Luminance_PR1;
                                else
                                {
                                    BrightnessValue = Brightness_PR1;
                                    ContrastValue = Contrast_PR1;
                                }
                            }
                        }
                    }
                    else if (DateTime.Compare(CurDateTime, PR1Time) == 0) //The same as PR1Time
                    {
                        if ((!IsMouseEnterSchedule_1 && !IsMouseEnterSchedule_2) && (!IsPR1Preview && !IsPR2Preview))
                        {
                            if ((isLuminanceSupport == Visibility.Visible))
                                LuminanceValue = Luminance_PR1;
                            else
                            {
                                BrightnessValue = Brightness_PR1;
                                ContrastValue = Contrast_PR1;
                            }
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
                                {
                                    if ((isLuminanceSupport == Visibility.Visible))
                                        LuminanceValue = Luminance_PR2 + (PerStepValue * Brightness_steps);
                                    else
                                        BrightnessValue = Brightness_PR2 + (PerStepValue * Brightness_steps);
                                }
                            }
                            else
                            {
                                if ((!IsMouseEnterSchedule_1 && !IsMouseEnterSchedule_2) && (!IsPR1Preview && !IsPR2Preview))
                                {
                                    if ((isLuminanceSupport == Visibility.Visible))

                                        LuminanceValue = Luminance_PR2 - (PerStepValue * Brightness_steps);
                                    else
                                        BrightnessValue = Brightness_PR2 - (PerStepValue * Brightness_steps);
                                }
                            }

                            if (!(isLuminanceSupport == Visibility.Visible))
                            {
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
                        }
                        else  //CurDateTime is same as  or  earlier than Pre_PR1Time
                        {
                            if ((!IsMouseEnterSchedule_1 && !IsMouseEnterSchedule_2) && (!IsPR1Preview && !IsPR2Preview))
                            {
                                if (!(isLuminanceSupport == Visibility.Visible))
                                {
                                    BrightnessValue = Brightness_PR2;
                                    ContrastValue = Contrast_PR2;
                                }
                                else
                                    LuminanceValue = Luminance_PR2;
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
                                {
                                    if (!(isLuminanceSupport == Visibility.Visible))
                                        BrightnessValue = Brightness_PR2 + (PerStepValue * Brightness_steps);
                                    else
                                        LuminanceValue = Luminance_PR2 + (PerStepValue * Brightness_steps);
                                }
                            }
                            else
                            {
                                if ((!IsMouseEnterSchedule_1 && !IsMouseEnterSchedule_2) && (!IsPR1Preview && !IsPR2Preview))
                                {
                                    if (!(isLuminanceSupport == Visibility.Visible))
                                        BrightnessValue = Brightness_PR2 - (PerStepValue * Brightness_steps);
                                    else
                                        LuminanceValue = Luminance_PR2 - (PerStepValue * Brightness_steps);
                                }
                            }
                            if (!(isLuminanceSupport == Visibility.Visible))
                            {
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
                        }
                        else  //CurDateTime is same as or earlier than Pre_PR1Timee_ADD1D
                        {
                            if ((!IsMouseEnterSchedule_1 && !IsMouseEnterSchedule_2) && (!IsPR1Preview && !IsPR2Preview))
                            {
                                if (!(isLuminanceSupport == Visibility.Visible))
                                {
                                    BrightnessValue = Brightness_PR2;
                                    ContrastValue = Contrast_PR2;
                                }
                                else
                                    LuminanceValue = Luminance_PR2;
                            }
                        }
                    }
                    else if (DateTime.Compare(CurDateTime, PR2Time) == 0) //The same as PR2Time.
                    {
                        if ((!IsMouseEnterSchedule_1 && !IsMouseEnterSchedule_2) && (!IsPR1Preview && !IsPR2Preview))
                        {
                            if (!(isLuminanceSupport == Visibility.Visible))
                            {
                                BrightnessValue = Brightness_PR2;
                                ContrastValue = Contrast_PR2;
                            }
                            else
                                LuminanceValue = Luminance_PR2;
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
                                {
                                    if (!(isLuminanceSupport == Visibility.Visible))
                                        BrightnessValue = Brightness_PR1 + (PerStepValue * Brightness_steps);
                                    else
                                        LuminanceValue = Luminance_PR1 + (PerStepValue * Brightness_steps);
                                }
                            }
                            else
                            {
                                if ((!IsMouseEnterSchedule_1 && !IsMouseEnterSchedule_2) && (!IsPR1Preview && !IsPR2Preview))
                                {
                                    if (!(isLuminanceSupport == Visibility.Visible))
                                        BrightnessValue = Brightness_PR1 - (PerStepValue * Brightness_steps);
                                    else
                                        LuminanceValue = Luminance_PR1 - (PerStepValue * Brightness_steps);
                                }
                            }

                            if (!(isLuminanceSupport == Visibility.Visible))
                            {
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
                        }
                        else  //CurDateTime is same as  or  earlier than Pre_PR2Time
                        {
                            if ((!IsMouseEnterSchedule_1 && !IsMouseEnterSchedule_2) && (!IsPR1Preview && !IsPR2Preview))
                            {
                                if (!(isLuminanceSupport == Visibility.Visible))
                                {
                                    BrightnessValue = Brightness_PR1;
                                    ContrastValue = Contrast_PR1;
                                }
                                else
                                    LuminanceValue = Luminance_PR1;
                            }
                        }
                    }
                }
            }
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

        /// <summary>
        /// DDPMW-784 Check All Monitor ALS Status
        /// </summary>
        /// <returns>True is > 2 ALS monitor and some one monitor PrimaryMonitorSync on</returns>
        public bool CheckMonitorALSStatus()
        {
            DdpmCommonHelper.WriteUILog($"CheckMonitorALSStatus in ...");
            List<ALSConfig> alsList = new List<ALSConfig>();
            alsList = DdpmCommonHelper.DeviceManagerSA.GetAllExistAlsConfig().Result;
            int alsSupportCount = 0;
            int alsPrimaryMSOn = 0;
            bool result = false;
            for (int i = 0; i < alsList.Count; i++)
            {
                if (alsList[i].isSupportALS == 2)
                {
                    alsSupportCount++;
                    if (alsList[i].isPrimaryMonitorSync)
                    {
                        alsPrimaryMSOn++;
                    }
                }
            }
            if (alsSupportCount <= 1)//1 ALS monitor Not processed, 0 or 1 ALS monitor
            {
                result = false;
            }
            else
            {
                if (alsPrimaryMSOn == 0)// > 2 LS monitor, but PrimaryMonitorSync all off
                    result = false;
                else
                {
                    result = true;
                }
            }
            DdpmCommonHelper.WriteUILog($"CheckMonitorALSStatus out ...");
            return result;
        }

        public void CloseSchedule()
        {
            if (ScheduleMap == null)
            {
                ScheduleMap = new scheduleInfo()
                {
                    model = SelectedHomeDevice.MonitorInfo.modelName ?? string.Empty,
                    serviceTag = SelectedHomeDevice.MonitorInfo.edid.ServiceTag ?? string.Empty,
                    IsEnable = false,
                    Pre1Name = string.Empty,
                    Pre2Name = string.Empty,
                    Hours1 = 8,
                    Mins1 = 0,
                    Duration1 = 60,
                    Hours2 = 5,
                    Mins2 = 0,
                    Duration2 = 60,
                    Brightness1 = 75,
                    Contrast1 = 75,
                    Brightness2 = 50,
                    Contrast2 = 50,
                };
            }
            else
                ScheduleMap.IsEnable = false;

            DdpmCommonHelper.DeviceManagerSA?.WriteScheduleMonitorSettings(SelectedHomeDevice.MonitorInfo, ScheduleMap);
        }

        public void GetScheduleInfo()
        {
            if (SelectedHomeDevice == null)
                return;

            ScheduleMap = DdpmCommonHelper.DeviceManagerSA.ReadScheduleMonitorSettings(SelectedHomeDevice.MonitorInfo).Result;

            if (ScheduleMap != null)
            {
                PR1Name = ScheduleMap.Pre1Name;
                PR2Name = ScheduleMap.Pre2Name;
                hOurs1 = ScheduleMap.Hours1;
                mIns1 = ScheduleMap.Mins1;
                dUration1 = ScheduleMap.Duration1;
                hOurs2 = ScheduleMap.Hours2;
                mIns2 = ScheduleMap.Mins2;
                dUration2 = ScheduleMap.Duration2;
                PR1Brightness_Value = ScheduleMap.Brightness1;
                PR1Contrast_Value = ScheduleMap.Contrast1;
                PR2Brightness_Value = ScheduleMap.Brightness2;
                PR2Contrast_Value = ScheduleMap.Contrast2;
                PR1Luminance_Value = ScheduleMap.Brightness1;
                PR2Luminance_Value = ScheduleMap.Brightness2;
            }
            else
            {
                ScheduleMap = new scheduleInfo()
                {
                    model = SelectedHomeDevice.MonitorInfo.modelName ?? string.Empty,
                    serviceTag = SelectedHomeDevice.MonitorInfo.edid.ServiceTag ?? string.Empty,
                    IsEnable = true,
                    Pre1Name = string.Empty,
                    Pre2Name = string.Empty,
                    Hours1 = 8,
                    Mins1 = 0,
                    Duration1 = 60,
                    Hours2 = 5,
                    Mins2 = 0,
                    Duration2 = 60,
                    Brightness1 = 75,
                    Contrast1 = 75,
                    Brightness2 = 50,
                    Contrast2 = 50,
                };

                PR1Name = ScheduleMap.Pre1Name;
                PR2Name = ScheduleMap.Pre2Name;
                hOurs1 = ScheduleMap.Hours1;
                mIns1 = ScheduleMap.Mins1;
                dUration1 = ScheduleMap.Duration1;
                hOurs2 = ScheduleMap.Hours2;
                mIns2 = ScheduleMap.Mins2;
                dUration2 = ScheduleMap.Duration2;
                PR1Brightness_Value = ScheduleMap.Brightness1;
                PR1Contrast_Value = ScheduleMap.Contrast1;
                PR2Brightness_Value = ScheduleMap.Brightness2;
                PR2Contrast_Value = ScheduleMap.Contrast2;
                PR1Luminance_Value = ScheduleMap.Brightness1;
                PR2Luminance_Value = ScheduleMap.Brightness2;
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
            NotifyPropertyChanged("PR1LuminanceValue");
            NotifyPropertyChanged("PR2LuminanceValue");
        }

        public void Invoke_ColorPreset_Sync()
        {
            BackgroundWorker bw_ColorPreset_Sync = new BackgroundWorker()
            {
                WorkerReportsProgress = false,
                WorkerSupportsCancellation = false
            };
            bw_ColorPreset_Sync.DoWork += DoWork_ColorPreset_Sync;
            bw_ColorPreset_Sync.RunWorkerCompleted += RunWorkerCompleted_ColorPreset_Sync;
            //Log?.Info("RunWorkerCompleted_DownloadICCData start...");
            //IsBusy = true;
            bw_ColorPreset_Sync.RunWorkerAsync();
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
            NotifyPropertyChanged("IsBusy");
            DdpmCommonHelper.WriteUILog($"Invoke_RefreshBrightnessPage IsBusy : true ...");
        }

        public void Invoke_RefreshHotkeySettings()
        {
            BackgroundWorker bw = new BackgroundWorker()
            {
                WorkerReportsProgress = false,
                WorkerSupportsCancellation = false
            };
            bw.DoWork += DoWork_RefreshData;
            bw.RunWorkerCompleted += RunWorkerCompleted_RefreshData;
            bw.RunWorkerAsync();
            IsBusy = true;
            NotifyPropertyChanged("IsBusy");
            DdpmCommonHelper.WriteUILog($"Invoke_RefreshHotkeySettings IsBusy : true ...");
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
            IsBusy = true;
            NotifyPropertyChanged("IsBusy");
            DdpmCommonHelper.WriteUILog($"Invoke_RefreshManualValue IsBusy : true ...");
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
            IsBusy = true;
            NotifyPropertyChanged("IsBusy");
            DdpmCommonHelper.WriteUILog($"Invoke_RefreshScheduleValue IsBusy : true ...");
        }

        public bool isBCHotkeyNotSet()
        {
            //DDPMW-764
            string defaultStr = LangHelper.Instance["None"];
            bool ret = (defaultStr.Equals(_brightnessMinsKey.Trim(), StringComparison.OrdinalIgnoreCase) &&
                    defaultStr.Equals(_brightnessAddKey.Trim(), StringComparison.OrdinalIgnoreCase) &&
                    defaultStr.Equals(_contrastMinsKey.Trim(), StringComparison.OrdinalIgnoreCase) &&
                    defaultStr.Equals(_contrastAddKey.Trim(), StringComparison.OrdinalIgnoreCase));
            DdpmCommonHelper.WriteUILog($"[isBCHotkeyNotSet]hotkey config status:{ret};" +
                $"defaultStr=[{defaultStr}]," +
                $"_brightnessMinsKey=[{_brightnessMinsKey.Trim()}],_brightnessAddKey=[{_brightnessAddKey.Trim()}]" +
                $"_contrastMinsKey=[{_contrastMinsKey.Trim()}],_contrastAddKey=[{_contrastAddKey.Trim()}]");
            return ret;
        }

        public void Luminance_Sync()
        {
            BackgroundWorker bw = new BackgroundWorker()
            {
                WorkerReportsProgress = false,
                WorkerSupportsCancellation = false
            };
            bw.DoWork += LuminanceSync;
            bw.RunWorkerCompleted += LuminanceSync_finish;
            bw.RunWorkerAsync();
            IsBusy = true;
            NotifyPropertyChanged("IsBusy");
            DdpmCommonHelper.WriteUILog($"Luminance_Sync IsBusy : true ...");
        }

        public void ResetClick()
        {
            BackgroundWorker bw = new BackgroundWorker()
            {
                WorkerReportsProgress = false,
                WorkerSupportsCancellation = false
            };
            bw.DoWork += Reset_Click;
            bw.RunWorkerCompleted += Reset_Click_finish;
            bw.RunWorkerAsync();
            IsBusy = true;
            NotifyPropertyChanged("IsBusy");
            DdpmCommonHelper.WriteUILog($"ResetClick IsBusy : true ...");
        }

        public void SaveHotkeySettings(MonitorInfo monitorInfo, HotkeyInfo hotkeyInfo)
        {
            if (DdpmCommonHelper.DeviceManagerSA != null)
            {
                IsBusy = true;
                NotifyPropertyChanged("IsBusy");
                DdpmCommonHelper.WriteUILog($"SaveHotkeySettings IsBusy : true ...");
                bool saveSettings = DdpmCommonHelper.DeviceManagerSA.SaveHotkeySetting(monitorInfo, hotkeyInfo).Result;
                if (saveSettings)
                {
                    DdpmCommonHelper.isHotkeyBypass = DdpmCommonHelper.DeviceManagerSA.ByPassHotkey(false).Result;
                    IsBusy = false;
                    NotifyPropertyChanged("IsBusy");
                    DdpmCommonHelper.WriteUILog($"SaveHotkeySettings IsBusy : false ...");
                }
                else
                {
                    DdpmCommonHelper.WriteUILog($"[DisplayHotkey] monitor:{monitorInfo.AliasDeviceName}:{monitorInfo.edid.ServiceTag},({hotkeyInfo.Job}) SaveHotkeySetting fail.");
                }
            }
            Invoke_RefreshHotkeySettings();
        }

        /// <summary>
        /// Set VCP command to monitor
        /// </summary>
        /// <param name="alsConfig">Monitor Info</param>
        /// <param name="type">ALS Feature Query Type</param>
        /// <param name="val">empty</param>
        /// <returns>Sucess or Fail</returns>
        public bool SetALSAll(ALSConfig alsConfig, ALSFeatureQueryType type, int val)
        {
            DdpmCommonHelper.WriteUILog($"SetALSAll in ...");
            return DdpmCommonHelper.DeviceManagerSA.SetALSFeatureValue(SelectedHomeDevice.MonitorInfo, Start_ALSConfig, ALSFeatureQueryType.All, "").Result;
        }

        public void SetIsBusy(bool busy)
        {
            List<ALSConfig> tmp = DdpmCommonHelper.DeviceManagerSA.GetAllExistAlsConfig().Result;
            if (tmp == null || tmp.Count <= 1)
                return;
            IsBusyALS = true;
            NotifyPropertyChanged("IsBusyALS");
            DdpmCommonHelper.WriteUILog($"SetIsBusy IsBusyALS : true ...");
            IsBusyALS = !AreAllConfigsNotBusy(tmp);
            if (!IsBusyALS)
            {
                NotifyPropertyChanged("IsBusyALS");
                DdpmCommonHelper.WriteUILog($"SetIsBusy IsBusyALS : false ...");
            }
            DdpmCommonHelper.WriteUILog($"[SetIsBusy][BrightnessViewModel] ModelName = {SelectedHomeDevice.MonitorInfo.modelName}, IsBusyALS = {IsBusyALS.ToString()}");
        }

        public void StartScheduleManger(int millisecond)
        {
            if (DdpmCommonHelper.DeviceManagerSA != null)
                DdpmCommonHelper.DeviceManagerSA.StartSchedulerManger(millisecond);
        }

        public void StopScheduleManger()
        {
            if (DdpmCommonHelper.DeviceManagerSA != null)
                DdpmCommonHelper.DeviceManagerSA.StopSchedulerManger();
        }

        ///<summary>
        ///Expected Result A: "Synchronize between monitors" is displayed and not greyed out with default is OFF.
        ///Expected Result B: "Synchronize between monitors" is displayed and not greyed out with default is OFF.
        ///Expected Result C: "Synchronize between monitors" is displayed and default is OFF.
        ///Expected Result D: "Synchronize between monitors" is displayed but greyed out.
        ///Expected Result E:  "Synchronize between monitors" is NOT displayed.
        ///</summary>
        public void SynchronizeBtnExpectedResult(string str)
        {
            switch (str)
            {
                case "A":
                case "B":
                case "C":
                    IsSynchronizeDisabled = false;
                    isShowSynchronize = Visibility.Visible;
                    DdpmCommonHelper.WriteUILog($"[BrightnessViewModel] Synchronize between monitors is displayed and not greyed out with default is OFF");
                    break;

                case "D":
                    IsSynchronizeDisabled = true;
                    isShowSynchronize = Visibility.Visible;
                    DdpmCommonHelper.WriteUILog($"[BrightnessViewModel] Synchronize between monitors is displayed but greyed out");
                    break;

                case "E":
                    isShowSynchronize = Visibility.Collapsed;
                    DdpmCommonHelper.WriteUILog($"[BrightnessViewModel] Synchronize between monitors is NOT displayed");
                    break;

                default:
                    isShowSynchronize = Visibility.Visible;
                    DdpmCommonHelper.WriteUILog($"[BrightnessViewModel] Synchronize between monitors is displayed and not greyed out with default is OFF");
                    break;
            }
            ;
            NotifyPropertyChanged("isShowSynchronize");
            NotifyPropertyChanged("IsSynchronizeDisabled");
        }

        //"Preview Changes";
        public void UpdataScheduleBoaderUI()
        {
            NotifyPropertyChanged(nameof(IsMouseEnterSchedule_1));
            NotifyPropertyChanged(nameof(IsMouseEnterSchedule_2));
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

        public void Update_AutoBrightnessRangeLevelStatus(AutoBrightnessRangeLevel value)
        {
            DdpmCommonHelper.WriteUILog($"Update_AutoBrightnessRangeLevelStatus in ...");
            if (value == null)
            {
                DdpmCommonHelper.WriteUILog("Update_AutoBrightnessRangeLevelStatus: value is null or empty.");
                return;
            }

            if (_autoBrightnessRangeLevel == null || _autoBrightnessRangeLevel.level_value != value.level_value)
            {
                DdpmCommonHelper.WriteUILog($"Update_AutoBrightnessRangeLevelStatus {value.level_value} ...");
                _autoBrightnessRangeLevel = value;
                _autoBrightnessRangeLevel.level_value = value.level_value;
                if (value.level_value == 0)
                    _autoBrightnessRangeLevel.level_name = LangHelper.Instance["AutoBrightnessRangeLevel.0"]; //"Low";
                else if (value.level_value == 1)
                    _autoBrightnessRangeLevel.level_name = LangHelper.Instance["AutoBrightnessRangeLevel.1"]; //"Mid";
                else
                    _autoBrightnessRangeLevel.level_name = LangHelper.Instance["AutoBrightnessRangeLevel.2"]; //"High";
                NotifyPropertyChanged("AutoBrightnessRangeLevel_SelectedIndex");
                NotifyPropertyChanged("AutoBrightnessRangeLevel_String");
            }
            DdpmCommonHelper.WriteUILog($"Update_AutoBrightnessRangeLevelStatus out ...");
        }

        /// <summary>
        /// Auto Brightness Binding data
        /// </summary>
        public void Update_AutoBrightnessStatus(bool value)
        {
            DdpmCommonHelper.WriteUILog($"Update_AutoBrightnessStatus {value.ToString()} in ...");
            _autoBrightnessStatus = value;
            NotifyPropertyChanged("AutoBrightnessStatus");
            NotifyPropertyChanged("AutoBrightness_String");
            NotifyPropertyChanged("AutoBrightnessRangeLevelVisible");
            NotifyPropertyChanged("AutoBrightnessRangeLevelVisible_invert");
            DdpmCommonHelper.WriteUILog($"Update_AutoBrightnessStatus out ...");
        }

        /// <summary>
        /// Auto Color Temp Binding data
        /// </summary>
        public void Update_AutoColorTempStatus(bool value)
        {
            DdpmCommonHelper.WriteUILog($"Update_AutoColorTempStatus {value.ToString()} in ...");
            _autoColorTempStatus = value;
            NotifyPropertyChanged("AutoColorTempStatus");
            NotifyPropertyChanged("AutoColorTemp_String");
            DdpmCommonHelper.WriteUILog($"Update_AutoColorTempStatus out ...");
        }

        public void Update_BriContLockStatus(bool value)
        {
            LockMaskVisible = value ? Visibility.Visible : Visibility.Collapsed;
            TabSTOP = value ? "None" : "Cycle";
        }

        /// <summary>
        /// Update PrimaryMonitorSyncStatus element
        /// </summary>
        /// <param name="value"></param>
        public void Update_PrimaryMonitorSyncStatus(bool value)
        {
            DdpmCommonHelper.WriteUILog($"Update_PrimaryMonitorSyncStatus {value.ToString()} in ...");
            _primaryMonitorSyncStatus = value;
            NotifyPropertyChanged("PrimaryMonitorSyncStatus");
            NotifyPropertyChanged("PrimaryMonitorSync_String");
            DdpmCommonHelper.WriteUILog($"Update_PrimaryMonitorSyncStatus out ...");
        }

        public void Update_SupportedPrimaryMonitorSync(bool isAutoBrightness, bool isAutoColorTemp)
        {
            DdpmCommonHelper.WriteUILog($"Update_SupportedPrimaryMonitorSync AutoBrightness:{isAutoBrightness.ToString()},  AutoColorTemp:{isAutoColorTemp.ToString()}, in ...");
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
            DdpmCommonHelper.WriteUILog($"Update_SupportedPrimaryMonitorSync out ...");
        }

        public void Update_SyncLockStatus(bool value)
        {
            synchronizeLock = value ? Visibility.Visible : Visibility.Collapsed;
        }

        public void UpdateBrightnessContrast()
        {
            if (SelectedHomeDevice != null)
            {
                var commands = new List<MultiCommandArch>();
                commands.Add(new MultiCommandArch()
                {
                    MonitorInfo = SelectedHomeDevice.MonitorInfo,
                    Action = MultiCommandAction.GetVCPCapability,
                    Function = 0x10,
                    Priority = Priority.SuperHigh,
                    Opt = 0,
                });

                commands.Add(new MultiCommandArch()
                {
                    MonitorInfo = SelectedHomeDevice.MonitorInfo,
                    Action = MultiCommandAction.GetVCPCapability,
                    Function = 0x12,
                    Priority = Priority.SuperHigh,
                    Opt = 0,
                });

                var BriConValue = DdpmCommonHelper.DeviceManagerSA.MultiCommandsRun(commands).Result;

                foreach (var item in BriConValue)
                {
                    if (item.Function is not null)
                    {
                        if (Convert.ToByte(item.Function).Equals(0x10))
                        {
                            if (!string.IsNullOrWhiteSpace(item.Result?.ToString()))
                            {
                                var objValue = Convert.ToUInt32(item.Result);

                                Brightness_Value = Convert.ToDouble((uint)(long)objValue);
                                Luminance_Value = Brightness_Value;
                                
                                NotifyPropertyChanged("BrightnessValue");
                                NotifyPropertyChanged("LuminanceValue");
                                NotifyPropertyChanged("AutoBrightnessRangeLevel_String");
                            }
                        }
                        else if (Convert.ToByte(item.Function).Equals(0x12))
                        {
                            if (!string.IsNullOrWhiteSpace(item.Result?.ToString()))
                            {
                                var objValue = Convert.ToUInt32(item.Result);
                                Contrast_Value = Convert.ToDouble((uint)(long)objValue);

                                NotifyPropertyChanged("ContrastValue");
                            }
                        }
                    }
                }

                //if (Brightness_Value < 0 || Contrast_Value < 0)
                //Get_Brightness_Value();

                ////NotifyPropertyChanged("BrightnessValue");

                //if (Contrast_Value < 0)
                //Get_Contrast_Value();

                ////NotifyPropertyChanged("ContrastValue");
            }
        }

        public void UpdateHDRStatus()
        {
            bool HDRStatus = DdpmCommonHelper.DeviceManagerSA.GetHDRStatus(MyModule.SelectedHomeDevice.MonitorInfo).Result;
            IsBrightnessEnable = !HDRStatus;
            NotifyPropertyChanged(nameof(BrightnessEnable));
        }

        //brightness/contrast/luminance hotkey is setup or not
        public void updateHotkeyBtn()
        {
            bool autoBrightnessStatus = AutoBrightnessStatus;
            bool hotkeyStatus = isBCHotkeyNotSet();
            if (autoBrightnessStatus && hotkeyStatus)
            {
                //disable hotkey btn
                //btnManualBrightnessContrast.IsEnabled = false;
                ManualBCHotkeyBtn = false;
            }
            else
            {
                //btnManualBrightnessContrast.IsEnabled = true;
                ManualBCHotkeyBtn = true;
            }
            NotifyPropertyChanged("ManualBCHotkeyBtn");
        }

        public void UpdateLuminance()
        {
            //if (Luminance_Value < 0)
            Get_Luminance_Value(); //0703 fix update luminance issue
                                   //NotifyPropertyChanged("LuminanceValue");
                                   //if (LuminanceMax_Value < 0)
            Get_LuminanceMax_Value();
            //NotifyPropertyChanged("LuminanceMaxValue");
        }

        public void UpdateScheduleInfo()
        {
            if (PR1Luminance_Value < 0 || PR2Luminance_Value < 0 || PR1Brightness_Value < 0 || PR2Brightness_Value < 0 || PR1Contrast_Value < 0 || PR2Contrast_Value < 0 || hOurs_1 < 0 || hOurs_2 < 0 || mIns_1 < 0 || mIns_2 < 0 || dUration_1 < 0 || dUration_2 < 0)
                GetScheduleInfo();
        }

        private void ALSFontColorUpdate(OSThemeEnum oSThemeEnum)
        {
            if (DdpmCommonHelper.PreviousOsTheme == OSThemeEnum.Dark)
            {
                IsDarkTheme = true;
            }
            else
            {
                IsDarkTheme = false;
            }
        }

        /// <summary>
        /// Detect the status of the PrimaryMonitorSync and obtain the current number of monitors that support the ALS function.
        /// </summary>
        /// <param name="onoff">UI PrimaryMonitorSync status</param>
        private void ALSSettingsChangesOnNonPrimary(ref bool onoff, string property = "AUTOBRI", int level = 0)
        {
            DdpmCommonHelper.WriteUILog($"ALSSettingsChangesOnNonPrimary in ...");
            int level_keep = 0;
            if (Start_ALSConfig.AutoBrightnessRangeLevel != null && Start_ALSConfig.AutoBrightnessRangeLevel != null)
                level_keep = (int)Start_ALSConfig.AutoBrightnessRangeLevel.level_value;

            if (!Start_ALSConfig.isPrimaryMonitorSync && CheckMonitorALSStatus())// user change non-Primary
            {
                //PIMS-328260
                //string pop_string = Strings.BrightnessPageNotice1;//"This is not your primary monitor. Do you want to proceed with the change and set this as primary Monitor for Sync?";
                MessageModalDialog messageModalDialog;
                Window mainWindow = System.Windows.Application.Current.MainWindow;
                messageModalDialog = new(Caption: LangHelper.Instance["Warning"], Message: LangHelper.Instance["Brightness.20"], Button1Caption: LangHelper.Instance["PairedInfo.6"], Button2Caption: LangHelper.Instance["Common.2"], btnLogicToggle: true);
                if (mainWindow != null)
                {
                    messageModalDialog.Owner = mainWindow;
                    messageModalDialog.Left = mainWindow.Left + (mainWindow!.ActualWidth - 417) / 2;
                    messageModalDialog.Top = mainWindow.Top + 300;
                }

                //PIMS-353731 change button text from "Yes"/"No" to "Continue"/"Cancel"
                //if (DdpmCommonHelper.DDPMMesssageBox(Strings.BrightnessPageWarning, pop_string, MyModule.GetRightView().Parent))
                if (messageModalDialog != null && messageModalDialog.ShowDialog().Value == true)//true means left button is "continue"
                {
                    onoff = true;
                    _primaryMonitorSyncStatus = true;
                    Start_ALSConfig.isPrimaryMonitorSync = true;// onoff; //PIMS-328260
                    if (property.Equals("AUTOBRILEVEL"))//PIMS-314583
                    {
                        SetBrightnessLevelDataToObject(level);
                    }
                    if (property.Equals("BRILEVEL"))
                    {
                        _autoBrightnessStatus = false;
                        Start_ALSConfig.isAutoBrightness = false;
                        NotifyPropertyChanged("AutoBrightnessStatus");
                        NotifyPropertyChanged("AutoBrightness_String");
                        NotifyPropertyChanged("AutoBrightnessRangeLevelVisible");
                        NotifyPropertyChanged("AutoBrightnessRangeLevelVisible_invert");
                    }
                    SetALSAll(Start_ALSConfig, ALSFeatureQueryType.All, 0);
                    NotifyPropertyChanged("PrimaryMonitorSyncStatus");
                    NotifyPropertyChanged("PrimaryMonitorSync_String");
                }
                else
                {
                    //SetALSAll(Start_ALSConfig, ALSFeatureQueryType.All, 0);
                    //onoff = !onoff;
                    //Update_AutoBrightnessStatus(onoff);
                    if (property.Equals("AUTOBRI"))
                    {
                        onoff = !onoff;
                        _autoBrightnessStatus = onoff;
                        Start_ALSConfig.isAutoBrightness = onoff;
                    }
                    if (property.Equals("AUTOCOLOR"))
                    {
                        onoff = !onoff;
                        _autoColorTempStatus = onoff;
                        Start_ALSConfig.isAutoColorTemp = onoff;
                    }
                    if (property.Equals("AUTOBRILEVEL"))
                    {
                        onoff = !onoff;
                        //click no, switch back due to UI action
                        SetBrightnessLevelDataToObject(level_keep);
                    }
                    if(property.Equals("BRILEVEL"))
                    {
                        onoff = false;
                    }
                }
            }
            else
            {
                if (property.Equals("PRISYNC"))
                {
                    _primaryMonitorSyncStatus = onoff;
                    Start_ALSConfig.isPrimaryMonitorSync = onoff;
                }
                if (property.Equals("AUTOBRILEVEL") && //PIMS-314583
                    Start_ALSConfig.AutoBrightnessRangeLevel != null)
                {
                    SetBrightnessLevelDataToObject(level);
                }
                SetALSAll(Start_ALSConfig, ALSFeatureQueryType.All, 0);
            }
            DdpmCommonHelper.WriteUILog($"ALSSettingsChangesOnNonPrimary out ...");
        }

        private void BRConSync(object sender, DoWorkEventArgs e)
        {
            Set_Brightness_Value(BrightnessValue);
            Set_Contrast_Value(ContrastValue);
        }

        private void BRConSync_finish(object sender, RunWorkerCompletedEventArgs e)
        {
            IsBusy = false;
            NotifyPropertyChanged("BrightnessValue");
            NotifyPropertyChanged("LuminanceValue");
            NotifyPropertyChanged("ContrastValue");
            NotifyPropertyChanged("IsBusy");
            NotifyPropertyChanged("AutoBrightnessRangeLevel_String");
            DdpmCommonHelper.WriteUILog($"BRConSync_finish IsBusy : false ...");
        }

        private void DoWork_ColorPreset_Sync(object sender, DoWorkEventArgs e)
        {
            try
            {
                DdpmCommonHelper.DeviceManagerSA.ReadColorPreset(DdpmCommonHelper.ModuleOwner?.SelectedHomeDevice?.MonitorInfo);
                string curPreset = DdpmCommonHelper.DeviceManagerSA?.ReadCurrentColorPreset(DdpmCommonHelper.ModuleOwner?.SelectedHomeDevice?.MonitorInfo).Result;
                string strSync_CurrentColorPreset = string.Empty;
                strSync_CurrentColorPreset = DdpmCommonHelper.DeviceManagerSA?.Sync_ColorPresetName(DdpmCommonHelper.ModuleOwner?.SelectedHomeDevice?.MonitorInfo, curPreset).Result;

                // jim 20241207 add  for The DDPM color profile can not be applied by DDPM on Smart HDR mode.(Gaming monitor ex: AW2724DM)
                bool Is_Game_DeviceName = false;
                bool HDRStatus = DdpmCommonHelper.DeviceManagerSA.GetHDRStatus(MyModule.SelectedHomeDevice.MonitorInfo).Result;

                if (MyModule.SelectedHomeDevice.MonitorInfo.CapabilityDic.ContainsKey("F4"))
                    Is_Game_DeviceName = true;

                DdpmCommonHelper.WriteUILog($"[DoWork_ColorPreset_Sync] Has Gaming Capability={Is_Game_DeviceName}, HDR Status = {HDRStatus}");

                Task.Run(() =>
                {
                    foreach (HomeDevice hd in DdpmCommonHelper.ModuleOwner.HomeDevices)
                    {
                        if (hd.MonitorInfo.IsDellMonitor)
                            DdpmCommonHelper.DeviceManagerSA?.WriteColorPreset(hd.MonitorInfo, strSync_CurrentColorPreset, 0, Is_Game_DeviceName, HDRStatus, null, false); // jim 20241207  modify for The DDPM color profile can not be applied by DDPM on Smart HDR mode.(Gaming monitor ex: AW2724DM)
                    }
                });

                /*
                Dispatcher.Invoke(new Action(() =>
                {
                }));
                */
            }
            catch (System.Exception ex)
            {
                DdpmCommonHelper.WriteUILog($"[DoWork_ColorPreset_Sync] Catch exception[{ex.Message}]");
            }
        }

        private void DoWork_RefreshBrightnessPage(object sender, DoWorkEventArgs e)
        {
            InitComponentData();
        }

        private void DoWork_RefreshData(object sender, DoWorkEventArgs e)
        {
            if (DdpmCommonHelper.DeviceManagerSA != null)
            {
                var temp = DdpmCommonHelper.DeviceManagerSA.ReadCurrentHotkey(this.SelectedHomeDevice?.MonitorInfo).Result;
                HotkeySettings curHotkey = temp.Item1;
                string swHortcutText = string.Empty;

                if (curHotkey != null && curHotkey.HotkeyInfo.Count > 0)
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
        }

        public void DoWork_RefreshManualValue(object sender, DoWorkEventArgs e)
        {
            if (isLuminanceSupport == Visibility.Visible)
                UpdateLuminance();
            else
                UpdateBrightnessContrast();

            UpdateHDRStatus();
        }

        private void DoWork_RefreshScheduleValue(object sender, DoWorkEventArgs e)
        {
            MyModule.GetRightView().Dispatcher.Invoke((Action)(() =>
            {
                RefreshUI();
                IsBusy = false;
                NotifyPropertyChanged("IsBusy");
                DdpmCommonHelper.WriteUILog($"DoWork_RefreshScheduleValue IsBusy : false ...");
            }));

            UpdateScheduleInfo();
        }

        private double Get_Brightness_Value()
        {
            if (SelectedHomeDevice == null)
                return 0;

            ObjGetVCP obj = DdpmCommonHelper.DeviceManagerSA.GetVCPCapability(SelectedHomeDevice.MonitorInfo, 0x10, opt: 0).Result;
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
            NotifyPropertyChanged("AutoBrightnessRangeLevel_String");
            return Brightness_Value;
        }

        private double Get_Contrast_Value()
        {
            if (SelectedHomeDevice == null)
                return 0;

            ObjGetVCP obj = DdpmCommonHelper.DeviceManagerSA.GetVCPCapability(SelectedHomeDevice.MonitorInfo, 0x12, opt: 0).Result;
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

        private double Get_Luminance_Value()
        {
            if (SelectedHomeDevice == null)
                return 0;

            ObjGetVCP obj = DdpmCommonHelper.DeviceManagerSA.GetVCPCapability(SelectedHomeDevice.MonitorInfo, 0x10, opt: 0).Result;
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

            ObjGetVCP obj = DdpmCommonHelper.DeviceManagerSA.GetVCPCapability(SelectedHomeDevice.MonitorInfo, 0x10, opt: 1).Result;
            if (obj.result)
                LuminanceMax_Value = Convert.ToDouble((uint)(long)obj.value);

            NotifyPropertyChanged("LuminanceMaxValue");
            return LuminanceMax_Value;
        }

        private bool Get_Synchronize()
        {
            if (DdpmCommonHelper.Settings_Cache == null)
                DdpmCommonHelper.Settings_Cache = DdpmCommonHelper.ReadDDPMSettings();//DeviceManagerSA.ReloadAppConfigData().Result;
            if (DdpmCommonHelper.Settings_Cache != null)
            {
                IsSynchronizeMonitor = DdpmCommonHelper.Settings_Cache.UserSettings.IsSynchronizemonitor;
                IsSynchronizeMonitor_Scheduled = DdpmCommonHelper.Settings_Cache.UserSettings.IsSynchronizemonitor_Scheduled;

                IsGetSynchronizeMonitor = true;
            }

            //return IsSynchronizeMonitor;
            return IsGetSynchronizeMonitor;
        }

        private void GetALSContentAndSyncUI(MonitorInfo mo)
        {
            DdpmCommonHelper.WriteUILog($"GetALSContentAndSyncUI ... in");
            try
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
                        DdpmCommonHelper.WriteUILog($"GetALSContentAndSyncUI {mo.modelName} No support ALS ...");
                    }
                    else
                    {
                        DdpmCommonHelper.WriteUILog($"GetALSContentAndSyncUI before Update ...");
                        isAlsSupported = Visibility.Visible;
                        Update_AutoBrightnessStatus(Start_ALSConfig.isAutoBrightness);
                        Update_AutoColorTempStatus(Start_ALSConfig.isAutoColorTemp);
                        Update_PrimaryMonitorSyncStatus(Start_ALSConfig.isPrimaryMonitorSync);
                        Update_AutoBrightnessRangeLevelStatus(Start_ALSConfig.AutoBrightnessRangeLevel);
                        Update_SupportedPrimaryMonitorSync(Start_ALSConfig.isAutoBrightness, Start_ALSConfig.isAutoColorTemp);
                        DdpmCommonHelper.WriteUILog($"GetALSContentAndSyncUI after Update ...");
                    }
                    NotifyPropertyChanged("isAlsSupported");
                    NotifyPropertyChanged("IsScheduledShow");
                    NotifyPropertyChanged("IsScheduledLuminanceShow");
                    DdpmCommonHelper.WriteUILog($"******************** GetALSContentAndSyncUI ********************");
                    DdpmCommonHelper.WriteUILog($"ModelName ****************** : {Start_ALSConfig.MoInfo.modelName}");
                    DdpmCommonHelper.WriteUILog($"SupportALS ***************** : {Start_ALSConfig.isSupportALS.ToString()}");
                    DdpmCommonHelper.WriteUILog($"AutoBrightness ************* : {(Start_ALSConfig.isAutoBrightness ? "ON" : "OFF")}");
                    DdpmCommonHelper.WriteUILog($"AutoColorTemp ************** : {(Start_ALSConfig.isAutoColorTemp ? "ON" : "OFF")}");
                    if (Start_ALSConfig.AutoBrightnessRangeLevel != null && Start_ALSConfig.AutoBrightnessRangeLevel != null)
                    {
                        DdpmCommonHelper.WriteUILog($"AutoBrightnessRangeLevel *** : {Start_ALSConfig.AutoBrightnessRangeLevel.level_value.ToString()}");
                    }
                    else
                    {
                        DdpmCommonHelper.WriteUILog("AutoBrightnessRangeLevel is empty or null.");
                    }
                    //DdpmCommonHelper.WriteUILog($"AutoBrightnessRangeLevel *** : {Start_ALSConfig.AutoBrightnessRangeLevel[0].level_name}");
                    DdpmCommonHelper.WriteUILog($"PrimaryMonitor ************* : {(Start_ALSConfig.isPrimaryMonitorSync ? "ON" : "OFF")}");
                    DdpmCommonHelper.WriteUILog($"ALS Value ****************** : {Start_ALSConfig.AllValue.ToString()}");
                    DdpmCommonHelper.WriteUILog($"******************** GetALSContentAndSyncUI ********************");
                }
                DdpmCommonHelper.WriteUILog($"GetALSContentAndSyncUI ... out");
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog($"GetALSContentAndSyncUI exception with {ex.Message}");
            }
        }

        private void InitComponentData()
        {
            DdpmCommonHelper.WriteUILog($"InitComponentData in ...");
            Trace.WriteLine($"1. {DateTime.Now.ToString("MM/dd/yyyy hh:mm ss fff")}");
            if (SelectedHomeDevice == null)
            {
                IsBusy = false;
                NotifyPropertyChanged("IsBusy");
                DdpmCommonHelper.WriteUILog($"InitComponentData IsBusy : false ...");
                return;
            }

            Trace.WriteLine($"2. {DateTime.Now.ToString("MM/dd/yyyy hh:mm ss fff")}");
            if (DdpmCommonHelper.DeviceManagerSA != null)
            {
                if (DdpmCommonHelper.ModuleOwner != null)
                    isShowSynchronize = DdpmCommonHelper.ModuleOwner.HomeDevices.Count > 1 ? Visibility.Visible : Visibility.Collapsed;
                else
                {
                    isShowSynchronize = Visibility.Collapsed;
                    DdpmCommonHelper.WriteUILog($"InitComponentData DdpmCommonHelper.DeviceManagerSA = null ...");
                }
                NotifyPropertyChanged("isShowSynchronize");

                Trace.WriteLine($"3. {DateTime.Now.ToString("MM/dd/yyyy hh:mm ss fff")}");
                isLuminanceSupport = Visibility.Collapsed;

                List<string> strings = new List<string>();
                isLuminance = false;
                if (!SelectedHomeDevice.MonitorInfo.CapabilityDic.ContainsKey("12"))
                {
                    isLuminanceSupport = Visibility.Visible;
                    isLuminance = true;
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
                //if (isLuminanceSupport != Visibility.Visible)
                //{
                Get_Synchronize();
                //}
                Trace.WriteLine($"6. {DateTime.Now.ToString("MM/dd/yyyy hh:mm ss fff")}");
                //Check if support ALS
                if ((SelectedHomeDevice.MonitorInfo.CapabilityDic.ContainsKey("66") || isLuminance == true) &&
                      SelectedHomeDevice != null &&
                      SelectedHomeDevice.MonitorInfo != null)
                {
                    //fixed releate PIMS-287891
                    List<ALSConfig> alsList = new List<ALSConfig>();
                    alsList = DdpmCommonHelper.DeviceManagerSA.GetAllExistAlsConfig().Result;
                    DdpmCommonHelper.WriteUILog($"InitComponentData after GetAllExistAlsConfig ...");
                    if (alsList.Count > 0) // Check Start_ALSConfig whether exist
                    {
                        DdpmCommonHelper.WriteUILog($"InitComponentData GetAllExistAlsConfig ALS Count : {alsList.Count.ToString()} ...");
                        for (int i = 0; i < alsList.Count; i++)
                        {
                            if (alsList[i].Edid.Equals(SelectedHomeDevice.MonitorInfo.edid))
                            {
                                Start_ALSConfig = alsList[i];
                                DdpmCommonHelper.WriteUILog($"InitComponentData Start_ALSConfig, ModelName = {Start_ALSConfig?.ModelName}, ALS Value = {Start_ALSConfig?.AllValue.ToString()}");
                            }
                        }
                    }
                    //Re-Get Start_ALSConfig
                    if (Start_ALSConfig.AllValue == 0 && isLuminance == false)//Need to Re-Get value
                    {
                        Start_ALSConfig = DdpmCommonHelper.DeviceManagerSA.GetALSFeatureValue(SelectedHomeDevice.MonitorInfo, ALSFeatureQueryType.All, 0).Result;
                        if (!alsList.Any(als => als.Edid.Equals(Start_ALSConfig.Edid)))
                        {
                            alsList.Add(Start_ALSConfig);
                            DdpmCommonHelper.WriteUILog($"InitComponentData Re-Get Start_ALSConfig, ModelName = {Start_ALSConfig?.ModelName}, ALS Value = {Start_ALSConfig?.AllValue.ToString()}");
                        }
                    }
                    GetALSContentAndSyncUI(SelectedHomeDevice.MonitorInfo);
                    SynchronizeBtnExpectedResult(DdpmCommonHelper.DeviceManagerSA.CheckisShowSynchronize(SelectedHomeDevice.MonitorInfo, alsList).Result);
                    //CheckisShowSynchronize(alsList);
                }

                //OSD control back event
                DdpmCommonHelper.DeviceManagerSA.VCPchanged += OnVCPChangedEvent;
                Trace.WriteLine($"7 {DateTime.Now.ToString("MM/dd/yyyy hh:mm ss fff")}");

                //Lock/unlock mask and tabstop init here
                DDPMSettings data = DdpmCommonHelper.ReadDDPMSettings();// DeviceManagerSA.ReloadAppConfigData().Result;//Be careful if spend much time here
                DdpmCommonHelper.WriteUILog($"InitComponentData after ReadDDPMSettings ...");
                Update_ALSLockStatus(data.LockSettings.Lock_Display_AutoBriTemp);
                Update_BriContLockStatus(data.LockSettings.Lock_Display_BriCont);
                Update_SyncLockStatus((data.LockSettings.Lock_Display_BriCont || data.LockSettings.Lock_Display_ColorPreset || data.LockSettings.Lock_Display_AutoBriTemp));

                SetDefaultExpanded(Start_ALSConfig, isLuminance);
                //ex: vm.LockMaskVisible = data.LockSettings.Lock_Display_BriCont ? Visibility.Visible : Visibility.Collapsed;
                //Read user default lock value, these values are synced from IT lock event
                Trace.WriteLine($"[SettingsPage] Apply Brightness/Contrast(Lock) : {data.LockSettings.Lock_Display_BriCont}");
                Trace.WriteLine($"[SettingsPage] Apply Auto Brightness(Lock) : {data.LockSettings.Lock_Display_AutoBriTemp}");
                Trace.WriteLine($"[SettingsPage] Apply Synchroniz Button(Lock) : {(data.LockSettings.Lock_Display_BriCont || data.LockSettings.Lock_Display_ColorPreset || data.LockSettings.Lock_Display_AutoBriTemp)}");
            }
            DdpmCommonHelper.WriteUILog($"InitComponentData out ...");
        }

        private void LuminanceSync(object sender, DoWorkEventArgs e)
        {
            Set_Luminance_Value(LuminanceValue);
        }

        private void LuminanceSync_finish(object sender, RunWorkerCompletedEventArgs e)
        {
            IsBusy = false;
            NotifyPropertyChanged("BrightnessValue");
            NotifyPropertyChanged("LuminanceValue");
            NotifyPropertyChanged("ContrastValue");
            NotifyPropertyChanged("IsBusy");
            NotifyPropertyChanged("AutoBrightnessRangeLevel_String");
            DdpmCommonHelper.WriteUILog($"LuminanceSync_finish IsBusy : false ...");
        }

        private void NotifyPropertyChanged(string info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        private void OnHDRChangedEvent(object? sender, EventManagerArgs e)
        {
            IsBrightnessEnable = !(bool)e.Tag;
            NotifyPropertyChanged(nameof(BrightnessEnable));
        }

        /// <summary>
        /// Catch OSD menu event
        /// </summary>
        /// <param name="sender">object type</param>
        /// <param name="e">changed event</param>
        private void OnVCPChangedEvent(object? sender, VCPchangedEventArgs e)
        {
            DdpmCommonHelper.WriteUILog($"OnVCPChangedEvent {e.vcpcode}, {e.value} ... in");
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

                try
                {
                    List<ALSConfig> tmp = DdpmCommonHelper.DeviceManagerSA.GetAllExistAlsConfig().Result;
                    if (tmp == null || tmp.Count == 0)
                        return;

                    int idx = tmp.FindIndex(x => x.Edid.Equals(mo.edid));
                    if (idx < 0)
                        return;

                    Start_ALSConfig = tmp[idx];

                    IsBusyALS = true;
                    NotifyPropertyChanged("IsBusyALS");
                    DdpmCommonHelper.WriteUILog($"OnVCPChangedEvent IsBusyALS : true ...");
                    //2.if yes, then update the vcp value to each option
                    GetALSContentAndSyncUI(SelectedHomeDevice.MonitorInfo);
                    IsBusyALS = !AreAllConfigsNotBusy(tmp);
                    if (!IsBusyALS)
                    {
                        NotifyPropertyChanged("IsBusyALS");
                        DdpmCommonHelper.WriteUILog($"OnVCPChangedEvent IsBusyALS : false ...");
                    }
                    DdpmCommonHelper.WriteUILog($"[OnVCPChangedEvent][BrightnessViewModel] ModelName = {SelectedHomeDevice.MonitorInfo.modelName}, IsBusyALS = {IsBusyALS.ToString()}");
                }
                catch (Exception ex)
                {
                    DdpmCommonHelper.WriteUILog($"[OnVCPChangedEvent][BrightnessViewModel] exception with {ex.Message}");
                }
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
                DdpmCommonHelper.WriteUILog($"[BrightnessVM]OnVCPChangedEvent,update Luminance_Value = {Luminance_Value}, Brightness_Value={Brightness_Value}");
                NotifyPropertyChanged("LuminanceValue");
                NotifyPropertyChanged("BrightnessValue");
                NotifyPropertyChanged("AutoBrightnessRangeLevel_String");
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
                NotifyPropertyChanged("IsSynchronize_String");
                NotifyPropertyChanged("IsSynchronize_Scheduled");
                NotifyPropertyChanged("IsSynchronize_Scheduled_String");
                NotifyPropertyChanged("AutoBrightnessRangeLevel_String");
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

        private void Reset_Click(object sender, DoWorkEventArgs e)
        {
            if (IsSynchronizeMonitor)
            {
                foreach (HomeDevice hd in ModuleOwner.HomeDevices)
                    _ = DdpmCommonHelper.DeviceManagerSA.SetVCPCapability(hd.MonitorInfo, 0x05, 1).Result;
            }
            else
                _ = DdpmCommonHelper.DeviceManagerSA.SetVCPCapability(SelectedHomeDevice.MonitorInfo, 0x05, 1).Result;

            ObjGetVCP rb_10 = DdpmCommonHelper.DeviceManagerSA.GetVCPCapability(SelectedHomeDevice.MonitorInfo, 0x10, opt: 0).Result;
            if (rb_10.result)
            {
                Brightness_Value = (uint)((long)rb_10.value);
                Luminance_Value = BrightnessValue;
            }
            ObjGetVCP rb_12 = DdpmCommonHelper.DeviceManagerSA.GetVCPCapability(SelectedHomeDevice.MonitorInfo, 0x12, opt: 0).Result;
            if (rb_12.result)
                Contrast_Value = (uint)((long)rb_12.value);
        }

        private void Reset_Click_finish(object sender, RunWorkerCompletedEventArgs e)
        {
            IsBusy = false;
            NotifyPropertyChanged("BrightnessValue");
            NotifyPropertyChanged("LuminanceValue");
            NotifyPropertyChanged("ContrastValue");
            NotifyPropertyChanged("IsBusy");
            NotifyPropertyChanged("AutoBrightnessRangeLevel_String");
            DdpmCommonHelper.WriteUILog($"Reset_Click_finish IsBusy : false ...");
        }

        private void RunManual(bool isLuminance)
        {
            if ((MyModule.GetRightView() is BrightnessRightView))
            {
                MyModule.GetRightView().Dispatcher.Invoke((Action)(() =>
                {
                    if (isLuminance)
                    {
                        ((BrightnessRightView)MyModule.GetRightView()).Expander_Manual_Luminance.IsExpanded = true;
                    }
                    else
                    {
                        ((BrightnessRightView)MyModule.GetRightView()).Expander_Manual.IsExpanded = true;
                    }
                }));
            }
        }

        private void RunWorkerCompleted_ColorPreset_Sync(object sender, RunWorkerCompletedEventArgs e)
        {
            //Handling the result and final process

            //IsBusy = false;

            //If BackgroundWorker. WorkerSupportsCancellation is true, and you set e.Cancel=true in DoWorker
            if (e.Cancelled)
            {
                //Log?.Info("** ColorPreset_Sync is cancelled.");
                return;
            }
            if (e.Error != null)
            {
                //The message is e.Error.Message
                //Log?.Info($"** ColorPreset_Sync stopped by an exception: {e.Error.Message}");
                return;
            }
            //
            if (e.Result == null)
            {
                //In case that you never set value to e-Result
                //Log?.Info("** ColorPreset_Sync abnormal stopped unknown reason.");
            }
            else
            {
                //Log?.Info($"** DownloadICCData result: {e.Result}");

                if (e.Result.ToString() == "OK")
                {
                    //Result is passed.
                }
                else
                {
                    //Result is failed.
                }
            }
        }

        private void RunWorkerCompleted_RefreshBrightnessPage(object sender, RunWorkerCompletedEventArgs e)
        {
            MyModule.GetRightView().Dispatcher.Invoke((Action)(() =>
            {
                RefreshUI();
                IsBusy = false;
                NotifyPropertyChanged("IsBusy");
                DdpmCommonHelper.WriteUILog($"RunWorkerCompleted_RefreshBrightnessPage IsBusy : false ...");
            }));

            Invoke_RefreshHotkeySettings();
            Invoke_RefreshManualValue();
        }

        private void RunWorkerCompleted_RefreshData(object sender, RunWorkerCompletedEventArgs e)
        {
            MyModule.GetRightView().Dispatcher.Invoke((Action)(() =>
            {
                RefreshUI();
                IsBusy = false;
                NotifyPropertyChanged("IsBusy");
                DdpmCommonHelper.WriteUILog($"RunWorkerCompleted_RefreshData IsBusy : false ...");
            }));

            //Handling the result and final process
            Debug.WriteLine("RefreshHotkeySettings done");
        }

        public void RunWorkerCompleted_RefreshManualValue(object sender, RunWorkerCompletedEventArgs e)
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
                    NotifyPropertyChanged("AutoBrightnessRangeLevel_String");
                }
                IsBusy = false;
                NotifyPropertyChanged("IsBusy");
                DdpmCommonHelper.WriteUILog($"RunWorkerCompleted_RefreshManualValue IsBusy : false ...");
            }));

            Invoke_RefreshScheduleValue();
        }

        private void RunWorkerCompleted_RefreshScheduleValue(object sender, RunWorkerCompletedEventArgs e)
        {
            MyModule.GetRightView().Dispatcher.Invoke((Action)(() =>
            {
                if (isLuminanceSupport == Visibility.Visible)
                {
                    //NotifyPropertyChanged("LuminanceValue");
                    //NotifyPropertyChanged("LuminanceMaxValue");
                    NotifyPropertyChanged("PR1Name");
                    NotifyPropertyChanged("PR2Name");
                    NotifyPropertyChanged("hOurs1");
                    NotifyPropertyChanged("mIns1");
                    NotifyPropertyChanged("dUration1");
                    NotifyPropertyChanged("hOurs2");
                    NotifyPropertyChanged("mIns2");
                    NotifyPropertyChanged("dUration2");
                    NotifyPropertyChanged("PR1LuminanceValue");
                    NotifyPropertyChanged("PR2LuminanceValue");
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

        private void Set_Brightness_Value(object value_)
        {
            bool r = false;

            void SET_Brightness()
            {
                var value = Convert.ToDouble(value_);
                uint nNewValue = Convert.ToUInt32(value);

                if (IsSynchronize)
                {
                    //Brightness_Value = value;

                    foreach (HomeDevice hd in ModuleOwner.HomeDevices)
                    {
                        if (hd.MonitorInfo.IsDellMonitor && hd.MonitorInfo.CapabilityDic.ContainsKey("12"))
                        {
                            var r = DdpmCommonHelper.DeviceManagerSA.CheckIsSyncBriCon(ModuleOwner.SelectedHomeDevice.MonitorInfo, hd.MonitorInfo).Result;

                            if (r)
                                _ = DdpmCommonHelper.DeviceManagerSA.SetVCPCapability(hd.MonitorInfo, 0x10, nNewValue);
                        }
                    }
                }
                else
                {
                    //Brightness_Value = value;
                    _ = DdpmCommonHelper.DeviceManagerSA.SetVCPCapability(ModuleOwner.SelectedHomeDevice.MonitorInfo, 0x10, nNewValue);
                }

                NotifyPropertyChanged("BrightnessValue");
            }
            if (CheckMonitorALSStatus() && !Start_ALSConfig.isPrimaryMonitorSync)
            {
                bool popMessageResult = false;
                MyModule.GetRightView().Dispatcher.Invoke((Action)(() =>
                {
                    ALSSettingsChangesOnNonPrimary(ref popMessageResult, "BRILEVEL");
                }));

                if (!popMessageResult) //means select no
                {
                    Get_Brightness_Value();
                    return;
                }
                else //means select yes
                    SET_Brightness();

                return;
            }
            if (Start_ALSConfig != null && Start_ALSConfig.isSupportALS > 0 && _autoBrightnessStatus)
            {
                string pop_string = LangHelper.Instance["Brightness.19"];// "Auto Brightness is currently enabled. Do you wish to disable it to continue?";

                MyModule.GetRightView().Dispatcher.Invoke((Action)(() =>
                {
                    r = DdpmCommonHelper.DDPMMesssageBox(LangHelper.Instance["Warning"], pop_string, MyModule.GetRightView().Parent);
                }));

                if (r)
                    AutoBrightnessStatus = _autoBrightnessStatus = false;
                else
                {
                    Get_Brightness_Value();
                    return;
                }
            }

            SET_Brightness();
        }

        private void Set_Contrast_Value(object value_)
        {
            var value = Convert.ToDouble(value_);

            uint nNewValue = Convert.ToUInt32(value);

            if (IsSynchronize)
            {
                //Contrast_Value = value;

                foreach (HomeDevice hd in ModuleOwner.HomeDevices)
                {
                    if (hd.MonitorInfo.IsDellMonitor && hd.MonitorInfo.CapabilityDic.ContainsKey("12"))
                    {
                        var r = DdpmCommonHelper.DeviceManagerSA.CheckIsSyncBriCon(ModuleOwner.SelectedHomeDevice.MonitorInfo, hd.MonitorInfo).Result;

                        if (r)
                            _ = DdpmCommonHelper.DeviceManagerSA.SetVCPCapability(hd.MonitorInfo, 0x12, nNewValue);
                    }
                }
            }
            else
            {
                //Contrast_Value = value;
                _ = DdpmCommonHelper.DeviceManagerSA.SetVCPCapability(ModuleOwner.SelectedHomeDevice.MonitorInfo, 0x12, nNewValue).Result;
            }

            NotifyPropertyChanged("ContrastValue");
        }

        private void Set_Luminance_Value(object value_)
        {
            var value = Convert.ToDouble(value_);
            uint nNewValue = Convert.ToUInt32(value);

            if (IsSynchronize)
            {
                //Luminance_Value = value;

                foreach (HomeDevice hd in ModuleOwner.HomeDevices)
                {
                    if (hd.MonitorInfo.IsDellMonitor && !hd.MonitorInfo.CapabilityDic.ContainsKey("12"))
                    {
                        var r = DdpmCommonHelper.DeviceManagerSA.CheckIsSyncBriCon(ModuleOwner.SelectedHomeDevice.MonitorInfo, hd.MonitorInfo).Result;

                        if (r)
                            _ = DdpmCommonHelper.DeviceManagerSA.SetVCPCapability(hd.MonitorInfo, 0x10, nNewValue);
                    }
                }
            }
            else
            {
                //Luminance_Value = value;
                _ = DdpmCommonHelper.DeviceManagerSA.SetVCPCapability(ModuleOwner.SelectedHomeDevice.MonitorInfo, 0x10, nNewValue).Result;
            }

            NotifyPropertyChanged("LuminanceValue");
        }

        private void SetBrightnessLevelDataToObject(int level)
        {
            DdpmCommonHelper.WriteUILog($"SetBrightnessLevelDataToObject in ...");
            if (Start_ALSConfig.AutoBrightnessRangeLevel != null && Start_ALSConfig.AutoBrightnessRangeLevel != null)
            {
                Start_ALSConfig.AutoBrightnessRangeLevel.level_value = level;
                if (level == 0)
                    Start_ALSConfig.AutoBrightnessRangeLevel.level_name = LangHelper.Instance["AutoBrightnessRangeLevel.0"]; //"Low";
                else if (level == 1)
                    Start_ALSConfig.AutoBrightnessRangeLevel.level_name = LangHelper.Instance["AutoBrightnessRangeLevel.1"]; //"Mid";
                else
                    Start_ALSConfig.AutoBrightnessRangeLevel.level_name = LangHelper.Instance["AutoBrightnessRangeLevel.2"]; //"High";
            }
            DdpmCommonHelper.WriteUILog($"SetBrightnessLevelDataToObject out ...");
        }

        private void SetDefaultExpanded(ALSConfig data, bool isLuminance)
        {
            bool isRunAlsNG = false;
            bool isRunScheduledNG = false;
            if (isAlsSupported == Visibility.Visible)
            {
                if ((data?.isSupportALS > 0) && (data?.isAutoBrightness == true
                    || data?.isAutoColorTemp == true || data?.isPrimaryMonitorSync == true
                    || (data?.AutoBrightnessRangeLevel != null && data?.AutoBrightnessRangeLevel.level_value != 1)
                    || (data?.isAutoColorTemp == true && data?.isAutoColorTemp == true) // any typo?
                    ))
                {
                    if ((MyModule.GetRightView() is BrightnessRightView))
                    {
                        MyModule.GetRightView().Dispatcher.Invoke((Action)(() =>
                        {
                            ((BrightnessRightView)MyModule.GetRightView()).Expander_Auto.IsExpanded = true;
                        }));
                        isRunAlsNG = true;
                    }
                }
            }
            else if (IsScheduledShow == Visibility.Visible || IsScheduledLuminanceShow == Visibility.Visible)
            {
                if (PR1Luminance_Value < 0 || PR2Luminance_Value < 0 || PR1Brightness_Value < 0 || PR2Brightness_Value < 0 || PR1Contrast_Value < 0 || PR2Contrast_Value < 0 || hOurs_1 < 0 || hOurs_2 < 0 || mIns_1 < 0 || mIns_2 < 0 || dUration_1 < 0 || dUration_2 < 0)
                {
                    GetScheduleInfo();
                    if (ScheduleMap.IsEnable == true &&
                        MyModule.GetRightView() is BrightnessRightView)
                    {
                        MyModule.GetRightView().Dispatcher.Invoke((Action)(() =>
                        {
                            if (isLuminance)
                            {
                                ((BrightnessRightView)MyModule.GetRightView()).Expander_Schedule_Luminance.IsExpanded = true;
                            }
                            else
                            {
                                ((BrightnessRightView)MyModule.GetRightView()).Expander_Schedule.IsExpanded = true;
                            }
                        }));
                        isRunScheduledNG = true;
                    }
                }
            }
            if (!isRunAlsNG && !isRunScheduledNG)
            {
                RunManual(isLuminance);
            }
        }
    }
}