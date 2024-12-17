using CommunityToolkit.Mvvm.ComponentModel;
using DDPM.SA.Common;
using DDPM.UI.Common.ViewModels;
using DDPM.UI.Resources.Helper;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.UX.WPF.Controls;
using DPeMPublic.Common.Enums;
using System.Drawing.Imaging;
using System.Globalization;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using VcpCore.Common;

namespace DDPM.UI.Common.Models
{
    //Robert_Lin, 2024-7-10, add IComparable<HomeDevice>
    public class HomeDevice : ObservableObject, IComparable<HomeDevice>
    {
        private eDeviceCategory _deviceCategory;
        private string _deviceName = string.Empty;
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
            ////Register a handler for BitmapImageUpdated for Theme changed. Will update the image resources
            //DdpmCommonHelper.BitmapImageUpdated += bitmapImageUpdate_OnThemeChanged;
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
                string model = string.IsNullOrWhiteSpace(_monitorInfo.modelName) ? GetModelFromMonitorCapabilityString(_monitorInfo.CapabilityString) : _monitorInfo.modelName;
                //If fail to get mode from CapabilityString, then use AliasDeviceName instead
                if (String.IsNullOrEmpty(model))
                {
                    _monitorModelName = _monitorInfo.AliasDeviceName;
                }
                else
                {
                    _monitorModelName = model;
                }

                //Robert_Lin, 2024-9-30 Add Monitor Product Images
                DetermineMonitorImage();

                //Robert_lin, 2024-11-14 add Pxp Capapbilies check support
                InitPipPbpCaps();

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

