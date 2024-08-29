using CommunityToolkit.Mvvm.Input;
using DPeMPublic.Common.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using MouseButton = DPeMPublic.Common.Enums.MouseButton;

namespace DDPM.SA.Common
{
    [Serializable]
    public class DeviceInfo : INotifyPropertyChanged
    {
        #region Private Members

        private string _name;
        private string _batteryStatus;
        private string _pairedHostName1;
        private string _pairedHostName2;
        private string _pairedHostName3;
        private byte[] _deviceImage;
        private int _totalNumberOfPairedHostName;
        private string _dpiValue;
        private string[] _pairedHostNames;
        private string _collabsKeysSupported;
        private bool _isCollabsKeysSupported;
        private DeviceType _physicalDeviceType;
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
        private int _wiredAudioVolumeAdjustmentTone;
        private bool _isWiredAudioMicMuteSoundEnable;
        private bool _isWiredAudioIMicNSEnable;
        private MouseButton _mousePrimaryButton;
        private bool _isReady;
        private bool _isDirty;
        private bool _isMicNoiseCancellationSupported;
        private bool _isSidetoneSupported;
        private bool _isBusyLightSupported;
        private bool _isVoiceGuidanceSupported;
        private bool _isPresetsSupported;
        private bool _isEqualizerSupported;
        private bool _isANCSupported;
        private bool _isWearDetectionSupported;
        private bool _isWearDetectionSensitivitySupported;
        private bool _isWearDetectionPauseMusicSupported;
        private bool _isWearDetectionMuteMicSupported;
        private bool _isWearDetectionQuickPauseSupported;
        private bool _micNoiseCancellation;
        private bool _micNCIncoming;
        private bool _sidetone;
        private bool _busyLight;
        private bool _voiceGuidance;
        private int _selectedPreset;
        private int _sidetoneLevel;
        private byte[] _bandsGain;
        private int _band1Gain;
        private int _band2Gain;
        private int _band3Gain;
        private int _band4Gain;
        private int _band5Gain;
        private int _ancMode;
        private int _ancGain;
        private int _wearDetection;
        private bool _isWearDetectionChecked;
        private bool _isPauseMusicChecked;
        private bool _isMuteMicrophoneChecked;
        private bool _isQuickPauseChecked;
        private bool _isMicNCIncomingSupported;
        private HeadsetConnectionType _connectionType;
        private bool _isBLE;
        private string _isdServiceVersion;
        private string _isdDriverVersion;
        private int _monitorCount;
        private byte[] _dockData;
        private byte[] _dockInfo;
        private int _dockType;
        private string _dockService;
        private string _dockPackageFwVersion;
        private int _dockFwUpdateStatus;
        private int _dockTBTConnectionStatus;

        #region private webcam Properties

        private string _deviceSymbolicLink;
        private string _parentDevInstanceId;
        private bool _isESISupported;
        private string[] _supportedProperties;
        private string[] _fOVValues;
        private string[] _supportedResolutions;
        private int _supportedFeatures;
        private int _currentFeatures;
        private bool _isMicEnumerationSupported;
        private bool _isMicEnumerationOn;
        private bool _isWindowsHelloSupported;
        private bool _hasWindowsHelloPowerConstraint;

        #endregion private webcam Properties

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

        public byte[] ThumbnailImageRawData { get; set; }

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

        public DPeMPublic.Common.Enums.MouseButton MousePrimaryButton
        {
            get => _mousePrimaryButton;
            set
            {
                _mousePrimaryButton = value;
                OnPropertyChanged();
            }
        }

        public string[] PairedHostNames
        {
            get => _pairedHostNames;
            set
            {
                _pairedHostNames = value;
                OnPropertyChanged();
            }
        }

