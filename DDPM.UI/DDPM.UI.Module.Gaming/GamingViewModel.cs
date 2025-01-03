using CommunityToolkit.Mvvm.ComponentModel;
using DDPM.SA.Common;
using DDPM.SA.Common.Display;
using DDPM.SA.Common.Settings;
using DDPM.UI.Common;
using DDPM.UI.Common.Interfaces;
using DDPM.UI.Resources.Helper;
using Dell.Client.Framework.Common;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;
using VcpCore.Common;
using Windows.System;
using static DDPM.UI.Module.Gaming.UI_HDRType;

[assembly: InternalsVisibleTo("DDPM.UI.Module.Gaming.Tests")]

namespace DDPM.UI.Module.Gaming
{
    internal class GamingViewModel : ObservableObject
    {
        private UI_Properties? _selectedResolution;
        private UI_GameEnhancementMode? _selectedGameEnhancementMode;
        private UI_ResponseTime? _selectedResponseTime;
        private UI_DarkStabilizer? _selectedDarkStabilizer;
        private UI_HDRType? _selectedHDRType;
        private UI_DualResolution? _selectedDualResolution;
        public IModuleOwner? ModuleOwner { get; set; }
        public GamingModule MyModule { get; set; }
        public List<UI_Properties> Resolution_ItemsCollection { get; set; }

        public UI_Properties SelectedResolution
        {
            get => _selectedResolution;
            set
            {
                SetProperty(ref _selectedResolution, value);
                MonitorInfo currentMonitorInfo = DdpmCommonHelper.ModuleOwner.SelectedHomeDevice.MonitorInfo;
                DdpmCommonHelper.DeviceManagerSA.SetResolutions(currentMonitorInfo,
                    _selectedResolution.Properties
                    ).Wait();
            }
        }
        public List<UI_GameEnhancementMode> GameEnhanceMode_ItemsCollection { get; set; }
        public UI_GameEnhancementMode SelectedGameEnhancementMode
        {
            get => _selectedGameEnhancementMode;
            set
            {
                SetProperty(ref _selectedGameEnhancementMode, value);
                MonitorInfo currentMonitorInfo = DdpmCommonHelper.ModuleOwner.SelectedHomeDevice.MonitorInfo;
                DdpmCommonHelper.DeviceManagerSA.SetGameEnhancementMode(currentMonitorInfo,
                    _selectedGameEnhancementMode.GameEnhancementMode
                    ).Wait();
            }
        }
        public List<UI_ResponseTime> ResponseTime_ItemsCollection { get; set; }
        public UI_ResponseTime SelectedResponseTime
        {
            get => _selectedResponseTime;
            set
            {
                SetProperty(ref _selectedResponseTime, value);
                MonitorInfo currentMonitorInfo = DdpmCommonHelper.ModuleOwner.SelectedHomeDevice.MonitorInfo;
                DdpmCommonHelper.DeviceManagerSA.SetGaming_ResponseTime(currentMonitorInfo,
                    _selectedResponseTime.ResponseTime
                    ).Wait();
            }
        }
        public List<UI_DarkStabilizer> DarkStabilizer_ItemsCollection { get; set; }
        public UI_DarkStabilizer SelectedDarkStabilizer
        {
            get => _selectedDarkStabilizer;
            set
            {
                SetProperty(ref _selectedDarkStabilizer, value);
                MonitorInfo currentMonitorInfo = DdpmCommonHelper.ModuleOwner.SelectedHomeDevice.MonitorInfo;
                DdpmCommonHelper.DeviceManagerSA.SetGaming_DarkStabilizer(currentMonitorInfo,
                    _selectedDarkStabilizer.DarkStabilizer
                    ).Wait();
            }
        }
        public List<UI_HDRType> HDRType_ItemsCollection { get; set; }
        public UI_HDRType SelectedHDRType
        {
            get => _selectedHDRType;
            set
            {
                SetProperty(ref _selectedHDRType, value);
                MonitorInfo currentMonitorInfo = DdpmCommonHelper.ModuleOwner.SelectedHomeDevice.MonitorInfo;
                DdpmCommonHelper.DeviceManagerSA.SetGaming_HDRType(currentMonitorInfo,
                    _selectedHDRType.HDRType
                    ).Wait();
            }
        }
        public List<UI_DualResolution> DualResolution_ItemsCollection { get; set; }
        public UI_DualResolution SelectedDualResolution
        {
            get => _selectedDualResolution;
            set
            {
                SetProperty(ref _selectedDualResolution, value);
                MonitorInfo currentMonitorInfo = DdpmCommonHelper.ModuleOwner.SelectedHomeDevice.MonitorInfo;
                DdpmCommonHelper.DeviceManagerSA.SetGaming_DualResolutionType(currentMonitorInfo,
                    _selectedDualResolution.DualResolutionType
                    ).Wait();
            }
        }
        public bool GameEnhanceMode_IsEnable { get; set; }
        public string GameEnhanceMode_Opacity
        {
            get
            {
                if (GameEnhanceMode_IsEnable)
                {
                    return "1.0";
                }
                return "0.5";
            }
        }
        public bool ResponseTime_IsEnable { get; set; }
        public string ResponseTime_Opacity
        {
            get
            {
                if (ResponseTime_IsEnable)
                {
                    return "1.0";
                }
                return "0.5";
            }
        }
        public bool HDRType_IsEnable { get; set; }
        public string HDRType_Opacity
        {
            get
            {
                if (HDRType_IsEnable)
                {
                    return "1.0";
                }
                return "0.5";
            }
        }
        public bool DarkStabilizer_IsEnable { get; set; }
        public string DarkStabilizer_Opacity
        {
            get
            {
                if (DarkStabilizer_IsEnable)
                {
                    return "1.0";
                }
                return "0.5";
            }
        }
        #region hotkey
        private string _darkStabilizerToggleKey = LangHelper.Instance["None"];

