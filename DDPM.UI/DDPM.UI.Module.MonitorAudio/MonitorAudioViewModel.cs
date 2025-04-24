using CommunityToolkit.Mvvm.ComponentModel;
using DDPM.SA.Common;
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

[assembly: InternalsVisibleTo("DDPM.UI.Module.DisplayProperties.Tests")]

namespace DDPM.UI.Module.MonitorAudio
{
    //0604 Bruce 將原本.xaml.cs中change select item和swich click事件放到這裡實作
    internal class MonitorAudioViewModel : ObservableObject
    {
        private UI_Properties? _selectedResolution;
        private UI_Orientation? _selectedOrientation;
        private bool _IsHighDataSpeed, _IsHighResolution, _SupportedUSBCPrioeitization;
        public IModuleOwner? ModuleOwner { get; set; }
        public MonitorAudioModule MyModule { get; set; }
        public List<UI_Properties> Resolution_ItemsCollection { get; set; }

        public UI_Properties SelectedResolution
        {
            get => _selectedResolution;
            set
            {
                SetProperty(ref _selectedResolution, value);
                DdpmCommonHelper.DeviceManagerSA.SetResolutions(DdpmCommonHelper.ModuleOwner.SelectedHomeDevice.MonitorInfo,
                    _selectedResolution.Properties
                    ).Wait();
            }
        }
        public Visibility Orientation_IsVisibility { get; set; } = Visibility.Visible;
        public bool Orientation_IsEnabled { get; set; } = true;
        public List<UI_Orientation> Orientation_ItemsCollection { get; set; }

        public UI_Orientation SelectedOrientation
        {
            get => _selectedOrientation;
            set
            {
                if (value != null)
                {
                    SetProperty(ref _selectedOrientation, value);
                    if (DdpmCommonHelper.DeviceManagerSA.SetOrientation(DdpmCommonHelper.ModuleOwner.SelectedHomeDevice.MonitorInfo,
                        _selectedOrientation.Orientation
                        ).Result)
                    {
                        DisplayPropertiesInfo displayPropertiesInfo = DdpmCommonHelper.DeviceManagerSA.GetDisplayPropertiesInfo(DdpmCommonHelper.ModuleOwner.SelectedHomeDevice.MonitorInfo).Result;
                        Resolution_ItemsCollection.Clear();
                        MyModule.GetRightView().Dispatcher.Invoke((Action)(() =>
                        {
                            foreach (Properties Properties in displayPropertiesInfo.SupportedProperties.Properties)
                            {
                                Resolution_ItemsCollection.Add(new UI_Properties
                                {
                                    Properties = Properties
                                });
                            }
                        }));
                        _selectedResolution = Resolution_ItemsCollection.Find(x => (x.Properties.isCurrent));
                        RefreshUI();
                        //DdpmCommonHelper.MyShowPluginManager?.ShowHomePage("GeHomeFirst");
                    }
                }
            }
        }
        public bool IsHighDataSpeed
        {
            get => _IsHighDataSpeed;
            set
            {
                SetProperty(ref _IsHighDataSpeed, value);
                if (_IsHighDataSpeed)
                {
                    DdpmCommonHelper.DeviceManagerSA.SetUSBCPrioritizationType(DdpmCommonHelper.ModuleOwner.SelectedHomeDevice.MonitorInfo,
                    USBCPrioritizationType.HighDataSpeed).Wait();
                }
            }
        }

        public bool IsHighResolution
        {
            get => _IsHighResolution;
            set
            {
                SetProperty(ref _IsHighResolution, value);
                if (_IsHighResolution)
                {
                    DdpmCommonHelper.DeviceManagerSA.SetUSBCPrioritizationType(DdpmCommonHelper.ModuleOwner.SelectedHomeDevice.MonitorInfo,
                    USBCPrioritizationType.HighResolution).Wait();
                }
            }
        }
        public Visibility SupportedUSBCPrioeitization
        {
            get => _SupportedUSBCPrioeitization ? Visibility.Visible : Visibility.Collapsed;
        }
        
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
        #region USB-C Prioritization
        private bool _Lock_USBCrioritization;
        public bool Lock_USBCrioritization
        {
            get
            {
                return _Lock_USBCrioritization;
            }
            set
            {
                _Lock_USBCrioritization = value;
                OnPropertyChanged("USBCrioritizationUI_IsTabStoppable");
                OnPropertyChanged("USBCrioritizationUI_Opacity");
                OnPropertyChanged("USBCrioritizationUI_LockTooltip");
                OnPropertyChanged("USBCrioritizationUI_NoLock");
            }
        }
        public bool USBCrioritizationUI_IsTabStoppable
        {
            get
            {
                return _Lock_USBCrioritization ? false : true;
            }
        }
        public string USBCrioritizationUI_Opacity
        {
            get
            {
                return _Lock_USBCrioritization ? "0.5" : "1.0";
            }
        }
        public Visibility USBCrioritizationUI_LockTooltip
        {
            get
            {
                return _Lock_USBCrioritization ? Visibility.Visible : Visibility.Collapsed;
            }
        }
        public Visibility USBCrioritizationUI_NoLock
        {
            get
            {
                return _Lock_USBCrioritization ? Visibility.Collapsed : Visibility.Visible;
            }
        }
        #endregion
        #endregion
        #region UI Enable Flags

        private bool _isBusy = false;

        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        #endregion UI Enable Flags

