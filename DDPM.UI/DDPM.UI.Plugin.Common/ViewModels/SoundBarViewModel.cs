using DDPM.SA.Common;
using DDPM.UI.Common;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.UX.WPF;
using Microsoft;
using Newtonsoft.Json.Linq;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DDPM.UI.Plugin.ViewModels
{
    public class SoundBarViewModel : PeripheralViewModel, INotifyPropertyChanged
    {
        #region Variables

        private readonly ILog _log;
        public IDeviceManagerSA _deviceManager;

        #endregion Variables

        public new event PropertyChangedEventHandler? PropertyChanged;

        public string modelTest;

        public SoundBarViewModel(IConsole console, ILog log, IDeviceManagerSA deviceManager) : base(console, log, deviceManager)
        {
            Requires.NotNull(console, nameof(console));
            Requires.NotNull(log, nameof(log));

            _log = log;
            _deviceManager = deviceManager;
        }

        public void DetectPageShow(string model)
        {
            modelTest = model;
            switch (model.ToUpper())
            {
                case "SB522A":
                    _controlGoogleMeetButtonShow = false;
                    _controlSkypeforBusinessButtonShow = false;
                    break;

                default:
                    break;
            }
            CheckAudioSettingsUI();
        }

        private void CheckAudioSettingsUI()
        {           
            _isIntelligentMicNoiseCancellationStatus = _deviceManager.GetIsWiredAudioIMicNSEnableAsync(CurrentDeviceInfo!.ID.ToString()).Result;//CurrentDeviceInfo!.IsWiredAudioIMicNSEnable;
            _isMuteSoundNotificationStatus = _deviceManager.GetIsWiredAudioMicMuteSoundEnableAsync(CurrentDeviceInfo!.ID.ToString()).Result;//CurrentDeviceInfo!.IsWiredAudioMicMuteSoundEnable;
            _isVolumeAdjustmentToneMode = _deviceManager.GetWiredAudioVolumeAdjustmentToneAsync(CurrentDeviceInfo!.ID.ToString()).Result;//CurrentDeviceInfo!.WiredAudioVolumeAdjustmentTone;

            if (_isVolumeAdjustmentToneMode == 3)
            {
                _volumeAdjustmentToneStatus = false;
                _isEveryLevelChecked = false;
                _isMinMaxOnlyChecked = false;
            }
            else if (_isVolumeAdjustmentToneMode == 2)
            {
                _volumeAdjustmentToneStatus = true;
                _isEveryLevelChecked = false;
                _isMinMaxOnlyChecked = true;
            }
            else
            {
                _volumeAdjustmentToneStatus = true;
                _isEveryLevelChecked = true;
                _isMinMaxOnlyChecked = false;
            }
            OnPropertyChanged("IntelligentMicNoiseCancellationStatus");
            OnPropertyChanged("MuteSoundNotificationStatus");
            OnPropertyChanged("VolumeAdjustmentToneStatus");
            OnPropertyChanged("VolumeAdjustmentTone_String");
            OnPropertyChanged("IsEveryLevelChecked");
            OnPropertyChanged("IsMinMaxOnlyChecked");
        }

        public void ChangeImage(string model, string btnName)
        {
            if (model == "SP3022")
            {
                switch (btnName)
                {
                    case "MicrosoftTeams":
                        ImageFilePath = "/DDPM.UI.Common;component/Resources/Speaker_SP3022_AllLight.png";
                        break;

                    case "Zoom":
                        ImageFilePath = "/DDPM.UI.Common;component/Resources/Speaker_SP3022_FourLight.png";
                        break;

                    case "GoogleMeet":
                        ImageFilePath = "/DDPM.UI.Common;component/Resources/Speaker_SP3022_TwoLight.png";
                        break;

                    case "SkypeforBusiness":
                        ImageFilePath = "/DDPM.UI.Common;component/Resources/Speaker_SP3022_RedLight.png";
                        break;

                    default:
                        ImageFilePath = "/DDPM.UI.Common;component/Resources/Speaker_SP3022.png";
                        break;
                }
            }
            else
            {
                switch (btnName)
                {
                    case "MicrosoftTeams":
                        ImageFilePath = "/DDPM.UI.Common;component/Resources/Speaker_SB522A_AllLight.png";
                        break;

                    case "Zoom":
                        ImageFilePath = "/DDPM.UI.Common;component/Resources/Speaker_SB522A_TwoLight.png";
                        break;

                    default:
                        ImageFilePath = "/DDPM.UI.Common;component/Resources/Speaker_SB522A.png";
                        break;
                }
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
                if (deviceInfo.LogicalDeviceType.Contains("LogicalWiredAudio"))
                    DeviceInfos.Add(deviceInfo.ID, deviceInfo);
            }
        }

        public override bool SetCurrentDevice(string deviceID)
        {
            deviceID ??= DeviceInfos.Values.ToList().FirstOrDefault()!.ID.ToString();

            if (!base.SetCurrentDevice(deviceID))
                return false;
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
                            default:
                                break;
                        }
                        //GenerateInfo();
                    }
                    break;

                default:
                    break;
            }
        }
        public void RestoreToDefault()
        {
            _deviceManager.SetResetToDefaultAsyncForSoundbar(CurrentDeviceInfo!.ID.ToString(), true).Wait();
            //IsRestoreEnable = false;
            //OnPropertyChanged(nameof(IsRestoreEnable));
        }


        /// <summary>
        /// Set Bit Value
        /// </summary>
        /// <param name="number">Were detect (2Byte)</param>
        /// <param name="startBitPosition">Bit Position</param>
        /// <param name="value">value</param>
        /// <returns>return set value</returns>
        private uint SetBitValue(uint number, int startBitPosition, int value)
        {
            uint mask = 0b1u << startBitPosition;//Create mask to clear two bits at the specified position
            number &= ~mask;// Clear two bits at the specified position
            number |= (uint)(value << startBitPosition);// Set the new value
            return number;
        }

        /// <summary>
        /// Set Bits Value
        /// </summary>
        /// <param name="number">Were detect (2Byte)</param>
        /// <param name="startBitPosition">Bit Position</param>
        /// <param name="value">value</param>
        /// <returns>return set value</returns>
        private uint SetBitsValue(uint number, int startBitPosition, int value)
        {
            uint mask = 0b11u << startBitPosition;//Create mask to clear two bits at the specified position
            number &= ~mask;// Clear two bits at the specified position
            number |= (uint)(value << startBitPosition);// Set the new value
            return number;
        }

        /// <summary>
        /// Get Bits Value
        /// </summary>
        /// <param name="number">status</param>
        /// <param name="startBitPosition">Bit Position</param>
        /// <returns>return BitPosition value</returns>
        private uint GetBitValue(uint number, int startBitPosition)
        {
            uint bitValue = ((number >> startBitPosition) & 0b1u);// Get startBitPosition和startBitPosition+1 value
            return bitValue;
        }

        /// <summary>
        /// Get Bits Value
        /// </summary>
        /// <param name="number">status</param>
        /// <param name="startBitPosition">Bit Position</param>
        /// <returns>return BitPosition value</returns>
        private uint GetBitsValue(uint number, int startBitPosition)
        {
            uint bitValue = ((number >> startBitPosition) & 0b11u);// Get startBitPosition和startBitPosition+1 value
            return bitValue;
        }

        #region SpeakerAudioPreset

        private bool _isDefaultChecked;

        public bool IsDefaultChecked
        {
            get { return _isDefaultChecked; }
            set
            {
                if (_isDefaultChecked != value)
                {
                    _isDefaultChecked = value;
                    if (_isDefaultChecked)
                    {
                        IsSpeechChecked = false;
                        IsBassBoostChecked = false;
                        IsTrebleBoostChecked = false;
                    }
                    OnPropertyChanged();
                }
            }
        }

        private bool _isSpeechChecked;

        public bool IsSpeechChecked
        {
            get { return _isSpeechChecked; }
            set
            {
                if (_isSpeechChecked != value)
                {
                    _isSpeechChecked = value;
                    if (_isSpeechChecked)
                    {
                        IsDefaultChecked = false;
                        IsBassBoostChecked = false;
                        IsTrebleBoostChecked = false;
                    }
                    OnPropertyChanged();
                }
            }
        }

        private bool _isBassBoostChecked;

        public bool IsBassBoostChecked
        {
            get { return _isBassBoostChecked; }
            set
            {
                if (_isBassBoostChecked != value)
                {
                    _isBassBoostChecked = value;
                    if (_isBassBoostChecked)
                    {
                        IsDefaultChecked = false;
                        IsSpeechChecked = false;
                        IsTrebleBoostChecked = false;
                    }
                    OnPropertyChanged();
                }
            }
        }

        private bool _isTrebleBoostChecked;

        public bool IsTrebleBoostChecked
        {
            get { return _isTrebleBoostChecked; }
            set
            {
                if (_isTrebleBoostChecked != value)
                {
                    _isTrebleBoostChecked = value;
                    if (_isTrebleBoostChecked)
                    {
                        IsDefaultChecked = false;
                        IsSpeechChecked = false;
                        IsBassBoostChecked = false;
                    }
                    OnPropertyChanged();
                }
            }
        }

        #endregion SpeakerAudioPreset

        #region SpeakerAudioSettings

        private bool _isIntelligentMicNoiseCancellationStatus = false;

        public bool IntelligentMicNoiseCancellationStatus
        {
            get
            {
                //_isIntelligentMicNoiseCancellationStatus = _deviceManager.GetIsWiredAudioIMicNSEnableValue(CurrentDeviceInfo!.ID.ToString()).Result;//CurrentDeviceInfo!.IsWiredAudioIMicNSEnable;
                return _isIntelligentMicNoiseCancellationStatus;
            }
            set
            {
                if (_isIntelligentMicNoiseCancellationStatus != value)
                {
                    _deviceManager.SetIsWiredAudioIMicNSEnableAsync(CurrentDeviceInfo!.ID.ToString(), value).Wait();
                    //_deviceManager.SetWiredAudioIMicNSEnable(value, CurrentDeviceInfo!.ID).Wait();
                    _isIntelligentMicNoiseCancellationStatus = value;
                    OnPropertyChanged("IntelligentMicNoiseCancellation_String");
                }
            }
        }

        private string _isIntelligentMicNoiseCancellation_String = "ON";

        public string IntelligentMicNoiseCancellation_String
        {
            get => _isIntelligentMicNoiseCancellationStatus ? "ON" : "OFF";
        }

        private bool _isMuteSoundNotificationStatus = false;

        public bool MuteSoundNotificationStatus
        {
            get
            {
                //_isMuteSoundNotificationStatus = CurrentDeviceInfo!.IsWiredAudioMicMuteSoundEnable;
                //_isMuteSoundNotificationStatus = _deviceManager.GetIsWiredAudioMicMuteSoundEnableAsync(CurrentDeviceInfo!.ID.ToString()).Result;
                return _isMuteSoundNotificationStatus;
            }
            set
            {
                if (_isMuteSoundNotificationStatus != value)
                {
                    _deviceManager.SetIsWiredAudioMicMuteSoundEnableAsync(CurrentDeviceInfo!.ID.ToString(), value).Wait();
                    _isMuteSoundNotificationStatus = value;
                    OnPropertyChanged("MuteSoundNotification_String");
                }
            }
        }

        private string _isMuteSoundNotification_String = "ON";

        public string MuteSoundNotification_String
        {
            get => _isMuteSoundNotificationStatus ? "ON" : "OFF";
        }

        private int _isVolumeAdjustmentToneMode;
        private bool _volumeAdjustmentToneStatus;

        public bool VolumeAdjustmentToneStatus
        {
            get
            {
                return _volumeAdjustmentToneStatus;
            }
            set
            {
                if (value)
                {
                    _volumeAdjustmentToneStatus = true;
                    _isEveryLevelChecked = true;
                    _isMinMaxOnlyChecked = false;
                    _isVolumeAdjustmentToneMode = 1;
                    //_deviceManager.SetWiredAudioVolumeAdjustmentTone(1, CurrentDeviceInfo!.ID).Wait();
                    _deviceManager.SetWiredAudioVolumeAdjustmentToneAsync(CurrentDeviceInfo!.ID.ToString(), 1).Wait();
                    OnPropertyChanged("VolumeAdjustmentToneStatus");
                    OnPropertyChanged("VolumeAdjustmentTone_String");
                    OnPropertyChanged("IsEveryLevelChecked");
                    OnPropertyChanged("IsMinMaxOnlyChecked");
                }
                if (!value)
                {
                    _isVolumeAdjustmentToneMode = 3;
                    //_deviceManager.SetWiredAudioVolumeAdjustmentTone(3, CurrentDeviceInfo!.ID).Wait();
                    _deviceManager.SetWiredAudioVolumeAdjustmentToneAsync(CurrentDeviceInfo!.ID.ToString(), 3).Wait();
                    _volumeAdjustmentToneStatus = false;
                    _isEveryLevelChecked = false;
                    _isMinMaxOnlyChecked = false;
                    OnPropertyChanged("VolumeAdjustmentToneStatus");
                    OnPropertyChanged("VolumeAdjustmentTone_String");
                    OnPropertyChanged("IsEveryLevelChecked");
                    OnPropertyChanged("IsMinMaxOnlyChecked");
                }
            }
        }

        private string _volumeAdjustmentToneString = "ON";

        public string VolumeAdjustmentTone_String
        {
            get
            {
                return _volumeAdjustmentToneStatus ? "ON" : "OFF";
            }
        }

        private bool _isEveryLevelChecked;

        public bool IsEveryLevelChecked
        {
            get
            {
                return _isEveryLevelChecked;
            }
            set
            {
                if (_isVolumeAdjustmentToneMode == 3)
                {
                    return;
                }
                if (_isEveryLevelChecked != value)
                {
                    _isEveryLevelChecked = true;
                    _isMinMaxOnlyChecked = false;
                    _isVolumeAdjustmentToneMode = 1;
                    //_deviceManager.SetWiredAudioVolumeAdjustmentTone(1, CurrentDeviceInfo!.ID).Wait();
                    _deviceManager.SetWiredAudioVolumeAdjustmentToneAsync(CurrentDeviceInfo!.ID.ToString(), 1).Wait();
                    OnPropertyChanged("IsEveryLevelChecked");
                    OnPropertyChanged("IsMinMaxOnlyChecked");
                }
            }
        }

        private bool _isMinMaxOnlyChecked;

        public bool IsMinMaxOnlyChecked
        {
            get
            {
                return _isMinMaxOnlyChecked;
            }
            set
            {
                if (_isVolumeAdjustmentToneMode == 3)
                {
                    return;
                }
                if (_isMinMaxOnlyChecked != value)
                {
                    _isEveryLevelChecked = false;
                    _isMinMaxOnlyChecked = true;
                    _isVolumeAdjustmentToneMode = 2;
                    //_deviceManager.SetWiredAudioVolumeAdjustmentTone(2, CurrentDeviceInfo!.ID).Wait();
                    _deviceManager.SetWiredAudioVolumeAdjustmentToneAsync(CurrentDeviceInfo!.ID.ToString(), 2).Wait();
                    OnPropertyChanged("IsEveryLevelChecked");
                    OnPropertyChanged("IsMinMaxOnlyChecked");
                }
            }
        }

        #endregion SpeakerAudioSettings

        #region SpeakerAudioSettings ToolTip

        private string _intelligentMicNoiseCancellationToolTip = Strings.SpeakerToolTip_1;//"Removes background noise to allow your voice to be heard clearly";

        public string IntelligentMicNoiseCancellationToolTip
        {
            get => _intelligentMicNoiseCancellationToolTip;
        }

        private string _muteSoundNotificationToolTip = Strings.SpeakerToolTip_2;//"Plays a sound when the device goes on mute";

        public string MuteSoundNotificationToolTip
        {
            get => _muteSoundNotificationToolTip;
        }

        private string _volumeAdjustmentToneToolTip = Strings.SpeakerToolTip_3;//"Plays a sound when the volume level is adjusted";

        public string VolumeAdjustmentToneToolTip
        {
            get => _volumeAdjustmentToneToolTip;
        }

        #endregion SpeakerAudioSettings ToolTip

        #region SpeakerInteractions

        private bool _isMicrosoftTeamsChecked;

        public bool IsMicrosoftTeamsChecked
        {
            get { return _isMicrosoftTeamsChecked; }
            set
            {
                if (_isMicrosoftTeamsChecked != value)
                {
                    _isMicrosoftTeamsChecked = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _isZoomChecked;

        public bool IsZoomChecked
        {
            get { return _isZoomChecked; }
            set
            {
                if (_isZoomChecked != value)
                {
                    _isZoomChecked = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _isGoogleMeetChecked;

        public bool IsGoogleMeetChecked
        {
            get { return _isGoogleMeetChecked; }
            set
            {
                if (_isGoogleMeetChecked != value)
                {
                    _isGoogleMeetChecked = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _isSkypeForBusinessChecked;

        public bool IsSkypeForBusinessChecked
        {
            get { return _isSkypeForBusinessChecked; }
            set
            {
                if (_isSkypeForBusinessChecked != value)
                {
                    _isSkypeForBusinessChecked = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _controlGoogleMeetButtonShow = true;

        public bool ControlGoogleMeetButtonShow
        {
            get => _controlGoogleMeetButtonShow;
            set
            {
                _controlGoogleMeetButtonShow = value;
                OnPropertyChanged(nameof(ControlGoogleMeetButtonShow));
            }
        }

        private bool _controlSkypeforBusinessButtonShow = true;

        public bool ControlSkypeforBusinessButtonShow
        {
            get => _controlSkypeforBusinessButtonShow;
            set
            {
                _controlSkypeforBusinessButtonShow = value;
                OnPropertyChanged(nameof(ControlSkypeforBusinessButtonShow));
            }
        }

        #endregion SpeakerInteractions
    }
}