        public string DarkStabilizerToggleKey
        {
            get => _darkStabilizerToggleKey;
            set
            {
                SetProperty(ref _darkStabilizerToggleKey, value);
                OnPropertyChanged("DarkStabilizerToggleKey");
                //NotifyPropertyChanged("DarkStabilizerToggleKey");
            }
        }

        private string _dualResolutionToggleKey = LangHelper.Instance["None"];

        public string DualResolutionToggleKey
        {
            get => _dualResolutionToggleKey;
            set
            {
                SetProperty(ref _dualResolutionToggleKey, value);
                OnPropertyChanged("DualResolutionToggleKey");
                //NotifyPropertyChanged("DualResolutionToggleKey");
            }
        }
        public void SaveHotkeySettings(MonitorInfo monitorInfo, HotkeyInfo hotkeyInfo)
        {
            if (DdpmCommonHelper.DeviceManagerSA != null)
            {
                IsBusy = true;
                OnPropertyChanged("IsBusy");
                bool saveSettings = DdpmCommonHelper.DeviceManagerSA.SaveHotkeySetting(monitorInfo, hotkeyInfo).Result;
                if (saveSettings)
                {
                    DdpmCommonHelper.isHotkeyBypass = DdpmCommonHelper.DeviceManagerSA.ByPassHotkey(false).Result;
                    IsBusy = false;
                    OnPropertyChanged("IsBusy");
                }
            }
            Invoke_RefreshHotkeySettings();
        }

