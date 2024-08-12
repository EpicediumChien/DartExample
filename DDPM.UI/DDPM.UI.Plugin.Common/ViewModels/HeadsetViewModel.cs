using DDPM.SA.Common;
using DDPM.UI.Plugin.ViewModels;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.UX.WPF;
using Microsoft;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Controls;
using System.Windows.Input;
using DDPM.UI.Common.Interfaces;
using DDPM.UI.Common;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Windows.Threading;

namespace DDPM.UI.Plugin.ViewModels
{
    public class HeadsetViewModel : PeripheralViewModel, INotifyPropertyChanged
    {
        #region Variables
        private readonly ILog _log;
        public IDeviceManagerSA _deviceManager;
        //private string _current_headset;
        #endregion

        public new event PropertyChangedEventHandler? PropertyChanged;
        //public string modelTest;
        public HeadsetViewModel(IConsole console, ILog log, IDeviceManagerSA deviceManager) : base(console, log, deviceManager)
        {

            Requires.NotNull(console, nameof(console));
            Requires.NotNull(log, nameof(log));

            _log = log;
            _deviceManager = deviceManager;
        }

        public void DetectPageShow(string model)
        {
            //modelTest = model;
            switch (model.ToUpper())
            {
                case "WL7024"://Mito
                    //Page 1
                    _controlTheNoiseIHearPageShow = true;
                    _configureMyAudioModesPageShow = true;
                    //_sidetonePageShow = false;
                    //Page 2                   
                    _wearDetectionPageShow = true;
                    _automatedActionsWhenHeadsetIsRemovedPageShow = true;
                    _automatedActionsQuickPausePageShow = true;
                    _automatedActionsSensitivityPageShow = true;
                    //Page 3
                    _voiceGuidancePageShow = true;
                    _deviceSettingsDownloadDellAudioPageShow = true;
                    break;
                case "WL5024"://Pegasus
                    //Page 1
                    _controlTheNoiseIHearPageShow = true;
                    _configureMyAudioModesPageShow = true;
                    //Page 2
                    _wearDetectionPageShow = true;
                    _automatedActionsSensitivityUpPageShow = true;
                    _automatedActionsWhenHeadsetIsRemovedPageShow = true;
                    _automatedActionsAnswerCallPageShow = true;
                    //Page 3
                    _voiceGuidancePageShow = true;
                    _deviceSettingsDownloadDellAudioPageShow = true;
                    break;
                case "WH5024"://Winflo
                    //Page 1
                    _controlTheNoiseIHearPageShow = true;
                    _configureMyAudioModesPageShow = true;
                    //Page 2
                    _automatedActionsAnswerCallPageShow = true;
                    //Page 3
                    _voiceGuidancePageShow = true;
                    break;
                case "WL3024"://Vaporify
                    //Page 1
                    _configureMyAudioModesPageShow = true;
                    //Page 2
                    _automatedActionsAnswerCallPageShow = true;
                    //Page 3
                    _voiceGuidancePageShow = true;
                    _deviceSettingsDownloadDellAudioPageShow = true;
                    break;
                case "WH3024"://Airmax
                    //Page 1
                    _configureMyAudioModesPageShow = true;
                    //Page 2
                    _automatedActionsAnswerCallPageShow = true;
                    //Page 3
                    //defult page
                    break;
                default:
                    break;
            }
            CheckHeadsetFunc();
        }
        public void CheckHeadsetFunc()
        {
            CheckSidetoneUI(false);
            CheckBusyLightUI(false);
            CheckMicNCIncomingUI(false);
            CheckMicNoiseCancellationUI(false);
            CheckWearDetectionUI(false);
            CheckPresetsUI(false);
            CheckVoiceGuidanceUI(false);
            CheckANCUI(false);
        }
        private void CheckSidetoneUI(bool PropertyChange)
        {
            if (CurrentDeviceInfo!.IsSidetoneSupported)
            {
                _isSidetoneStatus = CurrentDeviceInfo.Sidetone;

                if (PropertyChange)
                {
                    OnPropertyChanged("SidetoneStatus");
                    OnPropertyChanged("Sidetone_String");
                    OnPropertyChanged("SidetoneSliderStatus");
                    UpdateCollaborationAndultimediaUI(true, false);
                }
            }
        }
        private void CheckSidetoneLevelUI(bool PropertyChange)
        {
            if (CurrentDeviceInfo!.IsSidetoneSupported)
            {
                if (_isidetoneSliderValue != CurrentDeviceInfo.SidetoneLevel)
                {
                    _isidetoneSliderValue = CurrentDeviceInfo.SidetoneLevel;

                    if (PropertyChange)
                    {
                        OnPropertyChanged("SidetoneSliderValue");
                        OnPropertyChanged("SidetoneSliderStatus");
                        UpdateCollaborationAndultimediaUI(true, false);
                    }
                }
            }
        }
        private void CheckBusyLightUI(bool PropertyChange)
        {
            if (CurrentDeviceInfo!.IsBusyLightSupported)
                _isBusyLightStatus = CurrentDeviceInfo.BusyLight;
            if (PropertyChange)
            {
                OnPropertyChanged("BusyLightStatus");
                OnPropertyChanged("BusyLight_String");
            }
        }