        public void Invoke_RefreshData()
        {
            BackgroundWorker bw = new BackgroundWorker()
            {
                WorkerReportsProgress = false,
                WorkerSupportsCancellation = false
            };
            bw.DoWork += DoWork_RefreshData;
            bw.RunWorkerCompleted += RunWorkerCompleted_RefreshData;
            bw.RunWorkerAsync(); //myArg is the optional argument
            IsBusy = true;
        }

        private void DoWork_RefreshData(object sender, DoWorkEventArgs e)
        {
            try //2024-06-19 Elie, add try catch to get exception.
            {
                Resolution_ItemsCollection = new List<UI_Properties>();
                Orientation_ItemsCollection = new List<UI_Orientation>();
                MonitorInfo currentMonitorInfo = DdpmCommonHelper.ModuleOwner.SelectedHomeDevice.MonitorInfo;
                //Robert_Lin 2024-05-15: No this method "GetDisplayPropertiesInfo"
                DisplayPropertiesInfo displayPropertiesInfo = DdpmCommonHelper.DeviceManagerSA.GetDisplayPropertiesInfo(currentMonitorInfo).Result;
                //Add return to let program continue running
                //return;
                
                _SupportedUSBCPrioeitization = displayPropertiesInfo.SupportedUSBCPrioritization;
                switch (displayPropertiesInfo.USBCPrioritizationType)
                {
                    case USBCPrioritizationType.HighDataSpeed:
                        _IsHighDataSpeed = true;
                        _IsHighResolution = false;
                        break;

                    case USBCPrioritizationType.HighResolution:
                        _IsHighDataSpeed = false;
                        _IsHighResolution = true;
                        break;
                }
                if (!(currentMonitorInfo.inputCable.ToUpper().StartsWith("USB-C") ||
                    currentMonitorInfo.inputCable.ToUpper().StartsWith("THUNDERBOLT"))) // 2024-08-07 By Bruce.
                {
                    _SupportedUSBCPrioeitization = false;
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
                }));

                _selectedResolution = Resolution_ItemsCollection.Find(x => (x.Properties.isCurrent));
                if (displayPropertiesInfo.Supported_OSD_Orientation == null)
                {
                    Orientation_IsVisibility = Visibility.Collapsed;
                }
                else
                {
                    Orientation_IsVisibility = Visibility.Visible;
                    Orientation_IsEnabled = true;
                    foreach (DisplayOrientation orientation in displayPropertiesInfo.SupportedProperties.Orientations)
                    {
                        Orientation_ItemsCollection.Add(new UI_Orientation()
                        {
                            Orientation = orientation
                        });
                    }
                    _selectedOrientation = Orientation_ItemsCollection.Find(x => (x.Orientation == displayPropertiesInfo.CurrentOrientation));
                }
                //Lock/unlock UI init data here (user's lock data should be synced up from IT config, so read user's data directly)
                DDPMSettings data = DdpmCommonHelper.ReadDDPMSettings();//DeviceManagerSA.ReloadAppConfigData().Result;
                Lock_RefreshRate = (bool)data.LockSettings.Lock_Display_ResolutionRefreshRate;
                Trace.WriteLine($"[SettingsPage] DisplayProperty RefreshRate(Lock) : {Lock_RefreshRate}");
                Lock_USBCrioritization = (bool)data.LockSettings.Lock_Display_USBCPrioritization;
                Trace.WriteLine($"[SettingsPage] USBC Prioritization(Lock) : {Lock_USBCrioritization}");

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
            OnPropertyChanged("SelectedOrientation");
            OnPropertyChanged("Resolution_ItemsCollection");
            OnPropertyChanged("Orientation_ItemsCollection");
            OnPropertyChanged("SupportedUSBCPrioeitization");
            OnPropertyChanged("IsHighDataSpeed");
            OnPropertyChanged("IsHighResolution");
            OnPropertyChanged("Orientation_IsVisibility");
            OnPropertyChanged("Orientation_IsEnabled");
        }

        /*public void UpdateHDRStatus()
        {
            if (Resolution_ItemsCollection != null && Resolution_ItemsCollection.Count > 0)
            {
                _HDREnable = true;
                if (_SupportedHDR)
                {
                    UInt16 PipMode_Off = 0;
                    ObjGetVCP ret = DdpmCommonHelper.DeviceManagerSA.GetPxpMode(DdpmCommonHelper.ModuleOwner.SelectedHomeDevice.MonitorInfo).Result;
                    if (ret.result)
                    {
                        UInt16 _curPxpMode = Convert.ToUInt16(ret.value);
                        if (_curPxpMode != PipMode_Off)
                        {
                            _HDREnable = false;
                        }
                    }
                }
                RefreshUI();
            }
        }*/
        public void OSDOrientationChang(object o, DisplayOrientation? e)
        {
            if (e != null)
            {
                _selectedOrientation = Orientation_ItemsCollection.Find(x => (x.Orientation == e));
                RefreshUI();
            }
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

    internal class UI_Orientation
    {
        private string[] Orientations_Str = new string[] { "Landscape", "Portrait", "Landscape_flipped", "Portrait_flipped" };
        public DisplayOrientation Orientation { get; set; }

        public string DisplayText
        {
            get
            {
                return LangHelper.Instance[Orientations_Str[(int)Orientation]];//$"{Orientations_Str[(int)Orientation]}";
            }
        }
    }
}