        public void Invoke_RefreshHotkeySettings()
        {
            BackgroundWorker bw = new BackgroundWorker()
            {
                WorkerReportsProgress = false,
                WorkerSupportsCancellation = false
            };
            bw.DoWork += DoWork_RefreshHotkeyData;
            bw.RunWorkerCompleted += RunWorkerCompleted_RefreshHotkeyData;
            bw.RunWorkerAsync(ApartmentState.STA);
        }
        private void DoWork_RefreshHotkeyData(object sender, DoWorkEventArgs e)
        {
            var temp = DdpmCommonHelper.DeviceManagerSA.ReadCurrentHotkey(this.MyModule.SelectedHomeDevice.MonitorInfo).Result;
            HotkeySettings curHotkey = temp.Item1;
            string swHortcutText = string.Empty;

            if (curHotkey.HotkeyInfo.Count > 0)
            {
                foreach (var hotkeyInfo in curHotkey.HotkeyInfo)
                {
                    List<VirtualKey> hotkeys = hotkeyInfo.Hotkey;
                    switch (hotkeyInfo.Job)
                    {
                        case HotkeyType.DarkStabilizerToggle:
                            KeysHelper.ReSetHotKeyText(ref swHortcutText, ref hotkeys);
                            hotkeys.Clear();
                            DarkStabilizerToggleKey = swHortcutText;
                            break;

                        case HotkeyType.DualResolutionToggle:
                            KeysHelper.ReSetHotKeyText(ref swHortcutText, ref hotkeys);
                            hotkeys.Clear();
                            DualResolutionToggleKey = swHortcutText;
                            break;
                    }
                }
            }
        }
        private void RunWorkerCompleted_RefreshHotkeyData(object sender, RunWorkerCompletedEventArgs e)
        {
            //Handling the result and final process
            Debug.WriteLine("RefreshHotkeySettings done");
        }
        #endregion
        #region Lock/Unlock
        #region RefreshRate
        private bool _Lock_RefreshRate;
        public bool Lock_RefreshRate
        {
            get
            {
                return _Lock_RefreshRate;
            }
            set
            {
                _Lock_RefreshRate = value;
                OnPropertyChanged("RefreshRateUI_IsEnable");
                OnPropertyChanged("RefreshRateUI_Opacity");
                OnPropertyChanged("RefreshRateUI_LockTooltip");
            }
        }
        public bool RefreshRateUI_IsEnable
        {
            get
            {
                return _Lock_RefreshRate ? false : true;
            }
        }
        public string RefreshRateUI_Opacity
        {
            get
            {
                return _Lock_RefreshRate ? "0.5" : "1.0";
            }
        }
        public Visibility RefreshRateUI_LockTooltip
        {
            get
            {
                return _Lock_RefreshRate ? Visibility.Visible : Visibility.Collapsed;
            }
        }
        #endregion
        #endregion
        public Visibility IsSupported_GameEnhanceMode { get; set; } = Visibility.Collapsed;
        public Visibility IsSupported_ResponseTime { get; set; } = Visibility.Collapsed;
        public Visibility IsSupported_DarkStabilizer { get; set; } = Visibility.Collapsed;
        public Visibility IsSupported_DualResolution { get; set; } = Visibility.Collapsed;
        public Visibility IsGameSeries { get; set; } = Visibility.Collapsed;
        public Visibility IsAWSeries { get; set; } = Visibility.Collapsed;
        #region UI Enable Flags

        private bool _isBusy = false;

        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        #endregion UI Enable Flags
        public void GamingParamChang(object o, GamingDisplayPropertiesInfo e)
        {
            if (e != null)
            {
                if (GameEnhanceMode_ItemsCollection != null && e.Current_GameEnhancementMode != null)
                {
                    _selectedGameEnhancementMode = GameEnhanceMode_ItemsCollection.Find(x => (x.GameEnhancementMode.Equals(e.Current_GameEnhancementMode)));
                }
                if (ResponseTime_ItemsCollection != null && e.Current_ResponseTime != null)
                {
                    _selectedResponseTime = ResponseTime_ItemsCollection.Find(x => (x.ResponseTime.Equals(e.Current_ResponseTime)));
                }
                if (DarkStabilizer_ItemsCollection != null && e.Current_DarkStabilizer != null)
                {
                    _selectedDarkStabilizer = DarkStabilizer_ItemsCollection.Find(x => (x.DarkStabilizer.Equals(e.Current_DarkStabilizer)));
                }
                if (HDRType_ItemsCollection != null && e.Current_HDRType != null)
                {
                    _selectedHDRType = HDRType_ItemsCollection.Find(x => (x.HDRType.Equals(e.Current_HDRType)));
                }
                if (DualResolution_ItemsCollection != null && e.Current_DualResolutionType != null)
                {
                    _selectedDualResolution = DualResolution_ItemsCollection.Find(x => (x.DualResolutionType.Equals(e.Current_DualResolutionType)));
                }
                RefreshUI();
            }
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
            bw.RunWorkerAsync();
            IsBusy = true;
        }

