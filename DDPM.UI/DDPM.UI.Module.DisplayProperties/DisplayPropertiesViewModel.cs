using CommunityToolkit.Mvvm.ComponentModel;
using DDPM.SA.Common;
using DDPM.UI.Common;
using DDPM.UI.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Dell.Client.Framework.Common;
using VcpCore.Common;
[assembly: InternalsVisibleTo("DDPM.UI.Module.DisplayProperties.Tests")]

namespace DDPM.UI.Module.DisplayProperties
{
    //0604 Bruce 將原本.xaml.cs中change select item和swich click事件放到這裡實作
    internal class DisplayPropertiesViewModel : ObservableObject
    {
        private UI_Properties? _selectedResolution;
        private UI_Orientation? _selectedOrientation;
        private bool _HDRStatus, _IsHighDataSpeed, _IsHighResolution, _SupportedHDR, _SupportedUSBCPrioeitization, _HDREnable = true;
        public IModuleOwner? ModuleOwner { get; set; }
        public DisplayPropertiesModule MyModule { get; set; }
        public List<UI_Properties> Resolution_ItemsCollection { get; set; }
        public UI_Properties SelectedResolution
        {
            get => _selectedResolution;
            set
            {
                SetProperty(ref _selectedResolution, value);
                DdpmCommonHelper.DeviceManagerSA.SetDisplayPropertiest(MyModule.SelectedHomeDevice.MonitorInfo,
                    _selectedResolution.Properties,
                    _selectedOrientation.Orientation
                    ).Wait();
            }
        }
        public List<UI_Orientation> Orientation_ItemsCollection { get; set; }
        public UI_Orientation SelectedOrientation
        {
            get => _selectedOrientation;
            set
            {
                SetProperty(ref _selectedOrientation, value);
                DdpmCommonHelper.DeviceManagerSA.SetDisplayPropertiest(MyModule.SelectedHomeDevice.MonitorInfo,
                    _selectedResolution.Properties,
                    _selectedOrientation.Orientation
                    ).Wait();
            }
        }
        public bool HDRStatus
        {
            get => _HDRStatus;
            set
            {
                SetProperty(ref _HDRStatus, value);
                DdpmCommonHelper.DeviceManagerSA.SetHDRStatus(MyModule.SelectedHomeDevice.MonitorInfo, _HDRStatus).Wait();
                EventManagerArgs args = new EventManagerArgs(_HDRStatus);
                DdpmCommonHelper.MyConsole.RaiseEvent("DisplayHDRStatusChanged", this, args);
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
        public bool HDREnable
        {
            get
            {
                OnPropertyChanged("HDROpacity");
                return _HDREnable;
            }
        }
        public string HDROpacity
        {
            get
            {
                if (_HDREnable)
                {
                    return "1.0";
                }
                return "0.5";
            }
        }
        #region UI Enable Flags
        private bool _isBusy = false;
        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }
        #endregion
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

                //Robert_Lin 2024-05-15: No this method "GetDisplayPropertiesInfo"
                DisplayPropertiesInfo displayPropertiesInfo = DdpmCommonHelper.DeviceManagerSA.GetDisplayPropertiesInfo(MyModule.SelectedHomeDevice.MonitorInfo).Result;
                //Add return to let program continue running
                //return;
                _SupportedHDR = displayPropertiesInfo.SupportedHDR;
                _HDRStatus = displayPropertiesInfo.isHDREnable;
                EventManagerArgs args = new EventManagerArgs(_HDRStatus);
                DdpmCommonHelper.MyConsole.RaiseEvent("DisplayHDRStatusChanged", this, args);
                _HDREnable = true;
                if (_SupportedHDR)
                {
                    UInt16 PipMode_Off = 0;
                    ObjGetVCP ret = DdpmCommonHelper.DeviceManagerSA.GetPxpMode(MyModule.SelectedHomeDevice.MonitorInfo).Result;
                    if (ret.result)
                    {
                        UInt16 _curPxpMode = Convert.ToUInt16(ret.value);
                        if (_curPxpMode != PipMode_Off)
                        {
                            _HDREnable = false;
                        }
                    }
                }
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
                if (!(MyModule.SelectedHomeDevice.MonitorInfo.inputSource.ToUpper().StartsWith("USB-C") ||
                    MyModule.SelectedHomeDevice.MonitorInfo.inputSource.ToUpper().StartsWith("THUNDERBOLT"))) // 2024-08-07 By Bruce.
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
                foreach (DisplayOrientation orientation in displayPropertiesInfo.SupportedProperties.Orientations)
                {
                    Orientation_ItemsCollection.Add(new UI_Orientation()
                    {
                        Orientation = orientation
                    });
                }
                _selectedOrientation = Orientation_ItemsCollection.Find(x => (x.Orientation == displayPropertiesInfo.CurrentOrientation));
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
            OnPropertyChanged("HDREnable");
            OnPropertyChanged("SupportedUSBCPrioeitization");
            OnPropertyChanged("IsHighDataSpeed");
            OnPropertyChanged("IsHighResolution");
        }
        public void UpdateHDRStatus()
        {
            if (Resolution_ItemsCollection != null && Resolution_ItemsCollection.Count > 0)
            {
                _HDREnable = true;
                if (_SupportedHDR)
                {
                    UInt16 PipMode_Off = 0;
                    ObjGetVCP ret = DdpmCommonHelper.DeviceManagerSA.GetPxpMode(MyModule.SelectedHomeDevice.MonitorInfo).Result;
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
        string[] Orientations_Str = new string[] { "Landscape", "Portrait", "Landscape(flipped)", "Portrait(flipped)" };
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