        /// <summary>
        /// The tooltip text which showing on Homepage, device list tooltip when in hover state.
        /// </summary>
        public string TooltipModelName
        {
            get
            {
                if (DeviceInfo != null)
                {
                    if (DeviceInfo.Name.ToUpper().Contains("WD19S"))
                    {
                        return DeviceInfo.Name.Replace("_", " ");
                    }
                    //Robert_Lin, 2024-11-22, [PIMS-316846], DeviceName is "MouseSettings" so it seems that should be
                    // Name="Dell Pro Premium Mouse" + ModelNumber="MS900" => "Dell Pro Premium Mouse MS900"
                    //Based on Indilogic reply:
                    //Gayathri Iyer1(INDILOGIC) added a comment - 11/Nov/24 10:37 AM
                    //Hess Cheng(WISTRON) DPeM core provides two properties: DeviceName and ModelNumber.Please combine both to show in the UI. 

                    //Robert_Lin, 2024-11-22, Only Keyboard/Mouse need to combine {Name}+{Model}
                    //Other peripheals will display {Name} only, because Indilogical has combine {Model} inside {Name}

                    if ((DeviceCategory == eDeviceCategory.Mouse) ||
                        (DeviceCategory == eDeviceCategory.KB))
                    {
                        string model = DeviceInfo.ModelNumber;
                        //[#PeripheralModelMap] This mapping table has a duplicate code in
                        //1 DdpmCommonHelpers.cs    DeterminePeripheralProductImageFileName()
                        //2 HomeDevices             TooltipModelName property
                        //3 PeripheralViewModel.cs  MappingModel()
                        //If you need to modify, please also modify them.
                        switch (DeviceInfo.ModelNumber)
                        {
                            //Keyboard
                            case "KB740":
                            case "KB7120W":
                                model = "KB740";
                                break;
                            case "KB500":
                            case "KB3121W":
                                model = "KB500";
                                break;
                            case "KB700":
                            case "KB7221W":
                                model = "KB700";
                                break;

                            //Mouse
                            case "MS300":
                            case "MS3121W":
                                model = "MS300";
                                break;

                            //Default
                            default:
                                //model = deviceInfo.ModelNumber;
                                break;
                        } //switch(deviceInfo.ModelNumber)

                        return DeviceInfo.Name + $" {model}";
                    }
                    else
                    {
                        return DeviceInfo.Name;
                    }
                    //Robert_Lin, 2024-11-20, [PIMS-316846] change the tooltip on homepage to DeviceName
                    //return DeviceInfo.DeviceName;
                    //return DeviceInfo.Name; 
                }
                else if (MonitorInfo != null)
                {
                    //Robert_Lin, 2024-8-29 comment out for DDPMW-2094
                    //Robert_Lin, 2024-6-20, change to DisplayName (with (instanceNo)
                    //return MonitorInfo.AliasDeviceName;
                    //return DisplayName;

                    //Robert_Lin, 2024-9-9, per Dell Villavicencio, Kathia added a comment - 06/Sep/24 5:18 AM
                    //Tooltip show "{MarketName} {InstanceNo}"
                    //
                    //Robert_Lin, 2024-8-29, for DDPMW-2094 Update DDPM 2.0 Display Frontend for NPI; Non-NPI TBD
                    //For NPI models, MonitorInfo.MarketName will provide the name to show
                    //Otherwise (Non-NPI), MonitorInfo.MarketName will be empty, will show DisplayName (Model + instanceNo)
                    if (String.IsNullOrWhiteSpace(MonitorInfo.MarketingName))
                        return DisplayName;
                    else
                    {
                        //If the InstanceNo is 0
                        if (InstanceNo == 0)
                            return MonitorInfo.MarketingName;
                        else
                            return MonitorInfo.MarketingName + $" ({InstanceNo})";
                    }
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
                    //Robert_Lin, 2024-11-20 The display text in BatteryIndicator changed to {inputCable}-{InputName}
                    //  or {DIsplayName) PIMS-302436
                    //IF InputName is empty                 => Show MonitorInput.inputCable
                    //IF InputName==MonitorInfo.inputCable => show Monitor.InputCable
                    //ELSE                                  => Show "{MonitorInfo.inputCable} - {InputName}"
                    if (String.IsNullOrWhiteSpace(InputName))
                    {
                        return MonitorInfo.inputCable;
                    }
                    else if (InputName.Equals(MonitorInfo.inputCable))
                    {
                        return MonitorInfo.inputCable;
                    }
                    else
                    {
                        return MonitorInfo.inputCable + " - " + InputName;
                    }

                    //Robert_Lin, 2024-10-15 Change the Text1 of BatteryIndicator to inputCable.
                    //The inputCable has been remove unwant - and number, so we should show it directly
                    string strOut = MonitorInfo.inputCable;

                    ////2024-5-24 Robert_Lin, remove the tail number and dash
                    //// "USB-C1" => "USB-C"; "HDMI-1" => "HDMI"
                    ////Rule:
                    //// 1 If tail char is number => remove it
                    //// 2 If tail char is '-' => remove it
                    //string strOut = MonitorInfo.inputSource;
                    //char[] digits = new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '-' };
                    //strOut = strOut.TrimEnd(digits);
                    //strOut = strOut.TrimEnd(digits);
                    //strOut = strOut.TrimEnd(digits);
                    //return strOut;
                }
                return "";
            }
            //set => _text1 = value;
        }

        //Robert_Lin, 2024-11-20 PIMS-302436, need to show user's input name on BatteryIndicatior
        //Add a new property to stroe the user input name
        public string InputName { get; set; } = "";

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
                        //Robert_Lin, 2024-8-29
                        //Hess has added property 'Model2' to show ModelNumber + (InstanceNo).
                        return DeviceInfo.ModelNumber;
                        //return DeviceInfo.Model2;
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
            //Robert_Lin, 2024-11-16, PIMS-295748, "MS300" no image at homepage
            //move this code to DDPM.UI.Common/DdpmCommonHelper
            string imageFileName = DdpmCommonHelper.DeterminePeripheralProductImageFileName(DeviceInfo);
            //If fail to get the image will show the info, so we can easily to see the information
            if (!String.IsNullOrEmpty(imageFileName))
            {
                string assemblyName = "DDPM.UI.Resources";
                DeviceImage = DdpmCommonHelper.GetImageSourceFromCommonResource($"Resources/Images/{imageFileName}.png", assemblyName);
            }
        }

