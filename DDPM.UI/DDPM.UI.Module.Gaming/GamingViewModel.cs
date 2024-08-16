using CommunityToolkit.Mvvm.ComponentModel;
using DDPM.SA.Common;
using DDPM.UI.Common;
using DDPM.UI.Common.Interfaces;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

[assembly: InternalsVisibleTo("DDPM.UI.Module.Gaming.Tests")]

namespace DDPM.UI.Module.Gaming
{
    internal class GamingViewModel : ObservableObject
    {
        private UI_Properties? _selectedResolution;
        private UI_Orientation? _selectedOrientation;
        private bool _HDRStatus, _IsHighDataSpeed, _IsHighResolution, _SupportedHDR, _SupportedUSBCPrioeitization;
        public IModuleOwner? ModuleOwner { get; set; }
        public GamingModule MyModule { get; set; }
        public List<UI_Properties> Resolution_ItemsCollection { get; set; }

        public UI_Properties SelectedResolution
        {
            get => _selectedResolution;
            set
            {
                SetProperty(ref _selectedResolution, value);
                //DdpmCommonHelper.DeviceManagerSA.SetGamingt(MyModule.SelectedHomeDevice.MonitorInfo.DisplayName,
                //    _selectedResolution.Properties,
                //    _selectedOrientation.Orientation
                //    ).Wait();
            }
        }

        public List<UI_Orientation> Orientation_ItemsCollection { get; set; }

        public UI_Orientation SelectedOrientation
        {
            get => _selectedOrientation;
            set
            {
                SetProperty(ref _selectedOrientation, value);
                //DdpmCommonHelper.DeviceManagerSA.SetGamingt(MyModule.SelectedHomeDevice.MonitorInfo.DisplayName,
                //    _selectedResolution.Properties,
                //    _selectedOrientation.Orientation
                //    ).Wait();
            }
        }

        public bool HDRStatus
        {
            get => _HDRStatus;
            set
            {
                SetProperty(ref _HDRStatus, value);
                DdpmCommonHelper.DeviceManagerSA.SetHDRStatus(MyModule.SelectedHomeDevice.MonitorInfo, _HDRStatus).Wait();
                RefreshUI();
            }
        }

        public string HDRStatus_String
        {
            get
            {
                return HDRStatus ? "ON" : "OFF";
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
                    DdpmCommonHelper.DeviceManagerSA.SetUSBCPrioritizationType(MyModule.SelectedHomeDevice.MonitorInfo,
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
                    DdpmCommonHelper.DeviceManagerSA.SetUSBCPrioritizationType(MyModule.SelectedHomeDevice.MonitorInfo,
                    USBCPrioritizationType.HighResolution).Wait();
                }
            }
        }

        public Visibility SupportedHDR
        {
            get => _SupportedHDR ? Visibility.Visible : Visibility.Collapsed;
        }

        public Visibility SupportedUSBCPrioeitization
        {
            get => _SupportedUSBCPrioeitization ? Visibility.Visible : Visibility.Collapsed;
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

        private void DoWork_RefreshData(object sender, DoWorkEventArgs e)
        {
            try //2024-06-19 Elie, add try catch to get exception.
            {
                Resolution_ItemsCollection = new List<UI_Properties>();
                Orientation_ItemsCollection = new List<UI_Orientation>();

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
            OnPropertyChanged("SupportedHDR");
            OnPropertyChanged("HDRStatus");
            OnPropertyChanged("HDRStatus_String");
            OnPropertyChanged("SupportedUSBCPrioeitization");
            OnPropertyChanged("IsHighDataSpeed");
            OnPropertyChanged("IsHighResolution");
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

    internal class UI_Orientation
    {
        private string[] Orientations_Str = new string[] { "Landscape", "Portrait", "Landscape(flipped)", "Portrait(flipped)" };
        public DisplayOrientation Orientation { get; set; }

        public string DisplayText
        {
            get
            {
                return $"{Orientations_Str[(int)Orientation]}";
            }
        }
    }
}