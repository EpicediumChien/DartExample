using CommunityToolkit.Mvvm.ComponentModel;
using DDPM.SA.Common;
using DDPM.UI.Common;
using DDPM.UI.Common.Interfaces;
using Dell.Client.Framework.Common;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using VcpCore.Common;

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
        public IModuleOwner? ModuleOwner { get; set; }
        public GamingModule MyModule { get; set; }
        public List<UI_Properties> Resolution_ItemsCollection { get; set; }

        public UI_Properties SelectedResolution
        {
            get => _selectedResolution;
            set
            {
                SetProperty(ref _selectedResolution, value);
                DdpmCommonHelper.DeviceManagerSA.SetDisplayPropertiest(MyModule.SelectedHomeDevice.MonitorInfo,
                    _selectedResolution.Properties,
                    DisplayOrientation.Unknow
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
                DdpmCommonHelper.DeviceManagerSA.SetGameEnhancementMode(MyModule.SelectedHomeDevice.MonitorInfo,
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
                DdpmCommonHelper.DeviceManagerSA.SetGaming_ResponseTime(MyModule.SelectedHomeDevice.MonitorInfo,
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
                DdpmCommonHelper.DeviceManagerSA.SetGaming_DarkStabilizer(MyModule.SelectedHomeDevice.MonitorInfo,
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
                DdpmCommonHelper.DeviceManagerSA.SetGaming_HDRType(MyModule.SelectedHomeDevice.MonitorInfo,
                    _selectedHDRType.HDRType
                    ).Wait();
            }
        }
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
                if (GameEnhanceMode_ItemsCollection != null)
                {
                    _selectedGameEnhancementMode = GameEnhanceMode_ItemsCollection.Find(x => (x.GameEnhancementMode.Equals(e.Current_GameEnhancementMode)));
                }
                if (ResponseTime_ItemsCollection != null)
                {
                    _selectedResponseTime = ResponseTime_ItemsCollection.Find(x => (x.ResponseTime.Equals(e.Current_ResponseTime)));
                }
                if (DarkStabilizer_ItemsCollection != null)
                {
                    _selectedDarkStabilizer = DarkStabilizer_ItemsCollection.Find(x => (x.DarkStabilizer.Equals(e.Current_DarkStabilizer)));
                }
                if (HDRType_ItemsCollection != null)
                {
                    _selectedHDRType = HDRType_ItemsCollection.Find(x => (x.HDRType.Equals(e.Current_HDRType)));
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
                Resolution_ItemsCollection = new List<UI_Properties>();
                GameEnhanceMode_ItemsCollection = new List<UI_GameEnhancementMode>();
                ResponseTime_ItemsCollection = new List<UI_ResponseTime>();
                DarkStabilizer_ItemsCollection = new List<UI_DarkStabilizer>();
                HDRType_ItemsCollection = new List<UI_HDRType>();

                GamingDisplayPropertiesInfo displayPropertiesInfo = DdpmCommonHelper.DeviceManagerSA.GetGamingProperties(MyModule.SelectedHomeDevice.MonitorInfo).Result;
                IsGameSeries = MyModule.SelectedHomeDevice.MonitorInfo.modelName.ToUpper().StartsWith("G") ? Visibility.Visible : Visibility.Collapsed;
                IsAWSeries = MyModule.SelectedHomeDevice.MonitorInfo.modelName.ToUpper().StartsWith("AW") ? Visibility.Visible : Visibility.Collapsed;

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
                }));

                _selectedResolution = Resolution_ItemsCollection.Find(x => (x.Properties.isCurrent));
                _selectedGameEnhancementMode = GameEnhanceMode_ItemsCollection.Find(x => (x.GameEnhancementMode.Equals(displayPropertiesInfo.Current_GameEnhancementMode)));
                _selectedResponseTime = ResponseTime_ItemsCollection.Find(x => (x.ResponseTime.Equals(displayPropertiesInfo.Current_ResponseTime)));
                _selectedDarkStabilizer = DarkStabilizer_ItemsCollection.Find(x => (x.DarkStabilizer.Equals(displayPropertiesInfo.Current_DarkStabilizer)));
                _selectedHDRType = HDRType_ItemsCollection.Find(x => (x.HDRType.Equals(displayPropertiesInfo.Current_HDRType)));
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
            OnPropertyChanged("Resolution_ItemsCollection");
            OnPropertyChanged("GameEnhanceMode_ItemsCollection");
            OnPropertyChanged("ResponseTime_ItemsCollection");
            OnPropertyChanged("DarkStabilizer_ItemsCollection");
            OnPropertyChanged("HDRType_ItemsCollection");
            OnPropertyChanged("IsGameSeries");
            OnPropertyChanged("IsAWSeries");
        }
    }

    internal class UI_Properties
    {
        public Properties Properties { get; set; }

        public string DisplayText
        {
            get
            {
                return $"{Properties.Resolutions_Width}x{Properties.Resolutions_High}, {Properties.Frequency}Hz {(Properties.isRecommended ? "(Recommended)" : "")}";
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
                return $"{GameEnhancementMode.ToString().Replace("__", "/").Replace("_", " ")}";
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
                return $"{ResponseTime.ToString().Replace("__", "/").Replace("_", " ")}";
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
                return $"{DarkStabilizer.ToString().Replace("__", "/").Replace("_", " ")}";
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
                return $"{HDRType.ToString().Replace("__", "/").Replace("_", " ")}";
            }
        }
    }
}