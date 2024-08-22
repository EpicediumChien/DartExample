using CommunityToolkit.Mvvm.ComponentModel;
using DDPM.SA.Common;
using DDPM.UI.Common.ViewModels;
using DPeMPublic.Common.Enums;
using System.Net;
using System.Windows.Media;
using VcpCore.Common;

namespace DDPM.UI.Common.Models
{
    //Robert_Lin, 2024-7-10, add IComparable<HomeDevice>
    public class HomeDevice : ObservableObject, IComparable<HomeDevice>
    {
        private eDeviceCategory _deviceCategory;
        private string _deviceName = "";
        private ImageSource? _deviceImage;
        private double _normalWidth = 400;

        private MonitorInfo? _monitorInfo;
        private DeviceInfo? _deviceInfo;

        /// <summary>
        /// MonitorModelName, will be used to display on HomePage (Tooltip) and Display Landing Page ComboBox
        /// The value will be extracted from MonitorInfo's capability string, see
        /// </summary>

        public HomeDevice()
        {
            NormalWidth = 400;
        }
        public ImageSource? DeviceImage
        {
            get => _deviceImage;
            set => SetProperty(ref _deviceImage, value);
        }

        /// <summary>
        /// To be removed (2024-6-20), it's replaced by DisplayName property
        /// </summary>
        public string DeviceName
        {
            get => _deviceName;
            set => SetProperty<string>(ref _deviceName, value);
        }

        public eDeviceCategory DeviceCategory
        {
            get => _deviceCategory;
            set => SetProperty(ref _deviceCategory, value);
        }

        public MonitorInfo? MonitorInfo
        {
            get => _monitorInfo;
            set
            {
                SetProperty(ref _monitorInfo, value);
                //    MonitorIndicator indicator = new MonitorIndicator()
                //    {
                //        InputSource = "HDMI"
                //    };
                //    StatusIndicator = indicator;
                UpdateBatteryIndicator();

                //2024-6-20 Get the model from capability string
                string model = GetModelFromMonitorCapabilityString(_monitorInfo.CapabilityString);
                //If fail to get mode from CapabilityString, then use AliasDeviceName instead
                if (String.IsNullOrEmpty(model))
                {
                    _monitorModelName = _monitorInfo.AliasDeviceName;
                }
                else
                {
                    _monitorModelName = model;
                }
                OnPropertyChanged("DisplayName");
            }
        }

        public DeviceInfo? DeviceInfo
        {
            get => _deviceInfo;
            set
            {
                SetProperty(ref _deviceInfo, value);
                //BatteryIndicator indicator = new BatteryIndicator();
                if (_deviceInfo != null)
                {
                    //indicator.ConnectionType = _deviceInfo?.PhysicalDeviceType == "PhysicalDongle" ? "Dongle" : "Bluetooth";
                    //indicator.BatteryLevel = (double) (_deviceInfo.BatteryLevel < 0 ? 0 : _deviceInfo.BatteryLevel);
                    //indicator.BatteryStatus = _deviceInfo.BatteryStatus;

                    //2024-6-20 Determine the DeviceImage internally
                    DeterminePeripheralDeviceImage();
                }

                //StatusIndicator = indicator;
                UpdateBatteryIndicator();
            }
        }

        public bool IsSamePeripheralDevice(DeviceInfo di)
        {
            //If this HomeDevice is not peripheral or deviceinfo not set
            if (DeviceInfo == null)
                return false;
            //return (di.PhyscialDeviceID.Equals(DeviceInfo.PhyscialDeviceID) && (di.Type == DeviceInfo.Type)
            //    && (di.Name.Equals(DeviceInfo.Name)));
            return (di.ID.Equals(DeviceInfo.ID)); // 2024-07-12, Provided by Hess.
        }

        /*
        private UserControl? _statusIndicator;
        public UserControl? StatusIndicator
        {
            get => _statusIndicator;
            set
            {
                SetProperty(ref _statusIndicator, value);
            }
        }
        */
        #region RWD
        public double NormalWidth
        {
            get => _normalWidth;
            set
            {
                SetProperty(ref _normalWidth, value);
                OnPropertyChanged("HoverWidth");
            }
        }

        public double HoverWidth
        {
            get
            {
                return NormalWidth * (double)1.10;
            }
        }
        public double ItemWidth
        {
            get
            {
                return NormalWidth * 1.16;
            }
        }
        #endregion RWD

        #region Tooltip info
        public string FwVer
        {
            get
            {
                if (MonitorInfo != null)
                {
                    return MonitorInfo.FwVersion;
                }
                else
                {
                    return "(N/A)";
                }
            }
        }

        public string ServiceTag
        {
            get
            {
                if (MonitorInfo != null)
                {
                    return MonitorInfo.edid.ServiceTag;
                }
                else
                {
                    return "(N/A)";
                }
            }
        }

        public string MfgDate
        {
            get
            {
                if (MonitorInfo != null)
                {
                    DateTime dtMfg = new DateTime(MonitorInfo.edid.Year, MonitorInfo.edid.Month, 1);
                    string mfgDate = dtMfg.ToString("MMM yyyy");
                    return mfgDate;
                }
                else
                {
                    return "(N/A)";
                }
            }
        }