        private void DoWork_RefreshData(object sender, DoWorkEventArgs e)
        {
            try
            {
                MonitorInfo currentMonitorInfo = DdpmCommonHelper.ModuleOwner.SelectedHomeDevice.MonitorInfo;
                Resolution_ItemsCollection = new List<UI_Properties>();
                GameEnhanceMode_ItemsCollection = new List<UI_GameEnhancementMode>();
                ResponseTime_ItemsCollection = new List<UI_ResponseTime>();
                DarkStabilizer_ItemsCollection = new List<UI_DarkStabilizer>();
                HDRType_ItemsCollection = new List<UI_HDRType>();
                DualResolution_ItemsCollection = new List<UI_DualResolution>();
                IsSupported_GameEnhanceMode = Visibility.Collapsed;
                IsSupported_ResponseTime = Visibility.Collapsed;
                IsSupported_DarkStabilizer = Visibility.Collapsed;
                IsSupported_DualResolution = Visibility.Collapsed;
                IsGameSeries = Visibility.Collapsed;
                IsAWSeries = Visibility.Collapsed;
                GamingDisplayPropertiesInfo displayPropertiesInfo = DdpmCommonHelper.DeviceManagerSA.GetGamingProperties_SupportedList(currentMonitorInfo).Result;
                if (displayPropertiesInfo.IsSupported_GameEnhancementMode)
                {
                    displayPropertiesInfo.Current_GameEnhancementMode = DdpmCommonHelper.DeviceManagerSA.GetCurrentGame_EnhancementMode(currentMonitorInfo).Result;
                    IsSupported_GameEnhanceMode = Visibility.Visible;
                }
                if (displayPropertiesInfo.IsSupported_ResponseTime)
                {
                    displayPropertiesInfo.Current_ResponseTime = DdpmCommonHelper.DeviceManagerSA.GetCurrentGaming_ResponseTime(currentMonitorInfo).Result;
                    IsSupported_ResponseTime = Visibility.Visible;
                }
                if (displayPropertiesInfo.IsSupported_DarkStabilizer)
                {
                    displayPropertiesInfo.Current_DarkStabilizer = DdpmCommonHelper.DeviceManagerSA.GetCurrentGaming_DarkStabilizer(currentMonitorInfo).Result;
                    IsSupported_DarkStabilizer = Visibility.Visible;
                }
                if (displayPropertiesInfo.IsSupported_HDRType)
                {
                    displayPropertiesInfo.Current_HDRType = DdpmCommonHelper.DeviceManagerSA.GetCurrentGaming_HDRType(currentMonitorInfo).Result;
                    IsGameSeries = currentMonitorInfo.modelName.ToUpper().StartsWith("G") ? Visibility.Visible : Visibility.Collapsed;
                    IsAWSeries = currentMonitorInfo.modelName.ToUpper().StartsWith("AW") ? Visibility.Visible : Visibility.Collapsed;
                }
                if (displayPropertiesInfo.IsSupported_DualResolutionType)
                {
                    displayPropertiesInfo.Current_DualResolutionType = DdpmCommonHelper.DeviceManagerSA.GetCurrentGaming_DualResolutionType(currentMonitorInfo).Result;
                    IsSupported_DualResolution = Visibility.Visible;
                }

                MyModule.GetRightView().Dispatcher.Invoke((Action)(() =>
                {
                    foreach (Properties Properties in displayPropertiesInfo.SupportedProperties.Properties)
                    {
                        Resolution_ItemsCollection.Add(new UI_Properties
                        {
                            Properties = Properties
                        });
                    }
                    foreach (Gaming_GameEnhancementMode Properties in displayPropertiesInfo.Supported_GameEnhancementMode)
                    {
                        GameEnhanceMode_ItemsCollection.Add(new UI_GameEnhancementMode
                        {
                            GameEnhancementMode = Properties
                        });
                    }
                    foreach (Gaming_ResponseTime Properties in displayPropertiesInfo.Supported_ResponseTime)
                    {
                        ResponseTime_ItemsCollection.Add(new UI_ResponseTime
                        {
                            ResponseTime = Properties
                        });
                    }
                    foreach (Gaming_DarkStabilizer Properties in displayPropertiesInfo.Supported_DarkStabilizer)
                    {
                        DarkStabilizer_ItemsCollection.Add(new UI_DarkStabilizer
                        {
                            DarkStabilizer = Properties
                        });
                    }
                    foreach (Gaming_HDRType Properties in displayPropertiesInfo.Supported_HDRType)
                    {
                        HDRType_ItemsCollection.Add(new UI_HDRType
                        {
                            HDRType = Properties
                        });
                    }
                    foreach (Gaming_DualResolutionType Properties in displayPropertiesInfo.Supported_DualResolutionType)
                    {
                        DualResolution_ItemsCollection.Add(new UI_DualResolution
                        {
                            DualResolutionType = Properties
                        });
                    }
                }));
                GameEnhanceMode_IsEnable = true;
                ResponseTime_IsEnable = true;
                HDRType_IsEnable = true;
                DarkStabilizer_IsEnable = true;
                if (displayPropertiesInfo.Current_GameEnhancementMode == Gaming_GameEnhancementMode.Disable)
                {
                    GameEnhanceMode_IsEnable = false;
                }
                if (displayPropertiesInfo.Current_ResponseTime == Gaming_ResponseTime.Disable)
                {
                    ResponseTime_IsEnable = false;
                }
                if (displayPropertiesInfo.Current_HDRType == Gaming_HDRType.Disable)
                {
                    HDRType_IsEnable = false;
                }
                if (displayPropertiesInfo.Current_DarkStabilizer == Gaming_DarkStabilizer.Disable)
                {
                    DarkStabilizer_IsEnable = false;
                }
                _selectedResolution = Resolution_ItemsCollection.Find(x => (x.Properties.isCurrent));
                _selectedGameEnhancementMode = GameEnhanceMode_ItemsCollection.Find(x => (x.GameEnhancementMode.Equals(displayPropertiesInfo.Current_GameEnhancementMode)));
                _selectedResponseTime = ResponseTime_ItemsCollection.Find(x => (x.ResponseTime.Equals(displayPropertiesInfo.Current_ResponseTime)));
                _selectedDarkStabilizer = DarkStabilizer_ItemsCollection.Find(x => (x.DarkStabilizer.Equals(displayPropertiesInfo.Current_DarkStabilizer)));
                _selectedHDRType = HDRType_ItemsCollection.Find(x => (x.HDRType.Equals(displayPropertiesInfo.Current_HDRType)));
                _selectedDualResolution = DualResolution_ItemsCollection.Find(x => (x.DualResolutionType.Equals(displayPropertiesInfo.Current_DualResolutionType)));
                Invoke_RefreshHotkeySettings();
                //Lock/unlock UI init data here (user's lock data should be synced up from IT config, so read user's data directly)
                DDPMSettings data = DdpmCommonHelper.ReadDDPMSettings();//DeviceManagerSA.ReloadAppConfigData().Result;
                Lock_RefreshRate = (bool)data.LockSettings.Lock_Display_ResolutionRefreshRate;
                Trace.WriteLine($"[SettingsPage] DisplayProperty RefreshRate(Lock) : {Lock_RefreshRate}");
                RefreshUI();
            }
            catch (Exception)
            {
            }
        }

