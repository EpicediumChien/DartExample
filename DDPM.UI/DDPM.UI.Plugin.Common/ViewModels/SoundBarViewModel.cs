using DDPM.SA.Common;
using DDPM.UI.Common;
using DDPM.UI.Common.Method;
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

        public readonly ILog _log;
        public IDeviceManagerSA _deviceManager;
        public SpeakerInfoValue SpeakerInfoValueDTP;
        public string _current_soundBar;
        public string _default = "{CFA20B04-897A-4E5F-A0C3-D95FD4594F85}";
        public string _speech = "{78EE7B67-5946-4A11-959D-299CC77466D1}";
        public string _bassBoost = "{44F5D888-F551-4D2C-B41C-EFD6EBD09D23}";
        public string _trebleBoost = "{CEB39A69-EF4A-4E73-BC8F-8A8B66CE11E1}";
        private Debouncer _debouncerSpeaker;
        #endregion Variables

        public new event PropertyChangedEventHandler? PropertyChanged;

        public string modelTest;

        public SoundBarViewModel(IConsole console, ILog log, IDeviceManagerSA deviceManager) : base(console, log, deviceManager)
        {
            Requires.NotNull(console, nameof(console));
            Requires.NotNull(log, nameof(log));

            _log = log;
            _deviceManager = deviceManager;
            _log!.Info($"[SoundBarViewModel] SoundBarViewModel Start...");
            SpeakerInfoValueDTP = new SpeakerInfoValue();
            _current_soundBar = string.Empty;
            _debouncerSpeaker = new Debouncer(1000, ExecuteDebouncedAction);
        }

        private void ExecuteDebouncedAction(object param)
        {
            if (param is string mode)
            {
                switch (mode)
                {
                    case "DefaultCheck":
                        _log.Info($"[SoundBarViewModel] SetProfileForSpeaker ... DefaultCheck ... {_default.ToString()}");
                        _deviceManager.SetProfileForSpeaker(CurrentDeviceInfo!.ID.ToString(), _default).Wait();
                        break;
                    case "SpeechCheck":
                        _log.Info($"[SoundBarViewModel] SetProfileForSpeaker ... SpeechCheck ... {_speech.ToString()}");
                        _deviceManager.SetProfileForSpeaker(CurrentDeviceInfo!.ID.ToString(), _speech).Wait();
                        break;
                    case "BassBoostCheck":
                        _log.Info($"[SoundBarViewModel] SetProfileForSpeaker ... BassBoostCheck ... {_bassBoost.ToString()}");
                        _deviceManager.SetProfileForSpeaker(CurrentDeviceInfo!.ID.ToString(), _bassBoost).Wait();
                        break;
                    case "TrebleBoostCheck":
                        _log.Info($"[SoundBarViewModel] SetProfileForSpeaker ... TrebleBoostCheck ... {_trebleBoost.ToString()}");
                        _deviceManager.SetProfileForSpeaker(CurrentDeviceInfo!.ID.ToString(), _trebleBoost).Wait();
                        break;
                    //------------------------------------------------------------------------------------------
                    case "IntelligentMicNoiseCancellationCheck":
                        _log.Info($"[SoundBarViewModel] SetIsWiredAudioIMicNSEnableAsync ... IntelligentMicNoiseCancellationCheck ... {SpeakerInfoValueDTP.IsWiredAudioIMicNSEnable.ToString()}");
                        _deviceManager.SetIsWiredAudioIMicNSEnableAsync(CurrentDeviceInfo!.ID.ToString(), SpeakerInfoValueDTP.IsWiredAudioIMicNSEnable).Wait();
                        break;
                    case "MuteSoundNotificationCheck":
                        _log.Info($"[SoundBarViewModel] SetIsWiredAudioMicMuteSoundEnableAsync ... MuteSoundNotificationCheck .... {SpeakerInfoValueDTP.IsWiredAudioMicMuteSoundEnable.ToString()}");
                        _deviceManager.SetIsWiredAudioMicMuteSoundEnableAsync(CurrentDeviceInfo!.ID.ToString(), SpeakerInfoValueDTP.IsWiredAudioMicMuteSoundEnable).Wait();
                        break;
                    case "VolumeAdjustmentToneCheck":
                        _log.Info($"[SoundBarViewModel] SetWiredAudioVolumeAdjustmentToneAsync ... VolumeAdjustmentToneCheck .... {SpeakerInfoValueDTP.WiredAudioVolumeAdjustmentTone.ToString()}");
                        _deviceManager.SetWiredAudioVolumeAdjustmentToneAsync(CurrentDeviceInfo!.ID.ToString(), SpeakerInfoValueDTP.WiredAudioVolumeAdjustmentTone).Wait();
                        break;
                    //------------------------------------------------------------------------------------------
                    default:
                        break;
                }
            }
        }
        private async Task UpdateDTPValue()
        {
            try
            {
                _log.Info($"[SoundBarViewModel] Print get property ...UpdateDTPValue ... in");
                if (SpeakerInfoValueDTP == null)
                {
                    SpeakerInfoValueDTP = new SpeakerInfoValue();
                    _log.Info($"[SoundBarViewModel] Print before property ...UpdateDTPValue new DeviceInfo...");
                }

                SpeakerInfoValueDTP.SpeakerProfile = (await _deviceManager.GetProfileAsync(CurrentDeviceID.ToString())) ?? String.Empty;
                SpeakerInfoValueDTP.SpeakerBass = await _deviceManager.GetBassAsync(CurrentDeviceID.ToString());
                SpeakerInfoValueDTP.SpeakerMidRange = await _deviceManager.GetMidRangeAsync(CurrentDeviceID.ToString());
                SpeakerInfoValueDTP.SpeakerTreble = await _deviceManager.GetTrebleAsync(CurrentDeviceID.ToString());
                SpeakerInfoValueDTP.IsWiredAudioMicMuteSoundEnable = await _deviceManager.GetIsWiredAudioMicMuteSoundEnableAsync(CurrentDeviceID.ToString());
                SpeakerInfoValueDTP.WiredAudioVolumeAdjustmentTone = await _deviceManager.GetWiredAudioVolumeAdjustmentToneAsync(CurrentDeviceID.ToString());
                SpeakerInfoValueDTP.IsWiredAudioIMicNSEnable = await _deviceManager.GetIsWiredAudioIMicNSEnableAsync(CurrentDeviceID.ToString());
                SpeakerInfoValueDTP.IsAudioEqualizerSupported = await _deviceManager.GetIsAudioEqualizerSupportedAsync(CurrentDeviceID.ToString());
                SpeakerInfoValueDTP.MuteStatus = await _deviceManager.GetMuteStatusAsyncForSpeaker(CurrentDeviceID.ToString());
                ChangeImage(Model, "MuteStatusChanged");
                _log.Info($"[SoundBarViewModel] ***********************************************************************");
                _log.Info($"[SoundBarViewModel] SpeakerInfoValueDTP.SpeakerProfile .............= {SpeakerInfoValueDTP.SpeakerProfile.ToString()}");
                _log.Info($"[SoundBarViewModel] SpeakerInfoValueDTP.SpeakerBass .............= {SpeakerInfoValueDTP.SpeakerBass.ToString()}");
                _log.Info($"[SoundBarViewModel] SpeakerInfoValueDTP.SpeakerMidRange ........= {SpeakerInfoValueDTP.SpeakerMidRange.ToString()}");
                _log.Info($"[SoundBarViewModel] SpeakerInfoValueDTP.SpeakerTreble ...........= {SpeakerInfoValueDTP.SpeakerTreble.ToString()}");
                _log.Info($"[SoundBarViewModel] SpeakerInfoValueDTP.IsWiredAudioMicMuteSoundEnable = {SpeakerInfoValueDTP.IsWiredAudioMicMuteSoundEnable.ToString()}");
                _log.Info($"[SoundBarViewModel] SpeakerInfoValueDTP.WiredAudioVolumeAdjustmentTone .......= {SpeakerInfoValueDTP.WiredAudioVolumeAdjustmentTone.ToString()}");
                _log.Info($"[SoundBarViewModel] SpeakerInfoValueDTP.IsWiredAudioIMicNSEnable ............= {SpeakerInfoValueDTP.IsWiredAudioIMicNSEnable.ToString()}");
                _log.Info($"[SoundBarViewModel] SpeakerInfoValueDTP.IsAudioEqualizerSupported .......= {SpeakerInfoValueDTP.IsAudioEqualizerSupported.ToString()}");
                _log.Info($"[SoundBarViewModel] SpeakerInfoValueDTP.GetMuteStatusAsync .......= {SpeakerInfoValueDTP.MuteStatus.ToString()}");
            }
            catch (Exception ex)
            {
                _log!.Error($"[SoundBarViewModel] UpdateDTPValue ...... {ex.ToString()}");
            }
        }

        private async Task DoWork_PleaseWait(string model, SoundBarViewModel vm)
        {
            _log.Info($"[SoundBarViewModel] DoWork_PleaseWait .......");
            // Simulate time-consuming operation
            Thread.Sleep(500);
            await UpdateDTPValue();
            // Call DetectPageShow
            await DetectPageShow(model);
        }

        public async Task Invoke_PleaseWaitAsync(string model, SoundBarViewModel vm)
        {
            //vm.ShowPleaseWait();
            try
            {
                using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10)))
                {
                    await Task.Run(() => DoWork_PleaseWait(model, vm), cts.Token);
                }
            }
            catch (OperationCanceledException)
            {
                _log!.Error("[SoundBarViewModel] Invoke_PleaseWaitAsync timed out");
                throw;
            }
            catch (Exception ex)
            {
                vm._log!.Error($"[SoundBarViewModel] Invoke_PleaseWaitAsync exception: {ex.Message}");
                throw;
            }
            finally
            {
                //vm.HidePleaseWait();
            }
        }

        public void CheckPresetsUI()
        {
            if (SpeakerInfoValueDTP.IsAudioEqualizerSupported)
            {
                _log!.Info($"[SoundBarViewModel] IsAudioEqualizerSupported CheckPresetsUI ......");
                switch (SpeakerInfoValueDTP.SpeakerProfile)
                {
                    case "{CFA20B04-897A-4E5F-A0C3-D95FD4594F85}"://_default
                        _isDefaultChecked = true;
                        _isSpeechChecked = false;
                        _isBassBoostChecked = false;
                        _isTrebleBoostChecked = false;
                        break;

                    case "{78EE7B67-5946-4A11-959D-299CC77466D1}"://_speech
                        _isDefaultChecked = false;
                        _isSpeechChecked = true;
                        _isBassBoostChecked = false;
                        _isTrebleBoostChecked = false;
                        break;

                    case "{44F5D888-F551-4D2C-B41C-EFD6EBD09D23}"://_bassBoost
                        _isDefaultChecked = false;
                        _isSpeechChecked = false;
                        _isBassBoostChecked = true;
                        _isTrebleBoostChecked = false;
                        break;

                    case "{CEB39A69-EF4A-4E73-BC8F-8A8B66CE11E1}"://_trebleBoost
                        _isDefaultChecked = false;
                        _isSpeechChecked = false;
                        _isBassBoostChecked = false;
                        _isTrebleBoostChecked = true;
                        break;
                    default:
                        break;
                }
                OnPropertyChanged("IsDefaultChecked");
                OnPropertyChanged("IsSpeechChecked");
                OnPropertyChanged("IsBassBoostChecked");
                OnPropertyChanged("IsTrebleBoostChecked");
            }
        }
        public async Task DetectPageShow(string model)
        {
            _log!.Info($"[SoundBarViewModel] DetectPageShow ......");
            modelTest = model;
            switch (model.ToUpper())
            {
                case "SB522A":
                    _controlGoogleMeetButtonShow = false;
                    _controlSkypeforBusinessButtonShow = false;
                    break;
                case "SP3022":
                    _controlGoogleMeetButtonShow = true;
                    _controlSkypeforBusinessButtonShow = false;
                    break;

                default:
                    break;
            }
        }

        public void CheckAudioSettingsUI()
        {
            _log!.Info($"[SoundBarViewModel] CheckAudioSettingsUI ......");
            _isIntelligentMicNoiseCancellationStatus = SpeakerInfoValueDTP.IsWiredAudioIMicNSEnable;//CurrentDeviceInfo!.IsWiredAudioIMicNSEnable;
            _isMuteSoundNotificationStatus = SpeakerInfoValueDTP.IsWiredAudioMicMuteSoundEnable;//CurrentDeviceInfo!.IsWiredAudioMicMuteSoundEnable;
            _isVolumeAdjustmentToneMode = SpeakerInfoValueDTP.WiredAudioVolumeAdjustmentTone;//CurrentDeviceInfo!.WiredAudioVolumeAdjustmentTone;

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
            OnPropertyChanged("IntelligentMicNoiseCancellation_String");
            OnPropertyChanged("MuteSoundNotificationStatus");
            OnPropertyChanged("MuteSoundNotification_String");
            OnPropertyChanged("VolumeAdjustmentToneStatus");
            OnPropertyChanged("VolumeAdjustmentTone_String");
            OnPropertyChanged("IsEveryLevelChecked");
            OnPropertyChanged("IsMinMaxOnlyChecked");
        }

        public void ChangeImage(string model, string btnName)
        {
            _log!.Info($"[SoundBarViewModel] ChangeImage ...... {model} / {btnName}");
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

                    case "MuteStatusChanged":
                        if (SpeakerInfoValueDTP.MuteStatus)
                        {
                            ImageFilePath = "/DDPM.UI.Common;component/Resources/Speaker_SP3022_RedLight.png";
                        }
                        else
                        {
                            ImageFilePath = "/DDPM.UI.Common;component/Resources/Speaker_SP3022.png";
                        }
                        break;
                    default:
                        if (SpeakerInfoValueDTP.MuteStatus)
                        {
                            ImageFilePath = "/DDPM.UI.Common;component/Resources/Speaker_SP3022_RedLight.png";
                        }
                        else
                        {
                            ImageFilePath = "/DDPM.UI.Common;component/Resources/Speaker_SP3022.png";
                        }
                        break;
                }
            }
            else if(model == "SB522A")
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
            else
            {
                switch (btnName)
                {
                    case "MicrosoftTeams":
                        ImageFilePath = "/DDPM.UI.Common;component/Resources/Speaker_SB725.png";
                        break;

                    case "Zoom":
                        ImageFilePath = "/DDPM.UI.Common;component/Resources/Speaker_SB725.png";
                        break;
                    case "MuteStatusChanged":
                        ImageFilePath = $"/DDPM.UI.Common;component/Resources/Speaker_SB725.png";
                        break;
                    default:
                        ImageFilePath = $"/DDPM.UI.Common;component/Resources/Speaker_SB725.png";
                        break;
                }
            }
        }

        public void ChangeImageMouseLeave(string model)
        {
            _log!.Info($"[SoundBarViewModel] ChangeImageMouseLeave ...... {model}");

            switch (model)
            {
                case "SP3022":
                    if (SpeakerInfoValueDTP.MuteStatus)
                    {
                        ImageFilePath = "/DDPM.UI.Common;component/Resources/Speaker_SP3022_RedLight.png";
                    }
                    else
                    {
                        ImageFilePath = "/DDPM.UI.Common;component/Resources/Speaker_SP3022.png";
                    }
                    break;

                case "SB522A":
                    ImageFilePath = "/DDPM.UI.Common;component/Resources/Speaker_SB522A.png";
                    break;
                case "SB725":
                    ImageFilePath = $"/DDPM.UI.Common;component/Resources/Speaker_SB725.png";
                    break;
                default:
                    if (model == "SB725")
                    {
                        ImageFilePath = $"/DDPM.UI.Common;component/Resources/Speaker_SB725.png";
                    }
                    else
                        ImageFilePath = "/DDPM.UI.Common;component/Resources/Speaker_SP3022.png";
                    break;
            }
        }

        public override void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void PrepareDeviceInfo(List<DeviceInfo> deviceInfos)
        {
            _log!.Info($"[SoundBarViewModel] PrepareDeviceInfo ...... ");
            DeviceInfos.Clear();
            foreach (DeviceInfo deviceInfo in deviceInfos)
            {
                if (deviceInfo.LogicalDeviceType.Contains("LogicalWiredAudio"))
                    DeviceInfos.Add(deviceInfo.ID, deviceInfo);
            }
        }

        public override bool SetCurrentDevice(string deviceID)
        {
            _log.Info($"[SoundBarViewModel] SetCurrentDevice ...");
            deviceID ??= DeviceInfos.Values.ToList().FirstOrDefault()!.ID.ToString();

            if (!base.SetCurrentDevice(deviceID))
                return false;

            string fv = _deviceManager.GetProfileAsync(CurrentDeviceID.ToString()).Result;
            if (fv == null || fv == string.Empty)
                IsDTPReady = false;
            else
                IsDTPReady = true;

            return true;
        }

        public override void HandleNotification(DeviceChangedType changeType, DeviceInfo di, string property = "")
        {
            _log.Info($"[SoundBarViewModel] HandleNotification ... Receive {property.ToString()}");
            base.HandleNotification(changeType, di, property);
            switch (property)
            {
                case "MuteStatusChanged":
                    SpeakerInfoValueDTP.MuteStatus = _deviceManager.GetMuteStatusAsyncForSpeaker(CurrentDeviceID.ToString()).Result;
                    ChangeImage(Model, "MuteStatusChanged");
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
                        //GenerateInfo();
                    }
                    break;

                default:
                    break;
            }
        }
        public async void RestoreToDefault()
        {
            try
            {
                _log.Info($"[SoundBarViewModel] Print before property ...RestoreToDefault ... in");
                _log.Info($"[SoundBarViewModel] SpeakerInfoValueDTP.SpeakerProfile .............= {SpeakerInfoValueDTP.SpeakerProfile.ToString()}");
                _log.Info($"[SoundBarViewModel] SpeakerInfoValueDTP.SpeakerBass .............= {SpeakerInfoValueDTP.SpeakerBass.ToString()}");
                _log.Info($"[SoundBarViewModel] SpeakerInfoValueDTP.SpeakerMidRange ........= {SpeakerInfoValueDTP.SpeakerMidRange.ToString()}");
                _log.Info($"[SoundBarViewModel] SpeakerInfoValueDTP.SpeakerTreble ...........= {SpeakerInfoValueDTP.SpeakerTreble.ToString()}");
                _log.Info($"[SoundBarViewModel] SpeakerInfoValueDTP.IsWiredAudioMicMuteSoundEnable = {SpeakerInfoValueDTP.IsWiredAudioMicMuteSoundEnable.ToString()}");
                _log.Info($"[SoundBarViewModel] SpeakerInfoValueDTP.WiredAudioVolumeAdjustmentTone .......= {SpeakerInfoValueDTP.WiredAudioVolumeAdjustmentTone.ToString()}");
                _log.Info($"[SoundBarViewModel] SpeakerInfoValueDTP.IsWiredAudioIMicNSEnable ............= {SpeakerInfoValueDTP.IsWiredAudioIMicNSEnable.ToString()}");
                _log.Info($"[SoundBarViewModel] SpeakerInfoValueDTP.IsAudioEqualizerSupported .......= {SpeakerInfoValueDTP.IsAudioEqualizerSupported.ToString()}");
                _deviceManager.SetResetToDefaultAsyncForSoundbar(CurrentDeviceInfo!.ID.ToString(), true).Wait();
                await UpdateDTPValue();
                CheckHeadsetFunc();
            }
            catch (Exception ex)
            {
                _log!.Error($"[SoundBarViewModel] RestoreToDefault ...... {ex.ToString()}");
            }

        }

        public void CheckHeadsetFunc()
        {
            _log.Info($"[SoundBarViewModel] CheckHeadsetFunc ...");
            CheckAudioSettingsUI();
            CheckPresetsUI();
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
                if (_isDefaultChecked == value && value == true)
                {
                    return;
                }

                if (_isDefaultChecked != value && value)
                {
                    _isDefaultChecked = value;

                    if (_isDefaultChecked)
                    {
                        // 確保其他按鈕取消選中
                        _isSpeechChecked = false;
                        _isBassBoostChecked = false;
                        _isTrebleBoostChecked = false;
                        SpeakerInfoValueDTP.SpeakerProfile = _default;
                        _debouncerSpeaker.Debounce("DefaultCheck");
                        // 只通知其他按鈕已變更狀態
                        OnPropertyChanged(nameof(IsDefaultChecked));
                        OnPropertyChanged(nameof(IsSpeechChecked));
                        OnPropertyChanged(nameof(IsBassBoostChecked));
                        OnPropertyChanged(nameof(IsTrebleBoostChecked));
                    }
                }
            }
        }

        private bool _isSpeechChecked;

        public bool IsSpeechChecked
        {
            get { return _isSpeechChecked; }
            set
            {
                if (_isSpeechChecked == value && value == true)
                {
                    return;
                }

                if (_isSpeechChecked != value && value)
                {
                    _isSpeechChecked = value;

                    if (_isSpeechChecked)
                    {
                        _isDefaultChecked = false;
                        _isBassBoostChecked = false;
                        _isTrebleBoostChecked = false;
                        SpeakerInfoValueDTP.SpeakerProfile = _speech;
                        _debouncerSpeaker.Debounce("SpeechCheck");
                        OnPropertyChanged(nameof(IsDefaultChecked));
                        OnPropertyChanged(nameof(IsBassBoostChecked));
                        OnPropertyChanged(nameof(IsTrebleBoostChecked));
                    }
                }
            }
        }

        private bool _isBassBoostChecked;

        public bool IsBassBoostChecked
        {
            get { return _isBassBoostChecked; }
            set
            {
                if (_isBassBoostChecked == value && value == true)
                {
                    return;
                }

                if (_isBassBoostChecked != value && value)
                {
                    _isBassBoostChecked = value;

                    if (_isBassBoostChecked)
                    {
                        _isDefaultChecked = false;
                        _isSpeechChecked = false;
                        _isTrebleBoostChecked = false;
                        SpeakerInfoValueDTP.SpeakerProfile = _bassBoost;
                        _debouncerSpeaker.Debounce("BassBoostCheck");
                        OnPropertyChanged(nameof(IsDefaultChecked));
                        OnPropertyChanged(nameof(IsSpeechChecked));
                        OnPropertyChanged(nameof(IsTrebleBoostChecked));
                    }
                }
            }
        }

        private bool _isTrebleBoostChecked;

        public bool IsTrebleBoostChecked
        {
            get { return _isTrebleBoostChecked; }
            set
            {
                if (_isTrebleBoostChecked == value && value == true)
                {
                    return;
                }

                if (_isTrebleBoostChecked != value && value)
                {
                    _isTrebleBoostChecked = value;

                    if (_isTrebleBoostChecked)
                    {
                        _isDefaultChecked = false;
                        _isSpeechChecked = false;
                        _isBassBoostChecked = false;
                        SpeakerInfoValueDTP.SpeakerProfile = _trebleBoost;
                        _debouncerSpeaker.Debounce("TrebleBoostCheck");
                        OnPropertyChanged(nameof(IsDefaultChecked));
                        OnPropertyChanged(nameof(IsSpeechChecked));
                        OnPropertyChanged(nameof(IsBassBoostChecked));
                    }

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
                _isIntelligentMicNoiseCancellationStatus = SpeakerInfoValueDTP.IsWiredAudioIMicNSEnable;// _deviceManager.GetIsWiredAudioIMicNSEnableAsync(CurrentDeviceInfo!.ID.ToString()).Result;//CurrentDeviceInfo!.IsWiredAudioIMicNSEnable;
                return _isIntelligentMicNoiseCancellationStatus;
            }
            set
            {
                if (_isIntelligentMicNoiseCancellationStatus != value)
                {
                    _isIntelligentMicNoiseCancellationStatus = value;
                    SpeakerInfoValueDTP.IsWiredAudioIMicNSEnable = value;
                    _debouncerSpeaker.Debounce("IntelligentMicNoiseCancellationCheck");
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
                _isMuteSoundNotificationStatus = SpeakerInfoValueDTP.IsWiredAudioMicMuteSoundEnable;// _deviceManager.GetIsWiredAudioMicMuteSoundEnableAsync(CurrentDeviceInfo!.ID.ToString()).Result;
                return _isMuteSoundNotificationStatus;
            }
            set
            {
                if (_isMuteSoundNotificationStatus != value)
                {
                    _isMuteSoundNotificationStatus = value;
                    SpeakerInfoValueDTP.IsWiredAudioMicMuteSoundEnable = value;
                    _debouncerSpeaker.Debounce("MuteSoundNotificationCheck");
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
                    SpeakerInfoValueDTP.WiredAudioVolumeAdjustmentTone = 1;
                    _debouncerSpeaker.Debounce("VolumeAdjustmentToneCheck");
                    OnPropertyChanged("VolumeAdjustmentToneStatus");
                    OnPropertyChanged("VolumeAdjustmentTone_String");
                    OnPropertyChanged("IsEveryLevelChecked");
                    OnPropertyChanged("IsMinMaxOnlyChecked");
                }
                if (!value)
                {
                    _isVolumeAdjustmentToneMode = 3;
                    _volumeAdjustmentToneStatus = false;
                    _isEveryLevelChecked = false;
                    _isMinMaxOnlyChecked = false;
                    SpeakerInfoValueDTP.WiredAudioVolumeAdjustmentTone = 3;
                    _debouncerSpeaker.Debounce("VolumeAdjustmentToneCheck");
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
                    SpeakerInfoValueDTP.WiredAudioVolumeAdjustmentTone = 1;
                    _debouncerSpeaker.Debounce("VolumeAdjustmentToneCheck");
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
                    SpeakerInfoValueDTP.WiredAudioVolumeAdjustmentTone = 2;
                    _debouncerSpeaker.Debounce("VolumeAdjustmentToneCheck");
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
        public void Invoke_PleaseWait(string model, HeadsetViewModel vm)
        {
            BackgroundWorker bw = new BackgroundWorker
            {
                WorkerReportsProgress = false,
                WorkerSupportsCancellation = false
            };
            bw.DoWork += (sender, e) => DoWork_PleaseWait(model, vm);
            bw.RunWorkerCompleted += RunWorkerCompleted_PleaseWait;

            ShowPleaseWait();
            bw.RunWorkerAsync();
        }

        private void DoWork_PleaseWait(string model, HeadsetViewModel vm)
        {
            _log.Info($"[SoundBarViewModel] DoWork_PleaseWait .......");
            // Simulate time-consuming operation
            Thread.Sleep(500);
            int sun = 0;
            UpdateDTPValue();
            // Call DetectPageShow
            DetectPageShow(model);
        }

        private void RunWorkerCompleted_PleaseWait(object sender, RunWorkerCompletedEventArgs e)
        {
            HidePleaseWait();
        }

        #endregion Please Wait

        public class SpeakerInfoValue
        {
            public string SpeakerProfile { get; set; }
            public int SpeakerBass { get; set; }
            public int SpeakerMidRange { get; set; }
            public int SpeakerTreble { get; set; }
            public bool IsWiredAudioMicMuteSoundEnable { get; set; }
            public int WiredAudioVolumeAdjustmentTone { get; set; }
            public bool IsWiredAudioIMicNSEnable { get; set; }
            public bool IsAudioEqualizerSupported { get; set; }
            public bool MuteStatus { get; set; }
        }
    }
}