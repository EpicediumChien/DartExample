using CommunityToolkit.Mvvm.Input;
using DPeMPublic.Common.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows.Input;

namespace DDPM.SA.Common
{
    [Serializable]
    public class DDPMDevice : INotifyPropertyChanged
    {
        #region Private Members

        private string _name;
        private string _batteryStatus;
        private string _pairedHostName1;
        private string _pairedHostName2;
        private string _pairedHostName3;
        private byte[] _deviceImage;
        private int _totalNumberOfPairedHostName;
        private string _dpiLevelValue;
        private string _collabsKeysSupported;
        private bool _isCollabsKeysSupported;
        private string _physicalDeviceType;
        private string _logicalDeviceType;
        private string _status;
        private bool _isCollaborationKeyEnable;
        private bool _isCollaborationCameraEnable;
        private bool _isCollaborationScreenShareEnable;
        private bool _isCollaborationChatEnable;
        private bool _isCollaborationMicEnable;
        private bool _isCollaborationBlinkEffectEnable;
        private bool _isCollaborationDoubleTapEnable;
        private bool _isIlluminationSupported;
        private int _backLightingControls;
        private int _backLightingLevel;
        private List<string> _dPILevel;
        private List<string> _mouseButtonOptions;
        private string _primaryMouseButton;
        private bool _isDPILevelSupported;
        private bool _isDPIValueSupported;
        private bool _isDPILevelChangePending;
        private bool _isDPIValueChangePending;
        private int _dpiMin;
        private int _dpiMax;
        private int _dpiDelta;
        private bool _isTouchScrollSensitivitySupported;
        private bool _isReportRateSupported;
        private int _dpiLevel;
        private int _touchScrollSensitivityLevel;
        private int _touchSensitivityLevelValue;
        private int _reportRate;
        private int _backLightTabIndex;
        private string[] _dpiLevelValues;
        private string _deviceName;
        private bool _muteStatus;

        public const int SpiSetMouseButtonLeft = 23;

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        private static extern bool SwapMouseButton([param: MarshalAs(UnmanagedType.Bool)] bool fSwap);

        private static bool _SwapMouseButton(bool fSwap)
        {
            return SwapMouseButton(fSwap);
        }

        private const uint SpiSetMouseButtonRight = 33;
        private const uint SpiSendWinInChange = 2;
        private const uint SpiUpdateable = 1;

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        private static extern bool SystemParametersInfo(uint uiAction, uint uiParam, IntPtr pvParam, uint fWinIni);

        private static bool _SystemParametersInfo(uint uiAction, uint uiParam, IntPtr pvParam, uint fWinIni)
        {
            return SystemParametersInfo(uiAction, uiParam, pvParam, fWinIni);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public ICommand ToggleOptionCommand { get; set; }

        #region Physical Device Dongle Private Properties

        private string _pairingStatusName;
        private int _maxPairingSlots;
        private int _pairedDeviceCount;

        #endregion Physical Device Dongle Private Properties

        #endregion Private Members

        #region Properties

        public Guid ID { get; set; }
        public Guid PhyscialDeviceID { get; set; }

        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                OnPropertyChanged();
            }
        }

        public DeviceInterfaceType InterfaceType { get; set; }

        public DeviceType Type { get; set; }
        public string PluginId { get; set; }
        public int OdmId { get; set; }
        public string ModelNumber { get; set; }
        public bool IsBatteryLevelSupported { get; set; }

        public int InstanceNumber { get; set; }
        public int InstanceId { get; set; }
        public int ColorCode { get; set; }

        public string FirmwareVersion { get; set; }

        public int BatteryLevel { get; set; }

        public string PairedHostName1
        {
            get => _pairedHostName1;
            set
            {
                _pairedHostName1 = value;
                OnPropertyChanged();
            }
        }

        public string VisiblePairedHostName1 => string.IsNullOrEmpty(_pairedHostName1) ? "Collapsed" : "Visible";

        public string VisiblePairedHostName2 => string.IsNullOrEmpty(_pairedHostName2) ? "Collapsed" : "Visible";

        public string VisiblePairedHostName3 => string.IsNullOrEmpty(_pairedHostName3) ? "Collapsed" : "Visible";

        public string PairedHostName2
        {
            get => _pairedHostName2;
            set
            {
                _pairedHostName2 = value;
                OnPropertyChanged();
            }
        }

