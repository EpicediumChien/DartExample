using DDPM.SA.Common;
using DDPM.UI.Common;
using DDPM.UI.Common.Method;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.Security;
using Dell.Client.Framework.UX.WPF;
using Microsoft;
using Newtonsoft.Json.Linq;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace DDPM.UI.Plugin.ViewModels
{
    public class SoundBarViewModel : PeripheralViewModel, INotifyPropertyChanged
    {
        #region Variables

        public readonly ILog _log;
        public IDeviceManagerSA _deviceManager;
        public IShowPluginManager _showPluginManager;
        public SpeakerInfoValue SpeakerInfoValueDTP;
        public string _current_soundBar;
        public string _default = "{CFA20B04-897A-4E5F-A0C3-D95FD4594F85}";
        public string _speech = "{78EE7B67-5946-4A11-959D-299CC77466D1}";
        public string _bassBoost = "{44F5D888-F551-4D2C-B41C-EFD6EBD09D23}";
        public string _trebleBoost = "{CEB39A69-EF4A-4E73-BC8F-8A8B66CE11E1}";
        private Debouncer _debouncerSpeaker;
        public event EventHandler<EventArgs> SoundbarSettingChanged;
        #endregion Variables

        public new event PropertyChangedEventHandler? PropertyChanged;

        public string modelTest;

        public SoundBarViewModel(IShowPluginManager showPluginManager, IConsole console, ILog log, IDeviceManagerSA deviceManager) : base(console, log, deviceManager)
        {
            Requires.NotNull(console, nameof(console));
            Requires.NotNull(log, nameof(log));

            _log = log;
            _deviceManager = deviceManager;
            _showPluginManager = showPluginManager;
            _log!.Info($"[SoundBarViewModel] SoundBarViewModel Start...");
            SpeakerInfoValueDTP = new SpeakerInfoValue();
            _current_soundBar = string.Empty;
            _debouncerSpeaker = new Debouncer(1000, ExecuteDebouncedAction);
        }

        private void ExecuteDebouncedAction(object param)
        {
            _isRestoreEnable = false;
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
                        //_log.Info($"[SoundBarViewModel] SetIsWiredAudioIMicNSEnableAsync ... IntelligentMicNoiseCancellationCheck ... {SpeakerInfoValueDTP.IsWiredAudioIMicNSEnable.ToString()}");
                        //_deviceManager.SetIsWiredAudioIMicNSEnableAsync(CurrentDeviceInfo!.ID.ToString(), SpeakerInfoValueDTP.IsWiredAudioIMicNSEnable).Wait();
                        break;
                    case "MuteSoundNotificationCheck":
                        //_log.Info($"[SoundBarViewModel] SetIsWiredAudioMicMuteSoundEnableAsync ... MuteSoundNotificationCheck .... {SpeakerInfoValueDTP.IsWiredAudioMicMuteSoundEnable.ToString()}");
                        //_deviceManager.SetIsWiredAudioMicMuteSoundEnableAsync(CurrentDeviceInfo!.ID.ToString(), SpeakerInfoValueDTP.IsWiredAudioMicMuteSoundEnable).Wait();
                        break;
                    case "VolumeAdjustmentToneCheck":
                        if (IsDTPReady)
                        {
                            _log.Info($"[SoundBarViewModel] SetWiredAudioVolumeAdjustmentToneAsync ... VolumeAdjustmentToneCheck .... {SpeakerInfoValueDTP.WiredAudioVolumeAdjustmentTone.ToString()}");
                            _deviceManager.SetWiredAudioVolumeAdjustmentToneAsync(CurrentDeviceInfo!.ID.ToString(), SpeakerInfoValueDTP.WiredAudioVolumeAdjustmentTone).Wait();
                        }
                        else
                        {
                            _log.Info($"[SoundBarViewModel] DTH SetWiredAudioVolumeAdjustmentToneAsync ... VolumeAdjustmentToneCheck .... {SpeakerInfoValueDTP.WiredAudioVolumeAdjustmentTone.ToString()}");
                            _deviceManager.SetWiredAudioVolumeAdjustmentTone(1, CurrentDeviceInfo!.ID).Wait();
                        }
                        break;
                    //------------------------------------------------------------------------------------------
                    default:
                        break;
                }
            }
        }
        private void UpdateDTPValue()
        {
            try
            {
                if (IsDTPReady)
                {
                    _log.Info($"[SoundBarViewModel] DTP Print get property ...UpdateDTPValue ... in");
                    if (SpeakerInfoValueDTP == null)
                    {
                        SpeakerInfoValueDTP = new SpeakerInfoValue();
                        _log.Info($"[SoundBarViewModel] DTP Print before property ...UpdateDTPValue new DeviceInfo...");
                    }

                    SpeakerInfoValueDTP.SpeakerProfileName = _deviceManager.GetProfileNameAsync(CurrentDeviceID.ToString()).Result ?? String.Empty;
                    if(SpeakerInfoValueDTP.SpeakerProfile == _default)
                    {
                        SpeakerInfoValueDTP.SpeakerBass = _deviceManager.GetBassAsync(CurrentDeviceID.ToString()).Result;
                        SpeakerInfoValueDTP.SpeakerMidRange = _deviceManager.GetMidRangeAsync(CurrentDeviceID.ToString()).Result;
                        SpeakerInfoValueDTP.SpeakerTreble = _deviceManager.GetTrebleAsync(CurrentDeviceID.ToString()).Result;
                    }
                    else
                    {
                        SpeakerInfoValueDTP.SpeakerBass = 0;
                        SpeakerInfoValueDTP.SpeakerMidRange = 0;
                        SpeakerInfoValueDTP.SpeakerTreble = 0;
                    }
                    SpeakerInfoValueDTP.SpeakerProfile = _deviceManager.GetProfileAsync(CurrentDeviceID.ToString()).Result ?? String.Empty;
                    SpeakerInfoValueDTP.IsWiredAudioMicMuteSoundEnable = _deviceManager.GetIsWiredAudioMicMuteSoundEnableAsync(CurrentDeviceID.ToString()).Result;
                    SpeakerInfoValueDTP.WiredAudioVolumeAdjustmentTone = _deviceManager.GetWiredAudioVolumeAdjustmentToneAsync(CurrentDeviceID.ToString()).Result;
                    SpeakerInfoValueDTP.IsWiredAudioIMicNSEnable = _deviceManager.GetIsWiredAudioIMicNSEnableAsync(CurrentDeviceID.ToString()).Result;
                    SpeakerInfoValueDTP.IsAudioEqualizerSupported = _deviceManager.GetIsAudioEqualizerSupportedAsync(CurrentDeviceID.ToString()).Result;
                    SpeakerInfoValueDTP.MuteStatus = _deviceManager.GetMuteStatusAsyncForSpeaker(CurrentDeviceID.ToString()).Result;
                    SpeakerInfoValueDTP.IsIMicNSSupportedAsync = _deviceManager.GetIsIMicNSSupportedAsync(CurrentDeviceID.ToString()).Result;
                    SpeakerInfoValueDTP.IsVolumeAdjustmentToneSupportedAsync = _deviceManager.GetIsVolumeAdjustmentToneSupportedAsync(CurrentDeviceID.ToString()).Result;
                    SpeakerInfoValueDTP.IsMicMuteSoundSupportedAsync = _deviceManager.GetIsMicMuteSoundSupportedAsync(CurrentDeviceID.ToString()).Result;
                    SpeakerInfoValueDTP.PresetProfilesAsync = _deviceManager.GetPresetProfilesAsync(CurrentDeviceID.ToString()).Result;
                    SpeakerInfoValueDTP.IsBassEqualizerSupportedAsync = _deviceManager.GetIsBassEqualizerSupportedAsync(CurrentDeviceID.ToString()).Result;
                    SpeakerInfoValueDTP.IsMidRangeEqualizerSupportedAsync = _deviceManager.GetIsMidRangeEqualizerSupportedAsync(CurrentDeviceID.ToString()).Result;
                    SpeakerInfoValueDTP.IsTrebleEqualizerSupportedAsync = _deviceManager.GetIsTrebleEqualizerSupportedAsync(CurrentDeviceID.ToString()).Result;

                    ChangeImage(Model, "MuteStatusChanged");
                }
                else
                {
                    _log.Info($"[SoundBarViewModel] DTH Print get property ...UpdateDTPValue ... in");
                    if (SpeakerInfoValueDTP == null)
                    {
                        SpeakerInfoValueDTP = new SpeakerInfoValue();
                        _log.Info($"[SoundBarViewModel] DTH Print before property ...UpdateDTPValue new DeviceInfo...");
                    }

                    SpeakerInfoValueDTP.SpeakerProfile = CurrentDeviceInfo!.Profile;
                    SpeakerInfoValueDTP.SpeakerBass = 0;
                    SpeakerInfoValueDTP.SpeakerMidRange = 0;
                    SpeakerInfoValueDTP.SpeakerTreble = 0;
                    SpeakerInfoValueDTP.IsWiredAudioMicMuteSoundEnable = CurrentDeviceInfo!.IsWiredAudioMicMuteSoundEnable;
                    SpeakerInfoValueDTP.WiredAudioVolumeAdjustmentTone = CurrentDeviceInfo!.WiredAudioVolumeAdjustmentTone;
                    SpeakerInfoValueDTP.IsWiredAudioIMicNSEnable = CurrentDeviceInfo!.IsWiredAudioIMicNSEnable;
                    SpeakerInfoValueDTP.IsAudioEqualizerSupported = CurrentDeviceInfo!.IsEqualizerSupported;
                    SpeakerInfoValueDTP.MuteStatus = CurrentDeviceInfo!.MuteStatus;
                    SpeakerInfoValueDTP.SpeakerProfileName = CurrentDeviceInfo!.ProfileName;
                    SpeakerInfoValueDTP.IsIMicNSSupportedAsync = false;
                    SpeakerInfoValueDTP.IsVolumeAdjustmentToneSupportedAsync = false;
                    SpeakerInfoValueDTP.IsMicMuteSoundSupportedAsync = false;
                    SpeakerInfoValueDTP.PresetProfilesAsync = false;
                    SpeakerInfoValueDTP.IsBassEqualizerSupportedAsync = false;
                    SpeakerInfoValueDTP.IsMidRangeEqualizerSupportedAsync = false;
                    SpeakerInfoValueDTP.IsTrebleEqualizerSupportedAsync = false;
                    //ChangeImage(Model, "MuteStatusChanged");
                }
                _log.Info($"[SoundBarViewModel] ***********************************************************************");
                _log.Info($"[SoundBarViewModel] SpeakerInfoValueDTP.SpeakerProfileName .............= {SpeakerInfoValueDTP.SpeakerProfileName.ToString()}");
                _log.Info($"[SoundBarViewModel] SpeakerInfoValueDTP.SpeakerProfile .............= {SpeakerInfoValueDTP.SpeakerProfile.ToString()}");
                _log.Info($"[SoundBarViewModel] SpeakerInfoValueDTP.SpeakerBass .............= {SpeakerInfoValueDTP.SpeakerBass.ToString()}");
                _log.Info($"[SoundBarViewModel] SpeakerInfoValueDTP.SpeakerMidRange ........= {SpeakerInfoValueDTP.SpeakerMidRange.ToString()}");
                _log.Info($"[SoundBarViewModel] SpeakerInfoValueDTP.SpeakerTreble ...........= {SpeakerInfoValueDTP.SpeakerTreble.ToString()}");
                _log.Info($"[SoundBarViewModel] SpeakerInfoValueDTP.IsWiredAudioMicMuteSoundEnable = {SpeakerInfoValueDTP.IsWiredAudioMicMuteSoundEnable.ToString()}");
                _log.Info($"[SoundBarViewModel] SpeakerInfoValueDTP.WiredAudioVolumeAdjustmentTone .......= {SpeakerInfoValueDTP.WiredAudioVolumeAdjustmentTone.ToString()}");
                _log.Info($"[SoundBarViewModel] SpeakerInfoValueDTP.IsWiredAudioIMicNSEnable ............= {SpeakerInfoValueDTP.IsWiredAudioIMicNSEnable.ToString()}");
                _log.Info($"[SoundBarViewModel] SpeakerInfoValueDTP.IsAudioEqualizerSupported .......= {SpeakerInfoValueDTP.IsAudioEqualizerSupported.ToString()}");
                _log.Info($"[SoundBarViewModel] SpeakerInfoValueDTP.IsIMicNSSupportedAsync .............= {SpeakerInfoValueDTP.IsIMicNSSupportedAsync.ToString()}");
                _log.Info($"[SoundBarViewModel] SpeakerInfoValueDTP.IsVolumeAdjustmentToneSupportedAsync ........= {SpeakerInfoValueDTP.IsVolumeAdjustmentToneSupportedAsync.ToString()}");
                _log.Info($"[SoundBarViewModel] SpeakerInfoValueDTP.IsMicMuteSoundSupportedAsync ...........= {SpeakerInfoValueDTP.IsMicMuteSoundSupportedAsync.ToString()}");
                _log.Info($"[SoundBarViewModel] SpeakerInfoValueDTP.PresetProfilesAsync = {SpeakerInfoValueDTP.PresetProfilesAsync.ToString()}");
                _log.Info($"[SoundBarViewModel] SpeakerInfoValueDTP.IsBassEqualizerSupportedAsync .......= {SpeakerInfoValueDTP.IsBassEqualizerSupportedAsync.ToString()}");
                _log.Info($"[SoundBarViewModel] SpeakerInfoValueDTP.IsMidRangeEqualizerSupportedAsync ............= {SpeakerInfoValueDTP.IsMidRangeEqualizerSupportedAsync.ToString()}");
                _log.Info($"[SoundBarViewModel] SpeakerInfoValueDTP.IsTrebleEqualizerSupportedAsync .......= {SpeakerInfoValueDTP.IsTrebleEqualizerSupportedAsync.ToString()}");
                _log.Info($"[SoundBarViewModel] SpeakerInfoValueDTP.GetMuteStatusAsync .......= {SpeakerInfoValueDTP.MuteStatus.ToString()}");

                UpdateResetToDefault();
                CheckSpeakerFunc();
                Application.Current.Dispatcher.Invoke(() =>
                {
                    SoundbarSettingChanged?.Invoke(this, EventArgs.Empty);
                });

            }
            catch (Exception ex)
            {
                _log!.Error($"[SoundBarViewModel] UpdateDTPValue ...... {ex.ToString()}");
            }
        }

        private void UpdateResetToDefault()
        {
            if (CheckIfCurrentSettingsMatchDefault(SpeakerInfoValueDTP, Model))
                _isRestoreEnable = true;
            else
                _isRestoreEnable = false;
        }

        private async Task DoWork_PleaseWait(string model, SoundBarViewModel vm)
        {
            _log.Info($"[SoundBarViewModel] DoWork_PleaseWait .......");
            // Simulate time-consuming operation
            Thread.Sleep(500);
            if (!model.Contains("SB725"))
            {
                UpdateDTPValue();
                // Call DetectPageShow
                DetectPageShow(model);
            }
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
            _isIntelligentMicNoiseCancellationStatus = SpeakerInfoValueDTP.IsWiredAudioIMicNSEnable;
            _isMuteSoundNotificationStatus = SpeakerInfoValueDTP.IsWiredAudioMicMuteSoundEnable;
            _isVolumeAdjustmentToneMode = SpeakerInfoValueDTP.WiredAudioVolumeAdjustmentTone;

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

        public override bool SetCurrentDevice(string instanceIDs)
        {
            _log.Info($"[SoundBarViewModel] SetCurrentDevice ...");
            instanceIDs ??= DeviceInfos.Values.ToList().FirstOrDefault()!.ID.ToString();

            if (!base.SetCurrentDevice(instanceIDs))
                return false;

            CurrentDeviceID = new Guid(instanceIDs);

            if (DeviceInfos[CurrentDeviceID].ModelNumber.Contains("SB725"))
            {
                IsDTPReady = true;
                _log.Info($"[SoundBarViewModel] SetCurrentDevice ... ModelNumber ... SB725");
            }
            else
            {
                string fv = _deviceManager.GetProfileAsync(CurrentDeviceID.ToString()).Result;
                if (fv == null || fv == string.Empty)
                {
                    IsDTPReady = false;
                    _log.Info($"[SoundBarViewModel] SetCurrentDevice ... GetProfileAsync ... Null or Empty ... DTP fail ...");
                }
                else
                {
                    IsDTPReady = true;
                    _log.Info($"[SoundBarViewModel] SetCurrentDevice ... GetProfileAsync ... DTP success ...");
                }
            }
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
                _log.Info($"[SpeakerViewModel] RestoreToDefault model = {Model} ... in ");
                if (IsDTPReady)
                {
                    _log.Info($"[SoundBarViewModel] DTP Print before property ...RestoreToDefault ... in");
                    _log.Info($"[SoundBarViewModel] DTP SpeakerInfoValueDTP.SpeakerProfileName .............= {SpeakerInfoValueDTP.SpeakerProfileName.ToString()}");
                    _log.Info($"[SoundBarViewModel] DTP SpeakerInfoValueDTP.SpeakerProfile .............= {SpeakerInfoValueDTP.SpeakerProfile.ToString()}");
                    _log.Info($"[SoundBarViewModel] DTP SpeakerInfoValueDTP.SpeakerBass .............= {SpeakerInfoValueDTP.SpeakerBass.ToString()}");
                    _log.Info($"[SoundBarViewModel] DTP SpeakerInfoValueDTP.SpeakerMidRange ........= {SpeakerInfoValueDTP.SpeakerMidRange.ToString()}");
                    _log.Info($"[SoundBarViewModel] DTP SpeakerInfoValueDTP.SpeakerTreble ...........= {SpeakerInfoValueDTP.SpeakerTreble.ToString()}");
                    _log.Info($"[SoundBarViewModel] DTP SpeakerInfoValueDTP.IsWiredAudioMicMuteSoundEnable = {SpeakerInfoValueDTP.IsWiredAudioMicMuteSoundEnable.ToString()}");
                    _log.Info($"[SoundBarViewModel] DTP SpeakerInfoValueDTP.WiredAudioVolumeAdjustmentTone .......= {SpeakerInfoValueDTP.WiredAudioVolumeAdjustmentTone.ToString()}");
                    _log.Info($"[SoundBarViewModel] DTP SpeakerInfoValueDTP.IsWiredAudioIMicNSEnable ............= {SpeakerInfoValueDTP.IsWiredAudioIMicNSEnable.ToString()}");
                    _log.Info($"[SoundBarViewModel] DTP SpeakerInfoValueDTP.IsAudioEqualizerSupported .......= {SpeakerInfoValueDTP.IsAudioEqualizerSupported.ToString()}");
                    bool SetFactoryResult = _deviceManager.SetResetToDefaultAsyncForSoundbar(CurrentDeviceInfo!.ID.ToString(), true).Result;
                    _log.Info($"[SoundBarViewModel] DTP SetResetToDefaultAsyncForSoundbar = {SetFactoryResult.ToString()},  for model = {Model} ...");
                }
                else
                {
                    _log.Info($"[SoundBarViewModel] DTH Print before property ...RestoreToDefault ... in");
                    _log.Info($"[SoundBarViewModel] DTH SpeakerInfoValueDTP.SpeakerProfileName .............= {CurrentDeviceInfo!.ProfileName.ToString()}");
                    _log.Info($"[SoundBarViewModel] DTH SpeakerInfoValueDTP.SpeakerProfile .............= {CurrentDeviceInfo!.Profile.ToString()}");
                    _log.Info($"[SoundBarViewModel] DTH SpeakerInfoValueDTP.SpeakerBass .............= DTH no support");
                    _log.Info($"[SoundBarViewModel] DTH SpeakerInfoValueDTP.SpeakerMidRange ........= DTH no support");
                    _log.Info($"[SoundBarViewModel] DTH SpeakerInfoValueDTP.SpeakerTreble ...........= DTH no support");
                    _log.Info($"[SoundBarViewModel] DTH SpeakerInfoValueDTP.IsWiredAudioMicMuteSoundEnable = {CurrentDeviceInfo!.IsWiredAudioMicMuteSoundEnable.ToString()}");
                    _log.Info($"[SoundBarViewModel] DTH SpeakerInfoValueDTP.WiredAudioVolumeAdjustmentTone .......= {CurrentDeviceInfo!.WiredAudioVolumeAdjustmentTone.ToString()}");
                    _log.Info($"[SoundBarViewModel] DTH SpeakerInfoValueDTP.IsWiredAudioIMicNSEnable ............= {CurrentDeviceInfo!.IsWiredAudioIMicNSEnable.ToString()}");
                    _log.Info($"[SoundBarViewModel] DTH SpeakerInfoValueDTP.IsAudioEqualizerSupported .......= {CurrentDeviceInfo!.IsEqualizerSupported.ToString()}");
                    if (!ModelDefaultSettings.ContainsKey(Model))
                    {
                        _log.Warning($"[SoundBarViewModel] No default settings found for model: {Model}");
                        return;
                    }
                    var defaultSettings = ModelDefaultSettings[Model];

                    SpeakerInfoValueDTP.SpeakerProfileName = defaultSettings.SpeakerProfileName;
                    SpeakerInfoValueDTP.SpeakerProfile = defaultSettings.SpeakerProfile;
                    SpeakerInfoValueDTP.SpeakerBass = defaultSettings.SpeakerBass;
                    SpeakerInfoValueDTP.SpeakerMidRange = defaultSettings.SpeakerMidRange;
                    SpeakerInfoValueDTP.SpeakerTreble = defaultSettings.SpeakerTreble;
                    SpeakerInfoValueDTP.IsWiredAudioMicMuteSoundEnable = defaultSettings.IsWiredAudioMicMuteSoundEnable;
                    SpeakerInfoValueDTP.WiredAudioVolumeAdjustmentTone = defaultSettings.WiredAudioVolumeAdjustmentTone;
                    SpeakerInfoValueDTP.IsWiredAudioIMicNSEnable = defaultSettings.IsWiredAudioIMicNSEnable;
                    _deviceManager.SetWiredAudioVolumeAdjustmentTone(SpeakerInfoValueDTP.WiredAudioVolumeAdjustmentTone, CurrentDeviceInfo!.ID).Wait();
                    _deviceManager.SetWiredAudioMicMuteSoundEnable(SpeakerInfoValueDTP.IsWiredAudioMicMuteSoundEnable, CurrentDeviceInfo!.ID).Wait();
                    _deviceManager.SetWiredAudioIMicNSEnable(SpeakerInfoValueDTP.IsWiredAudioIMicNSEnable, CurrentDeviceInfo!.ID).Wait();
                    _log.Info($"[SoundBarViewModel] DTH SetResetToDefaultAsyncForSoundbar for model = {Model} ...");
                }
                UpdateDTPValue();
                CheckSpeakerFunc();
                SoundbarSettingChanged?.Invoke(this, EventArgs.Empty);
                //_showPluginManager?.ShowHomePage();
            }
            catch (Exception ex)
            {
                _log!.Error($"[SoundBarViewModel] RestoreToDefault ...... {ex.ToString()}");
            }

        }

        public void CheckSpeakerFunc()
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

        public bool _isRestoreEnable = false;

        public bool IsRestoreEnable
        {
            get
            {
                return _isRestoreEnable;
            }
            set
            {
                _isRestoreEnable = value;
                OnPropertyChanged(nameof(IsRestoreEnable));
            }
        }

        #region SpeakerAudioPreset

        private bool _isDefaultChecked = true;

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
                        _isRestoreEnable = false;
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

        private bool _isSpeechChecked = false;

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
                        _isRestoreEnable = false;
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

        private bool _isBassBoostChecked = false;

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
                        _isRestoreEnable = false;
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

        private bool _isTrebleBoostChecked = false;

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
                        _isRestoreEnable = false;
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

        private bool _supportedIntelligentMicNoiseCancellationToggleSwitch = true;

        public bool SupportedIntelligentMicNoiseCancellationToggleSwitch
        {
            get
            {
                //_isIntelligentMicNoiseCancellationStatus = SpeakerInfoValueDTP.IsWiredAudioIMicNSEnable;// _deviceManager.GetIsWiredAudioIMicNSEnableAsync(CurrentDeviceInfo!.ID.ToString()).Result;//CurrentDeviceInfo!.IsWiredAudioIMicNSEnable;
                return _supportedIntelligentMicNoiseCancellationToggleSwitch;
            }
            set
            {
                _supportedIntelligentMicNoiseCancellationToggleSwitch = value;
                OnPropertyChanged("SupportedIntelligentMicNoiseCancellationToggleSwitch");
            }
        }

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
                    _isRestoreEnable = false;
                    _isIntelligentMicNoiseCancellationStatus = value;
                    SpeakerInfoValueDTP.IsWiredAudioIMicNSEnable = value;
                    _supportedIntelligentMicNoiseCancellationToggleSwitch = false;
                    if (IsDTPReady)
                        _deviceManager.SetIsWiredAudioIMicNSEnableAsync(CurrentDeviceInfo!.ID.ToString(), SpeakerInfoValueDTP.IsWiredAudioIMicNSEnable);
                    else
                        _deviceManager.SetWiredAudioIMicNSEnable(value, CurrentDeviceInfo!.ID).Wait();
                    _supportedIntelligentMicNoiseCancellationToggleSwitch = true;
                    //_debouncerSpeaker.Debounce("IntelligentMicNoiseCancellationCheck");
                    OnPropertyChanged("IntelligentMicNoiseCancellation_String");
                    _log.Info($"[SoundBarViewModel] SetIsWiredAudioIMicNSEnableAsync ... IntelligentMicNoiseCancellationCheck ... {SpeakerInfoValueDTP.IsWiredAudioIMicNSEnable.ToString()}");
                }
            }
        }

        private string _isIntelligentMicNoiseCancellation_String = Strings.On;

        public string IntelligentMicNoiseCancellation_String
        {
            get => _isIntelligentMicNoiseCancellationStatus ? Strings.On : Strings.Off;
        }

        
        private bool _supportedMuteSoundNotificationToggleSwitch = true;
        public bool SupportedMuteSoundNotificationToggleSwitch
        {
            get
            {
                return _supportedMuteSoundNotificationToggleSwitch;
            }
            set
            {
                _supportedMuteSoundNotificationToggleSwitch = value;
                OnPropertyChanged("SupportedMuteSoundNotificationToggleSwitch");
            }
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
                    _isRestoreEnable = false;
                    _isMuteSoundNotificationStatus = value;
                    SpeakerInfoValueDTP.IsWiredAudioMicMuteSoundEnable = value;
                    _supportedMuteSoundNotificationToggleSwitch = false;
                    if (IsDTPReady)
                        _deviceManager.SetIsWiredAudioMicMuteSoundEnableAsync(CurrentDeviceInfo!.ID.ToString(), SpeakerInfoValueDTP.IsWiredAudioMicMuteSoundEnable).Wait();
                    else
                        _deviceManager.SetWiredAudioMicMuteSoundEnable(value, CurrentDeviceInfo!.ID).Wait();
                    _supportedMuteSoundNotificationToggleSwitch = true;
                    //_debouncerSpeaker.Debounce("MuteSoundNotificationCheck");
                    OnPropertyChanged("MuteSoundNotification_String");
                    _log.Info($"[SoundBarViewModel] SetIsWiredAudioMicMuteSoundEnableAsync ... MuteSoundNotificationCheck .... {SpeakerInfoValueDTP.IsWiredAudioMicMuteSoundEnable.ToString()}");
                }
            }
        }

        private string _isMuteSoundNotification_String = Strings.On;

        public string MuteSoundNotification_String
        {
            get => _isMuteSoundNotificationStatus ? Strings.On : Strings.Off;
        }


        private bool _supportedolumeAdjustmentToneToggleSwitch = true;
        public bool SupportedolumeAdjustmentToneToggleSwitch
        {
            get
            {
                return _supportedolumeAdjustmentToneToggleSwitch;
            }
            set
            {
                _supportedolumeAdjustmentToneToggleSwitch = value;
                OnPropertyChanged("SupportedolumeAdjustmentToneToggleSwitch");
            }
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
                    _isRestoreEnable = false;
                    _volumeAdjustmentToneStatus = true;
                    _isEveryLevelChecked = true;
                    _isMinMaxOnlyChecked = false;
                    _isVolumeAdjustmentToneMode = 1;
                    SpeakerInfoValueDTP.WiredAudioVolumeAdjustmentTone = 1;
                    _supportedolumeAdjustmentToneToggleSwitch = false;
                    if (IsDTPReady)
                        _deviceManager.SetWiredAudioVolumeAdjustmentToneAsync(CurrentDeviceInfo!.ID.ToString(), SpeakerInfoValueDTP.WiredAudioVolumeAdjustmentTone).Wait();
                    else
                        _deviceManager.SetWiredAudioVolumeAdjustmentTone(1, CurrentDeviceInfo!.ID).Wait();
                    _supportedolumeAdjustmentToneToggleSwitch = true;
                    //_debouncerSpeaker.Debounce("VolumeAdjustmentToneCheck");
                    OnPropertyChanged("VolumeAdjustmentToneStatus");
                    OnPropertyChanged("VolumeAdjustmentTone_String");
                    OnPropertyChanged("IsEveryLevelChecked");
                    OnPropertyChanged("IsMinMaxOnlyChecked");
                    _log.Info($"[SoundBarViewModel] SetWiredAudioVolumeAdjustmentToneAsync ... VolumeAdjustmentToneCheck .... {SpeakerInfoValueDTP.WiredAudioVolumeAdjustmentTone.ToString()}");
                }
                if (!value)
                {
                    _isRestoreEnable = false;
                    _volumeAdjustmentToneStatus = false;
                    _isEveryLevelChecked = false;
                    _isMinMaxOnlyChecked = false;
                    _isVolumeAdjustmentToneMode = 3;
                    SpeakerInfoValueDTP.WiredAudioVolumeAdjustmentTone = 3;
                    _supportedolumeAdjustmentToneToggleSwitch = false;
                    if (IsDTPReady)
                        _deviceManager.SetWiredAudioVolumeAdjustmentToneAsync(CurrentDeviceInfo!.ID.ToString(), SpeakerInfoValueDTP.WiredAudioVolumeAdjustmentTone).Wait();
                    else
                        _deviceManager.SetWiredAudioVolumeAdjustmentTone(3, CurrentDeviceInfo!.ID).Wait();
                    _supportedolumeAdjustmentToneToggleSwitch = true;
                    //_debouncerSpeaker.Debounce("VolumeAdjustmentToneCheck");
                    OnPropertyChanged("VolumeAdjustmentToneStatus");
                    OnPropertyChanged("VolumeAdjustmentTone_String");
                    OnPropertyChanged("IsEveryLevelChecked");
                    OnPropertyChanged("IsMinMaxOnlyChecked");
                    _log.Info($"[SoundBarViewModel] SetWiredAudioVolumeAdjustmentToneAsync ... VolumeAdjustmentToneCheck .... {SpeakerInfoValueDTP.WiredAudioVolumeAdjustmentTone.ToString()}");
                }
            }
        }

        private string _volumeAdjustmentToneString = Strings.On;

        public string VolumeAdjustmentTone_String
        {
            get
            {
                return _volumeAdjustmentToneStatus ? Strings.On : Strings.Off;
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
                    _isRestoreEnable = false;
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
                    _isRestoreEnable = false;
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
            public string SpeakerProfileName { get; set; } = string.Empty;
            public string SpeakerProfile { get; set; } = string.Empty;
            public int SpeakerBass { get; set; } = 0;
            public int SpeakerMidRange { get; set; } = 0;
            public int SpeakerTreble { get; set; } = 0;
            public bool IsWiredAudioMicMuteSoundEnable { get; set; } = false;
            public int WiredAudioVolumeAdjustmentTone { get; set; } = 0;
            public bool IsWiredAudioIMicNSEnable { get; set; } = false;
            public bool IsAudioEqualizerSupported { get; set; } = false;
            public bool MuteStatus { get; set; } = false;
            public bool IsIMicNSSupportedAsync { get; set; } = false;
            public bool IsVolumeAdjustmentToneSupportedAsync { get; set; } = false;
            public bool IsMicMuteSoundSupportedAsync { get; set; } = false;
            public bool PresetProfilesAsync { get; set; } = false;
            public bool IsBassEqualizerSupportedAsync { get; set; } = false;
            public bool IsMidRangeEqualizerSupportedAsync { get; set; } = false;
            public bool IsTrebleEqualizerSupportedAsync { get; set; } = false;
        }

        private class SoundbarDeviceDefaultSettings
        {
            public string SpeakerProfileName { get; set; } = "Default";
            public string SpeakerProfile { get; set; } = "{CFA20B04-897A-4E5F-A0C3-D95FD4594F85}";
            public int SpeakerBass { get; set; } = 0;
            public int SpeakerMidRange { get; set; } = 0;
            public int SpeakerTreble { get; set; } = 0;
            public bool IsWiredAudioMicMuteSoundEnable { get; set; } = true;
            public int WiredAudioVolumeAdjustmentTone { get; set; } = 1;
            public bool IsWiredAudioIMicNSEnable { get; set; } = true;
        }

        private bool CheckIfCurrentSettingsMatchDefault(SpeakerInfoValue currentSettings, string model)
        {
            if (!ModelDefaultSettings.ContainsKey(model))
            {
                return true;
            }

            var defaultSettings = ModelDefaultSettings[model];
            if (currentSettings.SpeakerProfileName != defaultSettings.SpeakerProfileName) return false;
            if (currentSettings.SpeakerProfile != defaultSettings.SpeakerProfile) return false;
            if (currentSettings.SpeakerBass != defaultSettings.SpeakerBass) return false;
            if (currentSettings.SpeakerMidRange != defaultSettings.SpeakerMidRange) return false;
            if (currentSettings.SpeakerTreble != defaultSettings.SpeakerTreble) return false;
            if (currentSettings.IsWiredAudioMicMuteSoundEnable != defaultSettings.IsWiredAudioMicMuteSoundEnable) return false;
            if (currentSettings.WiredAudioVolumeAdjustmentTone != defaultSettings.WiredAudioVolumeAdjustmentTone) return false;
            if (currentSettings.IsWiredAudioIMicNSEnable != defaultSettings.IsWiredAudioIMicNSEnable) return false;

            return true;
        }
        private static readonly Dictionary<string, SoundbarDeviceDefaultSettings> ModelDefaultSettings =
        new Dictionary<string, SoundbarDeviceDefaultSettings>()
        {
            { "SP3022", new SoundbarDeviceDefaultSettings {
                SpeakerProfileName = "Default",
                SpeakerProfile = "{CFA20B04-897A-4E5F-A0C3-D95FD4594F85}",
                SpeakerBass = 0,
                SpeakerMidRange = 0,
                SpeakerTreble = 0,
                IsWiredAudioMicMuteSoundEnable = true,
                WiredAudioVolumeAdjustmentTone = 1,
                IsWiredAudioIMicNSEnable = true,

            }},
            { "SB522A", new SoundbarDeviceDefaultSettings {
                SpeakerProfileName = "Default",
                SpeakerProfile = "{CFA20B04-897A-4E5F-A0C3-D95FD4594F85}",
                SpeakerBass = 0,
                SpeakerMidRange = 0,
                SpeakerTreble = 0,
                IsWiredAudioMicMuteSoundEnable = false,
                WiredAudioVolumeAdjustmentTone = 0,
                IsWiredAudioIMicNSEnable = true,
            }},
        };

        //SP3022
        //SpeakerInfoValueDTP.SpeakerProfile.............= {CFA20B04-897A-4E5F-A0C3-D95FD4594F85
        //SpeakerInfoValueDTP.SpeakerBass.............= 0
        //SpeakerInfoValueDTP.SpeakerMidRange........= 0
        //SpeakerInfoValueDTP.SpeakerTreble...........= 0
        //SpeakerInfoValueDTP.IsWiredAudioMicMuteSoundEnable = True
        //SpeakerInfoValueDTP.WiredAudioVolumeAdjustmentTone.......= 1
        //SpeakerInfoValueDTP.IsWiredAudioIMicNSEnable............= True
        //
        //SpeakerInfoValueDTP.IsAudioEqualizerSupported.......= True
        //SpeakerInfoValueDTP.IsIMicNSSupportedAsync.............= True
        //SpeakerInfoValueDTP.IsVolumeAdjustmentToneSupportedAsync........= True
        //SpeakerInfoValueDTP.IsMicMuteSoundSupportedAsync...........= True
        //SpeakerInfoValueDTP.PresetProfilesAsync = False
        //SpeakerInfoValueDTP.IsBassEqualizerSupportedAsync.......= True
        //SpeakerInfoValueDTP.IsMidRangeEqualizerSupportedAsync............= True
        //SpeakerInfoValueDTP.IsTrebleEqualizerSupportedAsync.......= True
        //SpeakerInfoValueDTP.GetMuteStatusAsync.......= False

        //ChangeImage......SB522A / MuteStatusChanged
        //***********************************************************************
        //SpeakerInfoValueDTP.SpeakerProfileName.............= Default
        //SpeakerInfoValueDTP.SpeakerProfile.............= {CFA20B04-897A-4E5F-A0C3-D95FD4594F85      
        //SpeakerInfoValueDTP.SpeakerBass.............= 0
        //SpeakerInfoValueDTP.SpeakerMidRange........= 0
        //SpeakerInfoValueDTP.SpeakerTreble...........= 0
        //SpeakerInfoValueDTP.IsWiredAudioMicMuteSoundEnable = False
        //SpeakerInfoValueDTP.WiredAudioVolumeAdjustmentTone.......= 0
        //SpeakerInfoValueDTP.IsWiredAudioIMicNSEnable............= True
        //
        //SpeakerInfoValueDTP.IsAudioEqualizerSupported.......= True
        //SpeakerInfoValueDTP.IsIMicNSSupportedAsync.............= True
        //SpeakerInfoValueDTP.IsVolumeAdjustmentToneSupportedAsync........= True
        //SpeakerInfoValueDTP.IsMicMuteSoundSupportedAsync...........= True
        //SpeakerInfoValueDTP.PresetProfilesAsync = False
        //SpeakerInfoValueDTP.IsBassEqualizerSupportedAsync.......= True
        //SpeakerInfoValueDTP.IsMidRangeEqualizerSupportedAsync............= True
        //SpeakerInfoValueDTP.IsTrebleEqualizerSupportedAsync.......= True
        //SpeakerInfoValueDTP.GetMuteStatusAsync.......= False
}
}