using DDPM.SA.Common;
using DDPM.UI.Common;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.UX.WPF;
using Microsoft;
using System.Buffers;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using Windows.Media.Capture;
using Windows.Media.Capture.Frames;
using Windows.Storage;

namespace DDPM.UI.Plugin.ViewModels
{
    public class WebCameraViewModel : PeripheralViewModel, INotifyPropertyChanged
    {
        #region Variables
        private readonly ILog _log;
        //private readonly IDeviceManagerSA _deviceManager;
        public readonly IDeviceManagerSA _deviceManager;

        private int _tipSensitivity = 75;
        private int _tiltSensitivity = 20;

        #endregion Variables

        public string TouchScrollCaption { get; set; } = "";
        public string TouchScrollInfoTip { get; set; } = "";
        public string WebCameraSettingCaption { get; set; } = "";
        public string PrimaryButtonCaption { get; set; } = "";
        public string DPISettingCaption { get; set; } = "";
        public string PollingRateCaption { get; set; } = "";
        public string PollingRateInfoTip { get; set; } = "";
        public int ButtonCount { get; set; } = 0;
        public int AppSelectedIndex { get; set; } = 0;

        public new event PropertyChangedEventHandler? PropertyChanged;

        public WebCameraViewModel(IConsole console, ILog log, IDeviceManagerSA deviceManager) : base(console, log, deviceManager)
        {
            Requires.NotNull(console, nameof(console));
            Requires.NotNull(log, nameof(log));

            _log = log;
            _deviceManager = deviceManager;

            IsChecked_FramingGrid = Visibility.Hidden;

            //Hz125ClickedCommand = new RelayCommand(OnHz125Clicked);
            //Hz133ClickedCommand = new RelayCommand(OnHz133Clicked);
            //Hz2501ClickedCommand = new RelayCommand(OnHz2501Clicked);
            //Hz2502ClickedCommand = new RelayCommand(OnHz2502Clicked);
            //Hz333ClickedCommand = new RelayCommand(OnHz333Clicked);
        }

        private void SwitchPollingRate(int index, int hz = 0, bool NeedSetting = false)
        {
            switch (index)
            {
                case 0:
                    Hz125Focused = true;
                    Hz250Focused = false;
                    Hz333Focused = false;
                    OnPropertyChanged(nameof(Hz125Focused));
                    OnPropertyChanged(nameof(Hz250Focused));
                    OnPropertyChanged(nameof(Hz333Focused));
                    break;

                case 1:
                    Hz125Focused = false;
                    Hz250Focused = true;
                    Hz333Focused = false;
                    OnPropertyChanged(nameof(Hz125Focused));
                    OnPropertyChanged(nameof(Hz250Focused));
                    OnPropertyChanged(nameof(Hz333Focused));
                    break;

                case 2:
                    Hz125Focused = false;
                    Hz250Focused = false;
                    Hz333Focused = true;
                    OnPropertyChanged(nameof(Hz125Focused));
                    OnPropertyChanged(nameof(Hz250Focused));
                    OnPropertyChanged(nameof(Hz333Focused));
                    break;

                case 3:
                    Hz133Focused = true;
                    Hz250Focused = false;
                    OnPropertyChanged(nameof(Hz133Focused));
                    OnPropertyChanged(nameof(Hz250Focused));
                    OnPropertyChanged(nameof(Hz333Focused));
                    break;

                case 4:
                    Hz133Focused = false;
                    Hz250Focused = true;
                    OnPropertyChanged(nameof(Hz133Focused));
                    OnPropertyChanged(nameof(Hz250Focused));
                    break;
            }
            if (NeedSetting)
            {
                //_deviceManager.SetBackLightingControls(hz, CurrentDeviceInfo.ID);
            }
        }