        /// <summary>
        /// Called in MonitorInfo setter, will output to HomeDevice.DeviceImage
        /// </summary>
        private void DetermineMonitorImage()
        {
            if (MonitorInfo == null)
                return;

            bool isLineArt = true;
            string imageFileName = "Lineart";
            string assemblyName = "DDPM.UI.Resources";

            //Determine filename
            //1 If no ImageFileName provided => Show line art
            //2 Not empty, use the filename provided
            if (!String.IsNullOrWhiteSpace(MonitorInfo.ImageFileName))
            {
                //The filename will come from MonitorInfo.ImageFileName
                imageFileName = MonitorInfo.ImageFileName;
                //The ImageFileName will not have extention file name
                //(for example, ImageFileName="U4323QE"), we need to append ".PNG"

                //Robert_Lin, 2024-11-26 for LightMode Lineart.png 
                //If the ImageFileName is NOT "LineArt" 
                if (!imageFileName.Equals("LINEART", StringComparison.OrdinalIgnoreCase))
                {
                    isLineArt = false;
                }
            }
            if (isLineArt)
            {
                OSThemeEnum oSTheme = UXSystemParameters.Instance.OSTheme;
                if (oSTheme == OSThemeEnum.Light)
                {
                    //LightMode: change filename to "LineArt-w"
                    imageFileName = "Lineart-w";
                }
                //It's "Lineart", we need to change image when theme changed
                DdpmCommonHelper.BitmapImageUpdated += bitmapImageUpdated_RefreshLineArt;
            }

            //Load the image
            //Try to load image from DDPM.UI.Resources project (assembly), Path="/Resources/Monitor/"
            ImageSource? imgSource = DdpmCommonHelper.GetImageSourceFromCommonResource($"Resources/Monitors/{imageFileName}.png", assemblyName);
            if (imgSource != null)
            {
                if (isLineArt)
                    DeviceImage = (BitmapImage)System.Windows.Application.Current.Resources["MonitorImage_LineArt"];
                else
                    DeviceImage = imgSource;
                return;
            }
        }

        private void bitmapImageUpdated_RefreshLineArt(OSThemeEnum obj)
        {
            //if (obj == OSThemeEnum.Dark)
            //{
            //    DdpmCommonHelper.UpdateBitmapImage("MonitorImage_LineArt", new Uri($"pack://application:,,,/DDPM.UI.Resources;component/Resources/Monitors/Lineart.png", UriKind.RelativeOrAbsolute));
            //}
            //else
            //{
            //    DdpmCommonHelper.UpdateBitmapImage("MonitorImage_LineArt", new Uri($"pack://application:,,,/DDPM.UI.Resources;component/Resources/Monitors/Lineart-w.png", UriKind.RelativeOrAbsolute));
            //}
            //Release original resource
            DeviceImage = null;
            //Allocate (refresh) new one
            DeviceImage = (ImageSource?)System.Windows.Application.Current.Resources["MonitorImage_LineArt"];
        }