        public string PairedHostName3
        {
            get => _pairedHostName3;
            set
            {
                _pairedHostName3 = value;
                OnPropertyChanged();
            }
        }

        public byte[] DeviceImage
        {
            get => _deviceImage;
            set
            {
                _deviceImage = value;
                OnPropertyChanged();
            }
        }

        public int TotalNumberOfPairedHostName
        {
            get => _totalNumberOfPairedHostName;
            set
            {
                _totalNumberOfPairedHostName = value;
                OnPropertyChanged();
            }
        }

        public string MousePrimaryButton
        {
            get; set;
        }

        public string DpiLevelValue
        {
            get => _dpiLevelValue;
            set
            {
                _dpiLevelValue = value;
                OnPropertyChanged();
            }
        }

        public string CollabsKeysSupported
        {
            get => _collabsKeysSupported;
            set
            {
                _collabsKeysSupported = value;
                OnPropertyChanged();
            }
        }

        public bool IsCollabsKeysSupported
        {
            get => _isCollabsKeysSupported;
            set
            {
                _isCollabsKeysSupported = value;
                OnPropertyChanged();
            }
        }

        public string PhysicalDeviceType
        {
            get => _physicalDeviceType;
            set
            {
                _physicalDeviceType = value;
                OnPropertyChanged();
            }
        }

        public string LogicalDeviceType
        {
            get
            {
                switch (_logicalDeviceType)
                {
                    case "LogicalMouse":
                        DeviceName = "Mouse Settings";
                        break;

                    case "LogicalKeyboard":
                        DeviceName = "Keyboard Settings";
                        break;

                    case "LogicalWiredAudio":
                        DeviceName = "Wired Audio Settings";
                        break;
                }
                return _logicalDeviceType;
            }
            set
            {
                _logicalDeviceType = value;
                OnPropertyChanged();
            }
        }

        public string BatteryStatus
        {
            get => _batteryStatus;
            set
            {
                _batteryStatus = value;
                OnPropertyChanged();
            }
        }

        public bool IsConnected { get; set; }

        public string Status
        {
            get => _status;
            set
            {
                _status = value;
                OnPropertyChanged();
            }
        }

        public bool IsCollaborationKeyEnable
        {
            get => _isCollaborationKeyEnable;
            set
            {
                _isCollaborationKeyEnable = value;
                OnPropertyChanged();
            }
        }

        public bool IsCollaborationCameraEnable
        {
            get => _isCollaborationCameraEnable;
            set
            {
                _isCollaborationCameraEnable = value;
                OnPropertyChanged();
            }
        }

        public bool IsCollaborationScreenShareEnable
        {
            get => _isCollaborationScreenShareEnable;
            set
            {
                _isCollaborationScreenShareEnable = value;
                OnPropertyChanged();
            }
        }

        public bool IsCollaborationChatEnable
        {
            get => _isCollaborationChatEnable;
            set
            {
                _isCollaborationChatEnable = value;
                OnPropertyChanged();
            }
        }

        public bool IsCollaborationMicEnable
        {
            get => _isCollaborationMicEnable;
            set
            {
                _isCollaborationMicEnable = value;
                OnPropertyChanged();
            }
        }

        public bool IsCollaborationBlinkEffectEnable
        {
            get => _isCollaborationBlinkEffectEnable;
            set
            {
                _isCollaborationBlinkEffectEnable = value;
                OnPropertyChanged();
            }
        }

        public bool IsCollaborationDoubleTapEnable
        {
            get => _isCollaborationDoubleTapEnable;
            set
            {
                _isCollaborationDoubleTapEnable = value;
                OnPropertyChanged();
            }
        }

        public bool IsIlluminationSupported
        {
            get => _isIlluminationSupported;
            set
            {
                _isIlluminationSupported = value;
                OnPropertyChanged();
            }
        }

        public int BackLightingControls
        {
            get => _backLightingControls;
            set
            {
                _backLightingControls = value;
                OnPropertyChanged();
            }
        }

        public int BackLightingLevel
        {
            get => _backLightingLevel;
            set
            {
                _backLightingLevel = value;
                OnPropertyChanged();
            }
        }

        public List<string> DPILevel
        {
            get => _dPILevel;
            set
            {
                _dPILevel = value;
                OnPropertyChanged();
            }
        }