        public string DpiValue
        {
            get => _dpiValue;
            set
            {
                _dpiValue = value;
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

        public DeviceType PhysicalDeviceType
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

                    case "LogicalHeadset":
                        DeviceName = "Headset Settings";
                        break;

                    case "LogicalPen":
                        DeviceName = "Pen Settings";
                        break;

                    case "LogicalDock":
                        DeviceName = "Dock Settings";
                        break;

                    case "LogicalWebcam":
                        DeviceName = "Webcam Settings";
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

        public bool IsPhysicalDeviceDongle { get; set; }

        public string PhysicalDeviceFirmwareVersion { get; set; }

        #endregion Physical Device Dongle Properties

        #region Audio Headsets Properties

        public bool MuteStatus
        {
            get => _muteStatus;
            set
            {
                _muteStatus = value;
                OnPropertyChanged();
            }
        }

        public bool IsWiredAudioIMicNSEnable
        {
            get => _isWiredAudioIMicNSEnable;
            set
            {
                _isWiredAudioIMicNSEnable = value;
                OnPropertyChanged();
            }
        }

        public bool IsWiredAudioMicMuteSoundEnable
        {
            get => _isWiredAudioMicMuteSoundEnable;
            set
            {
                _isWiredAudioMicMuteSoundEnable = value;
                OnPropertyChanged();
            }
        }

        public int WiredAudioVolumeAdjustmentTone
        {
            get => _wiredAudioVolumeAdjustmentTone;
            set
            {
                _wiredAudioVolumeAdjustmentTone = value;
                OnPropertyChanged();
            }
        }

        public bool IsReady
        {
            get => _isReady;
            set
            {
                _isReady = value;
                OnPropertyChanged();
            }
        }

        public bool IsDirty
        {
            get => _isDirty;
            set
            {
                _isDirty = value;
                OnPropertyChanged();
            }
        }

        public bool IsMicNoiseCancellationSupported
        {
            get => _isMicNoiseCancellationSupported;
            set
            {
                _isMicNoiseCancellationSupported = value;
                OnPropertyChanged();
            }
        }

        public bool IsSidetoneSupported
        {
            get => _isSidetoneSupported;
            set
            {
                _isSidetoneSupported = value;
                OnPropertyChanged();
            }
        }

        public bool IsBusyLightSupported
        {
            get => _isBusyLightSupported;
            set
            {
                _isBusyLightSupported = value;
                OnPropertyChanged();
            }
        }

        public bool IsVoiceGuidanceSupported
        {
            get => _isVoiceGuidanceSupported;
            set
            {
                _isVoiceGuidanceSupported = value;
                OnPropertyChanged();
            }
        }

        public bool IsPresetsSupported
        {
            get => _isPresetsSupported;
            set
            {
                _isPresetsSupported = value;
                OnPropertyChanged();
            }
        }

        public bool IsEqualizerSupported
        {
            get => _isEqualizerSupported;
            set
            {
                _isEqualizerSupported = value;
                OnPropertyChanged();
            }
        }

        public HeadsetConnectionType ConnectionType
        {
            get => _connectionType;
            set
            {
                _connectionType = value;
                OnPropertyChanged();
            }
        }

        public bool IsANCSupported
        {
            get => _isANCSupported;
            set
            {
                _isANCSupported = value;
                OnPropertyChanged();
            }
        }

        public bool IsWearDetectionSupported
        {
            get => _isWearDetectionSupported;
            set
            {
                _isWearDetectionSupported = value;
                OnPropertyChanged();
            }
        }

        public bool IsWearDetectionSensitivitySupported
        {
            get => _isWearDetectionSensitivitySupported;
            set
            {
                _isWearDetectionSensitivitySupported = value;
                OnPropertyChanged();
            }
        }

        public bool IsWearDetectionPauseMusicSupported
        {
            get => _isWearDetectionPauseMusicSupported;
            set
            {
                _isWearDetectionPauseMusicSupported = value;
                OnPropertyChanged();
            }
        }

        public bool IsWearDetectionMuteMicSupported
        {
            get => _isWearDetectionMuteMicSupported;
            set
            {
                _isWearDetectionMuteMicSupported = value;
                OnPropertyChanged();
            }
        }

        public bool IsWearDetectionQuickPauseSupported
        {
            get => _isWearDetectionQuickPauseSupported;
            set
            {
                _isWearDetectionQuickPauseSupported = value;
                OnPropertyChanged();
            }
        }

        public bool MicNoiseCancellation
        {
            get => _micNoiseCancellation;
            set
            {
                _micNoiseCancellation = value;
                OnPropertyChanged();
            }
        }

        public bool MicNCIncoming
        {
            get => _micNCIncoming;
            set
            {
                _micNCIncoming = value;
                OnPropertyChanged();
            }
        }

        public bool Sidetone
        {
            get => _sidetone && !(AncMode == 2);
            set
            {
                _sidetone = value;
                OnPropertyChanged();
            }
        }

        public bool BusyLight
        {
            get => _busyLight;
            set
            {
                _busyLight = value;
                OnPropertyChanged();
            }
        }

        public bool VoiceGuidance
        {
            get => _voiceGuidance;
            set
            {
                _voiceGuidance = value;
                OnPropertyChanged();
            }
        }

        public int SelectedPreset
        {
            get => _selectedPreset;
            set
            {
                _selectedPreset = value;
                OnPropertyChanged();
            }
        }

        public int SidetoneLevel
        {
            get => _sidetoneLevel;
            set
            {
                _sidetoneLevel = value;
                OnPropertyChanged();
            }
        }

        public byte[] BandsGain
        {
            get => _bandsGain;
            set
            {
                _bandsGain = value;
                OnPropertyChanged();
            }
        }

        public int Band1Gain
        {
            get => _band1Gain;
            set
            {
                _band1Gain = value;
                OnPropertyChanged();
            }
        }

        public int Band2Gain
        {
            get => _band2Gain;
            set
            {
                _band2Gain = value;
                OnPropertyChanged();
            }
        }

        public int Band3Gain
        {
            get => _band3Gain;
            set
            {
                _band3Gain = value;
                OnPropertyChanged();
            }
        }

        public int Band4Gain
        {
            get => _band4Gain;
            set
            {
                _band4Gain = value;
                OnPropertyChanged();
            }
        }

        public int Band5Gain
        {
            get => _band5Gain;
            set
            {
                _band5Gain = value;
                OnPropertyChanged();
            }
        }

        public int AncMode
        {
            get => _ancMode;
            set
            {
                _ancMode = value;
                OnPropertyChanged();
            }
        }

        public int AncGain
        {
            get => _ancGain;
            set
            {
                _ancGain = value;
                OnPropertyChanged();
            }
        }

        public int WearDetection
        {
            get => _wearDetection;
            set
            {
                _wearDetection = value;
                OnPropertyChanged();
            }
        }

        public bool IsWearDetectionChecked
        {
            get => _isWearDetectionChecked;
            set
            {
                _isWearDetectionChecked = value;
                OnPropertyChanged();
            }
        }

        public bool IsPauseMusicChecked
        {
            get => _isPauseMusicChecked;
            set
            {
                _isPauseMusicChecked = value;
                OnPropertyChanged();
            }
        }

        public bool IsMuteMicrophoneChecked
        {
            get => _isMuteMicrophoneChecked;
            set
            {
                _isMuteMicrophoneChecked = value;
                OnPropertyChanged();
            }
        }

        public bool IsQuickPauseChecked
        {
            get => _isQuickPauseChecked;
            set
            {
                _isQuickPauseChecked = value;
                OnPropertyChanged();
            }
        }

        public bool IsMicNCIncomingSupported
        {
            get => _isMicNCIncomingSupported;
            set
            {
                _isMicNCIncomingSupported = value;
                OnPropertyChanged();
            }
        }

        public bool IsMitoSensitivityTabControlEnabled
        {
            get => _isQuickPauseChecked && _isWearDetectionChecked;
        }

        #endregion Audio Headsets Properties

        #region Webcam Properties

        public string DeviceSymbolicLink
        {
            get => _deviceSymbolicLink;
            set
            {
                _deviceSymbolicLink = value;
                OnPropertyChanged();
            }
        }

        public string ParentDevInstanceId
        {
            get => _parentDevInstanceId;
            set
            {
                _parentDevInstanceId = value;
                OnPropertyChanged();
            }
        }

        public bool IsESISupported
        {
            get => _isESISupported;
            set
            {
                _isESISupported = value;
                OnPropertyChanged();
            }
        }

        public string[] SupportedProperties
        {
            get => _supportedProperties;
            set
            {
                _supportedProperties = value;
                OnPropertyChanged();
            }
        }

        public string[] FOVValues
        {
            get => _fOVValues;
            set
            {
                _fOVValues = value;
                OnPropertyChanged();
            }
        }

        public string[] SupportedResolutions
        {
            get => _supportedResolutions;
            set
            {
                _supportedResolutions = value;
                OnPropertyChanged();
            }
        }

        public int SupportedFeatures
        {
            get => _supportedFeatures;
            set
            {
                _supportedFeatures = value;
                OnPropertyChanged();
            }
        }

        public int CurrentFeatures
        {
            get => _currentFeatures;
            set
            {
                _currentFeatures = value;
                OnPropertyChanged();
            }
        }

        public bool IsMicEnumerationSupported
        {
            get => _isMicEnumerationSupported;
            set
            {
                _isMicEnumerationSupported = value;
                OnPropertyChanged();
            }
        }

        public bool IsMicEnumerationOn
        {
            get => _isMicEnumerationOn;
            set
            {
                _isMicEnumerationOn = value;
                OnPropertyChanged();
            }
        }

        public bool IsWindowsHelloSupported
        {
            get => _isWindowsHelloSupported;
            set
            {
                _isWindowsHelloSupported = value;
                OnPropertyChanged();
            }
        }

        public bool HasWindowsHelloPowerConstraint
        {
            get => _hasWindowsHelloPowerConstraint;
            set
            {
                _hasWindowsHelloPowerConstraint = value;
                OnPropertyChanged();
            }
        }

        #endregion Webcam Properties

        #region Pen Properties

        public bool IsBLE
        {
            get => _isBLE;
            set
            {
                _isBLE = value;
                OnPropertyChanged();
            }
        }

        public string IsdDriverVersion
        {
            get => _isdDriverVersion;
            set
            {
                _isdDriverVersion = value;
                OnPropertyChanged();
            }
        }

        public string IsdServiceVersion
        {
            get => _isdServiceVersion;
            set
            {
                _isdServiceVersion = value;
                OnPropertyChanged();
            }
        }

        public int MonitorCount
        {
            get => _monitorCount;
            set
            {
                _monitorCount = value;
                OnPropertyChanged();
            }
        }

        public byte[] DockData
        {
            get => _dockData;
            set
            {
                _dockData = value;
                OnPropertyChanged();
            }
        }

        public byte[] DockInfo
        {
            get => _dockInfo;
            set
            {
                _dockInfo = value;
                OnPropertyChanged();
            }
        }

        public int DockType
        {
            get => _dockType;
            set
            {
                _dockType = value;
                OnPropertyChanged();
            }
        }

        public string DockServiceTag
        {
            get => _dockService;
            set
            {
                _dockService = value;
                OnPropertyChanged();
            }
        }

        public string DockPackageFwVersion
        {
            get => _dockPackageFwVersion;
            set
            {
                _dockPackageFwVersion = value;
                OnPropertyChanged();
            }
        }

        public int DockFwUpdateStatus
        {
            get => _dockFwUpdateStatus;
            set
            {
                _dockFwUpdateStatus = value;
                OnPropertyChanged();
            }
        }

        public int DockTBTConnectionStatus
        {
            get => _dockTBTConnectionStatus;
            set
            {
                _dockTBTConnectionStatus = value;
                OnPropertyChanged();
            }
        }

        #endregion Pen Properties

        #endregion Properties

        #region Methods

        public DeviceInfo()
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

        #endregion Methods
    }

    public class DockData
    {
        public int BoardId { get; set; }
        public int Configuration { get; set; }
        public int DockType { get; set; }
        public string MarketingName { get; set; }
        public string ModuleSerialNumber { get; set; }
        public int ModuleType { get; set; }
        public string OriginalModuleSerialNumber { get; set; }
        public int PackageFirmwareVersion { get; set; }
        public int PowerSupplyWattage { get; set; }
        public string ServiceTag { get; set; }
        public int Status { get; set; }
    }
}