        //private void bitmapImageUpdate_OnThemeChanged(OSThemeEnum oSThemeEnum)
        //{
        //    //Reserved for future usage
        //}

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
                        /* 1101 Bruce The customer confirms that the Dock does not use the Hover icon.
                        if (DeviceCategory == eDeviceCategory.Dock)
                        {
                            ConnectionHoverMode = "Dock";
                            SetDockView();
                        }
                        else*/
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
        private void SetDockView()
        {
            if (DeviceInfo == null)
                return;
            var fv = Regex.Replace(DeviceInfo.DockPackageFwVersion, @"(\d{2})(?=\d)", "$1.");
            Dock_FirmwareVersion = $"Dock {Strings.FirmwareVersion} {fv}";
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
        private const int maxHostNameLength = 15;
        private void SetBLConnectionStatus_Mouse()
        {
            try
            {
                if (DeviceInfo == null)
                    return;

                string hostName = Dns.GetHostName();
                if (hostName.Length > 15)
                    hostName = hostName.Substring(0, 15);

                //Determine current connected host index: 1,2, or 3
                var hostIndex = DeviceInfo.VisiblePairedHostName1.ToUpper() == "VISIBLE" ? 1 : (DeviceInfo.VisiblePairedHostName2.ToUpper() == "VISIBLE" ? 2 : 3);

                //Set default styles are "2" (gray)
                BleHost1Style = "2";
                BleHost2Style = "2";
                BleHost3Style = "2";

                BleHost1Text = "";
                BleHost2Text = "";
                BleHost3Text = "";
                //txtBLHost1.Text = string.IsNullOrEmpty(_vm.PairedHostName1) ? Strings.ReadyToBePaired : _vm.PairedHostName1;
                //txtBLHost2.Text = string.IsNullOrEmpty(_vm.PairedHostName2) ? Strings.ReadyToBePaired : _vm.PairedHostName2;
                //txtBLHost3.Text = string.IsNullOrEmpty(_vm.PairedHostName3) ? Strings.ReadyToBePaired : _vm.PairedHostName3;

                switch (DeviceInfo.ModelNumber)
                {
                    case "MS700":
                        BleHost1Text = string.IsNullOrEmpty(DeviceInfo.PairedHostName1) ? Strings.ReadyToBePaired : DeviceInfo.PairedHostName1;
                        BleHost2Text = string.IsNullOrEmpty(DeviceInfo.PairedHostName2) ? Strings.ReadyToBePaired : DeviceInfo.PairedHostName2;
                        BleHost3Text = string.IsNullOrEmpty(DeviceInfo.PairedHostName3) ? Strings.ReadyToBePaired : DeviceInfo.PairedHostName3;
                        //BleHost3Style = "1";
                        //txt3.Visibility = Visibility.Visible;
                        //Host3.Visibility = Visibility.Visible;
                        if (BleHost1Text.Equals(hostName, StringComparison.CurrentCultureIgnoreCase))
                        {
                            BleHost1Style = "1";
                            BleHost1Text = BleHost1Text.Substring(0, maxHostNameLength);
                            //txt1.Style = ConnectionStyle1;
                            //imgBL1.Source = img1;
                            //txtBLHost1.Style = ConnectionStyle1;
                        }
                        else if (BleHost2Text.Equals(hostName, StringComparison.CurrentCultureIgnoreCase))
                        {
                            BleHost2Style = "1";
                            BleHost2Text = BleHost2Text.Substring(0, maxHostNameLength);
                            //txt2.Style = ConnectionStyle1;
                            //imgBL2.Source = img1;
                            //txtBLHost2.Style = ConnectionStyle1;
                        }
                        else
                        {
                            BleHost3Style = "1";
                            BleHost3Text = BleHost3Text.Substring(0, maxHostNameLength);
                            //txt3.Style = ConnectionStyle1;
                            //imgBL3.Source = img1;
                            //txtBLHost3.Style = ConnectionStyle1;
                        }
                        break;

                    case "MS5320W":
                    case "MS7421W":
                        //Host1 unused
                        BleHost1Style = "0";

                        //Host2 data from PairedHostName1
                        //Host3 data from PairedHostName2
                        BleHost2Text = string.IsNullOrEmpty(DeviceInfo.PairedHostName1) ? Strings.ReadyToBePaired : DeviceInfo.PairedHostName1;
                        BleHost3Text = string.IsNullOrEmpty(DeviceInfo.PairedHostName2) ? Strings.ReadyToBePaired : DeviceInfo.PairedHostName2;

                        if (BleHost2Text.Equals(hostName, StringComparison.CurrentCultureIgnoreCase))
                        {
                            BleHost2Style = "1";
                            BleHost2Text = BleHost2Text.Substring(0, maxHostNameLength);
                            //imgBL2.Source = img1;
                            //txtBLHost2.Style = ConnectionStyle1;
                        }
                        else
                        {
                            BleHost3Style = "1";
                            BleHost3Text = BleHost3Text.Substring(0, maxHostNameLength);
                            //imgBL3.Source = img1;
                            //txtBLHost3.Style = ConnectionStyle1;
                        }
                        break;

                    case "MS900":
                        BleHost3Style = "0";
                        BleHost1Text = string.IsNullOrEmpty(DeviceInfo.PairedHostName1) ? Strings.ReadyToBePaired : DeviceInfo.PairedHostName1;
                        BleHost2Text = string.IsNullOrEmpty(DeviceInfo.PairedHostName2) ? Strings.ReadyToBePaired : DeviceInfo.PairedHostName2;
                        //Host3.Visibility = Visibility.Collapsed;

                        if (BleHost1Text.Equals(hostName, StringComparison.CurrentCultureIgnoreCase))
                        {
                            BleHost1Style = "1";
                            BleHost1Text = BleHost1Text.Substring(0, maxHostNameLength);
                            //imgBL1.Source = img1;
                            //txtBLHost1.Style = ConnectionStyle1;
                        }
                        else
                        {
                            BleHost2Style = "1";
                            BleHost2Text = BleHost2Text.Substring(0, maxHostNameLength);
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
                        BleHost2Text = hostName.Substring(0, maxHostNameLength);
                        //txt2.Style = ConnectionStyle1;
                        //imgBL2.Source = img1;
                        //txtBLHost2.Style = ConnectionStyle1;
                        break;
                }
                //Trim string length to <= 15
                //if (BleHost1Text.Length > 15)
                //    BleHost1Text = BleHost1Text.Substring(0, 15);
                //if (BleHost2Text.Length > 15)
                //    BleHost2Text = BleHost2Text.Substring(0, 15);
                //if (BleHost3Text.Length > 15)
                //    BleHost3Text = BleHost3Text.Substring(0, 15);
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog($"[HomeDevice] SetBLConnectionStatus_Mouse Exception : {ex.Message}");
            }
        }

        private void SetBLConnectionStatus_Keyboard()
        {
            if (DeviceInfo == null)
                return;

            //Hess code 2024-11-5
            string hostName = Dns.GetHostName();
            if (hostName.Length > 15)
                hostName = hostName.Substring(0, 15);

            //txt1.Style = ConnectionStyle2;
            //imgBL1.Source = img2;
            //txtBLHost1.Style = ConnectionStyle2;
            //txt2.Style = ConnectionStyle2;
            //imgBL2.Source = img2;
            //txtBLHost2.Style = ConnectionStyle2;
            //txt3.Style = ConnectionStyle2;
            //imgBL3.Source = img2;
            //txtBLHost3.Style = ConnectionStyle2;

            BleHost1Text = "";
            BleHost2Text = "";
            BleHost3Text = "";

            //var hostIndex = DeviceInfo.VisiblePairedHostName1.ToUpper() == "VISIBLE" ? 1 : (DeviceInfo.VisiblePairedHostName2.ToUpper() == "VISIBLE" ? 2 : 3);
            //Set all default style 2 (Gray)
            BleHost1Style = "2";
            //txt1.Style = ConnectionStyle2;
            //imgBL1.Source = img2;
            //txtBLHost1.Style = ConnectionStyle2;

            BleHost2Style = "2";
            //txt2.Style = ConnectionStyle2;
            //imgBL2.Source = img2;
            //txtBLHost2.Style = ConnectionStyle2;

            BleHost3Style = "2";

            switch (DeviceInfo.ModelNumber)
            {
                case "KB700":
                case "KB740":
                case "KB7120W":
                case "KB7221W":
                    //Hess code, 2024-11-2
                    //Host1.Visibility = Visibility.Collapsed;
                    //txtBLHost2.Text = string.IsNullOrEmpty(_vm.PairedHostName1) ? Strings.ReadyToBePaired : _vm.PairedHostName1;
                    //txtBLHost3.Text = string.IsNullOrEmpty(_vm.PairedHostName2) ? Strings.ReadyToBePaired : _vm.PairedHostName2;
                    //if (txtBLHost2.Text.Equals(hostName, StringComparison.CurrentCultureIgnoreCase))
                    //{
                    //    txt2.Style = ConnectionStyle1;
                    //    imgBL2.Source = img1;
                    //    txtBLHost2.Style = ConnectionStyle1;
                    //}
                    //else
                    //{
                    //    txt3.Style = ConnectionStyle1;
                    //    imgBL3.Source = img1;
                    //    txtBLHost3.Style = ConnectionStyle1;
                    //}
                    //break;

                    //There modles has no host1
                    BleHost1Style = "0";
                    //BleHost1Text = string.IsNullOrEmpty(DeviceInfo.PairedHostName2) ? Strings.ReadyToBePaired : DeviceInfo.PairedHostName1;
                    //Host2 will show paired #1
                    BleHost2Text = string.IsNullOrEmpty(DeviceInfo.PairedHostName1) ? Strings.ReadyToBePaired : DeviceInfo.PairedHostName1;
                    //Host3 will show paired #2
                    BleHost3Text = string.IsNullOrEmpty(DeviceInfo.PairedHostName2) ? Strings.ReadyToBePaired : DeviceInfo.PairedHostName2;
                    //txtBLHost1.Text = string.IsNullOrEmpty(_vm.PairedHostName2) ? Strings.ReadyToBePaired : _vm.PairedHostName2;
                    //txtBLHost2.Text = string.IsNullOrEmpty(_vm.PairedHostName3) ? Strings.ReadyToBePaired : _vm.PairedHostName3;
                    //If host2 is current host (computer name)
                    if (BleHost2Text.Equals(hostName, StringComparison.CurrentCultureIgnoreCase))
                    //if (hostIndex == 2)
                    {
                        //Then show style 1 (white)
                        BleHost2Style = "1";
                        BleHost2Text = BleHost2Text.Substring(0, maxHostNameLength);
                        //txt1.Style = ConnectionStyle1;
                        //imgBL1.Source = img1;
                        //txtBLHost1.Style = ConnectionStyle1;
                    }
                    else
                    {
                        //Else host3 is current host
                        BleHost3Style = "1";
                        BleHost3Text = BleHost3Text.Substring(0, maxHostNameLength);
                        //txt2.Style = ConnectionStyle1;
                        //imgBL2.Source = img1;
                        //txtBLHost2.Style = ConnectionStyle1;
                    }
                    break;

                case "KB900":
                    //Host3 is not used
                    BleHost3Style = "0";
                    //Host 1 show the paired 1
                    BleHost1Text = string.IsNullOrEmpty(DeviceInfo.PairedHostName1) ? Strings.ReadyToBePaired : DeviceInfo.PairedHostName1;
                    //Host 2 show the paired 2
                    BleHost2Text = string.IsNullOrEmpty(DeviceInfo.PairedHostName2) ? Strings.ReadyToBePaired : DeviceInfo.PairedHostName2;
                    //txtBLHost1.Text = string.IsNullOrEmpty(_vm.PairedHostName1) ? Strings.ReadyToBePaired : _vm.PairedHostName1;
                    //txtBLHost2.Text = string.IsNullOrEmpty(_vm.PairedHostName2) ? Strings.ReadyToBePaired : _vm.PairedHostName2;
                    //If host1 is current host (computer name)
                    if (BleHost1Text.Equals(hostName, StringComparison.CurrentCultureIgnoreCase))
                    {
                        BleHost1Style = "1";
                        BleHost1Text = BleHost1Text.Substring(0, maxHostNameLength);
                        //imgBL1.Source = img1;
                        //txtBLHost1.Style = ConnectionStyle1;
                    }
                    else
                    {
                        BleHost2Style = "1";
                        BleHost2Text = BleHost2Text.Substring(0, maxHostNameLength);
                        //imgBL2.Source = img1;
                        //txtBLHost2.Style = ConnectionStyle1;
                    }
                    break;

                default:
                    //Host 1 and 3 is unused
                    BleHost1Style = "0";
                    BleHost3Style = "0";
                    //BleHost1Text = DeviceInfo.PairedHostName2;
                    //txtBLHost1.Text = _vm.PairedHostName2;

                    //Host 2 show current computer
                    BleHost2Style = "1";
                    BleHost2Text = hostName.Substring(0, maxHostNameLength);
                    //txt1.Style = ConnectionStyle1;
                    //imgBL1.Source = img1;
                    //txtBLHost1.Style = ConnectionStyle1;

                    //BleHost2Style = "0";
                    //Host2.Visibility = Visibility.Collapsed;
                    break;
            } //switch
            //Trim string length to <= 15
            //if (BleHost1Text.Length > 15)
            //    BleHost1Text = BleHost1Text.Substring(0, 15);
            //if (BleHost2Text.Length > 15)
            //    BleHost2Text = BleHost2Text.Substring(0, 15);
            //if (BleHost3Text.Length > 15)
            //    BleHost3Text = BleHost3Text.Substring(0, 15);
        }

        //Need to fix: How to know Audio BLE have 1 or 2 slots?
        //Currently, we will show only one host.
        private void SetBLConnectionStatus_Audio()
        {
            try
            {
                if (DeviceInfo == null)
                    return;

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
                AudioBleText = string.Format(Strings.Paired_Info, DeviceInfo.TotalNumberOfPairedHostName); //"This device can be paired with {0} hosts simultaneously";
                                                                                                           //int totalPairedHostCount = DeviceInfo.TotalNumberOfPairedHostName;
                                                                                                           //AudioBleText = String.Format(AudioBleConnectionTotalPairCountText, totalPairedHostCount);
                                                                                                           //BLConnection.Visibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog($"[HomeDevice] SetBLConnectionStatus_Audio Exception : {ex.Message}");
            }
        }

        private void SetBLConnectionStatus_IO()
        {
            if (DeviceInfo == null)
                return;

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
        private string _dock_FirmwareVersion;
        public string Dock_FirmwareVersion
        {
            get => _dock_FirmwareVersion;
            set => SetProperty(ref _dock_FirmwareVersion, value);
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
            //<< 241212 added by Hess for Pandora
            if (deviceInfo.ModelNumber == "PN5122W")
                return "Pandora";
            //>>

            switch (deviceInfo.PhysicalDeviceType)
            {
                case DeviceType.PhysicalWebcam:
                    return "Wired";
                case DeviceType.PhysicalAudioDongle:
                case DeviceType.PhysicalBluetoothAudio:
                    return deviceInfo.PhysicalDeviceType.ToString().Replace("Physical", "").Replace("Audio", "");

                case DeviceType.PhysicalBluetooth:
                case DeviceType.PhysicalDongle:
                case DeviceType.PhysicalPen:
                    return deviceInfo.PhysicalDeviceType.ToString().Replace("Physical", "");

                //0823 Bruce 新增Dock連線方式的濾字串的方式
                case DeviceType.PhysicalWiredDock:
                    string s = "";
                    if (deviceInfo.ModelNumber.Contains("TB5"))
                    {
                        s = "USB-C (TB 5)";
                    }
                    else if (deviceInfo.ModelNumber.Contains("TB4"))
                    {
                        s = "USB-C (TB 4)";
                    }
                    else if (deviceInfo.ModelNumber.Contains("DCS"))
                    {
                        s = "Dual USB-C (DP 1.4)";
                    }
                    else
                    {
                        s = "USB-C (DP 1.4)";
                    }
                    return s;

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
            if (MonitorInfo.CapabilityDic.ContainsKey("E7"))
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
                if (MonitorInfo.CapabilityDic.ContainsKey("E7"))
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
                    //Robert_Lin, 2024-11-18 add try-catch
                    try
                    {
                        _hasCapability_NetworkKvm = DeviceManagerSA.isNKVMSupportMonitor(MonitorInfo).Result;
                        return _hasCapability_NetworkKvm.Value;
                    }
                    catch (Exception e)
                    {
                        string errMsg = e.Message;
                    }
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
                    if (MonitorInfo.CapabilityDic != null && MonitorInfo.modelName.ToUpper().StartsWith("G"))
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

        #region PIP/PBP Capabilities
        //The Pxp capabilities Code array, will be build when the first time calling
        private List<string> _pxpCapStrings = new List<string>();//SDL, change to use new

        //Will be called once MonitorInfo been setup/updated
        private void InitPipPbpCaps()
        {
            //If it has been inited
            if (_pxpCapStrings.Count > 0)
                return;
            if (!HasCapability_PipPbp)
                return;

            _pxpCapStrings = MonitorInfo.CapabilityDic["E9"];
        }

        /// <summary>
        /// Return if current MonitorInfo in this HomeDevice has capability of the specified PxpMode
        /// </summary>
        /// <param name="hexStringPxpMode"> for example "21" will return true if has 0x21 mode capability</param>
        /// <returns></returns>
        public bool HasCapability_PxpMode(string hexStringPxpMode)
        {
            if (!HasCapability_PipPbp)
                return false;

            if ((_pxpCapStrings != null) && (_pxpCapStrings.Count > 0))
            {
                return _pxpCapStrings.Contains(hexStringPxpMode);
            }
            return false;
        }

        #endregion  PIP/PBP Capabilities

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
                if (mi1.DDCisON != mi2.DDCisON)
                    return false;
            }
            if (!mask.Contains("inputSource", StringComparison.OrdinalIgnoreCase))
            {
                if (mi1.inputSource != mi2.inputSource)
                    return false;
            }
            if (!mask.Contains("edid", StringComparison.OrdinalIgnoreCase))
            {
                if (!EqualityComparer<EDID>.Equals(mi1.edid, mi2.edid))
                    return false;
            }

            return true;
        }
        #endregion Monitor Equals

        #region Dump Info to Log
        public void DumpInfoToLog(ILog? log)
        {
            if (log == null)
                return;

            log.Info($"HomeDevice, DeviceCategory=[{DeviceCategory}], DisplayName=[{DisplayName}]");
            //log installedUICulture,2024-11-15 gavin
            CultureInfo installedUICulture = CultureInfo.InstalledUICulture;
            log.Info($"HomeDevice Page, installedUICulture=[{installedUICulture}]");
            if (MonitorInfo != null)
            {
                log.Info($"  * CapabilityString={MonitorInfo.CapabilityString}");
                log.Info($"  * AliasDeviceName=[{MonitorInfo.AliasDeviceName}]");
                log.Info($"  * MarketName=[{MonitorInfo.MarketingName}]");
            }
            if (DeviceInfo != null)
            {
                log.Info($"  * Name=[{DeviceInfo.Name}], ModelNumber=[{DeviceInfo.ModelNumber}], ColorCode=[{DeviceInfo.ColorCode}]");
                log.Info($"  * IsBatteryLevelSupported=[{DeviceInfo.IsBatteryLevelSupported}], BatteryLevel=[{DeviceInfo.BatteryLevel}], BatteryStatus=[{DeviceInfo.BatteryStatus}]");
                log.Info($"  * FirmwareVersion=[{DeviceInfo.FirmwareVersion}]");
                log.Info($"  * TotalNumberOfPairedHostName=[{DeviceInfo.TotalNumberOfPairedHostName}], PairedHostName1={DeviceInfo.PairedHostName1}], PairedHostName2={DeviceInfo.PairedHostName2}], PairedHostName3={DeviceInfo.PairedHostName3}]");
                log.Info($"  * IsConnected=[{DeviceInfo.IsConnected}], Status=[{DeviceInfo.Status}]");
                log.Info($"  * MaxPairingSlots=[{DeviceInfo.MaxPairingSlots}], PairingStatusName=[{DeviceInfo.PairingStatusName}], PairedDeviceCount=[{DeviceInfo.PairedDeviceCount}]");
                log.Info($"  * IsPhysicalDeviceDongle=[{DeviceInfo.IsPhysicalDeviceDongle}], PhysicalDeviceFirmwareVersion=[{DeviceInfo.PhysicalDeviceFirmwareVersion}]");
            }
        }
        #endregion Dump Info to Log

        #region LandingMarketName
        /// <summary>
        /// The market name displaying on Landing Page, after left arrow.
        /// Base on DDPMW-2094, the NPI projects need to show the MarketName 
        /// (provided by DDPM.SA.Plugins.User.VcpCorePlugin, and assign to MonitorInfo.MarketName)
        /// But for Non-NPI projects, MonitorInfo.MarketName will be String.Empty.
        /// We will need to display "Disaplay" and translate o multilingual text.
        /// </summary>
        public string LandingMarketName
        {
            get
            {
                if (MonitorInfo != null)
                {
                    if (String.IsNullOrWhiteSpace(MonitorInfo.MarketingName))
                        return Strings.Display;
                    /* Debug text
                    return "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Pellentesque sem lorem, ornare at fringilla sed, eleifend ut nibh. Nullam a tincidunt sapien. Donec luctus felis eget facilisis sodales. Mauris nec ipsum elit. Curabitur sagittis mollis libero, id fringilla neque interdum at. Vivamus sit amet tortor consectetur enim egestas volutpat in id elit.";
                    */
                    else
                        return MonitorInfo.MarketingName;
                }
                if (DeviceInfo != null)
                {
                    return DeviceInfo.Name;
                }
                return "(Noname)";
            }
        }
        #endregion

    }
}