        public override void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public void PrepareDeviceInfo(List<DeviceInfo> deviceInfos)
        {
            DeviceInfos.Clear();
            foreach (DeviceInfo deviceInfo in deviceInfos)
            {
                //if(deviceInfo.LogicalDeviceType.Contains("Pen")){
                // 20240627 jim modify
                if (deviceInfo.LogicalDeviceType.Contains("Webcam"))
                {
                    //deviceInfo.Name = "Dell Premier Rechargeable Active WebCamera";
                    //deviceInfo.ModelNumber = "WB7022";
                    DeviceInfos.Add(deviceInfo.ID, deviceInfo);
                }
            }
        }
        public override bool SetCurrentDevice(string deviceID)
        {
            deviceID ??= DeviceInfos.Values.ToList().FirstOrDefault()!.ID.ToString();

            if (!base.SetCurrentDevice(deviceID))
                return false;

            //IsTouchScrollSensitivitySupported = CurrentDeviceInfo.IsTouchScrollSensitivitySupported;
            //IsTouchScrollSensitivitySupported = true;
            //if(IsTouchScrollSensitivitySupported) {
            //  TouchScrollSensitivityLevel = CurrentDeviceInfo.TouchScrollSensitivityLevel;
            //}
            //OnPropertyChanged(nameof(IsTouchScrollSensitivitySupported));

            IsReportRateSupported = CurrentDeviceInfo!.IsReportRateSupported;
            IsReportRateSupported = true;

            OnPropertyChanged(nameof(IsReportRateSupported));
            ReportRate = CurrentDeviceInfo.ReportRate;

            //OnPropertyChanged(nameof(ButtonCollection));
            //PrimaryButtonIndex = (int)CurrentDeviceInfo.MousePrimaryButton;

            ButtonCount = Model switch
            {
                "MS7421W" => 5,
                "MS900" => 3,
                _ => 1,
            };
            return true;
        }
        public override void HandleNotification(DeviceChangedType changeType, DeviceInfo di, string property = "")
        {
            base.HandleNotification(changeType, di, property);

            switch (changeType)
            {
                case DeviceChangedType.Peripherals_SettingsChange:
                    if (DeviceInfos.ContainsKey(di.ID))
                    {
                        DeviceInfos.Remove(di.ID);
                        DeviceInfos.Add(di.ID, di);
                    }
                    else
                    {
                        return;
                    }
                    if (di.ID == CurrentDeviceID)
                    {
                        CurrentDeviceInfo = DeviceInfos[CurrentDeviceID];
                        switch (property)
                        {
                            //case "MousePrimaryButtonChanged":
                            //  PrimaryButtonIndex = (int)di.MousePrimaryButton;
                            //  break;
                            //case "TouchScrollSensitivityLevelChanged":
                            //  TouchScrollSensitivityLevel = di.TouchScrollSensitivityLevel;
                            //  break;
                            //case "DpiValueChanged":
                            //  DPIValue = int.Parse(di.DPIValue);
                            //  break;
                            default:
                                break;
                        }
                        GenerateInfo();
                    }
                    break;

                default:
                    break;
            }
        }

        public int TipSensitivity
        {
            get => _tipSensitivity;
            set
            {
                if (_tipSensitivity != value)
                {
                    _tipSensitivity = value;
                    //DPITextMargin = new double[] { (_DPIValue - DPIMin) * 1.0 / (DPIMax - DPIMin) * 365 + 39, 0, 0, 1 };
                    //if(_DPIValue == DPIMax || _DPIValue == DPIMin) {
                    //  DPIValueText = "";
                    //}
                    //else {
                    //  DPIValueText = _DPIValue.ToString();
                    //}
                    //if(!IsSliderDragging)
                    //  SetDPIValue();
                    OnPropertyChanged();
                    //OnPropertyChanged(nameof(DPITextMargin));
                    //OnPropertyChanged(nameof(DPIValueText));
                }
            }
        }

        public int TiltSensitivity
        {
            get => _tiltSensitivity;
            set
            {
                if (_tiltSensitivity != value)
                {
                    _tiltSensitivity = value;
                    //DPITextMargin = new double[] { (_DPIValue - DPIMin) * 1.0 / (DPIMax - DPIMin) * 365 + 39, 0, 0, 1 };
                    //if(_DPIValue == DPIMax || _DPIValue == DPIMin) {
                    //  DPIValueText = "";
                    //}
                    //else {
                    //  DPIValueText = _DPIValue.ToString();
                    //}
                    //if(!IsSliderDragging)
                    //  SetDPIValue();
                    OnPropertyChanged();
                    //OnPropertyChanged(nameof(DPITextMargin));
                    //OnPropertyChanged(nameof(DPIValueText));
                }
            }
        }