        private void RunWorkerCompleted_RefreshData(object sender, RunWorkerCompletedEventArgs e)
        {
            IsBusy = false;
            //Handling the result and final process
        }

        public void RefreshUI()
        {
            OnPropertyChanged("SelectedResolution");
            OnPropertyChanged("SelectedGameEnhancementMode");
            OnPropertyChanged("SelectedResponseTime");
            OnPropertyChanged("SelectedDarkStabilizer");
            OnPropertyChanged("SelectedHDRType");
            OnPropertyChanged("SelectedDualResolution");
            OnPropertyChanged("Resolution_ItemsCollection");
            OnPropertyChanged("GameEnhanceMode_ItemsCollection");
            OnPropertyChanged("ResponseTime_ItemsCollection");
            OnPropertyChanged("DarkStabilizer_ItemsCollection");
            OnPropertyChanged("HDRType_ItemsCollection");
            OnPropertyChanged("DualResolution_ItemsCollection");
            OnPropertyChanged("IsGameSeries");
            OnPropertyChanged("IsAWSeries");
            OnPropertyChanged("GameEnhanceMode_IsEnable");
            OnPropertyChanged("GameEnhanceMode_Opacity");
            OnPropertyChanged("ResponseTime_IsEnable");
            OnPropertyChanged("ResponseTime_Opacity");
            OnPropertyChanged("HDRType_IsEnable");
            OnPropertyChanged("HDRType_Opacity");
            OnPropertyChanged("DarkStabilizer_IsEnable");
            OnPropertyChanged("DarkStabilizer_Opacity");
            OnPropertyChanged("IsSupported_GameEnhanceMode");
            OnPropertyChanged("IsSupported_ResponseTime");
            OnPropertyChanged("IsSupported_DarkStabilizer");
            OnPropertyChanged("IsSupported_DualResolution");
        }
    }