        private void CheckMicNCIncomingUI(bool PropertyChange)
        {
            if (CurrentDeviceInfo!.IsMicNCIncomingSupported)
            {
                if (_isIncomingAudioStatus != CurrentDeviceInfo.MicNCIncoming)
                {
                    _isIncomingAudioStatus = CurrentDeviceInfo.MicNCIncoming;

                    if (PropertyChange)
                    {
                        OnPropertyChanged("IncomingAudioStatus");
                        OnPropertyChanged("IncomingAudio_String");
                        UpdateCollaborationAndultimediaUI(true, false);
                    }
                }
            }
        }
        private void CheckMicNoiseCancellationUI(bool PropertyChange)
        {
            if (CurrentDeviceInfo!.IsMicNoiseCancellationSupported)
                _isMicNoiseCancellationStatus = CurrentDeviceInfo.MicNoiseCancellation;

            if (PropertyChange)
            {
                OnPropertyChanged("MicNoiseCancellationStatus");
                OnPropertyChanged("MicNoiseCancellation_String");
                UpdateCollaborationAndultimediaUI(true, false);
            }
        }
        private void CheckWearDetectionUI(bool PropertyChange)
        {
            if (CurrentDeviceInfo!.IsWearDetectionSupported)
            {
                uint wearDetectionValue = (uint)CurrentDeviceInfo!.WearDetection;
                if (GetBitValue(wearDetectionValue, 0) == 1)
                    _isWearDetectionStatus = true;
                else
                    _isWearDetectionStatus = false;

                if (GetBitValue(wearDetectionValue, 1) == 1)
                    _isPauseMusicStatus = true;
                else
                    _isPauseMusicStatus = false;

                if (GetBitValue(wearDetectionValue, 2) == 1)
                    _isMuteMicrophoneStatus = true;
                else
                    _isMuteMicrophoneStatus = false;

                if (GetBitValue(wearDetectionValue, 3) == 1)
                {
                    _isNormal2Checked = true;
                    _isLowChecked = false;
                }
                else
                {
                    _isNormal2Checked = false;
                    _isLowChecked = true;
                }

                if (GetBitValue(wearDetectionValue, 4) == 1)
                    _isQuickPauseStatus = true;
                else
                    _isQuickPauseStatus = false;

                if (GetBitsValue(wearDetectionValue, 5) == 1)
                {
                    _isNormalChecked = true;
                    _isSensitiveChecked = false;
                }
                else
                {
                    _isNormalChecked = false;
                    _isSensitiveChecked = true;
                }

                if (PropertyChange)
                {
                    OnPropertyChanged("WearDetectionStatus");
                    OnPropertyChanged("WearDetection_String");

                    OnPropertyChanged("PauseMusicStatus");
                    OnPropertyChanged("PauseMusic_String");

                    OnPropertyChanged("MuteMicrophoneStatus");
                    OnPropertyChanged("MuteMicrophone_String");

                    OnPropertyChanged("IsNormal2Checked");
                    OnPropertyChanged("IsLowChecked");

                    OnPropertyChanged("QuickPauseStatus");
                    OnPropertyChanged("QuickPause_String");

                    OnPropertyChanged("IsNormalChecked");
                    OnPropertyChanged("IsSensitiveChecked");
                }
            }
        }
        private void CheckPresetsUI(bool PropertyChange)
        {
            if (CurrentDeviceInfo!.IsPresetsSupported)
            {
                switch (CurrentDeviceInfo.SelectedPreset)
                {
                    case 1:
                        _isDefaultChecked = true;
                        _isBassBoostChecked = false;
                        _isSpeechBoostChecked = false;
                        _isTrebleBoostChecked = false;
                        _isCustomChecked = false;
                        break;
                    case 2:
                        _isDefaultChecked = false;
                        _isBassBoostChecked = true;
                        _isSpeechBoostChecked = false;
                        _isTrebleBoostChecked = false;
                        _isCustomChecked = false;
                        break;
                    case 3:
                        _isDefaultChecked = false;
                        _isBassBoostChecked = false;
                        _isSpeechBoostChecked = true;
                        _isTrebleBoostChecked = false;
                        _isCustomChecked = false;
                        break;
                    case 4:
                        _isDefaultChecked = false;
                        _isBassBoostChecked = false;
                        _isSpeechBoostChecked = false;
                        _isTrebleBoostChecked = true;
                        _isCustomChecked = false;
                        break;
                    case 101:
                        _isDefaultChecked = false;
                        _isBassBoostChecked = false;
                        _isSpeechBoostChecked = false;
                        _isTrebleBoostChecked = false;
                        _isCustomChecked = true;
                        break;
                    default:
                        break;
                }

                if (PropertyChange)
                {
                    UpdateCollaborationAndultimediaUI(false, true);
                    OnPropertyChanged("IsCollaborationChecked");
                    OnPropertyChanged("IsMultimediaChecked");
                    OnPropertyChanged("IsDefaultChecked");
                    OnPropertyChanged("IsBassBoostChecked");
                    OnPropertyChanged("IsSpeechBoostChecked");
                    OnPropertyChanged("IsTrebleBoostChecked");
                    OnPropertyChanged("IsCustomChecked");
                }

            }
        }
        private void CheckVoiceGuidanceUI(bool PropertyChange)
        {
            if (CurrentDeviceInfo!.VoiceGuidance)
            {
                _isAllChecked = true;
                _isEssentialChecked = false;
            }
            else
            {
                _isAllChecked = false;
                _isEssentialChecked = true;
            }

            if (PropertyChange)
            {
                OnPropertyChanged("IsAllChecked");
                OnPropertyChanged("IsEssentialChecked");
            }
        }
        private void CheckANCUI(bool PropertyChange)
        {
            if (CurrentDeviceInfo!.IsANCSupported)
            {
                switch (CurrentDeviceInfo.AncMode)
                {
                    case 0:
                        _isNoiseOffChecked = true;
                        _isActiveNoiseCancellingChecked = false;
                        _isTransparencyChecked = false;
                        break;
                    case 1:
                        _isNoiseOffChecked = false;
                        _isActiveNoiseCancellingChecked = true;
                        _isTransparencyChecked = false;
                        break;
                    case 2:
                        _isNoiseOffChecked = false;
                        _isActiveNoiseCancellingChecked = false;
                        _isTransparencyChecked = true;
                        _isTransparencylevelSliderValue = CurrentDeviceInfo.AncGain;
                        break;
                }

                if (PropertyChange)
                {
                    OnPropertyChanged("IsNoiseOffChecked");
                    OnPropertyChanged("IsActiveNoiseCancellingChecked");
                    OnPropertyChanged("IsTransparencyChecked");
                    OnPropertyChanged("TransparencylevelSliderValue");
                }
            }
        }
        private void UpdateCollaborationAndultimediaUI(bool Collaboration, bool Multimedia)
        {
            _isCollaborationChecked = Collaboration;
            _isMultimediaChecked = Multimedia;
            OnPropertyChanged("IsCollaborationChecked");
            OnPropertyChanged("IsMultimediaChecked");
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
                if (deviceInfo.LogicalDeviceType.Contains("Headset"))
                    DeviceInfos.Add(deviceInfo.ID, deviceInfo);
            }
        }
        public override bool SetCurrentDevice(string deviceID)
        {
            deviceID ??= DeviceInfos.Values.ToList().FirstOrDefault()!.ID.ToString();

            if (!base.SetCurrentDevice(deviceID))
                return false;
            //else
            //    _current_headset = deviceID;
            return true;
        }
        public override void HandleNotification(DeviceChangedType changeType, DeviceInfo di, string property = "")
        {
            base.HandleNotification(changeType, di, property);
            //CheckHeadsetFunc();
            switch (property)
            {
                //case "IsReadyChanged":
                //    break;
                //case "IsDirtyChanged":
                //    break;
                case "MicNoiseCancellationChanged":
                    CheckMicNoiseCancellationUI(true);
                    break;
                case "MicNCIncomingChanged":
                    CheckMicNCIncomingUI(true);
                    break;
                case "SidetoneChanged":
                    CheckSidetoneUI(true);
                    break;
                case "BusyLightChanged":
                    CheckBusyLightUI(true);
                    break;
                case "VoiceGuidanceChanged":
                    CheckVoiceGuidanceUI(true);
                    break;
                case "SelectedPresetChanged":
                    CheckPresetsUI(true);
                    break;
                case "SidetoneLevelChanged":
                    CheckSidetoneLevelUI(true);
                    break;
                //case "MuteStatusChanged":
                //    break;
                case "BandsGainChanged":

                    break;
                case "AncModeChanged":
                    CheckANCUI(true);
                    break;
                case "AncGainChanged":
                    break;
                case "WearDetectionChanged":
                    CheckWearDetectionUI(true);
                    break;
                default:
                    break;
            }
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
                        GenerateInfo();
                    }
                    break;
                default:
                    break;
            }
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

        #region Please Wait
        private bool _isPleaseWaitVisible;
        public bool IsPleaseWaitVisible
        {
            get => _isPleaseWaitVisible;
            set
            {
                if (_isPleaseWaitVisible != value)
                {
                    _isPleaseWaitVisible = value;
                    OnPropertyChanged(nameof(IsPleaseWaitVisible));
                }
            }
        }

        public void ShowPleaseWait()
        {
            IsPleaseWaitVisible = true;
        }

        public void HidePleaseWait()
        {
            IsPleaseWaitVisible = false;
        }
        // Please Wait logic
        public void Invoke_PleaseWait(string model)
        {
            BackgroundWorker bw = new BackgroundWorker
            {
                WorkerReportsProgress = false,
                WorkerSupportsCancellation = false
            };
            bw.DoWork += (sender, e) => DoWork_PleaseWait(model);
            bw.RunWorkerCompleted += RunWorkerCompleted_PleaseWait;

            ShowPleaseWait();
            bw.RunWorkerAsync();
        }

        private void DoWork_PleaseWait(string model)
        {
            // Simulate time-consuming operation
            Thread.Sleep(1000);

            // Call DetectPageShow
            DetectPageShow(model);
        }

        private void RunWorkerCompleted_PleaseWait(object sender, RunWorkerCompletedEventArgs e)
        {
            HidePleaseWait();
        }

        #endregion

        /// <summary>
        /// HeadsetAudioSettings Page
        /// </summary>

        #region HeadsetAudioSettings ToggleSwitch Binding
        //Outgoing Audio ToggleSwitch 
        private string _isOutgoingAudio_String = "ON";
        public string OutgoingAudio_String
        {
            get => _isOutgoingAudioStatus ? "ON" : "OFF";
        }

        private bool _isOutgoingAudioStatus = false;
        public bool OutgoingAudioStatus
        {
            get
            {
                return _isOutgoingAudioStatus;
            }
            set
            {
                _isOutgoingAudioStatus = value;
                OnPropertyChanged("OutgoingAudio_String");
            }
        }

        //Incoming Audio ToggleSwitch 
        private string _isIncomingAudio_String = "ON";
        public string IncomingAudio_String
        {
            get => _isIncomingAudioStatus ? "ON" : "OFF";
        }

        private bool _isIncomingAudioStatus = false;
        public bool IncomingAudioStatus
        {
            get
            {
                _isIncomingAudioStatus = CurrentDeviceInfo!.MicNCIncoming;
                return _isIncomingAudioStatus;
            }
            set
            {
                _deviceManager.SetMicNCIncoming(value, CurrentDeviceInfo!.ID).Wait();
                _isIncomingAudioStatus = value;
                OnPropertyChanged("IncomingAudio_String");
            }
        }

        //MicNoiseCancellation ToggleSwitch
        private string _isMicNoiseCancellation_String = "ON";
        public string MicNoiseCancellation_String
        {
            get => _isMicNoiseCancellationStatus ? "ON" : "OFF";
        }

        private bool _isMicNoiseCancellationStatus;// = false;
        public bool MicNoiseCancellationStatus
        {
            get
            {
                _isMicNoiseCancellationStatus = CurrentDeviceInfo!.MicNoiseCancellation;
                return _isMicNoiseCancellationStatus;
            }
            set
            {
                _deviceManager.SetMicNoiseCancellation(value, CurrentDeviceInfo!.ID).Wait();
                _isMicNoiseCancellationStatus = value;
                OnPropertyChanged("MicNoiseCancellation_String");
            }
        }

        //Sidetone ToggleSwitch
        private string _isSidetone_String = "ON";
        public string Sidetone_String
        {
            get => _isSidetoneStatus ? "ON" : "OFF";
        }

        private bool _isSidetoneStatus;// = false;
        public bool SidetoneStatus
        {
            get
            {
                _isSidetoneStatus = CurrentDeviceInfo!.Sidetone;
                return _isSidetoneStatus;
            }
            set
            {
                _deviceManager.SetSidetone(value, CurrentDeviceInfo!.ID).Wait();
                _isSidetoneStatus = value;
                OnPropertyChanged("Sidetone_String");
                OnPropertyChanged("SidetoneSliderStatus");
            }
        }

        private int _isidetoneSliderValue;
        public int SidetoneSliderValue
        {
            get
            {
                _isidetoneSliderValue = CurrentDeviceInfo!.SidetoneLevel;
                return _isidetoneSliderValue;
            }

            set
            {
                _deviceManager.SetSidetoneLevel(value, CurrentDeviceInfo!.ID).Wait();
                _isidetoneSliderValue = value;
                OnPropertyChanged(nameof(SidetoneSliderValue));
            }
        }

        private bool _isSidetoneSliderStatus;// = false;
        public bool SidetoneSliderStatus
        {
            get
            {
                return SidetoneStatus;
            }
            set
            {
                _isSidetoneSliderStatus = SidetoneStatus;
                //OnPropertyChanged(nameof(SidetoneSliderStatus));
            }
        }
        #endregion

        #region HeadsetAudioSettings Grid Show/Hide

        private bool _controlTheNoiseIHearPageShow = false;
        public bool ControlTheNoiseIHearPageShow
        {
            get => _controlTheNoiseIHearPageShow;
            set
            {
                _controlTheNoiseIHearPageShow = value;
                OnPropertyChanged(nameof(ControlTheNoiseIHearPageShow));
            }
        }

        private bool _audioOutputPresetsPageShow = false;
        public bool AudioOutputPresetsPageShow
        {
            get => _audioOutputPresetsPageShow;
            set
            {
                _audioOutputPresetsPageShow = value;
                OnPropertyChanged(nameof(AudioOutputPresetsPageShow));
            }
        }

        private bool _configureMyAudioModesPageShow = false;
        public bool ConfigureMyAudioModesPageShow
        {
            get => _configureMyAudioModesPageShow;
            set
            {
                _configureMyAudioModesPageShow = value;
                OnPropertyChanged(nameof(ConfigureMyAudioModesPageShow));
            }
        }

        private bool _micNoiseCancellationPageShow = false;
        public bool MicNoiseCancellationPageShow
        {
            get => _micNoiseCancellationPageShow;
            set
            {
                _micNoiseCancellationPageShow = value;
                OnPropertyChanged(nameof(MicNoiseCancellationPageShow));
            }
        }

        private bool _micNoiseCancellationFewPageShow = false;
        public bool MicNoiseCancellationFewPageShow
        {
            get => _micNoiseCancellationFewPageShow;
            set
            {
                _micNoiseCancellationFewPageShow = value;
                OnPropertyChanged(nameof(MicNoiseCancellationFewPageShow));
            }
        }

        private bool _sidetonePageShow = false;
        public bool SidetonePageShow
        {
            get => _sidetonePageShow;
            set
            {
                _sidetonePageShow = value;
                OnPropertyChanged(nameof(SidetonePageShow));
            }
        }

        private bool _transparencylevelLinePageShow = false;
        public bool TransparencylevelLinePageShow
        {
            get => _transparencylevelLinePageShow;
            set
            {
                _transparencylevelLinePageShow = value;
                OnPropertyChanged(nameof(TransparencylevelLinePageShow));
            }
        }

        private bool _micNoiseInfoMuchPageShow = false;
        public bool MicNoiseInfoMuchPageShow
        {
            get => _micNoiseInfoMuchPageShow;
            set
            {
                _micNoiseInfoMuchPageShow = value;
                OnPropertyChanged(nameof(_micNoiseInfoMuchPageShow));
            }
        }

        private bool _audioEqualizerGridPageShow = false;
        public bool AudioEqualizerGridPageShow
        {
            get => _audioEqualizerGridPageShow;
            set
            {
                _audioEqualizerGridPageShow = value;
                OnPropertyChanged(nameof(AudioEqualizerGridPageShow));
            }
        }
        #endregion

        #region HeadsetAudioSettingsRightView
        //Group 1
        #region Group 1
        private bool _isActiveNoiseCancellingChecked;
        private bool _isTransparencyChecked;
        private bool _isNoiseOffChecked;

        public bool IsActiveNoiseCancellingChecked
        {
            get
            {
                return _isActiveNoiseCancellingChecked;
            }
            set
            {
                if (_isActiveNoiseCancellingChecked != value)
                {
                    _isActiveNoiseCancellingChecked = value;
                    if (_isActiveNoiseCancellingChecked)
                    {
                        IsTransparencyChecked = false;
                        IsNoiseOffChecked = false;
                    }
                    if (value)
                        _deviceManager.SetAncMode(1, CurrentDeviceInfo!.ID).Wait();
                    OnPropertyChanged(nameof(IsActiveNoiseCancellingChecked));
                }
            }
        }

        public bool IsTransparencyChecked
        {
            get => _isTransparencyChecked;
            set
            {
                if (_isTransparencyChecked != value)
                {
                    _isTransparencyChecked = value;
                    if (_isTransparencyChecked)
                    {
                        IsActiveNoiseCancellingChecked = false;
                        IsNoiseOffChecked = false;
                    }
                    if (value)
                        _deviceManager.SetAncMode(2, CurrentDeviceInfo!.ID).Wait();
                    OnPropertyChanged(nameof(IsTransparencyChecked));
                }
            }
        }

        public bool IsNoiseOffChecked
        {
            get => _isNoiseOffChecked;
            set
            {
                if (_isNoiseOffChecked != value)
                {
                    _isNoiseOffChecked = value;
                    if (_isNoiseOffChecked)
                    {
                        IsActiveNoiseCancellingChecked = false;
                        IsTransparencyChecked = false;
                    }
                    if (value)
                        _deviceManager.SetAncMode(0, CurrentDeviceInfo!.ID).Wait();
                    OnPropertyChanged(nameof(IsNoiseOffChecked));
                }
            }
        }

        private int _isTransparencylevelSliderValue;
        public int TransparencylevelSliderValue
        {
            get
            {
                _isTransparencylevelSliderValue = CurrentDeviceInfo!.AncGain;
                return _isTransparencylevelSliderValue;
            }

            set
            {
                //CurrentDeviceInfo!.AncGain = value;
                _deviceManager.SetAncGain(value, CurrentDeviceInfo!.ID).Wait();
                _isTransparencylevelSliderValue = value;
                OnPropertyChanged(nameof(TransparencylevelSliderValue));
            }
        }
        #endregion
        //Group 2
        #region Group 2
        private bool _isCollaborationChecked = true;
        private bool _isMultimediaChecked;
        public bool IsCollaborationChecked
        {
            get => _isCollaborationChecked;
            set
            {
                if (_isCollaborationChecked != value)
                {
                    _isCollaborationChecked = value;
                    if (_isCollaborationChecked)
                    {
                        IsMultimediaChecked = false;
                    }
                    _deviceManager.SetCollaborationMicEnable(value, CurrentDeviceInfo!.ID).Wait();
                    OnPropertyChanged(nameof(IsCollaborationChecked));
                }
            }
        }

        public bool IsMultimediaChecked
        {
            get => _isMultimediaChecked;
            set
            {
                if (_isMultimediaChecked != value)
                {
                    _isMultimediaChecked = value;
                    if (_isMultimediaChecked)
                    {
                        IsCollaborationChecked = false;
                    }
                    OnPropertyChanged(nameof(IsMultimediaChecked));
                }
            }
        }
        #endregion
        //Group 3
        #region Group 3
        private bool _isDefaultChecked;
        private bool _isBassBoostChecked;
        private bool _isSpeechBoostChecked;
        private bool _isTrebleBoostChecked;
        private bool _isCustomChecked;
        public bool IsDefaultChecked
        {
            get => _isDefaultChecked;
            set
            {
                if (_isDefaultChecked != value)
                {
                    _isDefaultChecked = value;
                    if (_isDefaultChecked)
                    {
                        IsBassBoostChecked = false;
                        IsSpeechBoostChecked = false;
                        IsTrebleBoostChecked = false;
                        IsCustomChecked = false;
                        _deviceManager.SetSelectedPreset(1, CurrentDeviceInfo!.ID).Wait();
                    }
                    OnPropertyChanged(nameof(IsDefaultChecked));
                }
            }
        }
        public bool IsBassBoostChecked
        {
            get => _isBassBoostChecked;
            set
            {
                if (_isBassBoostChecked != value)
                {
                    _isBassBoostChecked = value;
                    if (_isBassBoostChecked)
                    {
                        IsDefaultChecked = false;
                        IsSpeechBoostChecked = false;
                        IsTrebleBoostChecked = false;
                        IsCustomChecked = false;
                        _deviceManager.SetSelectedPreset(2, CurrentDeviceInfo!.ID).Wait();
                    }
                    OnPropertyChanged(nameof(IsBassBoostChecked));
                }
            }
        }
        public bool IsSpeechBoostChecked
        {
            get => _isSpeechBoostChecked;
            set
            {
                if (_isSpeechBoostChecked != value)
                {
                    _isSpeechBoostChecked = value;
                    if (_isSpeechBoostChecked)
                    {
                        IsDefaultChecked = false;
                        IsBassBoostChecked = false;
                        IsTrebleBoostChecked = false;
                        IsCustomChecked = false;
                        _deviceManager.SetSelectedPreset(3, CurrentDeviceInfo!.ID).Wait();
                    }
                    OnPropertyChanged(nameof(IsSpeechBoostChecked));
                }
            }
        }
        public bool IsTrebleBoostChecked
        {
            get => _isTrebleBoostChecked;
            set
            {
                if (_isTrebleBoostChecked != value)
                {
                    _isTrebleBoostChecked = value;
                    if (_isTrebleBoostChecked)
                    {
                        IsDefaultChecked = false;
                        IsBassBoostChecked = false;
                        IsSpeechBoostChecked = false;
                        IsCustomChecked = false;
                        _deviceManager.SetSelectedPreset(4, CurrentDeviceInfo!.ID).Wait();
                    }
                    OnPropertyChanged(nameof(IsTrebleBoostChecked));
                }
            }
        }
        public bool IsCustomChecked
        {
            get => _isCustomChecked;
            set
            {
                if (_isCustomChecked != value)
                {
                    _isCustomChecked = value;
                    if (_isCustomChecked)
                    {
                        IsDefaultChecked = false;
                        IsBassBoostChecked = false;
                        IsSpeechBoostChecked = false;
                        IsTrebleBoostChecked = false;
                        _audioEqualizerGridPageShow = true;
                        //CurrentDeviceInfo!.SelectedPreset = 101;
                        _deviceManager.SetSelectedPreset(101, CurrentDeviceInfo!.ID).Wait();
                    }
                    OnPropertyChanged(nameof(IsCustomChecked));
                }
            }
        }
        #endregion
        #endregion

        #region HeadsetAudioSettingsRightViewToolTip

        private string _noiseControlToolTip = "Controls the amount of external sound you hear";

        public string NoiseControlToolTip
        {
            get => _noiseControlToolTip;
        }

        private string _noiseCancellingInfoTip = "Eliminates surrounding noise";

        public string NoiseCancellingInfoTip
        {
            get => _noiseCancellingInfoTip;
        }

        private string _transparencyInfoTip = "Allows ambient sound to be heard. Adjusts the volume level of ambient sound heard.";

        public string TransparencyInfoTip
        {
            get => _transparencyInfoTip;
        }

        private string _noiseOffInfoTip = "Turns off Noise Cancellation features";

        public string NoiseOffInfoTip
        {
            get => _noiseOffInfoTip;
        }

        private string _outgoingAudioToolTip = "Limits your near-end mic noise to create a better audio experience for others";

        public string OutgoingAudioToolTip
        {
            get => _outgoingAudioToolTip;
        }

        private string _incomingAudioToolTip = "Limits far-end mic noise to create a better audio experience for you";

        public string IncomingAudioToolTip
        {
            get => _incomingAudioToolTip;
        }

        private string _audioOutputPresetsToolTip = "Equalizer adjusts based on chosen preset";

        public string AudioOutputPresetsToolTip
        {
            get => _audioOutputPresetsToolTip;
        }

        private string _sidetoneToolTip = "Adjusts how much you can hear your own voice while speaking on a call. (Not available in Transparency mode)";

        public string SidetoneToolTip
        {
            get => _sidetoneToolTip;
        }

        private string _micNoiseCancellationToolTip = "Removes background noise to allow your voice to be heard clearly";

        public string MicNoiseCancellationToolTip
        {
            get => _micNoiseCancellationToolTip;
        }

        #endregion

        /// <summary>
        /// HeadsetAutomatedActions Page
        /// </summary>

        #region HeadsetAutomatedActions ToggleSwitch Binding
        //Wear Detection ToggleSwitch
        private string _isWearDetection_String = "ON";
        public string WearDetection_String
        {
            get => _isWearDetectionStatus ? "ON" : "OFF";
        }

        private bool _isWearDetectionStatus = false;
        public bool WearDetectionStatus
        {
            get
            {
                return _isWearDetectionStatus;
            }
            set
            {
                if (value)
                    _deviceManager.SetWearDetection((int)SetBitValue((uint)CurrentDeviceInfo!.WearDetection, 0, 1), CurrentDeviceInfo!.ID).Wait();
                else
                    _deviceManager.SetWearDetection((int)SetBitValue((uint)CurrentDeviceInfo!.WearDetection, 0, 0), CurrentDeviceInfo!.ID).Wait();
                _isWearDetectionStatus = value;
                OnPropertyChanged("WearDetection_String");
            }
        }

        //PauseMusic ToggleSwitch
        private string _isPauseMusic_String = "ON";
        public string PauseMusic_String
        {
            get => _isPauseMusicStatus ? "ON" : "OFF";
        }

        private bool _isPauseMusicStatus = false;
        public bool PauseMusicStatus
        {
            get
            {
                return _isPauseMusicStatus;
            }
            set
            {
                if (value)
                    _deviceManager.SetWearDetection((int)SetBitValue((uint)CurrentDeviceInfo!.WearDetection, 1, 1), CurrentDeviceInfo!.ID).Wait();
                else
                    _deviceManager.SetWearDetection((int)SetBitValue((uint)CurrentDeviceInfo!.WearDetection, 1, 0), CurrentDeviceInfo!.ID).Wait();
                _isPauseMusicStatus = value;
                OnPropertyChanged("PauseMusic_String");
            }
        }

        //Mute Microphone ToggleSwitch
        private string _isMuteMicrophone_String = "ON";
        public string MuteMicrophone_String
        {
            get => _isMuteMicrophoneStatus ? "ON" : "OFF";
        }

        private bool _isMuteMicrophoneStatus = false;
        public bool MuteMicrophoneStatus
        {
            get
            {
                return _isMuteMicrophoneStatus;
            }
            set
            {
                if (value)
                    _deviceManager.SetWearDetection((int)SetBitValue((uint)CurrentDeviceInfo!.WearDetection, 2, 1), CurrentDeviceInfo!.ID).Wait();
                else
                    _deviceManager.SetWearDetection((int)SetBitValue((uint)CurrentDeviceInfo!.WearDetection, 2, 0), CurrentDeviceInfo!.ID).Wait();
                _isMuteMicrophoneStatus = value;
                OnPropertyChanged("MuteMicrophone_String");
            }
        }

        //Quick Pause ToggleSwitch
        private string _isQuickPause_String = "ON";
        public string QuickPause_String
        {
            get => _isQuickPauseStatus ? "ON" : "OFF";
        }

        private bool _isQuickPauseStatus = false;
        public bool QuickPauseStatus
        {
            get
            {
                return _isQuickPauseStatus;
            }
            set
            {
                if (value)
                    _deviceManager.SetWearDetection((int)SetBitValue((uint)CurrentDeviceInfo!.WearDetection, 4, 1), CurrentDeviceInfo!.ID).Wait();
                else
                    _deviceManager.SetWearDetection((int)SetBitValue((uint)CurrentDeviceInfo!.WearDetection, 4, 0), CurrentDeviceInfo!.ID).Wait();
                _isQuickPauseStatus = value;
                OnPropertyChanged("QuickPause_String");
            }
        }

        //AnswerCalls ToggleSwitch
        private string _isAnswerCalls_String = "ON";
        public string AnswerCalls_String
        {
            get => _isAnswerCallsStatus ? "ON" : "OFF";
        }

        private bool _isAnswerCallsStatus = false;
        public bool AnswerCallsStatus
        {
            get
            {
                return _isAnswerCallsStatus;
            }
            set
            {
                _isAnswerCallsStatus = value;
                OnPropertyChanged("AnswerCalls_String");
            }
        }
        #endregion

        #region HeadsetAutomatedActions Grid Show/Hide
        private bool _wearDetectionPageShow = false;
        public bool WearDetectionPageShow
        {
            get => _wearDetectionPageShow;
            set
            {
                _wearDetectionPageShow = value;
                OnPropertyChanged(nameof(WearDetectionPageShow));
            }
        }

        private bool _automatedActionsWhenHeadsetIsRemovedPageShow = false;
        public bool AutomatedActionsWhenHeadsetIsRemovedPageShow
        {
            get => _automatedActionsWhenHeadsetIsRemovedPageShow;
            set
            {
                _automatedActionsWhenHeadsetIsRemovedPageShow = value;
                OnPropertyChanged(nameof(AutomatedActionsWhenHeadsetIsRemovedPageShow));
            }
        }

        private bool _automatedActionsSensitivityUpPageShow = false;
        public bool AutomatedActionsSensitivityUpPageShow
        {
            get => _automatedActionsSensitivityUpPageShow;
            set
            {
                _automatedActionsSensitivityUpPageShow = value;
                OnPropertyChanged(nameof(AutomatedActionsSensitivityUpPageShow));
            }
        }

        private bool _automatedActionsQuickPausePageShow = false;
        public bool AutomatedActionsQuickPausePageShow
        {
            get => _automatedActionsQuickPausePageShow;
            set
            {
                _automatedActionsQuickPausePageShow = value;
                OnPropertyChanged(nameof(AutomatedActionsQuickPausePageShow));
            }
        }

        private bool _automatedActionsSensitivityPageShow = false;
        public bool AutomatedActionsSensitivityPageShow
        {
            get => _automatedActionsSensitivityPageShow;
            set
            {
                _automatedActionsSensitivityPageShow = value;
                OnPropertyChanged(nameof(AutomatedActionsSensitivityPageShow));
            }
        }

        private bool _automatedActionsAnswerCallPageShow = false;
        public bool AutomatedActionsAnswerCallPageShow
        {
            get => _automatedActionsAnswerCallPageShow;
            set
            {
                _automatedActionsAnswerCallPageShow = value;
                OnPropertyChanged(nameof(AutomatedActionsAnswerCallPageShow));
            }
        }
        #endregion

        #region HeadsetAutomatedActionsRightView
        private bool _isLowChecked;
        private bool _isNormal2Checked;
        public bool IsNormal2Checked
        {
            get => _isNormal2Checked;
            set
            {
                if (_isNormal2Checked != value)
                {
                    _isNormal2Checked = value;
                    if (_isNormal2Checked)
                    {
                        IsLowChecked = false;
                    }
                    if (value)
                        _deviceManager.SetWearDetection((int)SetBitValue((uint)CurrentDeviceInfo!.WearDetection, 3, 1), CurrentDeviceInfo!.ID).Wait();
                    OnPropertyChanged(nameof(IsNormal2Checked));
                }
            }
        }

        public bool IsLowChecked
        {
            get => _isLowChecked;
            set
            {
                if (_isLowChecked != value)
                {
                    _isLowChecked = value;
                    if (_isLowChecked)
                    {
                        IsNormal2Checked = false;
                    }
                    if (value)
                        _deviceManager.SetWearDetection((int)SetBitValue((uint)CurrentDeviceInfo!.WearDetection, 3, 0), CurrentDeviceInfo!.ID).Wait();
                    OnPropertyChanged(nameof(IsLowChecked));
                }
            }
        }

        private bool _isNormalChecked;
        private bool _isSensitiveChecked;
        public bool IsNormalChecked
        {
            get => _isNormalChecked;
            set
            {
                if (_isNormalChecked != value)
                {
                    _isNormalChecked = value;
                    if (_isNormalChecked)
                    {
                        IsSensitiveChecked = false;
                    }
                    if (value)
                        _deviceManager.SetWearDetection((int)SetBitsValue((uint)CurrentDeviceInfo!.WearDetection, 5, 1), CurrentDeviceInfo!.ID).Wait();
                    OnPropertyChanged(nameof(IsNormalChecked));
                }
            }
        }

        public bool IsSensitiveChecked
        {
            get => _isSensitiveChecked;
            set
            {
                if (_isSensitiveChecked != value)
                {
                    _isSensitiveChecked = value;
                    if (_isSensitiveChecked)
                    {
                        IsNormalChecked = false;
                    }
                    if (value)
                        _deviceManager.SetWearDetection((int)SetBitsValue((uint)CurrentDeviceInfo!.WearDetection, 5, 2), CurrentDeviceInfo!.ID).Wait();
                    OnPropertyChanged(nameof(IsSensitiveChecked));
                }
            }
        }

        #endregion

        #region HeadsetAutomatedActionsToolTip

        private string _wearDetectionToolTip = "Automatic actions when you remove your headset";

        public string WearDetectionToolTip
        {
            get => _wearDetectionToolTip;
        }


        private string _pauseMusicToolTip = "Pauses music automatically when headset is removed. Music will resume automatically when headset is put on.";

        public string PauseMusicToolTip
        {
            get => _pauseMusicToolTip;
        }

        private string _muteMicrophoneToolTip = "Mutes microphone automatically when headset is removed";

        public string MuteMicrophoneToolTip
        {
            get => _muteMicrophoneToolTip;
        }

        private string _answerCallsToolTip = "Pull down boom mic to answer calls";

        public string AnswerCallsToolTip
        {
            get => _answerCallsToolTip;
        }

        private string _quickPauseToolTip = "Automatic actions when you move an ear cup off your ear";

        public string QuickPauseToolTip
        {
            get => _quickPauseToolTip;
        }

        #endregion

        /// <summary>
        /// HeadsetDeviceSettings Page
        /// </summary>

        #region HeadsetDeviceSettings ToggleSwitch Binding
        //BusyLight ToggleSwitch
        private string _isBusyLight_String = "ON";
        public string BusyLight_String
        {
            get => CurrentDeviceInfo!.BusyLight ? "ON" : "OFF";
            //get => _isBusyLightStatus ? "ON" : "OFF";
            //set
            //{
            //    _isBusyLightStatus = CurrentDeviceInfo!.BusyLight;
            //}

        }

        private bool _isBusyLightStatus = false;
        public bool BusyLightStatus
        {
            get
            {
                _isBusyLightStatus = CurrentDeviceInfo!.BusyLight;
                return _isBusyLightStatus;
            }
            set
            {
                _deviceManager.SetBusyLight(value, CurrentDeviceInfo!.ID).Wait();
                _isBusyLightStatus = value;
                OnPropertyChanged("BusyLight_String");
            }
        }
        #endregion

        #region HeadsetDeviceSettings Grid Show/Hide

        private bool _voiceGuidancePageShow = false;
        public bool VoiceGuidancePageShow
        {
            get => _voiceGuidancePageShow;
            set
            {
                _voiceGuidancePageShow = value;
                OnPropertyChanged(nameof(VoiceGuidancePageShow));
            }
        }

        private bool _deviceSettingsDownloadDellAudioPageShow = false;
        public bool DeviceSettingsDownloadDellAudioPageShow
        {
            get => _deviceSettingsDownloadDellAudioPageShow;
            set
            {
                _deviceSettingsDownloadDellAudioPageShow = value;
                OnPropertyChanged(nameof(DeviceSettingsDownloadDellAudioPageShow));
            }
        }

        #endregion

        #region HeadsetDeviceSettingsRightView
        private bool _isEssentialChecked;
        private bool _isAllChecked;
        public bool IsEssentialChecked
        {
            get
            {
                return _isEssentialChecked;
            }
            set
            {
                if (_isEssentialChecked != value)
                {
                    _isEssentialChecked = value;
                    if (_isEssentialChecked)
                    {
                        IsAllChecked = false;
                        _deviceManager.SetVoiceGuidance(false, CurrentDeviceInfo!.ID).Wait();
                    }
                    OnPropertyChanged(nameof(IsEssentialChecked));
                }
            }
        }

        public bool IsAllChecked
        {
            get
            {
                return _isAllChecked;
            }
            set
            {
                if (_isAllChecked != value)
                {
                    _isAllChecked = value;
                    if (_isAllChecked)
                    {
                        IsEssentialChecked = false;
                        _deviceManager.SetVoiceGuidance(true, CurrentDeviceInfo!.ID).Wait();
                    }
                    OnPropertyChanged(nameof(IsAllChecked));
                }
            }
        }
        #endregion

        #region HeadsetDeviceSettingsToolTip

        private string _busyLightToolTip = "Indicator light when on a call";

        public string BusyLightToolTip
        {
            get => _busyLightToolTip;
        }

        private string _voiceGuidanceToolTip = "Audio prompts and announcements for device features";

        public string VoiceGuidanceToolTip
        {
            get => _voiceGuidanceToolTip;
        }

        #endregion
    }
}