        public string TooltipModelName
        {
            get
            {
                if (DeviceInfo != null)
                {
                    return DeviceInfo.Name;
                }
                else if (MonitorInfo != null)
                {
                    //Robert_Lin, 2024-6-20, change to DisplayName (with (instanceNo)
                    //return MonitorInfo.AliasDeviceName;
                    return DisplayName;
                }
                return DeviceCategory.ToString();
            }
        }
        #endregion Tooltip info

        #region BatteryIndicator

        public void UpdateBatteryIndicator()
        {
            OnPropertyChanged("BatteryLevel");
            OnPropertyChanged("BatteryStatus");
            OnPropertyChanged("ConnectionType");
            OnPropertyChanged("NoBattery");
            OnPropertyChanged("Text1");
        }

        public double BatteryLevel
        {
            get
            {
                if (DeviceInfo != null)
                {
                    //Robert_Lin, 2024-7-10, help Hess to modify
                    //return (double) (DeviceInfo.BatteryLevel < 0 ? 0 : DeviceInfo.BatteryLevel);
                    return (double)(DeviceInfo.BatteryLevel);
                }
                return 0;
            }
        }

        public string BatteryStatus
        {
            get
            {
                if (DeviceInfo != null)
                {
                    return DeviceInfo.BatteryStatus;
                }
                if (MonitorInfo != null)
                {
                    return "";
                }
                return string.Empty;
            }
        }

        public string ConnectionType
        {
            get
            {
                if (DeviceInfo != null)
                {
                    //Robert_Lin, 2024-7-22, Updated due to PeripheralViewModel has been changed, may be DPeM changed.
                    //So we will get the ConnectType from the Helper function in HomeDevice.cs
                    return HomeDevice.GetConnectionTypeFromDeviceInfo(DeviceInfo);
                    /*
                    //0614 Bruce 新增判斷Dock裝置，將原本的if判斷修改成switch
                    switch (DeviceInfo.PhysicalDeviceType)
                    {
                        case DPeMPublic.Common.Enums.DeviceType.PhysicalAudioDongle:
                        case DPeMPublic.Common.Enums.DeviceType.PhysicalDongle:
                            return "Dongle";

                        case DPeMPublic.Common.Enums.DeviceType.PhysicalWiredDock:
                            return "Port";

                        default:
                            //Robert_Lin, 2024-7-10 help Hess to modify
                            //return "Bluetooth";
                            return DeviceInfo.PhysicalDeviceType.ToString().Replace("Physical", "");
                    }
                    /*return DeviceInfo.PhysicalDeviceType == DPeMPublic.Common.Enums.DeviceType.PhysicalDongle ? "Dongle" : "Bluetooth";*/
                }
                else if (MonitorInfo != null)
                {
                    return "Port";
                }
                return string.Empty;
            }
        }

        public bool NoBattery
        {
            get
            {
                if (DeviceInfo != null)
                {
                    return !DeviceInfo.IsBatteryLevelSupported;
                }
                else if (MonitorInfo != null)
                {
                    return true;
                }
                return false;
            }
        }

        //Workaround for InputSource put in here
        //private string _text1 = "";

        public string Text1
        {
            get
            {
                if (DeviceInfo != null)
                {
                    return "";
                }
                else if (MonitorInfo != null)
                {
                    //2024-5-24 Robert_Lin, remove the tail number and dash
                    // "USB-C1" => "USB-C"; "HDMI-1" => "HDMI"
                    //Rule:
                    // 1 If tail char is number => remove it
                    // 2 If tail char is '-' => remove it
                    string strOut = MonitorInfo.inputSource;
                    char[] digits = new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '-' };
                    strOut = strOut.TrimEnd(digits);
                    strOut = strOut.TrimEnd(digits);
                    strOut = strOut.TrimEnd(digits);
                    return strOut;
                }
                return "";
            }
            //set => _text1 = value;
        }
        #endregion BatteryIndicator

        #region DisplayName
        /// <summary>
        /// The display name on
        /// 1. HomePage HomeDevice's tooltip,
        /// 2. Landing Page: ComboBox item name or display name if only one monitor.
        /// For monitor, it will append " ({InstanceNo})" if InstanceNo is not zero.
        /// </summary>
        public string DisplayName
        {
            get
            {
                //For Monitor, it will be
                // If (InstanceNo==0)" => _monitorModelName, Else => _monitorModelName + " ({InstanceNo})"
                if (MonitorInfo != null)
                {
                    //If the InstanceNo is 0
                    if (InstanceNo == 0)
                        return _monitorModelName;
                    else
                        return _monitorModelName + $" ({InstanceNo})";
                }
                else
                {
                    //For peripherals, it will display ModelNumber
                    if (DeviceInfo != null)
                    {
                        return DeviceInfo.ModelNumber;
                    }
                }
                return "";
            }
        }

        /// <summary>
        /// For Monitor only. the model value from capability string.
        /// </summary>
        private string _monitorModelName = string.Empty;

        public bool IsSameModel(HomeDevice other)
        {
            return _monitorModelName.Equals(other._monitorModelName, StringComparison.OrdinalIgnoreCase);
        }