        public List<string> MouseButtonOptions
        {
            get => _mouseButtonOptions;
            set
            {
                _mouseButtonOptions = value;
                OnPropertyChanged();
            }
        }

        public string SelectedPrimaryMouseButton
        {
            get => _primaryMouseButton;
            set
            {
                _primaryMouseButton = value;
                UpdateSwapButtonSetting(value);
                OnPropertyChanged();
            }
        }

        public bool IsDPILevelSupported
        {
            get => _isDPILevelSupported;
            set
            {
                _isDPILevelSupported = value;
                OnPropertyChanged();
            }
        }

        public bool IsDPIValueSupported
        {
            get => _isDPIValueSupported;
            set
            {
                _isDPIValueSupported = value;
                OnPropertyChanged();
            }
        }

        public int DpiLevel
        {
            get => _dpiLevel;
            set
            {
                _dpiLevel = value;
                OnPropertyChanged();
            }
        }

        public bool IsDPILevelChangePending
        {
            get => _isDPILevelChangePending;
            set
            {
                _isDPILevelChangePending = value;
                OnPropertyChanged();
            }
        }

        public bool IsDPIValueChangePending
        {
            get => _isDPIValueChangePending;
            set
            {
                _isDPIValueChangePending = value;
                OnPropertyChanged();
            }
        }

        public bool IsTouchScrollSensitivitySupported
        {
            get => _isTouchScrollSensitivitySupported;
            set
            {
                _isTouchScrollSensitivitySupported = value;
                OnPropertyChanged();
            }
        }

        public int TouchScrollSensitivityLevel
        {
            get => _touchScrollSensitivityLevel;
            set
            {
                _touchScrollSensitivityLevel = value;
                OnPropertyChanged();
            }
        }

        public int TouchSensitivityLevelValue
        {
            get => _touchSensitivityLevelValue;
            set
            {
                _touchSensitivityLevelValue = value;
                OnPropertyChanged();
            }
        }

        public bool IsReportRateSupported
        {
            get => _isReportRateSupported;
            set
            {
                _isReportRateSupported = value;
                OnPropertyChanged();
            }
        }

        public int ReportRate
        {
            get => _reportRate;
            set
            {
                _reportRate = value;
                OnPropertyChanged();
            }
        }

        public string[] DpiLevelValues
        {
            get => _dpiLevelValues;
            set
            {
                _dpiLevelValues = value;
                OnPropertyChanged();
            }
        }

        public string DeviceName
        {
            get => _deviceName;
            set
            {
                _deviceName = value;
                OnPropertyChanged();
            }
        }

        public int DpiMin
        {
            get => _dpiMin;
            set
            {
                _dpiMin = value;
                OnPropertyChanged();
            }
        }

        public int DpiMax
        {
            get => _dpiMax;
            set
            {
                _dpiMax = value;
                OnPropertyChanged();
            }
        }

        public int DpiDelta
        {
            get => _dpiDelta;
            set
            {
                _dpiDelta = value;
                OnPropertyChanged();
            }
        }

        public int BackLightTabIndex
        {
            get => _backLightTabIndex;
            set
            {
                _backLightTabIndex = value;
                OnPropertyChanged();
            }
        }

        #region Physical Device Dongle Properties

        public string PairingStatusName
        {
            get => _pairingStatusName;
            set
            {
                _pairingStatusName = value;
                OnPropertyChanged();
            }
        }

        public int MaxPairingSlots
        {
            get => _maxPairingSlots;
            set
            {
                _maxPairingSlots = value;
                OnPropertyChanged();
            }
        }

        public int PairedDeviceCount
        {
            get => _pairedDeviceCount;
            set
            {
                _pairedDeviceCount = value;
                OnPropertyChanged();
            }
        }

        public bool MuteStatus
        {
            get => _muteStatus;
            set
            {
                _muteStatus = value;
                OnPropertyChanged();
            }
        }

        public bool IsPhysicalDeviceDongle { get; set; }

        public string PhysicalDeviceFirmwareVersion { get; set; }

        #endregion Physical Device Dongle Properties

        #endregion Properties

        #region Methods

        public DDPMDevice()
        {
            ToggleOptionCommand = new RelayCommand<object>(ToggleOption);
        }

