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
        private double _AudioValue;
        private bool _DisplaySpeakerStatus;
        private bool _SpatialAudioStatus;
        private UI_AudioSource? _selectedAudioSource;
        private UI_AudioProfiles? _selectedAudioProfiles;
        private UI_PBPAudio? _selectedPBPAudio;
        public IModuleOwner? ModuleOwner { get; set; }
        public MonitorAudioModule MyModule { get; set; }

        public double AudioValue
        {
            get
            {
                if (_AudioValue >= 0)
                {
                    return _AudioValue;
                }
                else
                {
                    _AudioValue = 0;
                    return _AudioValue;
                }
            }
            set
            {
                _AudioValue = value;
                OnPropertyChanged("AudioValue");
            }
        }
        public Visibility IsNotMute { get; set; }
        public Visibility IsMute { get; set; }

        public bool DisplaySpeakerStatus
        {
            get => _DisplaySpeakerStatus;
            set
            {
                if (true)
                {
                    SetProperty(ref _DisplaySpeakerStatus, value);
                }
                OnPropertyChanged("DisplaySpeakerStatus");
                OnPropertyChanged("DisplaySpeakerStatus_String");
            }
        }

        public string DisplaySpeakerStatus_String
        {
            get
            {
                return _DisplaySpeakerStatus ? LangHelper.Instance["On"] : LangHelper.Instance["Off"];
            }
        }

        public bool SpatialAudioStatus
        {
            get => _SpatialAudioStatus;
            set
            {
                if (true)
                {
                    SetProperty(ref _SpatialAudioStatus, value);
                }
                OnPropertyChanged("SpatialAudioStatus");
                OnPropertyChanged("SpatialAudioStatus_String");
            }
        }

        public string SpatialAudioStatus_String
        {
            get
            {
                return _SpatialAudioStatus ? LangHelper.Instance["On"] : LangHelper.Instance["Off"];
            }
        }
        public List<UI_AudioSource> AudioSource_ItemsCollection { get; set; }

        public UI_AudioSource SelectedAudioSource
        {
            get => _selectedAudioSource;
            set
            {
                SetProperty(ref _selectedAudioSource, value);
                /*DdpmCommonHelper.DeviceManagerSA.SetResolutions(DdpmCommonHelper.ModuleOwner.SelectedHomeDevice.MonitorInfo,
                    _selectedResolution.Properties
                    ).Wait();*/
            }
        }
        public List<UI_AudioProfiles> AudioProfiles_ItemsCollection { get; set; }

        public UI_AudioProfiles SelectedAudioProfiles
        {
            get => _selectedAudioProfiles;
            set
            {
                SetProperty(ref _selectedAudioProfiles, value);
                /*DdpmCommonHelper.DeviceManagerSA.SetResolutions(DdpmCommonHelper.ModuleOwner.SelectedHomeDevice.MonitorInfo,
                    _selectedResolution.Properties
                    ).Wait();*/
            }
        }
        public List<UI_PBPAudio> PBPAudio_ItemsCollection { get; set; }

        public UI_PBPAudio SelectedPBPAudio
        {
            get => _selectedPBPAudio;
            set
            {
                SetProperty(ref _selectedPBPAudio, value);
                /*DdpmCommonHelper.DeviceManagerSA.SetResolutions(DdpmCommonHelper.ModuleOwner.SelectedHomeDevice.MonitorInfo,
                    _selectedResolution.Properties
                    ).Wait();*/
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
        public bool SetMute(bool isMute)
        {
            if (isMute)
            {
                IsMute = Visibility.Visible;
                IsNotMute = Visibility.Collapsed;
            }
            else
            {
                IsMute = Visibility.Collapsed;
                IsNotMute = Visibility.Visible;
            }
            RefreshUI();
            return true;
        }

        private void DoWork_RefreshData(object sender, DoWorkEventArgs e)
        {
            try //2024-06-19 Elie, add try catch to get exception.
            {
                AudioSource_ItemsCollection = new List<UI_AudioSource>();
                AudioProfiles_ItemsCollection = new List<UI_AudioProfiles>();
                PBPAudio_ItemsCollection = new List<UI_PBPAudio>();
                MonitorInfo currentMonitorInfo = DdpmCommonHelper.ModuleOwner.SelectedHomeDevice.MonitorInfo;

                _AudioValue = 0;
                SetMute(false);

                string[] ss = new string[] { "HDMI/USB-C", "USB" };
                MyModule.GetRightView().Dispatcher.Invoke((Action)(() =>
                {
                    foreach (string s in ss)
                    {
                        AudioSource_ItemsCollection.Add(new UI_AudioSource
                        {
                            AudioSource = s
                        });
                    }
                }));
                ss = new string[] { "Standard", "Movie", "Game", "Music", "Voice", "Custom" };
                MyModule.GetRightView().Dispatcher.Invoke((Action)(() =>
                {
                    foreach (string s in ss)
                    {
                        AudioProfiles_ItemsCollection.Add(new UI_AudioProfiles
                        {
                            AudioProfiles = s
                        });
                    }
                }));
                ss = new string[] { "Main", "Sub", "USB_only" };
                MyModule.GetRightView().Dispatcher.Invoke((Action)(() =>
                {
                    foreach (string s in ss)
                    {
                        PBPAudio_ItemsCollection.Add(new UI_PBPAudio
                        {
                            PBPAudio = s
                        });
                    }
                }));

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
            OnPropertyChanged("AudioValue");
            OnPropertyChanged("DisplaySpeakerStatus");
            OnPropertyChanged("DisplaySpeakerStatus_String");
            OnPropertyChanged("SpatialAudioStatus");
            OnPropertyChanged("SpatialAudioStatus_String");
            OnPropertyChanged("AudioSource_ItemsCollection");
            OnPropertyChanged("SelectedAudioSource");
            OnPropertyChanged("AudioProfiles_ItemsCollection");
            OnPropertyChanged("SelectedAudioProfiles");
            OnPropertyChanged("PBPAudio_ItemsCollection");
            OnPropertyChanged("SelectedPBPAudio");
            OnPropertyChanged("IsMute");
            OnPropertyChanged("IsNotMute");
        }
    }

    internal class UI_AudioSource
    {
        public string AudioSource { get; set; }

        public string DisplayText
        {
            get
            {
                return $"{AudioSource}";
            }
        }
    }

    internal class UI_AudioProfiles
    {
        public string AudioProfiles { get; set; }

        public string DisplayText
        {
            get
            {
                return LangHelper.Instance[$"{AudioProfiles}"];
            }
        }
    }
    internal class UI_PBPAudio
    {
        public string PBPAudio { get; set; }

        public string DisplayText
        {
            get
            {
                return LangHelper.Instance[$"{PBPAudio}"];
            }
        }
    }
}