        //Robert_Lin, 2024-7-10, added for non-Monitor device can check monitor's model name
        public bool IsSameModel(string nameToCheck)
        {
            return _monitorModelName.Equals(nameToCheck, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        ///  Get the model from MonitorInfo.Capability string
        /// </summary>
        /// <param name="capabilityString">
        /// Example: "(prot(monitor)type(LCD)model(U2722DE)cmds(01 02 03 07 0C E3 F3)vcp(02 04 05 08 10 . . .
        /// </param>
        /// <returns>The model value, or String.Empty and fail to get</returns>
        private string GetModelFromMonitorCapabilityString(string capabilityString)
        {
            if (String.IsNullOrEmpty(capabilityString))
                return string.Empty;

            //Find the start index of "model("
            string signature = "model(";
            int idxSignature = capabilityString.IndexOf(signature);
            if (idxSignature < 0)
                return String.Empty;

            int idxModelStart = idxSignature + signature.Length;
            //Find the index of next ')' char
            int idxEnd = capabilityString.IndexOf(')', idxModelStart);
            if (idxEnd < 0)
                return String.Empty;

            int modelLen = idxEnd - idxModelStart;
            //Extract the sub string contains PIP/PBP capabilities
            string retString = capabilityString.Substring(idxModelStart, modelLen);
            return retString;
        }
        /// <summary>
        /// HomePlugin will check all devices in HomeDevices, if there are any device which is same model with others,
        /// HomePlugin will assign their order in InstanceNo. When InstanceNo is zero it means no same model device exist.
        /// </summary>
        private int _instanceNo = 0;
        public int InstanceNo
        {
            get => _instanceNo;
            set
            {
                SetProperty(ref _instanceNo, value);
                OnPropertyChanged("DisplayName");
            }
        }

        //Robert_Lin, 2024-8-6
        /// <summary>
        /// For Peripherals only. Check if specifies (other) device is the same model with this HomeDevice.
        /// It's used to asssign InstanceNo.
        /// Rule: If same Category AND DeviceInfo.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool IsSamePeripheralModel(HomeDevice other)
        {
            if (other.DeviceCategory != DeviceCategory)
                return false;
            if (DeviceInfo == null)
                return false;
            if (other.DeviceInfo == null)
                return false;

            if (DeviceInfo.ModelNumber.Equals(other.DeviceInfo.ModelNumber, StringComparison.OrdinalIgnoreCase))
                return true;

            return false;
        }
        #endregion DisplayName

        #region DetermineDeviceImage - Robert_Lin 2024-6-20 added
        private void DeterminePeripheralDeviceImage()
        {
            if (DeviceInfo == null)
                return;

            //Copy from PeripheralViewModel.cs

            string assemblyName = "DDPM.UI.Resources";
            string model = DeviceInfo.ModelNumber;
            //1 For Dock
            //0613 Bruce 目前IL的dock還沒有回型號回來，先固定使用同張產品圖，待更改。 0614新增註解紀錄
            if (model.ToUpper().Contains("DOCK"))
            {
                //ImageFilePath = $"/DDPM.UI.Resources;component/Resources/Images/WD22TB4.png";
                DeviceImage = DdpmCommonHelper.GetImageSourceFromCommonResource("Resources/Images/WD22TB4.png", assemblyName);
                return;
            }

            //ImageFilePath = $"/DDPM.UI.Resources;component/Resources/Images/{Model}.png";
            DeviceImage = DdpmCommonHelper.GetImageSourceFromCommonResource($"Resources/Images/{model}.png", assemblyName);
        }

        #endregion DetermineDeviceImage - Robert_Lin 2024-6-20 added

        #region Module Data - Robert_Lin, 2024-6-23 added
        private Dictionary<string, ObservableObject> _moduleData = new Dictionary<string, ObservableObject>();

        public EzArrangeViewModel vmEzArrange { get; set; }
        #endregion Module Data - Robert_Lin, 2024-6-23 added

        #region Sort and Grouping

        /// <summary>
        /// Implementation of IComparable. The basic sort, not considering Grouping.
        /// 1. Sort by DeviceCategory (see DDPM.UI.Common/Enum.cs eDeviceCategory
        /// 2. If both are the same DeviceCategory, then sort by DisplayName
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public int CompareTo(HomeDevice? other)
        {
            int ret = 0;
            if (other != null)
            {
                //If both are the same Category
                if (DeviceCategory == other.DeviceCategory)
                {
                    //Sort by DisplayName
                    ret = DisplayName.CompareTo(other.DisplayName);
                }
                else //different DeviceCategory
                {
                    //Sort by DeviceCategory
                    ret = DeviceCategory.CompareTo(other.DeviceCategory);
                }
            }
            return ret;
        }
        /// <summary>
        /// Robert_Lin, 2024-7-9 added, for Wencam and Monitor grouping
        /// For Monitor,
        ///    SortOrder = eDeviceCategory.Display(=0) + Index*10
        /// For Webcam,
        ///     If it's integrated with Monitor {
        ///        Index = Integrated Monitor's Index
        ///        SortOrder = Integrated Monitor's SortOrder + Webcam Index
        ///     Else
        ///        SortOrder = eDeviceCategory.Webcam(=1000) + Webcam Index
        /// Others
        /// </summary>
        public int SortOrder { get; set; } = 0;

        /// <summary>
        /// MonitorIndex (MonitorInfo.Index) which is integrated with this peripheral
        /// -1 means that no integrated monitor.
        /// </summary>
        public int MonitorIndexOfIntegratedPeripheral = -1;
        #endregion Sort and Grouping

        #region For HomeDevice selection changed, update to modules, Robert_Lin, 2024-7-4 added
        //Add DDPM.SA.Common interfaces, will be set by DisplayP
        public static IDeviceManagerSA DeviceManagerSA { get; set; }

        #endregion For HomeDevice selection changed, update to modules, Robert_Lin, 2024-7-4 added

        #region Connection Hover View
        private bool _isConnectionHoverViewShow = false;
        public bool IsConnectionHoverViewShow
        {
            get => _isConnectionHoverViewShow;
            set
            {
                if (value == true)
                {
                    if (DeviceInfo != null)
                    {
                        if (ConnectionType == "Dongle")
                        {
                            ConnectionHoverMode = "IO_Dongle";
                            RefreshDongleView();
                        }
                        else if (ConnectionType.Contains("Bluetooth")) //Audio will be "BluetoothAudio"
                        {
                            ConnectionHoverMode = "IO_BLE";
                            if (DeviceCategory == eDeviceCategory.KB)
                                SetBLConnectionStatus_Keyboard();
                            else if (DeviceCategory == eDeviceCategory.Mouse)
                                SetBLConnectionStatus_Mouse();
                            else if (DeviceCategory == eDeviceCategory.Headset)
                            {
                                ConnectionHoverMode = "Audio_BLE";
                                SetBLConnectionStatus_Audio();
                            }
                            else
                                SetBLConnectionStatus_IO();
                        }
                    }
                }
                SetProperty(ref _isConnectionHoverViewShow, value);
                RefreshConnectoionHoverView();
            }
        }

        private void RefreshConnectoionHoverView()
        {
            OnPropertyChanged("DongleHostText");
            OnPropertyChanged("DongleFwVersion");
        }

        private string _connectionHoverMode = "";
        /// <summary>
        /// "IO_BLE", "IO_Dongle"
        /// </summary>
        public string ConnectionHoverMode
        {
            get => _connectionHoverMode;
            set => SetProperty(ref _connectionHoverMode, value);
        }

        // Donlge
        //
        private void RefreshDongleView()
        {
            if (DeviceInfo == null)
                return;

            //DongleFwVersion = "Receiver Firmware Version " {PhysicalDeviceFWVersion}
            string fv = DeviceInfo.PhysicalDeviceFirmwareVersion.PadLeft(4, '0');
            string PhysicalDeviceFWVersion = $"{fv.Substring(0, 1)}.{fv.Substring(1, 1)}.{fv.Substring(2, 1)}.{fv.Substring(3, 1)}";
            DongleFwVersion = $"{Strings.ReceiverFirmwareVersion} {PhysicalDeviceFWVersion}";

            //DongleSlot = "4 of 6 slots available"
            DongleSlot = $"{DeviceInfo.MaxPairingSlots - DeviceInfo.PairedDeviceCount} of {DeviceInfo.MaxPairingSlots} slots available";
        }
        /// <summary>
        /// "USB Wireless Receiver"
        /// </summary>
        public string DongleHostText
        {
            get
            {
                return Strings.USBWirelessReceiver;
            }
        }

        /// <summary>
        /// "Receiver Firmware Version" "x.x.x.x"{}
        /// </summary>
        private string _dongleFwVersion;
        public string DongleFwVersion
        {
            get => _dongleFwVersion;
            set => SetProperty(ref _dongleFwVersion, value);
        }
        private string _dongleSlot;
        public string DongleSlot
        {
            get => _dongleSlot;
            set => SetProperty(ref _dongleSlot, value);
        }

        // BLE
        //
        // {txt} PimgBL} {txtBLHost}          {IsBleHostVisible}
        // 1     {icon}  Window Machine 1      True/False     Color: ConnectionStyle=1|2|3

        private void SetBLConnectionStatus_Mouse()
        {
            if (DeviceInfo == null) return;

            //Determine current connected host index: 1,2, or 3
            var hostIndex = DeviceInfo.VisiblePairedHostName1.ToUpper() == "VISIBLE" ? 1 : (DeviceInfo.VisiblePairedHostName2.ToUpper() == "VISIBLE" ? 2 : 3);

            BleHost1Style = "2";
            //txt1.Style = ConnectionStyle2;
            //imgBL1.Source = img2;
            //txtBLHost1.Style = ConnectionStyle2;

            BleHost2Style = "2";
            //txt2.Style = ConnectionStyle2;
            //imgBL2.Source = img2;
            //txtBLHost2.Style = ConnectionStyle2;

            BleHost3Style = "2";
            //txt3.Style = ConnectionStyle2;
            //imgBL3.Source = img2;
            //txtBLHost3.Style = ConnectionStyle2;

            BleHost1Text = string.IsNullOrEmpty(DeviceInfo.PairedHostName1) ? Strings.ReadyToBePaired : DeviceInfo.PairedHostName1;
            BleHost2Text = string.IsNullOrEmpty(DeviceInfo.PairedHostName2) ? Strings.ReadyToBePaired : DeviceInfo.PairedHostName2;
            BleHost3Text = string.IsNullOrEmpty(DeviceInfo.PairedHostName3) ? Strings.ReadyToBePaired : DeviceInfo.PairedHostName3;
            //txtBLHost1.Text = string.IsNullOrEmpty(_vm.PairedHostName1) ? Strings.ReadyToBePaired : _vm.PairedHostName1;
            //txtBLHost2.Text = string.IsNullOrEmpty(_vm.PairedHostName2) ? Strings.ReadyToBePaired : _vm.PairedHostName2;
            //txtBLHost3.Text = string.IsNullOrEmpty(_vm.PairedHostName3) ? Strings.ReadyToBePaired : _vm.PairedHostName3;

            switch (DeviceInfo.ModelNumber)
            {
                case "MS700":
                    //BleHost3Style = "1";
                    //txt3.Visibility = Visibility.Visible;
                    //Host3.Visibility = Visibility.Visible;
                    if (hostIndex == 1)
                    {
                        BleHost1Style = "1";
                        //txt1.Style = ConnectionStyle1;
                        //imgBL1.Source = img1;
                        //txtBLHost1.Style = ConnectionStyle1;
                    }
                    else if (hostIndex == 2)
                    {
                        BleHost2Style = "1";
                        //txt2.Style = ConnectionStyle1;
                        //imgBL2.Source = img1;
                        //txtBLHost2.Style = ConnectionStyle1;
                    }
                    else
                    {
                        BleHost3Style = "1";
                        //txt3.Style = ConnectionStyle1;
                        //imgBL3.Source = img1;
                        //txtBLHost3.Style = ConnectionStyle1;
                    }
                    break;

                case "MS5320W":
                case "MS7421W":
                    BleHost1Style = "0";
                    //Host1.Visibility = Visibility.Collapsed;

                    if (hostIndex == 2)
                    {
                        BleHost2Style = "1";
                        //txt2.Style = ConnectionStyle1;
                        //imgBL2.Source = img1;
                        //txtBLHost2.Style = ConnectionStyle1;
                    }
                    else
                    {
                        BleHost3Style = "1";
                        //txt3.Style = ConnectionStyle1;
                        //imgBL3.Source = img1;
                        //txtBLHost3.Style = ConnectionStyle1;
                    }
                    break;

                case "MS900":
                    BleHost3Style = "0";
                    //Host3.Visibility = Visibility.Collapsed;

                    if (hostIndex == 1)
                    {
                        BleHost1Style = "1";
                        //txt1.Style = ConnectionStyle1;
                        //imgBL1.Source = img1;
                        //txtBLHost1.Style = ConnectionStyle1;
                    }
                    else
                    {
                        BleHost2Style = "1";
                        //txt2.Style = ConnectionStyle1;
                        //imgBL2.Source = img1;
                        //txtBLHost2.Style = ConnectionStyle1;
                    }
                    break;

                default:
                    BleHost1Style = "0";
                    BleHost3Style = "0";
                    //Host1.Visibility = Visibility.Collapsed;
                    //Host3.Visibility = Visibility.Collapsed;
                    BleHost2Style = "1";
                    //txt2.Style = ConnectionStyle1;
                    //imgBL2.Source = img1;
                    //txtBLHost2.Style = ConnectionStyle1;
                    break;
            }
        }

        private void SetBLConnectionStatus_Keyboard()
        {
            if (DeviceInfo == null) return;

            var hostIndex = DeviceInfo.VisiblePairedHostName1.ToUpper() == "VISIBLE" ? 1 : (DeviceInfo.VisiblePairedHostName2.ToUpper() == "VISIBLE" ? 2 : 3);

            BleHost1Style = "2";
            //txt1.Style = ConnectionStyle2;
            //imgBL1.Source = img2;
            //txtBLHost1.Style = ConnectionStyle2;

            BleHost2Style = "2";
            //txt2.Style = ConnectionStyle2;
            //imgBL2.Source = img2;
            //txtBLHost2.Style = ConnectionStyle2;

            BleHost3Style = "0";

            switch (DeviceInfo.ModelNumber)
            {
                case "KB700":
                case "KB740":
                    BleHost1Text = string.IsNullOrEmpty(DeviceInfo.PairedHostName2) ? Strings.ReadyToBePaired : DeviceInfo.PairedHostName1;
                    BleHost2Text = string.IsNullOrEmpty(DeviceInfo.PairedHostName3) ? Strings.ReadyToBePaired : DeviceInfo.PairedHostName2;
                    //txtBLHost1.Text = string.IsNullOrEmpty(_vm.PairedHostName2) ? Strings.ReadyToBePaired : _vm.PairedHostName2;
                    //txtBLHost2.Text = string.IsNullOrEmpty(_vm.PairedHostName3) ? Strings.ReadyToBePaired : _vm.PairedHostName3;
                    if (hostIndex == 2)
                    {
                        BleHost1Style = "1";
                        //txt1.Style = ConnectionStyle1;
                        //imgBL1.Source = img1;
                        //txtBLHost1.Style = ConnectionStyle1;
                    }
                    else
                    {
                        BleHost2Style = "1";
                        //txt2.Style = ConnectionStyle1;
                        //imgBL2.Source = img1;
                        //txtBLHost2.Style = ConnectionStyle1;
                    }
                    break;

                case "KB900":
                    BleHost1Text = string.IsNullOrEmpty(DeviceInfo.PairedHostName1) ? Strings.ReadyToBePaired : DeviceInfo.PairedHostName1;
                    BleHost2Text = string.IsNullOrEmpty(DeviceInfo.PairedHostName2) ? Strings.ReadyToBePaired : DeviceInfo.PairedHostName2;
                    //txtBLHost1.Text = string.IsNullOrEmpty(_vm.PairedHostName1) ? Strings.ReadyToBePaired : _vm.PairedHostName1;
                    //txtBLHost2.Text = string.IsNullOrEmpty(_vm.PairedHostName2) ? Strings.ReadyToBePaired : _vm.PairedHostName2;
                    if (hostIndex == 1)
                    {
                        BleHost1Style = "1";
                        //txt1.Style = ConnectionStyle1;
                        //imgBL1.Source = img1;
                        //txtBLHost1.Style = ConnectionStyle1;
                    }
                    else
                    {
                        BleHost2Style = "1";
                        //txt2.Style = ConnectionStyle1;
                        //imgBL2.Source = img1;
                        //txtBLHost2.Style = ConnectionStyle1;
                    }
                    break;

                default:
                    BleHost1Text = DeviceInfo.PairedHostName2;
                    //txtBLHost1.Text = _vm.PairedHostName2;

                    BleHost1Style = "1";
                    //txt1.Style = ConnectionStyle1;
                    //imgBL1.Source = img1;
                    //txtBLHost1.Style = ConnectionStyle1;

                    BleHost2Style = "0";
                    //Host2.Visibility = Visibility.Collapsed;
                    break;
            }
        }

        //Need to fix: How to know Audio BLE have 1 or 2 slots?
        //Currently, we will show only one host.
        private void SetBLConnectionStatus_Audio()
        {
            if (DeviceInfo == null) return;

            string hostName = Dns.GetHostName();

            if (DeviceInfo.PairedHostName1 == hostName)
            //if (_vm.VisiblePairedHostName1 == hostName)
            {
                BleHost1Style = "1";
                BleHost2Style = "0"; //Fix, Audio BLE has no gray color state
                                     //txt1.Style = ConnectionStyle1;
                                     //txt2.Style = ConnectionStyle2;
                                     //imgBL1.Source = img1;
                                     //imgBL2.Source = img2;
                                     //txtSystemName1.Style = ConnectionStyle1;
                                     //txtSystemName2.Style = ConnectionStyle2;

                //Workaround, if Hostname is empty, then show {hostName}
                BleHost1Text = string.IsNullOrEmpty(DeviceInfo.PairedHostName1) ? hostName : DeviceInfo.PairedHostName1;
            }
            else
            {
                BleHost1Style = "0";
                BleHost2Style = "1"; //Fix, Audio BLE has no gray color state
                //txt1.Style = ConnectionStyle2;
                //txt2.Style = ConnectionStyle1;
                //imgBL1.Source = img2;
                //imgBL2.Source = img1;
                //txtSystemName1.Style = ConnectionStyle2;
                //txtSystemName2.Style = ConnectionStyle1;

                //Workaround, if Hostname is empty, then show {hostName}
                BleHost2Text = string.IsNullOrEmpty(DeviceInfo.PairedHostName2) ? hostName : DeviceInfo.PairedHostName2;
            }
            string AudioBleConnectionTotalPairCountText = "This device can be paired with {0} hosts simultaneously";
            int totalPairedHostCount = DeviceInfo.TotalNumberOfPairedHostName;
            AudioBleText = String.Format(AudioBleConnectionTotalPairCountText, totalPairedHostCount);
            //BLConnection.Visibility = Visibility.Visible;
        }

        private void SetBLConnectionStatus_IO()
        {
            if (DeviceInfo == null) return;

            string hostName = Dns.GetHostName();

            if (DeviceInfo.PairedHostName1 == hostName)
            //if (_vm.VisiblePairedHostName1 == hostName)
            {
                BleHost1Style = "1";
                BleHost2Style = "2";
                //txt1.Style = ConnectionStyle1;
                //txt2.Style = ConnectionStyle2;
                //imgBL1.Source = img1;
                //imgBL2.Source = img2;
                //txtSystemName1.Style = ConnectionStyle1;
                //txtSystemName2.Style = ConnectionStyle2;
            }
            else
            {
                BleHost1Style = "2";
                BleHost2Style = "1";
                //txt1.Style = ConnectionStyle2;
                //txt2.Style = ConnectionStyle1;
                //imgBL1.Source = img2;
                //imgBL2.Source = img1;
                //txtSystemName1.Style = ConnectionStyle2;
                //txtSystemName2.Style = ConnectionStyle1;
            }
            //BLConnection.Visibility = Visibility.Visible;
        }
        //BLE Host Style:
        // 0=Collapse; 1=White; 2=Gray
        private string _bleHost1Style;
        public string BleHost1Style
        {
            get => _bleHost1Style;
            set => SetProperty(ref _bleHost1Style, value);
        }

        private string _bleHost2Style;
        public string BleHost2Style
        {
            get => _bleHost2Style;
            set => SetProperty(ref _bleHost2Style, value);
        }

        private string _bleHost3Style;
        public string BleHost3Style
        {
            get => _bleHost3Style;
            set => SetProperty(ref _bleHost3Style, value);
        }

        // BLE Host Text = Window Machine Name or "Ready to be paired"
        private string _bleHost1Text;
        public string BleHost1Text
        {
            get => _bleHost1Text;
            set => SetProperty(ref _bleHost1Text, value);
        }
        private string _bleHost2Text;
        public string BleHost2Text
        {
            get => _bleHost2Text;
            set => SetProperty(ref _bleHost2Text, value);
        }
        private string _bleHost3Text;
        public string BleHost3Text
        {
            get => _bleHost3Text;
            set => SetProperty(ref _bleHost3Text, value);
        }

        private string _audioBleText;
        public string AudioBleText
        {
            get => _audioBleText;
            set => SetProperty(ref _audioBleText, value);
        }
        #endregion Connection Hover View

        #region Peripherals Helper
        //Copy from PeripheralViewModel

        /// <summary>
        /// Return the ConnectionType from DeviceInfo
        /// It may need to update when DPeM SDK changed.
        /// </summary>
        /// <param name="deviceInfo"></param>
        /// <returns></returns>
        public static string GetConnectionTypeFromDeviceInfo(DeviceInfo deviceInfo)
        {
            //Original code from PeripheralViewModel.SetCurrentDevice()
            /*
            //ConnectionType = CurrentDeviceInfo.PhysicalDeviceType.ToString() == "PhysicalDongle" ? "Dongle" : "Bluetooth";
            switch (CurrentDeviceInfo.PhysicalDeviceType)
            {
                case DeviceType.PhysicalAudioDongle:
                case DeviceType.PhysicalBluetoothAudio:
                    ConnectionType = CurrentDeviceInfo.PhysicalDeviceType.ToString().Replace("Physical", "").Replace("Audio", "");
                    break;

                case DeviceType.PhysicalBluetooth:
                case DeviceType.PhysicalDongle:
                case DeviceType.PhysicalPen:
                    ConnectionType = CurrentDeviceInfo.PhysicalDeviceType.ToString().Replace("Physical", "");
                    break;
                //0617 Bruce 新增Dock連線方式的濾字串的方式
                case DeviceType.PhysicalWiredDock:
                    ConnectionType = CurrentDeviceInfo.PhysicalDeviceType.ToString().Replace("Physical", "").Replace("Dock", "");
                    break;

                default:
                    ConnectionType = CurrentDeviceInfo.PhysicalDeviceType.ToString().Replace("Physical", "");
                    break;
            }
            */
            switch (deviceInfo.PhysicalDeviceType)
            {
                case DeviceType.PhysicalAudioDongle:
                case DeviceType.PhysicalBluetoothAudio:
                    return deviceInfo.PhysicalDeviceType.ToString().Replace("Physical", "").Replace("Audio", "");

                case DeviceType.PhysicalBluetooth:
                case DeviceType.PhysicalDongle:
                case DeviceType.PhysicalPen:
                    return deviceInfo.PhysicalDeviceType.ToString().Replace("Physical", "");

                //0617 Bruce 新增Dock連線方式的濾字串的方式
                case DeviceType.PhysicalWiredDock:
                    return deviceInfo.PhysicalDeviceType.ToString().Replace("Physical", "").Replace("Dock", "");

                default:
                    return deviceInfo.PhysicalDeviceType.ToString().Replace("Physical", "");
            }
        }

        public static eDeviceCategory GetDeviceCategoryFromDeviceInfo(DeviceInfo deviceInfo)
        {
            DeviceType devType = deviceInfo.Type;
            if (devType.ToString().Contains("Keyboard"))
                return eDeviceCategory.KB;
            else if (devType.ToString().Contains("Mouse"))
                return eDeviceCategory.Mouse;
            else if (devType.ToString().Contains("Webcam"))
                return eDeviceCategory.Webcam;
            else if (devType.ToString().ToUpper().Contains("DOCK") ||
                (devType.ToString().ToUpper().Contains("23")))
                return eDeviceCategory.Dock;
            else if (devType.ToString().ToUpper().Contains("HEADSET"))
                return eDeviceCategory.Headset;
            else if (devType.ToString().ToUpper().Contains("SOUNDBAR"))
                return eDeviceCategory.Soundbar;
            return eDeviceCategory.Unknown;
        }

        public static HomeDevice CreateFromDeviceInfo(DeviceInfo deviceInfo)
        {
            HomeDevice homeDev = new HomeDevice();
            homeDev.DeviceInfo = deviceInfo;
            homeDev.DeviceCategory = GetDeviceCategoryFromDeviceInfo(deviceInfo);
            return homeDev;
        }
        #endregion Peripherals Helper

        #region HasCapability_XXXX Properties
        //Capabilities will be get once and then store for used later
        private bool? _hasCapability_NetworkKvm = null;

        /// <summary>
        /// Return true if the HomeDevice has PIP/PBP capability.
        /// </summary>
        public bool HasCapability_PipPbp
        {
            get
            {
                if (MonitorInfo != null)
                {
                    if (MonitorInfo.CapabilityDic != null)
                        return MonitorInfo.CapabilityDic.ContainsKey("E9");
                }
                return false;
            }
        }

        /// <summary>
        /// Check if MonitorInfo.CapabilityDic contains "EE" key
        /// </summary>
        //
        //public Lazy<bool> HasCapability_KVM => new Lazy<bool>(() => DetermineKvmCapability());
        public bool HasCapability_KVM
        {
            get
            {
                return HasCapability_UsbKvm || HasCapability_NetworkKvm;
            }
        }

        //Unused
        private bool DetermineKvmCapability()
        {
            //Non-monitor => No capability
            if (MonitorInfo == null)
                return false;

            //No Capability String => No capability
            if (MonitorInfo.CapabilityDic == null)
                return false;

            //VCP contains "EE" => has USB KVM capability
            if (MonitorInfo.CapabilityDic.ContainsKey("EE"))
                return true;

            //Determine if it has Network KVM capability
            if (DeviceManagerSA != null)
            {
                bool isSupportNKvm = DeviceManagerSA.isNKVMSupportMonitor(MonitorInfo).Result;
                if (isSupportNKvm)
                    return true;
            }
            return false;
        }

        public bool HasCapability_UsbKvm
        {
            get
            {
                //Non-monitor => No capability
                if (MonitorInfo == null)
                    return false;

                //No Capability String => No capability
                if (MonitorInfo.CapabilityDic == null)
                    return false;

                //VCP contains "EE" => has USB KVM capability
                if (MonitorInfo.CapabilityDic.ContainsKey("EE"))
                {
                    return true;
                }
                return false;
            }
        }

        public bool HasCapability_NetworkKvm
        {
            get
            {
                if (_hasCapability_NetworkKvm != null)
                    return _hasCapability_NetworkKvm.Value;

                _hasCapability_NetworkKvm = false;

                //Non-monitor => No capability
                if (MonitorInfo == null)
                    return false;

                //No Capability String => No capability
                if (MonitorInfo.CapabilityDic == null)
                    return false;

                //Determine if it has Network KVM capability
                if (DeviceManagerSA != null)
                {
                    _hasCapability_NetworkKvm = DeviceManagerSA.isNKVMSupportMonitor(MonitorInfo).Result;
                    return _hasCapability_NetworkKvm.Value;
                }
                return false;
            }
        }

        public bool HasCapability_Gaming
        {
            get
            {
                if (MonitorInfo != null)
                {
                    if (MonitorInfo.CapabilityDic != null)
                        return MonitorInfo.CapabilityDic.ContainsKey("F4");
                }
                return false;
            }
        }

        public bool HasCapability_VisionEngine
        {
            get
            {
                if (MonitorInfo != null)
                {
                    if (MonitorInfo.CapabilityDic != null)
                        return MonitorInfo.CapabilityDic.ContainsKey("EC");
                }
                return false;
            }
        }

        //Robert_Lin, 2024-8-4 Copy from DisplayPage.xaml.cs BuildModuleGroups()
        public bool HasCapability_Contrast
        {
            get
            {
                if (MonitorInfo != null)
                {
                    if (MonitorInfo.CapabilityDic != null)
                        return MonitorInfo.CapabilityDic.ContainsKey("12");
                }
                return false;
            }
        }
        #endregion HasCapability_XXXX Properties

        #region Monitor Equals
        public static bool IsSameMonitor(MonitorInfo mi1, MonitorInfo mi2, string mask = "")
        {
            //Part I. Must be compared and must be the same
            if ((mi1.AliasDeviceName != mi2.AliasDeviceName) ||
                (mi1.IsDellMonitor != mi2.IsDellMonitor) ||
                (mi1.DisplayName != mi2.DisplayName) ||
                (mi1.Index != mi2.Index) ||
                (mi1.CapabilityString != mi2.CapabilityString) ||
                (mi1.FwVersion != mi2.FwVersion) ||
                (mi1.modelName != mi2.modelName) ||
                (mi1.series != mi2.series))
                return false;

            //Part II. Maskable
            if (!mask.Contains("DDCisON", StringComparison.OrdinalIgnoreCase))
            {
                if (mi1.DDCisON != mi2.DDCisON) return false;
            }
            if (!mask.Contains("inputSource", StringComparison.OrdinalIgnoreCase))
            {
                if (mi1.inputSource != mi2.inputSource) return false;
            }
            if (!mask.Contains("edid", StringComparison.OrdinalIgnoreCase))
            {
                if (!EqualityComparer<EDID>.Equals(mi1.edid, mi2.edid))
                    return false;
            }

            return true;
        }
        #endregion Monitor Equals
    }
}