    internal class UI_Properties
    {
        public Properties Properties { get; set; }

        public string DisplayText
        {
            get
            {
                return $"{Properties.Resolutions_Width}x{Properties.Resolutions_High}, {Properties.Frequency}Hz {(Properties.isRecommended ? $"({LangHelper.Instance["Recommended"]})" : "")}";
            }
        }
    }
    internal class UI_GameEnhancementMode
    {
        public Gaming_GameEnhancementMode GameEnhancementMode { get; set; }

        public string DisplayText
        {
            get
            {
                return LangHelper.Instance[GameEnhancementMode.ToString()]; //$"{GameEnhancementMode.ToString().Replace("__", "/").Replace("_", " ")}";
            }
        }
    }
    internal class UI_ResponseTime
    {
        public Gaming_ResponseTime ResponseTime { get; set; }

        public string DisplayText
        {
            get
            {
                return LangHelper.Instance[ResponseTime.ToString()]; //$"{ResponseTime.ToString().Replace("__", "/").Replace("_", " ")}";
            }
        }
    }
    internal class UI_DarkStabilizer
    {
        public Gaming_DarkStabilizer DarkStabilizer { get; set; }

        public string DisplayText
        {
            get
            {
                return LangHelper.Instance[DarkStabilizer.ToString()]; //$"{DarkStabilizer.ToString().Replace("__", "/").Replace("_", " ")}";
            }
        }
    }
    internal class UI_HDRType
    {
        public Gaming_HDRType HDRType { get; set; }

        public string DisplayText
        {
            get
            {
                return LangHelper.Instance[HDRType.ToString()]; //$"{HDRType.ToString().Replace("__", "/").Replace("_", " ")}";
            }
        }
        internal class UI_DualResolution
        {
            public Gaming_DualResolutionType DualResolutionType { get; set; }

            public string DisplayText
            {
                get
                {
                    return LangHelper.Instance[DualResolutionType.ToString()];// $"{DualResolutionType.ToString().Replace("__", "/").Replace("_", "")}";
                }
            }
        }
    }
}