        public int DPIMax { get; set; }
        public int DpiDelta { get; set; }
        public string DPIValueText { get; set; } = "";
        public double[] DPITextMargin { get; set; } = { 0 };//SDL, change to use array
        public bool IsReportRateSupported { get; set; }
        public bool IsDongleRateVisible { get; set; }
        public bool IsBluetoothRateVisible { get; set; }
        public int ReportRate { get; set; }
        public bool Hz125Focused { get; set; }
        public bool Hz133Focused { get; set; }
        public bool Hz250Focused { get; set; }
        public bool Hz333Focused { get; set; }

        // 20240628 jim add
        // MediaCapture and its state variables
        public MediaCapture? _mediaCapture;
        public MediaFrameReader _mediaFrameReader;

        // 20240628 jim add
        public bool captureManagerInitialized = false;
        public bool _running = false;
        public bool _isRecording;

        // 20240628 jim add
        // Folder in which the captures will be stored (initialized in SetupUiAsync)
        public StorageFolder _captureFolder;

        //20240702
        private bool isChecked_Autofocus;

        public bool IsChecked_Autofocus
        {
            get { return isChecked_Autofocus; }
            set
            {
                isChecked_Autofocus = value;
                OnPropertyChanged("IsChecked_Autofocus");
            }
        }

        private string autofocusStatus_String;

        public string AutofocusStatus_String
        {
            get { return autofocusStatus_String; }
            set
            {
                autofocusStatus_String = value;
                OnPropertyChanged("AutofocusStatus_String");
            }
        }

        private bool isChecked_AWB;

        public bool IsChecked_AWB
        {
            get { return isChecked_AWB; }
            set
            {
                isChecked_AWB = value;
                OnPropertyChanged("IsChecked_AWB");
            }
        }

        private string awbStatus_String;

        public string AWBStatus_String
        {
            get { return awbStatus_String; }
            set
            {
                awbStatus_String = value;
                OnPropertyChanged("AWBStatus_String");
            }
        }

        private bool[] _fOV_IsSelected = new bool[3];

        public bool[] FOV_IsSelected
        {
            get { return _fOV_IsSelected; }
            set
            {
                _fOV_IsSelected = value;
                OnPropertyChanged("FOV_IsSelected");
            }
        }

        private Visibility isChecked_FramingGrid;

        public Visibility IsChecked_FramingGrid
        {
            get { return isChecked_FramingGrid; }
            set
            {
                isChecked_FramingGrid = value;
                OnPropertyChanged("IsChecked_FramingGrid");
            }
        }

        private bool framingGrid_isChecked;

        public bool FramingGrid_IsChecked
        {
            get { return framingGrid_isChecked; }
            set
            {
                framingGrid_isChecked = value;
                OnPropertyChanged("FramingGrid_IsChecked");
            }
        }
        public string IsMicEnumerationOnText
        {
            get => (CurrentDeviceInfo!.IsMicEnumerationOn ? Strings.On : Strings.Off;
        }
        public bool IsHoverClickOn
        {
            get => (SelectedButton == PenButtonName.TopBarrelButton.ToString() && PenAction.IsTopBarrelHoverClickOn) || (SelectedButton == PenButtonName.BottomBarrelButton.ToString() && PenAction.IsBottomBarrelHoverClickOn);
            set
            {
                if (SelectedButton == PenButtonName.TopBarrelButton.ToString())
                {
                    _deviceManager.SetIsSideTopButtonHoverClick(itemID, value);
                    PenAction.IsTopBarrelHoverClickOn = value;
                    ActionList.ExportActionList(PenAction, "PEN");
                }
                if (SelectedButton == PenButtonName.BottomBarrelButton.ToString())
                {
                    _deviceManager.SetIsSideBottomButtonHoverClick(itemID, value);
                    PenAction.IsBottomBarrelHoverClickOn = value;
                    ActionList.ExportActionList(PenAction, "PEN");
                }
                OnPropertyChanged();
                //IsMicEnumerationOnText = value ? Strings.On : Strings.Off;
                OnPropertyChanged(nameof(IsMicEnumerationOnText));
            }
        }
    }
}