        private void ToggleOption(object parameter)
        {
            if (parameter != null && bool.TryParse(parameter.ToString(), out bool isChecked))
            {
                IsCollaborationKeyEnable = isChecked;
                // Perform any other actions here based on the checkbox state.
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public void UpdateSwapButtonSetting(string selectedValue)
        {
            _SwapMouseButton(selectedValue != "Left");
            _SystemParametersInfo(
                selectedValue == "Right" ? SpiSetMouseButtonRight : SpiSetMouseButtonLeft,
                1, IntPtr.Zero, SpiSendWinInChange | SpiUpdateable);
        }

        #endregion Methods
    }

    public enum CommandReturnCode
    {
        success = 0,
        null_input = 1,
        empty_deviceID = 2,
        no_match_deviceID = 3,
    }

    public class CommandParams
    {
        public string deviceID { get; set; }
        public string serviceTag { get; set; }
        public string deviceStatus { get; set; }
    }

    public class CommandParams_notifyDeviceConnection
    {
        public string deviceID { get; set; } = string.Empty; //point to monitor [serial number], peripheral [id]
        public string serviceTag { get; set; } = string.Empty;
        public string deviceStatus { get; set; } = string.Empty; //1.connected, 2.disconnected, 3. empty if do query
    }

    public class configurationPayload
    {
        public bool enable { get; set; }
        public string accessPoint { get; set; }
        public bool certProvisioning { get; set; }
        public string ipAddress { get; set; }
        public string subnetMask { get; set; }
        public string gateway { get; set; }
        public string dns { get; set; }
        public string dhcp { get; set; }
        public additionalConfigurations addConfigs { get; set; }
    }

    public class additionalConfigurations
    {
        public string customSettings1 { get; set; }
        public string customSettings2 { get; set; }
    }

    public class CommandResult
    {
        public string deviceID { get; set; } //for display point to serial number, for peripherals point to ID (GUID)
        public string serviceTag { get; set; }
        public List<Monitor_Listen_param> monitors { get; set; } = new List<Monitor_Listen_param>();
        public List<Peripheral_Listen_param> peripherals { get; set; } = new List<Peripheral_Listen_param>();
    }

    public class CommandError
    {
        public int code { get; set; }
        public string message { get; set; }
    }

    public class CommandInput
    {
        public string jsonrpc { get; set; }
        public int id { get; set; }
        public string deviceName { get; set; }
        public string methodName { get; set; }
        public CommandParams Params { get; set; }

        #region CommandData instance

        private static CommandInput INSTANCE = null;

        public static CommandInput GetInstance()
        {
            if (INSTANCE == null)
            {
                INSTANCE = new CommandInput();
            }

            return INSTANCE;
        }

        #endregion CommandData instance
    }

    public class CommandOutput
    {
        public string jsonrpc { get; set; }
        public string id { get; set; }

        //public string deviceName { get; set; }
        public string methodName { get; set; }

        public CommandResult result { get; set; } = null;
        public CommandError error { get; set; } = null;

        #region CommandOutput instance

        private static CommandOutput INSTANCE = null;

        public static CommandOutput GetInstance()
        {
            if (INSTANCE == null)
            {
                INSTANCE = new CommandOutput();
            }

            return INSTANCE;
        }

        #endregion CommandOutput instance
    }

    public class CommandInput_notifyDeviceConnection
    {
        public string jsonrpc { get; set; } = "2.0";
        public int id { get; set; } = 1;
        public string methodName { get; set; } = string.Empty; // 1.notifyDeviceConnected, 2.notifyDeviceDisConnected
        public CommandParams_notifyDeviceConnection Params { get; set; }
    }

    public class CommandOutput_DeviceConnection
    {
        public string jsonrpc { get; set; } = "2.0";
        public string id { get; set; } = "1";
        public string methodName { get; set; } = string.Empty; //1.notifyDeviceConnected, 2.notifyDeviceDisConnected, 3.queryConnectedDeviceInfo
        public CommandResult result { get; set; } = null;
        public CommandError error { get; set; } = null;
    }

    public class Monitor_Listen_param
    {
        public string serialNumber { get; set; }
        public string serviceTag { get; set; }
        public string deviceStatus { get; set; }
    }

    public class Peripheral_Listen_param
    {
        public string ID { get; set; }

        //public string InstanceId { get; set; } //no chance to use this
        public string deviceStatus { get; set; }
    }
}